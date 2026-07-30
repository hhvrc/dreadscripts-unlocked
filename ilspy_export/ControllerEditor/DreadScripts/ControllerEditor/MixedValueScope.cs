using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class MixedValueScope : IDisposable
{
	private readonly bool m_ItemThread;

	internal static MixedValueScope CompareStatus;

	public MixedValueScope(bool isinstance)
	{
		m_ItemThread = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isinstance;
	}

	public MixedValueScope(SerializedProperty v)
	{
		m_ItemThread = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = v.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = m_ItemThread;
	}

	internal static bool PublishStatus()
	{
		return CompareStatus == null;
	}
}
