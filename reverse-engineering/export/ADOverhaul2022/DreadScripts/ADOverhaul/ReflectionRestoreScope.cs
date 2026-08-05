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

	public ReflectionRestoreScope(object instance, params string[] valuesToRestore)
		: this(instance, wantcol: true, valuesToRestore)
	{
	}

	public ReflectionRestoreScope(object i, bool wantcol, params string[] valuesToRestore)
	{
		ReflectionRestoreScope mappingMethod = this;
		logMissingMembers = wantcol;
		accessor = new ReflectionAccessor(i);
		savedValues = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object pol;
			if (!wantcol)
			{
				mappingMethod.accessor.TryGetValue(s, out pol);
			}
			else
			{
				pol = mappingMethod.accessor.GetValue(s);
			}
			if (pol == null)
			{
				return (object)null;
			}
			Type type = pol.GetType();
			if (!type.IsGenericType)
			{
				return pol;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(Array))
			{
				Type type2 = type.GetGenericArguments().Single();
				IEnumerable<object> enumerable = ((IEnumerable)pol).Cast<object>();
				return genericTypeDefinition.MakeGenericType(type2).GetConstructor(new Type[1] { typeof(IEnumerable<>).MakeGenericType(type2) }).Invoke(new object[1] { enumerable });
			}
			return pol;
		});
	}

	public void Dispose()
	{
		if (!restoreOnDispose)
		{
			return;
		}
		if (!logMissingMembers)
		{
			foreach (KeyValuePair<string, object> savedValue in savedValues)
			{
				accessor.TrySetValue(savedValue.Key, savedValue.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> savedValue2 in savedValues)
		{
			accessor.SetValue(savedValue2.Key, savedValue2.Value);
		}
	}
}
