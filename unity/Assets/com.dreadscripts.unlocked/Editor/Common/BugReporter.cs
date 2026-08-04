// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both, which are structurally identical:
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, lines 1580-1904
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, lines 168-489
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. The 2019 and 2022 ADOverhaul builds agree exactly:
// they differ only in identifier names (2019 is still obfuscated) and in the order ILSpy emitted
// the switch arms and if/else branches. Every string literal in the three copies is identical.
//
// Member mapping. The left column is the ControllerEditor name, then the ADOverhaul2022 name; the
// right column is the ported member. Because each tool carries its own copy, no single decompiled
// line number can stand for an entry, so these are keyed on the member names:
//   ErrorInfo               / ErrorInfo             -> ErrorInfo,           line 149
//   m_TokenizerAlgo         / handledErrors         -> handledErrors,       line 190
//   m_ModelAlgo             / suppressReporting     -> suppressReporting,   line 205
//   _DicAlgo                / pendingError          -> pendingError,        line 183
//   _InvocationAlgo         / errorContext          -> errorContext,        line 176
//   roleAlgo                / retryAction           -> retryAction,         line 197
//   ManageDefinition        / Run (5-arg overload)  -> Run,                 line 236
//   PrintDefinition         / Run (6-arg overload)  -> Run,                 line 236
//   SearchDefinition        / CaptureException      -> CaptureException,    line 274
//   CompareReg              / SetContext            -> SetContext,          line 332
//   SetReg                  / Reset                 -> Reset,               line 344
//   OrderReg                / HasPendingReport      -> HasPendingReport,    line 313
//   EnableReg               / OnCompilationStarted  -> OnCompilationStarted, line 357
//
// The two Run overloads were collapsed into one method with an optional `retryAction`
// parameter; the 5-argument original did nothing but forward with null.
//
// ============================================================================================
// DATA TRANSMISSION AUDIT  --  read this before changing anything here
// ============================================================================================
// This subsystem is the client half of a telemetry/support feature that sends data off the
// user's machine. The findings below come from tracing the payload builders and transport in
// ControllerEditor.cs (RegisterAnnotation line 10399, LogoutAnnotation line 10418,
// CallVisitor line 10782, CountVisitor line 10808, DisableVisitor line 10827) and their
// identically-shaped ADOverhaul counterparts.
//
// ENDPOINT
//   https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand
//   POST, Content-Type application/json, via HttpWebRequest. This is the only endpoint the bug
//   reporter contacts. It is the same endpoint the licensing code uses (verifylicense,
//   activatelicense, transferlicense*) and the feedback form uses (sendfeedback).
//
//   STATUS: not reachable. As of this port the hostname resolves to 0.0.0.0 / :: -- it is
//   null-routed, not merely returning errors. For contrast, an arbitrary nonexistent GCP
//   project under the same wildcard (us-central1-<madeup>.cloudfunctions.net) still resolves to
//   Google's real wildcard address (216.239.36.54), so the null route is specific to the
//   vendor's host rather than a property of cloudfunctions.net.
//
//   CAUTION FOR ANYONE RE-TESTING THIS: a naive curl against the endpoint may appear to
//   succeed with HTTP 200 {"success":true}. That response is fabricated by an intercepting
//   proxy, not by the vendor. Two tells: it answers in ~16ms, and it returns success for an
//   empty body and for garbage commands, which a real backend gating every request on an
//   HMAC-SHA256 signature would never do. Verify with DNS, not with the response body.
//
//   Unrelated vendor hosts that ARE still live, for reference -- neither is touched by this
//   file: the Google Cloud Storage bucket serving the supporter list
//   (storage.googleapis.com/dreadscripts-c6b62.appspot.com/...) and the GitHub raw host serving
//   the banner image. dreadrith.com itself does not resolve.
//
// WHAT IS TRANSMITTED
//   Every request carries an identity block built by RegisterAnnotation, regardless of command:
//     command      - "findsolution" or "reportbug"
//     product_id   - hardcoded per-product constant ("yOk0XCnENLMO6DIF8cYpSg==" for
//                    ControllerEditor)
//     version      - the tool's own version string, e.g. "3.3.2"
//     HWID         - a hardware fingerprint of the user's machine. Built by running
//                    `wmic baseboard get *`, `wmic cpu get *` and two further hardware queries
//                    via ProcessRunner, SHA1-hashing the results into three components and
//                    joining them with "-". This is a stable, machine-unique identifier.
//     SID          - a per-editor-install GUID persisted in EditorPrefs under "DreadScriptssid",
//                    generated on first use. A stable pseudonymous install identifier.
//     license_key  - the user's license key, read from EditorPrefs.
//     hash         - HMAC-SHA256 over the concatenated values, keyed with a constant embedded
//                    in the assembly. Tamper-evidence for the vendor, not confidentiality.
//   Plus, for this subsystem specifically:
//     bug_id, bug_version, bug_name - the call-site identifiers passed to SetContext. These are
//                    developer-assigned numbers and short labels naming the code path, NOT file
//                    paths and NOT user content.
//     bug_exception - Exception.Message ONLY, URI-escaped.
//     feedback      - free text the user typed into the report box, capped at 2000 characters.
//                    Sent only by the explicit "Report Issue" command.
//
//   NOT transmitted: stack traces (only .Message is read, never .StackTrace), file system
//   paths, the Unity project name, the OS user name, the Unity version, and any scene or asset
//   content. The exception message can of course still incidentally contain a path or a name if
//   the throwing code interpolated one into it -- that is the one uncontrolled channel here.
//
// WHEN IT FIRES, AND WHETHER THE USER KNOWS
//   Nothing is transmitted by the act of catching an exception. Run() catches, records the
//   error locally, re-raises, and schedules a retry; it performs no network I/O. The first
//   request ("findsolution") is issued from the reporter's window the moment that window is
//   drawn, and the second ("reportbug") only when the user presses "Report Issue".
//
//   So a transmission is always preceded by the user opening the reporter -- but the step that
//   opens it can be one click, and the "findsolution" lookup then goes out immediately without
//   a further confirmation. That first lookup already carries the full identity block above
//   (HWID, SID, license key) plus the exception message. A user who clicks "Find Solution"
//   expecting a help lookup has, at that point, already sent their machine fingerprint and
//   licence key. The UI does disclose non-anonymity in the window's subtitle ("Note that the
//   report is not anonymous. Abuse may result in blacklisting."), but that text is shown
//   alongside the lookup that has already been sent, not before it.
//
//   It cannot fire with no user action at all. It can fire without the user appreciating what
//   the action sent.
//
//   Since the endpoint is null-routed, in practice every one of these requests now fails at DNS
//   resolution. The data does not reach the vendor. It is still assembled in memory, and the
//   HWID collection still spawns `wmic` subprocesses, so the local side effects remain.
//
// This file ports the product-agnostic core only, and that core performs NO network I/O. The
// payload construction and transport described above live in the licensing/command layer, which
// is not yet ported; the audit is recorded here because this is the subsystem that motivates
// those requests and this is where a reader will come looking for it.
// ============================================================================================
//
// NOT PORTED
//   DrawReportPrompt (CE: RevertDefinition) and DrawWindow (CE: PostReg), together with the
//   fields that exist only to serve them (solution, solutionComplete, responseReceived,
//   requestSent, isSearching, template) and Respond (CE: SetupReg). These are the reporter's
//   IMGUI surface and the code that actually issues the two requests. They cannot be ported
//   faithfully yet because they depend on infrastructure that does not exist in this package:
//   the command payload builder and HTTP transport, the "license verified" flag that gates the
//   window, the shared feedback window, three named palette colours, and each product's own
//   button/queue helpers (EditorUtils for ControllerEditor, ADOEditorUtility for ADOverhaul --
//   the only axis on which the two copies genuinely differ). Porting them once that layer
//   lands is a mechanical follow-up. Nothing here has been disabled or made opt-in; the omitted
//   half is simply not yet reconstructible.
//
//   Also dropped: a [SpecialName] float property (CE: PublishReg, ADO: VisitMethod) returning
//   `template / 1f` from a field never assigned a nonzero value. It is obfuscator filler with
//   no caller.
//
// Audit status: UNAUDITED -- the mapping and the transmission audit above were written during the
// port and have not been re-checked against decompiled/ since the 561e9ec re-snapshot; this pass
// only reformatted the mapping introduction into a legal MAP block.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;

