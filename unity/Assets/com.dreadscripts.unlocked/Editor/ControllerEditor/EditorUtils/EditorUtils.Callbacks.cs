// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static Queue<Action> _MethodProperty -> delayedActions,       line 2152
//   static CountRules                    -> DelayCall,            line 4444
//   static InsertRules                   -> RunDelayedActions,    line 4470
//   static FillRules                     -> RequireQualifiedType, line 5265
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// RequireQualifiedType belongs by subject with EditorUtils.Types.cs (FindType / RequireType, the
// other two members of the decompiled file's three-strong type-resolution family). That file
// already existed and is owned by another port, so the member lands here instead. See the note at
// the bottom of this header about the claim its header currently makes.
//
// The immediate neighbours of the delayed-call pump are deliberately left to other partials, being
// the same shape of code over different editor callbacks: DisableRules (line 4456) / RestartRules
// (line 4489) are the identical queue-and-drain pair driven by
// EditorApplication.hierarchyWindowItemOnGUI rather than delayCall, and CancelRules (line 4434) is
// an unrelated dictionary lookup. They are not needed by anything ported so far.
//
// Duplication with ADOverhaul: DelayCall / RunDelayedActions are the same code as
// ADOEditorUtility.DelayCall / RunDelayedActions in Editor/ADOverhaul/ADOEditorUtility/
// ADOEditorUtility.Events.cs, and the names are kept identical so the correspondence is obvious.
// The queue is deliberately NOT shared: each product shipped its own static class holding its own
// Queue<Action> instance, and folding them together would merge two independent delayed-action
// queues into one - a real behavioural change, since a drain triggered by one product would then
// run the other product's pending work.
//
// Correction to EditorUtils.Types.cs's header: it records FillRules as "deliberately not ported
// here [...] Nothing in the reconstructed package calls it". The second half is true only of the
// package as it stands today, and only because RenameOverlayWrapper - the one ported type that
// used it - was rebuilt on TypeResolver. In the shipped assembly FillRules has roughly thirty call
// sites, all in the not-yet-ported ControllerEditor god class (the Harmony patch-target lookups at
// decompiled ControllerEditor.cs lines 6735-6736, 8975, 15282-15288, 15694-15704, 15867-15897,
// 16166-16167, 17077-17121) plus RenameOverlayWrapper.cs line 55. That header should be corrected
// to say the member is ported in EditorUtils.Callbacks.cs as RequireQualifiedType.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Work handed to <see cref="DelayCall"/>, drained by <see cref="RunDelayedActions"/> on the
        /// next editor tick.
        /// </summary>
        private static readonly Queue<Action> delayedActions = new Queue<Action>();

        /// <summary>
        /// Defers <paramref name="action"/> until the editor's next update, running it outside
        /// whatever callback is executing now.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This exists because <see cref="EditorApplication.delayCall"/> alone is not safe to
        /// subscribe to repeatedly: it is a plain multicast field that Unity clears as it invokes,
        /// so a caller that adds to it while it is being invoked can have its callback dropped.
        /// Queueing the work and keeping at most one subscription alive makes the ordering explicit
        /// and lets many callers pile on without racing.
        /// </para>
        /// <para>
        /// The subscription is only (re)armed when the queue was empty, i.e. when no drain is
        /// already pending -- an enqueue onto a non-empty queue will be picked up by the drain that
        /// is already scheduled. The Remove-then-Combine pair is a subscribe that cannot
        /// double-subscribe: removing a handler that is not attached is a no-op, so the sequence
        /// leaves exactly one copy attached however many times it runs.
        /// </para>
        /// <para>
        /// The other reason callers reach for this is that the drain runs on the main thread. Work
        /// started on a background thread -- the patch-failure banner's timed retry is the one such
        /// caller -- cannot touch the Unity or Harmony APIs directly, so it hops back through here.
        /// Note that the enqueue itself is not synchronised: <see cref="Queue{T}"/> is not
        /// thread-safe and this is called from a worker thread, which is how the original shipped.
        /// </para>
        /// </remarks>
        internal static void DelayCall(Action action)
        {
            bool drainPending = delayedActions.Count != 0;
            delayedActions.Enqueue(action);
            if (!drainPending)
            {
                EditorApplication.delayCall =
                    (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedActions));
                EditorApplication.delayCall =
                    (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedActions));
            }
        }

        /// <summary>Runs everything <see cref="DelayCall"/> has queued, then unsubscribes.</summary>
        /// <remarks>
        /// The loop dequeues rather than iterating so that an action which itself calls
        /// <see cref="DelayCall"/> has its work run in this same drain instead of waiting a further
        /// tick -- which also means a callback that unconditionally re-queues itself will spin the
        /// editor forever. Each action is isolated: one throwing must not strand the rest of the
        /// queue, so the exception is logged and the drain continues. The final unsubscribe pairs
        /// with the "queue was empty" test in <see cref="DelayCall"/>; leaving the handler attached
        /// would make every later enqueue believe a drain was already scheduled when the delegate
        /// had in fact been cleared.
        /// </remarks>
        private static void RunDelayedActions()
        {
            while (delayedActions.Count != 0)
            {
                Action action = delayedActions.Dequeue();
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            EditorApplication.delayCall =
                (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedActions));
        }

        /// <summary>
        /// Resolves a type strictly by assembly-qualified name, throwing when it is missing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The strict counterpart to <see cref="RequireType"/>: no assembly scan, just
        /// <see cref="Type.GetType(string)"/>. Callers pass a fully assembly-qualified name and get
        /// the type out of that assembly or nothing -- a same-named type in some other loaded
        /// assembly is not an acceptable answer.
        /// </para>
        /// <para>
        /// That is the right trade for its callers, which are almost all Harmony patch targets:
        /// internal UnityEditor and UnityEditor.Graphs types whose assembly is known and fixed, and
        /// where patching the wrong same-named type would be far worse than not patching at all.
        /// Being exact also makes it cheap enough to call from a patch-installation pass that
        /// resolves dozens of types in a row, where the scanning variant would walk the whole app
        /// domain per miss.
        /// </para>
        /// <para>
        /// Throwing rather than returning null is likewise deliberate: a missing internal type means
        /// this Unity version has moved the API and the patch built on it cannot be installed, which
        /// the patch manager wants to hear about as a failure it can report, not as a null that
        /// surfaces later as something unrelated.
        /// </para>
        /// </remarks>
        internal static Type RequireQualifiedType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new Exception("Type \"" + typeName + "\" not found.");
            }

            return type;
        }
    }
}
