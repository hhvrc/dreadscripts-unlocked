// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private enum ActionMode` nested in the static ControllerEditor class,
// line 2166 of the current snapshot. Line numbers move with the snapshot; the names are the durable
// reference.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.
//
// The behaviour described below is read from InterruptVisitor (line 13191) and PatchVisitor
// (line 13170), neither of which is ported.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Which part of the controller a <see cref="ControllerAction"/> is applied to.
    /// </summary>
    /// <remarks>
    /// THE ORDINALS ARE LOAD-BEARING, and shared with <see cref="MoveMode"/>. The layer-selecting
    /// helper takes a plain <c>int</c> and is called with either enum cast to it, so the first three
    /// members of the two types must keep both their names and their positions: 0 selects every
    /// layer, 1 selects the tagged ones, and 2 and 3 both select the layer that owns the current
    /// state machine.
    ///
    /// The member names are user-visible: the picker is a plain <c>EnumPopup</c>, so Unity nicifies
    /// these identifiers into the labels shown. Declaration order is menu order.
    /// </remarks>
    internal enum ActionMode
    {
        /// <summary>Every layer of the controller being edited.</summary>
        CurrentController,

        /// <summary>Only the layers carrying the tag typed alongside the action.</summary>
        LayersTaggedWith,

        /// <summary>Only the selected layer.</summary>
        CurrentLayer,

        /// <summary>
        /// Only the state machine currently open in the animator window, which may be a sub-state
        /// machine rather than the layer's root. This is the one scope that does not recurse into
        /// nested state machines.
        /// </summary>
        CurrentStatemachine
    }
}
