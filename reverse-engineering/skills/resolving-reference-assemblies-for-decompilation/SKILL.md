---
name: resolving-reference-assemblies-for-decompilation
description: Where to find a complete, matching reference-assembly set for decompiling or ilverify-checking the obfuscated Unity plugin DLLs — the checked-in deps/unity/ portable subset, and the full local Unity Editor install for ilverify's stricter requirements. Use whenever a decompile shows unresolved/garbled external types, or before running ilverify against any deobfuscated output.
---

# Resolving Reference Assemblies for Decompilation & Verification

## When to use

- ilspycmd (or another decompiler) renders a type/member from `UnityEngine`/`UnityEditor`/`mscorlib`/
  `Mono.*` as an opaque or fallback signature instead of a proper resolved type — a sign the
  decompiler can't find the referenced assembly.
- Running `ilverify` against a deobfuscated DLL (see de4dot's
  `measuring-deobfuscation-correctness-with-ilverify` skill for the methodology this feeds) — this
  needs a genuinely complete, version-matched reference set, or error counts are silently wrong.
- Investigating a de4dot bug using one of this project's real obfuscated samples as a test case (see
  `tracking-open-questions-with-theory-folders`) and you need to actually verify IL correctness, not
  just read a decompile.

## Two reference sets, two different jobs

### `deps/unity/` (checked into this repo) — lightweight, portable

A ~95-file subset of Unity/Mono assemblies (`UnityEngine*.dll`, `UnityEditor*.dll`, `mscorlib.dll`,
`Mono.Security.dll`, `netstandard.dll`, `System*.dll`, `0Harmony.dll`) small enough to check into git
and use on any machine without a Unity install. Good enough for **decompiling** the DLLs in
`binaries/` cleanly — it covers the actual Unity API surface these plugins call into. It is **not**
a complete framework reference set (it's missing large swaths of the BCL and any third-party
dependency like the VRChat SDK — see "Known gaps" below), so don't rely on it for `ilverify`.

### `deps/vrchat/` (checked into this repo) — the VRChat SDK references

The 11 VRChat SDK assemblies (`VRCSDK3A.dll`, `VRC.Dynamics.dll`, `VRC.SDK3.Dynamics.PhysBone.dll`,
`VRCSDKBase.dll`, …). **These are checked in.** Together with `deps/unity/` they form a complete
reference set for both decompiling and `ilverify`.

### Use `deps/` for ilverify too — do NOT use a machine-local Unity install

`ilverify`'s correctness methodology requires **every** reference resolved, or it silently *skips*
methods it can't fully resolve — under-counting errors rather than flagging them. This is the single
easiest way to get a false "looks fine" result:

```bash
ilverify <deobfuscated.dll> \
  -r "deps/unity/*.dll" \
  -r "deps/vrchat/*.dll" \
  -s mscorlib \
  -r binaries/0Harmony.dll
```

(`-s mscorlib` tells ilverify which loaded ref is the system/corlib assembly for the target runtime.)

`scripts/de4dot_scorecard.py` defaults to exactly these directories and will refuse to run if either
is missing, so prefer it over hand-rolled invocations.

> **Corrected 2026-07-29 — this skill previously said the opposite, and it was wrong.** It used to
> direct `ilverify` at a machine-local Unity Editor install under the user's home directory. That is
> both non-reproducible *and*, in practice, an **incomplete** set: it lacked the VRChat SDK, so
> ~447 `FileLoadErrorGeneric` lines were being written off as "expected noise" while ilverify quietly
> skipped the methods behind them. Re-measured with `deps/unity` + `deps/vrchat` the same binary went
> from an apparent 8 real errors to an actual **17**. Never reference a path outside this repo for
> reference assemblies.

`ilspycmd`, `ilverify`, and `dotnet-ilverify` are installed as global dotnet tools in this environment
but are **not on the default `PATH`** — add `~/.dotnet/tools` to `PATH` for the session, or invoke by
full path (`~/.dotnet/tools/ilverify`, `~/.dotnet/tools/ilspycmd`).

## `FileLoadErrorGeneric` is NOT noise — it means your numbers are wrong

A previous version of this skill told you to write off `Failed to load assembly 'VRCSDK3A'` (and
`netstandard`) as unavoidable third-party noise. **That was incorrect and actively harmful**: those
assemblies are checked in under `deps/vrchat/` and `deps/unity/`, and every unresolved assembly means
`ilverify` skipped methods, so the error counts printed alongside it are undercounts.

Treat any `FileLoadErrorGeneric` as a **defect in your reference set**, not a property of the sample:
find the missing assembly and add it to `deps/` before trusting a single number from that run. With
`deps/unity` + `deps/vrchat` the expected count of unresolved assemblies is **zero**.

`ilspycmd`, `ilverify`, and `dotnet-ilverify` are installed as global dotnet tools in this
environment but are **not on the default `PATH`** — add `~/.dotnet/tools` to `PATH` for the session,
or invoke by full path.

## Workflow

1. **For a plain decompile** (reading code, not verifying correctness): symlink or copy `deps/unity/`
   contents alongside the target DLL, or otherwise put it on the decompiler's assembly search path,
   before decompiling. This is usually enough to get clean, properly-typed output.
2. **For `ilverify`-based correctness checking**: use the full local Unity install as shown above,
   not `deps/unity/`. Cross-check the reference set is actually complete by confirming error counts
   are dominated by named third-party assemblies (`VRCSDK3A`) rather than core Unity/mscorlib types —
   if a *Unity* type is unresolved, the reference set itself is incomplete and every count downstream
   is suspect (an under-count, not a clean bill of health).
3. Filter results the same way the de4dot-side methodology does: only errors touching a
   plugin-internal type (`DreadScripts.*`) are meaningful; everything else is dependency noise.

## Pitfalls

- Don't treat a small `ilverify` error count as good news without checking whether the reference set
  was actually complete — a missing reference silently *skips* verification of methods that use it,
  which looks identical to "no errors" unless you specifically check for `FileLoadErrorGeneric`/
  similar resolution-failure lines mixed into the output.
- Don't confuse `deps/unity/`'s adequacy for decompilation with adequacy for verification — it's
  deliberately a smaller, portable set and will produce a much less trustworthy `ilverify` run than
  the full local Unity install.
- Don't chase `VRCSDK3A`-attributed errors as if they were real bugs — that dependency is
  legitimately unavailable, and its absence is expected, not a research question.
