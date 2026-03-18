using System;
using System.Reflection;

internal class ModelAccountCollection
{
	internal delegate void ComparatorImporterDescriptor(object o);

	internal static Module configurationConsumer = typeof(ModelAccountCollection).Assembly.ManifestModule;

	internal static ModelAccountCollection ChangeRule;

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
