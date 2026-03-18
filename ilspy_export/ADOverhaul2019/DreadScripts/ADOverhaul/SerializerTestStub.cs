using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal static class SerializerTestStub
{
	private static readonly Color m_FacadeDic = new Color(0.33f, 0.33f, 0.33f);

	public static Type _ProcessDic;

	private static Type _ConnectionDic;

	private static ConstructorInfo _CustomerDic;

	private static MethodInfo m_QueueDic;

	private static MethodInfo _AnnotationDic;

	internal static SerializerTestStub GetDatabase;

	[SpecialName]
	public static Type CollectMapping()
	{
		return _ProcessDic ?? (_ProcessDic = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static Type RestartMapping()
	{
		return _ConnectionDic ?? (_ConnectionDic = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));
	}

	[SpecialName]
	public static ConstructorInfo SearchMapping()
	{
		if (_CustomerDic == null)
		{
			_CustomerDic = RestartMapping().GetConstructor(new Type[1] { typeof(float[]) });
		}
		return _CustomerDic;
	}

	[SpecialName]
	public static MethodInfo OrderMapping()
	{
		if (m_QueueDic == null)
		{
			m_QueueDic = CollectMapping().GetMethod("BeginSplit", new Type[4]
			{
				RestartMapping(),
				typeof(GUIStyle),
				typeof(bool),
				typeof(GUILayoutOption[])
			});
		}
		return m_QueueDic;
	}

	[SpecialName]
	public static MethodInfo ConcatMapping()
	{
		if (_AnnotationDic == null)
		{
			_AnnotationDic = ((SerializerTestStub)(object)typeof(GUILayoutUtility)).DestroyMapping("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
		}
		return _AnnotationDic;
	}

	public static object TestMapping(params float[] relativeSizes)
	{
		return SearchMapping().Invoke(new object[1] { relativeSizes });
	}

	public static void ResetMapping(object ident, GUIStyle selection = null, params GUILayoutOption[] options)
	{
		VisitMapping(ident, selection, allowtemplate: false, options);
	}

	public static void GetMapping(object spec, GUIStyle pred = null, params GUILayoutOption[] options)
	{
		VisitMapping(spec, pred, allowtemplate: true, options);
	}

	public static void VisitMapping(object var1, GUIStyle cont = null, bool allowtemplate = true, params GUILayoutOption[] options)
	{
		OrderMapping().Invoke(null, new object[4]
		{
			var1,
			cont ?? GUIStyle.none,
			allowtemplate,
			options
		});
	}

	public static void AwakeMapping()
	{
		ConcatMapping().Invoke(null, null);
	}

	public static void InvokeMapping(string var1)
	{
		CustomizeMapping(new GUIContent(var1));
	}

	public static void CustomizeMapping(GUIContent def)
	{
		EditorGUILayout.LabelField(def, EditorStyles.boldLabel);
		FillMapping();
		GUILayout.Space(7f);
	}

	public static void MoveMapping(Rect def = default(Rect), Color cfg = default(Color))
	{
		if (cfg == default(Color))
		{
			cfg = m_FacadeDic;
		}
		if (def == default(Rect))
		{
			def = GUILayoutUtility.GetLastRect();
		}
		def.width = 1.5f;
		def.x -= 2f;
		EditorGUI.DrawRect(def, cfg);
	}

	public static void FillMapping(Color last = default(Color))
	{
		if (last == default(Color))
		{
			last = m_FacadeDic;
		}
		float height = 1.5f;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
		controlRect.height = height;
		controlRect.y += 1f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		EditorGUI.DrawRect(controlRect, last);
	}

	public static void ChangeMapping(Rect init = default(Rect), Color attr = default(Color), float dir = 1.5f)
	{
		if (attr == default(Color))
		{
			attr = m_FacadeDic;
		}
		if (init == default(Rect))
		{
			init = GUILayoutUtility.GetLastRect();
		}
		init.y += init.height + dir;
		init.height = dir;
		EditorGUI.DrawRect(init, attr);
		GUILayout.Space(dir * 3f);
	}

	MethodInfo DestroyMapping(string instance, BindingFlags vis)
	{
		return ((Type)this).GetMethod(instance, vis);
	}

	internal static bool CustomizeDatabase()
	{
		return GetDatabase == null;
	}
}
