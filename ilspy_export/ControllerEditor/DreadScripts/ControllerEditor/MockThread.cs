using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class MockThread
{
	private static readonly Color instanceThread = new Color(0.33f, 0.33f, 0.33f);

	public static Type fieldThread;

	private static Type _AttributeThread;

	private static ConstructorInfo _ClientThread;

	private static MethodInfo configThread;

	private static MethodInfo m_DescriptorThread;

	private static MockThread AssetStatus;

	[SpecialName]
	public static Type IncludeRecord()
	{
		return fieldThread ?? (fieldThread = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type CloneRecord()
	{
		return _AttributeThread ?? (_AttributeThread = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo ReflectRecord()
	{
		if (_ClientThread == null)
		{
			_ClientThread = CloneRecord().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return _ClientThread;
	}

	[SpecialName]
	public static MethodInfo CreateRecord()
	{
		if (configThread == null)
		{
			configThread = IncludeRecord().GetMethod("BeginSplit", new Type[4]
			{
				CloneRecord(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return configThread;
	}

	[SpecialName]
	public static MethodInfo PushRecord()
	{
		if (m_DescriptorThread == null)
		{
			m_DescriptorThread = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return m_DescriptorThread;
	}

	public static object ConnectRecord(params float[] relativeSizes)
	{
		return ReflectRecord().Invoke(new object[1] { relativeSizes });
	}

	public static void CalculateRecord(object info, GUIStyle b = null, params GUILayoutOption[] options)
	{
		MapRecord(info, b, isres: false, options);
	}

	public static void TestRecord(object reference, GUIStyle pred = null, params GUILayoutOption[] options)
	{
		MapRecord(reference, pred, isres: true, options);
	}

	public static void MapRecord(object param, GUIStyle connection = null, bool isres = true, params GUILayoutOption[] options)
	{
		CreateRecord().Invoke(null, new object[4]
		{
			param,
			connection ?? GUIStyle.none,
			isres,
			options
		});
	}

	public static void ValidateRecord()
	{
		PushRecord().Invoke(null, null);
	}

	public static void CustomizeRecord(string reference)
	{
		RateRecord(new GUIContent(reference));
	}

	public static void RateRecord(GUIContent param)
	{
		EditorGUILayout.LabelField(param, EditorStyles.boldLabel);
		GetRecord();
		GUILayout.Space(7f);
	}

	public static void DestroyRecord(Rect value = default(Rect), Color visitor = default(Color))
	{
		if (visitor == default(Color))
		{
			visitor = instanceThread;
		}
		if (value == default(Rect))
		{
			value = GUILayoutUtility.GetLastRect();
		}
		value.width = 1.5f;
		value.x -= 2f;
		EditorGUI.DrawRect(value, visitor);
	}

	public static void GetRecord(Color setup = default(Color))
	{
		if (setup == default(Color))
		{
			setup = instanceThread;
		}
		float height = 1.5f;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
		controlRect.height = height;
		controlRect.y += 1f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		EditorGUI.DrawRect(controlRect, setup);
	}

	public static void CalcRecord(Rect def = default(Rect), Color selection = default(Color), float helper = 1.5f)
	{
		if (selection == default(Color))
		{
			selection = instanceThread;
		}
		if (def == default(Rect))
		{
			def = GUILayoutUtility.GetLastRect();
		}
		def.y += def.height + helper;
		def.height = helper;
		EditorGUI.DrawRect(def, selection);
		GUILayout.Space(helper * 3f);
	}

	internal static bool SelectStatus()
	{
		return AssetStatus == null;
	}
}
