// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type,
// nested inside their respective settings classes.
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, class EditorSettings
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, class ADOSettings
//
// Member mapping. Because each tool carries its own copy, no single decompiled line number can
// stand for an entry; the two numbers in the right-hand column are the ControllerEditor line and
// the ADOverhaul2022 line, and the sub-entries below are keyed on the member names instead:
//   SettingBase                          -> SettingBase,                        1138 / 1416
//   SettingBase.defaultValue             -> SettingBase.defaultValue,           1140 / 1418
//   SettingBase.Reset / QueryCollection  -> SettingBase.Reset,                  1142 / 1420
//   NonSerializedSettingAttribute        -> NonSerializedSettingAttribute,      1145 / 1423
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DELIBERATE DEVIATION
// DrawResetButton has no counterpart in either source: there, each setting's draw method reached
// straight for its own product's icon table (EditorUtils.contents.reset drawn by IconButton in
// ControllerEditor, ADOEditorUtility's equivalent pair in ADOverhaul). Since neither table can be
// referenced from Common, the button became the one seam the framework leaves open. See the
// remarks on resetButton for what the built-in default reproduces.
//
// Audit status: PARTIAL -- the four mappings above were re-checked against decompiled/ in both
// tools (ControllerEditor.cs 1138-1146 and ADOverhaul.cs 1416-1424 in the post-561e9ec snapshot);
// the DrawResetButton deviation is a design note, not a transcription. The bodies were not re-diffed against decompiled/, so this is PARTIAL rather than VERIFIED.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// One persisted editor setting: a value that saves itself when assigned, remembers what it was
    /// created with, and knows how to draw itself.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Settings are declared as initialised fields of a product's settings class, and that class is
    /// round-tripped through <see cref="JsonUtility"/>. Only the <c>[SerializeField]</c> members of
    /// each subclass survive that trip; <see cref="defaultValue"/> and the change callbacks do not,
    /// and are re-established by the field initialiser running during deserialisation. That is the
    /// whole of the default-value mechanism: a setting absent from the stored JSON — because it was
    /// added after the JSON was written — simply keeps what its initialiser gave it.
    /// </para>
    /// <para>
    /// Values are stored by <see cref="JsonUtility"/>, whose number formatting and parsing are
    /// culture-invariant, so a settings block written under one locale reads back correctly under
    /// another.
    /// </para>
    /// </remarks>
    internal abstract class SettingBase
    {
        /// <summary>
        /// The value the setting was constructed with, boxed. Each subclass unboxes it with a hard
        /// cast in <see cref="Reset"/>, so the box must hold exactly the subclass's value type.
        /// </summary>
        internal object defaultValue;

        /// <summary>Returns the setting to <see cref="defaultValue"/>, saving as any assignment does.</summary>
        internal abstract void Reset();

        /// <summary>
        /// Draws the small revert button that sits at the right of most settings rows. Assign to
        /// have a product draw it with its own icon table and button style.
        /// </summary>
        /// <remarks>
        /// The default below reproduces the button ADOverhaul shipped, and the one ControllerEditor
        /// used for vector settings: an 18x18 refresh icon with no button chrome. ControllerEditor's
        /// other settings rows went through its <c>EditorUtils.IconButton</c> instead, which sizes
        /// the button to a line height and adds a link cursor — a product wanting that back assigns
        /// it here.
        /// </remarks>
        internal static Func<bool> resetButton { get; set; }

        private static GUIContent resetContent;

        private static GUIStyle resetStyle;

        internal static bool DrawResetButton()
        {
            if (resetButton != null)
            {
                return resetButton();
            }

            // Built lazily rather than in a static initialiser: both of these need the editor skin,
            // which is not loaded when the static constructor would run.
            if (resetContent == null)
            {
                resetContent = new GUIContent(EditorGUIUtility.IconContent("Refresh").image, "Reset");
            }

            if (resetStyle == null)
            {
                resetStyle = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(),
                    margin = new RectOffset(1, 1, 1, 1)
                };
            }

            return GUILayout.Button(resetContent, resetStyle, GUILayout.Width(18f), GUILayout.Height(18f));
        }
    }

    /// <summary>
    /// Marks a settings field that cannot go through <see cref="JsonUtility"/> with the rest of the
    /// block — typically a reference to a Unity object — so that it is persisted separately.
    /// </summary>
    /// <remarks>
    /// A product's settings class collects these fields by reflection and writes each one as its own
    /// <see cref="EditorJsonUtility"/> entry alongside the main block, keyed by field name. That
    /// keying is why such a field can be renamed only at the cost of its stored value.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    internal class NonSerializedSettingAttribute : Attribute
    {
    }
}
