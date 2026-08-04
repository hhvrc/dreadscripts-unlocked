// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static RunList     -> ProjectOntoLine,                   line 7250
//   static CollectList -> MirrorTransform,                   line 7424
//   static ResolveList -> ReflectRotation(Quaternion, Vector3), line 7488
//   static ListList    -> ReflectRotation(Quaternion, PlaneAxis), line 7493
//   static VerifyList  -> SetLocalScaleKeepingWorldPositions, line 7503
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// MirrorTransform is the mirror tool's whole implementation and has two quite different modes.
// If the source sits under a humanoid Animator and its *parent* is a mapped human bone, the
// mirroring is structural: the source is reparented to the opposite bone, keeping its local
// position and rotation, so "the thing on my left hand" becomes "the thing on my right hand"
// correctly no matter what shape the avatar is. Only when that does not apply does it fall back to
// reflecting coordinates through a plane.
//
// Two things about that path are the vendor's and are transcribed rather than repaired:
//   * It reparents `source` and then writes the saved local transform onto `destination`. When the
//     two are the same object -- the way the tool calls it -- that is right. When they are not, the
//     source is left reparented with whatever local transform the new parent implies.
//   * The bone scan is `for (int i = 0; i < 55; i++)`, i.e. every HumanBodyBones value; the loop
//     `break`s out to the coordinate path when the parent bone has no mirror, so a midline parent
//     falls through rather than being reported.

