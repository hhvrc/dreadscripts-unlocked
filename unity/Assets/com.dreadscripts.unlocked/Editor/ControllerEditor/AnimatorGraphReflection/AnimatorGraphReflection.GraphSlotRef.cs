// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested readonly struct GraphSlotRef -> GraphSlotRef, line 780
//     slot        -> slot,        line 782
//     Node()      -> Node,        line 787
//     Edges()     -> Edges,       line 793
//     constructor -> unchanged,   line 798
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Deliberately not ported: the static field TestStruct and the method IncludeStruct (lines 784 and
// 803), which are obfuscator scaffolding — a never-assigned object compared against null by a
// method nothing calls. They carry no behaviour.
// Audit status: VERIFIED against export member-by-member (2026-08-04). Dead scaffolding correctly dropped.

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
