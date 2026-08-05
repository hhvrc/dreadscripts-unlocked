// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EventWrapper.cs
//
// NOT PORTED
// The `internal static object NewDecorator` field and the `LoginDecorator()` method that only
// tested it for null. This is the protector's licence-check scaffolding, the same pattern recorded
// in Common/SphereHandle.cs and ADOverhaul/PhysBoneParameter.cs: nothing ever assigns the field, so
// the predicate is a constant `true`, and no caller in either build reads either member.
//
// Audit status: VERIFIED -- both fields, the constructor, Use() and both implicit operators were
// diffed statement by statement against export/ and match. The two dropped scaffolding members are
// recorded above.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// An <see cref="Event"/> that may be absent, letting a lookup that finds no matching event be
    /// returned as a value rather than null.
    /// </summary>
    /// <remarks>
    /// A default-constructed wrapper is falsy, so the result of such a lookup can be tested and used
    /// in one step:
    /// <code>
    /// EventWrapper e = FindEvent();
    /// if (e) { e.Use(); }
    /// </code>
    /// </remarks>
    internal struct EventWrapper
    {
        internal readonly Event currentEvent;

        /// <summary>False for a default-constructed wrapper, which carries no event.</summary>
        internal bool isValid;

        internal EventWrapper(Event currentEvent)
        {
            this.currentEvent = currentEvent;
            isValid = true;
        }

        /// <summary>Consumes the event so no other control in this GUI pass reacts to it.</summary>
        internal void Use()
        {
            currentEvent.Use();
        }

        public static implicit operator Event(EventWrapper wrapper)
        {
            return wrapper.currentEvent;
        }

        public static implicit operator bool(EventWrapper wrapper)
        {
            return wrapper.isValid;
        }
    }
}
