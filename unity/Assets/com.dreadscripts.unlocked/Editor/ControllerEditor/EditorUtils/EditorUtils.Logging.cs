// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static RunResolver     -> Colorize(string, Color),   line 2761
//   static CloneResolver   -> Colorize(string, LogType), line 2766
//   static LoginResolver   -> Log,                       line 2781
//   static ReflectResolver -> LogColored,                line 2802
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This is the whole rich-text logging family: the two colourisers, the severity-dispatching
// console call and the convenience method that composes them. Nothing from the region is left
// unported. The neighbouring members in the decompiled file are unrelated despite the shared
// "Resolver" suffix the obfuscator gave everything -- GetResolver/CalcResolver/IncludeResolver
// (lines 2742-2759) are numeric snapping helpers and DeleteResolver/CreateResolver (lines
// 2807-2821) build GUIContent; they belong to other partials.
// Audit status: VERIFIED against decompiled/
//
// The severity parameter is Unity's LogType, not the tools' own CustomLogType: the switch below
// distinguishes Assert and Exception, which CustomLogType does not model.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Wraps <paramref name="message"/> in a rich-text colour tag, for display in the console.
        /// </summary>
        internal static string Colorize(this string message, Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + message + "</color>";
        }

        /// <summary>
        /// Tints <paramref name="message"/> according to its severity, so a log line reads as its
        /// own severity even where Unity draws them all alike.
        /// </summary>
        /// <remarks>
        /// Assert and Exception share the error tint: all three mean the operation failed, and the
        /// distinction between them only matters to the console entry itself.
        /// </remarks>
        internal static string Colorize(this string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return message.Colorize(errorColor);
                case LogType.Warning:
                    return message.Colorize(warningColor);
                default:
                    return message.Colorize(validColor);
            }
        }

        /// <summary>
        /// Sends <paramref name="message"/> to the console at the given severity.
        /// </summary>
        /// <remarks>
        /// <see cref="LogType.Exception"/> is raised through a freshly constructed
        /// <see cref="Exception"/> because Unity has no way to log an exception from a plain
        /// string; the resulting entry carries no useful stack trace of its own.
        /// <see cref="LogType.Assert"/> is deliberately silent -- the original swallows it rather
        /// than forwarding to <c>Debug.LogAssertion</c>, so an assert-severity message is dropped.
        /// </remarks>
        internal static void Log(this string message, LogType logType = LogType.Log)
        {
            switch (logType)
            {
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        /// <summary>
        /// Tints <paramref name="message"/> for its severity and logs it, which is how nearly every
        /// call site wants both steps.
        /// </summary>
        internal static void LogColored(this string message, LogType logType = LogType.Log)
        {
            message.Colorize(logType).Log(logType);
        }
    }
}
