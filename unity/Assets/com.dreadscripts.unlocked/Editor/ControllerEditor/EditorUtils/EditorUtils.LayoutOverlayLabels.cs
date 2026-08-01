// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CalculateQueue -> OverlayLabel(string, ...), line 5923
//   static TestQueue      -> OverlayLabel(string, ...), line 5928
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EditorUtils.OverlayLabels.cs ported the Rect-based label and recorded these two as left for their
// proper owner. They are one method: CalculateQueue's body is TestQueue's with the `draw` flag
// hard-wired to true, which is already its default, so the obfuscator-split pair collapses the way
// EditorUtils.Buttons.cs collapses its own. Only the parameter *order* differs between the two
// decompiled signatures -- CalculateQueue puts `draw` last and TestQueue puts it second -- and the
// form kept below is TestQueue's, because it is the one the shipped call sites use and it matches
// the Rect overload's order.
//
// They live in a separate file from the Rect overload only because that file is another wave's and
// may not be edited; the two belong together and can be merged whenever the ownership constraint is
// lifted.
//
// The counterpart in the other product, ADOEditorUtility.OverlayLabel(string, ...)
// (Editor/ADOverhaul/ADOEditorUtility/ADOEditorUtility.Gui.cs), is the same one-line forward. The
// two copies are left independent, as everywhere else in this reconstruction.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Draws an overlay label on the control just emitted by the layout system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rect comes from <see cref="GUILayoutUtility.GetLastRect"/>, so this must be called
        /// immediately after the control it annotates and from inside the same layout group.
        /// Anything drawn in between -- including a call that itself emits a control -- retargets the
        /// label onto the wrong rect, and the failure is silent because the label still draws, just
        /// somewhere else.
        /// </para>
        /// <para>
        /// See
        /// <see cref="OverlayLabel(Rect, string, bool, float, float, bool, GUIStyle)"/> for what the
        /// remaining parameters do; every one of them is forwarded unchanged.
        /// </para>
        /// </remarks>
        internal static void OverlayLabel(string text, bool draw = true, float reservedWidth = 0f,
            float inset = 0f, bool alignLeft = true)
        {
            OverlayLabel(GUILayoutUtility.GetLastRect(), text, draw, reservedWidth, inset, alignLeft);
        }
    }
}
