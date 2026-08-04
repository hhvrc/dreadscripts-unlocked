// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// Ported region: the settings fields of the nested `EditorSettings` class, lines 1151-1457.
//
// Every field keeps its decompiled name -- these names are the on-disk contract. They are what
// JsonUtility writes into the MAIN envelope, and for the two [NonSerializedSetting] fields they are
// the literal key of that field's own envelope entry. Renaming any of them silently discards the
// user's stored value.
//
//   parameterLabelStyle          -> parameterLabelStyle,          line 1389
//   RebuildParameterLabelStyle() -> RebuildParameterLabelStyle,   line 1439
//   a_VerifyOnDisplay -> NOT PORTED, line 1151 -- licensing-gate remnant, see EditorSettings.cs
//   a_VerifyOnProjectLoad -> NOT PORTED, line 1154 -- licensing-gate remnant, see EditorSettings.cs
//   a_HasSucceededLastVerification -> NOT PORTED, line 1437 -- licensing-gate remnant, see EditorSettings.cs
//
// Every other field of the region keeps its decompiled name and its initialiser; the only other
// change of shape is uniform across them:
//   [SpecialName] GetValue()/SetValue() on each setting -> the Common port's `value` property
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// Audit status: PARTIAL -- the five MAP entries above were re-checked against
// decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs and each lands on
// the member named. The remaining settings fields of the region were not re-diffed field by field.

