// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//
// Ported region: the PhysBoneParameter struct, lines 1547-1590 of the current snapshot. Line
// numbers move with the snapshot; the member names below are the durable reference.
//
//   PhysBoneParameter(string, AnimatorControllerParameterType, string) -> same, line 47
//   suffix                                                            -> same, line 31
//   parameterType                                                     -> same, line 37
//   hasBackingField                                                   -> same, line 43
//   valueField                                                        -> same, line 45
//   GetFloat(VRCPhysBoneBase)                                         -> same, line 61
//   GetBool(VRCPhysBoneBase)                                          -> same, line 69
//   GetValueString(VRCPhysBoneBase)                                   -> same, line 78
//   QueryDescriptor()                                                 -> not ported (see below)
//
// Deliberately not ported: the private `CallDescriptor` field and the `QueryDescriptor()` method
// that only tested it for null. Both are licence-check scaffolding left by the protector; nothing
// ever assigned CallDescriptor, so QueryDescriptor() was a constant `true` and no caller in either
// build reads it. The 2019 build carries the same pair under the names `CreateCandidate` and
// `CancelCandidate()`.
//
// The decompiled struct is nested inside the static class ADOEditorUtility. ADOEditorUtility is not
// ported yet, so this is lifted to a top-level type in the same namespace; call sites in the
// original read `ADOEditorUtility.PhysBoneParameter`.
//
// 2019 vs 2022: identical apart from decompiler-generated parameter names and an inverted ternary
// in the constructor. No behavioural divergence.

using System.Reflection;
using UnityEngine;
using VRC.Dynamics;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// One of the animator parameters a <see cref="VRCPhysBoneBase"/> drives, paired with the
    /// component field that holds its live value.
    /// </summary>
    /// <remarks>
    /// A PhysBone exposes its state through a set of animator parameters named by appending a fixed
    /// suffix to the component's <c>parameter</c> string — <c>_IsGrabbed</c>, <c>_Stretch</c> and so
    /// on. Two things need that mapping: the "add missing parameter" menu, which needs the suffix and
    /// the parameter's type, and the play-mode readout, which needs the current value.
    /// <para>
    /// The value is reached by reflection over a field name rather than a direct member access
    /// because the fields backing these parameters (<c>param_IsGrabbedValue</c>,
    /// <c>param_IsPosedValue</c>, <c>param_StretchValue</c>, <c>param_SquishValue</c>,
    /// <c>param_AngleValue</c>) are public instance fields on <see cref="VRCPhysBoneBase"/> that
    /// VRChat has added to over successive SDK releases. Looking them up by name lets a table entry
    /// for a parameter the installed SDK does not have yet degrade to
    /// <see cref="hasBackingField"/> = false instead of failing to compile. Callers filter on that
    /// flag before asking for a value.
    /// </para>
    /// <para>
    /// The package references the VRChat SDK unconditionally; there is no compilation symbol guarding
    /// SDK types anywhere in it, and this type does not introduce one.
    /// </para>
    /// </remarks>
    internal readonly struct PhysBoneParameter
    {
        /// <summary>Appended to the PhysBone's <c>parameter</c> string to form the animator parameter name.</summary>
        internal readonly string suffix;

        /// <summary>
        /// The animator parameter type this maps to. Only <see cref="AnimatorControllerParameterType.Bool"/>
        /// is distinguished; everything else is read as a float.
        /// </summary>
        internal readonly AnimatorControllerParameterType parameterType;

        /// <summary>
        /// Whether the installed SDK actually has the field this entry names. When false, the value
        /// accessors below will throw — callers are expected to check this first.
        /// </summary>
        internal readonly bool hasBackingField;

        private readonly FieldInfo valueField;

        /// <param name="valueFieldName">
        /// Name of the public instance field on <see cref="VRCPhysBoneBase"/> holding this
        /// parameter's live value, or null/blank for a parameter with no readable backing field.
        /// </param>
        internal PhysBoneParameter(string suffix, AnimatorControllerParameterType parameterType, string valueFieldName)
        {
            this.suffix = suffix;
            this.parameterType = parameterType;

            valueField = string.IsNullOrWhiteSpace(valueFieldName)
                ? null
                : typeof(VRCPhysBoneBase).GetField(valueFieldName, BindingFlags.Instance | BindingFlags.Public);

            hasBackingField = valueField != null;
        }

        /// <summary>
        /// Reads the parameter's current value off <paramref name="physBone"/> as a float. Valid only
        /// while <see cref="hasBackingField"/> is true and the avatar is in play mode; outside play
        /// mode these fields hold no meaningful state.
        /// </summary>
        internal float GetFloat(VRCPhysBoneBase physBone)
        {
            return (float)valueField.GetValue(physBone);
        }

        /// <summary>Reads the parameter's current value off <paramref name="physBone"/> as a bool.</summary>
        internal bool GetBool(VRCPhysBoneBase physBone)
        {
            return (bool)valueField.GetValue(physBone);
        }

        /// <summary>
        /// The current value formatted for display, reading it as whichever of bool or float
        /// <see cref="parameterType"/> calls for.
        /// </summary>
        public string GetValueString(VRCPhysBoneBase physBone)
        {
            if (parameterType == AnimatorControllerParameterType.Bool)
            {
                return GetBool(physBone).ToString();
            }

            return GetFloat(physBone).ToString();
        }
    }
}
