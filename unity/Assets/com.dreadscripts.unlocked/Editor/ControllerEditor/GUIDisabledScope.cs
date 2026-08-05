// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/GUIDisabledScope.cs
//
// Audit status: VERIFIED -- the field, the constructor and Dispose are the whole type and were
// diffed statement by statement against export/. Only the obfuscated parameter name (iskey) differs.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Greys out and blocks input to the controls drawn inside the scope, restoring the previous
    /// state on dispose.
    /// </summary>
    /// <remarks>
    /// Note the inverted sense: the argument is what to <em>disable</em>, matching Unity's own
    /// <c>EditorGUI.DisabledScope</c>.
    /// </remarks>
    internal sealed class GUIDisabledScope : IDisposable
    {
        private readonly bool previousEnabled;

        public GUIDisabledScope(bool disabled)
        {
            previousEnabled = GUI.enabled;
            GUI.enabled = !disabled;
        }

        public void Dispose()
        {
            GUI.enabled = previousEnabled;
        }
    }
}
