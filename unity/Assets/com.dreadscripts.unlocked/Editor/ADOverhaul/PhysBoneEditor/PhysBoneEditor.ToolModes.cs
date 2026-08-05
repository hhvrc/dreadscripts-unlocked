// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the tool-mode state of PhysBoneEditor — lines 3116-3120 and the [SpecialName]
// accessors at lines 3133-3210, plus LogoutSingleton (line 4130) and SetupSingleton (line 4145) of
// the current snapshot. Line numbers move with the snapshot; the member names below are the durable
// reference.
//
//   m_StubAuthentication     -> editedBindingIndex,        line 3116
//   rulesAuthentication      -> toolModes,                 line 3118
//   m_TestsAuthentication    -> toolModeNames,             line 3120
//   CountAccount()           -> activeBinding,             line 3133
//   RemoveAccount()          -> isEditingProperty,         line 3143
//   ResolveAccount()/ResetAccount(bool)   -> isEditingEndpoints,          line 3153
//   FlushAccount()/ExcludeAccount(bool)   -> isSelectingIgnoreTransforms, line 3165
//   ConnectAccount()/FindAccount(bool)    -> isCopyingIgnoreTransforms,   line 3177
//   ValidateAccount()/CreateAccount(bool) -> isSelectingColliders,        line 3189
//   RevertAccount()/RunParams(bool)       -> isCopyingColliders,          line 3201
//   LogoutSingleton(int)     -> SetPropertyEditTarget(int), line 4130
//   SetupSingleton()         -> ExitTool(),                line 4145
//
// The [SpecialName] methods are property accessors that ILSpy did not recombine, so they are
// restored to properties here. The getter/setter pairs become read/write properties; RemoveAccount
// and CountAccount have no setter in the original and stay read-only.
//
// 2019 vs 2022: identical, including the seven mode names and their order.

using UnityEditor;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// Which of the scene-view tools is armed, if any. Index 0 is the inert "None" entry, so an
        /// <c>activeIndex</c> of 0 or below means no tool is running.
        /// </summary>
        /// <remarks>
        /// The tools are mutually exclusive because they all claim scene-view clicks, and an
        /// <see cref="ExclusiveSelectionState"/> is used rather than a plain enum so each mode can
        /// be bound to its own toggle button in the inspector: setting one selected clears whichever
        /// other was.
        /// </remarks>
        private static readonly ExclusiveSelectionState toolModes = new ExclusiveSelectionState(7);

        /// <summary>
        /// Display names for the tool dropdown in the scene-view overlay, indexed by
        /// <see cref="ExclusiveSelectionState.activeIndex"/>.
        /// </summary>
        private static readonly string[] toolModeNames = new string[7]
        {
            "None",
            "End Position Edit",
            "Ignore Selection",
            "Ignore Copy",
            "Collision Selection",
            "Collision Copy",
            "Property Edit"
        };

        /// <summary>
        /// Index into <see cref="bindings"/> of the property the Property Edit tool is currently
        /// driving, or -1 when none is chosen. Survives the tool being switched off, so re-arming
        /// the tool does not silently retarget it.
        /// </summary>
        private static int editedBindingIndex = -1;

        /// <summary>
        /// The binding the Property Edit tool is driving, or null when that tool is not the active
        /// one.
        /// </summary>
        /// <remarks>
        /// Deliberately null rather than simply reading <see cref="editedBindingIndex"/>: the index
        /// is remembered across tool switches, so it is only meaningful while the Property Edit tool
        /// actually holds the scene view.
        /// </remarks>
        private static PropertyBinding activeBinding
        {
            get
            {
                if (isEditingProperty)
                {
                    return bindings[editedBindingIndex];
                }

                return null;
            }
        }

        /// <summary>
        /// Whether the Property Edit tool is armed <em>and</em> pointed at a property. Both halves
        /// matter — the tool can be selected in the dropdown with no property chosen yet.
        /// </summary>
        private static bool isEditingProperty
        {
            get
            {
                if (editedBindingIndex < 0)
                {
                    return false;
                }

                return toolModes.activeIndex == 6;
            }
        }

        /// <summary>
        /// Whether the End Position Edit tool is armed: position handles on each chain's endpoint,
        /// writing back to <c>endpointPosition</c>.
        /// </summary>
        private static bool isEditingEndpoints
        {
            get
            {
                return toolModes.activeIndex == 1;
            }
            set
            {
                toolModes.SetSelected(1, value);
            }
        }

        /// <summary>
        /// Whether the Ignore Selection tool is armed: click transforms in the scene to add or
        /// remove them from the ignore list.
        /// </summary>
        private static bool isSelectingIgnoreTransforms
        {
            get
            {
                return toolModes.activeIndex == 2;
            }
            set
            {
                toolModes.SetSelected(2, value);
            }
        }

        /// <summary>
        /// Whether the Ignore Copy tool is armed: click another PhysBone to copy its whole ignore
        /// list onto the selection.
        /// </summary>
        private static bool isCopyingIgnoreTransforms
        {
            get
            {
                return toolModes.activeIndex == 3;
            }
            set
            {
                toolModes.SetSelected(3, value);
            }
        }

        /// <summary>
        /// Whether the Collision Selection tool is armed: click colliders in the scene to add or
        /// remove them from the collider list.
        /// </summary>
        private static bool isSelectingColliders
        {
            get
            {
                return toolModes.activeIndex == 4;
            }
            set
            {
                toolModes.SetSelected(4, value);
            }
        }

        /// <summary>
        /// Whether the Collision Copy tool is armed: click another PhysBone to copy its whole
        /// collider list onto the selection.
        /// </summary>
        private static bool isCopyingColliders
        {
            get
            {
                return toolModes.activeIndex == 5;
            }
            set
            {
                toolModes.SetSelected(5, value);
            }
        }

        /// <summary>
        /// Points the Property Edit tool at <paramref name="bindingIndex"/>, or turns it off if it
        /// was already pointed there. A negative index turns it off unconditionally.
        /// </summary>
        /// <remarks>
        /// The re-click-to-dismiss behaviour is what makes the little edit button next to each
        /// property row act as a toggle. Note the asymmetry in how the two branches clear the mode:
        /// dismissing goes through <see cref="ExclusiveSelectionState.SetSelected"/> with false,
        /// which restores whichever mode was active before, while arming goes through
        /// <see cref="ExclusiveSelectionState.Select"/>. Ported as written.
        /// </remarks>
        private static void SetPropertyEditTarget(int bindingIndex)
        {
            if (bindingIndex < 0 || (isEditingProperty && editedBindingIndex == bindingIndex))
            {
                editedBindingIndex = -1;
                toolModes.SetSelected(6, false);
            }
            else
            {
                editedBindingIndex = bindingIndex;
                toolModes.Select(6);
            }

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Disarms every tool and forgets the Property Edit target. Called when the inspector is
        /// disabled and when the user presses Enter or Escape in the scene view.
        /// </summary>
        internal static void ExitTool()
        {
            toolModes.Clear();
            editedBindingIndex = -1;
        }
    }
}
