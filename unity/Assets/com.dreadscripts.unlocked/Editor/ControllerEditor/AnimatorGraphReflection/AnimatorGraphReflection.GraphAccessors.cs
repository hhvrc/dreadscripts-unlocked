// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested static class GraphAccessors -> GraphAccessors, line 114
//     static cachedTool                    -> cachedTool,               line 116
//     static Tool() / Tool(EditorWindow)   -> Tool { get; set; },       lines 119, 129
//     static AnimatorController()          -> AnimatorController,       line 135
//     static StateMachineGraph()           -> StateMachineGraph,        line 145
//     static BlendTreeGraph()              -> BlendTreeGraph,           line 155
//     static StateMachineGraphGUI()        -> StateMachineGraphGUI,     line 165
//     static BlendTreeGraphGUI()           -> BlendTreeGraphGUI,        line 175
//     static ActiveGraph()                 -> ActiveGraph,              line 185
//     static ActiveGraphGUI()              -> ActiveGraphGUI,           line 199
//     static EdgeGUI()                     -> EdgeGUI,                  line 213
//     static ActiveStateMachine()          -> ActiveStateMachine,       line 223
//     static RootStateMachine()            -> RootStateMachine,         line 233
//     static ParentStateMachine()          -> ParentStateMachine,       line 243
//     static EntryNode()                   -> EntryNode,                line 253
//     static ExitNode()                    -> ExitNode,                 line 263
//     static AnyStateNode()                -> AnyStateNode,             line 273
//     static Nodes()                       -> Nodes,                    line 283
//     static Edges()                       -> Edges,                    line 294
//     static SelectedNodes()               -> SelectedNodes,            line 305
//     static SelectedEdges()               -> SelectedEdges,            line 316
//     static GraphDirtyCallback() / (Action) -> GraphDirtyCallback { get; set; }, lines 340, 346
//     static IsInBlendTree()               -> IsInBlendTree,            line 352
//     static GetBreadCrumbs()              -> GetBreadCrumbs,           line 362
//     static GetBreadCrumbTargets()        -> GetBreadCrumbTargets,     line 367
//     static GetTransitionToEdgeMap()      -> GetTransitionToEdgeMap,   line 382
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Everything marked [SpecialName] in the decompiled source is a property accessor that the
// deobfuscation pass left as a method; those are restored to properties here, which is why the
// call sites read Tool rather than Tool().
// Audit status: VERIFIED against decompiled/ member-by-member (2026-08-04).

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// Reads the state of the open Animator window: which controller and state machine it is
        /// showing, the graph and graph GUI behind it, and what the user has selected.
        /// </summary>
        /// <remarks>
        /// Every accessor tolerates the window being closed by returning null rather than throwing,
        /// so callers can ask about the graph unconditionally and act only when there is one.
        /// </remarks>
        internal static class GraphAccessors
        {
            private static EditorWindow cachedTool;

            /// <summary>
            /// The open Animator window, taken from the static field the window keeps of itself.
            /// </summary>
            /// <remarks>
            /// The cache is re-read whenever it is null, which — because Unity's <c>Object</c>
            /// equality treats a destroyed window as null — also covers the window having been
            /// closed and reopened since the last call. The setter writes Unity's field and
            /// deliberately leaves <see cref="cachedTool"/> alone, so the next read picks the new
            /// value up through the same path.
            /// </remarks>
            public static EditorWindow Tool
            {
                get
                {
                    if (cachedTool != null)
                    {
                        return cachedTool;
                    }

                    return cachedTool = (EditorWindow)MemberRefs.tool.Member.GetValue(null);
                }
                set
                {
                    MemberRefs.tool.Member.SetValue(null, value);
                }
            }

            /// <summary>The controller the window is showing, or null if it is closed.</summary>
            public static AnimatorController AnimatorController
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    return (AnimatorController)MemberRefs.animatorController.Member.GetValue(Tool);
                }
            }

            public static Graph StateMachineGraph
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    return (Graph)MemberRefs.stateMachineGraph.Member.GetValue(Tool);
                }
            }

            public static Graph BlendTreeGraph
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    return (Graph)MemberRefs.blendTreeGraph.Member.GetValue(Tool);
                }
            }

            public static GraphGUI StateMachineGraphGUI
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    return (GraphGUI)MemberRefs.stateMachineGraphGUI.Member.GetValue(Tool);
                }
            }

            public static GraphGUI BlendTreeGraphGUI
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    return (GraphGUI)MemberRefs.blendTreeGraphGUI.Member.GetValue(Tool);
                }
            }

            /// <summary>
            /// Whichever of the two graphs the window is currently drawing.
            /// </summary>
            /// <remarks>
            /// The window keeps both graphs alive at once and swaps which one is drawn as the user
            /// walks into and out of a blend tree, so "the graph" always has to go through
            /// <see cref="IsInBlendTree"/>.
            /// </remarks>
            public static Graph ActiveGraph
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    if (IsInBlendTree)
                    {
                        return BlendTreeGraph;
                    }

                    return StateMachineGraph;
                }
            }

            /// <inheritdoc cref="ActiveGraph"/>
            public static GraphGUI ActiveGraphGUI
            {
                get
                {
                    if (Tool == null)
                    {
                        return null;
                    }

                    if (IsInBlendTree)
                    {
                        return BlendTreeGraphGUI;
                    }

                    return StateMachineGraphGUI;
                }
            }

            /// <summary>
            /// The object that draws and selects transition arrows. Only the state machine graph has
            /// one; blend trees have no transitions.
            /// </summary>
            public static IEdgeGUI EdgeGUI
            {
                get
                {
                    if (StateMachineGraphGUI == null)
                    {
                        return null;
                    }

                    return (IEdgeGUI)MemberRefs.edgeGUI.Member.GetValue(StateMachineGraphGUI);
                }
            }

            /// <summary>The state machine the window is currently inside.</summary>
            public static AnimatorStateMachine ActiveStateMachine
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return (AnimatorStateMachine)MemberRefs.m_ActiveStateMachine.Member.GetValue(StateMachineGraph);
                }
            }

            /// <summary>The layer's own state machine, however deep the window has been navigated.</summary>
            public static AnimatorStateMachine RootStateMachine
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return (AnimatorStateMachine)MemberRefs.rootStateMachine.Member.GetValue(StateMachineGraph);
                }
            }

            /// <summary>The parent of <see cref="ActiveStateMachine"/>, or null at the root.</summary>
            public static AnimatorStateMachine ParentStateMachine
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return (AnimatorStateMachine)MemberRefs.parentStateMachine.Member.GetValue(StateMachineGraph);
                }
            }

            /// <summary>The Entry node of the state machine currently shown.</summary>
            public static GraphNodeRef EntryNode
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return new GraphNodeRef((Node)MemberRefs.m_EntryNode.Member.GetValue(StateMachineGraph));
                }
            }

            /// <inheritdoc cref="EntryNode"/>
            public static GraphNodeRef ExitNode
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return new GraphNodeRef((Node)MemberRefs.m_ExitNode.Member.GetValue(StateMachineGraph));
                }
            }

            /// <inheritdoc cref="EntryNode"/>
            public static GraphNodeRef AnyStateNode
            {
                get
                {
                    if (StateMachineGraph == null)
                    {
                        return null;
                    }

                    return new GraphNodeRef((Node)MemberRefs.m_AnyStateNode.Member.GetValue(StateMachineGraph));
                }
            }

            /// <summary>Every node drawn in the active graph, in the order the graph holds them.</summary>
            public static IEnumerable<GraphNodeRef> Nodes
            {
                get
                {
                    if (ActiveGraph == null)
                    {
                        return null;
                    }

                    return ((IList)MemberRefs.nodes.Member.GetValue(ActiveGraph))
                        .Cast<Node>()
                        .Select(n => new GraphNodeRef(n));
                }
            }

            /// <summary>
            /// Every edge in the active graph. The index of an edge here is what
            /// <see cref="IEdgeGUI.edgeSelection"/> refers to, so the order matters.
            /// </summary>
            public static IEnumerable<GraphEdgeRef> Edges
            {
                get
                {
                    if (ActiveGraph == null)
                    {
                        return null;
                    }

                    return ((IList)MemberRefs.edges.Member.GetValue(ActiveGraph))
                        .Cast<Edge>()
                        .Select(e => new GraphEdgeRef(e));
                }
            }

            /// <summary>The nodes the user has selected in the graph view.</summary>
            public static IEnumerable<GraphNodeRef> SelectedNodes
            {
                get
                {
                    if (ActiveGraphGUI == null)
                    {
                        return null;
                    }

                    return ((IList)MemberRefs.selection.Member.GetValue(ActiveGraphGUI))
                        .Cast<Node>()
                        .Select(n => new GraphNodeRef(n));
                }
            }

            /// <summary>The transition arrows the user has selected, or null if there is no graph.</summary>
            /// <remarks>
            /// Unity stores the edge selection as indices into the graph's edge list rather than as
            /// edges, so the whole list has to be materialised first and then filtered by position.
            /// The result is unordered — the filtering runs in parallel into a bag — which is
            /// harmless because the caller only ever asks what is selected, never in what order.
            /// </remarks>
            public static ConcurrentBag<GraphEdgeRef> SelectedEdges
            {
                get
                {
                    if (StateMachineGraph == null || EdgeGUI == null)
                    {
                        return null;
                    }

                    GraphEdgeRef[] allEdges = Edges.ToArray();

                    HashSet<int> selectedIndices = new HashSet<int>();
                    foreach (int index in EdgeGUI.edgeSelection)
                    {
                        selectedIndices.Add(index);
                    }

                    ConcurrentBag<GraphEdgeRef> selected = new ConcurrentBag<GraphEdgeRef>();
                    Parallel.For(0, allEdges.Length, i =>
                    {
                        if (selectedIndices.Contains(i))
                        {
                            selected.Add(allEdges[i]);
                        }
                    });

                    return selected;
                }
            }

            /// <summary>
            /// The window's own callback for "the graph no longer matches the controller".
            /// </summary>
            /// <remarks>
            /// Exposed for both reading and writing so a caller can swap in its own handler, run a
            /// batch of edits, and put the original back — see the reflection restore scopes.
            /// </remarks>
            public static Action GraphDirtyCallback
            {
                get
                {
                    return (Action)MemberRefs.graphDirtyCallback.Member.GetValue(Tool);
                }
                set
                {
                    MemberRefs.graphDirtyCallback.Member.SetValue(Tool, value);
                }
            }

            /// <summary>
            /// True when the window has been navigated into a blend tree rather than a state machine.
            /// </summary>
            /// <remarks>
            /// Decided from the last breadcrumb, which is the only place the window records what it
            /// descended into. An empty trail means the window has nothing open at all.
            /// </remarks>
            public static bool IsInBlendTree
            {
                get
                {
                    IList breadCrumbs = GetBreadCrumbs();
                    if (breadCrumbs.Count == 0)
                    {
                        return false;
                    }

                    return !(MemberRefs.m_Target.Member.GetValue(breadCrumbs[breadCrumbs.Count - 1]) is AnimatorStateMachine);
                }
            }

            /// <summary>The window's breadcrumb elements, as the untyped list Unity keeps them in.</summary>
            public static IList GetBreadCrumbs()
            {
                return (IList)MemberRefs.m_BreadCrumbs.Member.GetValue(Tool);
            }

            /// <summary>
            /// What each breadcrumb points at, from the layer's root down to whatever is open now.
            /// </summary>
            public static UnityEngine.Object[] GetBreadCrumbTargets()
            {
                IList breadCrumbs = GetBreadCrumbs();
                if (breadCrumbs.Count == 0)
                {
                    return Array.Empty<UnityEngine.Object>();
                }

                UnityEngine.Object[] targets = new UnityEngine.Object[breadCrumbs.Count];
                for (int i = 0; i < breadCrumbs.Count; i++)
                {
                    targets[i] = (UnityEngine.Object)MemberRefs.m_Target.Member.GetValue(breadCrumbs[i]);
                }

                return targets;
            }

            /// <summary>
            /// Maps each transition in the active graph back to the edge it is drawn on.
            /// </summary>
            /// <remarks>
            /// Several transitions between the same two states share one arrow, so this is
            /// many-to-one; it exists because the graph only offers the opposite direction.
            /// </remarks>
            public static Dictionary<AnimatorTransitionBase, GraphEdgeRef> GetTransitionToEdgeMap()
            {
                Dictionary<AnimatorTransitionBase, GraphEdgeRef> map =
                    new Dictionary<AnimatorTransitionBase, GraphEdgeRef>();

                foreach (GraphEdgeRef edge in Edges)
                {
                    foreach (TransitionEditionInfo transition in edge.Transitions)
                    {
                        map[transition.transition] = edge;
                    }
                }

                return map;
            }
        }
    }
}
