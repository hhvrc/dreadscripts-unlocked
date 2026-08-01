// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
// Ported region: the change callback passed to the four gizmo settings' field initialisers of the
// nested `ADOSettings` class, lines 1483, 1486, 1525 and 1528.
//
// decompiled member -> ported member, line N:
//   PhysBoneEditor.InterruptSingleton -> onGizmoSettingsChanged, 4210
// (the 2019 build calls the same method PhysBoneEditor.CancelProducer, line 4207 there.)
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// That method is a static member of PhysBoneEditor which is NOT yet ported -- see the "LARGELY NOT
// PORTED" list at the head of PhysBoneEditor.cs, where it is named. Rather than stub it, or drop the
// callbacks and silently change behaviour, it becomes an assignable seam here, in the manner
// Editor/Common/Settings/SettingBase.cs already uses for its reset button and
// Editor/ControllerEditor/EditorSettings/EditorSettings.ChangeHooks.cs uses for the same reason. The
// PhysBoneEditor port assigns it; until it does, changing one of the gizmo settings persists
// correctly and simply does not push the new value onto the PhysBones already in the scene.

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
        /// The shipped handler walks every <c>VRCPhysBone</c> in the open scenes and copies
        /// <see cref="gizmosActive"/>, <see cref="gizmoBoneOpacity"/> and
        /// <see cref="gizmoLimitOpacity"/> onto the component's own <c>showGizmos</c>,
        /// <c>boneOpacity</c> and <c>limitOpacity</c>. That is what makes these settings "global":
        /// the gizmos themselves are drawn by VRChat's own code from the component fields, so the
        /// only way to drive them from a preference is to write the components.
        /// </para>
        /// <para>
        /// SHIPPED BEHAVIOUR, worth knowing before wiring the handler up. The whole walk is skipped
        /// unless <see cref="globalGizmo"/> is on, so turning <see cref="globalGizmo"/> *off* leaves
        /// every PhysBone holding whatever the global settings last pushed onto it rather than
        /// restoring anything. The write also goes straight to the components without
        /// <c>Undo.RecordObject</c> or <c>EditorUtility.SetDirty</c>, so it dirties the scene only
        /// incidentally and is not undoable.
        /// </para>
        /// </remarks>
        internal static Action onGizmoSettingsChanged;
    }
}
