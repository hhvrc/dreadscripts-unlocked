// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Assigned region: decompiled lines 7153-7998 -- the tool's licensing, activation, transfer,
// machine-fingerprinting and request-signing layer, plus a handful of unrelated general-purpose
// members the obfuscator happened to lay down in the middle of it.
//
// Only that handful is ported. Forty members live in the range; two are reconstructed below, four
// were reconstructed elsewhere, one belongs to another region's port, and the remaining thirty-three
// are deliberately omitted and catalogued in full further down. Line numbers move with the snapshot;
// the decompiled names are the durable reference. Field references go through the table in
// ADOverhaul.State.cs.
//
//   RemoveSerializer     -> BugReporterOpen (set-only property), line 7212
//   InitConfiguration    -> DrawPanelHeader,                     line 7835
//
// Ported elsewhere, deliberately NOT repeated here. These are cross-references, not claims: each of
// the four is mapped with its line number by the file that ports it, so it stays claimed once:
//
//   IncludeConfiguration -> Json.ToJsonObject,   Editor/Common/Json.cs                (line 7924)
//   CalculateIdentifier  -> RepaintOpenWindowsDelayed, ADOverhaul.Menus.cs             (line 7974)
//   CalcIdentifier       -> RepaintOpenWindows,        ADOverhaul.Menus.cs             (line 7979)
//   DeleteIdentifier     -> DrawCreditLink,            ADOverhaul.Menus.cs             (line 7988)
//
// The last three are the tail of this range but were adopted by ADOverhaul.Menus.cs, which holds
// every surviving caller -- it explains the adoption in its own header. None of them touches the
// licence or the network: two are repaint helpers and one draws the author credit link. They are
// live and trivial; call them there rather than adding a second copy.
//
// ==================================================================================================
// WHY ALMOST NONE OF THIS RANGE IS PORTED
// ==================================================================================================
//
// Every omitted member below is part of one machine: a licence check that POSTs to
//
//   https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand
//
// and refuses to draw the tool's inspectors unless that POST comes back with a token the client can
// reproduce under an embedded HMAC key. The host is null-routed -- it resolves to 0.0.0.0 / :: --
// so the check can now only ever fail. See the DATA TRANSMISSION AUDIT at the top of
// Editor/Common/BugReporter.cs for the full endpoint analysis, including the warning that a naive
// HTTP probe of the endpoint may return a *fabricated* 200 from an intercepting proxy; DNS is the
// only trustworthy signal.
//
// This package exists to restore the tool for people who bought it. Reconstructing a gate that
// cannot succeed would simply reimplement the shutdown. So the gate is not reconstructed, and
// neither is anything whose only purpose is to feed it. Nothing is stubbed, nothing is faked, and
// no member below is replaced by an always-true variant: they are absent. Code elsewhere in the
// package that called into them is, correspondingly, ported without its licence branch -- see the
// same decision already recorded in PhysBoneEditor.cs, PhysBoneColliderEditor.cs and
// ADOverhaulWindow.cs.
//
// The catalogue is a deliverable in its own right. This repository documents what the DRM did.
//
// ==================================================================================================
// NOT PORTED -- CATALOGUE OF OMITTED MEMBERS, decompiled lines 7153-7998
// Each entry is <decompiled name> <decompiled line> followed by prose; they are catalogued rather
// than mapped because none of them is ported.
// ==================================================================================================
//
// -- Hardware fingerprinting (spawns child processes to read device serial numbers) ---------------
//
//   CloneConfiguration    7469  The fingerprinter, and the single most invasive thing in the
//                               assembly. Takes a continuation and runs it once a machine
//                               fingerprint is available. It builds four `ProcessRunner`s over
//                               `wmic baseboard get *`, `wmic cpu get *`, `wmic diskdrive get *` and
//                               `wmic memorychip get *`, and a parallel set of four PowerShell
//                               fallbacks (`Get-CimInstance -class Win32_baseboard | Select *` and
//                               the processor / diskdrive / physicalmemory equivalents) for when
//                               wmic is unavailable -- eight potential child processes, under a
//                               10-second CancellationTokenSource. From the output it keeps
//                               Manufacturer / Product / SerialNumber (motherboard), ProcessorId,
//                               SerialNumber (disk) and Manufacturer / PartNumber / SerialNumber /
//                               Capacity (RAM), SHA-1s them into components and joins them with
//                               dashes into `hardwareId`, cached in EditorPrefs under "DSLICINF".
//                               A stable, machine-unique identifier derived from hardware serials.
//   RestartConfiguration  7433  Interleaves slices of `hardwareId` with the day/month/year fields of
//                               the date stamp into `unreadDeviceDateFingerprint` -- a field
//                               nothing in either shipped build ever reads (see its summary in
//                               ADOverhaul.State.cs).
//   ManageConfiguration   7441  Generates and persists the install identifier `sessionId` in
//                               EditorPrefs under "DreadScriptssid", regenerating it unless the
//                               stored value matches [0-9a-f]{32}.
//
// -- The licence gate itself -----------------------------------------------------------------------
//
//   FlushConfiguration    7719  THE gate. Every inspector and the tool window open with
//                               `if (FlushConfiguration())`. Returns true only when `isLicensed`;
//                               otherwise it draws the activation pane and returns false, so the
//                               real UI is never reached. Reconstructing it would re-gate the tool
//                               on a dead server. Callers: ADOverhaulWindow.OnGUI (74),
//                               ContactReceiverEditor (1735), ContactSenderEditor (1991),
//                               PhysBoneColliderEditor (2192), PhysBoneEditor (3214).
//   EnableConfiguration   7170  The unlicensed branch of those five callers: a warning HelpBox
//                               ("This is 'Avatar Dynamics Overhaul'. If you don't know what this
//                               is...") over Locate / Info / Switch Editor buttons. Reachable only
//                               when the gate has refused, i.e. never in this package. Mechanically
//                               portable -- every helper it needs is in place -- but it is the
//                               refusal screen, so it is left out with the refusal.
//   ReflectConfiguration  7647  `RunWhenLicensed(Action)`: queue the action on
//                               `pendingLicensedCallbacks` while unlicensed, or run it immediately
//                               if an inline HMAC-SHA256 over `licenseKey` / `currentDateStamp` /
//                               `hardwareId` reproduces `licenseToken`. NOTE, correcting an earlier
//                               reading: it has NO callers in either shipped build, so
//                               `pendingLicensedCallbacks` is only ever drained, never filled. Dead
//                               code even in the original.
//   ResolveConfiguration  7664  The drain: the same inline HMAC check, then a one-shot invoke of
//                               `pendingLicensedCallbacks` guarded by `licensedCallbacksFlushed`.
//                               Given the above it always invokes an empty delegate.
//   ResetConfiguration    7680  Empty body, one unused bool parameter, four call sites. Obfuscator
//                               scaffolding or a stripped hook; there is nothing to port.
//
// -- Licence state, entry points and request flows --------------------------------------------------
//
//   DisableConfiguration  7268  [InitializeOnLoadMethod]. Fires on every domain reload: reads the
//                               stored key and, if `a_VerifyOnProjectLoad` is set, queues a
//                               verification POST. This is the automatic network call on project
//                               open. (It was missing from the region list I was given.)
//   VisitConfiguration    7285  The per-repaint counterpart: if `a_VerifyOnDisplay` is set and a key
//                               is stored, verify. Called from FlushConfiguration on Repaint.
//   AssetConfiguration    7293  The verification flow proper. First tries a cached response held in
//                               SessionState under a key of product id + EditorAnalyticsSessionInfo.id,
//                               decrypted with a hardcoded AES-128 key and IV and authenticated with
//                               HMAC-SHA1 keyed on that same session key; on a hit it restores
//                               `licenseUsername`, `licenseVariant`, `licenseToken` and `hardwareId`
//                               and marks the tool licensed. On a miss it calls CloneConfiguration
//                               (above) to fingerprint the machine and POSTs "verifylicense".
//   PopConfiguration      7380  Activation: validates the typed key, fingerprints the machine, POSTs
//                               "activatelicense", then re-runs verification.
//   ExcludeConfiguration  7782  The licence-transfer pane -- "send me a 6-digit code", the code
//                               field, and the transfer button.
//   ConnectConfiguration  7848  The licence-key text field, with its focus/commit handling.
//   FindConfiguration     7881  Whether a request may be sent right now: format valid, not inside
//                               the server-imposed backoff, and the transfer code present if the
//                               transfer pane is open.
//   AddConfiguration      7898  Format check, `^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$`.
//   ValidateConfiguration 7903  Format check for the 6-character transfer code.
//   CreateConfiguration   7912  The "Activate License" / "Transfer License" pane switcher.
//   GetConfiguration      7699  The licence banner: "License: <tier or Personal>" and
//                               "Authorized For: <name>". Drawn by the window and by
//                               PhysBoneEditor/PhysBoneColliderEditor, but purely a readout of
//                               licence state, so there is nothing for it to display here.
//   InstantiateConfiguration 7409 Formats `licensedToDisplayName` from the account name the server
//                               returned -- strips a trailing Discord "#1234" discriminator, any
//                               <color> markup around it and a leading '@'. Fed only by the
//                               response, so it has no input in this package.
//   RateConfiguration     7455  Loads the key from EditorPrefs ("No1lKII9IzcBAbihub6nCg==LK") and
//                               reports whether a well-formed one is stored.
//   ResolveSerializer     7256  [SpecialName] getter: seconds remaining in the server-imposed
//                               backoff (`retryAllowedAtRealtime - Time.realtimeSinceStartup`).
//   GetSerializer         7262  [SpecialName] getter: whether that backoff is still running.
//   ExcludeSerializer     7685  [SpecialName] getter composing the wait notice -- "Too many failed
//                               attempts! Further failed attempts will result in getting your device
//                               blocked!" plus "Please wait N seconds." All three exist only to
//                               throttle and warn about requests to the dead endpoint.
//
// -- Request construction, signing and transport ----------------------------------------------------
//
//   CountConfiguration    7606  Builds the identity block every command carries: command,
//                               product_id ("No1lKII9IzcBAbihub6nCg=="), version, HWID, SID,
//                               license_key, plus any command-specific pairs.
//   StartConfiguration    7625  Appends the tamper-evidence "hash": HMAC-SHA256 over the
//                               concatenated values under a 128-character key embedded in the
//                               assembly.
//   RemoveConfiguration   7638  Builds `currentDateStamp` as "<day>/<month>/<year>" UTC with the day
//                               and month passed through the obfuscation helper, for the server's
//                               clock-tampering check.
//   RevertConfiguration   7941  HttpWebRequest factory: POST, application/json.
//   RunIdentifier         7950  Writes the payload and reads the response on a background task,
//                               parsing it into a JsonObject.
//   OrderIdentifier       7969  Hardcodes the dead endpoint URL. This is the single line every
//                               request in the whole assembly funnels through.
//   ComputeConfiguration  7555  Response handler, thin wrapper over the below.
//   QueryConfiguration    7560  The response handler proper: reads success / message / url /
//                               url_name / wait_warn / wait_time, logs or dialogs the message,
//                               optionally opens the returned URL, and arms the backoff.
//
// -- Ported elsewhere, or belonging to another region ------------------------------------------------
//
//   IncludeConfiguration  7924  Confirmed already ported: it is one of four byte-identical
//                               `(string, string)`-pair-to-JSON serialisers the obfuscator duplicated
//                               across the two products, consolidated as `Json.ToJsonObject` in
//                               Editor/Common/Json.cs (which records all four originals, this one
//                               among them). Not duplicated here.
//   CalculateIdentifier   7974  \
//   CalcIdentifier        7979   > See the mapping at the top: adopted by ADOverhaul.Menus.cs.
//   DeleteIdentifier      7988  /
//   InsertConfiguration   7153  The PhysBone test-mode "collider changes require a restart" prompt.
//                               Nothing to do with licensing despite sitting at the head of this
//                               range; owned by the test-mode region.
//   AwakeConfiguration    7222  The feedback panel. Its layout is ordinary, but its only action is a
//                               "sendfeedback" POST through CountConfiguration / StartConfiguration
//                               / OrderIdentifier, so it cannot be ported without the transport.
//                               Worth recording for whoever revisits it: its second statement is
//                               `feedbackPanelOpen = isLicensed`, which means the panel closes
//                               itself on the first frame it draws unless the tool is licensed --
//                               the feedback form is gated too.
//
// ==================================================================================================
// WHAT IS PORTED, AND WHY IT IS SAFE
// ==================================================================================================
//
// The two members below touch no licence state and reach no network. Each is used by code outside
// the licensing flow, which is the reason it was worth separating them out:
//
//   BugReporterOpen  the bug reporter's open/close flag, whose setter runs the reporter's reset on
//                    close. Its callers are the reporter's own IMGUI surface (decompiled 268-474),
//                    which is the half of BugReporter that Editor/Common/BugReporter.cs records as
//                    not yet reconstructible; this setter is ported ahead of them so that when that
//                    surface lands it has the transition behaviour to call into.
//   DrawPanelHeader  the boxed title-and-tooltip row that every takeover panel opens with. Six
//                    callers: the five licence panels above, and the bug reporter at line 361.
//
// Neither has a ported caller yet, and neither is referenced by the omitted code -- they are here
// because they are the only general-purpose members the obfuscator left inside this range, and
// finding them again would otherwise mean re-auditing the whole licensing layer.
//
// Both keep the `private` visibility the source gave them, which was sufficient there because
// BugReporter was a type nested inside ADOverhaul. In this package BugReporter has been lifted out
// to DreadScripts.Common, so whoever reconstructs its IMGUI surface will have to widen these two to
// `internal` (and, for DrawPanelHeader, decide whether it belongs to ADOverhaul at all, since
// ControllerEditor ships the same row). That is a one-word change and is left to that port rather
// than pre-empted here.
//
// 2019 vs 2022: no divergence in either. The 2019 build carries them at lines 6985
// (CollectTemplate) and 7613 (EnableSystem), character-for-character identical under different
// obfuscated names. The omitted members are likewise the same machine in both builds, down to the
// endpoint URL (2019 line 7752) and the wmic command strings.
//
// No ILSpy artifacts were found in the ported pair: no spurious `while (true)`, and the two
// [SpecialName] members here that really were accessors are handled as such -- RemoveSerializer is
// restored as a property setter below, while ResolveSerializer / GetSerializer / ExcludeSerializer
// are accessors of the omitted licence state and are omitted with it.
//
// Audit status: PARTIAL -- every decompiled line number above was re-located by name in
// decompiled/ADOverhaul2022/.../ADOverhaul.cs on this pass and corrected: the whole region had
// shifted by ~204 lines in the 561e9ec re-snapshot (RemoveSerializer 7007 -> 7212, InitConfiguration
// 7631 -> 7835, the region itself 6949-7794 -> 7153-7998, and every catalogue entry likewise), as
// had the FlushConfiguration caller in PhysBoneEditor (3010 -> 3214). What was NOT re-audited is
// the prose: the described behaviour of the omitted members, and the 2019-build line numbers quoted
// below, are as first written. The bodies were not re-diffed against decompiled/, so this is PARTIAL rather than VERIFIED.