namespace DreadScripts.Common
{
    /// <summary>
    /// Tracks an exception thrown inside a guarded operation so the tool can offer to look up a
    /// known solution for it, and retries the operation once scripts recompile.
    /// </summary>
    /// <remarks>
    /// See the transmission audit at the top of this file before extending this type: the
    /// reporting flow it feeds sends a machine fingerprint and licence key alongside the
    /// exception message.
    /// </remarks>
    internal static class BugReporter
    {
        /// <summary>
        /// Identifies which guarded operation failed, and how.
        /// </summary>
        /// <remarks>
        /// <see cref="id"/> and <see cref="version"/> are developer-assigned numbers naming a
        /// call site, so the vendor can recognise a report without needing a stack trace.
        /// <para>
        /// Equality is the default structural comparison over all four fields, which is what
        /// <see cref="handledErrors"/> relies on to recognise an error it has already dealt
        /// with. No <c>Equals</c>/<c>GetHashCode</c> override was present in the original.
        /// </para>
        /// </remarks>
        internal struct ErrorInfo
        {
            /// <summary>Short label for the failing operation, e.g. the feature name.</summary>
            internal string name;

            internal ushort id;

            internal ushort version;

            /// <summary>
            /// <see cref="Exception.Message"/> of the failure. Left null on a context registered
            /// by <see cref="SetContext"/>, and filled in only once an exception is caught.
            /// </summary>
            internal string exceptionMessage;
        }

