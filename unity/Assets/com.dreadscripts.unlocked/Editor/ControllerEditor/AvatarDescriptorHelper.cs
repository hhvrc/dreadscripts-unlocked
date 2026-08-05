// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/AvatarDescriptorHelper.cs
//   Avatar() / Avatar(VRCAvatarDescriptor) -> Avatar, line 23
// The get/set pair above carries [SpecialName] in the decompiled source, i.e. it is the accessor
// pair of a property ILSpy could not recombine; it is restored as a property here. Line numbers are
// relative to the decompiled snapshot at the time of the port; the member names are the durable
// reference.
//
// VRChat SDK dependency: this file is written against com.vrchat.avatars (a hard vpmDependency of
// the package) and uses VRCAvatarDescriptor, its baseAnimationLayers array of
// VRCAvatarDescriptor.CustomAnimLayer, and VRCAvatarDescriptor.AnimLayerType.Action. There is no
// graceful degradation and none in the original: without the SDK present the file does not compile,
// exactly as for the other SDK-touching types in this folder (PhysBoneChainData,
// PhysBoneColliderSnapshot, MenuControlTreeItem). The shipped assembly likewise referenced the SDK
// unconditionally, so a tool built on this helper simply is not usable without it.
//
// Audit status: VERIFIED -- all five static fields and their initialisers, the Avatar property and
// all eight methods were diffed statement by statement against export/, including every default
// parameter value, the "Avatar"/"The Targeted VRCAvatar" strings, both HelpBox messages, the
// baseAnimationLayers[3]/[4] comparison and the length > 3 guards. Where the decompiled source
// nests the conditions (RefreshIssues, DrawWarnings, DrawActionLayerWarning) the port uses early
// returns; each was checked to be the same predicate. The MAP line number was re-checked against
// the current export/ snapshot and still lands on the Avatar getter.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Shared avatar selection for the editor windows: which descriptors are in the scene, which the
    /// user is working on, and the standard popup and warning boxes for both.
    /// </summary>
    /// <remarks>
    /// The state is static and array-shaped so that every window agrees on the current avatar, and so
    /// that a tool comparing two avatars can enlarge the arrays and address the slots by index. The
    /// three arrays are parallel — enlarging one without the others will throw on the next refresh.
    /// </remarks>
    internal static class AvatarDescriptorHelper
    {
        /// <summary>The avatar occupying each selection slot. Slot 0 is the one single-avatar tools use.</summary>
        internal static VRCAvatarDescriptor[] selectedAvatars = new VRCAvatarDescriptor[1];

        /// <summary>Per slot, the result of the last <see cref="RefreshIssues"/> humanoid check.</summary>
        internal static bool[] isHumanoid = new bool[1];

        /// <summary>Per slot, the result of the last <see cref="RefreshIssues"/> Action-layer check.</summary>
        internal static bool[] hasActionLayerBug = new bool[1];

        /// <summary>Every descriptor found in the scene by the last <see cref="RefreshAvatars"/>; null until then.</summary>
        internal static VRCAvatarDescriptor[] sceneAvatars;

        /// <summary>Raised with the slot index whenever a slot's avatar changes.</summary>
        internal static Action<int> onAvatarChanged;

        /// <summary>The avatar in slot 0.</summary>
        public static VRCAvatarDescriptor Avatar
        {
            get { return selectedAvatars[0]; }
            set { selectedAvatars[0] = value; }
        }

        /// <summary>
        /// Rescans the scene and fills any empty selection slot.
        /// </summary>
        /// <param name="preferred">
        /// Picks the avatar a caller would rather have — the one it was last used on, say. Slots fall
        /// back to any unselected scene avatar when this finds nothing.
        /// </param>
        /// <param name="onChanged">Runs once, before <see cref="onAvatarChanged"/>, if any slot was filled.</param>
        /// <remarks>
        /// Deactivated avatars are cleared first: hiding an avatar in the hierarchy is how users
        /// switch between them, and a hidden one should release its slot to whatever is now visible.
        /// Note that <see cref="onAvatarChanged"/> is raised for slot 0 regardless of which slot
        /// actually changed, which is how the decompiled source has it.
        /// </remarks>
        public static void RefreshAvatars(Func<VRCAvatarDescriptor, bool> preferred = null, Action onChanged = null)
        {
            for (int i = 0; i < selectedAvatars.Length; i++)
            {
                VRCAvatarDescriptor selected = selectedAvatars[i];
                if (selected != null && !selected.gameObject.activeInHierarchy)
                {
                    selectedAvatars[i] = null;
                }
            }

            bool anyAssigned = false;
            sceneAvatars = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
            if (sceneAvatars.Length == 0)
            {
                return;
            }

            for (int i = 0; i < selectedAvatars.Length; i++)
            {
                if (selectedAvatars[i] != null)
                {
                    continue;
                }

                if (preferred != null)
                {
                    selectedAvatars[i] = sceneAvatars.FirstOrDefault(preferred);
                    anyAssigned |= (bool)selectedAvatars[i];
                }

                if (!selectedAvatars[i])
                {
                    // Skip avatars already held by another slot, so two slots never show the same one.
                    selectedAvatars[i] = sceneAvatars.FirstOrDefault(a => !selectedAvatars.Contains(a));
                    anyAssigned |= (bool)selectedAvatars[i];
                }
            }

            if (anyAssigned)
            {
                onChanged?.Invoke();
                onAvatarChanged?.Invoke(0);
            }
        }

        /// <summary>
        /// Re-tests the selected avatar for the problems this helper knows how to report, and returns
        /// whether any of the enabled checks found one.
        /// </summary>
        /// <param name="index">Selection slot to test.</param>
        /// <param name="checkHumanoid">Whether a non-humanoid descriptor counts as an issue.</param>
        /// <param name="checkActionLayer">Whether the Action-layer bug counts as an issue.</param>
        /// <remarks>
        /// Both flags cache their result into <see cref="isHumanoid"/> and
        /// <see cref="hasActionLayerBug"/> regardless of whether the caller asked about them, so the
        /// warning drawers can rely on the values without repeating the work each repaint.
        /// </remarks>
        public static bool RefreshIssues(int index = 0, bool checkHumanoid = true, bool checkActionLayer = true)
        {
            if (!selectedAvatars[index])
            {
                return false;
            }

            isHumanoid[index] = selectedAvatars[index].IsHumanoid();

            // Layers 3 and 4 are Action and FX; equal types mean the Action slot was mislabelled.
            // The length guard only covers index 3, so a descriptor with exactly four base layers
            // would throw here. That is how the decompiled source reads and it is ported literally;
            // in practice VRChat always writes five.
            hasActionLayerBug[index] = selectedAvatars[index].baseAnimationLayers.Length > 3 &&
                                       selectedAvatars[index].baseAnimationLayers[3].type == selectedAvatars[index].baseAnimationLayers[4].type;

            if (checkActionLayer && hasActionLayerBug[index])
            {
                return true;
            }

            return checkHumanoid && !isHumanoid[index];
        }

        /// <summary>
        /// Draws the avatar popup followed by any warnings for the avatar it selects, and returns
        /// whether a blocking warning was shown — a caller should stop drawing its own UI when so.
        /// </summary>
        public static bool DrawAvatarSelector(int index = 0, bool checkHumanoid = true, bool checkActionLayer = true, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action onChanged = null)
        {
            selectedAvatars[index] = DrawAvatarPopup(index, label, tooltip, onChanged);
            if ((bool)selectedAvatars[index])
            {
                return DrawWarnings(index, checkHumanoid, checkActionLayer);
            }

            return false;
        }

        /// <summary>
        /// Draws the scene-avatar dropdown for one slot and returns the avatar it now holds.
        /// </summary>
        /// <remarks>
        /// Selecting an avatar pings it in the hierarchy, which is the quickest confirmation that the
        /// right one was picked when several share a name.
        /// </remarks>
        public static VRCAvatarDescriptor DrawAvatarPopup(int index = 0, string label = "Avatar", string tooltip = "The Targeted VRCAvatar", Action onChanged = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIContent content = new GUIContent(label, tooltip);
                if (sceneAvatars != null && sceneAvatars.Length != 0)
                {
                    using (EditorGUI.ChangeCheckScope changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        // The names are filtered but the index is taken against, and applied back to,
                        // the unfiltered array; a destroyed entry in sceneAvatars would therefore
                        // shift the selection. Ported as-is — RefreshAvatars normally keeps the array
                        // free of nulls.
                        int selected = EditorGUILayout.Popup(
                            content,
                            selectedAvatars[index] ? Array.IndexOf(sceneAvatars, selectedAvatars[index]) : -1,
                            sceneAvatars.Where(a => a).Select(a => a.name).ToArray());

                        if (changeCheck.changed)
                        {
                            selectedAvatars[index] = sceneAvatars[selected];
                            EditorGUIUtility.PingObject(selectedAvatars[index]);
                            onChanged?.Invoke();
                            onAvatarChanged?.Invoke(index);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(content, new GUIContent("No Avatar Descriptors Found"));
                }
            }

            return selectedAvatars[index];
        }

        /// <summary>
        /// Draws whichever warning applies, and returns whether one was drawn.
        /// </summary>
        /// <remarks>
        /// At most one warning is shown: the Action-layer bug takes precedence and suppresses the
        /// humanoid warning, since a mislabelled layer is the more specific and more actionable of
        /// the two.
        /// </remarks>
        private static bool DrawWarnings(int index = 0, bool checkHumanoid = true, bool checkActionLayer = true)
        {
            if (checkActionLayer && DrawActionLayerWarning(index))
            {
                return true;
            }

            return checkHumanoid && DrawHumanoidWarning(index);
        }

        /// <summary>
        /// Reports the descriptor bug where the Action playable layer is typed as FX, and offers a
        /// one-click fix.
        /// </summary>
        /// <remarks>
        /// Recomputed here rather than read from <see cref="hasActionLayerBug"/> so the box disappears
        /// on the repaint after the fix, without waiting for a <see cref="RefreshIssues"/>. Carries the
        /// same index-4 read past the length guard as <see cref="RefreshIssues"/>.
        /// </remarks>
        private static bool DrawActionLayerWarning(int index = 0)
        {
            VRCAvatarDescriptor avatar = selectedAvatars[index];
            if (!avatar)
            {
                return false;
            }

            VRCAvatarDescriptor.CustomAnimLayer[] baseAnimationLayers = avatar.baseAnimationLayers;
            if (baseAnimationLayers.Length <= 3)
            {
                return false;
            }

            if (baseAnimationLayers[3].type != baseAnimationLayers[4].type)
            {
                return false;
            }

            EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
            if (EditorUtils.Button("Fix"))
            {
                avatar.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
                EditorUtility.SetDirty(avatar);
            }

            return true;
        }

        private static bool DrawHumanoidWarning(int index = 0)
        {
            if (!selectedAvatars[index])
            {
                return false;
            }

            if (!isHumanoid[index])
            {
                EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether the descriptor was built for a humanoid rig.
        /// </summary>
        /// <remarks>
        /// Read off the number of base playable layers rather than the animator's avatar: the SDK
        /// gives a humanoid descriptor five base layers (Base, Additive, Gesture, Action, FX) and a
        /// generic one only three, and the descriptor is what the rest of this helper works from.
        /// </remarks>
        public static bool IsHumanoid(this VRCAvatarDescriptor avatar)
        {
            return avatar.baseAnimationLayers.Length > 3;
        }
    }
}
