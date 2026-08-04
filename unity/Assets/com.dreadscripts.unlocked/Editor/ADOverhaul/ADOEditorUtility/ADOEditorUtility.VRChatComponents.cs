// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CompareVal   -> ToContactSender(VRCContactReceiver, GameObject),      line 3928
//   static VerifyVal    -> ToContactSender(VRCPhysBoneCollider, GameObject),     line 3941
//   static SetVal       -> ToContactReceiver(VRCContactSender, GameObject),      line 3953
//   static SortVal      -> ToContactReceiver(VRCPhysBoneCollider, GameObject),   line 3966
//   static InvokeVal    -> ToPhysBoneCollider(VRCContactReceiver, GameObject),   line 3981
//   static CustomizeVal -> ToPhysBoneCollider(VRCContactSender, GameObject),     line 3993
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// DEOBF-BUG(resolved): ToContactReceiver(VRCPhysBoneCollider, ...) rendered its root-transform
// reset as
//     if (receiver.rootTransform == receiver.transform) { while (true) { receiver.rootTransform = null; } }
// -- an unconditional hang. The other five members of this family, which are otherwise identical
// line for line, all render the same statement as a plain assignment, and so does the 2019 build's
// copy of this one (UpdateParam, line 4075). It is a decompiler artefact; the assignment runs once.
//
// The 2019 build also confirms the ShapeSnapshot member name: it calls the typed overload `ApplyTo`
// rather than `Apply`, which is a protector rename in one build or the other. `Apply` is kept, as
// the 2022 build has it and as ShapeSnapshot's own no-argument overload is named in both.
//
// All six take the same shape: add the destination component with Undo so the conversion is
// undoable, copy the geometry through ShapeSnapshot, then carry across whatever else the source and
// destination have in common. The collision-tag list only exists on the two contact types, so it is
// copied on the two contact-to-contact conversions and on neither of the four that involve a
// collider. Clearing rootTransform when it resolves to the new component's own transform is what
// keeps the destination on VRChat's default rather than pinning it to a redundant explicit
// reference.

using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>Adds a contact sender to <paramref name="target"/> matching <paramref name="source"/>.</summary>
        internal static VRCContactSender ToContactSender(this VRCContactReceiver source, GameObject target)
        {
            VRCContactSender sender = Undo.AddComponent<VRCContactSender>(target);
            new ShapeSnapshot(source).Apply(sender);
            sender.collisionTags = source.collisionTags;
            sender.rootTransform = source.rootTransform;

            if (sender.rootTransform == sender.transform)
            {
                sender.rootTransform = null;
            }

            return sender;
        }

        /// <inheritdoc cref="ToContactSender(VRCContactReceiver, GameObject)"/>
        internal static VRCContactSender ToContactSender(this VRCPhysBoneCollider source, GameObject target)
        {
            VRCContactSender sender = Undo.AddComponent<VRCContactSender>(target);
            new ShapeSnapshot(source).Apply(sender);
            sender.rootTransform = source.rootTransform;

            if (sender.rootTransform == sender.transform)
            {
                sender.rootTransform = null;
            }

            return sender;
        }

        /// <summary>Adds a contact receiver to <paramref name="target"/> matching <paramref name="source"/>.</summary>
        internal static VRCContactReceiver ToContactReceiver(this VRCContactSender source, GameObject target)
        {
            VRCContactReceiver receiver = Undo.AddComponent<VRCContactReceiver>(target);
            new ShapeSnapshot(source).Apply(receiver);
            receiver.collisionTags = source.collisionTags;
            receiver.rootTransform = source.rootTransform;

            if (receiver.rootTransform == receiver.transform)
            {
                receiver.rootTransform = null;
            }

            return receiver;
        }

        /// <inheritdoc cref="ToContactReceiver(VRCContactSender, GameObject)"/>
        internal static VRCContactReceiver ToContactReceiver(this VRCPhysBoneCollider source, GameObject target)
        {
            VRCContactReceiver receiver = Undo.AddComponent<VRCContactReceiver>(target);
            new ShapeSnapshot(source).Apply(receiver);
            receiver.rootTransform = source.rootTransform;

            if (receiver.rootTransform == receiver.transform)
            {
                receiver.rootTransform = null;
            }

            return receiver;
        }

        /// <summary>Adds a PhysBone collider to <paramref name="target"/> matching <paramref name="source"/>.</summary>
        internal static VRCPhysBoneCollider ToPhysBoneCollider(this VRCContactReceiver source, GameObject target)
        {
            VRCPhysBoneCollider collider = Undo.AddComponent<VRCPhysBoneCollider>(target);
            new ShapeSnapshot(source).Apply(collider);
            collider.rootTransform = source.rootTransform;

            if (collider.rootTransform == collider.transform)
            {
                collider.rootTransform = null;
            }

            return collider;
        }

        /// <inheritdoc cref="ToPhysBoneCollider(VRCContactReceiver, GameObject)"/>
        internal static VRCPhysBoneCollider ToPhysBoneCollider(this VRCContactSender source, GameObject target)
        {
            VRCPhysBoneCollider collider = Undo.AddComponent<VRCPhysBoneCollider>(target);
            new ShapeSnapshot(source).Apply(collider);
            collider.rootTransform = source.rootTransform;

            if (collider.rootTransform == collider.transform)
            {
                collider.rootTransform = null;
            }

            return collider;
        }
    }
}
