// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/BoneTransformData.cs

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One bone in a PhysBone chain: where it sits, how deep it is, and its neighbours.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="PhysBoneChainData"/>. Not every entry corresponds to a real
    /// <see cref="UnityEngine.Transform"/> — see <see cref="isVirtual"/>.
    /// </remarks>
    internal class BoneTransformData
    {
        /// <summary>The chain this bone belongs to.</summary>
        internal PhysBoneChainData chain;

        /// <summary>Root of the chain, the same for every bone in it.</summary>
        internal Transform root;

        /// <summary>The scene transform this bone stands for, or null when <see cref="isVirtual"/>.</summary>
        internal Transform transform;

        /// <summary>World-space placement, which for a virtual bone is the only record of where it is.</summary>
        internal Matrix4x4 matrix;

        /// <summary>
        /// True for a bone the chain synthesised rather than found in the hierarchy — the endpoint
        /// added by <c>endpointPosition</c>, or the averaged stand-in for a multi-child branch.
        /// </summary>
        internal bool isVirtual;

        /// <summary>True when nothing follows this bone in the chain.</summary>
        internal bool isEndBone;

        /// <summary>Distance from the root, in bones.</summary>
        internal int depth;

        /// <summary>Next bone away from the root, or null at the end of the chain.</summary>
        internal BoneTransformData child;

        /// <summary>Previous bone towards the root, or null at its start.</summary>
        internal BoneTransformData parent;

        /// <summary>World-space position, read off the translation column of <see cref="matrix"/>.</summary>
        internal Vector3 Position => matrix.GetColumn(3);

        /// <summary>
        /// Largest of the three lossy scale axes — a single number to scale gizmos by, so a
        /// non-uniformly scaled bone still gets a handle big enough to grab.
        /// </summary>
        internal float MaxScale => Mathf.Max(matrix.lossyScale.x, matrix.lossyScale.y, matrix.lossyScale.z);

        /// <summary>
        /// Position along the chain as 0-1, root to tip. This is the axis PhysBone's falloff curves
        /// are defined over.
        /// </summary>
        internal float NormalizedDepth => chain.maxDepth == 0 ? 0f : (float)depth / chain.maxDepth;

        /// <summary>
        /// Samples a PhysBone falloff curve at this bone's depth. A curve that is absent or has
        /// fewer than two keys carries no falloff, so it evaluates to a flat 1.
        /// </summary>
        internal float EvaluateAtDepth(AnimationCurve curve)
        {
            if (curve == null || curve.length < 2)
            {
                return 1f;
            }

            return curve.Evaluate(NormalizedDepth);
        }
    }
}
