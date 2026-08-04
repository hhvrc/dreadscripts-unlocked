// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ControllerAction  -> ControllerAction,  lines 2156-2164 (vendor names; unobfuscated in the
//   ActionMode        -> ActionMode,        lines 2166-2172  shipped build)
//   MoveMode          -> MoveMode,          lines 2174-2179
//   MoveDestination   -> MoveDestination,   lines 2181-2185
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Grouped into one file: four small enums that together describe one dialog — which batch action
// runs, over what, and where its result goes.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>Which batch operation the controller-wide action dialog performs.</summary>
        private enum ControllerAction
        {
            ReplaceParameter,
            RemoveParameter,
            Copy,
            TagCurrentLayerWith,
            RemoveLayersWithTag,
            RemoveTag
        }

        /// <summary>
        /// How far a <see cref="ControllerAction"/> reaches: the whole controller, only the layers
        /// carrying a given tag, only the open layer, or only the open state machine.
        /// </summary>
        private enum ActionMode
        {
            CurrentController,
            LayersTaggedWith,
            CurrentLayer,
            CurrentStatemachine
        }

        /// <summary>
        /// Which layers <see cref="ControllerAction.Copy"/> takes. Same idea as
        /// <see cref="ActionMode"/> without the state-machine scope, which a copy cannot use.
        /// </summary>
        private enum MoveMode
        {
            CurrentController,
            LayersTaggedWith,
            CurrentLayer
        }

        /// <summary>
        /// Where <see cref="ControllerAction.Copy"/> puts the copied layers: into a controller the
        /// user picks, or back into the one being edited.
        /// </summary>
        private enum MoveDestination
        {
            Controller,
            CurrentController
        }
    }
}
