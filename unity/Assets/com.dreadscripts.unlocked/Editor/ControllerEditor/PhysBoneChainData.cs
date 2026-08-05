// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/PhysBoneChainData.cs
//
// Audit status: VERIFIED -- diffed in full against export/. The five fields, the constructor,
// BuildChain and BuildChains match statement for statement, and [SpecialName] GetBoneMatrices is
// the BoneMatrices property. Two shape-only changes, both behaviour-preserving: the decompile's
// two identical virtual-bone object initialisers are factored into the private NewVirtualBone
// helper (not a decompiled member), and its `child`/`boneTransformData2` locals -- always assigned
// the same instance on both branches -- are the single `virtualBone` local here.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The bone chain a <see cref="VRCPhysBone"/> affects, resolved once so it can be drawn and
    /// measured without re-walking the hierarchy every repaint.
    /// </summary>
    /// <remarks>
    /// This mirrors how VRChat's own PhysBone builds its chain, including the parts that have no
    /// counterpart in the scene: an <c>endpointPosition</c> adds a virtual bone past the last real
    /// one, and a branch point under <see cref="VRCPhysBoneBase.MultiChildType.Average"/> is replaced
    /// by a single virtual bone at the mean of its children. Reproducing those here is what makes the
    /// drawn gizmos line up with what the component actually simulates.
    /// </remarks>
    internal class PhysBoneChainData
    {
        internal readonly VRCPhysBone physBone;

        internal readonly Transform rootTransform;

        /// <summary>Every bone, in the order the hierarchy walk reached them.</summary>
        internal readonly List<BoneTransformData> bones;

        /// <summary>Depth of the deepest bone; the denominator for <see cref="BoneTransformData.NormalizedDepth"/>.</summary>
        internal readonly int maxDepth;

        /// <summary>
        /// The bones regrouped into root-to-tip runs. Null until <see cref="BuildChains"/> is called.
        /// </summary>
        internal List<List<BoneTransformData>> chains;

        internal PhysBoneChainData(VRCPhysBone physBone)
        {
            this.physBone = physBone;
            rootTransform = physBone.GetRootTransform();
            bones = new List<BoneTransformData>();

            BuildChain(rootTransform, 0);

            maxDepth = bones.Max(b => b.depth);
        }

        internal IEnumerable<Matrix4x4> BoneMatrices => bones.Select(b => b.matrix);

        /// <summary>
        /// Walks <paramref name="transform"/> and its children, appending a bone for each and
        /// recursing. <paramref name="depth"/> is the distance from the chain root.
        /// </summary>
        internal void BuildChain(Transform transform, int depth)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!physBone.ignoreTransforms.Contains(child))
                {
                    children.Add(child);
                }
            }

            BoneTransformData bone = new BoneTransformData();
            BoneTransformData virtualBone = null;
            bool skipThisBone = false;

            // A bone's rotation is defined by the direction of what follows it, so it can only be
            // settled once the children are known.
            Quaternion rotation = transform.rotation;
            bool isEndBone = children.Count == 0;

            if (isEndBone)
            {
                if (physBone.endpointPosition != Vector3.zero)
                {
                    Vector3 endpoint = transform.TransformPoint(physBone.endpointPosition);
                    rotation = transform.rotation *
                               Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(physBone.endpointPosition));

                    virtualBone = NewVirtualBone(endpoint, rotation, transform.lossyScale, depth + 1, bone);
                }
                else if (bones.Count != 0)
                {
                    // No endpoint to aim at, so keep the previous bone's rotation rather than the
                    // transform's own, which is arbitrary at a chain tip.
                    rotation = bones[bones.Count - 1].matrix.rotation;
                }
            }
            else if (children.Count > 1)
            {
                switch (physBone.multiChildType)
                {
                    case VRCPhysBoneBase.MultiChildType.Average:
                    {
                        Vector3 average = Vector3.zero;
                        foreach (Transform child in children)
                        {
                            average += child.position;
                        }

                        average /= children.Count;

                        rotation = transform.rotation *
                                   Quaternion.FromToRotation(transform.up, average - transform.position);

                        virtualBone = NewVirtualBone(average, rotation, transform.lossyScale, depth + 1, bone);
                        break;
                    }

                    case VRCPhysBoneBase.MultiChildType.Ignore:
                        // The branch point itself is not simulated; its children still are.
                        skipThisBone = true;
                        break;
                }
            }

            if (!skipThisBone)
            {
                bone.chain = this;
                bone.root = rootTransform;
                bone.transform = transform;
                bone.matrix = Matrix4x4.TRS(transform.position, rotation, transform.lossyScale);
                bone.depth = depth;
                bone.isEndBone = isEndBone;
                bone.child = virtualBone;

                // The walk is depth-first, so when the previous bone is still open-ended this bone is
                // the one that continues its run. Linking them here is what BuildChains later follows.
                BoneTransformData previous = bones.LastOrDefault();
                if (previous != null && !previous.isEndBone && previous.child == null)
                {
                    previous.child = bone;
                    bone.parent = previous;
                }

                bones.Add(bone);
            }

            if (virtualBone != null)
            {
                bones.Add(virtualBone);
            }

            foreach (Transform child in children)
            {
                BuildChain(child, depth + 1);
            }
        }

        private BoneTransformData NewVirtualBone(Vector3 position, Quaternion rotation, Vector3 scale, int depth, BoneTransformData parent)
        {
            return new BoneTransformData
            {
                chain = this,
                root = rootTransform,
                matrix = Matrix4x4.TRS(position, rotation, scale),
                depth = depth,
                isVirtual = true,
                isEndBone = true,
                parent = parent
            };
        }

        /// <summary>
        /// Groups <see cref="bones"/> into root-to-tip runs by following <see cref="BoneTransformData.child"/>,
        /// so a branching hierarchy can be drawn as a set of separate strands.
        /// </summary>
        internal void BuildChains()
        {
            HashSet<BoneTransformData> visited = new HashSet<BoneTransformData>();
            chains = new List<List<BoneTransformData>>();

            foreach (BoneTransformData bone in bones)
            {
                if (visited.Contains(bone))
                {
                    continue;
                }

                List<BoneTransformData> chain = new List<BoneTransformData>();
                for (BoneTransformData current = bone; current != null; current = current.child)
                {
                    chain.Add(current);
                    visited.Add(current);
                }

                chains.Add(chain);
            }
        }
    }
}
