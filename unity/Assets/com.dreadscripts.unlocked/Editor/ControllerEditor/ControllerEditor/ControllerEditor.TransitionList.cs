// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   DeleteVisitor -> DrawSelectedTransitionList, line 12544
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The body of the transition section's first sub-section, the one whose header reads "Transition
// Count: n". It is the selection viewer: one clickable row per selected transition, laid out in
// three columns, with a deselect cross per row and one for the whole selection.
//
// Clicking a row's name focuses that transition, which is what pins the condition editor to it;
// clicking the focused row again unfocuses. Those two paths do quite different amounts of work.
// Focusing re-points three things at the new transition -- the condition rows, the transition
// inspector's SerializedObject, and the property handles read out of it -- and forces the
// Shared/All toggle to Shared, since a focused transition has only its own conditions to show.
// Unfocusing calls SyncSelection instead, which re-derives all of that from the selection as a
// whole; that is the same path a selection change takes, so unfocusing and re-selecting land in the
// same state.
//
// The three-column layout is done by counting rows rather than by measuring: the column height is
// ceil(count / 3) and a new vertical scope is opened every time that many rows have been drawn.
// With a count that is not a multiple of three the last column is short, and with one or two
// transitions there is a single column. Rows whose transition has gone null -- deleted from under
// the selection -- are skipped by the Where() and still count towards nothing, so a stale selection
// draws short columns rather than blank rows.
//
// The whole grid is disabled when neither a plain nor a state transition is selected, but is still
// laid out, and in that case draws a single empty label so the section keeps a row's height instead
// of collapsing to nothing.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// None. Decompiled lines 12544-12611 contain no HMACSHA256 predicate; the section's licence gate is
// on its caller, DrawTransitionSection, and is documented in ControllerEditor.TransitionSection.cs.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// The decompiled body renders its outer `EditorGUI.DisabledScope` as an explicit try/finally around
// a local, which is ILSpy's rendering of `using` over a constrained value type; it is written back
// as `using`, matching the two `GUILayout.HorizontalScope` uses in the same method that ILSpy did
// render as `using`.
//
// The row click is written as `if (focused) { unfocus } else { focus }` where the decompiled body
// has `if (!flag) { focus } else { unfocus }`, ILSpy's usual branch-if-true rendering. The arms and
// their guard are unchanged.
//
// `item.DisplayName()` is read as a property and `EditorUtils.contents()` / `styles()` likewise,
// per the decisions recorded in AnimatorGraphReflection.TransitionEditionInfo.cs,
// EditorUtils.Contents.cs and EditorUtils.Styles.cs.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- diffed statement for statement against decompiled lines 12544-12611:
// the two FlexibleSpace calls around the "Editing n Transitions" label, the whole-selection
// deselect and its Except(), the disabled predicate, the ceil(count / 3f) column height and the
// column-break bookkeeping, the null-transition filter, the per-row ArrayUtility.Remove deselect,
// the focused/unfocused style choice, and both arms of the row click with their differing refresh
// calls. That range contains no `goto`, no residual `switch` dispatch, no `while (true)` and no
// unresolved `smethod_N`, so no deobfuscator fault applies to it.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Selected transition list

        /// <summary>
        /// Draws the "Transition Count" sub-section: every selected transition as a clickable row,
        /// in three columns, each with a deselect button.
        /// </summary>
        private void DrawSelectedTransitionList()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (EditorUtils.Button(EditorUtils.contents.deselect, EditorUtils.styles.centeredIcon,
                        GUILayout.Width(25f)))
                {
                    Selection.objects = Selection.objects.Except(selectedTransitions).ToArray();
                }

                GUILayout.Label($"Editing {selectedTransitionEdits.Count} Transitions");
                GUILayout.FlexibleSpace();
            }

            using (new EditorGUI.DisabledScope(!hasPlainTransitionSelected && !hasStateTransitionSelected))
            {
                int columnHeight = Mathf.CeilToInt((float)selectedTransitionEdits.Count / 3f);
                int drawnInColumn = 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                if (hasPlainTransitionSelected || hasStateTransitionSelected)
                {
                    foreach (AnimatorGraphReflection.TransitionEditionInfo edit in
                             selectedTransitionEdits.Where(e => e.transition != null))
                    {
                        if (drawnInColumn == columnHeight)
                        {
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                            drawnInColumn = 0;
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            if (EditorUtils.Button(EditorUtils.contents.deselect, EditorUtils.styles.centeredIcon,
                                    GUILayout.Width(25f)))
                            {
                                UnityEngine.Object[] objects = Selection.objects;
                                ArrayUtility.Remove(ref objects, edit.transition);
                                Selection.objects = objects;
                            }

                            bool focused = edit.transition == focusedTransition.transition;

                            if (EditorUtils.Button(edit.DisplayName,
                                    focused ? EditorUtils.styles.linkLabel : GUI.skin.label,
                                    GUILayout.MinWidth(1f)))
                            {
                                if (focused)
                                {
                                    // Unfocus, then let the ordinary selection sync rebuild
                                    // everything the focus was overriding.
                                    focusedTransition = default(AnimatorGraphReflection.TransitionEditionInfo);
                                    SyncSelection();
                                }
                                else
                                {
                                    focusedTransition = edit;

                                    // A focused transition has only its own conditions, so the
                                    // Shared/All toggle has nothing to choose between.
                                    showSharedConditions = true;

                                    RebuildConditionList();
                                    RebuildTransitionInspector();
                                    RefreshInspectorProperties();
                                }
                            }
                        }

                        drawnInColumn++;
                    }
                }
                else
                {
                    // Keeps the disabled section a row tall rather than letting it collapse.
                    GUILayout.Label(string.Empty);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
    }
}
