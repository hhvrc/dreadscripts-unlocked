using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class CandidateDic : IDisposable
{
	private readonly bool m_ExpressionDic;

	internal static CandidateDic RestartDatabase;

	public CandidateDic(bool isv)
	{
		m_ExpressionDic = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isv;
	}

	public CandidateDic(SerializedProperty last)
	{
		m_ExpressionDic = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = last.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = m_ExpressionDic;
	}

	internal static bool MoveDatabase()
	{
		return RestartDatabase == null;
	}
}
