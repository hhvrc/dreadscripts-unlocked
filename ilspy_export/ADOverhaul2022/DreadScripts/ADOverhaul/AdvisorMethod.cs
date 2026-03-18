using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal static class AdvisorMethod
{
	private static readonly Color exporterMethod = new Color(0.33f, 0.33f, 0.33f);

	public static Type creatorMethod;

	private static Type m_DispatcherMethod;

	private static ConstructorInfo connectionMethod;

	private static MethodInfo m_ExpressionMethod;

	private static MethodInfo _DecoratorMethod;

	internal static AdvisorMethod ValidateState;

	[SpecialName]
	public static Type RemoveIterator()
	{
		return creatorMethod ?? (creatorMethod = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type ResolveIterator()
	{
		return m_DispatcherMethod ?? (m_DispatcherMethod = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo GetIterator()
	{
		if (connectionMethod == null)
		{
			connectionMethod = ResolveIterator().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return connectionMethod;
	}

	[SpecialName]
	public static MethodInfo ExcludeIterator()
	{
		if (m_ExpressionMethod == null)
		{
			m_ExpressionMethod = RemoveIterator().GetMethod("BeginSplit", new Type[4]
			{
				ResolveIterator(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return m_ExpressionMethod;
	}

	[SpecialName]
	public static MethodInfo ConnectIterator()
	{
		if (_DecoratorMethod == null)
		{
			_DecoratorMethod = ((AdvisorMethod)(object)typeof(GUILayoutUtility)).VisitIterator("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return _DecoratorMethod;
	}

	public static object SearchIterator(params float[] relativeSizes)
	{
		return GetIterator().Invoke(new object[1] { relativeSizes });
	}

	public static void LoginIterator(object var1, GUIStyle counter = null, params GUILayoutOption[] options)
	{
		CheckIterator(var1, counter, iscomp: false, options);
	}

	public static void PatchIterator(object reference, GUIStyle pred = null, params GUILayoutOption[] options)
	{
		CheckIterator(reference, pred, iscomp: true, options);
	}

	public static void CheckIterator(object var1, GUIStyle cfg = null, bool iscomp = true, params GUILayoutOption[] options)
	{
		ExcludeIterator().Invoke(null, new object[4]
		{
			var1,
			cfg ?? GUIStyle.none,
			iscomp,
			options
		});
	}

	public static void CallIterator()
	{
		ConnectIterator().Invoke(null, null);
	}

	public static void RegisterIterator(string task)
	{
		ChangeIterator(new GUIContent(task));
	}

	public static void ChangeIterator(GUIContent v)
	{
		EditorGUILayout.LabelField(v, EditorStyles.boldLabel);
		PushIterator();
		GUILayout.Space(7f);
	}

	public static void StopIterator(Rect param = default(Rect), Color token = default(Color))
	{
		if (token == default(Color))
		{
			token = exporterMethod;
		}
		if (param == default(Rect))
		{
			param = GUILayoutUtility.GetLastRect();
		}
		param.width = 1.5f;
		param.x -= 2f;
		EditorGUI.DrawRect(param, token);
	}

	public static void PushIterator(Color info = default(Color))
	{
		if (info == default(Color))
		{
			info = exporterMethod;
		}
		float height = 1.5f;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
		controlRect.height = height;
		controlRect.y += 1f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		EditorGUI.DrawRect(controlRect, info);
	}

	public static void PrepareIterator(Rect reference = default(Rect), Color pol = default(Color), float dic = 1.5f)
	{
		if (pol == default(Color))
		{
			pol = exporterMethod;
		}
		if (reference == default(Rect))
		{
			reference = GUILayoutUtility.GetLastRect();
		}
		reference.y += reference.height + dic;
		reference.height = dic;
		EditorGUI.DrawRect(reference, pol);
		GUILayout.Space(dic * 3f);
	}

	MethodInfo VisitIterator(string def, BindingFlags pol)
	{
		return ((Type)this).GetMethod(def, pol);
	}

	internal static bool EnableState()
	{
		return ValidateState == null;
	}
}
