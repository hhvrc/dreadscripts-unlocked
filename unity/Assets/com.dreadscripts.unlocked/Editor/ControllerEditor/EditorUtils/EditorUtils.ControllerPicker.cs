// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   enum   PathOption       -> PathOption,       line 1287
//   enum   PlaneAxis        -> PlaneAxis,        line 1294
//   struct ControllerPicker -> ControllerPicker, line 1302
//     IsValid()             -> IsValid (property; [SpecialName] in the decompilation)
//     Set(descriptor, layerType, allowNull)             -> Set, line 1324
//     Set(descriptor, layerType, controller, allowNull) -> Set, line 1329
//     Set(controller, allowNull)                        -> Set, line 1339
//     Process()                                         -> Process, line 1347
//     Draw(onSelected, label)                           -> Draw, line 1363
//     OnControllerSelected(...)                         -> OnControllerSelected, line 1385
//     OnControllerCreated(controller)                   -> OnControllerCreated, line 1400
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The three members that the first pass on this file left out are ported now that what they call
// exists. They reach the rest of EditorUtils as:
//   ChangeList, line 7647 -> GetPlayableLayerController, EditorUtils.AvatarDescriptor.cs
//   SortList,   line 7656 -> SetPlayableLayerController, EditorUtils.AvatarDescriptor.cs
//   PopRules,   line 4302 -> AssetField<T>,              EditorUtils.Fields.cs
//   CallRules,  line 4427 -> IsMissing,                  EditorUtils.Fields.cs
// The type is complete apart from the note below.
//
// Deliberately unported: the AssetCandidate / SelectCandidate() pair, line 1316 and 1406. That is
// an obfuscator-injected null check on an always-null static with no callers, the same shape as
// LoginCandidate/PublishCandidate on the neighbouring types.

