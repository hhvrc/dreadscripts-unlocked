---
name: reversing-unity-license-drm-protocol
description: Reconstructs and documents the DreadScripts license-validation network protocol (HWID/SID/HMAC request format, endpoint, response handling) from decompiled Unity Editor plugin code, and explains the supported way to restore functionality (drm_server mock endpoint) versus the abandoned approach (runtime Harmony patches). Use when working on LicenseManager/TokenParamsDispatcher/WebRequestJob-equivalent classes or drm_server/.
---

# Reversing the Unity Plugin License/DRM Protocol

## When to use

- Reading or documenting the authentication flow in decompiled DRM classes (the de4dot-renamed
  equivalents of `LicenseManager`, `TokenParamsDispatcher`, `OrderImporterTask`, `WebRequestJob`).
- Extending or debugging the Go mock server (`drm_server/`, which lives in the sibling **public** repo
  `../public/drm_server/` — see AGENTS.md's "The WIP/finished split").
- Anyone proposes writing a runtime patch (Harmony hook, IL patch, in-memory bypass) to restore
  functionality — read the "Supported vs. abandoned approach" section below first.

## Supported vs. abandoned approach

**The project's restoration mechanism is `drm_server` (in `../public/`) — a local HTTPS server that impersonates the
now-dead validation endpoint and always returns success — plus documentation of the protocol.**

**Runtime restoration patches (Harmony hooks injected into the plugin, e.g. a
`dreadre-dll-playground`-style project with `ADORestorationPatch.cs`/`CERestorationPatch.cs`) were
explicitly tried and then abandoned by project decision.** They were deleted and are **not to be
recreated or restored**, even if references to them turn up in git history, old docs, or a stray
mention in an export file. If such files reappear in the working tree, that's a sign something went
wrong (e.g. a bad revert) — flag it rather than continuing to build on them.

This means: when the goal is "make the plugin work again," the answer is *"point it at `drm_server`
and document the protocol,"* not *"write a Harmony patch that forces the license check to pass in
memory."* If asked to build the latter, push back and point at this decision before proceeding.

## Protocol reconstruction workflow

1. **Find the request assembly point.** Look for the class that gathers hardware ID, session ID,
   license key, and computes a hash before building a request object — historically named something
   like `TokenParamsDispatcher` post-de4dot-renaming (names drift per de4dot run; check
   `RE_NOTES.md`'s class rename map first instead of grepping for a name that may no longer exist).
2. **Identify the fields**, typically:
   - HWID — a multi-field hardware identifier (check `RE_NOTES.md` for the exact field count/format
     already reconstructed for this product).
   - SID — a session ID, usually a fixed-length hex string cached in `EditorPrefs`.
   - License key — vendor format (e.g. Gumroad's `XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX`).
   - Hash — confirm the exact algorithm from decompiled code (don't assume; different products in
     the same codebase can use different algorithms/secrets — verify per-assembly).
3. **Find the transport layer** — usually a thin wrapper around `UnityWebRequest` POSTing JSON to a
   hardcoded Cloud Functions / API Gateway style URL. Extract the literal endpoint URL and the
   per-product ID constant sent in the payload (these are often base64-looking opaque tokens, not
   meaningful strings themselves — record them verbatim, don't try to "decode" them further).
4. **Find the response-handling logic** — what fields it expects back (`success`/similar) and what
   local state it flips (EditorPrefs keys, enable/disable flags) so `drm_server`'s canned response
   shape matches exactly what the client expects, not just "a plausible JSON blob."
5. **Cross-check local storage** — enumerate every `EditorPrefs` key touched by the flow (license
   key field, session ID, cached license-info blob) so a fresh Editor install's first-run behavior
   is understood, not just the steady-state case.
6. Update `RE_NOTES.md`'s "DRM Architecture" section with anything newly confirmed, and update
   `../public/drm_server/` if the reconstructed protocol reveals a field or endpoint behavior it doesn't yet
   emulate correctly.

## Common scenarios

**Scenario: `drm_server` returns success but the plugin still shows itself as unlicensed.**
Don't reach for a runtime patch. Instead re-check: (a) the hosts-file redirect actually resolves the
exact hostname the client requests (subdomain/casing mismatches are easy to miss), (b) the
self-signed cert is trusted by the process making the request (Unity's `UnityWebRequest` validates
TLS independently of the OS store in some configurations), (c) the response body shape matches what
the response-parsing code in the decompiled client actually expects field-for-field.

**Scenario: a second product's DRM flow looks similar but the hash algorithm/secret differs.**
Verify from that product's own decompiled code — don't assume shared secrets or algorithms across
products in the same codebase just because the request-building pattern (HWID/SID/key/hash) looks
identical. `RE_NOTES.md` documents per-product secrets separately for this reason.

## Pitfalls

- Don't add or "helpfully restore" any Harmony patch / runtime hook file — this is a settled decision,
  not an open design question.
- Don't invent protocol fields that "seem like they should be there" — every field in the
  documentation should trace back to a specific decompiled call site, not a guess based on how a
  typical license-check flow usually works.
- Treat product ID / opaque token constants as literal values to reproduce exactly, not as data to
  decode or reinterpret.
