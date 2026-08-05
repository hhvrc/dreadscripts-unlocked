// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of these helpers.
// Reconstructed from both, which are identical apart from obfuscated names:
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     SortQueue     -> GetSceneViewRect,        line 6471
//     RegisterQueue -> SubtractSceneViewChrome, line 6476
//   reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//     AddStatus     -> GetSceneViewRect,        line 3634
//     ValidateStatus-> SubtractSceneViewChrome, line 3639
// The ADOverhaul2019 build declares the same pair at lines 3650 and 3655
// (ExcludeManager / AddManager) with identical bodies; no divergence between the three snapshots.
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// DEOBF-BUG(resolved): ControllerEditor's copy of the y-offset comes out as
// `while (true) { rect.y += 40f; }` -- an infinite loop that cannot be what shipped, and that both
// ADOverhaul snapshots render as the plain `if` reproduced here. Two independent builds of the same
// method disagreeing is what settles it. The same de4dot fault is confirmed on
// AnimatorTypeCache.ParameterEntry.Source, where tracing the original Reactor IL showed a plain `if`
// turned into a `while`. reverse-engineering/export/ keeps the loop until that recovery is fixed; do not restore it.
//
// Audit status: VERIFIED -- all three shipped copies diffed statement by statement against this
// file. Both methods match in every copy: the extension method is a single call through
// GUIUtility.ScreenToGUIRect(sceneView.position), and the chrome subtraction is the same +40f y
// offset under the not-2022 branch and the same 27f/21f height reduction. The one divergence is the
// DEOBF-BUG above -- ControllerEditor's copy renders the y offset as `while (true)`, both ADOverhaul
// copies as `if`, and this file follows the `if`.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Scene view geometry helpers shared by the floating-panel code.
    /// </summary>
    internal static class SceneViewExtensions
    {
        /// <summary>
        /// The area of <paramref name="sceneView"/> that a floating panel may occupy, in GUI space:
        /// the window rect converted out of screen coordinates and with the editor chrome subtracted.
        /// </summary>
        internal static Rect GetSceneViewRect(this SceneView sceneView)
        {
            return SubtractSceneViewChrome(GUIUtility.ScreenToGUIRect(sceneView.position));
        }

        /// <summary>
        /// Trims the chrome a scene view draws around its viewport out of <paramref name="rect"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EditorWindow.position"/> is the whole window, including the tab strip and the
        /// toolbar at the top and the status/mode strip at the bottom, none of which a panel may draw
        /// over. The measurements are hard-coded rather than queried because the widths live in
        /// Unity's internal styles.
        /// </para>
        /// <para>
        /// The two versions place the chrome differently. Before 2022 the toolbar is a strip inside
        /// the window, so 40 pixels are taken off the top and the origin moves down by that much, and
        /// 21 pixels of bottom strip are removed. From 2022 the toolbar became a floating overlay
        /// drawn over the viewport, so the top of the rect is already correct and nothing is added to
        /// <c>y</c> — only the taller 27-pixel bottom strip is removed. The overlay does still cover
        /// the top of the viewport, which is what <c>SceneViewPanel</c>'s separate 46-pixel nudge
        /// accounts for.
        /// </para>
        /// </remarks>
        internal static Rect SubtractSceneViewChrome(Rect rect)
        {
            // ControllerEditor.EditorUtils owns the shared Unity-version check; see the note in
            // ResizeHandle.Zones.cs about that dependency pointing the wrong way.
            bool isUnity2022 = ControllerEditor.EditorUtils.isUnity2022;

            if (!isUnity2022)
            {
                rect.y += 40f;
            }

            rect.height -= isUnity2022 ? 27f : 21f;
            return rect;
        }
    }
}
