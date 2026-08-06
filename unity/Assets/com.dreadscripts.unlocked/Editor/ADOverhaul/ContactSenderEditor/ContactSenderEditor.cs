// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the ContactSenderEditor class, lines 1959-2159 of the current snapshot. Line
// numbers move with the snapshot; the decompiled names are the durable reference.
//
//   _StructIdentifier       -> foldouts,                     line 1961
//   _InterpreterIdentifier  -> overrideInstalled,            line 1967
//   _ParameterIdentifier    -> collisionTagList,             line 1969
//   attrIdentifier          -> shapeType,                    line 1971
//   objectIdentifier        -> rootTransform,                line 1973
//   m_ServiceIdentifier     -> radius,                       line 1975
//   _ReponseIdentifier      -> height,                       line 1977
//   specificationIdentifier -> position,                     line 1979
//   _WrapperIdentifier      -> rotation,                     line 1981
//   m_InfoIdentifier        -> collisionTags,                line 1983
//   m_ModelIdentifier       -> senderType,                   line 1985
//   m_ConfigIdentifier      -> sdkSenderEditorType,          line 1987
//   OnInspectorGUI          -> unchanged,                    line 1989
//   method_0                -> OnSceneGUI,                   line 2028
//   DestroyProperty         -> DrawCollisionTagRow,          line 2033
//   NewProperty             -> RecomputeShapeCapabilities,   line 2038
//   CompareProperty         -> CacheProperties,              line 2070
//   OnEnable                -> unchanged,                    line 2086
//   OnDisable               -> unchanged,                    line 2092
//   VerifyProperty          -> ConvertToReceiver,            line 2097
//   SetProperty             -> ConvertToCollider,            line 2108
//   SortProperty            -> ToggleEditorOverride,         line 2119
//   InvokeProperty          -> InstallEditorOverride,        line 2125
//   CustomizeProperty       -> NOT PORTED, line 2140 -- decompiler artifact: the lifted body of the
//       "Shape" foldout closure, marked [CompilerGenerated]. Written inline here.
//   ConcatProperty          -> NOT PORTED, line 2146 -- the same, for the "Filtering" closure.
//   LogoutProperty          -> NOT PORTED, line 2155 -- the same, for a lambda's capture of
//       `this.target`. Uses of it are written as `target`.
//
// LIFTED OUT OF ADOverhaul, exactly as PhysBoneEditor and PhysBoneColliderEditor were: the decompiled
// type is a private nested class, and it becomes a top-level internal type here. Nothing referred to
// it by name from outside ADOverhaul.
//
// LICENCE GATE, NOT PORTED. OnInspectorGUI is wrapped in the usual pair -- an outer
// `if (FlushConfiguration())` whose else-branch draws the activation panel, and an inner inline
// Func<bool> HMAC check that returns without drawing. Both are dropped and the body runs
// unconditionally. GetConfiguration, the licence banner between the commit and DrawToolHeader, goes
// with them. The two conversion menu items open with `if (MoveConfiguration())`, which is
// `Log(...); return isLicensed;` and nothing else, so the gate is dropped and the bodies kept.
//
// NOTES
// The six-property array passed to DrawShapeProperties is positional and its order matters:
// [shapeType, rootTransform, radius, height, position, rotation]. It is the six-entry form, without
// the collider's trailing insideBounds/bonesAsSpheres pair, and DrawShapeProperties is told
// isPhysBoneCollider: false so it does not look for them.
//
// Component kind 1 is the contact sender, for DrawShapeHandles. The collider is 0 and the receiver
// is 2.
//
// The capability switch reads by numeric case, as the snapshot has it: case 0 Sphere contributes
// radius, case 1 Capsule contributes all three, and the default arm (Plane) contributes rotation.
// Note the arms are emitted in a different order here than in the collider's copy -- 1, 0, default
// against the collider's default, 1, 0 -- which is a decompiler ordering difference and not a
// behavioural one.
//
// Audit status: PARTIAL -- every member was transcribed statement by statement from 1959-2159,
// including the ReorderableList construction flags (draggable, header and add shown, remove hidden)
// and both conversion commands' Undo.DestroyObjectImmediate tail. The 2019 build was not read for
// this type.

