// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/FoldoutScope.cs
//
// Audit status: VERIFIED -- both fields, all five constructors (including which overload each
// chains to and the iscfg/label/style arguments it passes), Dispose and the implicit bool operator
// were diffed statement by statement against export/. The only rewrites are cosmetic: the
// `style ?? EditorStyles.foldout` coalesce replaces the equivalent `if (style == null)` assignment,
// and ILSpy's `bool num`/`bool flag` duplication of the expanded flag is collapsed.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Draws a foldout header and indents its body while expanded. Test the scope itself to decide
    /// whether to draw the body:
    /// <code>
    /// using (var foldout = new FoldoutScope(ref expanded, "Options"))
    /// {
    ///     if (foldout) { /* body */ }
    /// }
    /// </code>
    /// </summary>
    /// <remarks>
    /// The expanded flag is taken by reference so the foldout writes the user's click straight back
    /// to wherever the caller keeps that state.
    /// </remarks>
    internal class FoldoutScope : IDisposable
    {
        private readonly bool isExpanded;

        private readonly IndentedLayoutScope indentScope;

        /// <summary>Indents a body that is already known to be expanded, drawing no header.</summary>
        public FoldoutScope(bool isExpanded)
            : this(ref isExpanded, drawHeader: false, null)
        {
        }

        /// <inheritdoc cref="FoldoutScope(bool)"/>
        public FoldoutScope(ref bool isExpanded)
            : this(ref isExpanded, drawHeader: false, null)
        {
        }

        public FoldoutScope(ref bool isExpanded, string label, GUIStyle style = null)
            : this(ref isExpanded, drawHeader: true, new GUIContent(label), style)
        {
        }

        public FoldoutScope(ref bool isExpanded, GUIContent label, GUIStyle style = null)
            : this(ref isExpanded, drawHeader: true, label, style)
        {
        }

        public FoldoutScope(ref bool isExpanded, bool drawHeader, GUIContent label, GUIStyle style = null)
        {
            if (drawHeader)
            {
                isExpanded = EditorGUILayout.Foldout(isExpanded, label, style ?? EditorStyles.foldout);
            }

            this.isExpanded = isExpanded;

            if (isExpanded)
            {
                indentScope = new IndentedLayoutScope();
            }
        }

        public void Dispose()
        {
            if (isExpanded)
            {
                indentScope.Dispose();
            }
        }

        public static implicit operator bool(FoldoutScope scope)
        {
            return scope.isExpanded;
        }
    }
}
