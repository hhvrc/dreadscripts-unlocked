// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The two containers every one of the tool's panels is built out of: the collapsible inspector
// section, and the draggable scene-view overlay. Line numbers move with the snapshot; the member
// names are the durable reference.
//
//   customer            (line 5650) -> toolOverlayResizeHandle
//   m_Database          (line 5652) -> toolOverlayDragControlId
//   SelectIdentifier    (line 8228) -> DrawSection
//   SelectConfiguration (line 6536) -> ResetFoldouts
//   WriteIdentifier     (line 8249) -> DrawSceneViewPanel(SceneView, string, ...)
//   MoveIdentifier      (line 8266) -> DrawSceneViewPanel(SceneView, Func<Rect>, ...)
//   PublishIdentifier   (line 8290) -> DrawSettingsButton
//
// NOT PORTED from this group, and deliberately so rather than left as a stub:
//   InitConfiguration (line 7631), the "title + help icon" header row. Its only four call sites are
//   the licence pane (7530, 7537, 7544), the licence-transfer pane (7580) and the feedback pane
//   (7020), all of which are removed with the licence code, so the header has no caller left.
//   DeleteIdentifier (line 7784), the "Made By @Dreadrith" credit button, is drawn only by the
//   toolbar strip (SortIdentifier), which is not ported yet.
//
// NO LICENCE CODE was removed from the members that are here -- none of them carried a gate. The
// licence pane reached DrawSection and DrawSceneViewPanel only as a caller.
//
// Audit status: VERIFIED against export -- every method re-read against lines 6536-6550 and
// 8228-8296 on 2026-08-04.

using System;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// The drag state of the scene-view tool overlay, shared by every panel so that moving one
        /// moves the corner all of them dock to.
        /// </summary>
        private static readonly ResizeHandle toolOverlayResizeHandle = new ResizeHandle();

        /// <summary>
        /// The IMGUI control id the overlay's title bar claims while it is being dragged.
        /// </summary>
        /// <remarks>
        /// Derived from a fixed string hash rather than requested per frame, because the handler
        /// that reads it is a static scene-view callback with no stable control ordering of its own.
        /// </remarks>
        private static readonly int toolOverlayDragControlId = GUIUtility.GetControlID("ADOTooltipDragControlID".GetHashCode(), FocusType.Passive);

        /// <summary>
        /// A collapsible inspector section: a bold title in a help box, an optional row of controls
        /// beside it, and a body that slides open and shut.
        /// </summary>
        /// <param name="header">Drawn to the right of the title, inside the same row.</param>
        /// <param name="body">Drawn inside the fading group.</param>
        /// <remarks>
        /// The whole box is a click target, not just the title, which is why the section takes an
        /// <see cref="AnimBool"/> instead of using <c>EditorGUILayout.Foldout</c>. When the user has
        /// turned animated foldouts off, the animation's value is snapped to its target so the box
        /// opens in one frame.
        /// </remarks>
        internal static void DrawSection(string title, AnimBool foldout, Action header, Action body)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(title, ADOEditorUtility.styles.indentedHeaderLabel);
                    header?.Invoke();
                }

                if (ADOEditorUtility.ClickArea())
                {
                    foldout.target = !foldout.target;
                    if (!ADOSettings.instance.editorAnimatedFoldouts)
                    {
                        foldout.value = foldout.target;
                    }
                }

                foldout.FadeGroup(body);
            }
        }

        /// <summary>
        /// Rebuilds an inspector's foldout animations and re-subscribes their repaint callback.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Called from <c>OnEnable</c>. The animations are static, so they outlive the inspector
        /// instance that subscribed to them; replacing each one instead of adding a second listener
        /// is what stops a destroyed editor's Repaint from being called.
        /// </para>
        /// <para>
        /// The open/shut state is carried across — a fresh <see cref="AnimBool"/> is built from the
        /// old one's target — so re-selecting a component does not collapse every section.
        /// </para>
        /// </remarks>
        internal static void ResetFoldouts(AnimBool[] foldouts, UnityAction onValueChanged)
        {
            for (int i = 0; i < foldouts.Length; i++)
            {
                foldouts[i] = foldouts[i] == null
                    ? new AnimBool()
                    : new AnimBool(foldouts[i].target);

                foldouts[i].valueChanged.AddListener(onValueChanged);
            }
        }

        /// <summary>
        /// The tool's scene-view overlay with its standard title bar: a centred title flanked by a
        /// spacer and the settings button, over the caller's body.
        /// </summary>
        internal static void DrawSceneViewPanel(SceneView sceneView, string title, Action body, float width, float height)
        {
            DrawSceneViewPanel(sceneView, () =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    ADOEditorUtility.IconSpacer();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(title, ADOEditorUtility.styles.centeredBoldRichLabel);
                    Rect titleRect = GUILayoutUtility.GetLastRect();
                    GUILayout.FlexibleSpace();
                    DrawSettingsButton();
                    return titleRect;
                }
            }, body, width, height);
        }

        /// <summary>
        /// The scene-view overlay in its general form, where the caller draws its own title bar.
        /// </summary>
        /// <param name="header">
        /// Draws the title bar and returns the rect that acts as its drag handle.
        /// </param>
        /// <param name="body">Drawn under a separator; may be null for a title-only panel.</param>
        /// <remarks>
        /// Dragging the title bar does not move the panel freely — releasing it opens the anchor
        /// picker over the scene view and the panel snaps to whichever corner is chosen, which is
        /// stored in <see cref="ADOSettings.toolOverlayAlignment"/>. The picker is drawn after the
        /// panel scope has closed, so it is not clipped by it.
        /// </remarks>
        internal static void DrawSceneViewPanel(SceneView sceneView, Func<Rect> header, Action body, float width, float height)
        {
            Rect sceneViewRect = sceneView.GetSceneViewRect();
            PositionFlag alignment = ADOSettings.instance.toolOverlayAlignment.GetEnumValue<PositionFlag>();

            bool dragging;
            using (new SceneViewPanel(sceneView, width, height, alignment, toolOverlayResizeHandle))
            {
                Rect dragRect = header();
                ADOEditorUtility.AddCursorRect(dragRect, MouseCursor.Pan);
                dragging = ADOEditorUtility.HasMouseCapture(dragRect, toolOverlayDragControlId);

                if (body != null)
                {
                    ADOEditorUtility.Separator(2, 0);
                    body();
                }
            }

            if (dragging)
            {
                Handles.BeginGUI();
                ADOSettings.instance.toolOverlayAlignment.IntValue = (int)ADOEditorUtility.AnchorPicker(alignment, sceneViewRect);
                Handles.EndGUI();
            }
        }

        /// <summary>The gear button that opens the settings window, drawn in every panel's title bar.</summary>
        internal static void DrawSettingsButton()
        {
            if (ADOEditorUtility.IconButton(ADOEditorUtility.contents.settings))
            {
                ADOverhaulWindow.ShowWindow();
            }
        }
    }
}