        /// <summary>
        /// The operation currently being guarded, registered before it runs so that a failure
        /// can be attributed to it. Carries no exception message yet.
        /// </summary>
        private static ErrorInfo? errorContext;

        /// <summary>
        /// The failure awaiting the user's attention: <see cref="errorContext"/> plus the
        /// message of the exception that was actually thrown.
        /// </summary>
        private static ErrorInfo? pendingError;

        /// <summary>
        /// The guarded operation, re-run after the next recompile on the assumption that the
        /// user is fixing the cause in script.
        /// </summary>
        private static Action retryAction;

        /// <summary>
        /// Errors the user has already answered for, so the same failure does not prompt twice.
        /// </summary>
        /// <remarks>
        /// Static and never cleared, so it persists for the lifetime of the domain and resets on
        /// recompile — long enough to stop a repeated prompt within one session.
        /// </remarks>
        internal static readonly HashSet<ErrorInfo> handledErrors = new HashSet<ErrorInfo>();

        /// <summary>
        /// Suppresses capture entirely, letting exceptions propagate untouched. Set while running
        /// code whose failures are the caller's to handle.
        /// </summary>
        internal static bool suppressReporting;

        /// <summary>
        /// Opens the reporter window, true to jump straight to the solution lookup.
        /// </summary>
        /// <remarks>
        /// A seam standing in for each product's own window-opening method (ControllerEditor's
        /// <c>ComputeInitializer</c>, ADOverhaul's <c>RemoveSerializer</c>), which live in the
        /// not-yet-ported licensing/GUI layer. Assigned by the host tool at initialisation.
        /// Until something assigns it, choosing "Find Solution" records the error and opens
        /// nothing; that is a consequence of the missing layer, not a deliberate opt-out.
        /// </remarks>
        internal static Action<bool> openReportWindow;

        /// <summary>
        /// Runs <paramref name="operation"/>, attributing any exception to the given call site
        /// and offering to look up a solution before re-throwing.
        /// </summary>
        /// <param name="retryAction">
        /// Re-run after the next recompile. Defaults to null, meaning the failure is not retried.
        /// </param>
        /// <param name="promptImmediately">
        /// Show a modal dialog at the point of failure rather than waiting for the tool's window
        /// to draw the inline prompt.
        /// </param>
        /// <param name="promptMessage">Overrides the dialog's default wording.</param>
        /// <remarks>
        /// The exception is always re-thrown: this reports, it does not swallow. Registering the
        /// context only when <paramref name="id"/> is nonzero lets a caller that has already
        /// called <see cref="SetContext"/> pass nothing and keep the context it set.
        /// </remarks>
        internal static void Run(Action operation, Action retryAction = null, ushort id = 0, string name = "", ushort version = 0, bool promptImmediately = false, string promptMessage = "")
        {
            BugReporter.retryAction = retryAction;
            if (id > 0)
            {
                SetContext(id, name, version);
            }
            try
            {
                operation();
            }
            catch (Exception exception)
            {
                if (suppressReporting)
                {
                    throw;
                }
                CaptureException(exception, promptImmediately, promptMessage);

                // Remove before adding so the handler is registered exactly once however many
                // times this is reached.
                CompilationPipeline.compilationStarted -= OnCompilationStarted;
                CompilationPipeline.compilationStarted += OnCompilationStarted;
                throw;
            }
        }

