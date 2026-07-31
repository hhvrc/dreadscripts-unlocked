using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class EventExtensions
{
	internal static EventWrapper QueryHelper(this EventWrapper item, Rect connection = default(Rect))
	{
		if (!item.isValid)
		{
			return item;
		}
		if (connection == default(Rect))
		{
			connection = GUILayoutUtility.GetLastRect();
		}
		item.isValid = connection.Contains(item.currentEvent.mousePosition);
		return item;
	}

	internal static EventWrapper AddHelper(this EventWrapper key)
	{
		if (!key.isValid)
		{
			return key;
		}
		key.isValid = key.currentEvent.type == EventType.ContextClick;
		return key;
	}

	internal static EventWrapper InvokeHelper(this EventWrapper config)
	{
		if (!config.isValid)
		{
			return config;
		}
		config.isValid = config.currentEvent.type == EventType.MouseDown;
		return config;
	}

	internal static EventWrapper FindHelper(this EventWrapper item)
	{
		if (!item.isValid)
		{
			return item;
		}
		item.isValid = item.currentEvent.type == EventType.MouseUp;
		return item;
	}

	internal static EventWrapper ExcludeHelper(this EventWrapper first)
	{
		if (!first.isValid)
		{
			return first;
		}
		first.isValid = first.currentEvent.button == 0;
		return first;
	}

	internal static EventWrapper InitHelper(this EventWrapper res)
	{
		if (!res.isValid)
		{
			return res;
		}
		res.isValid = res.currentEvent.button == 1;
		return res;
	}

	internal static EventWrapper VisitHelper(this EventWrapper def)
	{
		if (def.isValid)
		{
			def.isValid = def.currentEvent.clickCount == 2;
			return def;
		}
		return def;
	}
}
