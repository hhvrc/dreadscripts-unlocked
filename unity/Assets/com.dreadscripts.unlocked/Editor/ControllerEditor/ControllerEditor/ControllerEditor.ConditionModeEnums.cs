// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   FloatModes -> FloatModes, lines 2187-2191   (vendor names; unobfuscated in the shipped build)
//   IntModes   -> IntModes,   lines 2193-2199
//   BoolModes  -> BoolModes,  lines 2201-2205
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Grouped into one file because each is a handful of lines and all three answer the same question:
// which AnimatorConditionMode values a parameter of that type may use.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The <c>AnimatorConditionMode</c> values a float parameter may be compared with, so the
        /// condition popup can offer exactly those. The values are the engine enum's, not a
        /// zero-based list.
        /// </summary>
        private enum FloatModes
        {
            Greater = 3,
            Less = 4
        }

        /// <summary>
        /// The <c>AnimatorConditionMode</c> values an int parameter may be compared with. Same
        /// numbering as the engine enum.
        /// </summary>
        private enum IntModes
        {
            Greater = 3,
            Less = 4,
            Equals = 6,
            NotEqual = 7
        }

        /// <summary>
        /// The <c>AnimatorConditionMode</c> values a bool parameter may be compared with — the
        /// engine's <c>If</c> and <c>IfNot</c>, relabelled for the condition popup.
        /// </summary>
        private enum BoolModes
        {
            True = 1,
            False = 2
        }
    }
}
