// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs,   lines 503-582
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs, lines 985-1058
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. The class is nested inside EditorUtils /
// ADOEditorUtility in the shipped assemblies; it is lifted to the namespace here.
//
// The two copies agree on Dispose, on the inset arithmetic, on the 46-pixel 2022 nudge and on the
// anchoring rules, but they parameterise the panel's height differently:
//   ADOverhaul  -- (row count, row height); panel height = rows * rowHeight.
//   ControllerEditor -- (height); panel height = height.
// The ADOverhaul form is the general one: ControllerEditor's height constructor is exactly
// ADOverhaul's rows = 1 case, and ADOverhaul ships that convenience constructor itself. Both are
// kept below. The one place they part company is the titled constructor, which reserves room for
// the title and its separator: ControllerEditor adds a flat 40 pixels, ADOverhaul adds two rows.
// These coincide at the default 20-pixel row height -- every shipped call site -- and diverge only
// for a caller that passes a different row height, so both are ported as shipped rather than
// unified.
//
// Mapping, by ADOverhaul member name (see the ADOverhaul line range at the top of this header):
//   ctor(SceneView, float, int, float, PositionFlag, ResizeHandle)         -> the primary constructor
//   ctor(SceneView, float, float, PositionFlag, ResizeHandle)              -> single-row constructor
//   ctor(SceneView, string, float, int, float, PositionFlag, ResizeHandle) -> titled constructor
//   GetAnchoredRect                                                       -> GetAnchoredRect
//   Dispose                                                               -> Dispose
//   ControllerEditor's ctor(SceneView, string, float, float, ...)          -> titled constructor (flat 40px)
//
// Depends on three members of the same source files that were unported when this file was written,
// and are now in place:
//   PositionFlag (enum) and its predicates -- Common/ResizeHandle/PositionFlag.cs and
//     Common/ResizeHandle/PositionFlagExtensions.cs. The predicates named IsRight/IsLeft/IsTop/
//     IsBottom/GetResizeEdges when this file was drafted landed as IsAnchoredRight/IsAnchoredLeft/
//     IsAnchoredTop/IsAnchoredBottom/GetResizeEdges, and are called by those names below.
//   ResizeHandle (class) -- Common/ResizeHandle/, GetResizedRect and HandleResize.
//   SceneView.AddStatus() (ControllerEditor: SortQueue) -- landed as
//     SceneViewExtensions.GetSceneViewRect in Common/SceneViewExtensions.cs, keeping the name this
//     file drafted.
// The rounded background (ADOverhaul ResetProcess / ControllerEditor SetResolver) is a thin wrapper
// over the already-ported EditorGuiUtils.DrawRoundedBox, so it is called directly with the shipped
// colours instead.
//
// 2019 vs 2022: identical apart from obfuscated names.
//
// Audit status: PARTIAL -- the ADOverhaul source range (ADOEditorUtility.cs 503-582, class
// SceneViewPanel and its three constructors) was re-checked against decompiled/; the
// ControllerEditor copy and the constructor bodies were not re-diffed.

