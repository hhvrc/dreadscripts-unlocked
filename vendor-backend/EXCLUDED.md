# Deliberately not shipped

The single list of what this package leaves out of the original plugins, and why. Tooling reads this
file; do not duplicate its rows anywhere else.

## Why this file, and not a comment in the code

A `NOT PORTED` note in a file header works only while the file exists. The moment a subsystem is
removed outright rather than partially ported, its header goes with it and the decision leaves no
trace — the next person sees a member in `decompiled/` with no counterpart and no reason, which is
indistinguishable from an oversight.

`port_status.py` cannot cover this either. It reports what is missing by diffing `export/` file
names against the package, and most of what is listed below was never a file: it was a region inside
one of the two enormous root classes. There is no stem for it to miss, so a removed region is
invisible to it in both directions.

## Format

One row per entry. `Identifier` is the name as it appears in `decompiled/` — a file stem where the
subsystem was its own file, otherwise the decompiled member or type. `Product` is `ADO`, `CE` or
`both`. Rows are the machine-readable part: keep the four columns and the pipe layout.

| Identifier | Product | Removed | Why |
|---|---|---|---|
| `BugReporter` | both | 2026-08-05 | The reporter and its help lookup. Nothing but a request to a dead endpoint, and the help lookup sends the machine fingerprint and licence key before the user sees the non-anonymity notice. See [`README.md`](README.md). |
| `ADOverhaul.Licensing` | ADO | 2026-08-05 | Licence activation, verification and transfer. Protocol in [`../DRM.md`](../DRM.md); the working replacement is [`../drm_server/`](../drm_server/). |
| `DelegateBinder` | both | never ported | Reactor's own proxy-delegate infrastructure, not vendor code. |
| `ProxyDelegateBinder` | both | never ported | As above. |
| `-Module-` | both | never ported | Obfuscator module initialiser, not vendor code. |
| `ObfuscationMarker` | both | never ported | Empty type left behind by the obfuscator. |
| `AssemblyInfo` | both | never ported | Generated assembly attributes; Unity emits its own. |
| `SemVer` | ADO | never ported | Only consumer is the online update check, which is not shipped. |
| `VersionNumber` | CE | never ported | As above. |
| `getdownloadinfo` | both | never ported | The update check. Fires on editor load, so against a dead endpoint it means a failed request and an error toast on every project load. Its `u_*` settings fields stay, because removing them would change the settings blob format. |

## Still to remove

Listed here so the gap is visible rather than forgotten. These are agreed but not yet done:

| Identifier | Product | Why |
|---|---|---|
| `ControllerEditor.Licensing` | CE | The CE half of the licence flow, matching `ADOverhaul.Licensing` above. |
| `ProcessRunner` | both | Spawns the `wmic` subprocesses that derive the hardware fingerprint. Its only remaining callers are the licence code. |
| Hardware/session/licence state fields | both | `hardwareId`, `sessionId`, `licenseKey`, `licenseToken`, `currentDateStamp` and neighbours, declared in the ported state tables. They are only written by fingerprinting code that is not shipped. |
| `SupportWindow` and its cluster | CE | `SupporterEntry`, `SupporterStrings`, `SupportWindowAssets`, `TextFragment`, `WebRequestJob` — the supporter window and the transport it drives. |

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
