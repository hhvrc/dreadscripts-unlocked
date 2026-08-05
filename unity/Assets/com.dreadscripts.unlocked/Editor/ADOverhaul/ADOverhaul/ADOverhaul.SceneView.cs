// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: decompiled lines 6264-6682 of the outer ADOverhaul class. Line numbers are
// relative to the current snapshot; the decompiled names below are the durable reference.
//
//   CalculateConfiguration -> DrawShapeEditOverlay,       line 6264
//   CalcConfiguration      -> DrawShapeEditToggles,       line 6294
//   DeleteConfiguration    -> DrawTestModeOverlay,        line 6311
//   DefineConfiguration    -> TickTestSimulation,         line 6391
//   DestroyConfiguration   -> ResolvePhysBoneReflection,  line 6460
//   NewConfiguration       -> ToggleTestMode,             line 6476
//   CompareConfiguration   -> RestartTestMode,            line 6494
//   VerifyConfiguration    -> StartTestMode,              line 6503
//   SetConfiguration       -> StopTestMode,               line 6573
//   SortConfiguration      -> SetShapeCapabilities,       line 6595
//   CustomizeConfiguration -> DrawCollisionTagElement,    line 6637
//   ConcatConfiguration    -> DrawCollisionTagsHeader,    line 6678
//
// Twelve of the region's thirteen members are now ported. The thirteenth, InvokeConfiguration
// (6614), is NOT here and is not missing: ADOverhaul.PhysBoneDrawing.cs folded it into
// DrawShapeProperties as the three inline toggle rows its switch selected between. Its only call
// sites are the three consecutive calls at decompiled 5812-5814, all inside that method. Verified
// against that file's header before writing anything here; nothing is duplicated.
//
// Two members below correspond to no decompiled declaration and are named accordingly:
//   DrawTestModeButtons            the body delegate DeleteConfiguration passes to MoveIdentifier
//                                  (decompiled 6144-6183), lifted out of the lambda because it is
//                                  forty lines long. A representation change only.
//   StopTestModeOnEnteringPlayMode decompiled FillConfiguration (6488), which belongs to another
//                                  file's region -- see the inlining section below.
//
// Field references go through the names agreed in ADOverhaul.State.cs; see that file's header for
// the obfuscated-to-English table. Helpers reused rather than reimplemented (each checked to exist
// before being used):
//   ADOEditorUtility.CustomizeRef()._CreatorSerializer  -> ADOEditorUtility.contents.removeSelection
//   ADOEditorUtility.CustomizeRef().prototypeSerializer -> ADOEditorUtility.contents.customTool
//   ADOEditorUtility.MapRef().utilsMethod               -> ADOEditorUtility.styles.footerButton
//   ADOEditorUtility.MapRef().m_WriterSerializer        -> ADOEditorUtility.styles.centeredBoldRichLabel
//   ADOEditorUtility._ObserverSerializer / _BroadcasterSerializer / m_RecordSerializer -> ADOEditorUtility.validColor / errorColor / secondaryActionColor
//   ADOEditorUtility.ListStatus    -> ADOEditorUtility.IconButton
//   ADOEditorUtility.PatchStatus   -> ADOEditorUtility.Button
//   ADOEditorUtility.QueryStatus   -> ADOEditorUtility.CancelPressed
//   ADOEditorUtility.ComputeStatus -> ADOEditorUtility.SubmitPressed
//   ADOEditorUtility.AssetStatus   -> ADOEditorUtility.IconSpacer
//   ADOEditorUtility.InsertStatus  -> ADOEditorUtility.AddCursorRect
//   ADOEditorUtility.VisitStatus   -> ADOEditorUtility.HasMouseCapture
//   ADOEditorUtility.DisableStatus -> ADOEditorUtility.Separator
//   ADOEditorUtility.RunStatus     -> ADOEditorUtility.AnchorPicker
//   ADOEditorUtility.ConnectStatus -> ADOEditorUtility.TransformHandles
//   ADOEditorUtility.SetupStatus   -> ADOEditorUtility.MapComponents
//   ADOEditorUtility.PositionFlag / SceneViewPanel / SceneView.AddStatus -> DreadScripts.Common.PositionFlag / SceneViewPanel / SceneViewExtensions.GetSceneViewRect
//   NewIdentifier (7806)           -> ADOverhaul.Log (ADOverhaul.Logging.cs)
//   PublishIdentifier (8290)       -> ADOverhaul.DrawSettingsButton (ADOverhaul.Menus.cs)
//   ADOSettings.instance() / .GetValue() / .SetValue(x) -> ADOSettings.instance (a property) / .value
//
// ────────────────────────────── WHAT THIS REGION ACTUALLY IS ──────────────────────────────
//
// The region is not one family. Reading 6060-6478 straight through, it is four:
//   (a) the scene-view shape-handle overlay  -- CalculateConfiguration, CalcConfiguration
//   (b) the PhysBone test-mode driver and its scene-view overlay -- DeleteConfiguration,
//       DefineConfiguration, DestroyConfiguration, NewConfiguration, CompareConfiguration,
//       VerifyConfiguration, SetConfiguration
//   (c) two inspector-side helpers with no scene-view involvement at all -- SortConfiguration,
//       InvokeConfiguration
//   (d) two ReorderableList callbacks for the contact "Collision Tags" list -- CustomizeConfiguration
//       (drawElementCallback) and ConcatConfiguration (drawHeaderCallback)
// (b) is by far the bulk of it and is a simulation driver, not an overlay. The file keeps its
// assigned name so the wave's file ownership stays legible.
//
// ───────────────────────── SceneView.duringSceneGui REGISTRATION AUDIT ─────────────────────────
//
// Two members of this region are SceneView.duringSceneGui handlers. A leaked subscription survives
// domain reload and shows as a permanently stuck overlay, so where each one is registered is
// recorded here.
//
//   DrawShapeEditOverlay (6060) -- the shape-handle overlay. Subscribed and unsubscribed ONLY by
//       CancelConfiguration (6496), which has since landed in ADOverhaul.Lifecycle.cs as
//       SetShapeEditOverlayActive(bool), so the subscription below is now real code rather than a
//       deferred note. It does `duringSceneGui -= DrawShapeEditOverlay` first and then re-adds it
//       when its argument is true; the remove-then-add idiom is what keeps the handler
//       single-registered across the several inspector OnEnables that reach it (the OnEnable half
//       via MapConfiguration, the OnDisable half directly). On the false path it also restores
//       Tools.hidden, because DrawShapeEditOverlay sets it unconditionally and never clears it.
//       Both of those were re-read against decompiled 6700-6711 when that member was ported, and
//       both hold. The overlay is still not reached at runtime, but the blocker has moved one step
//       out: what is missing now is the three inspectors that call SetShapeEditOverlayActive --
//       ContactReceiverEditor, ContactSenderEditor and PhysBoneColliderEditor -- none of which is
//       ported.
//   DrawTestModeOverlay (6107) -- the test-mode overlay. Added by StartTestMode (6365-6366, again
//       remove-then-add) when test mode starts, removed by StopTestMode (6372) when it stops.
//       StopTestMode is reached from ToggleTestMode, which is also what FillConfiguration (6488)
//       calls on ExitingEditMode, so entering play mode tears the subscription down.
//
// SHIPPED HAZARD, PRESERVED. There is no [InitializeOnLoad] unsubscribe anywhere in either build, so
// a domain reload (script recompile, package import) during an active test session leaks the
// DrawTestModeOverlay subscription: the delegate is re-registered on the reloaded static class with
// isTesting back at false, so the handler returns immediately and the overlay disappears, but the
// duplicated "Physbone Tester" hierarchy is hideFlags-only and survives until the next
// GameObject.Find in StartTestMode destroys it, and the source roots stay SetActive(false) because
// StopTestMode never ran. This is what shipped. It is documented, not repaired.
//
// ──────────────────── HELPERS INLINED BECAUSE THEY BELONG TO OTHER REGIONS ────────────────────
//
// Five members this region calls are declared at decompiled lines that fall inside other files'
// regions, and are not ported there yet. Declaring them here would collide with those files the
// moment their owners land them, and a collision does not compile -- so each is reproduced as a
// LOCAL FUNCTION at its call site instead, following the precedent ADOverhaul.PhysBoneDrawing.cs
// set with SearchConfiguration/LoginConfiguration and this file already set with ListAuthentication.
// When the owning region lands the real member, delete the local copy and repoint the call.
//
//   MoveIdentifier      8266  the scene-view overlay frame (ADOverhaul.Menus.cs's region, deferred
//                             there). Reproduced twice, as the `DrawFramedPanel` local function of
//                             DrawShapeEditOverlay and of DrawTestModeOverlay. The duplication is
//                             deliberate: one shared private method would be a name to collide on.
//   WriteIdentifier     8249  the standard titled overlay -- icon spacer, centred title, settings
//                             button, wrapped in MoveIdentifier (ADOverhaul.Menus.cs's region).
//                             Folded into DrawShapeEditOverlay, its only caller in this region.
//   PatchConfiguration  6735  } a GUIContent-wrapping shim and the tinted toggle button under it.
//   CheckConfiguration  6740  } Folded together as DrawShapeEditToggles' `EditToggle` local.
//   InsertConfiguration 6949  the collider restart prompt (ADOverhaul.MultiObjectApply.cs's region,
//                             deferred there). Folded into DrawTestModeOverlay's apply handler.
//
// FillConfiguration (6488), the PlayModeStateChange handler, is a sixth such member -- it belongs to
// ADOverhaul.Lifecycle.cs's region and is deferred there -- but it cannot be a local function,
// because it is subscribed and unsubscribed by reference and a lambda would not unsubscribe. It is
// declared below under the deliberately unlikely-to-collide name
// `StopTestModeOnEnteringPlayMode`. Decompiled lines 6114-6115 are its only subscription in either
// build, and they are in this file, so if Lifecycle later lands its own copy that copy will be dead
// rather than a second subscription. Nothing double-fires.
//
// ───────────────────────────────── SHIPPED BUGS PRESERVED ─────────────────────────────────
//
// 1. RestartTestMode (6290) reads `if (isTesting) ToggleTestMode(); ToggleTestMode();` -- the
//    "Restart" button toggles test mode off and straight back on. The guard exists only so that
//    pressing Restart while not testing starts a session rather than stopping a nonexistent one.
//    Not a decompiler artifact; the 2019 build (RemoveSystem) has the identical shape.
//
// 2. DrawTestModeOverlay's "Apply All Changes" copies each clone onto its original with a bare
//    EditorUtility.CopySerialized. The inspector-side "Apply Changes" button at decompiled 6905-6915
//    does the same copy inside a `ReflectionRestoreScope(original, false, "rootTransform",
//    "ignoreTransforms", "colliders")`, which preserves the three fields that hold scene references
//    into the hierarchy. The overlay's button has no such scope, so applying from the scene view
//    rewrites the original's rootTransform, ignoreTransforms and colliders to point at objects
//    inside the throwaway "Physbone Tester" clone -- which StopTestMode then destroys, leaving the
//    original with null references. Both builds do this. Ported literally.
//
// 3. TickTestSimulation (6187) invokes the cached OnDisable MethodInfo with the array element as the
//    instance on the branch that runs *because* that element is null or destroyed (decompiled 6220
//    and 6230, 2019 lines 6178 and 6207). A truly-null element makes MethodInfo.Invoke throw
//    TargetException and a destroyed one makes the SDK body throw MissingReferenceException; either
//    way the editor update callback dies for that frame. In practice the arrays are rebuilt on every
//    start and nothing destroys entries mid-session, so the branch is unreachable in normal use.
//    Ported as written.
//
// 4. DrawTestModeOverlay's tools toggle assigns `Tools.hidden = false` on every press, including the
//    press that turns hiding *on*. It is corrected on the next scene-GUI pass, where the same method
//    ORs the setting back into Tools.hidden, so the tools flicker for at most one frame.
//
// ──────────────────────── LICENCE GATES AND OBFUSCATOR SCAFFOLDING, NOT PORTED ────────────────────────
//
// CalculateConfiguration (6067) and CustomizeConfiguration (6451) each open with an inline
// Func<bool> that HMAC-SHA256s two outer-class strings against a hard-coded key and skips the draw
// when the digest does not match. This is the protector's activation gate against a server that no
// longer answers, the same check already dropped from PhysBoneEditor.OnInspectorGUI and
// PhysBoneColliderEditor; it is not reproduced. DrawShapeEditOverlay therefore draws its panel
// whenever any edit toggle is on -- in the shipped, unlicensed build it would set Tools.hidden and
// then draw nothing, hiding the native tools with no replacement -- and DrawCollisionTagElement
// draws unconditionally. No other behaviour of either method depends on the gate.
//
// MoveConfiguration (6561) is a third gate of the same shape and is likewise deliberately not
// ported; it falls outside this file's range and is named here only for the record.
//
// ListAuthentication (8692) -- a one-line `type.GetMethod(name, flags)` wrapper -- is deliberately
// inlined into ResolvePhysBoneReflection rather than declared here, because line 8692 falls in
// another file's region and declaring it twice would not compile. Nothing is lost: it has no callers
// outside this region.
//
// _003C_003Ec__DisplayClass54_0 and the `ref` parameter on ViewIdentifier (8410) are compiler capture
// artifacts of a single local `List<Transform>` in VerifyConfiguration. The struct is not ported; the
// local is restored in StartTestMode and ViewIdentifier is restored as its `RecordClonePair` local
// function. Its `[CompilerGenerated]` attribute and its generic constraint are the giveaway.
//
// No other obfuscator scaffolding was found in 6060-6478: nothing in the range is an always-null
// static paired with a null-check, an empty marker type or a licensing remnant beyond the gates
// named above, and no member was dropped for that reason.
//
// 2019 vs 2022: the same thirteen members with the same behaviour (2019 lines 6035-6462, under
// different obfuscated names). The only differences are ones the decompiler chose, and the 2019
// output was used to settle each: the PhysBone half of the simulation tick is emitted as
// `if (!x) { ...; continue; }` in 2019 and as `if ((bool)x) { ... } else if (...)` in 2022 (the same
// control flow -- both are written here in 2019's uniform shape, which matches how 2022 already
// renders the collider half), the enable/disable arms of the collider half are inverted between the
// builds, and InvokeConfiguration's switch lists its cases 2/1/0 in 2019 against 2/0/1 in 2022.
// Neither changes evaluation order or result. No divergence to record.
//
// NOTES
// The twelve MAP line numbers at the top were re-based against the current reverse-engineering/export/ snapshot:
// they had been written before the 561e9ec re-snapshot and were each 204 lines short. The many
// other decompiled line references in the prose sections above (6067, 6144-6183, 6488, 6496,
// 6365-6366, 6372, 6451, 6561, 6735, 6740, 6949, 7806, 8249, 8266, 8290, 8410, 8692, ...) still
// carry the pre-561e9ec numbering and were NOT re-based; treat the member names, not those
// numbers, as the reference until someone sweeps them.
//
// Audit status: PARTIAL -- the twelve MAP line numbers above were checked against reverse-engineering/export/
// ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs and each now lands on the member named, and
// the DrawShapeEditOverlay half of the registration audit was re-read against CancelConfiguration
// (decompiled 6700-6711) and its four call sites when that member was ported; the helper and
// inlining tables further down were not re-verified.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        #region Shape-handle overlay

        /// <summary>
        /// Scene-view overlay listing the shape properties currently being edited by hand, drawn
        /// while any of the four edit toggles is on.
        /// </summary>
        /// <remarks>
        /// A <c>SceneView.duringSceneGui</c> handler, subscribed and unsubscribed by
        /// <see cref="SetShapeEditOverlayActive"/> (ADOverhaul.Lifecycle.cs). It hides Unity's own
        /// transform tools for as long as an edit toggle is on, because the tool's handles and
        /// Unity's would otherwise overlap on the same object; restoring <see cref="Tools.hidden"/>
        /// is that method's job, not this one's -- see the registration audit in the file header.
        /// <para>
        /// The panel is sized from the number of rows <see cref="DrawShapeEditToggles"/> will
        /// actually draw: a base of one for the always-present Position row, plus one for each shape
        /// capability the current component has, at 20 pixels a row over a 45-pixel frame.
        /// </para>
        /// </remarks>
        private static void DrawShapeEditOverlay(SceneView sceneView)
        {
            if (!editingPosition && !editingRotation && !editingRadius && !editingHeight)
            {
                return;
            }

            Tools.hidden = true;

            int rows = 1;
            if (shapeHasRadius)
            {
                rows++;
            }

            if (shapeHasHeight)
            {
                rows++;
            }

            if (shapeHasRotation)
            {
                rows++;
            }

            DrawFramedPanel(200f, 45 + 20 * rows);

            // Local copy of MoveIdentifier (8266) with WriteIdentifier's (8249) standard header
            // folded in; see the file header for why it is not a shared method.
            void DrawFramedPanel(float width, float height)
            {
                Rect sceneViewRect = sceneView.GetSceneViewRect();
                PositionFlag alignment = ADOSettings.instance.toolOverlayAlignment.GetEnumValue<PositionFlag>();

                bool headerDragged;
                using (new SceneViewPanel(sceneView, width, height, alignment, sceneViewPanelResizeHandle))
                {
                    Rect titleRect;
                    using (new GUILayout.HorizontalScope())
                    {
                        // Balances the settings button on the right so the title lands centred.
                        ADOEditorUtility.IconSpacer();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Editing", ADOEditorUtility.styles.centeredBoldRichLabel);
                        titleRect = GUILayoutUtility.GetLastRect();
                        GUILayout.FlexibleSpace();
                        DrawSettingsButton();
                    }

                    ADOEditorUtility.AddCursorRect(titleRect, MouseCursor.Pan);
                    headerDragged = ADOEditorUtility.HasMouseCapture(titleRect, tooltipDragControlId);

                    ADOEditorUtility.Separator(2, 0);
                    DrawShapeEditToggles();
                }

                // Dragging the title bar does not move the panel freely: it swaps the panel to
                // whichever of the nine anchors the pointer is nearest, picked from an overlay drawn
                // across the whole scene view.
                if (headerDragged)
                {
                    Handles.BeginGUI();
                    ADOSettings.instance.toolOverlayAlignment.IntValue =
                        (int)ADOEditorUtility.AnchorPicker(alignment, sceneViewRect);
                    Handles.EndGUI();
                }
            }
        }

        /// <summary>
        /// The body of <see cref="DrawShapeEditOverlay"/>: one toggle per editable shape property,
        /// tinted green when on and red when off.
        /// </summary>
        /// <remarks>
        /// Position has no capability flag and so is always offered; the other three appear only
        /// when the shape being drawn supports them. The row order -- radius, height, position,
        /// rotation -- is the order the properties appear in the inspector, not the field order.
        /// </remarks>
        private static void DrawShapeEditToggles()
        {
            if (shapeHasRadius)
            {
                EditToggle("Radius", ref editingRadius);
            }

            if (shapeHasHeight)
            {
                EditToggle("Height", ref editingHeight);
            }

            EditToggle("Position", ref editingPosition);

            if (shapeHasRotation)
            {
                EditToggle("Rotation", ref editingRotation);
            }

            // Local copy of PatchConfiguration (6735) / CheckConfiguration (6740); see the file
            // header for why it is not a shared method.
            void EditToggle(string label, ref bool state)
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, state, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
                {
                    state = ADOEditorUtility.ToggleButton(state, new GUIContent(label), GUI.skin.button);
                }
            }
        }

        #endregion

        #region PhysBone test mode

        /// <summary>
        /// Looks up the private Unity messages that PhysBone test mode has to invoke by hand, once
        /// per domain load.
        /// </summary>
        /// <remarks>
        /// Test mode runs the VRChat PhysBone simulation inside the editor, outside play mode, which
        /// means Unity never sends <c>Start</c>, <c>OnEnable</c>, <c>OnDisable</c> or the manager's
        /// <c>LateUpdate</c> to the duplicated components. The driver synthesises them, and the SDK
        /// exposes none of the four publicly, so each is fetched reflectively here and cached.
        /// <para>
        /// The lookups go against the <c>Base</c> types rather than <c>VRCPhysBone</c> /
        /// <c>VRCPhysBoneCollider</c> because that is where the SDK declares the messages; a lookup
        /// on the concrete type with <see cref="System.Reflection.BindingFlags.NonPublic"/> would not
        /// see an inherited private member.
        /// </para>
        /// </remarks>
        private static void ResolvePhysBoneReflection()
        {
            if (physBoneReflectionResolved)
            {
                return;
            }

            physBoneReflectionResolved = true;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            physBoneManagerLateUpdate = typeof(PhysBoneManager).GetMethod("LateUpdate", flags);
            physBoneManagerOnDestroy = typeof(PhysBoneManager).GetMethod("OnDestroy", flags);

            physBoneStart = typeof(VRCPhysBoneBase).GetMethod("Start", flags);
            physBoneOnEnable = typeof(VRCPhysBoneBase).GetMethod("OnEnable", flags);
            physBoneOnDisable = typeof(VRCPhysBoneBase).GetMethod("OnDisable", flags);

            physBoneColliderStart = typeof(VRCPhysBoneColliderBase).GetMethod("Start", flags);
            physBoneColliderOnEnable = typeof(VRCPhysBoneColliderBase).GetMethod("OnEnable", flags);
            physBoneColliderOnDisable = typeof(VRCPhysBoneColliderBase).GetMethod("OnDisable", flags);
        }

        /// <summary>
        /// Starts test mode if it is off and stops it if it is on, and is the single entry point for
        /// both. Never starts while the editor is playing, where the real simulation already runs.
        /// </summary>
        private static void ToggleTestMode()
        {
            ResolvePhysBoneReflection();

            isTesting = !isTesting;
            if (Application.isPlaying)
            {
                isTesting = false;
            }

            if (isTesting)
            {
                StartTestMode();
            }
            else
            {
                StopTestMode();
            }
        }

        /// <summary>
        /// Restarts test mode, so that changes the running simulation cannot pick up -- collider
        /// edits, chiefly -- take effect.
        /// </summary>
        /// <remarks>
        /// The double call is deliberate and is what shipped: the first toggle only happens when a
        /// session is running, and tears it down; the second always runs and starts a fresh one. The
        /// guard is there so that pressing "Restart" while not testing starts a session rather than
        /// stopping one that does not exist. Confirmed against the 2019 build, which is identical.
        /// </remarks>
        private static void RestartTestMode()
        {
            if (isTesting)
            {
                ToggleTestMode();
            }

            ToggleTestMode();
        }

        /// <summary>
        /// Builds the throwaway simulation scene: duplicates the selected avatars under a hidden
        /// "Physbone Tester" object, deactivates the originals, wires up a
        /// <see cref="PhysBoneManager"/>, and subscribes the editor update and scene GUI that drive
        /// and frame it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Working on duplicates rather than the originals is what makes the whole feature safe: the
        /// simulation moves transforms every frame, and doing that to the real hierarchy outside play
        /// mode would dirty the scene irrecoverably. The originals are deactivated so the two copies
        /// do not overlap visually, and <see cref="cloneToOriginal"/> is what lets an edit made on a
        /// duplicate be copied back afterwards.
        /// </para>
        /// <para>
        /// <see cref="testRoot"/> carries <see cref="HideFlags.DontSaveInEditor"/> and
        /// <see cref="HideFlags.DontSaveInBuild"/> but is deliberately not hidden in the hierarchy --
        /// the user has to be able to select the duplicates to inspect them. Any object already named
        /// "Physbone Tester" is destroyed first, which is what cleans up after a session that was
        /// interrupted by a domain reload.
        /// </para>
        /// <para>
        /// The selection is remapped onto the duplicates so the inspector keeps showing "the same"
        /// component: the active object is matched by its PhysBone first and its collider second, and
        /// every other selected object is matched positionally by
        /// <see cref="ADOEditorUtility.MapComponents{T}"/> walking the two hierarchies together.
        /// </para>
        /// </remarks>
        private static void StartTestMode()
        {
            // Seeded, not assigned: a prompt already dismissed earlier in this domain stays dismissed
            // even when the persisted "don't ask again" is off.
            hasShownColliderRestartPrompt |= ADOSettings.instance.hasReadColliderTestingWarning;

            selectedObjectsBeforeTest = Selection.gameObjects;
            activeObjectBeforeTest = Selection.activeGameObject;

            originalToClone = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            cloneToOriginal = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            cloneHasUnappliedChanges = new Dictionary<UnityEngine.Object, bool>();
            hasUnappliedTestChanges = false;

            // The list the compiler lifted into _003C_003Ec__DisplayClass54_0, restored as the local
            // it was: the duplicated transforms that correspond to what the user had selected.
            List<Transform> clonesToSelect = new List<Transform>();

            testSourceRoots = Selection.transforms.Select(t => t.root.gameObject).Distinct().ToArray();

            VRCPhysBone[] sourcePhysBones = testSourceRoots
                .SelectMany(o => o.GetComponentsInChildren<VRCPhysBone>(true))
                .ToArray();
            VRCPhysBoneColliderBase[] sourceColliders = testSourceRoots
                .SelectMany(o => o.GetComponentsInChildren<VRCPhysBoneColliderBase>(true))
                .ToArray();

            if (testSourceRoots.Length == 0)
            {
                Log("No Active Objects with PhysBones found in the scene.", CustomLogType.Error);
                return;
            }

            testRoot = GameObject.Find("Physbone Tester");
            if (testRoot)
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }

            testRoot = new GameObject("Physbone Tester")
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
            };
            testRoot.transform.position = activeObjectBeforeTest.transform.position;

            foreach (GameObject sourceRoot in testSourceRoots)
            {
                GameObject clonedRoot = UnityEngine.Object.Instantiate(
                    sourceRoot,
                    sourceRoot.transform.position,
                    sourceRoot.transform.rotation,
                    testRoot.transform);

                Dictionary<VRCPhysBone, VRCPhysBone> physBoneMap =
                    ADOEditorUtility.MapComponents(sourceRoot.transform, clonedRoot.transform, true, sourcePhysBones);
                Dictionary<VRCPhysBoneColliderBase, VRCPhysBoneColliderBase> colliderMap =
                    ADOEditorUtility.MapComponents(sourceRoot.transform, clonedRoot.transform, true, sourceColliders);

                VRCPhysBone activePhysBone = activeObjectBeforeTest.GetComponent<VRCPhysBone>();
                if (activePhysBone != null && physBoneMap.TryGetValue(activePhysBone, out VRCPhysBone clonedPhysBone) && clonedPhysBone != null)
                {
                    Selection.activeGameObject = clonedPhysBone.gameObject;
                }
                else
                {
                    VRCPhysBoneColliderBase activeCollider = activeObjectBeforeTest.GetComponent<VRCPhysBoneColliderBase>();
                    if (activeCollider != null && colliderMap.TryGetValue(activeCollider, out VRCPhysBoneColliderBase clonedCollider) && clonedCollider != null)
                    {
                        Selection.activeGameObject = clonedCollider.gameObject;
                    }
                }

                RecordClonePairs(physBoneMap);
                RecordClonePairs(colliderMap);

                sourceRoot.SetActive(false);
            }

            testPhysBoneManager = testRoot.AddComponent<PhysBoneManager>();
            PhysBoneManager.Inst = testPhysBoneManager;
            testPhysBoneManager.IsSDK = true;
            testPhysBoneManager.Init();

            testPhysBones = testRoot.GetComponentsInChildren<VRCPhysBone>(true);
            testPhysBoneEnabled = new bool[testPhysBones.Length];
            testPhysBoneStarted = new bool[testPhysBones.Length];

            testColliders = testRoot.GetComponentsInChildren<VRCPhysBoneCollider>(true);
            testColliderEnabled = new bool[testColliders.Length];
            testColliderStarted = new bool[testColliders.Length];

            Selection.objects = clonesToSelect.Select(t => t.gameObject).ToArray();

            // Remove-then-add, so that starting a session while one is somehow already registered
            // still leaves exactly one subscription.
            EditorApplication.update -= TickTestSimulation;
            EditorApplication.update += TickTestSimulation;

            SceneView.duringSceneGui -= DrawTestModeOverlay;
            SceneView.duringSceneGui += DrawTestModeOverlay;

            // ViewIdentifier (8410), restored from its capture-struct form: records both directions
            // of the mapping and remembers which duplicates the user had selected the originals of.
            void RecordClonePairs<T>(Dictionary<T, T> map) where T : Component
            {
                foreach (KeyValuePair<T, T> pair in map)
                {
                    originalToClone.Add(pair.Key, pair.Value);
                    cloneToOriginal.Add(pair.Value, pair.Key);
                    cloneHasUnappliedChanges.Add(pair.Value, false);

                    if (selectedObjectsBeforeTest.Contains(pair.Key.gameObject))
                    {
                        clonesToSelect.Add(pair.Value.transform);
                    }
                }
            }
        }

        /// <summary>
        /// Tears the simulation scene down and puts the scene back the way it was.
        /// </summary>
        /// <remarks>
        /// Order matters here. The selection is restored before the duplicates are destroyed, so no
        /// inspector is left pointing at an object that is about to vanish; the manager's
        /// <c>OnDestroy</c> is invoked by hand before <see cref="testRoot"/> goes, because Unity will
        /// not send it outside play mode and the SDK uses it to release the simulation's buffers. The
        /// source roots are reactivated last, and are filtered for null on the way -- a root the user
        /// deleted during the session is skipped rather than throwing.
        /// <para>
        /// Unapplied edits are discarded silently: the dictionaries are dropped without consulting
        /// <see cref="hasUnappliedTestChanges"/>. That is what shipped, and it is why the overlay
        /// puts "Apply All Changes" next to "Stop Testing".
        /// </para>
        /// </remarks>
        private static void StopTestMode()
        {
            EditorApplication.update -= TickTestSimulation;
            SceneView.duringSceneGui -= DrawTestModeOverlay;

            Selection.objects = selectedObjectsBeforeTest;
            Selection.activeObject = activeObjectBeforeTest;

            physBoneManagerOnDestroy.Invoke(testPhysBoneManager, null);

            if (testRoot)
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }

            foreach (GameObject sourceRoot in testSourceRoots.Where(o => o))
            {
                sourceRoot.SetActive(true);
            }

            originalToClone = cloneToOriginal = null;
            cloneHasUnappliedChanges = null;
            colliderChangedDuringTest = false;
            hasUnappliedTestChanges = false;
        }

        /// <summary>
        /// One simulation step, run from <see cref="EditorApplication.update"/>: pumps the PhysBone
        /// manager and synthesises the Unity messages the duplicated components would otherwise never
        /// receive.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Outside play mode Unity does not run <c>Start</c>, <c>OnEnable</c> or <c>OnDisable</c> on
        /// anything, so the driver tracks each component's enabled-and-active state itself and calls
        /// the matching private message whenever it changes. <c>Start</c> is invoked once per
        /// component, on its first enable, which is the order Unity itself guarantees and which the
        /// SDK's OnEnable depends on.
        /// </para>
        /// <para>
        /// SHIPPED BUG, preserved: on the branch taken because an array entry is null or destroyed,
        /// the OnDisable message is still invoked with that entry as the instance -- see the file
        /// header. Unreachable in normal use, since the arrays are rebuilt on every start.
        /// </para>
        /// <para>
        /// The missing-manager early exit is what makes a session survive its own scene being
        /// deleted: rather than throwing every frame, it toggles test mode, which lands in
        /// <see cref="StopTestMode"/> and unsubscribes this method.
        /// </para>
        /// </remarks>
        private static void TickTestSimulation()
        {
            if (!testPhysBoneManager)
            {
                ToggleTestMode();
                return;
            }

            physBoneManagerLateUpdate.Invoke(testPhysBoneManager, null);

            for (int i = 0; i < testPhysBones.Length; i++)
            {
                if (!testPhysBones[i])
                {
                    if (testPhysBoneEnabled[i])
                    {
                        testPhysBoneEnabled[i] = false;
                        physBoneOnDisable.Invoke(testPhysBones[i], null);
                    }

                    continue;
                }

                bool enabled = testPhysBones[i].enabled && testPhysBones[i].gameObject.activeInHierarchy;
                if (testPhysBoneEnabled[i] == enabled)
                {
                    continue;
                }

                testPhysBoneEnabled[i] = enabled;
                if (!enabled)
                {
                    physBoneOnDisable.Invoke(testPhysBones[i], null);
                    continue;
                }

                physBoneOnEnable.Invoke(testPhysBones[i], null);
                if (!testPhysBoneStarted[i])
                {
                    testPhysBoneStarted[i] = true;
                    physBoneStart.Invoke(testPhysBones[i], null);
                }
            }

            for (int i = 0; i < testColliders.Length; i++)
            {
                if (!testColliders[i])
                {
                    if (testColliderEnabled[i])
                    {
                        testColliderEnabled[i] = false;
                        physBoneColliderOnDisable.Invoke(testColliders[i], null);
                    }

                    continue;
                }

                bool enabled = testColliders[i].enabled && testColliders[i].gameObject.activeInHierarchy;
                if (testColliderEnabled[i] == enabled)
                {
                    continue;
                }

                testColliderEnabled[i] = enabled;
                if (!enabled)
                {
                    physBoneColliderOnDisable.Invoke(testColliders[i], null);
                    continue;
                }

                physBoneColliderOnEnable.Invoke(testColliders[i], null);
                if (!testColliderStarted[i])
                {
                    testColliderStarted[i] = true;
                    physBoneColliderStart.Invoke(testColliders[i], null);
                }
            }
        }

        /// <summary>
        /// Stops test mode when the editor is about to enter play mode.
        /// </summary>
        /// <remarks>
        /// The simulation scene is made of scene objects, so it would otherwise be carried into play
        /// mode alongside the real avatars -- both simulating, and the originals still deactivated.
        /// <c>ExitingEditMode</c> is the last callback that can still touch the edit-mode scene.
        /// <para>
        /// This is decompiled <c>FillConfiguration</c> (6488), which belongs to
        /// ADOverhaul.Lifecycle.cs's region; the name is deliberately distinct so the two can coexist
        /// if that region lands its own copy. See the file header.
        /// </para>
        /// </remarks>
        private static void StopTestModeOnEnteringPlayMode(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingEditMode && isTesting)
            {
                ToggleTestMode();
            }
        }

        /// <summary>
        /// The test-mode scene-view overlay: transform handles on the simulation root, a header with
        /// a "hide native tools" toggle, and the Stop / Restart / Apply All Changes buttons.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <c>SceneView.duringSceneGui</c> handler, subscribed by <see cref="StartTestMode"/> and
        /// unsubscribed by <see cref="StopTestMode"/>. It re-registers the play-mode handler on every
        /// pass rather than once at start -- remove-then-add, so the subscription count stays at one
        /// -- which is what re-establishes it after a domain reload that leaves a session running.
        /// </para>
        /// <para>
        /// The transform handles are drawn for the simulation root itself, so the whole duplicated
        /// hierarchy can be dragged somewhere with room to swing. Scale is suppressed because scaling
        /// the root would rescale the simulation.
        /// </para>
        /// <para>
        /// "Stop Testing" also responds to Escape and Enter, in that order and short-circuiting, so a
        /// key press is consumed by the button's own handler and not by the scene view.
        /// </para>
        /// </remarks>
        private static void DrawTestModeOverlay(SceneView sceneView)
        {
            if (!isTesting)
            {
                return;
            }

            // ORed, not assigned: turning the setting off does not reveal tools something else hid.
            Tools.hidden |= ADOSettings.instance.hideToolsDuringTesting;

            EditorApplication.playModeStateChanged -= StopTestModeOnEnteringPlayMode;
            EditorApplication.playModeStateChanged += StopTestModeOnEnteringPlayMode;

            if (testRoot != null)
            {
                ADOEditorUtility.TransformHandles(
                    testRoot.transform,
                    forceMove: true,
                    forceRotate: true,
                    forceScale: false,
                    suppressMove: false,
                    suppressRotate: false,
                    suppressScale: true);
            }

            DrawFramedPanel(200f, 104f);

            // Local copy of MoveIdentifier (8266); see the file header for why it is not shared with
            // DrawShapeEditOverlay's identical copy.
            void DrawFramedPanel(float width, float height)
            {
                Rect sceneViewRect = sceneView.GetSceneViewRect();
                PositionFlag alignment = ADOSettings.instance.toolOverlayAlignment.GetEnumValue<PositionFlag>();

                bool headerDragged;
                using (new SceneViewPanel(sceneView, width, height, alignment, sceneViewPanelResizeHandle))
                {
                    Rect titleRect;
                    using (new GUILayout.HorizontalScope())
                    {
                        bool toolsHidden = ADOSettings.instance.hideToolsDuringTesting;
                        string tooltip = toolsHidden
                            ? "Native tools are hidden during test."
                            : "Native tools are visible during test.";

                        using (new GUIColorScope(GUIColorScope.ColoringType.FG, toolsHidden, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
                        {
                            if (ADOEditorUtility.IconButton(new GUIContent(ADOEditorUtility.contents.customTool) { tooltip = tooltip }))
                            {
                                ADOSettings.instance.hideToolsDuringTesting.Toggle();

                                // Cleared unconditionally, including when hiding was just switched
                                // on -- the OR at the top of this method puts it back on the next
                                // pass. Shipped behaviour; see the file header.
                                Tools.hidden = false;
                            }
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Testing", ADOEditorUtility.styles.centeredBoldRichLabel);
                        titleRect = GUILayoutUtility.GetLastRect();
                        GUILayout.FlexibleSpace();
                        DrawSettingsButton();
                    }

                    ADOEditorUtility.AddCursorRect(titleRect, MouseCursor.Pan);
                    headerDragged = ADOEditorUtility.HasMouseCapture(titleRect, tooltipDragControlId);

                    ADOEditorUtility.Separator(2, 0);
                    DrawTestModeButtons();
                }

                if (headerDragged)
                {
                    Handles.BeginGUI();
                    ADOSettings.instance.toolOverlayAlignment.IntValue =
                        (int)ADOEditorUtility.AnchorPicker(alignment, sceneViewRect);
                    Handles.EndGUI();
                }
            }
        }

        /// <summary>
        /// The three buttons in the body of the test-mode overlay.
        /// </summary>
        /// <remarks>
        /// SHIPPED BUG, preserved: "Apply All Changes" copies each edited duplicate onto its original
        /// with a bare <see cref="EditorUtility.CopySerialized"/>. The inspector's equivalent button
        /// wraps the same copy in a <c>ReflectionRestoreScope</c> that preserves <c>rootTransform</c>,
        /// <c>ignoreTransforms</c> and <c>colliders</c>; this one does not, so applying from the scene
        /// view repoints those three fields at objects inside the throwaway hierarchy, which
        /// <see cref="StopTestMode"/> then destroys. See the file header.
        /// </remarks>
        private static void DrawTestModeButtons()
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.errorColor))
            {
                if (ADOEditorUtility.Button("Stop Testing") || ADOEditorUtility.CancelPressed() || ADOEditorUtility.SubmitPressed())
                {
                    ToggleTestMode();
                }
            }

            using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.secondaryActionColor))
            {
                if (ADOEditorUtility.Button("Restart"))
                {
                    RestartTestMode();
                }
            }

            using (new GUIColorScope(GUIColorScope.ColoringType.BG, hasUnappliedTestChanges, ADOEditorUtility.validColor))
            using (new EditorGUI.DisabledScope(!hasUnappliedTestChanges))
            {
                if (ADOEditorUtility.Button("Apply All Changes"))
                {
                    // Materialised, because the loop writes back into the dictionary it iterates.
                    foreach (UnityEngine.Object clone in cloneHasUnappliedChanges.Keys.ToList())
                    {
                        if (!cloneHasUnappliedChanges[clone])
                        {
                            continue;
                        }

                        UnityEngine.Object original = cloneToOriginal[clone];
                        if (original != null)
                        {
                            Undo.RecordObject(original, "ADO - Apply Changes");
                            EditorUtility.CopySerialized(clone, original);
                            cloneHasUnappliedChanges[clone] = false;
                        }
                    }

                    hasUnappliedTestChanges = false;
                    PromptForColliderRestart();
                }
            }
        }

        /// <summary>
        /// Offers to restart the session when a collider was edited during it, at most once per
        /// session.
        /// </summary>
        /// <remarks>
        /// The running simulation reads its collider set once, at start, so a collider edit applied
        /// mid-session has no visible effect until test mode is restarted. The flag is set before the
        /// dialog is shown, so dismissing it -- including with Escape, which maps to "No" -- silences
        /// it for the rest of the session; "Don't ask again" additionally persists that answer.
        /// <para>
        /// This is decompiled <c>InsertConfiguration</c> (6949), which belongs to
        /// ADOverhaul.MultiObjectApply.cs's region and is deferred there. It is reproduced here under
        /// a distinct name because this file's overlay is one of its three call sites and it cannot
        /// be declared twice; see the file header. Nothing on any path is destructive.
        /// </para>
        /// </remarks>
        private static void PromptForColliderRestart()
        {
            if (!isTesting || !colliderChangedDuringTest || hasShownColliderRestartPrompt)
            {
                return;
            }

            hasShownColliderRestartPrompt = true;

            switch (EditorUtility.DisplayDialogComplex(
                "Testing Restart Required",
                "Collider changes require a restart of the testing process. Do you want to restart testing?",
                "Yes",
                "No",
                "Don't ask again"))
            {
                case 0:
                    RestartTestMode();
                    break;

                case 2:
                    ADOSettings.instance.hasReadColliderTestingWarning.value = true;
                    break;
            }
        }

        #endregion

        #region Shape-handle capabilities

        /// <summary>
        /// Declares which of the shape properties the component currently being drawn actually has,
        /// and clears the matching edit toggle for any it does not.
        /// </summary>
        /// <remarks>
        /// Called from the inspectors as the shape type changes -- a sphere has a radius but no
        /// height and no meaningful orientation, a plane has neither radius nor height. Clearing the
        /// toggle alongside the capability is what stops a handle the user left switched on for a
        /// capsule from reappearing when they switch that collider to a sphere. Position is
        /// unconditional and so has no capability flag.
        /// </remarks>
        private static void SetShapeCapabilities(bool hasRadius, bool hasHeight, bool hasRotation)
        {
            shapeHasRadius = hasRadius;
            shapeHasHeight = hasHeight;
            shapeHasRotation = hasRotation;

            if (!shapeHasRadius)
            {
                editingRadius = false;
            }

            if (!shapeHasHeight)
            {
                editingHeight = false;
            }

            if (!shapeHasRotation)
            {
                editingRotation = false;
            }
        }

        #endregion

        #region Collision tag list

        /// <summary>
        /// Draws one row of a contact component's collision-tag list: a tag picker, the editable tag
        /// string, and a delete button.
        /// </summary>
        /// <remarks>
        /// This is a <c>ReorderableList</c> element callback, which is why it takes the list property
        /// and an index rather than the element -- and why it has to bounds-check them itself: the
        /// list can be drawn for one more frame after an element is removed, with an index that no
        /// longer exists.
        /// <para>
        /// The picker is a <see cref="EditorGUI.Popup(Rect, int, string[])"/> held permanently at
        /// index -1 so that it shows no current value and acts purely as a menu; picking an entry
        /// writes it into the string field and the popup falls back to -1 on the next frame. Its
        /// options come from <see cref="avatarCollisionTags"/>, where VRChat's built-in tags carry a
        /// "Default/" prefix to group them into a submenu -- the prefix is a display device only, so
        /// it is stripped before the tag is stored. The whole picker is disabled when no target
        /// avatar is set, since the tag list is gathered from that avatar.
        /// </para>
        /// </remarks>
        private static void DrawCollisionTagElement(SerializedProperty collisionTags, Rect rowRect, int index)
        {
            if (index >= collisionTags.arraySize || index < 0)
            {
                return;
            }

            SerializedProperty element = collisionTags.GetArrayElementAtIndex(index);

            rowRect.y += 1f;
            rowRect.height = 18f;
            rowRect.width -= 44f;

            Rect pickerRect = rowRect;
            pickerRect.width = 21f;

            Rect fieldRect = rowRect;
            fieldRect.x += 22f;
            fieldRect.width -= 12f;

            Rect deleteRect = fieldRect;
            deleteRect.x += fieldRect.width;
            deleteRect.width = 28f;

            using (new EditorGUI.DisabledScope(!selectedAvatar))
            {
                int picked = EditorGUI.Popup(pickerRect, -1, avatarCollisionTags);
                if (picked != -1)
                {
                    element.stringValue = Regex.Replace(avatarCollisionTags[picked], "^Default/", string.Empty);
                }
            }

            EditorGUI.PropertyField(fieldRect, element, GUIContent.none);

            if (GUI.Button(deleteRect, ADOEditorUtility.contents.removeSelection, ADOEditorUtility.styles.footerButton))
            {
                collisionTags.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>
        /// Draws the header row of the collision-tag list.
        /// </summary>
        /// <remarks>
        /// The style is rebuilt on every call, which for a <c>ReorderableList</c> header means once
        /// per repaint. That is what the shipped build does and it is reproduced rather than hoisted
        /// into the shared style table, so that the port stays a faithful mirror of the original;
        /// the cost is a small per-frame allocation, not a behavioural difference.
        /// </remarks>
        private static void DrawCollisionTagsHeader(Rect headerRect)
        {
            GUIStyle style = new GUIStyle("boldlabel");
            GUI.Label(headerRect, "Collision Tags", style);
        }

        #endregion
    }
}
