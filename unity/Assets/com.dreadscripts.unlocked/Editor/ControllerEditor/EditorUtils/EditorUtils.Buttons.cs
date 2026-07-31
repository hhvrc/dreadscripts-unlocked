// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ExcludeQueue -> ToggleButton(bool, GUIContent, ...), line 5763
//   static FindQueue    -> ToggleButton(bool, GUIContent, ...) with no style, line 5758
//   static AddQueue     -> ToggleButton(bool, string, ...),     line 5748
//   static InvokeQueue  -> ToggleButton(bool, string, ...) with no style, line 5753
//   static RestartQueue -> Button(GUIContent, ...),             line 5732
//   static InsertQueue  -> Button(GUIContent, ...) with no style, line 5727
//   static CountQueue   -> Button(string, ...),                 line 5717
//   static DisableQueue -> Button(string, ...) with no style,    line 5722
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- the region is byte-identical between the two snapshots
// and every statement below was transcribed from it.
//
// The decompiled class carries a separate overload for each combination of (content type, style
// present), because the obfuscator had already split them; the optional parameters below collapse
// each pair back into one method, so four names map onto two here.
//
// Partial in progress: the Rect-based buttons (QueryQueue/CancelQueue, lines 5742-5746), the
// icon button (CallQueue, line 5695) and the "did the toggle change" variants (VisitQueue/
// InitQueue, lines 5779-5793) are not ported yet.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A layout button that returns true on the frame it is clicked, and shows the link cursor
        /// while hovered.
        /// </summary>
        internal static bool Button(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="Button(string, GUIStyle, GUILayoutOption[])"/>
        internal static bool Button(GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(false, content, style, options);
        }

        /// <summary>
        /// A layout button that stays visibly held down while <paramref name="value"/> is true.
        /// Returns the toggle's new value, so it reads true for as long as it is on rather than
        /// only on the click.
        /// </summary>
        internal static bool ToggleButton(bool value, string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return ToggleButton(value, new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="ToggleButton(bool, string, GUIStyle, GUILayoutOption[])"/>
        internal static bool ToggleButton(bool value, GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                style = GUI.skin.button;
            }

            // A toggle drawn with the button style, rather than GUILayout.Button, so that the same
            // method covers both the plain and the held-down case.
            bool result = GUILayout.Toggle(value, content, style, options);
            AddLinkCursor();
            return result;
        }
    }
}
