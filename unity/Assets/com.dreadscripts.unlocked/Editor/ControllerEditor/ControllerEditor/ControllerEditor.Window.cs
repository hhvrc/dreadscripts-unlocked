// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The window half of the ControllerEditor type: its EditorWindow message handlers and the menu item
// that opens it. Every other partial in this folder holds statics; these are the only instance
// members the shipped type has, which is why ControllerEditor.State.cs had to stop declaring the
// type `static` before this file could exist. See that file's header.
//
//   InterruptWrapper -> ShowWindow,  line 8578
//   OnGUI            -> OnGUI,       line 8583
//   OnFocus          -> NOT PORTED, line 8831 -- its whole body is one call to ManageWrapper (8676),
//     the selection-sync routine, which is itself unported and licence gated; see PARTIAL PORT.
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
// Nothing below is stubbed. These are the calls the shipped bodies make that this port omits
// because their targets do not exist in the package yet. All of them live in the still-unported
// body regions of the god class; none is blocked on anything subtle.
//
//   OnGUI, after the header block:
//     LogoutMapper()       -- "is a controller loaded", gates the disabled group. See DEVIATION.
//     RevertMapper()       -- the active state machine, whose name the header row labels itself
//                             with; "No Active Machine" when null.
//     ManageMapper()       -- "is a VRC avatar loaded", gates the exit-transition label strip.
//     ReflectVisitor()     -- the controller/layer section.
//     DestroyVisitor()     -- the state section.
//     ValidateVisitor()    -- the transition section.
//     DefineVisitor()      -- the parameter section.
//     IncludeAnnotation()  -- the animated show/hide wrapper the four sections are nested in.
//     HarmonyPatchManager.LoginReg() -- the deferred-patch pump, run once per repaint.
//
//   OnEnable:
//     PrintWrapper       (8836) -- the Undo.undoRedoPerformed handler; needs UpdateVisitor.
//     SortAlgo                  -- the playmodeStateChanged handler.
//     CalculateAnnotation, MapVisitor, CancelAnnotation -- the three first-open initialisers.
//
//   OnFocus is not ported at all rather than ported empty: its entire body is one call to
//   ManageWrapper (8676), the selection-sync routine, which is both unported and itself licence
//   gated. An empty OnFocus would be a fabrication, not a partial port.
//
//   OnDisable's Undo.undoRedoPerformed unsubscribe is omitted, because OnEnable's matching
//   subscribe is: both name PrintWrapper. Porting one half would leave the delegate unbalanced.
//
// ================================ DELIBERATE DEVIATION ========================================
//
// The shipped OnGUI wraps its header block in EditorGUI.BeginDisabledGroup(!LogoutMapper()), ends
// that group midway through the header row so the manual and settings buttons stay live, and opens
// a second one that runs to the end of the block. With LogoutMapper unported there is no condition
// to pass, and an unbalanced Begin/End pair corrupts the whole inspector's GUI stack for the rest
// of the frame -- so both groups are omitted rather than passed a placeholder. The visible effect
// is that the title bar reads as enabled when no controller is loaded, where the shipped tool greys
// it out. Restoring this is a two-line change once LogoutMapper lands, and the call sites are
// marked below.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- the five MAP entries were re-checked against decompiled/ControllerEditor/
// DreadScripts/ControllerEditor/ControllerEditor.cs and each lands on the member named (8577 was the
// [MenuItem] attribute line, corrected to 8578; ManageWrapper corrected from 8672 to 8676). The
// bodies were not re-diffed statement by statement, which is why this is PARTIAL rather than VERIFIED.

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
        /// Draws the window: the section toolbar, the title bar, and -- once they are ported -- the
        /// four editor sections.
        /// </summary>
        /// <remarks>
        /// See the file header. The licence gate this body opened with is dropped, the two disabled
        /// groups around the title bar are omitted as a documented deviation, and the section calls
        /// after the separator are deferred on unported targets.
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

                // DEFERRED: EditorGUI.BeginDisabledGroup(!LogoutMapper()) -- see DEVIATION above.
                using (new GUILayout.VerticalScope(EditorUtils.styles.bigTitleBackground))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(18f);

                        // DEFERRED: the active state machine's name, or "No Active Machine".
                        //           Needs RevertMapper (the active machine accessor).
                        GUILayout.Label(string.Empty, EditorUtils.styles.centeredMiniLabel, GUILayout.ExpandWidth(expand: true));

                        // DEFERRED: EditorGUI.EndDisabledGroup() -- pairs with the group above.
                        if (EditorUtils.Button(EditorUtils.contents.inspectorWindow, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f))
                            && EditorUtility.DisplayDialog("Instructions", "Open Controller Editor's Online Manual?", "Open", "Cancel"))
                        {
                            Application.OpenURL("https://notes.sleightly.dev/ceditor");
                        }

                        if (EditorUtils.Button(EditorUtils.contents.settings, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f)))
                        {
                            ControllerEditorWindow.ShowWindow();
                        }

                        // DEFERRED: EditorGUI.BeginDisabledGroup(!LogoutMapper()) -- second group.
                    }

                    // DEFERRED: the exit-transition label strip, drawn when a VRC avatar is loaded
                    //           and exitTransitionNames is non-empty. Needs ManageMapper.
                }

                // DEFERRED: EditorGUI.EndDisabledGroup() -- pairs with the second group.
                EditorUtils.Separator();

                // DEFERRED, in shipped order: ReflectVisitor(), IncludeAnnotation(...),
                // DestroyVisitor(), IncludeAnnotation(...), ValidateVisitor(), DefineVisitor(),
                // HarmonyPatchManager.LoginReg(). See the file header for what each one draws.
            }
        }

        /// <summary>
        /// Clears the three transition-editing modes when the window is closed or reloaded.
        /// </summary>
        /// <remarks>
        /// These three are session state, not settings: a mode armed by a button in the transition
        /// section must not survive the window going away, or the next open would start mid-gesture.
        /// The shipped body also unsubscribes <c>PrintWrapper</c> from
        /// <see cref="Undo.undoRedoPerformed"/>; that half is omitted here because
        /// <see cref="OnEnable"/>'s matching subscribe is. See the file header.
        /// </remarks>
        private void OnDisable()
        {
            makeMultipleTransitionsMode = false;
            redirectTransitionsMode = false;
            replicateTransitionsMode = false;
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
        /// See the file header for the Undo and play-mode subscriptions and the three initialisers
        /// this body also makes, all of which are deferred on unported targets.
        /// </para>
        /// </remarks>
        private void OnEnable()
        {
            activeWindow = this;

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
        }

        #endregion
    }
}
