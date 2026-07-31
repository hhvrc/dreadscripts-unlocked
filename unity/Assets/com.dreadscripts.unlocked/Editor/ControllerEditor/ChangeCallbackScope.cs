// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ChangeCallbackScope.cs

using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Runs <paramref name="callback"/> when the user changed any control drawn inside the scope.
    /// </summary>
    /// <remarks>
    /// The shipped build never read <see cref="EditorGUI.ChangeCheckScope.changed"/> and so ran the
    /// callback on every repaint, leaving the change check it holds with nothing to do. Reading
    /// <c>changed</c> before disposing the inner scope is what makes it change-gated; the read has
    /// to happen first, because disposing the scope is what ends the change check.
    /// </remarks>
    internal sealed class ChangeCallbackScope : IDisposable
    {
        private readonly Action callback;

        private readonly EditorGUI.ChangeCheckScope changeScope;

        internal ChangeCallbackScope(Action callback)
        {
            this.callback = callback;
            changeScope = new EditorGUI.ChangeCheckScope();
        }

        public void Dispose()
        {
            try
            {
                if (changeScope.changed)
                {
                    callback();
                }
            }
            finally
            {
                changeScope.Dispose();
            }
        }
    }
}
