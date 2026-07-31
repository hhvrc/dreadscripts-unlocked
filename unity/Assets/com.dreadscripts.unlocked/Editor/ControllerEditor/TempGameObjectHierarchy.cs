// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/TempGameObjectHierarchy.cs

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Builds a throwaway chain of parented GameObjects from a slash-separated path.
    /// </summary>
    /// <remarks>
    /// Animation clips address their targets by hierarchy path, so recording or retargeting a clip
    /// needs the path to actually exist in the scene. This creates the minimum hierarchy that
    /// satisfies it, and <see cref="Destroy"/> takes the whole chain away again.
    /// </remarks>
    internal class TempGameObjectHierarchy
    {
        /// <summary>The chain from root to leaf; <c>gameObjects[0]</c> is the root.</summary>
        internal readonly GameObject[] gameObjects;

        /// <param name="path">Slash-separated hierarchy path, e.g. <c>"Armature/Hips/Spine"</c>.</param>
        /// <param name="underDummyRoot">
        /// Nests the chain under a "Dummy" root, keeping these objects distinguishable from the
        /// user's own and letting <see cref="Destroy"/> remove them in one call.
        /// </param>
        internal TempGameObjectHierarchy(string path, bool underDummyRoot = true)
        {
            if (underDummyRoot)
            {
                path = "Dummy/" + path;
            }

            string[] names = path.Split('/');
            gameObjects = new GameObject[names.Length];

            Transform parent = null;
            for (int i = 0; i < names.Length; i++)
            {
                GameObject gameObject = new GameObject(names[i]);
                gameObject.transform.parent = parent;
                parent = gameObject.transform;
                gameObjects[i] = gameObject;
            }
        }

        /// <summary>Destroys the root, taking every object in the chain with it.</summary>
        internal void Destroy()
        {
            Object.DestroyImmediate(gameObjects[0]);
        }
    }
}
