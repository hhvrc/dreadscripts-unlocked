# Reverse Engineering Notes

*Self-notes for AI sessions. Last updated 2026-07-30 — de-duplicated against the DRM spec and
`AGENTS.md`, and stale de4dot statuses corrected.*

**What this file owns:** reconstruction status (what has been rebuilt from which export class, and how
far it has been verified), why each name was chosen, non-DRM decrypted strings, obfuscation-pattern
reference, and the session log. It does **not** own:

| Not here | Lives in |
|---|---|
| The DRM protocol, crypto, keys, cache format, wire captures | [`../DRM.md`](../DRM.md) — published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md) |
| Repository layout, file counts, toolchain, scripts | [`AGENTS.md`](AGENTS.md) |
| The obfuscated → chosen-name map itself | `renames/{Assembly}.json` |
| de4dot's own state, gates and open bugs | `../de4dot/ROADMAP.md` |

Link to those rather than summarising them here. Every contradiction this file has accumulated came
from a well-meant local copy of a fact owned somewhere else.

---

## Project Status Summary

Decompiling two discontinued Unity Editor plugins (ADOverhaul, ControllerEditor) by DreadScripts.
Both use **.NET Reactor** obfuscation. The backend validation server is permanently offline.
Goal: understand the DRM, and restore the tools to a runnable state without it.

**String extraction complete** for both assemblies:
- ControllerEditor — 1318 strings via `Hooks.cs` runtime scan → `dumps/intercepted.txt`
- ADOverhaul2022 — 740 strings via `SmethodStringDumper` offline dump → `dumps/ADOverhaul2022.txt`

**Deobfuscation pipeline established (re-run 2026-03-02):**
- `binaries/ADOverhaul2019.dll` — 790KB original (March 2024 build; not previously analyzed)
- `binaries/ADOverhaul2022.dll` — 779KB original (March 2024)
- `binaries/ControllerEditor.dll` — 1349KB original (August 2024)
- `work/deobf/ADOverhaul2019-deobf.dll` — de4dot output (234KB)
- `work/deobf/ADOverhaul2022-deobf.dll` — de4dot output (242KB; proxy delegates inlined)
- `work/deobf/ControllerEditor-deobf.dll` — de4dot output (689KB, from March 1 run; 2026-03-02 run crashed with exit code 1 after 2+ hours on CFG cleaning — CE has 793 switch/XOR patterns, extremely slow)

