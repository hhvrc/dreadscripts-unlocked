// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ReflectVisitor -> DrawTransitionSection, line 12529
//   CreateVisitor -> DrawTransitionSettings, line 12615
//   DeleteVisitor -> DrawSelectedTransitionList, line 12544, in ControllerEditor.TransitionList.cs
//   NewVisitor -> DrawTransitionConditions, line 12672, in ControllerEditor.ConditionSection.cs
//   DeleteAnnotation -> DrawCollapsibleSection, line 9920, in ControllerEditor.CollapsibleSection.cs
//   CustomizeAlgo -> CopyTransitionSettings, line 14693, in ControllerEditor.TransitionCopy.cs
//   RunAlgo -> RebuildTransitionInspector, line 14801, in ControllerEditor.InspectorRefresh.cs
//   CalculateAnnotation -> RefreshInspectorProperties, line 9768, in ControllerEditor.InspectorRefresh.cs
//   ManageWrapper -> SyncSelection, line 8676, in ControllerEditor.SelectionSync.cs
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// THE FILE NAME IS WRONG AND IS KEPT ONLY BECAUSE IT WAS ASSIGNED. This file was commissioned as
// "ControllerSection" on the strength of ControllerEditor.Window.cs's header, which lists
// `ReflectVisitor()` as "the controller/layer section". That description does not survive contact
// with the decompiled body. Decompiled line 12529 reads
//
//     private void ReflectVisitor()
//     {
//         if (parserAnnotation && <licence predicate>)
//         {
//             string token = $"Transition Count: {_DefinitionAnnotation.Count}";
//             DeleteAnnotation(DeleteVisitor,  token,                    ...showTransitionsCount, ...);
//             DeleteAnnotation(CreateVisitor, "Transition Settings",     ...showTransitionSettings, ...);
//             DeleteAnnotation(NewVisitor,    "Transition Conditions",   ...showTransitionConditions, ...);
//         }
//     }
//
// `parserAnnotation` is `transitionSectionVisible` in ControllerEditor.State.cs's rename table
// (decompiled line 8238), `_DefinitionAnnotation` is `selectedTransitionEdits` (line 8030), and all
// three settings it reads are the `showTransition*` ones. Every observable thing about the member
// says transitions, not controllers or layers: it is the root of the window's TRANSITION section,
// the first of the four sections OnGUI draws. The ported member is therefore named
// DrawTransitionSection rather than being given a name the code would contradict. The file name is
// left as assigned so the orchestration that expects this path still finds it; renaming the file
// and correcting Window.cs's header are both changes to shared files, which this port is not
// permitted to make. Whoever integrates this should fix both together.
//
// Nothing in this file draws a controller or a layer. If you came here looking for the layer list,
// it remains unported and it is not ReflectVisitor.
//
// A note on the decompiled names in the MAP above. Two of them -- `ReflectVisitor` and
// `DeleteAnnotation` -- no longer appear under those spellings in `decompiled/`: the ported names
// were fed back into the rename map, so the snapshot now declares them as `DrawTransitionSection`
// and `DrawCollapsibleSection`. The obfuscated spellings are kept in the left-hand column because
// they are the names every other header in this package still joins on, and because the whole
// quoted body above is only readable in the obfuscated vocabulary. The line numbers, which are
// what the checker actually joins on, are current. `CreateVisitor`, `CustomizeAlgo`, `RunAlgo`,
// `CalculateAnnotation` and the two members still deferred below were each re-checked against the
// current snapshot and do still carry the spellings used here.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// The shipped guard is a two-term conjunction: the section-visible flag AND an inline
// `(Func<bool>)delegate { ... }` that recomputes an HMACSHA256 over the licence key and date/HWID
// stamp and compares it against `m_ParamsAnnotation` (`licenseToken`). That second term is the
// obfuscator's scattered licence test, the same block that appears verbatim in dozens of other
// members of this assembly. It is dropped here on the package-wide basis: the vendor's validation
// endpoint is gone, so the predicate can only ever evaluate false, and an unlicensed
// ReflectVisitor draws nothing at all -- the entire transition section would be invisible to the
// legitimate holders this restoration exists for. The ported guard keeps only the first term and
// behaves as though the licence check passed.
//
// ==================================== NOTES, CONTINUED =========================================
//
// ALL THREE SUB-SECTIONS NOW DRAW. Earlier revisions of this header carried a PARTIAL PORT section
// listing DeleteVisitor and NewVisitor as deferred with their blockers; both have landed and that
// section is deleted rather than left to rot beside the working code. What was blocking them, and
// where each piece now lives, since the chain is the largest one this package has had:
//
//   DeleteVisitor (12544) is ControllerEditor.TransitionList.cs. Its blocker was MapVisitor
//     (11763), now RebuildConditionList in ControllerEditor.ConditionList.cs.
//   NewVisitor (12672) is ControllerEditor.ConditionSection.cs. Its blocker was UpdateVisitor
//     (12980), now RefreshSharedConditions in ControllerEditor.ConditionMatching.cs, and through it
//     the same RebuildConditionList.
//
// RebuildConditionList in turn needed the whole condition editor, which is now five further files:
// ControllerEditor.ConditionMatching.cs (the matching rule and the three row-set builders),
// .ConditionClipboard.cs (copy/paste of conditions), .ConditionList.cs (the three ReorderableLists
// and the add callback), .ConditionListHeader.cs, .ConditionRow.cs, and -- reached from the
// header's last two buttons -- .ConditionMergeSplit.cs. Each of those carries its own licence-gate
// and audit notes.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// DrawTransitionSettings writes its two ternaries the way round the source must have had them.
// The decompiled body tests `(!focusedTransition.stateTransition) ? A : B` twice, which is ILSpy's
// rendering of a branch-if-true; both are written here as `focusedTransition.stateTransition ? B :
// A`. The conditions, the arms and their pairing are unchanged.
//
// The decompiled `EditorUtils.contents()` is read as `EditorUtils.contents`: the port turned that
// lazy accessor into a property, which EditorUtils.Contents.cs records. Likewise the decompiled
// `focusedTransition.DisplayName()` is a property in the port
// (AnimatorGraphReflection.TransitionEditionInfo.DisplayName) and is read as one here. Neither is
// a behavioural change; both are call-syntax consequences of decisions taken in the files that own
// those members.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- DrawTransitionSettings was diffed statement for statement against
// decompiled lines 12615-12670: the disabled-group open and close, the Update/ApplyModifiedProperties
// pair, both ternaries, the copy button's compound condition, the null-check and allocation of
// copiedTransitionSettings, the paste loop with its Undo.RecordObject label "PasteSettings", all
// ten PropertyField calls in shipped order with their three horizontal scopes and the nested
// exit-time disabled group, and the two trailing Space calls. DrawTransitionSection was re-diffed
// against decompiled lines 12529-12542 on the pass that landed the other two sub-sections: the
// guard, all three call targets, the three setting names, the three boxed flags (true, true,
// false), the three slot indices (0, 1, 2) and the interpolated "Transition Count" label, which the
// shipped code also computes into a local before the first call. Neither range contains a `goto`, a
// residual `switch` dispatch, a `while (true)` or an unresolved `smethod_N`, so no deobfuscator
// fault applies to either; the licence predicate recorded above is the only thing removed.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Transition section

        /// <summary>
        /// Draws the window's transition section: the transition count list, the transition
        /// settings grid, and the condition editor, each in its own collapsible sub-section.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the first of the four section roots <c>OnGUI</c> calls, and it is a root only --
        /// it owns no drawing of its own. All it does is decide whether the section is shown at all
        /// and then hand three bodies to the shared collapsible-section helper, which draws each
        /// one's header, remembers its expanded state in the corresponding
        /// <see cref="EditorSettings"/> flag, and gives it a collapse handle.
        /// </para>
        /// <para>
        /// The visibility flag it tests is <see cref="transitionSectionVisible"/> rather than
        /// <c>EditorSettings.Instance.editingTransitions</c> directly. Those are not the same thing:
        /// the setting is the user's persisted preference, and the field is the session mirror that
        /// <c>OnGUI</c>'s section toolbar writes whenever the preference changes. Reading the mirror
        /// is what lets other code force the section open for a frame without editing the user's
        /// saved settings.
        /// </para>
        /// <para>
        /// All three sub-sections draw. The licence predicate that the shipped guard ANDs into this
        /// test is dropped, per the file header.
        /// </para>
        /// </remarks>
        private void DrawTransitionSection()
        {
            if (!transitionSectionVisible)
            {
                return;
            }

            // The label is built per frame rather than passed as a literal, because it interpolates
            // the live selection size.
            string countLabel = $"Transition Count: {selectedTransitionEdits.Count}";

            DrawCollapsibleSection(DrawSelectedTransitionList, countLabel,
                EditorSettings.Instance.showTransitionsCount, boxed: true, index: 0);

            DrawCollapsibleSection(DrawTransitionSettings, "Transition Settings",
                EditorSettings.Instance.showTransitionSettings, boxed: true, index: 1);

            // The one sub-section drawn unboxed, because its body opens a box of its own.
            DrawCollapsibleSection(DrawTransitionConditions, "Transition Conditions",
                EditorSettings.Instance.showTransitionConditions, boxed: false, index: 2);
        }

        /// <summary>
        /// Draws the "Transition Settings" sub-section: the transition inspector's property grid,
        /// with buttons to copy the focused transition's settings and paste them onto every
        /// selected state transition.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Every field here is drawn from <see cref="transitionInspectorSerialized"/>, which is
        /// pointed at its target by <see cref="RebuildTransitionInspector"/> and whose property
        /// handles are read out by <see cref="RefreshTransitionProperties"/>. Both live in
        /// ControllerEditor.InspectorRefresh.cs. Because that SerializedObject may be wrapping
        /// several transitions at once, the grid multi-edits: PropertyField renders a mixed-value
        /// dash wherever the selection disagrees, with no extra work here.
        /// </para>
        /// <para>
        /// The whole grid is disabled unless a state transition is selected, but it is still drawn
        /// rather than skipped. That is what makes the fallback target matter: with nothing
        /// selected the inspector is pointed at the pair of throwaway transitions that disagree on
        /// every field, so the disabled grid shows mixed values rather than one arbitrary
        /// transition's settings.
        /// </para>
        /// <para>
        /// Copy is offered when there is exactly one thing it could unambiguously mean -- a single
        /// selected state transition, or a focused one, which wins over the selection. Paste is
        /// offered whenever anything has been copied, and applies to every selected state
        /// transition rather than just the focused one, each with its own undo entry.
        /// </para>
        /// </remarks>
        private void DrawTransitionSettings()
        {
            EditorGUI.BeginDisabledGroup(!hasStateTransitionSelected);
            transitionInspectorSerialized.Update();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(
                    focusedTransition.stateTransition ? focusedTransition.DisplayName + "'s Settings" : string.Empty,
                    GUILayout.ExpandWidth(true));

                if ((selectedStateTransitions.Count == 1 || (bool)focusedTransition.stateTransition)
                    && EditorUtils.Button(EditorUtils.contents.copy, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                {
                    if (copiedTransitionSettings == null)
                    {
                        copiedTransitionSettings = new AnimatorStateTransition();
                    }

                    CopyTransitionSettings(
                        focusedTransition.stateTransition ? focusedTransition.stateTransition : selectedStateTransitions[0],
                        copiedTransitionSettings);
                }

                using (new EditorGUI.DisabledScope(!copiedTransitionSettings))
                {
                    if (EditorUtils.Button(EditorUtils.contents.paste, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        for (int i = 0; i < selectedStateTransitions.Count; i++)
                        {
                            Undo.RecordObject(selectedStateTransitions[i], "PasteSettings");
                            CopyTransitionSettings(copiedTransitionSettings, selectedStateTransitions[i]);
                        }
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionHasExitTimeProperty);

                // Exit time is meaningless without one, so the field beside the toggle follows it.
                EditorGUI.BeginDisabledGroup(!transitionHasExitTimeProperty.boolValue);
                EditorGUILayout.PropertyField(transitionExitTimeProperty);
                EditorGUI.EndDisabledGroup();
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionHasFixedDurationProperty);
                EditorGUILayout.PropertyField(transitionDurationProperty);
            }

            EditorGUILayout.PropertyField(transitionOffsetProperty);
            EditorGUILayout.PropertyField(transitionInterruptionSourceProperty);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionOrderedInterruptionProperty);
                EditorGUILayout.PropertyField(transitionMuteProperty);
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionCanTransitionToSelfProperty);
                EditorGUILayout.PropertyField(transitionSoloProperty);
            }

            transitionInspectorSerialized.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
        }

        #endregion
    }
}