using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Opens or closes the bug reporter's takeover panel, clearing the reporter's captured error
        /// when it closes.
        /// </summary>
        /// <remarks>
        /// Write-only, as it was in the shipped build: readers go straight to
        /// <see cref="bugReporterOpen"/>. The reset has to hang off the transition rather than off
        /// the assignment, because the panel re-asserts its own open state every frame it draws --
        /// only the true-to-false edge means the user actually dismissed it.
        /// </remarks>
        private static bool BugReporterOpen
        {
            set
            {
                bool wasOpen = bugReporterOpen;
                bugReporterOpen = value;
                if (!bugReporterOpen && wasOpen)
                {
                    // Same entry point the compilation hook uses: drop the pending report and
                    // detach. Passing null is what the shipped code does; the argument is ignored.
                    BugReporter.OnCompilationStarted(null);
                }
            }
        }

        /// <summary>
        /// Draws the boxed header a full-panel takeover opens with: a title, and an info icon
        /// carrying the explanatory text as a tooltip.
        /// </summary>
        /// <param name="title">Heading text. Rich text is honoured.</param>
        /// <param name="tooltip">The longer explanation, shown only on hover over the icon.</param>
        /// <remarks>
        /// The leading empty label is a spacer exactly the width of the trailing icon, so the title
        /// stays centred in the row rather than being pushed left by the icon.
        /// </remarks>
        private static void DrawPanelHeader(string title, string tooltip)
        {
            using (new GUILayout.HorizontalScope(ADOEditorUtility.styles.bigTitleBackground))
            {
                GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
                GUILayout.Label(title, ADOEditorUtility.styles.centeredBoldRichLabel);
                GUILayout.Label(
                    new GUIContent(ADOEditorUtility.contents.inspectorWindow) { tooltip = tooltip },
                    ADOEditorUtility.styles.iconButton,
                    GUILayout.Width(17f),
                    GUILayout.Height(17f));
            }
        }
    }
}
