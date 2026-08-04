// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   VRCFullOptions -> VRCFullOptions, lines 423-428  (vendor names; unobfuscated in the shipped build)
//   VRCHalfOptions -> VRCHalfOptions, lines 430-434
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The parameter-driver change types offered for a numeric parameter. Mirrors
        /// <c>VRCAvatarParameterDriver.ChangeType</c>, which the tool cannot reference directly, so
        /// the drawer boxes the driver's int through this enum to get a popup with the right labels.
        /// </summary>
        private enum VRCFullOptions
        {
            Set = 0,
            Add = 1,
            Random = 2
        }

        /// <summary>
        /// The same change types, minus <c>Add</c>, for a bool or trigger parameter — adding to a
        /// bool is meaningless. The gap in the numbering is deliberate: the values must stay equal
        /// to <see cref="VRCFullOptions"/>' so the same underlying int reads correctly either way.
        /// </summary>
        private enum VRCHalfOptions
        {
            Set = 0,
            Random = 2
        }
    }
}
