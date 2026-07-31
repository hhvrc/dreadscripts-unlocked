using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class ScrollViewScope : IDisposable
{
	private readonly bool began;

	internal ScrollViewScope(ref Vector2 first)
	{
		try
		{
			first = GUILayout.BeginScrollView(first);
			began = true;
		}
		catch
		{
			began = false;
		}
	}

	public void Dispose()
	{
		if (began)
		{
			GUILayout.EndScrollView();
		}
	}
}
