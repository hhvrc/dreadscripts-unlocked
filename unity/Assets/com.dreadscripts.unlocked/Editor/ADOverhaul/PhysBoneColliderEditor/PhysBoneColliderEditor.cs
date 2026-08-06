// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the PhysBoneColliderEditor class, lines 2161-2356 of the current snapshot. Line
// numbers move with the snapshot; the member names below are the durable reference.
//
//   _FieldIdentifier        -> shapeFoldout,          line 122
//   advisorIdentifier       -> editorOverrideEnabled, line 137
//   _CreatorIdentifier      -> shapeType,             line 145
//   exporterIdentifier      -> rootTransform,         line 151
//   expressionIdentifier    -> radius,                line 154
//   _DecoratorIdentifier    -> height,                line 157
//   paramIdentifier         -> position,              line 163
//   _PrototypeIdentifier    -> rotation,              line 169
//   m_DispatcherIdentifier  -> insideBounds,          line 175
//   connectionIdentifier    -> bonesAsSpheres,        line 181
//   AwakeProperty()         -> CacheProperties(),     line 187
//   m_BaseIdentifier        -> colliderType,          line 2186
//   _RequestIdentifier      -> sdkColliderEditorType, line 2188
//   OnInspectorGUI()        -> unchanged,             line 2190
//   method_0()              -> OnSceneGUI,            line 2226
//   PushProperty()          -> not ported (see below)
//   PrepareProperty()       -> not ported (see below)
//   ReadProperty()          -> not ported (see below)
//   TestProperty()          -> ToggleEditorOverride,  line 2271
//   InsertProperty(bool)    -> InstallEditorOverride, line 2277
//   EnableProperty()        -> RecomputeShapeCapabilities, line 2291
//   OnEnable()              -> unchanged,             line 2335
//   OnDisable()             -> unchanged,             line 2341
//   DisableProperty()       -> not ported, decompiler artifact
//   RestartProperty()       -> not ported, decompiler artifact
//
// The decompiled class is nested inside the ADOverhaul class (`private sealed class
// PhysBoneColliderEditor : Editor`). ADOverhaul is not ported, so this is lifted to a top-level
// internal type in the same namespace; call sites in the original read
// `ADOverhaul.PhysBoneColliderEditor` and — because the type was a private nested class — every
// caller was inside ADOverhaul itself.
//
// ── PARTIAL PORT ────────────────────────────────────────────────────────────────────────────────
//
// The editor draws and registers itself now: state, shape panel, scene handles, enable/disable pair
// and the inspector override are all here. What is left out is the three context-menu conversion
// commands, each of which sits behind the licence gate and one or two unported helpers. Nothing
// below is stubbed; the unported members and their blocking dependencies are:
//
//   PushProperty()        line 2231 — [MenuItem "CONTEXT/VRCPhysBoneCollider/ADOverhaul/Move To
//                                     Empty", priority 896]. Body is self-contained
//                                     (ComponentUtility copy/paste onto a new sibling GameObject
//                                     named "<name> Collider", with the original transform's
//                                     position/rotation/localScale, undo-registered as "Move
//                                     Colliders To Empty", then Undo.DestroyObjectImmediate on the
//                                     source) but the whole thing is behind
//                                     ADOverhaul.MoveConfiguration (6561), the licence gate.
//   PrepareProperty()     line 2249 — [MenuItem ".../To Sender", 897]. Blocked on
//                                     MoveConfiguration and on the ADOEditorUtility extension
//                                     method `VerifyVal(this VRCPhysBoneCollider, GameObject)`
//                                     (ADOEditorUtility.cs 3941), which builds the VRCContactSender.
//   ReadProperty()        line 2260 — [MenuItem ".../To Receiver", 898]. Blocked on
//                                     MoveConfiguration and on `SortVal(this VRCPhysBoneCollider,
//                                     GameObject)` (ADOEditorUtility.cs 3966).
// (TestProperty and InsertProperty were on this list and have since been ported; they are MAP
// entries above, and the registration note below has been rewritten accordingly.)
// (OnInspectorGUI, method_0, EnableProperty, OnEnable and OnDisable were on this list and have
// since been ported; they are MAP entries above. The half-ported-OnEnable concern recorded here
// no longer applies: OnEnable now goes through ADOverhaul.BeginShapeInspectorSession, which
// subscribes the scene-view overlay, and OnDisable unsubscribes it.)
//
// The blocker names above are the ones the decompiled snapshot carried when this file was written,
// and several of those blockers have landed since. Re-checked during the audit pass: RunConfiguration
// is now ADOverhaul.DrawShapeProperties, OrderConfiguration is DrawShapeHandles, SortConfiguration is
// SetShapeCapabilities, SelectConfiguration is ResetFoldouts, CancelConfiguration is
// SetShapeEditOverlayActive, TestConfiguration is ApplyModifiedProperties, SelectIdentifier is
// DrawFoldoutBox, SortIdentifier is DrawToolHeader, ConcatIdentifier is DrawAnnouncementBanner, and
// ADOEditorUtility.CancelStatus / RevertStatus are FindType / OverrideCustomEditor -- all ported.
// Since then ReadConfiguration has landed as ADOverhaul.DrawTestModeToolbar, InsertConfiguration as
// PromptForColliderRestart and MapConfiguration as BeginShapeInspectorSession, which is what let the
// inspector body follow. What is still missing is FlushConfiguration, GetConfiguration,
// EnableConfiguration and MoveConfiguration, plus the licence statics, which the port declines to
// declare (see ADOverhaul.State.cs) -- the first three are the activation gate and are never coming
// back, and MoveConfiguration is the gate check the three conversion commands each open with.
//
// Two members are pure decompiler artifacts and are not ported at all:
//   RestartProperty() (2352) and DisableProperty() (2347). The first is ILSpy's rendering of a
//   lambda's capture of `this.target`; the second is the lifted body of the closure passed to
//   SelectIdentifier in OnInspectorGUI, marked [CompilerGenerated]. Neither is a real member.
//
// ── LICENCE GATE, NOT PORTED ────────────────────────────────────────────────────────────────────
//
// The shipped OnInspectorGUI is wrapped in the same activation gate as PhysBoneEditor's: an outer
// `if (FlushConfiguration())` whose else-branch draws the activation panel, and an inner inline
// Func<bool> that HMAC-SHA256s two outer-class strings against a hard-coded key and draws nothing
// unless the digest matches. Both are dropped, and the body they guarded runs unconditionally.
// GetConfiguration, the two-label "License: ..." / "Authorized For: ..." banner drawn between the
// commit and DrawToolHeader, is dropped with them.
//
// ── Inspector registration: there is NO [CustomEditor] attribute ────────────────────────────────
//
// This class carries no attribute of any kind. ADOverhaul does not register its inspectors
// declaratively; InsertProperty (2277) resolves `VRCPhysBoneCollider` and the SDK's own
// `VRCPhysBoneColliderEditor` by name through ADOEditorUtility.CancelStatus, then calls
// ADOEditorUtility.RevertStatus (ADOEditorUtility.cs 3741), which reflects into
// UnityEditor.CustomEditorAttributes, reaches the private static `kSCustomMultiEditors` dictionary,
// takes the entry already registered for VRCPhysBoneCollider and overwrites that entry's
// `m_InspectorType` field — swapping in either this type or the SDK's editor — before forcing
// InspectorWindow.RefreshInspectors. The override is therefore installed at runtime, is a toggle
// ("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Toggle Editor"), and is re-applied after every domain
// reload from ADOverhaul's startup path (ADOverhaul.cs 5552).
//
// Consequence for this port: InstallEditorOverride IS ported now, and ADOverhaul.InspectorInstall.cs
// calls it after every domain reload, so this type DOES take over the VRCPhysBoneCollider inspector.
//
// OnInspectorGUI is ported too, so what draws is ADOverhaul's shape panel rather than Unity's
// default property list. The per-component toggle
// ("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Toggle Editor") still hands the SDK's inspector back for
// the session. The three context-menu conversion commands remain unported -- see the list above --
// so this inspector's gear menu is shorter than the shipped one's.
//
// ── Scene geometry ──────────────────────────────────────────────────────────────────────────────
//
// This region contains no drawing code. `method_0` (2226) is the obfuscator's rename of what would
// have been OnSceneGUI — it is the only instance member matching that shape, and the sibling
// editors in the same file each carry an identically named `method_0` — and its entire body is a
// single call:
//
//     OrderConfiguration(target, targets, 0, Color.green)
//
// All handle placement, handle-space maths and axis conventions live in that method
// (ADOverhaul.cs 5840) and in the SceneView.duringSceneGui callback it cooperates with
// (CalculateConfiguration, 6060), none of which is in this region or ported. Rather than guess at
// conventions this file cannot observe, the two orderings this class *does* fix are recorded
// exactly, because both are positional and a transposition in either is silent:
//
//   1. The eight-element SerializedProperty array handed to RunConfiguration (2208, and the
//      identical copy in the compiler-generated 2349) is built in this order, which is NOT the
//      declaration order of the fields:
//
//        [0] shapeType      [1] rootTransform  [2] radius      [3] height
//        [4] position       [5] rotation       [6] insideBounds [7] bonesAsSpheres
//
//      RunConfiguration indexes it positionally: [0] is drawn as "Type" and its int value gates
//      everything else, [1] as "Root" (with the "S" set-to-self button), [5] is read as a
//      quaternion and edited through Euler angles, and [6]/[7] are only touched when its
//      `isres2` flag is set — which this editor always passes as true, since a collider has
//      insideBounds and bonesAsSpheres where a contact does not.
//
//   2. VRCPhysBoneColliderBase.ShapeType is Sphere = 0, Capsule = 1, Plane = 2, and
//      EnableProperty (2291) maps a multi-selection of those onto which dimensions may be edited.
//      Reading its three flags back through SortConfiguration's parameter order:
//
//        radius   editable if any selected collider is a Sphere or a Capsule  (shape 0 or 1)
//        height   editable if any selected collider is a Capsule              (shape 1 only)
//        rotation editable if any selected collider is a Capsule or a Plane   (shape 1 or 2)
//
//      A Sphere has no orientation and a Plane has neither radius nor height, which is why the
//      three predicates differ. SortConfiguration additionally forces off the scene-view edit mode
//      for any dimension that just became non-editable, so deselecting the last capsule cannot
//      leave a live height handle behind. EnableProperty's loop breaks early once all three flags
//      are set, so it is O(1) on large selections in the common case.
//
// ── Overlap with PhysBoneColliderSnapshot ───────────────────────────────────────────────────────
//
// None. DreadScripts.ControllerEditor.PhysBoneColliderSnapshot captures and restores a collider's
// shape as plain values; this editor never captures or restores anything — it only edits the live
// SerializedObject. The field sets look alike because both cover the same five shape fields, but
// there is no duplicated code to fold together and nothing here should be rewritten in terms of it.
//
// ── 2019 vs 2022 ────────────────────────────────────────────────────────────────────────────────
//
// Behaviourally identical; the 2019 copy is at ADOverhaul2019 lines 2152-2345. The only structural
// difference is in OnInspectorGUI, where 2019 writes the licence gate as an early-return guard
// (`if (!QuerySystem()) { ...; return; }`) and 2022 inverts it into if/else. Same evaluation order,
// same effect. Everything else differs only in obfuscated identifiers, including the helper names
// (2019: ResolveManager/RevertManager, CallTask; 2022: CancelStatus/RevertStatus, InsertProperty).
//
// Audit status: VERIFIED -- everything this file declares was diffed against the 2022 snapshot's
// PhysBoneColliderEditor: shapeFoldout (a one-element array seeded true), editorOverrideEnabled
// (true), the eight SerializedProperty fields, and CacheProperties statement by statement including
// its resolve order, which is not the declaration order. The two positional conventions the header
// pins down were re-derived from the snapshot rather than taken on trust: the eight-element array
// passed to DrawShapeProperties is [shapeType, rootTransform, radius, height, position, rotation,
// insideBounds, bonesAsSpheres], and EnableProperty's switch maps Capsule to all three capabilities,
// Sphere to radius, Plane (the default arm) to rotation, passed to SetShapeCapabilities in the order
// radius, height, rotation. Both artifact members were confirmed to be what the header says.
// One caveat: the MAP's line numbers for the state block (122-187) do not point into the stated
// decompiled region (2161-2356) or anywhere near these members in the snapshot; they were not used,
// and are left for the package-wide line-number sweep rather than fixed piecemeal here. The unported
// entries' numbers (2190-2352) are sound. The stale blocker list was corrected, see above.
//
// Second pass, when the inspector body landed: OnInspectorGUI, OnSceneGUI, OnEnable, OnDisable,
// RecomputeShapeCapabilities and the override installer were each transcribed statement by statement
// from 2190-2229, 2271-2289 and 2291-2341. The capability switch was re-derived a second time from
// the snapshot's numeric cases -- 0 Sphere, 1 Capsule, default Plane -- and matches the mapping this
// header already recorded. The two dropped members on that path are the licence gate and
// GetConfiguration, both noted above. The 2019 build was not read for the new members.

