// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static FlushProcess   -> TryGetSurroundingKeyframes, line 2274
//   static ExcludeProcess -> TryEvaluateTangent,         line 2312
//   static InitProcess    -> CatmullRom,                 line 2328
//   static ConnectProcess -> TangentBetween,             line 2337
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// VENDOR BUG, reproduced as shipped: TangentBetween converts each keyframe's tangent to degrees
// (`57.29578f * Mathf.Atan(t)`), adds 180, and then feeds the result to Mathf.Tan -- which takes
// radians. Adding 180 to an angle is also a no-op for a tangent, since tan has period 180 degrees.
// So the two synthesised outer control points are not the geometric continuations the arithmetic is
// reaching for. The 2019 build has the identical expression, so it is what shipped rather than a
// decompilation artefact, and correcting it would change every curve the tool evaluates. Left as-is
// and recorded here instead.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Finds the pair of keyframes <paramref name="time"/> falls between on
        /// <paramref name="curve"/>.
        /// </summary>
        /// <param name="before">The last keyframe at or before <paramref name="time"/>.</param>
        /// <param name="after">The first keyframe at or after it.</param>
        /// <returns>
        /// True when <paramref name="time"/> is within the curve's keyed range. False when the curve
        /// is empty, has a single key, or <paramref name="time"/> is past the last key -- in which
        /// case <paramref name="before"/> may still have been set to the nearest key and
        /// <paramref name="after"/> is left at default.
        /// </returns>
        /// <remarks>
        /// When <paramref name="time"/> lands exactly on a keyframe, both out parameters are that
        /// keyframe, which is what lets the caller detect the case by comparing their times.
        /// </remarks>
        internal static bool TryGetSurroundingKeyframes(this AnimationCurve curve, float time, out Keyframe before, out Keyframe after)
        {
            before = default(Keyframe);
            after = default(Keyframe);

            if (curve.length == 0)
            {
                return false;
            }

            if (curve.length == 1)
            {
                before = curve[0];
                return false;
            }

            for (int i = 0; i < curve.length; i++)
            {
                Keyframe key = curve[i];

                if (key.time == time)
                {
                    before = after = key;
                    return true;
                }

                if (key.time > time)
                {
                    after = key;
                    return true;
                }

                before = key;
            }

            return false;
        }

        /// <summary>
        /// The slope of <paramref name="curve"/> at <paramref name="time"/>.
        /// </summary>
        /// <returns>False, with <paramref name="tangent"/> zeroed, when the time is outside the keyed range.</returns>
        /// <remarks>
        /// <see cref="AnimationCurve"/> can evaluate a value but not a slope, which is why this
        /// exists. Landing exactly on a keyframe takes that keyframe's outgoing tangent rather than
        /// differentiating, so a deliberate corner reads as its right-hand slope.
        /// </remarks>
        internal static bool TryEvaluateTangent(this AnimationCurve curve, float time, out float tangent)
        {
            tangent = 0f;

            if (!curve.TryGetSurroundingKeyframes(time, out Keyframe before, out Keyframe after))
            {
                return false;
            }

            tangent = (before.time != after.time) ? TangentBetween(before, after, time) : before.outTangent;
            return true;
        }

        /// <summary>
        /// The Catmull-Rom spline through <paramref name="p1"/> and <paramref name="p2"/>, with
        /// <paramref name="p0"/> and <paramref name="p3"/> as the outer control points, at
        /// <paramref name="t"/> in [0, 1].
        /// </summary>
        internal static float CatmullRom(float p0, float p1, float p2, float p3, float t)
        {
            float a = 2f * p1;
            float b = p2 - p0;
            float c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            float d = 0f - p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + b * t + c * t * t + d * t * t * t);
        }

        /// <summary>
        /// The slope of the segment from <paramref name="before"/> to <paramref name="after"/> at
        /// <paramref name="time"/>.
        /// </summary>
        /// <remarks>
        /// Fits a Catmull-Rom through the two keyframes, using each one's own tangent to synthesise
        /// the outer control point, then differentiates it numerically with a forward difference of
        /// 1e-5. See the vendor-bug note at the top of this file about the degree/radian mix in the
        /// two synthesised points. Note also that <paramref name="time"/> is passed to
        /// <see cref="CatmullRom"/> as the spline parameter directly, without being normalised
        /// against the segment's time span.
        /// </remarks>
        internal static float TangentBetween(Keyframe before, Keyframe after, float time)
        {
            float span = after.time - before.time;

            float beforeAngle = 57.29578f * Mathf.Atan(before.outTangent);
            float afterAngle = 57.29578f * Mathf.Atan(after.inTangent);

            float p1 = before.value;
            float p2 = after.value;
            float p0 = before.value + Mathf.Tan(beforeAngle + 180f) * span;
            float p3 = after.value + Mathf.Tan(afterAngle + 180f) * span;

            const float delta = 1E-05f;
            float at = CatmullRom(p0, p1, p2, p3, time);
            return (CatmullRom(p0, p1, p2, p3, time + delta) - at) / delta;
        }
    }
}
