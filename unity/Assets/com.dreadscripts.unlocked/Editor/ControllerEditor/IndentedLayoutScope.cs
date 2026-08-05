// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/IndentedLayoutScope.cs
//
// NOT PORTED
// Two of the four decompiled constructors. Both are unreachable convenience overloads with no call
// site anywhere in either shipped assembly:
//   IndentedLayoutScope(bool)  -- ignores its argument entirely and chains to this(10f, 10f), i.e.
//     it is indistinguishable from the parameterless constructor that is ported.
//   IndentedLayoutScope(float) -- chains to this(10f, arg), setting only the right padding. The
//     ported (float, float) constructor covers it.
// Neither carries behaviour the ported pair cannot express, so they were dropped rather than
// reproduced. Restore them if a call site ever turns up.
//
// NOTES
// The private Spacer(float) helper is not a decompiled member. The fixed-width column it draws is
// written out twice in the decompiled source, once in the constructor and once in Dispose; it is
// factored out here. The DefaultPadding constant is likewise a name for the literal 10f that the
// decompiled source repeats.
//
// Audit status: VERIFIED -- the field, the parameterless and (float, float) constructors and
// Dispose were diffed statement by statement against export/, including the GUILayout begin/end
// nesting order and the rightPadding != 0f guard. The two dropped constructors are recorded above.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Indents everything drawn inside the scope by a fixed number of pixels on each side.
    /// </summary>
    /// <remarks>
    /// <see cref="UnityEditor.EditorGUI.indentLevel"/> only shifts prefix labels, which leaves
    /// controls that draw their own rects unindented. Wrapping the block in a horizontal group with
    /// a fixed-width spacer on each side indents the content itself, whatever it draws.
    /// </remarks>
    internal sealed class IndentedLayoutScope : IDisposable
    {
        private const float DefaultPadding = 10f;

        private readonly float rightPadding;

        internal IndentedLayoutScope()
            : this(DefaultPadding, DefaultPadding)
        {
        }

        internal IndentedLayoutScope(float leftPadding, float rightPadding)
        {
            this.rightPadding = rightPadding;

            GUILayout.BeginHorizontal();
            Spacer(leftPadding);
            GUILayout.BeginVertical();
        }

        public void Dispose()
        {
            GUILayout.EndVertical();

            if (rightPadding != 0f)
            {
                Spacer(rightPadding);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// A fixed-width column. The flexible space inside it is what stops the group collapsing to
        /// zero width when there is nothing else to give it a size.
        /// </summary>
        private static void Spacer(float width)
        {
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
