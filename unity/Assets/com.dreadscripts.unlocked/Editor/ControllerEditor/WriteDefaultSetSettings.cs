// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   nested enum WriteDefaultSetSettings -> WriteDefaultSetSettings, line 1060
// Line numbers are relative to the decompiled snapshot at the time of the port; the type name is
// the durable reference. The enum is nested inside EditorUtils in the shipped assembly; it is
// lifted to the namespace here.
//
// ControllerEditor only: ADOverhaul ships no counterpart under this or any other name, so this
// stays out of Common. No shipped code references the type -- it survives in the assembly with no
// call sites -- so the meaning of the members below is read off the names and off how Write
// Defaults is handled elsewhere in the tool, not off a use.
//
// Audit status: VERIFIED -- diffed in full against the nested enum in export/EditorUtils.cs
// (still at line 1060 in the current snapshot); the three members and their order match exactly.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// What an operation that creates animator states should do with their Write Defaults flag.
    /// </summary>
    /// <remarks>
    /// <see cref="Automatic"/> exists because neither fixed answer is safe: a layer whose states
    /// disagree with the rest of the controller misbehaves in ways that are hard to trace, so the
    /// setting a new state should get is normally the one the surrounding controller already uses
    /// rather than a global preference.
    /// </remarks>
    internal enum WriteDefaultSetSettings
    {
        Off,
        On,

        /// <summary>Match whatever the controller being edited already does.</summary>
        Automatic
    }
}
