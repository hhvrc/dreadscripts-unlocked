// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the inspector layout of PhysBoneEditor and the enable/disable pair that seeds the
// state it draws from. Line numbers are relative to the current snapshot; the decompiled names are
// the durable reference.
//
//   _AnnotationIdentifier -> foldouts,                     line 2974
//   m_CodeIdentifier      -> parameterReadoutSplitState,   line 2976
//   threadAuthentication  -> parameterReadoutSplitStateBuilt, line 3130
//   OnInspectorGUI        -> unchanged,                    line 3212
//   ConcatSingleton       -> HandleShortcuts,              line 3899
//   CollectSingleton      -> BuildParameterReadoutSplitState, line 4279
//   OnEnable              -> unchanged,                    line 4293
//   OnDisable             -> unchanged,                    line 4310
//   ViewSingleton         -> DrawIntegrationTypeToggle,    line 4428
//   PostSingleton         -> DrawBindingRow,               line 4443
//   ListSingleton         -> DrawBindingRowIfPresent,      line 4469
//   ForgotSingleton       -> NOT PORTED, line 4477 -- an unused variant of PostSingleton. It takes
//       the value and curve properties directly instead of a binding index and returns an icon
//       toggle instead of arming the property-edit tool. Nothing in either build calls it.
//   UpdateSingleton       -> DrawCurveField,               line 4487
//
// LICENCE GATE, NOT PORTED. The shipped OnInspectorGUI is one if/else whose whole body is the
// layout below:
//
//     if (!FlushConfiguration()) { if (!isLicensed) EnableConfiguration(SelectSingleton); }
//     else { if (!<inline HMAC-SHA256 check>()) return; <the layout> }
//
// FlushConfiguration (7719) draws the activation panel and returns false while the tool is
// unlicensed; the inline Func<bool> HMACs two outer-class strings against a hard-coded key and
// returns without drawing anything if the digest does not match. Both are the protector's
// activation gate, both are dropped in full, and what is left is the else-branch body running
// unconditionally. This is the same treatment the gate gets in PhysBoneParameter and
// ObfuscationMarker.
//
// GetConfiguration (7699), called between the last foldout and DrawToolHeader, is dropped for the
// same reason: it is two labels reading licenseVariant and licensedToDisplayName -- "License:
// Personal" and "Authorized For: <name>" -- and nothing else. The statics it reads are among the
// licence statics ADOverhaul.State.cs declines to declare.
//
// PARTIAL PORT. Three things OnEnable does are left out, all of them for the same unported member:
//
//   m_ProcessorIdentifier (2990) -- the static "live editor instance" handle, assigned first thing
//       in OnEnable. Its only reader is the scene-view GUI, which calls Repaint on it. Nothing in
//       this reconstruction reads it, so assigning it here would be dead state.
//   VerifySingleton (3563) -- the static SceneView.duringSceneGui handler that draws every scene
//       tool. OnEnable subscribes it and OnDisable unsubscribes it; both halves are omitted
//       together, so the subscription cannot leak. Porting it is what would make the scene tools
//       work; see the omissions list in PhysBoneEditor.cs for the rest of that surface.
//
// The tool-mode state the inspector's toggles write (isEditingEndpoints, isSelectingColliders and
// the rest) is therefore set and shown correctly but drives nothing, since the handler that would
// act on it is not here. That is a visible-but-inert tool row, not a broken one: ExitTool still
// clears it, and OnDisable still calls ExitTool.
//
// DEOBF-BUG(resolved). The "Grab & Pose" body renders its two permission rows as
//     if (hasCollisionFilter) { while (true) { DrawSelfOthersToggles(a); DrawSelfOthersToggles(b); } }
// -- an unconditional hang that would also make the two PropertyFields after it unreachable. The
// 2019 build's copy of the same body (2019 line 3363) writes it as a plain `if` with the two calls
// inside and the two PropertyFields after, and every sibling foldout in the 2022 build is shaped
// that way too. It is a decompiler artefact of the same family already recorded on
// ADOEditorUtility.VRChatComponents.cs; the plain `if` is what shipped and is what is written here.
//
// SHIPPED BUG
// Two labels are misspelled in the shipped build and are reproduced as-is: "Allow Collsion" on the
// collision permission toggle and "Limit Opacitiy" on both limit-opacity sliders (the local one and
// the global one). Correcting them would change what a user searching the inspector sees.
//
// DrawToolHeader is drawn *last*, after the final foldout and after the commit. That is the shipped
// order, not a transcription slip -- the licence banner GetConfiguration sat immediately before it,
// so the pair reads as a footer. It is kept where the original had it.
//
// NOTES
// The foldout indices are written out (0..7) rather than reproduced as the decompiled mixture of a
// running counter and two literal resets (`num = 1`, `num = 2`, then `_AnnotationIdentifier[num++]`
// six times). The sequence those produce is 0,1,2,3,4,5,6,7 in draw order, which is what the
// literals say; the counter form only exists because the compiler hoisted the array loads.
//
// The three limit-rotation curves are cleared by one chained assignment sharing a single
// AnimationCurve instance across all three properties, as the original does.
// SerializedProperty.animationCurveValue serialises a copy on assignment, so the sharing is not
// observable -- transcribed rather than tidied.
//
// Audit status: PARTIAL -- every member declared here was transcribed statement by statement from
// the 2022 snapshot regions named in the MAP, including all eight foldout bodies, both header
// actions, the parameter-suggestion filter and the add-missing-parameter menu, and the play-mode
// readout. The 2019 build was consulted only for the Grab & Pose control flow recorded above; the
// rest of this region was not diffed against it. Nothing here has been run in Unity -- the
// inspector is still not installed over the SDK's, because the registration hook remains unported
// (see ADOverhaul.Lifecycle.cs, WriteConfiguration), so this layout has been read but not seen.

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        // Foldout indices, in draw order. The array is sized 8 in the shipped build and all eight
        // are used.
        private const int TransformsFoldout = 0;
        private const int ForcesFoldout = 1;
        private const int LimitsFoldout = 2;
        private const int CollisionsFoldout = 3;
        private const int GrabPoseFoldout = 4;
        private const int StretchSquishFoldout = 5;
        private const int OptionsFoldout = 6;
        private const int GizmosFoldout = 7;

        /// <summary>
        /// Expansion animation for the eight sections, in the order the constants above give.
        /// </summary>
        /// <remarks>
        /// Static, like the rest of this type's state, so the sections stay open across selection
        /// changes. The entries start null and are seeded by <see cref="ADOverhaul.ResetFoldouts"/>
        /// on every enable, which is also what re-points their repaint callback at the current
        /// inspector instance.
        /// </remarks>
        private static readonly AnimBool[] foldouts = new AnimBool[8];

        /// <summary>
        /// Splitter state for the play-mode parameter readout: one column per PhysBone parameter
        /// that has a readable backing field, all equally wide to start with.
        /// </summary>
        /// <remarks>
        /// Opaque because <c>UnityEditor.SplitterState</c> is internal; see
        /// <see cref="GUILayoutUtils"/>. It has to outlive a frame, since it carries the widths the
        /// user has dragged the columns to.
        /// </remarks>
        private static object parameterReadoutSplitState;

        /// <summary>
        /// One-shot guard for <see cref="BuildParameterReadoutSplitState"/>: the column count
        /// depends only on the installed SDK, which cannot change without a domain reload.
        /// </summary>
        private static bool parameterReadoutSplitStateBuilt;

        /// <summary>
        /// Builds <see cref="parameterReadoutSplitState"/> on first use, with one equal-width column
        /// per readable PhysBone parameter.
        /// </summary>
        private static void BuildParameterReadoutSplitState()
        {
            if (parameterReadoutSplitStateBuilt)
            {
                return;
            }

            parameterReadoutSplitStateBuilt = true;

            float[] columnWidths = new float[ADOEditorUtility.physBoneParameters.Count(p => p.hasBackingField)];
            for (int i = 0; i < columnWidths.Length; i++)
            {
                columnWidths[i] = 1f / columnWidths.Length;
            }

            parameterReadoutSplitState = GUILayoutUtils.CreateSplitterState(columnWidths);
        }

        private void OnEnable()
        {
            BuildParameterReadoutSplitState();
            ADOverhaul.ResetFoldouts(foldouts, Repaint);
            ApplyGlobalGizmoSettings();
            ADOverhaul.RefreshSceneAvatars(ref ADOverhaul.selectedAvatar, ref ADOverhaul.sceneAvatars);
            ADOverhaul.RefreshAvatarTables();

            Transform root = ((VRCPhysBone)TargetObject()).transform.root;
            selectedPhysBones = targets.Cast<VRCPhysBone>().ToArray();
            sceneColliders = root.GetComponentsInChildren<VRCPhysBoneCollider>();
            scenePhysBones = root.GetComponentsInChildren<VRCPhysBone>();
            candidateTransforms = selectedPhysBones
                .SelectMany(pb => pb.GetRootTransform().GetComponentsInChildren<Transform>())
                .ToArray();
        }

        private void OnDisable()
        {
            ExitTool();
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            HandleShortcuts();

            serializedObject.Update();
            CacheProperties();
            ADOverhaul.DrawTestModeToolbar(selectedPhysBones);

            EditorGUIUtility.labelWidth = 160f;

            ADOverhaul.DrawFoldoutBox("Transforms", foldouts[TransformsFoldout], null, DrawTransformsSection);
            ADOverhaul.DrawFoldoutBox("Forces", foldouts[ForcesFoldout], DrawIntegrationTypeToggle, DrawForcesSection);
            ADOverhaul.DrawFoldoutBox("Limits", foldouts[LimitsFoldout], null, DrawLimitsSection);

            // Every permission pair below is drawn one of two ways depending on the installed SDK.
            // The older shape is a plain bool that the section header carries as a toggle button; the
            // newer one is a tri-state enum plus a self/others filter struct, which needs a row of
            // its own inside the section. collisionFilter is the probe for both, since the three
            // filters arrived together.
            bool hasPermissionFilters = collisionFilter != null;

            Action collisionsHeader = null;
            if (!hasPermissionFilters)
            {
                collisionsHeader = () => ADOverhaul.DrawPropertyToggleButton(
                    allowCollision, "Allow Collsion", null, GUILayout.ExpandWidth(false));
            }

            ADOverhaul.DrawFoldoutBox("Collisions", foldouts[CollisionsFoldout], collisionsHeader,
                () => DrawCollisionsSection(hasPermissionFilters));

            Action grabPoseHeader = null;
            if (!hasPermissionFilters)
            {
                grabPoseHeader = () =>
                {
                    ADOverhaul.DrawPropertyToggleButton(allowGrabbing, "Allow Grabbing", null, GUILayout.ExpandWidth(false));
                    ADOverhaul.DrawPropertyToggleButton(allowPosing, "Allow Posing", null, GUILayout.ExpandWidth(false));
                };
            }

            ADOverhaul.DrawFoldoutBox("Grab & Pose", foldouts[GrabPoseFoldout], grabPoseHeader,
                () => DrawGrabPoseSection(hasPermissionFilters));

            ADOverhaul.DrawFoldoutBox("Stretch & Squish", foldouts[StretchSquishFoldout], null, DrawStretchSquishSection);
            ADOverhaul.DrawFoldoutBox("Options", foldouts[OptionsFoldout], null, DrawOptionsSection);
            ADOverhaul.DrawFoldoutBox("Gizmos", foldouts[GizmosFoldout], DrawGizmoScopeToggle, DrawGizmosSection);

            ADOverhaul.ApplyModifiedProperties(serializedObject, selectedPhysBones, pb => pb.configHasUpdated = true);
            ADOverhaul.DrawToolHeader();
        }

        /// <summary>
        /// Root transform, endpoint position, multi-child handling and the ignore-transform list.
        /// </summary>
        private void DrawTransformsSection()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(rootTransform, new GUIContent("Root"));

                // Deliberately not routed through the cached property: this writes one target at a
                // time through a fresh SerializedObject, so each PhysBone in a multi-selection gets
                // its *own* transform rather than all of them getting the first one's.
                if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
                {
                    foreach (UnityEngine.Object inspected in targets)
                    {
                        VRCPhysBone physBone = inspected as VRCPhysBone;
                        if (!physBone)
                        {
                            continue;
                        }

                        SerializedObject serialized = new SerializedObject(physBone);
                        serialized.FindProperty("rootTransform").objectReferenceValue = physBone.transform;
                        serialized.ApplyModifiedProperties();
                    }
                }
            }

            isEditingEndpoints = ADOverhaul.DrawPropertyWithEditToggle(endpointPosition, isEditingEndpoints);
            EditorGUILayout.PropertyField(multiChildType);

            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(12f);
                    ignoreTransforms.isExpanded = EditorGUILayout.Foldout(
                        ignoreTransforms.isExpanded, "Ignore Transforms", toggleOnLabelClick: true);
                    GUILayout.FlexibleSpace();

                    isCopyingIgnoreTransforms = ADOverhaul.DrawIconToggle(
                        isCopyingIgnoreTransforms, ADOEditorUtility.contents.copyFromComponent);

                    EditorGUI.BeginChangeCheck();
                    isSelectingIgnoreTransforms = ADOverhaul.DrawIconToggle(
                        isSelectingIgnoreTransforms, ADOEditorUtility.contents.select);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshIgnoreTransformStates();
                    }
                }

                if (ignoreTransforms.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    ADOEditorUtility.ObjectListField<Transform>(ignoreTransforms);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>Pull, spring, stiffness, immobile and the two gravity rows.</summary>
        private void DrawForcesSection()
        {
            // Advanced integration renames spring to momentum and adds stiffness, which is inert
            // under the simple model.
            bool advanced = integrationType.enumValueIndex == 1;

            DrawBindingRow(0);
            DrawBindingRow(1, new GUIContent(advanced ? "Momentum" : "Spring", spring.tooltip));
            if (advanced)
            {
                DrawBindingRow(2);
            }

            DrawBindingRow(3);
            DrawBindingRow(4);
            DrawBindingRow(5);

            if (immobileType != null)
            {
                EditorGUILayout.PropertyField(immobileType);
            }
        }

        /// <summary>Limit type and, below it, whichever angle and rotation controls that type uses.</summary>
        private void DrawLimitsSection()
        {
            // Read before the field is drawn, so a change made this frame does not immediately
            // reveal or hide the controls under it -- the shipped behaviour.
            int limitTypeIndex = limitType.enumValueIndex;
            EditorGUILayout.PropertyField(limitType, new GUIContent("Type"));

            if (limitTypeIndex <= 0)
            {
                return;
            }

            DrawBindingRow(7);
            if (limitTypeIndex == 3)
            {
                DrawBindingRow(8);
            }

            EditorGUILayout.PropertyField(limitRotation);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Limit Rotation Curves");
                DrawCurveField(limitRotationXCurve, "X", drawResetButton: false);
                DrawCurveField(limitRotationYCurve, "Y", drawResetButton: false);
                DrawCurveField(limitRotationZCurve, "Z", drawResetButton: false);

                if (ADOEditorUtility.Button(ADOEditorUtility.contents.clear, GUI.skin.label, GUILayout.Width(14f)))
                {
                    limitRotationXCurve.animationCurveValue =
                        limitRotationYCurve.animationCurveValue =
                            limitRotationZCurve.animationCurveValue = new AnimationCurve();
                }
            }
        }

        /// <summary>Collision radius, the collision permission and the collider list.</summary>
        private void DrawCollisionsSection(bool hasPermissionFilters)
        {
            DrawBindingRow(6);

            if (hasPermissionFilters)
            {
                ADOverhaul.DrawSelfOthersToggles(allowCollision, collisionFilter);
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(12f);
                    colliders.isExpanded = EditorGUILayout.Foldout(
                        colliders.isExpanded, "Colliders", toggleOnLabelClick: true);
                    GUILayout.FlexibleSpace();

                    isCopyingColliders = ADOverhaul.DrawIconToggle(
                        isCopyingColliders, ADOEditorUtility.contents.copyFromComponent);

                    EditorGUI.BeginChangeCheck();
                    isSelectingColliders = ADOverhaul.DrawIconToggle(
                        isSelectingColliders, ADOEditorUtility.contents.select);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshColliderStates();
                    }
                }

                if (colliders.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    ADOEditorUtility.ObjectListField<VRCPhysBoneCollider>(colliders);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>The grab and pose permissions, grab movement and snap-to-hand.</summary>
        private void DrawGrabPoseSection(bool hasPermissionFilters)
        {
            if (hasPermissionFilters)
            {
                ADOverhaul.DrawSelfOthersToggles(allowGrabbing, grabFilter);
                ADOverhaul.DrawSelfOthersToggles(allowPosing, poseFilter);
            }

            EditorGUILayout.PropertyField(grabMovement);
            EditorGUILayout.PropertyField(snapToHand);
        }

        /// <summary>Max stretch and squish, plus the stretch-motion row on SDKs that have it.</summary>
        private void DrawStretchSquishSection()
        {
            DrawBindingRowIfPresent(13);
            DrawBindingRowIfPresent(14);

            if (version.enumValueIndex > 0)
            {
                DrawBindingRowIfPresent(12);
            }
        }

        /// <summary>
        /// Version, the animated and reset flags, the target avatar, the animator parameter name and
        /// -- in play mode -- the live parameter readout.
        /// </summary>
        private void DrawOptionsSection()
        {
            EditorGUILayout.PropertyField(version);
            EditorGUILayout.PropertyField(isAnimated);
            EditorGUILayout.PropertyField(resetWhenDisabled);
            ADOverhaul.DrawTargetAvatarSelector();

            using (new GUILayout.HorizontalScope())
            {
                if (ADOverhaul.selectedAvatar)
                {
                    DrawAvatarParameterNameField();
                }
                else
                {
                    // With no avatar to read parameter names off, the field degrades to a plain
                    // text property with no suggestions and no "add missing" button.
                    EditorGUILayout.PropertyField(parameter);
                }
            }

            DrawLiveParameterReadout();
        }

        /// <summary>
        /// The parameter-name field with a dropdown of names already on the avatar, and a button
        /// offering to add whichever of this PhysBone's parameters an animator is missing.
        /// </summary>
        /// <remarks>
        /// The suggestions are built by finding avatar parameters that end in one of the PhysBone
        /// suffixes and offering the *stem* -- everything before the last underscore -- because that
        /// stem is what the PhysBone's <c>parameter</c> field holds. Only three of the suffixes are
        /// probed, not the whole table; that is what the shipped build does.
        /// </remarks>
        private void DrawAvatarParameterNameField()
        {
            List<string> matches = new List<string>();
            foreach (string name in ADOverhaul.avatarParameterNames)
            {
                int suffixStart = name.LastIndexOf("_IsGrabbed", StringComparison.Ordinal);
                if (suffixStart < 0)
                {
                    suffixStart = name.LastIndexOf("_Angle", StringComparison.Ordinal);
                }

                if (suffixStart < 0)
                {
                    suffixStart = name.LastIndexOf("_Stretch", StringComparison.Ordinal);
                }

                if (suffixStart >= 0)
                {
                    matches.Add(name);
                }
            }

            // Note the stem is cut at the *last* underscore rather than at suffixStart, so a
            // parameter named "tail_Angle" suggests "tail" either way, but one named
            // "tail_Angle_extra" suggests "tail_Angle". Ported as the original has it.
            string[] suggestions = matches
                .Select(name => name.Substring(0, name.LastIndexOf('_')))
                .Distinct()
                .ToArray();

            string value = parameter.stringValue;
            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                value = ADOEditorUtility.TextFieldDropDown("Parameter", value, suggestions);
                if (changeCheck.changed)
                {
                    parameter.stringValue = value;
                }
            }

            using (new EditorGUI.DisabledScope(
                ADOverhaul.selectedAvatar == null || string.IsNullOrEmpty(parameter.stringValue)))
            {
                if (!ADOEditorUtility.IconButton(ADOEditorUtility.contents.create))
                {
                    return;
                }

                GenericMenu menu = new GenericMenu();

                foreach (VRCAvatarDescriptor.CustomAnimLayer layer in
                    ADOverhaul.selectedAvatar.baseAnimationLayers.Concat(ADOverhaul.selectedAvatar.specialAnimationLayers))
                {
                    AnimatorController controller = layer.animatorController as AnimatorController;
                    if (controller == null)
                    {
                        continue;
                    }

                    AnimatorControllerParameter[] existing = controller.parameters;

                    foreach (PhysBoneParameter physBoneParameter in ADOEditorUtility.physBoneParameters)
                    {
                        string parameterName = parameter.stringValue + physBoneParameter.suffix;

                        // Only the missing ones are offered; a parameter already on the controller
                        // simply does not appear in the menu.
                        if (existing.Any(p => p.name == parameterName))
                        {
                            continue;
                        }

                        menu.AddItem(new GUIContent($"{layer.type}/{parameterName}"), false, () =>
                        {
                            controller.AddParameterIfMissing(parameterName, physBoneParameter.parameterType, 0f);
                            ADOverhaul.Log($"Added {parameterName} to {layer.type} ({controller.name})");
                            ADOverhaul.RefreshAvatarParameterNames();
                        });
                    }
                }

                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// A live column per readable PhysBone parameter, shown only while playing and only for a
        /// single PhysBone that actually drives parameters.
        /// </summary>
        private void DrawLiveParameterReadout()
        {
            VRCPhysBone physBone = TargetObject() as VRCPhysBone;
            if (physBone == null
                || !Application.isPlaying
                || serializedObject.isEditingMultipleObjects
                || string.IsNullOrEmpty(physBone.parameter))
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayoutUtils.BeginHorizontalSplit(parameterReadoutSplitState);

                foreach (PhysBoneParameter physBoneParameter in
                    ADOEditorUtility.physBoneParameters.Where(p => p.hasBackingField))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label(physBoneParameter.suffix, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                        GUILayoutUtils.DrawUnderline();
                        GUILayout.Label(physBoneParameter.GetValueString(physBone));
                    }

                    GUILayoutUtils.DrawVerticalSeparator();
                }

                GUILayoutUtils.EndSplit();
            }
        }

        /// <summary>
        /// The gizmo section header: the per-component "Show Gizmos" toggle and the global/local
        /// scope switch beside it.
        /// </summary>
        private void DrawGizmoScopeToggle()
        {
            ADOverhaul.DrawPropertyToggleButton(showGizmos, "Show Gizmos", () =>
            {
                // While the global setting owns gizmo visibility, toggling it on one component is
                // taken as a request to change it everywhere.
                if (ADOSettings.instance.globalGizmo)
                {
                    ADOSettings.instance.gizmosActive.value = showGizmos.boolValue;
                }
            }, GUILayout.ExpandWidth(false));

            bool global = ADOSettings.instance.globalGizmo;
            string label = global ? "Global Setting" : "Local Setting";

            using (new GUIColorScope(GUIColorScope.ColoringType.BG, global,
                ADOEditorUtility.validColor, ADOEditorUtility.warningColor))
            {
                using (new SettingsChangeScope(ApplyGlobalGizmoSettings))
                {
                    ADOSettings.instance.globalGizmo.value =
                        GUILayout.Toggle(global, label, GUI.skin.button, GUILayout.ExpandWidth(false));
                }
            }
        }

        /// <summary>
        /// The two opacity sliders, reading and writing either the global settings or this
        /// component's own properties depending on which owns gizmos.
        /// </summary>
        /// <remarks>
        /// The global branch is deliberately not wrapped in a <see cref="SettingsChangeScope"/>: the
        /// two settings persist themselves on assignment. Only the scope switch above needs one,
        /// because it also has to re-apply the settings to every PhysBone in the scene.
        /// </remarks>
        private void DrawGizmosSection()
        {
            if (ADOSettings.instance.globalGizmo)
            {
                ADOSettings.instance.gizmoBoneOpacity.Value =
                    EditorGUILayout.Slider("Bone Opacity", ADOSettings.instance.gizmoBoneOpacity, 0f, 1f);
                ADOSettings.instance.gizmoLimitOpacity.Value =
                    EditorGUILayout.Slider("Limit Opacitiy", ADOSettings.instance.gizmoLimitOpacity, 0f, 1f);
            }
            else
            {
                boneOpacity.floatValue = EditorGUILayout.Slider("Bone Opacity", boneOpacity.floatValue, 0f, 1f);
                limitOpacity.floatValue = EditorGUILayout.Slider("Limit Opacitiy", limitOpacity.floatValue, 0f, 1f);
            }
        }

        /// <summary>
        /// The "Advanced" button in the Forces header, which switches the integration type between
        /// simple (0) and advanced (1).
        /// </summary>
        /// <remarks>
        /// A multi-selection that disagrees tints the button with the third (mixed) colour but still
        /// draws it as whichever state the property happens to report, since a two-state button has
        /// no way to show a third.
        /// </remarks>
        private void DrawIntegrationTypeToggle()
        {
            bool advanced = integrationType.enumValueIndex == 1;
            int tintIndex = integrationType.hasMultipleDifferentValues ? 2 : integrationType.enumValueIndex;

            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, tintIndex, ADOEditorUtility.styles.toggleStateColors))
                {
                    advanced = GUILayout.Toggle(advanced, "Advanced", GUI.skin.button, GUILayout.ExpandWidth(false));
                }

                if (changeCheck.changed)
                {
                    integrationType.enumValueIndex = advanced ? 1 : 0;
                }
            }
        }

        /// <summary>
        /// One row of the layout: a value property, its curve beside it, and the button that arms
        /// the scene-view editor for that property.
        /// </summary>
        /// <param name="bindingIndex">Position in <see cref="bindings"/>.</param>
        /// <param name="label">Overrides the property's own display name and tooltip.</param>
        private static void DrawBindingRow(int bindingIndex, GUIContent label = null)
        {
            PropertyBinding binding = bindings[bindingIndex];

            using (new GUILayout.HorizontalScope())
            {
                if (label != null)
                {
                    EditorGUILayout.PropertyField(binding.valueProperty, label);
                }
                else
                {
                    EditorGUILayout.PropertyField(binding.valueProperty);
                }

                DrawCurveField(binding.curveProperty, string.Empty);

                using (new GUIColorScope(GUIColorScope.ColoringType.BG,
                    isEditingProperty && activeBinding == binding,
                    ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
                {
                    if (ADOEditorUtility.Button(ADOEditorUtility.contents.edit,
                        ADOEditorUtility.styles.compactIconButton, GUILayout.ExpandWidth(false)))
                    {
                        SetPropertyEditTarget(bindingIndex);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="DrawBindingRow"/> for a binding whose value property may be absent on the
        /// installed SDK, drawing nothing when it is.
        /// </summary>
        private static void DrawBindingRowIfPresent(int bindingIndex)
        {
            if (bindings[bindingIndex].hasValue)
            {
                DrawBindingRow(bindingIndex);
            }
        }

        /// <summary>
        /// A compact curve field, hatched over while the curve is flat, optionally preceded by a
        /// label and followed by a reset button.
        /// </summary>
        /// <param name="label">Drawn before the field; skipped when blank.</param>
        /// <param name="drawResetButton">
        /// Whether to draw the trailing "X". The three limit-rotation curves are drawn without one
        /// and share a single reset button instead.
        /// </param>
        /// <remarks>
        /// A curve with fewer than two keys does not modulate anything, so the field is overdrawn
        /// with slashes rather than showing a flat line the user might read as meaningful. The
        /// hatching is a label over the field's own rect, so the field underneath stays clickable.
        /// </remarks>
        private static void DrawCurveField(SerializedProperty curveProperty, string label, bool drawResetButton = true)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
            }

            EditorGUILayout.CurveField(curveProperty, Color.cyan, new Rect(0f, 0f, 1f, 1f),
                GUIContent.none, GUILayout.MaxWidth(85f));

            if (curveProperty.animationCurveValue == null || curveProperty.animationCurveValue.length < 2)
            {
                GUI.Label(GUILayoutUtility.GetLastRect(), "///////////////////////////////",
                    ADOEditorUtility.styles.noteLeft);
            }

            if (drawResetButton
                && ADOEditorUtility.Button(ADOEditorUtility.contents.clear, GUI.skin.label, GUILayout.Width(14f)))
            {
                curveProperty.animationCurveValue = new AnimationCurve();
            }
        }

        /// <summary>
        /// The inspector's keyboard shortcuts: Enter or Escape leaves whichever tool is armed, and
        /// Ctrl+E / Ctrl+T arm the property editor and test mode when none is.
        /// </summary>
        /// <remarks>
        /// Ctrl+E additionally stops test mode when arming the property editor, because the two edit
        /// different objects -- the tool would otherwise be editing the original while the user
        /// watches the clone.
        /// </remarks>
        private static void HandleShortcuts()
        {
            Event current = Event.current;
            if (current.type == EventType.Used || current.type != EventType.KeyDown)
            {
                return;
            }

            KeyCode key = current.keyCode;

            if (toolModes.activeIndex >= 0)
            {
                if (key == KeyCode.Return || key == KeyCode.KeypadEnter || key == KeyCode.Escape)
                {
                    ExitTool();
                    current.Use();
                }

                return;
            }

            if (!current.control)
            {
                return;
            }

            switch (key)
            {
                case KeyCode.E:
                    if (!isEditingProperty)
                    {
                        SetPropertyEditTarget(0);
                    }
                    else
                    {
                        ExitTool();
                    }

                    if (ADOverhaul.isTesting)
                    {
                        ADOverhaul.ToggleTestMode();
                    }

                    current.Use();
                    break;

                case KeyCode.T:
                    ADOverhaul.ToggleTestMode();
                    current.Use();
                    break;
            }
        }
    }
}
