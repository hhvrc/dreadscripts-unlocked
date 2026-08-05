// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The three accessors that answer "what is the tool looking at right now": which controller, which
// layer's state machine, and which sub-machine the graph has been navigated into. Almost every other
// member of the god class starts by asking one of them.
//
//   ActiveController   -> ActiveController,   line 8509
//   PatchMapper        -> the ActiveController setter,   line 8519
//   RootStateMachine   -> RootStateMachine,   line 8532
//   PrintMapper        -> the RootStateMachine setter,   line 8542
//   ActiveStateMachine -> ActiveStateMachine, line 8552
//   OrderInitializer   -> the ActiveStateMachine setter, line 8562
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================== RESOLVED: ALL SIX ACCESSORS ARE NOW COMPLETE ============================
//
// This file previously carried the three getters with their lazy initialisation omitted, and marked
// the three setters NOT PORTED, because every setter fires a change notification that the package
// could not yet run. All six of those notifications have since landed and the omissions are gone:
//
//   ActiveController's setter   -> RebuildLayerCategories       (DisableMapper, 16776)
//                                    in ControllerEditor.LayerCategory.cs
//   RootStateMachine's setter   -> RefreshExitTransitionNames   (FlushAnnotation, 9732)
//   ActiveStateMachine's setter -> RepaintContextViewers        (RestartVisitor, 10837)
//   ActiveController's getter   -> PullControllerFromGraph        (InstantiateAnnotation, 9703)
//   RootStateMachine's getter   -> PullRootStateMachineFromGraph  (DefineAnnotation, 9655)
//   ActiveStateMachine's getter -> PullActiveStateMachineFromGraph(RemoveAnnotation, 9698)
//
// the last five in ControllerEditor.LayerCategoryNotifications.cs. The three accessors therefore now
// behave as shipped: a cold read pulls the value off the Animator window's graph, and a write that
// changes the value notifies. The DELIBERATE DEVIATION this section used to describe -- all three
// reading null forever because nothing in the package assigned them -- is withdrawn.
//
// ====================================== DEOBF-BUG =============================================
//
// DEOBF-BUG(resolved), the ActiveController setter, decompiled line 8519. The snapshot renders it as
//
//     if (currentController != v) { while (true) { currentController = v; DisableMapper(); } }
//
// which is a non-terminating loop and cannot be what shipped -- DisableMapper reads currentController
// back, so a real loop would rebuild the layer tree forever on the first controller change. What is
// written here is the straight-line body:
//
//     if (currentController != value) { currentController = value; RebuildLayerCategories(); }
//
// Evidence, and why this is not a guess. The two sibling setters in the same decompiled block are the
// identical shape over their own field and notifier and decompile *without* the wrapper: PrintMapper
// (8542) is `if (@class != var1) { @class = var1; FlushAnnotation(); }` and OrderInitializer (8562)
// is `if (activeStateMachine != instance) { activeStateMachine = instance; RestartVisitor(); }`. The
// three are the same generated property setter three times over; only one of them picked up the
// `while (true)`. That is the known Reactor-flattened-`if` artifact described in AGENTS.md, and
// ControllerEditor.State.cs's header has recorded this exact site since before it was ported.
//
// ======================================== NOTES ================================================
//
// SHAPE. The shipped members are C# properties: ILSpy renders them as `[SpecialName]` methods, and
// the attribute plus the get_/set_ pairing say property. A rename pass has since given the three
// getters their English names in the snapshot, so lines 8509/8532/8552 now read `ActiveController()`,
// `RootStateMachine()` and `ActiveStateMachine()`; the three setters are still on their obfuscated
// names, PatchMapper/PrintMapper/OrderInitializer. Anything still citing `LogoutMapper`,
// `ManageMapper` or `RevertMapper` is citing a name that no longer exists in the snapshot.
// Accessibility follows the shipped members: ActiveController is internal, the other two private,
// and each property's two accessors have the same accessibility as each other, as they did.
//
// CORRECTION TO ControllerEditor.Window.cs's HEADER, which lists these three among its blockers and
// describes two of them wrongly. That file is owned by another port and is not edited from here, so
// the disagreement is recorded rather than left silent:
//
//   * "LogoutMapper() -- 'is a controller loaded'". It is not a bool. It returns the
//     AnimatorController itself; the call site `BeginDisabledGroup(!LogoutMapper())` reads as a
//     bool only because UnityEngine.Object defines an implicit conversion that is true for a live
//     object and false for a null or destroyed one. "Is a controller loaded" is what that one call
//     site does with the value, not what the member returns.
//
//   * "ManageMapper() -- 'is a VRC avatar loaded'". It has nothing to do with VRChat or with
//     avatars. It returns the selected layer's root state machine. The strip it gates (decompiled
//     8651-8662) is `if ((bool)ManageMapper() && exitTransitionNames.Length != 0)`, and
//     `exitTransitionNames` holds the names of that machine's Any State transitions that target
//     Exit -- an "these transitions leave this layer" reminder drawn as asset labels. The VRChat
//     reading is presumably from an avatar controller being where one usually sees such a strip.
//
//   Window.cs's third description, RevertMapper as "the active state machine, whose name the header
//   row labels itself with; 'No Active Machine' when null", is accurate.
//
// OnGUI IS NOW UN-DEFERRED, for whoever owns ControllerEditor.Window.cs. The three setters and their
// notifiers have landed, so the lines that file defers can be written, in shipped order (decompiled
// 8626-8664):
//
//     EditorGUI.BeginDisabledGroup(!ActiveController);
//     ... GUILayout.Label(ActiveStateMachine != null ? ActiveStateMachine.name : "No Active Machine",
//                         EditorUtils.styles.centeredMiniLabel, GUILayout.ExpandWidth(true));
//     EditorGUI.EndDisabledGroup();   // manual and settings buttons stay live
//     ... EditorGUI.BeginDisabledGroup(!ActiveController);
//     if (RootStateMachine && exitTransitionNames.Length != 0) { ...label strip... }
//     EditorGUI.EndDisabledGroup();
//
// Both disabled groups were omitted as pairs, on the rule that half a Begin/End pair corrupts the
// GUI stack for the rest of the frame. Their condition can now be evaluated honestly, so both pairs
// can go back in whole. That edit belongs to Window.cs's owner and has not been made from here.
//
// WHAT IS STILL NOT REACHABLE FROM HERE. `RootStateMachine`'s notifier dereferences the property it
// was just set from, so setting the root state machine to null from a non-null value re-enters the
// getter, fails to pull a replacement off a graph that no longer has one, and throws on
// `.anyStateTransitions`. That is the shipped behaviour, transcribed; it is reachable only by the
// Animator window losing its graph between two reads, and correcting it would be a behaviour change.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- all six accessors were transcribed statement by statement from decompiled
// lines 8508-8569 on this pass, and the six helpers they call (9655, 9698, 9703, 9732, 10837, 16776)
// were each read in place and are ported in the two files named above. The one place the port does
// not match the snapshot character for character is the ActiveController setter, and that difference
// is the decompiler artifact set out under DEOBF-BUG.

