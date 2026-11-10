// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static MapVal    -> TryGetAnimatorController,   line 4034
//   static ConcatVal -> GetPopulatedPlayableLayers, line 4005
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above, including the one closure lambda named next.
//
// One lambda from the compiler-generated _003C_003Ec closure (line 1592) belongs here and gets no
// file: `l => l.animatorController`, inlined below.
//
// Shared with ControllerEditor: EditorUtils.AvatarDescriptor.cs covers the same descriptor surface
// for the other tool. Deliberately NOT consolidated, on the same basis as ADOEditorUtility.Colors.cs.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The controller <paramref name="descriptor"/> has assigned to playable layer
        /// <paramref name="layerType"/>.
        /// </summary>
        /// <returns>False, with <paramref name="controller"/> null, when the layer is unset or its
        /// controller is not an <see cref="AnimatorController"/> asset.</returns>
        /// <remarks>
        /// Searches the base and the special layer arrays together, because the descriptor splits
        /// them but the layer type identifies a slot uniquely across both. The cast is what makes a
        /// runtime-only controller -- an override controller, or one supplied at runtime -- report as
        /// absent: this is used by editing features that need an editable asset.
        /// </remarks>
        internal static bool TryGetAnimatorController(this VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType layerType, out AnimatorController controller)
        {
            controller = descriptor.baseAnimationLayers
                .Concat(descriptor.specialAnimationLayers)
                .Where(layer => layer.type == layerType)
                .Select(layer => layer.animatorController)
                .FirstOrDefault() as AnimatorController;

            return controller != null;
        }

        /// <summary>
        /// Builds the parallel name/value arrays for a playable-layer popup, listing only the layers
        /// <paramref name="descriptor"/> actually has a controller on.
        /// </summary>
        /// <param name="names">Receives the display names, in fixed layer order.</param>
        /// <param name="values">
        /// Receives the matching <see cref="VRCAvatarDescriptor.AnimLayerType"/> values, as ints
        /// because that is what <c>EditorGUI.IntPopup</c> takes.
        /// </param>
        /// <remarks>
        /// The eight names are hard-coded in layer order and the enum value is derived from the
        /// index, skipping 1: the SDK's AnimLayerType numbers Base as 0 and then leaves 1 unused, so
        /// index 0 maps to 0 and every later index maps to index + 1. That coupling to the SDK's
        /// numbering is why the table cannot simply be Enum.GetNames.
        /// </remarks>
        internal static void GetPopulatedPlayableLayers(VRCAvatarDescriptor descriptor, ref string[] names, ref int[] values)
        {
            string[] allNames = { "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose" };

            if (!(bool)(UnityEngine.Object)descriptor)
            {
                names = Array.Empty<string>();
                values = Array.Empty<int>();
                return;
            }

            List<(string name, int value)> populated = new List<(string, int)>();
            for (int i = 0; i < allNames.Length; i++)
            {
                int layerValue = (i != 0) ? (i + 1) : i;
                if (descriptor.TryGetAnimatorController((VRCAvatarDescriptor.AnimLayerType)layerValue, out AnimatorController _))
                {
                    populated.Add((allNames[i], layerValue));
                }
            }

            names = new string[populated.Count];
            values = new int[populated.Count];
            for (int i = 0; i < populated.Count; i++)
            {
                names[i] = populated[i].name;
                values[i] = populated[i].value;
            }
        }
    }
}
