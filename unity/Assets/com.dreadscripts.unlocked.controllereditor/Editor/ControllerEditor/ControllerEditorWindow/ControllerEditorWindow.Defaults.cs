// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ControllerEditorWindow.RunTests     -> DrawDefaultsTab,        line 3610
//   ControllerEditorWindow.CloneTests   -> DrawTransitionDefaults, line 3628
//   ControllerEditorWindow.LoginTests   -> DrawStateDefaults,      line 3690
//   ControllerEditorWindow.ReflectTests -> DrawOtherDefaults,      line 3794
//   ControllerEditorWindow.NewTests     -> DrawNodeColorField,     line 3909
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference. See ControllerEditorWindow.cs for the full member map.
//
// DrawNodeColorField belongs to the cosmetics tab rather than to this one; it lives here because it
// is a leaf helper and putting it beside the six call sites in ControllerEditorWindow.Cosmetics.cs
// would have been the only reason to.
//
// This file used to record three omissions. One is now closed and two remain, and the reason the
// two remain has changed -- none of them is waiting on unported code any more:
//
//   * CLOSED: the "Generated Assets Path" folder row (decompiled lines 3834-3838). Its blocker,
//     EditorUtils.EnableRules, has landed as EditorUtils.FolderField (EditorUtils.FolderField.cs)
//     and is `internal`, so the row is restored at the end of DrawOtherDefaults with the shipped
//     write-back guard intact.
//   * STILL OMITTED: the transition copy/paste buttons (lines 3634-3649).
//   * STILL OMITTED: the "Sample From Active StateMachine" button (lines 3822-3829).
//
// Both remaining omissions are blocked on VISIBILITY, not on missing code. Every member they call
// -- CopyTransitionSettings, copiedTransitionSettings, ActiveStateMachine -- is now ported, but all
// three are `private static` on ControllerEditor while this window is a separate top-level type
// here; the shipped build had it nested inside ControllerEditor, which is what gave it access.
// Compiling the calls produces five CS0122 "inaccessible due to its protection level" errors with
// every name resolving, which is the evidence. Closing them needs those three members widened to
// `internal` in ControllerEditor's own files. Each call site below carries the detail.
//
// Audit status: VERIFIED -- all five members this file declares were diffed statement by statement
// against export ControllerEditor.cs on 2026-08-05: DrawDefaultsTab (3610), DrawTransitionDefaults
// (3628), DrawStateDefaults (3690), DrawOtherDefaults (3794) and DrawNodeColorField (3909). Two
// deliberate, behaviour-preserving rewrites are worth naming rather than leaving implicit: the
// switch in DrawDefaultsTab and the mirror / cycle-offset branches in DrawStateDefaults are written
// in ascending / positive-test order where the decompiler emitted them inverted, which changes
// nothing at runtime. The restored folder row was diffed against 3834-3838. The two omitted buttons
// were diffed too -- that is how their blockers were re-derived -- but they are not written here.

