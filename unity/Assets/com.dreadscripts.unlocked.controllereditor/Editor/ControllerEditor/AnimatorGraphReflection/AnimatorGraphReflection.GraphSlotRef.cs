// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested readonly struct GraphSlotRef -> GraphSlotRef, line 780
//     slot        -> slot,        line 782
//     Node()      -> Node,        line 787
//     Edges()     -> Edges,       line 793
//     constructor -> GraphSlotRef(Slot), line 798
//   TestStruct    -> NOT PORTED, line 784 -- obfuscator scaffolding: a never-assigned object field,
//                    read only by IncludeStruct below. It carries no behaviour.
//   IncludeStruct -> NOT PORTED, line 803 -- obfuscator scaffolding: returns `TestStruct == null`,
//                    a constant true, and nothing calls it.
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED -- every entry above was re-checked member-by-member against
// reverse-engineering/export/ControllerEditor/.../AnimatorGraphReflection.cs on 2026-08-05; all seven line numbers
// land on the member named, and the dead scaffolding is correctly dropped.

using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// Refers to one connection point on a graph node — the stub an arrow is anchored to.
        /// </summary>
        internal readonly struct GraphSlotRef
        {
            public readonly Slot slot;

            public GraphSlotRef(Slot slot)
            {
                this.slot = slot;
            }

            /// <summary>The node this slot belongs to.</summary>
            public Node Node
            {
                get
                {
                    return slot.node;
                }
            }

            /// <summary>The arrows anchored to this slot.</summary>
            public List<GraphEdgeRef> Edges
            {
                get
                {
                    return new List<GraphEdgeRef>(slot.edges.Select(e => new GraphEdgeRef(e)));
                }
            }
        }
    }
}
