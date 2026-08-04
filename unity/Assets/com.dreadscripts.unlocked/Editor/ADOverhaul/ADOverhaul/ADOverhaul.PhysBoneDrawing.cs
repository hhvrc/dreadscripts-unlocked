// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the two members that draw a dynamics component's shape -- the inspector block and
// the scene-view handles. Line numbers are relative to the current snapshot; the decompiled names
// are the durable reference.
//
//   RunConfiguration    -> DrawShapeProperties, line 5746
//   OrderConfiguration  -> DrawShapeHandles,    line 5840
//
// Members folded into the two above rather than given their own declarations:
//
//   InvokeConfiguration      6410  the shape-toggle row dispatcher. Its only three call sites are
//                                  the three consecutive calls at 5812-5814, and its whole body is
//                                  a switch that selects which of the three rows to draw, so it is
//                                  expressed here as the three rows themselves.
//   SearchConfiguration      6717  one "property field + scene-edit toggle" row.
//   LoginConfiguration       6726  the scene-edit toggle button in that row.
//   ChangeConfiguration      6769  a null-guarded EditorGUILayout.PropertyField.
//   UpdateConfiguration      6712  Mathf.Max over a Transform's lossyScale.
//   CollectIdentifier        8299  } compiler-generated liftings of three local functions of
//   PrintIdentifier          8352  } OrderConfiguration (note their DisplayClass46_* parameters);
//   InterruptIdentifier      8366  } restored here as local functions, per the lambda rule.
//
// SearchConfiguration and LoginConfiguration are the only two of those that are genuinely shared:
// LoginConfiguration is also called from decompiled lines 3064, 3066, 3152, 3154 and 4279, and
// SearchConfiguration from 3055, all inside inspector regions that are not ported yet. They are
// reproduced here as private local functions (ShapePropertyRow / SceneEditToggle) rather than as
// file-level methods, deliberately: a file-level helper would collide by name with whatever the
// port of decompiled 6400-6800 chooses to call them, and this file must not be able to break that
// region's compilation. When that region lands, these two local functions should be deleted and
// their call sites pointed at the shared port. Nothing else in this file duplicates ported code.
//
// The four compiler-generated capture structs this region uses (_003C_003Ec__DisplayClass46_0,
// _1, _2 and _3, decompiled 5468-5512) are artifacts of the closures above and are not ported; the
// locals they carried are ordinary locals of DrawShapeHandles again. _2 in particular held a single
// float that is now the `delta` parameter of the ResizeToRadius local function.
//
// LICENCE GATE, NOT PORTED. Both methods are peppered with `if (!((Func<bool>)delegate { ... })())
// return;` blocks (decompiled 5754, 5788, 5881, 5961, 6021, 6044) whose bodies HMAC-SHA256 two
// outer-class strings against a hard-coded key and abandon the draw if the digest does not match.
// That is the protector's activation gate -- the same shape already removed from PhysBoneEditor,
// PhysBoneParameter and ObfuscationMarker -- and it is deliberately not reproduced. Note that in
// DrawShapeHandles two of those gates sit *between* handle blocks and one sits between the two
// height sliders, so with the gate failing the shipped tool would draw a partial handle set and
// skip the final write-back; removing them restores the intended, ungated path.
//
// Reused rather than reimplemented (all checked before writing anything here):
//   ADOEditorUtility.contents.edit / styles.compactIconButton  the pencil toggle and its style
//   ADOEditorUtility.ToggleButton                              decompiled ChangeStatus/PrepareStatus
//   ADOEditorUtility.validColor / errorColor / highlightColor   the palette
//   DreadScripts.Common.GUIColorScope                          the conditional tint scope
//   DreadScripts.ControllerEditor.PhysBoneColliderSnapshot     decompiled ADOEditorUtility.cs's
//     nested ShapeSnapshot (line 1264) is the same struct under a different obfuscated name, and
//     ShapeSnapshot.Apply() is its Restore(). ADOEditorUtility.VRChat.cs already established that
//     equivalence; this file follows it.
// ADOEditorUtility.RadiusHandle is deliberately NOT used: the shipped code calls Unity's own
// UnityEditor.Handles.RadiusHandle with handlesOnly:true here, not the tool's transcription of it.
//
// No static state is declared in this file. Everything it reads and writes -- editingRadius,
// editingHeight, editingPosition, editingRotation -- is already in ADOverhaul.State.cs.
//
// 2019 vs 2022: no behavioural divergence (2019 ReflectSystem, line 5727, and CountSystem, line
// 5821, with their local functions lifted as InitStruct and CancelStruct). The 2019 decompile
// was used to settle two places where ILSpy mangled the 2022 output, and both are noted at their
// sites: the position-handle switch, which 2022 renders as a `default:`/`goto IL_029b` loop spliced
// into the switch body and 2019 renders as the plain `for` loop it is, and the ordering of the two
// arms of the local-pivot branch inside it, which the two builds emit inverted from each other
// (2019 tests `==` and 2022 tests `!=` on the same pair). The only real textual difference is the
// snapshot's component field, called `target` in 2019 and `source` in 2022.

