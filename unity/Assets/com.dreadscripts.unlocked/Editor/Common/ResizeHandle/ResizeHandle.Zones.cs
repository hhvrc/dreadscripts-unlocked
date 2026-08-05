// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     ResizeHandle.ResizeZone       -> ResizeZone,       line 534
//     ResizeHandle.DoResizeHandles  -> HandleResize,     line 642
//                                   -> BuildZones,       line 656 (the inline zone array)
//                                   -> TryGetZoneCursor, line 716 (the inline cursor switch)
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//     ResizeHandle.ResizeZone       -> ResizeZone,       line 41
//     ResizeHandle.HandleResize     -> HandleResize,     line 143
//                                   -> BuildZones,       line 165 (the inline zone array)
//                                   -> TryGetZoneCursor, line 342 (the inline cursor switch)
// The zone array and the cursor lookup were statements inside the one method in both sources; they
// are lifted into private helpers here without reordering anything.
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Divergence between the snapshots, all of it in the same unreachable corner of the method: the
// cursor lookup's fall-through path (which the Top and Bottom zones reach) decompiles as `continue`
// in ControllerEditor, as a loop-exiting `break` in ADOverhaul2022, and as "raise onResized and
// abandon the method" in ADOverhaul2019. Only ControllerEditor's is structurally coherent -- it is
// also the only one of the three that ILSpy rendered as a plain `for` loop rather than a
// `while (true)` with gotos -- so it is the one reproduced here. The consequence either way is that
// the Top and Bottom zones are inert: they get no cursor and cannot start a drag. See
// ResizeHandle.Drag.cs for the matching unreachable branch there.
//
// Audit status: VERIFIED -- ResizeZone and all three methods diffed statement by statement against
// both snapshots. All eight zone rects were checked term by term against both (the -thickness
// origins, the +thickness insets, the width/height - band shortenings) and the index each carries
// matches its array slot. The cursor switch was traced through the decompiled binary search in both:
// Left/Right to ResizeHorizontal, TopLeft/BottomRight to ResizeUpLeft, TopRight/BottomLeft to
// ResizeUpRight, Middle and Middle|Right to Arrow, everything else -- which in practice means Top and
// Bottom -- to the skip. The event handling matches too: the pendingReset/MouseUp block before the
// zone loop, the `(zone.position & enabledZones) < zone.position` mask test, the 46f hit-test
// correction applied to a copy while the cursor rect keeps the original, the double-click arming, and
// the drag tail where a zero delta still advances lastMousePosition. The three-way `continue` /
// `break` / abandon divergence is exactly as described above and ControllerEditor's form is the one
// reproduced. Every line number in both MAP blocks was checked against the current snapshot and is
// sound. Unity2022MouseOffset is a named constant for the snapshots' literal 46f.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    internal partial class ResizeHandle
    {
        /// <summary>One grip: the position it resizes, the rect that grabs the mouse, and its index.</summary>
        /// <remarks>
        /// The index duplicates the grip's slot in the array returned by <see cref="BuildZones"/>. It
        /// is stored rather than inferred because the drag phase looks the zone up again by index on a
        /// freshly rebuilt array, after the loop that found it has gone.
        /// </remarks>
        private struct ResizeZone
        {
            internal PositionFlag position;

            internal Rect rect;

            internal int index;
        }

        /// <summary>
        /// The vertical correction applied to a grip rect before hit-testing it on Unity 2022.
        /// </summary>
        /// <remarks>
        /// 2022 puts extra chrome above a scene view's client area, so the event's mouse position and
        /// the rect the GUI was laid out with no longer share an origin. The offset is applied to the
        /// hit-test copy only — the cursor rect is registered against the uncorrected rect, because
        /// the cursor path applies its own correction.
        /// </remarks>
        private const float Unity2022MouseOffset = 46f;

        /// <summary>
        /// Draws the resize grips for <paramref name="rect"/> and processes this frame's event: sets
        /// the hover cursors, starts a drag on mouse-down, applies drag deltas, and resets the size on
        /// a double-click.
        /// </summary>
        /// <param name="rect">The panel rect the grips straddle, in GUI space.</param>
        /// <param name="enabledZones">
        /// Which grips are live. A grip is skipped unless all of its bits are present here, so a
        /// single-bit grip is live exactly when its bit is set. Defaults to horizontal resizing only.
        /// </param>
        /// <param name="anchor">
        /// The panel's anchor, consulted only in <see cref="UniformResize"/> mode to decide which of
        /// the two opposite offsets absorbs the mirrored delta.
        /// </param>
        /// <param name="thickness">
        /// Half the width of a grip band. Each band is <c>thickness * 2</c> across and is centred on
        /// the panel edge, so it is equally grabbable from just inside and just outside the panel.
        /// </param>
        public void HandleResize(Rect rect, PositionFlag enabledZones = PositionFlag.Right | PositionFlag.Left, PositionFlag anchor = PositionFlag.Middle, float thickness = 4f)
        {
            Event current = Event.current;

            // A double-click arms pendingReset on mouse-down; the reset itself waits for the matching
            // mouse-up so that a click-and-drag can still cancel it (see below).
            if (pendingReset && current.type == EventType.MouseUp)
            {
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                }

                ResetSize();
                current.Use();
                pendingReset = false;
            }

            ResizeZone[] zones = BuildZones(rect, thickness);
            bool isLeftButton = current.button == 0;

            foreach (ResizeZone zone in zones)
            {
                if ((zone.position & enabledZones) < zone.position)
                {
                    continue;
                }

                MouseCursor cursor;
                if (!TryGetZoneCursor(zone.position, out cursor))
                {
                    continue;
                }

                // Fully qualified: the cursor-rect deferral and the 2022 version probe were ported as
                // part of ControllerEditor's EditorUtils and have no Common equivalent yet. Going
                // through EditorUtils rather than EditorGUIUtility directly matters, because the grips
                // are drawn inside a scene-view overlay whose cursor rects are deferred and replayed.
                ControllerEditor.EditorUtils.AddCursorRect(zone.rect, cursor);

                Rect hitRect = zone.rect;
                if (ControllerEditor.EditorUtils.isUnity2022)
                {
                    hitRect.y += Unity2022MouseOffset;
                }

                if (isLeftButton && current.type == EventType.MouseDown && hitRect.Contains(current.mousePosition))
                {
                    if (current.clickCount == 2)
                    {
                        pendingReset = true;
                    }

                    activeZoneIndex = zone.index;
                    GUIUtility.hotControl = controlID;
                    lastMousePosition = GUIUtility.GUIToScreenPoint(current.mousePosition);
                    current.Use();
                }
            }

            if (current.type != EventType.MouseDrag || GUIUtility.hotControl != controlID)
            {
                return;
            }

            PositionFlag draggedZone = zones[activeZoneIndex].position;
            Vector2 delta = GUIUtility.GUIToScreenPoint(current.mousePosition) - lastMousePosition;

            if (pendingReset)
            {
                // The double-click's second press armed a reset, but the user is dragging out of it.
                // Swallow the movement until it passes the slop threshold, then treat the gesture as
                // an ordinary drag from here on.
                if (!(delta.sqrMagnitude > ResetCancelDistanceSquared))
                {
                    return;
                }

                pendingReset = false;
            }

            if (delta != Vector2.zero)
            {
                ApplyDragDelta(draggedZone, delta, anchor);
                onResized?.Invoke();
            }

            // Re-read rather than reusing the value the delta was computed from, so a drag that was
            // swallowed above still advances the baseline.
            lastMousePosition = GUIUtility.GUIToScreenPoint(current.mousePosition);
        }

        /// <summary>
        /// Builds the eight grip rects around <paramref name="rect"/>, in the fixed order the drag
        /// phase indexes into: Left, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft.
        /// </summary>
        /// <remarks>
        /// Every band is <c>thickness * 2</c> thick and centred on its edge — the left band starts at
        /// <c>x - thickness</c>, the right band at <c>x + width - thickness</c>, and likewise
        /// vertically. The four side bands are inset by <c>thickness</c> at each end and shortened by
        /// <c>thickness * 2</c> so they meet the corner squares without overlapping them; a corner
        /// therefore always wins over the side it touches, regardless of iteration order.
        /// </remarks>
        private static ResizeZone[] BuildZones(Rect rect, float thickness)
        {
            float band = thickness * 2f;

            return new ResizeZone[8]
            {
                new ResizeZone
                {
                    position = PositionFlag.Left,
                    index = 0,
                    rect = new Rect(rect.x - thickness, rect.y + thickness, band, rect.height - band)
                },
                new ResizeZone
                {
                    position = PositionFlag.TopLeft,
                    index = 1,
                    rect = new Rect(rect.x - thickness, rect.y - thickness, band, band)
                },
                new ResizeZone
                {
                    position = PositionFlag.Top,
                    index = 2,
                    rect = new Rect(rect.x + thickness, rect.y - thickness, rect.width - band, band)
                },
                new ResizeZone
                {
                    position = PositionFlag.TopRight,
                    index = 3,
                    rect = new Rect(rect.x + rect.width - thickness, rect.y - thickness, band, band)
                },
                new ResizeZone
                {
                    position = PositionFlag.Right,
                    index = 4,
                    rect = new Rect(rect.x + rect.width - thickness, rect.y + thickness, band, rect.height - band)
                },
                new ResizeZone
                {
                    position = PositionFlag.BottomRight,
                    index = 5,
                    rect = new Rect(rect.x + rect.width - thickness, rect.y + rect.height - thickness, band, band)
                },
                new ResizeZone
                {
                    position = PositionFlag.Bottom,
                    index = 6,
                    rect = new Rect(rect.x + thickness, rect.y + rect.height - thickness, rect.width - band, band)
                },
                new ResizeZone
                {
                    position = PositionFlag.BottomLeft,
                    index = 7,
                    rect = new Rect(rect.x - thickness, rect.y + rect.height - thickness, band, band)
                }
            };
        }

        /// <summary>
        /// The hover cursor for a grip, or false if the grip is inert and should be skipped entirely —
        /// no cursor, and no chance to begin a drag.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The diagonal cursors are named for the direction they point, so they follow the corner's
        /// diagonal rather than its name: TopLeft and BottomRight both lie on the "\" diagonal and get
        /// <c>ResizeUpLeft</c>, while TopRight and BottomLeft lie on "/" and get <c>ResizeUpRight</c>.
        /// </para>
        /// <para>
        /// Top and Bottom return false. Vertical edge resizing was never wired up: the source's cursor
        /// switch names only the sides, the corners and Middle, and Top and Bottom fall through to a
        /// skip. That is why <see cref="ApplyDragDelta"/>'s vertical branch is unreachable. (The
        /// source reaches Arrow rather than the skip for hypothetical multi-bit values above Bottom,
        /// but no such zone is ever built, so the distinction cannot be observed.)
        /// </para>
        /// </remarks>
        private static bool TryGetZoneCursor(PositionFlag position, out MouseCursor cursor)
        {
            switch (position)
            {
                case PositionFlag.Left:
                case PositionFlag.Right:
                    cursor = MouseCursor.ResizeHorizontal;
                    return true;

                case PositionFlag.TopLeft:
                case PositionFlag.BottomRight:
                    cursor = MouseCursor.ResizeUpLeft;
                    return true;

                case PositionFlag.TopRight:
                case PositionFlag.BottomLeft:
                    cursor = MouseCursor.ResizeUpRight;
                    return true;

                // Middle | Right is listed alongside Middle in both sources. No zone is ever built
                // with that value, so it is carried over rather than reasoned about.
                case PositionFlag.Middle:
                case PositionFlag.Middle | PositionFlag.Right:
                    cursor = MouseCursor.Arrow;
                    return true;

                default:
                    cursor = MouseCursor.Arrow;
                    return false;
            }
        }
    }
}
