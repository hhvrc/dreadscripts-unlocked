// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   LayerIndexEntry  -> LayerIndexEntry, lines 3165-3188 (names already in renames/)
//     layer          -> layer,       line 3167
//     layerIndex     -> layerIndex,  line 3169
//     NewProduct / LoginProduct() -> dropped, lines 3171 and 3184 (obfuscator sentinel: a
//                      never-written static object plus a "== null" predicate nothing calls;
//                      see RE_NOTES "Self-referential dead members")
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// A controller layer together with its index in the controller's layer array.
        /// </summary>
        /// <remarks>
        /// <see cref="AnimatorControllerLayer"/> does not know where it sits, and the category tree
        /// stores layers out of order, so the index has to travel with the layer for anything that
        /// selects, reorders or removes it. The implicit conversion lets an entry be used wherever a
        /// layer is expected.
        /// </remarks>
        private struct LayerIndexEntry
        {
            internal readonly AnimatorControllerLayer layer;

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
}
