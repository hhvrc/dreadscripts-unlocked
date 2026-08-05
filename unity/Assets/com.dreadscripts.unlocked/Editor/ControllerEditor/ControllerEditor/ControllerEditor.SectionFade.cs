// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   IncludeAnnotation -> SeparatorIf, line 9874
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// This member's declaration is not near its call sites and its line number was not recorded
// anywhere in the package; it was found by searching decompiled/ for the two calls OnGUI makes at
// lines 8667 and 8669, which resolve to the declaration at 9874.
//
// CORRECTION TO ControllerEditor.Window.cs's HEADER. That file lists this member as "the animated
// show/hide wrapper the four sections are nested in". It is not. There is no wrapper, no animation
// and no nesting: the shipped body is
//
//     internal static void IncludeAnnotation(bool calci)
//     {
//         if (calci) EditorUtils.MapQueue();
//     }
//
// -- a conditional call to the horizontal-rule helper, which is already ported as
// EditorUtils.Separator (EditorUtils.Separators.cs, decompiled MapQueue at EditorUtils.cs line
// 5933). The four sections are drawn as four flat, consecutive calls in OnGUI; this member only
// draws the divider *between* two of them, and only when both sides of that divider are actually
// visible, so a hidden section does not leave a stray rule behind. Window.cs is shared and is not
// edited from here, per the task's instructions; the correction is recorded in this header instead
// so that the two files disagree in writing rather than silently.
//
// The file name ControllerEditor.SectionFade.cs was fixed by the task and is kept for continuity
// with the work item, even though "fade" describes the mistaken reading above rather than what the
// member does. Nothing in this file fades anything.
//
// The call sites are (decompiled 8666-8671); all but the last are written out in
// ControllerEditor.Window.cs's OnGUI:
//
//     ReflectVisitor();                                              // TRANSITION section
//     IncludeAnnotation(parserAnnotation && (_Descriptor || EditorSettings.GetInstance()
//                                            .editingController.GetValue()));
//     DestroyVisitor();                                              // state section
//     IncludeAnnotation(_Descriptor && EditorSettings.GetInstance().editingController.GetValue());
//     ValidateVisitor();                                             // CONTROLLER section
//     DefineVisitor();                                               // parameter section, deferred
//
// The two section labels in capitals were the other way round here until this pass, repeating the
// mislabel that ControllerEditor.Window.cs's header used to carry. ReflectVisitor (12529) draws the
// transition section and ValidateVisitor (11806) draws the controller section; the two files that
// port them, ControllerEditor.TransitionSection.cs and ControllerEditor.ControllerSection.cs, each
// set out the evidence. The obfuscated names say nothing either way.
//
// where `parserAnnotation` is transitionSectionVisible and `_Descriptor` is stateSectionVisible in
// this package's names (see ControllerEditor.State.cs). Those conditions belong to the call site,
// not to this member, and are not reproduced here.
//
// The shipped signature takes no thickness/spacing arguments and so always draws the default rule:
// EditorUtils.Separator()'s defaults of 2px thick and 10px of surrounding space.
//
// Audit status: VERIFIED -- the whole member is the four lines quoted above and was transcribed
// from decompiled lines 9874-9880 on this pass; both call sites at 8667 and 8669 were read in
// place. There is nothing else in the region this file claims.

using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Section separators

        /// <summary>
        /// Draws the divider rule between two of the window's sections, but only when
        /// <paramref name="draw"/> says both sections either side of it are on screen.
        /// </summary>
        /// <param name="draw">
        /// Whether the divider is wanted. Each call site computes this from the visibility flags of
        /// the two sections the rule would sit between.
        /// </param>
        /// <remarks>
        /// <para>
        /// The reason this exists at all rather than the call sites writing <c>if (...)
        /// EditorUtils.Separator();</c> inline is that the window's sections can each be toggled off
        /// independently. A rule drawn unconditionally after a section that turned out to be hidden
        /// would leave a line floating against nothing, or two rules stacked where two adjacent
        /// sections are both hidden. Making the separator itself conditional keeps that decision in
        /// one place and keeps the section calls in OnGUI a flat, readable sequence.
        /// </para>
        /// <para>
        /// It is deliberately a no-op when <paramref name="draw"/> is false rather than the caller
        /// skipping it: IMGUI lays out in the order calls are made, so a skipped call and a call
        /// that reserves no layout are the same thing, and the second form keeps the call sites'
        /// shape uniform.
        /// </para>
        /// </remarks>
        internal static void SeparatorIf(bool draw)
        {
            if (draw)
            {
                EditorUtils.Separator();
            }
        }

        #endregion
    }
}
