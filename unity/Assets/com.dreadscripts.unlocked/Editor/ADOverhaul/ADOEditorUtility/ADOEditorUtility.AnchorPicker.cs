// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static RunStatus -> AnchorPicker, line 2539
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// The PositionFlag enum and its four anchor predicates are shared with ControllerEditor and live in
// DreadScripts.Common (ResizeHandle/PositionFlag.cs and ResizeHandle/PositionFlagExtensions.cs);
// this file only draws the picker for them. ControllerEditor ships no equivalent picker.
//
// Both call sites are the scene-view overlay alignment setting in
// decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, lines 3538 and 8285, which store
// the result as an int on ADOSettings.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Draws a 3x3 grid of cells, one per corner/edge/centre anchor, and returns the anchor the
        /// pointer is over -- or <paramref name="current"/> when it is over none of them.
        /// </summary>
        /// <param name="current">The anchor to keep when nothing is hovered.</param>
        /// <param name="rect">The whole grid; each cell is a third of it on each axis.</param>
        /// <param name="selectable">
        /// Which anchors may be picked. The rest are still drawn, tinted red, so the user can see
        /// that the position exists but is unavailable rather than wondering where it went.
        /// </param>
        /// <remarks>
        /// <para>
        /// Selection follows the pointer rather than a click, and only on Repaint. The caller writes
        /// the returned value straight back to its setting, so hovering a cell selects it and moving
        /// off the grid leaves the last hovered one in place -- there is no commit step.
        /// </para>
        /// <para>
        /// Composite members of the enum are skipped: only values with exactly one bit set get a
        /// cell, which is what <c>value != 0 &amp;&amp; (value &amp; (value - 1)) == 0</c> tests.
        /// Without it <c>All</c> would draw a tenth cell on top of the others.
        /// </para>
        /// </remarks>
        internal static PositionFlag AnchorPicker(PositionFlag current, Rect rect, PositionFlag selectable = PositionFlag.All)
        {
            AddCursorRect(rect, MouseCursor.Pan);

            float cellWidth = rect.width / 3f;
            float cellHeight = rect.height / 3f;

            foreach (PositionFlag anchor in PositionFlag.All.GetFlags())
            {
                if (anchor == (PositionFlag)0 || (anchor & (anchor - 1)) != 0)
                {
                    continue;
                }

                Rect cell = rect;

                if (anchor.IsAnchoredRight())
                {
                    cell.x += cellWidth * 2f;
                }
                else if (!anchor.IsAnchoredLeft())
                {
                    cell.x += cellWidth;
                }

                if (anchor.IsAnchoredBottom())
                {
                    cell.y += cellHeight * 2f;
                }
                else if (!anchor.IsAnchoredTop())
                {
                    cell.y += cellHeight;
                }

                cell.width = cellWidth;
                cell.height = cellHeight;

                // The outline is inset by half the border width on each side so neighbouring cells
                // share one grid line instead of drawing two side by side.
                Rect outline = cell;
                outline.x += 1.5f;
                outline.y += 1.5f;
                outline.width -= 3f;
                outline.height -= 3f;
                DrawRoundedBox(outline, Color.clear, Color.grey);

                if (!selectable.HasFlag(anchor))
                {
                    DrawRoundedBox(cell, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
                }
                else if (Event.current.type == EventType.Repaint)
                {
                    if (cell.Contains(Event.current.mousePosition))
                    {
                        current = anchor;
                        DrawRoundedBox(cell, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
                    }
                    else
                    {
                        DrawRoundedBox(cell, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
                    }
                }
            }

            return current;
        }
    }
}
