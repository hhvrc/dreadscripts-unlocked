// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   LayerViewViewType -> LayerViewViewType, lines 3019-3024 (vendor name; unobfuscated in the
//                        shipped build)
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// How the layer list is grouped: flat in controller order, or as a category tree built
        /// from the delimiter in each layer's name, or from each layer's state-machine tag.
        /// </summary>
        /// <remarks>
        /// Both category modes build a <see cref="LayerPathNode"/> tree; they differ only in where
        /// the path comes from.
        /// </remarks>
        private enum LayerViewViewType
        {
            DefaultView,
            CategoryByName,
            CategoryByTag
        }
    }
}
