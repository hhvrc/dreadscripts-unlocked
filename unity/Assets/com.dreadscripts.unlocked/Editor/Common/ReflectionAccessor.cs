// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ReflectionAccessor.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ObjectReflector.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Reads, writes and invokes members of an object by name, including private ones. ADOverhaul
    /// drives several internal UnityEditor types (PhysBone editors, handle caches) that expose no
    /// public API, so nearly all of its interaction with them goes through here.
    /// </summary>
    /// <remarks>
    /// Member tables are cached per <see cref="Type"/> in <see cref="cacheByType"/> and shared by
    /// every accessor built for that type, so the (expensive) <see cref="Type.GetMembers()"/> call
    /// happens once per type per domain reload rather than once per accessor.
    /// </remarks>
    internal class ReflectionAccessor
    {
        private const BindingFlags AllMembers =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static readonly Dictionary<Type, ReflectionCache> cacheByType = new Dictionary<Type, ReflectionCache>();

        internal readonly object target;

        internal readonly Type targetType;

        internal readonly ReflectionCache cache;

        internal ReflectionAccessor(object target)
        {
            this.target = target;
            targetType = target.GetType();

            if (cacheByType.TryGetValue(targetType, out cache))
            {
                return;
            }

            MemberInfo[] members = targetType.GetMembers(AllMembers);

            Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
            foreach (MethodInfo method in members.OfType<MethodInfo>())
            {
                if (!methods.TryGetValue(method.Name, out var overloads))
                {
                    overloads = new List<MethodInfo>();
                    methods.Add(method.Name, overloads);
                }

                overloads.Add(method);
            }

            cache = new ReflectionCache
            {
                members = members,
                fields = members.OfType<FieldInfo>().ToDictionary(f => f.Name),
                properties = members.OfType<PropertyInfo>().ToDictionary(p => p.Name),
                methods = methods
            };

            cacheByType.Add(targetType, cache);
        }

        /// <summary>
        /// Gets or sets a member by name, logging an error if no such member exists. Use
        /// <see cref="TryGetValue"/> / <see cref="TrySetValue"/> where a miss is expected and should
        /// stay quiet.
        /// </summary>
        public object this[string name]
        {
            get
            {
                if (TryGetValue(name, out var value))
                {
                    return value;
                }

                Debug.LogError("Member " + name + " not found in " + targetType.Name);
                return null;
            }
            set
            {
                if (!TrySetValue(name, value))
                {
                    Debug.LogError("Member " + name + " not found in " + targetType.Name);
                }
            }
        }

        /// <summary>
        /// Reads a field or property. Naming a method instead invokes it with no arguments and
        /// returns its result.
        /// </summary>
        public bool TryGetValue(string name, out object value)
        {
            if (cache.fields.TryGetValue(name, out var field))
            {
                value = field.GetValue(target);
                return true;
            }

            if (cache.properties.TryGetValue(name, out var property))
            {
                value = property.GetValue(target);
                return true;
            }

            if (cache.methods.ContainsKey(name))
            {
                value = Invoke(name);
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>Writes a field or property. Returns false if no member of that name exists.</summary>
        public bool TrySetValue(string name, object value)
        {
            // The ADOverhaul build fell through from the property branch into an unconditional
            // FieldInfo.SetValue on the failed out-var, so writing to a property threw
            // NullReferenceException instead of setting it. ControllerEditor's own copy of this
            // helper (ObjectReflector.ReflectContext) returns from each branch and is correct; the
            // behaviour below follows that one. Each branch returns on its own here.
            if (cache.fields.TryGetValue(name, out var field))
            {
                field.SetValue(target, value);
                return true;
            }

            if (cache.properties.TryGetValue(name, out var property))
            {
                property.SetValue(target, value);
                return true;
            }

            return false;
        }

        /// <summary>Invokes a method by name, resolving overloads against <paramref name="args"/>.</summary>
        internal object Invoke(string name, params object[] args)
        {
            return Invoke(name, null, args);
        }

        /// <summary>
        /// Invokes a method by name and casts the result. The return type also participates in
        /// overload resolution, as a last resort when the arguments alone are ambiguous.
        /// </summary>
        internal T Invoke<T>(string name, params object[] args)
        {
            return (T)Invoke(name, typeof(T), args);
        }

        private object Invoke(string name, Type returnType, params object[] args)
        {
            if (!cache.methods.TryGetValue(name, out var overloads))
            {
                Debug.LogError("Method " + name + " not found in " + targetType.Name);
                return null;
            }

            if (overloads.Count == 1)
            {
                return overloads[0].Invoke(target, args);
            }

            // Narrow the candidates one criterion at a time, stopping as soon as exactly one
            // overload survives: parameter count, then argument types, then return type.
            if (TrySingle(overloads, m => m.GetParameters().Length == args.Length, out var byCount))
            {
                return byCount[0].Invoke(target, args);
            }

            Type[] argTypes = args.Where(a => a != null).Select(a => a.GetType()).ToArray();
            if (TrySingle(byCount, m => !argTypes.Except(m.GetParameters().Select(p => p.ParameterType)).Any(), out var byArgs))
            {
                return byArgs[0].Invoke(target, args);
            }

            if (returnType != null && TrySingle(byArgs, m => m.ReturnType == returnType, out var byReturn))
            {
                return byReturn[0].Invoke(target, args);
            }

            Debug.LogError("Multiple methods named " + name + " found in " + targetType.Name);
            return null;
        }

        /// <summary>
        /// Filters <paramref name="candidates"/> and reports whether exactly one survived.
        /// <paramref name="matches"/> is always the filtered set, so a caller that did not get its
        /// single match can keep narrowing from it.
        /// </summary>
        private static bool TrySingle(IEnumerable<MethodInfo> candidates, Func<MethodInfo, bool> predicate, out MethodInfo[] matches)
        {
            if (candidates == null)
            {
                matches = null;
                return false;
            }

            matches = candidates.Where(predicate).ToArray();
            return matches.Length == 1;
        }
    }
}
