// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum ControllerAction` nested in the static ControllerEditor class,
// line 2156 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// The behaviour described below is read from the batch-action dispatcher (LogoutVisitor, line 13048)
// and the toolbar that drives it (line 11830). Neither is ported.
//
// Audit status: VERIFIED -- the six members and their implicit ordinals were diffed against the
// `private enum ControllerAction` still at line 2156 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; names and order are
// identical. Every behavioural claim in the member docs was re-read out of the dispatcher
// (LogoutVisitor, still line 13048) rather than trusted: the parameter-list rewrite in
// ReplaceParameter/RemoveParameter really is gated on `actionScope != ActionMode.CurrentController`;
// TagCurrentLayerWith really is a no-op when a matching transition exists and writes an any-state
// transition with isExit, mute and the tag as its name; RemoveTag matches on name *and* isExit;
// and the SHIPPED INCONSISTENCY on RemoveLayersWithTag is real -- it matches
// `anyStateTransitions.Any(t => t.name == actionFilterText)` with no isExit test, and iterates
// ActiveController().layers directly, ignoring the scope selector. The toolbar line 11830 still
// holds the EnumPopup for this type.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The batch operation the controller toolbar's Apply button performs.
    /// </summary>
    /// <remarks>
    /// The scope the operation runs over is chosen separately, with <see cref="ActionMode"/> for the
    /// parameter and tag actions and <see cref="MoveMode"/> for <see cref="Copy"/>.
    ///
    /// The member names are user-visible: the picker is a plain <c>EnumPopup</c>, so Unity nicifies
    /// these identifiers into the labels shown ("Replace Parameter", "Tag Current Layer With", and
    /// so on). Declaration order is menu order.
    ///
    /// A "tag" is not a Unity concept — the tool records one as a muted any-state transition on the
    /// layer's state machine, with <c>isExit</c> set and the tag as its name, which is inert at
    /// runtime and survives round-tripping through the controller asset.
    /// </remarks>
    internal enum ControllerAction
    {
        /// <summary>
        /// Rewrites every use of a parameter name within the scope, and — only when the scope is the
        /// whole controller — renames the matching entries in the controller's parameter list too.
        /// A separate "Match Whole Word" toggle decides between exact matches and substring matches.
        /// </summary>
        ReplaceParameter,

        /// <summary>
        /// Strips every use of a parameter within the scope, and — only when the scope is the whole
        /// controller — deletes the matching entries from the controller's parameter list too.
        /// </summary>
        RemoveParameter,

        /// <summary>
        /// Copies the layers in scope into a destination controller, optionally adding the parameters
        /// they need and a suffix to those parameters.
        /// </summary>
        Copy,

        /// <summary>
        /// Attaches a tag to the currently selected layer, doing nothing if it already carries it.
        /// </summary>
        TagCurrentLayerWith,

        /// <summary>
        /// Deletes every layer of the current controller carrying a tag, ignoring the scope selector.
        /// </summary>
        /// <remarks>
        /// SHIPPED INCONSISTENCY, PRESERVED: this is the only tag operation that matches an any-state
        /// transition by name alone without also requiring <c>isExit</c>, so an ordinary any-state
        /// transition that happens to share the tag's name will cause its layer to be deleted.
        /// </remarks>
        RemoveLayersWithTag,

        /// <summary>
        /// Removes a tag from every layer in scope.
        /// </summary>
        RemoveTag
    }
}
