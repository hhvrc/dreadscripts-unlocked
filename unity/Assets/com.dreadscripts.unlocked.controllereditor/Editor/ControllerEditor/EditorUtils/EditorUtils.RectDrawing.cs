// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PostResolver -> DrawRoundedRect, line 2369
//   static ResetRules   -> AnchorPicker,    line 4871
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// DrawRoundedRect leans on the six-argument GUI.DrawTexture Unity added in 2019.1, which takes a
// border width and a corner radius and does the rounding on the GPU. It is fed a 1x1 texture of the
// wanted colour (SharedColorTexture, EditorUtils.Textures.cs) plus the same colour again as the
// tint, which is how a solid rounded rectangle is drawn without a sprite.
//
// AnchorPicker is the nine-cell anchor grid: a 3x3 of cells, each standing for one PositionFlag
// corner/edge/centre, with the hovered one highlighted and the disallowed ones tinted red. It is
// the only consumer of DrawRoundedRect in the class.

using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Draws a rounded-cornered rectangle: <paramref name="fill"/> inside
        /// <paramref name="rect"/>, and <paramref name="border"/> as an outline around it.
        /// </summary>
        /// <param name="borderWidth">
        /// Thickness of the outline. The outline is drawn on a rect grown by this plus 2 on each
        /// side, so it sits outside <paramref name="rect"/> rather than eating into it.
        /// </param>
        /// <returns>
        /// The content rect: <paramref name="rect"/> inset by 4 on every side, ready to draw into.
        /// </returns>
        /// <remarks>
        /// Either colour may be <see cref="Color.clear"/> to skip that half. The corner radius is
        /// fixed at 8.
        /// </remarks>
        internal static Rect DrawRoundedRect(Rect rect, Color fill, Color border, float borderWidth = 3f)
        {
            float grow = borderWidth + 2f;
            Rect borderRect = rect;
            borderRect.x -= grow / 2f;
            borderRect.width += grow;
            borderRect.y -= grow / 2f;
            borderRect.height += grow;

            if (fill != Color.clear)
            {
                GUI.DrawTexture(rect, SharedColorTexture(fill), ScaleMode.StretchToFill, true, 0f, fill, 0f, 8f);
            }

            if (border != Color.clear)
            {
                GUI.DrawTexture(borderRect, SharedColorTexture(border), ScaleMode.StretchToFill, true, 0f, border,
                    borderWidth, 8f);
            }

            Rect content = rect;
            content.x += 4f;
            content.width -= 8f;
            content.y += 4f;
            content.height -= 8f;
            return content;
        }

        /// <summary>
        /// Draws a 3x3 anchor grid in <paramref name="rect"/> and returns the anchor the mouse is
        /// over, or <paramref name="current"/> if it is over none.
        /// </summary>
        /// <param name="allowed">
        /// Which anchors may be picked. A disallowed cell is tinted red and cannot be hovered into
        /// the result.
        /// </param>
        /// <remarks>
        /// Only the single-bit members of PositionFlag get a cell: the enumeration skips zero and
        /// any value with more than one bit set (<c>(f &amp; (f - 1)) != 0</c>), which is how the
        /// composite members such as All are excluded.
        /// <para>
        /// The hover test runs on Repaint only, so the returned value updates when the view redraws
        /// rather than on the mouse-move event itself. The caller is expected to be repainting
        /// continuously while this is on screen.
        /// </para>
        /// </remarks>
        internal static PositionFlag AnchorPicker(PositionFlag current, Rect rect,
            PositionFlag allowed = PositionFlag.All)
        {
            AddCursorRect(rect, MouseCursor.Pan);

            float cellWidth = rect.width / 3f;
            float cellHeight = rect.height / 3f;

            foreach (PositionFlag flag in PositionFlag.All.GetFlags())
            {
                if (flag == 0 || (flag & (flag - 1)) != 0)
                {
                    continue;
                }

                Rect cell = rect;
                if (flag.IsAnchoredRight())
                {
                    cell.x += cellWidth * 2f;
                }
                else if (!flag.IsAnchoredLeft())
                {
                    cell.x += cellWidth;
                }

                if (flag.IsAnchoredBottom())
                {
                    cell.y += cellHeight * 2f;
                }
                else if (!flag.IsAnchoredTop())
                {
                    cell.y += cellHeight;
                }

                cell.width = cellWidth;
                cell.height = cellHeight;

                const float outline = 3f;
                Rect inner = cell;
                inner.x += outline / 2f;
                inner.y += outline / 2f;
                inner.width -= outline;
                inner.height -= outline;
                DrawRoundedRect(inner, Color.clear, Color.grey);

                if (!allowed.HasFlag(flag))
                {
                    DrawRoundedRect(cell, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
                }
                else if (Event.current.type == EventType.Repaint)
                {
                    if (cell.Contains(Event.current.mousePosition))
                    {
                        current = flag;
                        DrawRoundedRect(cell, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
                    }
                    else
                    {
                        DrawRoundedRect(cell, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
                    }
                }
            }

            return current;
        }
    }
}
