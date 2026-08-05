// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ViewQueue    -> CommandIssued,   line 6247
//   static CollectQueue -> KeyPressed,      line 6266
//   static ResolveQueue -> SubmitPressed,   line 6281
//   static ListQueue    -> CancelPressed,   line 6290
//   static VerifyQueue  -> DeletePressed,   line 6295
//   static FillQueue    -> SubmitOrCancel,  line 6304
//   static WriteQueue   -> SubmitOrCancelAndDefocus, line 6319
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This is the whole contiguous event-handling region of the decompiled class: it begins at
// ViewQueue (the preceding members, IconContent / contents / styles at lines 6227-6245, belong to
// the Contents and Styles partials) and ends at WriteQueue, after which the file turns to the
// Handles/Graphics gizmo drawing helpers (ForgotQueue line 6329, StopQueue line 6356, CheckQueue
// line 6361) that are unrelated and are not ported here.
// Audit status: VERIFIED against reverse-engineering/export/
//
// The command-name enum the region tests against is the shared DreadScripts.Common.EventCommands,
// already ported; the decompiled source nests its own copy inside EditorUtils.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
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
        /// work they do (clearing a field, closing a popup).
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
        /// accept both -- testing only one would leave the field unclearable on some platforms, or
        /// only clearable with a modifier held.
        /// </para>
        /// <para>
        /// The distinction only carries meaning for a handler that actually destroys something and
        /// so has to choose between the recycle bin and permanent removal. Callers here are clearing
        /// a reference rather than deleting an asset, so both are equally recoverable and both are
        /// accepted.
        /// </para>
        /// <para>
        /// SoftDelete is tested first purely so the commoner of the two costs one comparison.
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
        /// test. The key press is always consumed -- the nested calls take the default -- because an
        /// edit that has been ended should not also reach the control underneath.
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
        /// represented is over.
        /// </remarks>
        internal static bool SubmitOrCancelAndDefocus(string controlName, Action onSubmit = null, Action onCancel = null)
        {
            if (SubmitOrCancel(controlName, onSubmit, onCancel))
            {
                GUI.FocusControl(null);
                return true;
            }

            return false;
        }
    }
}
