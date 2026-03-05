# drm_server — local DRM bypass via HTTPS spoof

A Go HTTPS server that impersonates `us-central1-dreadscripts-c6b62.cloudfunctions.net` and returns `success: true` for every DRM request from both ADOverhaul and ControllerEditor.

## Build

```
cd drm_server
go build -o drm_server.exe .
```

Or run directly:

```
go run .
```

## Setup (two steps required)

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
`certificateHandler = new AcceptAllCertificatesHandler()`.  No admin needed.

## Running

Port 443 (default) requires elevation on Windows. If you don't want to run as Administrator, use port 8443:

```
drm_server.exe serve --addr :8443
```

But note: the DLL hardcodes port 443 (standard HTTPS), so with a non-443 port you would also need to Harmony-patch the URL in the DLL.  The simplest approach is to run elevated with `patch-hosts` and `install-cert`.

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
| `activatelicense` | `success=true`, welcome message |
| `verifylicense` | `success=true` |
| `getdownloadinfo` | `success=true`, "no update available" |
| `sendfeedback` | `success=true` |
| `findsolution` | `success=true` |
| `reportbug` | `success=true` |
| `transferlicenserequest` | `success=true`, offline message |
| `transferlicense` | `success=true`, offline message |
| `transferlicenseconfirm` | `success=true`, offline message |
| _(anything else)_ | `success=true`, logged as unknown |

## How it works

1. On startup, loads the ECDSA P-256 self-signed cert from `drm-server-cert.pem` / `drm-server-key.pem` if they exist, otherwise generates a new one and saves it to those files. The cert covers both the DRM hostname and `localhost`.
2. Starts a TLS listener with that cert.
3. For every POST to `/receiveCommand`, parses the JSON body, logs `command` / product / version / HWID prefix, and returns `{"success": true, ...}`.