using System;
using DreadScripts.ControllerEditor;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Draws a floating panel over the scene view for the duration of a <c>using</c> block. The
    /// constructor opens the panel and the panel's contents are drawn by the block body with plain
    /// GUILayout calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// What the constructor pushes, and <see cref="Dispose"/> must pop, is:
    /// a <see cref="Handles.BeginGUI"/> / <see cref="Handles.EndGUI"/> pair, which switches the
    /// scene view from 3D handle drawing to 2D GUI space; and a
    /// <see cref="GUILayout.BeginArea(Rect)"/> / <see cref="GUILayout.EndArea"/> pair, which
    /// establishes the panel's own layout group. Nothing else is saved and restored -- in
    /// particular GUI.color, GUI.enabled and the Handles matrix are left exactly as the caller had
    /// them, so anything the block body changes is the body's own to put back.
    /// </para>
    /// <para>
    /// Skipping <see cref="Dispose"/> is not survivable. The layout group and the handles GUI block
    /// stay open past the end of the panel, so the rest of the scene view's GUI draws inside the
    /// panel's clip rect and every subsequent frame reports mismatched layout groups -- the usual
    /// symptom is a scene view that goes blank and a console full of "you are pushing more
    /// GUIClips than you are popping". Because the pairing is what matters, the block body must not
    /// return or throw past the end of the <c>using</c> in a way that skips it, and must leave the
    /// layout stack as it found it.
    /// </para>
    /// <para>
    /// <see cref="Dispose"/> also swallows mouse-down events that land outside the panel and clears
    /// <see cref="GUIUtility.hotControl"/>, so a click beside the panel dismisses whatever the
    /// panel had focused instead of falling through and starting a scene selection or a handle
    /// drag. That is a deliberate side effect of disposal, not just cleanup: a caller that wants
    /// clicks to pass through has to clear <see cref="consumeMouseDown"/>.
    /// </para>
    /// </remarks>
    internal class SceneViewPanel : IDisposable
    {
        /// <summary>
        /// When set, the width passed to the constructor is a percentage of the scene view's width
        /// rather than a pixel count.
        /// </summary>
        public readonly bool widthIsPercentage;

        /// <summary>
        /// When set, a mouse press outside the panel is consumed on dispose rather than reaching the
        /// scene view underneath.
        /// </summary>
        public readonly bool consumeMouseDown = true;

        /// <summary>The final on-screen rect of the panel, after anchoring and resizing.</summary>
        private readonly Rect area;

        /// <summary>
        /// Opens a panel sized as a whole number of rows.
        /// </summary>
        /// <param name="width">Pixels, or percent of the scene view width when <see cref="widthIsPercentage"/> is set.</param>
        /// <param name="resizeHandle">Optional; when supplied it may override the anchored rect with the size the user dragged it to, and draws the drag edges.</param>
        public SceneViewPanel(SceneView sceneView, float width, int rows, float rowHeight = 20f, PositionFlag position = PositionFlag.BottomRight, ResizeHandle resizeHandle = null)
        {
            Handles.BeginGUI();

            Rect sceneViewRect = sceneView.GetSceneViewRect();

            // The area the panel is allowed to be dragged within: the scene view inset by the same
            // 4-pixel margin the anchoring uses, so a resized panel cannot be pushed against the edge.
            Rect dragBounds = new Rect(sceneViewRect)
            {
                x = sceneViewRect.x + 4f,
                y = sceneViewRect.y + 4f,
                width = sceneViewRect.width - 8f,
                height = sceneViewRect.height - 8f
            };

            Rect panelRect = GetAnchoredRect(sceneViewRect, width, rows, rowHeight, position, widthIsPercentage);
            if (resizeHandle != null)
            {
                panelRect = resizeHandle.GetResizedRect(panelRect, position, dragBounds);
                resizeHandle.HandleResize(panelRect, position.GetResizeEdges(true));
            }

            area = EditorGuiUtils.DrawRoundedBox(panelRect, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), 2f);

            // The 2022 scene view reports a rect that does not account for the overlay toolbar, so
            // the panel would otherwise be drawn 46 pixels too high and sit under it.
            if (EditorUtils.isUnity2022)
            {
                area.y += 46f;
            }

            GUILayout.BeginArea(area);
        }

        /// <summary>Opens a single-row panel of the given pixel height.</summary>
        public SceneViewPanel(SceneView sceneView, float width, float height = 20f, PositionFlag position = PositionFlag.BottomRight, ResizeHandle resizeHandle = null)
            : this(sceneView, width, 1, height, position, resizeHandle)
        {
        }

        /// <summary>
        /// Opens a panel with <paramref name="title"/> drawn centred at the top above a separator,
        /// reserving two extra rows for it.
        /// </summary>
        /// <param name="rows">Rows of content; the title and its separator are added on top.</param>
        public SceneViewPanel(SceneView sceneView, string title, float width, int rows, float rowHeight = 20f, PositionFlag position = PositionFlag.BottomRight, ResizeHandle resizeHandle = null)
            : this(sceneView, width, rows + 2, rowHeight, position, resizeHandle)
        {
            GUILayout.Label(title, EditorUtils.styles.centeredBoldRichLabel);
            EditorUtils.Separator(2, 0);
        }

        /// <summary>
        /// Opens a titled panel of a given content height, reserving a flat 40 pixels for the title
        /// and separator. This is ControllerEditor's shape of the titled constructor; it agrees with
        /// the row-based one whenever a row is the default 20 pixels tall.
        /// </summary>
        public SceneViewPanel(SceneView sceneView, string title, float width = 200f, float height = 20f, PositionFlag position = PositionFlag.BottomRight, ResizeHandle resizeHandle = null)
            : this(sceneView, width, height + 40f, position, resizeHandle)
        {
            GUILayout.Label(title, EditorUtils.styles.centeredBoldRichLabel);
            EditorUtils.Separator(2, 0);
        }

        public void Dispose()
        {
            if (consumeMouseDown)
            {
                Event current = Event.current;
                if (current.type == EventType.MouseDown && !area.Contains(current.mousePosition))
                {
                    current.Use();

                    // Releasing the hot control as well, so a control the panel had captured does
                    // not keep receiving drag events after the click that dismissed the panel.
                    GUIUtility.hotControl = 0;
                }
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        /// <summary>
        /// Places a panel of the requested size against the edge or corner of
        /// <paramref name="sceneViewRect"/> named by <paramref name="position"/>, centring it on any
        /// axis the position does not pin.
        /// </summary>
        /// <remarks>
        /// The 4-pixel margin is applied to a local copy while the returned rect keeps the scene
        /// view's own origin, which is what lets a right- or bottom-anchored panel sit inset from
        /// the edge rather than flush against it.
        /// </remarks>
        private static Rect GetAnchoredRect(Rect sceneViewRect, float width, int rows, float rowHeight = 20f, PositionFlag position = PositionFlag.Bottom, bool widthIsPercentage = false)
        {
            Rect result = sceneViewRect;
            sceneViewRect.x += 4f;
            sceneViewRect.width -= 8f;

            float panelWidth = widthIsPercentage ? (width * sceneViewRect.width / 100f) : width;
            float panelHeight = rows * rowHeight;

            bool isRight = position.IsAnchoredRight();
            bool isLeft = position.IsAnchoredLeft();
            bool isTop = position.IsAnchoredTop();
            bool isBottom = position.IsAnchoredBottom();

            float x = isLeft
                ? sceneViewRect.x
                : (isRight ? (sceneViewRect.x + sceneViewRect.width - panelWidth) : (sceneViewRect.x + sceneViewRect.width / 2f - panelWidth / 2f));
            float y = isTop
                ? sceneViewRect.y
                : (isBottom ? (sceneViewRect.y + sceneViewRect.height - panelHeight) : (sceneViewRect.y + sceneViewRect.height / 2f - panelHeight / 2f));

            result.x = x;
            result.y = y;
            result.width = panelWidth;
            result.height = panelHeight;
            return result;
        }
    }
}
