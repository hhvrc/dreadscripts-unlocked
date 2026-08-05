// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   TrackingControlEditor                    -> TrackingControlEditor,   line 284
//     serializedObject                       -> serializedObject,        line 286
//     head .. mouth                          -> head .. mouth,           lines 288-305
//     properties                             -> properties,              line 308
//     labels                                 -> labels,                  line 310
//     .ctor(StateMachineBehaviour[])         -> .ctor,                   line 314
//     SetAll                                 -> SetAll,                  line 331
//     Draw                                   -> Draw,                    line 339
//   PrintIndexer                             -> NOT PORTED, line 312 -- obfuscator scaffolding
//   ResolveIndexer                           -> NOT PORTED, line 417 -- obfuscator scaffolding
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// The type was a private nested type of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// The static field `PrintIndexer` (line 312) and `ResolveIndexer()` (line 417), which returns
// whether that field is null, are omitted. The field is never assigned anywhere in the assembly
// and the method has no callers - obfuscator scaffolding.
//
// Draw was deferred when this file was written and landed on 2026-08-05; the type is complete.
// All four of its blockers had in fact been ported already, under the English names that replaced
// the obfuscated ones the note was written against: `m_AlgoAnnotation` is
// ControllerEditor.selectedStates and `m_ConnectionAnnotation` is
// ControllerEditor.allStatesHaveTrackingControl (both in ControllerEditor.State.cs), InvokeResolver
// is EditorUtils.ForEach and ReflectPredicate is EditorUtils.RemoveBehaviourOfType (both in
// EditorUtils.Behaviours.cs). Nothing new had to be derived. The remaining call targets the note
// listed as already available - EditorUtils.Button, EditorUtils.styles, EditorUtils.Separator
// (decompiled MapQueue), GUIColorScope and AnimatorTypeCache.TrackingControlType - were used as-is.
//
// The one change the port needed: the two ControllerEditor statics are `internal` rather than the
// shipped `private`, because this type could reach a private static of the window when it was
// nested inside it and cannot now that it is top-level. That file's own remarks record it.
//
// The type's remarks used to spell the target enum as "no change / tracking / animation / inherit".
// Nothing in the decompiled source names its members - the SDK type is reached reflectively - and a
// fourth member would contradict Draw, which uses index 3 as an out-of-range "mixed" marker. The
// invented member list is gone; the enum is described by role only.
//
// SHIPPED BUG
// The remove button's early return (decompiled line 364) leaves the frame's
// EditorGUI.BeginDisabledGroup (line 341) unclosed and skips ApplyModifiedProperties, since the
// matching calls are the last two statements of the method. Reproduced as shipped.
//
// Audit status: VERIFIED -- every member here was compared statement by statement against
// ControllerEditor.cs lines 284-421, and the two omissions above are the only members in that
// range the file does not declare.

