// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/PhysBoneColliderSnapshot.cs
//   Apply()                        -> Restore,                     line 205
//   Apply(ContactBase)             -> ApplyTo(ContactBase),        line 226
//   Apply(VRCPhysBoneCollider)     -> ApplyTo(VRCPhysBoneCollider), line 235
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference. The three same-named methods are split into Restore (write back onto
// the source) and ApplyTo (write onto another component) so the call sites read unambiguously.
//
// Audit status: VERIFIED -- diffed in full against export/. The eight fields, both constructors
// and all three Apply overloads match statement for statement. Restore's contact branch delegates
// to ApplyTo(ContactBase) instead of repeating the five assignments the decompile inlines there;
// the two are identical, including the shapeType asymmetry documented on Restore. The
// unreferenced static pair SetupDecorator/ExcludeDecorator is not ported, as an obfuscator decoy.

using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The shape of a PhysBone collider or an avatar contact, captured so it can be put back or
    /// copied onto another component.
    /// </summary>
    /// <remarks>
    /// PhysBone colliders and contacts have the same shape fields but no common base that exposes
    /// them, and their <c>ShapeType</c> enums are two distinct types. The snapshot keeps the shape as
    /// an <see cref="int"/> and casts on the way out, which is what lets one type serve both.
    /// </remarks>
    internal struct PhysBoneColliderSnapshot
    {
        /// <summary>The component this was captured from.</summary>
        internal readonly Object source;

        /// <summary>
        /// True when <see cref="source"/> is a <see cref="VRCPhysBoneColliderBase"/>, false when it
        /// is a <see cref="ContactBase"/>. Decides which enum the shape is cast back to.
        /// </summary>
        internal bool isPhysBoneCollider;

        internal readonly Transform rootTransform;

        /// <summary>Held untyped; see the note on the class.</summary>
        internal readonly int shapeType;

        internal float radius;

        internal float height;

        internal Vector3 position;

        internal Quaternion rotation;

        internal PhysBoneColliderSnapshot(VRCPhysBoneColliderBase collider)
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

        internal PhysBoneColliderSnapshot(ContactBase contact)
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

        /// <summary>Writes the snapshot back onto the component it came from.</summary>
        internal void Restore()
        {
            if (isPhysBoneCollider)
            {
                VRCPhysBoneColliderBase collider = (VRCPhysBoneColliderBase)source;
                collider.radius = radius;
                collider.height = height;
                collider.position = position;
                collider.rotation = rotation;

                // shapeType is deliberately NOT restored here. Both shipped builds omit it on this
                // branch (ControllerEditor's PhysBoneColliderSnapshot.Apply, and ADOverhaul's
                // ADOEditorUtility.ShapeSnapshot.Apply), while restoring it on the contact branch
                // and in both ApplyTo overloads. A shape change made between capture and restore
                // therefore survives the rollback on PhysBone colliders. The asymmetry looks like an
                // oversight in the original, but callers may depend on it, so it is ported as
                // shipped rather than "fixed".
            }
            else
            {
                ApplyTo((ContactBase)source);
            }
        }

        /// <summary>Copies the captured shape onto another contact.</summary>
        internal void ApplyTo(ContactBase contact)
        {
            contact.radius = radius;
            contact.height = height;
            contact.position = position;
            contact.rotation = rotation;
            contact.shapeType = (ContactBase.ShapeType)shapeType;
        }

        /// <summary>Copies the captured shape onto another PhysBone collider.</summary>
        internal void ApplyTo(VRCPhysBoneCollider collider)
        {
            collider.radius = radius;
            collider.height = height;
            collider.position = position;
            collider.rotation = rotation;
            collider.shapeType = (VRCPhysBoneColliderBase.ShapeType)shapeType;
        }
    }
}
