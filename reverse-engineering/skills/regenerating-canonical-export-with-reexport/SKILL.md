---
name: regenerating-canonical-export-with-reexport
description: scripts/reexport.py refreshes export/ from binaries/ via de4dot + ilspycmd. MANDATORY after any de4dot change that affects output — a de4dot fix is not finished until work/de4dot is republished and export/ is regenerated and committed. Also use when a new binary is added to binaries/.
---

# Regenerating the Canonical `export/` with `reexport.py`

## When to use

- **MANDATORY: after any de4dot change that can affect deobfuscated output.** A de4dot fix is not
  done until `export/` reflects it. Do not report a de4dot improvement as complete, and do not move
  on to other work, while `export/` is still stale. Treat
  *de4dot change → republish to `work/de4dot/` → reexport → commit both* as one atomic unit.
- A new or updated DLL was added to `binaries/`.

**Why this is mandatory, not discretionary:** `export/` is the ground truth that every reconstruction
decision in the restored package is validated against. A stale `export/` means reconstruction is being
checked against output from an older, buggier de4dot — silently wasting effort and baking in wrong
logic. This has actually gone wrong: `work/de4dot/de4dot` was once left at a months-old build, so
`export/` predated every fork improvement in `WORKLOG.md`.

The only thing this skill is *not* for is a quick "does my change help?" probe during development —
for that use the `fast-iterating-with-de4dot-scorecard` skill against `/tmp` output. But once the
change is being landed, regenerating `export/` is required.

## Republish de4dot first (easy to forget)

`reexport.py` runs `work/de4dot/de4dot` — a self-contained binary **copied into this repo**, not the
sibling fork's build output. Building `../de4dot` alone changes nothing here. Republish and copy:

```bash
dotnet publish ../de4dot/de4dot/de4dot.csproj -c Release -f net10.0 -r linux-x64 --self-contained true
rsync -a --exclude 'publish/' --exclude 'constdata/' \
  ../de4dot/Release/net10.0/linux-x64/ work/de4dot/
```

Two things about that pair of commands:

- **net10.0, not net8.0.** The fork-wide net8.0 pin is gone; only `de4dot.constdata` is pinned, and
  that worker lives in `work/de4dot/constdata/` and is published separately. Excluding it is why the
  copy is an `rsync --exclude`, not a `cp` of one file.
- **Copy the build output directory, not `publish/`.** `Directory.Build.props` sets
  `PublishSingleFile`, so `publish/` holds one 78 MB executable while `work/de4dot/` is the
  368-file self-contained layout that `Release/net10.0/linux-x64/` already is.

Then sanity-check the new binary actually contains your change (e.g. `-v` and grep for a log line the
new pass emits) *before* spending time on a full re-export.

## Only one re-export at a time

`reexport.py` takes an exclusive lock (`work/export.lock`) before it deletes anything, and holds it
until the rebuild finishes. A second run **refuses immediately** and names the holder rather than
queueing silently; `--wait-lock SECONDS` opts into queueing.

This is not defensive plumbing. Step 1 deletes the tracked `export/` tree wholesale before rebuilding
it, so two overlapping runs do not fail loudly — one's delete interleaves with the other's writes and
leaves a tree quietly missing most of its files. That has happened here: a concurrent pair produced a
**67846-line deletion** that read exactly like a catastrophic deobfuscator regression and was nothing
of the kind. If you see a re-export diff far larger than your change could explain, suspect this
first and check whether anyone else is working in the repo.

A lock whose owning process is gone *and* which names this host is reclaimed automatically. One that
cannot be parsed is left alone — "I could not read it" is not evidence that nobody holds it.

## What it does

```
scripts/reexport.py [--il] [--no-rename] [--unity-managed DIR ...] [--vrc-packages DIR] [-r DIR ...]
```

1. **Deletes `export/` entirely** and recreates it empty.
2. For every `*.dll` in `binaries/` (except `0Harmony.dll`, skipped as a dependency, not a target):
   deobfuscates via de4dot into a temp dir, applies `renames/<name>.json` with `ilrename`, then
   decompiles via `ilspycmd -p --nested-directories` into `export/<name>/`, flattening ilspycmd's
   nested project-dir output if it produces one.
3. Cleans up the temp directory and prints an `ilrename` coverage report per mapped assembly.

`export/` is **named by default** — the rename step is part of the canonical export, not a second
tree. An assembly with no map, or a map with nothing assigned, exports identically either way.

`--no-rename` skips `ilrename` and exports the raw deobfuscated assemblies into the same `export/`.
Use it when a chosen name is itself in question: `ilrename` refuses virtual/override methods and
constructors, and only *warns* when a renamed name also appears in a string literal, so if a name
might be a reflection target the raw tree is what to check it against. **Do not commit a raw tree** —
`export/` is tracked and named, so a raw one silently changes what every later diff means. The tree
records which mode built it in `export/.renames-applied`, and the script prints the mode at both
ends of the run.

## Reference-assembly auto-detection

