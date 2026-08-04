// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested class GraphNodeRef -> GraphNodeRef, line 396
//     nested enum NodeType             -> NodeType,                    line 398
//     nodeResolved                     -> nodeResolved,                line 409
//     state / stateMachine / nodeType  -> unchanged,                   lines 411-415
//     owningStateMachineResolved       -> owningStateMachineResolved,  line 417
//     cachedOwningStateMachine         -> cachedOwningStateMachine,    line 419
//     cachedNode                       -> cachedNode,                  line 421
//     OwningStateMachine()             -> OwningStateMachine,          line 424
//     Node()                           -> Node,                        line 435
//     Slots()                          -> Slots,                       line 466
//     InputEdges() / OutputEdges()     -> InputEdges / OutputEdges,    lines 472, 478
//     IncomingTransitions()            -> IncomingTransitions,         line 484
//     OutgoingTransitions()            -> OutgoingTransitions,         line 491
//     Color() / Color(Styles.Color)    -> Color { get; set; },         lines 498, 504
//     Position() / Position(Rect)      -> Position { get; set; },      lines 510, 516
//     the four constructors            -> unchanged,                   lines 521-578
//     implicit operator Node           -> unchanged,                   line 580
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The [SpecialName] methods in the decompiled source are property accessors the deobfuscation pass
// left as methods; they are restored to properties here.
// Audit status: VERIFIED against decompiled/ member-by-member (2026-08-04).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// Refers to one node of the Animator graph, either by the controller asset it stands for or
        /// by the graph node itself.
        /// </summary>
        /// <remarks>
        /// A node can be named before the graph is asked for it: constructing from an
        /// <see cref="AnimatorState"/> or <see cref="AnimatorStateMachine"/> records only what to
        /// look for, and the lookup happens the first time <see cref="Node"/> is read. That matters
        /// because the graph is rebuilt whenever the window navigates, so a node instance held across
        /// that boundary is stale while a reference by asset is not.
        /// </remarks>
        internal class GraphNodeRef
        {
            /// <summary>What a node stands for. Entry, Exit and Any State have no asset behind them.</summary>
            internal enum NodeType
            {
                unknown,
                state,
                machine,
                tree,
                entry,
                exit,
                any
            }

            public bool nodeResolved;

            /// <summary>Set when <see cref="nodeType"/> is <see cref="NodeType.state"/>.</summary>
            public readonly AnimatorState state;

            /// <summary>Set when <see cref="nodeType"/> is <see cref="NodeType.machine"/>.</summary>
            public readonly AnimatorStateMachine stateMachine;

            public readonly NodeType nodeType;

            public bool owningStateMachineResolved;

            private AnimatorStateMachine cachedOwningStateMachine;

            private Node cachedNode;

            public GraphNodeRef()
            {
            }

            /// <summary>Wraps a node the graph already handed us, classifying it by its runtime type.</summary>
            public GraphNodeRef(Node node)
            {
                nodeResolved = true;
                cachedNode = node;

                if (node == null)
                {
                    return;
                }

                Type type = node.GetType();

                if (type == TypeResolvers.stateNode.ResolvedType)
                {
                    nodeType = NodeType.state;
                    state = (AnimatorState)MemberRefs.state.Member.GetValue(node);
                }
                else if (type == TypeResolvers.stateMachineNode.ResolvedType)
                {
                    nodeType = NodeType.machine;
                    stateMachine = (AnimatorStateMachine)MemberRefs.stateMachine.Member.GetValue(node);
                }
                else if (type == TypeResolvers.entryNode.ResolvedType)
                {
                    nodeType = NodeType.entry;
                }
                else if (type == TypeResolvers.exitNode.ResolvedType)
                {
                    nodeType = NodeType.exit;
                }
                else if (type == TypeResolvers.anyStateNode.ResolvedType)
                {
                    nodeType = NodeType.any;
                }
                else if (type == TypeResolvers.blendTreeNode.ResolvedType)
                {
                    nodeType = NodeType.tree;
                }

                // Anything else stays NodeType.unknown.
            }

            public GraphNodeRef(AnimatorState state)
            {
                this.state = state;
                nodeType = NodeType.state;
            }

            public GraphNodeRef(AnimatorStateMachine stateMachine)
            {
                this.stateMachine = stateMachine;
                nodeType = NodeType.machine;
            }

            /// <summary>The state machine this node lives in, resolved once and remembered.</summary>
            public AnimatorStateMachine OwningStateMachine
            {
                get
                {
                    if (!owningStateMachineResolved)
                    {
                        owningStateMachineResolved = true;
                        cachedOwningStateMachine = GetOwningStateMachine(this);
                    }

                    return cachedOwningStateMachine;
                }
            }

            /// <summary>
            /// The graph node itself, looked up from <see cref="state"/> or <see cref="stateMachine"/>
            /// on first access.
            /// </summary>
            /// <remarks>
            /// A failed lookup is remembered as a null just as a successful one is remembered, so a
            /// reference to something the current graph does not contain is not re-searched on every
            /// access. Blend tree nodes are never looked up: the state machine graph has no entry for
            /// them, so they resolve to null unless the node was handed to us directly.
            /// </remarks>
            public Node Node
            {
                get
                {
                    if (nodeResolved || cachedNode != null)
                    {
                        return cachedNode;
                    }

                    nodeResolved = true;

                    switch (nodeType)
                    {
                        case NodeType.any:
                            cachedNode = GraphAccessors.AnyStateNode.Node;
                            break;
                        case NodeType.exit:
                            cachedNode = GraphAccessors.ExitNode.Node;
                            break;
                        case NodeType.entry:
                            cachedNode = GraphAccessors.EntryNode.Node;
                            break;
                        case NodeType.state:
                            cachedNode = FindNode(state).Node;
                            break;
                        case NodeType.tree:
                            break;
                        default:
                            cachedNode = FindNode(stateMachine).Node;
                            break;
                    }

                    return cachedNode;
                }
            }

            public IEnumerable<GraphSlotRef> Slots
            {
                get
                {
                    return Node.slots.Select(s => new GraphSlotRef(s));
                }
            }

            public IEnumerable<GraphEdgeRef> InputEdges
            {
                get
                {
                    return Node.inputEdges.Select(e => new GraphEdgeRef(e));
                }
            }

            public IEnumerable<GraphEdgeRef> OutputEdges
            {
                get
                {
                    return Node.outputEdges.Select(e => new GraphEdgeRef(e));
                }
            }

            /// <summary>
            /// Every transition arriving at this node. One incoming edge can carry several.
            /// </summary>
            public IEnumerable<AnimatorTransitionBase> IncomingTransitions
            {
                get
                {
                    return InputEdges.SelectMany(e => e.Transitions.Select(t => t.transition));
                }
            }

            /// <inheritdoc cref="IncomingTransitions"/>
            public IEnumerable<AnimatorTransitionBase> OutgoingTransitions
            {
                get
                {
                    return OutputEdges.SelectMany(e => e.Transitions.Select(t => t.transition));
                }
            }

            /// <summary>The node's tint in the graph view.</summary>
            public Styles.Color Color
            {
                get
                {
                    return Node.color;
                }
                set
                {
                    Node.color = value;
                }
            }

            /// <summary>The node's rectangle in graph space.</summary>
            public Rect Position
            {
                get
                {
                    return Node.position;
                }
                set
                {
                    Node.position = value;
                }
            }

            public static implicit operator Node(GraphNodeRef reference)
            {
                return reference.Node;
            }
        }
    }
}
