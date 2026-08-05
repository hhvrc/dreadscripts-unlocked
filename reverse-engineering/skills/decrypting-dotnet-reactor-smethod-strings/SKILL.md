---
name: decrypting-dotnet-reactor-smethod-strings
description: Resolves .NET Reactor's runtime-encrypted strings (the generic smethod_N string-decryption calls that de4dot leaves untouched) using scripts/dotnet_reactor.py — detecting the per-method A/B transform constants, scanning a runtime or offline string dump, and filling literals back into decompiled source. Use whenever export/ or ported source still contains unresolved smethod_N calls.
---

# Decrypting .NET Reactor `smethod_N` Strings

## When to use

- `export/<Assembly>/` still has `smethod_N<string>(KEY)` or `smethod_N(KEY)` calls after the
  standard de4dot + ilspycmd pipeline (see the `triaging-dotnet-reactor-obfuscation` skill).
- You're porting or updating a file in `../public/unity/` and hit a smethod call that needs its literal
  string value.
- A new smethod index (`smethod_N`) appears that hasn't been auto-detected yet.

## Background: the transform

.NET Reactor's generic string decrypter has this shape per assembly:

```
transformed = (key * A) ^ B          // int32 overflow arithmetic
type_tag    = (uint)transformed >> 30  // 0 = string, otherwise a non-string constant
offset      = (transformed & 0x3FFFFFFF) << 2
string      = utf8_at(byte_0, offset)  // 4-byte length prefix, then UTF-8 payload
```

`A` and `B` are distinct **per smethod_N**, embedded as IL constants inside that method. `byte_0` is
the module's encrypted data blob. The guard (`GetCallingAssembly() == GetExecutingAssembly()`) only
blocks *external* callers — it does not change the arithmetic, which is why this is fully solvable
offline once A/B and the blob are known.

## Toolkit

`scripts/dotnet_reactor.py` (run from `dreadful-re/`) has four subcommands:

```bash
# 1. Auto-extract A/B constants for every smethod_N from the full assembly dump
python scripts/dotnet_reactor.py detect "export/<Name>/-Module-.cs"

# 2. Scan a runtime/offline dump + a source dir; prints every resolved (smethod_N, key) -> string
python scripts/dotnet_reactor.py scan dumps/intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/

# 3. In-place replace smethod_N(key) calls with string literals (always dry-run first)
python scripts/dotnet_reactor.py fill dumps/intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/ --dry-run
python scripts/dotnet_reactor.py fill dumps/intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/

# 4. Just dump every (offset, string) pair from a log, no source involved
python scripts/dotnet_reactor.py dump dumps/intercepted.txt
```

`detect` reads the constants straight from decompiled IL/C# — no execution needed. `scan`/`fill`
need a **data source** for the actual decrypted string values: either
- `dumps/intercepted.txt` — a `[STRING] byte_0+OFFSET: "string"` runtime log (historical Harmony
  hook output; no longer regeneratable since the runtime tooling was removed — treat as a fixed
  corpus), or
- `dumps/<AssemblyName>.txt` — a `smethod_N(key) = "string"` offline IL-scan dump (also historical,
  same caveat).

Both dump formats already exist for ADOverhaul2022 (740 strings) and ControllerEditor (1318
strings) — check `dumps/` before assuming you need a new extraction method for those two assemblies.

You can override or supply constants explicitly instead of relying on `detect`:

```bash
python scripts/dotnet_reactor.py scan dumps/intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/ \
    --transforms '{"smethod_1":[1553271299,-1677909072]}'
# or --config transforms.json for a larger set
```

## Workflow

1. Run `detect` against the freshest `-Module-.cs` for the target assembly to get every smethod's
   `[A, B]` pair (or confirm the ones already recorded in `RE_NOTES.md` still match — obfuscator
   constants are per-build, so a re-obfuscated DLL version can shift them).
2. Confirm you have a dump (`dumps/intercepted.txt` or `dumps/<Assembly>.txt`) covering the keys
   actually called in the target source.
3. Run `scan` first (read-only) to sanity-check the resolved strings look correct (readable text,
   plausible URLs/DSL keys/etc.) before mutating anything.
4. Run `fill --dry-run` against the specific ported subtree you're editing, review the
   diff it would make, then run it for real.
5. Update `RE_NOTES.md`'s "Decrypted Strings" section with any newly-resolved constants worth
   documenting (especially ones filled via explicit `--transforms` overrides, since those aren't
   otherwise recoverable from `detect`).

## Common scenarios

**Scenario: a new assembly (e.g. a 2019 build) has different A/B constants than the 2022 build.**
.NET Reactor regenerates these constants per obfuscation pass, so don't reuse ADOverhaul2022's
recorded `[A, B]` pairs for ADOverhaul2019 — re-run `detect` against that assembly's own
`-Module-.cs`. Record both sets separately in `RE_NOTES.md`; don't overwrite one with the other.

**Scenario: `scan` resolves a key but the string looks like garbage.**
This almost always means the smethod index and the dump entry are mismatched (wrong `A`/`B` pair
picked up, or the dump was captured against a different DLL build than the one being decompiled
now). Re-run `detect` and diff constants before assuming the transform itself is wrong — the
arithmetic is fixed and has already been validated against thousands of resolved strings.

**Scenario: `fill` would touch a file with existing hand-written prose/comments around the call.**
Always `--dry-run` first and read the diff. `fill` does literal source rewriting via regex
(`replace_match`) — it's precise about the call site but has no awareness of surrounding comments
or formatting choices you've already made in the ported source.

## Pitfalls

- Never treat `dumps/*.txt` as regeneratable — the Harmony/offline dumper tooling that produced them
  (`Hooks.cs`, `SmethodStringDumper.cs`) has been deleted from the project. If a key is missing from
  every existing dump, it needs a new extraction approach, not a re-run of removed tooling.
- Don't hand-copy a decrypted value between the two DLL variants (e.g. ADOverhaul2019 vs. 2022) —
  same string *content* can exist at a different `byte_0` offset per build.
- `int32_mul`/the transform use signed int32 overflow semantics — if reimplementing this logic
  anywhere else (e.g. inside the de4dot fork), use wraparound arithmetic, not Python's arbitrary
  precision ints directly.
