// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ClonePredicate    -> IsExitOrDangling,                      line 3602
//   static CreatePredicate   -> ConditionSetsMatch,                    line 3661
//   static NewPredicate      -> Traverse(AnimatorController, ...),     line 3687
//   static PushPredicate     -> ForEachGraphElement, line 3696, in EditorUtils.ControllerTraversal.cs
//   static SortPredicate     -> GetWriteDefaultsMode,                  line 3973
//   static RegisterPredicate -> MapTransitionTargets,                  line 4023
//   class <>c__DisplayClass164_0 -> dissolved into MapTransitionTargets' lambdas, line 1920
//   class <>c__DisplayClass164_1 -> dissolved into MapTransitionTargets' lambdas, line 1954
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: PARTIAL -- all six members declared below were re-checked statement by statement
// against reverse-engineering/export/ (EditorUtils.cs lines 3602, 3661, 3687, 3696, 3973 and 4023, each of which
// still lands on the member named above), and each is a faithful transcription; the header claims
// no member the file does not declare. PARTIAL rather than VERIFIED because of the duplicate port
// recorded under NOTES, which cannot be settled from inside this file.
//
// NOTES
// Decompiled PushPredicate (line 3696) was ported twice by the parallel ports that were merged:
// once here as a Traverse overload on AnimatorStateMachine, and once as ForEachGraphElement in
// EditorUtils.ControllerTraversal.cs, with identical bodies. The two names made it invisible to the
// C# compiler. ForEachGraphElement is the copy kept; the overload here has been removed and the
// Traverse(AnimatorController) entry point below now calls it. Only NewPredicate (3687), which has
// no counterpart anywhere, is still ported in this file under the Traverse name.
//
// Walkers over an animator's graph, plus the few graph edits that do not fit anywhere else.
//
// The recursive walkers these members lean on now live elsewhere: ForEachStateMachine in
// EditorUtils.ControllerTraversal.cs, ForEachState and ForEachTransition in
// EditorUtils.StateMachineTraversal.cs. The merge that reconciled the parallel ports left only
// their callers here, so the note this header used to carry about the self-parented state machine
// guard (and about ForEachStateMachine not making that check) belongs to those files, where the
// walkers are.
//
// Grouped here rather than with the layer helpers because these operate on the graph inside a
// layer (state machines, states, transitions, behaviours) and none of them touches
// AnimatorControllerLayer at all except to reach its stateMachine.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Whether the transition has no state or state machine to re-point at -- either because it
        /// exits the layer, or because its destination has gone missing.
        /// </summary>
        /// <remarks>
        /// Callers use this to skip transitions that cannot be duplicated onto a new source: a copy
        /// of an exit or dangling transition would have nothing to connect to.
        /// </remarks>
        internal static bool IsExitOrDangling(this AnimatorTransitionBase transition)
        {
            if (transition.isExit)
            {
                return true;
            }

            return !transition.destinationState && !transition.destinationStateMachine;
        }

        /// <summary>
        /// Whether the two condition sets hold the same conditions, ignoring order but counting
        /// duplicates -- i.e. equality as multisets.
        /// </summary>
        /// <remarks>
        /// Order is ignored because Unity's transition inspector lets conditions be reordered
        /// freely and the result is the same transition; duplicates are counted because a
        /// condition listed twice is genuinely evaluated twice.
        /// </remarks>
        internal static bool ConditionSetsMatch(IEnumerable<AnimatorCondition> first,
            IEnumerable<AnimatorCondition> second)
        {
            Dictionary<AnimatorCondition, int> counts = new Dictionary<AnimatorCondition, int>();
            foreach (AnimatorCondition condition in first)
            {
                if (!counts.ContainsKey(condition))
                {
                    counts.Add(condition, 1);
                }
                else
                {
                    counts[condition]++;
                }
            }

            foreach (AnimatorCondition condition in second)
            {
                if (!counts.ContainsKey(condition))
                {
                    return false;
                }

                counts[condition]--;
            }

            return counts.Values.All(c => c == 0);
        }

        /// <summary>
        /// Runs the three callbacks over every state machine, state and transition in every layer of
        /// <paramref name="controller"/>.
        /// </summary>
        /// <remarks>
        /// The whole-controller entry point to
        /// <see cref="ForEachGraphElement(AnimatorStateMachine, Action{AnimatorStateMachine}, Action{AnimatorState}, Action{AnimatorStateTransitionSet}, bool)"/>,
        /// which it calls once per layer root. Any callback may be null; the walker skips that half.
        /// </remarks>
        internal static void Traverse(this AnimatorController controller, Action<AnimatorStateMachine> onStateMachine,
            Action<AnimatorState> onState, Action<AnimatorStateTransitionSet> onTransition)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].stateMachine.ForEachGraphElement(onStateMachine, onState, onTransition);
            }
        }

        /// <summary>
        /// Whether the controller's states agree about Write Defaults: 1 if every state has it on,
        /// 0 if every state has it off, and 2 if they disagree.
        /// </summary>
        /// <param name="defaultToOn">
        /// What to answer when there is nothing to look at -- a null controller, or one with no
        /// states at all. Note this returns 1 or 0, never 2: an empty controller is reported as
        /// consistent.
        /// </param>
        /// <remarks>
        /// The three return values line up with <see cref="WriteDefaultSetSettings"/> (Off = 0,
        /// On = 1, Automatic = 2), which is why they are these numbers and not an enum of their
        /// own; "mixed" and "match the controller" are the same answer to the caller.
        /// The walk stops as soon as it finds a state disagreeing with the ones before it.
        /// </remarks>
        internal static int GetWriteDefaultsMode(AnimatorController controller, bool defaultToOn = true)
        {
            if (!controller)
            {
                return defaultToOn ? 1 : 0;
            }

            bool empty = true;
            int mode = 2;

            controller.AnyState(s =>
            {
                empty = false;
                if (s.writeDefaultValues)
                {
                    if (mode == 0)
                    {
                        mode = 2;
                        return true;
                    }

                    if (mode == 2)
                    {
                        mode = 1;
                    }
                }
                else
                {
                    if (mode == 1)
                    {
                        mode = 2;
                        return true;
                    }

                    if (mode == 2)
                    {
                        mode = 0;
                    }
                }

                return false;
            });

            if (empty)
            {
                mode = defaultToOn ? 1 : 0;
            }

            return mode;
        }

        /// <summary>
        /// Indexes every state and state machine reachable from <paramref name="stateMachine"/> by
        /// the transitions that arrive at it, so a caller can ask "what points at this state" --
        /// which the graph itself cannot answer, since a transition only records where it goes.
        /// </summary>
        /// <param name="targets">
        /// An existing map to add to, so several state machines can be indexed together. A new one
        /// is created when null.
        /// </param>
        /// <remarks>
        /// Every state and state machine gets an entry even with no incoming transitions, so a
        /// missing key means "not in this graph" rather than "unreachable".
        /// </remarks>
        internal static Dictionary<UnityEngine.Object, AnimatorTransitionRef> MapTransitionTargets(
            this AnimatorStateMachine stateMachine,
            Dictionary<UnityEngine.Object, AnimatorTransitionRef> targets = null, bool recursive = true)
        {
            if (targets == null)
            {
                targets = new Dictionary<UnityEngine.Object, AnimatorTransitionRef>();
            }

            stateMachine.ForEachStateMachine(machine =>
            {
                if (machine == null)
                {
                    return;
                }

                // Not recurse: ForEachStateMachine is already visiting every machine, so
                // recursing here would record each transition once per ancestor.
                machine.ForEachTransition(transitionSet =>
                {
                    if (!transitionSet.transition)
                    {
                        return;
                    }

                    UnityEngine.Object destination = transitionSet.DestinationState;
                    bool isState = destination;
                    if (!isState)
                    {
                        destination = transitionSet.DestinationStateMachine;
                    }

                    if (!destination)
                    {
                        return;
                    }

                    bool known = targets.ContainsKey(destination);
                    AnimatorTransitionRef reference = known
                        ? targets[destination]
                        : (isState
                            ? new AnimatorTransitionRef(transitionSet.DestinationState)
                            : new AnimatorTransitionRef(machine, transitionSet.DestinationStateMachine));

                    if (!known)
                    {
                        targets.Add(destination, reference);
                    }

                    reference.incomingTransitions.Add(transitionSet.transition);
                }, recurse: false);

                foreach (AnimatorState state in machine.states.Select(cs => cs.state))
                {
                    if (!targets.ContainsKey(state))
                    {
                        targets.Add(state, new AnimatorTransitionRef(state));
                    }
                }

                foreach (AnimatorStateMachine child in machine.stateMachines.Select(cm => cm.stateMachine))
                {
                    if (!targets.ContainsKey(child))
                    {
                        targets.Add(child, new AnimatorTransitionRef(machine, child));
                    }
                }
            }, recursive);

            return targets;
        }
    }
}
