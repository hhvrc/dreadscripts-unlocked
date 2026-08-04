// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the PhysBoneEditor class, lines 2358-4367 of the current snapshot, together with
// the compiler-generated display classes between lines 2404 and 2760 that belong to it. Line
// numbers move with the snapshot; the member names below are the durable reference.
//
// This file carries the type declaration and the scene-wide target caches. The rest of the type is
// split across sibling partials, each of which owns (and maps) the members it ported:
//
//   PhysBoneEditor.PropertyBinding.cs   nested AlgoAuthentication (decompiled line 2360)
//   PhysBoneEditor.Properties.cs        the SerializedProperty cache and PrintSingleton (line 4115)
//   PhysBoneEditor.ToolModes.cs         rulesAuthentication and the [SpecialName] mode accessors
//                                       (lines 2914-3006)
//   PhysBoneEditor.MembershipStates.cs  MoveSingleton / PublishSingleton (lines 3967-4073)
//   PhysBoneEditor.EndpointEditing.cs   SearchSingleton / LoginSingleton / PatchSingleton
//                                       (lines 4301-4361)
//   PhysBoneEditor.GizmoSettings.cs     InterruptSingleton (line 4210)
//
// Members in this file:
//
//   _MessageIdentifier -> selectedPhysBones, line 2980
//   _PolicyIdentifier -> scenePhysBones, line 2982
//   m_MapperIdentifier -> sceneColliders, line 2984
//   mappingIdentifier -> candidateTransforms, line 2986
//   m_QueueIdentifier -> membershipStates, line 2988, in PhysBoneEditor.MembershipStates.cs
//   ChangeSingleton() -> TargetObject(), line 4567
//
// LIFTED OUT OF ADOverhaul. The decompiled type is `private sealed class PhysBoneEditor` nested
// inside the static `ADOverhaul` class, which is not ported. It is lifted here to a top-level
// `internal` type in the same namespace. Nothing outside ADOverhaul referred to it by name, so the
// change of nesting has no call-site consequences.
//
// NOT A [CustomEditor]. Despite deriving from Editor, this type carries no CustomEditor attribute —
// there is no such attribute anywhere in the decompiled assembly. ADOverhaul installs it by
// reflecting into UnityEditor's internal `CustomEditorAttributes.kSCustomMultiEditors` table and
// overwriting the `m_InspectorType` recorded for VRCPhysBone (ADOEditorUtility.RevertStatus, line
// 3741 of ADOEditorUtility.cs), which is what lets the [ADO] Toggle Editor context menu swap back
// and forth with VRChat's own VRCPhysBoneEditor at runtime. Because that installation path is not
// ported, adding this type does not change which inspector Unity draws for a PhysBone.
//
// LARGELY NOT PORTED. Only the parts that compile against what exists in this package today are
// here; everything reachable from OnInspectorGUI or the scene-view GUI is left out, because those
// bodies call members of the unported outer ADOverhaul class (SelectIdentifier, CallConfiguration,
// LoginConfiguration, StopConfiguration, PushConfiguration, ReadConfiguration, TestConfiguration,
// GetConfiguration, SortIdentifier, NewIdentifier, WriteIdentifier, MoveIdentifier,
// PublishIdentifier, SelectConfiguration, PrintConfiguration, LogoutConfiguration,
// SetupConfiguration, EnableConfiguration, SearchConfiguration, NewConfiguration, FlushConfiguration
// and the _Service / _Account / _Iterator / m_Predicate / m_Registry / _Collection statics), the
// unported ADOSettings singleton, the unported ADOEditorUtility.BoneNode / BoneChainTree types and
// roughly two dozen unported ADOEditorUtility helpers, and the unported ADOverhaulWindow. The
// omitted members, by decompiled name and line number in the current snapshot:
//
//   _003C_003Ec (display class)                 2404   lambda cache; folded into its call sites
//   _003C_003Ec.DeleteParams                    2440   licence gate, see note below
//   _003C_003Ec__DisplayClass108_1 / _2         2599/2607  "add missing parameter" menu closures
//   _003C_003Ec__DisplayClass120_0 / _1 / _2    2638/2742/2755  property-edit drag handles (BoneNode)
//   _AnnotationIdentifier                       2770   AnimBool[8] section foldout animations
//   m_CodeIdentifier                            2772   GUILayoutUtils table state for the play-mode readout
//   _CallbackIdentifier                         2774   whether the ADO inspector is installed
//   m_ProcessorIdentifier                       2786   the live editor instance, for Repaint()
//   _TokenizerIdentifier                        2788   control id for dragging the tool overlay
//   m_ExceptionIdentifier                       2790   ResizeHandle for the tool overlay
//   _DefinitionAuthentication                   2918   hot control of the endpoint slider
//   initializerAuthentication                   2920   remembered endpoint slider direction
//   m_TokenAuthentication / getterAuthentication 2922  cached VRCPhysBone / VRCPhysBoneEditor types
//   threadAuthentication                        2926   one-shot guard for CollectSingleton
//   OnInspectorGUI                              3008   the entire inspector layout
//   method_0 (OnSceneGUI)                       3336   endpoint and property-edit scene handles
//   VerifySingleton                             3359   static SceneView.duringSceneGui handler
//   SetSingleton                                3469   the on-scene tool-selection overlay
//   SortSingleton                               3548   "Gizmos Disabled" warning panel
//   InvokeSingleton                             3565   the on-scene editing/tooltip overlay
//   CustomizeSingleton                          3636   endpoint position handles
//   ConcatSingleton                             3695   Ctrl+E / Ctrl+T and Enter/Escape shortcuts
//   MapSingleton                                3742   walks a BoneChainTree evaluating a curve
//   FillSingleton                               3756   curve-key editing maths for a dragged handle
//   CancelSingleton                             3832   the property-edit scene handles
//   CollectSingleton                            4075   builds the play-mode parameter readout table
//   OnEnable / OnDisable                        4089/4106  scene-callback registration and target caching
//   ViewSingleton                               4224   the "Advanced" integration-type toggle
//   PostSingleton / ListSingleton               4239/4265  one property + curve + edit-button row
//   ForgotSingleton                             4273   unused variant of PostSingleton
//   UpdateSingleton                             4283   the inline curve field
//   SelectSingleton / WriteSingleton            3948/3953  the [ADO] Toggle Editor context menu
//
// LICENCE GATE, NOT PORTED. OnInspectorGUI (line 3008) opens by invoking an inline
// Func<bool> that HMAC-SHA256s two outer-class strings against a hard-coded key and returns without
// drawing anything if the digest does not match; _003C_003Ec.DeleteParams is the same check hoisted
// into the lambda cache. This is the protector's activation gate, identical in shape to the
// remnants removed from PhysBoneParameter and ObfuscationMarker, and it is deliberately not
// reproduced.
//
// Audit status: PARTIAL -- the six MAP entries above were re-checked against decompiled/ and their
// line numbers corrected (the field block is at 2980-2988 and ChangeSingleton at 4567 in the
// post-561e9ec snapshot, not 2776-2784/4363). The sibling-partial list and the omitted-member
// table still carry pre-re-snapshot numbers and were not re-checked; the numbers each
// sibling partial states in its own header are the ones kept current. The omitted-member table
// above was likewise not re-checked.

