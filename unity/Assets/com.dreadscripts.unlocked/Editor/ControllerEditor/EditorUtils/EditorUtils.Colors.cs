// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static configurationProperty  -> validColor,     line 2178
//   static _ProcProperty          -> errorColor,     line 2180
//   static _WrapperProcessor      -> warningColor,   line 2182
//   static m_AnnotationProcessor  -> highlightColor, line 2184
//   static m_AlgoProcessor        -> accentColor,    line 2188
//   static PushList               -> Grey,           line 7395
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// The four status colours are named for the role their call sites give them, not for the hue:
//   validColor / warningColor are the true/false pair of the valid-vs-over-budget counters
//     (ParameterCostTracker.DrawCounter line 1546, MenuClipboardState.DrawCounter line 1737) and
//     of the labelled asset field at line 4320; validColor is also the plain Log tint and the
//     Handles colour of the disc handle at line 6153.
//   warningColor / errorColor are the Warning and Error/Assert/Exception tints of the rich-text
//     log colouriser (CloneResolver, line 2771) and of the bug-report window's status lines
//     (ControllerEditor.cs lines 1791, 1846, 1853, 1869 -- ported as BugReporter).
//   highlightColor has no call site left in the ControllerEditor assembly. Its ADOverhaul twin
//     (see below) is the "on" background of a toggled row, so it is named for that emphasis role.
//
// Shared with ADOverhaul: ADOEditorUtility declares the same palette with the same literals
// (decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs lines 2062-2072:
// _ObserverSerializer, _BroadcasterSerializer, _EventSerializer, resolverSerializer; the 2019
// build matches under different obfuscated names). A later pass could lift these into
// Editor/Common; deliberately not consolidated here, since that file belongs to the other
// product.
//
// Partial in progress: the rest of the colour helpers in the outer class body are not ported yet.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Tint for a value that is within its limit, and for ordinary log output.</summary>
        internal static Color validColor = new Color(0.56f, 0.94f, 0.47f);

        /// <summary>Tint for a failed operation: errors, assertions and exceptions.</summary>
        internal static Color errorColor = new Color(1f, 0.25f, 0.25f);

        /// <summary>
        /// Tint for a value that is out of bounds but not yet fatal, and for warning log output.
        /// </summary>
        /// <remarks>
        /// This is the "false" half of every valid/invalid pair in the tool, so an over-budget
        /// counter reads as a warning rather than as an error -- the user can still fix it before
        /// anything is written.
        /// </remarks>
        internal static Color warningColor = new Color(0.99f, 0.95f, 0f);

        /// <summary>Emphasis tint, used to mark a row as active or singled out.</summary>
        internal static Color highlightColor = new Color(0.7f, 0.3f, 1f);

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
