// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The three read accessors that answer "what is the tool looking at right now": which controller,
// which layer's state machine, and which sub-machine the graph has been navigated into. Almost every
// other member of the god class starts by asking one of them, and ControllerEditor.Window.cs's OnGUI
// names all three as blockers.
//
//   LogoutMapper     -> ActiveController,   line 8509
//   ManageMapper     -> RootStateMachine,   line 8532
//   RevertMapper     -> ActiveStateMachine, line 8552
//   PatchMapper      -> NOT PORTED, line 8519 -- the ActiveController setter. Its body is
//     `currentController = value; DisableMapper();`, and DisableMapper (16776) rebuilds the entire
//     layer-category tree through a dozen unported members (InsertMapper, ReadMapper, LayerPathNode,
//     PushInitializer, ValidateInitializer). See PARTIAL PORT.
//   PrintMapper      -> NOT PORTED, line 8542 -- the RootStateMachine setter. Its notifier
//     FlushAnnotation (9732) is unported. See PARTIAL PORT.
//   OrderInitializer -> NOT PORTED, line 8562 -- the ActiveStateMachine setter. Its notifier
//     RestartVisitor (10837) is unported. See PARTIAL PORT.
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ==============================
//
// Nothing here is stubbed, but none of the three is complete either. Each shipped getter is two
// statements -- a lazy initialisation when the cached value is null, then the return -- and this
// file ports the return of all three and the lazy initialisation of none:
//
//     internal static AnimatorController LogoutMapper()      // 8509
//     {
//         if (!_Container) InstantiateAnnotation();          // omitted here
//         return _Container;
//     }
//
// ManageMapper (8532) and RevertMapper (8552) have exactly the same shape over `@class` and `_Mock`,
// calling DefineAnnotation (9655) and RemoveAnnotation (9698) respectively.
//
// The three initialisers are one line each, and each of those lines is the problem: every one of
// them writes its field *through the property setter*, and every setter fires a change notification
// that this package cannot yet run.
//
//   ActiveController -- omits `if (!currentController) InstantiateAnnotation();`
//     InstantiateAnnotation (9703) is `if (GraphAccessors.Tool != null) PatchMapper(
//     GraphAccessors.AnimatorController);`. Both accessors it reads are already ported, in
//     AnimatorGraphReflection.GraphAccessors.cs, so the blocker is entirely PatchMapper's notifier
//     DisableMapper (16776): it re-derives the layer category tree from the new controller's layers
//     and is the single largest unported member reachable from here. Assigning the field without it
//     would leave the layer view describing the previous controller, which is worse than not
//     assigning at all -- so the call is omitted rather than approximated.
//
//   RootStateMachine -- omits `if (!rootStateMachine) DefineAnnotation();`
//     DefineAnnotation (9655) is `if (ActiveController) PrintMapper(
//     GraphAccessors.RootStateMachine);`. PrintMapper's notifier FlushAnnotation (9732) refills
//     `exitTransitionNames` from the new machine's Any State transitions:
//         exitTransitionNames = ManageMapper().anyStateTransitions.Where(t => t.isExit)
//                                             .Select(t => t.name).ToArray();
//     That is portable as written, but it is PrintMapper's body and not this file's -- see NOTES.
//
//   ActiveStateMachine -- omits `if (!activeStateMachine) RemoveAnnotation();`
//     RemoveAnnotation (9698) is `OrderInitializer(GraphAccessors.ActiveStateMachine);`.
//     OrderInitializer's notifier RestartVisitor (10837) repaints every open window and editor whose
//     type is listed in `repaintTargetTypes`; that field is ported, the member is not.
//
// ================================ DELIBERATE DEVIATION ========================================
//
// The visible consequence of the above is that all three accessors return whatever the fields
// already hold, and in the package as it stands nothing assigns those fields -- so they read null.
// The window therefore behaves as though no controller were open: OnGUI's title bar shows
// "No Active Machine" and the exit-transition label strip stays hidden, where the shipped tool would
// pick the current controller up from the Animator window on the first repaint after it changed.
//
// This is deliberate, and the alternative was rejected. Reading the graph directly here --
// `activeStateMachine ? activeStateMachine : GraphAccessors.ActiveStateMachine` -- would make the
// window look correct while silently skipping the change notification the shipped code exists to
// fire, which is precisely the kind of plausible-looking fabrication this package's rules forbid.
// A caller that gets null learns the truth: the context is not wired up yet.
//
// The accessors are ported now, ahead of their setters, because they are read-only at every call
// site in the class and because their *names* are what a dozen deferred call sites are waiting on.
// Landing them lets those sites be written against the real identifiers, so that the day the setters
// arrive the fix is confined to these three bodies.
//
// ======================================== NOTES ================================================
//
// SHAPE. The shipped members are C# properties: ILSpy renders them as `[SpecialName]` methods named
// LogoutMapper/PatchMapper and so on, but the attribute and the get_/set_ pairing say property, and
// call sites such as `EditorGUI.BeginDisabledGroup(!LogoutMapper())` and `RevertMapper().name` read
// as property accesses in the original. They are ported as properties, with the names
// ControllerEditor.State.cs's header reserved for them. Accessibility follows the shipped members:
// ActiveController is internal, the other two private.
//
// CORRECTION TO ControllerEditor.Window.cs's HEADER, which lists these three among its blockers and
// describes two of them wrongly. That file is shared and is not edited from here, per the task's
// instructions, so the disagreement is recorded rather than left silent:
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
// WHAT UN-DEFERS OnGUI. Once the three setters and their notifiers land, the deferred lines in
// ControllerEditor.Window.cs become, in shipped order (decompiled 8626-8664):
//
//     EditorGUI.BeginDisabledGroup(!ActiveController);
//     ... GUILayout.Label(ActiveStateMachine != null ? ActiveStateMachine.name : "No Active Machine",
//                         EditorUtils.styles.centeredMiniLabel, GUILayout.ExpandWidth(true));
//     EditorGUI.EndDisabledGroup();   // manual and settings buttons stay live
//     ... EditorGUI.BeginDisabledGroup(!ActiveController);
//     if (RootStateMachine && exitTransitionNames.Length != 0) { ...label strip... }
//     EditorGUI.EndDisabledGroup();
//
// Both disabled groups stay omitted until then: half a Begin/End pair corrupts the GUI stack for the
// rest of the frame, so neither half is emitted while the condition cannot be evaluated honestly.
//
// FOLLOW-UP ORDER, for whoever picks this up. RevertMapper/OrderInitializer is the cheapest to
// finish -- RestartVisitor is a dozen lines over an already-ported field. ManageMapper/PrintMapper
// is next, needing only FlushAnnotation. LogoutMapper/PatchMapper is gated on DisableMapper and
// should be done last, with the layer-category work, not before it.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- the three getters were transcribed from decompiled lines 8508-8559 on
// this pass, and the three setters and the six initialisers and notifiers named above (8519, 8542,
// 8562, 9655, 9698, 9703, 9732, 10837, 16776) were each read in place to confirm the blockers are
// real and correctly attributed. PARTIAL rather than VERIFIED because the ported bodies are
// deliberately incomplete, as set out under PARTIAL PORT.

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
        /// PARTIALLY PORTED: the shipped getter first pulls the controller off the Animator window if
        /// the cached value is empty. That refresh is omitted; see the file header for why and for
        /// what it will take to restore.
        /// </para>
        /// </remarks>
        internal static UnityEditor.Animations.AnimatorController ActiveController
        {
            get
            {
                // DEFERRED: if (!currentController) InstantiateAnnotation();
                //           -- reads the controller off the Animator window through the unported
                //           setter, whose notifier DisableMapper rebuilds the layer tree.
                return currentController;
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
        /// PARTIALLY PORTED: the shipped getter re-reads the layer's machine from the graph when the
        /// cached value is empty, which is also what refreshes the exit-transition name cache. That
        /// refresh is omitted; see the file header.
        /// </para>
        /// </remarks>
        private static AnimatorStateMachine RootStateMachine
        {
            get
            {
                // DEFERRED: if (!rootStateMachine) DefineAnnotation();
                //           -- would re-read the layer's machine and, through the unported setter's
                //           notifier, rebuild exitTransitionNames.
                return rootStateMachine;
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
        /// PARTIALLY PORTED: the shipped getter re-reads it from the graph when the cached value is
        /// empty, and the setter repaints every window that displays it. Neither is ported; see the
        /// file header.
        /// </para>
        /// </remarks>
        private static AnimatorStateMachine ActiveStateMachine
        {
            get
            {
                // DEFERRED: if (!activeStateMachine) RemoveAnnotation();
                //           -- would re-read the current machine from the graph and, through the
                //           unported setter's notifier, repaint every window showing its name.
                return activeStateMachine;
            }
        }

        #endregion
    }
}
