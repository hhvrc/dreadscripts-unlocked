// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private struct LayerIndexEntry` nested in the static ControllerEditor class,
// lines 3165-3188 of the current snapshot. Line numbers move with the snapshot; the member names
// below are the durable reference.
//
//   layer                 (3167) -> layer
//   layerIndex            (3169) -> layerIndex
//   .ctor(value, next_cfg)(3173) -> LayerIndexEntry(layer, layerIndex)
//   implicit operator     (3179) -> implicit operator AnimatorControllerLayer
//
// NOT PORTED — obfuscator scaffolding:
//   NewProduct    (3171)  a private static object that is never assigned
//   LoginProduct()(3184)  an internal static bool that returns `NewProduct == null`, i.e. always
//                         true. This always-null-static/null-check pair is the licensing-gate
//                         remnant that appears on dozens of types throughout the assembly (compare
//                         PrintState/ResolveState at line 2143); it carries no behaviour.
//
// LIFTED OUT OF ControllerEditor, following the convention already used for PhysBoneEditor.

using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// An animator layer paired with the index it occupies in its controller's <c>layers</c> array.
    /// </summary>
    /// <remarks>
    /// The category view reorders and regroups layers for display, but every operation on a layer —
    /// selecting it, removing it, writing it back — has to address it by its index in the real
    /// controller. Carrying the index alongside the layer avoids searching the array for it, which
    /// would in any case be ambiguous because <see cref="AnimatorControllerLayer"/> is a plain
    /// value wrapper and duplicate layer names are legal.
    ///
    /// The implicit conversion lets an entry be passed anywhere a layer is expected, so call sites
    /// that do not care about the index need not unwrap it.
    /// </remarks>
    internal struct LayerIndexEntry
    {
        internal readonly AnimatorControllerLayer layer;

        /// <summary>Index of <see cref="layer"/> in its controller's <c>layers</c> array.</summary>
        internal readonly int layerIndex;

        internal LayerIndexEntry(AnimatorControllerLayer layer, int layerIndex)
        {
            this.layer = layer;
            this.layerIndex = layerIndex;
        }

        public static implicit operator AnimatorControllerLayer(LayerIndexEntry entry)
        {
            return entry.layer;
        }
    }
}
