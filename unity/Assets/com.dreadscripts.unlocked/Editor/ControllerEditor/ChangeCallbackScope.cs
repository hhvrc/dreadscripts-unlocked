// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ChangeCallbackScope.cs
//
// DELIBERATE DEVIATION
// Dispose gates the callback on `changeScope.changed`; the decompiled source calls it
// unconditionally and never reads `changed` at all, so in the shipped build the callback ran on
// every repaint and the change check the scope holds did nothing. See the type remarks for why the
// read has to precede the inner Dispose. This changes observable behaviour: a caller relying on the
// callback firing every pass will now only see it on an actual edit.
//
// Audit status: VERIFIED -- both fields, the constructor and Dispose were diffed statement by
// statement against export/. Everything matches apart from the `changed` gate recorded above;
// the try/finally and the inner changeScope.Dispose() are as decompiled.

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
