// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   NewVisitor -> DrawTransitionConditions, line 12672
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The body of the transition section's third sub-section, "Transition Conditions": the optional
// matching-options row, and whichever of the three condition ReorderableLists is current.
//
// This is the sub-section ControllerEditor.TransitionSection.cs draws unboxed, because the vertical
// scope opened here is the box.
//
// THE MATCHING OPTIONS. Three toggle buttons -- Match Parameter / Match Mode / Match Value -- shown
// only while the gear in the condition list's header has them turned on. They are what
// ConditionsMatch (ControllerEditor.ConditionMatching.cs) reads to decide which conditions of
// different transitions count as the same condition, so changing any of them has to rebuild the
// shared rows, which is what the change check around them does. Each draws in green when on and red
// when off, with the off state labelled "Ignore ..." rather than "Match ...", so the button says
// what it is doing rather than what it is called.
//
// THE ARROW KEYS. Up and down move the text cursor between threshold fields, which IMGUI does not
// do on its own: the fields are drawn by a ReorderableList and are not a focus chain. The control
// names are the "Threshold{n}" ones ControllerEditor.ConditionRow.cs assigns as it draws, so the
// regex here reads the current field's number back out of the focused control's name, steps it, and
// wraps with Mathf.Repeat.
//
// SHIPPED BUG: the wrap is taken modulo `list.count`, the number of *rows*, but the numbering it
// wraps is over *threshold fields* -- and Bool and Trigger rows have no threshold field. With any
// such row present the two counts differ, and stepping off the end of the list lands on a number no
// control has, so focus is lost instead of wrapping. Reproduced as shipped.
//
// The list is dereferenced without a null check, which is why RebuildConditionList
// (ControllerEditor.ConditionList.cs) had to be ported before this could be: with no list built,
// the section throws on its first frame rather than drawing nothing.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// None. Decompiled lines 12672-12712 contain no HMACSHA256 predicate; the section's licence gate is
// on its caller, DrawTransitionSection, and is documented in ControllerEditor.TransitionSection.cs.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// The list choice is written as nested conditionals where the decompiled body has
// `HasFocusedTransition() ? focusedConditionList : ((!showSharedConditions) ? allConditionList :
// sharedConditionList)`; the inner ternary is turned the right way up, which is ILSpy's usual
// branch-if-true rendering and not something the source can have said. Likewise the decompiled
// `bool flag2; bool flag = !(flag2 = current.keyCode == KeyCode.DownArrow) && current.keyCode ==
// KeyCode.UpArrow;` is written as the two plain assignments it is.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- diffed statement for statement against decompiled lines 12672-12712:
// the box scope, the showMatchingOptions guard, all three DrawButton calls with their on/off labels
// and their green/red colours, the change check and its RefreshSharedConditions call, the
// disabled scope's predicate, the three-way list choice, the KeyDown filter and the down/up pair,
// the "Threshold(\\d+)" pattern, the pre-increment/pre-decrement asymmetry inside Mathf.Repeat, the
// FocusTextInControl target string, and the DoLayoutList call. That range contains no `goto`, no
// residual `switch` dispatch, no `while (true)` and no unresolved `smethod_N`, so no deobfuscator
// fault applies to it.

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Transition conditions section

        /// <summary>
        /// Draws the "Transition Conditions" sub-section: the condition-matching options, and the
        /// condition editor itself.
        /// </summary>
        private void DrawTransitionConditions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                if ((bool)EditorSettings.Instance.showMatchingOptions)
                {
                    using (new GUILayout.HorizontalScope())
                    using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        EditorSettings.Instance.matchParameter.DrawButton(
                            "Match Parameter", "Ignore Parameter", true, Color.green, Color.red);
                        EditorSettings.Instance.matchMode.DrawButton(
                            "Match Mode", "Ignore Mode", true, Color.green, Color.red);
                        EditorSettings.Instance.matchValue.DrawButton(
                            "Match Value", "Ignore Value", true, Color.green, Color.red);

                        // What counts as "the same condition" has changed, so the shared rows have
                        // to be regrouped.
                        if (changeCheck.changed)
                        {
                            RefreshSharedConditions();
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(selectedTransitionEdits.Count == 0))
                {
                    ReorderableList list = HasFocusedTransition
                        ? focusedConditionList
                        : (showSharedConditions ? sharedConditionList : allConditionList);

                    Event current = Event.current;
                    if (current.type == EventType.KeyDown)
                    {
                        bool down = current.keyCode == KeyCode.DownArrow;
                        bool up = !down && current.keyCode == KeyCode.UpArrow;

                        if (down || up)
                        {
                            // The threshold fields are not a focus chain, so the move is done by
                            // reading the number back out of the focused control's name.
                            Match match = Regex.Match(GUI.GetNameOfFocusedControl(), "Threshold(\\d+)");
                            if (match.Success)
                            {
                                int number = int.Parse(match.Groups[1].Value);

                                // See SHIPPED BUG in the file header: the wrap is over row count,
                                // not over threshold-field count.
                                int next = (int)Mathf.Repeat(down ? ++number : --number, list.count);
                                EditorGUI.FocusTextInControl($"Threshold{next}");
                            }
                        }
                    }

                    list.DoLayoutList();
                }
            }
        }

        #endregion
    }
}
