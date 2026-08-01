// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   TrackingControlEditor                    -> TrackingControlEditor,   line 284
//     serializedObject                       -> serializedObject,        line 286
//     head .. mouth                          -> head .. mouth,           lines 288-305
//     properties                             -> properties,              line 308
//     labels                                 -> labels,                  line 310
//     .ctor(StateMachineBehaviour[])         -> .ctor,                   line 314
//     SetAll                                 -> SetAll,                  line 331
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The type was a private nested type of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// Not ported: the static field `PrintIndexer` (line 312) and `ResolveIndexer()` (line 417), which
// returns whether that field is null. The field is never assigned anywhere in the assembly and the
// method has no callers - obfuscator scaffolding, omitted.
//
// Deferred member (depends on code that is not ported yet, omitted rather than stubbed):
//   Draw(), line 339 - the whole IMGUI body. It reads and writes three members of the not-yet-
//   ported ControllerEditor window class: the selected-state list `m_AlgoAnnotation` (line 8024),
//   for both the disabled-group guard and the "remove the tracking control behaviour from every
//   selected state" button, and the `m_ConnectionAnnotation` flag (line 8064) that the same button
//   clears. It also needs the EditorUtils extensions InvokeResolver and ReflectPredicate
//   (EditorUtils.cs lines 2555 and 3620), which are likewise unported. Everything else it uses -
//   EditorUtils.Button, EditorUtils.styles, EditorUtils.Separator, GUIColorScope and
//   AnimatorTypeCache.TrackingControlType - is already available, so Draw can be restored as-is
//   once the window class lands. SetAll is kept because it is complete and is Draw's only helper.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A multi-object editor for VRChat's animator tracking-control state behaviour.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The behaviour exposes ten tracking targets - head, hands, hips, feet, fingers, eyes and mouth
    /// - each an enum of "no change / tracking / animation / inherit". The SDK's own inspector draws
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
    }
}
