// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/SplitterGUIUtils.cs
//
// The five [SpecialName] static methods in the decompiled source are property getters that ILSpy
// could not fold back into properties; they are restored as properties here:
//   SplitterGUILayoutType()   -> SplitterGUILayoutType
//   SplitterStateType()       -> SplitterStateType
//   SplitterStateConstructor() -> SplitterStateConstructor
//   BeginSplitMethod()        -> BeginSplitMethod
//   EndLayoutGroupMethod()    -> EndLayoutGroupMethod

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Draws the resizable split panes and the divider lines the tool windows are built from.
    /// </summary>
    /// <remarks>
    /// Unity has had draggable splitters since forever, but <c>UnityEditor.SplitterGUILayout</c> and
    /// its <c>SplitterState</c> are internal, so every call has to go through reflection. The lookups
    /// are cached in statics and resolved on first use rather than in a static constructor, so a Unity
    /// version that renamed one of them fails at the call that needs it instead of poisoning the whole
    /// class.
    /// <para>
    /// A splitter state is therefore passed around as <see cref="object"/>: the concrete type cannot
    /// be named at compile time. Create one with <see cref="CreateSplitterState"/> and keep it alive
    /// across repaints — it holds the pane sizes the user dragged to.
    /// </para>
    /// </remarks>
    internal static class SplitterGUIUtils
    {
        /// <summary>Mid-grey, chosen to read as a separator against both editor skins.</summary>
        private static readonly Color defaultLineColor = new Color(0.33f, 0.33f, 0.33f);

        public static Type splitterGUILayoutType;

        private static Type splitterStateType;

        private static ConstructorInfo splitterStateConstructor;

        private static MethodInfo beginSplitMethod;

        private static MethodInfo endLayoutGroupMethod;

        public static Type SplitterGUILayoutType
        {
            get
            {
                if (splitterGUILayoutType == null)
                {
                    splitterGUILayoutType = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                }

                return splitterGUILayoutType;
            }
        }

        public static Type SplitterStateType
        {
            get
            {
                if (splitterStateType == null)
                {
                    splitterStateType = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                }

                return splitterStateType;
            }
        }

        /// <summary>The <c>SplitterState(float[] relativeSizes)</c> constructor.</summary>
        public static ConstructorInfo SplitterStateConstructor
        {
            get
            {
                if (splitterStateConstructor == null)
                {
                    splitterStateConstructor = SplitterStateType.GetConstructor(new Type[] { typeof(float[]) });
                }

                return splitterStateConstructor;
            }
        }

        /// <summary>
        /// <c>SplitterGUILayout.BeginSplit(SplitterState, GUIStyle, bool vertical, GUILayoutOption[])</c>.
        /// </summary>
        public static MethodInfo BeginSplitMethod
        {
            get
            {
                if (beginSplitMethod == null)
                {
                    beginSplitMethod = SplitterGUILayoutType.GetMethod("BeginSplit", new Type[]
                    {
                        SplitterStateType,
                        typeof(GUIStyle),
                        typeof(bool),
                        typeof(GUILayoutOption[])
                    });
                }

                return beginSplitMethod;
            }
        }

        /// <summary>
        /// <c>GUILayoutUtility.EndLayoutGroup()</c>, which is what closes a split group.
        /// </summary>
        /// <remarks>
        /// <c>SplitterGUILayout</c> has no public End method to pair with BeginSplit; it ends the
        /// group through this non-public utility, so the port does the same.
        /// </remarks>
        public static MethodInfo EndLayoutGroupMethod
        {
            get
            {
                if (endLayoutGroupMethod == null)
                {
                    endLayoutGroupMethod = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
                }

                return endLayoutGroupMethod;
            }
        }

        /// <summary>
        /// Creates a splitter state whose panes start at the given relative sizes. The caller owns it
        /// and must keep it between repaints, since it is where the dragged sizes live.
        /// </summary>
        public static object CreateSplitterState(params float[] relativeSizes)
        {
            return SplitterStateConstructor.Invoke(new object[] { relativeSizes });
        }

        /// <summary>Begins a row of panes with draggable vertical dividers.</summary>
        public static void BeginHorizontalSplit(object state, GUIStyle style = null, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, vertical: false, options);
        }

        /// <summary>Begins a column of panes with draggable horizontal dividers.</summary>
        public static void BeginVerticalSplit(object state, GUIStyle style = null, params GUILayoutOption[] options)
        {
            BeginSplit(state, style, vertical: true, options);
        }

        public static void BeginSplit(object state, GUIStyle style = null, bool vertical = true, params GUILayoutOption[] options)
        {
            BeginSplitMethod.Invoke(null, new object[]
            {
                state,
                style ?? GUIStyle.none,
                vertical,
                options
            });
        }

        public static void EndSplit()
        {
            EndLayoutGroupMethod.Invoke(null, null);
        }

        public static void DrawTitle(string text)
        {
            DrawTitle(new GUIContent(text));
        }

        /// <summary>A bold section heading, underlined and followed by breathing room.</summary>
        public static void DrawTitle(GUIContent content)
        {
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
            DrawHorizontalLine();
            GUILayout.Space(7f);
        }

        /// <summary>
        /// Draws a thin vertical rule just left of <paramref name="rect"/>, defaulting to the last
        /// laid-out rect.
        /// </summary>
        /// <remarks>
        /// Both parameters use their default value as "not supplied", so a fully transparent black
        /// colour or a zero rect cannot be requested explicitly — neither would be visible anyway.
        /// </remarks>
        public static void DrawVerticalLine(Rect rect = default(Rect), Color color = default(Color))
        {
            if (color == default(Color))
            {
                color = defaultLineColor;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            rect.width = 1.5f;
            rect.x -= 2f;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a thin horizontal rule that consumes a line of layout space.
        /// </summary>
        /// <remarks>
        /// The reserved row is 3.5 units tall but only 1.5 of it is painted, which leaves the rule
        /// visually centred in its own gap. It is also pulled 2 units left and made 6 wider so that it
        /// overhangs the inset of the surrounding box rather than stopping short of its edges.
        /// </remarks>
        public static void DrawHorizontalLine(Color color = default(Color))
        {
            if (color == default(Color))
            {
                color = defaultLineColor;
            }

            float thickness = 1.5f;
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(3.5f));
            rect.height = thickness;
            rect.y += 1f;
            rect.x -= 2f;
            rect.width += 6f;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Underlines <paramref name="rect"/> — by default whatever was drawn last — and reserves the
        /// space the line occupies below it.
        /// </summary>
        public static void DrawUnderline(Rect rect = default(Rect), Color color = default(Color), float thickness = 1.5f)
        {
            if (color == default(Color))
            {
                color = defaultLineColor;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            rect.y += rect.height + thickness;
            rect.height = thickness;
            EditorGUI.DrawRect(rect, color);

            // The underline is drawn outside the layout system, so claim the gap above it, the line
            // itself and a matching gap below.
            GUILayout.Space(thickness * 3f);
        }
    }
}
