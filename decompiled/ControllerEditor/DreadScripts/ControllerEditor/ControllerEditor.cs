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
							if (array[i] != ActiveController())
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

	private class ConditionMultiEditor
	{
		internal bool matched;

		internal AnimatorCondition condition;

		internal readonly List<(AnimatorTransitionBase, int)> targets;

		internal readonly bool[] mixedValues = new bool[3];

		internal ConditionMultiEditor(AnimatorTransitionBase asset, int ivk_low)
		{
			matched = false;
			condition = asset.conditions[ivk_low];
			targets = new List<(AnimatorTransitionBase, int)> { (asset, ivk_low) };
		}

		internal void AddMatch(AnimatorTransitionBase res, int colhigh)
		{
			matched = true;
			targets.Add((res, colhigh));
		}

		internal void ApplyToAll(AnimatorCondition item)
		{
			UnityEngine.Object[] objectsToUndo = targets.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
			Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
			foreach (var target in targets)
			{
				AnimatorTransitionBase item2 = target.Item1;
				int item3 = target.Item2;
				AnimatorCondition[] conditions = item2.conditions;
				conditions[item3] = item;
				item2.conditions = conditions;
			}
		}

		private void ApplyToAll(Func<AnimatorCondition, AnimatorCondition> init)
		{
			UnityEngine.Object[] objectsToUndo = targets.Select(((AnimatorTransitionBase, int) t) => t.Item1).ToArray();
			Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");
			foreach (var target in targets)
			{
				AnimatorTransitionBase item = target.Item1;
				int item2 = target.Item2;
				AnimatorCondition[] conditions = item.conditions;
				conditions[item2] = init(conditions[item2]);
				item.conditions = conditions;
			}
		}

		internal void SetParameter(string value)
		{
			condition = new AnimatorCondition
			{
				parameter = value,
				mode = condition.mode,
				threshold = condition.threshold
			};
			ApplyToAll((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = value,
				mode = c.mode,
				threshold = c.threshold
			});
			mixedValues[0] = false;
		}

		internal void SetMode(AnimatorConditionMode value)
		{
			condition = new AnimatorCondition
			{
				parameter = condition.parameter,
				mode = value,
				threshold = condition.threshold
			};
			ApplyToAll((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = c.parameter,
				mode = value,
				threshold = c.threshold
			});
			mixedValues[1] = false;
		}

		internal void SetThreshold(float ident)
		{
			condition = new AnimatorCondition
			{
				parameter = condition.parameter,
				mode = condition.mode,
				threshold = ident
			};
			ApplyToAll((AnimatorCondition c) => new AnimatorCondition
			{
				parameter = c.parameter,
				mode = c.mode,
				threshold = ident
			});
			mixedValues[2] = false;
		}

		internal void Invert()
		{
			condition = ResolveAlgo(condition);
			ApplyToAll(ResolveAlgo);
		}

		internal void RemoveFromAll()
		{
			foreach (var target in targets)
			{
				target.Item1.RemoveCondition(condition);
			}
		}

		internal void MarkMixedValues(bool[] item)
		{
			for (int i = 0; i < 3; i++)
			{
				mixedValues[i] |= !item[i];
			}
		}
	}

	private class BehaviourPropertyMultiEditor
	{
		internal bool matched;

		internal AnimatorTypeCache.ParameterDriverBinding.ParameterEntry entry;

		internal List<(AnimatorTypeCache.ParameterDriverBinding, int)> targets;

		internal BehaviourPropertyMultiEditor(AnimatorTypeCache.ParameterDriverBinding asset, int cust)
		{
			matched = false;
			entry = asset.parameters[cust];
			targets = new List<(AnimatorTypeCache.ParameterDriverBinding, int)> { (asset, cust) };
		}

		internal void AddMatch(AnimatorTypeCache.ParameterDriverBinding config, int size_connection)
		{
			matched = true;
			targets.Add((config, size_connection));
		}

		internal void ApplyToAll(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry ident)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				AnimatorTypeCache.ParameterDriverBinding item = targets[i].Item1;
				RestartAnnotation(ident, item.parameters[targets[i].Item2]);
				EditorUtility.SetDirty(item.behaviour);
			}
		}

		internal void RemoveFromAll()
		{
			foreach (var target in targets)
			{
				AnimatorTypeCache.ParameterDriverBinding _RegAlgo = target.Item1;
				int item = target.Item2;
				bool num = _RegAlgo.RemoveParameter(item);
				EditorUtility.SetDirty(_RegAlgo.behaviour);
				if (num)
				{
					EditorUtils.ForEach(selectedStates, delegate(AnimatorState s)
					{
						s.RemoveBehaviourAt(s.IndexOfBehaviour(_RegAlgo.behaviour), verifytemp: true);
					});
				}
			}
		}
	}

	private struct TrackingControlEditor
	{
		private readonly SerializedObject serializedObject;

		private SerializedProperty head;

		private SerializedProperty leftHand;

		private SerializedProperty rightHand;

		private SerializedProperty hip;

		private SerializedProperty leftFoot;

		private SerializedProperty rightFoot;

		private SerializedProperty leftFingers;

		private SerializedProperty rightFingers;

		private SerializedProperty eyes;

		private SerializedProperty mouth;

		private readonly List<SerializedProperty> properties;

		private readonly List<GUIContent> labels;

		internal static object PrintIndexer;

		internal TrackingControlEditor(StateMachineBehaviour[] init)
		{
			serializedObject = new SerializedObject(init);
			head = serializedObject.FindProperty("trackingHead");
			leftHand = serializedObject.FindProperty("trackingLeftHand");
			rightHand = serializedObject.FindProperty("trackingRightHand");
			hip = serializedObject.FindProperty("trackingHip");
			leftFoot = serializedObject.FindProperty("trackingLeftFoot");
			rightFoot = serializedObject.FindProperty("trackingRightFoot");
			leftFingers = serializedObject.FindProperty("trackingLeftFingers");
			rightFingers = serializedObject.FindProperty("trackingRightFingers");
			eyes = serializedObject.FindProperty("trackingEyes");
			mouth = serializedObject.FindProperty("trackingMouth");
			properties = new List<SerializedProperty> { head, leftHand, rightHand, hip, leftFoot, rightFoot, leftFingers, rightFingers, eyes, mouth };
			labels = new List<GUIContent>(properties.Select((SerializedProperty t) => new GUIContent(t.displayName.Replace("Tracking ", string.Empty), t.tooltip)));
		}

		private void SetAll(int length_first)
		{
			properties.ForEach(delegate(SerializedProperty p)
			{
				p.enumValueIndex = length_first;
			});
		}

		internal void Draw()
		{
			EditorGUI.BeginDisabledGroup(selectedStates.Count < 1);
			serializedObject.Update();
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
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.red))
					{
						if (EditorUtils.Button(EditorUtils.styles().remove, EditorUtils.styles().paddedBox, GUILayout.Width(25f), GUILayout.Height(20f)))
						{
							EditorUtils.ForEach(selectedStates, delegate(AnimatorState s)
							{
								s.RemoveBehaviourOfType(AnimatorTypeCache.GetTrackingControlType(), isstate: true);
							});
							allStatesHaveTrackingControl = false;
							return;
						}
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					List<SerializedProperty> helperAlgo = properties;
					int num = (properties.All((SerializedProperty p) => !p.hasMultipleDifferentValues && p.enumValueIndex == helperAlgo[0].enumValueIndex) ? properties[0].enumValueIndex : 3);
					using (new GUIColorScope(GUIColorScope.ColoringType.FG, num, colors))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (EditorUtils.Button("All", GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
							{
								int all = ((Event.current.button == 0) ? ((num != 1) ? 1 : 0) : ((num != 2) ? 2 : 0));
								SetAll(all);
							}
							GUILayout.FlexibleSpace();
							EditorGUI.showMixedValue = num == 3;
							EditorGUI.BeginChangeCheck();
							num = EditorGUILayout.Popup(num, properties[0].enumDisplayNames, GUILayout.Width(260f));
							if (EditorGUI.EndChangeCheck())
							{
								SetAll(num);
							}
							EditorGUI.showMixedValue = false;
						}
					}
				}
				EditorUtils.Separator();
				for (int num2 = 0; num2 < properties.Count; num2++)
				{
					SerializedProperty serializedProperty = properties[num2];
					int visZ = ((!serializedProperty.hasMultipleDifferentValues) ? serializedProperty.enumValueIndex : 3);
					using (new GUIColorScope(GUIColorScope.ColoringType.FG, visZ, colors))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (EditorUtils.Button(labels[num2], GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
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
			serializedObject.ApplyModifiedProperties();
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
	private class EditorSettings
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

		internal class SettingsChangeScope : IDisposable
		{
			private readonly Action onChanged;

			private readonly bool previousDeferred;

			private readonly EditorGUI.ChangeCheckScope changeCheck;

			[SpecialName]
			internal bool IsChanged()
			{
				return changeCheck.changed;
			}

			public SettingsChangeScope(Action ident = null)
			{
				onChanged = ident;
				previousDeferred = GetDeferred();
				SetDeferred(isitem: true);
				changeCheck = new EditorGUI.ChangeCheckScope();
			}

			public void Dispose()
			{
				bool changed = changeCheck.changed;
				changeCheck.Dispose();
				if (changed)
				{
					onChanged?.Invoke();
					SaveSettings();
				}
				SetDeferred(previousDeferred);
			}

			public static implicit operator bool(SettingsChangeScope task)
			{
				return task.changeCheck.changed;
			}
		}

		internal class SettingsDeferScope : IDisposable
		{
			private readonly bool previousDeferred;

			public SettingsDeferScope()
			{
				previousDeferred = GetDeferred();
				SetDeferred(isitem: true);
			}

			public void Dispose()
			{
				SetDeferred(previousDeferred);
			}
		}

		[Serializable]
		internal class BoolSetting : SettingBase
		{
			[SerializeField]
			private bool _value;

			internal readonly Action onChange;

			[SpecialName]
			internal bool GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(bool excludeparam)
			{
				if (_value != excludeparam)
				{
					_value = excludeparam;
					onChange?.Invoke();
					SaveSettings();
				}
			}

			internal BoolSetting(bool appendlast, Action ord = null)
			{
				defaultValue = appendlast;
				_value = appendlast;
				onChange = ord;
			}

			internal void Toggle()
			{
				SetValue(!_value);
			}

			internal void Draw(string info, GUIStyle attr = null, params GUILayoutOption[] options)
			{
				Draw(new GUIContent(info), attr, options);
			}

			internal void Draw(GUIContent setup, GUIStyle visitor = null, params GUILayoutOption[] options)
			{
				if (visitor == null)
				{
					visitor = EditorStyles.toggle;
				}
				SetValue(EditorGUILayout.Toggle(setup, GetValue(), visitor, options));
			}

			internal void DrawButton(string last, string vis = null, bool isthird = false, Color? vis2 = null, Color? x3 = null, params GUILayoutOption[] options)
			{
				DrawButton((!string.IsNullOrEmpty(last)) ? new GUIContent(last) : GUIContent.none, (!string.IsNullOrEmpty(vis)) ? new GUIContent(vis) : GUIContent.none, isthird, vis2, x3, options);
			}

			internal void DrawButton(GUIContent init, GUIContent cont = null, bool striptemplate = false, Color? counter2 = null, Color? map3 = null, params GUILayoutOption[] options)
			{
				counter2 = counter2 ?? GUI.backgroundColor;
				map3 = map3 ?? GUI.backgroundColor;
				Color backgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ((!GetValue()) ? map3.Value : counter2.Value);
				SetValue(GUILayout.Toggle(GetValue(), (!GetValue() && cont != null) ? cont : init, (!striptemplate) ? GUI.skin.button : EditorStyles.toolbarButton, options));
				GUI.backgroundColor = backgroundColor;
			}

			public static implicit operator bool(BoolSetting key)
			{
				return key._value;
			}

			internal override void Reset()
			{
				SetValue((bool)defaultValue);
			}
		}

		[Serializable]
		internal class FloatSetting : SettingBase
		{
			[SerializeField]
			private float _value;

			internal readonly Action identifierAlgo;

			[SpecialName]
			internal float GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(float reference)
			{
				if (_value != reference)
				{
					_value = reference;
					identifierAlgo?.Invoke();
					SaveSettings();
				}
			}

			internal FloatSetting(float ident, Action result = null)
			{
				defaultValue = ident;
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
					SetValue(EditorGUILayout.FloatField(info, GetValue(), dir, options));
					if (havesecond && EditorUtils.IconButton(EditorUtils.contents().reset))
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
					SetValue(EditorGUILayout.Slider(last, GetValue(), cfg, temp, options));
					if (compareconfig2)
					{
						while (EditorUtils.IconButton(EditorUtils.contents().reset))
						{
							Reset();
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
				SetValue((float)defaultValue);
			}

			public static implicit operator int(FloatSetting key)
			{
				return (int)key._value;
			}

			public static implicit operator float(FloatSetting item)
			{
				return item._value;
			}
		}

		[Serializable]
		internal class EnumSetting : FloatSetting
		{
			[SerializeField]
			internal int IntValue
			{
				get
				{
					return (int)GetValue();
				}
				set
				{
					SetValue(value);
				}
			}

			internal EnumSetting(int previous_task, Action vis = null)
				: base(previous_task, vis)
			{
			}

			internal T GetEnumValue<T>() where T : Enum
			{
				return (T)(object)IntValue;
			}

			internal void DrawIntField(GUIContent value, GUIStyle cont = null, params GUILayoutOption[] options)
			{
				if (cont == null)
				{
					cont = EditorStyles.numberField;
				}
				IntValue = EditorGUILayout.IntField(value, IntValue, cont, options);
			}

			internal void DrawIntField(string spec, GUIStyle reg = null, params GUILayoutOption[] options)
			{
				DrawIntField(new GUIContent(spec), reg, options);
			}

			internal void DrawEnumPopup<T>(GUIContent info, bool ismap = false, GUIStyle tag = null, params GUILayoutOption[] options) where T : Enum
			{
				if (tag == null)
				{
					tag = EditorStyles.popup;
				}
				IntValue = ((!ismap) ? ((int)(object)EditorGUILayout.EnumPopup(info, (T)(object)IntValue, tag, options)) : ((int)(object)EditorGUILayout.EnumFlagsField(info, (T)(object)IntValue, tag, options)));
			}

			internal void DrawEnumPopup<T>(string spec, bool hasreg = false, GUIStyle template = null, params GUILayoutOption[] options) where T : Enum
			{
				DrawEnumPopup<T>(new GUIContent(spec), hasreg, template, options);
			}

			internal static EnumSetting FromEnum<T>(T ident, Action result = null) where T : Enum
			{
				return new EnumSetting((int)(object)ident, result);
			}

			public static implicit operator int(EnumSetting param)
			{
				return param.IntValue;
			}

			public static implicit operator float(EnumSetting var1)
			{
				return var1.IntValue;
			}
		}

		[Serializable]
		internal class VectorSetting : SettingBase
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
					SaveSettings();
				}
			}

			internal void IncludeDefinition(Vector3 config, Action ord)
			{
				defaultValue = config;
				_RegistryAlgo = ord;
				_valueX = config.x;
				_valueY = config.y;
				_valueZ = config.z;
			}

			internal VectorSetting(Vector3 value, Action selection = null)
			{
				IncludeDefinition(value, selection);
			}

			internal VectorSetting(float init, float token, float pool, Action second2 = null)
			{
				IncludeDefinition(new Vector3(init, token, pool), second2);
			}

			internal VectorSetting(float asset, float ivk, Action tag = null)
			{
				IncludeDefinition(new Vector3(asset, ivk), tag);
			}

			internal void RunDefinition(GUIContent init, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label(init, GUILayout.MaxWidth(117f));
					CreateDefinition(EditorGUILayout.Vector2Field(GUIContent.none, DeleteDefinition(), options));
					if (GUILayout.Button(EditorUtils.contents().reset, EditorUtils.styles().tightLabel, GUILayout.Width(18f), GUILayout.Height(18f)))
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
				CreateDefinition((Vector3)defaultValue);
			}

			public static implicit operator Vector2(VectorSetting asset)
			{
				return asset.DeleteDefinition();
			}
		}

		[Serializable]
		internal class StringSetting : SettingBase
		{
			[SerializeField]
			private string _value;

			internal readonly Action onChanged;

			[SpecialName]
			internal string GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(string value)
			{
				if (_value != value)
				{
					while (true)
					{
						_value = value;
					}
				}
			}

			internal StringSetting(string last = "", Action pred = null)
			{
				defaultValue = last;
				_value = last;
				onChanged = pred;
			}

			internal void Draw(string first, bool outputb = true, bool containstemplate = true, GUIStyle def2 = null, params GUILayoutOption[] options)
			{
				Draw(new GUIContent(first), outputb, containstemplate, def2, options);
			}

			internal void Draw(GUIContent def, bool setcust = true, bool deletehelper = true, GUIStyle counter2 = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					if (counter2 == null)
					{
						counter2 = EditorStyles.textField;
					}
					SetValue((!deletehelper) ? EditorGUILayout.TextField(def, GetValue(), counter2, options) : EditorGUILayout.DelayedTextField(def, GetValue(), counter2, options));
					if (setcust && EditorUtils.IconButton(EditorUtils.contents().reset))
					{
						Reset();
					}
				}
			}

			internal override void Reset()
			{
				SetValue((string)defaultValue);
			}

			public override string ToString()
			{
				return GetValue();
			}

			public static implicit operator string(StringSetting res)
			{
				return res._value;
			}
		}

		[Serializable]
		internal class ColorSetting : SettingBase
		{
			internal readonly Action onChange;

			[SerializeField]
			private float r;

			[SerializeField]
			private float g;

			[SerializeField]
			private float b;

			[SerializeField]
			private float a;

			[SpecialName]
			internal Color GetValue()
			{
				return new Color(r, g, b, a);
			}

			[SpecialName]
			internal void SetValue(Color value)
			{
				r = value.r;
				g = value.g;
				b = value.b;
				a = value.a;
				onChange?.Invoke();
				SaveSettings();
			}

			internal ColorSetting(float item, float second, float state, float visitor2 = 1f, Action res3 = null)
			{
				Color color = new Color(item, second, state, visitor2);
				defaultValue = color;
				r = item;
				g = second;
				b = state;
				a = visitor2;
				onChange = res3;
			}

			internal ColorSetting(Color info, Action cont = null)
			{
				defaultValue = info;
				r = info.r;
				g = info.g;
				b = info.b;
				a = info.a;
				onChange = cont;
			}

			internal void Draw(string setup, bool isvis = true, params GUILayoutOption[] options)
			{
				Draw(new GUIContent(setup), isvis, options);
			}

			internal void Draw(GUIContent task, bool outputtoken = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetValue(EditorGUILayout.ColorField(task, GetValue(), options));
					if (outputtoken && EditorUtils.IconButton(EditorUtils.contents().reset))
					{
						Reset();
					}
				}
			}

			internal override void Reset()
			{
				SetValue((Color)defaultValue);
			}
		}

		[Serializable]
		internal class ObjectReferenceSetting : SettingBase
		{
			internal readonly Action onChange;

			private readonly Type objectType;

			[SerializeField]
			internal string guid;

			[SerializeField]
			internal long localID;

			private string defaultGuid;

			private long defaultLocalID;

			private bool isCached;

			private UnityEngine.Object cachedObject;

			[SpecialName]
			internal UnityEngine.Object GetValue()
			{
				if (!isCached)
				{
					isCached = true;
					cachedObject = LoadAsset<UnityEngine.Object>(guid, localID);
				}
				return cachedObject;
			}

			[SpecialName]
			internal void SetValue(UnityEngine.Object reference)
			{
				if (cachedObject != reference)
				{
					cachedObject = reference;
					if (reference == null)
					{
						guid = string.Empty;
						localID = 0L;
					}
					else
					{
						AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out guid, out localID);
					}
					onChange?.Invoke();
					SaveSettings();
				}
			}

			internal ObjectReferenceSetting(Type asset, string ivk = "", long column_comp = 0L, Action pol2 = null)
			{
				objectType = asset;
				defaultGuid = ivk;
				defaultLocalID = column_comp;
				guid = ivk;
				localID = column_comp;
				onChange = pol2;
			}

			internal void Draw(string item, bool istoken = true, params GUILayoutOption[] options)
			{
				Draw(new GUIContent(item), istoken, options);
			}

			internal void Draw(GUIContent def, bool isb = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetValue(EditorGUILayout.ObjectField(def, GetValue(), objectType, allowSceneObjects: false, options));
					if (isb && EditorUtils.IconButton(EditorUtils.contents().reset))
					{
						Reset();
					}
				}
			}

			private static T LoadAsset<T>(string key, long indexOf_pol) where T : UnityEngine.Object
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

			internal T GetValue<T>() where T : UnityEngine.Object
			{
				return (T)GetValue();
			}

			internal override void Reset()
			{
				SetValue(LoadAsset<UnityEngine.Object>(defaultGuid, defaultLocalID));
			}

			public static implicit operator bool(ObjectReferenceSetting last)
			{
				return last.GetValue();
			}
		}

		internal abstract class SettingBase
		{
			internal object defaultValue;

			internal abstract void Reset();
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal class NonSerializedSettingAttribute : Attribute
		{
		}

		[SerializeField]
		internal BoolSetting a_VerifyOnDisplay = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting a_VerifyOnProjectLoad = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting editingTransitions = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting editingStates = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting editingController = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting matchParameter = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting matchMode = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting matchValue = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting showTransitionSettings = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting showTransitionConditions = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting showMatchingOptions = new BoolSetting(appendlast: false, UpdateVisitor);

		[SerializeField]
		internal BoolSetting showTransitionsCount = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting showStateSettings = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting showStateCount = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting showVRCDrivers = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting showVRCTracking = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting useLegacyDropdown = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting switchDoubleClick = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting autoReverseModes = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting reverseModifiesValues = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting animateInboundEdges = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting animateOutboundEdges = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting autoFrameLayer = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting displayLayerIndex = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting displayParameterType = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting capitalParameterIndicator = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting aw_active = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting aw_autoSwitchClip = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting aw_enablePropertyEditing = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting aw_enableGameObjectDND = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting aw_enableOverride = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting aw_warnPropertyMerge = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting graphBackgroundIsTexture = new BoolSetting(appendlast: false, ApplyGraphBackground);

		[SerializeField]
		internal BoolSetting cosmeticGraphActive = new BoolSetting(appendlast: false, ApplyGraphBackground);

		[SerializeField]
		internal BoolSetting cosmeticNodesActive = new BoolSetting(appendlast: false, PatchAlgo);

		[SerializeField]
		internal BoolSetting cosmeticTransitionsActive = new BoolSetting(appendlast: false, PatchAlgo);

		[SerializeField]
		internal BoolSetting hasPingedController = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting requiresStateRename = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting advancedQuickToggle = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting mergeQuickToggle = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting warnParameterConversion = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting displayCategoryView = new BoolSetting(appendlast: true, delegate
		{
			GetInstance().sortCategoryViewLayers.SetValue(excludeparam: false);
			layerViewType = LayerViewViewType.DefaultView;
		});

		[SerializeField]
		internal BoolSetting sortCategoryViewLayers = new BoolSetting(appendlast: true);

		[SerializeField]
		internal BoolSetting displayLayerCompactView = new BoolSetting(appendlast: true, delegate
		{
			GetInstance().layerCompactView.SetValue(excludeparam: false);
		});

		[SerializeField]
		internal BoolSetting layerCompactView = new BoolSetting(appendlast: false, PublishAnnotation);

		[SerializeField]
		internal FloatSetting anyStateNodeColor = new FloatSetting(2f, PatchAlgo);

		[SerializeField]
		internal FloatSetting entryStateNodeColor = new FloatSetting(3f, PatchAlgo);

		[SerializeField]
		internal FloatSetting exitStateNodeColor = new FloatSetting(6f, PatchAlgo);

		[SerializeField]
		internal FloatSetting machineStateNodeColor = new FloatSetting(0f, PatchAlgo);

		[SerializeField]
		internal FloatSetting normalStateNodeColor = new FloatSetting(0f, PatchAlgo);

		[SerializeField]
		internal FloatSetting defaultStateNodeColor = new FloatSetting(5f, PatchAlgo);

		[SerializeField]
		internal FloatSetting defaultLayerWeight = new FloatSetting(1f);

		[SerializeField]
		internal FloatSetting arrowLerpRatio = new FloatSetting(-0.5f);

		[SerializeField]
		internal VectorSetting defaultEntryPosition = new VectorSetting(50f, 120f);

		[SerializeField]
		internal VectorSetting defaultExitPosition = new VectorSetting(800f, 120f);

		[SerializeField]
		internal VectorSetting defaultAnyPosition = new VectorSetting(50f, 20f);

		[SerializeField]
		internal ColorSetting normalTransitionColor = new ColorSetting(1f, 1f, 1f);

		[SerializeField]
		internal ColorSetting entryTransitionColor = new ColorSetting(0.6f, 0.4f, 0f);

		[SerializeField]
		internal ColorSetting selectedTransitionColor = new ColorSetting(0.42f, 0.7f, 1f);

		[SerializeField]
		internal ColorSetting baseTransitionColor = new ColorSetting(0.5f, 0.5f, 0.5f);

		[SerializeField]
		internal ColorSetting gridBackgroundColor = new ColorSetting(0.1647f, 0.1647f, 0.16f, 1f, ApplyGraphBackground);

		[SerializeField]
		internal ColorSetting gridMinorLightColor = new ColorSetting(0f, 0f, 0f, 0.1f);

		[SerializeField]
		internal ColorSetting gridMajorLightColor = new ColorSetting(0f, 0f, 0f, 0.15f);

		[SerializeField]
		internal ColorSetting gridMinorDarkColor = new ColorSetting(0f, 0f, 0f, 0.18f);

		[SerializeField]
		internal ColorSetting gridMajorDarkColor = new ColorSetting(0f, 0f, 0f, 0.28f);

		[SerializeField]
		internal ColorSetting parameterLabelColor = new ColorSetting(0.7f, 0.7f, 0.7f);

		[SerializeField]
		internal ObjectReferenceSetting defaultLayerMask = new ObjectReferenceSetting(typeof(AvatarMask));

		[SerializeField]
		internal ObjectReferenceSetting graphBackgroundTexture = new ObjectReferenceSetting(typeof(Texture2D), "", 0L, ApplyGraphBackground);

		[SerializeField]
		internal StringSetting saveFolder = new StringSetting("Assets/DreadScripts/ControllerEditor/Generated Assets");

		[SerializeField]
		internal StringSetting lastAnimationPath = new StringSetting("Assets");

		[SerializeField]
		internal StringSetting lastAnimationName = new StringSetting("New Animation Clip");

		[SerializeField]
		internal StringSetting categoryBaseName = new StringSetting("Base");

		[SerializeField]
		internal StringSetting categoryDelimiter = new StringSetting("/");

		[SerializeField]
		internal EnumSetting parameterLabelFontStyle = EnumSetting.FromEnum(FontStyle.Normal, RebuildParameterLabelStyle);

		[SerializeField]
		internal EnumSetting stateCosmetics = EnumSetting.FromEnum(StateCosmeticOptions.all);

		[NonSerializedSetting]
		internal AnimatorState defaultState;

		[NonSerializedSetting]
		internal AnimatorStateTransition defaultTransition;

		[NonSerialized]
		internal static GUIStyle parameterLabelStyle;

		private static bool _InterpreterAlgo;

		private static bool pendingSave;

		private static bool deferred;

		private static FieldInfo[] nonSerializedSettingFields;

		private static EditorSettings instance;

		internal static Action onSettingsCleared;

		[SerializeField]
		internal StringSetting u_updateLink = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateVersion = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateMessage = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateChangelog = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateDay = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcement = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementLink = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementLinkName = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementHiddenDate = new StringSetting();

		[SerializeField]
		internal BoolSetting u_updateHidden = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting u_announcementHidden = new BoolSetting(appendlast: false);

		[SerializeField]
		internal BoolSetting a_HasSucceededLastVerification = new BoolSetting(appendlast: false);

		internal static void RebuildParameterLabelStyle()
		{
			parameterLabelStyle = new GUIStyle(EditorUtils.styles().noteRight)
			{
				fontStyle = GetInstance().parameterLabelFontStyle.GetEnumValue<FontStyle>()
			};
		}

		internal StateCosmeticOptions GetStateCosmetics()
		{
			return stateCosmetics.GetEnumValue<StateCosmeticOptions>();
		}

		[SpecialName]
		internal static bool GetDeferred()
		{
			return deferred;
		}

		[SpecialName]
		internal static void SetDeferred(bool isitem)
		{
			bool num = deferred;
			deferred = isitem;
			if (num && !deferred && pendingSave)
			{
				SaveSettings();
			}
		}

		[SpecialName]
		internal static EditorSettings GetInstance()
		{
			if (instance == null)
			{
				LoadSettings();
			}
			return instance;
		}

		private EditorSettings()
		{
			nonSerializedSettingFields = (from m in typeof(EditorSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				where m.IsDefined(typeof(NonSerializedSettingAttribute), inherit: false)
				select m).ToArray();
		}

		internal static void SaveSettings()
		{
			pendingSave = false;
			if (deferred)
			{
				pendingSave = true;
			}
			else
			{
				if (_InterpreterAlgo)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(GetInstance()) + "]\u200b\u200b\u200b");
				FieldInfo[] array = nonSerializedSettingFields;
				foreach (FieldInfo fieldInfo in array)
				{
					try
					{
						string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(GetInstance()));
						stringBuilder.Append(fieldInfo.Name + "[" + text + "]\u200b\u200b\u200b");
					}
					catch (Exception message)
					{
						UnityEngine.Debug.LogError(message);
					}
				}
				string value = stringBuilder.ToString();
				EditorPrefs.SetString("yOk0XCnENLMO6DIF8cYpSg==SettingsJSON", value);
			}
		}

		private static void LoadSettings()
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
				instance = JsonUtility.FromJson<EditorSettings>(value);
			}
			if (instance == null)
			{
				instance = new EditorSettings();
			}
			FieldInfo[] array = nonSerializedSettingFields;
			foreach (FieldInfo fieldInfo in array)
			{
				object obj = fieldInfo.GetValue(instance) ?? Activator.CreateInstance(fieldInfo.FieldType);
				if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
				{
					EditorJsonUtility.FromJsonOverwrite(value2, obj);
				}
				fieldInfo.SetValue(instance, obj);
				if (fieldInfo.GetValue(instance) == null)
				{
					fieldInfo.SetValue(instance, Activator.CreateInstance(fieldInfo.FieldType));
				}
			}
		}

		internal static void PromptClearSettings()
		{
			if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
			{
				ClearSettings();
			}
		}

		internal static void ClearSettings()
		{
			instance = new EditorSettings();
			FieldInfo[] array = nonSerializedSettingFields;
			foreach (FieldInfo fieldInfo in array)
			{
				fieldInfo.SetValue(instance, Activator.CreateInstance(fieldInfo.FieldType));
			}
			onSettingsCleared?.Invoke();
			SaveSettings();
		}
	}

	private static class BugReporter
	{
		internal struct ErrorInfo
		{
			internal string name;

			internal ushort id;

			internal ushort version;

			internal string exceptionMessage;
		}

		private static string m_AccountAlgo;

		private static bool m_RefAlgo;

		private static bool _StatusAlgo;

		private static bool m_TokenAlgo;

		private static bool codeAlgo;

		private static ErrorInfo? _DicAlgo;

		private static ErrorInfo? _InvocationAlgo;

		private static Action roleAlgo;

		private static ushort _ParamAlgo;

		internal static bool m_ModelAlgo;

		internal static readonly HashSet<ErrorInfo> m_TokenizerAlgo = new HashSet<ErrorInfo>();

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
				CompareReg(pool_amount, v2, selection3_length);
			}
			try
			{
				res();
			}
			catch (Exception info)
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

		private static void SearchDefinition(Exception info, bool isselection = false, string state = "")
		{
			if (!_InvocationAlgo.HasValue || m_TokenizerAlgo.Contains(_InvocationAlgo.Value))
			{
				return;
			}
			m_AccountAlgo = string.Empty;
			m_RefAlgo = false;
			m_TokenAlgo = false;
			_StatusAlgo = false;
			_DicAlgo = new ErrorInfo
			{
				name = _InvocationAlgo.Value.name,
				id = _InvocationAlgo.Value.id,
				version = _InvocationAlgo.Value.version,
				exceptionMessage = info.Message
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
				GUILayout.Label(EditorUtils.contents().error, EditorUtils.styles().iconButton);
				GUILayout.Label("An error has occurred! Do you want to report it?", EditorStyles.boldLabel);
				if (EditorUtils.Button("Ignore"))
				{
					SetupReg(isinstance: false);
				}
				if (EditorUtils.Button("Find Solution"))
				{
					SetupReg(isinstance: true);
				}
			}
			if (isvar1)
			{
				EditorUtils.Separator();
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
			_InvocationAlgo = new ErrorInfo
			{
				id = num_ident,
				name = ivk,
				version = dir_count
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
			ComputeInitializer(isLicensed && _DicAlgo.HasValue);
			if (!m_TokenAlgo)
			{
				m_TokenAlgo = true;
				codeAlgo = true;
				List<(string, string)> list = RegisterAnnotation("findsolution", new(string, string)[4]
				{
					("bug_id", _DicAlgo.Value.id.ToString()),
					("bug_version", _DicAlgo.Value.version.ToString()),
					("bug_name", _DicAlgo.Value.name),
					("bug_exception", Uri.EscapeUriString(_DicAlgo.Value.exceptionMessage))
				});
				LogoutAnnotation(list);
				DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
				{
					bool flag = response.Item("success");
					string text = response.Item("message");
					_StatusAlgo = true;
					if (!string.IsNullOrWhiteSpace(text))
					{
						Log(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
					}
					m_AccountAlgo = response.Item("solution");
					m_RefAlgo = response.Item("complete");
				}, UnityEngine.Debug.LogException, null, null, delegate
				{
					codeAlgo = false;
					DrawLicenseInfo();
				});
			}
			SetVisitor(codeAlgo ? "Finding a solution..." : "Bug Reporter", "If you have found a bug, please report it here!\nNote that the report is not anonymous. Abuse may result in blacklisting.");
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (codeAlgo)
				{
					if (EditorUtils.Button("Cancel", EditorStyles.toolbarButton))
					{
						ComputeInitializer(ignoresetup: false);
					}
					return;
				}
				if (_StatusAlgo)
				{
					if (string.IsNullOrWhiteSpace(m_AccountAlgo))
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, EditorUtils.warningColor))
						{
							GUILayout.Label("No solution Found! Please write the steps to reproduce this issue below:");
						}
						unusedLicensingText = EditorGUILayout.TextArea(unusedLicensingText, GUILayout.MinHeight(54f));
						if (!string.IsNullOrWhiteSpace(unusedLicensingText) && unusedLicensingText.Length > 2000)
						{
							unusedLicensingText = unusedLicensingText.Substring(0, 2000);
						}
						if (!string.IsNullOrWhiteSpace(m_AccountAlgo))
						{
							return;
						}
						using (new GUILayout.HorizontalScope())
						{
							if (EditorUtils.Button("Cancel", GUILayout.ExpandWidth(expand: false)))
							{
								ComputeInitializer(ignoresetup: false);
							}
							using (new EditorGUI.DisabledScope(unusedLicensingFlag))
							{
								if (!EditorUtils.Button("Report Issue"))
								{
									return;
								}
								List<(string, string)> list2 = RegisterAnnotation("reportbug", new(string, string)[5]
								{
									("bug_id", _DicAlgo.Value.id.ToString()),
									("bug_version", _DicAlgo.Value.version.ToString()),
									("bug_name", _DicAlgo.Value.name),
									("bug_exception", _DicAlgo.Value.exceptionMessage),
									("feedback", Uri.EscapeUriString(unusedLicensingText))
								});
								LogoutAnnotation(list2);
								unusedLicensingFlag = true;
								DisableVisitor(CallVisitor(list2.ToArray())).QueryRules(delegate(JsonObject response)
								{
									bool flag = response.Item("success");
									string text = response.Item("message");
									if (!string.IsNullOrEmpty(text))
									{
										Log(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
									}
								}, UnityEngine.Debug.LogException, null, null, delegate
								{
									ComputeInitializer(ignoresetup: false);
									unusedLicensingFlag = false;
									DrawLicenseInfo();
								});
								return;
							}
						}
					}
					if (m_RefAlgo)
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, EditorUtils.validColor))
						{
							GUILayout.Label("Solution Found!");
						}
					}
					else
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, EditorUtils.warningColor))
						{
							GUILayout.Label("Known issue! Details:");
						}
					}
					EditorGUILayout.Space();
					EditorGUILayout.SelectableLabel(m_AccountAlgo, GUI.skin.label, GUILayout.ExpandHeight(expand: false));
					if (EditorUtils.Button("Ok"))
					{
						ComputeInitializer(ignoresetup: false);
					}
					return;
				}
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(EditorUtils.contents().error, EditorUtils.styles().iconButton);
					using (new GUIColorScope(GUIColorScope.ColoringType.FG, EditorUtils.errorColor))
					{
						GUILayout.Label("There was an issue contacting the server for a solution.");
					}
				}
				if (EditorUtils.Button("Cancel"))
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
				ManageDefinition(roleAlgo, _DicAlgo.Value.id, _DicAlgo.Value.name, _DicAlgo.Value.version);
			}
			roleAlgo = null;
			CompilationPipeline.compilationStarted -= EnableReg;
		}
	}

	private sealed class ProcessRunner
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

		internal ProcessRunner(string var1, Action<string> col, bool isfield = false, bool containscol2 = false, Action token3 = null)
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
			catch (Exception ex)
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
	}

	[DefaultMember("Item")]
	internal readonly struct JsonObject
	{
		private readonly string raw;

		private readonly Dictionary<string, JsonValue> values;

		internal readonly bool isEmpty;

		private static object PushState;

		internal JsonObject(string i)
		{
			raw = i;
			MatchCollection matchCollection = Regex.Matches(i, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				isEmpty = false;
				values = new Dictionary<string, JsonValue>();
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
						values[value] = new JsonValue(value2);
					}
				}
			}
			else
			{
				isEmpty = true;
				values = null;
			}
		}

		[SpecialName]
		internal JsonValue Item(string ident)
		{
			values.TryGetValue(ident, out var value);
			return value;
		}

		public override string ToString()
		{
			return raw;
		}

		public string ToString(bool explicitinit)
		{
			if (!explicitinit)
			{
				return ToString();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("{");
			foreach (KeyValuePair<string, JsonValue> value in values)
			{
				stringBuilder.AppendLine($"{value.Key}: {value.Value},");
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		internal static bool PrepareState()
		{
			return PushState == null;
		}
	}

	internal readonly struct JsonValue
	{
		internal readonly string raw;

		internal readonly string stringValue;

		internal readonly bool boolValue;

		internal readonly float floatValue;

		internal readonly bool hasValue;

		internal static object PrintState;

		internal JsonValue(string last)
		{
			raw = last;
			hasValue = true;
			if (last.Length > 1)
			{
				if (last.StartsWith("\"") && last.EndsWith("\""))
				{
					stringValue = ((last.Length != 2) ? last.Substring(1, last.Length - 2) : string.Empty);
				}
				else
				{
					stringValue = last;
				}
			}
			else
			{
				stringValue = last;
			}
			boolValue = stringValue == "true";
			float.TryParse(stringValue, out floatValue);
		}

		public override string ToString()
		{
			return stringValue;
		}

		public static implicit operator string(JsonValue var1)
		{
			return var1.stringValue;
		}

		public static implicit operator bool(JsonValue key)
		{
			return key.boolValue;
		}

		public static implicit operator float(JsonValue key)
		{
			return key.floatValue;
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

	internal static class MotionEmbedMenu
	{
		[MenuItem("CONTEXT/AnimatorState/Motion/Embed", true)]
		private static bool ValidateEmbed(MenuCommand item)
		{
			return CanEmbed(item.context as AnimatorState);
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Embed")]
		private static void EmbedMotion(MenuCommand res)
		{
			EmbedMotion(res.context as AnimatorState);
		}

		private static bool CanEmbed(AnimatorState instance)
		{
			if (!(instance != null) || !(instance.motion != null))
			{
				return false;
			}
			return !IsEmbedded(instance.motion);
		}

		private static void EmbedMotion(AnimatorState setup)
		{
			if (!CanEmbed(setup))
			{
				return;
			}
			Motion motion = setup.motion;
			if (!IsEmbedded(motion) || EditorUtility.DisplayDialog("Caution", "The motion is already embedded into another controller. Do you want to move it anyway?", "Continue", "Cancel"))
			{
				string assetPath = AssetDatabase.GetAssetPath(setup);
				if (!string.IsNullOrEmpty(assetPath))
				{
					RemoveFromAsset(motion);
					AssetDatabase.AddObjectToAsset(motion, assetPath);
					motion.hideFlags |= HideFlags.HideInHierarchy;
					EditorUtility.SetDirty(motion);
					EditorSceneManager.MarkAllScenesDirty();
				}
			}
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Extract", true)]
		private static bool ValidateExtract(MenuCommand config)
		{
			return CanExtract(config.context as AnimatorState);
		}

		private static bool CanExtract(AnimatorState i)
		{
			if (!(i != null) || !(i.motion != null))
			{
				return false;
			}
			return IsEmbedded(i.motion);
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Extract")]
		private static void ExtractMotion(MenuCommand first)
		{
			ExtractMotion(first.context as AnimatorState);
		}

		private static void ExtractMotion(AnimatorState asset)
		{
			if (CanExtract(asset))
			{
				Motion motion = asset.motion;
				RemoveFromAsset(motion);
				string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(asset)) + "/" + SanitizeFileName(motion.name) + ".anim";
				AssetDatabase.CreateAsset(motion, path);
				motion.hideFlags &= ~HideFlags.HideInHierarchy;
				EditorUtility.SetDirty(motion);
				EditorSceneManager.MarkAllScenesDirty();
				AssetDatabase.Refresh();
			}
		}

		private static void RemoveFromAsset(Motion var1)
		{
			string assetPath = AssetDatabase.GetAssetPath(var1);
			while (!string.IsNullOrEmpty(assetPath))
			{
				bool num = AssetDatabase.LoadAllAssetsAtPath(assetPath).Length == 1;
				AssetDatabase.RemoveObjectFromAsset(var1);
				if (num)
				{
					AssetDatabase.DeleteAsset(assetPath);
					continue;
				}
				break;
			}
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Rename", true)]
		private static bool ValidateRename(MenuCommand ident)
		{
			AnimatorState animatorState = ident.context as AnimatorState;
			if (animatorState != null)
			{
				return animatorState.motion != null;
			}
			return false;
		}

		[MenuItem("CONTEXT/AnimatorState/Motion/Rename")]
		private static void RenameMotion(MenuCommand res)
		{
			AnimatorState animatorState = res.context as AnimatorState;
			if (!(animatorState == null) && !(animatorState.motion == null))
			{
				RenameMotion(animatorState.motion);
			}
		}

		private static void RenameMotion(Motion setup)
		{
			if (setup == null)
			{
				return;
			}
			MotionRenamerWindow window = EditorWindow.GetWindow<MotionRenamerWindow>(utility: true, "Motion Rename");
			window.motions.Add(setup);
			if (window.motions.Count != 1)
			{
				return;
			}
			window.newName = setup.name;
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

		internal static void MarkScenesDirty()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		internal static string SanitizeFileName(string init)
		{
			string text = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
			if (string.IsNullOrEmpty(init))
			{
				return "Unnamed";
			}
			return Regex.Replace(init, "[" + text + "]", "-");
		}

		internal static bool IsEmbedded(UnityEngine.Object spec)
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

		internal static string GenerateUniqueName(string info, string cont)
		{
			cont = SanitizeFileName(cont);
			string directoryName = Path.GetDirectoryName(info);
			string extension = Path.GetExtension(info);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
				AssetDatabase.ImportAsset(directoryName);
			}
			return Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(directoryName + "/" + cont + extension));
		}
	}

	internal static class HarmonyPatchManager
	{
		internal struct PatchSwapEntry
		{
			internal readonly MethodInfo triggerMethod;

			internal readonly MethodInfo triggerPatch;

			internal readonly MethodInfo targetMethod;

			internal readonly MethodInfo prefix;

			internal readonly MethodInfo postfix;

			internal readonly MethodInfo transpiler;

			internal static object ConnectProduct;

			internal PatchSwapEntry(MethodInfo asset, MethodInfo selection, MethodInfo field, MethodInfo ord2 = null, MethodInfo first3 = null, MethodInfo key4 = null)
			{
				triggerMethod = asset;
				triggerPatch = selection;
				targetMethod = field;
				prefix = ord2;
				postfix = first3;
				transpiler = key4;
			}

			internal static bool ViewProduct()
			{
				return ConnectProduct == null;
			}
		}

		internal delegate void RefAction<T>(ref T arg);

		internal delegate void RefAction<T, TT>(ref T arg1, ref TT arg2);

		internal delegate void RefAction<T, TT, T3>(ref T arg1, ref TT arg2, ref T3 arg3);

		internal delegate void RefAction<T, TT, T3, G>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

		internal delegate void RefAction<T, TT, T3, G, GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

		internal delegate void RefAction<T, TT, T3, G, GG, A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

		internal delegate AA RefFunc<T, TT, T3, G, GG, A, out AA>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

		internal delegate A RefFunc<T, TT, T3, G, GG, out A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

		internal delegate GG RefFunc<T, TT, T3, G, out GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

		internal delegate G RefFunc<T, TT, T3, out G>(ref T arg1, ref TT arg2, ref T3 arg3);

		internal delegate T3 RefFunc<T, TT, out T3>(ref T arg1, ref TT arg2);

		internal delegate TT RefFunc<T, out TT>(ref T arg);

		internal delegate void OutAction<T>(out T arg);

		internal delegate void OutAction<T, TT>(out T arg1, out TT arg2);

		internal delegate void OutAction<T, TT, T3>(out T arg1, out TT arg2, out T3 arg3);

		internal delegate void OutAction<T, TT, T3, G>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

		internal delegate void OutAction<T, TT, T3, G, GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

		internal delegate void OutAction<T, TT, T3, G, GG, A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

		internal delegate AA OutFunc<T, TT, T3, G, GG, A, out AA>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

		internal delegate A OutFunc<T, TT, T3, G, GG, out A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

		internal delegate GG OutFunc<T, TT, T3, G, out GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

		internal delegate G OutFunc<T, TT, T3, out G>(out T arg1, out TT arg2, out T3 arg3);

		internal delegate T3 OutFunc<T, TT, out T3>(out T arg1, out TT arg2);

		internal delegate TT OutFunc<T, out TT>(out T arg);

		internal delegate void ValValRefRefAction<T, TT, T3, G>(T arg1, TT arg2, ref T3 arg3, ref G arg4);

		internal delegate void RefValAction<T, in TT>(ref T arg1, TT arg2);

		internal delegate void ValRefAction<in T, TT>(T arg1, ref TT arg2);

		internal delegate void ValOutAction<in T, TT>(T arg1, out TT arg2);

		internal delegate void ValValOutAction<in T, in TT, T3>(T arg1, TT arg2, out T3 arg3);

		internal delegate void ValOutValAction<in T, TT, in T3>(T arg1, out TT arg2, T3 arg3);

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec candidateMapper = new _003C_003Ec();

			public static Func<ParameterInfo, Type> m_ProductMapper;

			public static Func<ParameterInfo, Type> m_ExpressionMapper;

			public static Action systemMapper;

			public static Func<Task> m_WorkerMapper;

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
				EditorUtils.DelayCall(delegate
				{
					try
					{
						RetryPatching();
					}
					catch (Exception exception)
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
					RetryPatching();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				finally
				{
					_StructAlgo = false;
				}
			}
		}

		internal static Dictionary<string, Harmony> harmonyInstances;

		private static Dictionary<string, PatchSwapEntry> deferredPatches = new Dictionary<string, PatchSwapEntry>();

		internal static bool m_BroadcasterAlgo;

		internal static bool patchingFailed;

		internal static bool _StructAlgo;

		internal static bool _ServiceAlgo;

		internal static bool hasRetried;

		internal static string patchErrorLog;

		internal static readonly (Action, bool)[] patchAppliers = new(Action, bool)[1] { (RevertWrapper, false) };

		[SpecialName]
		internal static Harmony defaultHarmony()
		{
			return GetHarmony("com.dreadscripts.controllereditor.tool");
		}

		[CallbackMethod(0)]
		internal static void ApplyPatches()
		{
			for (int i = 0; i < patchAppliers.Length; i++)
			{
				(Action, bool) tuple = patchAppliers[i];
				var (item, _) = tuple;
				if (!tuple.Item2)
				{
					patchAppliers[i] = (item, true);
					patchAppliers[i].Item1();
				}
			}
		}

		[ControllerCallback(0)]
		internal static void RemoveAllPatches()
		{
			if (harmonyInstances != null)
			{
				foreach (KeyValuePair<string, Harmony> harmonyInstance in harmonyInstances)
				{
					harmonyInstance.Value.UnpatchAll(harmonyInstance.Key);
				}
				harmonyInstances.Clear();
			}
			for (int i = 0; i < patchAppliers.Length; i++)
			{
				(Action, bool) tuple = patchAppliers[i];
				var (item, _) = tuple;
				if (tuple.Item2)
				{
					patchAppliers[i] = (item, false);
				}
			}
		}

		internal static void TestReg(string value, string token, MethodInfo comp = null, MethodInfo instance2 = null, MethodInfo res3 = null, string init4 = "")
		{
			Type type = EditorUtils.FindType(value);
			if (!(type == null))
			{
				MapReg(type, token, comp, instance2, res3);
			}
			else
			{
				Log("Couldn't find patch target type:\n" + value, CustomLogType.Error);
			}
		}

		internal static void MapReg(Type init, string selection, MethodInfo util = null, MethodInfo second2 = null, MethodInfo token3 = null, string task4 = "")
		{
			RateReg(AccessTools.GetDeclaredMethods(init).First((MethodInfo m) => m.Name == selection), util, second2, token3);
		}

		internal static void PatchByParameterType(Type first, Type caller, string pool, MethodInfo col2 = null, MethodInfo task3 = null, MethodInfo t4 = null, string def5 = "")
		{
			RateReg(AccessTools.GetDeclaredMethods(first).First((MethodInfo m) => m.Name == pool && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == caller)), col2, task3, t4);
		}

		internal static void PatchBySignature(Type spec, Type[] cfg, string temp, MethodInfo first2 = null, MethodInfo def3 = null, MethodInfo cust4 = null, string def5 = "")
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
				GetHarmony(second3).Patch(i, prefix, postfix, transpiler);
			}
			catch (Exception ex)
			{
				patchingFailed = true;
				patchErrorLog = patchErrorLog + ex.Message + "\n";
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
				GetHarmony(pol3).Patch(key, prefix, postfix, transpiler);
			}
			catch (Exception ex)
			{
				patchingFailed = true;
				patchErrorLog = patchErrorLog + ex.Message + "\n";
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
			PatchSwapEntry value = new PatchSwapEntry(cust, c, cfg2, res3, ivk4, ident5);
			deferredPatches[reference] = value;
			try
			{
				defaultHarmony().Patch(cust, null, new HarmonyMethod(c));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		internal static void ApplyDeferredPatch(string config)
		{
			PatchSwapEntry patchSwapEntry = deferredPatches[config];
			defaultHarmony().Unpatch(patchSwapEntry.triggerMethod, patchSwapEntry.triggerPatch);
			RateReg(patchSwapEntry.targetMethod, patchSwapEntry.prefix, patchSwapEntry.postfix, patchSwapEntry.transpiler);
		}

		internal static void LoginReg()
		{
			if (!patchingFailed || _ServiceAlgo)
			{
				return;
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (!hasRetried && !_StructAlgo)
				{
					_StructAlgo = true;
					Task.Run(async delegate
					{
						await Task.Delay(4000);
						EditorUtils.DelayCall(delegate
						{
							try
							{
								RetryPatching();
							}
							catch (Exception exception)
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
				GUILayout.Label(new GUIContent(EditorUtils.contents().invalidPattern)
				{
					tooltip = "This may happen if there were special characters in the project's path.\n\nSimple error log:\n" + patchErrorLog
				}, EditorUtils.styles().iconButton, GUILayout.Width(18f));
				GUILayout.Label("Patching not fully successful. Some functions/patches may be missing.", GUILayout.ExpandWidth(expand: false));
				if (_StructAlgo)
				{
					GUILayout.Label("Retrying...", GUILayout.ExpandWidth(expand: false));
				}
				GUILayout.FlexibleSpace();
				if (hasRetried)
				{
					if (EditorUtils.Button("Hide", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
					{
						_ServiceAlgo = true;
					}
					if (EditorUtils.Button("Retry", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
					{
						RetryPatching();
					}
				}
			}
		}

		private static void RetryPatching()
		{
			hasRetried = true;
			RemoveAllPatches();
			patchingFailed = false;
			m_BroadcasterAlgo = false;
			RevertWrapper();
		}

		private static Harmony GetHarmony(string v)
		{
			if (string.IsNullOrWhiteSpace(v))
			{
				return defaultHarmony();
			}
			if (harmonyInstances == null)
			{
				harmonyInstances = new Dictionary<string, Harmony>();
			}
			if (!harmonyInstances.TryGetValue(v, out var value))
			{
				value = new Harmony(v);
				harmonyInstances.Add(v, value);
			}
			return value;
		}

		internal static MethodInfo MethodOf(Action spec)
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

		internal static MethodInfo AssetReg<T>(RefAction<T> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo UpdateReg<T, TT>(RefAction<T, TT> item)
		{
			return item.Method;
		}

		internal static MethodInfo ChangeReg<T, TT, T3>(RefAction<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo SortReg<T, TT, T3, G>(RefAction<T, TT, T3, G> task)
		{
			return task.Method;
		}

		internal static MethodInfo RegisterReg<T, TT, T3, G, GG>(RefAction<T, TT, T3, G, GG> instance)
		{
			return instance.Method;
		}

		internal static MethodInfo LogoutReg<T, TT, T3, G, GG, A>(RefAction<T, TT, T3, G, GG, A> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo PatchReg<T, TT, T3, G, GG, A, AA>(RefFunc<T, TT, T3, G, GG, A, AA> first)
		{
			return first.Method;
		}

		internal static MethodInfo InterruptReg<T, TT, T3, G, GG, A>(RefFunc<T, TT, T3, G, GG, A> reference)
		{
			return reference.Method;
		}

		internal static MethodInfo ManageReg<T, TT, T3, G, GG>(RefFunc<T, TT, T3, G, GG> init)
		{
			return init.Method;
		}

		internal static MethodInfo PrintReg<T, TT, T3, G>(RefFunc<T, TT, T3, G> init)
		{
			return init.Method;
		}

		internal static MethodInfo SearchReg<T, TT, T3>(RefFunc<T, TT, T3> instance)
		{
			return instance.Method;
		}

		internal static MethodInfo RevertReg<T, TT>(RefFunc<T, TT> key)
		{
			return key.Method;
		}

		internal static MethodInfo OrderTests<T>(OutAction<T> task)
		{
			return task.Method;
		}

		internal static MethodInfo CompareTests<T, TT>(OutAction<T, TT> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo SetTests<T, TT, T3>(OutAction<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo PostTests<T, TT, T3, G>(OutAction<T, TT, T3, G> config)
		{
			return config.Method;
		}

		internal static MethodInfo SetupTests<T, TT, T3, G, GG>(OutAction<T, TT, T3, G, GG> var1)
		{
			return var1.Method;
		}

		internal static MethodInfo EnableTests<T, TT, T3, G, GG, A>(OutAction<T, TT, T3, G, GG, A> res)
		{
			return res.Method;
		}

		internal static MethodInfo PublishTests<T, TT, T3, G, GG, A, AA>(OutFunc<T, TT, T3, G, GG, A, AA> last)
		{
			return last.Method;
		}

		internal static MethodInfo PopTests<T, TT, T3, G, GG, A>(OutFunc<T, TT, T3, G, GG, A> i)
		{
			return i.Method;
		}

		internal static MethodInfo ComputeTests<T, TT, T3, G, GG>(OutFunc<T, TT, T3, G, GG> spec)
		{
			return spec.Method;
		}

		internal static MethodInfo MoveTests<T, TT, T3, G>(OutFunc<T, TT, T3, G> item)
		{
			return item.Method;
		}

		internal static MethodInfo ConcatTests<T, TT, T3>(OutFunc<T, TT, T3> v)
		{
			return v.Method;
		}

		internal static MethodInfo CallTests<T, TT>(OutFunc<T, TT> key)
		{
			return key.Method;
		}

		internal static MethodInfo CancelTests<T, TT, T3, G>(ValValRefRefAction<T, TT, T3, G> res)
		{
			return res.Method;
		}

		internal static MethodInfo CountTests<T, TT>(RefValAction<T, TT> item)
		{
			return item.Method;
		}

		internal static MethodInfo DisableTests<T, TT>(ValRefAction<T, TT> ident)
		{
			return ident.Method;
		}

		internal static MethodInfo InsertTests<T, TT>(ValOutAction<T, TT> asset)
		{
			return asset.Method;
		}

		internal static MethodInfo RestartTests<T, TT, T3>(ValValOutAction<T, TT, T3> def)
		{
			return def.Method;
		}

		internal static MethodInfo QueryTests<T, TT, T3>(ValOutValAction<T, TT, T3> info)
		{
			return info.Method;
		}
	}

	private enum LayerViewViewType
	{
		DefaultView,
		CategoryByName,
		CategoryByTag
	}

	private class LayerPathNode
	{
		internal readonly string name;

		internal readonly int depth;

		internal readonly string fullPath;

		internal readonly List<LayerPathNode> children = new List<LayerPathNode>();

		internal readonly List<LayerIndexEntry> layers = new List<LayerIndexEntry>();

		internal LayerPathNode baseCategoryNode;

		[SpecialName]
		internal string CategoryPath()
		{
			return StripRootPrefix(fullPath);
		}

		internal LayerPathNode(string v, string result, int dirPtr = 0)
		{
			name = v;
			depth = dirPtr;
			fullPath = result;
		}

		internal LayerPathNode AddLayer(string key, UnityEditor.Animations.AnimatorControllerLayer selection, int positionc)
		{
			LayerIndexEntry res = new LayerIndexEntry(selection, positionc);
			AddEntry(res);
			string[] array = QueryMapper(key);
			string text = array[0];
			string text2 = string.Join(PushInitializer(), array, 1, array.Length - 1);
			LayerPathNode layerPathNode = FindNode(text);
			if (layerPathNode == null && !text2.IsNullOrWhiteSpace())
			{
				layerPathNode = new LayerPathNode(text, fullPath + PushInitializer() + text, depth + 1);
				children.Add(layerPathNode);
			}
			if (!text2.IsNullOrEmpty())
			{
				return layerPathNode.AddLayer(text2, selection, positionc);
			}
			layerPathNode?.AddEntry(res);
			if (layerPathNode != GetOrCreateBaseCategory())
			{
				GetOrCreateBaseCategory().AddEntry(res);
			}
			return layerPathNode;
		}

		internal void AddEntry(LayerIndexEntry res)
		{
			if (layers.All((LayerIndexEntry l) => l.layerIndex != res.layerIndex))
			{
				layers.Add(res);
			}
		}

		internal LayerPathNode FindClosest(string info)
		{
			string[] array = QueryMapper(info);
			LayerPathNode layerPathNode = this;
			string[] array2 = array;
			foreach (string def in array2)
			{
				LayerPathNode layerPathNode2 = layerPathNode.FindNode(def);
				if (layerPathNode2 == null)
				{
					break;
				}
				layerPathNode = layerPathNode2;
			}
			return layerPathNode;
		}

		internal LayerPathNode FindNode(string def)
		{
			string[] array = QueryMapper(def);
			string m_CreatorMapper = array[0];
			string text = ((array.Length > 1) ? string.Join(PushInitializer(), array, 1, array.Length - 1) : "");
			LayerPathNode layerPathNode = children.FirstOrDefault((LayerPathNode c) => c.name == m_CreatorMapper);
			if (layerPathNode == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return layerPathNode.FindNode(text);
			}
			return layerPathNode;
		}

		internal LayerPathNode GetOrCreateBaseCategory()
		{
			if (baseCategoryNode != null)
			{
				return baseCategoryNode;
			}
			baseCategoryNode = FindNode(ValidateInitializer());
			if (baseCategoryNode == null)
			{
				children.Add(baseCategoryNode = new LayerPathNode(ValidateInitializer(), fullPath + PushInitializer() + ValidateInitializer(), depth + 1));
			}
			return baseCategoryNode;
		}

		internal void WalkPath(string param, Action<LayerPathNode> selection, bool isres = true)
		{
			if (isres)
			{
				selection(this);
			}
			if (param.IsNullOrEmpty())
			{
				return;
			}
			string[] array = param.Split(new char[1] { '/' });
			LayerPathNode layerPathNode = this;
			string[] array2 = array;
			foreach (string def in array2)
			{
				layerPathNode = layerPathNode.FindNode(def);
				if (layerPathNode != null)
				{
					selection(layerPathNode);
					continue;
				}
				break;
			}
		}

		private static string StripRootPrefix(string item)
		{
			return Regex.Replace(item, "^Root" + Regex.Escape(PushInitializer()) + "?", "");
		}
	}

	private struct LayerIndexEntry
	{
		internal readonly UnityEditor.Animations.AnimatorControllerLayer layer;

		internal readonly int layerIndex;

		private static object NewProduct;

		internal LayerIndexEntry(UnityEditor.Animations.AnimatorControllerLayer value, int next_cfg)
		{
			layer = value;
			layerIndex = next_cfg;
		}

		public static implicit operator UnityEditor.Animations.AnimatorControllerLayer(LayerIndexEntry asset)
		{
			return asset.layer;
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

		internal static Animator targetAnimator;

		internal static bool alwaysUseTargetAnimator;

		private static int selectedTab;

		private static readonly string[] tabLabels = new string[2] { "Behaviours & Cosmetics", "Defaults" };

		private static int selectedDefaultsTab;

		private static readonly string[] defaultsTabLabels = new string[3] { "Transition", "State", "Other" };

		private static readonly string[] emptyDropdownOptions = Array.Empty<string>();

		private static SerializedObject stateObject;

		private static SerializedProperty stateName;

		private static SerializedProperty stateSpeed;

		private static SerializedProperty stateCycleOffset;

		private static SerializedProperty stateIkOnFeet;

		private static SerializedProperty stateWriteDefaults;

		private static SerializedProperty stateMirror;

		private static SerializedProperty stateSpeedParameterActive;

		private static SerializedProperty stateMirrorParameterActive;

		private static SerializedProperty stateCycleOffsetParameterActive;

		private static SerializedProperty stateTimeParameterActive;

		private static SerializedProperty stateMotion;

		private static SerializedProperty stateTag;

		private static SerializedProperty stateSpeedParameter;

		private static SerializedProperty stateMirrorParameter;

		private static SerializedProperty stateCycleOffsetParameter;

		private static SerializedProperty stateTimeParameter;

		private static SerializedObject transitionObject;

		private static SerializedProperty transitionSolo;

		private static SerializedProperty transitionMute;

		private static SerializedProperty transitionDuration;

		private static SerializedProperty transitionOffset;

		private static SerializedProperty transitionExitTime;

		private static SerializedProperty transitionHasExitTime;

		private static SerializedProperty transitionHasFixedDuration;

		private static SerializedProperty transitionInterruptionSource;

		private static SerializedProperty transitionOrderedInterruption;

		private static SerializedProperty transitionCanTransitionToSelf;

		private static bool _BaseMapper;

		private static Vector2 scrollPosition;

		private static bool animationWindowExpanded;

		private static bool animatorWindowExpanded;

		private static bool layersExpanded;

		private static bool parametersExpanded;

		private static bool typeIndicatorExpanded;

		private static bool nodesExpanded;

		private static bool transitionsExpanded;

		private static bool graphColorsExpanded;

		private static bool nodeColorsExpanded;

		private static bool defaultLayerOptionsExpanded;

		private static bool colorsExpanded;

		private static bool transitionColorsExpanded;

		[SpecialName]
		internal static bool PushTests()
		{
			return EditorGUIUtility.isProSkin;
		}

		[MenuItem("DreadTools/Controller Editor/Settings", false, 4950)]
		internal static void ShowWindow()
		{
			EditorWindow.GetWindow<ControllerEditorWindow>(utility: false, "Controller Editor Settings", focus: true);
		}

		private void OnGUI()
		{
			if (!OrderVisitor(this))
			{
				return;
			}
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			selectedTab = GUILayout.Toolbar(selectedTab, tabLabels, "toolbarbutton");
			int num = selectedTab;
			if (num != 0)
			{
				if (num == 1)
				{
					DrawDefaultsTab();
				}
			}
			else
			{
				DrawBehavioursAndCosmeticsTab();
			}
			EditorUtils.Separator();
			RevertAnnotation();
			DefineVisitor();
			EditorUtils.setterProcessor.PopHelper(this);
			EditorGUILayout.EndScrollView();
		}

		private static void DrawBehavioursAndCosmeticsTab()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				using (new GUILayout.HorizontalScope())
				{
					animationWindowExpanded = EditorGUILayout.Foldout(animationWindowExpanded, "Animation Window");
					GUILayout.FlexibleSpace();
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, EditorSettings.GetInstance().aw_active, Color.green, Color.grey))
					{
						EditorSettings.GetInstance().aw_active.SetValue(EditorUtils.ToggleButton(EditorSettings.GetInstance().aw_active, (!EditorSettings.GetInstance().aw_active) ? "Disabled" : "Enabled"));
					}
				}
				if (animationWindowExpanded)
				{
					using (new EditorGUI.DisabledScope(!EditorSettings.GetInstance().aw_active))
					{
						using (new IndentedLayoutScope())
						{
							using (new GUILayout.HorizontalScope())
							{
								EditorGUI.BeginChangeCheck();
								EditorSettings.GetInstance().aw_enableOverride.Draw(new GUIContent("Overriding", "Allows you to explicitly set the controller for selecting clips, and explicitly set the root to change what the paths are relative to."), null);
								if (EditorGUI.EndChangeCheck())
								{
									TestInitializer(null);
									overrideAnimationRoot = null;
									overrideAnimationRootActive = false;
								}
								EditorSettings.GetInstance().aw_enablePropertyEditing.Draw(new GUIContent("Edit Property", "Allows you to drag and drop objects to properties and to edit the properties of the curves with right-click context menu."), null);
							}
							using (new GUILayout.HorizontalScope())
							{
								EditorSettings.GetInstance().aw_enableGameObjectDND.Draw(new GUIContent("Drag & Drop", "Allows you to drag and drop GameObjects to the animation window to add them as a new curve."), null);
								EditorSettings.GetInstance().aw_autoSwitchClip.Draw(new GUIContent("Auto-Switch Clip", "Automatically switch the clip in the animation window when selecting a state."), null);
							}
							using (new GUILayout.HorizontalScope())
							{
								EditorSettings.GetInstance().aw_warnPropertyMerge.Draw(new GUIContent("Property Merge Log", "Warn in the console when merging properties through property modification."), null);
							}
						}
					}
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				using (new FoldoutScope(ref animatorWindowExpanded, "Animator Window"))
				{
					if (!animatorWindowExpanded)
					{
						return;
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new FoldoutScope(ref layersExpanded, "Layers"))
						{
							if (layersExpanded)
							{
								EditorSettings.GetInstance().categoryBaseName.Draw("Uncategorized Name".CreateResolver("Name of the category for layers without a category."), true, true, null);
								EditorSettings.GetInstance().categoryDelimiter.Draw("Category Delimiter".CreateResolver("The character used to separate categories in the layer view."), true, true, null);
								using (new GUILayout.HorizontalScope())
								{
									EditorSettings.GetInstance().displayCategoryView.Draw("Category View".CreateResolver("Displays options to view layers in categories."), null);
									EditorSettings.GetInstance().displayLayerCompactView.Draw("Compact View".CreateResolver("Displays a button to view layers in a compact manner."), null);
								}
								using (new GUILayout.HorizontalScope())
								{
									EditorSettings.GetInstance().displayLayerIndex.Draw(new GUIContent("Layer Index", "Shows a small number on the layer's GUI for the layer's index in the list of layers."), null);
									EditorSettings.GetInstance().autoFrameLayer.Draw(new GUIContent("Auto-Frame Layer", "Upon selecting a layer, automatically frame the statemachine. Behaviour is similar to pressing 'A' after clicking the graph."), null);
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new FoldoutScope(ref parametersExpanded, "Parameters"))
						{
							if (parametersExpanded)
							{
								using (new GUILayout.HorizontalScope())
								{
									typeIndicatorExpanded = EditorGUILayout.Foldout(typeIndicatorExpanded, "Type Indicator");
									GUILayout.FlexibleSpace();
									using (new GUIColorScope(GUIColorScope.ColoringType.BG, EditorSettings.GetInstance().displayParameterType, Color.green, Color.grey))
									{
										EditorSettings.GetInstance().displayParameterType.SetValue(EditorUtils.ToggleButton(EditorSettings.GetInstance().displayParameterType, (!EditorSettings.GetInstance().displayParameterType) ? "Disabled" : "Enabled"));
									}
								}
								using (new EditorGUI.DisabledScope(!EditorSettings.GetInstance().displayParameterType))
								{
									if (typeIndicatorExpanded)
									{
										using (new IndentedLayoutScope())
										{
											EditorSettings.GetInstance().capitalParameterIndicator.Draw(new GUIContent("Capital Letters", "Changes 'f' to 'F' and 'i' to 'I'"), null);
											EditorSettings.GetInstance().parameterLabelFontStyle.DrawEnumPopup<FontStyle>(new GUIContent("Font style", "The font style of the parameter indicators."), ismap: false, null, Array.Empty<GUILayoutOption>());
											EditorSettings.GetInstance().parameterLabelColor.Draw(new GUIContent("Font Color", "The color of the parameter indicators. Supports Alpha."), true);
										}
									}
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new FoldoutScope(ref transitionsExpanded, "Transitions"))
						{
							if (transitionsExpanded)
							{
								using (new GUILayout.HorizontalScope())
								{
									using (new GUILayout.VerticalScope())
									{
										EditorSettings.GetInstance().autoReverseModes.Draw(new GUIContent("Auto Reverse Mode", "Reverse Transitions should also reverse the condition modes"), null);
										EditorSettings.GetInstance().animateInboundEdges.Draw("Animate In Transitions".CreateResolver("Incoming transitions to selected states get animated."), null);
									}
									using (new GUILayout.VerticalScope())
									{
										EditorSettings.GetInstance().reverseModifiesValues.Draw(new GUIContent("Reverse Adjusts Values", "Reversing a condition will also modify its values appropriately. Hold CTRL to temporarily flip this setting while reversing"), null);
										EditorSettings.GetInstance().animateOutboundEdges.Draw("Animate Out Transitions".CreateResolver("Outgoing transitions from selected states get animated."), null);
									}
								}
								EditorSettings.GetInstance().arrowLerpRatio.RemoveDefinition("Arrow Location".CreateResolver("Where the arrow exists on transitions."), -1f, 1f, true);
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new FoldoutScope(ref nodesExpanded, "Nodes"))
						{
							if (nodesExpanded)
							{
								using (new GUILayout.HorizontalScope())
								{
									EditorSettings.GetInstance().switchDoubleClick.Draw(new GUIContent("Alternate Double Click", "Switch Double click's behaviour on states. Ctrl Double Click will do the other behaviour"), null);
									EditorSettings.GetInstance().stateCosmetics.DrawEnumPopup<EditorSettings.StateCosmeticOptions>("State Extras", hasreg: true, null, Array.Empty<GUILayoutOption>());
								}
							}
						}
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new FoldoutScope(ref colorsExpanded, "Colors"))
						{
							if (!colorsExpanded)
							{
								return;
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									transitionColorsExpanded = EditorGUILayout.Foldout(transitionColorsExpanded, "Transition Colors");
									GUILayout.FlexibleSpace();
									bool value;
									string map = ((!(value = EditorSettings.GetInstance().cosmeticTransitionsActive.GetValue())) ? "Disabled" : "Enabled");
									using (new GUIColorScope(GUIColorScope.ColoringType.BG, value, Color.green, Color.grey))
									{
										EditorSettings.GetInstance().cosmeticTransitionsActive.SetValue(EditorUtils.ToggleButton(value, map));
									}
								}
								using (new EditorGUI.DisabledScope(!EditorSettings.GetInstance().cosmeticTransitionsActive))
								{
									if (transitionColorsExpanded)
									{
										using (new IndentedLayoutScope())
										{
											EditorSettings.GetInstance().normalTransitionColor.Draw("Normal Transition", true);
											EditorSettings.GetInstance().entryTransitionColor.Draw("Entry Transition", true);
											EditorSettings.GetInstance().selectedTransitionColor.Draw("Selected Transition", true);
											EditorSettings.GetInstance().baseTransitionColor.Draw("Base Transition", true);
										}
									}
								}
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									graphColorsExpanded = EditorGUILayout.Foldout(graphColorsExpanded, "Graph Colors");
									GUILayout.FlexibleSpace();
									bool value2;
									string map2 = ((!(value2 = EditorSettings.GetInstance().cosmeticGraphActive.GetValue())) ? "Disabled" : "Enabled");
									using (new GUIColorScope(GUIColorScope.ColoringType.BG, value2, Color.green, Color.grey))
									{
										using (new EditorSettings.SettingsChangeScope(ApplyGraphBackground))
										{
											EditorSettings.GetInstance().cosmeticGraphActive.SetValue(EditorUtils.ToggleButton(value2, map2));
										}
									}
								}
								using (new EditorGUI.DisabledScope(!EditorSettings.GetInstance().cosmeticGraphActive))
								{
									if (graphColorsExpanded)
									{
										using (new IndentedLayoutScope())
										{
											using (new GUILayout.HorizontalScope())
											{
												if (!EditorSettings.GetInstance().graphBackgroundIsTexture)
												{
													EditorSettings.GetInstance().gridBackgroundColor.Draw("Background", false);
												}
												else
												{
													EditorSettings.GetInstance().graphBackgroundTexture.Draw("Background", false, GUILayout.Height(17f), GUILayout.ExpandWidth(expand: true));
												}
												EditorSettings.GetInstance().graphBackgroundIsTexture.SetValue(EditorUtils.ToggleButton(EditorSettings.GetInstance().graphBackgroundIsTexture, new GUIContent("T", "Use Texture"), GUI.skin.button, GUILayout.Width(18f), GUILayout.Height(18f)));
												if (EditorUtils.IconButton(EditorUtils.contents().reset))
												{
													if (!EditorSettings.GetInstance().graphBackgroundIsTexture)
													{
														EditorSettings.GetInstance().gridBackgroundColor.Reset();
													}
													else
													{
														EditorSettings.GetInstance().graphBackgroundTexture.Reset();
													}
												}
											}
											if (EditorGUIUtility.isProSkin)
											{
												EditorSettings.GetInstance().gridMinorDarkColor.Draw("Minor Line", true);
												EditorSettings.GetInstance().gridMajorDarkColor.Draw("Major Line", true);
											}
											else
											{
												EditorSettings.GetInstance().gridMinorLightColor.Draw("Minor Line", true);
												EditorSettings.GetInstance().gridMajorLightColor.Draw("Major Line", true);
											}
										}
									}
								}
							}
							using (new GUILayout.VerticalScope(GUI.skin.box))
							{
								using (new GUILayout.HorizontalScope())
								{
									nodeColorsExpanded = EditorGUILayout.Foldout(nodeColorsExpanded, "Node Colors");
									GUILayout.FlexibleSpace();
									bool value3;
									string map3 = ((value3 = EditorSettings.GetInstance().cosmeticNodesActive.GetValue()) ? "Enabled" : "Disabled");
									using (new GUIColorScope(GUIColorScope.ColoringType.BG, value3, Color.green, Color.grey))
									{
										EditorSettings.GetInstance().cosmeticNodesActive.SetValue(EditorUtils.ToggleButton(value3, map3));
									}
								}
								using (new EditorGUI.DisabledScope(!EditorSettings.GetInstance().cosmeticNodesActive))
								{
									if (nodeColorsExpanded)
									{
										using (new IndentedLayoutScope())
										{
											DrawNodeColorField(EditorSettings.GetInstance().normalStateNodeColor, "State Node");
											DrawNodeColorField(EditorSettings.GetInstance().machineStateNodeColor, "Machine Node");
											DrawNodeColorField(EditorSettings.GetInstance().defaultStateNodeColor, "Default Node");
											DrawNodeColorField(EditorSettings.GetInstance().anyStateNodeColor, "AnyState Node");
											DrawNodeColorField(EditorSettings.GetInstance().entryStateNodeColor, "Entry Node");
											DrawNodeColorField(EditorSettings.GetInstance().exitStateNodeColor, "Exit Node");
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

		private static void DrawDefaultsTab()
		{
			selectedDefaultsTab = GUILayout.Toolbar(selectedDefaultsTab, defaultsTabLabels, "toolbarbutton");
			EditorUtils.Separator();
			switch (selectedDefaultsTab)
			{
			case 2:
				DrawOtherDefaults();
				break;
			case 0:
				DrawTransitionDefaults();
				break;
			case 1:
				DrawStateDefaults();
				break;
			}
		}

		private static void DrawTransitionDefaults()
		{
			transitionObject.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (EditorUtils.Button(EditorUtils.contents().copy, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					if (copiedTransitionSettings == null)
					{
						copiedTransitionSettings = new AnimatorStateTransition();
					}
					CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, copiedTransitionSettings);
				}
				using (new EditorGUI.DisabledScope(!copiedTransitionSettings))
				{
					if (EditorUtils.Button(EditorUtils.contents().paste, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
					{
						Undo.RecordObject(EditorSettings.GetInstance().defaultTransition, "PasteSettings");
						CustomizeAlgo(copiedTransitionSettings, EditorSettings.GetInstance().defaultTransition);
					}
				}
				if (EditorUtils.Button(EditorUtils.contents().restoreDefaults, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)) && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
				{
					EditorSettings.GetInstance().defaultTransition = new AnimatorStateTransition();
					RebuildTransitionSerializedObject();
					EditorSettings.SaveSettings();
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(transitionHasExitTime);
				using (new EditorGUI.DisabledScope(!transitionHasExitTime.boolValue))
				{
					EditorGUILayout.PropertyField(transitionExitTime);
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(transitionHasFixedDuration);
				EditorGUILayout.PropertyField(transitionDuration);
			}
			EditorGUILayout.PropertyField(transitionOffset);
			EditorGUILayout.PropertyField(transitionInterruptionSource);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(transitionOrderedInterruption);
				EditorGUILayout.PropertyField(transitionMute);
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(transitionCanTransitionToSelf);
				EditorGUILayout.PropertyField(transitionSolo);
			}
			bool hasModifiedProperties = transitionObject.hasModifiedProperties;
			transitionObject.ApplyModifiedProperties();
			if (hasModifiedProperties)
			{
				EditorSettings.SaveSettings();
			}
		}

		private static void DrawStateDefaults()
		{
			stateObject.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(EditorUtils.contents().animatorStates, GUILayout.Width(35f), GUILayout.Height(35f));
				using (new GUILayout.VerticalScope())
				{
					EditorGUILayout.PropertyField(stateName, new GUIContent(string.Empty));
					using (new GUILayout.HorizontalScope())
					{
						EditorGUIUtility.labelWidth = 35f;
						EditorGUILayout.PropertyField(stateTag);
						EditorGUIUtility.labelWidth = 0f;
						if (EditorUtils.Button(EditorUtils.contents().restoreDefaults, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)) && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
						{
							EditorSettings.GetInstance().defaultState = new AnimatorState
							{
								name = "New State"
							};
							RebuildStateSerializedObject();
							EditorSettings.SaveSettings();
						}
					}
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(stateMotion);
			EditorGUILayout.PropertyField(stateSpeed);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUI.indentLevel++;
				using (new EditorGUI.DisabledScope(!stateSpeedParameterActive.boolValue))
				{
					stateSpeedParameter.stringValue = EditorGUILayout.TextField("Multiplier", stateSpeedParameter.stringValue, "textfielddropdowntext");
				}
				EditorGUI.indentLevel--;
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
				}
				stateSpeedParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateSpeedParameterActive.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (stateTimeParameterActive.boolValue)
				{
					stateTimeParameter.stringValue = EditorGUILayout.TextField("Normalized Time", stateTimeParameter.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				else
				{
					GUILayout.Label("Normalized Time");
				}
				stateTimeParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateTimeParameterActive.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (!stateMirrorParameterActive.boolValue)
				{
					EditorGUILayout.PropertyField(stateMirror);
				}
				else
				{
					stateMirrorParameter.stringValue = EditorGUILayout.TextField("Mirror", stateMirrorParameter.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				stateMirrorParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateMirrorParameterActive.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				if (!stateCycleOffsetParameterActive.boolValue)
				{
					stateCycleOffset.floatValue = EditorGUILayout.Slider("Cycle Offset", stateCycleOffset.floatValue, 0f, 1f);
				}
				else
				{
					stateCycleOffsetParameter.stringValue = EditorGUILayout.TextField("Cycle Offset", stateCycleOffsetParameter.stringValue, "textfielddropdowntext");
					using (new EditorGUI.DisabledScope(disabled: true))
					{
						EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
					}
				}
				stateCycleOffsetParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateCycleOffsetParameterActive.boolValue, GUILayout.Width(90f));
			}
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(stateWriteDefaults, new GUIContent("Write Defaults"));
				EditorGUILayout.PropertyField(stateIkOnFeet, new GUIContent("Foot IK"));
			}
			bool hasModifiedProperties = stateObject.hasModifiedProperties;
			stateObject.ApplyModifiedProperties();
			if (hasModifiedProperties)
			{
				EditorSettings.SaveSettings();
			}
		}

		private static void DrawOtherDefaults()
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				targetAnimator = targetAnimator.ObjectField(new GUIContent("Targeted Animator", "The Animator that should be targeted by default when building Masks"), true);
				alwaysUseTargetAnimator = EditorUtils.ToggleButton(alwaysUseTargetAnimator, new GUIContent("Always Use"), GUILayout.Width(85f));
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				defaultLayerOptionsExpanded = EditorGUILayout.Foldout(defaultLayerOptionsExpanded, "Default Layer Options");
				if (defaultLayerOptionsExpanded)
				{
					using (new IndentedLayoutScope())
					{
						EditorSettings.GetInstance().defaultLayerWeight.SetValue(EditorGUILayout.Slider("Default Layer Weight", EditorSettings.GetInstance().defaultLayerWeight, 0f, 1f));
						EditorSettings.GetInstance().defaultLayerMask.Draw("Default Layer Mask", false);
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							EditorSettings.GetInstance().defaultEntryPosition.CloneDefinition("Entry Position");
						}
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							EditorSettings.GetInstance().defaultAnyPosition.CloneDefinition("AnyState Position");
						}
						using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
						{
							EditorSettings.GetInstance().defaultExitPosition.CloneDefinition("Exit Position");
						}
						using (new EditorGUI.DisabledScope(!ActiveStateMachine()))
						{
							if (EditorUtils.Button("Sample From Active StateMachine"))
							{
								EditorSettings.GetInstance().defaultEntryPosition.CreateDefinition(ActiveStateMachine().entryPosition);
								EditorSettings.GetInstance().defaultAnyPosition.CreateDefinition(ActiveStateMachine().anyStatePosition);
								EditorSettings.GetInstance().defaultExitPosition.CreateDefinition(ActiveStateMachine().exitPosition);
							}
						}
					}
				}
			}
			string value = EditorUtils.FolderField(EditorSettings.GetInstance().saveFolder, "Generated Assets Path");
			if (!string.IsNullOrEmpty(value))
			{
				EditorSettings.GetInstance().saveFolder.SetValue(value);
			}
		}

		private void OnEnable()
		{
			if (!_BaseMapper)
			{
				while (true)
				{
					RebuildTransitionSerializedObject();
					RebuildStateSerializedObject();
				}
			}
		}

		internal static void RebuildTransitionSerializedObject()
		{
			if (EditorSettings.GetInstance().defaultTransition == null)
			{
				EditorSettings.GetInstance().defaultTransition = new AnimatorStateTransition();
			}
			else
			{
				transitionObject = new SerializedObject(EditorSettings.GetInstance().defaultTransition);
				transitionSolo = transitionObject.FindProperty("m_Solo");
				transitionMute = transitionObject.FindProperty("m_Mute");
				transitionDuration = transitionObject.FindProperty("m_TransitionDuration");
				transitionOffset = transitionObject.FindProperty("m_TransitionOffset");
			}
			transitionExitTime = transitionObject.FindProperty("m_ExitTime");
			transitionHasExitTime = transitionObject.FindProperty("m_HasExitTime");
			transitionHasFixedDuration = transitionObject.FindProperty("m_HasFixedDuration");
			transitionInterruptionSource = transitionObject.FindProperty("m_InterruptionSource");
			transitionOrderedInterruption = transitionObject.FindProperty("m_OrderedInterruption");
			transitionCanTransitionToSelf = transitionObject.FindProperty("m_CanTransitionToSelf");
		}

		internal static void RebuildStateSerializedObject()
		{
			if (EditorSettings.GetInstance().defaultState == null)
			{
				EditorSettings.GetInstance().defaultState = new AnimatorState
				{
					name = "New State"
				};
			}
			stateObject = new SerializedObject(EditorSettings.GetInstance().defaultState);
			stateName = stateObject.FindProperty("m_Name");
			if (stateName != null && (bool)EditorSettings.GetInstance().requiresStateRename)
			{
				stateName.stringValue = "New State";
				EditorSettings.GetInstance().requiresStateRename.SetValue(excludeparam: false);
				stateObject.ApplyModifiedPropertiesWithoutUndo();
			}
			stateSpeed = stateObject.FindProperty("m_Speed");
			stateCycleOffset = stateObject.FindProperty("m_CycleOffset");
			stateIkOnFeet = stateObject.FindProperty("m_IKOnFeet");
			stateWriteDefaults = stateObject.FindProperty("m_WriteDefaultValues");
			stateMirror = stateObject.FindProperty("m_Mirror");
			stateSpeedParameterActive = stateObject.FindProperty("m_SpeedParameterActive");
			stateMirrorParameterActive = stateObject.FindProperty("m_MirrorParameterActive");
			stateCycleOffsetParameterActive = stateObject.FindProperty("m_CycleOffsetParameterActive");
			stateTimeParameterActive = stateObject.FindProperty("m_TimeParameterActive");
			stateMotion = stateObject.FindProperty("m_Motion");
			stateTag = stateObject.FindProperty("m_Tag");
			stateSpeedParameter = stateObject.FindProperty("m_SpeedParameter");
			stateMirrorParameter = stateObject.FindProperty("m_MirrorParameter");
			stateCycleOffsetParameter = stateObject.FindProperty("m_CycleOffsetParameter");
			stateTimeParameter = stateObject.FindProperty("m_TimeParameter");
		}

		[CompilerGenerated]
		internal static void DrawNodeColorField(EditorSettings.FloatSetting config, string attr)
		{
			using (new GUILayout.HorizontalScope())
			{
				config.SetValue((float)(NodeColor)(object)EditorGUILayout.EnumPopup(attr, (NodeColor)config.GetValue()));
				if (EditorUtils.Button(EditorUtils.contents().reset, EditorUtils.styles().tightLabel, GUILayout.Width(18f), GUILayout.Height(18f)))
				{
					config.Reset();
				}
			}
		}
	}

	internal class MotionRenamerWindow : EditorWindow
	{
		public List<Motion> motions = new List<Motion>();

		public string newName = "";

		private bool focusPending = true;

		public void OnGUI()
		{
			bool flag = false;
			string text = "Rename Field";
			GUI.SetNextControlName(text);
			newName = EditorGUILayout.TextField(newName);
			if (focusPending)
			{
				focusPending = false;
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
			UnityEngine.Object[] array = (objectsToUndo = motions.Where((Motion m) => m != null).Distinct().ToArray());
			Undo.RecordObjects(objectsToUndo, "Rename motion");
			StringBuilder stringBuilder = new StringBuilder();
			Motion[] array2 = (Motion[])array;
			foreach (Motion motion in array2)
			{
				if (MotionEmbedMenu.IsEmbedded(motion))
				{
					motion.name = newName;
				}
				else if (motion.name != newName)
				{
					string text2 = motion.name;
					string assetPath = AssetDatabase.GetAssetPath(motion);
					string text3 = MotionEmbedMenu.GenerateUniqueName(assetPath, newName);
					if (newName != text3)
					{
						stringBuilder.AppendLine(text2 + " -> " + text3);
					}
					AssetDatabase.RenameAsset(assetPath, text3);
				}
				EditorUtility.SetDirty(motion);
				MotionEmbedMenu.MarkScenesDirty();
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
	}

	internal class ParameterRenameWindow : DreadScripts.ControllerEditor.UtilityWindowBase<ParameterRenameWindow>
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass11_1
		{
			public string m_ProcMapper;

			internal bool PrepareTests(UnityEngine.AnimatorControllerParameter p2)
			{
				return p2.name != m_ProcMapper;
			}
		}

		private static readonly string[] m_BroadcasterMapper = new string[3] { "No Change", "Force On", "Force Off" };

		internal UnityEditor.Animations.AnimatorController m_ProxyMapper;

		internal UnityEditor.Animations.AnimatorController _StructMapper;

		internal bool serviceMapper = true;

		private int stateMapper;

		internal (UnityEngine.AnimatorControllerParameter, string)[] globalMapper;

		string DreadScripts.ControllerEditor.UtilityWindowBase<ParameterRenameWindow>.title => "Parameter Rename";

		internal static ParameterRenameWindow ResolveTests(UnityEditor.Animations.AnimatorController def, UnityEditor.Animations.AnimatorController vis, bool acceptutil)
		{
			ParameterRenameWindow parameterRenameWindow = DreadScripts.ControllerEditor.UtilityWindowBase<ParameterRenameWindow>.Create();
			parameterRenameWindow.m_ProxyMapper = def;
			parameterRenameWindow._StructMapper = vis;
			parameterRenameWindow.serviceMapper = acceptutil;
			UnityEngine.AnimatorControllerParameter[] array = def.parameters.Where((UnityEngine.AnimatorControllerParameter p) => !EditorUtils.reservedAvatarParameters.Contains(p.name)).ToArray();
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

		void DreadScripts.ControllerEditor.UtilityWindowBase<ParameterRenameWindow>.OnCustomGUI()
		{
			if (globalMapper == null)
			{
				Close();
				return;
			}
			canConfirm = true;
			EditorGUI.BeginChangeCheck();
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, serviceMapper, Color.green, Color.grey))
			{
				serviceMapper = EditorUtils.ToggleButton(serviceMapper, "Unique Parameters", GUI.skin.button);
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
						canConfirm = false;
						GUILayout.Label(new GUIContent(EditorUtils.contents().warning.texture(), "Parameter must not be empty"), EditorUtils.styles().centeredIcon, GUILayout.ExpandWidth(expand: false));
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
					s.MarkDirty();
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
			ShowAt(init, FillTests());
		}

		internal Vector2 FillTests()
		{
			return new Vector2(350f, 60f + (float)globalMapper.Length * (EditorGUIUtility.singleLineHeight + 7f));
		}
	}

	internal class QuickToggleWindow : DreadScripts.ControllerEditor.UtilityWindowBase<QuickToggleWindow>
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec observerInitializer = new _003C_003Ec();

			public static Func<AnimatorState, bool> serverInitializer;

			public static Func<GameObject, ComponentQueue> m_ThreadInitializer;

			public static Action<DreadScripts.ControllerEditor.SearchablePickerPopup<string>.PickerEntry> policyInitializer;

			public static Func<string, string, bool> m_SerializerInitializer;

			public static Func<Component, Type> m_PageInitializer;

			public static Action<DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry> _ResolverInitializer;

			public static Func<Type, object[]> predicateInitializer;

			public static Func<GameObject, ComponentQueue> rulesInitializer;

			public static Func<AnimatorState, bool> queueInitializer;

			public static Func<AnimatorState, bool> m_ErrorInitializer;

			public static Func<bool, bool> setterInitializer;

			public static Func<bool, bool> _ConnectionInitializer;

			internal bool SearchTests(AnimatorState s)
			{
				return s.motion as AnimationClip;
			}

			internal ComponentQueue RevertTests(GameObject o)
			{
				return new ComponentQueue(o);
			}

			internal void OrderProperty(DreadScripts.ControllerEditor.SearchablePickerPopup<string>.PickerEntry i)
			{
				GUILayout.Label(i.value);
			}

			internal bool CompareProperty(string p, string s)
			{
				return p.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
			}

			internal Type SetProperty(Component c)
			{
				return c.GetType();
			}

			internal void PostProperty(DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry item)
			{
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label((GUIContent)item.FirstExtra(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
				}
			}

			internal object[] SetupProperty(Type type)
			{
				return new object[1]
				{
					new GUIContent(image: EditorGUIUtility.ObjectContent(null, type).image ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image, text: type.Name, tooltip: type.AssemblyQualifiedName)
				};
			}

			internal ComponentQueue EnableProperty(GameObject o)
			{
				return new ComponentQueue(o);
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
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_0
		{
			public QuickToggleWindow m_ContextInitializer;

			internal void ConcatProperty()
			{
				m_ContextInitializer.targetList.DrawTitle("Target GameObjects", "The GameObjects that will be animated by the animation clip");
				GUILayout.FlexibleSpace();
				if (m_ContextInitializer.hasExistingClips && EditorUtils.Button(InterruptTests() ? EditorUtils.contents().defaultMergeClip : EditorUtils.contents().defaultReplaceClip, EditorUtils.styles().iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					ManageTests(!InterruptTests());
				}
				if (EditorUtils.Button((!RegisterTests()) ? EditorUtils.contents().simpleMode : EditorUtils.contents().advancedMode, EditorUtils.styles().iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					LogoutTests(!RegisterTests());
				}
				m_ContextInitializer.targetList.DrawHeaderButtons(rejectv: false, writeattr: false);
			}

			internal void CallProperty(ReorderableList r)
			{
				m_ContextInitializer.targets.Add(new ComponentQueue());
			}

			internal void CancelProperty(Rect rect, int index, bool active, bool focused)
			{
				_003C_003Ec__DisplayClass18_1 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_1();
				_003C_003Ec__DisplayClass18_.m_ConsumerInitializer = this;
				_003C_003Ec__DisplayClass18_._HelperInitializer = index;
				_003C_003Ec__DisplayClass18_.m_RecordInitializer = m_ContextInitializer.targets[_003C_003Ec__DisplayClass18_._HelperInitializer];
				Rect rect2 = rect.SliceLeft((!RegisterTests()) ? 80 : 45, isfield: false, -1f, iscont3: false, isattr4: false);
				Rect rect3 = new Rect(rect2)
				{
					width = 20f,
					x = rect2.x + rect2.width - 20f
				};
				if (!RegisterTests())
				{
					Rect position = rect.SliceLeft(20f, isfield: false, 80f);
					string text = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn()) ? "Off" : "On");
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, _003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn(), Color.green, Color.red))
					{
						if (GUI.Button(position, text))
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.value = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn()) ? 1 : 0);
						}
					}
				}
				else
				{
					Rect rect4 = rect.SliceLeft(40f, isfield: false, 45f, iscont3: false, isattr4: false);
					Rect rect5 = rect.SliceLeft(15f, isfield: false, 85f);
					rect4.height = 20f;
					bool flag = _003C_003Ec__DisplayClass18_.m_RecordInitializer.propertyNames.Length != 0;
					using (new EditorGUI.DisabledScope(!flag))
					{
						if (EditorGUI.DropdownButton(rect4, new GUIContent(_003C_003Ec__DisplayClass18_.m_RecordInitializer.PropertyName()), FocusType.Keyboard, EditorStyles.toolbarDropDown))
						{
							DreadScripts.ControllerEditor.SearchablePickerPopup<string> searchablePickerPopup = new DreadScripts.ControllerEditor.SearchablePickerPopup<string>("Property", _003C_003Ec__DisplayClass18_.m_RecordInitializer.propertyNames, _003C_003Ec.observerInitializer.OrderProperty, _003C_003Ec__DisplayClass18_.CountProperty);
							searchablePickerPopup.EnableSearch(_003C_003Ec.observerInitializer.CompareProperty);
							searchablePickerPopup.Show(rect4);
						}
					}
					_003C_003Ec__DisplayClass18_.m_RecordInitializer.value = EditorGUI.FloatField(rect5, _003C_003Ec__DisplayClass18_.m_RecordInitializer.value);
					EditorUtils.FlushQueue(rect4, "Property", 180f, 15f, stripresult3: false);
					EditorUtils.FlushQueue(rect5, "Value", 50f, 0f, stripresult3: false);
					if (!flag)
					{
						EditorUtils.FlushQueue(rect4, "No Valid Properties", 145f);
					}
				}
				using (new EditorGUI.DisabledScope(!_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject()))
				{
					if (GUI.Button(rect3, GUIContent.none, GUIStyle.none))
					{
						if (Event.current.button == 0)
						{
							DreadScripts.ControllerEditor.SearchablePickerPopup<Type> searchablePickerPopup2 = new DreadScripts.ControllerEditor.SearchablePickerPopup<Type>("Target Type", new Type[1] { typeof(GameObject) }.Concat(_003C_003Ec__DisplayClass18_.m_RecordInitializer.components.Select(_003C_003Ec.observerInitializer.SetProperty)).Distinct().ToList(), _003C_003Ec.observerInitializer.PostProperty, _003C_003Ec__DisplayClass18_.DisableProperty);
							searchablePickerPopup2.SetExtraData(_003C_003Ec.observerInitializer.SetupProperty);
							if (!EditorSettings.GetInstance().advancedQuickToggle)
							{
								DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry[] entries = searchablePickerPopup2.entries;
								for (int i = 0; i < entries.Length; i++)
								{
									_003C_003Ec__DisplayClass18_2 _003C_003Ec__DisplayClass18_2 = new _003C_003Ec__DisplayClass18_2
									{
										interpreterInitializer = entries[i]
									};
									if (!ComponentQueue.toggleableTypes.Any(_003C_003Ec__DisplayClass18_2.AddProperty))
									{
										_003C_003Ec__DisplayClass18_2.interpreterInitializer.isVisible = false;
									}
								}
							}
							searchablePickerPopup2.Show(rect3);
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.Next(!EditorSettings.GetInstance().advancedQuickToggle);
						}
						Event.current.Use();
					}
				}
				UnityEngine.Object target = _003C_003Ec__DisplayClass18_.m_RecordInitializer.target;
				EditorGUI.BeginChangeCheck();
				target = EditorGUI.ObjectField(rect2, target, typeof(GameObject), allowSceneObjects: true);
				if (EditorGUI.EndChangeCheck())
				{
					if ((bool)target)
					{
						if (!(target is GameObject def))
						{
							if (target is Component component)
							{
								_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(component.gameObject);
							}
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(def);
						}
					}
					else
					{
						_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(null);
					}
				}
				if ((bool)_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject())
				{
					EditorGUI.DropdownButton(rect3, GUIContent.none, FocusType.Passive, EditorUtils.styles().dropDownButton);
				}
				EditorUtils.FlushQueue(rect2, "Target", 200f, 20f, stripresult3: false);
				EditorUtils.HandleMultiDragAndDrop<GameObject>(rect2, _003C_003Ec__DisplayClass18_.InsertProperty);
				EditorUtils.HandleScrollWheel(rect2, _003C_003Ec__DisplayClass18_.QueryProperty);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_1
		{
			public ComponentQueue m_RecordInitializer;

			public int _HelperInitializer;

			public _003C_003Ec__DisplayClass18_0 m_ConsumerInitializer;

			public Func<GameObject, bool> adapterInitializer;

			internal void CountProperty(int i, string s)
			{
				m_RecordInitializer.propertyIndex = i;
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}

			internal void DisableProperty(int i, Type _)
			{
				m_RecordInitializer.ComponentIndex(i - 1);
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}

			internal void InsertProperty(IEnumerable<GameObject> ie)
			{
				m_ConsumerInitializer.m_ContextInitializer.targets.InsertRange(_HelperInitializer, ie.Where((GameObject o) => o != m_RecordInitializer.GameObject()).Select(_003C_003Ec.observerInitializer.EnableProperty));
			}

			internal bool RestartProperty(GameObject o)
			{
				return o != m_RecordInitializer.GameObject();
			}

			internal void QueryProperty(float y)
			{
				if (y <= 0f)
				{
					m_RecordInitializer.Next(!EditorSettings.GetInstance().advancedQuickToggle);
				}
				else
				{
					m_RecordInitializer.Previous(!EditorSettings.GetInstance().advancedQuickToggle);
				}
				m_ConsumerInitializer.m_ContextInitializer.Repaint();
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_2
		{
			public DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry interpreterInitializer;

			internal bool AddProperty(Type vt)
			{
				return interpreterInitializer.value.Is(vt);
			}
		}

		private Transform root;

		private List<ComponentQueue> targets;

		private List<AnimatorState> states;

		private DreadScripts.ControllerEditor.ReorderableListHelper<ComponentQueue> targetList;

		private static readonly Color[] mergeModeColors = new Color[3]
		{
			Color.green,
			Color.cyan,
			Color.yellow
		};

		private int mergeMode;

		private int existingClipCount;

		private bool[] mergePerState;

		private bool hasExistingClips;

		private bool existingClipsExpanded;

		private static readonly GUIContent[] labels = new GUIContent[4]
		{
			new GUIContent("Root", "Relative path root of the animation"),
			new GUIContent("Target", "Target GameObject or GameObject containing target Component"),
			new GUIContent("Component Index", "Which component to toggle. -1 is GameObject. 0 is Transform (Not toggleable)"),
			new GUIContent("Enabled", "What the toggled state is when animated")
		};

		string DreadScripts.ControllerEditor.UtilityWindowBase<QuickToggleWindow>.title => "CEditor QuickToggle";

		[SpecialName]
		private static bool RegisterTests()
		{
			return EditorSettings.GetInstance().advancedQuickToggle;
		}

		[SpecialName]
		private static void LogoutTests(bool loadinit)
		{
			EditorSettings.GetInstance().advancedQuickToggle.SetValue(loadinit);
		}

		[SpecialName]
		private static bool InterruptTests()
		{
			return EditorSettings.GetInstance().mergeQuickToggle;
		}

		[SpecialName]
		private static void ManageTests(bool appendasset)
		{
			EditorSettings.GetInstance().mergeQuickToggle.SetValue(appendasset);
		}

		internal static QuickToggleWindow AssetTests(List<AnimatorState> i, Transform second, List<GameObject> template)
		{
			QuickToggleWindow m_ContextInitializer = DreadScripts.ControllerEditor.UtilityWindowBase<QuickToggleWindow>.Create();
			m_ContextInitializer.states = i;
			m_ContextInitializer.mergePerState = new bool[i.Count];
			AnimatorState defaultState = EditorSettings.GetInstance().defaultState;
			Motion motion = ((!(defaultState != null)) ? null : defaultState.motion);
			if (InterruptTests())
			{
				for (int j = 0; j < i.Count; j++)
				{
					AnimatorState animatorState = m_ContextInitializer.states[j];
					m_ContextInitializer.mergePerState[j] = animatorState == null || animatorState.motion != motion;
				}
			}
			m_ContextInitializer.existingClipCount = i.Count((AnimatorState s) => s.motion as AnimationClip);
			m_ContextInitializer.hasExistingClips = m_ContextInitializer.existingClipCount > 0;
			m_ContextInitializer.root = second;
			m_ContextInitializer.targets = new List<ComponentQueue>(template.Select((GameObject o) => new ComponentQueue(o)));
			_003C_003Ec__DisplayClass18_0 CS_0024_003C_003E8__locals0;
			m_ContextInitializer.targetList = new DreadScripts.ControllerEditor.ReorderableListHelper<ComponentQueue>(delegate
			{
				m_ContextInitializer.targetList.DrawTitle("Target GameObjects", "The GameObjects that will be animated by the animation clip");
				GUILayout.FlexibleSpace();
				if (m_ContextInitializer.hasExistingClips && EditorUtils.Button(InterruptTests() ? EditorUtils.contents().defaultMergeClip : EditorUtils.contents().defaultReplaceClip, EditorUtils.styles().iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					ManageTests(!InterruptTests());
				}
				if (EditorUtils.Button((!RegisterTests()) ? EditorUtils.contents().simpleMode : EditorUtils.contents().advancedMode, EditorUtils.styles().iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					LogoutTests(!RegisterTests());
				}
				m_ContextInitializer.targetList.DrawHeaderButtons(rejectv: false, writeattr: false);
			}, m_ContextInitializer.targets, delegate
			{
				m_ContextInitializer.targets.Add(new ComponentQueue());
			}, delegate(Rect rect, int index, bool active, bool focused)
			{
				_003C_003Ec__DisplayClass18_1 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_1();
				_003C_003Ec__DisplayClass18_.m_ConsumerInitializer = CS_0024_003C_003E8__locals0;
				_003C_003Ec__DisplayClass18_._HelperInitializer = index;
				_003C_003Ec__DisplayClass18_.m_RecordInitializer = m_ContextInitializer.targets[_003C_003Ec__DisplayClass18_._HelperInitializer];
				Rect rect2 = rect.SliceLeft((!RegisterTests()) ? 80 : 45, isfield: false, -1f, iscont3: false, isattr4: false);
				Rect item = new Rect(rect2)
				{
					width = 20f,
					x = rect2.x + rect2.width - 20f
				};
				if (!RegisterTests())
				{
					Rect rect3 = rect.SliceLeft(20f, isfield: false, 80f);
					string text = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn()) ? "Off" : "On");
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, _003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn(), Color.green, Color.red))
					{
						if (GUI.Button(rect3, text))
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.value = ((!_003C_003Ec__DisplayClass18_.m_RecordInitializer.IsOn()) ? 1 : 0);
						}
					}
				}
				else
				{
					Rect rect4 = rect.SliceLeft(40f, isfield: false, 45f, iscont3: false, isattr4: false);
					Rect res = rect.SliceLeft(15f, isfield: false, 85f);
					rect4.height = 20f;
					bool flag = _003C_003Ec__DisplayClass18_.m_RecordInitializer.propertyNames.Length != 0;
					using (new EditorGUI.DisabledScope(!flag))
					{
						if (EditorGUI.DropdownButton(rect4, new GUIContent(_003C_003Ec__DisplayClass18_.m_RecordInitializer.PropertyName()), FocusType.Keyboard, EditorStyles.toolbarDropDown))
						{
							DreadScripts.ControllerEditor.SearchablePickerPopup<string> searchablePickerPopup = new DreadScripts.ControllerEditor.SearchablePickerPopup<string>("Property", _003C_003Ec__DisplayClass18_.m_RecordInitializer.propertyNames, _003C_003Ec.observerInitializer.OrderProperty, _003C_003Ec__DisplayClass18_.CountProperty);
							searchablePickerPopup.EnableSearch(_003C_003Ec.observerInitializer.CompareProperty);
							searchablePickerPopup.Show(rect4);
						}
					}
					_003C_003Ec__DisplayClass18_.m_RecordInitializer.value = EditorGUI.FloatField(res, _003C_003Ec__DisplayClass18_.m_RecordInitializer.value);
					EditorUtils.FlushQueue(rect4, "Property", 180f, 15f, stripresult3: false);
					EditorUtils.FlushQueue(res, "Value", 50f, 0f, stripresult3: false);
					if (!flag)
					{
						EditorUtils.FlushQueue(rect4, "No Valid Properties", 145f);
					}
				}
				using (new EditorGUI.DisabledScope(!_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject()))
				{
					if (GUI.Button(item, GUIContent.none, GUIStyle.none))
					{
						if (Event.current.button == 0)
						{
							DreadScripts.ControllerEditor.SearchablePickerPopup<Type> searchablePickerPopup2 = new DreadScripts.ControllerEditor.SearchablePickerPopup<Type>("Target Type", new Type[1] { typeof(GameObject) }.Concat(_003C_003Ec__DisplayClass18_.m_RecordInitializer.components.Select(_003C_003Ec.observerInitializer.SetProperty)).Distinct().ToList(), _003C_003Ec.observerInitializer.PostProperty, _003C_003Ec__DisplayClass18_.DisableProperty);
							searchablePickerPopup2.SetExtraData(_003C_003Ec.observerInitializer.SetupProperty);
							if (!EditorSettings.GetInstance().advancedQuickToggle)
							{
								DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry[] entries = searchablePickerPopup2.entries;
								for (int k = 0; k < entries.Length; k++)
								{
									_003C_003Ec__DisplayClass18_2 _003C_003Ec__DisplayClass18_2 = new _003C_003Ec__DisplayClass18_2();
									_003C_003Ec__DisplayClass18_2.interpreterInitializer = entries[k];
									if (!ComponentQueue.toggleableTypes.Any(_003C_003Ec__DisplayClass18_2.AddProperty))
									{
										_003C_003Ec__DisplayClass18_2.interpreterInitializer.isVisible = false;
									}
								}
							}
							searchablePickerPopup2.Show(item);
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.Next(!EditorSettings.GetInstance().advancedQuickToggle);
						}
						Event.current.Use();
					}
				}
				UnityEngine.Object target = _003C_003Ec__DisplayClass18_.m_RecordInitializer.target;
				EditorGUI.BeginChangeCheck();
				target = EditorGUI.ObjectField(rect2, target, typeof(GameObject), allowSceneObjects: true);
				if (EditorGUI.EndChangeCheck())
				{
					if ((bool)target)
					{
						if (!(target is GameObject def))
						{
							if (target is Component component)
							{
								_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(component.gameObject);
							}
						}
						else
						{
							_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(def);
						}
					}
					else
					{
						_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject(null);
					}
				}
				if ((bool)_003C_003Ec__DisplayClass18_.m_RecordInitializer.GameObject())
				{
					EditorGUI.DropdownButton(item, GUIContent.none, FocusType.Passive, EditorUtils.styles().dropDownButton);
				}
				EditorUtils.FlushQueue(rect2, "Target", 200f, 20f, stripresult3: false);
				EditorUtils.HandleMultiDragAndDrop<GameObject>(rect2, _003C_003Ec__DisplayClass18_.InsertProperty);
				EditorUtils.HandleScrollWheel(rect2, _003C_003Ec__DisplayClass18_.QueryProperty);
			});
			m_ContextInitializer.targetList.drawWhenEmpty = true;
			m_ContextInitializer.RefreshMergeMode();
			if (!i.All((AnimatorState s) => s.name.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				if (i.All((AnimatorState s) => s.name.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					foreach (ComponentQueue target2 in m_ContextInitializer.targets)
					{
						target2.value = 1f;
					}
				}
			}
			else
			{
				foreach (ComponentQueue target3 in m_ContextInitializer.targets)
				{
					target3.value = 0f;
				}
			}
			return m_ContextInitializer;
		}

		void DreadScripts.ControllerEditor.UtilityWindowBase<QuickToggleWindow>.OnCustomGUI()
		{
			if (targetList == null)
			{
				Close();
				return;
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				root = (Transform)EditorGUILayout.ObjectField(labels[0], root, typeof(Transform), true);
			}
			targetList.Draw();
			if (!hasExistingClips)
			{
				return;
			}
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				using (new GUILayout.HorizontalScope())
				{
					existingClipsExpanded = EditorGUILayout.Foldout(existingClipsExpanded, new GUIContent($"Existing Clips ({existingClipCount})"));
					GUILayout.FlexibleSpace();
					GUILayout.Label(new GUIContent(EditorUtils.contents().help.texture(), "Merge: Adds the properties to the existing clips on states. Creates a new clip if no clip exists.\n\nReplace: Replaces the existing clips on states with new clips and adds the properties to them."), GUILayout.Width(14f), GUILayout.Height(18f));
					string res = ((mergeMode == 0) ? "Merge" : ((mergeMode == 1) ? "Replace" : "Mixed"));
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergeMode, mergeModeColors[0], mergeModeColors[1], mergeModeColors[2]))
					{
						if (EditorUtils.Button(res))
						{
							switch (mergeMode)
							{
							case 0:
							{
								mergeMode = 1;
								for (int j = 0; j < mergePerState.Length; j++)
								{
									mergePerState[j] = false;
								}
								break;
							}
							case 1:
							case 2:
							{
								mergeMode = 0;
								for (int i = 0; i < mergePerState.Length; i++)
								{
									mergePerState[i] = true;
								}
								break;
							}
							}
						}
					}
				}
				if (!existingClipsExpanded)
				{
					return;
				}
				using (new IndentedLayoutScope())
				{
					for (int k = 0; k < states.Count; k++)
					{
						AnimatorState animatorState = states[k];
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
							string res2 = (mergePerState[k] ? "Merge" : "Replace");
							using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergePerState[k], mergeModeColors[0], mergeModeColors[1]))
							{
								if (EditorUtils.Button(res2))
								{
									mergePerState[k] = !mergePerState[k];
									RefreshMergeMode();
								}
							}
						}
					}
				}
			}
		}

		internal override void OnCustomConfirm()
		{
			if (RateAnnotation(!root, "No Root Set!"))
			{
				return;
			}
			List<AnimationClip> list = new List<AnimationClip>();
			int num = 0;
			while (true)
			{
				if (num < states.Count)
				{
					AnimatorState animatorState = states[num];
					Motion motion = animatorState.motion;
					if (mergePerState[num] || !RateAnnotation(motion is UnityEditor.Animations.BlendTree, "State " + animatorState.name + " has a Blendtree motion. Can't automatically merge."))
					{
						AnimationClip animationClip = motion as AnimationClip;
						if (!animationClip || !mergePerState[num])
						{
							Undo.RecordObject(animatorState, "Set Quick Toggle Curve");
							string i = $"{EditorSettings.GetInstance().saveFolder}/Animation Clips/{ActiveController().name}";
							animationClip = new AnimationClip();
							string path = EditorUtils.AwakeList(i, animatorState.name + ".anim", writestate: true);
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
					foreach (ComponentQueue target in targets)
					{
						if (target.IsValid())
						{
							animationClip2.SetCurve(AnimationUtility.CalculateTransformPath(target.GameObject().transform, root), target.targetType, target.PropertyName(), EditorUtils.InvokePredicate(AnimationUtility.TangentMode.Linear, (0f, target.value), (animationClip2.GetEffectiveLength(), target.value)));
						}
					}
				}
				break;
			}
		}

		internal Vector2 CalculateWindowSize()
		{
			return new Vector2(370f, 48 + 22 * Mathf.Max(1, targets.Count) + 28 + ((!string.IsNullOrEmpty(helpMessage)) ? 38 : 0) + (hasExistingClips ? 32 : 0));
		}

		internal void RefreshMergeMode()
		{
			mergeMode = ((!mergePerState.All((bool b) => b)) ? (mergePerState.All((bool b) => !b) ? 1 : 2) : 0);
		}

		internal void ShowAt(Vector2 instance)
		{
			ShowAt(instance, CalculateWindowSize());
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec watcherInitializer = new _003C_003Ec();

		public static Func<bool> candidateInitializer;

		public static Func<AnimatorGraphReflection.GraphNodeRef, bool> productInitializer;

		public static Func<AnimatorGraphReflection.GraphNodeRef, bool> _ExpressionInitializer;

		public static Func<AnimatorGraphReflection.GraphNodeRef, bool> systemInitializer;

		public static Func<AnimatorGraphReflection.TransitionEditionInfo, AnimatorTransitionBase> workerInitializer;

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

		public static Func<BehaviourPropertyMultiEditor, bool> m_ParamsInitializer;

		public static Func<StateMachineBehaviour, bool> _ListenerInitializer;

		public static Func<AnimatorStateTransition, bool> m_GetterInitializer;

		public static Func<AnimatorStateTransition, string> _InterceptorInitializer;

		public static Func<object[], bool[]> creatorInitializer;

		public static Func<Animator, bool> eventInitializer;

		public static Func<Type, IEnumerable<MethodInfo>> infoInitializer;

		public static Func<System.Reflection.Assembly, IEnumerable<MethodInfo>> _FacadeInitializer;

		public static Func<(MethodInfo, CallbackAttribute, bool), int> m_AdvisorInitializer;

		public static Action m_CallbackInitializer;

		public static Action _IndexerInitializer;

		public static Func<bool> m_IssuerInitializer;

		public static Action<Exception> m_PrototypeInitializer;

		public static Action m_RuleInitializer;

		public static Action<JsonObject> singletonInitializer;

		public static Action<Exception> _FactoryInitializer;

		public static Func<ProcessRunner, bool> _AccountInitializer;

		public static Func<string, bool> m_RefInitializer;

		public static Func<(bool, string), bool> statusInitializer;

		public static Func<(bool, string), string> _TokenInitializer;

		public static Func<(bool, string), bool> m_CodeInitializer;

		public static Func<(bool, string), string> _DicInitializer;

		public static Func<bool> m_InvocationInitializer;

		public static Func<bool> roleInitializer;

		public static Action<JsonObject> _ParamInitializer;

		public static Action<Exception> _ModelInitializer;

		public static Action m_TokenizerInitializer;

		public static Action _DecoratorInitializer;

		public static Action<JsonObject> m_ComparatorInitializer;

		public static Action<Exception> m_ExceptionInitializer;

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

		public static Action<Exception> mappingInitializer;

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

		public static Func<AnimatorGraphReflection.GraphNodeRef, bool> _ParserInitializer;

		public static Func<AnimatorStateTransition, bool> _ManagerInitializer;

		public static Func<bool> itemInitializer;

		public static Func<AnimatorGraphReflection.TransitionEditionInfo, bool> specificationInitializer;

		public static Func<System.Reflection.Assembly, IEnumerable<Type>> _MethodInitializer;

		public static Func<Type, bool> _SchemaInitializer;

		public static Func<Type, string> broadcasterInitializer;

		public static Func<EditorCurveBinding, bool> proxyInitializer;

		public static Func<EditorCurveBinding, string> structInitializer;

		public static Func<string, string> _ServiceInitializer;

		public static Func<EditorCurveBinding, string> m_StateInitializer;

		public static Func<string, string> globalInitializer;

		public static Func<ConditionMultiEditor, bool> _TaskInitializer;

		public static Func<ConditionMultiEditor, AnimatorCondition> m_ProcessInitializer;

		public static Func<ConditionMultiEditor, AnimatorCondition> producerInitializer;

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

		public static Action<AnimatorGraphReflection.TransitionEditionInfo> _BridgeDefinition;

		public static Func<ChildAnimatorState, bool> m_StrategyDefinition;

		public static Action<UnityEngine.Object> m_CustomerDefinition;

		public static Action<IEnumerable<UnityEditor.Animations.AnimatorController>> databaseDefinition;

		public static Action<UnityEngine.Object> _ExporterDefinition;

		public static Action<IEnumerable<GameObject>> identifierDefinition;

		public static Func<object, bool> attrDefinition;

		public static Func<object, EditorCurveBinding> dispatcherDefinition;

		public static Func<Renderer, IEnumerable<Material>> m_RegistryDefinition;

		public static Func<Material, Shader> m_TagDefinition;

		public static Action<DreadScripts.ControllerEditor.SearchablePickerPopup<string>.PickerEntry> importerDefinition;

		public static Func<string, string, bool> _RequestDefinition;

		public static Func<Component, Type> m_PrinterDefinition;

		public static Func<Type, bool> _WriterDefinition;

		public static Action<DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry> paramsDefinition;

		public static Func<Type, object[]> m_ListenerDefinition;

		public static Func<Type, string, bool> m_GetterDefinition;

		public static Func<ObjectReferenceKeyframe, float> interceptorDefinition;

		public static Func<IGrouping<float, ObjectReferenceKeyframe>, ObjectReferenceKeyframe> m_CreatorDefinition;

		public static Func<Keyframe, float> eventDefinition;

		public static Func<IGrouping<float, Keyframe>, Keyframe> m_InfoDefinition;

		public static Func<AnimatorGraphReflection.GraphEdgeRef, Vector3> _FacadeDefinition;

		public static Func<object[], bool[]> _AdvisorDefinition;

		public static Comparison<LayerPathNode> callbackDefinition;

		public static Comparison<LayerIndexEntry> m_IndexerDefinition;

		public static Func<string, string> m_IssuerDefinition;

		public static Func<LayerPathNode, bool> m_PrototypeDefinition;

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

		internal bool InvokeProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool FindProperty(AnimatorGraphReflection.GraphNodeRef nw)
		{
			return nw.Node() == AnimatorGraphReflection.GraphAccessors.EntryNode().Node();
		}

		internal bool ExcludeProperty(AnimatorGraphReflection.GraphNodeRef nw)
		{
			return nw.Node() == AnimatorGraphReflection.GraphAccessors.AnyStateNode().Node();
		}

		internal bool InitProperty(AnimatorGraphReflection.GraphNodeRef nw)
		{
			return nw.Node() == AnimatorGraphReflection.GraphAccessors.ExitNode().Node();
		}

		internal AnimatorTransitionBase VisitProperty(AnimatorGraphReflection.TransitionEditionInfo t)
		{
			return t.transition;
		}

		internal bool DefineProperty(AnimatorTransitionBase t)
		{
			return !selectedTransitions.Contains(t);
		}

		internal bool StartProperty(AnimatorTransitionBase t)
		{
			return !conditionEditorTransitions.Contains(t);
		}

		internal bool ReadProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool SelectProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool RemoveProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
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
					scaledBackgrounds = new Texture2D[1] { EditorUtils.SharedColorTexture(Color.black) }
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
					scaledBackgrounds = new Texture2D[1] { EditorUtils.SharedColorTexture(Color.black) }
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
			EditorSettings.GetInstance().useLegacyDropdown.Toggle();
		}

		internal void RateProperty()
		{
			GetAnnotation(updatereference: true);
		}

		internal void DestroyProperty()
		{
			GetAnnotation(updatereference: false);
		}

		internal bool GetProperty(BehaviourPropertyMultiEditor s)
		{
			return s.matched;
		}

		internal bool CalcProperty(StateMachineBehaviour b)
		{
			return b.GetType() != AnimatorTypeCache.GetTrackingControlType();
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

		internal int CreateProperty((MethodInfo, CallbackAttribute, bool onVerify) x)
		{
			return x.Item2.priority;
		}

		internal void NewProperty()
		{
			isSendingFeedback = false;
			feedbackPanelOpen = false;
			DrawLicenseInfo();
		}

		internal void PushProperty()
		{
			WriteAnnotation(assetneeded: false);
		}

		internal bool ViewProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal void CollectProperty(Exception exception)
		{
			isVerifyingLicense = false;
			isLicensed = false;
			licenseCheckRetryOffered = true;
			Log($"Something went wrong while verifying license:\n\n{exception}", CustomLogType.Error);
		}

		internal void ResolveProperty(JsonObject response)
		{
			isActivatingLicense = false;
			SortAnnotation(response, delegate
			{
				licenseKeyEntryRequired = false;
				EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: true);
				WriteAnnotation(assetneeded: true);
			});
		}

		internal void ListProperty()
		{
			licenseKeyEntryRequired = false;
			EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: true);
			WriteAnnotation(assetneeded: true);
		}

		internal void VerifyProperty(Exception exception)
		{
			isActivatingLicense = false;
			Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}

		internal bool FillProperty(ProcessRunner p)
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool UpdateProperty()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal void ChangeProperty()
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenserequest");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass239_0 _003C_003Ec__DisplayClass239_ = new _003C_003Ec__DisplayClass239_0
				{
					serviceDefinition = response
				};
				isRequestingTransferCode = false;
				SortAnnotation(_003C_003Ec__DisplayClass239_.serviceDefinition, _003C_003Ec__DisplayClass239_.InterruptObserver);
			}, delegate(Exception exception)
			{
				isRequestingTransferCode = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, DrawLicenseInfo);
		}

		internal void SortProperty(JsonObject response)
		{
			_003C_003Ec__DisplayClass239_0 _003C_003Ec__DisplayClass239_ = new _003C_003Ec__DisplayClass239_0
			{
				serviceDefinition = response
			};
			isRequestingTransferCode = false;
			SortAnnotation(_003C_003Ec__DisplayClass239_.serviceDefinition, _003C_003Ec__DisplayClass239_.InterruptObserver);
		}

		internal void RegisterProperty(Exception exception)
		{
			isRequestingTransferCode = false;
			Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void LogoutProperty()
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenseconfirm");
			list.Add(("verification_code", transferVerificationCode));
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				isConfirmingTransfer = false;
				SortAnnotation(response, delegate
				{
					showingTransferPanel = false;
					transferCodeSent = false;
					licenseKeyEntryRequired = false;
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(Exception exception)
			{
				isConfirmingTransfer = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, DrawLicenseInfo);
		}

		internal void PatchProperty(JsonObject response)
		{
			isConfirmingTransfer = false;
			SortAnnotation(response, delegate
			{
				showingTransferPanel = false;
				transferCodeSent = false;
				licenseKeyEntryRequired = false;
				WriteAnnotation(assetneeded: true);
			});
		}

		internal void InterruptProperty()
		{
			showingTransferPanel = false;
			transferCodeSent = false;
			licenseKeyEntryRequired = false;
			WriteAnnotation(assetneeded: true);
		}

		internal void ManageProperty(Exception exception)
		{
			isConfirmingTransfer = false;
			Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void PrintProperty()
		{
			SessionState.EraseString("yOk0XCnENLMO6DIF8cYpSg==updateinfo");
			AwakeVisitor();
		}

		internal void SearchProperty()
		{
			feedbackPanelOpen.Flip();
		}

		internal void RevertProperty()
		{
			EditorSettings.GetInstance().a_VerifyOnDisplay.Toggle();
			EditorSettings.GetInstance().a_VerifyOnProjectLoad.SetValue(excludeparam: false);
		}

		internal void OrderProcessor()
		{
			EditorSettings.GetInstance().a_VerifyOnProjectLoad.Toggle();
			EditorSettings.GetInstance().a_VerifyOnDisplay.SetValue(excludeparam: false);
		}

		internal void CompareProcessor()
		{
			Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
		}

		internal void SetProcessor()
		{
			Application.OpenURL(extraMenuLinks[0].Item2);
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

		internal void PopProcessor(Exception exc)
		{
			Log($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}

		internal void ComputeProcessor()
		{
			isCheckingForUpdate = false;
			DrawLicenseInfo();
		}

		internal async Task MoveProcessor()
		{
			await Task.Delay(3000);
			EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: true);
			DrawLicenseInfo();
		}

		internal bool ConcatProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool CallProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool CancelProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool CountProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool DisableProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool InsertProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal AnimatorState FindProcessor(ChildAnimatorState c)
		{
			return c.state;
		}

		internal bool ExcludeProcessor(AnimatorGraphReflection.GraphNodeRef n)
		{
			return n.nodeType == AnimatorGraphReflection.GraphNodeRef.NodeType.state;
		}

		internal bool InitProcessor(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass308_0 _003C_003Ec__DisplayClass308_ = new _003C_003Ec__DisplayClass308_0
			{
				serverReg = t
			};
			return ActiveStateMachine().states.Any(_003C_003Ec__DisplayClass308_.InvokeServer);
		}

		internal bool VisitProcessor()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool DefineProcessor(AnimatorGraphReflection.TransitionEditionInfo et)
		{
			return et.transition != null;
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
				return b.type.Is<Renderer>();
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

		internal bool ConnectProcessor(ConditionMultiEditor sc)
		{
			return !sc.matched;
		}

		internal AnimatorCondition CalculateProcessor(ConditionMultiEditor sc)
		{
			return sc.condition;
		}

		internal AnimatorCondition TestProcessor(ConditionMultiEditor sc)
		{
			return sc.condition;
		}

		internal bool MapProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == actionFilterText))
			{
				return false;
			}
			return t.isExit;
		}

		internal bool ValidateProcessor(AnimatorStateTransition t)
		{
			if (t.name == actionSourceName)
			{
				return t.isExit;
			}
			return false;
		}

		internal bool CustomizeProcessor(AnimatorStateTransition t)
		{
			return t.name == actionFilterText;
		}

		internal bool RateProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText && t.isExit);
		}

		internal bool DestroyProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == actionFilterText))
			{
				return false;
			}
			return t.isExit;
		}

		internal bool GetProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine == RootStateMachine();
		}

		internal bool CalcProcessor(UnityEditor.Animations.AnimatorControllerLayer l)
		{
			return l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText && t.isExit);
		}

		internal bool IncludeProcessor(AnimatorStateTransition t)
		{
			if (!(t.name == actionFilterText))
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
			l.ForEachStateMachine(delegate(AnimatorStateMachine m)
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
			return EditorUtils.reservedAvatarParameters.Contains(p.name);
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
			RateAlgo(EditorSettings.GetInstance().defaultTransition, t);
		}

		internal bool ForgotProcessor(ChildAnimatorState c)
		{
			return selectedStates.Contains(c.state);
		}

		internal bool StopProcessor(ChildAnimatorStateMachine c)
		{
			return selectedStateMachines.Contains(c.stateMachine);
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
			return selectedStates.Contains(c.state);
		}

		internal float UpdateProcessor(ChildAnimatorState c)
		{
			return c.position.x;
		}

		internal bool ChangeProcessor(ChildAnimatorState c)
		{
			return selectedStates.Contains(c.state);
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
			return cosmeticOnlyStyleNames.Contains(t);
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
			return ActiveStateMachine().entryTransitions.Contains(t);
		}

		internal bool RevertProcessor(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass420_1 _003C_003Ec__DisplayClass420_ = new _003C_003Ec__DisplayClass420_1
			{
				_ValueReg = t
			};
			return ActiveStateMachine().states.Any(_003C_003Ec__DisplayClass420_.QueryThread);
		}

		internal bool OrderObserver(UnityEngine.Object o)
		{
			return !(o is AnimatorStateTransition);
		}

		internal bool CompareObserver(AnimatorStateTransition t)
		{
			if (selectedStates.Contains(t.destinationState))
			{
				return true;
			}
			if (!exitNodeSelected)
			{
				return false;
			}
			return t.isExit;
		}

		internal bool SetObserver(AnimatorStateTransition t)
		{
			return selectedStates.Contains(t.destinationState);
		}

		internal bool PostObserver(AnimatorTransition t)
		{
			return selectedStates.Contains(t.destinationState);
		}

		internal void SetupObserver(AnimatorGraphReflection.TransitionEditionInfo t)
		{
			_003C_003Ec__DisplayClass430_0 _003C_003Ec__DisplayClass430_ = new _003C_003Ec__DisplayClass430_0
			{
				m_MerchantReg = t
			};
			EditorUtils.ForEach(selectedStates, _003C_003Ec__DisplayClass430_.AddThread);
			if (anyStateNodeSelected && !_003C_003Ec__DisplayClass430_.m_MerchantReg.transition.IsExitOrDangling())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.transition, ActiveStateMachine().AddAnyStateTransition((AnimatorState)null));
			}
			if (entryNodeSelected && !_003C_003Ec__DisplayClass430_.m_MerchantReg.transition.IsExitOrDangling())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.transition, ActiveStateMachine().AddEntryTransition((AnimatorState)null));
			}
			if (replaceTransitions)
			{
				_003C_003Ec__DisplayClass430_.m_MerchantReg.Remove();
			}
		}

		internal bool EnableObserver(ChildAnimatorState c)
		{
			return c.state.transitions.Contains(focusedTransition.transition);
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
			overrideAnimationRoot = (GameObject)o;
		}

		internal void MoveObserver(IEnumerable<GameObject> o)
		{
			GameObject[] source = (o as GameObject[]) ?? o.ToArray();
			if (source.Any())
			{
				overrideAnimationRoot = source.First();
			}
		}

		internal bool ConcatObserver(object n)
		{
			return ((TreeViewItem)n).children.CalcRules();
		}

		internal EditorCurveBinding CallObserver(object n)
		{
			return (EditorCurveBinding)hierarchyNodeBindingField.GetValue(n);
		}

		internal IEnumerable<Material> CancelObserver(Renderer c)
		{
			return c.sharedMaterials;
		}

		internal Shader CountObserver(Material m)
		{
			return m.shader;
		}

		internal void DisableObserver(DreadScripts.ControllerEditor.SearchablePickerPopup<string>.PickerEntry i)
		{
			GUILayout.Label(i.value, EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			EditorUtils.AddLinkCursor();
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

		internal void AddObserver(DreadScripts.ControllerEditor.SearchablePickerPopup<Type>.PickerEntry i)
		{
			GUILayout.Label((GUIContent)i.FirstExtra(), EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
			EditorUtils.AddLinkCursor();
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

		internal Vector3 StartObserver(AnimatorGraphReflection.GraphEdgeRef e)
		{
			return InitAnnotation(e)[1];
		}

		internal bool[] ReadObserver(object[] values)
		{
			return EditorUtils.Args<bool>(((string)values[0]).IsNullOrWhiteSpace());
		}

		internal int SelectObserver(LayerPathNode c1, LayerPathNode c2)
		{
			return string.Compare(c1.name, c2.name, StringComparison.Ordinal);
		}

		internal int RemoveObserver(LayerIndexEntry l1, LayerIndexEntry l2)
		{
			return string.Compare(l1.layer.name, l2.layer.name, StringComparison.Ordinal);
		}

		internal string InstantiateObserver(string n)
		{
			return n;
		}

		internal bool AwakeObserver(LayerPathNode c2)
		{
			return c2.layers.Count > 0;
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
			return EditorUtils.StripNumberSuffix(cs.state.name);
		}

		internal object CalculateObserver()
		{
			return layerControllerViewType.GetMethod("get_renameOverlay").Invoke(ReadAnnotation(), null);
		}

		internal void TestObserver(bool accepted)
		{
			if (accepted)
			{
				RestartAlgo(ActiveStateMachine(), selectedStates, stateRenameOverlay.Name());
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
					col = ((!animatorStateMachine.states.GetRules()) ? RootStateMachine().AddAnyStateTransition(animatorStateMachine) : RootStateMachine().anyStateTransitions.Last());
				}
			}
			else
			{
				col = RootStateMachine().AddAnyStateTransition(destinationState);
			}
			CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, col);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass186_0
	{
		public string m_ObjectDefinition;

		internal string RunObserver(string key, ref _003C_003Ec__DisplayClass186_1 _003C_003Ec__DisplayClass186_1_0, ref _003C_003Ec__DisplayClass186_2 _003C_003Ec__DisplayClass186_2_0)
		{
			return RateMapper(SessionState.GetString(DestroyMapper(m_ObjectDefinition + key, ref _003C_003Ec__DisplayClass186_2_0), string.Empty), ref _003C_003Ec__DisplayClass186_1_0);
		}

		internal void CloneObserver()
		{
			List<(string, string)> list = RegisterAnnotation("verifylicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass186_3 _003C_003Ec__DisplayClass186_ = new _003C_003Ec__DisplayClass186_3
				{
					m_MerchantDefinition = this,
					valueDefinition = response
				};
				isVerifyingLicense = false;
				licenseKeyEntryRequired = true;
				SortAnnotation(_003C_003Ec__DisplayClass186_.valueDefinition, _003C_003Ec__DisplayClass186_.DeleteObserver, SearchAnnotation, t2stop: false);
			}, _003C_003Ec.watcherInitializer.CollectProperty, null, null, DrawLicenseInfo);
		}

		internal void LoginObserver(JsonObject response)
		{
			_003C_003Ec__DisplayClass186_3 _003C_003Ec__DisplayClass186_ = new _003C_003Ec__DisplayClass186_3
			{
				m_MerchantDefinition = this,
				valueDefinition = response
			};
			isVerifyingLicense = false;
			licenseKeyEntryRequired = true;
			SortAnnotation(_003C_003Ec__DisplayClass186_.valueDefinition, _003C_003Ec__DisplayClass186_.DeleteObserver, SearchAnnotation, t2stop: false);
		}

		internal void ReflectObserver(string key, string value, ref _003C_003Ec__DisplayClass186_4 _003C_003Ec__DisplayClass186_4_0, ref _003C_003Ec__DisplayClass186_5 _003C_003Ec__DisplayClass186_5_0)
		{
			SessionState.SetString(GetMapper(m_ObjectDefinition + key, ref _003C_003Ec__DisplayClass186_5_0), CalcMapper(value, ref _003C_003Ec__DisplayClass186_4_0));
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
		public JsonObject valueDefinition;

		public _003C_003Ec__DisplayClass186_0 m_MerchantDefinition;

		internal void DeleteObserver()
		{
			try
			{
				string text = valueDefinition.Item("date");
				if (PatchAnnotation() != text)
				{
					Log("Date Mismatch! Please make sure your system's date is correct.\nLocal: " + currentDateStamp + "  |  Remote: " + text, CustomLogType.Error);
					licenseCheckRetryOffered = true;
					SearchAnnotation();
					return;
				}
				licenseUsername = valueDefinition.Item("username");
				licenseVariant = valueDefinition.Item("variant");
				licenseToken = valueDefinition.Item("token");
				StopAnnotation();
				CheckAnnotation();
				string param = valueDefinition.Item("message");
				if (!licenseRestoredFromCache)
				{
					Log(param);
				}
				isLicensed = true;
				EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: true);
				EditorPrefs.SetString("yOk0XCnENLMO6DIF8cYpSg==LK", licenseKey);
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
						m_MerchantDefinition.ReflectObserver("date", currentDateStamp, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("u", licenseUsername, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("v", licenseVariant, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("r", licenseToken, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
						m_MerchantDefinition.ReflectObserver("m", hardwareId, ref _003C_003Ec__DisplayClass186_4_, ref _003C_003Ec__DisplayClass186_5_);
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
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
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

		public ProcessRunner[] instanceDefinition;

		public bool _FieldDefinition;

		public Action m_AttributeDefinition;

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
			catch (Exception exc)
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
			catch (Exception exc)
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
				throw new Exception("Failed to gather hardware info through " + ((!isCMD) ? "Shell" : "CMD"));
			}
		}

		internal void ChangeObserver(bool isCMD, Exception exc)
		{
			if (!isCMD)
			{
				isConfirmingTransfer = false;
				isRequestingTransferCode = false;
				isVerifyingLicense = false;
				isActivatingLicense = false;
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
				isVerifyingLicense = false;
				isActivatingLicense = false;
				DrawLicenseInfo();
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
				catch (Exception exc2)
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
			hardwareId = string.Join("-", array);
			CheckAnnotation();
			m_AttributeDefinition();
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

		internal bool LogoutObserver(string v)
		{
			return v == m_DescriptorDefinition[0];
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass239_0
	{
		public JsonObject serviceDefinition;

		internal void InterruptObserver()
		{
			transferTargetEmail = serviceDefinition.Item("transfer_email");
			transferCodeSent = true;
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass308_0
	{
		public AnimatorStateTransition serverReg;

		internal bool InvokeServer(ChildAnimatorState c)
		{
			return c.state == serverReg.destinationState;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass370_1
	{
		public string _ResolverReg;

		internal bool VisitServer(UnityEngine.AnimatorControllerParameter p2)
		{
			return p2.name != _ResolverReg;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass375_0
	{
		public HashSet<UnityEngine.Object> consumerReg;

		public Action<AnimatorStateTransition> _AdapterReg;

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
			_003C_003Ec__DisplayClass375_.interpreterReg.states.ForEach(delegate(ChildAnimatorState s)
			{
				ValidateServer(s.state);
				s.state.transitions.ForEach(delegate(AnimatorStateTransition t)
				{
					if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
					{
						ValidateServer(t);
					}
				});
				((IEnumerable<StateMachineBehaviour>)s.state.behaviours).ForEach((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
				{
					consumerReg.Add(o);
				});
				s.state.motion.ForgotPredicate((Action<Motion>)delegate(UnityEngine.Object o)
				{
					consumerReg.Add(o);
				});
			});
			((IEnumerable<StateMachineBehaviour>)_003C_003Ec__DisplayClass375_.interpreterReg.behaviours).ForEach((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
			{
				consumerReg.Add(o);
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.entryTransitions.ForEach(delegate(AnimatorTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.anyStateTransitions.ForEach(delegate(AnimatorStateTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			_003C_003Ec__DisplayClass375_.interpreterReg.stateMachines.ForEach(_003C_003Ec__DisplayClass375_.RunServer);
		}

		internal void RateServer(ChildAnimatorState s)
		{
			ValidateServer(s.state);
			s.state.transitions.ForEach(delegate(AnimatorStateTransition t)
			{
				if ((bool)t && ((bool)t.destinationState || (bool)t.destinationStateMachine || t.isExit))
				{
					ValidateServer(t);
				}
			});
			((IEnumerable<StateMachineBehaviour>)s.state.behaviours).ForEach((Action<StateMachineBehaviour>)delegate(UnityEngine.Object o)
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass375_1
	{
		public AnimatorStateMachine interpreterReg;

		public _003C_003Ec__DisplayClass375_0 watcherReg;

		internal void RunServer(ChildAnimatorStateMachine c)
		{
			interpreterReg.GetStateMachineTransitions(c.stateMachine).ForEach(watcherReg.ValidateServer);
			watcherReg.CustomizeServer(c.stateMachine);
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
			string text2 = $"{EditorSettings.GetInstance().saveFolder}/Animation Clips/{m_ProductReg.name}";
			EditorUtils.InstantiateList(text2, overridecust: false, EditorUtils.PathOption.ForceFolder);
			string text3 = EditorUtils.AwakeList(text2, m.name + text, writestate: true);
			bool proc;
			T val = EditorUtils.CloneToAsset(m, text3, out proc, isinfo2: false);
			if (proc)
			{
				EditorUtils.AddSubAsset(val, m_ProductReg);
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass379_0
	{
		public bool _ReaderReg;

		public string m_BridgeReg;

		public string m_StrategyReg;

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
				if (!(stateMachineBehaviour.GetType() == AnimatorTypeCache.GetParameterDriverType()))
				{
					continue;
				}
				AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour);
				for (int num = parameterDriverBinding.parameters.Count - 1; num >= 0; num--)
				{
					if (PushServer(parameterDriverBinding.parameters[num].GetName()))
					{
						parameterDriverBinding.parameters[num].SetName(ViewServer(parameterDriverBinding.parameters[num].GetName()));
					}
					if (PushServer(parameterDriverBinding.parameters[num].GetSource()))
					{
						parameterDriverBinding.parameters[num].SetSource(ViewServer(parameterDriverBinding.parameters[num].GetSource()));
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
			if (AnimatorTypeCache.IsVRCSDKAvailable())
			{
				CollectServer(s.behaviours);
			}
		}

		internal void VerifyServer(AnimatorStateTransitionSet t)
		{
			for (int num = t.GetConditions().Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] conditions = t.GetConditions();
				conditions[num].parameter = ViewServer(conditions[num].parameter);
				t.SetConditions(conditions);
			}
			EditorUtility.SetDirty((AnimatorTransitionBase)t);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass380_0
	{
		public bool customerReg;

		public string _DatabaseReg;

		public string m_ExporterReg;

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
				if (!(stateMachineBehaviour.GetType() == AnimatorTypeCache.GetParameterDriverType()))
				{
					continue;
				}
				AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour);
				for (int num = parameterDriverBinding.parameters.Count - 1; num >= 0; num--)
				{
					if (FillServer(parameterDriverBinding.parameters[num].GetName()))
					{
						parameterDriverBinding.parameters[num].SetName(WriteServer(parameterDriverBinding.parameters[num].GetName()));
					}
					if (FillServer(parameterDriverBinding.parameters[num].GetSource()))
					{
						parameterDriverBinding.parameters[num].SetSource(WriteServer(parameterDriverBinding.parameters[num].GetSource()));
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass381_0
	{
		public string _IdentifierReg;

		public Action<StateMachineBehaviour[]> _AttrReg;

		public AnimatorStateMachine m_DispatcherReg;

		internal bool CheckServer(string s)
		{
			if (!matchWholeWord || !(s == _IdentifierReg))
			{
				if (!matchWholeWord)
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
				if (stateMachineBehaviour.GetType() != AnimatorTypeCache.GetParameterDriverType())
				{
					continue;
				}
				AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour);
				for (int num = parameterDriverBinding.parameters.Count - 1; num >= 0; num--)
				{
					if (CheckServer(parameterDriverBinding.parameters[num].GetName()))
					{
						parameterDriverBinding.RemoveParameter(num);
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
			if (AnimatorTypeCache.IsVRCSDKAvailable())
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass382_0
	{
		public string _RegistryReg;

		internal string RegisterServer(string s)
		{
			if (string.IsNullOrEmpty(s) || EditorUtils.reservedAvatarParameters.Contains(s))
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
				if (!(stateMachineBehaviour.GetType() == AnimatorTypeCache.GetParameterDriverType()))
				{
					continue;
				}
				foreach (AnimatorTypeCache.ParameterDriverBinding.ParameterEntry parameter in new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour).parameters)
				{
					parameter.SetName(RegisterServer(parameter.GetName()));
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
				if (!(stateMachineBehaviour.GetType() == AnimatorTypeCache.GetParameterDriverType()))
				{
					continue;
				}
				foreach (AnimatorTypeCache.ParameterDriverBinding.ParameterEntry parameter in new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour).parameters)
				{
					parameter.SetName(RegisterServer(parameter.GetName()));
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

		internal bool SetupThread(ChildAnimatorState c)
		{
			return _AccountReg.destinationState == c.state;
		}

		internal bool EnableThread(ChildAnimatorStateMachine c)
		{
			return _AccountReg.destinationStateMachine == c.stateMachine;
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

		internal void PopThread(List<AnimatorStateTransition> t, AnimatorState s)
		{
			_003C_003Ec__DisplayClass413_1 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_1();
			if (t.Count != 1)
			{
				_003C_003Ec__DisplayClass413_._ParamReg = CalculateAlgo(t[0]);
				s.RemoveTransition(t[0]);
				for (int i = 1; i < t.Count; i++)
				{
					t[i].conditions.ForEach(_003C_003Ec__DisplayClass413_.CancelThread);
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
				_TokenizerReg = ActiveStateMachine().AddEntryTransition(t[0].destinationState)
			};
			EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._TokenizerReg);
			ActiveStateMachine().RemoveEntryTransition((AnimatorTransition)t[0]);
			for (int i = 1; i < t.Count; i++)
			{
				t[i].conditions.ForEach(_003C_003Ec__DisplayClass413_.CountThread);
				ActiveStateMachine().RemoveEntryTransition((AnimatorTransition)t[i]);
			}
			tokenReg.Add(_003C_003Ec__DisplayClass413_._TokenizerReg);
		}

		internal void MoveThread(List<AnimatorStateTransition> t)
		{
			_003C_003Ec__DisplayClass413_3 _003C_003Ec__DisplayClass413_ = new _003C_003Ec__DisplayClass413_3
			{
				_ComparatorReg = ActiveStateMachine().AddAnyStateTransition(t[0].destinationState)
			};
			EditorUtility.CopySerialized(t[0], _003C_003Ec__DisplayClass413_._ComparatorReg);
			ActiveStateMachine().RemoveAnyStateTransition(t[0]);
			for (int i = 1; i < t.Count; i++)
			{
				t[i].conditions.ForEach(_003C_003Ec__DisplayClass413_.DisableThread);
				ActiveStateMachine().RemoveAnyStateTransition(t[i]);
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_1
	{
		public AnimatorStateTransition _ParamReg;

		public Action<AnimatorCondition> _ModelReg;

		internal void CancelThread(AnimatorCondition c)
		{
			_ParamReg.AddCondition(c.mode, c.threshold, c.parameter);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_2
	{
		public AnimatorTransitionBase _TokenizerReg;

		public Action<AnimatorCondition> decoratorReg;

		internal void CountThread(AnimatorCondition c)
		{
			_TokenizerReg.AddCondition(c.mode, c.threshold, c.parameter);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass413_3
	{
		public AnimatorStateTransition _ComparatorReg;

		public Action<AnimatorCondition> m_ExceptionReg;

		internal void DisableThread(AnimatorCondition c)
		{
			_ComparatorReg.AddCondition(c.mode, c.threshold, c.parameter);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass420_1
	{
		public AnimatorStateTransition _ValueReg;

		internal bool QueryThread(ChildAnimatorState s)
		{
			return s.state == _ValueReg.destinationState;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass430_0
	{
		public AnimatorGraphReflection.TransitionEditionInfo m_MerchantReg;

		internal void AddThread(AnimatorState s)
		{
			TestAlgo(m_MerchantReg.transition, s.AddTransition((AnimatorState)null));
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass431_0
	{
		public HashSet<UnityEngine.Object> authenticationReg;

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
			if (!RootStateMachine().anyStateTransitions.Any(_003C_003Ec__DisplayClass431_.FindThread))
			{
				authenticationReg.Add(MapAlgo(_003C_003Ec__DisplayClass431_._ReponseReg));
				int num = 0;
				while (true)
				{
					if (num >= ActiveStateMachine().states.Length)
					{
						return;
					}
					if (ActiveStateMachine().states[num].state.transitions.Any(_003C_003Ec__DisplayClass431_.InitThread))
					{
						break;
					}
					num++;
				}
				Undo.RecordObject(ActiveStateMachine().states[num].state, "Make AnyTransition");
				ActiveStateMachine().states[num].state.RemoveTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
			}
			else
			{
				ActiveStateMachine().states.ForEach(_003C_003Ec__DisplayClass431_.ExcludeThread);
				_ = RootStateMachine().anyStateTransitions;
				Undo.RecordObject(RootStateMachine(), "Remove AnyTransition");
				RootStateMachine().RemoveAnyStateTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass431_1
	{
		public AnimatorStateTransition _ReponseReg;

		public _003C_003Ec__DisplayClass431_0 m_PoolReg;

		public Func<AnimatorStateTransition, bool> _ParameterReg;

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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_0
	{
		public List<AnimatorTransitionBase> _ComposerReg;

		internal void VisitThread(AnimatorTransitionBase t)
		{
			_003C_003Ec__DisplayClass432_1 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_1
			{
				mappingReg = this,
				m_RepositoryReg = t
			};
			if (_003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.Length != 0)
			{
				_003C_003Ec__DisplayClass432_.m_RepositoryReg.conditions.ForEach(_003C_003Ec__DisplayClass432_.ReadThread);
				ActiveStateMachine().RemoveEntryTransition((AnimatorTransition)_003C_003Ec__DisplayClass432_.m_RepositoryReg);
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
				_003C_003Ec__DisplayClass432_.baseReg.conditions.ForEach(_003C_003Ec__DisplayClass432_.SelectThread);
				ActiveStateMachine().RemoveAnyStateTransition(_003C_003Ec__DisplayClass432_.baseReg);
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
				_003C_003Ec__DisplayClass432_.m_ClassReg.conditions.ForEach(_003C_003Ec__DisplayClass432_.RemoveThread);
				_003C_003Ec__DisplayClass432_.mockReg.RemoveTransition(_003C_003Ec__DisplayClass432_.m_ClassReg);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_1
	{
		public AnimatorTransitionBase m_RepositoryReg;

		public _003C_003Ec__DisplayClass432_0 mappingReg;

		internal void ReadThread(AnimatorCondition c)
		{
			AnimatorTransitionBase animatorTransitionBase = ActiveStateMachine().AddEntryTransition(m_RepositoryReg.destinationState);
			EditorUtility.CopySerializedManagedFieldsOnly(m_RepositoryReg, animatorTransitionBase);
			animatorTransitionBase.conditions = new AnimatorCondition[1] { c };
			mappingReg._ComposerReg.Add(animatorTransitionBase);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_2
	{
		public AnimatorStateTransition baseReg;

		public _003C_003Ec__DisplayClass432_0 containerReg;

		internal void SelectThread(AnimatorCondition c)
		{
			AnimatorTransitionBase animatorTransitionBase = ActiveStateMachine().AddAnyStateTransition(baseReg.destinationState);
			EditorUtility.CopySerialized(baseReg, animatorTransitionBase);
			animatorTransitionBase.conditions = new AnimatorCondition[1] { c };
			containerReg._ComposerReg.Add(animatorTransitionBase);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass432_3
	{
		public AnimatorStateTransition m_ClassReg;

		public AnimatorState mockReg;

		public _003C_003Ec__DisplayClass432_0 instanceReg;

		internal void RemoveThread(AnimatorCondition c)
		{
			AnimatorStateTransition animatorStateTransition = CalculateAlgo(m_ClassReg);
			animatorStateTransition.conditions = new AnimatorCondition[1] { c };
			mockReg.AddTransition(animatorStateTransition);
			instanceReg._ComposerReg.Add(animatorStateTransition);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass483_1
	{
		public EditorCurveBinding parserReg;

		internal bool MapThread(string p)
		{
			return p == parserReg.propertyName;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass616_0
	{
		public object m_IteratorReg;

		public string _PublisherReg;

		internal T ReflectThread<T>(string methodName) where T : Delegate
		{
			return (T)Delegate.CreateDelegate(typeof(T), m_IteratorReg, layerControllerViewType.DisableList(methodName));
		}

		internal bool DeleteThread(LayerIndexEntry l)
		{
			return l.layer.name == _PublisherReg;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass670_1
	{
		public UnityEditor.Animations.BlendTree visitorTests;

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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass73_0
	{
		public GUIStyle[] m_ObserverTests;

		public Texture2D _ServerTests;

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
					stateStylesByTag.Add(name, gUIStyle);
				}
				if (isCosmeticOnlyStyle)
				{
					cosmeticOnlyStyleNames.Add(name);
				}
			}
			if (!hiddenFromList)
			{
				styleMenuNames.Add(name.Substring(3));
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
	}

	private static EditorWindow activeWindow;

	private static UnityEditor.Animations.AnimatorController currentController;

	private static AnimatorStateMachine @class;

	private static AnimatorStateMachine activeStateMachine;

	private static Vector2 windowScroll;

	private static int thresholdControlCounter;

	private static bool writeDefaultsPanelOpen;

	private static bool subAssetPanelOpen;

	private static bool stateSectionVisible;

	private static bool anyStateNodeSelected;

	private static bool entryNodeSelected;

	private static bool exitNodeSelected;

	private static bool hasStateTransitionSelected;

	private static bool hasPlainTransitionSelected;

	private static bool showSharedConditions = true;

	private static string[] exitTransitionNames;

	private static string[] parameterNames;

	private static string[] boolParameterNames;

	private static string[] floatParameterNames;

	private static StateMachineBehaviour[] @struct;

	private static MethodVisitor assetInventory;

	private static AnimatorStateTransition[] mixedValueTransitionPair;

	private static SerializedObject mixedValueTransitionSerialized;

	private static readonly List<AnimatorGraphReflection.GraphNodeRef> emptyNodeSelection = new List<AnimatorGraphReflection.GraphNodeRef>();

	private static readonly ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef> emptyEdgeSelection = new ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef>();

	private static List<AnimatorTransitionBase> conditionEditorTransitions = new List<AnimatorTransitionBase>();

	private static SerializedObject selectedStatesSerialized;

	private static SerializedObject transitionInspectorSerialized;

	private static AnimatorStateMachine[] selectedStateMachines;

	private static AnimatorStateMachine[] multiTransitionStateMachines;

	private static List<AnimatorState> multiTransitionStates = new List<AnimatorState>();

	private static List<AnimatorState> selectedStates = new List<AnimatorState>();

	private static List<AnimatorGraphReflection.GraphNodeRef> selectedNodes;

	private static ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef> selectedEdges;

	private static List<AnimatorGraphReflection.TransitionEditionInfo> selectedTransitionEdits;

	private static AnimatorTransitionBase[] exitNodeIncomingTransitions;

	private static List<AnimatorStateTransition> selectedStateTransitions = new List<AnimatorStateTransition>();

	private static List<AnimatorTransitionBase> selectedTransitions = new List<AnimatorTransitionBase>();

	private static AnimatorGraphReflection.TransitionEditionInfo focusedTransition;

	private static AnimatorStateTransition copiedTransitionSettings;

	private static List<AnimatorGraphReflection.TransitionEditionInfo> pendingTransitionEdits = new List<AnimatorGraphReflection.TransitionEditionInfo>();

	private static List<AnimatorCondition> copiedConditions = new List<AnimatorCondition>();

	private static readonly Dictionary<string, GUIStyle> stateStylesByTag = new Dictionary<string, GUIStyle>();

	private static readonly HashSet<string> cosmeticOnlyStyleNames = new HashSet<string>();

	private static readonly List<string> styleMenuNames = new List<string>();

	private static Dictionary<string, GUIStyle> unityNodeStyleCache;

	private static GUIStyle defaultStateNodeStyle;

	private static TrackingControlEditor trackingControlEditor;

	private static List<BehaviourPropertyMultiEditor> parameterDriverEditors = new List<BehaviourPropertyMultiEditor>();

	private static ReorderableList parameterDriverList;

	private static List<AnimatorTypeCache.ParameterDriverBinding> parameterDriverBindings = new List<AnimatorTypeCache.ParameterDriverBinding>();

	private static bool allStatesHaveTrackingControl;

	private static FieldInfo layerEditorField;

	private static FieldInfo previewAnimatorField;

	private static PropertyInfo liveLinkProperty;

	private static PropertyInfo selectedLayerIndexProperty;

	private static MethodInfo activeGraphGUIGetter;

	private static MethodInfo getEdgePointsMethod;

	private static bool bugReporterOpen;

	private static bool unusedLicensingFlag;

	private static string unusedLicensingText;

	private static bool feedbackPanelOpen;

	private static bool isSendingFeedback;

	private static string feedbackText;

	private static string licenseUsername;

	private static string licensedToDisplayName;

	private static string licenseVariant;

	private static string licenseKey = "";

	private static string transferVerificationCode = "";

	private static string transferTargetEmail = "";

	private static string sessionId;

	private static bool serverWarnedTooManyAttempts;

	private static bool licenseKeyEntryRequired;

	private static bool licenseCheckRetryOffered;

	private static bool licenseCheckedThisSession;

	private static float retryAllowedAtRealtime;

	private static string currentDateStamp;

	private static bool isActivatingLicense;

	private static bool isVerifyingLicense;

	private static string unreadDeviceDateFingerprint;

	private static string hardwareId;

	private static string licenseToken;

	private static bool isLicensed;

	private static bool licenseRestoredFromCache;

	private static bool licensedCallbacksFlushed;

	private static Action pendingLicensedCallbacks;

	private static Action pendingResetCallbacks;

	private static readonly Type[] repaintTargetTypes = new Type[2]
	{
		typeof(ControllerEditor),
		typeof(ControllerEditorWindow)
	};

	private static bool showingTransferPanel;

	private static bool transferCodeSent;

	private static bool isRequestingTransferCode;

	private static bool isConfirmingTransfer;

	private static bool isDownloadingUpdate;

	private static bool isCheckingForUpdate;

	private static bool hasCheckedForUpdate;

	private static bool updateAvailable;

	private static readonly AnimBool updateFoldout = new AnimBool();

	private static readonly AnimBool announcementFoldout = new AnimBool();

	private static readonly VersionNumber m_RefAnnotation = new VersionNumber("3.3.2");

	private static readonly (string, string)[] extraMenuLinks = new(string, string)[1] { ("Templates", "https://notes.sleightly.dev/templates/") };

	private static List<ConditionMultiEditor> sharedConditionEditors = new List<ConditionMultiEditor>();

	private static List<ConditionMultiEditor> allConditionEditors = new List<ConditionMultiEditor>();

	private static List<ConditionMultiEditor> focusedConditionEditors = new List<ConditionMultiEditor>();

	private static ReorderableList sharedConditionList;

	private static ReorderableList allConditionList;

	private static ReorderableList focusedConditionList;

	private static int subAssetTabIndex = -1;

	private static SerializedProperty stateNameProperty;

	private static SerializedProperty stateTagProperty;

	private static SerializedProperty stateMotionProperty;

	private static SerializedProperty stateSpeedProperty;

	private static SerializedProperty stateSpeedParameterProperty;

	private static SerializedProperty stateTimeParameterProperty;

	private static SerializedProperty stateMirrorProperty;

	private static SerializedProperty stateCycleOffsetProperty;

	private static SerializedProperty stateIkOnFeetProperty;

	private static SerializedProperty stateWriteDefaultsProperty;

	private static SerializedProperty stateSpeedParameterActiveProperty;

	private static SerializedProperty stateTimeParameterActiveProperty;

	private static SerializedProperty stateMirrorParameterActiveProperty;

	private static SerializedProperty stateCycleOffsetParameterActiveProperty;

	private static SerializedProperty stateMirrorParameterProperty;

	private static SerializedProperty stateCycleOffsetParameterProperty;

	private static bool replicateTransitionsMode;

	private static bool redirectTransitionsMode;

	private static bool makeMultipleTransitionsMode;

	private static SerializedProperty transitionHasExitTimeProperty;

	private static SerializedProperty transitionExitTimeProperty;

	private static SerializedProperty transitionHasFixedDurationProperty;

	private static SerializedProperty transitionDurationProperty;

	private static SerializedProperty transitionOffsetProperty;

	private static SerializedProperty transitionInterruptionSourceProperty;

	private static SerializedProperty transitionOrderedInterruptionProperty;

	private static SerializedProperty transitionCanTransitionToSelfProperty;

	private static SerializedProperty transitionSoloProperty;

	private static SerializedProperty transitionMuteProperty;

	private static bool transitionSectionVisible;

	private static Type[] componentTypes;

	private static readonly Dictionary<Type, string[]> animatablePropertiesByType = new Dictionary<Type, string[]>();

	private static readonly Dictionary<Shader, string[]> materialPropertiesByShader = new Dictionary<Shader, string[]>();

	private static UnityEditor.Animations.AnimatorController actionTargetController;

	private static string actionSourceName;

	private static string actionReplacementName;

	private static string actionFilterText;

	private static bool matchWholeWord = true;

	private static bool addRequiredParameters = true;

	private static string copiedParameterSuffix;

	private static ControllerAction selectedAction = ControllerAction.Copy;

	private static ActionMode actionScope = ActionMode.CurrentController;

	private static MoveMode copySourceScope = MoveMode.CurrentLayer;

	private MoveDestination copyDestination;

	private static EditorWindow animationWindow;

	private static GameObject previewRoot;

	private static Animator previewAnimator;

	private static object unusedPreviewObject;

	private static bool forceGameObjectSelectionUpdate;

	private static UnityEditor.Animations.AnimatorController overrideAnimationController;

	private static bool overrideAnimationRootActive;

	private static GameObject overrideAnimationRoot;

	private static Type animationWindowType;

	private static Type animationWindowStateType;

	private static Type animationWindowHierarchyGUIType;

	private static Type animEditorType;

	private static Type animationWindowControlType;

	private static Type animationWindowSelectionItemType;

	private static PropertyInfo animationWindowStateProperty;

	private static PropertyInfo activeAnimationClipProperty;

	private static PropertyInfo activeRootGameObjectProperty;

	private static PropertyInfo activeGameObjectProperty;

	private static PropertyInfo activeScriptableObjectProperty;

	private static FieldInfo hierarchyNodeBindingField;

	private static bool propertyEditingMenuAllowed;

	private static List<object> interactedHierarchyNodes;

	private static Rect unusedGraphRect;

	private static Type graphGUIType;

	private static Type graphEdgeType;

	private static Type graphStylesType;

	private static FieldInfo graphBackgroundStyleField;

	private static Texture2D graphBackgroundTexture;

	private static readonly HashSet<Vector3> animatedEdgeArrowPoints = new HashSet<Vector3>();

	private static bool arrowLerpEnabled;

	private static bool animatingSelectedEdges;

	private static Type animatorControllerToolType;

	private static Type graphType;

	private static Type blendTreeGraphGUIType;

	private static Type stateMachineGraphGUIType;

	private static Type stateMachineGraphType;

	private static Type edgeGUIType;

	private static bool repaintGraphRequested;

	private static bool rebuildGraphRequested;

	private static MethodInfo rebuildGraphMethod;

	private static bool insideGraphGui;

	private static object layerControllerView;

	private static Type layerControllerViewType;

	private static Type layerSettingsWindowType;

	private static FieldInfo layerScrollField;

	private static MethodInfo onRemoveLayerMethod;

	private static MethodInfo layerRenameEndMethod;

	private static MethodInfo showAtPositionMethod;

	private static FieldInfo layerListField;

	private static FieldInfo layerViewHostField;

	private static FieldInfo toolAnimatorControllerField;

	private static MethodInfo keyboardHandlingMethod;

	private static bool templateDropdownArmed = true;

	private static Vector2 categoryLayerScroll;

	private static UnityEditor.Animations.AnimatorController[] layerTemplateControllers;

	private static string[] layerTemplateNames;

	private static LayerViewViewType layerViewType = LayerViewViewType.DefaultView;

	private static LayerPathNode layerCategoryRoot;

	private static LayerPathNode currentLayerCategory;

	private static ReorderableList categoryLayerList;

	private static ReorderableList unityLayerList;

	private static string[] categoryNames;

	private static bool categoryViewDrewLayerList;

	private static bool frameLayerRequested;

	private static ReorderableList.ElementCallbackDelegate drawLayerCallback;

	private static ReorderableList.SelectCallbackDelegate selectLayerCallback;

	private static ReorderableList.SelectCallbackDelegate mouseUpLayerCallback;

	private static RenameOverlayWrapper layerRenameOverlay;

	private static RenameOverlayWrapper stateRenameOverlay;

	private static MethodInfo addBreadCrumbMethod;

	private static ConstructorInfo menuItemConstructor;

	private static GenericMenu contextMenu;

	private static Type stateMachineNodeBaseType;

	private static Type graphNodeType;

	private static Type blendTreeNodeType;

	private static Type edgeGUIPatchType;

	private static FieldInfo stateNodeStateField;

	private static FieldInfo blendTreeNodeMotionField;

	private static FieldInfo blendTreeNodeChildrenField;

	private static PropertyInfo blendTreeNodeParentProperty;

	private static MethodInfo entryNodeMakeTransitionCallback;

	private static MethodInfo anyStateNodeMakeTransitionCallback;

	private static MethodInfo stateNodeMakeTransitionCallback;

	private static MethodInfo stateMachineNodeMakeTransitionCallback;

	private static MethodInfo genericMenuForStateMachineNodeMethod;

	private static readonly MethodInfo m_ComparatorVisitor = HarmonyPatchManager.NewReg<AnimatorState>(QueryAlgo);

	private static Node slotDragSourceNode;

	private static bool slotDragActive;

	private static bool transitionDragArmed;

	private static bool transitionDragPending;

	private static bool slotDraggingEnded;

	private static AnimatorStateTransition placeholderTransition;

	private static AnimatorState placeholderTransitionTarget;

	private static Vector2 currentNodeSize;

	private static bool dragAndDropPending;

	private static AnimatorState quickToggleState;

	private static UnityEngine.AnimatorControllerParameter[] parameterViewParameters;

	private static FieldInfo parameterViewScrollField;

	private static Type parameterControllerViewType;

	private static int unusedNodeIndex;

	private static Vector2 categoryMenuMousePosition;

	private static Node pendingTransitionSourceNode;

	private static int pendingTransitionSourceKind;

	private static bool nodeContextClickPending;

	private static bool replaceTransitionsDefault;

	private static bool replaceTransitions;

	private static bool reverseModifiesValues;

	private static int contextLayerIndex;

	private static bool resumeTransitionDragAfterSlotDrag;

	private static AnimatorState blendTreeBreadcrumbState;

	private static UnityEditor.Animations.AnimatorController layerContextController;

	private static UnityEditor.Animations.AnimatorControllerLayer copiedLayer;

	private static MethodInfo findClosestEdgeMethod;

	private static MethodInfo advancedPopupMethod;

	private static MethodInfo playControlsOnGUIMethod;

	private static MethodInfo getBuiltinSkinMethod;

	private static void RepaintWindow()
	{
		if (activeWindow != null)
		{
			activeWindow.Repaint();
		}
	}

	[SpecialName]
	internal static UnityEditor.Animations.AnimatorController ActiveController()
	{
		if (!currentController)
		{
			InstantiateAnnotation();
		}
		return currentController;
	}

	[SpecialName]
	internal static void PatchMapper(UnityEditor.Animations.AnimatorController v)
	{
		if (currentController != v)
		{
			while (true)
			{
				currentController = v;
				DisableMapper();
			}
		}
	}

	[SpecialName]
	private static AnimatorStateMachine RootStateMachine()
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
	private static AnimatorStateMachine ActiveStateMachine()
	{
		if (!activeStateMachine)
		{
			RemoveAnnotation();
		}
		return activeStateMachine;
	}

	[SpecialName]
	private static void OrderInitializer(AnimatorStateMachine instance)
	{
		if (activeStateMachine != instance)
		{
			activeStateMachine = instance;
			RestartVisitor();
		}
	}

	[SpecialName]
	private static bool HasFocusedTransition()
	{
		return focusedTransition.transition != null;
	}

	[MenuItem("DreadTools/Controller Editor/Window %t", false, 200)]
	internal static void ShowWindow()
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
		thresholdControlCounter = 0;
		using (new ScrollViewScope(ref windowScroll))
		{
			using (new GUILayout.HorizontalScope())
			{
				EditorSettings.BoolSetting[] array = new EditorSettings.BoolSetting[3]
				{
					EditorSettings.GetInstance().editingTransitions,
					EditorSettings.GetInstance().editingStates,
					EditorSettings.GetInstance().editingController
				};
				string[] array2 = new string[3] { "Transitions", "States", "Controller" };
				for (int i = 0; i < 3; i++)
				{
					EditorGUI.BeginChangeCheck();
					array[i].SetValue(EditorUtils.ToggleButton(array[i], array2[i], EditorStyles.toolbarButton));
					if (EditorGUI.EndChangeCheck())
					{
						switch (i)
						{
						case 0:
							transitionSectionVisible = EditorSettings.GetInstance().editingTransitions;
							break;
						case 1:
							stateSectionVisible = EditorSettings.GetInstance().editingStates;
							break;
						}
					}
				}
			}
			EditorGUI.BeginDisabledGroup(!ActiveController());
			using (new GUILayout.VerticalScope(EditorUtils.styles().bigTitleBackground))
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(18f);
					if (!(ActiveStateMachine() != null))
					{
						GUILayout.Label("No Active Machine", EditorUtils.styles().centeredMiniLabel, GUILayout.ExpandWidth(expand: true));
					}
					else
					{
						GUILayout.Label(ActiveStateMachine().name, EditorUtils.styles().centeredMiniLabel, GUILayout.ExpandWidth(expand: true));
					}
					EditorGUI.EndDisabledGroup();
					if (EditorUtils.Button(EditorUtils.contents().inspectorWindow, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)) && EditorUtility.DisplayDialog("Instructions", "Open Controller Editor's Online Manual?", "Open", "Cancel"))
					{
						Application.OpenURL("https://notes.sleightly.dev/ceditor");
					}
					if (EditorUtils.Button(EditorUtils.contents().settings, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						ControllerEditorWindow.ShowWindow();
					}
					EditorGUI.BeginDisabledGroup(!ActiveController());
				}
				if ((bool)RootStateMachine() && exitTransitionNames.Length != 0)
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.FlexibleSpace();
						for (int j = 0; j < exitTransitionNames.Length; j++)
						{
							GUILayout.Label(exitTransitionNames[j], "AssetLabel");
						}
						GUILayout.FlexibleSpace();
					}
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorUtils.Separator();
			DrawTransitionSection();
			SeparatorIf(transitionSectionVisible && (stateSectionVisible || EditorSettings.GetInstance().editingController.GetValue()));
			DrawStateSection();
			SeparatorIf(stateSectionVisible && EditorSettings.GetInstance().editingController.GetValue());
			DrawControllerSection();
			DefineVisitor();
			HarmonyPatchManager.LoginReg();
		}
	}

	private static void SyncSelection()
	{
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		if ((bool)EditorSettings.GetInstance().aw_active && (bool)EditorSettings.GetInstance().aw_autoSwitchClip && Selection.activeObject is AnimatorState { motion: AnimationClip motion })
		{
			StartInitializer(motion);
		}
		stateRenameOverlay.EndRename(isconfig: true);
		IEnumerable<AnimatorGraphReflection.GraphNodeRef> enumerable = AnimatorGraphReflection.GraphAccessors.SelectedNodes();
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
		obj = emptyNodeSelection;
		goto IL_0073;
		IL_0073:
		selectedNodes = (List<AnimatorGraphReflection.GraphNodeRef>)obj;
		selectedEdges = AnimatorGraphReflection.GraphAccessors.SelectedEdges() ?? emptyEdgeSelection;
		selectedTransitionEdits = new List<AnimatorGraphReflection.TransitionEditionInfo>();
		foreach (AnimatorGraphReflection.GraphEdgeRef selectedEdge in selectedEdges)
		{
			selectedTransitionEdits.AddRange(selectedEdge.GetTransitions());
		}
		hasStateTransitionSelected = false;
		hasPlainTransitionSelected = false;
		bool flag = !makeMultipleTransitionsMode && selectedNodes.Any((AnimatorGraphReflection.GraphNodeRef nw) => nw.Node() == AnimatorGraphReflection.GraphAccessors.EntryNode().Node());
		bool flag2 = !makeMultipleTransitionsMode && selectedNodes.Any((AnimatorGraphReflection.GraphNodeRef nw) => nw.Node() == AnimatorGraphReflection.GraphAccessors.AnyStateNode().Node());
		entryNodeSelected |= flag;
		anyStateNodeSelected |= flag2;
		exitNodeSelected = selectedNodes.Any((AnimatorGraphReflection.GraphNodeRef nw) => nw.Node() == AnimatorGraphReflection.GraphAccessors.ExitNode().Node());
		bool flag3 = false;
		foreach (UnityEngine.Object item in Selection.objects.WhereNotNull())
		{
			if (HasFocusedTransition())
			{
				flag3 |= item == focusedTransition.transition;
			}
			Type type = item.GetType();
			if (hasStateTransitionSelected || !(type == typeof(AnimatorStateTransition)))
			{
				if (!hasPlainTransitionSelected && type == typeof(AnimatorTransition))
				{
					hasPlainTransitionSelected = true;
				}
			}
			else
			{
				hasStateTransitionSelected = true;
			}
			if (hasStateTransitionSelected && hasPlainTransitionSelected && (!HasFocusedTransition() || flag3))
			{
				break;
			}
		}
		if (!makeMultipleTransitionsMode)
		{
			if (!flag2)
			{
				anyStateNodeSelected = false;
			}
			if (!flag)
			{
				entryNodeSelected = false;
			}
		}
		if (!flag3)
		{
			focusedTransition = default(AnimatorGraphReflection.TransitionEditionInfo);
		}
		if (AnimatorGraphReflection.GraphAccessors.ExitNode() != null && AnimatorGraphReflection.GraphAccessors.ExitNode().Node() != null)
		{
			exitNodeIncomingTransitions = AnimatorGraphReflection.GraphAccessors.ExitNode().IncomingTransitions().ToArray();
		}
		AnimatorTransitionBase[] filtered = Selection.GetFiltered<AnimatorTransitionBase>(SelectionMode.Editable);
		selectedTransitions = selectedTransitionEdits.Select((AnimatorGraphReflection.TransitionEditionInfo t) => t.transition).ToList();
		sharedConditionEditors = AssetVisitor(selectedTransitions);
		AnimatorState[] filtered2 = Selection.GetFiltered<AnimatorState>(SelectionMode.Editable);
		selectedStates = filtered2.ToList();
		object obj2;
		if (selectedStates.Count > 0)
		{
			UnityEngine.Object[] objs = filtered2;
			obj2 = new SerializedObject(objs);
		}
		else
		{
			obj2 = null;
		}
		selectedStatesSerialized = (SerializedObject)obj2;
		selectedStateMachines = Selection.GetFiltered<AnimatorStateMachine>(SelectionMode.Editable);
		if (selectedTransitionEdits.Count == 0)
		{
			allConditionEditors.Clear();
			conditionEditorTransitions.Clear();
		}
		else
		{
			List<ConditionMultiEditor> list = new List<ConditionMultiEditor>();
			AnimatorTransitionBase[] array = conditionEditorTransitions.Where((AnimatorTransitionBase t) => !selectedTransitions.Contains(t)).ToArray();
			AnimatorTransitionBase[] array2 = array;
			foreach (AnimatorTransitionBase processReg in array2)
			{
				list.AddRange(allConditionEditors.Where((ConditionMultiEditor c) => c.targets[0].Item1 == processReg));
			}
			conditionEditorTransitions = conditionEditorTransitions.Except(array).ToList();
			allConditionEditors = allConditionEditors.Except(list).ToList();
			foreach (AnimatorTransitionBase item2 in selectedTransitions.Where((AnimatorTransitionBase t) => !conditionEditorTransitions.Contains(t)))
			{
				conditionEditorTransitions.Add(item2);
				for (int num2 = 0; num2 < item2.conditions.Length; num2++)
				{
					allConditionEditors.Add(new ConditionMultiEditor(item2, num2));
				}
			}
		}
		MapVisitor();
		selectedStateTransitions = Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.Editable).ToList();
		RunAlgo();
		CalculateAnnotation();
		if (!redirectTransitionsMode && !replicateTransitionsMode)
		{
			pendingTransitionEdits = selectedTransitionEdits.ToList();
		}
		if (!makeMultipleTransitionsMode)
		{
			multiTransitionStates = selectedStates;
			multiTransitionStateMachines = selectedStateMachines;
		}
		ConnectAnnotation();
		if (AnimatorTypeCache.IsVRCSDKAvailable())
		{
			CallAnnotation();
			MoveAnnotation();
		}
		transitionSectionVisible = (bool)EditorSettings.GetInstance().editingTransitions || filtered.Length != 0;
		stateSectionVisible = (bool)EditorSettings.GetInstance().editingStates || selectedStates.Count > 0;
		RepaintWindow();
	}

	private void OnFocus()
	{
		SyncSelection();
	}

	private void PrintWrapper()
	{
		UpdateVisitor();
		Repaint();
	}

	private void OnDisable()
	{
		makeMultipleTransitionsMode = false;
		redirectTransitionsMode = false;
		replicateTransitionsMode = false;
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
	}

	private void SearchWrapper()
	{
		makeMultipleTransitionsMode = false;
		redirectTransitionsMode = false;
		replicateTransitionsMode = false;
	}

	private void OnEnable()
	{
		activeWindow = this;
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(PrintWrapper));
		EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(ApplyGraphBackground));
		EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(ApplyGraphBackground));
		if (mixedValueTransitionPair == null)
		{
			mixedValueTransitionPair = new AnimatorStateTransition[2]
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
			UnityEngine.Object[] objs = mixedValueTransitionPair;
			mixedValueTransitionSerialized = new SerializedObject(objs);
		}
		transitionInspectorSerialized = mixedValueTransitionSerialized;
		CalculateAnnotation();
		MapVisitor();
		CancelAnnotation();
	}

	private static void RevertWrapper()
	{
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(SyncSelection));
			Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(SyncSelection));
			OrderAnnotation();
			CompareAnnotation();
			EditorUtils.DelayCallOnHierarchyGui(SetAnnotation);
			SyncSelection();
			PublishAnnotation();
			rebuildGraphRequested = true;
		}
	}

	private static void OrderAnnotation()
	{
		try
		{
			PrimeAnimatorToolReflection();
			PrimeGraphNodeReflection();
			PrimeLayerControllerViewReflection();
			if (((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			})())
			{
				ValidateMapper();
				PrimeAnimationWindowReflection();
				PrimeMenuAndLayerEditorReflection();
				PrimeGraphStyleReflection();
			}
		}
		catch (Exception exception)
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
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
		if (unityNodeStyleCache == null)
		{
			unityNodeStyleCache = (Dictionary<string, GUIStyle>)EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetAnyField("m_NodeStyleCache").GetValue(null);
		}
		unityNodeStyleCache.Clear();
		stateStylesByTag.Clear();
		cosmeticOnlyStyleNames.Clear();
		CS_0024_003C_003E8__locals15.m_ObserverTests = new GUIStyle[4] { "flow node 0", "flow node 0 on", "flow node 5", "flow node 5 on" };
		defaultStateNodeStyle = new GUIStyle(CS_0024_003C_003E8__locals15.m_ObserverTests[0])
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
				scaledBackgrounds = new Texture2D[1] { EditorUtils.SharedColorTexture(Color.black) }
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
				scaledBackgrounds = new Texture2D[1] { EditorUtils.SharedColorTexture(Color.black) }
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
		styleMenuNames.Sort();
		rebuildGraphRequested = true;
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
				unityNodeStyleCache[stringBuilder.ToString()] = vis;
			}
		}
	}

	private static void PublishAnnotation()
	{
		if (unityLayerList != null)
		{
			unityLayerList.elementHeight = CloneInitializer();
		}
	}

	public void AddItemsToMenu(GenericMenu i)
	{
		i.AddItem(new GUIContent("Instructions"), on: false, delegate
		{
			Application.OpenURL("https://notes.sleightly.dev/controllereditor/");
		});
		i.AddSeparator(string.Empty);
		i.AddItem(new GUIContent("Legacy Dropdown"), EditorSettings.GetInstance().useLegacyDropdown, delegate
		{
			EditorSettings.GetInstance().useLegacyDropdown.Toggle();
		});
		i.AddSeparator(string.Empty);
		i.AddItem(new GUIContent("Settings"), on: false, ControllerEditorWindow.ShowWindow);
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

	private static List<BehaviourPropertyMultiEditor> PopAnnotation(AnimatorTypeCache.ParameterDriverBinding init, List<BehaviourPropertyMultiEditor> cust = null)
	{
		List<BehaviourPropertyMultiEditor> list = cust;
		if (list != null)
		{
			for (int i = 0; i < init.parameters.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (!list[j].matched && ComputeAnnotation(init.parameters[i], list[j].entry))
					{
						list[j].AddMatch(init, i);
						break;
					}
				}
			}
			list = list.Where((BehaviourPropertyMultiEditor s) => s.matched).ToList();
			for (int num = 0; num < list.Count; num++)
			{
				list[num].matched = false;
			}
		}
		else
		{
			list = new List<BehaviourPropertyMultiEditor>();
			for (int num2 = 0; num2 < init.parameters.Count; num2++)
			{
				list.Add(new BehaviourPropertyMultiEditor(init, num2));
			}
		}
		return list;
	}

	private static bool ComputeAnnotation(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry i, AnimatorTypeCache.ParameterDriverBinding.ParameterEntry pol)
	{
		if (!ActiveController())
		{
			return false;
		}
		if (!(i.GetName() != pol.GetName()))
		{
			if (i.GetChangeType() != pol.GetChangeType())
			{
				return false;
			}
			int second;
			UnityEngine.AnimatorControllerParameterType type = ResetAnnotation(i.GetName(), out second).type;
			if (i.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set || i.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Add)
			{
				if (type == UnityEngine.AnimatorControllerParameterType.Trigger)
				{
					return true;
				}
				if (i.GetValue() != pol.GetValue())
				{
					return false;
				}
			}
			else if (type == UnityEngine.AnimatorControllerParameterType.Bool || type == UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				if (i.GetChance() != pol.GetChance())
				{
					return false;
				}
			}
			else if (i.GetValueMin() != pol.GetValueMin() || i.GetValueMax() != pol.GetValueMax())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private static void MoveAnnotation()
	{
		allStatesHaveTrackingControl = true;
		List<StateMachineBehaviour> list = new List<StateMachineBehaviour>();
		foreach (AnimatorState selectedState in selectedStates)
		{
			bool flag = false;
			StateMachineBehaviour[] behaviours = selectedState.behaviours;
			foreach (StateMachineBehaviour stateMachineBehaviour in behaviours)
			{
				if (stateMachineBehaviour.GetType() == AnimatorTypeCache.GetTrackingControlType())
				{
					flag = true;
					list.Add(stateMachineBehaviour);
				}
			}
			if (!flag)
			{
				allStatesHaveTrackingControl = false;
				break;
			}
		}
		allStatesHaveTrackingControl = allStatesHaveTrackingControl && list.Count > 0;
		if (allStatesHaveTrackingControl)
		{
			trackingControlEditor = new TrackingControlEditor(list.ToArray());
		}
	}

	private static void ConcatAnnotation()
	{
		foreach (AnimatorState selectedState in selectedStates)
		{
			if (selectedState.behaviours.All((StateMachineBehaviour b) => b.GetType() != AnimatorTypeCache.GetTrackingControlType()))
			{
				selectedState.AddStateMachineBehaviour(AnimatorTypeCache.GetTrackingControlType());
			}
		}
		MoveAnnotation();
	}

	private static void CallAnnotation()
	{
		parameterDriverEditors = null;
		parameterDriverBindings.Clear();
		for (int i = 0; i < selectedStates.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < selectedStates[i].behaviours.Length; j++)
			{
				if (selectedStates[i].behaviours[j].GetType() == AnimatorTypeCache.GetParameterDriverType())
				{
					AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(selectedStates[i].behaviours[j]);
					parameterDriverBindings.Add(parameterDriverBinding);
					parameterDriverEditors = PopAnnotation(parameterDriverBinding, parameterDriverEditors);
					flag = true;
				}
			}
			if (!flag)
			{
				if (parameterDriverEditors != null)
				{
					parameterDriverEditors.Clear();
				}
				break;
			}
		}
		if (parameterDriverEditors == null)
		{
			parameterDriverEditors = new List<BehaviourPropertyMultiEditor>();
		}
		CancelAnnotation();
	}

	private static void CancelAnnotation()
	{
		parameterDriverList = new ReorderableList(parameterDriverEditors, typeof(BehaviourPropertyMultiEditor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
		{
			drawElementCallback = CountAnnotation,
			drawHeaderCallback = DisableAnnotation,
			onAddCallback = InsertAnnotation
		};
	}

	private static void CountAnnotation(Rect ident, int cust_end, bool rejectserv, bool bool_0)
	{
		if (!ActiveController() || cust_end >= parameterDriverEditors.Count || cust_end < 0)
		{
			return;
		}
		AnimatorTypeCache.ParameterDriverBinding.ParameterEntry entry = parameterDriverEditors[cust_end].entry;
		int second;
		UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(entry.GetName(), out second);
		Rect source = new Rect(ident);
		Rect rect = new Rect(ident.width - 29f, ident.y + 2f, 32f, 18f);
		source.width -= 42f;
		source.y += 2f;
		source.width /= 2f;
		EditorGUI.BeginChangeCheck();
		bool flag = false;
		if (animatorControllerParameter != null)
		{
			entry.SetName(TestAnnotation(EditorGUI.Popup(source, second, parameterNames)));
			source.x += source.width;
			if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger || entry.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set || (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Float && animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int) || entry.GetChangeType() != AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Random)
			{
				if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger || entry.GetChangeType() != AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set)
				{
					source.width /= 2f;
				}
			}
			else
			{
				source.width /= 3f;
			}
			entry.SetChangeType((AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType)(object)EditorGUI.EnumPopup(selected: (Enum)((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool && animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger) ? ((object)(VRCFullOptions)entry.GetChangeType()) : ((object)(VRCHalfOptions)entry.GetChangeType())), position: source));
			source.x += source.width;
			source.height -= 5f;
			if (animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Bool || animatorControllerParameter.type == UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				if (entry.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Add)
				{
					entry.SetChangeType(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set);
				}
				if (entry.GetChangeType() != AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set)
				{
					EditorGUIUtility.labelWidth = 50f;
					source.width -= 17f;
					entry.SetChance(EditorGUI.Slider(source, "Chance", entry.GetChance() * 100f, 0f, 100f) / 100f);
					source.x += source.width;
					GUI.Label(source, "%", "boldlabel");
					EditorGUIUtility.labelWidth = 0f;
				}
				else
				{
					entry.SetValue(Mathf.Clamp((int)entry.GetValue(), 0, 1));
					Enum selected = ((entry.GetValue() == 1f) ? BoolModes.True : BoolModes.False);
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
					{
						entry.SetValue(((BoolModes)(object)EditorGUI.EnumPopup(source, selected) == BoolModes.True) ? 1 : 0);
					}
				}
			}
			else
			{
				bool flag2 = entry.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Add;
				if (entry.GetChangeType() == AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType.Set || flag2)
				{
					EditorGUIUtility.labelWidth = 37f;
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						entry.SetValue(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Value"), entry.GetValue()), -1f, 1f));
					}
					else
					{
						entry.SetValue(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Value"), (int)entry.GetValue()), flag2 ? (-255) : 0, 255));
					}
					EditorGUIUtility.labelWidth = 0f;
				}
				else
				{
					EditorGUIUtility.labelWidth = 27f;
					if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Int)
					{
						entry.SetValueMin(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Min"), entry.GetValueMin()), -1f, 1f));
						source.x += source.width;
						entry.SetValueMax(Mathf.Clamp(EditorGUI.FloatField(source, new GUIContent("Max"), entry.GetValueMax()), entry.GetValueMin(), 1f));
					}
					else
					{
						entry.SetValueMin(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Min"), (int)entry.GetValueMin()), 0, 255));
						source.x += source.width;
						entry.SetValueMax(Mathf.Clamp(EditorGUI.IntField(source, new GUIContent("Max"), (int)entry.GetValueMax()), entry.GetValueMin(), 255f));
					}
					EditorGUIUtility.labelWidth = 0f;
				}
			}
		}
		else
		{
			EditorGUI.BeginChangeCheck();
			int firstsize = EditorGUI.Popup(source, -1, parameterNames);
			if (EditorGUI.EndChangeCheck())
			{
				entry.SetName(TestAnnotation(firstsize));
			}
			GUI.Label(new Rect(source)
			{
				width = source.width - 5f,
				x = source.x + 5f
			}, entry.GetName(), "minilabel");
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
				string item = ((!string.IsNullOrEmpty(entry.GetName())) ? entry.GetName() : "New Parameter");
				ActiveController().AddParameter(item, (UnityEngine.AnimatorControllerParameterType)num);
				List<string> list = parameterNames.ToList();
				list.Add(item);
				parameterNames = list.ToArray();
				entry.SetName(item);
				flag = true;
			}
		}
		if (EditorGUI.EndChangeCheck() || flag)
		{
			parameterDriverEditors[cust_end].ApplyToAll(entry);
			parameterDriverEditors[cust_end].entry = entry;
		}
		if (GUI.Button(rect, EditorUtils.contents().removeCondition, EditorUtils.styles().footerButton))
		{
			parameterDriverEditors[cust_end].RemoveFromAll();
			parameterDriverEditors.RemoveAt(cust_end);
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
		foreach (AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding in parameterDriverBindings)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					continue;
				case 3:
					if (!parameterDriverBinding.GetLocalOnly())
					{
						num = 2;
					}
					break;
				case 2:
					break;
				case 1:
					if (parameterDriverBinding.GetLocalOnly())
					{
						num = 2;
					}
					break;
				case 0:
					if (parameterDriverBinding.GetLocalOnly())
					{
						num = 3;
					}
					else if (!parameterDriverBinding.GetLocalOnly())
					{
						num = 1;
					}
					break;
				}
				break;
			}
			if (num == 2)
			{
				break;
			}
		}
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, num, Color.grey, Color.red, Color.yellow, Color.green))
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
				foreach (AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding2 in parameterDriverBindings)
				{
					parameterDriverBinding2.SetLocalOnly(instanceinstall: true);
				}
				break;
			}
			case 3:
			{
				foreach (AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding3 in parameterDriverBindings)
				{
					parameterDriverBinding3.SetLocalOnly(instanceinstall: false);
				}
				break;
			}
			}
		}
	}

	private static void InsertAnnotation(ReorderableList spec)
	{
		AnimatorTypeCache.ParameterDriverBinding.ParameterEntry parameterEntry = null;
		string text = "";
		if (parameterDriverEditors.Count <= 0)
		{
			if ((bool)ActiveController() && ActiveController().parameters.Length != 0)
			{
				text = TestAnnotation(0);
			}
		}
		else
		{
			parameterEntry = parameterDriverEditors.Last().entry;
		}
		for (int i = 0; i < selectedStates.Count; i++)
		{
			AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = null;
			for (int j = 0; j < selectedStates[i].behaviours.Length; j++)
			{
				if (selectedStates[i].behaviours[j].GetType() == AnimatorTypeCache.GetParameterDriverType())
				{
					parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(selectedStates[i].behaviours[j]);
					break;
				}
			}
			if (parameterDriverBinding == null)
			{
				StateMachineBehaviour stateMachineBehaviour = (StateMachineBehaviour)ScriptableObject.CreateInstance(AnimatorTypeCache.GetParameterDriverType());
				stateMachineBehaviour.hideFlags = HideFlags.HideInHierarchy;
				AssetDatabase.AddObjectToAsset(stateMachineBehaviour, ActiveController());
				StateMachineBehaviour[] array = selectedStates[i].behaviours;
				ArrayUtility.Add(ref array, stateMachineBehaviour);
				selectedStates[i].behaviours = array;
				parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour);
			}
			AnimatorTypeCache.ParameterDriverBinding.ParameterEntry parameterEntry2 = parameterDriverBinding.AddParameter();
			if (parameterEntry != null)
			{
				RestartAnnotation(parameterEntry, parameterEntry2);
			}
			else
			{
				parameterEntry2.SetName(text);
			}
		}
		CallAnnotation();
		RepaintWindow();
	}

	private static void RestartAnnotation(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry def, AnimatorTypeCache.ParameterDriverBinding.ParameterEntry vis)
	{
		vis.SetDeferApply(validatekey: true);
		vis.SetChance(def.GetChance());
		vis.SetName(def.GetName());
		vis.SetValue(def.GetValue());
		vis.SetChangeType(def.GetChangeType());
		vis.SetValueMin(def.GetValueMin());
		vis.SetValueMax(def.GetValueMax());
		vis.SetDeferApply(validatekey: false);
	}

	[SpecialName]
	private static int SetupInitializer()
	{
		return (int)selectedLayerIndexProperty.GetValue(ReadAnnotation());
	}

	[SpecialName]
	private static void EnableInitializer(int length_setup)
	{
		selectedLayerIndexProperty.SetValue(ReadAnnotation(), length_setup);
	}

	private static bool AddAnnotation()
	{
		EditorWindow editorWindow = AnimatorGraphReflection.GraphAccessors.Tool();
		if (!(editorWindow != null))
		{
			return false;
		}
		return (bool)liveLinkProperty.GetValue(editorWindow);
	}

	private static Animator InvokeAnnotation()
	{
		EditorWindow editorWindow = AnimatorGraphReflection.GraphAccessors.Tool();
		if (!(editorWindow == null))
		{
			return (Animator)previewAnimatorField.GetValue(editorWindow);
		}
		return null;
	}

	private static object FindAnnotation()
	{
		object obj = AnimatorGraphReflection.GraphAccessors.Tool();
		if (obj == null)
		{
			return null;
		}
		return activeGraphGUIGetter.Invoke(obj, null);
	}

	private static object ExcludeAnnotation()
	{
		return Traverse.Create(FindAnnotation()).Property("edgeGUI").GetValue();
	}

	private static Vector3[] InitAnnotation(Edge last)
	{
		return (Vector3[])getEdgePointsMethod.Invoke(ExcludeAnnotation(), new object[1] { last });
	}

	private static object VisitAnnotation()
	{
		if (!ActiveController())
		{
			return null;
		}
		if ((object)AnimatorGraphReflection.GraphAccessors.Tool() != null)
		{
			return AnimatorGraphReflection.GraphAccessors.ActiveGraphGUI();
		}
		return null;
	}

	private static void DefineAnnotation()
	{
		if ((bool)ActiveController())
		{
			PrintMapper(AnimatorGraphReflection.GraphAccessors.RootStateMachine());
		}
	}

	private static object StartAnnotation()
	{
		object obj = VisitAnnotation();
		if (obj != null)
		{
			return stateMachineGraphGUIType.GetMethod("get_stateMachineGraph", BindingFlags.Instance | BindingFlags.Public)?.Invoke(obj, null);
		}
		return null;
	}

	private static object ReadAnnotation()
	{
		EditorWindow editorWindow = AnimatorGraphReflection.GraphAccessors.Tool();
		if (editorWindow == null)
		{
			return null;
		}
		object obj = layerEditorField.GetValue(editorWindow);
		if (obj == null)
		{
			layerEditorField.SetValue(editorWindow, obj = Activator.CreateInstance(layerControllerViewType));
		}
		return obj;
	}

	private static ReorderableList SelectAnnotation()
	{
		object obj = ReadAnnotation();
		if (obj != null)
		{
			return (ReorderableList)layerListField.GetValue(obj);
		}
		return null;
	}

	private static void RemoveAnnotation()
	{
		OrderInitializer(AnimatorGraphReflection.GraphAccessors.ActiveStateMachine());
	}

	private static void InstantiateAnnotation()
	{
		if ((object)AnimatorGraphReflection.GraphAccessors.Tool() != null)
		{
			PatchMapper(AnimatorGraphReflection.GraphAccessors.AnimatorController());
		}
	}

	private static UnityEngine.AnimatorControllerParameter AwakeAnnotation(string config)
	{
		int second;
		return ResetAnnotation(config, out second);
	}

	private static UnityEngine.AnimatorControllerParameter ResetAnnotation(string spec, out int second)
	{
		if ((bool)ActiveController())
		{
			UnityEngine.AnimatorControllerParameter[] parameters = ActiveController().parameters;
			if (!parameters.TryFindIndex((UnityEngine.AnimatorControllerParameter p) => p.name == spec, out second))
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
		exitTransitionNames = (from t in RootStateMachine().anyStateTransitions
			where t.isExit
			select t.name).ToArray();
	}

	private static void ConnectAnnotation()
	{
		if (!ActiveController())
		{
			return;
		}
		UnityEngine.AnimatorControllerParameter[] parameters = ActiveController().parameters;
		parameterNames = new string[parameters.Length];
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < parameters.Length; i++)
		{
			parameterNames[i] = parameters[i].name;
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
		floatParameterNames = list.ToArray();
		boolParameterNames = list2.ToArray();
	}

	private static void CalculateAnnotation()
	{
		LoginVisitor();
		PushVisitor();
	}

	private static string TestAnnotation(int firstsize)
	{
		return ActiveController().parameters[firstsize].name;
	}

	private static string MapAnnotation(string ident, Func<string, bool> caller)
	{
		if (EditorUtils.reservedAvatarParameters.Contains(ident))
		{
			return ident;
		}
		return EditorUtils.SortRules(ident, caller);
	}

	private static void ValidateAnnotation(Action<object[]> key)
	{
		QuickInputWindow quickInputWindow = QuickInputWindow.Create("Animator QuickInput", new QuickInputWindow.FieldType[3]
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
		quickInputWindow.SetValue(0, ControllerEditorWindow.targetAnimator ? ControllerEditorWindow.targetAnimator : UnityEngine.Object.FindObjectsOfType<Animator>().FirstOrDefault((Animator a) => a.avatar));
		quickInputWindow.SetObjectType(0, typeof(Animator));
		quickInputWindow.SetValue(1, true);
		quickInputWindow.rowToggles = new bool[3] { false, true, false };
		quickInputWindow.ShowAt(categoryMenuMousePosition);
	}

	internal static void CustomizeAnnotation(string config)
	{
		Log(config);
	}

	internal static bool RateAnnotation(bool isi, string selection)
	{
		return Log(selection, CustomLogType.Warning, isi);
	}

	internal static void DestroyAnnotation(string task)
	{
		Log(task, CustomLogType.Error);
	}

	private static void GetAnnotation(bool updatereference)
	{
		using (new EditorSettings.SettingsChangeScope())
		{
			EditorSettings.BoolSetting editingTransitions = EditorSettings.GetInstance().editingTransitions;
			EditorSettings.BoolSetting editingStates = EditorSettings.GetInstance().editingStates;
			EditorSettings.BoolSetting editingController = EditorSettings.GetInstance().editingController;
			EditorSettings.BoolSetting showTransitionSettings = EditorSettings.GetInstance().showTransitionSettings;
			EditorSettings.BoolSetting showTransitionConditions = EditorSettings.GetInstance().showTransitionConditions;
			EditorSettings.BoolSetting showTransitionsCount = EditorSettings.GetInstance().showTransitionsCount;
			EditorSettings.BoolSetting showStateCount = EditorSettings.GetInstance().showStateCount;
			EditorSettings.BoolSetting showStateSettings = EditorSettings.GetInstance().showStateSettings;
			EditorSettings.BoolSetting showVRCDrivers = EditorSettings.GetInstance().showVRCDrivers;
			bool flag;
			EditorSettings.GetInstance().showVRCTracking.SetValue(flag = updatereference);
			bool flag2;
			showVRCDrivers.SetValue(flag2 = flag);
			bool flag3;
			showStateSettings.SetValue(flag3 = flag2);
			bool flag4;
			showStateCount.SetValue(flag4 = flag3);
			bool flag5;
			showTransitionsCount.SetValue(flag5 = flag4);
			bool flag6;
			showTransitionConditions.SetValue(flag6 = flag5);
			bool flag7;
			showTransitionSettings.SetValue(flag7 = flag6);
			bool flag8;
			editingController.SetValue(flag8 = flag7);
			bool value;
			editingStates.SetValue(value = flag8);
			editingTransitions.SetValue(value);
		}
	}

	internal static void CalcAnnotation(Rect param, string second)
	{
		if ((bool)EditorSettings.GetInstance().displayParameterType)
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.FG, EditorSettings.GetInstance().parameterLabelColor.GetValue()))
			{
				EditorUtils.ConnectQueue(param, second, overridethird: true, 0f, 1f, isparam4: false, EditorSettings.parameterLabelStyle);
			}
		}
	}

	internal static void SeparatorIf(bool calci)
	{
		if (calci)
		{
			EditorUtils.Separator();
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
		using (new GUILayout.HorizontalScope(EditorUtils.styles().bigTitleBackground))
		{
			GUILayout.FlexibleSpace();
			GUILayout.Label(value, EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
		}
	}

	private static void DrawCollapsibleSection(Action def, string token, EditorSettings.BoolSetting res, bool iscont2, int visitor3counter)
	{
		if (!res)
		{
			if (EditorUtils.Button(token, EditorStyles.toolbarButton))
			{
				res.Toggle();
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
			EditorUtils.CompareQueue(text, GUILayoutUtility.GetLastRect().height, EventType.Repaint);
			if (EditorUtils.Button(string.Empty, GUILayout.Height(EditorUtils.OrderQueue(text, 0f)), GUILayout.Width(7f)))
			{
				res.Toggle();
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
		bool num = EditorUtils.Button(map, flag ? EditorUtils.styles().linkLabel : GUI.skin.label, options);
		if (num)
		{
			Selection.activeObject = (flag ? null : v);
		}
		return num;
	}

	private static void ResolveAnnotation()
	{
		List<(MethodInfo, CallbackAttribute, bool)> list = new List<(MethodInfo, CallbackAttribute, bool)>();
		foreach (MethodInfo item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((System.Reflection.Assembly assembly) => assembly.GetTypes().SelectMany((Type t) => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))))
		{
			CallbackAttribute[] array = item.GetCustomAttributes<CallbackAttribute>().ToArray();
			foreach (CallbackAttribute callbackAttribute in array)
			{
				if (callbackAttribute is CallbackMethodAttribute)
				{
					list.Add((item, callbackAttribute, true));
				}
				else if (callbackAttribute is ControllerCallbackAttribute)
				{
					list.Add((item, callbackAttribute, false));
				}
			}
		}
		foreach (var item2 in list.OrderBy<(MethodInfo, CallbackAttribute, bool), int>(((MethodInfo, CallbackAttribute, bool onVerify) x) => x.Item2.priority))
		{
			var (m_ComparatorDefinition, exceptionDefinition, _) = item2;
			if (item2.Item3)
			{
				InterruptAnnotation(delegate
				{
					m_ComparatorDefinition.Invoke(null, exceptionDefinition.args);
				});
			}
			else
			{
				PrintAnnotation(delegate
				{
					m_ComparatorDefinition.Invoke(null, exceptionDefinition.args);
				});
			}
		}
	}

	[SpecialName]
	private static void ComputeInitializer(bool ignoresetup)
	{
		bool flag = bugReporterOpen;
		bugReporterOpen = ignoresetup;
		if (!bugReporterOpen && flag)
		{
			BugReporter.EnableReg(null);
		}
	}

	private static void ListAnnotation()
	{
		SetVisitor("Send Feedback for Controller Editor", "If you have a suggestion, preference, or something to comment, please send it here!\nNote that the feedback is not anonymous. Abuse may result in blacklisting.");
		feedbackPanelOpen = isLicensed;
		feedbackText = EditorGUILayout.TextArea(feedbackText, GUILayout.MinHeight(54f));
		using (new GUILayout.HorizontalScope())
		{
			if (EditorUtils.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
			{
				feedbackPanelOpen = false;
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(feedbackText) || isSendingFeedback))
			{
				if (EditorUtils.Button("Send Feedback", EditorStyles.toolbarButton))
				{
					if (feedbackText.Length > 2000)
					{
						feedbackText = feedbackText.Substring(0, 2000);
					}
					List<(string, string)> list = RegisterAnnotation("sendfeedback", new(string, string)[1] { ("feedback", Uri.EscapeUriString(feedbackText)) });
					LogoutAnnotation(list);
					isSendingFeedback = true;
					DisableVisitor(CallVisitor(list.ToArray())).QueryRules(ChangeAnnotation, UnityEngine.Debug.LogException, null, null, delegate
					{
						isSendingFeedback = false;
						feedbackPanelOpen = false;
						DrawLicenseInfo();
					});
				}
			}
		}
	}

	[SpecialName]
	private static float ConcatInitializer()
	{
		return retryAllowedAtRealtime - Time.realtimeSinceStartup;
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
		if (!EditorSettings.GetInstance().a_HasSucceededLastVerification)
		{
			licenseKeyEntryRequired = true;
			licenseCheckedThisSession = flag;
		}
		if (flag && (bool)EditorSettings.GetInstance().a_VerifyOnProjectLoad)
		{
			EditorUtils.DelayCall(delegate
			{
				WriteAnnotation(assetneeded: false);
			});
		}
	}

	private static void FillAnnotation()
	{
		if (!licenseCheckedThisSession && (bool)EditorSettings.GetInstance().a_VerifyOnDisplay && AssetAnnotation())
		{
			WriteAnnotation(assetneeded: false);
		}
	}

	private static void WriteAnnotation(bool assetneeded)
	{
		_003C_003Ec__DisplayClass186_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass186_0();
		if ((!EditorSettings.GetInstance().a_VerifyOnDisplay.GetValue() && !EditorSettings.GetInstance().a_VerifyOnProjectLoad.GetValue() && !assetneeded) || (licenseKeyEntryRequired && !licenseCheckRetryOffered) || isVerifyingLicense)
		{
			return;
		}
		licenseCheckRetryOffered = false;
		isVerifyingLicense = true;
		licenseCheckedThisSession = true;
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
							licenseUsername = CS_0024_003C_003E8__locals9.RunObserver("u", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							licenseVariant = CS_0024_003C_003E8__locals9.RunObserver("v", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							licenseToken = CS_0024_003C_003E8__locals9.RunObserver("r", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							hardwareId = CS_0024_003C_003E8__locals9.RunObserver("m", ref _003C_003Ec__DisplayClass186_1_, ref _003C_003Ec__DisplayClass186_2_);
							StopAnnotation();
							CheckAnnotation();
							isLicensed = true;
							licenseKeyEntryRequired = true;
							isVerifyingLicense = false;
							licenseRestoredFromCache = true;
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
			Log("failed to verify from cache.", CustomLogType.Warning);
		}
		if (licenseRestoredFromCache)
		{
			ManageAnnotation(applyident: true);
			RestartVisitor();
		}
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("verifylicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass186_3 _003C_003Ec__DisplayClass186_ = new _003C_003Ec__DisplayClass186_3();
				_003C_003Ec__DisplayClass186_.m_MerchantDefinition = CS_0024_003C_003E8__locals9;
				_003C_003Ec__DisplayClass186_.valueDefinition = response;
				isVerifyingLicense = false;
				licenseKeyEntryRequired = true;
				SortAnnotation(_003C_003Ec__DisplayClass186_.valueDefinition, _003C_003Ec__DisplayClass186_.DeleteObserver, SearchAnnotation, t2stop: false);
			}, _003C_003Ec.watcherInitializer.CollectProperty, null, null, DrawLicenseInfo);
		}, iscont: true);
	}

	private static void ForgotAnnotation()
	{
		isActivatingLicense = true;
		if (!SaveLicenseInfo())
		{
			Log("Invalid License Key!", CustomLogType.Error);
			return;
		}
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("activatelicense");
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				isActivatingLicense = false;
				SortAnnotation(response, delegate
				{
					licenseKeyEntryRequired = false;
					EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: true);
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(Exception exception)
			{
				isActivatingLicense = false;
				Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
			}, null, null, DrawLicenseInfo);
		}, iscont: true);
	}

	private static void StopAnnotation()
	{
		licensedToDisplayName = licenseUsername;
		if (string.IsNullOrWhiteSpace(licensedToDisplayName))
		{
			return;
		}
		try
		{
			Match match = Regex.Match(licensedToDisplayName, "(?i)(?:<color=#(?:[0-9a-f]{8}|[0-9a-f]{6})>)?.*?(#\\d{4})(?:<\\/color>)?$");
			if (match.Success)
			{
				licensedToDisplayName = licensedToDisplayName.Remove(match.Groups[1].Index, match.Groups[1].Length);
			}
			if (licensedToDisplayName.Length > 1 && licensedToDisplayName[0] == '@')
			{
				licensedToDisplayName = licensedToDisplayName.Substring(1);
			}
		}
		catch
		{
		}
	}

	private static void CheckAnnotation()
	{
		string[] array = hardwareId.Split(new char[1] { '-' });
		string[] array2 = PatchAnnotation().Split(new char[1] { '/' });
		array2[2] = array2[2].Substring(2, 2);
		unreadDeviceDateFingerprint = array2[2] + array[0].Substring(0, 10) + array2[1] + array[2].Substring(0, 10) + array2[0];
	}

	private static void PrepareAnnotation()
	{
		if (string.IsNullOrWhiteSpace(sessionId))
		{
			string key = "DreadScriptssid";
			sessionId = EditorPrefs.GetString(key, string.Empty);
			if (string.IsNullOrWhiteSpace(sessionId) || !Regex.IsMatch(sessionId, "[0-9a-f]{32}"))
			{
				sessionId = GUID.Generate().ToString();
				EditorPrefs.SetString(key, sessionId);
			}
		}
	}

	private static bool AssetAnnotation()
	{
		if (!string.IsNullOrWhiteSpace(licenseKey))
		{
			return true;
		}
		licenseKey = EditorPrefs.GetString("yOk0XCnENLMO6DIF8cYpSg==LK", string.Empty);
		if (!EnableVisitor())
		{
			licenseKey = string.Empty;
		}
		return !(licenseKeyEntryRequired = string.IsNullOrWhiteSpace(licenseKey));
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
		ProcessRunner[] key = new ProcessRunner[4]
		{
			new ProcessRunner("wmic baseboard get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[0] = o;
			}, isfield: true),
			new ProcessRunner("wmic cpu get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[1] = o;
			}, isfield: true),
			new ProcessRunner("wmic diskdrive get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[2] = o;
			}, isfield: true),
			new ProcessRunner("wmic memorychip get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.repositoryDefinition[3] = o;
			}, isfield: true)
		};
		CS_0024_003C_003E8__locals31.instanceDefinition = new ProcessRunner[4]
		{
			new ProcessRunner("Get-CimInstance -class Win32_baseboard | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[0] = o;
			}),
			new ProcessRunner("Get-CimInstance -class Win32_processor | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[1] = o;
			}),
			new ProcessRunner("Get-CimInstance -class Win32_diskdrive | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.mappingDefinition[2] = o;
			}),
			new ProcessRunner("Get-CimInstance -class win32_physicalmemory | Select *", delegate(string o)
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
			catch (Exception exc)
			{
				CS_0024_003C_003E8__locals31.ChangeObserver(isCMD: true, exc);
			}
		}, CS_0024_003C_003E8__locals31._MockDefinition);
	}

	private static void ChangeAnnotation(JsonObject v)
	{
		SortAnnotation(v, null);
	}

	private static void SortAnnotation(JsonObject key, Action token, Action serv = null, bool t2stop = true)
	{
		bool num = key.Item("success");
		string text = key.Item("message");
		string text2 = key.Item("url");
		bool flag = !string.IsNullOrEmpty(text2);
		string text3 = key.Item("url_name");
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
			bool flag2 = key.Item("wait_warn");
			float num2 = key.Item("wait_time");
			serverWarnedTooManyAttempts |= flag2;
			if (!(num2 <= 0f))
			{
				retryAllowedAtRealtime = Time.realtimeSinceStartup + num2;
			}
			serv?.Invoke();
			if (!string.IsNullOrEmpty(text))
			{
				Log(text, CustomLogType.Error);
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
				Log(text);
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
			("HWID", hardwareId),
			("SID", sessionId),
			("license_key", licenseKey)
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
		currentDateStamp = text + "/" + text2 + "/" + text3;
		return currentDateStamp;
	}

	private static void InterruptAnnotation(Action instance)
	{
		if (isLicensed)
		{
			if (((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			})())
			{
				instance?.Invoke();
			}
		}
		else
		{
			pendingLicensedCallbacks = (Action)Delegate.Remove(pendingLicensedCallbacks, instance);
			pendingLicensedCallbacks = (Action)Delegate.Combine(pendingLicensedCallbacks, instance);
		}
	}

	private static void ManageAnnotation(bool applyident)
	{
		if (isLicensed && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			if (!licensedCallbacksFlushed)
			{
				pendingLicensedCallbacks?.Invoke();
			}
			licensedCallbacksFlushed = true;
		}
	}

	private static void PrintAnnotation(Action v)
	{
		pendingResetCallbacks = (Action)Delegate.Remove(pendingResetCallbacks, v);
		pendingResetCallbacks = (Action)Delegate.Combine(pendingResetCallbacks, v);
	}

	private static void SearchAnnotation()
	{
		isLicensed = false;
		licenseRestoredFromCache = false;
		licenseToken = (licenseUsername = (licenseVariant = string.Empty));
		EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: false);
		SessionState.EraseBool("yOk0XCnENLMO6DIF8cYpSg==" + EditorAnalyticsSessionInfo.id);
		pendingResetCallbacks?.Invoke();
	}

	[SpecialName]
	private static string DisableInitializer()
	{
		string text = "";
		if (serverWarnedTooManyAttempts)
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
				GUILayout.Label("License: " + (string.IsNullOrWhiteSpace(licenseVariant) ? "Personal" : licenseVariant), EditorUtils.styles().noteLeft);
				GUILayout.FlexibleSpace();
			}
			if (!string.IsNullOrWhiteSpace(licensedToDisplayName))
			{
				using (new GUILayout.HorizontalScope(GUI.skin.box))
				{
					GUILayout.Label("Authorized For: " + licensedToDisplayName, EditorUtils.styles().noteRight);
					return;
				}
			}
		}
	}

	private static bool OrderVisitor(EditorWindow value = null, float map = 0f)
	{
		if (!isLicensed)
		{
			if (Event.current.type == EventType.Repaint)
			{
				FillAnnotation();
			}
			if ((object)value != null)
			{
				EditorUtils.setterProcessor.PopHelper(value, map);
			}
			RemoveVisitor();
			if (isActivatingLicense || isVerifyingLicense)
			{
				SetVisitor((!isActivatingLicense) ? "Verifying License..." : "Activating License...", "Please wait till this finishes processing.");
				return false;
			}
			if (showingTransferPanel)
			{
				CompareVisitor();
				return false;
			}
			if (!licenseKeyEntryRequired || licenseCheckRetryOffered)
			{
				SetVisitor("Check for License", "This will check for whether you already have a license for your device");
				if (EditorUtils.Button((!licenseCheckRetryOffered) ? "Check" : "Retry", EditorStyles.toolbarButton))
				{
					WriteAnnotation(assetneeded: true);
				}
				return false;
			}
			SetVisitor("Enter your license key", "Enter the license key you received with your purchase here. If your license was already activated, click on 'Transfer License'. For support, contact @Dreadrith.");
			bool flag = ReadLicenseKey(isinit: false);
			if (DisableInitializer().Length > 0)
			{
				EditorGUILayout.HelpBox(DisableInitializer(), MessageType.Error);
			}
			bool flag2 = EnableVisitor() && !CancelInitializer();
			flag &= flag2 && !licenseCheckedThisSession;
			using (new EditorGUI.DisabledScope(!flag2))
			{
				if (EditorUtils.Button("Activate") || flag)
				{
					ForgotAnnotation();
				}
			}
			DefineVisitor(PopVisitor);
			return false;
		}
		if (feedbackPanelOpen)
		{
			ListAnnotation();
			return false;
		}
		if (bugReporterOpen)
		{
			BugReporter.PostReg();
			return false;
		}
		return true;
	}

	private static void CompareVisitor()
	{
		SetVisitor("Transferring License", "This is for moving your license to a new device or re-activating it in case it fails to recognize your device.");
		if (!transferCodeSent)
		{
			EditorGUILayout.HelpBox("Use this to move your own license from another device.\nAfter entering your license key, press 'Send Verification Code' to send a 6-digit code to the email address associated with the license key.", MessageType.Info);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(isRequestingTransferCode);
			try
			{
				ReadLicenseKey(isinit: true);
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			if (DisableInitializer().Length > 0)
			{
				EditorGUILayout.HelpBox(DisableInitializer(), MessageType.Error);
			}
			disabledScope = new EditorGUI.DisabledScope(!SaveLicenseInfo() || isRequestingTransferCode);
			try
			{
				if (EditorUtils.Button(isRequestingTransferCode ? "Sending Verification Code..." : "Send Verification Code"))
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
			EditorGUILayout.HelpBox("A 6-digit verification code was sent to " + transferTargetEmail + ".\nIf this is not your email address, please contact support.\nIf you don't see the verification email, please check your spam folder.", MessageType.Info);
			transferVerificationCode = EditorGUILayout.TextField("Verification Code", transferVerificationCode);
			transferVerificationCode = Regex.Replace(transferVerificationCode, "[^0-9]", string.Empty, RegexOptions.Multiline);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!Regex.IsMatch(transferVerificationCode, "[0-9]{6}") || isConfirmingTransfer);
			try
			{
				if (EditorUtils.Button((!isConfirmingTransfer) ? "Transfer License" : "Transferring..."))
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
		using (new GUILayout.HorizontalScope(EditorUtils.styles().bigTitleBackground))
		{
			GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
			GUILayout.Label(item, EditorUtils.styles().centeredBoldRichLabel);
			GUILayout.Label(new GUIContent(EditorUtils.contents().inspectorWindow)
			{
				tooltip = cust
			}, EditorUtils.styles().iconButton, GUILayout.Width(17f), GUILayout.Height(17f));
		}
	}

	private static bool ReadLicenseKey(bool isinit)
	{
		using (new GUILayout.HorizontalScope())
		{
			string text = "Controller EditorLicenseField";
			if (EditorUtils.SubmitPressed(text))
			{
				GUI.FocusControl(null);
				return true;
			}
			if (EditorUtils.CancelPressed(text))
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
				licenseKey = EditorGUILayout.TextField(string.Empty, licenseKey).Trim();
				EditorUtils.TestQueue("License Key", string.IsNullOrWhiteSpace(licenseKey), 80f);
			}
			if (!licenseCheckedThisSession && EnableVisitor() && !CancelInitializer())
			{
				licenseCheckedThisSession = true;
				return true;
			}
		}
		return false;
	}

	private static bool SaveLicenseInfo()
	{
		if (!showingTransferPanel)
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
		return Regex.Match(licenseKey, "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
	}

	private static bool PublishVisitor()
	{
		if (!transferCodeSent)
		{
			return true;
		}
		return Regex.Match(transferVerificationCode, "^[a-zA-Z0-9]{6}$").Success;
	}

	private static void PopVisitor()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (EditorUtils.ComputeQueue((!showingTransferPanel) ? "Transfer License" : "Activate License"))
			{
				showingTransferPanel = !showingTransferPanel;
			}
		}
	}

	private static void ComputeVisitor(string asset = null, string pol = null, string dir = null)
	{
		EditorSettings.GetInstance().u_announcementHidden.SetValue(excludeparam: false);
		if (asset != null)
		{
			EditorSettings.GetInstance().u_announcement.SetValue(asset);
		}
		if (pol != null)
		{
			EditorSettings.GetInstance().u_announcementLink.SetValue(pol);
		}
		if (dir != null)
		{
			EditorSettings.GetInstance().u_announcementLinkName.SetValue(dir);
		}
	}

	private static void MoveVisitor(string ident = null, string pred = null, string third = null, string token2 = null, bool havetoken3 = false, bool? selection4 = null)
	{
		bool value = EditorSettings.GetInstance().u_announcementHidden;
		if (ident != null)
		{
			EditorSettings.GetInstance().u_updateVersion.SetValue(ident);
		}
		if (pred != null)
		{
			EditorSettings.GetInstance().u_updateMessage.SetValue(pred);
		}
		if (third != null)
		{
			EditorSettings.GetInstance().u_updateLink.SetValue(third);
		}
		if (token2 != null)
		{
			EditorSettings.GetInstance().u_updateChangelog.SetValue(token2);
		}
		if (selection4.HasValue)
		{
			EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: false);
		}
		ConnectVisitor(havetoken3);
		EditorSettings.GetInstance().u_announcementHidden.SetValue(value);
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

	private static async Task<JsonObject> CountVisitor(string reference, string caller)
	{
		JsonObject collectionDefinition = default(JsonObject);
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
			collectionDefinition = new JsonObject(i);
		});
		return collectionDefinition;
	}

	private static Task<JsonObject> DisableVisitor(string info)
	{
		return CountVisitor("https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand", info);
	}

	private static void DrawLicenseInfo()
	{
		EditorUtils.DelayCall(RestartVisitor);
	}

	private static void RestartVisitor()
	{
		Type[] array = repaintTargetTypes;
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
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button(new GUIContent("Made By @Dreadrith", "https://dreadrith.com/links"), EditorUtils.styles().linkNote))
				{
					Application.OpenURL("https://dreadrith.com/links");
				}
				EditorUtils.DrawLinkUnderline();
				SupportWindow.DrawButton();
			}
		}
	}

	internal static bool LogWarning(string def, bool countresult = true)
	{
		return Log(def, CustomLogType.Warning, countresult);
	}

	internal static bool LogError(string param, bool iscounter = true)
	{
		return Log(param, CustomLogType.Error, iscounter);
	}

	internal static bool Log(string param, CustomLogType selection = CustomLogType.Regular, bool setrule = true)
	{
		if (setrule)
		{
			Color color = ((selection == CustomLogType.Regular) ? EditorUtils.validColor : ((selection != CustomLogType.Warning) ? EditorUtils.errorColor : EditorUtils.warningColor));
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

	internal static void ThrowError(string spec, bool haveb = true)
	{
		if (haveb)
		{
			throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(EditorUtils.errorColor) + ">[Controller Editor]</color> " + spec);
		}
	}

	private static void InitVisitor()
	{
		string message = "License transfer is subject to the Terms of Service.\nLicense will stop working on the device it was previously activated on.\nYou will not be able to transfer back or again for 30 days.";
		switch (EditorUtility.DisplayDialogComplex("Terms of Service", message, "Continue", "Terms of Service", "Cancel"))
		{
		case 0:
			isRequestingTransferCode = true;
			UpdateAnnotation(delegate
			{
				List<(string, string)> list = RegisterAnnotation("transferlicenserequest");
				LogoutAnnotation(list);
				DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
				{
					_003C_003Ec__DisplayClass239_0 _003C_003Ec__DisplayClass239_ = new _003C_003Ec__DisplayClass239_0();
					_003C_003Ec__DisplayClass239_.serviceDefinition = response;
					isRequestingTransferCode = false;
					SortAnnotation(_003C_003Ec__DisplayClass239_.serviceDefinition, _003C_003Ec__DisplayClass239_.InterruptObserver);
				}, delegate(Exception exception)
				{
					isRequestingTransferCode = false;
					Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
				}, null, null, DrawLicenseInfo);
			}, iscont: true);
			break;
		case 1:
			Application.OpenURL("https://dreadrith.com/license-tos");
			break;
		}
	}

	private static void VisitVisitor()
	{
		isConfirmingTransfer = true;
		UpdateAnnotation(delegate
		{
			List<(string, string)> list = RegisterAnnotation("transferlicenseconfirm");
			list.Add(("verification_code", transferVerificationCode));
			LogoutAnnotation(list);
			DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
			{
				isConfirmingTransfer = false;
				SortAnnotation(response, delegate
				{
					showingTransferPanel = false;
					transferCodeSent = false;
					licenseKeyEntryRequired = false;
					WriteAnnotation(assetneeded: true);
				});
			}, delegate(Exception exception)
			{
				isConfirmingTransfer = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, DrawLicenseInfo);
		}, iscont: true);
	}

	[SpecialName]
	private static bool RestartInitializer()
	{
		return EditorSettings.GetInstance().u_updateDay == PatchAnnotation();
	}

	private static void DefineVisitor(Action item = null, Action<GenericMenu> cont = null)
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (EditorUtils.IconButton(EditorUtils.contents().hamburgerMenu))
				{
					ReadVisitor(cont);
				}
				if (updateAvailable && !EditorSettings.GetInstance().u_updateHidden && EditorUtils.IconButton(EditorUtils.contents().updateAvailable))
				{
					updateFoldout.target = !updateFoldout.target;
				}
				GUILayout.Label("v" + m_RefAnnotation, EditorUtils.styles().noteLeftTight, GUILayout.ExpandWidth(expand: false));
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
			if (updateAvailable && !EditorSettings.GetInstance().u_updateHidden && updateFoldout.target)
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
			if (EditorUtils.IconButton(EditorUtils.contents().hamburgerMenu))
			{
				ReadVisitor(cfg);
			}
			if (updateAvailable && !EditorSettings.GetInstance().u_updateHidden && EditorUtils.IconButton(EditorUtils.contents().updateAvailable))
			{
				updateFoldout.target = !updateFoldout.target;
			}
			GUILayout.Label("v" + m_RefAnnotation, EditorUtils.styles().miniNote, GUILayout.ExpandWidth(expand: false));
			key?.Invoke();
		}
	}

	private static void ReadVisitor(Action<GenericMenu> init = null)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Check For Update"), on: false, (!isCheckingForUpdate && !hasCheckedForUpdate) ? ((GenericMenu.MenuFunction)delegate
		{
			SessionState.EraseString("yOk0XCnENLMO6DIF8cYpSg==updateinfo");
			AwakeVisitor();
		}) : null);
		if (isLicensed)
		{
			genericMenu.AddItem(new GUIContent("Send Feedback"), feedbackPanelOpen, delegate
			{
				feedbackPanelOpen.Flip();
			});
		}
		if (isLicensed)
		{
			if (init != null)
			{
				init(genericMenu);
				genericMenu.AddSeparator(string.Empty);
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Verify/On Display"), EditorSettings.GetInstance().a_VerifyOnDisplay, delegate
			{
				EditorSettings.GetInstance().a_VerifyOnDisplay.Toggle();
				EditorSettings.GetInstance().a_VerifyOnProjectLoad.SetValue(excludeparam: false);
			});
			genericMenu.AddItem(new GUIContent("Verify/On Project Load"), EditorSettings.GetInstance().a_VerifyOnProjectLoad, delegate
			{
				EditorSettings.GetInstance().a_VerifyOnProjectLoad.Toggle();
				EditorSettings.GetInstance().a_VerifyOnDisplay.SetValue(excludeparam: false);
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
		if (isLicensed)
		{
			if (extraMenuLinks.Length != 0)
			{
				if (extraMenuLinks.Length <= 1)
				{
					genericMenu.AddItem(new GUIContent(extraMenuLinks[0].Item1), on: false, delegate
					{
						Application.OpenURL(extraMenuLinks[0].Item2);
					});
				}
				else
				{
					(string, string)[] array = extraMenuLinks;
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
		if (!updateAvailable || (bool)EditorSettings.GetInstance().u_updateHidden)
		{
			return;
		}
		updateFoldout.FadeGroup(delegate
		{
			if (isi)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(EditorUtils.contents().announcement, GUILayout.Width(24f), GUILayout.Height(24f));
				GUILayout.Label($"v{EditorSettings.GetInstance().u_updateVersion}", EditorUtils.styles().title);
			}
			if (EditorUtils.ClickArea())
			{
				updateFoldout.target = !updateFoldout.target;
			}
			EditorUtils.Separator();
			GUILayout.TextArea(EditorSettings.GetInstance().u_updateMessage, EditorUtils.styles().wrappedRichLabel);
			bool flag = !string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_updateLink);
			bool flag2 = !string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_updateChangelog);
			EditorGUILayout.Space();
			using (new GUILayout.HorizontalScope())
			{
				if (flag)
				{
					using (new EditorGUI.DisabledScope(isDownloadingUpdate))
					{
						if (EditorUtils.Button("Download Update", EditorStyles.toolbarButton))
						{
							FlushVisitor();
						}
					}
				}
				if (flag2 && EditorUtils.Button(new GUIContent("Open Changelog", EditorSettings.GetInstance().u_updateChangelog), EditorStyles.toolbarButton))
				{
					Application.OpenURL(EditorSettings.GetInstance().u_updateChangelog);
				}
				if (EditorUtils.Button("Skip for Today", EditorStyles.toolbarButton))
				{
					EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: true);
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
		if ((bool)EditorSettings.GetInstance().u_announcementHidden || string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_announcement))
		{
			return;
		}
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Rect taskDefinition = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true), GUILayout.Height(24f));
			Rect def = taskDefinition;
			GUI.Label(def.SliceLeft(24f, isfield: true), EditorUtils.contents().announcement);
			GUI.Label(def, "Announcement", EditorUtils.styles().title);
			announcementFoldout.FadeGroup(delegate
			{
				taskDefinition.height += 18f;
				EditorUtils.Separator();
				GUILayout.TextArea(EditorSettings.GetInstance().u_announcement, EditorUtils.styles().wrappedRichLabel);
				EditorGUILayout.Space();
				using (new GUILayout.HorizontalScope())
				{
					if (!string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_announcementLink) && EditorUtils.Button(EditorSettings.GetInstance().u_announcementLinkName, EditorStyles.toolbarButton))
					{
						Application.OpenURL(EditorSettings.GetInstance().u_announcementLink);
					}
					if (isLicensed && EditorUtils.Button("Hide", EditorStyles.toolbarButton))
					{
						EditorSettings.GetInstance().u_announcementHidden.SetValue(excludeparam: true);
						EditorSettings.GetInstance().u_announcementHiddenDate.SetValue(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
					}
				}
			}, RestartVisitor);
			if (EditorUtils.ClickArea(taskDefinition))
			{
				announcementFoldout.target = !announcementFoldout.target;
			}
		}
	}

	[InitializeOnLoadMethod]
	private static void InstantiateVisitor()
	{
		if (!RestartInitializer())
		{
			EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: false);
		}
		else if (!string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_updateVersion.GetValue()))
		{
			ConnectVisitor(setasset: false);
			return;
		}
		EditorUtils.DelayCall(delegate
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
		if ((!removereference && RestartInitializer()) || hasCheckedForUpdate || isCheckingForUpdate)
		{
			return;
		}
		isCheckingForUpdate = true;
		DisableVisitor(CallVisitor(new List<(string, string)>
		{
			("command", "getdownloadinfo"),
			("product_id", "yOk0XCnENLMO6DIF8cYpSg=="),
			("version", m_RefAnnotation.ToString())
		})).QueryRules(delegate(JsonObject response)
		{
			hasCheckedForUpdate = true;
			string value = EditorSettings.GetInstance().u_announcement.GetValue();
			using (new EditorSettings.SettingsDeferScope())
			{
				EditorSettings.GetInstance().u_updateLink.SetValue(response.Item("download_link"));
				EditorSettings.GetInstance().u_updateMessage.SetValue(response.Item("download_message"));
				EditorSettings.GetInstance().u_updateChangelog.SetValue(response.Item("changelog_link"));
				EditorSettings.GetInstance().u_updateVersion.SetValue(response.Item("version"));
				EditorSettings.GetInstance().u_updateDay.SetValue(PatchAnnotation());
				EditorSettings.GetInstance().u_announcement.SetValue(response.Item("announcement"));
				if (!string.IsNullOrWhiteSpace(EditorSettings.GetInstance().u_announcement))
				{
					EditorSettings.GetInstance().u_announcement.SetValue(EditorSettings.GetInstance().u_announcement.GetValue().Replace("\\\\n", "\n").Replace("\\n", "\n"));
				}
				EditorSettings.GetInstance().u_announcementLink.SetValue(response.Item("announcement_link"));
				EditorSettings.GetInstance().u_announcementLinkName.SetValue(response.Item("announcement_link_name"));
			}
			if (value != EditorSettings.GetInstance().u_announcement.GetValue())
			{
				EditorSettings.GetInstance().u_announcementHidden.SetValue(excludeparam: false);
			}
			ConnectVisitor(removereference);
		}, delegate(Exception exc)
		{
			Log($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}, null, null, delegate
		{
			isCheckingForUpdate = false;
			DrawLicenseInfo();
		});
	}

	private static void FlushVisitor()
	{
		isDownloadingUpdate = true;
		UnityWebRequest m_ProducerDefinition = new UnityWebRequest(EditorSettings.GetInstance().u_updateLink);
		m_ProducerDefinition.downloadHandler = new DownloadHandlerFile("Assets/Controller Editor.unitypackage");
		m_ProducerDefinition.SendWebRequest().completed += delegate
		{
			isDownloadingUpdate = false;
			string text = "Assets/Controller Editor.unitypackage";
			if (m_ProducerDefinition.isNetworkError || m_ProducerDefinition.isHttpError)
			{
				AssetDatabase.ImportAsset(text);
				AssetDatabase.DeleteAsset(text);
				m_ProducerDefinition.Dispose();
				throw new Exception(m_ProducerDefinition.error);
			}
			AssetDatabase.ImportPackage(text, interactive: true);
			AssetDatabase.DeleteAsset(text);
			m_ProducerDefinition.Dispose();
		};
	}

	private static void ConnectVisitor(bool setasset)
	{
		if ((bool)EditorSettings.GetInstance().u_announcementHidden)
		{
			if (DateTime.TryParse(EditorSettings.GetInstance().u_announcementHiddenDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
			{
				EditorSettings.GetInstance().u_announcementHidden.SetValue((DateTime.UtcNow - result).TotalDays < 7.0);
			}
			else
			{
				EditorSettings.GetInstance().u_announcementHidden.SetValue(excludeparam: false);
			}
		}
		if (!(m_RefAnnotation < new VersionNumber(EditorSettings.GetInstance().u_updateVersion.GetValue())))
		{
			if (setasset)
			{
				Log("Up to date!");
				Task.Run(async delegate
				{
					await Task.Delay(3000);
					EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: true);
					DrawLicenseInfo();
				});
			}
			else
			{
				EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: true);
			}
			return;
		}
		updateAvailable = true;
		if (setasset)
		{
			EditorSettings.GetInstance().u_updateHidden.SetValue(excludeparam: false);
			updateFoldout.target = true;
		}
		if (!EditorSettings.GetInstance().u_updateHidden)
		{
			Log($"Update Available! <b>(v{EditorSettings.GetInstance().u_updateVersion})</b>");
		}
	}

	private static void CalculateVisitor(Rect spec)
	{
		EditorGUI.DisabledScope disabledScope;
		if (!(focusedTransition.transition == null))
		{
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			})())
			{
				return;
			}
			Rect item = new Rect(spec);
			item.width = 18f;
			using (new EditorGUI.DisabledScope(focusedTransition.transition.conditions.Length == 0))
			{
				if (EditorUtils.QueryQueue(item, EditorUtils.contents().copy, GUI.skin.label))
				{
					SortVisitor();
				}
			}
			item.x += 20f;
			disabledScope = new EditorGUI.DisabledScope(copiedConditions.Count == 0);
			try
			{
				if (EditorUtils.QueryQueue(item, EditorUtils.contents().paste, GUI.skin.label))
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
			GUI.Label(item, focusedTransition.DisplayName() + "'s Conditions");
		}
		else
		{
			using (new GUIDisabledScope(iskey: false))
			{
				Rect item2 = new Rect(spec);
				item2.width = 16f;
				item2.x = spec.x - 3f;
				item2.y = spec.y + 2f;
				if (EditorUtils.QueryQueue(item2, EditorUtils.contents().shared, GUIStyle.none))
				{
					showSharedConditions = !showSharedConditions;
					MapVisitor();
				}
			}
			spec.x += 12f;
			GUI.Label(spec, (!showSharedConditions) ? "All Conditions" : "Shared Conditions");
			spec.x -= 12f;
		}
		Rect item3 = new Rect(spec);
		item3.x += 95f;
		if (showSharedConditions)
		{
			item3.x += 29f;
		}
		item3.width = 18f;
		if (!focusedTransition.transition)
		{
			disabledScope = new EditorGUI.DisabledScope((showSharedConditions && sharedConditionEditors.Count == 0) || (!showSharedConditions && allConditionEditors.Count == 0));
			try
			{
				if (EditorUtils.QueryQueue(item3, EditorUtils.contents().copy, GUI.skin.label))
				{
					SortVisitor();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			item3.x += 20f;
			using (new EditorGUI.DisabledScope(copiedConditions.Count == 0))
			{
				if (EditorUtils.QueryQueue(item3, EditorUtils.contents().paste, GUI.skin.label))
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
		EditorGUI.BeginDisabledGroup((!focusedTransition.transition && ((showSharedConditions && sharedConditionEditors.Count == 0) || (!showSharedConditions && allConditionEditors.Count == 0))) || (focusedTransition.transition != null && focusedTransition.transition.conditions.Length < 1));
		if (EditorUtils.QueryQueue(item3, EditorUtils.contents().switchLayer, GUIStyle.none))
		{
			if (!(focusedTransition.transition != null))
			{
				if (!showSharedConditions)
				{
					foreach (ConditionMultiEditor allConditionEditor in allConditionEditors)
					{
						allConditionEditor.Invert();
					}
					UpdateVisitor();
				}
				else
				{
					foreach (ConditionMultiEditor sharedConditionEditor in sharedConditionEditors)
					{
						sharedConditionEditor.Invert();
					}
					ChangeVisitor();
				}
			}
			else
			{
				foreach (ConditionMultiEditor focusedConditionEditor in focusedConditionEditors)
				{
					AnimatorCondition animatorCondition = ResolveAlgo(focusedConditionEditor.condition);
					focusedConditionEditor.ApplyToAll(animatorCondition);
					focusedConditionEditor.condition = animatorCondition;
				}
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
				})())
				{
					return;
				}
				ChangeVisitor();
				sharedConditionEditors = AssetVisitor(selectedTransitions);
			}
		}
		EditorGUI.EndDisabledGroup();
		spec.x += spec.width - 52f;
		spec.width = 15f;
		using (new GUIDisabledScope(iskey: false))
		{
			if (EditorUtils.QueryQueue(spec, new GUIContent(EditorUtils.contents().settings)
			{
				tooltip = "Toggles custom matching options"
			}, GUIStyle.none))
			{
				EditorSettings.GetInstance().showMatchingOptions.Toggle();
			}
		}
		spec.x += 17f;
		using (new EditorGUI.DisabledScope(focusedTransition.transition))
		{
			if (EditorUtils.QueryQueue(spec, EditorUtils.contents().merge, GUIStyle.none))
			{
				ConnectAlgo();
			}
		}
		spec.x += 17f;
		if (EditorUtils.QueryQueue(spec, EditorUtils.contents().separate, GUIStyle.none))
		{
			ViewAlgo();
		}
	}

	private static void TestVisitor(Rect def, int boffset, bool isconsumer, bool dotoken2)
	{
		_003C_003Ec__DisplayClass285_0 CS_0024_003C_003E8__locals43 = new _003C_003Ec__DisplayClass285_0();
		if (!ActiveController())
		{
			return;
		}
		List<ConditionMultiEditor> list = StopVisitor();
		if (boffset >= list.Count || boffset < 0)
		{
			return;
		}
		ConditionMultiEditor conditionMultiEditor = list[boffset];
		CS_0024_003C_003E8__locals43.m_ConfigurationDefinition = conditionMultiEditor.condition;
		int second;
		UnityEngine.AnimatorControllerParameter animatorControllerParameter = ResetAnnotation(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter, out second);
		bool flag = false;
		bool flag2 = false;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
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
			Rect rect = def2.SliceLeft(50f);
			Rect rect2 = new Rect(rect);
			rect2.x = rect.x + 3f;
			rect2.width = rect.width - 3f;
			Rect rect3 = rect2;
			def2.SliceLeft(3f, isfield: true);
			Rect rect4 = def2.SliceLeft(100f);
			CS_0024_003C_003E8__locals43.CompareServer(rect);
			if (!CS_0024_003C_003E8__locals43._IteratorDefinition)
			{
				using (new MixedValueScope(conditionMultiEditor.mixedValues[0]))
				{
					EditorGUI.BeginChangeCheck();
					int firstsize = EditorGUI.Popup(rect, -1, parameterNames);
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
				ActiveController().AddParameter(text, (UnityEngine.AnimatorControllerParameterType)num2);
				ArrayUtility.Add(ref parameterNames, text);
				CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = text;
				CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
			}
		}
		else
		{
			Rect rect5 = def2.SliceLeft(20f, isfield: true);
			Rect rect6 = def2.SliceLeft((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger) ? 50 : 100);
			Rect rect7 = new Rect(rect6)
			{
				width = 20f,
				x = rect6.x + rect6.width - 40f
			};
			Rect rect8 = def2.SliceLeft((animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Bool) ? 50 : 100);
			Rect rect9 = def2.SliceLeft(100f);
			if (GUI.Button(rect5, EditorUtils.contents().pickable, EditorUtils.styles().paddedBox))
			{
				IEnumerable<IEnumerable<AnimatorStateTransition>> first = ActiveStateMachine().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				List<AnimatorTransitionBase> mapperReg = new List<AnimatorTransitionBase>();
				first.ForEach(delegate(IEnumerable<AnimatorStateTransition> e)
				{
					mapperReg.AddRange(e);
				});
				mapperReg.AddRange(ActiveStateMachine().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				mapperReg.AddRange(ActiveStateMachine().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => ForgotVisitor(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition, c, forcetag: true))));
				Selection.objects = Selection.objects.Concat(mapperReg).Distinct().ToArray();
			}
			if (GUI.Button(rect7, GUIContent.none, GUIStyle.none))
			{
				string initializerReg = CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter;
				IEnumerable<IEnumerable<AnimatorStateTransition>> first2 = ActiveStateMachine().states.Select((ChildAnimatorState s) => s.state.transitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				List<AnimatorTransitionBase> m_DefinitionReg = new List<AnimatorTransitionBase>();
				first2.ForEach(delegate(IEnumerable<AnimatorStateTransition> e)
				{
					m_DefinitionReg.AddRange(e);
				});
				m_DefinitionReg.AddRange(ActiveStateMachine().anyStateTransitions.Where((AnimatorStateTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				m_DefinitionReg.AddRange(ActiveStateMachine().entryTransitions.Where((AnimatorTransition t) => t.conditions.Any((AnimatorCondition c) => c.parameter == initializerReg)));
				Selection.objects = Selection.objects.Concat(m_DefinitionReg).Distinct().ToArray();
			}
			CS_0024_003C_003E8__locals43.CompareServer(rect6);
			if (!CS_0024_003C_003E8__locals43.m_ProcDefinition && !CS_0024_003C_003E8__locals43._IteratorDefinition)
			{
				using (new MixedValueScope(conditionMultiEditor.mixedValues[0]))
				{
					if ((bool)EditorSettings.GetInstance().useLegacyDropdown)
					{
						EditorGUI.BeginChangeCheck();
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = TestAnnotation(EditorGUI.Popup(rect6, second, parameterNames));
						if (EditorGUI.EndChangeCheck())
						{
							CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
						}
					}
					else
					{
						object[] parameters = new object[3] { rect6, second, parameterNames };
						int num3 = (int)advancedPopupMethod.Invoke(null, parameters);
						if (num3 != second)
						{
							CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter = TestAnnotation(num3);
							CS_0024_003C_003E8__locals43.m_ProcDefinition = true;
						}
					}
				}
			}
			GUI.Label(rect7, EditorUtils.contents().pickable, GUIStyle.none);
			if (animatorControllerParameter.type != UnityEngine.AnimatorControllerParameterType.Trigger)
			{
				using (new MixedValueScope(conditionMultiEditor.mixedValues[1]))
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
				using (new MixedValueScope(conditionMultiEditor.mixedValues[2]))
				{
					EditorGUI.BeginChangeCheck();
					UnityEngine.AnimatorControllerParameterType type = animatorControllerParameter.type;
					if (type == UnityEngine.AnimatorControllerParameterType.Float)
					{
						GUI.SetNextControlName("Threshold" + thresholdControlCounter);
						thresholdControlCounter++;
						CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold = EditorGUI.FloatField(rect9, CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold);
					}
					else if (type == UnityEngine.AnimatorControllerParameterType.Int)
					{
						GUI.SetNextControlName("Threshold" + thresholdControlCounter);
						thresholdControlCounter++;
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
				else if (((int)CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode).IsOutside(3, 5))
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		if (CS_0024_003C_003E8__locals43.m_ProcDefinition)
		{
			conditionMultiEditor.SetParameter(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.parameter);
		}
		if (flag)
		{
			conditionMultiEditor.SetMode(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.mode);
		}
		if (flag2)
		{
			conditionMultiEditor.SetThreshold(CS_0024_003C_003E8__locals43.m_ConfigurationDefinition.threshold);
		}
		if (!GUI.Button(source, EditorUtils.contents().removeCondition, EditorUtils.styles().footerButton))
		{
			return;
		}
		conditionMultiEditor.RemoveFromAll();
		if (focusedTransition.transition == null)
		{
			if (!showSharedConditions)
			{
				allConditionEditors.RemoveAt(boffset);
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		if (!(focusedTransition.transition != null))
		{
			if (!showSharedConditions)
			{
				allConditionList = new ReorderableList(allConditionEditors, typeof(ConditionMultiEditor), draggable: false, displayHeader: true, selectedTransitionEdits.Count == 1, displayRemoveButton: false)
				{
					drawElementCallback = TestVisitor,
					drawHeaderCallback = CalculateVisitor,
					onAddCallback = FillVisitor
				};
			}
			else
			{
				sharedConditionList = new ReorderableList(sharedConditionEditors, typeof(ConditionMultiEditor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
				{
					drawElementCallback = TestVisitor,
					drawHeaderCallback = CalculateVisitor,
					onAddCallback = FillVisitor
				};
			}
		}
		else
		{
			focusedConditionEditors = PrepareVisitor(focusedTransition.transition);
			focusedConditionList = new ReorderableList(focusedConditionEditors, typeof(ConditionMultiEditor), draggable: false, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = TestVisitor,
				drawHeaderCallback = CalculateVisitor,
				onAddCallback = FillVisitor
			};
		}
	}

	private void DrawControllerSection()
	{
		if (!EditorSettings.GetInstance().editingController)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(ActiveController() == null))
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
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
					selectedAction = (ControllerAction)(object)EditorGUILayout.EnumPopup(selectedAction, GUILayout.Width(EditorUtils.PostQueue(selectedAction) + 28f));
					switch (selectedAction)
					{
					case ControllerAction.RemoveTag:
						flag = true;
						flag4 = true;
						break;
					case ControllerAction.Copy:
						break;
					default:
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
					if (flag4 && actionScope == ActionMode.LayersTaggedWith)
					{
						flag3 = true;
					}
					if (flag)
					{
						EditorGUIUtility.labelWidth = 40f;
						actionSourceName = EditorGUILayout.TextField("", actionSourceName, "textfielddropdowntext");
						EditorGUIUtility.labelWidth = 0f;
						int num = -1;
						EditorGUI.BeginChangeCheck();
						num = EditorGUILayout.Popup(-1, parameterNames ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
						if (EditorGUI.EndChangeCheck())
						{
							actionSourceName = parameterNames[num];
						}
					}
					if (flag3 && actionScope != ActionMode.LayersTaggedWith)
					{
						actionFilterText = EditorGUILayout.TextField(actionFilterText);
					}
					if (flag2)
					{
						GUILayout.Label("With", GUILayout.Width(32f));
						EditorGUIUtility.labelWidth = 40f;
						actionReplacementName = EditorGUILayout.TextField("", actionReplacementName, "textfielddropdowntext");
						EditorGUIUtility.labelWidth = 0f;
						int num2 = -1;
						EditorGUI.BeginChangeCheck();
						num2 = EditorGUILayout.Popup(-1, parameterNames ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
						if (EditorGUI.EndChangeCheck())
						{
							actionReplacementName = parameterNames[num2];
						}
					}
					if (flag4)
					{
						GUILayout.Label("In", GUILayout.Width(15f));
						actionScope = (ActionMode)(object)EditorGUILayout.EnumPopup(actionScope, GUILayout.Width(140f));
					}
					if (flag3 && actionScope == ActionMode.LayersTaggedWith)
					{
						actionFilterText = EditorGUILayout.TextField(actionFilterText);
					}
					if (selectedAction == ControllerAction.Copy)
					{
						copySourceScope = (MoveMode)(object)EditorGUILayout.EnumPopup(copySourceScope, GUILayout.Width(EditorUtils.PostQueue(copySourceScope) + 28f));
						if (copySourceScope == MoveMode.LayersTaggedWith)
						{
							flag3 = true;
							actionFilterText = EditorGUILayout.TextField(actionFilterText);
						}
						GUILayout.Label("To", GUILayout.Width(20f));
						copyDestination = (MoveDestination)(object)EditorGUILayout.EnumPopup(copyDestination, GUILayout.Width(EditorUtils.PostQueue(copyDestination) + 28f));
						if (copyDestination == MoveDestination.Controller)
						{
							actionTargetController = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(actionTargetController, typeof(UnityEditor.Animations.AnimatorController), false);
						}
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					if (selectedAction == ControllerAction.RemoveParameter || selectedAction == ControllerAction.ReplaceParameter)
					{
						matchWholeWord = EditorGUILayout.Toggle(new GUIContent("Match Whole Word", "Apply to parameters that match exactly. Otherwise apply to parameters that contain it"), matchWholeWord);
					}
					else if (selectedAction == ControllerAction.Copy)
					{
						addRequiredParameters = EditorGUILayout.Toggle(new GUIContent("Add Required Parameters", "Add the parameters used by the Source to the destination Controller. Adds Suffix if Suffix isn't empty."), addRequiredParameters, GUILayout.Width(180f));
						GUILayout.FlexibleSpace();
						EditorGUIUtility.labelWidth = 50f;
						copiedParameterSuffix = EditorGUILayout.TextField(new GUIContent("Suffix:", "Add a Suffix to all the Parameters in the newly copied layers. Adds a Suffix to the added parameters if enabled."), copiedParameterSuffix);
						EditorGUIUtility.labelWidth = 0f;
					}
					else
					{
						GUILayout.FlexibleSpace();
					}
					EditorGUI.BeginDisabledGroup((string.IsNullOrEmpty(actionSourceName) && flag) || (string.IsNullOrEmpty(actionReplacementName) && flag2) || (string.IsNullOrEmpty(actionFilterText) && flag3) || (selectedAction == ControllerAction.Copy && copyDestination == MoveDestination.Controller && !actionTargetController));
					if (EditorUtils.Button("Apply", "minibutton", GUILayout.Width(140f)))
					{
						LogoutVisitor();
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.Space();
				EditorUtils.Separator();
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
			writeDefaultsPanelOpen = EditorUtils.ToggleButton(writeDefaultsPanelOpen, "Write Defaults", "toolbarbutton");
			if (EditorGUI.EndChangeCheck())
			{
				WriteMapper();
			}
			EditorGUI.BeginChangeCheck();
			subAssetPanelOpen = EditorUtils.ToggleButton(subAssetPanelOpen, "Explore Controller Sub-Assets", "toolbarbutton");
			if (EditorGUI.EndChangeCheck())
			{
				FillMapper();
			}
			if (EditorUtils.Button(new GUIContent("Cleanup unused Sub-Assets", "Some Controllers have residue in their Sub-Assets that may be unused, may happen when using this tool. Use this button to clean it up."), "toolbarbutton") && (bool)ActiveController())
			{
				SearchVisitor(ActiveController());
				VerifyMapper();
			}
		}
		if (!subAssetPanelOpen)
		{
			if (!writeDefaultsPanelOpen)
			{
				return;
			}
			if (!ActiveController())
			{
				VerifyMapper();
			}
			using (new GUILayout.HorizontalScope())
			{
				using (new GUILayout.VerticalScope())
				{
					if (EditorUtils.Button("Set All On"))
					{
						ForgotMapper(istask: true);
					}
					EditorGUILayout.Space();
					ReflectAnnotation("Write Defaults On");
					foreach (AnimatorState item in assetInventory.m_ServiceVisitor.Where((AnimatorState s) => (bool)s && s.writeDefaultValues))
					{
						using (new GUILayout.HorizontalScope())
						{
							CreateAnnotation(item, GUILayout.ExpandWidth(expand: true));
							if (EditorUtils.Button(">", GUILayout.ExpandWidth(expand: false)))
							{
								StopMapper(item, connectionreguired: false);
							}
						}
					}
				}
				LoginAnnotation();
				using (new GUILayout.VerticalScope())
				{
					if (EditorUtils.Button("Set All Off"))
					{
						ForgotMapper(istask: false);
					}
					EditorGUILayout.Space();
					ReflectAnnotation("Write Defaults Off");
					foreach (AnimatorState item2 in assetInventory.m_ServiceVisitor.Where((AnimatorState s) => (bool)s && !s.writeDefaultValues))
					{
						using (new GUILayout.HorizontalScope())
						{
							if (EditorUtils.Button("<", GUILayout.ExpandWidth(expand: false)))
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
		subAssetTabIndex = GUILayout.Toolbar(subAssetTabIndex, EditorUtils.contents().animatorElementTypes, EditorStyles.toolbarButton);
		int num = subAssetTabIndex;
		while (true)
		{
			switch (num)
			{
			case 5:
				assetInventory.stateVisitor.ForEach(RateVisitor);
				return;
			case 2:
				assetInventory.structVisitor.ForEach(RateVisitor);
				return;
			case 3:
				assetInventory._SchemaVisitor.ForEach(RateVisitor);
				return;
			case 1:
				assetInventory.m_ServiceVisitor.ForEach(RateVisitor);
				return;
			case 4:
				assetInventory.broadcasterVisitor.ForEach(RateVisitor);
				return;
			case 0:
				assetInventory._ProxyVisitor.ForEach(RateVisitor);
				return;
			}
		}
	}

	private static void RateVisitor(UnityEngine.Object task)
	{
		if (!task)
		{
			subAssetTabIndex = 0;
			EditorWindow.GetWindow<ControllerEditor>().Repaint();
			return;
		}
		bool flag = Selection.activeObject == task;
		using (new GUILayout.HorizontalScope())
		{
			string text = ((!string.IsNullOrEmpty(task.name)) ? task.name : task.GetType().Name);
			if (EditorUtils.Button("- " + text, (!flag) ? GUI.skin.label : EditorUtils.styles().linkLabel))
			{
				Selection.activeObject = (flag ? null : task);
			}
			GUILayout.FlexibleSpace();
			if (EditorUtils.Button((!task.hideFlags.HasFlag(HideFlags.HideInHierarchy)) ? EditorUtils.contents().visible : EditorUtils.contents().hidden, EditorUtils.styles().centeredIcon, GUILayout.Width(14f), GUILayout.Height(18f)))
			{
				Undo.RecordObject(task, "Toggle Sub-Asset Visibility");
				task.hideFlags ^= HideFlags.HideInHierarchy;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(task), ImportAssetOptions.ForceUpdate);
			}
			if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon) && EditorUtility.DisplayDialog("Delete", "Delete " + task.name + "?\nUse cautiously! May result in unintended behavior!", "Ok", "Cancel"))
			{
				Undo.RecordObject(task, "Remove SubAsset");
				AssetDatabase.RemoveObjectFromAsset(task);
				Undo.DestroyObjectImmediate(task);
			}
		}
	}

	private static void DrawStateSection()
	{
		if (!stateSectionVisible)
		{
			return;
		}
		int num = selectedStates.Count + (anyStateNodeSelected ? 1 : 0) + (entryNodeSelected ? 1 : 0) + (exitNodeSelected ? 1 : 0);
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			string token = $"State Count: {num}";
			DrawCollapsibleSection(GetVisitor, token, EditorSettings.GetInstance().showStateCount, iscont2: true, 3);
			DrawCollapsibleSection(CalcVisitor, "State Settings", EditorSettings.GetInstance().showStateSettings, iscont2: true, 4);
			if (AnimatorTypeCache.IsVRCSDKAvailable())
			{
				DrawCollapsibleSection(RunVisitor, "VRC Parameter Drivers", EditorSettings.GetInstance().showVRCDrivers, iscont2: false, 5);
				DrawCollapsibleSection(CloneVisitor, "VRC Tracking Control", EditorSettings.GetInstance().showVRCTracking, iscont2: false, 6);
			}
		}
	}

	private static void GetVisitor()
	{
		int num = selectedStates.Count + (anyStateNodeSelected ? 1 : 0) + (entryNodeSelected ? 1 : 0) + (exitNodeSelected ? 1 : 0);
		if (num <= 0)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				GUILayout.Label("Selected 0 States", EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
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
				if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] objects = Selection.objects;
					objects = Selection.objects.Except(ActiveStateMachine().states.Select((ChildAnimatorState c) => c.state)).ToArray();
					for (int num2 = objects.Length - 1; num2 >= 0; num2--)
					{
						Type type = objects[num2].GetType();
						if (type == AnimatorGraphReflection.TypeResolvers.exitNode || type == AnimatorGraphReflection.TypeResolvers.anyStateNode || type == AnimatorGraphReflection.TypeResolvers.entryNode)
						{
							ArrayUtility.RemoveAt(ref objects, num2);
						}
					}
					Selection.objects = objects;
				}
				GUILayout.Space(18f);
				GUILayout.FlexibleSpace();
				if (EditorUtils.Button("Out", GUILayout.Width(34f)))
				{
					GetAlgo();
				}
				GUILayout.Label("Selected " + num + " States", EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
				if (EditorUtils.Button("In", GUILayout.Width(34f)))
				{
					CalcAlgo();
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
			EditorGUILayout.Space();
		}
		foreach (AnimatorGraphReflection.GraphNodeRef item in selectedNodes.Where((AnimatorGraphReflection.GraphNodeRef n) => n.nodeType == AnimatorGraphReflection.GraphNodeRef.NodeType.state))
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array = Selection.objects;
					ArrayUtility.Remove(ref array, item.state);
					Selection.objects = array;
				}
				using (new EditorGUI.DisabledScope(!item.state.motion))
				{
					if (EditorUtils.Button(EditorUtils.contents().animationClip, EditorUtils.styles().paddedBox, GUILayout.Width(17f)))
					{
						EditorGUIUtility.PingObject(item.state.motion);
					}
				}
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(item.state.transitions.Length < 1))
				{
					if (EditorUtils.Button("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(item.OutgoingTransitions()).ToArray();
					}
				}
				GUILayout.Label(RunAnnotation(item.state.name, 18), EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(!item.IncomingTransitions().Any()))
				{
					if (EditorUtils.Button("In", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(item.IncomingTransitions()).ToArray();
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (anyStateNodeSelected)
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array2 = Selection.objects;
					for (int num3 = 0; num3 < array2.Length; num3++)
					{
						if (array2[num3].GetType() == AnimatorGraphReflection.TypeResolvers.anyStateNode)
						{
							ArrayUtility.RemoveAt(ref array2, num3);
							anyStateNodeSelected = false;
							break;
						}
					}
					Selection.objects = array2;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(!ActiveStateMachine() || ActiveStateMachine().anyStateTransitions.Length < 1))
				{
					if (EditorUtils.Button("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(RootStateMachine().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
						{
							_003C_003Ec__DisplayClass308_0 _003C_003Ec__DisplayClass308_ = new _003C_003Ec__DisplayClass308_0();
							_003C_003Ec__DisplayClass308_.serverReg = t;
							return ActiveStateMachine().states.Any(_003C_003Ec__DisplayClass308_.InvokeServer);
						})).ToArray();
					}
				}
				GUILayout.Label("Any State", EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorUtils.Button("In", GUILayout.Width(34f));
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (entryNodeSelected)
		{
			using (new GUILayout.HorizontalScope("box"))
			{
				if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] array3 = Selection.objects;
					for (int num4 = 0; num4 < array3.Length; num4++)
					{
						if (array3[num4].GetType() == AnimatorGraphReflection.TypeResolvers.entryNode)
						{
							ArrayUtility.RemoveAt(ref array3, num4);
							entryNodeSelected = false;
							break;
						}
					}
					Selection.objects = array3;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(!ActiveStateMachine() || ActiveStateMachine().entryTransitions.Length == 0))
				{
					if (EditorUtils.Button("Out", GUILayout.Width(34f)))
					{
						Selection.objects = Selection.objects.Concat(ActiveStateMachine().entryTransitions).ToArray();
					}
				}
				GUILayout.Label("Entry", EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorUtils.Button("In", GUILayout.Width(34f));
				}
				GUILayout.FlexibleSpace();
				GUILayout.Space(42f);
			}
		}
		if (exitNodeSelected)
		{
			using (new GUILayout.HorizontalScope("box"))
			{
				if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(17f)))
				{
					UnityEngine.Object[] objects2 = Selection.objects;
					for (int num5 = 0; num5 < objects2.Length; num5++)
					{
						if (objects2[num5].GetType() == AnimatorGraphReflection.TypeResolvers.exitNode)
						{
							objects2[num5] = null;
							exitNodeSelected = false;
							break;
						}
					}
					Selection.objects = objects2;
				}
				GUILayout.Space(21f);
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(disabled: true);
				EditorUtils.Button("Out", GUILayout.Width(30f));
				EditorGUI.EndDisabledGroup();
				GUILayout.Label("Exit", EditorUtils.styles().centeredBoldRichLabel, GUILayout.Width(140f));
				EditorGUI.BeginDisabledGroup(exitNodeIncomingTransitions.GetRules());
				if (EditorUtils.Button("In", GUILayout.Width(30f)))
				{
					Selection.objects = Selection.objects.Concat(exitNodeIncomingTransitions).ToArray();
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
			if (EditorUtils.Button("Align Vertical", "toolbarbutton"))
			{
				CountAlgo();
			}
			if (EditorUtils.Button("Align Horizontal", "toolbarbutton"))
			{
				DisableAlgo();
			}
		}
		EditorGUI.EndDisabledGroup();
		EditorGUI.BeginDisabledGroup(num < 1);
		using (new GUILayout.HorizontalScope())
		{
			if (EditorUtils.Button("Up 0.25", "toolbarbutton"))
			{
				ChildAnimatorState[] states = ActiveStateMachine().states;
				for (int num6 = 0; num6 < states.Length; num6++)
				{
					if (selectedStates.Contains(states[num6].state))
					{
						states[num6].position += Vector3.down * 3f;
					}
				}
				if (entryNodeSelected)
				{
					ActiveStateMachine().entryPosition += Vector3.down * 3f;
					InterruptAlgo(wantfirst: false);
				}
				if (anyStateNodeSelected)
				{
					ActiveStateMachine().anyStatePosition += Vector3.down * 3f;
				}
				if (exitNodeSelected)
				{
					ActiveStateMachine().exitPosition += Vector3.down * 3f;
				}
				ActiveStateMachine().states = states;
				EditorUtility.SetDirty(ActiveController());
				if (entryNodeSelected || exitNodeSelected)
				{
					InterruptAlgo(wantfirst: false);
				}
			}
			if (EditorUtils.Button("Right 0.25", "toolbarbutton"))
			{
				ChildAnimatorState[] states2 = ActiveStateMachine().states;
				for (int num7 = 0; num7 < states2.Length; num7++)
				{
					if (selectedStates.Contains(states2[num7].state))
					{
						states2[num7].position += Vector3.right * 3f;
					}
				}
				if (entryNodeSelected)
				{
					ActiveStateMachine().entryPosition += Vector3.right * 3f;
				}
				if (anyStateNodeSelected)
				{
					ActiveStateMachine().anyStatePosition += Vector3.right * 3f;
				}
				if (exitNodeSelected)
				{
					ActiveStateMachine().exitPosition += Vector3.right * 3f;
				}
				ActiveStateMachine().states = states2;
				EditorUtility.SetDirty(ActiveController());
				if (entryNodeSelected || exitNodeSelected)
				{
					InterruptAlgo(wantfirst: false);
				}
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	private static void CalcVisitor()
	{
		if (selectedStates.Count >= 1)
		{
			selectedStatesSerialized.Update();
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(EditorUtils.contents().animatorStates, GUILayout.Width(35f), GUILayout.Height(35f));
				using (new GUILayout.VerticalScope())
				{
					using (new MixedValueScope(stateNameProperty))
					{
						EditorGUI.BeginChangeCheck();
						string serv = EditorGUILayout.DelayedTextField(string.Empty, stateNameProperty.stringValue);
						if (EditorGUI.EndChangeCheck())
						{
							RestartAlgo(ActiveStateMachine(), selectedStates, serv);
						}
					}
					EditorGUIUtility.labelWidth = 35f;
					EditorGUILayout.PropertyField(stateTagProperty);
					EditorGUIUtility.labelWidth = 0f;
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(stateMotionProperty);
			EditorGUILayout.PropertyField(stateSpeedProperty);
			PrepareMapper("Multiplier", null, stateSpeedParameterProperty, stateSpeedParameterActiveProperty, floatParameterNames, res4stop: true);
			PrepareMapper("Motion Time", null, stateTimeParameterProperty, stateTimeParameterActiveProperty, floatParameterNames);
			PrepareMapper("Mirror", stateMirrorProperty, stateMirrorParameterProperty, stateMirrorParameterActiveProperty, boolParameterNames);
			PrepareMapper("Cycle Offset", stateCycleOffsetProperty, stateCycleOffsetParameterProperty, stateCycleOffsetParameterActiveProperty, floatParameterNames);
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(stateWriteDefaultsProperty, new GUIContent("Write Defaults"));
				EditorGUILayout.PropertyField(stateIkOnFeetProperty, new GUIContent("Foot IK"));
			}
			selectedStatesSerialized.ApplyModifiedProperties();
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
			GUILayout.Label(EditorUtils.contents().animatorStates, GUILayout.Height(35f), GUILayout.Width(35f));
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
		EditorGUI.BeginDisabledGroup(selectedStates.Count < 1);
		parameterDriverList.DoLayoutList();
		EditorGUI.EndDisabledGroup();
	}

	internal static void CloneVisitor()
	{
		if (allStatesHaveTrackingControl)
		{
			trackingControlEditor.Draw();
			return;
		}
		using (new GUILayout.HorizontalScope("in bigtitle"))
		{
			EditorGUI.BeginDisabledGroup(selectedStates.Count <= 0);
			if (EditorUtils.Button("Add Tracking to Selected States"))
			{
				ConcatAnnotation();
			}
			EditorGUI.EndDisabledGroup();
		}
	}

	private static void LoginVisitor()
	{
		if (selectedStatesSerialized != null)
		{
			stateNameProperty = selectedStatesSerialized.FindProperty("m_Name");
			stateTagProperty = selectedStatesSerialized.FindProperty("m_Tag");
			stateMotionProperty = selectedStatesSerialized.FindProperty("m_Motion");
			stateSpeedProperty = selectedStatesSerialized.FindProperty("m_Speed");
			stateSpeedParameterProperty = selectedStatesSerialized.FindProperty("m_SpeedParameter");
			stateTimeParameterProperty = selectedStatesSerialized.FindProperty("m_TimeParameter");
			stateMirrorProperty = selectedStatesSerialized.FindProperty("m_Mirror");
			stateCycleOffsetProperty = selectedStatesSerialized.FindProperty("m_CycleOffset");
			stateIkOnFeetProperty = selectedStatesSerialized.FindProperty("m_IKOnFeet");
			stateWriteDefaultsProperty = selectedStatesSerialized.FindProperty("m_WriteDefaultValues");
			stateSpeedParameterActiveProperty = selectedStatesSerialized.FindProperty("m_SpeedParameterActive");
			stateTimeParameterActiveProperty = selectedStatesSerialized.FindProperty("m_TimeParameterActive");
			stateMirrorParameterActiveProperty = selectedStatesSerialized.FindProperty("m_MirrorParameterActive");
			stateCycleOffsetParameterActiveProperty = selectedStatesSerialized.FindProperty("m_CycleOffsetParameterActive");
			stateMirrorParameterProperty = selectedStatesSerialized.FindProperty("m_MirrorParameter");
			stateCycleOffsetParameterProperty = selectedStatesSerialized.FindProperty("m_CycleOffsetParameter");
		}
	}

	private void DrawTransitionSection()
	{
		if (transitionSectionVisible && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y p,A|I}oK^.}}< HC<dus8CGLPT6AdJi/Z)jj=*mX4V2# &x8Au~4ajsR# 27*Bh{F/o NM{aX4:jl4D/ N.gqjC.-kUtO'++JQF>?S+_)ieHv)O?`1EJ-w[" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			string token = $"Transition Count: {selectedTransitionEdits.Count}";
			DrawCollapsibleSection(DeleteVisitor, token, EditorSettings.GetInstance().showTransitionsCount, iscont2: true, 0);
			DrawCollapsibleSection(CreateVisitor, "Transition Settings", EditorSettings.GetInstance().showTransitionSettings, iscont2: true, 1);
			DrawCollapsibleSection(NewVisitor, "Transition Conditions", EditorSettings.GetInstance().showTransitionConditions, iscont2: false, 2);
		}
	}

	private void DeleteVisitor()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(25f)))
			{
				Selection.objects = Selection.objects.Except(selectedTransitions).ToArray();
			}
			GUILayout.Label($"Editing {selectedTransitionEdits.Count} Transitions");
			GUILayout.FlexibleSpace();
		}
		EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!hasPlainTransitionSelected && !hasStateTransitionSelected);
		try
		{
			int num = Mathf.CeilToInt((float)selectedTransitionEdits.Count / 3f);
			int num2 = 0;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			if (hasPlainTransitionSelected || hasStateTransitionSelected)
			{
				foreach (AnimatorGraphReflection.TransitionEditionInfo item in selectedTransitionEdits.Where((AnimatorGraphReflection.TransitionEditionInfo et) => et.transition != null))
				{
					if (num2 == num)
					{
						EditorGUILayout.EndVertical();
						EditorGUILayout.BeginVertical();
						num2 = 0;
					}
					using (new GUILayout.HorizontalScope())
					{
						if (EditorUtils.Button(EditorUtils.contents().deselect, EditorUtils.styles().centeredIcon, GUILayout.Width(25f)))
						{
							UnityEngine.Object[] array = Selection.objects;
							ArrayUtility.Remove(ref array, item.transition);
							Selection.objects = array;
						}
						bool flag = item.transition == focusedTransition.transition;
						if (EditorUtils.Button(item.DisplayName(), flag ? EditorUtils.styles().linkLabel : GUI.skin.label, GUILayout.MinWidth(1f)))
						{
							if (!flag)
							{
								focusedTransition = item;
								showSharedConditions = true;
								MapVisitor();
								RunAlgo();
								CalculateAnnotation();
							}
							else
							{
								focusedTransition = default(AnimatorGraphReflection.TransitionEditionInfo);
								SyncSelection();
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
		EditorGUI.BeginDisabledGroup(!hasStateTransitionSelected);
		transitionInspectorSerialized.Update();
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label((!focusedTransition.stateTransition) ? string.Empty : (focusedTransition.DisplayName() + "'s Settings"), GUILayout.ExpandWidth(expand: true));
			if ((selectedStateTransitions.Count == 1 || (bool)focusedTransition.stateTransition) && EditorUtils.Button(EditorUtils.contents().copy, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
			{
				if (copiedTransitionSettings == null)
				{
					copiedTransitionSettings = new AnimatorStateTransition();
				}
				CustomizeAlgo((!focusedTransition.stateTransition) ? selectedStateTransitions[0] : focusedTransition.stateTransition, copiedTransitionSettings);
			}
			using (new EditorGUI.DisabledScope(!copiedTransitionSettings))
			{
				if (EditorUtils.Button(EditorUtils.contents().paste, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
				{
					for (int i = 0; i < selectedStateTransitions.Count; i++)
					{
						Undo.RecordObject(selectedStateTransitions[i], "PasteSettings");
						CustomizeAlgo(copiedTransitionSettings, selectedStateTransitions[i]);
					}
				}
			}
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(transitionHasExitTimeProperty);
			EditorGUI.BeginDisabledGroup(!transitionHasExitTimeProperty.boolValue);
			EditorGUILayout.PropertyField(transitionExitTimeProperty);
			EditorGUI.EndDisabledGroup();
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(transitionHasFixedDurationProperty);
			EditorGUILayout.PropertyField(transitionDurationProperty);
		}
		EditorGUILayout.PropertyField(transitionOffsetProperty);
		EditorGUILayout.PropertyField(transitionInterruptionSourceProperty);
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(transitionOrderedInterruptionProperty);
			EditorGUILayout.PropertyField(transitionMuteProperty);
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(transitionCanTransitionToSelfProperty);
			EditorGUILayout.PropertyField(transitionSoloProperty);
		}
		transitionInspectorSerialized.ApplyModifiedProperties();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUI.EndDisabledGroup();
	}

	private void NewVisitor()
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			if ((bool)EditorSettings.GetInstance().showMatchingOptions)
			{
				using (new GUILayout.HorizontalScope())
				{
					using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
					EditorSettings.GetInstance().matchParameter.DrawButton("Match Parameter", "Ignore Parameter", true, Color.green, Color.red);
					EditorSettings.GetInstance().matchMode.DrawButton("Match Mode", "Ignore Mode", true, Color.green, Color.red);
					EditorSettings.GetInstance().matchValue.DrawButton("Match Value", "Ignore Value", true, Color.green, Color.red);
					if (changeCheckScope.changed)
					{
						UpdateVisitor();
					}
				}
			}
			using (new EditorGUI.DisabledScope(selectedTransitionEdits.Count == 0))
			{
				ReorderableList reorderableList = (HasFocusedTransition() ? focusedConditionList : ((!showSharedConditions) ? allConditionList : sharedConditionList));
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
		if (transitionInspectorSerialized != null)
		{
			SerializedObject serializedObject = transitionInspectorSerialized;
			transitionHasExitTimeProperty = serializedObject.FindProperty("m_HasExitTime");
			transitionExitTimeProperty = serializedObject.FindProperty("m_ExitTime");
			transitionHasFixedDurationProperty = serializedObject.FindProperty("m_HasFixedDuration");
			transitionDurationProperty = serializedObject.FindProperty("m_TransitionDuration");
			transitionOffsetProperty = serializedObject.FindProperty("m_TransitionOffset");
			transitionInterruptionSourceProperty = serializedObject.FindProperty("m_InterruptionSource");
			transitionOrderedInterruptionProperty = serializedObject.FindProperty("m_OrderedInterruption");
			transitionCanTransitionToSelfProperty = serializedObject.FindProperty("m_CanTransitionToSelf");
			transitionSoloProperty = serializedObject.FindProperty("m_Solo");
			transitionMuteProperty = serializedObject.FindProperty("m_Mute");
		}
	}

	[CallbackMethod(0)]
	private static void ViewVisitor()
	{
		List<Type> list = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany((System.Reflection.Assembly a) => a.GetTypes())
			where t.IsSubclassOf(typeof(Component)) && !t.IsAbstract && !t.IsGenericTypeDefinition
			select t).ToList();
		list.Add(typeof(GameObject));
		componentTypes = list.OrderBy((Type t) => t.Name).ToArray();
	}

	private static string[] CollectVisitor(Shader var1)
	{
		if (materialPropertiesByShader.TryGetValue(var1, out var value))
		{
			return value;
		}
		GameObject gameObject = new GameObject();
		Material material = new Material(var1);
		try
		{
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
			string[] array = (from b in AnimationUtility.GetAnimatableBindings(gameObject, gameObject)
				where b.propertyName.StartsWith("material.") && b.type.Is<Renderer>()
				select b.propertyName into p
				orderby p
				select p).ToArray();
			materialPropertiesByShader.Add(var1, array);
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
			if (!animatablePropertiesByType.TryGetValue(item, out var value))
			{
				if (item.IsSubclassOf(typeof(Component)))
				{
					InstantiateInitializer().GetOrAddComponent(item);
				}
				string[] array = (from b in AnimationUtility.GetAnimatableBindings(InstantiateInitializer(), InstantiateInitializer())
					where b.type == item
					select b.propertyName into s
					orderby s
					select s).ToArray();
				animatablePropertiesByType.Add(item, array);
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
			if (!animatablePropertiesByType.TryGetValue(var1, out var value))
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
		if (sharedConditionEditors.Count <= 0)
		{
			if (ActiveController() != null)
			{
				if (ActiveController().parameters.Length == 0)
				{
					animatorCondition.parameter = "New Parameter";
				}
				else
				{
					if (ActiveController().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Bool && ActiveController().parameters[0].type != UnityEngine.AnimatorControllerParameterType.Trigger)
					{
						animatorCondition.mode = AnimatorConditionMode.Equals;
					}
					else
					{
						animatorCondition.mode = AnimatorConditionMode.If;
					}
					animatorCondition.parameter = ActiveController().parameters[0].name;
					animatorCondition.threshold = 0f;
				}
			}
		}
		else
		{
			animatorCondition = sharedConditionEditors.Last().condition;
		}
		if (!HasFocusedTransition())
		{
			for (int i = 0; i < selectedTransitions.Count; i++)
			{
				selectedTransitions[i].AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
			}
		}
		else
		{
			focusedTransition.transition.AddCondition(animatorCondition.mode, animatorCondition.threshold, animatorCondition.parameter);
		}
		sharedConditionEditors = AssetVisitor(selectedTransitions);
		MapVisitor();
	}

	private static bool WriteVisitor(AnimatorCondition value, AnimatorCondition counter, bool isres, out bool[] reference2)
	{
		reference2 = new bool[3];
		if (!ActiveController())
		{
			return false;
		}
		isres |= !EditorSettings.GetInstance().showMatchingOptions;
		reference2[0] = value.parameter == counter.parameter;
		if ((!isres && !EditorSettings.GetInstance().matchParameter) || reference2[0])
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
					if ((!isres && (type != UnityEngine.AnimatorControllerParameterType.Bool || !EditorSettings.GetInstance().matchValue) && (type == UnityEngine.AnimatorControllerParameterType.Bool || !EditorSettings.GetInstance().matchMode)) || reference2[1])
					{
						if (type != UnityEngine.AnimatorControllerParameterType.Bool)
						{
							reference2[2] = value.threshold == counter.threshold;
							if (isres || (bool)EditorSettings.GetInstance().matchValue)
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

	private static List<ConditionMultiEditor> StopVisitor()
	{
		if (HasFocusedTransition())
		{
			return focusedConditionEditors;
		}
		if (showSharedConditions)
		{
			return sharedConditionEditors;
		}
		return allConditionEditors;
	}

	private static List<ConditionMultiEditor> CheckVisitor(AnimatorTransitionBase ident, List<ConditionMultiEditor> reg)
	{
		for (int i = 0; i < ident.conditions.Length; i++)
		{
			foreach (ConditionMultiEditor item in reg.Where((ConditionMultiEditor sc) => !sc.matched))
			{
				if (WriteVisitor(item.condition, ident.conditions[i], isres: false, out var reference))
				{
					item.AddMatch(ident, i);
					item.MarkMixedValues(reference);
					break;
				}
			}
		}
		List<ConditionMultiEditor> list = new List<ConditionMultiEditor>();
		foreach (ConditionMultiEditor item2 in reg)
		{
			if (!item2.matched)
			{
				list.Add(item2);
			}
			item2.matched = false;
		}
		reg = reg.Except(list).ToList();
		return reg;
	}

	private static List<ConditionMultiEditor> PrepareVisitor(AnimatorTransitionBase config)
	{
		List<ConditionMultiEditor> list = new List<ConditionMultiEditor>();
		for (int i = 0; i < config.conditions.Length; i++)
		{
			list.Add(new ConditionMultiEditor(config, i));
		}
		return list;
	}

	private static List<ConditionMultiEditor> AssetVisitor(List<AnimatorTransitionBase> ident)
	{
		if (ident.Count != 0)
		{
			List<ConditionMultiEditor> list = PrepareVisitor(ident[0]);
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
		return new List<ConditionMultiEditor>();
	}

	private static void UpdateVisitor()
	{
		sharedConditionEditors = AssetVisitor(selectedTransitions);
		MapVisitor();
	}

	private static void ChangeVisitor()
	{
		List<ConditionMultiEditor> list = new List<ConditionMultiEditor>();
		List<AnimatorTransitionBase> list2 = new List<AnimatorTransitionBase>();
		for (int i = 0; i < allConditionEditors.Count; i++)
		{
			AnimatorTransitionBase item = allConditionEditors[i].targets[0].Item1;
			if (!list2.Contains(item))
			{
				list2.Add(item);
				for (int j = 0; j < item.conditions.Length; j++)
				{
					list.Add(new ConditionMultiEditor(item, j));
				}
			}
		}
		allConditionEditors = list;
	}

	private static void SortVisitor()
	{
		List<AnimatorCondition> list;
		if (HasFocusedTransition())
		{
			list = focusedTransition.transition.conditions.ToList();
		}
		else
		{
			while (!showSharedConditions)
			{
			}
			list = sharedConditionEditors.Select((ConditionMultiEditor sc) => sc.condition).ToList();
		}
		copiedConditions = list;
	}

	private static void RegisterVisitor()
	{
		if (!HasFocusedTransition())
		{
			foreach (AnimatorGraphReflection.TransitionEditionInfo selectedTransitionEdit in selectedTransitionEdits)
			{
				foreach (AnimatorCondition copiedCondition in copiedConditions)
				{
					selectedTransitionEdit.transition.AddCondition(copiedCondition.mode, copiedCondition.threshold, copiedCondition.parameter);
					allConditionEditors.Add(new ConditionMultiEditor(selectedTransitionEdit.transition, selectedTransitionEdit.transition.conditions.Length - 1));
				}
			}
		}
		else
		{
			foreach (AnimatorCondition copiedCondition2 in copiedConditions)
			{
				focusedTransition.transition.AddCondition(copiedCondition2.mode, copiedCondition2.threshold, copiedCondition2.parameter);
				focusedConditionEditors.Add(new ConditionMultiEditor(focusedTransition.transition, focusedTransition.transition.conditions.Length - 1));
			}
		}
		sharedConditionEditors = AssetVisitor(selectedTransitions);
		ChangeVisitor();
		MapVisitor();
	}

	private void LogoutVisitor()
	{
		switch (selectedAction)
		{
		case ControllerAction.RemoveTag:
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array3 = PatchVisitor((int)actionScope);
			foreach (UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer2 in array3)
			{
				foreach (AnimatorStateTransition item in animatorControllerLayer2.stateMachine.anyStateTransitions.Where((AnimatorStateTransition t) => t.name == actionSourceName && t.isExit))
				{
					animatorControllerLayer2.stateMachine.RemoveAnyStateTransition(item);
				}
			}
			FlushAnnotation();
			break;
		}
		case ControllerAction.TagCurrentLayerWith:
			if (!RootStateMachine().anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText && t.isExit))
			{
				AnimatorStateTransition animatorStateTransition = RootStateMachine().AddAnyStateTransition((AnimatorState)null);
				animatorStateTransition.isExit = true;
				animatorStateTransition.mute = true;
				animatorStateTransition.name = actionFilterText;
				FlushAnnotation();
			}
			break;
		case ControllerAction.ReplaceParameter:
		{
			bool last2;
			AnimatorStateMachine[] array4 = InterruptVisitor(out last2);
			for (int num3 = 0; num3 < array4.Length; num3++)
			{
				SetAlgo(array4[num3], actionSourceName, actionReplacementName, matchWholeWord, last2);
			}
			if (actionScope != ActionMode.CurrentController)
			{
				break;
			}
			UnityEngine.AnimatorControllerParameter[] parameters = ActiveController().parameters;
			for (int num4 = ActiveController().parameters.Length - 1; num4 >= 0; num4--)
			{
				if (UpdateMapper(ActiveController().parameters[num4].name))
				{
					parameters[num4].name = parameters[num4].name.Replace(actionSourceName, actionReplacementName);
				}
			}
			ActiveController().parameters = parameters;
			break;
		}
		case ControllerAction.RemoveParameter:
		{
			bool last;
			AnimatorStateMachine[] array2 = InterruptVisitor(out last);
			for (int num = 0; num < array2.Length; num++)
			{
				SetupAlgo(array2[num], actionSourceName, last);
			}
			if (actionScope != ActionMode.CurrentController)
			{
				break;
			}
			for (int num2 = ActiveController().parameters.Length - 1; num2 >= 0; num2--)
			{
				if (AssetMapper(ActiveController().parameters[num2].name))
				{
					ActiveController().RemoveParameter(num2);
				}
			}
			break;
		}
		case ControllerAction.RemoveLayersWithTag:
		{
			for (int num5 = ActiveController().layers.Length - 1; num5 >= 0; num5--)
			{
				if (ActiveController().layers[num5].stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText))
				{
					ActiveController().RemoveLayer(num5);
				}
			}
			break;
		}
		case ControllerAction.Copy:
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = PatchVisitor((int)copySourceScope);
			UnityEditor.Animations.AnimatorController serializerReg = ((copyDestination != MoveDestination.Controller) ? ActiveController() : actionTargetController);
			for (int i = 0; i < array.Length; i++)
			{
				UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = EditorUtils.CopyLayer(array[i], serializerReg);
				if (animatorControllerLayer == null)
				{
					continue;
				}
				if (addRequiredParameters)
				{
					EditorUtils.ForEach(ManageVisitor(animatorControllerLayer), delegate(UnityEngine.AnimatorControllerParameter p)
					{
						_003C_003Ec__DisplayClass370_1 _003C_003Ec__DisplayClass370_ = new _003C_003Ec__DisplayClass370_1();
						if (p != null)
						{
							_003C_003Ec__DisplayClass370_._ResolverReg = ((!EditorUtils.reservedAvatarParameters.Contains(p.name)) ? (p.name + copiedParameterSuffix) : p.name);
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
				if (!string.IsNullOrEmpty(copiedParameterSuffix))
				{
					EnableAlgo(animatorControllerLayer, copiedParameterSuffix);
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
			result = new UnityEditor.Animations.AnimatorControllerLayer[1] { ActiveController().layers.First((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine == RootStateMachine()) };
			break;
		case 1:
			result = ActiveController().layers.Where((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText && t.isExit)).ToArray();
			break;
		case 0:
			result = ActiveController().layers;
			break;
		}
		return result;
	}

	private static AnimatorStateMachine[] InterruptVisitor(out bool last)
	{
		AnimatorStateMachine[] result = null;
		switch (actionScope)
		{
		case ActionMode.CurrentStatemachine:
			last = false;
			result = new AnimatorStateMachine[1] { ActiveStateMachine() };
			break;
		default:
			last = true;
			break;
		case ActionMode.CurrentController:
			last = true;
			result = ActiveController().layers.Select((UnityEditor.Animations.AnimatorControllerLayer l) => l.stateMachine).ToArray();
			break;
		case ActionMode.CurrentLayer:
			last = true;
			result = new AnimatorStateMachine[1] { RootStateMachine() };
			break;
		case ActionMode.LayersTaggedWith:
			last = true;
			result = (from l in ActiveController().layers
				where l.stateMachine.anyStateTransitions.Any((AnimatorStateTransition t) => t.name == actionFilterText && t.isExit)
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
			s.transitions.ForEach(delegate(AnimatorStateTransition t)
			{
				t.conditions.ForEach(delegate(AnimatorCondition c)
				{
					if (!string.IsNullOrEmpty(c.parameter))
					{
						predicateReg.Add(ResetAnnotation(c.parameter, out var _));
					}
				});
			});
			if (AnimatorTypeCache.IsVRCSDKAvailable())
			{
				s.behaviours.ForEach(delegate(StateMachineBehaviour b)
				{
					if (b.GetType() == AnimatorTypeCache.GetParameterDriverType())
					{
						EditorUtils.ForEach(new AnimatorTypeCache.ParameterDriverBinding(b).parameters, delegate(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry p)
						{
							if (!string.IsNullOrEmpty(p.GetName()))
							{
								predicateReg.Add(ResetAnnotation(p.GetName(), out var _));
							}
						});
					}
				});
			}
		});
		spec.anyStateTransitions.ForEach(delegate(AnimatorStateTransition t)
		{
			t.conditions.ForEach(delegate(AnimatorCondition c)
			{
				if (!string.IsNullOrEmpty(c.parameter))
				{
					predicateReg.Add(ResetAnnotation(c.parameter, out var _));
				}
			});
		});
		spec.entryTransitions.ForEach(delegate(AnimatorTransition t)
		{
			t.conditions.ForEach(delegate(AnimatorCondition c)
			{
				if (!string.IsNullOrEmpty(c.parameter))
				{
					predicateReg.Add(ResetAnnotation(c.parameter, out var _));
				}
			});
		});
		if (AnimatorTypeCache.IsVRCSDKAvailable())
		{
			spec.behaviours.ForEach(delegate(StateMachineBehaviour b)
			{
				if (b.GetType() == AnimatorTypeCache.GetParameterDriverType())
				{
					EditorUtils.ForEach(new AnimatorTypeCache.ParameterDriverBinding(b).parameters, delegate(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry p)
					{
						if (!string.IsNullOrEmpty(p.GetName()))
						{
							predicateReg.Add(ResetAnnotation(p.GetName(), out var _));
						}
					});
				}
			});
		}
		if (extractpol)
		{
			spec.stateMachines.ForEach(delegate(ChildAnimatorStateMachine c)
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
		init.layers.ForEach(delegate(UnityEditor.Animations.AnimatorControllerLayer l)
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
		ActiveController().ForEachRootStateMachine(delegate(AnimatorStateMachine l)
		{
			l.ForEachStateMachine(delegate(AnimatorStateMachine m)
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

	[CallbackMethod(0)]
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
		layerTemplateControllers = list.ToArray();
		layerTemplateNames = list2.ToArray();
	}

	private static void OrderAlgo(UnityEditor.Animations.AnimatorController init, UnityEditor.Animations.AnimatorController ord)
	{
		ParameterRenameWindow parameterRenameWindow = ParameterRenameWindow.ResolveTests(init, ord, !Event.current.control && !Event.current.shift);
		try
		{
			EditorWindow editorWindow = AnimatorGraphReflection.GraphAccessors.Tool();
			Vector2 reg = parameterRenameWindow.FillTests();
			Vector2 center = editorWindow.position.center;
			Vector2 setup = new Vector2(center.x - reg.x / 2f, center.y - reg.y / 2f);
			parameterRenameWindow.ShowAt(setup, reg);
		}
		catch
		{
			Vector2 reg2 = parameterRenameWindow.FillTests();
			Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			Vector2 setup2 = new Vector2(vector.x - reg2.x / 2f, vector.y - reg2.y / 2f);
			parameterRenameWindow.ShowAt(setup2, reg2);
		}
	}

	private static UnityEditor.Animations.AnimatorControllerLayer[] CompareAlgo(UnityEditor.Animations.AnimatorController spec, UnityEditor.Animations.AnimatorController ord, (UnityEngine.AnimatorControllerParameter, string)[] dir)
	{
		_003C_003Ec__DisplayClass378_0 CS_0024_003C_003E8__locals14 = new _003C_003Ec__DisplayClass378_0();
		CS_0024_003C_003E8__locals14.m_ProductReg = ord;
		CS_0024_003C_003E8__locals14._CandidateReg = new Dictionary<string, string>();
		EditorUtils.CopyLayers(spec, CS_0024_003C_003E8__locals14.m_ProductReg, out var dir2);
		foreach (UnityEngine.AnimatorControllerParameter item3 in spec.parameters.Where((UnityEngine.AnimatorControllerParameter p) => EditorUtils.reservedAvatarParameters.Contains(p.name)))
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
		if (AnimatorTypeCache.IsVRCSDKAvailable())
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
			if (AnimatorTypeCache.IsVRCSDKAvailable())
			{
				CS_0024_003C_003E8__locals15.CollectServer(s.behaviours);
			}
		}, requiresc: false);
		param.ForEachTransition(delegate(AnimatorStateTransitionSet t)
		{
			for (int num = t.GetConditions().Length - 1; num >= 0; num--)
			{
				AnimatorCondition[] conditions = t.GetConditions();
				conditions[num].parameter = CS_0024_003C_003E8__locals15.ViewServer(conditions[num].parameter);
				t.SetConditions(conditions);
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
		init.ForEachStateMachine(delegate(AnimatorStateMachine m)
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
		if (AnimatorTypeCache.IsVRCSDKAvailable())
		{
			CS_0024_003C_003E8__locals22._AttrReg = delegate(StateMachineBehaviour[] b)
			{
				foreach (StateMachineBehaviour stateMachineBehaviour in b)
				{
					if (!(stateMachineBehaviour.GetType() != AnimatorTypeCache.GetParameterDriverType()))
					{
						AnimatorTypeCache.ParameterDriverBinding parameterDriverBinding = new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour);
						for (int num2 = parameterDriverBinding.parameters.Count - 1; num2 >= 0; num2--)
						{
							if (CS_0024_003C_003E8__locals22.CheckServer(parameterDriverBinding.parameters[num2].GetName()))
							{
								parameterDriverBinding.RemoveParameter(num2);
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
			if (AnimatorTypeCache.IsVRCSDKAvailable())
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
		if (AnimatorTypeCache.IsVRCSDKAvailable())
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
				if (stateMachineBehaviour.GetType() == AnimatorTypeCache.GetParameterDriverType())
				{
					foreach (AnimatorTypeCache.ParameterDriverBinding.ParameterEntry parameter in new AnimatorTypeCache.ParameterDriverBinding(stateMachineBehaviour).parameters)
					{
						parameter.SetName(CS_0024_003C_003E8__locals9.RegisterServer(parameter.GetName()));
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
			animatorStateTransition.SetDirty();
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
		if ((bool)EditorSettings.GetInstance().warnParameterConversion)
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
				EditorSettings.GetInstance().warnParameterConversion.SetValue(excludeparam: false);
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
							int advisorReg = animatorTransitionBase2.conditions.FindIndex((AnimatorCondition c) => c.parameter == vis);
							if (advisorReg == -1)
							{
								continue;
							}
							AnimatorCondition animatorCondition = animatorTransitionBase2.conditions[advisorReg];
							if (animatorCondition.mode != mode && ((mode == AnimatorConditionMode.Less && animatorCondition.threshold <= array[infoReg].threshold + num2) | (mode == AnimatorConditionMode.Greater && animatorCondition.threshold >= array[infoReg].threshold - num2)))
							{
								IEnumerable<AnimatorCondition> key = array.Where((AnimatorCondition _, int index) => index != infoReg);
								IEnumerable<AnimatorCondition> b = animatorTransitionBase2.conditions.Where((AnimatorCondition _, int index) => index != advisorReg);
								if (EditorUtils.ConditionSetsMatch(key, b))
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
		makeMultipleTransitionsMode = !makeMultipleTransitionsMode;
		if (!makeMultipleTransitionsMode)
		{
			ConcatAlgo();
			return;
		}
		pendingTransitionEdits.Clear();
		redirectTransitionsMode = false;
		replicateTransitionsMode = false;
	}

	private static void ConcatAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		if (selectedStates.Count <= 0 && selectedStateMachines.Length == 0 && !exitNodeSelected)
		{
			return;
		}
		AnimatorStateMachine[] array;
		foreach (AnimatorState multiTransitionState in multiTransitionStates)
		{
			foreach (AnimatorState selectedState in selectedStates)
			{
				list.Add(multiTransitionState.AddTransition(selectedState));
			}
			array = selectedStateMachines;
			foreach (AnimatorStateMachine destinationStateMachine in array)
			{
				list.Add(multiTransitionState.AddTransition(destinationStateMachine));
			}
			if (exitNodeSelected)
			{
				list.Add(multiTransitionState.AddExitTransition());
			}
		}
		if (anyStateNodeSelected)
		{
			foreach (AnimatorState selectedState2 in selectedStates)
			{
				list.Add(RootStateMachine().AddAnyStateTransition(selectedState2));
			}
			array = selectedStateMachines;
			foreach (AnimatorStateMachine destinationStateMachine2 in array)
			{
				list.Add(RootStateMachine().AddAnyStateTransition(destinationStateMachine2));
			}
		}
		if (entryNodeSelected)
		{
			foreach (AnimatorState selectedState3 in selectedStates)
			{
				list.Add(ActiveStateMachine().AddEntryTransition(selectedState3));
			}
			array = selectedStateMachines;
			foreach (AnimatorStateMachine destinationStateMachine3 in array)
			{
				list.Add(ActiveStateMachine().AddEntryTransition(destinationStateMachine3));
			}
		}
		array = multiTransitionStateMachines;
		foreach (AnimatorStateMachine sourceStateMachine in array)
		{
			foreach (AnimatorState selectedState4 in selectedStates)
			{
				list.Add(ActiveStateMachine().AddStateMachineTransition(sourceStateMachine, selectedState4));
			}
			AnimatorStateMachine[] array2 = selectedStateMachines;
			foreach (AnimatorStateMachine destinationStateMachine4 in array2)
			{
				list.Add(ActiveStateMachine().AddStateMachineTransition(sourceStateMachine, destinationStateMachine4));
			}
			if (exitNodeSelected)
			{
				list.Add(ActiveStateMachine().AddStateMachineExitTransition(sourceStateMachine));
			}
		}
		list.ForEach(delegate(AnimatorTransitionBase t)
		{
			RateAlgo(EditorSettings.GetInstance().defaultTransition, t);
		});
		UnityEngine.Object[] objects = list.ToArray();
		Selection.objects = objects;
		if (anyStateNodeSelected || entryNodeSelected)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void CallAlgo()
	{
		MethodInfo _PrototypeReg = typeof(AnimatorStateMachine).GetMethod("MoveState", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method = typeof(AnimatorStateMachine).GetMethod("MoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
		IEnumerable<ChildAnimatorState> first = ActiveStateMachine().states.Where((ChildAnimatorState c) => selectedStates.Contains(c.state));
		IEnumerable<ChildAnimatorStateMachine> first2 = ActiveStateMachine().stateMachines.Where((ChildAnimatorStateMachine c) => selectedStateMachines.Contains(c.stateMachine));
		Dictionary<UnityEngine.Object, Vector3> m_CallbackReg = new Dictionary<UnityEngine.Object, Vector3>();
		Vector3 indexerReg = Vector3.zero;
		int m_IssuerReg = 0;
		first.ForEach(delegate(ChildAnimatorState c)
		{
			m_CallbackReg.Add(c.state, c.position);
			indexerReg += c.position;
			m_IssuerReg++;
		});
		first2.ForEach(delegate(ChildAnimatorStateMachine c)
		{
			m_CallbackReg.Add(c.stateMachine, c.position);
			indexerReg += c.position;
			m_IssuerReg++;
		});
		indexerReg /= (float)m_IssuerReg;
		AnimatorStateMachine m_RuleReg = ActiveStateMachine().AddStateMachine("New StateMachine", indexerReg);
		selectedStates.ForEach(delegate(AnimatorState s)
		{
			_PrototypeReg.Invoke(ActiveStateMachine(), new object[2] { s, m_RuleReg });
		});
		AnimatorStateMachine[] array = selectedStateMachines;
		foreach (AnimatorStateMachine animatorStateMachine in array)
		{
			AnimatorTransition[] stateMachineTransitions = ActiveStateMachine().GetStateMachineTransitions(animatorStateMachine);
			ActiveStateMachine().SetStateMachineTransitions(animatorStateMachine, null);
			method.Invoke(ActiveStateMachine(), new object[2] { animatorStateMachine, m_RuleReg });
			AnimatorTransition[] array2 = ActiveStateMachine().GetStateMachineTransitions(m_RuleReg);
			ArrayUtility.AddRange(ref array2, stateMachineTransitions);
			ActiveStateMachine().SetStateMachineTransitions(m_RuleReg, array2);
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
		for (int i = 0; i < selectedStateMachines.Length; i++)
		{
			AnimatorStateMachine _SingletonReg = selectedStateMachines[i];
			if (!(ActiveStateMachine() == _SingletonReg))
			{
				Vector3 vector = ActiveStateMachine().stateMachines.First((ChildAnimatorStateMachine c) => c.stateMachine == _SingletonReg).position;
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
						ActiveStateMachine()
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
						ActiveStateMachine()
					});
				}
				ActiveStateMachine().RemoveStateMachine(_SingletonReg);
			}
		}
		ChildAnimatorState[] states2 = ActiveStateMachine().states;
		for (int num2 = 0; num2 < states2.Length; num2++)
		{
			if (dictionary.TryGetValue(states2[num2].state, out var value))
			{
				states2[num2].position = value;
			}
		}
		ActiveStateMachine().states = states2;
		ChildAnimatorStateMachine[] stateMachines2 = ActiveStateMachine().stateMachines;
		for (int num3 = 0; num3 < stateMachines2.Length; num3++)
		{
			if (dictionary.TryGetValue(stateMachines2[num3].stateMachine, out var value2))
			{
				stateMachines2[num3].position = value2;
			}
		}
		ActiveStateMachine().stateMachines = stateMachines2;
	}

	private static void CountAlgo()
	{
		ChildAnimatorState[] source = ActiveStateMachine().states.Where((ChildAnimatorState c) => selectedStates.Contains(c.state)).ToArray();
		float x = source.Max((ChildAnimatorState c) => c.position.x);
		ChildAnimatorState[] states = ActiveStateMachine().states;
		for (int num = 0; num < states.Length; num++)
		{
			if (source.Contains(states[num]))
			{
				states[num].position = new Vector3(x, states[num].position.y);
			}
		}
		if (entryNodeSelected)
		{
			ActiveStateMachine().entryPosition = new Vector3(x, ActiveStateMachine().entryPosition.y);
		}
		if (anyStateNodeSelected)
		{
			ActiveStateMachine().anyStatePosition = new Vector3(x, ActiveStateMachine().anyStatePosition.y);
		}
		if (exitNodeSelected)
		{
			ActiveStateMachine().exitPosition = new Vector3(x, ActiveStateMachine().exitPosition.y);
		}
		Undo.RecordObject(ActiveStateMachine(), "Align Horizontal");
		ActiveStateMachine().states = states;
		EditorUtility.SetDirty(ActiveController());
		if (entryNodeSelected || exitNodeSelected)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void DisableAlgo()
	{
		ChildAnimatorState[] source = ActiveStateMachine().states.Where((ChildAnimatorState c) => selectedStates.Contains(c.state)).ToArray();
		float y = source.Min((ChildAnimatorState c) => c.position.y);
		ChildAnimatorState[] states = ActiveStateMachine().states;
		for (int num = 0; num < states.Length; num++)
		{
			if (source.Contains(states[num]))
			{
				states[num].position = new Vector3(states[num].position.x, y);
			}
		}
		if (entryNodeSelected)
		{
			ActiveStateMachine().entryPosition = new Vector3(ActiveStateMachine().entryPosition.x, y);
		}
		if (anyStateNodeSelected)
		{
			ActiveStateMachine().anyStatePosition = new Vector3(ActiveStateMachine().anyStatePosition.x, y);
		}
		if (exitNodeSelected)
		{
			ActiveStateMachine().exitPosition = new Vector3(ActiveStateMachine().exitPosition.x, y);
		}
		Undo.RecordObject(ActiveStateMachine(), "Align Vertical");
		ActiveStateMachine().states = states;
		EditorUtility.SetDirty(ActiveController());
		if (entryNodeSelected || exitNodeSelected)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void InsertAlgo()
	{
		if (selectedStates.Count != 0)
		{
			Selection.objects = Selection.objects.Where((UnityEngine.Object o) => !(o is AnimatorState)).ToArray();
		}
		else
		{
			Selection.objects = Selection.objects.Concat(ActiveStateMachine().states.Select((ChildAnimatorState c) => c.state)).ToArray();
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
		AddAlgo(EditorSettings.GetInstance().defaultState, i);
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
		if (!setup.tag.IsNullOrWhiteSpace())
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
		return InvokeAlgo(asset).Any((string t) => cosmeticOnlyStyleNames.Contains(t));
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
		config.SetDirty();
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
			config.SetDirty();
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
		AnimatorState[] array = selectedStates.ToArray();
		AnimatorStateMachine[] array2 = selectedStateMachines;
		UnityEngine.Object[] objectsToUndo = array;
		Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
		objectsToUndo = array2;
		Undo.RecordObjects(objectsToUndo, "Paste Behaviours");
		StateMachineBehaviour[] array3 = @struct;
		foreach (StateMachineBehaviour stateMachineBehaviour in array3)
		{
			Type type = stateMachineBehaviour.GetType();
			AnimatorState[] array4 = array;
			foreach (AnimatorState animatorState in array4)
			{
				EditorUtility.CopySerialized(stateMachineBehaviour, animatorState.AddStateMachineBehaviour(type));
			}
			AnimatorStateMachine[] array5 = array2;
			foreach (AnimatorStateMachine animatorStateMachine in array5)
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
		AnimatorState[] array = selectedStates.ToArray();
		AnimatorStateMachine[] array2 = selectedStateMachines;
		UnityEngine.Object[] objectsToUndo = array;
		Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
		objectsToUndo = array2;
		Undo.RecordObjects(objectsToUndo, "Remove Behaviours");
		AnimatorState[] array3 = array;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].behaviours = Array.Empty<StateMachineBehaviour>();
		}
		AnimatorStateMachine[] array4 = array2;
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].behaviours = Array.Empty<StateMachineBehaviour>();
		}
	}

	private static List<AnimatorStateTransition> AwakeAlgo(List<AnimatorStateTransition> value = null)
	{
		if (value == null)
		{
			value = selectedStateTransitions;
		}
		return RootStateMachine().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
		{
			_003C_003Ec__DisplayClass410_1 _003C_003Ec__DisplayClass410_ = new _003C_003Ec__DisplayClass410_1();
			_003C_003Ec__DisplayClass410_._AccountReg = t;
			return value.Contains(_003C_003Ec__DisplayClass410_._AccountReg) && (ActiveStateMachine().states.Any(_003C_003Ec__DisplayClass410_.SetupThread) || ActiveStateMachine().stateMachines.Any(_003C_003Ec__DisplayClass410_.EnableThread));
		}).ToList();
	}

	private static List<AnimatorTransitionBase> ResetAlgo(List<AnimatorTransitionBase> info = null)
	{
		if (info == null)
		{
			info = selectedTransitions;
		}
		return info.Where((AnimatorTransitionBase t) => ActiveStateMachine().entryTransitions.Contains(t)).ToList();
	}

	private static (AnimatorState, List<AnimatorStateTransition>)[] FlushAlgo(List<AnimatorStateTransition> var1 = null)
	{
		if (var1 == null)
		{
			var1 = selectedStateTransitions;
		}
		List<AnimatorState> list = new List<AnimatorState>();
		for (int i = 0; i < ActiveStateMachine().states.Length; i++)
		{
			if (ActiveStateMachine().states[i].state.transitions.Any((AnimatorStateTransition t) => var1.Contains(t)))
			{
				list.Add(ActiveStateMachine().states[i].state);
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
		(AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(selectedStateTransitions);
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
		EditorUtility.SetDirty(ActiveController());
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
		AnimatorStateTransition animatorStateTransition = RootStateMachine().AddAnyStateTransition(i.destinationState);
		EditorUtility.CopySerialized(i, animatorStateTransition);
		animatorStateTransition.hideFlags = i.hideFlags;
		return animatorStateTransition;
	}

	private static AnimatorTransitionBase ValidateAlgo(AnimatorTransitionBase reference)
	{
		AnimatorTransitionBase animatorTransitionBase = ActiveStateMachine().AddEntryTransition(reference.destinationState);
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
		if (selectedStateTransitions.Count == 0)
		{
			List<AnimatorStateTransition> valReg = new List<AnimatorStateTransition>();
			ActiveStateMachine().AssetPredicate(delegate(AnimatorState s)
			{
				valReg.AddRange(s.transitions);
			}, requiresc: false);
			valReg.AddRange(RootStateMachine().anyStateTransitions.Where(delegate(AnimatorStateTransition t)
			{
				_003C_003Ec__DisplayClass420_1 _003C_003Ec__DisplayClass420_ = new _003C_003Ec__DisplayClass420_1();
				_003C_003Ec__DisplayClass420_._ValueReg = t;
				return ActiveStateMachine().states.Any(_003C_003Ec__DisplayClass420_.QueryThread);
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
		foreach (AnimatorState selectedState in selectedStates)
		{
			list.AddRange(selectedState.transitions);
		}
		if (entryNodeSelected)
		{
			list.AddRange(ActiveStateMachine().entryTransitions);
		}
		if (anyStateNodeSelected)
		{
			list.AddRange(ActiveStateMachine().anyStateTransitions);
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void CalcAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		ChildAnimatorState[] states = ActiveStateMachine().states;
		foreach (ChildAnimatorState childAnimatorState in states)
		{
			list.AddRange(childAnimatorState.state.transitions.Where(delegate(AnimatorStateTransition t)
			{
				if (selectedStates.Contains(t.destinationState))
				{
					return true;
				}
				return exitNodeSelected && t.isExit;
			}));
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void IncludeAlgo()
	{
		List<AnimatorTransitionBase> list = new List<AnimatorTransitionBase>();
		foreach (AnimatorState selectedState in selectedStates)
		{
			AnimatorStateTransition[] transitions = selectedState.transitions;
			foreach (AnimatorStateTransition animatorStateTransition in transitions)
			{
				if (selectedStates.Contains(animatorStateTransition.destinationState) || (animatorStateTransition.destinationState == null && exitNodeSelected))
				{
					list.Add(animatorStateTransition);
				}
			}
		}
		if (anyStateNodeSelected)
		{
			list.AddRange(ActiveStateMachine().anyStateTransitions.Where((AnimatorStateTransition t) => selectedStates.Contains(t.destinationState)));
		}
		if (entryNodeSelected)
		{
			list.AddRange(ActiveStateMachine().entryTransitions.Where((AnimatorTransition t) => selectedStates.Contains(t.destinationState)));
		}
		Selection.objects = Selection.objects.Concat(list).ToArray();
	}

	private static void RunAlgo()
	{
		SerializedObject serializedObject;
		if (!(focusedTransition.transition != null))
		{
			if (selectedStateTransitions.Count > 0)
			{
				UnityEngine.Object[] objs = selectedStateTransitions.ToArray();
				serializedObject = new SerializedObject(objs);
			}
			else
			{
				serializedObject = mixedValueTransitionSerialized;
			}
		}
		else
		{
			serializedObject = new SerializedObject(focusedTransition.transition);
		}
		transitionInspectorSerialized = serializedObject;
	}

	private static void CloneAlgo()
	{
		replaceTransitionsDefault = !replaceTransitionsDefault;
	}

	private static void LoginAlgo()
	{
		redirectTransitionsMode = !redirectTransitionsMode;
		if (redirectTransitionsMode)
		{
			replicateTransitionsMode = false;
			makeMultipleTransitionsMode = false;
		}
		else
		{
			ReflectAlgo();
		}
	}

	private static void ReflectAlgo()
	{
		if (selectedStates.Count <= 0 && !exitNodeSelected)
		{
			return;
		}
		bool flag = replaceTransitions;
		foreach (AnimatorGraphReflection.TransitionEditionInfo pendingTransitionEdit in pendingTransitionEdits)
		{
			foreach (AnimatorState selectedState in selectedStates)
			{
				switch (pendingTransitionEdit.sourceType)
				{
				case AnimatorGraphReflection.GraphNodeRef.NodeType.state:
				{
					AnimatorStateTransition animatorStateTransition = CalculateAlgo(pendingTransitionEdit.stateTransition);
					animatorStateTransition.isExit = false;
					animatorStateTransition.destinationState = selectedState;
					pendingTransitionEdit.sourceState.AddTransition(animatorStateTransition);
					break;
				}
				case AnimatorGraphReflection.GraphNodeRef.NodeType.entry:
					ValidateAlgo(pendingTransitionEdit.transition).destinationState = selectedState;
					break;
				case AnimatorGraphReflection.GraphNodeRef.NodeType.any:
					flag = true;
					MapAlgo(pendingTransitionEdit.stateTransition).destinationState = selectedState;
					break;
				}
			}
			if (exitNodeSelected && pendingTransitionEdit.sourceType == AnimatorGraphReflection.GraphNodeRef.NodeType.state)
			{
				AnimatorStateTransition animatorStateTransition2 = CalculateAlgo(pendingTransitionEdit.stateTransition);
				animatorStateTransition2.isExit = true;
				animatorStateTransition2.destinationState = null;
				pendingTransitionEdit.sourceState.AddTransition(animatorStateTransition2);
			}
		}
		if (replaceTransitions)
		{
			foreach (AnimatorGraphReflection.TransitionEditionInfo pendingTransitionEdit2 in pendingTransitionEdits)
			{
				switch (pendingTransitionEdit2.sourceType)
				{
				case AnimatorGraphReflection.GraphNodeRef.NodeType.any:
					RootStateMachine().RemoveAnyStateTransition(pendingTransitionEdit2.stateTransition);
					break;
				case AnimatorGraphReflection.GraphNodeRef.NodeType.state:
					pendingTransitionEdit2.sourceState.RemoveTransition(pendingTransitionEdit2.stateTransition);
					break;
				case AnimatorGraphReflection.GraphNodeRef.NodeType.entry:
					pendingTransitionEdit2.sourceStateMachine.RemoveEntryTransition((AnimatorTransition)pendingTransitionEdit2.transition);
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
		if (selectedTransitionEdits.Count > 0)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			foreach (AnimatorGraphReflection.TransitionEditionInfo selectedTransitionEdit in selectedTransitionEdits)
			{
				if (selectedTransitionEdit.sourceType != AnimatorGraphReflection.GraphNodeRef.NodeType.state)
				{
					continue;
				}
				AnimatorStateTransition stateTransition = selectedTransitionEdit.stateTransition;
				if (!stateTransition.destinationState)
				{
					return;
				}
				AnimatorStateTransition animatorStateTransition = new AnimatorStateTransition();
				EditorUtility.CopySerialized(stateTransition, animatorStateTransition);
				animatorStateTransition.destinationState = selectedTransitionEdit.sourceState;
				if ((bool)EditorSettings.GetInstance().autoReverseModes)
				{
					AnimatorCondition[] conditions = animatorStateTransition.conditions;
					for (int i = 0; i < conditions.Length; i++)
					{
						conditions[i] = ListAlgo(conditions[i], reverseModifiesValues);
					}
					animatorStateTransition.conditions = conditions;
				}
				Undo.RegisterCreatedObjectUndo(animatorStateTransition, "ReversedTransitions");
				Undo.RecordObject(stateTransition.destinationState, "ReversedTransitions");
				AssetDatabase.AddObjectToAsset(animatorStateTransition, ActiveController());
				animatorStateTransition.hideFlags = stateTransition.hideFlags;
				stateTransition.destinationState.AddTransition(animatorStateTransition);
				list.Add(animatorStateTransition);
				if (replaceTransitions)
				{
					Undo.RecordObject(selectedTransitionEdit.sourceState, "ReversedTransitions");
					selectedTransitionEdit.sourceState.RemoveTransition(selectedTransitionEdit.stateTransition);
					PatchAlgo();
				}
			}
			if (list.Count > 0)
			{
				Selection.objects = list.ToArray();
			}
		}
		EditorUtility.SetDirty(ActiveController());
	}

	private static void CreateAlgo()
	{
		replicateTransitionsMode = !replicateTransitionsMode;
		if (replicateTransitionsMode)
		{
			redirectTransitionsMode = false;
			makeMultipleTransitionsMode = false;
		}
		else
		{
			NewAlgo();
		}
	}

	private static void NewAlgo()
	{
		pendingTransitionEdits?.ForEach(delegate(AnimatorGraphReflection.TransitionEditionInfo t)
		{
			_003C_003Ec__DisplayClass430_0 _003C_003Ec__DisplayClass430_ = new _003C_003Ec__DisplayClass430_0();
			_003C_003Ec__DisplayClass430_.m_MerchantReg = t;
			EditorUtils.ForEach(selectedStates, _003C_003Ec__DisplayClass430_.AddThread);
			if (anyStateNodeSelected && !_003C_003Ec__DisplayClass430_.m_MerchantReg.transition.IsExitOrDangling())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.transition, ActiveStateMachine().AddAnyStateTransition((AnimatorState)null));
			}
			if (entryNodeSelected && !_003C_003Ec__DisplayClass430_.m_MerchantReg.transition.IsExitOrDangling())
			{
				TestAlgo(_003C_003Ec__DisplayClass430_.m_MerchantReg.transition, ActiveStateMachine().AddEntryTransition((AnimatorState)null));
			}
			if (replaceTransitions)
			{
				_003C_003Ec__DisplayClass430_.m_MerchantReg.Remove();
			}
		});
		if (anyStateNodeSelected || replaceTransitions)
		{
			InterruptAlgo(wantfirst: false);
		}
	}

	private static void PushAlgo()
	{
		HashSet<UnityEngine.Object> authenticationReg = new HashSet<UnityEngine.Object>();
		_003C_003Ec__DisplayClass431_0 CS_0024_003C_003E8__locals0;
		EditorUtils.ForEach(selectedStateTransitions, delegate(AnimatorStateTransition sel)
		{
			_003C_003Ec__DisplayClass431_1 _003C_003Ec__DisplayClass431_ = new _003C_003Ec__DisplayClass431_1();
			_003C_003Ec__DisplayClass431_.m_PoolReg = CS_0024_003C_003E8__locals0;
			_003C_003Ec__DisplayClass431_._ReponseReg = sel;
			if (_003C_003Ec__DisplayClass431_._ReponseReg.destinationState != null || _003C_003Ec__DisplayClass431_._ReponseReg.destinationStateMachine != null)
			{
				if (!RootStateMachine().anyStateTransitions.Any(_003C_003Ec__DisplayClass431_.FindThread))
				{
					authenticationReg.Add(MapAlgo(_003C_003Ec__DisplayClass431_._ReponseReg));
					int num = 0;
					while (true)
					{
						if (num >= ActiveStateMachine().states.Length)
						{
							return;
						}
						if (ActiveStateMachine().states[num].state.transitions.Any(_003C_003Ec__DisplayClass431_.InitThread))
						{
							break;
						}
						num++;
					}
					Undo.RecordObject(ActiveStateMachine().states[num].state, "Make AnyTransition");
					ActiveStateMachine().states[num].state.RemoveTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
				}
				else
				{
					ActiveStateMachine().states.ForEach(_003C_003Ec__DisplayClass431_.ExcludeThread);
					_ = RootStateMachine().anyStateTransitions;
					Undo.RecordObject(RootStateMachine(), "Remove AnyTransition");
					RootStateMachine().RemoveAnyStateTransition(_003C_003Ec__DisplayClass431_._ReponseReg);
				}
			}
		});
		Selection.objects = authenticationReg.ToArray();
	}

	private static void ViewAlgo()
	{
		_003C_003Ec__DisplayClass432_0 _003C_003Ec__DisplayClass432_ = new _003C_003Ec__DisplayClass432_0();
		_003C_003Ec__DisplayClass432_._ComposerReg = new List<AnimatorTransitionBase>();
		if (HasFocusedTransition())
		{
			int num = 0;
			if (ActiveStateMachine().entryTransitions.Contains(focusedTransition.transition))
			{
				num = 1;
			}
			else if (ActiveStateMachine().anyStateTransitions.Contains(focusedTransition.transition))
			{
				num = 2;
			}
			AnimatorState s = null;
			if (num == 0)
			{
				s = ActiveStateMachine().states.First((ChildAnimatorState c) => c.state.transitions.Contains(focusedTransition.transition)).state;
			}
			switch (num)
			{
			case 2:
				_003C_003Ec__DisplayClass432_.DefineThread((AnimatorStateTransition)focusedTransition.transition);
				break;
			case 1:
				_003C_003Ec__DisplayClass432_.VisitThread(focusedTransition.transition);
				break;
			case 0:
				_003C_003Ec__DisplayClass432_.StartThread((AnimatorStateTransition)focusedTransition.transition, s);
				break;
			}
		}
		else
		{
			(AnimatorState, List<AnimatorStateTransition>)[] array = FlushAlgo(selectedStateTransitions);
			List<AnimatorStateTransition> list = AwakeAlgo(selectedStateTransitions);
			List<AnimatorTransitionBase> list2 = ResetAlgo(selectedTransitions);
			if (selectedTransitionEdits.Count > 0)
			{
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					AnimatorState item = array[num2].Item1;
					List<AnimatorStateTransition> item2 = array[num2].Item2;
					new List<AnimatorStateTransition>();
					for (int num3 = 0; num3 < item2.Count; num3++)
					{
						if (selectedStateTransitions.Contains(item2[num3]))
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
		EditorUtility.SetDirty(ActiveController());
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
		return ListAlgo(reference, (bool)EditorSettings.GetInstance().reverseModifiesValues ^ Event.current.control);
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
		if (animationWindow == null)
		{
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(animationWindowType);
			if (array.Length == 0)
			{
				return null;
			}
			animationWindow = array[0] as EditorWindow;
		}
		return animationWindow;
	}

	[SpecialName]
	private static object InitInitializer()
	{
		if (!(FindInitializer() == null))
		{
			return animationWindowStateProperty.GetValue(FindInitializer());
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
		return (AnimationClip)activeAnimationClipProperty.GetValue(InitInitializer());
	}

	[SpecialName]
	private static void StartInitializer(AnimationClip init)
	{
		if (InitInitializer() != null)
		{
			activeAnimationClipProperty.SetValue(InitInitializer(), init);
		}
	}

	[SpecialName]
	private static GameObject SelectInitializer()
	{
		return VerifyAlgo();
	}

	private static GameObject VerifyAlgo(bool checklast = true)
	{
		if (overrideAnimationRoot != null)
		{
			return overrideAnimationRoot;
		}
		if ((bool)overrideAnimationController)
		{
			return null;
		}
		return (GameObject)activeRootGameObjectProperty.GetValue(InitInitializer());
	}

	[SpecialName]
	private static GameObject InstantiateInitializer()
	{
		if (previewRoot == null)
		{
			AwakeInitializer(new GameObject("OverrideGameObject")
			{
				hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
			});
			previewAnimator = InstantiateInitializer().AddComponent<Animator>();
		}
		return previewRoot;
	}

	[SpecialName]
	private static void AwakeInitializer(GameObject ident)
	{
		previewRoot = ident;
	}

	[SpecialName]
	private static Animator FlushInitializer()
	{
		if (previewAnimator == null)
		{
			previewAnimator = InstantiateInitializer().GetComponent<Animator>();
		}
		return previewAnimator;
	}

	[SpecialName]
	private static void TestInitializer(UnityEditor.Animations.AnimatorController first)
	{
		if (first == overrideAnimationController)
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
			forceGameObjectSelectionUpdate = true;
			FlushInitializer().runtimeAnimatorController = first;
			animationWindowType.DisableList("EditGameObjectInternal").Invoke(Resources.FindObjectsOfTypeAll(animationWindowType)[0], new object[2]
			{
				InstantiateInitializer(),
				null
			});
			forceGameObjectSelectionUpdate = false;
		}
		overrideAnimationController = first;
	}

	internal static void PrimeAnimationWindowReflection()
	{
		animationWindowType = EditorUtils.RequireQualifiedType("UnityEditor.AnimationWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		animationWindowStateType = EditorUtils.RequireQualifiedType("UnityEditorInternal.AnimationWindowState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		animationWindowHierarchyGUIType = EditorUtils.RequireQualifiedType("UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		animEditorType = EditorUtils.RequireQualifiedType("UnityEditor.AnimEditor, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		animationWindowSelectionItemType = EditorUtils.RequireQualifiedType("UnityEditorInternal.AnimationWindowSelectionItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		animationWindowControlType = EditorUtils.RequireQualifiedType("UnityEditorInternal.AnimationWindowControl, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		hierarchyNodeBindingField = EditorUtils.RequireQualifiedType("UnityEditorInternal.AnimationWindowHierarchyNode, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("binding", BindingFlags.Instance | BindingFlags.Public);
		animationWindowStateProperty = animationWindowType.GetAnyProperty("state");
		activeAnimationClipProperty = animationWindowStateType.GetAnyProperty("activeAnimationClip");
		activeRootGameObjectProperty = animationWindowStateType.GetAnyProperty("activeRootGameObject");
		activeGameObjectProperty = animationWindowStateType.GetAnyProperty("activeGameObject");
		activeScriptableObjectProperty = animationWindowStateType.GetAnyProperty("activeScriptableObject");
		playControlsOnGUIMethod = animEditorType.DisableList("PlayControlsOnGUI");
	}

	internal static void WriteAlgo()
	{
		if (Event.current.type != EventType.Layout)
		{
			HarmonyPatchManager.ApplyDeferredPatch("AnimationWindowFieldsPatch");
		}
	}

	internal static void ForgotAlgo()
	{
		HarmonyPatchManager.IncludeReg("AnimationWindowFieldsPatch", animationWindowType, "OnGUI", HarmonyPatchManager.MethodOf(WriteAlgo), animEditorType, "TabSelectionOnGUI", HarmonyPatchManager.PrepareReg(PlayControlsOnGUIPrefix));
		HarmonyPatchManager.MapReg(animationWindowType, "ShouldUpdateGameObjectSelection", HarmonyPatchManager.RevertReg<bool, bool>(ShouldUpdateGameObjectSelectionPrefix));
		HarmonyPatchManager.MapReg(animationWindowType, "ShouldUpdateSelection", HarmonyPatchManager.PrepareReg(ShouldUpdateAnimationSelectionPrefix));
		HarmonyPatchManager.MapReg(animationWindowControlType, "get_canPlay", HarmonyPatchManager.PrepareReg(InterruptIfOverridingControllerPrefix));
		HarmonyPatchManager.MapReg(animationWindowControlType, "get_canPreview", HarmonyPatchManager.PrepareReg(InterruptIfOverridingControllerPrefix));
		HarmonyPatchManager.MapReg(animationWindowControlType, "get_canRecord", HarmonyPatchManager.PrepareReg(InterruptIfOverridingControllerPrefix));
		HarmonyPatchManager.MapReg(animationWindowSelectionItemType, "GetEditorCurveValueType", HarmonyPatchManager.MethodOf(GetEditorCurveValueTypePrefix), HarmonyPatchManager.MethodOf(GetEditorCurveValueTypePost));
		HarmonyPatchManager.MapReg(animationWindowSelectionItemType, "get_rootGameObject", HarmonyPatchManager.RevertReg<GameObject, bool>(AnimationWindowSelectionItemGetRootGameObjectPrefix));
		HarmonyPatchManager.TestReg("UnityEditorInternal.AddCurvesPopupHierarchyDataSource, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "FetchData", HarmonyPatchManager.MethodOf(CurvesPopupFetchDataPrefix), HarmonyPatchManager.MethodOf(CurvesPopupFetchDataPost));
		HarmonyPatchManager.MapReg(animationWindowHierarchyGUIType, "DoNodeGUI", HarmonyPatchManager.PushReg<Rect, TreeViewItem>(AnimationWindowHierarchyNodeGUIPrefix), HarmonyPatchManager.NewReg<Rect>(AnimationWindowHierarchyNodeGUIPost));
		HarmonyPatchManager.MapReg(animationWindowHierarchyGUIType, "GenerateMenu", null, HarmonyPatchManager.CountTests<GenericMenu, List<object>>(AnimationWindowHierarchyGUIGenerateMenuPost));
		HarmonyPatchManager.MapReg(animationWindowHierarchyGUIType, "DoAddCurveButton", null, HarmonyPatchManager.NewReg<Rect>(AnimationWindowDoAddCurveButtonPost));
	}

	private static bool PlayControlsOnGUIPrefix()
	{
		if (!EditorSettings.GetInstance().aw_active || !EditorSettings.GetInstance().aw_enableOverride)
		{
			return true;
		}
		using (new GUIDisabledScope(iskey: false))
		{
			if (overrideAnimationController == null)
			{
				TestInitializer(null);
				GUILayout.Label("[Override Controller]", EditorUtils.styles().noteLeft, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (EditorUtils.ClickArea(lastRect))
				{
					EditorUtils.ShowObjectPicker(overrideAnimationController, typeof(UnityEditor.Animations.AnimatorController), null, null, loaddef3: true, null, null, delegate(UnityEngine.Object o)
					{
						TestInitializer((UnityEditor.Animations.AnimatorController)o);
					});
				}
				EditorUtils.HandleMultiDragAndDrop(lastRect, delegate(IEnumerable<UnityEditor.Animations.AnimatorController> o)
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
				TestInitializer((UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(overrideAnimationController, typeof(UnityEditor.Animations.AnimatorController), true));
				if ((bool)overrideAnimationController)
				{
					EditorUtils.TestQueue("Controller", iscfg: true, 80f + (float)overrideAnimationController.name.Length * 6.5f, 20f, isivk3: false);
				}
			}
			if (overrideAnimationRoot == null)
			{
				overrideAnimationRoot = null;
				GUILayout.Label("[Override Root]", EditorUtils.styles().noteLeft, GUILayout.MinWidth(1f), GUILayout.ExpandWidth(expand: false));
				Rect lastRect2 = GUILayoutUtility.GetLastRect();
				if (EditorUtils.ClickArea(lastRect2))
				{
					EditorUtils.ShowObjectPicker(overrideAnimationRoot, typeof(GameObject), null, null, loaddef3: true, null, null, delegate(UnityEngine.Object o)
					{
						overrideAnimationRoot = (GameObject)o;
					});
				}
				EditorUtils.HandleMultiDragAndDrop(lastRect2, delegate(IEnumerable<GameObject> o)
				{
					GameObject[] source = (o as GameObject[]) ?? o.ToArray();
					if (source.Any())
					{
						overrideAnimationRoot = source.First();
					}
				});
			}
			else
			{
				overrideAnimationRoot = (GameObject)EditorGUILayout.ObjectField(overrideAnimationRoot, typeof(GameObject), true);
				if ((bool)overrideAnimationRoot)
				{
					EditorUtils.TestQueue("Root", iscfg: true, 50f + (float)overrideAnimationRoot.name.Length * 6.5f, 20f, isivk3: false);
				}
			}
		}
		return true;
	}

	private static void AnimationWindowHierarchyGUIGenerateMenuPost(ref GenericMenu __result, List<object> interactedNodes)
	{
		contextMenu = __result;
		propertyEditingMenuAllowed = true;
		interactedHierarchyNodes = interactedNodes;
	}

	private static void AnimationWindowHierarchyNodeGUIPrefix(Rect rect, TreeViewItem node)
	{
		if (!EditorSettings.GetInstance().aw_active || !EditorSettings.GetInstance().aw_enablePropertyEditing || node == null)
		{
			return;
		}
		EditorCurveBinding? _AttributeReg = (EditorCurveBinding?)hierarchyNodeBindingField.GetValue(node);
		if (!_AttributeReg.HasValue)
		{
			return;
		}
		GameObject fieldReg = SelectInitializer();
		if (fieldReg == null)
		{
			return;
		}
		EditorUtils.HandleDragAndDrop(rect, delegate(GameObject o)
		{
			AnimationClip setup = DefineInitializer();
			if (!(fieldReg == null))
			{
				if (!o.transform.IsChildOf(fieldReg.transform) && o != fieldReg)
				{
					Log(o.name + " is not a child of " + fieldReg.name, CustomLogType.Warning);
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
		if (!EditorSettings.GetInstance().aw_active || !EditorSettings.GetInstance().aw_enablePropertyEditing || !propertyEditingMenuAllowed)
		{
			return;
		}
		propertyEditingMenuAllowed = false;
		rect.y += 60f;
		rect.x += Event.current.mousePosition.x;
		EditorCurveBinding[] array;
		try
		{
			AnimationClip clientReg = DefineInitializer();
			array = (from n in interactedHierarchyNodes
				where ((TreeViewItem)n).children.CalcRules()
				select (EditorCurveBinding)hierarchyNodeBindingField.GetValue(n) into b
				where (b.isPPtrCurve && !AnimationUtility.GetObjectReferenceCurve(clientReg, b).GetRules()) || !AnimationUtility.GetEditorCurve(clientReg, b).keys.GetRules()
				select b).ToArray();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return;
		}
		if (array.Length != 0)
		{
			StopAlgo(contextMenu, rect, array);
			contextMenu.ShowAsContext();
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
				if (SelectInitializer() != null && configReg.type.Is<Renderer>())
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
				DreadScripts.ControllerEditor.SearchablePickerPopup<string> searchablePickerPopup = new DreadScripts.ControllerEditor.SearchablePickerPopup<string>("Property Name", array, _003C_003Ec.watcherInitializer.DisableObserver, delegate(int i, string s)
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
				searchablePickerPopup.EnableSearch(_003C_003Ec.watcherInitializer.InsertObserver);
				searchablePickerPopup.Show(token);
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
				enumerable = componentTypes.Where(VerifyVisitor);
			}
			DreadScripts.ControllerEditor.SearchablePickerPopup<Type> searchablePickerPopup = new DreadScripts.ControllerEditor.SearchablePickerPopup<Type>("Type", enumerable, _003C_003Ec.watcherInitializer.AddObserver, delegate(int i, Type t)
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
				Log(t.Name + " has no animatable properties.");
			});
			searchablePickerPopup.SetExtraData(_003C_003Ec.watcherInitializer.InvokeObserver);
			searchablePickerPopup.EnableSearch(_003C_003Ec.watcherInitializer.FindObserver);
			searchablePickerPopup.Show(new Rect(token));
		});
	}

	private static void AnimationWindowDoAddCurveButtonPost(Rect rect)
	{
		if (!EditorSettings.GetInstance().aw_active || !EditorSettings.GetInstance().aw_enableGameObjectDND)
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
		EditorUtils.AddLinkCursor(spec);
		rect = new Rect(0f, spec.y + spec.height + 4f, rect.width, EditorGUIUtility.singleLineHeight * 2f);
		Rect rect2 = new Rect(rect);
		rect2.height = 14f;
		Rect rect3 = rect2;
		using (new GUIDisabledScope(m_ItemReg == null))
		{
			GUI.Label(rect3, "[Drag & Drop GameObjects]", EditorUtils.styles().noteCenter);
			EditorUtils.HandleMultiDragAndDrop(rect, delegate(IEnumerable<GameObject> enu)
			{
				EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(m_ManagerReg);
				Undo.RecordObject(m_ManagerReg, "[CE] Add Curve");
				foreach (GameObject item in enu)
				{
					if (!Log(item.name + " is not a child of " + m_ItemReg.name, CustomLogType.Warning, !item.transform.IsChildOf(_SpecificationReg) && item != m_ItemReg))
					{
						string path = AnimationUtility.CalculateTransformPath(item.transform, m_ItemReg.transform);
						EditorCurveBinding tag = new EditorCurveBinding
						{
							type = typeof(GameObject),
							path = path,
							propertyName = "m_IsActive"
						};
						EditorCurveBinding editorCurveBinding = tag;
						if (!Log("Matching binding already exists!", CustomLogType.Warning, curveBindings.InitPredicate(editorCurveBinding, out tag)))
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
			Log("Caution! Merging with pre-existing property.", CustomLogType.Warning, animationCurve != null && (bool)EditorSettings.GetInstance().aw_warnPropertyMerge);
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
		Log("Caution! Merging with pre-existing property.", CustomLogType.Warning, array != null && (bool)EditorSettings.GetInstance().aw_warnPropertyMerge);
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
		return overrideAnimationController == null;
	}

	private static bool ShouldUpdateAnimationSelectionPrefix()
	{
		return !overrideAnimationController;
	}

	private static void GetEditorCurveValueTypePost()
	{
		overrideAnimationRootActive = false;
	}

	private static void CurvesPopupFetchDataPost()
	{
		overrideAnimationRootActive = false;
	}

	private static void GetEditorCurveValueTypePrefix()
	{
		overrideAnimationRootActive = overrideAnimationRoot;
	}

	private static void CurvesPopupFetchDataPrefix()
	{
		overrideAnimationRootActive = overrideAnimationRoot;
	}

	private static bool ShouldUpdateGameObjectSelectionPrefix(ref bool __result)
	{
		if (!forceGameObjectSelectionUpdate)
		{
			return !overrideAnimationController;
		}
		__result = true;
		return false;
	}

	private static bool AnimationWindowSelectionItemGetRootGameObjectPrefix(ref GameObject __result)
	{
		if (!overrideAnimationRootActive)
		{
			return true;
		}
		__result = overrideAnimationRoot;
		return false;
	}

	private static void PrimeGraphStyleReflection()
	{
		graphGUIType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		graphEdgeType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Edge, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		graphStylesType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
	}

	private static void AssetAlgo()
	{
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "get_selectedEdgeColor", HarmonyPatchManager.RevertReg<Color, bool>(SelectedTransitionColorPrefix));
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "get_defaultTransitionColor", HarmonyPatchManager.RevertReg<Color, bool>(EntryTransitionColorPrefix));
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "get_selectorTransitionColor", HarmonyPatchManager.RevertReg<Color, bool>(BaseTransitionColorPrefix));
		Type type = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Slot, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		HarmonyPatchManager.CalcReg(graphEdgeType.GetConstructor(new Type[2] { type, type }), null, HarmonyPatchManager.NewReg<Edge>(EdgeConstructorPost));
		HarmonyPatchManager.MapReg(graphGUIType, "get_gridMajorColor", HarmonyPatchManager.RevertReg<Color, bool>(GraphGUIMajorGridColorPrefix));
		HarmonyPatchManager.MapReg(graphGUIType, "get_gridMinorColor", HarmonyPatchManager.RevertReg<Color, bool>(GraphGUIMinorGridColorPrefix));
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "DrawArrows", HarmonyPatchManager.AssetReg<Vector3[]>(DrawArrowsPrefix));
		HarmonyPatchManager.PatchByParameterType(graphType, typeof(bool), "AddNode", null, HarmonyPatchManager.NewReg<Node>(AddNodePost));
	}

	private static bool SelectedTransitionColorPrefix(ref Color __result)
	{
		if (!EditorSettings.GetInstance().cosmeticTransitionsActive)
		{
			return true;
		}
		__result = EditorSettings.GetInstance().selectedTransitionColor.GetValue();
		return false;
	}

	private static bool EntryTransitionColorPrefix(ref Color __result)
	{
		__result = EditorSettings.GetInstance().entryTransitionColor.GetValue();
		return !EditorSettings.GetInstance().cosmeticTransitionsActive;
	}

	private static bool BaseTransitionColorPrefix(ref Color __result)
	{
		__result = EditorSettings.GetInstance().baseTransitionColor.GetValue();
		return !EditorSettings.GetInstance().cosmeticTransitionsActive;
	}

	private static void EdgeConstructorPost(Edge __instance)
	{
		if ((bool)EditorSettings.GetInstance().cosmeticTransitionsActive)
		{
			__instance.color = EditorSettings.GetInstance().normalTransitionColor.GetValue();
		}
	}

	private static void AddNodePost(Node node)
	{
	}

	private static void DrawArrowsPrefix(ref Vector3[] edgePoints)
	{
		if (!animatingSelectedEdges && !arrowLerpEnabled)
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
			float num2 = EditorSettings.GetInstance().arrowLerpRatio.GetValue();
			if (animatingSelectedEdges && animatedEdgeArrowPoints.Contains(vector2))
			{
				repaintGraphRequested = true;
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
		if ((bool)EditorSettings.GetInstance().cosmeticGraphActive)
		{
			__result = ((!ControllerEditorWindow.PushTests()) ? EditorSettings.GetInstance().gridMajorLightColor : EditorSettings.GetInstance().gridMajorDarkColor).GetValue();
			return false;
		}
		return true;
	}

	private static bool GraphGUIMinorGridColorPrefix(ref Color __result)
	{
		if ((bool)EditorSettings.GetInstance().cosmeticGraphActive)
		{
			__result = (ControllerEditorWindow.PushTests() ? EditorSettings.GetInstance().gridMinorDarkColor : EditorSettings.GetInstance().gridMinorLightColor).GetValue();
			return false;
		}
		return true;
	}

	private static void ApplyGraphBackground()
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
		if (graphBackgroundStyleField == null)
		{
			graphBackgroundStyleField = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("graphBackground", BindingFlags.Static | BindingFlags.Public);
		}
		bool flag = !EditorSettings.GetInstance().cosmeticGraphActive;
		GUIStyle gUIStyle = new GUIStyle();
		Texture2D value;
		if (flag || !EditorSettings.GetInstance().graphBackgroundIsTexture)
		{
			if (graphBackgroundTexture == null)
			{
				graphBackgroundTexture = new Texture2D(1, 1);
			}
			graphBackgroundTexture.SetPixel(0, 0, (!flag && (bool)EditorSettings.GetInstance().cosmeticGraphActive) ? EditorSettings.GetInstance().gridBackgroundColor.GetValue() : ((Color)EditorSettings.GetInstance().gridBackgroundColor.defaultValue));
			graphBackgroundTexture.Apply();
			value = graphBackgroundTexture;
		}
		else
		{
			value = EditorSettings.GetInstance().graphBackgroundTexture.GetValue<Texture2D>();
		}
		gUIStyle.normal.background = value;
		graphBackgroundStyleField.SetValue(null, gUIStyle);
	}

	private static void PrimeAnimatorToolReflection()
	{
		animatorControllerToolType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		graphType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		blendTreeGraphGUIType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationBlendTree.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		stateMachineGraphGUIType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		stateMachineGraphType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		edgeGUIType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		rebuildGraphMethod = animatorControllerToolType.DisableList("RebuildGraph");
		addBreadCrumbMethod = animatorControllerToolType.DisableList("AddBreadCrumb");
		activeGraphGUIGetter = animatorControllerToolType.DisableList("get_activeGraphGUI");
		getEdgePointsMethod = edgeGUIType.InsertList("GetEdgePoints", new Type[1] { typeof(Edge) });
	}

	private static void LogoutAlgo()
	{
		HarmonyPatchManager.MapReg(stateMachineGraphType, "SetStateMachines", HarmonyPatchManager.ViewReg<AnimatorStateMachine, AnimatorStateMachine, AnimatorStateMachine>(SetStateMachinesPost));
		HarmonyPatchManager.MapReg(animatorControllerToolType, "DoGraphBottomBar", null, new Action<Rect>(GraphGUIBottomBarPost).Method);
		HarmonyPatchManager.MapReg(stateMachineGraphGUIType, "OnGraphGUI", HarmonyPatchManager.PrepareReg(OnGraphGUIPrefix), HarmonyPatchManager.MethodOf(OnGraphGUIPost));
		HarmonyPatchManager.MapReg(stateMachineGraphGUIType, "HandleObjectDragging", null, null, HarmonyPatchManager.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(HandleObjectDraggingTranspiler));
		HarmonyPatchManager.MapReg(stateMachineGraphGUIType, "HandleContextMenu", HarmonyPatchManager.QueryTests<object, bool, object>(GraphGUIContextMenuPrefix), HarmonyPatchManager.NewReg<bool>(GraphGUIContextMenuPost));
	}

	private static void SetStateMachinesPost(AnimatorStateMachine stateMachine, AnimatorStateMachine parent, AnimatorStateMachine root)
	{
		OrderInitializer(stateMachine);
		PrintMapper(root);
		InstantiateAnnotation();
		ApplyGraphBackground();
	}

	private static bool OnGraphGUIPrefix()
	{
		insideGraphGui = true;
		if (dragAndDropPending)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		}
		dragAndDropPending = false;
		animatedEdgeArrowPoints.Clear();
		bool flag = EditorSettings.GetInstance().animateInboundEdges;
		bool flag2 = EditorSettings.GetInstance().animateOutboundEdges;
		bool flag3 = flag && flag2;
		arrowLerpEnabled = EditorSettings.GetInstance().arrowLerpRatio.GetValue() != 0f;
		animatingSelectedEdges = (flag || flag2) && !selectedNodes.CalcRules();
		if (animatingSelectedEdges)
		{
			try
			{
				foreach (AnimatorGraphReflection.GraphNodeRef selectedNode in selectedNodes)
				{
					foreach (Vector3 item in (flag3 ? selectedNode.InputEdges().Concat(selectedNode.OutputEdges()) : (flag ? selectedNode.InputEdges() : selectedNode.OutputEdges())).Select((AnimatorGraphReflection.GraphEdgeRef e) => InitAnnotation(e)[1]))
					{
						animatedEdgeArrowPoints.Add(item);
					}
				}
			}
			catch (Exception exception)
			{
				Selection.objects = null;
				UnityEngine.Debug.LogException(exception);
			}
		}
		return true;
	}

	private static void OnGraphGUIPost()
	{
		insideGraphGui = false;
	}

	private static void GraphGUIContextMenuPrefix(object __instance, out bool __state, object ___m_EdgeGUI)
	{
		__state = true;
		Event current = Event.current;
		switch (current.type)
		{
		case EventType.ContextClick:
			if ((hasPlainTransitionSelected || hasStateTransitionSelected) && findClosestEdgeMethod.Invoke(___m_EdgeGUI, null) != null)
			{
				__state = false;
				replaceTransitions = replaceTransitionsDefault ^ current.shift;
				reverseModifiesValues = (bool)EditorSettings.GetInstance().reverseModifiesValues ^ current.control;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Reverse Transitions"), on: false, DeleteAlgo);
				genericMenu.AddItem(new GUIContent("Redirect Transitions"), redirectTransitionsMode, LoginAlgo);
				genericMenu.AddItem(new GUIContent("Replicate Transitions"), replicateTransitionsMode, CreateAlgo);
				genericMenu.AddItem(new GUIContent("From\\To Any Transition"), on: false, PushAlgo);
				genericMenu.AddSeparator(string.Empty);
				genericMenu.AddItem(new GUIContent("(Replacing)"), replaceTransitions, CloneAlgo);
				genericMenu.ShowAsContext();
				current.Use();
			}
			break;
		case EventType.MouseDown:
			if ((current.control ^ (bool)EditorSettings.GetInstance().switchDoubleClick) && current.clickCount == 2)
			{
				Vector2 task = current.mousePosition + new Vector2(-100f, -20f);
				AnimatorState animatorState = ActiveStateMachine().AddState(EditorSettings.GetInstance().defaultState.name, task.PostPredicate(10));
				QueryAlgo(animatorState);
				animatorState.motion = EditorSettings.GetInstance().defaultState.motion;
				current.Use();
			}
			break;
		case EventType.KeyDown:
			switch (current.keyCode)
			{
			case KeyCode.Escape:
				makeMultipleTransitionsMode = false;
				redirectTransitionsMode = false;
				replicateTransitionsMode = false;
				AnimatorGraphReflection.GraphAccessors.Tool()?.Repaint();
				break;
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				if (!replicateTransitionsMode)
				{
					if (!redirectTransitionsMode)
					{
						if (makeMultipleTransitionsMode)
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
				AnimatorGraphReflection.GraphAccessors.Tool()?.Repaint();
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
			contextMenu.AddSeparator(string.Empty);
			contextMenu.AddItem(new GUIContent(((selectedStateTransitions.Count <= 0) ? "Select" : "Deselect") + " All Transitions"), on: false, DestroyAlgo);
			contextMenu.AddItem(new GUIContent(((selectedStates.Count > 0) ? "Deselect" : "Select") + " All States"), on: false, InsertAlgo);
			contextMenu.ShowAsContext();
		}
	}

	private static void GraphGUIBottomBarPost(Rect nameRect)
	{
		if (replicateTransitionsMode || redirectTransitionsMode || makeMultipleTransitionsMode)
		{
			Rect screenRect = new Rect(nameRect);
			screenRect.y -= screenRect.height + 5f;
			GUILayout.BeginArea(screenRect);
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.Label(replicateTransitionsMode ? "Replicating Transitions" : ((!redirectTransitionsMode) ? "Making Transitions" : "Redirecting Transitions"), EditorUtils.styles().centeredBoldRichLabel);
			}
			GUILayout.EndArea();
		}
		if ((bool)ActiveController())
		{
			if (!EditorSettings.GetInstance().hasPingedController)
			{
				GUI.Label(nameRect, "Click to highlight Controller", EditorUtils.styles().noteLeft);
			}
			if (EditorUtils.ClickArea(nameRect))
			{
				EditorGUIUtility.PingObject(ActiveController());
				EditorSettings.GetInstance().hasPingedController.SetValue(excludeparam: true);
			}
		}
		if (frameLayerRequested)
		{
			frameLayerRequested = false;
			EditorWindow obj = AnimatorGraphReflection.GraphAccessors.Tool();
			animatorControllerToolType.GetMethod("FrameAutofit").Invoke(obj, Array.Empty<object>());
		}
		if (repaintGraphRequested)
		{
			repaintGraphRequested = false;
			AnimatorGraphReflection.GraphAccessors.Tool().Repaint();
		}
		if (rebuildGraphRequested)
		{
			rebuildGraphRequested = false;
			PatchAlgo();
		}
	}

	private static void PatchAlgo()
	{
		InterruptAlgo(wantfirst: false);
	}

	private static void InterruptAlgo(bool wantfirst)
	{
		EditorWindow editorWindow = AnimatorGraphReflection.GraphAccessors.Tool();
		if (editorWindow != null)
		{
			rebuildGraphMethod.Invoke(editorWindow, new object[1] { wantfirst });
		}
	}

	[SpecialName]
	private static string ValidateInitializer()
	{
		return EditorSettings.GetInstance().categoryBaseName;
	}

	[SpecialName]
	private static bool RateInitializer()
	{
		return layerViewType == LayerViewViewType.DefaultView;
	}

	[SpecialName]
	private static bool GetInitializer()
	{
		return layerViewType == LayerViewViewType.CategoryByName;
	}

	[SpecialName]
	private static bool IncludeInitializer()
	{
		return layerViewType == LayerViewViewType.CategoryByTag;
	}

	[SpecialName]
	private static float CloneInitializer()
	{
		return (!EditorSettings.GetInstance().layerCompactView) ? 40 : 20;
	}

	[SpecialName]
	private static ReorderableList ReflectInitializer()
	{
		if (!RateInitializer())
		{
			return categoryLayerList;
		}
		return unityLayerList;
	}

	[SpecialName]
	private static List<UnityEditor.Animations.AnimatorControllerLayer> CreateInitializer()
	{
		return unityLayerList.list.Cast<UnityEditor.Animations.AnimatorControllerLayer>().ToList();
	}

	[SpecialName]
	private static string PushInitializer()
	{
		return EditorSettings.GetInstance().categoryDelimiter;
	}

	private static void PrimeLayerControllerViewReflection()
	{
		layerControllerViewType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.LayerControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		layerSettingsWindowType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.LayerSettingsWindow, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		layerScrollField = layerControllerViewType.GetAnyField("m_LayerScroll");
		onRemoveLayerMethod = layerControllerViewType.DisableList("OnRemoveLayer");
		layerRenameEndMethod = layerControllerViewType.DisableList("RenameEnd");
		layerListField = layerControllerViewType.GetAnyField("m_LayerList");
		layerViewHostField = layerControllerViewType.GetAnyField("m_Host");
		keyboardHandlingMethod = layerControllerViewType.DisableList("KeyboardHandling");
		showAtPositionMethod = layerSettingsWindowType.DisableList("ShowAtPosition");
		toolAnimatorControllerField = animatorControllerToolType.GetAnyField("m_AnimatorController");
	}

	private static void PrintAlgo()
	{
		HarmonyPatchManager.MapReg(layerControllerViewType, "OnToolbarGUI", HarmonyPatchManager.MethodOf(smethod_0), HarmonyPatchManager.MethodOf(OnToolbarGUIPost));
		HarmonyPatchManager.MapReg(layerControllerViewType, "OnGUI", HarmonyPatchManager.CheckReg<Rect, bool>(smethod_1), HarmonyPatchManager.MethodOf(smethod_2));
		HarmonyPatchManager.MapReg(layerControllerViewType, "Init", null, HarmonyPatchManager.PushReg<object, ReorderableList>(LayerViewInitPost));
		HarmonyPatchManager.MapReg(layerControllerViewType, "OnDrawLayer", HarmonyPatchManager.SearchReg<Rect, int, bool>(OnDrawLayerPrefix));
		HarmonyPatchManager.MapReg(layerControllerViewType, "OnRemoveLayer", null, HarmonyPatchManager.MethodOf(DisableMapper));
		HarmonyPatchManager.MapReg(layerControllerViewType, "RenameEnd", HarmonyPatchManager.OrderTests<string>(LayerViewRenameEndPrefix), HarmonyPatchManager.NewReg<string>(LayerViewRenameEndPost));
		HarmonyPatchManager.MapReg(layerControllerViewType, "OnSelectLayer", HarmonyPatchManager.OrderTests<int>(OnSelectLayerPrefix), HarmonyPatchManager.NewReg<int>(OnSelectLayerPost));
		HarmonyPatchManager.MapReg(layerControllerViewType, "ResetUI", null, null, HarmonyPatchManager.CheckReg<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>(LayerResetTranspiler));
		HarmonyPatchManager.MapReg(animatorControllerToolType, "AddNewLayer", HarmonyPatchManager.CheckReg<object, bool>(AddNewLayerPrefix));
		HarmonyPatchManager.MapReg(typeof(ReorderableList), "DoLayoutList", HarmonyPatchManager.PrepareReg(RevertAlgo));
	}

	private static void smethod_0()
	{
		string text = layerViewType.ToString().Humanize();
		bool flag = EditorSettings.GetInstance().displayCategoryView;
		bool flag2 = EditorSettings.GetInstance().displayLayerCompactView;
		Rect var = GUILayoutUtility.GetLastRect().MoveRight(-10f).WithWidth((!flag) ? 0f : (text.PushResolver() + 24f))
			.WithHeight(20f);
		Rect rect = var.CollapseToRightEdge().WithWidth(flag2 ? 20 : 0);
		Rect item = rect.CollapseToRightEdge().WithWidth(20f);
		if (flag && GUI.Button(var, text.CreateResolver(), EditorStyles.toolbarDropDown))
		{
			SearchAlgo();
		}
		if (flag2)
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.FG, !EditorSettings.GetInstance().layerCompactView, Color.gray))
			{
				if (EditorUtils.QueryQueue(rect, new GUIContent(EditorUtils.contents().hamburgerMenu)
				{
					tooltip = "Compact View"
				}, EditorUtils.styles().paddedBox))
				{
					EditorSettings.GetInstance().layerCompactView.Toggle();
					DisableMapper();
				}
			}
		}
		if (!flag)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(layerViewType == LayerViewViewType.DefaultView))
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.FG, !EditorSettings.GetInstance().sortCategoryViewLayers, Color.gray))
			{
				if (EditorUtils.QueryQueue(item, EditorUtils.contents().sort, EditorUtils.styles().paddedBox))
				{
					EditorSettings.GetInstance().sortCategoryViewLayers.Toggle();
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
			templateDropdownArmed = true;
		}
		else if (templateDropdownArmed)
		{
			templateDropdownArmed = false;
			RevertVisitor();
		}
		int num = EditorGUI.Popup(rect, -1, layerTemplateNames);
		switch (num)
		{
		case -1:
			break;
		default:
			OrderAlgo(layerTemplateControllers[num], ActiveController());
			break;
		case 0:
			ActiveController().AddLayer("New Layer", EditorSettings.GetInstance().defaultLayerWeight, EditorSettings.GetInstance().defaultLayerMask.GetValue<AvatarMask>());
			break;
		}
	}

	private static void SearchAlgo()
	{
		GenericMenu genericMenu = new GenericMenu();
		foreach (LayerViewViewType schemaReg in Enum.GetValues(typeof(LayerViewViewType)))
		{
			genericMenu.AddItem(schemaReg.ToString().Humanize().DeleteResolver(tokenneeded: true), on: false, delegate
			{
				layerViewType = schemaReg;
				DisableMapper();
			});
		}
		genericMenu.ShowAsContext();
	}

	private static bool smethod_1(Rect rect)
	{
		LayerViewViewType layerViewViewType = layerViewType;
		if (layerViewViewType == LayerViewViewType.DefaultView || (uint)(layerViewViewType - 1) > 1u)
		{
			categoryViewDrewLayerList = false;
			return true;
		}
		RestartMapper();
		keyboardHandlingMethod.Invoke(ReadAnnotation(), null);
		categoryLayerScroll = GUILayout.BeginScrollView(categoryLayerScroll);
		categoryLayerList.DoLayoutList();
		EditorGUILayout.HelpBox("Category view may have missing interactions or unhandled issues.", MessageType.None);
		GUILayout.EndScrollView();
		categoryViewDrewLayerList = true;
		return true;
	}

	private static bool RevertAlgo()
	{
		return !categoryViewDrewLayerList;
	}

	private static void smethod_2()
	{
		categoryViewDrewLayerList = false;
	}

	private static void LayerViewInitPost(object __instance, ReorderableList ___m_LayerList)
	{
		layerControllerView = __instance;
		unityLayerList = ___m_LayerList;
		PublishAnnotation();
	}

	private static bool OnDrawLayerPrefix(ref Rect rect, ref int index)
	{
		if (unityLayerList != null)
		{
			UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = unityLayerList.list[index] as UnityEditor.Animations.AnimatorControllerLayer;
			OrderMapper(rect, index);
			if ((bool)EditorSettings.GetInstance().displayLayerIndex && RateInitializer())
			{
				Rect rect2 = rect.MoveRight(-20f).WithWidth(22f);
				if (!EditorSettings.GetInstance().layerCompactView)
				{
					rect2 = rect2.MoveDown(10f).ExpandDown(-10f);
				}
				else
				{
					EditorGUI.DrawRect(rect2 = rect2.WithWidth(18f), new Color(0.2f, 0.2f, 0.2f));
				}
				GUI.Label(rect2, index.ToString(), EditorUtils.styles().noteCenter);
			}
			if (animatorControllerLayer.stateMachine == null)
			{
				float x = 20f + animatorControllerLayer.name.PushResolver();
				GUI.Label(new Rect(x, rect.yMin + 6f, 20f, 14f), new GUIContent(EditorUtils.contents().warning)
				{
					tooltip = "Statemachine is Null! This layer will cause issues!"
				});
			}
			if ((bool)EditorSettings.GetInstance().layerCompactView)
			{
				if (!layerRenameOverlay.IsRenaming() || layerRenameOverlay.UserData() != index || layerRenameOverlay.IsWaitingForDelay())
				{
					float num = ((index == 0) ? 1f : ((!AddAnnotation()) ? animatorControllerLayer.defaultWeight : InvokeAnnotation().GetLayerWeight(index)));
					bool flag = num < 1f;
					bool flag2 = num == 0f;
					float num2 = ((!flag) ? 1f : Mathf.Lerp(0.5f, 1f, num));
					using (new GUIColorScope(GUIColorScope.ColoringType.FG, flag, new Color(num2, num2, num2)))
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
					Rect rect3 = res.SliceRight(16f, isserv: true);
					if (EditorUtils.QueryQueue(rect3.MoveDown(2f), EditorUtils.contents().settingsGear, EditorUtils.styles().paddedBox))
					{
						showAtPositionMethod.Invoke(null, new object[4]
						{
							rect3,
							animatorControllerLayer,
							index,
							ActiveController()
						});
					}
					if (animatorControllerLayer.syncedLayerIndex >= 0)
					{
						bool syncedLayerAffectsTiming = animatorControllerLayer.syncedLayerAffectsTiming;
						GUI.Label(text: syncedLayerAffectsTiming ? "S+T" : "S", position: syncedLayerAffectsTiming ? res.SliceRight(28f, isserv: true) : res.SliceRight(14f, isserv: true), style: style);
					}
					if (animatorControllerLayer.iKPass)
					{
						GUI.Label(res.SliceRight(14f, isserv: true), "IK", style);
					}
					if (animatorControllerLayer.blendingMode == UnityEditor.Animations.AnimatorLayerBlendingMode.Additive)
					{
						GUI.Label(res.SliceRight(14f, isserv: true), "A", style);
					}
					if (!animatorControllerLayer.avatarMask.IsNull())
					{
						GUI.Label(res.SliceRight(14f, isserv: true), "M", style);
					}
					if (flag && !flag2)
					{
						GUI.Label(res.SliceRight(32f, isserv: true, 2f, isvisitor3: true).MoveDown(-1f), num.ToString("F2"), EditorUtils.styles().noteRight);
					}
					if (EditorUtils.DeletePressed())
					{
						CompareMapper();
					}
				}
				else
				{
					if (rect.width >= 0f && !(rect.height < 0f))
					{
						rect.x -= 2f;
						layerRenameOverlay.EditFieldRect(rect);
					}
					if (!layerRenameOverlay.OnGUI())
					{
						layerRenameEndMethod.Invoke(ReadAnnotation(), null);
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
		if (layerRenameOverlay.Instance() != null || layerRenameOverlay.ResolveInstance() != null)
		{
			string text = layerRenameOverlay.OriginalName();
			string text2 = layerRenameOverlay.Name();
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
		if (!__state.IsNullOrEmpty() && GetInitializer())
		{
			DisableMapper();
			LayerPathNode layerPathNode = currentLayerCategory;
			currentLayerCategory = layerCategoryRoot.FindClosest(__state);
			if (layerPathNode != currentLayerCategory)
			{
				AddMapper();
				int num = currentLayerCategory.layers.FindIndex((LayerIndexEntry l) => l.layer.name == __state);
				if (num >= 0)
				{
					categoryLayerList.index = num;
				}
			}
		}
		categoryLayerList.GrabKeyboardFocus();
	}

	private static void OrderMapper(Rect first, int cfgY)
	{
		Event m_ProxyReg = Event.current;
		if (!unityLayerList.HasKeyboardControl())
		{
			ReorderableList reorderableList = categoryLayerList;
			if (reorderableList == null || !reorderableList.HasKeyboardControl())
			{
				goto IL_0035;
			}
		}
		EventType type = m_ProxyReg.type;
		if (type != EventType.KeyDown)
		{
			if (type == EventType.ExecuteCommand)
			{
				contextLayerIndex = unityLayerList.index;
				layerContextController = (UnityEditor.Animations.AnimatorController)toolAnimatorControllerField.GetValue(layerViewHostField.GetValue(ReadAnnotation()));
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
				ReorderableList reorderableList2 = ((!(flag = layerViewType == LayerViewViewType.DefaultView)) ? categoryLayerList : unityLayerList);
				int count = reorderableList2.count;
				int num = -1;
				for (int i = 0; i < count; i++)
				{
					int num2 = (int)Mathf.Repeat(i + reorderableList2.index + 1, count);
					if (char.ToLower(DefineMapper(num2).name[0]) == character)
					{
						num = num2;
						break;
					}
				}
				if (num >= 0)
				{
					reorderableList2.index = num;
					float y = (float)num * CloneInitializer();
					layerScrollField.SetValue(ReadAnnotation(), new Vector2(0f, y));
					if (!flag)
					{
						categoryLayerScroll = new Vector2(0f, y);
					}
					m_ProxyReg.Use();
				}
			}
		}
		else
		{
			m_ProxyReg.Use();
			int index = unityLayerList.index;
			string task = ((UnityEditor.Animations.AnimatorControllerLayer)unityLayerList.list[index]).name;
			layerRenameOverlay.BeginRename(task, index, 0.1f);
		}
		goto IL_0035;
		IL_0035:
		if (m_ProxyReg.type != EventType.MouseUp || m_ProxyReg.button != 1 || !first.Contains(m_ProxyReg.mousePosition))
		{
			return;
		}
		contextLayerIndex = cfgY;
		layerContextController = (UnityEditor.Animations.AnimatorController)toolAnimatorControllerField.GetValue(layerViewHostField.GetValue(ReadAnnotation()));
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Delete"), on: false, CompareMapper);
		genericMenu.AddItem(new GUIContent("Duplicate"), on: false, SetMapper);
		genericMenu.AddItem(new GUIContent("Copy"), on: false, PostMapper);
		genericMenu.AddItem(new GUIContent("Paste"), on: false, (copiedLayer != null) ? new GenericMenu.MenuFunction(SetupMapper) : null);
		genericMenu.AddSeparator("");
		UnityEditor.Animations.AnimatorControllerLayer structReg = VisitMapper();
		string[] source = ReadMapper(structReg).ToArray();
		if (categoryNames == null)
		{
			InsertMapper();
		}
		string[] array = categoryNames;
		foreach (string m_StateReg in array)
		{
			bool _GlobalReg = source.Contains(m_StateReg);
			genericMenu.AddItem(("Category Tag/" + m_StateReg).DeleteResolver(tokenneeded: true), _GlobalReg, delegate
			{
				string text = "_category:" + m_StateReg;
				if (_GlobalReg)
				{
					structReg.RemoveTag(text);
				}
				else
				{
					structReg.AddTag(text);
				}
				EditorUtility.SetDirty(ActiveController());
				DisableMapper();
			});
		}
		genericMenu.AddItem("Category Tag/[New Category]".DeleteResolver(tokenneeded: true), on: false, delegate
		{
			QuickInputWindow quickInputWindow = QuickInputWindow.Create("New Category", EditorUtils.Args<QuickInputWindow.FieldType>(QuickInputWindow.FieldType.String), EditorUtils.Args<GUIContent>("Category Name".DeleteResolver(tokenneeded: true)), delegate(object[] results)
			{
				structReg.AddTag("_category:" + (string)results[0]);
				DisableMapper();
			}, _003C_003Ec.watcherInitializer.ReadObserver);
			quickInputWindow.SetValue(0, EditorUtils.ChangeRules("New Category", categoryNames));
			quickInputWindow.ShowAt(m_ProxyReg.mousePosition);
		});
		genericMenu.AddSeparator("");
		if (contextLayerIndex == 0)
		{
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Masks"), on: false, MoveMapper);
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/From Animator"), on: false, ConcatMapper);
			genericMenu.AddItem(new GUIContent("Build Cumulative Mask/From Layers/Generic"), on: false, ComputeMapper);
		}
		genericMenu.AddItem(new GUIContent("Build Mask/From Animator"), on: false, PublishMapper);
		genericMenu.AddItem(new GUIContent("Build Mask/Generic"), on: false, EnableMapper);
		genericMenu.ShowAsContext();
		categoryMenuMousePosition = Event.current.mousePosition;
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
			frameLayerRequested = EditorSettings.GetInstance().autoFrameLayer;
		}
	}

	private static bool AddNewLayerPrefix(object __instance)
	{
		UnityEditor.Animations.AnimatorController animatorController = (UnityEditor.Animations.AnimatorController)toolAnimatorControllerField.GetValue(__instance);
		string text = "New Layer";
		if (layerViewType == LayerViewViewType.CategoryByName && currentLayerCategory != layerCategoryRoot && currentLayerCategory.CategoryPath() != ValidateInitializer())
		{
			text = currentLayerCategory.CategoryPath() + PushInitializer() + text;
		}
		text = animatorController.MakeUniqueLayerName(text);
		UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = animatorController.AddLayer(text, EditorSettings.GetInstance().defaultLayerWeight, EditorSettings.GetInstance().defaultLayerMask.GetValue<AvatarMask>());
		animatorControllerToolType.GetAnyProperty("selectedLayerIndex").SetValue(__instance, animatorController.layers.Length - 1);
		object value = animatorControllerToolType.GetAnyField("m_LayerEditor").GetValue(__instance);
		layerScrollField.SetValue(value, new Vector2(0f, animatorController.layers.Length * 40));
		animatorControllerLayer.stateMachine.entryPosition = EditorSettings.GetInstance().defaultEntryPosition.DeleteDefinition();
		animatorControllerLayer.stateMachine.anyStatePosition = EditorSettings.GetInstance().defaultAnyPosition.DeleteDefinition();
		animatorControllerLayer.stateMachine.exitPosition = EditorSettings.GetInstance().defaultExitPosition.DeleteDefinition();
		if (layerViewType == LayerViewViewType.CategoryByTag && currentLayerCategory != layerCategoryRoot && currentLayerCategory.CategoryPath() != ValidateInitializer())
		{
			animatorControllerLayer.AddTag("_category:" + currentLayerCategory.CategoryPath());
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
		onRemoveLayerMethod.Invoke(ReadAnnotation(), new object[1] { contextLayerIndex });
	}

	private static void SetMapper()
	{
		UnityEditor.Animations.AnimatorControllerLayer item = EditorUtils.CopyLayer(layerContextController.layers[contextLayerIndex], layerContextController);
		if (contextLayerIndex < layerContextController.layers.Length - 2)
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = layerContextController.layers;
			ArrayUtility.Insert(ref array, contextLayerIndex + 1, item);
			ArrayUtility.RemoveAt(ref array, array.Length - 1);
			layerContextController.layers = array;
		}
	}

	private static void PostMapper()
	{
		copiedLayer = layerContextController.layers[contextLayerIndex];
	}

	private static void SetupMapper()
	{
		if (copiedLayer != null)
		{
			UnityEditor.Animations.AnimatorControllerLayer item = EditorUtils.CopyLayer(copiedLayer, layerContextController);
			if (contextLayerIndex < layerContextController.layers.Length - 2)
			{
				UnityEditor.Animations.AnimatorControllerLayer[] array = layerContextController.layers;
				ArrayUtility.Insert(ref array, contextLayerIndex + 1, item);
				ArrayUtility.RemoveAt(ref array, array.Length - 1);
				layerContextController.layers = array;
			}
		}
	}

	private static void EnableMapper()
	{
		CancelMapper();
	}

	private static void PublishMapper()
	{
		if (ControllerEditorWindow.alwaysUseTargetAnimator && (bool)ControllerEditorWindow.targetAnimator)
		{
			CancelMapper(ControllerEditorWindow.targetAnimator.transform);
		}
		else
		{
			ValidateAnnotation(PopMapper);
		}
	}

	private static void PopMapper(object[] ident)
	{
		if (!RateAnnotation(ident[0] == null, "No Animator was given!"))
		{
			ControllerEditorWindow.targetAnimator = (Animator)ident[0];
			ControllerEditorWindow.alwaysUseTargetAnimator = (bool)ident[2];
			CancelMapper(ControllerEditorWindow.targetAnimator.transform);
		}
	}

	private static void ComputeMapper()
	{
		CountMapper(null);
	}

	private static void MoveMapper()
	{
		string text = string.Concat(EditorSettings.GetInstance().saveFolder, "/Generated Masks/", layerContextController.name);
		EditorUtils.EnsureDirectoryExists(text);
		AvatarMask avatarMask = EditorUtils.CreateBaseLayerMask(layerContextController);
		if ((bool)avatarMask)
		{
			AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + layerContextController.name + ".mask"));
			UnityEditor.Animations.AnimatorControllerLayer[] layers = layerContextController.layers;
			layers[0].avatarMask = avatarMask;
			layerContextController.layers = layers;
			EditorUtility.SetDirty(layerContextController);
		}
	}

	private static void ConcatMapper()
	{
		if (ControllerEditorWindow.alwaysUseTargetAnimator && (bool)ControllerEditorWindow.targetAnimator)
		{
			CountMapper(ControllerEditorWindow.targetAnimator.transform);
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
			ControllerEditorWindow.targetAnimator = (Animator)v[0];
			ControllerEditorWindow.alwaysUseTargetAnimator = (bool)v[2];
			CountMapper(ControllerEditorWindow.targetAnimator.transform);
		}
	}

	private static void CancelMapper(Transform setup = null)
	{
		UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = layerContextController.layers[contextLayerIndex];
		AvatarMask avatarMask = EditorUtils.CreateMaskForLayer(animatorControllerLayer, setup);
		string text = string.Concat(EditorSettings.GetInstance().saveFolder, "/Generated Masks/", layerContextController.name);
		EditorUtils.EnsureDirectoryExists(text);
		AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + animatorControllerLayer.name + ".mask"));
		UnityEditor.Animations.AnimatorControllerLayer[] layers = layerContextController.layers;
		layers[contextLayerIndex].avatarMask = avatarMask;
		layerContextController.layers = layers;
	}

	private static void CountMapper(Transform task)
	{
		string text = string.Concat(EditorSettings.GetInstance().saveFolder, "/Generated Masks/", layerContextController.name);
		EditorUtils.EnsureDirectoryExists(text);
		AvatarMask avatarMask = EditorUtils.CreateCombinedMask(layerContextController, task);
		AssetDatabase.CreateAsset(avatarMask, AssetDatabase.GenerateUniqueAssetPath(text + "/" + layerContextController.name + ".mask"));
		UnityEditor.Animations.AnimatorControllerLayer[] layers = layerContextController.layers;
		layers[0].avatarMask = avatarMask;
		layerContextController.layers = layers;
		EditorUtility.SetDirty(layerContextController);
	}

	private static void DisableMapper()
	{
		InsertMapper();
		if (layerViewType == LayerViewViewType.DefaultView)
		{
			return;
		}
		try
		{
			if (ActiveController() == null)
			{
				return;
			}
			UnityEditor.Animations.AnimatorControllerLayer[] layers = ActiveController().layers;
			string text = currentLayerCategory?.CategoryPath();
			layerCategoryRoot = new LayerPathNode("Root", "Root");
			LayerViewViewType layerViewViewType = layerViewType;
			if (layerViewViewType == LayerViewViewType.CategoryByName)
			{
				for (int i = 0; i < layers.Length; i++)
				{
					UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = layers[i];
					layerCategoryRoot.AddLayer(animatorControllerLayer.name, animatorControllerLayer, i);
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
							layerCategoryRoot.AddLayer(text2 + PushInitializer() + "DUMMY", animatorControllerLayer2, j);
						}
					}
					else
					{
						layerCategoryRoot.AddLayer(ValidateInitializer(), animatorControllerLayer2, j);
					}
				}
			}
			Stack<LayerPathNode> stack = new Stack<LayerPathNode>();
			stack.Push(layerCategoryRoot);
			while (stack.Count > 0)
			{
				LayerPathNode layerPathNode = stack.Pop();
				foreach (LayerPathNode child in layerPathNode.children)
				{
					stack.Push(child);
				}
				layerPathNode.children.Sort((LayerPathNode c1, LayerPathNode c2) => string.Compare(c1.name, c2.name, StringComparison.Ordinal));
				if ((bool)EditorSettings.GetInstance().sortCategoryViewLayers)
				{
					layerPathNode.layers.Sort((LayerIndexEntry l1, LayerIndexEntry l2) => string.Compare(l1.layer.name, l2.layer.name, StringComparison.Ordinal));
				}
				LayerPathNode baseCategoryNode = layerPathNode.baseCategoryNode;
				if (baseCategoryNode != null)
				{
					layerPathNode.children.Remove(baseCategoryNode);
					layerPathNode.children.Add(baseCategoryNode);
				}
			}
			currentLayerCategory = ((!text.IsNullOrEmpty()) ? (layerCategoryRoot.FindNode(text) ?? layerCategoryRoot) : layerCategoryRoot);
			AddMapper();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		categoryLayerList.GrabKeyboardFocus();
	}

	private static void InsertMapper()
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = ActiveController().layers;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in layers.SelectMany(ReadMapper))
		{
			hashSet.Add(item);
		}
		categoryNames = hashSet.OrderBy((string n) => n).ToArray();
	}

	private static void RestartMapper()
	{
		bool _ProducerReg = true;
		layerCategoryRoot?.WalkPath(currentLayerCategory.CategoryPath(), delegate(LayerPathNode c)
		{
			LayerPathNode[] array = c.children.Where(_003C_003Ec.watcherInitializer.AwakeObserver).ToArray();
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
					string[] array2 = QueryMapper(currentLayerCategory.CategoryPath());
					LayerPathNode[] array3 = array;
					foreach (LayerPathNode layerPathNode in array3)
					{
						string[] array4 = QueryMapper(layerPathNode.CategoryPath());
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
						Rect var = def.SliceLeft(visitor, isfield: true);
						using (new GUIColorScope(GUIColorScope.ColoringType.BG, skipcont, EditorUtils.validColor, Color.gray))
						{
							if (EditorUtils.CancelQueue(var, layerPathNode.name, EditorStyles.toolbarButton))
							{
								if (currentLayerCategory == layerPathNode)
								{
									int num2 = layerPathNode.CategoryPath().LastIndexOf(PushInitializer(), StringComparison.Ordinal);
									currentLayerCategory = ((num2 <= 0) ? layerCategoryRoot : (layerCategoryRoot.FindNode(layerPathNode.CategoryPath().Substring(0, num2)) ?? layerCategoryRoot));
								}
								else
								{
									currentLayerCategory = layerPathNode;
								}
								AddMapper();
								categoryLayerList.index = 0;
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
		return task.Split(EditorUtils.Args<string>(PushInitializer()), StringSplitOptions.None);
	}

	private static void AddMapper()
	{
		_003C_003Ec__DisplayClass616_0 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass616_0();
		CS_0024_003C_003E8__locals9.m_IteratorReg = ReadAnnotation();
		if (CS_0024_003C_003E8__locals9.m_IteratorReg == null)
		{
			return;
		}
		CS_0024_003C_003E8__locals9._PublisherReg = ((categoryLayerList != null && categoryLayerList.HasSelection()) ? ((LayerIndexEntry)categoryLayerList.list[categoryLayerList.index]).layer.name : "");
		drawLayerCallback = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.ElementCallbackDelegate>("OnDrawLayer");
		selectLayerCallback = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnSelectLayer");
		mouseUpLayerCallback = CS_0024_003C_003E8__locals9.ReflectThread<ReorderableList.SelectCallbackDelegate>("OnMouseUpLayer");
		categoryLayerList = new ReorderableList(currentLayerCategory.layers, typeof(LayerIndexEntry), draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
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
		if (!CS_0024_003C_003E8__locals9._PublisherReg.IsNullOrEmpty())
		{
			int num = currentLayerCategory.layers.FindIndex((LayerIndexEntry l) => l.layer.name == CS_0024_003C_003E8__locals9._PublisherReg);
			if (num >= 0)
			{
				categoryLayerList.index = num;
			}
		}
	}

	private static void InvokeMapper(Rect last, int mean_cont, bool createrule, bool bool_0)
	{
		if (mean_cont.IsValidIndex(currentLayerCategory.layers))
		{
			int layerIndex = currentLayerCategory.layers[mean_cont].layerIndex;
			if (layerIndex.IsValidIndex(unityLayerList.list))
			{
				drawLayerCallback(last, layerIndex, bool_0, createrule);
			}
		}
	}

	private static void FindMapper(ReorderableList res)
	{
		InitMapper(selectLayerCallback);
	}

	private static void ExcludeMapper(ReorderableList i)
	{
		InitMapper(mouseUpLayerCallback);
	}

	private static void InitMapper(ReorderableList.SelectCallbackDelegate instance)
	{
		int index = StartMapper(isreference: true);
		unityLayerList.index = index;
		instance(unityLayerList);
	}

	private static UnityEditor.Animations.AnimatorControllerLayer VisitMapper()
	{
		return DefineMapper(StartMapper());
	}

	private static UnityEditor.Animations.AnimatorControllerLayer DefineMapper(int sizelast)
	{
		if (RateInitializer())
		{
			return (UnityEditor.Animations.AnimatorControllerLayer)unityLayerList.list[sizelast];
		}
		return currentLayerCategory.layers[sizelast].layer;
	}

	private static int StartMapper(bool isreference = false)
	{
		if (layerViewType != LayerViewViewType.DefaultView)
		{
			if (isreference)
			{
				if (!categoryLayerList.index.IsValidIndex(categoryLayerList.list))
				{
					return -1;
				}
				return currentLayerCategory.layers[categoryLayerList.index].layerIndex;
			}
			return categoryLayerList.index;
		}
		return unityLayerList.index;
	}

	private static IEnumerable<string> ReadMapper(UnityEditor.Animations.AnimatorControllerLayer var1)
	{
		IEnumerable<string> systemTags = var1.GetSystemTags();
		foreach (string item in systemTags)
		{
			Match match = Regex.Match(item, "^_category:(.+)$");
			if (match.Success)
			{
				yield return match.Groups[1].Value;
			}
		}
	}

	[CallbackMethod(1)]
	private static void SelectMapper()
	{
		if (unityLayerList == null)
		{
			object obj = ReadAnnotation();
			if (obj != null)
			{
				unityLayerList = (ReorderableList)layerListField.GetValue(obj);
			}
		}
	}

	private static void RemoveMapper()
	{
		HarmonyPatchManager.MapReg(typeof(Unsupported), "PasteToStateMachineFromPasteboard", HarmonyPatchManager.InsertTests<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPrefix), HarmonyPatchManager.PushReg<AnimatorStateMachine, ChildAnimatorState[]>(PasteToStateMachineFromPasteboardPost));
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
			select EditorUtils.StripNumberSuffix(cs.state.name));
		AnimatorState[] array2 = array;
		foreach (AnimatorState obj in array2)
		{
			string item = (obj.name = EditorUtils.ChangeRules(EditorUtils.StripNumberSuffix(obj.name), hashSet));
			hashSet.Add(item);
		}
		UnityEngine.Object[] objects = array;
		Selection.objects = objects;
	}

	private static void PrimeMenuAndLayerEditorReflection()
	{
		menuItemConstructor = EditorUtils.RequireQualifiedType("UnityEditor.GenericMenu+MenuItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetConstructor(new Type[4]
		{
			typeof(GUIContent),
			typeof(bool),
			typeof(bool),
			typeof(GenericMenu.MenuFunction)
		});
		advancedPopupMethod = typeof(EditorGUI).InsertList("AdvancedPopup", new Type[3]
		{
			typeof(Rect),
			typeof(int),
			typeof(string[])
		});
		getBuiltinSkinMethod = typeof(GUIUtility).DisableList("GetBuiltinSkin");
		layerEditorField = animatorControllerToolType.GetAnyField("m_LayerEditor");
		previewAnimatorField = animatorControllerToolType.GetAnyField("m_PreviewAnimator");
		liveLinkProperty = animatorControllerToolType.GetAnyProperty("liveLink");
		selectedLayerIndexProperty = layerControllerViewType.GetAnyProperty("selectedLayerIndex");
		layerRenameOverlay = new RenameOverlayWrapper(() => layerControllerViewType.GetMethod("get_renameOverlay").Invoke(ReadAnnotation(), null));
		stateRenameOverlay = new RenameOverlayWrapper();
		stateRenameOverlay.onEndRename = delegate(bool accepted)
		{
			if (accepted)
			{
				RestartAlgo(ActiveStateMachine(), selectedStates, stateRenameOverlay.Name());
			}
		};
	}

	private static void AwakeMapper()
	{
		HarmonyPatchManager.MapReg(typeof(GenericMenu), "ShowAsContext", null, HarmonyPatchManager.NewReg<object>(ShowAsContextPost));
	}

	private static void ShowAsContextPost(object __instance)
	{
		contextMenu = (GenericMenu)__instance;
	}

	private static void PrimeGraphNodeReflection()
	{
		stateMachineNodeBaseType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationStateMachine.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		graphNodeType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		blendTreeNodeType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		edgeGUIPatchType = EditorUtils.RequireQualifiedType("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		entryNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.entryNode.ResolvedType().DisableList("MakeTransitionCallback");
		anyStateNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.anyStateNode.ResolvedType().DisableList("MakeTransitionCallback");
		stateNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.stateNode.ResolvedType().DisableList("MakeTransitionCallback");
		stateMachineNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.stateMachineNode.ResolvedType().DisableList("MakeTransitionCallback");
		findClosestEdgeMethod = edgeGUIPatchType.DisableList("FindClosestEdge");
		genericMenuForStateMachineNodeMethod = stateMachineNodeBaseType.DisableList("GenericMenuForStateMachineNode");
		stateNodeStateField = AnimatorGraphReflection.TypeResolvers.stateNode.ResolvedType().GetAnyField("state");
		blendTreeNodeMotionField = blendTreeNodeType.GetAnyField("motion");
		blendTreeNodeChildrenField = blendTreeNodeType.GetAnyField("children");
		blendTreeNodeParentProperty = blendTreeNodeType.GetAnyProperty("parent");
	}

	private static void FlushMapper()
	{
		HarmonyPatchManager.MapReg(stateMachineGraphType, "CreateNodeFromState", null, HarmonyPatchManager.NewReg<ChildAnimatorState>(CreateNodeFromStatePost));
		HarmonyPatchManager.MapReg(stateMachineGraphType, "CreateNodeFromStateMachine", null, HarmonyPatchManager.PushReg<object, ChildAnimatorStateMachine>(CreateNodeFromStateMachinePost));
		HarmonyPatchManager.MapReg(stateMachineGraphType, "CreateNodes", null, HarmonyPatchManager.NewReg<object>(CreateNodesPost));
		HarmonyPatchManager.MapReg(stateMachineGraphGUIType, "AddStateEmptyCallback", HarmonyPatchManager.StopReg<object, object, bool>(AddEmptyStatePrefix));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.stateNode, "NodeUI", HarmonyPatchManager.NewReg<Node>(StateNodeUIPrefix), HarmonyPatchManager.MethodOf(StateNodeUIPost));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.stateMachineNode, "NodeUI", HarmonyPatchManager.NewReg<Node>(smethod_3), HarmonyPatchManager.MethodOf(MachineNodeUIPost));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.entryNode, "NodeUI", HarmonyPatchManager.NewReg<Node>(EntryStateNodeUIPrefix), HarmonyPatchManager.MethodOf(smethod_4));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.exitNode, "NodeUI", HarmonyPatchManager.NewReg<Node>(ExitNodeUIPrefix), HarmonyPatchManager.MethodOf(ExitNodeUIPost));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.anyStateNode, "NodeUI", HarmonyPatchManager.NewReg<Node>(smethod_5), HarmonyPatchManager.MethodOf(smethod_6));
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "EndSlotDragging", HarmonyPatchManager.MethodOf(EndSlotDraggingPrefix), HarmonyPatchManager.MethodOf(EndSlotDraggingPost));
		HarmonyPatchManager.MapReg(edgeGUIPatchType, "EndDragging", null, HarmonyPatchManager.MethodOf(EndDraggingPost));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.anyStateNode, "Connect", null, HarmonyPatchManager.PushReg<object, object>(AnyStateNodeConnectPost));
		HarmonyPatchManager.MapReg(AnimatorGraphReflection.TypeResolvers.stateNode, "Connect", null, HarmonyPatchManager.PushReg<Node, Node>(StateNodeConnectPost));
		HarmonyPatchManager.MapReg(blendTreeGraphGUIType, "HandleNodeInput", HarmonyPatchManager.OrderTests<bool>(TreeNodeInputPrefix), HarmonyPatchManager.PushReg<object, bool>(TreeNodeInputPost));
		HarmonyPatchManager.MapReg(blendTreeGraphGUIType, "NodeGUI", HarmonyPatchManager.NewReg<object>(TreeNodeGUIPrefix), HarmonyPatchManager.NewReg<Node>(TreeNodeGUIPost));
		HarmonyPatchManager.MapReg(stateMachineGraphGUIType, "NodeGUI", HarmonyPatchManager.InsertTests<Node, AnimatorState>(GraphGUINodeGUIPrefix), HarmonyPatchManager.NewReg<AnimatorState>(smethod_7));
	}

	private static void CreateNodeFromStatePost(ChildAnimatorState state)
	{
		AnimatorState state2 = state.state;
		AnimatorGraphReflection.GraphNodeRef graphNodeRef = AnimatorGraphReflection.FindNode(state2);
		if ((bool)EditorSettings.GetInstance().cosmeticNodesActive)
		{
			graphNodeRef.Node().color = (Styles.Color)EditorSettings.GetInstance().normalStateNodeColor.GetValue();
		}
		string[] array = InvokeAlgo(state2);
		foreach (string text in array)
		{
			if (stateStylesByTag.ContainsKey(text))
			{
				graphNodeRef.Node().style = text;
			}
		}
	}

	private static void CreateNodeFromStateMachinePost(object __instance, ChildAnimatorStateMachine subStateMachine)
	{
		if ((bool)EditorSettings.GetInstance().cosmeticNodesActive)
		{
			AnimatorGraphReflection.FindNode(subStateMachine.stateMachine).Node().color = (Styles.Color)EditorSettings.GetInstance().machineStateNodeColor.GetValue();
		}
	}

	private static void CreateNodesPost(object __instance)
	{
		if ((bool)EditorSettings.GetInstance().cosmeticNodesActive)
		{
			AnimatorGraphReflection.GraphAccessors.EntryNode().Color((Styles.Color)EditorSettings.GetInstance().entryStateNodeColor.GetValue());
			AnimatorGraphReflection.GraphAccessors.ExitNode().Color((Styles.Color)EditorSettings.GetInstance().exitStateNodeColor.GetValue());
			AnimatorGraphReflection.GraphAccessors.AnyStateNode().Color((Styles.Color)EditorSettings.GetInstance().anyStateNodeColor.GetValue());
			if ((bool)RootStateMachine().defaultState)
			{
				AnimatorGraphReflection.FindNode(RootStateMachine().defaultState).Node().color = (Styles.Color)EditorSettings.GetInstance().defaultStateNodeColor.GetValue();
			}
		}
	}

	private static bool AddEmptyStatePrefix(object __instance, object data)
	{
		AnimatorState animatorState = ActiveStateMachine().AddState(EditorSettings.GetInstance().defaultState.name, ((Vector2)data).PostPredicate(10));
		QueryAlgo(animatorState);
		animatorState.motion = EditorSettings.GetInstance().defaultState.motion;
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
					CodeInstruction[] collection = new CodeInstruction[2]
					{
						new CodeInstruction(OpCodes.Ldloc, operand),
						new CodeInstruction(OpCodes.Call, m_ComparatorVisitor)
					};
					list.InsertRange(num + 1, collection);
					num += 2;
				}
			}
		}
		return list;
	}

	private static void ConnectMapper(Node config, int contend)
	{
		nodeContextClickPending = false;
		bool flag = contend <= 1;
		Event current = Event.current;
		bool flag2 = current.control && current.shift;
		switch (current.type)
		{
		case EventType.MouseDown:
			if (!slotDragActive)
			{
				if (!flag2 || current.clickCount != 2)
				{
					if (contend == 3 || flag2 || current.clickCount != 2 || (flag && !transitionDragPending && !(current.control ^ (bool)EditorSettings.GetInstance().switchDoubleClick)))
					{
						if (!transitionDragArmed || current.clickCount != 1)
						{
							if (transitionDragPending && current.clickCount == 1)
							{
								transitionDragPending = false;
							}
						}
						else
						{
							transitionDragArmed = false;
							transitionDragPending = true;
						}
					}
					else
					{
						(contend switch
						{
							4 => anyStateNodeMakeTransitionCallback, 
							2 => entryNodeMakeTransitionCallback, 
							1 => stateMachineNodeMakeTransitionCallback, 
							_ => stateNodeMakeTransitionCallback, 
						}).Invoke(config, null);
						pendingTransitionSourceKind = contend;
						pendingTransitionSourceNode = config;
						resumeTransitionDragAfterSlotDrag = true;
						transitionDragArmed = true;
						current.Use();
					}
				}
				else if (!slotDragActive)
				{
					slotDragSourceNode = config;
					slotDragActive = true;
					edgeGUIPatchType.DisableList("BeginSlotDragging").Invoke(ExcludeAnnotation(), new object[3]
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
				slotDragActive = false;
				if (placeholderTransitionTarget == null)
				{
					placeholderTransitionTarget = new AnimatorState
					{
						name = "Amogus"
					};
				}
				if (placeholderTransition == null)
				{
					placeholderTransition = new AnimatorStateTransition
					{
						destinationState = placeholderTransitionTarget,
						exitTime = 69f,
						duration = 420f,
						offset = 80085f
					};
				}
				object obj = ExcludeAnnotation();
				stateMachineGraphType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m2) => m2.Name == "CreateEdges" && m2.GetParameters().Length == 3).Invoke(StartAnnotation(), new object[3]
				{
					slotDragSourceNode,
					config,
					Activator.CreateInstance(AnimatorGraphReflection.TypeResolvers.transitionEditionContext.ResolvedType(), placeholderTransition, null, null, null, null)
				});
				edgeGUIPatchType.DisableList("EndDragging").Invoke(obj, null);
				current.Use();
			}
			break;
		case EventType.ContextClick:
			if (contend == 3)
			{
				current.Use();
			}
			nodeContextClickPending = true;
			break;
		}
	}

	private static void CalculateMapper(int next_task)
	{
		if (!nodeContextClickPending)
		{
			return;
		}
		nodeContextClickPending = false;
		bool flag;
		if (flag = next_task == 3)
		{
			contextMenu = new GenericMenu();
		}
		IList menuItems = contextMenu.GetMenuItems();
		if (next_task == 0)
		{
			AnimatorState animatorState = Selection.activeObject as AnimatorState;
			if ((bool)animatorState && animatorState.motion is UnityEditor.Animations.BlendTree)
			{
				blendTreeBreadcrumbState = animatorState;
				object value = menuItemConstructor.Invoke(new object[4]
				{
					new GUIContent("Edit BlendTree"),
					false,
					false,
					new GenericMenu.MenuFunction(TestMapper)
				});
				menuItems.Insert(3, value);
			}
		}
		bool flag2 = false;
		if ((replicateTransitionsMode && !flag) || redirectTransitionsMode)
		{
			contextMenu.AddSeparator(string.Empty);
			if (redirectTransitionsMode)
			{
				contextMenu.AddItem(new GUIContent("Redirect Transitions"), on: true, LoginAlgo);
			}
			if (replicateTransitionsMode && !flag)
			{
				contextMenu.AddItem(new GUIContent("Replicate Transitions"), on: true, CreateAlgo);
			}
			flag2 = true;
		}
		if (!flag || makeMultipleTransitionsMode)
		{
			object value2 = menuItemConstructor.Invoke(new object[4]
			{
				new GUIContent("Make Multiple Transitions"),
				false,
				makeMultipleTransitionsMode,
				new GenericMenu.MenuFunction(MoveAlgo)
			});
			menuItems.Insert((!flag) ? 1 : 0, value2);
			flag2 = true;
		}
		if (next_task <= 1)
		{
			contextMenu.AddItem(new GUIContent("Pack into StateMachine"), on: false, CallAlgo);
			flag2 = true;
		}
		if (flag2)
		{
			contextMenu.AddSeparator(string.Empty);
		}
		if (next_task != 1)
		{
			contextMenu.AddItem(new GUIContent("Select Shared Transitions"), on: false, IncludeAlgo);
			if (!flag)
			{
				contextMenu.AddItem(new GUIContent("Select Out Transitions"), on: false, GetAlgo);
			}
			if (next_task != 2 && next_task != 4)
			{
				contextMenu.AddItem(new GUIContent("Select In Transitions"), on: false, CalcAlgo);
			}
		}
		if (next_task <= 1)
		{
			if (next_task == 1)
			{
				contextMenu.AddItem(new GUIContent("Unpack StateMachine"), on: false, CancelAlgo);
			}
			contextMenu.AddSeparator(string.Empty);
			contextMenu.AddItem(new GUIContent("Behaviours/Copy"), on: false, ReadAlgo);
			contextMenu.AddItem(new GUIContent("Behaviours/Paste"), on: false, RemoveAlgo() ? new GenericMenu.MenuFunction(SelectAlgo) : null);
			contextMenu.AddItem(new GUIContent("Behaviours/Remove"), on: false, InstantiateAlgo);
			foreach (string styleMenuName in styleMenuNames)
			{
				string m_ProcReg = "ce_" + styleMenuName;
				bool m_WrapperTests = selectedStates.All((AnimatorState state) => state.tag == m_ProcReg);
				contextMenu.AddItem(new GUIContent("Styles/" + styleMenuName), m_WrapperTests, delegate
				{
					UnityEngine.Object[] objectsToUndo = selectedStates.ToArray();
					Undo.RecordObjects(objectsToUndo, "Set State Style");
					foreach (AnimatorState selectedState in selectedStates)
					{
						selectedState.tag = ((!m_WrapperTests) ? m_ProcReg : "");
						selectedState.SetDirty();
					}
					rebuildGraphRequested = true;
				});
			}
		}
		contextMenu.ShowAsContext();
	}

	private static void TreeNodeGUIPrefix(object n)
	{
	}

	private static void TreeNodeGUIPost(Node n)
	{
		string text = $"TreeNode{n.GetInstanceID()}";
		if (Event.current.type == EventType.Repaint)
		{
			EditorUtils.SetGuiState(text, n.position);
		}
		Rect value = EditorUtils.OrderQueue(text, Rect.zero);
		float x = 0f;
		value.y = 0f;
		value.x = x;
		EditorUtils.HandleMultiDragAndDrop(value, delegate(IEnumerable<Motion> motions)
		{
			_003C_003Ec__DisplayClass670_1 _003C_003Ec__DisplayClass670_ = new _003C_003Ec__DisplayClass670_1();
			object value2 = blendTreeNodeMotionField.GetValue(n);
			_003C_003Ec__DisplayClass670_.visitorTests = value2 as UnityEditor.Animations.BlendTree;
			if ((object)_003C_003Ec__DisplayClass670_.visitorTests != null)
			{
				Undo.RecordObject(_003C_003Ec__DisplayClass670_.visitorTests, "DragNDrop Motions");
				_003C_003Ec__DisplayClass670_.visitorTests.children = _003C_003Ec__DisplayClass670_.visitorTests.children.Concat(motions.Select(_003C_003Ec__DisplayClass670_.CollectThread)).ToArray();
			}
			else
			{
				object value3 = blendTreeNodeParentProperty.GetValue(n);
				if (value3 != null)
				{
					IList list = (IList)blendTreeNodeChildrenField.GetValue(value3);
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
					UnityEditor.Animations.BlendTree obj2 = (UnityEditor.Animations.BlendTree)blendTreeNodeMotionField.GetValue(value3);
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
		UnityEditor.Animations.BlendTree algoTests = blendTreeNodeMotionField.GetValue(node) as UnityEditor.Animations.BlendTree;
		if ((bool)algoTests)
		{
			contextMenu.AddItem("Add Root Tree".CreateResolver(), on: false, delegate
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
					algoTests.MarkDirty();
					blendTree.MarkDirty();
				}
				else
				{
					DestroyAnnotation("Target tree is not saved to assets!");
				}
			});
			contextMenu.ShowAsContext();
		}
		nodeContextClickPending = false;
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
		addBreadCrumbMethod.Invoke(AnimatorGraphReflection.GraphAccessors.Tool(), new object[2] { blendTreeBreadcrumbState, true });
	}

	private static void EndSlotDraggingPrefix()
	{
		slotDraggingEnded = true;
	}

	private static void EndSlotDraggingPost()
	{
		if (resumeTransitionDragAfterSlotDrag)
		{
			MethodInfo methodInfo;
			switch (pendingTransitionSourceKind)
			{
			case 4:
				methodInfo = anyStateNodeMakeTransitionCallback;
				goto IL_002d;
			case 2:
				methodInfo = entryNodeMakeTransitionCallback;
				goto IL_002d;
			case 3:
				methodInfo = stateNodeMakeTransitionCallback;
				goto IL_002d;
			case 1:
				{
					methodInfo = stateMachineNodeMakeTransitionCallback;
					goto IL_002d;
				}
				IL_002d:
				methodInfo.Invoke(pendingTransitionSourceNode, null);
				break;
			}
			resumeTransitionDragAfterSlotDrag = true;
			transitionDragArmed = true;
		}
	}

	private static void EndDraggingPost()
	{
		resumeTransitionDragAfterSlotDrag = false;
		if (!slotDraggingEnded)
		{
			transitionDragArmed = false;
			transitionDragPending = false;
		}
		slotDraggingEnded = false;
	}

	private static void GraphGUINodeGUIPrefix(Node n, out AnimatorState __state)
	{
		if (n.GetType() != AnimatorGraphReflection.TypeResolvers.stateNode)
		{
			__state = null;
		}
		else
		{
			__state = (AnimatorState)stateNodeStateField.GetValue(n);
		}
		if (__state == null)
		{
			return;
		}
		currentNodeSize = n.position.size;
		if (VisitAlgo(__state))
		{
			return;
		}
		AnimatorState m_MapperTests = __state;
		EditorSettings.StateCosmeticOptions stateCosmetics = EditorSettings.GetInstance().GetStateCosmetics();
		Rect connection = new Rect(1f, 1f, 11f, 11f);
		if (stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.coordinates))
		{
			float x = n.position.x;
			float y = n.position.y;
			Rect source = new Rect(1f, currentNodeSize.y - 7f, RegisterMapper(x), 7f);
			Rect rect = new Rect(source);
			rect.x = source.width;
			rect.width = RegisterMapper(y);
			Rect rect2 = rect;
			EditorGUI.BeginChangeCheck();
			GUIStyle style = new GUIStyle(EditorUtils.styles().noteLeft)
			{
				fontSize = 7
			};
			x = EditorGUI.DelayedFloatField(source, x, style);
			y = EditorGUI.DelayedFloatField(rect2, y, style);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(ActiveStateMachine(), "Set ChildState Position");
				ChildAnimatorState[] states = ActiveStateMachine().states;
				if (states.TryFindIndex((ChildAnimatorState childAnimatorState) => childAnimatorState.state == m_MapperTests, out var c))
				{
					states[c].position = new Vector3(x, y);
					ActiveStateMachine().states = states;
				}
			}
		}
		if (stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.indicators))
		{
			bool flag = m_MapperTests.behaviours.Length != 0;
			bool writeDefaultValues = m_MapperTests.writeDefaultValues;
			Rect res = new Rect(0f, 0f, currentNodeSize.x, currentNodeSize.y);
			if (stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.inactiveIndicators) || writeDefaultValues)
			{
				GUI.Label(res.SliceRight(20f, isserv: true).WithHeight(14f), EditorUtils.styles().writeDefaults, (!writeDefaultValues) ? EditorUtils.styles().noteCenter : EditorUtils.styles().centeredMiniLabel);
			}
			if (stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.inactiveIndicators) || flag)
			{
				GUI.Label(res.SliceRight(10f, isserv: true).WithHeight(14f), EditorUtils.styles().behaviours, (!flag) ? EditorUtils.styles().noteCenter : EditorUtils.styles().centeredMiniLabel);
			}
		}
		if (!stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.quickNewClip))
		{
			return;
		}
		GUI.Label(connection, "+", GUI.skin.label);
		if (!new EventWrapper(Event.current).IsLeftButton().IsMouseDown().InRect(connection))
		{
			return;
		}
		string text = $"{EditorSettings.GetInstance().lastAnimationPath}/{EditorSettings.GetInstance().lastAnimationName}.anim";
		bool flag2;
		string defaultName = Path.GetFileNameWithoutExtension((!(flag2 = AssetDatabase.IsValidFolder(EditorSettings.GetInstance().lastAnimationPath))) ? text : AssetDatabase.GenerateUniqueAssetPath(text)) + ".anim";
		string text2 = EditorUtility.SaveFilePanel("New Animation Path", flag2 ? ((string)EditorSettings.GetInstance().lastAnimationPath) : "Assets", defaultName, "anim");
		if (string.IsNullOrWhiteSpace(text2))
		{
			return;
		}
		string projectRelativePath = FileUtil.GetProjectRelativePath(text2);
		if (projectRelativePath.StartsWith("Assets"))
		{
			using (new EditorSettings.SettingsChangeScope())
			{
				EditorSettings.GetInstance().lastAnimationPath.SetValue(Path.GetDirectoryName(projectRelativePath).Replace('\\', '/'));
				EditorSettings.GetInstance().lastAnimationName.SetValue(Path.GetFileNameWithoutExtension(projectRelativePath));
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
		GUIStyle gUIStyle = ((!stateStylesByTag.TryGetValue(__state.tag, out value)) ? defaultStateNodeStyle : value);
		if (gUIStyle == null)
		{
			return;
		}
		float num = ((gUIStyle.alignment >= TextAnchor.MiddleLeft) ? (gUIStyle.fixedHeight / 2f - 15f) : 5f);
		int num2 = ((gUIStyle.fontSize != 0) ? (gUIStyle.fontSize + 2) : 15);
		Event current = Event.current;
		EventType type = current.type;
		if (selectedStates.Contains(__state))
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
							stateRenameOverlay.EndRename(isconfig: false);
						}
						goto IL_0089;
					}
				}
				else if (keyCode != KeyCode.KeypadEnter)
				{
					if (keyCode == KeyCode.F2)
					{
						stateRenameOverlay.BeginRename((!flag) ? new Rect(gUIStyle.fixedWidth * 0.15f, num, gUIStyle.fixedWidth * 0.7f, num2) : new Rect(0f, 0f, gUIStyle.fixedWidth, gUIStyle.fixedHeight), __state.name, 0, 0f);
					}
					goto IL_0089;
				}
				if (stateRenameOverlay.IsRenaming())
				{
					stateRenameOverlay.Name(stateRenameOverlay.Name().Replace("\\n", "\n"));
				}
				stateRenameOverlay.EndRename(isconfig: true);
			}
			goto IL_0089;
		}
		goto IL_00d7;
		IL_00d7:
		if (flag)
		{
			return;
		}
		EditorSettings.StateCosmeticOptions stateCosmetics = EditorSettings.GetInstance().GetStateCosmetics();
		bool flag2 = stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.motionName);
		bool flag3 = stateCosmetics.HasFlag(EditorSettings.StateCosmeticOptions.motionIcon);
		if (flag2 || flag3)
		{
			Rect rect = new Rect(0f, num + (float)num2, currentNodeSize.x, 18f);
			bool num3 = __state.motion;
			if (num3 && flag3 && flag2)
			{
				rect.x = -9f;
			}
			GUIContent content = ((!num3) ? ((!flag2) ? GUIContent.none : new GUIContent("(None)")) : new GUIContent((!flag2) ? string.Empty : ("(" + __state.motion.name + ")"), (!flag3) ? null : ((__state.motion is AnimationClip animationClip) ? ((!animationClip.isLooping) ? EditorUtils.contents().animationClip.image : EditorUtils.contents().loopingClip.image) : EditorUtils.contents().blendTrees.image)));
			GUI.Label(rect, content, EditorUtils.styles().centeredMiniLabel);
		}
		return;
		IL_0089:
		if (stateRenameOverlay.IsRenaming())
		{
			string key = stateRenameOverlay.Name();
			stateRenameOverlay.OnGUI(GUI.skin.textArea);
			if (__state == selectedStates[0])
			{
				stateRenameOverlay.OnEvent();
			}
			else
			{
				stateRenameOverlay.Name(key);
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
			dragAndDropPending = true;
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
			if (selectedStates.Count <= 1 || !selectedStates.Contains(config))
			{
				Undo.RecordObject(config, "Drag & Drop to State");
				config.motion = motion;
				config.MarkDirty();
			}
			else
			{
				UnityEngine.Object[] objectsToUndo = selectedStates.ToArray();
				Undo.RecordObjects(objectsToUndo, "Drag & Drop to States");
				foreach (AnimatorState selectedState in selectedStates)
				{
					selectedState.motion = motion;
					selectedState.MarkDirty();
				}
			}
		}
		else if (flag)
		{
			quickToggleState = config;
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
		QuickToggleWindow quickToggleWindow = QuickToggleWindow.AssetTests(selectedStates.Contains(quickToggleState) ? selectedStates : new List<AnimatorState> { quickToggleState }, _InitializerTests, template);
		Vector2 mousePosition = current.mousePosition;
		quickToggleWindow.ShowAt(GUIUtility.GUIToScreenPoint(mousePosition));
		goto IL_0073;
	}

	private static void StateNodeConnectPost(Node __instance, Node toNode)
	{
		AnimatorState definitionTests = (AnimatorState)stateNodeStateField.GetValue(__instance);
		if (AnimatorGraphReflection.TypeResolvers.stateMachineNode != toNode.GetType())
		{
			AnimatorStateTransition animatorStateTransition = definitionTests.transitions.Last();
			CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, animatorStateTransition);
			animatorStateTransition.canTransitionToSelf = true;
			return;
		}
		genericMenuForStateMachineNodeMethod.Invoke(null, new object[3]
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
					CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, animatorStateTransition2);
					animatorStateTransition2.canTransitionToSelf = true;
				}
			}
		});
	}

	private static void AnyStateNodeConnectPost(object __instance, object toNode)
	{
		if (AnimatorGraphReflection.TypeResolvers.stateMachineNode != toNode.GetType())
		{
			CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, RootStateMachine().anyStateTransitions.Last());
			return;
		}
		genericMenuForStateMachineNodeMethod.Invoke(null, new object[3]
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
						col = ((!animatorStateMachine.states.GetRules()) ? RootStateMachine().AddAnyStateTransition(animatorStateMachine) : RootStateMachine().anyStateTransitions.Last());
					}
				}
				else
				{
					col = RootStateMachine().AddAnyStateTransition(destinationState);
				}
				CustomizeAlgo(EditorSettings.GetInstance().defaultTransition, col);
			}
		});
	}

	internal static void ValidateMapper()
	{
		parameterControllerViewType = Type.GetType("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		parameterViewScrollField = parameterControllerViewType.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	private static void CustomizeMapper()
	{
		HarmonyPatchManager.MapReg(parameterControllerViewType, "AddParameterMenu", null, HarmonyPatchManager.NewReg<object>(AddNewParameterPost));
		HarmonyPatchManager.MapReg(parameterControllerViewType, "OnDrawParameter", HarmonyPatchManager.RestartTests<Rect, int, int>(DrawParameterPrefix), HarmonyPatchManager.NewReg<int>(DrawParameterPost));
		HarmonyPatchManager.TestReg("UnityEditor.Graphs.ParameterControllerView+IntElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, HarmonyPatchManager.NewReg<Rect>(IntElementGUIPost));
		HarmonyPatchManager.TestReg("UnityEditor.Graphs.ParameterControllerView+FloatElement, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "OnSpecializedGUI", null, HarmonyPatchManager.NewReg<Rect>(smethod_8));
		HarmonyPatchManager.TestReg("UnityEditor.Graphs.ParameterControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "RenameEnd", HarmonyPatchManager.MethodOf(ParameterRenameEndPrefix), HarmonyPatchManager.MethodOf(ParameterRenameEndPost));
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
		UnityEngine.AnimatorControllerParameter m_RegTests = ActiveController().parameters[__state];
		if (m_RegTests.type == UnityEngine.AnimatorControllerParameterType.Float)
		{
			AnimationClip m_PropertyTests = DefineInitializer();
			bool flag = FindInitializer() != null && m_PropertyTests != null;
			contextMenu.AddItem(new GUIContent("Animate"), on: false, (!flag) ? null : ((GenericMenu.MenuFunction)delegate
			{
				EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Animator), m_RegTests.name);
				if (!RateAnnotation(AnimationUtility.GetEditorCurve(m_PropertyTests, binding) != null, "Property \"" + m_RegTests.name + "\" already exists in the active animation clip \"" + m_PropertyTests.name + "\"."))
				{
					AnimationUtility.SetEditorCurve(m_PropertyTests, binding, new AnimationCurve());
					FindInitializer().Repaint();
				}
			}));
		}
		contextMenu.AddSeparator(string.Empty);
		contextMenu.AddItem(new GUIContent("Convert/Bool"), on: false, delegate
		{
			PublishAlgo(ActiveController(), __state, UnityEngine.AnimatorControllerParameterType.Bool);
		});
		contextMenu.AddItem(new GUIContent("Convert/Int"), on: false, delegate
		{
			PublishAlgo(ActiveController(), __state, UnityEngine.AnimatorControllerParameterType.Int);
		});
		contextMenu.AddItem(new GUIContent("Convert/Float"), on: false, delegate
		{
			PublishAlgo(ActiveController(), __state, UnityEngine.AnimatorControllerParameterType.Float);
		});
		contextMenu.AddItem(new GUIContent("Convert/Trigger"), on: false, delegate
		{
			PublishAlgo(ActiveController(), __state, UnityEngine.AnimatorControllerParameterType.Trigger);
		});
		contextMenu.ShowAsContext();
	}

	private static void AddNewParameterPost(object __instance)
	{
		parameterViewScrollField.SetValue(__instance, new Vector2(0f, 2.1474836E+09f));
	}

	private static void ParameterRenameEndPrefix()
	{
		parameterViewParameters = ActiveController().parameters;
	}

	private static void ParameterRenameEndPost()
	{
		if (!AnimatorTypeCache.IsVRCSDKAvailable())
		{
			return;
		}
		UnityEngine.AnimatorControllerParameter[] parameters = ActiveController().parameters;
		try
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				string text = parameterViewParameters[i].name;
				string text2 = parameters[i].name;
				if (!(text == text2))
				{
					UnityEditor.Animations.AnimatorControllerLayer[] layers = ActiveController().layers;
					for (int j = 0; j < layers.Length; j++)
					{
						PostAlgo(layers[j].stateMachine, text, text2, isord2: true);
					}
					CustomizeAnnotation("Renamed " + text + " to " + text2 + ".");
				}
			}
		}
		catch (Exception arg)
		{
			DestroyAnnotation($"WARNING! Automatic driver renaming failed\n{arg}");
		}
	}

	private static void IntElementGUIPost(Rect rect)
	{
		CalcAnnotation(rect, (!EditorSettings.GetInstance().capitalParameterIndicator) ? "i" : "I");
	}

	private static void smethod_8(Rect rect)
	{
		CalcAnnotation(rect, (!EditorSettings.GetInstance().capitalParameterIndicator) ? "f" : "F");
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
		DisableVisitor(CallVisitor(list.ToArray())).QueryRules(delegate(JsonObject response)
		{
			isActivatingLicense = false;
			SortAnnotation(response, delegate
			{
				licenseKeyEntryRequired = false;
				EditorSettings.GetInstance().a_HasSucceededLastVerification.SetValue(excludeparam: true);
				WriteAnnotation(assetneeded: true);
			});
		}, delegate(Exception exception)
		{
			isActivatingLicense = false;
			Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}, null, null, DrawLicenseInfo);
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
		return ReflectMapper(RunMapper(config), global::_003CModule_003E.smethod_3<int[]>(565931375));
	}

	[CompilerGenerated]
	internal static string NewMapper(string config)
	{
		return LoginMapper(ReflectMapper(config, global::_003CModule_003E.smethod_1<int[]>(943980522)));
	}

	[CompilerGenerated]
	internal static async void PushMapper(ProcessRunner[] key, Action b, CancellationTokenSource serv)
	{
		try
		{
			await Task.Run(delegate
			{
				ProcessRunner[] array = key;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].CancelReg();
				}
			}, serv.Token);
			while (!key.All((ProcessRunner p) => p.m_MockAlgo))
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
		assetInventory = new MethodVisitor(ActiveController());
		subAssetTabIndex = -1;
		subAssetPanelOpen = false;
		writeDefaultsPanelOpen = false;
	}

	[CompilerGenerated]
	internal static void FillMapper()
	{
		bool num = subAssetPanelOpen;
		VerifyMapper();
		subAssetPanelOpen = num;
	}

	[CompilerGenerated]
	internal static void WriteMapper()
	{
		bool num = writeDefaultsPanelOpen;
		VerifyMapper();
		writeDefaultsPanelOpen = num;
	}

	[CompilerGenerated]
	internal static void ForgotMapper(bool istask)
	{
		AnimatorState[] array = assetInventory.m_ServiceVisitor.Where((AnimatorState s) => s.name.IndexOf("(wd on)", StringComparison.OrdinalIgnoreCase) < 0 && s.name.IndexOf("(wd off)", StringComparison.OrdinalIgnoreCase) < 0).ToArray();
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
		if (parameterNames == null)
		{
			ConnectAnnotation();
			if (parameterNames == null)
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
						using (new MixedValueScope(field))
						{
							EditorGUI.BeginChangeCheck();
							stringValue = (string)EditorUtils.styles().TextFieldDropDown().Invoke(null, parameters);
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
			using (new MixedValueScope(def2))
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
		if (!matchWholeWord || !(task == actionSourceName))
		{
			if (!matchWholeWord)
			{
				return task.Contains(actionSourceName);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	internal static bool UpdateMapper(string task)
	{
		if (!matchWholeWord || !(task == actionSourceName))
		{
			if (!matchWholeWord)
			{
				return task.Contains(actionSourceName);
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
}
