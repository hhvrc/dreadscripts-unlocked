using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class AlgoPolicy : IDisposable
{
	private readonly Action m_MapperPolicy;

	private readonly EditorGUI.ChangeCheckScope m_InitializerPolicy;

	internal static AlgoPolicy GetDecorator;

	internal AlgoPolicy(Action key)
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
