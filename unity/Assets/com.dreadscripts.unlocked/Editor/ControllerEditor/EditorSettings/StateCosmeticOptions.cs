// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// Ported region: the `internal enum StateCosmeticOptions` nested in the nested `EditorSettings`
// class, lines 439-449. Lifted to a top-level type, as this package does with the other enums the
// decompiled god-class carries as nested members (LayerViewViewType, OptionState, OrientationState).
// Read through EditorSettings.GetStateCosmetics(); stored in the stateCosmetics setting.
// Audit status: VERIFIED -- all eight members and their values diffed against
// EditorSettings.StateCosmeticOptions in export/; the absent [Flags] attribute and the flags-mask
// draw (EnumSetting.DrawEnumPopup's second bool selects EditorGUILayout.EnumFlagsField) were both
// confirmed at the declaration and at the one call site.

using System;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Which decorations the tool draws over the Animator window's state nodes.
    /// </summary>
    /// <remarks>
    /// A flags enum, but without <see cref="FlagsAttribute"/> -- the shipped build declares it plain
    /// and passes <c>flags: true</c> to the popup instead, so the values are drawn as a mask even
    /// though <see cref="Enum.ToString()"/> will not decompose them. Preserved as shipped.
    /// </remarks>
    internal enum StateCosmeticOptions
    {
        none = 0,

        /// <summary>Name of the state's motion, under the state name.</summary>
        motionName = 1,

        /// <summary>Small icon showing whether the motion is a clip or a blend tree.</summary>
        motionIcon = 2,

        /// <summary>The node's position in the graph.</summary>
        coordinates = 4,

        /// <summary>Behaviour and write-defaults markers.</summary>
        indicators = 8,

        /// <summary>The same markers for the conditions that are not currently satisfied.</summary>
        inactiveIndicators = 16,

        /// <summary>The inline button for creating a clip on an empty state.</summary>
        quickNewClip = 32,

        /// <summary>
        /// Everything, including bits no option above claims -- this is -1, not the union of the
        /// members, so a decoration added later is on by default for existing users.
        /// </summary>
        all = -1
    }
}
