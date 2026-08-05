// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ManageWrapper -> SyncSelection, line 8676
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// ================================ LICENCE GATE, NOT PORTED ====================================
//
// The shipped ManageWrapper opens with the obfuscator's inline licence test -- an immediately
// invoked `(Func<bool>)delegate { ... }` that recomputes an HMACSHA256 over the stored date stamp
// and hardware id and compares it against the token the vendor's server handed out, returning early
// when it does not match. It is dropped on the same basis as every other licence wall in this
// package: the validation endpoint is gone, so the check can only ever fail, and failing it here
// would freeze the whole tool's view of the selection -- no state inspector, no transition
// inspector, no condition editor. The body below is the guarded body, ported as if the check
// passed.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ==============================
//
// Nothing below is stubbed. Two of the shipped body's calls are still omitted because their targets
// are not in the package yet. Each omission is marked at its position in the method with a DEFERRED
// comment, so the routine can be completed in place.
//
//   StartInitializer (15195) -- pushes the selected state's clip into the Animation window when
//     both `aw_active` and `aw_autoSwitchClip` are on. The whole `if` is omitted rather than left
//     with an empty body, because an empty body would read as "this case does nothing", which is a
//     fabrication. Its blocker is now exactly one accessor pair: InitInitializer (15175), which
//     reads `animationWindowStateProperty` off FindInitializer (15160), the cached Animation window
//     lookup. Neither is ported. The other half of the blocker this header used to name -- "the
//     clip setter" -- is obsolete: `activeAnimationClipProperty` is resolved in
//     ControllerEditor.ReflectionPriming.cs (line 504), and the `animationWindow` field, the
//     `animationWindowType` handle and `animationWindowStateProperty` all exist, so landing the
//     Animation-window link is those two accessors and nothing else.
//   CallAnnotation (9263) -- the VRChat parameter-driver multi-editor, which rebuilds
//     `parameterDriverBindings` and `parameterDriverEditors` from the selected states. Blocked on
//     PopAnnotation (9148), the per-binding editor folder it calls once per driver found, and on
//     CancelAnnotation (9296), the ReorderableList rebuild it ends with, whose three callbacks
//     (CountAnnotation 9304, DisableAnnotation, InsertAnnotation) are all unported. Its sibling
//     MoveAnnotation has landed, so the `if (AnimatorTypeCache.IsVRCSDKAvailable())` guard the two
//     share is now written out around the one call it can make rather than omitted -- the guard is
//     no longer a guard around nothing.
//
// ============================ FORMER DEFERRALS, NOW WITHDRAWN =================================
//
// Six calls this header used to defer have since landed and are written at their shipped positions:
//
//   AssetVisitor (12961)       -> BuildSharedConditionEditors, in ControllerEditor.ConditionMatching.cs
//   MapVisitor (11763)         -> RebuildConditionList, in ControllerEditor.ConditionList.cs
//   RunAlgo (14801)            -> RebuildTransitionInspector, in ControllerEditor.InspectorRefresh.cs
//   CalculateAnnotation (9768) -> RefreshInspectorProperties, in ControllerEditor.InspectorRefresh.cs
//   ConnectAnnotation (9739)   -> RefreshParameterNames, in ControllerEditor.ParameterNames.cs
//   MoveAnnotation (9222)      -> RefreshTrackingControlEditor, in ControllerEditor.TrackingControlSync.cs
//
// CORRECTION, because this header described two of them wrongly and the wrong descriptions were
// what made them look harder to land than they were:
//
//   * MapVisitor was recorded as "refreshes the AnimatorState inspector's SerializedProperty bank
//     from the new state selection". It does not touch a SerializedProperty. It rebuilds the three
//     condition ReorderableLists -- focused, shared and whole-selection -- and is ported as
//     RebuildConditionList. The member that refreshes the two inspector property banks is
//     CalculateAnnotation, three statements further down. The old entry also called MapVisitor
//     "blocked on the property-refresh helpers"; it never was. Its licence gate is real, and is
//     recorded where the member is ported.
//
//   * CalculateAnnotation was recorded as "two calls, LoginVisitor + PushVisitor, that rebuild the
//     graph node caches". The two calls are right and the description of them is not: LoginVisitor
//     and PushVisitor are the state and transition SerializedProperty banks (RefreshStateProperties
//     and RefreshTransitionProperties). Nothing here rebuilds a graph node cache.
//
// ConnectAnnotation's recorded blocker -- "the ActiveController accessor, which
// ControllerEditor.State.cs's header defers as a group" -- was satisfied when that accessor landed
// as the ActiveController property in ControllerEditor.ControllerContext.cs, so the member itself
// was ported rather than left waiting; likewise MoveAnnotation, whose whole dependency set
// (`allStatesHaveTrackingControl`, TrackingControlEditor, AnimatorTypeCache.TrackingControlType) was
// already in the package.
//
// ==================================== DEOBF-BUG ===============================================
//
// The node-selection assignment decompiles as a flattened `goto` chain -- `obj = enumerable.ToList();
// if (obj != null) goto IL_0073; ... obj = _Process; goto IL_0073;` -- around what the compiler was
// given as `SelectedNodes()?.ToList() ?? emptyNodeSelection`. The same shape appears in the
// edge assignment one line later, where ILSpy did recover the `??`. The coherent form is written
// here.
//
// The transition-kind scan over `Selection.objects` decompiles inside out: `if (hasState || type !=
// typeof(AnimatorStateTransition)) { if (!hasPlain && type == typeof(AnimatorTransition)) hasPlain =
// true; } else hasState = true;`. Because a type cannot be both, and because setting an already-true
// flag is a no-op, that is exactly the two-armed `if/else if` written below.
//
// ======================================= NOTES ================================================
//
// Two aliases the shipped code creates deliberately and this port preserves:
//
//   * when the graph cannot be reached, `selectedNodes` and `selectedEdges` are pointed at the
//     shared `emptyNodeSelection` / `emptyEdgeSelection` sentinels rather than at fresh empties.
//     Nothing in the tool mutates either collection in place, which is what makes that safe.
//   * outside "Make Multiple Transitions" mode, `multiTransitionStates` is assigned the *same* list
//     object as `selectedStates` rather than a copy, so the snapshot only becomes independent on
//     the next sync, when `selectedStates` is replaced wholesale.
//
// The three Entry/Any/Exit node comparisons dereference `GraphAccessors.EntryNode.Node` and friends,
// which are null when there is no Animator window graph. They cannot throw, because they are only
// ever evaluated from inside `Any` over `selectedNodes`, and `selectedNodes` is empty in exactly the
// case where those accessors return null.
//
// Audit status: PARTIAL -- the body was transcribed statement by statement against decompiled lines
// 8676-8829 and the field names were taken from the rename table in ControllerEditor.State.cs. On
// the pass that landed the six former deferrals, every one of the eight calls the shipped body makes
// was re-opened in export/ and re-checked: the six now written out were confirmed to be ported under
// the names listed above, at the shipped positions and in the shipped order (AssetVisitor before the
// state filtering, MapVisitor after the condition-editor diff, RunAlgo immediately before
// CalculateAnnotation, ConnectAnnotation before the VRChat guard, MoveAnnotation second inside it);
// and the two still deferred were followed one level further into their own blockers, which is what
// the notes above now name. The bodies of those two were read only far enough to establish that.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Selection sync

        /// <summary>
        /// Re-reads everything the tool derives from the current selection: the graph's own node and
        /// edge selection, the Unity object selection, and the caches and editors built from both.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the tool's single entry point for "the selection changed". It is called from
        /// <c>OnFocus</c> and from Unity's <see cref="Selection.selectionChanged"/>, and it is
        /// deliberately a full rebuild rather than an incremental update: the graph selection lives
        /// behind reflection into Unity's own <c>GraphGUI</c> and offers no change notification, so
        /// there is nothing to diff against except the previous answer.
        /// </para>
        /// <para>
        /// The graph selection and the Unity selection are not the same set and neither contains the
        /// other. Entry, Any State and Exit are graph nodes with no asset behind them, so they can
        /// never appear in <see cref="Selection.objects"/>; conversely a state selected from the
        /// Project window is in the Unity selection without the graph knowing. Both are read here,
        /// which is why the method is long.
        /// </para>
        /// <para>
        /// The one place a rebuild is avoided is the condition editors: those are diffed rather than
        /// recreated, because each row carries per-row UI state (its mixed-value flags and the value
        /// being typed) that must survive a selection change which keeps the transition it edits.
        /// </para>
        /// <para>
        /// See the file header for the licence gate that wrapped this body and for the two calls
        /// still deferred on unported targets.
        /// </para>
        /// </remarks>
        private static void SyncSelection()
        {
            // DEFERRED: when the Animation window integration is enabled and set to auto-switch,
            //           the selected state's clip is pushed into the Animation window here.
            //           Needs StartInitializer (decompiled 15195), which is unported because
            //           InitInitializer (15175) and FindInitializer (15160) are. Everything else
            //           it touches -- activeAnimationClipProperty, animationWindow,
            //           animationWindowType, animationWindowStateProperty -- is in the package.

            // A rename typed onto a graph node is committed, not discarded, when the selection moves
            // away from it -- so the overlay ends with acceptChanges: true.
            stateRenameOverlay.EndRename(acceptChanges: true);

            // --- the graph's own selection -------------------------------------------------------

            selectedNodes = AnimatorGraphReflection.GraphAccessors.SelectedNodes?.ToList()
                            ?? emptyNodeSelection;
            selectedEdges = AnimatorGraphReflection.GraphAccessors.SelectedEdges ?? emptyEdgeSelection;

            // One edge is drawn per pair of nodes and may carry several transitions, so the edit list
            // is generally longer than the edge list.
            selectedTransitionEdits = new List<AnimatorGraphReflection.TransitionEditionInfo>();
            foreach (AnimatorGraphReflection.GraphEdgeRef edge in selectedEdges)
            {
                selectedTransitionEdits.AddRange(edge.Transitions);
            }

            hasStateTransitionSelected = false;
            hasPlainTransitionSelected = false;

            // In "Make Multiple Transitions" mode the graph selection is being driven by the tool
            // itself, so the Entry and Any State flags are frozen: they are OR-ed in below but never
            // cleared, which keeps a node the user picked before entering the mode part of the set.
            bool entryNodeInGraphSelection = !makeMultipleTransitionsMode && selectedNodes.Any(
                n => n.Node == AnimatorGraphReflection.GraphAccessors.EntryNode.Node);
            bool anyStateNodeInGraphSelection = !makeMultipleTransitionsMode && selectedNodes.Any(
                n => n.Node == AnimatorGraphReflection.GraphAccessors.AnyStateNode.Node);

            entryNodeSelected |= entryNodeInGraphSelection;
            anyStateNodeSelected |= anyStateNodeInGraphSelection;

            // Exit is not frozen the same way: it is assigned outright every sync.
            exitNodeSelected = selectedNodes.Any(
                n => n.Node == AnimatorGraphReflection.GraphAccessors.ExitNode.Node);

            // --- the Unity selection -------------------------------------------------------------

            // Whether the pinned transition is still selected. If it is not, the pin is dropped
            // below, so the condition editor falls back to the whole selection.
            bool focusedTransitionStillSelected = false;

            foreach (UnityEngine.Object selected in Selection.objects.WhereNotNull())
            {
                if (HasFocusedTransition)
                {
                    focusedTransitionStillSelected |= selected == focusedTransition.transition;
                }

                // Exact type tests, not `is`: AnimatorStateTransition derives from
                // AnimatorTransitionBase alongside AnimatorTransition, and the two drive different
                // inspectors, so the distinction has to be exact.
                Type type = selected.GetType();
                if (type == typeof(AnimatorStateTransition))
                {
                    hasStateTransitionSelected = true;
                }
                else if (type == typeof(AnimatorTransition))
                {
                    hasPlainTransitionSelected = true;
                }

                // Both kinds found and the pin accounted for: nothing further can change.
                if (hasStateTransitionSelected && hasPlainTransitionSelected
                    && (!HasFocusedTransition || focusedTransitionStillSelected))
                {
                    break;
                }
            }

            if (!makeMultipleTransitionsMode)
            {
                if (!anyStateNodeInGraphSelection)
                {
                    anyStateNodeSelected = false;
                }

                if (!entryNodeInGraphSelection)
                {
                    entryNodeSelected = false;
                }
            }

            if (!focusedTransitionStillSelected)
            {
                focusedTransition = default(AnimatorGraphReflection.TransitionEditionInfo);
            }

            // The Exit node's incoming transitions back the "select every exit transition" shortcut.
            // Left at its previous value when there is no graph rather than cleared, as shipped.
            if (AnimatorGraphReflection.GraphAccessors.ExitNode != null
                && AnimatorGraphReflection.GraphAccessors.ExitNode.Node != null)
            {
                exitNodeIncomingTransitions =
                    AnimatorGraphReflection.GraphAccessors.ExitNode.IncomingTransitions.ToArray();
            }

            // Transitions selected as objects, as opposed to selected as graph arrows. Only their
            // count is used, at the very end, to force the transition section open.
            AnimatorTransitionBase[] selectedTransitionAssets =
                Selection.GetFiltered<AnimatorTransitionBase>(SelectionMode.Editable);

            selectedTransitions = selectedTransitionEdits.Select(t => t.transition).ToList();

            // The only writer of the "Shared Conditions" rows: the conditions every selected
            // transition has in common. The "All Conditions" rows are built inline further down.
            sharedConditionEditors = BuildSharedConditionEditors(selectedTransitions);

            AnimatorState[] selectedStateAssets = Selection.GetFiltered<AnimatorState>(SelectionMode.Editable);
            selectedStates = selectedStateAssets.ToList();

            // A multi-object SerializedObject over the selected states, or null when there are none:
            // the state inspector's whole property bank hangs off this one object.
            selectedStatesSerialized = selectedStates.Count > 0
                ? new SerializedObject(selectedStateAssets)
                : null;

            selectedStateMachines = Selection.GetFiltered<AnimatorStateMachine>(SelectionMode.Editable);

            // --- the condition editors, diffed rather than rebuilt --------------------------------

            if (selectedTransitionEdits.Count == 0)
            {
                allConditionEditors.Clear();
                conditionEditorTransitions.Clear();
            }
            else
            {
                // Drop the editors belonging to transitions that have left the selection. An editor
                // is matched to its transition through its first target, which is the transition it
                // was constructed from.
                List<ConditionMultiEditor> staleEditors = new List<ConditionMultiEditor>();
                AnimatorTransitionBase[] departed = conditionEditorTransitions
                    .Where(t => !selectedTransitions.Contains(t))
                    .ToArray();

                foreach (AnimatorTransitionBase gone in departed)
                {
                    staleEditors.AddRange(allConditionEditors.Where(c => c.targets[0].transition == gone));
                }

                conditionEditorTransitions = conditionEditorTransitions.Except(departed).ToList();
                allConditionEditors = allConditionEditors.Except(staleEditors).ToList();

                // Add one editor per condition of each transition that has newly arrived. Transitions
                // present in both the old and the new selection are touched by neither pass, which is
                // the point of diffing: their rows keep the state the user has typed into them.
                foreach (AnimatorTransitionBase arrived in
                         selectedTransitions.Where(t => !conditionEditorTransitions.Contains(t)))
                {
                    conditionEditorTransitions.Add(arrived);
                    for (int i = 0; i < arrived.conditions.Length; i++)
                    {
                        allConditionEditors.Add(new ConditionMultiEditor(arrived, i));
                    }
                }
            }

            // Both row sets have just been replaced wholesale, and a ReorderableList binds to the
            // IList it was constructed from, so the list has to be rebuilt rather than repainted.
            RebuildConditionList();

            selectedStateTransitions =
                Selection.GetFiltered<AnimatorStateTransition>(SelectionMode.Editable).ToList();

            // In this order, and not the other way round: re-pointing the SerializedObject
            // invalidates every property handle taken from the old one.
            RebuildTransitionInspector();
            RefreshInspectorProperties();

            // The bulk modes act on the set that was selected when the mode started, so the pending
            // list is only refreshed while neither mode is armed.
            if (!redirectTransitionsMode && !replicateTransitionsMode)
            {
                pendingTransitionEdits = selectedTransitionEdits.ToList();
            }

            // Same idea for the multi-transition snapshot: while the mode is running it must keep
            // describing what the user had chosen before the mode began hijacking the selection.
            if (!makeMultipleTransitionsMode)
            {
                multiTransitionStates = selectedStates;
                multiTransitionStateMachines = selectedStateMachines;
            }

            RefreshParameterNames();

            if (AnimatorTypeCache.IsVRCSDKAvailable())
            {
                // DEFERRED: CallAnnotation() -- the parameter-driver multi-editor, which the
                //           shipped body calls here, before the tracking-control one. See the
                //           file header for the two members it is blocked on.
                RefreshTrackingControlEditor();
            }

            // Both sections are sticky: the setting forces them open, and so does having something
            // of that kind selected, but nothing here closes a section the setting has opened.
            transitionSectionVisible = (bool)EditorSettings.Instance.editingTransitions
                                       || selectedTransitionAssets.Length != 0;
            stateSectionVisible = (bool)EditorSettings.Instance.editingStates
                                  || selectedStates.Count > 0;

            RepaintWindow();
        }

        #endregion
    }
}
