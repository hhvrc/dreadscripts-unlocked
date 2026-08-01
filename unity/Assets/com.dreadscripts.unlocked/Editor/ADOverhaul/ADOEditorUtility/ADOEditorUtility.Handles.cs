// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   ReflectStatus         -> DrawBone(Vector3, Vector3, Vector3, ...),   line 3430
//   ResolveStatus         -> DrawBone(Vector3, Quaternion, float, ...),  line 3457
//   ResetStatus           -> DrawBone(Matrix4x4, ...),                   line 3462
//   GetStatus             -> CreateBoneMesh,                             line 3486
//   FlushStatus           -> CreateBoneMaterial,                         line 3531
//   ExcludeStatus         -> ConfigureBoneMaterial,                      line 3538
//   InitStatus            -> DrawSphereHandle,                           line 3546
//   ConnectStatus         -> DrawTransformHandles,                       line 3572
//   FindStatus            -> DrawSceneLabel,                             line 3616
//   CreateStatus          -> RadiusHandle,                               line 3649
//   m_InstanceSerializer  -> boneMesh,                                   line 2080
//   m_TaskSerializer      -> boneMaterial,                               line 2082
//   customerSerializer    -> colorPropertyId,                            line 2084
//   m_DatabaseSerializer  -> radiusHandleHash,                           line 2086
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// Already ported elsewhere, so deliberately NOT duplicated here:
//   AddStatus      (line 3634) -> DreadScripts.Common.SceneViewExtensions.GetSceneViewRect
//   ValidateStatus (line 3639) -> DreadScripts.Common.SceneViewExtensions.SubtractSceneViewChrome
// Those two are the scene-view rect helpers; ControllerEditor ships the same pair
// (SortQueue / RegisterQueue) and the shared copy in Editor/Common covers both products.
// The `collectionSerializer` static they read (line 2054, "is this Unity 2022") is likewise
// already covered by ControllerEditor.EditorUtils.isUnity2022 and is not redeclared here.
//
// DrawSceneLabel is also present, privately, in Editor/Common/SphereHandle.cs, which noted the
// duplication and asked to be pointed at the shared helper once this region landed. That edit
// belongs to SphereHandle.cs, not to this file; the two bodies are identical.
//
// 2019 vs 2022 (2019 lines 3447-3716, names ValidateManager / RestartManager / ViewManager /
// SearchManager / QueryManager / OrderManager / EnableManager / ConcatManager / LogoutManager /
// PublishManager): the bodies are token-for-token the same, including the mesh vertex table, the
// material state, the dead scale branch in DrawTransformHandles and the whole of RadiusHandle.
// The only differences are ILSpy rendering the same conditionals with the comparison inverted
// (`!(x < 1.0) ? b : a` in 2022 vs `!(x >= 1.0) ? a : b` in 2019). No behavioural divergence,
// which is worth stating explicitly for this region: the scene-view chrome measurements that do
// differ between the two Unity versions live in the already-ported SceneViewExtensions, not here,
// and nothing else in this family is version-sensitive.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The octahedral bone shape drawn by <see cref="DrawBone(Matrix4x4, int, Color?)"/>, built
        /// on first use and reused for every draw.
        /// </summary>
        private static Mesh boneMesh;

        /// <summary>Material the bone mesh is drawn with; see <see cref="CreateBoneMaterial"/>.</summary>
        private static Material boneMaterial;

        private static readonly int colorPropertyId = Shader.PropertyToID("_Color");

        /// <summary>
        /// Seed for the four control ids <see cref="RadiusHandle"/> allocates.
        /// </summary>
        /// <remarks>
        /// Unity's own internal radius handle hashes the same string, so using it here keeps the
        /// control ids of this reimplementation stable across layout and repaint in the same way.
        /// </remarks>
        private static readonly int radiusHandleHash = "RadiusHandleHash".GetHashCode();

        /// <summary>
        /// Draws the bone shape stretched from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="up">Reference up axis; only the roll of the shape about the bone depends on it.</param>
        /// <param name="controlId">
        /// When not -1 and currently the hot control, the bone is forced to yellow to show it is
        /// being dragged. Pass -1 for a purely decorative bone.
        /// </param>
        /// <param name="color">Defaults to the ambient <see cref="Handles.color"/>.</param>
        /// <remarks>
        /// The mesh is a unit-length bone along +Z, so the uniform scale is the distance between the
        /// two points: length, width and depth all grow with the bone, which is what makes a long
        /// bone read as a long spike rather than a stretched sliver.
        /// <para>
        /// No call site for any of the three <c>DrawBone</c> overloads survives in either shipped
        /// assembly -- the scene view draws bones through <see cref="SphereHandle"/> and
        /// <see cref="RadiusHandle"/> instead. They are ported because they are product code that
        /// still compiles and behaves, not obfuscator scaffolding.
        /// </para>
        /// </remarks>
        private static void DrawBone(Vector3 from, Vector3 to, Vector3 up, int controlId = -1, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = Handles.color;
            }

            if (controlId != -1 && GUIUtility.hotControl == controlId)
            {
                color = Color.yellow;
            }

            if (boneMesh == null)
            {
                boneMesh = CreateBoneMesh();
            }

            if (boneMaterial == null)
            {
                boneMaterial = CreateBoneMaterial();
            }

            // Re-applied on every draw even though CreateBoneMaterial already set it. Transcribed as
            // shipped: another editor can have poked at the shared material in between.
            ConfigureBoneMaterial(boneMaterial);

            float length = Vector3.Distance(from, to);
            Vector3 direction = (to - from).normalized;
            Matrix4x4 matrix = Matrix4x4.TRS(from, Quaternion.LookRotation(direction, up), new Vector3(length, length, length));

            boneMaterial.SetColor(colorPropertyId, color.Value);
            boneMaterial.SetPass(0);
            Graphics.DrawMeshNow(boneMesh, matrix);
        }

        /// <summary>
        /// Draws the bone shape at <paramref name="position"/> pointing along the +Z axis of
        /// <paramref name="rotation"/>, <paramref name="length"/> units long.
        /// </summary>
        private static void DrawBone(Vector3 position, Quaternion rotation, float length, int controlId = -1, Color? color = null)
        {
            DrawBone(Matrix4x4.TRS(position, rotation, new Vector3(length, length, length)), controlId, color);
        }

        /// <summary>
        /// Draws the bone shape under an arbitrary transform, for callers that already hold a matrix.
        /// </summary>
        private static void DrawBone(Matrix4x4 matrix, int controlId = -1, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = Handles.color;
            }

            if (controlId != -1 && GUIUtility.hotControl == controlId)
            {
                color = Color.yellow;
            }

            if (boneMesh == null)
            {
                boneMesh = CreateBoneMesh();
            }

            if (boneMaterial == null)
            {
                boneMaterial = CreateBoneMaterial();
            }

            ConfigureBoneMaterial(boneMaterial);

            boneMaterial.SetColor(colorPropertyId, color.Value);
            boneMaterial.SetPass(0);
            Graphics.DrawMeshNow(boneMesh, matrix);
        }

        /// <summary>
        /// Builds the octahedral bone mesh: two four-sided pyramids sharing a square waist, one
        /// tapering back to the origin and one tapering forward to (0, 0, 1).
        /// </summary>
        /// <remarks>
        /// The waist is a 0.2-unit square at z = 0.1, so the near pyramid is short and the far one
        /// long -- the same silhouette Unity's own bone gizmo uses, which is what makes the direction
        /// of a bone readable at a glance.
        /// <para>
        /// Vertices are not shared between triangles: each of the eight faces gets its own three, so
        /// <see cref="Mesh.RecalculateNormals"/> produces hard facets instead of a smoothly shaded
        /// blob. That is also why the triangle list is simply 0..23.
        /// </para>
        /// </remarks>
        private static Mesh CreateBoneMesh()
        {
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();

            Vector3[] vertices = new Vector3[24]
            {
                new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(0.1f, -0.1f, 0.1f),
                Vector3.zero,
                new Vector3(0.1f, -0.1f, 0.1f),
                new Vector3(-0.1f, -0.1f, 0.1f),
                Vector3.zero,
                new Vector3(-0.1f, -0.1f, 0.1f),
                new Vector3(-0.1f, 0.1f, 0.1f),
                Vector3.zero,
                new Vector3(-0.1f, 0.1f, 0.1f),
                new Vector3(0.1f, 0.1f, 0.1f),
                Vector3.zero,
                new Vector3(0.1f, -0.1f, 0.1f),
                new Vector3(0.1f, 0.1f, 0.1f),
                Vector3.forward,
                new Vector3(-0.1f, -0.1f, 0.1f),
                new Vector3(0.1f, -0.1f, 0.1f),
                Vector3.forward,
                new Vector3(-0.1f, 0.1f, 0.1f),
                new Vector3(-0.1f, -0.1f, 0.1f),
                Vector3.forward,
                new Vector3(0.1f, 0.1f, 0.1f),
                new Vector3(-0.1f, 0.1f, 0.1f),
                Vector3.forward
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

            // The mesh is never edited again, so the CPU-side copy is dropped.
            mesh.UploadMeshData(markNoLongerReadable: true);

            mesh.hideFlags = HideFlags.DontSave;
            return mesh;
        }

        /// <summary>
        /// Creates the material the bone mesh is drawn with.
        /// </summary>
        /// <remarks>
        /// "UI/Unlit/Text" is a built-in shader that is always present in a player-free editor and
        /// exposes a plain <c>_Color</c> with no lighting, which is all a gizmo needs.
        /// </remarks>
        private static Material CreateBoneMaterial()
        {
            Material material = new Material(Shader.Find("UI/Unlit/Text"));
            ConfigureBoneMaterial(material);
            return material;
        }

        /// <summary>
        /// Puts <paramref name="material"/> into the state a gizmo wants: double sided, no depth
        /// writes, and depth test always.
        /// </summary>
        /// <remarks>
        /// The literals are the <c>UnityEngine.Rendering</c> enum values the shader's state
        /// properties take: cull 2 is <see cref="UnityEngine.Rendering.CullMode.Back"/>, so back
        /// faces are hidden while the shape still reads from inside; ztest 8 is
        /// <see cref="UnityEngine.Rendering.CompareFunction.Always"/>, so the bone draws through the
        /// mesh it sits inside rather than being buried by it.
        /// </remarks>
        private static void ConfigureBoneMaterial(Material material)
        {
            material.hideFlags = HideFlags.DontSave;
            material.SetInt("_Cull", 2);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_ZTest", 8);
        }

        /// <summary>
        /// Draws <paramref name="handle"/> and drives its picking and click for the current event.
        /// </summary>
        /// <remarks>
        /// <see cref="SphereHandle"/> deliberately owns only its appearance and its distance
        /// function; the event plumbing lives here so the struct stays a description. On Layout every
        /// distance the handle reports is registered against its single control id, which is what
        /// lets one handle stand for something with extent and still be picked as a unit. On
        /// MouseDown the click runs only if Unity has already decided this control is the nearest,
        /// so overlapping handles do not all fire.
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
                    float[] distances = handle.GetDistances();
                    foreach (float distance in distances)
                    {
                        HandleUtility.AddControl(controlId, distance);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws move and rotate handles for <paramref name="transform"/> and writes the result
        /// straight back to it.
        /// </summary>
        /// <param name="forceMove">Show the move handle even when the active tool is not Move.</param>
        /// <param name="forceRotate">Show the rotate handle even when the active tool is not Rotate.</param>
        /// <param name="forceScale">Inert; see the remarks.</param>
        /// <param name="disableMove">Suppress the move handle even when the active tool is Move.</param>
        /// <param name="disableRotate">Suppress the rotate handle even when the active tool is Rotate.</param>
        /// <param name="disableScale">Inert; see the remarks.</param>
        /// <remarks>
        /// <para>
        /// SHIPPED BUG, preserved: the scale handle was never wired up. Both builds compute the
        /// force/disable decision for scale exactly as they do for move and rotate -- ILSpy renders
        /// it as a discarded `_ =` expression -- and then no scale handle is ever drawn, so
        /// <paramref name="forceScale"/> and <paramref name="disableScale"/> have no effect at all.
        /// The parameters are kept so call sites transcribe unchanged (ADOverhaul.cs line 6118
        /// passes disableScale: true, expecting suppression it was already getting for free).
        /// The dead decision itself is not reproduced: reading <see cref="Tools.current"/> and
        /// throwing the answer away is unobservable.
        /// </para>
        /// <para>
        /// The pivot-rotation check is why each handle appears twice: in Global mode the handle is
        /// oriented by, and writes back, the world rotation; in Local mode it uses the local one.
        /// Note the asymmetry the source has -- the position handle only ever writes
        /// <see cref="Transform.position"/> in both branches, while the rotation handle writes
        /// <c>localRotation</c> in the local branch and <c>rotation</c> in the global one.
        /// </para>
        /// </remarks>
        internal static void DrawTransformHandles(Transform transform, bool forceMove = false, bool forceRotate = false, bool forceScale = false, bool disableMove = false, bool disableRotate = false, bool disableScale = false)
        {
            if (transform == null)
            {
                return;
            }

            bool showMove = !disableMove && (forceMove || Tools.current == Tool.Move);
            bool showRotate = !disableRotate && (forceRotate || Tools.current == Tool.Rotate);
            bool isGlobal = Tools.pivotRotation == PivotRotation.Global;

            if (showMove)
            {
                if (!isGlobal)
                {
                    transform.position = Handles.PositionHandle(transform.position, transform.localRotation);
                }
                else
                {
                    transform.position = Handles.PositionHandle(transform.position, transform.rotation);
                }
            }

            if (showRotate)
            {
                if (!isGlobal)
                {
                    transform.localRotation = Handles.RotationHandle(transform.localRotation, transform.position);
                }
                else
                {
                    transform.rotation = Handles.RotationHandle(transform.rotation, transform.position);
                }
            }
        }

        /// <summary>
        /// Draws <paramref name="text"/> in the scene view at the screen position of
        /// <paramref name="worldPosition"/>, horizontally centred and lifted clear of the handle.
        /// </summary>
        /// <param name="offset">
        /// World-space size of the thing being labelled; the label is raised by an amount that
        /// shrinks with distance so it clears a handle of that size at any zoom.
        /// </param>
        internal static void DrawSceneLabel(string text, Vector3 worldPosition, float offset = 0f, GUIStyle style = null)
        {
            if (style == null)
            {
                style = EditorStyles.boldLabel;
            }

            GUIContent content = new GUIContent(text);
            float width = style.CalcSize(content).x;
            Vector3 guiPoint = HandleUtility.WorldToGUIPointWithDepth(worldPosition);

            // Behind the camera: WorldToGUIPointWithDepth still yields a point, but drawing it would
            // put the label on the wrong side of the view.
            if (guiPoint.z > 0f)
            {
                // The second term reduces to a constant 20 pixels; it is transcribed as shipped
                // rather than folded, since only the first term is depth-dependent.
                Vector3 labelPosition = guiPoint - new Vector3(width * 0.5f, offset * 500f / guiPoint.z + guiPoint.z / (guiPoint.z * 0.05f));
                Handles.BeginGUI();
                GUI.Label(new Rect(labelPosition, new Vector2(width, 20f)), content, style);
                Handles.EndGUI();
            }
        }

        /// <summary>
        /// An interactive radius handle: four dot grips on the equator of a sphere of
        /// <paramref name="radius"/> around <paramref name="center"/>, optionally ringed by wire
        /// arcs. Returns the radius after any drag.
        /// </summary>
        /// <param name="rotation">Orients the four grip axes; the grips sit on the local X and Z axes.</param>
        /// <param name="drawArcs">Draw the three great circles as well as the grips.</param>
        /// <param name="handleSizeMultiplier">Scales the on-screen size of the dot grips only, not the radius.</param>
        /// <remarks>
        /// <para>
        /// This is a transcription of Unity's own internal <c>Handles.RadiusHandle</c>, which is not
        /// public, reproduced so the tool can pass its own size multiplier and suppress the arcs.
        /// </para>
        /// <para>
        /// <c>horizonAngle</c> is the angular radius of the sphere's horizon as seen from the camera:
        /// grips further round than that are behind the sphere, and are drawn at a fifth of the alpha
        /// so the shape still reads without the back grips competing with the front ones. In an
        /// orthographic view there is no horizon, so it stays at its initial 90 degrees and every
        /// grip is drawn at full alpha. The <c>-1000f</c> case is the camera inside the sphere, where
        /// no angle is behind the horizon and the comparison must therefore always fail.
        /// </para>
        /// <para>
        /// Grips within 5 degrees of pointing straight at or away from the camera are skipped
        /// entirely -- edge-on they would be a dot on top of the centre, impossible to aim at --
        /// unless one is mid-drag, which must keep receiving events.
        /// </para>
        /// <para>
        /// <see cref="GUI.changed"/> is saved, cleared and OR-ed back so this handle can tell whether
        /// <em>its own</em> slider moved without swallowing a change flag an outer control had
        /// already raised.
        /// </para>
        /// <para>
        /// Note that <paramref name="radius"/> is updated inside the loop, so a drag on the first
        /// grip is already reflected in the arcs drawn for the remaining ones within the same frame.
        /// That is the shipped behaviour, and it is what makes the ring follow the pointer without a
        /// frame of lag.
        /// </para>
        /// </remarks>
        internal static float RadiusHandle(Quaternion rotation, Vector3 center, float radius, bool drawArcs = true, float handleSizeMultiplier = 1f)
        {
            float horizonAngle = 90f;

            Vector3[] axes = new Vector3[4]
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

                float sqrDistance = toCamera.sqrMagnitude;
                float sqrRadius = radius * radius;
                float sqrHorizonRadius = sqrRadius * sqrRadius / sqrDistance;

                horizonAngle = ((sqrHorizonRadius / sqrRadius < 1f)
                    ? (Mathf.Atan2(Mathf.Sqrt(sqrRadius - sqrHorizonRadius), Mathf.Sqrt(sqrHorizonRadius)) * Mathf.Rad2Deg)
                    : (-1000f));
            }

            Color baseColor = Handles.color;
            for (int i = 0; i < 4; i++)
            {
                int controlId = GUIUtility.GetControlID(radiusHandleHash, FocusType.Passive);
                float angleToCamera = Vector3.Angle(axes[i], -toCamera);

                if ((angleToCamera > 5f && angleToCamera < 175f) || GUIUtility.hotControl == controlId)
                {
                    float alpha = ((angleToCamera <= horizonAngle + 5f)
                        ? Mathf.Clamp01(baseColor.a * 2f)
                        : Mathf.Clamp01(0.2f * baseColor.a * 2f));

                    Color gripColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                    // Handles.color is consumed as-is by the GL state, so in linear rendering the
                    // gamma-space colour has to be converted by hand.
                    Handles.color = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? gripColor.linear : gripColor);

                    Vector3 gripPosition = center + radius * axes[i];

                    bool changedBefore = GUI.changed;
                    GUI.changed = false;
                    Vector3 dragged = Handles.Slider(controlId, gripPosition, axes[i], HandleUtility.GetHandleSize(gripPosition) * 0.05f * handleSizeMultiplier, Handles.DotHandleCap, 0f);
                    if (GUI.changed)
                    {
                        radius = Vector3.Distance(dragged, center);
                    }
                    GUI.changed |= changedBefore;

                    Handles.color = baseColor;
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
