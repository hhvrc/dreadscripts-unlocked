using System;
using System.Reflection;

internal class ProxyDelegateBinder
{
	internal delegate void ProxyDelegate(object o);

	internal static Module m_ProcessorDic = typeof(ProxyDelegateBinder).Assembly.ManifestModule;

	internal static void PostQueue(int typemdt)
	{
		Type type = m_ProcessorDic.ResolveType(33554432 + typemdt);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			MethodInfo method = (MethodInfo)m_ProcessorDic.ResolveMethod(fieldInfo.MetadataToken + 100663296);
			fieldInfo.SetValue(null, (MulticastDelegate)Delegate.CreateDelegate(type, method));
		}
	}
}
