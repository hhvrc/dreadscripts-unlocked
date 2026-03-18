using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul;

internal sealed class InfoAccountCollection : IDisposable
{
	private readonly bool _RequestMethod;

	internal static InfoAccountCollection DefineState;

	public InfoAccountCollection(bool extractdef)
	{
		_RequestMethod = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = extractdef;
	}

	public InfoAccountCollection(SerializedProperty i)
	{
		_RequestMethod = EditorGUI.showMixedValue;
		EditorGUI.showMixedValue = i.hasMultipleDifferentValues;
	}

	public void Dispose()
	{
		EditorGUI.showMixedValue = _RequestMethod;
	}

	internal static bool TestState()
	{
		return DefineState == null;
	}
}
