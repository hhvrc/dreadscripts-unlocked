using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class WrapperPolicy : IDisposable
{
	private static WrapperPolicy ConnectDecorator;

	public WrapperPolicy()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
	}

	public WrapperPolicy(GUIStyle res)
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
