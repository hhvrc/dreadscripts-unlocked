// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Everything that operates on the VRCAvatarDescriptor the tool is pointed at: finding it, drawing
// the picker, warning about the two states it cannot work with, and the three lookup tables derived
// from it. Line numbers move with the snapshot; the member names are the durable reference.
//
//   m_Predicate           (line 5622) -> targetAvatar
//   _Collection           (line 5624) -> sceneAvatars
//   m_Registry            (line 5626) -> avatarParameterNames
//   m_Client              (line 5628) -> collisionTagOptions
//   _Observer             (line 5630) -> populatedLayerNames
//   m_Broadcaster         (line 5632) -> populatedLayerValues
//   PrintConfiguration    (line 6596) -> RefreshAvatars
//   InterruptConfiguration(line 6617) -> DrawAvatarField
//   ViewConfiguration     (line 6626) -> DrawAvatarPopup
//   PostConfiguration     (line 6653) -> ValidateAvatar
//   ListConfiguration     (line 6666) -> DrawPrefabWarning
//   ForgotConfiguration   (line 6684) -> DrawPlayableLayerWarning
//   LogoutConfiguration   (line 6509) -> RefreshAvatarTables
//   SetupConfiguration    (line 6524) -> RefreshAvatarParameters
//   PushConfiguration     (line 6806) -> DrawTargetAvatarField
//   PublishConfiguration  (line 6567) -> BuildLayerParameterMenu
//   CollectConfiguration  (line 6583) -> SplitDigits
//
// Three closure lambdas belong here and get no file of their own; they are inlined at their use
// sites: the `x => x.name` and the two `collisionTags` projections.
//
// DEOBF-BUG(resolved) in RefreshAvatarTables -- see the marker on the method. export/ shows the
// no-avatar branch clearing only avatarParameterNames and then falling straight through into two
// statements that dereference the null avatar. The 2019 build of the same method (VerifySystem,
// line 6491 of decompiled/ADOverhaul2019/.../ADOverhaul.cs) clears *both* tables and returns, which
// is the form reproduced here: the decompiled 2022 body would throw a NullReferenceException every
// time the scene contains no avatar descriptor, which is the ordinary case on an empty scene and
// on the path RefreshAvatars takes when it finds nothing. export/ will keep showing the falling-
// through form until de4dot changes; do not "fix" the deviation back.
//
// NO LICENCE CODE was removed from this group -- none of these members carried a gate.
//
// Audit status: VERIFIED against export -- every method re-read against lines 6509-6534, 6567-6594,
// 6596-6710 and 6806-6809 on 2026-08-04, and RefreshAvatarTables cross-checked against the 2019
// build.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>The avatar every avatar-scoped feature works against.</summary>
        private static VRCAvatarDescriptor targetAvatar;

        /// <summary>Every avatar descriptor in the open scenes, as of the last refresh.</summary>
        private static VRCAvatarDescriptor[] sceneAvatars;

        /// <summary>
        /// The animator parameters declared anywhere on <see cref="targetAvatar"/>'s playable
        /// layers, minus the ones VRChat reserves — the completion list for a contact receiver's
        /// parameter field.
        /// </summary>
        private static string[] avatarParameterNames;

        /// <summary>
        /// The collision tags offered by the tag picker: every tag already in use anywhere on the
        /// avatar, plus VRChat's built-in set under a "Default/" submenu.
        /// </summary>
        private static string[] collisionTagOptions;

        /// <summary>Display names of the playable layers the avatar actually has a controller on.</summary>
        private static string[] populatedLayerNames;

        /// <summary>
        /// <see cref="VRCAvatarDescriptor.AnimLayerType"/> values matching
        /// <see cref="populatedLayerNames"/>, as ints because that is what IntPopup takes.
        /// </summary>
        private static int[] populatedLayerValues;

        /// <summary>
        /// Rescans the open scenes for avatar descriptors and picks one if none is selected yet.
        /// </summary>
        /// <param name="onPicked">Invoked once, after a pick, whether or not one was made.</param>
        /// <param name="preferred">
        /// Chooses among the found avatars; the first is used when it matches nothing.
        /// </param>
        /// <remarks>
        /// An avatar already selected is kept even if it is no longer in the list, so that a
        /// refresh triggered by an unrelated hierarchy change cannot move the user's selection.
        /// </remarks>
        internal static void RefreshAvatars(ref VRCAvatarDescriptor avatar, ref VRCAvatarDescriptor[] all, Action onPicked = null, Func<VRCAvatarDescriptor, bool> preferred = null)
        {
            all = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
            if ((bool)(UnityEngine.Object)avatar)
            {
                return;
            }

            if (all.Length != 0)
            {
                avatar = preferred == null
                    ? all[0]
                    : all.FirstOrDefault(preferred) ?? all[0];
            }

            onPicked?.Invoke();
        }

        /// <summary>
        /// Draws the avatar picker and the warnings that go with it, and reports whether the
        /// selected avatar is usable.
        /// </summary>
        /// <returns>
        /// False when nothing is selected, or when the avatar is in a state the tool refuses to
        /// work on — which is the caller's cue to stop drawing the rest of its GUI.
        /// </returns>
        internal static bool DrawAvatarField(ref VRCAvatarDescriptor avatar, VRCAvatarDescriptor[] all, Action onPicked = null, bool drawNonHumanoidWarning = true, bool requireUnpacked = true, bool checkPlayableLayers = true, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action extraGui = null)
        {
            if (!(UnityEngine.Object)DrawAvatarPopup(ref avatar, all, onPicked, label, tooltip, extraGui))
            {
                return false;
            }

            return ValidateAvatar(avatar, drawNonHumanoidWarning, requireUnpacked, checkPlayableLayers);
        }

        /// <summary>
        /// The avatar popup itself: a labelled dropdown of every descriptor in the scene, which
        /// pings the object it selects.
        /// </summary>
        /// <param name="extraGui">Drawn inside the same row, to the right of the popup.</param>
        /// <returns>The selected avatar, which may still be null.</returns>
        /// <remarks>
        /// The entries are the avatars' names with nulls filtered out, but the index the popup
        /// returns is used against the unfiltered array. A destroyed descriptor between the two
        /// therefore shifts the selection — as shipped.
        /// </remarks>
        private static VRCAvatarDescriptor DrawAvatarPopup(ref VRCAvatarDescriptor avatar, VRCAvatarDescriptor[] all, Action onPicked = null, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action extraGui = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent content = new GUIContent(label, tooltip);
                if (all == null || all.Length == 0)
                {
                    EditorGUILayout.LabelField(content, new GUIContent("No Avatar Descriptors Found"));
                }
                else
                {
                    using EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope();

                    int index = EditorGUILayout.Popup(
                        content,
                        (bool)(UnityEngine.Object)avatar ? Array.IndexOf(all, avatar) : -1,
                        all.Where(x => (bool)(UnityEngine.Object)x).Select(x => ((UnityEngine.Object)x).name).ToArray());

                    if (changeCheck.changed)
                    {
                        avatar = all[index];
                        EditorGUIUtility.PingObject((UnityEngine.Object)avatar);
                        onPicked?.Invoke();
                    }
                }

                extraGui?.Invoke();
            }

            return avatar;
        }

        /// <summary>
        /// Draws whichever "this avatar cannot be edited" warnings apply, and reports whether the
        /// avatar came through clean.
        /// </summary>
        /// <param name="drawNonHumanoidWarning">
        /// Draws the non-humanoid help box, and — as shipped — also decides whether that state counts
        /// as a failure at all: the check returns this flag rather than a fixed true.
        /// </param>
        /// <param name="requireUnpacked">Refuses an avatar that is still part of a prefab.</param>
        /// <param name="checkPlayableLayers">Also checks the descriptor's playable layer setup.</param>
        private static bool ValidateAvatar(VRCAvatarDescriptor avatar, bool drawNonHumanoidWarning = true, bool requireUnpacked = true, bool checkPlayableLayers = true)
        {
            if (requireUnpacked && DrawPrefabWarning(avatar))
            {
                return false;
            }

            if (!checkPlayableLayers)
            {
                return true;
            }

            return !DrawPlayableLayerWarning(avatar, drawNonHumanoidWarning);
        }

        /// <summary>
        /// Warns that the avatar is still a prefab instance, with a button that unpacks it.
        /// </summary>
        /// <returns>True while the avatar is part of a prefab.</returns>
        /// <remarks>
        /// The tool edits components in place and records undo against them, which a prefab
        /// instance would either reject or turn into overrides, so this is a hard stop rather than
        /// a caution.
        /// </remarks>
        private static bool DrawPrefabWarning(VRCAvatarDescriptor avatar)
        {
            if (!(bool)(UnityEngine.Object)avatar)
            {
                return false;
            }

            bool isPrefab = PrefabUtility.IsPartOfAnyPrefab(((UnityEngine.Component)avatar).gameObject);
            if (isPrefab)
            {
                EditorGUILayout.HelpBox("Target Avatar is a part of a prefab. Prefab unpacking is required.", MessageType.Error);
                if (GUILayout.Button("Unpack"))
                {
                    PrefabUtility.UnpackPrefabInstance(((UnityEngine.Component)avatar).gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }

            return isPrefab;
        }

        /// <summary>
        /// Warns about the two playable-layer states the tool cannot work with: a descriptor set up
        /// as non-humanoid, and the SDK bug that leaves the Action layer typed as FX.
        /// </summary>
        /// <param name="drawWarning">
        /// When false the non-humanoid case is reported without drawing anything — and, as shipped,
        /// is also reported as *no* problem, since the return value is this flag.
        /// </param>
        /// <returns>True when a state the caller should stop for was found.</returns>
        private static bool DrawPlayableLayerWarning(VRCAvatarDescriptor avatar, bool drawWarning = true)
        {
            if (!(bool)(UnityEngine.Object)avatar)
            {
                return false;
            }

            VRCAvatarDescriptor.CustomAnimLayer[] baseLayers = avatar.baseAnimationLayers;

            // A humanoid descriptor has five base layers; three means the rig was imported as
            // generic, and none of the avatar-dynamics features have anywhere to write.
            if (baseLayers.Length <= 3)
            {
                if (drawWarning)
                {
                    EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
                }

                return drawWarning;
            }

            // Slots 3 and 4 are Action and FX. Both reading FX is the SDK bug this offers to fix.
            bool actionLayerIsFx = baseLayers[3].type == baseLayers[4].type;
            if (actionLayerIsFx)
            {
                EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    avatar.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
                    EditorUtility.SetDirty((UnityEngine.Object)avatar);
                }
            }

            return actionLayerIsFx;
        }

        /// <summary>
        /// The avatar picker as the inspectors draw it: bound to <see cref="targetAvatar"/>, with
        /// the prefab and playable-layer checks turned off because an inspector must keep drawing
        /// its component either way.
        /// </summary>
        internal static void DrawTargetAvatarField()
        {
            DrawAvatarField(ref targetAvatar, sceneAvatars, RefreshAvatarTables, false, false, true, "Target Avatar");
        }

        /// <summary>
        /// Rebuilds every table derived from <see cref="targetAvatar"/>: the populated playable
        /// layers, the parameter names and the collision tags.
        /// </summary>
        internal static void RefreshAvatarTables()
        {
            ADOEditorUtility.GetPopulatedPlayableLayers(targetAvatar, ref populatedLayerNames, ref populatedLayerValues);

            // DEOBF-BUG(resolved): export/ clears only avatarParameterNames here and then falls
            // through into two statements that dereference the null avatar. The 2019 build clears
            // both tables and returns, which is what is reproduced.
            if (!(bool)(UnityEngine.Object)targetAvatar)
            {
                avatarParameterNames = Array.Empty<string>();
                collisionTagOptions = Array.Empty<string>();
                return;
            }

            RefreshAvatarParameters();

            collisionTagOptions = ((UnityEngine.Component)targetAvatar).GetComponentsInChildren<VRCContactSender>().SelectMany(sender => sender.collisionTags)
                .Concat(((UnityEngine.Component)targetAvatar).GetComponentsInChildren<VRCContactReceiver>().SelectMany(receiver => receiver.collisionTags))
                .Except(ADOEditorUtility.defaultCollisionTags)
                .Concat(ADOEditorUtility.defaultCollisionTags.Select(tag => "Default/" + tag))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Collects every animator parameter declared on the avatar's non-default playable layers,
        /// dropping the ones VRChat drives itself.
        /// </summary>
        /// <remarks>
        /// Each layer's controller is re-loaded from its asset path rather than used as handed
        /// over, which is what filters out runtime-only controllers.
        /// </remarks>
        private static void RefreshAvatarParameters()
        {
            avatarParameterNames = targetAvatar.baseAnimationLayers.Concat(targetAvatar.specialAnimationLayers)
                .Where(layer => !layer.isDefault && (bool)layer.animatorController)
                .Select(layer => AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(layer.animatorController)))
                .Where(controller => controller)
                .SelectMany(controller => controller.parameters)
                .Select(parameter => parameter.name)
                .Where(name => !ADOEditorUtility.reservedAvatarParameters.Contains(name))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Builds the flattened "Layer/Type" menu the add-parameter popup shows, pairing each entry
        /// with an int that encodes both choices.
        /// </summary>
        /// <param name="paths">Receives "Gesture/Bool"-style paths, layer-major.</param>
        /// <param name="values">
        /// Receives the encoded selection for each path: the layer's value with the type index
        /// appended as a decimal digit, decoded again by <see cref="SplitDigits"/>.
        /// </param>
        /// <remarks>
        /// The encoding is decimal-positional, so it only survives because both operands are single
        /// digits in practice — a playable layer value of 10 or more would collide. That is the
        /// shipped scheme, not a reconstruction choice.
        /// </remarks>
        internal static void BuildLayerParameterMenu(string[] layerNames, int[] layerValues, string[] typeNames, out string[] paths, out int[] values)
        {
            List<string> builtPaths = new List<string>();
            List<int> builtValues = new List<int>();

            for (int layer = 0; layer < layerNames.Length; layer++)
            {
                for (int type = 0; type < typeNames.Length; type++)
                {
                    builtPaths.Add(layerNames[layer] + "/" + typeNames[type]);
                    builtValues.Add(int.Parse($"{layerValues[layer]}{type}"));
                }
            }

            paths = builtPaths.ToArray();
            values = builtValues.ToArray();
        }

        /// <summary>
        /// Splits a non-negative integer into its decimal digits, right-aligned in an array of
        /// <paramref name="length"/> and zero-padded on the left.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The inverse of the encoding <see cref="BuildLayerParameterMenu"/> applies.
        /// </para>
        /// <para>
        /// Transcribed as shipped, including the padding bug: the loop is bounded by the number of
        /// digits rather than by <paramref name="length"/>, so a value with fewer digits than
        /// <paramref name="length"/> leaves its leading slots zero and never reaches its own
        /// digits. It is only ever called with a two-digit value and a length of two, where the
        /// offset is zero and the bug cannot show.
        /// </para>
        /// </remarks>
        internal static int[] SplitDigits(int value, int length)
        {
            string text = value.ToString();
            int[] digits = new int[length];
            int offset = length - text.Length;
            int read = 0;

            for (int i = 0; i < text.Length; i++)
            {
                digits[i] = i >= offset ? text[read++] - 48 : 0;
            }

            return digits;
        }
    }
}
