using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class ScrollViewScope : IDisposable
{
	private readonly bool began;

	internal ScrollViewScope(ref Vector2 i)
	{
		try
		{
			i = GUILayout.BeginScrollView(i);
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
