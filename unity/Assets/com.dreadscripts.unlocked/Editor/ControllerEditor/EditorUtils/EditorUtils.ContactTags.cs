// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static interpreterProcessor -> defaultCollisionTags, line 2244
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EditorUtils.ReservedParameterNames.cs deliberately left this table for its proper owner rather
// than filing it next to the reserved *parameter* names it sits beside in the decompiled file; they
// are separate VRChat tables about separate subsystems. This is that owner.
//
// The table has no caller anywhere in the decompiled ControllerEditor assembly -- it is carried in
// the shared utility class because the same class is compiled into ADOverhaul, where it does have
// callers. It is ported so the shared class is faithfully reconstructed, not because anything in
// this package reads it yet.
//
// The two products' copies are IDENTICAL here, unlike reservedAvatarParameters: both carry the same
// 23 tags in the same order. They are still kept as two independent arrays, one per product, for
// the reason set out at length in ADOEditorUtility.VRChatTables.cs and EditorUtils.
// ReservedParameterNames.cs -- these lists date their build and are behavioural inputs, and making
// two products share one would mean a later edit for one silently changing the other.

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The collision tags VRChat defines out of the box for avatar contact senders and receivers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A contact only fires when a sender and a receiver share at least one tag, so the tag is
        /// effectively the channel name. VRChat ships this set describing body parts, and the SDK's
        /// own inspector offers them as checkboxes distinct from the free-text custom tags; a tool
        /// that wants to reproduce that split -- or to warn that a "custom" tag the user typed is
        /// actually one of the built-ins under a different capitalisation -- needs the list, and the
        /// SDK does not expose it as an array.
        /// </para>
        /// <para>
        /// The ordering is VRChat's own, coarse to fine and then unsided-left-right, and is preserved
        /// because it is the order the tags are presented in.
        /// </para>
        /// </remarks>
        internal static readonly string[] defaultCollisionTags =
        {
            "Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing",
            "FingerLittle", "HandL", "FootL", "FingerL", "FingerIndexL", "FingerMiddleL",
            "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
            "FingerMiddleR", "FingerRingR", "FingerLittleR"
        };
    }
}
