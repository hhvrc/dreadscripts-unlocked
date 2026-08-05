// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The window half of the ControllerEditor type: its EditorWindow message handlers and the menu item
// that opens it. Every other partial in this folder holds statics; these are the only instance
// members the shipped type has, which is why ControllerEditor.State.cs had to stop declaring the
// type `static` before this file could exist. See that file's header.
//
//   InterruptWrapper -> ShowWindow,  line 8578
//   OnGUI            -> OnGUI,       line 8583
//   OnFocus          -> OnFocus,     line 8831
//   PrintWrapper     -> OnUndoRedo,  line 8836
//   OnDisable        -> OnDisable,   line 8842
//   OnEnable         -> OnEnable,    line 8857
// ShowWindow is the [MenuItem] entry point; the attribute sits one line above the declaration, at
// decompiled 8577.
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// ================================ LICENCE GATE, NOT PORTED ====================================
//
// The shipped OnGUI opens `if (!OrderVisitor(this)) return;`. OrderVisitor is this tool's licence
// gate, the ControllerEditor counterpart of ADOverhaul's FlushConfiguration: when unlicensed it
// draws the activation UI over the whole window and returns false. It is dropped on the same basis
// as every other licence wall in this package -- the vendor's validation endpoint is gone, so the
// gate can only ever fail, and it would lock out precisely the legitimate holders this restoration
// exists for. The window here behaves as fully licensed and draws its contents unconditionally.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ==============================
//
// Nothing below is stubbed. Three calls the shipped bodies make are still omitted because their
// targets do not exist in the package yet.
//
//   OnGUI, after the section run:
//     DefineVisitor() (10978) -- the parameter section, the fourth and last thing OnGUI draws. It
//       is unported: no member of the package declares it under any name, and it is the root of a
//       region (the parameter list, its add/remove/rename gestures and its own reorderable list)
//       that has not been started.
//     HarmonyPatchManager.LoginReg() (HarmonyPatchManager.cs 2716) -- the patch-failure banner,
//       drawn into the tool's toolbar once per repaint. It is deferred in
//       Editor/ControllerEditor/HarmonyPatchManager/HarmonyPatchManager.cs, which owns it. Note
//       that the blocker that file records for it -- EditorUtils.CountRules, the delay-call
//       marshal -- has since landed as EditorUtils.DelayCall in EditorUtils.Callbacks.cs, so
//       LoginReg looks unblocked now; that is a call for HarmonyPatchManager's owner to make, not
//       one to make from here, and it is reported rather than acted on.
//
//   OnEnable:
//     CancelAnnotation (9296) -- the first-open build of the parameter-driver ReorderableList. Its
//       three callbacks, CountAnnotation (9304), DisableAnnotation and InsertAnnotation, are all
//       unported, so the list cannot be constructed as shipped. Its two siblings in that same
//       three-call tail have landed and are written out below.
//
//   The rest of OnGUI's body has since landed and is no longer deferred. LogoutMapper,
//   ManageMapper and RevertMapper are ported as the ActiveController / RootStateMachine /
//   ActiveStateMachine properties in ControllerEditor.ControllerContext.cs, so the two disabled
//   groups, the machine-name label and the exit-transition strip are all restored here.
//   ReflectVisitor, DestroyVisitor and ValidateVisitor are ported as DrawTransitionSection,
//   DrawStateSection and DrawControllerSection, and IncludeAnnotation as SeparatorIf; those three
//   Draw calls are themselves partial, and each names its own blockers in its own file.
//
//   Note the shipped names do NOT say what they draw: ReflectVisitor (12529) draws the TRANSITION
//   section -- its three rows are Transition Count, Transition Settings, Transition Conditions --
//   and ValidateVisitor (11806) draws the CONTROLLER section, gated on editingController. The
//   ported names follow the behaviour, not the obfuscated name.
//
// ============================ FORMER DEFERRALS, NOW WITHDRAWN =================================
//
// Three of this file's four lifecycle members were partly or wholly deferred and no longer are:
//
//   OnFocus -- was not ported at all, on the grounds that its whole body is one call to
//     ManageWrapper (8676), "which is both unported and itself licence gated". That routine is
//     ported, as SyncSelection in ControllerEditor.SelectionSync.cs, with the licence gate dropped
//     under the package-wide rule. OnFocus is therefore written out, and is still one statement.
//
//   PrintWrapper -- the Undo.undoRedoPerformed handler, ported below as OnUndoRedo. Its recorded
//     blocker was UpdateVisitor, "which is itself unported: see ControllerEditor.Refresh.cs".
//     UpdateVisitor is ported, as RefreshSharedConditions in ControllerEditor.ConditionMatching.cs;
//     ControllerEditor.Refresh.cs's NOT PORTED narrative predates that and has been removed. With
//     the handler back, OnEnable's subscribe and OnDisable's unsubscribe go back in as a pair --
//     they were omitted as a pair, and porting one half would have left the delegate unbalanced.
//
//   OnEnable's three first-open initialisers -- CalculateAnnotation, MapVisitor, CancelAnnotation.
//     The first two are ported (RefreshInspectorProperties in ControllerEditor.InspectorRefresh.cs
//     and RebuildConditionList in ControllerEditor.ConditionList.cs) and are called below in the
//     shipped order. Only CancelAnnotation is still deferred; see above.
//
//   OnEnable's play-mode subscription. This header used to state, flatly, that ApplyGraphBackground
//     "is NOT subscribed here: it is a graph-background applier driven by a settings change, not the
//     playmodeStateChanged handler this header previously called it". That is wrong, and it was the
//     only claim in this file contradicted by export/ rather than merely gone stale. Decompiled
//     8862-8863 are
//
//         EditorApplication.playmodeStateChanged -= ApplyGraphBackground;
//         EditorApplication.playmodeStateChanged += ApplyGraphBackground;
//
//     -- the same remove-then-add shape as the Undo pair on the two lines above them, over the
//     member ControllerEditor.Refresh.cs ports as ApplyGraphBackground. Both lines are written out
//     below. It is a settings-change applier *and* a play-mode handler; those are not alternatives,
//     because entering play mode rebuilds Unity's editor styles and drops the background the
//     cosmetic settings had written into them.
//
//     `EditorApplication.playmodeStateChanged` is the pre-2019.3 lower-case-m `CallbackFunction`
//     field, superseded by `playModeStateChanged` (an `Action<PlayModeStateChange>`, which is what
//     ADOverhaul.SceneView.cs uses because its shipped build already did). It is deprecated but
//     still declared in the reference assemblies this package compiles against, which was checked by
//     compiling it rather than assumed, so the shipped lines are written literally with no adapter.
//     If a future Unity drops it, the honest replacement is the one-line adapter and a DELIBERATE
//     DEVIATION note -- not silently dropping the subscription, which is what the removed claim
//     above amounted to.
//
// ============================ FORMER DEVIATION, NOW WITHDRAWN =================================
//
// This header used to record that both of OnGUI's EditorGUI disabled groups were omitted, because
// LogoutMapper was unported and an unbalanced Begin/End pair corrupts Unity's GUI stack for the
// rest of the frame. LogoutMapper has since landed as the ActiveController property, so both groups
// are restored, in the shipped shape: the first opens before the title box and closes midway
// through the header row so the manual and settings buttons stay live with no controller loaded,
// and the second opens after them and closes past the exit-transition strip.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- the six MAP entries were re-checked against export/ControllerEditor/
// DreadScripts/ControllerEditor/ControllerEditor.cs and each lands on the member named (8577 was the
// [MenuItem] attribute line, corrected to 8578; ManageWrapper corrected from 8672 to 8676). On the
// pass that landed OnFocus and OnUndoRedo, decompiled 8831-8870 was diffed statement for statement:
// OnFocus's single call, OnUndoRedo's two, OnDisable's four statements including the unsubscribe,
// and OnEnable's whole body -- the two delegate pairs in their shipped order (Undo first, play mode
// second, each written remove-then-add), the mixed-value pair's null guard, and the three-call tail
// of which two are now written and the third is deferred. OnGUI's body was not re-diffed on this
// pass, which is why this stays PARTIAL rather than becoming VERIFIED.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Window lifecycle

        /// <summary>
        /// Opens the Controller Editor window, or focuses it if it is already open.
        /// </summary>
        /// <remarks>
        /// The leading space in the title is the shipped value, not a typo: it offsets the tab label
        /// from the icon that is assigned immediately after. The icon is Unity's built-in
        /// <c>d_EditCollider</c>, read through <see cref="EditorGUIUtility.IconContent(string)"/>
        /// rather than shipped as an asset.
        /// </remarks>
        [MenuItem("DreadTools/Controller Editor/Window %t", false, 200)]
        internal static void ShowWindow()
        {
            GetWindow<ControllerEditor>(utility: false, " Controller Editor", focus: true)
                .titleContent.image = EditorGUIUtility.IconContent("d_EditCollider").image;
        }

        /// <summary>
        /// Draws the window: the section toolbar, the title bar, and three of the four editor
        /// sections.
        /// </summary>
        /// <remarks>
        /// See the file header. The licence gate this body opened with is dropped; the two disabled
        /// groups around the title bar are drawn as shipped; and of the calls after the section run
        /// only the parameter section and the patch-failure banner are still deferred.
        /// </remarks>
        private void OnGUI()
        {
            Event current = Event.current;
            if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                && current.type == EventType.KeyDown)
            {
                GUI.FocusControl(null);
                Repaint();
                return;
            }

            thresholdControlCounter = 0;

            using (new ScrollViewScope(ref windowScroll))
            {
                using (new GUILayout.HorizontalScope())
                {
                    BoolSetting[] sections =
                    {
                        EditorSettings.Instance.editingTransitions,
                        EditorSettings.Instance.editingStates,
                        EditorSettings.Instance.editingController
                    };
                    string[] labels = { "Transitions", "States", "Controller" };

                    for (int i = 0; i < sections.Length; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        sections[i].value = EditorUtils.ToggleButton(sections[i], labels[i], EditorStyles.toolbarButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Only the first two mirror their setting into a session flag; the
                            // controller section reads its setting directly every frame.
                            switch (i)
                            {
                                case 0:
                                    transitionSectionVisible = EditorSettings.Instance.editingTransitions;
                                    break;
                                case 1:
                                    stateSectionVisible = EditorSettings.Instance.editingStates;
                                    break;
                            }
                        }
                    }
                }

                EditorGUI.BeginDisabledGroup(!ActiveController);
                using (new GUILayout.VerticalScope(EditorUtils.styles.bigTitleBackground))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(18f);
                        GUILayout.Label(ActiveStateMachine ? ActiveStateMachine.name : "No Active Machine",
                            EditorUtils.styles.centeredMiniLabel, GUILayout.ExpandWidth(expand: true));

                        // The manual and settings buttons stay live with no controller loaded, which
                        // is why the shipped body closes the group here and opens a second one below.
                        EditorGUI.EndDisabledGroup();

                        if (EditorUtils.Button(EditorUtils.contents.inspectorWindow, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f))
                            && EditorUtility.DisplayDialog("Instructions", "Open Controller Editor's Online Manual?", "Open", "Cancel"))
                        {
                            Application.OpenURL("https://notes.sleightly.dev/ceditor");
                        }

                        if (EditorUtils.Button(EditorUtils.contents.settings, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)))
                        {
                            ControllerEditorWindow.ShowWindow();
                        }

                        EditorGUI.BeginDisabledGroup(!ActiveController);
                    }

                    if (RootStateMachine && exitTransitionNames.Length != 0)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            for (int j = 0; j < exitTransitionNames.Length; j++)
                            {
                                GUILayout.Label(exitTransitionNames[j], "AssetLabel");
                            }

                            GUILayout.FlexibleSpace();
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();
                EditorUtils.Separator();

                DrawTransitionSection();
                SeparatorIf(transitionSectionVisible && (stateSectionVisible || EditorSettings.Instance.editingController.value));
                DrawStateSection();
                SeparatorIf(stateSectionVisible && EditorSettings.Instance.editingController.value);
                DrawControllerSection();

                // DEFERRED, still: DefineVisitor() -- the parameter section -- and
                // HarmonyPatchManager.LoginReg(), the patch-failure banner. See the file header.
            }
        }

        /// <summary>
        /// Re-reads everything derived from the selection when the window is focused.
        /// </summary>
        /// <remarks>
        /// One statement, as shipped. Focus is the tool's cheapest "something may have changed while
        /// you were elsewhere" signal: the graph selection lives behind reflection and raises no
        /// event, so returning to this window is one of the two moments a full re-read is worth
        /// doing. The other is Unity's own <see cref="Selection.selectionChanged"/>, which the
        /// shipped code subscribes to the same routine.
        /// </remarks>
        private void OnFocus()
        {
            SyncSelection();
        }

        /// <summary>
        /// Rebuilds the shared condition rows after an undo or redo, and repaints.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An undo can rewrite the conditions of a transition that is still selected, which leaves
        /// the shared rows describing conditions that no longer exist while the selection itself is
        /// unchanged -- so <see cref="SyncSelection"/>, which diffs on the selection, would find
        /// nothing to do. That is why the undo handler goes straight to
        /// <see cref="RefreshSharedConditions"/> instead.
        /// </para>
        /// <para>
        /// It is an instance method because it repaints this window, which is also why the
        /// subscription is made in <see cref="OnEnable"/> rather than statically.
        /// </para>
        /// </remarks>
        private void OnUndoRedo()
        {
            RefreshSharedConditions();
            Repaint();
        }

        /// <summary>
        /// Clears the three transition-editing modes and drops the undo subscription when the
        /// window is closed or reloaded.
        /// </summary>
        /// <remarks>
        /// The three modes are session state, not settings: a mode armed by a button in the
        /// transition section must not survive the window going away, or the next open would start
        /// mid-gesture.
        /// </remarks>
        private void OnDisable()
        {
            makeMultipleTransitionsMode = false;
            redirectTransitionsMode = false;
            replicateTransitionsMode = false;

            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        /// Claims the singleton window slot and builds the mixed-value transition pair the
        /// multi-edit inspectors compare against.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The pair is two <see cref="AnimatorStateTransition"/> objects whose every serialised
        /// field is deliberately set to opposite values -- "a" takes each field's low value, "b" its
        /// high one. Wrapping both in one <see cref="SerializedObject"/> makes every property in it
        /// report <c>hasMultipleDifferentValues</c>, which is what lets the transition inspector
        /// draw a real mixed-value dash without inventing one. They are built once and kept for the
        /// lifetime of the domain; nothing ever writes to them.
        /// </para>
        /// <para>
        /// The transitions are plain <c>new</c> objects rather than assets, so they are never saved
        /// and leak nothing into the project. The shipped code does not destroy them either.
        /// </para>
        /// <para>
        /// The undo subscription is written as a remove followed by an add, which is what the
        /// shipped body does and is not redundant: a domain reload can run OnEnable against a
        /// delegate list that survived it, and removing a handler that is not subscribed is a no-op.
        /// </para>
        /// <para>
        /// See the file header for the play-mode subscription this body also makes, which cannot be
        /// written against a modern Unity, and for the one first-open initialiser still deferred.
        /// </para>
        /// </remarks>
        private void OnEnable()
        {
            activeWindow = this;

            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            // The graph background is a GUIStyle held in a static field of Unity's own graph code,
            // and entering or leaving play mode rebuilds those styles -- so the cosmetic background
            // has to be written back over the top afterwards or it reverts. Same remove-then-add
            // shape, for the same reason.
            EditorApplication.playmodeStateChanged -= ApplyGraphBackground;
            EditorApplication.playmodeStateChanged += ApplyGraphBackground;

            if (mixedValueTransitionPair == null)
            {
                mixedValueTransitionPair = new[]
                {
                    new AnimatorStateTransition
                    {
                        name = "a",
                        canTransitionToSelf = false,
                        duration = 0f,
                        exitTime = 0f,
                        hasExitTime = false,
                        hasFixedDuration = false,
                        interruptionSource = TransitionInterruptionSource.None,
                        mute = false,
                        offset = 0f,
                        orderedInterruption = false,
                        solo = false
                    },
                    new AnimatorStateTransition
                    {
                        name = "b",
                        canTransitionToSelf = true,
                        duration = 1f,
                        exitTime = 1f,
                        hasExitTime = true,
                        hasFixedDuration = true,
                        interruptionSource = TransitionInterruptionSource.Destination,
                        mute = true,
                        offset = 1f,
                        orderedInterruption = true,
                        solo = true
                    }
                };

                mixedValueTransitionSerialized = new SerializedObject(mixedValueTransitionPair);
            }

            transitionInspectorSerialized = mixedValueTransitionSerialized;

            // The first-open initialisers, in shipped order. The property banks are refreshed
            // against the SerializedObject assigned on the line above, then the condition list is
            // built so the condition editor has something to draw on the very first frame.
            RefreshInspectorProperties();
            RebuildConditionList();

            // DEFERRED: CancelAnnotation() -- the parameter-driver ReorderableList, the third of
            //           these three. See the file header for the callbacks it needs.
        }

        #endregion
    }
}
