using System;
using System.Reflection;

internal class DelegateBinder
{
	internal delegate void ProxyDelegate(object o);

	internal static Module manifestModule = typeof(DelegateBinder).Assembly.ManifestModule;

	internal static void FillDescriptor(int typemdt)
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
