// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static InvokeResolver  -> ForEach,      line 2555
//   static FindResolver    -> FindIndex,    line 2563
//   static ExcludeResolver -> TryFindIndex, line 2580
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Small IEnumerable<T> helpers. FindIndex enumerates and returns the zero-based position of the
// first element that both is non-null and satisfies the predicate, or -1; TryFindIndex is the
// bool-plus-out wrapper around it.

using System;
using System.Collections.Generic;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Runs <paramref name="action"/> against every element of the sequence.</summary>
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Zero-based index of the first non-null element matching <paramref name="predicate"/>,
        /// or -1 if none. Unlike <c>List{T}.FindIndex</c> this works on any sequence and skips
        /// null elements.
        /// </summary>
        internal static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = -1;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    index++;
                    if (enumerator.Current != null && predicate(enumerator.Current))
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// <see cref="FindIndex{T}"/> as a try-pattern: returns whether a match was found and
        /// reports its index (or -1) in <paramref name="index"/>.
        /// </summary>
        internal static bool TryFindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate, out int index)
        {
            index = source.FindIndex(predicate);
            return index != -1;
        }
    }
}
