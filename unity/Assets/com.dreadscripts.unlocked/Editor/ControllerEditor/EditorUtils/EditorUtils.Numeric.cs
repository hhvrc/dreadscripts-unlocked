// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static MapResolver       -> IsBetween(this float, float, float),   line 2697
//   static ValidateResolver  -> IsBetween(this int, int, int),         line 2706
//   static CustomizeResolver -> IsValidIndex(this int, IList),         line 2715
//   static RateResolver      -> IsValidIndex(this int, Array),         line 2724
//   static DestroyResolver   -> IsOutside(this float, float, float),   line 2733
//   static GetResolver       -> IsOutside(this int, int, int),         line 2742
//   static CalcResolver      -> RoundToNearest(this float, int),       line 2751
//   static IncludeResolver   -> RoundToNearest(this int, int),         line 2756
//   static AwakeResolver     -> Flip(this ref bool),                   line 2668
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Numeric range/rounding helpers plus the ref-bool flip. IsBetween is inclusive at both ends;
// IsOutside is its complement (value < min || value > max), kept as a separate method because the
// original did. RoundToNearest(float, int) rounds to the nearest int first, then to the nearest
// multiple of the step -- matching the original's CalcResolver -> IncludeResolver chain.

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
