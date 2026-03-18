using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class AttrPolicy
{
	private static AttrPolicy MapDecorator;

	internal static DispatcherPolicy QueryHelper(this DispatcherPolicy item, Rect connection = default(Rect))
	{
		if (!item.m_TagPolicy)
		{
			return item;
		}
		if (connection == default(Rect))
		{
			connection = GUILayoutUtility.GetLastRect();
		}
		item.m_TagPolicy = connection.Contains(item.m_RegistryPolicy.mousePosition);
		return item;
	}

	internal static DispatcherPolicy AddHelper(this DispatcherPolicy key)
	{
		if (!key.m_TagPolicy)
		{
			return key;
		}
		key.m_TagPolicy = key.m_RegistryPolicy.type == EventType.ContextClick;
		return key;
	}

	internal static DispatcherPolicy InvokeHelper(this DispatcherPolicy config)
	{
		if (!config.m_TagPolicy)
		{
			return config;
		}
		config.m_TagPolicy = config.m_RegistryPolicy.type == EventType.MouseDown;
		return config;
	}

	internal static DispatcherPolicy FindHelper(this DispatcherPolicy item)
	{
		if (!item.m_TagPolicy)
		{
			return item;
		}
		item.m_TagPolicy = item.m_RegistryPolicy.type == EventType.MouseUp;
		return item;
	}

	internal static DispatcherPolicy ExcludeHelper(this DispatcherPolicy first)
	{
		if (!first.m_TagPolicy)
		{
			return first;
		}
		first.m_TagPolicy = first.m_RegistryPolicy.button == 0;
		return first;
	}

	internal static DispatcherPolicy InitHelper(this DispatcherPolicy res)
	{
		if (!res.m_TagPolicy)
		{
			return res;
		}
		res.m_TagPolicy = res.m_RegistryPolicy.button == 1;
		return res;
	}

	internal static DispatcherPolicy VisitHelper(this DispatcherPolicy def)
	{
		if (def.m_TagPolicy)
		{
			def.m_TagPolicy = def.m_RegistryPolicy.clickCount == 2;
			return def;
		}
		return def;
	}

	internal static bool AddDecorator()
	{
		return MapDecorator == null;
	}
}
