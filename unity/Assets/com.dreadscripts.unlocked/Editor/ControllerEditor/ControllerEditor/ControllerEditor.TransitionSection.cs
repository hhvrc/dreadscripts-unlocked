// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ReflectVisitor -> DrawTransitionSection, line 12529
//   CreateVisitor -> DrawTransitionSettings, line 12615
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
// it is not ported yet and it is not ReflectVisitor.
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
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ===============================
//
// The guard is ported for real, and so is the second of the three statements it guards. The other
// two are not, and nothing stands in for them: this file declares no empty helper, no placeholder
// Action and no substitute drawing code.
//
// LANDED since the previous pass, and no longer blockers:
//
//   DeleteAnnotation (9920), the collapsible-section helper all three statements call, is ported as
//     DrawCollapsibleSection in ControllerEditor.CollapsibleSection.cs.
//   CustomizeAlgo (14693), the transition-settings copier, is ported as CopyTransitionSettings in
//     ControllerEditor.TransitionCopy.cs. That was CreateVisitor's only blocker, which is why the
//     "Transition Settings" body below is real code rather than another deferral.
//   RunAlgo (14801) and CalculateAnnotation (9768) are ported as RebuildTransitionInspector and
//     RefreshInspectorProperties in ControllerEditor.InspectorRefresh.cs. Between them they are the
//     only writers of `transitionInspectorSerialized` and of the ten `transition*Property` handles
//     that DrawTransitionSettings reads, so without them that body would have compiled and then
//     dereferenced nulls on its first frame.
//   ManageWrapper (8676) is ported as SyncSelection in ControllerEditor.SelectionSync.cs. An
//     earlier revision of this header listed it as a DeleteVisitor blocker; it has not been one
//     since that file landed.
//
// Still missing, with what each one now actually needs:
//
//   DeleteVisitor (decompiled 12544) -- the body of the first section, the "Transition Count"
//     list. Draws one clickable row per entry of `selectedTransitionEdits` in three columns, with a
//     deselect button per row and one for the whole set. Three of its four refresh calls are ported
//     now; the remaining blocker is MapVisitor (11763) alone. MapVisitor rebuilds whichever of the
//     three condition ReorderableLists is current, and it cannot be ported as a leaf: it names
//     TestVisitor (11510), CalculateVisitor (11345) and FillVisitor (12814) as the list's element,
//     header and add callbacks, and PrepareVisitor (12951) to build the focused transition's rows.
//     Those are ~420 lines of the condition editor, and CalculateVisitor alone pulls in
//     SortVisitor, RegisterVisitor, ChangeVisitor, ResolveAlgo, ConnectAlgo, ViewAlgo, AssetVisitor
//     and the ConditionMultiEditor mutators. That is a region of its own, not a helper this file
//     should absorb, so DeleteVisitor stays deferred rather than being written against a MapVisitor
//     that does not exist.
//
//   NewVisitor (decompiled 12672) -- the body of the third section, "Transition Conditions": the
//     match-parameter/mode/value toggles and whichever of the three condition ReorderableLists is
//     current, with up/down arrow handling that walks focus between the "Threshold<n>" controls.
//     Blocked on UpdateVisitor (decompiled 12980), which its change check calls; the package has
//     that member only as the assignable seam `EditorSettings.onMatchingOptionsChanged`, and
//     calling the seam instead of the method would invert the direction of the dependency -- the
//     seam exists so the settings can notify the window, not so the window can notify itself.
//     UpdateVisitor is two statements, `sharedConditionEditors = AssetVisitor(selectedTransitions)`
//     followed by `MapVisitor()`, so it reduces to the same MapVisitor chain as DeleteVisitor plus
//     AssetVisitor (12961), which calls CheckVisitor (12924), which calls WriteVisitor (12859).
//     Beyond that, the body itself dereferences whichever ReorderableList MapVisitor would have
//     built, so porting it ahead of MapVisitor would produce a method that draws nothing but a
//     null reference.
//
// The two missing statements are left as inline DEFERRED comments at their shipped positions, the
// same convention ControllerEditor.Window.cs and ControllerEditor.SelectionSync.cs use. No GUI
// scope is opened or closed around them, so omitting them cannot leave Unity's layout stack
// unbalanced -- each of the three shipped statements is a self-contained call.
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
// Audit status: PARTIAL -- DrawTransitionSettings was diffed statement for statement against
// decompiled lines 12615-12670: the disabled-group open and close, the Update/ApplyModifiedProperties
// pair, both ternaries, the copy button's compound condition, the null-check and allocation of
// copiedTransitionSettings, the paste loop with its Undo.RecordObject label "PasteSettings", all
// ten PropertyField calls in shipped order with their three horizontal scopes and the nested
// exit-time disabled group, and the two trailing Space calls. That range contains no `goto`, no
// residual `switch` dispatch, no `while (true)` and no unresolved `smethod_N`, so no deobfuscator
// fault applies to it. DrawTransitionSection's guard, its three call targets, the three setting
// names, the three slot indices (0, 1, 2) and the interpolated "Transition Count" label were
// transcribed from decompiled lines 12529-12542 and re-checked on this pass. The file is PARTIAL
// rather than VERIFIED because two of the three shipped statements are still absent: the bodies of
// DeleteVisitor (12544) and NewVisitor (12672) were read only far enough to describe them and their
// blockers above, and were not diffed statement by statement.

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
        /// Only the middle of the three sub-sections is drawn. The first and third are marked
        /// DEFERRED at their shipped positions below; see the file header for what each one still
        /// needs. The licence predicate that the shipped guard ANDs into this test is dropped, also
        /// per the file header.
        /// </para>
        /// </remarks>
        private void DrawTransitionSection()
        {
            if (!transitionSectionVisible)
            {
                return;
            }

            // DEFERRED, slot 0 -- DrawCollapsibleSection(DrawSelectedTransitionList,
            //     $"Transition Count: {selectedTransitionEdits.Count}",
            //     EditorSettings.Instance.showTransitionsCount, boxed: true, index: 0).
            // The label is the one the shipped code builds per frame rather than passing as a
            // literal, because it interpolates the live selection size. It is not computed here:
            // with the body absent there is nothing to pass it to, and a local nothing reads is
            // just an unused variable. DrawSelectedTransitionList is decompiled DeleteVisitor
            // (12544), blocked on MapVisitor (11763).

            DrawCollapsibleSection(DrawTransitionSettings, "Transition Settings",
                EditorSettings.Instance.showTransitionSettings, boxed: true, index: 1);

            // DEFERRED, slot 2 -- DrawCollapsibleSection(DrawTransitionConditions,
            //     "Transition Conditions", EditorSettings.Instance.showTransitionConditions,
            //     boxed: false, index: 2).
            // This is the one sub-section drawn unboxed, because its body opens a box of its own.
            // DrawTransitionConditions is decompiled NewVisitor (12672), blocked on UpdateVisitor
            // (12980) and through it on the same MapVisitor chain.
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
