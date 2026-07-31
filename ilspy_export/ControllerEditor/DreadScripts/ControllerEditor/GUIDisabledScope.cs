using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class GUIDisabledScope : IDisposable
{
	private readonly bool visitorPolicy;

	public GUIDisabledScope(bool iskey)
	{
		visitorPolicy = GUI.enabled;
		GUI.enabled = !iskey;
	}

	public void Dispose()
	{
		GUI.enabled = visitorPolicy;
	}
}
