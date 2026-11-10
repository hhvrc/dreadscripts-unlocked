// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the five helpers that sit either side of the three context accessors in
// ControllerEditor.ControllerContext.cs -- the three that pull a fresh value off the Animator
// window's graph when the cached one has gone stale, and the two change notifications the setters
// fire. The third notification, the layer-category rebuild, is in ControllerEditor.LayerCategory.cs.
//
//   InstantiateAnnotation -> PullControllerFromGraph,         line 9703
//   DefineAnnotation      -> PullRootStateMachineFromGraph,   line 9655
//   RemoveAnnotation      -> PullActiveStateMachineFromGraph, line 9698
//   FlushAnnotation       -> RefreshExitTransitionNames,      line 9732
//   RestartVisitor        -> RepaintContextViewers,           line 10837
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// THE FILE NAME. This port wave requires new partials of ControllerEditor to be named
// ControllerEditor.LayerCategory*.cs so that two people adding files to this folder at once cannot
// collide. What the file actually holds is the context-change notifications; only their third
// sibling is about layer categories. The name is a collision rule, not a description.
//
// PullControllerFromGraph TESTS THE WINDOW BY REFERENCE, NOT BY UnityEngine.Object's OPERATOR. The
// decompiled body is `if ((object)GraphAccessors.Tool() != null)`, and the `(object)` is not ILSpy
// hedging: the IL does a plain reference comparison here, where the neighbouring members (line 9676,
// for one) call UnityEngine.Object's op_Inequality. The distinction is visible in exactly one case,
// a window that has been destroyed but whose managed wrapper is still alive: Unity's operator calls
// that null and a reference test does not. Transcribed as the IL has it, because the alternative
// silently changes which branch runs during a domain reload.
//
// RepaintContextViewers REPAINTS BY TYPE, NOT BY INSTANCE. It walks `repaintTargetTypes` -- the two
// window types this tool owns, declared in ControllerEditor.State.cs -- and repaints every live
// object of each, testing for EditorWindow first and Editor second because
// Resources.FindObjectsOfTypeAll returns both and neither derives from the other. It is called
// whenever the shown state machine changes, since both windows put that machine's name in their own
// chrome and neither is otherwise watching for it.
//
// Audit status: VERIFIED -- every statement was transcribed from decompiled lines 9655-9661,
// 9698-9709, 9732-9737 and 10837-10858, and each line number above was confirmed to land on the
// named member in the current snapshot.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Pulling context off the graph

        /// <summary>
        /// Re-reads the edited controller from the Animator window, which fires the layer-category
        /// rebuild if it has actually changed.
        /// </summary>
        /// <remarks>
        /// Guarded on the window existing rather than on the controller being non-null, so closing
        /// the Animator window is *not* what clears the cached controller -- switching it to a
        /// different asset, or to none, is.
        /// </remarks>
        private static void PullControllerFromGraph()
        {
            if ((object)AnimatorGraphReflection.GraphAccessors.Tool != null)
            {
                ActiveController = AnimatorGraphReflection.GraphAccessors.AnimatorController;
            }
        }

        /// <summary>
        /// Re-reads the selected layer's state machine from the graph, which refreshes the
        /// exit-transition name cache if it has changed.
        /// </summary>
        /// <remarks>
        /// Reading <see cref="ActiveController"/> first is not just a guard: its own getter is what
        /// pulls the controller off the window, so this ordering is what makes a cold read of
        /// <see cref="RootStateMachine"/> resolve both values in one call.
        /// </remarks>
        private static void PullRootStateMachineFromGraph()
        {
            if (ActiveController)
            {
                RootStateMachine = AnimatorGraphReflection.GraphAccessors.RootStateMachine;
            }
        }

        /// <summary>
        /// Re-reads the state machine the graph is showing -- which, unlike the other two, needs no
        /// guard: the accessor it reads answers null when there is no graph.
        /// </summary>
        private static void PullActiveStateMachineFromGraph()
        {
            ActiveStateMachine = AnimatorGraphReflection.GraphAccessors.ActiveStateMachine;
        }

        #endregion

        #region Context change notifications

        /// <summary>
        /// Rebuilds <see cref="exitTransitionNames"/> from the root state machine's Any State
        /// transitions that target Exit. Fired when the root state machine changes.
        /// </summary>
        /// <remarks>
        /// These are the transitions that leave the layer altogether, and the window draws their
        /// names as a label strip so that behaviour reachable only from Any State is not invisible.
        /// </remarks>
        private static void RefreshExitTransitionNames()
        {
            exitTransitionNames = RootStateMachine.anyStateTransitions
                .Where(t => t.isExit)
                .Select(t => t.name)
                .ToArray();
        }

        /// <summary>
        /// Repaints every live window and inspector of the tool's own types, so that anything showing
        /// the current state machine's name updates with it.
        /// </summary>
        private static void RepaintContextViewers()
        {
            foreach (System.Type targetType in repaintTargetTypes)
            {
                foreach (UnityEngine.Object found in Resources.FindObjectsOfTypeAll(targetType))
                {
                    if (found is EditorWindow window)
                    {
                        window.Repaint();
                    }
                    else if (found is Editor editor)
                    {
                        editor.Repaint();
                    }
                }
            }
        }

        #endregion
    }
}
