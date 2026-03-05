# DreadScripts DRM — Comprehensive Reverse Engineering Notes

> **Products:** ADOverhaul (2019 + 2022 builds), ControllerEditor  
> **Author:** DreadScripts  
> **Backend:** Google Firebase Cloud Function (permanently offline as of 2024)  
> **Goal:** Document the license verification system to restore functionality for legitimate license holders.

---

## Table of Contents

1. [Overview](#1-overview)
2. [DRM Entry Points](#2-drm-entry-points)
3. [License Key Format and Storage](#3-license-key-format-and-storage)
4. [Session ID (SID)](#4-session-id-sid)
5. [Hardware Fingerprint (HWID)](#5-hardware-fingerprint-hwid)
6. [HTTP Request Structure](#6-http-request-structure)
7. [HMAC Signing](#7-hmac-signing)
8. [Server Endpoint and Commands](#8-server-endpoint-and-commands)
9. [Server Response Parsing](#9-server-response-parsing)
10. [DSLICINF — Local License Cache](#10-dslicinf--local-license-cache)
11. [AES Cipher Details](#11-aes-cipher-details)
12. [DSLICINF Encryption Pipeline](#12-dslicinf-encryption-pipeline)
13. [Full Verification Flow — Step by Step](#13-full-verification-flow--step-by-step)
14. [License State Fields](#14-license-state-fields)
15. [EditorPrefs and SessionState Keys](#15-editorprefs-and-sessionstate-keys)
16. [Rate Limiting and Anti-Abuse](#17-rate-limiting-and-anti-abuse)
17. [License Transfer / 2FA Flow](#18-license-transfer--2fa-flow)
18. [Product IDs](#18-product-ids)
19. [Per-Product Differences: ADOverhaul vs ControllerEditor](#19-per-product-differences-adoverhaul-vs-controllereditor)
20. [ADOverhaul 2019 vs 2022 Differences](#20-adoverhaul-2019-vs-2022-differences)
21. [Obfuscation (.NET Reactor)](#21-obfuscation-net-reactor)
22. [Restoration / Bypass](#22-restoration--bypass)
23. [Captured Real Requests](#23-captured-real-requests)

---

## 1. Overview

Both plugins share an identical DRM architecture, implemented independently in each assembly. The DRM is a client-side license verification system with:

- A **Gumroad-format license key** entered by the user
- A **hardware fingerprint (HWID)** computed from WMI hardware identifiers
- A **session ID (SID)** persisted in EditorPrefs
- A **signed HTTP POST** to a Firebase Cloud Function
- A **daily AES-encrypted local cache (DSLICINF)** that avoids server round-trips when valid

The backend endpoint `https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand` is permanently offline. All attempts to activate now receive a "shutdown" response with `success=false`.

---

## 2. DRM Entry Points

### ADOverhaul 2022

- **DRM class:** `IdentifierSerializerConnector` (de4dot name); renamed `LicenseManager` in clean source
- **Entry:** `[InitializeOnLoadMethod] DisableConfiguration()` — fires on every Unity domain reload
  - Reads the license key from `EditorPrefs["No1lKII9IzcBAbihub6nCg==LK"]`
  - If key is present and `a_VerifyOnProjectLoad` setting is true → schedules `AssetConfiguration(false)`
- **Secondary entry:** `VisitConfiguration()` — fires on every repaint of the DRM window when `a_VerifyOnDisplay` is true
- **Primary gate method:** `MoveConfiguration()` — returns `_Service` and logs an error if not licensed

```csharp
[InitializeOnLoadMethod]
private static void DisableConfiguration()
{
    bool flag = RateConfiguration(); // reads key from EditorPrefs
    if (!RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification)
    {
        _Worker = true;
        m_Pool = flag;
    }
    if (flag && RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad)
    {
        ExceptionSingletonStruct.AddProcess(AssetConfiguration_deferred);
    }
}
```

### ControllerEditor

- **DRM class:** Main `ControllerEditor` class itself (no separate DRM class)
- **Entry:** `[InitializeOnLoadMethod] VerifyAnnotation()` — fires on every domain reload
  - Calls `ResolveAnnotation()` (installs any Harmony patches needed) then `AssetAnnotation()` (loads key)
  - Checks `ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification` for cache hit
  - Sets `m_DispatcherAnnotation = true` if cache hits

### ADOverhaul 2019

- **DRM class:** `ConfigurationTestStub` (de4dot name) — identical architecture to 2022
- Uses `HttpWebRequest` instead of `UnityWebRequest` for the HTTP call

---

## 3. License Key Format and Storage

### Format

```
^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$
```

Example: `<YOUR_LICENSE_KEY>`  
This is the standard Gumroad license key format (32 uppercase hex chars, 4 groups of 8, dash-separated).

### Storage

| Product | EditorPrefs key | In-memory field |
|---|---|---|
| ADOverhaul | `"No1lKII9IzcBAbihub6nCg==LK"` | `m_Repository` (static string) |
| ControllerEditor | `"yOk0XCnENLMO6DIF8cYpSg==LK"` | `m_BridgeAnnotation` (static string) |

There is also a secondary EditorPrefs key used by the GUI text field (but not the activation logic):

| Product | GUI backup key |
|---|---|
| ADOverhaul | `"ADOverhaulLicenseField"` |
| ControllerEditor | `"Controller EditorLicenseField"` |

Validation is done before sending:

```csharp
private static bool AddConfiguration()
{
    return Regex.Match(m_Repository,
        "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
}
```

---

## 4. Session ID (SID)

The SID is a 32-character lowercase hexadecimal string, shared between all DreadScripts products on the same machine. It is generated once and persisted.

**EditorPrefs key:** `"DreadScriptssid"` (both products)  
**Format regex:** `[0-9a-f]{32}`

**Generation logic:**

```csharp
private static void ManageConfiguration()
{
    if (string.IsNullOrWhiteSpace(global))
    {
        string text = "DreadScriptssid";
        global = EditorPrefs.GetString(text, string.Empty);
        if (string.IsNullOrWhiteSpace(global) || !Regex.IsMatch(global, "[0-9a-f]{32}"))
        {
            global = GUID.Generate().ToString(); // Unity's GUID, produces lowercase hex without dashes
            EditorPrefs.SetString(text, global);
        }
    }
}
```

Real captured value: `"<YOUR_SID>"`

---

## 5. Hardware Fingerprint (HWID)

The HWID is a 3-segment string, each segment being a 40-character uppercase SHA1 hash. The segments are joined with `-`.

**Format:** `<SHA1_A>-<SHA1_B>-<SHA1_C>`  
**Example:** `<HWID_SEGMENT_1>-<HWID_SEGMENT_2>-<HWID_SEGMENT_3>`

### WMI Queries Used

Four WMI categories are queried in parallel via `wmic` (primary, Windows CMD) with a `Get-CimInstance` (PowerShell) fallback:

| Segment | WMI class | Fields extracted |
|---|---|---|
| A (baseboard) | `Win32_baseboard` | `Manufacturer`, `Product`, `SerialNumber` |
| B (CPU) | `Win32_processor` | `ProcessorId` |
| C (disk) | `Win32_diskdrive` | `SerialNumber` |
| — (memory) | `Win32_physicalmemory` | `Manufacturer`, `PartNumber`, `SerialNumber`, `Capacity` |

> **Note:** The memory category fields are collected but — based on the 3-segment HWID structure — the final HWID only uses 3 SHA1 segments (baseboard, CPU, disk). The exact field concatenation order within each segment is determined by the closure callbacks (`FindWatcher`/`AddWatcher`/`ValidateWatcher` in ADO, `NewObserver`/`PushObserver`/`ViewObserver` in CE).

**Commands issued:**

```
wmic baseboard get *
wmic cpu get *
wmic diskdrive get *
wmic memorychip get *
```

Fallback:
```
Get-CimInstance -class Win32_baseboard | Select *
Get-CimInstance -class Win32_processor | Select *
Get-CimInstance -class Win32_diskdrive | Select *
Get-CimInstance -class win32_physicalmemory | Select *
```

The HWID collection runs asynchronously with a 10-second `CancellationTokenSource` timeout.

### Scrambled HWID (`_Interpreter`)

After HWID assembly, a **transport-scrambled** version is computed for display purposes (`RestartConfiguration`):

```csharp
private static void RestartConfiguration()
{
    string[] hwid  = attr.Split('-');                     // ["DDXX...", "56BX...", "EE7C..."]
    string[] date  = RemoveConfiguration().Split('/');    // ["DD", "MM", "YYYY"]
    date[2] = date[2].Substring(2, 2);                   // last 2 digits of year
    _Interpreter = date[2]
                 + hwid[0].Substring(0, 10)
                 + date[1]
                 + hwid[2].Substring(0, 10)
                 + date[0];
}
```

This is cosmetic — it is shown in the UI, not sent to the server.

---

## 6. HTTP Request Structure

All license commands are sent as a JSON object in an HTTP POST body.

**Endpoint:** `https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand`  
**Method:** POST  
**Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Standard request payload:**

```json
{
  "command":     "<command name>",
  "product_id":  "<base64 product id>",
  "version":     "<semver string>",
  "HWID":        "<3-segment SHA1 HWID>",
  "SID":         "<32-char hex session id>",
  "license_key": "<XXXXXXXX-XXXXXXXX-XXXXXXXX-XXXXXXXX>",
  "hash":        "<HMACSHA256 of fields, Base64 encoded>"
}
```

**Field list construction (`CountConfiguration`):**

```csharp
private static List<ValueTuple<string, string>> CountConfiguration(string cmd, ...)
{
    ManageConfiguration(); // ensure SID exists
    return new List<ValueTuple<string, string>> {
        ("command",     cmd),
        ("product_id",  "No1lKII9IzcBAbihub6nCg=="),    // base64 ID, NO "LK" suffix
        ("version",     m_Expression.ToString()),         // e.g. "0.11.1"
        ("HWID",        attr),
        ("SID",         global),
        ("license_key", m_Repository),
    };
}
```

> **Important:** The `product_id` field in HTTP requests uses the plain base64 ID — it does **NOT** include the `LK` suffix. The `LK` suffix is only used in EditorPrefs key names.

The `hash` field is appended to the list after HMAC computation (see §7). The full list is then JSON-serialized.

**HTTP client:**
- ADO 2022 / CE: `UnityWebRequest` (async, polled at 100ms intervals)
- ADO 2019: `HttpWebRequest` / `WebRequest.CreateHttp()`

---

## 7. HMAC Signing

The signature is computed over the **concatenation of all field values** in the order they appear in the list (not including the hash field itself):

```
HMAC input = command_value + product_id_value + version_value + HWID_value + SID_value + license_key_value
```

**Algorithm:** HMACSHA256  
**Output:** Base64-encoded 32-byte digest, sent as the `"hash"` field

**Per-product HMAC secrets (hardcoded plaintext in assembly):**

| Product | HMAC-SHA256 Secret Key |
|---|---|
| ADOverhaul (2019 + 2022) | `of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay\`phI qK_$*1;O KG?` |
| ControllerEditor | `z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?\`1EJ-w[` |

**Signing code (ADO `StartConfiguration`):**

```csharp
private static void StartConfiguration(List<ValueTuple<string, string>> item)
{
    StringBuilder sb = new StringBuilder();
    foreach (var (_, value) in item)
        sb.Append(value);

    using (HMACSHA256 hmac = new HMACSHA256(
        Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{...}")))
    {
        string hash = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        item.Add(("hash", hash));
    }
}
```

**Example HMAC computation** (from captured `activatelicense` ADO request):
```
input:  "activatelicense" + "No1lKII9IzcBAbihub6nCg==" + "0.11.1"
      + "DD2C...EE7C..." + "06bf7cf4..." + "<YOUR_LICENSE_KEY>"
output: "<HMAC_HASH_ADO_ACTIVATE>"
```

> **Note:** `sendfeedback` and `getdownloadinfo` commands send empty or missing HWID/license_key and still include a hash computed from empty strings. `getdownloadinfo` sends no hash at all (minimal payload).

---

## 8. Server Endpoint and Commands

**Endpoint:** `https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand`

| Command | Payload | Purpose |
|---|---|---|
| `activatelicense` | Full auth payload (all 6 fields + hash) | Initial license activation |
| `verifylicense` | Full auth payload | Re-verify on every project load |
| `getdownloadinfo` | `command`, `product_id`, `version` only | Check for available update |
| `sendfeedback` | Full payload + `feedback` text; HWID and `license_key` are empty | Send user feedback |
| `findsolution` | Full auth payload | Look up error solution in server DB |
| `reportbug` | Full auth payload | Submit bug report |
| `transferlicenserequest` | Full auth payload | Step 1 of device transfer — triggers email |
| `transferlicense` | Full auth payload + verification code | Step 2 of device transfer |

---

## 9. Server Response Parsing

The server returns a JSON object. The DLL reads the following fields:

```json
{
  "success":               true,
  "message":               "Optional status message (shown in dialog/log)",
  "url":                   "Optional URL for dialog button",
  "url_name":              "Label for the URL button (default: \"Link\")",
  "wait_warn":             false,
  "wait_time":             0,
  "download_message":      "Used by getdownloadinfo",
  "variant":               "Update variant identifier",
  "changelog_link":        "Link to changelog",
  "announcement":          "Banner text for plugin window",
  "announcement_link":     "URL for announcement banner",
  "announcement_link_name":"Label for announcement link",
  "transfer_email":        "Email address shown during transfer flow"
}
```

**Response handling (`QueryConfiguration`):**

```csharp
private static void QueryConfiguration(ParamsIdentifier i, Action onSuccess, Action onFail, ...)
{
    bool success  = i.PublishConsumer("success");
    string msg    = i.PublishConsumer("message");
    string url    = i.PublishConsumer("url");
    string url_name = i.PublishConsumer("url_name") ?? "Link";

    if (success)
    {
        if (!string.IsNullOrEmpty(msg)) NewIdentifier(msg, Regular, true);
        onSuccess?.Invoke();
    }
    else
    {
        bool waitWarn = i.PublishConsumer("wait_warn");
        float waitTime = i.PublishConsumer("wait_time");
        manager = manager || waitWarn;               // rate-limit flag
        if (waitTime > 0) _System = Time.realtimeSinceStartup + waitTime;
        onFail?.Invoke();
        if (!string.IsNullOrEmpty(msg))
        {
            if (string.IsNullOrEmpty(url))
                EditorUtility.DisplayDialog("Warning!", msg, "Ok");
            else if (EditorUtility.DisplayDialog("Warning!", msg, url_name, "Ok"))
                Application.OpenURL(url);
        }
    }
}
```

When `success = true` is received for `activatelicense`:
1. License data fields (`u`, `v`, `r`, `m`, `date`) are populated from the server response
2. Those fields, plus the current date, are **written to DSLICINF** (AES-encrypted to EditorPrefs)
3. `SessionState.SetBool(sessionKey, true)` is set
4. `_Service = true` (ADO) / `m_DispatcherAnnotation = true` (CE)
5. The `m_Wrapper` callback chain is fired, unlocking plugin subsystems

---

## 10. DSLICINF — Local License Cache

`DSLICINF` is an EditorPrefs string that caches the last successful license verification result, keyed by today's UTC date. This avoids making a server request on every Unity startup.

**EditorPrefs key:** `"DSLICINF"` (shared between ADO and CE — same key, same format)

### Stored Fields

| JSON key | Content | In-memory field (ADO) |
|---|---|---|
| `"date"` | Today's UTC date as `MM/DD/YYYY` — cache invalid if this doesn't match | `setter` |
| `"u"` | Username from server (Gumroad buyer name) | `listener` / `m_FilterAnnotation` |
| `"v"` | Display name (may differ from username) | `m_Printer` / `m_ReaderAnnotation` |
| `"r"` | Role / tier (e.g. license type) | `_Object` / `m_ParamsAnnotation` |
| `"m"` | HWID string (the same 3-segment SHA1 computed locally) | `attr` / `_WriterAnnotation` |

**TTL:** 1 day — the `"date"` field must match today's UTC date (`MM/DD/YYYY`) exactly.

### Cache Validation Path

On every Unity startup, `AssetConfiguration()` runs:

1. Checks `SessionState.GetBool(sessionKey, false)`:
   - Key: `"No1lKII9IzcBAbihub6nCg==" + EditorAnalyticsSessionInfo.id` (ADO)
   - Key: `"yOk0XCnENLMO6DIF8cYpSg==" + EditorAnalyticsSessionInfo.id` (CE)
   - If `false` → skip cache, go straight to HWID gather + server call
   - If `true` → attempt cache decode

2. Reads DSLICINF from EditorPrefs, AES-decrypts with **inner keys** (see §11), HMAC-validates each field.

3. Compares decrypted `"date"` field against today's date:
   - Match → load `u`, `v`, `r`, `m` into static fields, set `_Service = true`, done
   - Mismatch → log `"failed to verify from cache."` (warning), delete DSLICINF, proceed to HWID + server

4. Regardless of cache result: `CloneConfiguration(callback, true)` runs to re-collect HWID asynchronously. If the server call succeeds (currently impossible), DSLICINF is refreshed.

```csharp
// cache hit path (simplified)
if (SessionState.GetBool(itemContext, false))
{
    using AesManaged aes = new AesManaged();
    aes.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");
    aes.IV  = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
    using HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(itemContext));

    if (RemoveConfiguration() == AwakeWatcher("date", aes, hmac))
    {
        listener  = AwakeWatcher("u", aes, hmac);
        m_Printer = AwakeWatcher("v", aes, hmac);
        _Object   = AwakeWatcher("r", aes, hmac);
        attr      = AwakeWatcher("m", aes, hmac);
        _Service  = true;
        // ...
    }
}
```

---

## 11. AES Cipher Details

Two separate AES-128-CBC key/IV pairs are used, one for reading cached fields and one for the outer cache envelope:

### Read Key (DSLICINF per-field decrypt — `ListIdentifier`)

| Parameter | Value (Base64) |
|---|---|
| Key | `LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=` |
| IV | `MEZqk6gCgPTwifeH3YrTlQ==` |

Used when reading individual field values out of the cached JSON dict.

### Write Key (DSLICINF outer envelope — `PatchIdentifier` / `CallIdentifier`)

| Parameter | Value (Base64) |
|---|---|
| Key | `3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=` |
| IV | `MTOuc+v23iVKtf8SLX3WxQ==` |

Used for encrypting / decrypting the full DSLICINF blob before/after the permutation shuffle.

Both key pairs are hardcoded in plain text in both assemblies (not further obfuscated beyond .NET Reactor string encryption).

---

## 12. DSLICINF Encryption Pipeline

The DSLICINF cache value is protected by a two-layer transform: **AES-128-CBC** followed by a **character-position permutation shuffle**.

### Encryption (write path — `StopIdentifier` in ADO, `CreateMapper` in CE)

```
plaintext JSON
    │
    ▼  PatchIdentifier / RunMapper
AES-128-CBC encrypt
Key = "3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA="
IV  = "MTOuc+v23iVKtf8SLX3WxQ=="
    │
    ▼  RegisterIdentifier([3, 2, 6, 4, 2, 1, 8]) / ReflectMapper(enc_offsets)
Character-position permutation shuffle
    │
    ▼
Base64 ciphertext stored in EditorPrefs["DSLICINF"]
```

### Decryption (read path — `PushIdentifier` in ADO, `NewMapper` in CE)

```
Base64 ciphertext from EditorPrefs["DSLICINF"]
    │
    ▼  RegisterIdentifier([8, 1, 2, 4, 6, 2, 3]) / ReflectMapper(dec_offsets)
Inverse character-position permutation
    │
    ▼  CallIdentifier / LoginMapper
AES-128-CBC decrypt
Key = "3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA="
IV  = "MTOuc+v23iVKtf8SLX3WxQ=="
    │
    ▼
plaintext JSON
```

### Permutation Function (`RegisterIdentifier` / `ReflectMapper` / `ChangeIdentifier`)

The permutation cycles through the string with a counter, swapping characters at periodic intervals:

```csharp
internal static string ChangeIdentifier(string def, int stride)
{
    int counter = 2;
    for (int i = stride; i < def.Length; i += stride)
    {
        counter++;
        if (counter == 3)
        {
            int j = i + stride;
            if (j >= def.Length) break;
            char tmp = def[j];
            def = def.Remove(j, 1).Insert(j, def[i].ToString());
            def = def.Remove(i, 1).Insert(i, tmp.ToString());
            counter = 0;
        }
    }
    return def;
}
```

Applied repeatedly with each offset in the array `[3, 2, 6, 4, 2, 1, 8]` (write) or `[8, 1, 2, 4, 6, 2, 3]` (read). The ADO offsets are hard-coded literals; the CE offsets are loaded from an encrypted smethod call (`smethod_N(565931375)` for write, `smethod_N(943980522)` for read) but resolve to the same or equivalent arrays.

### Per-Field HMAC Integrity (`ForgotIdentifier`)

After decrypting each field value, its HMAC-SHA1 digest is computed and compared against a stored digest:

```csharp
internal static string ForgotIdentifier(string value, ref HMAC hmac)
    => Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
```

The HMAC-SHA1 key is the **SessionState key**: `"No1lKII9IzcBAbihub6nCg==" + EditorAnalyticsSessionInfo.id`.  
This means the DSLICINF cache is *session-bound* — it cannot be copied between machines with different session IDs.

---

## 13. Full Verification Flow — Step by Step

```
Unity domain reload / project load
    │
    ▼
[InitializeOnLoadMethod] DisableConfiguration() / VerifyAnnotation()
    │
    ├─ read license key from EditorPrefs
    ├─ if no key → _Worker = false, stop (no key → no verification)
    └─ if key present → schedule AssetConfiguration()
                            │
                            ▼
                      AssetConfiguration(bool testkey)
                            │
                            ├─ if already verifying (_Struct) → return
                            ├─ set _Struct = true, m_Pool = true
                            │
                            ├─ read SessionState.GetBool(sessionKey)
                            │       │
                            │       ├─ TRUE:
                            │       │    read DSLICINF from EditorPrefs
                            │       │    AES-decrypt per-field values
                            │       │    verify HMAC-SHA1 integrity of each field
                            │       │    compare "date" field to today's UTC date
                            │       │       ├─ MATCH → load u/v/r/m fields
                            │       │       │          _Service = true ✓
                            │       │       │          fire m_Wrapper callbacks
                            │       │       │          repaint windows
                            │       │       └─ MISMATCH → warn, delete DSLICINF
                            │       └─ FALSE: (skip cache)
                            │
                            └─ CloneConfiguration(callback, ispred=true)
                                    │
                                    ├─ read DSLICINF from EditorPrefs (decrypt outer envelope)
                                    │  (used to compare stored HWID against collected HWID)
                                    │
                                    ├─ launch 4 async WMI queries (10s timeout)
                                    │   wmic baseboard / cpu / diskdrive / memorychip
                                    │
                                    └─ when all complete → callback()
                                            │
                                            ├─ compare collected HWID vs cached HWID
                                            │
                                            ├─ if match AND DSLICINF valid → trust cache
                                            │
                                            └─ send activatelicense / verifylicense to server
                                                    │
                                                    ├─ success=true →
                                                    │      populate u/v/r/m from response
                                                    │      write DSLICINF to EditorPrefs
                                                    │      set SessionState.SetBool(sessionKey, true)
                                                    │      _Service = true ✓
                                                    │      fire m_Wrapper callbacks
                                                    │
                                                    └─ success=false →
                                                           show dialog with server message
                                                           _Service remains false ✗
```

---

## 14. License State Fields

### ADOverhaul (`IdentifierSerializerConnector` / `LicenseManager`)

| Field | Type | Purpose |
|---|---|---|
| `_Service` | `bool` | **PRIMARY LICENSE GATE** — guards all plugin functionality |
| `_Reponse` | `bool` | Session already successfully verified |
| `specification` | `bool` | License-grant callback chain already fired |
| `m_Wrapper` | `Action` | Callback chain invoked when license is granted |
| `_Worker` | `bool` | License key present and valid format |
| `_Struct` | `bool` | Verification in progress (network pending) |
| `m_Pool` | `bool` | Activation pending (awaiting HWID collection) |
| `_Rule` | `bool` | Activate button was clicked |
| `manager` | `bool` | Rate limit warning active |
| `m_Algo` | `bool` | Was licensed in previous session |
| `indexer` | `bool` | Troubleshoot mode (cosmetic, skips some HWID checks) |
| `m_Config` | `bool` | License transfer mode active |
| `m_Repository` | `string` | License key (Gumroad format) |
| `global` | `string` | SID from EditorPrefs |
| `attr` | `string` | Raw HWID (3-segment SHA1) |
| `_Interpreter` | `string` | Scrambled HWID for display |
| `listener` | `string` | Username from DSLICINF `"u"` |
| `m_Printer` | `string` | Display name from DSLICINF `"v"` |
| `_Object` | `string` | Role/tier from DSLICINF `"r"` |
| `setter` | `string` | Today's date `MM/DD/YYYY` |
| `_System` | `float` | Rate-limit end time (`Time.realtimeSinceStartup`) |
| `m_Expression` | `SemVer` | Plugin version string (e.g. `"0.11.1"`) |

### ControllerEditor (embedded in `ControllerEditor` class)

| Field | Type | Purpose |
|---|---|---|
| `m_DispatcherAnnotation` | `bool` | **PRIMARY LICENSE GATE** |
| `m_IdentifierAnnotation` | `bool` | License key present and valid |
| `_RequestAnnotation` | `bool` | Session already verified |
| `attrAnnotation` | `bool` | Waiting for network (must be false for normal operation) |
| `listenerAnnotation` | `bool` | License-grant callback chain ready |
| `m_GetterAnnotation` | `bool` | DSLICINF cache successfully read |
| `m_BridgeAnnotation` | `string` | License key |
| `databaseAnnotation` | `string` | SID |
| `_WriterAnnotation` | `string` | HWID |
| `m_FilterAnnotation` | `string` | Username from DSLICINF `"u"` |
| `m_ReaderAnnotation` | `string` | Display name from DSLICINF `"v"` |
| `m_ParamsAnnotation` | `string` | Role from DSLICINF `"r"` |

---

## 15. EditorPrefs and SessionState Keys

### EditorPrefs (persistent across Unity restarts)

| Key | Product | Content |
|---|---|---|
| `"No1lKII9IzcBAbihub6nCg==LK"` | ADOverhaul | License key (plaintext) |
| `"yOk0XCnENLMO6DIF8cYpSg==LK"` | ControllerEditor | License key (plaintext) |
| `"ADOverhaulLicenseField"` | ADOverhaul | GUI license field backup |
| `"Controller EditorLicenseField"` | ControllerEditor | GUI license field backup |
| `"DreadScriptssid"` | Both | 32-char hex session ID |
| `"DSLICINF"` | Both | AES+permutation encrypted license cache JSON |
| `"No1lKII9IzcBAbihub6nCg==SettingsJSON"` | ADOverhaul | Plugin settings (JSON blob) |
| `"yOk0XCnENLMO6DIF8cYpSg==SettingsJSON"` | ControllerEditor | Plugin settings (JSON blob) |

### SessionState (cleared on domain reload / Unity restart)

| Key pattern | Content |
|---|---|
| `"No1lKII9IzcBAbihub6nCg==" + EditorAnalyticsSessionInfo.id` | Bool: DSLICINF successfully validated for this session (ADO) |
| `"yOk0XCnENLMO6DIF8cYpSg==" + EditorAnalyticsSessionInfo.id` | Bool: DSLICINF successfully validated for this session (CE) |

`SessionState.EraseBool(key)` is called on logout/deactivation (`SearchAnnotation` in CE).

---

## 16. Rate Limiting and Anti-Abuse

The server can respond with rate-limit metadata:

- `"wait_warn": true` — sets `manager = true` (displays a warning: *"Too many failed attempts! Further failed attempts will result in getting your device blocked!"*)
- `"wait_time": <seconds>` — sets `_System = Time.realtimeSinceStartup + wait_time` (blocks activation button until elapsed)

```csharp
private static float ResolveSerializer() => _System - Time.realtimeSinceStartup;
private static bool GetSerializer()      => ResolveSerializer() > 0f;
```

The display during cooldown:
```
"Please wait N seconds."
```

---

## 17. License Transfer / 2FA Flow

To move a license to a new device, the DLL implements a two-step transfer flow:

**Step 1 — Request transfer:**
- User clicks *"Transfer License"* in the DRM UI
- POST with `command = "transferlicenserequest"` (full auth payload)
- Server responds with `transfer_email` field and a message like *"Verification code sent to your email."*

**Step 2 — Confirm with code:**
- User enters the 6-character alphanumeric code from their email
- Verification code format: `^[a-zA-Z0-9]{6}$`
- POST with `command = "transferlicense"` (full auth payload + the verification code)
- Server activates the new device and blocks the old device for 30 days

---

## 18. Product IDs

| Product | Base64 ID | In requests (`product_id` field) | In EditorPrefs key suffix |
|---|---|---|---|
| ADOverhaul | `No1lKII9IzcBAbihub6nCg==` | `No1lKII9IzcBAbihub6nCg==` | `No1lKII9IzcBAbihub6nCg==LK` |
| ControllerEditor | `yOk0XCnENLMO6DIF8cYpSg==` | `yOk0XCnENLMO6DIF8cYpSg==` | `yOk0XCnENLMO6DIF8cYpSg==LK` |

> The `LK` suffix appears only in EditorPrefs keys, never in HTTP payloads.

---

## 19. Per-Product Differences: ADOverhaul vs ControllerEditor

| Aspect | ADOverhaul 2022 | ControllerEditor |
|---|---|---|
| DRM class | `IdentifierSerializerConnector` (separate class) | Embedded in `ControllerEditor` |
| Primary license flag | `_Service` | `m_DispatcherAnnotation` |
| License key EditorPrefs | `"No1lKII9IzcBAbihub6nCg==LK"` | `"yOk0XCnENLMO6DIF8cYpSg==LK"` |
| Product ID | `No1lKII9IzcBAbihub6nCg==` | `yOk0XCnENLMO6DIF8cYpSg==` |
| HMAC secret | ADO-specific (see §7) | CE-specific (see §7) |
| HTTP client | `UnityWebRequest` | `UnityWebRequest` |
| DSLICINF AES keys | Same as CE | Same as ADO |
| Permutation offsets (write) | `[3, 2, 6, 4, 2, 1, 8]` | Encrypted smethod, equivalent |
| Permutation offsets (read) | `[8, 1, 2, 4, 6, 2, 3]` | Encrypted smethod, equivalent |
| SessionState key prefix | `"No1lKII9IzcBAbihub6nCg=="` | `"yOk0XCnENLMO6DIF8cYpSg=="` |
| Settings JSON key | `"No1lKII9IzcBAbihub6nCg==SettingsJSON"` | `"yOk0XCnENLMO6DIF8cYpSg==SettingsJSON"` |

Both products share the same `"DreadScriptssid"` EditorPrefs key, meaning the SID is the same across products on the same machine.

Both products share the same `"DSLICINF"` key. In practice only one product writes it at a time (whichever activates first), since the AES keys are identical — a valid DSLICINF written by one product would be readable by the other.

---

## 20. ADOverhaul 2019 vs 2022 Differences

| Aspect | ADOverhaul 2019 | ADOverhaul 2022 |
|---|---|---|
| DRM class | `ConfigurationTestStub` | `IdentifierSerializerConnector` |
| HTTP client | `HttpWebRequest` / `WebRequest.CreateHttp()` | `UnityWebRequest` |
| HMAC secret | **Identical** to 2022 | `of,ejcX?$0 &n*Uc{...}` |
| HMAC algorithm | HMACSHA256 | HMACSHA256 |
| Product ID | `No1lKII9IzcBAbihub6nCg==` (same) | `No1lKII9IzcBAbihub6nCg==` |
| smethod transform A/B | Different constants (see below) | Different constants (see below) |
| Module GUID | `{CAD6ED8D-8CDE-4E08-A19D-89CBC52DD07C}` | `{7907DD2F-A0A5-4805-95CD-D1B3741C5FB4}` |
| de4dot output | 234 KB | 242 KB |

The DRM logic, EditorPrefs keys, DSLICINF format, HWID algorithm, and server endpoint are identical between 2019 and 2022.

### smethod Transform Constants

#### ADOverhaul 2019

| smethod | A | B |
|---|---|---|
| smethod_1 | -2141544851 | 369471511 |
| smethod_2 | -390841461 | -1554884899 |
| smethod_3 | -986723015 | 1800645099 |
| smethod_4 | -166230583 | 170860046 |
| smethod_5 | 2100623687 | -1443590153 |

#### ADOverhaul 2022

| smethod | A | B |
|---|---|---|
| smethod_1 | 1524534657 | -1312116512 |
| smethod_2 | 2014371519 | -1993193028 |
| smethod_3 | 1440914379 | 453025292 |
| smethod_4 | -760883319 | -330307364 |
| smethod_5 | -943102013 | -7450518 |

#### ControllerEditor

| smethod | A | B |
|---|---|---|
| smethod_1 | 1553271299 | -1677909072 |
| smethod_2 | 1034558559 | -2111195616 |
| smethod_3 | 2075742147 | -222258040 |
| smethod_4 | -847612911 | 1949544612 |
| smethod_5 | -1528922027 | -1347067868 |

---

## 21. Obfuscation (.NET Reactor)

All three DLLs are packed with **.NET Reactor**. De4dot successfully strips most obfuscation but leaves the string encryption intact.

### String Encryption (`smethod_N`)

Every string constant is replaced by a call to a generic decrypt method:
```csharp
<Module>.smethod_3<string>(-1234567)
```

**Decryption transform:**
```csharp
int transformed = (key * A) ^ B;            // int32 overflow arithmetic
int offset      = (transformed & 0x3FFFFFFF) << 2;
// upper 2 bits of transformed = type tag: 0 = string, 1 = int, 2 = long
// offset indexes into byte_0 (the encrypted string blob)
```

### Anti-External-Caller Guard

Every `smethod_N` method checks:
```csharp
if (Assembly.GetExecutingAssembly().Equals(Assembly.GetCallingAssembly()))
    { /* decrypt */ }
else
    { return default(T); }
```

This blocks direct external invocation. String extraction was performed using `DynamicMethod` invocation (immune to the check) and offline IL scanning.

### Sentinel Fields (Cosmetic)

Every class has a cosmetic null-check pattern:
```csharp
internal static ClassName SentinelField;   // always null
internal static bool SentinelCheck() => SentinelField == null;  // always true
```

These are anti-tamper decoys — they do not gate any execution path.

### Control Flow Obfuscation

Switch-based CFG obfuscation (308 patterns in ADO, 793 in CE):
```csharp
for (;;) {
    uint num = CONSTANT;
    for (;;) {
        switch ((num ^ XOR_KEY) % MODULUS) {
            case N: /* real instruction */; num = NEXT; continue;
            case M: goto IL_XXXX;
        }
        return;
    }
}
```

---

## 22. Restoration / Bypass

Since the backend is offline and permanently returns `success=false`, the DSLICINF cache write path is never triggered under normal circumstances.

### ADOverhaul — `ADORestorationPatch`

Strategy (Options A+B combined):
- **Postfix on `AssetConfiguration`:** After it runs, set `_Service=true`, `_Reponse=true`, `_Worker=true`, `_Struct=false` via reflection; call `ResolveConfiguration(true)` to fire the callback chain.
- **Dialog suppressor:** Prefixes `EditorUtility.DisplayDialog` to suppress the server-down popup when `_Service=true`.
- **Direct field set on install:** `[InitializeOnLoad]` forces fields via reflection immediately on domain reload.
- **Class lookup:** by name `DreadScripts.ADOverhaul.IdentifierSerializerConnector`; falls back to TypeDef token `0x0200000D` for the original (non-de4dot) DLL.
- **Method lookup:** `AssetConfiguration` by name; falls back to `static void(bool)` signature matching.

### ControllerEditor — `CERestorationPatch`

Strategy:
- **Postfix on `VerifyAnnotation`:** Set `m_DispatcherAnnotation=true` after every DRM startup check.
- **Dialog suppressor:** Same pattern as ADO.
- **Class lookup:** `DreadScripts.ControllerEditor.ControllerEditor`; falls back to TypeDef token `0x0200001F` (RID 31).
- **Method lookup:** `VerifyAnnotation` by name; falls back to `[InitializeOnLoadMethod] static void()` signature.

### Mock Server

`mock_server/main.go` — a Go HTTPS server that accepts all requests and returns `success: true`. To use:
1. Add hosts entry: `127.0.0.1  us-central1-dreadscripts-c6b62.cloudfunctions.net`
2. Generate and install the self-signed TLS cert into the Windows Trusted Root CA store
3. Run `go run .` (requires port 443 admin access, or `-addr :8443`)

The mock server accepts all commands and returns appropriate stub responses. This causes the DLL to write a valid DSLICINF cache after activation.

### Synthetic DSLICINF (Option C — not yet implemented)

To construct a valid DSLICINF manually:
1. Build the JSON: `{"date":"MM/DD/YYYY","u":"username","v":"display","r":"role","m":"<HWID>"}`
2. Compute per-field HMAC-SHA1 integrity hashes using key `"No1lKII9IzcBAbihub6nCg==" + sessionId`
3. AES-128-CBC encrypt individual fields using the read-key pair
4. Encode the outer dict, AES-encrypt with the write-key pair, apply permutation shuffle
5. Store result in `EditorPrefs["DSLICINF"]`
6. Call `SessionState.SetBool(sessionKey, true)`

The DLL will then read this cache on next startup and set `_Service = true` without any server call.

---

## 23. Captured Real Requests

The following requests were captured via the local mock server from both DLLs running in Unity Editor.

### `activatelicense` — ADOverhaul

```json
{
  "command": "activatelicense",
  "product_id": "No1lKII9IzcBAbihub6nCg==",
  "version": "0.11.1",
  "HWID": "<HWID_SEGMENT_1>-<HWID_SEGMENT_2>-<HWID_SEGMENT_3>",
  "SID": "<YOUR_SID>",
  "license_key": "<YOUR_LICENSE_KEY>",
  "hash": "<HMAC_HASH_ADO_ACTIVATE>"
}
```

### `activatelicense` — ControllerEditor

```json
{
  "command": "activatelicense",
  "product_id": "yOk0XCnENLMO6DIF8cYpSg==",
  "version": "3.3.2",
  "HWID": "<HWID_SEGMENT_1>-<HWID_SEGMENT_2>-<HWID_SEGMENT_3>",
  "SID": "<YOUR_SID>",
  "license_key": "<YOUR_LICENSE_KEY>",
  "hash": "<HMAC_HASH_CE_ACTIVATE>"
}
```

### `transferlicenserequest` — ADOverhaul

```json
{
  "command": "transferlicenserequest",
  "product_id": "No1lKII9IzcBAbihub6nCg==",
  "version": "0.11.1",
  "HWID": "<HWID_SEGMENT_1>-...",
  "SID": "<YOUR_SID>",
  "license_key": "<YOUR_LICENSE_KEY>",
  "hash": "<HMAC_HASH_ADO_TRANSFER>"
}
```

### `sendfeedback` — ADOverhaul

```json
{
  "command": "sendfeedback",
  "product_id": "No1lKII9IzcBAbihub6nCg==",
  "version": "0.11.1",
  "HWID": "",
  "SID": "<YOUR_SID>",
  "license_key": "",
  "feedback": "test",
  "hash": "<HMAC_HASH_ADO_FEEDBACK>"
}
```

### `getdownloadinfo` — ADOverhaul (minimal payload)

```json
{
  "command": "getdownloadinfo",
  "product_id": "No1lKII9IzcBAbihub6nCg==",
  "version": "0.11.1"
}
```

### Key Observations

- `product_id` in HTTP requests does **not** include the `LK` suffix
- HWID is 3 × 40-char uppercase SHA1 segments joined by `-`
- Different commands with the same credentials always produce different hashes (command is in the HMAC input)
- `sendfeedback` sends empty HWID and license_key (both omitted from signing input as empty strings)
- `getdownloadinfo` sends no HWID, SID, license_key, or hash — server doesn't require auth for update checks
- Both products share the same HWID and SID on the same machine (same hardware, same `DreadScriptssid` key)
- The same license key (`539568E5-...`) produces different hashes for ADO vs CE because of different HMAC secrets
