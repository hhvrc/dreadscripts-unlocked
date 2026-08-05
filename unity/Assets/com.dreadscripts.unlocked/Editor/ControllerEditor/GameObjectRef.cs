// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/GameObjectRef.cs
//
// NOTES
// Transform, Components and Item all carry [SpecialName] in the decompiled source; the first two are
// property getters and Item is the indexer named by the type's [DefaultMember("Item")] attribute.
// All three are restored as properties/an indexer here. The Component and Transform constructors
// chain to the GameObject one rather than repeating its two assignments.
//
// NOT PORTED
// The `private static object PushDecorator` field and the `PrepareDecorator()` method that only
// tested it for null. Protector licence-check scaffolding, the same pattern recorded in
// Common/SphereHandle.cs and ADOverhaul/PhysBoneParameter.cs: nothing assigns the field, so the
// predicate is a constant `true`, and no caller reads either member.
//
// Audit status: VERIFIED -- both fields, all three constructors, Transform, Components, the
// indexer, GetComponent<T> and the implicit GameObject operator were diffed statement by statement
// against export/. The two dropped scaffolding members are recorded above.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A <see cref="GameObject"/> together with its component list, fetched once and reused.
    /// </summary>
    /// <remarks>
    /// <see cref="GameObject.GetComponents{T}()"/> allocates a fresh array on every call, which adds
    /// up when a hierarchy is walked once per repaint. Caching it here means the scan happens once
    /// per object per operation. The cache is never invalidated, so a ref must not outlive a change
    /// to the object's components.
    /// </remarks>
    internal struct GameObjectRef
    {
        internal readonly GameObject gameObject;

        internal Component[] cachedComponents;

        internal GameObjectRef(GameObject gameObject)
        {
            cachedComponents = null;
            this.gameObject = gameObject;
        }

        internal GameObjectRef(Component component)
            : this(component.gameObject)
        {
        }

        internal GameObjectRef(Transform transform)
            : this(transform.gameObject)
        {
        }

        internal Transform Transform => gameObject.transform;

        internal Component[] Components =>
            cachedComponents ?? (cachedComponents = gameObject.GetComponents<Component>());

        public Component this[int index] => Components[index];

        public T GetComponent<T>() where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        public static implicit operator GameObject(GameObjectRef reference)
        {
            return reference.gameObject;
        }
    }
}
