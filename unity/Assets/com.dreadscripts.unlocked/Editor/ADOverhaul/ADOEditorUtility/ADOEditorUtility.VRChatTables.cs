// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static _AlgoSerializer    -> reservedAvatarParameters, line 2110
//   static m_RoleSerializer   -> defaultCollisionTags,     line 2117
//   static m_VisitorSerializer -> physBoneParameters,      line 2124
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
// Audit status: VERIFIED against export -- all three tables were re-checked entry by entry against
// lines 2110-2131 on 2026-08-04: 23 reserved parameters, 23 collision tags and 5 PhysBone
// parameters, same entries in the same order, and the same readonly/non-readonly split.
//
// 2019 vs 2022: the same three tables with the same entries in the same order (2019 lines 2112,
// 2119 and 2126, under different obfuscated names). No behavioural divergence.
//
// These are the VRChat SDK's own fixed vocabularies, hard-coded because the SDK does not expose them
// as arrays. They are grouped here rather than left beside unrelated statics because all three are
// the same kind of thing: a snapshot of what the platform reserves, which is what dates the tool
// when VRChat adds to a list.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Animator parameter names VRChat drives itself. Used to filter the avatar's own parameters
        /// out of the pickers, since binding a tool to one of these would be overwritten every frame.
        /// </summary>
        internal static readonly string[] reservedAvatarParameters =
        {
            "IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight",
            "GestureRightWeight", "AngularY", "VelocityX", "VelocityY", "VelocityZ",
            "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode",
            "MuteSelf", "InStation", "Earmuffs", "IsOnFriendsList", "AvatarVersion"
        };

        /// <summary>
        /// The contact collision tags built into the SDK, as opposed to the custom strings a user
        /// types.
        /// </summary>
        /// <remarks>
        /// The tag picker gathers every tag in use on the avatar, removes the ones on this list and
        /// re-adds them under a "Default/" prefix, so the built-ins group into a submenu and the
        /// user's own tags stay at the top level.
        /// </remarks>
        internal static readonly string[] defaultCollisionTags =
        {
            "Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing",
            "FingerLittle", "HandL", "FootL", "FingerL", "FingerIndexL", "FingerMiddleL",
            "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
            "FingerMiddleR", "FingerRingR", "FingerLittleR"
        };

        /// <summary>
        /// Every animator parameter a PhysBone can drive, paired with the runtime field that reports
        /// its current value.
        /// </summary>
        /// <remarks>
        /// Not readonly, matching the shipped build. Nothing reassigns it, but the field was left
        /// writable and callers are in the same assembly, so the accessibility is kept as it was.
        /// </remarks>
        internal static PhysBoneParameter[] physBoneParameters =
        {
            new PhysBoneParameter("_IsGrabbed", AnimatorControllerParameterType.Bool, "param_IsGrabbedValue"),
            new PhysBoneParameter("_IsPosed", AnimatorControllerParameterType.Bool, "param_IsPosedValue"),
            new PhysBoneParameter("_Stretch", AnimatorControllerParameterType.Float, "param_StretchValue"),
            new PhysBoneParameter("_Squish", AnimatorControllerParameterType.Float, "param_SquishValue"),
            new PhysBoneParameter("_Angle", AnimatorControllerParameterType.Float, "param_AngleValue")
        };
    }
}
