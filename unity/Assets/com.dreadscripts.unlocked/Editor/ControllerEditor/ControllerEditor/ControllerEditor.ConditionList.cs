// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   MapVisitor  -> RebuildConditionList, line 11763
//   FillVisitor -> AddCondition, line 12814
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The condition editor is drawn by three separate ReorderableLists, one per row set -- focused,
// shared, whole-selection -- and only ever one of them at a time. RebuildConditionList is what
// creates whichever one is current; it is the member that ControllerEditor.TransitionSection.cs,
// ControllerEditor.SelectionSync.cs and ControllerEditor.Refresh.cs all named as their blocker.
//
// Why three lists rather than one re-pointed list: a ReorderableList binds to its backing IList at
// construction, and all three backing lists are replaced wholesale rather than mutated in place
// (see BuildSharedConditionEditors and RebuildAllConditionEditors in
// ControllerEditor.ConditionMatching.cs), so the list object has to be rebuilt every time its rows
// are. Keeping three fields rather than one means switching the header's Shared/All toggle does not
// discard the other set's list.
//
// The three share all three callbacks -- DrawConditionRow (ControllerEditor.ConditionRow.cs),
// DrawConditionListHeader (ControllerEditor.ConditionListHeader.cs) and AddCondition below -- and
// each of those re-derives which set it is working on from CurrentConditionEditors or from the
// same focused/shared flags, so none of them needs to know which list called it.
//
// They differ in exactly one flag: the whole-selection list shows its add button only when a single
// transition is selected, because adding a condition to several transitions at once through that
// list would append to each of them independently and immediately fall out of step with the rows.
// The shared list has no such problem -- adding there is meant to hit the whole selection -- and
// the focused list has only one transition to begin with.
//
// AddCondition seeds the new condition from the last shared row when there is one, so that adding a
// second condition next to an existing one starts from something the user has already set up rather
// than from the controller's first parameter. Note that it reads the *shared* rows even when the
// list being added to is the focused or whole-selection one; that is shipped.
//
// The `list` parameter of AddCondition is unused, as shipped -- the method works out its target
// from the focused/selection state rather than from the list that raised it. It is kept because it
// is the ReorderableList.AddCallbackDelegate signature.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// RebuildConditionList opens with the whole-body form of the scattered licence test: an inline
// `(Func<bool>)delegate { ... }` recomputing an HMACSHA256 over the licence key and the date/HWID
// stamp, compared against `licenseToken`, with `if (!that()) return;` in front of everything else.
// It is dropped on the package-wide basis recorded in ControllerEditor.TransitionSection.cs: the
// vendor's validation endpoint is gone, the predicate can only evaluate false, and an unlicensed
// RebuildConditionList leaves all three ReorderableLists null -- which does not merely hide the
// condition editor, it makes ControllerEditor.ConditionSection.cs throw on the first frame it
// draws. The port behaves as though the check passed.
//
// AddCondition carries no licence gate.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- RebuildConditionList was diffed against decompiled lines 11763-11809
// and AddCondition against 12814-12857: all six ReorderableList constructor arguments of each of
// the three lists, the three callback assignments, the seeding order of the new condition
// (shared-rows-last, then the controller's first parameter, then the empty-controller fallback),
// the If/Equals choice by parameter type, and the focused-versus-selection append. Neither range
// contains a `goto`, a residual `switch` dispatch, a `while (true)` or an unresolved `smethod_N`,
// so no deobfuscator fault applies to either.

