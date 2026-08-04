// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static EnableStatus  -> DrawNote(Rect, string, ...),  line 3234
//   static AwakeStatus   -> DrawNote(string, ...),        line 3250
//   static DisableStatus -> Separator,                    line 3255
//   static AssetStatus   -> IconSpacer,                   line 3281
//   static InvokeStatus  -> FadeGroup,                    line 2752
//   static VisitStatus   -> HasMouseCapture,              line 3266
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// 2019 vs 2022: DrawNote's guard is rendered `if (show && rect.width > minWidth + indent)` in the
// 2019 build (line 3251) against 2022's `if (show && !(rect.width <= minWidth + indent))`. Same
// condition; the un-negated 2019 form is the one written out below.
//
// Shared with ControllerEditor: EditorUtils.Separators.cs ports the same separator, with an extra
// maxWidth parameter this build does not ship. Deliberately NOT consolidated, on the same basis as
// ADOEditorUtility.Colors.cs.

using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Draws a small italic annotation inside <paramref name="rect"/>, skipping it when the rect
        /// is too narrow to be worth the clutter.
        /// </summary>
        /// <param name="show">Draw at all. Lets a caller gate the note without an <c>if</c>.</param>
        /// <param name="minWidth">Minimum rect width; below <c>minWidth + indent</c> nothing is drawn.</param>
        /// <param name="indent">
        /// Distance from the aligned edge, in pixels. 2.5 further pixels are always added so the
        /// text never sits flush against a neighbouring control.
        /// </param>
        /// <param name="alignLeft">
        /// Anchor to the left edge and indent rightwards; clear it to anchor right and indent
        /// leftwards. Also chooses the default style, since the two differ only in alignment.
        /// </param>
        internal static void DrawNote(Rect rect, string text, bool show = true, float minWidth = 0f, float indent = 0f, bool alignLeft = true, GUIStyle style = null)
        {
            if (!show || rect.width <= minWidth + indent)
            {
                return;
            }

            if (alignLeft)
            {
                rect.x += indent + 2.5f;
            }
            else
            {
                rect.x -= indent + 2.5f;
            }

            GUI.Label(rect, text, style ?? (alignLeft ? styles.noteLeft : styles.noteRight));
        }

        /// <summary>
        /// Draws an annotation over the control just laid out, for labelling a field in place
        /// instead of beside it.
        /// </summary>
        /// <inheritdoc cref="DrawNote(Rect, string, bool, float, float, bool, GUIStyle)"/>
        internal static void DrawNote(string text, bool show = true, float minWidth = 0f, float indent = 0f, bool alignLeft = true)
        {
            DrawNote(GUILayoutUtility.GetLastRect(), text, show, minWidth, indent, alignLeft);
        }

        /// <summary>A horizontal rule, drawn a little wider than the content area it divides.</summary>
        /// <param name="thickness">Height of the rule itself, in pixels.</param>
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

        /// <summary>
        /// Reserves the width of one icon button without drawing anything, so a row with no icon
        /// still lines up with the rows that have one.
        /// </summary>
        internal static void IconSpacer()
        {
            GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
        }

        /// <summary>
        /// Draws <paramref name="content"/> inside a fade group driven by <paramref name="value"/>,
        /// skipping the group entirely when it is fully collapsed.
        /// </summary>
        /// <param name="whileFading">
        /// Extra content drawn only while the group is part-way open. For anything that must not be
        /// visible in the settled open or closed state -- a clipping mask, a spinner.
        /// </param>
        /// <remarks>
        /// The zero check is not just an optimisation: a fully faded-out group still runs its body,
        /// so skipping it avoids laying out and hit-testing hidden controls.
        /// </remarks>
        internal static void FadeGroup(this AnimBool value, Action content, Action whileFading = null)
        {
            if (value.faded == 0f)
            {
                return;
            }

            EditorGUILayout.BeginFadeGroup(value.faded);
            content();

            if (whileFading != null && value.faded > 0f && value.faded < 1f)
            {
                whileFading();
            }

            EditorGUILayout.EndFadeGroup();
        }

        /// <summary>
        /// Claims the mouse for <paramref name="controlID"/> when it is pressed inside
        /// <paramref name="rect"/>, and reports whether that control currently holds it.
        /// </summary>
        /// <returns>
        /// True once the control owns the mouse. Note that the frame the press is captured on still
        /// returns false -- ownership is only visible from the next event onwards, which is what
        /// keeps a caller from treating the press itself as a drag.
        /// </returns>
        internal static bool HasMouseCapture(Rect rect, int controlID)
        {
            if (GUIUtility.hotControl == controlID)
            {
                return true;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
            {
                GUIUtility.hotControl = controlID;
                current.Use();
            }

            return false;
        }
    }
}
