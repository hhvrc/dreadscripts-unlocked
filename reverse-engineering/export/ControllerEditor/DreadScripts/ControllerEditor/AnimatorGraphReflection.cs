using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class AnimatorGraphReflection
{
	internal static class TypeResolvers
	{
		public static readonly TypeResolver stateMachineGraph = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver stateMachineGraphGUI = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver edgeGUI = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver animatorControllerTool = new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver breadCrumbElement = new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool+BreadCrumbElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver graph = new TypeResolver("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver graphGUI = new TypeResolver("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver stateNode = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver blendTreeNode = new TypeResolver("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver stateMachineNode = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateMachineNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver entryNode = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EntryNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver anyStateNode = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.AnyStateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver exitNode = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.ExitNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver edgeInfo = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeInfo, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver transitionEditionContext = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.TransitionEditionContext, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}

	internal static class MemberRefs
	{
		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> tool = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "tool");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> stateMachineGraph = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> blendTreeGraph = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> stateMachineGraphGUI = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> blendTreeGraphGUI = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_BreadCrumbs = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "m_BreadCrumbs");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> graphDirtyCallback = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "graphDirtyCallback");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_Target = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.breadCrumbElement, "m_Target");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ActiveStateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ActiveStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> rootStateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "rootStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> parentStateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "parentStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> selection = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graphGUI, "selection");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> nodes = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "nodes");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> edges = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "edges");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_EntryNode = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_EntryNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ExitNode = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ExitNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_AnyStateNode = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_AnyStateNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> state = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "state");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_StateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.entryNode, "m_StateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> stateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineNode, "stateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_StateMachineProxyLookup = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineProxyLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_StateMachineLookup = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> transitions = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.edgeInfo, "transitions");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> GetEdgeInfo = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "GetEdgeInfo");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> findNodeByState = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorState));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> findNodeByStateMachine = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorStateMachine));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> HasTransition = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.edgeInfo, "HasTransition", typeof(AnimatorTransitionBase));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> animatorController = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.animatorControllerTool, "animatorController");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> edgeGUI = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.stateMachineGraphGUI, "edgeGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> hasMultipleTransitions = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.edgeInfo, "hasMultipleTransitions");
	}

	internal static class GraphAccessors
	{
		private static EditorWindow cachedTool;

		[SpecialName]
		public static EditorWindow Tool()
		{
			if (!(cachedTool != null))
			{
				return cachedTool = (EditorWindow)MemberRefs.tool.GetMember().GetValue(null);
			}
			return cachedTool;
		}

		[SpecialName]
		public static void Tool(EditorWindow info)
		{
			MemberRefs.tool.GetMember().SetValue(null, info);
		}

		[SpecialName]
		public static AnimatorController AnimatorController()
		{
			if (Tool() == null)
			{
				return null;
			}
			return (AnimatorController)MemberRefs.animatorController.GetMember().GetValue(Tool());
		}

		[SpecialName]
		public static Graph StateMachineGraph()
		{
			if (Tool() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.stateMachineGraph.GetMember().GetValue(Tool());
		}

		[SpecialName]
		public static Graph BlendTreeGraph()
		{
			if (Tool() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.blendTreeGraph.GetMember().GetValue(Tool());
		}

		[SpecialName]
		public static GraphGUI StateMachineGraphGUI()
		{
			if (!(Tool() == null))
			{
				return (GraphGUI)MemberRefs.stateMachineGraphGUI.GetMember().GetValue(Tool());
			}
			return null;
		}

		[SpecialName]
		public static GraphGUI BlendTreeGraphGUI()
		{
			if (Tool() == null)
			{
				return null;
			}
			return (GraphGUI)MemberRefs.blendTreeGraphGUI.GetMember().GetValue(Tool());
		}

		[SpecialName]
		public static Graph ActiveGraph()
		{
			if (!(Tool() == null))
			{
				if (IsInBlendTree())
				{
					return BlendTreeGraph();
				}
				return StateMachineGraph();
			}
			return null;
		}

		[SpecialName]
		public static GraphGUI ActiveGraphGUI()
		{
			if (!(Tool() == null))
			{
				if (IsInBlendTree())
				{
					return BlendTreeGraphGUI();
				}
				return StateMachineGraphGUI();
			}
			return null;
		}

		[SpecialName]
		public static IEdgeGUI EdgeGUI()
		{
			if (!(StateMachineGraphGUI() == null))
			{
				return (IEdgeGUI)MemberRefs.edgeGUI.GetMember().GetValue(StateMachineGraphGUI());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine ActiveStateMachine()
		{
			if (!(StateMachineGraph() == null))
			{
				return (AnimatorStateMachine)MemberRefs.m_ActiveStateMachine.GetMember().GetValue(StateMachineGraph());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine RootStateMachine()
		{
			if (StateMachineGraph() == null)
			{
				return null;
			}
			return (AnimatorStateMachine)MemberRefs.rootStateMachine.GetMember().GetValue(StateMachineGraph());
		}

		[SpecialName]
		public static AnimatorStateMachine ParentStateMachine()
		{
			if (!(StateMachineGraph() == null))
			{
				return (AnimatorStateMachine)MemberRefs.parentStateMachine.GetMember().GetValue(StateMachineGraph());
			}
			return null;
		}

		[SpecialName]
		public static GraphNodeRef EntryNode()
		{
			if (!(StateMachineGraph() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_EntryNode.GetMember().GetValue(StateMachineGraph()));
			}
			return null;
		}

		[SpecialName]
		public static GraphNodeRef ExitNode()
		{
			if (StateMachineGraph() == null)
			{
				return null;
			}
			return new GraphNodeRef((Node)MemberRefs.m_ExitNode.GetMember().GetValue(StateMachineGraph()));
		}

		[SpecialName]
		public static GraphNodeRef AnyStateNode()
		{
			if (!(StateMachineGraph() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_AnyStateNode.GetMember().GetValue(StateMachineGraph()));
			}
			return null;
		}

		[SpecialName]
		public static IEnumerable<GraphNodeRef> Nodes()
		{
			if (ActiveGraph() == null)
			{
				return null;
			}
			return from Node n in (IList)MemberRefs.nodes.GetMember().GetValue(ActiveGraph())
				select new GraphNodeRef(n);
		}

		[SpecialName]
		public static IEnumerable<GraphEdgeRef> Edges()
		{
			if (ActiveGraph() == null)
			{
				return null;
			}
			return from Edge e in (IList)MemberRefs.edges.GetMember().GetValue(ActiveGraph())
				select new GraphEdgeRef(e);
		}

		[SpecialName]
		public static IEnumerable<GraphNodeRef> SelectedNodes()
		{
			if (ActiveGraphGUI() == null)
			{
				return null;
			}
			return from Node n in (IList)MemberRefs.selection.GetMember().GetValue(ActiveGraphGUI())
				select new GraphNodeRef(n);
		}

		[SpecialName]
		public static ConcurrentBag<GraphEdgeRef> SelectedEdges()
		{
			if (StateMachineGraph() == null || EdgeGUI() == null)
			{
				return null;
			}
			GraphEdgeRef[] _ModelTests = Edges().ToArray();
			HashSet<int> roleTests = new HashSet<int>();
			foreach (int item in EdgeGUI().edgeSelection)
			{
				roleTests.Add(item);
			}
			ConcurrentBag<GraphEdgeRef> _ParamTests = new ConcurrentBag<GraphEdgeRef>();
			Parallel.For(0, _ModelTests.Length, delegate(int i)
			{
				if (roleTests.Contains(i))
				{
					_ParamTests.Add(_ModelTests[i]);
				}
			});
			return _ParamTests;
		}

		[SpecialName]
		public static Action GraphDirtyCallback()
		{
			return (Action)MemberRefs.graphDirtyCallback.GetMember().GetValue(Tool());
		}

		[SpecialName]
		public static void GraphDirtyCallback(Action value)
		{
			MemberRefs.graphDirtyCallback.GetMember().SetValue(Tool(), value);
		}

		[SpecialName]
		public static bool IsInBlendTree()
		{
			IList breadCrumbs = GetBreadCrumbs();
			if (breadCrumbs.Count == 0)
			{
				return false;
			}
			return !(MemberRefs.m_Target.GetMember().GetValue(breadCrumbs[breadCrumbs.Count - 1]) is AnimatorStateMachine);
		}

		public static IList GetBreadCrumbs()
		{
			return (IList)MemberRefs.m_BreadCrumbs.GetMember().GetValue(Tool());
		}

		public static UnityEngine.Object[] GetBreadCrumbTargets()
		{
			IList breadCrumbs = GetBreadCrumbs();
			if (breadCrumbs.Count == 0)
			{
				return Array.Empty<UnityEngine.Object>();
			}
			UnityEngine.Object[] array = new UnityEngine.Object[breadCrumbs.Count];
			for (int i = 0; i < breadCrumbs.Count; i++)
			{
				array[i] = (UnityEngine.Object)MemberRefs.m_Target.GetMember().GetValue(breadCrumbs[i]);
			}
			return array;
		}

		public static Dictionary<AnimatorTransitionBase, GraphEdgeRef> GetTransitionToEdgeMap()
		{
			Dictionary<AnimatorTransitionBase, GraphEdgeRef> dictionary = new Dictionary<AnimatorTransitionBase, GraphEdgeRef>();
			foreach (GraphEdgeRef item in Edges())
			{
				foreach (TransitionEditionInfo transition in item.GetTransitions())
				{
					dictionary[transition.transition] = item;
				}
			}
			return dictionary;
		}
	}

	internal class GraphNodeRef
	{
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

		public readonly AnimatorState state;

		public readonly AnimatorStateMachine stateMachine;

		public readonly NodeType nodeType;

		public bool owningStateMachineResolved;

		private AnimatorStateMachine cachedOwningStateMachine;

		private Node cachedNode;

		[SpecialName]
		public AnimatorStateMachine OwningStateMachine()
		{
			if (!owningStateMachineResolved)
			{
				owningStateMachineResolved = true;
				cachedOwningStateMachine = GetOwningStateMachine(this);
			}
			return cachedOwningStateMachine;
		}

		[SpecialName]
		public Node Node()
		{
			if (nodeResolved || cachedNode != null)
			{
				return cachedNode;
			}
			nodeResolved = true;
			switch (nodeType)
			{
			case NodeType.any:
				cachedNode = GraphAccessors.AnyStateNode().Node();
				break;
			case NodeType.exit:
				cachedNode = GraphAccessors.ExitNode().Node();
				break;
			default:
				cachedNode = FindNode(stateMachine).Node();
				break;
			case NodeType.entry:
				cachedNode = GraphAccessors.EntryNode().Node();
				break;
			case NodeType.state:
				cachedNode = FindNode(state).Node();
				break;
			case NodeType.tree:
				break;
			}
			return cachedNode;
		}

		[SpecialName]
		public IEnumerable<GraphSlotRef> Slots()
		{
			return Node().slots.Select((Slot s) => new GraphSlotRef(s));
		}

		[SpecialName]
		public IEnumerable<GraphEdgeRef> InputEdges()
		{
			return Node().inputEdges.Select((Edge e) => new GraphEdgeRef(e));
		}

		[SpecialName]
		public IEnumerable<GraphEdgeRef> OutputEdges()
		{
			return Node().outputEdges.Select((Edge e) => new GraphEdgeRef(e));
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> IncomingTransitions()
		{
			return InputEdges().SelectMany((GraphEdgeRef e) => from t in e.GetTransitions()
				select t.transition);
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> OutgoingTransitions()
		{
			return OutputEdges().SelectMany((GraphEdgeRef e) => from t in e.GetTransitions()
				select t.transition);
		}

		[SpecialName]
		public Styles.Color Color()
		{
			return Node().color;
		}

		[SpecialName]
		public void Color(Styles.Color ident)
		{
			Node().color = ident;
		}

		[SpecialName]
		public Rect Position()
		{
			return Node().position;
		}

		[SpecialName]
		public void Position(Rect last)
		{
			Node().position = last;
		}

		public GraphNodeRef()
		{
		}

		public GraphNodeRef(Node init)
		{
			nodeResolved = true;
			cachedNode = init;
			if (init == null)
			{
				return;
			}
			Type type = init.GetType();
			if (type == TypeResolvers.stateNode.ResolvedType())
			{
				nodeType = NodeType.state;
				state = (AnimatorState)MemberRefs.state.GetMember().GetValue(init);
			}
			else if (!(type == TypeResolvers.stateMachineNode.ResolvedType()))
			{
				if (type == TypeResolvers.entryNode.ResolvedType())
				{
					nodeType = NodeType.entry;
				}
				else if (type == TypeResolvers.exitNode.ResolvedType())
				{
					nodeType = NodeType.exit;
				}
				else if (!(type == TypeResolvers.anyStateNode.ResolvedType()))
				{
					if (type == TypeResolvers.blendTreeNode.ResolvedType())
					{
						nodeType = NodeType.tree;
					}
				}
				else
				{
					nodeType = NodeType.any;
				}
			}
			else
			{
				nodeType = NodeType.machine;
				stateMachine = (AnimatorStateMachine)MemberRefs.stateMachine.GetMember().GetValue(init);
			}
		}

		public GraphNodeRef(AnimatorState task)
		{
			state = task;
			nodeType = NodeType.state;
		}

		public GraphNodeRef(AnimatorStateMachine item)
		{
			stateMachine = item;
			nodeType = NodeType.machine;
		}

		public static implicit operator Node(GraphNodeRef reference)
		{
			return reference.Node();
		}
	}

	internal class GraphEdgeRef
	{
		public readonly Edge edge;

		private bool edgeInfoCached;

		private object edgeInfo;

		[SpecialName]
		public GraphNodeRef GetFromNode()
		{
			return new GraphNodeRef(edge.fromSlot.node);
		}

		[SpecialName]
		public GraphNodeRef GetToNode()
		{
			return new GraphNodeRef(edge.toSlot.node);
		}

		[SpecialName]
		public GraphSlotRef GetFromSlot()
		{
			return new GraphSlotRef(edge.fromSlot);
		}

		[SpecialName]
		public GraphSlotRef GetToSlot()
		{
			return new GraphSlotRef(edge.toSlot);
		}

		[SpecialName]
		private object GetEdgeInfo()
		{
			if (!edgeInfoCached)
			{
				edgeInfoCached = true;
				edgeInfo = MemberRefs.GetEdgeInfo.GetMember().Invoke(GraphAccessors.StateMachineGraph(), new object[1] { edge });
				return edgeInfo;
			}
			return edgeInfo;
		}

		public bool HasTransition(AnimatorTransitionBase ident)
		{
			return (bool)MemberRefs.HasTransition.GetMember().Invoke(GetEdgeInfo(), new object[1] { ident });
		}

		public bool HasMultipleTransitions()
		{
			return (bool)MemberRefs.hasMultipleTransitions.GetMember().GetValue(GetEdgeInfo());
		}

		[SpecialName]
		public IEnumerable<TransitionEditionInfo> GetTransitions()
		{
			return from object setup in (IList)MemberRefs.transitions.GetMember().GetValue(GetEdgeInfo())
				select new TransitionEditionInfo(setup, this);
		}

		public GraphEdgeRef(Edge value)
		{
			edge = value;
		}

		public static implicit operator Edge(GraphEdgeRef task)
		{
			return task.edge;
		}

		[CompilerGenerated]
		private TransitionEditionInfo MoveSerializer(object setup)
		{
			return new TransitionEditionInfo(setup, this);
		}
	}

	internal readonly struct TransitionEditionInfo
	{
		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DisplayNameRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_DisplayName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FullNameRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_FullName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> sourceStateRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceState");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> sourceStateMachineRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> ownerStateMachineRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "ownerStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> transitionRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "transition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> isAnyStateTransitionRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isAnyStateTransition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> isDefaultTransitionRef = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isDefaultTransition");

		public readonly object context;

		public readonly GraphEdgeRef edge;

		public readonly AnimatorTransitionBase transition;

		public readonly AnimatorStateTransition stateTransition;

		public readonly AnimatorTransition animatorTransition;

		public readonly AnimatorState sourceState;

		public readonly AnimatorStateMachine sourceStateMachine;

		public readonly AnimatorStateMachine ownerStateMachine;

		public readonly AnimatorState destinationState;

		public readonly AnimatorStateMachine destinationStateMachine;

		public readonly GraphNodeRef.NodeType destinationType;

		public readonly GraphNodeRef.NodeType sourceType;

		public readonly bool isAnyStateTransition;

		public readonly bool isDefaultTransition;

		public readonly bool isExplicitEntryTransition;

		public readonly bool isExitTransition;

		internal static object LogoutStruct;

		[SpecialName]
		public string DisplayName()
		{
			return (string)m_DisplayNameRef.GetMember().GetValue(context);
		}

		[SpecialName]
		public string FullName()
		{
			return (string)m_FullNameRef.GetMember().GetValue(context);
		}

		public TransitionEditionInfo(object config, GraphEdgeRef second)
		{
			context = config;
			edge = second;
			isAnyStateTransition = (bool)isAnyStateTransitionRef.GetMember().GetValue(config);
			isDefaultTransition = (bool)isAnyStateTransitionRef.GetMember().GetValue(config);
			transition = (AnimatorTransitionBase)transitionRef.GetMember().GetValue(config);
			bool flag = transition != null;
			destinationState = ((!flag) ? null : transition.destinationState);
			destinationStateMachine = ((!flag) ? null : transition.destinationStateMachine);
			ownerStateMachine = (AnimatorStateMachine)ownerStateMachineRef.GetMember().GetValue(config);
			sourceState = (AnimatorState)sourceStateRef.GetMember().GetValue(config);
			sourceStateMachine = (AnimatorStateMachine)((!isAnyStateTransition && !isDefaultTransition) ? sourceStateMachineRef.GetMember().GetValue(config) : ownerStateMachine);
			sourceType = ((sourceState != null) ? GraphNodeRef.NodeType.state : ((!(sourceStateMachine == null)) ? ((!isAnyStateTransition) ? ((!isDefaultTransition && !(ownerStateMachine != null)) ? GraphNodeRef.NodeType.machine : GraphNodeRef.NodeType.entry) : GraphNodeRef.NodeType.any) : GraphNodeRef.NodeType.unknown));
			destinationType = ((destinationState != null) ? GraphNodeRef.NodeType.state : ((destinationStateMachine != null) ? GraphNodeRef.NodeType.machine : GraphNodeRef.NodeType.exit));
			isExplicitEntryTransition = sourceType == GraphNodeRef.NodeType.entry && !isDefaultTransition;
			isExitTransition = destinationType == GraphNodeRef.NodeType.exit;
			stateTransition = ((!isAnyStateTransition && sourceType != GraphNodeRef.NodeType.state) ? null : ((AnimatorStateTransition)transition));
			animatorTransition = ((!isExplicitEntryTransition && sourceType != GraphNodeRef.NodeType.machine) ? null : ((AnimatorTransition)transition));
		}

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

		internal static bool FindStruct()
		{
			return LogoutStruct == null;
		}
	}

	internal readonly struct GraphSlotRef
	{
		public readonly Slot slot;

		private static object TestStruct;

		[SpecialName]
		public Node Node()
		{
			return slot.node;
		}

		[SpecialName]
		public List<GraphEdgeRef> Edges()
		{
			return new List<GraphEdgeRef>(slot.edges.Select((Edge e) => new GraphEdgeRef(e)));
		}

		public GraphSlotRef(Slot param)
		{
			slot = param;
		}

		internal static bool IncludeStruct()
		{
			return TestStruct == null;
		}
	}

	public static GraphNodeRef FindNode(AnimatorState i)
	{
		return new GraphNodeRef((Node)MemberRefs.findNodeByState.GetMember().Invoke(GraphAccessors.StateMachineGraph(), new object[1] { i }));
	}

	public static GraphNodeRef FindNode(AnimatorStateMachine last)
	{
		return new GraphNodeRef((Node)MemberRefs.findNodeByStateMachine.GetMember().Invoke(GraphAccessors.StateMachineGraph(), new object[1] { last }));
	}

	public static AnimatorStateMachine GetOwningStateMachine(AnimatorState config)
	{
		return (AnimatorStateMachine)MemberRefs.m_StateMachineProxyLookup.GetMember().GetValue(FindNode(config).Node());
	}

	public static AnimatorStateMachine GetOwningStateMachine(AnimatorStateMachine spec)
	{
		return (AnimatorStateMachine)MemberRefs.m_StateMachineLookup.GetMember().GetValue(FindNode(spec).Node());
	}

	public static AnimatorStateMachine GetOwningStateMachine(GraphNodeRef spec)
	{
		return spec.nodeType switch
		{
			GraphNodeRef.NodeType.any => GraphAccessors.RootStateMachine(), 
			GraphNodeRef.NodeType.entry => (AnimatorStateMachine)MemberRefs.m_StateMachine.GetMember().GetValue(spec.Node()), 
			GraphNodeRef.NodeType.state => GetOwningStateMachine(spec.state), 
			GraphNodeRef.NodeType.machine => GetOwningStateMachine(spec.stateMachine), 
			_ => null, 
		};
	}
}
