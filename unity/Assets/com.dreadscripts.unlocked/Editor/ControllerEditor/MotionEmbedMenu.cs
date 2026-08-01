// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested static class MotionEmbedMenu, lines 2207-2400, lifted to a top-level type
//   (same treatment PhysBoneEditor received in the ADOverhaul assembly).
//
//   static ValidateEmbed(MenuCommand)   -> ValidateEmbed,               line 2210
//   static EmbedMotion(MenuCommand)     -> EmbedMotion(MenuCommand),    line 2216
//   static CanEmbed(AnimatorState)      -> CanEmbed,                    line 2221
//   static EmbedMotion(AnimatorState)   -> EmbedMotion(AnimatorState),  line 2230
//   static ValidateExtract(MenuCommand) -> ValidateExtract,             line 2252
//   static CanExtract(AnimatorState)    -> CanExtract,                  line 2257
//   static ExtractMotion(MenuCommand)   -> ExtractMotion(MenuCommand),  line 2267
//   static ExtractMotion(AnimatorState) -> ExtractMotion(AnimatorState),line 2272
//   static RemoveFromAsset(Motion)      -> RemoveFromAsset,             line 2287
//   static MarkScenesDirty()            -> MarkScenesDirty,             line 2359
//   static IsEmbedded(Object)           -> IsEmbedded,                  line 2374
//   static GenerateUniqueName(s, s)     -> GenerateUniqueName,          line 2388
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DEFERRED — not ported, because MotionRenamerWindow (ControllerEditor.cs line 3923) is not in
// the package yet and the package must keep compiling. Port these together with that window:
//   ValidateRename(MenuCommand)  line 2303   [MenuItem("CONTEXT/AnimatorState/Motion/Rename", true)]
//   RenameMotion(MenuCommand)    line 2314   [MenuItem("CONTEXT/AnimatorState/Motion/Rename")]
//   RenameMotion(Motion)         line 2324
// Consequence: the "Rename" entry is absent from the Motion context menu; "Embed" and "Extract"
// are complete. RenameMotion(Motion) is also where the decompiler's spurious `while (true)` at
// line 2340 lives — it is a plain "first EditorWindow whose type is named InspectorWindow" search
// used to position the rename popup, and should be restored as such when it is ported.
//
// NOT PORTED — duplicate. SanitizeFileName (line 2364) is character-for-character the same method
// as EditorUtils.SanitizeFileName (decompiled EditorUtils.cs `CalculateList`, line 7115), which is
// already in the package as EditorUtils.Paths.cs. The two call sites here delegate to it instead.
//
// ASSET MUTATION — read before calling. This menu moves motions between files on disk:
//   * NOTHING here registers Undo. Neither Embed nor Extract can be undone with Ctrl+Z; both are
//     immediate AssetDatabase edits.
//   * Embed DELETES the motion's original asset file when that file contained nothing else. That
//     is how the "move" is performed: RemoveFromAsset strips the object, then AssetDatabase.
//     DeleteAsset removes the now-empty .anim, and AddObjectToAsset re-parents the in-memory
//     object onto the controller.
//   * Extract calls AssetDatabase.CreateAsset at a path it composes itself, without uniquifying
//     it, so it OVERWRITES any asset already sitting at "<controller folder>/<motion name>.anim".
//   * Neither operation calls AssetDatabase.SaveAssets; the controller is left dirty for Unity's
//     normal save. Both call EditorSceneManager.MarkAllScenesDirty (see MarkScenesDirty).
//   * No object is ever destroyed — the Motion instance survives the move; only its owning file
//     changes.

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Context-menu commands on an animator state that move its motion between being a sub-asset of
    /// the controller and being a standalone file.
    /// </summary>
    /// <remarks>
    /// Embedding keeps a controller self-contained — one file to share, no loose clips to lose —
    /// while extracting is what you want when a clip should be reusable or version-controlled on its
    /// own. Unity offers no built-in way to go either direction, hence this menu.
    /// </remarks>
    internal static class MotionEmbedMenu
    {
        [MenuItem("CONTEXT/AnimatorState/Motion/Embed", true)]
        private static bool ValidateEmbed(MenuCommand command)
        {
            return CanEmbed(command.context as AnimatorState);
        }

        [MenuItem("CONTEXT/AnimatorState/Motion/Embed")]
        private static void EmbedMotion(MenuCommand command)
        {
            EmbedMotion(command.context as AnimatorState);
        }

        /// <summary>
        /// True when <paramref name="state"/> has a motion that is not already a sub-asset of some
        /// other file.
        /// </summary>
        private static bool CanEmbed(AnimatorState state)
        {
            if (!(state != null) || !(state.motion != null))
            {
                return false;
            }

            return !IsEmbedded(state.motion);
        }

        /// <summary>
        /// Moves the state's motion into the file that owns the state, deleting the motion's own
        /// asset file if it held nothing else.
        /// </summary>
        /// <remarks>
        /// Not undoable, and destructive to the source file — see the asset-mutation notes in the
        /// file header. Nothing happens if the state is not itself saved in the project, since there
        /// would be no file to embed into.
        /// <para>
        /// SHIPPED BUG, preserved: the confirmation dialog below is unreachable. It exists to warn
        /// that the motion already belongs to another controller, but the method returns early
        /// unless <see cref="CanEmbed"/> passed, and CanEmbed is precisely the assertion that
        /// <see cref="IsEmbedded"/> is false — so <c>!IsEmbedded(motion)</c> is always true and the
        /// dialog is short-circuited away. The guard was presumably written before CanEmbed grew the
        /// same check. Behaviour is unaffected; only the warning is lost.
        /// </para>
        /// </remarks>
        private static void EmbedMotion(AnimatorState state)
        {
            if (!CanEmbed(state))
            {
                return;
            }

            Motion motion = state.motion;

            if (!IsEmbedded(motion) || EditorUtility.DisplayDialog("Caution", "The motion is already embedded into another controller. Do you want to move it anyway?", "Continue", "Cancel"))
            {
                string assetPath = AssetDatabase.GetAssetPath(state);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    RemoveFromAsset(motion);
                    AssetDatabase.AddObjectToAsset(motion, assetPath);

                    // Sub-assets of a controller are noise in the Project window; the built-in
                    // animator sub-assets are hidden the same way.
                    motion.hideFlags |= HideFlags.HideInHierarchy;

                    EditorUtility.SetDirty(motion);
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }

        [MenuItem("CONTEXT/AnimatorState/Motion/Extract", true)]
        private static bool ValidateExtract(MenuCommand command)
        {
            return CanExtract(command.context as AnimatorState);
        }

        /// <summary>
        /// True when <paramref name="state"/> has a motion that is currently a sub-asset of some
        /// file, and so has something to be extracted from.
        /// </summary>
        private static bool CanExtract(AnimatorState state)
        {
            if (!(state != null) || !(state.motion != null))
            {
                return false;
            }

            return IsEmbedded(state.motion);
        }

        [MenuItem("CONTEXT/AnimatorState/Motion/Extract")]
        private static void ExtractMotion(MenuCommand command)
        {
            ExtractMotion(command.context as AnimatorState);
        }

        /// <summary>
        /// Detaches the state's motion from its owning file and saves it as its own asset beside
        /// that file.
        /// </summary>
        /// <remarks>
        /// Not undoable. The destination path is built from the motion's name and is not made
        /// unique, so an existing asset of that name in the same folder is overwritten.
        /// <para>
        /// SHIPPED QUIRK, preserved: the extension is hard-coded to ".anim" even though
        /// <see cref="Motion"/> also covers <see cref="BlendTree"/>. Extracting an embedded blend
        /// tree therefore produces a file named "*.anim" that in fact contains a blend tree. Unity
        /// loads it correctly — the extension is cosmetic here — but it is misleading in the Project
        /// window.
        /// </para>
        /// </remarks>
        private static void ExtractMotion(AnimatorState state)
        {
            if (CanExtract(state))
            {
                Motion motion = state.motion;
                RemoveFromAsset(motion);

                string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(state)) + "/" + EditorUtils.SanitizeFileName(motion.name) + ".anim";
                AssetDatabase.CreateAsset(motion, path);

                motion.hideFlags &= ~HideFlags.HideInHierarchy;

                EditorUtility.SetDirty(motion);
                EditorSceneManager.MarkAllScenesDirty();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Detaches <paramref name="motion"/> from whatever file currently stores it, and deletes
        /// that file if the motion was the only thing in it.
        /// </summary>
        /// <remarks>
        /// The empty-file deletion is what makes embedding a *move* rather than a copy: leaving the
        /// stripped .anim behind would litter the project with files that load as nothing.
        /// <para>
        /// The loop is in the original and is not a decompiler artifact, but it is not really a
        /// loop: <c>assetPath</c> is never reassigned, so at most two iterations run. When the
        /// motion was the file's only asset the first pass deletes the file and repeats; the second
        /// pass then sees an empty <c>LoadAllAssetsAtPath</c>, calls RemoveObjectFromAsset a second
        /// time against an object that no longer belongs to any asset, and breaks. That redundant
        /// second call is harmless but can log a warning. Ported literally rather than collapsed to
        /// an <c>if</c>, so the observable call sequence is unchanged.
        /// </para>
        /// </remarks>
        private static void RemoveFromAsset(Motion motion)
        {
            string assetPath = AssetDatabase.GetAssetPath(motion);

            while (!string.IsNullOrEmpty(assetPath))
            {
                bool wasOnlyAsset = AssetDatabase.LoadAllAssetsAtPath(assetPath).Length == 1;
                AssetDatabase.RemoveObjectFromAsset(motion);

                if (wasOnlyAsset)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    continue;
                }

                break;
            }
        }

        /// <summary>
        /// Marks every open scene dirty.
        /// </summary>
        /// <remarks>
        /// Called after a motion moves files. Nothing in a scene actually changed, but a scene may
        /// hold an Animator pointing at the affected controller, and dirtying the scenes is the
        /// blunt way to make sure the editor does not keep serving stale references from an
        /// unsaved-but-clean scene.
        /// </remarks>
        internal static void MarkScenesDirty()
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        /// <summary>
        /// True when <paramref name="asset"/> is stored inside another asset's file rather than
        /// being that file's main asset.
        /// </summary>
        /// <remarks>
        /// A file with a single asset in it cannot contain a sub-asset, so the count check short-
        /// circuits the more expensive main-asset load. An object that is not saved at all — a
        /// scene object or a freshly constructed one — is not embedded.
        /// </remarks>
        internal static bool IsEmbedded(Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            if (AssetDatabase.LoadAllAssetsAtPath(assetPath).Length > 1)
            {
                return AssetDatabase.LoadMainAssetAtPath(assetPath) != asset;
            }

            return false;
        }

        /// <summary>
        /// Returns a file name, without extension, that does not collide with anything already in
        /// the folder of <paramref name="templatePath"/>.
        /// </summary>
        /// <param name="templatePath">
        /// An asset path supplying the target folder and the extension to test against; its file
        /// name is ignored.
        /// </param>
        /// <param name="desiredName">The name to start from; sanitised before use.</param>
        /// <remarks>
        /// The extension is taken from the template only so that
        /// <see cref="AssetDatabase.GenerateUniqueAssetPath"/> tests the right file name, and is
        /// then stripped again — callers want a name to assign to an object, not a path.
        /// <para>
        /// SIDE EFFECT: creates the target folder if it is missing, and imports it so it appears in
        /// the Project window immediately. GenerateUniqueAssetPath cannot report a collision-free
        /// name for a folder the AssetDatabase has never seen.
        /// </para>
        /// </remarks>
        internal static string GenerateUniqueName(string templatePath, string desiredName)
        {
            desiredName = EditorUtils.SanitizeFileName(desiredName);

            string directory = Path.GetDirectoryName(templatePath);
            string extension = Path.GetExtension(templatePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.ImportAsset(directory);
            }

            return Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(directory + "/" + desiredName + extension));
        }
    }
}
