// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static AssetPredicate    -> ForEachState,      line 3919
//   static ResolvePredicate  -> ForEachTransition, line 3734
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// These two are the leaves of a small family of walkers the decompiled file spreads over lines
// 3680-3940 -- PushPredicate (3696, runs all three walkers in one pass), ViewPredicate (3712, the
// layers of a controller), CollectPredicate (3721, nested state machines), PreparePredicate (3910,
// ForEachState over a whole controller), UpdatePredicate (3940, a short-circuiting ForEachState).
// Only the two below were needed to unblock the members this pass was clearing; each of the others
// is a one-liner over them and they were recorded here so the family was not lost. All five have
// since landed in EditorUtils.ControllerTraversal.cs, which owns the controller-level helpers.
//
// The obfuscator's names carry no information: "AssetPredicate" walks states and "ResolvePredicate"
// walks transitions. Neither is a predicate and neither touches assets.
// Audit status: VERIFIED -- both bodies diffed statement by statement against export/, including
// ForEachTransition's four transition cases in their shipped order (entry, any-state, state,
// machine), the machine-transition pair being read and written through the parent, the `if
// (!stateMachine)` Unity truth test, and the immediate-self-reference guard on both recursions.

using System;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Runs <paramref name="action"/> over every state in <paramref name="stateMachine"/>, and by
        /// default over the states of its nested machines too.
        /// </summary>
        /// <param name="recurse">
        /// False restricts the walk to this machine's own states, for callers that are already
        /// iterating the nesting themselves and would otherwise visit each state twice.
        /// </param>
        /// <remarks>
        /// A child machine that lists itself among its own children is skipped rather than recursed
        /// into. That guard only covers the immediate self-reference; a longer cycle -- A containing
        /// B containing A -- still recurses until the stack runs out. Unity's own editor does not let
        /// you build one, so the cheap check is the whole of the protection, and it is preserved as
        /// shipped.
        /// </remarks>
        internal static void ForEachState(this AnimatorStateMachine stateMachine, Action<AnimatorState> action,
            bool recurse = true)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                action(child.state);
            }

            if (!recurse)
            {
                return;
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                if (child.stateMachine != stateMachine)
                {
                    child.stateMachine.ForEachState(action);
                }
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/> over every transition reachable from
        /// <paramref name="stateMachine"/>, each wrapped in the
        /// <see cref="AnimatorStateTransitionSet"/> that records where it came from.
        /// </summary>
        /// <param name="recurse">
        /// False visits only this machine's own transitions. Note that the transitions *into* nested
        /// machines are visited either way -- those are owned by this machine, not by the child --
        /// so this flag controls the descent, not the machine-transition case.
        /// </param>
        /// <remarks>
        /// <para>
        /// The wrapper is what makes the callback useful: Unity models the four transition cases with
        /// two unrelated classes and no back-pointer to the owner, so a caller handed a bare
        /// <see cref="AnimatorTransitionBase"/> could inspect it but not delete or reassign it.
        /// <see cref="AnimatorStateTransitionSet"/> carries the source alongside it. The four cases
        /// are visited in a fixed order -- entry, any-state, state, machine -- which callers that
        /// build an ordered list depend on.
        /// </para>
        /// <para>
        /// The null guard at the top is Unity's overloaded truth test on the object, so it catches a
        /// destroyed machine as well as a null reference. It is not repeated on the recursion, which
        /// re-enters through this same method and so is covered.
        /// </para>
        /// <para>
        /// As in <see cref="ForEachState"/>, only an immediate self-reference is guarded against when
        /// recursing.
        /// </para>
        /// </remarks>
        internal static void ForEachTransition(this AnimatorStateMachine stateMachine,
            Action<AnimatorStateTransitionSet> action, bool recurse = true)
        {
            if (!stateMachine)
            {
                return;
            }

            foreach (AnimatorTransition transition in stateMachine.entryTransitions)
            {
                action(new AnimatorStateTransitionSet(transition,
                    AnimatorStateTransitionSet.TransitionSourceType.EntryTransition, stateMachine));
            }

            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                action(new AnimatorStateTransitionSet(transition,
                    AnimatorStateTransitionSet.TransitionSourceType.AnyTransition, stateMachine));
            }

            foreach (ChildAnimatorState child in stateMachine.states)
            {
                foreach (AnimatorStateTransition transition in child.state.transitions)
                {
                    action(new AnimatorStateTransitionSet(transition,
                        AnimatorStateTransitionSet.TransitionSourceType.StateTransition, child.state));
                }
            }

            // A transition into a nested machine belongs to the parent, so it is read off this
            // machine even though the set records the child as its source.
            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                foreach (AnimatorTransition transition in stateMachine.GetStateMachineTransitions(child.stateMachine))
                {
                    action(new AnimatorStateTransitionSet(transition,
                        AnimatorStateTransitionSet.TransitionSourceType.MachineTransition, child.stateMachine,
                        stateMachine));
                }
            }

            if (!recurse)
            {
                return;
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                if (child.stateMachine != stateMachine)
                {
                    child.stateMachine.ForEachTransition(action);
                }
            }
        }
    }
}
