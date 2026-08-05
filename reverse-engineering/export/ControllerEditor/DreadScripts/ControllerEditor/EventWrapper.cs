using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal struct EventWrapper
{
	internal readonly Event currentEvent;

	internal bool isValid;

	internal static object NewDecorator;

	internal void Use()
	{
		currentEvent.Use();
	}

	internal EventWrapper(Event init)
	{
		currentEvent = init;
		isValid = true;
	}

	public static implicit operator Event(EventWrapper info)
	{
		return info.currentEvent;
	}

	public static implicit operator bool(EventWrapper i)
	{
		return i.isValid;
	}

	internal static bool LoginDecorator()
	{
		return NewDecorator == null;
	}
}
