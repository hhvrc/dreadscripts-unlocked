using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class ChangeCallbackScope : IDisposable
{
	private readonly Action callback;

	private readonly EditorGUI.ChangeCheckScope changeScope;

	internal ChangeCallbackScope(Action key)
	{
		callback = key;
		changeScope = new EditorGUI.ChangeCheckScope();
	}

	public void Dispose()
	{
		try
		{
			callback();
		}
		finally
		{
			changeScope.Dispose();
		}
	}
}
