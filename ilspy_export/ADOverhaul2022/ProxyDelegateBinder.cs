using System;
using System.Reflection;

internal class ProxyDelegateBinder
{
	internal delegate void ProxyDelegate(object o);

	internal static Module configurationConsumer = typeof(ProxyDelegateBinder).Assembly.ManifestModule;

	internal static ProxyDelegateBinder ChangeRule;

	internal static void LogoutRule(int typemdt)
	{
		Type type = configurationConsumer.ResolveType(33554432 + typemdt);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			MethodInfo method = (MethodInfo)configurationConsumer.ResolveMethod(fieldInfo.MetadataToken + 100663296);
			fieldInfo.SetValue(null, (MulticastDelegate)Delegate.CreateDelegate(type, method));
		}
	}

	internal static bool SetupRule()
	{
		return ChangeRule == null;
	}
}
