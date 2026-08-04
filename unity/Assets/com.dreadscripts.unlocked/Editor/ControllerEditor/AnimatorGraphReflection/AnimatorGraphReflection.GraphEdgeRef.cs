// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested class GraphEdgeRef -> GraphEdgeRef, line 586
//     edge                      -> edge,                     line 588
//     edgeInfoCached / edgeInfo -> edgeInfoCached / edgeInfo, lines 590-592
//     GetFromNode()             -> FromNode,                 line 595
//     GetToNode()               -> ToNode,                   line 601
//     GetFromSlot()             -> FromSlot,                 line 607
//     GetToSlot()               -> ToSlot,                   line 613
//     GetEdgeInfo()             -> EdgeInfo,                 line 619
//     HasTransition()           -> HasTransition,            line 630
//     HasMultipleTransitions()  -> HasMultipleTransitions,   line 635
//     GetTransitions()          -> Transitions,              line 641
//     constructor               -> unchanged,                line 647
//     implicit operator Edge    -> unchanged,                line 652
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The [SpecialName] methods in the decompiled source are property accessors the deobfuscation pass
// left as methods; they are restored to properties here. MoveSerializer (line 658) is the
// compiler-generated body of the Transitions projection and has no separate port.
// Audit status: VERIFIED against export member-by-member (2026-08-04). The [CompilerGenerated]
// MoveSerializer closure is correctly dissolved into the Transitions projection, not given a file.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEditor.Graphs;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// Refers to one arrow in the Animator graph, and to the transitions drawn on it.
        /// </summary>
        /// <remarks>
        /// An edge is not a transition: Unity collapses every transition between the same two nodes
        /// onto a single arrow, and keeps the list of them in a separate <c>EdgeInfo</c> record that
        /// only the graph can produce. That record is what <see cref="Transitions"/>,
        /// <see cref="HasTransition"/> and <see cref="HasMultipleTransitions"/> read, so it is fetched
        /// once per edge and remembered.
        /// </remarks>
        internal class GraphEdgeRef
        {
            public readonly Edge edge;

            private bool edgeInfoCached;

            private object edgeInfo;

            public GraphEdgeRef(Edge edge)
            {
                this.edge = edge;
            }

            /// <summary>The node this arrow leaves.</summary>
            public GraphNodeRef FromNode
            {
                get
                {
                    return new GraphNodeRef(edge.fromSlot.node);
                }
            }

            /// <summary>The node this arrow arrives at.</summary>
            public GraphNodeRef ToNode
            {
                get
                {
                    return new GraphNodeRef(edge.toSlot.node);
                }
            }

            public GraphSlotRef FromSlot
            {
                get
                {
                    return new GraphSlotRef(edge.fromSlot);
                }
            }

            public GraphSlotRef ToSlot
            {
                get
                {
                    return new GraphSlotRef(edge.toSlot);
                }
            }

            /// <summary>
            /// The graph's <c>EdgeInfo</c> record for this edge, asked for once and then remembered
            /// even if it came back null.
            /// </summary>
            private object EdgeInfo
            {
                get
                {
                    if (!edgeInfoCached)
                    {
                        edgeInfoCached = true;
                        edgeInfo = MemberRefs.GetEdgeInfo.Member.Invoke(
                            GraphAccessors.StateMachineGraph, new object[] { edge });
                    }

                    return edgeInfo;
                }
            }

            /// <summary>True when the given transition is one of those drawn on this arrow.</summary>
            public bool HasTransition(AnimatorTransitionBase transition)
            {
                return (bool)MemberRefs.HasTransition.Member.Invoke(EdgeInfo, new object[] { transition });
            }

            /// <summary>True when this arrow stands for more than one transition.</summary>
            public bool HasMultipleTransitions()
            {
                return (bool)MemberRefs.hasMultipleTransitions.Member.GetValue(EdgeInfo);
            }

            /// <summary>Every transition drawn on this arrow, in the order the graph lists them.</summary>
            public IEnumerable<TransitionEditionInfo> Transitions
            {
                get
                {
                    return ((IList)MemberRefs.transitions.Member.GetValue(EdgeInfo))
                        .Cast<object>()
                        .Select(context => new TransitionEditionInfo(context, this));
                }
            }

            public static implicit operator Edge(GraphEdgeRef reference)
            {
                return reference.edge;
            }
        }
    }
}
