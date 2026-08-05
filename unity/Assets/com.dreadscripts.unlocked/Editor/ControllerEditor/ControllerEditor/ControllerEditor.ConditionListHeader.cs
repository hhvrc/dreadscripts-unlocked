// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   CalculateVisitor -> DrawConditionListHeader, line 11345
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// The header row of the condition ReorderableList, and the only place in the condition editor with
// controls that act on the whole set rather than on one condition: copy, paste, invert all, the
// matching-options toggle, and the merge/split pair.
//
// It draws two quite different left-hand halves depending on whether a transition is focused. With
// one focused it is a copy/paste pair and the transition's name; with none it is the Shared/All
// toggle and its label. The right-hand half is then drawn unconditionally over both, which is why
// the rect arithmetic below looks like it is fighting itself: `clipboardRect` is offset a further
// 29px when the Shared/All toggle is showing, and the whole second copy/paste pair is skipped when
// a transition is focused because the first half already drew one.
//
// GUIDisabledScope(disabled: false) appears twice, which reads oddly until you notice the list is
// drawn inside a DisabledScope by ControllerEditor.ConditionSection.cs whenever nothing is
// selected. These two force-enable their contents inside it: the Shared/All toggle and the
// matching-options gear stay usable with an empty selection, since neither acts on the selection.
//
// The invert button is EditorUtils.contents.switchLayer, whose name is the icon's, not the
// button's. It reverses every condition in the current set, and which set that is decides what has
// to be rebuilt afterwards -- inverting the focused rows invalidates the shared set, inverting the
// shared rows invalidates the whole-selection set, and inverting the whole-selection rows
// invalidates the shared set. Each arm rebuilds exactly the other one.
//
// The focused arm is also the one arm that does not go through ConditionMultiEditor.Invert(): it
// inverts each row's condition itself and pushes the result with ApplyToAll. The two differ in
// that Invert() reverses each target from its own value while this reverses every target onto the
// row's value, so a focused row -- which has exactly one target -- gets the same result either
// way. Ported as shipped rather than unified.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// Two, both of the scattered inline `(Func<bool>)delegate { HMACSHA256 over the licence key,
// compared against licenseToken }` form, and both dropped on the package-wide basis recorded in
// ControllerEditor.TransitionSection.cs.
//
//   * decompiled 11349-11357, the first statement of the focused branch, guarding everything that
//     branch draws with an early `return`. Under it, a focused transition's header would draw
//     nothing at all -- no name, no copy, no paste -- and then fall through to the shared right-hand
//     half, which the same `return` also skips.
//   * decompiled 11482-11490, inside the invert button's focused arm, between the loop that
//     inverts the rows and the two calls that rebuild the other sets. Under it the rows would be
//     inverted and the shared set left stale.
//
// Both are removed and the code they guarded is kept, so the header behaves as though the check
// passed. Nothing else in decompiled 11345-11508 is licence-related.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// Three inverted tests, all ILSpy's rendering of a branch-if-true rather than anything the source
// can have said:
//   `if (!(focusedTransition.transition == null)) { A } else { B }`  -> `if (... != null) { A } else { B }`
//   `if (!(focusedTransition.transition != null)) { B } else { A }`  -> the same, in the invert handler
//   `(!showSharedConditions) ? "All Conditions" : "Shared Conditions"` -> the ternary the other way up
// The conditions, arms and pairings are unchanged.
//
// The decompiled `EditorUtils.contents()` and `EditorUtils.styles()` are read as properties and
// `focusedTransition.DisplayName()` as a property, per the decisions recorded in
// EditorUtils.Contents.cs, EditorUtils.Styles.cs and AnimatorGraphReflection.TransitionEditionInfo.cs.
// `EditorUtils.QueryQueue(Rect, GUIContent, GUIStyle)` is the decompiled spelling of what
// EditorUtils.Buttons.cs ports as `Button(Rect, GUIContent, GUIStyle)`.
//
// Two `EditorGUI.DisabledScope` uses that the decompiled body renders as an explicit
// try/finally around a local (ILSpy's rendering of `using` on a constrained value type) are written
// back as `using`, matching their four siblings in the same method that ILSpy did render as `using`.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- diffed statement for statement against decompiled lines 11345-11508:
// every rect construction and offset, both halves of the left-hand branch, the two disabled
// predicates on the second copy/paste pair, the compound predicate on the invert button's disabled
// group, all three arms of the invert handler with their differing rebuild calls, the settings
// gear's tooltip string, and the merge/split pair with the disabled scope on merge only. That
// range contains no `goto`, no residual `switch` dispatch, no `while (true)` and no unresolved
// `smethod_N`, so no deobfuscator fault applies to it -- the two licence delegates above are the
// only things removed.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition list header

        /// <summary>
        /// Draws the condition list's header: the set-wide copy/paste and invert controls, the
        /// Shared/All toggle or the focused transition's name, and the matching-options,
        /// merge and split buttons.
        /// </summary>
        /// <param name="rect">The header strip. Consumed by value; the local offsets do not escape.</param>
        private static void DrawConditionListHeader(Rect rect)
        {
            if (focusedTransition.transition != null)
            {
                Rect focusedRect = new Rect(rect) { width = 18f };

                using (new EditorGUI.DisabledScope(focusedTransition.transition.conditions.Length == 0))
                {
                    if (EditorUtils.Button(focusedRect, EditorUtils.contents.copy, GUI.skin.label))
                    {
                        CopyConditions();
                    }
                }

                focusedRect.x += 20f;
                using (new EditorGUI.DisabledScope(copiedConditions.Count == 0))
                {
                    if (EditorUtils.Button(focusedRect, EditorUtils.contents.paste, GUI.skin.label))
                    {
                        PasteConditions();
                    }
                }

                focusedRect.x += 20f;
                focusedRect.width = rect.width - 40f;
                GUI.Label(focusedRect, focusedTransition.DisplayName + "'s Conditions");
            }
            else
            {
                // Force-enabled: the toggle does not act on the selection, so it stays usable when
                // the list as a whole is disabled for having none.
                using (new GUIDisabledScope(disabled: false))
                {
                    Rect toggleRect = new Rect(rect)
                    {
                        width = 16f,
                        x = rect.x - 3f,
                        y = rect.y + 2f
                    };

                    if (EditorUtils.Button(toggleRect, EditorUtils.contents.shared, GUIStyle.none))
                    {
                        showSharedConditions = !showSharedConditions;
                        RebuildConditionList();
                    }
                }

                rect.x += 12f;
                GUI.Label(rect, showSharedConditions ? "Shared Conditions" : "All Conditions");
                rect.x -= 12f;
            }

            Rect clipboardRect = new Rect(rect);
            clipboardRect.x += 95f;
            if (showSharedConditions)
            {
                // Clear of the Shared/All label, which is the wider of the two.
                clipboardRect.x += 29f;
            }

            clipboardRect.width = 18f;

            if (!focusedTransition.transition)
            {
                using (new EditorGUI.DisabledScope(
                           (showSharedConditions && sharedConditionEditors.Count == 0)
                           || (!showSharedConditions && allConditionEditors.Count == 0)))
                {
                    if (EditorUtils.Button(clipboardRect, EditorUtils.contents.copy, GUI.skin.label))
                    {
                        CopyConditions();
                    }
                }

                clipboardRect.x += 20f;
                using (new EditorGUI.DisabledScope(copiedConditions.Count == 0))
                {
                    if (EditorUtils.Button(clipboardRect, EditorUtils.contents.paste, GUI.skin.label))
                    {
                        PasteConditions();
                    }
                }
            }
            else
            {
                // The focused branch above already drew a copy/paste pair; only the offset is kept.
                clipboardRect.x += 20f;
            }

            Rect invertRect = new Rect(rect);
            invertRect.y += 2f;
            invertRect.x += rect.width / 2f + rect.width / 8f - 25f;
            invertRect.width = 15f;

            EditorGUI.BeginDisabledGroup(
                (!focusedTransition.transition
                 && ((showSharedConditions && sharedConditionEditors.Count == 0)
                     || (!showSharedConditions && allConditionEditors.Count == 0)))
                || (focusedTransition.transition != null && focusedTransition.transition.conditions.Length < 1));

            if (EditorUtils.Button(invertRect, EditorUtils.contents.switchLayer, GUIStyle.none))
            {
                if (focusedTransition.transition != null)
                {
                    foreach (ConditionMultiEditor editor in focusedConditionEditors)
                    {
                        AnimatorCondition inverted = InvertCondition(editor.condition);
                        editor.ApplyToAll(inverted);
                        editor.condition = inverted;
                    }

                    RebuildAllConditionEditors();
                    sharedConditionEditors = BuildSharedConditionEditors(selectedTransitions);
                }
                else if (showSharedConditions)
                {
                    foreach (ConditionMultiEditor editor in sharedConditionEditors)
                    {
                        editor.Invert();
                    }

                    RebuildAllConditionEditors();
                }
                else
                {
                    foreach (ConditionMultiEditor editor in allConditionEditors)
                    {
                        editor.Invert();
                    }

                    RefreshSharedConditions();
                }
            }

            EditorGUI.EndDisabledGroup();

            rect.x += rect.width - 52f;
            rect.width = 15f;

            // Also force-enabled: a preference, not an operation on the selection.
            using (new GUIDisabledScope(disabled: false))
            {
                if (EditorUtils.Button(rect,
                        new GUIContent(EditorUtils.contents.settings) { tooltip = "Toggles custom matching options" },
                        GUIStyle.none))
                {
                    EditorSettings.Instance.showMatchingOptions.Toggle();
                }
            }

            rect.x += 17f;
            using (new EditorGUI.DisabledScope(focusedTransition.transition))
            {
                if (EditorUtils.Button(rect, EditorUtils.contents.merge, GUIStyle.none))
                {
                    MergeTransitions();
                }
            }

            rect.x += 17f;
            if (EditorUtils.Button(rect, EditorUtils.contents.separate, GUIStyle.none))
            {
                SplitTransitions();
            }
        }

        #endregion
    }
}
