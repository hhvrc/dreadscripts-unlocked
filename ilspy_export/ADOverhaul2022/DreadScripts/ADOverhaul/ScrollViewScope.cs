using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class ScrollViewScope : IDisposable
{
	private readonly bool facadeMethod;

	internal ScrollViewScope(ref Vector2 i)
	{
		try
		{
			i = GUILayout.BeginScrollView(i);
			facadeMethod = true;
		}
		catch
		{
			facadeMethod = false;
		}
	}

	public void Dispose()
	{
		if (facadeMethod)
		{
			GUILayout.EndScrollView();
		}
	}
}
