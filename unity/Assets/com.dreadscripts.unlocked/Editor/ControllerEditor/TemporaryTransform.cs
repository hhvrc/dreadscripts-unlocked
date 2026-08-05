// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/TemporaryTransform.cs
//
// Audit status: VERIFIED -- diffed in full against export/. Both constructors, Destroy and the
// implicit Transform conversion match statement for statement; the four HideFlags are the same
// set, lifted to the TemporaryFlags constant, and `if (gameObject)` is the same Unity null test
// the decompile spells `if ((bool)gameObject)`.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A throwaway <see cref="Transform"/> at a given position, used as a reference frame for
    /// mirroring and space conversions without disturbing anything in the scene.
    /// </summary>
    /// <remarks>
    /// The backing GameObject is hidden and marked not to be saved, so it never shows up in the
    /// hierarchy and cannot be left behind in a scene or build if <see cref="Destroy"/> is missed.
    /// </remarks>
    internal class TemporaryTransform
    {
        private const HideFlags TemporaryFlags =
            HideFlags.HideInHierarchy | HideFlags.HideInInspector |
            HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

        private readonly GameObject gameObject;

        private readonly Transform transform;

        /// <summary>Creates a copy of <paramref name="source"/>'s placement and parent.</summary>
        internal TemporaryTransform(Transform source)
            : this(source.position, source.rotation, source.localScale, source.parent)
        {
        }

        /// <summary>Any component left null falls back to identity.</summary>
        internal TemporaryTransform(Vector3? position, Quaternion? rotation, Vector3? localScale, Transform parent)
        {
            gameObject = new GameObject("Mirror Transform") { hideFlags = TemporaryFlags };

            transform = gameObject.transform;
            transform.parent = parent;
            transform.position = position ?? Vector3.zero;
            transform.rotation = rotation ?? Quaternion.identity;
            transform.localScale = localScale ?? Vector3.one;
        }

        internal void Destroy()
        {
            if (gameObject)
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        public static implicit operator Transform(TemporaryTransform temporary)
        {
            return temporary.transform;
        }
    }
}
