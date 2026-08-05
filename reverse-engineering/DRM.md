# DreadScripts DRM — Comprehensive Reverse Engineering Notes

> **Products:** ADOverhaul (2019 + 2022 builds), ControllerEditor  
> **Author:** DreadScripts  
> **Backend:** Google Firebase Cloud Function (permanently offline as of 2024)  
> **Goal:** Document the licence verification system, and what it takes to run the tools without a server to validate against.

> **Identifiers.** The decompiled sources in `reverse-engineering/export/` have been renamed from their de4dot output
> names. This document leads with the current name and gives the de4dot name in parentheses where one
> is useful for joining against older notes — e.g. `isLicensed` (de4dot `_Service`).

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
10. [The License Token — the Actual Gate](#10-the-license-token--the-actual-gate)
11. [License Cache and the DSLICINF Hardware Memo](#11-license-cache-and-the-dslicinf-hardware-memo)
12. [AES Cipher Details](#12-aes-cipher-details)
13. [DSLICINF Encryption Pipeline](#13-dslicinf-encryption-pipeline)
14. [Full Verification Flow — Step by Step](#14-full-verification-flow--step-by-step)
15. [License State Fields](#15-license-state-fields)
16. [EditorPrefs and SessionState Keys](#16-editorprefs-and-sessionstate-keys)
17. [Rate Limiting and Anti-Abuse](#17-rate-limiting-and-anti-abuse)
18. [License Transfer / 2FA Flow](#18-license-transfer--2fa-flow)
19. [Product IDs](#19-product-ids)
20. [Per-Product Differences: ADOverhaul vs ControllerEditor](#20-per-product-differences-adoverhaul-vs-controllereditor)
21. [ADOverhaul 2019 vs 2022 Differences](#21-adoverhaul-2019-vs-2022-differences)
22. [Obfuscation (.NET Reactor)](#22-obfuscation-net-reactor)
23. [Restoration / Bypass](#23-restoration--bypass)
24. [Captured Real Requests](#24-captured-real-requests)

---

## 1. Overview

Both plugins share an identical DRM architecture, implemented independently in each assembly — in both
cases directly on the main plugin class, not in a separate DRM type. The DRM is a client-side license
verification system with:

- A **Gumroad-format license key** entered by the user
- A **hardware fingerprint (HWID)** computed from WMI hardware identifiers
- A **session ID (SID)** persisted in EditorPrefs
- A **signed HTTP POST** to a Firebase Cloud Function
- A **cryptographic license token** returned by the server, which the client recomputes locally and
  which is what actually unlocks the plugin (see §10)
- A **per-editor-session field cache** in `SessionState`, which avoids a server round-trip on domain
  reload but never survives an editor restart (see §11)

The backend endpoint `https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand` is
permanently offline. All attempts to activate now receive a "shutdown" response with `success=false`.

> **Unverified:** that the live endpoint *permanently* returns `success=false` cannot be settled from
> the decompiled source. It would take a live probe of the endpoint to confirm it has not changed.

---

## 2. DRM Entry Points

### ADOverhaul 2022

- **DRM class:** `ADOverhaul` (de4dot `IdentifierSerializerConnector`) — the main plugin class. The
  DRM methods are static members of it, alongside `ADOverhaulWindow`, `ADOSettings`, `BugReporter`,
  `ProcessRunner` and the PhysBone editors. There is no separate DRM type.
- **Entry:** `[InitializeOnLoadMethod] DisableConfiguration()` — fires on every Unity domain reload
  - Reads the license key from `EditorPrefs["No1lKII9IzcBAbihub6nCg==LK"]`
  - If key is present and `a_VerifyOnProjectLoad` setting is true → schedules `AssetConfiguration(false)`
- **Secondary entry:** `VisitConfiguration()` — fires on every repaint of the DRM window when
  `a_VerifyOnDisplay` is true and the license has not already been checked this session
- **Primary gate method:** `MoveConfiguration()` — returns `isLicensed` and logs an error if not licensed

```csharp
[InitializeOnLoadMethod]
private static void DisableConfiguration()
{
    bool flag = RateConfiguration(); // reads key from EditorPrefs
    if (!ADOSettings.Instance().a_HasSucceededLastVerification)
    {
        licenseKeyEntryRequired = true;
        licenseCheckedThisSession = flag;
    }
    if (flag && (bool)ADOSettings.Instance().a_VerifyOnProjectLoad)
    {
        ADOEditorUtility.DelayCall(delegate
        {
            AssetConfiguration(testkey: false);
        });
    }
}
```

Note that this method always runs to completion — a missing key does not "stop" anything, it just
means no check is scheduled.

### ControllerEditor

- **DRM class:** the main `ControllerEditor` class itself — same arrangement as ADOverhaul
- **Entry:** `[InitializeOnLoadMethod] VerifyAnnotation()` — fires on every domain reload
  - Calls `ResolveAnnotation()`, then `AssetAnnotation()` (loads the key)
  - `ResolveAnnotation()` is *not* a Harmony installer: it is a reflection sweep that collects static
    methods across all loaded assemblies carrying `CallbackAttribute` / `CallbackMethodAttribute` /
    `ControllerCallbackAttribute` and orders them by priority. ControllerEditor's Harmony work lives
    in its separate `HarmonyPatchManager` nested class.
  - Sets `licenseCheckedThisSession` (de4dot `m_DispatcherAnnotation`) to the result of
    `AssetAnnotation()` — i.e. to whether a license key is present — not to a cache-hit result
- **Verification method:** `WriteAnnotation(bool)` — ControllerEditor's equivalent of ADOverhaul's
  `AssetConfiguration`

```csharp
[InitializeOnLoadMethod]
private static void VerifyAnnotation()
{
    ResolveAnnotation();
    bool flag = AssetAnnotation();
    if (!EditorSettings.GetInstance().a_HasSucceededLastVerification)
    {
        licenseKeyEntryRequired = true;
        licenseCheckedThisSession = flag;
    }
    if (flag && (bool)EditorSettings.GetInstance().a_VerifyOnProjectLoad)
        EditorUtils.DelayCall(delegate { WriteAnnotation(assetneeded: false); });
}
```

### ADOverhaul 2019

- **DRM class:** `ADOverhaul` (de4dot `ConfigurationTestStub`) — identical architecture to 2022
- Method names differ (e.g. `PrintSystem` for `CountConfiguration`, `RevertSystem` for
  `RevertConfiguration`), but the logic, keys and wire format are the same

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
| ADOverhaul | `"No1lKII9IzcBAbihub6nCg==LK"` | `licenseKey` (de4dot `m_Repository`) |
| ControllerEditor | `"yOk0XCnENLMO6DIF8cYpSg==LK"` | `licenseKey` (de4dot `m_BridgeAnnotation`) |

There is also a secondary EditorPrefs key used by the GUI text field (but not the activation logic):

| Product | GUI backup key |
|---|---|
| ADOverhaul | `"ADOverhaulLicenseField"` |
| ControllerEditor | `"Controller EditorLicenseField"` |

Validation is done before sending:

```csharp
private static bool AddConfiguration()
{
    return Regex.Match(licenseKey,
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
    if (string.IsNullOrWhiteSpace(sessionId))
    {
        string key = "DreadScriptssid";
        sessionId = EditorPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(sessionId) || !Regex.IsMatch(sessionId, "[0-9a-f]{32}"))
        {
            sessionId = GUID.Generate().ToString(); // Unity's GUID, produces lowercase hex without dashes
            EditorPrefs.SetString(key, sessionId);
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

Four WMI categories are queried via `wmic` (primary, Windows CMD) with a `Get-CimInstance` (PowerShell) fallback:

| Category | WMI class | Fields extracted (in this order) |
|---|---|---|
| 0 (baseboard) | `Win32_baseboard` | `Manufacturer`, `Product`, `SerialNumber` |
| 1 (CPU) | `Win32_processor` | `ProcessorId` |
| 2 (disk) | `Win32_diskdrive` | `SerialNumber` |
| 3 (memory) | `Win32_physicalmemory` | `Manufacturer`, `PartNumber`, `SerialNumber`, `Capacity` |

Within a category, the extracted values are concatenated in the literal order above
(`string.Join(string.Empty, …)`), and then **all spaces are stripped**
(`.Replace(" ", string.Empty)`). All four categories are collected and all four are used.

### Segment Assembly

The four category strings collapse into **three** SHA1 segments — the baseboard and CPU strings are
concatenated into one segment before hashing:

| Segment | Source |
|---|---|
| A | `Win32_baseboard` **+** `Win32_processor`, concatenated **then** SHA1 |
| B | `Win32_diskdrive` |
| C | `Win32_physicalmemory` |

```csharp
internal void SetReg()
{
    EditorPrefs.SetString("DSLICINF", StopIdentifier(attrContext.ToString()));  // see §11
    if (_ConfigContext)                            // true whenever ispred: true
    {
        for (int i = 0; i < 4; i++)
            reponseContext[i] += "\r\r";
    }
    string[] array = new string[3]
    {
        reponseContext[0] + reponseContext[1],   // baseboard + CPU
        reponseContext[2],                       // diskdrive
        reponseContext[3]                        // memorychip
    };
    using (SHA1 sHA = SHA1.Create())
    {
        for (int j = 0; j < 3; j++)
            array[j] = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(array[j]))).Replace("-", "");
    }
    hardwareId = string.Join("-", array);
    RestartConfiguration();
    m_MockContext();
}
```

> **Load-bearing and easy to miss:** when `CloneConfiguration` is called with `ispred: true` — which
> is **every** license-path call site — each of the four category strings gets `"\r\r"` appended
> before hashing. Anyone reproducing the HWID offline must include this or every segment will differ.

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

The four subprocesses run **sequentially** on one background thread — `Task.Run` over a `for` loop of
blocking `ProcessRunner.Run()` calls, each ending in `ReadToEnd()` — with the caller then polling
`isFinished` every 50 ms. The pass is bounded by a 10-second `CancellationTokenSource`, and the
PowerShell fallback pass gets its own fresh 10-second budget, so a fully failed collection can take
20 seconds. The four stdout receiver callbacks (`FindWatcher`/`AddWatcher`/`ValidateWatcher`/
`CreateWatcher` in ADO, the `…Observer` equivalents in CE) do nothing but write their process's
output into slot `0..3`; they have no part in the field ordering.

If both the CMD and PowerShell passes fail to yield enough categories, the plugin throws
`"Failed to gather hardware info through …"` and offers a Troubleshoot / Report dialog.

### Scrambled HWID (`unreadDeviceDateFingerprint`)

After HWID assembly, a **transport-scrambled** version is computed in `RestartConfiguration`:

```csharp
private static void RestartConfiguration()
{
    string[] hwid  = hardwareId.Split('-');               // ["DDXX...", "56BX...", "EE7C..."]
    string[] date  = RemoveConfiguration().Split('/');    // ["DD", "MM", "YYYY"]
    date[2] = date[2].Substring(2, 2);                    // last 2 digits of year
    unreadDeviceDateFingerprint = date[2]
                 + hwid[0].Substring(0, 10)
                 + date[1]
                 + hwid[2].Substring(0, 10)
                 + date[0];
}
```

This value is **dead code**. `unreadDeviceDateFingerprint` (de4dot `_Interpreter` in ADOverhaul,
`printerAnnotation` in ControllerEditor) has exactly two references in each assembly: its declaration
and this single assignment. It is never read, never rendered in the UI, and never sent to the server.

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

**Field list construction (`CountConfiguration`; CE `RegisterAnnotation`, ADO 2019 `PrintSystem`):**

```csharp
private static List<(string, string)> CountConfiguration(string i,
                                                         IEnumerable<(string, string)> connection = null)
{
    ManageConfiguration(); // ensure SID exists
    List<(string, string)> list = new List<(string, string)> {
        ("command",     i),
        ("product_id",  "No1lKII9IzcBAbihub6nCg=="),    // base64 ID, NO "LK" suffix
        ("version",     version.ToString()),            // e.g. "0.11.1"
        ("HWID",        hardwareId),
        ("SID",         sessionId),
        ("license_key", licenseKey),
    };
    if (connection != null) list.AddRange(connection);  // per-command extra fields
    return list;
}
```

> **Important:** The `product_id` field in HTTP requests uses the plain base64 ID — it does **NOT**
> include the `LK` suffix. The `LK` suffix is only used in EditorPrefs key names.

All six base fields are always emitted, from the current statics. A field is empty only because the
corresponding static happens to be empty at the time (see §8 on `sendfeedback`).

The `hash` field is appended to the list after HMAC computation (see §7). The full list is then
serialized to JSON by `IncludeConfiguration`, which emits every value as a JSON string.

**HTTP client:** all three assemblies use `HttpWebRequest` via `WebRequest.CreateHttp()` —
`RevertConfiguration` (ADO 2022), `RevertSystem` (ADO 2019), `CancelVisitor` (CE). The request is a
blocking `GetResponse()` inside a `Task.Run`; there is no polling of the web request.
`UnityWebRequest` does appear in all three assemblies, but only for downloading the update package.

---

## 7. HMAC Signing

The signature is computed over the **concatenation of all field values** in the order they appear in the list (not including the hash field itself):

```
HMAC input = command_value + product_id_value + version_value + HWID_value + SID_value + license_key_value
             [+ any per-command extra field values, in list order]
```

**Algorithm:** HMACSHA256  
**Output:** Base64-encoded 32-byte digest, sent as the `"hash"` field

**Per-product HMAC secrets (hardcoded plaintext in assembly):**

| Product | HMAC-SHA256 Secret Key |
|---|---|
| ADOverhaul (2019 + 2022) | `of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay\`phI qK_$*1;O KG?` |
| ControllerEditor | `z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?\`1EJ-w[` |

The same two secrets are also used, with the license key appended, as the key for the license token
in §10.

**Signing code (ADO `StartConfiguration`):**

```csharp
private static void StartConfiguration(List<(string, string)> item)
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

> **Note:** `getdownloadinfo` builds its three-field list inline and never calls `StartConfiguration`,
> so it sends no hash at all. Every other command is signed.

---

## 8. Server Endpoint and Commands

**Endpoint:** `https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand`

| Command | Payload | Purpose |
|---|---|---|
| `activatelicense` | Full auth payload (all 6 fields + hash) | Initial license activation |
| `verifylicense` | Full auth payload | Re-verify on every project load |
| `getdownloadinfo` | `command`, `product_id`, `version` only; no hash | Check for available update |
| `sendfeedback` | Full payload + `feedback` | Send user feedback |
| `findsolution` | Full payload + `bug_id`, `bug_version`, `bug_name`, `bug_exception` | Look up error solution in server DB |
| `reportbug` | Full payload + `bug_id`, `bug_version`, `bug_name`, `bug_exception`, `feedback` | Submit bug report |
| `transferlicenserequest` | Full auth payload | Step 1 of device transfer — triggers email |
| `transferlicenseconfirm` | Full auth payload + `verification_code` | Step 2 of device transfer |

The `feedback` and `bug_exception` values are `Uri.EscapeUriString`-escaped, and free-text bodies
(`feedback` for both `sendfeedback` and `reportbug`) are truncated to 2000 characters client-side.

> **On `sendfeedback`'s empty HWID:** `CountConfiguration` always emits all six base fields, so
> `sendfeedback` is not structurally different. But `sendfeedback` is not wrapped in
> `CloneConfiguration`, so no WMI collection has run and `hardwareId` is reliably empty. The
> `license_key` field, by contrast, will be non-empty whenever a key is stored — the empty value in
> §24's capture is a property of that capture, not of the command.

---

## 9. Server Response Parsing

The server returns a JSON object. The DLL reads the following fields:

| Field | Read by | Purpose |
|---|---|---|
| `success` | all | Bool; drives the success/failure branch |
| `message` | all | Status message; `\n` escapes are unescaped before display |
| `url` | all | Optional URL for dialog button |
| `url_name` | all | Label for the URL button (default `"Link"`) |
| `wait_warn` | all | Bool; sets the rate-limit warning (§17) |
| `wait_time` | all | Seconds; sets the activation cooldown (§17) |
| `date` | `activatelicense`, `verifylicense` | **Hard-compared to the local UTC date**; a mismatch aborts activation |
| `username` | `activatelicense`, `verifylicense` | Gumroad buyer name → cached as `u` |
| `variant` | `activatelicense`, `verifylicense` | **License variant / tier** → cached as `v` |
| `token` | `activatelicense`, `verifylicense` | **The license token (§10)** → cached as `r` |
| `transfer_email` | `transferlicenserequest` | Email address shown during transfer flow |
| `solution`, `complete` | `findsolution` | Server-side fix text and whether it is complete |
| `download_link`, `download_message`, `changelog_link`, `version` | `getdownloadinfo` | Update package info |
| `announcement`, `announcement_link`, `announcement_link_name` | `getdownloadinfo` | Banner text/link for the plugin window |

**Generic response handling (`QueryConfiguration`):**

```csharp
private static void QueryConfiguration(JsonObject i, Action selection, Action comp = null,
                                       bool comparesecond2 = true)
{
    bool success    = i.Item("success");
    string msg      = i.Item("message");
    string url      = i.Item("url");
    bool hasUrl     = !string.IsNullOrEmpty(url);
    string url_name = i.Item("url_name");
    if (string.IsNullOrWhiteSpace(url_name)) url_name = "Link";
    if (!string.IsNullOrWhiteSpace(msg)) msg = msg.Replace("\\n", "\n");

    if (success)
    {
        if (!string.IsNullOrEmpty(msg) && comparesecond2) Log(msg);
        selection?.Invoke();
        return;
    }

    bool waitWarn  = i.Item("wait_warn");
    float waitTime = i.Item("wait_time");
    serverWarnedTooManyAttempts |= waitWarn;
    if (waitTime > 0f) retryAllowedAtRealtime = Time.realtimeSinceStartup + waitTime;
    comp?.Invoke();
    if (!string.IsNullOrEmpty(msg))
    {
        Log(msg, CustomLogType.Error);
        if (!hasUrl)
            EditorUtility.DisplayDialog("Warning!", msg, "Ok");
        else if (EditorUtility.DisplayDialog("Warning!", msg, url_name, "Ok"))
            Application.OpenURL(url);
    }
}
```

### The `activatelicense` / `verifylicense` success path

`QueryConfiguration`'s success callback for these two commands (`RateWatcher` in ADO,
`DeleteObserver` in CE) does, in order:

1. **Date check.** `date` from the response must equal the local UTC date stamp exactly. On mismatch
   it logs and tears the license down — this is a real gate, not a warning:

   ```csharp
   string text = _SystemContext.Item("date");
   if (RemoveConfiguration() != text)
   {
       Log("Date Mismatch! Please make sure your system's date is correct.\nLocal: "
           + currentDateStamp + "  |  Remote: " + text, CustomLogType.Error);
       licenseCheckRetryOffered = true;
       m_SetterContext.EnableWatcher();   // clears isLicensed, erases the session flag
       return;
   }
   ```

2. Populates `licenseUsername` ← `username`, `licenseVariant` ← `variant`, `licenseToken` ← `token`
3. Derives the display name from the username (`InstantiateConfiguration`) and recomputes the
   scrambled fingerprint (`RestartConfiguration`)
4. Sets `isLicensed = true`, persists the license key to EditorPrefs, and marks
   `a_HasSucceededLastVerification`
5. Writes the five cache fields (`date`, `u`, `v`, `r`, `m`) into `SessionState` (§11) and sets
   `SessionState.SetBool(sessionKey, true)`
6. **Verifies the license token (§10).** If it does not match, the whole license is torn down
   immediately.
7. Flushes the pending licensed-callback chain via `ResolveConfiguration` — which checks the token
   again

---

## 10. The License Token — the Actual Gate

`isLicensed = true` on its own unlocks nothing. Every path that actually runs licensed functionality
first recomputes an HMAC-SHA256 locally and requires it to equal the server's `token` field. A mock
server that returns `success: true` without the correct token restores nothing.

**Construction:**

```
key   = <product HMAC secret from §7> + licenseKey
data  = currentDateStamp + hardwareId          // "DD/MM/YYYY" + "<SHA1_A>-<SHA1_B>-<SHA1_C>"
token = Base64(HMACSHA256(key, data))
```

```csharp
internal bool QueryServer()
{
    using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes(
        "of,ejcX?$0 &n*Uc{…KG?" + licenseKey));
    return licenseToken == Convert.ToBase64String(
        hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
}
```

**Where it is checked:**

| Site | Effect on failure |
|---|---|
| Immediately after the cache write on the `activatelicense`/`verifylicense` success path | `EnableWatcher()` / `SearchAnnotation()` — clears `isLicensed`, blanks `licenseToken`/`licenseUsername`/`licenseVariant`, clears `a_HasSucceededLastVerification`, erases the SessionState flag |
| `ReflectConfiguration(Action)` (CE `InterruptAnnotation`) — register a licensed callback | The callback is silently not invoked |
| `ResolveConfiguration(bool)` (CE `ManageAnnotation`) — flush the pending licensed-callback chain | The chain is never flushed; `licensedCallbacksFlushed` stays false |

Because `currentDateStamp` and `hardwareId` are both in the signed input, the token is bound to the
day, the machine, the license key and the product secret at once. Note the date is `DD/MM/YYYY`
(§15) — a token computed from `MM/DD/YYYY` will silently fail to match on every day past the 12th.

The mock server in `drm_server/` implements this correctly (`product.token()` in `handler.go`).

---

## 11. License Cache and the DSLICINF Hardware Memo

Two distinct caches exist and are easy to conflate. Neither is a persistent license cache.

### 11.1 SessionState field cache — where the license data lives

The five license fields are stored **individually in `SessionState`**, one entry each, under
HMAC-derived key names. `SessionState` survives domain reloads but is cleared when the Unity Editor
restarts, so **this cache never survives an editor restart**.

| Cache key | Content | In-memory field |
|---|---|---|
| `"date"` | Today's UTC date as `DD/MM/YYYY` — cache invalid if this doesn't match | `currentDateStamp` |
| `"u"` | Username from the server's `username` field (Gumroad buyer name) | `licenseUsername` |
| `"v"` | License variant / tier from `variant` — rendered as `"License: " + (blank ? "Personal" : variant)` | `licenseVariant` |
| `"r"` | The license token of §10, from `token` | `licenseToken` |
| `"m"` | HWID string (the same 3-segment SHA1 computed locally) | `hardwareId` |

Access is via a matched read/write pair — `AwakeWatcher`/`AssetWatcher` in ADO 2022,
`RateStruct`/`AssetStruct` in ADO 2019, `RunObserver`/`ReflectObserver` in CE:

```csharp
internal string AwakeWatcher(string key, ref … aes, ref … hmac)
{
    return ListIdentifier(SessionState.GetString(
        ForgotIdentifier(itemContext + key, ref hmac), string.Empty), ref aes);
}

internal void AssetWatcher(string key, string value, ref … aes, ref … hmac)
{
    SessionState.SetString(UpdateIdentifier(itemContext + key, ref hmac),
                           SearchIdentifier(value, ref aes));
}
```

`itemContext` is the session key `"<productId>" + EditorAnalyticsSessionInfo.id`. The *name* of each
SessionState entry is `Base64(HMACSHA1(key: itemContext, itemContext + fieldName))`; the *value* is
the field AES-encrypted (§12). Because the HMAC key contains `EditorAnalyticsSessionInfo.id`, the
entry names change every editor session — the cache is session-bound by construction.

### 11.2 Cache read path

On every verification, `AssetConfiguration()` (CE `WriteAnnotation()`) runs:

1. Bail out early if verification is disabled, if no key is entered and no retry was offered, or if a
   verification is already in flight.
2. Set `licenseCheckedThisSession = true`, build `itemContext`.
3. Check `SessionState.GetBool(itemContext, false)`:
   - `false` → skip the cache entirely
   - `true` → read the five fields back, and compare the decrypted `"date"` against today's UTC date
     - Match → load `u`, `v`, `r`, `m`, set `isLicensed = true`, mark restored-from-cache, repaint
     - Mismatch → nothing is loaded
   - Any exception in this block → log `"failed to verify from cache."` (warning)
4. **Regardless of the cache result**, `CloneConfiguration(callback, ispred: true)` runs to re-collect
   the HWID and then unconditionally sends `verifylicense`. There is no "trust the cache and skip the
   network" path and no comparison of the collected HWID against a cached one.

```csharp
if (SessionState.GetBool(itemContext, defaultValue: false))
{
    using AesManaged aes = new AesManaged();
    aes.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");   // ADO; CE differs — §12
    aes.IV  = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
    using HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(itemContext));

    if (RemoveConfiguration() == AwakeWatcher("date", aes, hmac))
    {
        licenseUsername = AwakeWatcher("u", aes, hmac);
        licenseVariant  = AwakeWatcher("v", aes, hmac);
        licenseToken    = AwakeWatcher("r", aes, hmac);
        hardwareId      = AwakeWatcher("m", aes, hmac);
        isLicensed      = true;
        // ...
    }
}
```

### 11.3 DSLICINF — the hardware-info memo

`EditorPrefs["DSLICINF"]` is **not** the license cache. It holds the encrypted plain-text dump of the
WMI query results — the `"<property>: <value>"` lines accumulated by `ConnectWatcher` while parsing
the four `wmic` / `Get-CimInstance` outputs. It carries no license data whatsoever.

```csharp
internal void SetReg()
{
    EditorPrefs.SetString("DSLICINF", StopIdentifier(attrContext.ToString()));
    …
}
```

Its purpose is **HWID stability**. On a later run, `CloneConfiguration` reads and decrypts it up
front, and `ConnectWatcher` then prefers a previously-recorded value for a WMI property whenever the
current query still offers that value among its candidates — so a driver or firmware reporting change
that reshuffles the candidate list does not silently change the HWID:

```csharp
locals._ParameterContext = EditorPrefs.GetString("DSLICINF", string.Empty);
…
locals._ParameterContext = PushIdentifier(locals._ParameterContext);   // decrypt; on failure, delete the key
```

If decryption throws, the key is deleted and collection proceeds without the memo.

---

## 12. AES Cipher Details

Two AES-128-CBC key/IV pairs exist. Both are hardcoded in plain text in all three assemblies (not
further obfuscated beyond .NET Reactor's string encryption, which no longer covers them — §22).

### Session-cache key (per-field encrypt/decrypt — `ListIdentifier` / `SearchIdentifier`)

| Parameter | Value (Base64) |
|---|---|
| Key | `LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=` |
| IV | `MEZqk6gCgPTwifeH3YrTlQ==` |

Used by **ADOverhaul only** (2019 and 2022), for the individual SessionState field values of §11.1.

### DSLICINF blob key (outer envelope — `PatchIdentifier` / `CallIdentifier`)

| Parameter | Value (Base64) |
|---|---|
| Key | `3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=` |
| IV | `MTOuc+v23iVKtf8SLX3WxQ==` |

Used for encrypting / decrypting the full DSLICINF blob before/after the permutation shuffle in both
products — and, in **ControllerEditor only**, also for the SessionState field values. ControllerEditor
does not reference the `LWw2tFi+…` pair at all.

| Pair | ADOverhaul (2019 + 2022) | ControllerEditor |
|---|---|---|
| `LWw2tFi+…` / `MEZqk6gC…` | SessionState per-field cache | not used |
| `3epqD3d1…` / `MTOuc+v2…` | DSLICINF blob | DSLICINF blob **and** SessionState per-field cache |

---

## 13. DSLICINF Encryption Pipeline

The DSLICINF value (the hardware-info memo of §11.3) is protected by a two-layer transform:
**AES-128-CBC** followed by a **character-position permutation shuffle**.

### Encryption (write path — `StopIdentifier` in ADO, `CreateMapper` in CE)

```
plaintext WMI property dump
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
plaintext WMI property dump
```

### Permutation Function (`RegisterIdentifier` / `ReflectMapper` → `ChangeIdentifier`)

`RegisterIdentifier` applies `ChangeIdentifier` once per offset in the array, skipping non-positive
strides. The permutation cycles through the string with a counter, swapping characters at periodic
intervals:

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

The ADO offsets are hard-coded literals: `[3, 2, 6, 4, 2, 1, 8]` (write) and `[8, 1, 2, 4, 6, 2, 3]`
(read). The CE offsets are loaded from encrypted `smethod` calls — `smethod_3<int[]>(565931375)` for
write, `smethod_1<int[]>(943980522)` for read.

> **Unverified:** that CE's two arrays resolve to the same values as ADO's. Settling it needs a
> runtime dump of CE's `byte_0` blob at those two offsets (`dotnet_reactor.py dump` against a fresh
> runtime capture); the retained string dumps cover strings only. This matters — if CE's offsets
> differ, DSLICINF blobs are not interchangeable between the two products (§20).

### SessionState Key Derivation (`ForgotIdentifier` / `UpdateIdentifier`)

```csharp
internal static string ForgotIdentifier(string reference, ref … counter)
    => Convert.ToBase64String(counter.m_PoolContext.ComputeHash(Encoding.UTF8.GetBytes(reference)));
```

This is **not** an integrity digest — nothing is ever compared against it. It is called only as
`ForgotIdentifier(itemContext + key, …)` and the result is passed straight to
`SessionState.GetString` / `SessionState.SetString` as the entry name. `UpdateIdentifier` is the
identical write-side twin.

The HMAC-SHA1 key is the SessionState session key
`"<productId>" + EditorAnalyticsSessionInfo.id`, which is what makes the entry names change every
editor session. Tamper detection is the license token of §10, not this.

---

## 14. Full Verification Flow — Step by Step

```
Unity domain reload / project load
    │
    ▼
[InitializeOnLoadMethod] DisableConfiguration() / VerifyAnnotation()
    │
    ├─ read license key from EditorPrefs
    ├─ licenseKeyEntryRequired = (key is missing)      ← true when there is NO key
    └─ if key present and verify-on-load enabled → schedule AssetConfiguration()
       (method always runs to completion either way)
                            │
                            ▼
                      AssetConfiguration(bool testkey)   [CE: WriteAnnotation]
                            │
                            ├─ return early if: verification disabled, or
                            │       (licenseKeyEntryRequired && !licenseCheckRetryOffered), or
                            │       isVerifyingLicense
                            ├─ licenseCheckRetryOffered = false
                            ├─ isVerifyingLicense = true, licenseCheckedThisSession = true
                            ├─ itemContext = "<productId>" + EditorAnalyticsSessionInfo.id
                            │
                            ├─ read SessionState.GetBool(itemContext)
                            │       │
                            │       ├─ TRUE:
                            │       │    read the 5 fields back from SessionState
                            │       │    (HMAC-SHA1-derived key names, AES-decrypted values)
                            │       │    compare "date" field to today's UTC date
                            │       │       ├─ MATCH → load u/v/r/m
                            │       │       │          isLicensed = true ✓
                            │       │       │          flush licensed callbacks (token-gated, §10)
                            │       │       │          repaint windows
                            │       │       └─ MISMATCH → load nothing
                            │       │    (any exception → warn "failed to verify from cache.")
                            │       └─ FALSE: (skip cache)
                            │
                            └─ CloneConfiguration(callback, ispred: true)   ← ALWAYS runs
                                    │
                                    ├─ read + decrypt DSLICINF from EditorPrefs
                                    │  (hardware-info memo; seeds per-property value preference)
                                    │
                                    ├─ run 4 sequential WMI subprocesses (10s budget,
                                    │   PowerShell fallback gets its own 10s)
                                    │
                                    ├─ append "\r\r" to each category, assemble the 3 SHA1 segments,
                                    │   rewrite DSLICINF
                                    │
                                    └─ callback() → unconditionally send verifylicense
                                            │
                                            ├─ success=false →
                                            │      show dialog with server message
                                            │      isLicensed remains false ✗
                                            │
                                            └─ success=true →
                                                   ├─ compare response "date" to local date
                                                   │      MISMATCH → log, tear down, stop ✗
                                                   ├─ populate u/v/r/m from response
                                                   ├─ isLicensed = true
                                                   ├─ write the 5 fields to SessionState
                                                   ├─ SessionState.SetBool(itemContext, true)
                                                   ├─ verify the license token (§10)
                                                   │      MISMATCH → tear everything down ✗
                                                   └─ flush licensed callbacks (token-gated again) ✓
```

---

## 15. License State Fields

### ADOverhaul (class `ADOverhaul`; de4dot `IdentifierSerializerConnector` / 2019 `ConfigurationTestStub`)

| Field (de4dot name) | Type | Purpose |
|---|---|---|
| `isLicensed` (`_Service`) | `bool` | **PRIMARY LICENSE GATE** — but see §10; it is necessary, not sufficient |
| `wasLicensedBeforeReset` (`_Reponse`) | `bool` | License was already granted before this teardown/re-check |
| `licensedCallbacksFlushed` (`specification`) | `bool` | License-grant callback chain already fired |
| `pendingLicensedCallbacks` (`m_Wrapper`) | `Action` | Callback chain invoked when license is granted |
| `licenseKeyEntryRequired` (`_Worker`) | `bool` | **True when the license key is absent** (i.e. the user must enter one) |
| `isVerifyingLicense` (`_Struct`) | `bool` | Verification in progress (network pending) |
| `licenseCheckedThisSession` (`m_Pool`) | `bool` | A license check has already been run this session |
| `isActivatingLicense` (`_Rule`) | `bool` | Activate button was clicked |
| `serverWarnedTooManyAttempts` (`manager`) | `bool` | Rate limit warning active |
| `feedbackPanelOpen` (`m_Algo`) | `bool` | Feedback panel toggle |
| `licenseCheckRetryOffered` (`indexer`) | `bool` | "Retry" latch — re-permits `AssetConfiguration` after a failure |
| `showingTransferPanel` (`_Info`) | `bool` | License transfer panel is open |
| `transferCodeSent` (`m_Config`) | `bool` | Verification code has been sent |
| `isRequestingTransferCode` (`_Mock`) | `bool` | Step 1 of transfer in flight |
| `isConfirmingTransfer` (`state`) | `bool` | Step 2 of transfer in flight |
| `licenseKey` (`m_Repository`) | `string` | License key (Gumroad format) |
| `sessionId` (`global`) | `string` | SID from EditorPrefs |
| `hardwareId` (`attr`) | `string` | Raw HWID (3-segment SHA1) |
| `unreadDeviceDateFingerprint` (`_Interpreter`) | `string` | Scrambled HWID — dead code, never read (§5) |
| `licenseUsername` (`listener`) | `string` | Username, from the server's `username` field |
| `licensedToDisplayName` (`_Parser`) | `string` | Display name, derived from the username by `InstantiateConfiguration` |
| `licenseVariant` (`m_Printer`) | `string` | License variant / tier, from `variant` |
| `licenseToken` (`_Object`) | `string` | The license token of §10, from `token` |
| `currentDateStamp` (`setter`) | `string` | Today's UTC date, `DD/MM/YYYY` |
| `transferTargetEmail` (`m_Strategy`) | `string` | Email address returned by `transferlicenserequest` |
| `transferVerificationCode` (`descriptor`) | `string` | The 6-digit code the user types |
| `retryAllowedAtRealtime` (`_System`) | `float` | Rate-limit end time (`Time.realtimeSinceStartup`) |
| `version` (`m_Expression`) | `SemVer` | Plugin version (e.g. `"0.11.1"`) |

### ControllerEditor (fields on the `ControllerEditor` class itself)

| Field (de4dot name) | Type | Purpose |
|---|---|---|
| `isLicensed` (`listenerAnnotation`) | `bool` | **PRIMARY LICENSE GATE** — the CE counterpart of ADO's `isLicensed` |
| `licenseCheckedThisSession` (`m_DispatcherAnnotation`) | `bool` | A license check has already been run this session |
| `licenseKeyEntryRequired` (`m_IdentifierAnnotation`) | `bool` | **True when the license key is absent** |
| `isVerifyingLicense` (`_RequestAnnotation`) | `bool` | Verification in progress |
| `isActivatingLicense` (`importerAnnotation`) | `bool` | Activate button was clicked |
| `licenseCheckRetryOffered` (`attrAnnotation`) | `bool` | "Retry" latch |
| `licenseRestoredFromCache` (`m_GetterAnnotation`) | `bool` | License came from the SessionState cache, not the network |
| `licensedCallbacksFlushed` (`m_InterceptorAnnotation`) | `bool` | Callback chain already fired |
| `pendingLicensedCallbacks` (`creatorAnnotation`) | `Action` | Callback chain invoked when license is granted |
| `pendingResetCallbacks` (`m_EventAnnotation`) | `Action` | Callback chain invoked on teardown |
| `serverWarnedTooManyAttempts` (`m_ExporterAnnotation`) | `bool` | Rate limit warning active |
| `feedbackPanelOpen` (`m_ExpressionAnnotation`) | `bool` | Feedback panel toggle |
| `showingTransferPanel` (`facadeAnnotation`) | `bool` | Transfer panel is open |
| `transferCodeSent` (`advisorAnnotation`) | `bool` | Verification code has been sent |
| `isRequestingTransferCode` (`m_CallbackAnnotation`) | `bool` | Step 1 of transfer in flight |
| `isConfirmingTransfer` (`_IndexerAnnotation`) | `bool` | Step 2 of transfer in flight |
| `licenseKey` (`m_BridgeAnnotation`) | `string` | License key |
| `sessionId` (`databaseAnnotation`) | `string` | SID |
| `hardwareId` (`_WriterAnnotation`) | `string` | HWID |
| `unreadDeviceDateFingerprint` (`printerAnnotation`) | `string` | Scrambled HWID — dead code (§5) |
| `licenseUsername` (`m_FilterAnnotation`) | `string` | Username, from `username` |
| `licensedToDisplayName` (`stubAnnotation`) | `string` | Display name derived from the username |
| `licenseVariant` (`m_ReaderAnnotation`) | `string` | License variant / tier, from `variant` |
| `licenseToken` (`m_ParamsAnnotation`) | `string` | The license token of §10, from `token` |
| `currentDateStamp` (`m_TagAnnotation`) | `string` | Today's UTC date, `DD/MM/YYYY` |
| `transferTargetEmail` (`m_CustomerAnnotation`) | `string` | Email returned by `transferlicenserequest` |
| `transferVerificationCode` (`strategyAnnotation`) | `string` | The 6-digit code the user types |
| `retryAllowedAtRealtime` (`m_RegistryAnnotation`) | `float` | Rate-limit end time |

### The date stamp

Both products build `currentDateStamp` the same way — **day first**:

```csharp
private static string RemoveConfiguration()   // CE: PatchAnnotation
{
    string d = EnableIdentifier(DateTime.UtcNow.Day.ToString());     // zero-padded to 2 digits
    string m = EnableIdentifier(DateTime.UtcNow.Month.ToString());
    string y = DateTime.UtcNow.Year.ToString();
    currentDateStamp = d + "/" + m + "/" + y;
    return currentDateStamp;
}
```

`DD/MM/YYYY`. This value is compared against the server's `date` field (§9) and is the first half of
the license token's HMAC input (§10), so getting the order wrong breaks both.

---

## 16. EditorPrefs and SessionState Keys

### EditorPrefs (persistent across Unity restarts)

| Key | Product | Content |
|---|---|---|
| `"No1lKII9IzcBAbihub6nCg==LK"` | ADOverhaul | License key (plaintext) |
| `"yOk0XCnENLMO6DIF8cYpSg==LK"` | ControllerEditor | License key (plaintext) |
| `"ADOverhaulLicenseField"` | ADOverhaul | GUI license field backup |
| `"Controller EditorLicenseField"` | ControllerEditor | GUI license field backup |
| `"DreadScriptssid"` | Both | 32-char hex session ID |
| `"DSLICINF"` | Both | AES+permutation encrypted **WMI hardware-info dump** (§11.3) — no license data |
| `"No1lKII9IzcBAbihub6nCg==SettingsJSON"` | ADOverhaul | Plugin settings (JSON blob) |
| `"yOk0XCnENLMO6DIF8cYpSg==SettingsJSON"` | ControllerEditor | Plugin settings (JSON blob) |

### SessionState (cleared on Unity Editor restart; survives domain reload)

| Key pattern | Content |
|---|---|
| `"<productId>" + EditorAnalyticsSessionInfo.id` | Bool: the license was verified in this editor session |
| `Base64(HMACSHA1(key: sessionKey, sessionKey + "date"/"u"/"v"/"r"/"m"))` | String: the AES-encrypted license field (§11.1) |
| `"<productId>updateinfo"` | String; erased when the user forces an update re-check |

where `<productId>` is `No1lKII9IzcBAbihub6nCg==` (ADO) or `yOk0XCnENLMO6DIF8cYpSg==` (CE).

> **Unverified:** `"<productId>updateinfo"` only ever appears in an `EraseString` call in all three
> assemblies — there is no write path in these builds, so its intended contents are unknown. It looks
> like a leftover from an earlier update-caching scheme; the update info now lives in the settings
> JSON. A build in which it is written would settle it.

`SessionState.EraseBool(sessionKey)` is called on logout/deactivation — `EnableWatcher` in ADO,
`SearchAnnotation` in CE.

---

## 17. Rate Limiting and Anti-Abuse

The server can respond with rate-limit metadata:

- `"wait_warn": true` — sets `serverWarnedTooManyAttempts = true` (displays a warning: *"Too many failed attempts! Further failed attempts will result in getting your device blocked!"*)
- `"wait_time": <seconds>` — sets `retryAllowedAtRealtime = Time.realtimeSinceStartup + wait_time` (blocks activation button until elapsed)

```csharp
private static float ResolveSerializer() => retryAllowedAtRealtime - Time.realtimeSinceStartup;
private static bool GetSerializer()      => ResolveSerializer() > 0f;
```

The display during cooldown:
```
"Please wait N seconds."
```

---

## 18. License Transfer / 2FA Flow

To move a license to a new device, the DLL implements a two-step transfer flow:

**Step 1 — Request transfer:**
- User clicks *"Transfer License"* in the DRM UI and confirms a Terms-of-Service dialog
- POST with `command = "transferlicenserequest"` (full auth payload)
- Server responds with `transfer_email`; the client sets `transferCodeSent` and shows
  *"A 6-digit verification code was sent to \<email\>."*

**Step 2 — Confirm with code:**
- User enters the code from their email
- POST with `command = "transferlicenseconfirm"` (full auth payload + a `verification_code` field)

The code is **numeric in practice**. The text field strips every non-digit
(`Regex.Replace(code, "[^0-9]", "")`) and the confirm button is gated on `[0-9]{6}`. A separate
validation helper does use `^[a-zA-Z0-9]{6}$`, but no non-digit can reach it through the UI.

The ToS dialog states: *"License will stop working on the device it was previously activated on. You
will not be able to transfer back or again for 30 days."*

> **Unverified:** the 30-day block is server-side behaviour. The client only *displays* the statement;
> nothing in the assemblies enforces or observes it. Only server logs or an empirical transfer against
> a live backend would confirm it.

---

## 19. Product IDs

| Product | Base64 ID | In requests (`product_id` field) | In EditorPrefs key suffix |
|---|---|---|---|
| ADOverhaul | `No1lKII9IzcBAbihub6nCg==` | `No1lKII9IzcBAbihub6nCg==` | `No1lKII9IzcBAbihub6nCg==LK` |
| ControllerEditor | `yOk0XCnENLMO6DIF8cYpSg==` | `yOk0XCnENLMO6DIF8cYpSg==` | `yOk0XCnENLMO6DIF8cYpSg==LK` |

> The `LK` suffix appears only in EditorPrefs keys, never in HTTP payloads.

---

## 20. Per-Product Differences: ADOverhaul vs ControllerEditor

| Aspect | ADOverhaul 2022 | ControllerEditor |
|---|---|---|
| DRM class | `ADOverhaul` (the main plugin class) | `ControllerEditor` (the main plugin class) |
| Primary license flag | `isLicensed` | `isLicensed` |
| License key EditorPrefs | `"No1lKII9IzcBAbihub6nCg==LK"` | `"yOk0XCnENLMO6DIF8cYpSg==LK"` |
| Product ID | `No1lKII9IzcBAbihub6nCg==` | `yOk0XCnENLMO6DIF8cYpSg==` |
| HMAC secret | ADO-specific (see §7) | CE-specific (see §7) |
| HTTP client | `HttpWebRequest` | `HttpWebRequest` |
| Session-cache AES keys | `LWw2tFi+…` / `MEZqk6gC…` | `3epqD3d1…` / `MTOuc+v2…` (the DSLICINF pair) |
| DSLICINF blob AES keys | `3epqD3d1…` / `MTOuc+v2…` | `3epqD3d1…` / `MTOuc+v2…` |
| Permutation offsets (write) | `[3, 2, 6, 4, 2, 1, 8]` (literal) | `smethod_3<int[]>(565931375)` |
| Permutation offsets (read) | `[8, 1, 2, 4, 6, 2, 3]` (literal) | `smethod_1<int[]>(943980522)` |
| SessionState key prefix | `"No1lKII9IzcBAbihub6nCg=="` | `"yOk0XCnENLMO6DIF8cYpSg=="` |
| Settings JSON key | `"No1lKII9IzcBAbihub6nCg==SettingsJSON"` | `"yOk0XCnENLMO6DIF8cYpSg==SettingsJSON"` |

Both products share the same `"DreadScriptssid"` EditorPrefs key, meaning the SID is the same across
products on the same machine.

Both products also write the same `"DSLICINF"` key, and both use the same cipher for the blob — so
whichever product ran most recently owns the hardware-info memo, and the other can read it (subject to
the permutation-offset question in §13). This has no licensing consequence: DSLICINF carries no
license data, only WMI property values. License state lives in per-product SessionState entries and is
never shared.

---

## 21. ADOverhaul 2019 vs 2022 Differences

| Aspect | ADOverhaul 2019 | ADOverhaul 2022 |
|---|---|---|
| DRM class (de4dot name) | `ConfigurationTestStub` | `IdentifierSerializerConnector` |
| HTTP client | `HttpWebRequest` | `HttpWebRequest` |
| HMAC secret | **Identical** to 2022 | `of,ejcX?$0 &n*Uc{...}` |
| HMAC algorithm | HMACSHA256 | HMACSHA256 |
| Product ID | `No1lKII9IzcBAbihub6nCg==` (same) | `No1lKII9IzcBAbihub6nCg==` |
| smethod transform A/B | Different constants (see below) | Different constants (see below) |
| Module GUID | `{CAD6ED8D-8CDE-4E08-A19D-89CBC52DD07C}` | `{7907DD2F-A0A5-4805-95CD-D1B3741C5FB4}` |

Both classes are renamed `ADOverhaul` in the decompiled sources. The DRM logic, EditorPrefs keys,
cache format, HWID algorithm, license token and server endpoint are identical between 2019 and 2022;
only the generated method names differ.

> An earlier revision of this table listed de4dot output sizes (234 KB / 242 KB). That row has been
> removed rather than refreshed: no de4dot output is retained anywhere, and the figure moves with
> every deobfuscator update, so it is not reproducible and is not a fact about the DRM.

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

## 22. Obfuscation (.NET Reactor)

All three DLLs are packed with **.NET Reactor**.

### String Encryption (`smethod_N`)

.NET Reactor replaces constants with calls to generic decrypt methods on `<Module>`:

```csharp
<Module>.smethod_3<string>(-1234567)
```

The current deobfuscation pipeline resolves nearly all of these. **Three** unresolved `<Module>.smethod_N`
calls survive in the decompiled sources, all in ControllerEditor, and none of them is a string:

```
smethod_3<int[]>(565931375)     // DSLICINF permutation offsets, write (§13)
smethod_1<int[]>(943980522)     // DSLICINF permutation offsets, read  (§13)
smethod_5<float[]>(1991865236)  // SupporterEntry splitter state
```

Both ADOverhaul assemblies have zero — which is why the AES keys and HMAC secrets in this document
appear as plaintext literals in the decompiled sources.

**Decryption transform:**
```csharp
int transformed = (key * A) ^ B;            // int32 overflow arithmetic
int tag         = transformed >>> 30;       // 4 possible values
int offset      = (transformed & 0x3FFFFFFF) << 2;
// offset indexes into byte_0 (the encrypted constant blob)
```

The tag selects one of three payload shapes — string, blittable scalar, or array — and **the mapping
is permuted per method**, so it cannot be read off once and reused:

| | string | scalar | array |
|---|---|---|---|
| CE `smethod_1` | 2 | 1 | 3 |
| CE `smethod_2` | 1 | 3 | 2 |
| CE `smethod_3` | 3 | 2 | 0 |

Unlisted tag values fall through to `default(T)`.

### Anti-External-Caller Guard

Every `smethod_N` method checks:
```csharp
if (!Assembly.GetExecutingAssembly().Equals(Assembly.GetCallingAssembly()))
    return default(T);
```

This blocks direct external invocation. String extraction was performed using `DynamicMethod`
invocation (immune to the check) and offline IL scanning.

### Sentinel Fields (Cosmetic)

Many classes carry a cosmetic null-check pattern — an always-null `object` static and a predicate
that is therefore always true:

```csharp
internal static object LogoutTokenizer;   // always null
internal static bool CreateTokenizer() => LogoutTokenizer == null;  // always true
```

`scripts/find_reactor_decoys.py` counts 60 surviving pairs across the three assemblies (11 / 10 / 37),
of which 58 are dead. They gate no execution path.

> **Do not pattern-match on shape alone.** The license-token check of §10 lives on a compiler-generated
> `<>c` singleton and *looks* like one of these decoys. It is not; removing it removes the license
> enforcement.

### Control Flow Obfuscation

Switch-based CFG obfuscation, in the shape:
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

Most of this is now resolved by the deobfuscation pipeline; what remains shows up as residual
`goto IL_` in a handful of files. A count is deliberately not stated here — it changes with every
deobfuscator update and is a property of the tooling, not of the DRM.

---

## 23. Restoration / Bypass

Since the backend is offline and returns `success=false`, no license is ever granted under normal
circumstances.

> **Abandoned:** A runtime Harmony-patch approach (`ADORestorationPatch` / `CERestorationPatch`) was
> prototyped but has been removed. **No patches are maintained.** The DRM field maps documented
> elsewhere in this file record which fields gate each license (`isLicensed` in both products) for
> reference only.

### What a working restoration has to produce

Returning `success: true` is not enough. A response must additionally:

1. Carry a `date` field equal to the client's local UTC date in `DD/MM/YYYY` (§9), or activation aborts
   with a "Date Mismatch!" error.
2. Carry a correct `token` (§10): `Base64(HMACSHA256(key = productSecret + licenseKey, data = date + HWID))`,
   using the product's own secret from §7. Without it, `isLicensed` is set and then immediately torn
   down, and no licensed callback ever fires.
3. Echo whatever `username` / `variant` the UI should display; these are cosmetic.

### Mock Server

`drm_server/` — a Go HTTPS server that accepts all requests and returns `success: true` together with
a correctly computed `date` and `token`. To use:
1. `drm_server.exe patch-hosts` (or manually add `127.0.0.1  us-central1-dreadscripts-c6b62.cloudfunctions.net` to the hosts file)
2. `drm_server.exe install-cert` (installs the self-signed TLS cert into the Windows Trusted Root CA store)
3. `drm_server.exe serve` (requires port 443 admin access, or `serve --addr :8443`)

Or run `install.ps1` from an elevated prompt to do all of the above plus register it as an auto-start
Windows service in one step. See `drm_server/README.md` for full details.

### Synthetic session cache (Option C — not viable externally)

Priming the cache by hand would mean, for session key
`sessionKey = "<productId>" + EditorAnalyticsSessionInfo.id`:

1. Compute the token: `r = Base64(HMACSHA256(key = productSecret + licenseKey, data = date + HWID))`
2. For each of `date`, `u`, `v`, `r`, `m`:
   - entry name = `Base64(HMACSHA1(key = sessionKey, sessionKey + fieldName))`
   - entry value = `Base64(AES-128-CBC-encrypt(fieldValue))` with the product's session-cache key pair (§12)
   - `SessionState.SetString(name, value)`
3. `SessionState.SetBool(sessionKey, true)`

**This cannot be done from outside the editor.** The original form of this idea assumed the cache lived
in `EditorPrefs` (the Windows registry), which an external tool can write. It does not — it lives in
`SessionState`, which is in-memory in the running Unity Editor process and is not addressable from
another process. Only a C# editor script running inside the same session could do it, and such a
script would still have to know `EditorAnalyticsSessionInfo.id`, the license key and the HWID. The mock
server is the practical route.

---

## 24. Captured Real Requests

The following requests were captured via the local mock server from both DLLs running in Unity Editor.

> **Unverified provenance.** These payloads are structurally consistent with `CountConfiguration` +
> `IncludeConfiguration` and the field order matches, but the original capture logs are not retained,
> so the concrete values cannot be re-derived from the assemblies alone. A fresh `drm_server` request
> log would re-establish them.

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
- `sendfeedback`'s HWID is empty because no WMI collection has run, not because the command omits the
  field; `license_key` is empty in this capture only because no key was stored at the time
- `getdownloadinfo` sends no HWID, SID, license_key, or hash — server doesn't require auth for update checks
- Both products share the same HWID and SID on the same machine (same hardware, same `DreadScriptssid` key)
- The same license key produces different hashes for ADO vs CE because of different HMAC secrets
