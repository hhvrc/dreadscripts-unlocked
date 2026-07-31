// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ExcludeQueue -> ToggleButton(bool, GUIContent, ...), line 5763
//   static FindQueue    -> ToggleButton(bool, GUIContent, ...) with no style, line 5758
//   static AddQueue     -> ToggleButton(bool, string, ...),     line 5748
//   static InvokeQueue  -> ToggleButton(bool, string, ...) with no style, line 5753
//   static RestartQueue -> Button(GUIContent, ...),             line 5732
//   static InsertQueue  -> Button(GUIContent, ...) with no style, line 5727
//   static CountQueue   -> Button(string, ...),                 line 5717
//   static DisableQueue -> Button(string, ...) with no style,    line 5722
//   static QueryQueue   -> Button(Rect, GUIContent, ...),       line 5737
//   static CancelQueue  -> Button(Rect, string, ...),           line 5712
//   static CallQueue    -> IconButton,                          line 5697
//   static VisitQueue   -> ToggleButtonChanged(bool, GUIContent, ...), line 5779
//   static InitQueue    -> ToggleButtonChanged(bool, string, ...),     line 5774
//   static DefineQueue  -> ClickArea,                           line 5790
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- the region is byte-identical between the two snapshots
// and every statement below was transcribed from it.
//
// The decompiled class carries a separate overload for each combination of (content type, style
// present), because the obfuscator had already split them; the optional parameters below collapse
// each pair back into one method, so four names map onto two here.
//
// The Buttons region is now complete: the layout buttons, the Rect-based buttons, the icon button,
// the "did the toggle change" variants and the bare click area are all ported.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A layout button that returns true on the frame it is clicked, and shows the link cursor
        /// while hovered.
        /// </summary>
        internal static bool Button(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="Button(string, GUIStyle, GUILayoutOption[])"/>
        internal static bool Button(GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, content, style, options);
        }

        /// <summary>
        /// A layout button that stays visibly held down while <paramref name="value"/> is true.
        /// Returns the toggle's new value, so it reads true for as long as it is on rather than
        /// only on the click.
        /// </summary>
        internal static bool ToggleButton(bool value, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(value, new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="ToggleButton(bool, string, GUIStyle, GUILayoutOption[])"/>
        internal static bool ToggleButton(bool value, GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                style = GUI.skin.button;
            }

            // A toggle drawn with the button style, rather than GUILayout.Button, so that the same
            // method covers both the plain and the held-down case.
            bool result = GUILayout.Toggle(value, content, style, options);
            AddLinkCursor();
            return result;
        }

        /// <summary>
        /// A button placed at an explicit <paramref name="rect"/> rather than by the layout system,
        /// for use inside manually laid out rows.
        /// </summary>
        internal static bool Button(Rect rect, string text, GUIStyle style = null)
        {
            return Button(rect, new GUIContent(text), style);
        }

        /// <inheritdoc cref="Button(Rect, string, GUIStyle)"/>
        internal static bool Button(Rect rect, GUIContent content, GUIStyle style = null)
        {
            if (style == null)
            {
                style = GUI.skin.button;
            }

            bool result = GUI.Button(rect, content, style);
            AddLinkCursor(rect);
            return result;
        }

        /// <summary>
        /// A square button carrying only an icon, drawn with no button chrome.
        /// </summary>
        /// <param name="width">Width in pixels; -1 means one line height, matching the row it sits in.</param>
        /// <param name="height">Height in pixels; -1 means one line height.</param>
        internal static bool IconButton(GUIContent content, float width = -1f, float height = -1f)
        {
            if (width == -1f)
            {
                width = EditorGUIUtility.singleLineHeight;
            }

            if (height == -1f)
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            bool result = GUILayout.Button(content, styles.iconButton, GUILayout.Width(width), GUILayout.Height(height));
            AddLinkCursor();
            return result;
        }

        /// <summary>
        /// A toggle drawn as a button that reports whether the user just changed it, rather than
        /// what it now is. Lets a caller react to the click without a surrounding change check; the
        /// caller is still responsible for storing the new value.
        /// </summary>
        internal static bool ToggleButtonChanged(bool value, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButtonChanged(value, new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="ToggleButtonChanged(bool, string, GUIStyle, GUILayoutOption[])"/>
        internal static bool ToggleButtonChanged(bool value, GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                style = GUI.skin.button;
            }

            bool newValue = GUILayout.Toggle(value, content, style, options);
            AddLinkCursor();
            return value != newValue;
        }

        /// <summary>
        /// Reports whether a left mouse press landed in <paramref name="rect"/>, and shows the link
        /// cursor over it. For making something that is not a control clickable -- a drawn texture,
        /// a label -- without giving it button chrome.
        /// </summary>
        /// <param name="rect">
        /// The area to test; the default means the rect of the control just drawn.
        /// </param>
        /// <remarks>
        /// The event is deliberately not consumed, so an enclosing control still sees the same
        /// press. Callers that need exclusive ownership of the click must call
        /// <see cref="Event.Use"/> themselves.
        /// </remarks>
        internal static bool ClickArea(Rect rect = default(Rect))
        {
            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            AddLinkCursor(rect);

            Event current = Event.current;
            if (current.type != EventType.MouseDown || current.button != 0)
            {
                return false;
            }

            return rect.Contains(current.mousePosition);
        }
    }
}