using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Controller context

        /// <summary>
        /// The controller the tool is editing: whatever the Animator window currently has open.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the class's root question. The tool does not own a controller field that the user
        /// assigns -- it follows Unity's own Animator window, so that editing in the graph and
        /// editing in this window always act on the same asset. Everything downstream (the layer
        /// list, the parameter caches, every batch action) is derived from this value, which is why
        /// so many members open by testing it.
        /// </para>
        /// <para>
        /// It is written as a Unity object test rather than an explicit null comparison at the call
        /// sites -- <c>if (ActiveController)</c> -- so a controller that has been destroyed but whose
        /// C# wrapper is still alive reads as absent, which is the behaviour wanted here.
        /// </para>
        /// <para>
        /// A read finds the cached value empty exactly once per change, because the setter is the
        /// only thing that clears it and the getter refills it from the window on the way past. The
        /// write is what rebuilds the layer-category tree, so assigning a controller is never a plain
        /// field write -- see <c>RebuildLayerCategories</c>.
        /// </para>
        /// </remarks>
        internal static UnityEditor.Animations.AnimatorController ActiveController
        {
            get
            {
                if (!currentController)
                {
                    PullControllerFromGraph();
                }

                return currentController;
            }
            set
            {
                if (currentController != value)
                {
                    currentController = value;
                    RebuildLayerCategories();
                }
            }
        }

        /// <summary>
        /// The state machine of the layer the Animator window has selected -- the top of that layer,
        /// not wherever the user has since navigated to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The distinction from <see cref="ActiveStateMachine"/> is the whole point of having both.
        /// A layer's machine can contain sub-machines nested arbitrarily deep, and double-clicking
        /// one moves the graph inside it. Operations that mean "this layer" -- listing the
        /// transitions that leave it, auditing write-defaults across it -- must use this one, or they
        /// would silently narrow to whichever sub-machine happened to be open.
        /// </para>
        /// <para>
        /// Writing it is what refreshes <see cref="exitTransitionNames"/>, so the "these transitions
        /// leave this layer" strip tracks the selected layer without anything polling for it.
        /// </para>
        /// </remarks>
        private static AnimatorStateMachine RootStateMachine
        {
            get
            {
                if (!rootStateMachine)
                {
                    PullRootStateMachineFromGraph();
                }

                return rootStateMachine;
            }
            set
            {
                if (rootStateMachine != value)
                {
                    rootStateMachine = value;
                    RefreshExitTransitionNames();
                }
            }
        }

        /// <summary>
        /// The state machine the graph is actually showing, which is <see cref="RootStateMachine"/>
        /// until the user walks into a sub-machine and one of its descendants after that.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the "you are here" value: it is what the window's title bar names, and what
        /// anything that adds or arranges nodes has to target, because a new state belongs to the
        /// machine on screen rather than to the layer as a whole. When it is null there is no graph
        /// open at all, and the title bar says so.
        /// </para>
        /// <para>
        /// Writing it repaints every window and inspector of the tool's own types, which is how both
        /// windows' chrome follows the graph without either of them subscribing to anything.
        /// </para>
        /// </remarks>
        private static AnimatorStateMachine ActiveStateMachine
        {
            get
            {
                if (!activeStateMachine)
                {
                    PullActiveStateMachineFromGraph();
                }

                return activeStateMachine;
            }
            set
            {
                if (activeStateMachine != value)
                {
                    activeStateMachine = value;
                    RepaintContextViewers();
                }
            }
        }

        #endregion
    }
}
