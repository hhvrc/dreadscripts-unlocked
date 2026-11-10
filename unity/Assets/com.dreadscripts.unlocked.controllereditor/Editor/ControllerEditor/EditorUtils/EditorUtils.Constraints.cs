// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PublishResolver -> SetSourceWeights,                          line 2407
//   static PopResolver     -> ActivateAndPreserveOffset(ParentConstraint),   line 2422
//   static ComputeResolver -> ActivateWithZeroOffset(ParentConstraint),      line 2431
//   static MoveResolver    -> ActivateAndPreserveOffset(RotationConstraint), line 2440
//   static ConcatResolver  -> ActivateWithZeroOffset(RotationConstraint),    line 2449
//   static CallResolver    -> ActivateAndPreserveOffset(PositionConstraint), line 2458
//   static CancelResolver  -> ActivateWithZeroOffset(PositionConstraint),    line 2467
//   static CountResolver   -> ActivateAndPreserveOffset(AimConstraint),      line 2476
//   static DisableResolver -> ActivateWithZeroOffset(AimConstraint),         line 2485
//   static InsertResolver  -> ActivateAndPreserveOffset(ScaleConstraint),    line 2494
//   static RestartResolver -> ActivateAndPreserveOffset(this IConstraint),   line 2503
//   static QueryResolver   -> ActivateWithZeroOffset(this IConstraint),      line 2529
//   backing MethodInfo caches (one per type/mode), line 2098-2114:
//     m_MockProperty       -> _parentActivatePreserve
//     m_InstanceProperty   -> _parentActivateZero
//     fieldProperty        -> _rotationActivatePreserve
//     _AttributeProperty   -> _rotationActivateZero
//     _ClientProperty      -> _positionActivatePreserve
//     _ConfigProperty      -> _positionActivateZero
//     m_DescriptorProperty -> _aimActivatePreserve
//     _TemplateProperty    -> _aimActivateZero
//     _MessageProperty     -> _scaleActivatePreserve
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// Helpers for Unity's built-in constraint components (UnityEngine.Animations.*Constraint).
//
// Each concrete constraint type exposes two internal methods, ActivateAndPreserveOffset and
// ActivateWithZeroOffset, that Unity's own constraint inspector calls when the "Activate" /
// "Zero" buttons are pressed. They are not part of the public IConstraint surface, so the vendor
// reaches them by reflection and caches the resolved MethodInfo the first time each is used.
// ScaleConstraint only has the preserve-offset variant in the original, so there is deliberately
// no ActivateWithZeroOffset(ScaleConstraint) here.
//
// The two IConstraint dispatchers mirror the original exactly: RestartResolver/QueryResolver
// type-switch over ParentConstraint / RotationConstraint / PositionConstraint / AimConstraint and
// do nothing for any other implementer (including ScaleConstraint) -- preserved as-is.

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        private static MethodInfo _parentActivatePreserve;
        private static MethodInfo _parentActivateZero;
        private static MethodInfo _rotationActivatePreserve;
        private static MethodInfo _rotationActivateZero;
        private static MethodInfo _positionActivatePreserve;
        private static MethodInfo _positionActivateZero;
        private static MethodInfo _aimActivatePreserve;
        private static MethodInfo _aimActivateZero;
        private static MethodInfo _scaleActivatePreserve;

        /// <summary>
        /// Overwrites the weights of the constraint's existing sources, in order, leaving each
        /// source transform untouched. Extra weights (beyond the source count) are ignored.
        /// </summary>
        internal static void SetSourceWeights(this IConstraint constraint, params float[] weights)
        {
            List<ConstraintSource> sources = new List<ConstraintSource>();
            constraint.GetSources(sources);
            for (int i = 0; i < weights.Length && i < sources.Count; i++)
            {
                sources[i] = new ConstraintSource
                {
                    sourceTransform = sources[i].sourceTransform,
                    weight = weights[i]
                };
            }

            constraint.SetSources(sources);
        }

        /// <summary>Activates the constraint, keeping the current offset between it and its sources.</summary>
        internal static void ActivateAndPreserveOffset(this ParentConstraint constraint)
        {
            if (_parentActivatePreserve == null)
            {
                _parentActivatePreserve = typeof(ParentConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _parentActivatePreserve.Invoke(constraint, null);
        }

        /// <summary>Activates the constraint, snapping it to a zero offset from its sources.</summary>
        internal static void ActivateWithZeroOffset(this ParentConstraint constraint)
        {
            if (_parentActivateZero == null)
            {
                _parentActivateZero = typeof(ParentConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _parentActivateZero.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateAndPreserveOffset(ParentConstraint)"/>
        internal static void ActivateAndPreserveOffset(this RotationConstraint constraint)
        {
            if (_rotationActivatePreserve == null)
            {
                _rotationActivatePreserve = typeof(RotationConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _rotationActivatePreserve.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateWithZeroOffset(ParentConstraint)"/>
        internal static void ActivateWithZeroOffset(this RotationConstraint constraint)
        {
            if (_rotationActivateZero == null)
            {
                _rotationActivateZero = typeof(RotationConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _rotationActivateZero.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateAndPreserveOffset(ParentConstraint)"/>
        internal static void ActivateAndPreserveOffset(this PositionConstraint constraint)
        {
            if (_positionActivatePreserve == null)
            {
                _positionActivatePreserve = typeof(PositionConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _positionActivatePreserve.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateWithZeroOffset(ParentConstraint)"/>
        internal static void ActivateWithZeroOffset(this PositionConstraint constraint)
        {
            if (_positionActivateZero == null)
            {
                _positionActivateZero = typeof(PositionConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _positionActivateZero.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateAndPreserveOffset(ParentConstraint)"/>
        internal static void ActivateAndPreserveOffset(this AimConstraint constraint)
        {
            if (_aimActivatePreserve == null)
            {
                _aimActivatePreserve = typeof(AimConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _aimActivatePreserve.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateWithZeroOffset(ParentConstraint)"/>
        internal static void ActivateWithZeroOffset(this AimConstraint constraint)
        {
            if (_aimActivateZero == null)
            {
                _aimActivateZero = typeof(AimConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _aimActivateZero.Invoke(constraint, null);
        }

        /// <inheritdoc cref="ActivateAndPreserveOffset(ParentConstraint)"/>
        internal static void ActivateAndPreserveOffset(this ScaleConstraint constraint)
        {
            if (_scaleActivatePreserve == null)
            {
                _scaleActivatePreserve = typeof(ScaleConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _scaleActivatePreserve.Invoke(constraint, null);
        }

        /// <summary>
        /// Dispatches <see cref="ActivateAndPreserveOffset(ParentConstraint)"/> to the concrete
        /// constraint type. Does nothing for constraint kinds the original never handled here.
        /// </summary>
        private static void ActivateAndPreserveOffset(this IConstraint constraint)
        {
            if (constraint is ParentConstraint parent)
            {
                parent.ActivateAndPreserveOffset();
            }
            else if (constraint is RotationConstraint rotation)
            {
                rotation.ActivateAndPreserveOffset();
            }
            else if (constraint is PositionConstraint position)
            {
                position.ActivateAndPreserveOffset();
            }
            else if (constraint is AimConstraint aim)
            {
                aim.ActivateAndPreserveOffset();
            }
        }

        /// <summary>
        /// Dispatches <see cref="ActivateWithZeroOffset(ParentConstraint)"/> to the concrete
        /// constraint type. Does nothing for constraint kinds the original never handled here.
        /// </summary>
        private static void ActivateWithZeroOffset(this IConstraint constraint)
        {
            if (constraint is ParentConstraint parent)
            {
                parent.ActivateWithZeroOffset();
            }
            else if (constraint is RotationConstraint rotation)
            {
                rotation.ActivateWithZeroOffset();
            }
            else if (constraint is PositionConstraint position)
            {
                position.ActivateWithZeroOffset();
            }
            else if (constraint is AimConstraint aim)
            {
                aim.ActivateWithZeroOffset();
            }
        }
    }
}
