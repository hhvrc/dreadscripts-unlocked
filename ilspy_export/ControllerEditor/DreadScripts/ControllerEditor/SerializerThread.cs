using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[DefaultMember("Item")]
internal class SerializerThread
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _QueueThread = new _003C_003Ec();

		public static Func<FieldInfo, string> m_ErrorThread;

		public static Func<PropertyInfo, string> m_SetterThread;

		public static Func<object, bool> connectionThread;

		public static Func<object, Type> _ContextThread;

		public static Func<ParameterInfo, Type> m_RecordThread;

		private static _003C_003Ec RunStatus;

		internal string FillContext(FieldInfo f)
		{
			return f.Name;
		}

		internal string WriteContext(PropertyInfo p)
		{
			return p.Name;
		}

		internal bool ForgotContext(object a)
		{
			return a != null;
		}

		internal Type StopContext(object a)
		{
			return a.GetType();
		}

		internal Type CheckContext(ParameterInfo p)
		{
			return p.ParameterType;
		}

		internal static bool ComputeStatus()
		{
			return RunStatus == null;
		}
	}

	internal static readonly Dictionary<Type, InterpreterThread> pageThread = new Dictionary<Type, InterpreterThread>();

	internal readonly object m_ResolverThread;

	internal readonly Type _PredicateThread;

	internal readonly InterpreterThread m_RulesThread;

	private static SerializerThread FillStatus;

	internal SerializerThread(object key)
	{
		m_ResolverThread = key;
		_PredicateThread = key.GetType();
		if (pageThread.TryGetValue(_PredicateThread, out m_RulesThread))
		{
			return;
		}
		MemberInfo[] members = _PredicateThread.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Dictionary<string, FieldInfo> candidateThread = members.OfType<FieldInfo>().ToDictionary((FieldInfo f) => f.Name);
		Dictionary<string, PropertyInfo> productThread = members.OfType<PropertyInfo>().ToDictionary((PropertyInfo p) => p.Name);
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
		m_RulesThread = new InterpreterThread
		{
			m_WatcherThread = members,
			candidateThread = candidateThread,
			productThread = productThread,
			m_ExpressionThread = dictionary
		};
		pageThread.Add(_PredicateThread, m_RulesThread);
	}

	[SpecialName]
	public object ResolveContext(string init)
	{
		if (LoginContext(init, out var attr))
		{
			return attr;
		}
		Debug.LogError("Member " + init + " not found in " + _PredicateThread.Name);
		return null;
	}

	[SpecialName]
	public void ListContext(string ident, object cfg)
	{
		if (!ReflectContext(ident, cfg))
		{
			Debug.LogError("Member " + ident + " not found in " + _PredicateThread.Name);
		}
	}

	public bool LoginContext(string v, out object attr)
	{
		if (!m_RulesThread.candidateThread.TryGetValue(v, out var value))
		{
			if (!m_RulesThread.productThread.TryGetValue(v, out var value2))
			{
				if (!m_RulesThread.m_ExpressionThread.ContainsKey(v))
				{
					attr = null;
					return false;
				}
				attr = DeleteContext(v);
				return true;
			}
			attr = value2.GetValue(m_ResolverThread);
			return true;
		}
		attr = value.GetValue(m_ResolverThread);
		return true;
	}

	public bool ReflectContext(string first, object cust)
	{
		if (m_RulesThread.candidateThread.TryGetValue(first, out var value))
		{
			value.SetValue(m_ResolverThread, cust);
			return true;
		}
		if (!m_RulesThread.productThread.TryGetValue(first, out var value2))
		{
			return false;
		}
		value2.SetValue(m_ResolverThread, cust);
		return true;
	}

	internal object DeleteContext(string ident, params object[] args)
	{
		return NewContext(ident, null, args);
	}

	internal T CreateContext<T>(string def, params object[] args)
	{
		return (T)NewContext(def, typeof(T), args);
	}

	private object NewContext(string last, Type pol, params object[] args)
	{
		if (!m_RulesThread.m_ExpressionThread.TryGetValue(last, out var value))
		{
			Debug.LogError("Method " + last + " not found in " + _PredicateThread.Name);
		}
		else
		{
			if (value.Count == 1)
			{
				return value[0].Invoke(m_ResolverThread, args);
			}
			if (PushContext(value, args.Length, out var c))
			{
				return c[0].Invoke(m_ResolverThread, args);
			}
			if (ViewContext(c, (from a in args
				where a != null
				select a.GetType()).ToArray(), out var control))
			{
				return control[0].Invoke(m_ResolverThread, args);
			}
			if (pol != null && CollectContext(control, pol, out var c2))
			{
				return c2[0].Invoke(m_ResolverThread, args);
			}
			Debug.LogError("Multiple methods named " + last + " found in " + _PredicateThread.Name);
		}
		return null;
	}

	private static bool PushContext(IEnumerable<MethodInfo> info, int cust_counter, out MethodInfo[] c)
	{
		c = null;
		if (info != null)
		{
			c = info.Where((MethodInfo m) => m.GetParameters().Length == cust_counter).ToArray();
			return c.Length == 1;
		}
		return false;
	}

	private static bool ViewContext(IEnumerable<MethodInfo> res, Type[] pol, out MethodInfo[] control)
	{
		control = null;
		if (res == null)
		{
			return false;
		}
		control = res.Where((MethodInfo m) => !pol.Except(m.GetParameters().Select(_003C_003Ec._QueueThread.CheckContext)).Any()).ToArray();
		return control.Length == 1;
	}

	private static bool CollectContext(IEnumerable<MethodInfo> asset, Type cont, out MethodInfo[] c)
	{
		c = null;
		if (asset == null)
		{
			return false;
		}
		c = asset.Where((MethodInfo m) => m.ReturnType == cont).ToArray();
		return c.Length == 1;
	}

	internal static bool DeleteStatus()
	{
		return FillStatus == null;
	}
}
