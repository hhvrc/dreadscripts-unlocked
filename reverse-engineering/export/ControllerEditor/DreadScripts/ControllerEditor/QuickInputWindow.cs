using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class QuickInputWindow : DreadScripts.ControllerEditor.UtilityWindowBase<QuickInputWindow>
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

	private bool inRow;

	private object[] values;

	private GUIContent[] labels;

	private FieldType[] fieldTypes;

	internal bool[] rowToggles;

	private Action<object[]> onConfirm;

	private Func<object[], bool[]> validate;

	private readonly Dictionary<int, Type> objectTypes = new Dictionary<int, Type>();

	string DreadScripts.ControllerEditor.UtilityWindowBase<QuickInputWindow>.title => string.Empty;

	internal static QuickInputWindow Create(string info, FieldType[] second, GUIContent[] dic, Action<object[]> map2, Func<object[], bool[]> second3 = null)
	{
		int num = second.Length;
		while (true)
		{
			object[] array = new object[second.Length];
			int num2 = 0;
			while (true)
			{
				if (num2 < num)
				{
					switch (second[num2])
					{
					case FieldType.ToggleGroup:
						array[num2] = false;
						goto IL_001e;
					case FieldType.Integer:
						array[num2] = 0;
						goto IL_001e;
					case FieldType.Toggle:
						array[num2] = false;
						goto IL_001e;
					case FieldType.String:
						array[num2] = "";
						goto IL_001e;
					case FieldType.Float:
						array[num2] = 0;
						goto IL_001e;
					case FieldType.Object:
						array[num2] = null;
						goto IL_001e;
					}
					break;
				}
				QuickInputWindow quickInputWindow = DreadScripts.ControllerEditor.UtilityWindowBase<QuickInputWindow>.Create();
				quickInputWindow.titleContent.text = info;
				quickInputWindow.values = array;
				quickInputWindow.fieldTypes = second;
				quickInputWindow.labels = dic;
				quickInputWindow.onConfirm = map2;
				quickInputWindow.validate = second3;
				return quickInputWindow;
				IL_001e:
				num2++;
			}
		}
	}

	internal void SetValue(int instancelow, object caller)
	{
		values[instancelow] = caller;
	}

	internal void SetObjectType(int instance_Position, Type cfg)
	{
		if (objectTypes.ContainsKey(instance_Position))
		{
			Debug.LogWarning($"{instance_Position} is already set as {cfg.Name}");
		}
		else
		{
			objectTypes.Add(instance_Position, cfg);
		}
	}

	internal Vector2 GetSize()
	{
		return new Vector2(370f, 26 * fieldTypes.Length + 28 + ((!string.IsNullOrEmpty(helpMessage)) ? 38 : 0));
	}

	internal void ShowAt(Vector2 asset)
	{
		ShowAt(asset, GetSize());
	}

	void DreadScripts.ControllerEditor.UtilityWindowBase<QuickInputWindow>.OnCustomGUI()
	{
		if (values != null)
		{
			bool[] array = validate?.Invoke(values);
			canConfirm = array == null || !array.Any((bool b) => b);
			bool flag = rowToggles != null;
			for (int num = 0; num < fieldTypes.Length; num++)
			{
				if (flag && rowToggles[num])
				{
					inRow = !inRow;
					if (!inRow)
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
					switch (fieldTypes[num])
					{
					case FieldType.ToggleGroup:
						EditorGUI.BeginChangeCheck();
						values[num] = EditorGUILayout.Toggle(labels[num], (bool)values[num]);
						if (!EditorGUI.EndChangeCheck())
						{
							break;
						}
						if ((bool)values[num])
						{
							for (int num2 = 0; num2 < fieldTypes.Length; num2++)
							{
								if (fieldTypes[num2] == FieldType.ToggleGroup && num2 != num)
								{
									values[num2] = false;
								}
							}
						}
						else
						{
							values[num] = true;
						}
						break;
					case FieldType.Float:
						values[num] = EditorGUILayout.FloatField(labels[num], (float)values[num]);
						break;
					case FieldType.Toggle:
						values[num] = EditorGUILayout.Toggle(labels[num], (bool)values[num]);
						break;
					case FieldType.Object:
						values[num] = EditorGUILayout.ObjectField(labels[num], (UnityEngine.Object)values[num], (!objectTypes.ContainsKey(num)) ? values[num].GetType() : objectTypes[num], true);
						break;
					case FieldType.Integer:
						values[num] = EditorGUILayout.IntField(labels[num], (int)values[num]);
						break;
					case FieldType.String:
						values[num] = EditorGUILayout.TextField(labels[num], (string)values[num]);
						break;
					}
					if (!canConfirm && array[num])
					{
						GUILayout.Label(new GUIContent(EditorUtils.contents().warning), EditorUtils.styles().centeredIcon, GUILayout.ExpandWidth(expand: false));
					}
				}
			}
			if (inRow)
			{
				inRow = false;
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
		onConfirm(values);
	}
}
