// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_AlgoProcessor -> accentColor, line 2188
//   static PushList        -> Grey,        line 7395
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Partial in progress: the rest of the colour helpers in the outer class body are not ported yet.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>The tool's accent colour, used for hover highlights on clickable labels.</summary>
        internal static Color accentColor = new Color(1f, 0.5f, 0.7f);

        /// <summary>
        /// A neutral grey. Accepts either a 0-1 level or an 0-255 one and tells them apart by
        /// magnitude, so call sites can be written either way.
        /// </summary>
        /// <remarks>
        /// The ambiguity is real but harmless in practice: the only 0-255 value that would be
        /// misread is 1, and a level of 1/255 is indistinguishable from black anyway.
        /// </remarks>
        internal static Color Grey(float level)
        {
            if (level > 1f)
            {
                level /= 255f;
            }

            return new Color(level, level, level, 1f);
        }
    }
}
