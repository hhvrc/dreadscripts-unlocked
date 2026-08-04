// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PushRules    -> InstantiatePrefab,          line 5159
//   static ViewRules    -> RecordPrefabChange,         line 5180
//   static CollectRules -> MapTransformsTo,            line 5188
//   static ResolveRules -> MapComponentsTo,            line 5211
//   static ListRules    -> GetOrAddComponent(GameObject, Type), line 5245
//   static VerifyRules  -> GetOrAddComponent<T>,       line 5255
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// MapTransformsTo / MapComponentsTo answer "where did this end up in the copy": given two
// hierarchies of the same shape, they translate references into the first into references into the
// second by hierarchy path. The tool needs this whenever it duplicates part of an avatar and has to
// re-point constraints, PhysBones or animation targets at the duplicate.
//
// Both take the same skipMissing flag, and it is worth being precise about it: false records a null
// entry for anything that does not resolve, so the caller can see what was lost; true leaves the key
// out entirely, so the dictionary only holds successful matches.
//
// VENDOR BUG, transcribed as shipped: MapComponentsTo indexes components2[num] after a bounds test
// that only fires when skipMissing is true. With skipMissing false and a destination object
// carrying fewer copies of the component than the source, it throws IndexOutOfRangeException. See
// the remark on the method.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Instantiates <paramref name="prefab"/> under <paramref name="parent"/>.
        /// </summary>
        /// <param name="unpack">
        /// Break the prefab link completely, so the result is plain GameObjects. On by default,
        /// because most of what the tool creates is meant to be edited rather than tracked.
        /// </param>
        /// <param name="resetTransform">Zero the local position/rotation and unit the scale.</param>
        /// <param name="temporary">
        /// Hide it from the hierarchy and keep it out of both the scene file and any build -- for
        /// scratch objects that exist only for the duration of an operation.
        /// </param>
        internal static GameObject InstantiatePrefab(GameObject prefab, Transform parent, bool unpack = true,
            bool resetTransform = true, bool temporary = false)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

            if (unpack)
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            if (resetTransform)
            {
                Transform transform = instance.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }

            if (temporary)
            {
                instance.hideFlags |= HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor
                                                                | HideFlags.DontSaveInBuild;
            }

            return instance;
        }

        /// <summary>
        /// Records a change to a prefab instance as an override, so it survives a reload. A
        /// GameObject that is not part of a prefab needs nothing and is ignored.
        /// </summary>
        /// <remarks>
        /// The record has to be made on the outermost instance root: an override on a nested
        /// instance is stored by its outer one, and recording against the inner object loses it.
        /// </remarks>
        internal static void RecordPrefabChange(GameObject gameObject)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject));
            }
        }

        /// <summary>
        /// Matches each of <paramref name="transformsToFind"/> to the transform at the same
        /// hierarchy path under <paramref name="destinationRoot"/>.
        /// </summary>
        /// <param name="skipMissing">
        /// Leave unmatched entries out of the result instead of mapping them to null. Note this
        /// covers both failure modes -- not under <paramref name="sourceRoot"/> at all, and no
        /// counterpart at that path.
        /// </param>
        internal static Dictionary<Transform, Transform> MapTransformsTo(Transform sourceRoot,
            Transform destinationRoot, bool skipMissing, params Transform[] transformsToFind)
        {
            Dictionary<Transform, Transform> map = new Dictionary<Transform, Transform>();
            foreach (Transform transform in transformsToFind)
            {
                if (!transform.IsChildOf(sourceRoot))
                {
                    if (!skipMissing)
                    {
                        map.Add(transform, null);
                    }

                    continue;
                }

                string path = AnimationUtility.CalculateTransformPath(transform, sourceRoot);
                Transform match = destinationRoot.Find(path);
                if (match != null || !skipMissing)
                {
                    map.Add(transform, match);
                }
            }

            return map;
        }

        /// <summary>
        /// <see cref="MapTransformsTo"/> for components: matches each component to the one at the
        /// same hierarchy path and the same position among that object's components of its type.
        /// </summary>
        /// <remarks>
        /// The ordinal matters because a GameObject can carry several components of one type --
        /// two constraints, several PhysBones -- and path alone cannot say which.
        /// <para>
        /// VENDOR BUG: with <paramref name="skipMissing"/> false and a destination object holding
        /// fewer components of that type than the source, the ordinal lookup runs off the end and
        /// throws. The guard the vendor wrote only skips that case when skipMissing is true, which
        /// is the opposite of what the null-recording behaviour elsewhere in this method would
        /// suggest. Left as shipped.
        /// </para>
        /// </remarks>
        internal static Dictionary<T, T> MapComponentsTo<T>(Transform sourceRoot, Transform destinationRoot,
            bool skipMissing, params T[] componentsToFind) where T : Component
        {
            Dictionary<T, T> map = new Dictionary<T, T>();
            foreach (T component in componentsToFind)
            {
                if (!component.transform.IsChildOf(sourceRoot))
                {
                    if (!skipMissing)
                    {
                        map.Add(component, null);
                    }

                    continue;
                }

                string path = AnimationUtility.CalculateTransformPath(component.transform, sourceRoot);
                Transform match = destinationRoot.Find(path);
                if (match == null)
                {
                    if (!skipMissing)
                    {
                        map.Add(component, null);
                    }

                    continue;
                }

                T[] sourceComponents = component.GetComponents<T>();
                T[] destinationComponents = match.GetComponents<T>();
                int ordinal = Array.IndexOf(sourceComponents, component);
                if (ordinal < destinationComponents.Length || !skipMissing)
                {
                    map.Add(component, destinationComponents[ordinal]);
                }
            }

            return map;
        }

        /// <summary>
        /// The GameObject's component of type <paramref name="componentType"/>, adding one if it
        /// has none.
        /// </summary>
        internal static Component GetOrAddComponent(this GameObject gameObject, Type componentType)
        {
            Component component = gameObject.GetComponent(componentType);
            return component == null ? gameObject.AddComponent(componentType) : component;
        }

        /// <summary>
        /// The GameObject's <typeparamref name="T"/>, adding one if it has none.
        /// </summary>
        internal static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component == null ? gameObject.AddComponent<T>() : component;
        }
    }
}