using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// ADOverhaul's replacement inspector for <see cref="VRCPhysBone"/>: a re-laid-out property
    /// panel plus a set of scene-view tools for editing endpoints, curve-driven properties, ignore
    /// transforms and collider assignments across a multi-selection of PhysBones at once.
    /// </summary>
    /// <remarks>
    /// Nearly all of the type's state is <c>static</c> in the original, including the
    /// <see cref="SerializedProperty"/> cache and the scene caches below. That is deliberate rather
    /// than sloppy: the scene-view callback (<c>VerifySingleton</c>) is a static
    /// <see cref="SceneView.duringSceneGui"/> handler and has no instance to reach through, so the
    /// inspector instance publishes what it knows into statics on enable and the handler reads them
    /// back. It works because Unity only ever has one PhysBone inspector alive at a time; two would
    /// trample each other's caches.
    /// <para>
    /// This reconstruction is a partial port — see the file header for what was left out and why.
    /// </para>
    /// </remarks>
    internal sealed partial class PhysBoneEditor : Editor
    {
        /// <summary>
        /// Every PhysBone currently being inspected, i.e. <see cref="Editor.targets"/> narrowed to
        /// <see cref="VRCPhysBone"/>. The scene tools branch on whether this holds one or many:
        /// with a multi-selection, Alt restricts an edit to the PhysBone under the handle and Shift
        /// assigns the same absolute value to all of them.
        /// </summary>
        /// <remarks>
        /// Populated by <c>OnEnable</c>, which is not ported; nothing in this reconstruction assigns
        /// it. See the omissions list.
        /// </remarks>
        internal static VRCPhysBone[] selectedPhysBones;

        /// <summary>
        /// Every PhysBone under the same avatar root as the inspected one. These are the sources the
        /// "Ignore Copy" and "Collision Copy" tools offer as scene-view pick targets.
        /// </summary>
        internal static VRCPhysBone[] scenePhysBones;

        /// <summary>
        /// Every collider under the same avatar root, in the order the collider-selection tool draws
        /// and indexes them. <see cref="membershipStates"/> is kept parallel to this array.
        /// </summary>
        internal static VRCPhysBoneCollider[] sceneColliders;

        /// <summary>
        /// Every transform reachable from the inspected PhysBones' root transforms — the candidates
        /// for the ignore-transform list, and the array
        /// <see cref="RefreshIgnoreTransformStates"/> reports membership against.
        /// </summary>
        /// <remarks>
        /// Built by concatenating each selected PhysBone's hierarchy without de-duplicating, so
        /// overlapping selections list a shared transform more than once. Ported as-is.
        /// </remarks>
        internal static Transform[] candidateTransforms;

        /// <summary>
        /// The object being inspected. A trivial forwarder to <see cref="Editor.target"/>; every
        /// call site in the original casts the result straight to <see cref="VRCPhysBone"/>, so a
        /// non-PhysBone target would throw there rather than here.
        /// </summary>
        private UnityEngine.Object TargetObject()
        {
            return target;
        }
    }
}