`export/` directories and their file counts are listed in
[`AGENTS.md`](AGENTS.md#repository-layout) — not duplicated here. The copy that used to sit in this
list had already drifted (it claimed 16 `.cs` for `ADOverhaul2019` against an actual 15), and
`export/` is regenerated wholesale by `scripts/reexport.py`, so any count written by hand goes stale
the next time that runs.

**What de4dot does and does NOT do** *(as of 2026-07-30 — the local `../de4dot` fork has moved a long
way past what this list originally said; check `../de4dot/ROADMAP.md` §1 for the current measured
state rather than trusting a summary here)*:
- ✅ Extracts the inner .NET assembly from the .NET Reactor outer loader (779KB → 339KB for ADO, 1348KB → 810KB for CE)
- ✅ Renames obfuscated symbols (class/method/field names) to readable de4dot names
- ✅ Inlines proxy delegates — `GetExecutingAssembly()` / `GetCallingAssembly()` / `Equals()` are now direct IL calls (no more `TestsRef::IncludeInterceptor` wrapper)
- ✅ **Decrypts `smethod_N` strings.** This entry used to read "does NOT — blocked by the
  `GetCallingAssembly()` caller check". That is out of date: the fork runs the target's `.cctor` in a
  confined worker process and reads the data blob directly, so the caller check is bypassed rather than
  defeated. Coverage is a standing gate on the fork side.
- ⚠️ **Partially resolves switch/XOR CFG obfuscation.** Not "does NOT" any more, but not fully either:
  dispatches it cannot resolve *faithfully* are now deliberately left unresolved rather than
  half-rewritten, because partial resolution silently deletes the unreachable tail of a state machine.
  Practical consequence for reconstruction is unchanged — see `resolving-residual-control-flow-manually`
  and run `scripts/detect_broken_state_machines.py` before trusting a `while (true) { switch }` body.

**smethod_N structure (identical in both assemblies, after de4dot inlining):**
```
IL_0000: call   Assembly::GetExecutingAssembly()
IL_0005: call   Assembly::GetCallingAssembly()
IL_000A: callvirt  Object::Equals(object)
IL_000F: brfalse   <bad-path/return-default>
IL_0014: br.s      <good-path/decryption-state-machine>
```
String decryption is guarded — only callable from within the assembly itself. External callers get `default(T)` back.

---

## What Has Been Completed

**The reconstruction this section used to describe no longer exists.** It listed ~96 source files in
`dreadre-devel/` with per-file "✅ Clean 1:1 / Discrepancies fixed" status. That tree was retired on
2026-07-31: it had drifted from `export/`, and rather than reconcile it file by file, the decision was
to re-port every file from `export/` and polish it on the way in. The Unity project moved to
`../public/unity/` and its sources were dropped.

The old table is deleted rather than kept as history, deliberately. It claimed completion for files
that are gone, and its "export origin" column named obfuscated types (`MerchantPolicy`, `Info`,
`Annotation`) that the rename maps have since renamed — so every column of it was a trap for anyone
reading it as current.

**What replaced it:**

| Question | Where the answer lives |
|---|---|
| What has been ported, what is left | `python3 scripts/port_status.py` — diffs `export/` against the package |
| Which files are excluded, and why | the same script; it owns that policy |
| Which obfuscated type is which | `renames/{Assembly}.json` |
| How much of a type is named | `python3 scripts/rename_status.py` |
| Does the ported source compile | `python3 scripts/typecheck_package.py` |

None of those are written down by hand, because the previous table's failure mode was precisely that
someone had to remember to update it and eventually did not. Do not reintroduce a hand-kept
per-file status list here.

The four early reconstruction discrepancies this section also recorded (`DelegateBinder`,
`EditorGuiUtils`, `TemporaryTransform`, `WebRequestJob` — wrong accessibility, a missing null check,
a missing `#if`) are gone with the files. They are worth knowing only as a shape of mistake to expect
when porting: the decompiler renders accessibility, sealedness and conditional compilation
imperfectly, so check those explicitly rather than trusting the rendered signature.

### Shapes of decompile damage seen repeatedly in the port

These recur often enough to be worth looking for deliberately rather than noticing by luck. None are
findable by compiling — every one of them compiles.

- **A non-terminating `while` where a single statement belongs.** This is how an unresolved
  control-flow construct renders, and it is a hang, not a slowdown. Found in
  `EditorGuiUtils.CloneWrapper`, `TypeResolver`'s type lookup, `CachedTextureContent`'s texture
  getter, `BatchOperationContext.Reset`, `SceneViewExtensions`' y-offset, `ValueSettings.DrawSlider`
  and `AnimatorTypeCache.ParameterEntry.Source`. In each case the body was meant to run once.

  **No longer a suspicion — confirmed on 2026-08-04.** The `ParameterEntry.Source` case was settled
  by tracing the original obfuscated IL (`ControllerEditor.dll`, `AttrProperty::TestPage`, RVA
  0x75f78), an XOR-switch state machine whose dispatch gives condition-false → body once → return and
  condition-true → return. The shipped code is a plain `if`; de4dot invented the backward branch. The
  tell is that sibling methods Reactor never flattened decompile to a correct forward branch, and the
  flattened ones sit at a very different RVA with a `.try`/`catch Object` wrapper.

  So: treat every non-terminating `while` in `export/` as this fault, but confirm each one against
  the original IL or a second build rather than assuming. Where the ported source therefore deviates
  from `export/`, it carries a `DEOBF-BUG` marker — `grep -rn DEOBF-BUG` over the package lists every
  such site, and the `renaming-and-documenting-deobfuscated-source` skill defines the convention and
  the `(resolved)` / `(guessed)` distinction.
- **`[SpecialName]` methods are properties or indexers that did not get re-formed.** A
  `[SpecialName] T FooRecord()` was a getter; a `[SpecialName]` pair over the same name was a
  property; `[DefaultMember("Item")]` plus a `[SpecialName] Item(int)` was an indexer. Restore them —
  this is common enough that it is worth grepping for `SpecialName` before starting a file.
- **Mangled explicit interface implementations.** A member named
  `Ns_002EType_003CArg_003E_002Emember` is an explicit implementation whose interface ILSpy could not
  resolve. In `UtilityWindowBase` the interface (`CustomUtilityWindow<T>`) appears nowhere else in
  the assembly and plain abstract members were all the call sites needed.
- **Self-referential dead members.** `internal static object X;` paired with
  `internal static bool Y() { return X == null; }`, referenced by nothing else, is obfuscator
  scaffolding. Drop it.
- **A rename pass can leave a file *less* usable than before.** The 2026-07-31 re-export renamed
  `UtilityWindowBase`'s fields but left the method bodies referring to the old names, and emitted an
  invalid `base._002Ector()`. When a re-exported file looks worse rather than better, prefer the
  earlier reading over re-deriving from the new one.

### Names chosen during the port that are not yet in `renames/`

The port has settled several names that the rename maps do not carry yet, so `export/` still shows
the old ones. They belong in `renames/ControllerEditor.json` so the next re-export produces them
directly — until then this is the only record, which is exactly the situation the renaming rule
exists to prevent.

| Export name | Chosen name | Evidence |
|---|---|---|
| `EditorUtils.WatcherProcessor` | `Contents` | holds ~90 `GUIContent` icons; matches `ADOEditorUtility.Contents` |
| `EditorUtils.BaseProcessor` | `Styles` | holds only `GUIStyle`s; matches `ADOEditorUtility.Styles` |
| `EditorUtils.DestroyError()` | the `contents` accessor | returns the singleton above |
| `EditorUtils.CalcError()` | the `styles` accessor | returns the singleton above |
| `EditorUtils.DisableQueue/InsertQueue/RestartQueue` | `Button` overloads | all delegate to the toggle-based button |
| `EditorUtils.ExcludeQueue/AddQueue/InvokeQueue/FindQueue` | `ToggleButton` overloads | return the new toggle value |
| `EditorUtils.InitQueue/VisitQueue` | `ToggleButtonChanged` | return whether the value changed |
| `EditorUtils.StartQueue` / `AwakeQueue` | `AddLinkCursor` / `AddCursor` | register cursor rects |
| `EditorUtils.PushQueue` / `NewList` | `IconContent` / `CachedIcon` | build the entries of `Contents` |
| `EditorUtils.CloneList` | `TrimTransparentBorder` | crops to the non-transparent bounding box |
| `EditorUtils.PushList` | `Grey` | builds a neutral grey from a 0-1 or 0-255 level |
| `EditorUtils.m_AlgoProcessor` | `accentColor` | `Color(1, 0.5, 0.7)`, used for hover highlights |

---

## Decrypted Strings — SupportThankies Module (ControllerEditor)

All strings confirmed from `dumps/intercepted.txt` byte_0 scan.

### EditorLayoutUtils (export: Definition)
| Constant | Value |
|---|---|
| `EditorGUILayoutTypeName` | `"UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"` |
| `GUILayoutOptionTypeName` | `"UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"` |
| `LayoutBeginMethodName` | `"BeginSplit"` |
| `LayoutEndMethodName` | `"EndLayoutGroup"` |

### SupportWindowAssets (Exception.cs)
| Purpose | Value |
|---|---|
| Banner URL | `"https://i.imgur.com/iHszIY3.png"` |
| Banner SessionState cache key | `"ds-supporters-main"` |
| Avatar URL | `"https://i.imgur.com/FMv1R6A.png"` |
| Avatar cache key | `"ds-supporters-kofi"` |

Note: `"https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png"` also exists (offset 32856) — likely a different context (not SupportThankies).

### SupporterEntry DSL keys (Rules.cs)
The DSL format is `<key=value>` — TryExtract pattern: `"<" + key + "=(.*?)>(?:<|$)"`
| Source field | DSL key |
|---|---|
| `AvatarImage` | `"bgimage"` |
| `ImageLayout` | `"bgtype"` |
| `BgColor` | `"bgcolor"` |
| `BorderColor` | `"bordercolor"` |
| `NameFragments` | `"name"` |
| `LeftFragments` | `"prefix"` |
| `RightFragments` | `"suffix"` |
| `NameColor` | `"namecolor"` |
| `Url` | `"onclick"` |
| `BadgeTitle` | `"tooltip"` |

TryExtract regex: `"<" + attributeKey + "=(.*?)>(?:<|$)"`

### TextFragment (Filter.cs)
- Inline image regex: `"<image=(.+?)>"`

### SupportWindow (SupportThankies.cs)
| Purpose | Value |
|---|---|
| Loading label | `"Loading supporters..."` |
| Error/title label | `"Failed to load supporters."` |
| Retry button | `"Retry"` |
| HorizontalScope style | `"in bigtitle"` |
| Ko-fi URL | `"https://ko-fi.com/dreadrith"` |
| Supporters data URL | `"https://storage.googleapis.com/dreadscripts-c6b62.appspot.com/Dreadscripts/Supporters.txt"` |
| ParseRawData delimiter 1 | `"\n"` |

---

## Decrypted Strings — ADOverhaul2022 (740 resolved via SmethodStringDumper)

Resolved from `dumps/ADOverhaul2022.txt`. **The DRM-relevant strings — EditorPrefs keys, server
command names, product IDs, AES key/IV pairs, and the license-key / SID / verification-code regexes —
live in the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md)).** They are not repeated here.

What is left below is reconstruction detail that has no home in the spec:

### GUILayoutUtils resolved strings (ADOverhaul-specific)
| smethod call | Resolved value |
|---|---|
| `smethod_3(193412188)` | `"UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"` |
| `smethod_2(351662802)` | `"UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"` |
| `smethod_5(-1101839931)` (BeginSplit method name) | needs verification from context |
| `smethod_4(-13555557)` (GUILayoutUtility method name) | needs verification from context |

---

## DRM Architecture

**Fully documented in the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md))** — the POST wire format, HMAC secrets and signing order, HWID
derivation, server commands and response fields, EditorPrefs/SessionState keys, the license-key
format, the transfer/2FA flow, the DSLICINF cache format and its full AES + permutation pipeline, and
the captured real requests from both DLLs.

That file is the single source of truth for all of it. This one used to carry a second copy, which
drifted: it described `mock_server/main.go`, a path that no longer exists, and it duplicated the AES
key/IV pairs where nothing could keep the two lists in step.

**Everything the RE originally set out to answer is answered there**, except one question, kept here
because it is still open:

- **Are the anti-debug checks purely cosmetic, or do they gate execution branches?** The `indexer`
  (troubleshoot) flag skips certain HWID validation checks. Appears cosmetic only — not confirmed.

---

## IdentifierSerializerConnector — Method Map

The DRM engine class; all state is static. **The field map (every state field, its type and role, for
both ADOverhaul and ControllerEditor) is in the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md)) § "License State Fields"** — it is more complete
there than the copy that used to be here.

What follows is the export-name → purpose map for its *methods*, which the spec does not carry and
which reconstruction needs:

