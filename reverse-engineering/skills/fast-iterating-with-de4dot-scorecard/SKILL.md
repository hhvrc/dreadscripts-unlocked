---
name: fast-iterating-with-de4dot-scorecard
description: scripts/de4dot_scorecard.py automates the deobfuscate, ilverify, decompile, and marker-triage pipeline against a throwaway /tmp output, for fast non-destructive iteration when testing a de4dot build or checking a sample's current state. Its `gates` subcommand is the acceptance check for any de4dot change — every gate in one run, with a diffable JSON baseline. Use instead of retyping the manual pipeline, instead of writing an ad-hoc measurement script, and instead of the destructive reexport.py when only a quick read is needed.
---

# Fast Iteration with `de4dot_scorecard.py`

## When to use

- **Accepting or rejecting a de4dot change** — run `gates --all --json` before and after, and diff.
  This is the check; do not hand-assemble one.
- Testing whether a de4dot fork change actually improves things, without committing to a full,
  destructive `export/` regeneration (see `regenerating-canonical-export-with-reexport` for that).
- Checking a sample's current `ilverify` correctness signal or `smethod_N`/`goto`/`TODO` marker
  counts without re-typing the manual multi-step pipeline (PATH setup for ilspycmd/ilverify, the
  `OPENSSL_ENABLE_SHA1_SIGNATURES` env var, the long Unity reference-assembly flag list) every time.
- Investigating a de4dot bug theory (see `tracking-open-questions-with-theory-folders`) against a
  real sample and needing a quick, repeatable way to reproduce the same ilverify errors.

## What it does

```bash
python scripts/de4dot_scorecard.py full ADOverhaul2022                    # one sample
python scripts/de4dot_scorecard.py full --all                            # all three samples
python scripts/de4dot_scorecard.py full ADOverhaul2022 \
    --de4dot-dll ../de4dot/Release/net8.0/linux-x64/de4dot.dll           # a fresh fork build
python scripts/de4dot_scorecard.py markers                                # just the marker triage
python scripts/de4dot_scorecard.py verify /some/deobf.dll --original binaries/X.dll
```

`full` runs, per sample: de4dot deobfuscate → `ilverify` the result (categorized: total errors,
known-third-party-dependency noise, and target-internal `DreadScripts.*` errors — the ones worth
looking at) → `ilspycmd` decompile → marker triage (`smethod_N` call count, `goto` count,
`TODO`/stub count) on the fresh decompile. All output goes to `--out` (default `/tmp/de4dot_scorecard`)
— nothing under `export/` or the restored package in `../public/unity/` is touched.

## `gates` — the acceptance check

```bash
python scripts/de4dot_scorecard.py gates --all --json /tmp/before.json \
    --de4dot-dll ../de4dot/Release/net10.0/linux-x64/de4dot.dll
# ... make the de4dot change, rebuild ...
python scripts/de4dot_scorecard.py gates --all --json /tmp/after.json \
    --de4dot-dll ../de4dot/Release/net10.0/linux-x64/de4dot.dll
```

