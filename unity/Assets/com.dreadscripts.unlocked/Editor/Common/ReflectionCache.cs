// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ReflectionCache.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/TypeReflectionData.cs
//
// Audit status: VERIFIED -- both copies diffed field by field against this file. All four fields
// (members, fields, properties, methods) match in name, type, accessibility and declaration order;
// the struct declares nothing else in either copy.

using System.Collections.Generic;
using System.Reflection;

namespace DreadScripts.Common
{
    /// <summary>
    /// Per-type reflection lookup tables, built once by <see cref="ReflectionAccessor"/> and reused
    /// for every subsequent member access on that type.
    /// </summary>
    /// <remarks>
    /// Methods are keyed to a list rather than a single <see cref="MethodInfo"/> because overloads
    /// share a name; the caller picks the overload by argument count.
    /// </remarks>
    internal struct ReflectionCache
    {
        internal MemberInfo[] members;

        internal Dictionary<string, FieldInfo> fields;

        internal Dictionary<string, PropertyInfo> properties;

        internal Dictionary<string, List<MethodInfo>> methods;
    }
}
