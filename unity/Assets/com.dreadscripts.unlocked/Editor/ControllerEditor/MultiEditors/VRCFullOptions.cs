// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   VRCFullOptions -> VRCFullOptions, line 423
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The type was a private nested enum of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// This is value-for-value identical to AnimatorTypeCache.ParameterDriverBinding.ParameterEntry
// .ChangeType, which is already ported. It is *not* a duplicate to be collapsed into it: it exists
// solely as the type handed to EditorGUI.EnumPopup (line 9337) so that the popup's option list can
// be swapped at runtime, and deleting it would change what that popup shows. See VRCHalfOptions.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The parameter-driver change modes offered for a float or int parameter, i.e. all of them.
    /// </summary>
    /// <remarks>
    /// Only ever used as the enum type of the change-mode popup; the value is cast back to
    /// <see cref="AnimatorTypeCache.ParameterDriverBinding.ParameterEntry.ChangeType"/> immediately,
    /// which the matching numbering makes safe.
    /// </remarks>
    internal enum VRCFullOptions
    {
        Set,
        Add,
        Random
    }
}
