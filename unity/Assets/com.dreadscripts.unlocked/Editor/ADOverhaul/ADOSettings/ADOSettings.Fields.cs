// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
// Ported region: the settings fields of the nested `ADOSettings` class, lines 1441-1549.
//
// Every field keeps its decompiled name -- these names are the on-disk contract. They are what
// JsonUtility writes into the MAIN envelope, so renaming any of them silently discards the user's
// stored value.
//
//   a_HasSucceededLastVerification -> NOT PORTED, line 1474 -- licensing-gate remnant, see ADOSettings.cs
//   a_VerifyOnDisplay -> NOT PORTED, line 1477 -- licensing-gate remnant, see ADOSettings.cs
//   a_VerifyOnProjectLoad -> NOT PORTED, line 1480 -- licensing-gate remnant, see ADOSettings.cs
//
// Every other field of the region keeps its decompiled name and its initialiser; the entries below
// are the only ones whose shape changed:
//   [SpecialName] GetValue()/SetValue() on each setting -> the Common port's `value` property
//   PhysBoneEditor.InterruptSingleton (declared 4414) -> onGizmoSettingsChanged, in ADOSettings.ChangeHooks.cs
//   ADOEditorUtility.PositionFlag -> DreadScripts.Common.PositionFlag (Common/ResizeHandle/)
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The `stateColors` property here corresponds to [SpecialName] StateColors() (line 1679), which
// sits outside the field region and is claimed by ADOSettings.cs -- which also declares its own
// `StateColors` property. The decompiled member is therefore ported twice under two spellings; the
// duplicate is left alone here because resolving it would mean deleting C# code.
// The onChange delegate cited above was recorded as line 4210 before the decompiled/ re-snapshot;
// 4210 now lands inside PublishSingleton, and InterruptSingleton is declared at 4414.
//
// Audit status: PARTIAL -- the field list, names, defaults and the three unported a_* lines were
// compared against decompiled/ lines 1441-1549 for this port; the XML doc remarks citing reader
// line numbers elsewhere in the file (3366, 3461, 6113, 8240, ...) were not re-checked.

