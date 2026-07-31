using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class ShowMixedValueScope : IDisposable
{
	private readonly bool _RequestMethod;

	public ShowMixedValueScope(bool extractdef)
	{
		_RequestMethod = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = extractdef;
	}

	public ShowMixedValueScope(SerializedProperty i)
	{
		_RequestMethod = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = i.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = _RequestMethod;
	}
}
