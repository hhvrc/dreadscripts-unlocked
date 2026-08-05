using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

[DefaultMember("Item")]
internal class ReflectionAccessor
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
	}

	internal static readonly Dictionary<Type, ReflectionCache> caches = new Dictionary<Type, ReflectionCache>();

	internal readonly object target;

	internal readonly Type type;

	internal readonly ReflectionCache reflectionCache;

	internal ReflectionAccessor(object param)
	{
		target = param;
		type = param.GetType();
		if (caches.TryGetValue(type, out reflectionCache))
		{
			return;
		}
		MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Dictionary<string, FieldInfo> fields = members.OfType<FieldInfo>().ToDictionary((FieldInfo f) => f.Name);
		Dictionary<string, PropertyInfo> properties = members.OfType<PropertyInfo>().ToDictionary((PropertyInfo p) => p.Name);
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
		reflectionCache = new ReflectionCache
		{
			members = members,
			fields = fields,
			properties = properties,
			methods = dictionary
		};
		caches.Add(type, reflectionCache);
	}

	[SpecialName]
	public object Item(string key)
	{
		if (!TryGetValue(key, out var token))
		{
			Debug.LogError("Member " + key + " not found in " + type.Name);
			return null;
		}
		return token;
	}

	[SpecialName]
	public void Item(string info, object caller)
	{
		if (!TrySetValue(info, caller))
		{
			Debug.LogError("Member " + info + " not found in " + type.Name);
		}
	}

	public bool TryGetValue(string first, out object token)
	{
		if (reflectionCache.fields.TryGetValue(first, out var value))
		{
			token = value.GetValue(target);
			return true;
		}
		if (reflectionCache.properties.TryGetValue(first, out var value2))
		{
			token = value2.GetValue(target);
			return true;
		}
		if (!reflectionCache.methods.ContainsKey(first))
		{
			token = null;
			return false;
		}
		token = Invoke(first);
		return true;
	}

	public bool TrySetValue(string setup, object result)
	{
		if (!reflectionCache.fields.TryGetValue(setup, out var value))
		{
			if (!reflectionCache.properties.TryGetValue(setup, out var value2))
			{
				return false;
			}
			value2.SetValue(target, result);
			return true;
		}
		value.SetValue(target, result);
		return true;
	}

	internal object Invoke(string task, params object[] args)
	{
		return Invoke(task, null, args);
	}

	internal T Invoke<T>(string key, params object[] args)
	{
		return (T)Invoke(key, typeof(T), args);
	}

	private object Invoke(string spec, Type cont, params object[] args)
	{
		if (!reflectionCache.methods.TryGetValue(spec, out var value))
		{
			Debug.LogError("Method " + spec + " not found in " + type.Name);
		}
		else
		{
			if (value.Count == 1)
			{
				return value[0].Invoke(target, args);
			}
			if (TryMatchByParameterCount(value, args.Length, out var dir))
			{
				return dir[0].Invoke(target, args);
			}
			if (TryMatchByParameterTypes(dir, (from a in args
				where a != null
				select a.GetType()).ToArray(), out var comp))
			{
				return comp[0].Invoke(target, args);
			}
			if (cont != null && TryMatchByReturnType(comp, cont, out var state))
			{
				return state[0].Invoke(target, args);
			}
			Debug.LogError("Multiple methods named " + spec + " found in " + type.Name);
		}
		return null;
	}

	private static bool TryMatchByParameterCount(IEnumerable<MethodInfo> info, int positioncaller, out MethodInfo[] dir)
	{
		dir = null;
		if (info != null)
		{
			dir = info.Where((MethodInfo m) => m.GetParameters().Length == positioncaller).ToArray();
			return dir.Length == 1;
		}
		return false;
	}

	private static bool TryMatchByParameterTypes(IEnumerable<MethodInfo> param, Type[] b, out MethodInfo[] comp)
	{
		comp = null;
		if (param != null)
		{
			comp = param.Where((MethodInfo m) => !b.Except(m.GetParameters().Select(_003C_003Ec.m_PolicyDic.CreateParser)).Any()).ToArray();
			return comp.Length == 1;
		}
		return false;
	}

	private static bool TryMatchByReturnType(IEnumerable<MethodInfo> config, Type result, out MethodInfo[] state)
	{
		state = null;
		if (config != null)
		{
			state = config.Where((MethodInfo m) => m.ReturnType == result).ToArray();
			return state.Length == 1;
		}
		return false;
	}
}
