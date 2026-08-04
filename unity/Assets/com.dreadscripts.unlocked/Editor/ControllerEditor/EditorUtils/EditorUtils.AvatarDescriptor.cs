// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static UpdateList    -> TryGetPlayableLayerController, line 7639
//   static ChangeList    -> GetPlayableLayerController,    line 7647
//   static SortList      -> SetPlayableLayerController,    line 7656
//   static ValidateError -> TryAssignLayerController,      line 8489
//   static CallError     -> SetExpressionParameters,       line 8000
//   static ReadError     -> SetExpressionsMenu,            line 8263
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// ValidateError is the body of a lambda the obfuscator had lifted into a static method: at the call
// site (decompiled SortList) its three captured variables are passed as a compiler-generated display
// struct, _003C_003Ec__DisplayClass446_0, holding the layer type, the controller and the descriptor.
// It has no other caller, so it is restored here as an ordinary private helper taking those three as
// parameters; the display struct itself is a decompiler artifact and is not ported.
//
// Deliberately not ported from the same region: AssetList (line 7610, builds the parallel
// name/type arrays for a playable-layer popup), RegisterList (line 7669, layer parameter cost),
// LogoutList (line 7674, descriptor lookup with a change callback), and the asset-creating siblings
// ConcatError (line 7978) and its menu counterpart (line ~8240), which create and save a new
// parameters/menu asset when the descriptor has none. Those belong to other regions and can be added
// when a call site needs them.
// Audit status: VERIFIED against decompiled/

using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Finds the animator controller the avatar has assigned to one playable layer.
        /// </summary>
        /// <returns>True when the layer exists and holds a controller.</returns>
        /// <remarks>
        /// Base and special layers are searched as one sequence, so the caller does not have to know
        /// which of the two arrays a given <see cref="VRCAvatarDescriptor.AnimLayerType"/> lives in.
        ///
        /// The descriptor stores a <see cref="RuntimeAnimatorController"/>, but everything the editor
        /// does with it -- reading layers, parameters, state machines -- needs the editor-only
        /// <see cref="AnimatorController"/>. The cast is therefore a filter as much as a conversion:
        /// an override controller in the slot yields null and reads as "no controller here".
        /// </remarks>
        internal static bool TryGetPlayableLayerController(this VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType, out AnimatorController controller)
        {
            controller = (from layer in avatar.baseAnimationLayers.Concat(avatar.specialAnimationLayers)
                          where layer.type == layerType
                          select layer.animatorController).FirstOrDefault() as AnimatorController;

            return controller != null;
        }

        /// <summary>
        /// The animator controller assigned to one playable layer, or null when the layer is missing,
        /// empty, or holds something that is not an <see cref="AnimatorController"/>.
        /// </summary>
        internal static AnimatorController GetPlayableLayerController(this VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType)
        {
            if (avatar.TryGetPlayableLayerController(layerType, out AnimatorController controller))
            {
                return controller;
            }

            return null;
        }

        /// <summary>
        /// Assigns <paramref name="controller"/> to the avatar's playable layer of the given type.
        /// </summary>
        /// <returns>
        /// False when the descriptor has no layer of that type, in which case nothing was written.
        /// </returns>
        /// <remarks>
        /// Mutates the descriptor and marks it dirty; no <see cref="Undo"/> is registered, so the
        /// assignment cannot be undone with Ctrl+Z. That matches the original, and matches the
        /// descriptor's own inspector, which writes the same fields through a SerializedObject.
        ///
        /// Base layers are searched before special layers and the first matching layer wins, so a
        /// descriptor that somehow listed the same type in both arrays would only have its base entry
        /// written.
        /// </remarks>
        internal static bool SetPlayableLayerController(this VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType, RuntimeAnimatorController controller)
        {
            if (TryAssignLayerController(avatar.baseAnimationLayers, avatar, layerType, controller))
            {
                return true;
            }

            return TryAssignLayerController(avatar.specialAnimationLayers, avatar, layerType, controller);
        }

        /// <summary>
        /// Writes the controller into the first layer of <paramref name="layers"/> with a matching
        /// type, and reports whether such a layer was found.
        /// </summary>
        /// <remarks>
        /// <c>isDefault</c> is the descriptor's "use the SDK's stock layer" flag, so clearing the slot
        /// (a null controller) has to restore it rather than leave the avatar with an empty custom
        /// layer. Conversely <c>customizeAnimationLayers</c> is only ever turned on, never off: the
        /// user may have customised other layers, and this method cannot tell.
        ///
        /// The array is written through directly. <c>baseAnimationLayers</c> and
        /// <c>specialAnimationLayers</c> are arrays of a struct, so indexing the field yields the live
        /// element and the assignment sticks -- it would not if these were copied into a local first.
        /// </remarks>
        private static bool TryAssignLayerController(VRCAvatarDescriptor.CustomAnimLayer[] layers, VRCAvatarDescriptor avatar, VRCAvatarDescriptor.AnimLayerType layerType, RuntimeAnimatorController controller)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].type != layerType)
                {
                    continue;
                }

                if ((bool)controller)
                {
                    avatar.customizeAnimationLayers = true;
                }

                layers[i].isDefault = !controller;
                layers[i].animatorController = controller;
                EditorUtility.SetDirty(avatar);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Assigns the avatar's expression parameters asset, keeping
        /// <see cref="VRCAvatarDescriptor.customExpressions"/> consistent with it.
        /// </summary>
        /// <remarks>
        /// Mutates the descriptor and marks it dirty; no <see cref="Undo"/> is registered, so the
        /// change cannot be undone with Ctrl+Z, and the previous asset reference is simply dropped.
        /// The asset itself is untouched -- nothing is deleted -- but a caller passing null is
        /// responsible for remembering what was there.
        ///
        /// The flag is raised whenever a value is assigned, and lowered on null only if the menu slot
        /// is empty too: the one flag governs both slots, so clearing one while the other is still in
        /// use must leave expressions enabled or the surviving menu would stop being applied.
        /// </remarks>
        internal static void SetExpressionParameters(this VRCAvatarDescriptor avatar, VRCExpressionParameters parameters)
        {
            avatar.expressionParameters = parameters;
            if (!parameters)
            {
                if (!avatar.expressionsMenu)
                {
                    avatar.customExpressions = false;
                }
            }
            else
            {
                avatar.customExpressions = true;
            }

            EditorUtility.SetDirty(avatar);
        }

        /// <summary>
        /// Assigns the avatar's expressions menu asset, keeping
        /// <see cref="VRCAvatarDescriptor.customExpressions"/> consistent with it.
        /// </summary>
        /// <remarks>
        /// The mirror image of <see cref="SetExpressionParameters"/>, with the same caveats: the
        /// descriptor is marked dirty but no <see cref="Undo"/> is registered, the displaced menu
        /// asset is left on disk unreferenced, and the shared flag is only lowered when the parameters
        /// slot is empty as well.
        /// </remarks>
        internal static void SetExpressionsMenu(this VRCAvatarDescriptor avatar, VRCExpressionsMenu menu)
        {
            avatar.expressionsMenu = menu;
            if (!menu)
            {
                if (!avatar.expressionParameters)
                {
                    avatar.customExpressions = false;
                }
            }
            else
            {
                avatar.customExpressions = true;
            }

            EditorUtility.SetDirty(avatar);
        }
    }
}