Per sample it reports gate 1 (`ilverify`, target-internal errors and unclassified diagnostics),
gate 5 (state-machine termination, read back out of de4dot's own run, plus how many dispatch
resolutions were rejected), gate 6 (metadata round-trip), gate 7 (residual `smethod_N` against that
assembly's own ceiling), and metadata counts against the original. It **exits non-zero** if any gate
fails, so it works as a check and not only as a report.

Three things about it that are deliberate and worth not undoing:

- **A missing measurement is a FAIL, never a zero.** If de4dot's state-machine summary line is absent
  from the output, the gate reports FAIL and says so, because an absent line means the trace did not
  run or its wording changed — neither of which is evidence that nothing loops. Four confident green
  results in this project came from reading an absence as a clean result.
- **Baselines live in `pipeline.py`** (`STATE_MACHINE_BASELINE`, `DECRYPT_BASELINE`), next to the
  gate, so a ceiling cannot drift away from what it constrains.
- **Gate 7 is per assembly, not aggregate.** An aggregate ceiling hides one sample tripling behind
  another falling to zero.

**Do not write your own gate runner.** If a gate is missing, add it here — a measurement kept in a
scratch directory cannot be diffed against next session, which is the whole point of having one.
The same goes for any adjacent workflow you find yourself doing by hand more than once: extend this
script (or whichever of `de4dot_lab.py` / `reexport.py` / `tools/IlRename` owns the closest job),
commit it, and list it in AGENTS.md's tool table so the next session finds it.

This is not a ban on scratch scripts. A snippet that answers "what does *this one* method do" —
hand-simulating a state machine, decoding a constant — is fine to leave in scratch, because once you
know the answer you write it down and nobody re-runs it. The line is whether a later session would
have to run it again to know whether something regressed. See the "Scripts" section of `AGENTS.md`
for the rule in full.

**This is a fast triage tool, not the full rigorous correctness methodology** — it does NOT do
method-token correlation between an original and deobfuscated run (their symbol names are unrelated,
so a text diff between error lists would be meaningless). It tells you *where to look* — see
de4dot's `measuring-deobfuscation-correctness-with-ilverify` skill for the actual verdict methodology
once you've narrowed down a specific method to investigate.

## Defaults and overrides

- `--de4dot-dll` defaults to `work/de4dot/de4dot` (the locally-built native binary this project's
  pipeline normally uses). Point it at a fresh `../de4dot` fork build to test a change before it's
  copied into `work/de4dot/`.
- `--unity-managed` / `--mono45` default to this environment's actual Unity Editor install path (see
  the `resolving-reference-assemblies-for-decompilation` skill) — override if running somewhere else.
- Requires `ilspycmd` and `ilverify` as dotnet global tools; the script checks `PATH` and falls back
  to `~/.dotnet/tools/` automatically, so you don't need to fix `PATH` yourself first.

## Reading the report

```
=== ilverify: ADOverhaul2022 (deobfuscated) ===
  total errors:            457
  known-dependency noise:  447  (VRC*/netstandard — expected, ignore)
  TARGET-INTERNAL (DreadScripts.*): 10  <- look here first
  by error kind: FileLoadErrorGeneric=447, MissingMethod=1, StackUnexpected=9
```

Only the target-internal count and its listed error lines are worth reading closely — the noise
count is the VRChat SDK / netstandard dependency gap documented in the reference-assemblies skill,
expected every time regardless of de4dot's correctness.

## Common scenarios

**Scenario: verifying a de4dot theory's repro still reproduces after a fork rebuild.** Run
`full <sample> --de4dot-dll <path to the rebuilt de4dot.dll>` and compare the target-internal error
lines against what the theory file's evidence log recorded — same lines still present means the
theory's repro is still valid; a shrunk or changed set means something changed and the theory file
should get a new dated evidence-log entry either way (fixed or changed shape).

**Scenario: `full --all` is too slow for a quick check.** Run against a single named sample instead
— `ControllerEditor` in particular can be slow through de4dot's CFG cleaning on some builds (see
`triaging-dotnet-reactor-obfuscation`); don't default to `--all` if you only need to check one thing.

## Pitfalls

- Don't treat the target-internal count as a final correctness verdict — it's a starting point for
  manual inspection, not a substitute for the full methodology when something is about to be reported
  as fixed.
- Don't forget `--out` results are throwaway (`/tmp` by default) — nothing here updates `export/`;
  use `reexport.py` when you actually mean to refresh the tracked canonical decompiles.
- If the noise categorization ever looks wrong (a known third-party dependency showing up as
  "target-internal"), check `KNOWN_NOISE_PREFIXES` in the script — the noise detection is prefix-based
  and only knows about `VRC*`/`netstandard` today; a new missing dependency needs adding there.
