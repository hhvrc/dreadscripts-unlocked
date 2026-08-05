// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum IntModes` nested in the static ControllerEditor class,
// line 2193 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// Used by the transition-condition inspector at line 11670, which is not ported.
//
// Audit status: VERIFIED -- diffed against the `private enum IntModes` still at line 2193 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; all four members carry
// the same explicit values there (3, 4, 6, 7), gap at 5 included. Line 11670 still holds the
// `selected = (IntModes)...mode` cast the remarks describe.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The comparisons a transition condition may use against an int parameter.
    /// </summary>
    /// <remarks>
    /// THE VALUES ARE LOAD-BEARING. This is a filtered view of
    /// <see cref="AnimatorConditionMode"/>, not an independent enumeration: the condition editor
    /// casts the stored <see cref="AnimatorCondition.mode"/> straight into this type to draw the
    /// popup and casts the result straight back. Every member must therefore keep the numeric value
    /// its <see cref="AnimatorConditionMode"/> counterpart has. Value 5 is deliberately absent
    /// because <see cref="AnimatorConditionMode"/> has no such member.
    /// </remarks>
    internal enum IntModes
    {
        /// <summary><see cref="AnimatorConditionMode.Greater"/>.</summary>
        Greater = 3,

        /// <summary><see cref="AnimatorConditionMode.Less"/>.</summary>
        Less = 4,

        /// <summary><see cref="AnimatorConditionMode.Equals"/>.</summary>
        Equals = 6,

        /// <summary><see cref="AnimatorConditionMode.NotEqual"/>.</summary>
        NotEqual = 7
    }
}
