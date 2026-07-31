using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class FoldoutScope : IDisposable
{
	private readonly bool isExpanded;

	private readonly IndentedLayoutScope indentScope;

	public FoldoutScope(bool vopen)
		: this(ref vopen, iscfg: false, null)
	{
	}

	public FoldoutScope(ref bool def)
		: this(ref def, iscfg: false, null)
	{
	}

	public FoldoutScope(ref bool value, string col, GUIStyle c = null)
		: this(ref value, iscfg: true, new GUIContent(col), c)
	{
	}

	public FoldoutScope(ref bool info, GUIContent connection, GUIStyle filter = null)
		: this(ref info, iscfg: true, connection, filter)
	{
	}

	public FoldoutScope(ref bool spec, bool iscfg, GUIContent proc, GUIStyle info2 = null)
	{
		if (iscfg)
		{
			if (info2 == null)
			{
				info2 = EditorStyles.foldout;
			}
			spec = EditorGUILayout.Foldout(spec, proc, info2);
		}
		bool num = spec;
		bool flag = num;
		isExpanded = num;
		if (flag)
		{
			indentScope = new IndentedLayoutScope();
		}
	}

	public void Dispose()
	{
		if (isExpanded)
		{
			indentScope.Dispose();
		}
	}

	public static implicit operator bool(FoldoutScope last)
	{
		return last.isExpanded;
	}
}
