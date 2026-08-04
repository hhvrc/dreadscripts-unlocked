// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static SetStatus       -> Toggle,   line 2732
//   static CustomizeStatus -> As<T>,    line 2766
//   static ConcatStatus    -> GetFlags, line 2780
//   static MapStatus       -> ForEach,  line 2790
//   static FillStatus      -> And,      line 2798
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above.
//
// These five are the general-purpose extensions in the outer class body -- the ones that operate on
// a BCL type rather than on anything Unity or VRChat specific. They are grouped by that rather than
// by a shared caller; nothing in the shipped build uses more than one of them at a time.
//
// GetFlags is written out as a plain HasFlag test. The decompiler rendered it as a ref local plus a
// boxed object (`ref T reference = ref task; object flag = value; reference.HasFlag((Enum)flag)`),
// which is how a `where T : Enum` HasFlag call comes back through ILSpy, not something the source
// could have said.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>Flips <paramref name="value"/> in place and returns its new state.</summary>
        /// <remarks>
        /// Lets a toggle be flipped and acted on in one expression -- <c>if (flag.Toggle())</c> --
        /// which is why it takes a <c>ref</c> rather than returning a new value.
        /// </remarks>
        internal static bool Toggle(this ref bool value)
        {
            return value = !value;
        }

        /// <summary>
        /// Reinterprets <paramref name="obj"/> as <typeparamref name="T"/>, reaching through a
        /// <see cref="GameObject"/> when <typeparamref name="T"/> is a component.
        /// </summary>
        /// <returns>The object as <typeparamref name="T"/>, or null.</returns>
        /// <remarks>
        /// Both the object picker and a drag payload hand over a GameObject rather than a component,
        /// so a caller asking for a component has to fetch it off the object. The test is
        /// <see cref="Type.IsSubclassOf(Type)"/>, which is false for <see cref="Component"/> itself,
        /// so a <c>T</c> of exactly <c>Component</c> takes the plain cast path. Shipped behaviour,
        /// preserved as-is.
        /// </remarks>
        internal static T As<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                return obj as T;
            }

            GameObject gameObject = obj as GameObject;
            return (gameObject != null) ? gameObject.GetComponent<T>() : null;
        }

        /// <summary>
        /// Every declared value of <typeparamref name="T"/> that <paramref name="flags"/> has set.
        /// </summary>
        /// <remarks>
        /// Enumerates the enum's declared members, so a composite member such as <c>All</c> is
        /// yielded alongside the single bits it covers, and a bit with no name is not yielded at all.
        /// Callers that want single bits only filter for a power of two themselves -- the anchor grid
        /// in ADOEditorUtility.AnchorPicker.cs is the one place that does.
        /// </remarks>
        internal static IEnumerable<T> GetFlags<T>(this T flags) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(value => flags.HasFlag(value));
        }

        /// <summary>Runs <paramref name="action"/> for each element, eagerly.</summary>
        /// <remarks>
        /// Unlike the LINQ operators this is not deferred: it enumerates on call. There is no
        /// <c>Select</c>-style counterpart in the shipped build.
        /// </remarks>
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// A predicate that holds only when both <paramref name="predicate"/> and
        /// <paramref name="other"/> hold.
        /// </summary>
        /// <remarks>
        /// For building up a filter across call sites that each know about one condition -- the
        /// drag-and-drop and object-picker helpers both take a single <c>Func&lt;T, bool&gt;</c>.
        /// Neither operand is null-checked, so both must be non-null.
        /// </remarks>
        public static Func<T, bool> And<T>(this Func<T, bool> predicate, Func<T, bool> other)
        {
            return arg => predicate(arg) && other(arg);
        }
    }
}
