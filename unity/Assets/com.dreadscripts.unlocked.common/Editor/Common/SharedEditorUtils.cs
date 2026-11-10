// Reconstructed from: reverse-engineering/export/ADOverhaul2019/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//
//   m_WrapperTemplate    -> unityVersion,              line 2054
//   m_AlgoTemplate       -> isUnity2022,               line 2056
//   m_MappingTemplate    -> deferredCursorRects,       line 2058
//   m_ParserTemplate     -> deferringCursorRects,      line 2060
//   m_DescriptorTemplate -> stylesInstance,            line 2080
//   StartManager         -> BeginDeferredCursorRects,  line 3043
//   InitManager          -> ClearDeferredCursorRects,  line 3052
//   CheckManager         -> EndDeferredCursorRects,    line 3057
//   CallManager          -> AddCursorRect,             line 3230
//   RemoveManager        -> Separator,                 line 3272
//   ManageRequest        -> styles,                    line 3360
//   _WriterTemplate      -> centeredBoldRichLabel,     line 710
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The handful of editor helpers the Common types draw with. Both tools shipped their own copy of
// each: ControllerEditor's in EditorUtils (EditorUtils.Cursors.cs, EditorUtils.Separators.cs,
// EditorUtils.Styles.cs) and ADOverhaul's in ADOEditorUtility (ADOEditorUtility.Cursors.cs,
// ADOEditorUtility.Layout.cs, ADOEditorUtility.Styles.cs). Common used to reach into
// ControllerEditor's, which made the shared code depend on one of the two products. Once each
// product is its own package and assembly that dependency cannot exist, so Common carries its own
// copy, as each shipped assembly did. It is ported from the ADOverhaul2019 build because the 2022
// build and ControllerEditor are already claimed by the two product ports, and the three copies
// are statement-for-statement identical. The one difference is a modifier: 2019 declares the two
// version fields without `readonly`. They are never written after initialisation in any build, so
// they are readonly here, as in ControllerEditor's copy.
//
// Behaviour is unchanged by the move. Nothing in either product ever opens a deferral
// (BeginDeferredCursorRects has no caller in this package), so the deferral state Common used to
// read from ControllerEditor was always false, exactly as Common's own copy is.
//
// The InternalsVisibleTo attributes below are this package's, not the original's. Every Common type
// is internal, as it was in both shipped assemblies; now that Common is an assembly of its own, the
// two product assemblies are named as friends rather than widening forty types to public.
//
// DELIBERATE DEVIATION
// The shipped Styles class holds some forty styles; Common draws with one of them, so only that one
// is carried here. It is still built lazily on first use, as the original is, because GUI.skin is
// only valid inside an OnGUI pass.
//
// Audit status: VERIFIED -- every member above was transcribed from the 2019 region listed and
// compared against the already-verified ControllerEditor and ADOverhaul2022 copies.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("DreadScripts.ControllerEditor")]
[assembly: InternalsVisibleTo("DreadScripts.ADOverhaul")]

namespace DreadScripts.Common
{
    internal static class SharedEditorUtils
    {
        /// <summary>The running editor's version string.</summary>
        internal static readonly string unityVersion = Application.unityVersion;

        /// <summary>
        /// Whether the editor is a 2022 release. Used to correct for the extra chrome 2022 puts
        /// above a window's client area, which shifts GUI rects relative to screen space.
        /// </summary>
        internal static readonly bool isUnity2022 = unityVersion.Contains("2022");

        private static readonly Stack<(Rect rect, MouseCursor cursor)> deferredCursorRects = new Stack<(Rect, MouseCursor)>();

        private static bool deferringCursorRects;

        private static Styles stylesInstance;

        internal static Styles styles => stylesInstance ?? (stylesInstance = new Styles());

        internal sealed class Styles
        {
            internal readonly GUIStyle centeredBoldRichLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                richText = true
            };
        }

        /// <summary>
        /// Starts collecting cursor rects instead of registering them, so ones added inside a
        /// clipped or offset GUI area can be replayed in window space by
        /// <see cref="EndDeferredCursorRects"/>. Only acts during Repaint.
        /// </summary>
        internal static void BeginDeferredCursorRects()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            deferringCursorRects = true;
            ClearDeferredCursorRects();
        }

        internal static void ClearDeferredCursorRects()
        {
            deferredCursorRects.Clear();
        }

        /// <summary>Stops deferring and registers every collected cursor rect. Only acts during Repaint.</summary>
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
        /// Shows <paramref name="cursor"/> while the mouse is over <paramref name="rect"/>, or queues
        /// it while a deferral is open.
        /// </summary>
        /// <param name="evenWhenDisabled">Register it even when <c>GUI.enabled</c> is false.</param>
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

        /// <summary>Draws a horizontal rule centred in its own block of vertical space.</summary>
        /// <param name="thickness">Height of the rule in pixels.</param>
        /// <param name="spacing">Total vertical padding around it; the rule is centred in it.</param>
        /// <remarks>
        /// The greys are the ones the editor's own separators use, one per skin. The 2-pixel
        /// leftward nudge and 6 extra pixels of width let the rule reach past the inspector's
        /// content margins.
        /// </remarks>
        internal static void Separator(int thickness = 2, int spacing = 10)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + spacing));
            rect.height = thickness;
            rect.y += (float)spacing / 2f;
            rect.x -= 2f;
            rect.width += 6f;

            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out Color color);
            EditorGUI.DrawRect(rect, color);
        }
    }
}
