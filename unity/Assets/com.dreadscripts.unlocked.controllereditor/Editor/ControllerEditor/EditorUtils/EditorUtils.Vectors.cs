// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static SetPredicate     -> RoundToNearest(Vector3, int), line 3070
//   static PostPredicate    -> RoundToNearest(Vector2, int), line 3075
//   static SetupPredicate   -> MaxComponent,                 line 3080
//   static EnablePredicate  -> MinComponent,                 line 3085
//   static PublishPredicate -> AverageComponent,             line 3090
//   static PopPredicate     -> Negate,                       line 3095
//   static ComputePredicate -> Mask,                         line 3103
//   static MovePredicate    -> Add180,                       line 3111
//   static ConcatPredicate  -> Add180(Vector3, Axis),        line 3116
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// Small per-component Vector helpers. The Axis-taking pair exist for the mirror/flip tools: Negate
// mirrors a position or scale across the chosen axes, and Add180 does the equivalent to a Euler
// rotation. Add180 deliberately does not wrap into [0, 360) -- Unity's Transform.eulerAngles
// normalises on assignment, and the callers rely on the raw sum when comparing two rotations.
//
// The Axis flags enum is UnityEngine.Animations.Axis (X = 1, Y = 2, Z = 4), not a type of the
// vendor's; the constraint components use it and the tool reuses it for its own axis pickers.

using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Each component rounded to the nearest multiple of <paramref name="step"/>.</summary>
        internal static Vector3 RoundToNearest(this Vector3 value, int step)
        {
            return new Vector3(value.x.RoundToNearest(step), value.y.RoundToNearest(step), value.z.RoundToNearest(step));
        }

        /// <summary>Each component rounded to the nearest multiple of <paramref name="step"/>.</summary>
        internal static Vector2 RoundToNearest(this Vector2 value, int step)
        {
            return new Vector2(value.x.RoundToNearest(step), value.y.RoundToNearest(step));
        }

        /// <summary>The largest of the three components.</summary>
        internal static float MaxComponent(this Vector3 value)
        {
            return Mathf.Max(value.x, value.y, value.z);
        }

        /// <summary>The smallest of the three components.</summary>
        internal static float MinComponent(this Vector3 value)
        {
            return Mathf.Min(value.x, value.y, value.z);
        }

        /// <summary>The mean of the three components.</summary>
        internal static float AverageComponent(this Vector3 value)
        {
            return (value.x + value.y + value.z) / 3f;
        }

        /// <summary>
        /// The vector with the components named by <paramref name="axes"/> negated and the rest
        /// left alone.
        /// </summary>
        internal static Vector3 Negate(this Vector3 value, Axis axes = Axis.X)
        {
            return new Vector3(
                value.x * (((axes & Axis.X) == 0) ? 1f : -1f),
                value.y * (((axes & Axis.Y) == 0) ? 1f : -1f),
                value.z * (((axes & Axis.Z) == 0) ? 1f : -1f));
        }

        /// <summary>
        /// The vector with the components *not* named by <paramref name="axes"/> zeroed -- the
        /// projection onto the selected axes.
        /// </summary>
        internal static Vector3 Mask(this Vector3 value, Axis axes)
        {
            return new Vector3(
                value.x * (((axes & Axis.X) != Axis.None) ? 1f : 0f),
                value.y * (((axes & Axis.Y) != Axis.None) ? 1f : 0f),
                value.z * (((axes & Axis.Z) != Axis.None) ? 1f : 0f));
        }

        /// <summary>180 added to every component of a Euler rotation.</summary>
        internal static Vector3 Add180(this Vector3 eulerAngles)
        {
            return new Vector3(eulerAngles.x + 180f, eulerAngles.y + 180f, eulerAngles.z + 180f);
        }

        /// <summary>
        /// 180 added to the components of a Euler rotation named by <paramref name="axes"/>.
        /// </summary>
        internal static Vector3 Add180(this Vector3 eulerAngles, Axis axes)
        {
            return new Vector3(
                eulerAngles.x + (((axes & Axis.X) != Axis.None) ? 180f : 0f),
                eulerAngles.y + (((axes & Axis.Y) != Axis.None) ? 180f : 0f),
                eulerAngles.z + (((axes & Axis.Z) != Axis.None) ? 180f : 0f));
        }
    }
}
