// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both, which are identical apart from obfuscated names (see the note on the
// Top/Bottom fall-through in ResizeHandle.Zones.cs for the one place the snapshots disagree):
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     ResizeHandle          -> ResizeHandle,          line 532
//     activeZoneIndex       -> activeZoneIndex,       line 543
//     lastMousePosition     -> lastMousePosition,     line 545
//     controlId             -> controlID,             line 547
//     onResize              -> onResized,             line 549
//     left                  -> leftOffset,            line 551
//     right                 -> rightOffset,           line 553
//     top                   -> topOffset,             line 555
//     bottom                -> bottomOffset,          line 557
//     _RulesObserver        -> uniformResize,         line 559
//     isDoubleClick         -> pendingReset,          line 561
//     ResolveError          -> UniformResize (getter), line 564
//     ListError             -> UniformResize (setter), line 570
//     .ctor                 -> .ctor,                 line 608
//     Reset                 -> ResetSize,             line 613
//     Apply                 -> GetResizedRect,        line 622
//     GetHorizontalPivot    -> GetHorizontalPivot,    line 928
//     GetVerticalPivot      -> GetVerticalPivot,      line 955
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//     ResizeHandle          -> ResizeHandle,          line 39
//     activeZoneIndex       -> activeZoneIndex,       line 50
//     lastMousePosition     -> lastMousePosition,     line 52
//     controlID             -> controlID,             line 54
//     onResized             -> onResized,             line 56
//     leftOffset            -> leftOffset,            line 58
//     rightOffset           -> rightOffset,           line 60
//     topOffset             -> topOffset,             line 62
//     bottomOffset          -> bottomOffset,          line 64
//     uniformResize         -> uniformResize,         line 66
//     pendingReset          -> pendingReset,          line 68
//     GetUniformResize      -> UniformResize (getter), line 71
//     SetUniformResize      -> UniformResize (setter), line 77
//     .ctor                 -> .ctor,                 line 109
//     ResetSize             -> ResetSize,             line 114
//     GetResizedRect        -> GetResizedRect,        line 123
//     GetHorizontalPivot    -> GetHorizontalPivot,    line 446
//     GetVerticalPivot      -> GetVerticalPivot,      line 473
// The four anchor predicates this file's pivot helpers call were free-standing extension methods on
// PositionFlag in both sources, declared far outside this type's body and shared with the
// panel-layout code. An earlier revision of this port folded them in here as private statics; they
// now live in PositionFlagExtensions.cs as extensions, which is the shipped shape, and are called
// from here as such. Behaviour is unchanged -- the bodies moved verbatim.
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Split across ResizeHandle.cs (state and rect maths), ResizeHandle.Zones.cs (grip hit-testing and
// cursors) and ResizeHandle.Drag.cs (per-grip offset maths).

using System;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// The drag-to-resize grips drawn around a floating scene-view panel: eight zones straddling the
    /// panel's edges and corners, plus the accumulated size offsets they produce.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The handle does not own the panel rect. It accumulates four independent offsets — how far each
    /// side has been dragged outward — and <see cref="GetResizedRect"/> applies them to whatever base
    /// rect the caller passes in each frame. Positive means "grow on that side", so the resized size
    /// is <c>width + leftOffset + rightOffset</c> by <c>height + topOffset + bottomOffset</c>. Keeping
    /// the offsets rather than a rect is what lets the panel keep tracking a layout-driven base
    /// position while still honouring the user's manual resize.
    /// </para>
    /// <para>
    /// All drag arithmetic is done in <em>screen</em> space, not GUI space: the panel's own GUI origin
    /// moves as it is resized, so a GUI-space delta would feed back into itself. Screen space shares
    /// GUI space's y-down orientation, so the sign conventions below are the same in both.
    /// </para>
    /// </remarks>
    internal partial class ResizeHandle
    {
        /// <summary>Index into the zone array built each frame; identifies the grip being dragged.</summary>
        private int activeZoneIndex;

        /// <summary>Last observed mouse position, in screen space. See the class remarks.</summary>
        private Vector2 lastMousePosition = Vector2.zero;

        /// <summary>
        /// The control ID that marks this handle as the active drag target.
        /// </summary>
        /// <remarks>
        /// Seeded from a fixed string hash rather than left to Unity's automatic numbering, so the ID
        /// stays the same across the frames of a drag even though the handle is not drawn through the
        /// usual per-control ordering. Note that this is a field initialiser: a handle must therefore
        /// be constructed from inside a GUI callback, where <c>GUIUtility.GetControlID</c> is legal.
        /// </remarks>
        private readonly int controlID = GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), FocusType.Passive);

        /// <summary>Raised whenever the offsets change, including on reset.</summary>
        public Action onResized;

        /// <summary>How far the left edge has been dragged outward, in pixels.</summary>
        public float leftOffset;

        /// <summary>How far the right edge has been dragged outward, in pixels.</summary>
        public float rightOffset;

        /// <summary>How far the top edge has been dragged outward (upward), in pixels.</summary>
        public float topOffset;

        /// <summary>How far the bottom edge has been dragged outward (downward), in pixels.</summary>
        public float bottomOffset;

        private bool uniformResize;

        /// <summary>
        /// Set on a double-click, cleared either by the matching mouse-up (which resets the size) or
        /// by dragging far enough that the gesture is clearly a drag rather than a double-click.
        /// </summary>
        private bool pendingReset;

        /// <summary>
        /// Squared distance the mouse must travel after a double-click before the pending reset is
        /// abandoned. Corresponds to a diagonal of 15 pixels on each axis.
        /// </summary>
        private const float ResetCancelDistanceSquared = 450f;

        /// <summary>The minimum width and height <see cref="GetResizedRect"/> will clamp down to.</summary>
        private const float MinimumSize = 10f;

        /// <param name="uniformResize">Initial value of <see cref="UniformResize"/>.</param>
        public ResizeHandle(bool uniformResize = false)
        {
            // Assigns the field directly, not the property: at construction there is nothing to
            // reconcile, and the property's reconciliation would be a no-op on all-zero offsets
            // anyway.
            this.uniformResize = uniformResize;
        }

        /// <summary>
        /// Whether dragging one axis drives the other, so the panel keeps its proportions.
        /// </summary>
        /// <remarks>
        /// Turning this on mid-session has to reconcile four offsets that were free to disagree. The
        /// source picks a single "leader" in a fixed priority — left, then right, then top, then
        /// bottom — and copies it onto its opposite number, leaving the other axis alone. So a panel
        /// widened only from the left comes out symmetric horizontally and untouched vertically. The
        /// setter is inert unless the value actually changes and unless it is being turned on.
        /// </remarks>
        public bool UniformResize
        {
            get
            {
                return uniformResize;
            }
            set
            {
                if (uniformResize == value)
                {
                    return;
                }

                uniformResize = value;
                if (!value)
                {
                    return;
                }

                if (leftOffset != 0f)
                {
                    rightOffset = leftOffset;
                }
                else if (rightOffset != 0f)
                {
                    leftOffset = rightOffset;
                }
                else if (topOffset != 0f)
                {
                    bottomOffset = topOffset;
                }
                else if (bottomOffset != 0f)
                {
                    topOffset = bottomOffset;
                }
            }
        }

        /// <summary>Clears all four offsets, returning the panel to its layout-driven size.</summary>
        public void ResetSize()
        {
            leftOffset = 0f;
            rightOffset = 0f;
            topOffset = 0f;
            bottomOffset = 0f;
            onResized?.Invoke();
        }

        /// <summary>
        /// Applies the accumulated offsets to <paramref name="rect"/>, holding the point named by
        /// <paramref name="anchor"/> fixed and optionally keeping the result inside
        /// <paramref name="bounds"/>.
        /// </summary>
        /// <param name="anchor">
        /// Which point of the rect stays put as it grows. The pivot derived from it is 0 at the
        /// left/top edge, 1 at the right/bottom edge and 0.5 for anything else, and the rect's origin
        /// is pulled back by <c>sizeDelta * pivot</c> — so anchoring right keeps the right edge still
        /// and grows leftward, anchoring left grows rightward, and the default grows both ways.
        /// </param>
        /// <param name="bounds">
        /// Containing rect to clamp against. The two axes are opted into independently, and the opt-in
        /// is by sentinel rather than by nullable: an axis is clamped only when neither its position
        /// nor its size is -1. Passing <c>default(Rect)</c> (the default) is rewritten to
        /// (-1, -1, -1, -1) and so clamps nothing.
        /// </param>
        /// <remarks>
        /// The order matters and is load-bearing: <c>rect.x</c> is clamped first and the width clamp
        /// then reads the already-clamped <c>rect.x</c>, and likewise for y and height. Note also that
        /// the size limits read <c>bounds.width - rect.x</c> rather than
        /// <c>bounds.x + bounds.width - rect.x</c>; that looks like an oversight in the original — it
        /// only behaves as intended for a bounds rect at the origin — but both products compute it the
        /// same way, so it is reproduced rather than corrected.
        /// </remarks>
        public Rect GetResizedRect(Rect rect, PositionFlag anchor = PositionFlag.Middle, Rect bounds = default(Rect))
        {
            if (bounds == default(Rect))
            {
                bounds = new Rect(-1f, -1f, -1f, -1f);
            }

            bool clampHorizontally = bounds.x != -1f && bounds.width != -1f;
            bool clampVertically = bounds.y != -1f && bounds.height != -1f;

            float width = rect.width + leftOffset + rightOffset;
            float height = rect.height + topOffset + bottomOffset;
            float x = rect.x - (width - rect.width) * GetHorizontalPivot(anchor);
            float y = rect.y - (height - rect.height) * GetVerticalPivot(anchor);

            rect.x = clampHorizontally ? Mathf.Clamp(x, bounds.x, bounds.x + bounds.width - MinimumSize) : x;
            rect.width = clampHorizontally ? Mathf.Clamp(width, MinimumSize, bounds.width - rect.x) : width;
            rect.y = clampVertically ? Mathf.Clamp(y, bounds.y, bounds.y + bounds.height - MinimumSize) : y;
            rect.height = clampVertically ? Mathf.Clamp(height, MinimumSize, bounds.height - rect.y) : height;
            return rect;
        }

        /// <summary>
        /// The normalised x of the point an <paramref name="anchor"/> pins: 0 at the left edge, 1 at
        /// the right edge, 0.5 when the anchor names neither side.
        /// </summary>
        /// <param name="mirrored">Return the opposite edge's pivot instead, leaving 0.5 unchanged.</param>
        public static float GetHorizontalPivot(PositionFlag anchor, bool mirrored = false)
        {
            if (!mirrored)
            {
                if (anchor.IsAnchoredRight())
                {
                    return 1f;
                }

                if (anchor.IsAnchoredLeft())
                {
                    return 0f;
                }
            }
            else
            {
                if (anchor.IsAnchoredRight())
                {
                    return 0f;
                }

                if (anchor.IsAnchoredLeft())
                {
                    return 1f;
                }
            }

            return 0.5f;
        }

        /// <summary>
        /// The normalised y of the point an <paramref name="anchor"/> pins: 0 at the top edge, 1 at
        /// the bottom edge, 0.5 when the anchor names neither side. Remember that GUI space is y-down,
        /// so pivot 1 is the visually lower edge.
        /// </summary>
        /// <param name="mirrored">Return the opposite edge's pivot instead, leaving 0.5 unchanged.</param>
        /// <remarks>
        /// Bottom is tested before top, so an anchor that somehow claimed both would resolve as bottom.
        /// The horizontal counterpart resolves the same conflict in favour of right.
        /// </remarks>
        public static float GetVerticalPivot(PositionFlag anchor, bool mirrored = false)
        {
            bool top = anchor.IsAnchoredTop();
            bool bottom = anchor.IsAnchoredBottom();

            if (!mirrored)
            {
                if (bottom)
                {
                    return 1f;
                }

                if (top)
                {
                    return 0f;
                }
            }
            else
            {
                if (top)
                {
                    return 1f;
                }

                if (bottom)
                {
                    return 0f;
                }
            }

            return 0.5f;
        }
    }
}
