// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ValidateVisitor -> DrawControllerSection, line 11806
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// ================================ LICENCE GATE, NOT PORTED ====================================
//
// Two statements into the box scope the shipped body evaluates the obfuscator's inline licence
// test -- the `(Func<bool>)delegate { ... HMACSHA256 ... m_ParamsAnnotation ==
// Convert.ToBase64String(...) ... }` shape that appears at dozens of sites across this assembly --
// and `return`s when it fails, leaving an empty grey box drawn. The gate is dropped on the same
// basis as every other licence wall in this package: the vendor's validation endpoint is gone, so
// the test can only ever fail, and it would lock out precisely the legitimate holders this
// restoration exists for. The body below runs as if the gate had passed.
//
// Note the ordering the shipped code chose: the `GUILayout.VerticalScope(GUI.skin.box)` is opened
// *before* the gate is evaluated, so an unlicensed build drew the section's frame and nothing
// inside it. That is why dropping the gate needs no scope surgery -- the scope was always balanced.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ==============================
//
// Nothing here is stubbed. These are the calls the shipped body makes that this port omits because
// their targets are not in the package yet.
//
//   LogoutVisitor()    (13048) -- the batch-action dispatcher, i.e. everything the Apply button
//     actually does. The button and the EditorGUI.BeginDisabledGroup/EndDisabledGroup pair that
//     computes its enabled state are omitted together; see DELIBERATE DEVIATION.
//
//   CustomizeVisitor() (11948) -- the last statement of the body, the row of toggles that opens the
//     "Write Defaults" audit panel and the "Explore Controller Sub-Assets" panel, plus those panels
//     themselves. It is an instance member of the window in its own right and belongs to a region
//     that is still outstanding; ControllerEditor.State.cs's header already lists it as such. It is
//     not called from here rather than being called and doing nothing.
//
// ================================ DELIBERATE DEVIATION ========================================
//
// 1. THE APPLY BUTTON IS OMITTED, WITH ITS DISABLED GROUP. Its only statement is the call to the
//    unported `LogoutVisitor()`. A button drawn without it would look like a working control and do
//    nothing, which is worse than an absent one. `EditorGUI.BeginDisabledGroup` and its matching
//    `EndDisabledGroup` bracket only that button, so dropping all three keeps the GUI stack
//    balanced -- the pair is omitted as a pair, never half of it.
//
//    The condition that group computed is transcribed at the call site as a commented block rather
//    than kept as live code, because a `bool applyBlocked` nothing reads is dead weight the next
//    reader has to re-derive the purpose of. It is the honest record of when Apply was live: Apply
//    was disabled whenever a field the current action needs was left blank, or when Copy was
//    targeting a named controller that had not been assigned yet.
//
//    One consequence: `needsTagField = true` inside the Copy branch is now a dead store, since the
//    only reader left of that assignment was the omitted condition. It is kept so the branch still
//    matches the shipped one statement for statement and so re-enabling Apply is a pure deletion of
//    comment markers.
//
// ==================================== DEOBF-BUG ================================================
//
// * The four booleans decompile as `bool flag; bool flag2; bool flag3; bool flag4;`, and the switch
//   that sets them decompiles with its `default:` label wedged between the `Copy` and
//   `RemoveLayersWithTag` cases. Both are ILSpy artifacts of a jump table, not source order. The
//   coherent form is written below: the flags are named for what they gate, and `default:` sits
//   last, where it covers the one enumerator the switch does not name -- `TagCurrentLayerWith`.
//
// * `int num = -1; ... num = EditorGUILayout.Popup(-1, ...)` (twice) is the decompiler splitting a
//   declaration from its initialiser across the `BeginChangeCheck` that separates them. The dead
//   `-1` store is dropped; the two popups are written as plain initialisations.
//
// ============================ FORMER DEFERRAL, NOW WITHDRAWN ==================================
//
// THE OUTER DISABLED SCOPE IS BACK. This header used to carry it as the first of two deliberate
// deviations: `new EditorGUI.DisabledScope(LogoutMapper() == null)` wraps the whole section and
// greys the form out when no controller is loaded, and it was left out entirely because "there is no
// honest value to pass in its place" -- `false` would have asserted that a controller is always
// loaded and `true` would have greyed the section out permanently. LogoutMapper has since landed as
// the ActiveController property in ControllerEditor.ControllerContext.cs, so the condition can be
// written as shipped, and it is. The note said restoring it would be a one-line change; it was, plus
// the indent.
//
// ======================================= NOTES =================================================
//
// THIS FILE'S NAME WAS QUESTIONED AND IS RIGHT. An earlier revision of this header recorded that the
// file name was wrong "and so is one line of ControllerEditor.Window.cs", because both called
// `ValidateVisitor` "the transition section". `ValidateVisitor` is the CONTROLLER section: its first
// statement is `if (!EditorSettings.Instance.editingController) return;`, every field it touches
// belongs to the layer/parameter batch-action bank (decompiled 8246-8266), and in OnGUI's section
// run (decompiled 8666-8671) it is the fourth call, after `ReflectVisitor` and `DestroyVisitor`. So
// the file name, ControllerEditor.ControllerSection.cs, says exactly what the file does and needs no
// change; what was wrong was the Window.cs line, which now reads "ValidateVisitor (11806) draws the
// CONTROLLER section, gated on editingController". The sibling file that genuinely was misnamed --
// commissioned as "ControllerSection" while drawing the transition section -- is
// ControllerEditor.TransitionSection.cs, and it has been renamed; its header records that.
//
// The action/scope/destination pickers are plain `EnumPopup`s sized to their own current value
// (`GetTextWidth(...) + 28f`), so each popup is exactly as wide as the entry it is showing and
// changes width as the user picks. That is shipped behaviour and reads as intentional: the row is a
// sentence -- "Replace Parameter [name] With [name] In [scope]" -- and fixed-width popups would
// leave gaps in it.
//
// `parameterNames` is guarded with `?? new string[0]` where the dropdown is drawn but not where the
// picked entry is read back. That is safe rather than a latent NRE: an empty popup cannot register
// a selection, so the change check never fires and the indexer is never reached. Preserved as-is.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- the MAP entry was checked against export/ and line 11806 is the member
// named. On the pass that restored the outer disabled scope, that scope was diffed against export
// (`using (new EditorGUI.DisabledScope(ActiveController() == null))` immediately inside the
// editingController early return and immediately outside the box scope, closing after the last
// statement of the body), and the two remaining deferred targets were re-confirmed absent from the
// package under any name: nothing declares LogoutVisitor or CustomizeVisitor, and no ported member
// claims decompiled 13048 or 11948. The rest of the body was transcribed statement by statement from
// 11806-11945 on an earlier pass and has not been run in the editor, which is why this is PARTIAL.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Controller section -- batch-action toolbar

        /// <summary>
        /// Draws the controller section's batch-action toolbar: the action picker, whichever operand
        /// fields that action needs, the scope selector, and the options row beneath them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The row is deliberately built as a sentence rather than as a form. Choosing an action
        /// decides which operands are meaningful, and only those are drawn -- "Replace Parameter"
        /// shows two name fields and the word "With" between them, "Remove Layers With Tag" shows a
        /// single tag field and no scope selector at all, because it always runs over the whole
        /// controller. Four booleans, computed from the action before anything is drawn, are what
        /// carry that decision through the rest of the method.
        /// </para>
        /// <para>
        /// The tag field is drawn from one of two places depending on the scope, so that it always
        /// reads left to right: on its own when the scope is something else, and immediately after
        /// the scope popup when the scope is "Layers Tagged With", giving "In [Layers Tagged With]
        /// [tag]".
        /// </para>
        /// <para>
        /// See the file header: the licence gate this body contained is dropped, the outer disabled
        /// scope is drawn as shipped, and the Apply button and the trailing panel row are omitted
        /// with their blockers named.
        /// </para>
        /// </remarks>
        private void DrawControllerSection()
        {
            if (!EditorSettings.Instance.editingController)
            {
                return;
            }

            // The whole section is greyed out with no controller loaded. EditorGUI.DisabledScope
            // is self-balancing, so this is a scope rather than the Begin/End pair used further
            // down; see the file header for why it was omitted until ActiveController landed.
            using (new EditorGUI.DisabledScope(ActiveController == null))
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    // The licence gate sat here, inside the box and before anything else. Dropped.

                    // Which operands the chosen action needs. Named for what they gate rather than for
                    // the flag/flag2/flag3/flag4 the decompiler produced.
                    bool needsSourceName = false;      // the "from" parameter name
                    bool needsReplacementName = false; // the "to" parameter name, plus the word "With"
                    bool needsTagField = false;        // the free-text tag
                    bool needsScopeSelector = false;   // the word "In" and the ActionMode popup

                    using (new GUILayout.HorizontalScope())
                    {
                        selectedAction = (ControllerAction)EditorGUILayout.EnumPopup(
                            selectedAction, GUILayout.Width(selectedAction.GetTextWidth() + 28f));

                        switch (selectedAction)
                        {
                            case ControllerAction.ReplaceParameter:
                                needsSourceName = true;
                                needsReplacementName = true;
                                needsScopeSelector = true;
                                break;

                            case ControllerAction.RemoveParameter:
                                needsSourceName = true;
                                needsScopeSelector = true;
                                break;

                            case ControllerAction.RemoveTag:
                                needsSourceName = true;
                                needsScopeSelector = true;
                                break;

                            case ControllerAction.Copy:
                                // Copy carries its own source and destination pickers, below.
                                break;

                            case ControllerAction.RemoveLayersWithTag:
                                // Always runs over the whole controller, so it takes a tag and no scope.
                                needsTagField = true;
                                break;

                            default:
                                // TagCurrentLayerWith: a tag, applied to the selected layer.
                                needsTagField = true;
                                break;
                        }

                        // A scoped action pointed at tagged layers needs somewhere to type the tag too.
                        if (needsScopeSelector && actionScope == ActionMode.LayersTaggedWith)
                        {
                            needsTagField = true;
                        }

                        if (needsSourceName)
                        {
                            // The text field and the 12px popup beside it are one composite control:
                            // "textfielddropdowntext" is the body, "textfielddropdown" the arrow. The
                            // popup is drawn with no selection (-1) so it never shows a current value --
                            // it exists only to write a parameter name into the field.
                            EditorGUIUtility.labelWidth = 40f;
                            actionSourceName = EditorGUILayout.TextField(string.Empty, actionSourceName, "textfielddropdowntext");
                            EditorGUIUtility.labelWidth = 0f;

                            EditorGUI.BeginChangeCheck();
                            int picked = EditorGUILayout.Popup(-1, parameterNames ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                actionSourceName = parameterNames[picked];
                            }
                        }

                        if (needsTagField && actionScope != ActionMode.LayersTaggedWith)
                        {
                            actionFilterText = EditorGUILayout.TextField(actionFilterText);
                        }

                        if (needsReplacementName)
                        {
                            GUILayout.Label("With", GUILayout.Width(32f));

                            EditorGUIUtility.labelWidth = 40f;
                            actionReplacementName = EditorGUILayout.TextField(string.Empty, actionReplacementName, "textfielddropdowntext");
                            EditorGUIUtility.labelWidth = 0f;

                            EditorGUI.BeginChangeCheck();
                            int picked = EditorGUILayout.Popup(-1, parameterNames ?? new string[0], "textfielddropdown", GUILayout.Width(12f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                actionReplacementName = parameterNames[picked];
                            }
                        }

                        if (needsScopeSelector)
                        {
                            GUILayout.Label("In", GUILayout.Width(15f));
                            actionScope = (ActionMode)EditorGUILayout.EnumPopup(actionScope, GUILayout.Width(140f));
                        }

                        // The other half of the tag field: after the scope popup, so the row reads
                        // "In [Layers Tagged With] [tag]".
                        if (needsTagField && actionScope == ActionMode.LayersTaggedWith)
                        {
                            actionFilterText = EditorGUILayout.TextField(actionFilterText);
                        }

                        if (selectedAction == ControllerAction.Copy)
                        {
                            // Copy uses MoveMode rather than ActionMode for its source -- the same first
                            // three scopes, minus the state-machine one, which has no meaning when whole
                            // layers are being copied. See MoveMode's header on the shared ordinals.
                            copySourceScope = (MoveMode)EditorGUILayout.EnumPopup(
                                copySourceScope, GUILayout.Width(copySourceScope.GetTextWidth() + 28f));

                            if (copySourceScope == MoveMode.LayersTaggedWith)
                            {
                                needsTagField = true;
                                actionFilterText = EditorGUILayout.TextField(actionFilterText);
                            }

                            GUILayout.Label("To", GUILayout.Width(20f));

                            // copyDestination is the one instance field in this method; the Copy panel's
                            // destination is per-window and resets with it. See ControllerEditor.State.cs.
                            copyDestination = (MoveDestination)EditorGUILayout.EnumPopup(
                                copyDestination, GUILayout.Width(copyDestination.GetTextWidth() + 28f));

                            if (copyDestination == MoveDestination.Controller)
                            {
                                actionTargetController = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField(
                                    actionTargetController, typeof(UnityEditor.Animations.AnimatorController), false);
                            }
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (selectedAction == ControllerAction.RemoveParameter || selectedAction == ControllerAction.ReplaceParameter)
                        {
                            matchWholeWord = EditorGUILayout.Toggle(
                                new GUIContent("Match Whole Word", "Apply to parameters that match exactly. Otherwise apply to parameters that contain it"),
                                matchWholeWord);
                        }
                        else if (selectedAction == ControllerAction.Copy)
                        {
                            addRequiredParameters = EditorGUILayout.Toggle(
                                new GUIContent("Add Required Parameters", "Add the parameters used by the Source to the destination Controller. Adds Suffix if Suffix isn't empty."),
                                addRequiredParameters, GUILayout.Width(180f));

                            GUILayout.FlexibleSpace();

                            EditorGUIUtility.labelWidth = 50f;
                            copiedParameterSuffix = EditorGUILayout.TextField(
                                new GUIContent("Suffix:", "Add a Suffix to all the Parameters in the newly copied layers. Adds a Suffix to the added parameters if enabled."),
                                copiedParameterSuffix);
                            EditorGUIUtility.labelWidth = 0f;
                        }
                        else
                        {
                            // No options for this action; the flexible space is what right-aligns the
                            // Apply button in the shipped layout.
                            GUILayout.FlexibleSpace();
                        }

                        // DEFERRED: EditorGUI.BeginDisabledGroup(applyBlocked), the Apply button whose
                        //           body is LogoutVisitor(), and EditorGUI.EndDisabledGroup(). All three
                        //           are omitted together so the pair stays balanced; see the header.
                        //
                        //   bool applyBlocked =
                        //       (string.IsNullOrEmpty(actionSourceName) && needsSourceName)
                        //       || (string.IsNullOrEmpty(actionReplacementName) && needsReplacementName)
                        //       || (string.IsNullOrEmpty(actionFilterText) && needsTagField)
                        //       || (selectedAction == ControllerAction.Copy
                        //           && copyDestination == MoveDestination.Controller
                        //           && !actionTargetController);
                        //   EditorGUI.BeginDisabledGroup(applyBlocked);
                        //   if (EditorUtils.Button("Apply", "minibutton", GUILayout.Width(140f)))
                        //   {
                        //       LogoutVisitor();
                        //   }
                        //   EditorGUI.EndDisabledGroup();
                        //
                        // Apply was live only once every field the current action needs had been filled
                        // in, and -- for a Copy into a named controller -- once that controller had been
                        // assigned.
                    }

                    EditorGUILayout.Space();
                    EditorUtils.Separator();
                    EditorGUILayout.Space();

                    // DEFERRED: CustomizeVisitor() -- the Write Defaults / sub-asset panel row. See the
                    //           PARTIAL PORT section of the file header.
                }
            }
        }

        #endregion
    }
}
