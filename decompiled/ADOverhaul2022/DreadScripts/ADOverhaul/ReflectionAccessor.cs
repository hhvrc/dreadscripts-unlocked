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
		public static readonly _003C_003Ec valueMethod = new _003C_003Ec();

		public static Func<FieldInfo, string> errorMethod;

		public static Func<PropertyInfo, string> m_ProducerMethod;

		public static Func<object, bool> templateMethod;

		public static Func<object, Type> m_WriterMethod;

		public static Func<ParameterInfo, Type> classMethod;

		internal string EnablePredicate(FieldInfo f)
		{
			return f.Name;
		}

		internal string AwakePredicate(PropertyInfo p)
		{
			return p.Name;
		}

		internal bool DisablePredicate(object a)
		{
			return a != null;
		}

		internal Type VisitPredicate(object a)
		{
			return a.GetType();
		}

		internal Type AssetPredicate(ParameterInfo p)
		{
			return p.ParameterType;
		}
	}

	internal static readonly Dictionary<Type, ReflectionCache> m_QueueMethod = new Dictionary<Type, ReflectionCache>();

	internal readonly object processorMethod;

	internal readonly Type m_TokenizerMethod;

	internal readonly ReflectionCache reflectionCache;

	internal ReflectionAccessor(object task)
	{
		processorMethod = task;
		m_TokenizerMethod = task.GetType();
		if (m_QueueMethod.TryGetValue(m_TokenizerMethod, out reflectionCache))
		{
			return;
		}
		MemberInfo[] members = m_TokenizerMethod.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
		m_QueueMethod.Add(m_TokenizerMethod, reflectionCache);
	}

	[SpecialName]
	public object ReadPredicate(string setup)
	{
		if (SelectPredicate(setup, out var pol))
		{
			return pol;
		}
		Debug.LogError("Member " + setup + " not found in " + m_TokenizerMethod.Name);
		return null;
	}

	[SpecialName]
	public void TestPredicate(string setup, object pol)
	{
		if (!WritePredicate(setup, pol))
		{
			Debug.LogError("Member " + setup + " not found in " + m_TokenizerMethod.Name);
		}
	}

	public bool SelectPredicate(string key, out object pol)
	{
		if (!reflectionCache.fields.TryGetValue(key, out var value))
		{
			if (!reflectionCache.properties.TryGetValue(key, out var value2))
			{
				if (reflectionCache.methods.ContainsKey(key))
				{
					pol = MovePredicate(key);
					return true;
				}
				pol = null;
				return false;
			}
			pol = value2.GetValue(processorMethod);
			return true;
		}
		pol = value.GetValue(processorMethod);
		return true;
	}

	public bool WritePredicate(string param, object second)
	{
		if (!reflectionCache.fields.TryGetValue(param, out var value))
		{
			if (!reflectionCache.properties.TryGetValue(param, out var value2))
			{
				return false;
			}
			value2.SetValue(processorMethod, second);
		}
		value.SetValue(processorMethod, second);
		return true;
	}

	internal object MovePredicate(string setup, params object[] args)
	{
		return CollectPredicate(setup, null, args);
	}

	internal T PublishPredicate<T>(string instance, params object[] args)
	{
		return (T)CollectPredicate(instance, typeof(T), args);
	}

	private object CollectPredicate(string ident, Type second, params object[] args)
	{
		if (reflectionCache.methods.TryGetValue(ident, out var value))
		{
			if (value.Count == 1)
			{
				return value[0].Invoke(processorMethod, args);
			}
			if (PrintPredicate(value, args.Length, out var rule))
			{
				return rule[0].Invoke(processorMethod, args);
			}
			if (InterruptPredicate(rule, (from a in args
				where a != null
				select a.GetType()).ToArray(), out var serv))
			{
				return serv[0].Invoke(processorMethod, args);
			}
			if (second != null && ViewPredicate(serv, second, out var proc))
			{
				return proc[0].Invoke(processorMethod, args);
			}
			Debug.LogError("Multiple methods named " + ident + " found in " + m_TokenizerMethod.Name);
		}
		else
		{
			Debug.LogError("Method " + ident + " not found in " + m_TokenizerMethod.Name);
		}
		return null;
	}

	private static bool PrintPredicate(IEnumerable<MethodInfo> reference, int ID_reg, out MethodInfo[] rule)
	{
		rule = null;
		if (reference == null)
		{
			return false;
		}
		rule = reference.Where((MethodInfo m) => m.GetParameters().Length == ID_reg).ToArray();
		return rule.Length == 1;
	}

	private static bool InterruptPredicate(IEnumerable<MethodInfo> init, Type[] map, out MethodInfo[] serv)
	{
		serv = null;
		if (init != null)
		{
			serv = init.Where((MethodInfo m) => !map.Except(m.GetParameters().Select(_003C_003Ec.valueMethod.AssetPredicate)).Any()).ToArray();
			return serv.Length == 1;
		}
		return false;
	}

	private static bool ViewPredicate(IEnumerable<MethodInfo> ident, Type second, out MethodInfo[] proc)
	{
		proc = null;
		if (ident == null)
		{
			return false;
		}
		proc = ident.Where((MethodInfo m) => m.ReturnType == second).ToArray();
		return proc.Length == 1;
	}
}
