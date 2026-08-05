// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the tool's own console-logging family, decompiled lines 10876-10913.
//
//   FindVisitor    -> Log,        line 10886
//   AddVisitor     -> LogWarning, line 10876
//   InvokeVisitor  -> LogError,   line 10881
//   ExcludeVisitor -> ThrowError, line 10906
//
// Line numbers are relative to the current snapshot; the decompiled names are the durable
// reference. The whole region is ported -- nothing between those lines is left out.
//
// Parameter names in the decompiled source are obfuscator noise (`def`, `param`, `selection`,
// `setrule`, `spec`, `haveb`) and have been given readable names; the call convention is
// unchanged, including the defaults.
//
// -------------------------------- Relationship to ADOverhaul ---------------------------------
//
// This is a character-for-character twin of ADOverhaul's logger, already reconstructed as
// ADOverhaul.Log / LogWarning / LogError / ThrowError in
// Editor/ADOverhaul/ADOverhaul/ADOverhaul.Logging.cs (decompiled ADOverhaul2022 lines 7796-7840).
// Verified member by member against that file: the bodies agree statement for statement, and the
// only differences are the bracket text ("[Controller Editor]" instead of "[ADOverhaul]") and the
// palette fields the tints are read from (EditorUtils here, ADOEditorUtility there).
//
// It is deliberately NOT shared. Each product shipped its own private copy inside its own
// top-level class in its own namespace, and previous waves of this port have kept such twins
// separate rather than hoisting them into DreadScripts.Common. Two products that happen to log
// alike are not one logger; unifying them would invent a coupling the shipped builds never had,
// and would force a decision about the bracket text that neither product ever made.
//
// -------------------------- Relationship to EditorUtils.Logging.cs ---------------------------
//
// A different family, despite both ending up in the same console. EditorUtils.Log (decompiled
// EditorUtils.cs line 2781, ported at Editor/ControllerEditor/EditorUtils/EditorUtils.Logging.cs)
// is an extension method on string, takes Unity's LogType, colourises through separate Colorize
// helpers, and -- as that file records -- silently swallows LogType.Assert.
//
// This family cannot have that bug. It takes CustomLogType, which models only Regular / Warning /
// Error, so there is no Assert case to drop. Its own analogous edge is different and is described
// on Log below: a CustomLogType value outside the three declared ones composes the message and
// then logs nothing at all. Nothing in the assembly produces such a value, so it never fires,
// whereas EditorUtils' Assert case is reachable by any caller that passes LogType.Assert. The two
// families are unrelated code and neither calls the other.
//
// ----------------------------------- ILSpy artifact: absent -----------------------------------
//
// ADOverhaul.Logging.cs records that the 2022 decompilation of the same method wraps its body in a
// spurious `while (true) { ... switch { default: continue; } break; }`, and reasons that it is a
// decompiler reconstruction rather than shipped control flow. This copy independently confirms
// that reading: ILSpy renders the identical IL here as a plain switch with no loop and no default
// arm, matching ADOverhaul2019 and disagreeing only with ADOverhaul2022. Ported as the plain
// switch, which is also what the source in front of this file literally says.
//
// The switch has no default arm in any of the three builds, so an out-of-range severity really
// does fall through to no console call at all. That is preserved.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor
    {
        /// <summary>
        /// Writes <paramref name="message"/> to the console behind a tinted "[Controller Editor]"
        /// prefix, and returns <paramref name="condition"/> so a check and its complaint can be
        /// written as one expression.
        /// </summary>
        /// <param name="condition">
        /// Whether to log at all. Call sites pass the test they are reporting on, so the message is
        /// emitted only when the test held and the caller still sees the result -- for example
        /// <c>if (LogError(name + " was not found", type == null)) return;</c>.
        /// </param>
        /// <remarks>
        /// The literal two-character sequence <c>\n</c> is expanded to a real newline, because
        /// several messages reach this method from strings that were escaped once too often --
        /// notably server responses, which arrive with their newlines still in JSON form.
        /// <para>
        /// A severity outside the three declared values tints the prefix as an error and then logs
        /// nothing, the composed message being discarded. That is the shipped behaviour and nothing
        /// in the assembly produces such a value.
        /// </para>
        /// </remarks>
        internal static bool Log(string message, CustomLogType logType = CustomLogType.Regular, bool condition = true)
        {
            if (condition)
            {
                Color color;
                if (logType == CustomLogType.Regular)
                {
                    color = EditorUtils.validColor;
                }
                else if (logType == CustomLogType.Warning)
                {
                    color = EditorUtils.warningColor;
                }
                else
                {
                    color = EditorUtils.errorColor;
                }

                string text = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[Controller Editor]</color> " +
                    message.Replace("\\n", "\n");

                switch (logType)
                {
                    case CustomLogType.Error:
                        Debug.LogError(text);
                        break;
                    case CustomLogType.Warning:
                        Debug.LogWarning(text);
                        break;
                    case CustomLogType.Regular:
                        Debug.Log(text);
                        break;
                }
            }

            return condition;
        }

        /// <inheritdoc cref="Log(string, CustomLogType, bool)"/>
        internal static bool LogWarning(string message, bool condition = true)
        {
            return Log(message, CustomLogType.Warning, condition);
        }

        /// <inheritdoc cref="Log(string, CustomLogType, bool)"/>
        internal static bool LogError(string message, bool condition = true)
        {
            return Log(message, CustomLogType.Error, condition);
        }

        /// <summary>
        /// Throws <paramref name="message"/> as an exception carrying the same tinted
        /// "[Controller Editor]" prefix, when <paramref name="condition"/> holds.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Log"/> this does not expand escaped newlines, and it always uses the
        /// error tint rather than deriving one from a severity -- an exception has only the one
        /// severity. The prefix is rich text, so wherever the message is surfaced outside the
        /// console it renders as visible markup rather than as a colour.
        /// </remarks>
        internal static void ThrowError(string message, bool condition = true)
        {
            if (condition)
            {
                throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(EditorUtils.errorColor) +
                    ">[Controller Editor]</color> " + message);
            }
        }
    }
}
