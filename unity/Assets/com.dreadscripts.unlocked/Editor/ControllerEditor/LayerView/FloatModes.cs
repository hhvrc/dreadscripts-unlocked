// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum FloatModes` nested in the static ControllerEditor class,
// line 2187 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// Used by the transition-condition inspector at line 11664, which is not ported.
//
// Audit status: VERIFIED -- diffed against the `private enum FloatModes` still at line 2187 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs. Same two members, same
// values: the decompiled source writes `Greater = 3, Less` and the port spells the implicit 4 on
// Less explicitly, which is the only difference. Line 11664 still holds the
// `selected = (FloatModes)...mode` cast the remarks describe.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The comparisons a transition condition may use against a float parameter.
    /// </summary>
    /// <remarks>
    /// THE VALUES ARE LOAD-BEARING. This is a filtered view of
    /// <see cref="AnimatorConditionMode"/>, not an independent enumeration: the condition editor
    /// casts the stored <see cref="AnimatorCondition.mode"/> straight into this type to draw the
    /// popup and casts the result straight back. Every member must therefore keep the numeric value
    /// its <see cref="AnimatorConditionMode"/> counterpart has, and the type exists only so that the
    /// popup offers the two comparisons that are legal for a float instead of all six modes.
    /// </remarks>
    internal enum FloatModes
    {
        /// <summary><see cref="AnimatorConditionMode.Greater"/>.</summary>
        Greater = 3,

        /// <summary><see cref="AnimatorConditionMode.Less"/>.</summary>
        Less = 4
    }
}
