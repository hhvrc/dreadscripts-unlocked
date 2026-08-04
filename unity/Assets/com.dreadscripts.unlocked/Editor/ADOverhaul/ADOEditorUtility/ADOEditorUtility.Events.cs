// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static RateStatus    -> CommandIssued,              line 3348
//   static CloneStatus   -> KeyPressed,                 line 3367
//   static ComputeStatus -> SubmitPressed,              line 3382
//   static QueryStatus   -> CancelPressed,              line 3391
//   static CountStatus   -> DeletePressed,              line 3396
//   static StartStatus   -> SubmitOrCancel,             line 3405
//   static RemoveStatus  -> SubmitOrCancelAndDefocus,   line 3420
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above and cross-checked against the ControllerEditor twin named next.
//
// The nested EventCommands enum these read is shared with ControllerEditor and lives in
// DreadScripts.Common.EventCommands; see that file. Its member names have to match Unity's command
// strings exactly, because CommandIssued compares Event.commandName against ToString().
//
// Shared with ControllerEditor: EditorUtils.Events.cs is the same seven members under the same
// seven names, statement for statement. Deliberately NOT consolidated, on the same basis as
// ADOEditorUtility.Colors.cs.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Whether Unity is dispatching <paramref name="command"/> this event.
        /// </summary>
        /// <param name="controlName">
        /// Only respond while this named control has focus. Empty means "regardless of focus".
        /// </param>
        /// <param name="consumeEvent">Consume the event on a match, so nothing else also acts on it.</param>
        /// <remarks>
        /// True for both <see cref="EventType.ValidateCommand"/> and
        /// <see cref="EventType.ExecuteCommand"/>. Unity sends the validate pass first to ask whether
        /// anyone will handle the command; answering it is what makes the menu item enable.
        /// </remarks>
        internal static bool CommandIssued(EventCommands command, string controlName = "", bool consumeEvent = true)
        {
            if (!string.IsNullOrEmpty(controlName) && GUI.GetNameOfFocusedControl() != controlName)
            {
                return false;
            }

            Event current = Event.current;
            if (current.type != EventType.ExecuteCommand && current.type != EventType.ValidateCommand)
            {
                return false;
            }

            bool matched = command.ToString() == current.commandName;
            if (matched && consumeEvent)
            {
                current.Use();
            }

            return matched;
        }

        /// <summary>Whether <paramref name="key"/> went down this event.</summary>
        /// <inheritdoc cref="CommandIssued(EventCommands, string, bool)"/>
        internal static bool KeyPressed(KeyCode key, string controlName = "", bool consumeEvent = true)
        {
            if (!string.IsNullOrEmpty(controlName) && GUI.GetNameOfFocusedControl() != controlName)
            {
                return false;
            }

            Event current = Event.current;
            bool matched = current.type == EventType.KeyDown && current.keyCode == key;
            if (matched && consumeEvent)
            {
                current.Use();
            }

            return matched;
        }

        /// <summary>Whether either Enter key went down -- the main one or the numpad's.</summary>
        /// <inheritdoc cref="CommandIssued(EventCommands, string, bool)"/>
        internal static bool SubmitPressed(string controlName = "", bool consumeEvent = true)
        {
            return KeyPressed(KeyCode.Return, controlName, consumeEvent)
                || KeyPressed(KeyCode.KeypadEnter, controlName, consumeEvent);
        }

        /// <summary>Whether Escape went down.</summary>
        /// <inheritdoc cref="CommandIssued(EventCommands, string, bool)"/>
        internal static bool CancelPressed(string controlName = "", bool consumeEvent = true)
        {
            return KeyPressed(KeyCode.Escape, controlName, consumeEvent);
        }

        /// <summary>
        /// Whether a delete was issued, by either the Delete or the Backspace binding.
        /// </summary>
        /// <remarks>
        /// Unity routes the two through separate commands -- SoftDelete for Backspace, Delete for
        /// the Delete key -- and platforms disagree about which one a keyboard sends. Both are
        /// accepted, which is why this goes through the command channel rather than
        /// <see cref="KeyPressed"/>.
        /// </remarks>
        /// <inheritdoc cref="CommandIssued(EventCommands, string, bool)"/>
        internal static bool DeletePressed(string controlName = "", bool consumeEvent = true)
        {
            return CommandIssued(EventCommands.SoftDelete, controlName, consumeEvent)
                || CommandIssued(EventCommands.Delete, controlName, consumeEvent);
        }

        /// <summary>
        /// Runs whichever of <paramref name="onSubmit"/> / <paramref name="onCancel"/> the user
        /// asked for, and reports whether either fired.
        /// </summary>
        /// <returns>
        /// True if the edit was ended either way, so a caller can close its editing state with one
        /// check instead of two.
        /// </returns>
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
        /// <see cref="SubmitOrCancel"/>, additionally dropping keyboard focus when the edit ends.
        /// </summary>
        /// <remarks>
        /// Without this a text field keeps focus after Enter, so the next keystroke lands back in
        /// the field the user just finished with.
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
