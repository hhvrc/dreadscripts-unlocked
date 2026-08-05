---
name: triaging-dotnet-reactor-obfuscation
description: Establishes the deobfuscation pipeline for a .NET Reactor-protected assembly (de4dot → ilspycmd) and documents exactly what de4dot leaves behind so it's clear what still needs manual work. Use at the start of work on any new or re-baselined DLL in binaries/, or when re-running the pipeline after a de4dot fork update.
---

# Triaging .NET Reactor Obfuscation

## When to use

- A new obfuscated DLL has been dropped into `binaries/` and needs a first pass.
- The `work/de4dot/de4dot` binary was updated (e.g. from the local de4dot fork) and the pipeline
  needs re-running to see if more is now resolved.
- You need to decide whether an oddity in `export/` is a de4dot limitation (expected) or a real bug
  worth reporting upstream to the de4dot fork.

## Background

**de4dot lives at `../de4dot`** (sibling directory, i.e. `/src/reverse-engineering/de4dot` from this
repo's perspective) and is a fork **actively being developed alongside this project specifically to
improve .NET Reactor handling** — it is not a frozen third-party dependency. When the pipeline
leaves something unresolved, always check whether it's a known limitation currently being worked on
there (`../de4dot/WORKLOG.md`, `../de4dot/IMPROVEMENT_PLAN.md`) before assuming it's permanent or
spending time working around it by hand — a fork rebuild may resolve it outright.

Both `ADOverhaul*.dll` and `ControllerEditor.dll` are obfuscated with **.NET Reactor**, layering:
1. An outer native loader wrapping the real managed assembly.
2. Symbol renaming (types/methods/fields → `Class0`, `smethod_N`, `field_N`, ...).
3. Proxy-delegate indirection around common calls (`GetExecutingAssembly`, `Equals`, etc.).
4. Runtime string encryption via generic `smethod_N<T>(int)` methods, guarded by a
   `GetCallingAssembly() == GetExecutingAssembly()` check (external callers get `default(T)`).
5. Switch/XOR control-flow obfuscation — a state variable driving a `switch` whose case values are
   produced by an affine transform (`(key * A) ^ B`), scattered across many blocks per method.

## What de4dot already does (do not redo this by hand)

- Strips the native loader → recovers the real managed assembly (e.g. 779KB → 339KB for ADO).
- Renames obfuscated symbols to readable de4dot names (`Class0` → real-ish names, still generic).
- Inlines proxy delegates — calls route directly again instead of through wrapper classes.

## What de4dot does NOT do (this is where the actual work is)

- **Does not decrypt `smethod_N` strings.** The caller-check guard blocks de4dot's normal static/
  dynamic string inliner from safely invoking the method. This is the entire subject of
  the `decrypting-dotnet-reactor-smethod-strings` skill.
- **Does not fully simplify the switch/XOR control flow.** Large assemblies can have 300-800+
  dispatch patterns; some resolve, most don't on complex methods. See
  the `resolving-residual-control-flow-manually` skill for reading what's left by hand.

Both of these are active areas of improvement in the local de4dot fork (`../de4dot`) — check its
`WORKLOG.md`/`IMPROVEMENT_PLAN.md` before assuming a gap is permanent; it may already be fixed on a
branch that hasn't been re-run here yet.

## Workflow

```bash
# 1. Deobfuscate with the local de4dot fork (not a system-installed de4dot) —
#    the binary here is built from ../de4dot; rebuild there and copy over to pick up fork changes
work/de4dot/de4dot binaries/<Name>.dll -o work/deobf/<Name>.dll

# 2. Export as a compilable C# project (always use -p; -o required)
DOTNET_ROLL_FORWARD=LatestMajor ilspycmd -p -o export/<Name> work/deobf/<Name>.dll
```

Large assemblies (ControllerEditor: 793 switch/XOR patterns) can take **2+ hours** on CFG cleaning
and have crashed mid-run (exit code 1) — expect this, run in the background, and don't assume a
long-running de4dot invocation is hung.

After exporting, do a quick triage pass:

```bash
# Count remaining smethod_N calls (undecrypted strings) per export
grep -rho 'smethod_[0-9]*<[^>]*>(' export/<Name>/ | sort | uniq -c | sort -rn

# Spot switch/XOR dispatch patterns left unresolved (heuristic — look for dense
# switch(...) blocks with many `case` labels that fall through as goto soup)
grep -rlc 'switch (' export/<Name>/ | sort -t: -k2 -rn | head
```

Record the pipeline run (DLL sizes before/after, file counts, any crashes) at the top of
`RE_NOTES.md` under "Project Status Summary" so the next session doesn't have to re-derive it.

## Common scenarios

**Scenario: a fresh DLL just landed in `binaries/`.**
Run the two-step pipeline, then grep for `smethod_` and dense `switch` blocks to get a rough size of
the remaining work before touching any source files. Don't start writing clean source in
porting anything until this triage is done — it tells you which files are "clean 1:1" candidates vs.
which need the string/CFG workflows first.

**Scenario: de4dot output for the same DLL changed after a fork update.**
Diff the new `export/<Name>/-Module-.cs` against the previous run (if kept) or at least re-run the
`smethod_` / `switch` counts above. A drop in either count means the fork's improvement is landing —
note it in `RE_NOTES.md` so already-ported source can be re-checked against the newly
resolved code instead of hand-reconstructed logic that's now redundant.

## Pitfalls

- Don't hand-decrypt strings or hand-resolve control flow that a newer de4dot build would now
  resolve automatically — check the fork's `WORKLOG.md` first, since re-running the pipeline is far
  cheaper than manual reconstruction.
- `export/` is READ-ONLY by repo convention — never edit it, even to "fix" an obviously wrong
  decompile; treat the wrongness as a data point about the pipeline, and put corrections in
  `../public/unity/Assets/com.dreadscripts.unlocked/Editor/`.
- A crashed de4dot run on a large assembly may still have produced a **partial but reusable**
  `work/deobf/*.dll` from a prior successful run — check timestamps before assuming you need to
  babysit a fresh multi-hour run.
