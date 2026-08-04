// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the private static field bank of the outer ADOverhaul class, lines 5578-5744 of the
// current snapshot -- all 84 fields, nothing omitted. Line numbers move with the snapshot; the
// decompiled names below are the durable reference, and every later port of an ADOverhaul member
// should map its field references through this table rather than re-deriving names.
//
// The decompiled type is `internal sealed class ADOverhaul` with an all-static member list and no
// instance constructor of its own. It is reconstructed as `internal static partial class` so the
// remaining ~3,000 lines can be ported a region at a time into sibling files; that is a
// representation change only, and the nested editor types that were lifted out of it (see
// PhysBoneEditor, PhysBoneColliderEditor, ADOverhaulWindow) are unaffected.
//
//   -- PhysBone driver reflection (decompiled 5578-5594) --
//   m_Configuration  -> physBoneReflectionResolved, line 5578
//   _Identifier      -> physBoneManagerLateUpdate,  line 5580
//   context          -> physBoneManagerOnDestroy,   line 5582
//   _Serializer      -> physBoneStart,              line 5584
//   method           -> physBoneOnEnable,           line 5586
//   utils            -> physBoneOnDisable,          line 5588
//   _Page            -> physBoneColliderStart,      line 5590
//   property         -> physBoneColliderOnEnable,   line 5592
//   _Singleton       -> physBoneColliderOnDisable,  line 5594
//
//   -- PhysBone test mode (decompiled 5596-5618, 5654-5664) --
//   _Account         -> isTesting,                       line 5596
//   _Params          -> testPhysBoneManager,             line 5598
//   importer         -> testRoot,                        line 5600
//   m_Server         -> testSourceRoots,                 line 5602
//   watcher          -> selectedObjectsBeforeTest,       line 5604
//   reg              -> activeObjectBeforeTest,          line 5606
//   _Process         -> testPhysBones,                   line 5608
//   m_Val            -> testPhysBoneEnabled,             line 5610
//   m_Adapter        -> testPhysBoneStarted,             line 5612
//   proxy            -> testColliders,                   line 5614
//   m_Comparator     -> testColliderEnabled,             line 5616
//   _Product         -> testColliderStarted,             line 5618
//   helper           -> originalToClone,                 line 5654
//   candidate        -> cloneToOriginal,                 line 5656
//   _Test            -> cloneHasUnappliedChanges,        line 5658
//   m_Stub           -> hasUnappliedTestChanges,         line 5660
//   _Rules           -> colliderChangedDuringTest,       line 5662
//   _Definition      -> hasShownColliderRestartPrompt,   line 5664
//
//   -- Scene-view handle plumbing (decompiled 5620, 5650-5652) --
//   _Iterator        -> handleControlIdBase,        line 5620
//   customer         -> sceneViewPanelResizeHandle, line 5650
//   m_Database       -> tooltipDragControlId,       line 5652
//
//   -- Avatar selection and its derived tables (decompiled 5622-5632) --
//   m_Predicate      -> selectedAvatar,             line 5622
//   _Collection      -> sceneAvatars,               line 5624
//   m_Registry       -> avatarParameterNames,       line 5626
//   m_Client         -> avatarCollisionTags,        line 5628
//   _Observer        -> avatarPlayableLayerNames,   line 5630
//   m_Broadcaster    -> avatarPlayableLayerTypes,   line 5632
//
//   -- Shape-handle toggles (decompiled 5634-5648) --
//   @event           -> unusedShapeFlag,   line 5634
//   record           -> editingRadius,     line 5636
//   _Resolver        -> editingHeight,     line 5638
//   _Tag             -> editingPosition,   line 5640
//   filter           -> editingRotation,   line 5642
//   m_Factory        -> shapeHasRadius,    line 5644
//   _Attribute       -> shapeHasHeight,    line 5646
//   task             -> shapeHasRotation,  line 5648
//
//   -- Bug report and feedback panels (decompiled 5666-5678) --
//   initializer      -> bugReporterOpen,      line 5666
//   getter           -> isSendingBugReport,   line 5668
//   thread           -> bugReportText,        line 5670
//   m_Algo           -> feedbackPanelOpen,    line 5672
//   role             -> isSendingFeedback,    line 5674
//   m_Invocation     -> feedbackText,         line 5676
//
//   -- Licensing (decompiled 5678-5728, defunct; see the note below) --
//   listener         -> licenseUsername,               line 5678
//   _Parser          -> licensedToDisplayName,         line 5680
//   m_Printer        -> licenseVariant,                line 5682
//   m_Repository     -> licenseKey,                    line 5684
//   descriptor       -> transferVerificationCode,      line 5686
//   m_Strategy       -> transferTargetEmail,           line 5688
//   global           -> sessionId,                     line 5690
//   manager          -> serverWarnedTooManyAttempts,   line 5692
//   _Worker          -> licenseKeyEntryRequired,       line 5694
//   indexer          -> licenseCheckRetryOffered,      line 5696
//   m_Pool           -> licenseCheckedThisSession,     line 5698
//   _System          -> retryAllowedAtRealtime,        line 5700
//   setter           -> currentDateStamp,              line 5702
//   _Rule            -> isActivatingLicense,           line 5704
//   _Struct          -> isVerifyingLicense,            line 5706
//   _Interpreter     -> unreadDeviceDateFingerprint,   line 5708
//   attr             -> hardwareId,                    line 5710
//   _Object          -> licenseToken,                  line 5712
//   _Service         -> isLicensed,                    line 5714
//   _Reponse         -> wasLicensedBeforeReset,        line 5716
//   specification    -> licensedCallbacksFlushed,      line 5718
//   m_Wrapper        -> pendingLicensedCallbacks,      line 5720
//   _Info            -> showingTransferPanel,          line 5722
//   m_Config         -> transferCodeSent,              line 5724
//   _Mock            -> isRequestingTransferCode,      line 5726
//   state            -> isConfirmingTransfer,          line 5728
//
//   -- Update and announcement banner (decompiled 5730-5744) --
//   m_Field          -> isDownloadingUpdate,   line 5730
//   m_Advisor        -> isCheckingForUpdate,   line 5732
//   exporter         -> hasCheckedForUpdate,   line 5734
//   creator          -> updateAvailable,       line 5736
//   dispatcher       -> updateFoldout,         line 5738
//   m_Connection     -> announcementFoldout,   line 5740
//   m_Expression     -> version,               line 5742
//   m_Decorator      -> extraMenuLinks,        line 5744
//
// Names were derived from each field's call sites across decompiled lines 164-8696, not from the
// obfuscated identifiers, which are drawn at random from a design-pattern word list and are
// systematically misleading (`m_Expression` is the version number, `customer` is a resize handle,
// `task` is a bool, `_Serializer`/`method`/`utils`/`property` are MethodInfos for four unrelated
// Unity messages). Two fields could not be given a confident role and are documented as such rather
// than guessed at: `@event` and `_Interpreter` -- see their summaries.
//
// LICENSING FIELDS ARE PORTED, THE LICENSING CODE IS NOT. Twenty-six of these fields exist only to
// drive the vendor's activation/verification/transfer flow against a server that no longer answers.
// They are declared here because the members that read them are interleaved with functional code
// throughout the class -- the version banner, the settings pane and several inspector headers all
// sit inside methods that also touch `isLicensed` -- so later agents porting those regions need the
// names to already exist and to agree. Declaring a field commits to nothing; whether any given
// reader is ported is that reader's decision. `hardwareId`, `sessionId` and `licenseKey` in
// particular are only ever *populated* by machine-fingerprinting code (decompiled 5407-5432, 7237,
// 7257) that is not ported and should not be.
//
// Nothing is omitted from this range, and no obfuscator scaffolding was found in it: unlike the
// compiler-generated display classes elsewhere in the file, this bank contains no always-null
// statics paired with null-check predicates. The four `_003C_003Ec__DisplayClass*` structs that
// immediately precede line 5578 are captured-variable artifacts belonging to methods in other
// regions and are not part of this port.
//
// SHIPPED BUG PRESERVED: `tooltipDragControlId` calls GUIUtility.GetControlID from a static field
// initializer, i.e. outside any IMGUI event. Unity has no active control list at that point, so the
// value is whatever the idle GUI state returns rather than a control ID reserved within a layout.
// It is used as a drag-ownership token and compared against hotControl, which works only because
// the same arbitrary number is used on both sides. Ported as-is.
//
// 2019 vs 2022: the same 84 fields in the same order with the same types and the same three
// initialiser values (2019 lines 5559-5725, under a completely different set of obfuscated names).
// No divergence.

