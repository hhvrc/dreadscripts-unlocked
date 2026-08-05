// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ResetQueue   -> HandleScrollWheel, line 5886
//   static ConnectQueue -> OverlayLabel,      line 5907
//   static FlushQueue   -> OverlayLabel,      line 5902
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// FlushQueue and ConnectQueue are collapsed into one method, as EditorUtils.Buttons.cs does for the
// same pattern: FlushQueue's entire body is a call to ConnectQueue passing true for the draw flag,
// which is already the parameter's default. Every other parameter matches one-for-one in the same
// order. Call sites written against FlushQueue therefore need the `draw` argument skipped by name,
// or their trailing arguments moved along one place.
//
// Not ported here: CalculateQueue (line 5923) and TestQueue (line 5928), the two layout-flow
// overloads that pass GUILayoutUtility.GetLastRect() to ConnectQueue. They add nothing but that one
// call and were left out of this pass; they have since landed in
// EditorUtils.LayoutOverlayLabels.cs, collapsed into a single OverlayLabel(string, ...).
//
// The ADOverhaul twin of the label method is decompiled EnableStatus in export/ADOverhaul2022
// ADOEditorUtility.cs -- character-for-character the same logic, down to the +/- 2.5f nudge and the
// noteLeft/noteRight style pick. It is not ported on the ADOverhaul side yet, so there is no package
// file to point at; the two products carry independent copies and are left independent.
// Audit status: VERIFIED -- both bodies diffed statement by statement against export/, including
// which branch of the alignLeft test adds and which subtracts (alignLeft adds), and the inverted
// style pick that goes with it. One shape-only difference: the decompiled guard is a positive
// `if (draw && !(width <= reserved + inset))` wrapping the body, written here as an early return.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Forwards the scroll delta to <paramref name="onScroll"/> when the wheel turns over
        /// <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">
        /// The region to watch. An all-zero rect is taken to mean "the control just drawn" and is
        /// replaced with <see cref="GUILayoutUtility.GetLastRect"/>, which is what lets a layout-flow
        /// caller pass <c>default</c> instead of tracking a rect. A genuinely empty rect at the
        /// origin is indistinguishable from that request, but such a rect could not contain the
        /// cursor anyway.
        /// </param>
        /// <remarks>
        /// <para>
        /// The event is deliberately <em>not</em> consumed. Scrolling over one of these regions still
        /// scrolls the enclosing scroll view, which is how the shipped tool behaves -- the callback
        /// is an additional effect layered on the normal scroll, typically nudging a value, not a
        /// replacement for it. Callers wanting exclusive control have to call
        /// <c>Event.current.Use()</c> themselves.
        /// </para>
        /// <para>
        /// Only the vertical component is passed on; horizontal scrolling is ignored.
        /// </para>
        /// </remarks>
        internal static void HandleScrollWheel(Rect rect, Action<float> onScroll)
        {
            Event current = Event.current;
            if (current.type != EventType.ScrollWheel)
            {
                return;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            if (rect.Contains(current.mousePosition))
            {
                onScroll(current.delta.y);
            }
        }

        /// <summary>
        /// Draws a small italic annotation inside an existing rect, offset from one of its edges.
        /// Used for placeholder text over an empty field, for unit suffixes, and for the column
        /// captions above a list.
        /// </summary>
        /// <param name="draw">Lets a caller gate the label without an <c>if</c> at the call site.</param>
        /// <param name="reservedWidth">
        /// Width the rect's own content needs. The label is skipped when the rect is not wider than
        /// this plus <paramref name="inset"/>, so a narrow inspector drops the annotation instead of
        /// overlapping it with the content.
        /// </param>
        /// <param name="inset">How far in from the chosen edge the label starts, plus a 2.5px gap.</param>
        /// <param name="alignLeft">
        /// Left edge when true, right edge when false. Also picks the matching alignment style, so
        /// the two stay consistent unless <paramref name="style"/> overrides it.
        /// </param>
        /// <remarks>
        /// <paramref name="reservedWidth"/> takes part only in the width test; it never shifts the
        /// label. Only <paramref name="inset"/> does. The rect is passed by value, so the shift is
        /// local to this call.
        /// </remarks>
        internal static void OverlayLabel(Rect rect, string text, bool draw = true, float reservedWidth = 0f,
            float inset = 0f, bool alignLeft = true, GUIStyle style = null)
        {
            if (!draw || rect.width <= reservedWidth + inset)
            {
                return;
            }

            if (alignLeft)
            {
                rect.x += inset + 2.5f;
            }
            else
            {
                rect.x -= inset + 2.5f;
            }

            GUI.Label(rect, text, style ?? (alignLeft ? styles.noteLeft : styles.noteRight));
        }
    }
}
