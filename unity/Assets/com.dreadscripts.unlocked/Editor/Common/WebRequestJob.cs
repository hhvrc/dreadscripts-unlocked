// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/WebRequestJob.cs
//
//   _Algo                                -> Request,                    line 10
//   mapper                               -> pollIntervalMilliseconds,   line 12
//   initializer                          -> onCompleted,                line 14
//   NewCode                              -> NOT PORTED, line 16 -- an always-null static that only
//     LoginCode() reads; obfuscator-injected, no other callers.
//   IsError()                            -> IsError,                    line 19
//   WebRequestJob(string, string, int)   -> the four-parameter constructor, line 28
//   WebRequestJob(string, Action, string, int) -> WebRequestJob,         line 33
//   Dispose                              -> same,                       line 44
//   Process                              -> same,                       line 49
//   LoginCode                            -> NOT PORTED, line 59 -- a null check on NewCode, which is
//     never assigned; obfuscator-injected, no callers.
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// NOTES
// IsError() is a method in the decompilation but carries [SpecialName], i.e. it was a property in
// the shipped assembly; it is restored as a property here.
//
// The two decompiled constructors are collapsed into the single four-parameter constructor below
// via optional parameters -- the three-parameter one is a pure forwarding overload.
//
// Audit status: PARTIAL -- the member list and line numbers above were checked against decompiled/
// ControllerEditor/DreadScripts/Common/SupportThankies/WebRequestJob.cs; the bodies were read but
// not diffed statement by statement.

using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace DreadScripts.Common
{
    /// <summary>
    /// Awaits a <see cref="UnityWebRequest"/> as a <see cref="Task"/>.
    /// </summary>
    /// <remarks>
    /// Unity's async operation is not awaitable on its own, and the editor has no coroutine runner
    /// outside of play mode, so completion is polled from an <c>await Task.Delay</c> loop instead.
    /// The caller owns the request: dispose the job (or the request) once the response has been
    /// read.
    /// </remarks>
    internal readonly struct WebRequestJob : IDisposable
    {
        /// <summary>
        /// The underlying request, exposed so the caller can configure it (download handler,
        /// timeout) before awaiting <see cref="Process"/> and read the response afterwards.
        /// </summary>
        internal UnityWebRequest Request { get; }

        private readonly int pollIntervalMilliseconds;
        private readonly Action onCompleted;

        /// <param name="method">HTTP verb; defaults to GET when null or blank.</param>
        /// <param name="pollIntervalMilliseconds">How long to sleep between completion checks.</param>
        internal WebRequestJob(string url, Action onCompleted = null, string method = null, int pollIntervalMilliseconds = 100)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                method = "GET";
            }

            Request = new UnityWebRequest(url, method);
            this.onCompleted = onCompleted;
            this.pollIntervalMilliseconds = pollIntervalMilliseconds;
        }

        /// <summary>Whether the request failed, at the network or the HTTP status level.</summary>
        internal bool IsError
        {
            get
            {
                if (Request.isNetworkError)
                {
                    return true;
                }

                return Request.isHttpError;
            }
        }

        public void Dispose()
        {
            Request.Dispose();
        }

        /// <summary>Sends the request and completes once it is done, then runs the callback.</summary>
        internal async Task Process()
        {
            UnityWebRequestAsyncOperation operation = Request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Delay(pollIntervalMilliseconds);
            }

            onCompleted?.Invoke();
        }
    }
}
