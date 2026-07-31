// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
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
//   m_BaseIdentifier        -> not ported (see below)
//   _RequestIdentifier      -> not ported (see below)
//   OnInspectorGUI()        -> not ported (see below)
//   method_0()              -> not ported (see below)
//   PushProperty()          -> not ported (see below)
//   PrepareProperty()       -> not ported (see below)
//   ReadProperty()          -> not ported (see below)
//   TestProperty()          -> not ported (see below)
//   InsertProperty(bool)    -> not ported (see below)
//   EnableProperty()        -> not ported (see below)
//   OnEnable() / OnDisable()-> not ported (see below)
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
// What is here is the editor's state: the foldout animation, the override flag, the eight cached
// SerializedProperty handles and the method that resolves them. Everything else in the shipped
// class is a call into ADOverhaul's private static surface, none of which is ported. Nothing below
// is stubbed; the unported members and their blocking dependencies are:
//
//   OnInspectorGUI()      line 2190 — the whole body. Blocked on ADOverhaul.FlushConfiguration
//                                     (7515), ReadConfiguration (6872), InsertConfiguration (6949),
//                                     SelectIdentifier (8228), RunConfiguration (5746),
//                                     TestConfiguration (6926), GetConfiguration (7495),
//                                     SortIdentifier (7904), ConcatIdentifier (8059),
//                                     EnableConfiguration (6966), and the `_Service` /
//                                     `_Rules` licence statics.
//   method_0()            line 2226 — the scene-view drawing entry point; see the note on scene
//                                     geometry below. Blocked on ADOverhaul.OrderConfiguration
//                                     (5840).
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
//   TestProperty()        line 2271 — [MenuItem ".../Toggle Editor", 899]. One line:
//                                     InsertProperty(editorOverrideEnabled).
//   InsertProperty(bool)  line 2277 — installs or removes the custom inspector; see the note on
//                                     inspector registration below. Blocked on
//                                     ADOEditorUtility.CancelStatus (2803) and RevertStatus (3741).
//   EnableProperty()      line 2291 — recomputes which shape dimensions are editable across the
//                                     current multi-selection; see the note on shape dimensions
//                                     below. Blocked on ADOverhaul.SortConfiguration (6391).
//   OnEnable()            line 2335 — ADOverhaul.SelectConfiguration (6536) + MapConfiguration
//                                     (6480). Deliberately left out rather than half-ported: a live
//                                     OnEnable that reinitialised the AnimBool without also
//                                     subscribing the scene-view callback would leave the editor in
//                                     a state the shipped build never produced.
//   OnDisable()           line 2341 — ADOverhaul.CancelConfiguration (6496).
//
// Two members are pure decompiler artifacts and are not ported at all:
//   RestartProperty() (2352) and DisableProperty() (2347). The first is ILSpy's rendering of a
//   lambda's capture of `this.target`; the second is the lifted body of the closure passed to
//   SelectIdentifier in OnInspectorGUI, marked [CompilerGenerated]. Neither is a real member.
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
// Consequence for this port: because no attribute is reproduced (there was none) and
// InsertProperty is not ported, adding this file does NOT change the inspector for
// VRCPhysBoneCollider. The VRChat SDK's own inspector stays in place. The type compiles as an
// unreferenced Editor subclass that Unity never instantiates.
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

using UnityEditor;
using UnityEditor.AnimatedValues;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// ADOverhaul's replacement inspector for <c>VRCPhysBoneCollider</c>.
    /// </summary>
    /// <remarks>
    /// Only the editor's cached state is reconstructed here — the drawing, the context-menu commands
    /// and the runtime inspector-override installation all call into ADOverhaul's private static
    /// surface, which is not ported. See the file header for the full list and for the two positional
    /// conventions (the property-array order and the shape/dimension mapping) that this class fixes.
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
