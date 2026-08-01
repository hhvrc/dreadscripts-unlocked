// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static InstantiateResolver -> IsSameOrSubclassOf, line 2659
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This would naturally sit in EditorUtils.Types.cs beside FindType and RequireType, but that partial
// is owned elsewhere in this pass and is not edited here. If the two are ever merged, this is the
// member to move.

using System;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Whether <paramref name="type"/> is <paramref name="other"/> or derives from it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Type.IsSubclassOf"/> alone answers false for a type compared against itself,
        /// which is not what the callers mean: the component filters ask "would an object of this
        /// type be accepted where <paramref name="other"/> is wanted?", and an exact match plainly
        /// qualifies. The extra equality test is the whole point of the method.
        /// </para>
        /// <para>
        /// This is not <see cref="Type.IsAssignableFrom"/> with the arguments reversed. Interfaces
        /// are not considered -- <see cref="Type.IsSubclassOf"/> only walks the base-class chain --
        /// so a type is never reported as matching an interface it implements. The callers compare
        /// against component classes, so the distinction has not mattered in practice, but a caller
        /// that passed an interface would silently get false for everything.
        /// </para>
        /// </remarks>
        internal static bool IsSameOrSubclassOf(this Type type, Type other)
        {
            if (type.IsSubclassOf(other))
            {
                return true;
            }

            return type == other;
        }
    }
}