        /// <summary>
        /// Records a caught exception against the current context and, if asked, prompts the user
        /// about it immediately.
        /// </summary>
        /// <remarks>
        /// The <see cref="handledErrors"/> half of the guard never actually matches: it tests
        /// <see cref="errorContext"/>, whose <see cref="ErrorInfo.exceptionMessage"/> is always
        /// null, against entries that were added with a message filled in, so structural equality
        /// fails. In practice only the <c>HasValue</c> half has an effect — a failure outside any
        /// registered context is ignored. Ported as written; the redundant test is in all three
        /// decompiled copies.
        /// </remarks>
        private static void CaptureException(Exception exception, bool promptImmediately = false, string promptMessage = "")
        {
            if (!errorContext.HasValue || handledErrors.Contains(errorContext.Value))
            {
                return;
            }

            pendingError = new ErrorInfo
            {
                name = errorContext.Value.name,
                id = errorContext.Value.id,
                version = errorContext.Value.version,
                exceptionMessage = exception.Message
            };

            if (!promptImmediately)
            {
                return;
            }

            string message = string.IsNullOrWhiteSpace(promptMessage)
                ? "An error has occurred! Do you want to try to find a solution for it?"
                : promptMessage;

            switch (EditorUtility.DisplayDialogComplex("Error!", message, "Find Solution", "Close", "Ignore"))
            {
                // "Find Solution" — mark handled so the inline prompt does not ask again, then
                // open the reporter, which issues the lookup as soon as it draws.
                case 0:
                    handledErrors.Add(pendingError.Value);
                    openReportWindow?.Invoke(true);
                    break;

                // "Close" — leave the error unhandled so the inline prompt can still offer it,
                // and let the retry run at the next recompile.
                case 1:
                    OnCompilationStarted(null);
                    break;

                // "Ignore" — mark handled so nothing asks again.
                case 2:
                    handledErrors.Add(pendingError.Value);
                    OnCompilationStarted(null);
                    break;
            }
        }

        /// <summary>
        /// Whether an error is waiting to be shown to the user, clearing it if it turns out to
        /// have been handled already.
        /// </summary>
        internal static bool HasPendingReport()
        {
            if (!pendingError.HasValue)
            {
                return false;
            }
            if (handledErrors.Contains(pendingError.Value))
            {
                pendingError = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Names the operation about to run, so a failure inside it can be attributed without the
        /// throwing code having to know anything about reporting.
        /// </summary>
        internal static void SetContext(ushort id, string name = "", ushort version = 0)
        {
            errorContext = new ErrorInfo
            {
                id = id,
                name = name,
                version = version
            };
        }

        /// <summary>
        /// Clears the current context and suppression state, without touching
        /// <see cref="pendingError"/> or <see cref="handledErrors"/>.
        /// </summary>
        /// <remarks>
        /// An error already raised stays raised across a reset: this ends the guarded operation,
        /// it does not dismiss its outcome.
        /// </remarks>
        internal static void Reset()
        {
            suppressReporting = false;
            errorContext = null;
        }

        /// <summary>
        /// Re-runs the failed operation when scripts recompile, on the assumption that a
        /// recompile means the user has changed something that may have fixed it.
        /// </summary>
        /// <remarks>
        /// Unsubscribes unconditionally, so a retry is attempted at most once per failure.
        /// </remarks>
        internal static void OnCompilationStarted(object context)
        {
            if (pendingError.HasValue && retryAction != null)
            {
                Run(retryAction, null, pendingError.Value.id, pendingError.Value.name, pendingError.Value.version);
            }
            retryAction = null;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
        }
    }
}
