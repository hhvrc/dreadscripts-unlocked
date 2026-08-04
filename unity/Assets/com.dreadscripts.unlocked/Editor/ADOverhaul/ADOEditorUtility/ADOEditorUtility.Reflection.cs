// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CancelStatus   -> FindType,                     line 2803
//   static OrderVal       -> FindMethod(Type, string),        line 3765
//   static CalculateVal   -> FindMethod(Type, string, Type),  line 3783
//   static CalcVal        -> FindMethod(Type, string, Type[]), line 3801
//   static DeleteVal      -> FindMethod(Type, string, int),   line 3819
//   static FlushAdapter   -> not ported; see below,        line 4058
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above.
//
// Deliberately not ported: FlushAdapter, a file-scoped static whose whole body is
// `type.GetMethod(name, flags, binder, types, modifiers)`. It is one of the protector's call
// proxies, not product code; its single caller (the TextFieldDropDown lookup, now in
// ADOEditorUtility.EditorInternals.cs) calls Type.GetMethod directly instead.
//
// Two lambdas from the compiler-generated _003C_003Ec closure (line 1592) belong here and get no
// file: `p => p.ParameterType` and `t => t.Name`, both inlined below.
//
// Shared with ControllerEditor: EditorUtils.Types.cs ports the same FindType (plus a RequireType
// this build does not ship). Deliberately NOT consolidated, on the same basis as
// ADOEditorUtility.Colors.cs.

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Resolves a type by assembly-qualified name, full name, or bare name, searching every
        /// loaded assembly.
        /// </summary>
        /// <returns>The type, or null if no assembly has one by that name.</returns>
        /// <remarks>
        /// For reaching an editor-internal or optional-package type without a compile-time
        /// reference. The three lookups are tried in decreasing order of precision, so an
        /// assembly-qualified name costs one call and a bare name costs a scan of every loaded
        /// assembly. The full-name and bare-name passes are interleaved per assembly rather than run
        /// one after the other across all of them, so an assembly that has a bare-name match wins
        /// over a later assembly that has a full-name match. Shipped behaviour, preserved as-is.
        /// </remarks>
        internal static Type FindType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();

                type = types.FirstOrDefault(t => t.FullName == typeName);
                if (type != null)
                {
                    return type;
                }

                type = types.FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// The one method named <paramref name="name"/> on <paramref name="type"/>, public or not,
        /// static or not.
        /// </summary>
        /// <returns>The method, or null after logging when there is no match or more than one.</returns>
        /// <remarks>
        /// Logging rather than throwing is deliberate throughout this family: these run from editor
        /// GUI code against Unity internals, where a rename between editor versions should degrade
        /// the feature rather than break the inspector.
        /// </remarks>
        internal static MethodInfo FindMethod(this Type type, string name)
        {
            MethodInfo[] matches = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name)
                .ToArray();

            switch (matches.Length)
            {
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name);
                    return null;
                case 1:
                    return matches[0];
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name);
                    return null;
            }
        }

        /// <summary>
        /// The one overload of <paramref name="name"/> that takes a parameter of type
        /// <paramref name="parameterType"/> somewhere in its signature.
        /// </summary>
        /// <inheritdoc cref="FindMethod(Type, string)"/>
        internal static MethodInfo FindMethod(this Type type, string name, Type parameterType)
        {
            MethodInfo[] matches = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && m.GetParameters().Any(p => p.ParameterType == parameterType))
                .ToArray();

            switch (matches.Length)
            {
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name + " with parameter of type " + parameterType.Name);
                    return null;
                case 1:
                    return matches[0];
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name + " with parameter of type " + parameterType.Name);
                    return null;
            }
        }

        /// <summary>
        /// The one overload of <paramref name="name"/> whose parameters cover every type in
        /// <paramref name="parameterTypes"/>.
        /// </summary>
        /// <remarks>
        /// A containment test, not a signature match: the overload may take further parameters, and
        /// order is not considered. That is what makes it usable when only some of the parameter
        /// types are known.
        /// </remarks>
        /// <inheritdoc cref="FindMethod(Type, string)"/>
        internal static MethodInfo FindMethod(this Type type, string name, Type[] parameterTypes)
        {
            MethodInfo[] matches = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && !parameterTypes.Except(m.GetParameters().Select(p => p.ParameterType)).Any())
                .ToArray();

            switch (matches.Length)
            {
                case 0:
                    Debug.LogError("Method " + name + " not found in " + type.Name + " with parameters of types " + string.Join(", ", parameterTypes.Select(t => t.Name)));
                    return null;
                case 1:
                    return matches[0];
                default:
                    Debug.LogError("Multiple methods named " + name + " found in " + type.Name + " with parameters of types " + string.Join(", ", parameterTypes.Select(t => t.Name)));
                    return null;
            }
        }

        /// <summary>
        /// The one overload of <paramref name="name"/> that takes exactly
        /// <paramref name="parameterCount"/> parameters.
        /// </summary>
        /// <inheritdoc cref="FindMethod(Type, string)"/>
        internal static MethodInfo FindMethod(this Type type, string name, int parameterCount)
        {
            MethodInfo[] matches = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && m.GetParameters().Length == parameterCount)
                .ToArray();

            switch (matches.Length)
            {
                case 0:
                    Debug.LogError($"Method {name} not found in {type.Name} with {parameterCount} parameters");
                    return null;
                case 1:
                    return matches[0];
                default:
                    Debug.LogError($"Multiple methods named {name} found in {type.Name} with {parameterCount} parameters");
                    return null;
            }
        }
    }
}
