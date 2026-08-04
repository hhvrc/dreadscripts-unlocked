// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The property rows the replacement inspectors are assembled from — each one draws a
// SerializedProperty with something the built-in drawer does not give it. Line numbers move with
// the snapshot; the member names are the durable reference.
//
//   SearchConfiguration   (line 6717) -> DrawPropertyWithEditToggle
//   ChangeConfiguration   (line 6769) -> DrawOptionalProperty
//   StopConfiguration     (line 6777) -> DrawPermissionFilter
//   PrepareConfiguration  (line 6811) -> DrawParameterField
//   CustomizeConfiguration(line 6433) -> DrawCollisionTagElement
//   ConcatConfiguration   (line 6474) -> DrawCollisionTagsHeader
//
// The compiler-generated display structs _003C_003Ec__DisplayClass86_0 (line 5566) and _1 (line
// 5571) and the lifted local function PostIdentifier (line 8425) all belong to DrawParameterField
// and get no file; they are folded back into it as an ordinary local and a local function.
//
// LICENCE CODE REMOVED, one region:
//   DrawCollisionTagElement, export line 6451. Between laying the row's rects out and drawing
//   anything into them, the shipped body evaluated an inline Func<bool> that HMAC-SHA256s the
//   licence key and returns from the method when the digest does not match. The rect arithmetic
//   above it and the three controls below it are kept; only the guard is gone, so the row draws.
//   Nothing else in this group carried a gate.
//
// DEOBF-BUG(resolved) in DrawParameterField -- see the marker on the method. export/ wraps the
// add-parameter popup in a `while (true)` and gives its inner switch a `default: continue` arm. The
// 2019 build of the same method (ChangeSystem, line 6795 of
// decompiled/ADOverhaul2019/.../ADOverhaul.cs) has a plain `if (EditorGUI.EndChangeCheck())` with
// no loop and a `default: return`, which is the form reproduced here. export/ will keep showing the
// loop until de4dot changes; do not "fix" the deviation back.
//
// Audit status: VERIFIED against export -- every method re-read against lines 6433-6478 and
// 6717-6869 on 2026-08-04, and DrawParameterField cross-checked against the 2019 build.

