// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static field _SchemaProperty -> hierarchyDelayedCalls,    line 2154
//   static DisableRules          -> DelayCallOnHierarchyGui,  line 4455
//   static RestartRules          -> RunHierarchyDelayedCalls, line 4487
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
//
// ControllerEditor's hierarchy-tick half of the deferred-work queue. The plain EditorApplication
// .delayCall half (decompiled _MethodProperty line 2152, CountRules line 4444, InsertRules line
// 4470) was ported into EditorUtils.Callbacks.cs, which claims those three; the duplicate port that
// once sat here was removed in the port-reconciliation merges. See that file, or ADOverhaul's
// ADOEditorUtility.DelayedCalls.cs, for why the Remove/Combine pair on the delegate is not
// redundant.
//
// The second queue exists because EditorApplication.delayCall does not fire while the Hierarchy
// window is the one that needs redrawing. hierarchyWindowItemOnGUI does fire then -- once per
// visible item -- so it is used as a "the hierarchy is about to draw" tick. The drain runs on the
// first item of the pass and unsubscribes immediately, so the per-item cost is one delegate check.
//
// Audit status: VERIFIED against reverse-engineering/export/ -- all three declared members diffed statement by
// statement against reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs at the
// cited lines (_SchemaProperty 2154, DisableRules 4455, RestartRules 4487), all of which still land
// on the named member in the current snapshot. Field initialiser, the empty-queue test, both
// Delegate.Remove/Combine pairs, the repaint flag, the drain loop with its per-action
// try/LogException and the trailing unsubscribe all match; the only change is naming and the two
// unused callback parameters being named rather than left as decompiler placeholders. The header
// claims no member the file does not declare.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {

        /// <summary>
        /// Work handed to <see cref="DelayCallOnHierarchyGui"/> and not yet run, oldest first.
        /// </summary>
        private static readonly Queue<Action> hierarchyDelayedCalls = new Queue<Action>();

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
