// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   TrackingControlEditor  -> TrackingControlEditor, lines 284-421 (name already in renames/)
//     serializedObject ... mouth, properties, labels -> unchanged, lines 286-310 (already named)
//     SetAll               -> SetAll,  line 331
//     Draw                 -> Draw,    line 339
//     PrintIndexer / ResolveIndexer() -> dropped, lines 312 and 417 (obfuscator sentinel: a
//                             never-written static object plus a "== null" predicate nothing
//                             calls; see RE_NOTES "Self-referential dead members")
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   m_AlgoAnnotation, m_ConnectionAnnotation -- ControllerEditor outer class body (the currently
//                                               selected states; whether a tracking-control
//                                               behaviour is present on them)
//   ReflectPredicate                         -- EditorUtils (not yet ported)
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The inspector for VRChat's tracking-control behaviour, drawn over however many of them
        /// the current state selection carries.
        /// </summary>
        /// <remarks>
        /// The behaviour is addressed through <see cref="SerializedObject"/> rather than by type,
        /// because the tool cannot reference the VRChat SDK. All ten body parts share one enum, so
        /// the rows are held in a list and drawn in a loop, with an "All" row on top that reads the
        /// common value (or index 3 — the out-of-range "mixed" slot — when they disagree) and writes
        /// every row at once.
        ///
        /// The colour table has four entries for three enum values: index 3 is the mixed state.
        /// Left-clicking a row toggles it between "no change" and "tracking"; right-clicking toggles
        /// between "no change" and "animation".
        /// </remarks>
        private struct TrackingControlEditor
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

            private readonly List<SerializedProperty> properties;

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

                labels = new List<GUIContent>(properties.Select(
                    p => new GUIContent(p.displayName.Replace("Tracking ", string.Empty), p.tooltip)));
            }

            private void SetAll(int enumValueIndex)
            {
                properties.ForEach(delegate(SerializedProperty p)
                {
                    p.enumValueIndex = enumValueIndex;
                });
            }

            internal void Draw()
            {
                EditorGUI.BeginDisabledGroup(m_AlgoAnnotation.Count < 1);
                serializedObject.Update();

                using (new GUILayout.VerticalScope("helpbox"))
                {
                    // Index 3 is the mixed-value slot, not an enum member.
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
                                m_AlgoAnnotation.ForEach(delegate(AnimatorState s)
                                {
                                    s.ReflectPredicate(AnimatorTypeCache.TrackingControlType, isstate: true);
                                });
                                m_ConnectionAnnotation = false;
                                return;
                            }
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        List<SerializedProperty> all = properties;
                        int common = properties.All(p => !p.hasMultipleDifferentValues
                                                         && p.enumValueIndex == all[0].enumValueIndex)
                            ? properties[0].enumValueIndex
                            : 3;

                        using (new GUIColorScope(GUIColorScope.ColoringType.FG, common, colors))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                if (EditorUtils.Button("All", GUI.skin.label, GUILayout.ExpandWidth(false)))
                                {
                                    int target = Event.current.button == 0
                                        ? (common != 1 ? 1 : 0)
                                        : (common != 2 ? 2 : 0);
                                    SetAll(target);
                                }

                                GUILayout.FlexibleSpace();
                                EditorGUI.showMixedValue = common == 3;
                                EditorGUI.BeginChangeCheck();
                                common = EditorGUILayout.Popup(common, properties[0].enumDisplayNames,
                                    GUILayout.Width(260f));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    SetAll(common);
                                }

                                EditorGUI.showMixedValue = false;
                            }
                        }
                    }

                    EditorUtils.Separator();

                    for (int i = 0; i < properties.Count; i++)
                    {
                        SerializedProperty property = properties[i];
                        int colorIndex = property.hasMultipleDifferentValues ? 3 : property.enumValueIndex;

                        using (new GUIColorScope(GUIColorScope.ColoringType.FG, colorIndex, colors))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                if (EditorUtils.Button(labels[i], GUI.skin.label, GUILayout.ExpandWidth(false)))
                                {
                                    bool leftClick = Event.current.button == 0;
                                    property.enumValueIndex = leftClick
                                        ? (property.enumValueIndex != 1 ? 1 : 0)
                                        : (property.enumValueIndex != 2 ? 2 : 0);
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
}
