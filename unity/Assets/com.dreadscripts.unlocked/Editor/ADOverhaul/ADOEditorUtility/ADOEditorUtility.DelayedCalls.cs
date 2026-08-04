// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static field _IteratorSerializer -> delayedCalls,     line 2050
//   static AddProcess                -> DelayCall,        line 2367
//   static ValidateProcess           -> RunDelayedCalls,  line 2378
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above.
//
// The Remove-then-Combine pair on EditorApplication.delayCall is written out as it shipped rather
// than simplified to a bare `+=`. The Remove is not redundant: delayCall is a plain multicast
// delegate that Unity clears only after it fires, so re-subscribing without removing first would
// leave two copies of the callback registered if the queue drained and refilled within one tick.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>Work handed to <see cref="DelayCall"/> and not yet run, oldest first.</summary>
        private static readonly Queue<Action> delayedCalls = new Queue<Action>();

        /// <summary>
        /// Runs <paramref name="action"/> after the current editor tick, in the order it was queued.
        /// </summary>
        /// <remarks>
        /// For work that must not happen inside <c>OnGUI</c> -- destroying an object, reloading an
        /// asset, opening a window -- because doing it mid-layout desynchronises IMGUI's layout and
        /// repaint passes. <see cref="EditorApplication.delayCall"/> alone would do that, but it
        /// gives no ordering guarantee between subscribers and swallows nothing; the queue here adds
        /// both FIFO order and per-item exception isolation.
        /// </remarks>
        internal static void DelayCall(Action action)
        {
            // Only the first item arms the callback; anything queued while it is already armed just
            // joins the queue.
            bool wasEmpty = delayedCalls.Count == 0;
            delayedCalls.Enqueue(action);

            if (!wasEmpty)
            {
                return;
            }

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
        }

        /// <summary>
        /// Drains the queue, then unsubscribes so an empty queue costs nothing per tick.
        /// </summary>
        /// <remarks>
        /// A throwing item is logged and the drain continues, so one bad callback cannot strand the
        /// rest of the queue. Items queued from inside an item are picked up by the same drain,
        /// because the loop re-reads the count.
        /// </remarks>
        private static void RunDelayedCalls()
        {
            while (delayedCalls.Count != 0)
            {
                Action action = delayedCalls.Dequeue();
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
        }
    }
}
