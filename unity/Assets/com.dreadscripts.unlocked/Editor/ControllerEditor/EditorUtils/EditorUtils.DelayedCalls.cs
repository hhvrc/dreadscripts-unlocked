// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static field _MethodProperty -> delayedCalls,             line 2152
//   static field _SchemaProperty -> hierarchyDelayedCalls,    line 2154
//   static CountRules            -> DelayCall,                line 4444
//   static InsertRules           -> RunDelayedCalls,          line 4470
//   static DisableRules          -> DelayCallOnHierarchyGui,  line 4455
//   static RestartRules          -> RunHierarchyDelayedCalls, line 4487
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// ControllerEditor's copy of the deferred-work queue. The DelayCall half is the same code as
// ADOverhaul's ADOEditorUtility.DelayedCalls.cs -- see that file for why the Remove/Combine pair on
// the delegate is not redundant -- but this class is not shared with it: ControllerEditor adds a
// second queue on a different hook, and the two products' copies were never merged in the shipped
// assemblies either.
//
// The second queue exists because EditorApplication.delayCall does not fire while the Hierarchy
// window is the one that needs redrawing. hierarchyWindowItemOnGUI does fire then -- once per
// visible item -- so it is used as a "the hierarchy is about to draw" tick. The drain runs on the
// first item of the pass and unsubscribes immediately, so the per-item cost is one delegate check.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Work handed to <see cref="DelayCall"/> and not yet run, oldest first.</summary>
        private static readonly Queue<Action> delayedCalls = new Queue<Action>();

        /// <summary>
        /// Work handed to <see cref="DelayCallOnHierarchyGui"/> and not yet run, oldest first.
        /// </summary>
        private static readonly Queue<Action> hierarchyDelayedCalls = new Queue<Action>();

        /// <summary>
        /// Runs <paramref name="action"/> after the current editor tick, in the order it was queued.
        /// </summary>
        /// <remarks>
        /// For work that must not happen inside <c>OnGUI</c> -- destroying an object, reloading an
        /// asset, opening a window -- because doing it mid-layout desynchronises IMGUI's layout and
        /// repaint passes.
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

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(
                EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(
                EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
        }

        /// <summary>Drains the queue, then unsubscribes so an empty queue costs nothing per tick.</summary>
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

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(
                EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
        }

        /// <summary>
        /// Runs <paramref name="action"/> the next time the Hierarchy window draws.
        /// </summary>
        /// <param name="repaintHierarchy">
        /// Ask for that redraw immediately rather than waiting for one to happen on its own. False
        /// when the caller knows a repaint is already coming.
        /// </param>
        internal static void DelayCallOnHierarchyGui(Action action, bool repaintHierarchy = true)
        {
            bool wasEmpty = hierarchyDelayedCalls.Count == 0;
            hierarchyDelayedCalls.Enqueue(action);

            if (wasEmpty)
            {
                EditorApplication.hierarchyWindowItemOnGUI =
                    (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(
                        EditorApplication.hierarchyWindowItemOnGUI,
                        new EditorApplication.HierarchyWindowItemCallback(RunHierarchyDelayedCalls));
                EditorApplication.hierarchyWindowItemOnGUI =
                    (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(
                        EditorApplication.hierarchyWindowItemOnGUI,
                        new EditorApplication.HierarchyWindowItemCallback(RunHierarchyDelayedCalls));
            }

            if (repaintHierarchy)
            {
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        /// <summary>
        /// Drains the hierarchy queue and unsubscribes. Both parameters are the callback's -- the
        /// instance id and rect of the hierarchy row being drawn -- and neither is used: this is a
        /// tick, not a per-row hook.
        /// </summary>
        private static void RunHierarchyDelayedCalls(int instanceId, Rect selectionRect)
        {
            while (hierarchyDelayedCalls.Count != 0)
            {
                Action action = hierarchyDelayedCalls.Dequeue();
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            EditorApplication.hierarchyWindowItemOnGUI =
                (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(
                    EditorApplication.hierarchyWindowItemOnGUI,
                    new EditorApplication.HierarchyWindowItemCallback(RunHierarchyDelayedCalls));
        }
    }
}
