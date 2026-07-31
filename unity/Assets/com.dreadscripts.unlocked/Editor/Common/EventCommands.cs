// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type, character-for-character identical in both. Reconstructed from:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs, line 1067
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs,   line 837
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. The enum is nested inside EditorUtils /
// ADOEditorUtility in the shipped assemblies; it is lifted to the namespace here because it is
// shared.

namespace DreadScripts.Common
{
    /// <summary>
    /// The editor commands that arrive as <see cref="UnityEngine.EventType.ValidateCommand"/> and
    /// <see cref="UnityEngine.EventType.ExecuteCommand"/> events.
    /// </summary>
    /// <remarks>
    /// Unity identifies these commands by string name on <c>Event.commandName</c>. Naming them as
    /// an enum lets a window ask for one by symbol and have the string produced from
    /// <see cref="System.Enum.ToString()"/>, so a typo becomes a compile error rather than a
    /// command that silently never fires. The member names therefore have to match Unity's command
    /// strings exactly, and must not be renamed for style.
    /// </remarks>
    internal enum EventCommands
    {
        Copy,
        Cut,
        Paste,
        Duplicate,
        Delete,
        SoftDelete,
        SelectAll,
        Find,
        FrameSelected,
        FrameSelectedWithLock,
        FocusProjectWindow
    }
}
