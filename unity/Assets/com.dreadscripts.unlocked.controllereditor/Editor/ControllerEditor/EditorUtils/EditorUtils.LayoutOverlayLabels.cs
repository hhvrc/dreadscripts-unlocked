// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CalculateQueue -> OverlayLabel(string, ...), line 5923
//   static TestQueue      -> OverlayLabel(string, ...), line 5928
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EditorUtils.OverlayLabels.cs ported the Rect-based label and recorded these two as left for their
// proper owner. They are one method: CalculateQueue has no `draw` parameter at all and hard-wires
// the flag to true when forwarding, which is already the Rect overload's default, so the
// obfuscator-split pair collapses the way EditorUtils.Buttons.cs collapses its own. Its other four
// parameters are TestQueue's minus `draw`, in the same order. The form kept below is TestQueue's,
// because it is the one the shipped call sites use and it matches the Rect overload's order; a call
// site written against CalculateQueue therefore has to skip `draw` by name.
//
// They live in a separate file from the Rect overload only because that file was another wave's and
// could not be edited at the time; the two belong together and can be merged whenever that is
// convenient.
//
// The counterpart in the other product is the same one-line forward: decompiled AwakeStatus in
// export/ADOverhaul2022 ADOEditorUtility.cs, forwarding GUILayoutUtility.GetLastRect() to
// EnableStatus. Neither is ported on the ADOverhaul side yet, so there is no package file to point
// at; the two products' copies are left independent, as everywhere else in this reconstruction.
// Audit status: VERIFIED -- the single body is TestQueue's forward, argument for argument, against
// export/. CalculateQueue's signature was re-read at the same time, which is what turned up the
// wrong `draw`-parameter claim this header used to carry.

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
