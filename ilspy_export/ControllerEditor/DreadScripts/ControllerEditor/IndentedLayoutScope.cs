using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class IndentedLayoutScope : IDisposable
{
	private readonly float m_BroadcasterThread;

	internal static IndentedLayoutScope RegisterDecorator;

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
		m_BroadcasterThread = col;
		GUILayout.BeginHorizontal();
		GUILayout.BeginHorizontal(GUILayout.MaxWidth(first));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical();
	}

	public void Dispose()
	{
		GUILayout.EndVertical();
		if (m_BroadcasterThread != 0f)
		{
			GUILayout.BeginHorizontal(GUILayout.MaxWidth(m_BroadcasterThread));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndHorizontal();
	}

	internal static bool FlushDecorator()
	{
		return RegisterDecorator == null;
	}
}
