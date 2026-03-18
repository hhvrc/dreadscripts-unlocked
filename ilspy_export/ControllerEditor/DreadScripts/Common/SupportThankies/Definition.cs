using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class Definition
{
	private static readonly Color reg = new Color(0.33f, 0.33f, 0.33f);

	public static Type processor;

	private static Type _Observer;

	private static ConstructorInfo _Server;

	private static MethodInfo thread;

	private static MethodInfo policy;

	private static Definition PrintCode;

	[SpecialName]
	public static Type CancelWrapper()
	{
		return processor ?? (processor = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type DisableWrapper()
	{
		return _Observer ?? (_Observer = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo RestartWrapper()
	{
		if (_Server == null)
		{
			_Server = DisableWrapper().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return _Server;
	}

	[SpecialName]
	public static MethodInfo AddWrapper()
	{
		if (thread == null)
		{
			thread = CancelWrapper().GetMethod("BeginSplit", new Type[4]
			{
				DisableWrapper(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return thread;
	}

	[SpecialName]
	public static MethodInfo FindWrapper()
	{
		if (policy == null)
		{
			policy = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return policy;
	}

	public static object SetWrapper(params float[] relativeSizes)
	{
		return RestartWrapper().Invoke(new object[1] { relativeSizes });
	}

	public static void PostWrapper(object asset, GUIStyle cont = null, params GUILayoutOption[] options)
	{
		EnableWrapper(asset, cont, loadconsumer: false, options);
	}

	public static void SetupWrapper(object setup, GUIStyle pol = null, params GUILayoutOption[] options)
	{
		EnableWrapper(setup, pol, loadconsumer: true, options);
	}

	public static void EnableWrapper(object def, GUIStyle cfg = null, bool loadconsumer = true, params GUILayoutOption[] options)
	{
		AddWrapper().Invoke(null, new object[4]
		{
			def,
			cfg ?? GUIStyle.none,
			loadconsumer,
			options
		});
	}

	public static void PublishWrapper()
	{
		FindWrapper().Invoke(null, null);
	}

	public static void PopWrapper(string task)
	{
		ComputeWrapper(new GUIContent(task));
	}

	public static void ComputeWrapper(GUIContent param)
	{
		EditorGUILayout.LabelField(param, EditorStyles.boldLabel);
		Color setup;
		uint num = default(uint);
		while (true)
		{
			setup = default(Color);
			switch ((num = (num * 1171511289) ^ 0x704A8B4E ^ 0x1AC13A25) % 5)
			{
			case 0u:
			case 4u:
				continue;
			default:
				return;
			case 3u:
				break;
			case 1u:
				GUILayout.Space(7f);
				break;
			case 2u:
				return;
			}
			break;
		}
		ConcatWrapper(setup);
	}

	public static void MoveWrapper(Rect res = default(Rect), Color reg = default(Color))
	{
		if (reg == default(Color))
		{
			reg = Definition.reg;
		}
		if (res == default(Rect))
		{
			res = GUILayoutUtility.GetLastRect();
		}
		res.width = 1.5f;
		res.x -= 2f;
		EditorGUI.DrawRect(res, reg);
	}

	public static void ConcatWrapper(Color setup = default(Color))
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

	public static void CallWrapper(Rect first = default(Rect), Color cust = default(Color), float consumer = 1.5f)
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

	internal static bool ResolveCode()
	{
		return PrintCode == null;
	}
}
