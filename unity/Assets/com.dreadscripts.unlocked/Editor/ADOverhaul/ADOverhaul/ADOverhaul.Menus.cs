// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the tool's chrome -- the hamburger context menu, the update and announcement
// banners, the shared foldout box and the scene-view overlay frame (decompiled lines 7904-8094 and
// 8112-8296). Line numbers move with the snapshot; the decompiled names below are the durable
// reference. Field references go through the table in ADOverhaul.State.cs.
//
//   PublishIdentifier  -> DrawSettingsButton, line 8290
//
// That is the whole of what could be ported. This is a HEAVILY PARTIAL port, and the reason is a
// single missing dependency, recorded in full below so that finishing the region is mechanical.
//
// ===================== BLOCKED ON ADOSettings (not ported at time of writing) ====================
//
// `ADOSettings` is ADOverhaul's persisted-preferences singleton (decompiled ADOverhaul.cs line 751,
// accessor `Instance()` at line 1569). The settings *framework* it is built on is already ported --
// Editor/Common/Settings/{SettingBase,ValueSettings,CompositeSettings,SettingsPersistence}.cs, which
// was reconstructed from ADOSettings and ControllerEditor's EditorSettings together -- and
// ControllerEditor's own half of that pair is ported as
// Editor/ControllerEditor/EditorSettings/EditorSettings.cs. ADOverhaul's half is not. The two
// products shipped separate assemblies with separate persisted blocks and separate EditorPrefs keys,
// so ControllerEditor's EditorSettings is NOT a substitute even though it carries identically named
// `u_update*` / `u_announcement*` fields; pointing this region at it would silently make the two
// tools share one settings block. Nothing here is stubbed and nothing is redirected.
//
// Eleven members wait on it. Each is listed with its decompiled name, line and the exact settings it
// needs, plus any other unported call target:
//
//   SortIdentifier       line 7904  -- the boxed header row every ADOverhaul surface opens with:
//                                     hamburger button, optional update-banner toggle, "v<version>"
//                                     label, then either a caller-supplied trailing block or the
//                                     credit link. Needs `u_updateHidden`; also DeleteIdentifier
//                                     (line 7784, the credit link, belongs to another region) and
//                                     CustomizeIdentifier below. Everything else it touches is in
//                                     place: ADOEditorUtility.IconButton, contents.hamburgerMenu,
//                                     contents.updateAvailable, styles.noteLeft, and the
//                                     `updateAvailable` / `updateFoldout` / `version` state fields.
//   InvokeIdentifier     line 7936  -- builds and shows the hamburger GenericMenu. Needs
//                                     `a_VerifyOnDisplay` and `a_VerifyOnProjectLoad`, and
//                                     FillIdentifier (see the dead-path section). See the note on
//                                     its menu contents below.
//   CustomizeIdentifier  line 8020  -- the update banner's fade group: version + message help box,
//                                     "Download Update", "Open Changelog", "Skip for Today". Needs
//                                     `u_updateHidden`, `u_updateVersion`, `u_updateMessage`,
//                                     `u_updateLink`, `u_updateChangelog`; also LogoutIdentifier and
//                                     CalcIdentifier (line 7775, another region).
//   ConcatIdentifier     line 8059  -- the announcement banner, same shape as the above. Needs
//                                     `u_announcementHidden`, `u_announcement`,
//                                     `u_announcementLink`, `u_announcementLinkName`,
//                                     `u_announcementHiddenDate`; also CalcIdentifier.
//   SetupIdentifier      line 8185  -- see its own section below. Needs `u_announcementHidden`,
//                                     `u_announcementHiddenDate`, `u_updateVersion`,
//                                     `u_updateHidden`; also NewIdentifier (line 7806) and
//                                     CalculateIdentifier (line 7770), both in another region.
//   LogoutIdentifier     line 8163  -- downloads and imports the update package. Needs
//                                     `u_updateLink`. See the dead-path section: the link it reads
//                                     is only ever written by the dead request.
//   SelectIdentifier     line 8228  -- the shared titled foldout box (help-box frame, a title in
//                                     styles.indentedHeaderLabel, an optional header-row block, a
//                                     click-anywhere-on-the-box toggle and an AnimBool fade group).
//                                     Needs `editorAnimatedFoldouts` and nothing else whatsoever --
//                                     ADOEditorUtility.ClickArea and FadeGroup are both in place.
//                                     This is the cheapest one to finish.
//   MoveIdentifier       line 8266  -- the scene-view overlay frame: opens a SceneViewPanel at the
//                                     configured corner, lets the caller draw the panel's header
//                                     row and body, gives the header row a Pan cursor and makes it
//                                     draggable, and on drag re-runs the anchor picker to move the
//                                     panel. Needs `toolOverlayAlignment` (read as an enum and
//                                     written back through `IntValue`). Everything else is ported:
//                                     SceneViewExtensions.GetSceneViewRect, Common.SceneViewPanel,
//                                     ADOEditorUtility.AddCursorRect / CaptureHotControl / Separator
//                                     / DrawAnchorPicker, and the `sceneViewPanelResizeHandle` and
//                                     `tooltipDragControlId` state fields.
//   WriteIdentifier      line 8249  -- the standard overlay panel: an icon-width spacer, a centred
//                                     rich-text title, and the settings button ported below, wrapped
//                                     in MoveIdentifier. Blocked only through MoveIdentifier; it
//                                     touches no setting itself.
//   FillIdentifier       line 8112  -- see the dead-path section.
//   CancelIdentifier     line 8117  -- see the dead-path section.
//
// Two neighbours of this region are named here only to say they are NOT this file's: MapIdentifier
// (line 8096, the [InitializeOnLoadMethod] hook) is already audited and deferred in
// ADOverhaul.Lifecycle.cs, and ConnectSerializer (line 7898) is a [SpecialName] accessor -- really a
// `updateCheckedToday` property, `ADOSettings.Instance().u_updateDay == RemoveConfiguration()` --
// that belongs with the update-day helper RemoveConfiguration (line 7434) in another region.
//
// ========================== THE DEAD NETWORK HALF, DELIBERATELY OMITTED =========================
//
// CancelIdentifier (line 8117) is the update check's request. It POSTs
//
//     { command: "getdownloadinfo", product_id: "No1lKII9IzcBAbihub6nCg==", version: <tool version> }
//
// through OrderIdentifier (line 7765) to
// https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand, and on success writes
// the reply into ADOSettings inside a SettingsDeferScope: `u_updateLink`, `u_updateMessage`,
// `u_updateChangelog`, `u_updateVersion`, `u_updateDay`, `u_announcement` (with literal "\\n" and
// "\n" unescaped to real newlines), `u_announcementLink` and `u_announcementLinkName`; it clears
// `u_announcementHidden` if the announcement text changed, then hands off to SetupIdentifier. It
// self-guards on `hasCheckedForUpdate` / `isCheckingForUpdate` and, when not forced, on the
// checked-today test.
//
// It is NOT PORTED. That host has been shut down and its DNS resolves to 0.0.0.0, so the request can
// only ever fail; what it would leave behind is an error toast on every editor start. Note what is
// being dropped and what is not: unlike the licensing verification (see the audit in
// ADOverhaul.Lifecycle.cs) this request carries no machine fingerprint, no session id and no licence
// key, and spawns no subprocess -- it is a plain version ping. It is omitted because it is dead, not
// because it is invasive.
//
// FillIdentifier (line 8112) is likewise not ported: its entire body is
// `CancelIdentifier(isparam: true)`. It is not a separate member so much as the forced-check entry
// point the hamburger menu's "Check For Update" item binds to, and it has no meaning without the
// request.
//
// LogoutIdentifier (line 8163), the "Download Update" button's handler, is listed above as blocked
// on ADOSettings, but it is worth being explicit that it is dead for a second, independent reason:
// the URL it downloads from is `u_updateLink`, which nothing but CancelIdentifier ever writes. With
// the request gone the setting stays empty and the button it hangs off never appears
// (CustomizeIdentifier gates it on the link being non-blank). Whoever ports the banner should decide
// whether to keep it at all; it is not stubbed here either way. For the record its body is a
// UnityWebRequest with a DownloadHandlerFile onto Assets/ADOverhaul.unitypackage, followed by
// ImportPackage and DeleteAsset -- and it carries a shipped bug: the error branch imports the
// half-written file before deleting it and throwing.
//
// SetupIdentifier (line 8185) is the local half and IS worth keeping -- it is deferred for
// ADOSettings, not dropped for being dead. It expires a "hide the announcement" flag once its
// recorded timestamp is more than seven days old (and clears the flag outright if the timestamp does
// not parse), then compares the cached remote version against `version` to decide whether to raise
// the update banner or mark the tool up to date. None of that needs the network; it reads only what
// a previous run cached. Its one shipped oddity, worth preserving when it lands: the up-to-date
// branch's forced-check path fires a `Task.Run` that sleeps three seconds and then touches
// ADOSettings and repaints from a worker thread.
//
// So, the reading of the update path this file commits to: the request (CancelIdentifier) and its
// forced-check wrapper (FillIdentifier) are dead and dropped; the download button
// (LogoutIdentifier) is dead in practice because its input is only ever produced by the request; the
// version comparison and the announcement expiry (SetupIdentifier) and both banners
// (CustomizeIdentifier, ConcatIdentifier) are live local code and should be finished once ADOSettings
// exists.
//
// ============================ COMPILER-GENERATED SCAFFOLDING, NOT PORTED ========================
//
// Decompiled lines 4481-4495 declare eight `public static GenericMenu.MenuFunction` fields inside a
// closure-cache class alongside a handful of Action fields. They are the cached delegate instances
// the C# compiler emits for the non-capturing lambdas InvokeIdentifier passes to
// GenericMenu.AddItem -- an implementation detail of how those lambdas are allocated once rather
// than per call, not members the author wrote. When InvokeIdentifier is ported the lambdas go back
// inline and the cache class is not reproduced. Nothing in this file needs them today.
//
// ================================= NOTES ON THE HAMBURGER MENU =================================
//
// Recorded now because the details are easy to lose and InvokeIdentifier is only deferred, not
// dropped. Its menu, in order, is: "Check For Update" (disabled -- AddItem with a null callback --
// while a check is running or one has already completed this session; it erases the cached update
// info out of SessionState before forcing a check); "Send Feedback"; a caller-supplied injection
// point plus separators; "Verify/On Display" and "Verify/On Project Load", two mutually exclusive
// toggles; "Documentation"; the `extraMenuLinks` block (one entry as a plain item, several under a
// "Samples/" submenu -- the array ships empty in both builds, so neither form ever appears);
// "Changelog"; and "ToS and Privacy Policy" linking to https://dreadrith.com/license-tos.
//
// Three things about it. First, every item except "Check For Update" and the ToS link is gated on
// `isLicensed`, so on an unlicensed install the menu is two entries long. Second, the "Documentation"
// and "Changelog" items are both guarded by `!string.IsNullOrWhiteSpace("")` -- a string literal, so
// the guard is constantly false and neither item can ever be added. That is not decompiler noise:
// both builds have it, and it is the residue of a per-product URL that was left blank for this
// product. Third, the two "Verify/..." toggles drive the licensing gate, and their settings are
// exactly the pair that ControllerEditor's EditorSettings port deliberately left out as
// licensing-gate remnants; whoever ports ADOSettings will presumably make the same call, in which
// case those two menu items go with them.
//
// ================================== 2019 vs 2022 ==============================================
//
// No behavioural divergence anywhere in this region. The 2019 counterparts are ADOverhaul2019
// ADOverhaul.cs lines 7879-8069 and 8087-8271, under a different obfuscated name set; the member
// ported below is `StartStruct` at 2019 line 8266 and is identical statement for statement. The
// differences elsewhere in the region are the usual ILSpy ones -- inverted branch polarity and the
// tuple deconstruction in the extra-links loop being spelled out differently.

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOverhaul
    {
        /// <summary>
        /// Draws the gear button that opens the ADOverhaul window, as used in the top-right of the
        /// scene-view overlay panel.
        /// </summary>
        /// <remarks>
        /// The same three lines appear inline in the PhysBone inspector's toolbar (decompiled
        /// ADOverhaul.cs line 3529) rather than calling this; that call site belongs to an unported
        /// region and should be pointed here when it lands.
        /// </remarks>
        internal static void DrawSettingsButton()
        {
            if (ADOEditorUtility.IconButton(ADOEditorUtility.contents.settings))
            {
                ADOverhaulWindow.ShowWindow();
            }
        }
    }
}
