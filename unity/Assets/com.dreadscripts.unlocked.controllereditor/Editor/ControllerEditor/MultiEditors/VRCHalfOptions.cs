// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   VRCHalfOptions -> VRCHalfOptions, line 430
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The type was a private nested enum of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// Audit status: VERIFIED -- diffed against the `private enum VRCHalfOptions` still at line 430 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, which declares exactly
// `Set = 0, Random = 2`; the explicit values the remarks call load-bearing are the shipped ones, not
// a port decision. Line 9337 is still the single call site, and it picks between this type and
// VRCFullOptions on parameter type before casting the result back to ChangeType.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The parameter-driver change modes offered for a bool or trigger parameter: the full set minus
    /// Add, which is meaningless on a boolean.
    /// </summary>
    /// <remarks>
    /// The explicit values are the point of the type. Random keeps its value of 2 from
    /// <see cref="AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType"/> rather than
    /// closing the gap left by Add, so the change-mode popup can be drawn with this enum in place of
    /// <see cref="VRCFullOptions"/> (line 9337) and the selected value still round-trips through the
    /// cast back to ChangeType. Renumbering these would silently turn every "Random" driver on a
    /// bool parameter into an "Add".
    /// </remarks>
    internal enum VRCHalfOptions
    {
        Set = 0,
        Random = 2
    }
}
