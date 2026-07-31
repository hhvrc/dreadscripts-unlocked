using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class SplitterGUIUtils
{
	private static readonly Color defaultLineColor = new Color(0.33f, 0.33f, 0.33f);

	public static Type splitterGUILayoutType;

	private static Type splitterStateType;

	private static ConstructorInfo splitterStateConstructor;

	private static MethodInfo beginSplitMethod;

	private static MethodInfo endLayoutGroupMethod;

	[SpecialName]
	public static Type SplitterGUILayoutType()
	{
		return splitterGUILayoutType ?? (splitterGUILayoutType = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type SplitterStateType()
	{
		return splitterStateType ?? (splitterStateType = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo SplitterStateConstructor()
	{
		if (splitterStateConstructor == null)
		{
			splitterStateConstructor = SplitterStateType().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return splitterStateConstructor;
	}

	[SpecialName]
	public static MethodInfo BeginSplitMethod()
	{
		if (beginSplitMethod == null)
		{
			beginSplitMethod = SplitterGUILayoutType().GetMethod("BeginSplit", new Type[4]
			{
				SplitterStateType(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return beginSplitMethod;
	}

	[SpecialName]
	public static MethodInfo EndLayoutGroupMethod()
	{
		if (endLayoutGroupMethod == null)
		{
			endLayoutGroupMethod = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return endLayoutGroupMethod;
	}

	public static object CreateSplitterState(params float[] relativeSizes)
	{
		return SplitterStateConstructor().Invoke(new object[1] { relativeSizes });
	}

	public static void BeginHorizontalSplit(object info, GUIStyle b = null, params GUILayoutOption[] options)
	{
		BeginSplit(info, b, isres: false, options);
	}

	public static void BeginVerticalSplit(object reference, GUIStyle pred = null, params GUILayoutOption[] options)
	{
		BeginSplit(reference, pred, isres: true, options);
	}

	public static void BeginSplit(object param, GUIStyle connection = null, bool isres = true, params GUILayoutOption[] options)
	{
		BeginSplitMethod().Invoke(null, new object[4]
		{
			param,
			connection ?? GUIStyle.none,
			isres,
			options
		});
	}

	public static void EndSplit()
	{
		EndLayoutGroupMethod().Invoke(null, null);
	}

	public static void DrawTitle(string reference)
	{
		DrawTitle(new GUIContent(reference));
	}

	public static void DrawTitle(GUIContent param)
	{
		EditorGUILayout.LabelField(param, EditorStyles.boldLabel);
		DrawHorizontalLine();
		GUILayout.Space(7f);
	}

	public static void DrawVerticalLine(Rect value = default(Rect), Color visitor = default(Color))
	{
		if (visitor == default(Color))
		{
			visitor = defaultLineColor;
		}
		if (value == default(Rect))
		{
			value = GUILayoutUtility.GetLastRect();
		}
		value.width = 1.5f;
		value.x -= 2f;
		EditorGUI.DrawRect(value, visitor);
	}

	public static void DrawHorizontalLine(Color setup = default(Color))
	{
		if (setup == default(Color))
		{
			setup = defaultLineColor;
		}
		float height = 1.5f;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
		controlRect.height = height;
		controlRect.y += 1f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		EditorGUI.DrawRect(controlRect, setup);
	}

	public static void DrawUnderline(Rect def = default(Rect), Color selection = default(Color), float helper = 1.5f)
	{
		if (selection == default(Color))
		{
			selection = defaultLineColor;
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
}
