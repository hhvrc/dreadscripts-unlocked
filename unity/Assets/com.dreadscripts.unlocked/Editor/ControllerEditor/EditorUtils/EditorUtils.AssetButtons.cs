// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static itemProperty  -> lastAssetFolder,           line 2122
//   static SetupRules    -> CreateAssetViaSavePanel,   line 4223
//   static IncludeQueue  -> PingButton,                line 6028
//   static MoveRules     -> AssetButtons,              line 4382
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// These are the small button groups that sit at the right-hand end of an asset row -- select an
// existing asset, create a new one, ping the current one -- plus the save-panel helper the create
// button drives. The plain button primitives they are built on (IconButton and friends) live in
// EditorUtils.Buttons.cs and are not repeated here.
//
// The one warning below goes through the rich-text logging family -- CloneResolver / LoginResolver
// at lines 2766 / 2781, ported as Colorize / Log -- which lives in EditorUtils.Logging.cs.
//
// These members write to disk, so for the record: the only thing created is the one asset the user
// names in the save panel, at the path they chose, via AssetDatabase.CreateAsset. Nothing is
// registered with Undo -- matching the avatar-descriptor setters in EditorUtils.AvatarDescriptor.cs,
// which likewise mark their target dirty without an Undo record. Asset creation is not undoable in
// Unity anyway, so here the omission costs nothing; there it is a real gap.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The folder the last asset was created in, used as the starting directory of the next save
        /// panel so a run of creations stays in one place.
        /// </summary>
        /// <remarks>
        /// Domain-reload lifetime only, and deliberately so: it is a convenience, not a setting.
        /// </remarks>
        internal static string lastAssetFolder = "Assets";

        /// <summary>
        /// Asks the user where to save a new asset of type <typeparamref name="T"/>, creates it
        /// there, and hands it to <paramref name="onCreated"/>.
        /// </summary>
        /// <param name="onCreated">
        /// Invoked with the freshly created asset, after it exists on disk. This is where a caller
        /// fills in the asset's contents and assigns it to whatever field asked for it.
        /// </param>
        /// <param name="title">Title bar of the save panel.</param>
        /// <param name="defaultName">
        /// Proposed file name, before uniquifying. The name actually offered is this one made unique
        /// within <see cref="lastAssetFolder"/>, so repeatedly creating from the same button yields
        /// "New Asset", "New Asset 1", and so on rather than prompting to overwrite.
        /// </param>
        /// <param name="extension">Extension for the new file, without the dot.</param>
        /// <remarks>
        /// <para>
        /// Note where the uniquifying stops: <see cref="AssetDatabase.GenerateUniqueAssetPath"/> only
        /// seeds the panel's default name. The path finally used is whatever the user typed, and
        /// <see cref="AssetDatabase.CreateAsset"/> replaces an existing asset at that path rather than
        /// refusing -- so the user can still overwrite, having been asked to confirm by the platform
        /// save dialog first.
        /// </para>
        /// <para>
        /// The instantiation has to branch because <see cref="ScriptableObject"/> subclasses must be
        /// created through <see cref="ScriptableObject.CreateInstance(Type)"/> to get their native
        /// half; anything else (an <see cref="UnityEditor.Animations.AnimatorController"/>, say) is a
        /// normal managed construction.
        /// </para>
        /// </remarks>
        internal static void CreateAssetViaSavePanel<T>(Action<T> onCreated, string title = "Create New File", string defaultName = "New Asset", string extension = "asset") where T : UnityEngine.Object
        {
            // The remembered folder can have been deleted or renamed since it was recorded.
            if (!AssetDatabase.IsValidFolder(lastAssetFolder))
            {
                lastAssetFolder = "Assets";
            }

            defaultName = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(lastAssetFolder + "/" + defaultName + "." + extension));

            string absolutePath = EditorUtility.SaveFilePanel(title, lastAssetFolder, defaultName, extension);
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return;
            }

            string projectRelativePath = FileUtil.GetProjectRelativePath(absolutePath);
            if (!projectRelativePath.StartsWith("Assets"))
            {
                // GetProjectRelativePath returns an empty string for anything outside the project, so
                // this catches both "elsewhere on disk" and "inside the project but outside Assets".
                "Path must be in the Assets folder!".Colorize(LogType.Warning).Log(LogType.Warning);
                return;
            }

            lastAssetFolder = Path.GetDirectoryName(projectRelativePath);

            Type assetType = typeof(T);
            UnityEngine.Object asset = (assetType.IsSubclassOf(typeof(ScriptableObject)) || assetType == typeof(ScriptableObject))
                ? ScriptableObject.CreateInstance(assetType)
                : (Activator.CreateInstance(assetType) as UnityEngine.Object);

            AssetDatabase.CreateAsset(asset, projectRelativePath);
            onCreated?.Invoke(asset as T);
        }

        /// <summary>
        /// A small icon button that reveals and highlights <paramref name="target"/> in the Project
        /// window. Disabled when there is nothing to ping.
        /// </summary>
        internal static void PingButton(UnityEngine.Object target)
        {
            using (new EditorGUI.DisabledScope(target == null))
            {
                if (IconButton(new GUIContent(contents.ping) { tooltip = "Ping in Project window" }, 10f))
                {
                    EditorGUIUtility.PingObject(target);
                }
            }
        }

        /// <summary>
        /// The trailing button group of an asset row: pick an existing asset, create a new one, and
        /// optionally ping the current one.
        /// </summary>
        /// <param name="onAssign">
        /// Invoked with the asset the user chose or created. When null, the select and create buttons
        /// are omitted entirely -- a read-only row -- and only the ping button can appear.
        /// </param>
        /// <param name="current">The asset currently in the row, shown as the picker's initial selection and pinged.</param>
        /// <param name="onCreated">
        /// Applied to a newly created asset before <paramref name="onAssign"/> sees it, so the caller
        /// can populate it while it is still private.
        /// </param>
        /// <param name="extension">File extension for the create button, without the dot.</param>
        /// <param name="allowNull">
        /// Whether picking "None" clears the field. When false, a null selection is dropped and the
        /// row keeps whatever it had.
        /// </param>
        /// <param name="showPing">
        /// Whether to draw the ping button. Off for callers that already ping from the field itself.
        /// </param>
        /// <remarks>
        /// The picker's handler is passed as the selection-changed callback rather than the closed
        /// one, so the row follows the highlighted entry live while the picker is open and a cancelled
        /// picker leaves the last previewed value in place. That is the shipped behaviour; it reads
        /// like an argument-position slip in the original, but it is what the tool does and the
        /// preview is useful, so it is kept.
        /// </remarks>
        internal static void AssetButtons<T>(Action<T> onAssign, T current, Action<T> onCreated = null, string extension = "asset", bool allowNull = true, bool showPing = true) where T : UnityEngine.Object
        {
            if (onAssign != null)
            {
                string typeName = typeof(T).Name;

                if (IconButton(new GUIContent(contents.selectFolder) { tooltip = "Select from Project" }))
                {
                    ShowObjectPicker(current, typeof(T), allowSceneObjects: false, onSelectionChanged: delegate(UnityEngine.Object picked)
                    {
                        if (allowNull || picked != null)
                        {
                            onAssign((T)picked);
                        }
                    });
                }

                string newAssetName = "New " + typeName;

                // A scope that never disables anything: the original's condition had already been
                // folded to a constant by the time it was compiled. Kept so the layout matches.
                using (new EditorGUI.DisabledScope(disabled: false))
                {
                    if (IconButton(new GUIContent(contents.folderAdded) { tooltip = "Create " + newAssetName }))
                    {
                        CreateAssetViaSavePanel(delegate(T created)
                        {
                            onCreated?.Invoke(created);
                            onAssign(created);
                        }, "Create " + newAssetName, newAssetName, extension);
                    }
                }
            }

            if (showPing)
            {
                PingButton(current);
            }
        }
    }
}
