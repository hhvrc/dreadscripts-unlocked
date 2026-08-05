// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ConnectAlgo   -> MergeTransitions, line 14593
//   ViewAlgo      -> SplitTransitions, line 15035
//   FlushAlgo     -> GroupStateTransitionsBySource, line 14562
//   AwakeAlgo     -> SelectedAnyStateTransitions, line 14539
//   ResetAlgo     -> SelectedEntryTransitions, line 14553
//   CalculateAlgo -> DuplicateAsStateTransition, line 14650
//   PopThread     -> MergeStateTransitions, line 7564
//   ComputeThread -> MergeEntryTransitions, line 7585
//   MoveThread    -> MergeAnyStateTransitions, line 7601
//   VisitThread   -> SplitEntryTransition, line 7769
//   DefineThread  -> SplitAnyStateTransition, line 7783
//   StartThread   -> SplitStateTransition, line 7797
//
// The compiler-generated closures these were hoisted into. Each becomes ordinary parameters and
// locals here; the six methods above keep their own entries:
//   _003C_003Ec__DisplayClass410_1 -> dissolved into SelectedAnyStateTransitions, lines 7536-7549
//     _AccountReg -> the lambda parameter t
//     SetupThread, EnableThread -> the two Any() predicates
//   _003C_003Ec__DisplayClass413_0 -> dissolved into MergeTransitions, lines 7552-7626
//     tokenReg  -> the merged parameter threaded through the three merge helpers
//     codeReg   -> the local anyStateTransitions
//     m_DicReg  -> the local entryTransitions
//     _InvocationReg, roleReg -> not ported, cached-delegate fields
//     ConcatThread, CallThread -> the two destination-grouping predicates
//   _003C_003Ec__DisplayClass413_1 -> dissolved into MergeStateTransitions, lines 7629-7639
//     _ParamReg, CancelThread -> the local target and its condition-copy lambda
//   _003C_003Ec__DisplayClass413_2 -> dissolved into MergeEntryTransitions, lines 7642-7652
//     _TokenizerReg, CountThread -> the same pair
//   _003C_003Ec__DisplayClass413_3 -> dissolved into MergeAnyStateTransitions, lines 7655-7665
//     _ComparatorReg, DisableThread -> the same pair
//   _003C_003Ec__DisplayClass432_0 -> dissolved into SplitTransitions, lines 7765-7811
//     _ComposerReg -> the created parameter threaded through the three split helpers
//   _003C_003Ec__DisplayClass432_1 -> dissolved into SplitEntryTransition, lines 7814-7827
//     m_RepositoryReg, mappingReg, ReadThread -> the captured transition, the accumulator, the lambda
//   _003C_003Ec__DisplayClass432_2 -> dissolved into SplitAnyStateTransition, lines 7830-7843
//     baseReg, containerReg, SelectThread -> the same three
//   _003C_003Ec__DisplayClass432_3 -> dissolved into SplitStateTransition, lines 7846-7861
//     m_ClassReg, mockReg, instanceReg, RemoveThread -> the same, plus the captured source state
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The merge/split pair at the right-hand end of the condition list's header
// (ControllerEditor.ConditionListHeader.cs), and the four helpers they share. Both rewrite
// transitions rather than conditions, but they are condition operations: split turns one transition
// carrying n conditions into n transitions carrying one each, and merge is its inverse, collapsing
// the transitions between one pair of endpoints into a single transition carrying all of their
// conditions. That is why this file sits under the mandated Condition* name -- it is the region the
// condition editor reaches, not part of the transition list.
//
// The file name aside, nothing here is reachable from anywhere else in the package yet.
//
// THE THREE KINDS OF TRANSITION. Unity keeps state transitions on their source state, Any State
// transitions on the layer's root machine, and entry transitions on the current machine, and each
// needs its own add/remove pair. That is the whole reason both operations are three methods rather
// than one: the shapes are identical and only the two calls in the middle differ.
//
// GROUPING. Merge groups by destination twice over. First by source -- GroupStateTransitionsBySource
// buckets the selected state transitions by the state they leave, consuming each transition as it
// goes so a transition cannot land in two buckets -- and then, inside each bucket, by the state
// they arrive at. Only transitions agreeing on both endpoints can be merged, since a merged
// transition has one of each. Any State and entry transitions have no source to group by, so they
// are grouped by destination only.
//
// ASYMMETRY, SHIPPED. MergeStateTransitions returns the single transition untouched when its group
// has one member; the Any State and entry versions have no such guard and replace a lone transition
// with a fresh copy of itself. Both are ported as shipped.
//
// UNDO. Merge registers an undo entry for each duplicated state transition, through
// DuplicateAsStateTransition, and none for the removals or for the Any State and entry paths. Split
// registers one per created state transition and none for entry or Any State. Neither operation is
// undoable as a whole. Shipped, and not corrected.
//
// SPLIT KEEPS THE SOURCE OF THE COPIES DIFFERENT. SplitEntryTransition copies with
// EditorUtility.CopySerializedManagedFieldsOnly where the other two use CopySerialized, and
// SplitStateTransition goes through DuplicateAsStateTransition, which also adds the copy to the
// controller asset and registers the undo. Those three differ in the decompiled source and are not
// unified here.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// None. Neither decompiled 14539-14591 and 14593-14648 nor 14650-14658 and 15035-15098 contains an
// HMACSHA256 predicate; this region is one of the few in the class the obfuscator's scattered
// licence test did not reach.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// Three dead statements are dropped, each an allocation the very next statement overwrites without
// anything reading it: `List<AnimatorStateTransition> list4 = new List<AnimatorStateTransition>();`
// and its two siblings in MergeTransitions' three grouping loops (decompiled 14618, 14628, 14638),
// and the bare `new List<AnimatorStateTransition>();` in SplitTransitions' state loop (decompiled
// 15079), which is not even assigned.
//
// `if (!(spec.GetType() == cust.GetType()))` style inversions are ILSpy's rendering of
// branch-if-true and are written the way round the source must have had them, as are
// `if (!HasFocusedTransition())`, `if (t.Count != 1)` in MergeStateTransitions and
// `if (conditions.Length != 0)` in the three split helpers.
//
// The three merge helpers and the three split helpers take the accumulator list as a parameter,
// where the shipped code reaches it as a field of the closure they were hoisted into. That is the
// same thing written out: the closure exists only because those methods are local to the operation.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- diffed statement for statement against decompiled lines 14539-14591,
// 14593-14648, 14650-14658, 15035-15098 and the nine closure declarations listed above: the
// bucketing order of both grouping passes, the consume-as-you-go Except() in each, the three
// add/remove pairs on each side, which copy call each of the six helpers uses, the undo labels
// "DuplicatedTransition" (merge) and none elsewhere, the entry-kind switch in SplitTransitions with
// its 2/1/0 case order, and the trailing Selection.objects concatenation and SetDirty in both
// operations. None of those ranges contains a `goto`, a residual `switch` dispatch, a
// `while (true)` or an unresolved `smethod_N` -- the three `while (list.Count > 0)` loops in
// MergeTransitions are the genuine consume-until-empty grouping loops, and the `switch (num)` in
// SplitTransitions is the genuine three-way kind test -- so no deobfuscator fault applies.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Transition merge and split

        /// <summary>
        /// The selected Any State transitions of the current layer whose destination is reachable
        /// from the machine on screen.
        /// </summary>
        /// <param name="transitions">
        /// The set to filter; the current state-transition selection when omitted.
        /// </param>
        /// <remarks>
        /// Any State transitions live on the layer's root machine rather than on the machine the
        /// graph is showing, so they have to be filtered by destination to get the ones that belong
        /// to the current view -- either a state or a sub-machine of it.
        /// </remarks>
        private static List<AnimatorStateTransition> SelectedAnyStateTransitions(
            List<AnimatorStateTransition> transitions = null)
        {
            if (transitions == null)
            {
                transitions = selectedStateTransitions;
            }

            return RootStateMachine.anyStateTransitions.Where(t =>
                transitions.Contains(t)
                && (ActiveStateMachine.states.Any(c => t.destinationState == c.state)
                    || ActiveStateMachine.stateMachines.Any(c => t.destinationStateMachine == c.stateMachine)))
                .ToList();
        }

        /// <summary>The selected transitions that are entry transitions of the machine on screen.</summary>
        /// <param name="transitions">The set to filter; the current transition selection when omitted.</param>
        private static List<AnimatorTransitionBase> SelectedEntryTransitions(
            List<AnimatorTransitionBase> transitions = null)
        {
            if (transitions == null)
            {
                transitions = selectedTransitions;
            }

            return transitions.Where(t => ActiveStateMachine.entryTransitions.Contains(t)).ToList();
        }

        /// <summary>
        /// Buckets <paramref name="transitions"/> by the state they leave, dropping states that own
        /// none of them.
        /// </summary>
        /// <param name="transitions">The set to bucket; the current state-transition selection when omitted.</param>
        /// <remarks>
        /// Each transition lands in exactly one bucket: the working set has each bucket's contents
        /// removed from it as the bucket is filled, so a transition already claimed cannot be
        /// claimed again by a later state. Only states of the machine on screen are considered.
        /// </remarks>
        private static (AnimatorState, List<AnimatorStateTransition>)[] GroupStateTransitionsBySource(
            List<AnimatorStateTransition> transitions = null)
        {
            if (transitions == null)
            {
                transitions = selectedStateTransitions;
            }

            List<AnimatorState> sources = new List<AnimatorState>();
            for (int i = 0; i < ActiveStateMachine.states.Length; i++)
            {
                if (ActiveStateMachine.states[i].state.transitions.Any(t => transitions.Contains(t)))
                {
                    sources.Add(ActiveStateMachine.states[i].state);
                }
            }

            (AnimatorState, List<AnimatorStateTransition>)[] groups =
                new (AnimatorState, List<AnimatorStateTransition>)[sources.Count];

            for (int i = 0; i < sources.Count; i++)
            {
                List<AnimatorStateTransition> owned = new List<AnimatorStateTransition>();
                for (int j = 0; j < transitions.Count; j++)
                {
                    if (sources[i].transitions.Contains(transitions[j]))
                    {
                        owned.Add(transitions[j]);
                    }
                }

                transitions = transitions.Except(owned).ToList();
                groups[i] = (sources[i], owned);
            }

            return groups;
        }

        /// <summary>
        /// A new <see cref="AnimatorStateTransition"/> with the same serialised state as
        /// <paramref name="transition"/>, added to the same asset and registered for undo.
        /// </summary>
        /// <remarks>
        /// The copy is not attached to anything; the caller decides where it goes. Its
        /// <see cref="Object.hideFlags"/> are re-applied after the copy because CopySerialized does
        /// not carry them.
        /// </remarks>
        private static AnimatorStateTransition DuplicateAsStateTransition<T>(T transition)
            where T : AnimatorTransitionBase
        {
            AnimatorStateTransition copy = new AnimatorStateTransition();
            EditorUtility.CopySerialized(transition, copy);
            Undo.RegisterCreatedObjectUndo(copy, "DuplicatedTransition");
            AssetDatabase.AddObjectToAsset(copy, AssetDatabase.GetAssetPath(transition));
            copy.hideFlags = transition.hideFlags;
            return copy;
        }

        /// <summary>
        /// Replaces a group of state transitions sharing both endpoints with one transition carrying
        /// all of their conditions.
        /// </summary>
        /// <param name="group">Transitions leaving <paramref name="source"/> for one destination.</param>
        /// <param name="source">The state they leave.</param>
        /// <param name="merged">Collects what the operation ends up selecting.</param>
        private static void MergeStateTransitions(List<AnimatorStateTransition> group, AnimatorState source,
            List<AnimatorTransitionBase> merged)
        {
            // A group of one is already merged; keep the original rather than replace it with a copy.
            if (group.Count == 1)
            {
                merged.Add(group[0]);
                return;
            }

            AnimatorStateTransition target = DuplicateAsStateTransition(group[0]);
            source.RemoveTransition(group[0]);

            for (int i = 1; i < group.Count; i++)
            {
                group[i].conditions.ForEach<AnimatorCondition>(c =>
                    target.AddCondition(c.mode, c.threshold, c.parameter));
                source.RemoveTransition(group[i]);
            }

            source.AddTransition(target);
            merged.Add(target);
        }

        /// <summary>
        /// The entry-transition form of <see cref="MergeStateTransitions"/>. Note it has no
        /// single-member shortcut, as shipped.
        /// </summary>
        private static void MergeEntryTransitions(List<AnimatorTransitionBase> group,
            List<AnimatorTransitionBase> merged)
        {
            AnimatorTransitionBase target = ActiveStateMachine.AddEntryTransition(group[0].destinationState);
            EditorUtility.CopySerialized(group[0], target);
            ActiveStateMachine.RemoveEntryTransition((AnimatorTransition)group[0]);

            for (int i = 1; i < group.Count; i++)
            {
                group[i].conditions.ForEach<AnimatorCondition>(c =>
                    target.AddCondition(c.mode, c.threshold, c.parameter));
                ActiveStateMachine.RemoveEntryTransition((AnimatorTransition)group[i]);
            }

            merged.Add(target);
        }

        /// <summary>
        /// The Any State form of <see cref="MergeStateTransitions"/>. Also has no single-member
        /// shortcut.
        /// </summary>
        private static void MergeAnyStateTransitions(List<AnimatorStateTransition> group,
            List<AnimatorTransitionBase> merged)
        {
            AnimatorStateTransition target = ActiveStateMachine.AddAnyStateTransition(group[0].destinationState);
            EditorUtility.CopySerialized(group[0], target);
            ActiveStateMachine.RemoveAnyStateTransition(group[0]);

            for (int i = 1; i < group.Count; i++)
            {
                group[i].conditions.ForEach<AnimatorCondition>(c =>
                    target.AddCondition(c.mode, c.threshold, c.parameter));
                ActiveStateMachine.RemoveAnyStateTransition(group[i]);
            }

            merged.Add(target);
        }

        /// <summary>
        /// Collapses the selected transitions into one per pair of endpoints, carrying every
        /// condition the collapsed transitions had between them, and selects the results.
        /// </summary>
        /// <remarks>
        /// Grouping happens in full before anything is rewritten: the three grouping passes below
        /// consume their working sets, and only then are the groups handed to the merge helpers.
        /// Doing it in one pass would mean grouping over a collection the rewrite is mutating.
        /// </remarks>
        private static void MergeTransitions()
        {
            List<AnimatorTransitionBase> merged = new List<AnimatorTransitionBase>();

            (AnimatorState, List<AnimatorStateTransition>)[] bySource =
                GroupStateTransitionsBySource(selectedStateTransitions);
            List<AnimatorStateTransition> anyStateTransitions = SelectedAnyStateTransitions();
            List<AnimatorTransitionBase> entryTransitions = SelectedEntryTransitions();

            (AnimatorState, List<List<AnimatorStateTransition>>)[] byEndpoints =
                new (AnimatorState, List<List<AnimatorStateTransition>>)[bySource.Length];
            List<List<AnimatorStateTransition>> anyStateByDestination = new List<List<AnimatorStateTransition>>();
            List<List<AnimatorTransitionBase>> entryByDestination = new List<List<AnimatorTransitionBase>>();

            for (int i = 0; i < byEndpoints.Length; i++)
            {
                AnimatorState source = bySource[i].Item1;
                List<AnimatorStateTransition> remaining = bySource[i].Item2;
                List<List<AnimatorStateTransition>> groups = new List<List<AnimatorStateTransition>>();

                while (remaining.Count > 0)
                {
                    List<AnimatorStateTransition> group = remaining
                        .Where(t => t.destinationState == remaining[0].destinationState).ToList();
                    remaining = remaining.Except(group).ToList();
                    groups.Add(group);
                }

                byEndpoints[i] = (source, groups);
            }

            while (anyStateTransitions.Count > 0)
            {
                List<AnimatorStateTransition> group = anyStateTransitions
                    .Where(t => t.destinationState == anyStateTransitions[0].destinationState).ToList();
                anyStateTransitions = anyStateTransitions.Except(group).ToList();
                anyStateByDestination.Add(group);
            }

            while (entryTransitions.Count > 0)
            {
                List<AnimatorTransitionBase> group = entryTransitions
                    .Where(t => t.destinationState == entryTransitions[0].destinationState).ToList();
                entryTransitions = entryTransitions.Except(group).ToList();
                entryByDestination.Add(group);
            }

            for (int i = 0; i < byEndpoints.Length; i++)
            {
                for (int j = 0; j < byEndpoints[i].Item2.Count; j++)
                {
                    MergeStateTransitions(byEndpoints[i].Item2[j], byEndpoints[i].Item1, merged);
                }
            }

            for (int i = 0; i < anyStateByDestination.Count; i++)
            {
                MergeAnyStateTransitions(anyStateByDestination[i], merged);
            }

            for (int i = 0; i < entryByDestination.Count; i++)
            {
                MergeEntryTransitions(entryByDestination[i], merged);
            }

            Selection.objects = Selection.objects.Concat(merged).ToArray();
            EditorUtility.SetDirty(ActiveController);
        }

        /// <summary>
        /// Replaces one entry transition with one copy per condition it carries, each keeping a
        /// single condition.
        /// </summary>
        /// <param name="transition">The transition to split. Left alone if it has no conditions.</param>
        /// <param name="created">Collects what the operation ends up selecting.</param>
        private static void SplitEntryTransition(AnimatorTransitionBase transition,
            List<AnimatorTransitionBase> created)
        {
            // Nothing to split a conditionless transition along, and removing it would lose it.
            if (transition.conditions.Length == 0)
            {
                return;
            }

            transition.conditions.ForEach<AnimatorCondition>(c =>
            {
                AnimatorTransitionBase copy = ActiveStateMachine.AddEntryTransition(transition.destinationState);
                EditorUtility.CopySerializedManagedFieldsOnly(transition, copy);
                copy.conditions = new AnimatorCondition[1] { c };
                created.Add(copy);
            });

            ActiveStateMachine.RemoveEntryTransition((AnimatorTransition)transition);
        }

        /// <summary>The Any State form of <see cref="SplitEntryTransition"/>.</summary>
        private static void SplitAnyStateTransition(AnimatorStateTransition transition,
            List<AnimatorTransitionBase> created)
        {
            if (transition.conditions.Length == 0)
            {
                return;
            }

            transition.conditions.ForEach<AnimatorCondition>(c =>
            {
                AnimatorTransitionBase copy = ActiveStateMachine.AddAnyStateTransition(transition.destinationState);
                EditorUtility.CopySerialized(transition, copy);
                copy.conditions = new AnimatorCondition[1] { c };
                created.Add(copy);
            });

            ActiveStateMachine.RemoveAnyStateTransition(transition);
        }

        /// <summary>The state-transition form of <see cref="SplitEntryTransition"/>.</summary>
        /// <param name="source">The state the transition leaves, which the copies are added to.</param>
        private static void SplitStateTransition(AnimatorStateTransition transition, AnimatorState source,
            List<AnimatorTransitionBase> created)
        {
            if (transition.conditions.Length == 0)
            {
                return;
            }

            transition.conditions.ForEach<AnimatorCondition>(c =>
            {
                AnimatorStateTransition copy = DuplicateAsStateTransition(transition);
                copy.conditions = new AnimatorCondition[1] { c };
                source.AddTransition(copy);
                created.Add(copy);
            });

            source.RemoveTransition(transition);
        }

        /// <summary>
        /// Splits every selected transition into one transition per condition, and selects the
        /// results.
        /// </summary>
        /// <remarks>
        /// With a transition focused, only that one is split, and the kind test in front of it is
        /// what decides which of the three helpers applies: an entry transition of the current
        /// machine, an Any State transition of it, or -- failing both -- a state transition, whose
        /// source state is found by searching the machine's states for the one that owns it.
        /// </remarks>
        private static void SplitTransitions()
        {
            List<AnimatorTransitionBase> created = new List<AnimatorTransitionBase>();

            if (HasFocusedTransition)
            {
                int kind = 0;
                if (ActiveStateMachine.entryTransitions.Contains(focusedTransition.transition))
                {
                    kind = 1;
                }
                else if (ActiveStateMachine.anyStateTransitions.Contains(focusedTransition.transition))
                {
                    kind = 2;
                }

                AnimatorState source = null;
                if (kind == 0)
                {
                    source = ActiveStateMachine.states
                        .First(c => c.state.transitions.Contains(focusedTransition.transition)).state;
                }

                switch (kind)
                {
                    case 2:
                        SplitAnyStateTransition((AnimatorStateTransition)focusedTransition.transition, created);
                        break;
                    case 1:
                        SplitEntryTransition(focusedTransition.transition, created);
                        break;
                    case 0:
                        SplitStateTransition((AnimatorStateTransition)focusedTransition.transition, source, created);
                        break;
                }
            }
            else
            {
                (AnimatorState, List<AnimatorStateTransition>)[] bySource =
                    GroupStateTransitionsBySource(selectedStateTransitions);
                List<AnimatorStateTransition> anyStateTransitions =
                    SelectedAnyStateTransitions(selectedStateTransitions);
                List<AnimatorTransitionBase> entryTransitions = SelectedEntryTransitions(selectedTransitions);

                if (selectedTransitionEdits.Count > 0)
                {
                    for (int i = 0; i < bySource.Length; i++)
                    {
                        AnimatorState source = bySource[i].Item1;
                        List<AnimatorStateTransition> owned = bySource[i].Item2;

                        for (int j = 0; j < owned.Count; j++)
                        {
                            if (selectedStateTransitions.Contains(owned[j]))
                            {
                                SplitStateTransition(owned[j], source, created);
                            }
                        }
                    }

                    for (int i = 0; i < anyStateTransitions.Count; i++)
                    {
                        SplitAnyStateTransition(anyStateTransitions[i], created);
                    }

                    for (int i = 0; i < entryTransitions.Count; i++)
                    {
                        SplitEntryTransition(entryTransitions[i], created);
                    }
                }
            }

            Selection.objects = Selection.objects.Concat(created).ToArray();
            EditorUtility.SetDirty(ActiveController);
        }

        #endregion
    }
}
