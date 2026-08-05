// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static DisableList   -> GetAnyMethod(Type, string),           line 6773
//   static InsertList    -> GetAnyMethod(Type, string, Type[]),   line 6778
//   static RestartList   -> GetAnyField,                          line 6783
//   static QueryList     -> GetAnyProperty,                       line 6788
//   static AddList       -> GetAnyConstructor,                    line 6793
//   static TestList      -> FindMethod(Type, string),             line 7125
//   static MapList       -> FindMethod(Type, string, Type),       line 7143
//   static ValidateList  -> FindMethod(Type, string, Type[]),     line 7161
//   static CustomizeList -> FindMethod(Type, string, int),        line 7179
//   IncludeSetter (the <>c cached lambda) -> inlined into FindMethod(Type, string, Type[]), line 1866
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// IncludeSetter is the compiler's cached `p => p.ParameterType` selector; the decompiler hoisted it
// into the <>c display class at line 1866 and calls it from the Except() at line 7164. It is written
// back inline at that single use site rather than resurrected as a member.
//
// Two families that look alike and are not:
//   * GetAny* are one-line wrappers over Type.GetMember with the four binding flags spelt out, so
//     a lookup does not have to repeat them. They return null silently.
//   * FindMethod scans GetMethods and filters, which finds what GetMethod's overload-resolution
//     rules refuse to -- a method whose parameter types are known only approximately, or only by
//     count. It logs an error and returns null on both misses and ambiguity, so a wrong internal
//     Unity member name shows up in the console rather than as a NullReferenceException later.
//
// ControllerEditor's FindMethod family is the same code as ADOverhaul's, in
// ADOEditorUtility.Reflection.cs, down to the log messages. It is deliberately not consolidated,
// for the same reason given there: the two products shipped their own copies and the restored
// package keeps each tool's utility class self-contained.
//
// The Type[] overload is not a signature match, it is a subset test: it keeps any method whose
// parameter types are a *superset* of the ones asked for, so trailing optional parameters do not
// have to be listed. Being set-based it also ignores parameter order and repeats.
//
// Audit status: VERIFIED -- all ten entries were re-checked against reverse-engineering/export/ on 2026-08-05 and
// each line number lands on the member named (the IncludeSetter lambda at EditorUtils.cs line 1866,
// used at 7164). This replaces an older "VERIFIED against reverse-engineering/export/" claim, which cannot be
// reproduced.

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        private const BindingFlags AnyMember =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>The method named <paramref name="name"/>, public or not, static or not.</summary>
        internal static MethodInfo GetAnyMethod(this Type type, string name)
        {
            return type.GetMethod(name, AnyMember);
        }

        /// <summary>
        /// The method named <paramref name="name"/> taking exactly
        /// <paramref name="parameterTypes"/>, public or not, static or not.
        /// </summary>
        internal static MethodInfo GetAnyMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type.GetMethod(name, AnyMember, null, parameterTypes, null);
        }

        /// <summary>The field named <paramref name="name"/>, public or not, static or not.</summary>
        internal static FieldInfo GetAnyField(this Type type, string name)
        {
            return type.GetField(name, AnyMember);
        }

        /// <summary>The property named <paramref name="name"/>, public or not, static or not.</summary>
        internal static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            return type.GetProperty(name, AnyMember);
        }

        /// <summary>
        /// The constructor taking exactly <paramref name="parameterTypes"/>, public or not.
        /// </summary>
        internal static ConstructorInfo GetAnyConstructor(this Type type, Type[] parameterTypes)
        {
            return type.GetConstructor(AnyMember, null, parameterTypes, null);
        }

        /// <summary>
        /// The one method named <paramref name="name"/>. Logs and returns null if there is none or
        /// more than one -- use an overload below to disambiguate.
        /// </summary>
        internal static MethodInfo FindMethod(this Type type, string name)
        {
            MethodInfo[] matches = type.GetMethods(AnyMember).Where(m => m.Name == name).ToArray();
            switch (matches.Length)
            {
                case 1:
                    return matches[0];
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name);
                    return null;
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name);
                    return null;
            }
        }

        /// <summary>
        /// The one method named <paramref name="name"/> that takes a parameter of type
        /// <paramref name="parameterType"/> anywhere in its signature.
        /// </summary>
        internal static MethodInfo FindMethod(this Type type, string name, Type parameterType)
        {
            MethodInfo[] matches = type.GetMethods(AnyMember)
                .Where(m => m.Name == name && m.GetParameters().Any(p => p.ParameterType == parameterType))
                .ToArray();

            switch (matches.Length)
            {
                case 1:
                    return matches[0];
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name + " with parameter of type " +
                                   parameterType.Name);
                    return null;
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name +
                                   " with parameter of type " + parameterType.Name);
                    return null;
            }
        }

        /// <summary>
        /// The one method named <paramref name="name"/> whose parameter types include all of
        /// <paramref name="parameterTypes"/>. Order and extra parameters are ignored.
        /// </summary>
        internal static MethodInfo FindMethod(this Type type, string name, Type[] parameterTypes)
        {
            MethodInfo[] matches = type.GetMethods(AnyMember)
                .Where(m => m.Name == name
                            && !parameterTypes.Except(m.GetParameters().Select(p => p.ParameterType)).Any())
                .ToArray();

            switch (matches.Length)
            {
                case 1:
                    return matches[0];
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name + " with parameters of types " +
                                   string.Join(", ", parameterTypes.Select(t => t.Name)));
                    return null;
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name +
                                   " with parameters of types " +
                                   string.Join(", ", parameterTypes.Select(t => t.Name)));
                    return null;
            }
        }

        /// <summary>
        /// The one method named <paramref name="name"/> taking exactly
        /// <paramref name="parameterCount"/> parameters.
        /// </summary>
        internal static MethodInfo FindMethod(this Type type, string name, int parameterCount)
        {
            MethodInfo[] matches = type.GetMethods(AnyMember)
                .Where(m => m.Name == name && m.GetParameters().Length == parameterCount)
                .ToArray();

            switch (matches.Length)
            {
                case 1:
                    return matches[0];
                case 0:
                    Debug.LogError($"Method {name} not found in {type.Name} with {parameterCount} parameters");
                    return null;
                default:
                    Debug.LogError($"Multiple methods named {name} found in {type.Name} with {parameterCount} parameters");
                    return null;
            }
        }
    }
}
