// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CreateRules     -> ForEach(IEnumerable, Action<T>), line 5142
//   static IncludeRules    -> LogEach,      line 5096
//   static RunRules        -> Distinct,     line 5104
//   static CloneRules      -> WhereNotNull, line 5117
//   static LoginRules      -> Except,       line 5122
//   static ReflectRules    -> Cast,         line 5134
//   static NewRules        -> GetValueOrDefault, line 5150
//   static GetRules        -> IsNullOrEmpty(T[]),  line 5070
//   static CalcRules       -> IsNullOrEmpty(IList), line 5083
//   static CheckRules      -> Args,         line 5322
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Small IEnumerable<T> helpers, all of them unique to this partial.
//
// Three members this file was ported with are not here, because EditorUtils.Behaviours.cs had
// already ported the same decompiled members under different names and that port is the one kept:
// the typed ForEach (InvokeResolver, 2555) lives there beside the AnimatorState helpers that are
// its only callers, and FindIndex/TryFindIndex (2563/2580) are its IndexOf/TryGetIndex. Only the
// untyped ForEach overload, which has no counterpart there, remains here.
//
// Distinct and Except are the vendor's own -- they take an equality *delegate* rather than an
// IEqualityComparer, which LINQ's overloads of those names cannot, and which is the whole point:
// the tool compares animator objects by content, and writing a comparer type per comparison would
// have been unbearable. Both are O(n*m) as a result, since without GetHashCode there is nothing to
// bucket on. Distinct additionally does not overload LINQ's: its parameter list differs, so
// `seq.Distinct()` still resolves to LINQ's.
//
// Args exists purely so a `params` call site can be written without naming the array type -- e.g.
// Args(a, b, c) in place of new[] { a, b, c } where inference would otherwise fail.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// <see cref="ForEach{T}(IEnumerable{T}, Action{T})"/> over an untyped sequence, casting
        /// each element to <typeparamref name="T"/>. Throws on the first element that is not one.
        /// </summary>
        /// <remarks>
        /// The typed overload this complements lives in EditorUtils.Behaviours.cs, beside the
        /// AnimatorState helpers that are its only callers.
        /// </remarks>
        internal static void ForEach<T>(this IEnumerable source, Action<T> action)
        {
            foreach (object item in source)
            {
                action((T)item);
            }
        }

        /// <summary>
        /// Logs one line per element, produced by <paramref name="describe"/>. A debugging aid the
        /// vendor left in.
        /// </summary>
        internal static void LogEach<T>(this IEnumerable<T> source, Func<T, string> describe)
        {
            foreach (T item in source)
            {
                Debug.Log(describe(item));
            }
        }

        /// <summary>
        /// The sequence with later elements dropped when <paramref name="areEqual"/> says they
        /// match one already yielded.
        /// </summary>
        /// <remarks>
        /// Compares against every element kept so far, so this is quadratic; it is meant for the
        /// short lists the tool builds, not for bulk data. Deferred, like LINQ's own operators.
        /// </remarks>
        internal static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> areEqual)
        {
            HashSet<T> seen = new HashSet<T>();
            foreach (T element in source)
            {
                if (!seen.Any(seenElement => areEqual(element, seenElement)))
                {
                    seen.Add(element);
                    yield return element;
                }
            }
        }

        /// <summary>The sequence without its null elements.</summary>
        internal static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => x != null);
        }

        /// <summary>
        /// The elements of <paramref name="source"/> that <paramref name="areEqual"/> does not
        /// match to anything in <paramref name="other"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="other"/> is materialised once up front, so it is safe to pass a
        /// deferred query; <paramref name="source"/> is not, and is walked lazily.
        /// </remarks>
        internal static IEnumerable<T> Except<T>(this IEnumerable<T> source, IEnumerable<T> other,
            Func<T, T, bool> areEqual)
        {
            T[] otherItems = (other as T[]) ?? other.ToArray();
            foreach (T element in source)
            {
                if (!otherItems.Any(otherElement => areEqual(element, otherElement)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// The remainder of a non-generic enumerator, cast to <typeparamref name="T"/>. Takes the
        /// enumerator rather than the enumerable, so a partly-consumed one continues from where it
        /// is -- which is how Unity's older reflection APIs hand back their results.
        /// </summary>
        internal static IEnumerable<T> Cast<T>(this IEnumerator source)
        {
            while (source.MoveNext())
            {
                yield return (T)source.Current;
            }
        }

        /// <summary>
        /// The value for <paramref name="key"/>, or <paramref name="fallback"/> if the dictionary
        /// has no such key.
        /// </summary>
        internal static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
            TValue fallback)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : fallback;
        }

        /// <summary>
        /// Whether the array is null or has no elements -- and, with
        /// <paramref name="nullElementsCountAsEmpty"/>, whether every element it does have is null.
        /// </summary>
        internal static bool IsNullOrEmpty<T>(this T[] array, bool nullElementsCountAsEmpty = false)
        {
            if (array == null || array.Length == 0)
            {
                return true;
            }

            return nullElementsCountAsEmpty && array.All(e => e == null);
        }

        /// <summary>
        /// <see cref="IsNullOrEmpty{T}(T[], bool)"/> for any IList, including the non-generic ones
        /// ReorderableList hands out.
        /// </summary>
        internal static bool IsNullOrEmpty(this IList list, bool nullElementsCountAsEmpty = false)
        {
            if (list == null || list.Count == 0)
            {
                return true;
            }

            return nullElementsCountAsEmpty && list.Cast<object>().All(e => e == null);
        }

        /// <summary>Its arguments as an array -- a spelling aid for `params` call sites.</summary>
        internal static T[] Args<T>(params T[] items)
        {
            return items;
        }
    }
}