Key methods:
| Method | Purpose |
|---|---|
| `AssetConfiguration(bool)` | Entry: reads DSLICINF cache then triggers HWID gather |
| `CloneConfiguration(Action, bool)` | HWID gather via WMI → fire callback when done |
| `LoginIdentifier()` | Build + sign + send `activatelicense` request |
| `QueryConfiguration(...)` | Parse JSON response, route success/error |
| `CountConfiguration(cmd, extras)` | Build request field list |
| `StartConfiguration(list)` | HMAC-SHA256 sign request fields |
| `OrderIdentifier(json)` | HTTP POST to Firebase endpoint |
| `RemoveConfiguration()` | Return today's date as `DD/MM/YYYY` (UTC) |
| `RestartConfiguration()` | Compute scrambled HWID (`_Interpreter`) |
| `ManageConfiguration()` | Ensure SID exists; generate random if not |
| `ResolveConfiguration(bool)` | Fire `m_Wrapper` callbacks if licensed |
| `ReflectConfiguration(Action)` | Register or immediately fire callback |
| `PushIdentifier(s)` | Decode DSLICINF: inverse-permute then AES-decrypt |
| `StopIdentifier(s)` | Encode DSLICINF: AES-encrypt then permute |
| `PatchIdentifier(s)` | AES-128 encrypt, DSLICINF outer envelope (key/IV in the spec) |
| `CallIdentifier(s)` | AES-128 decrypt (same keys) |
| `RegisterIdentifier(s, int[])` | Segment permutation cipher |
| `ListIdentifier(s, aes)` | AES-128 decrypt with cache read keys |
| `SearchIdentifier(s, aes)` | AES-128 encrypt with cache write keys |
| `ForgotIdentifier(s, hmac)` | HMAC-SHA1 per-field integrity hash |
| `CalculateIdentifier()` | Schedule UI repaint on main thread |
| `CalcIdentifier()` | Repaint all DRM-related windows |
| `PopConfiguration()` | Activate button handler |
| `AddConfiguration()` | Regex-validate license key format |
| `FindConfiguration()` | True if all required fields present and valid |

---

## ExceptionSingletonStruct — Structure

**Rename to:** `ADOEditorUtility` — monolithic static utility class for the editor plugin.

Purpose: GUI primitives, scene-view overlay windows, PhysBone gizmo rendering, bone-chain walking, VRC component cross-copy, reflection-based Unity Editor internals, deferred action queue, VRChat parameter metadata.

### Nested Types

| Type | Kind | Purpose |
|---|---|---|
| `PositionFlag` | `[Flags]` enum | 9-zone hit regions for overlay window resize handles |
| `ExporterServerStub` | class | Overlay window resize handle manager |
| `ExporterServerStub.RegistryRegDic` | struct | Single resize zone (rect + flag + cursor) |
| `SystemSerializer` | class (IDisposable) | Scene view overlay GUI scope |
| `InterpreterSerializer` | class | GUI icon/content singleton cache |
| `CreatorServerStub` | class | GUI style singleton registry |
| `EventCommands` | enum | Unity editor keyboard command names |
| `PageMethod` | struct | Sphere handle gizmo descriptor |
| `ParserWatcherRule` | sealed class | DreadBanner.png async downloader/cache |
| `EventMethod` | sealed class (IDisposable) | Texture2D pixel read/write scope |
| `SchemaMapping` | sealed class | Icon + tooltip GUIContent with SessionState cache |
| `TaskMethod` | struct | VRC component shape data snapshot for cross-copy |
| `StrategyAuthenticationFactory` | class | PhysBone bone chain tree builder |
| `ClientRegDic` | class | Single node in bone chain tree |
| `InstanceConsumerExporter` | readonly struct | VRChat parameter → PhysBone field mapping |

### PositionFlag Enum

```csharp
[Flags]
internal enum PositionFlag
{
    Middle      = 1,   Right  = 2,  Left   = 4,
    Top         = 8,   Bottom = 16,
    TopRight    = 32,  TopLeft = 64,
    BottomRight = 128, BottomLeft = 256,
    All = -1
}
```

### Key Static Fields

| Field | Value / Purpose |
|---|---|
| `_ObserverSerializer` | `Color(0.56f, 0.94f, 0.47f)` — green (used in TaskConsumerExporter scope) |
| `resolverSerializer` | `Color(0.7f, 0.3f, 1f)` — purple (used in TaskConsumerExporter scope) |
| `_AlgoSerializer` | `string[]` of 23 VRChat built-in avatar parameter names |
| `m_RoleSerializer` | `string[]` of 23 bone name prefixes |
| `m_VisitorSerializer` | `InstanceConsumerExporter[]` — 5 PhysBone output parameter descriptors |
| `getterSerializer` | `ParserWatcherRule` singleton — DreadBanner.png downloader |

### `ChangeStatus` Method

```csharp
internal static bool ChangeStatus(bool isitem, string ivk, GUIStyle rule = null, params GUILayoutOption[] options)
    => GUILayout.Toggle(isitem, new GUIContent(ivk), rule ?? "Button", options);
```
A toggle that renders as a button (pressed/unpressed).

---

## ControllerEditor DRM — entry point

**ControllerEditor DRM differs from ADOverhaul**: DRM state is embedded directly in the main
`ControllerEditor` class as private static fields, not in a separate DRM class. The field table is in
the DRM spec ([`../DRM.md`](../DRM.md)) § "License State Fields".

DRM entry point: `VerifyAnnotation()` — marked `[InitializeOnLoadMethod]`.
- Calls `ResolveAnnotation()` (sets up Harmony patches), then `AssetAnnotation()` (reads key from EditorPrefs)
- Checks `ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification` (DSLICINF cache result)
- If cache hit and key present: sets `m_DispatcherAnnotation = true`

TypeDef token for `ControllerEditor` class: `0x0200001F` (RID 31)

---

## Class Rename Map

**The map now lives in `renames/{Assembly}.json`** — the table that used to be here has been
removed. It duplicated the map by hand and had drifted: it listed `CreatorMethod -> MixedValueScope`
and `LoaderMethod -> FoldoutStateTracker` for classes that no longer exist in the export, and gave
two different answers for `TokenParamsDispatcher` and for `GUIColorScope`.

`renames/` is the single source of truth because the pipeline actually applies it: `ilrename` writes
those names into the assembly metadata before ILSpy runs, producing `export/`. A name that
only exists in prose here will not appear anywhere in the output. Each entry is keyed by metadata
token, so `ilrename report` can tell you when an entry stops matching the current export instead of
letting it rot silently.

See the "Renaming" section of `AGENTS.md` for the format and workflow. Keep using this file for
*why* a name was chosen and what has been verified against the export — that reasoning has no home
in a key/value map.

**Names recorded only in a `dreadre-devel/` file header do not reach the output.** 47 of them had
accumulated as `// Original name: X` comments in devel sources — real naming work, done and correct,
that `ilrename` never saw because the map is what the pipeline applies. They were transferred into
`renames/` on 2026-07-30 after checking each against the export rather than trusting the comment:
shared literals where the type had any (`"Rename Overlay Wrapper has failed to initialize!"`,
`"No Avatar Descriptors Found"`, `"BeginSplit"`, `"d_ol_plus"`), and behaviour where it did not —
every `IDisposable` scope was confirmed from what its constructor and `Dispose` actually save and
restore (`GUI.enabled`, `EditorGUI.showMixedValue`, `BeginScrollView`/`EndScrollView`,
`BeginHorizontal` + `FlexibleSpace`). Two headers did not survive that check and are recorded above.

If you name something in a devel header, put it in `renames/` in the same edit, or it is invisible.

A second pass took the nested types the same way. `dreadre-devel/.../EditorUtils.cs` carries explicit
`// originally:` comments, which are direct; the rest were matched by walking the devel class and the
export class in parallel and checking that kind, order and **inheritance** all line up —
`EnumSetting : FloatSetting` against `AttrAlgo : ExporterAlgo`, `ExpressionsMenuBinding :
SerializedObjectWrapper` against `ParameterProperty : MappingProperty`. Unobfuscated field names
settled the settings family on their own (`_value`, `_valueX/Y/Z`, `r`/`g`/`b`, `guid`/`localID`),
and a nested type already sharing a name on both sides (`NodeType`, `ChangeType`) anchors the
sequence it sits in.

