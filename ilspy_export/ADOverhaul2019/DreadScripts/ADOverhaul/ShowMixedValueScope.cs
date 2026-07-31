using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class ShowMixedValueScope : IDisposable
{
	private readonly bool m_ExpressionDic;

	public ShowMixedValueScope(bool isv)
	{
		m_ExpressionDic = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isv;
	}

	public ShowMixedValueScope(SerializedProperty last)
	{
		m_ExpressionDic = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = last.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = m_ExpressionDic;
	}
}