using System.Text.RegularExpressions;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// A property row with the tool's "edit through the scene view" toggle on the right.
        /// </summary>
        /// <param name="editing">Whether the matching scene-view handle is currently active.</param>
        /// <returns>The toggle's value after the click.</returns>
        internal static bool DrawPropertyWithEditToggle(SerializedProperty property, bool editing)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(property);
                return ToggleIconButton(editing, ADOEditorUtility.contents.edit);
            }
        }

        /// <summary>Draws a property, or nothing at all when the caller has none to draw.</summary>
        /// <remarks>
        /// Saves each caller a null check where a row is only present on some of the component
        /// types sharing a layout.
        /// </remarks>
        internal static void DrawOptionalProperty(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        /// <summary>
        /// Draws one of VRChat's three-state permission properties as the pair of "Self" / "Others"
        /// checkboxes it really controls.
        /// </summary>
        /// <param name="permission">
        /// The tri-state property: 0 denies both, 1 allows both, 2 defers to
        /// <paramref name="filter"/>.
        /// </param>
        /// <param name="filter">The struct property holding <c>allowSelf</c> and <c>allowOthers</c>.</param>
        /// <remarks>
        /// <para>
        /// Editing either checkbox moves the permission to state 2 and writes both flags, because
        /// the two checkboxes cannot express the other two states between them — which is exactly
        /// the collapse the built-in drawer's separate enum and struct rows make the user perform by
        /// hand.
        /// </para>
        /// <para>
        /// Mixed values are shown for a multi-selection that disagrees about the enum, and also for
        /// one that agrees on state 2 but disagrees inside the filter.
        /// </para>
        /// </remarks>
        internal static void DrawPermissionFilter(SerializedProperty permission, SerializedProperty filter)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(permission.displayName, permission.tooltip));

                SerializedProperty allowSelf = filter.FindPropertyRelative("allowSelf");
                SerializedProperty allowOthers = filter.FindPropertyRelative("allowOthers");

                bool self = permission.enumValueIndex == 1 || (permission.enumValueIndex != 0 && allowSelf.boolValue);
                bool others = permission.enumValueIndex == 1 || (permission.enumValueIndex != 0 && allowOthers.boolValue);

                EditorGUI.BeginChangeCheck();
                EditorGUIUtility.labelWidth = 50f;

                using (new MixedValueScope(permission.hasMultipleDifferentValues || (permission.enumValueIndex == 2 && allowSelf.hasMultipleDifferentValues)))
                {
                    self = EditorGUILayout.Toggle("Self", self);
                }

                using (new MixedValueScope(permission.hasMultipleDifferentValues || (permission.enumValueIndex == 2 && allowOthers.hasMultipleDifferentValues)))
                {
                    others = EditorGUILayout.Toggle("Others", others);
                }

                EditorGUIUtility.labelWidth = 160f;

                if (EditorGUI.EndChangeCheck())
                {
                    permission.enumValueIndex = 2;
                    allowSelf.boolValue = self;
                    allowOthers.boolValue = others;
                }
            }
        }

        /// <summary>
        /// Draws a contact receiver's parameter-name field, with a dropdown of the parameters the
        /// target avatar already declares and a second dropdown that creates the named parameter on
        /// a chosen playable layer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The second dropdown is the point of the row: a receiver whose parameter does not exist
        /// on any controller does nothing at runtime and reports no error, so the tool offers to
        /// add it where it is missing rather than leaving the user to find out later.
        /// </para>
        /// <para>
        /// Both dropdowns are drawn with a selected index of -1 so that they always read as a
        /// picker rather than as a display of the current value, which the text field beside them
        /// already shows.
        /// </para>
        /// </remarks>
        internal static void DrawParameterField(SerializedProperty parameterProperty)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(parameterProperty);

                using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    int selected = EditorGUILayout.Popup(-1, avatarParameterNames, "textfielddropdown", GUILayout.Width(18f));
                    if (changeCheck.changed)
                    {
                        parameterProperty.stringValue = avatarParameterNames[selected];
                    }
                }

                // Nothing to add when the selection disagrees about the name, or when there is no
                // name yet.
                if (parameterProperty.hasMultipleDifferentValues || string.IsNullOrEmpty(parameterProperty.stringValue))
                {
                    return;
                }

                Rect addRect = EditorGUILayout.GetControlRect(GUILayout.Width(50f));

                BuildLayerParameterMenu(populatedLayerNames, populatedLayerValues, new[] { "Bool", "Int", "Float" }, out string[] paths, out int[] values);

                EditorGUI.BeginChangeCheck();
                int encoded = EditorGUI.IntPopup(addRect, -1, paths, values);

                // DEOBF-BUG(resolved): export/ wraps everything from `addRect` down in a
                // `while (true)` and closes the switch below with `default: continue`. The 2019
                // build has this plain `if` and a `default: return`.
                if (EditorGUI.EndChangeCheck())
                {
                    int[] choice = SplitDigits(encoded, 2);
                    if (targetAvatar.TryGetAnimatorController((VRCAvatarDescriptor.AnimLayerType)choice[0], out AnimatorController controller))
                    {
                        // The shipped code has this as a lifted local function taking the property
                        // and the controller through a display struct; it is a closure here.
                        void LogAddResult(bool added)
                        {
                            Log(added
                                ? parameterProperty.stringValue + " added to " + controller.name
                                : parameterProperty.stringValue + " already exists in " + controller.name);
                        }

                        switch (choice[1])
                        {
                            case 0:
                                LogAddResult(controller.AddParameterIfMissing(parameterProperty.stringValue, AnimatorControllerParameterType.Bool, 0f));
                                break;
                            case 1:
                                LogAddResult(controller.AddParameterIfMissing(parameterProperty.stringValue, AnimatorControllerParameterType.Int, 0f));
                                break;
                            case 2:
                                LogAddResult(controller.AddParameterIfMissing(parameterProperty.stringValue, AnimatorControllerParameterType.Float, 0f));
                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        Log("Couldn't fetch selected playable layer!", CustomLogType.Error);
                    }
                }

                // Drawn over the popup rather than as its label, because an IntPopup with a
                // selected index of -1 renders empty.
                addRect.x += 3f;
                GUI.Label(addRect, "Add");
            }
        }

        /// <summary>
        /// Draws one row of the collision-tags reorderable list: a picker of tags already in use on
        /// the avatar, the tag's own text field, and a remove button.
        /// </summary>
        /// <remarks>
        /// The picker strips the "Default/" prefix the menu groups VRChat's built-in tags under, so
        /// choosing one stores the bare tag name the SDK expects.
        /// </remarks>
        internal static void DrawCollisionTagElement(SerializedProperty tags, Rect rect, int index)
        {
            if (index >= tags.arraySize || index < 0)
            {
                return;
            }

            SerializedProperty tag = tags.GetArrayElementAtIndex(index);

            rect.y += 1f;
            rect.height = 18f;
            rect.width -= 44f;

            Rect pickerRect = rect;
            pickerRect.width = 21f;

            Rect fieldRect = rect;
            fieldRect.x += 22f;
            fieldRect.width -= 12f;

            Rect removeRect = fieldRect;
            removeRect.x += fieldRect.width;
            removeRect.width = 28f;

            using (new EditorGUI.DisabledScope(!(UnityEngine.Object)targetAvatar))
            {
                int picked = EditorGUI.Popup(pickerRect, -1, collisionTagOptions);
                if (picked != -1)
                {
                    tag.stringValue = Regex.Replace(collisionTagOptions[picked], "^Default/", string.Empty);
                }
            }

            EditorGUI.PropertyField(fieldRect, tag, GUIContent.none);

            if (GUI.Button(removeRect, ADOEditorUtility.contents.removeSelection, ADOEditorUtility.styles.footerButton))
            {
                tags.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>The collision-tags reorderable list's header.</summary>
        internal static void DrawCollisionTagsHeader(Rect rect)
        {
            GUI.Label(rect, "Collision Tags", new GUIStyle("boldlabel"));
        }
    }
}
