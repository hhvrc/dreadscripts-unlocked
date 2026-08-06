// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the shared commit path that every replacement inspector routes its edits through
// (decompiled lines 6872-6964), and the test-mode toolbar they all draw above it (7076-7128).
//
//   TestConfiguration<T> -> ApplyModifiedProperties<T>, line 6926
//   ReadConfiguration    -> DrawTestModeToolbar,        line 7076
//
// Line numbers are relative to the current snapshot; the decompiled names are the durable
// reference. Field references go through the table in ADOverhaul.State.cs.
//
// PARTIAL PORT. One member of the region is still left out rather than stubbed, and it is not
// blocked on anything: the test-mode entry point it needs -- CompareConfiguration (6290, the
// restart) -- has landed in ADOverhaul.SceneView.cs as RestartTestMode. It is simply still
// unported. ADOSettings, which used to block it as well, has since landed at
// Editor/ADOverhaul/ADOSettings/ and is no longer an obstacle -- the one setting it needs reads and
// writes as `ADOSettings.instance.hasReadColliderTestingWarning.value`, whose setter persists on its
// own, so the decompiled `.SetValue(true)` plus save is a single assignment here.
//
// When it lands it takes `internal` for the same reason as the two members below: its call sites are
// the inspector types this reconstruction lifted out of the class. The "Needs ..." note on the entry
// below records what it calls, not what is still missing -- the callee exists now.
//
//   InsertConfiguration  line 6949 -- the collider restart prompt, raised at most once per test
//       session. Guarded by `isTesting && colliderChangedDuringTest && !hasShownColliderRestartPrompt`,
//       and it sets hasShownColliderRestartPrompt before showing anything, so the prompt cannot
//       repeat even if the user dismisses it. DisplayDialogComplex("Testing Restart Required",
//       "Collider changes require a restart of the testing process. Do you want to restart
//       testing?", ok: "Yes", cancel: "No", alt: "Don't ask again"): "Yes" restarts test mode,
//       "No" -- which is also what Escape maps to -- does nothing beyond the flag already set, and
//       "Don't ask again" persists ADOSettings.hasReadColliderTestingWarning. Nothing destructive
//       happens on any path; the worst outcome is a test session whose simulation no longer matches
//       the edited colliders. Needs CompareConfiguration (line 6290) for the "Yes" arm, and nothing
//       else: the "Don't ask again" arm is now expressible.
//
// Visibility: all three are `private` in the decompiled source, which worked because their callers
// were nested inside the same class. The inspectors have been lifted out to top-level types in this
// reconstruction (PhysBoneEditor, PhysBoneColliderEditor and the two contact editors), so the
// two members below are `internal` instead, and so is the third when it lands. Same assembly, same
// reachable set; it is a consequence of the nesting change recorded in ADOverhaul.State.cs, not a
// widening of the shipped API. The same widening has been applied to DrawAvatarParameterField in
// ADOverhaul.AvatarSelection.cs, which has the contact editors as its only call sites.
//
// 2019 vs 2022: identical. The 2019 build carries the same three members at lines 6850, 6903 and
// 6925 (CalculateSystem / PopSystem / CallSystem) with the same dialog strings and the same
// evaluation order; only the two switch arms of the restart prompt are emitted in the opposite
// order, which does not matter.
//
// Audit status: PARTIAL -- ApplyModifiedProperties, the member this file declared first, diffed
// statement by statement against TestConfiguration<T> in the 2022 snapshot: the destroyed-target
// early out, the `hasModifiedProperties` capture, the per-target callback, the isTesting /
// cloneHasUnappliedChanges pair and the trailing ApplyModifiedProperties all match. The two unported
// members were re-read as well; their descriptions above are accurate, but the claim that they were
// blocked was not -- ToggleTestMode and RestartTestMode have landed in ADOverhaul.SceneView.cs, and
// the PARTIAL PORT note has been corrected to say so. The 2019 counterparts were checked for the
// switch-arm ordering claim. Line numbers not checked -- located by name.
// DrawTestModeToolbar was transcribed statement by statement from decompiled 7076-7128 and matches;
// its 2019 counterpart was not read, so this file is PARTIAL rather than VERIFIED.

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// The test-mode toolbar every replacement inspector draws above its own content: the
        /// test-mode toggle, a restart button and an "Apply Changes" button.
        /// </summary>
        /// <param name="objects">
        /// The objects the calling inspector is editing. Only those that are test-mode clones with
        /// an original still alive are considered by "Apply Changes"; anything else is ignored, so
        /// an inspector can pass its whole selection.
        /// </param>
        /// <returns>
        /// True on the frame "Apply Changes" was pressed, so the caller can rebuild anything it
        /// derived from the objects it just overwrote.
        /// </returns>
        /// <remarks>
        /// Only the toggle is drawn outside test mode; the other two buttons appear once test mode
        /// is running. The toggle is disabled and relabelled while the editor is playing, because
        /// test mode builds a temporary scene hierarchy and play mode owns the scene.
        /// <para>
        /// Applying copies the clone back over the original with
        /// <see cref="EditorUtility.CopySerialized"/>, which would otherwise carry the clone's
        /// <c>rootTransform</c>, <c>ignoreTransforms</c> and <c>colliders</c> across with it -- all
        /// three point into the temporary hierarchy. The <see cref="ReflectionRestoreScope"/> saves
        /// the original's own values for those three and puts them back afterwards.
        /// </para>
        /// </remarks>
        internal static bool DrawTestModeToolbar(IEnumerable<UnityEngine.Object> objects)
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, isTesting, ADOEditorUtility.errorColor))
                {
                    bool isPlaying = Application.isPlaying;
                    string label = isPlaying
                        ? "Editor is in PlayMode"
                        : isTesting
                            ? "Stop Testing - ESC / Enter"
                            : "Test PhysBones in Scene";

                    using (new EditorGUI.DisabledScope(isPlaying))
                    {
                        if (ADOEditorUtility.Button(label))
                        {
                            ToggleTestMode();
                        }
                    }
                }

                if (!isTesting)
                {
                    return false;
                }

                using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.secondaryActionColor))
                {
                    if (ADOEditorUtility.Button("Restart", null, GUILayout.ExpandWidth(false)))
                    {
                        RestartTestMode();
                    }
                }

                UnityEngine.Object[] clones = objects
                    .Where(o => o != null && cloneHasUnappliedChanges.ContainsKey(o) && cloneToOriginal[o] != null)
                    .ToArray();

                bool anyUnapplied = clones.Any(clone => cloneHasUnappliedChanges[clone]);

                using (new GUIColorScope(GUIColorScope.ColoringType.BG, anyUnapplied, ADOEditorUtility.validColor))
                {
                    using (new EditorGUI.DisabledScope(!anyUnapplied))
                    {
                        if (ADOEditorUtility.Button("Apply Changes", null, GUILayout.ExpandWidth(false)))
                        {
                            foreach (UnityEngine.Object clone in clones)
                            {
                                UnityEngine.Object original = cloneToOriginal[clone];
                                using (new ReflectionRestoreScope(original, false, "rootTransform", "ignoreTransforms", "colliders"))
                                {
                                    Undo.RecordObject(original, "ADO - Apply Changes");
                                    EditorUtility.CopySerialized(clone, original);
                                    cloneHasUnappliedChanges[clone] = false;
                                }
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Commits an inspector's pending edits and reports whether there were any, marking each of
        /// <paramref name="targets"/> as needing to be applied back to its original if test mode is
        /// running.
        /// </summary>
        /// <param name="serializedObject">
        /// The inspector's serialized object. A destroyed target is treated as "nothing changed"
        /// and nothing is applied, which is what happens when an inspector repaints one frame after
        /// its object was deleted.
        /// </param>
        /// <param name="targets">
        /// The objects the edit affects. During test mode these are the duplicates inside the
        /// "Physbone Tester" hierarchy, and each one that has an original recorded gets flagged for
        /// the "Apply Changes" button.
        /// </param>
        /// <param name="onChanged">
        /// Per-target follow-up work, run only when something actually changed -- rebuilding a
        /// cached table, say. It runs before <see cref="SerializedObject.ApplyModifiedProperties"/>,
        /// so it sees the object as it was before the edit landed.
        /// </param>
        /// <remarks>
        /// Every target is flagged, not only the ones whose values differ: a multi-object edit sets
        /// the same property on all of them, and the tool has no cheap way to tell which were
        /// already at that value. The result is at worst a redundant CopySerialized on apply.
        /// </remarks>
        internal static bool ApplyModifiedProperties<T>(SerializedObject serializedObject, IEnumerable<T> targets,
            Action<T> onChanged = null) where T : UnityEngine.Object
        {
            if (!serializedObject.targetObject)
            {
                return false;
            }

            bool hasModifiedProperties = serializedObject.hasModifiedProperties;
            if (hasModifiedProperties)
            {
                foreach (T target in targets)
                {
                    onChanged?.Invoke(target);
                    if (isTesting && cloneHasUnappliedChanges.ContainsKey(target))
                    {
                        cloneHasUnappliedChanges[target] = true;
                        hasUnappliedTestChanges = true;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            return hasModifiedProperties;
        }
    }
}
