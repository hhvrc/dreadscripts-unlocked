// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ForgotRules        -> FindType(string),         line 5285
//   static WriteRules         -> RequireType(string),      line 5276
//   static RemoveResolver     -> Is<T>(this Type),         line 2650
//   static InstantiateResolver -> Is(this Type, Type),     line 2659
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// The decompiled ForgotRules body is a while/continue/break loop produced by control-flow
// flattening; it is written out below as the plain foreach it started life as. The evaluation
// order is preserved exactly: the two candidate matches are tried *per assembly*, so a Name
// match in an early assembly wins over a FullName match in a later one.
//
// Also present in the decompiled file and deliberately not ported here: FillRules (line 5265),
// which is RequireType without the assembly scan -- a plain Type.GetType that throws when it
// misses. Nothing in the reconstructed package calls it.

using System;
using System.Linq;
using System.Reflection;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Resolves a type by assembly-qualified name, full name, or bare name, returning null when
        /// no loaded assembly declares it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The scan exists because the types this tool reaches for the most are SDK types whose
        /// assembly is not known at compile time: the editor cannot reference the VRChat SDK
        /// directly (it may simply not be installed, and its assembly name has changed across SDK
        /// versions), so callers pass a name string and let this find whatever assembly happens to
        /// carry it in the current domain. <see cref="Type.GetType(string)"/> alone only sees
        /// mscorlib and the calling assembly unless the caller already knows the assembly-qualified
        /// name, which is exactly the thing that cannot be hard-coded.
        /// </para>
        /// <para>
        /// The fallback is expensive: <see cref="Assembly.GetTypes"/> materialises every type in an
        /// assembly, and an editor domain routinely has hundreds of assemblies loaded, so a miss
        /// walks the whole domain. Callers are expected to cache the result rather than resolve
        /// names per frame — see <see cref="AnimatorTypeCache"/>, which memoises even the null
        /// answers.
        /// </para>
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

                // Exact match first so that a namespace-qualified request cannot be answered by a
                // same-named type from an unrelated namespace in the same assembly.
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
        /// Resolves a type the same way <see cref="FindType"/> does, but throws when it is missing.
        /// For lookups where a null type would only surface later as a confusing failure.
        /// </summary>
        internal static Type RequireType(string typeName)
        {
            Type type = FindType(typeName);
            if (type == null)
            {
                throw new Exception("Type \"" + typeName + "\" not found.");
            }

            return type;
        }

        /// <summary>True if <paramref name="type"/> is <typeparamref name="T"/> or a subclass of it.</summary>
        internal static bool Is<T>(this Type type)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                return true;
            }

            return type == typeof(T);
        }

        /// <summary>True if <paramref name="type"/> is <paramref name="other"/> or a subclass of it.</summary>
        internal static bool Is(this Type type, Type other)
        {
            if (type.IsSubclassOf(other))
            {
                return true;
            }

            return type == other;
        }
    }
}
