// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CompareVal   -> ConvertToContactSender(this VRCContactReceiver, GameObject),    line 3928
//   static VerifyVal    -> ConvertToContactSender(this VRCPhysBoneCollider, GameObject),   line 3941
//   static SetVal       -> ConvertToContactReceiver(this VRCContactSender, GameObject),    line 3953
//   static SortVal      -> ConvertToContactReceiver(this VRCPhysBoneCollider, GameObject), line 3966
//   static InvokeVal    -> ConvertToPhysBoneCollider(this VRCContactReceiver, GameObject), line 3981
//   static CustomizeVal -> ConvertToPhysBoneCollider(this VRCContactSender, GameObject),   line 3993
//   static ConcatVal    -> GetPlayableLayerOptions,                                        line 4005
//   static FillVal      -> ToggleTriState,                                                 line 4042
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// The six conversions are the bodies behind the CONTEXT/VRC*/ADOverhaul/"To Sender", "To Receiver"
// and "To Collider" menu items. They are grouped here because they are one family: every one of
// them snapshots the source component's shape, adds the destination component through Undo, copies
// the shape and root transform across, and normalises a self-referencing root transform to null.
// The three destination types give three method names, overloaded on the source type.
//
// Not ported from this region, because the package already has an equivalent:
//   static MapVal        line 4034 — playable-layer controller lookup. Character-for-character the
//                                   same query as EditorUtils.TryGetPlayableLayerController
//                                   (Editor/ControllerEditor/EditorUtils/EditorUtils.AvatarDescriptor.cs,
//                                   decompiled ControllerEditor EditorUtils.UpdateList, line 7639).
//                                   The ADOverhaul assembly shipped its own copy; the package keeps
//                                   one, and GetPlayableLayerOptions below calls it.
//   static IncludeProcess<T> line 2487 — identical to EditorUtils.HandleDragAndDrop
//                                   (EditorUtils.DragAndDrop.cs / decompiled InstantiateRules 4817).
//   static RevertProcess<T>  line 2512 — the multi-object drop handler, ported as
//                                   EditorUtils.HandleMultiDragAndDrop. NOT byte-identical: see the
//                                   divergence note below before wiring an ADOverhaul call site to
//                                   it.
//
// RevertProcess divergence. The ControllerEditor original (AwakeRules, line 4842) applies the
// caller's filter on both branches. The ADOverhaul copy applies it only when T is a Component
// subclass; for a plain asset type it is `DragAndDrop.objectReferences.OfType<T>().ToArray()`, with
// the filter dropped on the floor. Both the 2019 and the 2022 ADOverhaul builds have it, so it is
// the shipped behaviour of this assembly rather than a decompiler slip -- but it is a bug, not a
// design, and the package's HandleMultiDragAndDrop has the ControllerEditor (correct) form. Any
// ADOverhaul caller that passes a filter for a non-Component T will therefore behave differently
// than it did when shipped, and needs a decision at that call site rather than here.
//
// Obfuscator scaffolding omitted throughout: each of the surrounding compiler-generated display
// classes carried an always-null `object` static paired with a `== null` predicate
// (ShapeSnapshot.ValidateDescriptor/EnableDescriptor and friends). Those are licence-gate residue
// with no callers and are not ported.
//
// ILSpy artifact. SortVal's null-out of a self-referencing rootTransform decompiles as
// `while (true) { receiver.rootTransform = null; }`, an infinite loop that would hang the editor.
// The 2019 build's counterpart (UpdateParam, line 4075) decompiles as the plain assignment the
// other five methods also have, which settles it: the loop is a decompiler artifact and the
// assignment is ported.
//
// 2019 vs 2022: no behavioural divergence anywhere in this region. The same eight members appear
// under different obfuscated names (WriteParam 4037, DefineParam 4050, PushParam 4062, UpdateParam
// 4075, InsertParam 4087, PrepareParam 4099, ListParam 4110, ReadParam 4148), calling
// ShapeSnapshot.ApplyTo where 2022 calls the renamed ShapeSnapshot.Apply.
//
// ShapeSnapshot itself (decompiled ADOEditorUtility.cs 1264) is the ADOverhaul assembly's copy of
// ControllerEditor's PhysBoneColliderSnapshot, already in the package; it is used here rather than
// duplicated. The two differ only in that the ADOverhaul copy's parameterless Apply() also omits
// the shapeType restore on the PhysBone path -- the same shipped bug already documented on
// PhysBoneColliderSnapshot.Restore. Nothing in this file calls that overload.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using DreadScripts.ControllerEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Adds a <see cref="VRCContactSender"/> to <paramref name="target"/> carrying the receiver's
        /// shape, collision tags and root transform.
        /// </summary>
        /// <param name="target">
        /// The GameObject the new sender is added to. It does not have to be the receiver's own
        /// GameObject -- the menu items that call this pass a freshly created sibling.
        /// </param>
        /// <returns>The new sender. The source receiver is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// The component is added through <see cref="Undo.AddComponent{T}"/>, so a single Ctrl+Z
        /// removes it. Note that only the addition is undoable: the fields written afterwards are
        /// assigned directly, which is harmless here because they are written onto a component that
        /// the same undo step deletes.
        ///
        /// A root transform equal to the sender's own transform is stored as null. The two mean the
        /// same thing to the SDK, but an explicit self-reference survives reparenting where null does
        /// not, so collapsing it keeps the converted component behaving like a hand-authored one.
        /// Because the comparison is against the *destination's* transform, converting onto a
        /// different GameObject deliberately keeps a root transform that pointed at the source.
        /// </remarks>
        internal static VRCContactSender ConvertToContactSender(this VRCContactReceiver receiver, GameObject target)
        {
            VRCContactSender sender = Undo.AddComponent<VRCContactSender>(target);
            new PhysBoneColliderSnapshot(receiver).ApplyTo(sender);
            sender.collisionTags = receiver.collisionTags;
            sender.rootTransform = receiver.rootTransform;

            if (sender.rootTransform == sender.transform)
            {
                sender.rootTransform = null;
            }

            return sender;
        }

        /// <summary>
        /// Adds a <see cref="VRCContactSender"/> to <paramref name="target"/> carrying the collider's
        /// shape and root transform.
        /// </summary>
        /// <returns>The new sender. The source collider is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// No collision tags are copied, because a PhysBone collider has none -- the new sender is
        /// created with an empty tag list and the user has to fill it in. See
        /// <see cref="ConvertToContactSender(VRCContactReceiver, GameObject)"/> for the undo and
        /// root-transform behaviour, which is the same.
        /// </remarks>
        internal static VRCContactSender ConvertToContactSender(this VRCPhysBoneCollider collider, GameObject target)
        {
            VRCContactSender sender = Undo.AddComponent<VRCContactSender>(target);
            new PhysBoneColliderSnapshot(collider).ApplyTo(sender);
            sender.rootTransform = collider.rootTransform;

            if (sender.rootTransform == sender.transform)
            {
                sender.rootTransform = null;
            }

            return sender;
        }

        /// <summary>
        /// Adds a <see cref="VRCContactReceiver"/> to <paramref name="target"/> carrying the sender's
        /// shape, collision tags and root transform.
        /// </summary>
        /// <returns>The new receiver. The source sender is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// Only the fields senders and receivers have in common are copied. The receiver's own
        /// settings -- receiver type, parameter name, local-only, min/max velocity -- keep their
        /// defaults, since a sender has nothing to supply for them. See
        /// <see cref="ConvertToContactSender(VRCContactReceiver, GameObject)"/> for the undo and
        /// root-transform behaviour.
        /// </remarks>
        internal static VRCContactReceiver ConvertToContactReceiver(this VRCContactSender sender, GameObject target)
        {
            VRCContactReceiver receiver = Undo.AddComponent<VRCContactReceiver>(target);
            new PhysBoneColliderSnapshot(sender).ApplyTo(receiver);
            receiver.collisionTags = sender.collisionTags;
            receiver.rootTransform = sender.rootTransform;

            if (receiver.rootTransform == receiver.transform)
            {
                receiver.rootTransform = null;
            }

            return receiver;
        }

        /// <summary>
        /// Adds a <see cref="VRCContactReceiver"/> to <paramref name="target"/> carrying the
        /// collider's shape and root transform.
        /// </summary>
        /// <returns>The new receiver. The source collider is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// As with the sender conversion, no collision tags are copied because a PhysBone collider
        /// has none. See <see cref="ConvertToContactSender(VRCContactReceiver, GameObject)"/> for the
        /// undo and root-transform behaviour.
        /// </remarks>
        internal static VRCContactReceiver ConvertToContactReceiver(this VRCPhysBoneCollider collider, GameObject target)
        {
            VRCContactReceiver receiver = Undo.AddComponent<VRCContactReceiver>(target);
            new PhysBoneColliderSnapshot(collider).ApplyTo(receiver);
            receiver.rootTransform = collider.rootTransform;

            if (receiver.rootTransform == receiver.transform)
            {
                // See the header: the decompiler renders this assignment as an infinite loop. The
                // 2019 build decompiles it as the plain assignment ported here.
                receiver.rootTransform = null;
            }

            return receiver;
        }

        /// <summary>
        /// Adds a <see cref="VRCPhysBoneCollider"/> to <paramref name="target"/> carrying the
        /// receiver's shape and root transform.
        /// </summary>
        /// <returns>The new collider. The source receiver is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// The receiver's collision tags are dropped: a collider has no tags to put them in. The
        /// collider's own settings -- inside bounds, the affected-transforms list -- keep their
        /// defaults. See <see cref="ConvertToContactSender(VRCContactReceiver, GameObject)"/> for the
        /// undo and root-transform behaviour.
        /// </remarks>
        internal static VRCPhysBoneCollider ConvertToPhysBoneCollider(this VRCContactReceiver receiver, GameObject target)
        {
            VRCPhysBoneCollider collider = Undo.AddComponent<VRCPhysBoneCollider>(target);
            new PhysBoneColliderSnapshot(receiver).ApplyTo(collider);
            collider.rootTransform = receiver.rootTransform;

            if (collider.rootTransform == collider.transform)
            {
                collider.rootTransform = null;
            }

            return collider;
        }

        /// <summary>
        /// Adds a <see cref="VRCPhysBoneCollider"/> to <paramref name="target"/> carrying the
        /// sender's shape and root transform.
        /// </summary>
        /// <returns>The new collider. The source sender is left in place; nothing is destroyed.</returns>
        /// <remarks>
        /// The sender's collision tags are dropped, as in
        /// <see cref="ConvertToPhysBoneCollider(VRCContactReceiver, GameObject)"/>. See
        /// <see cref="ConvertToContactSender(VRCContactReceiver, GameObject)"/> for the undo and
        /// root-transform behaviour.
        /// </remarks>
        internal static VRCPhysBoneCollider ConvertToPhysBoneCollider(this VRCContactSender sender, GameObject target)
        {
            VRCPhysBoneCollider collider = Undo.AddComponent<VRCPhysBoneCollider>(target);
            new PhysBoneColliderSnapshot(sender).ApplyTo(collider);
            collider.rootTransform = sender.rootTransform;

            if (collider.rootTransform == collider.transform)
            {
                collider.rootTransform = null;
            }

            return collider;
        }

        /// <summary>The playable layers in the order the avatar descriptor's inspector shows them.</summary>
        private static readonly string[] playableLayerNames =
        {
            "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose"
        };

        /// <summary>
        /// Fills two parallel arrays describing the playable layers the avatar actually has a
        /// controller on: display names in <paramref name="names"/> and the matching
        /// <see cref="VRCAvatarDescriptor.AnimLayerType"/> values, as ints, in
        /// <paramref name="layerTypes"/>.
        /// </summary>
        /// <remarks>
        /// Shaped for <see cref="EditorGUI.IntPopup(Rect, int, string[], int[], GUIStyle)"/>, which
        /// wants exactly this pair. Layers with no controller are left out rather than shown greyed,
        /// so the popup only ever offers something the tool can open.
        ///
        /// The name index is not the enum value: <c>AnimLayerType</c> has a deprecated member
        /// occupying 1, so everything from Additive on is one higher than its position in
        /// <see cref="playableLayerNames"/>. That is what the index shift encodes -- the names array
        /// is written in inspector order and the enum value is derived from it, not the reverse.
        ///
        /// A null or destroyed descriptor is not an error; both arrays are simply emptied, which is
        /// what the popup needs when the user has not picked an avatar yet. The check is Unity's
        /// overloaded truth test, so a destroyed descriptor takes the same path as a null one.
        /// </remarks>
        internal static void GetPlayableLayerOptions(VRCAvatarDescriptor avatar, ref string[] names, ref int[] layerTypes)
        {
            if (!avatar)
            {
                names = Array.Empty<string>();
                layerTypes = Array.Empty<int>();
                return;
            }

            List<(string name, int layerType)> present = new List<(string, int)>();
            for (int i = 0; i < playableLayerNames.Length; i++)
            {
                int layerType = i != 0 ? i + 1 : i;
                if (avatar.TryGetPlayableLayerController((VRCAvatarDescriptor.AnimLayerType)layerType, out AnimatorController _))
                {
                    present.Add((playableLayerNames[i], layerType));
                }
            }

            names = new string[present.Count];
            layerTypes = new int[present.Count];
            for (int i = 0; i < present.Count; i++)
            {
                names[i] = present[i].name;
                layerTypes[i] = present[i].layerType;
            }
        }

        /// <summary>
        /// Flips one entry of a tri-state checkbox column held as a byte array, where 0 is off, 1 is
        /// on and anything else means mixed.
        /// </summary>
        /// <param name="states">The column. Written in place.</param>
        /// <param name="index">The row to flip.</param>
        /// <param name="mixedBecomes">
        /// What a mixed entry resolves to. Mixed has no opposite to toggle to, so the caller decides;
        /// the default of true turns a partially-checked group fully on.
        /// </param>
        /// <returns>The entry's new value.</returns>
        /// <remarks>
        /// SHIPPED BUG, preserved. On the mixed path the returned bool and the stored byte disagree:
        /// <c>mixedBecomes</c> is returned, but the byte written is its inverse (true stores 0, false
        /// stores 1). The two agree on the off and on paths. A caller that trusts the return value
        /// and a caller that re-reads the array will therefore disagree about a row that started out
        /// mixed, until something writes it again. Ported as-is.
        /// </remarks>
        internal static bool ToggleTriState(byte[] states, int index, bool mixedBecomes = true)
        {
            switch (states[index])
            {
                case 0:
                    states[index] = 1;
                    return true;

                case 1:
                    states[index] = 0;
                    return false;

                default:
                    states[index] = mixedBecomes ? (byte)0 : (byte)1;
                    return mixedBecomes;
            }
        }
    }
}
