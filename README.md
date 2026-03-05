# dreadrith-drm-re

Reverse engineering of the DRM system used by two discontinued [DreadScripts](https://dreadscripts.com) Unity Editor plugins. The backend validation server (`us-central1-dreadscripts-c6b62.cloudfunctions.net`) is permanently offline, making legitimately purchased products non-functional.

**Goal:** Document the DRM, restore functionality for legitimate license holders.

---

## Affected Products

| Product | Package | Product ID |
|---|---|---|
| **ADOverHaul** | `com.dreadscripts.avatardynamicsoverhaul` | `No1lKII9IzcBAbihub6nCg==` |
| **ControllerEditor** | `com.dreadscripts.controllereditor` | `yOk0XCnENLMO6DIF8cYpSg==` |

Both products share the same auth endpoint, DSLICINF cache format, and session ID EditorPrefs key (`DreadScriptssid`).

---

## Restoration

`drm_server/` is a Go HTTPS server that intercepts the validation endpoint and returns a successful license response. Requires patching the hosts file and installing the self-signed cert.

See **[drm_server/README.md](drm_server/README.md)** for step-by-step instructions.

---

## Documentation

Full reverse engineering notes are in [`DRM.md`](DRM.md).

---

## Repository Layout

```
drm_server/                # Go HTTPS server: intercepts DRM endpoint, returns licensed
DRM.md                     # Full reverse engineering documentation
```

---

## Legal & Ethical Context

This analysis is conducted solely to preserve access for **legitimate license holders** of discontinued products whose backend has been permanently shut down by the original developer. No circumvention is provided for products with active license enforcement.
