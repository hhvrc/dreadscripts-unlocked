// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EventExtensions.cs
//
// NOTES
// The private Where(EventWrapper, Func<Event, bool>) helper is not a decompiled member. Each of the
// six tests below is written out in full in the decompiled source as the same validity check, the
// same single assignment and the same return; only the predicate differs. They are factored through
// one helper here. The predicates capture nothing, so the compiler caches one delegate per call
// site and the refactor costs no allocation.
//
// Audit status: VERIFIED -- all seven decompiled methods were diffed statement by statement against
// export/. InRect is transcribed literally, including the default(Rect) sentinel and the
// GetLastRect fallback; the other six differ only by the factoring recorded above, and each
// predicate was checked against the condition it replaces (ContextClick, MouseDown, MouseUp,
// button == 0, button == 1, clickCount == 2).

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Chainable tests on an <see cref="EventWrapper"/>, for reading an event's kind, button and
    /// location as one expression:
    /// <code>
    /// if (new EventWrapper(Event.current).IsMouseDown().IsRightButton().InRect(rect))
    /// </code>
    /// </summary>
    /// <remarks>
    /// Each test narrows the wrapper and hands it back, so a failure anywhere in the chain carries
    /// through to the end. Once a wrapper is invalid the remaining tests short-circuit — which they
    /// must, because a default-constructed wrapper has no event to look at.
    /// </remarks>
    internal static class EventExtensions
    {
        /// <summary>Passes when the mouse is inside <paramref name="rect"/>, defaulting to the last laid-out rect.</summary>
        internal static EventWrapper InRect(this EventWrapper wrapper, Rect rect = default(Rect))
        {
            if (!wrapper.isValid)
            {
                return wrapper;
            }

            if (rect == default(Rect))
            {
                rect = GUILayoutUtility.GetLastRect();
            }

            wrapper.isValid = rect.Contains(wrapper.currentEvent.mousePosition);
            return wrapper;
        }

        internal static EventWrapper IsContextClick(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.type == EventType.ContextClick);
        }

        internal static EventWrapper IsMouseDown(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.type == EventType.MouseDown);
        }

        internal static EventWrapper IsMouseUp(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.type == EventType.MouseUp);
        }

        internal static EventWrapper IsLeftButton(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.button == 0);
        }

        internal static EventWrapper IsRightButton(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.button == 1);
        }

        internal static EventWrapper IsDoubleClick(this EventWrapper wrapper)
        {
            return Where(wrapper, e => e.clickCount == 2);
        }

        /// <summary>
        /// Applies <paramref name="predicate"/> only to a wrapper that is still valid. The predicates
        /// above capture nothing, so the compiler caches one delegate instance per call site.
        /// </summary>
        private static EventWrapper Where(EventWrapper wrapper, Func<Event, bool> predicate)
        {
            if (wrapper.isValid)
            {
                wrapper.isValid = predicate(wrapper.currentEvent);
            }

            return wrapper;
        }
    }
}
