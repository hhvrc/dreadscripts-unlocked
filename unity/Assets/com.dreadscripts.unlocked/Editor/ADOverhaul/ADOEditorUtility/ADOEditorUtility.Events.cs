// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static Queue<Action> _IteratorSerializer -> delayedActions,            line 2050
//   static AddProcess                        -> DelayCall,                 line 2367
//   static ValidateProcess                   -> RunDelayedActions,         line 2378
//   static SetStatus                         -> Toggle,                    line 2732
//   static InvokeStatus                      -> FadeGroup,                 line 2752
//   static RateStatus                        -> CommandIssued,             line 3348
//   static CloneStatus                       -> KeyPressed,                line 3367
//   static ComputeStatus                     -> SubmitPressed,             line 3382
//   static QueryStatus                       -> CancelPressed,             line 3391
//   static CountStatus                       -> DeletePressed,             line 3396
//   static StartStatus                       -> SubmitOrCancel,            line 3405
//   static RemoveStatus                      -> SubmitOrCancelAndDefocus,  line 3420
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The members are not contiguous in the decompiled class: RateStatus..RemoveStatus form one
// unbroken block (the obfuscator kept the event-dispatch family together), while the delayed-call
// pump and the two small GUI helpers sit far earlier, interleaved with unrelated utilities. They
// are collected here because they are all "respond to something the editor loop hands you".
// The neighbours deliberately left to other partials: SortStatus (line 2737, a Rect splitter, part
// of the Rects region), CustomizeStatus (line 2766, an Object-to-Component cast helper),
// ReflectStatus (line 3430, the Handles/Graphics gizmo drawing that follows RemoveStatus), and the
// [SpecialName] accessors CustomizeRef / MapRef (lines 3337/3343) that immediately precede
// RateStatus and are really the lazy `contents` / `styles` properties belonging to the Contents and
// Styles partials.
//
// The command-name enum tested against is the shared DreadScripts.Common.EventCommands, already
// ported; the decompiled source nests its own copy inside ADOEditorUtility (line 837).
//
// Duplication with ControllerEditor: the seven event members are the same code that ships in
// decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs (lines 6247-6328),
// ported as Editor/ControllerEditor/EditorUtils/EditorUtils.Events.cs, and the delayed-call pump is
// the same as that file's CountRules/InsertRules pair (lines 4444/4468). They are NOT shared here:
// each product shipped its own static class and its own copy of the queue, so folding them together
// would merge two independent delayed-action queues into one. The names are kept identical to the
// ControllerEditor port so the correspondence is obvious. Nothing here was already ported under
// another name inside ADOverhaul -- the package was searched before this file was added.
//
// 2019 vs 2022: all twelve members are present in
// decompiled/ADOverhaul2019/DreadScripts/ADOverhaul/ADOEditorUtility.cs (queue 2052; ExcludeAccount
// 2371 / AddAccount 2382; PushManager 2744; InsertManager 2764; CalcManager 3365; MapManager 3384;
// SortManager 3400; SetupManager 3408; PrintManager 3413; FindManager 3422; CollectManager 3437)
// with identical behaviour. The only differences are decompiler-level: 2022 writes several of the
// guards with the branches inverted (`if (!x) return false;` where 2019 writes
// `if (x) { ... } return false;`), and 2019 spells the fade-group's mid-transition test
// `0f < faded && !(faded >= 1f)` where 2022 writes `!(0f >= faded) && faded < 1f`. Both are the
// same predicate. No behavioural divergence.

