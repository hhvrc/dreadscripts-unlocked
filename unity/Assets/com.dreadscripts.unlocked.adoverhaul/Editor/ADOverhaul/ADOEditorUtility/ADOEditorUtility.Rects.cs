// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static SortStatus   -> SliceLeft,      line 2737
//   static ResetProcess -> DrawRoundedBox(Rect, float),                      line 2245
//   static GetProcess   -> DrawRoundedBox(Rect, Color, Color, float),        line 2250
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above.
//
// Shared with ControllerEditor: EditorUtils.Rects.cs ports the identical SliceLeft, under the same
// name and with the same parameter list, plus SliceRight/SliceTop and the Expand/With family that
// this build does not ship. The rounded-box drawer has no ControllerEditor counterpart.
// Deliberately NOT consolidated, on the same basis as ADOEditorUtility.Colors.cs.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Carves a column off the left of <paramref name="rect"/> and returns it, shrinking
        /// <paramref name="rect"/> by the same amount so the next call continues where this one
        /// stopped.
        /// </summary>
        /// <param name="amount">Column width, as a percentage of the current width unless <paramref name="absolute"/> is set.</param>
        /// <param name="absolute">Treat <paramref name="amount"/> as pixels rather than a percentage.</param>
        /// <param name="offset">Where the column starts; -1 means "at the current left edge".</param>
        /// <param name="offsetAbsolute">Treat <paramref name="offset"/> as an absolute x rather than a percentage of the width.</param>
        /// <param name="consume">
        /// Advance <paramref name="rect"/> past the column. Clear it to peek at a column without
        /// taking it -- used to overlay something on a row that a later call will still lay out.
        /// </param>
        internal static Rect SliceLeft(this ref Rect rect, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            Rect result = rect;
            result.width = absolute ? amount : (amount * rect.width / 100f);
            result.height = rect.height;
            result.x = (offset == -1f) ? rect.x : (offsetAbsolute ? offset : (rect.x + offset * rect.width / 100f));
            result.y = rect.y;

            if (consume)
            {
                rect.x = result.x + result.width;
                rect.width -= result.width;
            }

            return result;
        }

        /// <summary>
        /// Draws the tool's standard rounded panel background into <paramref name="rect"/> and
        /// returns the area left for content.
        /// </summary>
        /// <remarks>
        /// The two default colours are a near-black fill and the slightly lighter grey the editor
        /// uses for a raised surface, both at half alpha so whatever is behind still shows through.
        /// </remarks>
        internal static Rect DrawRoundedBox(Rect rect, float borderWidth = 2f)
        {
            return DrawRoundedBox(rect, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), borderWidth);
        }

        /// <summary>
        /// Draws a rounded rectangle: <paramref name="fill"/> inside <paramref name="rect"/> and
        /// <paramref name="border"/> as a ring around it, and returns the area left for content.
        /// </summary>
        /// <param name="borderWidth">
        /// Thickness of the ring. The ring is drawn into a rect grown by <c>borderWidth + 2</c> on
        /// each axis and centred on <paramref name="rect"/>, so it sits half outside and half over
        /// the fill's edge.
        /// </param>
        /// <returns><paramref name="rect"/> inset by 4 pixels on every side.</returns>
        /// <remarks>
        /// Both passes go through <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, float, float)"/>
        /// with a corner radius of 8 and a 1x1 texture, which is what gives the rounded corners
        /// without an artwork asset. A colour of <see cref="Color.clear"/> skips its pass, so the
        /// same method draws a fill only, a border only, or both.
        /// </remarks>
        internal static Rect DrawRoundedBox(Rect rect, Color fill, Color border, float borderWidth = 3f)
        {
            float grow = borderWidth + 2f;

            Rect borderRect = rect;
            borderRect.x -= grow / 2f;
            borderRect.width += grow;
            borderRect.y -= grow / 2f;
            borderRect.height += grow;

            if (fill != Color.clear)
            {
                GUI.DrawTexture(rect, SolidColorTexture(fill), ScaleMode.StretchToFill, alphaBlend: true, 0f, fill, 0f, 8f);
            }

            if (border != Color.clear)
            {
                GUI.DrawTexture(borderRect, SolidColorTexture(border), ScaleMode.StretchToFill, alphaBlend: true, 0f, border, borderWidth, 8f);
            }

            Rect content = rect;
            content.x += 4f;
            content.width -= 8f;
            content.y += 4f;
            content.height -= 8f;
            return content;
        }
    }
}
