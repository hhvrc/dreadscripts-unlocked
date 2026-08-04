// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested readonly struct TransitionEditionInfo -> TransitionEditionInfo, line 664
//     m_DisplayNameRef ... isDefaultTransitionRef -> unchanged,   lines 666-680
//     context, edge, transition, stateTransition, animatorTransition, sourceState,
//     sourceStateMachine, ownerStateMachine, destinationState, destinationStateMachine,
//     destinationType, sourceType, isAnyStateTransition, isDefaultTransition,
//     isExplicitEntryTransition, isExitTransition -> unchanged,   lines 682-712
//     DisplayName()  -> DisplayName,   line 717
//     FullName()     -> FullName,      line 723
//     constructor    -> TransitionEditionInfo(object, GraphEdgeRef), line 728
//     Remove()       -> Remove,        line 749
//     LogoutStruct   -> NOT PORTED, line 714 -- obfuscator scaffolding: a never-assigned static
//     FindStruct()   -> NOT PORTED, line 774 -- the null-check on it; nothing calls it, no behaviour
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// The [SpecialName] methods in the decompiled source are property accessors the deobfuscation pass
// left as methods; they are restored to properties here.
//
// SHIPPED BUG
// The constructor reproduces the vendor copy-paste bug where isDefaultTransition reads the
// isAnyStateTransition ref (decompiled line 733, RE_NOTES 'Vendor bugs'). Preserved faithfully,
// not corrected.
//
// Audit status: VERIFIED -- compared member by member against decompiled/ControllerEditor/
// DreadScripts/ControllerEditor/AnimatorGraphReflection.cs lines 664-778 on 2026-08-05; every line
// number above lands on the member named, including the two unported ones.

