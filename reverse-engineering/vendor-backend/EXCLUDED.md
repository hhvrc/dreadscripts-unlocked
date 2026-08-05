# Deliberately not shipped

The single list of what this package leaves out of the original plugins, and why. Tooling reads this
file; do not duplicate its rows anywhere else.

## Why this file, and not a comment in the code

A `NOT PORTED` note in a file header works only while the file exists. The moment a subsystem is
removed outright rather than partially ported, its header goes with it and the decision leaves no
trace — the next person sees a member in `reverse-engineering/export/` with no counterpart and no reason, which is
indistinguishable from an oversight.

`port_status.py` cannot cover this either. It reports what is missing by diffing `export/` file
names against the package, and most of what is listed below was never a file: it was a region inside
one of the two enormous root classes. There is no stem for it to miss, so a removed region is
invisible to it in both directions.

## Format

One row per entry. `Identifier` is the name as it appears in `reverse-engineering/export/` — a file stem where the
subsystem was its own file, otherwise the decompiled member or type. `Product` is `ADO`, `CE` or
`both`. Rows are the machine-readable part: keep the four columns and the pipe layout.

| Identifier | Product | Removed | Why |
|---|---|---|---|
| `BugReporter` | both | 2026-08-05 | The reporter and its help lookup. Nothing but a request to a dead endpoint, and the help lookup sends the machine fingerprint and licence key before the user sees the non-anonymity notice. See [`README.md`](README.md). |
| `ADOverhaul.Licensing` | ADO | 2026-08-05 | Licence activation, verification and transfer. Protocol in [`../DRM.md`](../DRM.md); the working replacement is [`../../drm_server/`](../../drm_server/). |
| `DelegateBinder` | both | never ported | Reactor's own proxy-delegate infrastructure, not vendor code. |
| `ProxyDelegateBinder` | both | never ported | As above. |
| `-Module-` | both | never ported | Obfuscator module initialiser, not vendor code. |
| `ObfuscationMarker` | both | never ported | Empty type left behind by the obfuscator. |
| `AssemblyInfo` | both | never ported | Generated assembly attributes; Unity emits its own. |
| `SemVer` | ADO | never ported | Only consumer is the online update check, which is not shipped. |
| `VersionNumber` | CE | never ported | As above. |
| `getdownloadinfo` | both | never ported | The update check. Fires on editor load, so against a dead endpoint it means a failed request and an error toast on every project load. Its `u_*` settings fields stay, because removing them would change the settings blob format. |

Everything above and below was removed in one pass on 2026-08-05, and the package compiled at every
step: the licence code had no callers outside itself, which is the clearest evidence that the
restoration never depended on it.

| Identifier | Product | Removed | Why |
|---|---|---|---|
| `ControllerEditor.Licensing` | CE | 2026-08-05 | The CE half of the licence flow, matching `ADOverhaul.Licensing`. |
| `ProcessRunner` | both | 2026-08-05 | Spawned the `wmic` subprocesses that derive the hardware fingerprint. Its only callers were the licence code and the bug reporter. |
| The `Licensing` field regions | both | 2026-08-05 | 28 fields in `ControllerEditor.State.cs` and 26 in `ADOverhaul.State.cs` — `hardwareId`, `sessionId`, `licenseKey`, `licenseToken`, `currentDateStamp` and neighbours. Originally declared-but-unwritten so that later ports of the regions reading them would have names to agree on; once the readers were removed, a declared field was a stub rather than a seam. Recorded in both headers as `NOT PORTED` entries. |
| `SupportWindow` | CE | 2026-08-05 | The supporter window: fetched a supporter list over the network at editor time. |
| `SupporterEntry`, `SupporterStrings`, `SupportWindowAssets`, `TextFragment` | CE | 2026-08-05 | Existed only to serve the supporter window. |
| `WebRequestJob` | CE | 2026-08-05 | The polling HTTP transport the supporter window drove. No other caller. |

## Not excluded, deliberately

Recorded because each looks like a candidate and is not.

- **Remote image fetching** — `RemoteTexture`, `RemoteTextureView`, the ADOverhaul banner downloader.
  Static public assets, no identifying data. Kept for now; see [`README.md`](README.md).
- **`Application.OpenURL` links** — they send nothing on their own and only open on a button press.
- **The two root classes** — `ADOverhaul` and `ControllerEditor` each keep licence validation inline
  among the product's real code. They are stripped during the port, never skipped; excluding either
  file would drop that whole tool.
- **`ADOverhaul2019`** — reference only, for cross-checking a body the 2022 decompile mangled. Never
  ported, but not excluded by policy either.
