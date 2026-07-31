// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/GameObjectRef.cs

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
