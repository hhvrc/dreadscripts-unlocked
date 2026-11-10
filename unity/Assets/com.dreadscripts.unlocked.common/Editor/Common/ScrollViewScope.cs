// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ScrollViewScope.cs
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ScrollViewScope.cs
//
// Audit status: VERIFIED -- both copies diffed statement by statement against this file. The
// `began` field, the ref-Vector2 constructor with its try/catch, and Dispose are transcribed
// exactly; the two shipped copies are byte-identical apart from the constructor parameter name.

using System;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Begins a GUILayout scroll view and ends it on dispose.
    /// </summary>
    /// <remarks>
    /// <see cref="GUILayout.BeginScrollView(Vector2, GUILayoutOption[])"/> is allowed to throw when
    /// the layout stack is in an inconsistent state — which happens routinely in the editor when a
    /// GUI call chain is interrupted by an exception or a hierarchy change mid-repaint. Swallowing
    /// it here and remembering whether the view actually opened keeps <see cref="Dispose"/> from
    /// emitting an unbalanced <c>EndScrollView</c>, which would otherwise turn one recoverable error
    /// into a cascade of layout-mismatch errors.
    /// </remarks>
    internal sealed class ScrollViewScope : IDisposable
    {
        private readonly bool began;

        internal ScrollViewScope(ref Vector2 scrollPosition)
        {
            try
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                began = true;
            }
            catch
            {
                began = false;
            }
        }

        public void Dispose()
        {
            if (began)
            {
                GUILayout.EndScrollView();
            }
        }
    }
}
