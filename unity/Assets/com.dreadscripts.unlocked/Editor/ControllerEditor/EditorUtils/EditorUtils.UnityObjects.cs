// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static SetDontSave            -> same,                 line 2586
//   static SetHidden             -> same,                 line 2601
//   static SetDontSaveRecursively -> same,                line 2616
//   static RevertResolver        -> AsComponentOrAsset,   line 3037
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// The first three entries were written against the pre-561e9ec snapshot, where they were still
// obfuscated as InitResolver (2586), VisitResolver (2601) and DefineResolver (2616). The
// re-snapshot renamed them at the same lines; the names above are the current ones.
//
// HideFlags helpers on UnityEngine.Object. SetDontSave toggles the DontSaveInEditor|DontSaveInBuild
// pair (the "temporary object" flags); SetHidden toggles HideInHierarchy|HideInInspector. Both are
// null-tolerant no-ops, matching the original. SetDontSaveRecursively walks every child transform's
// GameObject (including inactive ones) and applies SetDontSave to each.
//
// AsComponentOrAsset is filed here rather than with the asset helpers because it is a cast, not an
// asset operation: it exists so a picker that hands back whatever the user clicked can be asked for
// the type the caller actually wanted.
//
// Audit status: VERIFIED against decompiled/ -- all four methods this file declares were compared
// statement by statement with EditorUtils.cs lines 2586-2622 and 3037-3049, and all four line
// numbers still land on the members they name. The only differences are branch shape with identical
// behaviour: the null guards are written `if (obj == null) return;` where decompiled/ wraps the body
// in `if (!(x == null))`, SetDontSave's two arms are ordered set-then-clear where decompiled/ tests
// the negation first, and AsComponentOrAsset's `obj is GameObject go` replaces the decompiled
// `as GameObject` plus `(object)obj != null` pair.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Sets or clears the DontSaveInEditor and DontSaveInBuild hide flags -- i.e. marks the
        /// object as a temporary object that should not be persisted. No-op on a null object.
        /// </summary>
        internal static void SetDontSave(this Object obj, bool dontSave)
        {
            if (obj == null)
            {
                return;
            }

            if (dontSave)
            {
                obj.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
            else
            {
                obj.hideFlags &= ~(HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild);
            }
        }

        /// <summary>
        /// Sets or clears the HideInHierarchy and HideInInspector hide flags. No-op on a null object.
        /// </summary>
        internal static void SetHidden(this Object obj, bool hidden)
        {
            if (obj == null)
            {
                return;
            }

            if (hidden)
            {
                obj.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
            else
            {
                obj.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
            }
        }

        /// <summary>
        /// Applies <see cref="SetDontSave"/> to the GameObject and every descendant GameObject
        /// (inactive children included).
        /// </summary>
        internal static void SetDontSaveRecursively(this GameObject go, bool dontSave)
        {
            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.SetDontSave(dontSave);
            }
        }

        /// <summary>
        /// <paramref name="obj"/> as <typeparamref name="T"/>, reaching through a GameObject to its
        /// component when <typeparamref name="T"/> is one. Null when it cannot be had.
        /// </summary>
        /// <remarks>
        /// Unity's object picker returns the GameObject for a prefab even when the field being
        /// filled wants a component on it, so a plain cast would drop the selection. Note the test
        /// is IsSubclassOf(Component), so asking for exactly <c>Component</c> takes the plain-cast
        /// path and yields null for a GameObject.
        /// </remarks>
        internal static T AsComponentOrAsset<T>(this Object obj) where T : Object
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                return obj as T;
            }

            return obj is GameObject go ? go.GetComponent<T>() : null;
        }
    }
}
