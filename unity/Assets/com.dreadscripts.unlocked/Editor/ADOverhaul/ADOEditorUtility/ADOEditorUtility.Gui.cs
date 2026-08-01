// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static field _PredicateSerializer   -> unityVersion,                line 2052
//   static field collectionSerializer   -> isUnity2022,                 line 2054
//   static field interceptorSerializer  -> deferredCursorRects,         line 2056
//   static field m_RegistrySerializer   -> deferringCursorRects,        line 2058
//   static field _ClientSerializer      -> textFieldDropDownMethod,     line 2060
//   static PublishStatus                -> BeginDeferredCursorRects,    line 3030
//   static CollectStatus                -> ClearDeferredCursorRects,    line 3039
//   static PrintStatus                  -> EndDeferredCursorRects,      line 3044
//   static InterruptStatus              -> LinkLabel(string, Color?),   line 3057
//   static ViewStatus                   -> LinkLabel(GUIContent, Color?), line 3062
//   static PostStatus                   -> DrawLinkUnderline,           line 3079
//   static ListStatus                   -> IconButton,                  line 3097
//   static ForgotStatus                 -> Button(Rect, string, ...),   line 3115
//   static UpdateStatus                 -> Button(Rect, string, ...) with no style, line 3120
//   static SearchStatus                 -> Button(Rect, GUIContent, ...) with no style, line 3125
//   static RegisterStatus               -> Button(Rect, GUIContent, ...), line 3150
//   static LoginStatus                  -> Button(string, ...),         line 3130
//   static PatchStatus                  -> Button(string, ...) with no style, line 3135
//   static CheckStatus                  -> Button(GUIContent, ...) with no style, line 3140
//   static CallStatus                   -> Button(GUIContent, ...),     line 3145
//   static ChangeStatus                 -> ToggleButton(bool, string, ...), line 3161
//   static StopStatus                   -> ToggleButton(bool, string, ...) with no style, line 3166
//   static PushStatus                   -> ToggleButton(bool, GUIContent, ...) with no style, line 3171
//   static PrepareStatus                -> ToggleButton(bool, GUIContent, ...), line 3176
//   static ReadStatus                   -> ClickArea,                   line 3187
//   static TestStatus                   -> AddLinkCursor,               line 3202
//   static InsertStatus                 -> AddCursorRect,               line 3214
//   static EnableStatus                 -> OverlayLabel(Rect, ...),     line 3234
//   static AwakeStatus                  -> OverlayLabel(string, ...),   line 3250
//   static DisableStatus                -> Separator,                   line 3255
//   static VisitStatus                  -> CaptureHotControl,           line 3266
//   static AssetStatus                  -> IconSpacer,                  line 3281
//   static SortRef                      -> textFieldDropDown (property), line 3286
//   static PopStatus                    -> TextFieldDropDown(string, ...), line 3298
//   static InstantiateStatus            -> TextFieldDropDown(GUIContent, ...), line 3303
//   static RestartStatus                -> TextFieldDropDown(Rect, ...), line 3313
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// As in EditorUtils.Buttons.cs, the obfuscator had already split each button into one overload per
// (content type, style present) combination; the optional style parameter collapses each pair back
// into one method, so eight names map onto four here.
//
// Already ported elsewhere, deliberately NOT repeated in this file:
//   ManageStatus  (line 3328) -> IconContent,             ADOEditorUtility.Contents.cs
//   CustomizeRef  (line 3336) -> the `contents` property, ADOEditorUtility.Contents.cs
//   MapRef        (line 3342) -> the `styles` property,   ADOEditorUtility.Styles.cs
// All three were [SpecialName] accessors and were already restored as properties there.
//
// Obfuscator scaffolding dropped: the shipped SortRef went through a static helper
// `FlushAdapter(Type, string, BindingFlags, Binder, Type[], ParameterModifier[])` (line 4058) whose
// entire body was `return type.GetMethod(...)` with the arguments passed straight through. It is an
// indirection the obfuscator inserted, not a helper the author wrote, so the GetMethod call is
// inlined below.
//
// 2019 vs 2022: behaviourally identical throughout (2019 lines 3043-3343, under different
// obfuscated names). The only textual differences are decompiler choices — 2019 renders the
// OverlayLabel guard as `width > reserved + inset` where 2022 renders the equivalent
// `!(width <= reserved + inset)`, 2019 inverts the early-out in AddCursorRect, and the two builds
// mangle the IconButton default-size block differently (see the note on IconButton).
//
// Shipped bug preserved: Button(Rect, GUIContent, GUIStyle) registers its link cursor over the last
// *layout* rect instead of over the rect it was given. See the comment on that method.
//
// Styles referenced here map to ADOEditorUtility.Styles.cs as: m_ProducerSerializer -> iconButton,
// _SchemaSerializer -> toggleLabel, m_ProcSerializer -> noteLeft, _IdentifierMethod -> noteRight.
//
// Not ported from this region: nothing. Members after line 3346 (RateStatus, CloneStatus, ...)
// belong to the events region and are out of scope here.

