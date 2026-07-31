using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ControllerEditor;

internal sealed class PropertyRestoreScope : IDisposable
{
	internal readonly ObjectReflector reflector;

	internal readonly Dictionary<string, object> savedValues;

	internal readonly bool logMissingMembers;

	internal bool restoreOnDispose = true;

	public PropertyRestoreScope(object config, params string[] valuesToRestore)
		: this(config, iscfg: true, valuesToRestore)
	{
	}

	public PropertyRestoreScope(object info, bool iscfg, params string[] valuesToRestore)
	{
		PropertyRestoreScope _IteratorThread = this;
		logMissingMembers = iscfg;
		reflector = new ObjectReflector(info);
		savedValues = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object attr;
			if (!iscfg)
			{
				_IteratorThread.reflector.LoginContext(s, out attr);
			}
			else
			{
				attr = _IteratorThread.reflector.ResolveContext(s);
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
		if (!restoreOnDispose)
		{
			return;
		}
		if (logMissingMembers)
		{
			foreach (KeyValuePair<string, object> savedValue in savedValues)
			{
				reflector.ListContext(savedValue.Key, savedValue.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> savedValue2 in savedValues)
		{
			reflector.ReflectContext(savedValue2.Key, savedValue2.Value);
		}
	}
}
