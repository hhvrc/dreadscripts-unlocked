using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class AnnotationProperty
{
	private static bool _VisitorProperty;

	private static Type m_AlgoProperty;

	private static MethodInfo m_MapperProperty;

	private static MethodInfo initializerProperty;

	private static MethodInfo definitionProperty;

	private static MethodInfo m_RegProperty;

	private static MethodInfo _TestsProperty;

	private static MethodInfo _PropertyProperty;

	private static FieldInfo _ProcessorProperty;

	private static FieldInfo _ObserverProperty;

	private static FieldInfo _ServerProperty;

	private static FieldInfo _ThreadProperty;

	private static FieldInfo _PolicyProperty;

	private static FieldInfo m_SerializerProperty;

	private object pageProperty;

	private bool m_ResolverProperty;

	private readonly Func<object> m_PredicateProperty;

	internal Action<bool> _RulesProperty;

	private static AnnotationProperty MapStruct;

	private static void FlushSerializer()
	{
		if (_VisitorProperty)
		{
			return;
		}
		_VisitorProperty = true;
		try
		{
			m_AlgoProperty = ClassProperty.FillRules("UnityEditor.RenameOverlay, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			m_MapperProperty = m_AlgoProperty.GetMethod("BeginRename");
			initializerProperty = m_AlgoProperty.GetMethod("EndRename");
			definitionProperty = m_AlgoProperty.GetMethod("IsRenaming");
			m_RegProperty = m_AlgoProperty.GetMethod("OnGUI", new Type[1] { typeof(GUIStyle) });
			_TestsProperty = m_AlgoProperty.GetMethod("OnEvent");
			_PropertyProperty = m_AlgoProperty.GetMethod("Clear");
			_ProcessorProperty = m_AlgoProperty.RestartList("m_EditFieldRect");
			_ObserverProperty = m_AlgoProperty.RestartList("m_UserAcceptedRename");
			_ServerProperty = m_AlgoProperty.RestartList("m_OriginalName");
			_ThreadProperty = m_AlgoProperty.RestartList("m_Name");
			_PolicyProperty = m_AlgoProperty.RestartList("m_UserData");
			m_SerializerProperty = m_AlgoProperty.RestartList("m_IsWaitingForDelay");
		}
		catch (Exception)
		{
			Debug.LogError("Rename Overlay Wrapper has failed to initialize!");
			throw;
		}
	}

	internal AnnotationProperty()
	{
		FlushSerializer();
		DestroySerializer(Activator.CreateInstance(m_AlgoProperty));
	}

	internal AnnotationProperty(object value)
	{
		while (true)
		{
			FlushSerializer();
		}
	}

	internal AnnotationProperty(Func<object> i)
	{
		m_PredicateProperty = i;
	}

	internal object ConnectSerializer()
	{
		FlushSerializer();
		pageProperty = m_PredicateProperty?.Invoke();
		m_ResolverProperty = true;
		return pageProperty;
	}

	[SpecialName]
	internal object RateSerializer()
	{
		if (pageProperty != null || m_ResolverProperty)
		{
			return pageProperty;
		}
		FlushSerializer();
		ConnectSerializer();
		return pageProperty;
	}

	[SpecialName]
	internal void DestroySerializer(object reference)
	{
		pageProperty = reference;
	}

	[SpecialName]
	internal Rect CalcSerializer()
	{
		return (Rect)_ProcessorProperty.GetValue(RateSerializer());
	}

	[SpecialName]
	internal void IncludeSerializer(Rect def)
	{
		_ProcessorProperty.SetValue(RateSerializer(), def);
	}

	[SpecialName]
	internal bool CloneSerializer()
	{
		return (bool)_ObserverProperty.GetValue(RateSerializer());
	}

	[SpecialName]
	internal bool ReflectSerializer()
	{
		object obj = RateSerializer();
		return (bool)definitionProperty.Invoke(obj, null);
	}

	[SpecialName]
	internal int CreateSerializer()
	{
		return (int)_PolicyProperty.GetValue(RateSerializer());
	}

	[SpecialName]
	internal void NewSerializer(int key_Position)
	{
		_PolicyProperty.SetValue(RateSerializer(), key_Position);
	}

	[SpecialName]
	internal bool ViewSerializer()
	{
		return (bool)m_SerializerProperty.GetValue(RateSerializer());
	}

	[SpecialName]
	internal void CollectSerializer(bool containslast)
	{
		m_SerializerProperty.SetValue(RateSerializer(), containslast);
	}

	[SpecialName]
	internal string ListSerializer()
	{
		return (string)_ThreadProperty.GetValue(RateSerializer());
	}

	[SpecialName]
	internal void VerifySerializer(string key)
	{
		_ThreadProperty.SetValue(RateSerializer(), key);
	}

	[SpecialName]
	internal string WriteSerializer()
	{
		return (string)_ServerProperty.GetValue(RateSerializer());
	}

	internal bool CalculateSerializer(Rect first, string ivk, int minhelper, float map2)
	{
		bool result = TestSerializer(ivk, minhelper, map2);
		object obj = RateSerializer();
		_ProcessorProperty.SetValue(obj, first);
		return result;
	}

	internal bool TestSerializer(string task, int indexvisitor, float helper)
	{
		object obj = RateSerializer();
		return (bool)m_MapperProperty.Invoke(obj, new object[3] { task, indexvisitor, helper });
	}

	internal void MapSerializer(bool isconfig, bool iscfg = true)
	{
		if (ReflectSerializer())
		{
			object obj = RateSerializer();
			initializerProperty.Invoke(obj, new object[1] { isconfig });
			_RulesProperty?.Invoke(isconfig);
			if (iscfg)
			{
				CustomizeSerializer();
			}
		}
	}

	internal bool OnGUI(GUIStyle textFieldStyle = null)
	{
		object obj = RateSerializer();
		return (bool)m_RegProperty.Invoke(obj, new object[1] { textFieldStyle });
	}

	internal bool ValidateSerializer()
	{
		object obj = RateSerializer();
		return (bool)_TestsProperty.Invoke(obj, null);
	}

	internal void CustomizeSerializer()
	{
		object obj = RateSerializer();
		_PropertyProperty.Invoke(obj, null);
	}

	internal static bool AddStruct()
	{
		return MapStruct == null;
	}
}
