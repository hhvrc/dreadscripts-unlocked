// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/GUILayoutUtils.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/SplitterGUIUtils.cs
//
// The two copies sit at identical line numbers; the numbers below are the ADOverhaul2022 ones, and
// the decompiled-name column gives the ADOverhaul2022 spelling first and the ControllerEditor
// spelling second where they differ.
//
//   GUILayoutUtils / SplitterGUIUtils        -> GUILayoutUtils, lines 9-164
//   defaultLineColor                         -> separatorColor
//   creatorMethod / splitterGUILayoutType    -> splitterGUILayoutType
//   splitterStateType                        -> unchanged
//   splitterStateConstructor                 -> unchanged
//   beginSplitMethod                         -> unchanged
//   endLayoutGroupMethod                     -> unchanged
//   RemoveIterator / SplitterGUILayoutType() -> SplitterGUILayoutType (property), line 24
//   ResolveIterator / SplitterStateType()    -> SplitterStateType (property), line 30
//   GetIterator / SplitterStateConstructor() -> SplitterStateConstructor (property), line 36
//   ExcludeIterator / BeginSplitMethod()     -> BeginSplitMethod (property), line 46
//   ConnectIterator / EndLayoutGroupMethod() -> EndLayoutGroupMethod (property), line 62
//   SearchIterator / CreateSplitterState     -> CreateSplitterState, line 71
//   LoginIterator / BeginHorizontalSplit     -> BeginHorizontalSplit, line 76
//   PatchIterator / BeginVerticalSplit       -> BeginVerticalSplit, line 81
//   CheckIterator / BeginSplit               -> BeginSplit, line 86
//   CallIterator / EndSplit                  -> EndSplit, line 97
//   RegisterIterator / DrawTitle(string)     -> TitleField(string), line 102
//   ChangeIterator / DrawTitle(GUIContent)   -> TitleField(GUIContent), line 107
//   StopIterator / DrawVerticalLine          -> DrawVerticalSeparator, line 114
//   PushIterator / DrawHorizontalLine        -> DrawHorizontalSeparator, line 129
//   PrepareIterator / DrawUnderline          -> DrawUnderline, line 144
//   VisitIterator / GetMethod                -> NOT PORTED, line 160 -- a one-line
//       type.GetMethod(name, flags) wrapper, inlined back into the two reflection properties that
//       were its only callers
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// An earlier wave also landed the ControllerEditor source as a standalone
// Editor/ControllerEditor/SplitterGUIUtils.cs, duplicating all ten members of this type including
// the five cached reflection statics -- two independent caches of the same internal Unity members,
// with the same names spelled differently (DrawTitle -> TitleField, DrawHorizontalLine ->
// DrawHorizontalSeparator, DrawVerticalLine -> DrawVerticalSeparator). That was an oversight rather
// than a deliberate twin: it had no call sites anywhere in the package, and
// Editor/Common/EditorGuiUtils.cs already recorded that the ControllerEditor layout companion is not
// ported because every member of it exists here. The duplicate file has been removed; this type is
// the single copy, and both products' call sites use it.
//
// Audit status: PARTIAL -- the member mapping above was built by comparing this file against both
// decompiled/ copies member for member; the bodies were not re-diffed statement by statement.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Layout helpers: resizable split panes, section titles and separator lines.
    /// </summary>
    /// <remarks>
    /// Unity's splitter (<c>UnityEditor.SplitterGUILayout</c> and <c>UnityEditor.SplitterState</c>)
    /// is internal to the editor assembly, so it is reached by reflection. Every reflected member is
    /// resolved once and cached; if a future Unity version renames or removes one, the corresponding
    /// property returns null and the split calls throw rather than silently drawing nothing.
    /// </remarks>
    internal static class GUILayoutUtils
    {
        private const string SplitterGUILayoutTypeName =
            "UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private const string SplitterStateTypeName =
            "UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private static readonly Color separatorColor = new Color(0.33f, 0.33f, 0.33f);

        private static Type splitterGUILayoutType;
        private static Type splitterStateType;
        private static ConstructorInfo splitterStateConstructor;
        private static MethodInfo beginSplitMethod;
        private static MethodInfo endLayoutGroupMethod;

        private static Type SplitterGUILayoutType =>
            splitterGUILayoutType ?? (splitterGUILayoutType = Type.GetType(SplitterGUILayoutTypeName));

        private static Type SplitterStateType =>
            splitterStateType ?? (splitterStateType = Type.GetType(SplitterStateTypeName));

        private static ConstructorInfo SplitterStateConstructor =>
            splitterStateConstructor ?? (splitterStateConstructor =
                SplitterStateType.GetConstructor(new[] { typeof(float[]) }));

        private static MethodInfo BeginSplitMethod =>
            beginSplitMethod ?? (beginSplitMethod = SplitterGUILayoutType.GetMethod(
                "BeginSplit",
                new[] { SplitterStateType, typeof(GUIStyle), typeof(bool), typeof(GUILayoutOption[]) }));

        private static MethodInfo EndLayoutGroupMethod =>
            endLayoutGroupMethod ?? (endLayoutGroupMethod = typeof(GUILayoutUtility).GetMethod(
                "EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic));

        /// <summary>
        /// Creates the opaque <c>SplitterState</c> to hand to <see cref="BeginHorizontalSplit"/> or
        /// <see cref="BeginVerticalSplit"/>. <paramref name="relativeSizes"/> gives one weight per
        /// pane; the state must be kept alive across frames, since it carries the sizes the user has
        /// dragged the splitters to.
        /// </summary>
        public static object CreateSplitterState(params float[] relativeSizes)
        {
            return SplitterStateConstructor.Invoke(new object[] { relativeSizes });
        }

        /// <summary>Begins a row of horizontally split panes. Pair with <see cref="EndSplit"/>.</summary>
        public static void BeginHorizontalSplit(object state, GUIStyle style = null, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, vertical: false, options);
        }

        /// <summary>Begins a column of vertically split panes. Pair with <see cref="EndSplit"/>.</summary>
        public static void BeginVerticalSplit(object state, GUIStyle style = null, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, vertical: true, options);
        }

        public static void BeginSplit(object state, GUIStyle style = null, bool vertical = true, params GUILayoutOption[] options)
        {
            BeginSplitMethod.Invoke(null, new object[] { state, style ?? GUIStyle.none, vertical, options });
        }

        public static void EndSplit()
        {
            EndLayoutGroupMethod.Invoke(null, null);
        }

        /// <summary>Draws a bold section title with a separator line beneath it.</summary>
        public static void TitleField(string title)
        {
            TitleField(new GUIContent(title));
        }

        /// <summary>Draws a bold section title with a separator line beneath it.</summary>
        public static void TitleField(GUIContent title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            DrawHorizontalSeparator();
            GUILayout.Space(7f);
        }

        /// <summary>
        /// Draws a vertical rule down the left edge of <paramref name="rect"/>, defaulting to the
        /// last laid-out rect.
        /// </summary>
        public static void DrawVerticalSeparator(Rect rect = default(Rect), Color color = default(Color))
        {
            if (color == default(Color))
            {
                color = separatorColor;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            rect.width = 1.5f;
            rect.x -= 2f;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>Reserves a thin layout row and draws a horizontal rule through it.</summary>
        public static void DrawHorizontalSeparator(Color color = default(Color))
        {
            if (color == default(Color))
            {
                color = separatorColor;
            }

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
            rect.height = 1.5f;
            rect.y += 1f;
            rect.x -= 2f;
            rect.width += 6f;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a rule just below <paramref name="rect"/> (defaulting to the last laid-out rect) and
        /// advances the layout past it — an underline for content that has already been drawn.
        /// </summary>
        public static void DrawUnderline(Rect rect = default(Rect), Color color = default(Color), float thickness = 1.5f)
        {
            if (color == default(Color))
            {
                color = separatorColor;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            rect.y += rect.height + thickness;
            rect.height = thickness;
            EditorGUI.DrawRect(rect, color);
            GUILayout.Space(thickness * 3f);
        }
    }
}
