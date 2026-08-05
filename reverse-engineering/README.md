# Reverse engineering

How the two discontinued DreadScripts plugins were taken apart, and everything that came out of it.
The tool this produced is in [`../unity/`](../unity/) — start at the [root README](../README.md) if
you came here to use it rather than to read how it was made.

**This work is finished.** Both products are fully ported and the handoff is done: the project is a
Unity tool now, maintained and improved like any other, and the reverse engineering is not an
ongoing effort. What is here is the record of how it was done and the means to check it — the
pipeline still runs, so any claim below can be re-derived from the binaries rather than believed.

It is kept for three reasons: the port is only trustworthy if the thing it was derived from is
available to compare against; the notes explain *why* a member is named or shaped the way it is,
which is the first question anyone changing that code will have; and the open questions in
[`theory/`](theory/) are honest about what was never settled, rather than quietly dropped.

## The two products

| Product | Original package | Product ID |
|---|---|---|
| Avatar Dynamics Overhaul | `com.dreadscripts.avatardynamicsoverhaul` | `No1lKII9IzcBAbihub6nCg==` |
| Controller Editor | `com.dreadscripts.controllereditor` | `yOk0XCnENLMO6DIF8cYpSg==` |

They shipped separately and were obfuscated separately, but share an auth endpoint, a licence cache
format, a session-ID key (`DreadScriptssid`) and most of their code — which is why the restoration
is one package and why the 2019 and 2022 ADOverhaul builds are useful against each other: the same
method decompiled twice, and rarely badly in both.

The backend that validated them,
`us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand`, is permanently offline.
[`DRM.md`](DRM.md) specifies the protocol it spoke, [`vendor-backend/`](vendor-backend/) indexes
everything the plugins talked to, and [`../drm_server/`](../drm_server/) answers it locally for
anyone running the original assemblies.

## What is here

| Folder | What it is |
|---|---|
| `export/` | The deobfuscated, renamed source of the shipped assemblies. Every ported file names the `export/` path it came from and is diffed against it. |
| `binaries/` | The shipped assemblies themselves, via Git LFS — so every claim here can be re-derived rather than believed. |
| `renames/` | The obfuscated→English member maps. Data the pipeline acts on, not documentation: they are applied to the assembly metadata *before* decompilation, so every call site and file name follows automatically. |
| `tools/` | `IlRename`, which applies those maps, `SourceAnalysis`, and the header/audit checkers that keep the port honest. |
| `tooling/` | The pipeline — deobfuscate, apply names, decompile, typecheck, diff — plus the port-status reporter. |
| `notes/` | The living record — why each name was chosen, what has been verified, what is still open, and the working conventions the port follows. |
| `theory/` | Open investigations tracked as falsifiable hypotheses, so evidence and dead ends survive across sessions instead of being re-derived. |
| `skills/` | The methodology, written as procedures: how to triage a .NET Reactor assembly, decrypt its strings, trace control flow the deobfuscator could not resolve, split a monolithic class, name a member from evidence. |
| `dumps/` | Extracted string tables — the decrypted output of the obfuscator's string routines, kept as evidence for claims made in the notes. |

## Nothing here is a snapshot to be read once

`export/` is not an archive — it is regenerated from `binaries/` whenever `renames/` changes, and
the ported package is checked against the result continuously. That loop is the method:

    binaries/  --deobfuscate-->  --apply renames/-->  --decompile-->  export/  --diff-->  unity/

So a name is never edited into the decompiled source. It goes into `renames/`, gets applied to the
assembly metadata by `tools/IlRename` before ILSpy runs, and every call site and file name follows
from that automatically. `export/` is downstream of the maps, which is why hand-editing it would be
overwritten by the next regeneration.

## Binaries

`binaries/` holds the four shipped assemblies the whole analysis derives from — both ADOverhaul
builds, ControllerEditor, and the Harmony library they load. Stored with Git LFS.

They are here so the work can be checked rather than taken on trust. Every claim in the notes, every
rename and every line of the restored package traces back to these files, and none of it is
independently verifiable without them — a reverse-engineering write-up whose subject is missing is
just an assertion.

## Reference assemblies — identified, not redistributed

The pipeline also needs Unity's and VRChat's own managed assemblies to decompile against and to
typecheck the restored package. Those are third-party redistributables and are **not** checked in.

[`reference-assemblies.json`](reference-assemblies.json) identifies them instead: every file's path,
size and SHA-256, grouped by where it came from, with the exact product version beside it —

- **Unity Editor 2022.3.22f1** (revision `887be4894c44`), 95 files from `Data/Managed/`
- **VRChat `com.vrchat.base` 3.10.4**, 11 files

That is enough to reassemble the identical set and prove it: install the named version, copy the
listed paths, check the hashes. It is also more useful than the DLLs would have been, because it
pins *which* build the analysis was done against — a detail that silently matters when a decompile
resolves a type differently between Unity versions.

With `binaries/` present and the reference set reassembled, `tooling/` runs end to end: deobfuscate,
apply the rename maps, decompile, typecheck the restored package, diff the result.
