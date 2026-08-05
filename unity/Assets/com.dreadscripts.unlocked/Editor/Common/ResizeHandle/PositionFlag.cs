// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both, which are identical apart from obfuscated names:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     PositionFlag -> PositionFlag, line 518
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//     PositionFlag -> PositionFlag, line 25
// The ADOverhaul2019 build declares the same enum with the same values (line 25 there too).
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED -- all ten members and their values diffed against all three snapshots,
// which declare them identically and still at the lines recorded above (ControllerEditor 518,
// both ADOverhaul builds 25). [Flags] is on the decompiled enum too, and All is -1 in all three.

using System;

namespace DreadScripts.Common
{
    /// <summary>
    /// A position within a rectangle, used both as an anchor ("which point of the panel is pinned")
    /// and as a mask ("which resize grips are live").
    /// </summary>
    /// <remarks>
    /// <para>
    /// Conventions, because the rest of the resize code depends on them and they are easy to get
    /// backwards. Unity GUI space is y-down: <c>y</c> increases toward the bottom of the screen, so
    /// <see cref="Top"/> is the <em>lower</em> y coordinate and <see cref="Bottom"/> the higher one.
    /// <c>x</c> increases to the right as usual. A <c>Rect</c>'s edges are therefore
    /// left = <c>x</c>, right = <c>x + width</c>, top = <c>y</c>, bottom = <c>y + height</c>.
    /// </para>
    /// <para>
    /// The four side values name an <em>edge</em> and the four compound values name a <em>corner</em>:
    /// <see cref="TopLeft"/> is the (x, y) corner, <see cref="TopRight"/> is (x + width, y),
    /// <see cref="BottomLeft"/> is (x, y + height) and <see cref="BottomRight"/> is
    /// (x + width, y + height). The corners are distinct bits rather than <c>Top | Left</c>, so a
    /// corner never satisfies <c>HasFlag(Top)</c>; code that wants "is this anchored anywhere along
    /// the top" has to test <see cref="Top"/> and both top corners, which is what
    /// <see cref="ResizeHandle.GetVerticalPivot"/> and <see cref="ResizeHandle.GetHorizontalPivot"/>
    /// do.
    /// </para>
    /// <para>
    /// <see cref="All"/> is <c>-1</c> rather than the union of the declared bits, so it also matches
    /// any bit that might be added later. Masking code relies on that: the grip test is
    /// <c>(zone &amp; mask) &lt; zone</c>, meaning "the zone's bits are not all present in the mask",
    /// and <c>-1</c> passes every zone.
    /// </para>
    /// </remarks>
    [Flags]
    internal enum PositionFlag
    {
        /// <summary>The centre of the rect. As an anchor it makes the rect grow symmetrically.</summary>
        Middle = 1,

        /// <summary>The right edge (x + width).</summary>
        Right = 2,

        /// <summary>The left edge (x).</summary>
        Left = 4,

        /// <summary>The top edge (y) — the smaller y, since GUI space is y-down.</summary>
        Top = 8,

        /// <summary>The bottom edge (y + height) — the larger y.</summary>
        Bottom = 0x10,

        /// <summary>The (x + width, y) corner.</summary>
        TopRight = 0x20,

        /// <summary>The (x, y) corner.</summary>
        TopLeft = 0x40,

        /// <summary>The (x + width, y + height) corner.</summary>
        BottomRight = 0x80,

        /// <summary>The (x, y + height) corner.</summary>
        BottomLeft = 0x100,

        /// <summary>Every position. Deliberately <c>-1</c> so it matches bits not listed here.</summary>
        All = -1
    }
}
