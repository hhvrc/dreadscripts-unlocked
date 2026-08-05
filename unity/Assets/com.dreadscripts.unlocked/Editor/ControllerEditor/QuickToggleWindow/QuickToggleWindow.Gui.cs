// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class QuickToggleWindow, line 4157
//   UtilityWindowBase<QuickToggleWindow>.OnCustomGUI -> OnCustomGUI, line 4678
// Line numbers are relative to the decompiled snapshot at the time of the port; the member name is
// the durable reference.
//
// This is the window body only. The per-row drawing lives in the ReorderableList callbacks built
// by the factory (decompiled AssetTests, line 4511), which is still unwritten — see the header of
// QuickToggleWindow.cs, which records that its blockers have all since cleared.
//
// Audit status: VERIFIED -- OnCustomGUI is the only member this file declares, and it was diffed
// statement by statement against export ControllerEditor.cs lines 4678-4766 on 2026-08-05: the
// null-targetList self-close, the root ObjectField in its helpBox, the targetList.Draw(), the
// early return when no state has a clip, the foldout header with its help-icon tooltip, the
// three-way mergeMode summary and its GUIColorScope, the cycle switch (case 0 sets replace-all,
// cases 1 and 2 set merge-all), and the per-clip rows with their skip conditions and the
// RefreshMergeMode call. Two deliberate, behaviour-preserving rewrites: the `res`/`res2` ternaries
// are written as C# conditional expressions in the same order, and the decompiler's separate loop
// variables (i/j/k) are renamed. The range contains no goto, no residual switch dispatch, no
// `while (true)` and no unresolved smethod_N, so no deobfuscator fault applies here.

using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class QuickToggleWindow
    {
        /// <summary>
        /// Draws the path root, the list of objects to toggle, and — when any of the selected states
        /// already has a clip — the per-state merge/replace choices.
        /// </summary>
        /// <remarks>
        /// The list helper is only ever built by the factory, so finding it null means Unity revived
        /// the window across a domain reload with none of its state. There is nothing to draw and
        /// nothing to recover, so the window closes itself.
        /// </remarks>
        internal override void OnCustomGUI()
        {
            if (targetList == null)
            {
                Close();
                return;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                root = (Transform)EditorGUILayout.ObjectField(labels[0], root, typeof(Transform), true);
            }

            targetList.Draw();

            if (!hasExistingClips)
            {
                return;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    existingClipsExpanded = EditorGUILayout.Foldout(existingClipsExpanded,
                        new GUIContent($"Existing Clips ({existingClipCount})"));

                    GUILayout.FlexibleSpace();

                    GUILayout.Label(
                        new GUIContent(EditorUtils.contents.help.texture,
                            "Merge: Adds the properties to the existing clips on states. Creates a new clip if no clip exists.\n\nReplace: Replaces the existing clips on states with new clips and adds the properties to them."),
                        GUILayout.Width(14f), GUILayout.Height(18f));

                    string summary = mergeMode == 0 ? "Merge" : mergeMode == 1 ? "Replace" : "Mixed";
                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergeMode,
                        mergeModeColors[0], mergeModeColors[1], mergeModeColors[2]))
                    {
                        if (EditorUtils.Button(summary))
                        {
                            // The button cycles merge -> replace -> merge, and a mixed selection
                            // collapses to merge rather than to whichever mode is in the majority.
                            switch (mergeMode)
                            {
                                case 0:
                                    mergeMode = 1;
                                    for (int i = 0; i < mergePerState.Length; i++)
                                    {
                                        mergePerState[i] = false;
                                    }

                                    break;
                                case 1:
                                case 2:
                                    mergeMode = 0;
                                    for (int i = 0; i < mergePerState.Length; i++)
                                    {
                                        mergePerState[i] = true;
                                    }

                                    break;
                            }
                        }
                    }
                }

                if (!existingClipsExpanded)
                {
                    return;
                }

                using (new IndentedLayoutScope())
                {
                    for (int i = 0; i < states.Count; i++)
                    {
                        AnimatorState state = states[i];
                        if (!state)
                        {
                            continue;
                        }

                        AnimationClip clip = state.motion as AnimationClip;
                        if (!clip)
                        {
                            continue;
                        }

                        using (new GUILayout.HorizontalScope(GUI.skin.box))
                        {
                            GUILayout.Label(clip.name);
                            GUILayout.FlexibleSpace();

                            string mode = mergePerState[i] ? "Merge" : "Replace";
                            using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergePerState[i],
                                mergeModeColors[0], mergeModeColors[1]))
                            {
                                if (EditorUtils.Button(mode))
                                {
                                    mergePerState[i] = !mergePerState[i];
                                    RefreshMergeMode();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
