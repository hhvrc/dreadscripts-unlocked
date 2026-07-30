using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class ChangeCallbackScope : IDisposable
{
	private readonly Action m_MapperPolicy;

	private readonly EditorGUI.ChangeCheckScope m_InitializerPolicy;

	internal static ChangeCallbackScope GetDecorator;

	internal ChangeCallbackScope(Action key)
	{
		m_MapperPolicy = key;
		m_InitializerPolicy = new EditorGUI.ChangeCheckScope();
	}

	public void Dispose()
	{
		try
		{
			m_MapperPolicy();
		}
		finally
		{
			m_InitializerPolicy.Dispose();
		}
	}

	internal static bool VisitDecorator()
	{
		return GetDecorator == null;
	}
}
