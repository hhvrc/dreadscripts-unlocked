# Tooling

The pipeline that produced everything else here: deobfuscate the shipped assemblies, apply the
chosen names, decompile, then check the restored package against the result.

## Running it

Everything anchors relative to this folder, so the scripts work from a checkout with no
configuration — except for one thing they cannot ship.

**The reference assemblies are not in this repository.** They are Unity's and VRChat's
redistributables. [`../reference-assemblies.json`](../reference-assemblies.json) identifies every
one by path, size and SHA-256 and names the exact build it came from; reassemble the set into
`../deps/unity/` and `../deps/vrchat/` and the pipeline runs unchanged. Anything that needs them
says so and points here rather than failing obscurely.

## The main entry points

| | |
|---|---|
| `reexport.py` | Rebuild `../export/` from `../binaries/` — deobfuscate, apply `../renames/`, decompile. The canonical regeneration; everything else diffs against its output. |
| `port_status.py` | What is ported, what is left, what is excluded and why. Derived by diffing the two trees, so it cannot go stale. |
| `typecheck_package.py` | Compile the restored package against the reference set. A type check, not a correctness check. |
| `apply_renames.py` | The only writer for `../renames/`. Holds a lock; refuses to overwrite a name someone already chose. |
| `harvest_ported_names.py` | Recover names that were decided during porting and only ever recorded in a ported file's header. |
| `de4dot_scorecard.py` | Deobfuscate → decompile → verify → triage, in one command, against a throwaway output. |

The rest are narrower: string decryption (`dotnet_reactor.py`), control-flow tracing
(`trace_original_machine.py`), closure and decoy measurement, rename bookkeeping.

## What is missing compared to the research checkout

`sync_public.py` is gone. It enforced a boundary between a work-in-progress repository and this
published one, moving finished artefacts across and failing if anything reappeared on the wrong
side. With the research and the deliverable in one repository that boundary does not exist, so the
script would be answering a question nobody can ask. Retiring it beat repointing it at paths that
would have made it quietly meaningless.

## Licence

Everything in this directory is **GPL-3.0**, not the MIT licence the rest of the repository uses.
See [`LICENSE`](LICENSE) beside it.

The split is deliberate. This pipeline is wholly this project's work, so unlike the restored
package it is ours to license, and GPL-3.0 keeps derivatives of it open.

Copyright (C) 2026 HeavenVR.
