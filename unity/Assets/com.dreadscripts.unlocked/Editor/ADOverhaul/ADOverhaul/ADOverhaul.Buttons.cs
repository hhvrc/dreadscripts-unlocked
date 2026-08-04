// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The tool's own toggle buttons: a button that stays held down while its value is on, tinted green
// or red so the state reads at a glance. Line numbers move with the snapshot; the member names are
// the durable reference.
//
//   LoginConfiguration(bool, GUIContent)                       (line 6726) -> ToggleIconButton
//   PatchConfiguration(string, ref bool, ...)                  (line 6735) -> ToggleButton(string, ref bool, ...)
//   CheckConfiguration(GUIContent, ref bool, ...)              (line 6740) -> ToggleButton(GUIContent, ref bool, ...)
//   CallConfiguration(SerializedProperty, string, Action, ...)  (line 6748) -> PropertyToggleButton(..., string, ...)
//   RegisterConfiguration(SerializedProperty, GUIContent, Action, ...) (line 6753) -> PropertyToggleButton(..., GUIContent, ...)
//
// These are ADOverhaul's, not ADOEditorUtility's: the utility class has a plain ToggleButton that
// returns the new value, while these four add the colour scope and the tri-state (off / on / mixed)
// palette that the multi-object inspectors need, and take their value by ref or through a
// SerializedProperty rather than returning it.
//
// Audit status: VERIFIED against export -- all five methods re-read against lines 6726-6767 on
// 2026-08-04.

using System;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// An 18x18 icon button that stays held down while <paramref name="value"/> is on, tinted
        /// green when on and red when off.
        /// </summary>
        /// <returns>The value after the click.</returns>
        internal static bool ToggleIconButton(bool value, GUIContent content)
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, value, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
            {
                return ADOEditorUtility.ToggleButton(value, content, ADOEditorUtility.styles.compactIconButton, GUILayout.Width(18f), GUILayout.Height(18f));
            }
        }

        /// <summary>
        /// A labelled toggle button that writes straight back into the caller's flag.
        /// </summary>
        /// <remarks>
        /// By-ref rather than by return value because the callers are drawing rows of scene-tool
        /// mode flags held in statics, where <c>ToggleButton("Radius", ref editingRadius)</c> is the
        /// whole statement.
        /// </remarks>
        internal static void ToggleButton(string text, ref bool value, params GUILayoutOption[] options)
        {
            ToggleButton(new GUIContent(text), ref value, options);
        }

        /// <inheritdoc cref="ToggleButton(string, ref bool, GUILayoutOption[])"/>
        internal static void ToggleButton(GUIContent content, ref bool value, params GUILayoutOption[] options)
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, value, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
            {
                value = ADOEditorUtility.ToggleButton(value, content, GUI.skin.button, options);
            }
        }

        /// <summary>
        /// A toggle button backed by a boolean <see cref="SerializedProperty"/>, tinted from the
        /// three-state palette so that a multi-selection which disagrees reads as mixed.
        /// </summary>
        /// <param name="onChanged">Invoked only when the click actually changed the property.</param>
        internal static void PropertyToggleButton(SerializedProperty property, string text, Action onChanged = null, params GUILayoutOption[] options)
        {
            PropertyToggleButton(property, new GUIContent(text), onChanged, options);
        }

        /// <inheritdoc cref="PropertyToggleButton(SerializedProperty, string, Action, GUILayoutOption[])"/>
        /// <remarks>
        /// The property is written from inside the change check rather than assigned unconditionally,
        /// so that drawing a mixed-value row does not collapse the selection to one value until the
        /// user actually presses it.
        /// </remarks>
        internal static void PropertyToggleButton(SerializedProperty property, GUIContent content, Action onChanged = null, params GUILayoutOption[] options)
        {
            int state = property.hasMultipleDifferentValues ? 2 : property.boolValue ? 1 : 0;

            using EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope();

            bool value;
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, state, ADOEditorUtility.styles.toggleStateColors))
            {
                value = ADOEditorUtility.ToggleButton(property.boolValue, content, GUI.skin.button, options);
            }

            if (changeCheck.changed)
            {
                property.boolValue = value;
                onChanged?.Invoke();
            }
        }
    }
}
