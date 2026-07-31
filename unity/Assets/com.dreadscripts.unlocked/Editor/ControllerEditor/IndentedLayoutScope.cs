// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/IndentedLayoutScope.cs

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
