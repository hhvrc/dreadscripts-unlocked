// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ColorHashUtils.cs
//
// Audit status: VERIFIED -- ColorFromString is the only member; every statement, the 73244475
// multiplier, all three mask/shift expressions and the 0.7/0.3 and 0.8/0.2 constants were diffed
// against export/ and match. Only the redundant (float) casts ILSpy emits are dropped.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static class ColorHashUtils
    {
        /// <summary>
        /// Derives a stable, readable colour from a string, so that the same layer or parameter name
        /// is always tinted the same way without anyone having to choose a palette.
        /// </summary>
        /// <remarks>
        /// Hue comes from the low byte of the mixed hash and so spans the full circle. Saturation and
        /// value are deliberately confined to the top of their ranges (0.7-1.0 and 0.8-1.0) — an
        /// unconstrained hash would produce dark and washed-out colours that are unreadable as editor
        /// text.
        /// </remarks>
        internal static Color ColorFromString(string value)
        {
            int hash = value.GetHashCode();

            // Fold the high half onto the low half, then scatter, so that names sharing a prefix do
            // not land on neighbouring hues.
            hash = (hash >> 16) ^ (hash & 0xFFFF);
            hash *= 73244475;

            float h = (hash & 0xFF) / 255f;
            float s = 0.7f + ((hash >> 8) & 0x7F) / 255f * 0.3f;
            float v = 0.8f + ((hash >> 16) & 0x7F) / 255f * 0.2f;

            return Color.HSVToRGB(h, s, v);
        }
    }
}
