using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ADOverhaul;

internal sealed class ReflectionRestoreScope : IDisposable
{
	internal readonly ReflectionAccessor accessor;

	internal readonly Dictionary<string, object> savedValues;

	internal readonly bool logMissingMembers;

	internal bool restoreOnDispose = true;

	public ReflectionRestoreScope(object task, params string[] valuesToRestore)
		: this(task, removecounter: true, valuesToRestore)
	{
	}

	public ReflectionRestoreScope(object param, bool removecounter, params string[] valuesToRestore)
	{
		ReflectionRestoreScope _MessageDic = this;
		logMissingMembers = removecounter;
		accessor = new ReflectionAccessor(param);
		savedValues = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object token;
			if (removecounter)
			{
				token = _MessageDic.accessor.Item(s);
			}
			else
			{
				_MessageDic.accessor.TryGetValue(s, out token);
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
		if (!restoreOnDispose)
		{
			return;
		}
		if (logMissingMembers)
		{
			foreach (KeyValuePair<string, object> savedValue in savedValues)
			{
				accessor.Item(savedValue.Key, savedValue.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> savedValue2 in savedValues)
		{
			accessor.TrySetValue(savedValue2.Key, savedValue2.Value);
		}
	}
}
