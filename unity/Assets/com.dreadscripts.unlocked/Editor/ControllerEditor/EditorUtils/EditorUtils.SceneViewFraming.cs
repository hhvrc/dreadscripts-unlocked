// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static GetList      -> FrameSceneView(point, size, rotation, ...),      line 7209
//   static RateList     -> FrameSceneView(point, direction, size, ...),     line 7197
//   static DestroyList  -> FrameSceneView(point, offset, ...),              line 7203
//   static CalcList     -> FrameAvatar,                                     line 7224
//   static IncludeList  -> GetAvatarHeight,                                 line 7233
//   static LogoutQueue  -> GetHandAttachment(descriptor, points, ...),      line 6489
//   static PatchQueue   -> GetHandAttachment(descriptor, point, ...),       line 6494
//   static InterruptQueue -> GetHandRotation,                               line 6528
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// Moving the Scene view camera to look at something, and the avatar measurements the callers use to
// decide how far away "looking at it" should be.
//
// SIZE IS NOT DISTANCE. SceneView.LookAt takes the radius of the sphere it should fit on screen,
// so FrameSceneView converts the caller's requested distance with sin(fov/2) -- the half-angle the
// camera actually covers -- rather than passing it through. Get that backwards and every framing
// call ends up at a different distance on a different user's editor layout.
//
// GetAvatarHeight measures to the top of the head by extrapolating one neck-to-head length past the
// head bone, since the head *bone* sits well below the crown. A non-humanoid or unrigged avatar
// throws somewhere in that chain and falls back to the descriptor's view position, which the user
// has placed at eye level and is the best available answer.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Points the Scene view camera at <paramref name="point"/> from
        /// <paramref name="direction"/>, framing a sphere of radius <paramref name="size"/>.
        /// </summary>
        internal static void FrameSceneView(Vector3 point, Vector3 direction, float size = 1.4f,
            bool animated = false, bool orthographic = false)
        {
            FrameSceneView(point, size, Quaternion.LookRotation(-direction), animated, orthographic);
        }

        /// <summary>
        /// Points the Scene view camera at <paramref name="point"/> from
        /// <paramref name="offset"/>, framing a sphere whose radius is the offset's length -- so
        /// the offset gives both the viewing angle and the distance.
        /// </summary>
        internal static void FrameSceneView(Vector3 point, Vector3 offset, bool animated = false,
            bool orthographic = false)
        {
            FrameSceneView(point, offset.magnitude, Quaternion.LookRotation(-offset), animated, orthographic);
        }

        /// <summary>
        /// Points the Scene view camera at <paramref name="point"/>, framing a sphere of radius
        /// <paramref name="size"/> around it. Does nothing if there is no Scene view open.
        /// </summary>
        /// <param name="rotation">
        /// Where to view from. Null keeps the camera's current orientation and only moves it.
        /// </param>
        /// <param name="animated">
        /// Ease the camera over rather than jumping. This is the vendor's default here and the
        /// opposite of the default on the two overloads above.
        /// </param>
        internal static void FrameSceneView(Vector3 point, float size, Quaternion? rotation = null,
            bool animated = true, bool orthographic = false)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (!sceneView)
            {
                return;
            }

            Camera camera = sceneView.camera;
            if (!rotation.HasValue)
            {
                rotation = camera.transform.rotation;
            }

            float newSize = size * Mathf.Sin(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            sceneView.LookAt(point, rotation.Value, newSize, orthographic, !animated);
        }

        /// <summary>
        /// Frames the avatar's upper body from the front -- five-sixths of the way up its height,
        /// looking along the avatar's own forward.
        /// </summary>
        internal static void FrameAvatar(this VRCAvatarDescriptor avatar, bool animated = false)
        {
            float height = avatar.GetAvatarHeight();
            Transform transform = avatar.transform;
            Vector3 feet = transform.position;
            Vector3 crown = feet + Vector3.up * height;
            FrameSceneView((feet + crown * 5f) / 6f, transform.forward, height * 0.66f, animated);
        }

        /// <summary>
        /// The avatar's height in world units, measured from its root to the extrapolated top of
        /// the head. Falls back to the descriptor's view position height when the rig cannot be
        /// read.
        /// </summary>
        internal static float GetAvatarHeight(this VRCAvatarDescriptor avatar)
        {
            try
            {
                Vector3 root = avatar.transform.position;
                Animator animator = avatar.GetComponent<Animator>();
                Vector3 head = animator.GetBoneTransform(HumanBodyBones.Head).position;
                Vector3 neck = animator.GetBoneTransform(HumanBodyBones.Neck).position;

                // One more neck-to-head length past the head bone approximates the crown.
                return (head + (head - neck) - root).y;
            }
            catch
            {
                return avatar.ViewPosition.y;
            }
        }

        /// <summary>
        /// <see cref="GetHandAttachment(VRCAvatarDescriptor, Vector3, bool, out Vector3, out Quaternion, Vector3?, Vector3?)"/>
        /// for a set of points, attaching at their centroid.
        /// </summary>
        internal static void GetHandAttachment(VRCAvatarDescriptor avatar, IEnumerable<Vector3> points,
            bool rightHand, out Vector3 position, out Quaternion rotation, Vector3? positionOffset = null,
            Vector3? rotationOffset = null)
        {
            GetHandAttachment(avatar,
                points.Aggregate(Vector3.zero, (current, point) => current + point) / points.Count(),
                rightHand, out position, out rotation, positionOffset, rotationOffset);
        }

        /// <summary>
        /// Where to put something the avatar should appear to hold: a world position and rotation
        /// near <paramref name="point"/>, oriented to the chosen hand and scaled to the avatar.
        /// </summary>
        /// <param name="rightHand">
        /// Which hand. The left-hand form is the right-hand one mirrored: the position offset's X
        /// is negated, the rotation offset's Y is negated, and the result is turned 180 degrees.
        /// </param>
        /// <param name="positionOffset">
        /// An offset in hand space, in units of 1/50th of the avatar's height, so the same numbers
        /// work on avatars of any size.
        /// </param>
        /// <param name="rotationOffset">Euler angles applied in hand space.</param>
        /// <remarks>
        /// The base position is <paramref name="point"/> nudged along the hand rotation's up and
        /// right axes by the same 1/50th-of-height step -- the vendor's fixed clearance from the
        /// palm, not a computed one.
        /// </remarks>
        internal static void GetHandAttachment(VRCAvatarDescriptor avatar, Vector3 point, bool rightHand,
            out Vector3 position, out Quaternion rotation, Vector3? positionOffset = null,
            Vector3? rotationOffset = null)
        {
            avatar.GetComponent<Animator>().GetHandRotation(out Quaternion handRotation, rightHand);

            Vector3 up = handRotation * Vector3.up;
            Vector3 right = handRotation * Vector3.right * (rightHand ? 1 : -1);
            rotation = handRotation;

            float height = avatar.GetAvatarHeight();
            float step = height * 0.02f;
            position = point + up * step + right * step;

            if (positionOffset.HasValue)
            {
                Vector3 offset = positionOffset.Value;
                if (!rightHand)
                {
                    offset = new Vector3(-offset.x, offset.y, offset.z);
                }

                position += rotation * offset * height * 0.02f;
            }

            if (rotationOffset.HasValue)
            {
                Vector3 euler = rotationOffset.Value;
                if (!rightHand)
                {
                    euler = new Vector3(euler.x, -euler.y, euler.z);
                }

                rotation *= Quaternion.Euler(euler);
            }

            if (!rightHand)
            {
                rotation *= Quaternion.Euler(0f, 180f, 0f);
            }
        }

        /// <summary>
        /// A rotation whose up axis runs along the avatar's forearm, so something parented to it
        /// lines up with the hand.
        /// </summary>
        /// <returns>
        /// Always false. The vendor never used the return value and never set it; it is kept so the
        /// signature matches what shipped, but it says nothing about whether the rig was readable.
        /// Check <paramref name="rotation"/> against the +/-90 degree fallback instead.
        /// </returns>
        /// <remarks>
        /// Starts from a flat +/-90 degree roll and refines it by the actual lower-arm-to-hand
        /// direction, measured about the avatar's forward. A rig missing either bone throws inside
        /// the try and leaves the flat fallback in place.
        /// </remarks>
        internal static bool GetHandRotation(this Animator animator, out Quaternion rotation, bool rightHand = true)
        {
            rotation = rightHand ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.Euler(0f, 0f, 90f);

            try
            {
                Transform lowerArm = animator.GetBoneTransform(
                    rightHand ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
                Transform hand = animator.GetBoneTransform(
                    rightHand ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);

                Vector3 armDirection = (hand.position - lowerArm.position).normalized;
                float roll = Vector3.SignedAngle(rotation * Vector3.up, armDirection, animator.transform.forward);
                rotation *= Quaternion.Euler(0f, 0f, roll);
            }
            catch
            {
            }

            return false;
        }
    }
}
