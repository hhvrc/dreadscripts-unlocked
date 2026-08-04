// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CancelPredicate -> GetFlags, line 3129
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export

using System;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The declared values of <typeparamref name="T"/> that <paramref name="value"/> has set.
        /// </summary>
        /// <remarks>
        /// Uses Enum.HasFlag, so a composite member (one whose value is the OR of others) is
        /// yielded whenever all of its bits are set, and the zero member is yielded always. A
        /// caller that wants only the single-bit members has to filter for them itself -- which is
        /// what the anchor picker does.
        /// <para>
        /// Deferred, and it re-reads the enum's values on each enumeration.
        /// </para>
        /// </remarks>
        internal static IEnumerable<T> GetFlags<T>(this T value) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(flag => value.HasFlag(flag));
        }
    }
}
