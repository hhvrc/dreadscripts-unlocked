using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ADOverhaul;

internal sealed class ReflectionRestoreScope : IDisposable
{
	internal readonly ReflectionAccessor m_MockDic;

	internal readonly Dictionary<string, object> instanceDic;

	internal readonly bool listenerDic;

	internal bool m_ObserverDic = true;

	public ReflectionRestoreScope(object task, params string[] valuesToRestore)
		: this(task, removecounter: true, valuesToRestore)
	{
	}

	public ReflectionRestoreScope(object param, bool removecounter, params string[] valuesToRestore)
	{
		ReflectionRestoreScope _MessageDic = this;
		listenerDic = removecounter;
		m_MockDic = new ReflectionAccessor(param);
		instanceDic = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object token;
			if (removecounter)
			{
				token = _MessageDic.m_MockDic.CalculateParser(s);
			}
			else
			{
				_MessageDic.m_MockDic.CompareParser(s, out token);
			}
			if (token == null)
			{
				return (object)null;
			}
			Type type = token.GetType();
			if (!type.IsGenericType)
			{
				return token;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(Array))
			{
				Type type2 = type.GetGenericArguments().Single();
				IEnumerable<object> enumerable = ((IEnumerable)token).Cast<object>();
				return genericTypeDefinition.MakeGenericType(type2).GetConstructor(new Type[1] { typeof(IEnumerable<>).MakeGenericType(type2) }).Invoke(new object[1] { enumerable });
			}
			return token;
		});
	}

	public void Dispose()
	{
		if (!m_ObserverDic)
		{
			return;
		}
		if (listenerDic)
		{
			foreach (KeyValuePair<string, object> item in instanceDic)
			{
				m_MockDic.PopParser(item.Key, item.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> item2 in instanceDic)
		{
			m_MockDic.InterruptParser(item2.Key, item2.Value);
		}
	}
}
