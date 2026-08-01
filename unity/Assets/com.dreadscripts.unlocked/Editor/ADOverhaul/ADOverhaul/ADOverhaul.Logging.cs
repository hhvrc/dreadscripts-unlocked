// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the tool's console-logging family, decompiled lines 7796-7840.
//
//   NewIdentifier     -> Log,        line 7806
//   DefineIdentifier  -> LogWarning, line 7796
//   DestroyIdentifier -> LogError,   line 7801
//   CompareIdentifier -> ThrowError, line 7834
//
// Line numbers are relative to the current snapshot; the decompiled names are the durable
// reference. Field references go through the table in ADOverhaul.State.cs.
//
// -------------------------------- Relationship to ControllerEditor --------------------------------
//
// ControllerEditor ships a character-for-character twin of this family under its own obfuscated
// names -- FindVisitor / AddVisitor / InvokeVisitor / ExcludeVisitor, decompiled
// ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs lines 10886, 10876, 10881 and
// 10906. The only differences are the bracket text ("[Controller Editor]" instead of
// "[ADOverhaul]") and the palette fields the colours come from. It is NOT shared and NOT ported
// here: each product shipped its own private copy inside its own top-level class, and previous
// waves have deliberately kept such twins separate. ControllerEditor's copy is, as of this port,
// not reconstructed anywhere in the package.
//
// It is a different family from EditorUtils.Logging.cs (Editor/ControllerEditor/EditorUtils/),
// which takes Unity's LogType, colourises through extension methods, and silently swallows
// LogType.Assert. This one takes CustomLogType, which models only three severities, so there is no
// Assert case for it to swallow -- ADOverhaul's logger drops nothing. Both products carry both
// families; they are unrelated code that happens to end up in the console.
//
// ----------------------------------- ILSpy artifact removed -----------------------------------
//
// The 2022 decompilation wraps Log's body in `while (true) { ... switch (reg) { default: continue;
// ... } break; }`. That is a decompiler reconstruction of the switch's fallthrough block, not
// shipped control flow: the 2019 build (line 7790, `StopStruct`) and ControllerEditor's twin both
// decompile the identical IL as a plain switch with no loop and no default arm. Ported as the plain
// switch. The practical difference would only ever show for a CustomLogType value outside the three
// declared ones, which nothing constructs.
//
// The default arm is left off rather than folded into the Regular case, which is what the IL says:
// an out-of-range severity composes the message (tinted as an error) and then logs nothing at all.
//
// ------------------------------------- Region NOT ported -------------------------------------
//
// Three members sit inside these line numbers and are deliberately left out. None is a logging
// member despite the shared "Identifier" suffix the obfuscator handed out; all three belong to the
// defunct licence-transfer flow, which talks to a shut-down server.
//
//   VerifyIdentifier   line 7842 -- the transfer confirmation dialog. DisplayDialogComplex(
//       "Terms of Service", "License transfer is subject to the Terms of Service.\nLicense will
//       stop working on the device it was previously activated on.\nYou will not be able to
//       transfer back or again for 30 days.", ok: "Continue", cancel: "Terms of Service",
//       alt: "Cancel"). Note the button assignment: the *cancel* slot holds "Terms of Service" and
//       the *alt* slot holds "Cancel", so dismissing the dialog with Escape -- which Unity reports
//       as the cancel slot -- opens https://dreadrith.com/license-tos in a browser instead of doing
//       nothing, while the button captioned "Cancel" is the one that does nothing. Only "Continue"
//       starts the transfer, so nothing destructive proceeds unconfirmed; the misfire is a stray
//       browser tab. Beyond the dialog it needs CloneConfiguration, CountConfiguration,
//       StartConfiguration, OrderIdentifier, IncludeConfiguration, QueryConfiguration and
//       CalculateIdentifier, none of which are ported.
//   SetIdentifier      line 7872 -- despite its placement next to the dialog above, it asks
//       nothing: it POSTs "transferlicenseconfirm" with the six-digit code from
//       transferVerificationCode and, on success, clears showingTransferPanel, transferCodeSent and
//       licenseKeyEntryRequired and re-runs verification. Same unported dependencies.
//   ConnectSerializer  line 7899 -- a [SpecialName] method, i.e. a property getter the obfuscator
//       flattened; it reads `ADOSettings.Instance().u_updateDay == <today's date stamp>` and would
//       be restored as a bool property along the lines of `checkedForUpdateToday`. ADOSettings has
//       since landed at Editor/ADOverhaul/ADOSettings/ and declares u_updateDay, so the only
//       remaining blocker is RemoveConfiguration (line 7434), the date stamp builder, which also
//       writes currentDateStamp as a side effect and belongs to another region.
//
// 2019 vs 2022: identical behaviour. The 2019 build (lines 7781-7818, names SelectStruct /
// RunStruct / StopStruct / WriteStruct) differs only in the order the decompiler emits the switch
// arms and in the absence of the spurious loop discussed above.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOverhaul
    {
        /// <summary>
        /// Writes <paramref name="message"/> to the console behind a tinted "[ADOverhaul]" prefix,
        /// and returns <paramref name="condition"/> so a check and its complaint can be one
        /// expression.
        /// </summary>
        /// <param name="condition">
        /// Whether to log at all. Call sites pass the test they are reporting on -- for example
        /// <c>if (LogWarning(name + " is not a child of " + root.name, !t.IsChildOf(root)))</c> --
        /// so the message is emitted only when the test held, and the caller still sees the result.
        /// </param>
        /// <remarks>
        /// The literal two-character sequence <c>\n</c> is expanded to a real newline, because
        /// several messages are assembled from strings that were themselves escaped once too often.
        /// A severity outside the three declared values tints the prefix as an error and then logs
        /// nothing; that is the shipped behaviour and nothing in either build produces such a value.
        /// </remarks>
        internal static bool Log(string message, CustomLogType logType = CustomLogType.Regular, bool condition = true)
        {
            if (condition)
            {
                Color color;
                if (logType == CustomLogType.Regular)
                {
                    color = ADOEditorUtility.validColor;
                }
                else if (logType == CustomLogType.Warning)
                {
                    color = ADOEditorUtility.warningColor;
                }
                else
                {
                    color = ADOEditorUtility.errorColor;
                }

                string text = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[ADOverhaul]</color> " +
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
        /// "[ADOverhaul]" prefix, when <paramref name="condition"/> holds.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Log"/> this does not expand escaped newlines, and it always uses the
        /// error tint regardless of what the caller was reporting. The prefix is rich text, so it
        /// renders as markup rather than as a colour anywhere the message is shown outside the
        /// console.
        /// </remarks>
        internal static void ThrowError(string message, bool condition = true)
        {
            if (condition)
            {
                throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(ADOEditorUtility.errorColor) +
                    ">[ADOverhaul]</color> " + message);
            }
        }
    }
}
