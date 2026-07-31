using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class GUIDisabledScope : IDisposable
{
	private readonly bool previousEnabled;

	public GUIDisabledScope(bool iskey)
	{
		previousEnabled = GUI.enabled;
		GUI.enabled = !iskey;
	}

	public void Dispose()
	{
		GUI.enabled = previousEnabled;
	}
}
