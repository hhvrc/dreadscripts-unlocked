// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
// Ported region: the change callback passed to the four gizmo settings' field initialisers of the
// nested `ADOSettings` class, lines 1483, 1486, 1525 and 1528.
//
// The callback those four initialisers pass, and what this file stands in for:
//   PhysBoneEditor.ApplyGlobalGizmoSettings -> onGizmoSettingsChanged
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The callback itself is claimed by Editor/ADOverhaul/PhysBoneEditor/PhysBoneEditor.GizmoSettings.cs,
// not here; this file only declares the seam it is assigned to, so it deliberately makes no MAP
// claim on it. In the 2022 snapshot the method is declared at ADOverhaul.cs line 4414, under the
// name ApplyGlobalGizmoSettings that renames/ADOverhaul2022.json now applies to the obfuscator's
// InterruptSingleton; older revisions of this header cited that obfuscated name, and before the
// reverse-engineering/export/ re-snapshot, line 4210.
//
// WIRED UP. The seam is satisfied. PhysBoneEditor.ApplyGlobalGizmoSettings is ported, and the
// declaration below carries it as the field's initialiser, so changing one of the four gizmo
// settings once again pushes the new value onto the PhysBones already in the scene. The field is
// left assignable rather than readonly, so a later PhysBoneEditor port can still redirect it.
//
// DELIBERATE DEVIATION
// The shipped build names PhysBoneEditor.ApplyGlobalGizmoSettings directly in each of the four
// settings' constructor arguments; this package routes all four through the one Action field below,
// in the manner Editor/Common/Settings/SettingBase.cs uses for its reset button and
// Editor/ControllerEditor/EditorSettings/EditorSettings.ChangeHooks.cs uses for its five hooks. The
// indirection is the whole of the difference: the same method runs, on the same four settings. The
// field's initialiser runs at ADOSettings' type initialisation, which the CLR guarantees to precede
// the first read of the field -- and that read is inside each setting's callback, i.e. no earlier
// than the moment a setting actually changes.
//
// 2019 vs 2022
// The 2019 build has the same method under the obfuscator's name CancelProducer, at line 4400 of
// reverse-engineering/export/ADOverhaul2019/DreadScripts/ADOverhaul/ADOverhaul.cs, passed to the same four settings
// at lines 1480, 1483, 1522 and 1525 there. PhysBoneEditor.GizmoSettings.cs carries the evidence
// tying the two together, and the DEOBF-BUG note on what the 2019 decompile of the body loses.
//
// Audit status: VERIFIED -- the four initialiser lines, the declaration at 4414 and the whole of
// the method body at 4414-4426 were read against reverse-engineering/export/ADOverhaul2022 on 2026-08-05, and the
// behaviour described in the remarks below (skipped entirely when globalGizmo is off; no
// Undo.RecordObject and no EditorUtility.SetDirty anywhere in the body) was re-derived from it.

using System;

namespace DreadScripts.ADOverhaul
{
    internal partial class ADOSettings
    {
        /// <summary>
        /// Raised when one of the four global gizmo settings changes -- <see cref="gizmosActive"/>,
        /// <see cref="globalGizmo"/>, <see cref="gizmoBoneOpacity"/> or
        /// <see cref="gizmoLimitOpacity"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The shipped handler is <see cref="PhysBoneEditor.ApplyGlobalGizmoSettings"/>, which this
        /// field is initialised to. It walks every <c>VRCPhysBone</c> in the open scenes and copies
        /// <see cref="gizmosActive"/>, <see cref="gizmoBoneOpacity"/> and
        /// <see cref="gizmoLimitOpacity"/> onto the component's own <c>showGizmos</c>,
        /// <c>boneOpacity</c> and <c>limitOpacity</c>. That is what makes these settings "global":
        /// the gizmos themselves are drawn by VRChat's own code from the component fields, so the
        /// only way to drive them from a preference is to write the components.
        /// </para>
        /// <para>
        /// SHIPPED BEHAVIOUR, worth knowing. The whole walk is skipped unless
        /// <see cref="globalGizmo"/> is on, so turning <see cref="globalGizmo"/> *off* leaves every
        /// PhysBone holding whatever the global settings last pushed onto it rather than restoring
        /// anything. The write also goes straight to the components without <c>Undo.RecordObject</c>
        /// or <c>EditorUtility.SetDirty</c>, so it dirties the scene only incidentally and is not
        /// undoable.
        /// </para>
        /// <para>
        /// The field remains assignable so that the rest of the PhysBoneEditor port, or a test, can
        /// substitute a different handler.
        /// </para>
        /// </remarks>
        internal static Action onGizmoSettingsChanged = PhysBoneEditor.ApplyGlobalGizmoSettings;
    }
}
