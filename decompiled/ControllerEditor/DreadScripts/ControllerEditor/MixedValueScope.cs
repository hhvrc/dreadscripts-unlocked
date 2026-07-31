using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal sealed class MixedValueScope : IDisposable
{
	private readonly bool previousMixedValue;

	public MixedValueScope(bool isinstance)
	{
		previousMixedValue = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isinstance;
	}

	public MixedValueScope(SerializedProperty v)
	{
		previousMixedValue = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = v.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = previousMixedValue;
	}
}
