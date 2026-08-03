// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_BroadcasterProperty -> unityVersion,         line 2155
//   static m_ProxyProperty       -> isUnity2022,          line 2158
//   static stateProperty         -> deferredCursorRects,  line 2164
//   static globalProperty        -> deferCursorRects,     line 2166
//   static DisableQueue/InsertQueue/RestartQueue -> Button,       lines 5722-5735
//   static QueryQueue                            -> Button(Rect), line 5737
//   static AddQueue/InvokeQueue/FindQueue/ExcludeQueue -> ToggleButton,        lines 5748-5771
//   static InitQueue/VisitQueue                        -> ToggleButtonChanged, lines 5773-5787
//   static DefineQueue           -> ClickedIn,            line 5789
//   static StartQueue            -> AddLinkCursor,        line 5805
//   static AwakeQueue            -> AddCursor,            line 5841
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Partial in progress: this holds the button family and the cursor helpers only. The rest of the
// outer class body's GUI code (Rect layout helpers, the field drawers) is not ported yet.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        internal static readonly string unityVersion = Application.unityVersion;

        /// <summary>
        /// Scene-view overlay rects sit 46px lower on 2022 than on earlier versions, so the cursor
        /// rects pushed while drawing an overlay need shifting to match.
        /// </summary>
        internal static readonly bool isUnity2022 = unityVersion.Contains("2022");

        /// <summary>
        /// Cursor rects captured while <see cref="deferCursorRects"/> is set, in screen space.
        /// </summary>
        private static readonly Stack<(Rect, MouseCursor)> deferredCursorRects = new Stack<(Rect, MouseCursor)>();

        /// <summary>
        /// Set while drawing inside a scene-view overlay, where cursor rects cannot be registered
        /// directly because the overlay's GUI runs outside the window that owns the cursor.
        /// </summary>
        private static bool deferCursorRects;

        /// <summary>
        /// Registers a mouse cursor over <paramref name="rect"/>, or defers it when drawing inside a
        /// scene-view overlay.
        /// </summary>
        /// <param name="evenWhenDisabled">
        /// Register the cursor even where the GUI is disabled. Off by default, so a greyed-out
        /// control does not advertise itself as clickable.
        /// </param>
        internal static void AddCursor(Rect rect, MouseCursor cursor, bool evenWhenDisabled = false)
        {
            if (!GUI.enabled && !evenWhenDisabled)
            {
                return;
            }

            if (deferCursorRects)
            {
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
        /// Marks <paramref name="rect"/> (defaulting to the last laid-out rect) as clickable by
        /// showing the link cursor over it.
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

            AddCursor(rect, MouseCursor.Link, evenWhenDisabled);
        }

        // ── Buttons ─────────────────────────────────────────────────────────────────────────
        //
        // These are drawn as a Toggle held at false rather than as GUI.Button. A Toggle renders
        // identically but reports the click on mouse-up over the control, which is what lets the
        // whole family share one implementation with the real toggles below. Each one also claims
        // the link cursor, which a plain button would not.

        internal static bool Button(string label, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(label), null, options);
        }

        internal static bool Button(GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, label, style, options);
        }

        /// <summary>A button at an explicit rect rather than in the layout flow.</summary>
        internal static bool Button(Rect rect, GUIContent label, GUIStyle style = null)
        {
            bool clicked = GUI.Button(rect, label, style ?? GUI.skin.button);
            AddLinkCursor(rect);
            return clicked;
        }

        // ── Toggles ─────────────────────────────────────────────────────────────────────────

        internal static bool ToggleButton(bool value, string label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(value, new GUIContent(label), style, options);
        }

        /// <summary>A button that stays visibly pressed while <paramref name="value"/> is true.</summary>
        /// <returns>The new value.</returns>
        internal static bool ToggleButton(bool value, GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool newValue = GUILayout.Toggle(value, label, style ?? GUI.skin.button, options);
            AddLinkCursor();
            return newValue;
        }

        internal static bool ToggleButtonChanged(bool value, string label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButtonChanged(value, new GUIContent(label), style, options);
        }

        /// <summary>
        /// As <see cref="ToggleButton(bool, GUIContent, GUIStyle, GUILayoutOption[])"/>, but reports
        /// whether the user changed it rather than what it now is — for a toggle whose state lives
        /// somewhere the caller cannot simply assign.
        /// </summary>
        /// <returns>True if this call flipped the value.</returns>
        internal static bool ToggleButtonChanged(bool value, GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool newValue = GUILayout.Toggle(value, label, style ?? GUI.skin.button, options);
            AddLinkCursor();
            return value != newValue;
        }

        /// <summary>
        /// Whether the user just left-clicked inside <paramref name="rect"/>, defaulting to the last
        /// laid-out rect. For making an arbitrary drawn area clickable.
        /// </summary>
        internal static bool ClickedIn(Rect rect = default(Rect))
        {
            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            AddLinkCursor(rect);

            Event current = Event.current;
            return current.type == EventType.MouseDown
                   && current.button == 0
                   && rect.Contains(current.mousePosition);
        }
    }
}
