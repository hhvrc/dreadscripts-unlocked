// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the `private struct LayerIndexEntry` nested in the static ControllerEditor class.
//
//   LayerIndexEntry             -> LayerIndexEntry, lines 3165-3188
//   layer                       -> layer, line 3167
//   layerIndex                  -> layerIndex, line 3169
//   .ctor(value, next_cfg)      -> LayerIndexEntry(layer, layerIndex), line 3173
//   implicit operator           -> implicit operator AnimatorControllerLayer, line 3179
//   NewProduct                  -> NOT PORTED, line 3171 -- obfuscator scaffolding: a private
//                                  static object that is never assigned.
//   LoginProduct()              -> NOT PORTED, line 3184 -- an internal static bool returning
//                                  `NewProduct == null`, i.e. always true. This always-null-static
//                                  plus null-check pair is the licensing-gate remnant that appears
//                                  on dozens of types throughout the assembly (compare
//                                  PrintState/ResolveState at line 2143); it carries no behaviour.
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// Lifted out of ControllerEditor into its own file, following the convention already used for
// PhysBoneEditor.
//
// Audit status: PARTIAL -- every entry above was re-checked against reverse-engineering/export/ (the struct still
// sits at lines 3165-3188 of the post-561e9ec snapshot); the bodies were not re-diffed, which is why
// this is PARTIAL rather than VERIFIED.

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
