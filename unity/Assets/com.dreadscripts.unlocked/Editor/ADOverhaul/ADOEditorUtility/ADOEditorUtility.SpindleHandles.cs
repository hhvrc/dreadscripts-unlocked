// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static field customerSerializer   -> colorPropertyId,           line 2084
//   static field m_InstanceSerializer -> spindleMesh,               line 2080
//   static field m_TaskSerializer     -> spindleMaterial,           line 2082
//   static ReflectStatus -> DrawSpindle(Vector3, Vector3, Vector3, int, Color?),    line 3430
//   static ResolveStatus -> DrawSpindle(Vector3, Quaternion, float, int, Color?),   line 3457
//   static ResetStatus   -> DrawSpindle(Matrix4x4, int, Color?),                    line 3462
//   static GetStatus     -> CreateSpindleMesh,     line 3486
//   static FlushStatus   -> CreateSpindleMaterial, line 3531
//   static ExcludeStatus -> ConfigureHandleMaterial, line 3538
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above.
//
// DEAD IN BOTH SHIPPED BUILDS. Nothing calls any of the three DrawSpindle overloads: they are
// private, and the only references to them anywhere in ADOverhaul2022 are the ones they make to each
// other. The mesh, material and colour-property statics exist only to serve them. Ported anyway,
// rather than dropped, because it is complete and coherent code whose behaviour is fully determined
// -- so recording what it does is more useful than deleting it and leaving three otherwise
// unexplained statics behind. It is not protector scaffolding; that pattern is a static object field
// paired with a null-check predicate, and several of those have been dropped elsewhere in this
// class.
//
// The names describe the geometry, because there is no call site to name them from. The mesh is a
// square bipyramid spanning z = 0 to z = 1: four triangles from a 0.2-wide square ring at z = 0.1
// back to the origin, and four more from the same ring forward to (0, 0, 1). Scaled uniformly by the
// distance between two points and aimed with LookRotation, it reads as a spike joining them.
//
// SHARED WITH CONTROLLEREDITOR, NOT CONSOLIDATED -- REPORT-ONLY OVERLAP.
// decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs has the same mesh builder
// (line 6388), the same material (line 6432) and the same four material settings (UpdateQueue, line
// 6437). Whether the pair belongs in DreadScripts.Common is a cross-product decision this file does
// not take.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>Cached shader property ID for <c>_Color</c>.</summary>
        private static readonly int colorPropertyId = Shader.PropertyToID("_Color");

        private static Mesh spindleMesh;

        private static Material spindleMaterial;

        /// <summary>
        /// Draws a spindle spanning <paramref name="from"/> to <paramref name="to"/>, with its
        /// square cross-section oriented by <paramref name="up"/>.
        /// </summary>
        /// <param name="controlID">
        /// Control the spindle belongs to; when it holds the mouse the spindle turns yellow, matching
        /// Unity's own active-handle colour. -1 for a spindle that is not interactive.
        /// </param>
        /// <param name="color">Defaults to the ambient <see cref="Handles.color"/>.</param>
        private static void DrawSpindle(Vector3 from, Vector3 to, Vector3 up, int controlID = -1, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = Handles.color;
            }

            if (controlID != -1 && GUIUtility.hotControl == controlID)
            {
                color = Color.yellow;
            }

            if (spindleMesh == null)
            {
                spindleMesh = CreateSpindleMesh();
            }

            if (spindleMaterial == null)
            {
                spindleMaterial = CreateSpindleMaterial();
            }

            ConfigureHandleMaterial(spindleMaterial);

            // The mesh spans one unit along +z, so scaling uniformly by the distance makes it reach
            // exactly from one point to the other. The scale is uniform rather than z-only, so the
            // spindle also gets thicker as it gets longer.
            float distance = Vector3.Distance(from, to);
            Vector3 direction = (to - from).normalized;
            Matrix4x4 matrix = Matrix4x4.TRS(from, Quaternion.LookRotation(direction, up), new Vector3(distance, distance, distance));

            spindleMaterial.SetColor(colorPropertyId, color.Value);
            spindleMaterial.SetPass(0);
            Graphics.DrawMeshNow(spindleMesh, matrix);
        }

        /// <summary>
        /// Draws a spindle at <paramref name="position"/>, aimed by <paramref name="rotation"/> and
        /// uniformly scaled to <paramref name="size"/>.
        /// </summary>
        /// <inheritdoc cref="DrawSpindle(Vector3, Vector3, Vector3, int, Color?)"/>
        private static void DrawSpindle(Vector3 position, Quaternion rotation, float size, int controlID = -1, Color? color = null)
        {
            DrawSpindle(Matrix4x4.TRS(position, rotation, new Vector3(size, size, size)), controlID, color);
        }

        /// <summary>Draws a spindle under an arbitrary transform.</summary>
        /// <inheritdoc cref="DrawSpindle(Vector3, Vector3, Vector3, int, Color?)"/>
        private static void DrawSpindle(Matrix4x4 matrix, int controlID = -1, Color? color = null)
        {
            if (!color.HasValue)
            {
                color = Handles.color;
            }

            if (controlID != -1 && GUIUtility.hotControl == controlID)
            {
                color = Color.yellow;
            }

            if (spindleMesh == null)
            {
                spindleMesh = CreateSpindleMesh();
            }

            if (spindleMaterial == null)
            {
                spindleMaterial = CreateSpindleMaterial();
            }

            ConfigureHandleMaterial(spindleMaterial);

            spindleMaterial.SetColor(colorPropertyId, color.Value);
            spindleMaterial.SetPass(0);
            Graphics.DrawMeshNow(spindleMesh, matrix);
        }

        /// <summary>
        /// Builds the spindle mesh: a square bipyramid from the origin to (0, 0, 1), with its waist
        /// at z = 0.1.
        /// </summary>
        /// <remarks>
        /// Every triangle gets its own three vertices -- 24 for 8 triangles -- so
        /// <see cref="Mesh.RecalculateNormals"/> produces flat faceted shading rather than smoothing
        /// across the edges. The index buffer is therefore just 0..23 in order. Uploaded
        /// non-readable and flagged <see cref="HideFlags.DontSave"/>, so it costs no CPU copy and is
        /// not written into a scene.
        /// </remarks>
        private static Mesh CreateSpindleMesh()
        {
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();

            Vector3[] vertices =
            {
                new Vector3(0.1f, 0.1f, 0.1f),   new Vector3(0.1f, -0.1f, 0.1f),  Vector3.zero,
                new Vector3(0.1f, -0.1f, 0.1f),  new Vector3(-0.1f, -0.1f, 0.1f), Vector3.zero,
                new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(-0.1f, 0.1f, 0.1f),  Vector3.zero,
                new Vector3(-0.1f, 0.1f, 0.1f),  new Vector3(0.1f, 0.1f, 0.1f),   Vector3.zero,

                new Vector3(0.1f, -0.1f, 0.1f),  new Vector3(0.1f, 0.1f, 0.1f),   Vector3.forward,
                new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(0.1f, -0.1f, 0.1f),  Vector3.forward,
                new Vector3(-0.1f, 0.1f, 0.1f),  new Vector3(-0.1f, -0.1f, 0.1f), Vector3.forward,
                new Vector3(0.1f, 0.1f, 0.1f),   new Vector3(-0.1f, 0.1f, 0.1f),  Vector3.forward
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
            mesh.UploadMeshData(markNoLongerReadable: true);
            mesh.hideFlags = HideFlags.DontSave;

            return mesh;
        }

        /// <summary>
        /// Builds the material the spindle is drawn with, from the built-in <c>UI/Unlit/Text</c>
        /// shader.
        /// </summary>
        /// <remarks>
        /// That shader is picked because it is always present in a build of the editor, takes a
        /// <c>_Color</c> and does no lighting -- which is what a handle wants. It is not being used
        /// for text.
        /// </remarks>
        private static Material CreateSpindleMaterial()
        {
            Material material = new Material(Shader.Find("UI/Unlit/Text"));
            ConfigureHandleMaterial(material);
            return material;
        }

        /// <summary>
        /// Applies the settings that make a material behave as a scene-view handle: back-face
        /// culling, no depth write, and a depth test that always passes.
        /// </summary>
        /// <remarks>
        /// The literals are the enum values Unity's shaders expect -- <c>_Cull</c> 2 is
        /// <c>CullMode.Back</c>, and <c>_ZTest</c> 8 is <c>CompareFunction.Always</c>, which is what
        /// draws the handle over the geometry it is attached to instead of inside it. Re-applied on
        /// every draw, as shipped.
        /// </remarks>
        private static void ConfigureHandleMaterial(Material material)
        {
            material.hideFlags = HideFlags.DontSave;
            material.SetInt("_Cull", 2);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_ZTest", 8);
        }
    }
}
