using System;
using System.Reflection;

internal class DelegateBinder
{
	internal delegate void ProxyDelegate(object o);

	internal static Module m_AuthenticationPolicy = typeof(DelegateBinder).Assembly.ManifestModule;

	internal static DelegateBinder DefineClient;

	internal static void FillDescriptor(int typemdt)
	{
		Type type = m_AuthenticationPolicy.ResolveType(33554432 + typemdt);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			MethodInfo method = (MethodInfo)m_AuthenticationPolicy.ResolveMethod(fieldInfo.MetadataToken + 100663296);
			fieldInfo.SetValue(null, (MulticastDelegate)Delegate.CreateDelegate(type, method));
		}
	}

	internal static bool EnableClient()
	{
		return DefineClient == null;
	}
}
