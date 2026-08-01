// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the six eager "resolve the internal Unity members we are about to patch" methods of
// the ControllerEditor god class, and the reflection handle fields they fill. Line numbers are
// relative to the current snapshot; the decompiled names are the durable reference.
//
//   FillAlgo          -> PrimeAnimationWindowReflection,      line 15280
//   PrepareAlgo       -> PrimeGraphStyleReflection,           line 15692
//   RegisterAlgo      -> PrimeAnimatorToolReflection,         line 15890
//   ManageAlgo        -> PrimeLayerControllerViewReflection,  line 16164
//   InstantiateMapper -> PrimeMenuAndLayerEditorReflection,   line 17075
//   ResetMapper       -> PrimeGraphNodeReflection,            line 17116
//
// Field map, grouped by the method that fills it. Every one of these was a bare `private static
// Type/FieldInfo/PropertyInfo/MethodInfo/ConstructorInfo` in the god class's field bank (decompiled
// lines 8066-8076 and 8284-8498); they are moved here rather than into ControllerEditor.State.cs
// because they are not editor state, they are the binding table for the Harmony patch set and are
// meaningless apart from the method that fills them.
//
//   -- PrimeAnimationWindowReflection (decompiled 15282-15292) --
//   mapperVisitor        -> animationWindowType,                line 8284
//   _InitializerVisitor  -> animationWindowStateType,           line 8286
//   definitionVisitor    -> animationWindowHierarchyGUIType,    line 8288
//   _RegVisitor          -> animEditorType,                     line 8290
//   _TestsVisitor        -> animationWindowControlType,         line 8292
//   _PropertyVisitor     -> animationWindowSelectionItemType,   line 8294
//   m_ProcessorVisitor   -> animationWindowStateProperty,       line 8296
//   _ObserverVisitor     -> activeAnimationClipProperty,        line 8298
//   serverVisitor        -> activeRootGameObjectProperty,       line 8300
//   m_ThreadVisitor      -> activeGameObjectProperty,           line 8302
//   m_PolicyVisitor      -> activeScriptableObjectProperty,     line 8304
//   m_SerializerVisitor  -> hierarchyNodeBindingField,          line 8306
//   m_ItemVisitor        -> playControlsOnGUIMethod,            line 8496
//
//   -- PrimeGraphStyleReflection (decompiled 15694-15696) --
//   rulesVisitor         -> graphGUIType,                       line 8314
//   m_QueueVisitor       -> graphEdgeType,                      line 8316
//   errorVisitor         -> graphStylesType,                    line 8318
//
//   -- PrimeAnimatorToolReflection (decompiled 15892-15901) --
//   _ConsumerVisitor     -> animatorControllerToolType,         line 8330
//   m_AdapterVisitor     -> graphType,                          line 8332
//   m_InterpreterVisitor -> blendTreeGraphGUIType,              line 8334
//   _WatcherVisitor      -> stateMachineGraphGUIType,           line 8336
//   m_CandidateVisitor   -> stateMachineGraphType,              line 8338
//   m_ProductVisitor     -> edgeGUIType,                        line 8340
//   workerVisitor        -> rebuildGraphMethod,                 line 8348
//   m_PrototypeVisitor   -> addBreadCrumbMethod,                line 8406
//   _AdapterAnnotation   -> activeGraphGUIGetter,               line 8074
//   interpreterAnnotation-> getEdgePointsMethod,                line 8076
//
//   -- PrimeLayerControllerViewReflection (decompiled 16166-16175) --
//   _ReaderVisitor       -> layerControllerViewType,            line 8352
//   _BridgeVisitor       -> layerSettingsWindowType,            line 8354
//   m_StrategyVisitor    -> layerScrollField,                   line 8356
//   _CustomerVisitor     -> onRemoveLayerMethod,                line 8358
//   m_DatabaseVisitor    -> layerRenameEndMethod,               line 8360
//   _ExporterVisitor     -> showAtPositionMethod,               line 8362
//   m_IdentifierVisitor  -> layerListField,                     line 8364
//   m_AttrVisitor        -> layerViewHostField,                 line 8366
//   m_DispatcherVisitor  -> toolAnimatorControllerField,        line 8368
//   registryVisitor      -> keyboardHandlingMethod,             line 8370
//
//   -- PrimeMenuAndLayerEditorReflection (decompiled 17077-17094) --
//   m_RuleVisitor        -> menuItemConstructor,                line 8408
//   _ManagerVisitor      -> advancedPopupMethod,                line 8494
//   m_SpecificationVisitor -> getBuiltinSkinMethod,             line 8498
//   contextAnnotation    -> layerEditorField,                   line 8066
//   recordAnnotation     -> previewAnimatorField,               line 8068
//   helperAnnotation     -> liveLinkProperty,                   line 8070
//   m_ConsumerAnnotation -> selectedLayerIndexProperty,         line 8072
//
//   -- PrimeGraphNodeReflection (decompiled 17118-17131) --
//   _FactoryVisitor      -> stateMachineNodeBaseType,           line 8412
//   _AccountVisitor      -> graphNodeType,                      line 8414
//   _RefVisitor          -> blendTreeNodeType,                  line 8416
//   _StatusVisitor       -> edgeGUIPatchType,                   line 8418
//   roleVisitor          -> entryNodeMakeTransitionCallback,    line 8428
//   paramVisitor         -> anyStateNodeMakeTransitionCallback, line 8430
//   modelVisitor         -> stateNodeMakeTransitionCallback,    line 8432
//   tokenizerVisitor     -> stateMachineNodeMakeTransitionCallback, line 8434
//   _ParserVisitor       -> findClosestEdgeMethod,              line 8492
//   m_DecoratorVisitor   -> genericMenuForStateMachineNodeMethod, line 8436
//   tokenVisitor         -> stateNodeStateField,                line 8420
//   codeVisitor          -> blendTreeNodeMotionField,           line 8422
//   m_DicVisitor         -> blendTreeNodeChildrenField,         line 8424
//   _InvocationVisitor   -> blendTreeNodeParentProperty,        line 8426
//
// ================================ Not ported from this region ================================
//
//   ControllerEditor.cs line 15282 et al call EditorUtils.FillRules, ported as
//   EditorUtils.RequireQualifiedType in EditorUtils/EditorUtils.Callbacks.cs. Reused, not
//   reimplemented. (EditorUtils.Types.cs's header still says FillRules is unported; that claim was
//   already corrected in EditorUtils.Callbacks.cs's header and is corrected again here.)
//
//   The four Type extension helpers these methods lean on -- DisableList / InsertList / RestartList /
//   QueryList, decompiled EditorUtils.cs lines 6773-6790 -- are NOT ported anywhere in the package,
//   and EditorUtils is owned by another port so they are not added here. They are one-liners over
//   GetMethod / GetField / GetProperty with the same flag set, so the calls are written out directly
//   against the PrimingBindingFlags constant below:
//     DisableList(name)           -> GetMethod(name, PrimingBindingFlags)
//     InsertList(name, types)     -> GetMethod(name, PrimingBindingFlags, null, types, null)
//     RestartList(name)           -> GetField(name, PrimingBindingFlags)
//     QueryList(name)             -> GetProperty(name, PrimingBindingFlags)
//   That is a spelling change only; the binding flags and the resolution result are identical.
//
//   DEFERRED, decompiled InstantiateMapper lines 17095-17106: the tail of the method builds the two
//   RenameOverlayWrapper instances --
//     indexerVisitor  (line 8402) wraps the layer view's own overlay, via
//       `new RenameOverlayWrapper(() => layerControllerViewType.GetMethod("get_renameOverlay")
//                                          .Invoke(ReadAnnotation(), null))`
//     _IssuerVisitor  (line 8404) is a second, freshly constructed overlay whose onEndRename runs
//       RestartAlgo(RevertMapper(), m_AlgoAnnotation, _IssuerVisitor.Name()) when the rename was
//       accepted.
//   Both are shared UI state read all over the layer view (decompiled lines 6265, 8690, 16401,
//   16403, 16527) rather than reflection handles, so the fields belong in ControllerEditor.State.cs,
//   which another port owns; and the closures need ReadAnnotation (line 6258), RevertMapper (8552),
//   RestartAlgo (14376) and m_AlgoAnnotation, none of which are ported. Omitted rather than stubbed.
//   Note for whoever lands it: that lookup is a plain `GetMethod("get_renameOverlay")`, i.e. public
//   binding only, unlike every other lookup in the region -- port it literally, including the null
//   it will return if Unity ever makes the property non-public.
//
//   NOT PORTED, decompiled line 8906 RevertWrapper / line 15867: the only caller of this priming
//   chain is RevertWrapper, which runs the whole chain inside an inline HMAC-SHA256 licence check
//   (`m_ParamsAnnotation == Convert.ToBase64String(...)`). That is obfuscator/licence scaffolding for
//   a validation backend that no longer exists, so it is not ported here; see the project's standing
//   rule on licence gates. Line 15867 (`m_SetterVisitor`, UnityEditor.Graphs.Styles.graphBackground)
//   is a lazy one-field cache inside a GUI-drawing method, not part of this priming set -- see
//   ControllerEditorWindow.Cosmetics.cs, which already documents that write.
//
// ================================ Shipped behaviour preserved ================================
//
//   * edgeGUIPatchType (_StatusVisitor) and edgeGUIType (m_ProductVisitor) resolve the *same* type,
//     UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, from two different methods into two
//     different fields. Redundant, but both fields are read by different call sites, so both are
//     kept.
//   * graphStylesType (errorVisitor) and graphNodeType (_AccountVisitor) are assigned here and never
//     read anywhere in the assembly. Dead in the shipped build; kept so the resolution -- and its
//     ability to throw -- is preserved.
//   * PrimeLayerControllerViewReflection and PrimeMenuAndLayerEditorReflection both read
//     animatorControllerToolType, which only PrimeAnimatorToolReflection fills. The call order at
//     decompiled line 8929-8941 is RegisterAlgo, ResetMapper, ManageAlgo, then FillAlgo,
//     InstantiateMapper, PrepareAlgo, so the dependency happens to be satisfied. Calling any of them
//     out of order throws a NullReferenceException. Preserved as-is.
//
// ================================ DELIBERATE DEVIATION ================================
//
//   NONE in behaviour. This is a note that the temptation to make one was resisted.
//
//   The package's other internal-API bindings (AnimatorGraphReflection, RenameOverlayWrapper) were
//   rebuilt on TypeResolver / ReflectionMemberRef, which resolve lazily and yield null on a miss.
//   These six methods are deliberately NOT converted to that shape, because the difference is
//   behavioural and load-bearing: see the eager/lazy discussion on
//   <see cref="PrimeAnimationWindowReflection"/>. Where the decompiled source itself already reads
//   through AnimatorGraphReflection.TypeResolvers -- the four MakeTransitionCallback lookups in
//   PrimeGraphNodeReflection -- that is kept, because it is what shipped, not because it is nicer.

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The tool's binding table for the Unity editor internals its Harmony patches target, and the
    /// eager passes that fill it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Everything ControllerEditor does to the Animation window, the Animator graph and the layer
    /// list is done to types that are <c>internal</c> to <c>UnityEditor</c>,
    /// <c>UnityEditorInternal</c> and <c>UnityEditor.Graphs</c>. None of them can be named at compile
    /// time, so each is resolved from an assembly-qualified name string and each member off it is
    /// resolved by name. This file is the whole of that surface: if a Unity release moves any member
    /// listed here, this is the file that says so.
    /// </para>
    /// <para>
    /// The bindings assume the internal layout shipped by Unity 2019.4 through 2022.3, the range the
    /// two shipped builds target. None of it is public API and none of it is versioned; the code has
    /// no per-version branches, so it is one shape or nothing.
    /// </para>
    /// </remarks>
    internal static partial class ControllerEditor
    {
        /// <summary>
        /// The binding flags every lookup in this file uses: any member, public or not, instance or
        /// static.
        /// </summary>
        /// <remarks>
        /// Deliberately unselective. The members being bound are internals whose accessibility and
        /// staticness are not contracted and have in fact changed between Unity versions; matching on
        /// the name alone is what keeps a lookup working across such a change.
        /// </remarks>
        private const BindingFlags PrimingBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        // ── Animation window ────────────────────────────────────────────────────────────────────

        /// <summary><c>UnityEditor.AnimationWindow</c>.</summary>
        private static Type animationWindowType;

        /// <summary><c>UnityEditorInternal.AnimationWindowState</c>, the window's model object.</summary>
        private static Type animationWindowStateType;

        /// <summary><c>UnityEditorInternal.AnimationWindowHierarchyGUI</c>, the curve list on the left.</summary>
        private static Type animationWindowHierarchyGUIType;

        /// <summary>
        /// <c>UnityEditor.AnimEditor</c> — the animation editor widget the window hosts, and where
        /// the playback controls are drawn.
        /// </summary>
        private static Type animEditorType;

        /// <summary><c>UnityEditorInternal.AnimationWindowControl</c>, the playback controller.</summary>
        private static Type animationWindowControlType;

        /// <summary>
        /// <c>UnityEditorInternal.AnimationWindowSelectionItem</c> — what the window is currently
        /// editing, which is the object the tool substitutes to retarget the window.
        /// </summary>
        private static Type animationWindowSelectionItemType;

        /// <summary><c>AnimationWindow.state</c>.</summary>
        private static PropertyInfo animationWindowStateProperty;

        /// <summary><c>AnimationWindowState.activeAnimationClip</c>.</summary>
        private static PropertyInfo activeAnimationClipProperty;

        /// <summary><c>AnimationWindowState.activeRootGameObject</c>.</summary>
        private static PropertyInfo activeRootGameObjectProperty;

        /// <summary><c>AnimationWindowState.activeGameObject</c>.</summary>
        private static PropertyInfo activeGameObjectProperty;

        /// <summary><c>AnimationWindowState.activeScriptableObject</c>.</summary>
        private static PropertyInfo activeScriptableObjectProperty;

        /// <summary>
        /// <c>AnimationWindowHierarchyNode.binding</c> — the <see cref="EditorCurveBinding"/> one row
        /// of the curve list stands for.
        /// </summary>
        /// <remarks>
        /// The only lookup in this file that narrows the binding flags, to public instance members
        /// only; that is what the decompiled source does and it is kept, so this one goes null rather
        /// than resolving if Unity ever makes the field internal.
        /// </remarks>
        private static FieldInfo hierarchyNodeBindingField;

        /// <summary><c>AnimEditor.PlayControlsOnGUI</c>.</summary>
        private static MethodInfo playControlsOnGUIMethod;

        // ── Graph drawing ───────────────────────────────────────────────────────────────────────

        /// <summary><c>UnityEditor.Graphs.GraphGUI</c>, the graph base class, patched for its grid colours.</summary>
        private static Type graphGUIType;

        /// <summary>
        /// <c>UnityEditor.Graphs.Edge</c>. Its two-slot constructor is patched so every arrow the
        /// graph creates can be recoloured as it appears.
        /// </summary>
        private static Type graphEdgeType;

        /// <summary><c>UnityEditor.Graphs.Styles</c>.</summary>
        /// <remarks>
        /// Resolved and never read — nothing in the shipped assembly uses this field. The one thing
        /// the tool does want off <c>Styles</c>, the <c>graphBackground</c> style, is bound
        /// separately and lazily by the cosmetics code. Kept because the resolution itself is
        /// observable: it throws if the type is gone.
        /// </remarks>
        private static Type graphStylesType;

        // ── Animator window ─────────────────────────────────────────────────────────────────────

        /// <summary><c>UnityEditor.Graphs.AnimatorControllerTool</c>, the Animator window.</summary>
        private static Type animatorControllerToolType;

        /// <summary><c>UnityEditor.Graphs.Graph</c>.</summary>
        private static Type graphType;

        /// <summary><c>UnityEditor.Graphs.AnimationBlendTree.GraphGUI</c>.</summary>
        private static Type blendTreeGraphGUIType;

        /// <summary><c>UnityEditor.Graphs.AnimationStateMachine.GraphGUI</c>.</summary>
        private static Type stateMachineGraphGUIType;

        /// <summary><c>UnityEditor.Graphs.AnimationStateMachine.Graph</c>.</summary>
        private static Type stateMachineGraphType;

        /// <summary><c>UnityEditor.Graphs.AnimationStateMachine.EdgeGUI</c>.</summary>
        /// <remarks>
        /// The same type is resolved a second time into <see cref="edgeGUIPatchType"/> by
        /// <see cref="PrimeGraphNodeReflection"/>. Redundant in the shipped build; both fields are
        /// read, so both are kept.
        /// </remarks>
        private static Type edgeGUIType;

        /// <summary><c>AnimatorControllerTool.RebuildGraph</c>, called to force a redraw after edits.</summary>
        private static MethodInfo rebuildGraphMethod;

        /// <summary><c>AnimatorControllerTool.AddBreadCrumb</c>, used to navigate the window into a sub-machine.</summary>
        private static MethodInfo addBreadCrumbMethod;

        /// <summary>
        /// <c>AnimatorControllerTool.activeGraphGUI</c>'s getter, bound as a method rather than a
        /// property because that is what the shipped code does.
        /// </summary>
        private static MethodInfo activeGraphGUIGetter;

        /// <summary>
        /// <c>AnimationStateMachine.EdgeGUI.GetEdgePoints(Edge)</c> — the polyline an arrow is drawn
        /// along, which the tool needs in order to draw its own decorations on top.
        /// </summary>
        /// <remarks>
        /// Bound by exact signature: there is more than one <c>GetEdgePoints</c> overload, and the
        /// one taking a single <see cref="Edge"/> is the one that returns the finished points.
        /// </remarks>
        private static MethodInfo getEdgePointsMethod;

        // ── Layer list ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>UnityEditor.Graphs.LayerControllerView</c>, the Animator window's layer sidebar. The
        /// tool replaces most of its drawing.
        /// </summary>
        private static Type layerControllerViewType;

        /// <summary><c>UnityEditor.Graphs.LayerSettingsWindow</c>, the per-layer settings popup.</summary>
        private static Type layerSettingsWindowType;

        /// <summary><c>LayerControllerView.m_LayerScroll</c>, so the replacement drawing can keep the scroll position.</summary>
        private static FieldInfo layerScrollField;

        /// <summary><c>LayerControllerView.OnRemoveLayer</c>.</summary>
        private static MethodInfo onRemoveLayerMethod;

        /// <summary><c>LayerControllerView.RenameEnd</c>.</summary>
        private static MethodInfo layerRenameEndMethod;

        /// <summary><c>LayerSettingsWindow.ShowAtPosition</c>, called to open the settings popup at the tool's own rect.</summary>
        private static MethodInfo showAtPositionMethod;

        /// <summary><c>LayerControllerView.m_LayerList</c>, the underlying <c>ReorderableList</c>.</summary>
        private static FieldInfo layerListField;

        /// <summary><c>LayerControllerView.m_Host</c> — the Animator window the layer view belongs to.</summary>
        private static FieldInfo layerViewHostField;

        /// <summary><c>AnimatorControllerTool.m_AnimatorController</c>, reached through the layer view's host.</summary>
        private static FieldInfo toolAnimatorControllerField;

        /// <summary>
        /// <c>LayerControllerView.KeyboardHandling</c>, so the tool can run Unity's own key handling
        /// from its replacement <c>OnGUI</c>.
        /// </summary>
        private static MethodInfo keyboardHandlingMethod;

        // ── Menus, popups and the layer editor ──────────────────────────────────────────────────

        /// <summary>
        /// <c>UnityEditor.GenericMenu+MenuItem(GUIContent, bool, bool, GenericMenu.MenuFunction)</c>.
        /// </summary>
        /// <remarks>
        /// Constructed directly so that entries can be inserted into a menu Unity has already built,
        /// which <see cref="GenericMenu"/>'s public surface cannot do — it only appends.
        /// </remarks>
        private static ConstructorInfo menuItemConstructor;

        /// <summary><c>EditorGUI.AdvancedPopup(Rect, int, string[])</c>, the searchable popup with no public entry point.</summary>
        private static MethodInfo advancedPopupMethod;

        /// <summary>
        /// <c>GUIUtility.GetBuiltinSkin</c>, used to read a style out of a skin other than the one
        /// currently in effect.
        /// </summary>
        private static MethodInfo getBuiltinSkinMethod;

        /// <summary><c>AnimatorControllerTool.m_LayerEditor</c>.</summary>
        private static FieldInfo layerEditorField;

        /// <summary><c>AnimatorControllerTool.m_PreviewAnimator</c>.</summary>
        private static FieldInfo previewAnimatorField;

        /// <summary><c>AnimatorControllerTool.liveLink</c> — true while the window is mirroring a playing animator.</summary>
        private static PropertyInfo liveLinkProperty;

        /// <summary><c>LayerControllerView.selectedLayerIndex</c>.</summary>
        private static PropertyInfo selectedLayerIndexProperty;

        // ── Graph nodes ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>UnityEditor.Graphs.AnimationStateMachine.Node</c>, the base of every node in the state
        /// machine graph.
        /// </summary>
        private static Type stateMachineNodeBaseType;

        /// <summary><c>UnityEditor.Graphs.Node</c>.</summary>
        /// <remarks>
        /// Resolved and never read, like <see cref="graphStylesType"/>. Kept for the same reason.
        /// </remarks>
        private static Type graphNodeType;

        /// <summary><c>UnityEditor.Graphs.AnimationBlendTree.Node</c>, one box in the blend tree graph.</summary>
        private static Type blendTreeNodeType;

        /// <summary>
        /// <c>UnityEditor.Graphs.AnimationStateMachine.EdgeGUI</c>, resolved a second time — see
        /// <see cref="edgeGUIType"/>. This is the field the transition-colour and arrow-drawing
        /// patches are applied through.
        /// </summary>
        private static Type edgeGUIPatchType;

        /// <summary><c>EntryNode.MakeTransitionCallback</c>.</summary>
        private static MethodInfo entryNodeMakeTransitionCallback;

        /// <summary><c>AnyStateNode.MakeTransitionCallback</c>.</summary>
        private static MethodInfo anyStateNodeMakeTransitionCallback;

        /// <summary><c>StateNode.MakeTransitionCallback</c>.</summary>
        private static MethodInfo stateNodeMakeTransitionCallback;

        /// <summary><c>StateMachineNode.MakeTransitionCallback</c>.</summary>
        private static MethodInfo stateMachineNodeMakeTransitionCallback;

        /// <summary>
        /// <c>EdgeGUI.FindClosestEdge</c> — which arrow the mouse is over, needed to make the tool's
        /// own transition picking agree with Unity's.
        /// </summary>
        private static MethodInfo findClosestEdgeMethod;

        /// <summary>
        /// <c>AnimationStateMachine.Node.GenericMenuForStateMachineNode</c>, invoked to build Unity's
        /// own node context menu before the tool adds to it.
        /// </summary>
        private static MethodInfo genericMenuForStateMachineNodeMethod;

        /// <summary><c>StateNode.state</c>.</summary>
        private static FieldInfo stateNodeStateField;

        /// <summary><c>AnimationBlendTree.Node.motion</c>.</summary>
        private static FieldInfo blendTreeNodeMotionField;

        /// <summary><c>AnimationBlendTree.Node.children</c>.</summary>
        private static FieldInfo blendTreeNodeChildrenField;

        /// <summary><c>AnimationBlendTree.Node.parent</c>.</summary>
        private static PropertyInfo blendTreeNodeParentProperty;

        /// <summary>
        /// Binds everything the tool needs off the Animation window and its state object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.AnimationWindow</c>,
        /// <c>UnityEditor.AnimEditor</c> and the <c>UnityEditorInternal</c> animation-window family
        /// (<c>AnimationWindowState</c>, <c>AnimationWindowControl</c>,
        /// <c>AnimationWindowSelectionItem</c>, <c>AnimationWindowHierarchyGUI</c>,
        /// <c>AnimationWindowHierarchyNode</c>), as they are laid out in Unity 2019.4 through 2022.3.
        /// </para>
        /// <para>
        /// <b>Failure model, and it is the same for all six priming methods.</b> Resolution is eager
        /// and strict. Types go through <see cref="EditorUtils.RequireQualifiedType"/>, which uses
        /// the assembly-qualified name and no fallback scan, and <i>throws</i> rather than returning
        /// null. So a Unity version that has moved one of these types aborts the priming method at
        /// that line: every binding after it in the method is left null, and the callers further down
        /// the startup chain that would have used them fail later with null reference errors rather
        /// than with the original, informative message. Member lookups behave the other way round —
        /// <c>GetMethod</c>/<c>GetField</c>/<c>GetProperty</c> return null quietly, so a renamed
        /// <i>member</i> produces no error here at all and only surfaces at the point of use.
        /// </para>
        /// <para>
        /// The failure is permanent for the editor session either way. There is no retry, no
        /// per-field guard and no second attempt: the priming chain runs once per domain reload, from
        /// a single startup call, and nothing re-enters it. A partial failure therefore stays partial
        /// until the domain reloads.
        /// </para>
        /// </remarks>
        internal static void PrimeAnimationWindowReflection()
        {
            animationWindowType = EditorUtils.RequireQualifiedType(
                "UnityEditor.AnimationWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            animationWindowStateType = EditorUtils.RequireQualifiedType(
                "UnityEditorInternal.AnimationWindowState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            animationWindowHierarchyGUIType = EditorUtils.RequireQualifiedType(
                "UnityEditorInternal.AnimationWindowHierarchyGUI, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            animEditorType = EditorUtils.RequireQualifiedType(
                "UnityEditor.AnimEditor, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            animationWindowSelectionItemType = EditorUtils.RequireQualifiedType(
                "UnityEditorInternal.AnimationWindowSelectionItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            animationWindowControlType = EditorUtils.RequireQualifiedType(
                "UnityEditorInternal.AnimationWindowControl, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            hierarchyNodeBindingField = EditorUtils.RequireQualifiedType(
                    "UnityEditorInternal.AnimationWindowHierarchyNode, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
                .GetField("binding", BindingFlags.Instance | BindingFlags.Public);

            animationWindowStateProperty = animationWindowType.GetProperty("state", PrimingBindingFlags);
            activeAnimationClipProperty = animationWindowStateType.GetProperty("activeAnimationClip", PrimingBindingFlags);
            activeRootGameObjectProperty = animationWindowStateType.GetProperty("activeRootGameObject", PrimingBindingFlags);
            activeGameObjectProperty = animationWindowStateType.GetProperty("activeGameObject", PrimingBindingFlags);
            activeScriptableObjectProperty = animationWindowStateType.GetProperty("activeScriptableObject", PrimingBindingFlags);
            playControlsOnGUIMethod = animEditorType.GetMethod("PlayControlsOnGUI", PrimingBindingFlags);
        }

        /// <summary>
        /// Binds the three <c>UnityEditor.Graphs</c> drawing types whose colour properties the tool
        /// replaces.
        /// </summary>
        /// <remarks>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.Graphs.GraphGUI</c>,
        /// <c>UnityEditor.Graphs.Edge</c> and <c>UnityEditor.Graphs.Styles</c>. Failure model as on
        /// <see cref="PrimeAnimationWindowReflection"/>: eager, throws on a missing type, never
        /// retried.
        /// </remarks>
        private static void PrimeGraphStyleReflection()
        {
            graphGUIType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            graphEdgeType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.Edge, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            graphStylesType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }

        /// <summary>
        /// Binds the Animator window itself, the four graph and graph-GUI flavours it hosts, and the
        /// handful of its methods the tool calls directly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.Graphs.AnimatorControllerTool</c>,
        /// <c>Graph</c>, <c>AnimationBlendTree.GraphGUI</c>, <c>AnimationStateMachine.GraphGUI</c>,
        /// <c>AnimationStateMachine.Graph</c> and <c>AnimationStateMachine.EdgeGUI</c>.
        /// </para>
        /// <para>
        /// This runs first of the six, because the two that follow it read
        /// <see cref="animatorControllerToolType"/> out of it. Failure model as on
        /// <see cref="PrimeAnimationWindowReflection"/>.
        /// </para>
        /// </remarks>
        private static void PrimeAnimatorToolReflection()
        {
            animatorControllerToolType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            graphType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            blendTreeGraphGUIType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationBlendTree.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            stateMachineGraphGUIType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationStateMachine.GraphGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            stateMachineGraphType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationStateMachine.Graph, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            edgeGUIType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            rebuildGraphMethod = animatorControllerToolType.GetMethod("RebuildGraph", PrimingBindingFlags);
            addBreadCrumbMethod = animatorControllerToolType.GetMethod("AddBreadCrumb", PrimingBindingFlags);
            activeGraphGUIGetter = animatorControllerToolType.GetMethod("get_activeGraphGUI", PrimingBindingFlags);
            getEdgePointsMethod = edgeGUIType.GetMethod("GetEdgePoints", PrimingBindingFlags, null, new[] { typeof(Edge) }, null);
        }

        /// <summary>
        /// Binds the Animator window's layer sidebar, which the tool draws over almost entirely.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.Graphs.LayerControllerView</c> and
        /// <c>UnityEditor.Graphs.LayerSettingsWindow</c>, plus
        /// <c>AnimatorControllerTool.m_AnimatorController</c>.
        /// </para>
        /// <para>
        /// Depends on <see cref="PrimeAnimatorToolReflection"/> having run: the last line reads
        /// <see cref="animatorControllerToolType"/>, and throws a null reference if it has not.
        /// Failure model otherwise as on <see cref="PrimeAnimationWindowReflection"/>.
        /// </para>
        /// </remarks>
        private static void PrimeLayerControllerViewReflection()
        {
            layerControllerViewType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.LayerControllerView, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            layerSettingsWindowType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.LayerSettingsWindow, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            layerScrollField = layerControllerViewType.GetField("m_LayerScroll", PrimingBindingFlags);
            onRemoveLayerMethod = layerControllerViewType.GetMethod("OnRemoveLayer", PrimingBindingFlags);
            layerRenameEndMethod = layerControllerViewType.GetMethod("RenameEnd", PrimingBindingFlags);
            layerListField = layerControllerViewType.GetField("m_LayerList", PrimingBindingFlags);
            layerViewHostField = layerControllerViewType.GetField("m_Host", PrimingBindingFlags);
            keyboardHandlingMethod = layerControllerViewType.GetMethod("KeyboardHandling", PrimingBindingFlags);
            showAtPositionMethod = layerSettingsWindowType.GetMethod("ShowAtPosition", PrimingBindingFlags);
            toolAnimatorControllerField = animatorControllerToolType.GetField("m_AnimatorController", PrimingBindingFlags);
        }

        /// <summary>
        /// Binds the menu and popup internals the tool builds its own UI out of, and the Animator
        /// window members the layer view reads.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.GenericMenu+MenuItem</c>'s four-argument
        /// constructor, <c>EditorGUI.AdvancedPopup(Rect, int, string[])</c>,
        /// <c>GUIUtility.GetBuiltinSkin</c>, and <c>AnimatorControllerTool</c>'s
        /// <c>m_LayerEditor</c> / <c>m_PreviewAnimator</c> / <c>liveLink</c>.
        /// </para>
        /// <para>
        /// The <c>MenuItem</c> constructor is bound with an exact signature rather than by name
        /// alone, because a wrong overload here would be found and would then fail at construction
        /// time with an argument mismatch far from this line. Failure model otherwise as on
        /// <see cref="PrimeAnimationWindowReflection"/>; note that
        /// <see cref="Type.GetConstructor(Type[])"/> is a public-only lookup, so this one goes null
        /// rather than throwing if the constructor's signature changes.
        /// </para>
        /// <para>
        /// Depends on <see cref="PrimeAnimatorToolReflection"/> and
        /// <see cref="PrimeLayerControllerViewReflection"/> having run.
        /// </para>
        /// </remarks>
        private static void PrimeMenuAndLayerEditorReflection()
        {
            menuItemConstructor = EditorUtils.RequireQualifiedType(
                    "UnityEditor.GenericMenu+MenuItem, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
                .GetConstructor(new[] { typeof(GUIContent), typeof(bool), typeof(bool), typeof(GenericMenu.MenuFunction) });

            advancedPopupMethod = typeof(EditorGUI).GetMethod(
                "AdvancedPopup", PrimingBindingFlags, null, new[] { typeof(Rect), typeof(int), typeof(string[]) }, null);
            getBuiltinSkinMethod = typeof(GUIUtility).GetMethod("GetBuiltinSkin", PrimingBindingFlags);

            layerEditorField = animatorControllerToolType.GetField("m_LayerEditor", PrimingBindingFlags);
            previewAnimatorField = animatorControllerToolType.GetField("m_PreviewAnimator", PrimingBindingFlags);
            liveLinkProperty = animatorControllerToolType.GetProperty("liveLink", PrimingBindingFlags);
            selectedLayerIndexProperty = layerControllerViewType.GetProperty("selectedLayerIndex", PrimingBindingFlags);

            // Omitted here: the two RenameOverlayWrapper instances the decompiled method goes on to
            // build. See the deferral note in this file's header.
        }

        /// <summary>
        /// Binds the graph's node types and the per-node members the tool reads and re-invokes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Assumed Unity internals.</b> <c>UnityEditor.Graphs.Node</c>,
        /// <c>AnimationStateMachine.Node</c>, <c>AnimationBlendTree.Node</c> and
        /// <c>AnimationStateMachine.EdgeGUI</c>, plus <c>MakeTransitionCallback</c> on each of the
        /// four state-machine node kinds.
        /// </para>
        /// <para>
        /// The four <c>MakeTransitionCallback</c> lookups go through
        /// <see cref="AnimatorGraphReflection.TypeResolvers"/> rather than resolving their own type
        /// strings — that is what the shipped code does, and it means these four alone fail softly:
        /// a <see cref="TypeResolver"/> yields null for a type it cannot find, so a missing node type
        /// gives a null reference here instead of the "Type not found" exception the strict resolver
        /// would raise.
        /// </para>
        /// <para>
        /// Failure model otherwise as on <see cref="PrimeAnimationWindowReflection"/>.
        /// </para>
        /// </remarks>
        private static void PrimeGraphNodeReflection()
        {
            stateMachineNodeBaseType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationStateMachine.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            graphNodeType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            blendTreeNodeType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationBlendTree.Node, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            edgeGUIPatchType = EditorUtils.RequireQualifiedType(
                "UnityEditor.Graphs.AnimationStateMachine.EdgeGUI, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            entryNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.entryNode.ResolvedType
                .GetMethod("MakeTransitionCallback", PrimingBindingFlags);
            anyStateNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.anyStateNode.ResolvedType
                .GetMethod("MakeTransitionCallback", PrimingBindingFlags);
            stateNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.stateNode.ResolvedType
                .GetMethod("MakeTransitionCallback", PrimingBindingFlags);
            stateMachineNodeMakeTransitionCallback = AnimatorGraphReflection.TypeResolvers.stateMachineNode.ResolvedType
                .GetMethod("MakeTransitionCallback", PrimingBindingFlags);

            findClosestEdgeMethod = edgeGUIPatchType.GetMethod("FindClosestEdge", PrimingBindingFlags);
            genericMenuForStateMachineNodeMethod =
                stateMachineNodeBaseType.GetMethod("GenericMenuForStateMachineNode", PrimingBindingFlags);

            stateNodeStateField = AnimatorGraphReflection.TypeResolvers.stateNode.ResolvedType
                .GetField("state", PrimingBindingFlags);
            blendTreeNodeMotionField = blendTreeNodeType.GetField("motion", PrimingBindingFlags);
            blendTreeNodeChildrenField = blendTreeNodeType.GetField("children", PrimingBindingFlags);
            blendTreeNodeParentProperty = blendTreeNodeType.GetProperty("parent", PrimingBindingFlags);
        }
    }
}
