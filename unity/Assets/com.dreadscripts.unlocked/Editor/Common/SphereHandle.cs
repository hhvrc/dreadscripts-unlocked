// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. The two are the same type with the name transposed -- ADOverhaul calls it
// SphereHandle, ControllerEditor calls it HandleSphere -- with identical fields, identical
// factory defaults and identical drawing. Reconstructed from:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs,   lines 852-917 (SphereHandle)
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs, lines 1082-1148 (HandleSphere)
// SphereHandle is kept here as the clearer of the two shipped names: it reads as "a handle that
// is a sphere", and matches Unity's own Handles.SphereHandleCap that it draws with.
//
//   ADOverhaul member -> ported member
//   getDistances      -> distanceFunc   (ControllerEditor already called it distanceFunc)
//   DrawDefault       -> DrawDefault
//   Create            -> Create
//   FindStatus(...)   -> DrawSceneLabel, line 3616 (see note below)
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DrawSceneLabel stays private here, and is NOT redirected to ADOEditorUtility.DrawSceneLabel
// (Editor/ADOverhaul/ADOEditorUtility/ADOEditorUtility.Handles.cs), even though that helper has
// since landed and its body is character-for-character identical to the copy below. The earlier
// note in this header asked for exactly that redirect; it is withdrawn, because the shared helper
// it named is not shared. ADOEditorUtility is ADOverhaul-only, while this type is in
// DreadScripts.Common precisely because BOTH products ship it -- ControllerEditor's copy is
// HandleSphere, and it called ControllerEditor's own EditorUtils.CreateQueue (decompiled
// EditorUtils.cs line 6157), never ADOverhaul's. Pointing Common at ADOverhaul would give the
// ControllerEditor half of the package a dependency on the ADOverhaul half that neither shipped
// assembly had -- the same product-to-product coupling that put Json, SemVer and GUIColorScope in
// Common in the first place. The package currently compiles as one assembly so the redirect would
// build, but it would quietly make the two products inseparable.
// The de-duplication that IS correct is to promote the label helper into Common, the way the
// scene-view rect pair already was (ADOverhaul AddStatus/ValidateStatus and ControllerEditor
// SortQueue/RegisterQueue -> Common.SceneViewExtensions), and have both products' utility classes
// call that. Whoever does it should note that the two shipped copies are not quite identical: the
// depth guard is `vector.z > 0f` in ADOverhaul and `!(vector.z <= 0f)` in ControllerEditor, which
// differ when WorldToGUIPointWithDepth returns NaN -- ADOverhaul skips the label, ControllerEditor
// draws it at a NaN position. The copy below is the ADOverhaul form, matching this type's source.
//
// Deliberately not ported:
//  - Three fields that exist in both copies but are never read or written anywhere in either
//    product: a Quaternion, a Vector3 and a float[]. They carry no behaviour.
//  - A private static object field paired with a "field == null" predicate (IncludeCandidate /
//    SortDescriptor). The field is never assigned, so the predicate is a constant true; it is an
//    obfuscator tamper-check stub, not product behaviour.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// A clickable sphere drawn into the scene view, optionally labelled, with pluggable drawing
    /// and hit-testing.
    /// </summary>
    /// <remarks>
    /// Callers build one per object they want to expose in the scene (a PhysBone, a collider, a
    /// bone position), then drive selection themselves from <see cref="GetDistances"/> rather than
    /// letting Unity's handle system own the picking. Both <see cref="onDraw"/> and
    /// <see cref="distanceFunc"/> are fields rather than virtual members so a call site can swap in
    /// its own drawing -- typically to tint the sphere by selection state -- and still delegate to
    /// <see cref="DrawDefault"/> for the rest.
    /// </remarks>
    internal struct SphereHandle
    {
        /// <summary>Text drawn above the sphere; nothing is drawn when null or blank.</summary>
        internal string label;

        internal GUIStyle labelStyle;

        internal Vector3 position;

        /// <summary>Radius passed to the handle cap, in world units.</summary>
        internal float size;

        /// <summary>
        /// The control id the handle cap draws under, so the caller can correlate a picked control
        /// back to the object this handle stands for. -1 means the handle is decorative.
        /// </summary>
        internal int controlId;

        internal Action onClick;

        /// <summary>
        /// Returns the screen-space distances used to decide whether the pointer is over this
        /// handle. It returns an array rather than a single value so a handle standing for
        /// something with extent -- a bone chain, a capsule -- can report one distance per part.
        /// </summary>
        internal Func<SphereHandle, float[]> distanceFunc;

        internal Action<SphereHandle> onDraw;

        /// <summary>
        /// Creates a handle with the default sphere-and-label drawing and a single distance measured
        /// to the sphere's silhouette.
        /// </summary>
        /// <param name="size">Radius in world units; the default is deliberately small so handles do not obscure the model.</param>
        /// <param name="controlId">-1 for a handle that is not meant to be picked.</param>
        internal static SphereHandle Create(Vector3 position, string label = "", float size = 0.05f, int controlId = -1, Action onClick = null)
        {
            return new SphereHandle
            {
                onDraw = DrawDefault,

                // A copy of the style, not the shared instance, because a caller is free to recolor
                // its own handle's label and must not tint every bold label in the editor.
                labelStyle = new GUIStyle(EditorStyles.boldLabel),
                distanceFunc = (SphereHandle handle) => new float[1] { HandleUtility.DistanceToCircle(handle.position, handle.size / 2f) },
                position = position,
                size = size,
                label = label,
                controlId = controlId,
                onClick = onClick
            };
        }

        internal void Draw()
        {
            onDraw(this);
        }

        internal float[] GetDistances()
        {
            return distanceFunc(this);
        }

        /// <summary>
        /// Draws the sphere cap and its label. Exposed so a caller that replaces
        /// <see cref="onDraw"/> only to change state around the draw -- a color scope, say -- can
        /// still call back into the standard appearance.
        /// </summary>
        internal static void DrawDefault(SphereHandle handle)
        {
            Handles.SphereHandleCap(handle.controlId, handle.position, Quaternion.identity, handle.size, EventType.Repaint);
            if (!string.IsNullOrWhiteSpace(handle.label))
            {
                DrawSceneLabel(handle.label, handle.position, handle.size, handle.labelStyle);
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
        /// <remarks>
        /// Both products carry this as a static helper on their editor-utility class (FindStatus in
        /// ADOverhaul, CreateQueue in ControllerEditor), so this is the shared copy rather than a
        /// third one. It was private while the utility regions were unported; ADOEditorUtility has
        /// since landed and deliberately did not port FindStatus again, and the ADOverhaul root
        /// class calls it directly in three places, so it is internal now.
        /// </remarks>
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
    }
}
