// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type.
// Reconstructed from both, which differ only in obfuscated names and one De Morgan'd branch:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, lines 490-602
//     ProcessRunner(string i, Action<string> second, bool wantfilter, bool istask2, Action token3)
//                          -> ProcessRunner(command, onOutput, useCommandPrompt, ignoreFailure, onFailure)
//     startInfo            -> startInfo
//     process              -> process
//     onOutput / onFailure -> onOutput / onFailure
//     ignoreFailure        -> ignoreFailure
//     output               -> output
//     callbackInvoked      -> callbackInvoked
//     isFinished           -> isFinished
//     succeeded            -> succeeded
//     Run                  -> Run
//     Complete             -> Complete
//     DisposeComponent     -> not ported; it is a decompiler-emitted thunk for the
//                             Component.Dispose() call, inlined back into Run below. The
//                             ControllerEditor copy has the plain `process?.Dispose()` call.
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, lines 1905-2008
//     CancelReg -> Run, CountReg -> Complete, m_MockAlgo -> isFinished (the rest are the same
//     members under obfuscated names).
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The two copies are behaviourally identical. The only textual divergence in the logic is the
// success branch in Complete(): ControllerEditor writes `if (!succeeded && !ignoreFailure)
// { onFailure(); } else { onOutput(text); }` and ADOverhaul writes the De Morgan equivalent
// `if (succeeded || ignoreFailure) { onOutput(text); } else { onFailure(); }`. The 2019 and 2022
// ADOverhaul builds differ only in obfuscated local/parameter names.
//
// ---------------------------------------------------------------------------------------------
// SECURITY AUDIT (this type launches external processes; recorded here so it is not re-derived)
//
//   Executables launched: exactly two, both chosen by the `useCommandPrompt` constructor flag and
//   both resolved from PATH by name only, never from a caller-supplied path --
//     "cmd.exe"        when useCommandPrompt is true
//     "powershell.exe" when it is false (the default)
//   So yes, this always shells out; the command string is interpreted by a shell, never exec'd
//   directly.
//
//   Arguments: always the literal "/c " followed by the caller's `command` string, verbatim and
//   unquoted. cmd.exe reads /c as "run this and exit"; powershell.exe accepts the slash form of
//   its switches and treats /c as -Command.
//
//   Caller-controlled? The `command` parameter is entirely caller-controlled by design -- this is
//   a "run this shell line" primitive with no quoting, escaping or validation whatsoever. Anything
//   a caller concatenates into it is executed as shell syntax. In the shipped products, however,
//   every one of the eight construction sites passes a compile-time string literal:
//     via cmd.exe      "wmic baseboard get *", "wmic cpu get *", "wmic diskdrive get *",
//                      "wmic memorychip get *"
//     via powershell   "Get-CimInstance -class Win32_baseboard | Select *",
//                      "Get-CimInstance -class Win32_processor | Select *",
//                      "Get-CimInstance -class Win32_diskdrive | Select *",
//                      "Get-CimInstance -class win32_physicalmemory | Select *"
//   They are all read-only hardware inventory queries, used to build a machine fingerprint for
//   licence binding, and the WMIC set is the fallback for the CIM set. No downloaded content, file
//   path, asset name or user-entered string reaches a command line in the shipped code, so there
//   is no reachable injection path -- but the gap between "safe callers" and "safe primitive" is
//   the whole of the safety here, and a future caller that interpolates a value into `command`
//   would have a command injection with no further mistake required.
//
//   Not reachable from the BugReporter subsystem: neither product's BugReporter constructs a
//   ProcessRunner or otherwise touches System.Diagnostics.Process, directly or transitively.
//
//   Output handling: standard output is redirected and captured, then handed to the `onOutput`
//   callback. Standard error is NOT redirected, and standard input is explicitly not redirected.
//   CreateNoWindow is set, so no console is shown to the user and stderr is effectively discarded.
//   The captured text is never echoed to a shell or re-executed.
// ---------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;

namespace DreadScripts.Common
{
    /// <summary>
    /// Runs a single shell command to completion and hands its standard output to a callback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Run"/> blocks, so callers drive a batch of runners from a background task and
    /// poll <see cref="isFinished"/> rather than awaiting anything; that is why completion is
    /// reported through a flag and a callback instead of a return value.
    /// </para>
    /// <para>
    /// The command text is passed to a shell unescaped. See the audit block at the top of this
    /// file before adding a call site that builds the string from anything but a literal.
    /// </para>
    /// </remarks>
    internal sealed class ProcessRunner
    {
        private readonly ProcessStartInfo startInfo;

