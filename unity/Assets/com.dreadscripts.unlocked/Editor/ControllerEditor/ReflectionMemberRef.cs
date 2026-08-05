// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ReflectionMemberRef.cs
//
// The three [SpecialName] accessors GetFirstParameterType/GetMembers/GetMember were properties
// before the obfuscator split them, and are properties (FirstParameterType/Members/Member) again
// here. The decompile's [CompilerGenerated] FillRecord is the lambda inside Member.
//
// DELIBERATE DEVIATION
// Members casts element-wise (`.Cast<T>().ToArray()`) where the shipped build casts the whole
// array (`(T[])Type.GetMember(...)`). Type.GetMember is declared to return MemberInfo[], so
// whether that array cast succeeds depends on the runtime handing back a covariant array; the
// element-wise cast produces the same T[] without depending on it. The reason it is written down
// rather than treated as cosmetic: this is the one place the port could differ observably, by
// succeeding where the shipped build would have thrown InvalidCastException.
//
// Audit status: VERIFIED -- diffed in full against export/. The eight fields, all five
// constructors (including which two pass matchExactSignature false and which two pass true, and
// the shared default BindingFlags now named AnyMember) and all three accessors match statement for
// statement. The Member getter's branch conditions were checked against the decompile's inverted
// form -- `Length != 1 && FirstParameterType != null` selects the overload scan, everything else
// takes Members[0] -- as was the exact/loose match test inside the scan. The unreferenced static
// pair StopDecorator/ReflectDecorator is not ported, as an obfuscator decoy.

using System;
using System.Linq;
using System.Reflection;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A lazily-resolved reference to a field, property or method on a type that cannot be named at
    /// compile time.
    /// </summary>
    /// <typeparam name="T">
    /// <see cref="FieldInfo"/>, <see cref="PropertyInfo"/> or <see cref="MethodInfo"/>. Which one is
    /// used decides what is searched for.
    /// </typeparam>
    /// <remarks>
    /// Declared as static fields alongside a <see cref="TypeResolver"/>, so the lookup costs nothing
    /// until the member is first used and nothing again afterwards — including when it is not found,
    /// which resolves to null rather than being retried on every access.
    /// </remarks>
    internal class ReflectionMemberRef<T> where T : MemberInfo
    {
        private const BindingFlags AnyMember =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public readonly string memberName;

        public readonly TypeResolver typeResolver;

        public readonly BindingFlags bindingFlags;

        /// <summary>
        /// Parameter types used to pick between overloads. May be a single-element array holding
        /// null, meaning "do not disambiguate".
        /// </summary>
        public readonly Type[] parameterTypes;

        /// <summary>
        /// True when <see cref="parameterTypes"/> must match the full signature in order; false when
        /// it is enough for the overload to take a parameter of the first listed type.
        /// </summary>
        private readonly bool matchExactSignature;

        public bool membersResolved;

        private T[] members;

        private bool memberResolved;

        private T member;

        public ReflectionMemberRef(TypeResolver typeResolver, string memberName, Type[] parameterTypes,
                                   BindingFlags bindingFlags, bool matchExactSignature)
        {
            this.memberName = memberName;
            this.typeResolver = typeResolver;
            this.bindingFlags = bindingFlags;
            this.parameterTypes = parameterTypes;
            this.matchExactSignature = matchExactSignature;
        }

        public ReflectionMemberRef(TypeResolver typeResolver, string memberName, Type parameterType = null,
                                   BindingFlags bindingFlags = AnyMember)
            : this(typeResolver, memberName, new[] { parameterType }, bindingFlags, matchExactSignature: false)
        {
        }

        public ReflectionMemberRef(Type typeResolver, string memberName, Type parameterType = null,
                                   BindingFlags bindingFlags = AnyMember)
            : this(new TypeResolver(typeResolver), memberName, new[] { parameterType }, bindingFlags,
                   matchExactSignature: false)
        {
        }

        public ReflectionMemberRef(TypeResolver typeResolver, string memberName, Type[] parameterTypes,
                                   BindingFlags bindingFlags)
            : this(typeResolver, memberName, parameterTypes, bindingFlags, matchExactSignature: true)
        {
        }

        public ReflectionMemberRef(Type typeResolver, string memberName, Type[] parameterTypes,
                                   BindingFlags bindingFlags)
            : this(new TypeResolver(typeResolver), memberName, parameterTypes, bindingFlags,
                   matchExactSignature: true)
        {
        }

        /// <summary>The type used to pick an overload, or null when overloads are not disambiguated.</summary>
        public Type FirstParameterType => parameterTypes[0];

        /// <summary>Every member of that name, resolved on first access.</summary>
        public T[] Members
        {
            get
            {
                if (membersResolved)
                {
                    return members;
                }

                membersResolved = true;

                MemberTypes memberTypes =
                    typeof(T) == typeof(FieldInfo) ? MemberTypes.Field :
                    typeof(T) == typeof(PropertyInfo) ? MemberTypes.Property :
                    MemberTypes.Method;

                // Cast element-wise rather than casting the array. GetMember is declared to return
                // MemberInfo[], and whether the runtime hands back a T[] that a direct array cast
                // would accept is an implementation detail of the reflection stack.
                members = typeResolver.ResolvedType
                    .GetMember(memberName, memberTypes, bindingFlags)
                    .Cast<T>()
                    .ToArray();

                return members;
            }
        }

        /// <summary>
        /// The single member this refers to, resolved on first access, or null if there is none or
        /// the overloads could not be narrowed to one.
        /// </summary>
        public T Member
        {
            get
            {
                if (memberResolved)
                {
                    return member;
                }

                memberResolved = true;

                if (Members.Length == 0)
                {
                    return null;
                }

                if (Members.Length == 1 || FirstParameterType == null)
                {
                    return member = Members[0];
                }

                foreach (MethodInfo candidate in Members.Cast<MethodInfo>())
                {
                    Type[] candidateTypes = candidate.GetParameters().Select(p => p.ParameterType).ToArray();

                    bool matches = matchExactSignature
                        ? candidateTypes.SequenceEqual(parameterTypes)
                        : candidateTypes.Contains(FirstParameterType);

                    if (matches)
                    {
                        member = (T)(MemberInfo)candidate;
                        break;
                    }
                }

                return member;
            }
        }
    }
}
