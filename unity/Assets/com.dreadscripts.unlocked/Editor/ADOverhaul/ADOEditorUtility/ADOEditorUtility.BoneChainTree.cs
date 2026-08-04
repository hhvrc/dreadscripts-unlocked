// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested class BoneChainTree -> BoneChainTree, lines 1353-1497
//   GetNodeMatrices()          -> nodeMatrices (property), line 1366
//   .ctor(VRCPhysBone)         -> .ctor,                   line 1371
//   BuildNodes                 -> BuildNodes,              line 1380
//   BuildChains                -> BuildChains,             line 1479
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every field and every statement below was transcribed
// from the region above.
//
// DEOBF-BUG(resolved): GetNodeMatrices carried [SpecialName] with no matching setter -- ILSpy's
// rendering of a property getter it could not re-form. Restored as a read-only property.
//
// Nothing in this type was renamed by the protector: physBone, rootTransform, nodes, maxDepth,
// chains, BuildNodes and BuildChains all read as English, and none of them rhymes with the
// Serializer family the protector used across the rest of this class. Only the two BuildNodes
// parameters are generated names (`v`, `next_cust`), renamed to transform/depth below.
//
// ControllerEditor ships no equivalent; this is ADOverhaul's PhysBone chain analysis. The nodes it
// builds are in ADOEditorUtility.BoneNode.cs.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The joint tree a <see cref="VRCPhysBone"/> will actually simulate, resolved the way
        /// VRChat resolves it: ignore list applied, multi-child rule applied, and endpoints
        /// materialised as nodes.
        /// </summary>
        /// <remarks>
        /// The point of the type is that a PhysBone's simulated chain is not the same as its
        /// transform hierarchy. Ignored transforms drop out, a branch may be averaged into one
        /// virtual joint or dropped entirely, and a leaf may gain a virtual endpoint that no
        /// transform corresponds to. Anything drawing or measuring the chain has to see the resolved
        /// version, not the hierarchy.
        /// </remarks>
        internal class BoneChainTree
        {
            internal readonly VRCPhysBone physBone;

            internal readonly Transform rootTransform;

            /// <summary>
            /// Every node, in the depth-first order they were built. This is the order the endpoint
            /// linking below depends on.
            /// </summary>
            internal readonly List<BoneNode> nodes;

            /// <summary>The greatest <see cref="BoneNode.depth"/> in the tree, used to normalise depth to 0..1.</summary>
            internal readonly int maxDepth;

            /// <summary>Root-to-tip runs of nodes; null until <see cref="BuildChains"/> is called.</summary>
            internal List<List<BoneNode>> chains;

            /// <summary>Every node's world matrix, in <see cref="nodes"/> order.</summary>
            internal IEnumerable<Matrix4x4> nodeMatrices => nodes.Select(node => node.matrix);

            internal BoneChainTree(VRCPhysBone physBone)
            {
                this.physBone = physBone;
                rootTransform = physBone.GetRootTransform();
                nodes = new List<BoneNode>();
                BuildNodes(rootTransform, 0);
                maxDepth = nodes.Max(node => node.depth);
            }

            /// <summary>
            /// Adds <paramref name="transform"/> and everything below it to <see cref="nodes"/>,
            /// depth first.
            /// </summary>
            /// <param name="depth">Depth of <paramref name="transform"/> itself.</param>
            /// <remarks>
            /// <para>
            /// Three cases decide what happens at a joint, and they follow VRChat's own rules:
            /// </para>
            /// <list type="bullet">
            /// <item><description>
            /// No children left after the ignore list: the joint is an end bone. If the PhysBone has
            /// an endpoint offset, a virtual node is added past it, oriented along the offset;
            /// otherwise the joint inherits the previous node's rotation, since a leaf has no child
            /// to aim at.
            /// </description></item>
            /// <item><description>
            /// Several children, with the multi-child rule set to Average: the joint is aimed at the
            /// mean of the children's positions and a virtual end node is placed there.
            /// </description></item>
            /// <item><description>
            /// Several children, with the rule set to Ignore: the joint itself is skipped -- it gets
            /// no node -- while its children are still walked. That is what the <c>skipSelf</c> flag
            /// carries.
            /// </description></item>
            /// </list>
            /// <para>
            /// Parent/child links are stitched by looking at the node added immediately before this
            /// one, which is correct only because the walk is depth first and because a node that
            /// already has a child or is an end bone is never linked to. A branch therefore leaves
            /// the second and later children unlinked to a parent, which is why
            /// <see cref="BuildChains"/> treats them as chain starts.
            /// </para>
            /// </remarks>
            internal void BuildNodes(Transform transform, int depth)
            {
                bool skipSelf = false;
                BoneNode node = new BoneNode();
                BoneNode child = null;
                BoneNode virtualEndNode = null;
                Quaternion rotation = transform.rotation;

                List<Transform> children = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform candidate = transform.GetChild(i);
                    if (!physBone.ignoreTransforms.Contains(candidate))
                    {
                        children.Add(candidate);
                    }
                }

                bool isEndBone = children.Count == 0;
                if (isEndBone)
                {
                    if (physBone.endpointPosition != Vector3.zero)
                    {
                        Vector3 endpoint = transform.TransformPoint(physBone.endpointPosition);
                        rotation = transform.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(physBone.endpointPosition));

                        child = virtualEndNode = new BoneNode
                        {
                            tree = this,
                            root = rootTransform,
                            matrix = Matrix4x4.TRS(endpoint, rotation, transform.lossyScale),
                            depth = depth + 1,
                            isVirtual = true,
                            isEndBone = true,
                            parent = node
                        };
                    }
                    else if (nodes.Count != 0)
                    {
                        rotation = nodes[nodes.Count - 1].matrix.rotation;
                    }
                }
                else if (children.Count > 1)
                {
                    if (physBone.multiChildType == VRCPhysBoneBase.MultiChildType.Average)
                    {
                        Vector3 average = Vector3.zero;
                        foreach (Transform candidate in children)
                        {
                            average += candidate.position;
                        }

                        average /= (float)children.Count;
                        rotation = transform.rotation * Quaternion.FromToRotation(transform.up, average - transform.position);

                        child = virtualEndNode = new BoneNode
                        {
                            tree = this,
                            root = rootTransform,
                            matrix = Matrix4x4.TRS(average, rotation, transform.lossyScale),
                            depth = depth + 1,
                            isVirtual = true,
                            isEndBone = true,
                            parent = node
                        };
                    }
                    else if (physBone.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
                    {
                        skipSelf = true;
                    }
                }

                if (!skipSelf)
                {
                    node.tree = this;
                    node.root = rootTransform;
                    node.transform = transform;
                    node.matrix = Matrix4x4.TRS(transform.position, rotation, transform.lossyScale);
                    node.depth = depth;
                    node.isEndBone = isEndBone;
                    node.child = child;

                    BoneNode previous = nodes.LastOrDefault();
                    if (previous != null && !previous.isEndBone && previous.child == null)
                    {
                        previous.child = node;
                        node.parent = previous;
                    }

                    nodes.Add(node);
                }

                if (virtualEndNode != null)
                {
                    nodes.Add(virtualEndNode);
                }

                foreach (Transform candidate in children)
                {
                    BuildNodes(candidate, depth + 1);
                }
            }

            /// <summary>
            /// Groups <see cref="nodes"/> into root-to-tip runs and stores them in
            /// <see cref="chains"/>.
            /// </summary>
            /// <remarks>
            /// Each unvisited node starts a chain that is then walked down its <c>child</c> links to
            /// the tip, so a branching tree becomes one chain per branch. Rebuilds
            /// <see cref="chains"/> from scratch on every call.
            /// </remarks>
            internal void BuildChains()
            {
                HashSet<BoneNode> visited = new HashSet<BoneNode>();
                chains = new List<List<BoneNode>>();

                foreach (BoneNode node in nodes)
                {
                    if (visited.Contains(node))
                    {
                        continue;
                    }

                    List<BoneNode> chain = new List<BoneNode>();
                    for (BoneNode current = node; current != null; current = current.child)
                    {
                        chain.Add(current);
                        visited.Add(current);
                    }

                    chains.Add(chain);
                }
            }
        }
    }
}
