// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the ADOverhaulWindow class, lines 35-166 of the current snapshot. Line numbers
// move with the snapshot; the member names below are the durable reference.
//
//   ADOverhaulWindow            -> lifted to a top-level type, line 35
//   enum EasyDynamicsFunctions  -> same, line 37
//   banner                      -> same, line 48
//   selectedFunction            -> same, line 50
//   editorFoldout               -> same, line 60
//   handlesFoldout              -> same, line 62
//   overlayFoldout              -> same, line 64
//   ShowWindow()                -> same, line 67
//   OnGUI()                     -> same, line 72
//   DrawEasyDynamicsGUI()       -> same, line 85
//   DrawSettingsGUI()           -> same, line 98
//   OnEnable()                  -> same, line 162
//
// The decompiled class is nested inside the static class ADOverhaul, whose members it calls
// unqualified. The window is lifted to a top-level type in the same namespace (ADOverhaul itself is
// ported as a static partial class in ADOverhaul/); call sites in the original read
// `ADOverhaul.ADOverhaulWindow` -- two of them (`PublishIdentifier`, decompiled line 8292, and the
// settings-gear button at line 3529) invoke ShowWindow, and `CalcIdentifier` (line 7776) repaints
// every open instance via Resources.FindObjectsOfTypeAll. `internal` keeps all three reachable.
// The two calls the window makes back into the outer class go through the bridge methods in
// ADOverhaul/ADOverhaul.Window.cs, which explains itself.
//
// ================================= LICENCE GATE, NOT PORTED ============================//
// None of that is ported. The vendor's validation backend is permanently shut down, so every one of
// those paths can now only fail, and the gate would lock out exactly the legitimate holders this
// restoration exists for. Its two remaining branches -- the ones taken when the tool considers
// itself licensed -- are also omitted, but for a different and much smaller reason, recorded here so
// nobody mistakes it for the same decision:
//
//   `if (m_Algo) { AwakeConfiguration(); return false; }`   -- the Send Feedback panel, line 7018.
//   `if (initializer) { BugReporter.DrawWindow(); return false; }` -- the bug reporter's IMGUI.
//
// Neither AwakeConfiguration nor BugReporter.DrawWindow is ported (see the DELIBERATELY NOT PORTED
// block in Common/BugReporter.cs), so both guards are omitted as unreachable-and-unported rather
// than as dead ends -- when the two panels land, this OnGUI should grow the guards back at the top
// of its body, ahead of DrawSettingsGUI.
//
// One half of that has since changed and is recorded here so the note is not read as still true:
// the hamburger menu (InvokeIdentifier, line 7936) HAS landed, as ADOverhaul.ShowContextMenu in
// ADOverhaul.Menus.cs, and its "Send Feedback" item toggles `feedbackPanelOpen` (the m_Algo flag).
// So that flag can now become true, and while it is true the shipped build would have replaced this
// window's contents with the feedback panel. Here it does nothing visible, because
// AwakeConfiguration -- the panel body -- is still unported. `bugReporterOpen` (the `initializer`
// flag) remains genuinely unsettable: nothing in the ported menu or anywhere else writes it.
//
// GetConfiguration() (decompiled line 7495, called by OnGUI between the separator and the header
// row) is dropped for the first reason rather than the second. It is a two-box read-only strip
// showing `License: <tier or "Personal">` and, when set, `Authorized For: <name>`. Both strings are
// only ever written by a successful verification response, so with the endpoint gone the row can
// only ever read "License: Personal" -- a permanent, misleading reminder of a gate that no longer
// applies. It is licence UI reachable only through the dead endpoint and is not reproduced.
//
// =========================== DEFERRED: NONE REMAIN IN OnGUI ==============================
//
// All three calls this header previously listed as deferred have since had their blockers cleared
// and are restored in OnGUI, in the decompiled order. Recorded here because the deferrals were
// documented and their removal should be traceable:
//
//   SortIdentifier()   line 7904 -- the boxed header row (hamburger button, update-available
//                                   toggle, "v<version>" label, credit link). Ported as
//                                   ADOverhaul.DrawToolHeader in ADOverhaul.Menus.cs (line 218),
//                                   `internal static`, both parameters optional. Called with no
//                                   arguments here, as decompiled.
//   ConcatIdentifier() line 8059 -- the announcement banner. Ported as
//                                   ADOverhaul.DrawAnnouncementBanner in ADOverhaul.Menus.cs
//                                   (line 402), `internal static`, no parameters.
//   banner.Draw(this)  line 81   -- the author's artwork strip. ADOEditorUtility.BannerDownloader
//                                   has landed (ADOEditorUtility.BannerDownloader.cs, decompiled
//                                   ADOEditorUtility.cs lines 918-1101); the type is
//                                   `internal sealed` and the EditorWindow overload of Draw is
//                                   `internal void Draw(EditorWindow window, float offsetX = 0f,
//                                   float reservedHeight = 60f)`. The `banner` field is restored
//                                   below with it.
//
// The earlier note here argued that substituting Common/RemoteTexture.cs or
// ControllerEditor/RemoteTextureView.cs for BannerDownloader would be wrong, because both paint a
// placeholder GUI.Box while the image is missing where BannerDownloader paints nothing at all. That
// reasoning stands but is now moot: the real type is ported and is used, so no substitution is
// being made. What it predicted about behaviour still holds and is worth keeping -- the URL now
// 404s, the download fails, the failure is swallowed silently, and CanDraw() short-circuits the
// draw, so the restored call paints nothing on every frame. It is restored because it is the
// shipped code, not because it will show anything.
//
// Fields, all `private static`, still omitted with their sole readers:
//   m_Param           line 44  -- selected index for a two-tab toolbar named by `prototype`;
//                                 nothing in either build reads or writes it.
//   prototype         line 46  -- { "Easy Dynamics", "Cosmetic" }, the labels of that same unbuilt
//                                 toolbar. Also unread.
//   m_Issuer, facade,
//   m_Composer,
//   m_Annotation      lines 52-58 -- four bools with no reader anywhere in either build.
//
// SHIPPED BUG PRESERVED (in OnEnable, and it is user-visible): opening this window in a scene that
// contains no VRCAvatarDescriptor throws a NullReferenceException. OnEnable refreshes the shared
// avatar selection, that refresh rebuilds the derived tables whenever no avatar was already set,
// and the 2022 rebuild dereferences the null descriptor. Unity logs the exception and the window
// opens and draws normally regardless. The full explanation is on ADOverhaul.RefreshAvatarTables;
// the 2019 build returns early there and does not throw. Ported as shipped.
//
// 2019 vs 2022: structurally identical -- same menu path, order, priority, window title, field set
// and method bodies, differing only in obfuscated names (the window's own members were renamed
// wholesale, e.g. selectedFunction -> m_Ref, editorFoldout -> _Instance). The only divergence
// anywhere near this file is the RefreshAvatarTables one noted above, which belongs to the outer
// class.
//
// Audit status: VERIFIED -- every member this file declares was diffed statement by statement
// against export/ADOverhaul2022 on 2026-08-05: the enum and its three values, all four surviving
// fields (banner, selectedFunction, the three foldout flags), ShowWindow's menu path/priority/title,
// OnGUI, DrawEasyDynamicsGUI, DrawSettingsGUI and OnEnable. DrawSettingsGUI matches call for call
// including the two DrawEnumPopup<PositionFlag> sites and the conditional labelColor row. The
// obfuscated GUIContent field names it reads through were resolved by their literal labels rather
// than taken on trust: m_RequestSerializer="Handle Size"=handleSize, issuerSerializer="Animated
// Foldouts"=animatedFoldouts, _FacadeSerializer="Show Name Labels"=showNameLabels,
// _MapperSerializer="Function"=function, queueSerializer="Tooltips"=tooltips, all confirmed against
// export ADOEditorUtility.cs lines 632-654. The three previously deferred OnGUI calls were verified
// unblocked and restored on this pass. What is deliberately absent -- the licence gate,
// GetConfiguration, the two unreachable feedback/bug-reporter guards and the six unread fields --
// is documented above and was re-confirmed against export, not merely carried over.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// The tool's main window: the settings pane for ADOverhaul's inspector, scene-view handles and
    /// scene-view overlays, under the shared DreadTools menu.
    /// </summary>
    internal sealed class ADOverhaulWindow : EditorWindow
    {
        /// <summary>
        /// The avatar-dynamics setup the unfinished "Easy Dynamics" tab would generate.
        /// </summary>
        /// <remarks>
        /// The pane that selects it ends in an "Under Development" help box in both shipped builds,
        /// and nothing acts on the choice.
        /// </remarks>
        private enum EasyDynamicsFunctions
        {
            EasyGrab,
            EasyTouch,
            EasyPat
        }

        /// <summary>
        /// The author's promotional artwork, drawn at the bottom of the window.
        /// </summary>
        /// <remarks>
        /// The URL 404s today, so the fetch fails, the failure is swallowed, and
        /// <see cref="ADOEditorUtility.BannerDownloader.CanDraw"/> short-circuits every draw. The
        /// window therefore shows nothing here -- which is exactly what the shipped build does now,
        /// and why this is restored as-is rather than substituted for one of the package's other
        /// remote-image types, both of which would paint a placeholder box the tool never had.
        /// </remarks>
        private static readonly ADOEditorUtility.BannerDownloader banner = new ADOEditorUtility.BannerDownloader(
            "https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png",
            autoDownload: true,
            "DreadBanner.png");

        /// <summary>The setup chosen in the Easy Dynamics pane. Never read outside that pane.</summary>
        private static EasyDynamicsFunctions selectedFunction = EasyDynamicsFunctions.EasyGrab;

        /// <summary>Expansion state of the "Editor" settings group.</summary>
        /// <remarks>
        /// The three foldout flags are static rather than per-window so that the pane reopens in the
        /// state it was left in, including after a domain reload destroys the window instance.
        /// </remarks>
        private static bool editorFoldout;

        /// <summary>Expansion state of the "Handles" settings group.</summary>
        private static bool handlesFoldout;

        /// <summary>Expansion state of the "Overlay" settings group.</summary>
        private static bool overlayFoldout;

        /// <summary>
        /// Opens the window, focusing it if it is already open.
        /// </summary>
        /// <remarks>
        /// The menu path, priority and window title are user-facing and are reproduced exactly:
        /// priority 6 places this below the other DreadTools entries, and the docked tab reads
        /// "Avatar Dynamics Overhaul" rather than the shorter menu name.
        /// </remarks>
        [MenuItem("DreadTools/ADOverhaul", false, 6)]
        internal static void ShowWindow()
        {
            GetWindow<ADOverhaulWindow>(false, "Avatar Dynamics Overhaul", true);
        }

        /// <summary>
        /// Draws the settings pane, the horizontal rule beneath it, and the window's footer: the
        /// tool header row, the announcement banner and the artwork.
        /// </summary>
        /// <remarks>
        /// See the file header: the shipped body wrapped all of this in the licence gate and drew
        /// the licence strip between the separator and the header row. The gate and the strip are
        /// dropped. The remaining three calls are the shipped ones, in the shipped order.
        /// </remarks>
        private void OnGUI()
        {
            DrawSettingsGUI();
            ADOEditorUtility.Separator();
            ADOverhaul.DrawToolHeader();
            ADOverhaul.DrawAnnouncementBanner();
            banner.Draw(this);
        }

        /// <summary>
        /// The unfinished "Easy Dynamics" pane: a target-avatar row, a picker for the setup to
        /// generate, and a notice that neither does anything yet.
        /// </summary>
        /// <remarks>
        /// Dead in both shipped builds and kept only for completeness. It is never called: the sole
        /// body that would have reached it is the two-tab toolbar the unread <c>m_Param</c> and
        /// <c>prototype</c> fields belong to, which was never written.
        /// </remarks>
        private void DrawEasyDynamicsGUI()
        {
            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                ADOverhaul.WindowDrawTargetAvatarSelector();
            }

            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                selectedFunction = (EasyDynamicsFunctions)EditorGUILayout.EnumPopup(
                    ADOEditorUtility.contents.function, selectedFunction);
            }

            EditorGUILayout.HelpBox("Under Development", MessageType.Info);
        }

        /// <summary>
        /// The settings form: three boxed foldouts over <see cref="ADOSettings"/> -- inspector,
        /// scene-view handles, scene-view overlays.
        /// </summary>
        /// <remarks>
        /// Each setting writes itself through on change, so there is no apply step; persistence is
        /// <see cref="SettingsPersistence"/>'s job. The label colour has no label of its own: it
        /// shares the "Show Name Labels" row and only appears while labels are on, since it has
        /// nothing to colour otherwise. The two overlay alignment popups follow the same idea, but
        /// stay visible and merely disabled when their overlay is off.
        /// </remarks>
        private void DrawSettingsGUI()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                editorFoldout = EditorGUILayout.Foldout(editorFoldout, "Editor", true);
                if (editorFoldout)
                {
                    EditorGUI.indentLevel++;
                    ADOSettings.instance.editorAnimatedFoldouts.Draw(ADOEditorUtility.contents.animatedFoldouts);
                    EditorGUI.indentLevel--;
                }
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                handlesFoldout = EditorGUILayout.Foldout(handlesFoldout, "Handles", true);
                if (handlesFoldout)
                {
                    EditorGUI.indentLevel++;

                    using (new GUILayout.HorizontalScope())
                    {
                        ADOSettings.instance.onSceneNameLabels.Draw(ADOEditorUtility.contents.showNameLabels);
                        if (ADOSettings.instance.onSceneNameLabels)
                        {
                            ADOSettings.instance.labelColor.Draw(GUIContent.none);
                        }
                    }

                    ADOSettings.instance.generalColor.Draw(ADOEditorUtility.contents.generalColor);
                    ADOSettings.instance.activeColor.Draw(ADOEditorUtility.contents.activeColor);
                    ADOSettings.instance.inactiveColor.Draw(ADOEditorUtility.contents.inactiveColor);
                    ADOSettings.instance.mixedColor.Draw(ADOEditorUtility.contents.mixedColor);
                    ADOSettings.instance.selectionColor.Draw(ADOEditorUtility.contents.selectionColor);
                    ADOSettings.instance.handleSizeMultiplier.DrawField(ADOEditorUtility.contents.handleSize);

                    EditorGUI.indentLevel--;
                }
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                overlayFoldout = EditorGUILayout.Foldout(overlayFoldout, "Overlay", true);
                if (!overlayFoldout)
                {
                    return;
                }

                EditorGUI.indentLevel++;

                using (new GUILayout.HorizontalScope())
                {
                    // The one label in this pane built inline rather than taken from the shared
                    // content table, as shipped.
                    ADOSettings.instance.onSceneToolSelection.Draw(
                        new GUIContent("Tool Overlay", "Displays the tool selection overlay on the scene view."));

                    using (new EditorGUI.DisabledScope(!ADOSettings.instance.onSceneToolSelection))
                    {
                        ADOSettings.instance.toolSelectionOverlayAlignment.DrawEnumPopup<PositionFlag>("Position");
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    ADOSettings.instance.onSceneEditingOverlay.Draw(ADOEditorUtility.contents.propertyAndTipOverlay);

                    using (new EditorGUI.DisabledScope(!ADOSettings.instance.onSceneEditingOverlay))
                    {
                        ADOSettings.instance.toolOverlayAlignment.DrawEnumPopup<PositionFlag>("Position");
                    }
                }

                ADOSettings.instance.onSceneTooltip.Draw(ADOEditorUtility.contents.tooltips);

                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Refreshes the shared avatar selection so the inspectors' dropdowns are populated by the
        /// time the user reaches them.
        /// </summary>
        /// <remarks>
        /// This throws in a scene with no avatar descriptor; see the SHIPPED BUG note in the file
        /// header and the remarks on <see cref="ADOverhaul.WindowRefreshAvatarSelection"/>.
        /// </remarks>
        private void OnEnable()
        {
            ADOverhaul.WindowRefreshAvatarSelection();
        }
    }
}