        private Process process;

        /// <summary>Receives the captured output once the command has finished.</summary>
        private readonly Action<string> onOutput;

        /// <summary>Invoked instead of <see cref="onOutput"/> when the run threw.</summary>
        private readonly Action onFailure;

        /// <summary>
        /// When set, a failed run still reports through <see cref="onOutput"/> — the exception text
        /// becomes the output — so the caller can treat a crashed probe as just another answer.
        /// </summary>
        private readonly bool ignoreFailure;

        private string output;

        /// <summary>Guards against reporting completion twice; see <see cref="Complete"/>.</summary>
        private bool callbackInvoked;

        /// <summary>
        /// Set once the callback has been invoked. Polled by callers waiting on a batch of runners.
        /// </summary>
        internal bool isFinished;

        private bool succeeded;

        /// <param name="command">The shell command line, passed through verbatim.</param>
        /// <param name="onOutput">Receives the command's standard output.</param>
        /// <param name="useCommandPrompt">
        /// Run through <c>cmd.exe</c> rather than <c>powershell.exe</c>.
        /// </param>
        /// <param name="ignoreFailure">Report a failed run through <paramref name="onOutput"/> too.</param>
        /// <param name="onFailure">Invoked when the run fails and failures are not ignored.</param>
        internal ProcessRunner(string command, Action<string> onOutput, bool useCommandPrompt = false, bool ignoreFailure = false, Action onFailure = null)
        {
            startInfo = new ProcessStartInfo(useCommandPrompt ? "cmd.exe" : "powershell.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                Arguments = "/c " + command
            };

            // Start from a directory that is guaranteed to exist and to be readable, so the run is
            // not affected by wherever the editor happens to have been launched from.
            string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            startInfo.WorkingDirectory = systemFolder;

            if (!useCommandPrompt)
            {
                // Prefer PowerShell's own directory when it is present; the executable is found on
                // PATH either way, this only sets where the command starts out.
                string powerShellFolder = Path.Combine(systemFolder, "WindowsPowerShell", "v1.0");
                if (Directory.Exists(powerShellFolder))
                {
                    startInfo.WorkingDirectory = powerShellFolder;
                }
            }

            this.onOutput = onOutput;
            this.onFailure = onFailure;
            this.ignoreFailure = ignoreFailure;
        }

        /// <summary>
        /// Starts the command and blocks until it has produced output and exited, then reports the
        /// result. Safe to call again to re-run: every field it reads is reset up front.
        /// </summary>
        internal void Run()
        {
            output = string.Empty;
            succeeded = false;
            isFinished = false;
            callbackInvoked = false;

            process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            try
            {
                // A command can exit without ever writing anything, and ReadToEnd on a stream that
                // was closed empty returns immediately; retry until there is output or the process
                // is gone, so a slow first write is not mistaken for no output at all.
                do
                {
                    output = process.StandardOutput.ReadToEnd();
                }
                while (string.IsNullOrEmpty(output) && !process.HasExited);

                succeeded = true;
                Complete();
            }
            catch (Exception ex)
            {
                succeeded = false;
                output = "Failure! Exception: " + ex.Message + "\n" + ex.StackTrace;
                process?.Close();
                process?.Dispose();
                Complete();
            }

            process.WaitForExit();
        }

        /// <summary>
        /// Reports the run exactly once. <see cref="Run"/> reaches here from both the success path
        /// and the catch, and an exception thrown after a successful report would otherwise deliver
        /// a second, contradictory result.
        /// </summary>
        private void Complete()
        {
            if (callbackInvoked)
            {
                return;
            }

            callbackInvoked = true;

            try
            {
                string text = output;
                if (string.IsNullOrWhiteSpace(text))
                {
                    // Callers compare and hash these strings; a placeholder keeps a silent command
                    // distinguishable from one that was never run.
                    text = "Missing";
                }

                if (succeeded || ignoreFailure)
                {
                    onOutput(text);
                }
                else
                {
                    onFailure?.Invoke();
                }
            }
            finally
            {
                // In the finally block so that a throwing callback still releases anyone polling
                // this runner rather than hanging them forever.
                isFinished = true;
            }
        }
    }
}
