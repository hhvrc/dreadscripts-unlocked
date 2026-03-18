using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ADOverhaul;

internal sealed class AuthenticationSerializerConnector : IDisposable
{
	internal readonly OrderImporterTask m_ComposerMethod;

	internal readonly Dictionary<string, object> annotationMethod;

	internal readonly bool _CodeMethod;

	internal bool callbackMethod = true;

	internal static AuthenticationSerializerConnector VerifyState;

	public AuthenticationSerializerConnector(object instance, params string[] valuesToRestore)
		: this(instance, wantcol: true, valuesToRestore)
	{
	}

	public AuthenticationSerializerConnector(object i, bool wantcol, params string[] valuesToRestore)
	{
		AuthenticationSerializerConnector mappingMethod = this;
		_CodeMethod = wantcol;
		m_ComposerMethod = new OrderImporterTask(i);
		annotationMethod = valuesToRestore.ToDictionary((string s) => s, delegate(string s)
		{
			object pol;
			if (!wantcol)
			{
				mappingMethod.m_ComposerMethod.SelectPredicate(s, out pol);
			}
			else
			{
				pol = mappingMethod.m_ComposerMethod.ReadPredicate(s);
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
		if (!callbackMethod)
		{
			return;
		}
		if (!_CodeMethod)
		{
			foreach (KeyValuePair<string, object> item in annotationMethod)
			{
				m_ComposerMethod.WritePredicate(item.Key, item.Value);
			}
			return;
		}
		foreach (KeyValuePair<string, object> item2 in annotationMethod)
		{
			m_ComposerMethod.TestPredicate(item2.Key, item2.Value);
		}
	}

	internal static bool PublishState()
	{
		return VerifyState == null;
	}
}
