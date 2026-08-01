// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the outer ADOverhaul class's avatar-selection block (decompiled 6561-6711) and the
// small GUI field helpers that follow it (6712-6871). Line numbers move with the snapshot; the
// decompiled names below are the durable reference. Field references go through the name table in
// ADOverhaul.State.cs.
//
//   -- Avatar selection --
//   PublishConfiguration   -> BuildLayerParameterOptions,    line 6567
//   CollectConfiguration   -> SplitDigits,                   line 6583
//   PrintConfiguration     -> RefreshSceneAvatars,           line 6596
//   InterruptConfiguration -> DrawAvatarSelector,            line 6617
//   ViewConfiguration      -> DrawAvatarPopup,               line 6626
//   PostConfiguration      -> DrawAvatarWarnings,            line 6653
//   ListConfiguration      -> DrawPrefabWarning,             line 6666
//   ForgotConfiguration    -> DrawPlayableLayerWarning,      line 6684
//   PushConfiguration      -> DrawTargetAvatarSelector,      line 6806
//
//   -- GUI field helpers --
//   UpdateConfiguration    -> GetMaxLossyScale,              line 6712
//   SearchConfiguration    -> DrawPropertyWithEditToggle,    line 6717
//   LoginConfiguration     -> DrawIconToggle,                line 6726
//   PatchConfiguration     -> DrawToggleButton(string, ...), line 6735
//   CheckConfiguration     -> DrawToggleButton(GUIContent, ...), line 6740
//   CallConfiguration      -> DrawPropertyToggleButton(SerializedProperty, string, ...), line 6748
//   RegisterConfiguration  -> DrawPropertyToggleButton(SerializedProperty, GUIContent, ...), line 6753
//   ChangeConfiguration    -> DrawOptionalProperty,          line 6769
//   StopConfiguration      -> DrawSelfOthersToggles,         line 6777
//
// ============================ MoveConfiguration (6561) -- NOT PORTED ============================
//
// Several already-ported files record members as "gated on ADOverhaul.MoveConfiguration"
// (PhysBoneColliderEditor.cs lines 61-67 among them). It is the licence gate, and nothing else. In
// full, both builds (2022 line 6561, 2019 `ComputeSystem` line 6545):
//
//     internal static bool MoveConfiguration()
//     {
//         NewIdentifier(isVerifyingLicense ? "Please wait for verification."
//                                          : "Please activate your license.",
//                       CustomLogType.Error, !isLicensed);
//         return isLicensed;
//     }
//
// That is: log an error to the console when `isLicensed` is false -- NewIdentifier's third argument
// is "actually emit this" -- and return `isLicensed`. `isLicensed` is only ever set true by the
// activation/verification routine that POSTs to the vendor's cloud function (see the
// [InitializeOnLoadMethod] audit in ADOverhaul.Lifecycle.cs); that endpoint is permanently gone, so
// this predicate can now only ever return false. Reintroducing it would mean every call site it
// guards is dead code plus a console error, which is the opposite of what this restoration is for.
//
// It is therefore deliberately omitted rather than deferred, and it will not appear in a later wave.
// Call sites recorded as "gated on MoveConfiguration" should be ported with the gate dropped: the
// guarded body runs unconditionally, exactly as it did for a licensed user. Each such site should
// say so in its own header, since dropping a guard is a visible behavioural change at that site and
// belongs in that file's record rather than only here.
//
// ================================= Deferred, not stubbed =======================================
//
//   PrepareConfiguration  line 6811 -> the avatar-parameter name field with the "Add" dropdown that
//                                     creates the parameter on the chosen playable layer. Every
//                                     other dependency is available -- avatarParameterNames /
//                                     avatarPlayableLayerNames / avatarPlayableLayerTypes in
//                                     ADOverhaul.State.cs, BuildLayerParameterOptions and
//                                     SplitDigits below, EditorUtils.TryGetPlayableLayerController
//                                     for the decompiled `MapVal`, and ADOEditorUtility
//                                     .TryAddParameter for `FindProcess` -- but its two user
//                                     messages go through ADOverhaul.NewIdentifier (line 7806),
//                                     which is not ported yet. It is left out rather than rewired
//                                     to a different logger.
//
//                                     Notes for whoever lands it. `PostIdentifier` (line 8425) is
//                                     not a member: it is the [CompilerGenerated] lift of a local
//                                     function, taking the two `_003C_003Ec__DisplayClass86_*`
//                                     capture structs by ref. Restore it as a local function and
//                                     drop both structs. The `while (true)` wrapping the popup, and
//                                     the `default: continue;` inside the switch, are ILSpy
//                                     artifacts: the 2019 build (`ChangeSystem`, line 6819)
//                                     decompiles the same IL as a plain
//                                     `if (EditorGUI.EndChangeCheck()) { ... }` with
//                                     `default: return;`, which settles it.
//
// BuildLayerParameterOptions and SplitDigits are ported anyway, even though PrepareConfiguration is
// their only caller in either shipped build: they are complete, self-contained and pure, and both
// are `internal` in the original.
//
// ======================= Overlap with ControllerEditor's AvatarDescriptorHelper =================
//
// ADOverhaul shipped its own avatar picker rather than reusing the one in
// Editor/ControllerEditor/AvatarDescriptorHelper.cs. The two are recognisably the same design and
// share three literal strings, but they are NOT the same code and this one is not expressible in
// terms of the other, so it is ported rather than delegated:
//
//   * this one is a ref-parameter API over the caller's own fields (selectedAvatar / sceneAvatars in
//     ADOverhaul.State.cs); AvatarDescriptorHelper owns indexed static slots and an onAvatarChanged
//     event, neither of which exists here.
//   * this one adds a prefab check (DrawPrefabWarning) with an "Unpack" button that
//     AvatarDescriptorHelper has no counterpart for.
//   * the humanoid and Action-layer warnings are one method here and two there, and the precedence
//     is inverted: AvatarDescriptorHelper reports the Action-layer bug in preference to the humanoid
//     one, whereas DrawPlayableLayerWarning below reaches the Action-layer test only on a humanoid
//     descriptor, and reports non-humanoid otherwise.
//   * the return polarity is opposite. DrawAvatarSelector returns true when the avatar is USABLE;
//     AvatarDescriptorHelper.DrawAvatarSelector returns true when a blocking warning was SHOWN.
//   * RefreshSceneAvatars does not release the slot held by a deactivated avatar, which
//     AvatarDescriptorHelper.RefreshAvatars does.
//
// SHIPPED BUGS PRESERVED (see the remarks on each member):
//   * SplitDigits loops over the digit string's length instead of the requested digit count, so any
//     value with fewer digits than requested decodes as all zeroes.
//   * DrawAvatarPopup indexes the unfiltered array with an index chosen from the filtered names.
//     AvatarDescriptorHelper carries the same one.
//   * DrawPlayableLayerWarning reads baseAnimationLayers[4] behind a `Length > 3` guard.
//     AvatarDescriptorHelper carries the same one.
//   * DrawSelfOthersToggles restores EditorGUIUtility.labelWidth to a hard-coded 160 rather than to
//     what it found.
//
// 2019 vs 2022: no behavioural divergence anywhere in this region. The same members appear under a
// different set of obfuscated names (StartSystem 6551, InitSystem 6566, CheckSystem 6578,
// CancelSystem 6598, DisableSystem 6608, IncludeSystem 6633, RateSystem 6645, ForgotSystem 6665,
// AssetSystem 6693, TestSystem 6698, ResetSystem 6710, GetSystem 6719, VisitSystem 6724,
// AwakeSystem 6733, InvokeSystem 6738, CustomizeSystem 6760, MoveSystem 6768, FillSystem 6812).
// The only textual differences are decompiler branch-ordering choices: 2019 renders
// DrawPlayableLayerWarning's humanoid test as the fallthrough rather than the early exit, and
// renders RefreshSceneAvatars' preferred-avatar ternary the other way round. Same expressions.

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOverhaul
    {
        #region Avatar selection

        /// <summary>
        /// Rescans the open scenes for avatar descriptors and, if <paramref name="avatar"/> is not
        /// set, picks one.
        /// </summary>
        /// <param name="avatar">The current selection, assigned only when it is null.</param>
        /// <param name="sceneDescriptors">Receives every descriptor found, always.</param>
        /// <param name="onChanged">Runs after a pick was attempted, to rebuild the derived tables.</param>
        /// <param name="preferred">
        /// Picks out the avatar the caller would rather have. The first scene descriptor is used
        /// when it matches nothing.
        /// </param>
        /// <remarks>
        /// <paramref name="sceneDescriptors"/> is refreshed before the early exit, so a caller that
        /// already has an avatar still gets an up-to-date list for its popup. Conversely
        /// <paramref name="onChanged"/> fires whenever the avatar was null on entry -- including when
        /// the scene holds no descriptors at all and nothing was assigned -- and never fires when it
        /// was already set. That is deliberate on the second count: the derived tables only need
        /// rebuilding when the avatar might have changed.
        /// </remarks>
        internal static void RefreshSceneAvatars(ref VRCAvatarDescriptor avatar, ref VRCAvatarDescriptor[] sceneDescriptors, Action onChanged = null, Func<VRCAvatarDescriptor, bool> preferred = null)
        {
            sceneDescriptors = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
            if (avatar)
            {
                return;
            }

            if (sceneDescriptors.Length != 0)
            {
                if (preferred == null)
                {
                    avatar = sceneDescriptors[0];
                }
                else
                {
                    avatar = sceneDescriptors.FirstOrDefault(preferred) ?? sceneDescriptors[0];
                }
            }

            onChanged?.Invoke();
        }

        /// <summary>
        /// Draws the avatar popup followed by any warning that applies to the avatar it selects, and
        /// reports whether that avatar is usable.
        /// </summary>
        /// <param name="warnNonHumanoid">
        /// Whether a non-humanoid descriptor counts as a problem. When false the warning is not drawn
        /// and the descriptor is accepted.
        /// </param>
        /// <param name="checkPrefab">Whether being part of a prefab counts as a problem.</param>
        /// <param name="checkPlayableLayers">Whether to run the playable-layer checks at all.</param>
        /// <param name="drawExtra">Drawn inside the popup's row, to the right of it.</param>
        /// <returns>
        /// True when an avatar is selected and passed every enabled check. Note that this is the
        /// opposite polarity from ControllerEditor's <c>AvatarDescriptorHelper.DrawAvatarSelector</c>,
        /// which returns whether a warning was shown.
        /// </returns>
        internal static bool DrawAvatarSelector(ref VRCAvatarDescriptor avatar, VRCAvatarDescriptor[] sceneDescriptors, Action onChanged = null, bool warnNonHumanoid = true, bool checkPrefab = true, bool checkPlayableLayers = true, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action drawExtra = null)
        {
            if (!DrawAvatarPopup(ref avatar, sceneDescriptors, onChanged, label, tooltip, drawExtra))
            {
                return false;
            }

            return DrawAvatarWarnings(avatar, warnNonHumanoid, checkPrefab, checkPlayableLayers);
        }

        /// <summary>
        /// Draws the scene-avatar dropdown and returns the avatar it now holds.
        /// </summary>
        /// <remarks>
        /// Picking an avatar pings it in the hierarchy, which is the quickest way to confirm the
        /// right one was chosen when several share a name.
        /// </remarks>
        private static VRCAvatarDescriptor DrawAvatarPopup(ref VRCAvatarDescriptor avatar, VRCAvatarDescriptor[] sceneDescriptors, Action onChanged = null, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action drawExtra = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent content = new GUIContent(label, tooltip);
                if (sceneDescriptors == null || sceneDescriptors.Length == 0)
                {
                    EditorGUILayout.LabelField(content, new GUIContent("No Avatar Descriptors Found"));
                }
                else
                {
                    using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        // Shipped bug: the displayed names are filtered for destroyed entries but the
                        // returned index is applied back to the unfiltered array, so a null anywhere
                        // in it shifts the selection. Ported as-is; the array comes straight from
                        // FindObjectsOfType and is normally free of nulls. ControllerEditor's
                        // AvatarDescriptorHelper.DrawAvatarPopup has the same bug.
                        int selected = EditorGUILayout.Popup(
                            content,
                            avatar ? Array.IndexOf(sceneDescriptors, avatar) : -1,
                            sceneDescriptors.Where(descriptor => descriptor).Select(descriptor => descriptor.name).ToArray());

                        if (changeCheck.changed)
                        {
                            avatar = sceneDescriptors[selected];
                            EditorGUIUtility.PingObject(avatar);
                            onChanged?.Invoke();
                        }
                    }
                }

                drawExtra?.Invoke();
            }

            return avatar;
        }

        /// <summary>
        /// Draws whichever warning applies to <paramref name="avatar"/> and reports whether it came
        /// through clean.
        /// </summary>
        /// <remarks>
        /// At most one warning is drawn: a prefab has to be unpacked before anything else can be
        /// judged, so that check short-circuits the playable-layer ones.
        /// </remarks>
        private static bool DrawAvatarWarnings(VRCAvatarDescriptor avatar, bool warnNonHumanoid = true, bool checkPrefab = true, bool checkPlayableLayers = true)
        {
            if (checkPrefab && DrawPrefabWarning(avatar))
            {
                return false;
            }

            if (checkPlayableLayers)
            {
                return !DrawPlayableLayerWarning(avatar, warnNonHumanoid);
            }

            return true;
        }

        /// <summary>
        /// Reports that the avatar is a prefab instance and offers to unpack it, returning whether
        /// it is one.
        /// </summary>
        /// <remarks>
        /// The tool edits components on the avatar directly; on a prefab instance those edits become
        /// overrides that the next prefab apply or revert can undo without warning, so unpacking is
        /// required rather than merely advised.
        /// </remarks>
        private static bool DrawPrefabWarning(VRCAvatarDescriptor avatar)
        {
            if (!avatar)
            {
                return false;
            }

            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(avatar.gameObject);
            if (isPrefab)
            {
                EditorGUILayout.HelpBox("Target Avatar is a part of a prefab. Prefab unpacking is required.", MessageType.Error);
                if (GUILayout.Button("Unpack"))
                {
                    PrefabUtility.UnpackPrefabInstance(avatar.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }

            return isPrefab;
        }

        /// <summary>
        /// Reports whichever playable-layer problem the descriptor has -- a non-humanoid rig, or the
        /// Action layer mistyped as FX -- and returns whether one was found.
        /// </summary>
        /// <param name="warnNonHumanoid">
        /// Whether a non-humanoid descriptor counts. It gates both the warning box and the return
        /// value, so passing false makes a non-humanoid avatar report as problem-free; that is what
        /// the tools which only care about the Action-layer bug pass.
        /// </param>
        /// <remarks>
        /// Humanoid-ness is read off the base playable layer count: the SDK gives a humanoid
        /// descriptor five (Base, Additive, Gesture, Action, FX) and a generic one three.
        /// <para>
        /// Shipped bug preserved: the guard covers index 3 but the comparison also reads index 4, so
        /// a descriptor with exactly four base layers throws here. VRChat always writes three or
        /// five, so it does not arise in practice. ControllerEditor's AvatarDescriptorHelper carries
        /// the same one.
        /// </para>
        /// </remarks>
        private static bool DrawPlayableLayerWarning(VRCAvatarDescriptor avatar, bool warnNonHumanoid = true)
        {
            if (!avatar)
            {
                return false;
            }

            VRCAvatarDescriptor.CustomAnimLayer[] baseAnimationLayers = avatar.baseAnimationLayers;
            if (baseAnimationLayers.Length <= 3)
            {
                if (warnNonHumanoid)
                {
                    EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
                }

                return warnNonHumanoid;
            }

            // Slots 3 and 4 are Action and FX; equal types mean the Action slot was mislabelled.
            bool hasActionLayerBug = baseAnimationLayers[3].type == baseAnimationLayers[4].type;
            if (hasActionLayerBug)
            {
                EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    avatar.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
                    EditorUtility.SetDirty(avatar);
                }
            }

            return hasActionLayerBug;
        }

        /// <summary>
        /// The "Target Avatar" row the inspectors draw: the shared selection, refreshing the derived
        /// parameter, tag and playable-layer tables whenever it changes.
        /// </summary>
        /// <remarks>
        /// Neither the humanoid nor the prefab check is enabled. The inspectors use the avatar only
        /// to populate dropdowns, so an avatar that would block a full tool is still perfectly
        /// serviceable here; only the Action-layer bug, which would send a parameter to the wrong
        /// controller, is worth reporting.
        /// </remarks>
        private static void DrawTargetAvatarSelector()
        {
            DrawAvatarSelector(ref selectedAvatar, sceneAvatars, RefreshAvatarTables, warnNonHumanoid: false, checkPrefab: false, checkPlayableLayers: true, "Target Avatar");
        }

        /// <summary>
        /// Builds the flattened "layer / parameter type" option list for an <c>EditorGUI.IntPopup</c>,
        /// as the cross product of the avatar's playable layers with the three animator parameter
        /// types.
        /// </summary>
        /// <param name="layerNames">Display names of the playable layers, e.g. "Gesture".</param>
        /// <param name="layerTypes">
        /// The <see cref="VRCAvatarDescriptor.AnimLayerType"/> value matching each entry of
        /// <paramref name="layerNames"/>, positionally.
        /// </param>
        /// <param name="parameterTypeNames">Parameter type names, in the order "Bool", "Int", "Float".</param>
        /// <param name="displayOptions">Receives the "Gesture/Bool" style paths the popup shows.</param>
        /// <param name="values">Receives the matching popup values.</param>
        /// <remarks>
        /// Each value is the layer type and the parameter type index written as decimal digits and
        /// parsed back as one integer, so that a single popup value carries both coordinates;
        /// <see cref="SplitDigits"/> takes them apart again. Note that this only works while both
        /// coordinates stay single-digit -- and see the caveat on <see cref="SplitDigits"/>, which
        /// mishandles the values whose leading digit is zero.
        /// </remarks>
        internal static void BuildLayerParameterOptions(string[] layerNames, int[] layerTypes, string[] parameterTypeNames, out string[] displayOptions, out int[] values)
        {
            List<string> options = new List<string>();
            List<int> optionValues = new List<int>();

            for (int layer = 0; layer < layerNames.Length; layer++)
            {
                for (int parameterType = 0; parameterType < parameterTypeNames.Length; parameterType++)
                {
                    options.Add(layerNames[layer] + "/" + parameterTypeNames[parameterType]);
                    optionValues.Add(int.Parse($"{layerTypes[layer]}{parameterType}"));
                }
            }

            displayOptions = options.ToArray();
            values = optionValues.ToArray();
        }

        /// <summary>
        /// Splits <paramref name="value"/> into its decimal digits, right-aligned in an array of
        /// <paramref name="digitCount"/> entries. The inverse of the digit packing
        /// <see cref="BuildLayerParameterOptions"/> does.
        /// </summary>
        /// <remarks>
        /// SHIPPED BUG PRESERVED. The loop runs over the length of the digit string rather than over
        /// <paramref name="digitCount"/>, while the guard inside it skips the leading positions that
        /// the shorter string does not fill. The two cancel out only when the string is exactly
        /// <paramref name="digitCount"/> long; when it is shorter, every iteration falls into the
        /// skipped range and the array comes back all zeroes with the actual digits dropped.
        /// <para>
        /// At the one call site that matters -- the parameter-type popup -- the packed value loses
        /// its leading zero to <see cref="int.Parse(string)"/> whenever the layer type is Base
        /// (<see cref="VRCAvatarDescriptor.AnimLayerType.Base"/> = 0), so "01" and "02" arrive here
        /// as 1 and 2 and both decode as {0, 0}: choosing Int or Float on the Base layer creates a
        /// Bool instead. Every other layer type is non-zero and round-trips correctly.
        /// </para>
        /// </remarks>
        internal static int[] SplitDigits(int value, int digitCount)
        {
            string digits = value.ToString();
            int[] result = new int[digitCount];
            int firstUsedIndex = digitCount - digits.Length;
            int digitIndex = 0;

            for (int i = 0; i < digits.Length; i++)
            {
                result[i] = i >= firstUsedIndex ? digits[digitIndex++] - '0' : 0;
            }

            return result;
        }

        #endregion

        #region GUI field helpers

        /// <summary>
        /// The largest of a transform's three world-space scale components.
        /// </summary>
        /// <remarks>
        /// The shape handles are drawn as spheres and capsules, which have one radius rather than
        /// three. Under a non-uniform parent scale the largest axis is the one that has to be
        /// matched, otherwise the drawn shape would sit inside the volume it represents.
        /// </remarks>
        private static float GetMaxLossyScale(Transform transform)
        {
            return Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        /// <summary>
        /// A property field with the small "edit through the scene view" toggle beside it, and
        /// returns the toggle's new state.
        /// </summary>
        private static bool DrawPropertyWithEditToggle(SerializedProperty property, bool editing)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(property);
                return DrawIconToggle(editing, ADOEditorUtility.contents.edit);
            }
        }

        /// <summary>
        /// A square 18px icon toggle, tinted green while on and red while off, for the scene-view
        /// editing switches that sit at the end of a field row.
        /// </summary>
        private static bool DrawIconToggle(bool value, GUIContent content)
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, value, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
            {
                value = ADOEditorUtility.ToggleButton(value, content, ADOEditorUtility.styles.compactIconButton, GUILayout.Width(18f), GUILayout.Height(18f));
                return value;
            }
        }

        /// <summary>
        /// A labelled toggle drawn as a button, tinted green while on and red while off, writing
        /// straight back through <paramref name="value"/>.
        /// </summary>
        private static void DrawToggleButton(string label, ref bool value, params GUILayoutOption[] options)
        {
            DrawToggleButton(new GUIContent(label), ref value, options);
        }

        /// <inheritdoc cref="DrawToggleButton(string, ref bool, GUILayoutOption[])"/>
        private static void DrawToggleButton(GUIContent content, ref bool value, params GUILayoutOption[] options)
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, value, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
            {
                value = ADOEditorUtility.ToggleButton(value, content, GUI.skin.button, options);
            }
        }

        /// <summary>
        /// <see cref="DrawToggleButton(GUIContent, ref bool, GUILayoutOption[])"/> over a boolean
        /// serialized property, with a third tint for a multi-selection that disagrees.
        /// </summary>
        /// <param name="onChanged">Runs after the property is written, only when it changed.</param>
        private static void DrawPropertyToggleButton(SerializedProperty property, string label, Action onChanged = null, params GUILayoutOption[] options)
        {
            DrawPropertyToggleButton(property, new GUIContent(label), onChanged, options);
        }

        /// <inheritdoc cref="DrawPropertyToggleButton(SerializedProperty, string, Action, GUILayoutOption[])"/>
        /// <remarks>
        /// The tint index and the drawn value are read from the property separately: the index
        /// distinguishes the mixed case, which the button itself cannot show, so the button falls
        /// back to drawing the property's own (arbitrary) value in the mixed colour.
        /// </remarks>
        private static void DrawPropertyToggleButton(SerializedProperty property, GUIContent content, Action onChanged = null, params GUILayoutOption[] options)
        {
            int tintIndex = property.hasMultipleDifferentValues ? 2 : (property.boolValue ? 1 : 0);

            using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
            {
                bool value;
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, tintIndex, ADOEditorUtility.styles.toggleStateColors))
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

        /// <summary>
        /// Draws <paramref name="property"/> if it exists, so a caller can pass the result of a
        /// <c>FindPropertyRelative</c> that may miss on an older SDK without guarding it.
        /// </summary>
        private static void DrawOptionalProperty(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        /// <summary>
        /// Draws one of VRChat's allow-self/allow-others permission pairs as two plain checkboxes,
        /// collapsing the underlying tri-state enum and its filter struct into the two answers a user
        /// actually gives.
        /// </summary>
        /// <param name="permission">
        /// The tri-state enum property: 0 = nobody, 1 = everybody, 2 = defer to
        /// <paramref name="filter"/>.
        /// </param>
        /// <param name="filter">The struct property holding <c>allowSelf</c> and <c>allowOthers</c>.</param>
        /// <remarks>
        /// Editing either checkbox forces the enum to 2 and writes both flags, because "self only" and
        /// "others only" are not expressible any other way. The mixed-value marker is shown when the
        /// enum itself disagrees across the selection, and additionally when the enum agrees on 2 but
        /// the flag behind it does not -- a flag under a 0 or 1 enum is not being read, so a
        /// disagreement there is invisible to the user and is not reported.
        /// <para>
        /// Shipped quirk preserved: <see cref="EditorGUIUtility.labelWidth"/> is narrowed to 50 for
        /// the two checkboxes and then set to a hard-coded 160 rather than restored to whatever it
        /// was. 160 is the width the tool's inspectors use, so this reads correctly there and would
        /// silently widen the labels of anything else drawing afterwards.
        /// </para>
        /// </remarks>
        private static void DrawSelfOthersToggles(SerializedProperty permission, SerializedProperty filter)
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

        #endregion
    }
}
