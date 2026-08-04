// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested class BoneNode -> BoneNode, lines 1499-1545
//   GetPosition()         -> position (property),        line 1520
//   GetMaxScale()         -> maxScale (property),        line 1526
//   GetNormalizedDepth()  -> normalizedDepth (property), line 1532
//   EvaluateCurve         -> EvaluateCurve,              line 1537
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every field and every statement below was transcribed
// from the region above.
//
// DEOBF-BUG(resolved): all three getters carried [SpecialName] with no matching setter, which is
// how ILSpy renders a property getter it could not re-form. They are restored as read-only
// properties, matching how the same attribute was handled on CachedIcon and RemoteTexture. Nothing
// else in the type was renamed by the protector -- tree, root, transform, matrix, isVirtual,
// isEndBone, depth, child and parent all read as English and none rhymes with the Serializer family.
//
// ControllerEditor ships no equivalent; this is part of ADOverhaul's PhysBone chain analysis, built
// by BoneChainTree.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// One joint of a PhysBone chain, as the tool sees it: a transform plus where it sits in the
        /// chain.
        /// </summary>
        /// <remarks>
        /// Not a wrapper around a <see cref="Transform"/> -- a node may be virtual, meaning it has no
        /// transform at all. See <see cref="isVirtual"/>.
        /// </remarks>
        internal class BoneNode
        {
            /// <summary>The chain this node was built into. Read for the chain's depth.</summary>
            internal BoneChainTree tree;

            /// <summary>The PhysBone's root transform, the same for every node of one chain.</summary>
            internal Transform root;

            /// <summary>
            /// The transform this node stands for, or null when <see cref="isVirtual"/>.
            /// </summary>
            internal Transform transform;

            /// <summary>
            /// The node's world transform. Held separately from <see cref="transform"/> because a
            /// virtual node has none, and because a multi-child average node sits at a position no
            /// transform occupies.
            /// </summary>
            internal Matrix4x4 matrix;

            /// <summary>
            /// Whether this node has no transform behind it -- an endpoint the PhysBone synthesises,
            /// either from its endpoint offset or as the average of several children.
            /// </summary>
            internal bool isVirtual;

            /// <summary>Whether the chain stops here.</summary>
            internal bool isEndBone;

            /// <summary>Distance from the root, in joints.</summary>
            internal int depth;

            internal BoneNode child;

            internal BoneNode parent;

            /// <summary>The node's world position, read out of the translation column of <see cref="matrix"/>.</summary>
            internal Vector3 position => matrix.GetColumn(3);

            /// <summary>
            /// The largest of the three lossy scale axes, for sizing a handle so it stays visible on
            /// a non-uniformly scaled bone.
            /// </summary>
            internal float maxScale => Mathf.Max(matrix.lossyScale.x, matrix.lossyScale.y, matrix.lossyScale.z);

            /// <summary>
            /// <see cref="depth"/> mapped onto 0..1 across the chain's full depth, which is the
            /// domain every PhysBone falloff curve is authored in.
            /// </summary>
            internal float normalizedDepth => 1f / (float)tree.maxDepth * (float)depth;

            /// <summary>
            /// <paramref name="curve"/> sampled at this node's <see cref="normalizedDepth"/>.
            /// </summary>
            /// <returns>
            /// 1 for a null curve or one with fewer than two keys, which is how VRChat treats an
            /// unauthored falloff curve -- so a caller can multiply by this unconditionally.
            /// </returns>
            internal float EvaluateCurve(AnimationCurve curve)
            {
                if (curve == null || curve.length < 2)
                {
                    return 1f;
                }

                return curve.Evaluate(normalizedDepth);
            }
        }
    }
}