using System;
using System.Collections.Generic;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
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
        /// Typical callers are things that must not run during asset import or during a GUI pass --
        /// re-validating the project after a load, or firing the update check.
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

        /// <summary>Flips <paramref name="value"/> in place and returns its new state.</summary>
        /// <remarks>
        /// Taking the receiver by <c>ref</c> is what lets this be written at the call site as
        /// <c>myFlag.Toggle()</c> and still mutate the caller's field. Returning the new value
        /// rather than the old one means it reads as "turn it on/off and tell me where it ended
        /// up", which is what the foldout and mode-switch call sites want.
        /// </remarks>
        internal static bool Toggle(this ref bool value)
        {
            value = !value;
            return value;
        }

        /// <summary>
        /// Draws <paramref name="body"/> inside a fade group driven by <paramref name="animation"/>,
        /// skipping the group entirely when it is fully collapsed.
        /// </summary>
        /// <param name="whileFading">
        /// Optional extra content drawn only while the group is part-way open, i.e. mid-animation.
        /// </param>
        /// <remarks>
        /// <para>
        /// The outer test is not just an optimisation. <see cref="EditorGUILayout.BeginFadeGroup"/>
        /// still reserves and clips layout for a group at zero fade, and a collapsed section that
        /// contains controls with side effects (or that participate in focus) is better left
        /// undrawn; the guard makes "fully closed" mean "not in the layout at all".
        /// </para>
        /// <para>
        /// <paramref name="whileFading"/> covers the case where a section needs something drawn
        /// only during the transition -- a filler that keeps the height stable while the real
        /// content is still clipped. Its bounds exclude both endpoints, so it appears on no frame
        /// where the group is settled.
        /// </para>
        /// </remarks>
        internal static void FadeGroup(this AnimBool animation, Action body, Action whileFading = null)
        {
            if (animation.faded == 0f)
            {
                return;
            }

            EditorGUILayout.BeginFadeGroup(animation.faded);
            body();
            if (whileFading != null && animation.faded > 0f && animation.faded < 1f)
            {
                whileFading();
            }

            EditorGUILayout.EndFadeGroup();
        }

        /// <summary>
        /// Reports whether the current event is Unity's <paramref name="command"/> command, and by
        /// default consumes it.
        /// </summary>
        /// <param name="controlName">
        /// When non-empty, the command only counts if the named control currently holds keyboard
        /// focus, so that several fields on one window can each answer for themselves. Empty means
        /// "answer for the window as a whole".
        /// </param>
        /// <param name="consumeEvent">
        /// Whether to call <see cref="Event.Use"/> on a match, so nothing else in this GUI pass acts
        /// on the same command. Pass false to peek without claiming it.
        /// </param>
        /// <remarks>
        /// Both <see cref="EventType.ValidateCommand"/> and <see cref="EventType.ExecuteCommand"/>
        /// answer true. Unity sends the validate pass first to ask whether anyone is willing to
        /// handle the command, and only sends the execute pass if something consumed the validate;
        /// a handler that answered only to execute would therefore never be asked to execute at all.
        /// Callers that act on the result will run their side effect twice per command -- once per
        /// pass -- which is what the decompiled call sites do, and is harmless for the idempotent
        /// work they do.
        /// </remarks>
        internal static bool CommandIssued(EventCommands command, string controlName = "", bool consumeEvent = true)
        {
            if (!string.IsNullOrEmpty(controlName) && GUI.GetNameOfFocusedControl() != controlName)
            {
                return false;
            }

            Event current = Event.current;
            if (current.type == EventType.ExecuteCommand || current.type == EventType.ValidateCommand)
            {
                // Enum member names are Unity's command strings verbatim; see EventCommands.
                bool isMatch = command.ToString() == current.commandName;
                if (isMatch && consumeEvent)
                {
                    current.Use();
                }

                return isMatch;
            }

            return false;
        }

        /// <summary>
        /// Reports whether <paramref name="key"/> was pressed this event, and by default consumes the
        /// key press.
        /// </summary>
        /// <param name="controlName">
        /// When non-empty, restricts the test to the event pass in which the named control holds
        /// keyboard focus.
        /// </param>
        /// <param name="consumeEvent">Whether to consume the key press on a match.</param>
        internal static bool KeyPressed(KeyCode key, string controlName = "", bool consumeEvent = true)
        {
            if (!string.IsNullOrEmpty(controlName) && GUI.GetNameOfFocusedControl() != controlName)
            {
                return false;
            }

            Event current = Event.current;
            bool isMatch = current.type == EventType.KeyDown && current.keyCode == key;
            if (isMatch && consumeEvent)
            {
                current.Use();
            }

            return isMatch;
        }

        /// <summary>
        /// Reports whether the user asked to commit what they are editing -- Return on the main
        /// keyboard or Enter on the numeric keypad.
        /// </summary>
        /// <remarks>
        /// The two are separate key codes and Unity never folds one into the other, so a field that
        /// only tested <see cref="KeyCode.Return"/> would ignore the keypad. The Return test runs
        /// first and short-circuits, which matters when the event is being consumed: only one of the
        /// two calls can ever match a given event anyway.
        /// </remarks>
        internal static bool SubmitPressed(string controlName = "", bool consumeEvent = true)
        {
            if (KeyPressed(KeyCode.Return, controlName, consumeEvent))
            {
                return true;
            }

            return KeyPressed(KeyCode.KeypadEnter, controlName, consumeEvent);
        }

        /// <summary>
        /// Reports whether the user asked to abandon what they are editing, i.e. pressed Escape.
        /// </summary>
        internal static bool CancelPressed(string controlName = "", bool consumeEvent = true)
        {
            return KeyPressed(KeyCode.Escape, controlName, consumeEvent);
        }

        /// <summary>
        /// Reports whether the user asked to delete the current selection, and by default consumes
        /// the command.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unity turns the delete keys into a command event rather than a key event, and issues one
        /// of two commands depending on the platform's convention for what deletion means. "Delete"
        /// is the permanent one; "SoftDelete" is the recoverable one that would send an asset to the
        /// trash and is what Unity raises for the key the platform treats as an ordinary delete. On
        /// Windows and Linux, Delete raises "SoftDelete" and Shift+Delete raises "Delete"; on macOS
        /// the Command+Delete / Command+Backspace pair maps the same way. Which key produces which
        /// command therefore varies, so a handler that wants "the user pressed a delete key" has to
        /// accept both.
        /// </para>
        /// <para>
        /// The distinction only carries meaning for a handler that actually destroys something and
        /// so has to choose between the recycle bin and permanent removal. Callers here are clearing
        /// a reference rather than deleting an asset, so both are equally recoverable and both are
        /// accepted. SoftDelete is tested first purely so the commoner of the two costs one
        /// comparison.
        /// </para>
        /// </remarks>
        internal static bool DeletePressed(string controlName = "", bool consumeEvent = true)
        {
            if (CommandIssued(EventCommands.SoftDelete, controlName, consumeEvent))
            {
                return true;
            }

            return CommandIssued(EventCommands.Delete, controlName, consumeEvent);
        }

        /// <summary>
        /// Runs whichever of <paramref name="onSubmit"/> / <paramref name="onCancel"/> the user's key
        /// press asked for, and reports whether the edit was ended either way.
        /// </summary>
        /// <remarks>
        /// Both callbacks are optional, so this doubles as a plain "did the user finish editing"
        /// test -- which is how the test-mode toolbars use it, passing neither. The key press is
        /// always consumed (the nested calls take the default) because an edit that has been ended
        /// should not also reach the control underneath.
        /// </remarks>
        internal static bool SubmitOrCancel(string controlName = "", Action onSubmit = null, Action onCancel = null)
        {
            if (SubmitPressed(controlName))
            {
                onSubmit?.Invoke();
                return true;
            }

            if (CancelPressed(controlName))
            {
                onCancel?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// <see cref="SubmitOrCancel"/>, and on either outcome drops keyboard focus.
        /// </summary>
        /// <remarks>
        /// For a text field that should stop being edited once committed or abandoned. Without the
        /// defocus the field keeps the caret and keeps swallowing typing, even though the edit it
        /// represented is over. Note that <paramref name="controlName"/> is required here where its
        /// counterpart defaults it to empty -- defocusing the whole window on any Return keypress is
        /// not something a caller would want by accident.
        /// </remarks>
        internal static bool SubmitOrCancelAndDefocus(string controlName, Action onSubmit = null, Action onCancel = null)
        {
            if (!SubmitOrCancel(controlName, onSubmit, onCancel))
            {
                return false;
            }

            GUI.FocusControl(null);
            return true;
        }
    }
}
