// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum BoolModes` nested in the static ControllerEditor class,
// line 2201 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// Used by the transition-condition inspector at line 11659 and by the VRC parameter-driver inspector
// at line 9358, neither of which is ported.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The comparisons a transition condition may use against a bool parameter.
    /// </summary>
    /// <remarks>
    /// THE VALUES ARE LOAD-BEARING. This is a filtered view of
    /// <see cref="AnimatorConditionMode"/>, not an independent enumeration: the condition editor
    /// casts the stored <see cref="AnimatorCondition.mode"/> straight into this type to draw the
    /// popup and casts the result straight back, so <see cref="True"/> must stay
    /// <see cref="AnimatorConditionMode.If"/> and <see cref="False"/>
    /// <see cref="AnimatorConditionMode.IfNot"/>. The type exists to relabel those two as plain
    /// True/False, which reads better than Unity's own wording.
    ///
    /// The parameter-driver inspector reuses it as a nicer-looking bool field, mapping the popup
    /// onto the 0/1 the driver stores rather than onto a condition mode; the numeric values are
    /// irrelevant there, only the labels matter.
    /// </remarks>
    internal enum BoolModes
    {
        /// <summary><see cref="AnimatorConditionMode.If"/>.</summary>
        True = 1,

        /// <summary><see cref="AnimatorConditionMode.IfNot"/>.</summary>
        False = 2
    }
}
