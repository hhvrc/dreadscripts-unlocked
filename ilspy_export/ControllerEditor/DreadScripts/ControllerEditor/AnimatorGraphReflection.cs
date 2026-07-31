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
		public static readonly TypeResolver contextTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver recordTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver helperTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver consumerTests = new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver _AdapterTests = new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool+BreadCrumbElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver interpreterTests = new TypeResolver("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver _WatcherTests = new TypeResolver("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver _CandidateTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver m_ProductTests = new TypeResolver("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver m_ExpressionTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateMachineNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver _SystemTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EntryNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver _WorkerTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.AnyStateNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver m_FilterTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.ExitNode, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver m_StubTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeInfo, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

		public static readonly TypeResolver readerTests = new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.TransitionEditionContext, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}

	internal static class MemberRefs
	{
		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _StrategyTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "tool");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> customerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "stateMachineGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DatabaseTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "blendTreeGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ExporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "stateMachineGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_IdentifierTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "blendTreeGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attrTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "m_BreadCrumbs");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DispatcherTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.consumerTests, "graphDirtyCallback");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _RegistryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._AdapterTests, "m_Target");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_TagTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "m_ActiveStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ImporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "rootStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_RequestTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "parentStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> printerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._WatcherTests, "selection");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_WriterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.interpreterTests, "nodes");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ParamsTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.interpreterTests, "edges");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ListenerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "m_EntryNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> getterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "m_ExitNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InterceptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.contextTests, "m_AnyStateNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _CreatorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._CandidateTests, "state");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> eventTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._SystemTests, "m_StateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InfoTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.m_ExpressionTests, "stateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FacadeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._CandidateTests, "m_StateMachineProxyLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> advisorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers._CandidateTests, "m_StateMachineLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> callbackTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.m_StubTests, "transitions");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_IndexerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.contextTests, "GetEdgeInfo");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _IssuerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.contextTests, "FindNode", typeof(AnimatorState));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _PrototypeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.contextTests, "FindNode", typeof(AnimatorStateMachine));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_RuleTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(TypeResolvers.m_StubTests, "HasTransition", typeof(AnimatorTransitionBase));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> _SingletonTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.consumerTests, "animatorController");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> factoryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.recordTests, "edgeGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_AccountTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.m_StubTests, "hasMultipleTransitions");
	}

	internal static class GraphAccessors
	{
		private static EditorWindow _StatusTests;

		[SpecialName]
		public static EditorWindow PopPolicy()
		{
			if (!(_StatusTests != null))
			{
				return _StatusTests = (EditorWindow)MemberRefs._StrategyTests.PrepareRecord().GetValue(null);
			}
			return _StatusTests;
		}

		[SpecialName]
		public static void ComputePolicy(EditorWindow info)
		{
			MemberRefs._StrategyTests.PrepareRecord().SetValue(null, info);
		}

		[SpecialName]
		public static AnimatorController ConcatPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (AnimatorController)MemberRefs._SingletonTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph CancelPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.customerTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph DisablePolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)MemberRefs.m_DatabaseTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static GraphGUI RestartPolicy()
		{
			if (!(PopPolicy() == null))
			{
				return (GraphGUI)MemberRefs.m_ExporterTests.PrepareRecord().GetValue(PopPolicy());
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
			return (GraphGUI)MemberRefs.m_IdentifierTests.PrepareRecord().GetValue(PopPolicy());
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
				return (IEdgeGUI)MemberRefs.factoryTests.PrepareRecord().GetValue(RestartPolicy());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine ReadPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)MemberRefs.m_TagTests.PrepareRecord().GetValue(CancelPolicy());
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
			return (AnimatorStateMachine)MemberRefs._ImporterTests.PrepareRecord().GetValue(CancelPolicy());
		}

		[SpecialName]
		public static AnimatorStateMachine AwakePolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)MemberRefs.m_RequestTests.PrepareRecord().GetValue(CancelPolicy());
			}
			return null;
		}

		[SpecialName]
		public static GraphNodeRef FlushPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_ListenerTests.PrepareRecord().GetValue(CancelPolicy()));
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
			return new GraphNodeRef((Node)MemberRefs.getterTests.PrepareRecord().GetValue(CancelPolicy()));
		}

		[SpecialName]
		public static GraphNodeRef MapPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new GraphNodeRef((Node)MemberRefs.m_InterceptorTests.PrepareRecord().GetValue(CancelPolicy()));
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
			return from Node n in (IList)MemberRefs.m_WriterTests.PrepareRecord().GetValue(FindPolicy())
				select new GraphNodeRef(n);
		}

		[SpecialName]
		public static IEnumerable<GraphEdgeRef> DestroyPolicy()
		{
			if (FindPolicy() == null)
			{
				return null;
			}
			return from Edge e in (IList)MemberRefs._ParamsTests.PrepareRecord().GetValue(FindPolicy())
				select new GraphEdgeRef(e);
		}

		[SpecialName]
		public static IEnumerable<GraphNodeRef> CalcPolicy()
		{
			if (InitPolicy() == null)
			{
				return null;
			}
			return from Node n in (IList)MemberRefs.printerTests.PrepareRecord().GetValue(InitPolicy())
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
			return (Action)MemberRefs.m_DispatcherTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static void ReflectPolicy(Action value)
		{
			MemberRefs.m_DispatcherTests.PrepareRecord().SetValue(PopPolicy(), value);
		}

		[SpecialName]
		public static bool CreatePolicy()
		{
			IList list = SetupPolicy();
			if (list.Count == 0)
			{
				return false;
			}
			return !(MemberRefs._RegistryTests.PrepareRecord().GetValue(list[list.Count - 1]) is AnimatorStateMachine);
		}

		public static IList SetupPolicy()
		{
			return (IList)MemberRefs.attrTests.PrepareRecord().GetValue(PopPolicy());
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
				array[i] = (UnityEngine.Object)MemberRefs._RegistryTests.PrepareRecord().GetValue(list[i]);
			}
			return array;
		}

		public static Dictionary<AnimatorTransitionBase, GraphEdgeRef> PublishPolicy()
		{
			Dictionary<AnimatorTransitionBase, GraphEdgeRef> dictionary = new Dictionary<AnimatorTransitionBase, GraphEdgeRef>();
			foreach (GraphEdgeRef item in DestroyPolicy())
			{
				foreach (TransitionEditionInfo item2 in item.FindSerializer())
				{
					dictionary[item2.itemTests] = item;
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

		public bool _DecoratorTests;

		public readonly AnimatorState comparatorTests;

		public readonly AnimatorStateMachine m_ExceptionTests;

		public readonly NodeType objectTests;

		public bool _UtilsTests;

		private AnimatorStateMachine _ValTests;

		private Node valueTests;

		[SpecialName]
		public AnimatorStateMachine ListPolicy()
		{
			if (!_UtilsTests)
			{
				_UtilsTests = true;
				_ValTests = PostPolicy(this);
			}
			return _ValTests;
		}

		[SpecialName]
		public Node FillPolicy()
		{
			if (_DecoratorTests || valueTests != null)
			{
				return valueTests;
			}
			_DecoratorTests = true;
			switch (objectTests)
			{
			case NodeType.any:
				valueTests = GraphAccessors.MapPolicy().FillPolicy();
				break;
			case NodeType.exit:
				valueTests = GraphAccessors.CalculatePolicy().FillPolicy();
				break;
			default:
				valueTests = OrderPolicy(m_ExceptionTests).FillPolicy();
				break;
			case NodeType.entry:
				valueTests = GraphAccessors.FlushPolicy().FillPolicy();
				break;
			case NodeType.state:
				valueTests = RevertThread(comparatorTests).FillPolicy();
				break;
			case NodeType.tree:
				break;
			}
			return valueTests;
		}

		[SpecialName]
		public IEnumerable<GraphSlotRef> ForgotPolicy()
		{
			return FillPolicy().slots.Select((Slot s) => new GraphSlotRef(s));
		}

		[SpecialName]
		public IEnumerable<GraphEdgeRef> CheckPolicy()
		{
			return FillPolicy().inputEdges.Select((Edge e) => new GraphEdgeRef(e));
		}

		[SpecialName]
		public IEnumerable<GraphEdgeRef> AssetPolicy()
		{
			return FillPolicy().outputEdges.Select((Edge e) => new GraphEdgeRef(e));
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> ChangePolicy()
		{
			return CheckPolicy().SelectMany((GraphEdgeRef e) => from t in e.FindSerializer()
				select t.itemTests);
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> RegisterPolicy()
		{
			return AssetPolicy().SelectMany((GraphEdgeRef e) => from t in e.FindSerializer()
				select t.itemTests);
		}

		[SpecialName]
		public Styles.Color PatchPolicy()
		{
			return FillPolicy().color;
		}

		[SpecialName]
		public void InterruptPolicy(Styles.Color ident)
		{
			FillPolicy().color = ident;
		}

		[SpecialName]
		public Rect PrintPolicy()
		{
			return FillPolicy().position;
		}

		[SpecialName]
		public void SearchPolicy(Rect last)
		{
			FillPolicy().position = last;
		}

		public GraphNodeRef()
		{
		}

		public GraphNodeRef(Node init)
		{
			_DecoratorTests = true;
			valueTests = init;
			if (init == null)
			{
				return;
			}
			Type type = init.GetType();
			if (type == TypeResolvers._CandidateTests.ChangeRecord())
			{
				objectTests = NodeType.state;
				comparatorTests = (AnimatorState)MemberRefs._CreatorTests.PrepareRecord().GetValue(init);
			}
			else if (!(type == TypeResolvers.m_ExpressionTests.ChangeRecord()))
			{
				if (type == TypeResolvers._SystemTests.ChangeRecord())
				{
					objectTests = NodeType.entry;
				}
				else if (type == TypeResolvers.m_FilterTests.ChangeRecord())
				{
					objectTests = NodeType.exit;
				}
				else if (!(type == TypeResolvers._WorkerTests.ChangeRecord()))
				{
					if (type == TypeResolvers.m_ProductTests.ChangeRecord())
					{
						objectTests = NodeType.tree;
					}
				}
				else
				{
					objectTests = NodeType.any;
				}
			}
			else
			{
				objectTests = NodeType.machine;
				m_ExceptionTests = (AnimatorStateMachine)MemberRefs.m_InfoTests.PrepareRecord().GetValue(init);
			}
		}

		public GraphNodeRef(AnimatorState task)
		{
			comparatorTests = task;
			objectTests = NodeType.state;
		}

		public GraphNodeRef(AnimatorStateMachine item)
		{
			m_ExceptionTests = item;
			objectTests = NodeType.machine;
		}

		public static implicit operator Node(GraphNodeRef reference)
		{
			return reference.FillPolicy();
		}
	}

	internal class GraphEdgeRef
	{
		public readonly Edge m_ContainerTests;

		private bool m_ClassTests;

		private object _MockTests;

		[SpecialName]
		public GraphNodeRef ConcatSerializer()
		{
			return new GraphNodeRef(m_ContainerTests.fromSlot.node);
		}

		[SpecialName]
		public GraphNodeRef CancelSerializer()
		{
			return new GraphNodeRef(m_ContainerTests.toSlot.node);
		}

		[SpecialName]
		public GraphSlotRef DisableSerializer()
		{
			return new GraphSlotRef(m_ContainerTests.fromSlot);
		}

		[SpecialName]
		public GraphSlotRef RestartSerializer()
		{
			return new GraphSlotRef(m_ContainerTests.toSlot);
		}

		[SpecialName]
		private object AddSerializer()
		{
			if (!m_ClassTests)
			{
				m_ClassTests = true;
				_MockTests = MemberRefs.m_IndexerTests.PrepareRecord().Invoke(GraphAccessors.CancelPolicy(), new object[1] { m_ContainerTests });
				return _MockTests;
			}
			return _MockTests;
		}

		public bool PopSerializer(AnimatorTransitionBase ident)
		{
			return (bool)MemberRefs.m_RuleTests.PrepareRecord().Invoke(AddSerializer(), new object[1] { ident });
		}

		public bool ComputeSerializer()
		{
			return (bool)MemberRefs.m_AccountTests.PrepareRecord().GetValue(AddSerializer());
		}

		[SpecialName]
		public IEnumerable<TransitionEditionInfo> FindSerializer()
		{
			return from object setup in (IList)MemberRefs.callbackTests.PrepareRecord().GetValue(AddSerializer())
				select new TransitionEditionInfo(setup, this);
		}

		public GraphEdgeRef(Edge value)
		{
			m_ContainerTests = value;
		}

		public static implicit operator Edge(GraphEdgeRef task)
		{
			return task.m_ContainerTests;
		}

		[CompilerGenerated]
		private TransitionEditionInfo MoveSerializer(object setup)
		{
			return new TransitionEditionInfo(setup, this);
		}
	}

	internal readonly struct TransitionEditionInfo
	{
		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FieldTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "m_DisplayName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attributeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "m_FullName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ClientTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "sourceState");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ConfigTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "sourceStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> descriptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "ownerStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _TemplateTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(TypeResolvers.readerTests, "transition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_MessageTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.readerTests, "isAnyStateTransition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_CollectionTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(TypeResolvers.readerTests, "isDefaultTransition");

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
			return (string)m_FieldTests.PrepareRecord().GetValue(parserTests);
		}

		[SpecialName]
		public string StartSerializer()
		{
			return (string)attributeTests.PrepareRecord().GetValue(parserTests);
		}

		public TransitionEditionInfo(object config, GraphEdgeRef second)
		{
			parserTests = config;
			_ManagerTests = second;
			taskTests = (bool)m_MessageTests.PrepareRecord().GetValue(config);
			m_ProcessTests = (bool)m_MessageTests.PrepareRecord().GetValue(config);
			itemTests = (AnimatorTransitionBase)_TemplateTests.PrepareRecord().GetValue(config);
			bool flag = itemTests != null;
			_StructTests = ((!flag) ? null : itemTests.destinationState);
			_ServiceTests = ((!flag) ? null : itemTests.destinationStateMachine);
			proxyTests = (AnimatorStateMachine)descriptorTests.PrepareRecord().GetValue(config);
			m_SchemaTests = (AnimatorState)m_ClientTests.PrepareRecord().GetValue(config);
			broadcasterTests = (AnimatorStateMachine)((!taskTests && !m_ProcessTests) ? m_ConfigTests.PrepareRecord().GetValue(config) : proxyTests);
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

	public static GraphNodeRef RevertThread(AnimatorState i)
	{
		return new GraphNodeRef((Node)MemberRefs._IssuerTests.PrepareRecord().Invoke(GraphAccessors.CancelPolicy(), new object[1] { i }));
	}

	public static GraphNodeRef OrderPolicy(AnimatorStateMachine last)
	{
		return new GraphNodeRef((Node)MemberRefs._PrototypeTests.PrepareRecord().Invoke(GraphAccessors.CancelPolicy(), new object[1] { last }));
	}

	public static AnimatorStateMachine ComparePolicy(AnimatorState config)
	{
		return (AnimatorStateMachine)MemberRefs.m_FacadeTests.PrepareRecord().GetValue(RevertThread(config).FillPolicy());
	}

	public static AnimatorStateMachine SetPolicy(AnimatorStateMachine spec)
	{
		return (AnimatorStateMachine)MemberRefs.advisorTests.PrepareRecord().GetValue(OrderPolicy(spec).FillPolicy());
	}

	public static AnimatorStateMachine PostPolicy(GraphNodeRef spec)
	{
		return spec.objectTests switch
		{
			GraphNodeRef.NodeType.any => GraphAccessors.RemovePolicy(), 
			GraphNodeRef.NodeType.entry => (AnimatorStateMachine)MemberRefs.eventTests.PrepareRecord().GetValue(spec.FillPolicy()), 
			GraphNodeRef.NodeType.state => ComparePolicy(spec.comparatorTests), 
			GraphNodeRef.NodeType.machine => SetPolicy(spec.m_ExceptionTests), 
			_ => null, 
		};
	}
}
