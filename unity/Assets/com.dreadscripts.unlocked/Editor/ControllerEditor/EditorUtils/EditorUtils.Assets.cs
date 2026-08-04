// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static LogoutPredicate    -> DestroyAssetObject,      line 4058
//   static PatchPredicate     -> IsSubAsset,              line 4082
//   static InterruptPredicate -> AddSubAsset,             line 4088
//   static ManagePredicate    -> SetDirty,                line 4103
//   static PrintPredicate     -> RecordChange,            line 4108
//   static SearchPredicate    -> LoadByGuid<T>(string, long),   line 4130
//   static RevertPredicate    -> LoadByGuid<T>(string, string), line 4170
//   static CompareRules       -> CloneSerialized,         line 4193
//   static SetRules           -> CloneToAsset,            line 4205
//   static PostRules          -> CloneAsSubAsset,         line 4216
//   static PublishRules       -> AddNoteSubAsset,         line 4293
//   static ConcatRules        -> IsNull,                  line 4422
//   static MapError           -> TryRecordPrefabModifications, line 8478
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Everything here is about the difference between a Unity object that IS an asset file and one
// that merely lives inside one. AnimatorControllers, blend trees, state machines, states and
// StateMachineBehaviours are all stored as sub-assets of a single .controller file, so "delete
// this" and "duplicate this" have two completely different implementations depending on which of
// the two an object is -- which is what IsSubAsset asks and what the rest of this file branches on.
//
// MapError sits at the far end of the decompiled file (line 8478) rather than beside its only
// caller, because it is a [CompilerGenerated] local function of RecordChange that the obfuscator
// lifted out. It is written back as an ordinary member here rather than as a nested local
// function, since RecordChange captures nothing and so nothing is lost by doing so.

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Destroys <paramref name="asset"/>, deleting the whole asset file if it is the file's
        /// main asset and detaching it from its parent if it is a sub-asset. Does nothing for an
        /// object that is not an asset at all.
        /// </summary>
        internal static void DestroyAssetObject(UnityEngine.Object asset, bool recordUndo = false)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            UnityEngine.Object mainAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (!mainAsset)
            {
                return;
            }

            if (mainAsset == asset)
            {
                AssetDatabase.DeleteAsset(assetPath);
                return;
            }

            AssetDatabase.RemoveObjectFromAsset(asset);
            if (recordUndo)
            {
                Undo.DestroyObjectImmediate(asset);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        /// <summary>
        /// Whether <paramref name="asset"/> is stored inside someone else's asset file rather than
        /// being one itself, reporting the file's main asset in <paramref name="mainAsset"/>.
        /// </summary>
        /// <remarks>
        /// An object that is not an asset at all also answers true, because its path is empty and
        /// so its "main asset" is null, which is not itself. Callers that care have to check the
        /// path separately.
        /// </remarks>
        internal static bool IsSubAsset(UnityEngine.Object asset, out UnityEngine.Object mainAsset)
        {
            mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(asset));
            return asset != mainAsset;
        }

        /// <summary>
        /// Stores <paramref name="asset"/> inside <paramref name="parent"/>'s asset file.
        /// </summary>
        /// <param name="hide">
        /// Hide the sub-asset in the Project window and the inspector, which is what Unity itself
        /// does for the machinery inside a controller.
        /// </param>
        /// <param name="lockEditing">Additionally mark it not editable.</param>
        internal static void AddSubAsset(UnityEngine.Object asset, UnityEngine.Object parent, bool hide = true,
            bool lockEditing = false)
        {
            AssetDatabase.AddObjectToAsset(asset, parent);
            if (hide)
            {
                asset.hideFlags |= HideFlags.HideInHierarchy;
                asset.hideFlags |= HideFlags.HideInInspector;
            }

            if (lockEditing)
            {
                asset.hideFlags |= HideFlags.NotEditable;
            }

            EditorUtility.SetDirty(parent);
        }

        /// <summary>
        /// <see cref="EditorUtility.SetDirty"/> as an extension, so it can end a chain.
        /// </summary>
        internal static void SetDirty(this UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Marks <paramref name="target"/> as changed by whichever mechanism actually applies to
        /// it: a prefab instance records a property modification, an asset is set dirty, and a
        /// scene object additionally marks its scene dirty.
        /// </summary>
        /// <remarks>
        /// Wrapped in a bare catch, as shipped. The calls it makes are all documented to be safe on
        /// a destroyed or non-asset object, so this mostly guards against being called from a
        /// context Unity does not allow them in -- and the vendor evidently preferred a missed
        /// dirty flag to an exception out of a GUI callback.
        /// </remarks>
        internal static void RecordChange(this UnityEngine.Object target)
        {
            try
            {
                if (!TryRecordPrefabModifications(target))
                {
                    EditorUtility.SetDirty(target);
                }

                if (target is GameObject gameObject)
                {
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
                else if (target is Component component)
                {
                    EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Records a prefab instance's overrides, and reports whether
        /// <paramref name="target"/> was a prefab at all.
        /// </summary>
        internal static bool TryRecordPrefabModifications(UnityEngine.Object target)
        {
            if (PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.NotAPrefab)
            {
                return false;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            return true;
        }

        /// <summary>
        /// The object with GUID <paramref name="guid"/> and local file id
        /// <paramref name="localId"/>, or null.
        /// </summary>
        /// <param name="localId">
        /// Zero means "the main asset", in which case the file's main asset is returned without the
        /// sub-asset scan.
        /// </param>
        /// <remarks>
        /// A GUID alone identifies the file; inside it every sub-asset has its own local id, which
        /// is what distinguishes one layer's state machine from another's. The fast path tries the
        /// main asset first and only falls back to LoadAllAssetsAtPath -- which deserialises every
        /// object in the file -- when that is not the one wanted.
        /// </remarks>
        internal static T LoadByGuid<T>(string guid, long localId) where T : UnityEngine.Object
        {
            T mainAsset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (mainAsset != null)
            {
                if (localId == 0L)
                {
                    return mainAsset;
                }

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mainAsset, out string _, out long mainLocalId);
                if (mainLocalId == localId)
                {
                    return mainAsset;
                }
            }

            foreach (UnityEngine.Object candidate in AssetDatabase.LoadAllAssetsAtPath(
                         AssetDatabase.GUIDToAssetPath(guid)))
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out string _, out long candidateLocalId);
                if (candidateLocalId == localId && candidate is T match)
                {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// <see cref="LoadByGuid{T}(string, long)"/> taking the local id as text, as it is stored
        /// in the tool's saved references. Blank means "the main asset".
        /// </summary>
        internal static T LoadByGuid<T>(string guid, string localId) where T : UnityEngine.Object
        {
            return LoadByGuid<T>(guid, string.IsNullOrWhiteSpace(localId) ? 0L : long.Parse(localId));
        }

        /// <summary>
        /// A detached copy of <paramref name="original"/> with every serialised field copied
        /// across, belonging to no asset file.
        /// </summary>
        /// <remarks>
        /// Deliberately not <c>Object.Instantiate</c>: that renames the copy ("Foo (Clone)"), and
        /// on a Component or GameObject it would drag the whole hierarchy along. CopySerialized
        /// leaves the name alone, which the layer-copy path depends on.
        /// </remarks>
        internal static T CloneSerialized<T>(T original) where T : UnityEngine.Object
        {
            if (!original)
            {
                return null;
            }

            Type type = original.GetType();
            UnityEngine.Object copy = (type.IsSubclassOf(typeof(ScriptableObject)) || type == typeof(ScriptableObject))
                ? ScriptableObject.CreateInstance(type)
                : (UnityEngine.Object)Activator.CreateInstance(type);

            EditorUtility.CopySerialized(original, copy);
            return (T)copy;
        }

        /// <summary>
        /// A copy of <paramref name="original"/> saved as its own asset at <paramref name="path"/>.
        /// </summary>
        /// <param name="wasSubAsset">
        /// Whether the original lived inside another asset file; see <see cref="IsSubAsset"/> for
        /// the caveat about non-assets.
        /// </param>
        /// <param name="alwaysCreateAsset">
        /// When false, a copy of something that was already a sub-asset is returned unsaved, so the
        /// caller can attach it somewhere itself rather than have a stray file created.
        /// </param>
        internal static T CloneToAsset<T>(T original, string path, out bool wasSubAsset,
            bool alwaysCreateAsset = true) where T : UnityEngine.Object
        {
            wasSubAsset = IsSubAsset(original, out UnityEngine.Object _);
            T copy = CloneSerialized(original);
            if (!wasSubAsset || alwaysCreateAsset)
            {
                AssetDatabase.CreateAsset(copy, path);
            }

            return copy;
        }

        /// <summary>
        /// A copy of <paramref name="original"/> stored inside <paramref name="parent"/>'s asset
        /// file, hidden the way <see cref="AddSubAsset"/> hides things by default.
        /// </summary>
        internal static T CloneAsSubAsset<T>(T original, UnityEngine.Object parent) where T : UnityEngine.Object
        {
            T copy = CloneSerialized(original);
            AddSubAsset(copy, parent);
            return copy;
        }

        /// <summary>
        /// Stores <paramref name="text"/> inside <paramref name="parent"/>'s asset file as a
        /// hidden, non-editable TextAsset -- the tool's way of leaving a note on an asset.
        /// </summary>
        /// <remarks>
        /// The TextAsset's name is the text as well as its content, because the Project window
        /// shows the name and nothing in the tool ever reads the content back.
        /// </remarks>
        internal static void AddNoteSubAsset(UnityEngine.Object parent, string text)
        {
            AssetDatabase.AddObjectToAsset(new TextAsset(text)
            {
                name = text,
                hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable
            }, parent);
        }

        /// <summary>
        /// Unity's null test as an extension: true for a real null reference and for a destroyed
        /// object alike. Use <c>IsMissing</c> instead when the two need telling apart.
        /// </summary>
        internal static bool IsNull(this UnityEngine.Object target)
        {
            return target == null;
        }
    }
}
