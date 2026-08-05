// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class ControllerEditorWindow -> lifted to a top-level type, line 3190
//
// Despite the name this is the tool's *settings* window, not its main view: the Controller Editor
// proper lives entirely in Harmony patches on Unity's own Animator window, and this EditorWindow
// exists only to edit EditorSettings. Its two entry points into the rest of the assembly say as
// much -- the menu item "DreadTools/Controller Editor/Settings" (line 3307) and the "Settings"
// item the shared hamburger menu adds (line 9136).
//
// The nested private enum NodeColor (decompiled line 3192) is lifted to a top-level type of its
// own and is mapped by the file that declares it, NodeColor.cs; it is not claimed here.
//
// Member mapping, with the decompiled line:
//   m_AdvisorMapper             -> targetAnimator,                          line 3203
//   _CallbackMapper             -> alwaysUseTargetAnimator,                 line 3205
//   indexerMapper               -> selectedTab,                             line 3207
//   m_IssuerMapper              -> tabLabels,                               line 3209
//   _PrototypeMapper            -> selectedDefaultsTab,                     line 3211
//   _RuleMapper                 -> defaultsTabLabels,                       line 3213
//   m_SingletonMapper           -> emptyDropdownOptions,                    line 3215
//   _FactoryMapper              -> stateObject,                             line 3217
//   m_Name                      -> stateName,                               line 3219
//   _AccountMapper              -> stateSpeed,                              line 3221
//   m_RefMapper                 -> stateCycleOffset,                        line 3223
//   m_StatusMapper              -> stateIkOnFeet,                           line 3225
//   _TokenMapper                -> stateWriteDefaults,                      line 3227
//   _CodeMapper                 -> stateMirror,                             line 3229
//   _DicMapper                  -> stateSpeedParameterActive,               line 3231
//   invocationMapper            -> stateMirrorParameterActive,              line 3233
//   roleMapper                  -> stateCycleOffsetParameterActive,         line 3235
//   paramMapper                 -> stateTimeParameterActive,                line 3237
//   modelMapper                 -> stateMotion,                             line 3239
//   tokenizerMapper             -> stateTag,                                line 3241
//   _DecoratorMapper            -> stateSpeedParameter,                     line 3243
//   _ComparatorMapper           -> stateMirrorParameter,                    line 3245
//   m_ExceptionMapper           -> stateCycleOffsetParameter,               line 3247
//   objectMapper                -> stateTimeParameter,                      line 3249
//   _UtilsMapper                -> transitionObject,                        line 3251
//   _ValMapper                  -> transitionSolo,                          line 3253
//   valueMapper                 -> transitionMute,                          line 3255
//   _MerchantMapper             -> transitionDuration,                      line 3257
//   m_AuthenticationMapper      -> transitionOffset,                        line 3259
//   reponseMapper               -> transitionExitTime,                      line 3261
//   m_PoolMapper                -> transitionHasExitTime,                   line 3263
//   _ParameterMapper            -> transitionHasFixedDuration,              line 3265
//   _ComposerMapper             -> transitionInterruptionSource,            line 3267
//   repositoryMapper            -> transitionOrderedInterruption,           line 3269
//   _MappingMapper              -> transitionCanTransitionToSelf,           line 3271
//   containerMapper             -> scrollPosition,                          line 3275
//   _ClassMapper                -> animationWindowExpanded,                 line 3277
//   mockMapper                  -> animatorWindowExpanded,                  line 3279
//   instanceMapper              -> layersExpanded,                          line 3281
//   m_FieldMapper               -> parametersExpanded,                      line 3283
//   _AttributeMapper            -> typeIndicatorExpanded,                   line 3285
//   _ClientMapper               -> nodesExpanded,                           line 3287
//   configMapper                -> transitionsExpanded,                     line 3289
//   m_DescriptorMapper          -> graphColorsExpanded,                     line 3291
//   templateMapper              -> nodeColorsExpanded,                      line 3293
//   m_MessageMapper             -> defaultLayerOptionsExpanded,             line 3295
//   collectionMapper            -> colorsExpanded,                          line 3297
//   _ParserMapper               -> transitionColorsExpanded,                line 3299
//   [SpecialName] PushTests()   -> IsProSkin (property),                    line 3301
//   CalcTests()                 -> ShowWindow,                              line 3307
//   OnGUI                       -> OnGUI,                                   line 3313
//   IncludeTests                -> DrawBehavioursAndCosmeticsTab, line 3340 (ControllerEditorWindow.Cosmetics.cs)
//   RunTests                    -> DrawDefaultsTab,               line 3610 (ControllerEditorWindow.Defaults.cs)
//   CloneTests                  -> DrawTransitionDefaults,        line 3628 (ControllerEditorWindow.Defaults.cs)
//   LoginTests                  -> DrawStateDefaults,             line 3690 (ControllerEditorWindow.Defaults.cs)
//   ReflectTests                -> DrawOtherDefaults,             line 3794 (ControllerEditorWindow.Defaults.cs)
//   OnEnable                    -> OnEnable,                                line 3841
//   DeleteTests                 -> RebuildTransitionSerializedObject, line 3853 (ControllerEditorWindow.SerializedDefaults.cs)
//   CreateTests                 -> RebuildStateSerializedObject,      line 3875 (ControllerEditorWindow.SerializedDefaults.cs)
//   NewTests                    -> DrawNodeColorField,            line 3909 (ControllerEditorWindow.Defaults.cs)
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// ─── NOT PORTED: licence gates, phone-home and the remote banner ────────────────────────────────
// The vendor's validation backend is permanently shut down, so every gate below can now only fail
// and would lock out legitimate holders. Each is dropped and its guarded body runs unconditionally.
//
//   OrderVisitor(this) as the first statement of OnGUI            line 3315, defined at line 10527
//       The licence gate. It returns false -- suppressing the entire window body -- unless the
//       assembly-wide `listenerAnnotation` flag says a licence was validated, and otherwise draws
//       the key-entry, "Check for License" and "Transfer License" screens, all of which post to the
//       dead backend. It also routes to BugReporter.PostReg() (line 10584) and to the announcement
//       view. Dropped whole; OnGUI now always draws the settings.
//   RevertAnnotation()                                            line 3334, defined at line 10507
//       The licence banner strip: `License: <tier or "Personal">` plus `Authorized For: <name>`,
//       read from the validation response. Nothing but a readout of the dropped gate's state.
//   DefineVisitor()                                               line 3335, defined at line 10978
//       The footer: hamburger menu, update-available button, version label. The update check it
//       drives (AwakeVisitor) queries
//       us-central1-dreadscripts-c6b62.cloudfunctions.net, which no longer resolves. The menu it
//       opens (ReadVisitor, line 11028) also carries the two "Verify/On Display" and
//       "Verify/On Project Load" toggles -- whose backing settings EditorSettings deliberately does
//       not port -- and the "Send Feedback" item that opens BugReporter (line 11038).
//       *** The BugReporter entry point is deliberately NOT wired up. *** Common/BugReporter.cs is
//       ported but intentionally has no UI, because submitting a report transmits a hardware id, a
//       session id and the licence key. Restoring that button would restore the transmission.
//   EditorUtils.setterProcessor.PopHelper(this)                   line 3336
//       Draws the author's promotional banner, downloaded from raw.githubusercontent.com. The
//       RemoteTextureView type is ported, but the `setterProcessor` instance itself lives on
//       EditorUtils (decompiled EditorUtils.cs line 2226) and is not in the package; adding it
//       would mean editing a shared file. Absent rather than stubbed.
//
// ─── DEFERRED ────────────────────────────────────────────────────────────────────────────────────
// This section used to read "blocked on the unported static ControllerEditor god-class". That
// premise is gone: ControllerEditor is now ported across the files in ControllerEditor/, and every
// member listed below except one has landed. Re-derived on 2026-08-05, and the picture is now two
// quite different kinds of problem which were previously conflated.
//
// (a) VISIBILITY, not missing code. The shipped ControllerEditorWindow is a class nested inside
//     ControllerEditor, which is what let it touch ControllerEditor's private statics freely. This
//     port lifts it to a top-level type, so those accesses no longer compile. The members exist and
//     resolve by name; they are `private`. Confirmed by compiling the call sites -- five CS0122
//     "inaccessible due to its protection level" errors, no CS0103 or CS0117. Closing these needs
//     `internal` on three members, in files this port does not own; it needs no new code.
//
//   CloneTests, the copy and paste buttons                   line 3634-3649
//       Needs ControllerEditor.CopyTransitionSettings (line 14693, ControllerEditor.TransitionCopy.cs
//       line 80) and the clipboard field ControllerEditor.copiedTransitionSettings (line 8040,
//       ControllerEditor.State.cs line 627). Both `private static`. The "Restore Defaults" button in
//       the same row IS ported.
//   ReflectTests, the "Sample From Active StateMachine" button  line 3822-3829
//       Needs ControllerEditor.ActiveStateMachine (line 8552, ControllerEditor.ControllerContext.cs
//       line 226), `private static`. The three default-position fields above it are ported.
//
// (b) GENUINELY UNPORTED -- one member, and it is not on ControllerEditor's window side at all.
//
//   IncludeTests, the aw_enableOverride change handler       line 3363-3368
//       Needs TestInitializer (line 15253), the setter for the proxy AnimatorController the
//       Animation-window override drives. Nothing in the package claims that line, and it pulls in
//       three further unported members (InstantiateInitializer, FlushInitializer,
//       forceGameObjectSelectionUpdate). The two fields it clears alongside --
//       overrideAnimationRoot (8282) and overrideAnimationRootActive (8280) -- ARE ported, at
//       ControllerEditor.State.cs lines 1119 and 1112, but are `private`, so this one needs both
//       kinds of fix. The setting still draws and still persists; only the teardown is missing, so
//       a stale override survives the toggle. Full detail at the call site in
//       ControllerEditorWindow.Cosmetics.cs.
//
//   OnGUI      -- nothing further; see the licence section above.
//
// ─── CLOSED SINCE THE LAST PASS ─────────────────────────────────────────────────────────────────
//   ReflectTests, the "Generated Assets Path" row            line 3834-3838
//       Was blocked on EditorUtils.EnableRules (decompiled EditorUtils.cs line 4249). That has
//       landed as EditorUtils.FolderField (EditorUtils.FolderField.cs) and is `internal`, so the
//       row is restored at the end of DrawOtherDefaults with the shipped write-back guard intact.
//       The note that "EditorUtils is a shared file this port may not extend" was the reason it
//       waited; the extension was made by whoever owns that partial, not here.
//
// ─── DELIBERATE DEVIATION ───────────────────────────────────────────────────────────────────────
// Six labels in the cosmetics tab are built by the string extension `CreateResolver`
// (decompiled EditorUtils.cs line 2812), which fills in and returns one process-wide shared
// GUIContent rather than allocating. That extension is not in the package and lives on EditorUtils,
// a shared file. Those call sites use `new GUIContent(text, tooltip)` here instead. This changes
// allocation, not behaviour: at every one of the six sites the content is handed straight to a
// draw call and never retained, so nothing can observe the difference -- and the shared-instance
// version carries an aliasing hazard that this does not.
//
// ─── DECOMPILER ARTIFACTS NOT PORTED ────────────────────────────────────────────────────────────
//   DEOBF-BUG(guessed): the `while (true)` in OnEnable (line 3845) -- see the marker on OnEnable
//       below for what export/ shows, what is written instead, and what would settle it.
//   `[CompilerGenerated]` on NewTests (line 3909) -- NewTests is ordinary hand-written code that
//       the obfuscator merely marked; the attribute is not carried over.
//   `[SpecialName]` on PushTests -- it was a property getter, restored as IsProSkin.
//
// A shipped bug in RebuildTransitionSerializedObject is preserved verbatim; see that method.
//
// Audit status: VERIFIED -- every member this file declares was diffed against export
// ControllerEditor.cs on 2026-08-05.
//   * The whole field run at 3203-3298 was read in one pass and matches name for name, in order,
//     in type and in modifier, including the three string[] initialisers element for element. The
//     single field in that range NOT ported is _BaseMapper (3273), dropped deliberately: it is
//     declared once and read once and assigned nowhere in the assembly, which was re-confirmed by
//     grepping export for every occurrence of the name.
//   * IsProSkin (3301), ShowWindow (3307) including its menu path/priority/title, OnGUI (3313) and
//     OnEnable (3841) match statement for statement. OnGUI is written as a switch where the
//     decompiler emitted if/else over a temporary; same dispatch, same order.
//   * The DEFERRED section was re-derived from scratch rather than carried over, and was materially
//     wrong before this pass -- three of its four entries claimed missing code that has since
//     landed. Each entry's current blocker was confirmed by compiling the call site and reading the
//     error, not by inspection.
//   * The NOT PORTED (licence) section was re-checked against OnGUI at 3313-3339: OrderVisitor,
//     RevertAnnotation, DefineVisitor and the setterProcessor banner are the four calls dropped,
//     and they are the four the shipped body makes. Nothing else in this file's range is omitted.
// One deviation from export/ is deliberate and marked DEOBF-BUG(guessed); see OnEnable.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The Controller Editor's settings window: every persisted preference of the tool, split
    /// across a behaviours-and-cosmetics tab and a defaults tab.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Almost all of its state is <c>static</c>, including the scroll position and every foldout
    /// flag. That is not an oversight in the reconstruction -- the shipped class is written that
    /// way, and the effect is that closing and reopening the window restores the exact view the
    /// user left, at the cost of two windows never being independently scrollable. Only
    /// <see cref="OnGUI"/> and <see cref="OnEnable"/> are instance members.
    /// </para>
    /// <para>
    /// The two <c>SerializedObject</c>s are the reason this window needs a lifecycle at all. The
    /// default state and default transition are real Unity objects rather than settings values, so
    /// they are edited through <see cref="SerializedProperty"/> fields the way the Animator
    /// window's own inspectors edit them -- which is what makes the fields below look like a
    /// transcription of <c>AnimatorState</c>'s serialised layout, because they are.
    /// </para>
    /// </remarks>
    internal partial class ControllerEditorWindow : EditorWindow
    {
        // ---- Animator targeting, read from elsewhere in the assembly ----

        /// <summary>
        /// The avatar Animator that mask building and the quick-input window target by default.
        /// </summary>
        /// <remarks>
        /// Read by the mask builders and by the quick-input window (decompiled lines 9806, 16690,
        /// 16732), which is why it is <c>internal</c> rather than private despite only ever being
        /// assigned here and by those same builders when they resolve an animator themselves.
        /// </remarks>
        internal static Animator targetAnimator;

        /// <summary>
        /// When set, <see cref="targetAnimator"/> is used even where the caller could have inferred
        /// an animator from the current selection.
        /// </summary>
        internal static bool alwaysUseTargetAnimator;

        // ---- Tab selection ----

        private static int selectedTab;

        private static readonly string[] tabLabels = new string[2] { "Behaviours & Cosmetics", "Defaults" };

        private static int selectedDefaultsTab;

        private static readonly string[] defaultsTabLabels = new string[3] { "Transition", "State", "Other" };

        /// <summary>
        /// Deliberately empty: the popups it feeds are always drawn disabled, purely so the
        /// parameter text fields get the same dropdown chrome the Animator window's own state
        /// inspector has.
        /// </summary>
        private static readonly string[] emptyDropdownOptions = Array.Empty<string>();

        // ---- The default AnimatorState, and its serialised properties ----

        private static SerializedObject stateObject;

        private static SerializedProperty stateName;

        private static SerializedProperty stateSpeed;

        private static SerializedProperty stateCycleOffset;

        private static SerializedProperty stateIkOnFeet;

        private static SerializedProperty stateWriteDefaults;

        private static SerializedProperty stateMirror;

        private static SerializedProperty stateSpeedParameterActive;

        private static SerializedProperty stateMirrorParameterActive;

        private static SerializedProperty stateCycleOffsetParameterActive;

        private static SerializedProperty stateTimeParameterActive;

        private static SerializedProperty stateMotion;

        private static SerializedProperty stateTag;

        private static SerializedProperty stateSpeedParameter;

        private static SerializedProperty stateMirrorParameter;

        private static SerializedProperty stateCycleOffsetParameter;

        private static SerializedProperty stateTimeParameter;

        // ---- The default AnimatorStateTransition, and its serialised properties ----

        private static SerializedObject transitionObject;

        private static SerializedProperty transitionSolo;

        private static SerializedProperty transitionMute;

        private static SerializedProperty transitionDuration;

        private static SerializedProperty transitionOffset;

        private static SerializedProperty transitionExitTime;

        private static SerializedProperty transitionHasExitTime;

        private static SerializedProperty transitionHasFixedDuration;

        private static SerializedProperty transitionInterruptionSource;

        private static SerializedProperty transitionOrderedInterruption;

        private static SerializedProperty transitionCanTransitionToSelf;

        // ---- View state ----

        private static Vector2 scrollPosition;

        private static bool animationWindowExpanded;

        private static bool animatorWindowExpanded;

        private static bool layersExpanded;

        private static bool parametersExpanded;

        private static bool typeIndicatorExpanded;

        private static bool nodesExpanded;

        private static bool transitionsExpanded;

        private static bool graphColorsExpanded;

        private static bool nodeColorsExpanded;

        private static bool defaultLayerOptionsExpanded;

        private static bool colorsExpanded;

        private static bool transitionColorsExpanded;

        /// <summary>
        /// Whether the editor is running the dark skin, and so which of the paired light/dark grid
        /// colours applies.
        /// </summary>
        /// <remarks>
        /// Lives on this type only because the settings it disambiguates do. Its real callers are
        /// the Harmony patches that supply the Animator window's grid colours (decompiled lines
        /// 15836 and 15846), which is why it is <c>internal</c>.
        /// </remarks>
        internal static bool IsProSkin
        {
            get
            {
                return EditorGUIUtility.isProSkin;
            }
        }

        /// <summary>
        /// Opens the settings window, focusing it if it is already open.
        /// </summary>
        [MenuItem("DreadTools/Controller Editor/Settings", false, 4950)]
        internal static void ShowWindow()
        {
            GetWindow<ControllerEditorWindow>(utility: false, "Controller Editor Settings", focus: true);
        }

        /// <remarks>
        /// <para>
        /// Both rebuilds run on every enable, unconditionally. The shipped code guards them with a
        /// <c>_BaseMapper</c> flag (line 3273) that nothing in the assembly ever assigns -- it is
        /// declared once and read once, both verified against export/ -- so the guard is always
        /// taken, and it is dropped here along with the dead field.
        /// </para>
        /// <para>
        /// DEOBF-BUG(guessed). export/ shows the guarded body as a non-terminating loop:
        /// <c>if (!_BaseMapper) { while (true) { RebuildTransitionSerializedObject();
        /// RebuildStateSerializedObject(); } }</c>, whose body contains no break, return or throw.
        /// What is written instead is the two calls, once, unguarded.
        /// </para>
        /// <para>
        /// This matches the known de4dot fault exactly -- a Reactor-flattened <c>if</c> recovered as
        /// a <c>while</c> -- which was itself established elsewhere by tracing the obfuscated IL.
        /// Two things make the reconstruction safe: the loop body is carried over unchanged, so
        /// nothing about its <em>shape</em> is being guessed, and the loop as written would hang the
        /// editor thread the first time the settings window was opened, which the shipped tool
        /// plainly did not do. What remains genuinely unestablished is only whether the body runs
        /// once or not at all, and the never-assigned <c>_BaseMapper</c> settles that as "once".
        /// </para>
        /// <para>
        /// It is marked (guessed) rather than (resolved) because neither form of evidence the
        /// project accepts for (resolved) is available here: there is no IL trace of this specific
        /// method, and ControllerEditor ships a single build, so there is no second decompilation of
        /// it to diff against. An IL trace of this method is the only thing that would settle it.
        /// export/ will keep showing the loop until de4dot's control-flow recovery changes -- do not
        /// "fix" this back to match it.
        /// </para>
        /// </remarks>
        private void OnEnable()
        {
            RebuildTransitionSerializedObject();
            RebuildStateSerializedObject();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            selectedTab = GUILayout.Toolbar(selectedTab, tabLabels, "toolbarbutton");
            switch (selectedTab)
            {
                case 0:
                    DrawBehavioursAndCosmeticsTab();
                    break;
                case 1:
                    DrawDefaultsTab();
                    break;
            }

            // In the shipped build this rule separated the settings from the licence banner and the
            // version footer beneath it; with those gone it closes the window instead.
            EditorUtils.Separator();

            EditorGUILayout.EndScrollView();
        }
    }
}
