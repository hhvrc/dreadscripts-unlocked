---
name: using-historical-support-scripts
description: What scripts/explore_dreadscripts.py and scripts/patch_caller_check.py are for, and which is safe to rerun vs. kept only for reference. Use before running any script in scripts/ that isn't dotnet_reactor.py, reexport.py, or de4dot_scorecard.py.
---

# Historical & Support Scripts

## `explore_dreadscripts.py` — safe to rerun

Fetches remaining web-accessible DreadScripts assets into `dreadrith_exploration.zip` +
a manifest. Standard library only, no dependencies.

```bash
python scripts/explore_dreadscripts.py
```

Use for archival/reference purposes (e.g. confirming a claim about the original product pages) —
this is read-only against public web content, safe to rerun anytime.

## `patch_caller_check.py` — reference tool, not a restoration mechanism

Statically patches the `Assembly.GetCallingAssembly() == GetExecutingAssembly()` guard that blocks
`smethod_N` string-decryption methods from being invoked by an external caller, so the decryption
body always runs. This exists to enable **dynamic string extraction** (calling the decryption
routine from outside the assembly to dump its output) — it is unrelated to license/DRM bypass.

The guard is emitted in **both branch polarities** and the patcher handles each: `brfalse` past the
body to a `return default` (neutralised by `pop`, falling through to the body), and `brtrue` to the
body with the `return default` as the fall-through (neutralised by `pop` + an unconditional `br` to
the body). The shape is chosen by where the `return default` arm sits, not by the opcode — an
earlier version matched `brfalse` only and silently found nothing on the current binaries, whose
guards are all `brtrue`. If it reports "no Assembly caller guards found", that is now a real absence,
not a shape mismatch.

**Do not confuse this with the abandoned Harmony restoration-patch approach** (see the
`reversing-unity-license-drm-protocol` skill) — that was a *runtime* patch to force a license flag
true, explicitly abandoned by project decision. This is a *static IL patch* used purely as a
string-extraction aid, kept for reference. Whether it's still needed depends on whether static
extraction (`dotnet_reactor.py`, using the transform constants directly — see
`decrypting-dotnet-reactor-smethod-strings`) already covers a given case; check there first, since
static extraction requires no binary patching at all.

## Pitfalls

- Don't treat `patch_caller_check.py` as part of the restoration story — it's a RE aid for string
  extraction, and the project's actual restoration mechanism is `drm_server` in `../public/` (see
  `reversing-unity-license-drm-protocol`).
