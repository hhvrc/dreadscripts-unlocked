using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DreadScripts.Common.SupportThankies;
using HarmonyLib;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.Compilation;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.ControllerEditor;

internal sealed class ControllerEditor : EditorWindow, IHasCustomMenu
{
	private struct MethodVisitor
	{
		internal readonly List<UnityEditor.Animations.BlendTree> _SchemaVisitor;

		internal readonly List<StateMachineBehaviour> broadcasterVisitor;

		internal readonly List<AnimatorStateMachine> _ProxyVisitor;

		internal readonly List<AnimatorTransitionBase> structVisitor;

		internal readonly List<AnimatorState> m_ServiceVisitor;

		internal readonly List<UnityEngine.Object> stateVisitor;

		private static object DisableIndexer;

		internal MethodVisitor(UnityEditor.Animations.AnimatorController instance)
		{
			_SchemaVisitor = new List<UnityEditor.Animations.BlendTree>();
			broadcasterVisitor = new List<StateMachineBehaviour>();
			_ProxyVisitor = new List<AnimatorStateMachine>();
			structVisitor = new List<AnimatorTransitionBase>();
			m_ServiceVisitor = new List<AnimatorState>();
			stateVisitor = new List<UnityEngine.Object>();
			if (!instance)
			{
				return;
			}
			UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(instance));
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] is AnimatorTransitionBase item))
				{
					if (array[i] is AnimatorState item2)
					{
						m_ServiceVisitor.Add(item2);
					}
					else if (array[i] is UnityEditor.Animations.BlendTree item3)
					{
						_SchemaVisitor.Add(item3);
					}
					else if (!(array[i] is StateMachineBehaviour item4))
					{
						if (!(array[i] is AnimatorStateMachine item5))
						{
							if (array[i] != LogoutMapper())
							{
								stateVisitor.Add(array[i]);
							}
						}
						else
						{
							_ProxyVisitor.Add(item5);
						}
					}
					else
					{
						broadcasterVisitor.Add(item4);
					}
				}
				else
				{
					structVisitor.Add(item);
				}
			}
		}

		internal static bool VerifyIndexer()
		{
			return DisableIndexer == null;
		}
	}

	private class GlobalVisitor
	{
		internal bool _TaskVisitor;

		internal AnimatorCondition m_ProcessVisitor;

		internal readonly List<(AnimatorTransitionBase, int)> _ProducerVisitor;

		internal readonly bool[] iteratorVisitor = new bool[3];

		internal static GlobalVisitor ConcatIndexer;

		internal GlobalVisitor(AnimatorTransitionBase asset, int ivk_low)
		{
			_TaskVisitor = false;
			m_ProcessVisitor = asset.conditions[ivk_low];
			_ProducerVisitor = new List<(AnimatorTransitionBase, int)> { (asset, ivk_low) };
		}

		internal void CollectInitializer(AnimatorTransitionBase res, int colhigh)
		{
			_TaskVisitor = true;
			_ProducerVisitor.Add((res, colhigh));
		}

		internal void ResolveInitializer(AnimatorCondition item)
		{
			UnityEngine.Object[] objectsToUndo = _ProducerVisitor.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
			Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
			foreach (var item4 in _ProducerVisitor)
			{
				AnimatorTransitionBase item2 = item4.Item1;
				int item3 = item4.Item2;
				AnimatorCondition[] conditions = item2.conditions;
				conditions[item3] = item;
				item2.conditions = conditions;
			}
		}

		private void ListInitializer(Func<AnimatorCondition, AnimatorCondition> init)
		{
			UnityEngine.Object[] objectsToUndo = _ProducerVisitor.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
			Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
			foreach (var item3 in _ProducerVisitor)
			{
				AnimatorTransitionBase item = item3.Item1;
				int item2 = item3.Item2;
				AnimatorCondition[] conditions = item.conditions;
				conditions[item2] = init(conditions[item2]);
				item.conditions = conditions;
			}
		}

		internal void VerifyInitializer(string value)
		{
			m_ProcessVisitor = new AnimatorCondition
			{
				parameter = value,
				mode = m_ProcessVisitor.mode,
				threshold = m_ProcessVisitor.threshold
			};
			ListInitializer((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = value,
				mode = c.mode,
				threshold = c.threshold
			});
			iteratorVisitor[0] = false;
		}

		internal void FillInitializer(AnimatorConditionMode value)
		{
			m_ProcessVisitor = new AnimatorCondition
			{
				parameter = m_ProcessVisitor.parameter,
				mode = value,
				threshold = m_ProcessVisitor.threshold
			};
			ListInitializer((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = c.parameter,
				mode = value,
				threshold = c.threshold
			});
			iteratorVisitor[1] = false;
		}

		internal void WriteInitializer(float ident)
		{
			m_ProcessVisitor = new AnimatorCondition
			{
				parameter = m_ProcessVisitor.parameter,
				mode = m_ProcessVisitor.mode,
				threshold = ident
			};
			ListInitializer((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = c.parameter,
				mode = c.mode,
				threshold = ident
			});
			iteratorVisitor[2] = false;
		}

		internal void ForgotInitializer()
		{
			m_ProcessVisitor = ResolveAlgo(m_ProcessVisitor);
			ListInitializer(ResolveAlgo);
		}

		internal void StopInitializer()
		{
			foreach (var item in _ProducerVisitor)
			{
				item.Item1.RemoveCondition(m_ProcessVisitor);
			}
		}

		internal void CheckInitializer(bool[] item)
		{
			for (int i = 0; i < 3; i++)
			{
				iteratorVisitor[i] |= !item[i];
			}
		}

		internal static bool CollectIndexer()
		{
			return ConcatIndexer == null;
		}
	}

	private class AlgoAlgo
	{
		internal bool _MapperAlgo;

		internal WorkerProperty.BridgeProperty.AttrProperty m_InitializerAlgo;

		internal List<(WorkerProperty.BridgeProperty, int)> definitionAlgo;

		private static AlgoAlgo NewIndexer;

		internal AlgoAlgo(WorkerProperty.BridgeProperty asset, int cust)
		{
			_MapperAlgo = false;
			m_InitializerAlgo = asset.m_DatabaseProperty[cust];
			definitionAlgo = new List<(WorkerProperty.BridgeProperty, int)> { (asset, cust) };
		}

		internal void RegisterInitializer(WorkerProperty.BridgeProperty config, int size_connection)
		{
			_MapperAlgo = true;
			definitionAlgo.Add((config, size_connection));
		}

		internal void LogoutInitializer(WorkerProperty.BridgeProperty.AttrProperty ident)
		{
			for (int i = 0; i < definitionAlgo.Count; i++)
			{
				WorkerProperty.BridgeProperty item = definitionAlgo[i].Item1;
				RestartAnnotation(ident, item.m_DatabaseProperty[definitionAlgo[i].Item2]);
				EditorUtility.SetDirty(item._StrategyProperty);
			}
		}

		internal void PatchInitializer()
		{
			foreach (var item2 in definitionAlgo)
			{
				WorkerProperty.BridgeProperty _RegAlgo = item2.Item1;
				int item = item2.Item2;
				bool num = _RegAlgo.InitPage(item);
				EditorUtility.SetDirty(_RegAlgo._StrategyProperty);
				if (num)
				{
					m_AlgoAnnotation.InvokeResolver(delegate(AnimatorState s)
					{
						s.DeletePredicate(s.LoginPredicate(_RegAlgo._StrategyProperty), verifytemp: true);
					});
				}
			}
		}

		internal static bool LoginIndexer()
		{
			return NewIndexer == null;
		}
	}

	private struct TestsAlgo
	{
		private readonly SerializedObject _PropertyAlgo;

		private SerializedProperty m_ProcessorAlgo;

		private SerializedProperty m_ObserverAlgo;

		private SerializedProperty _ServerAlgo;

		private SerializedProperty threadAlgo;

		private SerializedProperty _PolicyAlgo;

		private SerializedProperty serializerAlgo;

		private SerializedProperty m_PageAlgo;

		private SerializedProperty m_ResolverAlgo;

		private SerializedProperty m_PredicateAlgo;

		private SerializedProperty _RulesAlgo;

		private readonly List<SerializedProperty> queueAlgo;

		private readonly List<GUIContent> _ErrorAlgo;

		internal static object PrintIndexer;

		internal TestsAlgo(StateMachineBehaviour[] init)
		{
			_PropertyAlgo = new SerializedObject(init);
			m_ProcessorAlgo = _PropertyAlgo.FindProperty("trackingHead");
			m_ObserverAlgo = _PropertyAlgo.FindProperty("trackingLeftHand");
			_ServerAlgo = _PropertyAlgo.FindProperty("trackingRightHand");
			threadAlgo = _PropertyAlgo.FindProperty("trackingHip");
			_PolicyAlgo = _PropertyAlgo.FindProperty("trackingLeftFoot");
			serializerAlgo = _PropertyAlgo.FindProperty("trackingRightFoot");
			m_PageAlgo = _PropertyAlgo.FindProperty("trackingLeftFingers");
			m_ResolverAlgo = _PropertyAlgo.FindProperty("trackingRightFingers");
			m_PredicateAlgo = _PropertyAlgo.FindProperty("trackingEyes");
			_RulesAlgo = _PropertyAlgo.FindProperty("trackingMouth");
			queueAlgo = new List<SerializedProperty> { m_ProcessorAlgo, m_ObserverAlgo, _ServerAlgo, threadAlgo, _PolicyAlgo, serializerAlgo, m_PageAlgo, m_ResolverAlgo, m_PredicateAlgo, _RulesAlgo };
			_ErrorAlgo = new List<GUIContent>(queueAlgo.Select((SerializedProperty t) => new GUIContent(t.displayName.Replace("Tracking ", string.Empty), t.tooltip)));
		}

		private void ManageInitializer(int length_first)
		{
			while (true)
			{
				int recordAlgo = length_first;
			}
		}

		internal void PrintInitializer()
		{
			EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count < 1);
			_PropertyAlgo.Update();
			using (new GUILayout.VerticalScope("helpbox"))
			{
				Color[] colors = new Color[4]
				{
					new Color(0.7f, 0.7f, 0.7f),
					Color.green,
					Color.yellow,
					Color.cyan
				};
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("Tracking Control");
					using (new TemplateThread(TemplateThread.ColoringType.BG, Color.red))
					{
						if (ClassProperty.RestartQueue(ClassProperty.CalcError().fieldProcessor, ClassProperty.CalcError().configProcessor, GUILayout.Width(25f), GUILayout.Height(20f)))
						{
							m_AlgoAnnotation.InvokeResolver(delegate(AnimatorState s)
							{
								s.ReflectPredicate(WorkerProperty.QueryPage(), isstate: true);
							});
							m_ConnectionAnnotation = false;
							return;
						}
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					List<SerializedProperty> helperAlgo = queueAlgo;
					int num = (queueAlgo.All((SerializedProperty p) => !p.hasMultipleDifferentValues && p.enumValueIndex == helperAlgo[0].enumValueIndex) ? queueAlgo[0].enumValueIndex : 3);
					using (new TemplateThread(TemplateThread.ColoringType.FG, num, colors))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (ClassProperty.CountQueue("All", GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
							{
								int length_first = ((Event.current.button == 0) ? ((num != 1) ? 1 : 0) : ((num != 2) ? 2 : 0));
								ManageInitializer(length_first);
							}
							GUILayout.FlexibleSpace();
							EditorGUI.showMixedValue = num == 3;
							EditorGUI.BeginChangeCheck();
							num = EditorGUILayout.Popup(num, queueAlgo[0].enumDisplayNames, GUILayout.Width(260f));
							if (EditorGUI.EndChangeCheck())
							{
								ManageInitializer(num);
							}
							EditorGUI.showMixedValue = false;
						}
					}
				}
				ClassProperty.MapQueue();
				for (int num2 = 0; num2 < queueAlgo.Count; num2++)
				{
					SerializedProperty serializedProperty = queueAlgo[num2];
					int visZ = ((!serializedProperty.hasMultipleDifferentValues) ? serializedProperty.enumValueIndex : 3);
					using (new TemplateThread(TemplateThread.ColoringType.FG, visZ, colors))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (ClassProperty.RestartQueue(_ErrorAlgo[num2], GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
							{
								bool flag = Event.current.button == 0;
								serializedProperty.enumValueIndex = ((!flag) ? ((serializedProperty.enumValueIndex != 2) ? 2 : 0) : ((serializedProperty.enumValueIndex != 1) ? 1 : 0));
							}
							GUILayout.FlexibleSpace();
							EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, GUILayout.Width(260f));
						}
					}
				}
			}
			_PropertyAlgo.ApplyModifiedProperties();
			EditorGUI.EndDisabledGroup();
		}

		internal static bool ResolveIndexer()
		{
			return PrintIndexer == null;
		}
	}

	private enum VRCFullOptions
	{
		Set,
		Add,
		Random
	}

	private enum VRCHalfOptions
	{
		Set = 0,
		Random = 2
	}

	[Serializable]
	private class ConsumerAlgo
	{
		internal enum StateCosmeticOptions
		{
			none = 0,
			motionName = 1,
			motionIcon = 2,
			coordinates = 4,
			indicators = 8,
			inactiveIndicators = 16,
			quickNewClip = 32,
			all = -1
		}

		internal class WorkerAlgo : IDisposable
		{
			private readonly Action _FilterAlgo;

			private readonly bool stubAlgo;

			private readonly EditorGUI.ChangeCheckScope m_ReaderAlgo;

			private static WorkerAlgo CancelState;

			[SpecialName]
			internal bool CountDefinition()
			{
				return m_ReaderAlgo.changed;
			}

			public WorkerAlgo(Action ident = null)
			{
				_FilterAlgo = ident;
				stubAlgo = ComputeDefinition();
				MoveDefinition(isitem: true);
				m_ReaderAlgo = new EditorGUI.ChangeCheckScope();
			}

			public void Dispose()
			{
				bool changed = m_ReaderAlgo.changed;
				m_ReaderAlgo.Dispose();
				if (changed)
				{
					_FilterAlgo?.Invoke();
					SetupDefinition();
				}
				MoveDefinition(stubAlgo);
			}

			public static implicit operator bool(WorkerAlgo task)
			{
				return task.m_ReaderAlgo.changed;
			}

			internal static bool RestartState()
			{
				return CancelState == null;
			}
		}

		internal class BridgeAlgo : IDisposable
		{
			private readonly bool _StrategyAlgo;

			private static BridgeAlgo FillState;

			public BridgeAlgo()
			{
				_StrategyAlgo = ComputeDefinition();
				MoveDefinition(isitem: true);
			}

			public void Dispose()
			{
				MoveDefinition(_StrategyAlgo);
			}

			internal static bool DeleteState()
			{
				return FillState == null;
			}
		}

		[Serializable]
		internal class CustomerAlgo : AdvisorAlgo
		{
			[SerializeField]
			private bool _value;

			internal readonly Action m_DatabaseAlgo;

			private static CustomerAlgo RunState;

			[SpecialName]
			internal bool FindDefinition()
			{
				return _value;
			}

			[SpecialName]
			internal void ExcludeDefinition(bool excludeparam)
			{
				if (_value != excludeparam)
				{
					_value = excludeparam;
					m_DatabaseAlgo?.Invoke();
					SetupDefinition();
				}
			}

			internal CustomerAlgo(bool appendlast, Action ord = null)
			{
				callbackAlgo = (string)(object)appendlast;
				_value = appendlast;
				m_DatabaseAlgo = ord;
			}

			internal void InsertDefinition()
			{
				ExcludeDefinition(!_value);
			}

			internal void RestartDefinition(string info, GUIStyle attr = null, params GUILayoutOption[] options)
			{
				QueryDefinition(new GUIContent(info), attr, options);
			}

			internal void QueryDefinition(GUIContent setup, GUIStyle visitor = null, params GUILayoutOption[] options)
			{
				if (visitor == null)
				{
					visitor = EditorStyles.toggle;
				}
				ExcludeDefinition(EditorGUILayout.Toggle(setup, FindDefinition(), visitor, options));
			}

			internal void AddDefinition(string last, string vis = null, bool isthird = false, Color? vis2 = null, Color? x3 = null, params GUILayoutOption[] options)
			{
				InvokeDefinition((!string.IsNullOrEmpty(last)) ? new GUIContent(last) : GUIContent.none, (!string.IsNullOrEmpty(vis)) ? new GUIContent(vis) : GUIContent.none, isthird, vis2, x3, options);
			}

			internal void InvokeDefinition(GUIContent init, GUIContent cont = null, bool striptemplate = false, Color? counter2 = null, Color? map3 = null, params GUILayoutOption[] options)
			{
				counter2 = counter2 ?? GUI.backgroundColor;
				map3 = map3 ?? GUI.backgroundColor;
				Color backgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ((!FindDefinition()) ? map3.Value : counter2.Value);
				ExcludeDefinition(GUILayout.Toggle(FindDefinition(), (!FindDefinition() && cont != null) ? cont : init, (!striptemplate) ? GUI.skin.button : EditorStyles.toolbarButton, options));
				GUI.backgroundColor = backgroundColor;
			}

			public static implicit operator bool(CustomerAlgo key)
			{
				return key._value;
			}

			internal override void Reset()
			{
				ExcludeDefinition((bool)(object)callbackAlgo);
			}

			internal static bool ComputeState()
			{
				return RunState == null;
			}
		}

		[Serializable]
		internal class ExporterAlgo : AdvisorAlgo
		{
			[SerializeField]
			private float _value;

			internal readonly Action identifierAlgo;

			private static ExporterAlgo ConnectState;

			[SpecialName]
			internal float ResetDefinition()
			{
				return _value;
			}

			[SpecialName]
			internal void FlushDefinition(float reference)
			{
				if (_value != reference)
				{
					_value = reference;
					identifierAlgo?.Invoke();
					SetupDefinition();
				}
			}

			internal ExporterAlgo(float ident, Action result = null)
			{
				callbackAlgo = (string)(object)ident;
				_value = ident;
				identifierAlgo = result;
			}

			internal void VisitDefinition(string asset, bool rejectcont = true, GUIStyle comp = null, params GUILayoutOption[] options)
			{
				StartDefinition(new GUIContent(asset), rejectcont, comp, options);
			}

			internal void DefineDefinition(string config, float selection, bool testserv = true, GUIStyle vis2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = selection;
				StartDefinition(new GUIContent(config), testserv, vis2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void StartDefinition(GUIContent info, bool havesecond = true, GUIStyle dir = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					if (dir == null)
					{
						dir = EditorStyles.numberField;
					}
					FlushDefinition(EditorGUILayout.FloatField(info, ResetDefinition(), dir, options));
					if (havesecond && ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
					{
						Reset();
					}
				}
			}

			internal void ReadDefinition(GUIContent reference, float visitor, bool isserv = true, GUIStyle second2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = visitor;
				StartDefinition(reference, isserv, second2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void SelectDefinition(string info, float pred, float dir, bool counter2stop = true, params GUILayoutOption[] options)
			{
				RemoveDefinition(new GUIContent(info), pred, dir, counter2stop, options);
			}

			internal void RemoveDefinition(GUIContent last, float cfg, float temp, bool compareconfig2 = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					FlushDefinition(EditorGUILayout.Slider(last, ResetDefinition(), cfg, temp, options));
					if (!compareconfig2)
					{
						return;
					}
					uint num4 = default(uint);
					while (true)
					{
						int num;
						int num2;
						if (!ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
						{
							num = 1145303574;
							num2 = 1145303574;
						}
						else
						{
							num = 39847116;
							num2 = 39847116;
						}
						int num3 = num ^ (int)(num4 * 1512022182);
						while (true)
						{
							switch ((num4 = (uint)(num3 ^ -2088492295)) % 4)
							{
							case 0u:
							case 2u:
								break;
							default:
								return;
							case 1u:
								goto IL_0085;
							case 3u:
								return;
							}
							break;
							IL_0085:
							Reset();
							num3 = (int)((num4 * 1782949549) ^ 0x2FFAFC4F);
						}
					}
				}
			}

			internal void InstantiateDefinition(string key, bool ignorecounter = true, params GUILayoutOption[] options)
			{
				AwakeDefinition(new GUIContent(key), ignorecounter, options);
			}

			internal void AwakeDefinition(GUIContent ident, bool isord = true, params GUILayoutOption[] options)
			{
				RemoveDefinition(ident, 0f, 1f, isord, options);
			}

			internal override void Reset()
			{
				FlushDefinition((float)(object)callbackAlgo);
			}

			public static implicit operator int(ExporterAlgo key)
			{
				return (int)key._value;
			}

			public static implicit operator float(ExporterAlgo item)
			{
				return item._value;
			}

			internal static bool ViewState()
			{
				return ConnectState == null;
			}
		}

		[Serializable]
		internal class AttrAlgo : ExporterAlgo
		{
			private static AttrAlgo ChangeState;

			[SerializeField]
			internal int CalcDefinition
			{
				get
				{
					return (int)ResetDefinition();
				}
				set
				{
					FlushDefinition(value);
				}
			}

			internal AttrAlgo(int previous_task, Action vis = null)
				: base(previous_task, vis)
			{
			}

			internal T CalculateDefinition<T>() where T : Enum
			{
				return (T)(object)CalcDefinition;
			}

			internal void TestDefinition(GUIContent value, GUIStyle cont = null, params GUILayoutOption[] options)
			{
				if (cont == null)
				{
					cont = EditorStyles.numberField;
				}
				CalcDefinition = EditorGUILayout.IntField(value, CalcDefinition, cont, options);
			}

			internal void MapDefinition(string spec, GUIStyle reg = null, params GUILayoutOption[] options)
			{
				TestDefinition(new GUIContent(spec), reg, options);
			}

			internal void ValidateDefinition<T>(GUIContent info, bool ismap = false, GUIStyle tag = null, params GUILayoutOption[] options) where T : Enum
			{
				if (tag == null)
				{
					tag = EditorStyles.popup;
				}
				CalcDefinition = ((!ismap) ? ((int)(object)EditorGUILayout.EnumPopup(info, (T)(object)CalcDefinition, tag, options)) : ((int)(object)EditorGUILayout.EnumFlagsField(info, (T)(object)CalcDefinition, tag, options)));
			}

			internal void CustomizeDefinition<T>(string spec, bool hasreg = false, GUIStyle template = null, params GUILayoutOption[] options) where T : Enum
			{
				ValidateDefinition<T>(new GUIContent(spec), hasreg, template, options);
			}

			internal static AttrAlgo RateDefinition<T>(T ident, Action result = null) where T : Enum
			{
				return new AttrAlgo((int)(object)ident, result);
			}

			public static implicit operator int(AttrAlgo param)
			{
				return param.CalcDefinition;
			}

			public static implicit operator float(AttrAlgo var1)
			{
				return var1.CalcDefinition;
			}

			internal static bool CalculateState()
			{
				return ChangeState == null;
			}
		}

		[Serializable]
		internal class DispatcherAlgo : AdvisorAlgo
		{
			[SerializeField]
			private float _valueX;

			[SerializeField]
			private float _valueY;

			[SerializeField]
			private float _valueZ;

			internal Action _RegistryAlgo;

			internal bool _TagAlgo;

			internal Vector3 importerAlgo;

			private static DispatcherAlgo GetState;

			[SpecialName]
			internal Vector3 DeleteDefinition()
			{
				if (!_TagAlgo)
				{
					_TagAlgo = true;
					importerAlgo = new Vector3(_valueX, _valueY, _valueZ);
				}
				return importerAlgo;
			}

			[SpecialName]
			internal void CreateDefinition(Vector3 value)
			{
				if (importerAlgo != value)
				{
					importerAlgo = value;
					_valueX = value.x;
					_valueY = value.y;
					_valueZ = value.z;
					_RegistryAlgo?.Invoke();
					SetupDefinition();
				}
			}

			internal void IncludeDefinition(Vector3 config, Action ord)
			{
				callbackAlgo = (string)(object)config;
				_RegistryAlgo = ord;
				_valueX = config.x;
				_valueY = config.y;
				_valueZ = config.z;
			}

			internal DispatcherAlgo(Vector3 value, Action selection = null)
			{
				IncludeDefinition(value, selection);
			}

			internal DispatcherAlgo(float init, float token, float pool, Action second2 = null)
			{
				IncludeDefinition(new Vector3(init, token, pool), second2);
			}

			internal DispatcherAlgo(float asset, float ivk, Action tag = null)
			{
				IncludeDefinition(new Vector3(asset, ivk), tag);
			}

			internal void RunDefinition(GUIContent init, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label(init, GUILayout.MaxWidth(117f));
					CreateDefinition(EditorGUILayout.Vector2Field(GUIContent.none, DeleteDefinition(), options));
					if (GUILayout.Button(ClassProperty.DestroyError()._CallbackProcessor, ClassProperty.CalcError().m_ServiceProcessor, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						Reset();
					}
				}
			}

			internal void CloneDefinition(string key, params GUILayoutOption[] options)
			{
				RunDefinition(new GUIContent(key), options);
			}

			internal void LoginDefinition(GUIContent reference, params GUILayoutOption[] options)
			{
				CreateDefinition(EditorGUILayout.Vector3Field(reference, DeleteDefinition(), options));
			}

			internal void ReflectDefinition(string res, params GUILayoutOption[] options)
			{
				LoginDefinition(new GUIContent(res), options);
			}

			internal override void Reset()
			{
				CreateDefinition((Vector3)(object)callbackAlgo);
			}

			public static implicit operator Vector2(DispatcherAlgo asset)
			{
				return asset.DeleteDefinition();
			}

			internal static bool VisitState()
			{
				return GetState == null;
			}
		}

		[Serializable]
		internal class RequestAlgo : AdvisorAlgo
		{
			[SerializeField]
			private string _value;

			internal readonly Action printerAlgo;

			private static RequestAlgo StopState;

			[SpecialName]
			internal string CollectDefinition()
			{
				return _value;
			}

			[SpecialName]
			internal void ResolveDefinition(string value)
			{
				if (_value != value)
				{
					_value = value;
					printerAlgo?.Invoke();
					SetupDefinition();
				}
			}

			internal RequestAlgo(string last = "", Action pred = null)
			{
				callbackAlgo = last;
				_value = last;
				printerAlgo = pred;
			}

			internal void PushDefinition(string first, bool outputb = true, bool containstemplate = true, GUIStyle def2 = null, params GUILayoutOption[] options)
			{
				ViewDefinition(new GUIContent(first), outputb, containstemplate, def2, options);
			}

			internal void ViewDefinition(GUIContent def, bool setcust = true, bool deletehelper = true, GUIStyle counter2 = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					if (counter2 == null)
					{
						counter2 = EditorStyles.textField;
					}
					ResolveDefinition((!deletehelper) ? EditorGUILayout.TextField(def, CollectDefinition(), counter2, options) : EditorGUILayout.DelayedTextField(def, CollectDefinition(), counter2, options));
					if (setcust && ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
					{
						Reset();
					}
				}
			}

			internal override void Reset()
			{
				ResolveDefinition(callbackAlgo);
			}

			public override string ToString()
			{
				return CollectDefinition();
			}

			public static implicit operator string(RequestAlgo res)
			{
				return res._value;
			}

			internal static bool ReflectState()
			{
				return StopState == null;
			}
		}

		[Serializable]
		internal class WriterAlgo : AdvisorAlgo
		{
			internal readonly Action m_ParamsAlgo;

			[SerializeField]
			private float r;

			[SerializeField]
			private float g;

			[SerializeField]
			private float b;

			[SerializeField]
			private float a;

			private static WriterAlgo RateState;

			[SpecialName]
			internal Color WriteDefinition()
			{
				return new Color(r, g, b, a);
			}

			[SpecialName]
			internal void ForgotDefinition(Color value)
			{
				r = value.r;
				g = value.g;
				b = value.b;
				a = value.a;
				m_ParamsAlgo?.Invoke();
				SetupDefinition();
			}

			internal WriterAlgo(float item, float second, float state, float visitor2 = 1f, Action res3 = null)
			{
				Color color = new Color(item, second, state, visitor2);
				callbackAlgo = (string)(object)color;
				r = item;
				g = second;
				b = state;
				a = visitor2;
				m_ParamsAlgo = res3;
			}

			internal WriterAlgo(Color info, Action cont = null)
			{
				callbackAlgo = (string)(object)info;
				r = info.r;
				g = info.g;
				b = info.b;
				a = info.a;
				m_ParamsAlgo = cont;
			}

			internal void VerifyDefinition(string setup, bool isvis = true, params GUILayoutOption[] options)
			{
				FillDefinition(new GUIContent(setup), isvis, options);
			}

			internal void FillDefinition(GUIContent task, bool outputtoken = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					ForgotDefinition(EditorGUILayout.ColorField(task, WriteDefinition(), options));
					if (outputtoken && ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
					{
						Reset();
					}
				}
			}

			internal override void Reset()
			{
				ForgotDefinition((Color)(object)callbackAlgo);
			}

			internal static bool PostState()
			{
				return RateState == null;
			}
		}

		[Serializable]
		internal class ListenerAlgo : AdvisorAlgo
		{
			internal readonly Action _GetterAlgo;

			private readonly Type interceptorAlgo;

			[SerializeField]
			internal string guid;

			[SerializeField]
			internal long localID;

			private string _CreatorAlgo;

			private long _EventAlgo;

			private bool m_InfoAlgo;

			private UnityEngine.Object m_FacadeAlgo;

			internal static ListenerAlgo DefineState;

			[SpecialName]
			internal UnityEngine.Object ChangeDefinition()
			{
				if (!m_InfoAlgo)
				{
					m_InfoAlgo = true;
					m_FacadeAlgo = AssetDefinition<UnityEngine.Object>(guid, localID);
				}
				return m_FacadeAlgo;
			}

			[SpecialName]
			internal void SortDefinition(UnityEngine.Object reference)
			{
				if (m_FacadeAlgo != reference)
				{
					m_FacadeAlgo = reference;
					if (reference == null)
					{
						guid = string.Empty;
						localID = 0L;
					}
					else
					{
						AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out guid, out localID);
					}
					_GetterAlgo?.Invoke();
					SetupDefinition();
				}
			}

			internal ListenerAlgo(Type asset, string ivk = "", long column_comp = 0L, Action pol2 = null)
			{
				interceptorAlgo = asset;
				_CreatorAlgo = ivk;
				_EventAlgo = column_comp;
				guid = ivk;
				localID = column_comp;
				_GetterAlgo = pol2;
			}

			internal void CheckDefinition(string item, bool istoken = true, params GUILayoutOption[] options)
			{
				PrepareDefinition(new GUIContent(item), istoken, options);
			}

			internal void PrepareDefinition(GUIContent def, bool isb = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SortDefinition(EditorGUILayout.ObjectField(def, ChangeDefinition(), interceptorAlgo, allowSceneObjects: false, options));
					if (isb && ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
					{
						Reset();
					}
				}
			}

			private static T AssetDefinition<T>(string key, long indexOf_pol) where T : UnityEngine.Object
			{
				if (!string.IsNullOrWhiteSpace(key))
				{
					if (indexOf_pol != 0L)
					{
						UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(key));
						foreach (UnityEngine.Object obj in array)
						{
							AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long localId);
							if (localId == indexOf_pol)
							{
								return (T)obj;
							}
						}
						return null;
					}
					return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(key));
				}
				return null;
			}

			internal T UpdateDefinition<T>() where T : UnityEngine.Object
			{
				return (T)ChangeDefinition();
			}

			internal override void Reset()
			{
				SortDefinition(AssetDefinition<UnityEngine.Object>(_CreatorAlgo, _EventAlgo));
			}

			public static implicit operator bool(ListenerAlgo last)
			{
				return last.ChangeDefinition();
			}

			internal static bool EnableState()
			{
				return DefineState == null;
			}
		}

		internal abstract class AdvisorAlgo
		{
			internal string callbackAlgo;

			private static AdvisorAlgo DisableState;

			internal abstract void Reset();

			internal static bool VerifyState()
			{
				return DisableState == null;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal class IndexerAlgo : Attribute
		{
			internal static IndexerAlgo ConcatState;

			internal static bool CollectState()
			{
				return ConcatState == null;
			}
		}

		[SerializeField]
		internal CustomerAlgo a_VerifyOnDisplay = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo a_VerifyOnProjectLoad = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo editingTransitions = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo editingStates = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo editingController = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo matchParameter = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo matchMode = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo matchValue = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo showTransitionSettings = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo showTransitionConditions = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo showMatchingOptions = new CustomerAlgo(appendlast: false, UpdateVisitor);

		[SerializeField]
		internal CustomerAlgo showTransitionsCount = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo showStateSettings = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo showStateCount = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo showVRCDrivers = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo showVRCTracking = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo useLegacyDropdown = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo switchDoubleClick = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo autoReverseModes = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo reverseModifiesValues = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo animateInboundEdges = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo animateOutboundEdges = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo autoFrameLayer = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo displayLayerIndex = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo displayParameterType = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo capitalParameterIndicator = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo aw_active = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo aw_autoSwitchClip = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo aw_enablePropertyEditing = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo aw_enableGameObjectDND = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo aw_enableOverride = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo aw_warnPropertyMerge = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo graphBackgroundIsTexture = new CustomerAlgo(appendlast: false, SortAlgo);

		[SerializeField]
		internal CustomerAlgo cosmeticGraphActive = new CustomerAlgo(appendlast: false, SortAlgo);

		[SerializeField]
		internal CustomerAlgo cosmeticNodesActive = new CustomerAlgo(appendlast: false, PatchAlgo);

		[SerializeField]
		internal CustomerAlgo cosmeticTransitionsActive = new CustomerAlgo(appendlast: false, PatchAlgo);

		[SerializeField]
		internal CustomerAlgo hasPingedController = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo requiresStateRename = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo advancedQuickToggle = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo mergeQuickToggle = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo warnParameterConversion = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo displayCategoryView = new CustomerAlgo(appendlast: true, delegate
		{
			CallDefinition().sortCategoryViewLayers.ExcludeDefinition(excludeparam: false);
			writerVisitor = LayerViewViewType.DefaultView;
		});

		[SerializeField]
		internal CustomerAlgo sortCategoryViewLayers = new CustomerAlgo(appendlast: true);

		[SerializeField]
		internal CustomerAlgo displayLayerCompactView = new CustomerAlgo(appendlast: true, delegate
		{
			CallDefinition().layerCompactView.ExcludeDefinition(excludeparam: false);
		});

		[SerializeField]
		internal CustomerAlgo layerCompactView = new CustomerAlgo(appendlast: false, PublishAnnotation);

		[SerializeField]
		internal ExporterAlgo anyStateNodeColor = new ExporterAlgo(2f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo entryStateNodeColor = new ExporterAlgo(3f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo exitStateNodeColor = new ExporterAlgo(6f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo machineStateNodeColor = new ExporterAlgo(0f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo normalStateNodeColor = new ExporterAlgo(0f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo defaultStateNodeColor = new ExporterAlgo(5f, PatchAlgo);

		[SerializeField]
		internal ExporterAlgo defaultLayerWeight = new ExporterAlgo(1f);

		[SerializeField]
		internal ExporterAlgo arrowLerpRatio = new ExporterAlgo(-0.5f);

		[SerializeField]
		internal DispatcherAlgo defaultEntryPosition = new DispatcherAlgo(50f, 120f);

		[SerializeField]
		internal DispatcherAlgo defaultExitPosition = new DispatcherAlgo(800f, 120f);

		[SerializeField]
		internal DispatcherAlgo defaultAnyPosition = new DispatcherAlgo(50f, 20f);

		[SerializeField]
		internal WriterAlgo normalTransitionColor = new WriterAlgo(1f, 1f, 1f);

		[SerializeField]
		internal WriterAlgo entryTransitionColor = new WriterAlgo(0.6f, 0.4f, 0f);

		[SerializeField]
		internal WriterAlgo selectedTransitionColor = new WriterAlgo(0.42f, 0.7f, 1f);

		[SerializeField]
		internal WriterAlgo baseTransitionColor = new WriterAlgo(0.5f, 0.5f, 0.5f);

		[SerializeField]
		internal WriterAlgo gridBackgroundColor = new WriterAlgo(0.1647f, 0.1647f, 0.16f, 1f, SortAlgo);

		[SerializeField]
		internal WriterAlgo gridMinorLightColor = new WriterAlgo(0f, 0f, 0f, 0.1f);

		[SerializeField]
		internal WriterAlgo gridMajorLightColor = new WriterAlgo(0f, 0f, 0f, 0.15f);

		[SerializeField]
		internal WriterAlgo gridMinorDarkColor = new WriterAlgo(0f, 0f, 0f, 0.18f);

		[SerializeField]
		internal WriterAlgo gridMajorDarkColor = new WriterAlgo(0f, 0f, 0f, 0.28f);

		[SerializeField]
		internal WriterAlgo parameterLabelColor = new WriterAlgo(0.7f, 0.7f, 0.7f);

		[SerializeField]
		internal ListenerAlgo defaultLayerMask = new ListenerAlgo(typeof(AvatarMask));

		[SerializeField]
		internal ListenerAlgo graphBackgroundTexture = new ListenerAlgo(typeof(Texture2D), "", 0L, SortAlgo);

		[SerializeField]
		internal RequestAlgo saveFolder = new RequestAlgo("Assets/DreadScripts/ControllerEditor/Generated Assets");

		[SerializeField]
		internal RequestAlgo lastAnimationPath = new RequestAlgo("Assets");

		[SerializeField]
		internal RequestAlgo lastAnimationName = new RequestAlgo("New Animation Clip");

		[SerializeField]
		internal RequestAlgo categoryBaseName = new RequestAlgo("Base");

		[SerializeField]
		internal RequestAlgo categoryDelimiter = new RequestAlgo("/");

		[SerializeField]
		internal AttrAlgo parameterLabelFontStyle = AttrAlgo.RateDefinition(FontStyle.Normal, SetDefinition);

		[SerializeField]
		internal AttrAlgo stateCosmetics = AttrAlgo.RateDefinition(StateCosmeticOptions.all);

		[IndexerAlgo]
		internal AnimatorState defaultState;

		[IndexerAlgo]
		internal AnimatorStateTransition defaultTransition;

		[NonSerialized]
		internal static GUIStyle m_AdapterAlgo;

		private static bool _InterpreterAlgo;

		private static bool watcherAlgo;

		private static bool _CandidateAlgo;

		private static FieldInfo[] _ProductAlgo;

		private static ConsumerAlgo m_ExpressionAlgo;

		internal static Action _SystemAlgo;

		[SerializeField]
		internal RequestAlgo u_updateLink = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_updateVersion = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_updateMessage = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_updateChangelog = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_updateDay = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_announcement = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_announcementLink = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_announcementLinkName = new RequestAlgo();

		[SerializeField]
		internal RequestAlgo u_announcementHiddenDate = new RequestAlgo();

		[SerializeField]
		internal CustomerAlgo u_updateHidden = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo u_announcementHidden = new CustomerAlgo(appendlast: false);

		[SerializeField]
		internal CustomerAlgo a_HasSucceededLastVerification = new CustomerAlgo(appendlast: false);

		private static ConsumerAlgo RegisterState;

		internal static void SetDefinition()
		{
			m_AdapterAlgo = new GUIStyle(ClassProperty.CalcError().algoObserver)
			{
				fontStyle = CallDefinition().parameterLabelFontStyle.CalculateDefinition<FontStyle>()
			};
		}

		internal StateCosmeticOptions PostDefinition()
		{
			return stateCosmetics.CalculateDefinition<StateCosmeticOptions>();
		}

		[SpecialName]
		internal static bool ComputeDefinition()
		{
			return _CandidateAlgo;
		}

		[SpecialName]
		internal static void MoveDefinition(bool isitem)
		{
			bool candidateAlgo = _CandidateAlgo;
			_CandidateAlgo = isitem;
			if (candidateAlgo && !_CandidateAlgo && watcherAlgo)
			{
				SetupDefinition();
			}
		}

		[SpecialName]
		internal static ConsumerAlgo CallDefinition()
		{
			if (m_ExpressionAlgo == null)
			{
				EnableDefinition();
			}
			return m_ExpressionAlgo;
		}

		private ConsumerAlgo()
		{
			_ProductAlgo = (from m in typeof(ConsumerAlgo).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				where m.IsDefined(typeof(IndexerAlgo), inherit: false)
				select m).ToArray();
		}

		internal static void SetupDefinition()
		{
			watcherAlgo = false;
			if (_CandidateAlgo)
			{
				watcherAlgo = true;
			}
			else
			{
				if (_InterpreterAlgo)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(CallDefinition()) + "]\u200b\u200b\u200b");
				FieldInfo[] productAlgo = _ProductAlgo;
				foreach (FieldInfo fieldInfo in productAlgo)
				{
					try
					{
						string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(CallDefinition()));
						stringBuilder.Append(fieldInfo.Name + "[" + text + "]\u200b\u200b\u200b");
					}
					catch (System.Exception message)
					{
						UnityEngine.Debug.LogError(message);
					}
				}
				string value = stringBuilder.ToString();
				EditorPrefs.SetString("yOk0XCnENLMO6DIF8cYpSg==SettingsJSON", value);
			}
		}

		private static void EnableDefinition()
		{
			string text = string.Empty;
			if (EditorPrefs.HasKey("yOk0XCnENLMO6DIF8cYpSg==SettingsJSON"))
			{
				text = EditorPrefs.GetString("yOk0XCnENLMO6DIF8cYpSg==SettingsJSON", string.Empty);
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(text))
			{
				MatchCollection matchCollection = Regex.Matches(text, "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B");
				for (int i = 0; i < matchCollection.Count; i++)
				{
					Match match = matchCollection[i];
					dictionary.Add(match.Groups[1].Value, match.Groups[2].Value);
				}
			}
			if (dictionary.TryGetValue("MAIN", out var value))
			{
				m_ExpressionAlgo = JsonUtility.FromJson<ConsumerAlgo>(value);
			}
			if (m_ExpressionAlgo == null)
			{
				m_ExpressionAlgo = new ConsumerAlgo();
			}
			FieldInfo[] productAlgo = _ProductAlgo;
			foreach (FieldInfo fieldInfo in productAlgo)
			{
				object obj = fieldInfo.GetValue(m_ExpressionAlgo) ?? Activator.CreateInstance(fieldInfo.FieldType);
				if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
				{
					EditorJsonUtility.FromJsonOverwrite(value2, obj);
				}
				fieldInfo.SetValue(m_ExpressionAlgo, obj);
				if (fieldInfo.GetValue(m_ExpressionAlgo) == null)
				{
					fieldInfo.SetValue(m_ExpressionAlgo, Activator.CreateInstance(fieldInfo.FieldType));
				}
			}
		}

		internal static void PublishDefinition()
		{
			if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
			{
				PopDefinition();
			}
		}

		internal static void PopDefinition()
		{
			m_ExpressionAlgo = new ConsumerAlgo();
			FieldInfo[] productAlgo = _ProductAlgo;
			foreach (FieldInfo fieldInfo in productAlgo)
			{
				fieldInfo.SetValue(m_ExpressionAlgo, Activator.CreateInstance(fieldInfo.FieldType));
			}
			_SystemAlgo?.Invoke();
			SetupDefinition();
		}

		internal static bool FlushState()
		{
			return RegisterState == null;
		}
	}

	private static class FactoryAlgo
	{
		internal struct DecoratorAlgo
		{
			internal string comparatorAlgo;

			internal ushort m_ExceptionAlgo;

			internal ushort _ObjectAlgo;

			internal string utilsAlgo;
		}

		private static string m_AccountAlgo;

		private static bool m_RefAlgo;

		private static bool _StatusAlgo;

		private static bool m_TokenAlgo;

		private static bool codeAlgo;

		private static DecoratorAlgo? _DicAlgo;

		private static DecoratorAlgo? _InvocationAlgo;

		private static Action roleAlgo;

		private static ushort _ParamAlgo;

		internal static bool m_ModelAlgo;

		internal static readonly HashSet<DecoratorAlgo> m_TokenizerAlgo = new HashSet<DecoratorAlgo>();

		internal static FactoryAlgo TestState;

		[SpecialName]
		private static float PublishReg()
		{
			return (float)(int)_ParamAlgo / 1f;
		}

		internal static void ManageDefinition(Action var1, ushort flagssecond = 0, string third = "", ushort max_v2 = 0, bool useinfo3 = false, string first4 = "")
		{
			PrintDefinition(var1, null, flagssecond, third, max_v2, useinfo3, first4);
		}

		internal static void PrintDefinition(Action res, Action b, ushort pool_amount = 0, string v2 = "", ushort selection3_length = 0, bool isspec4 = false, string asset5 = "")
		{
			roleAlgo = b;
			if (pool_amount > 0)
			{
				while (true)
				{
					CompareReg(pool_amount, v2, selection3_length);
				}
			}
			try
			{
				res();
			}
			catch (System.Exception info)
			{
				if (m_ModelAlgo)
				{
					throw;
				}
				SearchDefinition(info, isspec4, asset5);
				CompilationPipeline.compilationStarted -= EnableReg;
				CompilationPipeline.compilationStarted += EnableReg;
				throw;
			}
		}

		private static void SearchDefinition(System.Exception info, bool isselection = false, string state = "")
		{
			if (!_InvocationAlgo.HasValue || m_TokenizerAlgo.Contains(_InvocationAlgo.Value))
			{
				return;
			}
			m_AccountAlgo = string.Empty;
			m_RefAlgo = false;
			m_TokenAlgo = false;
			_StatusAlgo = false;
			_DicAlgo = new DecoratorAlgo
			{
				comparatorAlgo = _InvocationAlgo.Value.comparatorAlgo,
				m_ExceptionAlgo = _InvocationAlgo.Value.m_ExceptionAlgo,
				_ObjectAlgo = _InvocationAlgo.Value._ObjectAlgo,
				utilsAlgo = info.Message
			};
			if (isselection)
			{
				switch (EditorUtility.DisplayDialogComplex("Error!", string.IsNullOrWhiteSpace(state) ? "An error has occurred! Do you want to try to find a solution for it?" : state, "Find Solution", "Close", "Ignore"))
				{
				case 2:
					m_TokenizerAlgo.Add(_DicAlgo.Value);
					EnableReg(null);
					break;
				case 0:
					m_TokenizerAlgo.Add(_DicAlgo.Value);
					ComputeInitializer(ignoresetup: true);
					break;
				case 1:
					EnableReg(null);
					break;
				}
			}
		}

		internal static void RevertDefinition(bool isvar1 = true)
		{
			if (!OrderReg())
			{
				return;
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ClassProperty.DestroyError().ruleProcessor, ClassProperty.CalcError().broadcasterProcessor);
				GUILayout.Label("An error has occurred! Do you want to report it?", EditorStyles.boldLabel);
				if (ClassProperty.DisableQueue("Ignore"))
				{
					SetupReg(isinstance: false);
				}
				if (ClassProperty.DisableQueue("Find Solution"))
				{
					SetupReg(isinstance: true);
				}
			}
			if (isvar1)
			{
				ClassProperty.MapQueue();
			}
		}

		internal static bool OrderReg()
		{
			if (!_DicAlgo.HasValue)
			{
				return false;
			}
			if (!m_TokenizerAlgo.Contains(_DicAlgo.Value))
			{
				return true;
			}
			_DicAlgo = null;
			return false;
		}

		internal static void CompareReg(ushort num_ident, string ivk = "", ushort dir_count = 0)
		{
			_InvocationAlgo = new DecoratorAlgo
			{
				m_ExceptionAlgo = num_ident,
				comparatorAlgo = ivk,
				_ObjectAlgo = dir_count
			};
		}

		internal static void SetReg()
		{
			m_AccountAlgo = string.Empty;
			m_RefAlgo = false;
			m_ModelAlgo = false;
			_ParamAlgo = 0;
			_InvocationAlgo = null;
		}

		internal static void PostReg()
		{
			ComputeInitializer(listenerAnnotation && _DicAlgo.HasValue);
			if (!m_TokenAlgo)
			{
				m_TokenAlgo = true;
				codeAlgo = true;
				List<(string, string)> list = RegisterAnnotation("findsolution", new(string, string)[4]
				{
					("bug_id", _DicAlgo.Value.m_ExceptionAlgo.ToString()),
					("bug_version", _DicAlgo.Value._ObjectAlgo.ToString()),
					("bug_name", _DicAlgo.Value.comparatorAlgo),
					("bug_exception", Uri.EscapeUriString(_DicAlgo.Value.utilsAlgo))
				});
				LogoutAnnotation(list);
				DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
				{
					bool flag = response.InsertReg("success");
					string text = response.InsertReg("message");
					_StatusAlgo = true;
					if (!string.IsNullOrWhiteSpace(text))
					{
						FindVisitor(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
					}
					m_AccountAlgo = response.InsertReg("solution");
					m_RefAlgo = response.InsertReg("complete");
				}, UnityEngine.Debug.LogException, null, null, delegate
				{
					codeAlgo = false;
					InsertVisitor();
				});
			}
			SetVisitor(codeAlgo ? "Finding a solution..." : "Bug Reporter", "If you have found a bug, please report it here!\nNote that the report is not anonymous. Abuse may result in blacklisting.");
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (codeAlgo)
				{
					if (ClassProperty.CountQueue("Cancel", EditorStyles.toolbarButton))
					{
						ComputeInitializer(ignoresetup: false);
					}
					return;
				}
				if (_StatusAlgo)
				{
					if (string.IsNullOrWhiteSpace(m_AccountAlgo))
					{
						using (new TemplateThread(TemplateThread.ColoringType.FG, ClassProperty._WrapperProcessor))
						{
							GUILayout.Label("No solution Found! Please write the steps to reproduce this issue below:");
						}
						_ProductAnnotation = EditorGUILayout.TextArea(_ProductAnnotation, GUILayout.MinHeight(54f));
						if (!string.IsNullOrWhiteSpace(_ProductAnnotation) && _ProductAnnotation.Length > 2000)
						{
							_ProductAnnotation = _ProductAnnotation.Substring(0, 2000);
						}
						if (!string.IsNullOrWhiteSpace(m_AccountAlgo))
						{
							return;
						}
						using (new GUILayout.HorizontalScope())
						{
							if (ClassProperty.DisableQueue("Cancel", GUILayout.ExpandWidth(expand: false)))
							{
								ComputeInitializer(ignoresetup: false);
							}
							using (new EditorGUI.DisabledScope(candidateAnnotation))
							{
								if (!ClassProperty.DisableQueue("Report Issue"))
								{
									return;
								}
								List<(string, string)> list2 = RegisterAnnotation("reportbug", new(string, string)[5]
								{
									("bug_id", _DicAlgo.Value.m_ExceptionAlgo.ToString()),
									("bug_version", _DicAlgo.Value._ObjectAlgo.ToString()),
									("bug_name", _DicAlgo.Value.comparatorAlgo),
									("bug_exception", _DicAlgo.Value.utilsAlgo),
									("feedback", Uri.EscapeUriString(_ProductAnnotation))
								});
								LogoutAnnotation(list2);
								candidateAnnotation = true;
								DisableVisitor(CallVisitor(list2.ToArray())).QueryRules(delegate(FieldAlgo response)
								{
									bool flag = response.InsertReg("success");
									string text = response.InsertReg("message");
									if (!string.IsNullOrEmpty(text))
									{
										FindVisitor(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
									}
								}, UnityEngine.Debug.LogException, null, null, delegate
								{
									ComputeInitializer(ignoresetup: false);
									candidateAnnotation = false;
									InsertVisitor();
								});
								return;
							}
						}
					}
					if (m_RefAlgo)
					{
						using (new TemplateThread(TemplateThread.ColoringType.FG, ClassProperty.configurationProperty))
						{
							GUILayout.Label("Solution Found!");
						}
					}
					else
					{
						using (new TemplateThread(TemplateThread.ColoringType.FG, ClassProperty._WrapperProcessor))
						{
							GUILayout.Label("Known issue! Details:");
						}
					}
					EditorGUILayout.Space();
					EditorGUILayout.SelectableLabel(m_AccountAlgo, GUI.skin.label, GUILayout.ExpandHeight(expand: false));
					if (ClassProperty.DisableQueue("Ok"))
					{
						ComputeInitializer(ignoresetup: false);
					}
					return;
				}
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(ClassProperty.DestroyError().ruleProcessor, ClassProperty.CalcError().broadcasterProcessor);
					using (new TemplateThread(TemplateThread.ColoringType.FG, ClassProperty._ProcProperty))
					{
						GUILayout.Label("There was an issue contacting the server for a solution.");
					}
				}
				if (ClassProperty.DisableQueue("Cancel"))
				{
					ComputeInitializer(ignoresetup: false);
				}
			}
		}

		internal static void SetupReg(bool isinstance)
		{
			if (OrderReg() && _DicAlgo.HasValue)
			{
				if (m_TokenizerAlgo.Contains(_DicAlgo.Value))
				{
					_DicAlgo = null;
				}
				ComputeInitializer(isinstance);
				m_TokenizerAlgo.Add(_DicAlgo.Value);
			}
		}

		internal static void EnableReg(object res)
		{
			if (_DicAlgo.HasValue && roleAlgo != null)
			{
				ManageDefinition(roleAlgo, _DicAlgo.Value.m_ExceptionAlgo, _DicAlgo.Value.comparatorAlgo, _DicAlgo.Value._ObjectAlgo);
			}
			roleAlgo = null;
			CompilationPipeline.compilationStarted -= EnableReg;
		}

		internal static bool IncludeState()
		{
			return TestState == null;
		}
	}

	private sealed class PoolAlgo
	{
		private readonly ProcessStartInfo _ParameterAlgo;

		private Process composerAlgo;

		private readonly Action<string> _RepositoryAlgo;

		private readonly Action m_MappingAlgo;

		private readonly bool baseAlgo;

		private string _ContainerAlgo;

		private bool classAlgo;

		internal bool m_MockAlgo;

		private bool _InstanceAlgo;

		internal static PoolAlgo NewState;

		internal PoolAlgo(string var1, Action<string> col, bool isfield = false, bool containscol2 = false, Action token3 = null)
		{
			_ParameterAlgo = new ProcessStartInfo((!isfield) ? "powershell.exe" : "cmd.exe")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = false,
				RedirectStandardOutput = true,
				Arguments = "/c " + var1
			};
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
			_ParameterAlgo.WorkingDirectory = folderPath;
			if (!isfield)
			{
				string text = Path.Combine(folderPath, "WindowsPowerShell", "v1.0");
				if (Directory.Exists(text))
				{
					_ParameterAlgo.WorkingDirectory = text;
				}
			}
			_RepositoryAlgo = col;
			m_MappingAlgo = token3;
			baseAlgo = containscol2;
		}

		internal void CancelReg()
		{
			_ContainerAlgo = string.Empty;
			_InstanceAlgo = false;
			m_MockAlgo = false;
			classAlgo = false;
			composerAlgo = new Process();
			composerAlgo.StartInfo = _ParameterAlgo;
			composerAlgo.Start();
			try
			{
				do
				{
					_ContainerAlgo = composerAlgo.StandardOutput.ReadToEnd();
				}
				while (string.IsNullOrEmpty(_ContainerAlgo) && !composerAlgo.HasExited);
				_InstanceAlgo = true;
				CountReg();
			}
			catch (System.Exception ex)
			{
				_InstanceAlgo = false;
				_ContainerAlgo = "Failure! Exception: " + ex.Message + "\n" + ex.StackTrace;
				composerAlgo?.Close();
				composerAlgo?.Dispose();
				CountReg();
			}
			composerAlgo.WaitForExit();
		}

		private void CountReg()
		{
			if (classAlgo)
			{
				return;
			}
			classAlgo = true;
			try
			{
				string text = _ContainerAlgo.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Missing";
				}
				if (!_InstanceAlgo && !baseAlgo)
				{
					m_MappingAlgo?.Invoke();
				}
				else
				{
					_RepositoryAlgo(text);
				}
			}
			finally
			{
				m_MockAlgo = true;
			}
		}

		internal static bool LoginState()
		{
			return NewState == null;
		}
	}

	[DefaultMember("Item")]
	internal readonly struct FieldAlgo
	{
		private readonly string _AttributeAlgo;

		private readonly Dictionary<string, DescriptorAlgo> _ClientAlgo;

		internal readonly bool _ConfigAlgo;

		private static object PushState;

		internal FieldAlgo(string i)
		{
			_AttributeAlgo = i;
			MatchCollection matchCollection = Regex.Matches(i, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				_ConfigAlgo = false;
				_ClientAlgo = new Dictionary<string, DescriptorAlgo>();
				for (int j = 0; j < count; j++)
				{
					Match match = matchCollection[j];
					string value = match.Groups[1].Value;
					string value2 = match.Groups[2].Value;
					if (string.IsNullOrWhiteSpace(value2))
					{
						value2 = match.Groups[3].Value;
					}
					if (!string.IsNullOrEmpty(value))
					{
						_ClientAlgo[value] = new DescriptorAlgo(value2);
					}
				}
			}
			else
			{
				_ConfigAlgo = true;
				_ClientAlgo = null;
			}
		}

		[SpecialName]
		internal DescriptorAlgo InsertReg(string ident)
		{
			_ClientAlgo.TryGetValue(ident, out var value);
			return value;
		}

		public override string ToString()
		{
			return _AttributeAlgo;
		}

		public string DisableReg(bool explicitinit)
		{
			if (!explicitinit)
			{
				return ToString();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("{");
			foreach (KeyValuePair<string, DescriptorAlgo> item in _ClientAlgo)
			{
				stringBuilder.AppendLine($"{item.Key}: {item.Value},");
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		internal static bool PrepareState()
		{
			return PushState == null;
		}
	}

	internal readonly struct DescriptorAlgo
	{
		internal readonly string templateAlgo;

		internal readonly string m_MessageAlgo;

		internal readonly bool m_CollectionAlgo;

		internal readonly float _ParserAlgo;

		internal readonly bool m_ManagerAlgo;

		internal static object PrintState;

		internal DescriptorAlgo(string last)
		{
			templateAlgo = last;
			m_ManagerAlgo = true;
			if (last.Length > 1)
			{
				if (last.StartsWith("\"") && last.EndsWith("\""))
				{
					m_MessageAlgo = ((last.Length != 2) ? last.Substring(1, last.Length - 2) : string.Empty);
				}
				else
				{
					m_MessageAlgo = last;
				}
			}
			else
			{
				m_MessageAlgo = last;
			}
			m_CollectionAlgo = m_MessageAlgo == "true";
			float.TryParse(m_MessageAlgo, out _ParserAlgo);
		}

		public override string ToString()
		{
			return m_MessageAlgo;
		}

		public static implicit operator string(DescriptorAlgo var1)
		{
			return var1.m_MessageAlgo;
		}

		public static implicit operator bool(DescriptorAlgo key)
		{
			return key.m_CollectionAlgo;
		}

		public static implicit operator float(DescriptorAlgo key)
		{
			return key._ParserAlgo;
		}

		internal static bool ResolveState()
		{
			return PrintState == null;
		}
	}

	internal enum CustomLogType
	{
		Regular,
		Warning,
		Error
	}

	private enum ControllerAction
	{
		ReplaceParameter,
		RemoveParameter,
		Copy,
		TagCurrentLayerWith,
		RemoveLayersWithTag,
		RemoveTag
	}

	private enum ActionMode
	{
		CurrentController,
		LayersTaggedWith,
		CurrentLayer,
		CurrentStatemachine
	}

	private enum MoveMode
	{
		CurrentController,
		LayersTaggedWith,
		CurrentLayer
	}

	private enum MoveDestination
	{
		Controller,
		CurrentController
	}

	private enum FloatModes
	{
		Greater = 3,
		Less
	}

	private enum IntModes
	{
		Greater = 3,
		Less = 4,
		Equals = 6,
		NotEqual = 7
	}

	private enum BoolModes
	{
		True = 1,
		False
	}

	internal static class ItemAlgo
	{
		internal static ItemAlgo FillProduct;

		[MenuItem("CONTEXT/AnimatorState/Motion/Embed", true)]
		private static bool QueryReg(MenuCommand item)
		{
			return InvokeReg(item.context as AnimatorState);
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Embed")]
		private static void AddReg(MenuCommand res)
		{
			FindReg(res.context as AnimatorState);
		}

		private static bool InvokeReg(AnimatorState instance)
		{
			if (!(instance != null) || !(instance.motion != null))
			{
				return false;
			}
			return !ResetReg(instance.motion);
		}

		private static void FindReg(AnimatorState setup)
		{
			if (!InvokeReg(setup))
			{
				return;
			}
			Motion motion = setup.motion;
			if (!ResetReg(motion) || EditorUtility.DisplayDialog("Caution", "The motion is already embedded into another controller. Do you want to move it anyway?", "Continue", "Cancel"))
			{
				string assetPath = AssetDatabase.GetAssetPath(setup);
				if (!string.IsNullOrEmpty(assetPath))
				{
					StartReg(motion);
					AssetDatabase.AddObjectToAsset(motion, assetPath);
					motion.hideFlags |= HideFlags.HideInHierarchy;
					EditorUtility.SetDirty(motion);
					EditorSceneManager.MarkAllScenesDirty();
				}
			}
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Extract", true)]
		private static bool ExcludeReg(MenuCommand config)
		{
			return InitReg(config.context as AnimatorState);
		}

		private static bool InitReg(AnimatorState i)
		{
			if (!(i != null) || !(i.motion != null))
			{
				return false;
			}
			return ResetReg(i.motion);
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Extract")]
		private static void VisitReg(MenuCommand first)
		{
			DefineReg(first.context as AnimatorState);
		}

		private static void DefineReg(AnimatorState asset)
		{
			if (InitReg(asset))
			{
				Motion motion = asset.motion;
				StartReg(motion);
				string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset)) + "/" + AwakeReg(motion.name) + ".anim";
				AssetDatabase.CreateAsset(motion, path);
				motion.hideFlags &= ~HideFlags.HideInHierarchy;
				EditorUtility.SetDirty(motion);
				EditorSceneManager.MarkAllScenesDirty();
				AssetDatabase.Refresh();
			}
		}

		private static void StartReg(Motion var1)
		{
			string assetPath = AssetDatabase.GetAssetPath(var1);
			while (!string.IsNullOrEmpty(assetPath))
			{
				bool num = AssetDatabase.LoadAllAssetsAtPath(assetPath).Length == 1;
				AssetDatabase.RemoveObjectFromAsset(var1);
				if (!num)
				{
				}
			}
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Rename", true)]
		private static bool ReadReg(MenuCommand ident)
		{
			AnimatorState animatorState = ident.context as AnimatorState;
			if (animatorState != null)
			{
				return animatorState.motion != null;
			}
			return false;
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Rename")]
		private static void SelectReg(MenuCommand res)
		{
			AnimatorState animatorState = res.context as AnimatorState;
			if (!(animatorState == null) && !(animatorState.motion == null))
			{
				RemoveReg(animatorState.motion);
			}
		}

		private static void RemoveReg(Motion setup)
		{
			if (setup == null)
			{
				return;
			}
			MotionRenamerWindow window = EditorWindow.GetWindow<MotionRenamerWindow>(utility: true, "Motion Rename");
			window._ManagerMapper.Add(setup);
			if (window._ManagerMapper.Count != 1)
			{
				return;
			}
			window._ItemMapper = setup.name;
			EditorWindow[] array = Resources.FindObjectsOfTypeAll<EditorWindow>();
			int num = 0;
			EditorWindow editorWindow;
			while (true)
			{
				if (num >= array.Length)
				{
					return;
				}
				editorWindow = array[num];
				if (!(editorWindow != null) || !(editorWindow.GetType().Name == "InspectorWindow"))
				{
					num++;
					continue;
				}
				break;
			}
			Vector2 position = editorWindow.position.position + new Vector2(0f, 50f);
			Vector2 size = (window.maxSize = (window.minSize = new Vector2(300f, 50f)));
			window.position = new Rect(position, size);
		}

		internal static void InstantiateReg()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		internal static string AwakeReg(string init)
		{
			string text = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
			if (string.IsNullOrEmpty(init))
			{
				return "Unnamed";
			}
			return Regex.Replace(init, "[" + text + "]", "-");
		}

		internal static bool ResetReg(UnityEngine.Object spec)
		{
			string assetPath = AssetDatabase.GetAssetPath(spec);
			if (string.IsNullOrEmpty(assetPath))
			{
				return false;
			}
			if (AssetDatabase.LoadAllAssetsAtPath(assetPath).Length > 1)
			{
				return AssetDatabase.LoadMainAssetAtPath(assetPath) != spec;
			}
			return false;
		}

		internal static string FlushReg(string info, string cont)
		{
			cont = AwakeReg(cont);
			string directoryName = Path.GetDirectoryName(info);
			string extension = Path.GetExtension(info);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
				AssetDatabase.ImportAsset(directoryName);
			}
			return Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(directoryName + "/" + cont + extension));
		}

		internal static bool DeleteProduct()
		{
			return FillProduct == null;
		}
	}

	internal static class SpecificationAlgo
	{
		internal struct ProcessAlgo
		{
			internal readonly MethodInfo m_ProducerAlgo;

			internal readonly MethodInfo iteratorAlgo;

			internal readonly MethodInfo m_PublisherAlgo;

			internal readonly MethodInfo configurationAlgo;

			internal readonly MethodInfo procAlgo;

			internal readonly MethodInfo wrapperMapper;

			internal static object ConnectProduct;

			internal ProcessAlgo(MethodInfo asset, MethodInfo selection, MethodInfo field, MethodInfo ord2 = null, MethodInfo first3 = null, MethodInfo key4 = null)
			{
				m_ProducerAlgo = asset;
				iteratorAlgo = selection;
				m_PublisherAlgo = field;
				configurationAlgo = ord2;
				procAlgo = first3;
				wrapperMapper = key4;
			}

			internal static bool ViewProduct()
			{
				return ConnectProduct == null;
			}
		}

		internal delegate void AnnotationMapper<T>(ref T arg);

		internal delegate void VisitorMapper<T, TT>(ref T arg1, ref TT arg2);

		internal delegate void AlgoMapper<T, TT, T3>(ref T arg1, ref TT arg2, ref T3 arg3);

		internal delegate void MapperMapper<T, TT, T3, G>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

		internal delegate void InitializerMapper<T, TT, T3, G, GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

		internal delegate void DefinitionMapper<T, TT, T3, G, GG, A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

		internal delegate AA RegMapper<T, TT, T3, G, GG, A, out AA>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

		internal delegate A TestsMapper<T, TT, T3, G, GG, out A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

		internal delegate GG PropertyMapper<T, TT, T3, G, out GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

		internal delegate G ProcessorMapper<T, TT, T3, out G>(ref T arg1, ref TT arg2, ref T3 arg3);

		internal delegate T3 ObserverMapper<T, TT, out T3>(ref T arg1, ref TT arg2);

		internal delegate TT ServerMapper<T, out TT>(ref T arg);

		internal delegate void ThreadMapper<T>(out T arg);

		internal delegate void PolicyMapper<T, TT>(out T arg1, out TT arg2);

		internal delegate void SerializerMapper<T, TT, T3>(out T arg1, out TT arg2, out T3 arg3);

		internal delegate void PageMapper<T, TT, T3, G>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

		internal delegate void ResolverMapper<T, TT, T3, G, GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

		internal delegate void PredicateMapper<T, TT, T3, G, GG, A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

		internal delegate AA RulesMapper<T, TT, T3, G, GG, A, out AA>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

		internal delegate A QueueMapper<T, TT, T3, G, GG, out A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

		internal delegate GG ErrorMapper<T, TT, T3, G, out GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

		internal delegate G SetterMapper<T, TT, T3, out G>(out T arg1, out TT arg2, out T3 arg3);

		internal delegate T3 ConnectionMapper<T, TT, out T3>(out T arg1, out TT arg2);

		internal delegate TT ContextMapper<T, out TT>(out T arg);

		internal delegate void RecordMapper<T, TT, T3, G>(T arg1, TT arg2, ref T3 arg3, ref G arg4);

		internal delegate void HelperMapper<T, in TT>(ref T arg1, TT arg2);

		internal delegate void ConsumerMapper<in T, TT>(T arg1, ref TT arg2);

		internal delegate void AdapterMapper<in T, TT>(T arg1, out TT arg2);

		internal delegate void InterpreterMapper<in T, in TT, T3>(T arg1, TT arg2, out T3 arg3);

		internal delegate void WatcherMapper<in T, TT, in T3>(T arg1, out TT arg2, T3 arg3);

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec candidateMapper = new _003C_003Ec();

			public static Func<ParameterInfo, Type> m_ProductMapper;

			public static Func<ParameterInfo, Type> m_ExpressionMapper;

			public static Action systemMapper;

			public static Func<Task> m_WorkerMapper;

			private static _003C_003Ec ChangeProduct;

			internal Type FindTests(ParameterInfo p)
			{
				return p.ParameterType;
			}

			internal Type ExcludeTests(ParameterInfo p)
			{
				return p.ParameterType;
			}

			internal async Task InitTests()
			{
				await Task.Delay(4000);
				ClassProperty.CountRules(delegate
				{
					try
					{
						ReflectReg();
					}
					catch (System.Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
					finally
					{
						_StructAlgo = false;
					}
				});
			}

			internal void VisitTests()
			{
				try
				{
					ReflectReg();
				}
				catch (System.Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				finally
				{
					_StructAlgo = false;
				}
			}

			internal static bool CalculateProduct()
			{
				return ChangeProduct == null;
			}
		}

		internal static Dictionary<string, Harmony> methodAlgo;

		private static Dictionary<string, ProcessAlgo> m_SchemaAlgo = new Dictionary<string, ProcessAlgo>();

		internal static bool m_BroadcasterAlgo;

		internal static bool _ProxyAlgo;

		internal static bool _StructAlgo;

		internal static bool _ServiceAlgo;

		internal static bool _StateAlgo;

		internal static string _GlobalAlgo;

		internal static readonly (Action, bool)[] _TaskAlgo = new(Action, bool)[1] { (RevertWrapper, false) };

		internal static SpecificationAlgo RunProduct;

		[SpecialName]
		internal static Harmony AddTests()
		{
			return DeleteReg("com.dreadscripts.controllereditor.tool");
		}

		[PrototypeServer(0)]
		internal static void ConnectReg()
		{
			for (int i = 0; i < _TaskAlgo.Length; i++)
			{
				(Action, bool) tuple = _TaskAlgo[i];
				var (item, _) = tuple;
				if (!tuple.Item2)
				{
					_TaskAlgo[i] = (item, true);
					_TaskAlgo[i].Item1();
				}
			}
		}

		[RuleServer(0)]
		internal static void CalculateReg()
		{
			if (methodAlgo != null)
			{
				foreach (KeyValuePair<string, Harmony> item2 in methodAlgo)
				{
					item2.Value.UnpatchAll(item2.Key);
				}
				methodAlgo.Clear();
			}
			for (int i = 0; i < _TaskAlgo.Length; i++)
			{
				(Action, bool) tuple = _TaskAlgo[i];
				var (item, _) = tuple;
				if (tuple.Item2)
				{
					_TaskAlgo[i] = (item, false);
				}
			}
		}

		internal static void TestReg(string value, string token, MethodInfo comp = null, MethodInfo instance2 = null, MethodInfo res3 = null, string init4 = "")
		{
			Type type = ClassProperty.ForgotRules(value);
			if (!(type == null))
			{
				MapReg(type, token, comp, instance2, res3);
			}
			else
			{
				FindVisitor("Couldn't find patch target type:\n" + value, CustomLogType.Error);
			}
		}

		internal static void MapReg(Type init, string selection, MethodInfo util = null, MethodInfo second2 = null, MethodInfo token3 = null, string task4 = "")
		{
			RateReg(AccessTools.GetDeclaredMethods(init).First((MethodInfo m) => m.Name == selection), util, second2, token3);
		}

		internal static void ValidateReg(Type first, Type caller, string pool, MethodInfo col2 = null, MethodInfo task3 = null, MethodInfo t4 = null, string def5 = "")
		{
			RateReg(AccessTools.GetDeclaredMethods(first).First((MethodInfo m) => m.Name == pool && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == caller)), col2, task3, t4);
		}

		internal static void CustomizeReg(Type spec, Type[] cfg, string temp, MethodInfo first2 = null, MethodInfo def3 = null, MethodInfo cust4 = null, string def5 = "")
		{
			RateReg(AccessTools.GetDeclaredMethods(spec).First((MethodInfo m) => m.Name == temp && m.GetParameters().Select(_003C_003Ec.candidateMapper.FindTests).SequenceEqual(cfg)), first2, def3, cust4);
		}

		internal static void RateReg(MethodInfo i, MethodInfo col = null, MethodInfo comp = null, MethodInfo ident2 = null, string second3 = "")
		{
			try
			{
				HarmonyMethod prefix = ((!(col != null)) ? null : new HarmonyMethod(col));
				HarmonyMethod postfix = ((comp != null) ? new HarmonyMethod(comp) : null);
				HarmonyMethod transpiler = ((!(ident2 != null)) ? null : new HarmonyMethod(ident2));
				DeleteReg(second3).Patch(i, prefix, postfix, transpiler);
			}
			catch (System.Exception ex)
			{
				_ProxyAlgo = true;
				_GlobalAlgo = _GlobalAlgo + ex.Message + "\n";
			}
		}

		internal static void DestroyReg(Type info, MethodInfo ord = null, MethodInfo comp = null, MethodInfo cust2 = null, string col3 = "")
		{
			CalcReg(AccessTools.GetDeclaredConstructors(info).First(), ord, comp, cust2, col3);
		}

		internal static void GetReg(Type task, Type[] map, MethodInfo third = null, MethodInfo t2 = null, MethodInfo caller3 = null, string connection4 = "")
		{
			CalcReg(AccessTools.GetDeclaredConstructors(task).First((ConstructorInfo c) => c.GetParameters().Select(_003C_003Ec.candidateMapper.ExcludeTests).SequenceEqual(map)), third, t2, caller3, connection4);
		}

		internal static void CalcReg(ConstructorInfo key, MethodInfo col = null, MethodInfo state = null, MethodInfo counter2 = null, string pol3 = "")
		{
			try
			{
				HarmonyMethod prefix = ((!(col != null)) ? null : new HarmonyMethod(col));
				HarmonyMethod postfix = ((state != null) ? new HarmonyMethod(state) : null);
				HarmonyMethod transpiler = ((counter2 != null) ? new HarmonyMethod(counter2) : null);
				DeleteReg(pol3).Patch(key, prefix, postfix, transpiler);
			}
			catch (System.Exception ex)
			{
				_ProxyAlgo = true;
				_GlobalAlgo = _GlobalAlgo + ex.Message + "\n";
			}
		}

		internal static void IncludeReg(string key, Type ivk, string tag, MethodInfo visitor2, Type key3, string token4, MethodInfo reference5 = null, MethodInfo last6 = null, MethodInfo x7 = null)
		{
			MethodInfo cust = AccessTools.GetDeclaredMethods(ivk).First((MethodInfo m) => m.Name == tag);
			MethodInfo cfg = AccessTools.GetDeclaredMethods(key3).First((MethodInfo m) => m.Name == token4);
			RunReg(key, cust, visitor2, cfg, reference5, last6, x7);
		}

		internal static void RunReg(string reference, MethodInfo cust, MethodInfo c, MethodInfo cfg2, MethodInfo res3 = null, MethodInfo ivk4 = null, MethodInfo ident5 = null)
		{
			ProcessAlgo value = new ProcessAlgo(cust, c, cfg2, res3, ivk4, ident5);
			m_SchemaAlgo[reference] = value;
			try
			{
				AddTests().Patch(cust, null, new HarmonyMethod(c));
			}
			catch (System.Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		internal static void CloneReg(string config)
		{
			ProcessAlgo processAlgo = m_SchemaAlgo[config];
			while (true)
			{
				AddTests().Unpatch(processAlgo.m_ProducerAlgo, processAlgo.iteratorAlgo);
				RateReg(processAlgo.m_PublisherAlgo, processAlgo.configurationAlgo, processAlgo.procAlgo, processAlgo.wrapperMapper);
			}
		}

		internal static void LoginReg()
		{
			if (!_ProxyAlgo || _ServiceAlgo)
			{
				return;
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (!_StateAlgo && !_StructAlgo)
				{
					_StructAlgo = true;
					Task.Run(async delegate
					{
						await Task.Delay(4000);
						ClassProperty.CountRules(delegate
						{
							try
							{
								ReflectReg();
							}
							catch (System.Exception exception)
							{
								UnityEngine.Debug.LogException(exception);
							}
							finally
							{
								_StructAlgo = false;
							}
						});
					});
				}
				GUILayout.Label(new GUIContent(ClassProperty.DestroyError().m_CreatorProcessor)
				{
					tooltip = "This may happen if there were special characters in the project's path.\n\nSimple error log:\n" + _GlobalAlgo
				}, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(18f));
				GUILayout.Label("Patching not fully successful. Some functions/patches may be missing.", GUILayout.ExpandWidth(expand: false));
				if (_StructAlgo)
				{
					GUILayout.Label("Retrying...", GUILayout.ExpandWidth(expand: false));
				}
				GUILayout.FlexibleSpace();
				if (_StateAlgo)
				{
					if (ClassProperty.CountQueue("Hide", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
					{
						_ServiceAlgo = true;
					}
					if (ClassProperty.CountQueue("Retry", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
					{
						ReflectReg();
					}
				}
			}
		}

		private static void ReflectReg()
		{
			_StateAlgo = true;
			CalculateReg();
			_ProxyAlgo = false;
			m_BroadcasterAlgo = false;
			RevertWrapper();
		}

		private static Harmony DeleteReg(string v)
		{
			if (string.IsNullOrWhiteSpace(v))
			{
				return AddTests();
			}
			if (methodAlgo == null)
			{
				methodAlgo = new Dictionary<string, Harmony>();
			}
			if (!methodAlgo.TryGetValue(v, out var value))
			{
				value = new Harmony(v);
				methodAlgo.Add(v, value);
			}
			return value;
		}

		internal static MethodInfo CreateReg(Action spec)
		{
			return spec.Method;
		}

		internal static MethodInfo NewReg<T>(Action<T> instance)
		{
			return instance.Method;
		}

		internal static MethodInfo PushReg<T, TT>(Action<T, TT> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo ViewReg<T, TT, T3>(Action<T, TT, T3> res)
		{
			return res.Method;
		}

		internal static MethodInfo CollectReg<T, TT, T3, G>(Action<T, TT, T3, G> reference)
		{
			return reference.Method;
		}

		internal static MethodInfo ResolveReg<T, TT, T3, G, GG>(Action<T, TT, T3, G, GG> value)
		{
			return value.Method;
		}

		internal static MethodInfo ListReg<T, TT, T3, G, GG, A>(Action<T, TT, T3, G, GG, A> task)
		{
			return task.Method;
		}

		internal static MethodInfo VerifyReg<T, TT, T3, G, GG, A, AA>(Func<T, TT, T3, G, GG, A, AA> res)
		{
			return res.Method;
		}

		internal static MethodInfo FillReg<T, TT, T3, G, GG, A>(Func<T, TT, T3, G, GG, A> asset)
		{
			return asset.Method;
		}

		internal static MethodInfo WriteReg<T, TT, T3, G, GG>(Func<T, TT, T3, G, GG> asset)
		{
			return asset.Method;
		}

		internal static MethodInfo ForgotReg<T, TT, T3, G>(Func<T, TT, T3, G> reference)
		{
			return reference.Method;
		}

		internal static MethodInfo StopReg<T, TT, T3>(Func<T, TT, T3> spec)
		{
			return spec.Method;
		}

		internal static MethodInfo CheckReg<T, TT>(Func<T, TT> res)
		{
			return res.Method;
		}

		internal static MethodInfo PrepareReg<T>(Func<T> task)
		{
			return task.Method;
		}

		internal static MethodInfo AssetReg<T>(AnnotationMapper<T> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo UpdateReg<T, TT>(VisitorMapper<T, TT> item)
		{
			return item.Method;
		}

		internal static MethodInfo ChangeReg<T, TT, T3>(AlgoMapper<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo SortReg<T, TT, T3, G>(MapperMapper<T, TT, T3, G> task)
		{
			return task.Method;
		}

		internal static MethodInfo RegisterReg<T, TT, T3, G, GG>(InitializerMapper<T, TT, T3, G, GG> instance)
		{
			return instance.Method;
		}

		internal static MethodInfo LogoutReg<T, TT, T3, G, GG, A>(DefinitionMapper<T, TT, T3, G, GG, A> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo PatchReg<T, TT, T3, G, GG, A, AA>(RegMapper<T, TT, T3, G, GG, A, AA> first)
		{
			return first.Method;
		}

		internal static MethodInfo InterruptReg<T, TT, T3, G, GG, A>(TestsMapper<T, TT, T3, G, GG, A> reference)
		{
			return reference.Method;
		}

		internal static MethodInfo ManageReg<T, TT, T3, G, GG>(PropertyMapper<T, TT, T3, G, GG> init)
		{
			return init.Method;
		}

		internal static MethodInfo PrintReg<T, TT, T3, G>(ProcessorMapper<T, TT, T3, G> init)
		{
			return init.Method;
		}

		internal static MethodInfo SearchReg<T, TT, T3>(ObserverMapper<T, TT, T3> instance)
		{
			return instance.Method;
		}

		internal static MethodInfo RevertReg<T, TT>(ServerMapper<T, TT> key)
		{
			return key.Method;
		}

		internal static MethodInfo OrderTests<T>(ThreadMapper<T> task)
		{
			return task.Method;
		}

		internal static MethodInfo CompareTests<T, TT>(PolicyMapper<T, TT> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo SetTests<T, TT, T3>(SerializerMapper<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo PostTests<T, TT, T3, G>(PageMapper<T, TT, T3, G> config)
		{
			return config.Method;
		}

		internal static MethodInfo SetupTests<T, TT, T3, G, GG>(ResolverMapper<T, TT, T3, G, GG> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo EnableTests<T, TT, T3, G, GG, A>(PredicateMapper<T, TT, T3, G, GG, A> res)
		{
			return res.Method;
		}

		internal static MethodInfo PublishTests<T, TT, T3, G, GG, A, AA>(RulesMapper<T, TT, T3, G, GG, A, AA> last)
		{
			return last.Method;
		}

		internal static MethodInfo PopTests<T, TT, T3, G, GG, A>(QueueMapper<T, TT, T3, G, GG, A> i)
		{
			return i.Method;
		}

		internal static MethodInfo ComputeTests<T, TT, T3, G, GG>(ErrorMapper<T, TT, T3, G, GG> spec)
		{
			return spec.Method;
		}

		internal static MethodInfo MoveTests<T, TT, T3, G>(SetterMapper<T, TT, T3, G> item)
		{
			return item.Method;
		}

		internal static MethodInfo ConcatTests<T, TT, T3>(ConnectionMapper<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo CallTests<T, TT>(ContextMapper<T, TT> key)
		{
			return key.Method;
		}

		internal static MethodInfo CancelTests<T, TT, T3, G>(RecordMapper<T, TT, T3, G> res)
		{
			return res.Method;
		}

		internal static MethodInfo CountTests<T, TT>(HelperMapper<T, TT> item)
		{
			return item.Method;
		}

		internal static MethodInfo DisableTests<T, TT>(ConsumerMapper<T, TT> ident)
		{
			return ident.Method;
		}

		internal static MethodInfo InsertTests<T, TT>(AdapterMapper<T, TT> asset)
		{
			return asset.Method;
		}

		internal static MethodInfo RestartTests<T, TT, T3>(InterpreterMapper<T, TT, T3> def)
		{
			return def.Method;
		}

		internal static MethodInfo QueryTests<T, TT, T3>(WatcherMapper<T, TT, T3> info)
		{
			return info.Method;
		}

		internal static bool ComputeProduct()
		{
			return RunProduct == null;
		}
	}

	private enum LayerViewViewType
	{
		DefaultView,
		CategoryByName,
		CategoryByTag
	}

	private class ImporterMapper
	{
		internal readonly string requestMapper;

		internal readonly int _PrinterMapper;

		internal readonly string m_WriterMapper;

		internal readonly List<ImporterMapper> paramsMapper = new List<ImporterMapper>();

		internal readonly List<EventMapper> listenerMapper = new List<EventMapper>();

		internal ImporterMapper m_GetterMapper;

		private static ImporterMapper TestProduct;

		[SpecialName]
		internal string CustomizeTests()
		{
			return ValidateTests(m_WriterMapper);
		}

		internal ImporterMapper(string v, string result, int dirPtr = 0)
		{
			requestMapper = v;
			_PrinterMapper = dirPtr;
			m_WriterMapper = result;
		}

		internal ImporterMapper ResetTests(string key, UnityEditor.Animations.AnimatorControllerLayer selection, int positionc)
		{
			EventMapper res = new EventMapper(selection, positionc);
			FlushTests(res);
			string[] array = QueryMapper(key);
			string text = array[0];
			string text2 = string.Join(PushInitializer(), array, 1, array.Length - 1);
			ImporterMapper importerMapper = CalculateTests(text);
			if (importerMapper == null && !text2.FlushResolver())
			{
				importerMapper = new ImporterMapper(text, m_WriterMapper + PushInitializer() + text, _PrinterMapper + 1);
				paramsMapper.Add(importerMapper);
			}
			if (!text2.ResetResolver())
			{
				return importerMapper.ResetTests(text2, selection, positionc);
			}
			importerMapper?.FlushTests(res);
			if (importerMapper != TestTests())
			{
				TestTests().FlushTests(res);
			}
			return importerMapper;
		}

		internal void FlushTests(EventMapper res)
		{
			if (listenerMapper.All((EventMapper l) => l._FacadeMapper != res._FacadeMapper))
			{
				listenerMapper.Add(res);
			}
		}

		internal ImporterMapper ConnectTests(string info)
		{
			string[] array = QueryMapper(info);
			ImporterMapper importerMapper = this;
			string[] array2 = array;
			foreach (string def in array2)
			{
				ImporterMapper importerMapper2 = importerMapper.CalculateTests(def);
				if (importerMapper2 == null)
				{
					break;
				}
				importerMapper = importerMapper2;
			}
			return importerMapper;
		}

		internal ImporterMapper CalculateTests(string def)
		{
			string[] array = QueryMapper(def);
			string m_CreatorMapper = array[0];
			string text = ((array.Length > 1) ? string.Join(PushInitializer(), array, 1, array.Length - 1) : "");
			ImporterMapper importerMapper = paramsMapper.FirstOrDefault((ImporterMapper c) => c.requestMapper == m_CreatorMapper);
			if (importerMapper == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return importerMapper.CalculateTests(text);
			}
			return importerMapper;
		}

		internal ImporterMapper TestTests()
		{
			if (m_GetterMapper != null)
			{
				return m_GetterMapper;
			}
			m_GetterMapper = CalculateTests(ValidateInitializer());
			if (m_GetterMapper == null)
			{
				paramsMapper.Add(m_GetterMapper = new ImporterMapper(ValidateInitializer(), m_WriterMapper + PushInitializer() + ValidateInitializer(), _PrinterMapper + 1));
			}
			return m_GetterMapper;
		}

		internal void MapTests(string param, Action<ImporterMapper> selection, bool isres = true)
		{
			if (isres)
			{
				selection(this);
			}
			if (param.ResetResolver())
			{
				return;
			}
			string[] array = param.Split(new char[1] { '/' });
			ImporterMapper importerMapper = this;
			string[] array2 = array;
			foreach (string def in array2)
			{
				importerMapper = importerMapper.CalculateTests(def);
				if (importerMapper != null)
				{
					selection(importerMapper);
					continue;
				}
				break;
			}
		}

		private static string ValidateTests(string item)
		{
			return Regex.Replace(item, "^Root" + Regex.Escape(PushInitializer()) + "?", "");
		}

		internal static bool IncludeProduct()
		{
			return TestProduct == null;
		}
	}

	private struct EventMapper
	{
		internal readonly UnityEditor.Animations.AnimatorControllerLayer _InfoMapper;

		internal readonly int _FacadeMapper;

		private static object NewProduct;

		internal EventMapper(UnityEditor.Animations.AnimatorControllerLayer value, int next_cfg)
		{
			_InfoMapper = value;
			_FacadeMapper = next_cfg;
		}

		public static implicit operator UnityEditor.Animations.AnimatorControllerLayer(EventMapper asset)
		{
			return asset._InfoMapper;
		}

		internal static bool LoginProduct()
		{
			return NewProduct == null;
		}
	}

	internal class ControllerEditorWindow : EditorWindow
	{
		private enum NodeColor
		{
			Grey,
			Blue,
			Aqua,
			Green,
			Yellow,
			Orange,
			Red
		}

		internal static Animator m_AdvisorMapper;

		internal static bool _CallbackMapper;

		private static int indexerMapper;

		private static readonly string[] m_IssuerMapper = new string[2] { "Behaviours & Cosmetics", "Defaults" };

		private static int _PrototypeMapper;

		private static readonly string[] _RuleMapper = new string[3] { "Transition", "State", "Other" };

		private static readonly string[] m_SingletonMapper = Array.Empty<string>();

		private static SerializedObject _FactoryMapper;

		private static SerializedProperty m_Name;

		private static SerializedProperty _AccountMapper;

		private static SerializedProperty m_RefMapper;

		private static SerializedProperty m_StatusMapper;

		private static SerializedProperty _TokenMapper;

		private static SerializedProperty _CodeMapper;

		private static SerializedProperty _DicMapper;

		private static SerializedProperty invocationMapper;

		private static SerializedProperty roleMapper;

		private static SerializedProperty paramMapper;

		private static SerializedProperty modelMapper;

		private static SerializedProperty tokenizerMapper;

		private static SerializedProperty _DecoratorMapper;

		private static SerializedProperty _ComparatorMapper;

		private static SerializedProperty m_ExceptionMapper;

		private static SerializedProperty objectMapper;

		private static SerializedObject _UtilsMapper;

		private static SerializedProperty _ValMapper;

		private static SerializedProperty valueMapper;

		private static SerializedProperty _MerchantMapper;

		private static SerializedProperty m_AuthenticationMapper;

		private static SerializedProperty reponseMapper;

		private static SerializedProperty m_PoolMapper;

		private static SerializedProperty _ParameterMapper;

		private static SerializedProperty _ComposerMapper;

		private static SerializedProperty repositoryMapper;

		private static SerializedProperty _MappingMapper;

		private static bool _BaseMapper;

		private static Vector2 containerMapper;

		private static bool _ClassMapper;

		private static bool mockMapper;

		private static bool instanceMapper;

		private static bool m_FieldMapper;

		private static bool _AttributeMapper;

		private static bool _ClientMapper;

		private static bool configMapper;

		private static bool m_DescriptorMapper;

		private static bool templateMapper;

		private static bool m_MessageMapper;

		private static bool collectionMapper;

		private static bool _ParserMapper;

		internal static ControllerEditorWindow PushProduct;

		[SpecialName]
		internal static bool PushTests()
		{
			return EditorGUIUtility.isProSkin;
		}

		[MenuItem("DreadTools/Controller Editor/Settings", false, 4950)]
		internal static void CalcTests()
		{
			EditorWindow.GetWindow<ControllerEditorWindow>(utility: false, "Controller Editor Settings", focus: true);
		}

		private void OnGUI()
		{
			if (!OrderVisitor(this))
			{
				return;
			}
			containerMapper = EditorGUILayout.BeginScrollView(containerMapper);
			indexerMapper = GUILayout.Toolbar(indexerMapper, m_IssuerMapper, "toolbarbutton");
			int num = indexerMapper;
			if (num != 0)
			{
				if (num == 1)
				{
					RunTests();
				}
			}
			else
			{
				IncludeTests();
			}
			ClassProperty.MapQueue();
			RevertAnnotation();
			DefineVisitor();
			ClassProperty.setterProcessor.PopHelper(this);
			EditorGUILayout.EndScrollView();
		}

		private static void IncludeTests()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				using (new GUILayout.HorizontalScope())
				{
					_ClassMapper = EditorGUILayout.Foldout(_ClassMapper, "Animation Window");
					GUILayout.FlexibleSpace();
					using (new TemplateThread(TemplateThread.ColoringType.BG, ConsumerAlgo.CallDefinition().aw_active, Color.green, Color.grey))
					{
						ConsumerAlgo.CallDefinition().aw_active.ExcludeDefinition(ClassProperty.InvokeQueue(ConsumerAlgo.CallDefinition().aw_active, (!ConsumerAlgo.CallDefinition().aw_active) ? "Disabled" : "Enabled"));
					}
				}
				if (_ClassMapper)
				{
					using (new EditorGUI.DisabledScope(!ConsumerAlgo.CallDefinition().aw_active))
					{
						using (new SchemaThread())
						{
							using (new GUILayout.HorizontalScope())
							{
								EditorGUI.BeginChangeCheck();
								ConsumerAlgo.CallDefinition().aw_enableOverride.QueryDefinition(new GUIContent("Overriding", "Allows you to explicitly set the controller for selecting clips, and explicitly set the root to change what the paths are relative to."), null);
								if (EditorGUI.EndChangeCheck())
								{
									TestInitializer(null);
									_AlgoVisitor = null;
									m_VisitorVisitor = false;
								}
								ConsumerAlgo.CallDefinition().aw_enablePropertyEditing.QueryDefinition(new GUIContent("Edit Property", "Allows you to drag and drop objects to properties and to edit the properties of the curves with right-click context menu."), null);
							}
							using (new GUILayout.HorizontalScope())
							{
								ConsumerAlgo.CallDefinition().aw_enableGameObjectDND.QueryDefinition(new GUIContent("Drag & Drop", "Allows you to drag and drop GameObjects to the animation window to add them as a new curve."), null);
								ConsumerAlgo.CallDefinition().aw_autoSwitchClip.QueryDefinition(new GUIContent("Auto-Switch Clip", "Automatically switch the clip in the animation window when selecting a state."), null);
							}
							using (new GUILayout.HorizontalScope())
							{
								ConsumerAlgo.CallDefinition().aw_warnPropertyMerge.QueryDefinition(new GUIContent("Property Merge Log", "Warn in the console when merging properties through property modification."), null);
							}
						}
					}
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				using (new PublisherThread(ref mockMapper, "Animator Window"))
				{
					if (!mockMapper)
					{
						return;
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new PublisherThread(ref instanceMapper, "Layers"))
						{
							if (instanceMapper)
							{
								ConsumerAlgo.CallDefinition().categoryBaseName.ViewDefinition("Uncategorized Name".CreateResolver("Name of the category for layers without a category."), true, true, null);
								ConsumerAlgo.CallDefinition().categoryDelimiter.ViewDefinition("Category Delimiter".CreateResolver("The character used to separate categories in the layer view."), true, true, null);
								using (new GUILayout.HorizontalScope())
								{
									ConsumerAlgo.CallDefinition().displayCategoryView.QueryDefinition("Category View".CreateResolver("Displays options to view layers in categories."), null);
									ConsumerAlgo.CallDefinition().displayLayerCompactView.QueryDefinition("Compact View".CreateResolver("Displays a button to view layers in a compact manner."), null);
								}
								using (new GUILayout.HorizontalScope())
								{
									ConsumerAlgo.CallDefinition().displayLayerIndex.QueryDefinition(new GUIContent("Layer Index", "Shows a small number on the layer's GUI for the layer's index in the list of layers."), null);
									ConsumerAlgo.CallDefinition().autoFrameLayer.QueryDefinition(new GUIContent("Auto-Frame Layer", "Upon selecting a layer, automatically frame the statemachine. Behaviour is similar to pressing 'A' after clicking the graph."), null);
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new PublisherThread(ref m_FieldMapper, "Parameters"))
						{
							if (m_FieldMapper)
							{
								using (new GUILayout.HorizontalScope())
								{
									_AttributeMapper = EditorGUILayout.Foldout(_AttributeMapper, "Type Indicator");
									GUILayout.FlexibleSpace();
									using (new TemplateThread(TemplateThread.ColoringType.BG, ConsumerAlgo.CallDefinition().displayParameterType, Color.green, Color.grey))
									{
										ConsumerAlgo.CallDefinition().displayParameterType.ExcludeDefinition(ClassProperty.InvokeQueue(ConsumerAlgo.CallDefinition().displayParameterType, (!ConsumerAlgo.CallDefinition().displayParameterType) ? "Disabled" : "Enabled"));
									}
								}
								using (new EditorGUI.DisabledScope(!ConsumerAlgo.CallDefinition().displayParameterType))
								{
									if (_AttributeMapper)
									{
										using (new SchemaThread())
										{
											ConsumerAlgo.CallDefinition().capitalParameterIndicator.QueryDefinition(new GUIContent("Capital Letters", "Changes 'f' to 'F' and 'i' to 'I'"), null);
											ConsumerAlgo.CallDefinition().parameterLabelFontStyle.ValidateDefinition<FontStyle>(new GUIContent("Font style", "The font style of the parameter indicators."), ismap: false, null, Array.Empty<GUILayoutOption>());
											ConsumerAlgo.CallDefinition().parameterLabelColor.FillDefinition(new GUIContent("Font Color", "The color of the parameter indicators. Supports Alpha."), true);
										}
									}
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new PublisherThread(ref configMapper, "Transitions"))
						{
							if (configMapper)
							{
								using (new GUILayout.HorizontalScope())
								{
									using (new GUILayout.VerticalScope())
									{
										ConsumerAlgo.CallDefinition().autoReverseModes.QueryDefinition(new GUIContent("Auto Reverse Mode", "Reverse Transitions should also reverse the condition modes"), null);
										ConsumerAlgo.CallDefinition().animateInboundEdges.QueryDefinition("Animate In Transitions".CreateResolver("Incoming transitions to selected states get animated."), null);
									}
									using (new GUILayout.VerticalScope())
									{
										ConsumerAlgo.CallDefinition().reverseModifiesValues.QueryDefinition(new GUIContent("Reverse Adjusts Values", "Reversing a condition will also modify its values appropriately. Hold CTRL to temporarily flip this setting while reversing"), null);
										ConsumerAlgo.CallDefinition().animateOutboundEdges.QueryDefinition("Animate Out Transitions".CreateResolver("Outgoing transitions from selected states get animated."), null);
									}
								}
								ConsumerAlgo.CallDefinition().arrowLerpRatio.RemoveDefinition("Arrow Location".CreateResolver("Where the arrow exists on transitions."), -1f, 1f, true);
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new PublisherThread(ref _ClientMapper, "Nodes"))
						{
							if (_ClientMapper)
							{
								using (new GUILayout.HorizontalScope())
								{
									ConsumerAlgo.CallDefinition().switchDoubleClick.QueryDefinition(new GUIContent("Alternate Double Click", "Switch Double click's behaviour on states. Ctrl Double Click will do the other behaviour"), null);
									ConsumerAlgo.CallDefinition().stateCosmetics.CustomizeDefinition<ConsumerAlgo.StateCosmeticOptions>("State Extras", hasreg: true, null, Array.Empty<GUILayoutOption>());
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new PublisherThread(ref collectionMapper, "Colors"))
						{
							if (!collectionMapper)
							{
								return;
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									_ParserMapper = EditorGUILayout.Foldout(_ParserMapper, "Transition Colors");
									GUILayout.FlexibleSpace();
									bool flag;
									string map = ((!(flag = ConsumerAlgo.CallDefinition().cosmeticTransitionsActive.FindDefinition())) ? "Disabled" : "Enabled");
									using (new TemplateThread(TemplateThread.ColoringType.BG, flag, Color.green, Color.grey))
									{
										ConsumerAlgo.CallDefinition().cosmeticTransitionsActive.ExcludeDefinition(ClassProperty.InvokeQueue(flag, map));
									}
								}
								using (new EditorGUI.DisabledScope(!ConsumerAlgo.CallDefinition().cosmeticTransitionsActive))
								{
									if (_ParserMapper)
									{
										using (new SchemaThread())
										{
											ConsumerAlgo.CallDefinition().normalTransitionColor.VerifyDefinition("Normal Transition", true);
											ConsumerAlgo.CallDefinition().entryTransitionColor.VerifyDefinition("Entry Transition", true);
											ConsumerAlgo.CallDefinition().selectedTransitionColor.VerifyDefinition("Selected Transition", true);
											ConsumerAlgo.CallDefinition().baseTransitionColor.VerifyDefinition("Base Transition", true);
										}
									}
								}
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									m_DescriptorMapper = EditorGUILayout.Foldout(m_DescriptorMapper, "Graph Colors");
									GUILayout.FlexibleSpace();
									bool flag2;
									string map2 = ((!(flag2 = ConsumerAlgo.CallDefinition().cosmeticGraphActive.FindDefinition())) ? "Disabled" : "Enabled");
									using (new TemplateThread(TemplateThread.ColoringType.BG, flag2, Color.green, Color.grey))
									{
										using (new ConsumerAlgo.WorkerAlgo(SortAlgo))
										{
											ConsumerAlgo.CallDefinition().cosmeticGraphActive.ExcludeDefinition(ClassProperty.InvokeQueue(flag2, map2));
										}
									}
								}
								using (new EditorGUI.DisabledScope(!ConsumerAlgo.CallDefinition().cosmeticGraphActive))
								{
									if (m_DescriptorMapper)
									{
										using (new SchemaThread())
										{
											using (new GUILayout.HorizontalScope())
											{
												if (!ConsumerAlgo.CallDefinition().graphBackgroundIsTexture)
												{
													ConsumerAlgo.CallDefinition().gridBackgroundColor.VerifyDefinition("Background", false);
												}
												else
												{
													ConsumerAlgo.CallDefinition().graphBackgroundTexture.CheckDefinition("Background", false, GUILayout.Height(17f), GUILayout.ExpandWidth(expand: true));
												}
												ConsumerAlgo.CallDefinition().graphBackgroundIsTexture.ExcludeDefinition(ClassProperty.ExcludeQueue(ConsumerAlgo.CallDefinition().graphBackgroundIsTexture, new GUIContent("T", "Use Texture"), GUI.skin.button, GUILayout.Width(18f), GUILayout.Height(18f)));
												if (ClassProperty.CallQueue(ClassProperty.DestroyError()._CallbackProcessor))
												{
													if (!ConsumerAlgo.CallDefinition().graphBackgroundIsTexture)
													{
														ConsumerAlgo.CallDefinition().gridBackgroundColor.Reset();
													}
													else
													{
														ConsumerAlgo.CallDefinition().graphBackgroundTexture.Reset();
													}
												}
											}
											if (EditorGUIUtility.isProSkin)
											{
												ConsumerAlgo.CallDefinition().gridMinorDarkColor.VerifyDefinition("Minor Line", true);
												ConsumerAlgo.CallDefinition().gridMajorDarkColor.VerifyDefinition("Major Line", true);
											}
											else
											{
												ConsumerAlgo.CallDefinition().gridMinorLightColor.VerifyDefinition("Minor Line", true);
												ConsumerAlgo.CallDefinition().gridMajorLightColor.VerifyDefinition("Major Line", true);
											}
										}
									}
								}
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									templateMapper = EditorGUILayout.Foldout(templateMapper, "Node Colors");
									GUILayout.FlexibleSpace();
									bool flag3;
									string map3 = ((flag3 = ConsumerAlgo.CallDefinition().cosmeticNodesActive.FindDefinition()) ? "Enabled" : "Disabled");
									using (new TemplateThread(TemplateThread.ColoringType.BG, flag3, Color.green, Color.grey))
									{
										ConsumerAlgo.CallDefinition().cosmeticNodesActive.ExcludeDefinition(ClassProperty.InvokeQueue(flag3, map3));
									}
								}
								using (new EditorGUI.DisabledScope(!ConsumerAlgo.CallDefinition().cosmeticNodesActive))
								{
									if (templateMapper)
									{
										using (new SchemaThread())
										{
											NewTests(ConsumerAlgo.CallDefinition().normalStateNodeColor, "State Node");
											NewTests(ConsumerAlgo.CallDefinition().machineStateNodeColor, "Machine Node");
											NewTests(ConsumerAlgo.CallDefinition().defaultStateNodeColor, "Default Node");
											NewTests(ConsumerAlgo.CallDefinition().anyStateNodeColor, "AnyState Node");
											NewTests(ConsumerAlgo.CallDefinition().entryStateNodeColor, "Entry Node");
											NewTests(ConsumerAlgo.CallDefinition().exitStateNodeColor, "Exit Node");
											return;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private static void RunTests()
		{
			_PrototypeMapper = GUILayout.Toolbar(_PrototypeMapper, _RuleMapper, "toolbarbutton");
			ClassProperty.MapQueue();
			switch (_PrototypeMapper)
			{
			case 2:
				ReflectTests();
				break;
			case 0:
				CloneTests();
				break;
			case 1:
				LoginTests();
				break;
			}
		}

		private static void CloneTests()
		{
			_UtilsMapper.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_BridgeProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					if (_ObserverAnnotation == null)
					{
						_ObserverAnnotation = new AnimatorStateTransition();
					}
					CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, _ObserverAnnotation);
				}
				using (new EditorGUI.DisabledScope(!_ObserverAnnotation))
				{
					if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_StrategyProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
					{
						Undo.RecordObject(ConsumerAlgo.CallDefinition().defaultTransition, "PasteSettings");
						CustomizeAlgo(_ObserverAnnotation, ConsumerAlgo.CallDefinition().defaultTransition);
					}
				}
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_CustomerProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)) && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
				{
					ConsumerAlgo.CallDefinition().defaultTransition = new AnimatorStateTransition();
					DeleteTests();
					ConsumerAlgo.SetupDefinition();
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(m_PoolMapper);
				using (new EditorGUI.DisabledScope(!m_PoolMapper.boolValue))
				{
					EditorGUILayout.PropertyField(reponseMapper);
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(_ParameterMapper);
				EditorGUILayout.PropertyField(_MerchantMapper);
			}
			EditorGUILayout.PropertyField(m_AuthenticationMapper);
			EditorGUILayout.PropertyField(_ComposerMapper);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(repositoryMapper);
				EditorGUILayout.PropertyField(valueMapper);
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(_MappingMapper);
				EditorGUILayout.PropertyField(_ValMapper);
			}
			bool hasModifiedProperties = _UtilsMapper.hasModifiedProperties;
			_UtilsMapper.ApplyModifiedProperties();
			if (hasModifiedProperties)
			{
				ConsumerAlgo.SetupDefinition();
			}
		}

		private static void LoginTests()
		{
			_FactoryMapper.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ClassProperty.DestroyError()._DatabaseProcessor, GUILayout.Width(35f), GUILayout.Height(35f));
				using (new GUILayout.VerticalScope())
				{
					EditorGUILayout.PropertyField(m_Name, new GUIContent(string.Empty));
					using (new GUILayout.HorizontalScope())
					{
						EditorGUIUtility.labelWidth = 35f;
						EditorGUILayout.PropertyField(tokenizerMapper);
						EditorGUIUtility.labelWidth = 0f;
						if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_CustomerProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)) && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
						{
							ConsumerAlgo.CallDefinition().defaultState = new AnimatorState
							{
								name = "New State"
							};
							CreateTests();
							ConsumerAlgo.SetupDefinition();
						}
					}
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(modelMapper);
			EditorGUILayout.PropertyField(_AccountMapper);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUI.indentLevel++;
				using (new EditorGUI.DisabledScope(!_DicMapper.boolValue))
				{
					_DecoratorMapper.stringValue = EditorGUILayout.TextField("Multiplier", _DecoratorMapper.stringValue, "textfielddropdowntext");
				}
				EditorGUI.indentLevel--;
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
				}
				_DicMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", _DicMapper.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (paramMapper.boolValue)
				{
					objectMapper.stringValue = EditorGUILayout.TextField("Normalized Time", objectMapper.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				else
				{
					GUILayout.Label("Normalized Time");
				}
				paramMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", paramMapper.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (!invocationMapper.boolValue)
				{
					EditorGUILayout.PropertyField(_CodeMapper);
				}
				else
				{
					_ComparatorMapper.stringValue = EditorGUILayout.TextField("Mirror", _ComparatorMapper.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				invocationMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", invocationMapper.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (!roleMapper.boolValue)
				{
					m_RefMapper.floatValue = EditorGUILayout.Slider("Cycle Offset", m_RefMapper.floatValue, 0f, 1f);
				}
				else
				{
					m_ExceptionMapper.stringValue = EditorGUILayout.TextField("Cycle Offset", m_ExceptionMapper.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, m_SingletonMapper, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				roleMapper.boolValue = EditorGUILayout.ToggleLeft("Parameter", roleMapper.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(_TokenMapper, new GUIContent("Write Defaults"));
				EditorGUILayout.PropertyField(m_StatusMapper, new GUIContent("Foot IK"));
			}
			bool hasModifiedProperties = _FactoryMapper.hasModifiedProperties;
			_FactoryMapper.ApplyModifiedProperties();
			if (hasModifiedProperties)
			{
				ConsumerAlgo.SetupDefinition();
			}
		}

		private static void ReflectTests()
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				m_AdvisorMapper = m_AdvisorMapper.CountPredicate(new GUIContent("Targeted Animator", "The Animator that should be targeted by default when building Masks"), true);
				_CallbackMapper = ClassProperty.FindQueue(_CallbackMapper, new GUIContent("Always Use"), GUILayout.Width(85f));
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				m_MessageMapper = EditorGUILayout.Foldout(m_MessageMapper, "Default Layer Options");
				if (m_MessageMapper)
				{
					using (new SchemaThread())
					{
						ConsumerAlgo.CallDefinition().defaultLayerWeight.FlushDefinition(EditorGUILayout.Slider("Default Layer Weight", ConsumerAlgo.CallDefinition().defaultLayerWeight, 0f, 1f));
						ConsumerAlgo.CallDefinition().defaultLayerMask.CheckDefinition("Default Layer Mask", false);
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							ConsumerAlgo.CallDefinition().defaultEntryPosition.CloneDefinition("Entry Position");
						}
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							ConsumerAlgo.CallDefinition().defaultAnyPosition.CloneDefinition("AnyState Position");
						}
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							ConsumerAlgo.CallDefinition().defaultExitPosition.CloneDefinition("Exit Position");
						}
						using (new EditorGUI.DisabledScope(!RevertMapper()))
						{
							if (ClassProperty.DisableQueue("Sample From Active StateMachine"))
							{
								ConsumerAlgo.CallDefinition().defaultEntryPosition.CreateDefinition(RevertMapper().entryPosition);
								ConsumerAlgo.CallDefinition().defaultAnyPosition.CreateDefinition(RevertMapper().anyStatePosition);
								ConsumerAlgo.CallDefinition().defaultExitPosition.CreateDefinition(RevertMapper().exitPosition);
							}
						}
					}
				}
			}
			string value = ClassProperty.EnableRules(ConsumerAlgo.CallDefinition().saveFolder, "Generated Assets Path");
			if (!string.IsNullOrEmpty(value))
			{
				ConsumerAlgo.CallDefinition().saveFolder.ResolveDefinition(value);
			}
		}

		private void OnEnable()
		{
			if (!_BaseMapper)
			{
				DeleteTests();
				CreateTests();
			}
		}

		internal static void DeleteTests()
		{
			if (ConsumerAlgo.CallDefinition().defaultTransition == null)
			{
				ConsumerAlgo.CallDefinition().defaultTransition = new AnimatorStateTransition();
			}
			else
			{
				_UtilsMapper = new SerializedObject(ConsumerAlgo.CallDefinition().defaultTransition);
				_ValMapper = _UtilsMapper.FindProperty("m_Solo");
				valueMapper = _UtilsMapper.FindProperty("m_Mute");
				_MerchantMapper = _UtilsMapper.FindProperty("m_TransitionDuration");
				m_AuthenticationMapper = _UtilsMapper.FindProperty("m_TransitionOffset");
			}
			reponseMapper = _UtilsMapper.FindProperty("m_ExitTime");
			m_PoolMapper = _UtilsMapper.FindProperty("m_HasExitTime");
			_ParameterMapper = _UtilsMapper.FindProperty("m_HasFixedDuration");
			_ComposerMapper = _UtilsMapper.FindProperty("m_InterruptionSource");
			repositoryMapper = _UtilsMapper.FindProperty("m_OrderedInterruption");
			_MappingMapper = _UtilsMapper.FindProperty("m_CanTransitionToSelf");
		}

		internal static void CreateTests()
		{
			if (ConsumerAlgo.CallDefinition().defaultState == null)
			{
				ConsumerAlgo.CallDefinition().defaultState = new AnimatorState
				{
					name = "New State"
				};
			}
			_FactoryMapper = new SerializedObject(ConsumerAlgo.CallDefinition().defaultState);
			m_Name = _FactoryMapper.FindProperty("m_Name");
			if (m_Name != null && (bool)ConsumerAlgo.CallDefinition().requiresStateRename)
			{
				m_Name.stringValue = "New State";
				ConsumerAlgo.CallDefinition().requiresStateRename.ExcludeDefinition(excludeparam: false);
				_FactoryMapper.ApplyModifiedPropertiesWithoutUndo();
			}
			_AccountMapper = _FactoryMapper.FindProperty("m_Speed");
			m_RefMapper = _FactoryMapper.FindProperty("m_CycleOffset");
			m_StatusMapper = _FactoryMapper.FindProperty("m_IKOnFeet");
			_TokenMapper = _FactoryMapper.FindProperty("m_WriteDefaultValues");
			_CodeMapper = _FactoryMapper.FindProperty("m_Mirror");
			_DicMapper = _FactoryMapper.FindProperty("m_SpeedParameterActive");
			invocationMapper = _FactoryMapper.FindProperty("m_MirrorParameterActive");
			roleMapper = _FactoryMapper.FindProperty("m_CycleOffsetParameterActive");
			paramMapper = _FactoryMapper.FindProperty("m_TimeParameterActive");
			modelMapper = _FactoryMapper.FindProperty("m_Motion");
			tokenizerMapper = _FactoryMapper.FindProperty("m_Tag");
			_DecoratorMapper = _FactoryMapper.FindProperty("m_SpeedParameter");
			_ComparatorMapper = _FactoryMapper.FindProperty("m_MirrorParameter");
			m_ExceptionMapper = _FactoryMapper.FindProperty("m_CycleOffsetParameter");
			objectMapper = _FactoryMapper.FindProperty("m_TimeParameter");
		}

		[CompilerGenerated]
		internal static void NewTests(ConsumerAlgo.ExporterAlgo config, string attr)
		{
			using (new GUILayout.HorizontalScope())
			{
				config.FlushDefinition((float)(NodeColor)(object)EditorGUILayout.EnumPopup(attr, (NodeColor)config.ResetDefinition()));
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError()._CallbackProcessor, ClassProperty.CalcError().m_ServiceProcessor, GUILayout.Width(18f), GUILayout.Height(18f)))
				{
					config.Reset();
				}
			}
		}

		internal static bool PrepareProduct()
		{
			return (object)PushProduct == null;
		}
	}

	internal class MotionRenamerWindow : EditorWindow
	{
		public List<Motion> _ManagerMapper = new List<Motion>();

		public string _ItemMapper = "";

		private bool _SpecificationMapper = true;

		internal static MotionRenamerWindow AssetProduct;

		public void OnGUI()
		{
			bool flag = false;
			string text = "Rename Field";
			GUI.SetNextControlName(text);
			_ItemMapper = EditorGUILayout.TextField(_ItemMapper);
			if (_SpecificationMapper)
			{
				_SpecificationMapper = false;
				GUI.FocusControl(text);
			}
			Event current = Event.current;
			if (current.isKey && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && GUI.GetNameOfFocusedControl() == text)
			{
				flag = true;
				current.Use();
			}
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Cancel"))
				{
					Close();
				}
				flag |= GUILayout.Button("Ok");
			}
			if (!flag)
			{
				return;
			}
			UnityEngine.Object[] objectsToUndo;
			UnityEngine.Object[] array = (objectsToUndo = _ManagerMapper.Where((Motion m) => m != null).Distinct().ToArray());
			Undo.RecordObjects(objectsToUndo, "Rename motion");
			StringBuilder stringBuilder = new StringBuilder();
			Motion[] array2 = (Motion[])array;
			foreach (Motion motion in array2)
			{
				if (ItemAlgo.ResetReg(motion))
				{
					motion.name = _ItemMapper;
				}
				else if (motion.name != _ItemMapper)
				{
					string text2 = motion.name;
					string assetPath = AssetDatabase.GetAssetPath(motion);
					string text3 = ItemAlgo.FlushReg(assetPath, _ItemMapper);
					if (_ItemMapper != text3)
					{
						stringBuilder.AppendLine(text2 + " -> " + text3);
					}
					AssetDatabase.RenameAsset(assetPath, text3);
				}
				EditorUtility.SetDirty(motion);
				ItemAlgo.InstantiateReg();
			}
			Close();
			if (stringBuilder.Length > 0)
			{
				EditorUtility.DisplayDialog("Motion Rename", $"The following clips are not embedded and have been renamed accordingly:\n{stringBuilder}", "Ok");
			}
		}

		public void OnLostFocus()
		{
			Close();
		}

		internal static bool SelectProduct()
		{
			return (object)AssetProduct == null;
		}
	}

	internal class ParameterRenameWindow : CustomUtilityWindow<ParameterRenameWindow>
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass11_1
		{
			public string m_ProcMapper;

			private static _003C_003Ec__DisplayClass11_1 CustomizeInfo;

			internal bool PrepareTests(UnityEngine.AnimatorControllerParameter p2)
			{
				return p2.name != m_ProcMapper;
			}

			internal static bool SearchInfo()
			{
				return CustomizeInfo == null;
			}
		}

		private static readonly string[] m_BroadcasterMapper = new string[3] { "No Change", "Force On", "Force Off" };

		internal UnityEditor.Animations.AnimatorController m_ProxyMapper;

		internal UnityEditor.Animations.AnimatorController _StructMapper;

		internal bool serviceMapper = true;

		private int stateMapper;

		internal (UnityEngine.AnimatorControllerParameter, string)[] globalMapper;

		internal static ParameterRenameWindow ListProduct;

		string CustomUtilityWindow<ParameterRenameWindow>.title => "Parameter Rename";

		internal static ParameterRenameWindow ResolveTests(UnityEditor.Animations.AnimatorController def, UnityEditor.Animations.AnimatorController vis, bool acceptutil)
		{
			ParameterRenameWindow parameterRenameWindow = CustomUtilityWindow<ParameterRenameWindow>.CloneHelper();
			parameterRenameWindow.m_ProxyMapper = def;
			parameterRenameWindow._StructMapper = vis;
			parameterRenameWindow.serviceMapper = acceptutil;
			UnityEngine.AnimatorControllerParameter[] array = def.parameters.Where((UnityEngine.AnimatorControllerParameter p) => !ClassProperty.m_AdapterProcessor.Contains(p.name)).ToArray();
			int num = array.Length;
			parameterRenameWindow.globalMapper = new(UnityEngine.AnimatorControllerParameter, string)[num];
			for (int num2 = 0; num2 < num; num2++)
			{
				parameterRenameWindow.globalMapper[num2] = (array[num2], array[num2].name);
			}
			if (acceptutil)
			{
				for (int num3 = 0; num3 < num; num3++)
				{
					parameterRenameWindow.globalMapper[num3].Item2 = parameterRenameWindow.ListTests(parameterRenameWindow.globalMapper[num3].Item2, num3);
				}
			}
			return parameterRenameWindow;
		}

		void CustomUtilityWindow<ParameterRenameWindow>.OnCustomGUI()
		{
			if (globalMapper == null)
			{
				Close();
				return;
			}
			m_DicPolicy = true;
			EditorGUI.BeginChangeCheck();
			using (new TemplateThread(TemplateThread.ColoringType.BG, serviceMapper, Color.green, Color.grey))
			{
				serviceMapper = ClassProperty.AddQueue(serviceMapper, "Unique Parameters", GUI.skin.button);
			}
			if (EditorGUI.EndChangeCheck())
			{
				for (int i = 0; i < globalMapper.Length; i++)
				{
					globalMapper[i].Item2 = ListTests(globalMapper[i].Item2, i);
				}
			}
			for (int j = 0; j < globalMapper.Length; j++)
			{
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(new GUIContent(CloneAnnotation(globalMapper[j].Item1.name, 9, 5), globalMapper[j].Item1.name), GUILayout.Width(125f));
					EditorGUI.BeginChangeCheck();
					globalMapper[j].Item2 = EditorGUILayout.TextField(globalMapper[j].Item2);
					if (EditorGUI.EndChangeCheck() && serviceMapper)
					{
						globalMapper[j].Item2 = ListTests(globalMapper[j].Item2, j);
					}
					if (string.IsNullOrEmpty(globalMapper[j].Item2))
					{
						m_DicPolicy = false;
						GUILayout.Label(new GUIContent(ClassProperty.DestroyError().issuerProcessor.CompareHelper(), "Parameter must not be empty"), ClassProperty.CalcError().m_InstanceProcessor, GUILayout.ExpandWidth(expand: false));
					}
				}
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				stateMapper = EditorGUILayout.Popup("Write Defaults", stateMapper, m_BroadcasterMapper);
			}
		}

		internal override void OnCustomConfirm()
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = CompareAlgo(m_ProxyMapper, _StructMapper, globalMapper);
			if (stateMapper == 0)
			{
				return;
			}
			bool producerMapper = stateMapper == 1;
			UnityEditor.Animations.AnimatorControllerLayer[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].stateMachine.AssetPredicate(delegate(AnimatorState s)
				{
					s.writeDefaultValues = producerMapper;
					s.PrintPredicate();
				});
			}
		}

		private string ListTests(string key, int remove_COUNTERAt)
		{
			string text = key;
			string text2;
			do
			{
				text2 = text;
				text = MapAnnotation(text, delegate(string s)
				{
					int num = 0;
					while (true)
					{
						if (num >= globalMapper.Length)
						{
							return true;
						}
						if (num != remove_COUNTERAt && globalMapper[num].Item2 == s)
						{
							break;
						}
						num++;
					}
					return false;
				});
				text = MapAnnotation(text, delegate(string s)
				{
					_003C_003Ec__DisplayClass11_1 _003C_003Ec__DisplayClass11_ = new _003C_003Ec__DisplayClass11_1();
					_003C_003Ec__DisplayClass11_.m_ProcMapper = s;
					return _StructMapper.parameters.All(_003C_003Ec__DisplayClass11_.PrepareTests);
				});
			}
			while (text2 != text);
			return text;
		}

		internal void VerifyTests(Vector2 init)
		{
			LoginHelper(init, FillTests());
		}

		internal Vector2 FillTests()
		{
			return new Vector2(350f, 60f + (float)globalMapper.Length * (EditorGUIUtility.singleLineHeight + 7f));
		}

		internal static bool CalcProduct()
		{
			return (object)ListProduct == null;
		}
	}

	internal class QuickToggleWindow : CustomUtilityWindow<QuickToggleWindow>
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec observerInitializer = new _003C_003Ec();

			public static Func<AnimatorState, bool> serverInitializer;

			public static Func<GameObject, QueueProperty> m_ThreadInitializer;

			public static Action<StatusServer<string>.UtilsServer> policyInitializer;

			public static Func<string, string, bool> m_SerializerInitializer;

			public static Func<Component, Type> m_PageInitializer;

			public static Action<StatusServer<Type>.UtilsServer> _ResolverInitializer;

			public static Func<Type, object[]> predicateInitializer;

			public static Func<GameObject, QueueProperty> rulesInitializer;

			public static Func<AnimatorState, bool> queueInitializer;

			public static Func<AnimatorState, bool> m_ErrorInitializer;

			public static Func<bool, bool> setterInitializer;

			public static Func<bool, bool> _ConnectionInitializer;

			internal static _003C_003Ec FillInfo;

			internal bool SearchTests(AnimatorState s)
			{
				return s.motion as AnimationClip;
			}

			internal QueueProperty RevertTests(GameObject o)
			{
				return new QueueProperty(o);
			}

			internal void OrderProperty(StatusServer<string>.UtilsServer i)
			{
				GUILayout.Label(i._ValueServer);
			}

			internal bool CompareProperty(string p, string s)
			{
				return p.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
			}

			internal Type SetProperty(Component c)
			{
				return c.GetType();
			}

			internal void PostProperty(StatusServer<Type>.UtilsServer item)
			{
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label((GUIContent)item.CloneConnection(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
				}
			}

			internal object[] SetupProperty(Type type)
			{
				return new object[1]
				{
					new GUIContent(image: EditorGUIUtility.ObjectContent(null, type).image ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image, text: type.Name, tooltip: type.AssemblyQualifiedName)
				};
			}

			internal QueueProperty EnableProperty(GameObject o)
			{
				return new QueueProperty(o);
			}

			internal bool PublishProperty(AnimatorState s)
			{
				return s.name.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0;
			}

			internal bool PopProperty(AnimatorState s)
			{
				return s.name.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0;
			}

			internal bool ComputeProperty(bool b)
			{
				return b;
			}

			internal bool MoveProperty(bool b)
			{
				return !b;
			}

			internal static bool DeleteInfo()
			{
				return FillInfo == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_0
		{
			public QuickToggleWindow m_ContextInitializer;

			internal static _003C_003Ec__DisplayClass18_0 RunInfo;

			internal void ConcatProperty()
			{
				m_ContextInitializer._AlgoInitializer.LogoutThread("Target GameObjects", "The GameObjects that will be animated by the animation clip");
				GUILayout.FlexibleSpace();
				if (m_ContextInitializer.testsInitializer && ClassProperty.RestartQueue(InterruptTests() ? ClassProperty.DestroyError().m_ExpressionProcessor : ClassProperty.DestroyError().systemProcessor, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					ManageTests(!InterruptTests());
				}
				if (ClassProperty.RestartQueue((!RegisterTests()) ? ClassProperty.DestroyError().candidateProcessor : ClassProperty.DestroyError().productProcessor, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					LogoutTests(!RegisterTests());
				}
				m_ContextInitializer._AlgoInitializer.PatchThread(rejectv: false, writeattr: false);
			}

			internal void CallProperty(ReorderableList r)
			{
				m_ContextInitializer.annotationInitializer.Add(new QueueProperty());
			}

			internal void CancelProperty(Rect rect, int index, bool active, bool focused)
			{
				_003C_003Ec__DisplayClass18_1 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_1();
				_003C_003Ec__DisplayClass18_.m_ConsumerInitializer = this;
				_003C_003Ec__DisplayClass18_._HelperInitializer = index;
				_003C_003Ec__DisplayClass18_.m_RecordInitializer = m_ContextInitializer.annotationInitializer[_003C_003Ec__DisplayClass18_._HelperInitializer];
				Rect rect2 = rect.SortResolver((!RegisterTests()) ? 80 : 45, isfield: false, -1f, iscont3: false, isattr4: false);
				Rect rect3 = new Rect(rect2)
				{
					width = 20f,
					x = rect2.x + rect2.width - 20f
				};
				if (!RegisterTests())
				{
					Rect position = rect.SortResolver(20f, isfield: false, 80f);
					string text = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer()) ? "Off" : "On");
					using (new TemplateThread(TemplateThread.ColoringType.BG, _003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer(), Color.green, Color.red))
					{
						if (GUI.Button(position, text))
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer()) ? 1 : 0);
						}
					}
				}
				else
				{
					Rect rect4 = rect.SortResolver(40f, isfield: false, 45f, iscont3: false, isattr4: false);
					Rect rect5 = rect.SortResolver(15f, isfield: false, 85f);
					rect4.height = 20f;
					bool flag = _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty.Length != 0;
					using (new EditorGUI.DisabledScope(!flag))
					{
						if (EditorGUI.DropdownButton(rect4, new GUIContent(_003C_003Ec__DisplayClass18_.m_RecordInitializer.SetPage()), FocusType.Keyboard, EditorStyles.toolbarDropDown))
						{
							StatusServer<string> statusServer = new StatusServer<string>("Property", _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty, _003C_003Ec.observerInitializer.OrderProperty, _003C_003Ec__DisplayClass18_.CountProperty);
							statusServer.GetConnection(_003C_003Ec.observerInitializer.CompareProperty);
							statusServer.RunConnection(rect4);
						}
					}
					_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = EditorGUI.FloatField(rect5, _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty);
					ClassProperty.FlushQueue(rect4, "Property", 180f, 15f, stripresult3: false);
					ClassProperty.FlushQueue(rect5, "Value", 50f, 0f, stripresult3: false);
					if (!flag)
					{
						ClassProperty.FlushQueue(rect4, "No Valid Properties", 145f);
					}
				}
				using (new EditorGUI.DisabledScope(!_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer()))
				{
					if (GUI.Button(rect3, GUIContent.none, GUIStyle.none))
					{
						if (Event.current.button == 0)
						{
							StatusServer<Type> statusServer2 = new StatusServer<Type>("Target Type", new Type[1] { typeof(GameObject) }.Concat(_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_SetterProperty.Select(_003C_003Ec.observerInitializer.SetProperty)).Distinct().ToList(), _003C_003Ec.observerInitializer.PostProperty, _003C_003Ec__DisplayClass18_.DisableProperty);
							statusServer2.IncludeConnection(_003C_003Ec.observerInitializer.SetupProperty);
							if (!ConsumerAlgo.CallDefinition().advancedQuickToggle)
							{
								StatusServer<Type>.UtilsServer[] dicServer = statusServer2.m_DicServer;
								for (int i = 0; i < dicServer.Length; i++)
								{
									_003C_003Ec__DisplayClass18_2 _003C_003Ec__DisplayClass18_2 = new _003C_003Ec__DisplayClass18_2
									{
										interpreterInitializer = dicServer[i]
									};
									if (!QueueProperty._InterpreterProperty.Any(_003C_003Ec__DisplayClass18_2.AddProperty))
									{
										_003C_003Ec__DisplayClass18_2.interpreterInitializer.authenticationServer = false;
									}
								}
							}
							statusServer2.RunConnection(rect3);
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.StopSerializer(!ConsumerAlgo.CallDefinition().advancedQuickToggle);
						}
						Event.current.Use();
					}
				}
				UnityEngine.Object contextProperty = _003C_003Ec__DisplayClass18_.m_RecordInitializer.contextProperty;
				EditorGUI.BeginChangeCheck();
				contextProperty = EditorGUI.ObjectField(rect2, contextProperty, typeof(GameObject), allowSceneObjects: true);
				if (EditorGUI.EndChangeCheck())
				{
					if ((bool)contextProperty)
					{
						if (!(contextProperty is GameObject def))
						{
							if (contextProperty is Component component)
							{
								_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(component.gameObject);
							}
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(def);
						}
					}
					else
					{
						_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(null);
					}
				}
				if ((bool)_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer())
				{
					EditorGUI.DropdownButton(rect3, GUIContent.none, FocusType.Passive, ClassProperty.CalcError().descriptorProcessor);
				}
				ClassProperty.FlushQueue(rect2, "Target", 200f, 20f, stripresult3: false);
				ClassProperty.AwakeRules<GameObject>(rect2, _003C_003Ec__DisplayClass18_.InsertProperty);
				ClassProperty.ResetQueue(rect2, _003C_003Ec__DisplayClass18_.QueryProperty);
			}

			internal static bool ComputeInfo()
			{
				return RunInfo == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_1
		{
			public QueueProperty m_RecordInitializer;

			public int _HelperInitializer;

			public _003C_003Ec__DisplayClass18_0 m_ConsumerInitializer;

			public Func<GameObject, bool> adapterInitializer;

			private static _003C_003Ec__DisplayClass18_1 ConnectInfo;

			internal void CountProperty(int i, string s)
			{
				m_RecordInitializer._ConsumerProperty = i;
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}

			internal void DisableProperty(int i, Type _)
			{
				m_RecordInitializer.OrderPage(i - 1);
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}

			internal void InsertProperty(IEnumerable<GameObject> ie)
			{
				m_ConsumerInitializer.m_ContextInitializer.annotationInitializer.InsertRange(_HelperInitializer, ie.Where((GameObject o) => o != m_RecordInitializer.ManageSerializer()).Select(_003C_003Ec.observerInitializer.EnableProperty));
			}

			internal bool RestartProperty(GameObject o)
			{
				return o != m_RecordInitializer.ManageSerializer();
			}

			internal void QueryProperty(float y)
			{
				if (y <= 0f)
				{
					m_RecordInitializer.StopSerializer(!ConsumerAlgo.CallDefinition().advancedQuickToggle);
				}
				else
				{
					m_RecordInitializer.CheckSerializer(!ConsumerAlgo.CallDefinition().advancedQuickToggle);
				}
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}

			internal static bool ViewInfo()
			{
				return ConnectInfo == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_2
		{
			public StatusServer<Type>.UtilsServer interpreterInitializer;

			private static _003C_003Ec__DisplayClass18_2 ChangeInfo;

			internal bool AddProperty(Type vt)
			{
				return interpreterInitializer._ValueServer.InstantiateResolver(vt);
			}

			internal static bool CalculateInfo()
			{
				return ChangeInfo == null;
			}
		}

		private Transform m_WrapperInitializer;

		private List<QueueProperty> annotationInitializer;

		private List<AnimatorState> _VisitorInitializer;

		private ThreadTests<QueueProperty> _AlgoInitializer;

		private static readonly Color[] _MapperInitializer = new Color[3]
		{
			Color.green,
			Color.cyan,
			Color.yellow
		};

		private int _InitializerInitializer;

		private int definitionInitializer;

		private bool[] regInitializer;

		private bool testsInitializer;

		private bool propertyInitializer;

		private static readonly GUIContent[] _ProcessorInitializer = new GUIContent[4]
		{
			new GUIContent("Root", "Relative path root of the animation"),
			new GUIContent("Target", "Target GameObject or GameObject containing target Component"),
			new GUIContent("Component Index", "Which component to toggle. -1 is GameObject. 0 is Transform (Not toggleable)"),
			new GUIContent("Enabled", "What the toggled state is when animated")
		};

		internal static QuickToggleWindow CancelInfo;

		string CustomUtilityWindow<QuickToggleWindow>.title => "CEditor QuickToggle";

		[SpecialName]
		private static bool RegisterTests()
		{
			return ConsumerAlgo.CallDefinition().advancedQuickToggle;
		}

		[SpecialName]
		private static void LogoutTests(bool loadinit)
		{
			ConsumerAlgo.CallDefinition().advancedQuickToggle.ExcludeDefinition(loadinit);
		}

		[SpecialName]
		private static bool InterruptTests()
		{
			return ConsumerAlgo.CallDefinition().mergeQuickToggle;
		}

		[SpecialName]
		private static void ManageTests(bool appendasset)
		{
			ConsumerAlgo.CallDefinition().mergeQuickToggle.ExcludeDefinition(appendasset);
		}

		internal static QuickToggleWindow AssetTests(List<AnimatorState> i, Transform second, List<GameObject> template)
		{
			QuickToggleWindow m_ContextInitializer = CustomUtilityWindow<QuickToggleWindow>.CloneHelper();
			m_ContextInitializer._VisitorInitializer = i;
			m_ContextInitializer.regInitializer = new bool[i.Count];
			AnimatorState defaultState = ConsumerAlgo.CallDefinition().defaultState;
			Motion motion = ((!(defaultState != null)) ? null : defaultState.motion);
			if (InterruptTests())
			{
				for (int j = 0; j < i.Count; j++)
				{
					AnimatorState animatorState = m_ContextInitializer._VisitorInitializer[j];
					m_ContextInitializer.regInitializer[j] = animatorState == null || animatorState.motion != motion;
				}
			}
			m_ContextInitializer.definitionInitializer = i.Count((AnimatorState s) => s.motion as AnimationClip);
			m_ContextInitializer.testsInitializer = m_ContextInitializer.definitionInitializer > 0;
			m_ContextInitializer.m_WrapperInitializer = second;
			m_ContextInitializer.annotationInitializer = new List<QueueProperty>(template.Select((GameObject o) => new QueueProperty(o)));
			_003C_003Ec__DisplayClass18_0 CS_0024_003C_003E8__locals0;
			m_ContextInitializer._AlgoInitializer = new ThreadTests<QueueProperty>(delegate
			{
				m_ContextInitializer._AlgoInitializer.LogoutThread("Target GameObjects", "The GameObjects that will be animated by the animation clip");
				GUILayout.FlexibleSpace();
				if (m_ContextInitializer.testsInitializer && ClassProperty.RestartQueue(InterruptTests() ? ClassProperty.DestroyError().m_ExpressionProcessor : ClassProperty.DestroyError().systemProcessor, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					ManageTests(!InterruptTests());
				}
				if (ClassProperty.RestartQueue((!RegisterTests()) ? ClassProperty.DestroyError().candidateProcessor : ClassProperty.DestroyError().productProcessor, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					LogoutTests(!RegisterTests());
				}
				m_ContextInitializer._AlgoInitializer.PatchThread(rejectv: false, writeattr: false);
			}, m_ContextInitializer.annotationInitializer, delegate
			{
				m_ContextInitializer.annotationInitializer.Add(new QueueProperty());
			}, delegate(Rect rect, int index, bool active, bool focused)
			{
				_003C_003Ec__DisplayClass18_1 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_1();
				_003C_003Ec__DisplayClass18_.m_ConsumerInitializer = CS_0024_003C_003E8__locals0;
				_003C_003Ec__DisplayClass18_._HelperInitializer = index;
				_003C_003Ec__DisplayClass18_.m_RecordInitializer = m_ContextInitializer.annotationInitializer[_003C_003Ec__DisplayClass18_._HelperInitializer];
				Rect rect2 = rect.SortResolver((!RegisterTests()) ? 80 : 45, isfield: false, -1f, iscont3: false, isattr4: false);
				Rect item = new Rect(rect2)
				{
					width = 20f,
					x = rect2.x + rect2.width - 20f
				};
				if (!RegisterTests())
				{
					Rect rect3 = rect.SortResolver(20f, isfield: false, 80f);
					string text = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer()) ? "Off" : "On");
					using (new TemplateThread(TemplateThread.ColoringType.BG, _003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer(), Color.green, Color.red))
					{
						if (GUI.Button(rect3, text))
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.PatchSerializer()) ? 1 : 0);
						}
					}
				}
				else
				{
					Rect rect4 = rect.SortResolver(40f, isfield: false, 45f, iscont3: false, isattr4: false);
					Rect res = rect.SortResolver(15f, isfield: false, 85f);
					rect4.height = 20f;
					bool flag = _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty.Length != 0;
					using (new EditorGUI.DisabledScope(!flag))
					{
						if (EditorGUI.DropdownButton(rect4, new GUIContent(_003C_003Ec__DisplayClass18_.m_RecordInitializer.SetPage()), FocusType.Keyboard, EditorStyles.toolbarDropDown))
						{
							StatusServer<string> statusServer = new StatusServer<string>("Property", _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_HelperProperty, _003C_003Ec.observerInitializer.OrderProperty, _003C_003Ec__DisplayClass18_.CountProperty);
							statusServer.GetConnection(_003C_003Ec.observerInitializer.CompareProperty);
							statusServer.RunConnection(rect4);
						}
					}
					_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty = EditorGUI.FloatField(res, _003C_003Ec__DisplayClass18_.m_RecordInitializer.m_AdapterProperty);
					ClassProperty.FlushQueue(rect4, "Property", 180f, 15f, stripresult3: false);
					ClassProperty.FlushQueue(res, "Value", 50f, 0f, stripresult3: false);
					if (!flag)
					{
						ClassProperty.FlushQueue(rect4, "No Valid Properties", 145f);
					}
				}
				using (new EditorGUI.DisabledScope(!_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer()))
				{
					if (GUI.Button(item, GUIContent.none, GUIStyle.none))
					{
						if (Event.current.button == 0)
						{
							StatusServer<Type> statusServer2 = new StatusServer<Type>("Target Type", new Type[1] { typeof(GameObject) }.Concat(_003C_003Ec__DisplayClass18_.m_RecordInitializer.m_SetterProperty.Select(_003C_003Ec.observerInitializer.SetProperty)).Distinct().ToList(), _003C_003Ec.observerInitializer.PostProperty, _003C_003Ec__DisplayClass18_.DisableProperty);
							statusServer2.IncludeConnection(_003C_003Ec.observerInitializer.SetupProperty);
							if (!ConsumerAlgo.CallDefinition().advancedQuickToggle)
							{
								StatusServer<Type>.UtilsServer[] dicServer = statusServer2.m_DicServer;
								for (int k = 0; k < dicServer.Length; k++)
								{
									_003C_003Ec__DisplayClass18_2 _003C_003Ec__DisplayClass18_2 = new _003C_003Ec__DisplayClass18_2();
									_003C_003Ec__DisplayClass18_2.interpreterInitializer = dicServer[k];
									if (!QueueProperty._InterpreterProperty.Any(_003C_003Ec__DisplayClass18_2.AddProperty))
									{
										_003C_003Ec__DisplayClass18_2.interpreterInitializer.authenticationServer = false;
									}
								}
							}
							statusServer2.RunConnection(item);
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.StopSerializer(!ConsumerAlgo.CallDefinition().advancedQuickToggle);
						}
						Event.current.Use();
					}
				}
				UnityEngine.Object contextProperty = _003C_003Ec__DisplayClass18_.m_RecordInitializer.contextProperty;
				EditorGUI.BeginChangeCheck();
				contextProperty = EditorGUI.ObjectField(rect2, contextProperty, typeof(GameObject), allowSceneObjects: true);
				if (EditorGUI.EndChangeCheck())
				{
					if ((bool)contextProperty)
					{
						if (!(contextProperty is GameObject def))
						{
							if (contextProperty is Component component)
							{
								_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(component.gameObject);
							}
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(def);
						}
					}
					else
					{
						_003C_003Ec__DisplayClass18_.m_RecordInitializer.PrintSerializer(null);
					}
				}
				if ((bool)_003C_003Ec__DisplayClass18_.m_RecordInitializer.ManageSerializer())
				{
					EditorGUI.DropdownButton(item, GUIContent.none, FocusType.Passive, ClassProperty.CalcError().descriptorProcessor);
				}
				ClassProperty.FlushQueue(rect2, "Target", 200f, 20f, stripresult3: false);
				ClassProperty.AwakeRules<GameObject>(rect2, _003C_003Ec__DisplayClass18_.InsertProperty);
				ClassProperty.ResetQueue(rect2, _003C_003Ec__DisplayClass18_.QueryProperty);
			});
			m_ContextInitializer._AlgoInitializer.m_ErrorTests = true;
			m_ContextInitializer.ChangeTests();
			if (!i.All((AnimatorState s) => s.name.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				if (i.All((AnimatorState s) => s.name.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					foreach (QueueProperty item2 in m_ContextInitializer.annotationInitializer)
					{
						item2.m_AdapterProperty = 1f;
					}
				}
			}
			else
			{
				foreach (QueueProperty item3 in m_ContextInitializer.annotationInitializer)
				{
					item3.m_AdapterProperty = 0f;
				}
			}
			return m_ContextInitializer;
		}

		void CustomUtilityWindow<QuickToggleWindow>.OnCustomGUI()
		{
			if (_AlgoInitializer == null)
			{
				Close();
				return;
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				m_WrapperInitializer = (Transform)EditorGUILayout.ObjectField(_ProcessorInitializer[0], m_WrapperInitializer, typeof(Transform), true);
			}
			_AlgoInitializer.SortThread();
			if (!testsInitializer)
			{
				return;
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				using (new GUILayout.HorizontalScope())
				{
					propertyInitializer = EditorGUILayout.Foldout(propertyInitializer, new GUIContent($"Existing Clips ({definitionInitializer})"));
					GUILayout.FlexibleSpace();
					GUILayout.Label(new GUIContent(ClassProperty.DestroyError()._AccountProcessor.CompareHelper(), "Merge: Adds the properties to the existing clips on states. Creates a new clip if no clip exists.\n\nReplace: Replaces the existing clips on states with new clips and adds the properties to them."), GUILayout.Width(14f), GUILayout.Height(18f));
					string res = ((_InitializerInitializer == 0) ? "Merge" : ((_InitializerInitializer == 1) ? "Replace" : "Mixed"));
					using (new TemplateThread(TemplateThread.ColoringType.BG, _InitializerInitializer, _MapperInitializer[0], _MapperInitializer[1], _MapperInitializer[2]))
					{
						if (ClassProperty.DisableQueue(res))
						{
							switch (_InitializerInitializer)
							{
							case 0:
							{
								_InitializerInitializer = 1;
								for (int j = 0; j < regInitializer.Length; j++)
								{
									regInitializer[j] = false;
								}
								break;
							}
							case 1:
							case 2:
							{
								_InitializerInitializer = 0;
								for (int i = 0; i < regInitializer.Length; i++)
								{
									regInitializer[i] = true;
								}
								break;
							}
							}
						}
					}
				}
				if (!propertyInitializer)
				{
					return;
				}
				using (new SchemaThread())
				{
					for (int k = 0; k < _VisitorInitializer.Count; k++)
					{
						AnimatorState animatorState = _VisitorInitializer[k];
						if (!animatorState)
						{
							continue;
						}
						AnimationClip animationClip = animatorState.motion as AnimationClip;
						if (!animationClip)
						{
							continue;
						}
						using (new GUILayout.HorizontalScope(GUI.skin.box))
						{
							GUILayout.Label(animationClip.name);
							GUILayout.FlexibleSpace();
							string res2 = (regInitializer[k] ? "Merge" : "Replace");
							using (new TemplateThread(TemplateThread.ColoringType.BG, regInitializer[k], _MapperInitializer[0], _MapperInitializer[1]))
							{
								if (ClassProperty.DisableQueue(res2))
								{
									regInitializer[k] = !regInitializer[k];
									ChangeTests();
								}
							}
						}
					}
				}
			}
		}

		internal override void OnCustomConfirm()
		{
			if (RateAnnotation(!m_WrapperInitializer, "No Root Set!"))
			{
				return;
			}
			List<AnimationClip> list = new List<AnimationClip>();
			int num = 0;
			while (true)
			{
				if (num < _VisitorInitializer.Count)
				{
					AnimatorState animatorState = _VisitorInitializer[num];
					Motion motion = animatorState.motion;
					if (regInitializer[num] || !RateAnnotation(motion is UnityEditor.Animations.BlendTree, "State " + animatorState.name + " has a Blendtree motion. Can't automatically merge."))
					{
						AnimationClip animationClip = motion as AnimationClip;
						if (!animationClip || !regInitializer[num])
						{
							Undo.RecordObject(animatorState, "Set Quick Toggle Curve");
							string i = $"{ConsumerAlgo.CallDefinition().saveFolder}/Animation Clips/{LogoutMapper().name}";
							animationClip = new AnimationClip();
							string path = ClassProperty.AwakeList(i, animatorState.name + ".anim", writestate: true);
							AssetDatabase.CreateAsset(animationClip, path);
							animatorState.motion = animationClip;
							EditorUtility.SetDirty(animatorState);
						}
						list.Add(animationClip);
						num++;
						continue;
					}
					break;
				}
				UnityEngine.Object[] objectsToUndo;
				UnityEngine.Object[] array = (objectsToUndo = list.Distinct().ToArray());
				Undo.RecordObjects(objectsToUndo, "Set Quick Toggle Curve");
				AnimationClip[] array2 = (AnimationClip[])array;
				foreach (AnimationClip animationClip2 in array2)
				{
					foreach (QueueProperty item in annotationInitializer)
					{
						if (item.RegisterSerializer())
						{
							animationClip2.SetCurve(AnimationUtility.CalculateTransformPath(item.ManageSerializer().transform, m_WrapperInitializer), item._RecordProperty, item.SetPage(), ClassProperty.InvokePredicate(AnimationUtility.TangentMode.Linear, (0f, item.m_AdapterProperty), (animationClip2.FindPredicate(), item.m_AdapterProperty)));
						}
					}
				}
				break;
			}
		}

		internal Vector2 UpdateTests()
		{
			return new Vector2(370f, 48 + 22 * Mathf.Max(1, annotationInitializer.Count) + 28 + ((!string.IsNullOrEmpty(invocationPolicy)) ? 38 : 0) + (testsInitializer ? 32 : 0));
		}

		internal void ChangeTests()
		{
			_InitializerInitializer = ((!regInitializer.All((bool b) => b)) ? (regInitializer.All((bool b) => !b) ? 1 : 2) : 0);
		}

		internal void SortTests(Vector2 instance)
		{
			LoginHelper(instance, UpdateTests());
		}

		internal static bool RestartInfo()
		{
			return (object)CancelInfo == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec watcherInitializer = new _003C_003Ec();

		public static Func<bool> candidateInitializer;

		public static Func<SetterTests.TokenizerTests, bool> productInitializer;

		public static Func<SetterTests.TokenizerTests, bool> _ExpressionInitializer;

		public static Func<SetterTests.TokenizerTests, bool> systemInitializer;

		public static Func<SetterTests.InstanceTests, AnimatorTransitionBase> workerInitializer;

		public static Func<AnimatorTransitionBase, bool> m_FilterInitializer;

		public static Func<AnimatorTransitionBase, bool> _StubInitializer;

		public static Func<bool> readerInitializer;

		public static Func<bool> bridgeInitializer;

		public static Func<bool> m_StrategyInitializer;

		public static Func<GUIStyle, GUIStyle> _CustomerInitializer;

		public static Func<GUIStyle, GUIStyle> databaseInitializer;

		public static Func<GUIStyle, GUIStyle> m_ExporterInitializer;

		public static Func<GUIStyle, GUIStyle> identifierInitializer;

		public static Func<GUIStyle, GUIStyle> _AttrInitializer;

		public static Func<GUIStyle, GUIStyle> dispatcherInitializer;

		public static Func<GUIStyle, GUIStyle> _RegistryInitializer;

		public static Func<GUIStyle, GUIStyle> tagInitializer;

		public static GenericMenu.MenuFunction _ImporterInitializer;

		public static GenericMenu.MenuFunction _RequestInitializer;

		public static GenericMenu.MenuFunction printerInitializer;

		public static GenericMenu.MenuFunction m_WriterInitializer;

		public static Func<AlgoAlgo, bool> m_ParamsInitializer;

		public static Func<StateMachineBehaviour, bool> _ListenerInitializer;

		public static Func<AnimatorStateTransition, bool> m_GetterInitializer;

		public static Func<AnimatorStateTransition, string> _InterceptorInitializer;

		public static Func<object[], bool[]> creatorInitializer;

		public static Func<Animator, bool> eventInitializer;

		public static Func<Type, IEnumerable<MethodInfo>> infoInitializer;

		public static Func<System.Reflection.Assembly, IEnumerable<MethodInfo>> _FacadeInitializer;

		public static Func<(MethodInfo, CallbackServer, bool), int> m_AdvisorInitializer;

		public static Action m_CallbackInitializer;

		public static Action _IndexerInitializer;

		public static Func<bool> m_IssuerInitializer;

		public static Action<System.Exception> m_PrototypeInitializer;

		public static Action m_RuleInitializer;

		public static Action<FieldAlgo> singletonInitializer;

		public static Action<System.Exception> _FactoryInitializer;

		public static Func<PoolAlgo, bool> _AccountInitializer;

		public static Func<string, bool> m_RefInitializer;

		public static Func<(bool, string), bool> statusInitializer;

		public static Func<(bool, string), string> _TokenInitializer;

		public static Func<(bool, string), bool> m_CodeInitializer;

		public static Func<(bool, string), string> _DicInitializer;

		public static Func<bool> m_InvocationInitializer;

		public static Func<bool> roleInitializer;

		public static Action<FieldAlgo> _ParamInitializer;

		public static Action<System.Exception> _ModelInitializer;

		public static Action m_TokenizerInitializer;

		public static Action _DecoratorInitializer;

		public static Action<FieldAlgo> m_ComparatorInitializer;

		public static Action<System.Exception> m_ExceptionInitializer;

		public static Action objectInitializer;

		public static GenericMenu.MenuFunction _UtilsInitializer;

		public static GenericMenu.MenuFunction m_ValInitializer;

		public static GenericMenu.MenuFunction m_ValueInitializer;

		public static GenericMenu.MenuFunction m_MerchantInitializer;

		public static GenericMenu.MenuFunction _AuthenticationInitializer;

		public static GenericMenu.MenuFunction reponseInitializer;

		public static GenericMenu.MenuFunction poolInitializer;

		public static GenericMenu.MenuFunction _ParameterInitializer;

		public static GenericMenu.MenuFunction _ComposerInitializer;

		public static Action repositoryInitializer;

		public static Action<System.Exception> mappingInitializer;

		public static Action m_BaseInitializer;

		public static Func<Task> _ContainerInitializer;

		public static Func<bool> classInitializer;

		public static Func<bool> m_MockInitializer;

		public static Func<bool> m_InstanceInitializer;

		public static Func<bool> _FieldInitializer;

		public static Func<bool> attributeInitializer;

		public static Func<bool> _ClientInitializer;

		public static Func<AnimatorState, bool> m_ConfigInitializer;

		public static Func<AnimatorState, bool> m_DescriptorInitializer;

		public static Func<AnimatorState, bool> m_TemplateInitializer;

		public static Func<bool> messageInitializer;

		public static Func<ChildAnimatorState, AnimatorState> collectionInitializer;

		public static Func<SetterTests.TokenizerTests, bool> _ParserInitializer;

		public static Func<AnimatorStateTransition, bool> _ManagerInitializer;

		public static Func<bool> itemInitializer;

		public static Func<SetterTests.InstanceTests, bool> specificationInitializer;

		public static Func<System.Reflection.Assembly, IEnumerable<Type>> _MethodInitializer;

		public static Func<Type, bool> _SchemaInitializer;

		public static Func<Type, string> broadcasterInitializer;

		public static Func<EditorCurveBinding, bool> proxyInitializer;

		public static Func<EditorCurveBinding, string> structInitializer;

		public static Func<string, string> _ServiceInitializer;

		public static Func<EditorCurveBinding, string> m_StateInitializer;

		public static Func<string, string> globalInitializer;

		public static Func<GlobalVisitor, bool> _TaskInitializer;

		public static Func<GlobalVisitor, AnimatorCondition> m_ProcessInitializer;

		public static Func<GlobalVisitor, AnimatorCondition> producerInitializer;

		public static Func<AnimatorStateTransition, bool> m_IteratorInitializer;

		public static Func<AnimatorStateTransition, bool> publisherInitializer;

		public static Func<AnimatorStateTransition, bool> _ConfigurationInitializer;

		public static Func<AnimatorStateTransition, bool> procInitializer;

		public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> wrapperDefinition;

		public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> _AnnotationDefinition;

		public static Func<AnimatorStateTransition, bool> _VisitorDefinition;

		public static Func<UnityEditor.Animations.AnimatorControllerLayer, bool> m_AlgoDefinition;

		public static Func<UnityEditor.Animations.AnimatorControllerLayer, AnimatorStateMachine> m_MapperDefinition;

		public static Func<UnityEditor.Animations.AnimatorControllerLayer, AnimatorStateMachine> initializerDefinition;

		public static Func<UnityEngine.AnimatorControllerParameter, bool> _DefinitionDefinition;

		public static Func<AnimatorStateTransition, bool> m_RegDefinition;

		public static Action<AnimatorState> _TestsDefinition;

		public static Func<AnimatorTransition, bool> propertyDefinition;

		public static Func<AnimatorStateTransition, bool> _ProcessorDefinition;

		public static Action<AnimatorStateMachine> m_ObserverDefinition;

		public static Action<AnimatorStateMachine> _ServerDefinition;

		public static Func<UnityEngine.AnimatorControllerParameter, bool> _ThreadDefinition;

		public static Func<ChildMotion, Motion> m_PolicyDefinition;

		public static Func<ChildAnimatorStateMachine, AnimatorStateMachine> serializerDefinition;

		public static Func<ChildAnimatorStateMachine, AnimatorStateMachine> pageDefinition;

		public static Func<ChildAnimatorState, AnimatorState> resolverDefinition;

		public static Action<AnimatorTransitionBase> _PredicateDefinition;

		public static Func<ChildAnimatorState, bool> m_RulesDefinition;

		public static Func<ChildAnimatorStateMachine, bool> queueDefinition;

		public static Func<Vector3, ChildAnimatorState, Vector3> errorDefinition;

		public static Func<Vector3, ChildAnimatorStateMachine, Vector3> setterDefinition;

		public static Func<ChildAnimatorState, bool> _ConnectionDefinition;

		public static Func<ChildAnimatorState, float> contextDefinition;

		public static Func<ChildAnimatorState, bool> recordDefinition;

		public static Func<ChildAnimatorState, float> m_HelperDefinition;

		public static Func<ChildAnimatorState, AnimatorState> m_ConsumerDefinition;

		public static Func<UnityEngine.Object, bool> _AdapterDefinition;

		public static Func<string, bool> m_InterpreterDefinition;

		public static Func<string, bool> m_WatcherDefinition;

		public static Func<string, bool> m_CandidateDefinition;

		public static Func<UnityEngine.Object, bool> productDefinition;

		public static Func<AnimatorTransitionBase, bool> expressionDefinition;

		public static Func<AnimatorStateTransition, bool> systemDefinition;

		public static Func<UnityEngine.Object, bool> m_WorkerDefinition;

		public static Func<AnimatorStateTransition, bool> _FilterDefinition;

		public static Func<AnimatorStateTransition, bool> stubDefinition;

		public static Func<AnimatorTransition, bool> _ReaderDefinition;

		public static Action<SetterTests.InstanceTests> _BridgeDefinition;

		public static Func<ChildAnimatorState, bool> m_StrategyDefinition;

		public static Action<UnityEngine.Object> m_CustomerDefinition;

		public static Action<IEnumerable<UnityEditor.Animations.AnimatorController>> databaseDefinition;

		public static Action<UnityEngine.Object> _ExporterDefinition;

		public static Action<IEnumerable<GameObject>> identifierDefinition;

		public static Func<object, bool> attrDefinition;

		public static Func<object, EditorCurveBinding> dispatcherDefinition;

		public static Func<Renderer, IEnumerable<Material>> m_RegistryDefinition;

		public static Func<Material, Shader> m_TagDefinition;

		public static Action<StatusServer<string>.UtilsServer> importerDefinition;

		public static Func<string, string, bool> _RequestDefinition;

		public static Func<Component, Type> m_PrinterDefinition;

		public static Func<Type, bool> _WriterDefinition;

		public static Action<StatusServer<Type>.UtilsServer> paramsDefinition;

		public static Func<Type, object[]> m_ListenerDefinition;

		public static Func<Type, string, bool> m_GetterDefinition;

		public static Func<ObjectReferenceKeyframe, float> interceptorDefinition;

		public static Func<IGrouping<float, ObjectReferenceKeyframe>, ObjectReferenceKeyframe> m_CreatorDefinition;

		public static Func<Keyframe, float> eventDefinition;

		public static Func<IGrouping<float, Keyframe>, Keyframe> m_InfoDefinition;

		public static Func<SetterTests.BaseTests, Vector3> _FacadeDefinition;

		public static Func<object[], bool[]> _AdvisorDefinition;

		public static Comparison<ImporterMapper> callbackDefinition;

		public static Comparison<EventMapper> m_IndexerDefinition;

		public static Func<string, string> m_IssuerDefinition;

		public static Func<ImporterMapper, bool> m_PrototypeDefinition;

		public static Func<ChildAnimatorState, AnimatorState> _RuleDefinition;

		public static Func<ChildAnimatorState, AnimatorState> m_SingletonDefinition;

		public static Func<ChildAnimatorState, string> _FactoryDefinition;

		public static Func<object> accountDefinition;

		public static Action<bool> refDefinition;

		public static Func<MethodInfo, bool> statusDefinition;

		public static Func<MethodInfo, bool> tokenDefinition;

		public static Func<UnityEngine.Object, bool> codeDefinition;

		public static Func<UnityEngine.Object, bool> m_DicDefinition;

		public static GenericMenu.MenuFunction2 invocationDefinition;

		internal static _003C_003Ec StopInfo;

		internal bool InvokeProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool FindProperty(SetterTests.TokenizerTests nw)
		{
			return nw.FillPolicy() == SetterTests.RefTests.FlushPolicy().FillPolicy();
		}

		internal bool ExcludeProperty(SetterTests.TokenizerTests nw)
		{
			return nw.FillPolicy() == SetterTests.RefTests.MapPolicy().FillPolicy();
		}

		internal bool InitProperty(SetterTests.TokenizerTests nw)
		{
			return nw.FillPolicy() == SetterTests.RefTests.CalculatePolicy().FillPolicy();
		}

		internal AnimatorTransitionBase VisitProperty(SetterTests.InstanceTests t)
		{
			return t.itemTests;
		}

		internal bool DefineProperty(AnimatorTransitionBase t)
		{
			return !propertyAnnotation.Contains(t);
		}

		internal bool StartProperty(AnimatorTransitionBase t)
		{
			return !_Iterator.Contains(t);
		}

		internal bool ReadProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool SelectProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool RemoveProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal GUIStyle InstantiateProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				alignment = TextAnchor.UpperLeft,
				clipping = TextClipping.Overflow,
				fontStyle = FontStyle.Bold,
				overflow = new RectOffset(),
				contentOffset = default(Vector2),
				padding = new RectOffset(2, 2, 2, 2),
				wordWrap = true,
				fixedHeight = 100f,
				fixedWidth = 100f,
				normal = 
				{
					scaledBackgrounds = new Texture2D[1] { ClassProperty.LoginList(Color.black) }
				}
			};
		}

		internal GUIStyle AwakeProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				alignment = TextAnchor.UpperLeft,
				clipping = TextClipping.Overflow,
				fontStyle = FontStyle.Bold,
				overflow = new RectOffset(),
				contentOffset = default(Vector2),
				padding = new RectOffset(2, 2, 2, 2),
				wordWrap = true,
				fixedHeight = 200f,
				fixedWidth = 200f,
				normal = 
				{
					scaledBackgrounds = new Texture2D[1] { ClassProperty.LoginList(Color.black) }
				}
			};
		}

		internal GUIStyle ResetProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedHeight = 40f,
				fixedWidth = 100f
			};
		}

		internal GUIStyle FlushProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedHeight = 40f,
				fixedWidth = 40f
			};
		}

		internal GUIStyle ConnectProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedHeight = 80f,
				fixedWidth = 80f,
				alignment = TextAnchor.MiddleCenter
			};
		}

		internal GUIStyle CalculateProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedHeight = 80f,
				fixedWidth = 400f,
				fontSize = 20
			};
		}

		internal GUIStyle TestProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedWidth = 20f,
				fixedHeight = 20f,
				clipping = TextClipping.Clip
			};
		}

		internal GUIStyle MapProperty(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				fixedWidth = 10f,
				fixedHeight = 10f,
				clipping = TextClipping.Clip
			};
		}

		internal void ValidateProperty()
		{
			Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
		}

		internal void CustomizeProperty()
		{
			ConsumerAlgo.CallDefinition().useLegacyDropdown.InsertDefinition();
		}

		internal void RateProperty()
		{
			GetAnnotation(updatereference: true);
		}

		internal void DestroyProperty()
		{
			GetAnnotation(updatereference: false);
		}

		internal bool GetProperty(AlgoAlgo s)
		{
			return s._MapperAlgo;
		}

		internal bool CalcProperty(StateMachineBehaviour b)
		{
			return b.GetType() != WorkerProperty.QueryPage();
		}

		internal bool IncludeProperty(AnimatorStateTransition t)
		{
			return t.isExit;
		}

		internal string RunProperty(AnimatorStateTransition t)
		{
			return t.name;
		}

		internal bool[] CloneProperty(object[] arr)
		{
			bool[] array = new bool[arr.Length];
			array[0] = arr[0] == null;
			return array;
		}

		internal bool LoginProperty(Animator a)
		{
			return a.avatar;
		}

		internal IEnumerable<MethodInfo> ReflectProperty(System.Reflection.Assembly assembly)
		{
			return assembly.GetTypes().SelectMany((Type t) => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
		}

		internal IEnumerable<MethodInfo> DeleteProperty(Type t)
		{
			return t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		internal int CreateProperty((MethodInfo, CallbackServer, bool onVerify) x)
		{
			return x.Item2._IssuerServer;
		}

		internal void NewProperty()
		{
			systemAnnotation = false;
			while (true)
			{
				m_ExpressionAnnotation = false;
			}
		}

		internal void PushProperty()
		{
			WriteAnnotation(assetneeded: false);
		}

		internal bool ViewProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal void CollectProperty(System.Exception exception)
		{
			_RequestAnnotation = false;
			listenerAnnotation = false;
			attrAnnotation = true;
			FindVisitor($"Something went wrong while verifying license:\n\n{exception}", CustomLogType.Error);
		}

		internal void ResolveProperty(FieldAlgo response)
		{
			importerAnnotation = false;
			SortAnnotation(response, delegate
			{
				m_IdentifierAnnotation = false;
				ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: true);
				WriteAnnotation(assetneeded: true);
			});
		}

		internal void ListProperty()
		{
			m_IdentifierAnnotation = false;
			ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: true);
			WriteAnnotation(assetneeded: true);
		}

		internal void VerifyProperty(System.Exception exception)
		{
			importerAnnotation = false;
			FindVisitor($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}

		internal bool FillProperty(PoolAlgo p)
		{
			return p.m_MockAlgo;
		}

		internal bool WriteProperty(string v)
		{
			return !string.IsNullOrWhiteSpace(v);
		}

		internal bool ForgotProperty((bool, string) i)
		{
			return !i.Item1;
		}

		internal string StopProperty((bool, string) i)
		{
			return i.Item2;
		}

		internal bool CheckProperty((bool, string) i)
		{
			return !i.Item1;
		}

		internal string PrepareProperty((bool, string) i)
		{
			return i.Item2;
		}

		internal bool AssetProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool UpdateProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal void ChangeProperty()
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenserequest");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				_003C_003Ec__DisplayClass239_0 _003C_003Ec__DisplayClass239_ = new _003C_003Ec__DisplayClass239_0
				{
					serviceDefinition = response
				};
			}, delegate(System.Exception exception)
			{
				m_CallbackAnnotation = false;
				FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, InsertVisitor);
		}

		internal void SortProperty(FieldAlgo response)
		{
			_003C_003Ec__DisplayClass239_0 _003C_003Ec__DisplayClass239_ = new _003C_003Ec__DisplayClass239_0
			{
				serviceDefinition = response
			};
		}

		internal void RegisterProperty(System.Exception exception)
		{
			m_CallbackAnnotation = false;
			FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void LogoutProperty()
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenseconfirm");
			list.Add(("verification_code", strategyAnnotation));
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				_IndexerAnnotation = false;
				SortAnnotation(response, delegate
				{
					facadeAnnotation = false;
					advisorAnnotation = false;
					m_IdentifierAnnotation = false;
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(System.Exception exception)
			{
				_IndexerAnnotation = false;
				FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, InsertVisitor);
		}

		internal void PatchProperty(FieldAlgo response)
		{
			_IndexerAnnotation = false;
			SortAnnotation(response, delegate
			{
				facadeAnnotation = false;
				advisorAnnotation = false;
				m_IdentifierAnnotation = false;
				WriteAnnotation(assetneeded: true);
			});
		}

		internal void InterruptProperty()
		{
			facadeAnnotation = false;
			advisorAnnotation = false;
			m_IdentifierAnnotation = false;
			WriteAnnotation(assetneeded: true);
		}

		internal void ManageProperty(System.Exception exception)
		{
			_IndexerAnnotation = false;
			FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void PrintProperty()
		{
			SessionState.EraseString("yOk0XCnENLMO6DIF8cYpSg==updateinfo");
			AwakeVisitor();
		}

		internal void SearchProperty()
		{
			m_ExpressionAnnotation.AwakeResolver();
		}

		internal void RevertProperty()
		{
			ConsumerAlgo.CallDefinition().a_VerifyOnDisplay.InsertDefinition();
			ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad.ExcludeDefinition(excludeparam: false);
		}

		internal void OrderProcessor()
		{
			ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad.InsertDefinition();
			ConsumerAlgo.CallDefinition().a_VerifyOnDisplay.ExcludeDefinition(excludeparam: false);
		}

		internal void CompareProcessor()
		{
			Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
		}

		internal void SetProcessor()
		{
			Application.OpenURL(statusAnnotation[0].Item2);
		}

		internal void PostProcessor()
		{
			Application.OpenURL("https://github.com/Dreadrith/DreadScripts/blob/main/ControllerEditor/Changelog.txt");
		}

		internal void SetupProcessor()
		{
			Application.OpenURL("https://www.dreadrith.com/l/CEditor");
		}

		internal void EnableProcessor()
		{
			Application.OpenURL("https://dreadrith.com/license-tos");
		}

		internal void PublishProcessor()
		{
			ResetVisitor(removereference: false);
		}

		internal void PopProcessor(System.Exception exc)
		{
			FindVisitor($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}

		internal void ComputeProcessor()
		{
			_PrototypeAnnotation = false;
			InsertVisitor();
		}

		internal async Task MoveProcessor()
		{
			await Task.Delay(3000);
			ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: true);
			InsertVisitor();
		}

		internal bool ConcatProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool CallProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool CancelProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool CountProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool DisableProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool InsertProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool RestartProcessor(AnimatorState s)
		{
			if (s.name.IndexOf("(wd on)", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return s.name.IndexOf("(wd off)", StringComparison.OrdinalIgnoreCase) < 0;
			}
			return false;
		}

		internal bool QueryProcessor(AnimatorState s)
		{
			if (!s)
			{
				return false;
			}
			return s.writeDefaultValues;
		}

		internal bool AddProcessor(AnimatorState s)
		{
			if (!s)
			{
				return false;
			}
			return !s.writeDefaultValues;
		}

		internal bool InvokeProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal AnimatorState FindProcessor(ChildAnimatorState c)
		{
			return c.state;
		}

		internal bool ExcludeProcessor(SetterTests.TokenizerTests n)
		{
			return n.objectTests == SetterTests.TokenizerTests.NodeType.state;
		}

		internal bool InitProcessor(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass308_0 _003C_003Ec__DisplayClass308_ = new _003C_003Ec__DisplayClass308_0
			{
				serverReg = t
			};
			return RevertMapper().states.Any(_003C_003Ec__DisplayClass308_.InvokeServer);
		}

		internal bool VisitProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		}

		internal bool DefineProcessor(SetterTests.InstanceTests et)
		{
			return et.itemTests != null;
		}

		internal IEnumerable<Type> StartProcessor(System.Reflection.Assembly a)
		{
			return a.GetTypes();
		}

		internal bool ReadProcessor(Type t)
		{
			if (!t.IsSubclassOf(typeof(Component)) || t.IsAbstract)
			{
				return false;
			}
			return !t.IsGenericTypeDefinition;
		}

		internal string SelectProcessor(Type t)
		{
			return t.Name;
		}

		internal bool RemoveProcessor(EditorCurveBinding b)
		{
			if (b.propertyName.StartsWith("material."))
			{
				return b.type.RemoveResolver<Renderer>();
			}
			return false;
		}

		internal string InstantiateProcessor(EditorCurveBinding b)
		{
			return b.propertyName;
		}

		internal string AwakeProcessor(string p)
		{
			return p;
		}

		internal string ResetProcessor(EditorCurveBinding b)
		{
			return b.propertyName;
		}

		internal string FlushProcessor(string s)
		{
			return s;
		}

		internal bool ConnectProcessor(GlobalVisitor sc)
		{
			return !sc._TaskVisitor;
		}

		internal AnimatorCondition CalculateProcessor(GlobalVisitor sc)
		{
			return sc.m_ProcessVisitor;
		}

		internal AnimatorCondition TestProcessor(GlobalVisitor sc)
		{
			return sc.m_ProcessVisitor;
		}

		internal bool MapProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == proxyAnnotation))
			{
				return false;
			}
			return t.isExit;
		}

		internal bool ValidateProcessor(AnimatorStateTransition t)
		{
			if (t.name == _SchemaAnnotation)
			{
				return t.isExit;
			}
			return false;
		}

		internal bool CustomizeProcessor(AnimatorStateTransition t)
		{
			return t.name == proxyAnnotation;
		}

		internal bool RateProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation && t.isExit);
		}

		internal bool DestroyProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == proxyAnnotation))
			{
				return false;
			}
			return t.isExit;
		}

		internal bool GetProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine == ManageMapper();
		}

		internal bool CalcProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation && t.isExit);
		}

		internal bool IncludeProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == proxyAnnotation))
			{
				return false;
			}
			return t.isExit;
		}

		internal AnimatorStateMachine RunProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine;
		}

		internal AnimatorStateMachine CloneProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine;
		}

		internal bool LoginProcessor(UnityEngine.AnimatorControllerParameter p)
		{
			return p != null;
		}

		internal void ReflectProcessor(AnimatorStateMachine l)
		{
			l.CollectPredicate(delegate(AnimatorStateMachine m)
			{
				m.AssetPredicate(delegate(AnimatorState s)
				{
					s.transitions = s.transitions.Where((AnimatorStateTransition t) => t).ToArray();
					EditorUtility.SetDirty(s);
				}, requiresc: false);
				m.entryTransitions = m.entryTransitions.Where((AnimatorTransition t) => t).ToArray();
				m.anyStateTransitions = m.anyStateTransitions.Where((AnimatorStateTransition t) => t).ToArray();
				EditorUtility.SetDirty(m);
			});
		}

		internal void DeleteProcessor(AnimatorStateMachine m)
		{
			m.AssetPredicate(delegate(AnimatorState s)
			{
				s.transitions = s.transitions.Where((AnimatorStateTransition t) => t).ToArray();
				EditorUtility.SetDirty(s);
			}, requiresc: false);
			m.entryTransitions = m.entryTransitions.Where((AnimatorTransition t) => t).ToArray();
			m.anyStateTransitions = m.anyStateTransitions.Where((AnimatorStateTransition t) => t).ToArray();
			EditorUtility.SetDirty(m);
		}

		internal void CreateProcessor(AnimatorState s)
		{
			s.transitions = s.transitions.Where((AnimatorStateTransition t) => t).ToArray();
			EditorUtility.SetDirty(s);
		}

		internal bool NewProcessor(AnimatorStateTransition t)
		{
			return t;
		}

		internal bool PushProcessor(AnimatorTransition t)
		{
			return t;
		}

		internal bool ViewProcessor(AnimatorStateTransition t)
		{
			return t;
		}

		internal bool CollectProcessor(UnityEngine.AnimatorControllerParameter p)
		{
			return ClassProperty.m_AdapterProcessor.Contains(p.name);
		}

		internal Motion ResolveProcessor(ChildMotion c)
		{
			return c.motion;
		}

		internal AnimatorStateMachine ListProcessor(ChildAnimatorStateMachine c)
		{
			return c.stateMachine;
		}

		internal AnimatorStateMachine VerifyProcessor(ChildAnimatorStateMachine cm)
		{
			return cm.stateMachine;
		}

		internal AnimatorState FillProcessor(ChildAnimatorState cs)
		{
			return cs.state;
		}

		internal void WriteProcessor(AnimatorTransitionBase t)
		{
			RateAlgo(ConsumerAlgo.CallDefinition().defaultTransition, t);
		}

		internal bool ForgotProcessor(ChildAnimatorState c)
		{
			return m_AlgoAnnotation.Contains(c.state);
		}

		internal bool StopProcessor(ChildAnimatorStateMachine c)
		{
			return m_WrapperAnnotation.Contains(c.stateMachine);
		}

		internal Vector3 CheckProcessor(Vector3 current, ChildAnimatorState child)
		{
			return current + child.position;
		}

		internal Vector3 PrepareProcessor(Vector3 current, ChildAnimatorStateMachine child)
		{
			return current + child.position;
		}

		internal bool AssetProcessor(ChildAnimatorState c)
		{
			return m_AlgoAnnotation.Contains(c.state);
		}

		internal float UpdateProcessor(ChildAnimatorState c)
		{
			return c.position.x;
		}

		internal bool ChangeProcessor(ChildAnimatorState c)
		{
			return m_AlgoAnnotation.Contains(c.state);
		}

		internal float SortProcessor(ChildAnimatorState c)
		{
			return c.position.y;
		}

		internal AnimatorState RegisterProcessor(ChildAnimatorState c)
		{
			return c.state;
		}

		internal bool LogoutProcessor(UnityEngine.Object o)
		{
			return !(o is AnimatorState);
		}

		internal bool PatchProcessor(string t)
		{
			return t == "ce_comment";
		}

		internal bool InterruptProcessor(string t)
		{
			return t == "ce_bigcomment";
		}

		internal bool ManageProcessor(string t)
		{
			return m_SerializerAnnotation.Contains(t);
		}

		internal bool PrintProcessor(UnityEngine.Object o)
		{
			if (!(o is AnimatorState))
			{
				return o is AnimatorStateMachine;
			}
			return true;
		}

		internal bool SearchProcessor(AnimatorTransitionBase t)
		{
			return RevertMapper().entryTransitions.Contains(t);
		}

		internal bool RevertProcessor(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass420_1 _003C_003Ec__DisplayClass420_ = new _003C_003Ec__DisplayClass420_1
			{
				_ValueReg = t
			};
			return RevertMapper().states.Any(_003C_003Ec__DisplayClass420_.QueryThread);
		}

		internal bool OrderObserver(UnityEngine.Object o)
		{
			return !(o is AnimatorStateTransition);
		}

		internal bool CompareObserver(AnimatorStateTransition t)
		{
			if (m_AlgoAnnotation.Contains(t.destinationState))
			{
				return true;
			}
			if (!collection)
			{
				return false;
			}
			return t.isExit;
		}

		internal bool SetObserver(AnimatorStateTransition t)
		{
			return m_AlgoAnnotation.Contains(t.destinationState);
		}

		internal bool PostObserver(AnimatorTransition t)
		{
			return m_AlgoAnnotation.Contains(t.destinationState);
		}

		internal void SetupObserver(SetterTests.InstanceTests t)
		{
			_003C_003Ec__DisplayClass430_0 _003C_003Ec__DisplayClass430_ = new _003C_003Ec__DisplayClass430_0
			{
				m_MerchantReg = t
			};
			m_AlgoAnnotation.InvokeResolver(_003C_003Ec__DisplayClass430_.AddThread);
			if (m_Template && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddAnyStateTransition((AnimatorState)null));
			}
			if (m_Message && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddEntryTransition((AnimatorState)null));
			}
			if (_AttributeVisitor)
			{
				_003C_003Ec__DisplayClass430_.m_MerchantReg.InitSerializer();
			}
		}

		internal bool EnableObserver(ChildAnimatorState c)
		{
			return c.state.transitions.Contains(_ProcessorAnnotation.itemTests);
		}

		internal void PublishObserver(UnityEngine.Object o)
		{
			TestInitializer((UnityEditor.Animations.AnimatorController)o);
		}

		internal void PopObserver(IEnumerable<UnityEditor.Animations.AnimatorController> o)
		{
			UnityEditor.Animations.AnimatorController[] source = (o as UnityEditor.Animations.AnimatorController[]) ?? o.ToArray();
			if (source.Any())
			{
				TestInitializer(source.First());
			}
		}

		internal void ComputeObserver(UnityEngine.Object o)
		{
			_AlgoVisitor = (GameObject)o;
		}

		internal void MoveObserver(IEnumerable<GameObject> o)
		{
			GameObject[] source = (o as GameObject[]) ?? o.ToArray();
			if (source.Any())
			{
				_AlgoVisitor = source.First();
			}
		}

		internal bool ConcatObserver(object n)
		{
			return ((TreeViewItem)n).children.CalcRules();
		}

		internal EditorCurveBinding CallObserver(object n)
		{
			return (EditorCurveBinding)m_SerializerVisitor.GetValue(n);
		}

		internal IEnumerable<Material> CancelObserver(Renderer c)
		{
			return c.sharedMaterials;
		}

		internal Shader CountObserver(Material m)
		{
			return m.shader;
		}

		internal void DisableObserver(StatusServer<string>.UtilsServer i)
		{
			GUILayout.Label(i._ValueServer, EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			ClassProperty.StartQueue();
		}

		internal bool InsertObserver(string p, string s)
		{
			return p.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1;
		}

		internal Type RestartObserver(Component c)
		{
			return c.GetType();
		}

		internal bool QueryObserver(Type t)
		{
			return VerifyVisitor(t);
		}

		internal void AddObserver(StatusServer<Type>.UtilsServer i)
		{
			GUILayout.Label((GUIContent)i.CloneConnection(), EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			ClassProperty.StartQueue();
		}

		internal object[] InvokeObserver(Type type)
		{
			return new object[1]
			{
				new GUIContent(image: EditorGUIUtility.ObjectContent(null, type).image ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image, text: type.Name, tooltip: type.AssemblyQualifiedName)
			};
		}

		internal bool FindObserver(Type t, string s)
		{
			return t.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1;
		}

		internal float ExcludeObserver(ObjectReferenceKeyframe f)
		{
			return f.time;
		}

		internal ObjectReferenceKeyframe InitObserver(IGrouping<float, ObjectReferenceKeyframe> g)
		{
			return g.First();
		}

		internal float VisitObserver(Keyframe k)
		{
			return k.time;
		}

		internal Keyframe DefineObserver(IGrouping<float, Keyframe> g)
		{
			return g.First();
		}

		internal Vector3 StartObserver(SetterTests.BaseTests e)
		{
			return InitAnnotation(e)[1];
		}

		internal bool[] ReadObserver(object[] values)
		{
			return ClassProperty.CheckRules<bool>(((string)values[0]).FlushResolver());
		}

		internal int SelectObserver(ImporterMapper c1, ImporterMapper c2)
		{
			return string.Compare(c1.requestMapper, c2.requestMapper, StringComparison.Ordinal);
		}

		internal int RemoveObserver(EventMapper l1, EventMapper l2)
		{
			return string.Compare(l1._InfoMapper.name, l2._InfoMapper.name, StringComparison.Ordinal);
		}

		internal string InstantiateObserver(string n)
		{
			return n;
		}

		internal bool AwakeObserver(ImporterMapper c2)
		{
			return c2.listenerMapper.Count > 0;
		}

		internal AnimatorState ResetObserver(ChildAnimatorState s)
		{
			return s.state;
		}

		internal AnimatorState FlushObserver(ChildAnimatorState cs)
		{
			return cs.state;
		}

		internal string ConnectObserver(ChildAnimatorState cs)
		{
			return ClassProperty.RegisterRules(cs.state.name);
		}

		internal object CalculateObserver()
		{
			return _ReaderVisitor.GetMethod("get_renameOverlay").Invoke(ReadAnnotation(), null);
		}

		internal void TestObserver(bool accepted)
		{
			if (accepted)
			{
				RestartAlgo(RevertMapper(), m_AlgoAnnotation, _IssuerVisitor.ListSerializer());
			}
		}

		internal bool MapObserver(MethodInfo m)
		{
			return m.Name == "AddState";
		}

		internal bool ValidateObserver(MethodInfo m2)
		{
			if (!(m2.Name == "CreateEdges"))
			{
				return false;
			}
			return m2.GetParameters().Length == 3;
		}

		internal bool CustomizeObserver(UnityEngine.Object o)
		{
			if (o is GameObject { scene: var scene })
			{
				return scene.isLoaded;
			}
			return false;
		}

		internal bool RateObserver(UnityEngine.Object o)
		{
			return o is Motion;
		}

		internal void DestroyObserver(object data)
		{
			AnimatorStateTransition col = null;
			if (!(data is AnimatorState destinationState))
			{
				if (data is AnimatorStateMachine animatorStateMachine)
				{
					col = ((!animatorStateMachine.states.GetRules()) ? ManageMapper().AddAnyStateTransition(animatorStateMachine) : ManageMapper().anyStateTransitions.Last());
				}
			}
			else
			{
				col = ManageMapper().AddAnyStateTransition(destinationState);
			}
			CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, col);
		}

		internal static bool ReflectInfo()
		{
			return StopInfo == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass186_0
	{
		public string m_ObjectDefinition;

		internal static _003C_003Ec__DisplayClass186_0 ConcatInfo;

		internal string RunObserver(string key, ref _003C_003Ec__DisplayClass186_1 _003C_003Ec__DisplayClass186_1_0, ref _003C_003Ec__DisplayClass186_2 _003C_003Ec__DisplayClass186_2_0)
		{
			return RateMapper(SessionState.GetString(DestroyMapper(m_ObjectDefinition + key, ref _003C_003Ec__DisplayClass186_2_0), string.Empty), ref _003C_003Ec__DisplayClass186_1_0);
		}

		internal void CloneObserver()
		{
			List<(string, string)> list = RegisterAnnotation("verifylicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				_003C_003Ec__DisplayClass186_3 _003C_003Ec__DisplayClass186_ = new _003C_003Ec__DisplayClass186_3();
				while (true)
				{
					_003C_003Ec__DisplayClass186_.m_MerchantDefinition = this;
					_003C_003Ec__DisplayClass186_.valueDefinition = response;
				}
			}, _003C_003Ec.watcherInitializer.CollectProperty, null, null, InsertVisitor);
		}

		internal void LoginObserver(FieldAlgo response)
		{
			_003C_003Ec__DisplayClass186_3 _003C_003Ec__DisplayClass186_ = new _003C_003Ec__DisplayClass186_3();
			while (true)
			{
				_003C_003Ec__DisplayClass186_.m_MerchantDefinition = this;
				_003C_003Ec__DisplayClass186_.valueDefinition = response;
			}
		}

		internal void ReflectObserver(string key, string value, ref _003C_003Ec__DisplayClass186_4 _003C_003Ec__DisplayClass186_4_0, ref _003C_003Ec__DisplayClass186_5 _003C_003Ec__DisplayClass186_5_0)
		{
			SessionState.SetString(GetMapper(m_ObjectDefinition + key, ref _003C_003Ec__DisplayClass186_5_0), CalcMapper(value, ref _003C_003Ec__DisplayClass186_4_0));
		}

		internal static bool CollectInfo()
		{
			return ConcatInfo == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass186_1
	{
		public AesManaged utilsDefinition;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass186_2
	{
		public HMACSHA1 _ValDefinition;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass186_3
	{
		public FieldAlgo valueDefinition;

		public _003C_003Ec__DisplayClass186_0 m_MerchantDefinition;

		internal static _003C_003Ec__DisplayClass186_3 WriteInfo;

		internal void DeleteObserver()
		{
			try
			{
				string text = valueDefinition.InsertReg("date");
				if (PatchAnnotation() != text)
				{
					FindVisitor("Date Mismatch! Please make sure your system's date is correct.\nLocal: " + m_TagAnnotation + "  |  Remote: " + text, CustomLogType.Error);
					attrAnnotation = true;
					SearchAnnotation();
					return;
				}
				m_FilterAnnotation = valueDefinition.InsertReg("username");
				m_ReaderAnnotation = valueDefinition.InsertReg("variant");
				m_ParamsAnnotation = valueDefinition.InsertReg("token");
				StopAnnotation();
				CheckAnnotation();
				string param = valueDefinition.InsertReg("message");
				if (!m_GetterAnnotation)
				{
					FindVisitor(param);
				}
				listenerAnnotation = true;
				ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: true);
				EditorPrefs.SetString("yOk0XCnENLMO6DIF8cYpSg==LK", m_BridgeAnnotation);
				_003C_003Ec__DisplayClass186_4 _003C_003Ec__DisplayClass186_4_ = default(_003C_003Ec__DisplayClass186_4);
				_003C_003Ec__DisplayClass186_4_.authenticationDefinition = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass186_4_.authenticationDefinition.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
					_003C_003Ec__DisplayClass186_4_.authenticationDefinition.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
					_003C_003Ec__DisplayClass186_5 _003C_003Ec__DisplayClass186_5_ = default(_003C_003Ec__DisplayClass186_5);
					_003C_003Ec__DisplayClass186_5_.m_ReponseDefinition = new HMACSHA1(Encoding.UTF8.GetBytes(m_MerchantDefinition.m_ObjectDefinition));
					try
					{
						m_MerchantDefinition.ReflectObserver("date", m_TagAnnotation, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("u", m_FilterAnnotation, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("v", m_ReaderAnnotation, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("r", m_ParamsAnnotation, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("m", _WriterAnnotation, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
					}
					finally
					{
						if (_003C_003Ec__DisplayClass186_5_.m_ReponseDefinition != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass186_5_.m_ReponseDefinition).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass186_4_.authenticationDefinition != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass186_4_.authenticationDefinition).Dispose();
					}
				}
				SessionState.SetBool(m_MerchantDefinition.m_ObjectDefinition, value: true);
				if (!new Func<bool>(_003C_003Ec.watcherInitializer.ViewProperty)())
				{
					SearchAnnotation();
				}
				ManageAnnotation(applyident: false);
			}
			catch (System.Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		internal static bool RemoveInfo()
		{
			return WriteInfo == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass186_4
	{
		public AesManaged authenticationDefinition;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass186_5
	{
		public HMACSHA1 m_ReponseDefinition;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass192_0
	{
		public bool _PoolDefinition;

		public string _ParameterDefinition;

		public StringBuilder composerDefinition;

		public string[] repositoryDefinition;

		public string[] mappingDefinition;

		public string[] baseDefinition;

		public string[][] containerDefinition;

		public StringBuilder classDefinition;

		public CancellationTokenSource _MockDefinition;

		public PoolAlgo[] instanceDefinition;

		public bool _FieldDefinition;

		public Action m_AttributeDefinition;

		internal static _003C_003Ec__DisplayClass192_0 PushInfo;

		internal string CreateObserver(string property, string[] extractedValues)
		{
			string text = extractedValues.FirstOrDefault(_003C_003Ec.watcherInitializer.WriteProperty);
			if (!_PoolDefinition)
			{
				_003C_003Ec__DisplayClass192_3 _003C_003Ec__DisplayClass192_ = new _003C_003Ec__DisplayClass192_3();
				text = (ResolveMapper(_ParameterDefinition, property, out _003C_003Ec__DisplayClass192_.m_DescriptorDefinition) ? (extractedValues.FirstOrDefault(_003C_003Ec__DisplayClass192_.LogoutObserver) ?? text) : text);
			}
			composerDefinition.AppendLine(property + ": " + text);
			return text;
		}

		internal void NewObserver(string o)
		{
			repositoryDefinition[0] = o;
		}

		internal void PushObserver(string o)
		{
			repositoryDefinition[1] = o;
		}

		internal void ViewObserver(string o)
		{
			repositoryDefinition[2] = o;
		}

		internal void CollectObserver(string o)
		{
			repositoryDefinition[3] = o;
		}

		internal void ResolveObserver(string o)
		{
			mappingDefinition[0] = o;
		}

		internal void ListObserver(string o)
		{
			mappingDefinition[1] = o;
		}

		internal void VerifyObserver(string o)
		{
			mappingDefinition[2] = o;
		}

		internal void FillObserver(string o)
		{
			mappingDefinition[3] = o;
		}

		internal bool WriteObserver((List<string>, Dictionary<string, RangeInt>) cmdParsedOutput, string property, out string result)
		{
			if (CollectMapper(cmdParsedOutput, property, out var c))
			{
				result = CreateObserver(property, c);
				return true;
			}
			result = "Default String";
			return false;
		}

		internal bool ForgotObserver(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			if (ViewMapper(fullInfo, properties[0], out var control))
			{
				(bool, string)[] array = new(bool, string)[properties.Length];
				for (int i = 0; i < properties.Length; i++)
				{
					string result2;
					bool item = WriteObserver(control, properties[i], out result2);
					array[i] = (item, result2);
				}
				int num = Mathf.CeilToInt((float)array.Length / 2f);
				if (array.Count(_003C_003Ec.watcherInitializer.ForgotProperty) < num)
				{
					result = string.Join(string.Empty, array.Select(_003C_003Ec.watcherInitializer.StopProperty)).Replace(" ", string.Empty);
					return true;
				}
				return false;
			}
			return false;
		}

		internal void StopObserver()
		{
			try
			{
				UpdateObserver(isCMD: true);
				SortObserver();
			}
			catch (System.Exception exc)
			{
				ChangeObserver(isCMD: true, exc);
			}
		}

		internal bool CheckObserver(string fullInfo, string property, out string result)
		{
			if (ResolveMapper(fullInfo, property, out var consumer))
			{
				result = CreateObserver(property, consumer);
				return true;
			}
			result = "Default String";
			return false;
		}

		internal bool PrepareObserver(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			(bool, string)[] array = new(bool, string)[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				string result2;
				bool item = CheckObserver(fullInfo, properties[i], out result2);
				array[i] = (item, result2);
			}
			if (array.All(_003C_003Ec.watcherInitializer.CheckProperty))
			{
				return false;
			}
			result = string.Join(string.Empty, array.Select(_003C_003Ec.watcherInitializer.PrepareProperty)).Replace(" ", string.Empty);
			return true;
		}

		internal void AssetObserver()
		{
			try
			{
				UpdateObserver(isCMD: false);
				SortObserver();
			}
			catch (System.Exception exc)
			{
				ChangeObserver(isCMD: false, exc);
			}
		}

		internal void UpdateObserver(bool isCMD)
		{
			bool[] array = new bool[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = (isCMD ? ForgotObserver(repositoryDefinition[i], out baseDefinition[i], containerDefinition[i]) : PrepareObserver(mappingDefinition[i], out baseDefinition[i], containerDefinition[i]));
			}
			bool num = array[0] || array[1];
			bool flag = num;
			if ((!num || !array[2]) && (!flag || !array[3]) && (!array[2] || !array[3]))
			{
				throw new System.Exception("Failed to gather hardware info through " + ((!isCMD) ? "Shell" : "CMD"));
			}
		}

		internal void ChangeObserver(bool isCMD, System.Exception exc)
		{
			if (!isCMD)
			{
				_IndexerAnnotation = false;
				m_CallbackAnnotation = false;
				_RequestAnnotation = false;
				importerAnnotation = false;
			}
			string text = ((!isCMD) ? "Shell" : "CMD");
			classDefinition.AppendLine("Failed " + text + " Parse");
			classDefinition.AppendLine("Reason: " + exc.Message);
			classDefinition.AppendLine(exc.StackTrace + Environment.NewLine);
			string[] array = (isCMD ? repositoryDefinition : mappingDefinition);
			for (int i = 0; i < 4; i++)
			{
				classDefinition.AppendLine($"Info {i}:");
				try
				{
					classDefinition.AppendLine(array[i]);
				}
				catch
				{
					classDefinition.AppendLine($"Missing Info {i}!");
				}
			}
			if (!isCMD)
			{
				int num = EditorUtility.DisplayDialogComplex("Error!", "Generating HWID failed and cannot proceed!\nPlease try the 'Troubleshoot' instructions.\nIf troubleshooting didn't work, press 'Report'.", "Troubleshoot", "Close", "Report");
				if (num != 0)
				{
					if (num == 2)
					{
						string text2 = "HWIDInfo";
						string text3 = "Assets/" + text2;
						if (EditorUtility.DisplayDialog("Reporting", "Pressing the 'Proceed' button below will generate the file '" + text2 + "' in Assets with your Hardware Information for debugging purposes. Please send that file to @Dreadrith#3238", "Ok", "Cancel"))
						{
							File.WriteAllText(text3, CreateMapper(classDefinition.ToString()));
							AssetDatabase.ImportAsset(text3);
							EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(text3));
						}
					}
				}
				else
				{
					Application.OpenURL("https://dreadrith.com/HWIDHelp");
				}
				_RequestAnnotation = false;
				importerAnnotation = false;
				InsertVisitor();
				return;
			}
			_MockDefinition = new CancellationTokenSource();
			_MockDefinition.CancelAfter(10000);
			PushMapper(instanceDefinition, delegate
			{
				try
				{
					UpdateObserver(isCMD: false);
					SortObserver();
				}
				catch (System.Exception exc2)
				{
					ChangeObserver(isCMD: false, exc2);
				}
			}, _MockDefinition);
		}

		internal void SortObserver()
		{
			EditorPrefs.SetString("DSLICINF", CreateMapper(composerDefinition.ToString()));
			if (_FieldDefinition)
			{
				for (int i = 0; i < 4; i++)
				{
					baseDefinition[i] += "\r\r";
				}
			}
			string[] array = new string[3]
			{
				baseDefinition[0] + baseDefinition[1],
				baseDefinition[2],
				baseDefinition[3]
			};
			using (SHA1 sHA = SHA1.Create())
			{
				for (int j = 0; j < 3; j++)
				{
					array[j] = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(array[j]))).Replace("-", "");
				}
			}
			_WriterAnnotation = string.Join("-", array);
			CheckAnnotation();
			m_AttributeDefinition();
		}

		internal static bool PrepareInfo()
		{
			return PushInfo == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass192_1
	{
		public AesManaged clientDefinition;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass192_3
	{
		public string[] m_DescriptorDefinition;

		internal static _003C_003Ec__DisplayClass192_3 SetupInfo;

		internal bool LogoutObserver(string v)
		{
			return v == m_DescriptorDefinition[0];
		}

		internal static bool ExcludeInfo()
		{
			return SetupInfo == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass239_0
	{
		public FieldAlgo serviceDefinition;

		private static _003C_003Ec__DisplayClass239_0 InstantiateInfo;

		internal void InterruptObserver()
		{
			m_CustomerAnnotation = serviceDefinition.InsertReg("transfer_email");
			advisorAnnotation = true;
		}

		internal static bool RevertInfo()
		{
			return InstantiateInfo == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass285_0
	{
		public bool _IteratorDefinition;

		public string publisherDefinition;

		public AnimatorCondition m_ConfigurationDefinition;

		public bool m_ProcDefinition;

		public Func<AnimatorCondition, bool> wrapperReg;

		public Func<AnimatorStateTransition, bool> _AnnotationReg;

		public Func<AnimatorCondition, bool> _VisitorReg;

		public Func<AnimatorCondition, bool> m_AlgoReg;

		internal static _003C_003Ec__DisplayClass285_0 ConnectParser;

		internal void CompareServer(Rect targetRect)
		{
			Event current = Event.current;
			if (_IteratorDefinition && current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
			{
				GUI.FocusControl(string.Empty);
			}
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName(publisherDefinition);
			m_ConfigurationDefinition.parameter = EditorGUI.DelayedTextField((!_IteratorDefinition) ? Rect.zero : targetRect, m_ConfigurationDefinition.parameter);
			m_ProcDefinition = EditorGUI.EndChangeCheck();
			if (current.type == EventType.MouseUp && current.button == 1 && targetRect.Contains(current.mousePosition))
			{
				GUI.FocusControl(publisherDefinition);
				current.Use();
			}
		}

		internal IEnumerable<AnimatorStateTransition> SetServer(ChildAnimatorState s)
		{
			return s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true)));
		}

		internal bool PostServer(AnimatorStateTransition t)
		{
			return t.conditions.Any((AnimatorCondition c) => ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true));
		}

		internal bool SetupServer(AnimatorCondition c)
		{
			return ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true);
		}

		internal bool EnableServer(AnimatorStateTransition t)
		{
			return t.conditions.Any((AnimatorCondition c) => ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true));
		}

		internal bool PublishServer(AnimatorCondition c)
		{
			return ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true);
		}

		internal bool PopServer(AnimatorTransition t)
		{
			return t.conditions.Any((AnimatorCondition c) => ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true));
		}

		internal bool ComputeServer(AnimatorCondition c)
		{
			return ForgotVisitor(m_ConfigurationDefinition, c, forcetag: true);
		}

		internal static bool ViewParser()
		{
			return ConnectParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass308_0
	{
		public AnimatorStateTransition serverReg;

		internal static _003C_003Ec__DisplayClass308_0 RateParser;

		internal bool InvokeServer(ChildAnimatorState c)
		{
			return c.state == serverReg.destinationState;
		}

		internal static bool PostParser()
		{
			return RateParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass370_1
	{
		public string _ResolverReg;

		internal static _003C_003Ec__DisplayClass370_1 LogoutParser;

		internal bool VisitServer(UnityEngine.AnimatorControllerParameter p2)
		{
			return p2.name != _ResolverReg;
		}

		internal static bool FindParser()
		{
			return LogoutParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass375_0
	{
		public HashSet<UnityEngine.Object> consumerReg;

		public Action<AnimatorStateTransition> _AdapterReg;

		internal static _003C_003Ec__DisplayClass375_0 WriteParser;

		internal void ValidateServer(UnityEngine.Object o)
		{
			consumerReg.Add(o);
		}

		internal void CustomizeServer(AnimatorStateMachine m)
		{
			_003C_003Ec__DisplayClass375_1 _003C_003Ec__DisplayClass375_ = new _003C_003Ec__DisplayClass375_1
			{
				watcherReg = this,
				interpreterReg = m
			};
			ValidateServer(_003C_003Ec__DisplayClass375_.interpreterReg);
			_003C_003Ec__DisplayClass375_.interpreterReg.states.InvokeResolver(delegate(ChildAnimatorState s)
			{
				ValidateServer(s.state);
				s.state.transitions.InvokeResolver(delegate(AnimatorStateTransition t)
				{
					if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
					{
						ValidateServer(t);
					}
				});
				((IEnumerable<StateMachineBehaviour>)s.state.behaviours).InvokeResolver((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
				{
					consumerReg.Add(o);
				});
				s.state.motion.ForgotPredicate((Action<Motion>)delegate(UnityEngine.Object o)
				{
					consumerReg.Add(o);
				});
			});
			((IEnumerable<StateMachineBehaviour>)_003C_003Ec__DisplayClass375_.interpreterReg.behaviours).InvokeResolver((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
			{
				consumerReg.Add(o);
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.entryTransitions.InvokeResolver(delegate(AnimatorTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.anyStateTransitions.InvokeResolver(delegate(AnimatorStateTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.stateMachines.InvokeResolver(_003C_003Ec__DisplayClass375_.RunServer);
		}

		internal void RateServer(ChildAnimatorState s)
		{
			ValidateServer(s.state);
			s.state.transitions.InvokeResolver(delegate(AnimatorStateTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			((IEnumerable<StateMachineBehaviour>)s.state.behaviours).InvokeResolver((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
			{
				consumerReg.Add(o);
			});
			s.state.motion.ForgotPredicate((Action<Motion>)delegate(UnityEngine.Object o)
			{
				consumerReg.Add(o);
			});
		}

		internal void DestroyServer(AnimatorStateTransition t)
		{
			if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
			{
				ValidateServer(t);
			}
		}

		internal void GetServer(AnimatorTransition t)
		{
			if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
			{
				ValidateServer(t);
			}
		}

		internal void CalcServer(AnimatorStateTransition t)
		{
			if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
			{
				ValidateServer(t);
			}
		}

		internal void IncludeServer(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			CustomizeServer(l.stateMachine);
		}

		internal static bool RemoveParser()
		{
			return WriteParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass375_1
	{
		public AnimatorStateMachine interpreterReg;

		public _003C_003Ec__DisplayClass375_0 watcherReg;

		internal static _003C_003Ec__DisplayClass375_1 MapParser;

		internal void RunServer(ChildAnimatorStateMachine c)
		{
			interpreterReg.GetStateMachineTransitions(c.stateMachine).InvokeResolver(watcherReg.ValidateServer);
			watcherReg.CustomizeServer(c.stateMachine);
		}

		internal static bool AddParser()
		{
			return MapParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass378_0
	{
		public Dictionary<string, string> _CandidateReg;

		public UnityEditor.Animations.AnimatorController m_ProductReg;

		public Dictionary<Motion, Motion> m_ExpressionReg;

		public HashSet<Motion> systemReg;

		public Action<AnimatorState> _WorkerReg;

		public Action<AnimatorState> _FilterReg;

		private static _003C_003Ec__DisplayClass378_0 NewParser;

		internal string CloneServer(string name)
		{
			MatchCollection matchCollection = Regex.Matches(name, "%(.+?)%");
			if (matchCollection.Count > 0)
			{
				for (int num = matchCollection.Count - 1; num >= 0; num--)
				{
					Match match = matchCollection[num];
					string text = match.Groups[1].Value;
					if (_CandidateReg.TryGetValue(text, out var value))
					{
						text = value;
					}
					name = name.Remove(match.Index, match.Length).Insert(match.Index, text);
				}
			}
			return name;
		}

		internal void LoginServer(AnimatorState s)
		{
			s.name = CloneServer(s.name);
		}

		internal T ReflectServer<T>(T m) where T : Motion
		{
			string text = ((!(m is AnimationClip)) ? ".asset" : ".anim");
			string text2 = $"{ConsumerAlgo.CallDefinition().saveFolder}/Animation Clips/{m_ProductReg.name}";
			ClassProperty.InstantiateList(text2, overridecust: false, ClassProperty.PathOption.ForceFolder);
			string text3 = ClassProperty.AwakeList(text2, m.name + text, writestate: true);
			bool proc;
			T val = ClassProperty.SetRules(m, text3, out proc, isinfo2: false);
			if (proc)
			{
				ClassProperty.InterruptPredicate(val, m_ProductReg);
			}
			else
			{
				AssetDatabase.ImportAsset(text3);
			}
			m_ExpressionReg.Add(m, val);
			return val;
		}

		internal Motion DeleteServer(Motion m)
		{
			if ((bool)m)
			{
				if (!m_ExpressionReg.TryGetValue(m, out var value))
				{
					if (!systemReg.Add(m))
					{
						return m;
					}
					if (!(m is UnityEditor.Animations.BlendTree blendTree))
					{
						AnimationClip animationClip = (AnimationClip)m;
						AnimationClip animationClip2 = animationClip;
						bool flag = false;
						if (animationClip.name.StartsWith("T_"))
						{
							animationClip2 = ReflectServer(animationClip);
							flag = true;
						}
						EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animationClip);
						for (int i = 0; i < curveBindings.Length; i++)
						{
							EditorCurveBinding binding = curveBindings[i];
							if (!(binding.type != typeof(Animator)) && !(binding.path != string.Empty) && _CandidateReg.ContainsKey(binding.propertyName))
							{
								if (!flag)
								{
									animationClip2 = ReflectServer(animationClip);
									flag = true;
								}
								AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(animationClip2, binding);
								AnimationUtility.SetEditorCurve(animationClip2, binding, null);
								binding.propertyName = _CandidateReg[binding.propertyName];
								AnimationUtility.SetEditorCurve(animationClip2, binding, editorCurve);
								EditorUtility.SetDirty(animationClip2);
							}
						}
						return animationClip2;
					}
					UnityEditor.Animations.BlendTree blendTree2 = blendTree;
					bool flag2 = false;
					if (blendTree.name.StartsWith("T_"))
					{
						flag2 = true;
						blendTree2 = ReflectServer(blendTree);
					}
					ChildMotion[] children = blendTree.children;
					for (int j = 0; j < children.Length; j++)
					{
						Motion motion = children[j].motion;
						Motion motion2 = DeleteServer(motion);
						if (!flag2 && motion != motion2)
						{
							flag2 = true;
							blendTree2 = ReflectServer(blendTree);
						}
						children[j].motion = motion2;
					}
					blendTree2.children = children;
					EditorUtility.SetDirty(blendTree2);
					return blendTree2;
				}
				return value;
			}
			return null;
		}

		internal void CreateServer(AnimatorState s)
		{
			s.motion = DeleteServer(s.motion);
		}

		internal static bool LoginParser()
		{
			return NewParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass379_0
	{
		public bool _ReaderReg;

		public string m_BridgeReg;

		public string m_StrategyReg;

		internal static _003C_003Ec__DisplayClass379_0 PrintParser;

		internal bool PushServer(string s)
		{
			if (!_ReaderReg || !(s == m_BridgeReg))
			{
				if (!_ReaderReg)
				{
					return s.Contains(m_BridgeReg);
				}
				return false;
			}
			return true;
		}

		internal string ViewServer(string s)
		{
			if (string.IsNullOrEmpty(s) || !PushServer(s))
			{
				return s;
			}
			return s.Replace(m_BridgeReg, m_StrategyReg);
		}

		internal void CollectServer(StateMachineBehaviour[] behaviours)
		{
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (!(stateMachineBehaviour.GetType() == WorkerProperty.InsertPage()))
				{
					continue;
				}
				WorkerProperty.BridgeProperty bridgeProperty = new WorkerProperty.BridgeProperty(stateMachineBehaviour);
				for (int num = bridgeProperty.m_DatabaseProperty.Count - 1; num >= 0; num--)
				{
					if (PushServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()))
					{
						bridgeProperty.m_DatabaseProperty[num].FlushPage(ViewServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()));
					}
					if (PushServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()))
					{
						bridgeProperty.m_DatabaseProperty[num].TestPage(ViewServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()));
					}
				}
				EditorUtility.SetDirty(stateMachineBehaviour);
			}
		}

		internal void ResolveServer(Motion motion)
		{
			if (!(motion is UnityEditor.Animations.BlendTree blendTree))
			{
				return;
			}
			blendTree.blendParameter = ViewServer(blendTree.blendParameter);
			blendTree.blendParameterY = ViewServer(blendTree.blendParameterY);
			EditorUtility.SetDirty(blendTree);
			foreach (Motion item in blendTree.children.Select(_003C_003Ec.watcherInitializer.ResolveProcessor))
			{
				ResolveServer(item);
			}
		}

		internal void ListServer(AnimatorState s)
		{
			if (s.cycleOffsetParameterActive)
			{
				s.cycleOffsetParameter = ViewServer(s.cycleOffsetParameter);
			}
			if (s.mirrorParameterActive)
			{
				s.mirrorParameter = ViewServer(s.mirrorParameter);
			}
			if (s.speedParameterActive)
			{
				s.speedParameter = ViewServer(s.speedParameter);
			}
			if (s.timeParameterActive)
			{
				s.timeParameter = ViewServer(s.timeParameter);
			}
			ResolveServer(s.motion);
			for (int num = s.transitions.Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] conditions = s.transitions[num].conditions;
				for (int num2 = s.transitions[num].conditions.Length - 1; num2 >= 0; num2--)
				{
					conditions[num2].parameter = ViewServer(conditions[num2].parameter);
				}
				s.transitions[num].conditions = conditions;
			}
			EditorUtility.SetDirty(s);
			if (WorkerProperty.InvokePage())
			{
				CollectServer(s.behaviours);
			}
		}

		internal void VerifyServer(InstanceServer t)
		{
			for (int num = t.SetContext().Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] array = t.SetContext();
				array[num].parameter = ViewServer(array[num].parameter);
				t.PostContext(array);
			}
			EditorUtility.SetDirty((AnimatorTransitionBase)t);
		}

		internal static bool ResolveParser()
		{
			return PrintParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass380_0
	{
		public bool customerReg;

		public string _DatabaseReg;

		public string m_ExporterReg;

		internal static _003C_003Ec__DisplayClass380_0 AssetParser;

		internal bool FillServer(string s)
		{
			if (!customerReg || !(s == _DatabaseReg))
			{
				if (customerReg)
				{
					return false;
				}
				return s.Contains(_DatabaseReg);
			}
			return true;
		}

		internal string WriteServer(string s)
		{
			if (string.IsNullOrEmpty(s) || !FillServer(s))
			{
				return s;
			}
			return s.Replace(_DatabaseReg, m_ExporterReg);
		}

		internal void ForgotServer(StateMachineBehaviour[] behaviours)
		{
			if (behaviours == null)
			{
				return;
			}
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (!(stateMachineBehaviour.GetType() == WorkerProperty.InsertPage()))
				{
					continue;
				}
				WorkerProperty.BridgeProperty bridgeProperty = new WorkerProperty.BridgeProperty(stateMachineBehaviour);
				for (int num = bridgeProperty.m_DatabaseProperty.Count - 1; num >= 0; num--)
				{
					if (FillServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()))
					{
						bridgeProperty.m_DatabaseProperty[num].FlushPage(WriteServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()));
					}
					if (FillServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()))
					{
						bridgeProperty.m_DatabaseProperty[num].TestPage(WriteServer(bridgeProperty.m_DatabaseProperty[num].CalculatePage()));
					}
				}
				EditorUtility.SetDirty(stateMachineBehaviour);
			}
		}

		internal void StopServer(AnimatorStateMachine m)
		{
			ForgotServer(m.behaviours);
			ChildAnimatorState[] states = m.states;
			foreach (ChildAnimatorState childAnimatorState in states)
			{
				ForgotServer(childAnimatorState.state.behaviours);
			}
		}

		internal static bool SelectParser()
		{
			return AssetParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass381_0
	{
		public string _IdentifierReg;

		public Action<StateMachineBehaviour[]> _AttrReg;

		public AnimatorStateMachine m_DispatcherReg;

		private static _003C_003Ec__DisplayClass381_0 SetupParser;

		internal bool CheckServer(string s)
		{
			if (!structAnnotation || !(s == _IdentifierReg))
			{
				if (!structAnnotation)
				{
					return s.Contains(_IdentifierReg);
				}
				return false;
			}
			return true;
		}

		internal void PrepareServer<T>(T[] transitions, Action<T> removeTransitionAction) where T : AnimatorTransitionBase
		{
			if (transitions == null)
			{
				return;
			}
			for (int num = transitions.Length - 1; num >= 0; num--)
			{
				T val = transitions[num];
				for (int num2 = val.conditions.Length - 1; num2 >= 0; num2--)
				{
					AnimatorCondition condition = val.conditions[num2];
					if (CheckServer(condition.parameter))
					{
						val.RemoveCondition(condition);
						if (val.conditions.Length == 0)
						{
							removeTransitionAction(val);
						}
					}
				}
			}
		}

		internal void AssetServer(StateMachineBehaviour[] b)
		{
			foreach (StateMachineBehaviour stateMachineBehaviour in b)
			{
				if (stateMachineBehaviour.GetType() != WorkerProperty.InsertPage())
				{
					continue;
				}
				WorkerProperty.BridgeProperty bridgeProperty = new WorkerProperty.BridgeProperty(stateMachineBehaviour);
				for (int num = bridgeProperty.m_DatabaseProperty.Count - 1; num >= 0; num--)
				{
					if (CheckServer(bridgeProperty.m_DatabaseProperty[num].ResetPage()))
					{
						bridgeProperty.InitPage(num);
					}
				}
				EditorUtility.SetDirty(stateMachineBehaviour);
			}
		}

		internal void UpdateServer(AnimatorState s)
		{
			if (s.cycleOffsetParameterActive && CheckServer(s.cycleOffsetParameter))
			{
				s.cycleOffsetParameterActive = false;
			}
			if (s.mirrorParameterActive && CheckServer(s.mirrorParameter))
			{
				s.mirrorParameterActive = false;
			}
			if (s.speedParameterActive && CheckServer(s.speedParameter))
			{
				s.speedParameterActive = false;
			}
			if (s.timeParameterActive && CheckServer(s.timeParameter))
			{
				s.timeParameterActive = false;
			}
			PrepareServer(s.transitions, s.RemoveTransition);
			if (WorkerProperty.InvokePage())
			{
				_AttrReg(s.behaviours);
			}
		}

		internal void ChangeServer(AnimatorTransition t)
		{
			m_DispatcherReg.RemoveEntryTransition(t);
		}

		internal void SortServer(AnimatorStateTransition t)
		{
			m_DispatcherReg.RemoveAnyStateTransition(t);
		}

		internal static bool ExcludeParser()
		{
			return SetupParser == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass382_0
	{
		public string _RegistryReg;

		internal static _003C_003Ec__DisplayClass382_0 ListParser;

		internal string RegisterServer(string s)
		{
			if (string.IsNullOrEmpty(s) || ClassProperty.m_AdapterProcessor.Contains(s))
			{
				return s;
			}
			return s + _RegistryReg;
		}

		internal void LogoutServer(AnimatorState s)
		{
			s.mirrorParameter = RegisterServer(s.mirrorParameter);
			s.cycleOffsetParameter = RegisterServer(s.cycleOffsetParameter);
			s.speedParameter = RegisterServer(s.speedParameter);
			s.timeParameter = RegisterServer(s.timeParameter);
			StateMachineBehaviour[] behaviours = s.behaviours;
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (!(stateMachineBehaviour.GetType() == WorkerProperty.InsertPage()))
				{
					continue;
				}
				foreach (WorkerProperty.BridgeProperty.AttrProperty item in new WorkerProperty.BridgeProperty(stateMachineBehaviour).m_DatabaseProperty)
				{
					item.FlushPage(RegisterServer(item.ResetPage()));
					EditorUtility.SetDirty(stateMachineBehaviour);
				}
			}
			for (int j = 0; j < s.transitions.Length; j++)
			{
				AnimatorStateTransition animatorStateTransition = s.transitions[j];
				AnimatorCondition[] conditions = animatorStateTransition.conditions;
				for (int k = 0; k < animatorStateTransition.conditions.Length; k++)
				{
					conditions[k].parameter = RegisterServer(conditions[k].parameter);
				}
				animatorStateTransition.conditions = conditions;
				EditorUtility.SetDirty(animatorStateTransition);
			}
		}

		internal void PatchServer(AnimatorStateMachine m)
		{
			m.name += _RegistryReg;
			StateMachineBehaviour[] behaviours = m.behaviours;
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (!(stateMachineBehaviour.GetType() == WorkerProperty.InsertPage()))
				{
					continue;
				}
				foreach (WorkerProperty.BridgeProperty.AttrProperty item in new WorkerProperty.BridgeProperty(stateMachineBehaviour).m_DatabaseProperty)
				{
					item.FlushPage(RegisterServer(item.ResetPage()));
				}
				EditorUtility.SetDirty(stateMachineBehaviour);
			}
			for (int j = 0; j < m.entryTransitions.Length; j++)
			{
				AnimatorTransition animatorTransition = m.entryTransitions[j];
				AnimatorCondition[] conditions = animatorTransition.conditions;
				for (int k = 0; k < animatorTransition.conditions.Length; k++)
				{
					conditions[k].parameter = RegisterServer(conditions[k].parameter);
				}
				animatorTransition.conditions = conditions;
				EditorUtility.SetDirty(animatorTransition);
			}
			ChildAnimatorStateMachine[] stateMachines = m.stateMachines;
			for (int i = 0; i < stateMachines.Length; i++)
			{
				ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
				if (childAnimatorStateMachine.stateMachine != m)
				{
					PatchServer(childAnimatorStateMachine.stateMachine);
				}
			}
		}

		internal static bool CalcParser()
		{
			return ListParser == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass383_1
	{
		public bool m_RequestReg;

		public string m_PrinterReg;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass383_2
	{
		public UnityEditor.Animations.AnimatorControllerLayer _WriterReg;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass383_3
	{
		public AnimatorState _ParamsReg;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass384_0
	{
		public string listenerReg;

		public UnityEngine.AnimatorControllerParameterType m_GetterReg;

		public UnityEngine.AnimatorControllerParameterType m_InterceptorReg;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass410_1
	{
		public AnimatorStateTransition _AccountReg;

		internal static _003C_003Ec__DisplayClass410_1 RateIdentifier;

		internal bool SetupThread(ChildAnimatorState c)
		{
			return _AccountReg.destinationState == c.state;
		}

		internal bool EnableThread(ChildAnimatorStateMachine c)
		{
			return _AccountReg.destinationStateMachine == c.stateMachine;
		}

		internal static bool PostIdentifier()
		{
			return RateIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_0
	{
		public List<AnimatorTransitionBase> tokenReg;

		public List<AnimatorStateTransition> codeReg;

		public List<AnimatorTransitionBase> m_DicReg;

		public Func<AnimatorStateTransition, bool> _InvocationReg;

		public Func<AnimatorTransitionBase, bool> roleReg;

		private static _003C_003Ec__DisplayClass413_0 DisableIdentifier;

		internal void PopThread(List<AnimatorStateTransition> t, AnimatorState s)
		{
			_003C_003Ec__DisplayClass413_1 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_1();
			if (t.Count != 1)
			{
				_003C_003Ec__DisplayClass413_._ParamReg = CalculateAlgo(t[0]);
				s.RemoveTransition(t[0]);
				for (int i = 1; i < t.Count; i++)
				{
					t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.CancelThread);
					s.RemoveTransition(t[i]);
				}
				s.AddTransition(_003C_003Ec__DisplayClass413_._ParamReg);
				tokenReg.Add(_003C_003Ec__DisplayClass413_._ParamReg);
			}
			else
			{
				tokenReg.Add(t[0]);
			}
		}

		internal void ComputeThread(List<AnimatorTransitionBase> t)
		{
			_003C_003Ec__DisplayClass413_2 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_2
			{
				_TokenizerReg = RevertMapper().AddEntryTransition(t[0].destinationState)
			};
			EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._TokenizerReg);
			RevertMapper().RemoveEntryTransition((AnimatorTransition)t[0]);
			for (int i = 1; i < t.Count; i++)
			{
				t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.CountThread);
				RevertMapper().RemoveEntryTransition((AnimatorTransition)t[i]);
			}
			tokenReg.Add(_003C_003Ec__DisplayClass413_._TokenizerReg);
		}

		internal void MoveThread(List<AnimatorStateTransition> t)
		{
			_003C_003Ec__DisplayClass413_3 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_3
			{
				_ComparatorReg = RevertMapper().AddAnyStateTransition(t[0].destinationState)
			};
			EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._ComparatorReg);
			RevertMapper().RemoveAnyStateTransition(t[0]);
			for (int i = 1; i < t.Count; i++)
			{
				t[i].conditions.InvokeResolver(_003C_003Ec__DisplayClass413_.DisableThread);
				RevertMapper().RemoveAnyStateTransition(t[i]);
			}
			tokenReg.Add(_003C_003Ec__DisplayClass413_._ComparatorReg);
		}

		internal bool ConcatThread(AnimatorStateTransition t)
		{
			return t.destinationState == codeReg[0].destinationState;
		}

		internal bool CallThread(AnimatorTransitionBase t)
		{
			return t.destinationState == m_DicReg[0].destinationState;
		}

		internal static bool VerifyIdentifier()
		{
			return DisableIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_1
	{
		public AnimatorStateTransition _ParamReg;

		public Action<AnimatorCondition> _ModelReg;

		internal static _003C_003Ec__DisplayClass413_1 ConcatIdentifier;

		internal void CancelThread(AnimatorCondition c)
		{
			_ParamReg.AddCondition(c.mode, c.threshold, c.parameter);
		}

		internal static bool CollectIdentifier()
		{
			return ConcatIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_2
	{
		public AnimatorTransitionBase _TokenizerReg;

		public Action<AnimatorCondition> decoratorReg;

		internal static _003C_003Ec__DisplayClass413_2 LogoutIdentifier;

		internal void CountThread(AnimatorCondition c)
		{
			_TokenizerReg.AddCondition(c.mode, c.threshold, c.parameter);
		}

		internal static bool FindIdentifier()
		{
			return LogoutIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_3
	{
		public AnimatorStateTransition _ComparatorReg;

		public Action<AnimatorCondition> m_ExceptionReg;

		internal static _003C_003Ec__DisplayClass413_3 TestIdentifier;

		internal void DisableThread(AnimatorCondition c)
		{
			_ComparatorReg.AddCondition(c.mode, c.threshold, c.parameter);
		}

		internal static bool IncludeIdentifier()
		{
			return TestIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass420_1
	{
		public AnimatorStateTransition _ValueReg;

		private static _003C_003Ec__DisplayClass420_1 NewIdentifier;

		internal bool QueryThread(ChildAnimatorState s)
		{
			return s.state == _ValueReg.destinationState;
		}

		internal static bool LoginIdentifier()
		{
			return NewIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass430_0
	{
		public SetterTests.InstanceTests m_MerchantReg;

		private static _003C_003Ec__DisplayClass430_0 PushIdentifier;

		internal void AddThread(AnimatorState s)
		{
			TestAlgo(m_MerchantReg.itemTests, s.AddTransition((AnimatorState)null));
		}

		internal static bool PrepareIdentifier()
		{
			return PushIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass431_0
	{
		public HashSet<UnityEngine.Object> authenticationReg;

		private static _003C_003Ec__DisplayClass431_0 PrintIdentifier;

		internal void InvokeThread(AnimatorStateTransition sel)
		{
			_003C_003Ec__DisplayClass431_1 _003C_003Ec__DisplayClass431_ = new _003C_003Ec__DisplayClass431_1
			{
				m_PoolReg = this,
				_ReponseReg = sel
			};
			if (!(_003C_003Ec__DisplayClass431_._ReponseReg.destinationState != null) && !(_003C_003Ec__DisplayClass431_._ReponseReg.destinationStateMachine != null))
			{
				return;
			}
			if (!ManageMapper().anyStateTransitions.Any(_003C_003Ec__DisplayClass431_.FindThread))
			{
				authenticationReg.Add(MapAlgo(_003C_003Ec__DisplayClass431_._ReponseReg));
				int num = 0;
				while (true)
				{
					if (num >= RevertMapper().states.Length)
					{
						return;
					}
					if (RevertMapper().states[num].state.transitions.Any(_003C_003Ec__DisplayClass431_.InitThread))
					{
						break;
					}
					num++;
				}
				Undo.RecordObject(RevertMapper().states[num].state, "Make AnyTransition");
				RevertMapper().states[num].state.RemoveTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
			}
			else
			{
				RevertMapper().states.InvokeResolver(_003C_003Ec__DisplayClass431_.ExcludeThread);
				_ = ManageMapper().anyStateTransitions;
				Undo.RecordObject(ManageMapper(), "Remove AnyTransition");
				ManageMapper().RemoveAnyStateTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
			}
		}

		internal static bool ResolveIdentifier()
		{
			return PrintIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass431_1
	{
		public AnimatorStateTransition _ReponseReg;

		public _003C_003Ec__DisplayClass431_0 m_PoolReg;

		public Func<AnimatorStateTransition, bool> _ParameterReg;

		internal static _003C_003Ec__DisplayClass431_1 AssetIdentifier;

		internal bool FindThread(AnimatorStateTransition t)
		{
			return t == _ReponseReg;
		}

		internal void ExcludeThread(ChildAnimatorState c)
		{
			if (c.state != _ReponseReg.destinationState || (c.state == _ReponseReg.destinationState && _ReponseReg.canTransitionToSelf))
			{
				AnimatorStateTransition animatorStateTransition = CalculateAlgo(_ReponseReg);
				m_PoolReg.authenticationReg.Add(animatorStateTransition);
				c.state.AddTransition(animatorStateTransition);
			}
		}

		internal bool InitThread(AnimatorStateTransition t)
		{
			return t == _ReponseReg;
		}

		internal static bool SelectIdentifier()
		{
			return AssetIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_0
	{
		public List<AnimatorTransitionBase> _ComposerReg;

		internal static _003C_003Ec__DisplayClass432_0 SetupIdentifier;

		internal void VisitThread(AnimatorTransitionBase t)
		{
			_003C_003Ec__DisplayClass432_1 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_1
			{
				mappingReg = this,
				m_RepositoryReg = t
			};
			if (_003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.Length != 0)
			{
				_003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.ReadThread);
				RevertMapper().RemoveEntryTransition((AnimatorTransition)_003C_003Ec__DisplayClass432_.m_RepositoryReg);
			}
		}

		internal void DefineThread(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass432_2 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_2
			{
				containerReg = this,
				baseReg = t
			};
			if (_003C_003Ec__DisplayClass432_.baseReg.conditions.Length != 0)
			{
				_003C_003Ec__DisplayClass432_.baseReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.SelectThread);
				RevertMapper().RemoveAnyStateTransition(_003C_003Ec__DisplayClass432_.baseReg);
			}
		}

		internal void StartThread(AnimatorStateTransition t, AnimatorState s)
		{
			_003C_003Ec__DisplayClass432_3 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_3
			{
				instanceReg = this,
				m_ClassReg = t,
				mockReg = s
			};
			if (_003C_003Ec__DisplayClass432_.m_ClassReg.conditions.Length != 0)
			{
				_003C_003Ec__DisplayClass432_.m_ClassReg.conditions.InvokeResolver(_003C_003Ec__DisplayClass432_.RemoveThread);
				_003C_003Ec__DisplayClass432_.mockReg.RemoveTransition(_003C_003Ec__DisplayClass432_.m_ClassReg);
			}
		}

		internal static bool ExcludeIdentifier()
		{
			return SetupIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_1
	{
		public AnimatorTransitionBase m_RepositoryReg;

		public _003C_003Ec__DisplayClass432_0 mappingReg;

		internal static _003C_003Ec__DisplayClass432_1 ListIdentifier;

		internal void ReadThread(AnimatorCondition c)
		{
			AnimatorTransitionBase animatorTransitionBase = RevertMapper().AddEntryTransition(m_RepositoryReg.destinationState);
			EditorUtility.CopySerializedManagedFieldsOnly(m_RepositoryReg, animatorTransitionBase);
			animatorTransitionBase.conditions = new AnimatorCondition[1] { c };
			mappingReg._ComposerReg.Add(animatorTransitionBase);
		}

		internal static bool CalcIdentifier()
		{
			return ListIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_2
	{
		public AnimatorStateTransition baseReg;

		public _003C_003Ec__DisplayClass432_0 containerReg;

		internal static _003C_003Ec__DisplayClass432_2 CompareIdentifier;

		internal void SelectThread(AnimatorCondition c)
		{
			AnimatorTransitionBase dest = RevertMapper().AddAnyStateTransition(baseReg.destinationState);
			while (true)
			{
				EditorUtility.CopySerialized(baseReg, dest);
			}
		}

		internal static bool PublishIdentifier()
		{
			return CompareIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_3
	{
		public AnimatorStateTransition m_ClassReg;

		public AnimatorState mockReg;

		public _003C_003Ec__DisplayClass432_0 instanceReg;

		internal static _003C_003Ec__DisplayClass432_3 InstantiateIdentifier;

		internal void RemoveThread(AnimatorCondition c)
		{
			AnimatorStateTransition animatorStateTransition = CalculateAlgo(m_ClassReg);
			animatorStateTransition.conditions = new AnimatorCondition[1] { c };
			mockReg.AddTransition(animatorStateTransition);
			instanceReg._ComposerReg.Add(animatorStateTransition);
		}

		internal static bool RevertIdentifier()
		{
			return InstantiateIdentifier == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass483_1
	{
		public EditorCurveBinding parserReg;

		internal static _003C_003Ec__DisplayClass483_1 FillDatabase;

		internal bool MapThread(string p)
		{
			return p == parserReg.propertyName;
		}

		internal static bool DeleteDatabase()
		{
			return FillDatabase == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass616_0
	{
		public object m_IteratorReg;

		public string _PublisherReg;

		internal static _003C_003Ec__DisplayClass616_0 ConcatDatabase;

		internal T ReflectThread<T>(string methodName) where T : Delegate
		{
			return (T)Delegate.CreateDelegate(typeof(T), m_IteratorReg, _ReaderVisitor.DisableList(methodName));
		}

		internal bool DeleteThread(EventMapper l)
		{
			return l._InfoMapper.name == _PublisherReg;
		}

		internal static bool CollectDatabase()
		{
			return ConcatDatabase == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass670_1
	{
		public UnityEditor.Animations.BlendTree visitorTests;

		internal static _003C_003Ec__DisplayClass670_1 MapDatabase;

		internal ChildMotion CollectThread(Motion m)
		{
			return new ChildMotion
			{
				motion = m,
				timeScale = 1f,
				threshold = 0f,
				directBlendParameter = visitorTests.blendParameter
			};
		}

		internal static bool AddDatabase()
		{
			return MapDatabase == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass73_0
	{
		public GUIStyle[] m_ObserverTests;

		public Texture2D _ServerTests;

		private static _003C_003Ec__DisplayClass73_0 CompareDatabase;

		internal void AssetThread(string name, Func<GUIStyle, GUIStyle> func, bool isCosmeticOnlyStyle = false, bool hiddenFromList = false)
		{
			for (int i = 0; i < 4; i++)
			{
				bool validatesecond = i > 1;
				bool isrule = i % 2 == 1;
				GUIStyle arg = m_ObserverTests[i];
				GUIStyle gUIStyle = func(arg);
				EnableAnnotation(name, gUIStyle, isrule, validatesecond);
				if (i == 0)
				{
					m_PolicyAnnotation.Add(name, gUIStyle);
				}
				if (isCosmeticOnlyStyle)
				{
					m_SerializerAnnotation.Add(name);
				}
			}
			if (!hiddenFromList)
			{
				_PageAnnotation.Add(name.Substring(3));
			}
		}

		internal GUIStyle UpdateThread(GUIStyle s)
		{
			return new GUIStyle(s)
			{
				alignment = TextAnchor.UpperLeft,
				clipping = TextClipping.Overflow,
				fontStyle = FontStyle.Bold,
				overflow = new RectOffset(),
				contentOffset = default(Vector2),
				padding = new RectOffset(2, 2, 20, 2),
				wordWrap = true,
				fixedHeight = 100f,
				fixedWidth = 100f,
				normal = 
				{
					scaledBackgrounds = new Texture2D[1] { _ServerTests },
					textColor = Color.black
				}
			};
		}

		internal static bool PublishDatabase()
		{
			return CompareDatabase == null;
		}
	}

	private static EditorWindow m_Base;

	private static UnityEditor.Animations.AnimatorController _Container;

	private static AnimatorStateMachine @class;

	private static AnimatorStateMachine _Mock;

	private static Vector2 field;

	private static int _Attribute;

	private static bool _Client;

	private static bool m_Config;

	private static bool _Descriptor;

	private static bool m_Template;

	private static bool m_Message;

	private static bool collection;

	private static bool m_Parser;

	private static bool _Manager;

	private static bool specification = true;

	private static string[] _Method;

	private static string[] _Schema;

	private static string[] broadcaster;

	private static string[] _Proxy;

	private static StateMachineBehaviour[] @struct;

	private static MethodVisitor _Service;

	private static AnimatorStateTransition[] global;

	private static SerializedObject task;

	private static readonly List<SetterTests.TokenizerTests> _Process = new List<SetterTests.TokenizerTests>();

	private static readonly ConcurrentBag<SetterTests.BaseTests> _Producer = new ConcurrentBag<SetterTests.BaseTests>();

	private static List<AnimatorTransitionBase> _Iterator = new List<AnimatorTransitionBase>();

	private static SerializedObject _Publisher;

	private static SerializedObject m_Proc;

	private static AnimatorStateMachine[] m_WrapperAnnotation;

	private static AnimatorStateMachine[] _AnnotationAnnotation;

	private static List<AnimatorState> _VisitorAnnotation = new List<AnimatorState>();

	private static List<AnimatorState> m_AlgoAnnotation = new List<AnimatorState>();

	private static List<SetterTests.TokenizerTests> m_MapperAnnotation;

	private static ConcurrentBag<SetterTests.BaseTests> _InitializerAnnotation;

	private static List<SetterTests.InstanceTests> _DefinitionAnnotation;

	private static AnimatorTransitionBase[] regAnnotation;

	private static List<AnimatorStateTransition> _TestsAnnotation = new List<AnimatorStateTransition>();

	private static List<AnimatorTransitionBase> propertyAnnotation = new List<AnimatorTransitionBase>();

	private static SetterTests.InstanceTests _ProcessorAnnotation;

	private static AnimatorStateTransition _ObserverAnnotation;

	private static List<SetterTests.InstanceTests> serverAnnotation = new List<SetterTests.InstanceTests>();

	private static List<AnimatorCondition> m_ThreadAnnotation = new List<AnimatorCondition>();

	private static readonly Dictionary<string, GUIStyle> m_PolicyAnnotation = new Dictionary<string, GUIStyle>();

	private static readonly HashSet<string> m_SerializerAnnotation = new HashSet<string>();

	private static readonly List<string> _PageAnnotation = new List<string>();

	private static Dictionary<string, GUIStyle> m_ResolverAnnotation;

	private static GUIStyle predicateAnnotation;

	private static TestsAlgo rulesAnnotation;

	private static List<AlgoAlgo> _QueueAnnotation = new List<AlgoAlgo>();

	private static ReorderableList _ErrorAnnotation;

	private static List<WorkerProperty.BridgeProperty> setterAnnotation = new List<WorkerProperty.BridgeProperty>();

	private static bool m_ConnectionAnnotation;

	private static FieldInfo contextAnnotation;

	private static FieldInfo recordAnnotation;

	private static PropertyInfo helperAnnotation;

	private static PropertyInfo m_ConsumerAnnotation;

	private static MethodInfo _AdapterAnnotation;

	private static MethodInfo interpreterAnnotation;

	private static bool m_WatcherAnnotation;

	private static bool candidateAnnotation;

	private static string _ProductAnnotation;

	private static bool m_ExpressionAnnotation;

	private static bool systemAnnotation;

	private static string m_WorkerAnnotation;

	private static string m_FilterAnnotation;

	private static string stubAnnotation;

	private static string m_ReaderAnnotation;

	private static string m_BridgeAnnotation = "";

	private static string strategyAnnotation = "";

	private static string m_CustomerAnnotation = "";

	private static string databaseAnnotation;

	private static bool m_ExporterAnnotation;

	private static bool m_IdentifierAnnotation;

	private static bool attrAnnotation;

	private static bool m_DispatcherAnnotation;

	private static float m_RegistryAnnotation;

	private static string m_TagAnnotation;

	private static bool importerAnnotation;

	private static bool _RequestAnnotation;

	private static string printerAnnotation;

	private static string _WriterAnnotation;

	private static string m_ParamsAnnotation;

	private static bool listenerAnnotation;

	private static bool m_GetterAnnotation;

	private static bool m_InterceptorAnnotation;

	private static Action creatorAnnotation;

	private static Action m_EventAnnotation;

	private static readonly Type[] infoAnnotation = new Type[2]
	{
		typeof(ControllerEditor),
		typeof(ControllerEditorWindow)
	};

	private static bool facadeAnnotation;

	private static bool advisorAnnotation;

	private static bool m_CallbackAnnotation;

	private static bool _IndexerAnnotation;

	private static bool _IssuerAnnotation;

	private static bool _PrototypeAnnotation;

	private static bool _RuleAnnotation;

	private static bool singletonAnnotation;

	private static readonly AnimBool factoryAnnotation = new AnimBool();

	private static readonly AnimBool _AccountAnnotation = new AnimBool();

	private static readonly SingletonServer m_RefAnnotation = new SingletonServer("3.3.2");

	private static readonly (string, string)[] statusAnnotation = new(string, string)[1] { ("Templates", "https://notes.sleightly.dev/templates/") };

	private static List<GlobalVisitor> _TokenAnnotation = new List<GlobalVisitor>();

	private static List<GlobalVisitor> m_CodeAnnotation = new List<GlobalVisitor>();

	private static List<GlobalVisitor> m_DicAnnotation = new List<GlobalVisitor>();

	private static ReorderableList invocationAnnotation;

	private static ReorderableList _RoleAnnotation;

	private static ReorderableList m_ParamAnnotation;

	private static int _ModelAnnotation = -1;

	private static SerializedProperty m_TokenizerAnnotation;

	private static SerializedProperty decoratorAnnotation;

	private static SerializedProperty m_ComparatorAnnotation;

	private static SerializedProperty m_ExceptionAnnotation;

	private static SerializedProperty objectAnnotation;

	private static SerializedProperty _UtilsAnnotation;

	private static SerializedProperty valAnnotation;

	private static SerializedProperty m_ValueAnnotation;

	private static SerializedProperty _MerchantAnnotation;

	private static SerializedProperty m_AuthenticationAnnotation;

	private static SerializedProperty m_ReponseAnnotation;

	private static SerializedProperty _PoolAnnotation;

	private static SerializedProperty m_ParameterAnnotation;

	private static SerializedProperty _ComposerAnnotation;

	private static SerializedProperty _RepositoryAnnotation;

	private static SerializedProperty m_MappingAnnotation;

	private static bool baseAnnotation;

	private static bool containerAnnotation;

	private static bool m_ClassAnnotation;

	private static SerializedProperty _MockAnnotation;

	private static SerializedProperty instanceAnnotation;

	private static SerializedProperty _FieldAnnotation;

	private static SerializedProperty attributeAnnotation;

	private static SerializedProperty m_ClientAnnotation;

	private static SerializedProperty configAnnotation;

	private static SerializedProperty m_DescriptorAnnotation;

	private static SerializedProperty _TemplateAnnotation;

	private static SerializedProperty _MessageAnnotation;

	private static SerializedProperty m_CollectionAnnotation;

	private static bool parserAnnotation;

	private static Type[] m_ManagerAnnotation;

	private static readonly Dictionary<Type, string[]> m_ItemAnnotation = new Dictionary<Type, string[]>();

	private static readonly Dictionary<Shader, string[]> _SpecificationAnnotation = new Dictionary<Shader, string[]>();

	private static UnityEditor.Animations.AnimatorController m_MethodAnnotation;

	private static string _SchemaAnnotation;

	private static string m_BroadcasterAnnotation;

	private static string proxyAnnotation;

	private static bool structAnnotation = true;

	private static bool serviceAnnotation = true;

	private static string _StateAnnotation;

	private static ControllerAction m_GlobalAnnotation = ControllerAction.Copy;

	private static ActionMode _TaskAnnotation = ActionMode.CurrentController;

	private static MoveMode m_ProcessAnnotation = MoveMode.CurrentLayer;

	private MoveDestination producerAnnotation;

	private static EditorWindow _IteratorAnnotation;

	private static GameObject m_PublisherAnnotation;

	private static Animator m_ConfigurationAnnotation;

	private static object procAnnotation;

	private static bool wrapperVisitor;

	private static UnityEditor.Animations.AnimatorController annotationVisitor;

	private static bool m_VisitorVisitor;

	private static GameObject _AlgoVisitor;

	private static Type mapperVisitor;

	private static Type _InitializerVisitor;

	private static Type definitionVisitor;

	private static Type _RegVisitor;

	private static Type _TestsVisitor;

	private static Type _PropertyVisitor;

	private static PropertyInfo m_ProcessorVisitor;

	private static PropertyInfo _ObserverVisitor;

	private static PropertyInfo serverVisitor;

	private static PropertyInfo m_ThreadVisitor;

	private static PropertyInfo m_PolicyVisitor;

	private static FieldInfo m_SerializerVisitor;

	private static bool pageVisitor;

	private static List<object> resolverVisitor;

	private static Rect m_PredicateVisitor;

	private static Type rulesVisitor;

	private static Type m_QueueVisitor;

	private static Type errorVisitor;

	private static FieldInfo m_SetterVisitor;

	private static Texture2D connectionVisitor;

	private static readonly HashSet<Vector3> m_ContextVisitor = new HashSet<Vector3>();

	private static bool m_RecordVisitor;

	private static bool m_HelperVisitor;

	private static Type _ConsumerVisitor;

	private static Type m_AdapterVisitor;

	private static Type m_InterpreterVisitor;

	private static Type _WatcherVisitor;

	private static Type m_CandidateVisitor;

	private static Type m_ProductVisitor;

	private static bool _ExpressionVisitor;

	private static bool _SystemVisitor;

	private static MethodInfo workerVisitor;

	private static bool filterVisitor;

	private static object m_StubVisitor;

	private static Type _ReaderVisitor;

	private static Type _BridgeVisitor;

	private static FieldInfo m_StrategyVisitor;

	private static MethodInfo _CustomerVisitor;

	private static MethodInfo m_DatabaseVisitor;

	private static MethodInfo _ExporterVisitor;

	private static FieldInfo m_IdentifierVisitor;

	private static FieldInfo m_AttrVisitor;

	private static FieldInfo m_DispatcherVisitor;

	private static MethodInfo registryVisitor;

	private static bool _TagVisitor = true;

	private static Vector2 importerVisitor;

	private static UnityEditor.Animations.AnimatorController[] requestVisitor;

	private static string[] m_PrinterVisitor;

	private static LayerViewViewType writerVisitor = LayerViewViewType.DefaultView;

	private static ImporterMapper m_ParamsVisitor;

	private static ImporterMapper listenerVisitor;

	private static ReorderableList m_GetterVisitor;

	private static ReorderableList m_InterceptorVisitor;

	private static string[] creatorVisitor;

	private static bool eventVisitor;

	private static bool _InfoVisitor;

	private static ReorderableList.ElementCallbackDelegate _FacadeVisitor;

	private static ReorderableList.SelectCallbackDelegate _AdvisorVisitor;

	private static ReorderableList.SelectCallbackDelegate _CallbackVisitor;

	private static AnnotationProperty indexerVisitor;

	private static AnnotationProperty _IssuerVisitor;

	private static MethodInfo m_PrototypeVisitor;

	private static ConstructorInfo m_RuleVisitor;

	private static GenericMenu m_SingletonVisitor;

	private static Type _FactoryVisitor;

	private static Type _AccountVisitor;

	private static Type _RefVisitor;

	private static Type _StatusVisitor;

	private static FieldInfo tokenVisitor;

	private static FieldInfo codeVisitor;

	private static FieldInfo m_DicVisitor;

	private static PropertyInfo _InvocationVisitor;

	private static MethodInfo roleVisitor;

	private static MethodInfo paramVisitor;

	private static MethodInfo modelVisitor;

	private static MethodInfo tokenizerVisitor;

	private static MethodInfo m_DecoratorVisitor;

	private static readonly MethodInfo m_ComparatorVisitor = SpecificationAlgo.NewReg<AnimatorState>(QueryAlgo);

	private static Node exceptionVisitor;

	private static bool m_ObjectVisitor;

	private static bool m_UtilsVisitor;

	private static bool _ValVisitor;

	private static bool valueVisitor;

	private static AnimatorStateTransition merchantVisitor;

	private static AnimatorState m_AuthenticationVisitor;

	private static Vector2 _ReponseVisitor;

	private static bool poolVisitor;

	private static AnimatorState parameterVisitor;

	private static UnityEngine.AnimatorControllerParameter[] m_ComposerVisitor;

	private static FieldInfo _RepositoryVisitor;

	private static Type mappingVisitor;

	private static int m_BaseVisitor;

	private static Vector2 m_ContainerVisitor;

	private static Node classVisitor;

	private static int _MockVisitor;

	private static bool _InstanceVisitor;

	private static bool _FieldVisitor;

	private static bool _AttributeVisitor;

	private static bool clientVisitor;

	private static int m_ConfigVisitor;

	private static bool m_DescriptorVisitor;

	private static AnimatorState templateVisitor;

	private static UnityEditor.Animations.AnimatorController m_MessageVisitor;

	private static UnityEditor.Animations.AnimatorControllerLayer collectionVisitor;

	private static MethodInfo _ParserVisitor;

	private static MethodInfo _ManagerVisitor;

	private static MethodInfo m_ItemVisitor;

	private static MethodInfo m_SpecificationVisitor;

	internal static ControllerEditor DefineIndexer;

	private static void PatchWrapper()
	{
		if (m_Base != null)
		{
			m_Base.Repaint();
		}
	}

	[SpecialName]
	internal static UnityEditor.Animations.AnimatorController LogoutMapper()
	{
		if (!_Container)
		{
			InstantiateAnnotation();
		}
		return _Container;
	}

	[SpecialName]
	internal static void PatchMapper(UnityEditor.Animations.AnimatorController v)
	{
		if (_Container != v)
		{
			while (true)
			{
				_Container = v;
				DisableMapper();
			}
		}
	}

	[SpecialName]
	private static AnimatorStateMachine ManageMapper()
	{
		if (!@class)
		{
			DefineAnnotation();
		}
		return @class;
	}

	[SpecialName]
	private static void PrintMapper(AnimatorStateMachine var1)
	{
		if (@class != var1)
		{
			@class = var1;
			FlushAnnotation();
		}
	}

	[SpecialName]
	private static AnimatorStateMachine RevertMapper()
	{
		if (!_Mock)
		{
			RemoveAnnotation();
		}
		return _Mock;
	}

	[SpecialName]
	private static void OrderInitializer(AnimatorStateMachine instance)
	{
		if (_Mock != instance)
		{
			_Mock = instance;
			RestartVisitor();
		}
	}

	[SpecialName]
	private static bool SetInitializer()
	{
		return _ProcessorAnnotation.itemTests != null;
	}

	[MenuItem("DreadTools/Controller Editor/Window %t", false, 200)]
	internal static void InterruptWrapper()
	{
		EditorWindow.GetWindow<ControllerEditor>(utility: false, " Controller Editor", focus: true).titleContent.image = EditorGUIUtility.IconContent("d_EditCollider").image;
	}

	private void OnGUI()
	{
		if (!OrderVisitor(this))
		{
			return;
		}
		Event current = Event.current;
		if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && current.type == EventType.KeyDown)
		{
			GUI.FocusControl(null);
			Repaint();
			return;
		}
		_Attribute = 0;
		using (new SpecificationThread(ref field))
		{
			using (new GUILayout.HorizontalScope())
			{
				ConsumerAlgo.CustomerAlgo[] array = new ConsumerAlgo.CustomerAlgo[3]
				{
					ConsumerAlgo.CallDefinition().editingTransitions,
					ConsumerAlgo.CallDefinition().editingStates,
					ConsumerAlgo.CallDefinition().editingController
				};
				string[] array2 = new string[3] { "Transitions", "States", "Controller" };
				for (int i = 0; i < 3; i++)
				{
					EditorGUI.BeginChangeCheck();
					array[i].ExcludeDefinition(ClassProperty.AddQueue(array[i], array2[i], EditorStyles.toolbarButton));
					if (EditorGUI.EndChangeCheck())
					{
						switch (i)
						{
						case 0:
							parserAnnotation = ConsumerAlgo.CallDefinition().editingTransitions;
							break;
						case 1:
							_Descriptor = ConsumerAlgo.CallDefinition().editingStates;
							break;
						}
					}
				}
			}
			EditorGUI.BeginDisabledGroup(!LogoutMapper());
			using (new GUILayout.VerticalScope(ClassProperty.CalcError()._WrapperObserver))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(18f);
					if (!(RevertMapper() != null))
					{
						GUILayout.Label("No Active Machine", ClassProperty.CalcError()._ClassProcessor, GUILayout.ExpandWidth(expand: true));
					}
					else
					{
						GUILayout.Label(RevertMapper().name, ClassProperty.CalcError()._ClassProcessor, GUILayout.ExpandWidth(expand: true));
					}
					EditorGUI.EndDisabledGroup();
					if (ClassProperty.RestartQueue(ClassProperty.DestroyError()._DicProcessor, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)) && EditorUtility.DisplayDialog("Instructions", "Open Controller Editor's Online Manual?", "Open", "Cancel"))
					{
						Application.OpenURL("https://notes.sleightly.dev/ceditor");
					}
					if (ClassProperty.RestartQueue(ClassProperty.DestroyError()._RequestProcessor, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						ControllerEditorWindow.CalcTests();
					}
					EditorGUI.BeginDisabledGroup(!LogoutMapper());
				}
				if ((bool)ManageMapper() && _Method.Length != 0)
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();
						for (int j = 0; j < _Method.Length; j++)
						{
							GUILayout.Label(_Method[j], "AssetLabel");
						}
						GUILayout.FlexibleSpace();
					}
				}
			}
			EditorGUI.EndDisabledGroup();
			ClassProperty.MapQueue();
			ReflectVisitor();
			IncludeAnnotation(parserAnnotation && (_Descriptor || ConsumerAlgo.CallDefinition().editingController.FindDefinition()));
			DestroyVisitor();
			IncludeAnnotation(_Descriptor && ConsumerAlgo.CallDefinition().editingController.FindDefinition());
			ValidateVisitor();
			DefineVisitor();
			SpecificationAlgo.LoginReg();
		}
	}

	private static void ManageWrapper()
	{
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			return;
		}
		if ((bool)ConsumerAlgo.CallDefinition().aw_active && (bool)ConsumerAlgo.CallDefinition().aw_autoSwitchClip && Selection.activeObject is AnimatorState { motion: AnimationClip motion })
		{
			StartInitializer(motion);
		}
		_IssuerVisitor.MapSerializer(isconfig: true);
		IEnumerable<SetterTests.TokenizerTests> enumerable = SetterTests.RefTests.CalcPolicy();
		object obj;
		if (enumerable != null)
		{
			obj = enumerable.ToList();
			if (obj != null)
			{
				goto IL_0073;
			}
		}
		else
		{
			obj = null;
		}
		obj = _Process;
		goto IL_0073;
		IL_0073:
		m_MapperAnnotation = (List<SetterTests.TokenizerTests>)obj;
		_InitializerAnnotation = SetterTests.RefTests.RunPolicy() ?? _Producer;
		_DefinitionAnnotation = new List<SetterTests.InstanceTests>();
		foreach (SetterTests.BaseTests item in _InitializerAnnotation)
		{
			_DefinitionAnnotation.AddRange(item.FindSerializer());
		}
		m_Parser = false;
		_Manager = false;
		bool flag = !m_ClassAnnotation && m_MapperAnnotation.Any((SetterTests.TokenizerTests nw) => nw.FillPolicy() == SetterTests.RefTests.FlushPolicy().FillPolicy());
		bool flag2 = !m_ClassAnnotation && m_MapperAnnotation.Any((SetterTests.TokenizerTests nw) => nw.FillPolicy() == SetterTests.RefTests.MapPolicy().FillPolicy());
		m_Message |= flag;
		m_Template |= flag2;
		collection = m_MapperAnnotation.Any((SetterTests.TokenizerTests nw) => nw.FillPolicy() == SetterTests.RefTests.CalculatePolicy().FillPolicy());
		bool flag3 = false;
		foreach (UnityEngine.Object item2 in Selection.objects.CloneRules())
		{
			if (SetInitializer())
			{
				flag3 |= item2 == _ProcessorAnnotation.itemTests;
			}
			Type type = item2.GetType();
			if (m_Parser || !(type == typeof(AnimatorStateTransition)))
			{
				if (!_Manager && type == typeof(AnimatorTransition))
				{
					_Manager = true;
				}
			}
			else
			{
				m_Parser = true;
			}
			if (m_Parser && _Manager && (!SetInitializer() || flag3))
			{
				break;
			}
		}
		if (!m_ClassAnnotation)
		{
			if (!flag2)
			{
				m_Template = false;
			}
			if (!flag)
			{
				m_Message = false;
			}
		}
		if (!flag3)
		{
			_ProcessorAnnotation = default(SetterTests.InstanceTests);
		}
		if (SetterTests.RefTests.CalculatePolicy() != null && SetterTests.RefTests.CalculatePolicy().FillPolicy() != null)
		{
			regAnnotation = SetterTests.RefTests.CalculatePolicy().ChangePolicy().ToArray();
		}
		AnimatorTransitionBase[] filtered = Selection.GetFiltered<AnimatorTransitionBase>(SelectionMode.Editable);
		propertyAnnotation = _DefinitionAnnotation.Select((SetterTests.InstanceTests t) => t.itemTests).ToList();
		_TokenAnnotation = AssetVisitor(propertyAnnotation);
		AnimatorState[] filtered2 = Selection.GetFiltered<AnimatorState>(SelectionMode.Editable);
		m_AlgoAnnotation = filtered2.ToList();
		object publisher;
		if (m_AlgoAnnotation.Count > 0)
		{
			UnityEngine.Object[] objs = filtered2;
			publisher = new SerializedObject(objs);
		}
		else
		{
			publisher = null;
		}
		_Publisher = (SerializedObject)publisher;
		m_WrapperAnnotation = Selection.GetFiltered<AnimatorStateMachine>(SelectionMode.Editable);
		if (_DefinitionAnnotation.Count == 0)
		{
			m_CodeAnnotation.Clear();
			_Iterator.Clear();
		}
		else
		{
			List<GlobalVisitor> list = new List<GlobalVisitor>();
			AnimatorTransitionBase[] array = _Iterator.Where((AnimatorTransitionBase t) => !propertyAnnotation.Contains(t)).ToArray();
			AnimatorTransitionBase[] array2 = array;
			foreach (AnimatorTransitionBase processReg in array2)
			{
				list.AddRange(m_CodeAnnotation.Where((GlobalVisitor c) => c._ProducerVisitor[0].Item1 == processReg));
			}
			_Iterator = _Iterator.Except(array).ToList();
			m_CodeAnnotation = m_CodeAnnotation.Except(list).ToList();
			foreach (AnimatorTransitionBase item3 in propertyAnnotation.Where((AnimatorTransitionBase t) => !_Iterator.Contains(t)))
			{
				_Iterator.Add(item3);
				for (int num2 = 0; num2 < item3.conditions.Length; num2++)
				{
					m_CodeAnnotation.Add(new GlobalVisitor(item3, num2));
				}
			}
		}
		MapVisitor();
		_TestsAnnotation = Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.Editable).ToList();
		RunAlgo();
		CalculateAnnotation();
		if (!containerAnnotation && !baseAnnotation)
		{
			serverAnnotation = _DefinitionAnnotation.ToList();
		}
		if (!m_ClassAnnotation)
		{
			_VisitorAnnotation = m_AlgoAnnotation;
			_AnnotationAnnotation = m_WrapperAnnotation;
		}
		ConnectAnnotation();
		if (WorkerProperty.InvokePage())
		{
			CallAnnotation();
			MoveAnnotation();
		}
		parserAnnotation = (bool)ConsumerAlgo.CallDefinition().editingTransitions || filtered.Length != 0;
		_Descriptor = (bool)ConsumerAlgo.CallDefinition().editingStates || m_AlgoAnnotation.Count > 0;
		PatchWrapper();
	}

	private void OnFocus()
	{
		ManageWrapper();
	}

	private void PrintWrapper()
	{
		UpdateVisitor();
		Repaint();
	}

	private void OnDisable()
	{
		m_ClassAnnotation = false;
		containerAnnotation = false;
		baseAnnotation = false;
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
	}

	private void SearchWrapper()
	{
		m_ClassAnnotation = false;
		containerAnnotation = false;
		baseAnnotation = false;
	}

	private void OnEnable()
	{
		m_Base = this;
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
		EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(SortAlgo));
		EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(SortAlgo));
		if (global == null)
		{
			global = new AnimatorStateTransition[2]
			{
				new AnimatorStateTransition
				{
					name = "a",
					canTransitionToSelf = false,
					duration = 0f,
					exitTime = 0f,
					hasExitTime = false,
					hasFixedDuration = false,
					interruptionSource = TransitionInterruptionSource.None,
					mute = false,
					offset = 0f,
					orderedInterruption = false,
					solo = false
				},
				new AnimatorStateTransition
				{
					name = "b",
					canTransitionToSelf = true,
					duration = 1f,
					exitTime = 1f,
					hasExitTime = true,
					hasFixedDuration = true,
					interruptionSource = TransitionInterruptionSource.Destination,
					mute = true,
					offset = 1f,
					orderedInterruption = true,
					solo = true
				}
			};
			UnityEngine.Object[] objs = global;
			task = new SerializedObject(objs);
		}
		m_Proc = task;
		CalculateAnnotation();
		MapVisitor();
		CancelAnnotation();
	}

	private static void RevertWrapper()
	{
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(ManageWrapper));
			Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(ManageWrapper));
			OrderAnnotation();
			CompareAnnotation();
			ClassProperty.DisableRules(SetAnnotation);
			ManageWrapper();
			PublishAnnotation();
			_SystemVisitor = true;
		}
	}

	private static void OrderAnnotation()
	{
		try
		{
			RegisterAlgo();
			ResetMapper();
			ManageAlgo();
			if (((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
				return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
			})())
			{
				ValidateMapper();
				FillAlgo();
				InstantiateMapper();
				PrepareAlgo();
			}
		}
		catch (System.Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			throw;
		}
	}

	private static void CompareAnnotation()
	{
		ForgotAlgo();
		CustomizeMapper();
		PrintAlgo();
		FlushMapper();
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			RemoveMapper();
			AssetAlgo();
			LogoutAlgo();
			AwakeMapper();
		}
	}

	private static void SetAnnotation()
	{
		_003C_003Ec__DisplayClass73_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass73_0();
		if (m_ResolverAnnotation == null)
		{
			m_ResolverAnnotation = (Dictionary<string, GUIStyle>)ClassProperty.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").RestartList("m_NodeStyleCache").GetValue(null);
		}
		m_ResolverAnnotation.Clear();
		m_PolicyAnnotation.Clear();
		m_SerializerAnnotation.Clear();
		CS_0024_003C_003E8__locals15.m_ObserverTests = new GUIStyle[4] { "flow node 0", "flow node 0 on", "flow node 5", "flow node 5 on" };
		predicateAnnotation = new GUIStyle(CS_0024_003C_003E8__locals15.m_ObserverTests[0])
		{
			fixedWidth = 200f,
			fixedHeight = 40f
		};
		CS_0024_003C_003E8__locals15._ServerTests = new Texture2D(1, 64, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Bilinear
		};
		for (int i = 0; i < 64; i++)
		{
			Color color = ((i <= 40) ? new Color(0.973f, 0.757f, 0.267f) : new Color(0.961f, 0.655f, 0.176f));
			CS_0024_003C_003E8__locals15._ServerTests.SetPixel(0, i, color);
			CS_0024_003C_003E8__locals15._ServerTests.Apply();
		}
		CS_0024_003C_003E8__locals15.AssetThread("ce_Note Sticky", (GUIStyle s) => new GUIStyle(s)
		{
			alignment = TextAnchor.UpperLeft,
			clipping = TextClipping.Overflow,
			fontStyle = FontStyle.Bold,
			overflow = new RectOffset(),
			contentOffset = default(Vector2),
			padding = new RectOffset(2, 2, 20, 2),
			wordWrap = true,
			fixedHeight = 100f,
			fixedWidth = 100f,
			normal = 
			{
				scaledBackgrounds = new Texture2D[1] { CS_0024_003C_003E8__locals15._ServerTests },
				textColor = Color.black
			}
		}, isCosmeticOnlyStyle: true);
		CS_0024_003C_003E8__locals15.AssetThread("ce_Note", (GUIStyle s) => new GUIStyle(s)
		{
			alignment = TextAnchor.UpperLeft,
			clipping = TextClipping.Overflow,
			fontStyle = FontStyle.Bold,
			overflow = new RectOffset(),
			contentOffset = default(Vector2),
			padding = new RectOffset(2, 2, 2, 2),
			wordWrap = true,
			fixedHeight = 100f,
			fixedWidth = 100f,
			normal = 
			{
				scaledBackgrounds = new Texture2D[1] { ClassProperty.LoginList(Color.black) }
			}
		}, isCosmeticOnlyStyle: true);
		CS_0024_003C_003E8__locals15.AssetThread("ce_Note Big", (GUIStyle s) => new GUIStyle(s)
		{
			alignment = TextAnchor.UpperLeft,
			clipping = TextClipping.Overflow,
			fontStyle = FontStyle.Bold,
			overflow = new RectOffset(),
			contentOffset = default(Vector2),
			padding = new RectOffset(2, 2, 2, 2),
			wordWrap = true,
			fixedHeight = 200f,
			fixedWidth = 200f,
			normal = 
			{
				scaledBackgrounds = new Texture2D[1] { ClassProperty.LoginList(Color.black) }
			}
		}, isCosmeticOnlyStyle: true);
		CS_0024_003C_003E8__locals15.AssetThread("ce_Mini", (GUIStyle s) => new GUIStyle(s)
		{
			fixedHeight = 40f,
			fixedWidth = 100f
		});
		CS_0024_003C_003E8__locals15.AssetThread("ce_Square", (GUIStyle s) => new GUIStyle(s)
		{
			fixedHeight = 40f,
			fixedWidth = 40f
		}, isCosmeticOnlyStyle: true);
		CS_0024_003C_003E8__locals15.AssetThread("ce_Square Big", (GUIStyle s) => new GUIStyle(s)
		{
			fixedHeight = 80f,
			fixedWidth = 80f,
			alignment = TextAnchor.MiddleCenter
		});
		CS_0024_003C_003E8__locals15.AssetThread("ce_Big", (GUIStyle s) => new GUIStyle(s)
		{
			fixedHeight = 80f,
			fixedWidth = 400f,
			fontSize = 20
		});
		CS_0024_003C_003E8__locals15.AssetThread("ce_Tiny", (GUIStyle s) => new GUIStyle(s)
		{
			fixedWidth = 20f,
			fixedHeight = 20f,
			clipping = TextClipping.Clip
		}, isCosmeticOnlyStyle: true, hiddenFromList: true);
		CS_0024_003C_003E8__locals15.AssetThread("ce_Dot", (GUIStyle s) => new GUIStyle(s)
		{
			fixedWidth = 10f,
			fixedHeight = 10f,
			clipping = TextClipping.Clip
		}, isCosmeticOnlyStyle: true, hiddenFromList: true);
		_PageAnnotation.Sort();
		_SystemVisitor = true;
	}

	private static void PostAnnotation(string param, GUIStyle cont)
	{
		SetupAnnotation(param, cont, wantthird: false);
		SetupAnnotation(param, cont, wantthird: true);
	}

	private static void SetupAnnotation(string value, GUIStyle b, bool wantthird)
	{
		EnableAnnotation(value, b, wantthird, validatesecond2: false);
		EnableAnnotation(value, b, wantthird, validatesecond2: true);
	}

	private static void EnableAnnotation(string spec, GUIStyle vis, bool isrule, bool validatesecond2)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			bool flag = i > 1;
			bool flag2 = i % 2 == 1;
			if (!(flag ^ validatesecond2) && !(flag2 ^ isrule))
			{
				int num = (flag ? 5 : 0);
				stringBuilder.Clear();
				stringBuilder.Append($"flow {spec} {num}");
				if (flag2)
				{
					stringBuilder.Append(" on");
				}
				m_ResolverAnnotation[stringBuilder.ToString()] = vis;
			}
		}
	}

	private static void PublishAnnotation()
	{
		if (m_InterceptorVisitor != null)
		{
			m_InterceptorVisitor.elementHeight = CloneInitializer();
		}
	}

	public void AddItemsToMenu(GenericMenu i)
	{
		i.AddItem(new GUIContent("Instructions"), on: false, delegate
		{
			Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
		});
		i.AddSeparator(string.Empty);
		i.AddItem(new GUIContent("Legacy Dropdown"), ConsumerAlgo.CallDefinition().useLegacyDropdown, delegate
		{
			ConsumerAlgo.CallDefinition().useLegacyDropdown.InsertDefinition();
		});
		i.AddSeparator(string.Empty);
		i.AddItem(new GUIContent("Settings"), on: false, ControllerEditorWindow.CalcTests);
		i.AddSeparator(string.Empty);
		i.AddItem(new GUIContent("Expand Tabs"), on: false, delegate
		{
			GetAnnotation(updatereference: true);
		});
		i.AddItem(new GUIContent("Collapse Tabs"), on: false, delegate
		{
			GetAnnotation(updatereference: false);
		});
	}

	private static List<AlgoAlgo> PopAnnotation(WorkerProperty.BridgeProperty init, List<AlgoAlgo> cust = null)
	{
		List<AlgoAlgo> list = cust;
		if (list != null)
		{
			for (int i = 0; i < init.m_DatabaseProperty.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (!list[j]._MapperAlgo && ComputeAnnotation(init.m_DatabaseProperty[i], list[j].m_InitializerAlgo))
					{
						list[j].RegisterInitializer(init, i);
						break;
					}
				}
			}
			list = list.Where((AlgoAlgo s) => s._MapperAlgo).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				list[num]._MapperAlgo = false;
			}
		}
		else
		{
			list = new List<AlgoAlgo>();
			for (int num2 = 0; num2 < init.m_DatabaseProperty.Count; num2++)
			{
				list.Add(new AlgoAlgo(init, num2));
			}
		}
		return list;
	}

	private static bool ComputeAnnotation(WorkerProperty.BridgeProperty.AttrProperty i, WorkerProperty.BridgeProperty.AttrProperty pol)
	{
		if (!LogoutMapper())
		{
			return false;
		}
		if (!(i.ResetPage() != pol.ResetPage()))
		{
			if (i.CreatePage() != pol.CreatePage())
			{
				return false;
			}
			int second;
			UnityEngine.AnimatorControllerParameterType type = ResetAnnotation(i.ResetPage(), out second).type;
			if (i.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set || i.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Add)
			{
				if (type == UnityEngine.AnimatorControllerParameterType.Trigger)
				{
					return true;
				}
				if (i.ValidatePage() != pol.ValidatePage())
				{
					return false;
				}
			}
			else if (type == UnityEngine.AnimatorControllerParameterType.Bool || type == UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				if (i.DestroyPage() != pol.DestroyPage())
				{
					return false;
				}
			}
			else if (i.IncludePage() != pol.IncludePage() || i.LoginPage() != pol.LoginPage())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private static void MoveAnnotation()
	{
		m_ConnectionAnnotation = true;
		List<StateMachineBehaviour> list = new List<StateMachineBehaviour>();
		foreach (AnimatorState item in m_AlgoAnnotation)
		{
			bool flag = false;
			StateMachineBehaviour[] behaviours = item.behaviours;
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (stateMachineBehaviour.GetType() == WorkerProperty.QueryPage())
				{
					flag = true;
					list.Add(stateMachineBehaviour);
				}
			}
			if (!flag)
			{
				m_ConnectionAnnotation = false;
				break;
			}
		}
		m_ConnectionAnnotation = m_ConnectionAnnotation && list.Count > 0;
		if (m_ConnectionAnnotation)
		{
			rulesAnnotation = new TestsAlgo(list.ToArray());
		}
	}

	private static void ConcatAnnotation()
	{
		foreach (AnimatorState item in m_AlgoAnnotation)
		{
			if (item.behaviours.All((StateMachineBehaviour b) => b.GetType() != WorkerProperty.QueryPage()))
			{
				item.AddStateMachineBehaviour(WorkerProperty.QueryPage());
			}
		}
		MoveAnnotation();
	}

	private static void CallAnnotation()
	{
		_QueueAnnotation = null;
		setterAnnotation.Clear();
		for (int i = 0; i < m_AlgoAnnotation.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < m_AlgoAnnotation[i].behaviours.Length; j++)
			{
				if (m_AlgoAnnotation[i].behaviours[j].GetType() == WorkerProperty.InsertPage())
				{
					WorkerProperty.BridgeProperty bridgeProperty = new WorkerProperty.BridgeProperty(m_AlgoAnnotation[i].behaviours[j]);
					setterAnnotation.Add(bridgeProperty);
					_QueueAnnotation = PopAnnotation(bridgeProperty, _QueueAnnotation);
					flag = true;
				}
			}
			if (!flag)
			{
				if (_QueueAnnotation != null)
				{
					_QueueAnnotation.Clear();
				}
				break;
			}
		}
		if (_QueueAnnotation == null)
		{
			_QueueAnnotation = new List<AlgoAlgo>();
		}
		CancelAnnotation();
	}

	private static void CancelAnnotation()
	{
		_ErrorAnnotation = new ReorderableList(_QueueAnnotation, typeof(AlgoAlgo), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
		{
			drawElementCallback = CountAnnotation,
			drawHeaderCallback = DisableAnnotation,
			onAddCallback = InsertAnnotation
		};
	}

	private static void CountAnnotation(Rect ident, int cust_end, bool rejectserv, bool bool_0)
	{
		if (!LogoutMapper() || cust_end >= _QueueAnnotation.Count || cust_end < 0)
		{
			return;
		}
		WorkerProperty.BridgeProperty.AttrProperty initializerAlgo = _QueueAnnotation[cust_end].m_InitializerAlgo;
		int second;
		UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(initializerAlgo.ResetPage(), out second);
		Rect source = new Rect(ident);
		Rect rect = new Rect(ident.width - 29f, ident.y + 2f, 32f, 18f);
		source.width -= 42f;
		source.y += 2f;
		source.width /= 2f;
		EditorGUI.BeginChangeCheck();
		bool flag = false;
		if (animatorControllerParameter != null)
		{
			initializerAlgo.FlushPage(TestAnnotation(EditorGUI.Popup(source, second, _Schema)));
			source.x += source.width;
			if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger || initializerAlgo.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set || (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Float && animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int) || initializerAlgo.CreatePage() != WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Random)
			{
				if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger || initializerAlgo.CreatePage() != WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set)
				{
					source.width /= 2f;
				}
			}
			else
			{
				source.width /= 3f;
			}
			initializerAlgo.NewPage((WorkerProperty.BridgeProperty.AttrProperty.ChangeType)(object)EditorGUI.EnumPopup(selected: (Enum)((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool && animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger) ? ((object)(VRCFullOptions)initializerAlgo.CreatePage()) : ((object)(VRCHalfOptions)initializerAlgo.CreatePage())), position: source));
			source.x += source.width;
			source.height -= 5f;
			if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Bool || animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				if (initializerAlgo.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Add)
				{
					initializerAlgo.NewPage(WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set);
				}
				if (initializerAlgo.CreatePage() != WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set)
				{
					EditorGUIUtility.labelWidth = 50f;
					source.width -= 17f;
					initializerAlgo.GetPage(EditorGUI.Slider(source, "Chance", initializerAlgo.DestroyPage() * 100f, 0f, 100f) / 100f);
					source.x += source.width;
					GUI.Label(source, "%", "boldlabel");
					EditorGUIUtility.labelWidth = 0f;
				}
				else
				{
					initializerAlgo.CustomizePage(Mathf.Clamp((int)initializerAlgo.ValidatePage(), 0, 1));
					Enum selected = ((initializerAlgo.ValidatePage() == 1f) ? BoolModes.True : BoolModes.False);
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
					{
						initializerAlgo.CustomizePage(((BoolModes)(object)EditorGUI.EnumPopup(source, selected) == BoolModes.True) ? 1 : 0);
					}
				}
			}
			else
			{
				bool flag2 = initializerAlgo.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Add;
				if (initializerAlgo.CreatePage() == WorkerProperty.BridgeProperty.AttrProperty.ChangeType.Set || flag2)
				{
					EditorGUIUtility.labelWidth = 37f;
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						initializerAlgo.CustomizePage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Value"), initializerAlgo.ValidatePage()), -1f, 1f));
					}
					else
					{
						initializerAlgo.CustomizePage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Value"), (int)initializerAlgo.ValidatePage()), flag2 ? (-255) : 0, 255));
					}
					EditorGUIUtility.labelWidth = 0f;
				}
				else
				{
					EditorGUIUtility.labelWidth = 27f;
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						initializerAlgo.RunPage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Min"), initializerAlgo.IncludePage()), -1f, 1f));
						source.x += source.width;
						initializerAlgo.ReflectPage(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Max"), initializerAlgo.LoginPage()), initializerAlgo.IncludePage(), 1f));
					}
					else
					{
						initializerAlgo.RunPage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Min"), (int)initializerAlgo.IncludePage()), 0, 255));
						source.x += source.width;
						initializerAlgo.ReflectPage(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Max"), (int)initializerAlgo.LoginPage()), initializerAlgo.IncludePage(), 255f));
					}
					EditorGUIUtility.labelWidth = 0f;
				}
			}
		}
		else
		{
			EditorGUI.BeginChangeCheck();
			int firstsize = EditorGUI.Popup(source, -1, _Schema);
			if (EditorGUI.EndChangeCheck())
			{
				initializerAlgo.FlushPage(TestAnnotation(firstsize));
			}
			GUI.Label(new Rect(source)
			{
				width = source.width - 5f,
				x = source.x + 5f
			}, initializerAlgo.ResetPage(), "minilabel");
			source.x += source.width + 3f;
			GUI.Label(source, "Parameter not found in Controller!");
			source = new Rect(ident)
			{
				x = ident.x + ident.width - 67f,
				y = ident.y + 2f,
				width = 40f,
				height = EditorGUIUtility.singleLineHeight
			};
			int num = -1;
			EditorGUI.BeginChangeCheck();
			num = (int)(UnityEngine.AnimatorControllerParameterType)(object)EditorGUI.EnumPopup(source, (UnityEngine.AnimatorControllerParameterType)(-1));
			source.x += 4f;
			GUI.Label(source, "Add");
			if (EditorGUI.EndChangeCheck())
			{
				string text = ((!string.IsNullOrEmpty(initializerAlgo.ResetPage())) ? initializerAlgo.ResetPage() : "New Parameter");
				LogoutMapper().AddParameter(text, (UnityEngine.AnimatorControllerParameterType)num);
				List<string> list = _Schema.ToList();
				list.Add(text);
				_Schema = list.ToArray();
				initializerAlgo.FlushPage(text);
				flag = true;
			}
		}
		if (EditorGUI.EndChangeCheck() || flag)
		{
			_QueueAnnotation[cust_end].LogoutInitializer(initializerAlgo);
			_QueueAnnotation[cust_end].m_InitializerAlgo = initializerAlgo;
		}
		if (GUI.Button(rect, ClassProperty.DestroyError()._TagProcessor, ClassProperty.CalcError()._TemplateProcessor))
		{
			_QueueAnnotation[cust_end].PatchInitializer();
			_QueueAnnotation.RemoveAt(cust_end);
			CallAnnotation();
		}
	}

	private static void DisableAnnotation(Rect spec)
	{
		GUI.Label(spec, "Shared VRCParameter Drivers");
		Rect rect = new Rect(spec);
		rect.x += rect.width - 80f;
		rect.width -= rect.x + 2f;
		int num = 0;
		foreach (WorkerProperty.BridgeProperty item in setterAnnotation)
		{
			switch (num)
			{
			case 3:
				if (!item.StartPage())
				{
					num = 2;
				}
				break;
			case 1:
				if (item.StartPage())
				{
					num = 2;
				}
				break;
			case 0:
				if (item.StartPage())
				{
					num = 3;
				}
				else if (!item.StartPage())
				{
					num = 1;
				}
				break;
			}
			if (num == 2)
			{
				break;
			}
		}
		using (new TemplateThread(TemplateThread.ColoringType.BG, num, Color.grey, Color.red, Color.yellow, Color.green))
		{
			if (!GUI.Button(rect, "Local Only"))
			{
				return;
			}
			switch (num)
			{
			case 1:
			case 2:
			{
				foreach (WorkerProperty.BridgeProperty item2 in setterAnnotation)
				{
					item2.ReadPage(instanceinstall: true);
				}
				break;
			}
			case 3:
			{
				foreach (WorkerProperty.BridgeProperty item3 in setterAnnotation)
				{
					item3.ReadPage(instanceinstall: false);
				}
				break;
			}
			}
		}
	}

	private static void InsertAnnotation(ReorderableList spec)
	{
		WorkerProperty.BridgeProperty.AttrProperty attrProperty = null;
		string def = "";
		if (_QueueAnnotation.Count <= 0)
		{
			if ((bool)LogoutMapper() && LogoutMapper().parameters.Length != 0)
			{
				def = TestAnnotation(0);
			}
		}
		else
		{
			attrProperty = _QueueAnnotation.Last().m_InitializerAlgo;
		}
		for (int i = 0; i < m_AlgoAnnotation.Count; i++)
		{
			WorkerProperty.BridgeProperty bridgeProperty = null;
			for (int j = 0; j < m_AlgoAnnotation[i].behaviours.Length; j++)
			{
				if (m_AlgoAnnotation[i].behaviours[j].GetType() == WorkerProperty.InsertPage())
				{
					bridgeProperty = new WorkerProperty.BridgeProperty(m_AlgoAnnotation[i].behaviours[j]);
					break;
				}
			}
			if (bridgeProperty == null)
			{
				StateMachineBehaviour stateMachineBehaviour = (StateMachineBehaviour)ScriptableObject.CreateInstance(WorkerProperty.InsertPage());
				stateMachineBehaviour.hideFlags = HideFlags.HideInHierarchy;
				AssetDatabase.AddObjectToAsset(stateMachineBehaviour, LogoutMapper());
				StateMachineBehaviour[] array = m_AlgoAnnotation[i].behaviours;
				ArrayUtility.Add(ref array, stateMachineBehaviour);
				m_AlgoAnnotation[i].behaviours = array;
				bridgeProperty = new WorkerProperty.BridgeProperty(stateMachineBehaviour);
			}
			WorkerProperty.BridgeProperty.AttrProperty attrProperty2 = bridgeProperty.VisitPage();
			if (attrProperty != null)
			{
				RestartAnnotation(attrProperty, attrProperty2);
			}
			else
			{
				attrProperty2.FlushPage(def);
			}
		}
		CallAnnotation();
		PatchWrapper();
	}

	private static void RestartAnnotation(WorkerProperty.BridgeProperty.AttrProperty def, WorkerProperty.BridgeProperty.AttrProperty vis)
	{
		vis.InstantiatePage(validatekey: true);
		vis.GetPage(def.DestroyPage());
		vis.FlushPage(def.ResetPage());
		vis.CustomizePage(def.ValidatePage());
		vis.NewPage(def.CreatePage());
		vis.RunPage(def.IncludePage());
		vis.ReflectPage(def.LoginPage());
		vis.InstantiatePage(validatekey: false);
	}

	[SpecialName]
	private static int SetupInitializer()
	{
		return (int)m_ConsumerAnnotation.GetValue(ReadAnnotation());
	}

	[SpecialName]
	private static void EnableInitializer(int length_setup)
	{
		m_ConsumerAnnotation.SetValue(ReadAnnotation(), length_setup);
	}

	private static bool AddAnnotation()
	{
		EditorWindow editorWindow = SetterTests.RefTests.PopPolicy();
		if (!(editorWindow != null))
		{
			return false;
		}
		return (bool)helperAnnotation.GetValue(editorWindow);
	}

	private static Animator InvokeAnnotation()
	{
		EditorWindow editorWindow = SetterTests.RefTests.PopPolicy();
		if (!(editorWindow == null))
		{
			return (Animator)recordAnnotation.GetValue(editorWindow);
		}
		return null;
	}

	private static object FindAnnotation()
	{
		object obj = SetterTests.RefTests.PopPolicy();
		if (obj == null)
		{
			return null;
		}
		return _AdapterAnnotation.Invoke(obj, null);
	}

	private static object ExcludeAnnotation()
	{
		return Traverse.Create(FindAnnotation()).Property("edgeGUI").GetValue();
	}

	private static Vector3[] InitAnnotation(Edge last)
	{
		return (Vector3[])interpreterAnnotation.Invoke(ExcludeAnnotation(), new object[1] { last });
	}

	private static object VisitAnnotation()
	{
		if (!LogoutMapper())
		{
			return null;
		}
		if ((object)SetterTests.RefTests.PopPolicy() != null)
		{
			return SetterTests.RefTests.InitPolicy();
		}
		return null;
	}

	private static void DefineAnnotation()
	{
		if ((bool)LogoutMapper())
		{
			PrintMapper(SetterTests.RefTests.RemovePolicy());
		}
	}

	private static object StartAnnotation()
	{
		object obj = VisitAnnotation();
		if (obj != null)
		{
			return _WatcherVisitor.GetMethod("get_stateMachineGraph", BindingFlags.Instance | BindingFlags.Public)?.Invoke(obj, null);
		}
		return null;
	}

	private static object ReadAnnotation()
	{
		EditorWindow editorWindow = SetterTests.RefTests.PopPolicy();
		if (editorWindow == null)
		{
			return null;
		}
		object obj = contextAnnotation.GetValue(editorWindow);
		if (obj == null)
		{
			contextAnnotation.SetValue(editorWindow, obj = Activator.CreateInstance(_ReaderVisitor));
		}
		return obj;
	}

	private static ReorderableList SelectAnnotation()
	{
		object obj = ReadAnnotation();
		if (obj != null)
		{
			return (ReorderableList)m_IdentifierVisitor.GetValue(obj);
		}
		return null;
	}

	private static void RemoveAnnotation()
	{
		OrderInitializer(SetterTests.RefTests.ReadPolicy());
	}

	private static void InstantiateAnnotation()
	{
		if ((object)SetterTests.RefTests.PopPolicy() != null)
		{
			PatchMapper(SetterTests.RefTests.ConcatPolicy());
		}
	}

	private static UnityEngine.AnimatorControllerParameter AwakeAnnotation(string config)
	{
		int second;
		return ResetAnnotation(config, out second);
	}

	private static UnityEngine.AnimatorControllerParameter ResetAnnotation(string spec, out int second)
	{
		if ((bool)LogoutMapper())
		{
			UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
			if (!parameters.ExcludeResolver((UnityEngine.AnimatorControllerParameter p) => p.name == spec, out second))
			{
				return null;
			}
			return parameters[second];
		}
		second = -1;
		return null;
	}

	private static void FlushAnnotation()
	{
		_Method = (from t in ManageMapper().anyStateTransitions
			where t.isExit
			select t.name).ToArray();
	}

	private static void ConnectAnnotation()
	{
		if (!LogoutMapper())
		{
			return;
		}
		UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
		_Schema = new string[parameters.Length];
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < parameters.Length; i++)
		{
			_Schema[i] = parameters[i].name;
			if (parameters[i].type != UnityEngine.AnimatorControllerParameterType.Bool)
			{
				if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Float)
				{
					list.Add(parameters[i].name);
				}
			}
			else
			{
				list2.Add(parameters[i].name);
			}
		}
		_Proxy = list.ToArray();
		broadcaster = list2.ToArray();
	}

	private static void CalculateAnnotation()
	{
		LoginVisitor();
		PushVisitor();
	}

	private static string TestAnnotation(int firstsize)
	{
		return LogoutMapper().parameters[firstsize].name;
	}

	private static string MapAnnotation(string ident, Func<string, bool> caller)
	{
		if (ClassProperty.m_AdapterProcessor.Contains(ident))
		{
			return ident;
		}
		return ClassProperty.SortRules(ident, caller);
	}

	private static void ValidateAnnotation(Action<object[]> key)
	{
		QuickInputWindow quickInputWindow = QuickInputWindow.CreateHelper("Animator QuickInput", new QuickInputWindow.FieldType[3]
		{
			QuickInputWindow.FieldType.Object,
			QuickInputWindow.FieldType.ToggleGroup,
			QuickInputWindow.FieldType.ToggleGroup
		}, new GUIContent[3]
		{
			new GUIContent("Target Animator"),
			new GUIContent("Use Once"),
			new GUIContent("Always Use")
		}, key, delegate(object[] arr)
		{
			bool[] array = new bool[arr.Length];
			array[0] = arr[0] == null;
			return array;
		});
		quickInputWindow.NewHelper(0, ControllerEditorWindow.m_AdvisorMapper ? ControllerEditorWindow.m_AdvisorMapper : UnityEngine.Object.FindObjectsOfType<Animator>().FirstOrDefault((Animator a) => a.avatar));
		quickInputWindow.PushHelper(0, typeof(Animator));
		quickInputWindow.NewHelper(1, true);
		quickInputWindow.comparatorPolicy = new bool[3] { false, true, false };
		quickInputWindow.CollectHelper(m_ContainerVisitor);
	}

	internal static void CustomizeAnnotation(string config)
	{
		FindVisitor(config);
	}

	internal static bool RateAnnotation(bool isi, string selection)
	{
		return FindVisitor(selection, CustomLogType.Warning, isi);
	}

	internal static void DestroyAnnotation(string task)
	{
		FindVisitor(task, CustomLogType.Error);
	}

	private static void GetAnnotation(bool updatereference)
	{
		using (new ConsumerAlgo.WorkerAlgo())
		{
			ConsumerAlgo.CustomerAlgo editingTransitions = ConsumerAlgo.CallDefinition().editingTransitions;
			ConsumerAlgo.CustomerAlgo editingStates = ConsumerAlgo.CallDefinition().editingStates;
			ConsumerAlgo.CustomerAlgo editingController = ConsumerAlgo.CallDefinition().editingController;
			ConsumerAlgo.CustomerAlgo showTransitionSettings = ConsumerAlgo.CallDefinition().showTransitionSettings;
			ConsumerAlgo.CustomerAlgo showTransitionConditions = ConsumerAlgo.CallDefinition().showTransitionConditions;
			ConsumerAlgo.CustomerAlgo showTransitionsCount = ConsumerAlgo.CallDefinition().showTransitionsCount;
			ConsumerAlgo.CustomerAlgo showStateCount = ConsumerAlgo.CallDefinition().showStateCount;
			ConsumerAlgo.CustomerAlgo showStateSettings = ConsumerAlgo.CallDefinition().showStateSettings;
			ConsumerAlgo.CustomerAlgo showVRCDrivers = ConsumerAlgo.CallDefinition().showVRCDrivers;
			bool flag;
			ConsumerAlgo.CallDefinition().showVRCTracking.ExcludeDefinition(flag = updatereference);
			bool flag2;
			showVRCDrivers.ExcludeDefinition(flag2 = flag);
			bool flag3;
			showStateSettings.ExcludeDefinition(flag3 = flag2);
			bool flag4;
			showStateCount.ExcludeDefinition(flag4 = flag3);
			bool flag5;
			showTransitionsCount.ExcludeDefinition(flag5 = flag4);
			bool flag6;
			showTransitionConditions.ExcludeDefinition(flag6 = flag5);
			bool flag7;
			showTransitionSettings.ExcludeDefinition(flag7 = flag6);
			bool flag8;
			editingController.ExcludeDefinition(flag8 = flag7);
			bool excludeparam;
			editingStates.ExcludeDefinition(excludeparam = flag8);
			editingTransitions.ExcludeDefinition(excludeparam);
		}
	}

	internal static void CalcAnnotation(Rect param, string second)
	{
		if ((bool)ConsumerAlgo.CallDefinition().displayParameterType)
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, ConsumerAlgo.CallDefinition().parameterLabelColor.WriteDefinition()))
			{
				ClassProperty.ConnectQueue(param, second, overridethird: true, 0f, 1f, isparam4: false, ConsumerAlgo.m_AdapterAlgo);
			}
		}
	}

	internal static void IncludeAnnotation(bool calci)
	{
		if (calci)
		{
			ClassProperty.MapQueue();
		}
	}

	private static string RunAnnotation(string i, int indexOfpred)
	{
		if (i.Length <= indexOfpred)
		{
			return i;
		}
		return i.Substring(0, indexOfpred - 3) + "...";
	}

	private static string CloneAnnotation(string setup, int endcont, int tag_Z)
	{
		if (setup.Length > endcont + tag_Z + 3)
		{
			return setup.Substring(0, endcont) + "..." + setup.Substring(setup.Length - tag_Z, tag_Z);
		}
		return setup;
	}

	private static void LoginAnnotation(float asset = 8f)
	{
		GUILayout.Label(string.Empty, new GUIStyle(GUI.skin.verticalSlider)
		{
			margin = new RectOffset(),
			padding = new RectOffset(),
			stretchHeight = true
		}, GUILayout.Width(asset));
	}

	private static void ReflectAnnotation(string value)
	{
		using (new GUILayout.HorizontalScope(ClassProperty.CalcError()._WrapperObserver))
		{
			GUILayout.FlexibleSpace();
			GUILayout.Label(value, EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
		}
	}

	private static void DeleteAnnotation(Action def, string token, ConsumerAlgo.CustomerAlgo res, bool iscont2, int visitor3counter)
	{
		if (!res)
		{
			if (ClassProperty.CountQueue(token, EditorStyles.toolbarButton))
			{
				res.InsertDefinition();
			}
			return;
		}
		using (new EditorGUILayout.HorizontalScope())
		{
			using (new EditorGUILayout.VerticalScope(iscont2 ? GUI.skin.box : GUIStyle.none))
			{
				def();
			}
			string text = $"CollapsePart{visitor3counter}";
			ClassProperty.CompareQueue(text, GUILayoutUtility.GetLastRect().height, EventType.Repaint);
			if (ClassProperty.DisableQueue(string.Empty, GUILayout.Height(ClassProperty.OrderQueue(text, 0f)), GUILayout.Width(7f)))
			{
				res.InsertDefinition();
			}
		}
	}

	private static bool CreateAnnotation(UnityEngine.Object asset, params GUILayoutOption[] options)
	{
		return NewAnnotation(asset, string.Empty, options);
	}

	private static bool NewAnnotation(UnityEngine.Object v, string map = "", params GUILayoutOption[] options)
	{
		bool flag = default(bool);
		if (string.IsNullOrEmpty(map))
		{
			map = v.name;
		}
		else
		{
			flag = Selection.activeObject == v;
		}
		bool num = ClassProperty.CountQueue(map, flag ? ClassProperty.CalcError().m_MockProcessor : GUI.skin.label, options);
		if (num)
		{
			Selection.activeObject = (flag ? null : v);
		}
		return num;
	}

	private static void ResolveAnnotation()
	{
		List<(MethodInfo, CallbackServer, bool)> list = new List<(MethodInfo, CallbackServer, bool)>();
		foreach (MethodInfo item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((System.Reflection.Assembly assembly) => assembly.GetTypes().SelectMany((Type t) => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))))
		{
			CallbackServer[] array = item.GetCustomAttributes<CallbackServer>().ToArray();
			foreach (CallbackServer callbackServer in array)
			{
				if (callbackServer is PrototypeServer)
				{
					list.Add((item, callbackServer, true));
				}
				else if (callbackServer is RuleServer)
				{
					list.Add((item, callbackServer, false));
				}
			}
		}
		foreach (var item2 in list.OrderBy<(MethodInfo, CallbackServer, bool), int>(((MethodInfo, CallbackServer, bool onVerify) x) => x.Item2._IssuerServer))
		{
			var (m_ComparatorDefinition, exceptionDefinition, _) = item2;
			if (item2.Item3)
			{
				InterruptAnnotation(delegate
				{
					m_ComparatorDefinition.Invoke(null, exceptionDefinition.m_IndexerServer);
				});
			}
			else
			{
				PrintAnnotation(delegate
				{
					m_ComparatorDefinition.Invoke(null, exceptionDefinition.m_IndexerServer);
				});
			}
		}
	}

	[SpecialName]
	private static void ComputeInitializer(bool ignoresetup)
	{
		bool watcherAnnotation = m_WatcherAnnotation;
		m_WatcherAnnotation = ignoresetup;
		if (!m_WatcherAnnotation && watcherAnnotation)
		{
			FactoryAlgo.EnableReg(null);
		}
	}

	private static void ListAnnotation()
	{
		SetVisitor("Send Feedback for Controller Editor", "If you have a suggestion, preference, or something to comment, please send it here!\nNote that the feedback is not anonymous. Abuse may result in blacklisting.");
		m_ExpressionAnnotation = listenerAnnotation;
		m_WorkerAnnotation = EditorGUILayout.TextArea(m_WorkerAnnotation, GUILayout.MinHeight(54f));
		using (new GUILayout.HorizontalScope())
		{
			if (ClassProperty.CountQueue("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
			{
				m_ExpressionAnnotation = false;
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(m_WorkerAnnotation) || systemAnnotation))
			{
				if (!ClassProperty.CountQueue("Send Feedback", EditorStyles.toolbarButton))
				{
					return;
				}
				if (m_WorkerAnnotation.Length > 2000)
				{
					m_WorkerAnnotation = m_WorkerAnnotation.Substring(0, 2000);
				}
				List<(string, string)> list = RegisterAnnotation("sendfeedback", new(string, string)[1] { ("feedback", Uri.EscapeUriString(m_WorkerAnnotation)) });
				LogoutAnnotation(list);
				systemAnnotation = true;
				DisableVisitor(CallVisitor(list.ToArray())).QueryRules(ChangeAnnotation, UnityEngine.Debug.LogException, null, null, delegate
				{
					systemAnnotation = false;
					while (true)
					{
						m_ExpressionAnnotation = false;
					}
				});
			}
		}
	}

	[SpecialName]
	private static float ConcatInitializer()
	{
		return m_RegistryAnnotation - Time.realtimeSinceStartup;
	}

	[SpecialName]
	private static bool CancelInitializer()
	{
		return ConcatInitializer() > 0f;
	}

	[InitializeOnLoadMethod]
	private static void VerifyAnnotation()
	{
		ResolveAnnotation();
		bool flag = AssetAnnotation();
		if (!ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification)
		{
			m_IdentifierAnnotation = true;
			m_DispatcherAnnotation = flag;
		}
		if (flag && (bool)ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad)
		{
			ClassProperty.CountRules(delegate
			{
				WriteAnnotation(assetneeded: false);
			});
		}
	}

	private static void FillAnnotation()
	{
		if (!m_DispatcherAnnotation && (bool)ConsumerAlgo.CallDefinition().a_VerifyOnDisplay && AssetAnnotation())
		{
			WriteAnnotation(assetneeded: false);
		}
	}

	private static void WriteAnnotation(bool assetneeded)
	{
		_003C_003Ec__DisplayClass186_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass186_0();
		if ((!ConsumerAlgo.CallDefinition().a_VerifyOnDisplay.FindDefinition() && !ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad.FindDefinition() && !assetneeded) || (m_IdentifierAnnotation && !attrAnnotation) || _RequestAnnotation)
		{
			return;
		}
		attrAnnotation = false;
		_RequestAnnotation = true;
		m_DispatcherAnnotation = true;
		CS_0024_003C_003E8__locals9.m_ObjectDefinition = "yOk0XCnENLMO6DIF8cYpSg==" + EditorAnalyticsSessionInfo.id;
		try
		{
			if (SessionState.GetBool(CS_0024_003C_003E8__locals9.m_ObjectDefinition, defaultValue: false))
			{
				_003C_003Ec__DisplayClass186_1 _003C_003Ec__DisplayClass186_1_ = default(_003C_003Ec__DisplayClass186_1);
				_003C_003Ec__DisplayClass186_1_.utilsDefinition = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass186_1_.utilsDefinition.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
					_003C_003Ec__DisplayClass186_1_.utilsDefinition.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
					_003C_003Ec__DisplayClass186_2 _003C_003Ec__DisplayClass186_2_ = default(_003C_003Ec__DisplayClass186_2);
					_003C_003Ec__DisplayClass186_2_._ValDefinition = new HMACSHA1(Encoding.UTF8.GetBytes(CS_0024_003C_003E8__locals9.m_ObjectDefinition));
					try
					{
						if (PatchAnnotation() == CS_0024_003C_003E8__locals9.RunObserver("date", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_))
						{
							m_FilterAnnotation = CS_0024_003C_003E8__locals9.RunObserver("u", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							m_ReaderAnnotation = CS_0024_003C_003E8__locals9.RunObserver("v", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							m_ParamsAnnotation = CS_0024_003C_003E8__locals9.RunObserver("r", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							_WriterAnnotation = CS_0024_003C_003E8__locals9.RunObserver("m", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							StopAnnotation();
							CheckAnnotation();
							listenerAnnotation = true;
							m_IdentifierAnnotation = true;
							_RequestAnnotation = false;
							m_GetterAnnotation = true;
						}
					}
					finally
					{
						if (_003C_003Ec__DisplayClass186_2_._ValDefinition != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass186_2_._ValDefinition).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass186_1_.utilsDefinition != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass186_1_.utilsDefinition).Dispose();
					}
				}
			}
		}
		catch
		{
			FindVisitor("failed to verify from cache.", CustomLogType.Warning);
		}
		if (m_GetterAnnotation)
		{
			ManageAnnotation(applyident: true);
			RestartVisitor();
		}
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("verifylicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				while (true)
				{
					_003C_003Ec__DisplayClass186_0 m_MerchantDefinition = CS_0024_003C_003E8__locals9;
					FieldAlgo valueDefinition = response;
				}
			}, _003C_003Ec.watcherInitializer.CollectProperty, null, null, InsertVisitor);
		}, iscont: true);
	}

	private static void ForgotAnnotation()
	{
		importerAnnotation = true;
		if (!SetupVisitor())
		{
			FindVisitor("Invalid License Key!", CustomLogType.Error);
			return;
		}
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("activatelicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				importerAnnotation = false;
				SortAnnotation(response, delegate
				{
					m_IdentifierAnnotation = false;
					ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: true);
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(System.Exception exception)
			{
				importerAnnotation = false;
				FindVisitor($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
			}, null, null, InsertVisitor);
		}, iscont: true);
	}

	private static void StopAnnotation()
	{
		stubAnnotation = m_FilterAnnotation;
		if (string.IsNullOrWhiteSpace(stubAnnotation))
		{
			return;
		}
		try
		{
			Match match = Regex.Match(stubAnnotation, "(?i)(?:<color=#(?:[0-9a-f]{8}|[0-9a-f]{6})>)?.*?(#\\d{4})(?:<\\/color>)?$");
			if (match.Success)
			{
				stubAnnotation = stubAnnotation.Remove(match.Groups[1].Index, match.Groups[1].Length);
			}
			if (stubAnnotation.Length > 1 && stubAnnotation[0] == '@')
			{
				stubAnnotation = stubAnnotation.Substring(1);
			}
		}
		catch
		{
		}
	}

	private static void CheckAnnotation()
	{
		string[] array = _WriterAnnotation.Split(new char[1] { '-' });
		string[] array2 = PatchAnnotation().Split(new char[1] { '/' });
		array2[2] = array2[2].Substring(2, 2);
		printerAnnotation = array2[2] + array[0].Substring(0, 10) + array2[1] + array[2].Substring(0, 10) + array2[0];
	}

	private static void PrepareAnnotation()
	{
		if (string.IsNullOrWhiteSpace(databaseAnnotation))
		{
			string key = "DreadScriptssid";
			databaseAnnotation = EditorPrefs.GetString(key, string.Empty);
			if (string.IsNullOrWhiteSpace(databaseAnnotation) || !Regex.IsMatch(databaseAnnotation, "[0-9a-f]{32}"))
			{
				databaseAnnotation = GUID.Generate().ToString();
				EditorPrefs.SetString(key, databaseAnnotation);
			}
		}
	}

	private static bool AssetAnnotation()
	{
		if (!string.IsNullOrWhiteSpace(m_BridgeAnnotation))
		{
			return true;
		}
		m_BridgeAnnotation = EditorPrefs.GetString("yOk0XCnENLMO6DIF8cYpSg==LK", string.Empty);
		if (!EnableVisitor())
		{
			m_BridgeAnnotation = string.Empty;
		}
		return !(m_IdentifierAnnotation = string.IsNullOrWhiteSpace(m_BridgeAnnotation));
	}

	private static void UpdateAnnotation(Action item, bool iscont = false)
	{
		_003C_003Ec__DisplayClass192_0 CS_0024_003C_003E8__locals31 = new _003C_003Ec__DisplayClass192_0();
		CS_0024_003C_003E8__locals31._FieldDefinition = iscont;
		CS_0024_003C_003E8__locals31.m_AttributeDefinition = item;
		CS_0024_003C_003E8__locals31.containerDefinition = new string[4][]
		{
			new string[3] { "Manufacturer", "Product", "SerialNumber" },
			new string[1] { "ProcessorId" },
			new string[1] { "SerialNumber" },
			new string[4] { "Manufacturer", "PartNumber", "SerialNumber", "Capacity" }
		};
		CS_0024_003C_003E8__locals31.composerDefinition = new StringBuilder();
		CS_0024_003C_003E8__locals31.classDefinition = new StringBuilder();
		CS_0024_003C_003E8__locals31._ParameterDefinition = EditorPrefs.GetString("DSLICINF", string.Empty);
		CS_0024_003C_003E8__locals31._PoolDefinition = string.IsNullOrWhiteSpace(CS_0024_003C_003E8__locals31._ParameterDefinition);
		if (!CS_0024_003C_003E8__locals31._PoolDefinition)
		{
			try
			{
				CS_0024_003C_003E8__locals31._ParameterDefinition = NewMapper(CS_0024_003C_003E8__locals31._ParameterDefinition);
			}
			catch
			{
				CS_0024_003C_003E8__locals31._ParameterDefinition = string.Empty;
				CS_0024_003C_003E8__locals31._PoolDefinition = true;
				EditorPrefs.DeleteKey("DSLICINF");
			}
		}
		CS_0024_003C_003E8__locals31.repositoryDefinition = new string[4];
		CS_0024_003C_003E8__locals31.mappingDefinition = new string[4];
		CS_0024_003C_003E8__locals31.baseDefinition = new string[4];
		PoolAlgo[] key = new PoolAlgo[4]
		{
			new PoolAlgo("wmic baseboard get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[0] = o;
			}, isfield: true),
			new PoolAlgo("wmic cpu get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[1] = o;
			}, isfield: true),
			new PoolAlgo("wmic diskdrive get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[2] = o;
			}, isfield: true),
			new PoolAlgo("wmic memorychip get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[3] = o;
			}, isfield: true)
		};
		CS_0024_003C_003E8__locals31.instanceDefinition = new PoolAlgo[4]
		{
			new PoolAlgo("Get-CimInstance -class Win32_baseboard | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[0] = o;
			}),
			new PoolAlgo("Get-CimInstance -class Win32_processor | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[1] = o;
			}),
			new PoolAlgo("Get-CimInstance -class Win32_diskdrive | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[2] = o;
			}),
			new PoolAlgo("Get-CimInstance -class win32_physicalmemory | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[3] = o;
			})
		};
		CS_0024_003C_003E8__locals31._MockDefinition = new CancellationTokenSource();
		CS_0024_003C_003E8__locals31._MockDefinition.CancelAfter(10000);
		PushMapper(key, delegate
		{
			try
			{
				CS_0024_003C_003E8__locals31.UpdateObserver(isCMD: true);
				CS_0024_003C_003E8__locals31.SortObserver();
			}
			catch (System.Exception exc)
			{
				CS_0024_003C_003E8__locals31.ChangeObserver(isCMD: true, exc);
			}
		}, CS_0024_003C_003E8__locals31._MockDefinition);
	}

	private static void ChangeAnnotation(FieldAlgo v)
	{
		SortAnnotation(v, null);
	}

	private static void SortAnnotation(FieldAlgo key, Action token, Action serv = null, bool t2stop = true)
	{
		bool num = key.InsertReg("success");
		string text = key.InsertReg("message");
		string text2 = key.InsertReg("url");
		bool flag = !string.IsNullOrEmpty(text2);
		string text3 = key.InsertReg("url_name");
		if (string.IsNullOrWhiteSpace(text3))
		{
			text3 = "Link";
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			text = text.Replace("\\n", "\n");
		}
		if (!num)
		{
			bool flag2 = key.InsertReg("wait_warn");
			float num2 = key.InsertReg("wait_time");
			m_ExporterAnnotation |= flag2;
			if (!(num2 <= 0f))
			{
				m_RegistryAnnotation = Time.realtimeSinceStartup + num2;
			}
			serv?.Invoke();
			if (!string.IsNullOrEmpty(text))
			{
				FindVisitor(text, CustomLogType.Error);
				if (!flag)
				{
					EditorUtility.DisplayDialog("Warning!", text, "Ok");
				}
				else if (EditorUtility.DisplayDialog("Warning!", text, text3, "Ok"))
				{
					Application.OpenURL(text2);
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(text) && t2stop)
			{
				FindVisitor(text);
			}
			token?.Invoke();
		}
	}

	private static List<(string, string)> RegisterAnnotation(string task, IEnumerable<(string, string)> pred = null)
	{
		PrepareAnnotation();
		List<(string, string)> list = new List<(string, string)>
		{
			("command", task),
			("product_id", "yOk0XCnENLMO6DIF8cYpSg=="),
			("version", m_RefAnnotation.ToString()),
			("HWID", _WriterAnnotation),
			("SID", databaseAnnotation),
			("license_key", m_BridgeAnnotation)
		};
		if (pred != null)
		{
			list.AddRange(pred);
		}
		return list;
	}

	private static void LogoutAnnotation(List<(string, string)> item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var item4 in item)
		{
			string item2 = item4.Item2;
			stringBuilder.Append(item2);
		}
		using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w["));
		string item3 = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
		item.Add(("hash", item3));
	}

	private static string PatchAnnotation()
	{
		string text = ListMapper(DateTime.UtcNow.Day.ToString());
		string text2 = ListMapper(DateTime.UtcNow.Month.ToString());
		string text3 = DateTime.UtcNow.Year.ToString();
		m_TagAnnotation = text + "/" + text2 + "/" + text3;
		return m_TagAnnotation;
	}

	private static void InterruptAnnotation(Action instance)
	{
		if (listenerAnnotation)
		{
			if (((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
				return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
			})())
			{
				instance?.Invoke();
			}
		}
		else
		{
			creatorAnnotation = (Action)Delegate.Remove(creatorAnnotation, instance);
			creatorAnnotation = (Action)Delegate.Combine(creatorAnnotation, instance);
		}
	}

	private static void ManageAnnotation(bool applyident)
	{
		if (listenerAnnotation && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			if (!m_InterceptorAnnotation)
			{
				creatorAnnotation?.Invoke();
			}
			m_InterceptorAnnotation = true;
		}
	}

	private static void PrintAnnotation(Action v)
	{
		m_EventAnnotation = (Action)Delegate.Remove(m_EventAnnotation, v);
		m_EventAnnotation = (Action)Delegate.Combine(m_EventAnnotation, v);
	}

	private static void SearchAnnotation()
	{
		listenerAnnotation = false;
		m_GetterAnnotation = false;
		m_ParamsAnnotation = (m_FilterAnnotation = (m_ReaderAnnotation = string.Empty));
		ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: false);
		SessionState.EraseBool("yOk0XCnENLMO6DIF8cYpSg==" + EditorAnalyticsSessionInfo.id);
		m_EventAnnotation?.Invoke();
	}

	[SpecialName]
	private static string DisableInitializer()
	{
		string text = "";
		if (m_ExporterAnnotation)
		{
			text += "Too many failed attempts! Further failed attempts will result in getting your device blocked!\n";
		}
		if (CancelInitializer())
		{
			text += $"Please wait {Mathf.CeilToInt(ConcatInitializer())} seconds.";
		}
		return text;
	}

	private static void RevertAnnotation()
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.Label("License: " + (string.IsNullOrWhiteSpace(m_ReaderAnnotation) ? "Personal" : m_ReaderAnnotation), ClassProperty.CalcError().annotationObserver);
				GUILayout.FlexibleSpace();
			}
			if (!string.IsNullOrWhiteSpace(stubAnnotation))
			{
				using (new GUILayout.HorizontalScope(GUI.skin.box))
				{
					GUILayout.Label("Authorized For: " + stubAnnotation, ClassProperty.CalcError().algoObserver);
					return;
				}
			}
		}
	}

	private static bool OrderVisitor(EditorWindow value = null, float map = 0f)
	{
		if (!listenerAnnotation)
		{
			if (Event.current.type == EventType.Repaint)
			{
				FillAnnotation();
			}
			if ((object)value != null)
			{
				ClassProperty.setterProcessor.PopHelper(value, map);
			}
			RemoveVisitor();
			if (importerAnnotation || _RequestAnnotation)
			{
				SetVisitor((!importerAnnotation) ? "Verifying License..." : "Activating License...", "Please wait till this finishes processing.");
				return false;
			}
			if (facadeAnnotation)
			{
				CompareVisitor();
				return false;
			}
			if (!m_IdentifierAnnotation || attrAnnotation)
			{
				SetVisitor("Check for License", "This will check for whether you already have a license for your device");
				if (ClassProperty.CountQueue((!attrAnnotation) ? "Check" : "Retry", EditorStyles.toolbarButton))
				{
					WriteAnnotation(assetneeded: true);
				}
				return false;
			}
			SetVisitor("Enter your license key", "Enter the license key you received with your purchase here. If your license was already activated, click on 'Transfer License'. For support, contact @Dreadrith.");
			bool flag = PostVisitor(isinit: false);
			if (DisableInitializer().Length > 0)
			{
				EditorGUILayout.HelpBox(DisableInitializer(), MessageType.Error);
			}
			bool flag2 = EnableVisitor() && !CancelInitializer();
			flag &= flag2 && !m_DispatcherAnnotation;
			using (new EditorGUI.DisabledScope(!flag2))
			{
				if (ClassProperty.DisableQueue("Activate") || flag)
				{
					ForgotAnnotation();
				}
			}
			DefineVisitor(PopVisitor);
			return false;
		}
		if (m_ExpressionAnnotation)
		{
			ListAnnotation();
			return false;
		}
		if (m_WatcherAnnotation)
		{
			FactoryAlgo.PostReg();
			return false;
		}
		return true;
	}

	private static void CompareVisitor()
	{
		SetVisitor("Transferring License", "This is for moving your license to a new device or re-activating it in case it fails to recognize your device.");
		if (!advisorAnnotation)
		{
			EditorGUILayout.HelpBox("Use this to move your own license from another device.\nAfter entering your license key, press 'Send Verification Code' to send a 6-digit code to the email address associated with the license key.", MessageType.Info);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(m_CallbackAnnotation);
			try
			{
				PostVisitor(isinit: true);
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			if (DisableInitializer().Length > 0)
			{
				EditorGUILayout.HelpBox(DisableInitializer(), MessageType.Error);
			}
			disabledScope = new EditorGUI.DisabledScope(!SetupVisitor() || m_CallbackAnnotation);
			try
			{
				if (ClassProperty.DisableQueue(m_CallbackAnnotation ? "Sending Verification Code..." : "Send Verification Code"))
				{
					InitVisitor();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("A 6-digit verification code was sent to " + m_CustomerAnnotation + ".\nIf this is not your email address, please contact support.\nIf you don't see the verification email, please check your spam folder.", MessageType.Info);
			strategyAnnotation = EditorGUILayout.TextField("Verification Code", strategyAnnotation);
			strategyAnnotation = Regex.Replace(strategyAnnotation, "[^0-9]", string.Empty, RegexOptions.Multiline);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!Regex.IsMatch(strategyAnnotation, "[0-9]{6}") || _IndexerAnnotation);
			try
			{
				if (ClassProperty.DisableQueue((!_IndexerAnnotation) ? "Transfer License" : "Transferring..."))
				{
					VisitVisitor();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		DefineVisitor(PopVisitor);
	}

	private static void SetVisitor(string item, string cust)
	{
		using (new GUILayout.HorizontalScope(ClassProperty.CalcError()._WrapperObserver))
		{
			GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
			GUILayout.Label(item, ClassProperty.CalcError()._StructProcessor);
			GUILayout.Label(new GUIContent(ClassProperty.DestroyError()._DicProcessor)
			{
				tooltip = cust
			}, ClassProperty.CalcError().broadcasterProcessor, GUILayout.Width(17f), GUILayout.Height(17f));
		}
	}

	private static bool PostVisitor(bool isinit)
	{
		using (new GUILayout.HorizontalScope())
		{
			string text = "Controller EditorLicenseField";
			if (ClassProperty.ResolveQueue(text))
			{
				GUI.FocusControl(null);
				return true;
			}
			if (ClassProperty.ListQueue(text))
			{
				GUI.FocusControl(null);
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (isinit)
				{
					EditorGUILayout.PrefixLabel("License Key");
				}
				GUI.SetNextControlName(text);
				m_BridgeAnnotation = EditorGUILayout.TextField(string.Empty, m_BridgeAnnotation).Trim();
				ClassProperty.TestQueue("License Key", string.IsNullOrWhiteSpace(m_BridgeAnnotation), 80f);
			}
			if (!m_DispatcherAnnotation && EnableVisitor() && !CancelInitializer())
			{
				m_DispatcherAnnotation = true;
				return true;
			}
		}
		return false;
	}

	private static bool SetupVisitor()
	{
		if (!facadeAnnotation)
		{
			if (!CancelInitializer())
			{
				return EnableVisitor();
			}
			return false;
		}
		if (!CancelInitializer() && EnableVisitor())
		{
			return PublishVisitor();
		}
		return false;
	}

	private static bool EnableVisitor()
	{
		return Regex.Match(m_BridgeAnnotation, "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
	}

	private static bool PublishVisitor()
	{
		if (!advisorAnnotation)
		{
			return true;
		}
		return Regex.Match(strategyAnnotation, "^[a-zA-Z0-9]{6}$").Success;
	}

	private static void PopVisitor()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (ClassProperty.ComputeQueue((!facadeAnnotation) ? "Transfer License" : "Activate License"))
			{
				facadeAnnotation = !facadeAnnotation;
			}
		}
	}

	private static void ComputeVisitor(string asset = null, string pol = null, string dir = null)
	{
		ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition(excludeparam: false);
		if (asset != null)
		{
			ConsumerAlgo.CallDefinition().u_announcement.ResolveDefinition(asset);
		}
		if (pol != null)
		{
			ConsumerAlgo.CallDefinition().u_announcementLink.ResolveDefinition(pol);
		}
		if (dir != null)
		{
			ConsumerAlgo.CallDefinition().u_announcementLinkName.ResolveDefinition(dir);
		}
	}

	private static void MoveVisitor(string ident = null, string pred = null, string third = null, string token2 = null, bool havetoken3 = false, bool? selection4 = null)
	{
		bool excludeparam = ConsumerAlgo.CallDefinition().u_announcementHidden;
		if (ident != null)
		{
			ConsumerAlgo.CallDefinition().u_updateVersion.ResolveDefinition(ident);
		}
		if (pred != null)
		{
			ConsumerAlgo.CallDefinition().u_updateMessage.ResolveDefinition(pred);
		}
		if (third != null)
		{
			ConsumerAlgo.CallDefinition().u_updateLink.ResolveDefinition(third);
		}
		if (token2 != null)
		{
			ConsumerAlgo.CallDefinition().u_updateChangelog.ResolveDefinition(token2);
		}
		if (selection4.HasValue)
		{
			ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: false);
		}
		ConnectVisitor(havetoken3);
		ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition(excludeparam);
	}

	private static void ConcatVisitor()
	{
		ComputeVisitor("A longy test announcement for testing the GUI of an announcement in an announcy way. This is one of the announcements of all time. It's announcing what should be announced during an announcement. Thank you for being announced to.", "https://www.youtube.com/watch?v=0tOXxuLcaog", "Chipi Chipi");
		MoveVisitor("3.3.3", "You have an update to be updated about! This update is an update for what's to be update till it's up to date. Being up to date is recommended by those that want to be updated about all sorts of updates", null, "https://www.youtube.com/watch?v=0tOXxuLcaog", havetoken3: true, false);
	}

	private static string CallVisitor(IEnumerable<(string, string)> value)
	{
		StringBuilder stringBuilder = new StringBuilder("{");
		bool flag = true;
		foreach (var (text, text2) in value)
		{
			if (!flag)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("\"" + text + "\":\"" + text2 + "\"");
			flag = false;
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private static HttpWebRequest CancelVisitor(string asset)
	{
		HttpWebRequest httpWebRequest = WebRequest.CreateHttp(asset);
		httpWebRequest.Method = "POST";
		httpWebRequest.Accept = "application/json";
		httpWebRequest.ContentType = "application/json";
		return httpWebRequest;
	}

	private static async Task<FieldAlgo> CountVisitor(string reference, string caller)
	{
		FieldAlgo collectionDefinition = default(FieldAlgo);
		await Task.Run(async delegate
		{
			HttpWebRequest httpWebRequest = CancelVisitor(reference);
			using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				await streamWriter.WriteAsync(caller);
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
			string i = await streamReader.ReadToEndAsync();
			streamReader.Dispose();
			collectionDefinition = new FieldAlgo(i);
		});
		return collectionDefinition;
	}

	private static Task<FieldAlgo> DisableVisitor(string info)
	{
		return CountVisitor("https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand", info);
	}

	private static void InsertVisitor()
	{
		ClassProperty.CountRules(RestartVisitor);
	}

	private static void RestartVisitor()
	{
		Type[] array = infoAnnotation;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(array[i]);
			foreach (UnityEngine.Object obj in array2)
			{
				if (!(obj is EditorWindow editorWindow))
				{
					if (obj is Editor editor)
					{
						editor.Repaint();
					}
				}
				else
				{
					editorWindow.Repaint();
				}
			}
		}
	}

	private static void QueryVisitor()
	{
		using (new TemplateThread(TemplateThread.ColoringType.BG, Color.clear))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button(new GUIContent("Made By @Dreadrith", "https://dreadrith.com/links"), ClassProperty.CalcError().m_DefinitionObserver))
				{
					Application.OpenURL("https://dreadrith.com/links");
				}
				ClassProperty.ConcatQueue();
				SupportThankies.ListWrapper();
			}
		}
	}

	internal static bool AddVisitor(string def, bool countresult = true)
	{
		return FindVisitor(def, CustomLogType.Warning, countresult);
	}

	internal static bool InvokeVisitor(string param, bool iscounter = true)
	{
		return FindVisitor(param, CustomLogType.Error, iscounter);
	}

	internal static bool FindVisitor(string param, CustomLogType selection = CustomLogType.Regular, bool setrule = true)
	{
		if (setrule)
		{
			Color color = ((selection == CustomLogType.Regular) ? ClassProperty.configurationProperty : ((selection != CustomLogType.Warning) ? ClassProperty._ProcProperty : ClassProperty._WrapperProcessor));
			string message = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[Controller Editor]</color> " + param.Replace("\\n", "\n");
			switch (selection)
			{
			case CustomLogType.Error:
				UnityEngine.Debug.LogError(message);
				break;
			case CustomLogType.Warning:
				UnityEngine.Debug.LogWarning(message);
				break;
			case CustomLogType.Regular:
				UnityEngine.Debug.Log(message);
				break;
			}
		}
		return setrule;
	}

	internal static void ExcludeVisitor(string spec, bool haveb = true)
	{
		if (haveb)
		{
			throw new System.Exception("<color=#" + ColorUtility.ToHtmlStringRGB(ClassProperty._ProcProperty) + ">[Controller Editor]</color> " + spec);
		}
	}

	private static void InitVisitor()
	{
		string message = "License transfer is subject to the Terms of Service.\nLicense will stop working on the device it was previously activated on.\nYou will not be able to transfer back or again for 30 days.";
		switch (EditorUtility.DisplayDialogComplex("Terms of Service", message, "Continue", "Terms of Service", "Cancel"))
		{
		case 0:
			m_CallbackAnnotation = true;
			UpdateAnnotation(delegate
			{
				List<(string, string)> list = RegisterAnnotation("transferlicenserequest");
				LogoutAnnotation(list);
				DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate
				{
				}, delegate(System.Exception exception)
				{
					m_CallbackAnnotation = false;
					FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
				}, null, null, InsertVisitor);
			}, iscont: true);
			break;
		case 1:
			Application.OpenURL("https://dreadrith.com/license-tos");
			break;
		}
	}

	private static void VisitVisitor()
	{
		_IndexerAnnotation = true;
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenseconfirm");
			list.Add(("verification_code", strategyAnnotation));
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
			{
				_IndexerAnnotation = false;
				SortAnnotation(response, delegate
				{
					facadeAnnotation = false;
					advisorAnnotation = false;
					m_IdentifierAnnotation = false;
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(System.Exception exception)
			{
				_IndexerAnnotation = false;
				FindVisitor($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, InsertVisitor);
		}, iscont: true);
	}

	[SpecialName]
	private static bool RestartInitializer()
	{
		return ConsumerAlgo.CallDefinition().u_updateDay == PatchAnnotation();
	}

	private static void DefineVisitor(Action item = null, Action<GenericMenu> cont = null)
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (ClassProperty.CallQueue(ClassProperty.DestroyError().m_SingletonProcessor))
				{
					ReadVisitor(cont);
				}
				if (singletonAnnotation && !ConsumerAlgo.CallDefinition().u_updateHidden && ClassProperty.CallQueue(ClassProperty.DestroyError().m_AdvisorProcessor))
				{
					factoryAnnotation.target = !factoryAnnotation.target;
				}
				GUILayout.Label("v" + m_RefAnnotation, ClassProperty.CalcError()._MapperObserver, GUILayout.ExpandWidth(expand: false));
				if (item != null)
				{
					item();
				}
				else
				{
					GUILayout.FlexibleSpace();
					QueryVisitor();
				}
			}
			if (singletonAnnotation && !ConsumerAlgo.CallDefinition().u_updateHidden && factoryAnnotation.target)
			{
				EditorGUILayout.Space();
			}
			SelectVisitor();
		}
	}

	private static void StartVisitor(Action key = null, Action<GenericMenu> cfg = null)
	{
		using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
		{
			if (ClassProperty.CallQueue(ClassProperty.DestroyError().m_SingletonProcessor))
			{
				ReadVisitor(cfg);
			}
			if (singletonAnnotation && !ConsumerAlgo.CallDefinition().u_updateHidden && ClassProperty.CallQueue(ClassProperty.DestroyError().m_AdvisorProcessor))
			{
				factoryAnnotation.target = !factoryAnnotation.target;
			}
			GUILayout.Label("v" + m_RefAnnotation, ClassProperty.CalcError().m_InitializerObserver, GUILayout.ExpandWidth(expand: false));
			key?.Invoke();
		}
	}

	private static void ReadVisitor(Action<GenericMenu> init = null)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Check For Update"), on: false, (!_PrototypeAnnotation && !_RuleAnnotation) ? ((GenericMenu.MenuFunction)delegate
		{
			SessionState.EraseString("yOk0XCnENLMO6DIF8cYpSg==updateinfo");
			AwakeVisitor();
		}) : null);
		if (listenerAnnotation)
		{
			genericMenu.AddItem(new GUIContent("Send Feedback"), m_ExpressionAnnotation, delegate
			{
				m_ExpressionAnnotation.AwakeResolver();
			});
		}
		if (listenerAnnotation)
		{
			if (init != null)
			{
				init(genericMenu);
				genericMenu.AddSeparator(string.Empty);
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Verify/On Display"), ConsumerAlgo.CallDefinition().a_VerifyOnDisplay, delegate
			{
				ConsumerAlgo.CallDefinition().a_VerifyOnDisplay.InsertDefinition();
				ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad.ExcludeDefinition(excludeparam: false);
			});
			genericMenu.AddItem(new GUIContent("Verify/On Project Load"), ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad, delegate
			{
				ConsumerAlgo.CallDefinition().a_VerifyOnProjectLoad.InsertDefinition();
				ConsumerAlgo.CallDefinition().a_VerifyOnDisplay.ExcludeDefinition(excludeparam: false);
			});
		}
		genericMenu.AddSeparator(string.Empty);
		if (!string.IsNullOrWhiteSpace("https://notes.sleightly.dev/controllereditor/"))
		{
			genericMenu.AddItem(new GUIContent("Documentation"), on: false, delegate
			{
				Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
			});
		}
		if (listenerAnnotation)
		{
			if (statusAnnotation.Length != 0)
			{
				if (statusAnnotation.Length <= 1)
				{
					genericMenu.AddItem(new GUIContent(statusAnnotation[0].Item1), on: false, delegate
					{
						Application.OpenURL(statusAnnotation[0].Item2);
					});
				}
				else
				{
					(string, string)[] array = statusAnnotation;
					for (int num = 0; num < array.Length; num++)
					{
						(string, string) tuple = array[num];
						string item = tuple.Item1;
						string stateDefinition = tuple.Item2;
						string text = "Samples/" + item;
						genericMenu.AddItem(new GUIContent(text), on: false, delegate
						{
							Application.OpenURL(stateDefinition);
						});
					}
				}
			}
			if (!string.IsNullOrWhiteSpace("https://github.com/Dreadrith/DreadScripts/blob/main/ControllerEditor/Changelog.txt"))
			{
				genericMenu.AddItem(new GUIContent("Changelog"), on: false, delegate
				{
					Application.OpenURL("https://github.com/Dreadrith/DreadScripts/blob/main/ControllerEditor/Changelog.txt");
				});
			}
		}
		if (!string.IsNullOrWhiteSpace("https://www.dreadrith.com/l/CEditor"))
		{
			genericMenu.AddItem(new GUIContent("Store Page"), on: false, delegate
			{
				Application.OpenURL("https://www.dreadrith.com/l/CEditor");
			});
		}
		genericMenu.AddItem(new GUIContent("ToS and Privacy Policy"), on: false, delegate
		{
			Application.OpenURL("https://dreadrith.com/license-tos");
		});
		genericMenu.ShowAsContext();
	}

	private static void SelectVisitor(bool isi = false, bool isivk = true)
	{
		if (!singletonAnnotation || (bool)ConsumerAlgo.CallDefinition().u_updateHidden)
		{
			return;
		}
		factoryAnnotation.SearchResolver(delegate
		{
			if (isi)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ClassProperty.DestroyError().indexerProcessor, GUILayout.Width(24f), GUILayout.Height(24f));
				GUILayout.Label($"v{ConsumerAlgo.CallDefinition().u_updateVersion}", ClassProperty.CalcError().itemProcessor);
			}
			if (ClassProperty.DefineQueue())
			{
				factoryAnnotation.target = !factoryAnnotation.target;
			}
			ClassProperty.MapQueue();
			GUILayout.TextArea(ConsumerAlgo.CallDefinition().u_updateMessage, ClassProperty.CalcError().m_GlobalProcessor);
			bool flag = !string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_updateLink);
			bool flag2 = !string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_updateChangelog);
			EditorGUILayout.Space();
			using (new GUILayout.HorizontalScope())
			{
				if (flag)
				{
					using (new EditorGUI.DisabledScope(_IssuerAnnotation))
					{
						if (ClassProperty.CountQueue("Download Update", EditorStyles.toolbarButton))
						{
							FlushVisitor();
						}
					}
				}
				if (flag2 && ClassProperty.RestartQueue(new GUIContent("Open Changelog", ConsumerAlgo.CallDefinition().u_updateChangelog), EditorStyles.toolbarButton))
				{
					Application.OpenURL(ConsumerAlgo.CallDefinition().u_updateChangelog);
				}
				if (ClassProperty.CountQueue("Skip for Today", EditorStyles.toolbarButton))
				{
					ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: true);
				}
			}
			if (isi)
			{
				EditorGUILayout.EndVertical();
			}
		}, RestartVisitor);
	}

	private static void RemoveVisitor()
	{
		if ((bool)ConsumerAlgo.CallDefinition().u_announcementHidden || string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_announcement))
		{
			return;
		}
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Rect taskDefinition = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true), GUILayout.Height(24f));
			Rect def = taskDefinition;
			GUI.Label(def.SortResolver(24f, isfield: true), ClassProperty.DestroyError().indexerProcessor);
			GUI.Label(def, "Announcement", ClassProperty.CalcError().itemProcessor);
			_AccountAnnotation.SearchResolver(delegate
			{
				taskDefinition.height += 18f;
				ClassProperty.MapQueue();
				GUILayout.TextArea(ConsumerAlgo.CallDefinition().u_announcement, ClassProperty.CalcError().m_GlobalProcessor);
				EditorGUILayout.Space();
				using (new GUILayout.HorizontalScope())
				{
					if (!string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_announcementLink) && ClassProperty.CountQueue(ConsumerAlgo.CallDefinition().u_announcementLinkName, EditorStyles.toolbarButton))
					{
						Application.OpenURL(ConsumerAlgo.CallDefinition().u_announcementLink);
					}
					if (listenerAnnotation && ClassProperty.CountQueue("Hide", EditorStyles.toolbarButton))
					{
						ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition(excludeparam: true);
						ConsumerAlgo.CallDefinition().u_announcementHiddenDate.ResolveDefinition(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
					}
				}
			}, RestartVisitor);
			if (ClassProperty.DefineQueue(taskDefinition))
			{
				_AccountAnnotation.target = !_AccountAnnotation.target;
			}
		}
	}

	[InitializeOnLoadMethod]
	private static void InstantiateVisitor()
	{
		if (!RestartInitializer())
		{
			ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: false);
		}
		else if (!string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_updateVersion.CollectDefinition()))
		{
			ConnectVisitor(setasset: false);
			return;
		}
		ClassProperty.CountRules(delegate
		{
			ResetVisitor(removereference: false);
		});
	}

	private static void AwakeVisitor()
	{
		ResetVisitor(removereference: true);
	}

	private static void ResetVisitor(bool removereference)
	{
		if ((!removereference && RestartInitializer()) || _RuleAnnotation || _PrototypeAnnotation)
		{
			return;
		}
		_PrototypeAnnotation = true;
		DisableVisitor(CallVisitor(new List<(string, string)>
		{
			("command", "getdownloadinfo"),
			("product_id", "yOk0XCnENLMO6DIF8cYpSg=="),
			("version", m_RefAnnotation.ToString())
		})).QueryRules(delegate(FieldAlgo response)
		{
			_RuleAnnotation = true;
			string text = ConsumerAlgo.CallDefinition().u_announcement.CollectDefinition();
			using (new ConsumerAlgo.BridgeAlgo())
			{
				ConsumerAlgo.CallDefinition().u_updateLink.ResolveDefinition(response.InsertReg("download_link"));
				ConsumerAlgo.CallDefinition().u_updateMessage.ResolveDefinition(response.InsertReg("download_message"));
				ConsumerAlgo.CallDefinition().u_updateChangelog.ResolveDefinition(response.InsertReg("changelog_link"));
				ConsumerAlgo.CallDefinition().u_updateVersion.ResolveDefinition(response.InsertReg("version"));
				ConsumerAlgo.CallDefinition().u_updateDay.ResolveDefinition(PatchAnnotation());
				ConsumerAlgo.CallDefinition().u_announcement.ResolveDefinition(response.InsertReg("announcement"));
				if (!string.IsNullOrWhiteSpace(ConsumerAlgo.CallDefinition().u_announcement))
				{
					ConsumerAlgo.CallDefinition().u_announcement.ResolveDefinition(ConsumerAlgo.CallDefinition().u_announcement.CollectDefinition().Replace("\\\\n", "\n").Replace("\\n", "\n"));
				}
				ConsumerAlgo.CallDefinition().u_announcementLink.ResolveDefinition(response.InsertReg("announcement_link"));
				ConsumerAlgo.CallDefinition().u_announcementLinkName.ResolveDefinition(response.InsertReg("announcement_link_name"));
			}
			if (text != ConsumerAlgo.CallDefinition().u_announcement.CollectDefinition())
			{
				ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition(excludeparam: false);
			}
			ConnectVisitor(removereference);
		}, delegate(System.Exception exc)
		{
			FindVisitor($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}, null, null, delegate
		{
			_PrototypeAnnotation = false;
			InsertVisitor();
		});
	}

	private static void FlushVisitor()
	{
		_IssuerAnnotation = true;
		UnityWebRequest m_ProducerDefinition = new UnityWebRequest(ConsumerAlgo.CallDefinition().u_updateLink);
		m_ProducerDefinition.downloadHandler = new DownloadHandlerFile("Assets/Controller Editor.unitypackage");
		m_ProducerDefinition.SendWebRequest().completed += delegate
		{
			_IssuerAnnotation = false;
			string text = "Assets/Controller Editor.unitypackage";
			if (m_ProducerDefinition.isNetworkError || m_ProducerDefinition.isHttpError)
			{
				AssetDatabase.ImportAsset(text);
				AssetDatabase.DeleteAsset(text);
				m_ProducerDefinition.Dispose();
				throw new System.Exception(m_ProducerDefinition.error);
			}
			AssetDatabase.ImportPackage(text, interactive: true);
			AssetDatabase.DeleteAsset(text);
			m_ProducerDefinition.Dispose();
		};
	}

	private static void ConnectVisitor(bool setasset)
	{
		if ((bool)ConsumerAlgo.CallDefinition().u_announcementHidden)
		{
			if (DateTime.TryParse(ConsumerAlgo.CallDefinition().u_announcementHiddenDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
			{
				ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition((DateTime.UtcNow - result).TotalDays < 7.0);
			}
			else
			{
				ConsumerAlgo.CallDefinition().u_announcementHidden.ExcludeDefinition(excludeparam: false);
			}
		}
		if (!(m_RefAnnotation < new SingletonServer(ConsumerAlgo.CallDefinition().u_updateVersion.CollectDefinition())))
		{
			if (setasset)
			{
				FindVisitor("Up to date!");
				Task.Run(async delegate
				{
					await Task.Delay(3000);
					ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: true);
					InsertVisitor();
				});
			}
			else
			{
				ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: true);
			}
			return;
		}
		singletonAnnotation = true;
		if (setasset)
		{
			ConsumerAlgo.CallDefinition().u_updateHidden.ExcludeDefinition(excludeparam: false);
			factoryAnnotation.target = true;
		}
		if (!ConsumerAlgo.CallDefinition().u_updateHidden)
		{
			FindVisitor($"Update Available! <b>(v{ConsumerAlgo.CallDefinition().u_updateVersion})</b>");
		}
	}

	private static void CalculateVisitor(Rect spec)
	{
		EditorGUI.DisabledScope disabledScope;
		if (!(_ProcessorAnnotation.itemTests == null))
		{
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
				return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
			})())
			{
				return;
			}
			Rect item = new Rect(spec);
			item.width = 18f;
			using (new EditorGUI.DisabledScope(_ProcessorAnnotation.itemTests.conditions.Length == 0))
			{
				if (ClassProperty.QueryQueue(item, ClassProperty.DestroyError().m_BridgeProcessor, GUI.skin.label))
				{
					SortVisitor();
				}
			}
			item.x += 20f;
			disabledScope = new EditorGUI.DisabledScope(m_ThreadAnnotation.Count == 0);
			try
			{
				if (ClassProperty.QueryQueue(item, ClassProperty.DestroyError().m_StrategyProcessor, GUI.skin.label))
				{
					RegisterVisitor();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			item.x += 20f;
			item.width = spec.width - 40f;
			GUI.Label(item, _ProcessorAnnotation.VisitSerializer() + "'s Conditions");
		}
		else
		{
			using (new AnnotationPolicy(iskey: false))
			{
				Rect item2 = new Rect(spec);
				item2.width = 16f;
				item2.x = spec.x - 3f;
				item2.y = spec.y + 2f;
				if (ClassProperty.QueryQueue(item2, ClassProperty.DestroyError().dispatcherProcessor, GUIStyle.none))
				{
					specification = !specification;
					MapVisitor();
				}
			}
			spec.x += 12f;
			GUI.Label(spec, (!specification) ? "All Conditions" : "Shared Conditions");
			spec.x -= 12f;
		}
		Rect item3 = new Rect(spec);
		item3.x += 95f;
		if (specification)
		{
			item3.x += 29f;
		}
		item3.width = 18f;
		if (!_ProcessorAnnotation.itemTests)
		{
			disabledScope = new EditorGUI.DisabledScope((specification && _TokenAnnotation.Count == 0) || (!specification && m_CodeAnnotation.Count == 0));
			try
			{
				if (ClassProperty.QueryQueue(item3, ClassProperty.DestroyError().m_BridgeProcessor, GUI.skin.label))
				{
					SortVisitor();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			item3.x += 20f;
			using (new EditorGUI.DisabledScope(m_ThreadAnnotation.Count == 0))
			{
				if (ClassProperty.QueryQueue(item3, ClassProperty.DestroyError().m_StrategyProcessor, GUI.skin.label))
				{
					RegisterVisitor();
				}
			}
		}
		else
		{
			item3.x += 20f;
		}
		item3 = new Rect(spec);
		item3.y += 2f;
		item3.x += spec.width / 2f + spec.width / 8f - 25f;
		item3.width = 15f;
		EditorGUI.BeginDisabledGroup((!_ProcessorAnnotation.itemTests && ((specification && _TokenAnnotation.Count == 0) || (!specification && m_CodeAnnotation.Count == 0))) || (_ProcessorAnnotation.itemTests != null && _ProcessorAnnotation.itemTests.conditions.Length < 1));
		if (ClassProperty.QueryQueue(item3, ClassProperty.DestroyError().m_ExporterProcessor, GUIStyle.none))
		{
			if (!(_ProcessorAnnotation.itemTests != null))
			{
				if (!specification)
				{
					foreach (GlobalVisitor item4 in m_CodeAnnotation)
					{
						item4.ForgotInitializer();
					}
					UpdateVisitor();
				}
				else
				{
					foreach (GlobalVisitor item5 in _TokenAnnotation)
					{
						item5.ForgotInitializer();
					}
					ChangeVisitor();
				}
			}
			else
			{
				foreach (GlobalVisitor item6 in m_DicAnnotation)
				{
					AnimatorCondition animatorCondition = ResolveAlgo(item6.m_ProcessVisitor);
					item6.ResolveInitializer(animatorCondition);
					item6.m_ProcessVisitor = animatorCondition;
				}
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
					return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
				})())
				{
					return;
				}
				ChangeVisitor();
				_TokenAnnotation = AssetVisitor(propertyAnnotation);
			}
		}
		EditorGUI.EndDisabledGroup();
		spec.x += spec.width - 52f;
		spec.width = 15f;
		using (new AnnotationPolicy(iskey: false))
		{
			if (ClassProperty.QueryQueue(spec, new GUIContent(ClassProperty.DestroyError()._RequestProcessor)
			{
				tooltip = "Toggles custom matching options"
			}, GUIStyle.none))
			{
				ConsumerAlgo.CallDefinition().showMatchingOptions.InsertDefinition();
			}
		}
		spec.x += 17f;
		using (new EditorGUI.DisabledScope(_ProcessorAnnotation.itemTests))
		{
			if (ClassProperty.QueryQueue(spec, ClassProperty.DestroyError()._AttrProcessor, GUIStyle.none))
			{
				ConnectAlgo();
			}
		}
		spec.x += 17f;
		if (ClassProperty.QueryQueue(spec, ClassProperty.DestroyError()._IdentifierProcessor, GUIStyle.none))
		{
			ViewAlgo();
		}
	}

	private static void TestVisitor(Rect def, int boffset, bool isconsumer, bool dotoken2)
	{
		_003C_003Ec__DisplayClass285_0 CS_0024_003C_003E8__locals43 = new _003C_003Ec__DisplayClass285_0();
		if (!LogoutMapper())
		{
			return;
		}
		List<GlobalVisitor> list = StopVisitor();
		if (boffset >= list.Count || boffset < 0)
		{
			return;
		}
		GlobalVisitor globalVisitor = list[boffset];
		CS_0024_003C_003E8__locals43.m_ConfigurationDefinition = globalVisitor.m_ProcessVisitor;
		int second;
		UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter, out second);
		bool flag = false;
		bool flag2 = false;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			return;
		}
		bool num = animatorControllerParameter == null;
		CS_0024_003C_003E8__locals43.publisherDefinition = $"ConditionParameterField{boffset}";
		CS_0024_003C_003E8__locals43._IteratorDefinition = GUI.GetNameOfFocusedControl() == CS_0024_003C_003E8__locals43.publisherDefinition;
		Rect source = new Rect(def.width - 22f, def.y + 2f, 32f, 18f);
		Rect source2 = ((!num) ? Rect.zero : new Rect(source)
		{
			width = 60f,
			x = source.x - 60f
		});
		Rect def2 = new Rect(def.x, def.y + 2f, def.width - source2.width - 40f, EditorGUIUtility.singleLineHeight);
		if (num)
		{
			Rect rect = def2.SortResolver(50f);
			Rect rect2 = new Rect(rect);
			rect2.x = rect.x + 3f;
			rect2.width = rect.width - 3f;
			Rect rect3 = rect2;
			def2.SortResolver(3f, isfield: true);
			Rect rect4 = def2.SortResolver(100f);
			CS_0024_003C_003E8__locals43.CompareServer(rect);
			if (!CS_0024_003C_003E8__locals43._IteratorDefinition)
			{
				using (new ManagerThread(globalVisitor.iteratorVisitor[0]))
				{
					EditorGUI.BeginChangeCheck();
					int firstsize = EditorGUI.Popup(rect, -1, _Schema);
					if (EditorGUI.EndChangeCheck())
					{
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = TestAnnotation(firstsize);
						CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
					}
				}
				GUI.Label(rect3, CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter);
			}
			GUI.Label(rect4, "Parameter not found in Controller!");
			int num2 = -1;
			EditorGUI.BeginChangeCheck();
			num2 = (int)(UnityEngine.AnimatorControllerParameterType)(object)EditorGUI.EnumPopup(source2, (UnityEngine.AnimatorControllerParameterType)(-1));
			rect2 = new Rect(source2);
			rect2.x = source2.x + 3f;
			GUI.Label(rect2, "Add");
			if (EditorGUI.EndChangeCheck())
			{
				string text = ((!string.IsNullOrEmpty(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter)) ? CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter : "New Parameter");
				LogoutMapper().AddParameter(text, (UnityEngine.AnimatorControllerParameterType)num2);
				ArrayUtility.Add(ref _Schema, text);
				CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = text;
				CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
			}
		}
		else
		{
			Rect rect5 = def2.SortResolver(20f, isfield: true);
			Rect rect6 = def2.SortResolver((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger) ? 50 : 100);
			Rect rect7 = new Rect(rect6)
			{
				width = 20f,
				x = rect6.x + rect6.width - 40f
			};
			Rect rect8 = def2.SortResolver((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool) ? 50 : 100);
			Rect rect9 = def2.SortResolver(100f);
			if (GUI.Button(rect5, ClassProperty.DestroyError().m_ValProcessor, ClassProperty.CalcError().configProcessor))
			{
				IEnumerable<IEnumerable<AnimatorStateTransition>> first = RevertMapper().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				List<AnimatorTransitionBase> mapperReg = new List<AnimatorTransitionBase>();
				first.InvokeResolver(delegate(IEnumerable<AnimatorStateTransition> e)
				{
					mapperReg.AddRange(e);
				});
				mapperReg.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				mapperReg.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				Selection.objects = Selection.objects.Concat(mapperReg).Distinct().ToArray();
			}
			if (GUI.Button(rect7, GUIContent.none, GUIStyle.none))
			{
				string initializerReg = CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter;
				IEnumerable<IEnumerable<AnimatorStateTransition>> first2 = RevertMapper().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				List<AnimatorTransitionBase> m_DefinitionReg = new List<AnimatorTransitionBase>();
				first2.InvokeResolver(delegate(IEnumerable<AnimatorStateTransition> e)
				{
					m_DefinitionReg.AddRange(e);
				});
				m_DefinitionReg.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				m_DefinitionReg.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				Selection.objects = Selection.objects.Concat(m_DefinitionReg).Distinct().ToArray();
			}
			CS_0024_003C_003E8__locals43.CompareServer(rect6);
			if (!CS_0024_003C_003E8__locals43.m_ProcDefinition && !CS_0024_003C_003E8__locals43._IteratorDefinition)
			{
				using (new ManagerThread(globalVisitor.iteratorVisitor[0]))
				{
					if ((bool)ConsumerAlgo.CallDefinition().useLegacyDropdown)
					{
						EditorGUI.BeginChangeCheck();
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = TestAnnotation(EditorGUI.Popup(rect6, second, _Schema));
						if (EditorGUI.EndChangeCheck())
						{
							CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
						}
					}
					else
					{
						object[] parameters = new object[3] { rect6, second, _Schema };
						int num3 = (int)_ManagerVisitor.Invoke(null, parameters);
						if (num3 != second)
						{
							CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = TestAnnotation(num3);
							CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
						}
					}
				}
			}
			GUI.Label(rect7, ClassProperty.DestroyError().m_ValProcessor, GUIStyle.none);
			if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				using (new ManagerThread(globalVisitor.iteratorVisitor[1]))
				{
					EditorGUI.BeginChangeCheck();
					Enum selected;
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Bool)
						{
							selected = (BoolModes)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode;
							selected = EditorGUI.EnumPopup(rect8, selected);
						}
						else
						{
							selected = (FloatModes)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode;
							selected = EditorGUI.EnumPopup(rect8, selected);
						}
					}
					else
					{
						selected = (IntModes)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode;
						selected = EditorGUI.EnumPopup(rect8, selected);
					}
					if (EditorGUI.EndChangeCheck())
					{
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode = (AnimatorConditionMode)(object)selected;
						flag = true;
					}
				}
				using (new ManagerThread(globalVisitor.iteratorVisitor[2]))
				{
					EditorGUI.BeginChangeCheck();
					UnityEngine.AnimatorControllerParameterType type = animatorControllerParameter.type;
					if (type == UnityEngine.AnimatorControllerParameterType.Float)
					{
						GUI.SetNextControlName("Threshold" + _Attribute);
						_Attribute++;
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold = EditorGUI.FloatField(rect9, CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold);
					}
					else if (type == UnityEngine.AnimatorControllerParameterType.Int)
					{
						GUI.SetNextControlName("Threshold" + _Attribute);
						_Attribute++;
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold = EditorGUI.IntField(rect9, (int)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold);
					}
					if (EditorGUI.EndChangeCheck())
					{
						flag2 = true;
					}
				}
			}
			if (animatorControllerParameter.type <= UnityEngine.AnimatorControllerParameterType.Int || CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode <= AnimatorConditionMode.IfNot)
			{
				UnityEngine.AnimatorControllerParameterType type = animatorControllerParameter.type;
				if (type != UnityEngine.AnimatorControllerParameterType.Float)
				{
					if (type == UnityEngine.AnimatorControllerParameterType.Int && CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode < AnimatorConditionMode.Greater)
					{
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode = AnimatorConditionMode.Equals;
						flag = true;
					}
				}
				else if (((int)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode).GetResolver(3, 5))
				{
					CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode = AnimatorConditionMode.Greater;
					flag = true;
				}
			}
			else
			{
				CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode = AnimatorConditionMode.If;
				flag = true;
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			return;
		}
		if (CS_0024_003C_003E8__locals43.m_ProcDefinition)
		{
			globalVisitor.VerifyInitializer(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter);
		}
		if (flag)
		{
			globalVisitor.FillInitializer(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode);
		}
		if (flag2)
		{
			globalVisitor.WriteInitializer(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold);
		}
		if (!GUI.Button(source, ClassProperty.DestroyError()._TagProcessor, ClassProperty.CalcError()._TemplateProcessor))
		{
			return;
		}
		globalVisitor.StopInitializer();
		if (_ProcessorAnnotation.itemTests == null)
		{
			if (!specification)
			{
				m_CodeAnnotation.RemoveAt(boffset);
				MapVisitor();
			}
			else
			{
				UpdateVisitor();
			}
		}
	}

	private static void MapVisitor()
	{
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			return;
		}
		if (!(_ProcessorAnnotation.itemTests != null))
		{
			if (!specification)
			{
				_RoleAnnotation = new ReorderableList(m_CodeAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, _DefinitionAnnotation.Count == 1, displayRemoveButton: false)
				{
					drawElementCallback = TestVisitor,
					drawHeaderCallback = CalculateVisitor,
					onAddCallback = FillVisitor
				};
			}
			else
			{
				invocationAnnotation = new ReorderableList(_TokenAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
				{
					drawElementCallback = TestVisitor,
					drawHeaderCallback = CalculateVisitor,
					onAddCallback = FillVisitor
				};
			}
		}
		else
		{
			m_DicAnnotation = PrepareVisitor(_ProcessorAnnotation.itemTests);
			m_ParamAnnotation = new ReorderableList(m_DicAnnotation, typeof(GlobalVisitor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = TestVisitor,
				drawHeaderCallback = CalculateVisitor,
				onAddCallback = FillVisitor
			};
		}
	}

	private void ValidateVisitor()
	{
		if (!ConsumerAlgo.CallDefinition().editingController)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(LogoutMapper() == null))
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
					return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
				})())
				{
					return;
				}
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				using (new GUILayout.HorizontalScope())
				{
					m_GlobalAnnotation = (ControllerAction)(object)EditorGUILayout.EnumPopup(m_GlobalAnnotation, GUILayout.Width(ClassProperty.PostQueue(m_GlobalAnnotation) + 28f));
					switch (m_GlobalAnnotation)
					{
					case ControllerAction.RemoveTag:
						flag = true;
						flag4 = true;
						break;
					case ControllerAction.TagCurrentLayerWith:
						flag3 = true;
						break;
					case ControllerAction.RemoveLayersWithTag:
						flag3 = true;
						break;
					case ControllerAction.RemoveParameter:
						flag = true;
						flag4 = true;
						break;
					case ControllerAction.ReplaceParameter:
						flag = true;
						flag2 = true;
						flag4 = true;
						break;
					}
					if (flag4 && _TaskAnnotation == ActionMode.LayersTaggedWith)
					{
						flag3 = true;
					}
					if (flag)
					{
						EditorGUIUtility.labelWidth = 40f;
						_SchemaAnnotation = EditorGUILayout.TextField("", _SchemaAnnotation, "textfielddropdowntext");
						EditorGUIUtility.labelWidth = 0f;
						int num = -1;
						EditorGUI.BeginChangeCheck();
						num = EditorGUILayout.Popup(-1, _Schema ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
						if (EditorGUI.EndChangeCheck())
						{
							_SchemaAnnotation = _Schema[num];
						}
					}
					if (flag3 && _TaskAnnotation != ActionMode.LayersTaggedWith)
					{
						proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
					}
					if (flag2)
					{
						GUILayout.Label("With", GUILayout.Width(32f));
						EditorGUIUtility.labelWidth = 40f;
						m_BroadcasterAnnotation = EditorGUILayout.TextField("", m_BroadcasterAnnotation, "textfielddropdowntext");
						EditorGUIUtility.labelWidth = 0f;
						int num2 = -1;
						EditorGUI.BeginChangeCheck();
						num2 = EditorGUILayout.Popup(-1, _Schema ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
						if (EditorGUI.EndChangeCheck())
						{
							m_BroadcasterAnnotation = _Schema[num2];
						}
					}
					if (flag4)
					{
						GUILayout.Label("In", GUILayout.Width(15f));
						_TaskAnnotation = (ActionMode)(object)EditorGUILayout.EnumPopup(_TaskAnnotation, GUILayout.Width(140f));
					}
					if (flag3 && _TaskAnnotation == ActionMode.LayersTaggedWith)
					{
						proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
					}
					if (m_GlobalAnnotation == ControllerAction.Copy)
					{
						m_ProcessAnnotation = (MoveMode)(object)EditorGUILayout.EnumPopup(m_ProcessAnnotation, GUILayout.Width(ClassProperty.PostQueue(m_ProcessAnnotation) + 28f));
						if (m_ProcessAnnotation == MoveMode.LayersTaggedWith)
						{
							flag3 = true;
							proxyAnnotation = EditorGUILayout.TextField(proxyAnnotation);
						}
						GUILayout.Label("To", GUILayout.Width(20f));
						producerAnnotation = (MoveDestination)(object)EditorGUILayout.EnumPopup(producerAnnotation, GUILayout.Width(ClassProperty.PostQueue(producerAnnotation) + 28f));
						if (producerAnnotation == MoveDestination.Controller)
						{
							m_MethodAnnotation = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(m_MethodAnnotation, typeof(UnityEditor.Animations.AnimatorController), false);
						}
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					if (m_GlobalAnnotation == ControllerAction.RemoveParameter || m_GlobalAnnotation == ControllerAction.ReplaceParameter)
					{
						structAnnotation = EditorGUILayout.Toggle(new GUIContent("Match Whole Word", "Apply to parameters that match exactly. Otherwise apply to parameters that contain it"), structAnnotation);
					}
					else if (m_GlobalAnnotation == ControllerAction.Copy)
					{
						serviceAnnotation = EditorGUILayout.Toggle(new GUIContent("Add Required Parameters", "Add the parameters used by the Source to the destination Controller. Adds Suffix if Suffix isn't empty."), serviceAnnotation, GUILayout.Width(180f));
						GUILayout.FlexibleSpace();
						EditorGUIUtility.labelWidth = 50f;
						_StateAnnotation = EditorGUILayout.TextField(new GUIContent("Suffix:", "Add a Suffix to all the Parameters in the newly copied layers. Adds a Suffix to the added parameters if enabled."), _StateAnnotation);
						EditorGUIUtility.labelWidth = 0f;
					}
					else
					{
						GUILayout.FlexibleSpace();
					}
					EditorGUI.BeginDisabledGroup((string.IsNullOrEmpty(_SchemaAnnotation) && flag) || (string.IsNullOrEmpty(m_BroadcasterAnnotation) && flag2) || (string.IsNullOrEmpty(proxyAnnotation) && flag3) || (m_GlobalAnnotation == ControllerAction.Copy && producerAnnotation == MoveDestination.Controller && !m_MethodAnnotation));
					if (ClassProperty.CountQueue("Apply", "minibutton", GUILayout.Width(140f)))
					{
						LogoutVisitor();
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.Space();
				ClassProperty.MapQueue();
				EditorGUILayout.Space();
				CustomizeVisitor();
			}
		}
	}

	private void CustomizeVisitor()
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUI.BeginChangeCheck();
			_Client = ClassProperty.AddQueue(_Client, "Write Defaults", "toolbarbutton");
			if (EditorGUI.EndChangeCheck())
			{
				WriteMapper();
			}
			EditorGUI.BeginChangeCheck();
			m_Config = ClassProperty.AddQueue(m_Config, "Explore Controller Sub-Assets", "toolbarbutton");
			if (EditorGUI.EndChangeCheck())
			{
				FillMapper();
			}
			if (ClassProperty.RestartQueue(new GUIContent("Cleanup unused Sub-Assets", "Some Controllers have residue in their Sub-Assets that may be unused, may happen when using this tool. Use this button to clean it up."), "toolbarbutton") && (bool)LogoutMapper())
			{
				SearchVisitor(LogoutMapper());
				VerifyMapper();
			}
		}
		if (!m_Config)
		{
			if (!_Client)
			{
				return;
			}
			if (!LogoutMapper())
			{
				VerifyMapper();
			}
			using (new GUILayout.HorizontalScope())
			{
				using (new GUILayout.VerticalScope())
				{
					if (ClassProperty.DisableQueue("Set All On"))
					{
						ForgotMapper(istask: true);
					}
					EditorGUILayout.Space();
					ReflectAnnotation("Write Defaults On");
					foreach (AnimatorState item in _Service.m_ServiceVisitor.Where((AnimatorState s) => (bool)s && s.writeDefaultValues))
					{
						using (new GUILayout.HorizontalScope())
						{
							CreateAnnotation(item, GUILayout.ExpandWidth(expand: true));
							if (ClassProperty.DisableQueue(">", GUILayout.ExpandWidth(expand: false)))
							{
								StopMapper(item, connectionreguired: false);
							}
						}
					}
				}
				LoginAnnotation();
				using (new GUILayout.VerticalScope())
				{
					if (ClassProperty.DisableQueue("Set All Off"))
					{
						ForgotMapper(istask: false);
					}
					EditorGUILayout.Space();
					ReflectAnnotation("Write Defaults Off");
					foreach (AnimatorState item2 in _Service.m_ServiceVisitor.Where((AnimatorState s) => (bool)s && !s.writeDefaultValues))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (ClassProperty.DisableQueue("<", GUILayout.ExpandWidth(expand: false)))
							{
								StopMapper(item2, connectionreguired: true);
							}
							CreateAnnotation(item2, GUILayout.ExpandWidth(expand: true));
						}
					}
					return;
				}
			}
		}
		_ModelAnnotation = GUILayout.Toolbar(_ModelAnnotation, ClassProperty.DestroyError().m_FacadeProcessor, EditorStyles.toolbarButton);
		switch (_ModelAnnotation)
		{
		case 5:
			_Service.stateVisitor.ForEach(RateVisitor);
			break;
		case 2:
			_Service.structVisitor.ForEach(RateVisitor);
			break;
		case 3:
			_Service._SchemaVisitor.ForEach(RateVisitor);
			break;
		case 1:
			_Service.m_ServiceVisitor.ForEach(RateVisitor);
			break;
		case 4:
			_Service.broadcasterVisitor.ForEach(RateVisitor);
			break;
		case 0:
			_Service._ProxyVisitor.ForEach(RateVisitor);
			break;
		}
	}

	private static void RateVisitor(UnityEngine.Object task)
	{
		if (!task)
		{
			_ModelAnnotation = 0;
			EditorWindow.GetWindow<ControllerEditor>().Repaint();
			return;
		}
		bool flag = Selection.activeObject == task;
		using (new GUILayout.HorizontalScope())
		{
			string text = ((!string.IsNullOrEmpty(task.name)) ? task.name : task.GetType().Name);
			if (ClassProperty.CountQueue("- " + text, (!flag) ? GUI.skin.label : ClassProperty.CalcError().m_MockProcessor))
			{
				Selection.activeObject = (flag ? null : task);
			}
			GUILayout.FlexibleSpace();
			if (ClassProperty.RestartQueue((!task.hideFlags.HasFlag(HideFlags.HideInHierarchy)) ? ClassProperty.DestroyError()._RefProcessor : ClassProperty.DestroyError().m_StatusProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(14f), GUILayout.Height(18f)))
			{
				Undo.RecordObject(task, "Toggle Sub-Asset Visibility");
				task.hideFlags ^= HideFlags.HideInHierarchy;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(task), ImportAssetOptions.ForceUpdate);
			}
			if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor) && EditorUtility.DisplayDialog("Delete", "Delete " + task.name + "?\nUse cautiously! May result in unintended behavior!", "Ok", "Cancel"))
			{
				Undo.RecordObject(task, "Remove SubAsset");
				AssetDatabase.RemoveObjectFromAsset(task);
				Undo.DestroyObjectImmediate(task);
			}
		}
	}

	private static void DestroyVisitor()
	{
		if (!_Descriptor)
		{
			return;
		}
		int num = m_AlgoAnnotation.Count + (m_Template ? 1 : 0) + (m_Message ? 1 : 0) + (collection ? 1 : 0);
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			string token = $"State Count: {num}";
			DeleteAnnotation(GetVisitor, token, ConsumerAlgo.CallDefinition().showStateCount, iscont2: true, 3);
			DeleteAnnotation(CalcVisitor, "State Settings", ConsumerAlgo.CallDefinition().showStateSettings, iscont2: true, 4);
			if (WorkerProperty.InvokePage())
			{
				DeleteAnnotation(RunVisitor, "VRC Parameter Drivers", ConsumerAlgo.CallDefinition().showVRCDrivers, iscont2: false, 5);
				DeleteAnnotation(CloneVisitor, "VRC Tracking Control", ConsumerAlgo.CallDefinition().showVRCTracking, iscont2: false, 6);
			}
		}
	}

	private static void GetVisitor()
	{
		int num = m_AlgoAnnotation.Count + (m_Template ? 1 : 0) + (m_Message ? 1 : 0) + (collection ? 1 : 0);
		if (num <= 0)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Selected 0 States", ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.Space();
			GUILayout.Space(37f);
		}
		else
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(5f);
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] objects = Selection.objects;
					objects = Selection.objects.Except(RevertMapper().states.Select((ChildAnimatorState c) => c.state)).ToArray();
					for (int num2 = objects.Length - 1; num2 >= 0; num2--)
					{
						Type type = objects[num2].GetType();
						if (type == SetterTests.ConnectionTests.m_FilterTests || type == SetterTests.ConnectionTests._WorkerTests || type == SetterTests.ConnectionTests._SystemTests)
						{
							ArrayUtility.RemoveAt(ref objects, num2);
						}
					}
					Selection.objects = objects;
				}
				GUILayout.Space(18f);
				GUILayout.FlexibleSpace();
				if (ClassProperty.DisableQueue("Out", GUILayout.Width(34f)))
				{
					GetAlgo();
				}
				GUILayout.Label("Selected " + num + " States", ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				if (ClassProperty.DisableQueue("In", GUILayout.Width(34f)))
				{
					CalcAlgo();
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
			EditorGUILayout.Space();
		}
		foreach (SetterTests.TokenizerTests item in m_MapperAnnotation.Where((SetterTests.TokenizerTests n) => n.objectTests == SetterTests.TokenizerTests.NodeType.state))
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array = Selection.objects;
					ArrayUtility.Remove(ref array, item.comparatorTests);
					Selection.objects = array;
				}
				using (new EditorGUI.DisabledScope(!item.comparatorTests.motion))
				{
					if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_RegistryProcessor, ClassProperty.CalcError().configProcessor, GUILayout.Width(17f)))
					{
						EditorGUIUtility.PingObject(item.comparatorTests.motion);
					}
				}
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(item.comparatorTests.transitions.Length < 1))
				{
					if (ClassProperty.DisableQueue("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(item.RegisterPolicy()).ToArray();
					}
				}
				GUILayout.Label(RunAnnotation(item.comparatorTests.name, 18), ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(!item.ChangePolicy().Any()))
				{
					if (ClassProperty.DisableQueue("In", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(item.ChangePolicy()).ToArray();
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (m_Template)
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array2 = Selection.objects;
					for (int num3 = 0; num3 < array2.Length; num3++)
					{
						if (array2[num3].GetType() == SetterTests.ConnectionTests._WorkerTests)
						{
							ArrayUtility.RemoveAt(ref array2, num3);
							m_Template = false;
							break;
						}
					}
					Selection.objects = array2;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(!RevertMapper() || RevertMapper().anyStateTransitions.Length < 1))
				{
					if (ClassProperty.DisableQueue("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(ManageMapper().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
						{
							_003C_003Ec__DisplayClass308_0 _003C_003Ec__DisplayClass308_ = new _003C_003Ec__DisplayClass308_0();
							_003C_003Ec__DisplayClass308_.serverReg = t;
							return RevertMapper().states.Any(_003C_003Ec__DisplayClass308_.InvokeServer);
						})).ToArray();
					}
				}
				GUILayout.Label("Any State", ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					ClassProperty.DisableQueue("In", GUILayout.Width(34f));
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (m_Message)
		{
			using (new GUILayout.HorizontalScope("box"))
			{
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array3 = Selection.objects;
					for (int num4 = 0; num4 < array3.Length; num4++)
					{
						if (array3[num4].GetType() == SetterTests.ConnectionTests._SystemTests)
						{
							ArrayUtility.RemoveAt(ref array3, num4);
							m_Message = false;
							break;
						}
					}
					Selection.objects = array3;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(!RevertMapper() || RevertMapper().entryTransitions.Length == 0))
				{
					if (ClassProperty.DisableQueue("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(RevertMapper().entryTransitions).ToArray();
					}
				}
				GUILayout.Label("Entry", ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					ClassProperty.DisableQueue("In", GUILayout.Width(34f));
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (collection)
		{
			using (new GUILayout.HorizontalScope("box"))
			{
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] objects2 = Selection.objects;
					for (int num5 = 0; num5 < objects2.Length; num5++)
					{
						if (objects2[num5].GetType() == SetterTests.ConnectionTests.m_FilterTests)
						{
							objects2[num5] = null;
							collection = false;
							break;
						}
					}
					Selection.objects = objects2;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(disabled: true);
				ClassProperty.DisableQueue("Out", GUILayout.Width(30f));
				EditorGUI.EndDisabledGroup();
				GUILayout.Label("Exit", ClassProperty.CalcError()._StructProcessor, GUILayout.Width(140f));
				EditorGUI.BeginDisabledGroup(regAnnotation.GetRules());
				if (ClassProperty.DisableQueue("In", GUILayout.Width(30f)))
				{
					Selection.objects = Selection.objects.Concat(regAnnotation).ToArray();
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUI.BeginDisabledGroup(num < 2);
		using (new GUILayout.HorizontalScope())
		{
			if (ClassProperty.CountQueue("Align Vertical", "toolbarbutton"))
			{
				CountAlgo();
			}
			if (ClassProperty.CountQueue("Align Horizontal", "toolbarbutton"))
			{
				DisableAlgo();
			}
		}
		EditorGUI.EndDisabledGroup();
		EditorGUI.BeginDisabledGroup(num < 1);
		using (new GUILayout.HorizontalScope())
		{
			if (ClassProperty.CountQueue("Up 0.25", "toolbarbutton"))
			{
				ChildAnimatorState[] states = RevertMapper().states;
				for (int num6 = 0; num6 < states.Length; num6++)
				{
					if (m_AlgoAnnotation.Contains(states[num6].state))
					{
						states[num6].position += Vector3.down * 3f;
					}
				}
				if (m_Message)
				{
					RevertMapper().entryPosition += Vector3.down * 3f;
					InterruptAlgo(wantfirst: false);
				}
				if (m_Template)
				{
					RevertMapper().anyStatePosition += Vector3.down * 3f;
				}
				if (collection)
				{
					RevertMapper().exitPosition += Vector3.down * 3f;
				}
				RevertMapper().states = states;
				EditorUtility.SetDirty(LogoutMapper());
				if (m_Message || collection)
				{
					InterruptAlgo(wantfirst: false);
				}
			}
			if (ClassProperty.CountQueue("Right 0.25", "toolbarbutton"))
			{
				ChildAnimatorState[] states2 = RevertMapper().states;
				for (int num7 = 0; num7 < states2.Length; num7++)
				{
					if (m_AlgoAnnotation.Contains(states2[num7].state))
					{
						states2[num7].position += Vector3.right * 3f;
					}
				}
				if (m_Message)
				{
					RevertMapper().entryPosition += Vector3.right * 3f;
				}
				if (m_Template)
				{
					RevertMapper().anyStatePosition += Vector3.right * 3f;
				}
				if (collection)
				{
					RevertMapper().exitPosition += Vector3.right * 3f;
				}
				RevertMapper().states = states2;
				EditorUtility.SetDirty(LogoutMapper());
				if (m_Message || collection)
				{
					InterruptAlgo(wantfirst: false);
				}
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	private static void CalcVisitor()
	{
		if (m_AlgoAnnotation.Count >= 1)
		{
			_Publisher.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ClassProperty.DestroyError()._DatabaseProcessor, GUILayout.Width(35f), GUILayout.Height(35f));
				using (new GUILayout.VerticalScope())
				{
					using (new ManagerThread(m_TokenizerAnnotation))
					{
						EditorGUI.BeginChangeCheck();
						string serv = EditorGUILayout.DelayedTextField(string.Empty, m_TokenizerAnnotation.stringValue);
						if (EditorGUI.EndChangeCheck())
						{
							RestartAlgo(RevertMapper(), m_AlgoAnnotation, serv);
						}
					}
					EditorGUIUtility.labelWidth = 35f;
					EditorGUILayout.PropertyField(decoratorAnnotation);
					EditorGUIUtility.labelWidth = 0f;
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_ComparatorAnnotation);
			EditorGUILayout.PropertyField(m_ExceptionAnnotation);
			PrepareMapper("Multiplier", null, objectAnnotation, m_ReponseAnnotation, _Proxy, res4stop: true);
			PrepareMapper("Motion Time", null, _UtilsAnnotation, _PoolAnnotation, _Proxy);
			PrepareMapper("Mirror", valAnnotation, _RepositoryAnnotation, m_ParameterAnnotation, broadcaster);
			PrepareMapper("Cycle Offset", m_ValueAnnotation, m_MappingAnnotation, _ComposerAnnotation, _Proxy);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(m_AuthenticationAnnotation, new GUIContent("Write Defaults"));
				EditorGUILayout.PropertyField(_MerchantAnnotation, new GUIContent("Foot IK"));
			}
			_Publisher.ApplyModifiedProperties();
		}
		else
		{
			IncludeVisitor();
		}
	}

	private static void IncludeVisitor()
	{
		EditorGUI.showMixedValue = true;
		EditorGUI.BeginDisabledGroup(disabled: true);
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label(ClassProperty.DestroyError()._DatabaseProcessor, GUILayout.Height(35f), GUILayout.Width(35f));
			using (new GUILayout.VerticalScope())
			{
				GUILayout.TextField("");
				EditorGUIUtility.labelWidth = 35f;
				EditorGUILayout.TextField("Tag", "");
				EditorGUIUtility.labelWidth = 0f;
			}
		}
		EditorGUILayout.Space();
		EditorGUILayout.ObjectField("Motion", null, typeof(UnityEngine.Object), false);
		EditorGUILayout.IntField("Speed", 0);
		EditorGUI.indentLevel++;
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.TextField("Multiplier", "");
			EditorGUI.indentLevel--;
			EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Normalized Time");
			EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.Toggle("Mirror", false);
			EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.IntField("Cycle Offset", 0);
			EditorGUILayout.ToggleLeft("Parameter", false, GUILayout.Width(90f));
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.Toggle("Write Defaults", false);
			EditorGUILayout.Toggle("Foot IK", false);
		}
		EditorGUI.EndDisabledGroup();
		EditorGUI.showMixedValue = false;
	}

	private static void RunVisitor()
	{
		EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count < 1);
		_ErrorAnnotation.DoLayoutList();
		EditorGUI.EndDisabledGroup();
	}

	internal static void CloneVisitor()
	{
		if (m_ConnectionAnnotation)
		{
			rulesAnnotation.PrintInitializer();
			return;
		}
		using (new GUILayout.HorizontalScope("in bigtitle"))
		{
			EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count <= 0);
			if (ClassProperty.DisableQueue("Add Tracking to Selected States"))
			{
				ConcatAnnotation();
			}
			EditorGUI.EndDisabledGroup();
		}
	}

	private static void LoginVisitor()
	{
		if (_Publisher != null)
		{
			m_TokenizerAnnotation = _Publisher.FindProperty("m_Name");
			decoratorAnnotation = _Publisher.FindProperty("m_Tag");
			m_ComparatorAnnotation = _Publisher.FindProperty("m_Motion");
			m_ExceptionAnnotation = _Publisher.FindProperty("m_Speed");
			objectAnnotation = _Publisher.FindProperty("m_SpeedParameter");
			_UtilsAnnotation = _Publisher.FindProperty("m_TimeParameter");
			valAnnotation = _Publisher.FindProperty("m_Mirror");
			m_ValueAnnotation = _Publisher.FindProperty("m_CycleOffset");
			_MerchantAnnotation = _Publisher.FindProperty("m_IKOnFeet");
			m_AuthenticationAnnotation = _Publisher.FindProperty("m_WriteDefaultValues");
			m_ReponseAnnotation = _Publisher.FindProperty("m_SpeedParameterActive");
			_PoolAnnotation = _Publisher.FindProperty("m_TimeParameterActive");
			m_ParameterAnnotation = _Publisher.FindProperty("m_MirrorParameterActive");
			_ComposerAnnotation = _Publisher.FindProperty("m_CycleOffsetParameterActive");
			_RepositoryAnnotation = _Publisher.FindProperty("m_MirrorParameter");
			m_MappingAnnotation = _Publisher.FindProperty("m_CycleOffsetParameter");
		}
	}

	private void ReflectVisitor()
	{
		if (parserAnnotation && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + m_BridgeAnnotation));
			return m_ParamsAnnotation == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
		})())
		{
			string token = $"Transition Count: {_DefinitionAnnotation.Count}";
			DeleteAnnotation(DeleteVisitor, token, ConsumerAlgo.CallDefinition().showTransitionsCount, iscont2: true, 0);
			DeleteAnnotation(CreateVisitor, "Transition Settings", ConsumerAlgo.CallDefinition().showTransitionSettings, iscont2: true, 1);
			DeleteAnnotation(NewVisitor, "Transition Conditions", ConsumerAlgo.CallDefinition().showTransitionConditions, iscont2: false, 2);
		}
	}

	private void DeleteVisitor()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(25f)))
			{
				Selection.objects = Selection.objects.Except(propertyAnnotation).ToArray();
			}
			GUILayout.Label($"Editing {_DefinitionAnnotation.Count} Transitions");
			GUILayout.FlexibleSpace();
		}
		EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!_Manager && !m_Parser);
		try
		{
			int num = Mathf.CeilToInt((float)_DefinitionAnnotation.Count / 3f);
			int num2 = 0;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			if (_Manager || m_Parser)
			{
				foreach (SetterTests.InstanceTests item in _DefinitionAnnotation.Where((SetterTests.InstanceTests et) => et.itemTests != null))
				{
					if (num2 == num)
					{
						EditorGUILayout.EndVertical();
						EditorGUILayout.BeginVertical();
						num2 = 0;
					}
					using (new GUILayout.HorizontalScope())
					{
						if (ClassProperty.RestartQueue(ClassProperty.DestroyError().importerProcessor, ClassProperty.CalcError().m_InstanceProcessor, GUILayout.Width(25f)))
						{
							UnityEngine.Object[] array = Selection.objects;
							ArrayUtility.Remove(ref array, item.itemTests);
							Selection.objects = array;
						}
						bool flag = item.itemTests == _ProcessorAnnotation.itemTests;
						if (ClassProperty.CountQueue(item.VisitSerializer(), flag ? ClassProperty.CalcError().m_MockProcessor : GUI.skin.label, GUILayout.MinWidth(1f)))
						{
							if (!flag)
							{
								_ProcessorAnnotation = item;
								specification = true;
								MapVisitor();
								RunAlgo();
								CalculateAnnotation();
							}
							else
							{
								_ProcessorAnnotation = default(SetterTests.InstanceTests);
								ManageWrapper();
							}
						}
					}
					num2++;
				}
			}
			else
			{
				GUILayout.Label(string.Empty);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		finally
		{
			((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private void CreateVisitor()
	{
		EditorGUI.BeginDisabledGroup(!m_Parser);
		m_Proc.Update();
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label((!_ProcessorAnnotation.specificationTests) ? string.Empty : (_ProcessorAnnotation.VisitSerializer() + "'s Settings"), GUILayout.ExpandWidth(expand: true));
			if ((_TestsAnnotation.Count == 1 || (bool)_ProcessorAnnotation.specificationTests) && ClassProperty.RestartQueue(ClassProperty.DestroyError().m_BridgeProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
			{
				if (_ObserverAnnotation == null)
				{
					_ObserverAnnotation = new AnimatorStateTransition();
				}
				CustomizeAlgo((!_ProcessorAnnotation.specificationTests) ? _TestsAnnotation[0] : _ProcessorAnnotation.specificationTests, _ObserverAnnotation);
			}
			using (new EditorGUI.DisabledScope(!_ObserverAnnotation))
			{
				if (ClassProperty.RestartQueue(ClassProperty.DestroyError().m_StrategyProcessor, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					for (int i = 0; i < _TestsAnnotation.Count; i++)
					{
						Undo.RecordObject(_TestsAnnotation[i], "PasteSettings");
						CustomizeAlgo(_ObserverAnnotation, _TestsAnnotation[i]);
					}
				}
			}
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(_MockAnnotation);
			EditorGUI.BeginDisabledGroup(!_MockAnnotation.boolValue);
			EditorGUILayout.PropertyField(instanceAnnotation);
			EditorGUI.EndDisabledGroup();
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(_FieldAnnotation);
			EditorGUILayout.PropertyField(attributeAnnotation);
		}
		EditorGUILayout.PropertyField(m_ClientAnnotation);
		EditorGUILayout.PropertyField(configAnnotation);
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(m_DescriptorAnnotation);
			EditorGUILayout.PropertyField(m_CollectionAnnotation);
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(_TemplateAnnotation);
			EditorGUILayout.PropertyField(_MessageAnnotation);
		}
		m_Proc.ApplyModifiedProperties();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUI.EndDisabledGroup();
	}

	private void NewVisitor()
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			if ((bool)ConsumerAlgo.CallDefinition().showMatchingOptions)
			{
				using (new GUILayout.HorizontalScope())
				{
					using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
					ConsumerAlgo.CallDefinition().matchParameter.AddDefinition("Match Parameter", "Ignore Parameter", true, Color.green, Color.red);
					ConsumerAlgo.CallDefinition().matchMode.AddDefinition("Match Mode", "Ignore Mode", true, Color.green, Color.red);
					ConsumerAlgo.CallDefinition().matchValue.AddDefinition("Match Value", "Ignore Value", true, Color.green, Color.red);
					if (changeCheckScope.changed)
					{
						UpdateVisitor();
					}
				}
			}
			using (new EditorGUI.DisabledScope(_DefinitionAnnotation.Count == 0))
			{
				ReorderableList reorderableList = (SetInitializer() ? m_ParamAnnotation : ((!specification) ? _RoleAnnotation : invocationAnnotation));
				Event current = Event.current;
				if (current.type == EventType.KeyDown)
				{
					bool flag2;
					bool flag = !(flag2 = current.keyCode == KeyCode.DownArrow) && current.keyCode == KeyCode.UpArrow;
					if (flag2 || flag)
					{
						Match match = Regex.Match(GUI.GetNameOfFocusedControl(), "Threshold(\\d+)");
						if (match.Success)
						{
							int num = int.Parse(match.Groups[1].Value);
							int num2 = (int)Mathf.Repeat(flag2 ? (++num) : (--num), reorderableList.count);
							EditorGUI.FocusTextInControl($"Threshold{num2}");
						}
					}
				}
				reorderableList.DoLayoutList();
			}
		}
	}

	private static void PushVisitor()
	{
		if (m_Proc != null)
		{
			SerializedObject proc = m_Proc;
			_MockAnnotation = proc.FindProperty("m_HasExitTime");
			instanceAnnotation = proc.FindProperty("m_ExitTime");
			_FieldAnnotation = proc.FindProperty("m_HasFixedDuration");
			attributeAnnotation = proc.FindProperty("m_TransitionDuration");
			m_ClientAnnotation = proc.FindProperty("m_TransitionOffset");
			configAnnotation = proc.FindProperty("m_InterruptionSource");
			m_DescriptorAnnotation = proc.FindProperty("m_OrderedInterruption");
			_TemplateAnnotation = proc.FindProperty("m_CanTransitionToSelf");
			_MessageAnnotation = proc.FindProperty("m_Solo");
			m_CollectionAnnotation = proc.FindProperty("m_Mute");
		}
	}

	[PrototypeServer(0)]
	private static void ViewVisitor()
	{
		List<Type> list = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany((System.Reflection.Assembly a) => a.GetTypes())
			where t.IsSubclassOf(typeof(Component)) && !t.IsAbstract && !t.IsGenericTypeDefinition
			select t).ToList();
		list.Add(typeof(GameObject));
		m_ManagerAnnotation = list.OrderBy((Type t) => t.Name).ToArray();
	}

	private static string[] CollectVisitor(Shader var1)
	{
		if (_SpecificationAnnotation.TryGetValue(var1, out var value))
		{
			return value;
		}
		GameObject gameObject = new GameObject();
		Material material = new Material(var1);
		try
		{
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
			string[] array = (from b in AnimationUtility.GetAnimatableBindings(gameObject, gameObject)
				where b.propertyName.StartsWith("material.") && b.type.RemoveResolver<Renderer>()
				select b.propertyName into p
				orderby p
				select p).ToArray();
			_SpecificationAnnotation.Add(var1, array);
			return array;
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(gameObject);
			UnityEngine.Object.DestroyImmediate(material);
		}
	}

	private static string[] ResolveVisitor(string[] first)
	{
		string m_ThreadReg = "material.";
		return first.Select((string s) => (!s.StartsWith(m_ThreadReg)) ? s : s.Substring(m_ThreadReg.Length)).ToArray();
	}

	private static string[] ListVisitor(Type item)
	{
		if (item == typeof(GameObject))
		{
			return new string[1] { "m_IsActive" };
		}
		if (!(item == typeof(Behaviour)))
		{
			if (!m_ItemAnnotation.TryGetValue(item, out var value))
			{
				if (item.IsSubclassOf(typeof(Component)))
				{
					InstantiateInitializer().ListRules(item);
				}
				string[] array = (from b in AnimationUtility.GetAnimatableBindings(InstantiateInitializer(), InstantiateInitializer())
					where b.type == item
					select b.propertyName into s
					orderby s
					select s).ToArray();
				m_ItemAnnotation.Add(item, array);
				return array;
			}
			return value;
		}
		return new string[1] { "m_Enabled" };
	}

	private static bool VerifyVisitor(Type var1)
	{
		if (!(var1 == typeof(GameObject)))
		{
			if (!m_ItemAnnotation.TryGetValue(var1, out var value))
			{
				return true;
			}
			return value.Length != 0;
		}
		return true;
	}

	private static void FillVisitor(ReorderableList spec)
	{
		AnimatorCondition animatorCondition = default(AnimatorCondition);
		if (_TokenAnnotation.Count <= 0)
		{
			if (LogoutMapper() != null)
			{
				if (LogoutMapper().parameters.Length == 0)
				{
					animatorCondition.parameter = "New Parameter";
				}
				else
				{
					if (LogoutMapper().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Bool && LogoutMapper().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Trigger)
					{
						animatorCondition.mode = AnimatorConditionMode.Equals;
					}
					else
					{
						animatorCondition.mode = AnimatorConditionMode.If;
					}
					animatorCondition.parameter = LogoutMapper().parameters[0].name;
					animatorCondition.threshold = 0f;
				}
			}
		}
		else
		{
			animatorCondition = _TokenAnnotation.Last().m_ProcessVisitor;
		}
		if (!SetInitializer())
		{
			for (int i = 0; i < propertyAnnotation.Count; i++)
			{
				propertyAnnotation[i].AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
			}
		}
		else
		{
			_ProcessorAnnotation.itemTests.AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
		}
		_TokenAnnotation = AssetVisitor(propertyAnnotation);
		MapVisitor();
	}

	private static bool WriteVisitor(AnimatorCondition value, AnimatorCondition counter, bool isres, out bool[] reference2)
	{
		reference2 = new bool[3];
		if (!LogoutMapper())
		{
			return false;
		}
		isres |= !ConsumerAlgo.CallDefinition().showMatchingOptions;
		reference2[0] = value.parameter == counter.parameter;
		if ((!isres && !ConsumerAlgo.CallDefinition().matchParameter) || reference2[0])
		{
			int second;
			UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(value.parameter, out second);
			UnityEngine.AnimatorControllerParameter animatorControllerParameter2 = ResetAnnotation(counter.parameter, out second);
			if (animatorControllerParameter != null || animatorControllerParameter2 != null)
			{
				if (animatorControllerParameter.type != animatorControllerParameter2.type)
				{
					return false;
				}
				UnityEngine.AnimatorControllerParameterType type = animatorControllerParameter.type;
				if (type != UnityEngine.AnimatorControllerParameterType.Trigger)
				{
					reference2[1] = value.mode == counter.mode;
					if ((!isres && (type != UnityEngine.AnimatorControllerParameterType.Bool || !ConsumerAlgo.CallDefinition().matchValue) && (type == UnityEngine.AnimatorControllerParameterType.Bool || !ConsumerAlgo.CallDefinition().matchMode)) || reference2[1])
					{
						if (type != UnityEngine.AnimatorControllerParameterType.Bool)
						{
							reference2[2] = value.threshold == counter.threshold;
							if (isres || (bool)ConsumerAlgo.CallDefinition().matchValue)
							{
								return reference2[2];
							}
							return true;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private static bool ForgotVisitor(AnimatorCondition asset, AnimatorCondition token, bool forcetag)
	{
		bool[] reference;
		return WriteVisitor(asset, token, forcetag, out reference);
	}

	private static List<GlobalVisitor> StopVisitor()
	{
		if (SetInitializer())
		{
			return m_DicAnnotation;
		}
		if (specification)
		{
			return _TokenAnnotation;
		}
		return m_CodeAnnotation;
	}

	private static List<GlobalVisitor> CheckVisitor(AnimatorTransitionBase ident, List<GlobalVisitor> reg)
	{
		for (int i = 0; i < ident.conditions.Length; i++)
		{
			foreach (GlobalVisitor item in reg.Where((GlobalVisitor sc) => !sc._TaskVisitor))
			{
				if (WriteVisitor(item.m_ProcessVisitor, ident.conditions[i], isres: false, out var reference))
				{
					item.CollectInitializer(ident, i);
					item.CheckInitializer(reference);
					break;
				}
			}
		}
		List<GlobalVisitor> list = new List<GlobalVisitor>();
		foreach (GlobalVisitor item2 in reg)
		{
			if (!item2._TaskVisitor)
			{
				list.Add(item2);
			}
			item2._TaskVisitor = false;
		}
		reg = reg.Except(list).ToList();
		return reg;
	}

	private static List<GlobalVisitor> PrepareVisitor(AnimatorTransitionBase config)
	{
		List<GlobalVisitor> list = new List<GlobalVisitor>();
		for (int i = 0; i < config.conditions.Length; i++)
		{
			list.Add(new GlobalVisitor(config, i));
		}
		return list;
	}

	private static List<GlobalVisitor> AssetVisitor(List<AnimatorTransitionBase> ident)
	{
		if (ident.Count != 0)
		{
			List<GlobalVisitor> list = PrepareVisitor(ident[0]);
			for (int i = 1; i < ident.Count; i++)
			{
				if (list.Count != 0)
				{
					list = CheckVisitor(ident[i], list);
					continue;
				}
				return list;
			}
			return list;
		}
		return new List<GlobalVisitor>();
	}

	private static void UpdateVisitor()
	{
		_TokenAnnotation = AssetVisitor(propertyAnnotation);
		MapVisitor();
	}

	private static void ChangeVisitor()
	{
		List<GlobalVisitor> list = new List<GlobalVisitor>();
		List<AnimatorTransitionBase> list2 = new List<AnimatorTransitionBase>();
		for (int i = 0; i < m_CodeAnnotation.Count; i++)
		{
			AnimatorTransitionBase item = m_CodeAnnotation[i]._ProducerVisitor[0].Item1;
			if (!list2.Contains(item))
			{
				list2.Add(item);
				for (int j = 0; j < item.conditions.Length; j++)
				{
					list.Add(new GlobalVisitor(item, j));
				}
			}
		}
		m_CodeAnnotation = list;
	}

	private static void SortVisitor()
	{
		m_ThreadAnnotation = (SetInitializer() ? _ProcessorAnnotation.itemTests.conditions.ToList() : ((!specification) ? m_CodeAnnotation.Select((GlobalVisitor sc) => sc.m_ProcessVisitor).ToList() : _TokenAnnotation.Select((GlobalVisitor sc) => sc.m_ProcessVisitor).ToList()));
	}

	private static void RegisterVisitor()
	{
		if (!SetInitializer())
		{
			foreach (SetterTests.InstanceTests item in _DefinitionAnnotation)
			{
				foreach (AnimatorCondition item2 in m_ThreadAnnotation)
				{
					item.itemTests.AddCondition(item2.mode, item2.threshold, item2.parameter);
					m_CodeAnnotation.Add(new GlobalVisitor(item.itemTests, item.itemTests.conditions.Length - 1));
				}
			}
		}
		else
		{
			foreach (AnimatorCondition item3 in m_ThreadAnnotation)
			{
				_ProcessorAnnotation.itemTests.AddCondition(item3.mode, item3.threshold, item3.parameter);
				m_DicAnnotation.Add(new GlobalVisitor(_ProcessorAnnotation.itemTests, _ProcessorAnnotation.itemTests.conditions.Length - 1));
			}
		}
		_TokenAnnotation = AssetVisitor(propertyAnnotation);
		ChangeVisitor();
		MapVisitor();
	}

	private void LogoutVisitor()
	{
		switch (m_GlobalAnnotation)
		{
		case ControllerAction.RemoveTag:
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array3 = PatchVisitor((int)_TaskAnnotation);
			foreach (UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer2 in array3)
			{
				foreach (AnimatorStateTransition item in animatorControllerLayer2.stateMachine.anyStateTransitions.Where((AnimatorStateTransition t) => t.name == _SchemaAnnotation && t.isExit))
				{
					animatorControllerLayer2.stateMachine.RemoveAnyStateTransition(item);
				}
			}
			FlushAnnotation();
			break;
		}
		case ControllerAction.TagCurrentLayerWith:
			if (!ManageMapper().anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation && t.isExit))
			{
				AnimatorStateTransition animatorStateTransition = ManageMapper().AddAnyStateTransition((AnimatorState)null);
				animatorStateTransition.isExit = true;
				animatorStateTransition.mute = true;
				animatorStateTransition.name = proxyAnnotation;
				FlushAnnotation();
			}
			break;
		case ControllerAction.ReplaceParameter:
		{
			bool last2;
			AnimatorStateMachine[] array4 = InterruptVisitor(out last2);
			for (int num3 = 0; num3 < array4.Length; num3++)
			{
				SetAlgo(array4[num3], _SchemaAnnotation, m_BroadcasterAnnotation, structAnnotation, last2);
			}
			if (_TaskAnnotation != ActionMode.CurrentController)
			{
				break;
			}
			UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
			for (int num4 = LogoutMapper().parameters.Length - 1; num4 >= 0; num4--)
			{
				if (UpdateMapper(LogoutMapper().parameters[num4].name))
				{
					parameters[num4].name = parameters[num4].name.Replace(_SchemaAnnotation, m_BroadcasterAnnotation);
				}
			}
			LogoutMapper().parameters = parameters;
			break;
		}
		case ControllerAction.RemoveParameter:
		{
			bool last;
			AnimatorStateMachine[] array2 = InterruptVisitor(out last);
			for (int num = 0; num < array2.Length; num++)
			{
				SetupAlgo(array2[num], _SchemaAnnotation, last);
			}
			if (_TaskAnnotation != ActionMode.CurrentController)
			{
				break;
			}
			for (int num2 = LogoutMapper().parameters.Length - 1; num2 >= 0; num2--)
			{
				if (AssetMapper(LogoutMapper().parameters[num2].name))
				{
					LogoutMapper().RemoveParameter(num2);
				}
			}
			break;
		}
		case ControllerAction.RemoveLayersWithTag:
		{
			for (int num5 = LogoutMapper().layers.Length - 1; num5 >= 0; num5--)
			{
				if (LogoutMapper().layers[num5].stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation))
				{
					LogoutMapper().RemoveLayer(num5);
				}
			}
			break;
		}
		case ControllerAction.Copy:
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = PatchVisitor((int)m_ProcessAnnotation);
			UnityEditor.Animations.AnimatorController serializerReg = ((producerAnnotation != MoveDestination.Controller) ? LogoutMapper() : m_MethodAnnotation);
			for (int i = 0; i < array.Length; i++)
			{
				UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = ClassProperty.CalculatePredicate(array[i], serializerReg);
				if (animatorControllerLayer == null)
				{
					continue;
				}
				if (serviceAnnotation)
				{
					ManageVisitor(animatorControllerLayer).InvokeResolver(delegate(UnityEngine.AnimatorControllerParameter p)
					{
						_003C_003Ec__DisplayClass370_1 _003C_003Ec__DisplayClass370_ = new _003C_003Ec__DisplayClass370_1();
						if (p != null)
						{
							_003C_003Ec__DisplayClass370_._ResolverReg = ((!ClassProperty.m_AdapterProcessor.Contains(p.name)) ? (p.name + _StateAnnotation) : p.name);
							if (serializerReg.parameters.All(_003C_003Ec__DisplayClass370_.VisitServer))
							{
								serializerReg.AddParameter(new UnityEngine.AnimatorControllerParameter
								{
									name = _003C_003Ec__DisplayClass370_._ResolverReg,
									defaultBool = p.defaultBool,
									defaultFloat = p.defaultFloat,
									defaultInt = p.defaultInt,
									type = p.type
								});
							}
						}
					});
				}
				if (!string.IsNullOrEmpty(_StateAnnotation))
				{
					EnableAlgo(animatorControllerLayer, _StateAnnotation);
				}
			}
			break;
		}
		}
	}

	private static UnityEditor.Animations.AnimatorControllerLayer[] PatchVisitor(int ID_ident)
	{
		UnityEditor.Animations.AnimatorControllerLayer[] result = null;
		switch (ID_ident)
		{
		case 2:
		case 3:
			result = new UnityEditor.Animations.AnimatorControllerLayer[1] { LogoutMapper().layers.First((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine == ManageMapper()) };
			break;
		case 1:
			result = LogoutMapper().layers.Where((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation && t.isExit)).ToArray();
			break;
		case 0:
			result = LogoutMapper().layers;
			break;
		}
		return result;
	}

	private static AnimatorStateMachine[] InterruptVisitor(out bool last)
	{
		AnimatorStateMachine[] result = null;
		switch (_TaskAnnotation)
		{
		case ActionMode.CurrentStatemachine:
			last = false;
			result = new AnimatorStateMachine[1] { RevertMapper() };
			break;
		default:
			last = true;
			break;
		case ActionMode.CurrentController:
			last = true;
			result = LogoutMapper().layers.Select((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine).ToArray();
			break;
		case ActionMode.CurrentLayer:
			last = true;
			result = new AnimatorStateMachine[1] { ManageMapper() };
			break;
		case ActionMode.LayersTaggedWith:
			last = true;
			result = (from l in LogoutMapper().layers
				where l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == proxyAnnotation && t.isExit)
				select l.stateMachine).ToArray();
			break;
		}
		return result;
	}

	private static List<UnityEngine.AnimatorControllerParameter> ManageVisitor(UnityEditor.Animations.AnimatorControllerLayer last)
	{
		return PrintVisitor(last.stateMachine);
	}

	private static List<UnityEngine.AnimatorControllerParameter> PrintVisitor(AnimatorStateMachine spec, bool extractpol = true)
	{
		List<UnityEngine.AnimatorControllerParameter> predicateReg = new List<UnityEngine.AnimatorControllerParameter>();
		spec.AssetPredicate(delegate(AnimatorState s)
		{
			int second;
			if (s.cycleOffsetParameterActive && !string.IsNullOrEmpty(s.cycleOffsetParameter))
			{
				predicateReg.Add(ResetAnnotation(s.cycleOffsetParameter, out second));
			}
			if (s.mirrorParameterActive && !string.IsNullOrEmpty(s.mirrorParameter))
			{
				predicateReg.Add(ResetAnnotation(s.mirrorParameter, out second));
			}
			if (s.speedParameterActive && !string.IsNullOrEmpty(s.speedParameter))
			{
				predicateReg.Add(ResetAnnotation(s.speedParameter, out second));
			}
			if (s.timeParameterActive && !string.IsNullOrEmpty(s.timeParameter))
			{
				predicateReg.Add(ResetAnnotation(s.timeParameter, out second));
			}
			s.motion.StopPredicate(delegate(UnityEditor.Animations.BlendTree tree)
			{
				if (tree.blendType != BlendTreeType.Direct)
				{
					int second2;
					if (!string.IsNullOrEmpty(tree.blendParameter))
					{
						predicateReg.Add(ResetAnnotation(tree.blendParameter, out second2));
					}
					if (tree.blendType != BlendTreeType.Simple1D && !string.IsNullOrEmpty(tree.blendParameterY))
					{
						predicateReg.Add(ResetAnnotation(tree.blendParameterY, out second2));
					}
				}
			}, null);
			s.transitions.InvokeResolver(delegate(AnimatorStateTransition t)
			{
				t.conditions.InvokeResolver(delegate(AnimatorCondition c)
				{
					if (!string.IsNullOrEmpty(c.parameter))
					{
						predicateReg.Add(ResetAnnotation(c.parameter, out var _));
					}
				});
			});
			if (WorkerProperty.InvokePage())
			{
				s.behaviours.InvokeResolver(delegate(StateMachineBehaviour b)
				{
					if (b.GetType() == WorkerProperty.InsertPage())
					{
						new WorkerProperty.BridgeProperty(b).m_DatabaseProperty.InvokeResolver(delegate(WorkerProperty.BridgeProperty.AttrProperty p)
						{
							if (!string.IsNullOrEmpty(p.ResetPage()))
							{
								predicateReg.Add(ResetAnnotation(p.ResetPage(), out var _));
							}
						});
					}
				});
			}
		});
		spec.anyStateTransitions.InvokeResolver(delegate(AnimatorStateTransition t)
		{
			t.conditions.InvokeResolver(delegate(AnimatorCondition c)
			{
				if (!string.IsNullOrEmpty(c.parameter))
				{
					predicateReg.Add(ResetAnnotation(c.parameter, out var _));
				}
			});
		});
		spec.entryTransitions.InvokeResolver(delegate(AnimatorTransition t)
		{
			t.conditions.InvokeResolver(delegate(AnimatorCondition c)
			{
				if (!string.IsNullOrEmpty(c.parameter))
				{
					predicateReg.Add(ResetAnnotation(c.parameter, out var _));
				}
			});
		});
		if (WorkerProperty.InvokePage())
		{
			spec.behaviours.InvokeResolver(delegate(StateMachineBehaviour b)
			{
				if (b.GetType() == WorkerProperty.InsertPage())
				{
					new WorkerProperty.BridgeProperty(b).m_DatabaseProperty.InvokeResolver(delegate(WorkerProperty.BridgeProperty.AttrProperty p)
					{
						if (!string.IsNullOrEmpty(p.ResetPage()))
						{
							predicateReg.Add(ResetAnnotation(p.ResetPage(), out var _));
						}
					});
				}
			});
		}
		if (extractpol)
		{
			spec.stateMachines.InvokeResolver(delegate(ChildAnimatorStateMachine c)
			{
				predicateReg.AddRange(PrintVisitor(c.stateMachine));
			});
		}
		return predicateReg.Where((UnityEngine.AnimatorControllerParameter p) => p != null).Distinct().ToList();
	}

	private static void SearchVisitor(UnityEditor.Animations.AnimatorController init)
	{
		_003C_003Ec__DisplayClass375_0 CS_0024_003C_003E8__locals3 = new _003C_003Ec__DisplayClass375_0();
		CS_0024_003C_003E8__locals3.consumerReg = new HashSet<UnityEngine.Object> { init };
		init.layers.InvokeResolver(delegate(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			CS_0024_003C_003E8__locals3.CustomizeServer(l.stateMachine);
		});
		UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(init));
		int num = 0;
		string text = "";
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			if (!CS_0024_003C_003E8__locals3.consumerReg.Contains(array[num2]) && array[num2] != null)
			{
				num++;
				text = text + (string.IsNullOrEmpty(array[num2].name) ? array[num2].GetType().Name : array[num2].name) + "\n";
				Undo.DestroyObjectImmediate(array[num2]);
			}
		}
		LogoutMapper().ViewPredicate(delegate(AnimatorStateMachine l)
		{
			l.CollectPredicate(delegate(AnimatorStateMachine m)
			{
				m.AssetPredicate(delegate(AnimatorState s)
				{
					s.transitions = s.transitions.Where((AnimatorStateTransition t) => t).ToArray();
					EditorUtility.SetDirty(s);
				}, requiresc: false);
				m.entryTransitions = m.entryTransitions.Where((AnimatorTransition t) => t).ToArray();
				m.anyStateTransitions = m.anyStateTransitions.Where((AnimatorStateTransition t) => t).ToArray();
				EditorUtility.SetDirty(m);
			});
		});
		CustomizeAnnotation($"Found and removed {num} unused Sub-Assets:\n{text}");
		AssetDatabase.SaveAssets();
	}

	[PrototypeServer(0)]
	private static void RevertVisitor()
	{
		List<UnityEditor.Animations.AnimatorController> list = new List<UnityEditor.Animations.AnimatorController>();
		List<string> list2 = new List<string>();
		list.Add(null);
		list2.Add("Blank Layer");
		foreach (string item2 in AssetDatabase.FindAssets("l:Template").Select(AssetDatabase.GUIDToAssetPath))
		{
			UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(item2);
			if (!animatorController)
			{
				continue;
			}
			string[] labels = AssetDatabase.GetLabels(animatorController);
			for (int i = 0; i < labels.Length; i++)
			{
				Match match = Regex.Match(labels[i], "^Template:(.*)$");
				if (match.Success)
				{
					string item = Regex.Replace(match.Groups[1].Value, "(?<!%)%(?!%)", " ").Replace("%%", "%");
					list.Add(animatorController);
					list2.Add(item);
					break;
				}
			}
		}
		requestVisitor = list.ToArray();
		m_PrinterVisitor = list2.ToArray();
	}

	private static void OrderAlgo(UnityEditor.Animations.AnimatorController init, UnityEditor.Animations.AnimatorController ord)
	{
		ParameterRenameWindow parameterRenameWindow = ParameterRenameWindow.ResolveTests(init, ord, !Event.current.control && !Event.current.shift);
		try
		{
			EditorWindow editorWindow = SetterTests.RefTests.PopPolicy();
			Vector2 reg = parameterRenameWindow.FillTests();
			Vector2 center = editorWindow.position.center;
			Vector2 setup = new Vector2(center.x - reg.x / 2f, center.y - reg.y / 2f);
			parameterRenameWindow.LoginHelper(setup, reg);
		}
		catch
		{
			Vector2 reg2 = parameterRenameWindow.FillTests();
			Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			Vector2 setup2 = new Vector2(vector.x - reg2.x / 2f, vector.y - reg2.y / 2f);
			parameterRenameWindow.LoginHelper(setup2, reg2);
		}
	}

	private static UnityEditor.Animations.AnimatorControllerLayer[] CompareAlgo(UnityEditor.Animations.AnimatorController spec, UnityEditor.Animations.AnimatorController ord, (UnityEngine.AnimatorControllerParameter, string)[] dir)
	{
		_003C_003Ec__DisplayClass378_0 CS_0024_003C_003E8__locals14 = new _003C_003Ec__DisplayClass378_0();
		CS_0024_003C_003E8__locals14.m_ProductReg = ord;
		CS_0024_003C_003E8__locals14._CandidateReg = new Dictionary<string, string>();
		ClassProperty.ResetPredicate(spec, CS_0024_003C_003E8__locals14.m_ProductReg, out var dir2);
		foreach (UnityEngine.AnimatorControllerParameter item3 in spec.parameters.Where((UnityEngine.AnimatorControllerParameter p) => ClassProperty.m_AdapterProcessor.Contains(p.name)))
		{
			CS_0024_003C_003E8__locals14.m_ProductReg.RatePredicate(item3);
		}
		UnityEditor.Animations.AnimatorControllerLayer[] array;
		for (int num = 0; num < dir.Length; num++)
		{
			(UnityEngine.AnimatorControllerParameter, string) tuple = dir[num];
			UnityEngine.AnimatorControllerParameter item = tuple.Item1;
			string item2 = tuple.Item2;
			UnityEngine.AnimatorControllerParameter predicate = item.GetPredicate();
			predicate.name = item2;
			CS_0024_003C_003E8__locals14.m_ProductReg.RatePredicate(predicate);
			if (!(item.name == item2))
			{
				CS_0024_003C_003E8__locals14._CandidateReg.Add(item.name, item2);
				array = dir2;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					SetAlgo(array[num2].stateMachine, item.name, item2, istask2: true);
				}
			}
		}
		UnityEditor.Animations.AnimatorControllerLayer[] layers = CS_0024_003C_003E8__locals14.m_ProductReg.layers;
		array = layers;
		foreach (UnityEditor.Animations.AnimatorControllerLayer _StubReg in array)
		{
			if (!dir2.All((UnityEditor.Animations.AnimatorControllerLayer l2) => l2.stateMachine != _StubReg.stateMachine))
			{
				_StubReg.name = CS_0024_003C_003E8__locals14.CloneServer(_StubReg.name);
				_StubReg.stateMachine.name = CS_0024_003C_003E8__locals14.CloneServer(_StubReg.stateMachine.name);
				_StubReg.stateMachine.AssetPredicate(delegate(AnimatorState s)
				{
					s.name = CS_0024_003C_003E8__locals14.CloneServer(s.name);
				});
			}
		}
		CS_0024_003C_003E8__locals14.m_ProductReg.layers = layers;
		CS_0024_003C_003E8__locals14.systemReg = new HashSet<Motion>();
		CS_0024_003C_003E8__locals14.m_ExpressionReg = new Dictionary<Motion, Motion>();
		array = dir2;
		for (int num = 0; num < array.Length; num++)
		{
			array[num].stateMachine.AssetPredicate(delegate(AnimatorState s)
			{
				s.motion = CS_0024_003C_003E8__locals14.DeleteServer(s.motion);
			});
		}
		return dir2;
	}

	private static void SetAlgo(AnimatorStateMachine param, string map, string consumer, bool istask2, bool skipconnection3 = true)
	{
		_003C_003Ec__DisplayClass379_0 CS_0024_003C_003E8__locals15 = new _003C_003Ec__DisplayClass379_0();
		CS_0024_003C_003E8__locals15._ReaderReg = istask2;
		CS_0024_003C_003E8__locals15.m_BridgeReg = map;
		CS_0024_003C_003E8__locals15.m_StrategyReg = consumer;
		if (WorkerProperty.InvokePage())
		{
			CS_0024_003C_003E8__locals15.CollectServer(param.behaviours);
		}
		param.AssetPredicate(delegate(AnimatorState s)
		{
			if (s.cycleOffsetParameterActive)
			{
				s.cycleOffsetParameter = CS_0024_003C_003E8__locals15.ViewServer(s.cycleOffsetParameter);
			}
			if (s.mirrorParameterActive)
			{
				s.mirrorParameter = CS_0024_003C_003E8__locals15.ViewServer(s.mirrorParameter);
			}
			if (s.speedParameterActive)
			{
				s.speedParameter = CS_0024_003C_003E8__locals15.ViewServer(s.speedParameter);
			}
			if (s.timeParameterActive)
			{
				s.timeParameter = CS_0024_003C_003E8__locals15.ViewServer(s.timeParameter);
			}
			CS_0024_003C_003E8__locals15.ResolveServer(s.motion);
			for (int num = s.transitions.Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] conditions = s.transitions[num].conditions;
				for (int num2 = s.transitions[num].conditions.Length - 1; num2 >= 0; num2--)
				{
					conditions[num2].parameter = CS_0024_003C_003E8__locals15.ViewServer(conditions[num2].parameter);
				}
				s.transitions[num].conditions = conditions;
			}
			EditorUtility.SetDirty(s);
			if (WorkerProperty.InvokePage())
			{
				CS_0024_003C_003E8__locals15.CollectServer(s.behaviours);
			}
		}, requiresc: false);
		param.ResolvePredicate(delegate(InstanceServer t)
		{
			for (int num = t.SetContext().Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] array = t.SetContext();
				array[num].parameter = CS_0024_003C_003E8__locals15.ViewServer(array[num].parameter);
				t.PostContext(array);
			}
			EditorUtility.SetDirty((AnimatorTransitionBase)t);
		}, moveserv: false);
		if (!skipconnection3)
		{
			return;
		}
		foreach (AnimatorStateMachine item in param.stateMachines.Select((ChildAnimatorStateMachine c) => c.stateMachine))
		{
			SetAlgo(item, CS_0024_003C_003E8__locals15.m_BridgeReg, CS_0024_003C_003E8__locals15.m_StrategyReg, CS_0024_003C_003E8__locals15._ReaderReg);
		}
	}

	private static void PostAlgo(AnimatorStateMachine init, string token, string serv, bool isord2, bool islast3 = true)
	{
		_003C_003Ec__DisplayClass380_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass380_0();
		CS_0024_003C_003E8__locals5.customerReg = isord2;
		CS_0024_003C_003E8__locals5._DatabaseReg = token;
		CS_0024_003C_003E8__locals5.m_ExporterReg = serv;
		init.CollectPredicate(delegate(AnimatorStateMachine m)
		{
			CS_0024_003C_003E8__locals5.ForgotServer(m.behaviours);
			ChildAnimatorState[] states = m.states;
			foreach (ChildAnimatorState childAnimatorState in states)
			{
				CS_0024_003C_003E8__locals5.ForgotServer(childAnimatorState.state.behaviours);
			}
		});
	}

	private static void SetupAlgo(AnimatorStateMachine res, string connection, bool insertrule = true)
	{
		_003C_003Ec__DisplayClass381_0 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass381_0();
		CS_0024_003C_003E8__locals22._IdentifierReg = connection;
		CS_0024_003C_003E8__locals22.m_DispatcherReg = res;
		CS_0024_003C_003E8__locals22._AttrReg = null;
		if (WorkerProperty.InvokePage())
		{
			CS_0024_003C_003E8__locals22._AttrReg = delegate(StateMachineBehaviour[] b)
			{
				foreach (StateMachineBehaviour stateMachineBehaviour in b)
				{
					if (!(stateMachineBehaviour.GetType() != WorkerProperty.InsertPage()))
					{
						WorkerProperty.BridgeProperty bridgeProperty = new WorkerProperty.BridgeProperty(stateMachineBehaviour);
						for (int num2 = bridgeProperty.m_DatabaseProperty.Count - 1; num2 >= 0; num2--)
						{
							if (CS_0024_003C_003E8__locals22.CheckServer(bridgeProperty.m_DatabaseProperty[num2].ResetPage()))
							{
								bridgeProperty.InitPage(num2);
							}
						}
						EditorUtility.SetDirty(stateMachineBehaviour);
					}
				}
			};
		}
		CS_0024_003C_003E8__locals22.m_DispatcherReg.AssetPredicate(delegate(AnimatorState s)
		{
			if (s.cycleOffsetParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.cycleOffsetParameter))
			{
				s.cycleOffsetParameterActive = false;
			}
			if (s.mirrorParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.mirrorParameter))
			{
				s.mirrorParameterActive = false;
			}
			if (s.speedParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.speedParameter))
			{
				s.speedParameterActive = false;
			}
			if (s.timeParameterActive && CS_0024_003C_003E8__locals22.CheckServer(s.timeParameter))
			{
				s.timeParameterActive = false;
			}
			CS_0024_003C_003E8__locals22.PrepareServer(s.transitions, s.RemoveTransition);
			if (WorkerProperty.InvokePage())
			{
				CS_0024_003C_003E8__locals22._AttrReg(s.behaviours);
			}
		});
		CS_0024_003C_003E8__locals22.PrepareServer(CS_0024_003C_003E8__locals22.m_DispatcherReg.entryTransitions, delegate(AnimatorTransition t)
		{
			CS_0024_003C_003E8__locals22.m_DispatcherReg.RemoveEntryTransition(t);
		});
		CS_0024_003C_003E8__locals22.PrepareServer(CS_0024_003C_003E8__locals22.m_DispatcherReg.anyStateTransitions, delegate(AnimatorStateTransition t)
		{
			CS_0024_003C_003E8__locals22.m_DispatcherReg.RemoveAnyStateTransition(t);
		});
		if (WorkerProperty.InvokePage())
		{
			CS_0024_003C_003E8__locals22._AttrReg(CS_0024_003C_003E8__locals22.m_DispatcherReg.behaviours);
		}
		if (insertrule)
		{
			ChildAnimatorStateMachine[] stateMachines = CS_0024_003C_003E8__locals22.m_DispatcherReg.stateMachines;
			foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
			{
				SetupAlgo(childAnimatorStateMachine.stateMachine, CS_0024_003C_003E8__locals22._IdentifierReg);
			}
		}
	}

	private static void EnableAlgo(UnityEditor.Animations.AnimatorControllerLayer item, string attr)
	{
		_003C_003Ec__DisplayClass382_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass382_0();
		CS_0024_003C_003E8__locals9._RegistryReg = attr;
		item.stateMachine.AssetPredicate(delegate(AnimatorState s)
		{
			s.mirrorParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.mirrorParameter);
			s.cycleOffsetParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.cycleOffsetParameter);
			s.speedParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.speedParameter);
			s.timeParameter = CS_0024_003C_003E8__locals9.RegisterServer(s.timeParameter);
			StateMachineBehaviour[] behaviours = s.behaviours;
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (stateMachineBehaviour.GetType() == WorkerProperty.InsertPage())
				{
					foreach (WorkerProperty.BridgeProperty.AttrProperty item2 in new WorkerProperty.BridgeProperty(stateMachineBehaviour).m_DatabaseProperty)
					{
						item2.FlushPage(CS_0024_003C_003E8__locals9.RegisterServer(item2.ResetPage()));
						EditorUtility.SetDirty(stateMachineBehaviour);
					}
				}
			}
			for (int j = 0; j < s.transitions.Length; j++)
			{
				AnimatorStateTransition animatorStateTransition2 = s.transitions[j];
				AnimatorCondition[] conditions2 = animatorStateTransition2.conditions;
				for (int k = 0; k < animatorStateTransition2.conditions.Length; k++)
				{
					conditions2[k].parameter = CS_0024_003C_003E8__locals9.RegisterServer(conditions2[k].parameter);
				}
				animatorStateTransition2.conditions = conditions2;
				EditorUtility.SetDirty(animatorStateTransition2);
			}
		});
		AnimatorStateTransition[] anyStateTransitions = item.stateMachine.anyStateTransitions;
		foreach (AnimatorStateTransition animatorStateTransition in anyStateTransitions)
		{
			AnimatorCondition[] conditions = animatorStateTransition.conditions;
			for (int num2 = 0; num2 < animatorStateTransition.conditions.Length; num2++)
			{
				conditions[num2].parameter = CS_0024_003C_003E8__locals9.RegisterServer(conditions[num2].parameter);
			}
			animatorStateTransition.conditions = conditions;
			animatorStateTransition.ManagePredicate();
		}
		CS_0024_003C_003E8__locals9.PatchServer(item.stateMachine);
	}

	internal static void PublishAlgo(UnityEditor.Animations.AnimatorController v, int sizecol, UnityEngine.AnimatorControllerParameterType consumer)
	{
		UnityEngine.AnimatorControllerParameter[] parameters = v.parameters;
		UnityEngine.AnimatorControllerParameter tagReg = parameters[sizecol];
		if (RateAnnotation(tagReg.type == consumer, $"Parameter {tagReg.name} is already of type {consumer}!"))
		{
			return;
		}
		if (tagReg.type == UnityEngine.AnimatorControllerParameterType.Float)
		{
			_003C_003Ec__DisplayClass383_1 visitor = default(_003C_003Ec__DisplayClass383_1);
			visitor.m_RequestReg = false;
			visitor.m_PrinterReg = string.Empty;
			Queue<AnimatorStateMachine> queue = new Queue<AnimatorStateMachine>();
			UnityEditor.Animations.AnimatorControllerLayer[] layers = v.layers;
			_003C_003Ec__DisplayClass383_2 comp = default(_003C_003Ec__DisplayClass383_2);
			_003C_003Ec__DisplayClass383_3 second = default(_003C_003Ec__DisplayClass383_3);
			for (int i = 0; i < layers.Length; i++)
			{
				comp._WriterReg = layers[i];
				if ((bool)comp._WriterReg.stateMachine)
				{
					queue.Enqueue(comp._WriterReg.stateMachine);
					while (!visitor.m_RequestReg && queue.Count != 0)
					{
						AnimatorStateMachine animatorStateMachine = queue.Dequeue();
						foreach (AnimatorStateMachine item in animatorStateMachine.stateMachines.Select((ChildAnimatorStateMachine cm) => cm.stateMachine))
						{
							queue.Enqueue(item);
						}
						foreach (AnimatorState item2 in animatorStateMachine.states.Select((ChildAnimatorState cs) => cs.state))
						{
							second._ParamsReg = item2;
							if (!(second._ParamsReg.motion is UnityEditor.Animations.BlendTree blendTree))
							{
								continue;
							}
							if (blendTree.blendType != BlendTreeType.Direct)
							{
								if (ChangeMapper(blendTree.blendParameter == tagReg.name, ref visitor, ref comp, ref second) || ChangeMapper(blendTree.blendType != BlendTreeType.Simple1D && blendTree.blendParameterY == tagReg.name, ref visitor, ref comp, ref second))
								{
									break;
								}
							}
							else if (ChangeMapper(blendTree.children.Any((ChildMotion c) => c.directBlendParameter == tagReg.name), ref visitor, ref comp, ref second))
							{
								break;
							}
						}
					}
				}
				if (visitor.m_RequestReg)
				{
					break;
				}
			}
			if (visitor.m_RequestReg && !string.IsNullOrEmpty(visitor.m_PrinterReg) && !EditorUtility.DisplayDialog("WARNING!", "WARNING! This float is used in a blendtree! Converting " + tagReg.name + " will make blendtrees using it stop working.\n[" + visitor.m_PrinterReg + "]", "Continue", "Cancel"))
			{
				return;
			}
		}
		if ((bool)ConsumerAlgo.CallDefinition().warnParameterConversion)
		{
			int i = EditorUtility.DisplayDialogComplex("WARNING!", "WARNING: MOST CONVERSIONS ARE LOSSY!\nConverted animator parameters may not handle all existing condition cases the same way as their original types! Use at your own risk! Undo is possible.", "Continue", "Always Continue", "Cancel");
			if (i != 1)
			{
				if (i == 2)
				{
					return;
				}
			}
			else
			{
				ConsumerAlgo.CallDefinition().warnParameterConversion.ExcludeDefinition(excludeparam: false);
			}
		}
		Undo.RegisterCompleteObjectUndo(v, "Convert Parameter");
		try
		{
			int num = v.layers.Length;
			float num2 = 1f / (float)num;
			AssetDatabase.StartAssetEditing();
			for (int num3 = 0; num3 < num; num3++)
			{
				UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = v.layers[num3];
				EditorUtility.DisplayProgressBar("Converting Parameter", $"{animatorControllerLayer.name} ({num3 + 1}/{num})", (float)(num3 + 1) * num2);
				PopAlgo(v, animatorControllerLayer.stateMachine, tagReg.name, tagReg.type, consumer);
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
			EditorUtility.ClearProgressBar();
		}
		UnityEngine.AnimatorControllerParameterType type = tagReg.type;
		parameters[sizecol].type = consumer;
		v.parameters = parameters;
		CustomizeAnnotation($"Finished converting {tagReg.name} from {type} to {consumer}!");
	}

	internal static void PopAlgo(UnityEditor.Animations.AnimatorController config, AnimatorStateMachine ivk, string dir, UnityEngine.AnimatorControllerParameterType last2, UnityEngine.AnimatorControllerParameterType value3)
	{
		_003C_003Ec__DisplayClass384_0 asset = default(_003C_003Ec__DisplayClass384_0);
		asset.listenerReg = dir;
		asset.m_GetterReg = last2;
		asset.m_InterceptorReg = value3;
		if (asset.m_GetterReg == asset.m_InterceptorReg)
		{
			return;
		}
		Undo.RecordObject(ivk, "Convert Parameter");
		ChildAnimatorState[] states = ivk.states;
		AnimatorTransitionBase[] transitions;
		for (int i = 0; i < states.Length; i++)
		{
			ChildAnimatorState childAnimatorState = states[i];
			Undo.RecordObject(childAnimatorState.state, "Convert Parameter");
			transitions = childAnimatorState.state.transitions;
			SortMapper(transitions, out var counter, out var helper, ref asset);
			foreach (AnimatorTransitionBase item in counter)
			{
				childAnimatorState.state.AddTransition((AnimatorStateTransition)item);
				AssetDatabase.AddObjectToAsset(item, config);
				item.hideFlags = HideFlags.HideInHierarchy;
			}
			foreach (AnimatorTransitionBase item2 in helper)
			{
				childAnimatorState.state.RemoveTransition((AnimatorStateTransition)item2);
			}
		}
		transitions = ivk.entryTransitions;
		SortMapper(transitions, out var counter2, out var helper2, ref asset);
		foreach (AnimatorTransitionBase item3 in counter2)
		{
			if (!item3.destinationState)
			{
				if ((bool)item3.destinationStateMachine)
				{
					AnimatorTransition cust = ivk.AddEntryTransition(item3.destinationStateMachine);
					TestAlgo(item3, cust);
				}
			}
			else
			{
				AnimatorTransition cust2 = ivk.AddEntryTransition(item3.destinationState);
				TestAlgo(item3, cust2);
			}
		}
		foreach (AnimatorTransitionBase item4 in helper2)
		{
			ivk.RemoveEntryTransition((AnimatorTransition)item4);
		}
		transitions = ivk.anyStateTransitions;
		SortMapper(transitions, out var counter3, out var helper3, ref asset);
		foreach (AnimatorTransitionBase item5 in counter3)
		{
			if (!item5.destinationState)
			{
				if ((bool)item5.destinationStateMachine)
				{
					AnimatorStateTransition cust3 = ivk.AddAnyStateTransition(item5.destinationStateMachine);
					TestAlgo(item5, cust3);
				}
			}
			else
			{
				AnimatorStateTransition cust4 = ivk.AddAnyStateTransition(item5.destinationState);
				TestAlgo(item5, cust4);
			}
		}
		foreach (AnimatorTransitionBase item6 in helper3)
		{
			ivk.RemoveAnyStateTransition((AnimatorStateTransition)item6);
		}
		ChildAnimatorStateMachine[] stateMachines = ivk.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
			AnimatorTransition[] stateMachineTransitions = ivk.GetStateMachineTransitions(childAnimatorStateMachine.stateMachine);
			transitions = stateMachineTransitions;
			SortMapper(transitions, out var counter4, out var helper4, ref asset);
			ivk.SetStateMachineTransitions(childAnimatorStateMachine.stateMachine, stateMachineTransitions.Union(counter4).Except(helper4).Cast<AnimatorTransition>()
				.ToArray());
			foreach (AnimatorTransitionBase item7 in counter4)
			{
				AssetDatabase.AddObjectToAsset(item7, config);
				item7.hideFlags = HideFlags.HideInHierarchy;
			}
			foreach (AnimatorTransitionBase item8 in helper4)
			{
				AssetDatabase.RemoveObjectFromAsset(item8);
			}
		}
		stateMachines = ivk.stateMachines;
		foreach (ChildAnimatorStateMachine childAnimatorStateMachine2 in stateMachines)
		{
			PopAlgo(config, childAnimatorStateMachine2.stateMachine, asset.listenerReg, asset.m_GetterReg, asset.m_InterceptorReg);
		}
	}

	internal static void ComputeAlgo(AnimatorTransitionBase[] v, string vis, UnityEngine.AnimatorControllerParameterType serv, UnityEngine.AnimatorControllerParameterType task2, out List<AnimatorTransitionBase> init3, out List<AnimatorTransitionBase> def4)
	{
		init3 = (def4 = null);
		if (serv == task2)
		{
			return;
		}
		init3 = new List<AnimatorTransitionBase>();
		def4 = new List<AnimatorTransitionBase>();
		for (int i = 0; i < v.Length; i++)
		{
			AnimatorTransitionBase animatorTransitionBase = v[i];
			if (def4.Contains(animatorTransitionBase))
			{
				continue;
			}
			Undo.RecordObject(animatorTransitionBase, "Convert Parameter");
			AnimatorCondition[] array = animatorTransitionBase.conditions;
			int infoReg;
			for (infoReg = array.Length - 1; infoReg >= 0; infoReg--)
			{
				if (array[infoReg].parameter != vis)
				{
					continue;
				}
				bool flag2;
				bool flag = ((!(flag2 = serv == UnityEngine.AnimatorControllerParameterType.Bool)) ? (array[infoReg].threshold > 0.008f) : (array[infoReg].mode == AnimatorConditionMode.If));
				switch (task2)
				{
				case UnityEngine.AnimatorControllerParameterType.Bool:
					switch (serv)
					{
					case UnityEngine.AnimatorControllerParameterType.Int:
						switch (array[infoReg].mode)
						{
						case AnimatorConditionMode.NotEqual:
							array[infoReg].mode = ((array[infoReg].threshold != 1f) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
							break;
						case AnimatorConditionMode.Equals:
							array[infoReg].mode = ((!(array[infoReg].threshold < 1f)) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
							break;
						default:
							if (array[infoReg].threshold >= 0f)
							{
								if (array[infoReg].threshold <= 1f)
								{
									array[infoReg].mode = ((array[infoReg].mode == AnimatorConditionMode.Greater) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
								}
								else
								{
									array[infoReg].mode = AnimatorConditionMode.If;
								}
							}
							else
							{
								array[infoReg].mode = AnimatorConditionMode.IfNot;
							}
							break;
						}
						break;
					case UnityEngine.AnimatorControllerParameterType.Float:
						if (array[infoReg].threshold < 0f)
						{
							array[infoReg].mode = AnimatorConditionMode.IfNot;
						}
						else if (array[infoReg].threshold <= 1f)
						{
							array[infoReg].mode = ((array[infoReg].mode == AnimatorConditionMode.Greater) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot);
						}
						else
						{
							array[infoReg].mode = AnimatorConditionMode.If;
						}
						break;
					}
					break;
				case UnityEngine.AnimatorControllerParameterType.Float:
					if (serv != UnityEngine.AnimatorControllerParameterType.Int)
					{
						if (flag2)
						{
							array[infoReg].threshold = (flag ? 0.992f : 0.008f);
							array[infoReg].mode = ((!flag) ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater);
						}
					}
					else if (array[infoReg].mode == AnimatorConditionMode.Equals)
					{
						array[infoReg].mode = AnimatorConditionMode.Greater;
						array[infoReg].threshold -= 0.008f;
						AnimatorCondition item = array[infoReg];
						item.mode = AnimatorConditionMode.Less;
						item.threshold += 0.016f;
						ArrayUtility.Insert(ref array, infoReg, item);
					}
					else if (array[infoReg].mode == AnimatorConditionMode.NotEqual)
					{
						array[infoReg].mode = AnimatorConditionMode.Less;
						array[infoReg].threshold -= 0.008f;
						AnimatorTransitionBase animatorTransitionBase3 = null;
						if (animatorTransitionBase is AnimatorStateTransition spec)
						{
							AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
							TestAlgo(spec, animatorStateTransition);
							animatorTransitionBase3 = animatorStateTransition;
						}
						else if (animatorTransitionBase is AnimatorTransition spec2)
						{
							AnimatorTransition animatorTransition = new AnimatorTransition();
							TestAlgo(spec2, animatorTransition);
							animatorTransitionBase3 = animatorTransition;
						}
						else
						{
							AnimatorTransitionBase animatorTransitionBase4 = (AnimatorTransitionBase)Activator.CreateInstance(animatorTransitionBase.GetType());
							TestAlgo(animatorTransitionBase, animatorTransitionBase4);
							animatorTransitionBase3 = animatorTransitionBase4;
						}
						Undo.RegisterCreatedObjectUndo(animatorTransitionBase3, "Convert Parameter");
						AnimatorCondition[] conditions = animatorTransitionBase3.conditions;
						conditions[infoReg].mode = AnimatorConditionMode.Greater;
						conditions[infoReg].threshold += 0.008f;
						animatorTransitionBase3.conditions = conditions;
						init3.Add(animatorTransitionBase3);
					}
					break;
				case UnityEngine.AnimatorControllerParameterType.Int:
					if (serv == UnityEngine.AnimatorControllerParameterType.Float)
					{
						float num = 0.9f;
						float num2 = 0.032f;
						AnimatorConditionMode mode = array[infoReg].mode;
						bool flag3 = false;
						for (int num3 = infoReg; num3 >= 0; num3--)
						{
							if (!(array[num3].parameter != vis) && mode != array[num3].mode)
							{
								bool flag4 = mode == AnimatorConditionMode.Less && array[num3].threshold >= array[infoReg].threshold - num;
								bool flag5 = mode == AnimatorConditionMode.Greater && array[num3].threshold <= array[infoReg].threshold + num;
								if (flag4 || flag5)
								{
									int num4 = (flag4 ? Mathf.FloorToInt(array[infoReg].threshold) : Mathf.FloorToInt(array[num3].threshold));
									array[num3].threshold = num4;
									array[num3].mode = AnimatorConditionMode.Equals;
									ArrayUtility.RemoveAt(ref array, infoReg);
									flag3 = true;
									break;
								}
							}
						}
						if (flag3)
						{
							break;
						}
						for (int j = i + 1; j < v.Length; j++)
						{
							AnimatorTransitionBase animatorTransitionBase2 = v[j];
							int advisorReg = animatorTransitionBase2.conditions.FindResolver((AnimatorCondition c) => c.parameter == vis);
							if (advisorReg == -1)
							{
								continue;
							}
							AnimatorCondition animatorCondition = animatorTransitionBase2.conditions[advisorReg];
							if (animatorCondition.mode != mode && ((mode == AnimatorConditionMode.Less && animatorCondition.threshold <= array[infoReg].threshold + num2) | (mode == AnimatorConditionMode.Greater && animatorCondition.threshold >= array[infoReg].threshold - num2)))
							{
								IEnumerable<AnimatorCondition> key = array.Where((AnimatorCondition _, int index) => index != infoReg);
								IEnumerable<AnimatorCondition> b = animatorTransitionBase2.conditions.Where((AnimatorCondition _, int index) => index != advisorReg);
								if (ClassProperty.CreatePredicate(key, b))
								{
									array[infoReg].mode = AnimatorConditionMode.NotEqual;
									array[infoReg].threshold = Mathf.RoundToInt((array[infoReg].threshold + animatorCondition.threshold) / 2f);
									def4.Add(animatorTransitionBase2);
									flag3 = true;
									break;
								}
							}
						}
						if (!flag3)
						{
							array[infoReg].threshold = Mathf.RoundToInt(array[infoReg].threshold);
						}
					}
					else if (flag2)
					{
						array[infoReg].threshold = (flag ? 1 : 0);
						array[infoReg].mode = AnimatorConditionMode.Equals;
					}
					break;
				}
			}
			animatorTransitionBase.conditions = array;
		}
		if (init3.Any())
		{
			List<AnimatorTransitionBase> init4 = init3;
			List<AnimatorTransitionBase> def5 = def4;
			int num5 = 0;
			do
			{
				ComputeAlgo(init4.ToArray(), vis, serv, task2, out init4, out def5);
				init3.AddRange(init4);
				def4.AddRange(def5);
				num5++;
			}
			while (init4.Any() && num5 <= 30);
		}
	}

	private static void MoveAlgo()
	{
		m_ClassAnnotation = !m_ClassAnnotation;
		if (!m_ClassAnnotation)
		{
			ConcatAlgo();
			return;
		}
		serverAnnotation.Clear();
		containerAnnotation = false;
		baseAnnotation = false;
	}

	private static void ConcatAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		if (m_AlgoAnnotation.Count <= 0 && m_WrapperAnnotation.Length == 0 && !collection)
		{
			return;
		}
		AnimatorStateMachine[] wrapperAnnotation;
		foreach (AnimatorState item in _VisitorAnnotation)
		{
			foreach (AnimatorState item2 in m_AlgoAnnotation)
			{
				list.Add(item.AddTransition(item2));
			}
			wrapperAnnotation = m_WrapperAnnotation;
			foreach (AnimatorStateMachine destinationStateMachine in wrapperAnnotation)
			{
				list.Add(item.AddTransition(destinationStateMachine));
			}
			if (collection)
			{
				list.Add(item.AddExitTransition());
			}
		}
		if (m_Template)
		{
			foreach (AnimatorState item3 in m_AlgoAnnotation)
			{
				list.Add(ManageMapper().AddAnyStateTransition(item3));
			}
			wrapperAnnotation = m_WrapperAnnotation;
			foreach (AnimatorStateMachine destinationStateMachine2 in wrapperAnnotation)
			{
				list.Add(ManageMapper().AddAnyStateTransition(destinationStateMachine2));
			}
		}
		if (m_Message)
		{
			foreach (AnimatorState item4 in m_AlgoAnnotation)
			{
				list.Add(RevertMapper().AddEntryTransition(item4));
			}
			wrapperAnnotation = m_WrapperAnnotation;
			foreach (AnimatorStateMachine destinationStateMachine3 in wrapperAnnotation)
			{
				list.Add(RevertMapper().AddEntryTransition(destinationStateMachine3));
			}
		}
		wrapperAnnotation = _AnnotationAnnotation;
		foreach (AnimatorStateMachine sourceStateMachine in wrapperAnnotation)
		{
			foreach (AnimatorState item5 in m_AlgoAnnotation)
			{
				list.Add(RevertMapper().AddStateMachineTransition(sourceStateMachine, item5));
			}
			AnimatorStateMachine[] wrapperAnnotation2 = m_WrapperAnnotation;
			foreach (AnimatorStateMachine destinationStateMachine4 in wrapperAnnotation2)
			{
				list.Add(RevertMapper().AddStateMachineTransition(sourceStateMachine, destinationStateMachine4));
			}
			if (collection)
			{
				list.Add(RevertMapper().AddStateMachineExitTransition(sourceStateMachine));
			}
		}
		list.ForEach(delegate(AnimatorTransitionBase t)
		{
			RateAlgo(ConsumerAlgo.CallDefinition().defaultTransition, t);
		});
		UnityEngine.Object[] objects = list.ToArray();
		Selection.objects = objects;
		if (m_Template || m_Message)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void CallAlgo()
	{
		MethodInfo _PrototypeReg = typeof(AnimatorStateMachine).GetMethod("MoveState", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method = typeof(AnimatorStateMachine).GetMethod("MoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
		IEnumerable<ChildAnimatorState> first = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state));
		IEnumerable<ChildAnimatorStateMachine> first2 = RevertMapper().stateMachines.Where((ChildAnimatorStateMachine c) => m_WrapperAnnotation.Contains(c.stateMachine));
		Dictionary<UnityEngine.Object, Vector3> m_CallbackReg = new Dictionary<UnityEngine.Object, Vector3>();
		Vector3 indexerReg = Vector3.zero;
		int m_IssuerReg = 0;
		first.InvokeResolver(delegate(ChildAnimatorState c)
		{
			m_CallbackReg.Add(c.state, c.position);
			indexerReg += c.position;
			m_IssuerReg++;
		});
		first2.InvokeResolver(delegate(ChildAnimatorStateMachine c)
		{
			m_CallbackReg.Add(c.stateMachine, c.position);
			indexerReg += c.position;
			m_IssuerReg++;
		});
		indexerReg /= (float)m_IssuerReg;
		AnimatorStateMachine m_RuleReg = RevertMapper().AddStateMachine("New StateMachine", indexerReg);
		m_AlgoAnnotation.ForEach(delegate(AnimatorState s)
		{
			_PrototypeReg.Invoke(RevertMapper(), new object[2] { s, m_RuleReg });
		});
		AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
		foreach (AnimatorStateMachine animatorStateMachine in wrapperAnnotation)
		{
			AnimatorTransition[] stateMachineTransitions = RevertMapper().GetStateMachineTransitions(animatorStateMachine);
			RevertMapper().SetStateMachineTransitions(animatorStateMachine, null);
			method.Invoke(RevertMapper(), new object[2] { animatorStateMachine, m_RuleReg });
			AnimatorTransition[] array = RevertMapper().GetStateMachineTransitions(m_RuleReg);
			ArrayUtility.AddRange(ref array, stateMachineTransitions);
			RevertMapper().SetStateMachineTransitions(m_RuleReg, array);
		}
		ChildAnimatorState[] states = m_RuleReg.states;
		for (int num2 = 0; num2 < states.Length; num2++)
		{
			if (m_CallbackReg.ContainsKey(states[num2].state))
			{
				states[num2].position = m_CallbackReg[states[num2].state];
			}
		}
		m_RuleReg.states = states;
		ChildAnimatorStateMachine[] stateMachines = m_RuleReg.stateMachines;
		for (int num3 = 0; num3 < stateMachines.Length; num3++)
		{
			if (m_CallbackReg.ContainsKey(stateMachines[num3].stateMachine))
			{
				stateMachines[num3].position = m_CallbackReg[stateMachines[num3].stateMachine];
			}
		}
		m_RuleReg.stateMachines = stateMachines;
	}

	private static void CancelAlgo()
	{
		MethodInfo method = typeof(AnimatorStateMachine).GetMethod("MoveState", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method2 = typeof(AnimatorStateMachine).GetMethod("MoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
		Dictionary<UnityEngine.Object, Vector3> dictionary = new Dictionary<UnityEngine.Object, Vector3>();
		for (int i = 0; i < m_WrapperAnnotation.Length; i++)
		{
			AnimatorStateMachine _SingletonReg = m_WrapperAnnotation[i];
			if (!(RevertMapper() == _SingletonReg))
			{
				Vector3 vector = RevertMapper().stateMachines.First((ChildAnimatorStateMachine c) => c.stateMachine == _SingletonReg).position;
				Vector3 seed = _SingletonReg.states.Aggregate(Vector3.zero, (Vector3 current, ChildAnimatorState child) => current + child.position);
				seed = _SingletonReg.stateMachines.Aggregate(seed, (Vector3 current, ChildAnimatorStateMachine child) => current + child.position);
				seed /= (float)(_SingletonReg.stateMachines.Length + _SingletonReg.states.Length);
				ChildAnimatorState[] states = _SingletonReg.states;
				for (int num = 0; num < states.Length; num++)
				{
					ChildAnimatorState childAnimatorState = states[num];
					dictionary.Add(childAnimatorState.state, childAnimatorState.position - seed + vector);
					method.Invoke(_SingletonReg, new object[2]
					{
						childAnimatorState.state,
						RevertMapper()
					});
				}
				ChildAnimatorStateMachine[] stateMachines = _SingletonReg.stateMachines;
				for (int num = 0; num < stateMachines.Length; num++)
				{
					ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[num];
					dictionary.Add(childAnimatorStateMachine.stateMachine, childAnimatorStateMachine.position - seed + vector);
					method2.Invoke(_SingletonReg, new object[2]
					{
						childAnimatorStateMachine.stateMachine,
						RevertMapper()
					});
				}
				RevertMapper().RemoveStateMachine(_SingletonReg);
			}
		}
		ChildAnimatorState[] states2 = RevertMapper().states;
		for (int num2 = 0; num2 < states2.Length; num2++)
		{
			if (dictionary.TryGetValue(states2[num2].state, out var value))
			{
				states2[num2].position = value;
			}
		}
		RevertMapper().states = states2;
		ChildAnimatorStateMachine[] stateMachines2 = RevertMapper().stateMachines;
		for (int num3 = 0; num3 < stateMachines2.Length; num3++)
		{
			if (dictionary.TryGetValue(stateMachines2[num3].stateMachine, out var value2))
			{
				stateMachines2[num3].position = value2;
			}
		}
		RevertMapper().stateMachines = stateMachines2;
	}

	private static void CountAlgo()
	{
		ChildAnimatorState[] source = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state)).ToArray();
		float x = source.Max((ChildAnimatorState c) => c.position.x);
		ChildAnimatorState[] states = RevertMapper().states;
		for (int num = 0; num < states.Length; num++)
		{
			if (source.Contains(states[num]))
			{
				states[num].position = new Vector3(x, states[num].position.y);
			}
		}
		if (m_Message)
		{
			RevertMapper().entryPosition = new Vector3(x, RevertMapper().entryPosition.y);
		}
		if (m_Template)
		{
			RevertMapper().anyStatePosition = new Vector3(x, RevertMapper().anyStatePosition.y);
		}
		if (collection)
		{
			RevertMapper().exitPosition = new Vector3(x, RevertMapper().exitPosition.y);
		}
		Undo.RecordObject(RevertMapper(), "Align Horizontal");
		RevertMapper().states = states;
		EditorUtility.SetDirty(LogoutMapper());
		if (m_Message || collection)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void DisableAlgo()
	{
		ChildAnimatorState[] source = RevertMapper().states.Where((ChildAnimatorState c) => m_AlgoAnnotation.Contains(c.state)).ToArray();
		float y = source.Min((ChildAnimatorState c) => c.position.y);
		ChildAnimatorState[] states = RevertMapper().states;
		for (int num = 0; num < states.Length; num++)
		{
			if (source.Contains(states[num]))
			{
				states[num].position = new Vector3(states[num].position.x, y);
			}
		}
		if (m_Message)
		{
			RevertMapper().entryPosition = new Vector3(RevertMapper().entryPosition.x, y);
		}
		if (m_Template)
		{
			RevertMapper().anyStatePosition = new Vector3(RevertMapper().anyStatePosition.x, y);
		}
		if (collection)
		{
			RevertMapper().exitPosition = new Vector3(RevertMapper().exitPosition.x, y);
		}
		Undo.RecordObject(RevertMapper(), "Align Vertical");
		RevertMapper().states = states;
		EditorUtility.SetDirty(LogoutMapper());
		if (m_Message || collection)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void InsertAlgo()
	{
		if (m_AlgoAnnotation.Count != 0)
		{
			Selection.objects = Selection.objects.Where((UnityEngine.Object o) => !(o is AnimatorState)).ToArray();
		}
		else
		{
			Selection.objects = Selection.objects.Concat(RevertMapper().states.Select((ChildAnimatorState c) => c.state)).ToArray();
		}
	}

	private static void RestartAlgo(AnimatorStateMachine param, IEnumerable<AnimatorState> vis, string serv)
	{
		foreach (AnimatorState vi in vis)
		{
			if (vi.name != serv)
			{
				Undo.RecordObject(vi, "Rename States");
				vi.name = param.MakeUniqueStateName(serv);
			}
		}
	}

	private static void QueryAlgo(AnimatorState i)
	{
		AddAlgo(ConsumerAlgo.CallDefinition().defaultState, i);
	}

	private static void AddAlgo(AnimatorState last, AnimatorState reg)
	{
		string text = reg.name;
		Motion motion = reg.motion;
		EditorUtility.CopySerialized(last, reg);
		reg.name = text;
		reg.motion = motion;
		EditorUtility.SetDirty(reg);
	}

	private static string[] InvokeAlgo(AnimatorState setup)
	{
		if (!setup.tag.FlushResolver())
		{
			return setup.tag.Split(new char[1] { ',' });
		}
		return Array.Empty<string>();
	}

	private static bool FindAlgo(AnimatorState last)
	{
		return InvokeAlgo(last).Any((string t) => t == "ce_comment");
	}

	private static bool ExcludeAlgo(AnimatorState reference)
	{
		return InvokeAlgo(reference).Any((string t) => t == "ce_bigcomment");
	}

	private static bool InitAlgo(AnimatorState param)
	{
		if (!FindAlgo(param))
		{
			return ExcludeAlgo(param);
		}
		return true;
	}

	private static bool VisitAlgo(AnimatorState asset)
	{
		return InvokeAlgo(asset).Any((string t) => m_SerializerAnnotation.Contains(t));
	}

	private static void DefineAlgo(AnimatorState config, string second)
	{
		if (string.IsNullOrWhiteSpace(config.tag))
		{
			config.tag = second;
		}
		else if (!InvokeAlgo(config).Contains(second))
		{
			config.tag = config.tag + "," + second;
		}
		config.ManagePredicate();
	}

	private static void StartAlgo(AnimatorState config, string ord)
	{
		if (!string.IsNullOrWhiteSpace(config.tag))
		{
			string[] array = InvokeAlgo(config);
			if (array.Contains(ord))
			{
				config.tag = string.Join(",", array.Except(new string[1] { ord }));
			}
			config.ManagePredicate();
		}
	}

	private static void ReadAlgo()
	{
		UnityEngine.Object obj = Selection.objects.FirstOrDefault((UnityEngine.Object o) => o is AnimatorState || o is AnimatorStateMachine);
		AnimatorState obj2 = obj as AnimatorState;
		object obj3;
		if ((object)obj2 != null)
		{
			obj3 = obj2.behaviours;
			if (obj3 != null)
			{
				goto IL_0054;
			}
		}
		else
		{
			obj3 = null;
		}
		obj3 = (obj as AnimatorStateMachine)?.behaviours;
		goto IL_0054;
		IL_0054:
		@struct = (StateMachineBehaviour[])obj3;
	}

	private static void SelectAlgo()
	{
		AnimatorState[] array = m_AlgoAnnotation.ToArray();
		AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
		UnityEngine.Object[] objectsToUndo = array;
		Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
		objectsToUndo = wrapperAnnotation;
		Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
		StateMachineBehaviour[] array2 = @struct;
		foreach (StateMachineBehaviour stateMachineBehaviour in array2)
		{
			Type type = stateMachineBehaviour.GetType();
			AnimatorState[] array3 = array;
			foreach (AnimatorState animatorState in array3)
			{
				EditorUtility.CopySerialized(stateMachineBehaviour, animatorState.AddStateMachineBehaviour(type));
			}
			AnimatorStateMachine[] array4 = wrapperAnnotation;
			foreach (AnimatorStateMachine animatorStateMachine in array4)
			{
				EditorUtility.CopySerialized(stateMachineBehaviour, animatorStateMachine.AddStateMachineBehaviour(type));
			}
		}
	}

	private static bool RemoveAlgo()
	{
		if (@struct == null)
		{
			return false;
		}
		return @struct.Length != 0;
	}

	private static void InstantiateAlgo()
	{
		AnimatorState[] array = m_AlgoAnnotation.ToArray();
		AnimatorStateMachine[] wrapperAnnotation = m_WrapperAnnotation;
		UnityEngine.Object[] objectsToUndo = array;
		Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
		objectsToUndo = wrapperAnnotation;
		Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
		AnimatorState[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].behaviours = Array.Empty<StateMachineBehaviour>();
		}
		AnimatorStateMachine[] array3 = wrapperAnnotation;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].behaviours = Array.Empty<StateMachineBehaviour>();
		}
	}

	private static List<AnimatorStateTransition> AwakeAlgo(List<AnimatorStateTransition> value = null)
	{
		if (value == null)
		{
			value = _TestsAnnotation;
		}
		return ManageMapper().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass410_1 _003C_003Ec__DisplayClass410_ = new _003C_003Ec__DisplayClass410_1();
			_003C_003Ec__DisplayClass410_._AccountReg = t;
			return value.Contains(_003C_003Ec__DisplayClass410_._AccountReg) && (RevertMapper().states.Any(_003C_003Ec__DisplayClass410_.SetupThread) || RevertMapper().stateMachines.Any(_003C_003Ec__DisplayClass410_.EnableThread));
		}).ToList();
	}

	private static List<AnimatorTransitionBase> ResetAlgo(List<AnimatorTransitionBase> info = null)
	{
		if (info == null)
		{
			info = propertyAnnotation;
		}
		return info.Where((AnimatorTransitionBase t) => RevertMapper().entryTransitions.Contains(t)).ToList();
	}

	private static (AnimatorState, List<AnimatorStateTransition>)[] FlushAlgo(List<AnimatorStateTransition> var1 = null)
	{
		if (var1 == null)
		{
			var1 = _TestsAnnotation;
		}
		List<AnimatorState> list = new List<AnimatorState>();
		for (int i = 0; i < RevertMapper().states.Length; i++)
		{
			if (RevertMapper().states[i].state.transitions.Any((AnimatorStateTransition t) => var1.Contains(t)))
			{
				list.Add(RevertMapper().states[i].state);
			}
		}
		(AnimatorState, List<AnimatorStateTransition>)[] array = new(AnimatorState, List<AnimatorStateTransition>)[list.Count];
		for (int num = 0; num < list.Count; num++)
		{
			List<AnimatorStateTransition> list2 = new List<AnimatorStateTransition>();
			for (int num2 = 0; num2 < var1.Count; num2++)
			{
				if (list[num].transitions.Contains(var1[num2]))
				{
					list2.Add(var1[num2]);
				}
			}
			var1 = var1.Except(list2).ToList();
			array[num] = (list[num], list2);
		}
		return array;
	}

	private static void ConnectAlgo()
	{
		_003C_003Ec__DisplayClass413_0 CS_0024_003C_003E8__locals17 = new _003C_003Ec__DisplayClass413_0();
		CS_0024_003C_003E8__locals17.tokenReg = new List<AnimatorTransitionBase>();
		(AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(_TestsAnnotation);
		CS_0024_003C_003E8__locals17.codeReg = AwakeAlgo();
		CS_0024_003C_003E8__locals17.m_DicReg = ResetAlgo();
		(AnimatorState, List<List<AnimatorStateTransition>>)[] array2 = new(AnimatorState, List<List<AnimatorStateTransition>>)[array.Length];
		List<List<AnimatorStateTransition>> list = new List<List<AnimatorStateTransition>>();
		List<List<AnimatorTransitionBase>> list2 = new List<List<AnimatorTransitionBase>>();
		for (int i = 0; i < array2.Length; i++)
		{
			AnimatorState item = array[i].Item1;
			List<AnimatorStateTransition> objectReg = array[i].Item2;
			List<List<AnimatorStateTransition>> list3 = new List<List<AnimatorStateTransition>>();
			while (objectReg.Count > 0)
			{
				List<AnimatorStateTransition> list4 = new List<AnimatorStateTransition>();
				list4 = objectReg.Where((AnimatorStateTransition t) => t.destinationState == objectReg[0].destinationState).ToList();
				objectReg = objectReg.Except(list4).ToList();
				list3.Add(list4);
			}
			array2[i] = (item, list3);
		}
		while (CS_0024_003C_003E8__locals17.codeReg.Count > 0)
		{
			List<AnimatorStateTransition> list5 = new List<AnimatorStateTransition>();
			list5 = CS_0024_003C_003E8__locals17.codeReg.Where((AnimatorStateTransition t) => t.destinationState == CS_0024_003C_003E8__locals17.codeReg[0].destinationState).ToList();
			CS_0024_003C_003E8__locals17.codeReg = CS_0024_003C_003E8__locals17.codeReg.Except(list5).ToList();
			list.Add(list5);
		}
		while (CS_0024_003C_003E8__locals17.m_DicReg.Count > 0)
		{
			List<AnimatorTransitionBase> list6 = new List<AnimatorTransitionBase>();
			list6 = CS_0024_003C_003E8__locals17.m_DicReg.Where((AnimatorTransitionBase t) => t.destinationState == CS_0024_003C_003E8__locals17.m_DicReg[0].destinationState).ToList();
			CS_0024_003C_003E8__locals17.m_DicReg = CS_0024_003C_003E8__locals17.m_DicReg.Except(list6).ToList();
			list2.Add(list6);
		}
		for (int num = 0; num < array2.Length; num++)
		{
			for (int num2 = 0; num2 < array2[num].Item2.Count; num2++)
			{
				CS_0024_003C_003E8__locals17.PopThread(array2[num].Item2[num2], array2[num].Item1);
			}
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			CS_0024_003C_003E8__locals17.MoveThread(list[num3]);
		}
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			CS_0024_003C_003E8__locals17.ComputeThread(list2[num4]);
		}
		Selection.objects = Selection.objects.Concat(CS_0024_003C_003E8__locals17.tokenReg).ToArray();
		EditorUtility.SetDirty(LogoutMapper());
	}

	private static AnimatorStateTransition CalculateAlgo<T>(T value) where T : AnimatorTransitionBase
	{
		AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
		EditorUtility.CopySerialized(value, animatorStateTransition);
		Undo.RegisterCreatedObjectUndo(animatorStateTransition, "DuplicatedTransition");
		AssetDatabase.AddObjectToAsset(animatorStateTransition, AssetDatabase.GetAssetPath(value));
		animatorStateTransition.hideFlags = value.hideFlags;
		return animatorStateTransition;
	}

	private static void TestAlgo<T>(T spec, T cust) where T : AnimatorTransitionBase
	{
		if (!(spec.GetType() == cust.GetType()))
		{
			cust.isExit = spec.isExit;
			cust.mute = spec.mute;
			cust.name = spec.name;
			cust.solo = spec.solo;
			cust.destinationState = spec.destinationState;
			cust.destinationStateMachine = spec.destinationStateMachine;
			cust.conditions = spec.conditions;
		}
		else
		{
			EditorUtility.CopySerialized(spec, cust);
		}
	}

	private static AnimatorStateTransition MapAlgo<T>(T i) where T : AnimatorTransitionBase
	{
		AnimatorStateTransition animatorStateTransition = ManageMapper().AddAnyStateTransition(i.destinationState);
		EditorUtility.CopySerialized(i, animatorStateTransition);
		animatorStateTransition.hideFlags = i.hideFlags;
		return animatorStateTransition;
	}

	private static AnimatorTransitionBase ValidateAlgo(AnimatorTransitionBase reference)
	{
		AnimatorTransitionBase animatorTransitionBase = RevertMapper().AddEntryTransition(reference.destinationState);
		EditorUtility.CopySerialized(reference, animatorTransitionBase);
		return animatorTransitionBase;
	}

	private static void CustomizeAlgo(AnimatorStateTransition reference, AnimatorStateTransition col)
	{
		AnimatorCondition[] conditions = col.conditions;
		AnimatorStateMachine destinationStateMachine = col.destinationStateMachine;
		AnimatorState destinationState = col.destinationState;
		bool isExit = col.isExit;
		string text = col.name;
		EditorUtility.CopySerialized(reference, col);
		col.conditions = conditions;
		col.destinationStateMachine = destinationStateMachine;
		col.destinationState = destinationState;
		col.isExit = isExit;
		col.name = text;
		EditorUtility.SetDirty(col);
	}

	private static void RateAlgo(AnimatorTransitionBase key, AnimatorTransitionBase map)
	{
		if (key is AnimatorStateTransition reference && map is AnimatorStateTransition col)
		{
			CustomizeAlgo(reference, col);
		}
	}

	private static void DestroyAlgo()
	{
		if (_TestsAnnotation.Count == 0)
		{
			List<AnimatorStateTransition> valReg = new List<AnimatorStateTransition>();
			RevertMapper().AssetPredicate(delegate(AnimatorState s)
			{
				valReg.AddRange(s.transitions);
			}, requiresc: false);
			valReg.AddRange(ManageMapper().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
			{
				_003C_003Ec__DisplayClass420_1 _003C_003Ec__DisplayClass420_ = new _003C_003Ec__DisplayClass420_1();
				_003C_003Ec__DisplayClass420_._ValueReg = t;
				return RevertMapper().states.Any(_003C_003Ec__DisplayClass420_.QueryThread);
			}));
			Selection.objects = Selection.objects.Concat(valReg).ToArray();
		}
		else
		{
			Selection.objects = Selection.objects.Where((UnityEngine.Object o) => !(o is AnimatorStateTransition)).ToArray();
		}
	}

	private static void GetAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		foreach (AnimatorState item in m_AlgoAnnotation)
		{
			list.AddRange(item.transitions);
		}
		if (m_Message)
		{
			list.AddRange(RevertMapper().entryTransitions);
		}
		if (m_Template)
		{
			list.AddRange(RevertMapper().anyStateTransitions);
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void CalcAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		ChildAnimatorState[] states = RevertMapper().states;
		foreach (ChildAnimatorState childAnimatorState in states)
		{
			list.AddRange(childAnimatorState.state.transitions.Where(delegate(AnimatorStateTransition t)
			{
				if (m_AlgoAnnotation.Contains(t.destinationState))
				{
					return true;
				}
				return collection && t.isExit;
			}));
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void IncludeAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		foreach (AnimatorState item in m_AlgoAnnotation)
		{
			AnimatorStateTransition[] transitions = item.transitions;
			foreach (AnimatorStateTransition animatorStateTransition in transitions)
			{
				if (m_AlgoAnnotation.Contains(animatorStateTransition.destinationState) || (animatorStateTransition.destinationState == null && collection))
				{
					list.Add(animatorStateTransition);
				}
			}
		}
		if (m_Template)
		{
			list.AddRange(RevertMapper().anyStateTransitions.Where((AnimatorStateTransition t) => m_AlgoAnnotation.Contains(t.destinationState)));
		}
		if (m_Message)
		{
			list.AddRange(RevertMapper().entryTransitions.Where((AnimatorTransition t) => m_AlgoAnnotation.Contains(t.destinationState)));
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void RunAlgo()
	{
		SerializedObject proc;
		if (!(_ProcessorAnnotation.itemTests != null))
		{
			if (_TestsAnnotation.Count > 0)
			{
				UnityEngine.Object[] objs = _TestsAnnotation.ToArray();
				proc = new SerializedObject(objs);
			}
			else
			{
				proc = task;
			}
		}
		else
		{
			proc = new SerializedObject(_ProcessorAnnotation.itemTests);
		}
		m_Proc = proc;
	}

	private static void CloneAlgo()
	{
		_FieldVisitor = !_FieldVisitor;
	}

	private static void LoginAlgo()
	{
		containerAnnotation = !containerAnnotation;
		if (containerAnnotation)
		{
			baseAnnotation = false;
			m_ClassAnnotation = false;
		}
		else
		{
			ReflectAlgo();
		}
	}

	private static void ReflectAlgo()
	{
		if (m_AlgoAnnotation.Count <= 0 && !collection)
		{
			return;
		}
		bool flag = _AttributeVisitor;
		foreach (SetterTests.InstanceTests item in serverAnnotation)
		{
			foreach (AnimatorState item2 in m_AlgoAnnotation)
			{
				switch (item.globalTests)
				{
				case SetterTests.TokenizerTests.NodeType.state:
				{
					AnimatorStateTransition animatorStateTransition = CalculateAlgo(item.specificationTests);
					animatorStateTransition.isExit = false;
					animatorStateTransition.destinationState = item2;
					item.m_SchemaTests.AddTransition(animatorStateTransition);
					break;
				}
				case SetterTests.TokenizerTests.NodeType.entry:
					ValidateAlgo(item.itemTests).destinationState = item2;
					break;
				case SetterTests.TokenizerTests.NodeType.any:
					flag = true;
					MapAlgo(item.specificationTests).destinationState = item2;
					break;
				}
			}
			if (collection && item.globalTests == SetterTests.TokenizerTests.NodeType.state)
			{
				AnimatorStateTransition animatorStateTransition2 = CalculateAlgo(item.specificationTests);
				animatorStateTransition2.isExit = true;
				animatorStateTransition2.destinationState = null;
				item.m_SchemaTests.AddTransition(animatorStateTransition2);
			}
		}
		if (_AttributeVisitor)
		{
			foreach (SetterTests.InstanceTests item3 in serverAnnotation)
			{
				switch (item3.globalTests)
				{
				case SetterTests.TokenizerTests.NodeType.any:
					ManageMapper().RemoveAnyStateTransition(item3.specificationTests);
					break;
				case SetterTests.TokenizerTests.NodeType.state:
					item3.m_SchemaTests.RemoveTransition(item3.specificationTests);
					break;
				case SetterTests.TokenizerTests.NodeType.entry:
					item3.broadcasterTests.RemoveEntryTransition((AnimatorTransition)item3.itemTests);
					break;
				}
			}
		}
		if (flag)
		{
			PatchAlgo();
		}
	}

	private static void DeleteAlgo()
	{
		if (_DefinitionAnnotation.Count > 0)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			foreach (SetterTests.InstanceTests item in _DefinitionAnnotation)
			{
				if (item.globalTests != SetterTests.TokenizerTests.NodeType.state)
				{
					continue;
				}
				AnimatorStateTransition specificationTests = item.specificationTests;
				if (!specificationTests.destinationState)
				{
					return;
				}
				AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
				EditorUtility.CopySerialized(specificationTests, animatorStateTransition);
				animatorStateTransition.destinationState = item.m_SchemaTests;
				if ((bool)ConsumerAlgo.CallDefinition().autoReverseModes)
				{
					AnimatorCondition[] conditions = animatorStateTransition.conditions;
					for (int i = 0; i < conditions.Length; i++)
					{
						conditions[i] = ListAlgo(conditions[i], clientVisitor);
					}
					animatorStateTransition.conditions = conditions;
				}
				Undo.RegisterCreatedObjectUndo(animatorStateTransition, "ReversedTransitions");
				Undo.RecordObject(specificationTests.destinationState, "ReversedTransitions");
				AssetDatabase.AddObjectToAsset(animatorStateTransition, LogoutMapper());
				animatorStateTransition.hideFlags = specificationTests.hideFlags;
				specificationTests.destinationState.AddTransition(animatorStateTransition);
				list.Add(animatorStateTransition);
				if (_AttributeVisitor)
				{
					Undo.RecordObject(item.m_SchemaTests, "ReversedTransitions");
					item.m_SchemaTests.RemoveTransition(item.specificationTests);
					PatchAlgo();
				}
			}
			if (list.Count > 0)
			{
				Selection.objects = list.ToArray();
			}
		}
		EditorUtility.SetDirty(LogoutMapper());
	}

	private static void CreateAlgo()
	{
		baseAnnotation = !baseAnnotation;
		if (baseAnnotation)
		{
			containerAnnotation = false;
			m_ClassAnnotation = false;
		}
		else
		{
			NewAlgo();
		}
	}

	private static void NewAlgo()
	{
		serverAnnotation?.InvokeResolver(delegate(SetterTests.InstanceTests t)
		{
			_003C_003Ec__DisplayClass430_0 _003C_003Ec__DisplayClass430_ = new _003C_003Ec__DisplayClass430_0();
			_003C_003Ec__DisplayClass430_.m_MerchantReg = t;
			m_AlgoAnnotation.InvokeResolver(_003C_003Ec__DisplayClass430_.AddThread);
			if (m_Template && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddAnyStateTransition((AnimatorState)null));
			}
			if (m_Message && !_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests.ClonePredicate())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.itemTests, RevertMapper().AddEntryTransition((AnimatorState)null));
			}
			if (_AttributeVisitor)
			{
				_003C_003Ec__DisplayClass430_.m_MerchantReg.InitSerializer();
			}
		});
		if (m_Template || _AttributeVisitor)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void PushAlgo()
	{
		HashSet<UnityEngine.Object> authenticationReg = new HashSet<UnityEngine.Object>();
		_003C_003Ec__DisplayClass431_0 CS_0024_003C_003E8__locals0;
		_TestsAnnotation.InvokeResolver(delegate(AnimatorStateTransition sel)
		{
			_003C_003Ec__DisplayClass431_1 _003C_003Ec__DisplayClass431_ = new _003C_003Ec__DisplayClass431_1();
			_003C_003Ec__DisplayClass431_.m_PoolReg = CS_0024_003C_003E8__locals0;
			_003C_003Ec__DisplayClass431_._ReponseReg = sel;
			if (_003C_003Ec__DisplayClass431_._ReponseReg.destinationState != null || _003C_003Ec__DisplayClass431_._ReponseReg.destinationStateMachine != null)
			{
				if (!ManageMapper().anyStateTransitions.Any(_003C_003Ec__DisplayClass431_.FindThread))
				{
					authenticationReg.Add(MapAlgo(_003C_003Ec__DisplayClass431_._ReponseReg));
					int num = 0;
					while (true)
					{
						if (num >= RevertMapper().states.Length)
						{
							return;
						}
						if (RevertMapper().states[num].state.transitions.Any(_003C_003Ec__DisplayClass431_.InitThread))
						{
							break;
						}
						num++;
					}
					Undo.RecordObject(RevertMapper().states[num].state, "Make AnyTransition");
					RevertMapper().states[num].state.RemoveTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
				}
				else
				{
					RevertMapper().states.InvokeResolver(_003C_003Ec__DisplayClass431_.ExcludeThread);
					_ = ManageMapper().anyStateTransitions;
					Undo.RecordObject(ManageMapper(), "Remove AnyTransition");
					ManageMapper().RemoveAnyStateTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
				}
			}
		});
		Selection.objects = authenticationReg.ToArray();
	}

	private static void ViewAlgo()
	{
		_003C_003Ec__DisplayClass432_0 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_0();
		_003C_003Ec__DisplayClass432_._ComposerReg = new List<AnimatorTransitionBase>();
		if (SetInitializer())
		{
			int num = 0;
			if (RevertMapper().entryTransitions.Contains(_ProcessorAnnotation.itemTests))
			{
				num = 1;
			}
			else if (RevertMapper().anyStateTransitions.Contains(_ProcessorAnnotation.itemTests))
			{
				num = 2;
			}
			AnimatorState s = null;
			if (num == 0)
			{
				s = RevertMapper().states.First((ChildAnimatorState c) => c.state.transitions.Contains(_ProcessorAnnotation.itemTests)).state;
			}
			switch (num)
			{
			case 2:
				_003C_003Ec__DisplayClass432_.DefineThread((AnimatorStateTransition)_ProcessorAnnotation.itemTests);
				break;
			case 1:
				_003C_003Ec__DisplayClass432_.VisitThread(_ProcessorAnnotation.itemTests);
				break;
			case 0:
				_003C_003Ec__DisplayClass432_.StartThread((AnimatorStateTransition)_ProcessorAnnotation.itemTests, s);
				break;
			}
		}
		else
		{
			(AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(_TestsAnnotation);
			List<AnimatorStateTransition> list = AwakeAlgo(_TestsAnnotation);
			List<AnimatorTransitionBase> list2 = ResetAlgo(propertyAnnotation);
			if (_DefinitionAnnotation.Count > 0)
			{
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					AnimatorState item = array[num2].Item1;
					List<AnimatorStateTransition> item2 = array[num2].Item2;
					new List<AnimatorStateTransition>();
					for (int num3 = 0; num3 < item2.Count; num3++)
					{
						if (_TestsAnnotation.Contains(item2[num3]))
						{
							_003C_003Ec__DisplayClass432_.StartThread(item2[num3], item);
						}
					}
				}
				for (int num4 = 0; num4 < list.Count; num4++)
				{
					_003C_003Ec__DisplayClass432_.DefineThread(list[num4]);
				}
				for (int num5 = 0; num5 < list2.Count; num5++)
				{
					_003C_003Ec__DisplayClass432_.VisitThread(list2[num5]);
				}
			}
		}
		Selection.objects = Selection.objects.Concat(_003C_003Ec__DisplayClass432_._ComposerReg).ToArray();
		EditorUtility.SetDirty(LogoutMapper());
	}

	private static AnimatorConditionMode CollectAlgo(AnimatorConditionMode reference)
	{
		return reference switch
		{
			AnimatorConditionMode.NotEqual => AnimatorConditionMode.Equals, 
			AnimatorConditionMode.Equals => AnimatorConditionMode.NotEqual, 
			AnimatorConditionMode.If => AnimatorConditionMode.IfNot, 
			AnimatorConditionMode.IfNot => AnimatorConditionMode.If, 
			AnimatorConditionMode.Greater => AnimatorConditionMode.Less, 
			_ => AnimatorConditionMode.Greater, 
		};
	}

	private static AnimatorCondition ResolveAlgo(AnimatorCondition reference)
	{
		return ListAlgo(reference, (bool)ConsumerAlgo.CallDefinition().reverseModifiesValues ^ Event.current.control);
	}

	private static AnimatorCondition ListAlgo(AnimatorCondition config, bool excludeord)
	{
		AnimatorCondition result = config;
		result.mode = CollectAlgo(config.mode);
		if (excludeord)
		{
			bool flag = config.mode == AnimatorConditionMode.Greater;
			bool flag2 = config.mode == AnimatorConditionMode.Less;
			if (flag || flag2)
			{
				int second;
				UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(config.parameter, out second);
				if (animatorControllerParameter != null)
				{
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						if (flag)
						{
							result.threshold += 0.008f;
						}
						else
						{
							result.threshold -= 0.008f;
						}
					}
					else if (!flag)
					{
						result.threshold -= 1f;
					}
					else
					{
						result.threshold += 1f;
					}
				}
			}
		}
		return result;
	}

	[SpecialName]
	private static EditorWindow FindInitializer()
	{
		if (_IteratorAnnotation == null)
		{
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(mapperVisitor);
			if (array.Length == 0)
			{
				return null;
			}
			_IteratorAnnotation = array[0] as EditorWindow;
		}
		return _IteratorAnnotation;
	}

	[SpecialName]
	private static object InitInitializer()
	{
		if (!(FindInitializer() == null))
		{
			return m_ProcessorVisitor.GetValue(FindInitializer());
		}
		return null;
	}

	[SpecialName]
	private static AnimationClip DefineInitializer()
	{
		if (InitInitializer() == null)
		{
			return null;
		}
		return (AnimationClip)_ObserverVisitor.GetValue(InitInitializer());
	}

	[SpecialName]
	private static void StartInitializer(AnimationClip init)
	{
		if (InitInitializer() != null)
		{
			_ObserverVisitor.SetValue(InitInitializer(), init);
		}
	}

	[SpecialName]
	private static GameObject SelectInitializer()
	{
		return VerifyAlgo();
	}

	private static GameObject VerifyAlgo(bool checklast = true)
	{
		if (_AlgoVisitor != null)
		{
			return _AlgoVisitor;
		}
		if ((bool)annotationVisitor)
		{
			return null;
		}
		return (GameObject)serverVisitor.GetValue(InitInitializer());
	}

	[SpecialName]
	private static GameObject InstantiateInitializer()
	{
		if (m_PublisherAnnotation == null)
		{
			AwakeInitializer(new GameObject("OverrideGameObject")
			{
				hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
			});
			m_ConfigurationAnnotation = InstantiateInitializer().AddComponent<Animator>();
		}
		return m_PublisherAnnotation;
	}

	[SpecialName]
	private static void AwakeInitializer(GameObject ident)
	{
		m_PublisherAnnotation = ident;
	}

	[SpecialName]
	private static Animator FlushInitializer()
	{
		if (m_ConfigurationAnnotation == null)
		{
			m_ConfigurationAnnotation = InstantiateInitializer().GetComponent<Animator>();
		}
		return m_ConfigurationAnnotation;
	}

	[SpecialName]
	private static void TestInitializer(UnityEditor.Animations.AnimatorController first)
	{
		if (first == annotationVisitor)
		{
			return;
		}
		if (!(first != null))
		{
			if (InstantiateInitializer() != null)
			{
				UnityEngine.Object.DestroyImmediate(InstantiateInitializer());
			}
		}
		else
		{
			wrapperVisitor = true;
			FlushInitializer().runtimeAnimatorController = first;
			mapperVisitor.DisableList("EditGameObjectInternal").Invoke(Resources.FindObjectsOfTypeAll(mapperVisitor)[0], new object[2]
			{
				InstantiateInitializer(),
				null
			});
			wrapperVisitor = false;
		}
		annotationVisitor = first;
	}

	internal static void FillAlgo()
	{
		mapperVisitor = ClassProperty.FillRules("UnityEditor.AnimationWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_InitializerVisitor = ClassProperty.FillRules("UnityEditorInternal.AnimationWindowState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		definitionVisitor = ClassProperty.FillRules("UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_RegVisitor = ClassProperty.FillRules("UnityEditor.AnimEditor, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_PropertyVisitor = ClassProperty.FillRules("UnityEditorInternal.AnimationWindowSelectionItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_TestsVisitor = ClassProperty.FillRules("UnityEditorInternal.AnimationWindowControl, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_SerializerVisitor = ClassProperty.FillRules("UnityEditorInternal.AnimationWindowHierarchyNode, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("binding", BindingFlags.Instance | BindingFlags.Public);
		m_ProcessorVisitor = mapperVisitor.QueryList("state");
		_ObserverVisitor = _InitializerVisitor.QueryList("activeAnimationClip");
		serverVisitor = _InitializerVisitor.QueryList("activeRootGameObject");
		m_ThreadVisitor = _InitializerVisitor.QueryList("activeGameObject");
		m_PolicyVisitor = _InitializerVisitor.QueryList("activeScriptableObject");
		m_ItemVisitor = _RegVisitor.DisableList("PlayControlsOnGUI");
	}

	internal static void WriteAlgo()
	{
		if (Event.current.type != EventType.Layout)
		{
			SpecificationAlgo.CloneReg("AnimationWindowFieldsPatch");
		}
	}

	internal static void ForgotAlgo()
	{
		SpecificationAlgo.IncludeReg("AnimationWindowFieldsPatch", mapperVisitor, "OnGUI", SpecificationAlgo.CreateReg(WriteAlgo), _RegVisitor, "TabSelectionOnGUI", SpecificationAlgo.PrepareReg(PlayControlsOnGUIPrefix));
		SpecificationAlgo.MapReg(mapperVisitor, "ShouldUpdateGameObjectSelection", SpecificationAlgo.RevertReg<bool, bool>(ShouldUpdateGameObjectSelectionPrefix));
		SpecificationAlgo.MapReg(mapperVisitor, "ShouldUpdateSelection", SpecificationAlgo.PrepareReg(ShouldUpdateAnimationSelectionPrefix));
		SpecificationAlgo.MapReg(_TestsVisitor, "get_canPlay", SpecificationAlgo.PrepareReg(InterruptIfOverridingControllerPrefix));
		SpecificationAlgo.MapReg(_TestsVisitor, "get_canPreview", SpecificationAlgo.PrepareReg(InterruptIfOverridingControllerPrefix));
		SpecificationAlgo.MapReg(_TestsVisitor, "get_canRecord", SpecificationAlgo.PrepareReg(InterruptIfOverridingControllerPrefix));
		SpecificationAlgo.MapReg(_PropertyVisitor, "GetEditorCurveValueType", SpecificationAlgo.CreateReg(GetEditorCurveValueTypePrefix), SpecificationAlgo.CreateReg(GetEditorCurveValueTypePost));
		SpecificationAlgo.MapReg(_PropertyVisitor, "get_rootGameObject", SpecificationAlgo.RevertReg<GameObject, bool>(AnimationWindowSelectionItemGetRootGameObjectPrefix));
		SpecificationAlgo.TestReg("UnityEditorInternal.AddCurvesPopupHierarchyDataSource, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "FetchData", SpecificationAlgo.CreateReg(CurvesPopupFetchDataPrefix), SpecificationAlgo.CreateReg(CurvesPopupFetchDataPost));
		SpecificationAlgo.MapReg(definitionVisitor, "DoNodeGUI", SpecificationAlgo.PushReg<Rect, TreeViewItem>(AnimationWindowHierarchyNodeGUIPrefix), SpecificationAlgo.NewReg<Rect>(AnimationWindowHierarchyNodeGUIPost));
		SpecificationAlgo.MapReg(definitionVisitor, "GenerateMenu", null, SpecificationAlgo.CountTests<GenericMenu, List<object>>(AnimationWindowHierarchyGUIGenerateMenuPost));
		SpecificationAlgo.MapReg(definitionVisitor, "DoAddCurveButton", null, SpecificationAlgo.NewReg<Rect>(AnimationWindowDoAddCurveButtonPost));
	}

	private static bool PlayControlsOnGUIPrefix()
	{
		if (!ConsumerAlgo.CallDefinition().aw_active || !ConsumerAlgo.CallDefinition().aw_enableOverride)
		{
			return true;
		}
		using (new AnnotationPolicy(iskey: false))
		{
			if (annotationVisitor == null)
			{
				TestInitializer(null);
				GUILayout.Label("[Override Controller]", ClassProperty.CalcError().annotationObserver, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (ClassProperty.DefineQueue(lastRect))
				{
					ClassProperty.ConcatList(annotationVisitor, typeof(UnityEditor.Animations.AnimatorController), null, null, loaddef3: true, null, null, delegate(UnityEngine.Object o)
					{
						TestInitializer((UnityEditor.Animations.AnimatorController)o);
					});
				}
				ClassProperty.AwakeRules(lastRect, delegate(IEnumerable<UnityEditor.Animations.AnimatorController> o)
				{
					UnityEditor.Animations.AnimatorController[] source = (o as UnityEditor.Animations.AnimatorController[]) ?? o.ToArray();
					if (source.Any())
					{
						TestInitializer(source.First());
					}
				});
			}
			else
			{
				TestInitializer((UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(annotationVisitor, typeof(UnityEditor.Animations.AnimatorController), true));
				if ((bool)annotationVisitor)
				{
					ClassProperty.TestQueue("Controller", iscfg: true, 80f + (float)annotationVisitor.name.Length * 6.5f, 20f, isivk3: false);
				}
			}
			if (_AlgoVisitor == null)
			{
				_AlgoVisitor = null;
				GUILayout.Label("[Override Root]", ClassProperty.CalcError().annotationObserver, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
				Rect lastRect2 = GUILayoutUtility.GetLastRect();
				if (ClassProperty.DefineQueue(lastRect2))
				{
					ClassProperty.ConcatList(_AlgoVisitor, typeof(GameObject), null, null, loaddef3: true, null, null, delegate(UnityEngine.Object o)
					{
						_AlgoVisitor = (GameObject)o;
					});
				}
				ClassProperty.AwakeRules(lastRect2, delegate(IEnumerable<GameObject> o)
				{
					GameObject[] source = (o as GameObject[]) ?? o.ToArray();
					if (source.Any())
					{
						_AlgoVisitor = source.First();
					}
				});
			}
			else
			{
				_AlgoVisitor = (GameObject)EditorGUILayout.ObjectField(_AlgoVisitor, typeof(GameObject), true);
				if ((bool)_AlgoVisitor)
				{
					ClassProperty.TestQueue("Root", iscfg: true, 50f + (float)_AlgoVisitor.name.Length * 6.5f, 20f, isivk3: false);
				}
			}
		}
		return true;
	}

	private static void AnimationWindowHierarchyGUIGenerateMenuPost(ref GenericMenu __result, List<object> interactedNodes)
	{
		m_SingletonVisitor = __result;
		pageVisitor = true;
		resolverVisitor = interactedNodes;
	}

	private static void AnimationWindowHierarchyNodeGUIPrefix(Rect rect, TreeViewItem node)
	{
		if (!ConsumerAlgo.CallDefinition().aw_active || !ConsumerAlgo.CallDefinition().aw_enablePropertyEditing || node == null)
		{
			return;
		}
		EditorCurveBinding? _AttributeReg = (EditorCurveBinding?)m_SerializerVisitor.GetValue(node);
		if (!_AttributeReg.HasValue)
		{
			return;
		}
		GameObject fieldReg = SelectInitializer();
		if (fieldReg == null)
		{
			return;
		}
		ClassProperty.InstantiateRules(rect, delegate(GameObject o)
		{
			AnimationClip setup = DefineInitializer();
			if (!(fieldReg == null))
			{
				if (!o.transform.IsChildOf(fieldReg.transform) && o != fieldReg)
				{
					FindVisitor(o.name + " is not a child of " + fieldReg.name, CustomLogType.Warning);
				}
				else
				{
					string text = AnimationUtility.CalculateTransformPath(o.transform, fieldReg.transform);
					if (!(text == _AttributeReg.Value.path))
					{
						EditorCurveBinding value = _AttributeReg.Value;
						value.path = text;
						CheckAlgo(setup, _AttributeReg.Value, value);
					}
				}
			}
		});
	}

	private static void AnimationWindowHierarchyNodeGUIPost(Rect rect)
	{
		if (!ConsumerAlgo.CallDefinition().aw_active || !ConsumerAlgo.CallDefinition().aw_enablePropertyEditing || !pageVisitor)
		{
			return;
		}
		pageVisitor = false;
		rect.y += 60f;
		rect.x += Event.current.mousePosition.x;
		EditorCurveBinding[] array;
		try
		{
			AnimationClip clientReg = DefineInitializer();
			array = (from n in resolverVisitor
				where ((TreeViewItem)n).children.CalcRules()
				select (EditorCurveBinding)m_SerializerVisitor.GetValue(n) into b
				where (b.isPPtrCurve && !AnimationUtility.GetObjectReferenceCurve(clientReg, b).GetRules()) || !AnimationUtility.GetEditorCurve(clientReg, b).keys.GetRules()
				select b).ToArray();
		}
		catch (System.Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return;
		}
		if (array.Length != 0)
		{
			StopAlgo(m_SingletonVisitor, rect, array);
			m_SingletonVisitor.ShowAsContext();
		}
	}

	private static void StopAlgo(GenericMenu last, Rect token, EditorCurveBinding[] temp)
	{
		EditorCurveBinding configReg = temp[0];
		bool num = temp.Any((EditorCurveBinding b) => b.type != configReg.type);
		last.AddSeparator("");
		if (!num)
		{
			last.AddItem(new GUIContent("Set Property Name"), on: false, delegate
			{
				string[] array = ListVisitor(configReg.type);
				if (SelectInitializer() != null && configReg.type.RemoveResolver<Renderer>())
				{
					Transform transform = SelectInitializer().transform.Find(configReg.path);
					if (transform != null)
					{
						IEnumerable<string> second = transform.GetComponents<Renderer>().SelectMany(_003C_003Ec.watcherInitializer.CancelObserver).Select(_003C_003Ec.watcherInitializer.CountObserver)
							.SelectMany(CollectVisitor)
							.Distinct();
						array = array.Concat(second).ToArray();
					}
				}
				StatusServer<string> statusServer = new StatusServer<string>("Property Name", array, _003C_003Ec.watcherInitializer.DisableObserver, delegate(int i, string s)
				{
					foreach (EditorCurveBinding item in temp.Distinct())
					{
						CheckAlgo(DefineInitializer(), item, new EditorCurveBinding
						{
							type = item.type,
							path = item.path,
							propertyName = s
						});
					}
				});
				statusServer.GetConnection(_003C_003Ec.watcherInitializer.InsertObserver);
				statusServer.RunConnection(token);
			});
		}
		else
		{
			last.AddDisabledItem("Set Property Name".CreateResolver());
		}
		last.AddItem("Set Type".CreateResolver(), on: false, delegate
		{
			GameObject gameObject = VerifyAlgo(checklast: false);
			IEnumerable<Type> enumerable = null;
			if (gameObject != null)
			{
				GameObject gameObject2 = (string.IsNullOrEmpty(configReg.path) ? gameObject : gameObject.transform.Find(configReg.path)?.gameObject);
				if (gameObject2 != null)
				{
					enumerable = new Type[1] { typeof(GameObject) }.Concat(gameObject2.GetComponents<Component>().Select(_003C_003Ec.watcherInitializer.RestartObserver)).Where(_003C_003Ec.watcherInitializer.QueryObserver).Distinct();
				}
			}
			if (enumerable == null)
			{
				enumerable = m_ManagerAnnotation.Where(VerifyVisitor);
			}
			StatusServer<Type> statusServer = new StatusServer<Type>("Type", enumerable, _003C_003Ec.watcherInitializer.AddObserver, delegate(int i, Type t)
			{
				string[] array = ListVisitor(t);
				if (array.Length != 0)
				{
					using (IEnumerator<EditorCurveBinding> enumerator = temp.Distinct().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							_003C_003Ec__DisplayClass483_1 _003C_003Ec__DisplayClass483_ = new _003C_003Ec__DisplayClass483_1();
							_003C_003Ec__DisplayClass483_.parserReg = enumerator.Current;
							CheckAlgo(DefineInitializer(), _003C_003Ec__DisplayClass483_.parserReg, new EditorCurveBinding
							{
								type = t,
								path = _003C_003Ec__DisplayClass483_.parserReg.path,
								propertyName = (array.FirstOrDefault(_003C_003Ec__DisplayClass483_.MapThread) ?? array[0])
							});
						}
						return;
					}
				}
				FindVisitor(t.Name + " has no animatable properties.");
			});
			statusServer.IncludeConnection(_003C_003Ec.watcherInitializer.InvokeObserver);
			statusServer.GetConnection(_003C_003Ec.watcherInitializer.FindObserver);
			statusServer.RunConnection(new Rect(token));
		});
	}

	private static void AnimationWindowDoAddCurveButtonPost(Rect rect)
	{
		if (!ConsumerAlgo.CallDefinition().aw_active || !ConsumerAlgo.CallDefinition().aw_enableGameObjectDND)
		{
			return;
		}
		GameObject m_ItemReg = SelectInitializer();
		AnimationClip m_ManagerReg = DefineInitializer();
		if (m_ItemReg == null || m_ManagerReg == null)
		{
			return;
		}
		Transform _SpecificationReg = m_ItemReg.transform;
		float num = (rect.width - 230f) / 2f;
		Rect spec = new Rect(rect.xMin + num, rect.yMin + 10f, rect.width - num * 2f, rect.height - 20f);
		ClassProperty.StartQueue(spec);
		rect = new Rect(0f, spec.y + spec.height + 4f, rect.width, EditorGUIUtility.singleLineHeight * 2f);
		Rect rect2 = new Rect(rect);
		rect2.height = 14f;
		Rect rect3 = rect2;
		using (new AnnotationPolicy(m_ItemReg == null))
		{
			GUI.Label(rect3, "[Drag & Drop GameObjects]", ClassProperty.CalcError()._VisitorObserver);
			ClassProperty.AwakeRules(rect, delegate(IEnumerable<GameObject> enu)
			{
				EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(m_ManagerReg);
				Undo.RecordObject(m_ManagerReg, "[CE] Add Curve");
				foreach (GameObject item in enu)
				{
					if (!FindVisitor(item.name + " is not a child of " + m_ItemReg.name, CustomLogType.Warning, !item.transform.IsChildOf(_SpecificationReg) && item != m_ItemReg))
					{
						string path = AnimationUtility.CalculateTransformPath(item.transform, m_ItemReg.transform);
						EditorCurveBinding tag = new EditorCurveBinding
						{
							type = typeof(GameObject),
							path = path,
							propertyName = "m_IsActive"
						};
						EditorCurveBinding editorCurveBinding = tag;
						if (!FindVisitor("Matching binding already exists!", CustomLogType.Warning, curveBindings.InitPredicate(editorCurveBinding, out tag)))
						{
							int num2 = (item.activeSelf ? 1 : 0);
							AnimationUtility.SetEditorCurve(DefineInitializer(), editorCurveBinding, new AnimationCurve(new Keyframe(0f, num2), new Keyframe(1f / m_ManagerReg.frameRate, num2)));
						}
					}
				}
				EditorUtility.SetDirty(m_ManagerReg);
			});
		}
	}

	private static void CheckAlgo(AnimationClip setup, EditorCurveBinding pol, EditorCurveBinding util)
	{
		if (pol == util)
		{
			return;
		}
		Undo.RecordObject(setup, "[CE] Modify Binding");
		if (!pol.isPPtrCurve)
		{
			EditorCurveBinding editorCurveBinding = AnimationUtility.GetCurveBindings(setup).FirstOrDefault((EditorCurveBinding b) => b.path == util.path && b.propertyName == util.propertyName && b.type == util.type);
			AnimationCurve animationCurve = ((!(editorCurveBinding != default(EditorCurveBinding))) ? null : AnimationUtility.GetEditorCurve(setup, editorCurveBinding));
			FindVisitor("Caution! Merging with pre-existing property.", CustomLogType.Warning, animationCurve != null && (bool)ConsumerAlgo.CallDefinition().aw_warnPropertyMerge);
			AnimationCurve animationCurve2 = AnimationUtility.GetEditorCurve(setup, pol);
			AnimationUtility.SetEditorCurve(setup, pol, null);
			if (animationCurve != null)
			{
				animationCurve2 = new AnimationCurve((from k in animationCurve2.keys.Concat(animationCurve.keys)
					group k by k.time into g
					select g.First()).ToArray());
			}
			AnimationUtility.SetEditorCurve(setup, util, animationCurve2);
			return;
		}
		EditorCurveBinding editorCurveBinding2 = AnimationUtility.GetObjectReferenceCurveBindings(setup).FirstOrDefault((EditorCurveBinding b) => b.path == util.path && b.propertyName == util.propertyName && b.type == util.type);
		ObjectReferenceKeyframe[] array = ((editorCurveBinding2 != default(EditorCurveBinding)) ? AnimationUtility.GetObjectReferenceCurve(setup, editorCurveBinding2) : null);
		FindVisitor("Caution! Merging with pre-existing property.", CustomLogType.Warning, array != null && (bool)ConsumerAlgo.CallDefinition().aw_warnPropertyMerge);
		ObjectReferenceKeyframe[] array2 = AnimationUtility.GetObjectReferenceCurve(setup, pol);
		AnimationUtility.SetObjectReferenceCurve(setup, pol, null);
		if (array != null)
		{
			array2 = (from f in array2.Concat(array)
				group f by f.time into g
				select g.First()).ToArray();
		}
		AnimationUtility.SetObjectReferenceCurve(setup, util, array2);
	}

	private static bool InterruptIfOverridingControllerPrefix()
	{
		return annotationVisitor == null;
	}

	private static bool ShouldUpdateAnimationSelectionPrefix()
	{
		return !annotationVisitor;
	}

	private static void GetEditorCurveValueTypePost()
	{
		m_VisitorVisitor = false;
	}

	private static void CurvesPopupFetchDataPost()
	{
		m_VisitorVisitor = false;
	}

	private static void GetEditorCurveValueTypePrefix()
	{
		m_VisitorVisitor = _AlgoVisitor;
	}

	private static void CurvesPopupFetchDataPrefix()
	{
		m_VisitorVisitor = _AlgoVisitor;
	}

	private static bool ShouldUpdateGameObjectSelectionPrefix(ref bool __result)
	{
		if (!wrapperVisitor)
		{
			return !annotationVisitor;
		}
		__result = true;
		return false;
	}

	private static bool AnimationWindowSelectionItemGetRootGameObjectPrefix(ref GameObject __result)
	{
		if (!m_VisitorVisitor)
		{
			return true;
		}
		__result = _AlgoVisitor;
		return false;
	}

	private static void PrepareAlgo()
	{
		rulesVisitor = ClassProperty.FillRules("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_QueueVisitor = ClassProperty.FillRules("UnityEditor.Graphs.Edge, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		errorVisitor = ClassProperty.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}

	private static void AssetAlgo()
	{
		SpecificationAlgo.MapReg(_StatusVisitor, "get_selectedEdgeColor", SpecificationAlgo.RevertReg<Color, bool>(SelectedTransitionColorPrefix));
		SpecificationAlgo.MapReg(_StatusVisitor, "get_defaultTransitionColor", SpecificationAlgo.RevertReg<Color, bool>(EntryTransitionColorPrefix));
		SpecificationAlgo.MapReg(_StatusVisitor, "get_selectorTransitionColor", SpecificationAlgo.RevertReg<Color, bool>(BaseTransitionColorPrefix));
		while (true)
		{
			Type type = ClassProperty.FillRules("UnityEditor.Graphs.Slot, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			SpecificationAlgo.CalcReg(m_QueueVisitor.GetConstructor(new Type[2] { type, type }), null, SpecificationAlgo.NewReg<Edge>(EdgeConstructorPost));
		}
	}

	private static bool SelectedTransitionColorPrefix(ref Color __result)
	{
		if (!ConsumerAlgo.CallDefinition().cosmeticTransitionsActive)
		{
			return true;
		}
		__result = ConsumerAlgo.CallDefinition().selectedTransitionColor.WriteDefinition();
		return false;
	}

	private static bool EntryTransitionColorPrefix(ref Color __result)
	{
		__result = ConsumerAlgo.CallDefinition().entryTransitionColor.WriteDefinition();
		return !ConsumerAlgo.CallDefinition().cosmeticTransitionsActive;
	}

	private static bool BaseTransitionColorPrefix(ref Color __result)
	{
		__result = ConsumerAlgo.CallDefinition().baseTransitionColor.WriteDefinition();
		return !ConsumerAlgo.CallDefinition().cosmeticTransitionsActive;
	}

	private static void EdgeConstructorPost(Edge __instance)
	{
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticTransitionsActive)
		{
			__instance.color = ConsumerAlgo.CallDefinition().normalTransitionColor.WriteDefinition();
		}
	}

	private static void AddNodePost(Node node)
	{
	}

	private static void DrawArrowsPrefix(ref Vector3[] edgePoints)
	{
		if (!m_HelperVisitor && !m_RecordVisitor)
		{
			return;
		}
		Vector3 vector = edgePoints[0];
		Vector3 vector2 = edgePoints[1];
		Vector3 vector3 = vector2 - vector;
		if (!(vector3.magnitude <= 1f))
		{
			Vector2 a = UpdateAlgo(vector, vector2);
			float magnitude = vector3.magnitude;
			float num = Vector2.Distance(a, vector2);
			float num2 = ConsumerAlgo.CallDefinition().arrowLerpRatio.ResetDefinition();
			if (m_HelperVisitor && m_ContextVisitor.Contains(vector2))
			{
				_ExpressionVisitor = true;
				float num3 = Mathf.Repeat((float)EditorApplication.timeSinceStartup / 2f, 1f);
				num2 = ((num3 > 1f) ? Mathf.Lerp(1f, -1f, num3 - 1f) : Mathf.Lerp(-1f, 1f, num3));
			}
			edgePoints[1] = vector2 + vector3 * num2 - vector3 * (num / magnitude) * 2f * num2;
		}
	}

	public static Vector2 UpdateAlgo(Vector2 asset, Vector2 map)
	{
		Vector2 vector = new Vector2(asset.x, asset.y);
		Vector2 vector2 = new Vector2(map.x, map.y);
		Vector2 to = vector2 - vector;
		float num = Vector2.SignedAngle(Vector3.up, to);
		if (num > -78.69f && !(num >= 78.69f))
		{
			goto IL_0088;
		}
		float num2 = num;
		Vector2 state;
		Vector2 attr;
		if (num2 <= 78.69f || !(num2 < 101.31f))
		{
			float num3 = num;
			if (!(num3 > 101.31f) && num3 >= -101.31f)
			{
				float num4 = num;
				if (num4 >= -78.69f || num4 <= -101.31f)
				{
					goto IL_0088;
				}
				state = vector2 + new Vector2(-100f, -20f);
				attr = vector2 + new Vector2(-100f, 20f);
			}
			else
			{
				state = vector2 + new Vector2(100f, 20f);
				attr = vector2 + new Vector2(-100f, 20f);
			}
		}
		else
		{
			state = vector2 + new Vector2(100f, 20f);
			attr = vector2 + new Vector2(100f, -20f);
		}
		goto IL_0168;
		IL_0168:
		ChangeAlgo(vector, vector2, state, attr, out var ident);
		return ident;
		IL_0088:
		state = vector2 + new Vector2(-100f, -20f);
		attr = vector2 + new Vector2(100f, -20f);
		goto IL_0168;
	}

	public static bool ChangeAlgo(Vector2 reference, Vector2 visitor, Vector2 state, Vector2 attr2, out Vector2 ident3)
	{
		ident3 = Vector2.zero;
		Vector2 normalized = (visitor - reference).normalized;
		Vector2 normalized2 = (attr2 - state).normalized;
		if (Mathf.Abs(Vector2.Dot(normalized, Vector2.Perpendicular(normalized2))) >= Mathf.Epsilon)
		{
			float num = Vector2.Dot(state - reference, Vector2.Perpendicular(normalized2)) / Vector2.Dot(normalized, Vector2.Perpendicular(normalized2));
			ident3 = reference + num * normalized;
			return true;
		}
		return false;
	}

	private static bool GraphGUIMajorGridColorPrefix(ref Color __result)
	{
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticGraphActive)
		{
			__result = ((!ControllerEditorWindow.PushTests()) ? ConsumerAlgo.CallDefinition().gridMajorLightColor : ConsumerAlgo.CallDefinition().gridMajorDarkColor).WriteDefinition();
			return false;
		}
		return true;
	}

	private static bool GraphGUIMinorGridColorPrefix(ref Color __result)
	{
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticGraphActive)
		{
			__result = (ControllerEditorWindow.PushTests() ? ConsumerAlgo.CallDefinition().gridMinorDarkColor : ConsumerAlgo.CallDefinition().gridMinorLightColor).WriteDefinition();
			return false;
		}
		return true;
	}

	private static void SortAlgo()
	{
		try
		{
			if (GUI.skin == null)
			{
				return;
			}
		}
		catch
		{
			return;
		}
		if (m_SetterVisitor == null)
		{
			m_SetterVisitor = ClassProperty.FillRules("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("graphBackground", BindingFlags.Static | BindingFlags.Public);
		}
		bool flag = !ConsumerAlgo.CallDefinition().cosmeticGraphActive;
		GUIStyle gUIStyle = new GUIStyle();
		Texture2D background;
		if (flag || !ConsumerAlgo.CallDefinition().graphBackgroundIsTexture)
		{
			if (connectionVisitor == null)
			{
				connectionVisitor = new Texture2D(1, 1);
			}
			connectionVisitor.SetPixel(0, 0, (!flag && (bool)ConsumerAlgo.CallDefinition().cosmeticGraphActive) ? ConsumerAlgo.CallDefinition().gridBackgroundColor.WriteDefinition() : ((Color)(object)ConsumerAlgo.CallDefinition().gridBackgroundColor.callbackAlgo));
			connectionVisitor.Apply();
			background = connectionVisitor;
		}
		else
		{
			background = ConsumerAlgo.CallDefinition().graphBackgroundTexture.UpdateDefinition<Texture2D>();
		}
		gUIStyle.normal.background = background;
		m_SetterVisitor.SetValue(null, gUIStyle);
	}

	private static void RegisterAlgo()
	{
		_ConsumerVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_AdapterVisitor = ClassProperty.FillRules("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_InterpreterVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationBlendTree.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_WatcherVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_CandidateVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_ProductVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		workerVisitor = _ConsumerVisitor.DisableList("RebuildGraph");
		m_PrototypeVisitor = _ConsumerVisitor.DisableList("AddBreadCrumb");
		_AdapterAnnotation = _ConsumerVisitor.DisableList("get_activeGraphGUI");
		interpreterAnnotation = m_ProductVisitor.InsertList("GetEdgePoints", new Type[1] { typeof(Edge) });
	}

	private static void LogoutAlgo()
	{
		SpecificationAlgo.MapReg(m_CandidateVisitor, "SetStateMachines", SpecificationAlgo.ViewReg<AnimatorStateMachine, AnimatorStateMachine, AnimatorStateMachine>(SetStateMachinesPost));
		SpecificationAlgo.MapReg(_ConsumerVisitor, "DoGraphBottomBar", null, new Action<Rect>(GraphGUIBottomBarPost).Method);
		SpecificationAlgo.MapReg(_WatcherVisitor, "OnGraphGUI", SpecificationAlgo.PrepareReg(OnGraphGUIPrefix), SpecificationAlgo.CreateReg(OnGraphGUIPost));
		SpecificationAlgo.MapReg(_WatcherVisitor, "HandleObjectDragging", null, null, SpecificationAlgo.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(HandleObjectDraggingTranspiler));
		SpecificationAlgo.MapReg(_WatcherVisitor, "HandleContextMenu", SpecificationAlgo.QueryTests<object, bool, object>(GraphGUIContextMenuPrefix), SpecificationAlgo.NewReg<bool>(GraphGUIContextMenuPost));
	}

	private static void SetStateMachinesPost(AnimatorStateMachine stateMachine, AnimatorStateMachine parent, AnimatorStateMachine root)
	{
		OrderInitializer(stateMachine);
		PrintMapper(root);
		InstantiateAnnotation();
		SortAlgo();
	}

	private static bool OnGraphGUIPrefix()
	{
		filterVisitor = true;
		if (poolVisitor)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		}
		poolVisitor = false;
		m_ContextVisitor.Clear();
		bool flag = ConsumerAlgo.CallDefinition().animateInboundEdges;
		bool flag2 = ConsumerAlgo.CallDefinition().animateOutboundEdges;
		bool flag3 = flag && flag2;
		m_RecordVisitor = ConsumerAlgo.CallDefinition().arrowLerpRatio.ResetDefinition() != 0f;
		m_HelperVisitor = (flag || flag2) && !m_MapperAnnotation.CalcRules();
		if (m_HelperVisitor)
		{
			try
			{
				foreach (SetterTests.TokenizerTests item in m_MapperAnnotation)
				{
					foreach (Vector3 item2 in (flag3 ? item.CheckPolicy().Concat(item.AssetPolicy()) : (flag ? item.CheckPolicy() : item.AssetPolicy())).Select((SetterTests.BaseTests e) => InitAnnotation(e)[1]))
					{
						m_ContextVisitor.Add(item2);
					}
				}
			}
			catch (System.Exception exception)
			{
				Selection.objects = null;
				UnityEngine.Debug.LogException(exception);
			}
		}
		return true;
	}

	private static void OnGraphGUIPost()
	{
		filterVisitor = false;
	}

	private static void GraphGUIContextMenuPrefix(object __instance, out bool __state, object ___m_EdgeGUI)
	{
		__state = true;
		Event current = Event.current;
		switch (current.type)
		{
		case EventType.ContextClick:
			if ((_Manager || m_Parser) && _ParserVisitor.Invoke(___m_EdgeGUI, null) != null)
			{
				__state = false;
				_AttributeVisitor = _FieldVisitor ^ current.shift;
				clientVisitor = (bool)ConsumerAlgo.CallDefinition().reverseModifiesValues ^ current.control;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Reverse Transitions"), on: false, DeleteAlgo);
				genericMenu.AddItem(new GUIContent("Redirect Transitions"), containerAnnotation, LoginAlgo);
				genericMenu.AddItem(new GUIContent("Replicate Transitions"), baseAnnotation, CreateAlgo);
				genericMenu.AddItem(new GUIContent("From\\To Any Transition"), on: false, PushAlgo);
				genericMenu.AddSeparator(string.Empty);
				genericMenu.AddItem(new GUIContent("(Replacing)"), _AttributeVisitor, CloneAlgo);
				genericMenu.ShowAsContext();
				current.Use();
			}
			break;
		case EventType.MouseDown:
			if ((current.control ^ (bool)ConsumerAlgo.CallDefinition().switchDoubleClick) && current.clickCount == 2)
			{
				Vector2 vector = current.mousePosition + new Vector2(-100f, -20f);
				AnimatorState animatorState = RevertMapper().AddState(ConsumerAlgo.CallDefinition().defaultState.name, vector.PostPredicate(10));
				QueryAlgo(animatorState);
				animatorState.motion = ConsumerAlgo.CallDefinition().defaultState.motion;
				current.Use();
			}
			break;
		case EventType.KeyDown:
			switch (current.keyCode)
			{
			case KeyCode.Escape:
				m_ClassAnnotation = false;
				containerAnnotation = false;
				baseAnnotation = false;
				SetterTests.RefTests.PopPolicy()?.Repaint();
				break;
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				if (!baseAnnotation)
				{
					if (!containerAnnotation)
					{
						if (m_ClassAnnotation)
						{
							MoveAlgo();
						}
					}
					else
					{
						LoginAlgo();
					}
				}
				else
				{
					CreateAlgo();
				}
				SetterTests.RefTests.PopPolicy()?.Repaint();
				break;
			case KeyCode.A:
				if (current.control)
				{
					if (current.shift)
					{
						DestroyAlgo();
					}
					else
					{
						InsertAlgo();
					}
					current.Use();
				}
				break;
			}
			break;
		}
	}

	private static void GraphGUIContextMenuPost(bool __state)
	{
		if (__state && Event.current.type == EventType.ContextClick)
		{
			m_SingletonVisitor.AddSeparator(string.Empty);
			m_SingletonVisitor.AddItem(new GUIContent(((_TestsAnnotation.Count <= 0) ? "Select" : "Deselect") + " All Transitions"), on: false, DestroyAlgo);
			m_SingletonVisitor.AddItem(new GUIContent(((m_AlgoAnnotation.Count > 0) ? "Deselect" : "Select") + " All States"), on: false, InsertAlgo);
			m_SingletonVisitor.ShowAsContext();
		}
	}

	private static void GraphGUIBottomBarPost(Rect nameRect)
	{
		if (baseAnnotation || containerAnnotation || m_ClassAnnotation)
		{
			Rect screenRect = new Rect(nameRect);
			screenRect.y -= screenRect.height + 5f;
			GUILayout.BeginArea(screenRect);
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.Label(baseAnnotation ? "Replicating Transitions" : ((!containerAnnotation) ? "Making Transitions" : "Redirecting Transitions"), ClassProperty.CalcError()._StructProcessor);
			}
			GUILayout.EndArea();
		}
		if ((bool)LogoutMapper())
		{
			if (!ConsumerAlgo.CallDefinition().hasPingedController)
			{
				GUI.Label(nameRect, "Click to highlight Controller", ClassProperty.CalcError().annotationObserver);
			}
			if (ClassProperty.DefineQueue(nameRect))
			{
				EditorGUIUtility.PingObject(LogoutMapper());
				ConsumerAlgo.CallDefinition().hasPingedController.ExcludeDefinition(excludeparam: true);
			}
		}
		if (_InfoVisitor)
		{
			_InfoVisitor = false;
			EditorWindow obj = SetterTests.RefTests.PopPolicy();
			_ConsumerVisitor.GetMethod("FrameAutofit").Invoke(obj, Array.Empty<object>());
		}
		if (_ExpressionVisitor)
		{
			_ExpressionVisitor = false;
			SetterTests.RefTests.PopPolicy().Repaint();
		}
		if (_SystemVisitor)
		{
			_SystemVisitor = false;
			PatchAlgo();
		}
	}

	private static void PatchAlgo()
	{
		InterruptAlgo(wantfirst: false);
	}

	private static void InterruptAlgo(bool wantfirst)
	{
		EditorWindow editorWindow = SetterTests.RefTests.PopPolicy();
		if (editorWindow != null)
		{
			workerVisitor.Invoke(editorWindow, new object[1] { wantfirst });
		}
	}

	[SpecialName]
	private static string ValidateInitializer()
	{
		return ConsumerAlgo.CallDefinition().categoryBaseName;
	}

	[SpecialName]
	private static bool RateInitializer()
	{
		return writerVisitor == LayerViewViewType.DefaultView;
	}

	[SpecialName]
	private static bool GetInitializer()
	{
		return writerVisitor == LayerViewViewType.CategoryByName;
	}

	[SpecialName]
	private static bool IncludeInitializer()
	{
		return writerVisitor == LayerViewViewType.CategoryByTag;
	}

	[SpecialName]
	private static float CloneInitializer()
	{
		return (!ConsumerAlgo.CallDefinition().layerCompactView) ? 40 : 20;
	}

	[SpecialName]
	private static ReorderableList ReflectInitializer()
	{
		if (!RateInitializer())
		{
			return m_GetterVisitor;
		}
		return m_InterceptorVisitor;
	}

	[SpecialName]
	private static List<UnityEditor.Animations.AnimatorControllerLayer> CreateInitializer()
	{
		return m_InterceptorVisitor.list.Cast<UnityEditor.Animations.AnimatorControllerLayer>().ToList();
	}

	[SpecialName]
	private static string PushInitializer()
	{
		return ConsumerAlgo.CallDefinition().categoryDelimiter;
	}

	private static void ManageAlgo()
	{
		_ReaderVisitor = ClassProperty.FillRules("UnityEditor.Graphs.LayerControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_BridgeVisitor = ClassProperty.FillRules("UnityEditor.Graphs.LayerSettingsWindow, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_StrategyVisitor = _ReaderVisitor.RestartList("m_LayerScroll");
		_CustomerVisitor = _ReaderVisitor.DisableList("OnRemoveLayer");
		m_DatabaseVisitor = _ReaderVisitor.DisableList("RenameEnd");
		m_IdentifierVisitor = _ReaderVisitor.RestartList("m_LayerList");
		m_AttrVisitor = _ReaderVisitor.RestartList("m_Host");
		registryVisitor = _ReaderVisitor.DisableList("KeyboardHandling");
		_ExporterVisitor = _BridgeVisitor.DisableList("ShowAtPosition");
		m_DispatcherVisitor = _ConsumerVisitor.RestartList("m_AnimatorController");
	}

	private static void PrintAlgo()
	{
		SpecificationAlgo.MapReg(_ReaderVisitor, "OnToolbarGUI", SpecificationAlgo.CreateReg(smethod_0), SpecificationAlgo.CreateReg(OnToolbarGUIPost));
		SpecificationAlgo.MapReg(_ReaderVisitor, "OnGUI", SpecificationAlgo.CheckReg<Rect, bool>(smethod_1), SpecificationAlgo.CreateReg(smethod_2));
		SpecificationAlgo.MapReg(_ReaderVisitor, "Init", null, SpecificationAlgo.PushReg<object, ReorderableList>(LayerViewInitPost));
		SpecificationAlgo.MapReg(_ReaderVisitor, "OnDrawLayer", SpecificationAlgo.SearchReg<Rect, int, bool>(OnDrawLayerPrefix));
		SpecificationAlgo.MapReg(_ReaderVisitor, "OnRemoveLayer", null, SpecificationAlgo.CreateReg(DisableMapper));
		SpecificationAlgo.MapReg(_ReaderVisitor, "RenameEnd", SpecificationAlgo.OrderTests<string>(LayerViewRenameEndPrefix), SpecificationAlgo.NewReg<string>(LayerViewRenameEndPost));
		SpecificationAlgo.MapReg(_ReaderVisitor, "OnSelectLayer", SpecificationAlgo.OrderTests<int>(OnSelectLayerPrefix), SpecificationAlgo.NewReg<int>(OnSelectLayerPost));
		SpecificationAlgo.MapReg(_ReaderVisitor, "ResetUI", null, null, SpecificationAlgo.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(LayerResetTranspiler));
		SpecificationAlgo.MapReg(_ConsumerVisitor, "AddNewLayer", SpecificationAlgo.CheckReg<object, bool>(AddNewLayerPrefix));
		SpecificationAlgo.MapReg(typeof(ReorderableList), "DoLayoutList", SpecificationAlgo.PrepareReg(RevertAlgo));
	}

	private static void smethod_0()
	{
		string text = writerVisitor.ToString().NewResolver();
		bool flag = ConsumerAlgo.CallDefinition().displayCategoryView;
		bool flag2 = ConsumerAlgo.CallDefinition().displayLayerCompactView;
		Rect var = GUILayoutUtility.GetLastRect().ForgotResolver(-10f).InterruptResolver((!flag) ? 0f : (text.PushResolver() + 24f))
			.ManageResolver(20f);
		Rect rect = var.ViewResolver().InterruptResolver(flag2 ? 20 : 0);
		Rect item = rect.ViewResolver().InterruptResolver(20f);
		if (flag && GUI.Button(var, text.CreateResolver(), EditorStyles.toolbarDropDown))
		{
			SearchAlgo();
		}
		if (flag2)
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, !ConsumerAlgo.CallDefinition().layerCompactView, Color.gray))
			{
				if (ClassProperty.QueryQueue(rect, new GUIContent(ClassProperty.DestroyError().m_SingletonProcessor)
				{
					tooltip = "Compact View"
				}, ClassProperty.CalcError().configProcessor))
				{
					ConsumerAlgo.CallDefinition().layerCompactView.InsertDefinition();
					DisableMapper();
				}
			}
		}
		if (!flag)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(writerVisitor == LayerViewViewType.DefaultView))
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, !ConsumerAlgo.CallDefinition().sortCategoryViewLayers, Color.gray))
			{
				if (ClassProperty.QueryQueue(item, ClassProperty.DestroyError()._MappingProcessor, ClassProperty.CalcError().configProcessor))
				{
					ConsumerAlgo.CallDefinition().sortCategoryViewLayers.InsertDefinition();
					DisableMapper();
				}
			}
		}
	}

	private static void OnToolbarGUIPost()
	{
		Rect rect = GUILayoutUtility.GetRect(20f, 18f);
		rect.x -= 1f;
		rect.y += 1f;
		if (!rect.Contains(Event.current.mousePosition))
		{
			_TagVisitor = true;
		}
		else if (_TagVisitor)
		{
			_TagVisitor = false;
			RevertVisitor();
		}
		int num = EditorGUI.Popup(rect, -1, m_PrinterVisitor);
		switch (num)
		{
		case -1:
			break;
		default:
			OrderAlgo(requestVisitor[num], LogoutMapper());
			break;
		case 0:
			LogoutMapper().VisitPredicate("New Layer", ConsumerAlgo.CallDefinition().defaultLayerWeight, ConsumerAlgo.CallDefinition().defaultLayerMask.UpdateDefinition<AvatarMask>());
			break;
		}
	}

	private static void SearchAlgo()
	{
		GenericMenu genericMenu = new GenericMenu();
		foreach (LayerViewViewType schemaReg in Enum.GetValues(typeof(LayerViewViewType)))
		{
			genericMenu.AddItem(schemaReg.ToString().NewResolver().DeleteResolver(tokenneeded: true), on: false, delegate
			{
				writerVisitor = schemaReg;
				DisableMapper();
			});
		}
		genericMenu.ShowAsContext();
	}

	private static bool smethod_1(Rect rect)
	{
		LayerViewViewType layerViewViewType = writerVisitor;
		if (layerViewViewType == LayerViewViewType.DefaultView || (uint)(layerViewViewType - 1) > 1u)
		{
			eventVisitor = false;
			return true;
		}
		RestartMapper();
		registryVisitor.Invoke(ReadAnnotation(), null);
		importerVisitor = GUILayout.BeginScrollView(importerVisitor);
		m_GetterVisitor.DoLayoutList();
		EditorGUILayout.HelpBox("Category view may have missing interactions or unhandled issues.", MessageType.None);
		GUILayout.EndScrollView();
		eventVisitor = true;
		return true;
	}

	private static bool RevertAlgo()
	{
		return !eventVisitor;
	}

	private static void smethod_2()
	{
		eventVisitor = false;
	}

	private static void LayerViewInitPost(object __instance, ReorderableList ___m_LayerList)
	{
		m_StubVisitor = __instance;
		m_InterceptorVisitor = ___m_LayerList;
		PublishAnnotation();
	}

	private static bool OnDrawLayerPrefix(ref Rect rect, ref int index)
	{
		if (m_InterceptorVisitor != null)
		{
			UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = m_InterceptorVisitor.list[index] as UnityEditor.Animations.AnimatorControllerLayer;
			OrderMapper(rect, index);
			if ((bool)ConsumerAlgo.CallDefinition().displayLayerIndex && RateInitializer())
			{
				Rect rect2 = rect.ForgotResolver(-20f).InterruptResolver(22f);
				if (!ConsumerAlgo.CallDefinition().layerCompactView)
				{
					rect2 = rect2.StopResolver(10f).UpdateResolver(-10f);
				}
				else
				{
					EditorGUI.DrawRect(rect2 = rect2.InterruptResolver(18f), new Color(0.2f, 0.2f, 0.2f));
				}
				GUI.Label(rect2, index.ToString(), ClassProperty.CalcError()._VisitorObserver);
			}
			if (animatorControllerLayer.stateMachine == null)
			{
				float x = 20f + animatorControllerLayer.name.PushResolver();
				GUI.Label(new Rect(x, rect.yMin + 6f, 20f, 14f), new GUIContent(ClassProperty.DestroyError().issuerProcessor)
				{
					tooltip = "Statemachine is Null! This layer will cause issues!"
				});
			}
			if ((bool)ConsumerAlgo.CallDefinition().layerCompactView)
			{
				if (!indexerVisitor.ReflectSerializer() || indexerVisitor.CreateSerializer() != index || indexerVisitor.ViewSerializer())
				{
					float num = ((index == 0) ? 1f : ((!AddAnnotation()) ? animatorControllerLayer.defaultWeight : InvokeAnnotation().GetLayerWeight(index)));
					bool flag = num < 1f;
					bool flag2 = num == 0f;
					float num2 = ((!flag) ? 1f : Mathf.Lerp(0.5f, 1f, num));
					using (new TemplateThread(TemplateThread.ColoringType.FG, flag, new Color(num2, num2, num2)))
					{
						GUI.Label(rect, animatorControllerLayer.name, EditorStyles.label);
					}
					Rect res = rect;
					GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
					{
						margin = new RectOffset(),
						padding = new RectOffset(),
						alignment = TextAnchor.MiddleCenter
					};
					Rect rect3 = res.PatchResolver(16f, isserv: true);
					if (ClassProperty.QueryQueue(rect3.StopResolver(2f), ClassProperty.DestroyError()._ParamProcessor, ClassProperty.CalcError().configProcessor))
					{
						_ExporterVisitor.Invoke(null, new object[4]
						{
							rect3,
							animatorControllerLayer,
							index,
							LogoutMapper()
						});
					}
					if (animatorControllerLayer.syncedLayerIndex >= 0)
					{
						bool syncedLayerAffectsTiming = animatorControllerLayer.syncedLayerAffectsTiming;
						GUI.Label(text: syncedLayerAffectsTiming ? "S+T" : "S", position: syncedLayerAffectsTiming ? res.PatchResolver(28f, isserv: true) : res.PatchResolver(14f, isserv: true), style: style);
					}
					if (animatorControllerLayer.iKPass)
					{
						GUI.Label(res.PatchResolver(14f, isserv: true), "IK", style);
					}
					if (animatorControllerLayer.blendingMode == UnityEditor.Animations.AnimatorLayerBlendingMode.Additive)
					{
						GUI.Label(res.PatchResolver(14f, isserv: true), "A", style);
					}
					if (!animatorControllerLayer.avatarMask.ConcatRules())
					{
						GUI.Label(res.PatchResolver(14f, isserv: true), "M", style);
					}
					if (flag && !flag2)
					{
						GUI.Label(res.PatchResolver(32f, isserv: true, 2f, isvisitor3: true).StopResolver(-1f), num.ToString("F2"), ClassProperty.CalcError().algoObserver);
					}
					if (ClassProperty.VerifyQueue())
					{
						CompareMapper();
					}
				}
				else
				{
					if (rect.width >= 0f && !(rect.height < 0f))
					{
						rect.x -= 2f;
						indexerVisitor.IncludeSerializer(rect);
					}
					if (!indexerVisitor.OnGUI())
					{
						m_DatabaseVisitor.Invoke(ReadAnnotation(), null);
					}
				}
				return false;
			}
			return true;
		}
		return true;
	}

	private static void LayerViewRenameEndPrefix(out string __state)
	{
		if (indexerVisitor.RateSerializer() != null || indexerVisitor.ConnectSerializer() != null)
		{
			string text = indexerVisitor.WriteSerializer();
			string text2 = indexerVisitor.ListSerializer();
			__state = ((!(text2 != text)) ? "" : text2);
		}
		else
		{
			__state = "";
		}
	}

	private static void LayerViewRenameEndPost(string __state)
	{
		if (RateInitializer())
		{
			return;
		}
		if (!__state.ResetResolver() && GetInitializer())
		{
			DisableMapper();
			ImporterMapper importerMapper = listenerVisitor;
			listenerVisitor = m_ParamsVisitor.ConnectTests(__state);
			if (importerMapper != listenerVisitor)
			{
				AddMapper();
				int num = listenerVisitor.listenerMapper.FindIndex((EventMapper l) => l._InfoMapper.name == __state);
				if (num >= 0)
				{
					m_GetterVisitor.index = num;
				}
			}
		}
		m_GetterVisitor.GrabKeyboardFocus();
	}

	private static void OrderMapper(Rect first, int cfgY)
	{
		Event m_ProxyReg = Event.current;
		if (!m_InterceptorVisitor.HasKeyboardControl())
		{
			ReorderableList getterVisitor = m_GetterVisitor;
			if (getterVisitor == null || !getterVisitor.HasKeyboardControl())
			{
				goto IL_0035;
			}
		}
		EventType type = m_ProxyReg.type;
		if (type != EventType.KeyDown)
		{
			if (type == EventType.ExecuteCommand)
			{
				m_ConfigVisitor = m_InterceptorVisitor.index;
				m_MessageVisitor = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(m_AttrVisitor.GetValue(ReadAnnotation()));
				if (m_ProxyReg.commandName == "Copy")
				{
					m_ProxyReg.Use();
					PostMapper();
				}
				else if (!(m_ProxyReg.commandName == "Paste"))
				{
					if (m_ProxyReg.commandName == "Duplicate")
					{
						m_ProxyReg.Use();
						SetMapper();
					}
				}
				else
				{
					m_ProxyReg.Use();
					SetupMapper();
				}
			}
		}
		else if (m_ProxyReg.keyCode != KeyCode.F2)
		{
			char character = m_ProxyReg.character;
			if (character != 0)
			{
				bool flag;
				ReorderableList reorderableList = ((!(flag = writerVisitor == LayerViewViewType.DefaultView)) ? m_GetterVisitor : m_InterceptorVisitor);
				int count = reorderableList.count;
				int num = -1;
				for (int i = 0; i < count; i++)
				{
					int num2 = (int)Mathf.Repeat(i + reorderableList.index + 1, count);
					if (char.ToLower(DefineMapper(num2).name[0]) == character)
					{
						num = num2;
						break;
					}
				}
				if (num >= 0)
				{
					reorderableList.index = num;
					float y = (float)num * CloneInitializer();
					m_StrategyVisitor.SetValue(ReadAnnotation(), new Vector2(0f, y));
					if (!flag)
					{
						importerVisitor = new Vector2(0f, y);
					}
					m_ProxyReg.Use();
				}
			}
		}
		else
		{
			m_ProxyReg.Use();
			int index = m_InterceptorVisitor.index;
			string text = ((UnityEditor.Animations.AnimatorControllerLayer)m_InterceptorVisitor.list[index]).name;
			indexerVisitor.TestSerializer(text, index, 0.1f);
		}
		goto IL_0035;
		IL_0035:
		if (m_ProxyReg.type != EventType.MouseUp || m_ProxyReg.button != 1 || !first.Contains(m_ProxyReg.mousePosition))
		{
			return;
		}
		m_ConfigVisitor = cfgY;
		m_MessageVisitor = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(m_AttrVisitor.GetValue(ReadAnnotation()));
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Delete"), on: false, CompareMapper);
		genericMenu.AddItem(new GUIContent("Duplicate"), on: false, SetMapper);
		genericMenu.AddItem(new GUIContent("Copy"), on: false, PostMapper);
		genericMenu.AddItem(new GUIContent("Paste"), on: false, (collectionVisitor != null) ? new GenericMenu.MenuFunction(SetupMapper) : null);
		genericMenu.AddSeparator("");
		UnityEditor.Animations.AnimatorControllerLayer structReg = VisitMapper();
		string[] source = ReadMapper(structReg).ToArray();
		if (creatorVisitor == null)
		{
			InsertMapper();
		}
		string[] array = creatorVisitor;
		foreach (string m_StateReg in array)
		{
			bool _GlobalReg = source.Contains(m_StateReg);
			genericMenu.AddItem(("Category Tag/" + m_StateReg).DeleteResolver(tokenneeded: true), _GlobalReg, delegate
			{
				string text2 = "_category:" + m_StateReg;
				if (_GlobalReg)
				{
					structReg.ReadPredicate(text2);
				}
				else
				{
					structReg.StartPredicate(text2);
				}
				EditorUtility.SetDirty(LogoutMapper());
				DisableMapper();
			});
		}
		genericMenu.AddItem("Category Tag/[New Category]".DeleteResolver(tokenneeded: true), on: false, delegate
		{
			QuickInputWindow quickInputWindow = QuickInputWindow.CreateHelper("New Category", ClassProperty.CheckRules<QuickInputWindow.FieldType>(QuickInputWindow.FieldType.String), ClassProperty.CheckRules<GUIContent>("Category Name".DeleteResolver(tokenneeded: true)), delegate(object[] results)
			{
				structReg.StartPredicate("_category:" + (string)results[0]);
				DisableMapper();
			}, _003C_003Ec.watcherInitializer.ReadObserver);
			quickInputWindow.NewHelper(0, ClassProperty.ChangeRules("New Category", creatorVisitor));
			quickInputWindow.CollectHelper(m_ProxyReg.mousePosition);
		});
		genericMenu.AddSeparator("");
		if (m_ConfigVisitor == 0)
		{
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Masks"), on: false, MoveMapper);
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/From Animator"), on: false, ConcatMapper);
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/Generic"), on: false, ComputeMapper);
		}
		genericMenu.AddItem(new GUIContent("Build Mask/From Animator"), on: false, PublishMapper);
		genericMenu.AddItem(new GUIContent("Build Mask/Generic"), on: false, EnableMapper);
		genericMenu.ShowAsContext();
		m_ContainerVisitor = Event.current.mousePosition;
		m_ProxyReg.Use();
	}

	private static void OnSelectLayerPrefix(out int __state)
	{
		__state = SetupInitializer();
	}

	private static void OnSelectLayerPost(int __state)
	{
		if (__state != SetupInitializer())
		{
			_InfoVisitor = ConsumerAlgo.CallDefinition().autoFrameLayer;
		}
	}

	private static bool AddNewLayerPrefix(object __instance)
	{
		UnityEditor.Animations.AnimatorController animatorController = (UnityEditor.Animations.AnimatorController)m_DispatcherVisitor.GetValue(__instance);
		string text = "New Layer";
		if (writerVisitor == LayerViewViewType.CategoryByName && listenerVisitor != m_ParamsVisitor && listenerVisitor.CustomizeTests() != ValidateInitializer())
		{
			text = listenerVisitor.CustomizeTests() + PushInitializer() + text;
		}
		text = animatorController.MakeUniqueLayerName(text);
		UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = animatorController.VisitPredicate(text, ConsumerAlgo.CallDefinition().defaultLayerWeight, ConsumerAlgo.CallDefinition().defaultLayerMask.UpdateDefinition<AvatarMask>());
		_ConsumerVisitor.QueryList("selectedLayerIndex").SetValue(__instance, animatorController.layers.Length - 1);
		object value = _ConsumerVisitor.RestartList("m_LayerEditor").GetValue(__instance);
		m_StrategyVisitor.SetValue(value, new Vector2(0f, animatorController.layers.Length * 40));
		animatorControllerLayer.stateMachine.entryPosition = ConsumerAlgo.CallDefinition().defaultEntryPosition.DeleteDefinition();
		animatorControllerLayer.stateMachine.anyStatePosition = ConsumerAlgo.CallDefinition().defaultAnyPosition.DeleteDefinition();
		animatorControllerLayer.stateMachine.exitPosition = ConsumerAlgo.CallDefinition().defaultExitPosition.DeleteDefinition();
		if (writerVisitor == LayerViewViewType.CategoryByTag && listenerVisitor != m_ParamsVisitor && listenerVisitor.CustomizeTests() != ValidateInitializer())
		{
			animatorControllerLayer.StartPredicate("_category:" + listenerVisitor.CustomizeTests());
		}
		DisableMapper();
		return false;
	}

	private static IEnumerable<CodeInstruction> LayerResetTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		int num = list.Count - 1;
		while (num > 1)
		{
			if (list[num].operand == null || !(list[num].opcode.Name == "stfld") || !(list[num].operand.ToString() == "UnityEngine.Vector2 m_LayerScroll"))
			{
				num--;
				continue;
			}
			list.RemoveRange(num - 2, 3);
			break;
		}
		return list.AsEnumerable();
	}

	private static void CompareMapper()
	{
		_CustomerVisitor.Invoke(ReadAnnotation(), new object[1] { m_ConfigVisitor });
	}

	private static void SetMapper()
	{
		UnityEditor.Animations.AnimatorControllerLayer item = ClassProperty.CalculatePredicate(m_MessageVisitor.layers[m_ConfigVisitor], m_MessageVisitor);
		if (m_ConfigVisitor < m_MessageVisitor.layers.Length - 2)
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = m_MessageVisitor.layers;
			ArrayUtility.Insert(ref array, m_ConfigVisitor + 1, item);
			ArrayUtility.RemoveAt(ref array, array.Length - 1);
			m_MessageVisitor.layers = array;
		}
	}

	private static void PostMapper()
	{
		collectionVisitor = m_MessageVisitor.layers[m_ConfigVisitor];
	}

	private static void SetupMapper()
	{
		if (collectionVisitor != null)
		{
			UnityEditor.Animations.AnimatorControllerLayer item = ClassProperty.CalculatePredicate(collectionVisitor, m_MessageVisitor);
			if (m_ConfigVisitor < m_MessageVisitor.layers.Length - 2)
			{
				UnityEditor.Animations.AnimatorControllerLayer[] array = m_MessageVisitor.layers;
				ArrayUtility.Insert(ref array, m_ConfigVisitor + 1, item);
				ArrayUtility.RemoveAt(ref array, array.Length - 1);
				m_MessageVisitor.layers = array;
			}
		}
	}

	private static void EnableMapper()
	{
		CancelMapper();
	}

	private static void PublishMapper()
	{
		if (ControllerEditorWindow._CallbackMapper)
		{
			uint num3 = default(uint);
			while (true)
			{
				int num;
				int num2;
				if ((bool)ControllerEditorWindow.m_AdvisorMapper)
				{
					num = 1586421337;
					num2 = 1586421337;
				}
				else
				{
					num = 1658763188;
					num2 = 1658763188;
				}
				switch ((num3 = (uint)(num ^ ((int)num3 * -1915708665) ^ -180051237)) % 5)
				{
				case 1u:
				case 3u:
					continue;
				default:
					return;
				case 0u:
					CancelMapper(ControllerEditorWindow.m_AdvisorMapper.transform);
					return;
				case 2u:
					break;
				}
				break;
			}
		}
		ValidateAnnotation(PopMapper);
	}

	private static void PopMapper(object[] ident)
	{
		if (!RateAnnotation(ident[0] == null, "No Animator was given!"))
		{
			ControllerEditorWindow.m_AdvisorMapper = (Animator)ident[0];
			ControllerEditorWindow._CallbackMapper = (bool)ident[2];
			CancelMapper(ControllerEditorWindow.m_AdvisorMapper.transform);
		}
	}

	private static void ComputeMapper()
	{
		CountMapper(null);
	}

	private static void MoveMapper()
	{
		string text = string.Concat(ConsumerAlgo.CallDefinition().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
		ClassProperty.RemoveList(text);
		AvatarMask avatarMask = ClassProperty.AddRules(m_MessageVisitor);
		if ((bool)avatarMask)
		{
			AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + m_MessageVisitor.name + ".mask"));
			UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
			layers[0].avatarMask = avatarMask;
			m_MessageVisitor.layers = layers;
			EditorUtility.SetDirty(m_MessageVisitor);
		}
	}

	private static void ConcatMapper()
	{
		if (ControllerEditorWindow._CallbackMapper && (bool)ControllerEditorWindow.m_AdvisorMapper)
		{
			CountMapper(ControllerEditorWindow.m_AdvisorMapper.transform);
		}
		else
		{
			ValidateAnnotation(CallMapper);
		}
	}

	private static void CallMapper(object[] v)
	{
		if (!RateAnnotation(v[0] == null, "No Animator was given!"))
		{
			ControllerEditorWindow.m_AdvisorMapper = (Animator)v[0];
			ControllerEditorWindow._CallbackMapper = (bool)v[2];
			CountMapper(ControllerEditorWindow.m_AdvisorMapper.transform);
		}
	}

	private static void CancelMapper(Transform setup = null)
	{
		UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = m_MessageVisitor.layers[m_ConfigVisitor];
		AvatarMask avatarMask = ClassProperty.FindRules(animatorControllerLayer, setup);
		string text = string.Concat(ConsumerAlgo.CallDefinition().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
		ClassProperty.RemoveList(text);
		AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + animatorControllerLayer.name + ".mask"));
		UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
		layers[m_ConfigVisitor].avatarMask = avatarMask;
		m_MessageVisitor.layers = layers;
	}

	private static void CountMapper(Transform task)
	{
		string text = string.Concat(ConsumerAlgo.CallDefinition().saveFolder, "/Generated Masks/", m_MessageVisitor.name);
		ClassProperty.RemoveList(text);
		AvatarMask avatarMask = ClassProperty.InvokeRules(m_MessageVisitor, task);
		AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + m_MessageVisitor.name + ".mask"));
		UnityEditor.Animations.AnimatorControllerLayer[] layers = m_MessageVisitor.layers;
		layers[0].avatarMask = avatarMask;
		m_MessageVisitor.layers = layers;
		EditorUtility.SetDirty(m_MessageVisitor);
	}

	private static void DisableMapper()
	{
		InsertMapper();
		if (writerVisitor == LayerViewViewType.DefaultView)
		{
			return;
		}
		try
		{
			if (LogoutMapper() == null)
			{
				return;
			}
			UnityEditor.Animations.AnimatorControllerLayer[] layers = LogoutMapper().layers;
			string text = listenerVisitor?.CustomizeTests();
			m_ParamsVisitor = new ImporterMapper("Root", "Root");
			LayerViewViewType layerViewViewType = writerVisitor;
			if (layerViewViewType == LayerViewViewType.CategoryByName)
			{
				for (int i = 0; i < layers.Length; i++)
				{
					UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = layers[i];
					m_ParamsVisitor.ResetTests(animatorControllerLayer.name, animatorControllerLayer, i);
				}
			}
			else if (layerViewViewType == LayerViewViewType.CategoryByTag)
			{
				for (int j = 0; j < layers.Length; j++)
				{
					UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer2 = layers[j];
					string[] array = ReadMapper(animatorControllerLayer2).ToArray();
					if (array.Any())
					{
						string[] array2 = array;
						foreach (string text2 in array2)
						{
							m_ParamsVisitor.ResetTests(text2 + PushInitializer() + "DUMMY", animatorControllerLayer2, j);
						}
					}
					else
					{
						m_ParamsVisitor.ResetTests(ValidateInitializer(), animatorControllerLayer2, j);
					}
				}
			}
			Stack<ImporterMapper> stack = new Stack<ImporterMapper>();
			stack.Push(m_ParamsVisitor);
			while (stack.Count > 0)
			{
				ImporterMapper importerMapper = stack.Pop();
				foreach (ImporterMapper item in importerMapper.paramsMapper)
				{
					stack.Push(item);
				}
				importerMapper.paramsMapper.Sort((ImporterMapper c1, ImporterMapper c2) => string.Compare(c1.requestMapper, c2.requestMapper, StringComparison.Ordinal));
				if ((bool)ConsumerAlgo.CallDefinition().sortCategoryViewLayers)
				{
					importerMapper.listenerMapper.Sort((EventMapper l1, EventMapper l2) => string.Compare(l1._InfoMapper.name, l2._InfoMapper.name, StringComparison.Ordinal));
				}
				ImporterMapper getterMapper = importerMapper.m_GetterMapper;
				if (getterMapper != null)
				{
					importerMapper.paramsMapper.Remove(getterMapper);
					importerMapper.paramsMapper.Add(getterMapper);
				}
			}
			listenerVisitor = ((!text.ResetResolver()) ? (m_ParamsVisitor.CalculateTests(text) ?? m_ParamsVisitor) : m_ParamsVisitor);
			AddMapper();
		}
		catch (System.Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		m_GetterVisitor.GrabKeyboardFocus();
	}

	private static void InsertMapper()
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = LogoutMapper().layers;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in layers.SelectMany(ReadMapper))
		{
			hashSet.Add(item);
		}
		creatorVisitor = hashSet.OrderBy((string n) => n).ToArray();
	}

	private static void RestartMapper()
	{
		bool _ProducerReg = true;
		m_ParamsVisitor?.MapTests(listenerVisitor.CustomizeTests(), delegate(ImporterMapper c)
		{
			ImporterMapper[] array = c.paramsMapper.Where(_003C_003Ec.watcherInitializer.AwakeObserver).ToArray();
			if (array.Length >= 2)
			{
				using (new GUILayout.HorizontalScope())
				{
					Rect def = EditorGUILayout.GetControlRect(false, 24f, GUIStyle.none, GUILayout.ExpandWidth(expand: true));
					if (!_ProducerReg)
					{
						def.y -= 4f;
					}
					_ProducerReg = false;
					float visitor = def.width / (float)array.Length;
					string[] array2 = QueryMapper(listenerVisitor.CustomizeTests());
					ImporterMapper[] array3 = array;
					foreach (ImporterMapper importerMapper in array3)
					{
						string[] array4 = QueryMapper(importerMapper.CustomizeTests());
						bool skipcont;
						if (skipcont = array2.Length >= array4.Length)
						{
							int num = Mathf.Min(array2.Length, array4.Length);
							for (int j = 0; j < num; j++)
							{
								if (array2[j] != array4[j])
								{
									skipcont = false;
									break;
								}
							}
						}
						Rect var = def.SortResolver(visitor, isfield: true);
						using (new TemplateThread(TemplateThread.ColoringType.BG, skipcont, ClassProperty.configurationProperty, Color.gray))
						{
							if (ClassProperty.CancelQueue(var, importerMapper.requestMapper, EditorStyles.toolbarButton))
							{
								if (listenerVisitor == importerMapper)
								{
									int num2 = importerMapper.CustomizeTests().LastIndexOf(PushInitializer(), StringComparison.Ordinal);
									listenerVisitor = ((num2 <= 0) ? m_ParamsVisitor : (m_ParamsVisitor.CalculateTests(importerMapper.CustomizeTests().Substring(0, num2)) ?? m_ParamsVisitor));
								}
								else
								{
									listenerVisitor = importerMapper;
								}
								AddMapper();
								m_GetterVisitor.index = 0;
								EnableInitializer(StartMapper(isreference: true));
							}
						}
					}
				}
			}
		});
	}

	private static string[] QueryMapper(string task)
	{
		return task.Split(ClassProperty.CheckRules<string>(PushInitializer()), StringSplitOptions.None);
	}

	private static void AddMapper()
	{
		_003C_003Ec__DisplayClass616_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass616_0();
		CS_0024_003C_003E8__locals9.m_IteratorReg = ReadAnnotation();
		if (CS_0024_003C_003E8__locals9.m_IteratorReg == null)
		{
			return;
		}
		CS_0024_003C_003E8__locals9._PublisherReg = ((m_GetterVisitor != null && m_GetterVisitor.CallPredicate()) ? ((EventMapper)m_GetterVisitor.list[m_GetterVisitor.index])._InfoMapper.name : "");
		_FacadeVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.ElementCallbackDelegate>("OnDrawLayer");
		_AdvisorVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnSelectLayer");
		_CallbackVisitor = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnMouseUpLayer");
		m_GetterVisitor = new ReorderableList(listenerVisitor.listenerMapper, typeof(EventMapper), draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
		{
			drawElementBackgroundCallback = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.ElementCallbackDelegate>("OnDrawLayerBackground"),
			drawElementCallback = InvokeMapper,
			onSelectCallback = FindMapper,
			onMouseUpCallback = ExcludeMapper,
			showDefaultBackground = false,
			headerHeight = 0f,
			footerHeight = 0f,
			elementHeight = CloneInitializer()
		};
		if (!CS_0024_003C_003E8__locals9._PublisherReg.ResetResolver())
		{
			int num = listenerVisitor.listenerMapper.FindIndex((EventMapper l) => l._InfoMapper.name == CS_0024_003C_003E8__locals9._PublisherReg);
			if (num >= 0)
			{
				m_GetterVisitor.index = num;
			}
		}
	}

	private static void InvokeMapper(Rect last, int mean_cont, bool createrule, bool bool_0)
	{
		if (mean_cont.CustomizeResolver(listenerVisitor.listenerMapper))
		{
			int facadeMapper = listenerVisitor.listenerMapper[mean_cont]._FacadeMapper;
			if (facadeMapper.CustomizeResolver(m_InterceptorVisitor.list))
			{
				_FacadeVisitor(last, facadeMapper, bool_0, createrule);
			}
		}
	}

	private static void FindMapper(ReorderableList res)
	{
		InitMapper(_AdvisorVisitor);
	}

	private static void ExcludeMapper(ReorderableList i)
	{
		InitMapper(_CallbackVisitor);
	}

	private static void InitMapper(ReorderableList.SelectCallbackDelegate instance)
	{
		int index = StartMapper(isreference: true);
		m_InterceptorVisitor.index = index;
		instance(m_InterceptorVisitor);
	}

	private static UnityEditor.Animations.AnimatorControllerLayer VisitMapper()
	{
		return DefineMapper(StartMapper());
	}

	private static UnityEditor.Animations.AnimatorControllerLayer DefineMapper(int sizelast)
	{
		if (RateInitializer())
		{
			return (UnityEditor.Animations.AnimatorControllerLayer)m_InterceptorVisitor.list[sizelast];
		}
		return listenerVisitor.listenerMapper[sizelast]._InfoMapper;
	}

	private static int StartMapper(bool isreference = false)
	{
		if (writerVisitor != LayerViewViewType.DefaultView)
		{
			if (isreference)
			{
				if (!m_GetterVisitor.index.CustomizeResolver(m_GetterVisitor.list))
				{
					return -1;
				}
				return listenerVisitor.listenerMapper[m_GetterVisitor.index]._FacadeMapper;
			}
			return m_GetterVisitor.index;
		}
		return m_InterceptorVisitor.index;
	}

	private static IEnumerable<string> ReadMapper(UnityEditor.Animations.AnimatorControllerLayer var1)
	{
		IEnumerable<string> enumerable = var1.InstantiatePredicate();
		foreach (string item in enumerable)
		{
			Match match = Regex.Match(item, "^_category:(.+)$");
			if (match.Success)
			{
				yield return match.Groups[1].Value;
			}
		}
	}

	[PrototypeServer(1)]
	private static void SelectMapper()
	{
		if (m_InterceptorVisitor == null)
		{
			object obj = ReadAnnotation();
			if (obj != null)
			{
				m_InterceptorVisitor = (ReorderableList)m_IdentifierVisitor.GetValue(obj);
			}
		}
	}

	private static void RemoveMapper()
	{
		SpecificationAlgo.MapReg(typeof(Unsupported), "PasteToStateMachineFromPasteboard", SpecificationAlgo.InsertTests<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPrefix), SpecificationAlgo.PushReg<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPost));
	}

	private static void PasteToStateMachineFromPasteboardPrefix(AnimatorStateMachine sm, out ChildAnimatorState[] __state)
	{
		__state = sm.states;
	}

	private static void PasteToStateMachineFromPasteboardPost(AnimatorStateMachine sm, ChildAnimatorState[] __state)
	{
		IEnumerable<AnimatorState> configurationReg = __state.Select((ChildAnimatorState s) => s.state);
		AnimatorState[] array = (from cs in sm.states.Except(__state)
			select cs.state).ToArray();
		HashSet<string> hashSet = new HashSet<string>(from cs in sm.states
			where !configurationReg.Contains(cs.state)
			select ClassProperty.RegisterRules(cs.state.name));
		AnimatorState[] array2 = array;
		foreach (AnimatorState obj in array2)
		{
			string item = (obj.name = ClassProperty.ChangeRules(ClassProperty.RegisterRules(obj.name), hashSet));
			hashSet.Add(item);
		}
		UnityEngine.Object[] objects = array;
		Selection.objects = objects;
	}

	private static void InstantiateMapper()
	{
		m_RuleVisitor = ClassProperty.FillRules("UnityEditor.GenericMenu+MenuItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetConstructor(new Type[4]
		{
			typeof(GUIContent),
			typeof(bool),
			typeof(bool),
			typeof(GenericMenu.MenuFunction)
		});
		_ManagerVisitor = typeof(EditorGUI).InsertList("AdvancedPopup", new Type[3]
		{
			typeof(Rect),
			typeof(int),
			typeof(string[])
		});
		m_SpecificationVisitor = typeof(GUIUtility).DisableList("GetBuiltinSkin");
		contextAnnotation = _ConsumerVisitor.RestartList("m_LayerEditor");
		recordAnnotation = _ConsumerVisitor.RestartList("m_PreviewAnimator");
		helperAnnotation = _ConsumerVisitor.QueryList("liveLink");
		m_ConsumerAnnotation = _ReaderVisitor.QueryList("selectedLayerIndex");
		indexerVisitor = new AnnotationProperty(() => _ReaderVisitor.GetMethod("get_renameOverlay").Invoke(ReadAnnotation(), null));
		_IssuerVisitor = new AnnotationProperty();
		_IssuerVisitor._RulesProperty = delegate(bool accepted)
		{
			if (accepted)
			{
				RestartAlgo(RevertMapper(), m_AlgoAnnotation, _IssuerVisitor.ListSerializer());
			}
		};
	}

	private static void AwakeMapper()
	{
		SpecificationAlgo.MapReg(typeof(GenericMenu), "ShowAsContext", null, SpecificationAlgo.NewReg((Action<object>)ShowAsContextPost));
	}

	private static void ShowAsContextPost(GenericMenu __instance)
	{
		m_SingletonVisitor = __instance;
	}

	private static void ResetMapper()
	{
		_FactoryVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationStateMachine.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_AccountVisitor = ClassProperty.FillRules("UnityEditor.Graphs.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_RefVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_StatusVisitor = ClassProperty.FillRules("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		roleVisitor = SetterTests.ConnectionTests._SystemTests.ChangeRecord().DisableList("MakeTransitionCallback");
		paramVisitor = SetterTests.ConnectionTests._WorkerTests.ChangeRecord().DisableList("MakeTransitionCallback");
		modelVisitor = SetterTests.ConnectionTests._CandidateTests.ChangeRecord().DisableList("MakeTransitionCallback");
		tokenizerVisitor = SetterTests.ConnectionTests.m_ExpressionTests.ChangeRecord().DisableList("MakeTransitionCallback");
		_ParserVisitor = _StatusVisitor.DisableList("FindClosestEdge");
		m_DecoratorVisitor = _FactoryVisitor.DisableList("GenericMenuForStateMachineNode");
		tokenVisitor = SetterTests.ConnectionTests._CandidateTests.ChangeRecord().RestartList("state");
		codeVisitor = _RefVisitor.RestartList("motion");
		m_DicVisitor = _RefVisitor.RestartList("children");
		_InvocationVisitor = _RefVisitor.QueryList("parent");
	}

	private static void FlushMapper()
	{
		SpecificationAlgo.MapReg(m_CandidateVisitor, "CreateNodeFromState", null, SpecificationAlgo.NewReg<ChildAnimatorState>(CreateNodeFromStatePost));
		SpecificationAlgo.MapReg(m_CandidateVisitor, "CreateNodeFromStateMachine", null, SpecificationAlgo.PushReg<object, ChildAnimatorStateMachine>(CreateNodeFromStateMachinePost));
		SpecificationAlgo.MapReg(m_CandidateVisitor, "CreateNodes", null, SpecificationAlgo.NewReg<object>(CreateNodesPost));
		SpecificationAlgo.MapReg(_WatcherVisitor, "AddStateEmptyCallback", SpecificationAlgo.StopReg<object, object, bool>(AddEmptyStatePrefix));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests._CandidateTests, "NodeUI", SpecificationAlgo.NewReg<Node>(StateNodeUIPrefix), SpecificationAlgo.CreateReg(StateNodeUIPost));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests.m_ExpressionTests, "NodeUI", SpecificationAlgo.NewReg<Node>(smethod_3), SpecificationAlgo.CreateReg(MachineNodeUIPost));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests._SystemTests, "NodeUI", SpecificationAlgo.NewReg<Node>(EntryStateNodeUIPrefix), SpecificationAlgo.CreateReg(smethod_4));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests.m_FilterTests, "NodeUI", SpecificationAlgo.NewReg<Node>(ExitNodeUIPrefix), SpecificationAlgo.CreateReg(ExitNodeUIPost));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests._WorkerTests, "NodeUI", SpecificationAlgo.NewReg<Node>(smethod_5), SpecificationAlgo.CreateReg(smethod_6));
		SpecificationAlgo.MapReg(_StatusVisitor, "EndSlotDragging", SpecificationAlgo.CreateReg(EndSlotDraggingPrefix), SpecificationAlgo.CreateReg(EndSlotDraggingPost));
		SpecificationAlgo.MapReg(_StatusVisitor, "EndDragging", null, SpecificationAlgo.CreateReg(EndDraggingPost));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests._WorkerTests, "Connect", null, SpecificationAlgo.PushReg<object, object>(AnyStateNodeConnectPost));
		SpecificationAlgo.MapReg(SetterTests.ConnectionTests._CandidateTests, "Connect", null, SpecificationAlgo.PushReg<Node, Node>(StateNodeConnectPost));
		SpecificationAlgo.MapReg(m_InterpreterVisitor, "HandleNodeInput", SpecificationAlgo.OrderTests<bool>(TreeNodeInputPrefix), SpecificationAlgo.PushReg<object, bool>(TreeNodeInputPost));
		SpecificationAlgo.MapReg(m_InterpreterVisitor, "NodeGUI", SpecificationAlgo.NewReg<object>(TreeNodeGUIPrefix), SpecificationAlgo.NewReg<Node>(TreeNodeGUIPost));
		SpecificationAlgo.MapReg(_WatcherVisitor, "NodeGUI", SpecificationAlgo.InsertTests<Node, AnimatorState>(GraphGUINodeGUIPrefix), SpecificationAlgo.NewReg<AnimatorState>(smethod_7));
	}

	private static void CreateNodeFromStatePost(ChildAnimatorState state)
	{
		AnimatorState state2 = state.state;
		SetterTests.TokenizerTests tokenizerTests = SetterTests.RevertThread(state2);
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticNodesActive)
		{
			tokenizerTests.FillPolicy().color = (Styles.Color)ConsumerAlgo.CallDefinition().normalStateNodeColor.ResetDefinition();
		}
		string[] array = InvokeAlgo(state2);
		foreach (string text in array)
		{
			if (m_PolicyAnnotation.ContainsKey(text))
			{
				tokenizerTests.FillPolicy().style = text;
			}
		}
	}

	private static void CreateNodeFromStateMachinePost(object __instance, ChildAnimatorStateMachine subStateMachine)
	{
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticNodesActive)
		{
			SetterTests.OrderPolicy(subStateMachine.stateMachine).FillPolicy().color = (Styles.Color)ConsumerAlgo.CallDefinition().machineStateNodeColor.ResetDefinition();
		}
	}

	private static void CreateNodesPost(object __instance)
	{
		if ((bool)ConsumerAlgo.CallDefinition().cosmeticNodesActive)
		{
			SetterTests.RefTests.FlushPolicy().InterruptPolicy((Styles.Color)ConsumerAlgo.CallDefinition().entryStateNodeColor.ResetDefinition());
			SetterTests.RefTests.CalculatePolicy().InterruptPolicy((Styles.Color)ConsumerAlgo.CallDefinition().exitStateNodeColor.ResetDefinition());
			SetterTests.RefTests.MapPolicy().InterruptPolicy((Styles.Color)ConsumerAlgo.CallDefinition().anyStateNodeColor.ResetDefinition());
			if ((bool)ManageMapper().defaultState)
			{
				SetterTests.RevertThread(ManageMapper().defaultState).FillPolicy().color = (Styles.Color)ConsumerAlgo.CallDefinition().defaultStateNodeColor.ResetDefinition();
			}
		}
	}

	private static bool AddEmptyStatePrefix(object __instance, object data)
	{
		AnimatorState animatorState = RevertMapper().AddState(ConsumerAlgo.CallDefinition().defaultState.name, ((Vector2)data).PostPredicate(10));
		QueryAlgo(animatorState);
		animatorState.motion = ConsumerAlgo.CallDefinition().defaultState.motion;
		InterruptAlgo(wantfirst: false);
		return false;
	}

	private static IEnumerable<CodeInstruction> HandleObjectDraggingTranspiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = instructions.ToList();
		MethodInfo[] source = (from m in AccessTools.GetDeclaredMethods(typeof(AnimatorStateMachine))
			where m.Name == "AddState"
			select m).ToArray();
		for (int num = 0; num < list.Count; num++)
		{
			CodeInstruction codeInstruction = list[num];
			if (codeInstruction.opcode.ToString() == "callvirt" && !codeInstruction.operand.ToString().StartsWith("Void") && source.Any(codeInstruction.Calls))
			{
				num++;
				CodeInstruction codeInstruction2 = list[num];
				if (codeInstruction2.IsStloc())
				{
					object operand = codeInstruction2.operand;
					CodeInstruction[] array = new CodeInstruction[2]
					{
						new CodeInstruction(OpCodes.Ldloc, operand),
						new CodeInstruction(OpCodes.Call, m_ComparatorVisitor)
					};
					list.InsertRange(num + 1, array);
					num += 2;
				}
			}
		}
		return list;
	}

	private static void ConnectMapper(Node config, int contend)
	{
		_InstanceVisitor = false;
		bool flag = contend <= 1;
		Event current = Event.current;
		bool flag2 = current.control && current.shift;
		switch (current.type)
		{
		case EventType.MouseDown:
			if (!m_ObjectVisitor)
			{
				if (!flag2 || current.clickCount != 2)
				{
					if (contend == 3 || flag2 || current.clickCount != 2 || (flag && !_ValVisitor && !(current.control ^ (bool)ConsumerAlgo.CallDefinition().switchDoubleClick)))
					{
						if (!m_UtilsVisitor || current.clickCount != 1)
						{
							if (_ValVisitor && current.clickCount == 1)
							{
								_ValVisitor = false;
							}
						}
						else
						{
							m_UtilsVisitor = false;
							_ValVisitor = true;
						}
					}
					else
					{
						(contend switch
						{
							4 => paramVisitor, 
							2 => roleVisitor, 
							1 => tokenizerVisitor, 
							_ => modelVisitor, 
						}).Invoke(config, null);
						_MockVisitor = contend;
						classVisitor = config;
						m_DescriptorVisitor = true;
						m_UtilsVisitor = true;
						current.Use();
					}
				}
				else if (!m_ObjectVisitor)
				{
					exceptionVisitor = config;
					m_ObjectVisitor = true;
					_StatusVisitor.DisableList("BeginSlotDragging").Invoke(ExcludeAnnotation(), new object[3]
					{
						config.inputSlots.First(),
						true,
						true
					});
					current.Use();
				}
			}
			else
			{
				m_ObjectVisitor = false;
				if (m_AuthenticationVisitor == null)
				{
					m_AuthenticationVisitor = new AnimatorState
					{
						name = "Amogus"
					};
				}
				if (merchantVisitor == null)
				{
					merchantVisitor = new AnimatorStateTransition
					{
						destinationState = m_AuthenticationVisitor,
						exitTime = 69f,
						duration = 420f,
						offset = 80085f
					};
				}
				object obj = ExcludeAnnotation();
				m_CandidateVisitor.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m2) => m2.Name == "CreateEdges" && m2.GetParameters().Length == 3).Invoke(StartAnnotation(), new object[3]
				{
					exceptionVisitor,
					config,
					Activator.CreateInstance(SetterTests.ConnectionTests.readerTests.ChangeRecord(), merchantVisitor, null, null, null, null)
				});
				_StatusVisitor.DisableList("EndDragging").Invoke(obj, null);
				current.Use();
			}
			break;
		case EventType.ContextClick:
			if (contend == 3)
			{
				current.Use();
			}
			_InstanceVisitor = true;
			break;
		}
	}

	private static void CalculateMapper(int next_task)
	{
		if (!_InstanceVisitor)
		{
			return;
		}
		_InstanceVisitor = false;
		bool flag;
		if (flag = next_task == 3)
		{
			m_SingletonVisitor = new GenericMenu();
		}
		IList list = m_SingletonVisitor.CountList();
		if (next_task == 0)
		{
			AnimatorState animatorState = Selection.activeObject as AnimatorState;
			if ((bool)animatorState && animatorState.motion is UnityEditor.Animations.BlendTree)
			{
				templateVisitor = animatorState;
				object value = m_RuleVisitor.Invoke(new object[4]
				{
					new GUIContent("Edit BlendTree"),
					false,
					false,
					new GenericMenu.MenuFunction(TestMapper)
				});
				list.Insert(3, value);
			}
		}
		bool flag2 = false;
		if ((baseAnnotation && !flag) || containerAnnotation)
		{
			m_SingletonVisitor.AddSeparator(string.Empty);
			if (containerAnnotation)
			{
				m_SingletonVisitor.AddItem(new GUIContent("Redirect Transitions"), on: true, LoginAlgo);
			}
			if (baseAnnotation && !flag)
			{
				m_SingletonVisitor.AddItem(new GUIContent("Replicate Transitions"), on: true, CreateAlgo);
			}
			flag2 = true;
		}
		if (!flag || m_ClassAnnotation)
		{
			object value2 = m_RuleVisitor.Invoke(new object[4]
			{
				new GUIContent("Make Multiple Transitions"),
				false,
				m_ClassAnnotation,
				new GenericMenu.MenuFunction(MoveAlgo)
			});
			list.Insert((!flag) ? 1 : 0, value2);
			flag2 = true;
		}
		if (next_task <= 1)
		{
			m_SingletonVisitor.AddItem(new GUIContent("Pack into StateMachine"), on: false, CallAlgo);
			flag2 = true;
		}
		if (flag2)
		{
			m_SingletonVisitor.AddSeparator(string.Empty);
		}
		if (next_task != 1)
		{
			m_SingletonVisitor.AddItem(new GUIContent("Select Shared Transitions"), on: false, IncludeAlgo);
			if (!flag)
			{
				m_SingletonVisitor.AddItem(new GUIContent("Select Out Transitions"), on: false, GetAlgo);
			}
			if (next_task != 2 && next_task != 4)
			{
				m_SingletonVisitor.AddItem(new GUIContent("Select In Transitions"), on: false, CalcAlgo);
			}
		}
		if (next_task <= 1)
		{
			if (next_task == 1)
			{
				m_SingletonVisitor.AddItem(new GUIContent("Unpack StateMachine"), on: false, CancelAlgo);
			}
			m_SingletonVisitor.AddSeparator(string.Empty);
			m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Copy"), on: false, ReadAlgo);
			m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Paste"), on: false, RemoveAlgo() ? new GenericMenu.MenuFunction(SelectAlgo) : null);
			m_SingletonVisitor.AddItem(new GUIContent("Behaviours/Remove"), on: false, InstantiateAlgo);
			foreach (string item in _PageAnnotation)
			{
				string m_ProcReg = "ce_" + item;
				bool m_WrapperTests = m_AlgoAnnotation.All((AnimatorState state) => state.tag == m_ProcReg);
				m_SingletonVisitor.AddItem(new GUIContent("Styles/" + item), m_WrapperTests, delegate
				{
					UnityEngine.Object[] objectsToUndo = m_AlgoAnnotation.ToArray();
					Undo.RecordObjects(objectsToUndo, "Set State Style");
					foreach (AnimatorState item2 in m_AlgoAnnotation)
					{
						item2.tag = ((!m_WrapperTests) ? m_ProcReg : "");
						item2.ManagePredicate();
					}
					_SystemVisitor = true;
				});
			}
		}
		m_SingletonVisitor.ShowAsContext();
	}

	private static void TreeNodeGUIPrefix(object n)
	{
	}

	private static void TreeNodeGUIPost(Node n)
	{
		string last = $"TreeNode{n.GetInstanceID()}";
		if (Event.current.type == EventType.Repaint)
		{
			ClassProperty.SearchRules(last, n.position);
		}
		Rect value = ClassProperty.OrderQueue(last, Rect.zero);
		float x = 0f;
		value.y = 0f;
		value.x = x;
		ClassProperty.AwakeRules(value, delegate(IEnumerable<Motion> motions)
		{
			_003C_003Ec__DisplayClass670_1 _003C_003Ec__DisplayClass670_ = new _003C_003Ec__DisplayClass670_1();
			object value2 = codeVisitor.GetValue(n);
			_003C_003Ec__DisplayClass670_.visitorTests = value2 as UnityEditor.Animations.BlendTree;
			if ((object)_003C_003Ec__DisplayClass670_.visitorTests != null)
			{
				Undo.RecordObject(_003C_003Ec__DisplayClass670_.visitorTests, "DragNDrop Motions");
				_003C_003Ec__DisplayClass670_.visitorTests.children = _003C_003Ec__DisplayClass670_.visitorTests.children.Concat(motions.Select(_003C_003Ec__DisplayClass670_.CollectThread)).ToArray();
			}
			else
			{
				object value3 = _InvocationVisitor.GetValue(n);
				if (value3 != null)
				{
					IList list = (IList)m_DicVisitor.GetValue(value3);
					int num = -1;
					for (int i = 0; i < list.Count; i++)
					{
						object obj = list[i];
						if (n == obj)
						{
							num = i;
							break;
						}
					}
					UnityEditor.Animations.BlendTree obj2 = (UnityEditor.Animations.BlendTree)codeVisitor.GetValue(value3);
					Undo.RecordObject(obj2, "DragNDrop Motion");
					ChildMotion[] children = obj2.children;
					children[num].motion = motions.First();
					obj2.children = children;
				}
			}
		});
	}

	private static void TreeNodeInputPrefix(out bool __state)
	{
		Event current = Event.current;
		__state = current.type == EventType.MouseDown && current.button == 1;
	}

	private static void TreeNodeInputPost(object node, bool __state)
	{
		if (!__state)
		{
			return;
		}
		UnityEditor.Animations.BlendTree algoTests = codeVisitor.GetValue(node) as UnityEditor.Animations.BlendTree;
		if ((bool)algoTests)
		{
			m_SingletonVisitor.AddItem("Add Root Tree".CreateResolver(), on: false, delegate
			{
				string assetPath = AssetDatabase.GetAssetPath(algoTests);
				if (!string.IsNullOrWhiteSpace(assetPath))
				{
					UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);
					UnityEditor.Animations.BlendTree blendTree = new UnityEditor.Animations.BlendTree
					{
						name = "BlendTree"
					};
					Undo.RegisterCreatedObjectUndo(blendTree, "Add Root Tree");
					Undo.RecordObject(algoTests, "Add Root Tree");
					if (!(animatorController != null))
					{
						string directoryName = Path.GetDirectoryName(assetPath);
						string fileName = Path.GetFileName(assetPath);
						string path = AssetDatabase.GenerateUniqueAssetPath(directoryName + "/" + fileName + ".blendtree");
						AssetDatabase.CreateAsset(blendTree, path);
					}
					else
					{
						AssetDatabase.AddObjectToAsset(blendTree, animatorController);
						blendTree.hideFlags = HideFlags.HideInHierarchy;
					}
					ChildMotion[] children = algoTests.children;
					ChildMotion[] children2 = new ChildMotion[1]
					{
						new ChildMotion
						{
							motion = blendTree,
							timeScale = 1f
						}
					};
					algoTests.children = children2;
					blendTree.children = children;
					algoTests.PrintPredicate();
					blendTree.PrintPredicate();
				}
				else
				{
					DestroyAnnotation("Target tree is not saved to assets!");
				}
			});
			m_SingletonVisitor.ShowAsContext();
		}
		_InstanceVisitor = false;
	}

	private static void StateNodeUIPrefix(Node __instance)
	{
		ConnectMapper(__instance, 0);
	}

	private static void StateNodeUIPost()
	{
		CalculateMapper(0);
	}

	private static void smethod_3(Node __instance)
	{
		ConnectMapper(__instance, 1);
	}

	private static void MachineNodeUIPost()
	{
		CalculateMapper(1);
	}

	private static void EntryStateNodeUIPrefix(Node __instance)
	{
		ConnectMapper(__instance, 2);
	}

	private static void smethod_4()
	{
		CalculateMapper(2);
	}

	private static void ExitNodeUIPrefix(Node __instance)
	{
		ConnectMapper(__instance, 3);
	}

	private static void ExitNodeUIPost()
	{
		CalculateMapper(3);
	}

	private static void smethod_5(Node __instance)
	{
		ConnectMapper(__instance, 4);
	}

	private static void smethod_6()
	{
		CalculateMapper(4);
	}

	private static void TestMapper()
	{
		m_PrototypeVisitor.Invoke(SetterTests.RefTests.PopPolicy(), new object[2] { templateVisitor, true });
	}

	private static void EndSlotDraggingPrefix()
	{
		valueVisitor = true;
	}

	private static void EndSlotDraggingPost()
	{
		if (m_DescriptorVisitor)
		{
			(_MockVisitor switch
			{
				4 => paramVisitor, 
				2 => roleVisitor, 
				1 => tokenizerVisitor, 
				_ => modelVisitor, 
			}).Invoke(classVisitor, null);
			m_DescriptorVisitor = true;
			m_UtilsVisitor = true;
		}
	}

	private static void EndDraggingPost()
	{
		m_DescriptorVisitor = false;
		if (!valueVisitor)
		{
			m_UtilsVisitor = false;
			_ValVisitor = false;
		}
		valueVisitor = false;
	}

	private static void GraphGUINodeGUIPrefix(Node n, out AnimatorState __state)
	{
		if (n.GetType() != SetterTests.ConnectionTests._CandidateTests)
		{
			__state = null;
		}
		else
		{
			__state = (AnimatorState)tokenVisitor.GetValue(n);
		}
		if (__state == null)
		{
			return;
		}
		_ReponseVisitor = n.position.size;
		if (VisitAlgo(__state))
		{
			return;
		}
		AnimatorState m_MapperTests = __state;
		ConsumerAlgo.StateCosmeticOptions stateCosmeticOptions = ConsumerAlgo.CallDefinition().PostDefinition();
		Rect connection = new Rect(1f, 1f, 11f, 11f);
		if (stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.coordinates))
		{
			float x = n.position.x;
			float y = n.position.y;
			Rect source = new Rect(1f, _ReponseVisitor.y - 7f, RegisterMapper(x), 7f);
			Rect rect = new Rect(source);
			rect.x = source.width;
			rect.width = RegisterMapper(y);
			Rect rect2 = rect;
			EditorGUI.BeginChangeCheck();
			GUIStyle style = new GUIStyle(ClassProperty.CalcError().annotationObserver)
			{
				fontSize = 7
			};
			x = EditorGUI.DelayedFloatField(source, x, style);
			y = EditorGUI.DelayedFloatField(rect2, y, style);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(RevertMapper(), "Set ChildState Position");
				ChildAnimatorState[] states = RevertMapper().states;
				if (states.ExcludeResolver((ChildAnimatorState childAnimatorState) => childAnimatorState.state == m_MapperTests, out var c))
				{
					states[c].position = new Vector3(x, y);
					RevertMapper().states = states;
				}
			}
		}
		if (stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.indicators))
		{
			bool flag = m_MapperTests.behaviours.Length != 0;
			bool writeDefaultValues = m_MapperTests.writeDefaultValues;
			Rect res = new Rect(0f, 0f, _ReponseVisitor.x, _ReponseVisitor.y);
			if (stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.inactiveIndicators) || writeDefaultValues)
			{
				GUI.Label(res.PatchResolver(20f, isserv: true).ManageResolver(14f), ClassProperty.CalcError()._ClientProcessor, (!writeDefaultValues) ? ClassProperty.CalcError()._VisitorObserver : ClassProperty.CalcError()._ClassProcessor);
			}
			if (stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.inactiveIndicators) || flag)
			{
				GUI.Label(res.PatchResolver(10f, isserv: true).ManageResolver(14f), ClassProperty.CalcError().m_AttributeProcessor, (!flag) ? ClassProperty.CalcError()._VisitorObserver : ClassProperty.CalcError()._ClassProcessor);
			}
		}
		if (!stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.quickNewClip))
		{
			return;
		}
		GUI.Label(connection, "+", GUI.skin.label);
		if (!new DispatcherPolicy(Event.current).ExcludeHelper().InvokeHelper().QueryHelper(connection))
		{
			return;
		}
		string text = $"{ConsumerAlgo.CallDefinition().lastAnimationPath}/{ConsumerAlgo.CallDefinition().lastAnimationName}.anim";
		bool flag2;
		string defaultName = Path.GetFileNameWithoutExtension((!(flag2 = AssetDatabase.IsValidFolder(ConsumerAlgo.CallDefinition().lastAnimationPath))) ? text : AssetDatabase.GenerateUniqueAssetPath(text)) + ".anim";
		string text2 = EditorUtility.SaveFilePanel("New Animation Path", flag2 ? ((string)ConsumerAlgo.CallDefinition().lastAnimationPath) : "Assets", defaultName, "anim");
		if (string.IsNullOrWhiteSpace(text2))
		{
			return;
		}
		string projectRelativePath = FileUtil.GetProjectRelativePath(text2);
		if (projectRelativePath.StartsWith("Assets"))
		{
			using (new ConsumerAlgo.WorkerAlgo())
			{
				ConsumerAlgo.CallDefinition().lastAnimationPath.ResolveDefinition(Path.GetDirectoryName(projectRelativePath).Replace('\\', '/'));
				ConsumerAlgo.CallDefinition().lastAnimationName.ResolveDefinition(Path.GetFileNameWithoutExtension(projectRelativePath));
			}
			AnimationClip animationClip = new AnimationClip();
			AssetDatabase.CreateAsset(animationClip, projectRelativePath);
			m_MapperTests.motion = animationClip;
			EditorUtility.SetDirty(m_MapperTests);
		}
		else
		{
			RateAnnotation(isi: true, "Asset Path must be a folder within Assets!");
		}
	}

	private static void smethod_7(AnimatorState __state)
	{
		if (__state == null)
		{
			return;
		}
		bool flag;
		if (!(flag = VisitAlgo(__state)))
		{
			MapMapper(__state);
		}
		GUIStyle value;
		GUIStyle gUIStyle = ((!m_PolicyAnnotation.TryGetValue(__state.tag, out value)) ? predicateAnnotation : value);
		if (gUIStyle == null)
		{
			return;
		}
		float num = ((gUIStyle.alignment >= TextAnchor.MiddleLeft) ? (gUIStyle.fixedHeight / 2f - 15f) : 5f);
		int num2 = ((gUIStyle.fontSize != 0) ? (gUIStyle.fontSize + 2) : 15);
		Event current = Event.current;
		EventType type = current.type;
		if (m_AlgoAnnotation.Contains(__state))
		{
			if (type == EventType.KeyDown)
			{
				KeyCode keyCode = current.keyCode;
				if (keyCode <= KeyCode.Escape)
				{
					if (keyCode != KeyCode.Return)
					{
						if (keyCode == KeyCode.Escape)
						{
							_IssuerVisitor.MapSerializer(isconfig: false);
						}
						goto IL_0089;
					}
				}
				else if (keyCode != KeyCode.KeypadEnter)
				{
					if (keyCode == KeyCode.F2)
					{
						_IssuerVisitor.CalculateSerializer((!flag) ? new Rect(gUIStyle.fixedWidth * 0.15f, num, gUIStyle.fixedWidth * 0.7f, num2) : new Rect(0f, 0f, gUIStyle.fixedWidth, gUIStyle.fixedHeight), __state.name, 0, 0f);
					}
					goto IL_0089;
				}
				if (_IssuerVisitor.ReflectSerializer())
				{
					_IssuerVisitor.VerifySerializer(_IssuerVisitor.ListSerializer().Replace("\\n", "\n"));
				}
				_IssuerVisitor.MapSerializer(isconfig: true);
			}
			goto IL_0089;
		}
		goto IL_00d7;
		IL_00d7:
		if (flag)
		{
			return;
		}
		ConsumerAlgo.StateCosmeticOptions stateCosmeticOptions = ConsumerAlgo.CallDefinition().PostDefinition();
		bool flag2 = stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.motionName);
		bool flag3 = stateCosmeticOptions.HasFlag(ConsumerAlgo.StateCosmeticOptions.motionIcon);
		if (flag2 || flag3)
		{
			Rect rect = new Rect(0f, num + (float)num2, _ReponseVisitor.x, 18f);
			bool num3 = __state.motion;
			if (num3 && flag3 && flag2)
			{
				rect.x = -9f;
			}
			GUIContent content = ((!num3) ? ((!flag2) ? GUIContent.none : new GUIContent("(None)")) : new GUIContent((!flag2) ? string.Empty : ("(" + __state.motion.name + ")"), (!flag3) ? null : ((__state.motion is AnimationClip animationClip) ? ((!animationClip.isLooping) ? ClassProperty.DestroyError().m_RegistryProcessor.image : ClassProperty.DestroyError().readerProcessor.image) : ClassProperty.DestroyError().m_PrinterProcessor.image)));
			GUI.Label(rect, content, ClassProperty.CalcError()._ClassProcessor);
		}
		return;
		IL_0089:
		if (_IssuerVisitor.ReflectSerializer())
		{
			string key = _IssuerVisitor.ListSerializer();
			_IssuerVisitor.OnGUI(GUI.skin.textArea);
			if (__state == m_AlgoAnnotation[0])
			{
				_IssuerVisitor.ValidateSerializer();
			}
			else
			{
				_IssuerVisitor.VerifySerializer(key);
			}
		}
		goto IL_00d7;
	}

	private static void MapMapper(AnimatorState config)
	{
		Event current = Event.current;
		EventType type = current.type;
		List<GameObject> list = DragAndDrop.objectReferences.Where((UnityEngine.Object o) => o is GameObject { scene: var scene } && scene.isLoaded).Cast<GameObject>().ToList();
		bool flag = list.Any();
		if (type == EventType.DragUpdated && flag)
		{
			poolVisitor = true;
		}
		if (type != EventType.DragPerform)
		{
			return;
		}
		Motion motion = DragAndDrop.objectReferences.FirstOrDefault((UnityEngine.Object o) => o is Motion) as Motion;
		if (!((bool)motion || flag))
		{
			return;
		}
		Transform _InitializerTests;
		object obj;
		if ((bool)motion)
		{
			if (m_AlgoAnnotation.Count <= 1 || !m_AlgoAnnotation.Contains(config))
			{
				Undo.RecordObject(config, "Drag & Drop to State");
				config.motion = motion;
				config.PrintPredicate();
			}
			else
			{
				UnityEngine.Object[] objectsToUndo = m_AlgoAnnotation.ToArray();
				Undo.RecordObjects(objectsToUndo, "Drag & Drop to States");
				foreach (AnimatorState item in m_AlgoAnnotation)
				{
					item.motion = motion;
					item.PrintPredicate();
				}
			}
		}
		else if (flag)
		{
			parameterVisitor = config;
			_InitializerTests = list[0].transform.root;
			Animator componentInChildren = _InitializerTests.GetComponentInChildren<Animator>();
			if ((object)componentInChildren != null)
			{
				obj = componentInChildren.transform;
				if (obj != null)
				{
					goto IL_01a1;
				}
			}
			else
			{
				obj = null;
			}
			obj = _InitializerTests;
			goto IL_01a1;
		}
		goto IL_0073;
		IL_0073:
		EditorUtility.SetDirty(config);
		current.Use();
		return;
		IL_01a1:
		_InitializerTests = (Transform)obj;
		List<GameObject> template = list.Where((GameObject o) => o == _InitializerTests || o.transform.IsChildOf(_InitializerTests)).ToList();
		QuickToggleWindow quickToggleWindow = QuickToggleWindow.AssetTests(m_AlgoAnnotation.Contains(parameterVisitor) ? m_AlgoAnnotation : new List<AnimatorState> { parameterVisitor }, _InitializerTests, template);
		Vector2 mousePosition = current.mousePosition;
		quickToggleWindow.SortTests(GUIUtility.GUIToScreenPoint(mousePosition));
		goto IL_0073;
	}

	private static void StateNodeConnectPost(Node __instance, Node toNode)
	{
		AnimatorState definitionTests = (AnimatorState)tokenVisitor.GetValue(__instance);
		if (SetterTests.ConnectionTests.m_ExpressionTests != toNode.GetType())
		{
			AnimatorStateTransition animatorStateTransition = definitionTests.transitions.Last();
			CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, animatorStateTransition);
			animatorStateTransition.canTransitionToSelf = true;
			return;
		}
		m_DecoratorVisitor.Invoke(null, new object[3]
		{
			toNode,
			true,
			(GenericMenu.MenuFunction2)delegate(object data)
			{
				AnimatorStateTransition animatorStateTransition2 = null;
				if (data is AnimatorState destinationState)
				{
					animatorStateTransition2 = definitionTests.AddTransition(destinationState);
				}
				else if (data is AnimatorStateMachine animatorStateMachine)
				{
					animatorStateTransition2 = ((!animatorStateMachine.states.GetRules()) ? definitionTests.AddTransition(animatorStateMachine) : definitionTests.transitions.Last());
				}
				if (animatorStateTransition2 != null)
				{
					CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, animatorStateTransition2);
					animatorStateTransition2.canTransitionToSelf = true;
				}
			}
		});
	}

	private static void AnyStateNodeConnectPost(object __instance, object toNode)
	{
		if (SetterTests.ConnectionTests.m_ExpressionTests != toNode.GetType())
		{
			CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, ManageMapper().anyStateTransitions.Last());
			return;
		}
		m_DecoratorVisitor.Invoke(null, new object[3]
		{
			toNode,
			true,
			(GenericMenu.MenuFunction2)delegate(object data)
			{
				AnimatorStateTransition col = null;
				if (!(data is AnimatorState destinationState))
				{
					if (data is AnimatorStateMachine animatorStateMachine)
					{
						col = ((!animatorStateMachine.states.GetRules()) ? ManageMapper().AddAnyStateTransition(animatorStateMachine) : ManageMapper().anyStateTransitions.Last());
					}
				}
				else
				{
					col = ManageMapper().AddAnyStateTransition(destinationState);
				}
				CustomizeAlgo(ConsumerAlgo.CallDefinition().defaultTransition, col);
			}
		});
	}

	internal static void ValidateMapper()
	{
		mappingVisitor = Type.GetType("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_RepositoryVisitor = mappingVisitor.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	private static void CustomizeMapper()
	{
		SpecificationAlgo.MapReg(mappingVisitor, "AddParameterMenu", null, SpecificationAlgo.NewReg<object>(AddNewParameterPost));
		SpecificationAlgo.MapReg(mappingVisitor, "OnDrawParameter", SpecificationAlgo.RestartTests<Rect, int, int>(DrawParameterPrefix), SpecificationAlgo.NewReg<int>(DrawParameterPost));
		SpecificationAlgo.TestReg("UnityEditor.Graphs.ParameterControllerView+IntElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, SpecificationAlgo.NewReg<Rect>(IntElementGUIPost));
		SpecificationAlgo.TestReg("UnityEditor.Graphs.ParameterControllerView+FloatElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, SpecificationAlgo.NewReg<Rect>(smethod_8));
		SpecificationAlgo.TestReg("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "RenameEnd", SpecificationAlgo.CreateReg(ParameterRenameEndPrefix), SpecificationAlgo.CreateReg(ParameterRenameEndPost));
	}

	private static void DrawParameterPrefix(Rect rect, int index, out int __state)
	{
		__state = -1;
		Event current = Event.current;
		if (current.type == EventType.MouseUp && current.button == 1 && rect.Contains(current.mousePosition))
		{
			__state = index;
		}
	}

	private static void DrawParameterPost(int __state)
	{
		if (__state < 0)
		{
			return;
		}
		UnityEngine.AnimatorControllerParameter m_RegTests = LogoutMapper().parameters[__state];
		if (m_RegTests.type == UnityEngine.AnimatorControllerParameterType.Float)
		{
			AnimationClip m_PropertyTests = DefineInitializer();
			bool flag = FindInitializer() != null && m_PropertyTests != null;
			m_SingletonVisitor.AddItem(new GUIContent("Animate"), on: false, (!flag) ? null : ((GenericMenu.MenuFunction)delegate
			{
				EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Animator), m_RegTests.name);
				if (!RateAnnotation(AnimationUtility.GetEditorCurve(m_PropertyTests, binding) != null, "Property \"" + m_RegTests.name + "\" already exists in the active animation clip \"" + m_PropertyTests.name + "\"."))
				{
					AnimationUtility.SetEditorCurve(m_PropertyTests, binding, new AnimationCurve());
					FindInitializer().Repaint();
				}
			}));
		}
		m_SingletonVisitor.AddSeparator(string.Empty);
		m_SingletonVisitor.AddItem(new GUIContent("Convert/Bool"), on: false, delegate
		{
			PublishAlgo(LogoutMapper(), __state, UnityEngine.AnimatorControllerParameterType.Bool);
		});
		m_SingletonVisitor.AddItem(new GUIContent("Convert/Int"), on: false, delegate
		{
			PublishAlgo(LogoutMapper(), __state, UnityEngine.AnimatorControllerParameterType.Int);
		});
		m_SingletonVisitor.AddItem(new GUIContent("Convert/Float"), on: false, delegate
		{
			PublishAlgo(LogoutMapper(), __state, UnityEngine.AnimatorControllerParameterType.Float);
		});
		m_SingletonVisitor.AddItem(new GUIContent("Convert/Trigger"), on: false, delegate
		{
			PublishAlgo(LogoutMapper(), __state, UnityEngine.AnimatorControllerParameterType.Trigger);
		});
		m_SingletonVisitor.ShowAsContext();
	}

	private static void AddNewParameterPost(object __instance)
	{
		_RepositoryVisitor.SetValue(__instance, new Vector2(0f, 2.1474836E+09f));
	}

	private static void ParameterRenameEndPrefix()
	{
		m_ComposerVisitor = LogoutMapper().parameters;
	}

	private static void ParameterRenameEndPost()
	{
		if (!WorkerProperty.InvokePage())
		{
			return;
		}
		UnityEngine.AnimatorControllerParameter[] parameters = LogoutMapper().parameters;
		try
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				string text = m_ComposerVisitor[i].name;
				string text2 = parameters[i].name;
				if (!(text == text2))
				{
					UnityEditor.Animations.AnimatorControllerLayer[] layers = LogoutMapper().layers;
					for (int j = 0; j < layers.Length; j++)
					{
						PostAlgo(layers[j].stateMachine, text, text2, isord2: true);
					}
					CustomizeAnnotation("Renamed " + text + " to " + text2 + ".");
				}
			}
		}
		catch (System.Exception arg)
		{
			DestroyAnnotation($"WARNING! Automatic driver renaming failed\n{arg}");
		}
	}

	private static void IntElementGUIPost(Rect rect)
	{
		CalcAnnotation(rect, (!ConsumerAlgo.CallDefinition().capitalParameterIndicator) ? "i" : "I");
	}

	private static void smethod_8(Rect rect)
	{
		CalcAnnotation(rect, (!ConsumerAlgo.CallDefinition().capitalParameterIndicator) ? "f" : "F");
	}

	[CompilerGenerated]
	internal static string RateMapper(string task, ref _003C_003Ec__DisplayClass186_1 map)
	{
		if (!string.IsNullOrEmpty(task))
		{
			ICryptoTransform cryptoTransform = map.utilsDefinition.CreateDecryptor(map.utilsDefinition.Key, map.utilsDefinition.IV);
			byte[] array = Convert.FromBase64String(task);
			byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			return Encoding.UTF8.GetString(bytes);
		}
		return task;
	}

	[CompilerGenerated]
	internal static string DestroyMapper(string value, ref _003C_003Ec__DisplayClass186_2 map)
	{
		return Convert.ToBase64String(map._ValDefinition.ComputeHash(Encoding.UTF8.GetBytes(value)));
	}

	[CompilerGenerated]
	internal static string GetMapper(string init, ref _003C_003Ec__DisplayClass186_5 cont)
	{
		return Convert.ToBase64String(cont.m_ReponseDefinition.ComputeHash(Encoding.UTF8.GetBytes(init)));
	}

	[CompilerGenerated]
	internal static string CalcMapper(string reference, ref _003C_003Ec__DisplayClass186_4 second)
	{
		if (string.IsNullOrEmpty(reference))
		{
			return reference;
		}
		ICryptoTransform cryptoTransform = second.authenticationDefinition.CreateEncryptor(second.authenticationDefinition.Key, second.authenticationDefinition.IV);
		byte[] bytes = Encoding.UTF8.GetBytes(reference);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	[CompilerGenerated]
	internal static void IncludeMapper()
	{
		List<(string, string)> list = RegisterAnnotation("activatelicense");
		LogoutAnnotation(list);
		DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(FieldAlgo response)
		{
			importerAnnotation = false;
			SortAnnotation(response, delegate
			{
				m_IdentifierAnnotation = false;
				ConsumerAlgo.CallDefinition().a_HasSucceededLastVerification.ExcludeDefinition(excludeparam: true);
				WriteAnnotation(assetneeded: true);
			});
		}, delegate(System.Exception exception)
		{
			importerAnnotation = false;
			FindVisitor($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}, null, null, InsertVisitor);
	}

	[CompilerGenerated]
	internal static string RunMapper(string ident)
	{
		_003C_003Ec__DisplayClass192_1 vis = default(_003C_003Ec__DisplayClass192_1);
		vis.clientDefinition = new AesManaged();
		try
		{
			vis.clientDefinition.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
			vis.clientDefinition.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
			return CloneMapper(ident, ref vis);
		}
		finally
		{
			if (vis.clientDefinition != null)
			{
				((IDisposable)vis.clientDefinition).Dispose();
			}
		}
	}

	[CompilerGenerated]
	internal static string CloneMapper(string def, ref _003C_003Ec__DisplayClass192_1 vis)
	{
		ICryptoTransform cryptoTransform = vis.clientDefinition.CreateEncryptor(vis.clientDefinition.Key, vis.clientDefinition.IV);
		byte[] bytes = Encoding.UTF8.GetBytes(def);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	[CompilerGenerated]
	internal static string LoginMapper(string param)
	{
		using AesManaged aesManaged = new AesManaged();
		aesManaged.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
		aesManaged.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
		ICryptoTransform cryptoTransform = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
		byte[] array = Convert.FromBase64String(param);
		return Encoding.UTF8.GetString(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
	}

	[CompilerGenerated]
	internal static string ReflectMapper(string ident, int[] selection)
	{
		foreach (int num in selection)
		{
			if (num > 0)
			{
				ident = DeleteMapper(ident, num);
			}
		}
		return ident;
	}

	[CompilerGenerated]
	internal static string DeleteMapper(string def, int connectionID)
	{
		int num = 2;
		for (int i = connectionID; i < def.Length; i += connectionID)
		{
			num++;
			if (num == 3)
			{
				int num2 = i + connectionID;
				if (num2 >= def.Length)
				{
					break;
				}
				char c = def[num2];
				def = def.Remove(num2, 1).Insert(num2, def[i].ToString());
				def = def.Remove(i, 1).Insert(i, c.ToString());
				num = 0;
			}
		}
		return def;
	}

	[CompilerGenerated]
	internal static string CreateMapper(string config)
	{
		return ReflectMapper(RunMapper(config), new int[7] { 3, 2, 6, 4, 2, 1, 8 });
	}

	[CompilerGenerated]
	internal static string NewMapper(string config)
	{
		return LoginMapper(ReflectMapper(config, new int[7] { 8, 1, 2, 4, 6, 2, 3 }));
	}

	[CompilerGenerated]
	internal static async void PushMapper(PoolAlgo[] key, Action b, CancellationTokenSource serv)
	{
		try
		{
			await Task.Run(delegate
			{
				PoolAlgo[] array = key;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].CancelReg();
				}
			}, serv.Token);
			while (!key.All((PoolAlgo p) => p.m_MockAlgo))
			{
				serv.Token.ThrowIfCancellationRequested();
				await Task.Delay(50, serv.Token);
			}
		}
		finally
		{
			b?.Invoke();
		}
	}

	[CompilerGenerated]
	internal static bool ViewMapper(string reference, string reg, out (List<string>, Dictionary<string, RangeInt>) control)
	{
		control = (new List<string>(), new Dictionary<string, RangeInt>());
		(List<string>, Dictionary<string, RangeInt>) tuple = control;
		List<string> item = tuple.Item1;
		Dictionary<string, RangeInt> item2 = tuple.Item2;
		string[] array = reference.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (!flag)
			{
				if (text.IndexOf(reg, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}
				string pattern = "(\\w+?)\\b *";
				MatchCollection matchCollection = Regex.Matches(text, pattern);
				for (int j = 0; j < matchCollection.Count; j++)
				{
					Match match = matchCollection[j];
					if (match.Success)
					{
						string value = match.Groups[1].Value;
						RangeInt value2 = new RangeInt(match.Groups[0].Index, match.Groups[0].Length);
						item2.Add(value, value2);
					}
				}
				flag = true;
			}
			else
			{
				item.Add(text);
			}
		}
		return item.Count > 0;
	}

	[CompilerGenerated]
	internal static bool CollectMapper((List<string>, Dictionary<string, RangeInt>) item, string vis, out string[] c)
	{
		(List<string>, Dictionary<string, RangeInt>) tuple = item;
		List<string> item2 = tuple.Item1;
		Dictionary<string, RangeInt> item3 = tuple.Item2;
		c = new string[item2.Count];
		if (!item3.TryGetValue(vis, out var value))
		{
			return false;
		}
		for (int i = 0; i < item2.Count; i++)
		{
			string text = item2[i];
			c[i] = text.Substring(value.start, value.length).Trim();
		}
		return !c.All(string.IsNullOrWhiteSpace);
	}

	[CompilerGenerated]
	internal static bool ResolveMapper(string asset, string selection, out string[] consumer)
	{
		string pattern = "(?i).*" + selection + ".*?: *(.*)";
		MatchCollection matchCollection = Regex.Matches(asset, pattern);
		if (matchCollection.Count != 0)
		{
			consumer = new string[matchCollection.Count];
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];
				consumer[i] = match.Groups[1].Value.Trim();
			}
			return !consumer.All(string.IsNullOrWhiteSpace);
		}
		consumer = Array.Empty<string>();
		return false;
	}

	[CompilerGenerated]
	internal static string ListMapper(string def)
	{
		if (def.Length < 2)
		{
			return "0" + def;
		}
		return def;
	}

	[CompilerGenerated]
	internal static void VerifyMapper()
	{
		_Service = new MethodVisitor(LogoutMapper());
		_ModelAnnotation = -1;
		m_Config = false;
		_Client = false;
	}

	[CompilerGenerated]
	internal static void FillMapper()
	{
		bool config = m_Config;
		VerifyMapper();
		m_Config = config;
	}

	[CompilerGenerated]
	internal static void WriteMapper()
	{
		bool client = _Client;
		VerifyMapper();
		_Client = client;
	}

	[CompilerGenerated]
	internal static void ForgotMapper(bool istask)
	{
		AnimatorState[] array = _Service.m_ServiceVisitor.Where((AnimatorState s) => s.name.IndexOf("(wd on)", StringComparison.OrdinalIgnoreCase) < 0 && s.name.IndexOf("(wd off)", StringComparison.OrdinalIgnoreCase) < 0).ToArray();
		int num = array.Count((AnimatorState s) => s.writeDefaultValues != istask);
		UnityEngine.Object[] objs = array;
		SerializedObject serializedObject = new SerializedObject(objs);
		serializedObject.FindProperty("m_WriteDefaultValues").boolValue = istask;
		serializedObject.ApplyModifiedProperties();
		CustomizeAnnotation($"Set {num} States' Write Defaults to {istask}.");
	}

	[CompilerGenerated]
	internal static void StopMapper(AnimatorState reference, bool connectionreguired)
	{
		SerializedObject serializedObject = new SerializedObject(reference);
		serializedObject.FindProperty("m_WriteDefaultValues").boolValue = connectionreguired;
		serializedObject.ApplyModifiedProperties();
	}

	[CompilerGenerated]
	internal static bool CheckMapper(SerializedProperty task)
	{
		if (!task.boolValue)
		{
			return task.hasMultipleDifferentValues;
		}
		return true;
	}

	[CompilerGenerated]
	internal static void PrepareMapper(string ident, SerializedProperty selection, SerializedProperty field, SerializedProperty def2, string[] var13, bool res4stop = false)
	{
		if (_Schema == null)
		{
			ConnectAnnotation();
			if (_Schema == null)
			{
				return;
			}
		}
		using (new GUILayout.HorizontalScope())
		{
			if (res4stop)
			{
				EditorGUI.indentLevel++;
			}
			bool flag = CheckMapper(def2);
			if (!(res4stop || flag))
			{
				if (selection == null)
				{
					EditorGUILayout.LabelField(ident);
				}
				else
				{
					EditorGUILayout.PropertyField(selection);
				}
			}
			else
			{
				using (new EditorGUI.DisabledScope(!flag))
				{
					string stringValue = field.stringValue;
					object[] parameters = new object[3]
					{
						new GUIContent(ident),
						field.stringValue,
						var13
					};
					try
					{
						using (new ManagerThread(field))
						{
							EditorGUI.BeginChangeCheck();
							stringValue = (string)ClassProperty.CalcError().LoginError().Invoke(null, parameters);
							if (EditorGUI.EndChangeCheck())
							{
								field.stringValue = stringValue;
							}
						}
					}
					catch
					{
					}
				}
			}
			if (res4stop)
			{
				EditorGUI.indentLevel--;
			}
			using (new ManagerThread(def2))
			{
				EditorGUI.BeginChangeCheck();
				bool boolValue = EditorGUILayout.ToggleLeft("Parameter", def2.boolValue, GUILayout.MaxWidth(100f));
				if (EditorGUI.EndChangeCheck())
				{
					def2.boolValue = boolValue;
				}
			}
		}
	}

	[CompilerGenerated]
	internal static bool AssetMapper(string task)
	{
		if (!structAnnotation || !(task == _SchemaAnnotation))
		{
			if (!structAnnotation)
			{
				return task.Contains(_SchemaAnnotation);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	internal static bool UpdateMapper(string task)
	{
		if (!structAnnotation || !(task == _SchemaAnnotation))
		{
			if (!structAnnotation)
			{
				return task.Contains(_SchemaAnnotation);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	internal static bool ChangeMapper(bool vclose, ref _003C_003Ec__DisplayClass383_1 visitor, ref _003C_003Ec__DisplayClass383_2 comp, ref _003C_003Ec__DisplayClass383_3 second2)
	{
		if (vclose)
		{
			visitor.m_RequestReg = true;
			visitor.m_PrinterReg = comp._WriterReg.name + " -> " + second2._ParamsReg.name;
		}
		return vclose;
	}

	[CompilerGenerated]
	internal static void SortMapper(AnimatorTransitionBase[] key, out List<AnimatorTransitionBase> counter, out List<AnimatorTransitionBase> helper, ref _003C_003Ec__DisplayClass384_0 asset2)
	{
		ComputeAlgo(key, asset2.listenerReg, asset2.m_GetterReg, asset2.m_InterceptorReg, out counter, out helper);
	}

	[CompilerGenerated]
	internal static float RegisterMapper(float def)
	{
		return (float)def.ToString().Length * 4f + 4f;
	}

	internal static bool EnableIndexer()
	{
		return (object)DefineIndexer == null;
	}
}
