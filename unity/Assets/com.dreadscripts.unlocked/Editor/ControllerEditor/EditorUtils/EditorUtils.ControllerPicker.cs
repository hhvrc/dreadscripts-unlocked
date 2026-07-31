// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   enum   PathOption       -> PathOption,       line 1287
//   enum   PlaneAxis        -> PlaneAxis,        line 1294
//   struct ControllerPicker -> ControllerPicker, line 1302
//     IsValid()             -> IsValid (property; [SpecialName] in the decompilation)
//     Set(descriptor, layerType, controller, allowNull) -> Set, line 1329
//     Set(controller, allowNull)                        -> Set, line 1339
//     Process()                                         -> Process, line 1347
//     OnControllerCreated(controller)                   -> OnControllerCreated, line 1400
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Deliberately unported, each because it calls an EditorUtils member that has not been ported yet:
//   Set(descriptor, layerType, allowNull), line 1324 -- needs the VRCAvatarDescriptor ->
//     AnimatorController lookup (decompiled ChangeList, line 7647).
//   Draw(onSelected, label), line 1363 -- needs the shared object-picker row (decompiled PopRules /
//     ComputeRules, line 4302) and the destroyed-object test (decompiled CallRules, line 4427).
//   OnControllerSelected(...), line 1385 -- only reachable from Draw, and needs ChangeList plus the
//     layer assignment (decompiled SortList, line 7656).
// The state machine below -- what the picker holds and how it validates -- is complete; only the
// IMGUI row and the avatar-layer convenience overload are outstanding.
//
// Deliberately unported: the AssetCandidate / SelectCandidate() pair, line 1316 and 1406. That is
// an obfuscator-injected null check on an always-null static with no callers, the same shape as
// LoginCandidate/PublishCandidate on the neighbouring types.

using UnityEditor;
using UnityEditor.Animations;
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
