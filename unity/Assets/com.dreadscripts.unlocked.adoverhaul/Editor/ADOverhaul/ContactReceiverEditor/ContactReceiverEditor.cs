// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the ContactReceiverEditor class, lines 1690-1958 of the current snapshot. Line
// numbers move with the snapshot; the decompiled names are the durable reference.
//
//   _GetterIdentifier       -> foldouts,                     line 1692
//   _ThreadIdentifier       -> overrideInstalled,            line 1699
//   m_AlgoIdentifier        -> collisionTagList,             line 1701
//   m_RoleIdentifier        -> shapeType,                    line 1703
//   visitorIdentifier       -> rootTransform,                line 1705
//   invocationIdentifier    -> radius,                       line 1707
//   m_ListenerIdentifier    -> height,                       line 1709
//   m_ParserIdentifier      -> position,                     line 1711
//   m_PrinterIdentifier     -> rotation,                     line 1713
//   m_RepositoryIdentifier  -> collisionTags,                line 1715
//   m_DescriptorIdentifier  -> allowSelf,                    line 1717
//   _StrategyIdentifier     -> allowOthers,                  line 1719
//   globalIdentifier        -> localOnly,                    line 1721
//   m_ManagerIdentifier     -> receiverType,                 line 1723
//   m_WorkerIdentifier      -> parameter,                    line 1725
//   m_ItemIdentifier        -> minVelocity,                  line 1727
//   m_IndexerIdentifier     -> receiverComponentType,        line 1729
//   poolIdentifier          -> sdkReceiverEditorType,        line 1731
//   OnInspectorGUI          -> unchanged,                    line 1733
//   method_0                -> OnSceneGUI,                   line 1795
//   CallPage                -> DrawCollisionTagRow,          line 1800
//   RegisterPage            -> RecomputeShapeCapabilities,   line 1805
//   ChangePage              -> CacheProperties,              line 1837
//   OnEnable                -> unchanged,                    line 1859
//   OnDisable               -> unchanged,                    line 1865
//   StopPage                -> ConvertToSender,              line 1870
//   PushPage                -> ConvertToCollider,            line 1881
//   PreparePage             -> ToggleEditorOverride,         line 1892
//   ReadPage                -> InstallEditorOverride,        line 1898
//   AssetPage               -> NOT PORTED, line 1953 -- decompiler artifact: a lambda's capture of
//       `this.target`. Uses of it are written as `target`.
//
// LIFTED OUT OF ADOverhaul, as the three sibling inspectors were.
//
// LICENCE GATE, NOT PORTED. The same pair as the sibling editors -- outer FlushConfiguration, inner
// inline HMAC Func<bool> -- both dropped, body runs unconditionally, and GetConfiguration (the
// licence banner) goes with them. The two conversion menu items open with MoveConfiguration, which
// is the gate and nothing else, so it is dropped and the bodies kept.
//
// NOTES
// Three sections, not the sender's two: Shape, Receiver and Filtering. DrawTargetAvatarSelector is
// drawn at the top of BOTH Receiver and Filtering -- that is what the snapshot does, and it is not a
// transcription slip; the selector is idempotent and each section is independently collapsible.
//
// The Receiver section shows a live, read-only value while playing: the receiver's own parameter
// name and paramValue in a disabled FloatField, indented one level. It is guarded on a single
// non-null target with a non-empty parameter, so it does not draw for a multi-selection.
//
// minVelocity is drawn only when receiverType is index 1 (OnEnter) or the selection disagrees, since
// it is meaningless for the other receiver types.
//
// Component kind 2 is the contact receiver, for DrawShapeHandles.
//
// The three filter toggles go through DrawPropertyToggleButton's GUIContent overload
// (RegisterConfiguration in the snapshot; CallConfiguration is its string-taking sibling), each fed
// the property's own GetContent so the label and tooltip come from the SDK's serialised field.
//
// Audit status: PARTIAL -- every member was transcribed statement by statement from 1690-1958,
// including the thirteen property resolves in their snapshot order, the ReorderableList flags and
// the play-mode readout's guards. The 2019 build was not read for this type.

