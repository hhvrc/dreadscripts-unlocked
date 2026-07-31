// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of these extensions.
// Reconstructed from both, which are identical apart from obfuscated names:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     PrintPage       -> IsAnchoredRight,  line 2252
//     SearchPage      -> IsAnchoredLeft,   line 2261
//     RevertPage      -> IsAnchoredTop,    line 2270
//     OrderResolver   -> IsAnchoredBottom, line 2279
//     CompareResolver -> GetResizeEdges,   line 2287
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//     CountProcess    -> IsAnchoredRight,  line 2133
//     StartProcess    -> IsAnchoredLeft,   line 2142
//     RemoveProcess   -> IsAnchoredTop,    line 2151
//     ReflectProcess  -> IsAnchoredBottom, line 2160
//     ResolveProcess  -> GetResizeEdges,   line 2168
// The ADOverhaul2019 build declares the same five methods at lines 2135-2171
// (PrintAccount / FindAccount / CollectAccount / ValidateAccount / RestartAccount) with identical
// bodies; there is no divergence between any of the three snapshots. The only textual difference is
// that ILSpy rendered the right-edge and bottom-edge predicates with their first two tests negated
// in the ADOverhaul snapshots and un-negated in ControllerEditor's -- the same boolean either way.
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// All five were free-standing extension methods on PositionFlag in the shipped assemblies, declared
// far outside ResizeHandle's body and shared between the resize code and the panel-layout code, so
// they are ported as extensions here rather than folded into either caller.

namespace DreadScripts.Common
{
    /// <summary>
    /// Queries over a <see cref="PositionFlag"/> used as an anchor: which edges of the rect it pins,
    /// and which edges are therefore free to be dragged.
    /// </summary>
    internal static class PositionFlagExtensions
    {
        /// <summary>
        /// Whether <paramref name="anchor"/> lies anywhere along the right edge. The corners are their
        /// own bits rather than <c>Top | Right</c>, so each has to be listed explicitly.
        /// </summary>
        internal static bool IsAnchoredRight(this PositionFlag anchor)
        {
            if (anchor.HasFlag(PositionFlag.Right) || anchor.HasFlag(PositionFlag.TopRight))
            {
                return true;
            }

            return anchor.HasFlag(PositionFlag.BottomRight);
        }

        /// <inheritdoc cref="IsAnchoredRight"/>
        internal static bool IsAnchoredLeft(this PositionFlag anchor)
        {
            if (anchor.HasFlag(PositionFlag.Left) || anchor.HasFlag(PositionFlag.TopLeft))
            {
                return true;
            }

            return anchor.HasFlag(PositionFlag.BottomLeft);
        }

        /// <inheritdoc cref="IsAnchoredRight"/>
        internal static bool IsAnchoredTop(this PositionFlag anchor)
        {
            if (anchor.HasFlag(PositionFlag.Top) || anchor.HasFlag(PositionFlag.TopLeft))
            {
                return true;
            }

            return anchor.HasFlag(PositionFlag.TopRight);
        }

        /// <inheritdoc cref="IsAnchoredRight"/>
        internal static bool IsAnchoredBottom(this PositionFlag anchor)
        {
            if (anchor.HasFlag(PositionFlag.Bottom) || anchor.HasFlag(PositionFlag.BottomLeft))
            {
                return true;
            }

            return anchor.HasFlag(PositionFlag.BottomRight);
        }

        /// <summary>
        /// The set of edges a panel anchored at <paramref name="anchor"/> can be resized from: every
        /// edge the anchor does not pin.
        /// </summary>
        /// <param name="horizontalOnly">Drop the top and bottom edges from the result.</param>
        /// <param name="verticalOnly">Drop the right and left edges from the result.</param>
        /// <remarks>
        /// <para>
        /// A pinned edge cannot be a grip, because dragging it would move the very edge the anchor is
        /// holding still. So an edge anchor frees the other three edges and a corner anchor frees the
        /// two edges opposite it — <see cref="PositionFlag.BottomRight"/> yields
        /// <c>Left | Top</c>, and so on.
        /// </para>
        /// <para>
        /// The switch matches whole values, not bits: a composite anchor such as
        /// <c>Top | Left</c> — as distinct from the single <see cref="PositionFlag.TopLeft"/> bit —
        /// falls through to <see cref="PositionFlag.Middle"/>, which enables no grips at all. That is
        /// the shipped behaviour in all three snapshots and matches how the enum is used elsewhere,
        /// where an anchor is always one of the nine named positions.
        /// </para>
        /// <para>
        /// The two filters are applied after the switch rather than folded into it, so passing both
        /// leaves <see cref="PositionFlag.Middle"/> set or clear exactly as the switch left it.
        /// </para>
        /// </remarks>
        public static PositionFlag GetResizeEdges(this PositionFlag anchor, bool horizontalOnly = false, bool verticalOnly = false)
        {
            PositionFlag edges;
            switch (anchor)
            {
                case PositionFlag.Right:
                    edges = PositionFlag.Left | PositionFlag.Top | PositionFlag.Bottom;
                    break;
                case PositionFlag.Left:
                    edges = PositionFlag.Right | PositionFlag.Top | PositionFlag.Bottom;
                    break;
                case PositionFlag.Top:
                    edges = PositionFlag.Right | PositionFlag.Left | PositionFlag.Bottom;
                    break;
                case PositionFlag.Bottom:
                    edges = PositionFlag.Right | PositionFlag.Left | PositionFlag.Top;
                    break;
                case PositionFlag.TopRight:
                    edges = PositionFlag.Left | PositionFlag.Bottom;
                    break;
                case PositionFlag.TopLeft:
                    edges = PositionFlag.Right | PositionFlag.Bottom;
                    break;
                case PositionFlag.BottomRight:
                    edges = PositionFlag.Left | PositionFlag.Top;
                    break;
                case PositionFlag.BottomLeft:
                    edges = PositionFlag.Right | PositionFlag.Top;
                    break;
                default:
                    edges = PositionFlag.Middle;
                    break;
            }

            if (horizontalOnly)
            {
                edges &= ~(PositionFlag.Top | PositionFlag.Bottom);
            }

            if (verticalOnly)
            {
                edges &= ~(PositionFlag.Right | PositionFlag.Left);
            }

            return edges;
        }
    }
}
