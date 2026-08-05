// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ShowMixedValueScope.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/MixedValueScope.cs
//
// Audit status: VERIFIED -- both copies diffed statement by statement against this file: the
// previous-value field, both constructors and Dispose. The SerializedProperty constructor chains to
// the bool one instead of repeating its two statements, as both shipped copies do; identical order
// of effects.

using System;
using UnityEditor;

namespace DreadScripts.Common
{
    /// <summary>
    /// Temporarily sets <see cref="EditorGUI.showMixedValue"/>, restoring the previous value on
    /// dispose. Used when drawing a field whose value differs across a multi-object selection.
    /// </summary>
    internal sealed class MixedValueScope : IDisposable
    {
        private readonly bool previous;

        public MixedValueScope(bool showMixedValue)
        {
            previous = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = showMixedValue;
        }

        /// <summary>Shows the mixed-value dash when <paramref name="property"/> differs across the selection.</summary>
        public MixedValueScope(SerializedProperty property)
            : this(property.hasMultipleDifferentValues)
        {
        }

        public void Dispose()
        {
            EditorGUI.showMixedValue = previous;
        }
    }
}
