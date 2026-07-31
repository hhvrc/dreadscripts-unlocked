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
		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _StrategyTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "tool");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> customerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DatabaseTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ExporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "stateMachineGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_IdentifierTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "blendTreeGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attrTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "m_BreadCrumbs");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DispatcherTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.animatorControllerTool, "graphDirtyCallback");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _RegistryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.breadCrumbElement, "m_Target");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_TagTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ActiveStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ImporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "rootStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_RequestTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "parentStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> printerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graphGUI, "selection");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_WriterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "nodes");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ParamsTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.graph, "edges");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ListenerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_EntryNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> getterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_ExitNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InterceptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineGraph, "m_AnyStateNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _CreatorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "state");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> eventTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.entryNode, "m_StateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InfoTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateMachineNode, "stateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FacadeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineProxyLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> advisorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.stateNode, "m_StateMachineLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> callbackTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.edgeInfo, "transitions");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_IndexerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "GetEdgeInfo");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _IssuerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorState));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _PrototypeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.stateMachineGraph, "FindNode", typeof(AnimatorStateMachine));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_RuleTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.edgeInfo, "HasTransition", typeof(AnimatorTransitionBase));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> _SingletonTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.animatorControllerTool, "animatorController");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> factoryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.stateMachineGraphGUI, "edgeGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_AccountTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.edgeInfo, "hasMultipleTransitions");
	}

	internal static class GraphAccessors
	{
		private static EditorWindow _StatusTests;

		[SpecialName]
		public static EditorWindow PopPolicy()
		{
			if (!(_StatusTests != null))
			{
				return _StatusTests = (EditorWindow)MemberRefs._StrategyTests.GetMember().GetValue(null);
			}
			return _StatusTests;
		}

		[SpecialName]
		public static void ComputePolicy(EditorWindow info)
		{
			MemberRefs._StrategyTests.GetMember().SetValue(null, info);
		}

		[SpecialName]
		public static AnimatorController ConcatPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (AnimatorController)MemberRefs._SingletonTests.GetMember().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph CancelPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.customerTests.GetMember().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph DisablePolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.m_DatabaseTests.GetMember().GetValue(PopPolicy());
		}

		[SpecialName]
		public static GraphGUI RestartPolicy()
		{
			if (!(PopPolicy() == null))
			{
				return (GraphGUI)MemberRefs.m_ExporterTests.GetMember().GetValue(PopPolicy());
			}
			return null;
		}

		[SpecialName]
		public static GraphGUI AddPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (GraphGUI)MemberRefs.m_IdentifierTests.GetMember().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph FindPolicy()
		{
			if (!(PopPolicy() == null))
			{
				if (CreatePolicy())
				{
					return DisablePolicy();
				}
				return CancelPolicy();
			}
			return null;
		}

		[SpecialName]
		public static GraphGUI InitPolicy()
		{
			if (!(PopPolicy() == null))
			{
				if (CreatePolicy())
				{
					return AddPolicy();
				}
				return RestartPolicy();
			}
			return null;
		}

		[SpecialName]
		public static IEdgeGUI DefinePolicy()
		{
			if (!(RestartPolicy() == null))
			{
				return (IEdgeGUI)MemberRefs.factoryTests.GetMember().GetValue(RestartPolicy());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine ReadPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)MemberRefs.m_TagTests.GetMember().GetValue(CancelPolicy());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine RemovePolicy()
		{
			if (CancelPolicy() == null)
			{
				return null;
			}
			return (AnimatorStateMachine)MemberRefs._ImporterTests.GetMember().GetValue(CancelPolicy());
		}

		[SpecialName]
		public static AnimatorStateMachine AwakePolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)MemberRefs.m_RequestTests.GetMember().GetValue(CancelPolicy());
			}
			return null;
		}

		[SpecialName]
		public static GraphNodeRef FlushPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_ListenerTests.GetMember().GetValue(CancelPolicy()));
			}
			return null;
		}

		[SpecialName]
		public static GraphNodeRef CalculatePolicy()
		{
			if (CancelPolicy() == null)
			{
				return null;
			}
			return new GraphNodeRef((Node)MemberRefs.getterTests.GetMember().GetValue(CancelPolicy()));
		}

		[SpecialName]
		public static GraphNodeRef MapPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_InterceptorTests.GetMember().GetValue(CancelPolicy()));
			}
			return null;
		}

		[SpecialName]
		public static IEnumerable<GraphNodeRef> CustomizePolicy()
		{
			if (FindPolicy() == null)
			{
				return null;
			}
			return from Node n in (IList)MemberRefs.m_WriterTests.GetMember().GetValue(FindPolicy())
				select new GraphNodeRef(n);
		}

		[SpecialName]
		public static IEnumerable<GraphEdgeRef> DestroyPolicy()
		{
			if (FindPolicy() == null)
			{
				return null;
			}
			return from Edge e in (IList)MemberRefs._ParamsTests.GetMember().GetValue(FindPolicy())
				select new GraphEdgeRef(e);
		}

		[SpecialName]
		public static IEnumerable<GraphNodeRef> CalcPolicy()
		{
			if (InitPolicy() == null)
			{
				return null;
			}
			return from Node n in (IList)MemberRefs.printerTests.GetMember().GetValue(InitPolicy())
				select new GraphNodeRef(n);
		}

		[SpecialName]
		public static ConcurrentBag<GraphEdgeRef> RunPolicy()
		{
			if (CancelPolicy() == null || DefinePolicy() == null)
			{
				return null;
			}
			GraphEdgeRef[] _ModelTests = DestroyPolicy().ToArray();
			HashSet<int> roleTests = new HashSet<int>();
			foreach (int item in DefinePolicy().edgeSelection)
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
		public static Action LoginPolicy()
		{
			return (Action)MemberRefs.m_DispatcherTests.GetMember().GetValue(PopPolicy());
		}

		[SpecialName]
		public static void ReflectPolicy(Action value)
		{
			MemberRefs.m_DispatcherTests.GetMember().SetValue(PopPolicy(), value);
		}

		[SpecialName]
		public static bool CreatePolicy()
		{
			IList list = SetupPolicy();
			if (list.Count == 0)
			{
				return false;
			}
			return !(MemberRefs._RegistryTests.GetMember().GetValue(list[list.Count - 1]) is AnimatorStateMachine);
		}

		public static IList SetupPolicy()
		{
			return (IList)MemberRefs.attrTests.GetMember().GetValue(PopPolicy());
		}

		public static UnityEngine.Object[] EnablePolicy()
		{
			IList list = SetupPolicy();
			if (list.Count == 0)
			{
				return Array.Empty<UnityEngine.Object>();
			}
			UnityEngine.Object[] array = new UnityEngine.Object[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = (UnityEngine.Object)MemberRefs._RegistryTests.GetMember().GetValue(list[i]);
			}
			return array;
		}

		public static Dictionary<AnimatorTransitionBase, GraphEdgeRef> PublishPolicy()
		{
			Dictionary<AnimatorTransitionBase, GraphEdgeRef> dictionary = new Dictionary<AnimatorTransitionBase, GraphEdgeRef>();
			foreach (GraphEdgeRef item in DestroyPolicy())
			{
				foreach (TransitionEditionInfo transition in item.GetTransitions())
				{
					dictionary[transition.itemTests] = item;
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
				cachedNode = GraphAccessors.MapPolicy().Node();
				break;
			case NodeType.exit:
				cachedNode = GraphAccessors.CalculatePolicy().Node();
				break;
			default:
				cachedNode = FindNode(stateMachine).Node();
				break;
			case NodeType.entry:
				cachedNode = GraphAccessors.FlushPolicy().Node();
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
				select t.itemTests);
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> OutgoingTransitions()
		{
			return OutputEdges().SelectMany((GraphEdgeRef e) => from t in e.GetTransitions()
				select t.itemTests);
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
				state = (AnimatorState)MemberRefs._CreatorTests.GetMember().GetValue(init);
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
				stateMachine = (AnimatorStateMachine)MemberRefs.m_InfoTests.GetMember().GetValue(init);
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
				edgeInfo = MemberRefs.m_IndexerTests.GetMember().Invoke(GraphAccessors.CancelPolicy(), new object[1] { edge });
				return edgeInfo;
			}
			return edgeInfo;
		}

		public bool HasTransition(AnimatorTransitionBase ident)
		{
			return (bool)MemberRefs.m_RuleTests.GetMember().Invoke(GetEdgeInfo(), new object[1] { ident });
		}

		public bool HasMultipleTransitions()
		{
			return (bool)MemberRefs.m_AccountTests.GetMember().GetValue(GetEdgeInfo());
		}

		[SpecialName]
		public IEnumerable<TransitionEditionInfo> GetTransitions()
		{
			return from object setup in (IList)MemberRefs.callbackTests.GetMember().GetValue(GetEdgeInfo())
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
		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FieldTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_DisplayName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attributeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "m_FullName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ClientTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceState");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ConfigTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "sourceStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> descriptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "ownerStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _TemplateTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.transitionEditionContext, "transition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_MessageTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isAnyStateTransition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_CollectionTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.transitionEditionContext, "isDefaultTransition");

		public readonly object parserTests;

		public readonly GraphEdgeRef _ManagerTests;

		public readonly AnimatorTransitionBase itemTests;

		public readonly AnimatorStateTransition specificationTests;

		public readonly AnimatorTransition m_MethodTests;

		public readonly AnimatorState m_SchemaTests;

		public readonly AnimatorStateMachine broadcasterTests;

		public readonly AnimatorStateMachine proxyTests;

		public readonly AnimatorState _StructTests;

		public readonly AnimatorStateMachine _ServiceTests;

		public readonly GraphNodeRef.NodeType stateTests;

		public readonly GraphNodeRef.NodeType globalTests;

		public readonly bool taskTests;

		public readonly bool m_ProcessTests;

		public readonly bool _ProducerTests;

		public readonly bool m_IteratorTests;

		internal static object LogoutStruct;

		[SpecialName]
		public string VisitSerializer()
		{
			return (string)m_FieldTests.GetMember().GetValue(parserTests);
		}

		[SpecialName]
		public string StartSerializer()
		{
			return (string)attributeTests.GetMember().GetValue(parserTests);
		}

		public TransitionEditionInfo(object config, GraphEdgeRef second)
		{
			parserTests = config;
			_ManagerTests = second;
			taskTests = (bool)m_MessageTests.GetMember().GetValue(config);
			m_ProcessTests = (bool)m_MessageTests.GetMember().GetValue(config);
			itemTests = (AnimatorTransitionBase)_TemplateTests.GetMember().GetValue(config);
			bool flag = itemTests != null;
			_StructTests = ((!flag) ? null : itemTests.destinationState);
			_ServiceTests = ((!flag) ? null : itemTests.destinationStateMachine);
			proxyTests = (AnimatorStateMachine)descriptorTests.GetMember().GetValue(config);
			m_SchemaTests = (AnimatorState)m_ClientTests.GetMember().GetValue(config);
			broadcasterTests = (AnimatorStateMachine)((!taskTests && !m_ProcessTests) ? m_ConfigTests.GetMember().GetValue(config) : proxyTests);
			globalTests = ((m_SchemaTests != null) ? GraphNodeRef.NodeType.state : ((!(broadcasterTests == null)) ? ((!taskTests) ? ((!m_ProcessTests && !(proxyTests != null)) ? GraphNodeRef.NodeType.machine : GraphNodeRef.NodeType.entry) : GraphNodeRef.NodeType.any) : GraphNodeRef.NodeType.unknown));
			stateTests = ((_StructTests != null) ? GraphNodeRef.NodeType.state : ((_ServiceTests != null) ? GraphNodeRef.NodeType.machine : GraphNodeRef.NodeType.exit));
			_ProducerTests = globalTests == GraphNodeRef.NodeType.entry && !m_ProcessTests;
			m_IteratorTests = stateTests == GraphNodeRef.NodeType.exit;
			specificationTests = ((!taskTests && globalTests != GraphNodeRef.NodeType.state) ? null : ((AnimatorStateTransition)itemTests));
			m_MethodTests = ((!_ProducerTests && globalTests != GraphNodeRef.NodeType.machine) ? null : ((AnimatorTransition)itemTests));
		}

		public void InitSerializer()
		{
			switch (globalTests)
			{
			case GraphNodeRef.NodeType.any:
				broadcasterTests.RemoveAnyStateTransition(specificationTests);
				break;
			case GraphNodeRef.NodeType.state:
				m_SchemaTests.RemoveTransition(specificationTests);
				break;
			case GraphNodeRef.NodeType.machine:
				proxyTests.RemoveStateMachineTransition(broadcasterTests, m_MethodTests);
				break;
			case GraphNodeRef.NodeType.entry:
				if (!m_ProcessTests)
				{
					broadcasterTests.RemoveEntryTransition(m_MethodTests);
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
		return new GraphNodeRef((Node)MemberRefs._IssuerTests.GetMember().Invoke(GraphAccessors.CancelPolicy(), new object[1] { i }));
	}

	public static GraphNodeRef FindNode(AnimatorStateMachine last)
	{
		return new GraphNodeRef((Node)MemberRefs._PrototypeTests.GetMember().Invoke(GraphAccessors.CancelPolicy(), new object[1] { last }));
	}

	public static AnimatorStateMachine GetOwningStateMachine(AnimatorState config)
	{
		return (AnimatorStateMachine)MemberRefs.m_FacadeTests.GetMember().GetValue(FindNode(config).Node());
	}

	public static AnimatorStateMachine GetOwningStateMachine(AnimatorStateMachine spec)
	{
		return (AnimatorStateMachine)MemberRefs.advisorTests.GetMember().GetValue(FindNode(spec).Node());
	}

	public static AnimatorStateMachine GetOwningStateMachine(GraphNodeRef spec)
	{
		return spec.nodeType switch
		{
			GraphNodeRef.NodeType.any => GraphAccessors.RemovePolicy(), 
			GraphNodeRef.NodeType.entry => (AnimatorStateMachine)MemberRefs.eventTests.GetMember().GetValue(spec.Node()), 
			GraphNodeRef.NodeType.state => GetOwningStateMachine(spec.state), 
			GraphNodeRef.NodeType.machine => GetOwningStateMachine(spec.stateMachine), 
			_ => null, 
		};
	}
}
