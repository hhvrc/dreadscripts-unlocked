# AGENTS.md

Project instructions for coding agents working in this repository. `CLAUDE.md` and
`.github/copilot-instructions.md` are pointers to this file — keep the content here, not in copies.

## AI Session Notes

**Read `RE_NOTES.md` at the start of every session.** It contains:
- Current reconstruction status (what's done, what has known issues)
- Non-DRM decrypted strings, and the ones still needing decryption
- Why names were chosen and what has been verified (the map itself lives in `renames/`)
- Priority task list for next steps

It does **not** contain the DRM protocol — that is [`../DRM.md`](../DRM.md) — or the
repository layout, which is this file. See "Documentation rules" below.

### The five things most easily got wrong here

- **`export/` is READ-ONLY.** It is decompiler output with `renames/` already applied, regenerated
  from `binaries/` by `scripts/reexport.py`. Hand edits are silently lost on the next run and make
  diffs meaningless. Cleaned-up source is ported out of it into
  [`../public/unity/`](../public/unity/) — see the next point.
- **Renames go in `renames/{Assembly}.json`, never into decompiled text.** `ilrename` applies them to
  the assembly metadata before ILSpy runs, so every use site follows automatically. See "Renaming"
  below. A name that exists only in prose will not appear anywhere in the output.
- **`dreadre-devel/` is gone. Clean source lives in `../public/unity/`, and is ported from
  `export/`, one file at a time.** Retired 2026-07-31: it was a reconstruction that had drifted from
  `export/`, so rather than reconcile it, the Unity project moved to
  `../public/unity/Assets/com.dreadscripts.unlocked/Editor/` and its 96 `.cs` files were dropped.
  Each is now re-derived from `export/` and polished on the way in. Two rules that follow:
  **the licence/DRM code is not ported at all** — but note that *neither* product keeps it in a file
  of its own. Both `ADOverhaul` and `ControllerEditor` carry HWID/HMAC validation *inline in their
  root class*, alongside the editor window, inspectors and settings, so both must be **stripped
  during the port rather than skipped** — excluding either file drops that whole tool; and the two vendor packages are consolidated into
  one, because the tools shared most of their code. `python3 scripts/port_status.py` says what has
  landed and what is left — it diffs the two trees rather than reading a checklist, so it cannot go
  stale. Any older reference to `dreadre-devel/` or to `Assets/DreadScripts/Editor/` is dead.
- **Reusable tooling belongs in `scripts/`, not in a scratch directory.** Before running any
  multi-step toolchain work by hand, or writing a script to do it, read the **"Scripts"** section
  below: the workflow you need very likely already exists as a subcommand, and if it doesn't, the
  rule is to *add* it there rather than improvise. A measurement kept in `/tmp` cannot be reproduced
  or diffed against next session, which is how baselines silently stop meaning anything. A genuinely
  single-use snippet is still fine in scratch — see the same section for where that line falls.
- **Never write a fact down twice.** Each fact class has one owning file (**"Documentation rules"**
  below). Summarising something owned elsewhere feels helpful and is how this project produced a wiki
  that contradicted every other document, a rename map that had been deleted for being wrong, and
  status claims about de4dot that were months out of date. Link instead.

## Project Purpose

Reverse engineering and documentation of the DRM mechanisms used by two discontinued Unity Editor plugins (ADOverhaul and ControllerEditor) sold by DreadScripts on Gumroad. The backend validation service has been permanently shut down, so no copy can pass its licence check. The goal is to understand the validation system and restore the tools to a runnable state without it.

## The WIP/finished split

**This repo holds work in progress. `../public` holds finished work.** When something is done it
*moves* to `../public` (published as [dreadscripts-unlocked](https://github.com/hhvrc/dreadscripts-unlocked))
and is **deleted here** — it does not get copied and kept in both places.

Already migrated, and deliberately absent from this repo:

| Artefact | Now lives at | This repo does |
|---|---|---|
| The DRM specification | [`../DRM.md`](../DRM.md) | link to it; keep no copy |
| The restoration server | `../public/drm_server/` | link to it; keep no copy |

`python3 scripts/sync_public.py check` enforces this: it fails if a migrated artefact reappears here,
if a link still points at its old in-repo path, or if the published copy of `export/` in
`reverse-engineering/export/` has fallen behind. `promote <path>` performs the move; `snapshot` refreshes
the published copy — run it after every re-export, because nothing else will notice it going stale.

**Why "move" and not "copy".** Both of the artefacts above previously existed in both repos and both
drifted without anyone noticing. `DRM.md`'s two copies diverged, and then the deleted `wiki/` grew a
*third* copy describing a server path (`mock_server/main.go`) that existed nowhere. `drm_server/` was
worse: a commit here titled *"fix: correct misleading comment on handleActivate/handleVerify"* never
reached the public copy, so the repo users actually read kept the misleading comment. Two copies of a
file in two repos cannot be kept in step by hand, because noticing the drift requires remembering to
diff two repos, and nobody does.

## Documentation rules

**Every fact has exactly one owner. Link to it; never restate it.**

| Fact class | Owner |
|---|---|
| Repository layout, file counts, toolchain, scripts, workflows | `AGENTS.md` (this file) |
| DRM protocol, crypto, keys, cache format, wire captures | [`../DRM.md`](../DRM.md) |
| Reconstruction status, why a name was chosen, what is verified | `RE_NOTES.md` |
| The obfuscated → chosen-name map itself | `renames/{Assembly}.json` |
| Open questions and their evidence | `theory/<topic>/<question>/THEORY.md` |
| de4dot's state, gates, root causes, open bugs | `../de4dot/ROADMAP.md` |
| How to do a *kind of work* | the matching `.claude/skills/` skill |

This is not a style preference — it is the single largest source of wrong information this project has
produced. Documented failures, all real:

- A `wiki/` directory (now deleted) paraphrased `README.md`, this file, and `DRM.md` across 1,357 lines.
  Being a *paraphrase* rather than a byte copy is what made it dangerous: it could not be diffed, so it
  rotted invisibly. It ended up resurrecting the class rename map that `RE_NOTES.md` had deliberately
  deleted for being wrong — **with the wrong values still in it** — and documenting a dead pipeline
  (`dnSpy.Console` → `work/decompiled/`) and a dead output path (`Assets/DreadScripts/Editor/`).
- `RE_NOTES.md` asserted "de4dot does NOT decrypt smethod_N strings" and listed a de4dot bug as "live,
  not yet root-caused" months after both had been fixed on the fork. It had a local summary of state
  that `../de4dot/ROADMAP.md` owns.
- `README.md`'s copy of the repository layout claimed 13 export classes and 14 devel source files
  against real counts of 15 and 30.
- Two AES key/IV pairs and three tables of smethod transform constants existed in three files at once.
  Nothing could have kept them in step, and a stale decryption constant fails *silently* — it decrypts
  to garbage rather than erroring.

Concretely, when writing or updating docs here:

- **Adding "just a brief summary" of something owned elsewhere is the failure mode.** Link instead.
- **Never hand-write a number that a script produces** — file counts, string counts, gate results,
  coverage. `export/` is regenerated wholesale by `reexport.py`, so any count typed by hand is wrong
  as soon as it next runs. Say which command prints it.
- **Never hand-copy a constant** (key, IV, transform pair, token). Point at the owner, or at the
  command that re-derives it — `dotnet_reactor.py detect` for transforms, `ilrename report` for tokens.
- **If a section has gone stale, fix or delete it.** Writing the correct version somewhere else while
  leaving the stale one is how every contradiction above was created.
- **Prefer deleting a paragraph to hedging it.** A document nobody trusts costs more than a short one.
- **When you do delete a drifted copy, say so in one line where it used to be**, naming what it got
  wrong. Several notes in these files do that; they are why the failure modes above are documented
  rather than repeated.

Direction of dependency, which is asymmetric on purpose: **this repo may reference `../de4dot`;
`../de4dot` must never reference this repo, this organisation, or these sample filenames** — see
`../de4dot/CLAUDE.md`. When a finding here belongs to de4dot, port it into de4dot's own generic phrasing
rather than linking back.

## Repository Layout

```
binaries/            # Original obfuscated DLLs (+ 0Harmony.dll dependency)
export/              # READ-ONLY — decompiler output (ILSpy), with renames/ already applied to the
                     # metadata; never edit directly. Regenerated wholesale by reexport.py.
                     # one directory per namespace segment (ilspycmd --nested-directories)
                     # .renames-applied records that this tree is the named variant
  ADOverhaul2019/            # 15 .cs
  ADOverhaul2022/            # 15 .cs
    DreadScripts/ADOverhaul/     # individual class files
    -Module-.cs                  # full assembly dump (search/reference)
  ControllerEditor/          # 70 .cs
    DreadScripts/ControllerEditor/
    DreadScripts/Common/SupportThankies/
    -Module-.cs
renames/             # {Assembly}.json — the name map: obfuscated entity -> hand-chosen name,
                     # keyed by metadata token. Hand-edited; this is where naming decisions live.
tools/IlRename/      # ilrename: applies renames/ to an assembly's metadata before decompiling
                     # (clean source is NOT here — it lives in ../public/unity/, see below)
dumps/               # Output from analysis tools (gitignored except .gitkeep)
  intercepted.txt    # Hooks.cs runtime log (ControllerEditor, 1318 strings)
  ADOverhaul2022.txt # SmethodStringDumper offline dump (740 strings)
  string_table.txt   # Test class string[] arrays
scripts/             # Python analysis toolkit — see "Scripts" section below
theory/              # Open RE investigations tracked as falsifiable hypotheses (de4dot bugs,
                     # DRM protocol questions, etc.) — see "Skills" section below
work/                # Local tooling + de4dot pipeline (gitignored)
  de4dot/            # de4dot v3.2.0.0 executable (native ELF binary)
  deobf/             # de4dot output DLLs (deobfuscated)
  ilrename/          # published ilrename binary; reexport.py rebuilds it if missing
```

`binaries/` holds original distributions only. A fourth entry, `ADOverhaul2019-cleaned.dll`, was
removed on 2026-07-30: it was not an original but an ADOverhaul2019 deobfuscated by an unrecorded
pre-fork toolchain, so the one thing it could have been — a quality baseline for the fork — was a
number nobody could reproduce. It looked better than the fork's output (22 `goto` vs 53, 0 residual
`smethod_N` vs 35) but that second figure was an artefact: undetected as obfuscated, it got no
renaming pass, so its proxies kept Reactor's invisible-Unicode names and gate 7's `smethod_\d+`
never saw them. Its only measurable effect was a phantom `dummy_ptr/-<guid>-.cs` diff on every
re-export. Do not re-add a derived binary here.

`export-named/` was removed on 2026-07-30 and folded into `export/`, which is now named by default.
The two trees held the same decompiled source rendered twice, so every reader and every consumer had
to know which of two nearly identical directories they wanted — and the published snapshot in
`../public` was taken from the raw one for months without anyone noticing. `reexport.py --no-rename`
covers the case the raw tree existed for.

## No Build System

This is a research/documentation repository: no solution file and no test suite. The one thing that
does build is `tools/IlRename` (`dotnet publish`, done for you by `reexport.py`). Work happens in
three modes:

1. **Static analysis** — reading (never editing) `export/` for reference and cross-checking
2. **Deobfuscation passes** — running de4dot against the original DLLs in `binaries/`, then ilspycmd into `export/`; `scripts/reexport.py` does the whole pipeline
3. **Naming and cleanup** — recording names in `renames/` (applied to metadata, see "Renaming") and porting clean source into [`../public/unity/`](../public/unity/)

## Toolchain

- **ilspycmd** — CLI decompiler (ILSpy v9.1+); used for all exports to `export/`
- **de4dot** — automated deobfuscation (control flow, string encryption); native binary at `work/de4dot/de4dot`, built from the sibling `../de4dot` fork (actively co-developed alongside this project — see the Skills section below)
- **scripts/dotnet_reactor.py** — Python toolkit: `detect <Module.cs>` extracts transforms, `fill <dump> <src/>` replaces smethod calls with string literals in source files (source dir is `../public/unity/Assets/com.dreadscripts.unlocked/Editor/`)
- **ilrename** — applies `renames/*.json` to assembly metadata between de4dot and ilspycmd (`tools/IlRename`, .NET 10, dnlib + System.CommandLine); built to `work/ilrename/` on demand by `reexport.py`

### Publishing de4dot into `work/` — the constdata worker is not optional

`work/de4dot` is populated by copying the fork's build output. Copying **one** framework directory
(`../de4dot/Release/net10.0/<rid>/`) is not enough: the net8.0 `de4dot.constdata` worker must come
too, in a `constdata/` subdirectory beside `de4dot`. The host is net10.0 but that worker stays pinned
to net8.0 because .NET 10's loader rejects Reactor metadata, and de4dot runs it out-of-process to
extract the constant/string data array. In the fork's own `Release/` layout the worker is found by
probing a *sibling* `net8.0/<rid>/` directory, which is why the dev tree works and a one-directory
copy into `work/` silently does not — de4dot still exits 0 and writes an assembly in which **nothing
is decrypted**, and only gate 7 notices. `pipeline.find_de4dot()` now warns when the worker is
unreachable. Publish it explicitly:

```bash
dotnet build -c Release ../de4dot/de4dot.net.slnf
cp -a  ../de4dot/Release/net10.0/<rid>/.  work/de4dot/
dotnet publish ../de4dot/de4dot.constdata -c Release -r <rid>
mkdir -p work/de4dot/constdata
cp -a  ../de4dot/Release/net8.0/<rid>/publish/.  work/de4dot/constdata/
```

### Cross-platform: this toolkit runs on Windows too

The scripts run on Windows (the ported Unity project in `../public/unity/` is worked on from one), and
two platform assumptions have bitten before. Both are handled centrally now; keep them that way.
**File locking**: `apply_renames.py` takes a `msvcrt`/`fcntl` shim, not a bare `import fcntl`.
**Console encoding**: a Windows console is cp1252 and cannot encode the box-drawing rules, arrows and
em dashes every report uses — printing one raises `UnicodeEncodeError` *after* the work is done. Any
script that prints such characters must call `pipeline.configure_console()` (UTF-8 + `errors=replace`)
at import; `pipeline.py` calls it for everything that imports it, and the three report scripts that do
not (`sync_public.py`, `explore_dreadscripts.py`, and `apply_renames.py`'s siblings) call it directly.

## Scripts

### Start here: which tool do I want?

| I want to… | Run |
|---|---|
| Accept or reject a de4dot change (every gate, before/after, diffable) | `python3 scripts/de4dot_scorecard.py gates --all --json /tmp/after.json --de4dot-dll ../de4dot/Release/net10.0/linux-x64/de4dot.dll` |
| Quickly see one sample's ilverify + marker state | `python3 scripts/de4dot_scorecard.py full ADOverhaul2022` |
| Compare two de4dot configurations against each other | `python3 scripts/de4dot_lab.py …` |
| Regenerate the canonical `export/` (destructive, authoritative) | `python3 scripts/reexport.py --unity-managed deps/unity -r deps/vrchat` |
| (that one takes `work/export.lock`; a second run refuses rather than racing — `--wait-lock SECONDS` to queue) | |
| Check reconstruction work still compiles | `python3 scripts/typecheck_package.py` |
| Refresh `renames/` skeletons after a de4dot change, keeping chosen names and hand-added notes | `python3 scripts/refresh_renames.py` (`-n` to preview) |
| Find mis-resolved state machines in `export/` before reconstructing one | `python3 scripts/detect_broken_state_machines.py` |
| Size how much closure structure survives into a decompiled tree, split by cause | `python3 scripts/analyze_closures.py` |
| Read/compare metadata totals for an assembly | `work/ilrename/ilrename counts --in <dll>` |
| Check nothing finished has been copied back into this repo | `python3 scripts/sync_public.py check` |
| Move a finished directory out to the public repo | `python3 scripts/sync_public.py promote <path>` |
| Refresh the published copy of `export/` in `../public` (do this after every re-export) | `python3 scripts/sync_public.py snapshot` |

Every one of these takes `--help`. Check that first; the workflow you need is usually already a flag.

### The rule about writing new tooling

**Tooling lives in `scripts/`, not in a scratch directory.** If you are about to write a script that
runs de4dot, ilspycmd, ilverify or ilrename, or that produces a number anyone might want to compare
against later, it goes in `scripts/` (as a new subcommand on the tool that already owns that job, if
there is one) and it gets committed — even when you wrote it mid-task for one measurement.

**If you find yourself needing a workflow that does not exist yet, and you can reasonably expect a
future session to need it too, add it to the tooling.** Extend the script that already owns the
closest job — a new subcommand, a new flag, a new baseline constant — and only create a new script
when nothing sensibly owns it. Then document it in the table above and in the covering skill, so the
next agent finds it instead of rebuilding it slightly differently. Improvising the same multi-step
pipeline by hand each session is the failure this section exists to prevent: it is slower, it is not
reviewable, and two hand-runs of "the same" check are not actually comparable.

The test for whether something belongs here is not "is this throwaway?", it is **"will this result
need to be reproduced or diffed against later?"** Gate results, baselines and correctness numbers
always fail that test: a baseline kept in `/tmp` cannot be compared against next session, so the next
session re-derives it by hand, differently, and the comparison silently stops meaning anything.
`detect_broken_state_machines.py` and `de4dot_scorecard.py` both started as exactly this kind of
"one-off".

### When a scratch script is fine

**It is genuinely fine to write a throwaway in a scratch directory when the thing is niche enough
that it almost certainly will not be wanted outside the exact piece of work in front of you.** Not
every snippet needs to become tooling, and turning one into a committed subcommand has its own cost:
a flag nobody uses again is still a flag everyone reads.

Good scratch: hand-simulating one specific method's state machine to confirm what it does; counting
something in one file to answer a question mid-investigation; a scrap of Python to decode one
constant. These answer *"what is true about this one thing, right now"*, and the answer, once known,
is written into notes or a commit message — nobody needs to re-run the script to trust it.

Not scratch: anything that invokes de4dot/ilspycmd/ilverify/ilrename, and anything producing a number
that a later session will want to compare against — gates, baselines, coverage, counts. If you would
have to run it again to know whether something regressed, it is tooling.

When it is genuinely ambiguous, prefer `scripts/`: an unused subcommand is a much cheaper mistake
than a baseline nobody can reproduce.

Two rules follow from that and are not negotiable:

- **Nothing but `pipeline.py` invokes the toolchain.** Everything else imports it, so the reference
  set, the de4dot argument order and the failure handling are resolved in one place.
- **Baselines live next to the gate they belong to**, in `pipeline.py` (`STATE_MACHINE_BASELINE`,
  `DECRYPT_BASELINE`), never inline in the script that reads them — otherwise a gate and its ceiling
  drift apart.

| Script | Purpose | Covering skill |
|---|---|---|
| `pipeline.py` | **Shared toolchain — not run directly.** The only place that resolves reference assemblies or invokes de4dot/ilspycmd/ilverify/ilrename, and where every gate baseline lives. Everything else imports it. `python3 scripts/pipeline.py` prints the resolved reference set as a sanity check | — |
| `dotnet_reactor.py` | Detect .NET Reactor's per-method A/B transform constants, scan/fill `smethod_N` string-decryption calls against a runtime or offline dump | `decrypting-dotnet-reactor-smethod-strings` |
| `reexport.py` | **Canonical, destructive** re-export: deletes and rebuilds `export/` from `binaries/` via de4dot + ilrename + ilspycmd, against the checked-in `deps/` reference set. `--no-rename` builds the raw tree instead | `regenerating-canonical-export-with-reexport` |
| `../tools/IlRename` | `ilrename` — applies `renames/{Assembly}.json` to assembly metadata before decompiling; also `template`, `report`, `usages` and `counts` (metadata totals, the one regression signal that survives renaming). See "Renaming" below | — |
| `de4dot_scorecard.py` | **Fast, non-destructive** iteration: deobfuscate → ilverify → decompile → marker-triage against throwaway `/tmp` output, for testing a de4dot build or checking a sample without touching `export/`. Its `gates` subcommand is the **acceptance check for a de4dot change** — every gate (1 ilverify, 5 state machines, 6 metadata round-trip, 7 decryption coverage, plus metadata counts) in one run, non-zero exit on any failure, `--json` for diffing before against after | `fast-iterating-with-de4dot-scorecard` |
| `de4dot_lab.py` | **A/B experiments.** Two variants differing by de4dot args, env or ilspycmd args, each deobfuscated + decompiled into a temp workspace (deleted unless `--keep`), then diffed on state-machine termination, dispatch counts, `goto` density and `smethod_N`. Dispatches are counted in **two** rows because they are two different Reactor shapes: `switch(var)` for a plain state variable, and `switch((num = (num*A)^B) % k)` for the opaque-predicate form, which the first row does not match. Also `show` for one type's C#/IL. Never touches `export/` | — |
| `analyze_closures.py` | Counts compiler-generated closure types still visible in a decompiled tree and splits them by *why* they were not inlined: obfuscator residue (a writable static self-reference field — de4dot's to remove), a captured parent closure (a decompiler limit), or neither. `--list` names the affected types and construction sites; `--dir` points it at a lab workspace instead of `export/`. The metric that closed ROADMAP §7 item 5 | — |
| `detect_broken_state_machines.py` | Traces every `while (true) { switch }` in `export/` from its seed and reports whether it terminates. **Run before reconstructing any method containing one** — a LOOPS verdict means de4dot mis-resolved a case, so statements are silently missing from the decompiled body | — |
| `rename_status.py` | Per-type naming coverage, read from `renames/` alone — no deobfuscated DLL needed, so it answers "what is left" without a re-export. `--smallest N` is the work queue. Counts members that already carry their real name (`Dispose`, `op_*`, Unity messages) separately, since those must never be renamed | `renaming-and-documenting-deobfuscated-source` |
| `apply_renames.py` | **The only writer for `renames/*.json`.** `show` lists a type's members and points at its `export/` file; `set` assigns names, and `set --set-type` names the type itself (same lock; refuses a name a sibling type in the same scope already holds). Every write takes an exclusive lock across the whole read-modify-write and *waits* rather than failing, which is what makes parallel naming safe — hand-editing the maps concurrently loses writes invisibly. `--force` corrects a name already set | `renaming-and-documenting-deobfuscated-source` |
| `mirror_renames.py` | Copies member names between the two ADOverhaul builds by token, but only where the two `export/` bodies are identical under identifier masking *and* kind/signature agree. Most type pairs fail that check — a branch inversion is enough — so the 2019 map is largely hand work | — |
| `port_status.py` | What is left to port from `export/` into `../public/unity/`, diffed from the two trees rather than a checklist. Owns the exclusion policy, including which files must have their licence code **stripped** rather than being skipped | — |
| `typecheck_package.py` | Compiles the restored package in `../public/unity/` against `deps/` and reports C# errors. **Run before committing reconstruction work** — this repo has no build system, so members left on the wrong class, stale call sites and swapped arguments are otherwise invisible. A type check, not a correctness check | — |
| `sync_public.py` | The WIP/finished boundary with `../public`. `check` fails if a migrated artefact (`DRM.md`, `drm_server/`) has reappeared here, if a link still points at its old in-repo path, or if `reverse-engineering/export/` has fallen behind `export/`; `promote <path>` moves a finished path out to `../public`; `snapshot` re-copies `export/` into `../public` | — |
| `explore_dreadscripts.py` | Fetch remaining web-accessible DreadScripts assets for archival reference | `using-historical-support-scripts` |
| `patch_caller_check.py` | Static IL patcher for the `GetCallingAssembly()` string-decryption guard — a RE aid for string extraction, **not** a license/DRM bypass mechanism | `using-historical-support-scripts` |

## Skills

`.claude/skills/` has project-specific Claude Code skills — read the relevant one before doing the
matching kind of work, rather than re-deriving the workflow from scratch:

**Pipeline & toolchain**
- `triaging-dotnet-reactor-obfuscation` — what de4dot does/doesn't resolve; start-of-session pipeline triage
- `regenerating-canonical-export-with-reexport` — the destructive, authoritative `export/` refresh
- `fast-iterating-with-de4dot-scorecard` — fast non-destructive iteration and correctness triage
- `resolving-reference-assemblies-for-decompilation` — `deps/unity/` vs. the full Unity install, and known dependency-noise sources
- `using-historical-support-scripts` — what the non-primary `scripts/` tools are for

**Deobfuscation & source cleanup**
- `decrypting-dotnet-reactor-smethod-strings` — resolving string-decryption calls
- `resolving-residual-control-flow-manually` — manually tracing switch/XOR dispatch de4dot leaves unresolved
- `renaming-and-documenting-deobfuscated-source` — the export/devel two-tier model and naming taxonomy
- `chunking-large-decompiled-files` — splitting/processing a file too large for one pass
- `splitting-large-classes-into-partials` — permanent partial-class-per-folder layout for a class with many nested types, one file per type with an export line-range + audit-status header

**DRM & protocol**
- `reversing-unity-license-drm-protocol` — the license protocol and the supported (`drm_server`, now in `../public`) vs. abandoned (Harmony patches) restoration approach

**Investigation tracking**
- `tracking-open-questions-with-theory-folders` — the `theory/` hypothesis-tracking workflow; applies to de4dot bug investigations and any other open DreadScripts-tool RE question

## Exporting Deobfuscated Source

### Prerequisites

```bash
# Install ilspycmd (ILSpy command-line decompiler)
dotnet tool install -g ilspycmd

# On .NET 9/10 (ilspycmd targets .NET 8), set roll-forward:
export DOTNET_ROLL_FORWARD=LatestMajor
```

### Full workflow: deobfuscate + export

```bash
# 1. Deobfuscate with de4dot (strips .NET Reactor obfuscation)
work/de4dot/de4dot binaries/ADOverhaul2022.dll -o work/deobf/ADOverhaul2022.dll

# 2. Export as C# project with ilspycmd
DOTNET_ROLL_FORWARD=LatestMajor ilspycmd -p -o export/ADOverhaul2022 work/deobf/ADOverhaul2022.dll
```

### Useful ilspycmd options

| Flag | Description |
|------|-------------|
| `-p` / `--project` | Export as compilable project (requires `-o`) |
| `-o <dir>` | Output directory |
| `-t <type>` | Decompile a specific type only |
| `-il` | Show IL code instead of C# |
| `-l c` | List all classes in assembly |
| `-l i` | List all interfaces in assembly |
| `-lv <ver>` | Target C# language version |

### Notes

- Always use `-p` for exports — it includes all types including compiler-generated anonymous types (`<>c__DisplayClass*`, iterator state machines, etc.)
- `reexport.py` passes `--nested-directories`, so output is one folder per namespace segment (`DreadScripts/ADOverhaul/`). Without that flag ilspycmd uses dot-separated folders (`DreadScripts.ADOverhaul/`) instead — older notes and exports show that layout
- The `-Module-.cs` file contains the full assembly decompilation including .NET Reactor helper classes

## Obfuscation: .NET Reactor

Both DLLs use .NET Reactor with:
- Control flow obfuscation (complex switch/case + XOR integer constants)
- Runtime string decryption via generic `smethod_N<T>(int)` or `smethod_N(int) → string`
- Anti-debugging (cosmetic only — do not gate execution branches)

Key transform: `transformed = (key * A) ^ B` (int32 overflow), then `offset = (transformed & 0x3FFFFFFF) << 2` indexes into `byte_0` (the encrypted string blob). Type tag = upper 2 bits; 0 = string.

The per-assembly A/B constants are tabulated in the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md)), and
`scripts/dotnet_reactor.py detect <Module.cs>` re-derives them from the IL, which is authoritative. Do
not paste a copy here — a stale constant silently decrypts to garbage.

> **Historical tooling (removed):** The extraction tools below lived in the `dreadre-dll-playground`
> Unity project, which has been **deleted**. No Harmony patches or runtime tooling are maintained. Their
> outputs in `dumps/` remain and are still consumed by `scripts/dotnet_reactor.py`; the descriptions are
> kept only to explain how those dumps were produced.

`Hooks.cs` (removed) extracted smethod_N strings via DynamicMethod invocation (immune to the anti-external-caller check) and Harmony-hooked WebRequest/HashAlgorithm/EditorPrefs/OpenURL calls into `dumps/intercepted.txt`.

`SmethodStringDumper.cs` (removed) performed an offline IL scan — no Harmony needed — into `dumps/{AssemblyName}.txt`.

`StringListDumper.cs` (removed) forced the static constructor of the `Test` class (ControllerEditor only) and dumped all `string[]` fields to `dumps/string_table.txt`.

## Renaming: `renames/` + `ilrename`

Names are applied to the **assembly metadata**, before ILSpy ever runs — never by find-and-replace
over decompiled text. IL references entities by metadata token, so renaming a definition makes every
use site follow it automatically: call sites, field accesses, generic arguments, base types, and the
output filename ILSpy chooses. There is no such thing as a missed call site or a false hit inside an
unrelated identifier, and no need to reason about whether `Exception` in a `catch` clause is the
obfuscator's class or the BCL one.

```
binaries/X.dll  ──de4dot──►  deobfuscated  ──ilrename renames/X.json──►  ──ilspycmd──►  export/X/
```

`scripts/reexport.py` runs all of this. `--no-rename` drops the ilrename step and exports the raw
deobfuscated assembly instead — for checking a name that may be a reflection target against ground
truth. It is a temporary state: `export/` is tracked and named, so re-run without the flag before
committing. The tree says which mode built it in `export/.renames-applied`.

### Naming in parallel, with several agents

Naming parallelises well because types are independent, and most of the 1300+ names in the maps were
produced that way. The rules exist because each was learned by breaking something:

- **`apply_renames.py set` is the only writer.** Never hand-edit a map while anything else is
  running. The lock protects the read-modify-write; two workers editing the JSON directly lose one
  another's writes and both files stay valid, so nothing detects it.
- **`scripts/` and `tools/` are frozen while agents run.** The lock protects the *data*, not the
  *code*. Editing a shared script mid-run broke it for four agents at once, and one of them started
  editing it back. Batch tool fixes between rounds; if a bug cannot wait, stop the round.
- **Give each agent a disjoint type list**, and say which types are already named nearby so it can
  reuse that vocabulary rather than invent a parallel one.
- **`reexport.py` mid-run is survivable but not free.** It rewrites `export/` under agents that are
  reading it, and can rename a file they were told to open. Warn them first. Map *names* are
  unaffected, so anything they were about to apply stays valid.
- **Tell agents to leave a member unnamed rather than guess.** A wrong name is worse than none: the
  next reader trusts it, and `--force` did not exist for most of this project's life, so wrong names
  propagated into two trees before anyone could correct them.
- **Ask for what was deliberately *not* named, and why.** That half of the report is where the real
  findings turned up — dead vendor fields, a licence-signed bug reporter, a copy-paste bug in the
  original.

### Deciding a name

The map never invents anything — every value is a human decision made by reading the code:

```bash
work/ilrename/ilrename usages --in work/deobf/ADOverhaul2022.dll \
    --entity 'DreadScripts.ADOverhaul.TokenParamsDispatcher::singletonSerializer'
```

lists every method that references the entity, with counts. Decide from that plus the body, then fill
the value into `renames/{Assembly}.json`.

### Map format

Flat JSON, `key: "NewSimpleName"`, `//` comments allowed. An empty value means "not named yet" and is
skipped, so a partly-filled map is always safe to apply. Keys look like:

```json
"DreadScripts.ADOverhaul.ExceptionSingletonStruct#0x02000060": "ADOEditorUtility"
```

The `#0x…` metadata token is what identifies the entity; the name in front is a label. de4dot's
*generated* names change between fork versions, so a name-only key silently orphans itself on the
next re-export — a token key does not, and `ilrename report` flags any key whose token no longer
matches its label. Regenerate a skeleton for a new binary with `ilrename template`.

**TypeDef tokens are stable across a de4dot change; Method and Field tokens are not.** An earlier
version of this section claimed all three were, on the strength of every token in the output also
being present in `binaries/`. That check only showed the tokens *exist* there, not that they identify
the same entity, and they do not: in ADOverhaul2022, `GUIColorScope` keeps TypeDef `0x02000088` in
both, while its `InvokeState` field is `0x040003d1` in `binaries/` and `0x040003a2` after de4dot.

The practical consequence is the one that bites: any pass that adds or removes members renumbers
every Method and Field RID after it, so a token-keyed member entry orphans itself even though the
member never moved or changed name. That is not hypothetical — removing Reactor's dead opaque
predicates renumbered both ADOverhaul maps and orphaned every member name in them at once.
`refresh_renames.py` therefore falls back to matching on the fully-qualified entity name when a token
vanishes, and only when that name is unique on both sides; anything ambiguous is still reported as
lost rather than guessed. Type-level names are unaffected either way.

Do **not** add `--preserve-tokens` or `--preserve-table all` to force stability — when they take
effect the module can no longer be rewritten by dnlib, which is what ilrename uses.

### What ilrename refuses

`apply` stops rather than produce a misleading decompile, unless `--force`:

- **Virtual/override/interface methods** — their identity is matched by name against a base type in
  another assembly, so renaming `OnInspectorGUI` detaches the override.
- **Name collisions** in the same scope, including two map entries colliding with each other.
- **Constructors** and P/Invoke methods without an explicit `EntryPoint`.

It warns but proceeds when a renamed name also appears in a string literal (possible reflection) or
in a custom-attribute named argument, since the attribute blob stores that name as plain text.

## Deobfuscated Naming Conventions

The export files still contain machine-generated class names. The naming patterns that have been imposed during cleanup:

| Suffix | Semantic role |
|--------|---------------|
| `*Thread` | Async worker / coroutine-like handler |
| `*Server` | Singleton or static service/registry |
| `*Policy` | Strategy or behavior encapsulation |
| `*Property` | Data holder / model field wrapper |
| `*Method` | Functional adapter (command/strategy pattern) |
| `*Connector` | Bridge between two subsystems |
| `*Dispatcher` | Routes messages or tokens |
| `*Adapter` | Converts between representations |
| `*Task` | One-shot async operation |
| `*Exporter` | Serializes / writes out data |
| `*Collection` | Aggregates related model objects |
| `*Struct` | Value-type singleton/exception holder |

Many internal methods are still named `smethod_N` / field names are `field_N` — these should be renamed as their purpose is identified.

## DRM Architecture

**Not documented here.** the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md)) is the single source of truth for the authentication flow, product
IDs, HMAC secrets and signing order, HWID derivation, the DSLICINF cache format and cipher, and every
EditorPrefs/SessionState key. `RE_NOTES.md` covers reconstruction status against it.

This section used to hold a partial summary. It drifted — it stated the flow in terms of class names
that had been renamed, and repeated product IDs and EditorPrefs keys that then had to be kept in step
with two other files. Link to the spec instead of summarising it.

## Working with Export Files

The `-Module-.cs` files are the full assembly decompilation — useful for searching and cross-referencing. Individual class files under each assembly's `DreadScripts.*` namespace folder are the per-type exports; the authoritative clean source lives in [`../public/unity/`](../public/unity/).

LLMs are used extensively to interpret obfuscated control flow and suggest meaningful names for `smethod_N` methods. **No Harmony runtime-patch scripts are generated or maintained** — that restoration approach was tried and explicitly abandoned; see `reversing-unity-license-drm-protocol` for the supported alternative (`drm_server`, in `../public`).
