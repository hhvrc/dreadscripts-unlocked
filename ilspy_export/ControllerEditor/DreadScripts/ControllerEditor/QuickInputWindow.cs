using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class QuickInputWindow : CustomUtilityWindow<QuickInputWindow>
{
	internal enum FieldType
	{
		Object,
		Integer,
		Float,
		String,
		Toggle,
		ToggleGroup
	}

	private bool m_ParamPolicy;

	private object[] _ModelPolicy;

	private GUIContent[] m_TokenizerPolicy;

	private FieldType[] m_DecoratorPolicy;

	internal bool[] comparatorPolicy;

	private Action<object[]> exceptionPolicy;

	private Func<object[], bool[]> m_ObjectPolicy;

	private readonly Dictionary<int, Type> m_UtilsPolicy = new Dictionary<int, Type>();

	private static QuickInputWindow FillClient;

	string CustomUtilityWindow<QuickInputWindow>.title => string.Empty;

	internal static QuickInputWindow CreateHelper(string info, FieldType[] second, GUIContent[] dic, Action<object[]> map2, Func<object[], bool[]> second3 = null)
	{
		int num = second.Length;
		object[] array = new object[second.Length];
		for (int i = 0; i < num; i++)
		{
			switch (second[i])
			{
			case FieldType.ToggleGroup:
				array[i] = false;
				break;
			case FieldType.Integer:
				array[i] = 0;
				break;
			case FieldType.Toggle:
				array[i] = false;
				break;
			case FieldType.String:
				array[i] = "";
				break;
			case FieldType.Float:
				array[i] = 0;
				break;
			case FieldType.Object:
				array[i] = null;
				break;
			}
		}
		QuickInputWindow quickInputWindow = CustomUtilityWindow<QuickInputWindow>.CloneHelper();
		quickInputWindow.titleContent.text = info;
		quickInputWindow._ModelPolicy = array;
		quickInputWindow.m_DecoratorPolicy = second;
		quickInputWindow.m_TokenizerPolicy = dic;
		quickInputWindow.exceptionPolicy = map2;
		quickInputWindow.m_ObjectPolicy = second3;
		return quickInputWindow;
	}

	internal void NewHelper(int instancelow, object caller)
	{
		_ModelPolicy[instancelow] = caller;
	}

	internal void PushHelper(int instance_Position, Type cfg)
	{
		if (m_UtilsPolicy.ContainsKey(instance_Position))
		{
			Debug.LogWarning($"{instance_Position} is already set as {cfg.Name}");
		}
		else
		{
			m_UtilsPolicy.Add(instance_Position, cfg);
		}
	}

	internal Vector2 ViewHelper()
	{
		return new Vector2(370f, 26 * m_DecoratorPolicy.Length + 28 + ((!string.IsNullOrEmpty(invocationPolicy)) ? 38 : 0));
	}

	internal void CollectHelper(Vector2 asset)
	{
		LoginHelper(asset, ViewHelper());
	}

	void CustomUtilityWindow<QuickInputWindow>.OnCustomGUI()
	{
		if (_ModelPolicy != null)
		{
			bool[] array = m_ObjectPolicy?.Invoke(_ModelPolicy);
			m_DicPolicy = array == null || !array.Any((bool b) => b);
			bool flag = comparatorPolicy != null;
			for (int num = 0; num < m_DecoratorPolicy.Length; num++)
			{
				if (flag && comparatorPolicy[num])
				{
					m_ParamPolicy = !m_ParamPolicy;
					if (!m_ParamPolicy)
					{
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						EditorGUILayout.BeginHorizontal();
					}
				}
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					switch (m_DecoratorPolicy[num])
					{
					case FieldType.ToggleGroup:
						EditorGUI.BeginChangeCheck();
						_ModelPolicy[num] = EditorGUILayout.Toggle(m_TokenizerPolicy[num], (bool)_ModelPolicy[num]);
						if (!EditorGUI.EndChangeCheck())
						{
							break;
						}
						if ((bool)_ModelPolicy[num])
						{
							for (int num2 = 0; num2 < m_DecoratorPolicy.Length; num2++)
							{
								if (m_DecoratorPolicy[num2] == FieldType.ToggleGroup && num2 != num)
								{
									_ModelPolicy[num2] = false;
								}
							}
						}
						else
						{
							_ModelPolicy[num] = true;
						}
						break;
					case FieldType.Float:
						_ModelPolicy[num] = EditorGUILayout.FloatField(m_TokenizerPolicy[num], (float)_ModelPolicy[num]);
						break;
					case FieldType.Toggle:
						_ModelPolicy[num] = EditorGUILayout.Toggle(m_TokenizerPolicy[num], (bool)_ModelPolicy[num]);
						break;
					case FieldType.Object:
						_ModelPolicy[num] = EditorGUILayout.ObjectField(m_TokenizerPolicy[num], (UnityEngine.Object)_ModelPolicy[num], (!m_UtilsPolicy.ContainsKey(num)) ? _ModelPolicy[num].GetType() : m_UtilsPolicy[num], true);
						break;
					case FieldType.Integer:
						_ModelPolicy[num] = EditorGUILayout.IntField(m_TokenizerPolicy[num], (int)_ModelPolicy[num]);
						break;
					case FieldType.String:
						_ModelPolicy[num] = EditorGUILayout.TextField(m_TokenizerPolicy[num], (string)_ModelPolicy[num]);
						break;
					}
					if (!m_DicPolicy && array[num])
					{
						GUILayout.Label(new GUIContent(ClassProperty.DestroyError().issuerProcessor), ClassProperty.CalcError().m_InstanceProcessor, GUILayout.ExpandWidth(expand: false));
					}
				}
			}
			if (m_ParamPolicy)
			{
				m_ParamPolicy = false;
				EditorGUILayout.EndHorizontal();
			}
		}
		else
		{
			Close();
		}
	}

	internal override void OnCustomConfirm()
	{
		exceptionPolicy(_ModelPolicy);
	}

	internal static bool DeleteClient()
	{
		return (object)FillClient == null;
	}
}
