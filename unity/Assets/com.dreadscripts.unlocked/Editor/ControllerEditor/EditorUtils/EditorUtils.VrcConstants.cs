// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_AdapterProcessor   -> vrcBuiltInParameters, line 2238
//   static interpreterProcessor -> vrcCollisionTags,     line 2245
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- both tables transcribed entry for entry, in order.
//
// Two literal tables of VRChat-defined names. They are transcribed rather than read from the SDK
// because the SDK does not expose either as a list: the built-in parameters exist only as rows in
// the avatar descriptor's inspector, and the collision tags only as entries in a dropdown. That
// means both go stale when VRChat adds to them, and the entries at the end of vrcBuiltInParameters
// (AvatarVersion onwards) show which SDK generation this snapshot was built against.
//
// The tool treats a parameter whose name is in vrcBuiltInParameters as one it must not rename,
// prefix or add to an expression parameters asset -- VRChat drives them itself and a copy would
// both cost budget and never update.

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The animator parameters VRChat writes itself. A parameter with one of these names is
        /// driven by the platform, not by the avatar, so it must keep its name exactly and must
        /// not be declared in an expression parameters asset.
        /// </summary>
        internal static readonly string[] vrcBuiltInParameters =
        {
            "IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight",
            "AngularY", "VelocityX", "VelocityY", "VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated",
            "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList", "AvatarVersion",
            "ScaleModified", "ScaleFactor", "ScaleFactorInverse", "EyeHeightAsMeters", "EyeHeightAsPercent"
        };

        /// <summary>
        /// The collision tags VRChat defines for contact senders and receivers. The unsuffixed
        /// entries match either side; the L and R suffixed ones match only that side.
        /// </summary>
        internal static readonly string[] vrcCollisionTags =
        {
            "Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle",
            "HandL", "FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL",
            "HandR", "FootR", "FingerR", "FingerIndexR", "FingerMiddleR", "FingerRingR", "FingerLittleR"
        };
    }
}