using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// ADOverhaul's replacement inspector for <c>VRCPhysBoneCollider</c>.
    /// </summary>
    /// <remarks>
    /// The shape panel, the scene handles, the enable/disable pair and the runtime inspector-override
    /// installation are all reconstructed. What is not is the three context-menu conversion commands,
    /// each of which opens with the licence gate. See the file header for that list and for the two
    /// positional conventions (the property-array order and the shape/dimension mapping) this class
    /// fixes.
    /// </remarks>
    internal sealed class PhysBoneColliderEditor : Editor
    {
        /// <summary>
        /// Animation state for the single "Shape" foldout, which starts expanded.
        /// </summary>
        /// <remarks>
        /// An array of one, and static rather than per-instance, because ADOverhaul's foldout helper
        /// takes an <see cref="AnimBool"/>[] and re-seeds every element on enable so the expansion
        /// state survives reselecting the component. The sibling editors in the shipped build use the
        /// same array shape with more entries.
        /// </remarks>
        private static readonly AnimBool[] shapeFoldout = { new AnimBool(true) };

        /// <summary>
        /// Whether ADOverhaul's inspector is currently installed in place of the SDK's, toggled from
        /// the component's context menu. Defaults to on, matching the shipped build, which installs
        /// the override during startup after every domain reload.
        /// </summary>
        private static bool editorOverrideEnabled = true;

        /// <summary>The component type being replaced, resolved lazily by name.</summary>
        private static Type colliderType;

        /// <summary>
        /// VRChat's own collider inspector, which the override displaces and the toggle puts back.
        /// </summary>
        private static Type sdkColliderEditorType;

        /// <summary>
        /// Context-menu entry that swaps between this inspector and VRChat's.
        /// </summary>
        /// <remarks>
        /// The SDK's collider inspector carries fields and warnings this one does not reproduce, so
        /// the escape hatch is worth having even now that the shape panel draws.
        /// </remarks>
        [MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Toggle Editor", false, 899)]
        private static void ToggleEditorOverride()
        {
            InstallEditorOverride(editorOverrideEnabled);
        }

        /// <summary>
        /// Points Unity's editor table for <c>VRCPhysBoneCollider</c> at this inspector, or back at
        /// the SDK's.
        /// </summary>
        /// <param name="revert">
        /// True to restore VRChat's inspector. The default installs this one, which is what the
        /// post-reload hook wants.
        /// </param>
        internal static void InstallEditorOverride(bool revert = false)
        {
            if (colliderType == null)
            {
                colliderType = ADOEditorUtility.FindType("VRCPhysBoneCollider");
            }

            if (sdkColliderEditorType == null)
            {
                sdkColliderEditorType = ADOEditorUtility.FindType("VRCPhysBoneColliderEditor");
            }

            editorOverrideEnabled = !revert;

            ADOEditorUtility.OverrideCustomEditor(
                colliderType,
                !editorOverrideEnabled ? sdkColliderEditorType : typeof(PhysBoneColliderEditor));
        }

        // The eight properties the shape section edits. They are resolved once per OnInspectorGUI
        // rather than in OnEnable because the shipped editor is re-pointed at a new selection without
        // being re-enabled.

        /// <summary>The collider shape, ordered Sphere, Capsule, Plane. Gates all the fields below.</summary>
        private SerializedProperty shapeType;

        /// <summary>Transform the shape is positioned and oriented relative to.</summary>
        private SerializedProperty rootTransform;

        /// <summary>Sphere and capsule radius; meaningless on a plane.</summary>
        private SerializedProperty radius;

        /// <summary>
        /// Capsule length along its local axis. Meaningless on a sphere or a plane.
        /// </summary>
        private SerializedProperty height;

        /// <summary>Offset of the shape from <see cref="rootTransform"/>.</summary>
        private SerializedProperty position;

        /// <summary>
        /// Orientation of the shape relative to <see cref="rootTransform"/>. Stored as a quaternion
        /// but presented as Euler angles, and hidden entirely for a sphere, which has no orientation.
        /// </summary>
        private SerializedProperty rotation;

        /// <summary>
        /// Whether bones are pushed towards the inside of the shape rather than out of it. Drawn as a
        /// labelled toggle button ("Inside Bounds" / "Outside Bounds") beside the shape type.
        /// </summary>
        private SerializedProperty insideBounds;

        /// <summary>
        /// Whether the affected bones collide as spheres instead of as their own shapes.
        /// </summary>
        private SerializedProperty bonesAsSpheres;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CacheProperties();

            // Applying test-mode changes can move colliders the running simulation is using, which
            // is what the restart prompt exists to offer.
            if (ADOverhaul.DrawTestModeToolbar(targets))
            {
                ADOverhaul.PromptForColliderRestart();
            }

            ADOverhaul.DrawFoldoutBox("Shape", shapeFoldout[0], null, () =>
                ADOverhaul.DrawShapeProperties(
                    target,
                    new[] { shapeType, rootTransform, radius, height, position, rotation, insideBounds, bonesAsSpheres },
                    RecomputeShapeCapabilities,
                    isPhysBoneCollider: true));

            if (ADOverhaul.ApplyModifiedProperties(serializedObject, targets))
            {
                SceneView.RepaintAll();
                ADOverhaul.colliderChangedDuringTest = true;
            }

            ADOverhaul.DrawToolHeader();
            ADOverhaul.DrawAnnouncementBanner();
        }

        /// <summary>Draws the collider's shape handles in the scene view.</summary>
        /// <remarks>
        /// Component kind 0 is the collider; the same helper draws contacts under the other kinds.
        /// </remarks>
        public void OnSceneGUI()
        {
            ADOverhaul.DrawShapeHandles(target, targets, 0, Color.green);
        }

        private void OnEnable()
        {
            ADOverhaul.ResetFoldouts(shapeFoldout, Repaint);
            ADOverhaul.BeginShapeInspectorSession(RecomputeShapeCapabilities);
        }

        public void OnDisable()
        {
            ADOverhaul.SetShapeEditOverlayActive(false);
        }

        /// <summary>
        /// Works out which of radius, height and rotation are meaningful across the whole selection
        /// and tells the shape drawing code, so a mixed selection offers the union of what its
        /// shapes support.
        /// </summary>
        /// <remarks>
        /// Sphere contributes radius, capsule contributes all three, and plane -- the default arm --
        /// contributes rotation. The loop stops early once all three are on, since nothing further
        /// can change the answer.
        /// <para>
        /// The switch arms read oddly against that description because the decompiled cases are
        /// keyed on the enum's numeric values: case 0 is Sphere, case 1 is Capsule, and the default
        /// arm is Plane. Transcribed by value rather than rewritten to named cases, so it stays
        /// diffable against the snapshot.
        /// </para>
        /// </remarks>
        private void RecomputeShapeCapabilities()
        {
            serializedObject.ApplyModifiedProperties();

            bool hasRotation = false;
            bool hasHeight = false;
            bool hasRadius = false;

            foreach (UnityEngine.Object inspected in targets)
            {
                VRCPhysBoneCollider collider = (VRCPhysBoneCollider)inspected;

                if (hasRadius && hasHeight && hasRotation)
                {
                    break;
                }

                switch ((int)collider.shapeType)
                {
                    case 0:
                        hasRadius = true;
                        break;

                    case 1:
                        hasRotation = true;
                        hasHeight = true;
                        hasRadius = true;
                        break;

                    default:
                        hasRotation = true;
                        break;
                }
            }

            ADOverhaul.SetShapeCapabilities(hasRadius, hasHeight, hasRotation);
        }

        /// <summary>
        /// Resolves the eight shape properties against the current <see cref="Editor.serializedObject"/>.
        /// </summary>
        private void CacheProperties()
        {
            rootTransform = serializedObject.FindProperty("rootTransform");
            shapeType = serializedObject.FindProperty("shapeType");
            insideBounds = serializedObject.FindProperty("insideBounds");
            bonesAsSpheres = serializedObject.FindProperty("bonesAsSpheres");
            radius = serializedObject.FindProperty("radius");
            height = serializedObject.FindProperty("height");
            position = serializedObject.FindProperty("position");
            rotation = serializedObject.FindProperty("rotation");
        }
    }
}
