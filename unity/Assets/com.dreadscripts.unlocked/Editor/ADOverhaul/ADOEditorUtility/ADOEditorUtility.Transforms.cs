// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static LogoutStatus -> MapTransforms, line 2829
//   static SetupStatus  -> MapComponents, line 2851
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above.
//
// The `transformsToFind` and `componentsToFind` parameters kept their original names through
// obfuscation -- they are the only parameters in either signature that read as English, while their
// neighbours are the protector's usual generated names (task/counter/isutil, param/connection/
// skipfilter). Both are preserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Pairs each of <paramref name="transformsToFind"/> with the transform at the same
        /// hierarchy path under <paramref name="destinationRoot"/>.
        /// </summary>
        /// <param name="sourceRoot">The root the paths are measured from.</param>
        /// <param name="destinationRoot">The root the paths are resolved against.</param>
        /// <param name="skipMissing">
        /// Leave a transform out of the result when it has no counterpart, instead of mapping it to
        /// null. Also drops anything that is not under <paramref name="sourceRoot"/> at all.
        /// </param>
        /// <remarks>
        /// For carrying a selection across from one avatar to a copy of it -- the two hierarchies
        /// have the same shape but no shared object identity, so the path is the only thing that
        /// relates them.
        /// </remarks>
        internal static Dictionary<Transform, Transform> MapTransforms(Transform sourceRoot, Transform destinationRoot, bool skipMissing, params Transform[] transformsToFind)
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
                Transform counterpart = destinationRoot.Find(path);

                if (!(counterpart == null && skipMissing))
                {
                    map.Add(transform, counterpart);
                }
            }

            return map;
        }

        /// <summary>
        /// The component form of <see cref="MapTransforms"/>: pairs each of
        /// <paramref name="componentsToFind"/> with the same-typed component at the same path and the
        /// same position in its object's component list.
        /// </summary>
        /// <inheritdoc cref="MapTransforms(Transform, Transform, bool, Transform[])"/>
        /// <remarks>
        /// The ordinal is what makes this work when an object carries several components of one type:
        /// the source component's index among its own object's components of that type is used as the
        /// index into the counterpart's. It assumes the copy kept the same order, which a duplicated
        /// hierarchy does.
        /// <para>
        /// Two failure modes are handled differently, both as shipped. A missing counterpart object
        /// maps to null (or is skipped). A counterpart object that exists but has fewer components of
        /// that type is skipped when <paramref name="skipMissing"/> is set -- and indexes past the end
        /// of the array when it is not, which throws. That second case is a vendor bug; both shipped
        /// call sites pass true.
        /// </para>
        /// </remarks>
        internal static Dictionary<T, T> MapComponents<T>(Transform sourceRoot, Transform destinationRoot, bool skipMissing, params T[] componentsToFind) where T : Component
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
                Transform counterpartTransform = destinationRoot.Find(path);

                if (counterpartTransform == null)
                {
                    if (!skipMissing)
                    {
                        map.Add(component, null);
                    }

                    continue;
                }

                T[] sourceComponents = component.GetComponents<T>();
                T[] counterpartComponents = counterpartTransform.GetComponents<T>();
                int ordinal = System.Array.IndexOf(sourceComponents, component);

                if (!(ordinal >= counterpartComponents.Length && skipMissing))
                {
                    map.Add(component, counterpartComponents[ordinal]);
                }
            }

            return map;
        }
    }
}
