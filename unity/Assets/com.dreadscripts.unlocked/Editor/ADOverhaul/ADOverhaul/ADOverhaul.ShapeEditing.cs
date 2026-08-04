// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The shape editor shared by all three primitive-shaped VRChat components — PhysBone colliders,
// contact senders and contact receivers. One inspector layout and one set of scene-view handles,
// parameterised by which family the inspected component belongs to. Line numbers move with the
// snapshot; the member names are the durable reference.
//
//   record             (line 5636) -> editingRadius
//   _Resolver          (line 5638) -> editingHeight
//   _Tag               (line 5640) -> editingPosition
//   filter             (line 5642) -> editingRotation
//   m_Factory          (line 5644) -> radiusEditable
//   _Attribute         (line 5646) -> heightEditable
//   task               (line 5648) -> rotationEditable
//   RunConfiguration      (line 5746) -> DrawShapeProperties
//   OrderConfiguration    (line 5840) -> DrawShapeHandles
//   CollectIdentifier     (line 8299) -> ApplyRadiusDrag        (local function)
//   PrintIdentifier       (line 8352) -> ResizeAroundRadius     (local function)
//   InterruptIdentifier   (line 8366) -> ApplyHeightDrag        (local function)
//   CalculateConfiguration(line 6060) -> OnSceneGuiShapeTools
//   CalcConfiguration     (line 6090) -> DrawShapeToolOverlay
//   InvokeConfiguration   (line 6410) -> DrawShapeToolToggle
//   SortConfiguration     (line 6391) -> SetShapeToolAvailability
//   CancelConfiguration   (line 6496) -> SetShapeToolsActive
//   MapConfiguration      (line 6480) -> BeginShapeInspector
//   UpdateConfiguration   (line 6712) -> MaxScale
//
// The compiler-generated display structs _003C_003Ec__DisplayClass46_0 through _3 (lines 5468-5513)
// exist only to pass DrawShapeHandles' locals into the three lifted local functions above; they get
// no file and are dissolved back into locals and closures. The three `t2 => new ShapeSnapshot(...)`
// lambdas from _003C_003Ec are inlined at their use site.
//
// NOT PORTED, and deliberately absent: the field `@event` (line 5634), a bool with no reader
// anywhere in either build.
//
// LICENCE CODE REMOVED, six regions, all of them the same inline tripwire — a Func<bool> that
// HMAC-SHA256s the licence key and compares the digest with a cached response field:
//   DrawShapeProperties  export 5754  before the Type row      -> guard dropped, row draws
//   DrawShapeProperties  export 5788  before the Root row      -> guard dropped, row draws
//   DrawShapeHandles     export 5881  before the handle maths  -> guard dropped, handles draw
//   DrawShapeHandles     export 5961  before the rotation handle -> guard dropped
//   DrawShapeHandles     export 6021 and 6044, opening and closing the height-slider block
//                                                              -> both dropped, sliders draw
//   OnSceneGuiShapeTools export 6067  as the condition of the `if` that draws the overlay
//                                                              -> overlay drawn unconditionally
// Every one of them was a bare early-return (or, in the last case, the whole condition), so
// removing them leaves the surrounding method drawing exactly what it draws when licensed. No
// method in this group existed only to serve the gate.
//
// Audit status: VERIFIED against export -- every method re-read against lines 5746-6105, 6391-6431,
// 6480-6507, 6712-6715 and 8298-8407 on 2026-08-04.

