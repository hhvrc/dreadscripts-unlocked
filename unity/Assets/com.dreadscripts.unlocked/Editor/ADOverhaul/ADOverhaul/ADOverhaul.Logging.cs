// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The console-logging helpers. Line numbers move with the snapshot; the member names are the
// durable reference.
//
//   NewIdentifier(string, CustomLogType, bool) (line 7806) -> Log
//   DefineIdentifier(string, bool)             (line 7796) -> LogWarning
//   DestroyIdentifier(string, bool)            (line 7801) -> LogError
//   CompareIdentifier(string, bool)            (line 7834) -> ThrowError
//
// DEOBF-BUG(resolved) in Log -- see the marker on the method. export/ renders the body as a
// `while (true)` whose `default:` case is a `continue`, i.e. a loop that cannot terminate for an
// out-of-range CustomLogType. ADOverhaul2019's copy of the same method (StopStruct, line 7791 of
// decompiled/ADOverhaul2019/.../ADOverhaul.cs) is a plain `if` with a plain `switch` and no loop,
// which settles it: this is de4dot recovering a Reactor-flattened branch as a backward one.
// export/ will keep showing the loop until de4dot changes; do not "fix" the deviation back.
//
// Audit status: VERIFIED against export -- all four methods re-read against lines 7796-7840 and
// cross-checked against the 2019 build on 2026-08-04.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Writes a message to the console under a coloured <c>[ADOverhaul]</c> tag.
        /// </summary>
        /// <param name="doLog">
        /// When false nothing is logged. The parameter exists so callers can write
        /// <c>if (!Log(message, type, condition)) return;</c> — the return value is this flag
        /// unchanged, which is what makes that idiom read as "complain and bail, but only when
        /// there is something to complain about".
        /// </param>
        /// <returns><paramref name="doLog"/>, unchanged.</returns>
        /// <remarks>
        /// The literal <c>\n</c> replacement is for messages that arrived over the wire, where a
        /// newline survives the JSON as two characters.
        /// </remarks>
        internal static bool Log(string message, CustomLogType type = CustomLogType.Regular, bool doLog = true)
        {
            if (doLog)
            {
                // DEOBF-BUG(resolved): export/ wraps everything below in `while (true)` with a
                // `default: continue` arm, so an out-of-range CustomLogType would hang the editor.
                // ADOverhaul2019's copy of this method has the plain if/switch reproduced here.
                Color color = type == CustomLogType.Regular
                    ? ADOEditorUtility.validColor
                    : type == CustomLogType.Warning
                        ? ADOEditorUtility.warningColor
                        : ADOEditorUtility.errorColor;

                string tagged = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[ADOverhaul]</color> " + message.Replace("\\n", "\n");

                switch (type)
                {
                    case CustomLogType.Error:
                        Debug.LogError(tagged);
                        break;
                    case CustomLogType.Warning:
                        Debug.LogWarning(tagged);
                        break;
                    case CustomLogType.Regular:
                        Debug.Log(tagged);
                        break;
                }
            }

            return doLog;
        }

        /// <inheritdoc cref="Log"/>
        internal static bool LogWarning(string message, bool doLog = true)
        {
            return Log(message, CustomLogType.Warning, doLog);
        }

        /// <inheritdoc cref="Log"/>
        internal static bool LogError(string message, bool doLog = true)
        {
            return Log(message, CustomLogType.Error, doLog);
        }

        /// <summary>
        /// Throws the same message <see cref="LogError"/> would have logged, tag and colour
        /// included, when <paramref name="doThrow"/> is set.
        /// </summary>
        /// <remarks>
        /// Used where a failure has to unwind rather than be noted and stepped over. The tag is
        /// built into the exception message rather than added by a handler, so it survives whatever
        /// catches it.
        /// </remarks>
        internal static void ThrowError(string message, bool doThrow = true)
        {
            if (doThrow)
            {
                throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(ADOEditorUtility.errorColor) + ">[ADOverhaul]</color> " + message);
            }
        }
    }
}
