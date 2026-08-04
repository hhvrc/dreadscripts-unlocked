// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/TypeResolver.cs

using System;
using System.Reflection;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Resolves a <see cref="Type"/> from its name once, on first use, and caches the result —
    /// including the failure, so a type that is not present is looked up only once.
    /// </summary>
    /// <remarks>
    /// ControllerEditor drives Unity's internal animator graph window, whose types live in
    /// <c>UnityEditor.Graphs</c> and cannot be referenced directly. Declaring them as
    /// <see cref="TypeResolver"/> fields keeps the lookup lazy, so a Unity version that has moved or
    /// removed one of them costs a null rather than a type-load failure at class-initialisation time.
    /// </remarks>
    internal class TypeResolver
    {
        public readonly string typeName;

        /// <summary>
        /// Scan every loaded assembly when the name alone does not resolve. Only needed for names
        /// that are not assembly-qualified.
        /// </summary>
        public readonly bool searchAllAssemblies;

        public bool resolved;

        private Type cachedType;

        public TypeResolver(string typeName, bool searchAllAssemblies = false)
        {
            this.typeName = typeName;
            this.searchAllAssemblies = searchAllAssemblies;
        }

        public TypeResolver(Type type)
        {
            cachedType = type;
            typeName = type.FullName;
            resolved = true;
        }

        /// <summary>The resolved type, or null if the name could not be resolved.</summary>
        /// <remarks>
        /// DEOBF-BUG(guessed): de4dot could not recover this body at all — the shipped build
        /// decompiles to an empty infinite loop where the lookup used to be, so there is nothing
        /// here to transcribe. The body below is inferred from the surrounding members: the two
        /// fields and the "resolve once" flag only admit roughly this shape.
        /// This is the weakest reconstruction in the package. Unlike the other DEOBF-BUG sites it
        /// is not backed by a cross-build copy or an IL trace, so the search order and the
        /// null-handling are plausible rather than established. If de4dot's control-flow recovery
        /// improves, re-derive this body from the new decompiled/ before trusting it.
        /// </remarks>
        public Type ResolvedType
        {
            get
            {
                if (resolved)
                {
                    return cachedType;
                }

                resolved = true;
                cachedType = Type.GetType(typeName);

                if (cachedType == null && searchAllAssemblies)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        cachedType = assembly.GetType(typeName);
                        if (cachedType != null)
                        {
                            break;
                        }
                    }
                }

                return cachedType;
            }
        }

        public static implicit operator Type(TypeResolver resolver)
        {
            return resolver.ResolvedType;
        }
    }
}