using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;

namespace DreadScripts.ADOverhaul
{
    /// <summary>ADOverhaul's replacement inspector for <see cref="VRCContactSender"/>.</summary>
    internal sealed class ContactSenderEditor : Editor
    {
        /// <summary>
        /// Expansion animation for the "Shape" and "Filtering" sections, in that order. Shape starts
        /// open, Filtering closed.
        /// </summary>
        private static readonly AnimBool[] foldouts = { new AnimBool(true), new AnimBool() };

        /// <summary>Whether this editor is currently installed over the SDK's.</summary>
        private static bool overrideInstalled = true;

        private static Type senderType;
        private static Type sdkSenderEditorType;

        private ReorderableList collisionTagList;

        private SerializedProperty shapeType;
        private SerializedProperty rootTransform;
        private SerializedProperty radius;
        private SerializedProperty height;
        private SerializedProperty position;
        private SerializedProperty rotation;
        private SerializedProperty collisionTags;

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

            ADOverhaul.DrawFoldoutBox("Filtering", foldouts[1], null, () =>
            {
                ADOverhaul.DrawTargetAvatarSelector();
                using (new GUILayout.VerticalScope())
                {
                    collisionTagList.DoLayoutList();
                }
            });

            serializedObject.ApplyModifiedProperties();
            ADOverhaul.DrawToolHeader();
        }

        /// <summary>Draws the sender's shape handles in the scene view. Component kind 1.</summary>
        private void OnSceneGUI()
        {
            ADOverhaul.DrawShapeHandles(target, targets, 1, Color.yellow);
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
                VRCContactSender sender = (VRCContactSender)inspected;

                if (hasRadius && hasHeight && hasRotation)
                {
                    break;
                }

                switch ((int)sender.shapeType)
                {
                    case 1:
                        hasRotation = true;
                        hasHeight = true;
                        hasRadius = true;
                        break;

                    case 0:
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

            collisionTagList = new ReorderableList(serializedObject, collisionTags,
                draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
            {
                drawElementCallback = DrawCollisionTagRow,
                drawHeaderCallback = ADOverhaul.DrawCollisionTagsHeader
            };
        }

        [MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Receiver", false, 897)]
        private static void ConvertToReceiver(MenuCommand command)
        {
            VRCContactSender sender = (VRCContactSender)command.context;
            sender.ToContactReceiver(sender.gameObject);
            Undo.DestroyObjectImmediate(sender);
        }

        [MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Collider", false, 898)]
        private static void ConvertToCollider(MenuCommand command)
        {
            VRCContactSender sender = (VRCContactSender)command.context;
            sender.ToPhysBoneCollider(sender.gameObject);
            Undo.DestroyObjectImmediate(sender);
        }

        [MenuItem("CONTEXT/VRCContactSender/ADOverhaul/Toggle Editor", false, 899)]
        private static void ToggleEditorOverride()
        {
            InstallEditorOverride(overrideInstalled);
        }

        /// <summary>
        /// Points Unity's editor table for <c>VRCContactSender</c> at this inspector, or back at the
        /// SDK's.
        /// </summary>
        internal static void InstallEditorOverride(bool revert = false)
        {
            if (senderType == null)
            {
                senderType = ADOEditorUtility.FindType("VRCContactSender");
            }

            if (sdkSenderEditorType == null)
            {
                sdkSenderEditorType = ADOEditorUtility.FindType("VRCContactSenderEditor");
            }

            overrideInstalled = !revert;

            ADOEditorUtility.OverrideCustomEditor(
                senderType,
                !overrideInstalled ? sdkSenderEditorType : typeof(ContactSenderEditor));
        }
    }
}
