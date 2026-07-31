// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   the members declared directly on the type, after the nested types:
//     FindNode(AnimatorState)                    -> FindNode,             line 809
//     FindNode(AnimatorStateMachine)             -> FindNode,             line 814
//     GetOwningStateMachine(AnimatorState)       -> GetOwningStateMachine, line 819
//     GetOwningStateMachine(AnimatorStateMachine)-> GetOwningStateMachine, line 824
//     GetOwningStateMachine(GraphNodeRef)        -> GetOwningStateMachine, line 829
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The nested types live in the sibling files of this folder: TypeResolvers, MemberRefs,
// GraphAccessors, GraphNodeRef, GraphEdgeRef, GraphSlotRef and TransitionEditionInfo.

using UnityEditor.Animations;
using UnityEditor.Graphs;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Drives Unity's Animator window through its internals: which controller and state machine it
    /// is showing, the nodes and transition arrows it has laid out, and what the user has selected.
    /// </summary>
    /// <remarks>
    /// None of this is public API. The window, its graph and every node and edge type live in the
    /// <c>UnityEditor.Graphs</c> assembly with internal visibility, so each member has to be reached
    /// by name through <see cref="TypeResolvers"/> and <see cref="MemberRefs"/>. Going through the
    /// window rather than reading the controller asset directly is what lets the tool act on what the
    /// user is looking at — a selection, a layout position, an arrow that stands for several
    /// transitions — none of which the asset records.
    /// </remarks>
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>The graph node standing for a state, or a reference wrapping null if it has none.</summary>
        /// <remarks>
        /// Only the state machine graph is searched, so a state inside a blend tree the window has
        /// descended into will not be found.
        /// </remarks>
        public static GraphNodeRef FindNode(AnimatorState state)
        {
            return new GraphNodeRef((Node)MemberRefs.findNodeByState.Member.Invoke(
                GraphAccessors.StateMachineGraph, new object[] { state }));
        }

        /// <inheritdoc cref="FindNode(AnimatorState)"/>
        public static GraphNodeRef FindNode(AnimatorStateMachine stateMachine)
        {
            return new GraphNodeRef((Node)MemberRefs.findNodeByStateMachine.Member.Invoke(
                GraphAccessors.StateMachineGraph, new object[] { stateMachine }));
        }

        /// <summary>The state machine that contains the given state.</summary>
        /// <remarks>
        /// The controller gives no way back from a state to its parent, so this reads the lookup the
        /// graph builds while laying the layer out. It therefore only answers for states in the layer
        /// the window currently has open.
        /// </remarks>
        public static AnimatorStateMachine GetOwningStateMachine(AnimatorState state)
        {
            return (AnimatorStateMachine)MemberRefs.m_StateMachineProxyLookup.Member.GetValue(FindNode(state).Node);
        }

        /// <inheritdoc cref="GetOwningStateMachine(AnimatorState)"/>
        public static AnimatorStateMachine GetOwningStateMachine(AnimatorStateMachine stateMachine)
        {
            return (AnimatorStateMachine)MemberRefs.m_StateMachineLookup.Member.GetValue(FindNode(stateMachine).Node);
        }

        /// <summary>
        /// The state machine the given node belongs to, whichever kind of node it is.
        /// </summary>
        /// <remarks>
        /// Any State always belongs to the layer's root, whatever the window is currently showing;
        /// Entry nodes carry their state machine on themselves; Exit and blend tree nodes have no
        /// owner to give.
        /// </remarks>
        public static AnimatorStateMachine GetOwningStateMachine(GraphNodeRef node)
        {
            switch (node.nodeType)
            {
                case GraphNodeRef.NodeType.any:
                    return GraphAccessors.RootStateMachine;
                case GraphNodeRef.NodeType.entry:
                    return (AnimatorStateMachine)MemberRefs.m_StateMachine.Member.GetValue(node.Node);
                case GraphNodeRef.NodeType.state:
                    return GetOwningStateMachine(node.state);
                case GraphNodeRef.NodeType.machine:
                    return GetOwningStateMachine(node.stateMachine);
                default:
                    return null;
            }
        }
    }
}
