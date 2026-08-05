using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class ShowMixedValueScope : IDisposable
{
	private readonly bool previous;

	public ShowMixedValueScope(bool isv)
	{
		previous = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = isv;
	}

	public ShowMixedValueScope(SerializedProperty last)
	{
		previous = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = last.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = previous;
	}
}
