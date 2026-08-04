// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: one member of the PhysBoneEditor class. Line numbers move with the snapshot; the
// member name below is the durable reference.
//
//   InterruptSingleton -> ApplyGlobalGizmoSettings, line 4210
//
// Added out of order, ahead of the rest of PhysBoneEditor's inspector: this is the change callback
// on four of ADOSettings' settings fields (gizmosActive, globalGizmo, gizmoBoneOpacity,
// gizmoLimitOpacity), so ADOSettings cannot be ported without it. PhysBoneEditor.cs's omission list
// no longer names it.
//
// Two further call sites remain unported, both inside PhysBoneEditor's own inspector: the
// SettingsChangeScope around the gizmo settings row (decompiled lines 2539 and 3309) and the
// OnEnable-time push at line 4094.
//
// Audit status: VERIFIED against export -- the whole method is nine lines and was re-read against
// lines 4210-4222 on 2026-08-04.

using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// Pushes the user's gizmo preferences onto every PhysBone in the open scenes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// VRChat stores gizmo visibility and opacity per component rather than as an editor
        /// preference, so "make this a global setting" has to mean "write it to all of them". This
        /// runs whenever one of the four gizmo settings changes.
        /// </para>
        /// <para>
        /// Doing nothing when <see cref="ADOSettings.globalGizmo"/> is off is what makes the
        /// setting mean what it says: each PhysBone keeps whatever it was last given, including
        /// values written while the setting was still on.
        /// </para>
        /// <para>
        /// FindObjectsOfType skips inactive objects, so a PhysBone on a disabled avatar is not
        /// updated until it is enabled and something changes a setting again — as shipped.
        /// </para>
        /// </remarks>
        internal static void ApplyGlobalGizmoSettings()
        {
            if (!ADOSettings.instance.globalGizmo)
            {
                return;
            }

            foreach (VRCPhysBone physBone in Object.FindObjectsOfType<VRCPhysBone>())
            {
                physBone.showGizmos = ADOSettings.instance.gizmosActive;
                physBone.boneOpacity = ADOSettings.instance.gizmoBoneOpacity;
                physBone.limitOpacity = ADOSettings.instance.gizmoLimitOpacity;
            }
        }
    }
}
