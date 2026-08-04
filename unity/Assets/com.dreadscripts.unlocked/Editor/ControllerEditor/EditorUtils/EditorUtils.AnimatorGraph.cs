// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ClonePredicate    -> IsExitOrDangling,                      line 3602
//   static CreatePredicate   -> ConditionSetsMatch,                    line 3661
//   static NewPredicate      -> Traverse(AnimatorController, ...),     line 3687
//   static PushPredicate     -> Traverse(AnimatorStateMachine, ...),   line 3696
//   static SortPredicate     -> GetWriteDefaultsMode,                  line 3973
//   static RegisterPredicate -> MapTransitionTargets,                  line 4023
//   class <>c__DisplayClass164_0/_1 -> dissolved into MapTransitionTargets' lambdas, lines 1920/1954
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Walkers over an animator's graph, plus the few graph edits that do not fit anywhere else.
//
// Every recursive walker here guards against a state machine listed as its own child
// (`cm.stateMachine != parent`), which Unity's data model permits and which would otherwise
// recurse forever. ForEachStateMachine is the one exception -- it visits the node first and does
// not make that check -- so it will still hang on a self-parented machine. That is the vendor's
// code and it is left as-is; the guard was added by whoever wrote the other four, not by us.
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
        /// Walks every layer of the controller, offering each state machine, state and transition
        /// to the matching callback. Any callback may be null.
        /// </summary>
        internal static void Traverse(this AnimatorController controller, Action<AnimatorStateMachine> onStateMachine,
            Action<AnimatorState> onState, Action<AnimatorStateTransitionSet> onTransition)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].stateMachine.Traverse(onStateMachine, onState, onTransition);
            }
        }

        /// <summary>
        /// Walks the state machine, offering each state machine, state and transition to the
        /// matching callback. Any callback may be null.
        /// </summary>
        /// <param name="recurseStateMachines">
        /// Only controls the state-machine callback. States and transitions are always visited
        /// recursively -- the vendor did not thread the flag through to them.
        /// </param>
        internal static void Traverse(this AnimatorStateMachine stateMachine,
            Action<AnimatorStateMachine> onStateMachine, Action<AnimatorState> onState,
            Action<AnimatorStateTransitionSet> onTransition, bool recurseStateMachines = true)
        {
            if (onStateMachine != null)
            {
                stateMachine.ForEachStateMachine(onStateMachine, recurseStateMachines);
            }

            if (onState != null)
            {
                stateMachine.ForEachState(onState);
            }

            if (onTransition != null)
            {
                stateMachine.ForEachTransition(onTransition);
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
