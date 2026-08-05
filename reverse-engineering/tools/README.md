# Tools

Programs the pipeline in [`../tooling/`](../tooling/) drives, and the checkers that keep the port
honest.

| | |
|---|---|
| `IlRename/` | Applies [`../renames/`](../renames/) to assembly metadata before decompilation. This is the piece that makes naming reproducible: one decision updates the declaration, every call site and the filename ILSpy picks, and the decompiled source stays a regenerable artefact rather than something hand-edited. |
| `SourceAnalysis/` | Roslyn-based analysis over the decompiled tree, used to answer structural questions the text alone cannot. |
| `check-headers.py` | Validates every ported file's provenance header against the format, and confirms no two files claim the same decompiled member. |
| `check-audit-freshness.py` | Whether a `VERIFIED` audit stamp is still true, distinguishing comment edits from code edits. |
| `migrate-headers.py` | One-off migration of older header spellings into the current format. |

The two C# projects build with `dotnet build`; their output is gitignored.

The Python checkers anchor on their own location, so they can be run from any directory.

## Licence

Everything in this directory is **GPL-3.0**, not the MIT licence the rest of the repository uses.
See [`LICENSE`](LICENSE) beside it, and [`../tooling/README.md`](../tooling/README.md) for why the
split exists.

Copyright (C) 2026 HeavenVR.
