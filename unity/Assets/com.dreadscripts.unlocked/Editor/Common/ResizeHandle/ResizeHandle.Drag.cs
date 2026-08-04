// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs:
//     ResizeHandle.DoResizeHandles -> ApplyDragDelta, line 794
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs:
//     ResizeHandle.HandleResize    -> ApplyDragDelta, line 243
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// NOTES
// Each cited line is the head of the drag-dispatch block inside that resize method -- the
// `if (vector != Vector2.zero)` guard in ControllerEditor, the `if (position > PositionFlag.Bottom)`
// switch in ADOverhaul2022. Both sources inline the block into the one resize method; it is lifted
// into a private helper here without reordering anything.
//
// The six reachable grips (Left, Right and the four corners) are byte-for-byte identical across
// ControllerEditor, ADOverhaul2022 and ADOverhaul2019 once the obfuscated names are resolved,
// including the two quirks called out below. The unreachable vertical branch is the one place the
// three snapshots disagree -- see the remarks on that branch.
//
// Audit status: PARTIAL -- both source methods and both line numbers were re-checked against
// decompiled/ on 2026-08-05 (DoResizeHandles at EditorUtils.cs 642, HandleResize at
// ADOEditorUtility.cs 143); the three-way comparison of the branch bodies asserted below was taken
// from the earlier port and not re-derived.

using UnityEngine;

namespace DreadScripts.Common
{
    internal partial class ResizeHandle
    {
        /// <summary>
        /// Folds one frame of mouse movement into the four side offsets, according to which grip is
        /// being dragged.
        /// </summary>
        /// <param name="zone">The grip under the drag.</param>
        /// <param name="delta">
        /// Screen-space mouse movement since the last frame. Screen space is y-down like GUI space, so
        /// a positive <c>delta.y</c> means the mouse moved down the screen.
        /// </param>
        /// <param name="anchor">
        /// The panel's anchor. Read only in <see cref="UniformResize"/> mode, to pick which of a pair
        /// of opposite offsets absorbs the mirrored movement — the rule is always "put it on the side
        /// away from the anchor", so that mirroring grows the panel rather than sliding it.
        /// </param>
        /// <remarks>
        /// <para>
        /// Sign conventions. Each offset measures outward growth on its own side, so the right and
        /// bottom offsets take the delta directly (<c>+= delta.x</c>, <c>+= delta.y</c>) while the left
        /// and top offsets take it negated (<c>-= delta.x</c>, <c>-= delta.y</c>): dragging the left
        /// grip leftward gives a negative <c>delta.x</c>, and negating it grows the panel to the left.
        /// A corner combines its horizontal rule with its vertical one.
        /// </para>
        /// <para>
        /// Two quirks are reproduced deliberately, because all three shipped builds agree on them and
        /// there is no way to tell an intentional feel from a slip. First, the uniform-mode mirrors
        /// mostly drive the vertical offsets from the <em>horizontal</em> movement (<c>delta.x</c>),
        /// which is what keeps the panel proportional when only the width is being dragged. Second,
        /// the TopRight grip breaks that pattern: its uniform branch adjusts a <em>horizontal</em>
        /// offset by <c>delta.y</c>, so the right offset can be written twice in one frame. This reads
        /// like a copy-paste slip in the original, but it is the shipped behaviour.
        /// </para>
        /// </remarks>
        private void ApplyDragDelta(PositionFlag zone, Vector2 delta, PositionFlag anchor)
        {
            // Kept as the source's two-level test rather than one flat switch: the corner bits all sit
            // above Bottom, and a multi-bit value above Bottom must fall through doing nothing rather
            // than landing in the vertical default below.
            if (zone > PositionFlag.Bottom)
            {
                switch (zone)
                {
                    case PositionFlag.TopRight:
                        rightOffset += delta.x;
                        if (!UniformResize)
                        {
                            topOffset -= delta.y;
                        }
                        else if (!anchor.HasFlag(PositionFlag.Left))
                        {
                            leftOffset -= delta.y;
                        }
                        else
                        {
                            rightOffset -= delta.y;
                        }

                        break;

                    case PositionFlag.TopLeft:
                        leftOffset -= delta.x;
                        if (!UniformResize)
                        {
                            topOffset -= delta.y;
                        }
                        else if (!anchor.HasFlag(PositionFlag.Bottom))
                        {
                            bottomOffset -= delta.x;
                        }
                        else
                        {
                            topOffset -= delta.x;
                        }

                        break;

                    case PositionFlag.BottomRight:
                        rightOffset += delta.x;
                        if (UniformResize)
                        {
                            if (anchor.HasFlag(PositionFlag.Top))
                            {
                                bottomOffset += delta.x;
                            }
                            else
                            {
                                topOffset += delta.x;
                            }
                        }
                        else
                        {
                            bottomOffset += delta.y;
                        }

                        break;

                    case PositionFlag.BottomLeft:
                        leftOffset -= delta.x;
                        if (!UniformResize)
                        {
                            bottomOffset += delta.y;
                        }
                        else if (anchor.HasFlag(PositionFlag.Bottom))
                        {
                            topOffset += delta.x;
                        }
                        else
                        {
                            bottomOffset += delta.x;
                        }

                        break;
                }

                return;
            }

            switch (zone)
            {
                case PositionFlag.Left:
                    leftOffset -= delta.x;
                    if (UniformResize)
                    {
                        if (anchor.HasFlag(PositionFlag.Bottom))
                        {
                            topOffset -= delta.x;
                        }
                        else
                        {
                            bottomOffset -= delta.x;
                        }
                    }

                    break;

                case PositionFlag.Right:
                    rightOffset += delta.x;
                    if (UniformResize)
                    {
                        if (anchor.HasFlag(PositionFlag.Bottom))
                        {
                            topOffset += delta.x;
                        }
                        else
                        {
                            bottomOffset += delta.x;
                        }
                    }

                    break;

                // Dragging the middle resizes nothing; the caller still gets its onResized callback.
                case PositionFlag.Middle:
                case PositionFlag.Middle | PositionFlag.Right:
                    break;

                default:
                    // Unreachable in practice: the Top and Bottom grips are skipped during hit-testing
                    // (see TryGetZoneCursor), so neither can ever become the active zone, and no other
                    // value at or below Bottom is ever built into a zone. The three snapshots
                    // decompile this dead branch three different ways -- ControllerEditor as the two
                    // statements below, ADOverhaul2022 as nothing at all, ADOverhaul2019 as a bare
                    // "bottomOffset -= delta.x" that is really the tail it shares with TopLeft's
                    // uniform branch. ControllerEditor's is the only reading that makes sense as
                    // source (grow downward from a vertical drag, mirroring horizontally in uniform
                    // mode), so it is the one kept.
                    bottomOffset += delta.y;
                    if (UniformResize)
                    {
                        if (anchor.HasFlag(PositionFlag.Left))
                        {
                            rightOffset += delta.y;
                        }
                        else
                        {
                            leftOffset += delta.y;
                        }
                    }

                    break;
            }
        }
    }
}
