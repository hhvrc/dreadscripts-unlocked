// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   LayerPathNode  -> LayerPathNode, lines 3026-3163 (all member names already in renames/)
//     name, depth, fullPath, children, layers, baseCategoryNode -> unchanged, lines 3028-3038
//     CategoryPath()             -> CategoryPath,            line 3041  [SpecialName getter]
//     AddLayer, AddEntry, FindClosest, FindNode,
//     GetOrCreateBaseCategory, WalkPath, StripRootPrefix     -> unchanged, lines 3053-3159
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   QueryMapper, PushInitializer, ValidateInitializer -- ControllerEditor outer class body
//                                                        (split a layer name into path segments;
//                                                         the category delimiter setting; the
//                                                         uncategorised-category name setting)
//
// Audit status: VERIFIED against export member-by-member (2026-08-04). One vendor quirk preserved,
// see the remark on AddLayer.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// One node of the category tree the layer list is grouped by, built from the delimiter
        /// inside each layer's name.
        /// </summary>
        /// <remarks>
        /// A layer called <c>Face/Eyes/Blink</c> contributes a <c>Face</c> node holding an
        /// <c>Eyes</c> node holding the layer. A node's <see cref="layers"/> holds every layer at or
        /// below it, so the tree can be drawn collapsed without re-walking, and
        /// <see cref="GetOrCreateBaseCategory"/> supplies the bucket for layers whose name carries
        /// no delimiter at all.
        /// </remarks>
        private class LayerPathNode
        {
            internal readonly string name;

            internal readonly int depth;

            /// <summary>Path from the root, delimiter-joined, including the synthetic "Root".</summary>
            internal readonly string fullPath;

            internal readonly List<LayerPathNode> children = new List<LayerPathNode>();

            /// <summary>Every layer at or below this node.</summary>
            internal readonly List<LayerIndexEntry> layers = new List<LayerIndexEntry>();

            internal LayerPathNode baseCategoryNode;

            /// <summary><see cref="fullPath"/> without the synthetic "Root" prefix.</summary>
            internal string CategoryPath => StripRootPrefix(fullPath);

            internal LayerPathNode(string name, string fullPath, int depth = 0)
            {
                this.name = name;
                this.depth = depth;
                this.fullPath = fullPath;
            }

            /// <summary>
            /// Files a layer under <paramref name="categoryPath"/>, creating the nodes it needs, and
            /// returns the node it landed in.
            /// </summary>
            /// <remarks>
            /// Preserved vendor quirk: when the remaining path is whitespace but not empty, the node
            /// is not created and the recursive call dereferences null. Reaching it needs a layer
            /// name whose category segment is blank.
            /// </remarks>
            internal LayerPathNode AddLayer(string categoryPath, AnimatorControllerLayer layer, int layerIndex)
            {
                LayerIndexEntry entry = new LayerIndexEntry(layer, layerIndex);
                AddEntry(entry);

                string[] segments = QueryMapper(categoryPath);
                string head = segments[0];
                string rest = string.Join(PushInitializer(), segments, 1, segments.Length - 1);

                LayerPathNode child = FindNode(head);
                if (child == null && !rest.IsNullOrWhiteSpace())
                {
                    child = new LayerPathNode(head, fullPath + PushInitializer() + head, depth + 1);
                    children.Add(child);
                }

                if (!rest.IsNullOrEmpty())
                {
                    return child.AddLayer(rest, layer, layerIndex);
                }

                child?.AddEntry(entry);
                if (child != GetOrCreateBaseCategory())
                {
                    GetOrCreateBaseCategory().AddEntry(entry);
                }

                return child;
            }

            internal void AddEntry(LayerIndexEntry entry)
            {
                if (layers.All(l => l.layerIndex != entry.layerIndex))
                {
                    layers.Add(entry);
                }
            }

            /// <summary>
            /// Walks as far down <paramref name="categoryPath"/> as nodes exist and returns the
            /// deepest one reached, which is this node when the first segment is already missing.
            /// </summary>
            internal LayerPathNode FindClosest(string categoryPath)
            {
                LayerPathNode node = this;
                foreach (string segment in QueryMapper(categoryPath))
                {
                    LayerPathNode next = node.FindNode(segment);
                    if (next == null)
                    {
                        break;
                    }

                    node = next;
                }

                return node;
            }

            /// <summary>Resolves a whole path below this node, or null if any segment is missing.</summary>
            internal LayerPathNode FindNode(string categoryPath)
            {
                string[] segments = QueryMapper(categoryPath);
                string head = segments[0];
                string rest = segments.Length > 1
                    ? string.Join(PushInitializer(), segments, 1, segments.Length - 1)
                    : "";

                LayerPathNode child = children.FirstOrDefault(c => c.name == head);
                if (child == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(rest))
                {
                    return child.FindNode(rest);
                }

                return child;
            }

            /// <summary>The bucket for layers with no category of their own, created on demand.</summary>
            internal LayerPathNode GetOrCreateBaseCategory()
            {
                if (baseCategoryNode != null)
                {
                    return baseCategoryNode;
                }

                baseCategoryNode = FindNode(ValidateInitializer());
                if (baseCategoryNode == null)
                {
                    children.Add(baseCategoryNode = new LayerPathNode(
                        ValidateInitializer(),
                        fullPath + PushInitializer() + ValidateInitializer(),
                        depth + 1));
                }

                return baseCategoryNode;
            }

            /// <summary>
            /// Invokes <paramref name="visit"/> on every node along <paramref name="categoryPath"/>,
            /// stopping at the first missing segment.
            /// </summary>
            /// <param name="includeSelf">Whether this node is visited before the walk starts.</param>
            internal void WalkPath(string categoryPath, Action<LayerPathNode> visit, bool includeSelf = true)
            {
                if (includeSelf)
                {
                    visit(this);
                }

                if (categoryPath.IsNullOrEmpty())
                {
                    return;
                }

                LayerPathNode node = this;
                foreach (string segment in categoryPath.Split('/'))
                {
                    node = node.FindNode(segment);
                    if (node == null)
                    {
                        break;
                    }

                    visit(node);
                }
            }

            private static string StripRootPrefix(string path)
            {
                return Regex.Replace(path, "^Root" + Regex.Escape(PushInitializer()) + "?", "");
            }
        }
    }
}
