# Vendor backend integration

What the original DreadScripts plugins talked to. This is an index: each subsystem appears here only
far enough to say what it was and where its detail lives.

What this package leaves out, and why, is [`EXCLUDED.md`](EXCLUDED.md).

## The authenticated backend — [`../DRM.md`](../DRM.md) owns this

A single Firebase Cloud Function,
`us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand`, multiplexed by a `command`
field. It is not only the licence endpoint — four distinct features shared it:

| `command` | Feature | Payload |
|---|---|---|
| `activatelicense`, `verifylicense`, `transferlicenserequest` | Licence activation, verification, transfer | Full authenticated payload: HWID, SID, licence key, HMAC |
| `getdownloadinfo` | Update check | `command`, `product_id`, `version` only — no HWID, no SID, no key, no hash |
| `reportbug`, `sendfeedback` | Bug reporter and its help lookup | Full authenticated payload |

[`../DRM.md`](../DRM.md) documents all of it: request structure (§6), HMAC construction (§7), the
endpoint and full command table (§8), response parsing (§9), the WMI hardware fingerprint (§5), the
DSLICINF local cache and its ciphers (§10–12), and captured real requests for `activatelicense`,
`transferlicenserequest`, `sendfeedback` and `getdownloadinfo` (§23).

The endpoint is permanently offline and answers with a shutdown response, which is why a
legitimately activated machine can no longer pass its own licence check.
[`../../drm_server/`](../../drm_server/) is the local stand-in that answers it.

### Two things that are not protocol facts, and so live here

**The update check identifies nothing.** `getdownloadinfo` sends only the product ID and version — no
fingerprint, no key, no hash. It is genuinely not part of the DRM and would survive a review that
only looked for the licence signature. But it fires on editor load via `[InitializeOnLoadMethod]`, so
against a dead endpoint it means a failed request and an error toast on every project load.

**The help lookup sends more than a user would expect.** The bug reporter's "search for a solution"
path issues the same authenticated request shape as a report. By the time the window's "the report is
not anonymous" notice is on screen, the lookup has already sent the machine fingerprint and licence
key. It cannot fire with no user action; it can fire without the user appreciating what the action
sent.

## The unauthenticated fetches

Different hosts, no authentication, nothing identifying. This is the part `../DRM.md` does not cover,
because it is not part of that protocol.

| Host | What | Fires when |
|---|---|---|
| `storage.googleapis.com/dreadscripts-c6b62.appspot.com/Dreadscripts/Supporters.txt` | Plain-text supporter list | The supporter window is opened |
| `raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png` | Banner atop the ADOverhaul window | The window is drawn; cached to disk |
| `i.imgur.com/FMv1R6A.png`, `i.imgur.com/iHszIY3.png` | Decorative images | As above |

## Links, which are not requests

`Application.OpenURL` targets: they open a browser on a button press and send nothing on their own.
`dreadrith.com/links`, `dreadrith.com/license-tos`, `dreadrith.com/HWIDHelp`, `ko-fi.com/dreadrith`,
`linktr.ee/Dreadrith`, `notes.sleightly.dev/ceditor`, `notes.sleightly.dev/templates/`,
`notes.sleightly.dev/controllereditor/`, the GitHub changelog, and a YouTube link.

## Why this is one file and not five

`../DRM.md` already owns the licence protocol, the update-check command, the bug-report command and
the hardware fingerprint. Giving the update check or the reporter a document of its own would mean
restating that file's §8 and §23 somewhere the two could drift apart — which is the failure this
workspace has paid for more than once. Only the unauthenticated fetches had no home, and they are
four lines.

Split a section out when it outgrows a section, not before.