using System;
using System.Collections.Generic;
using System.Reflection;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>The running editor's version string.</summary>
        internal static string unityVersion = Application.unityVersion;

        /// <summary>
        /// Whether the editor is a 2022 release. Used to correct for the extra chrome 2022 puts
        /// above a window's client area, which shifts GUI rects relative to screen space.
        /// </summary>
        internal static bool isUnity2022 = unityVersion.Contains("2022");

        /// <summary>
        /// Cursor rects collected while <see cref="deferringCursorRects"/> is set, in screen space.
        /// </summary>
        private static readonly Stack<(Rect rect, MouseCursor cursor)> deferredCursorRects = new Stack<(Rect, MouseCursor)>();

        private static bool deferringCursorRects;

        private static MethodInfo textFieldDropDownMethod;

        /// <summary>
        /// Starts collecting cursor rects instead of registering them immediately.
        /// </summary>
        /// <remarks>
        /// Unity applies cursor rects in registration order, so a rect registered inside a nested
        /// area loses to one registered later by an enclosing control. Deferring them and replaying
        /// the stack in reverse on <see cref="EndDeferredCursorRects"/> gives the innermost control
        /// the last word instead.
        /// </remarks>
        internal static void BeginDeferredCursorRects()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            deferringCursorRects = true;
            ClearDeferredCursorRects();
        }

        /// <summary>Drops anything collected so far without registering it.</summary>
        internal static void ClearDeferredCursorRects()
        {
            deferredCursorRects.Clear();
        }

        /// <summary>
        /// Stops collecting and registers everything collected, innermost first.
        /// </summary>
        internal static void EndDeferredCursorRects()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            deferringCursorRects = false;
            while (deferredCursorRects.Count > 0)
            {
                var (screenRect, cursor) = deferredCursorRects.Pop();
                EditorGUIUtility.AddCursorRect(GUIUtility.ScreenToGUIRect(screenRect), cursor);
            }
        }

        /// <summary>
        /// Registers <paramref name="cursor"/> over <paramref name="rect"/>, honouring an active
        /// <see cref="BeginDeferredCursorRects"/> collection.
        /// </summary>
        /// <param name="evenWhenDisabled">
        /// Register the cursor even inside a disabled scope. Off by default so disabled controls do
        /// not look clickable.
        /// </param>
        internal static void AddCursorRect(Rect rect, MouseCursor cursor, bool evenWhenDisabled = false)
        {
            if (!GUI.enabled && !evenWhenDisabled)
            {
                return;
            }

            if (deferringCursorRects)
            {
                // 2022 windows carry extra chrome above the client area, so the GUI-to-screen
                // conversion is off by its height; corrected here rather than on replay because the
                // stack stores screen rects.
                if (isUnity2022)
                {
                    rect.y += 46f;
                }

                deferredCursorRects.Push((GUIUtility.GUIToScreenRect(rect), cursor));
            }
            else if (Event.current.type == EventType.Repaint)
            {
                EditorGUIUtility.AddCursorRect(rect, cursor);
            }
        }

        /// <summary>
        /// Shows the link cursor over <paramref name="rect"/>, defaulting to the rect of the control
        /// just drawn. Call right after drawing a clickable control.
        /// </summary>
        internal static void AddLinkCursor(Rect rect = default(Rect), bool evenWhenDisabled = false)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            AddCursorRect(rect, MouseCursor.Link, evenWhenDisabled);
        }

        /// <summary>
        /// A label styled and behaving like a hyperlink: no button chrome, tinted text, and an
        /// underline that appears under the pointer.
        /// </summary>
        /// <param name="color">Link tint; the default is the tool's link blue.</param>
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

            // Clearing the background tint removes the button chrome that toggleLabel would
            // otherwise draw on hover, leaving only the style's hover text colour.
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.FG, color.Value))
                {
                    bool result = Button(content, styles.toggleLabel, GUILayout.ExpandWidth(false));
                    DrawLinkUnderline(color);
                    return result;
                }
            }
        }

        /// <summary>
        /// Underlines the control just drawn while the pointer is over it, and gives it the link
        /// cursor. Split out so a caller that draws its own link-like control can reuse the
        /// decoration.
        /// </summary>
        /// <remarks>
        /// The cursor rect is registered directly rather than through <see cref="AddCursorRect"/>,
        /// so it is never deferred. Ported as shipped.
        /// </remarks>
        internal static void DrawLinkUnderline(Color? color = null)
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

        /// <summary>
        /// A square button carrying only an icon, drawn with no button chrome.
        /// </summary>
        /// <param name="width">Width in pixels; -1 means one line height, matching the row it sits in.</param>
        /// <param name="height">Height in pixels; -1 means one line height.</param>
        /// <remarks>
        /// Both shipped builds decompile this method's default-size block incorrectly — 2022 as an
        /// infinite <c>while (true)</c> around the height assignment, 2019 as a goto chain that
        /// re-enters the width assignment. ControllerEditor's byte-identical copy of the same method
        /// (EditorUtils.CallQueue) decompiles cleanly as the two plain checks below, which is what
        /// they are restored to.
        /// </remarks>
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
        /// A layout button that returns true on the frame it is clicked, and shows the link cursor
        /// while hovered.
        /// </summary>
        internal static bool Button(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="Button(string, GUIStyle, GUILayoutOption[])"/>
        internal static bool Button(GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, content, style, options);
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

            // Shipped bug: no rect is passed, so the link cursor lands on whatever GUILayout drew
            // last rather than on this button. ControllerEditor's otherwise byte-identical copy of
            // this method (EditorUtils.QueryQueue) passes the rect, which is what this was meant to
            // do. Ported as shipped.
            AddLinkCursor();
            return result;
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

        /// <summary>
        /// Draws a small italic annotation inside an existing rect, offset from its edge. Used for
        /// the placeholder text over an empty field and for unit suffixes.
        /// </summary>
        /// <param name="draw">Lets a caller gate the label without an <c>if</c> at the call site.</param>
        /// <param name="reservedWidth">
        /// Width the rect's own content needs. The label is skipped when the rect is not wider than
        /// this plus <paramref name="inset"/>, so it disappears instead of overlapping in a narrow
        /// inspector.
        /// </param>
        /// <param name="inset">How far in from the chosen edge the label starts, plus a 2.5px gap.</param>
        /// <param name="alignLeft">
        /// Left edge when true, right edge when false. Also picks the matching alignment style.
        /// </param>
        /// <remarks>
        /// <paramref name="reservedWidth"/> only takes part in the width test; it never shifts the
        /// label. Only <paramref name="inset"/> does.
        /// </remarks>
        internal static void OverlayLabel(Rect rect, string text, bool draw = true, float reservedWidth = 0f, float inset = 0f, bool alignLeft = true, GUIStyle style = null)
        {
            if (!draw || rect.width <= reservedWidth + inset)
            {
                return;
            }

            if (alignLeft)
            {
                rect.x += inset + 2.5f;
            }
            else
            {
                rect.x -= inset + 2.5f;
            }

            GUI.Label(rect, text, style ?? (alignLeft ? styles.noteLeft : styles.noteRight));
        }

        /// <summary>
        /// <see cref="OverlayLabel(Rect, string, bool, float, float, bool, GUIStyle)"/> over the
        /// control just drawn, for annotating a layout field.
        /// </summary>
        internal static void OverlayLabel(string text, bool draw = true, float reservedWidth = 0f, float inset = 0f, bool alignLeft = true)
        {
            OverlayLabel(GUILayoutUtility.GetLastRect(), text, draw, reservedWidth, inset, alignLeft);
        }

        /// <summary>
        /// A horizontal rule with vertical breathing room around it, in a grey that reads the same
        /// on both editor skins.
        /// </summary>
        /// <param name="thickness">Height of the drawn line.</param>
        /// <param name="spacing">Total extra height reserved, split evenly above and below.</param>
        /// <remarks>
        /// The rule is widened past the reserved rect (2px left, 4px right) so it spans the full
        /// panel rather than stopping inside the standard layout margins.
        /// </remarks>
        internal static void Separator(int thickness = 2, int spacing = 10)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + spacing));
            rect.height = thickness;
            rect.y += spacing / 2f;
            rect.x -= 2f;
            rect.width += 6f;

            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out Color color);
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Claims the GUI's hot control for <paramref name="controlId"/> when a press lands in
        /// <paramref name="rect"/>, and reports whether that control currently holds it. The basis
        /// of the tool's manual drag handles.
        /// </summary>
        /// <remarks>
        /// On the frame the press is captured this still returns false, because the check runs
        /// before the assignment; the caller only sees true from the following event onwards. That
        /// is what makes a click without movement not register as a drag.
        /// </remarks>
        internal static bool CaptureHotControl(Rect rect, int controlId)
        {
            if (GUIUtility.hotControl == controlId)
            {
                return true;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
            {
                GUIUtility.hotControl = controlId;
                current.Use();
            }

            return false;
        }

        /// <summary>
        /// Reserves the width of one icon button in a layout row, to keep rows aligned when a row
        /// has no icon to draw.
        /// </summary>
        internal static void IconSpacer()
        {
            GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
        }

        /// <summary>
        /// <c>EditorGUI.TextFieldDropDown</c>, which Unity keeps internal. A text field with a
        /// dropdown of suggestions that the user may ignore and type over.
        /// </summary>
        /// <remarks>
        /// Resolved by reflection and cached, and every caller tolerates a null result by returning
        /// the value unchanged — the method is internal API and could disappear in any editor
        /// release.
        /// </remarks>
        private static MethodInfo textFieldDropDown
        {
            get
            {
                return textFieldDropDownMethod ?? (textFieldDropDownMethod = typeof(EditorGUI).GetMethod(
                    "TextFieldDropDown",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(Rect), typeof(GUIContent), typeof(string), typeof(string[]) },
                    null));
            }
        }

        /// <summary>
        /// A labelled text field with a dropdown of <paramref name="suggestions"/>. Returns the
        /// edited value, or <paramref name="value"/> unchanged if the editor does not expose the
        /// underlying control.
        /// </summary>
        internal static string TextFieldDropDown(string label, string value, string[] suggestions, params GUILayoutOption[] options)
        {
            return TextFieldDropDown(new GUIContent(label), value, suggestions, options);
        }

        /// <inheritdoc cref="TextFieldDropDown(string, string, string[], GUILayoutOption[])"/>
        internal static string TextFieldDropDown(GUIContent label, string value, string[] suggestions, params GUILayoutOption[] options)
        {
            if (textFieldDropDown == null)
            {
                return value;
            }

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, options);
            return (string)textFieldDropDown.Invoke(null, new object[] { rect, label, value, suggestions });
        }

        /// <summary>
        /// <see cref="TextFieldDropDown(GUIContent, string, string[], GUILayoutOption[])"/> at an
        /// explicit rect.
        /// </summary>
        internal static string TextFieldDropDown(Rect rect, string label, string value, string[] suggestions)
        {
            if (textFieldDropDown == null)
            {
                return value;
            }

            return (string)textFieldDropDown.Invoke(null, new object[] { rect, new GUIContent(label), value, suggestions });
        }
    }
}
