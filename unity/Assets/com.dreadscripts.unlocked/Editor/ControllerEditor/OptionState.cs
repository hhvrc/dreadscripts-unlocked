// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/OptionState.cs

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