using System;
using UnityEditor.Animations;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
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
    /// The two exceptions are <see cref="defaultState"/> and <see cref="defaultTransition"/>, which
    /// are scene/asset objects rather than values and are marked
    /// <see cref="NonSerializedSettingAttribute"/> so that they get their own
    /// <see cref="UnityEditor.EditorJsonUtility"/> entry inside the same EditorPrefs string.
    /// </para>
    /// <para>
    /// Each field's initialiser carries its default. That initialiser is also what supplies the
    /// default when a setting is missing from the stored block -- see <see cref="SettingBase"/> --
    /// so a setting added in a later version appears at its default without invalidating the rest.
    /// </para>
    /// </remarks>
    internal partial class EditorSettings
    {
        // ---- Multi-edit scope: which parts of the selection the window edits ----

        /// <summary>Edit the selected transitions. Default true.</summary>
        [SerializeField]
        internal BoolSetting editingTransitions = new BoolSetting(true);

        /// <summary>Edit the selected states. Default false.</summary>
        [SerializeField]
        internal BoolSetting editingStates = new BoolSetting(false);

        /// <summary>Edit the controller itself. Default false.</summary>
        [SerializeField]
        internal BoolSetting editingController = new BoolSetting(false);

        // ---- Which fields of a condition have to agree for two conditions to be edited as one ----

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting matchParameter = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting matchMode = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting matchValue = new BoolSetting(true);

        // ---- Window sections ----

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting showTransitionSettings = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting showTransitionConditions = new BoolSetting(true);

        /// <summary>
        /// Show the match-parameter/mode/value toggles. Default false.
        /// </summary>
        /// <remarks>
        /// Changing this regroups the conditions, so it rebuilds the condition multi-editors --
        /// see <see cref="onMatchingOptionsChanged"/>.
        /// </remarks>
        [SerializeField]
        internal BoolSetting showMatchingOptions = new BoolSetting(false, () => onMatchingOptionsChanged?.Invoke());

        /// <summary>Default false.</summary>
        [SerializeField]
        internal BoolSetting showTransitionsCount = new BoolSetting(false);

        /// <summary>Default false.</summary>
        [SerializeField]
        internal BoolSetting showStateSettings = new BoolSetting(false);

        /// <summary>Default false.</summary>
        [SerializeField]
        internal BoolSetting showStateCount = new BoolSetting(false);

        /// <summary>Show the VRChat parameter-driver section. Default false.</summary>
        [SerializeField]
        internal BoolSetting showVRCDrivers = new BoolSetting(false);

        /// <summary>Show the VRChat tracking-control section. Default false.</summary>
        [SerializeField]
        internal BoolSetting showVRCTracking = new BoolSetting(false);

        // ---- Interaction ----

        /// <summary>
        /// Use Unity's plain popup for parameter pickers instead of the searchable one. Default false.
        /// </summary>
        [SerializeField]
        internal BoolSetting useLegacyDropdown = new BoolSetting(false);

        /// <summary>Default false.</summary>
        [SerializeField]
        internal BoolSetting switchDoubleClick = new BoolSetting(false);

        /// <summary>
        /// Flip a condition's mode as well as its value when reversing it. Default true.
        /// </summary>
        [SerializeField]
        internal BoolSetting autoReverseModes = new BoolSetting(true);

        /// <summary>
        /// Also change condition values, not only modes, when reversing. Default false.
        /// </summary>
        /// <remarks>
        /// Read by the condition inverter -- <c>ConditionMultiEditor.Invert</c> -- to decide whether
        /// inverting a threshold comparison should nudge the threshold too.
        /// </remarks>
        [SerializeField]
        internal BoolSetting reverseModifiesValues = new BoolSetting(false);

        /// <summary>Default false.</summary>
        [SerializeField]
        internal BoolSetting animateInboundEdges = new BoolSetting(false);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting animateOutboundEdges = new BoolSetting(true);

        /// <summary>Frame the graph on a layer when it is selected. Default true.</summary>
        [SerializeField]
        internal BoolSetting autoFrameLayer = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting displayLayerIndex = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting displayParameterType = new BoolSetting(true);

        /// <summary>
        /// Draw the parameter-type indicator as a capital letter rather than lower case. Default false.
        /// </summary>
        [SerializeField]
        internal BoolSetting capitalParameterIndicator = new BoolSetting(false);

        // ---- Animation window integration (aw_) ----

        /// <summary>Master switch for the Animation window integration. Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_active = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_autoSwitchClip = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_enablePropertyEditing = new BoolSetting(true);

        /// <summary>Accept GameObjects dropped onto the Animation window. Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_enableGameObjectDND = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_enableOverride = new BoolSetting(true);

        /// <summary>Warn before merging properties into an existing clip. Default true.</summary>
        [SerializeField]
        internal BoolSetting aw_warnPropertyMerge = new BoolSetting(true);

        // ---- Animator graph cosmetics ----

        /// <summary>
        /// Use <see cref="graphBackgroundTexture"/> rather than <see cref="gridBackgroundColor"/> for
        /// the graph background. Default false.
        /// </summary>
        [SerializeField]
        internal BoolSetting graphBackgroundIsTexture = new BoolSetting(false, () => onGraphBackgroundChanged?.Invoke());

        /// <summary>Master switch for the graph background restyling. Default false.</summary>
        [SerializeField]
        internal BoolSetting cosmeticGraphActive = new BoolSetting(false, () => onGraphBackgroundChanged?.Invoke());

        /// <summary>Master switch for the node recolouring. Default false.</summary>
        [SerializeField]
        internal BoolSetting cosmeticNodesActive = new BoolSetting(false, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Master switch for the transition recolouring. Default false.</summary>
        [SerializeField]
        internal BoolSetting cosmeticTransitionsActive = new BoolSetting(false, () => onGraphRebuildRequested?.Invoke());

        // ---- One-shot state flags ----

        /// <summary>
        /// Whether the tool has already pinged the controller asset once for this user. Default false.
        /// </summary>
        [SerializeField]
        internal BoolSetting hasPingedController = new BoolSetting(false);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting requiresStateRename = new BoolSetting(true);

        // ---- Quick Toggle ----

        /// <summary>
        /// Quick Toggle opens in advanced mode rather than simple. Default false.
        /// </summary>
        /// <remarks>Read by <c>QuickToggleWindow</c> as its advanced/simple mode.</remarks>
        [SerializeField]
        internal BoolSetting advancedQuickToggle = new BoolSetting(false);

        /// <summary>
        /// Quick Toggle merges into the existing layer rather than replacing it. Default true.
        /// </summary>
        /// <remarks>Read by <c>QuickToggleWindow</c> as its merge-vs-replace choice.</remarks>
        [SerializeField]
        internal BoolSetting mergeQuickToggle = new BoolSetting(true);

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting warnParameterConversion = new BoolSetting(true);

        // ---- Layer list ----

        /// <summary>
        /// Group layers into categories in the layer list. Default true.
        /// </summary>
        /// <remarks>
        /// SHIPPED BEHAVIOUR, preserved: the callback runs on every change, not only when the
        /// category view is switched off, so turning the category view *on* also clears
        /// <see cref="sortCategoryViewLayers"/> and resets the layer list to its default view. The
        /// same is true of <see cref="displayLayerCompactView"/> below.
        /// </remarks>
        [SerializeField]
        internal BoolSetting displayCategoryView = new BoolSetting(true, delegate
        {
            Instance.sortCategoryViewLayers.value = false;
            onCategoryViewReset?.Invoke();
        });

        /// <summary>Default true.</summary>
        [SerializeField]
        internal BoolSetting sortCategoryViewLayers = new BoolSetting(true);

        /// <summary>
        /// Offer the compact layer row. Default true. Turning this off also clears
        /// <see cref="layerCompactView"/> -- and so does turning it on, see
        /// <see cref="displayCategoryView"/>.
        /// </summary>
        [SerializeField]
        internal BoolSetting displayLayerCompactView = new BoolSetting(true, delegate
        {
            Instance.layerCompactView.value = false;
        });

        /// <summary>Draw layer rows compactly. Default false.</summary>
        [SerializeField]
        internal BoolSetting layerCompactView = new BoolSetting(false, () => onLayerListLayoutChanged?.Invoke());

        // ---- Node colours ----
        // Stored as floats although they are indices into the Animator window's style palette; the
        // framework has no int setting, and EnumSetting -- which is the float setting with an int
        // face -- was not used here.

        /// <summary>Palette index for Any State nodes. Default 2.</summary>
        [SerializeField]
        internal FloatSetting anyStateNodeColor = new FloatSetting(2f, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Palette index for Entry nodes. Default 3.</summary>
        [SerializeField]
        internal FloatSetting entryStateNodeColor = new FloatSetting(3f, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Palette index for Exit nodes. Default 6.</summary>
        [SerializeField]
        internal FloatSetting exitStateNodeColor = new FloatSetting(6f, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Palette index for sub-state-machine nodes. Default 0.</summary>
        [SerializeField]
        internal FloatSetting machineStateNodeColor = new FloatSetting(0f, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Palette index for ordinary state nodes. Default 0.</summary>
        [SerializeField]
        internal FloatSetting normalStateNodeColor = new FloatSetting(0f, () => onGraphRebuildRequested?.Invoke());

        /// <summary>Palette index for the layer's default state. Default 5.</summary>
        [SerializeField]
        internal FloatSetting defaultStateNodeColor = new FloatSetting(5f, () => onGraphRebuildRequested?.Invoke());

        // ---- Defaults applied to newly created animator content ----

        /// <summary>Weight given to a newly added layer. Default 1.</summary>
        [SerializeField]
        internal FloatSetting defaultLayerWeight = new FloatSetting(1f);

        /// <summary>
        /// Where along a transition edge its arrow sits. Default -0.5.
        /// </summary>
        /// <remarks>The negative default is the shipped value; it is measured from the far end.</remarks>
        [SerializeField]
        internal FloatSetting arrowLerpRatio = new FloatSetting(-0.5f);

        /// <summary>Graph position for a new layer's Entry node. Default (50, 120).</summary>
        [SerializeField]
        internal VectorSetting defaultEntryPosition = new VectorSetting(50f, 120f);

        /// <summary>Graph position for a new layer's Exit node. Default (800, 120).</summary>
        [SerializeField]
        internal VectorSetting defaultExitPosition = new VectorSetting(800f, 120f);

        /// <summary>Graph position for a new layer's Any State node. Default (50, 20).</summary>
        [SerializeField]
        internal VectorSetting defaultAnyPosition = new VectorSetting(50f, 20f);

        // ---- Transition and grid colours ----

        /// <summary>Default opaque white.</summary>
        [SerializeField]
        internal ColorSetting normalTransitionColor = new ColorSetting(1f, 1f, 1f);

        /// <summary>Default (0.6, 0.4, 0).</summary>
        [SerializeField]
        internal ColorSetting entryTransitionColor = new ColorSetting(0.6f, 0.4f, 0f);

        /// <summary>Default (0.42, 0.7, 1).</summary>
        [SerializeField]
        internal ColorSetting selectedTransitionColor = new ColorSetting(0.42f, 0.7f, 1f);

        /// <summary>Default mid grey.</summary>
        [SerializeField]
        internal ColorSetting baseTransitionColor = new ColorSetting(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// Graph background fill when <see cref="graphBackgroundIsTexture"/> is off. Default
        /// (0.1647, 0.1647, 0.16), which is the stock Animator window grey.
        /// </summary>
        [SerializeField]
        internal ColorSetting gridBackgroundColor = new ColorSetting(0.1647f, 0.1647f, 0.16f, 1f, () => onGraphBackgroundChanged?.Invoke());

        /// <summary>Minor grid line on a light background. Default black at 10% alpha.</summary>
        [SerializeField]
        internal ColorSetting gridMinorLightColor = new ColorSetting(0f, 0f, 0f, 0.1f);

        /// <summary>Major grid line on a light background. Default black at 15% alpha.</summary>
        [SerializeField]
        internal ColorSetting gridMajorLightColor = new ColorSetting(0f, 0f, 0f, 0.15f);

        /// <summary>Minor grid line on a dark background. Default black at 18% alpha.</summary>
        [SerializeField]
        internal ColorSetting gridMinorDarkColor = new ColorSetting(0f, 0f, 0f, 0.18f);

        /// <summary>Major grid line on a dark background. Default black at 28% alpha.</summary>
        [SerializeField]
        internal ColorSetting gridMajorDarkColor = new ColorSetting(0f, 0f, 0f, 0.28f);

        /// <summary>Default (0.7, 0.7, 0.7).</summary>
        [SerializeField]
        internal ColorSetting parameterLabelColor = new ColorSetting(0.7f, 0.7f, 0.7f);

        // ---- Asset references ----

        /// <summary>
        /// Mask applied to a newly created layer. Default none.
        /// </summary>
        /// <remarks>Stored as a GUID plus local file id, not as a path -- see
        /// <see cref="ObjectReferenceSetting"/>.</remarks>
        [SerializeField]
        internal ObjectReferenceSetting defaultLayerMask = new ObjectReferenceSetting(typeof(AvatarMask));

        /// <summary>Texture used as the graph background. Default none.</summary>
        [SerializeField]
        internal ObjectReferenceSetting graphBackgroundTexture = new ObjectReferenceSetting(typeof(Texture2D), "", 0L, () => onGraphBackgroundChanged?.Invoke());

        // ---- Paths and names ----

        /// <summary>
        /// Folder every asset the tool generates is written to. Default
        /// <c>Assets/DreadScripts/ControllerEditor/Generated Assets</c>.
        /// </summary>
        /// <remarks>Read by <c>QuickToggleWindow</c> when it writes its generated clips.</remarks>
        [SerializeField]
        internal StringSetting saveFolder = new StringSetting("Assets/DreadScripts/ControllerEditor/Generated Assets");

        /// <summary>Folder last used by the new-animation-clip prompt. Default <c>Assets</c>.</summary>
        [SerializeField]
        internal StringSetting lastAnimationPath = new StringSetting("Assets");

        /// <summary>Name last used by the new-animation-clip prompt. Default <c>New Animation Clip</c>.</summary>
        [SerializeField]
        internal StringSetting lastAnimationName = new StringSetting("New Animation Clip");

        /// <summary>
        /// Category a layer falls into when its name carries no category prefix. Default <c>Base</c>.
        /// </summary>
        /// <remarks>Read by <c>LayerPathNode</c> as the root category name.</remarks>
        [SerializeField]
        internal StringSetting categoryBaseName = new StringSetting("Base");

        /// <summary>
        /// Separator that splits a layer name into category path segments. Default <c>/</c>.
        /// </summary>
        /// <remarks>Read by <c>LayerPathNode</c> when it splits a layer name into its path.</remarks>
        [SerializeField]
        internal StringSetting categoryDelimiter = new StringSetting("/");

        // ---- Enum-backed ----

        /// <summary>
        /// Font style of the parameter labels drawn on the graph. Default
        /// <see cref="FontStyle.Normal"/>.
        /// </summary>
        [SerializeField]
        internal EnumSetting parameterLabelFontStyle = EnumSetting.FromEnum(FontStyle.Normal, RebuildParameterLabelStyle);

        /// <summary>
        /// Which state decorations are drawn, as <see cref="StateCosmeticOptions"/> flags. Default
        /// <see cref="StateCosmeticOptions.all"/>. Read through <see cref="GetStateCosmetics"/>.
        /// </summary>
        [SerializeField]
        internal EnumSetting stateCosmetics = EnumSetting.FromEnum(StateCosmeticOptions.all);

        // ---- Object-valued settings, persisted separately ----

        /// <summary>
        /// The state whose settings a newly created state is copied from, or null for Unity's own
        /// defaults.
        /// </summary>
        /// <remarks>
        /// Not a <see cref="SettingBase"/> at all but a raw object, so it cannot ride in the main
        /// JSON block; <see cref="NonSerializedSettingAttribute"/> gives it its own entry, keyed by
        /// the field name. Read by <c>QuickToggleWindow</c> when it creates states.
        /// </remarks>
        [NonSerializedSetting]
        internal AnimatorState defaultState;

        /// <inheritdoc cref="defaultState"/>
        [NonSerializedSetting]
        internal AnimatorStateTransition defaultTransition;

        // ---- Update and announcement banner ----
        // Populated from the remote manifest the tool used to fetch; kept because they are the
        // banner's state, not part of the licence check.

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

        // ---- Derived, not persisted ----

        /// <summary>
        /// The style parameter labels are drawn with, rebuilt whenever
        /// <see cref="parameterLabelFontStyle"/> changes.
        /// </summary>
        /// <remarks>
        /// A <see cref="GUIStyle"/> cannot survive the settings round-trip, so it is derived rather
        /// than stored. It is null until <see cref="RebuildParameterLabelStyle"/> has run at least
        /// once, which the shipped build arranges by calling it during window setup -- changing the
        /// setting alone is not enough to create it.
        /// </remarks>
        [NonSerialized]
        internal static GUIStyle parameterLabelStyle;

        internal static void RebuildParameterLabelStyle()
        {
            parameterLabelStyle = new GUIStyle(EditorUtils.styles.noteRight)
            {
                fontStyle = Instance.parameterLabelFontStyle.GetEnumValue<FontStyle>()
            };
        }
    }
}
