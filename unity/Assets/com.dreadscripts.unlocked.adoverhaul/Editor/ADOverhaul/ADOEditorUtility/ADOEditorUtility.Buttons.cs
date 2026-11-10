// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static PrepareStatus   -> ToggleButton(bool, GUIContent, GUIStyle, ...), line 3176
//   static ChangeStatus    -> ToggleButton(bool, string, GUIStyle, ...),     line 3161
//   static StopStatus      -> ToggleButton(bool, string, ...) with no style, line 3166
//   static PushStatus      -> ToggleButton(bool, GUIContent, ...) no style,  line 3171
//   static CallStatus      -> Button(GUIContent, GUIStyle, ...),             line 3145
//   static CheckStatus     -> Button(GUIContent, ...) with no style,         line 3140
//   static LoginStatus     -> Button(string, GUIStyle, ...),                 line 3130
//   static PatchStatus     -> Button(string, ...) with no style,             line 3135
//   static RegisterStatus  -> Button(Rect, GUIContent, GUIStyle),            line 3150
//   static SearchStatus    -> Button(Rect, GUIContent) with no style,        line 3125
//   static ForgotStatus    -> Button(Rect, string, GUIStyle),                line 3115
//   static UpdateStatus    -> Button(Rect, string) with no style,            line 3120
//   static ListStatus      -> IconButton,                                    line 3097
//   static ReadStatus      -> ClickArea,                                     line 3187
//   static ViewStatus      -> LinkLabel(GUIContent, Color?),                 line 3062
//   static InterruptStatus -> LinkLabel(string, Color?),                      line 3057
//   static PostStatus      -> MarkAsLink,                                    line 3079
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above and cross-checked against the ControllerEditor twin named next.
//
// The obfuscator had already split each button into one overload per (content type, style present)
// combination, exactly as it did in ControllerEditor. The optional parameters below collapse each
// pair back into one method, so twelve export names map onto five here. This mirrors what
// EditorUtils.Buttons.cs did for the same shapes.
//
// DEOBF-BUG(resolved): ListStatus's second default read
//     if (field == -1f) { while (true) { field = EditorGUIUtility.singleLineHeight; } }
// -- an unconditional hang where the first default (width, immediately above it) is a plain
// assignment. ControllerEditor's twin renders the same line as a plain assignment, and the shipped
// tool visibly draws icon buttons, so the loop is a decompiler artefact and the body runs once.
//
// Shared with ControllerEditor: EditorUtils.Buttons.cs holds the same Button / ToggleButton /
// IconButton / ClickArea family, statement for statement. Two differences, both real:
//   - ControllerEditor also has ToggleButtonChanged (the "did it change" variant); this build
//     does not ship one.
//   - This build has LinkLabel and MarkAsLink, which ControllerEditor keeps elsewhere.
// Deliberately NOT consolidated, on the same basis as ADOEditorUtility.Colors.cs.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
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
        /// Returns the toggle's new value, so it reads true for as long as it is on rather than only
        /// on the click.
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

            bool result = GUILayout.Toggle(value, content, style, options);
            AddLinkCursor();
            return result;
        }

        /// <summary>A button drawn into an explicit rect rather than the layout flow.</summary>
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

            // No rect is passed, so this takes the last layout rect rather than the rect just drawn
            // into. Reproduced as shipped: ControllerEditor's twin does the same, and correcting it
            // would change where the cursor appears.
            AddLinkCursor();
            return result;
        }

        /// <summary>A square button carrying only an icon, drawn with no button chrome.</summary>
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
        /// Reports whether a left mouse press landed in <paramref name="rect"/>, and shows the link
        /// cursor over it. For making something that is not a control clickable -- a drawn texture,
        /// a label -- without giving it button chrome.
        /// </summary>
        /// <param name="rect">The area to test; the default means the rect of the control just drawn.</param>
        /// <remarks>
        /// The event is deliberately not consumed, so an enclosing control still sees the same press.
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

        /// <summary>
        /// A label that reads and behaves as a hyperlink: tinted, underlined on hover, link cursor,
        /// and true on the frame it is clicked.
        /// </summary>
        /// <param name="color">Link tint; the default is the same pale blue the styles table uses.</param>
        internal static bool LinkLabel(string text, Color? color = null)
        {
            return LinkLabel(new GUIContent(text), color);
        }

        /// <inheritdoc cref="LinkLabel(string, Color?)"/>
        internal static bool LinkLabel(GUIContent content, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = new Color(0.3f, 0.7f, 1f);
            }

            // The button is drawn with the toggle-named label style, so it takes hover feedback from
            // IMGUI while keeping a label's chrome. Clearing the background colour removes the
            // button's own box; the foreground colour tints the text.
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.FG, color.Value))
                {
                    bool result = Button(content, styles.toggleLabel, GUILayout.ExpandWidth(expand: false));
                    MarkAsLink(color);
                    return result;
                }
            }
        }

        /// <summary>
        /// Underlines the control just drawn while the pointer is over it, and shows the link cursor.
        /// Turns any already-drawn control into something that reads as a hyperlink.
        /// </summary>
        internal static void MarkAsLink(Color? color = null)
        {
            if (!color.HasValue)
            {
                color = new Color(0.3f, 0.7f, 1f);
            }

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax - 1f, lastRect.width, 1f), color.Value);
            }

            EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
        }
    }
}
