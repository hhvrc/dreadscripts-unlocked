// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static field iteratorProperty   -> radiusHandleHash,             line 2174
//   static field regProcessor       -> arrowMesh,                    line 2196
//   static field testsProcessor     -> arrowMaterial,                line 2198
//   static field _PropertyProcessor -> colorPropertyId,              line 2200
//   static field processProperty    -> rotationHandleStartRotation,  line 2170
//   static field m_ProducerProperty -> rotationHandleDragging,       line 2172
//   static ForgotQueue -> DrawArrow(from, to, up, ...),      line 6329
//   static StopQueue   -> DrawArrow(position, rotation, ...), line 6356
//   static CheckQueue  -> DrawArrow(matrix, ...),            line 6361
//   static PrepareQueue -> CreateArrowMesh,                  line 6385
//   static AssetQueue   -> CreateArrowMaterial,              line 6430
//   static UpdateQueue  -> ConfigureArrowMaterial,           line 6437
//   static ChangeQueue  -> DrawSphereHandle,                 line 6445
//   static RunQueue     -> TransformHandles,                 line 6042
//   static CloneQueue   -> RotationHandles,                  line 6087
//   static ReflectQueue -> AxisRotationHandle,               line 6140
//   static DeleteQueue  -> RotationDisc,                     line 6151
//   static NewQueue     -> RadiusHandle,                     line 6175
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// SHARED WITH ADOVERHAUL, NOT CONSOLIDATED. ADOEditorUtility.SceneHandles.cs ports the same
// DrawSphereHandle, TransformHandles and RadiusHandle from ADOverhaul's own copy, and says there why
// the pair is left duplicated rather than lifted into Common. Two differences are worth recording,
// because they show the two copies are not textually identical:
//   * ControllerEditor's TransformHandles actually draws the scale handle. ADOverhaul's computes the
//     same scale gate and then discards it -- documented as a vendor omission in that file.
//   * ControllerEditor's rotation half is far richer: it has a per-axis disc mode and a global-space
//     mode with its own drag state, neither of which ADOverhaul has.
// SphereHandle itself is already shared, in DreadScripts.Common; only the drawing entry point is
// duplicated here. So is the scene label -- SphereHandle.DrawSceneLabel is the shared port of
// CreateQueue (line 6157), which is therefore deliberately absent below.
//
// GLOBAL-SPACE ROTATION NEEDS DRAG STATE. Handles.RotationHandle always returns an absolute
// rotation, so driving a transform from an identity-seeded handle would snap it to the handle's
// own frame on the first frame of a drag. RotationHandles instead records the transform's rotation
// when the drag begins (detected by hotControl changing) and applies the handle's delta to it. The
// two static fields exist only for that, and only one drag can be in progress at a time, which is
// why they can be static.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Seed for <see cref="RadiusHandle"/>'s control IDs. A stable per-call-site hash keeps
        /// IMGUI's control numbering steady across frames.
        /// </summary>
        private static readonly int radiusHandleHash = "RadiusHandleHash".GetHashCode();

        /// <summary>Shared arrow mesh, built on first use. Never saved.</summary>
        private static Mesh arrowMesh;

        /// <summary>Shared arrow material, built on first use. Never saved.</summary>
        private static Material arrowMaterial;

        private static readonly int colorPropertyId = Shader.PropertyToID("_Color");

        /// <summary>The rotation the transform had when the current rotation drag started.</summary>
        private static Quaternion rotationHandleStartRotation;

        /// <summary>Whether a global-space rotation drag is in progress.</summary>
        private static bool rotationHandleDragging;

        /// <summary>
        /// Draws the tool handles <paramref name="settings"/> permits for
        /// <paramref name="transform"/>, honouring the editor's current tool and pivot-rotation
        /// mode, and writing changes back through Undo.
        /// </summary>
        /// <remarks>
        /// Unity's Transform tool counts as all three of move, rotate and scale; otherwise the
        /// active tool enables exactly one, and each control's own setting can force it on or off
        /// regardless. Scale is clamped to a small positive minimum so a handle dragged to zero
        /// does not leave a transform that can never be scaled back up.
        /// </remarks>
        internal static void TransformHandles(Transform transform, TransformControlSettings settings)
        {
            if (transform == null)
            {
                return;
            }

            bool allTools = Tools.current == Tool.Transform;
            bool moveTool = allTools || Tools.current == Tool.Move;
            bool rotateTool = allTools || (!moveTool && Tools.current == Tool.Rotate);
            bool scaleTool = allTools || (!moveTool && !rotateTool && Tools.current == Tool.Scale);

            PivotRotation pivotRotation = Tools.pivotRotation;

            if (settings.positionControl.IsEnabled(moveTool))
            {
                Vector3 position = transform.position;
                bool global = settings.positionControl.ResolvePivotRotation(pivotRotation) == PivotRotation.Global;

                EditorGUI.BeginChangeCheck();
                position = global
                    ? Handles.PositionHandle(position, Quaternion.identity)
                    : Handles.PositionHandle(position, transform.localRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(transform, "Custom Tool Control");
                    transform.position = position;
                }
            }

            if (settings.rotationControl.IsEnabled(rotateTool))
            {
                bool global = settings.rotationControl.ResolvePivotRotation(pivotRotation) == PivotRotation.Global;
                RotationHandles(transform, settings.rotationControl.axis, global);
            }

            if (settings.scaleControl.IsEnabled(scaleTool))
            {
                Vector3 localScale = transform.localScale;
                Vector3 position = transform.position;

                EditorGUI.BeginChangeCheck();
                localScale = Handles.ScaleHandle(localScale, position, transform.rotation,
                    HandleUtility.GetHandleSize(position));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(transform, "Custom Tool Control");
                    transform.localScale = new Vector3(
                        Mathf.Max(localScale.x, 0.0001f),
                        Mathf.Max(localScale.y, 0.0001f),
                        Mathf.Max(localScale.z, 0.0001f));
                }
            }
        }

        /// <summary>
        /// Draws rotation handles for <paramref name="transform"/>.
        /// </summary>
        /// <param name="axes">
        /// Which axes may be rotated. All three gives Unity's full rotation ball; anything less
        /// gives one disc per axis.
        /// </param>
        /// <param name="global">
        /// Rotate in world space rather than the transform's own. In per-axis mode this picks
        /// between the world axes and the transform's; in ball mode it switches to the delta-drag
        /// path described at the top of this file.
        /// </param>
        internal static void RotationHandles(Transform transform, Axis axes = Axis.X | Axis.Y | Axis.Z,
            bool global = true)
        {
            if (axes != (Axis.X | Axis.Y | Axis.Z))
            {
                if (axes.HasFlag(Axis.X))
                {
                    AxisRotationHandle(transform, global ? Vector3.right : transform.right);
                }

                if (axes.HasFlag(Axis.Y))
                {
                    AxisRotationHandle(transform, global ? Vector3.up : transform.up);
                }

                if (axes.HasFlag(Axis.Z))
                {
                    AxisRotationHandle(transform, global ? Vector3.forward : transform.forward);
                }

                return;
            }

            if (!global)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion rotation = Handles.RotationHandle(transform.rotation, transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(transform, "Custom Tool Control");
                    transform.rotation = rotation;
                }

                return;
            }

            int hotControlBefore = GUIUtility.hotControl;
            bool dragging = Event.current.type == EventType.MouseDrag;

            EditorGUI.BeginChangeCheck();
            Quaternion delta = Handles.RotationHandle(Quaternion.identity, transform.position);

            if (hotControlBefore != GUIUtility.hotControl)
            {
                // The handle grabbed or released a control this frame: that is the start of a drag.
                rotationHandleStartRotation = transform.rotation;
                rotationHandleDragging = true;
            }
            else if (hotControlBefore == 0)
            {
                rotationHandleDragging = false;
            }

            if (EditorGUI.EndChangeCheck() && rotationHandleDragging && dragging)
            {
                Undo.RecordObject(transform, "Custom Tool Control");
                transform.rotation = delta * rotationHandleStartRotation;
            }
        }

        /// <summary>
        /// Draws a single rotation disc about <paramref name="axis"/> and writes the result back
        /// through Undo.
        /// </summary>
        internal static void AxisRotationHandle(Transform transform, Vector3 axis)
        {
            EditorGUI.BeginChangeCheck();
            Quaternion rotation = RotationDisc(transform.position, transform.rotation, axis);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(transform, "Custom Tool Control");
                transform.rotation = rotation;
            }
        }

        /// <summary>
        /// A rotation disc at <paramref name="position"/> about <paramref name="axis"/>, sized to
        /// stay constant on screen. Sets Handles.color and does not restore it.
        /// </summary>
        internal static Quaternion RotationDisc(Vector3 position, Quaternion rotation, Vector3 axis)
        {
            Handles.color = validColor;
            return Handles.Disc(rotation, position, axis, HandleUtility.GetHandleSize(position), false, 0f);
        }

        /// <summary>
        /// A radius handle: four dot handles on the equator of a sphere, any of which resizes it.
        /// </summary>
        /// <param name="rotation">The frame the four handles are placed in.</param>
        /// <param name="drawArcs">Draw the wire circles joining the handles.</param>
        /// <param name="handleScale">Multiplier on the dot handles' screen size.</param>
        /// <returns>The radius after any drag; <paramref name="radius"/> unchanged otherwise.</returns>
        /// <remarks>
        /// A reimplementation of Unity's internal <c>Handles.RadiusHandle</c>. Handles pointing
        /// nearly at or away from the camera are skipped, since they would be a dot that cannot be
        /// dragged meaningfully, and those past the sphere's silhouette are drawn faded. The
        /// silhouette angle comes out of the perspective projection; an orthographic camera has no
        /// silhouette, so the angle is left at 90 degrees and only the near/far cull applies.
        /// </remarks>
        internal static float RadiusHandle(Quaternion rotation, Vector3 center, float radius, bool drawArcs = true,
            float handleScale = 1f)
        {
            float silhouetteAngle = 90f;
            Vector3[] axes =
            {
                rotation * Vector3.right,
                rotation * Vector3.forward,
                rotation * -Vector3.right,
                rotation * -Vector3.forward
            };

            Vector3 toCamera;
            if (!Camera.current.orthographic)
            {
                toCamera = center - Matrix4x4.Inverse(Handles.matrix)
                    .MultiplyPoint(Camera.current.transform.position);

                float distanceSqr = toCamera.sqrMagnitude;
                float radiusSqr = radius * radius;
                float t = radiusSqr * radiusSqr / distanceSqr;

                // Inside the sphere: nothing is on the silhouette, so nothing is faded.
                silhouetteAngle = (t / radiusSqr >= 1.0)
                    ? -1000f
                    : Mathf.Atan2(Mathf.Sqrt(radiusSqr - t), Mathf.Sqrt(t)) * Mathf.Rad2Deg;
            }
            else
            {
                toCamera = Camera.current.transform.forward;
            }

            Color color = Handles.color;
            for (int i = 0; i < 4; i++)
            {
                int controlId = GUIUtility.GetControlID(radiusHandleHash, FocusType.Passive);
                float angle = Vector3.Angle(axes[i], -toCamera);

                if ((angle > 5.0 && angle < 175.0) || GUIUtility.hotControl == controlId)
                {
                    float alpha = (angle <= silhouetteAngle + 5.0)
                        ? Mathf.Clamp01(color.a * 2f)
                        : Mathf.Clamp01(0.2f * color.a * 2f);

                    Color faded = new Color(color.r, color.g, color.b, alpha);
                    Handles.color = QualitySettings.activeColorSpace == ColorSpace.Linear ? faded.linear : faded;

                    Vector3 handlePosition = center + radius * axes[i];

                    // GUI.changed has to be isolated: the caller's own change flag must survive,
                    // but this handle's own change has to be readable on its own.
                    bool changedBefore = GUI.changed;
                    GUI.changed = false;
                    Vector3 dragged = Handles.Slider(controlId, handlePosition, axes[i],
                        HandleUtility.GetHandleSize(handlePosition) * 0.05f * handleScale, Handles.DotHandleCap, 0f);
                    if (GUI.changed)
                    {
                        radius = Vector3.Distance(dragged, center);
                    }

                    GUI.changed |= changedBefore;
                    Handles.color = color;
                }

                if (drawArcs)
                {
                    Handles.DrawWireArc(center, axes[i], axes[(i + 1) % 4], 360f, radius);
                }
            }

            return radius;
        }

        /// <summary>
        /// Draws <paramref name="handle"/> and turns clicks on it into its onClick callback.
        /// </summary>
        /// <remarks>
        /// The Layout pass is what makes the handle clickable at all: HandleUtility.AddControl
        /// registers a distance per sample point, and the control nearest the mouse wins. Skipping
        /// it would draw a handle nothing can hit.
        /// </remarks>
        internal static void DrawSphereHandle(SphereHandle handle)
        {
            Event current = Event.current;
            handle.onDraw?.Invoke(handle);

            int controlId = handle.controlId;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlId && current.button == 0)
                    {
                        handle.onClick();
                        current.Use();
                    }

                    break;

                case EventType.Layout:
                    foreach (float distance in handle.GetDistances())
                    {
                        HandleUtility.AddControl(controlId, distance);
                    }

                    break;
            }
        }

        /// <summary>
        /// Draws an arrow from <paramref name="from"/> to <paramref name="to"/>, scaled to the
        /// distance between them and rolled so its flat faces align with <paramref name="up"/>.
        /// </summary>
        /// <param name="hotControlId">
        /// If this control is being dragged the arrow is drawn yellow. -1 to disable.
        /// </param>
        /// <param name="color">Defaults to the current Handles.color.</param>
        private static void DrawArrow(Vector3 from, Vector3 to, Vector3 up, int hotControlId = -1,
            Color? color = null)
        {
            float length = Vector3.Distance(from, to);
            Vector3 direction = (to - from).normalized;
            DrawArrow(Matrix4x4.TRS(from, Quaternion.LookRotation(direction, up), Vector3.one * length),
                hotControlId, color);
        }

        /// <summary>
        /// Draws an arrow at <paramref name="position"/> pointing along
        /// <paramref name="rotation"/>, <paramref name="size"/> units long.
        /// </summary>
        private static void DrawArrow(Vector3 position, Quaternion rotation, float size, int hotControlId = -1,
            Color? color = null)
        {
            DrawArrow(Matrix4x4.TRS(position, rotation, Vector3.one * size), hotControlId, color);
        }

        /// <summary>
        /// Draws the arrow mesh through <paramref name="matrix"/>, immediately -- so this is only
        /// valid from a scene-view Repaint.
        /// </summary>
        private static void DrawArrow(Matrix4x4 matrix, int hotControlId = -1, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = Handles.color;
            }

            if (hotControlId != -1 && GUIUtility.hotControl == hotControlId)
            {
                color = Color.yellow;
            }

            if (arrowMesh == null)
            {
                arrowMesh = CreateArrowMesh();
            }

            if (arrowMaterial == null)
            {
                arrowMaterial = CreateArrowMaterial();
            }

            // Re-applied every draw, not just on creation: entering play mode or reloading shaders
            // can reset the material's state while the instance itself survives.
            ConfigureArrowMaterial(arrowMaterial);

            arrowMaterial.SetColor(colorPropertyId, color.Value);
            arrowMaterial.SetPass(0);
            Graphics.DrawMeshNow(arrowMesh, matrix);
        }

        /// <summary>
        /// The arrow mesh: a unit-length four-sided pyramid, tip at +Z, with an inverted base cone
        /// closing it at the origin.
        /// </summary>
        /// <remarks>
        /// Built with unshared vertices -- twenty-four of them for eight triangles -- so
        /// RecalculateNormals gives each face a flat normal instead of averaging them into a
        /// rounded look. Uploaded as no-longer-readable, since nothing reads it back.
        /// </remarks>
        private static Mesh CreateArrowMesh()
        {
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();

            Vector3[] vertices =
            {
                new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.1f, -0.1f, 0.1f), Vector3.zero,
                new Vector3(0.1f, -0.1f, 0.1f), new Vector3(-0.1f, -0.1f, 0.1f), Vector3.zero,
                new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(-0.1f, 0.1f, 0.1f), Vector3.zero,
                new Vector3(-0.1f, 0.1f, 0.1f), new Vector3(0.1f, 0.1f, 0.1f), Vector3.zero,
                new Vector3(0.1f, -0.1f, 0.1f), new Vector3(0.1f, 0.1f, 0.1f), Vector3.forward,
                new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(0.1f, -0.1f, 0.1f), Vector3.forward,
                new Vector3(-0.1f, 0.1f, 0.1f), new Vector3(-0.1f, -0.1f, 0.1f), Vector3.forward,
                new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-0.1f, 0.1f, 0.1f), Vector3.forward
            };

            int[] triangles = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                triangles[i] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        /// <summary>
        /// The arrow material. UI/Unlit/Text is used because it is a plain vertex-coloured unlit
        /// shader guaranteed to be present in every project.
        /// </summary>
        private static Material CreateArrowMaterial()
        {
            Material material = new Material(Shader.Find("UI/Unlit/Text"));
            ConfigureArrowMaterial(material);
            return material;
        }

        /// <summary>
        /// Sets the arrow material to draw both faces, write no depth, and always pass the depth
        /// test -- so the arrow is visible through geometry, which is what a gizmo wants.
        /// </summary>
        /// <remarks>
        /// The literals are Unity's own enum values: _Cull 2 is CullMode.Back, _ZWrite 0 is off, and
        /// _ZTest 8 is CompareFunction.Always. Note the culling value is Back rather than Off, so
        /// the arrow's inward-facing base cone is not drawn from behind.
        /// </remarks>
        private static void ConfigureArrowMaterial(Material material)
        {
            material.hideFlags = HideFlags.DontSave;
            material.SetInt("_Cull", 2);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_ZTest", 8);
        }
    }
}
