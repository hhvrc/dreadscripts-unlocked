// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static MapQueue -> Separator, line 5933
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Draws a horizontal rule across the current layout width.
        /// </summary>
        /// <param name="thickness">Height of the drawn line, in pixels.</param>
        /// <param name="spacing">
        /// Blank pixels reserved around the line, split evenly above and below it.
        /// </param>
        /// <param name="maxWidth">Caps the rule's width; zero or less means the full width.</param>
        /// <remarks>
        /// The line is nudged two pixels left and made six pixels wider than the control rect so it
        /// runs past the inspector's inner margin and reaches the panel edges, which is what makes it
        /// read as a divider rather than as a control.
        /// </remarks>
        internal static void Separator(int thickness = 2, int spacing = 10, int maxWidth = 0)
        {
            // The whole thickness + spacing block is reserved as one control, then the drawn rect is
            // shrunk back to the line itself, so the spacing lands as real layout gaps.
            Rect rect = maxWidth <= 0
                ? EditorGUILayout.GetControlRect(GUILayout.Height(thickness + spacing))
                : EditorGUILayout.GetControlRect(GUILayout.Height(thickness + spacing), GUILayout.MaxWidth(maxWidth));

            rect.height = thickness;
            rect.y += spacing / 2f;
            rect.x -= 2f;
            rect.width += 6f;

            // Darker than the background on the pro skin, lighter-mid grey on the light skin; both
            // are chosen to sit just off the panel colour rather than contrast with it.
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out Color color);
            EditorGUI.DrawRect(rect, color);
        }
    }
}
