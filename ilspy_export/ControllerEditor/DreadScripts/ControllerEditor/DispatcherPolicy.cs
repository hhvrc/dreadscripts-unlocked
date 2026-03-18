using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal struct DispatcherPolicy
{
	internal readonly Event m_RegistryPolicy;

	internal bool m_TagPolicy;

	internal static object NewDecorator;

	internal void DefineHelper()
	{
		m_RegistryPolicy.Use();
	}

	internal DispatcherPolicy(Event init)
	{
		m_RegistryPolicy = init;
		m_TagPolicy = true;
	}

	public static implicit operator Event(DispatcherPolicy info)
	{
		return info.m_RegistryPolicy;
	}

	public static implicit operator bool(DispatcherPolicy i)
	{
		return i.m_TagPolicy;
	}

	internal static bool LoginDecorator()
	{
		return NewDecorator == null;
	}
}