using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// How a string handed to the asset-path sanitiser should be read.
        /// </summary>
        /// <remarks>
        /// <see cref="Normal"/> guesses from the string itself -- an extension means a file, no
        /// extension means a folder -- which is right for user-typed paths but wrong for the two
        /// cases the caller already knows the answer to: a folder whose name happens to contain a
        /// dot, and a file name that has not been given its extension yet. The two Force values
        /// exist to override the guess in those cases.
        /// </remarks>
        internal enum PathOption
        {
            /// <summary>Infer file or folder from whether the path has an extension.</summary>
            Normal,

            /// <summary>Treat the whole string as a directory path, extension or not.</summary>
            ForceFolder,

            /// <summary>Treat the whole string as a bare file name, extension or not.</summary>
            ForceFile
        }

        /// <summary>
        /// A coordinate plane, naming the plane a transform is mirrored across.
        /// </summary>
        /// <remarks>
        /// The plane is identified by the two axes lying in it, so the reflection normal is the
        /// third: YZ reflects about X, XZ about Y, XY about Z. <see cref="None"/> means "leave the
        /// rotation alone" rather than naming a plane.
        /// </remarks>
        internal enum PlaneAxis
        {
            None,
            YZ,
            XZ,
            XY
        }

        /// <summary>
        /// The state behind a "pick an animator controller" row: what is selected, whether it came
        /// from an avatar's playable layer, and whether that selection is currently usable.
        /// </summary>
        /// <remarks>
        /// This is a struct and its mutating methods return a copy, so callers must assign the
        /// result back -- <c>picker = picker.Set(...).Process();</c> -- rather than relying on the
        /// call to have changed the value in place.
        /// </remarks>
        internal struct ControllerPicker
        {
            /// <summary>The avatar the controller is expected to belong to; unused when
            /// <see cref="useAvatarLayer"/> is false.</summary>
            internal VRCAvatarDescriptor avatar;

            /// <summary>Which of the avatar's playable layers this picker stands for.</summary>
            internal VRCAvatarDescriptor.AnimLayerType layerType;

            /// <summary>
            /// Whether the controller is sourced from <see cref="avatar"/>'s
            /// <see cref="layerType"/> layer. When false the picker is a plain controller field and
            /// <see cref="avatar"/> and <see cref="layerType"/> carry no meaning.
            /// </summary>
            internal bool useAvatarLayer;

            internal AnimatorController controller;

            /// <summary>The result of the last <see cref="Process"/> call.</summary>
            internal ValidationResult validation;

            /// <summary>Whether an empty selection is an acceptable answer rather than an error.</summary>
            internal bool allowNull;

            /// <summary>Whether the last <see cref="Process"/> call accepted the current state.</summary>
            internal bool IsValid
            {
                get
                {
                    return validation.isValid;
                }
            }

            /// <summary>
            /// Points the picker at whatever controller <paramref name="avatar"/> currently has on
            /// its <paramref name="layerType"/> layer.
            /// </summary>
            /// <remarks>
            /// A null avatar is not an error here: the picker is still put into avatar-layer mode
            /// with an empty controller, and <see cref="Process"/> is left to report the missing
            /// avatar. That lets a window bind its pickers before an avatar has been chosen.
            /// </remarks>
            internal ControllerPicker Set(VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType,
                bool allowNull = false)
            {
                return Set(avatar, layerType, avatar != null ? avatar.GetPlayableLayerController(layerType) : null, allowNull);
            }

            /// <summary>
            /// Points the picker at a controller belonging to <paramref name="avatar"/>'s
            /// <paramref name="layerType"/> layer.
            /// </summary>
            /// <remarks>
            /// The caller passes the controller in rather than the picker looking it up, so that a
            /// selection the user has made but not yet written back to the descriptor can be shown.
            /// </remarks>
            internal ControllerPicker Set(VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType,
                AnimatorController controller, bool allowNull = false)
            {
                validation = new ValidationResult(false, "Unknown Error");
                this.avatar = avatar;
                this.layerType = layerType;
                useAvatarLayer = true;
                this.allowNull = allowNull;
                return Set(controller, allowNull);
            }

            /// <summary>
            /// Points the picker at a standalone controller, with no avatar layer behind it.
            /// </summary>
            internal ControllerPicker Set(AnimatorController controller, bool allowNull = false)
            {
                validation = new ValidationResult(false, "Unknown Error");
                this.allowNull = allowNull;
                this.controller = controller;
                return this;
            }

            /// <summary>
            /// Re-evaluates <see cref="validation"/> against the current selection.
            /// </summary>
            /// <remarks>
            /// A missing avatar is reported before a missing controller, because with no avatar
            /// there is no layer to have found a controller on and "controller not found" would
            /// only send the user looking in the wrong place.
            /// </remarks>
            internal ControllerPicker Process()
            {
                if (useAvatarLayer && avatar == null)
                {
                    validation = new ValidationResult(false, "Avatar is not set (Null)");
                    return this;
                }

                if (controller == null && !allowNull)
                {
                    // Phrased differently for the two modes: on an avatar layer the controller was
                    // looked up and came back empty, whereas a standalone field was simply never
                    // filled in.
                    validation = useAvatarLayer
                        ? new ValidationResult(false, $"{layerType} Controller was not found")
                        : new ValidationResult(false, "Controller is not set (Null)");
                    return this;
                }

                validation = new ValidationResult(true, "Check is valid");
                return this;
            }

            /// <summary>
            /// Draws the picker as a labelled asset row.
            /// </summary>
            /// <param name="onSelected">
            /// Raised with the asset the user picked, or with null when the field is cleared. The
            /// picker owns no storage of its own -- it never writes the pick back into
            /// <see cref="controller"/> -- so the caller must apply the value and re-<see cref="Set"/>
            /// the picker for the next frame to show it.
            /// </param>
            /// <param name="label">
            /// Overrides the generated row label, which is otherwise the layer's name in avatar-layer
            /// mode and "Target Controller" outside it.
            /// </param>
            /// <remarks>
            /// The text drawn inside the field is not the asset's name whenever something more useful
            /// can be said: an empty field says so, a reference to a deleted asset says so, and a
            /// controller that is still the avatar's own layer controller is shown as
            /// "[Avatar's FX]" rather than by name -- bracketed, because it names where the controller
            /// came from rather than what it is called. Once the user picks something else, that stops
            /// matching the descriptor and the row falls back to the asset name, which is the visible
            /// signal that the selection has diverged from the avatar.
            /// </remarks>
            internal void Draw(Action<AnimatorController> onSelected, string label = null)
            {
                bool avatarIsMissing = avatar == null;
                VRCAvatarDescriptor.AnimLayerType layerType = this.layerType;
                string layerName = layerType.ToString();

                AnimatorController assignedController = avatarIsMissing ? null : avatar.GetPlayableLayerController(layerType);
                bool isAvatarsController = useAvatarLayer && !avatarIsMissing && assignedController == controller;

                if (label == null)
                {
                    label = useAvatarLayer ? "Target " + layerName + ":" : "Target Controller";
                }

                string valueText;
                if (controller.IsMissing(out bool isDestroyed))
                {
                    valueText = !isDestroyed
                        ? "No Controller Selected"
                        : isAvatarsController
                            ? "[Avatar's " + layerName + " Is Missing!]"
                            : "Controller Is Missing!";
                }
                else
                {
                    valueText = isAvatarsController ? "[Avatar's " + layerName + "]" : controller.name;
                }

                // A struct's `this` cannot be captured by a lambda, so the callback closes over a copy.
                // The copy is only ever read, so the divergence does not matter: OnControllerSelected
                // consults useAvatarLayer and avatar, and neither can change between this frame's Draw
                // and the pick it produces.
                ControllerPicker self = this;

                AssetField(label, valueText, controller, delegate(AnimatorController picked)
                {
                    self.OnControllerSelected(picked, layerType, onSelected);
                }, validation, null, OnControllerCreated, allowNull);
            }

            /// <summary>
            /// Forwards a freshly picked controller to the owner, then offers to store it on the
            /// avatar as well.
            /// </summary>
            /// <remarks>
            /// The offer is only made when the layer is currently empty, so this never proposes to
            /// overwrite a controller the avatar already has -- the case where the user picking a
            /// different controller for the tool most likely did not mean to re-rig the avatar.
            ///
            /// Accepting it calls <see cref="SetPlayableLayerController"/>, which writes the descriptor
            /// and marks it dirty <em>without</em> registering an <see cref="Undo"/>: the assignment
            /// cannot be taken back with Ctrl+Z. That is the original behaviour and is left as-is.
            ///
            /// The menu is a context menu rather than a dialog because it must not block: this runs
            /// from inside the object picker's selection callback, mid-OnGUI.
            /// </remarks>
            private void OnControllerSelected(AnimatorController controller, VRCAvatarDescriptor.AnimLayerType layerType,
                Action<AnimatorController> onSelected)
            {
                onSelected(controller);

                if (useAvatarLayer && avatar != null && avatar.GetPlayableLayerController(layerType) == null)
                {
                    // Copied out of the struct for the same reason as in Draw: the menu item runs long
                    // after this call returns, so it cannot close over a field of `this`.
                    VRCAvatarDescriptor avatar = this.avatar;

                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Set As Avatar's {layerType}?/Yes"), on: false, delegate
                    {
                        avatar.SetPlayableLayerController(layerType, controller);
                    });
                    menu.ShowAsContext();
                }
            }

            /// <summary>
            /// Prepares a controller the user just created from the picker.
            /// </summary>
            /// <remarks>
            /// A freshly created <see cref="AnimatorController"/> asset has no layers at all, which
            /// no other code expects; adding the conventional "Base Layer" makes it look like one
            /// created through the Animator window.
            /// </remarks>
            private void OnControllerCreated(AnimatorController controller)
            {
                controller.AddLayer("Base Layer");
                EditorUtility.SetDirty(controller);
            }
        }
    }
}
