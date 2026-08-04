# DreadScripts Unlocked

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
unity/                     # The restored Unity package (work in progress) — see below
  Assets/com.dreadscripts.unlocked/
    Editor/Common/           # types both tools shipped their own copy of
    Editor/ADOverhaul/       # ADOverHaul-only
    Editor/ControllerEditor/ # ControllerEditor-only
decompiled/                # Decompiled, deobfuscated and renamed source of the shipped DLLs
  ADOverhaul2019/          # ADOverHaul (2019 build) — reference only, not ported
  ADOverhaul2022/          # ADOverHaul (2022 build)
  ControllerEditor/        # ControllerEditor
DRM.md                     # Full reverse engineering documentation
```

### The restored package

`unity/` holds the two tools rebuilt as one package, since they shared most of their code. Every
file is re-derived from `decompiled/` by hand and carries a header naming the `decompiled/` path it
came from; nothing is a straight copy. It does not build a working tool yet — the two main product
classes have not landed.

Deliberately **not** restored, and not merely disabled: the licence/DRM validation, the supporter
window, the update check, and the remote banner. The package makes no network calls of any kind.

The port is driven from the research repo, which owns the tooling and the status:

- `python3 scripts/port_status.py` — what has landed, what is left, what is excluded and why.
  It diffs the two trees rather than reading a checklist, so it cannot go stale.
- `python3 scripts/typecheck_devel.py` — compiles the package against the reference assemblies.

---

## Legal & Ethical Context

This analysis is conducted solely to preserve access for **legitimate license holders** of discontinued products whose backend has been permanently shut down by the original developer. No circumvention is provided for products with active license enforcement.
