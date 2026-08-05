// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/CenteredHorizontalScope.cs
//
// Audit status: VERIFIED -- both constructors and Dispose diffed statement by statement against
// export/; the call sequence is identical. Only the obfuscated parameter name (res) differs.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A horizontal layout group with flexible space on both sides, so whatever is drawn inside ends
    /// up centred in the available width.
    /// </summary>
    internal class CenteredHorizontalScope : IDisposable
    {
        public CenteredHorizontalScope()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }

        public CenteredHorizontalScope(GUIStyle style)
        {
            EditorGUILayout.BeginHorizontal(style);
            GUILayout.FlexibleSpace();
        }

        public void Dispose()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}
