using System;
using System.Reflection;

internal class ProxyDelegateBinder
{
	internal delegate void ProxyDelegate(object o);

	internal static Module manifestModule = typeof(ProxyDelegateBinder).Assembly.ManifestModule;

	internal static void PostQueue(int typemdt)
	{
		Type type = manifestModule.ResolveType(33554432 + typemdt);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			MethodInfo method = (MethodInfo)manifestModule.ResolveMethod(fieldInfo.MetadataToken + 100663296);
			fieldInfo.SetValue(null, (MulticastDelegate)Delegate.CreateDelegate(type, method));
		}
	}
}