using System.Reflection;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// One transition as the Animator graph sees it: the transition asset itself, both of its
        /// endpoints resolved to whatever kind of node they are, and the arrow it is drawn on.
        /// </summary>
        /// <remarks>
        /// A transition asset alone does not say where it starts — an <see cref="AnimatorStateTransition"/>
        /// could belong to a state or to Any State, and an <see cref="AnimatorTransition"/> to a
        /// sub-state-machine or to Entry — and it is the owner, not the transition, that has to remove
        /// it. Unity's graph works that out when it lays the arrows out, so this reads the answer back
        /// out of the graph's own record rather than trying to reconstruct it from the controller.
        /// </remarks>
        internal readonly struct TransitionEditionInfo
        {
            private static readonly ReflectionMemberRef<FieldInfo> m_DisplayNameRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_DisplayName");

            private static readonly ReflectionMemberRef<FieldInfo> m_FullNameRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_FullName");

            private static readonly ReflectionMemberRef<FieldInfo> sourceStateRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceState");

            private static readonly ReflectionMemberRef<FieldInfo> sourceStateMachineRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceStateMachine");

            private static readonly ReflectionMemberRef<FieldInfo> ownerStateMachineRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "ownerStateMachine");

            private static readonly ReflectionMemberRef<FieldInfo> transitionRef =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "transition");

            private static readonly ReflectionMemberRef<PropertyInfo> isAnyStateTransitionRef =
                new ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isAnyStateTransition");

            // Declared but never read: the constructor below reads isAnyStateTransitionRef where it
            // means to read this one. Kept so the reference matches the original.
            private static readonly ReflectionMemberRef<PropertyInfo> isDefaultTransitionRef =
                new ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isDefaultTransition");

            /// <summary>The graph's own <c>TransitionEditionContext</c> this was read from.</summary>
            public readonly object context;

            /// <summary>The arrow this transition is drawn on, shared with any sibling transitions.</summary>
            public readonly GraphEdgeRef edge;

            public readonly AnimatorTransitionBase transition;

            /// <summary>
            /// <see cref="transition"/> as a state transition, when it leaves a state or Any State;
            /// null otherwise.
            /// </summary>
            public readonly AnimatorStateTransition stateTransition;

            /// <summary>
            /// <see cref="transition"/> as a plain transition, when it leaves a sub-state-machine or
            /// Entry; null otherwise.
            /// </summary>
            public readonly AnimatorTransition animatorTransition;

            public readonly AnimatorState sourceState;

            public readonly AnimatorStateMachine sourceStateMachine;

            /// <summary>The state machine holding the transition, which is what can remove it.</summary>
            public readonly AnimatorStateMachine ownerStateMachine;

            public readonly AnimatorState destinationState;

            public readonly AnimatorStateMachine destinationStateMachine;

            public readonly GraphNodeRef.NodeType destinationType;

            public readonly GraphNodeRef.NodeType sourceType;

            public readonly bool isAnyStateTransition;

            public readonly bool isDefaultTransition;

            /// <summary>An Entry transition with a condition, as opposed to the default one.</summary>
            public readonly bool isExplicitEntryTransition;

            public readonly bool isExitTransition;

            public TransitionEditionInfo(object context, GraphEdgeRef edge)
            {
                this.context = context;
                this.edge = edge;

                isAnyStateTransition = (bool)isAnyStateTransitionRef.Member.GetValue(context);

                // Ported literally: the original reads isAnyStateTransitionRef here too, so
                // isDefaultTransition is in practice a second copy of isAnyStateTransition rather
                // than the graph's own flag. It is almost certainly a copy-paste slip in the original
                // — it also skews the sourceType classification below towards entry — but it is
                // behaviour the shipped tool has, so it is reproduced rather than corrected.
                isDefaultTransition = (bool)isAnyStateTransitionRef.Member.GetValue(context);

                transition = (AnimatorTransitionBase)transitionRef.Member.GetValue(context);

                bool hasTransition = transition != null;
                destinationState = hasTransition ? transition.destinationState : null;
                destinationStateMachine = hasTransition ? transition.destinationStateMachine : null;

                ownerStateMachine = (AnimatorStateMachine)ownerStateMachineRef.Member.GetValue(context);
                sourceState = (AnimatorState)sourceStateRef.Member.GetValue(context);

                // Any State and default Entry transitions are held by the owner itself, and the
                // graph leaves their sourceStateMachine unset.
                sourceStateMachine = isAnyStateTransition || isDefaultTransition
                    ? ownerStateMachine
                    : (AnimatorStateMachine)sourceStateMachineRef.Member.GetValue(context);

                sourceType =
                    sourceState != null ? GraphNodeRef.NodeType.state :
                    sourceStateMachine == null ? GraphNodeRef.NodeType.unknown :
                    isAnyStateTransition ? GraphNodeRef.NodeType.any :
                    isDefaultTransition || ownerStateMachine != null ? GraphNodeRef.NodeType.entry :
                    GraphNodeRef.NodeType.machine;

                // A transition with neither destination goes to Exit; Unity records that by leaving
                // both destinations null rather than by naming the exit node.
                destinationType =
                    destinationState != null ? GraphNodeRef.NodeType.state :
                    destinationStateMachine != null ? GraphNodeRef.NodeType.machine :
                    GraphNodeRef.NodeType.exit;

                isExplicitEntryTransition = sourceType == GraphNodeRef.NodeType.entry && !isDefaultTransition;
                isExitTransition = destinationType == GraphNodeRef.NodeType.exit;

                stateTransition = isAnyStateTransition || sourceType == GraphNodeRef.NodeType.state
                    ? (AnimatorStateTransition)transition
                    : null;

                animatorTransition = isExplicitEntryTransition || sourceType == GraphNodeRef.NodeType.machine
                    ? (AnimatorTransition)transition
                    : null;
            }

            /// <summary>The short label the graph shows on the arrow.</summary>
            public string DisplayName
            {
                get
                {
                    return (string)m_DisplayNameRef.Member.GetValue(context);
                }
            }

            /// <summary>The full "source -> destination" name.</summary>
            public string FullName
            {
                get
                {
                    return (string)m_FullNameRef.Member.GetValue(context);
                }
            }

            /// <summary>
            /// Deletes this transition from whatever holds it.
            /// </summary>
            /// <remarks>
            /// Which call removes a transition depends entirely on where it starts, and asking the
            /// wrong owner silently does nothing — hence the switch on <see cref="sourceType"/>. The
            /// default Entry transition is not removable at all: Unity keeps exactly one per state
            /// machine and rewrites it when the default state changes.
            /// </remarks>
            public void Remove()
            {
                switch (sourceType)
                {
                    case GraphNodeRef.NodeType.any:
                        sourceStateMachine.RemoveAnyStateTransition(stateTransition);
                        break;
                    case GraphNodeRef.NodeType.state:
                        sourceState.RemoveTransition(stateTransition);
                        break;
                    case GraphNodeRef.NodeType.machine:
                        ownerStateMachine.RemoveStateMachineTransition(sourceStateMachine, animatorTransition);
                        break;
                    case GraphNodeRef.NodeType.entry:
                        if (!isDefaultTransition)
                        {
                            sourceStateMachine.RemoveEntryTransition(animatorTransition);
                        }
                        break;
                    case GraphNodeRef.NodeType.tree:
                    case GraphNodeRef.NodeType.exit:
                        break;
                }
            }
        }
    }
}
