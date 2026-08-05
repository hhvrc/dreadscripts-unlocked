// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum LayerViewViewType` nested in the static ControllerEditor class,
// line 3019 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor: the
// decompiled type is nested inside a god-class that is not ported, so it becomes a top-level
// `internal` type in the same namespace. Nothing outside that class named it, so the change of
// nesting has no call-site consequences.
//
// Audit status: VERIFIED -- the three members and their implicit ordinals were diffed against the
// `private enum LayerViewViewType` still at line 3019 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; names and order are
// identical. The remarks were checked against use: the value is held in the single static
// `layerViewType` (line 8380, initialised to DefaultView), the picker at line 16267 does build its
// menu with `Enum.GetValues(typeof(LayerViewViewType))` and `ToString().Humanize()`, the
// DefaultView guards that hand drawing back to Unity are at 16121/16223/16779/17006, and the tree
// builder at 16792-16818 files a layer under each of its tags and untagged layers under the base
// category, exactly as described.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Which layout the patched Animator layer list is drawn in.
    /// </summary>
    /// <remarks>
    /// The selected value is held in a single static and read by the Harmony patches on Unity's
    /// internal <c>LayerControllerView</c>; <see cref="DefaultView"/> means "stand aside and let
    /// Unity draw its own flat list", while the two category values swap in the tool's own
    /// <see cref="LayerPathNode"/> tree.
    ///
    /// The member names are user-visible: the view picker builds its menu by enumerating this type
    /// and nicifying <c>ToString()</c>, so renaming a member renames the menu entry.
    /// </remarks>
    internal enum LayerViewViewType
    {
        /// <summary>Unity's own flat, unmodified layer list.</summary>
        DefaultView,

        /// <summary>
        /// Layers grouped into a folder tree by splitting their names on the configured category
        /// delimiter, so a layer called <c>Locomotion/Idle</c> is filed under <c>Locomotion</c>.
        /// </summary>
        CategoryByName,

        /// <summary>
        /// Layers grouped by the tags attached to them. A tag is recorded as a muted any-state exit
        /// transition on the layer's state machine whose name is the tag; a layer carrying several
        /// tags is filed under each of them, and an untagged layer falls into the base category.
        /// </summary>
        CategoryByTag
    }
}