Unlike a manual ilspycmd invocation, this script actively searches for reference assemblies instead
of requiring them spelled out every time:

- **Unity install**: searches common Unity Hub Editor install roots (Windows, WSL `/mnt/c/...`
  translation, macOS, Linux) for the exact `UNITY_VERSION` constant in the script (currently
  `2022.3.22f1`) and includes both `Data/Managed` and its `UnityEngine/` subdirectory. Override with
  one or more `--unity-managed DIR` flags if auto-detection doesn't find it (e.g. this environment's
  install lives outside any Hub-managed path — see the `resolving-reference-assemblies-for-
  decompilation` skill for the actual local path).
- **0Harmony.dll**: auto-included from `binaries/` if present.
- **VRC SDK**: resolved from the checked-in `deps/vrchat/`. (It once searched a Unity project's
  `Packages/` for `com.vrchat.avatars`/`com.vrchat.base` and their
  `Runtime|Editor/VRCSDK/Plugins` subdirectories.
- **`-r DIR`** (repeatable): any additional reference directory, appended after auto-detection.

If Unity isn't found by auto-detection and no `--unity-managed` is given, it still proceeds but warns
that decompilation may lack proper Unity type info — check for that warning in the output before
trusting the resulting `export/` as fully resolved.

## Workflow

1. Republish de4dot into `work/de4dot/` (see above) and verify the new binary has your change.
2. Run it with this repo's **checked-in** reference assemblies — do not rely on auto-detection
   finding a machine-local Unity install (not reproducible, and historically an incomplete set):

   ```bash
   python3 scripts/reexport.py --unity-managed deps/unity -r deps/vrchat
   ```
3. After it completes, do the triage pass from `triaging-dotnet-reactor-obfuscation` (grep for
   `smethod_N`/`goto`/`TODO` markers) to see what changed relative to before.
4. Update `RE_NOTES.md` with the refreshed pipeline run details (sizes, file counts, anything newly
   resolved) per that skill's convention — don't leave the "Project Status Summary" stale after a
   real re-export.
5. Diff the new `export/` against the previous state (if you kept a copy, or via git if `export/` is
   tracked) before assuming nothing meaningful changed.

## Common scenarios

**Scenario: testing whether a de4dot fork change actually helps, before committing to a full
re-export.** Don't reach for `reexport.py` first — use the `fast-iterating-with-de4dot-scorecard`
skill against a single sample in `/tmp` to get a quick correctness/marker read, and only run the full
canonical re-export once you're confident the change is worth reflecting in tracked `export/`.

**Scenario: wondering which reference assemblies to pass.** Use the ones checked into this repo:
`--unity-managed deps/unity -r deps/vrchat`. They exist precisely so runs are reproducible and the
set is complete. Do not point this at a machine-local Unity install (e.g. somewhere under a user's
home/Downloads) — that is not reproducible for anyone else and has previously been an *incomplete*
set, which silently degrades output quality.

**Scenario: the re-export produced a large, surprising diff.** Two known causes, both benign but
worth checking rather than assuming: ilspycmd versions differ in namespace-folder layout (dotted
`DreadScripts.ADOverhaul/` vs nested `DreadScripts/ADOverhaul/`), which shows up as mass
delete+recreate; and de4dot's generated names can shift between builds. Say so explicitly rather
than letting it break silently under someone else's work.

A name shift does *not* invalidate `renames/{Assembly}.json` — its keys are metadata tokens, not
names, precisely so this case survives. What it does invalidate is the human-readable name label in
front of each token, which `ilrename report` flags as drift; `reexport.py` prints that report at the
end of every run, so check it rather than assuming the maps still say what you think they say.

**Scenario: a re-export produces no diff at all.** Now a real signal, and the expected way to learn
your change did nothing to the exported source. `export/` used to carry one irreducibly noisy file —
a `dummy_ptr/-<guid>-.cs` placeholder ILSpy synthesised with a fresh GUID every run — so "only that
file changed" was the old way of saying "nothing changed". The binary that produced it was a derived
artefact and has been removed (see `AGENTS.md` → Repository Layout), so every diff here is now
meaningful.

## Pitfalls

- **Don't skip this after a de4dot change because it feels heavyweight.** It is fast in practice
  (the full re-export of every binary completes in well under a minute), and leaving `export/` stale is far
  more costly than running it. It is destructive only in the sense that it rebuilds `export/` from
  scratch — which is tracked in git, so the diff is reviewable and revertible.
- Don't forget to republish `work/de4dot/de4dot` first — rebuilding `../de4dot` alone changes nothing
  here, and re-exporting with the old binary produces an unchanged `export/` that looks like your fix
  had no effect.
- Don't assume auto-detection found a complete reference set silently — pass `deps/` explicitly.
- After a re-export, don't forget the file-naming/namespace-folder layout can shift between ilspycmd
  versions (flat `DreadScripts/` vs. dotted `DreadScripts.ADOverhaul/` folders) — this has caused
  drift against the public repo's older snapshot before; check the "Deferred" resync task context in
  git history if reconciling against `reverse-engineering/export/`.
