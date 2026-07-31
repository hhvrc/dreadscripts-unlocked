using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class CenteredHorizontalScope : IDisposable
{
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
}
