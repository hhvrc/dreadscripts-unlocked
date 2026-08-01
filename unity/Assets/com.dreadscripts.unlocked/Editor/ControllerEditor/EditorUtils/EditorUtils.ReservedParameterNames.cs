// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_AdapterProcessor -> reservedAvatarParameters, line 2238
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This table would normally have gone into EditorUtils.Parameters.cs, but that partial is already
// taken by the expression-parameter cost helpers, which are a different subject: those are about
// the VRChat *expression* parameter budget, this is about *animator* parameter names.
//
// Adjacent in the decompiled file and deliberately not ported here: interpreterProcessor
// (line 2244), the 23 built-in contact collision tags. It is the same table the ADOverhaul side
// already ships as ADOEditorUtility.defaultCollisionTags, it has no caller among the members being
// reconstructed in this pass, and it belongs to whichever partial ends up owning the contact
// helpers rather than here.
//
// DIVERGENCE FROM THE ADOVERHAUL TWIN -- READ BEFORE "DEDUPLICATING".
// ADOEditorUtility.reservedAvatarParameters (Editor/ADOverhaul/ADOEditorUtility.VRChatTables.cs) is
// the same VRChat list, but as it stood at an earlier date: 23 entries, stopping at AvatarVersion.
// The ControllerEditor build shipped later and carries 28 -- the five avatar-scaling parameters
// VRChat added afterwards (ScaleModified, ScaleFactor, ScaleFactorInverse, EyeHeightAsMeters,
// EyeHeightAsPercent). The two are NOT to be merged or made to share a single array. Each product
// shipped with the list it shipped with, and the list is a behavioural input: it decides which
// parameters a picker hides and which names a rename refuses. Unifying them would silently change
// what ADOverhaul does, which is exactly the kind of "fix" this reconstruction does not make.

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The animator parameter names VRChat drives itself on an avatar.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These names are not the user's to own. VRChat writes them every frame from the player's
        /// own state -- gestures, velocity, tracking mode, avatar scale -- so a tool that bound one
        /// of its own toggles to <c>Grounded</c> would see its value overwritten immediately. The
        /// list is consulted in three places, all of them variations on the same idea: parameter
        /// pickers filter these out of the "your parameters" section, the rename flow refuses to
        /// rename onto one of them, and the copy flow skips them when merging a controller's
        /// parameters into another, since the destination already has them by definition.
        /// </para>
        /// <para>
        /// It is hard-coded because the SDK does not expose it as an array, which means the list
        /// dates the build: a parameter VRChat adds after this array was written is treated as an
        /// ordinary user parameter until the tool is updated. That is the failure mode to expect if
        /// a newer platform parameter starts showing up in the pickers.
        /// </para>
        /// </remarks>
        internal static readonly string[] reservedAvatarParameters =
        {
            "IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight",
            "GestureRightWeight", "AngularY", "VelocityX", "VelocityY", "VelocityZ",
            "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode",
            "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList", "AvatarVersion",
            "ScaleModified", "ScaleFactor", "ScaleFactorInverse", "EyeHeightAsMeters",
            "EyeHeightAsPercent"
        };
    }
}
