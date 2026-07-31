using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class IndentedLayoutScope : IDisposable
{
	private readonly float rightPadding;

	internal IndentedLayoutScope()
		: this(10f, 10f)
	{
	}

	internal IndentedLayoutScope(bool isfirst)
		: this(10f, 10f)
	{
	}

	internal IndentedLayoutScope(float res)
		: this(10f, res)
	{
	}

	internal IndentedLayoutScope(float first, float col)
	{
		rightPadding = col;
		GUILayout.BeginHorizontal();
		GUILayout.BeginHorizontal(GUILayout.MaxWidth(first));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical();
	}

	public void Dispose()
	{
		GUILayout.EndVertical();
		if (rightPadding != 0f)
		{
			GUILayout.BeginHorizontal(GUILayout.MaxWidth(rightPadding));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndHorizontal();
	}
}
