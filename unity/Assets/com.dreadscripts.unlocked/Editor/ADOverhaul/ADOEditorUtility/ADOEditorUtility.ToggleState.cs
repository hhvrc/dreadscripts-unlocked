// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static FillVal -> CycleToggleState, line 4042
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above and cross-checked against the 2019 build (ReadParam, line 4148), which is identical.
//
// Both shipped call sites are in decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs,
// lines 3375 and 3397, where the returned bool is passed straight to SetInArray as the "should the
// list contain these" argument:
//     property.SetInArray<VRCPhysBoneCollider>(CycleToggleState(states, index), colliders);
// which is what fixes the encoding below: 1 means on, 0 means off, and anything else means mixed.
//
// VENDOR BUG, reproduced as shipped. The two settled states agree with each other -- 0 becomes 1 and
// reports true, 1 becomes 0 and reports false -- but the mixed state does not: it stores the
// *opposite* of what it returns. A tri-state toggle cycled out of Mixed therefore adds the objects
// to the list while recording itself as off (or removes them while recording itself as on), so the
// next click cycles the wrong way and the checkbox disagrees with the list until something rebuilds
// the state array. Correcting it would be a behaviour change and is deliberately not done here; the
// fix, if it is ever wanted, is `states[index] = defaultValue ? (byte)1 : (byte)0`.

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Advances one entry of a tri-state toggle array and reports the state it moved to.
        /// </summary>
        /// <param name="states">
        /// Per-item toggle states, encoded as 0 = off, 1 = on, anything else = mixed. A byte array
        /// rather than an enum because it is built alongside the serialized data it mirrors.
        /// </param>
        /// <param name="index">Which entry to cycle.</param>
        /// <param name="defaultValue">
        /// What a mixed entry resolves to -- the state a click on a mixed checkbox is taken to mean.
        /// </param>
        /// <returns>
        /// True if the entry is now on. See the vendor-bug note at the top of this file for the
        /// mixed case, where the returned value and the stored value disagree.
        /// </returns>
        internal static bool CycleToggleState(byte[] states, int index, bool defaultValue = true)
        {
            switch (states[index])
            {
                case 0:
                    states[index] = 1;
                    return true;

                case 1:
                    states[index] = 0;
                    return false;

                default:
                    states[index] = defaultValue ? (byte)0 : (byte)1;
                    return defaultValue;
            }
        }
    }
}
