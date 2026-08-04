// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static _PredicateSerializer  -> unityVersion,               line 2052
//   static collectionSerializer  -> isUnity2022,                line 2054
//   static interceptorSerializer -> deferredCursorRects,        line 2056
//   static m_RegistrySerializer  -> deferringCursorRects,       line 2058
//   static PublishStatus         -> BeginDeferredCursorRects,   line 3030
//   static CollectStatus         -> ClearDeferredCursorRects,   line 3039
//   static PrintStatus           -> EndDeferredCursorRects,     line 3044
//   static TestStatus            -> AddLinkCursor,              line 3202
//   static InsertStatus          -> AddCursorRect,              line 3214
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: UNAUDITED -- was VERIFIED in ec740dc, but the code has changed
// since (+0 code lines); needs re-checking against export/ before the claim is restored.
// above and cross-checked against the ControllerEditor twin named next.
//
// 2019 vs 2022: the same nine members with the same bodies (2019 lines 2054, 2056, 2058, 2060,
// 3043, 3052, 3057, 3218 and 3230, under different obfuscated names). The only difference is that
// ILSpy inverted the deferring/immediate branch in AddCursorRect for the 2019 build; same
// behaviour either way. No behavioural divergence.
//
// Shared with ControllerEditor: EditorUtils.Cursors.cs is the same nine members, statement for
// statement, down to the 46-pixel 2022 nudge and the reverse-order replay. Deliberately NOT
// consolidated, on the same basis as ADOEditorUtility.Colors.cs: merging the two products'
// utility surfaces is a separate decision, and this file must not reach into the other product's
// namespace. See that file for the longer explanation of why the deferral exists.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>The running editor's version string.</summary>
        private static readonly string UnityVersion = Application.unityVersion;

        /// <summary>
        /// Whether the editor is a 2022 release. Used to correct for the extra chrome 2022 puts
        /// above a window's client area, which shifts GUI rects relative to screen space.
        /// </summary>
        private static readonly bool IsUnity2022 = UnityVersion.Contains("2022");

        /// <summary>
        /// Cursor rects collected while <see cref="deferringCursorRects"/> is set, in screen space.
        /// </summary>
        private static readonly Stack<(Rect rect, MouseCursor cursor)> DeferredCursorRects = new();

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
            DeferredCursorRects.Clear();
        }

        /// <summary>Stops collecting and registers everything collected, innermost first.</summary>
        internal static void EndDeferredCursorRects()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            deferringCursorRects = false;
            while (DeferredCursorRects.Count > 0)
            {
                var (screenRect, cursor) = DeferredCursorRects.Pop();
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
        internal static void AddLinkCursor(Rect rect = default, bool evenWhenDisabled = false)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (rect == default)
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
                if (IsUnity2022)
                {
                    rect.y += 46f;
                }

                DeferredCursorRects.Push((GUIUtility.GUIToScreenRect(rect), cursor));
            }
            else if (Event.current.type == EventType.Repaint)
            {
                EditorGUIUtility.AddCursorRect(rect, cursor);
            }
        }
    }
}
