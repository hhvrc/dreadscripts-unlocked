// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AxisControlSettings.cs
//
// DELIBERATE DEVIATION
// The three preset fields (allowed, forced, off) are `public static` in the decompiled source and
// `public static readonly` here. They are shared instances of a mutable struct: without readonly a
// caller writing `AxisControlSettings.off.axis = ...` would silently repoint the preset for every
// later reader. Nothing in either build assigns them after the initialisers, so this cannot change
// observable behaviour; it only makes the accident uncompilable.
//
// NOT PORTED
// The `internal static object CompareDecorator` field and the `PublishDecorator()` method that only
// tested it for null. Protector licence-check scaffolding, the same pattern recorded in
// Common/SphereHandle.cs and ADOverhaul/PhysBoneParameter.cs: nothing assigns the field, so the
// predicate is a constant `true`, and no caller reads either member.
//
// NOTES
// The private AllAxes constant is not a decompiled member; it names the `Axis.X | Axis.Y | Axis.Z`
// literal that the decompiled source repeats in five places.
//
// Audit status: VERIFIED -- all three fields, the three static presets and their constructor
// arguments, all four constructors, IsEnabled and ResolvePivotRotation were diffed statement by
// statement against export/. IsEnabled is rewritten from the decompiled nested form
// (`axis > None && state > Off`, then `state != Forced`) into an early-return; the two are
// equivalent given OptionState's declaration order (Off, Allowed, Forced -- checked in
// export/OptionState.cs) and Axis.None being the zero of a flags enum. ResolvePivotRotation is the
// same switch as a statement rather than an expression.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Whether one transform channel (position, rotation or scale) may be driven, on which axes, and
    /// in which space.
    /// </summary>
    [Serializable]
    internal struct AxisControlSettings
    {
        private const Axis AllAxes = Axis.X | Axis.Y | Axis.Z;

        [SerializeField]
        public Axis axis;

        [SerializeField]
        public OptionState state;

        [SerializeField]
        public OrientationState orientation;

        /// <summary>All axes, offered to the user but off unless they ask for it.</summary>
        public static readonly AxisControlSettings allowed =
            new AxisControlSettings(AllAxes, OptionState.Allowed, OrientationState.Both);

        /// <summary>All axes, always on and not up to the user.</summary>
        public static readonly AxisControlSettings forced = new AxisControlSettings(OptionState.Forced);

        /// <summary>Not offered at all.</summary>
        public static readonly AxisControlSettings off = new AxisControlSettings(OptionState.Off);

        public AxisControlSettings(Axis axis = AllAxes, OptionState state = OptionState.Allowed,
                                   OrientationState orientation = OrientationState.Both)
        {
            this.axis = axis;
            this.state = state;
            this.orientation = orientation;
        }

        public AxisControlSettings(OptionState state)
            : this(AllAxes, state)
        {
        }

        public AxisControlSettings(OrientationState orientation)
            : this(AllAxes, OptionState.Allowed, orientation)
        {
        }

        public AxisControlSettings(OptionState state, OrientationState orientation)
            : this(AllAxes, state, orientation)
        {
        }

        /// <summary>
        /// Whether this channel should actually be driven.
        /// </summary>
        /// <param name="userEnabled">
        /// What the user asked for. Consulted only when <see cref="state"/> is
        /// <see cref="OptionState.Allowed"/>; <see cref="OptionState.Forced"/> overrides it and
        /// <see cref="OptionState.Off"/> ignores it.
        /// </param>
        public bool IsEnabled(bool userEnabled)
        {
            if (axis == Axis.None || state == OptionState.Off)
            {
                return false;
            }

            return state == OptionState.Forced || userEnabled;
        }

        /// <summary>
        /// Resolves the pivot space to use, falling back to <paramref name="current"/> when this
        /// channel does not insist on one.
        /// </summary>
        public PivotRotation ResolvePivotRotation(PivotRotation current)
        {
            switch (orientation)
            {
                case OrientationState.Local:
                    return PivotRotation.Local;
                case OrientationState.Global:
                    return PivotRotation.Global;
                default:
                    return current;
            }
        }
    }
}