using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A multi-object editor for VRChat's animator tracking-control state behaviour.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The behaviour exposes ten tracking targets - head, hands, hips, feet, fingers, eyes and mouth
    /// - each an enum of tracking modes owned by the SDK. The SDK's own inspector draws
    /// them as a plain field list; this one adds a colour-coded row per target plus an "All" row that
    /// reports and sets the common value across every target and every selected state at once, which
    /// is how these behaviours are actually used.
    /// </para>
    /// <para>
    /// The behaviours are reached through a multi-object <see cref="SerializedObject"/> rather than
    /// by type, both because the SDK cannot be referenced directly (see
    /// <see cref="AnimatorTypeCache"/>) and because that is what gives mixed-value handling and undo
    /// across the selection for free.
    /// </para>
    /// </remarks>
    internal struct TrackingControlEditor
    {
        private readonly SerializedObject serializedObject;

        private SerializedProperty head;

        private SerializedProperty leftHand;

        private SerializedProperty rightHand;

        private SerializedProperty hip;

        private SerializedProperty leftFoot;

        private SerializedProperty rightFoot;

        private SerializedProperty leftFingers;

        private SerializedProperty rightFingers;

        private SerializedProperty eyes;

        private SerializedProperty mouth;

        /// <summary>
        /// The ten target properties in display order, so the drawer can treat them uniformly.
        /// </summary>
        private readonly List<SerializedProperty> properties;

        /// <summary>
        /// Labels for <see cref="properties"/>, cached because building a GUIContent per property per
        /// repaint would be pure garbage.
        /// </summary>
        private readonly List<GUIContent> labels;

        internal TrackingControlEditor(StateMachineBehaviour[] behaviours)
        {
            serializedObject = new SerializedObject(behaviours);
            head = serializedObject.FindProperty("trackingHead");
            leftHand = serializedObject.FindProperty("trackingLeftHand");
            rightHand = serializedObject.FindProperty("trackingRightHand");
            hip = serializedObject.FindProperty("trackingHip");
            leftFoot = serializedObject.FindProperty("trackingLeftFoot");
            rightFoot = serializedObject.FindProperty("trackingRightFoot");
            leftFingers = serializedObject.FindProperty("trackingLeftFingers");
            rightFingers = serializedObject.FindProperty("trackingRightFingers");
            eyes = serializedObject.FindProperty("trackingEyes");
            mouth = serializedObject.FindProperty("trackingMouth");

            properties = new List<SerializedProperty>
            {
                head, leftHand, rightHand, hip, leftFoot,
                rightFoot, leftFingers, rightFingers, eyes, mouth
            };

            // The SDK names every field "Tracking <part>"; the prefix is already the group's header,
            // so it is stripped to keep the rows narrow. The tooltip is taken as-is.
            labels = new List<GUIContent>(properties.Select(
                p => new GUIContent(p.displayName.Replace("Tracking ", string.Empty), p.tooltip)));
        }

        /// <summary>Sets every tracking target to the same enum value.</summary>
        private void SetAll(int enumValueIndex)
        {
            properties.ForEach(p => p.enumValueIndex = enumValueIndex);
        }

        /// <summary>
        /// Draws the whole tracking-control group: the header with its remove button, the "All" row,
        /// and one row per tracking target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Value 3 is used throughout as a fourth, out-of-range enum index meaning "the targets
        /// disagree". It selects the mixed colour from <c>colors</c> and drives
        /// <see cref="EditorGUI.showMixedValue"/>; it is never written to a property, because the two
        /// places that could write it - the "All" click and the popup - both replace it first.
        /// </para>
        /// <para>
        /// Every label doubles as a click target, and the click is read off
        /// <see cref="Event.current"/>'s button: the left button toggles the target between
        /// <c>Tracking</c> and <c>NoChange</c>, any other button between <c>Animation</c> and
        /// <c>NoChange</c>. Clicking a value that is already set therefore clears it.
        /// </para>
        /// <para>
        /// The whole group is disabled when nothing is selected, and the edits are made through the
        /// multi-object <see cref="SerializedObject"/>, so one
        /// <see cref="SerializedObject.ApplyModifiedProperties"/> at the end covers every selected
        /// behaviour and gives undo for free.
        /// </para>
        /// </remarks>
        internal void Draw()
        {
            EditorGUI.BeginDisabledGroup(ControllerEditor.selectedStates.Count < 1);
            serializedObject.Update();

            using (new GUILayout.VerticalScope("helpbox"))
            {
                // Indexed by enum value, with the fourth entry standing in for "mixed".
                Color[] colors =
                {
                    new Color(0.7f, 0.7f, 0.7f),
                    Color.green,
                    Color.yellow,
                    Color.cyan
                };

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Tracking Control");

                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.red))
                    {
                        if (EditorUtils.Button(EditorUtils.styles.remove, EditorUtils.styles.paddedBox,
                            GUILayout.Width(25f), GUILayout.Height(20f)))
                        {
                            // The behaviours this editor was built over are about to be destroyed, so
                            // the drawing is abandoned rather than finished against dead objects. See
                            // the file header for what that early return skips.
                            ControllerEditor.selectedStates.ForEach<AnimatorState>(
                                s => s.RemoveBehaviourOfType(AnimatorTypeCache.TrackingControlType, withUndo: true));
                            ControllerEditor.allStatesHaveTrackingControl = false;
                            return;
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    // A struct's method cannot capture `this` in a lambda, so the field is read into
                    // a local before the predicate can reach it.
                    List<SerializedProperty> allProperties = properties;
                    int commonValue = allProperties.All(
                        p => !p.hasMultipleDifferentValues && p.enumValueIndex == allProperties[0].enumValueIndex)
                        ? properties[0].enumValueIndex
                        : 3;

                    using (new GUIColorScope(GUIColorScope.ColoringType.FG, commonValue, colors))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            if (EditorUtils.Button("All", GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
                            {
                                int toggled = (Event.current.button == 0)
                                    ? ((commonValue != 1) ? 1 : 0)
                                    : ((commonValue != 2) ? 2 : 0);
                                SetAll(toggled);
                            }

                            GUILayout.FlexibleSpace();

                            EditorGUI.showMixedValue = commonValue == 3;
                            EditorGUI.BeginChangeCheck();
                            commonValue = EditorGUILayout.Popup(
                                commonValue, properties[0].enumDisplayNames, GUILayout.Width(260f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                SetAll(commonValue);
                            }

                            EditorGUI.showMixedValue = false;
                        }
                    }
                }

                EditorUtils.Separator();

                for (int i = 0; i < properties.Count; i++)
                {
                    SerializedProperty property = properties[i];
                    int displayValue = property.hasMultipleDifferentValues ? 3 : property.enumValueIndex;

                    using (new GUIColorScope(GUIColorScope.ColoringType.FG, displayValue, colors))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            if (EditorUtils.Button(labels[i], GUI.skin.label, GUILayout.ExpandWidth(expand: false)))
                            {
                                property.enumValueIndex = (Event.current.button == 0)
                                    ? ((property.enumValueIndex != 1) ? 1 : 0)
                                    : ((property.enumValueIndex != 2) ? 2 : 0);
                            }

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.Width(260f));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndDisabledGroup();
        }
    }
}
