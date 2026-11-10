// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum MoveDestination` nested in the static ControllerEditor class,
// line 2181 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// The behaviour described below is read from the Copy branch of the batch-action dispatcher
// (line 13130) and the toolbar that drives it (line 11908), neither of which is ported.
//
// Audit status: VERIFIED -- both members and their implicit ordinals were diffed against the
// `private enum MoveDestination` still at line 2181 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; names and order are
// identical. The remarks were checked at the call sites: the EnumPopup is still at line 11908, the
// Copy branch resolves the destination as `copyDestination != Controller ? ActiveController() :
// actionTargetController` with no same-controller guard, and the Apply button's disabling condition
// (line 11933) includes `selectedAction == Copy && copyDestination == Controller &&
// !actionTargetController`, which is exactly the default-value consequence described above.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Where <see cref="ControllerAction.Copy"/> puts the layers it copies.
    /// </summary>
    /// <remarks>
    /// The member names are user-visible: the picker is a plain <c>EnumPopup</c>, so Unity nicifies
    /// these identifiers into "Controller" and "Current Controller". Declaration order is menu order,
    /// which makes <see cref="Controller"/> the default value of a freshly constructed field — hence
    /// the Apply button being disabled until a controller has actually been assigned to the object
    /// field this value reveals.
    /// </remarks>
    internal enum MoveDestination
    {
        /// <summary>
        /// A controller picked in the object field that appears next to this selector. Copying into
        /// the same controller the layers came from is not prevented, and duplicates them.
        /// </summary>
        Controller,

        /// <summary>The controller currently being edited, duplicating the layers in place.</summary>
        CurrentController
    }
}
