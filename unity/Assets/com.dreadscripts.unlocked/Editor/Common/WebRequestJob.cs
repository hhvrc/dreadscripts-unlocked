// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/WebRequestJob.cs
//   _Algo -> Request, mapper -> pollIntervalMilliseconds, initializer -> onCompleted
//   IsError() -> IsError (property; [SpecialName] in the decompilation)
//   WebRequestJob(string, string, int) and WebRequestJob(string, Action, string, int) are
//     collapsed into the single constructor below via optional parameters.
// Deliberately unported: the NewCode / LoginCode() pair, an obfuscator-injected null check on an
// always-null static with no callers.

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
