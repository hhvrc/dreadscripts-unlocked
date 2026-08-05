// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/OptionState.cs
//
// Audit status: VERIFIED -- diffed in full against export/; the three members and their
// declaration order match exactly.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Tri-state for a generation option: leave it out, offer it where applicable, or apply it
    /// everywhere regardless.
    /// </summary>
    internal enum OptionState
    {
        Off,
        Allowed,
        Forced
    }
}