using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace DreadScripts.ADOverhaul
{
    /// <summary>ADOverhaul's replacement inspector for <see cref="VRCContactReceiver"/>.</summary>
    internal sealed class ContactReceiverEditor : Editor
    {
        /// <summary>
        /// Expansion animation for the "Shape", "Receiver" and "Filtering" sections, in that order.
        /// Shape starts open, the other two closed.
        /// </summary>
        private static readonly AnimBool[] foldouts = { new AnimBool(true), new AnimBool(), new AnimBool() };

        /// <summary>Whether this editor is currently installed over the SDK's.</summary>
        private static bool overrideInstalled = true;

        private static Type receiverComponentType;
        private static Type sdkReceiverEditorType;

        private ReorderableList collisionTagList;

        private SerializedProperty shapeType;
        private SerializedProperty rootTransform;
        private SerializedProperty radius;
        private SerializedProperty height;
        private SerializedProperty position;
        private SerializedProperty rotation;
        private SerializedProperty collisionTags;
        private SerializedProperty allowSelf;
        private SerializedProperty allowOthers;
        private SerializedProperty localOnly;
        private SerializedProperty receiverType;
        private SerializedProperty parameter;
        private SerializedProperty minVelocity;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CacheProperties();

            ADOverhaul.DrawFoldoutBox("Shape", foldouts[0], null, () =>
                ADOverhaul.DrawShapeProperties(
                    target,
                    new[] { shapeType, rootTransform, radius, height, position, rotation },
                    RecomputeShapeCapabilities,
                    isPhysBoneCollider: false));

            ADOverhaul.DrawFoldoutBox("Receiver", foldouts[1], null, DrawReceiverSection);

            ADOverhaul.DrawFoldoutBox("Filtering", foldouts[2], null, () =>
            {
                ADOverhaul.DrawTargetAvatarSelector();

                using (new GUILayout.HorizontalScope())
                {
                    ADOverhaul.DrawPropertyToggleButton(allowSelf, allowSelf.GetContent(), null);
                    ADOverhaul.DrawPropertyToggleButton(allowOthers, allowOthers.GetContent(), null);
                    ADOverhaul.DrawPropertyToggleButton(localOnly, localOnly.GetContent(), null);
                }

                collisionTagList.DoLayoutList();
            });

            serializedObject.ApplyModifiedProperties();
            ADOverhaul.DrawToolHeader();
        }

        /// <summary>
        /// Receiver type, the driven animator parameter, the velocity threshold where it applies, and
        /// -- while playing -- the parameter's live value.
        /// </summary>
        private void DrawReceiverSection()
        {
            ADOverhaul.DrawTargetAvatarSelector();
            EditorGUILayout.PropertyField(receiverType);
            ADOverhaul.DrawAvatarParameterField(parameter);

            // minVelocity only means anything to the OnEnter receiver type (index 1); a disagreeing
            // multi-selection shows it too, since one of them may be that type.
            if (receiverType.hasMultipleDifferentValues || receiverType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(minVelocity);
            }

            ContactReceiver receiver = target as ContactReceiver;
            if (receiver == null
                || !Application.isPlaying
                || serializedObject.isEditingMultipleObjects
                || string.IsNullOrEmpty(receiver.parameter))
            {
                return;
            }

            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(receiver.parameter, receiver.paramValue);
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>Draws the receiver's shape handles in the scene view. Component kind 2.</summary>
        private void OnSceneGUI()
        {
            ADOverhaul.DrawShapeHandles(target, targets, 2, Color.cyan);
        }

        private void OnEnable()
        {
            ADOverhaul.ResetFoldouts(foldouts, Repaint);
            ADOverhaul.BeginShapeInspectorSession(RecomputeShapeCapabilities);
        }

        private void OnDisable()
        {
            ADOverhaul.SetShapeEditOverlayActive(false);
        }

        private void DrawCollisionTagRow(Rect rowRect, int index, bool isActive, bool isFocused)
        {
            ADOverhaul.DrawCollisionTagElement(collisionTags, rowRect, index);
        }

        /// <summary>
        /// Works out which of radius, height and rotation are meaningful across the whole selection.
        /// </summary>
        private void RecomputeShapeCapabilities()
        {
            serializedObject.ApplyModifiedProperties();

            bool hasRotation = false;
            bool hasHeight = false;
            bool hasRadius = false;

            foreach (UnityEngine.Object inspected in targets)
            {
                VRCContactReceiver receiver = (VRCContactReceiver)inspected;

                if (hasRadius && hasHeight && hasRotation)
                {
                    break;
                }

                switch ((int)receiver.shapeType)
                {
                    case 0:
                        hasRadius = true;
                        break;

                    case 1:
                        hasRotation = true;
                        hasHeight = true;
                        hasRadius = true;
                        break;

                    default:
                        hasRotation = true;
                        break;
                }
            }

            ADOverhaul.SetShapeCapabilities(hasRadius, hasHeight, hasRotation);
        }

        private void CacheProperties()
        {
            rootTransform = serializedObject.FindProperty("rootTransform");
            shapeType = serializedObject.FindProperty("shapeType");
            radius = serializedObject.FindProperty("radius");
            height = serializedObject.FindProperty("height");
            position = serializedObject.FindProperty("position");
            rotation = serializedObject.FindProperty("rotation");
            collisionTags = serializedObject.FindProperty("collisionTags");
            allowSelf = serializedObject.FindProperty("allowSelf");
            allowOthers = serializedObject.FindProperty("allowOthers");
            localOnly = serializedObject.FindProperty("localOnly");
            receiverType = serializedObject.FindProperty("receiverType");
            parameter = serializedObject.FindProperty("parameter");
            minVelocity = serializedObject.FindProperty("minVelocity");

            collisionTagList = new ReorderableList(serializedObject, collisionTags,
                draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
            {
                drawElementCallback = DrawCollisionTagRow,
                drawHeaderCallback = ADOverhaul.DrawCollisionTagsHeader
            };
        }

        [MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Sender", false, 897)]
        private static void ConvertToSender(MenuCommand command)
        {
            VRCContactReceiver receiver = (VRCContactReceiver)command.context;
            receiver.ToContactSender(receiver.gameObject);
            Undo.DestroyObjectImmediate(receiver);
        }

        [MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Collider", false, 898)]
        private static void ConvertToCollider(MenuCommand command)
        {
            VRCContactReceiver receiver = (VRCContactReceiver)command.context;
            receiver.ToPhysBoneCollider(receiver.gameObject);
            Undo.DestroyObjectImmediate(receiver);
        }

        [MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/Toggle Editor", false, 899)]
        private static void ToggleEditorOverride()
        {
            InstallEditorOverride(overrideInstalled);
        }

        /// <summary>
        /// Points Unity's editor table for <c>VRCContactReceiver</c> at this inspector, or back at
        /// the SDK's.
        /// </summary>
        internal static void InstallEditorOverride(bool revert = false)
        {
            if (receiverComponentType == null)
            {
                receiverComponentType = ADOEditorUtility.FindType("VRCContactReceiver");
            }

            if (sdkReceiverEditorType == null)
            {
                sdkReceiverEditorType = ADOEditorUtility.FindType("VRCContactReceiverEditor");
            }

            overrideInstalled = !revert;

            ADOEditorUtility.OverrideCustomEditor(
                receiverComponentType,
                !overrideInstalled ? sdkReceiverEditorType : typeof(ContactReceiverEditor));
        }
    }
}
