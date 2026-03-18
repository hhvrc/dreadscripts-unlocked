using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class AnnotationPolicy : IDisposable
{
	private readonly bool visitorPolicy;

	private static AnnotationPolicy ChangeDecorator;

	public AnnotationPolicy(bool iskey)
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
