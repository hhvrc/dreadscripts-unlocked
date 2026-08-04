// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_BroadcasterProperty -> unityVersion,               line 2156
//   static m_ProxyProperty       -> isUnity2022,                line 2158
//   static stateProperty         -> deferredCursorRects,        line 2164
//   static globalProperty        -> deferringCursorRects,       line 2166
//   static EnableQueue           -> BeginDeferredCursorRects,   line 5630
//   static PublishQueue          -> ClearDeferredCursorRects,   line 5639
//   static PopQueue              -> EndDeferredCursorRects,     line 5644
//   static StartQueue            -> AddLinkCursor,              line 5805
//   static AwakeQueue            -> AddCursorRect,              line 5866
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- the region is byte-identical between the two snapshots
// and every statement below was transcribed from it.
//
// Complete: nothing cursor-related is left in the outer class body. The scroll-wheel handler this
// header previously listed as outstanding is HandleScrollWheel, which is an input helper rather
// than a cursor one and now lives in EditorUtils.GuiHelpers.cs; the link cursor a clickable label
// registers is DrawLinkUnderline, in the same file.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>The running editor's version string.</summary>
        internal static readonly string unityVersion = Application.unityVersion;

        /// <summary>
        /// Whether the editor is a 2022 release. Used to correct for the extra chrome 2022 puts
        /// above a window's client area, which shifts GUI rects relative to screen space.
        /// </summary>
        internal static readonly bool isUnity2022 = unityVersion.Contains("2022");

        /// <summary>
        /// Cursor rects collected while <see cref="deferringCursorRects"/> is set, in screen space.
        /// </summary>
        private static readonly Stack<(Rect rect, MouseCursor cursor)> deferredCursorRects = new Stack<(Rect, MouseCursor)>();

        private static bool deferringCursorRects;

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
        /// Shows the link cursor over <paramref name="rect"/>, defaulting to the rect of the control
        /// just drawn. Call right after drawing a clickable control.
        /// </summary>
        /// <param name="evenWhenDisabled">
        /// Register the cursor even inside a disabled scope. Off by default so disabled controls do
        /// not look clickable.
        /// </param>
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
        /// Registers <paramref name="cursor"/> over <paramref name="rect"/>, honouring an active
        /// <see cref="BeginDeferredCursorRects"/> collection.
        /// </summary>
        internal static void AddCursorRect(Rect rect, MouseCursor cursor, bool evenWhenDisabled = false)
        {
            if (!GUI.enabled && !evenWhenDisabled)
            {
                return;
            }

            if (deferringCursorRects)
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
    }
}