using System;
using System.Collections.Generic;
using System.Reflection;
using DreadScripts.Common;
using UnityEditor.AnimatedValues;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// The body of the Avatar Dynamics Overhaul tool: the replacement inspectors' shared drawing
    /// code, the scene-view shape handles, the PhysBone test-mode driver, and the update and
    /// licensing panels that frame them.
    /// </summary>
    /// <remarks>
    /// Every member of the original is static, and almost all of the state below is shared between
    /// the inspectors, the scene-view callbacks and the window rather than owned by any one of them.
    /// This file declares that shared state and nothing else; the members that read and write it are
    /// ported into sibling <c>ADOverhaul.*.cs</c> files.
    /// </remarks>
    internal sealed partial class ADOverhaul
    {
        #region PhysBone driver reflection

        /// <summary>
        /// Whether the <see cref="MethodInfo"/> fields below have been looked up yet. The lookup is
        /// done once, lazily, the first time test mode is entered.
        /// </summary>
        private static bool physBoneReflectionResolved;

        /// <summary>
        /// <c>PhysBoneManager.LateUpdate</c>. Test mode drives the VRChat PhysBone simulation by
        /// hand from an editor update, so it has to call the runtime's private Unity messages
        /// itself; nothing in the SDK exposes them.
        /// </summary>
        private static MethodInfo physBoneManagerLateUpdate;

        /// <summary>
        /// <c>PhysBoneManager.OnDestroy</c>, invoked when test mode tears its temporary scene down.
        /// </summary>
        private static MethodInfo physBoneManagerOnDestroy;

        /// <summary><c>VRCPhysBoneBase.Start</c>.</summary>
        private static MethodInfo physBoneStart;

        /// <summary><c>VRCPhysBoneBase.OnEnable</c>.</summary>
        private static MethodInfo physBoneOnEnable;

        /// <summary><c>VRCPhysBoneBase.OnDisable</c>.</summary>
        private static MethodInfo physBoneOnDisable;

        /// <summary><c>VRCPhysBoneColliderBase.Start</c>.</summary>
        private static MethodInfo physBoneColliderStart;

        /// <summary><c>VRCPhysBoneColliderBase.OnEnable</c>.</summary>
        private static MethodInfo physBoneColliderOnEnable;

        /// <summary><c>VRCPhysBoneColliderBase.OnDisable</c>.</summary>
        private static MethodInfo physBoneColliderOnDisable;

        #endregion

        #region PhysBone test mode

        /// <summary>Whether test mode is currently running.</summary>
        /// <remarks>
        /// Test mode duplicates the selected avatars into a throwaway "Physbone Tester" object and
        /// simulates their PhysBones in the editor, so changes can be previewed without entering
        /// play mode. It is force-disabled while the editor is playing.
        /// </remarks>
        private static bool isTesting;

        /// <summary>
        /// The <see cref="PhysBoneManager"/> added to <see cref="testRoot"/>, which owns the
        /// simulation the editor update pumps by hand.
        /// </summary>
        private static PhysBoneManager testPhysBoneManager;

        /// <summary>
        /// The temporary "Physbone Tester" GameObject holding the duplicated hierarchy. Any
        /// pre-existing object of that name is destroyed first, so a leftover from a crashed session
        /// does not accumulate.
        /// </summary>
        private static GameObject testRoot;

        /// <summary>
        /// The distinct scene roots of the selection that test mode was started from -- the objects
        /// that get duplicated, not the duplicates.
        /// </summary>
        private static GameObject[] testSourceRoots;

        /// <summary>
        /// <c>Selection.gameObjects</c> as it was when test mode started, used to decide which of
        /// the duplicated transforms to re-select and to frame.
        /// </summary>
        private static GameObject[] selectedObjectsBeforeTest;

        /// <summary>
        /// <c>Selection.activeGameObject</c> as it was when test mode started. It supplies the
        /// position for <see cref="testRoot"/> and is restored as the active object on exit.
        /// </summary>
        private static GameObject activeObjectBeforeTest;

        /// <summary>Every PhysBone inside <see cref="testRoot"/>, including inactive ones.</summary>
        private static VRCPhysBone[] testPhysBones;

        /// <summary>
        /// Per-entry mirror of <see cref="testPhysBones"/>' enabled-and-active state as of the last
        /// simulation tick. The driver compares against it to synthesise OnEnable/OnDisable, which
        /// Unity will not send for a component it is not itself running.
        /// </summary>
        private static bool[] testPhysBoneEnabled;

        /// <summary>
        /// Whether <c>Start</c> has already been invoked on the matching entry of
        /// <see cref="testPhysBones"/>. Unity's contract is that Start runs once, before the first
        /// enable, and the driver reproduces that.
        /// </summary>
        private static bool[] testPhysBoneStarted;

        /// <summary>Every PhysBone collider inside <see cref="testRoot"/>, including inactive ones.</summary>
        private static VRCPhysBoneCollider[] testColliders;

        /// <summary>
        /// <see cref="testPhysBoneEnabled"/>'s counterpart for <see cref="testColliders"/>.
        /// </summary>
        private static bool[] testColliderEnabled;

        /// <summary>
        /// <see cref="testPhysBoneStarted"/>'s counterpart for <see cref="testColliders"/>.
        /// </summary>
        private static bool[] testColliderStarted;

        /// <summary>
        /// Maps each original scene object to its duplicate inside <see cref="testRoot"/>. Null
        /// while test mode is not running.
        /// </summary>
        private static Dictionary<UnityEngine.Object, UnityEngine.Object> originalToClone =
            new Dictionary<UnityEngine.Object, UnityEngine.Object>();

        /// <summary>
        /// The reverse of <see cref="originalToClone"/>, so an edit made on a duplicate can be
        /// copied back onto the object it came from.
        /// </summary>
        private static Dictionary<UnityEngine.Object, UnityEngine.Object> cloneToOriginal =
            new Dictionary<UnityEngine.Object, UnityEngine.Object>();

        /// <summary>
        /// Per duplicate, whether it has been edited since the last "Apply All Changes". Keyed by
        /// the duplicate, matching <see cref="cloneToOriginal"/>.
        /// </summary>
        private static Dictionary<UnityEngine.Object, bool> cloneHasUnappliedChanges =
            new Dictionary<UnityEngine.Object, bool>();

        /// <summary>
        /// Whether any entry of <see cref="cloneHasUnappliedChanges"/> is set. Drives the enabled
        /// state and the tint of the "Apply All Changes" button.
        /// </summary>
        private static bool hasUnappliedTestChanges;

        /// <summary>
        /// Set when a collider inspector commits a change during test mode. Collider changes cannot
        /// be picked up by the running simulation, so this triggers the restart prompt.
        /// </summary>
        private static bool colliderChangedDuringTest;

        /// <summary>
        /// Whether the "Testing Restart Required" prompt has already been answered, so it is not
        /// raised again. Seeded from the persisted "don't ask again" setting when test mode starts.
        /// </summary>
        private static bool hasShownColliderRestartPrompt;

        #endregion

        #region Scene-view handle plumbing

        /// <summary>
        /// Base of the control-ID range the scene-view sphere handles allocate from, one ID per
        /// handle at <c>handleControlIdBase + index</c>.
        /// </summary>
        /// <remarks>
        /// A hash of a fixed string rather than a <c>GUIUtility.GetControlID</c> call, because the
        /// IDs have to stay stable across events and across the several code paths that draw into
        /// the same scene view. <c>handleControlIdBase - 1</c> is used as a sentinel hot control to
        /// mean "a handle drag is in progress but no specific handle owns it".
        /// </remarks>
        private static readonly int handleControlIdBase = "ADOControlID".GetHashCode();

        /// <summary>The drag-to-resize grip on the scene-view overlay panel.</summary>
        private static readonly ResizeHandle sceneViewPanelResizeHandle = new ResizeHandle();

        /// <summary>
        /// Control ID owning a drag of the scene-view tooltip panel.
        /// </summary>
        /// <remarks>
        /// Allocated from a static initialiser, which runs outside any IMGUI event, so Unity has no
        /// control list to reserve within and the returned value is not a real layout-relative ID.
        /// This is the shipped behaviour and is preserved: the number is only ever compared against
        /// itself, so its arbitrariness never surfaces.
        /// </remarks>
        private static readonly int tooltipDragControlId =
            GUIUtility.GetControlID("ADOTooltipDragControlID".GetHashCode(), FocusType.Passive);

        #endregion

        #region Avatar selection

        /// <summary>
        /// The avatar the inspectors resolve parameter names, collision tags and playable layers
        /// against. Chosen from <see cref="sceneAvatars"/> by the "Target Avatar" picker.
        /// </summary>
        private static VRCAvatarDescriptor selectedAvatar;

        /// <summary>Every avatar descriptor in the open scenes, refreshed alongside the selection.</summary>
        private static VRCAvatarDescriptor[] sceneAvatars;

        /// <summary>
        /// Animator parameter names gathered from <see cref="selectedAvatar"/>'s non-default
        /// playable layers, minus the VRChat-reserved ones. Feeds the parameter-name dropdown next
        /// to contact receivers' parameter fields.
        /// </summary>
        private static string[] avatarParameterNames;

        /// <summary>
        /// Collision tags in use anywhere under <see cref="selectedAvatar"/>, with VRChat's default
        /// tags folded in under a "Default/" prefix so the dropdown groups them.
        /// </summary>
        private static string[] avatarCollisionTags;

        /// <summary>
        /// Display names of the playable layers <see cref="selectedAvatar"/> actually has a
        /// controller for -- "Base", "Gesture", "FX" and so on.
        /// </summary>
        private static string[] avatarPlayableLayerNames;

        /// <summary>
        /// The <c>VRCAvatarDescriptor.AnimLayerType</c> value matching each entry of
        /// <see cref="avatarPlayableLayerNames"/>, positionally. The two are always rebuilt together.
        /// </summary>
        private static int[] avatarPlayableLayerTypes;

        #endregion

        #region Shape-handle toggles

        /// <summary>
        /// Declared between the avatar tables and the shape toggles, and never read or written
        /// anywhere in either shipped build.
        /// </summary>
        /// <remarks>
        /// Its position in the declaration order puts it with the four editing toggles below, which
        /// suggests a fifth editable shape property that was dropped, but nothing in the assemblies
        /// supports naming it more specifically than this. Kept so the field bank matches the
        /// original one-for-one.
        /// </remarks>
        private static bool unusedShapeFlag;

        /// <summary>Whether the scene-view radius handle is active for the current shape.</summary>
        private static bool editingRadius;

        /// <summary>Whether the scene-view height handle is active for the current shape.</summary>
        private static bool editingHeight;

        /// <summary>Whether the scene-view position handle is active for the current shape.</summary>
        private static bool editingPosition;

        /// <summary>Whether the scene-view rotation handle is active for the current shape.</summary>
        private static bool editingRotation;

        /// <summary>
        /// Whether the shape type being drawn has a radius at all, and so whether
        /// <see cref="editingRadius"/> may be offered. Cleared toggles follow their capability flag
        /// down: turning the capability off also turns the toggle off.
        /// </summary>
        private static bool shapeHasRadius;

        /// <summary>Whether the shape type being drawn has a height. Gates <see cref="editingHeight"/>.</summary>
        private static bool shapeHasHeight;

        /// <summary>
        /// Whether the shape type being drawn is orientable. Gates <see cref="editingRotation"/>.
        /// </summary>
        private static bool shapeHasRotation;

        #endregion

        #region Bug report and feedback panels

        /// <summary>
        /// Whether the shared bug-reporter panel is taking over the inspector. Toggled through a
        /// setter that fires the compilation-started reset when it closes.
        /// </summary>
        private static bool bugReporterOpen;

        /// <summary>Whether a bug report is in flight, which disables the submit button.</summary>
        private static bool isSendingBugReport;

        /// <summary>
        /// The reproduction steps typed into the bug-reporter panel. Truncated to 2000 characters
        /// on submission.
        /// </summary>
        private static string bugReportText;

        /// <summary>Whether the feedback panel is taking over the inspector.</summary>
        private static bool feedbackPanelOpen;

        /// <summary>Whether a feedback message is in flight, which disables the send button.</summary>
        private static bool isSendingFeedback;

        /// <summary>
        /// The message typed into the feedback panel. Truncated to 2000 characters on submission.
        /// </summary>
        private static string feedbackText;

        #endregion

        #region Licensing

        // Everything in this region belongs to the vendor's activation flow, which talked to a
        // server that has been shut down. The fields are declared so that ports of the surrounding
        // GUI can compile and so their names are agreed; see the file header.

        /// <summary>Account name returned by a successful verification. Shown in the settings pane.</summary>
        private static string licenseUsername;

        /// <summary>
        /// The "Authorized For" line under the license banner: <see cref="licenseUsername"/> with a
        /// trailing Discord-style discriminator and any colour markup stripped, and a leading '@'
        /// removed.
        /// </summary>
        private static string licensedToDisplayName;

        /// <summary>
        /// License tier returned by verification. Blank means the tool displays "Personal".
        /// </summary>
        private static string licenseVariant;

        /// <summary>
        /// The license key, cached in EditorPrefs. Validated against a four-group hexadecimal
        /// pattern before it is considered usable.
        /// </summary>
        private static string licenseKey = "";

        /// <summary>The six-digit code entered to confirm a license transfer.</summary>
        private static string transferVerificationCode = "";

        /// <summary>
        /// The email address the transfer code was sent to, echoed back by the server so the panel
        /// can show the user where to look.
        /// </summary>
        private static string transferTargetEmail = "";

        /// <summary>
        /// A per-install identifier persisted in EditorPrefs under "DreadScriptssid" and sent with
        /// every request. Regenerated if the stored value is not a 32-digit hexadecimal GUID.
        /// </summary>
        private static string sessionId;

        /// <summary>
        /// Sticky flag raised when the server reports that the device is close to being blocked for
        /// repeated failures. Once set it stays set for the session and prefixes the wait notice.
        /// </summary>
        private static bool serverWarnedTooManyAttempts;

        /// <summary>
        /// Whether the license panel should ask for a key to be typed in rather than offer a
        /// device check. Set when no usable key is stored and when a check comes back rejected.
        /// </summary>
        private static bool licenseKeyEntryRequired;

        /// <summary>
        /// Whether a device check has already failed once this session, which relabels the check
        /// button "Retry" and lets it through the guard that otherwise suppresses repeat attempts.
        /// </summary>
        private static bool licenseCheckRetryOffered;

        /// <summary>
        /// Whether a verification has been attempted since the domain was loaded, so the
        /// on-display check does not fire on every repaint.
        /// </summary>
        private static bool licenseCheckedThisSession;

        /// <summary>
        /// <c>Time.realtimeSinceStartup</c> before which no further request may be sent, set from
        /// the server's own backoff instruction.
        /// </summary>
        private static float retryAllowedAtRealtime;

        /// <summary>
        /// The current UTC date as a "d/M/yyyy" string with the day and month obfuscated, sent with
        /// requests and compared against the server's date to detect a tampered system clock.
        /// </summary>
        private static string currentDateStamp;

        /// <summary>Whether an activation request is in flight.</summary>
        private static bool isActivatingLicense;

        /// <summary>Whether a verification request is in flight.</summary>
        private static bool isVerifyingLicense;

        /// <summary>
        /// Composed from slices of <see cref="hardwareId"/> and <see cref="currentDateStamp"/> each
        /// time the hardware fingerprint is rebuilt, and then never read: no code in either shipped
        /// build consumes it. What it was for is not recoverable from the assemblies -- the shape
        /// (device identity interleaved with a date) fits a cache key or an offline grace token, but
        /// that is a guess and is not asserted here.
        /// </summary>
        private static string unreadDeviceDateFingerprint;

        /// <summary>
        /// The machine fingerprint sent as "HWID": three SHA-1 hashes of gathered device
        /// properties, joined with dashes.
        /// </summary>
        private static string hardwareId;

        /// <summary>
        /// The token a successful verification returns. It is the input to the inline HMAC check
        /// that the obfuscator scattered through roughly thirty methods as a licensed/unlicensed
        /// test.
        /// </summary>
        private static string licenseToken;

        /// <summary>
        /// Whether the tool considers itself licensed. The coarse gate; the inline HMAC checks are
        /// the fine one.
        /// </summary>
        private static bool isLicensed;

        /// <summary>
        /// Carries the previous value of <see cref="isLicensed"/> across a reset so the reset
        /// handler can tell "was never licensed" from "was licensed and just lost it".
        /// </summary>
        private static bool wasLicensedBeforeReset;

        /// <summary>
        /// Whether <see cref="pendingLicensedCallbacks"/> has already been drained. It is a
        /// one-shot: callbacks queued while unlicensed run once, on the first licensed frame.
        /// </summary>
        private static bool licensedCallbacksFlushed;

        /// <summary>
        /// Work deferred until the tool is licensed. While unlicensed, each caller is removed and
        /// re-added so a given delegate is queued at most once; while licensed, callers run
        /// immediately instead of queueing.
        /// </summary>
        private static Action pendingLicensedCallbacks;

        /// <summary>
        /// Whether the license pane is showing the transfer form rather than the activation form.
        /// </summary>
        private static bool showingTransferPanel;

        /// <summary>
        /// Whether a transfer code has been sent, which is what reveals the code-entry field.
        /// </summary>
        private static bool transferCodeSent;

        /// <summary>Whether the "send me a transfer code" request is in flight.</summary>
        private static bool isRequestingTransferCode;

        /// <summary>Whether the transfer confirmation request is in flight.</summary>
        private static bool isConfirmingTransfer;

        #endregion

        #region Update and announcement banner

        /// <summary>
        /// Whether the update package is downloading, which disables the download button. The
        /// download writes Assets/ADOverhaul.unitypackage and imports it.
        /// </summary>
        private static bool isDownloadingUpdate;

        /// <summary>Whether an update check is in flight.</summary>
        private static bool isCheckingForUpdate;

        /// <summary>
        /// Whether an update check has already completed this session, which suppresses further
        /// automatic checks regardless of the cached-for-today test.
        /// </summary>
        private static bool hasCheckedForUpdate;

        /// <summary>
        /// Whether the cached remote version is newer than <see cref="version"/>. Gates the update
        /// banner and its menu entry.
        /// </summary>
        private static bool updateAvailable;

        /// <summary>Expansion state of the update banner.</summary>
        private static readonly AnimBool updateFoldout = new AnimBool();

        /// <summary>Expansion state of the announcement banner.</summary>
        private static readonly AnimBool announcementFoldout = new AnimBool();

        /// <summary>
        /// The tool's own version, compared against the version the update check reports.
        /// </summary>
        private static readonly SemVer version = new SemVer("0.11.1");

        /// <summary>
        /// Extra (label, URL) entries appended to the tool's dropdown menu. Ships empty in both
        /// builds, so the menu section it would populate never appears; the reading code handles
        /// one entry as a plain item and several as a submenu.
        /// </summary>
        private static readonly (string, string)[] extraMenuLinks = new (string, string)[0];

        #endregion
    }
}
