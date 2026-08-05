using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class ShowMixedValueScope : IDisposable
{
	private readonly bool previous;

	public ShowMixedValueScope(bool extractdef)
	{
		previous = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = extractdef;
	}

	public ShowMixedValueScope(SerializedProperty i)
	{
		previous = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = i.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = previous;
	}
}
