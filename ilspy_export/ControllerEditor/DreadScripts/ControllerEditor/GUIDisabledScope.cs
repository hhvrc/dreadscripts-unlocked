using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class GUIDisabledScope : IDisposable
{
	private readonly bool visitorPolicy;

	private static GUIDisabledScope ChangeDecorator;

	public GUIDisabledScope(bool iskey)
	{
		visitorPolicy = GUI.enabled;
		GUI.enabled = !iskey;
	}

	public void Dispose()
	{
		GUI.enabled = visitorPolicy;
	}

	internal static bool CalculateDecorator()
	{
		return ChangeDecorator == null;
	}
}
