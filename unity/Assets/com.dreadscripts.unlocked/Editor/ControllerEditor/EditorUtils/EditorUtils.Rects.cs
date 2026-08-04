// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static SortResolver      -> SliceLeft(this ref Rect, float, bool, float, bool, bool),  line 2943
//   static PatchResolver     -> SliceRight(this ref Rect, float, bool, float, bool, bool), line 2982
//   static RegisterResolver  -> SliceTop(this ref Rect, float, bool, float, bool, bool),   line 2959
//   static ChangeResolver    -> SliceLeftIf,                                               line 2934
//   static LogoutResolver    -> SliceRightIf,                                              line 2973
//   static VerifyResolver    -> Expand(this Rect, float),                                  line 2873
//   static AssetResolver     -> ExpandUp,                                                  line 2921
//   static UpdateResolver    -> ExpandDown,                                                line 2928
//   static InterruptResolver -> WithWidth,                                                 line 2995
//   static ManageResolver    -> WithHeight,                                                line 3001
//   static PrintResolver     -> FitAspect,                                                 line 3007
//   static ViewResolver      -> CollapseToRightEdge,                                       line 2851
//   static CollectResolver   -> Shrink,                                                    line 2858
//   static ResolveResolver   -> ShrinkHorizontally,                                        line 2863
//   static ListResolver      -> ShrinkVertically,                                          line 2868
//   static FillResolver      -> ExpandHorizontally,                                        line 2882
//   static WriteResolver     -> ExpandVertically,                                          line 2889
//   static ForgotResolver    -> MoveRight,                                                 line 2896
//   static StopResolver      -> MoveDown,                                                  line 2902
//   static CheckResolver     -> InsetLeft,                                                 line 2908
//   static PrepareResolver   -> Widen,                                                     line 2915
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Complete: the whole Rect region (2851-3022) is now here. The three Shrink* methods are literally
// the Expand* ones with a negated argument, which is how the vendor wrote them and is kept, so the
// pair reads the same way at a call site whichever direction is wanted.
// Audit status: VERIFIED against decompiled/

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Cuts a column off the left of <paramref name="rect"/> and returns it, by default shrinking
        /// <paramref name="rect"/> to the space that is left.
        /// </summary>
        /// <param name="amount">
        /// Width of the slice: pixels when <paramref name="absolute"/> is true, otherwise a
        /// percentage of the current width of <paramref name="rect"/>.
        /// </param>
        /// <param name="absolute">Whether <paramref name="amount"/> is in pixels rather than percent.</param>
        /// <param name="offset">
        /// Gap left between the rect's left edge and the slice, measured the same way as
        /// <paramref name="amount"/> but switched by <paramref name="offsetAbsolute"/>. The value
        /// -1 is a sentinel for "no gap"; any other negative number is applied literally and pulls
        /// the slice out to the left of the rect.
        /// </param>
        /// <param name="consume">
        /// Whether the slice is taken out of <paramref name="rect"/>. When false the caller's rect is
        /// untouched and this is only a measurement, which is how several slices can be laid over the
        /// same row.
        /// </param>
        /// <remarks>
        /// Consuming advances the left edge past both the gap and the slice, but only subtracts the
        /// slice's width, so a non-zero <paramref name="offset"/> pushes the remaining rect's right
        /// edge outwards by that much. That is the shipped behaviour and callers lay out against it,
        /// so it is kept as-is rather than made symmetric with <see cref="SliceRight"/>, which does
        /// charge the gap to the remaining width.
        /// </remarks>
        internal static Rect SliceLeft(this ref Rect rect, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            Rect result = rect;
            result.width = absolute ? amount : (amount * rect.width / 100f);

            float gap = (offset == -1f) ? 0f : (offsetAbsolute ? offset : (offset * rect.width / 100f));
            result.x = rect.x + gap;
            result.y = rect.y;

            if (consume)
            {
                rect.x = result.x + result.width;
                rect.width -= result.width;
            }

            return result;
        }

        /// <summary>
        /// Cuts a column off the right of <paramref name="rect"/> and returns it, by default shrinking
        /// <paramref name="rect"/> to the space that is left.
        /// </summary>
        /// <param name="amount">
        /// Width of the slice: pixels when <paramref name="absolute"/> is true, otherwise a
        /// percentage of the current width of <paramref name="rect"/>.
        /// </param>
        /// <param name="absolute">Whether <paramref name="amount"/> is in pixels rather than percent.</param>
        /// <param name="offset">
        /// Gap left between the rect's right edge and the slice, measured the same way as
        /// <paramref name="amount"/> but switched by <paramref name="offsetAbsolute"/>. The value
        /// -1 is a sentinel for "no gap"; any other negative number is applied literally and pushes
        /// the slice out past the right edge.
        /// </param>
        /// <param name="consume">
        /// Whether the slice is taken out of <paramref name="rect"/>. Unlike <see cref="SliceLeft"/>
        /// the gap is consumed along with the slice, so repeated right slices with the same offset
        /// stay evenly spaced.
        /// </param>
        /// <remarks>
        /// The slice keeps the rect's y and height, so this only divides a row horizontally.
        /// </remarks>
        internal static Rect SliceRight(this ref Rect rect, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            Rect result = rect;
            result.width = absolute ? amount : (amount * rect.width / 100f);

            float gap = (offset == -1f) ? 0f : (offsetAbsolute ? offset : (offset * rect.width / 100f));
            result.x = rect.x + rect.width - result.width - gap;

            if (consume)
            {
                rect.width -= result.width + gap;
            }

            return result;
        }

        /// <summary>
        /// Cuts a row off the top of <paramref name="rect"/> and returns it, by default shrinking
        /// <paramref name="rect"/> to the space that is left. The vertical counterpart of
        /// <see cref="SliceLeft"/>.
        /// </summary>
        /// <param name="amount">
        /// Height of the slice: pixels when <paramref name="absolute"/> is true, otherwise a
        /// percentage of the current height of <paramref name="rect"/>.
        /// </param>
        /// <param name="absolute">Whether <paramref name="amount"/> is in pixels rather than percent.</param>
        /// <param name="offset">
        /// Gap left between the rect's top edge and the slice. Percentages are measured against the
        /// rect's <i>height</i> here, not its width. The value -1 is a sentinel for "no gap"; any
        /// other negative number is applied literally and lifts the slice above the rect.
        /// </param>
        /// <param name="consume">
        /// Whether the slice is taken out of <paramref name="rect"/>. When false the caller's rect is
        /// untouched and this is only a measurement.
        /// </param>
        /// <remarks>
        /// The slice keeps the rect's x and width, so this only divides a column vertically.
        /// <para>
        /// Consuming shares the asymmetry of <see cref="SliceLeft"/> rather than of
        /// <see cref="SliceRight"/>: the top edge advances past both the gap and the slice, but only
        /// the slice's height is subtracted, so a non-zero <paramref name="offset"/> pushes the
        /// remaining rect's bottom edge down by that much. That is the shipped behaviour.
        /// </para>
        /// <para>
        /// The decompiled source writes this method's offset ternary in the mirrored form
        /// (<c>absolute ? offset : percent</c>) compared to <see cref="SliceLeft"/>'s
        /// (<c>!absolute ? percent : offset</c>). That is a difference in spelling only -- the two
        /// select the same branch for the same flag, and this port was checked against both. There is
        /// no inversion of the flag's meaning to preserve.
        /// </para>
        /// </remarks>
        internal static Rect SliceTop(this ref Rect rect, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            Rect result = rect;
            result.height = absolute ? amount : (amount * rect.height / 100f);

            float gap = (offset == -1f) ? 0f : (offsetAbsolute ? offset : (offset * rect.height / 100f));
            result.y = rect.y + gap;

            if (consume)
            {
                rect.y = result.y + result.height;
                rect.height -= result.height;
            }

            return result;
        }

        /// <summary>
        /// Runs <see cref="SliceLeft"/> only when <paramref name="condition"/> holds, and otherwise
        /// returns <paramref name="rect"/> as it stands without consuming anything from it.
        /// </summary>
        /// <remarks>
        /// This exists so a row that optionally carries a leading column -- an icon, a toggle -- can be
        /// laid out as a single unconditional chain of slices.
        /// </remarks>
        internal static Rect SliceLeftIf(this ref Rect rect, bool condition, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            if (condition)
            {
                return rect.SliceLeft(amount, absolute, offset, offsetAbsolute, consume);
            }

            return rect;
        }

        /// <summary>
        /// Runs <see cref="SliceRight"/> only when <paramref name="condition"/> holds, and otherwise
        /// returns <paramref name="rect"/> as it stands without consuming anything from it.
        /// </summary>
        internal static Rect SliceRightIf(this ref Rect rect, bool condition, float amount, bool absolute = false, float offset = -1f, bool offsetAbsolute = false, bool consume = true)
        {
            if (!condition)
            {
                return rect;
            }

            return rect.SliceRight(amount, absolute, offset, offsetAbsolute, consume);
        }

        /// <summary>
        /// Returns a copy of <paramref name="rect"/> grown by <paramref name="amount"/> on all four
        /// sides, keeping the same centre. A negative amount shrinks it, which is how most callers use
        /// it -- <c>Expand(-1f)</c> insets an icon by a pixel.
        /// </summary>
        /// <remarks>
        /// The caller's rect is not modified; these expanders all take their receiver by value.
        /// <para>
        /// <c>DreadScripts.Common.EditorGuiUtils.Shrink</c> is the same arithmetic with the sign
        /// flipped. Both are kept because each product shipped its own copy in its own assembly.
        /// </para>
        /// </remarks>
        internal static Rect Expand(this Rect rect, float amount)
        {
            rect.x -= amount;
            rect.y -= amount;
            rect.width += amount * 2f;
            rect.height += amount * 2f;
            return rect;
        }

        /// <summary>
        /// Returns a copy of <paramref name="rect"/> grown upwards by <paramref name="amount"/>: the
        /// top edge moves, the bottom edge stays put.
        /// </summary>
        internal static Rect ExpandUp(this Rect rect, float amount)
        {
            rect.height += amount;
            rect.y -= amount;
            return rect;
        }

        /// <summary>
        /// Returns a copy of <paramref name="rect"/> grown downwards by <paramref name="amount"/>: the
        /// bottom edge moves, the top edge stays put.
        /// </summary>
        internal static Rect ExpandDown(this Rect rect, float amount)
        {
            rect.height += amount;
            return rect;
        }

        /// <summary>
        /// Returns a copy of <paramref name="rect"/> with its width replaced, anchored at the left edge.
        /// </summary>
        internal static Rect WithWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        /// <summary>
        /// Returns a copy of <paramref name="rect"/> with its height replaced, anchored at the top edge.
        /// </summary>
        internal static Rect WithHeight(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }

        /// <summary>
        /// Returns the largest rect of the given width-to-height ratio that fits inside
        /// <paramref name="rect"/>, centred on the unconstrained axis.
        /// </summary>
        /// <param name="aspect">Target ratio, expressed as width divided by height.</param>
        /// <remarks>
        /// Used to letterbox banner and thumbnail images into a layout rect whose shape is decided by
        /// the surrounding GUI rather than by the image.
        /// </remarks>
        internal static Rect FitAspect(this Rect rect, float aspect)
        {
            Rect result = rect;

            if (rect.width / rect.height <= aspect)
            {
                // Too tall for the target shape: width is the limit, so pillar the height and centre it.
                result.height = rect.width / aspect;
                result.y += (rect.height - result.height) / 2f;
            }
            else
            {
                result.width = rect.height * aspect;
                result.x += (rect.width - result.width) / 2f;
            }

            return result;
        }
    
        /// <summary>
        /// The zero-width rect sitting immediately to the right of <paramref name="rect"/>, at the
        /// same y and height -- the cursor position after it, for laying the next control out by
        /// hand.
        /// </summary>
        internal static Rect CollapseToRightEdge(this Rect rect)
        {
            Rect result = new Rect(rect);
            result.x = rect.x + rect.width;
            return result;
        }

        /// <summary>Inset on all four sides by <paramref name="amount"/>.</summary>
        internal static Rect Shrink(this Rect rect, float amount)
        {
            return rect.Expand(-amount);
        }

        /// <summary>Inset on the left and right by <paramref name="amount"/>.</summary>
        internal static Rect ShrinkHorizontally(this Rect rect, float amount)
        {
            return rect.ExpandHorizontally(-amount);
        }

        /// <summary>Inset on the top and bottom by <paramref name="amount"/>.</summary>
        internal static Rect ShrinkVertically(this Rect rect, float amount)
        {
            return rect.ExpandVertically(-amount);
        }

        /// <summary>
        /// Grown by <paramref name="amount"/> on the left and right, keeping the centre.
        /// </summary>
        internal static Rect ExpandHorizontally(this Rect rect, float amount)
        {
            rect.x -= amount;
            rect.width += amount * 2f;
            return rect;
        }

        /// <summary>
        /// Grown by <paramref name="amount"/> on the top and bottom, keeping the centre.
        /// </summary>
        internal static Rect ExpandVertically(this Rect rect, float amount)
        {
            rect.y -= amount;
            rect.height += amount * 2f;
            return rect;
        }

        /// <summary>Moved right by <paramref name="amount"/>; the size is unchanged.</summary>
        internal static Rect MoveRight(this Rect rect, float amount)
        {
            rect.x += amount;
            return rect;
        }

        /// <summary>Moved down by <paramref name="amount"/>; the size is unchanged.</summary>
        internal static Rect MoveDown(this Rect rect, float amount)
        {
            rect.y += amount;
            return rect;
        }

        /// <summary>
        /// Inset from the left by <paramref name="amount"/>: the left edge moves right and the
        /// right edge stays put.
        /// </summary>
        internal static Rect InsetLeft(this Rect rect, float amount)
        {
            rect.width -= amount;
            rect.x += amount;
            return rect;
        }

        /// <summary>
        /// Widened by <paramref name="amount"/> on the right only; the left edge stays put.
        /// </summary>
        internal static Rect Widen(this Rect rect, float amount)
        {
            rect.width += amount;
            return rect;
        }
    }
}