using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditorWindow
    {
        /// <summary>
        /// Draws the second tab: the template state and transition new ones are cloned from, plus
        /// the assorted defaults that belong to neither.
        /// </summary>
        private static void DrawDefaultsTab()
        {
            selectedDefaultsTab = GUILayout.Toolbar(selectedDefaultsTab, defaultsTabLabels, "toolbarbutton");
            EditorUtils.Separator();

            switch (selectedDefaultsTab)
            {
                case 0:
                    DrawTransitionDefaults();
                    break;
                case 1:
                    DrawStateDefaults();
                    break;
                case 2:
                    DrawOtherDefaults();
                    break;
            }
        }

        /// <summary>
        /// The template <see cref="AnimatorStateTransition"/>, laid out the way Unity's own
        /// transition inspector lays it out.
        /// </summary>
        /// <remarks>
        /// Edited through a <see cref="SerializedObject"/> rather than through the settings
        /// framework because the template is a real Unity object, which buys undo support and the
        /// stock property drawers for free. The settings block only stores a reference to it, so a
        /// modification has to be pushed to <c>SettingsPersistence</c> explicitly -- hence the
        /// <c>hasModifiedProperties</c> read below, which must happen *before*
        /// <see cref="SerializedObject.ApplyModifiedProperties"/> clears the flag.
        /// </remarks>
        private static void DrawTransitionDefaults()
        {
            transitionObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                // DEFERRED: the copy and paste buttons that sat here (decompiled lines 3634-3649).
                // The blocker has changed and is now purely one of visibility, not of missing code.
                // Both members they need HAVE landed:
                //   ControllerEditor.CopyTransitionSettings   (line 14693, ControllerEditor.TransitionCopy.cs)
                //   ControllerEditor.copiedTransitionSettings (line 8040,  ControllerEditor.State.cs, line 627)
                // but both are declared `private static` on ControllerEditor, and this window is a
                // separate top-level type here where the shipped build had it nested inside
                // ControllerEditor -- which is exactly what gave it access. Verified by compiling
                // the call: four CS0122 "inaccessible due to its protection level" errors, one per
                // reference, with the names resolving correctly. Writing the buttons therefore needs
                // those two members widened to `internal`, an edit to files this port does not own.

                if (EditorUtils.Button(EditorUtils.contents.restoreDefaults, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f))
                    && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                {
                    EditorSettings.Instance.defaultTransition = new AnimatorStateTransition();
                    RebuildTransitionSerializedObject();
                    SettingsPersistence.Save();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionHasExitTime);
                using (new EditorGUI.DisabledScope(!transitionHasExitTime.boolValue))
                {
                    EditorGUILayout.PropertyField(transitionExitTime);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionHasFixedDuration);
                EditorGUILayout.PropertyField(transitionDuration);
            }

            EditorGUILayout.PropertyField(transitionOffset);
            EditorGUILayout.PropertyField(transitionInterruptionSource);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionOrderedInterruption);
                EditorGUILayout.PropertyField(transitionMute);
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(transitionCanTransitionToSelf);
                EditorGUILayout.PropertyField(transitionSolo);
            }

            bool hasModifiedProperties = transitionObject.hasModifiedProperties;
            transitionObject.ApplyModifiedProperties();
            if (hasModifiedProperties)
            {
                SettingsPersistence.Save();
            }
        }

        /// <summary>
        /// The template <see cref="AnimatorState"/>, laid out the way Unity's own state inspector
        /// lays it out.
        /// </summary>
        /// <remarks>
        /// Four of the state's fields -- speed, normalized time, mirror and cycle offset -- can each
        /// be driven either by a literal or by an animator parameter, and the serialised object
        /// carries both a value and a parameter name for each. The "Parameter" toggle on the right
        /// of each row picks which is live, and the row redraws as a parameter-name field when it is
        /// on. The disabled dropdown button beside those name fields is chrome only: Unity's own
        /// inspector offers a parameter picker there, and this reproduces its shape without its
        /// contents, which is why <see cref="emptyDropdownOptions"/> is empty.
        /// </remarks>
        private static void DrawStateDefaults()
        {
            stateObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(EditorUtils.contents.animatorStates, GUILayout.Width(35f), GUILayout.Height(35f));

                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.PropertyField(stateName, new GUIContent(string.Empty));

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUIUtility.labelWidth = 35f;
                        EditorGUILayout.PropertyField(stateTag);
                        EditorGUIUtility.labelWidth = 0f;

                        if (EditorUtils.Button(EditorUtils.contents.restoreDefaults, GUI.skin.label, GUILayout.Width(20f), GUILayout.Height(20f))
                            && EditorUtility.DisplayDialog("Restoring Default Settings", "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                        {
                            EditorSettings.Instance.defaultState = new AnimatorState
                            {
                                name = "New State"
                            };
                            RebuildStateSerializedObject();
                            SettingsPersistence.Save();
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(stateMotion);
            EditorGUILayout.PropertyField(stateSpeed);

            using (new GUILayout.HorizontalScope())
            {
                // The indent is applied and removed by hand because only the left-hand field is
                // meant to sit under it; the toggle on the right stays flush.
                EditorGUI.indentLevel++;
                using (new EditorGUI.DisabledScope(!stateSpeedParameterActive.boolValue))
                {
                    stateSpeedParameter.stringValue = EditorGUILayout.TextField("Multiplier", stateSpeedParameter.stringValue, "textfielddropdowntext");
                }
                EditorGUI.indentLevel--;

                using (new EditorGUI.DisabledScope(disabled: true))
                {
                    EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
                }

                stateSpeedParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateSpeedParameterActive.boolValue, GUILayout.Width(90f));
            }

            using (new GUILayout.HorizontalScope())
            {
                if (stateTimeParameterActive.boolValue)
                {
                    stateTimeParameter.stringValue = EditorGUILayout.TextField("Normalized Time", stateTimeParameter.stringValue, "textfielddropdowntext");
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
                    }
                }
                else
                {
                    // There is no literal to edit: normalized time only exists as a parameter, so
                    // the off state is a bare label rather than a field.
                    GUILayout.Label("Normalized Time");
                }

                stateTimeParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateTimeParameterActive.boolValue, GUILayout.Width(90f));
            }

            using (new GUILayout.HorizontalScope())
            {
                if (stateMirrorParameterActive.boolValue)
                {
                    stateMirrorParameter.stringValue = EditorGUILayout.TextField("Mirror", stateMirrorParameter.stringValue, "textfielddropdowntext");
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(stateMirror);
                }

                stateMirrorParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateMirrorParameterActive.boolValue, GUILayout.Width(90f));
            }

            using (new GUILayout.HorizontalScope())
            {
                if (stateCycleOffsetParameterActive.boolValue)
                {
                    stateCycleOffsetParameter.stringValue = EditorGUILayout.TextField("Cycle Offset", stateCycleOffsetParameter.stringValue, "textfielddropdowntext");
                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        EditorGUILayout.Popup(-1, emptyDropdownOptions, "textfielddropdown", GUILayout.Width(12f));
                    }
                }
                else
                {
                    stateCycleOffset.floatValue = EditorGUILayout.Slider("Cycle Offset", stateCycleOffset.floatValue, 0f, 1f);
                }

                stateCycleOffsetParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter", stateCycleOffsetParameterActive.boolValue, GUILayout.Width(90f));
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(stateWriteDefaults, new GUIContent("Write Defaults"));
                EditorGUILayout.PropertyField(stateIkOnFeet, new GUIContent("Foot IK"));
            }

            bool hasModifiedProperties = stateObject.hasModifiedProperties;
            stateObject.ApplyModifiedProperties();
            if (hasModifiedProperties)
            {
                SettingsPersistence.Save();
            }
        }

        /// <summary>
        /// The defaults that belong to neither template: which animator is targeted, and how a newly
        /// created layer's state machine is laid out.
        /// </summary>
        private static void DrawOtherDefaults()
        {
            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                // The shipped code routes this through the EditorUtils extension CountPredicate
                // (decompiled EditorUtils.cs line 3139), whose entire body is this ObjectField call.
                // It is inlined rather than added to the shared EditorUtils partials.
                targetAnimator = (Animator)EditorGUILayout.ObjectField(
                    new GUIContent("Targeted Animator", "The Animator that should be targeted by default when building Masks"),
                    targetAnimator,
                    typeof(Animator),
                    true);

                alwaysUseTargetAnimator = EditorUtils.ToggleButton(alwaysUseTargetAnimator, new GUIContent("Always Use"), null, GUILayout.Width(85f));
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                defaultLayerOptionsExpanded = EditorGUILayout.Foldout(defaultLayerOptionsExpanded, "Default Layer Options");
                if (defaultLayerOptionsExpanded)
                {
                    using (new IndentedLayoutScope())
                    {
                        EditorSettings.Instance.defaultLayerWeight.Value = EditorGUILayout.Slider("Default Layer Weight", EditorSettings.Instance.defaultLayerWeight.Value, 0f, 1f);
                        EditorSettings.Instance.defaultLayerMask.Draw("Default Layer Mask", false);

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorSettings.Instance.defaultEntryPosition.DrawVector2Field("Entry Position");
                        }

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorSettings.Instance.defaultAnyPosition.DrawVector2Field("AnyState Position");
                        }

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorSettings.Instance.defaultExitPosition.DrawVector2Field("Exit Position");
                        }

                        // DEFERRED: the "Sample From Active StateMachine" button (decompiled lines
                        // 3822-3829), which copies the three positions off whichever state machine
                        // the Animator window is currently showing. Same visibility blocker as the
                        // copy/paste buttons above, and no longer a missing-code one: the accessor
                        // has landed as ControllerEditor.ActiveStateMachine (line 8552,
                        // ControllerEditor.ControllerContext.cs, line 226) but is `private static`
                        // on ControllerEditor. Verified by compiling the reference: CS0122, name
                        // resolved. Needs widening to `internal` in a file this port does not own.
                    }
                }
            }

            // Where the tool writes anything it generates. The setting is passed by its implicit
            // string conversion and written back explicitly, because the field is drawn by a helper
            // that deliberately owns no storage -- see EditorUtils.FolderField.
            string pickedFolder = EditorUtils.FolderField(EditorSettings.Instance.saveFolder, "Generated Assets Path");
            if (!string.IsNullOrEmpty(pickedFolder))
            {
                EditorSettings.Instance.saveFolder.Value = pickedFolder;
            }
        }

        /// <summary>
        /// One row of the node-colour table: a popup over Unity's node palette, plus a revert button.
        /// </summary>
        /// <remarks>
        /// The double cast is what bridges the stored float to the enum in both directions; see
        /// <see cref="NodeColor"/> for why the setting is a float in the first place.
        /// </remarks>
        internal static void DrawNodeColorField(FloatSetting setting, string label)
        {
            using (new GUILayout.HorizontalScope())
            {
                setting.Value = (float)(NodeColor)EditorGUILayout.EnumPopup(label, (NodeColor)setting.Value);

                if (EditorUtils.Button(EditorUtils.contents.reset, EditorUtils.styles.tightLabel, GUILayout.Width(18f), GUILayout.Height(18f)))
                {
                    setting.Reset();
                }
            }
        }
    }
}
