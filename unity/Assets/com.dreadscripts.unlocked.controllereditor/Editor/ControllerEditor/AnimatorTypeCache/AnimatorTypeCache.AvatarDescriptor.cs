// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTypeCache.cs
//   AvatarDescriptorBinding -> AvatarDescriptorBinding, line 240
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. The fields keep the decompiled order, which is the
// descriptor's own serialized field order.
// Audit status: VERIFIED against reverse-engineering/export/ member-by-member (2026-08-04).
// Note: the colliderFingerLittleRProperty field initializer (new SerializedObject(null).FindProperty)
// matches export exactly but is a decompiler mis-attribution / vendor artefact. Preserved faithfully.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorTypeCache
    {
        /// <summary>
        /// Every serialized field of a VRChat avatar descriptor, resolved once into a
        /// <see cref="SerializedProperty"/> per field.
        /// </summary>
        /// <remarks>
        /// The descriptor is held as a plain <see cref="Component"/> and its fields are named as
        /// strings, because the tool cannot reference the SDK — see <see cref="AnimatorTypeCache"/>.
        /// Resolving them all up front means the editor pays one lookup per field per selection
        /// rather than one per field per repaint. A field absent from the installed SDK version
        /// simply resolves to null; nothing here validates that, so consumers must null-check the
        /// properties they use.
        /// </remarks>
        internal class AvatarDescriptorBinding
        {
            private Component descriptor;

            private SerializedObject serializedObject;

            internal SerializedProperty viewPositionProperty;

            internal SerializedProperty animationsProperty;

            internal SerializedProperty scaleIPDProperty;

            internal SerializedProperty lipSyncProperty;

            internal SerializedProperty lipSyncJawBoneProperty;

            internal SerializedProperty lipSyncJawClosedProperty;

            internal SerializedProperty lipSyncJawOpenProperty;

            internal SerializedProperty visemeSkinnedMeshProperty;

            internal SerializedProperty mouthOpenBlendShapeNameProperty;

            internal SerializedProperty visemeBlendShapesProperty;

            internal SerializedProperty unityVersionProperty;

            internal SerializedProperty portraitCameraPositionOffsetProperty;

            internal SerializedProperty portraitCameraRotationOffsetProperty;

            internal SerializedProperty customExpressionsProperty;

            internal SerializedProperty expressionsMenuProperty;

            internal SerializedProperty expressionParametersProperty;

            internal SerializedProperty enableEyeLookProperty;

            internal SerializedProperty customEyeLookSettingsProperty;

            internal SerializedProperty customizeAnimationLayersProperty;

            internal SerializedProperty baseAnimationLayersProperty;

            internal SerializedProperty specialAnimationLayersProperty;

            internal SerializedProperty animationPresetProperty;

            internal SerializedProperty animationHashSetProperty;

            internal SerializedProperty autoFootstepsProperty;

            internal SerializedProperty autoLocomotionProperty;

            internal SerializedProperty colliderHeadProperty;

            internal SerializedProperty colliderTorsoProperty;

            internal SerializedProperty colliderFootRProperty;

            internal SerializedProperty colliderFootLProperty;

            internal SerializedProperty colliderHandRProperty;

            internal SerializedProperty colliderHandLProperty;

            internal SerializedProperty colliderFingerIndexLProperty;

            internal SerializedProperty colliderFingerMiddleLProperty;

            internal SerializedProperty colliderFingerRingLProperty;

            internal SerializedProperty colliderFingerLittleLProperty;

            internal SerializedProperty colliderFingerIndexRProperty;

            internal SerializedProperty colliderFingerMiddleRProperty;

            internal SerializedProperty colliderFingerRingRProperty;

            // Transcribed literally: the shipped assembly really does carry a field initializer here
            // that builds a SerializedObject over a null target and asks it for the *left* index
            // finger collider, before the constructor overwrites the field with the right little
            // finger's. It cannot be intentional - Unity throws on a null-target SerializedObject, so
            // this makes every construction of the binding fail - and it reads like an initializer
            // the obfuscator mis-attributed to the last field of the run. Ported as found rather than
            // guessed at; see the porting note on this file.
            internal SerializedProperty colliderFingerLittleRProperty = new SerializedObject((Object)null).FindProperty("collider_fingerIndexL");

            internal AvatarDescriptorBinding(Component descriptor)
            {
                this.descriptor = descriptor;
                serializedObject = new SerializedObject(descriptor);
                viewPositionProperty = serializedObject.FindProperty("ViewPosition");
                animationsProperty = serializedObject.FindProperty("Animations");
                scaleIPDProperty = serializedObject.FindProperty("ScaleIPD");
                lipSyncProperty = serializedObject.FindProperty("lipSync");
                lipSyncJawBoneProperty = serializedObject.FindProperty("lipSyncJawBone");
                lipSyncJawClosedProperty = serializedObject.FindProperty("lipSyncJawClosed");
                lipSyncJawOpenProperty = serializedObject.FindProperty("lipSyncJawOpen");
                visemeSkinnedMeshProperty = serializedObject.FindProperty("VisemeSkinnedMesh");
                mouthOpenBlendShapeNameProperty = serializedObject.FindProperty("MouthOpenBlendShapeName");
                visemeBlendShapesProperty = serializedObject.FindProperty("VisemeBlendShapes");
                unityVersionProperty = serializedObject.FindProperty("unityVersion");
                portraitCameraPositionOffsetProperty = serializedObject.FindProperty("portraitCameraPositionOffset");
                portraitCameraRotationOffsetProperty = serializedObject.FindProperty("portraitCameraRotationOffset");
                customExpressionsProperty = serializedObject.FindProperty("customExpressions");
                expressionsMenuProperty = serializedObject.FindProperty("expressionsMenu");
                expressionParametersProperty = serializedObject.FindProperty("expressionParameters");
                enableEyeLookProperty = serializedObject.FindProperty("enableEyeLook");
                customEyeLookSettingsProperty = serializedObject.FindProperty("customEyeLookSettings");
                customizeAnimationLayersProperty = serializedObject.FindProperty("customizeAnimationLayers");
                baseAnimationLayersProperty = serializedObject.FindProperty("baseAnimationLayers");
                specialAnimationLayersProperty = serializedObject.FindProperty("specialAnimationLayers");
                animationPresetProperty = serializedObject.FindProperty("AnimationPreset");
                animationHashSetProperty = serializedObject.FindProperty("animationHashSet");
                autoFootstepsProperty = serializedObject.FindProperty("autoFootsteps");
                autoLocomotionProperty = serializedObject.FindProperty("autoLocomotion");
                colliderHeadProperty = serializedObject.FindProperty("collider_head");
                colliderTorsoProperty = serializedObject.FindProperty("collider_torso");
                colliderFootRProperty = serializedObject.FindProperty("collider_footR");
                colliderFootLProperty = serializedObject.FindProperty("collider_footL");
                colliderHandRProperty = serializedObject.FindProperty("collider_handR");
                colliderHandLProperty = serializedObject.FindProperty("collider_handL");
                colliderFingerIndexLProperty = serializedObject.FindProperty("collider_fingerIndexL");
                colliderFingerMiddleLProperty = serializedObject.FindProperty("collider_fingerMiddleL");
                colliderFingerRingLProperty = serializedObject.FindProperty("collider_fingerRingL");
                colliderFingerLittleLProperty = serializedObject.FindProperty("collider_fingerLittleL");
                colliderFingerIndexRProperty = serializedObject.FindProperty("collider_fingerIndexR");
                colliderFingerMiddleRProperty = serializedObject.FindProperty("collider_fingerMiddleR");
                colliderFingerRingRProperty = serializedObject.FindProperty("collider_fingerRingR");
                colliderFingerLittleRProperty = serializedObject.FindProperty("collider_fingerLittleR");
            }
        }
    }
}
