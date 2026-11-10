// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static field m_DatabaseSerializer -> radiusHandleHash,   line 2086
//   static InitStatus                 -> DrawSphereHandle,   line 3546
//   static ConnectStatus              -> TransformHandles,   line 3572
//   static CreateStatus               -> RadiusHandle,       line 3649
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above, and TransformHandles's discarded third condition was cross-checked against the 2019 build.
//
// DECOMPILE DAMAGE: TransformHandles's export body computes a third condition and throws it away --
//     if (!bool_0) { if (readparam2) { _ = 1; } else { _ = Tools.current == Tool.Scale; } }
//     else { _ = 0; }
// -- the exact shape of the two conditions above it, which are the move and rotate gates. The 2019
// build (ConcatManager, line 3589) has the identical discarded expression, so this is not a
// decompilation slip in one snapshot: the vendor computed a scale gate and never wrote the scale
// handle. It is reproduced below as a documented no-op rather than either deleted or invented,
// because deleting it loses the evidence that the sixth and seventh parameters mean something, and
// writing a Handles.ScaleHandle call would be a guess at code that never shipped.
//
// SHARED WITH CONTROLLEREDITOR, NOT CONSOLIDATED -- REPORT-ONLY OVERLAP.
// reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs carries twins of all
// three: ChangeQueue (line 6445) is DrawSphereHandle statement for statement, the radius handle is
// at line 6211, and the "RadiusHandleHash" control-id seed is at line 2174. The SphereHandle type
// itself has already been consolidated into DreadScripts.Common.SphereHandle, so these three
// arguably belong beside it rather than duplicated per product. That is a cross-product decision
// this file does not take.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Seed for the radius handle's control IDs. A stable per-call-site hash keeps IMGUI's
        /// control numbering steady across frames.
        /// </summary>
        private static readonly int radiusHandleHash = "RadiusHandleHash".GetHashCode();

        /// <summary>
        /// Draws <paramref name="handle"/> and runs its picking and click handling for this event.
        /// </summary>
        /// <remarks>
        /// Split across two event types the way IMGUI handles need: on Layout the handle registers
        /// its distances so Unity can decide what the pointer is nearest to, and on MouseDown it
        /// acts only if it won that decision. Registering several distances per handle is what lets
        /// one control cover more than one piece of geometry.
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
        /// Draws move and rotate handles for <paramref name="transform"/>, following the editor's
        /// current tool and pivot-rotation mode.
        /// </summary>
        /// <param name="forceMove">Draw the move handle whatever tool is selected.</param>
        /// <param name="forceRotate">Draw the rotate handle whatever tool is selected.</param>
        /// <param name="forceScale">
        /// Would force the scale handle. Has no effect: the scale handle was never implemented. See
        /// the note at the top of this file.
        /// </param>
        /// <param name="suppressMove">Never draw the move handle.</param>
        /// <param name="suppressRotate">Never draw the rotate handle.</param>
        /// <param name="suppressScale">Counterpart to <paramref name="forceScale"/>; likewise inert.</param>
        /// <remarks>
        /// Writes straight back to the transform without an <see cref="Undo"/> record; the callers in
        /// the PhysBone editors open their own undo group around the whole scene-GUI pass.
        /// </remarks>
        internal static void TransformHandles(
            Transform transform,
            bool forceMove = false,
            bool forceRotate = false,
            bool forceScale = false,
            bool suppressMove = false,
            bool suppressRotate = false,
            bool suppressScale = false)
        {
            if (transform == null)
            {
                return;
            }

            bool drawMove = !suppressMove && (forceMove || Tools.current == Tool.Move);
            bool drawRotate = !suppressRotate && (forceRotate || Tools.current == Tool.Rotate);

            // The scale gate the shipped build computes and discards. Kept so the two scale
            // parameters are not silently meaningless; nothing reads it, in either build.
            bool drawScale = !suppressScale && (forceScale || Tools.current == Tool.Scale);
            _ = drawScale;

            bool globalPivot = Tools.pivotRotation == PivotRotation.Global;

            if (drawMove)
            {
                // Orienting the handle by localRotation in Local mode gives the parent's frame, not
                // the object's own -- which is what "Local" means to Unity's own move handle.
                transform.position = globalPivot
                    ? Handles.PositionHandle(transform.position, transform.rotation)
                    : Handles.PositionHandle(transform.position, transform.localRotation);
            }

            if (drawRotate)
            {
                if (globalPivot)
                {
                    transform.rotation = Handles.RotationHandle(transform.rotation, transform.position);
                }
                else
                {
                    transform.localRotation = Handles.RotationHandle(transform.localRotation, transform.position);
                }
            }
        }

        /// <summary>
        /// A four-grip radius handle: draws a wire sphere of <paramref name="radius"/> at
        /// <paramref name="center"/> and lets the user drag any of its four equatorial grips.
        /// </summary>
        /// <param name="rotation">Orients the four grip axes.</param>
        /// <param name="drawArcs">Draw the wire arcs as well as the grips.</param>
        /// <param name="handleScale">Multiplier on the grip dot size, from the tool's handle-size setting.</param>
        /// <returns>The radius after any drag; unchanged when nothing was dragged.</returns>
        /// <remarks>
        /// <para>
        /// This is a reimplementation of Unity's own internal <c>Handles.RadiusHandle</c>, which is
        /// not public. Everything past the grip loop is what Unity's version does too: the grips on
        /// the far side of the sphere are faded, and the ones that would be drawn edge-on are
        /// skipped entirely because they cannot be aimed at.
        /// </para>
        /// <para>
        /// <c>horizonAngle</c> is the angular radius of the sphere's visible horizon from the camera,
        /// or 90 degrees under an orthographic camera where there is no perspective horizon. A grip
        /// further from the viewer than that is behind the sphere and gets a fifth of the alpha. The
        /// -1000 case is the camera being inside the sphere, where the whole surface is "near" and no
        /// grip is faded.
        /// </para>
        /// <para>
        /// <see cref="GUI.changed"/> is saved and restored around each grip so that only that grip's
        /// own change is read, and any change a caller had already made is not lost.
        /// </para>
        /// </remarks>
        internal static float RadiusHandle(Quaternion rotation, Vector3 center, float radius, bool drawArcs = true, float handleScale = 1f)
        {
            float horizonAngle = 90f;

            Vector3[] axes =
            {
                rotation * Vector3.right,
                rotation * Vector3.forward,
                rotation * -Vector3.right,
                rotation * -Vector3.forward
            };

            Vector3 toCamera;
            if (Camera.current.orthographic)
            {
                toCamera = Camera.current.transform.forward;
            }
            else
            {
                toCamera = center - Matrix4x4.Inverse(Handles.matrix).MultiplyPoint(Camera.current.transform.position);

                float distanceSquared = toCamera.sqrMagnitude;
                float radiusSquared = radius * radius;
                float horizonSquared = radiusSquared * radiusSquared / distanceSquared;

                horizonAngle = ((double)(horizonSquared / radiusSquared) < 1.0)
                    ? (Mathf.Atan2(Mathf.Sqrt(radiusSquared - horizonSquared), Mathf.Sqrt(horizonSquared)) * 57.29578f)
                    : (-1000f);
            }

            Color color = Handles.color;
            for (int i = 0; i < 4; i++)
            {
                int controlID = GUIUtility.GetControlID(radiusHandleHash, FocusType.Passive);
                float angle = Vector3.Angle(axes[i], -toCamera);

                if ((!((double)angle <= 5.0) && (double)angle < 175.0) || GUIUtility.hotControl == controlID)
                {
                    float alpha = ((double)angle <= (double)horizonAngle + 5.0)
                        ? Mathf.Clamp01(color.a * 2f)
                        : Mathf.Clamp01(0.2f * color.a * 2f);

                    Color gripColor = new Color(color.r, color.g, color.b, alpha);
                    Handles.color = (QualitySettings.activeColorSpace == ColorSpace.Linear) ? gripColor.linear : gripColor;

                    Vector3 gripPosition = center + radius * axes[i];

                    bool callerChanged = GUI.changed;
                    GUI.changed = false;

                    Vector3 dragged = Handles.Slider(controlID, gripPosition, axes[i], HandleUtility.GetHandleSize(gripPosition) * 0.05f * handleScale, Handles.DotHandleCap, 0f);
                    if (GUI.changed)
                    {
                        radius = Vector3.Distance(dragged, center);
                    }

                    GUI.changed |= callerChanged;
                    Handles.color = color;
                }

                if (drawArcs)
                {
                    Handles.DrawWireArc(center, axes[i], axes[(i + 1) % 4], 360f, radius);
                }
            }

            return radius;
        }
    }
}
