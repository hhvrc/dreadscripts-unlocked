// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the ADOverhaulWindow class, lines 35-166 of the current snapshot. Line numbers
// move with the snapshot; the member names below are the durable reference.
//
//   ADOverhaulWindow                 -> same, line 51
//   enum EasyDynamicsFunctions       -> same, line 62
//   ShowWindow()                     -> same, line 84
//
// The decompiled class is nested inside the static class ADOverhaul, whose members it calls
// unqualified. ADOverhaul itself is not ported, so the window is lifted to a top-level type in the
// same namespace; call sites in the original read `ADOverhaul.ADOverhaulWindow` — two of them
// (`PublishIdentifier`, decompiled line 8292, and the settings-gear button at line 3529) invoke
// ShowWindow, and `CalcIdentifier` (line 7776) repaints every open instance via
// Resources.FindObjectsOfTypeAll. `internal` keeps all three reachable once they land.
//
// PARTIAL PORT. Everything below the window's identity is left out, because every one of these
// members reaches a member of the unported outer class or of the unported ADOSettings. None of it
// is stubbed; the list is here so the rest is mechanical once its dependencies land.
//
//   OnGUI()                  line 88  -- needs ADOverhaul.FlushConfiguration (line 7515),
//                                       ADOEditorUtility.DisableStatus (ADOEditorUtility.cs line
//                                       3255), ADOverhaul.GetConfiguration (line 7495),
//                                       ADOverhaul.SortIdentifier (line 7904),
//                                       ADOverhaul.ConcatIdentifier (line 8059), and the `banner`
//                                       field below.
//   DrawEasyDynamicsGUI()    line 101 -- needs ADOverhaul.PushConfiguration (line 6806) and the
//                                       `selectedFunction` field. Never called from anywhere in
//                                       either build: the only body that would have reached it is
//                                       the unfinished toolbar the `m_Param`/`prototype` pair below
//                                       belongs to, and it ends in a
//                                       HelpBox("Under Development", Info).
//   DrawSettingsGUI()        line 114 -- needs ADOSettings (line 751) and its Instance() accessor
//                                       (line 1569), plus eleven GUIContents from
//                                       ADOEditorUtility.contents and the three foldout bools.
//   OnEnable()               line 178 -- needs ADOverhaul.PrintConfiguration (line 6596), the
//                                       static avatar-selection fields m_Predicate (line 5622) and
//                                       _Collection (line 5624), and ADOverhaul.LogoutConfiguration
//                                       (line 6509).
//
// Fields, all `private static`, all deliberately omitted with their sole readers:
//   m_Param                  line 44  -- selected index for the two-tab toolbar named by
//                                       `prototype`; nothing in either build reads or writes it.
//   prototype                line 46  -- { "Easy Dynamics", "Cosmetic" }, the labels of that same
//                                       unbuilt toolbar. Also unread.
//   banner                   line 48  -- ADOEditorUtility.BannerDownloader (ADOEditorUtility.cs
//                                       line 918, not ported) for DreadBanner.png; read by OnGUI.
//   selectedFunction         line 50  -- read by DrawEasyDynamicsGUI.
//   m_Issuer, facade,
//   m_Composer, m_Annotation lines 52-58 -- four bools with no reader anywhere in either build.
//   editorFoldout,
//   handlesFoldout,
//   overlayFoldout           lines 60-64 -- read by DrawSettingsGUI.
//
// 2019 vs 2022: structurally identical — same menu path, order, priority, window title and field
// set, differing only in obfuscated names (the window's own members were renamed wholesale, e.g.
// selectedFunction -> m_Ref, editorFoldout -> _Instance). No behavioural divergence.

using UnityEditor;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// The tool's main window: the settings pane for ADOverhaul's scene-view handles and overlays,
    /// under the shared DreadTools menu.
    /// </summary>
    internal sealed class ADOverhaulWindow : EditorWindow
    {
        /// <summary>
        /// The avatar-dynamics setup the unfinished "Easy Dynamics" tab would generate.
        /// </summary>
        /// <remarks>
        /// Retained because it is the window's own nested type and the popup that selects it is
        /// still drawn; the tab that would host that popup was never wired up, and the pane it
        /// belongs to ends in an "Under Development" help box in both shipped builds.
        /// </remarks>
        private enum EasyDynamicsFunctions
        {
            EasyGrab,
            EasyTouch,
            EasyPat
        }

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
    }
}
