// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested static class MemberRefs -> MemberRefs, line 51
//     every member reference keeps its decompiled name (tool, stateMachineGraph, blendTreeGraph,
//     stateMachineGraphGUI, blendTreeGraphGUI, m_BreadCrumbs, graphDirtyCallback, m_Target,
//     m_ActiveStateMachine, rootStateMachine, parentStateMachine, selection, nodes, edges,
//     m_EntryNode, m_ExitNode, m_AnyStateNode, state, m_StateMachine, stateMachine,
//     m_StateMachineProxyLookup, m_StateMachineLookup, transitions, GetEdgeInfo,
//     findNodeByState, findNodeByStateMachine, HasTransition, animatorController, edgeGUI,
//     hasMultipleTransitions), lines 53-111
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The decompiled source writes the type as DreadScripts.ControllerEditor.ReflectionMemberRef<T>
// in full; the namespace is already in scope here, so the qualification is dropped.
// Audit status: VERIFIED against export member-by-member (2026-08-04). All 30 member refs present.

using System.Reflection;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// The individual fields, properties and methods read out of the Animator window's internals.
        /// </summary>
        /// <remarks>
        /// Field names deliberately mirror the Unity member they point at, <c>m_</c> prefixes and
        /// all, so that a break against a new Unity version can be traced back to the exact internal
        /// member that moved. Only the two <c>FindNode</c> overloads and the two graph flavours
        /// (state machine vs blend tree) need names of our own choosing.
        /// </remarks>
        internal static class MemberRefs
        {
            /// <summary>The open Animator window. A static field, so it is read with a null instance.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> tool =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "tool");

            public static readonly ReflectionMemberRef<FieldInfo> stateMachineGraph =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraph");

            public static readonly ReflectionMemberRef<FieldInfo> blendTreeGraph =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraph");

            public static readonly ReflectionMemberRef<FieldInfo> stateMachineGraphGUI =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraphGUI");

            public static readonly ReflectionMemberRef<FieldInfo> blendTreeGraphGUI =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraphGUI");

            /// <summary>The breadcrumb trail, whose last entry says what the window is looking at.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> m_BreadCrumbs =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "m_BreadCrumbs");

            /// <summary>The window's own "graph needs rebuilding" callback.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> graphDirtyCallback =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "graphDirtyCallback");

            /// <summary>What one breadcrumb points at: a state machine, or a blend tree.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> m_Target =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.breadCrumbElement, "m_Target");

            public static readonly ReflectionMemberRef<FieldInfo> m_ActiveStateMachine =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ActiveStateMachine");

            public static readonly ReflectionMemberRef<FieldInfo> rootStateMachine =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "rootStateMachine");

            public static readonly ReflectionMemberRef<FieldInfo> parentStateMachine =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "parentStateMachine");

            /// <summary>The nodes the user currently has selected, on the GUI rather than the graph.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> selection =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.graphGUI, "selection");

            public static readonly ReflectionMemberRef<FieldInfo> nodes =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "nodes");

            public static readonly ReflectionMemberRef<FieldInfo> edges =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "edges");

            public static readonly ReflectionMemberRef<FieldInfo> m_EntryNode =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_EntryNode");

            public static readonly ReflectionMemberRef<FieldInfo> m_ExitNode =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ExitNode");

            public static readonly ReflectionMemberRef<FieldInfo> m_AnyStateNode =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_AnyStateNode");

            /// <summary>The <see cref="AnimatorState"/> behind a state node.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> state =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "state");

            /// <summary>The state machine an entry node belongs to.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> m_StateMachine =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.entryNode, "m_StateMachine");

            /// <summary>The <see cref="AnimatorStateMachine"/> behind a sub-state-machine node.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> stateMachine =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineNode, "stateMachine");

            /// <summary>
            /// Declared on <c>StateNode</c>, but the graph uses it as the owning state machine of the
            /// node's proxy — see <see cref="GetOwningStateMachine(AnimatorState)"/>.
            /// </summary>
            public static readonly ReflectionMemberRef<FieldInfo> m_StateMachineProxyLookup =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineProxyLookup");

            /// <inheritdoc cref="m_StateMachineProxyLookup"/>
            public static readonly ReflectionMemberRef<FieldInfo> m_StateMachineLookup =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineLookup");

            /// <summary>Every transition drawn on one edge.</summary>
            public static readonly ReflectionMemberRef<FieldInfo> transitions =
                new ReflectionMemberRef<FieldInfo>(TypeResolvers.edgeInfo, "transitions");

            public static readonly ReflectionMemberRef<MethodInfo> GetEdgeInfo =
                new ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "GetEdgeInfo");

            /// <summary>
            /// <c>FindNode</c> is overloaded on state versus state machine; the parameter type picks
            /// which one this refers to.
            /// </summary>
            public static readonly ReflectionMemberRef<MethodInfo> findNodeByState =
                new ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorState));

            /// <inheritdoc cref="findNodeByState"/>
            public static readonly ReflectionMemberRef<MethodInfo> findNodeByStateMachine =
                new ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorStateMachine));

            public static readonly ReflectionMemberRef<MethodInfo> HasTransition =
                new ReflectionMemberRef<MethodInfo>(TypeResolvers.edgeInfo, "HasTransition", typeof(AnimatorTransitionBase));

            public static readonly ReflectionMemberRef<PropertyInfo> animatorController =
                new ReflectionMemberRef<PropertyInfo>(TypeResolvers.animatorControllerTool, "animatorController");

            public static readonly ReflectionMemberRef<PropertyInfo> edgeGUI =
                new ReflectionMemberRef<PropertyInfo>(TypeResolvers.stateMachineGraphGUI, "edgeGUI");

            public static readonly ReflectionMemberRef<PropertyInfo> hasMultipleTransitions =
                new ReflectionMemberRef<PropertyInfo>(TypeResolvers.edgeInfo, "hasMultipleTransitions");
        }
    }
}
