// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the shared commit path that every replacement inspector routes its edits through,
// decompiled lines 6872-6964.
//
//   TestConfiguration<T> -> ApplyModifiedProperties<T>, line 6926
//
// Line numbers are relative to the current snapshot; the decompiled names are the durable
// reference. Field references go through the table in ADOverhaul.State.cs.
//
// PARTIAL PORT. The other two members of the region are left out rather than stubbed. Neither is
// blocked on anything any more: the test-mode entry points they need -- NewConfiguration (6272, the
// toggle) and CompareConfiguration (6290, the restart) -- have since landed in
// ADOverhaul.SceneView.cs as ToggleTestMode and RestartTestMode. They are simply still unported.
// ADOSettings, which used to block InsertConfiguration as well, has since landed at
// Editor/ADOverhaul/ADOSettings/ and is no longer an obstacle -- the one setting it needs reads and
// writes as `ADOSettings.instance.hasReadColliderTestingWarning.value`, whose setter persists on its
// own, so the decompiled `.SetValue(true)` plus save is a single assignment here.
//
// When they land, both take `internal` for the same reason as the member below: their call sites are
// the inspector types this reconstruction lifted out of the class. The "Needs ..." notes on the two
// entries below record what each one calls, not what is still missing -- both callees exist now.
//
//   ReadConfiguration    line 6872 -- the test-mode toolbar drawn above every PhysBone and collider
//       inspector: a "Test PhysBones in Scene" / "Stop Testing - ESC / Enter" toggle (disabled and
//       relabelled "Editor is in PlayMode" while playing), a "Restart" button, and an "Apply
//       Changes" button that is enabled only when one of the passed objects is a test-mode clone
//       with unapplied edits. Applying copies each clone back onto its original with
//       EditorUtility.CopySerialized inside a ReflectionRestoreScope that preserves the original's
//       rootTransform, ignoreTransforms and colliders -- those are scene references that must not
//       follow the clone -- records an Undo step named "ADO - Apply Changes", and clears the
//       clone's dirty flag. Needs NewConfiguration (line 6272, the test-mode toggle) and
//       CompareConfiguration (line 6290, the restart).
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
// member below is `internal` instead, and so are the other two when they land. Same assembly, same
// reachable set; it is a consequence of the nesting change recorded in ADOverhaul.State.cs, not a
// widening of the shipped API. The same widening has been applied to DrawAvatarParameterField in
// ADOverhaul.AvatarSelection.cs, which has the contact editors as its only call sites.
//
// 2019 vs 2022: identical. The 2019 build carries the same three members at lines 6850, 6903 and
// 6925 (CalculateSystem / PopSystem / CallSystem) with the same dialog strings and the same
// evaluation order; only the two switch arms of the restart prompt are emitted in the opposite
// order, which does not matter.
//
// Audit status: VERIFIED -- ApplyModifiedProperties, the only member this file declares, diffed
// statement by statement against TestConfiguration<T> in the 2022 snapshot: the destroyed-target
// early out, the `hasModifiedProperties` capture, the per-target callback, the isTesting /
// cloneHasUnappliedChanges pair and the trailing ApplyModifiedProperties all match. The two unported
// members were re-read as well; their descriptions above are accurate, but the claim that they were
// blocked was not -- ToggleTestMode and RestartTestMode have landed in ADOverhaul.SceneView.cs, and
// the PARTIAL PORT note has been corrected to say so. The 2019 counterparts were checked for the
// switch-arm ordering claim. Line numbers not checked -- located by name.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
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
