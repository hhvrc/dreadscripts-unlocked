// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the half of PhysBoneEditor that installs it over the SDK's own inspector. Line
// numbers are relative to the current snapshot; the decompiled names are the durable reference.
//
//   _CallbackIdentifier    -> overrideInstalled,      line 2978
//   m_TokenAuthentication  -> physBoneType,           line 3126
//   getterAuthentication   -> sdkPhysBoneEditorType,  line 3128
//   SelectSingleton        -> ToggleEditorOverride,   line 4152
//   WriteSingleton         -> InstallEditorOverride,  line 4157
//
// NOTES
// This is the piece that answers "why is the ADO inspector not attaching". Neither product ships a
// [CustomEditor] attribute; both take the inspector over by rewriting Unity's internal editor table
// at runtime, which is what lets the [ADO] Toggle Editor context menu hand it back. The write does
// not survive a domain reload -- Unity rebuilds the table from attributes -- so
// ADOverhaul.InspectorInstall.cs re-runs InstallEditorOverride after every reload.
//
// The two cached types are resolved by name through ADOEditorUtility.FindType rather than with
// typeof, because the revert path needs VRCPhysBoneEditor, which lives in the SDK's editor assembly
// and is not public. Ported as the original has it, including caching a failed lookup as null and
// retrying it on the next call.
//
// The toggle's parameter is inverted on the way in (`overrideInstalled = !revert`) and then read
// back inverted again to choose the editor, which reads oddly but is what the shipped build does:
// SelectSingleton passes the *current* state as `revert`, so calling it while installed reverts and
// calling it while reverted installs.
//
// Audit status: VERIFIED -- all five members diffed statement by statement against the 2022 snapshot
// at the lines above: both null-checked type lookups, the inverted assignment, the conditional
// operand order in the OverrideCustomEditor call, the menu path and its 899 priority, and the
// `issetup = false` default. The 2019 build was not read for this region.

using System;
using UnityEditor;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// Whether this editor is currently installed over the SDK's. Starts true because the
        /// shipped build installs the override during startup after every domain reload, so by the
        /// time anything can read this it is meant to be the state it describes.
        /// </summary>
        private static bool overrideInstalled = true;

        /// <summary>The component type being replaced, resolved lazily by name.</summary>
        private static Type physBoneType;

        /// <summary>
        /// VRChat's own PhysBone inspector, which the override displaces and the toggle puts back.
        /// </summary>
        private static Type sdkPhysBoneEditorType;

        /// <summary>
        /// Context-menu entry that swaps between this inspector and VRChat's.
        /// </summary>
        /// <remarks>
        /// Sits on the component's own gear menu rather than in a tool window, so it is reachable
        /// from the inspector the user is looking at when they want the other one.
        /// </remarks>
        [MenuItem("CONTEXT/VRCPhysBone/[ADO] Toggle Editor", false, 899)]
        private static void ToggleEditorOverride()
        {
            InstallEditorOverride(overrideInstalled);
        }

        /// <summary>
        /// Points Unity's editor table for <c>VRCPhysBone</c> at this inspector, or back at the
        /// SDK's.
        /// </summary>
        /// <param name="revert">
        /// True to restore VRChat's inspector. The default installs this one, which is what the
        /// post-reload hook wants.
        /// </param>
        internal static void InstallEditorOverride(bool revert = false)
        {
            if (physBoneType == null)
            {
                physBoneType = ADOEditorUtility.FindType("VRCPhysBone");
            }

            if (sdkPhysBoneEditorType == null)
            {
                sdkPhysBoneEditorType = ADOEditorUtility.FindType("VRCPhysBoneEditor");
            }

            overrideInstalled = !revert;

            ADOEditorUtility.OverrideCustomEditor(
                physBoneType,
                !overrideInstalled ? sdkPhysBoneEditorType : typeof(PhysBoneEditor));
        }
    }
}
