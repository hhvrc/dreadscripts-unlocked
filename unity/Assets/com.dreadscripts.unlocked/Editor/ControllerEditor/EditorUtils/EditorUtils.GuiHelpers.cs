// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ReadQueue        -> Clicked(Rect),                       line 5817
//   static SelectQueue      -> Clicked(out bool),                   line 5823
//   static RemoveQueue      -> Clicked(Rect, out bool),             line 5828
//   static InstantiateQueue -> RightClicked,                        line 5848
//   static ValidateQueue    -> CaptureHotControl,                   line 5944
//   static CustomizeQueue   -> SplitIntoGrid,                       line 5959
//   static RateQueue        -> IconSpacer,                          line 5981
//   static DestroyQueue     -> TextFieldDropDown(label, ...),       line 5998
//   static GetQueue         -> TextFieldDropDown(GUIContent, ...),  line 6003
//   static CalcQueue        -> TextFieldDropDown(Rect, ...),        line 6013
//   static ComputeQueue     -> LinkLabel(string, ...),              line 5657
//   static MoveQueue        -> LinkLabel(GUIContent, ...),          line 5662
//   static ConcatQueue      -> DrawLinkUnderline,                   line 5679
//   static SearchResolver   -> FadeGroup,                           line 3023
//   static CustomizeError   -> the textFieldDropDown accessor,      line 5987
//   static field m_TaskProperty -> textFieldDropDownMethod,         line 2168
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
//
// The click helpers all share one convention: a default(Rect) argument means "the rect the last
// layout control occupied", resolved through GUILayoutUtility.GetLastRect. It is what lets a
// caller write a layout control and then ask whether it was clicked without ever naming a rect.
//
// TextFieldDropDown wraps UnityEditor.EditorGUI.TextFieldDropDown, which is internal: a text field
// with a dropdown of suggestions beside it. When the reflection lookup fails -- a Unity version
// where the method moved or changed signature -- every overload degrades to returning the value it
// was given, so the field simply does not draw rather than throwing.
//
// SHIPPED BUG
//
// RightClicked (decompiled InstantiateQueue, line 5848), transcribed as shipped: the two branches
// are an if/else, so when the rect is defaulted it is resolved from the last control and then *not*
// tested -- the method returns false. The containment test only runs on the branch where an
// explicit rect was passed. A right-click on a layout control therefore never registers through
// this. See the remark on the method.
//
// DELIBERATE DEVIATION
//
// SplitIntoGrid allocates `new Rect[Mathf.Max(count, 0)]` where decompiled CustomizeQueue (line
// 5959) allocates `new Rect[visitorPosition]` directly. For count >= 0 the two are identical; for a
// negative count the vendor throws OverflowException on the allocation, before its own `count <= 0`
// guard can return, whereas this returns an empty array. No shipped call site passes a negative
// count, so the difference is unreachable in practice, but it is a real behavioural change and not
// a decompiler artifact.
//
// SplitIntoGrid is also `internal` where decompiled CustomizeQueue is `public`. EditorUtils itself
// is an internal type, so this is not observable outside the assembly.
//
// Audit status: PARTIAL -- all sixteen declared members were diffed statement by statement against
// decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs at the cited lines, all
// of which still land on the named member in the current snapshot. Fifteen match exactly once
// inverted guard clauses and the decompiler's repeated `CustomizeError()` calls (hoisted into a
// local here, equivalent because the accessor caches) are accounted for. The sixteenth,
// SplitIntoGrid, diverges on negative counts -- see DELIBERATE DEVIATION above; that one point is
// what keeps this from VERIFIED. The MAP claims no member the file does not declare; the stale
// paragraph about FlushQueue/ConnectQueue/CalculateQueue/TestQueue was removed, those four having
// moved to EditorUtils.OverlayLabels.cs and EditorUtils.LayoutOverlayLabels.cs, and the previously
// unclaimed backing field for the TextFieldDropDown accessor (m_TaskProperty) was added.

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Whether a left mouse-down landed in <paramref name="rect"/> this event. Consumes the
        /// event when it did.
        /// </summary>
        internal static bool Clicked(Rect rect = default(Rect))
        {
            return Clicked(rect, out bool _);
        }

        /// <summary>
        /// <see cref="Clicked(Rect)"/> over the last layout control, also reporting whether it was
        /// a double click.
        /// </summary>
        internal static bool Clicked(out bool doubleClick)
        {
            return Clicked(default(Rect), out doubleClick);
        }

        /// <summary>
        /// Whether a left mouse-down landed in <paramref name="rect"/> this event, reporting in
        /// <paramref name="doubleClick"/> whether it was the second of a double click.
        /// </summary>
        /// <remarks>
        /// <paramref name="doubleClick"/> is set for any left mouse-down of the event, whether or
        /// not it landed in the rect -- it describes the event, not the hit.
        /// </remarks>
        internal static bool Clicked(Rect rect, out bool doubleClick)
        {
            Event current = Event.current;
            doubleClick = false;

            if (current.type != EventType.MouseDown || current.button != 0)
            {
                return false;
            }

            doubleClick = current.clickCount == 2;

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            if (!rect.Contains(current.mousePosition))
            {
                return false;
            }

            current.Use();
            return true;
        }

        /// <summary>
        /// Whether a context click (right-click, or ctrl-click on macOS) landed in
        /// <paramref name="rect"/>. Consumes the event when it did.
        /// </summary>
        /// <remarks>
        /// VENDOR BUG, transcribed as shipped: the default-rect case resolves the last control's
        /// rect and then returns false without testing it, because the containment test is in the
        /// <c>else</c> branch. Only an explicitly passed rect can ever match. Fixing it would make
        /// context menus appear where they currently do not, which is a behaviour change rather
        /// than a reconstruction.
        /// </remarks>
        internal static bool RightClicked(Rect rect = default(Rect))
        {
            Event current = Event.current;
            if (current.type != EventType.ContextClick)
            {
                return false;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }
            else if (rect.Contains(current.mousePosition))
            {
                current.Use();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Claims <paramref name="controlId"/> as the hot control when the mouse goes down in
        /// <paramref name="rect"/>, and reports whether it currently holds it.
        /// </summary>
        /// <remarks>
        /// The building block of a hand-rolled drag: true means this control owns the mouse and
        /// should act on drag events wherever the pointer has since moved to. Note it never
        /// releases -- the caller has to clear hotControl on mouse-up itself.
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
        /// Divides <paramref name="rect"/> into <paramref name="count"/> cells of equal size, laid
        /// out row-major in as square a grid as the count allows.
        /// </summary>
        /// <remarks>
        /// Columns are ceil(sqrt(count)) and rows are ceil(count / columns), so the grid is never
        /// taller than it is wide and the last row may be short. A count of zero or less gives an
        /// empty array rather than throwing.
        /// </remarks>
        internal static Rect[] SplitIntoGrid(Rect rect, int count)
        {
            Rect[] cells = new Rect[Mathf.Max(count, 0)];
            if (count <= 0)
            {
                return cells;
            }

            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt((float)count / columns);
            float cellWidth = rect.width / columns;
            float cellHeight = rect.height / rows;

            for (int i = 0; i < count; i++)
            {
                cells[i] = new Rect(
                    rect.x + i % columns * cellWidth,
                    rect.y + i / columns * cellHeight,
                    cellWidth,
                    cellHeight);
            }

            return cells;
        }

        /// <summary>
        /// A blank layout control one icon-button wide, to keep a row aligned with rows that do
        /// have a button there.
        /// </summary>
        internal static void IconSpacer()
        {
            GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
        }

        /// <summary>
        /// Cached UnityEditor.EditorGUI.TextFieldDropDown(Rect, GUIContent, string, string[]),
        /// which is internal.
        /// </summary>
        private static MethodInfo textFieldDropDownMethod;

        /// <summary>
        /// The internal TextFieldDropDown method, or null on a Unity version that does not have it.
        /// </summary>
        /// <remarks>
        /// The lookup is retried on every call while it fails, as shipped -- the vendor cached the
        /// hit but not the miss. That costs a reflection lookup per frame on an editor without the
        /// method, which is the case this is meant to tolerate; kept as-is rather than "improved",
        /// since the difference is measurable only in that failure case.
        /// </remarks>
        private static MethodInfo TextFieldDropDownMethod =>
            textFieldDropDownMethod ?? (textFieldDropDownMethod = typeof(EditorGUI).GetMethod(
                "TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(Rect), typeof(GUIContent), typeof(string), typeof(string[]) }, null));

        /// <summary>
        /// A text field with a dropdown of <paramref name="suggestions"/> beside it. Returns
        /// <paramref name="value"/> unchanged, without drawing, where the editor does not have the
        /// internal method.
        /// </summary>
        internal static string TextFieldDropDown(string label, string value, string[] suggestions,
            params GUILayoutOption[] options)
        {
            return TextFieldDropDown(new GUIContent(label), value, suggestions, options);
        }

        /// <summary>
        /// <see cref="TextFieldDropDown(string, string, string[], GUILayoutOption[])"/> taking a
        /// GUIContent label.
        /// </summary>
        internal static string TextFieldDropDown(GUIContent label, string value, string[] suggestions,
            params GUILayoutOption[] options)
        {
            MethodInfo method = TextFieldDropDownMethod;
            if (method == null)
            {
                return value;
            }

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, options);
            return (string)method.Invoke(null, new object[] { rect, label, value, suggestions });
        }

        /// <summary>
        /// <see cref="TextFieldDropDown(string, string, string[], GUILayoutOption[])"/> at an
        /// explicit rect.
        /// </summary>
        internal static string TextFieldDropDown(Rect rect, string label, string value, string[] suggestions)
        {
            MethodInfo method = TextFieldDropDownMethod;
            if (method == null)
            {
                return value;
            }

            return (string)method.Invoke(null, new object[] { rect, new GUIContent(label), value, suggestions });
        }

        /// <summary>
        /// A label that behaves like a hyperlink: tinted, underlined on hover, with a link cursor,
        /// and returning true when clicked.
        /// </summary>
        internal static bool LinkLabel(string text, Color? color = null)
        {
            return LinkLabel(new GUIContent(text), color);
        }

        /// <summary>
        /// <see cref="LinkLabel(string, Color?)"/> taking a GUIContent, so the link can carry an
        /// icon or a tooltip.
        /// </summary>
        /// <param name="color">Defaults to <see cref="linkColor"/>.</param>
        /// <remarks>
        /// The background is forced clear so the button style contributes nothing but its text
        /// layout; the foreground carries the link colour. Width is not expanded, so several links
        /// sit side by side in a horizontal group rather than each taking a whole row.
        /// </remarks>
        internal static bool LinkLabel(GUIContent content, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = linkColor;
            }

            using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.FG, color.Value))
                {
                    bool clicked = Button(content, styles.toggleLabel, GUILayout.ExpandWidth(false));
                    DrawLinkUnderline(color);
                    return clicked;
                }
            }
        }

        /// <summary>
        /// Underlines the last layout control while the mouse is over it, and gives it the link
        /// cursor. Repaint only, so it costs nothing on the other event passes.
        /// </summary>
        /// <param name="color">Defaults to <see cref="linkColor"/>'s literal value.</param>
        /// <remarks>
        /// The cursor rect is registered for the whole control, not only while hovered -- the
        /// underline is the hover feedback, the cursor is what says it is clickable at all.
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

            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color.Value);
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        }
    
        /// <summary>
        /// Draws <paramref name="content"/> inside an EditorGUILayout fade group driven by
        /// <paramref name="fade"/>, skipping it entirely when the group is fully closed.
        /// </summary>
        /// <param name="whileFading">
        /// Extra drawing to do only while the group is part-way open -- for a cross-fade, or an
        /// overlay that should not be there once the animation settles. Never called at 0 or at 1.
        /// </param>
        /// <remarks>
        /// The zero check matters for more than performance: a fade group at 0 still runs its
        /// contents' layout, so anything inside that reserves a control ID or reads GetLastRect
        /// would misbehave.
        /// </remarks>
        internal static void FadeGroup(this AnimBool fade, Action content, Action whileFading = null)
        {
            if (fade.faded == 0f)
            {
                return;
            }

            EditorGUILayout.BeginFadeGroup(fade.faded);
            content();
            if (whileFading != null && fade.faded > 0f && fade.faded < 1f)
            {
                whileFading();
            }

            EditorGUILayout.EndFadeGroup();
        }
    }
}
