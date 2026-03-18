using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

[DefaultMember("Item")]
internal class DispatcherProducerList
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_PolicyDic = new _003C_003Ec();

		public static Func<FieldInfo, string> _DispatcherDic;

		public static Func<PropertyInfo, string> collectionDic;

		public static Func<object, bool> m_ReaderDic;

		public static Func<object, Type> m_PoolDic;

		public static Func<ParameterInfo, Type> _WriterDic;

		internal static _003C_003Ec PushDatabase;

		internal string PostParser(FieldInfo f)
		{
			return f.Name;
		}

		internal string LoginParser(PropertyInfo p)
		{
			return p.Name;
		}

		internal bool RemoveParser(object a)
		{
			return a != null;
		}

		internal Type DestroyParser(object a)
		{
			return a.GetType();
		}

		internal Type CreateParser(ParameterInfo p)
		{
			return p.ParameterType;
		}

		internal static bool InvokeDatabase()
		{
			return PushDatabase == null;
		}
	}

	internal static readonly Dictionary<Type, VisitorTokenizerResolver> _BridgeDic = new Dictionary<Type, VisitorTokenizerResolver>();

	internal readonly object utilsDic;

	internal readonly Type _IdentifierDic;

	internal readonly VisitorTokenizerResolver globalDic;

	internal static DispatcherProducerList SetupDatabase;

	internal DispatcherProducerList(object param)
	{
		utilsDic = param;
		_IdentifierDic = param.GetType();
		if (_BridgeDic.TryGetValue(_IdentifierDic, out globalDic))
		{
			return;
		}
		MemberInfo[] members = _IdentifierDic.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Dictionary<string, FieldInfo> contextDic = members.OfType<FieldInfo>().ToDictionary((FieldInfo f) => f.Name);
		Dictionary<string, PropertyInfo> schemaDic = members.OfType<PropertyInfo>().ToDictionary((PropertyInfo p) => p.Name);
		Dictionary<string, List<MethodInfo>> dictionary = new Dictionary<string, List<MethodInfo>>();
		foreach (MethodInfo item in members.OfType<MethodInfo>())
		{
			if (!dictionary.TryGetValue(item.Name, out var value))
			{
				value = new List<MethodInfo>();
				dictionary.Add(item.Name, value);
			}
			value.Add(item);
		}
		globalDic = new VisitorTokenizerResolver
		{
			_WatcherDic = members,
			_ContextDic = contextDic,
			m_SchemaDic = schemaDic,
			m_ReponseDic = dictionary
		};
		_BridgeDic.Add(_IdentifierDic, globalDic);
	}

	[SpecialName]
	public object CalculateParser(string key)
	{
		if (!CompareParser(key, out var token))
		{
			Debug.LogError("Member " + key + " not found in " + _IdentifierDic.Name);
			return null;
		}
		return token;
	}

	[SpecialName]
	public void PopParser(string info, object caller)
	{
		if (!InterruptParser(info, caller))
		{
			Debug.LogError("Member " + info + " not found in " + _IdentifierDic.Name);
		}
	}

	public bool CompareParser(string first, out object token)
	{
		if (globalDic._ContextDic.TryGetValue(first, out var value))
		{
			token = value.GetValue(utilsDic);
			return true;
		}
		if (globalDic.m_SchemaDic.TryGetValue(first, out var value2))
		{
			token = value2.GetValue(utilsDic);
			return true;
		}
		if (!globalDic.m_ReponseDic.ContainsKey(first))
		{
			token = null;
			return false;
		}
		token = ComputeParser(first);
		return true;
	}

	public bool InterruptParser(string setup, object result)
	{
		if (!globalDic._ContextDic.TryGetValue(setup, out var value))
		{
			if (!globalDic.m_SchemaDic.TryGetValue(setup, out var value2))
			{
				return false;
			}
			value2.SetValue(utilsDic, result);
			return true;
		}
		value.SetValue(utilsDic, result);
		return true;
	}

	internal object ComputeParser(string task, params object[] args)
	{
		return InitParser(task, null, args);
	}

	internal T StartParser<T>(string key, params object[] args)
	{
		return (T)InitParser(key, typeof(T), args);
	}

	private object InitParser(string spec, Type cont, params object[] args)
	{
		if (!globalDic.m_ReponseDic.TryGetValue(spec, out var value))
		{
			Debug.LogError("Method " + spec + " not found in " + _IdentifierDic.Name);
		}
		else
		{
			if (value.Count == 1)
			{
				return value[0].Invoke(utilsDic, args);
			}
			if (CheckParser(value, args.Length, out var dir))
			{
				return dir[0].Invoke(utilsDic, args);
			}
			if (CancelParser(dir, (from a in args
				where a != null
				select a.GetType()).ToArray(), out var comp))
			{
				return comp[0].Invoke(utilsDic, args);
			}
			if (cont != null && DisableParser(comp, cont, out var state))
			{
				return state[0].Invoke(utilsDic, args);
			}
			Debug.LogError("Multiple methods named " + spec + " found in " + _IdentifierDic.Name);
		}
		return null;
	}

	private static bool CheckParser(IEnumerable<MethodInfo> info, int positioncaller, out MethodInfo[] dir)
	{
		dir = null;
		if (info != null)
		{
			dir = info.Where((MethodInfo m) => m.GetParameters().Length == positioncaller).ToArray();
			return dir.Length == 1;
		}
		return false;
	}

	private static bool CancelParser(IEnumerable<MethodInfo> param, Type[] b, out MethodInfo[] comp)
	{
		comp = null;
		if (param != null)
		{
			comp = param.Where((MethodInfo m) => !b.Except(m.GetParameters().Select(_003C_003Ec.m_PolicyDic.CreateParser)).Any()).ToArray();
			return comp.Length == 1;
		}
		return false;
	}

	private static bool DisableParser(IEnumerable<MethodInfo> config, Type result, out MethodInfo[] state)
	{
		state = null;
		if (config != null)
		{
			state = config.Where((MethodInfo m) => m.ReturnType == result).ToArray();
			return state.Length == 1;
		}
		return false;
	}

	internal static bool QueryDatabase()
	{
		return SetupDatabase == null;
	}
}
