# drm_server — local DRM bypass via HTTPS spoof

A Go HTTPS server that impersonates `us-central1-dreadscripts-c6b62.cloudfunctions.net` and returns `success: true` for every DRM request from both ADOverhaul and ControllerEditor.

## Build

```
cd drm_server
go build -o drm_server.exe .
```

Or run directly:

```
go run . serve
```

## Quick install (Windows, recommended)

From an elevated PowerShell prompt in this folder, after building `drm_server.exe`:

```
.\install.ps1
```

This copies the binary to `C:\ProgramData\DreadScriptsDRM\`, patches the hosts file, installs the
self-signed cert, and registers + starts a Windows auto-start service — one command, no manual
steps. To remove everything:

```
.\uninstall.ps1
```

(The cert stays in the Windows Root store afterward — remove it manually via **certmgr.msc** →
Trusted Root Certification Authorities → "DreadScripts DRM Server" → Delete, if desired.)

## Manual setup (two steps required)

### 1. Redirect DNS to localhost

Add to `C:\Windows\System32\drivers\etc\hosts` **as Administrator**:

```
127.0.0.1  us-central1-dreadscripts-c6b62.cloudfunctions.net
```

Or let the server do it automatically (requires an elevated prompt):

```
drm_server.exe patch-hosts
```

To undo: remove the line from the hosts file.

### 2. Trust the self-signed certificate

`UnityWebRequest` validates TLS by default and will reject an untrusted self-signed cert.
Choose one approach:

#### Option A — Install cert into Windows trust store (easiest, permanent)

```
drm_server.exe install-cert
```

This calls `certutil -addstore -f Root <cert>` internally.  Requires an elevated prompt.
To uninstall, open **certmgr.msc** → Trusted Root Certification Authorities → find "DreadScripts DRM Server" → delete.

#### Option B — Export cert, import manually

```
drm_server.exe export-cert cert.pem
certutil -addstore -f Root cert.pem
```

#### Option C — Harmony-patch UnityWebRequest to skip cert validation

Add a prefix patch to `UnityWebRequest.SendWebRequest` that sets
`certificateHandler = new AcceptAllCertificatesHandler()`. No admin needed, but requires a Harmony
mod loader in the Unity Editor process.

## Running

```
drm_server.exe serve
```

Port 443 (default) requires elevation on Windows. If you don't want to run as Administrator, use port 8443:

```
drm_server.exe serve --addr :8443
```

But note: the DLL hardcodes port 443 (standard HTTPS), so a non-443 port only works if you also
Harmony-patch the URL in the DLL, or redirect 443 → your port at the network/proxy layer. The
simplest approach is to run elevated with `patch-hosts` + `install-cert` (or just `install.ps1`).

### Activating in Unity, once the server is up

Open the plugin's license window and enter any license key matching the expected format
(`XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX`, hex), then click Activate. The server returns success, and the
DLL runs its normal activation path — which writes a valid DSLICINF cache into `EditorPrefs`.

That cache is keyed on the current date, so **it satisfies startup checks for the rest of the day and
the plugin then re-checks the next day.** Leaving the server installed as a service
(`install-service`, or `install.ps1`, which does it for you) is what makes that invisible; otherwise
activate again whenever the plugin reports itself unlicensed.
[`DRM.md`](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/DRM.md) § "DSLICINF — Local License
Cache" has the cache format and validation rules. (Absolute link on purpose: this file is published
into the public repo alongside `DRM.md`, but lives one directory deeper here.)

### All subcommands

| Subcommand | Description |
|---|---|
| `serve [--addr :443]` | Start the DRM HTTPS server (default port 443, requires admin) |
| `install-cert` | Install self-signed cert into Windows Root store via certutil (requires admin) |
| `export-cert <file>` | Write the self-signed cert PEM to a file |
| `patch-hosts` | Append DRM hostname → 127.0.0.1 to the system hosts file (requires admin) |
| `install-service` | Register as a Windows auto-start service (requires admin) |
| `uninstall-service` | Stop and remove the Windows service (requires admin) |
| `start-service` | Ask the SCM to start the service (requires admin) |
| `stop-service` | Ask the SCM to stop the service (requires admin) |

## What the server handles

| `command` | Response |
|---|---|
| `activatelicense` | `success=true`, `message="License verified."`, plus `date` / `username` / `variant` / HMAC `token` for the DSLICINF cache |
| `verifylicense` | Same as `activatelicense` — both call the identical grant path |
| `getdownloadinfo` | `success=true`, `version` echoed back so the plugin sees itself as up to date; no download or announcement offered |
| `sendfeedback` | `success=true`, feedback logged to the console, confirmation `message` echoed to the Unity console |
| `findsolution` | `success=true` with no `solution`, which puts the bug reporter on its "no solution found" branch |
| `reportbug` | `success=true`, report logged to the console |
| `transferlicenserequest` | `success=true` plus a `transfer_email` placeholder, and a `message` saying no code was mailed |
| `transferlicenseconfirm` | `success=true` if `verification_code` is 6 alphanumerics (the plugin's own rule), otherwise `success=false` with an explanation |
| _(anything else)_ | `success=true`, logged as unrecognised |

These eight are the complete set the plugins send to this host. The request and response fields
of each are documented inline in `handler.go`; the protocol itself is specified in
[`../DRM.md`](../DRM.md).

## How it works

1. On startup, loads the ECDSA P-256 self-signed cert from `drm-server-cert.pem` / `drm-server-key.pem`
   in the working directory if present, otherwise generates a fresh one (10-year validity, covering
   the DRM hostname + `localhost` + `127.0.0.1`) and saves it there for reuse on subsequent runs —
   so an installed trust-store entry stays valid across restarts.
2. Starts a TLS listener with that cert.
3. For every POST to `/receiveCommand`, parses the JSON body, logs `command` / product / version / HWID prefix, and returns `{"success": true, ...}`.
4. Can optionally run as a Windows service (`install-service`) so it survives reboots without a login session.
