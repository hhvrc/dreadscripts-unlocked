using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class EditorLayoutUtils
{
	private static readonly Color reg = new Color(0.33f, 0.33f, 0.33f);

	public static Type processor;

	private static Type _Observer;

	private static ConstructorInfo _Server;

	private static MethodInfo thread;

	private static MethodInfo policy;

	[SpecialName]
	public static Type SplitterGUILayoutType()
	{
		return processor ?? (processor = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type SplitterStateType()
	{
		return _Observer ?? (_Observer = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo SplitterStateConstructor()
	{
		if (_Server == null)
		{
			_Server = SplitterStateType().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return _Server;
	}

	[SpecialName]
	public static MethodInfo BeginSplitMethod()
	{
		if (thread == null)
		{
			thread = SplitterGUILayoutType().GetMethod("BeginSplit", new Type[4]
			{
				SplitterStateType(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return thread;
	}

	[SpecialName]
	public static MethodInfo EndLayoutGroupMethod()
	{
		if (policy == null)
		{
			policy = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return policy;
	}

	public static object CreateSplitterState(params float[] relativeSizes)
	{
		return SplitterStateConstructor().Invoke(new object[1] { relativeSizes });
	}

	public static void BeginHorizontalSplit(object asset, GUIStyle cont = null, params GUILayoutOption[] options)
	{
		BeginSplit(asset, cont, loadconsumer: false, options);
	}

	public static void BeginVerticalSplit(object setup, GUIStyle pol = null, params GUILayoutOption[] options)
	{
		BeginSplit(setup, pol, loadconsumer: true, options);
	}

	public static void BeginSplit(object def, GUIStyle cfg = null, bool loadconsumer = true, params GUILayoutOption[] options)
	{
		BeginSplitMethod().Invoke(null, new object[4]
		{
			def,
			cfg ?? GUIStyle.none,
			loadconsumer,
			options
		});
	}

	public static void EndSplit()
	{
		EndLayoutGroupMethod().Invoke(null, null);
	}

	public static void DrawTitle(string task)
	{
		DrawTitle(new GUIContent(task));
	}

	public static void DrawTitle(GUIContent param)
	{
		EditorGUILayout.LabelField(param, EditorStyles.boldLabel);
		DrawHorizontalLine();
		GUILayout.Space(7f);
	}

	public static void DrawVerticalLine(Rect res = default(Rect), Color reg = default(Color))
	{
		if (reg == default(Color))
		{
			reg = EditorLayoutUtils.reg;
		}
		if (res == default(Rect))
		{
			res = GUILayoutUtility.GetLastRect();
		}
		res.width = 1.5f;
		res.x -= 2f;
		EditorGUI.DrawRect(res, reg);
	}

	public static void DrawHorizontalLine(Color setup = default(Color))
	{
		if (setup == default(Color))
		{
			setup = reg;
		}
		float height = 1.5f;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
		controlRect.height = height;
		controlRect.y += 1f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		EditorGUI.DrawRect(controlRect, setup);
	}

	public static void DrawUnderline(Rect first = default(Rect), Color cust = default(Color), float consumer = 1.5f)
	{
		if (cust == default(Color))
		{
			cust = reg;
		}
		if (first == default(Rect))
		{
			first = GUILayoutUtility.GetLastRect();
		}
		first.y += first.height + consumer;
		first.height = consumer;
		EditorGUI.DrawRect(first, cust);
		GUILayout.Space(consumer * 3f);
	}
}