`ComponentQueue.cs`'s header said `// Original name: ComponentQueue`, which is why it resolved
against nothing — the export type is `QueueProperty` (`GameObject` + `Component[]`, matching what the
file's own comment describes). Fixed in the map on 2026-07-31.

**`ADOEditorUtility` and `EditorUtils` share code.** ADOverhaul's `ExporterServerStub` and
ControllerEditor's `ProcessorObserver` are the same class — same nested position-flag/rect struct,
same `GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), …)`, same field sequence — so a
name derived in one plugin applies to the other, and `ResizeHandle` / `ResizeZone` / `SceneViewPanel`
are now used in both. Check the other plugin before naming a utility nested type from scratch.

### Known-unresolved mappings

Left empty in `renames/` on purpose, because the evidence contradicts itself — resolve these against
the export before filling them in:

| Export class | Candidates | Note |
|---|---|---|
| `TokenParamsDispatcher` | `FoldoutStateTracker` / `SelectionState` | The `FoldoutStateTracker.cs` header and the `ADOverhaulInspectors.cs` class map both say `FoldoutStateTracker`; the old table here said `SelectionState`. `ilrename usages` shows the export class has `singletonSerializer`, `LoginProcess` and `PatchProcess`, which matches what `ADOverhaulInspectors.cs` claims maps to `_current`/`SetExpanded`/`ClearCurrent`. |
| `CreatorMethod`, `LoaderMethod` | `MixedValueScope`, `FoldoutStateTracker` | Neither export class exists in the current export at all. |
| ADO `ContextSerializerConnector` | `ReflectionCache` / `TypeReflectionData` | **Unresolved, and now blocking.** ADOverhaul's map says `ReflectionCache`; ControllerEditor's structurally identical struct is named `TypeReflectionData`. Both fit — it caches `MemberInfo[]` plus field/property/method dictionaries. This used to be cosmetic, two names for one shape across two separate plugins. **The 2026-07-31 package consolidation makes it a real collision**: both tools now ship inside one `com.dreadscripts.unlocked` assembly, so the struct has to be ported once, under one name, with the other call sites pointing at it. Decide before either is ported; the loser is a `rename_status`/`apply_renames` edit in the map, not a source edit. |

Resolved by elimination, 2026-07-30: `InputMethod` / `TaskConsumerExporter`, which both claimed
`GUIColorScope`. `InputMethod` exists in no export and no rename template — the same staleness as the
`CreatorMethod` row above — leaving one claimant, so `TaskConsumerExporter` is now mapped.
## String Extraction Method

> **Historical:** The `Hooks.cs` / `SmethodStringDumper.cs` / `StringListDumper.cs` tools referenced below
> lived in the now-removed `dreadre-dll-playground` project and are **no longer in the repo**. Their outputs
> (`dumps/*.txt`) remain and are still consumed by `scripts/dotnet_reactor.py`. The methodology is kept here
> for reference only.

**Why smethod_N can't be called externally:**
Every smethod_N has `if (Assembly.GetExecutingAssembly().Equals(Assembly.GetCallingAssembly())) { proceed } else { return default(T); }` — returns null for all external callers.

**Why Harmony patching fails:**
Mono's `Module.ResolveField(token)` returns null for global (`<Module>`) fields — MonoMod DMD generation throws "Unexpected null @ ldsfld byte_0".

**Working approaches:**

1. **Runtime hooks** (Hooks.cs): Harmony-patch each smethod_N to log decrypted strings. Also logs all UnityWebRequest calls. Output: `dumps/intercepted.txt`. Good for ControllerEditor.

2. **Offline IL scan** (SmethodStringDumper.cs): Forces `<Module>` static constructor, reads `byte_0` directly, applies the key transform offline.
   - `RuntimeHelpers.RunClassConstructor(moduleType.TypeHandle)` to init `byte_0`
   - Read `byte_0` via `moduleType.GetField("byte_0", ...).GetValue(null)`
   - Auto-detect transform constants from IL: look for `ldarg.0 → ldc.i4 A → mul → ldc.i4 B → xor → starg` pattern
   - Output: `dumps/{AssemblyName}.txt`

**The per-method A/B transform constants for all three assemblies** (ADOverhaul2019,
ADOverhaul2022, ControllerEditor) are tabulated in the DRM spec ([`../DRM.md`](../DRM.md), published as [dreadscripts-unlocked/reverse-engineering/DRM.md](https://github.com/hhvrc/dreadscripts-unlocked/blob/main/reverse-engineering/DRM.md)) § "smethod Transform Constants". They are
not duplicated here — `scripts/dotnet_reactor.py detect` re-derives them from the IL on demand, which
is the authoritative source for any of them.

Note: ControllerEditor uses invisible Unicode characters as method names (not `smethod_N`), since it wasn't fully processed by de4dot. SmethodStringDumper maps them to smethod_1..5 by discovery order.

---

## Obfuscation Pattern Reference

```csharp
for (;;) {
    IL_XXXX:
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

Sentinel fields (present in every class, always null/true, cosmetic):
```csharp
internal static ClassName SentinelField;
internal static bool SentinelCheck() => SentinelField == null;
```

---

## Dump formats consumed by `scripts/dotnet_reactor.py`

The tool's own subcommands are documented in [`AGENTS.md`](AGENTS.md#scripts) and by
`python3 scripts/dotnet_reactor.py --help` — not repeated here. What is worth recording is the two
historical dump formats it auto-detects, because the tools that produced them are gone and nothing else
explains the shapes:

- `dumps/intercepted.txt` — `Hooks.cs` runtime format: `[STRING] byte_0+OFFSET: "string"`
- `dumps/<AssemblyName>.txt` — `SmethodStringDumper` offline format: `smethod_N(key) = "string"`

---

## Stripping the licence code: what counts, inside the root classes

Both products keep their licence code inline in their root class rather than in a file of its own,
so `port_status.py` marks `ADOverhaul` and `ControllerEditor` strip-during-port rather than excluded.
These are the parts that are *not* obviously licence code and would be shipped by accident. Found
2026-07-31 while naming; verified against `export/`.

- **The bug reporter shares the DRM request path.** `BugReporter`'s `findsolution` and `reportbug`
  requests are built by the same `CountConfiguration` + `StartConfiguration` pair as
  `transferlicenserequest` and `transferlicenseconfirm`, so every bug report carries `product_id`,
  `HWID`, `SID`, `license_key` and an HMAC signature. It is not licence *validation*, but it cannot
  be ported unchanged — and beyond stripping, a restored tool that still posted a licence key and
  hardware id to a dead endpoint would be worse than one that does not. The wire format itself is
  [`../DRM.md`](../DRM.md)'s to document, not this file's.
- **`ADOverhaulWindow.OnGUI` is entirely inside the gate.** Its whole body sits under
  `FlushConfiguration(this)`, the check that draws "Activating License..." / "Verifying License...".
  Stripping means keeping the inner draw calls and dropping the gate around them — deleting the
  method removes the window.
- **Three `ADOSettings` fields are licence state**, and keep the vendor's own `a_` prefix:
  `a_HasSucceededLastVerification`, `a_VerifyOnDisplay`, `a_VerifyOnProjectLoad`. They round-trip
  through the settings blob under an EditorPrefs key whose prefix is the product hash, so dropping
  them must not change the rest of the blob's format or previously-saved user settings stop loading.
- **`BannerDownloader` is not licence code** but has a call site inside the gate. Keep the class and
  its other call site; the gated one goes with the gate.
- **ADOverhaul's update notice is not licence code, and whether it ships is an open decision.**
  It is the client side of the `getdownloadinfo` command — the wire format is
  [`../DRM.md`](../DRM.md) §8's to document, not this file's, and it carries no HWID,
  SID, licence key or HMAC, so it is not DRM. The port consequence is the part that belongs here:
  it runs from `[InitializeOnLoadMethod]`, so whatever it does, it does on every project load.

  **The endpoint is not simply dead for our users**, which is what makes this a real choice rather
  than an obvious exclusion. `../public/drm_server/` already implements `handleDownloadInfo` and
  answers "You are running the latest version." So for anyone running the restoration server the
  feature works; for anyone not running it the request fails and the user gets
  *"Something went wrong while checking for an update!"* on every load. That is unlike the supporter
  window, which `port_status.py` excludes outright — that one fetches from a different host the
  server does not stand in for, so no configuration makes it work.

  Unresolved: ship it (works with the server, noisy without), ship it silenced on failure, or drop it
  as the no-network-I/O policy would suggest. Either way its `u_*` settings fields stay in
  `ADOSettings` — removing those would change the settings blob format, and the fields and the
  feature are separable.

---

## Vendor bugs found in the original code

Distinct from decompiler artefacts: these are mistakes in the shipped product, so the port has to
decide *preserve or fix* rather than *transcribe correctly*. Default to preserving unless the bug
makes a feature unusable — a restoration that quietly changes behaviour is harder to trust.

**`EditorUtils`' preserve-offset constraint dispatcher silently ignores `ScaleConstraint`**
(ControllerEditor, `EditorUtils.cs`, `RestartResolver` at line 2503 — ported as
`ActivateAndPreserveOffset(this IConstraint)`). It type-switches over `ParentConstraint`,
`RotationConstraint`, `PositionConstraint` and `AimConstraint`, and falls off the end for anything
else, so a `ScaleConstraint` passed to it does nothing at all. The wrapper it would need exists and
works: `InsertResolver` at line 2494 is the `ScaleConstraint` preserve-offset activator, and nothing
else calls it.

The sibling zero-offset dispatcher (`QueryResolver`, 2529) omits `ScaleConstraint` too, but that one
is *correct* — Unity's `ScaleConstraint` genuinely has no `ActivateWithZeroOffset`, which is also why
the cached-`MethodInfo` field list has nine entries rather than ten. That asymmetry is what makes the
first case a bug rather than a deliberate omission: only one of the two dispatchers had a wrapper
available and failed to call it.

Found 2026-08-04 while recording the constraint names. Preserved, not fixed — no shipped call site
passes a `ScaleConstraint` through the dispatcher, so fixing it would change behaviour no user can
currently reach.

**`TransitionEditionInfo`'s constructor reads the wrong reflection handle** (ControllerEditor,
`AnimatorGraphReflection.cs`, ctor around line 732). Two consecutive lines assign
`isAnyStateTransition` and `isDefaultTransition`, and *both* read the `"isAnyStateTransition"`
handle. The `"isDefaultTransition"` handle is resolved on the line above and then never read
anywhere in the assembly.

Confirmed by reading the export directly: the two assignments are textually identical apart from
their destination field, and a grep for the second handle finds the declaration and no use. A
decompiler renders whichever field the IL loads; it does not invent a duplicated read, and it would
have no reason to leave a resolved handle dangling. The shape — resolve two handles, paste the line,
forget to change one — is an ordinary copy-paste slip.

Consequence: `isDefaultTransition` is always equal to `isAnyStateTransition`. It feeds the node-type
classification, the `isExplicitEntryTransition` derivation, and the branch that chooses
`RemoveEntryTransition`, so entry-transition handling behaves as though every entry transition were
the default one (or none were, depending on the any-state flag). Found 2026-07-31 while naming.

---

## ADOverhaul2019 — New Binary Analysis (2026-03-02)

`binaries/ADOverhaul2019.dll` is a March 2024 build of ADOverhaul targeting Unity 2019 (earlier than the 2022 build). It was analyzed for the first time in this pipeline run.

**Neither build is uniformly the better decompile, and that is worth exploiting.** Naming agents
comparing the two line by line in 2026-07-31 found mis-renders in *both* directions, so when a body
looks wrong, check the other build before assuming the logic is wrong:

- 2019 is right, 2022 is mangled: `ReflectionAccessor.TrySetValue` (2022 loses the `return true` after
  the property branch and falls into `SetValue` on a null `FieldInfo`); `BugReporter.Draw` (2022
  scrambles the branch nesting so the solution is stored only when the server message is *blank*).
- 2022 is right, 2019 is mangled: `CachedIcon.GetTexture` (2019 renders the retry as a
  non-terminating `while (true)` where 2022 gives the plain `if`).
- 2022 is mangled and 2019 was never checked: `VectorSetting.GetValue` — see
  `theory/de4dot/cflow-resolves-to-infinite-loop`, where the 2019 body is the ground truth that
  reopened it.

Everything else the agents diffed was shape-only — inverted `if`/`else`, flipped ternaries, De Morgan
rewrites — with identical semantics. That is also why `mirror_renames.py` rejects most type pairs:
its identity check is on masked body text, so a branch inversion, or even an `internal` vs `private`
difference on a scaffolding field, is enough to refuse a genuine match.

### Key differences from ADO2022

| Aspect | ADO2019 | ADO2022 |
|---|---|---|
| Main DRM class | `ConfigurationTestStub` (de4dot name) | `IdentifierSerializerConnector` (de4dot name) |
| HTTP client | `HttpWebRequest` / `WebRequest.CreateHttp()` | `UnityWebRequest` |
| HMAC algorithm | HMACSHA256 | HMACSHA256 |
| HMAC secret | **Identical** to ADO2022, as a plaintext literal | (value in the spec) |
| smethod transforms | Different A/B constants (see table above) | Different A/B constants |
| Module GUID | `{CAD6ED8D-8CDE-4E08-A19D-89CBC52DD07C}` | `{7907DD2F-A0A5-4805-95CD-D1B3741C5FB4}` |
| de4dot output size | 234KB | 242KB |

### Confirmed plaintext literals in ADO2019 (not smethod-encrypted)

- `"license_key"` — JSON field name in request body (identical to ADO2022)
- `"No1lKII9IzcBAbihub6nCg==SettingsJSON"` — EditorPrefs key for plugin settings
- HMAC-SHA256 secret — byte-identical to ADO2022's; the value is in the spec
- `"application/json"` — `Accept` header for HttpWebRequest
- `"^[a-zA-Z0-9]{6}$"` — verification code regex

### Request building (ADO2019 `PrintSystem`)

```csharp
// command name, product_id key, product_id value, version key, version value,
// HWID key, HWID value, SID key, SID value, "license_key" (literal), key value
new List<ValueTuple<string, string>> {
    (smethod_2(369897284),   param),                              // command field
    (smethod_4(-1415579542), smethod_2(609496728)),               // "product_id": <productId>
    (smethod_5(345281593),   ConfigurationTestStub._Connection),  // "version": <version>
    (smethod_2(1435612786),  ConfigurationTestStub.record),       // HWID field
    (smethod_5(-987090458),  ConfigurationTestStub.status),       // SID field
    ("license_key",          ConfigurationTestStub.setter),       // literal!
}
```

### HTTP transport (ADO2019 vs ADO2022)

ADO2019 uses `HttpWebRequest` for DRM POSTs (older .NET class); ADO2022 uses Unity's `UnityWebRequest`. Both POST JSON to the same Firebase endpoint. ADO2019 also uses `UnityWebRequest` for the package update download (separate code path).

### HMACSHA1 usage in ADO2019

ADO2019 has a secondary `HMACSHA1` call at `ConfigurationTestStub.cs:1968` — this is in the DSLICINF cache per-field integrity check, not the server request hash. Identical pattern to ADO2022 (cache field HMAC-SHA1 with `productId + sessionId` as key).

### ADO2019 smethod transform constants

See "String Extraction Method" section above for the per-method A/B constants.

---

*The repo layout used to be duplicated here; it drifted out of sync with reality (stale
`Assets/DreadScripts/Editor/` path — see the correction note near the top of this file) and is now
maintained in exactly one place: `AGENTS.md`'s "Repository Layout" section. Check there instead of
trusting a copy here.*

---

## Next Steps (Priority Order)

> **Open decisions, blocking nothing but worth settling before the port reaches them.** Each needs a
> judgement rather than more analysis:
>
> 1. **Converge the typed-setting family across the two assemblies.** `BoolSetting`, `FloatSetting`,
>    `ColorSetting`, `EnumSetting` and friends exist in both products and both now ship in one
>    package, so they port once under one set of names. ControllerEditor uses plain overloaded
>    `Draw`; ADOverhaul carries invented `DrawContent`-style suffixes left over from a tooling
>    limitation that no longer applies. ControllerEditor's is the shape the original had.
>    `apply_renames.py set --force` is how to correct the losing side.
> 2. **`ReflectionCache` vs `TypeReflectionData`** — one struct, two names, same reason as above.
> 3. **Preserve or fix the vendor bug** in `TransitionEditionInfo` (see "Vendor bugs" above). Default
>    is preserve; this one is borderline because it affects a removal path.
> 4. **`SplitterGUIUtils` and `EditorLayoutUtils` are line-for-line duplicates.** One should be
>    dropped during the port rather than both shipped.



*Refreshed 2026-07-31. Items 1 and 2 previously described filling `TODO` stubs in `dreadre-devel/`
and reconciling that tree against ADOverhaul2019. Both are void: that tree was retired and its
sources dropped, so there are no stubs to fill. Replaced with the work that actually remains.*

> **Scope note:** The runtime restoration-patch approach (Harmony `ADORestorationPatch`/`CERestorationPatch`,
> the `dreadre-dll-playground` Unity project, and the `Hooks.cs`/`SmethodStringDumper.cs`/`StringListDumper.cs`
> tooling) has been **abandoned and removed**. No patches will be maintained.

1. **Port `export/` into `../public/unity/`, one file at a time, polishing on the way in.**
   `python3 scripts/port_status.py` is the ledger — it diffs the two trees, so it cannot go stale,
   and it owns the exclusion policy. Two things about that policy are easy to get wrong:
   **both products carry HWID/HMAC validation inline in their root class** — `ADOverhaul` (formerly
   mis-named `LicenseManager`) and `ControllerEditor` — so both must be stripped during the port
   rather than skipped; excluding either file would drop that entire tool. Only
   ADOverhaul**2022** is ported; the 2019 tree stays a reference (see below).

   Three classes are far larger than the rest and each is one top-level class with dozens of nested
   types, so they want the partial-class split — see the `splitting-large-classes-into-partials`
   skill, which also explains why splitting will *not* isolate the DRM. Name a class's members before
   splitting it, or the partial filenames get derived from names like `ConsumerAlgo`.

2. **Keep naming members.** `python3 scripts/rename_status.py` reports coverage; write names only
   through `python3 scripts/apply_renames.py set`, which takes the lock that makes parallel work
   safe. `mirror_renames.py` copies names between the two ADOverhaul builds, but only for types whose
   bodies it can prove identical — most cannot be, so the 2019 map is largely hand work.

   Two measurement caveats. **The maps only offer about half of each assembly's members**: `ilrename
   template` filters on a vocabulary heuristic, and Reactor names that read like real code slip past
   it, so the "remaining" numbers understate the job — `--all-members` is the lever if that trade is
   worth taking. And **ADOverhaul2019 is reference-only**: it is not ported, but its decompile is
   sometimes *cleaner* than 2022's, which makes it ground truth when a 2022 body is mangled.

   Type identity across the two builds still holds: Reactor assigns the same TypeDef token to the
   same type in both, verified against all 20 named types and independently of the content matching
   that first paired them. Treat that as corroboration, never a substitute for matching on content.
   **Method and Field tokens are not stable** — see AGENTS.md's "Renaming" section, which owns that
   correction.

3. **Re-check the `theory/de4dot/` files against the fork before trusting any `export/` body.** This
   item previously asserted "two live de4dot bugs, not yet root-caused". That is no longer true and was
   misleading in both directions:
   - `reflection-proxy-type-confusion` — **FIXED** on the fork (`FakeInstanceStubFixer`). Root cause was
     that the receiver type confusion is *pre-existing in the obfuscated input*, not de4dot-introduced.
   - `cflow-resolves-to-infinite-loop` — **REOPENED 2026-07-31.** The three root causes found are
     fixed, but the "corpus 21 → 0" that closed it counted only switch-shaped machines, and a
     surviving non-switch instance has been found (`VectorSetting.GetValue`, an Editor-hanging
     lazy-init getter). Both `detect_broken_state_machines.py` and de4dot's gate 5 report clean on
     it. Status and open questions live in the theory file — do not restate them here.
   - `partial-dispatch-resolution-corrupts-state-machine` — **CONTAINED, not fixed.** Branch-and-select
     rejects the bad resolution, so affected machines come out unresolved-but-faithful (verified
     terminating) instead of silently truncated. The underlying `EdgeResolver` mis-seeding is still
     there.
   - `displayclass-async-statemachine-missingmethod` — still **OPEN**, single data point.

   The rule this item exists to enforce is unchanged: read the relevant theory file before assuming a
   given stub's `export/` source is trustworthy, and prefer `../de4dot/ROADMAP.md` §1 over any status
   summary written here — a per-theory status kept in two repos is exactly what went stale above.

## Session Notes — 2026-07-29: ADOEditorUtility deep audit + de4dot validation

Started on the stub-filling backlog above (item 1), beginning with `ADOverhaulSettings.cs` (3 TODOs,
all a dead-delegate gizmo-callback wiring bug — fixed, matches export 1:1) then `ADOEditorUtility.cs`
(8 TODOs). The latter escalated well past its original scope once actual auditing began:

- **`RunStatus`, `BuildArrowMesh`, `InitStatus`, `CreateStatus`**: all had real bugs, not just
  uncertain "verify" annotations — a 9-zone resize-grid with wrong column/row math for every corner
  flag, a completely fabricated "arrow mesh" (the real original is a small pyramid/cone mesh with a
  hardcoded vertex array, now copied verbatim), a fabricated MouseUp/hotControl phase in a handle
  click-dispatcher that the original doesn't have, and a "33-case CFG" comment describing a method
  that's actually a clean 4-dot radius-handle drag loop. All fixed and verified against `export/`.
- **`ConnectStatus`** — not TODO-tagged at all, found wrong anyway while checking an adjacent method:
  used `if/else if` where the original uses two independent `if`s (so move+rotate handles could never
  both draw in one call), plus a wrong Handles call in one branch. **Lesson generalized into memory**:
  absence of a TODO marker is not evidence of correctness — `ConnectStatus`, and later
  `InterpreterSerializer`'s entire field list (see below), were both silently wrong with zero markers.
- **`TaskMethod`** (a VRC component shape-snapshot struct, feeds `CopyColliderShape`/`CopyContactShape`/
  `SetVal`/`CompareVal`): found the previous devel reconstruction wrote `shapeType` back onto colliders
  when the real original never does that on the collider path, and found a literal `while (true) {
  reference.rotation = ...; }` **in the raw `export/` decompile itself** — a de4dot/ilspycmd CFG
  artifact, not original program behavior (an unconditional Editor-hanging infinite loop on a basic
  "copy shape" button cannot be real shipped behavior). Logged as a new theory,
  `theory/de4dot/cflow-resolves-to-infinite-loop/`. Rewrote `TaskMethod` to match the original's real
  field/method shape (including the previously-entirely-missing `ApplyBack()`/`SortProduct` method and
  `rootTransform` capture), reconstructed as a plain assignment instead of the loop.
- **`InterpreterSerializer` / `CreatorServerStub` / `MapRef()` / `CustomizeRef()`**: found that the
  previous devel version fabricated ~9 icon fields for `InterpreterSerializer` (lock/settings/expand/
  collapse/copy/paste/visibility) that don't exist in the real class at all (~34 real fields: update/
  refresh/console icons, lock-button on/off, scene-picking icons, plus ~15 settings-label `GUIContent`s
  with real tooltip text) — **and** that 4-5 of those fabricated fields (`_ClassSerializer`,
  `m_SerializerMethod`, `methodMethod`, `m_ProcSerializer`, `configurationMethod`) actually belong on a
  *different* class, `CreatorServerStub` (a ~20-field GUIStyle registry), confirmed by finding their
  exact definitions there in `export/`. Worse: devel's `MapRef()`/`CustomizeRef()` accessor **method
  bodies are swapped** relative to the original (`CustomizeRef()` should return `InterpreterSerializer`,
  `MapRef()` should return `CreatorServerStub` — devel currently has it backwards), which is why the
  wrong-class field placement didn't already show up as an obvious compile mismatch. Confirmed by
  checking real call sites: `ADOverhaulWindow.cs`'s `CustomizeRef()._MapperSerializer` only makes sense
  if `CustomizeRef()` returns `InterpreterSerializer` (which has a real `_MapperSerializer` field) —
  matching the original, not current devel. **Not yet fixed** — paused per explicit decision below.
- **`SchemaMapping`/`EventMethod`** (lazy icon-texture caching, feeding `InterpreterSerializer`): both
  entirely fabricated in devel (a guessed AssetDatabase/GUID caching scheme vs. the real
  SessionState-int-array PNG-encoding approach; a missing `IDisposable` RenderTexture-readback wrapper
  for non-readable built-in icon textures). Read and understood the real versions; **not yet rewritten**.
- **`ExporterServerStub`/`SystemSerializer`** (scene-view overlay resize-handle system): confirmed the
  existing devel version is a plausible-but-wrong simplified reimplementation of a much larger
  (~200-line) aspect-lock-capable 8-handle per-corner-delta drag system spanning two interdependent
  classes. **Not yet rewritten** — flagged as its own dedicated task, too large to rush.

**De4dot validation performed** (per explicit instruction not to treat `export/` as ground truth
before validating de4dot): ran `scripts/de4dot_scorecard.py full ADOverhaul2022` and confirmed the
*only* 10 known-real (ilverify-confirmed, non-noise) target-internal errors in this build are all
outside every class touched above (`TaskMethod`, `SchemaMapping`, `EventMethod`,
`InterpreterSerializer`, `CreatorServerStub`) — they cluster in `AdvisorMethod`,
`AuthenticationIdentifier`, `ConnectionIdentifierService`/`BroadcasterIdentifier` constructors (both
in `ADOverhaulSettings.cs`'s export source), and `ExceptionSingletonStruct::SortRef`/`FlushAdapter`,
all matching the already-tracked `reflection-proxy-type-confusion` pattern (now logged as 6 additional
confirmed instances of that same bug shape in the theory file). **Caveat that matters for future
reconstruction work**: ilverify only proves IL type-safety, not correct logic — it would never have
caught the `TaskMethod` infinite-loop bug above, since that IL is perfectly type-safe. A clean
ilverify result for a class is real evidence de4dot didn't introduce *type-confusion*, not a guarantee
every line of its logic is trustworthy.

**Structural change**: introduced a permanent partial-class-per-folder convention for large classes
with many nested types (`splitting-large-classes-into-partials` skill) — `ADOEditorUtility.cs`
(2852 lines, 14 nested types) is now split into `ADOEditorUtility.Core.cs` +
`ADOEditorUtility/ADOEditorUtility.<TypeName>.cs`, one file per nested type, each headed with its
exact `export/` line range and an explicit audit status (`VERIFIED` / `KNOWN DIVERGENT` /
`NOT YET AUDITED`) so progress persists across sessions instead of needing to be re-derived by
grepping a monolith. Confirmed via this work that `[MenuItem(...)]` attributes decompile perfectly
(100% match against `export/` for both ADOverhaul and ControllerEditor) — Reactor's string encryption
never touches attribute constructor arguments, only runtime string usage.

**Paused by explicit decision**, not by running out of leads. The resume instructions that stood
here pointed at per-file audit headers in the retired `dreadre-devel/` tree and are void — those files no
longer exist, and everything they recorded will be re-derived when the class is ported from `export/`.
What survives is the *findings*, above: which nested types were fabricated rather than merely buggy, and
that `MapRef()`/`CustomizeRef()` had their bodies swapped. Re-check those specific things when
`ADOEditorUtility` is ported, rather than assuming a fresh port cannot repeat them.

## Session Notes — 2026-07-31: EditorUtils names backported into `renames/`

The four `EditorUtils` partials already ported into `../public/unity/` (`Colors`, `Contents`,
`Styles`, `Textures`) had been named **only in the ported files**, never in the map — every
`EditorUtils` member entry in `renames/ControllerEditor.json` was still empty, so `export/` kept
showing `WatcherProcessor`/`DestroyError`/`m_AlgoProcessor` while the deliverable called them
`Contents`/`contents`/`accentColor`. That is precisely the failure the "record the rename in
`renames/`, not just in prose" rule exists to prevent: the two trees disagreed on 130 names.

All of them are now in the map, plus the `Buttons`/`Cursors` slice ported this session:

- **`Contents` (was `WatcherProcessor`)** — 64 fields, matched one-to-one on their icon name and
  tooltip, which are unique per entry.
- **`Styles` (was `BaseProcessor`)** — 40 members, matched on the properties each style sets.
- **`EditorUtils` itself** — 26: the two lazy accessors and their backing fields, `IconContent`,
  `CachedIcon`, `TrimTransparentBorder`, `Grey`, `accentColor`, and the button/cursor family.

Two naming decisions worth knowing about, since neither is recoverable from the map alone:

- **Four decompiled methods map onto `Button`, and four onto `ToggleButton`.** Reactor had already
  split each into a (content type × style present) overload set; the ported C# collapses each pair
  with optional parameters, but the *metadata* keeps four distinct signatures, so ilrename's
  name+signature collision check passes and `export/` will show four overloads. Deliberate — an
  invented suffix would claim a distinction the original never had.
- **`contents`, `styles` and `TextFieldDropDown` are `[SpecialName]` property getters** whose
  Property rows the obfuscator dropped, so they decompile as methods and `export/` will render them
  as `EditorUtils.contents()`. The ported source makes them properties again. The lowercase names
  match the port rather than the shape of the export on purpose.

**`export/` was regenerated and `reverse-engineering/export/` re-snapshotted**, but only after a false
alarm worth recording, because it will recur. The first re-export produced 1984 residual `smethod_N`
call sites in ControllerEditor against a ceiling of 37, and `refresh_renames.py` reported 187 chosen
names across the three maps whose keys no longer resolved. Both readings said "the fork regressed
badly today". Neither was true: **`work/de4dot` had been published by copying
`../de4dot/Release/net10.0/<rid>/` alone, which leaves the net8.0 `de4dot.constdata` worker
behind.** The fork's net8.0 pin is gone — the host is net10.0 and only that worker stays pinned,
because .NET 10's loader rejects Reactor metadata — and it is found by probing the *sibling*
`Release/net8.0/<rid>/` directory, which exists in the fork's own layout and not in `work/`. Without
it de4dot still exits 0 and still writes an assembly; nothing is decrypted, and the output passes
gates 1, 5 and 6. Publishing the worker into `work/de4dot/constdata/` restored gate 7 to its 35/35/27
baseline and `refresh_renames.py` to 0 re-keyed, 0 lost. `pipeline.py` now warns when the worker is
unreachable, and its publish instructions include it.

What the re-export actually changed, beyond this session's names: 18 files, and the ADOverhaul
`-203`/`-206` line diffs are **not** lost code. They are the receiver-type confusion finally
resolving — `<>c__DisplayClass108_0.PublishImporter((Editor)this) as VRCPhysBone` is now
`base.target as VRCPhysBone`, and the proxy holder that call site needed went with it. The 2
remaining target-internal ilverify errors (gate 1, both ADOverhaul builds, `StackUnexpected` in that
same now-unreferenced `<>c__DisplayClass108_0`) sit in code nothing reaches; that matches the
`reflection-proxy-type-confusion` finding that the confusion is pre-existing in the obfuscated input.
ControllerEditor passes all four gates. `typecheck_package.py` reports 0 errors on the ported package.

Two tooling fixes fell out of this and are committed with it: `apply_renames.py` was POSIX-only
(unconditional `import fcntl`, so the only sanctioned writer could not run on Windows at all) and
gained `--set-type`, since type names live in the same map and had no writer — the two nested types
above are the first ones set through the lock rather than by hand. `refresh_renames.py` probed for a
`pipeline.find_ilrename()` that has never existed and fell back to a hardcoded POSIX path, so it
reported a working `ilrename.exe` as missing.

## Session Notes — 2026-07-31 (cont.): de4dot commit 39649089 + Windows tooling sweep

**Commit 39649089 ("Reactor: fold opaque predicates that are provably constant, not merely short")
was a no-op on this corpus — but that commit was later reworked and this no longer holds; see the
correction below.** A/B'd it with `de4dot_lab.py ab` (parent `39649089^` = f2616708 vs the
commit) across all three samples: every measured column is identical — .cs count, total lines,
unresolved-reference comments, switch/opaque dispatch sites and their termination split, `goto`
density, undecrypted `smethod_N`. `de4dot_scorecard.py gates --all` before/after diffs to nothing but
the temp-directory name in two ilverify paths. The commit tightens a heuristic that *replaced a
predicate call with `ldc.i4.1` whenever the callee did not end in `ldc.i4.0`* — which would corrupt a
real `return x > 0` — but nothing in ADOverhaul/ControllerEditor hits the newly-guarded shape, so the
output is byte-equivalent. Safe to build `export/` against; it changes nothing here and fixes a
latent correctness bug that could bite a different target. (The commit sits **uncommitted** in the
de4dot working tree after a `reset HEAD~1`: the cflow edit plus an untracked
`NeverWrittenStaticFields.cs`. `work/de4dot` was built from that tree, so `export/` already contains
it.)

**Windows tooling sweep.** Ran every script in `scripts/` and every `ilrename` subcommand on this
Windows host. Fixed three portability breaks and one pre-existing bug, none of them Windows-cosmetic:

- **`apply_renames.py`** — POSIX-only (`import fcntl` at module scope); see the previous note. Fixed
  with an `msvcrt`/`fcntl` lock shim.
- **Console encoding** — a Windows console hands Python a cp1252 stdout, and every report here uses
  box-drawing rules, arrows and em dashes. Printing one raised `UnicodeEncodeError` *after* the work
  was done: `explore_dreadscripts.py` crashed on its first heading, `sync_public.py` mangled its
  arrows. Added `configure_console()` to `pipeline.py` (UTF-8 + `errors="replace"`), called from it
  and from the three report scripts that do not import it.
- **`explore_dreadscripts.py` had no argument parser at all**, so `--help` — and any other argument —
  fell through into the live fetch loop, which hit dozens of remote endpoints and **overwrote the
  tracked `dreadrith_exploration_manifest.json`** with that day's responses. Several recorded sizes
  came back smaller, which is a finding about the endpoints, not something to capture by accident.
  Added a real parser and a `--dry-run`; `--help` is now inert. Reverted the clobbered manifest.
- **`refresh_renames.py`** — probed for a `pipeline.find_ilrename()` that has never existed and fell
  back to a hardcoded POSIX path, reporting a working `ilrename.exe` as missing. Pointed it at
  `pipeline.ILRENAME`.
- **`trace_original_machine.py`** matched the method name *anywhere*, so `--method Button` bound to a
  `GUILayout.Button(` call inside another body, sliced from mid-method, and died with
  `KeyError: 'IL_002a'` in the block builder. Now matches inside a `.method` declaration header only,
  and when the name is absent says so — noting that it reads the *obfuscated* binary, where
  renames/export names do not exist. Verified it still traces a real machine
  (`ControllerEditor::DisableAnnotation`).

Verified working as-is on Windows (no change needed): `reexport.py`, `de4dot_scorecard.py`
(deobf/verify/markers/full/gates), `de4dot_lab.py` (run/ab/show/clean), `dotnet_reactor.py`
(detect/scan), `port_status.py`, `rename_status.py`, `detect_broken_state_machines.py`,
`analyze_closures.py`, `find_reactor_decoys.py`, `mirror_renames.py`, `changed_methods.py`,
`typecheck_package.py`, `sync_public.py`, and `ilrename counts/report/usages/template/apply`.

**`patch_caller_check.py` — fixed.** It had been matching a `brfalse` caller guard, but the guard
these binaries emit branches with `brtrue.s` to the real body (the `return default(T)` is the
fall-through), so `IsCallerGuard` never matched and it exited "no Assembly caller guards found". The
recognition logic was already correct; only two things were wrong, both in `SourceAnalysis`'s
`PatchCallerCheck`. It now accepts either polarity, and — crucially — decides how to neutralise from
where the `return default` arm actually sits rather than assuming the fall-through is the body:
blindly `pop`ing a `brtrue` guard would have dropped execution into the `return default` and made
every call yield `default(T)`. When the body is the branch target it emits `pop` + an unconditional
`br` to the body, leaving the `return default` arm as dead code; when the body is the fall-through it
emits `pop` alone (the original behaviour, preserved).

Verified three ways. On the real deobfuscated ADOverhaul2022 it patches all 5 `smethod_N` guards to
`pop; br body`, and ilverify is unchanged (2 errors before and after — the pre-existing
receiver-confusion pair in `<>c__DisplayClass108_0`, nothing new). A throwaway dnlib harness (left in
scratch) emitted a DLL carrying *both* guard shapes; the patcher neutralised each to the expected IL
(`brtrue`→`pop; br body`, `brfalse`→`pop` + fall-through), with no new verify errors on the patched
methods. Runtime execution of the synthetic assembly was not usable as a proof — the CLR rejects a
hand-built dnlib module with `BadImageFormatException` at load, identically before and after the
patch, so it says nothing either way; the structural + ilverify evidence is what stands. README and
the `using-historical-support-scripts` skill updated to describe both polarities.

## Session Notes — 2026-07-31 (cont.): reexport with the reworked opaque-predicate fold

The opaque-predicate-fold commit was **rebased**: the `39649089` I A/B'd is gone from de4dot history,
replaced by `a9fc8614` (same title) plus a `NeverWrittenStaticFields` class the cflow deobfuscator now
calls, at de4dot HEAD `74ac3000`. The A/B "no-op" verdict was correct for the *old* commit only —
worktrees built from a committed ref, and the reworked logic (partly an untracked file at the time)
never entered that test. **The reworked fold is not a no-op.**

Re-exporting against it changes exactly two files, both a de4dot proxy artifact and neither ported:
`ADOverhaul2019/ProxyDelegateBinder.cs` and `ADOverhaul2022/ProxyDelegateBinder.cs`. `LogoutRule`
regressed in *readability* — it was clean `foreach` over `type.GetFields()`, and now decompiles as an
unresolved `while (true) { switch }` because the stricter fold no longer resolves the `SetupRule()`
opaque predicate. Part of that is the intended trade (the old fold produced clean code by *guessing*
the predicate, the exact bug the commit fixes), but this particular predicate was **provably
constant** and should still have folded: `SetupRule()` is `return ChangeRule == null`, and
`ChangeRule` is an `internal static` field with **zero writes anywhere in the module** and no
`InternalsVisibleTo` on the assembly, so it is never-written → always null → the comparison is
constant `true`. §7c of the de4dot ROADMAP claims exactly this case ("a comparison of two
provably-null operands", decided by `NeverWrittenStaticFields`), so the fold left resolvable,
provably-safe work on the table — a gap, not just conservatism. The result is still
**unresolved-but-faithful**, so nothing is corrupted. Gate 5
confirms faithful — `non-terminating=0`, and the method shows up only as `undecidable` (+1 per
ADOverhaul build: 2019 1→2, 2022 2→3); `detect_broken_state_machines.py` reports `LOOPS: 0`. Gate 1
is unchanged (the same 2 pre-existing receiver-confusion errors, no new ones), gate 7 holds at
35/35/27, the licence-landmark still reaches. ControllerEditor is byte-identical to before.

`ProxyDelegateBinder` is de4dot's own proxy-delegate binder, not vendor code, and is not in
`../public/unity/`, so the deliverable and `typecheck_package.py` (0 errors) are unaffected; the change
is confined to `export/` and its published snapshot. If the readability of that one artifact ever
matters, the fold could be extended to resolve a method-call predicate that is *provably* constant
(not merely never-written-static-null) — but that is de4dot work, tracked in `../de4dot/ROADMAP.md`,
not here.
