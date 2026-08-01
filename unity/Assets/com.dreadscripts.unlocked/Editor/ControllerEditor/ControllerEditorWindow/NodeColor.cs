// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   private enum NodeColor, nested in ControllerEditorWindow -> lifted to a top-level type, line 3192
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The palette Unity's Animator window offers for state nodes, in the order its own tinting
    /// code expects.
    /// </summary>
    /// <remarks>
    /// The six node-colour preferences in <see cref="EditorSettings"/> are stored as
    /// <c>FloatSetting</c>s rather than as this enum, because the settings block is JSON and
    /// its framework has no enum-typed setting that round-trips a named value. The stored float is
    /// the member's ordinal; this type exists to give the settings window a popup with names on it,
    /// and to document what those numbers mean.
    /// </remarks>
    internal enum NodeColor
    {
        Grey,
        Blue,
        Aqua,
        Green,
        Yellow,
        Orange,
        Red
    }
}
