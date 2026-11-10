// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ManageQueue  -> TryGetBones(animator, out map, bones),        line 6544
//   static PrintQueue   -> TryGetBones(animator, useFallbacks, ...),     line 6550
//   static SearchQueue  -> TryGetBones(animator, ..., out cancelled, ...), line 6556
//   static RevertQueue  -> GetBoneTransformOrFallback,                   line 6593
//   static OrderList    -> HasBone,                                      line 6620
//   static CompareList  -> IsFinger,                                     line 6625
//   static SetList      -> IsToes,                                       line 6634
//   static PostList     -> IsLeft,                                       line 6643
//   static SetupList    -> IsRight,                                      line 6648
//   static EnableList   -> IsSided,                                      line 6653
//   static PublishList  -> TryGetMirroredBoneIndex,                      line 6663
//   static PopList      -> GetMirroredBoneIndex,                         line 6669
//   static MoveList     -> TryGetMirroredBone,                           line 6679
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// Humanoid rigs are optional in almost every part: Animator.GetBoneTransform returns null for a
// bone the avatar's Human description does not map, which is the normal case for fingers, toes,
// eyes, jaw and the upper chest. So every read here has a fallback or a report.
//
// THE MIRROR MATH is index arithmetic over HumanBodyBones, whose layout makes it possible:
// paired body bones alternate left/right from index 1 (LeftUpperLeg) to 22 (RightEye), so a swap is
// +1 on odd and -1 on even; the fifteen left finger bones run 24..38 and the fifteen right ones
// 39..53, so a swap is +/-15. Everything else -- Hips, Spine, Chest, Neck, Head, Jaw, UpperChest --
// is on the midline and mirrors to itself, which is what an offset of 0 means. The literals are
// transcribed rather than derived from the enum, as shipped.
//
// GetBoneTransformOrFallback walks *up* the rig when a bone is missing, on the reasoning that an
// unmapped bone's parent is the thing that actually moves it: a missing finger tip falls back to
// the phalanx above it (an unmapped proximal has no phalanx above and gives up), and missing toes
// fall back to the foot, 14 indices below.

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Resolves each of <paramref name="bones"/> on the animator, using the fallback walk for
        /// any that are unmapped.
        /// </summary>
        /// <returns>
        /// Whether every requested bone resolved. <paramref name="boneTransforms"/> always has an
        /// entry per requested bone, null included, so it can be indexed without checking.
        /// </returns>
        internal static bool TryGetBones(this Animator animator, out Dictionary<HumanBodyBones, Transform> boneTransforms,
            params HumanBodyBones[] bones)
        {
            return animator.TryGetBones(true, false, null, out bool _, out boneTransforms, bones);
        }

        /// <summary>
        /// <see cref="TryGetBones(Animator, out Dictionary{HumanBodyBones, Transform}, HumanBodyBones[])"/>
        /// with the fallback walk switchable.
        /// </summary>
        /// <param name="useFallbacks">
        /// False to use Animator.GetBoneTransform directly, so an unmapped bone stays null instead
        /// of resolving to an ancestor.
        /// </param>
        internal static bool TryGetBones(this Animator animator, bool useFallbacks,
            out Dictionary<HumanBodyBones, Transform> boneTransforms, params HumanBodyBones[] bones)
        {
            return animator.TryGetBones(useFallbacks, false, null, out bool _, out boneTransforms, bones);
        }

        /// <summary>
        /// <see cref="TryGetBones(Animator, bool, out Dictionary{HumanBodyBones, Transform}, HumanBodyBones[])"/>
        /// that can also put the missing bones in front of the user.
        /// </summary>
        /// <param name="prompt">
        /// Show a dialog listing the missing bones and offering to continue. Only shown when
        /// something is actually missing.
        /// </param>
        /// <param name="message">
        /// The first line of that dialog. Null uses "The following bones are missing from the
        /// avatar's rig:".
        /// </param>
        /// <param name="cancelled">
        /// Whether the user chose Cancel. False when nothing was missing or no prompt was asked
        /// for, so it never reports a refusal that was not made.
        /// </param>
        /// <remarks>
        /// The list in the dialog is built from HasBone, i.e. from the *unmapped* bones, not from
        /// the ones the fallback walk failed to resolve -- so a bone shown as missing may still
        /// have a usable transform in <paramref name="boneTransforms"/>.
        /// </remarks>
        internal static bool TryGetBones(this Animator animator, bool useFallbacks, bool prompt, string message,
            out bool cancelled, out Dictionary<HumanBodyBones, Transform> boneTransforms, params HumanBodyBones[] bones)
        {
            cancelled = false;
            boneTransforms = new Dictionary<HumanBodyBones, Transform>();

            bool complete = true;
            foreach (HumanBodyBones bone in bones)
            {
                Transform transform = useFallbacks
                    ? animator.GetBoneTransformOrFallback(bone)
                    : animator.GetBoneTransform(bone);

                boneTransforms.Add(bone, transform);
                if (transform == null)
                {
                    complete = false;
                }
            }

            if (complete || !prompt)
            {
                return complete;
            }

            StringBuilder missing = new StringBuilder();
            foreach (HumanBodyBones bone in bones)
            {
                if (!animator.HasBone(bone))
                {
                    missing.AppendLine(bone.ToString());
                }
            }

            if (message == null)
            {
                message = "The following bones are missing from the avatar's rig:";
            }

            cancelled = !EditorUtility.DisplayDialog("Missing Bones",
                $"{message}\n{missing}\n\nContinue anyway?", "Continue", "Cancel");
            return false;
        }

        /// <summary>
        /// The transform for <paramref name="bone"/>, or the nearest mapped ancestor when the rig
        /// does not map it.
        /// </summary>
        /// <param name="fallbackBone">
        /// A bone to try instead if the walk finds nothing. Tried once, through this same method,
        /// so it gets its own walk.
        /// </param>
        internal static Transform GetBoneTransformOrFallback(this Animator animator, HumanBodyBones bone,
            HumanBodyBones? fallbackBone = null)
        {
            Transform transform = animator.GetBoneTransform(bone);
            if (transform)
            {
                return transform;
            }

            int index = (int)bone;

            // A finger bone that is not the first phalanx falls back to the phalanx above it;
            // index % 3 == 0 is exactly the proximal of each of the ten fingers.
            if (bone.IsFinger() && index % 3 != 0)
            {
                transform = animator.GetBoneTransformOrFallback((HumanBodyBones)(index - 1));
            }
            else if (bone.IsToes())
            {
                transform = animator.GetBoneTransform((HumanBodyBones)(index - 14));
            }

            if (transform == null && fallbackBone.HasValue)
            {
                transform = animator.GetBoneTransformOrFallback(fallbackBone.Value);
            }

            return transform;
        }

        /// <summary>Whether the rig maps <paramref name="bone"/> to a transform.</summary>
        internal static bool HasBone(this Animator animator, HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone) != null;
        }

        /// <summary>Whether the bone is one of the thirty finger phalanges.</summary>
        internal static bool IsFinger(this HumanBodyBones bone)
        {
            return bone >= HumanBodyBones.LeftThumbProximal && bone <= HumanBodyBones.RightLittleDistal;
        }

        /// <summary>Whether the bone is either foot's toes.</summary>
        internal static bool IsToes(this HumanBodyBones bone)
        {
            return bone == HumanBodyBones.LeftToes || bone == HumanBodyBones.RightToes;
        }

        /// <summary>Whether the bone's name starts with "Left".</summary>
        internal static bool IsLeft(this HumanBodyBones bone)
        {
            return bone.ToString().StartsWith("Left");
        }

        /// <summary>Whether the bone's name starts with "Right".</summary>
        internal static bool IsRight(this HumanBodyBones bone)
        {
            return bone.ToString().StartsWith("Right");
        }

        /// <summary>Whether the bone belongs to a side rather than the midline.</summary>
        internal static bool IsSided(this HumanBodyBones bone)
        {
            string name = bone.ToString();
            return name.StartsWith("Left") || name.StartsWith("Right");
        }

        /// <summary>
        /// The index of the bone on the opposite side, reporting false -- with
        /// <paramref name="mirrored"/> set to the input -- for a midline bone that mirrors to
        /// itself.
        /// </summary>
        internal static bool TryGetMirroredBoneIndex(int boneIndex, out int mirrored)
        {
            mirrored = GetMirroredBoneIndex(boneIndex);
            return mirrored != boneIndex;
        }

        /// <summary>
        /// The index of the bone on the opposite side, or <paramref name="boneIndex"/> itself for a
        /// midline bone. See the note at the top of this file for the layout this relies on.
        /// </summary>
        internal static int GetMirroredBoneIndex(int boneIndex)
        {
            if (boneIndex <= 0)
            {
                return boneIndex;
            }

            int offset;
            if (boneIndex.IsBetween(11, 22) || boneIndex.IsBetween(1, 6))
            {
                // Paired body bones alternate left, right, left, right...
                offset = (boneIndex % 2 == 1) ? 1 : -1;
            }
            else if (boneIndex.IsBetween(24, 38))
            {
                offset = 15;
            }
            else if (boneIndex.IsBetween(39, 53))
            {
                offset = -15;
            }
            else
            {
                offset = 0;
            }

            return boneIndex + offset;
        }

        /// <summary>
        /// The bone on the opposite side, reporting false -- with <paramref name="mirrored"/> set
        /// to the input -- for a midline bone.
        /// </summary>
        internal static bool TryGetMirroredBone(this HumanBodyBones bone, out HumanBodyBones mirrored)
        {
            if (!TryGetMirroredBoneIndex((int)bone, out int mirroredIndex))
            {
                mirrored = bone;
                return false;
            }

            mirrored = (HumanBodyBones)mirroredIndex;
            return true;
        }
    }
}