using System;
using System.Linq;
using DreadScripts.Common;
using DreadScripts.ControllerEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Draws the shape block shared by the PhysBone collider, contact sender and contact
        /// receiver inspectors: type, root transform, the radius/height/position rows with their
        /// scene-edit toggles, and the rotation row.
        /// </summary>
        /// <param name="target">
        /// The inspected component. Only used by the "set root to self" button, which is why that
        /// button acts on one object even in a multi-selection.
        /// </param>
        /// <param name="shapeProperties">
        /// The component's shape properties, positionally: 0 shapeType, 1 rootTransform, 2 radius,
        /// 3 height, 4 position, 5 rotation, and for a PhysBone collider 6 insideBounds and
        /// 7 bonesAsSpheres. Contacts pass a six-element array and the last two are read as null.
        /// </param>
        /// <param name="onShapeTypeChanged">
        /// Recomputes <see cref="shapeHasRadius"/>, <see cref="shapeHasHeight"/> and
        /// <see cref="shapeHasRotation"/> across the whole selection. It has to run after the type
        /// field commits, because those flags decide which scene-edit toggles stay available.
        /// </param>
        /// <param name="isPhysBoneCollider">
        /// Whether <paramref name="shapeProperties"/> is the eight-element collider form. Colliders
        /// have two properties contacts do not.
        /// </param>
        /// <remarks>
        /// The type field's change handler reads the shape type as it was <em>before</em> the field
        /// drew, not after, and turns off the toggles that the old type supported: leaving Sphere
        /// clears the rotation and height toggles, leaving Plane clears the height and radius ones.
        /// Reading the old value is what makes this work -- the toggles being cleared are the ones
        /// that were on-screen a moment ago.
        /// </remarks>
        private static void DrawShapeProperties(UnityEngine.Object target, SerializedProperty[] shapeProperties, Action onShapeTypeChanged, bool isPhysBoneCollider)
        {
            SerializedProperty shapeType = shapeProperties[0];
            SerializedProperty rootTransform = shapeProperties[1];
            SerializedProperty rotation = shapeProperties[5];
            SerializedProperty insideBounds = isPhysBoneCollider ? shapeProperties[6] : null;
            SerializedProperty bonesAsSpheres = isPhysBoneCollider ? shapeProperties[7] : null;
            int shapeTypeBeforeDraw = shapeType.intValue;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(shapeType, new GUIContent("Type"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (shapeTypeBeforeDraw == 0)
                    {
                        editingRotation = false;
                        editingHeight = false;
                    }
                    else if (shapeTypeBeforeDraw == 2)
                    {
                        editingHeight = false;
                        editingRadius = false;
                    }

                    onShapeTypeChanged();
                }

                if (isPhysBoneCollider && insideBounds != null)
                {
                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, insideBounds.boolValue, ADOEditorUtility.highlightColor, ADOEditorUtility.validColor))
                    {
                        insideBounds.boolValue = ADOEditorUtility.ToggleButton(
                            insideBounds.boolValue,
                            insideBounds.boolValue ? "Inside Bounds" : "Outside Bounds",
                            GUI.skin.button,
                            GUILayout.ExpandWidth(false));
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(rootTransform, new GUIContent("Root"));

                if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
                {
                    // SHIPPED BUG PRESERVED, twice over. The write goes through a throwaway
                    // SerializedObject built over `target` alone, so in a multi-selection only the
                    // active object's root is set even though every other row on this panel edits
                    // the whole selection; and it bypasses the inspector's own SerializedObject, so
                    // the "Root" field above still shows the old value until the inspector is next
                    // rebuilt. The Undo.RecordObject is redundant on top of that -- applying a
                    // SerializedObject already registers its own undo -- and leaves two entries on
                    // the stack for one click.
                    Undo.RecordObject(target, "Set Root to Self");

                    UnityEngine.Component component = target as UnityEngine.Component;
                    if (component)
                    {
                        SerializedObject serializedObject = new SerializedObject(component);
                        serializedObject.FindProperty("rootTransform").objectReferenceValue = component.transform;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            EditorGUILayout.Space();

            // Radius is meaningless on a Plane, height on anything but a Capsule. Both conditions
            // re-read the type property, which the field above may have just changed.
            if (shapeType.intValue != 2)
            {
                editingRadius = ShapePropertyRow(shapeProperties[2], editingRadius);
            }

            if (shapeType.intValue == 1)
            {
                editingHeight = ShapePropertyRow(shapeProperties[3], editingHeight);
            }

            editingPosition = ShapePropertyRow(shapeProperties[4], editingPosition);

            if (shapeType.enumValueIndex != 0)
            {
                using (new GUILayout.HorizontalScope())
                {
                    using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                    {
                        // Edited as Euler angles, stored as a quaternion, and only written back on
                        // a change so that typing into one axis does not renormalise the other two.
                        Vector3 eulerAngles = rotation.quaternionValue.eulerAngles;
                        eulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Rotation", "Rotation offset from the root transform"), eulerAngles);
                        if (check.changed)
                        {
                            rotation.quaternionValue = Quaternion.Euler(eulerAngles);
                        }
                    }

                    // Unlike every other toggle on this panel, the rotation one is drawn with a raw
                    // GUILayout.Toggle tinted with the stock Color.green/Color.red rather than with
                    // ADOEditorUtility.ToggleButton and the palette. The visible difference is small
                    // -- a slightly more saturated tint -- but it is what shipped.
                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, editingRotation, Color.green, Color.red))
                    {
                        editingRotation = GUILayout.Toggle(
                            editingRotation,
                            ADOEditorUtility.contents.edit,
                            ADOEditorUtility.styles.compactIconButton,
                            GUILayout.Width(18f),
                            GUILayout.Height(18f));
                    }
                }
            }

            if (isPhysBoneCollider && bonesAsSpheres != null)
            {
                EditorGUILayout.PropertyField(bonesAsSpheres);
            }

            // One property field with the pencil toggle that puts its value on a scene handle.
            // See the file header: local, not a method, so it cannot clash with the shared port.
            bool ShapePropertyRow(SerializedProperty property, bool editing)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(property);
                    return SceneEditToggle(editing, ADOEditorUtility.contents.edit);
                }
            }

            bool SceneEditToggle(bool editing, GUIContent content)
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, editing, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
                {
                    return ADOEditorUtility.ToggleButton(editing, content, ADOEditorUtility.styles.compactIconButton, GUILayout.Width(18f), GUILayout.Height(18f));
                }
            }
        }

        /// <summary>
        /// Draws the scene-view shape handles for a selection of PhysBone colliders or contacts and
        /// writes any drag straight back onto the components.
        /// </summary>
        /// <param name="target">The active object, which the handles are placed on and sized from.</param>
        /// <param name="targets">
        /// The whole selection. Which of these a drag reaches depends on the modifier key: none
        /// edits all of them relative to their own roots, Alt edits only <paramref name="target"/>,
        /// Shift edits all of them and then copies the active object's value onto every one.
        /// </param>
        /// <param name="componentKind">0 PhysBone collider, 1 contact sender, 2 contact receiver.</param>
        /// <param name="handleColor">Per-component-type handle tint, one of green, yellow and cyan.</param>
        /// <remarks>
        /// <para>
        /// Every component in <paramref name="targets"/> is snapshotted at the top, edited as plain
        /// numbers, and written back in one pass at the bottom. Going through the snapshots rather
        /// than the components is what lets a single drag apply consistently across a mixed
        /// selection -- the relative maths needs each entry's original value, not the value another
        /// entry's write has already changed.
        /// </para>
        /// <para>
        /// The snapshots hold local-space values, so most of this method is the conversion in and
        /// out of world space. <c>scale</c> is the largest component of the root's lossy scale, and
        /// is the single factor that maps a local radius or height onto the world-space handle; a
        /// non-uniformly scaled root therefore gets handles that do not match the drawn shape, which
        /// is the shipped behaviour and is what VRChat's own gizmos do too.
        /// </para>
        /// </remarks>
        private static void DrawShapeHandles(UnityEngine.Object target, UnityEngine.Object[] targets, int componentKind, Color handleColor)
        {
            // Enter or Escape anywhere in the scene view drops out of every editing mode at once.
            if (Event.current.type == EventType.KeyDown
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
            {
                editingRotation = false;
                editingPosition = false;
                editingHeight = false;
                editingRadius = false;
            }

            if (!target)
            {
                return;
            }

            Handles.color = handleColor;

            int activeIndex = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == target)
                {
                    activeIndex = i;
                    break;
                }
            }

            PhysBoneColliderSnapshot[] snapshots;
            if (componentKind == 0)
            {
                snapshots = targets.Select(t => new PhysBoneColliderSnapshot((VRCPhysBoneCollider)t)).ToArray();
            }
            else if (componentKind == 1)
            {
                snapshots = targets.Select(t => new PhysBoneColliderSnapshot((VRCContactSender)t)).ToArray();
            }
            else
            {
                snapshots = targets.Select(t => new PhysBoneColliderSnapshot((VRCContactReceiver)t)).ToArray();
            }

            Transform root = snapshots[activeIndex].rootTransform;
            float scale = Mathf.Max(root.lossyScale.x, root.lossyScale.y, root.lossyScale.z);
            int shapeType = snapshots[activeIndex].shapeType;

            Quaternion worldRotation = root.rotation * snapshots[activeIndex].rotation;
            Vector3 worldPosition = root.TransformPoint(snapshots[activeIndex].position);
            Vector3 up = worldRotation * Vector3.up;

            // Distance from the centre to either cap's centre, in local units. Negative when the
            // capsule is shorter than it is wide, which the Max below collapses to a sphere.
            float halfSpan = snapshots[activeIndex].height * 0.5f - snapshots[activeIndex].radius;
            float worldRadius = snapshots[activeIndex].radius * scale;
            Vector3 capOffset = worldRadius * up;
            Vector3 topCap = worldPosition + Mathf.Max(halfSpan * scale, 0f) * up;
            Vector3 bottomCap = worldPosition - Mathf.Max(halfSpan * scale, 0f) * up;

            // 0 all, 1 active only, 2 all then equalise. Shift wins over Alt.
            int mode = Event.current.shift ? 2 : (Event.current.alt ? 1 : 0);
            bool activeOnly = mode == 1;

            if (editingPosition)
            {
                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    bool localPivot = Tools.pivotRotation == PivotRotation.Local;
                    Vector3 moved = Handles.PositionHandle(worldPosition, localPivot ? worldRotation : Quaternion.identity);
                    if (check.changed)
                    {
                        RecordUndo("Adjust Position");

                        Vector3 delta = moved - worldPosition;

                        // In global-pivot "all" mode the delta stays in world space so each entry
                        // can convert it through its own root below. Every other combination works
                        // in the active object's root space.
                        if (localPivot || mode != 0)
                        {
                            delta = root.InverseTransformVector(delta);
                        }

                        switch (mode)
                        {
                            case 2:
                                snapshots[activeIndex].position += delta;
                                for (int i = 0; i < snapshots.Length; i++)
                                {
                                    snapshots[i].position = snapshots[activeIndex].position;
                                }

                                break;

                            case 1:
                                snapshots[activeIndex].position += delta;
                                break;

                            default:
                                for (int i = 0; i < snapshots.Length; i++)
                                {
                                    if (!localPivot)
                                    {
                                        snapshots[i].position += snapshots[i].rootTransform.InverseTransformVector(delta);
                                    }
                                    else if (snapshots[i].source == snapshots[activeIndex].source)
                                    {
                                        snapshots[activeIndex].position += delta;
                                    }
                                    else
                                    {
                                        // Re-express the active object's local delta in this
                                        // entry's own shape rotation, so a drag along the active
                                        // capsule's axis moves each other capsule along its own.
                                        snapshots[i].position += snapshots[i].rotation * Quaternion.Inverse(snapshots[activeIndex].rotation) * delta;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            // A sphere has no meaningful orientation, so the rotation handle is suppressed for it.
            if (editingRotation && shapeType != 0)
            {
                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    Quaternion rotated = Handles.RotationHandle(worldRotation, worldPosition);
                    if (check.changed)
                    {
                        RecordUndo("Adjust Rotation");

                        // Round-tripped through Euler angles rather than assigned directly. That
                        // normalises the sign of the quaternion and keeps the inspector's Euler
                        // readout from flipping by 360 degrees mid-drag.
                        Quaternion localRotation = Quaternion.Euler((Quaternion.Inverse(root.rotation) * rotated).eulerAngles);

                        switch (mode)
                        {
                            // SHIPPED BEHAVIOUR: unlike position and height, rotation treats Shift
                            // and no-modifier the same -- both assign the active object's rotation
                            // to every entry outright, rather than applying a relative delta.
                            case 0:
                            case 2:
                                for (int i = 0; i < snapshots.Length; i++)
                                {
                                    snapshots[i].rotation = localRotation;
                                }

                                break;

                            case 1:
                                snapshots[activeIndex].rotation = localRotation;
                                break;
                        }
                    }
                }
            }

            // A plane has no radius. A capsule gets a ring at each cap; a sphere gets one ring,
            // unrotated, at its centre.
            if (editingRadius && shapeType != 2)
            {
                bool isCapsule = shapeType == 1;

                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    float dragged = Handles.RadiusHandle(
                        isCapsule ? worldRotation : Quaternion.identity,
                        isCapsule ? topCap : worldPosition,
                        worldRadius,
                        handlesOnly: true) / scale;
                    ApplyRadius(check.changed, dragged);
                }

                if (isCapsule)
                {
                    using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                    {
                        float dragged = Handles.RadiusHandle(worldRotation, bottomCap, worldRadius, handlesOnly: true) / scale;
                        ApplyRadius(check.changed, dragged);
                    }
                }
            }

            // Height is only editable on a capsule; a sphere's height tracks its radius and a plane
            // has none.
            if (editingHeight && shapeType == 1)
            {
                Vector3 topGrip = topCap + capOffset;
                Vector3 bottomGrip = bottomCap - capOffset;

                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 moved = Handles.Slider(topGrip, up);
                    ApplyHeight(check.changed, topGrip, moved);
                }

                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 moved = Handles.Slider(bottomGrip, up * -1f);
                    ApplyHeight(check.changed, bottomGrip, moved);
                }
            }

            foreach (PhysBoneColliderSnapshot snapshot in snapshots)
            {
                snapshot.Restore();
            }

            void RecordUndo(string label)
            {
                if (activeOnly)
                {
                    Undo.RecordObject(target, label);
                }
                else
                {
                    Undo.RecordObjects(targets, label);
                }
            }

            // Grows or shrinks one snapshot by a radius delta, keeping a sphere's height locked to
            // its diameter and lengthening a capsule by the same amount at both ends.
            void ResizeToRadius(ref PhysBoneColliderSnapshot snapshot, float radiusDelta)
            {
                float newRadius = snapshot.radius + radiusDelta;
                float newHeight = snapshot.shapeType != 0 ? snapshot.height + radiusDelta * 2f : newRadius * 2f;
                snapshot.radius = newRadius;
                snapshot.height = newHeight;
            }

            void ApplyRadius(bool changed, float draggedRadius)
            {
                if (!changed)
                {
                    return;
                }

                RecordUndo("Adjust Radius");

                float radiusDelta = draggedRadius - snapshots[activeIndex].radius;

                switch (mode)
                {
                    case 2:
                        ResizeToRadius(ref snapshots[activeIndex], radiusDelta);
                        for (int i = 0; i < snapshots.Length; i++)
                        {
                            if (i == activeIndex)
                            {
                                continue;
                            }

                            // SHIPPED INCONSISTENCY: the radius is equalised to the active object's
                            // but the height is not -- a capsule keeps its own length and merely
                            // grows by the same delta, and a sphere is re-derived from the new
                            // shared radius. Shift on the height slider equalises outright.
                            snapshots[i].radius = snapshots[activeIndex].radius;
                            if (snapshots[i].shapeType == 0)
                            {
                                snapshots[i].height = snapshots[i].radius * 2f;
                            }
                            else
                            {
                                snapshots[i].height += radiusDelta * 2f;
                            }
                        }

                        break;

                    case 1:
                        ResizeToRadius(ref snapshots[activeIndex], radiusDelta);
                        break;

                    default:
                        for (int i = 0; i < snapshots.Length; i++)
                        {
                            ResizeToRadius(ref snapshots[i], radiusDelta);
                        }

                        break;
                }
            }

            void ApplyHeight(bool changed, Vector3 grip, Vector3 moved)
            {
                if (!changed)
                {
                    return;
                }

                RecordUndo("Adjust Height");

                // The slider is unsigned, so the direction is recovered by asking whether the grip
                // ended up nearer the shape's centre than it started. Doubled because moving one
                // cap by d lengthens the capsule at both ends, and divided by the root's scale to
                // get back into local units.
                bool towardsCentre = (worldPosition - moved).magnitude < (worldPosition - grip).magnitude;
                float heightDelta = (moved - grip).magnitude * (towardsCentre ? -1 : 1) * 2f / scale;

                switch (mode)
                {
                    case 2:
                        snapshots[activeIndex].height += heightDelta;
                        for (int i = 0; i < snapshots.Length; i++)
                        {
                            snapshots[i].height = snapshots[activeIndex].height;
                        }

                        break;

                    case 1:
                        snapshots[activeIndex].height += heightDelta;
                        break;

                    default:
                        for (int i = 0; i < snapshots.Length; i++)
                        {
                            snapshots[i].height += heightDelta;
                        }

                        break;
                }
            }
        }
    }
}
