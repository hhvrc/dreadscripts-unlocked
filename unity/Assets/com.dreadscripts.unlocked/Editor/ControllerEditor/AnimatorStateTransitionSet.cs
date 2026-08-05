// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorStateTransitionSet.cs
//   GetSourceName                                -> SourceName,                 line 31
//   GetDestinationName                           -> DestinationName,            line 55
//   GetDisplayName                               -> DisplayName,                line 73
//   GetIsExit / SetIsExit                        -> IsExit,                     line 83
//   GetDestinationState / SetDestinationState    -> DestinationState,           line 95
//   GetHasDestinationState                       -> HasDestinationState,        line 107
//   GetDestinationStateMachine / Set...          -> DestinationStateMachine,    line 113
//   GetHasDestinationStateMachine                -> HasDestinationStateMachine, line 125
//   GetConditions / SetConditions                -> Conditions,                 line 131
//   .ctor(base, type, stateMachine)              -> .ctor(..., parentStateMachine = null), line 152
//   .ctor(base, type, stateMachine, parent)      -> same, line 162
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// The Get*/Set* pairs above carry [SpecialName] in the decompiled source, i.e. they are the
// accessors of properties that ILSpy could not recombine; they are restored as properties here.
// The two state-machine constructors differ only in whether a parent is supplied, so they are
// collapsed into one with an optional parameter.
//
// NOTES
// The MAP above is not exhaustive. The nested TransitionSourceType enum, the AnimatorState
// constructor, Remove, the three implicit operators and the SetupSystem/ExcludeSystem pair are all
// ported and were audited, but carry no MAP line.
//
// Unlike its siblings in this folder, this type keeps the protector's SetupSystem/ExcludeSystem
// scaffolding rather than dropping it; see the doc comments on those two members.
//
// Audit status: VERIFIED -- all six fields, all three decompiled constructors (against the two
// ported ones), all nine accessors, Remove and the three implicit operators were diffed statement by
// statement against export/. The switch arms in SourceName and Remove were checked case by case,
// including which owner each removal path goes through and the "!AnyState" and "Exit" literals. The
// MAP line numbers were re-checked against the current export/ snapshot and all eleven still land on
// the member they name.

