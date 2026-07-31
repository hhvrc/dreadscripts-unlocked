using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class EventExtensions
{
	internal static EventWrapper QueryHelper(this EventWrapper item, Rect connection = default(Rect))
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

	internal static EventWrapper AddHelper(this EventWrapper key)
	{
		if (!key.m_TagPolicy)
		{
			return key;
		}
		key.m_TagPolicy = key.m_RegistryPolicy.type == EventType.ContextClick;
		return key;
	}

	internal static EventWrapper InvokeHelper(this EventWrapper config)
	{
		if (!config.m_TagPolicy)
		{
			return config;
		}
		config.m_TagPolicy = config.m_RegistryPolicy.type == EventType.MouseDown;
		return config;
	}

	internal static EventWrapper FindHelper(this EventWrapper item)
	{
		if (!item.m_TagPolicy)
		{
			return item;
		}
		item.m_TagPolicy = item.m_RegistryPolicy.type == EventType.MouseUp;
		return item;
	}

	internal static EventWrapper ExcludeHelper(this EventWrapper first)
	{
		if (!first.m_TagPolicy)
		{
			return first;
		}
		first.m_TagPolicy = first.m_RegistryPolicy.button == 0;
		return first;
	}

	internal static EventWrapper InitHelper(this EventWrapper res)
	{
		if (!res.m_TagPolicy)
		{
			return res;
		}
		res.m_TagPolicy = res.m_RegistryPolicy.button == 1;
		return res;
	}

	internal static EventWrapper VisitHelper(this EventWrapper def)
	{
		if (def.m_TagPolicy)
		{
			def.m_TagPolicy = def.m_RegistryPolicy.clickCount == 2;
			return def;
		}
		return def;
	}
}
