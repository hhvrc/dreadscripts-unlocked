// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CreateProcess -> HandleTask, line 2395
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// The method's real name and its real parameter names are recovered from the state machine, not
// guessed. Roslyn names the state-machine struct after the method it was generated for
// (_003CHandleTask_003Ed__18<T>, i.e. <HandleTask>d__18, decompiled line 1851) and names each
// hoisted parameter field after the parameter it holds, and the protector renamed neither. The
// fields are taskHandle, onComplete, onFailure, OnCancelled, onSuccess and onFinale; which
// parameter each one is was settled by matching the state machine's body against the async
// method's, since the field declaration order does not follow the parameter order:
//   res (1st)       -> taskHandle
//   attr            -> onSuccess
//   res (2nd)       -> onFailure
//   task2           -> onCancelled
//   var13           -> onComplete
//   selection4      -> onFinale
//
// Two of those need a word. The decompiler emitted two parameters both named `res`; the second
// shadows the first, which is why the decompiled null check reads `if (res == null)` against an
// Action. And the cancellation field is spelled OnCancelled, capitalised; it is lower-cased here
// to match its five siblings.
//
// Seven single-statement smethod_N proxies in the state machine (smethod_0..smethod_6, lines
// 2014-2047) are inlined back to what they wrap: Task.IsCompleted, Debug.LogError,
// Debug.LogException, Task.IsFaulted, Task.IsCanceled, Task.Exception and
// Exception.GetBaseException.
//
// The outcome dispatch is written out below as three plain branches. The decompiled form was
//     if (!IsFaulted || IsCanceled) { if (IsFaulted || !IsCanceled) success; else cancelled; }
//     else failure;
// which is the same function of the two flags for every combination, including the impossible
// faulted-and-cancelled one (both forms take the success branch there).
//
// Audit status: PARTIAL -- the mapping above was re-checked against decompiled/ (the async method
// at line 2395 and the state-machine struct at line 1851, which agree statement for statement);
// the ported method body was not re-diffed in this pass.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Awaits <paramref name="taskHandle"/> and dispatches its outcome to the matching callback,
        /// so a caller can start background work from synchronous editor code without writing its
        /// own await and try/catch.
        /// </summary>
        /// <param name="onSuccess">Raised with the result when the task ran to completion.</param>
        /// <param name="onFailure">
        /// Raised with the task's base exception when it faulted. When null the exception is logged
        /// instead, so a fault is never silent.
        /// </param>
        /// <param name="onCancelled">Raised instead of <paramref name="onSuccess"/> when the task was cancelled.</param>
        /// <param name="onComplete">Raised first, whatever the outcome, before the outcome is dispatched.</param>
        /// <param name="onFinale">Raised last, whatever the outcome. The place to clear a "busy" flag.</param>
        /// <returns>
        /// The task's result, or <c>default</c> if it did not produce one. The returned task itself
        /// faults if any callback other than <paramref name="onFailure"/>'s log path throws.
        /// </returns>
        /// <remarks>
        /// A callback that throws is logged and then rethrown, so a bug in the caller's handler is
        /// visible in the console rather than being swallowed by the task machinery -- but it does
        /// stop <paramref name="onFinale"/> from running.
        /// </remarks>
        internal static async Task<T> HandleTask<T>(
            this Task<T> taskHandle,
            Action<T> onSuccess,
            Action<Exception> onFailure = null,
            Action onCancelled = null,
            Action onComplete = null,
            Action onFinale = null)
        {
            T result;
            try
            {
                result = await taskHandle;
            }
            catch
            {
                // Swallowed on purpose: the awaited exception is rethrown here only because await
                // does that, and the fault is dispatched below off the task's own state instead.
                result = default(T);
            }

            if (!taskHandle.IsCompleted)
            {
                Debug.LogError("FATAL ERROR! Task not completed?");
                return result;
            }

            if (onComplete != null)
            {
                try
                {
                    onComplete();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    throw;
                }
            }

            if (taskHandle.IsFaulted && !taskHandle.IsCanceled)
            {
                Exception exception = taskHandle.Exception.GetBaseException();
                if (onFailure == null)
                {
                    Debug.LogException(exception);
                }
                else
                {
                    try
                    {
                        onFailure(exception);
                    }
                    catch (Exception callbackException)
                    {
                        Debug.LogException(callbackException);
                        throw callbackException;
                    }
                }
            }
            else if (!taskHandle.IsFaulted && taskHandle.IsCanceled)
            {
                if (onCancelled != null)
                {
                    try
                    {
                        onCancelled();
                    }
                    catch (Exception callbackException)
                    {
                        Debug.LogException(callbackException);
                        throw callbackException;
                    }
                }
            }
            else
            {
                try
                {
                    onSuccess(result);
                }
                catch (Exception callbackException)
                {
                    Debug.LogException(callbackException);
                    throw callbackException;
                }
            }

            if (onFinale != null)
            {
                try
                {
                    onFinale();
                }
                catch (Exception callbackException)
                {
                    Debug.LogException(callbackException);
                    throw callbackException;
                }
            }

            return result;
        }
    }
}
