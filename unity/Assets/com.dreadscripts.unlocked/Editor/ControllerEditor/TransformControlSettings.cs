// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/TransformControlSettings.cs
//
// Audit status: VERIFIED -- diffed in full against export/. The four serialized fields, the
// constructor and the three [SpecialName] accessors (properties again here) match, including the
// `uniformScaleOnly: false` on PositionAndRotation. The unreferenced static pair
// ListDecorator/CalcDecorator is not ported, on the same reading as the CustomizeStruct/
// SearchStruct pair in ReorderableListHelper.cs: never assigned, never called, obfuscator decoys.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Which of a transform's three channels a generated control may drive, and how.
    /// </summary>
    [Serializable]
    internal struct TransformControlSettings
    {
        [SerializeField]
        public AxisControlSettings positionControl;

        [SerializeField]
        public AxisControlSettings rotationControl;

        [SerializeField]
        public AxisControlSettings scaleControl;

        /// <summary>Scale is driven as a single value on all three axes rather than per axis.</summary>
        [SerializeField]
        public bool uniformScaleOnly;

        public TransformControlSettings(AxisControlSettings positionControl, AxisControlSettings rotationControl,
                                        AxisControlSettings scaleControl, bool uniformScaleOnly = true)
        {
            this.positionControl = positionControl;
            this.rotationControl = rotationControl;
            this.scaleControl = scaleControl;
            this.uniformScaleOnly = uniformScaleOnly;
        }

        /// <summary>Position and rotation, no scale. Per-axis scale, having no channel, is left off.</summary>
        public static TransformControlSettings PositionAndRotation =>
            new TransformControlSettings(AxisControlSettings.allowed, AxisControlSettings.allowed,
                                         AxisControlSettings.off, uniformScaleOnly: false);

        public static TransformControlSettings PositionOnly =>
            new TransformControlSettings(AxisControlSettings.allowed, AxisControlSettings.off,
                                         AxisControlSettings.off);

        public static TransformControlSettings RotationOnly =>
            new TransformControlSettings(AxisControlSettings.off, AxisControlSettings.allowed,
                                         AxisControlSettings.off);
    }
}
