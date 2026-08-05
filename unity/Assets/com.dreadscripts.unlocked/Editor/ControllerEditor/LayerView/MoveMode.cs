// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum MoveMode` nested in the static ControllerEditor class,
// line 2174 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// The behaviour described below is read from PatchVisitor (line 13170) and the Copy branch of the
// batch-action dispatcher (line 13130), neither of which is ported.
//
// Audit status: VERIFIED -- the three members and their implicit ordinals were diffed against the
// `private enum MoveMode` still at line 2174 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; names and order are
// identical, and the type really is ActionMode minus CurrentStatemachine. The shared-ordinals claim
// was confirmed at the call site: the Copy branch of the dispatcher passes `(int)copySourceScope`
// into the same PatchVisitor(int) selector that ActionMode is cast into. The prose line numbers for
// PatchVisitor and the Copy branch are two to three lines stale (13173 and 13132 now); left alone
// as ordinary snapshot drift.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Which layers <see cref="ControllerAction.Copy"/> takes as its source.
    /// </summary>
    /// <remarks>
    /// THE ORDINALS ARE LOAD-BEARING, and shared with <see cref="ActionMode"/>: the layer-selecting
    /// helper takes a plain <c>int</c> and is called with either enum cast to it. This type is
    /// <see cref="ActionMode"/> minus its state-machine scope, which has no meaning when whole layers
    /// are being copied, so the three members it does have must keep their positions.
    ///
    /// The member names are user-visible: the picker is a plain <c>EnumPopup</c>, so Unity nicifies
    /// these identifiers into the labels shown. Declaration order is menu order.
    /// </remarks>
    internal enum MoveMode
    {
        /// <summary>Every layer of the controller being edited.</summary>
        CurrentController,

        /// <summary>Only the layers carrying the tag typed alongside the action.</summary>
        LayersTaggedWith,

        /// <summary>Only the selected layer.</summary>
        CurrentLayer
    }
}
