// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   WriteVisitor   -> ConditionsMatch(AnimatorCondition, AnimatorCondition, bool, out bool[]), line 12859
//   ForgotVisitor  -> ConditionsMatch(AnimatorCondition, AnimatorCondition, bool), line 12905
//   StopVisitor    -> CurrentConditionEditors, line 12911
//   CheckVisitor   -> IntersectConditionEditors, line 12924
//   PrepareVisitor -> BuildConditionEditors, line 12951
//   AssetVisitor   -> BuildSharedConditionEditors, line 12961
//   ChangeVisitor  -> RebuildAllConditionEditors, line 12986
//
// One further member of this region is ported below without a line number of its own, because its
// decompiled line is already claimed elsewhere -- see the NOTES section:
//   UpdateVisitor -> RefreshSharedConditions
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// This file is the bottom of the condition-editor region: the matching rule that decides whether
// two AnimatorConditions are "the same condition", the three list builders on top of it, and the
// two refresh entry points the rest of the region calls. Nothing here draws anything.
//
// The obfuscated names carry no information -- WriteVisitor writes nothing, AssetVisitor touches no
// asset -- so every name below is derived from the body.
//
// UPDATEVISITOR AND ITS MISSING LINE NUMBER. `UpdateVisitor` (decompiled 12980) is ported here as
// RefreshSharedConditions, but its MAP entry deliberately carries no line number. Decompiled line
// 12980 is already claimed by Editor/ControllerEditor/EditorSettings/EditorSettings.ChangeHooks.cs,
// which recorded it as the settings seam `onMatchingOptionsChanged` back when this method was
// unported and said in its own header that "the ControllerEditor port assigns them; until it does,
// changing one of the settings below persists correctly and simply does not refresh the view it
// used to refresh". That claim is now stale -- the method exists -- and the correct fix is for
// ChangeHooks.cs's entry to become `UpdateVisitor() -> RefreshSharedConditions, line 12980, in
// ControllerEditor.ConditionMatching.cs`, with the seam assignment made wherever the window's
// initialisation lands. That is an edit to a file this port does not own, so it is flagged rather
// than made, and this file uses the no-line-number sub-entry form so the package does not gain a
// double claim on 12980 in the meantime. Nothing assigns EditorSettings.onMatchingOptionsChanged
// yet, so toggling "show matching options" still does not refresh the condition rows.
//
// The three list builders are what make the condition editor multi-edit. BuildConditionEditors
// turns one transition's conditions into one row each; IntersectConditionEditors keeps only the
// rows a second transition also has, marking the fields the two disagree on; and
// BuildSharedConditionEditors folds a whole selection down by repeating that. The matched flag is
// used as scratch space by the intersection pass and is cleared again before it returns, which is
// why ConditionMultiEditor documents it as "survived matching" rather than as state.
//
// CurrentConditionEditors is a method in the decompiled source and a property here, matching the
// treatment ControllerEditor.State.cs already gave HasFocusedTransition and
// ControllerEditor.ControllerContext.cs gave ActiveController.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// None of the members in this file carries one. The scattered HMACSHA256 predicate that the rest of
// this region is peppered with (see ControllerEditor.ConditionList.cs, .ConditionListHeader.cs and
// .ConditionRow.cs) does not appear anywhere between decompiled lines 12859 and 13004.
//
// ==================================== SHIPPED BUG =============================================
//
// ConditionsMatch dereferences `parameter` after a test that only guarantees that *one* of the two
// lookups found something. The decompiled body reads
//
//     if (animatorControllerParameter != null || animatorControllerParameter2 != null)
//     {
//         if (animatorControllerParameter.type != animatorControllerParameter2.type) return false;
//
// and the IL confirms that shape rather than it being an ILSpy inversion: at IL_0037 the first
// lookup is tested with `brtrue` into the body and the second with `brfalse` to the `return true`,
// so the fall-through case -- first lookup null, second non-null -- enters the body and throws a
// NullReferenceException. It is written the same way here, as the equivalent "both null -> match"
// early return, which is what that IL pair actually spells.
//
// The case is reachable only through the one path that lets the two parameter names differ: the
// user turning "Match Parameter" off while one of the two conditions names a parameter that is not
// in the controller. It is narrow, but it is the shipped behaviour and is not corrected here.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// ConditionsMatch is written as a chain of early returns where the decompiled body nests four
// `if`s and returns from the tail of each. The predicates are the same ones under De Morgan:
// decompiled `if ((!isres && !matchParameter) || matches[0]) { ...body... } return false;` is
// `if ((strict || matchParameter) && !matches[0]) return false;` followed by the body, and the mode
// test collapses the decompiled
//
//     (!isres && (type != Bool || !matchValue) && (type == Bool || !matchMode)) || matches[1]
//
// into the local `modeMatters`, which is exactly its negation's left half. Short-circuiting is
// preserved: with `strict` set, neither matchValue nor matchMode is read, as in the shipped IL.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- ConditionsMatch was diffed against decompiled lines 12859-12903 and
// against the IL of ControllerEditor::WriteVisitor branch by branch (the dispatch is not flattened
// there; the method is straight-line IL with forward branches only), including the exact predicate
// each of the three fieldMatches slots gates. The other six members were diffed statement for
// statement against decompiled lines 12905-13003. That whole range contains no `goto`, no residual
// `switch` dispatch, no `while (true)` and no unresolved `smethod_N`, so no deobfuscator fault
// applies to it.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition matching

        /// <summary>
        /// Decides whether two conditions are close enough to be edited as one row, and reports
        /// which of their three fields actually agreed.
        /// </summary>
        /// <param name="condition">The condition an existing row holds.</param>
        /// <param name="other">A candidate condition from another transition.</param>
        /// <param name="strict">
        /// Ignore the user's matching options and require every field to agree. Also forced on
        /// whenever the matching options are hidden, so that a user who has never opened them gets
        /// exact matching regardless of what the individual toggles happen to say.
        /// </param>
        /// <param name="fieldMatches">
        /// Parameter / mode / threshold, true where the two agreed. A slot the rule never reaches
        /// stays false, so this is only meaningful for the fields the parameter's type has: a
        /// trigger has neither mode nor threshold, and a bool has no threshold.
        /// </param>
        /// <remarks>
        /// <para>
        /// The rule walks the three fields in order and stops at the first one that both matters
        /// and disagrees. Which fields matter depends on the parameter type as well as on the
        /// settings: a Trigger condition has nothing to compare beyond its parameter, a Bool's
        /// "mode" is its value so the Match Value toggle governs it rather than Match Mode, and only
        /// Int and Float conditions have a threshold at all.
        /// </para>
        /// <para>
        /// A pair naming parameters that are both absent from the controller is treated as matching:
        /// there is no type to compare, so the parameter names having already been accepted is all
        /// the agreement available. See the SHIPPED BUG note in the file header for what happens
        /// when exactly one of them is absent.
        /// </para>
        /// </remarks>
        private static bool ConditionsMatch(AnimatorCondition condition, AnimatorCondition other,
            bool strict, out bool[] fieldMatches)
        {
            fieldMatches = new bool[3];

            if (!ActiveController)
            {
                return false;
            }

            strict |= !EditorSettings.Instance.showMatchingOptions;

            fieldMatches[0] = condition.parameter == other.parameter;
            if ((strict || (bool)EditorSettings.Instance.matchParameter) && !fieldMatches[0])
            {
                return false;
            }

            AnimatorControllerParameter parameter = FindParameter(condition.parameter, out _);
            AnimatorControllerParameter otherParameter = FindParameter(other.parameter, out _);

            // Both names unresolvable: nothing left to compare. See SHIPPED BUG for the one-sided case.
            if (parameter == null && otherParameter == null)
            {
                return true;
            }

            if (parameter.type != otherParameter.type)
            {
                return false;
            }

            AnimatorControllerParameterType type = parameter.type;
            if (type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }

            fieldMatches[1] = condition.mode == other.mode;

            // A bool's mode is its value, so it is the Match Value toggle that governs it.
            bool modeMatters = strict || (bool)(type == AnimatorControllerParameterType.Bool
                ? EditorSettings.Instance.matchValue
                : EditorSettings.Instance.matchMode);

            if (modeMatters && !fieldMatches[1])
            {
                return false;
            }

            if (type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }

            fieldMatches[2] = condition.threshold == other.threshold;
            if (strict || (bool)EditorSettings.Instance.matchValue)
            {
                return fieldMatches[2];
            }

            return true;
        }

        /// <inheritdoc cref="ConditionsMatch(AnimatorCondition, AnimatorCondition, bool, out bool[])"/>
        /// <remarks>For callers that only want the verdict, such as the row's "select every
        /// transition using this condition" button.</remarks>
        private static bool ConditionsMatch(AnimatorCondition condition, AnimatorCondition other, bool strict)
        {
            return ConditionsMatch(condition, other, strict, out _);
        }

        /// <summary>
        /// The rows the condition editor is currently showing: the focused transition's if one is
        /// pinned, otherwise the shared or the whole-selection set depending on the header toggle.
        /// </summary>
        private static List<ConditionMultiEditor> CurrentConditionEditors
        {
            get
            {
                if (HasFocusedTransition)
                {
                    return focusedConditionEditors;
                }

                if (showSharedConditions)
                {
                    return sharedConditionEditors;
                }

                return allConditionEditors;
            }
        }

        /// <summary>
        /// Narrows <paramref name="editors"/> to the rows <paramref name="transition"/> also has,
        /// attaching it to each survivor and marking the fields the two disagree on.
        /// </summary>
        /// <remarks>
        /// Each of the transition's conditions claims at most one row, and each row is claimed at
        /// most once, because the search skips rows already matched on this pass and stops at the
        /// first hit. Rows nothing claimed are dropped, which is what makes repeated application
        /// across a selection converge on the conditions every transition shares.
        /// </remarks>
        private static List<ConditionMultiEditor> IntersectConditionEditors(
            AnimatorTransitionBase transition, List<ConditionMultiEditor> editors)
        {
            for (int i = 0; i < transition.conditions.Length; i++)
            {
                foreach (ConditionMultiEditor editor in editors.Where(e => !e.matched))
                {
                    if (ConditionsMatch(editor.condition, transition.conditions[i], strict: false,
                            out bool[] fieldMatches))
                    {
                        editor.AddMatch(transition, i);
                        editor.MarkMixedValues(fieldMatches);
                        break;
                    }
                }
            }

            List<ConditionMultiEditor> unmatched = new List<ConditionMultiEditor>();
            foreach (ConditionMultiEditor editor in editors)
            {
                if (!editor.matched)
                {
                    unmatched.Add(editor);
                }

                // Cleared here rather than by the caller: the flag is scratch space for this pass
                // only, and the next transition must start from a clean set.
                editor.matched = false;
            }

            return editors.Except(unmatched).ToList();
        }

        /// <summary>One row per condition on <paramref name="transition"/>, matched to nothing yet.</summary>
        private static List<ConditionMultiEditor> BuildConditionEditors(AnimatorTransitionBase transition)
        {
            List<ConditionMultiEditor> editors = new List<ConditionMultiEditor>();
            for (int i = 0; i < transition.conditions.Length; i++)
            {
                editors.Add(new ConditionMultiEditor(transition, i));
            }

            return editors;
        }

        /// <summary>
        /// The rows for the conditions every transition in <paramref name="transitions"/> has: the
        /// first transition's rows, intersected with each of the rest in turn.
        /// </summary>
        private static List<ConditionMultiEditor> BuildSharedConditionEditors(List<AnimatorTransitionBase> transitions)
        {
            if (transitions.Count == 0)
            {
                return new List<ConditionMultiEditor>();
            }

            List<ConditionMultiEditor> editors = BuildConditionEditors(transitions[0]);
            for (int i = 1; i < transitions.Count; i++)
            {
                // Nothing survives an empty set, so stop rather than walk the rest of the selection.
                if (editors.Count == 0)
                {
                    return editors;
                }

                editors = IntersectConditionEditors(transitions[i], editors);
            }

            return editors;
        }

        /// <summary>
        /// Rebuilds the shared-condition rows from the current selection and re-points the list that
        /// draws them.
        /// </summary>
        /// <remarks>
        /// This is the entry point everything that can change what "shared" means calls: a changed
        /// selection, a changed matching option, an added or removed condition. See the file header
        /// for why it carries no decompiled line number of its own.
        /// </remarks>
        private static void RefreshSharedConditions()
        {
            sharedConditionEditors = BuildSharedConditionEditors(selectedTransitions);
            RebuildConditionList();
        }

        /// <summary>
        /// Rebuilds the whole-selection rows from scratch, one per condition of every transition the
        /// existing rows point at.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The transitions are recovered from the rows themselves rather than from the selection,
        /// through each row's first target. That is deliberate: this runs after conditions have been
        /// added to or removed from those transitions, when the stored per-row indices no longer
        /// line up with the arrays, and re-deriving the rows from the transitions is the only way to
        /// get them back in step.
        /// </para>
        /// <para>
        /// A transition reached by more than one row is expanded once, which is what the seen-list
        /// is for -- without it every row of an n-condition transition would contribute n new rows.
        /// </para>
        /// </remarks>
        private static void RebuildAllConditionEditors()
        {
            List<ConditionMultiEditor> editors = new List<ConditionMultiEditor>();
            List<AnimatorTransitionBase> seen = new List<AnimatorTransitionBase>();

            for (int i = 0; i < allConditionEditors.Count; i++)
            {
                AnimatorTransitionBase transition = allConditionEditors[i].targets[0].transition;
                if (seen.Contains(transition))
                {
                    continue;
                }

                seen.Add(transition);
                for (int j = 0; j < transition.conditions.Length; j++)
                {
                    editors.Add(new ConditionMultiEditor(transition, j));
                }
            }

            allConditionEditors = editors;
        }

        #endregion
    }
}
