using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class CenteredHorizontalScope : IDisposable
{
	private static CenteredHorizontalScope ConnectDecorator;

	public CenteredHorizontalScope()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
	}

	public CenteredHorizontalScope(GUIStyle res)
	{
		EditorGUILayout.BeginHorizontal(res);
		GUILayout.FlexibleSpace();
	}

	public void Dispose()
	{
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	internal static bool ViewDecorator()
	{
		return ConnectDecorator == null;
	}
}
