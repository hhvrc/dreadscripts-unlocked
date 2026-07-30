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

internal static class SetterTests
{
	internal static class ConnectionTests
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

		private static ConnectionTests RunStruct;

		internal static bool ComputeStruct()
		{
			return RunStruct == null;
		}
	}

	internal static class BridgeTests
	{
		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _StrategyTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "tool");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> customerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "stateMachineGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DatabaseTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "blendTreeGraph");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ExporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "stateMachineGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_IdentifierTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "blendTreeGraphGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attrTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "m_BreadCrumbs");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_DispatcherTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.consumerTests, "graphDirtyCallback");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _RegistryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._AdapterTests, "m_Target");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_TagTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "m_ActiveStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ImporterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "rootStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_RequestTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "parentStateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> printerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._WatcherTests, "selection");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_WriterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.interpreterTests, "nodes");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _ParamsTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.interpreterTests, "edges");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ListenerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "m_EntryNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> getterTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "m_ExitNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InterceptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.contextTests, "m_AnyStateNode");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _CreatorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._CandidateTests, "state");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> eventTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._SystemTests, "m_StateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_InfoTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.m_ExpressionTests, "stateMachine");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FacadeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._CandidateTests, "m_StateMachineProxyLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> advisorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests._CandidateTests, "m_StateMachineLookup");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> callbackTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.m_StubTests, "transitions");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_IndexerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(ConnectionTests.contextTests, "GetEdgeInfo");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _IssuerTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(ConnectionTests.contextTests, "FindNode", typeof(AnimatorState));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> _PrototypeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(ConnectionTests.contextTests, "FindNode", typeof(AnimatorStateMachine));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo> m_RuleTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<MethodInfo>(ConnectionTests.m_StubTests, "HasTransition", typeof(AnimatorTransitionBase));

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> _SingletonTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(ConnectionTests.consumerTests, "animatorController");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> factoryTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(ConnectionTests.recordTests, "edgeGUI");

		public static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_AccountTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(ConnectionTests.m_StubTests, "hasMultipleTransitions");

		private static BridgeTests ConnectStruct;

		internal static bool ViewStruct()
		{
			return ConnectStruct == null;
		}
	}

	internal static class RefTests
	{
		private static EditorWindow _StatusTests;

		private static RefTests ChangeStruct;

		[SpecialName]
		public static EditorWindow PopPolicy()
		{
			if (!(_StatusTests != null))
			{
				return _StatusTests = (EditorWindow)BridgeTests._StrategyTests.PrepareRecord().GetValue(null);
			}
			return _StatusTests;
		}

		[SpecialName]
		public static void ComputePolicy(EditorWindow info)
		{
			BridgeTests._StrategyTests.PrepareRecord().SetValue(null, info);
		}

		[SpecialName]
		public static AnimatorController ConcatPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (AnimatorController)BridgeTests._SingletonTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph CancelPolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)BridgeTests.customerTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static Graph DisablePolicy()
		{
			if (PopPolicy() == null)
			{
				return null;
			}
			return (Graph)BridgeTests.m_DatabaseTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static GraphGUI RestartPolicy()
		{
			if (!(PopPolicy() == null))
			{
				return (GraphGUI)BridgeTests.m_ExporterTests.PrepareRecord().GetValue(PopPolicy());
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
			return (GraphGUI)BridgeTests.m_IdentifierTests.PrepareRecord().GetValue(PopPolicy());
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
				return (IEdgeGUI)BridgeTests.factoryTests.PrepareRecord().GetValue(RestartPolicy());
			}
			return null;
		}

		[SpecialName]
		public static AnimatorStateMachine ReadPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)BridgeTests.m_TagTests.PrepareRecord().GetValue(CancelPolicy());
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
			return (AnimatorStateMachine)BridgeTests._ImporterTests.PrepareRecord().GetValue(CancelPolicy());
		}

		[SpecialName]
		public static AnimatorStateMachine AwakePolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return (AnimatorStateMachine)BridgeTests.m_RequestTests.PrepareRecord().GetValue(CancelPolicy());
			}
			return null;
		}

		[SpecialName]
		public static TokenizerTests FlushPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new TokenizerTests((Node)BridgeTests.m_ListenerTests.PrepareRecord().GetValue(CancelPolicy()));
			}
			return null;
		}

		[SpecialName]
		public static TokenizerTests CalculatePolicy()
		{
			if (CancelPolicy() == null)
			{
				return null;
			}
			return new TokenizerTests((Node)BridgeTests.getterTests.PrepareRecord().GetValue(CancelPolicy()));
		}

		[SpecialName]
		public static TokenizerTests MapPolicy()
		{
			if (!(CancelPolicy() == null))
			{
				return new TokenizerTests((Node)BridgeTests.m_InterceptorTests.PrepareRecord().GetValue(CancelPolicy()));
			}
			return null;
		}

		[SpecialName]
		public static IEnumerable<TokenizerTests> CustomizePolicy()
		{
			if (FindPolicy() == null)
			{
				return null;
			}
			return from Node n in (IList)BridgeTests.m_WriterTests.PrepareRecord().GetValue(FindPolicy())
				select new TokenizerTests(n);
		}

		[SpecialName]
		public static IEnumerable<BaseTests> DestroyPolicy()
		{
			if (FindPolicy() == null)
			{
				return null;
			}
			return from Edge e in (IList)BridgeTests._ParamsTests.PrepareRecord().GetValue(FindPolicy())
				select new BaseTests(e);
		}

		[SpecialName]
		public static IEnumerable<TokenizerTests> CalcPolicy()
		{
			if (InitPolicy() == null)
			{
				return null;
			}
			return from Node n in (IList)BridgeTests.printerTests.PrepareRecord().GetValue(InitPolicy())
				select new TokenizerTests(n);
		}

		[SpecialName]
		public static ConcurrentBag<BaseTests> RunPolicy()
		{
			if (CancelPolicy() == null || DefinePolicy() == null)
			{
				return null;
			}
			BaseTests[] _ModelTests = DestroyPolicy().ToArray();
			HashSet<int> roleTests = new HashSet<int>();
			foreach (int item in DefinePolicy().edgeSelection)
			{
				roleTests.Add(item);
			}
			ConcurrentBag<BaseTests> _ParamTests = new ConcurrentBag<BaseTests>();
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
			return (Action)BridgeTests.m_DispatcherTests.PrepareRecord().GetValue(PopPolicy());
		}

		[SpecialName]
		public static void ReflectPolicy(Action value)
		{
			BridgeTests.m_DispatcherTests.PrepareRecord().SetValue(PopPolicy(), value);
		}

		[SpecialName]
		public static bool CreatePolicy()
		{
			IList list = SetupPolicy();
			if (list.Count == 0)
			{
				return false;
			}
			return !(BridgeTests._RegistryTests.PrepareRecord().GetValue(list[list.Count - 1]) is AnimatorStateMachine);
		}

		public static IList SetupPolicy()
		{
			return (IList)BridgeTests.attrTests.PrepareRecord().GetValue(PopPolicy());
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
				array[i] = (UnityEngine.Object)BridgeTests._RegistryTests.PrepareRecord().GetValue(list[i]);
			}
			return array;
		}

		public static Dictionary<AnimatorTransitionBase, BaseTests> PublishPolicy()
		{
			Dictionary<AnimatorTransitionBase, BaseTests> dictionary = new Dictionary<AnimatorTransitionBase, BaseTests>();
			foreach (BaseTests item in DestroyPolicy())
			{
				foreach (InstanceTests item2 in item.FindSerializer())
				{
					dictionary[item2.itemTests] = item;
				}
			}
			return dictionary;
		}

		internal static bool CalculateStruct()
		{
			return ChangeStruct == null;
		}
	}

	internal class TokenizerTests
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

		internal static TokenizerTests RateStruct;

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
				valueTests = RefTests.MapPolicy().FillPolicy();
				break;
			case NodeType.exit:
				valueTests = RefTests.CalculatePolicy().FillPolicy();
				break;
			case NodeType.machine:
				valueTests = OrderPolicy(m_ExceptionTests).FillPolicy();
				break;
			case NodeType.entry:
				valueTests = RefTests.FlushPolicy().FillPolicy();
				break;
			case NodeType.state:
				valueTests = RevertThread(comparatorTests).FillPolicy();
				break;
			}
			return valueTests;
		}

		[SpecialName]
		public IEnumerable<PublisherTests> ForgotPolicy()
		{
			return FillPolicy().slots.Select((Slot s) => new PublisherTests(s));
		}

		[SpecialName]
		public IEnumerable<BaseTests> CheckPolicy()
		{
			return FillPolicy().inputEdges.Select((Edge e) => new BaseTests(e));
		}

		[SpecialName]
		public IEnumerable<BaseTests> AssetPolicy()
		{
			return FillPolicy().outputEdges.Select((Edge e) => new BaseTests(e));
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> ChangePolicy()
		{
			return CheckPolicy().SelectMany((BaseTests e) => from t in e.FindSerializer()
				select t.itemTests);
		}

		[SpecialName]
		public IEnumerable<AnimatorTransitionBase> RegisterPolicy()
		{
			return AssetPolicy().SelectMany((BaseTests e) => from t in e.FindSerializer()
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

		public TokenizerTests()
		{
		}

		public TokenizerTests(Node init)
		{
			_DecoratorTests = true;
			valueTests = init;
			if (init == null)
			{
				return;
			}
			Type type = init.GetType();
			if (type == ConnectionTests._CandidateTests.ChangeRecord())
			{
				objectTests = NodeType.state;
				comparatorTests = (AnimatorState)BridgeTests._CreatorTests.PrepareRecord().GetValue(init);
			}
			else if (!(type == ConnectionTests.m_ExpressionTests.ChangeRecord()))
			{
				if (type == ConnectionTests._SystemTests.ChangeRecord())
				{
					objectTests = NodeType.entry;
				}
				else if (type == ConnectionTests.m_FilterTests.ChangeRecord())
				{
					objectTests = NodeType.exit;
				}
				else if (!(type == ConnectionTests._WorkerTests.ChangeRecord()))
				{
					if (type == ConnectionTests.m_ProductTests.ChangeRecord())
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
				m_ExceptionTests = (AnimatorStateMachine)BridgeTests.m_InfoTests.PrepareRecord().GetValue(init);
			}
		}

		public TokenizerTests(AnimatorState task)
		{
			comparatorTests = task;
			objectTests = NodeType.state;
		}

		public TokenizerTests(AnimatorStateMachine item)
		{
			m_ExceptionTests = item;
			objectTests = NodeType.machine;
		}

		public static implicit operator Node(TokenizerTests reference)
		{
			return reference.FillPolicy();
		}

		internal static bool PostStruct()
		{
			return RateStruct == null;
		}
	}

	internal class BaseTests
	{
		public readonly Edge m_ContainerTests;

		private bool m_ClassTests;

		private object _MockTests;

		private static BaseTests ConcatStruct;

		[SpecialName]
		public TokenizerTests ConcatSerializer()
		{
			return new TokenizerTests(m_ContainerTests.fromSlot.node);
		}

		[SpecialName]
		public TokenizerTests CancelSerializer()
		{
			return new TokenizerTests(m_ContainerTests.toSlot.node);
		}

		[SpecialName]
		public PublisherTests DisableSerializer()
		{
			return new PublisherTests(m_ContainerTests.fromSlot);
		}

		[SpecialName]
		public PublisherTests RestartSerializer()
		{
			return new PublisherTests(m_ContainerTests.toSlot);
		}

		[SpecialName]
		private object AddSerializer()
		{
			if (!m_ClassTests)
			{
				m_ClassTests = true;
				_MockTests = BridgeTests.m_IndexerTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1] { m_ContainerTests });
				return _MockTests;
			}
			return _MockTests;
		}

		public bool PopSerializer(AnimatorTransitionBase ident)
		{
			return (bool)BridgeTests.m_RuleTests.PrepareRecord().Invoke(AddSerializer(), new object[1] { ident });
		}

		public bool ComputeSerializer()
		{
			return (bool)BridgeTests.m_AccountTests.PrepareRecord().GetValue(AddSerializer());
		}

		[SpecialName]
		public IEnumerable<InstanceTests> FindSerializer()
		{
			return from object setup in (IList)BridgeTests.callbackTests.PrepareRecord().GetValue(AddSerializer())
				select new InstanceTests(setup, this);
		}

		public BaseTests(Edge value)
		{
			m_ContainerTests = value;
		}

		public static implicit operator Edge(BaseTests task)
		{
			return task.m_ContainerTests;
		}

		[CompilerGenerated]
		private InstanceTests MoveSerializer(object setup)
		{
			return new InstanceTests(setup, this);
		}

		internal static bool CollectStruct()
		{
			return ConcatStruct == null;
		}
	}

	internal readonly struct InstanceTests
	{
		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_FieldTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "m_DisplayName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> attributeTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "m_FullName");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ClientTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "sourceState");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> m_ConfigTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "sourceStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> descriptorTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "ownerStateMachine");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo> _TemplateTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<FieldInfo>(ConnectionTests.readerTests, "transition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_MessageTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(ConnectionTests.readerTests, "isAnyStateTransition");

		private static readonly DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo> m_CollectionTests = new DreadScripts.ControllerEditor.ReflectionMemberRef<PropertyInfo>(ConnectionTests.readerTests, "isDefaultTransition");

		public readonly object parserTests;

		public readonly BaseTests _ManagerTests;

		public readonly AnimatorTransitionBase itemTests;

		public readonly AnimatorStateTransition specificationTests;

		public readonly AnimatorTransition m_MethodTests;

		public readonly AnimatorState m_SchemaTests;

		public readonly AnimatorStateMachine broadcasterTests;

		public readonly AnimatorStateMachine proxyTests;

		public readonly AnimatorState _StructTests;

		public readonly AnimatorStateMachine _ServiceTests;

		public readonly TokenizerTests.NodeType stateTests;

		public readonly TokenizerTests.NodeType globalTests;

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

		public InstanceTests(object config, BaseTests second)
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
			globalTests = ((m_SchemaTests != null) ? TokenizerTests.NodeType.state : ((!(broadcasterTests == null)) ? ((!taskTests) ? ((!m_ProcessTests && !(proxyTests != null)) ? TokenizerTests.NodeType.machine : TokenizerTests.NodeType.entry) : TokenizerTests.NodeType.any) : TokenizerTests.NodeType.unknown));
			stateTests = ((_StructTests != null) ? TokenizerTests.NodeType.state : ((_ServiceTests != null) ? TokenizerTests.NodeType.machine : TokenizerTests.NodeType.exit));
			_ProducerTests = globalTests == TokenizerTests.NodeType.entry && !m_ProcessTests;
			m_IteratorTests = stateTests == TokenizerTests.NodeType.exit;
			specificationTests = ((!taskTests && globalTests != TokenizerTests.NodeType.state) ? null : ((AnimatorStateTransition)itemTests));
			m_MethodTests = ((!_ProducerTests && globalTests != TokenizerTests.NodeType.machine) ? null : ((AnimatorTransition)itemTests));
		}

		public void InitSerializer()
		{
			switch (globalTests)
			{
			case TokenizerTests.NodeType.any:
				broadcasterTests.RemoveAnyStateTransition(specificationTests);
				break;
			case TokenizerTests.NodeType.state:
				m_SchemaTests.RemoveTransition(specificationTests);
				break;
			case TokenizerTests.NodeType.machine:
				proxyTests.RemoveStateMachineTransition(broadcasterTests, m_MethodTests);
				break;
			case TokenizerTests.NodeType.entry:
				if (!m_ProcessTests)
				{
					broadcasterTests.RemoveEntryTransition(m_MethodTests);
				}
				break;
			case TokenizerTests.NodeType.tree:
			case TokenizerTests.NodeType.exit:
				break;
			}
		}

		internal static bool FindStruct()
		{
			return LogoutStruct == null;
		}
	}

	internal readonly struct PublisherTests
	{
		public readonly Slot m_ConfigurationTests;

		private static object TestStruct;

		[SpecialName]
		public Node SelectSerializer()
		{
			return m_ConfigurationTests.node;
		}

		[SpecialName]
		public List<BaseTests> InstantiateSerializer()
		{
			return new List<BaseTests>(m_ConfigurationTests.edges.Select((Edge e) => new BaseTests(e)));
		}

		public PublisherTests(Slot param)
		{
			m_ConfigurationTests = param;
		}

		internal static bool IncludeStruct()
		{
			return TestStruct == null;
		}
	}

	private static SetterTests FillStruct;

	public static TokenizerTests RevertThread(AnimatorState i)
	{
		return new TokenizerTests((Node)BridgeTests._IssuerTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1] { i }));
	}

	public static TokenizerTests OrderPolicy(AnimatorStateMachine last)
	{
		return new TokenizerTests((Node)BridgeTests._PrototypeTests.PrepareRecord().Invoke(RefTests.CancelPolicy(), new object[1] { last }));
	}

	public static AnimatorStateMachine ComparePolicy(AnimatorState config)
	{
		return (AnimatorStateMachine)BridgeTests.m_FacadeTests.PrepareRecord().GetValue(RevertThread(config).FillPolicy());
	}

	public static AnimatorStateMachine SetPolicy(AnimatorStateMachine spec)
	{
		return (AnimatorStateMachine)BridgeTests.advisorTests.PrepareRecord().GetValue(OrderPolicy(spec).FillPolicy());
	}

	public static AnimatorStateMachine PostPolicy(TokenizerTests spec)
	{
		return spec.objectTests switch
		{
			TokenizerTests.NodeType.any => RefTests.RemovePolicy(), 
			TokenizerTests.NodeType.entry => (AnimatorStateMachine)BridgeTests.eventTests.PrepareRecord().GetValue(spec.FillPolicy()), 
			TokenizerTests.NodeType.state => ComparePolicy(spec.comparatorTests), 
			TokenizerTests.NodeType.machine => SetPolicy(spec.m_ExceptionTests), 
			_ => null, 
		};
	}

	internal static bool DeleteStruct()
	{
		return FillStruct == null;
	}
}
