// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private class LayerPathNode` nested in the static ControllerEditor class,
// lines 3026-3163 of the current snapshot. Line numbers move with the snapshot; the member names
// below are the durable reference.
//
//   name / depth / fullPath / children / layers / baseCategoryNode (3028-3038) -> unchanged
//   CategoryPath()            (3041) -> CategoryPath, restored to a property (see below)
//   .ctor(v, result, dirPtr)  (3046) -> LayerPathNode(name, fullPath, categoryDelimiter,
//                                       baseCategoryName, depth)
//   AddLayer(key, selection, positionc) (3053) -> AddLayer(path, layer, layerIndex)
//   AddEntry(res)             (3078) -> AddEntry(entry)
//   FindClosest(info)         (3086) -> FindClosest(path)
//   FindNode(def)             (3103) -> FindNode(path)
//   GetOrCreateBaseCategory() (3120) -> GetOrCreateBaseCategory()
//   WalkPath(param, selection, isres) (3134) -> WalkPath(path, action, includeSelf)
//   StripRootPrefix(item)     (3159) -> StripRootPrefix(path), now an instance method
//
// CategoryPath carried [SpecialName] in the decompiled output with no arguments and no return-value
// side effects, which is how ILSpy renders a property getter whose `get_` prefix the deobfuscator
// has already stripped. It is restored here as a property; the outer class's call sites read it as
// `node.CategoryPath()`, so a property is what the original source had.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// ============================ DELIBERATE DEVIATION ============================
// The decompiled methods read two user settings through statics on the enclosing ControllerEditor
// class, which is not ported:
//
//   PushInitializer() -> EditorSettings.GetInstance().categoryDelimiter, default "/", line 16159
//   ValidateInitializer() -> EditorSettings.GetInstance().categoryBaseName,  default "Base", line 16113
//   QueryMapper(task) -> task.Split(new[] { PushInitializer() }, StringSplitOptions.None), line 16923
//
// EditorSettings (line 437) is not ported either, so every method of this type except AddEntry and
// the constructor depends on unported code. Rather than defer nine tenths of the type, the two
// setting values are taken as constructor arguments on the root node and inherited by every child
// this type creates for itself. QueryMapper, which is used only by this type and by the layer-view
// drawing code in the outer class, becomes the private SplitPath below.
//
// No method signature other than the constructor's changes, and the only place a root node is ever
// constructed is the unported outer class (`new LayerPathNode("Root", "Root")`, line 16266), so
// there are no call sites to break. The one observable difference: the original re-reads the
// settings on every call, so editing the delimiter would immediately change how an already-built
// tree resolves paths, whereas here a tree keeps the values it was built with. The outer class
// rebuilds the whole tree whenever those settings change, so this is not reachable in practice.
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One folder in the category view's layer tree: the layers filed directly under it, and the
    /// sub-categories below it.
    /// </summary>
    /// <remarks>
    /// A tree is built by feeding every layer of the controller to <see cref="AddLayer"/> on a root
    /// node named <c>"Root"</c>. The path handed in is either the layer's own name (for
    /// <see cref="LayerViewViewType.CategoryByName"/>, so <c>"Locomotion/Idle"</c> files the layer
    /// under <c>Locomotion</c>) or a tag followed by a dummy trailing segment (for
    /// <see cref="LayerViewViewType.CategoryByTag"/>).
    ///
    /// The last segment of a path is always treated as the layer's own name rather than as a
    /// category, which is why the tag view has to append a throwaway <c>"/DUMMY"</c> segment to make
    /// the tag itself become a folder.
    ///
    /// <see cref="layers"/> is cumulative rather than exclusive: a node lists every layer beneath it
    /// at any depth, not just those filed directly under it, because <see cref="AddLayer"/> records
    /// the entry on each node it descends through.
    /// </remarks>
    internal class LayerPathNode
    {
        /// <summary>This category's own name, without any of its ancestors.</summary>
        internal readonly string name;

        /// <summary>Distance from the root node, which is zero.</summary>
        internal readonly int depth;

        /// <summary>Delimiter-joined path from the root node inclusive, so it begins with <c>"Root"</c>.</summary>
        internal readonly string fullPath;

        internal readonly List<LayerPathNode> children = new List<LayerPathNode>();

        /// <summary>Every layer at or below this node, in the order it was added.</summary>
        internal readonly List<LayerIndexEntry> layers = new List<LayerIndexEntry>();

        /// <summary>
        /// The child category that collects layers filed directly under this node rather than under
        /// one of its sub-categories. Created lazily by <see cref="GetOrCreateBaseCategory"/>.
        /// </summary>
        internal LayerPathNode baseCategoryNode;

        /// <summary>The configured separator between category names, from the tool's settings.</summary>
        private readonly string categoryDelimiter;

        /// <summary>The configured display name of the uncategorised bucket, from the tool's settings.</summary>
        private readonly string baseCategoryName;

        /// <summary><see cref="fullPath"/> with the leading <c>"Root"</c> removed, for display.</summary>
        internal string CategoryPath
        {
            get
            {
                return StripRootPrefix(fullPath);
            }
        }

        internal LayerPathNode(string name, string fullPath, string categoryDelimiter, string baseCategoryName, int depth = 0)
        {
            this.name = name;
            this.depth = depth;
            this.fullPath = fullPath;
            this.categoryDelimiter = categoryDelimiter;
            this.baseCategoryName = baseCategoryName;
        }

        /// <summary>
        /// Files a layer under <paramref name="path"/>, creating the intermediate categories it names,
        /// and returns the node the layer's own segment resolved to.
        /// </summary>
        /// <remarks>
        /// The final segment names the layer, not a category, so it is never turned into a folder:
        /// once the remainder of the path is empty the layer lands in this node's base category
        /// instead. The one exception is a final segment that happens to match a category that
        /// already exists, in which case the layer is recorded both on that category and on the base
        /// category — a layer named <c>"A"</c> added after <c>"A/B"</c> shows up in both places.
        ///
        /// SHIPPED BUG, PRESERVED: the guard that creates a missing child tests
        /// <c>IsNullOrWhiteSpace</c> on the remainder while the guard that recurses into it tests
        /// <c>IsNullOrEmpty</c>. A remainder that is whitespace but not empty — as produced by a
        /// layer name ending in the delimiter followed by a space, e.g. <c>"A/ "</c> — passes the
        /// second test but not the first, so the recursion dereferences a node that was never
        /// created and throws a <see cref="NullReferenceException"/>.
        /// </remarks>
        internal LayerPathNode AddLayer(string path, AnimatorControllerLayer layer, int layerIndex)
        {
            LayerIndexEntry entry = new LayerIndexEntry(layer, layerIndex);
            AddEntry(entry);

            string[] segments = SplitPath(path);
            string head = segments[0];
            string remainder = string.Join(categoryDelimiter, segments, 1, segments.Length - 1);

            LayerPathNode node = FindNode(head);
            if (node == null && !string.IsNullOrWhiteSpace(remainder))
            {
                node = new LayerPathNode(head, fullPath + categoryDelimiter + head, categoryDelimiter, baseCategoryName, depth + 1);
                children.Add(node);
            }

            if (!string.IsNullOrEmpty(remainder))
            {
                return node.AddLayer(remainder, layer, layerIndex);
            }

            node?.AddEntry(entry);
            if (node != GetOrCreateBaseCategory())
            {
                GetOrCreateBaseCategory().AddEntry(entry);
            }

            return node;
        }

        /// <summary>Records a layer on this node unless one with the same controller index is already there.</summary>
        internal void AddEntry(LayerIndexEntry entry)
        {
            if (layers.All(l => l.layerIndex != entry.layerIndex))
            {
                layers.Add(entry);
            }
        }

        /// <summary>
        /// Walks as far down <paramref name="path"/> as categories actually exist and returns the
        /// deepest node reached, which is this node when the very first segment does not match.
        /// </summary>
        /// <remarks>
        /// Used to keep a selection anchored when the tree is rebuilt: a category that has since
        /// disappeared resolves to its nearest surviving ancestor rather than to nothing.
        /// </remarks>
        internal LayerPathNode FindClosest(string path)
        {
            string[] segments = SplitPath(path);
            LayerPathNode current = this;
            foreach (string segment in segments)
            {
                LayerPathNode next = current.FindNode(segment);
                if (next == null)
                {
                    break;
                }

                current = next;
            }

            return current;
        }

        /// <summary>
        /// Resolves a delimiter-separated path below this node, or null if any segment is missing.
        /// </summary>
        internal LayerPathNode FindNode(string path)
        {
            string[] segments = SplitPath(path);
            string head = segments[0];
            string remainder = segments.Length > 1 ? string.Join(categoryDelimiter, segments, 1, segments.Length - 1) : "";

            LayerPathNode child = children.FirstOrDefault(c => c.name == head);
            if (child == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(remainder))
            {
                return child.FindNode(remainder);
            }

            return child;
        }

        /// <summary>
        /// The child category that holds this node's own layers, created on first use.
        /// </summary>
        /// <remarks>
        /// The lookup by name runs before the field is trusted to be unset, so a base category that
        /// was created by an earlier <see cref="AddLayer"/> as an ordinary child — which happens when
        /// a layer is literally named after the configured base-category name — is adopted rather
        /// than duplicated.
        /// </remarks>
        internal LayerPathNode GetOrCreateBaseCategory()
        {
            if (baseCategoryNode != null)
            {
                return baseCategoryNode;
            }

            baseCategoryNode = FindNode(baseCategoryName);
            if (baseCategoryNode == null)
            {
                baseCategoryNode = new LayerPathNode(baseCategoryName, fullPath + categoryDelimiter + baseCategoryName, categoryDelimiter, baseCategoryName, depth + 1);
                children.Add(baseCategoryNode);
            }

            return baseCategoryNode;
        }

        /// <summary>
        /// Invokes <paramref name="action"/> on each node along <paramref name="path"/>, stopping at
        /// the first segment that does not exist.
        /// </summary>
        /// <param name="includeSelf">Whether this node is visited before descending.</param>
        /// <remarks>
        /// SHIPPED BUG, PRESERVED: this is the one path-walking method that splits on a hard-coded
        /// <c>'/'</c> instead of the configured category delimiter. With the default delimiter the
        /// two agree; change the delimiter in the settings and this method silently walks nothing,
        /// because the whole path arrives as a single segment that matches no child.
        /// </remarks>
        internal void WalkPath(string path, Action<LayerPathNode> action, bool includeSelf = true)
        {
            if (includeSelf)
            {
                action(this);
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] segments = path.Split('/');
            LayerPathNode current = this;
            foreach (string segment in segments)
            {
                current = current.FindNode(segment);
                if (current == null)
                {
                    break;
                }

                action(current);
            }
        }

        private string[] SplitPath(string path)
        {
            return path.Split(new[] { categoryDelimiter }, StringSplitOptions.None);
        }

        /// <summary>
        /// Drops the leading <c>"Root"</c>, and the delimiter after it, from a full path.
        /// </summary>
        /// <remarks>
        /// SHIPPED BUG, PRESERVED: the pattern is built as <c>"^Root" + Regex.Escape(delimiter) + "?"</c>,
        /// so the <c>?</c> makes only the *last* character of the delimiter optional. With a
        /// single-character delimiter that is the intent; with a longer one, the root node's own path
        /// (<c>"Root"</c>, with no delimiter after it) no longer matches and is returned unchanged.
        /// </remarks>
        private string StripRootPrefix(string path)
        {
            return Regex.Replace(path, "^Root" + Regex.Escape(categoryDelimiter) + "?", "");
        }
    }
}