using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The point on the line through <paramref name="lineOrigin"/> along
        /// <paramref name="lineDirection"/> nearest to <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="lineDirection"/> need not be normalised; it is normalised here.
        /// </remarks>
        internal static Vector3 ProjectOntoLine(Vector3 lineOrigin, Vector3 point, Vector3 lineDirection)
        {
            lineDirection = lineDirection.normalized;
            return lineOrigin + lineDirection * Vector3.Dot(point - lineOrigin, lineDirection);
        }

        /// <summary>
        /// Mirrors <paramref name="source"/>'s placement onto <paramref name="destination"/>.
        /// </summary>
        /// <param name="throughHumanoid">
        /// Try the bone-swap path first. Ignored when the source is a hierarchy root.
        /// </param>
        /// <param name="relativeToLocalSpace">
        /// With a <paramref name="pivot"/>: mirror in the pivot's local space rather than mirroring
        /// the world-space offset from it. The two differ as soon as the pivot is rotated.
        /// </param>
        /// <param name="positionAxes">Which axes the position is flipped across.</param>
        /// <param name="rotationPlane">
        /// The plane the rotation is reflected through. <c>None</c> leaves the rotation alone.
        /// </param>
        /// <param name="extraRotationAxes">
        /// Axes to additionally turn 180 degrees in the destination's own space, for handedness
        /// that the reflection alone does not fix.
        /// </param>
        internal static void MirrorTransform(Transform source, Transform destination, Transform pivot,
            bool throughHumanoid, bool relativeToLocalSpace, Axis positionAxes = Axis.X,
            PlaneAxis rotationPlane = PlaneAxis.YZ, Axis extraRotationAxes = Axis.None)
        {
            if (throughHumanoid && source != source.root)
            {
                Animator animator = source.root.GetComponentInChildren<Animator>();
                if (animator != null && animator.avatar && animator.isHuman
                    && source.IsChildOf(animator.transform))
                {
                    for (int i = 0; i < 55; i++)
                    {
                        Transform bone = animator.GetBoneTransform((HumanBodyBones)i);
                        if (!bone || bone != source.parent)
                        {
                            continue;
                        }

                        if (!TryGetMirroredBoneIndex(i, out int mirroredIndex))
                        {
                            break;
                        }

                        Transform mirroredBone = animator.GetBoneTransform((HumanBodyBones)mirroredIndex);
                        if (!mirroredBone)
                        {
                            Debug.LogWarning(
                                "Attempting to mirror through humanoid but mirror human bone can't be found!");
                            break;
                        }

                        Vector3 localPosition = source.localPosition;
                        Quaternion localRotation = source.localRotation;
                        Undo.SetTransformParent(source, mirroredBone, "Mirror Transform");
                        destination.localPosition = localPosition;
                        destination.localRotation = localRotation;
                        Debug.Log("Mirrored!");
                        return;
                    }
                }
            }

            if (!pivot)
            {
                destination.position = source.position.Negate(positionAxes);
            }
            else if (relativeToLocalSpace)
            {
                Vector3 local = pivot.InverseTransformPoint(source.position).Negate(positionAxes);
                destination.position = pivot.TransformPoint(local);
            }
            else
            {
                Vector3 position = source.position;
                Vector3 pivotPosition = pivot.position;

                // Mirror the offset from the pivot, then keep only the components on the mirrored
                // axes: the others must not move at all.
                Vector3 delta = pivotPosition + (position - pivotPosition).Negate(positionAxes) - position;
                destination.position = position + delta.Mask(positionAxes);
            }

            if (rotationPlane == PlaneAxis.None)
            {
                return;
            }

            destination.rotation = ReflectRotation(source.rotation, rotationPlane);
            if (extraRotationAxes != Axis.None)
            {
                destination.Rotate(
                    extraRotationAxes.HasFlag(Axis.X) ? 180 : 0,
                    extraRotationAxes.HasFlag(Axis.Y) ? 180 : 0,
                    extraRotationAxes.HasFlag(Axis.Z) ? 180 : 0,
                    Space.Self);
            }
        }

        /// <summary>
        /// <paramref name="rotation"/> reflected through the plane whose normal is
        /// <paramref name="planeNormal"/>.
        /// </summary>
        /// <remarks>
        /// Built by reflecting the rotation's forward and up axes separately and rebuilding a
        /// rotation from the pair. A reflection is not itself a rotation -- it reverses handedness
        /// -- so this is the closest rotation to one, which is exactly what a mirrored object
        /// needs.
        /// </remarks>
        private static Quaternion ReflectRotation(Quaternion rotation, Vector3 planeNormal)
        {
            return Quaternion.LookRotation(
                Vector3.Reflect(rotation * Vector3.forward, planeNormal),
                Vector3.Reflect(rotation * Vector3.up, planeNormal));
        }

        /// <summary>
        /// <paramref name="rotation"/> reflected through a world axis plane. Anything other than
        /// XY or XZ -- including None -- is treated as YZ.
        /// </summary>
        private static Quaternion ReflectRotation(Quaternion rotation, PlaneAxis plane)
        {
            Vector3 normal;
            switch (plane)
            {
                case PlaneAxis.XY:
                    normal = Vector3.forward;
                    break;
                case PlaneAxis.XZ:
                    normal = Vector3.up;
                    break;
                default:
                    normal = Vector3.right;
                    break;
            }

            return ReflectRotation(rotation, normal);
        }

        /// <summary>
        /// Sets <paramref name="target"/>'s local scale while leaving each of
        /// <paramref name="keepInPlace"/> where it was in world space.
        /// </summary>
        /// <remarks>
        /// Scaling a transform drags its descendants along; this snapshots their world positions
        /// first and puts them back afterwards, which is what "scale the bone but not the things
        /// attached to it" needs. Only positions are restored -- a scaled parent still scales its
        /// children's world scale. Null entries are skipped throughout.
        /// </remarks>
        internal static void SetLocalScaleKeepingWorldPositions(Transform target, Vector3 localScale,
            Transform[] keepInPlace, bool recordUndo = true)
        {
            if (recordUndo)
            {
                Undo.RecordObject(target, "Scale and Preserve");
                foreach (Transform transform in keepInPlace)
                {
                    if (transform != null)
                    {
                        Undo.RecordObject(transform, "Scale and Preserve");
                    }
                }
            }

            Vector3[] positions = new Vector3[keepInPlace.Length];
            for (int i = 0; i < keepInPlace.Length; i++)
            {
                if (keepInPlace[i] != null)
                {
                    positions[i] = keepInPlace[i].position;
                }
            }

            target.localScale = localScale;

            for (int i = 0; i < keepInPlace.Length; i++)
            {
                if (keepInPlace[i] != null)
                {
                    keepInPlace[i].position = positions[i];
                }
            }
        }
    }
}
