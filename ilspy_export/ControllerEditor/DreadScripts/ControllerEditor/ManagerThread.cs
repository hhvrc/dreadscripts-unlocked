using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class ManagerThread : IDisposable
{
	private readonly bool m_ItemThread;

	internal static ManagerThread CompareStatus;

	public ManagerThread(bool isinstance)
	{
		m_ItemThread = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isinstance;
	}

	public ManagerThread(SerializedProperty v)
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
