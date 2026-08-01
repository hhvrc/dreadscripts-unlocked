// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static EnableRules -> FolderField, line 4249
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This is the "Generated Assets Path" row of the settings window, which
// ControllerEditorWindow.Defaults (DrawOtherDefaults) records as DEFERRED pending this method. That
// deferral can now be closed: the call is
//   EditorSettings.Instance.saveFolder.value = EditorUtils.FolderField(saveFolder, "Generated Assets Path");
// with the shipped write-back guard at decompiled ControllerEditor.cs line 3834 preserved.
//
// A shipped bug in the "walk up to a real folder" step is preserved verbatim; see the remarks.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A labelled, read-only path field with a folder browser and a ping button, returning either
        /// the newly chosen folder or <paramref name="path"/> unchanged.
        /// </summary>
        /// <returns>
        /// The picked folder as a project-relative path, or <paramref name="path"/> if the user
        /// cancelled or picked somewhere outside the project. The caller owns the storage: nothing is
        /// written back from here, and nothing is marked dirty.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The path is drawn with <see cref="EditorGUILayout.SelectableLabel"/> styled as an object
        /// field rather than as a text field. It therefore looks editable and is not -- the text can
        /// be selected and copied but not typed into, which is deliberate: a hand-typed path could
        /// name a folder that does not exist, and the rest of the tool assumes this one does.
        /// </para>
        /// <para>
        /// The ping button is disabled unless the current path is a real folder, so the usual
        /// "PingObject on null does nothing visible" confusion cannot arise.
        /// </para>
        /// <para>
        /// SHIPPED BUG, preserved. The browser opens at the nearest existing ancestor of the current
        /// path, found by repeatedly stripping the last segment until
        /// <see cref="AssetDatabase.IsValidFolder"/> accepts what is left. Two things are wrong with
        /// that loop and both are as shipped. First, <see cref="Path.GetDirectoryName(string)"/>
        /// returns a path with the platform separator, so on Windows the very first strip turns
        /// "Assets/A/B" into "Assets\A", which <see cref="AssetDatabase.IsValidFolder"/> does not
        /// recognise; the loop then keeps stripping until it reaches the bare "Assets", which it does
        /// accept. The effect is that the browser almost always opens at the project root instead of
        /// near the configured folder -- an annoyance, not a hazard, and it does terminate. Second,
        /// a stored path that starts with the letters "Assets" but has no separator at all -- the
        /// degenerate "AssetsFoo" -- passes the <c>StartsWith</c> test, strips to the empty string,
        /// and then throws out of <see cref="Path.GetDirectoryName(string)"/>. Only a hand-edited
        /// preference can reach that state, which is presumably why it was never hit.
        /// </para>
        /// </remarks>
        internal static string FolderField(string path, string label)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.SelectableLabel(path, EditorStyles.objectField, GUILayout.Height(16f),
                    GUILayout.ExpandWidth(true));

                if (IconButton(contents.selectFolder))
                {
                    string startingFolder = path;
                    if (!startingFolder.StartsWith("Assets"))
                    {
                        startingFolder = "Assets";
                    }
                    else
                    {
                        while (!AssetDatabase.IsValidFolder(startingFolder))
                        {
                            startingFolder = Path.GetDirectoryName(startingFolder);
                        }
                    }

                    string picked = EditorUtility.OpenFolderPanel(label, startingFolder, string.Empty);
                    if (string.IsNullOrEmpty(picked))
                    {
                        return path;
                    }

                    string relative = FileUtil.GetProjectRelativePath(picked);
                    if (!relative.StartsWith("Assets"))
                    {
                        // A folder outside the project cannot hold generated assets at all, so the
                        // choice is rejected rather than stored and failed on later.
                        "New Path must be a folder within Assets!".LogColored(LogType.Warning);
                        return path;
                    }

                    path = relative;
                }

                using (new EditorGUI.DisabledScope(!AssetDatabase.IsValidFolder(path)))
                {
                    if (IconButton(contents.ping))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
                    }
                }
            }

            return path;
        }
    }
}
