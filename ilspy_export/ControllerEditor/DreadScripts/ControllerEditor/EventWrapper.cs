using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal struct EventWrapper
{
	internal readonly Event m_RegistryPolicy;

	internal bool m_TagPolicy;

	internal static object NewDecorator;

	internal void DefineHelper()
	{
		m_RegistryPolicy.Use();
	}

	internal EventWrapper(Event init)
	{
		m_RegistryPolicy = init;
		m_TagPolicy = true;
	}

	public static implicit operator Event(EventWrapper info)
	{
		return info.m_RegistryPolicy;
	}

	public static implicit operator bool(EventWrapper i)
	{
		return i.m_TagPolicy;
	}

	internal static bool LoginDecorator()
	{
		return NewDecorator == null;
	}
}
