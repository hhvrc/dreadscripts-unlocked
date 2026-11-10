// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: one member of the PhysBoneEditor class. Line numbers move with the snapshot; the
// member name below is the durable reference.
//
//   ApplyGlobalGizmoSettings -> same, line 4414
//
// NOTES
// Added out of order, ahead of the rest of PhysBoneEditor's inspector: this is the change callback
// on four of ADOSettings' settings fields (gizmosActive, globalGizmo, gizmoBoneOpacity,
// gizmoLimitOpacity, decompiled 1483/1486/1525/1528), so ADOSettings cannot be ported without it.
//
// The decompiled member used to read InterruptSingleton here; that was the obfuscator's name, and
// renames/ADOverhaul2022.json now maps it (token 0x0600019b) to ApplyGlobalGizmoSettings, so
// reverse-engineering/export/ and this file agree on the identifier.
//
// WIRING. The four settings do not name this method in this package: they raise the assignable seam
// ADOSettings.onGizmoSettingsChanged, declared in Editor/ADOverhaul/ADOSettings/ADOSettings.ChangeHooks.cs,
// and that field is initialised to this method. Changing a gizmo setting therefore reaches the
// scene's PhysBones exactly as it did in the shipped build.
//
// Three further call sites remain unported, all inside PhysBoneEditor's own inspector: the two
// SettingsChangeScopes around the gizmo settings rows (decompiled lines 2539 and 3513) and the
// direct call from OnEnable at line 4298.
//
// 2019 vs 2022
// The 2019 build carries the same method under the obfuscator's name CancelProducer, declared at
// line 4400 of reverse-engineering/export/ADOverhaul2019/DreadScripts/ADOverhaul/ADOverhaul.cs. The two are the
// same method, on five independent pieces of evidence:
//
//   1. Same signature and accessibility on the same type: `internal static void`, no parameters, on
//      PhysBoneEditor.
//   2. Same four use sites, in the same order and on the same fields: it is the change callback of
//      gizmosActive, globalGizmo, gizmoBoneOpacity and gizmoLimitOpacity (2019 lines
//      1480/1483/1522/1525; 2022 lines 1483/1486/1525/1528).
//   3. Same three remaining call sites, in identical surroundings: two ADOSettings.SettingsChangeScope
//      uses wrapping the "Global Setting"/"Local Setting" toggle (2019 2531 and 3492; 2022 2539 and
//      3513), and one direct call from OnEnable, at the same position in an otherwise
//      statement-for-statement identical prologue (2019 4284; 2022 4298).
//   4. Same position in the type: immediately after the binding-label builder and immediately
//      before the "Advanced" integration-type toggle in both builds.
//   5. Same body skeleton: both guard on ADOSettings' singleton accessor reading globalGizmo and
//      then call Object.FindObjectsOfType<VRCPhysBone>().
//
// DEOBF-BUG(resolved)
// The 2019 decompile of this method loses its loop. reverse-engineering/export/ADOverhaul2019 line 4400 renders the
// whole body as a guard around a bare `UnityEngine.Object.FindObjectsOfType<VRCPhysBone>();` whose
// result is discarded -- the three field writes and the foreach around them are simply absent, and
// nothing in the 2019 tree writes VRCPhysBone.showGizmos at all.
//
// That is a deobfuscation defect, not a difference between the two builds. `ilspycmd -il` on the
// original, still-obfuscated binaries/ADOverhaul2019.dll shows CancelProducer with a VRCPhysBone[]
// local and an intact loop body inside the Reactor state machine, carrying
// `stfld VRC.Dynamics.VRCPhysBoneBase::showGizmos`, `::boneOpacity` and `::limitOpacity` fed from
// gizmosActive, gizmoBoneOpacity and gizmoLimitOpacity through their implicit conversions -- the
// same three assignments the 2022 decompile shows in full.
//
// Nothing was guessed to repair it: what is written below is the 2022 body, transcribed from
// reverse-engineering/export/ADOverhaul2022 lines 4414-4426, which is complete and needs no reconstruction. The
// 2019 loss is recorded here only because that build is this file's cross-reference, and a reader
// comparing the two would otherwise read it as a behavioural change between versions.
//
// Audit status: VERIFIED -- the nine-line body was re-read against reverse-engineering/export/ADOverhaul2022 lines
// 4414-4426 on 2026-08-05, together with all seven use sites in both builds and the 2019 original's
// IL as described above.

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
        /// runs whenever one of the four gizmo settings changes, via
        /// <see cref="ADOSettings.onGizmoSettingsChanged"/>.
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
