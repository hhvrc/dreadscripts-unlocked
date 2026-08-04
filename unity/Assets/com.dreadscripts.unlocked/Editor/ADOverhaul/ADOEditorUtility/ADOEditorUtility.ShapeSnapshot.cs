// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested struct ShapeSnapshot -> ShapeSnapshot, lines 1264-1351
//   ValidateDescriptor / EnableDescriptor -> not ported; see below
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every field and every statement below was transcribed
// from the region above.
//
// Every member of this struct kept its original name through obfuscation except the tamper pair:
// source, isPhysBoneCollider, rootTransform, shapeType, radius, height, position, rotation and the
// three Apply overloads all read as English and none of them rhymes with the Serializer/Descriptor
// families the protector used elsewhere in this class.
//
// Deliberately not ported: the private `ValidateDescriptor` object field and the
// `EnableDescriptor()` method whose whole body is `ValidateDescriptor == null`. Nothing assigns the
// field and nothing calls the method, so the predicate is a constant true -- the same protector
// tamper-bait already dropped from PhysBoneParameter (CallDescriptor/QueryDescriptor) and
// SphereHandle.
//
// ControllerEditor ships no equivalent: this is specific to the two VRChat component families
// ADOverhaul converts between, and is used only by the six conversion helpers in
// ADOEditorUtility.VRChatComponents.cs.

using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The shape of a PhysBone collider or a contact -- the fields the two component families
        /// have in common -- captured off one component so it can be written back to it or copied
        /// onto another.
        /// </summary>
        /// <remarks>
        /// <para>
        /// VRChat's collider and contact components describe the same primitives with the same
        /// fields, but through two unrelated base classes with no shared interface. This struct is
        /// the bridge: it reads either one, and writes either one.
        /// </para>
        /// <para>
        /// <see cref="shapeType"/> is held as an int rather than as either enum for the same reason.
        /// The two enums are distinct types with matching members, so the value round-trips through
        /// an int and is cast back to whichever one the destination wants.
        /// </para>
        /// </remarks>
        internal struct ShapeSnapshot
        {
            /// <summary>The component this was read from, for the no-argument <see cref="Apply()"/>.</summary>
            internal readonly UnityEngine.Object source;

            /// <summary>Which of the two families <see cref="source"/> belongs to.</summary>
            internal bool isPhysBoneCollider;

            /// <summary>
            /// The transform the shape is positioned relative to. Captured but never written back by
            /// any of the <c>Apply</c> overloads -- the conversion helpers set it themselves,
            /// because they also have to null it out when it resolves to the destination's own
            /// transform.
            /// </summary>
            internal readonly Transform rootTransform;

            internal readonly int shapeType;

            internal float radius;

            internal float height;

            internal Vector3 position;

            internal Quaternion rotation;

            internal ShapeSnapshot(VRCPhysBoneColliderBase collider)
            {
                source = collider;
                isPhysBoneCollider = true;
                rootTransform = collider.GetRootTransform();
                shapeType = (int)collider.shapeType;
                radius = collider.radius;
                height = collider.height;
                position = collider.position;
                rotation = collider.rotation;
            }

            internal ShapeSnapshot(ContactBase contact)
            {
                source = contact;
                isPhysBoneCollider = false;
                rootTransform = contact.GetRootTransform();
                shapeType = (int)contact.shapeType;
                radius = contact.radius;
                height = contact.height;
                position = contact.position;
                rotation = contact.rotation;
            }

            /// <summary>Writes the snapshot back to the component it was taken from.</summary>
            /// <remarks>
            /// The collider branch deliberately does not restore <c>shapeType</c>, while the contact
            /// branch does. Both typed overloads below restore it. That asymmetry is in the shipped
            /// build and is preserved.
            /// </remarks>
            internal void Apply()
            {
                if (isPhysBoneCollider)
                {
                    VRCPhysBoneColliderBase collider = (VRCPhysBoneColliderBase)source;
                    collider.radius = radius;
                    collider.height = height;
                    collider.position = position;
                    collider.rotation = rotation;
                }
                else
                {
                    ContactBase contact = (ContactBase)source;
                    contact.radius = radius;
                    contact.height = height;
                    contact.position = position;
                    contact.rotation = rotation;
                    contact.shapeType = (ContactBase.ShapeType)shapeType;
                }
            }

            /// <summary>Copies the snapshot onto a contact component.</summary>
            internal void Apply(ContactBase contact)
            {
                contact.radius = radius;
                contact.height = height;
                contact.position = position;
                contact.rotation = rotation;
                contact.shapeType = (ContactBase.ShapeType)shapeType;
            }

            /// <summary>Copies the snapshot onto a PhysBone collider.</summary>
            internal void Apply(VRCPhysBoneCollider collider)
            {
                collider.radius = radius;
                collider.height = height;
                collider.position = position;
                collider.rotation = rotation;
                collider.shapeType = (VRCPhysBoneColliderBase.ShapeType)shapeType;
            }
        }
    }
}