using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition list

        /// <summary>
        /// Rebuilds whichever of the three condition <see cref="ReorderableList"/>s the editor is
        /// currently showing, around the row set that goes with it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only the current one is rebuilt. The other two keep whatever they were last built with,
        /// which is why switching the header's Shared/All toggle calls this again rather than
        /// relying on the list already existing.
        /// </para>
        /// <para>
        /// The focused arm also rebuilds its rows, which the other two do not: the shared and
        /// whole-selection sets are maintained by the matching passes, whereas the focused set is
        /// just one transition's conditions and is cheaper to re-derive than to keep in step.
        /// </para>
        /// <para>
        /// None of the three is draggable or offers a remove button. Reordering conditions has no
        /// meaning to Unity's evaluator, and removal is done by the per-row button that
        /// <see cref="DrawConditionRow"/> draws, because a row can stand for a condition on several
        /// transitions at once and the list's own remove button would only know about the row.
        /// </para>
        /// </remarks>
        private static void RebuildConditionList()
        {
            if (HasFocusedTransition)
            {
                focusedConditionEditors = BuildConditionEditors(focusedTransition.transition);
                focusedConditionList = new ReorderableList(focusedConditionEditors, typeof(ConditionMultiEditor),
                    draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
                {
                    drawElementCallback = DrawConditionRow,
                    drawHeaderCallback = DrawConditionListHeader,
                    onAddCallback = AddCondition
                };
            }
            else if (showSharedConditions)
            {
                sharedConditionList = new ReorderableList(sharedConditionEditors, typeof(ConditionMultiEditor),
                    draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
                {
                    drawElementCallback = DrawConditionRow,
                    drawHeaderCallback = DrawConditionListHeader,
                    onAddCallback = AddCondition
                };
            }
            else
            {
                // The add button is offered only for a single transition; see the file header.
                allConditionList = new ReorderableList(allConditionEditors, typeof(ConditionMultiEditor),
                    draggable: false, displayHeader: true, selectedTransitionEdits.Count == 1,
                    displayRemoveButton: false)
                {
                    drawElementCallback = DrawConditionRow,
                    drawHeaderCallback = DrawConditionListHeader,
                    onAddCallback = AddCondition
                };
            }
        }

        /// <summary>
        /// The condition list's add button: appends a new condition to the focused transition, or to
        /// every selected transition when none is focused.
        /// </summary>
        /// <param name="list">
        /// Unused, as shipped -- the target is worked out from the window's state. Present because
        /// it is the callback signature.
        /// </param>
        /// <remarks>
        /// <para>
        /// The new condition copies the last shared row when there is one, so that adding beside an
        /// existing condition starts from a parameter the user is already working with. Failing
        /// that it takes the controller's first parameter, with the mode that parameter's type
        /// admits -- <see cref="AnimatorConditionMode.If"/> for bools and triggers, which have no
        /// comparison, and <see cref="AnimatorConditionMode.Equals"/> for numbers.
        /// </para>
        /// <para>
        /// With no parameters in the controller at all, the condition is left naming "New
        /// Parameter", which is not a parameter that exists; the row then draws its
        /// parameter-not-found state, whose type dropdown is how the user creates it.
        /// </para>
        /// </remarks>
        private static void AddCondition(ReorderableList list)
        {
            AnimatorCondition condition = default(AnimatorCondition);

            if (sharedConditionEditors.Count > 0)
            {
                condition = sharedConditionEditors.Last().condition;
            }
            else if (ActiveController != null)
            {
                if (ActiveController.parameters.Length == 0)
                {
                    condition.parameter = "New Parameter";
                }
                else
                {
                    UnityEngine.AnimatorControllerParameterType type = ActiveController.parameters[0].type;
                    condition.mode = type == UnityEngine.AnimatorControllerParameterType.Bool
                                     || type == UnityEngine.AnimatorControllerParameterType.Trigger
                        ? AnimatorConditionMode.If
                        : AnimatorConditionMode.Equals;
                    condition.parameter = ActiveController.parameters[0].name;
                    condition.threshold = 0f;
                }
            }

            if (HasFocusedTransition)
            {
                focusedTransition.transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
            else
            {
                for (int i = 0; i < selectedTransitions.Count; i++)
                {
                    selectedTransitions[i].AddCondition(condition.mode, condition.threshold, condition.parameter);
                }
            }

            sharedConditionEditors = BuildSharedConditionEditors(selectedTransitions);
            RebuildConditionList();
        }

        #endregion
    }
}
