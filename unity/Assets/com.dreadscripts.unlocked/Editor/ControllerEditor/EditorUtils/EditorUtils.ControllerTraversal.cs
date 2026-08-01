// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ViewPredicate    -> ForEachRootStateMachine,                line 3712
//   static CollectPredicate -> ForEachStateMachine,                    line 3721
//   static PushPredicate    -> ForEachGraphElement,                    line 3696
//   static PreparePredicate -> ForEachState(AnimatorController, ...),  line 3910
//   static UpdatePredicate  -> AnyState(AnimatorController, ...),      line 3941
//   static ChangePredicate  -> AnyState(AnimatorStateMachine, ...),    line 3960
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EditorUtils.StateMachineTraversal.cs ported the two leaf walkers (ForEachState and
// ForEachTransition over one state machine) and recorded the rest of the family as left for its
// proper owner. This file is that owner: everything here either lifts a walk from one state machine
// to a whole controller, or composes the leaf walkers.
//
// ChangePredicate is not in this pass's assigned set but is ported here because UpdatePredicate is
// nothing but a loop that calls it, and it was not ported anywhere else. It is the short-circuiting
// counterpart of ForEachState, so the two are named as a pair: ForEachState / AnyState.
//
// Not ported: NewPredicate (line 3687), the controller-level ForEachGraphElement. It is a two-line
// loop over ForEachGraphElement, it is not in the assigned set, and its single decompiled caller is
// in the unported god class -- adding it now would be adding a member with no caller.
//
// UpdatePredicate's decompiled body is a `while (true)` with break/continue, produced by control-
// flow flattening rather than written that way; it is restored below as the loop it started as. The
// ADOverhaul builds carry no counterpart to cross-check against, but the flattened form has exactly
// one exit per branch and the translation is unambiguous.

using System;
using System.Linq;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Runs <paramref name="action"/> once per layer, on that layer's root state machine only.
        /// </summary>
        /// <remarks>
        /// Deliberately shallow: this is for callers that want to do something to each layer as a
        /// unit. Pair it with <see cref="ForEachStateMachine"/> to reach nested machines as well,
        /// which is how the shipped call sites use it.
        /// </remarks>
        internal static void ForEachRootStateMachine(this AnimatorController controller,
            Action<AnimatorStateMachine> action)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                action(layer.stateMachine);
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/> on <paramref name="stateMachine"/> and, by default, on every
        /// machine nested inside it.
        /// </summary>
        /// <param name="recurse">False visits only the machine passed in.</param>
        /// <remarks>
        /// The machine itself is visited before its children, so a caller that reparents or renames
        /// during the walk sees the parent in its original state.
        /// <para>
        /// Unlike <see cref="ForEachState(AnimatorStateMachine, Action{AnimatorState}, bool)"/>,
        /// this walk has no guard against a machine listed among
        /// its own children: such a graph recurses until the stack runs out. Unity's editor does not
        /// let one be built, and the omission is preserved as shipped rather than papered over.
        /// </para>
        /// </remarks>
        internal static void ForEachStateMachine(this AnimatorStateMachine stateMachine,
            Action<AnimatorStateMachine> action, bool recurse = true)
        {
            action(stateMachine);

            if (!recurse)
            {
                return;
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                child.stateMachine.ForEachStateMachine(action);
            }
        }

        /// <summary>
        /// Walks a layer once and hands out every state machine, every state and every transition it
        /// contains, to whichever of the three callbacks the caller supplied.
        /// </summary>
        /// <param name="recurseStateMachines">
        /// Applies to <paramref name="onStateMachine"/> only. The state and transition walks always
        /// recurse; this parameter cannot restrict them, which is a wrinkle of the shipped signature
        /// rather than a considered design.
        /// </param>
        /// <remarks>
        /// A null callback is skipped rather than invoked, so passing two nulls costs only the one
        /// walk that is actually wanted. The three walks are independent passes over the graph, not
        /// one interleaved pass: every state machine is visited before the first state is, and every
        /// state before the first transition. Callers whose state callback depends on the state
        /// machine callback having run are therefore safe; the reverse is not.
        /// </remarks>
        internal static void ForEachGraphElement(this AnimatorStateMachine stateMachine,
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
        /// Runs <paramref name="action"/> on every state of every layer, nested machines included.
        /// </summary>
        internal static void ForEachState(this AnimatorController controller, Action<AnimatorState> action)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                layer.stateMachine.ForEachState(action);
            }
        }

        /// <summary>
        /// Reports whether any state anywhere in <paramref name="controller"/> satisfies
        /// <paramref name="predicate"/>.
        /// </summary>
        /// <remarks>
        /// Short-circuits on the first match, layer by layer in order, which matters because the
        /// predicate is also used for its side effects at some call sites -- it doubles as a "stop
        /// here" signal for a scan that accumulates as it goes.
        /// </remarks>
        internal static bool AnyState(this AnimatorController controller, Func<AnimatorState, bool> predicate)
        {
            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                if (layer.stateMachine.AnyState(predicate))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reports whether any state in <paramref name="stateMachine"/> satisfies
        /// <paramref name="predicate"/>, descending into nested machines unless told not to.
        /// </summary>
        /// <remarks>
        /// The machine's own states are tested before any child's, and a child machine that lists
        /// itself among its children is skipped -- the same shallow cycle guard
        /// <see cref="ForEachState(AnimatorStateMachine, Action{AnimatorState}, bool)"/> uses, with
        /// the same limits.
        /// </remarks>
        internal static bool AnyState(this AnimatorStateMachine stateMachine,
            Func<AnimatorState, bool> predicate, bool recurse = true)
        {
            if (stateMachine.states.Any(child => predicate(child.state)))
            {
                return true;
            }

            if (!recurse)
            {
                return false;
            }

            return stateMachine.stateMachines
                .Where(child => child.stateMachine != stateMachine)
                .Any(child => child.stateMachine.AnyState(predicate));
        }
    }
}
