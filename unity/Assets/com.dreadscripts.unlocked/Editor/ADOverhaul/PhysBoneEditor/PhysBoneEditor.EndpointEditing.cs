// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: SearchSingleton (line 4505), LoginSingleton (line 4546) and PatchSingleton
// (line 4558) of the current snapshot, together with the compiler-generated capture struct
// _003C_003Ec__DisplayClass116_0 (line 2834) that carried their arguments. Line numbers move with
// the snapshot; the member names below are the durable reference.
//
//   SearchSingleton -> ApplyEndpointOffset(Vector3, VRCPhysBone[], VRCPhysBone),        line 4505
//   LoginSingleton  -> OffsetEndpoint(VRCPhysBone, Vector3),                            line 4546
//   PatchSingleton  -> EditProperty(VRCPhysBone, string, Action<SerializedProperty>),   line 4558
//
// The capture struct held exactly the two values the original method needed from its caller —
// _MessageAuthentication (the selection) and _PolicyAuthentication (the PhysBone under the handle) —
// so it is dissolved into ordinary parameters and the `ref` disappears with it.
//
// NOT PORTED from this group: CustomizeSingleton (line 3636), the caller that draws the position
// and slider handles and computes the offset passed in here. It needs the unported
// ADOEditorUtility.BoneChainTree and BoneNode types.
//
// 2019 vs 2022: identical.

using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// Applies an endpoint drag to the selection, honouring the modifier keys that decide how far
        /// the edit spreads.
        /// </summary>
        /// <param name="offset">
        /// The drag, expressed in the local space of the bone whose endpoint handle was moved. It is
        /// added to each PhysBone's <c>endpointPosition</c> as-is, which is only correct while the
        /// bones share an orientation — the same assumption the handle itself makes.
        /// </param>
        /// <param name="physBones">Every PhysBone being inspected.</param>
        /// <param name="draggedPhysBone">The one whose handle the user grabbed.</param>
        /// <remarks>
        /// With a single PhysBone selected the modifiers are irrelevant and the drag simply applies.
        /// With several: plain drag offsets every one of them by the same delta, Alt restricts the
        /// edit to the dragged PhysBone, and Shift offsets the dragged one and then assigns its
        /// resulting absolute endpoint to all the others, levelling them up rather than nudging them.
        /// <para>
        /// Written as a chain rather than a switch because the single-target case has to win over the
        /// modifiers; ported in the original's order.
        /// </para>
        /// </remarks>
        internal static void ApplyEndpointOffset(Vector3 offset, VRCPhysBone[] physBones, VRCPhysBone draggedPhysBone)
        {
            Event current = Event.current;
            bool alt = current.alt;

            if (physBones.Length == 1)
            {
                OffsetEndpoint(draggedPhysBone, offset);
            }
            else if (!alt)
            {
                if (current.shift)
                {
                    Vector3 endpoint = OffsetEndpoint(draggedPhysBone, offset);
                    foreach (VRCPhysBone physBone in physBones)
                    {
                        if (physBone != draggedPhysBone)
                        {
                            EditProperty(physBone, endpointPosition.propertyPath, delegate(SerializedProperty property)
                            {
                                property.vector3Value = endpoint;
                            });
                        }
                    }
                }
                else
                {
                    foreach (VRCPhysBone physBone in physBones)
                    {
                        OffsetEndpoint(physBone, offset);
                    }
                }
            }
            else
            {
                OffsetEndpoint(draggedPhysBone, offset);
            }
        }

        /// <summary>
        /// Adds <paramref name="offset"/> to one PhysBone's <c>endpointPosition</c> and returns the
        /// value it ended up at.
        /// </summary>
        private static Vector3 OffsetEndpoint(VRCPhysBone physBone, Vector3 offset)
        {
            Vector3 result = Vector3.zero;

            EditProperty(physBone, endpointPosition.propertyPath, delegate(SerializedProperty property)
            {
                property.vector3Value += offset;
                result = property.vector3Value;
            });

            return result;
        }

        /// <summary>
        /// Opens a <see cref="SerializedObject"/> over <paramref name="physBone"/>, hands
        /// <paramref name="edit"/> the property at <paramref name="propertyPath"/>, and applies the
        /// result.
        /// </summary>
        /// <remarks>
        /// The scene tools edit PhysBones that are not the inspected one, and so cannot reuse the
        /// editor's own <see cref="Editor.serializedObject"/>. Going through a throwaway
        /// SerializedObject per edit rather than assigning the component's fields directly is what
        /// records the change with the undo system and marks the scene dirty.
        /// </remarks>
        private static void EditProperty(VRCPhysBone physBone, string propertyPath, Action<SerializedProperty> edit)
        {
            SerializedObject serializedPhysBone = new SerializedObject(physBone);
            serializedPhysBone.UpdateIfRequiredOrScript();
            edit(serializedPhysBone.FindProperty(propertyPath));
            serializedPhysBone.ApplyModifiedProperties();
        }
    }
}