using System;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        // ── Which scene-view handle is currently armed ──────────────────────────────────────
        // Static rather than per-inspector because the scene-view callback that reads them is a
        // static SceneView.duringSceneGui handler.

        private static bool editingRadius;

        private static bool editingHeight;

        private static bool editingPosition;

        private static bool editingRotation;

        // ── Which handles the inspected shape supports at all ───────────────────────────────
        // Recomputed from the selection's shape types whenever one of them changes; a handle whose
        // flag is off is neither offered nor drawn.

        private static bool radiusEditable;

        private static bool heightEditable;

        private static bool rotationEditable;

        /// <summary>
        /// Draws the shape block every one of the three inspectors opens with: type, root
        /// transform, the three dimension rows with their scene-edit toggles, and the rotation
        /// offset.
        /// </summary>
        /// <param name="properties">
        /// In fixed order: shapeType, rootTransform, radius, height, position, rotation, and — for
        /// a PhysBone collider only — insideBounds and bonesAsSpheres.
        /// </param>
        /// <param name="onShapeTypeChanged">
        /// Invoked after the type is edited, so the caller can recompute which handles the new
        /// selection supports.
        /// </param>
        /// <param name="isPhysBoneCollider">
        /// Whether <paramref name="properties"/> carries the two extra collider-only entries.
        /// </param>
        /// <remarks>
        /// The armed handles are cleared from the *previous* type rather than the new one, which is
        /// what makes "switch a capsule to a sphere while dragging its height" leave nothing armed
        /// that the new shape cannot express.
        /// </remarks>
        internal static void DrawShapeProperties(UnityEngine.Object target, SerializedProperty[] properties, Action onShapeTypeChanged, bool isPhysBoneCollider)
        {
            SerializedProperty shapeType = properties[0];
            SerializedProperty rootTransform = properties[1];
            SerializedProperty rotation = properties[5];
            SerializedProperty insideBounds = isPhysBoneCollider ? properties[6] : null;
            SerializedProperty bonesAsSpheres = isPhysBoneCollider ? properties[7] : null;

            int previousShapeType = shapeType.intValue;

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(shapeType, new GUIContent("Type"));
                if (EditorGUI.EndChangeCheck())
                {
                    if (previousShapeType == 0)
                    {
                        editingRotation = false;
                        editingHeight = false;
                    }
                    else if (previousShapeType == 2)
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

                // Writes through a second SerializedObject rather than through the property in
                // hand, because the inspected selection may be several components and this only
                // ever sets the one the row belongs to.
                if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
                {
                    Undo.RecordObject(target, "Set Root to Self");
                    UnityEngine.Component component = target as UnityEngine.Component;
                    if ((bool)component)
                    {
                        SerializedObject serializedObject = new SerializedObject(component);
                        serializedObject.FindProperty("rootTransform").objectReferenceValue = component.transform;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            EditorGUILayout.Space();

            DrawShapeToolToggle(properties, 0);
            DrawShapeToolToggle(properties, 1);
            DrawShapeToolToggle(properties, 2);

            // A sphere has no meaningful orientation, so the row is absent rather than disabled.
            if (shapeType.enumValueIndex != 0)
            {
                using (new GUILayout.HorizontalScope())
                {
                    using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        Vector3 euler = rotation.quaternionValue.eulerAngles;
                        euler = EditorGUILayout.Vector3Field(new GUIContent("Rotation", "Rotation offset from the root transform"), euler);
                        if (changeCheck.changed)
                        {
                            rotation.quaternionValue = Quaternion.Euler(euler);
                        }
                    }

                    // Green/red rather than the shared palette: this row's toggle is drawn straight
                    // instead of through ToggleIconButton, as shipped.
                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, editingRotation, Color.green, Color.red))
                    {
                        editingRotation = GUILayout.Toggle(editingRotation, ADOEditorUtility.contents.edit, ADOEditorUtility.styles.compactIconButton, GUILayout.Width(18f), GUILayout.Height(18f));
                    }
                }
            }

            if (isPhysBoneCollider)
            {
                DrawOptionalProperty(bonesAsSpheres);
            }
        }

        /// <summary>
        /// One dimension row: the property plus the toggle that arms its scene-view handle, drawn
        /// only when the current shape type has that dimension.
        /// </summary>
        /// <param name="row">0 = radius, 1 = height, 2 = position.</param>
        internal static void DrawShapeToolToggle(SerializedProperty[] properties, int row)
        {
            int shapeType = properties[0].intValue;

            switch (row)
            {
                case 2:
                    editingPosition = DrawPropertyWithEditToggle(properties[4], editingPosition);
                    break;
                case 0:
                    // Type 2 is the plane, which has no radius.
                    if (shapeType != 2)
                    {
                        editingRadius = DrawPropertyWithEditToggle(properties[2], editingRadius);
                    }

                    break;
                case 1:
                    // Only the capsule (type 1) has a height.
                    if (shapeType == 1)
                    {
                        editingHeight = DrawPropertyWithEditToggle(properties[3], editingHeight);
                    }

                    break;
            }
        }

        /// <summary>
        /// Records which handles the current selection supports, and disarms any that it does not.
        /// </summary>
        internal static void SetShapeToolAvailability(bool radius, bool height, bool rotation)
        {
            radiusEditable = radius;
            heightEditable = height;
            rotationEditable = rotation;

            if (!radiusEditable)
            {
                editingRadius = false;
            }

            if (!heightEditable)
            {
                editingHeight = false;
            }

            if (!rotationEditable)
            {
                editingRotation = false;
            }
        }

        /// <summary>
        /// Subscribes or unsubscribes the shape tools' scene-view callback.
        /// </summary>
        /// <remarks>
        /// Always unsubscribes first, so that repeated enables cannot stack the handler. Turning
        /// the tools off also restores Unity's own transform gizmo, which
        /// <see cref="OnSceneGuiShapeTools"/> hides while a handle is armed.
        /// </remarks>
        internal static void SetShapeToolsActive(bool active)
        {
            SceneView.duringSceneGui -= OnSceneGuiShapeTools;
            if (active)
            {
                SceneView.duringSceneGui += OnSceneGuiShapeTools;
            }
            else
            {
                Tools.hidden = false;
            }
        }

        /// <summary>
        /// What a shape inspector runs from <c>OnEnable</c>: arm the scene callback, let the
        /// inspector rebuild its own caches, then refresh the avatar-derived tables.
        /// </summary>
        internal static void BeginShapeInspector(Action refreshInspector)
        {
            SetShapeToolsActive(true);
            refreshInspector();
            RefreshAvatars(ref targetAvatar, ref sceneAvatars);
            RefreshAvatarTables();
        }

        /// <summary>
        /// Draws the scene-view overlay listing the handles that can be armed, while at least one
        /// of them is.
        /// </summary>
        /// <remarks>
        /// Unity's transform gizmo is hidden for as long as any handle is armed, because both would
        /// otherwise sit on the same point and compete for the drag.
        /// </remarks>
        internal static void OnSceneGuiShapeTools(SceneView sceneView)
        {
            if (!editingPosition && !editingRotation && !editingRadius && !editingHeight)
            {
                return;
            }

            Tools.hidden = true;

            int rows = 1;
            if (radiusEditable)
            {
                rows++;
            }

            if (heightEditable)
            {
                rows++;
            }

            if (rotationEditable)
            {
                rows++;
            }

            DrawSceneViewPanel(sceneView, "Editing", DrawShapeToolOverlay, 200f, 45 + 20 * rows);
        }

        /// <summary>The overlay's body: one toggle per handle the shape supports.</summary>
        internal static void DrawShapeToolOverlay()
        {
            if (radiusEditable)
            {
                ToggleButton("Radius", ref editingRadius);
            }

            if (heightEditable)
            {
                ToggleButton("Height", ref editingHeight);
            }

            ToggleButton("Position", ref editingPosition);

            if (rotationEditable)
            {
                ToggleButton("Rotation", ref editingRotation);
            }
        }

        /// <summary>
        /// The largest of a transform's three lossy scale components, used to convert handle
        /// distances measured in world space back into the local units the shape fields are in.
        /// </summary>
        /// <remarks>
        /// A single scalar, so a non-uniformly scaled root gives handles that do not track the
        /// cursor exactly. VRChat's own shape fields are scalars too, so there is nothing better to
        /// pick.
        /// </remarks>
        internal static float MaxScale(Transform transform)
        {
            return Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        /// <summary>
        /// Draws the armed scene-view handles for the inspected shape and writes what the user
        /// drags back onto every component in the selection.
        /// </summary>
        /// <param name="componentFamily">
        /// 0 = <see cref="VRCPhysBoneCollider"/>, 1 = <see cref="VRCContactSender"/>,
        /// 2 = <see cref="VRCContactReceiver"/>. Selects which snapshot constructor reads the
        /// targets; the three families describe the same primitives through unrelated base classes.
        /// </param>
        /// <param name="color">The handle colour, one per family, so a mixed scene reads.</param>
        /// <remarks>
        /// <para>
        /// Three edit modes, chosen live by the modifier held while dragging:
        /// none applies the change to every selected component, Alt restricts it to the one under
        /// the handle, and Shift assigns the resulting absolute value to all of them.
        /// </para>
        /// <para>
        /// All edits go through <see cref="ADOEditorUtility.ShapeSnapshot"/> copies and are written
        /// back at the end, so one drag produces one write per component regardless of how many
        /// handles it touched.
        /// </para>
        /// <para>
        /// Return, Enter and Escape disarm every handle, which is the only way out of the tools
        /// that does not require going back to the inspector.
        /// </para>
        /// </remarks>
        internal static void DrawShapeHandles(UnityEngine.Object target, UnityEngine.Object[] targets, int componentFamily, Color color)
        {
            if (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
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

            Handles.color = color;

            int index = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == target)
                {
                    index = i;
                    break;
                }
            }

            ADOEditorUtility.ShapeSnapshot[] shapes;
            if (componentFamily == 0)
            {
                shapes = targets.Select(t => new ADOEditorUtility.ShapeSnapshot((VRCPhysBoneCollider)t)).ToArray();
            }
            else if (componentFamily == 1)
            {
                shapes = targets.Select(t => new ADOEditorUtility.ShapeSnapshot((VRCContactSender)t)).ToArray();
            }
            else
            {
                shapes = targets.Select(t => new ADOEditorUtility.ShapeSnapshot((VRCContactReceiver)t)).ToArray();
            }

            Transform root = shapes[index].rootTransform;
            float scale = MaxScale(root);
            int shapeType = shapes[index].shapeType;

            Quaternion worldRotation = root.rotation * shapes[index].rotation;
            Vector3 worldPosition = root.TransformPoint(shapes[index].position);
            Vector3 axis = worldRotation * Vector3.up;

            // The distance from the centre to either cap's centre: half the height less one radius,
            // clamped at zero so a capsule shorter than its own diameter reads as a sphere.
            float halfSegment = shapes[index].height * 0.5f - shapes[index].radius;
            float scaledRadius = shapes[index].radius * scale;
            Vector3 radiusOffset = scaledRadius * axis;
            Vector3 topCenter = worldPosition + Mathf.Max(halfSegment * scale, 0f) * (root.rotation * shapes[index].rotation * Vector3.up);
            Vector3 bottomCenter = worldPosition - Mathf.Max(halfSegment * scale, 0f) * (root.rotation * shapes[index].rotation * Vector3.up);

            // 0 = every selected shape, 1 = only the one under the handle, 2 = assign one value to
            // all of them.
            int mode = Event.current.shift ? 2 : Event.current.alt ? 1 : 0;
            bool soloEdit = mode == 1;

            if (editingPosition)
            {
                using EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope();

                bool localPivot = Tools.pivotRotation == PivotRotation.Local;
                Vector3 dragged = Handles.PositionHandle(worldPosition, localPivot ? worldRotation : Quaternion.identity);

                if (changeCheck.changed)
                {
                    if (soloEdit)
                    {
                        Undo.RecordObject(target, "Adjust Position");
                    }
                    else
                    {
                        Undo.RecordObjects(targets, "Adjust Position");
                    }

                    Vector3 delta = dragged - worldPosition;
                    if (localPivot || mode != 0)
                    {
                        delta = root.InverseTransformVector(delta);
                    }

                    switch (mode)
                    {
                        case 2:
                            shapes[index].position += delta;
                            for (int i = 0; i < shapes.Length; i++)
                            {
                                shapes[i].position = shapes[index].position;
                            }

                            break;
                        case 1:
                            shapes[index].position += delta;
                            break;
                        default:
                            for (int i = 0; i < shapes.Length; i++)
                            {
                                if (!localPivot)
                                {
                                    // The delta is still in world space here, so each shape
                                    // converts it into its own root's space.
                                    shapes[i].position += shapes[i].rootTransform.InverseTransformVector(delta);
                                }
                                else if (shapes[i].source != shapes[index].source)
                                {
                                    shapes[i].position += shapes[i].rotation * Quaternion.Inverse(shapes[index].rotation) * delta;
                                }
                                else
                                {
                                    // As shipped: the shape sharing the dragged one's source is
                                    // moved by writing to the dragged one, so a selection with
                                    // several entries off the same component moves it once per
                                    // entry.
                                    shapes[index].position += delta;
                                }
                            }

                            break;
                    }
                }
            }

            if (editingRotation && shapeType != 0)
            {
                using EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope();

                Quaternion dragged = Handles.RotationHandle(worldRotation, worldPosition);
                if (changeCheck.changed)
                {
                    if (soloEdit)
                    {
                        Undo.RecordObject(target, "Adjust Rotation");
                    }
                    else
                    {
                        Undo.RecordObjects(targets, "Adjust Rotation");
                    }

                    // Absolute rather than relative, which is why modes 0 and 2 behave the same
                    // here: there is no per-shape delta to apply.
                    Quaternion local = Quaternion.Euler((Quaternion.Inverse(root.rotation) * dragged).eulerAngles);
                    switch (mode)
                    {
                        case 0:
                        case 2:
                            for (int i = 0; i < shapes.Length; i++)
                            {
                                shapes[i].rotation = local;
                            }

                            break;
                        case 1:
                            shapes[index].rotation = local;
                            break;
                    }
                }
            }

            if (editingRadius && shapeType != 2)
            {
                bool isCapsule = shapeType == 1;

                using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 position = isCapsule ? topCenter : worldPosition;
                    Quaternion rotation = isCapsule ? worldRotation : Quaternion.identity;
                    float newRadius = Handles.RadiusHandle(rotation, position, scaledRadius, true) / scale;
                    ApplyRadiusDrag(changeCheck.changed, newRadius);
                }

                // A capsule gets a second radius handle on its far cap, so it can be resized from
                // whichever end is facing the camera.
                if (isCapsule)
                {
                    using EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope();
                    float newRadius = Handles.RadiusHandle(worldRotation, bottomCenter, scaledRadius, true) / scale;
                    ApplyRadiusDrag(changeCheck.changed, newRadius);
                }
            }

            if (editingHeight && shapeType == 1)
            {
                Vector3 topAnchor = topCenter + radiusOffset;
                Vector3 bottomAnchor = bottomCenter - radiusOffset;

                using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 dragged = Handles.Slider(topAnchor, axis);
                    ApplyHeightDrag(changeCheck.changed, dragged, topAnchor);
                }

                using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    Vector3 dragged = Handles.Slider(bottomAnchor, axis * -1f);
                    ApplyHeightDrag(changeCheck.changed, dragged, bottomAnchor);
                }
            }

            foreach (ADOEditorUtility.ShapeSnapshot shape in shapes)
            {
                shape.Apply();
            }

            // ── The three lifted local functions ────────────────────────────────────────────

            // Writes a dragged radius back, keeping each capsule's caps attached.
            void ApplyRadiusDrag(bool changed, float newRadius)
            {
                if (!changed)
                {
                    return;
                }

                if (soloEdit)
                {
                    Undo.RecordObject(target, "Adjust Radius");
                }
                else
                {
                    Undo.RecordObjects(targets, "Adjust Radius");
                }

                float radiusDelta = newRadius - shapes[index].radius;

                switch (mode)
                {
                    case 2:
                        ResizeAroundRadius(shapes[index], radiusDelta, out shapes[index].radius, out shapes[index].height);
                        for (int i = 0; i < shapes.Length; i++)
                        {
                            if (i != index)
                            {
                                shapes[i].radius = shapes[index].radius;

                                // A sphere's height is its diameter; a capsule keeps its straight
                                // section and grows by the same amount at both caps.
                                if (shapes[i].shapeType == 0)
                                {
                                    shapes[i].height = shapes[i].radius * 2f;
                                }
                                else
                                {
                                    shapes[i].height += radiusDelta * 2f;
                                }
                            }
                        }

                        break;
                    case 1:
                        ResizeAroundRadius(shapes[index], radiusDelta, out shapes[index].radius, out shapes[index].height);
                        break;
                    default:
                        for (int i = 0; i < shapes.Length; i++)
                        {
                            ResizeAroundRadius(shapes[i], radiusDelta, out shapes[i].radius, out shapes[i].height);
                        }

                        break;
                }
            }

            // The radius and height a shape takes when its radius changes by a delta.
            void ResizeAroundRadius(ADOEditorUtility.ShapeSnapshot shape, float radiusDelta, out float radius, out float height)
            {
                radius = shape.radius + radiusDelta;
                height = shape.shapeType != 0
                    ? shape.height + radiusDelta * 2f
                    : radius * 2f;
            }

            // Writes a dragged cap position back as a change in height.
            void ApplyHeightDrag(bool changed, Vector3 dragged, Vector3 anchor)
            {
                if (!changed)
                {
                    return;
                }

                if (soloEdit)
                {
                    Undo.RecordObject(target, "Adjust Height");
                }
                else
                {
                    Undo.RecordObjects(targets, "Adjust Height");
                }

                // The slider only reports a point, so the sign has to come from whether the drag
                // ended closer to the shape's centre than the cap started.
                bool inward = (worldPosition - dragged).magnitude < (worldPosition - anchor).magnitude;
                float delta = (dragged - anchor).magnitude * (inward ? -1 : 1) * 2f / scale;

                switch (mode)
                {
                    case 2:
                        shapes[index].height += delta;
                        for (int i = 0; i < shapes.Length; i++)
                        {
                            shapes[i].height = shapes[index].height;
                        }

                        break;
                    case 1:
                        shapes[index].height += delta;
                        break;
                    default:
                        for (int i = 0; i < shapes.Length; i++)
                        {
                            shapes[i].height += delta;
                        }

                        break;
                }
            }
        }
    }
}
