using System;
using System.Reflection;

internal class MerchantPolicy
{
	internal delegate void ReponsePolicy(object o);

	internal static Module m_AuthenticationPolicy = typeof(MerchantPolicy).Assembly.ManifestModule;

	internal static MerchantPolicy DefineClient;

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
