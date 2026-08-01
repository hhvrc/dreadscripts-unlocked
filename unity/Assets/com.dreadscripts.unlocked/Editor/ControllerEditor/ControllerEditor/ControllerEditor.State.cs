// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the field bank of the outer ControllerEditor class, lines 7962-8499 of the current
// snapshot -- 267 static fields and one instance field. Line numbers move with the snapshot; the
// decompiled names below are the durable reference, and every later port of a ControllerEditor
// member should map its field references through this table rather than re-deriving names.
//
// ============================ CORRECTION: THE SHIPPED TYPE IS AN EditorWindow ====================
//
// ControllerEditor is not a static class in the assembly. It has instance members -- `private void
// OnGUI()` (line 8583), `private void OnEnable()` (line 8857), `private void CustomizeVisitor()`
// (line 11948) -- it is opened with `EditorWindow.GetWindow<ControllerEditor>(false,
// " Controller Editor", true)` from the menu item at line 8577, and `producerAnnotation` (line 8266)
// is an instance field. It is the tool's *main* window; the ControllerEditorWindow type already in
// the package is the separate *settings* window that was nested inside it.
//
// It is nevertheless reconstructed here as `internal static partial class`, because the sibling
// ControllerEditor.*.cs files in this folder had already been written that way and a partial type
// cannot be static in one declaration and not in another. Reversing them was outside this port's
// scope. The consequence is recorded here rather than hidden:
//
//   DELIBERATE DEVIATION 1 -- `producerAnnotation` (line 8266) is ported as `static
//   copyDestination`. Shipped, it is per-window and resets when the window is closed; here it is
//   process-wide and survives, exactly like the neighbouring settings in the same GUI row already
//   do. Behaviourally this makes one control in the Copy panel consistent with the ten around it,
//   which is almost certainly what the author meant, but it is still a change and is not presented
//   as faithful.
//
//   DELIBERATE DEVIATION 2 -- `OnGUI`, `OnEnable`, `CustomizeVisitor` and the `GetWindow` menu item
//   cannot be ported at all while the type is static, so the tool's main window is currently
//   unreachable. Nothing in the package calls them yet, so nothing is broken today, but whoever
//   ports the window must first change every `internal static partial class ControllerEditor` in
//   this folder to `internal partial class ControllerEditor : EditorWindow` -- at which point
//   `copyDestination` should go back to being an instance field and DEVIATION 1 disappears.
//
//   -- Window and controller context (decompiled 7962-8016) --
//   m_Base           -> activeWindow,                     line 7962
//   _Container       -> currentController,                line 7964
//   @class           -> rootStateMachine,                 line 7966
//   _Mock            -> activeStateMachine,               line 7968
//   field            -> windowScroll,                     line 7970
//   _Attribute       -> thresholdControlCounter,          line 7972
//   _Client          -> writeDefaultsPanelOpen,           line 7974
//   m_Config         -> subAssetPanelOpen,                line 7976
//   _Descriptor      -> stateSectionVisible,              line 7978
//   m_Template       -> anyStateNodeSelected,             line 7980
//   m_Message        -> entryNodeSelected,                line 7982
//   collection       -> exitNodeSelected,                 line 7984
//   m_Parser         -> hasStateTransitionSelected,       line 7986
//   _Manager         -> hasPlainTransitionSelected,       line 7988
//   specification    -> showSharedConditions,             line 7990
//   _Method          -> exitTransitionNames,              line 7992
//   _Schema          -> parameterNames,                   line 7994
//   broadcaster      -> boolParameterNames,               line 7996
//   _Proxy           -> floatParameterNames,              line 7998
//   @struct          -> copiedBehaviours,                 line 8000
//   _Service         -> assetInventory,                   line 8002
//   global           -> mixedValueTransitionPair,         line 8004
//   task             -> mixedValueTransitionSerialized,   line 8006
//   _Process         -> emptyNodeSelection,               line 8008
//   _Producer        -> emptyEdgeSelection,               line 8010
//   _Iterator        -> conditionEditorTransitions,       line 8012
//   _Publisher       -> selectedStatesSerialized,         line 8014
//   m_Proc           -> transitionInspectorSerialized,    line 8016
//
//   -- Graph and object selection (decompiled 8018-8044) --
//   m_WrapperAnnotation      -> selectedStateMachines,          line 8018
//   _AnnotationAnnotation    -> multiTransitionStateMachines,   line 8020
//   _VisitorAnnotation       -> multiTransitionStates,          line 8022
//   m_AlgoAnnotation         -> selectedStates,                 line 8024
//   m_MapperAnnotation       -> selectedNodes,                  line 8026
//   _InitializerAnnotation   -> selectedEdges,                  line 8028
//   _DefinitionAnnotation    -> selectedTransitionEdits,        line 8030
//   regAnnotation            -> exitNodeIncomingTransitions,    line 8032
//   _TestsAnnotation         -> selectedStateTransitions,       line 8034
//   propertyAnnotation       -> selectedTransitions,            line 8036
//   _ProcessorAnnotation     -> focusedTransition,              line 8038
//   _ObserverAnnotation      -> copiedTransitionSettings,       line 8040
//   serverAnnotation         -> pendingTransitionEdits,         line 8042
//   m_ThreadAnnotation       -> copiedConditions,               line 8044
//
//   -- State node cosmetics (decompiled 8046-8054) --
//   m_PolicyAnnotation       -> stateStylesByTag,          line 8046
//   m_SerializerAnnotation   -> cosmeticOnlyStyleNames,    line 8048
//   _PageAnnotation          -> styleMenuNames,            line 8050
//   m_ResolverAnnotation     -> unityNodeStyleCache,       line 8052
//   predicateAnnotation      -> defaultStateNodeStyle,     line 8054
//
//   -- Behaviour multi-editing (decompiled 8056-8064) --
//   rulesAnnotation          -> trackingControlEditor,             line 8056
//   _QueueAnnotation         -> parameterDriverEditors,            line 8058
//   _ErrorAnnotation         -> parameterDriverList,               line 8060
//   setterAnnotation         -> parameterDriverBindings,           line 8062
//   m_ConnectionAnnotation   -> allStatesHaveTrackingControl,      line 8064
//
//   -- AnimatorControllerTool member handles (decompiled 8066-8076) --
//   DECLARED ELSEWHERE, see the DECLARED ELSEWHERE section below.
//
//   -- Bug report, feedback, licensing and update banner (decompiled 8078-8164; see the note below) --
//   m_WatcherAnnotation      -> bugReporterOpen,                 line 8078
//   candidateAnnotation      -> unusedLicensingFlag,             line 8080
//   _ProductAnnotation       -> unusedLicensingText,             line 8082
//   m_ExpressionAnnotation   -> feedbackPanelOpen,               line 8084
//   systemAnnotation         -> isSendingFeedback,               line 8086
//   m_WorkerAnnotation       -> feedbackText,                    line 8088
//   m_FilterAnnotation       -> licenseUsername,                 line 8090
//   stubAnnotation           -> licensedToDisplayName,           line 8092
//   m_ReaderAnnotation       -> licenseVariant,                  line 8094
//   m_BridgeAnnotation       -> licenseKey,                      line 8096
//   strategyAnnotation       -> transferVerificationCode,        line 8098
//   m_CustomerAnnotation     -> transferTargetEmail,             line 8100
//   databaseAnnotation       -> sessionId,                       line 8102
//   m_ExporterAnnotation     -> serverWarnedTooManyAttempts,     line 8104
//   m_IdentifierAnnotation   -> licenseKeyEntryRequired,         line 8106
//   attrAnnotation           -> licenseCheckRetryOffered,        line 8108
//   m_DispatcherAnnotation   -> licenseCheckedThisSession,       line 8110
//   m_RegistryAnnotation     -> retryAllowedAtRealtime,          line 8112
//   m_TagAnnotation          -> currentDateStamp,                line 8114
//   importerAnnotation       -> isActivatingLicense,             line 8116
//   _RequestAnnotation       -> isVerifyingLicense,              line 8118
//   printerAnnotation        -> unreadDeviceDateFingerprint,     line 8120
//   _WriterAnnotation        -> hardwareId,                      line 8122
//   m_ParamsAnnotation       -> licenseToken,                    line 8124
//   listenerAnnotation       -> isLicensed,                      line 8126
//   m_GetterAnnotation       -> licenseRestoredFromCache,        line 8128
//   m_InterceptorAnnotation  -> licensedCallbacksFlushed,        line 8130
//   creatorAnnotation        -> pendingLicensedCallbacks,        line 8132
//   m_EventAnnotation        -> pendingResetCallbacks,           line 8134
//   infoAnnotation           -> repaintTargetTypes,              line 8136
//   facadeAnnotation         -> showingTransferPanel,            line 8142
//   advisorAnnotation        -> transferCodeSent,                line 8144
//   m_CallbackAnnotation     -> isRequestingTransferCode,        line 8146
//   _IndexerAnnotation       -> isConfirmingTransfer,            line 8148
//   _IssuerAnnotation        -> isDownloadingUpdate,             line 8150
//   _PrototypeAnnotation     -> isCheckingForUpdate,             line 8152
//   _RuleAnnotation          -> hasCheckedForUpdate,             line 8154
//   singletonAnnotation      -> updateAvailable,                 line 8156
//   factoryAnnotation        -> updateFoldout,                   line 8158
//   _AccountAnnotation       -> announcementFoldout,             line 8160
//   m_RefAnnotation          -> OMITTED (version),               line 8162
//   statusAnnotation         -> extraMenuLinks,                  line 8164
//
//   -- Condition editing (decompiled 8166-8178) --
//   _TokenAnnotation     -> sharedConditionEditors,    line 8166
//   m_CodeAnnotation     -> allConditionEditors,       line 8168
//   m_DicAnnotation      -> focusedConditionEditors,   line 8170
//   invocationAnnotation -> sharedConditionList,       line 8172
//   _RoleAnnotation      -> allConditionList,          line 8174
//   m_ParamAnnotation    -> focusedConditionList,      line 8176
//   _ModelAnnotation     -> subAssetTabIndex,          line 8178
//
//   -- AnimatorState inspector properties (decompiled 8180-8210) --
//   m_TokenizerAnnotation      -> stateNameProperty,                     line 8180
//   decoratorAnnotation        -> stateTagProperty,                      line 8182
//   m_ComparatorAnnotation     -> stateMotionProperty,                   line 8184
//   m_ExceptionAnnotation      -> stateSpeedProperty,                    line 8186
//   objectAnnotation           -> stateSpeedParameterProperty,           line 8188
//   _UtilsAnnotation           -> stateTimeParameterProperty,            line 8190
//   valAnnotation              -> stateMirrorProperty,                   line 8192
//   m_ValueAnnotation          -> stateCycleOffsetProperty,              line 8194
//   _MerchantAnnotation        -> stateIkOnFeetProperty,                 line 8196
//   m_AuthenticationAnnotation -> stateWriteDefaultsProperty,            line 8198
//   m_ReponseAnnotation        -> stateSpeedParameterActiveProperty,     line 8200
//   _PoolAnnotation            -> stateTimeParameterActiveProperty,      line 8202
//   m_ParameterAnnotation      -> stateMirrorParameterActiveProperty,    line 8204
//   _ComposerAnnotation        -> stateCycleOffsetParameterActiveProperty, line 8206
//   _RepositoryAnnotation      -> stateMirrorParameterProperty,          line 8208
//   m_MappingAnnotation        -> stateCycleOffsetParameterProperty,     line 8210
//
//   -- Bulk transition graph modes (decompiled 8212-8216) --
//   baseAnnotation      -> replicateTransitionsMode,       line 8212
//   containerAnnotation -> redirectTransitionsMode,        line 8214
//   m_ClassAnnotation   -> makeMultipleTransitionsMode,    line 8216
//
//   -- AnimatorStateTransition inspector properties (decompiled 8218-8238) --
//   _MockAnnotation         -> transitionHasExitTimeProperty,         line 8218
//   instanceAnnotation      -> transitionExitTimeProperty,            line 8220
//   _FieldAnnotation        -> transitionHasFixedDurationProperty,    line 8222
//   attributeAnnotation     -> transitionDurationProperty,            line 8224
//   m_ClientAnnotation      -> transitionOffsetProperty,              line 8226
//   configAnnotation        -> transitionInterruptionSourceProperty,  line 8228
//   m_DescriptorAnnotation  -> transitionOrderedInterruptionProperty, line 8230
//   _TemplateAnnotation     -> transitionCanTransitionToSelfProperty, line 8232
//   _MessageAnnotation      -> transitionSoloProperty,                line 8234
//   m_CollectionAnnotation  -> transitionMuteProperty,                line 8236
//   parserAnnotation        -> transitionSectionVisible,              line 8238
//
//   -- Animatable-property caches (decompiled 8240-8244) --
//   m_ManagerAnnotation        -> componentTypes,                line 8240
//   m_ItemAnnotation           -> animatablePropertiesByType,    line 8242
//   _SpecificationAnnotation   -> materialPropertiesByShader,    line 8244
//
//   -- Layer and parameter batch actions (decompiled 8246-8266) --
//   m_MethodAnnotation       -> actionTargetController,      line 8246
//   _SchemaAnnotation        -> actionSourceName,            line 8248
//   m_BroadcasterAnnotation  -> actionReplacementName,       line 8250
//   proxyAnnotation          -> actionFilterText,            line 8252
//   structAnnotation         -> matchWholeWord,              line 8254
//   serviceAnnotation        -> addRequiredParameters,       line 8256
//   _StateAnnotation         -> copiedParameterSuffix,       line 8258
//   m_GlobalAnnotation       -> selectedAction,              line 8260
//   _TaskAnnotation          -> actionScope,                 line 8262
//   m_ProcessAnnotation      -> copySourceScope,             line 8264
//   producerAnnotation       -> copyDestination,             line 8266  (INSTANCE field)
//
//   -- Animation window integration (decompiled 8268-8312) --
//   _IteratorAnnotation        -> animationWindow,                     line 8268
//   m_PublisherAnnotation      -> previewRoot,                         line 8270
//   m_ConfigurationAnnotation  -> previewAnimator,                     line 8272
//   procAnnotation             -> unusedPreviewObject,                 line 8274
//   wrapperVisitor             -> forceGameObjectSelectionUpdate,      line 8276
//   annotationVisitor          -> overrideAnimationController,         line 8278
//   m_VisitorVisitor           -> overrideAnimationRootActive,         line 8280
//   _AlgoVisitor               -> overrideAnimationRoot,               line 8282
//   pageVisitor                -> propertyEditingMenuAllowed,          line 8308
//   resolverVisitor            -> interactedHierarchyNodes,            line 8310
//   m_PredicateVisitor         -> unusedGraphRect,                     line 8312
//
//   -- Graph rendering reflection (decompiled 8314-8350) --
//   m_SetterVisitor      -> graphBackgroundStyleField,     line 8320
//   connectionVisitor    -> graphBackgroundTexture,        line 8322
//   m_ContextVisitor     -> animatedEdgeArrowPoints,       line 8324
//   m_RecordVisitor      -> arrowLerpEnabled,              line 8326
//   m_HelperVisitor      -> animatingSelectedEdges,        line 8328
//   _ExpressionVisitor   -> repaintGraphRequested,         line 8342
//   _SystemVisitor       -> rebuildGraphRequested,         line 8344
//   filterVisitor        -> insideGraphGui,                line 8348
//   m_StubVisitor        -> layerControllerView,           line 8350
//
//   -- Layer view (decompiled 8352-8404) --
//   _TagVisitor          -> templateDropdownArmed,             line 8372
//   importerVisitor      -> categoryLayerScroll,               line 8374
//   requestVisitor       -> layerTemplateControllers,          line 8376
//   m_PrinterVisitor     -> layerTemplateNames,                line 8378
//   writerVisitor        -> layerViewType,                     line 8380
//   m_ParamsVisitor      -> layerCategoryRoot,                 line 8382
//   listenerVisitor      -> currentLayerCategory,              line 8384
//   m_GetterVisitor      -> categoryLayerList,                 line 8386
//   m_InterceptorVisitor -> unityLayerList,                    line 8388
//   creatorVisitor       -> categoryNames,                     line 8390
//   eventVisitor         -> categoryViewDrewLayerList,         line 8392
//   _InfoVisitor         -> frameLayerRequested,               line 8394
//   _FacadeVisitor       -> drawLayerCallback,                 line 8396
//   _AdvisorVisitor      -> selectLayerCallback,               line 8398
//   _CallbackVisitor     -> mouseUpLayerCallback,              line 8400
//   indexerVisitor       -> layerRenameOverlay,                line 8402
//   _IssuerVisitor       -> stateRenameOverlay,                line 8404
//
//   -- Graph node menus, transition dragging and layer context (decompiled 8406-8498) --
//   m_SingletonVisitor   -> contextMenu,                            line 8410
//   m_ComparatorVisitor  -> OMITTED (queryAlgoPatchMethod),         line 8438
//   exceptionVisitor     -> slotDragSourceNode,                     line 8440
//   m_ObjectVisitor      -> slotDragActive,                         line 8442
//   m_UtilsVisitor       -> transitionDragArmed,                    line 8444
//   _ValVisitor          -> transitionDragPending,                  line 8446
//   valueVisitor         -> slotDraggingEnded,                      line 8448
//   merchantVisitor      -> placeholderTransition,                  line 8450
//   m_AuthenticationVisitor -> placeholderTransitionTarget,         line 8452
//   _ReponseVisitor      -> currentNodeSize,                        line 8454
//   poolVisitor          -> dragAndDropPending,                     line 8456
//   parameterVisitor     -> quickToggleState,                       line 8458
//   m_ComposerVisitor    -> parameterViewParameters,                line 8460
//   _RepositoryVisitor   -> parameterViewScrollField,               line 8462
//   mappingVisitor       -> parameterControllerViewType,            line 8464
//   m_BaseVisitor        -> unusedNodeIndex,                        line 8466
//   m_ContainerVisitor   -> categoryMenuMousePosition,              line 8468
//   classVisitor         -> pendingTransitionSourceNode,            line 8470
//   _MockVisitor         -> pendingTransitionSourceKind,            line 8472
//   _InstanceVisitor     -> nodeContextClickPending,                line 8474
//   _FieldVisitor        -> replaceTransitionsDefault,              line 8476
//   _AttributeVisitor    -> replaceTransitions,                     line 8478
//   clientVisitor        -> reverseModifiesValues,                  line 8480
//   m_ConfigVisitor      -> contextLayerIndex,                      line 8482
//   m_DescriptorVisitor  -> resumeTransitionDragAfterSlotDrag,      line 8484
//   templateVisitor      -> blendTreeBreadcrumbState,               line 8486
//   m_MessageVisitor     -> layerContextController,                 line 8488
//   collectionVisitor    -> copiedLayer,                            line 8490
//
//   -- Members ported alongside the bank (decompiled 8500-8575) --
//   PatchWrapper     -> RepaintWindow,           line 8500
//   SetInitializer   -> HasFocusedTransition,    line 8571
//
// Names were derived from each field's call sites across decompiled lines 8500-18535, not from the
// obfuscated identifiers, which come from a design-pattern word list and are systematically
// misleading: `field` is a scroll position, `task` is a SerializedObject, `broadcaster` is an array
// of bool parameter names, `collection` is a bool, and the entire `*Annotation` / `*Visitor` suffix
// split is meaningless -- both families contain licensing state, reflection handles and GUI flags
// alike.
//
// DECLARED ELSEWHERE (57 of the 267): the pure reflection handles of this bank -- decompiled lines
// 8066-8076, 8284-8306, 8314-8318, 8330-8340, 8346, 8352-8370, 8406-8408, 8412-8436 and 8492-8498 --
// are declared in ControllerEditor.ReflectionPriming.cs, beside the six eager priming methods that
// fill them, and its own header carries the full obfuscated-name table for them. They are omitted
// here rather than duplicated. Their names there differ from this file's convention in that they
// keep Unity's own casing for the initialisms (`graphGUIType`, not `graphGuiType`); when they are
// referenced from a later port, that file's spelling is the correct one. The 208 fields below are
// everything else in 7962-8499 apart from the two genuine omissions that follow.
//
// OMISSIONS (nothing else in 7962-8499 is left out):
//   * `m_RefAnnotation` (line 8162), `new VersionNumber("3.3.2")`. The `VersionNumber` type is not
//     ported yet -- the package has `DreadScripts.Common.SemVer`, which is the ADOverhaul build's
//     equivalent but a different type, and substituting it would be an invention. The field is the
//     tool's own version, compared against `u_updateVersion` and sent with update requests. Declare
//     it here once VersionNumber lands.
//   * `m_ComparatorVisitor` (line 8438), `HarmonyPatchManager.NewReg<AnimatorState>(QueryAlgo)`. Its
//     initialiser names `QueryAlgo` (the state-created callback, decompiled line 16038), which is
//     not ported. It is the MethodInfo emitted by a transpiler at decompiled line 17222. Declaring
//     it without its initialiser would change behaviour, so it is left out entirely.
//   * The six `[SpecialName]` accessors at lines 8508-8569 -- `LogoutMapper`/`PatchMapper`
//     (ActiveController), `ManageMapper`/`PrintMapper` (RootStateMachine), `RevertMapper`/
//     `OrderInitializer` (ActiveStateMachine). They are NOT pure accessors: each getter lazily
//     initialises through a helper and each setter fires a change notification, and the notifiers
//     live outside this region -- `InstantiateAnnotation` (9703), `DisableMapper` (16776),
//     `DefineAnnotation` (9655), `FlushAnnotation` (9732), `RemoveAnnotation` (9698),
//     `RestartVisitor` (10837). `DisableMapper` in particular rebuilds the whole layer-category tree
//     and depends on a dozen unported members. Porting the getters without them would silently drop
//     the lazy initialisation, and stubbing is not allowed, so all six are deferred to whoever ports
//     `DisableMapper`. When they land they should be:
//         LogoutMapper/PatchMapper       -> internal static AnimatorController ActiveController
//         ManageMapper/PrintMapper       -> private  static AnimatorStateMachine RootStateMachine
//         RevertMapper/OrderInitializer  -> private  static AnimatorStateMachine ActiveStateMachine
//     Several already-ported files list `LogoutMapper()` as a blocker; `ActiveController` is the
//     name they should compile against.
//
// ILSpy artifact noted, not ported: `PatchMapper` (line 8519) decompiles as
// `if (_Container != v) { while (true) { _Container = v; DisableMapper(); } }`. The `while (true)`
// is a decompilation artifact around a straight-line body, not an infinite loop -- the same shape
// appears throughout this assembly wherever a two-statement block follows an inequality test.
//
// The two `_003C_003Ec__DisplayClass*` structs immediately preceding line 7962 are compiler-
// generated capture classes belonging to methods in other regions and are not part of this port.
// No obfuscator scaffolding (always-null statics paired with null-check predicates, marker types)
// was found inside 7962-8499.
//
// LICENSING FIELDS ARE DECLARED, THE LICENSING CODE IS NOT. Roughly thirty of the fields below
// exist only to drive the vendor's activation/verification/transfer flow against a server that has
// been shut down; with the backend gone those checks can now only ever fail, which would lock out
// legitimate license holders. They are declared because the members that read them are interleaved
// with functional code -- the version banner, the settings pane and several graph headers sit in
// methods that also touch `isLicensed` -- so later ports of those regions need the names to exist
// and to agree. Declaring a field commits to nothing; whether any given reader is ported is that
// reader's decision. `hardwareId`, `sessionId`, `licenseKey`, `currentDateStamp` and
// `unreadDeviceDateFingerprint` are only ever populated by machine-fingerprinting code (decompiled
// 10222-10260, 10424-10440) that is not ported and should not be.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The Controller Editor itself: the shared state behind the Harmony patches that rewrite
    /// Unity's Animator window, plus the small standalone window that hosts the bulk-edit panels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Almost the whole tool lives in patches on <c>UnityEditor.Graphs</c> types rather than in a
    /// window of its own, so nearly every field here is <c>static</c>: a patch is a static method
    /// with no instance to hang state on. This file declares that state and nothing else; the
    /// members that read and write it are ported into sibling <c>ControllerEditor.*.cs</c> files.
    /// </para>
    /// <para>
    /// The type derives from <see cref="EditorWindow"/> because the shipped one does -- see the file
    /// header. Only a handful of its members are instance members, and until they are ported the
    /// window has no <c>OnGUI</c> and will draw empty.
    /// </para>
    /// </remarks>
    internal static partial class ControllerEditor
    {
        #region Window and controller context

        /// <summary>
        /// The open Controller Editor window, assigned from <c>OnEnable</c>. Used only to repaint;
        /// see <see cref="RepaintWindow"/>.
        /// </summary>
        private static EditorWindow activeWindow;

        /// <summary>
        /// Backing field for the controller the tool is editing, read from the Animator window's
        /// <c>AnimatorControllerTool</c>.
        /// </summary>
        /// <remarks>
        /// The shipped property that lazily fills this in and rebuilds the layer view when it
        /// changes is not ported yet -- see the header. Until it is, treat this field as read-only
        /// and assign it only through that property.
        /// </remarks>
        private static UnityEditor.Animations.AnimatorController currentController;

        /// <summary>
        /// The controller's root state machine, i.e. the state machine of the selected layer rather
        /// than whichever sub-machine is currently open.
        /// </summary>
        private static AnimatorStateMachine rootStateMachine;

        /// <summary>
        /// The state machine the graph is currently showing, which may be a sub-machine several
        /// breadcrumbs deep inside <see cref="rootStateMachine"/>.
        /// </summary>
        private static AnimatorStateMachine activeStateMachine;

        /// <summary>Scroll position of the window's single outer scroll view.</summary>
        private static Vector2 windowScroll;

        /// <summary>
        /// Running counter used to give each condition threshold field a unique GUI control name
        /// ("Threshold0", "Threshold1", ...), reset to zero at the top of every <c>OnGUI</c>.
        /// </summary>
        /// <remarks>
        /// The names exist so keyboard focus can be moved between threshold fields explicitly;
        /// IMGUI has no other way to address a control that is drawn in a loop.
        /// </remarks>
        private static int thresholdControlCounter;

        /// <summary>Whether the "Write Defaults" audit panel is expanded.</summary>
        private static bool writeDefaultsPanelOpen;

        /// <summary>Whether the "Explore Controller Sub-Assets" panel is expanded.</summary>
        private static bool subAssetPanelOpen;

        /// <summary>
        /// Whether the state-editing section is shown. Sticky: once any state is selected it stays
        /// on for the rest of the session unless the corresponding setting turns it off.
        /// </summary>
        private static bool stateSectionVisible;

        /// <summary>Whether the graph's Any State node is part of the current selection.</summary>
        private static bool anyStateNodeSelected;

        /// <summary>Whether the graph's Entry node is part of the current selection.</summary>
        private static bool entryNodeSelected;

        /// <summary>Whether the graph's Exit node is part of the current selection.</summary>
        private static bool exitNodeSelected;

        /// <summary>
        /// Whether the Unity selection contains at least one <see cref="AnimatorStateTransition"/>.
        /// </summary>
        private static bool hasStateTransitionSelected;

        /// <summary>
        /// Whether the Unity selection contains at least one plain <see cref="AnimatorTransition"/>,
        /// the kind that leaves Entry or a state machine rather than a state.
        /// </summary>
        private static bool hasPlainTransitionSelected;

        /// <summary>
        /// Whether the condition editor is showing only the conditions common to every selected
        /// transition ("Shared Conditions") rather than all of them ("All Conditions").
        /// </summary>
        private static bool showSharedConditions = true;

        /// <summary>
        /// Names of the Any State transitions of <c>RootStateMachine</c> that are exit transitions.
        /// Rebuilt whenever the root state machine changes.
        /// </summary>
        private static string[] exitTransitionNames;

        /// <summary>Every parameter name on the current controller, in declaration order.</summary>
        private static string[] parameterNames;

        /// <summary>The subset of <see cref="parameterNames"/> whose parameters are bools.</summary>
        private static string[] boolParameterNames;

        /// <summary>The subset of <see cref="parameterNames"/> whose parameters are floats.</summary>
        private static string[] floatParameterNames;

        /// <summary>
        /// The behaviours held by the "Behaviours/Copy" clipboard, pasted onto every selected state
        /// and state machine by "Behaviours/Paste".
        /// </summary>
        private static StateMachineBehaviour[] copiedBehaviours;

        /// <summary>
        /// Every sub-asset of the current controller, grouped by kind. Rebuilt whenever the
        /// sub-asset or write-defaults panel is opened or the controller changes.
        /// </summary>
        private static ControllerAssetInventory assetInventory;

        /// <summary>
        /// Two throwaway transitions built with deliberately opposite values for every property.
        /// </summary>
        /// <remarks>
        /// They exist only to be wrapped in <see cref="mixedValueTransitionSerialized"/>: a
        /// multi-object <see cref="SerializedObject"/> over two objects that disagree about
        /// everything reports every property as having mixed values, which is how the transition
        /// inspector draws itself as "nothing selected" without a separate code path. The names "a"
        /// and "b" and the values 69/420/80085 elsewhere in this file are the original author's.
        /// </remarks>
        private static AnimatorStateTransition[] mixedValueTransitionPair;

        /// <summary>
        /// <see cref="mixedValueTransitionPair"/> wrapped in a <see cref="SerializedObject"/>, used
        /// as the transition inspector's target when no transition is selected.
        /// </summary>
        private static SerializedObject mixedValueTransitionSerialized;

        /// <summary>
        /// Empty list returned in place of the graph's node selection when the graph cannot be
        /// reached. Never mutated -- it is a shared "no selection" sentinel.
        /// </summary>
        private static readonly List<AnimatorGraphReflection.GraphNodeRef> emptyNodeSelection =
            new List<AnimatorGraphReflection.GraphNodeRef>();

        /// <summary>
        /// <see cref="emptyNodeSelection"/>'s counterpart for the graph's edge selection.
        /// </summary>
        /// <remarks>
        /// A <see cref="ConcurrentBag{T}"/> only because that is the collection type the graph's own
        /// selected-edge accessor returns; nothing here is threaded.
        /// </remarks>
        private static readonly ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef> emptyEdgeSelection =
            new ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef>();

        /// <summary>
        /// The transitions <see cref="allConditionEditors"/> currently has editors for. Kept in step
        /// with <see cref="selectedTransitions"/> by diffing rather than rebuilding, so that an
        /// editor's per-row state survives a selection change that keeps its transition.
        /// </summary>
        private static List<AnimatorTransitionBase> conditionEditorTransitions =
            new List<AnimatorTransitionBase>();

        /// <summary>
        /// The selected states wrapped in one multi-object <see cref="SerializedObject"/>, or null
        /// when no state is selected. Backs the state inspector's <c>state*Property</c> fields.
        /// </summary>
        private static SerializedObject selectedStatesSerialized;

        /// <summary>
        /// Whatever the transition inspector is currently editing: the focused transition, or the
        /// selected state transitions, or <see cref="mixedValueTransitionSerialized"/> when neither.
        /// </summary>
        private static SerializedObject transitionInspectorSerialized;

        #endregion

        #region Graph and object selection

        /// <summary>State machines in the Unity selection.</summary>
        private static AnimatorStateMachine[] selectedStateMachines;

        /// <summary>
        /// Snapshot of <see cref="selectedStateMachines"/> taken on the last selection change that
        /// happened outside "Make Multiple Transitions" mode, so that mode can keep operating on the
        /// set the user had chosen before it started hijacking the selection.
        /// </summary>
        private static AnimatorStateMachine[] multiTransitionStateMachines;

        /// <summary><see cref="multiTransitionStateMachines"/>'s counterpart for states.</summary>
        private static List<AnimatorState> multiTransitionStates = new List<AnimatorState>();

        /// <summary>States in the Unity selection.</summary>
        private static List<AnimatorState> selectedStates = new List<AnimatorState>();

        /// <summary>
        /// The graph's own node selection, which is not the same thing as the Unity selection: it
        /// includes the Entry, Any State and Exit nodes, which are not assets and so can never
        /// appear in <c>Selection.objects</c>.
        /// </summary>
        private static List<AnimatorGraphReflection.GraphNodeRef> selectedNodes;

        /// <summary>The graph's own edge selection, for the same reason.</summary>
        private static ConcurrentBag<AnimatorGraphReflection.GraphEdgeRef> selectedEdges;

        /// <summary>
        /// Every transition carried by <see cref="selectedEdges"/>. One edge is drawn per pair of
        /// nodes and can carry several transitions, so this is generally longer than the edge list.
        /// </summary>
        private static List<AnimatorGraphReflection.TransitionEditionInfo> selectedTransitionEdits;

        /// <summary>
        /// Transitions arriving at the Exit node, offered as a one-click selection.
        /// </summary>
        private static AnimatorTransitionBase[] exitNodeIncomingTransitions;

        /// <summary>State transitions in the Unity selection.</summary>
        private static List<AnimatorStateTransition> selectedStateTransitions =
            new List<AnimatorStateTransition>();

        /// <summary>
        /// <see cref="selectedTransitionEdits"/> reduced to the transitions themselves.
        /// </summary>
        private static List<AnimatorTransitionBase> selectedTransitions =
            new List<AnimatorTransitionBase>();

        /// <summary>
        /// The single transition the condition editor is pinned to, when the user has clicked one
        /// specific transition on a multi-transition edge. Its <c>transition</c> being null means
        /// "not pinned" -- see <see cref="HasFocusedTransition"/>.
        /// </summary>
        private static AnimatorGraphReflection.TransitionEditionInfo focusedTransition;

        /// <summary>
        /// Scratch transition holding the settings captured by the transition inspector's copy
        /// button, pasted onto every selected transition by the paste button. Allocated on first
        /// use and reused thereafter.
        /// </summary>
        private static AnimatorStateTransition copiedTransitionSettings;

        /// <summary>
        /// The transitions the Redirect and Replicate modes will act on when they are confirmed --
        /// a copy of <see cref="selectedTransitionEdits"/> frozen at the moment the mode started, so
        /// that clicking a new target does not also change what is being redirected.
        /// </summary>
        private static List<AnimatorGraphReflection.TransitionEditionInfo> pendingTransitionEdits =
            new List<AnimatorGraphReflection.TransitionEditionInfo>();

        /// <summary>
        /// The condition clipboard: conditions captured by the condition editor's copy button and
        /// appended to every target transition by the paste button.
        /// </summary>
        private static List<AnimatorCondition> copiedConditions = new List<AnimatorCondition>();

        #endregion

        #region State node cosmetics

        /// <summary>
        /// The tool's own state styles by name ("ce_Note", "ce_Dot", ...). A state opts into one by
        /// setting its <c>tag</c> to the style name, which is why the graph patch looks the style up
        /// by tag when it draws a node.
        /// </summary>
        private static readonly Dictionary<string, GUIStyle> stateStylesByTag =
            new Dictionary<string, GUIStyle>();

        /// <summary>
        /// The subset of <see cref="stateStylesByTag"/> that is purely decorative -- sticky notes
        /// and dots, which mark a state as a comment rather than as part of the machine. Several
        /// operations skip states tagged with one of these.
        /// </summary>
        private static readonly HashSet<string> cosmeticOnlyStyleNames = new HashSet<string>();

        /// <summary>
        /// Style names offered in the node context menu's "Styles/" submenu, sorted. Styles
        /// registered as hidden are deliberately absent, and the "ce_" prefix is stripped.
        /// </summary>
        private static readonly List<string> styleMenuNames = new List<string>();

        /// <summary>
        /// Unity's own <c>Styles.m_NodeStyleCache</c>, reached by reflection and written into
        /// directly so the graph resolves the tool's styles through its normal lookup.
        /// </summary>
        /// <remarks>
        /// There is no supported way to add a node style; the cache is keyed by the string the graph
        /// builds from the style name, selection state and colour index, so each registered style
        /// has to be inserted under all four such keys.
        /// </remarks>
        private static Dictionary<string, GUIStyle> unityNodeStyleCache;

        /// <summary>
        /// Fallback style for a state whose tag names no registered style: a 200x40 copy of Unity's
        /// "flow node 0".
        /// </summary>
        private static GUIStyle defaultStateNodeStyle;

        #endregion

        #region Behaviour multi-editing

        /// <summary>
        /// Editor for the VRChat Animator Tracking Control behaviours of the selected states, drawn
        /// only when every selected state has one -- see <see cref="allStatesHaveTrackingControl"/>.
        /// </summary>
        private static TrackingControlEditor trackingControlEditor;

        /// <summary>
        /// One editor per parameter-driver entry across the selection, letting a value be typed once
        /// and applied to every driver that shares that parameter.
        /// </summary>
        private static List<BehaviourPropertyMultiEditor> parameterDriverEditors =
            new List<BehaviourPropertyMultiEditor>();

        /// <summary>The reorderable list that draws <see cref="parameterDriverEditors"/>.</summary>
        private static ReorderableList parameterDriverList;

        /// <summary>
        /// The VRChat Parameter Driver behaviours found on the selected states, one binding each.
        /// </summary>
        private static List<AnimatorTypeCache.ParameterDriverBinding> parameterDriverBindings =
            new List<AnimatorTypeCache.ParameterDriverBinding>();

        /// <summary>
        /// Whether every selected state carries a tracking-control behaviour and there is at least
        /// one. Only then is <see cref="trackingControlEditor"/> meaningful, since the editor edits
        /// the behaviours as a single set.
        /// </summary>
        private static bool allStatesHaveTrackingControl;

        #endregion

        #region Bug report and feedback panels

        /// <summary>
        /// Whether the shared bug-reporter panel is taking over the window. Written through a setter
        /// that resets the reporter when it closes.
        /// </summary>
        private static bool bugReporterOpen;

        /// <summary>
        /// Declared between the bug reporter and the feedback panel and never read or written
        /// anywhere in the assembly.
        /// </summary>
        /// <remarks>
        /// Its position pairs it with the panel-open and in-flight flags around it, which suggests a
        /// third panel that was dropped, but nothing in the assembly supports naming it more
        /// specifically. Kept so the bank matches the original one for one.
        /// </remarks>
        private static bool unusedLicensingFlag;

        /// <summary>
        /// Declared among the licensing strings and never read or written anywhere in the assembly.
        /// Like <see cref="unusedLicensingFlag"/>, its role is not recoverable and is not guessed at
        /// here.
        /// </summary>
        private static string unusedLicensingText;

        /// <summary>Whether the feedback panel is taking over the window.</summary>
        private static bool feedbackPanelOpen;

        /// <summary>Whether a feedback message is in flight, which disables the send button.</summary>
        private static bool isSendingFeedback;

        /// <summary>
        /// The message typed into the feedback panel. Truncated to 2000 characters on submission.
        /// </summary>
        private static string feedbackText;

        #endregion

        #region Licensing

        // Everything in this region belongs to the vendor's activation flow, which talked to a
        // server that has been shut down. The fields are declared so that ports of the surrounding
        // GUI can compile and so their names are agreed; see the file header.

        /// <summary>Account name returned by a successful verification.</summary>
        private static string licenseUsername;

        /// <summary>
        /// The "Authorized For" line: <see cref="licenseUsername"/> with a trailing Discord-style
        /// discriminator and any colour markup stripped, and a leading '@' removed.
        /// </summary>
        private static string licensedToDisplayName;

        /// <summary>
        /// License tier returned by verification. Blank means the tool displays "Personal".
        /// </summary>
        private static string licenseVariant;

        /// <summary>
        /// The license key, cached in EditorPrefs. Validated against a four-group hexadecimal
        /// pattern before it is considered usable.
        /// </summary>
        private static string licenseKey = "";

        /// <summary>The six-digit code entered to confirm a license transfer.</summary>
        private static string transferVerificationCode = "";

        /// <summary>
        /// The email address the transfer code was sent to, echoed back by the server so the panel
        /// can show the user where to look.
        /// </summary>
        private static string transferTargetEmail = "";

        /// <summary>
        /// A per-install identifier persisted in EditorPrefs and sent with every request.
        /// Regenerated if the stored value is not a 32-digit hexadecimal GUID.
        /// </summary>
        private static string sessionId;

        /// <summary>
        /// Sticky flag raised when the server reports that the device is close to being blocked for
        /// repeated failures. Once set it stays set for the session and prefixes the wait notice.
        /// </summary>
        private static bool serverWarnedTooManyAttempts;

        /// <summary>
        /// Whether the license panel should ask for a key to be typed in rather than offer a device
        /// check. Set when no usable key is stored and when a check comes back rejected.
        /// </summary>
        private static bool licenseKeyEntryRequired;

        /// <summary>
        /// Whether a device check has already failed once this session, which relabels the check
        /// button "Retry" and lets it through the guard that otherwise suppresses repeat attempts.
        /// </summary>
        private static bool licenseCheckRetryOffered;

        /// <summary>
        /// Whether a verification has been attempted since the domain was loaded, so the on-display
        /// check does not fire on every repaint.
        /// </summary>
        private static bool licenseCheckedThisSession;

        /// <summary>
        /// <c>Time.realtimeSinceStartup</c> before which no further request may be sent, set from
        /// the server's own backoff instruction.
        /// </summary>
        private static float retryAllowedAtRealtime;

        /// <summary>
        /// The current UTC date as a "d/M/yyyy" string with the day and month obfuscated, sent with
        /// requests and compared against the server's date to detect a tampered system clock.
        /// </summary>
        private static string currentDateStamp;

        /// <summary>Whether an activation request is in flight.</summary>
        private static bool isActivatingLicense;

        /// <summary>Whether a verification request is in flight.</summary>
        private static bool isVerifyingLicense;

        /// <summary>
        /// Composed from slices of <see cref="hardwareId"/> and <see cref="currentDateStamp"/> each
        /// time the fingerprint is rebuilt, and then never read: nothing in the assembly consumes
        /// it. What it was for is not recoverable -- the shape (device identity interleaved with a
        /// date) would fit a cache key or an offline grace token, but that is a guess and is not
        /// asserted here.
        /// </summary>
        private static string unreadDeviceDateFingerprint;

        /// <summary>
        /// The machine fingerprint sent as "HWID": several hashes of gathered device properties,
        /// joined with dashes.
        /// </summary>
        private static string hardwareId;

        /// <summary>
        /// The token a successful verification returns. It is the input to the inline HMAC check
        /// that the obfuscator scattered through dozens of methods as a licensed/unlicensed test:
        /// each site recomputes <c>HMACSHA256(currentDateStamp + hardwareId)</c> and compares.
        /// </summary>
        private static string licenseToken;

        /// <summary>
        /// Whether the tool considers itself licensed. The coarse gate; the inline HMAC checks are
        /// the fine one.
        /// </summary>
        private static bool isLicensed;

        /// <summary>
        /// Set when a verification succeeded from the EditorPrefs cache rather than from the
        /// network, which triggers a settings write-back and a repaint of every tool window.
        /// </summary>
        private static bool licenseRestoredFromCache;

        /// <summary>
        /// Whether <see cref="pendingLicensedCallbacks"/> has already been drained. It is a
        /// one-shot: callbacks queued while unlicensed run once, on the first licensed frame.
        /// </summary>
        private static bool licensedCallbacksFlushed;

        /// <summary>
        /// Work deferred until the tool is licensed. While unlicensed, each caller is removed and
        /// re-added so a given delegate is queued at most once; while licensed, callers run
        /// immediately instead of queueing.
        /// </summary>
        private static Action pendingLicensedCallbacks;

        /// <summary>
        /// Work deferred until the licensing state is reset, queued the same way as
        /// <see cref="pendingLicensedCallbacks"/> and drained by the reset handler.
        /// </summary>
        private static Action pendingResetCallbacks;

        /// <summary>
        /// The types whose open instances are repainted when shared state changes -- this window and
        /// the settings window. Editors as well as windows, since some of this state is drawn from
        /// custom inspectors.
        /// </summary>
        private static readonly Type[] repaintTargetTypes =
        {
            typeof(ControllerEditor),
            typeof(ControllerEditorWindow)
        };

        /// <summary>
        /// Whether the license pane is showing the transfer form rather than the activation form.
        /// </summary>
        private static bool showingTransferPanel;

        /// <summary>
        /// Whether a transfer code has been sent, which is what reveals the code-entry field.
        /// </summary>
        private static bool transferCodeSent;

        /// <summary>Whether the "send me a transfer code" request is in flight.</summary>
        private static bool isRequestingTransferCode;

        /// <summary>Whether the transfer confirmation request is in flight.</summary>
        private static bool isConfirmingTransfer;

        #endregion

        #region Update and announcement banner

        /// <summary>Whether the update package is downloading, which disables the download button.</summary>
        private static bool isDownloadingUpdate;

        /// <summary>Whether an update check is in flight.</summary>
        private static bool isCheckingForUpdate;

        /// <summary>
        /// Whether an update check has already completed this session, which suppresses further
        /// automatic checks.
        /// </summary>
        private static bool hasCheckedForUpdate;

        /// <summary>
        /// Whether the cached remote version is newer than the tool's own. Gates the update banner
        /// and its menu entry.
        /// </summary>
        private static bool updateAvailable;

        /// <summary>Expansion state of the update banner.</summary>
        private static readonly AnimBool updateFoldout = new AnimBool();

        /// <summary>Expansion state of the announcement banner.</summary>
        private static readonly AnimBool announcementFoldout = new AnimBool();

        /// <summary>
        /// Extra (label, URL) entries appended to the tool's dropdown menu. The reading code handles
        /// a single entry as a plain item and several as a submenu; this build ships one.
        /// </summary>
        private static readonly (string, string)[] extraMenuLinks =
        {
            ("Templates", "https://notes.sleightly.dev/templates/")
        };

        #endregion

        #region Condition editing

        /// <summary>
        /// One editor per condition that every selected transition shares -- same parameter, mode
        /// and threshold. Editing one row writes through to all of them.
        /// </summary>
        private static List<ConditionMultiEditor> sharedConditionEditors = new List<ConditionMultiEditor>();

        /// <summary>
        /// One editor per condition of every selected transition, listed separately rather than
        /// merged. This is what "All Conditions" shows.
        /// </summary>
        private static List<ConditionMultiEditor> allConditionEditors = new List<ConditionMultiEditor>();

        /// <summary>
        /// One editor per condition of <see cref="focusedTransition"/>, used instead of the two
        /// lists above whenever a single transition is pinned.
        /// </summary>
        private static List<ConditionMultiEditor> focusedConditionEditors = new List<ConditionMultiEditor>();

        /// <summary>The reorderable list that draws <see cref="sharedConditionEditors"/>.</summary>
        private static ReorderableList sharedConditionList;

        /// <summary>The reorderable list that draws <see cref="allConditionEditors"/>.</summary>
        /// <remarks>
        /// Its add button is enabled only when exactly one transition is selected, because appending
        /// a condition to a merged list of several transitions has no single meaning.
        /// </remarks>
        private static ReorderableList allConditionList;

        /// <summary>The reorderable list that draws <see cref="focusedConditionEditors"/>.</summary>
        private static ReorderableList focusedConditionList;

        /// <summary>
        /// Selected tab of the sub-asset explorer, indexing the animator element types. -1 means no
        /// tab is chosen, which is the state a fresh inventory starts in.
        /// </summary>
        private static int subAssetTabIndex = -1;

        #endregion

        #region AnimatorState inspector properties

        // Serialised properties of selectedStatesSerialized, refreshed together whenever the state
        // selection changes. Names mirror AnimatorState's serialised layout.

        /// <summary><c>m_Name</c>.</summary>
        private static SerializedProperty stateNameProperty;

        /// <summary><c>m_Tag</c> -- which for this tool doubles as the node's style name.</summary>
        private static SerializedProperty stateTagProperty;

        /// <summary><c>m_Motion</c>.</summary>
        private static SerializedProperty stateMotionProperty;

        /// <summary><c>m_Speed</c>.</summary>
        private static SerializedProperty stateSpeedProperty;

        /// <summary><c>m_SpeedParameter</c>.</summary>
        private static SerializedProperty stateSpeedParameterProperty;

        /// <summary><c>m_TimeParameter</c>.</summary>
        private static SerializedProperty stateTimeParameterProperty;

        /// <summary><c>m_Mirror</c>.</summary>
        private static SerializedProperty stateMirrorProperty;

        /// <summary><c>m_CycleOffset</c>.</summary>
        private static SerializedProperty stateCycleOffsetProperty;

        /// <summary><c>m_IKOnFeet</c>, drawn as "Foot IK".</summary>
        private static SerializedProperty stateIkOnFeetProperty;

        /// <summary><c>m_WriteDefaultValues</c>.</summary>
        private static SerializedProperty stateWriteDefaultsProperty;

        /// <summary><c>m_SpeedParameterActive</c>.</summary>
        private static SerializedProperty stateSpeedParameterActiveProperty;

        /// <summary><c>m_TimeParameterActive</c>.</summary>
        private static SerializedProperty stateTimeParameterActiveProperty;

        /// <summary><c>m_MirrorParameterActive</c>.</summary>
        private static SerializedProperty stateMirrorParameterActiveProperty;

        /// <summary><c>m_CycleOffsetParameterActive</c>.</summary>
        private static SerializedProperty stateCycleOffsetParameterActiveProperty;

        /// <summary><c>m_MirrorParameter</c>.</summary>
        private static SerializedProperty stateMirrorParameterProperty;

        /// <summary><c>m_CycleOffsetParameter</c>.</summary>
        private static SerializedProperty stateCycleOffsetParameterProperty;

        #endregion

        #region Bulk transition graph modes

        // The three modes the transition context menu offers. They are mutually exclusive: entering
        // one clears the other two, Escape clears all three, and Return commits whichever is active.

        /// <summary>
        /// "Replicate Transitions": the next nodes clicked receive copies of
        /// <see cref="pendingTransitionEdits"/>.
        /// </summary>
        private static bool replicateTransitionsMode;

        /// <summary>
        /// "Redirect Transitions": the next node clicked becomes the destination of
        /// <see cref="pendingTransitionEdits"/>.
        /// </summary>
        private static bool redirectTransitionsMode;

        /// <summary>
        /// "Make Multiple Transitions": clicks accumulate nodes instead of replacing the selection,
        /// so one gesture can wire many transitions at once.
        /// </summary>
        private static bool makeMultipleTransitionsMode;

        #endregion

        #region AnimatorStateTransition inspector properties

        // Serialised properties of transitionInspectorSerialized, refreshed together.

        /// <summary><c>m_HasExitTime</c>.</summary>
        private static SerializedProperty transitionHasExitTimeProperty;

        /// <summary><c>m_ExitTime</c>.</summary>
        private static SerializedProperty transitionExitTimeProperty;

        /// <summary><c>m_HasFixedDuration</c>.</summary>
        private static SerializedProperty transitionHasFixedDurationProperty;

        /// <summary><c>m_TransitionDuration</c>.</summary>
        private static SerializedProperty transitionDurationProperty;

        /// <summary><c>m_TransitionOffset</c>.</summary>
        private static SerializedProperty transitionOffsetProperty;

        /// <summary><c>m_InterruptionSource</c>.</summary>
        private static SerializedProperty transitionInterruptionSourceProperty;

        /// <summary><c>m_OrderedInterruption</c>.</summary>
        private static SerializedProperty transitionOrderedInterruptionProperty;

        /// <summary><c>m_CanTransitionToSelf</c>.</summary>
        private static SerializedProperty transitionCanTransitionToSelfProperty;

        /// <summary><c>m_Solo</c>.</summary>
        private static SerializedProperty transitionSoloProperty;

        /// <summary><c>m_Mute</c>.</summary>
        private static SerializedProperty transitionMuteProperty;

        /// <summary>
        /// Whether the transition-editing section is shown. Sticky in the same way as
        /// <see cref="stateSectionVisible"/>.
        /// </summary>
        private static bool transitionSectionVisible;

        #endregion

        #region Animatable-property caches

        /// <summary>
        /// Every concrete non-generic <see cref="Component"/> subclass in the domain, plus
        /// <see cref="GameObject"/>, sorted by name. Feeds the component picker in the
        /// property-adding UI.
        /// </summary>
        private static Type[] componentTypes;

        /// <summary>
        /// Animatable property names per component type, discovered by adding the component to a
        /// scratch object and asking Unity for its bindings. Cached because that is expensive and
        /// the answer never changes within a session.
        /// </summary>
        private static readonly Dictionary<Type, string[]> animatablePropertiesByType =
            new Dictionary<Type, string[]>();

        /// <summary>
        /// Animatable <c>material.*</c> property names per shader, discovered the same way through a
        /// scratch renderer.
        /// </summary>
        private static readonly Dictionary<Shader, string[]> materialPropertiesByShader =
            new Dictionary<Shader, string[]>();

        #endregion

        #region Layer and parameter batch actions

        /// <summary>
        /// Destination controller for a Copy action, shown only when the destination is another
        /// controller rather than this one.
        /// </summary>
        private static UnityEditor.Animations.AnimatorController actionTargetController;

        /// <summary>
        /// The parameter name an action reads: the parameter to remove, or the one to replace.
        /// Typed freely or picked from <see cref="parameterNames"/>.
        /// </summary>
        private static string actionSourceName;

        /// <summary>The parameter name a Replace action substitutes in.</summary>
        private static string actionReplacementName;

        /// <summary>
        /// The layer name or tag an action is scoped by, shown whenever the chosen scope is
        /// "layers tagged with" or a named layer.
        /// </summary>
        private static string actionFilterText;

        /// <summary>
        /// Whether Remove/Replace match a parameter name exactly rather than as a substring.
        /// </summary>
        private static bool matchWholeWord = true;

        /// <summary>
        /// Whether a Copy also creates, on the destination controller, the parameters the copied
        /// layers use.
        /// </summary>
        private static bool addRequiredParameters = true;

        /// <summary>
        /// Suffix appended to the parameter names of copied layers, so a layer can be duplicated
        /// without its parameters colliding with the originals. Applies to the parameters added by
        /// <see cref="addRequiredParameters"/> as well.
        /// </summary>
        private static string copiedParameterSuffix;

        /// <summary>Which batch action the panel is configured for.</summary>
        private static ControllerAction selectedAction = ControllerAction.Copy;

        /// <summary>Which layers the selected action applies to.</summary>
        private static ActionMode actionScope = ActionMode.CurrentController;

        /// <summary>Which layers a Copy action takes as its source.</summary>
        private static MoveMode copySourceScope = MoveMode.CurrentLayer;

        /// <summary>
        /// Where a Copy action puts its result.
        /// </summary>
        /// <remarks>
        /// DELIBERATE DEVIATION: the shipped field is an INSTANCE field (decompiled line 8266) --
        /// the only one in a class of 267 statics -- and is declared <c>static</c> here. See the
        /// DELIBERATE DEVIATION block in this file's header for why and for what it changes.
        /// </remarks>
        private static MoveDestination copyDestination;

        #endregion

        #region Animation window integration

        /// <summary>
        /// The open Animation window, found by type since it is internal. Cached after the first
        /// successful lookup.
        /// </summary>
        private static EditorWindow animationWindow;

        /// <summary>
        /// A hidden scratch GameObject the tool binds the Animation window to when there is no real
        /// avatar to preview against, so curves can still be authored.
        /// </summary>
        private static GameObject previewRoot;

        /// <summary>The <see cref="Animator"/> added to <see cref="previewRoot"/>.</summary>
        private static Animator previewAnimator;

        /// <summary>
        /// Declared with the preview objects and never read or written anywhere in the assembly.
        /// Its type (<see cref="object"/>) carries no hint and no call site exists, so it is not
        /// named more specifically here.
        /// </summary>
        private static object unusedPreviewObject;

        /// <summary>
        /// Forces the Animation window's "should I follow the scene selection?" check to answer yes
        /// for one call, so the tool can retarget the window itself.
        /// </summary>
        private static bool forceGameObjectSelectionUpdate;

        /// <summary>
        /// The controller the Animation window is made to edit instead of whatever the scene
        /// selection implies. Non-null means the tool has taken the window over, which is also what
        /// disables its play and record buttons.
        /// </summary>
        private static UnityEditor.Animations.AnimatorController overrideAnimationController;

        /// <summary>
        /// Whether the root-GameObject patch is currently allowed to substitute
        /// <see cref="overrideAnimationRoot"/>. Raised around the calls that need the substitution
        /// and cleared immediately after, so the patch does not affect unrelated queries.
        /// </summary>
        private static bool overrideAnimationRootActive;

        /// <summary>
        /// The GameObject curve authoring is resolved against while the tool owns the Animation
        /// window -- an avatar chosen by the user, so property paths resolve against a real
        /// hierarchy.
        /// </summary>
        private static GameObject overrideAnimationRoot;

        /// <summary>
        /// Whether the extra property-editing entries may be added to the Animation window's
        /// hierarchy context menu. Set when the menu is built for nodes the tool can handle and
        /// cleared once the entries are added.
        /// </summary>
        private static bool propertyEditingMenuAllowed;

        /// <summary>
        /// The hierarchy nodes the context menu was raised on, kept so the added menu entries can
        /// act on the same rows once the menu is clicked.
        /// </summary>
        private static List<object> interactedHierarchyNodes;

        /// <summary>
        /// Declared among the graph reflection fields and never read or written anywhere in the
        /// assembly. A <see cref="Rect"/> with no call site gives nothing to name it from.
        /// </summary>
        private static Rect unusedGraphRect;

        // The Animation window's own Type / PropertyInfo / FieldInfo handles that sat between these
        // fields in the original bank (decompiled 8284-8306, 8496) are declared in
        // ControllerEditor.ReflectionPriming.cs alongside the method that fills them. See the
        // "DECLARED ELSEWHERE" list in this file's header for the name mapping.

        #endregion

        #region Graph rendering state

        // The Type / MethodInfo handles for the graph types this region patches (decompiled
        // 8314-8318, 8330-8340, 8346) are declared in ControllerEditor.ReflectionPriming.cs.

        /// <summary>
        /// <c>Styles.graphBackground</c>, overwritten outright so the grid can be given a custom
        /// colour. There is no hook for the background, only the static style itself.
        /// </summary>
        private static FieldInfo graphBackgroundStyleField;

        /// <summary>
        /// The 1x1 texture assigned into <see cref="graphBackgroundStyleField"/>. Allocated once and
        /// recoloured in place rather than reallocated, since it is written on every settings change.
        /// </summary>
        private static Texture2D graphBackgroundTexture;

        /// <summary>
        /// Arrowhead positions of the edges attached to the current node selection, gathered once
        /// per graph pass so the arrow-animation patch can recognise them by position.
        /// </summary>
        /// <remarks>
        /// Position is the only identity available: the patch that draws an arrow is given points,
        /// not the edge they came from.
        /// </remarks>
        private static readonly HashSet<Vector3> animatedEdgeArrowPoints = new HashSet<Vector3>();

        /// <summary>Whether the arrow-position setting is non-zero, i.e. arrows are offset at all.</summary>
        private static bool arrowLerpEnabled;

        /// <summary>
        /// Whether the selected edges' arrows should be animated this pass: the setting is on, and
        /// the node selection is small enough to be worth walking.
        /// </summary>
        private static bool animatingSelectedEdges;

        /// <summary>
        /// Request for a graph repaint at the end of the frame, raised from inside the arrow drawing
        /// so the animation keeps ticking. Repainting from the draw call itself would recurse.
        /// </summary>
        private static bool repaintGraphRequested;

        /// <summary>
        /// Request for a full graph rebuild at the end of the frame, raised by the operations that
        /// invalidate node layout.
        /// </summary>
        private static bool rebuildGraphRequested;

        /// <summary>
        /// Whether execution is currently inside the graph's own <c>OnGraphGUI</c>. Several patches
        /// behave differently depending on whether they were reached from the graph or from the
        /// window around it.
        /// </summary>
        private static bool insideGraphGui;

        /// <summary>
        /// The live <c>LayerControllerView</c> instance, captured when the layer list is created.
        /// </summary>
        private static object layerControllerView;

        #endregion

        #region Layer view

        // The LayerControllerView / LayerSettingsWindow reflection handles that opened this part of
        // the bank (decompiled 8352-8370) are declared in ControllerEditor.ReflectionPriming.cs.

        /// <summary>
        /// Whether the layer-template dropdown is ready to be opened. Cleared while the pointer is
        /// over the button so its contents are refreshed exactly once per hover, rather than on
        /// every repaint.
        /// </summary>
        private static bool templateDropdownArmed = true;

        /// <summary>Scroll position of the layer list in category view.</summary>
        private static Vector2 categoryLayerScroll;

        /// <summary>
        /// Controllers in the project labelled as layer templates, offered by the "add layer"
        /// dropdown.
        /// </summary>
        private static UnityEditor.Animations.AnimatorController[] layerTemplateControllers;

        /// <summary>
        /// Display names for <see cref="layerTemplateControllers"/>, positionally -- taken from the
        /// asset label after "Template:", with escaped spaces restored. The two are always rebuilt
        /// together.
        /// </summary>
        private static string[] layerTemplateNames;

        /// <summary>How the layer list is grouped: flat, by name prefix, or by layer tag.</summary>
        private static LayerViewViewType layerViewType = LayerViewViewType.DefaultView;

        /// <summary>
        /// Root of the category tree built from the controller's layers. Rebuilt whenever the
        /// controller or the view type changes.
        /// </summary>
        private static LayerPathNode layerCategoryRoot;

        /// <summary>
        /// The category currently open in the layer list. Equal to <see cref="layerCategoryRoot"/>
        /// at the top level; changing it is what makes a new layer inherit the category's name
        /// prefix or tag.
        /// </summary>
        private static LayerPathNode currentLayerCategory;

        /// <summary>
        /// The reorderable list that draws <see cref="currentLayerCategory"/>'s layers in category
        /// view, replacing Unity's own list.
        /// </summary>
        private static ReorderableList categoryLayerList;

        /// <summary>
        /// Unity's own <c>m_LayerList</c>, captured from the layer view so the tool can read the
        /// selected index and, in the default view, draw through it.
        /// </summary>
        private static ReorderableList unityLayerList;

        /// <summary>
        /// Distinct category names across the controller's layers, offered as completions when a new
        /// category is typed.
        /// </summary>
        private static string[] categoryNames;

        /// <summary>
        /// Whether the category view drew the layer list this pass, which suppresses Unity's own
        /// list for the rest of the frame.
        /// </summary>
        private static bool categoryViewDrewLayerList;

        /// <summary>
        /// Request to frame the selected layer's graph at the end of the frame, honoured only when
        /// the auto-frame setting is on.
        /// </summary>
        private static bool frameLayerRequested;

        /// <summary>
        /// Unity's own <c>OnDrawLayer</c>, bound as a delegate so category view can reuse the stock
        /// row drawing rather than reimplement it.
        /// </summary>
        private static ReorderableList.ElementCallbackDelegate drawLayerCallback;

        /// <summary>Unity's own <c>OnSelectLayer</c>, bound for the same reason.</summary>
        private static ReorderableList.SelectCallbackDelegate selectLayerCallback;

        /// <summary>Unity's own <c>OnMouseUpLayer</c>, bound for the same reason.</summary>
        private static ReorderableList.SelectCallbackDelegate mouseUpLayerCallback;

        /// <summary>
        /// Wrapper over the layer view's own rename overlay, so a category-view row renames a layer
        /// through exactly the path Unity uses.
        /// </summary>
        private static RenameOverlayWrapper layerRenameOverlay;

        /// <summary>
        /// A rename overlay owned by the tool, used to rename a state in place on the graph -- a
        /// thing Unity's Animator window does not otherwise offer. Its callback also restores
        /// escaped newlines, so a multi-line state name can be typed.
        /// </summary>
        private static RenameOverlayWrapper stateRenameOverlay;

        #endregion

        #region Graph node menus and transition dragging

        // The node-type, node-member and menu reflection handles interleaved through this part of
        // the bank (decompiled 8406-8408, 8412-8436, 8492-8498) are declared in
        // ControllerEditor.ReflectionPriming.cs.

        /// <summary>
        /// The context menu currently being built or shown -- either one the tool created or one it
        /// intercepted from the graph in order to insert entries.
        /// </summary>
        private static GenericMenu contextMenu;

        /// <summary>
        /// The node a shift-double-click slot drag started from, held until the drag ends on another
        /// node so an edge can be created between the two.
        /// </summary>
        private static Node slotDragSourceNode;

        /// <summary>Whether such a drag is in progress.</summary>
        private static bool slotDragActive;

        /// <summary>
        /// Whether a "make transition" gesture has been started on a node and is waiting for the
        /// click that picks its destination.
        /// </summary>
        private static bool transitionDragArmed;

        /// <summary>
        /// Whether the next single click should end the armed transition gesture rather than start a
        /// new one. Together with <see cref="transitionDragArmed"/> this is what lets a transition
        /// be drawn with two clicks instead of a held drag.
        /// </summary>
        private static bool transitionDragPending;

        /// <summary>
        /// Set by the slot-drag-end patch so the drag-end patch that runs after it can tell a slot
        /// drag from a plain one and leave the transition gesture armed.
        /// </summary>
        private static bool slotDraggingEnded;

        /// <summary>
        /// A throwaway transition passed to the graph's edge-creation call, which requires a
        /// transition to describe the edge it is about to draw. Its values are deliberate nonsense;
        /// the transition is never added to a state machine.
        /// </summary>
        private static AnimatorStateTransition placeholderTransition;

        /// <summary>
        /// A throwaway destination state for <see cref="placeholderTransition"/>, for the same
        /// reason. Both are allocated once and reused.
        /// </summary>
        private static AnimatorState placeholderTransitionTarget;

        /// <summary>
        /// The size of the node currently being drawn, captured at the top of the node-GUI patch so
        /// the overlays drawn afterwards can position themselves against it.
        /// </summary>
        private static Vector2 currentNodeSize;

        /// <summary>
        /// Whether a drag-and-drop onto the graph is pending, so the next graph pass can set the
        /// drag visual mode. The mode has to be set from inside the graph's own GUI to take effect.
        /// </summary>
        private static bool dragAndDropPending;

        /// <summary>The state the quick-toggle window was opened for.</summary>
        private static AnimatorState quickToggleState;

        /// <summary>
        /// The controller's parameters as of the start of the parameter list's draw, so each row can
        /// be drawn against a stable snapshot while the list is being edited.
        /// </summary>
        private static UnityEngine.AnimatorControllerParameter[] parameterViewParameters;

        /// <summary><c>ParameterControllerView.m_ScrollPosition</c>.</summary>
        private static FieldInfo parameterViewScrollField;

        /// <summary><c>UnityEditor.Graphs.ParameterControllerView</c>.</summary>
        private static Type parameterControllerViewType;

        /// <summary>
        /// Declared among the graph interaction state and never read or written anywhere in the
        /// assembly. An <see cref="int"/> with no call site gives nothing to name it from.
        /// </summary>
        private static int unusedNodeIndex;

        /// <summary>
        /// Mouse position captured when a category or layer menu is opened, so the popup that opens
        /// on a later event can still appear where the click happened.
        /// </summary>
        private static Vector2 categoryMenuMousePosition;

        /// <summary>
        /// The node an armed transition gesture started from, re-invoked when a slot drag interrupts
        /// and then ends.
        /// </summary>
        private static Node pendingTransitionSourceNode;

        /// <summary>
        /// Which kind of node <see cref="pendingTransitionSourceNode"/> is, as the graph's own node
        /// code: 1 sub-machine, 2 entry, 3 state, 4 any state. Selects which
        /// <c>MakeTransitionCallback</c> to re-invoke.
        /// </summary>
        private static int pendingTransitionSourceKind;

        /// <summary>
        /// Whether a context click landed on a node this event, so the menu-building patch that runs
        /// afterwards knows to add the node entries.
        /// </summary>
        private static bool nodeContextClickPending;

        /// <summary>
        /// The sticky default for <see cref="replaceTransitions"/>, toggled by the "(Replacing)"
        /// menu entry so the preference persists between menu invocations.
        /// </summary>
        private static bool replaceTransitionsDefault;

        /// <summary>
        /// Whether the bulk transition operations replace the transitions they act on rather than
        /// adding alongside them. Recomputed per menu as the default XOR the shift key.
        /// </summary>
        private static bool replaceTransitions;

        /// <summary>
        /// Whether reversing a transition also inverts its conditions' values rather than only their
        /// direction. Recomputed per menu as the setting XOR the control key.
        /// </summary>
        private static bool reverseModifiesValues;

        /// <summary>
        /// The layer index a layer context menu was raised on, used by the paste and duplicate
        /// entries once the menu is clicked.
        /// </summary>
        private static int contextLayerIndex;

        /// <summary>
        /// Whether the transition gesture interrupted by a slot drag should be resumed when the drag
        /// ends. Cleared by the drag-end patch.
        /// </summary>
        private static bool resumeTransitionDragAfterSlotDrag;

        /// <summary>
        /// The blend-tree state a "open blend tree" menu entry was built for, opened by pushing a
        /// breadcrumb when the entry is clicked.
        /// </summary>
        private static AnimatorState blendTreeBreadcrumbState;

        /// <summary>
        /// The controller a layer context menu was raised against, read from the tool rather than
        /// from <see cref="currentController"/> so the menu acts on what the view is actually
        /// showing.
        /// </summary>
        private static UnityEditor.Animations.AnimatorController layerContextController;

        /// <summary>
        /// The layer on the copy clipboard. Non-null is what enables the layer menu's "Paste".
        /// </summary>
        private static UnityEditor.Animations.AnimatorControllerLayer copiedLayer;

        #endregion

        #region Accessors over this state

        /// <summary>
        /// Repaints the Controller Editor window if one is open. A no-op otherwise -- most of this
        /// state is written from Animator window patches, which have no window of their own to
        /// repaint.
        /// </summary>
        private static void RepaintWindow()
        {
            if (activeWindow != null)
            {
                activeWindow.Repaint();
            }
        }

        /// <summary>
        /// Whether the condition editor is pinned to one specific transition rather than showing the
        /// whole selection.
        /// </summary>
        private static bool HasFocusedTransition
        {
            get
            {
                return focusedTransition.transition != null;
            }
        }

        #endregion
    }
}
