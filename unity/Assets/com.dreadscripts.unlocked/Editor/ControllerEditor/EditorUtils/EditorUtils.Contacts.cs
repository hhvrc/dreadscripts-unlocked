// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static FillList    -> ConvertToSender(VRCContactReceiver, GameObject),      line 7536
//   static WriteList   -> ConvertToSender(VRCPhysBoneCollider, GameObject),     line 7549
//   static ForgotList  -> ConvertToReceiver(VRCContactSender, GameObject),      line 7561
//   static StopList    -> ConvertToReceiver(VRCPhysBoneCollider, GameObject),   line 7574
//   static CheckList   -> ConvertToCollider(VRCContactReceiver, GameObject),    line 7586
//   static PrepareList -> ConvertToCollider(VRCContactSender, GameObject),      line 7598
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// VRChat's contact senders, contact receivers and PhysBone colliders are three unrelated component
// types that happen to describe the same thing: a shape (sphere, capsule or plane) placed relative
// to a transform. Converting between them is therefore mostly copying that shape across, which is
// what PhysBoneColliderSnapshot does; these six add only what the shape does not cover.
//
// Two details are common to all six and easy to get wrong:
//   * Undo.AddComponent, not AddComponent, so a conversion is one undo step with whatever the
//     caller is doing around it.
//   * A rootTransform equal to the new component's own transform is cleared to null. VRChat treats
//     null as "this object", and leaving the explicit self-reference in place makes the component
//     break if it is later moved or duplicated.
//
// Collision tags only exist on the two contact types, so they are copied on the four
// contact-to-contact edges and simply absent on the two involving a collider -- a PhysBone collider
// has nothing to copy them from or to.

using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Adds a contact sender to <paramref name="target"/> matching
        /// <paramref name="receiver"/>'s shape, placement and collision tags.
        /// </summary>
        internal static VRCContactSender ConvertToSender(this VRCContactReceiver receiver, GameObject target)
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
        /// Adds a contact sender to <paramref name="target"/> matching
        /// <paramref name="collider"/>'s shape and placement. The sender is left with no collision
        /// tags -- a PhysBone collider has none to give.
        /// </summary>
        internal static VRCContactSender ConvertToSender(this VRCPhysBoneCollider collider, GameObject target)
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
        /// Adds a contact receiver to <paramref name="target"/> matching
        /// <paramref name="sender"/>'s shape, placement and collision tags.
        /// </summary>
        internal static VRCContactReceiver ConvertToReceiver(this VRCContactSender sender, GameObject target)
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
        /// Adds a contact receiver to <paramref name="target"/> matching
        /// <paramref name="collider"/>'s shape and placement, with no collision tags.
        /// </summary>
        internal static VRCContactReceiver ConvertToReceiver(this VRCPhysBoneCollider collider, GameObject target)
        {
            VRCContactReceiver receiver = Undo.AddComponent<VRCContactReceiver>(target);
            new PhysBoneColliderSnapshot(collider).ApplyTo(receiver);
            receiver.rootTransform = collider.rootTransform;
            if (receiver.rootTransform == receiver.transform)
            {
                receiver.rootTransform = null;
            }

            return receiver;
        }

        /// <summary>
        /// Adds a PhysBone collider to <paramref name="target"/> matching
        /// <paramref name="receiver"/>'s shape and placement. Collision tags are dropped.
        /// </summary>
        internal static VRCPhysBoneCollider ConvertToCollider(this VRCContactReceiver receiver, GameObject target)
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
        /// Adds a PhysBone collider to <paramref name="target"/> matching
        /// <paramref name="sender"/>'s shape and placement. Collision tags are dropped.
        /// </summary>
        internal static VRCPhysBoneCollider ConvertToCollider(this VRCContactSender sender, GameObject target)
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
    }
}
