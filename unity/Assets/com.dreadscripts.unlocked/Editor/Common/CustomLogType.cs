// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both, which are identical:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, line 743
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, line 2149
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. The 2019 and 2022 ADOverhaul builds agree exactly.

namespace DreadScripts.Common
{
    /// <summary>
    /// The severity of a message logged by the tools' own console helper.
    /// </summary>
    /// <remarks>
    /// Deliberately separate from Unity's <c>LogType</c>: the helper prefixes and tints each
    /// message before forwarding it, and only these three levels get a colour, so the wider set of
    /// Unity severities would have nothing to map onto.
    /// </remarks>
    internal enum CustomLogType
    {
        Regular,
        Warning,
        Error
    }
}
