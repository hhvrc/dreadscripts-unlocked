// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//
//   static Flip           -> Flip(this ref bool),                   line 2668
//   static IsBetween      -> IsBetween(this float, float, float),   line 2697
//   static IsBetween      -> IsBetween(this int, int, int),         line 2706
//   static IsValidIndex   -> IsValidIndex(this int, IList),         line 2715
//   static IsValidIndex   -> IsValidIndex(this int, Array),         line 2724
//   static IsOutside      -> IsOutside(this float, float, float),   line 2733
//   static IsOutside      -> IsOutside(this int, int, int),         line 2742
//   static RoundToNearest -> RoundToNearest(this float, int),       line 2751
//   static RoundToNearest -> RoundToNearest(this int, int),         line 2756
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// Numeric range/rounding helpers plus the ref-bool flip. IsBetween is inclusive at both ends;
// IsOutside is its complement (value < min || value > max), kept as a separate method because the
// original did. RoundToNearest(float, int) rounds to the nearest int first and then hands off to
// RoundToNearest(int, int) for the multiple-of-step step, matching the chain the original had.
//
// The 561e9ec re-snapshot deobfuscated these nine members, so the decompiled column above now reads
// the same as the ported column. Their pre-561e9ec obfuscated spellings, in the order listed, were
// AwakeResolver, MapResolver, ValidateResolver, CustomizeResolver, RateResolver, DestroyResolver,
// GetResolver, CalcResolver, IncludeResolver.
//
// Audit status: PARTIAL -- all nine line numbers were checked against reverse-engineering/export/ControllerEditor/
// DreadScripts/ControllerEditor/EditorUtils.cs and each lands on the signature named; the bodies
// were not re-diffed, which is why this is PARTIAL rather than VERIFIED.

using System;
using System.Collections;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>True if <paramref name="value"/> lies within [<paramref name="min"/>, <paramref name="max"/>] inclusive.</summary>
        internal static bool IsBetween(this float value, float min, float max)
        {
            if (value < min)
            {
                return false;
            }

            return value <= max;
        }

        /// <inheritdoc cref="IsBetween(float, float, float)"/>
        internal static bool IsBetween(this int value, int min, int max)
        {
            if (value < min)
            {
                return false;
            }

            return value <= max;
        }

        /// <summary>True if <paramref name="index"/> is a valid index into <paramref name="list"/>.</summary>
        internal static bool IsValidIndex(this int index, IList list)
        {
            if (index >= 0)
            {
                return index < list.Count;
            }

            return false;
        }

        /// <summary>True if <paramref name="index"/> is a valid index into <paramref name="array"/>.</summary>
        internal static bool IsValidIndex(this int index, Array array)
        {
            if (index >= 0)
            {
                return index < array.Length;
            }

            return false;
        }

        /// <summary>Complement of <see cref="IsBetween(float, float, float)"/>: value below min or above max.</summary>
        internal static bool IsOutside(this float value, float min, float max)
        {
            if (!(value >= min))
            {
                return true;
            }

            return value > max;
        }

        /// <summary>Complement of <see cref="IsBetween(int, int, int)"/>: value below min or above max.</summary>
        internal static bool IsOutside(this int value, int min, int max)
        {
            if (value < min)
            {
                return true;
            }

            return value > max;
        }

        /// <summary>Rounds to the nearest int, then to the nearest multiple of <paramref name="step"/>.</summary>
        internal static int RoundToNearest(this float value, int step)
        {
            return Mathf.RoundToInt(value).RoundToNearest(step);
        }

        /// <summary>Rounds <paramref name="value"/> to the nearest multiple of <paramref name="step"/>.</summary>
        internal static int RoundToNearest(this int value, int step)
        {
            return Mathf.RoundToInt((float)value / (float)step) * step;
        }

        /// <summary>Inverts the boolean in place and returns the new value.</summary>
        internal static bool Flip(this ref bool value)
        {
            return value = !value;
        }
    }
}
