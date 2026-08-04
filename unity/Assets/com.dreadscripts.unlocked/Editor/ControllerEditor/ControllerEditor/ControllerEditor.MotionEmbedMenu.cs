// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   MotionEmbedMenu -> MotionEmbedMenu, lines 2207-2400 (all member names already in renames/)
//     ValidateEmbed, EmbedMotion x2, CanEmbed, ValidateExtract, CanExtract, ExtractMotion x2,
//     RemoveFromAsset, ValidateRename, RenameMotion x2, MarkScenesDirty, SanitizeFileName,
//     IsEmbedded, GenerateUniqueName -> unchanged, lines 2210-2398
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DEOBF-BUG(guessed) -- RemoveFromAsset, export line 2290.
//   export/ renders the body as
//       while (!string.IsNullOrEmpty(assetPath))
//       { bool only = ...; RemoveObjectFromAsset(motion); if (only) { DeleteAsset(assetPath); continue; } break; }
//   `assetPath` is never reassigned, so the `continue` re-enters with the same condition and the
//   body runs a second time — calling RemoveObjectFromAsset again on an asset file that was just
//   deleted. That backward edge is the known de4dot fault of recovering a Reactor-flattened `if`
//   as a `while` (RE_NOTES, "Shapes of decompile damage"), and the shipped code is the plain `if`
//   written below. GUESSED: neither ADOverhaul build has this method, so there is no second
//   decompile to check it against, and the original IL was not traced. What would settle it is a
//   trace of the corresponding obfuscated method. export/ will keep showing the loop.
//
// The InspectorWindow search in RenameMotion(Motion) is a plain de-flattening of export's
// `while (true) { ... }` scan, not a deviation: same predicate, same early return when nothing
// matches.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04), except the DEOBF-BUG site.

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The Motion submenu on an animator state's context menu: move a clip into the controller
        /// asset, pull it back out into its own file, or rename it.
        /// </summary>
        /// <remarks>
        /// "Embedded" here means the motion is a sub-asset of some other asset file rather than the
        /// main asset of its own — which is what <see cref="IsEmbedded"/> tests, and what decides
        /// whether renaming means setting <c>name</c> or renaming a file on disk.
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

            private static bool CanEmbed(AnimatorState state)
            {
                if (state == null || state.motion == null)
                {
                    return false;
                }

                return !IsEmbedded(state.motion);
            }

            private static void EmbedMotion(AnimatorState state)
            {
                if (!CanEmbed(state))
                {
                    return;
                }

                Motion motion = state.motion;

                // CanEmbed already rejected an embedded motion, so the dialog is unreachable in
                // practice; it is what the shipped build does, and is kept.
                if (!IsEmbedded(motion) || EditorUtility.DisplayDialog("Caution",
                        "The motion is already embedded into another controller. Do you want to move it anyway?",
                        "Continue", "Cancel"))
                {
                    string assetPath = AssetDatabase.GetAssetPath(state);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        RemoveFromAsset(motion);
                        AssetDatabase.AddObjectToAsset(motion, assetPath);
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

            private static bool CanExtract(AnimatorState state)
            {
                if (state == null || state.motion == null)
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

            private static void ExtractMotion(AnimatorState state)
            {
                if (!CanExtract(state))
                {
                    return;
                }

                Motion motion = state.motion;
                RemoveFromAsset(motion);

                string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(state))
                              + "/" + SanitizeFileName(motion.name) + ".anim";
                AssetDatabase.CreateAsset(motion, path);
                motion.hideFlags &= ~HideFlags.HideInHierarchy;
                EditorUtility.SetDirty(motion);
                EditorSceneManager.MarkAllScenesDirty();
                AssetDatabase.Refresh();
            }

            /// <summary>
            /// Detaches a motion from whatever asset file currently holds it, deleting that file if
            /// the motion was the only thing in it.
            /// </summary>
            private static void RemoveFromAsset(Motion motion)
            {
                string assetPath = AssetDatabase.GetAssetPath(motion);

                // DEOBF-BUG(guessed): export/ has this as a `while` with a `continue`. See the header.
                if (!string.IsNullOrEmpty(assetPath))
                {
                    bool isOnlyAssetInFile = AssetDatabase.LoadAllAssetsAtPath(assetPath).Length == 1;
                    AssetDatabase.RemoveObjectFromAsset(motion);
                    if (isOnlyAssetInFile)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Rename", true)]
            private static bool ValidateRename(MenuCommand command)
            {
                AnimatorState state = command.context as AnimatorState;
                if (state != null)
                {
                    return state.motion != null;
                }

                return false;
            }

            [MenuItem("CONTEXT/AnimatorState/Motion/Rename")]
            private static void RenameMotion(MenuCommand command)
            {
                AnimatorState state = command.context as AnimatorState;
                if (state != null && state.motion != null)
                {
                    RenameMotion(state.motion);
                }
            }

            /// <summary>
            /// Opens the rename popup for a motion, adding to the pending list if one is already
            /// open so a multi-selection renames together. Only the first motion positions the
            /// window, under the inspector it was invoked from.
            /// </summary>
            private static void RenameMotion(Motion motion)
            {
                if (motion == null)
                {
                    return;
                }

                MotionRenamerWindow window = EditorWindow.GetWindow<MotionRenamerWindow>(true, "Motion Rename");
                window.motions.Add(motion);
                if (window.motions.Count != 1)
                {
                    return;
                }

                window.newName = motion.name;

                EditorWindow inspector = Resources.FindObjectsOfTypeAll<EditorWindow>()
                    .FirstOrDefault(w => w != null && w.GetType().Name == "InspectorWindow");
                if (inspector == null)
                {
                    return;
                }

                Vector2 position = inspector.position.position + new Vector2(0f, 50f);
                Vector2 size = window.maxSize = window.minSize = new Vector2(300f, 50f);
                window.position = new Rect(position, size);
            }

            internal static void MarkScenesDirty()
            {
                EditorSceneManager.MarkAllScenesDirty();
            }

            /// <summary>Replaces every character the filesystem rejects with a dash.</summary>
            internal static string SanitizeFileName(string name)
            {
                string invalid = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
                if (string.IsNullOrEmpty(name))
                {
                    return "Unnamed";
                }

                return Regex.Replace(name, "[" + invalid + "]", "-");
            }

            /// <summary>
            /// True when the object is a sub-asset: its file holds more than one asset and it is not
            /// the main one.
            /// </summary>
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
            /// Returns a file name, based on <paramref name="name"/>, that is free in the directory
            /// of <paramref name="referencePath"/>, creating that directory if it does not exist.
            /// </summary>
            internal static string GenerateUniqueName(string referencePath, string name)
            {
                name = SanitizeFileName(name);
                string directory = Path.GetDirectoryName(referencePath);
                string extension = Path.GetExtension(referencePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    AssetDatabase.ImportAsset(directory);
                }

                return Path.GetFileNameWithoutExtension(
                    AssetDatabase.GenerateUniqueAssetPath(directory + "/" + name + extension));
            }
        }
    }
}
