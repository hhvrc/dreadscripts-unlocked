using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ControllerEditor;

internal sealed class ProxyThread : IDisposable
{
	internal readonly SerializerThread m_StructThread;

	internal readonly Dictionary<string, object> serviceThread;

	internal readonly bool m_StateThread;

	internal bool m_GlobalThread = true;

	private static ProxyThread CustomizeDecorator;

	public ProxyThread(object config, params string[] valuesToRestore)
		: this(config, iscfg: true, valuesToRestore)
	{
	}

	public ProxyThread(object info, bool iscfg, params string[] valuesToRestore)
	{
		ProxyThread _IteratorThread = this;
		m_StateThread = iscfg;
		m_StructThread = new SerializerThread(info);
		serviceThread = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object attr;
			if (!iscfg)
			{
				_IteratorThread.m_StructThread.LoginContext(s, out attr);
			}
			else
			{
				attr = _IteratorThread.m_StructThread.ResolveContext(s);
			}
			if (attr != null)
			{
				Type type = attr.GetType();
				if (!type.IsGenericType)
				{
					return attr;
				}
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(Array))
				{
					Type type2 = type.GetGenericArguments().Single();
					IEnumerable<object> enumerable = ((IEnumerable)attr).Cast<object>();
					return genericTypeDefinition.MakeGenericType(type2).GetConstructor(new Type[1] { typeof(IEnumerable<>).MakeGenericType(type2) }).Invoke(new object[1] { enumerable });
				}
				return attr;
			}
			return (object)null;
		});
	}

	public void Dispose()
	{
		if (!m_GlobalThread)
		{
			return;
		}
		if (m_StateThread)
		{
			foreach (KeyValuePair<string, object> item in serviceThread)
			{
				m_StructThread.ListContext(item.Key, item.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> item2 in serviceThread)
		{
			m_StructThread.ReflectContext(item2.Key, item2.Value);
		}
	}

	internal static bool SearchDecorator()
	{
		return CustomizeDecorator == null;
	}
}
