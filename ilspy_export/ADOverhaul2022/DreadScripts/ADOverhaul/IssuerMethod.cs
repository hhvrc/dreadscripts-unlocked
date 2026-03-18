using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class IssuerMethod : IDisposable
{
	private readonly bool facadeMethod;

	internal static IssuerMethod CallState;

	internal IssuerMethod(ref Vector2 i)
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

	internal static bool QueryState()
	{
		return CallState == null;
	}
}
