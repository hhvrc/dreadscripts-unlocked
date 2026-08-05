// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   SortVisitor     -> CopyConditions, line 13005
//   RegisterVisitor -> PasteConditions, line 13022
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The condition clipboard: the pair behind the copy and paste icons in the condition list's header
// (ControllerEditor.ConditionListHeader.cs). The clipboard itself is the static
// `copiedConditions`, owned by ControllerEditor.State.cs.
//
// Copy takes whatever the header is showing -- the focused transition's conditions, or the shared
// set, or the whole selection's set -- and paste puts them on whatever the header is showing,
// except that paste never distinguishes shared from all: with no focused transition it appends to
// every selected transition. That asymmetry is shipped, not a porting choice. Pasting the shared
// set back onto the selection it came from therefore duplicates those conditions on every
// transition, which is what "paste" has to mean when the target is a multi-selection.
//
// Both are static, as shipped, and neither records an Undo entry -- also as shipped.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// Neither member carries one; decompiled lines 13005-13049 contain no HMACSHA256 predicate.
//
// ==================================== DEOBF-BUG ===============================================
//
// DEOBF-BUG(resolved): CopyConditions. The decompiled body is
//
//     if (HasFocusedTransition()) { list = focusedTransition.transition.conditions.ToList(); }
//     else
//     {
//         while (!showSharedConditions)
//         {
//         }
//         list = sharedConditionEditors.Select(sc => sc.condition).ToList();
//     }
//
// -- an empty non-terminating loop, which would hang the editor the moment the copy icon was
// pressed with the header on "All Conditions". That loop is not in the shipped product; it is
// de4dot's output. The deobfuscated IL shows it as `IL_001e: ldsfld showSharedConditions;
// brfalse.s IL_001d` where IL_001d is a lone `nop` -- the branch target de4dot was left with after
// it dropped a block.
//
// Re-running de4dot with its XOR-switch resolution disabled (DE4DOT_NO_XORSWITCH=1) recovers the
// dropped block, and the state machine resolves by hand with no ambiguity. The dispatch is
// `switch ((state ^ -1180238114) % 4)` over four cases:
//
//   * the focused arm falls out of the `brfalse` at IL_0005, assigns copiedConditions, then seeds
//     state -1293592683; (-1293592683 ^ -1180238114) % 4 == 3, which is the `ret` case.
//   * the non-focused arm seeds -1349569581 at IL_0092; that resolves to case 1, which tests
//     showSharedConditions. True takes `sharedConditionEditors.Select(...)` and joins the same
//     assignment-then-ret path.
//   * false recomputes the state as `(current * -1464941001) ^ 788357063`, which resolves to case
//     2 -- and case 2 is `allConditionEditors.Select(...).ToList()`, the arm the resolved output
//     lost, joining the same assignment.
//
// The two `Select` calls use two different cached delegate fields (`m_ProcessInitializer` with
// `CalculateProcessor`, and `producerInitializer` with `TestProcessor`), which is what a compiler
// emits for two separate `sc => sc.condition` lambdas in the source and confirms the arms were
// written out separately rather than as one ternary over the list. The three-armed `if` below is
// therefore the shipped source, and it is also exactly the three-way choice that
// CurrentConditionEditors (ControllerEditor.ConditionMatching.cs) makes for the same three cases.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- PasteConditions was diffed statement for statement against decompiled
// lines 13022-13046, including the order of its three trailing refresh calls and the fact that the
// non-focused arm iterates selectedTransitionEdits while the focused arm uses the focused
// transition directly. CopyConditions was diffed against decompiled lines 13005-13020 and its
// missing arm recovered from IL as recorded under DEOBF-BUG above.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition clipboard

        /// <summary>
        /// Copies whatever set of conditions the condition list is currently showing onto the
        /// clipboard.
        /// </summary>
        /// <remarks>
        /// The focused transition's conditions are taken from the transition itself rather than from
        /// its rows, so a row the user has edited but not yet committed is copied in its stored
        /// state. The other two arms read the rows, which is the only place the shared set exists.
        /// </remarks>
        private static void CopyConditions()
        {
            List<AnimatorCondition> conditions;

            if (HasFocusedTransition)
            {
                conditions = focusedTransition.transition.conditions.ToList();
            }
            else if (showSharedConditions)
            {
                conditions = sharedConditionEditors.Select(e => e.condition).ToList();
            }
            else
            {
                // Recovered from IL; see DEOBF-BUG in the file header.
                conditions = allConditionEditors.Select(e => e.condition).ToList();
            }

            copiedConditions = conditions;
        }

        /// <summary>
        /// Appends every condition on the clipboard to the focused transition, or to every selected
        /// transition when none is focused.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Rows are appended alongside the conditions as they are added, rather than the whole set
        /// being rebuilt afterwards, so that the newly pasted conditions are editable on the same
        /// frame. The three refresh calls that follow then put the other two row sets back in step:
        /// the shared set is recomputed from the selection, the whole-selection set is re-derived
        /// from its transitions -- which is what fixes up the indices the append invalidated -- and
        /// the reorderable list is rebuilt around whichever set is now current.
        /// </para>
        /// <para>
        /// No Undo entry is recorded, as shipped, so a paste cannot be undone in one step.
        /// </para>
        /// </remarks>
        private static void PasteConditions()
        {
            if (HasFocusedTransition)
            {
                foreach (AnimatorCondition condition in copiedConditions)
                {
                    focusedTransition.transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
                    focusedConditionEditors.Add(new ConditionMultiEditor(
                        focusedTransition.transition, focusedTransition.transition.conditions.Length - 1));
                }
            }
            else
            {
                foreach (AnimatorGraphReflection.TransitionEditionInfo edit in selectedTransitionEdits)
                {
                    foreach (AnimatorCondition condition in copiedConditions)
                    {
                        edit.transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
                        allConditionEditors.Add(new ConditionMultiEditor(
                            edit.transition, edit.transition.conditions.Length - 1));
                    }
                }
            }

            sharedConditionEditors = BuildSharedConditionEditors(selectedTransitions);
            RebuildAllConditionEditors();
            RebuildConditionList();
        }

        #endregion
    }
}
