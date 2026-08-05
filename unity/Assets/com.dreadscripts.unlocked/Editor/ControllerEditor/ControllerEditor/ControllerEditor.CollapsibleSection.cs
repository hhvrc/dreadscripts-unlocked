// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   DeleteAnnotation -> DrawCollapsibleSection, line 9920
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// This file exists to own one member. `DeleteAnnotation` is the collapsible-section helper that
// every section root of the window is built out of -- ControllerEditor.StateSection.cs and
// ControllerEditor.TransitionSection.cs both named it as their blocker, and both deferred it
// rather than port it, because whichever of them had ported it would have been claiming a member
// belonging to neither of their decompiled regions. It is ported here instead, in a file of its
// own, so that exactly one file claims decompiled line 9920.
//
// The obfuscated name is noise in both halves: the method deletes nothing and has nothing to do
// with annotations. `DrawCollapsibleSection` is what the body does -- it draws a section that is
// either collapsed to a single header button or expanded with a collapse handle beside it.
//
// The parameter names are the decompiled ones replaced with what they carry:
//   def             -> body
//   token           -> label
//   res             -> expanded
//   iscont2         -> boxed
//   visitor3counter -> index
//
// `index` is not free to renumber. It is the section's identity in EditorUtils' GUI-state relay,
// under the key `CollapsePart{index}`, and the call sites across the window's sections share one
// numbering: ControllerEditor.StateSection.cs records slots 3-6 for the state section's four rows
// and ControllerEditor.TransitionSection.cs records slots 0-2 for the transition section's three.
// Two sections given the same index would read each other's measured height.
//
// The decompiled body needed no control-flow reconstruction: lines 9920-9943 decompile as
// straight-line C# with a single early return, no residual switch dispatch and no `while (true)`,
// so none of the deobfuscator faults this package records elsewhere apply here.
//
// The rest of the small static GUI helpers surrounding this one in the decompiled file
// (RunAnnotation 9882, CloneAnnotation 9891, LoginAnnotation 9900, ReflectAnnotation 9910,
// CreateAnnotation 9945, NewAnnotation 9950) are not ported and are not claimed here -- this file
// takes only the member its two blocked callers named.
//
// ================================ DELIBERATE DEVIATION =========================================
//
// The thin collapse handle is drawn by the shipped code as
//
//     EditorUtils.Button(string.Empty, GUILayout.Height(...), GUILayout.Width(7f))
//
// which binds to the decompiled `Button(string, params GUILayoutOption[])` overload at
// EditorUtils.cs line 5722. The port of EditorUtils collapsed that overload and its styled twin
// (line 5717) into the single `Button(string, GUIStyle = null, params GUILayoutOption[])` in
// EditorUtils.Buttons.cs, so the call here must pass the style slot explicitly as `null`. That is
// the same value the shipped call ended up with: decompiled 5722 forwards a literal `null` style
// to 5732, and the shared ToggleButton at 5763 is what substitutes `GUI.skin.button` for it. The
// drawn result is identical; only the source text differs.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- every statement of DrawCollapsibleSection below was compared token for
// token with decompiled ControllerEditor.cs lines 9920-9943. The three EditorUtils members it
// calls were each followed to their ported declarations and their signatures checked against the
// decompiled originals: CompareQueue (EditorUtils.cs 5603) -> SetGuiStateOnEvent(string, object,
// EventType), OrderQueue (5593) -> GetGuiState<T>(string, T), and Button (5722) -> the collapsed
// Button overload recorded under DELIBERATE DEVIATION. BoolSetting's implicit bool conversion and
// Toggle() were checked in Editor/Common/Settings/ValueSettings.cs.

using System;
using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Collapsible section

        /// <summary>
        /// Draws one collapsible section: <paramref name="body"/> under a header the user can fold
        /// away, with the folded state persisted in <paramref name="expanded"/>.
        /// </summary>
        /// <param name="body">Draws the section's contents. Called only while it is expanded.</param>
        /// <param name="label">
        /// The header text. Shown only while the section is collapsed -- an expanded section is
        /// titled by whatever <paramref name="body"/> draws, not by this.
        /// </param>
        /// <param name="expanded">
        /// The setting that remembers whether this section is open. Both the collapsed header and
        /// the expanded collapse handle toggle it, so it is the only state the section has.
        /// </param>
        /// <param name="boxed">Draws the expanded body inside a box rather than flush.</param>
        /// <param name="index">
        /// Identifies this section in <see cref="EditorUtils"/>' GUI-state relay, under the key
        /// <c>CollapsePart{index}</c>. Every section of the window draws from one pool of these
        /// numbers, so two sections must never share one -- see the file header.
        /// </param>
        /// <remarks>
        /// <para>
        /// The two states are drawn by quite different code. Collapsed, the section is nothing but
        /// a toolbar button carrying <paramref name="label"/>, and clicking it reopens the section.
        /// Expanded, the label disappears and the body is drawn in a row with a 7px-wide button
        /// down its right-hand edge; that strip is the collapse handle.
        /// </para>
        /// <para>
        /// Making the handle full height is what the GUI-state relay is for. IMGUI can only measure
        /// the body once it has been laid out, and only on a Repaint pass, so the height is written
        /// down after the body is drawn and read back on the next pass to size the strip. The strip
        /// is therefore one frame behind the body it sits beside, which is invisible while the body
        /// keeps its size and shows as a brief mismatch on the frame the body changes height. Until
        /// the first Repaint has run the stored height is missing and the fallback of 0 applies, so
        /// a section drawn for the very first time gets a zero-height handle for one frame.
        /// </para>
        /// </remarks>
        private static void DrawCollapsibleSection(Action body, string label, BoolSetting expanded, bool boxed, int index)
        {
            if (!expanded)
            {
                if (EditorUtils.Button(label, EditorStyles.toolbarButton))
                {
                    expanded.Toggle();
                }

                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(boxed ? GUI.skin.box : GUIStyle.none))
                {
                    body();
                }

                // Written on Repaint, when GetLastRect is meaningful, and read back below on every
                // pass. See the remarks: this is the one-frame relay that gives the collapse handle
                // the height of the body it was drawn next to.
                string heightKey = $"CollapsePart{index}";
                EditorUtils.SetGuiStateOnEvent(heightKey, GUILayoutUtility.GetLastRect().height, EventType.Repaint);

                // The style slot is passed explicitly because the ported Button overloads are
                // collapsed; see DELIBERATE DEVIATION in the file header.
                if (EditorUtils.Button(string.Empty, null, GUILayout.Height(EditorUtils.GetGuiState(heightKey, 0f)), GUILayout.Width(7f)))
                {
                    expanded.Toggle();
                }
            }
        }

        #endregion
    }
}
