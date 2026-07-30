using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class ScrollViewScope : IDisposable
{
	private readonly bool methodThread;

	internal static ScrollViewScope InstantiateStatus;

	internal ScrollViewScope(ref Vector2 first)
	{
		try
		{
			first = GUILayout.BeginScrollView(first);
			methodThread = true;
		}
		catch
		{
			methodThread = false;
		}
	}

	public void Dispose()
	{
		if (methodThread)
		{
			GUILayout.EndScrollView();
		}
	}

	internal static bool RevertStatus()
	{
		return InstantiateStatus == null;
	}
}
