// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Assigned region: the licence gate, its banner, the tool footer and the hamburger menu
// (decompiled lines 10507, 10527, 10978 and 11028), plus an audit of ControllerEditor's private
// copies of BugReporter (1580-1904) and ProcessRunner (1905-2006).
//
// NOTHING FROM THIS REGION IS PORTED. This file is the catalogue, and the catalogue is the
// deliverable: documenting what the DRM did is part of this repository's purpose. The type
// declaration below is empty on purpose, so that the file is a real compilation unit anchored to
// the same partial class as its neighbours and so a future port has somewhere obvious to land.
//
// Line numbers move with the snapshot; the decompiled names are the durable reference. Every claim
// below was checked against the decompiled source rather than carried over from the brief, and the
// three places where the source disagreed with what I was told are called out inline as
// CORRECTION.
//
// ==================================================================================================
// WHY NONE OF THIS IS PORTED
// ==================================================================================================
//
// Every member catalogued here is part of, or feeds, a licence check that POSTs to
//
//   https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand   (line 10829)
//
// and refuses to draw the tool unless the response carries a token the client can reproduce under
// an embedded HMAC key. That host is permanently dead: it resolves to 0.0.0.0. See the DATA
// TRANSMISSION AUDIT at the top of Editor/Common/BugReporter.cs for the full endpoint analysis,
// including the warning that a naive HTTP probe of it returns a *fabricated* 200 from an
// intercepting proxy -- DNS is the only trustworthy signal, and the endpoint was not re-probed for
// this port.
//
// This package exists to restore the tool for the people who bought it. Reconstructing a gate that
// can now only fail would simply reimplement the shutdown. So the gate is not reconstructed, and
// neither is anything whose only purpose is to feed it or to read its results back out. Nothing is
// stubbed, nothing is faked, and no member below is replaced by an always-true variant: they are
// absent. Code elsewhere in the package that called into them is ported without its licence branch
// -- the same decision already recorded in ControllerEditorWindow.cs and, for the other product, in
// ADOverhaul.Licensing.cs, whose structure this file follows.
//
// ==================================================================================================
// CATALOGUE OF OMITTED MEMBERS
// ==================================================================================================
//
// -- The licence gate itself -----------------------------------------------------------------------
//
//   OrderVisitor        10527  THE gate, `bool OrderVisitor(EditorWindow, float)`. Returns true only
//                              once `listenerAnnotation` (the "a licence was validated" flag) is
//                              set; until then it returns false after drawing, in order: the
//                              announcement strip, then whichever of five takeover panes applies --
//                              "Activating/Verifying License..." while a request is in flight, the
//                              transfer pane (CompareVisitor, 10590), the "Check for License" pane
//                              with its Check/Retry button, or the key-entry pane with its Activate
//                              button. Its two callers are the settings window's OnGUI (3315) and a
//                              second window at 8585, and each opens with `if (!OrderVisitor(this))
//                              return;`, so a false return suppresses the entire window body. Also
//                              routes to the feedback panel (ListAnnotation, 10018) and to
//                              BugReporter.PostReg (1742). ControllerEditorWindow has already
//                              dropped its call and now always draws the settings; see the NOT
//                              PORTED block in ControllerEditorWindow.cs.
//                              The panes it draws pull in: SetVisitor, ReadLicenseKey (10680),
//                              EnableVisitor (10706), SaveLicenseInfo (10689), CancelInitializer
//                              (10058), DisableInitializer (10493), ForgotAnnotation (10171),
//                              WriteAnnotation (10090), FillAnnotation (10082), RemoveVisitor
//                              (11173), PopVisitor (10720) and CompareVisitor (10590) -- none of
//                              which is ported either, and none of which is listed separately below
//                              because they exist only to serve this method.
//   RevertAnnotation    10507  The licence banner strip: a boxed `License: <tier, or "Personal" when
//                              blank>` on the left and, when a name is known, a boxed
//                              `Authorized For: <name>` on the right. Both strings
//                              (m_ReaderAnnotation, stubAnnotation) are written only by the
//                              validation response handler, so with the gate gone they are
//                              permanently empty and the banner would read "License: Personal" and
//                              nothing else. A pure readout of dropped state; dropped with it.
//                              Worth recording as a curiosity: the decompiler renders the
//                              "Authorized For" arm as a `return` from inside two nested
//                              GUILayout.HorizontalScope usings. That is faithful IL -- both scopes
//                              still dispose on the way out -- and not a mistranslation, but it is
//                              why the method has an early exit that appears to do nothing.
//
// -- Licence-derived chrome the window loses with the gate -------------------------------------------
//
//   DefineVisitor       10978  The footer: a boxed row holding the hamburger button (which opens
//                              ReadVisitor), an "update available" toggle button, the version label
//                              "v3.3.2", and then either a caller-supplied trailing widget or the
//                              author credit link; below the row, the expandable update panel
//                              (SelectVisitor, 11119). Called by the settings window at 3335, by a
//                              second window at 8671, and by OrderVisitor at 10574 with PopVisitor
//                              as the trailing widget (the "Activate License"/"Transfer License"
//                              switcher). ControllerEditorWindow has already dropped it.
//                              It is not itself a licence gate, but everything it can show is
//                              downstream of the dead host: `singletonAnnotation` (the "an update
//                              exists" flag) and every u_update* setting it reads are written only
//                              by the update query at 11235, which POSTs "getdownloadinfo" to the
//                              same endpoint. With that query dead the whole row degenerates to a
//                              version label and a menu.
//                              CORRECTION to the framing I was given: DefineVisitor does not
//                              itself perform the update check. It only reflects a result the
//                              [InitializeOnLoadMethod] at 11215 fetched; the "Check For Update"
//                              menu item in ReadVisitor is the manual trigger.
//   StartVisitor        11011  A second, unboxed variant of the same footer for use inside a
//                              helpBox, with the update panel omitted. Same dependencies, same
//                              reason for omission. It was not in the region list I was given but
//                              belongs to it.
//   SelectVisitor       11119  The expandable update panel DefineVisitor hangs off its row: the
//                              announcement icon, the offered version, the update message as a
//                              rich-text TextArea, and Download/Changelog buttons pointing at URLs
//                              from the response. Every field is server-supplied. Its second
//                              parameter (`isivk`, default true) is never read -- obfuscator
//                              scaffolding, cf. ADOverhaul's ResetConfiguration.
//   RemoveVisitor       11173  The announcement strip OrderVisitor draws above the panes: an
//                              animated foldout over u_announcement, with an optional link button
//                              and a "Hide" button that is itself gated on `listenerAnnotation`.
//                              Server-supplied text; empty here.
//   QueryVisitor        10860  The author credit link, `Made By @Dreadrith` opening
//                              https://dreadrith.com/links, followed by SupportWindow.DrawButton().
//                              This one touches neither the licence nor the network and would port
//                              cleanly -- it is the exact twin of ADOverhaul's DeleteIdentifier,
//                              which ADOverhaul.Menus.cs adopted as DrawCreditLink. It is left out
//                              here only because its sole caller is DefineVisitor's else branch,
//                              so porting it now would add a member nothing calls. NOTE for whoever
//                              restores the footer: the credit link is currently absent from the
//                              ControllerEditor window as a side effect of dropping DefineVisitor,
//                              and it is the one piece of that footer worth bringing back.
//   SetVisitor          10643  The boxed title-and-tooltip row every takeover pane opens with
//                              (spacer label, centred bold rich-text title, info icon carrying the
//                              explanation as a tooltip). Also licence-free and network-free, and
//                              the exact twin of ADOverhaul's InitConfiguration, which
//                              ADOverhaul.Licensing.cs did port as the private DrawPanelHeader.
//                              Not ported here for the same reason as QueryVisitor: all seven of
//                              its callers -- the five licence panes, the feedback panel and
//                              BugReporter.PostReg -- are omitted. If any of them is ever restored,
//                              take the body from ADOverhaul.Licensing.cs; the two differ only in
//                              which utility class the styles come from.
//
// -- The hamburger menu ------------------------------------------------------------------------------
//
//   ReadVisitor         11028  Builds and shows the footer's GenericMenu. Not ported, but it is the
//                              member most worth restoring later, because roughly half of it is
//                              harmless. Item by item, in the order it adds them:
//                                "Check For Update"        always present; disabled (null handler)
//                                                          while a query is in flight. Erases the
//                                                          cached response from SessionState under
//                                                          "yOk0XCnENLMO6DIF8cYpSg==updateinfo" and
//                                                          re-runs AwakeVisitor (11229), i.e. the
//                                                          "getdownloadinfo" POST to the dead host.
//                                "Send Feedback"           licence-gated. Opens the feedback panel
//                                                          (ListAnnotation, 10018), whose only
//                                                          action is a "sendfeedback" POST.
//                                caller-supplied items     licence-gated, via the `init` callback.
//                                "Verify/On Display"       licence-gated. Toggles a_VerifyOnDisplay
//                                "Verify/On Project Load"  and a_VerifyOnProjectLoad, which are
//                                                          mutually exclusive -- setting either
//                                                          clears the other. Both control *when* the
//                                                          tool phones home. EditorSettings
//                                                          deliberately does not port them.
//                                "Documentation"           ungated, opens
//                                                          notes.sleightly.dev/controllereditor.
//                                "Samples/Templates"       licence-gated, from the statusAnnotation
//                                                          table (8164): one entry, "Templates",
//                                                          opening notes.sleightly.dev/templates. The
//                                                          `Length <= 1` branch drops the "Samples/"
//                                                          prefix for a single entry, so as shipped
//                                                          the item reads just "Templates".
//                                "Changelog"               licence-gated, opens the GitHub changelog.
//                                "Store Page"              ungated, opens dreadrith.com/l/CEditor.
//                                "ToS and Privacy Policy"  ungated, opens dreadrith.com/license-tos.
//                              *** The "Send Feedback" item is deliberately NOT wired up anywhere.
//                              *** Editor/Common/BugReporter.cs is ported but intentionally has no
//                              *** UI, because submitting a report transmits a hardware id, a
//                              *** session id and the licence key. The same applies to the feedback
//                              *** panel this item opens. Restoring either button restores the
//                              *** transmission, dead endpoint or not -- the payload is still
//                              *** assembled and the hardware probe still spawns subprocesses.
//                              Three of the ungated items (Documentation, Store Page, ToS) plus the
//                              Changelog would be worth reinstating in a hand-written menu; note
//                              that dreadrith.com does not resolve either, so only the
//                              notes.sleightly.dev and github.com links still lead anywhere.
//                              CORRECTION: the brief placed ReadVisitor at ~11038; it is defined at
//                              11028, and 11038-11040 is the "Send Feedback" item inside it.
//                              ControllerEditorWindow.cs has the definition line right.
//
// -- Panels reached only through the gate ------------------------------------------------------------
//
//   CompareVisitor      10590  The licence-transfer pane: an explanatory HelpBox, the key field, a
//                              "Send Verification Code" button and the six-digit code field.
//   InitVisitor         10915  The transfer confirmation dialog, and the exact twin of ADOverhaul's
//                              VerifyIdentifier -- including the button misassignment that file
//                              documents: DisplayDialogComplex is called with ok "Continue", cancel
//                              "Terms of Service", alt "Cancel", so dismissing with Escape (which
//                              Unity reports as the cancel slot) opens the ToS page in a browser
//                              while the button captioned "Cancel" is the one that does nothing.
//                              Confirmed present in this build too. Only "Continue" starts the
//                              transfer, so nothing destructive proceeds unconfirmed.
//   ListAnnotation      10018  The feedback panel. Its layout is ordinary and its only action is a
//                              "sendfeedback" POST, so it cannot be ported without the transport.
//                              Carries the same shipped quirk ADOverhaul.Licensing.cs recorded for
//                              its counterpart: the panel's second statement is
//                              `m_ExpressionAnnotation = listenerAnnotation`, i.e. the panel closes
//                              itself on the first frame it draws unless the tool is licensed. The
//                              feedback form is gated too.
//
// -- Licence state, request construction and transport ------------------------------------------------
//
//   Not enumerated member by member here, because Editor/Common/BugReporter.cs already documents
//   this layer in full from the same source lines and there is no value in a second copy. The
//   entry points are RegisterAnnotation (10399, the identity block: command, product_id
//   "yOk0XCnENLMO6DIF8cYpSg==", version, HWID, SID, license_key), LogoutAnnotation (10418, the
//   HMAC-SHA256 tamper-evidence field), CallVisitor (10782), CountVisitor (10808) and
//   DisableVisitor (10827), with the endpoint URL hardcoded at 10829. Around them sit the
//   activation, verification and transfer flows (WriteAnnotation 10090, ForgotAnnotation 10171),
//   the backoff timer and its warning text (CancelInitializer 10058, DisableInitializer 10493),
//   the key format check and EditorPrefs storage, the date-stamp builder (PatchAnnotation 10431)
//   and the hardware fingerprinter that drives ProcessRunner. None is ported.
//
// ==================================================================================================
// BUGREPORTER AND PROCESSRUNNER: ALREADY PORTED, NOT DUPLICATED
// ==================================================================================================
//
// Both were audited against the already-ported shared copies before writing anything, and both are
// twins of the ADOverhaul originals those copies were built from. Neither is re-ported here.
//
//   BugReporter -> BugReporter, line 1580, in BugReporter.cs
//       The nested type spans 1580-1904. That file (Editor/Common/BugReporter.cs, namespace
//       DreadScripts.Common) already names this exact line range as one of its two sources, and
//       maps every member:
//       m_TokenizerAlgo -> handledErrors, ManageDefinition/PrintDefinition -> Run,
//       SearchDefinition -> CaptureException, CompareReg -> SetContext, SetReg -> Reset, OrderReg ->
//       HasPendingReport, EnableReg -> OnCompilationStarted.
//       Re-verified member by member for this
//       port: the reconstruction is faithful, including the quirk it flags in CaptureException --
//       the `handledErrors.Contains(errorContext.Value)` half of the guard can never match, because
//       errorContext always has a null exceptionMessage and the entries in handledErrors never do.
//       Present in this copy too, at 1651.
//       The UI half remains deliberately unbuilt: RevertDefinition (the inline "An error has
//       occurred! Do you want to report it?" prompt, 1683), PostReg (the reporter window, 1742) and
//       SetupReg (1878), together with the fields that serve only them. PostReg is where the
//       "findsolution" and "reportbug" requests are actually issued, so building it is what would
//       transmit the hardware id, session id and licence key. Not wired up, per instruction.
//       One divergence worth recording, and it is a divergence in shape rather than behaviour:
//       ControllerEditor's SetReg takes no parameters and clears five fields (solution,
//       solutionComplete, suppressReporting, template, errorContext) where the ported Reset clears
//       two. The other three are the unported UI fields, so the ported method is complete with
//       respect to what exists. ADOverhaul's Reset is the same.
//
//   ProcessRunner -> ProcessRunner, line 1905, in ProcessRunner.cs
//       The nested type spans 1905-2006; Editor/Common/ProcessRunner.cs (DreadScripts.Common) is
//       likewise already sourced from this exact range. Re-verified: identical field for field and
//       statement for statement. The one textual difference between the two products is the success
//       branch of Complete, which that file already records -- ControllerEditor writes
//       `if (!succeeded && !ignoreFailure) { onFailure?.Invoke(); } else { onOutput(text); }` and
//       ADOverhaul writes the De Morgan equivalent. Confirmed at 1993 here. ControllerEditor also
//       has the plain `process?.Dispose()` call where ADOverhaul2022's decompilation emits a
//       DisposeComponent thunk, which is a decompiler artifact on ADOverhaul's side and is already
//       resolved that way.
//       So: twins, and no second copy is warranted.
//
// ==================================================================================================
// KNOCK-ON EFFECTS
// ==================================================================================================
//
// Dropping this region has consequences that are recorded rather than compensated for:
//   * The settings window has no footer, so it shows no version number, no hamburger menu and no
//     author credit. QueryVisitor above is the piece worth restoring by hand.
//   * The announcement and update panels never appear, because the queries that fill them are dead.
//   * EditorSettings' u_update* and u_announcement* entries are written by nothing, and the
//     a_Verify* entries are read by nothing.
//   * The "Send Feedback" and bug-report entry points do not exist, by design.
//
// No ILSpy artifacts were found in the members catalogued here: no spurious `while (true)`, and the
// only [SpecialName] member in the two audited nested types is BugReporter.PublishReg (1614), a
// float property over a field that is never assigned a nonzero value and that has no caller --
// Common/BugReporter.cs already identifies it as obfuscator filler and drops it. The
// `_003C_003Ec__DisplayClass239_0` capture struct visible at 10930 belongs to InitVisitor's transfer
// continuation, which is omitted along with it.
//
// Audit status: PARTIAL -- the catalogue was written by reading decompiled/ directly (the file's
// own CORRECTION notes are the record of that), but nothing here is re-checked against the
// post-561e9ec snapshot beyond the two nested-type entry points, BugReporter at 1580 and
// ProcessRunner at 1905; the 10xxx/11xxx line numbers in the catalogue may have drifted.

namespace DreadScripts.ControllerEditor
{
    // Intentionally empty: see the catalogue above. This region of the decompiled source is the
    // licence gate and its chrome, and none of it is reconstructed.
    internal partial class ControllerEditor
    {
    }
}