using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One transition together with the context needed to name it, retarget it and delete it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity models transitions as four unrelated cases — a state's own outgoing transition, a child
    /// state machine's transition, an Entry transition and an Any State transition — and each is
    /// removed through a different method on a different owner. An <see cref="AnimatorTransitionBase"/>
    /// on its own carries no record of which case it came from, so a caller holding one cannot delete
    /// it. This type keeps that provenance alongside the transition so the four cases can be passed
    /// around and acted on uniformly; see <see cref="Remove"/>.
    /// </para>
    /// <para>
    /// This is the counterpart of <see cref="AnimatorTransitionRef"/>, which describes the
    /// <em>source</em> a set of transitions leaves from; this type describes a single transition
    /// already resolved out of such a source.
    /// </para>
    /// </remarks>
    internal readonly struct AnimatorStateTransitionSet
    {
        /// <summary>Which of Unity's four transition cases <see cref="transition"/> came from.</summary>
        internal enum TransitionSourceType
        {
            StateTransition,
            MachineTransition,
            EntryTransition,
            AnyTransition
        }

        internal readonly AnimatorTransitionBase transition;

        /// <summary>
        /// <see cref="transition"/> as an <see cref="AnimatorStateTransition"/>, or null for Entry and
        /// state-machine transitions, which are plain <see cref="AnimatorTransition"/>s and therefore
        /// have no duration, exit time or interruption settings to edit.
        /// </summary>
        internal readonly AnimatorStateTransition stateTransition;

        internal readonly TransitionSourceType sourceType;

        /// <summary>The state the transition leaves from; null unless <see cref="sourceType"/> is <see cref="TransitionSourceType.StateTransition"/>.</summary>
        internal readonly AnimatorState sourceState;

        /// <summary>
        /// The state machine the transition leaves from, or the one that owns it for the Entry and
        /// Any State cases.
        /// </summary>
        internal readonly AnimatorStateMachine sourceStateMachine;

        /// <summary>
        /// The state machine that owns <see cref="sourceStateMachine"/>'s transitions. Only set for
        /// <see cref="TransitionSourceType.MachineTransition"/>, where removal has to go through the
        /// parent rather than the child.
        /// </summary>
        internal readonly AnimatorStateMachine parentStateMachine;

        /// <summary>
        /// Never assigned, so <see cref="ExcludeSystem"/> is always true.
        /// </summary>
        /// <remarks>
        /// This is a remnant of the shipped build's licensing gate. It is kept, along with
        /// <see cref="ExcludeSystem"/>, because the decompiled source has it and callers may still
        /// consult it; nothing in the restored package writes to it.
        /// </remarks>
        private static object SetupSystem = null;

        internal AnimatorStateTransitionSet(AnimatorTransitionBase transition, TransitionSourceType sourceType, AnimatorState sourceState)
        {
            this.transition = transition;
            stateTransition = transition as AnimatorStateTransition;
            this.sourceType = sourceType;
            this.sourceState = sourceState;
            sourceStateMachine = null;
            parentStateMachine = null;
        }

        internal AnimatorStateTransitionSet(AnimatorTransitionBase transition, TransitionSourceType sourceType, AnimatorStateMachine sourceStateMachine, AnimatorStateMachine parentStateMachine = null)
        {
            this.transition = transition;
            stateTransition = transition as AnimatorStateTransition;
            this.sourceType = sourceType;
            sourceState = null;
            this.sourceStateMachine = sourceStateMachine;
            this.parentStateMachine = parentStateMachine;
        }

        /// <summary>
        /// What the transition leaves from, as shown in the graph.
        /// </summary>
        /// <remarks>
        /// "!AnyState" marks a transition whose recorded source has gone away — a broken entry the
        /// user needs to see rather than a blank label.
        /// </remarks>
        internal string SourceName
        {
            get
            {
                switch (sourceType)
                {
                    case TransitionSourceType.AnyTransition:
                        return "AnyState";

                    case TransitionSourceType.MachineTransition:
                        if (sourceStateMachine != null)
                        {
                            return sourceStateMachine.name;
                        }

                        return "!AnyState";

                    case TransitionSourceType.EntryTransition:
                        return "Entry";

                    default:
                        if (sourceState != null)
                        {
                            return sourceState.name;
                        }

                        return "!AnyState";
                }
            }
        }

        /// <summary>
        /// What the transition leads to, as shown in the graph. A transition with no destination at
        /// all reads as "Exit", which is what Unity treats it as.
        /// </summary>
        internal string DestinationName
        {
            get
            {
                if (IsExit)
                {
                    return "Exit";
                }

                if (HasDestinationStateMachine)
                {
                    return DestinationStateMachine.name;
                }

                if (HasDestinationState)
                {
                    return DestinationState.name;
                }

                return "Exit";
            }
        }

        /// <summary>
        /// The transition's own name when it has one, otherwise a "Source -&gt; Destination" label.
        /// </summary>
        /// <remarks>
        /// Transitions are usually unnamed in the animator, so the derived label is the common case;
        /// an explicit name wins because it was set deliberately.
        /// </remarks>
        internal string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(transition.name))
                {
                    return transition.name;
                }

                return SourceName + " -> " + DestinationName;
            }
        }

        internal bool IsExit
        {
            get { return transition.isExit; }
            set { transition.isExit = value; }
        }

        internal AnimatorState DestinationState
        {
            get { return transition.destinationState; }
            set { transition.destinationState = value; }
        }

        internal bool HasDestinationState
        {
            get { return DestinationState != null; }
        }

        internal AnimatorStateMachine DestinationStateMachine
        {
            get { return transition.destinationStateMachine; }
            set { transition.destinationStateMachine = value; }
        }

        internal bool HasDestinationStateMachine
        {
            get { return DestinationStateMachine != null; }
        }

        internal AnimatorCondition[] Conditions
        {
            get { return transition.conditions; }
            set { transition.conditions = value; }
        }

        /// <summary>
        /// Deletes the transition from whichever owner holds it.
        /// </summary>
        /// <remarks>
        /// Each case has to go through its own owner: Unity leaks the sub-asset if the transition is
        /// destroyed directly instead. A missing owner is skipped silently rather than throwing,
        /// because this runs over collected transitions that may have been invalidated by an edit
        /// earlier in the same batch.
        /// </remarks>
        internal void Remove()
        {
            switch (sourceType)
            {
                case TransitionSourceType.AnyTransition:
                    if (sourceStateMachine != null)
                    {
                        sourceStateMachine.RemoveAnyStateTransition(stateTransition);
                    }

                    break;

                case TransitionSourceType.EntryTransition:
                    if (sourceStateMachine != null)
                    {
                        sourceStateMachine.RemoveEntryTransition((AnimatorTransition)transition);
                    }

                    break;

                case TransitionSourceType.StateTransition:
                    if (sourceState != null)
                    {
                        sourceState.RemoveTransition(stateTransition);
                    }

                    break;

                case TransitionSourceType.MachineTransition:
                    if (parentStateMachine != null && sourceStateMachine != null)
                    {
                        parentStateMachine.RemoveStateMachineTransition(sourceStateMachine, (AnimatorTransition)transition);
                    }

                    break;
            }
        }

        /// <summary>Null for Entry and state-machine transitions, which are not state transitions.</summary>
        public static implicit operator AnimatorStateTransition(AnimatorStateTransitionSet set)
        {
            return set.stateTransition;
        }

        /// <summary>Throws for a state transition, which is not an <see cref="AnimatorTransition"/>.</summary>
        public static implicit operator AnimatorTransition(AnimatorStateTransitionSet set)
        {
            return (AnimatorTransition)set.transition;
        }

        public static implicit operator AnimatorTransitionBase(AnimatorStateTransitionSet set)
        {
            return set.transition;
        }

        /// <summary>
        /// Always true in this build; see <see cref="SetupSystem"/>.
        /// </summary>
        internal static bool ExcludeSystem()
        {
            return SetupSystem == null;
        }
    }
}
