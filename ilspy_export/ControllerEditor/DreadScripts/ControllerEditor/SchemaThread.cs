using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class SchemaThread : IDisposable
{
	private readonly float m_BroadcasterThread;

	internal static SchemaThread RegisterDecorator;

	internal SchemaThread()
		: this(10f, 10f)
	{
	}

	internal SchemaThread(bool isfirst)
		: this(10f, 10f)
	{
	}

	internal SchemaThread(float res)
		: this(10f, res)
	{
	}

	internal SchemaThread(float first, float col)
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
