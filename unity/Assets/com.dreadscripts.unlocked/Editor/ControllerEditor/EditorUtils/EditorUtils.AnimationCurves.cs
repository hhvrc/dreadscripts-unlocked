// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static DisablePredicate -> TryGetSurroundingKeys,     line 3144
//   static InsertPredicate  -> TryGetTangentAt,           line 3183
//   static RestartPredicate -> CatmullRom,                line 3199
//   static QueryPredicate   -> EstimateTangentBetween,    line 3208
//   static AddPredicate     -> CreateCurve(TangentMode, params Keyframe[]),      line 3221
//   static InvokePredicate  -> CreateCurve(TangentMode, params (float,float)[]), line 3226
//   static FindPredicate    -> GetEffectiveLength,        line 3242
//   static ExcludePredicate -> TryGetBinding(AnimationClip, ...),                line 3255
//   static InitPredicate    -> TryGetBinding(IEnumerable<EditorCurveBinding>, ...), line 3260
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// TWO VENDOR BUGS ARE PRESERVED HERE. Both are in the shipped assembly, not artefacts of the
// decompile, and both are transcribed rather than corrected -- fixing them would change what the
// tool does, and nothing in the reconstruction is in a position to say which behaviour users
// depend on. They are called out at their sites:
//   * EstimateTangentBetween mixes degrees and radians, so the "tangent" it returns is not the
//     curve's slope.
//   * TryGetBinding(AnimationClip, ...) searches the float curves when asked about a discrete
//     (object-reference) binding and the object-reference curves otherwise -- the two branches are
//     the wrong way round.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The two keys bracketing <paramref name="time"/>, or the same key twice when
        /// <paramref name="time"/> lands exactly on one.
        /// </summary>
        /// <returns>
        /// False when the curve cannot bracket the time: it is empty, it has a single key, or the
        /// time is past the last key. In the single-key case <paramref name="before"/> is still set
        /// to that key.
        /// </returns>
        /// <remarks>
        /// A time before the first key returns true with <paramref name="before"/> left at
        /// default(Keyframe) -- i.e. time 0, value 0 -- rather than being rejected. That is the
        /// vendor's behaviour and callers have to allow for it.
        /// </remarks>
        internal static bool TryGetSurroundingKeys(this AnimationCurve curve, float time, out Keyframe before,
            out Keyframe after)
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

                if (key.time >= time)
                {
                    after = key;
                    return true;
                }

                before = key;
            }

            return false;
        }

        /// <summary>
        /// The slope of the curve at <paramref name="time"/>, or false when the curve cannot
        /// bracket that time.
        /// </summary>
        /// <remarks>
        /// Landing exactly on a key gives that key's outgoing tangent. Between keys the value comes
        /// from <see cref="EstimateTangentBetween"/> -- see the bug noted there.
        /// </remarks>
        internal static bool TryGetTangentAt(this AnimationCurve curve, float time, out float tangent)
        {
            tangent = 0f;
            if (!curve.TryGetSurroundingKeys(time, out Keyframe before, out Keyframe after))
            {
                return false;
            }

            if (before.time == after.time)
            {
                tangent = before.outTangent;
                return true;
            }

            tangent = EstimateTangentBetween(before, after, time);
            return true;
        }

        /// <summary>
        /// The Catmull-Rom spline through <paramref name="p1"/> and <paramref name="p2"/> with
        /// <paramref name="p0"/> and <paramref name="p3"/> as the surrounding control points,
        /// evaluated at <paramref name="t"/>.
        /// </summary>
        internal static float CatmullRom(float p0, float p1, float p2, float p3, float t)
        {
            float a = 2f * p1;
            float b = p2 - p0;
            float c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            float d = -p0 + 3f * p1 - 3f * p2 + p3;
            return 0.5f * (a + b * t + c * t * t + d * t * t * t);
        }

        /// <summary>
        /// The slope between two keyframes at <paramref name="time"/>, taken as the numeric
        /// derivative of a Catmull-Rom spline fitted through them.
        /// </summary>
        /// <remarks>
        /// VENDOR BUG, transcribed as shipped. The two outer control points are meant to be the
        /// neighbouring keys projected along the keyframes' tangents, but the projection converts
        /// each tangent to *degrees* and then feeds it to <see cref="Mathf.Tan"/>, which takes
        /// radians -- so the control points are unrelated to the actual tangents and the returned
        /// slope is not the curve's. The `+ 180f` is equally suspect: in degrees it negates the
        /// tangent, which would be deliberate only if the intent were to reflect it.
        /// <para>
        /// The spline is also evaluated at the absolute <paramref name="time"/> rather than at the
        /// fraction of the way between the two keys, so the result depends on where the clip's
        /// timeline happens to start.
        /// </para>
        /// Fixing either would change the numbers the tool produces, so both are left alone.
        /// </remarks>
        internal static float EstimateTangentBetween(Keyframe before, Keyframe after, float time)
        {
            float span = after.time - before.time;
            float outAngle = Mathf.Rad2Deg * Mathf.Atan(before.outTangent);
            float inAngle = Mathf.Rad2Deg * Mathf.Atan(after.inTangent);

            float p1 = before.value;
            float p2 = after.value;
            float p0 = before.value + Mathf.Tan(outAngle + 180f) * span;
            float p3 = after.value + Mathf.Tan(inAngle + 180f) * span;

            const float epsilon = 1E-05f;
            float at = CatmullRom(p0, p1, p2, p3, time);
            return (CatmullRom(p0, p1, p2, p3, time + epsilon) - at) / epsilon;
        }

        /// <summary>
        /// A curve through the given keyframes' times and values, with every tangent set to
        /// <paramref name="tangentMode"/>. Only the time and value of each keyframe are used.
        /// </summary>
        private static AnimationCurve CreateCurve(
            AnimationUtility.TangentMode tangentMode = AnimationUtility.TangentMode.Free,
            params Keyframe[] keyFrames)
        {
            return CreateCurve(tangentMode, keyFrames.Select(k => (k.time, k.value)).ToArray());
        }

        /// <summary>
        /// A curve through the given (time, value) pairs, with every tangent set to
        /// <paramref name="tangentMode"/>.
        /// </summary>
        internal static AnimationCurve CreateCurve(
            AnimationUtility.TangentMode tangentMode = AnimationUtility.TangentMode.Free,
            params (float time, float value)[] timeValuePairs)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < timeValuePairs.Length; i++)
            {
                (float time, float value) = timeValuePairs[i];
                curve.AddKey(time, value);

                // The right tangent of the previous key and the left tangent of this one meet in
                // the same segment, so both have to be set as each key is added.
                if (i > 0)
                {
                    AnimationUtility.SetKeyRightTangentMode(curve, i - 1, tangentMode);
                }

                AnimationUtility.SetKeyLeftTangentMode(curve, i, tangentMode);
            }

            return curve;
        }

        /// <summary>
        /// The clip's length, or -- for a clip with no curves at all, whose reported length is
        /// zero -- one frame at its frame rate.
        /// </summary>
        /// <remarks>
        /// An empty clip is still meant to occupy a state for one frame, and a zero length would
        /// make every duration computed from it collapse. A clip with a frame rate of zero falls
        /// back to 60fps.
        /// </remarks>
        internal static float GetEffectiveLength(this AnimationClip clip)
        {
            if (AnimationUtility.GetCurveBindings(clip).Any()
                || AnimationUtility.GetObjectReferenceCurveBindings(clip).Any())
            {
                return clip.length;
            }

            return clip.frameRate > 0f ? 1f / clip.frameRate : 1f / 60f;
        }

        /// <summary>
        /// The clip's own binding matching <paramref name="binding"/> by path, type and property
        /// name -- the same curve, but as the clip records it.
        /// </summary>
        /// <remarks>
        /// VENDOR BUG, transcribed as shipped: the two branches are swapped. A discrete binding is
        /// an object-reference curve and should be looked for among
        /// GetObjectReferenceCurveBindings, but this searches GetCurveBindings for it and
        /// GetObjectReferenceCurveBindings for everything else. In practice it therefore misses
        /// every binding it is asked about, since a float binding never appears in the
        /// object-reference list or vice versa.
        /// </remarks>
        internal static bool TryGetBinding(this AnimationClip clip, EditorCurveBinding binding,
            out EditorCurveBinding match)
        {
            IEnumerable<EditorCurveBinding> candidates = binding.isDiscreteCurve
                ? AnimationUtility.GetCurveBindings(clip)
                : AnimationUtility.GetObjectReferenceCurveBindings(clip);

            return candidates.TryGetBinding(binding, out match);
        }

        /// <summary>
        /// The first binding in the sequence matching <paramref name="binding"/> by path, type and
        /// property name. Unlike EditorCurveBinding's own equality this ignores isPPtrCurve and
        /// isDiscreteCurve, so a binding built by hand still matches one read off a clip.
        /// </summary>
        internal static bool TryGetBinding(this IEnumerable<EditorCurveBinding> bindings, EditorCurveBinding binding,
            out EditorCurveBinding match)
        {
            foreach (EditorCurveBinding candidate in bindings)
            {
                if (candidate.propertyName == binding.propertyName
                    && candidate.type == binding.type
                    && candidate.path == binding.path)
                {
                    match = candidate;
                    return true;
                }
            }

            match = default(EditorCurveBinding);
            return false;
        }
    }
}
