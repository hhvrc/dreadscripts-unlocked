// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static FindProcess -> AddParameterIfMissing, line 2350
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// The avatar-descriptor side of the animator work -- looking a controller up by playable layer -- is
// in ADOEditorUtility.AvatarDescriptors.cs, since it operates on the descriptor rather than on a
// controller.

using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Adds a parameter to <paramref name="controller"/> unless one of that name already exists.
        /// </summary>
        /// <param name="defaultValue">
        /// Written into all three default slots at once -- non-zero for the bool default, truncated
        /// for the int, and used as-is for the float -- so one argument covers whichever
        /// <paramref name="type"/> is being added.
        /// </param>
        /// <returns>True if the parameter was added, false if it was already there.</returns>
        /// <remarks>
        /// Matching is by name only, not by name and type: a parameter of the right name but the
        /// wrong type is left alone and reported as already present. That is what keeps the tool from
        /// clobbering a user's own parameter, at the cost of not detecting a type mismatch.
        /// </remarks>
        internal static bool AddParameterIfMissing(this AnimatorController controller, string name, AnimatorControllerParameterType type, float defaultValue)
        {
            bool missing = controller.parameters.All(p => p.name != name);
            if (missing)
            {
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = name,
                    type = type,
                    defaultBool = defaultValue != 0f,
                    defaultInt = (int)defaultValue,
                    defaultFloat = defaultValue
                });
            }

            return missing;
        }
    }
}