using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// The settings themselves.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All of these persist together, as a single JSON string in <see cref="UnityEditor.EditorPrefs"/>
    /// under <see cref="prefsKey"/>. Nothing here is stored in an asset or in the project; settings
    /// are per-user and per-machine, and are shared by every project on that machine.
    /// </para>
    /// <para>
    /// Each field's initialiser carries its default. That initialiser is also what supplies the
    /// default when a setting is missing from the stored block -- see <see cref="SettingBase"/> --
    /// so a setting added in a later version appears at its default without invalidating the rest.
    /// </para>
    /// </remarks>
    internal partial class ADOSettings
    {
        // ---- Gizmos ----

        /// <summary>
        /// Whether PhysBone gizmos are drawn at all. Default true.
        /// </summary>
        /// <remarks>
        /// Not itself consulted when drawing: it is pushed onto every <c>VRCPhysBone.showGizmos</c>
        /// in the scene by the handler on <see cref="onGizmoSettingsChanged"/>, and the PhysBone
        /// inspector's gizmo toggle writes here rather than to the component when
        /// <see cref="globalGizmo"/> is on.
        /// </remarks>
        [SerializeField]
        internal BoolSetting gizmosActive = new BoolSetting(true, () => onGizmoSettingsChanged?.Invoke());

        /// <summary>
        /// The gizmo toggle in the PhysBone inspector edits this settings block rather than the
        /// selected components -- the "Global Setting" / "Local Setting" switch. Default true.
        /// </summary>
        [SerializeField]
        internal BoolSetting globalGizmo = new BoolSetting(true, () => onGizmoSettingsChanged?.Invoke());

        /// <summary>
        /// Alpha of the bone gizmo spheres, pushed onto <c>VRCPhysBone.boneOpacity</c>. Default 0.5.
        /// </summary>
        [SerializeField]
        internal FloatSetting gizmoBoneOpacity = new FloatSetting(0.5f, () => onGizmoSettingsChanged?.Invoke());

        /// <summary>
        /// Alpha of the angle/limit gizmo cones, pushed onto <c>VRCPhysBone.limitOpacity</c>.
        /// Default 0.5.
        /// </summary>
        [SerializeField]
        internal FloatSetting gizmoLimitOpacity = new FloatSetting(0.5f, () => onGizmoSettingsChanged?.Invoke());

        // ---- Inspector ----

        /// <summary>
        /// Animate the inspector's section foldouts rather than snapping them open. Default true.
        /// </summary>
        /// <remarks>Read where the foldout's <c>AnimBool</c> target is set, decompiled line 8240.</remarks>
        [SerializeField]
        internal BoolSetting editorAnimatedFoldouts = new BoolSetting(true);

        // ---- Scene view handles ----

        /// <summary>
        /// Label each bone and collider handle with the transform's name. Default true.
        /// </summary>
        /// <remarks>Read by the scene GUI at decompiled lines 3366, 3388, 3410 and 3436.</remarks>
        [SerializeField]
        internal BoolSetting onSceneNameLabels = new BoolSetting(true);

        /// <summary>
        /// Show the tool-selection overlay in the scene view. Default true. Positioned by
        /// <see cref="toolSelectionOverlayAlignment"/>.
        /// </summary>
        [SerializeField]
        internal BoolSetting onSceneToolSelection = new BoolSetting(true);

        /// <summary>
        /// Keep the tool-selection overlay up even when no ADO tool is active. Default true.
        /// </summary>
        [SerializeField]
        internal BoolSetting onSceneToolSelectionAlwaysVisible = new BoolSetting(true);

        /// <summary>
        /// Show the editing overlay -- the panel naming the active tool -- in the scene view.
        /// Default true. Positioned by <see cref="toolOverlayAlignment"/>.
        /// </summary>
        [SerializeField]
        internal BoolSetting onSceneEditingOverlay = new BoolSetting(true);

        /// <summary>
        /// Declared, never read. Default true.
        /// </summary>
        /// <remarks>
        /// Nothing in either shipped build reads this -- not the scene GUI, not even the settings
        /// window, which does not draw it. Presumably the overlay's click-through behaviour was made
        /// unconditional and the setting was left behind. Kept because it is part of the persisted
        /// block: dropping it would be harmless for loading but would lose a user's stored value if
        /// a future version ever wires it up again.
        /// </remarks>
        [SerializeField]
        internal BoolSetting onSceneOverlayInterceptsClick = new BoolSetting(true);

        /// <summary>
        /// Show the tooltip strip under the editing overlay. Default true.
        /// </summary>
        /// <remarks>Read at decompiled line 3572, where the overlay decides whether to reserve room
        /// for the hint text.</remarks>
        [SerializeField]
        internal BoolSetting onSceneTooltip = new BoolSetting(true);

        /// <summary>
        /// Swallow left-clicks in the scene view while an ADO tool is active, so that clicking a
        /// handle cannot reselect the object underneath it. Default true.
        /// </summary>
        /// <remarks>
        /// Read at decompiled line 3461 (the click guard) and toggled from the overlay's own button
        /// at 3489-3491, which is also the only place its two icons are chosen between.
        /// </remarks>
        [SerializeField]
        internal BoolSetting ignoreSceneClicks = new BoolSetting(true);

        // ---- Collider testing mode ----

        /// <summary>
        /// Hide Unity's own transform tools while collider testing is running. Default true.
        /// </summary>
        /// <remarks>
        /// Read at decompiled line 6113, which ORs it into <c>Tools.hidden</c> -- so turning the
        /// setting off does not reveal tools something else has hidden.
        /// </remarks>
        [SerializeField]
        internal BoolSetting hideToolsDuringTesting = new BoolSetting(true);

        /// <summary>
        /// The user has acknowledged the collider-testing warning, so it is not shown again.
        /// Default false.
        /// </summary>
        /// <remarks>Read at decompiled line 6301 and set at 6960, when the warning is dismissed.</remarks>
        [SerializeField]
        internal BoolSetting hasReadColliderTestingWarning = new BoolSetting(false);

        // ---- Overlay placement ----

        /// <summary>
        /// Corner of the scene view the tool-selection overlay is anchored to. Default
        /// <see cref="PositionFlag.BottomLeft"/>.
        /// </summary>
        /// <remarks>
        /// Stored as the flag's int; read back with <c>GetEnumValue&lt;PositionFlag&gt;()</c> and fed
        /// to the anchoring helpers in <c>Common/ResizeHandle/</c>. The overlay writes a new value
        /// straight into <see cref="EnumSetting.IntValue"/> when it is dragged.
        /// </remarks>
        [SerializeField]
        internal EnumSetting toolSelectionOverlayAlignment = EnumSetting.FromEnum(PositionFlag.BottomLeft);

        /// <summary>
        /// Corner of the scene view the editing overlay is anchored to. Default
        /// <see cref="PositionFlag.BottomRight"/>.
        /// </summary>
        /// <inheritdoc cref="toolSelectionOverlayAlignment" path="/remarks"/>
        [SerializeField]
        internal EnumSetting toolOverlayAlignment = EnumSetting.FromEnum(PositionFlag.BottomRight);

        // ---- Handle sizing and colours ----

        /// <summary>
        /// Scales every scene handle ADOverhaul draws. Default 1.
        /// </summary>
        /// <remarks>
        /// Applied as a plain multiplier on top of each handle's own base radius (0.05 for bone
        /// handles, 0.25 for PhysBone and collider root handles), so it is a taste control rather
        /// than a hit-target correction.
        /// </remarks>
        [SerializeField]
        internal FloatSetting handleSizeMultiplier = new FloatSetting(1f);

        /// <summary>
        /// Colour of the name labels drawn beside handles. Default opaque white. Only used when
        /// <see cref="onSceneNameLabels"/> is on.
        /// </summary>
        [SerializeField]
        internal ColorSetting labelColor = new ColorSetting(1f, 1f, 1f);

        /// <summary>
        /// Colour of handles that carry no membership meaning -- the endpoint and property-edit
        /// handles. Default opaque white.
        /// </summary>
        [SerializeField]
        internal ColorSetting generalColor = new ColorSetting(1f, 1f, 1f);

        /// <summary>
        /// Membership state 1: the transform or collider is in the edited list. Default a green,
        /// (0.56, 0.94, 0.47). Read through <see cref="stateColors"/>.
        /// </summary>
        [SerializeField]
        internal ColorSetting activeColor = new ColorSetting(0.56f, 0.94f, 0.47f);

        /// <summary>
        /// Membership state 0: not in the edited list. Default a magenta-red, (1, 0, 0.3765).
        /// Read through <see cref="stateColors"/>.
        /// </summary>
        [SerializeField]
        internal ColorSetting inactiveColor = new ColorSetting(1f, 0f, 0.3765f);

        /// <summary>
        /// Membership state 2: in some of the multi-edited targets' lists but not all. Default
        /// orange, (1, 0.65, 0). Read through <see cref="stateColors"/>.
        /// </summary>
        [SerializeField]
        internal ColorSetting mixedColor = new ColorSetting(1f, 0.65f, 0f);

        /// <summary>
        /// Colour of the handle representing the currently selected PhysBone or collider. Default
        /// orange, (1, 0.65, 0) -- the same value as <see cref="mixedColor"/>, but a separate
        /// setting.
        /// </summary>
        [SerializeField]
        internal ColorSetting selectionColor = new ColorSetting(1f, 0.65f, 0f);

        /// <summary>
        /// The user's handle palette in the order a tri-state toggle indexes it: 0 = inactive,
        /// 1 = active, 2 = mixed.
        /// </summary>
        /// <remarks>
        /// Built fresh on every read rather than cached, so it always reflects the current setting;
        /// callers pass it straight to a GUIColorScope and drop it.
        /// </remarks>
        internal Color[] stateColors => new[]
        {
            inactiveColor.value,
            activeColor.value,
            mixedColor.value
        };

        // ---- Update and announcement banner ----
        // Populated from the remote manifest the tool used to fetch; kept because they are the
        // banner's state, not part of the licence check. Every reader is the banner itself.

        /// <summary>URL the update banner links to.</summary>
        [SerializeField]
        internal StringSetting u_updateLink = new StringSetting();

        /// <summary>Version string the update banner advertises.</summary>
        [SerializeField]
        internal StringSetting u_updateVersion = new StringSetting();

        /// <summary>Body text of the update banner.</summary>
        [SerializeField]
        internal StringSetting u_updateMessage = new StringSetting();

        /// <summary>Changelog shown alongside the update banner.</summary>
        [SerializeField]
        internal StringSetting u_updateChangelog = new StringSetting();

        /// <summary>Day the update manifest was last fetched, used to fetch at most once a day.</summary>
        [SerializeField]
        internal StringSetting u_updateDay = new StringSetting();

        /// <summary>Body text of the announcement banner.</summary>
        [SerializeField]
        internal StringSetting u_announcement = new StringSetting();

        /// <summary>URL the announcement banner links to.</summary>
        [SerializeField]
        internal StringSetting u_announcementLink = new StringSetting();

        /// <summary>Label of the announcement banner's link.</summary>
        [SerializeField]
        internal StringSetting u_announcementLinkName = new StringSetting();

        /// <summary>Date of the announcement the user dismissed, so a new one shows again.</summary>
        [SerializeField]
        internal StringSetting u_announcementHiddenDate = new StringSetting();

        /// <summary>The user dismissed the update banner. Default false.</summary>
        [SerializeField]
        internal BoolSetting u_updateHidden = new BoolSetting(false);

        /// <summary>The user dismissed the announcement banner. Default false.</summary>
        [SerializeField]
        internal BoolSetting u_announcementHidden = new BoolSetting(false);
    }
}
