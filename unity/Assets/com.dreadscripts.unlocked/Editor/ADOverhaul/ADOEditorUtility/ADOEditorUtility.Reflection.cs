// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CancelStatus  -> FindType(string),                       line 2803
//   static RevertStatus  -> OverrideCustomEditor(Type, Type),       line 3741
//   static RunVal        -> RefreshInspectors(),                    line 3755
//   static OrderVal      -> FindMethod(Type, string),               line 3765
//   static CalculateVal  -> FindMethod(Type, string, Type),         line 3783
//   static CalcVal       -> FindMethod(Type, string, Type[]),       line 3801
//   static DeleteVal     -> FindMethod(Type, string, int),          line 3819
//   static _StubSerializer / rulesSerializer / testsSerializer / _DefinitionSerializer (2094-2100)
//                        -> CustomEditorAttributesRefs, a private nested cache
//   static _InitializerSerializer / _TokenSerializer (2102-2104)
//                        -> InspectorWindowRefs, a private nested cache
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// ── 2019 vs 2022 ────────────────────────────────────────────────────────────────────────────────
//
// No behavioural divergence in this region. The same members appear in ADOverhaul2019 under
// different obfuscated names (RevertManager 3758, ReflectParam 3856, CountParam 3869, SetParam
// 3887, DeleteParam 3905, NewParam 3923, and the type lookup at 2822), and reach for exactly the
// same UnityEditor internals: CustomEditorAttributes, CustomEditorAttributes+MonoEditorType,
// kSCustomMultiEditors, m_InspectorType and InspectorWindow.RefreshInspectors. The strings are
// character-for-character identical between the two builds. The expectation that the two builds
// would diverge here — because UnityEditor's internals changed between those Unity versions — is
// not borne out: the *tool* did not adapt, which is precisely why the mechanism is fragile (see the
// remarks on OverrideCustomEditor).
//
// The 2019 build's RevertManager and its type-lookup are rendered by ILSpy as while(true) loops
// over a switch on a rolling state integer — control-flow flattening from the obfuscator, not real
// code. The 2022 build decompiles cleanly and confirms the intended shape; both are written out
// below as the straight-line code they started as.
//
// ── Deliberately not ported here ────────────────────────────────────────────────────────────────
//
// IncludeStatus (3701) sits between the type lookup and OverrideCustomEditor in the decompiled file
// and shares their nested reflection-handle statics (candidateSerializer / helperSerializer /
// readerSerializer, 2088-2092), but it is not a reflection utility: it opens UnityEditor's internal
// ObjectSelector window, i.e. it is an object-picker. It belongs with the GUI regions, not here, and
// its direct counterpart is already ported as EditorUtils.ShowObjectPicker in
// Editor/ControllerEditor/EditorUtils/EditorUtils.Pickers.cs. Deferred rather than duplicated.
//
// The `_003C_003Ec` compiler-generated closure cache (1594) that CalcVal's query references through
// `_003C_003Ec.m_SystemMethod.DisableProduct` is a decompiler artifact for the lambda
// `p => p.ParameterType`; the lambda is restored inline below and the class is not ported.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        private const BindingFlags AllMethods =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Resolves a type by assembly-qualified name, full name, or bare name, returning null when
        /// no loaded assembly declares it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The scan exists because almost everything this tool reaches for lives in an assembly
        /// whose name cannot be written down at compile time: the VRChat SDK (which may not be
        /// installed, and has been renamed across SDK versions) and UnityEditor internals. Callers
        /// pass a name and let this find whichever loaded assembly happens to carry it.
        /// <see cref="Type.GetType(string)"/> alone only searches mscorlib and the calling assembly
        /// unless the caller already knows the assembly-qualified name.
        /// </para>
        /// <para>
        /// Note the evaluation order: the full-name match and the bare-name match are both tried
        /// <em>per assembly</em> before moving on, so a bare-name hit in an early assembly beats a
        /// full-name hit in a later one. This is the shipped order and is preserved.
        /// </para>
        /// <para>
        /// A miss walks every type of every loaded assembly, which in an editor domain is thousands
        /// of types. Callers are expected to cache the result.
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
        /// Lazily-resolved handles onto <c>UnityEditor.CustomEditorAttributes</c>, the internal
        /// registry that maps an inspected type to the editor Unity draws for it.
        /// </summary>
        /// <remarks>
        /// The shipped code guarded the whole block on the <see cref="customEditorAttributes"/> field
        /// alone, so a Unity version that still has the class but has renamed anything below it
        /// leaves the remaining handles null forever and every later call fails on a null reference
        /// rather than re-attempting. That behaviour is kept: the guard below is the same single
        /// check.
        /// </remarks>
        private static class CustomEditorAttributesRefs
        {
            /// <summary><c>UnityEditor.CustomEditorAttributes</c>.</summary>
            internal static Type customEditorAttributes;

            /// <summary>
            /// <c>UnityEditor.CustomEditorAttributes+MonoEditorType</c>, one registry entry: the
            /// inspected type, the editor type, and the flags Unity recorded from the
            /// <c>[CustomEditor]</c> attribute.
            /// </summary>
            internal static Type monoEditorType;

            /// <summary>
            /// The private static <c>kSCustomMultiEditors</c> field: a
            /// <c>Dictionary&lt;Type, List&lt;MonoEditorType&gt;&gt;</c>.
            /// </summary>
            internal static FieldInfo customMultiEditors;

            /// <summary>The public instance <c>MonoEditorType.m_InspectorType</c> field.</summary>
            internal static FieldInfo inspectorType;

            internal static void EnsureResolved()
            {
                if (customEditorAttributes != null)
                {
                    return;
                }

                customEditorAttributes = Type.GetType(
                    "UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                monoEditorType = Type.GetType(
                    "UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                customMultiEditors = customEditorAttributes.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);
                inspectorType = monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        /// <summary>
        /// Replaces the inspector Unity draws for <paramref name="inspectedType"/> with
        /// <paramref name="editorType"/>, by rewriting the entry in UnityEditor's internal custom
        /// editor registry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the whole of ADOverhaul's inspector-replacement mechanism. The assembly contains
        /// no <c>[CustomEditor]</c> attribute anywhere: PhysBoneEditor, PhysBoneColliderEditor and
        /// their siblings are plain <c>Editor</c> subclasses that Unity would never instantiate on
        /// its own. Installing them declaratively is not an option, because the point is to displace
        /// the VRChat SDK's own inspectors — two attributes claiming the same inspected type is a
        /// conflict Unity resolves arbitrarily — and because the replacement has to be reversible at
        /// runtime from a context-menu toggle. So instead ADOverhaul finds the registration the SDK
        /// already made and overwrites the editor recorded in it, which both wins unconditionally
        /// and can be swapped back by calling this again with the SDK's editor type.
        /// </para>
        /// <para>
        /// It depends on four UnityEditor internals, none of which carries any compatibility
        /// guarantee:
        /// </para>
        /// <list type="bullet">
        /// <item><description>the class <c>UnityEditor.CustomEditorAttributes</c>;</description></item>
        /// <item><description>its nested <c>MonoEditorType</c>, which must be a <em>reference</em>
        /// type (see below);</description></item>
        /// <item><description>its private static field <c>kSCustomMultiEditors</c>, which must be a
        /// dictionary keyed by inspected type whose values are indexable lists;</description></item>
        /// <item><description>the public instance field <c>MonoEditorType.m_InspectorType</c>.</description></item>
        /// </list>
        /// <para>
        /// Two details are worth spelling out. First, only the <em>multi</em>-edit table is written,
        /// never <c>kSCustomEditors</c>. That is enough because Unity builds both tables from the
        /// same <c>MonoEditorType</c> instances — an entry that supports multi-object editing is
        /// added to both lists by reference — so mutating the object reachable through the multi
        /// table also changes what the single-object lookup returns. This holds only while
        /// <c>MonoEditorType</c> is a class; were it a struct, <c>list[0]</c> would hand back a boxed
        /// copy and the write would be silently discarded, leaving the SDK's inspector in place.
        /// </para>
        /// <para>
        /// Second, only <c>list[0]</c> is rewritten. If more than one editor is registered for the
        /// inspected type, the others are left alone, and if none is registered — the SDK is absent,
        /// or a Unity version populates the tables lazily and this runs before the first inspector
        /// is drawn — the dictionary lookup returns null and this throws. There is no guard; the
        /// callers only ever pass a type they have just resolved out of the loaded SDK.
        /// </para>
        /// <para>
        /// Consequently this works only on the Unity versions the tool shipped against. Where
        /// UnityEditor renames or restructures any of the four members, the failure is a
        /// <see cref="NullReferenceException"/> at the point of use rather than a graceful fallback,
        /// and — because of the single-field cache guard above — one that repeats on every call.
        /// </para>
        /// </remarks>
        /// <param name="inspectedType">The component type whose inspector is being replaced.</param>
        /// <param name="editorType">The <c>Editor</c> subclass to draw instead.</param>
        internal static void OverrideCustomEditor(Type inspectedType, Type editorType)
        {
            CustomEditorAttributesRefs.EnsureResolved();

            IDictionary editorsByInspectedType = CustomEditorAttributesRefs.customMultiEditors.GetValue(null) as IDictionary;
            IList registrations = editorsByInspectedType[inspectedType] as IList;

            CustomEditorAttributesRefs.inspectorType.SetValue(registrations[0], editorType);

            RefreshInspectors();
        }

        /// <summary>
        /// Lazily-resolved handle onto <c>UnityEditor.InspectorWindow.RefreshInspectors</c>.
        /// </summary>
        private static class InspectorWindowRefs
        {
            internal static Type inspectorWindow;

            internal static MethodInfo refreshInspectors;

            internal static void EnsureResolved()
            {
                if (inspectorWindow != null)
                {
                    return;
                }

                inspectorWindow = Type.GetType(
                    "UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                refreshInspectors = inspectorWindow.GetMethod("RefreshInspectors", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        /// <summary>
        /// Forces every open Inspector to throw away its editor instances and rebuild them.
        /// </summary>
        /// <remarks>
        /// Needed after <see cref="OverrideCustomEditor"/>, because the registry is only consulted
        /// when an editor is created: an Inspector that is already showing the object would keep
        /// drawing the old editor until the selection changed. There is no public API for this —
        /// <c>Repaint</c> would redraw the stale editor, not replace it — so the internal static
        /// <c>InspectorWindow.RefreshInspectors</c> is invoked directly.
        /// </remarks>
        internal static void RefreshInspectors()
        {
            InspectorWindowRefs.EnsureResolved();
            InspectorWindowRefs.refreshInspectors.Invoke(null, null);
        }

        /// <summary>
        /// Finds the single method named <paramref name="name"/> on <paramref name="type"/>, public
        /// or not, instance or static. Logs an error and returns null when there is no match or more
        /// than one.
        /// </summary>
        /// <remarks>
        /// Refusing an ambiguous match rather than picking one is deliberate across this whole
        /// family: these lookups target internal UnityEditor and SDK methods, where quietly binding
        /// to the wrong overload would surface much later as a wrong-argument exception with no clue
        /// where it came from. Note that inherited methods are included, since no
        /// <c>DeclaredOnly</c> is used.
        /// </remarks>
        internal static MethodInfo FindMethod(this Type type, string name)
        {
            MethodInfo[] matches = type.GetMethods(AllMethods).Where(m => m.Name == name).ToArray();

            if (matches.Length == 0)
            {
                Debug.LogError("Method " + name + " not found in " + type.Name);
                return null;
            }

            if (matches.Length > 1)
            {
                Debug.LogError("Multiple methods named " + name + " found in " + type.Name);
                return null;
            }

            return matches[0];
        }

        /// <summary>
        /// Finds the single method named <paramref name="name"/> that takes a parameter of
        /// <paramref name="parameterType"/> anywhere in its signature.
        /// </summary>
        /// <remarks>
        /// The parameter is only required to appear somewhere in the list — its position and the
        /// total parameter count are not constrained — which is enough to separate the overloads
        /// these callers care about while staying tolerant of Unity adding trailing optional
        /// parameters between versions.
        /// </remarks>
        internal static MethodInfo FindMethod(this Type type, string name, Type parameterType)
        {
            MethodInfo[] matches = type.GetMethods(AllMethods)
                .Where(m => m.Name == name && m.GetParameters().Any(p => p.ParameterType == parameterType))
                .ToArray();

            if (matches.Length == 0)
            {
                Debug.LogError("Method " + name + " not found in " + type.Name + " with parameter of type " + parameterType.Name);
                return null;
            }

            if (matches.Length > 1)
            {
                Debug.LogError("Multiple methods named " + name + " found in " + type.Name + " with parameter of type " + parameterType.Name);
                return null;
            }

            return matches[0];
        }

        /// <summary>
        /// Finds the single method named <paramref name="name"/> whose parameter list contains every
        /// type in <paramref name="parameterTypes"/>.
        /// </summary>
        /// <remarks>
        /// This is a subset test, not a signature match: the check is that no requested type is
        /// missing from the method's parameter types, so order is ignored, duplicates collapse, and
        /// a method with extra parameters still matches. An empty <paramref name="parameterTypes"/>
        /// therefore matches every overload of the name. That is the shipped behaviour and is
        /// preserved — the looseness is what makes it survive Unity signature churn, and the
        /// ambiguity check is what stops it binding to the wrong thing when it does not.
        /// </remarks>
        internal static MethodInfo FindMethod(this Type type, string name, Type[] parameterTypes)
        {
            MethodInfo[] matches = type.GetMethods(AllMethods)
                .Where(m => m.Name == name && !parameterTypes.Except(m.GetParameters().Select(p => p.ParameterType)).Any())
                .ToArray();

            if (matches.Length == 0)
            {
                Debug.LogError("Method " + name + " not found in " + type.Name + " with parameters of types "
                    + string.Join(", ", parameterTypes.Select(t => t.Name)));
                return null;
            }

            if (matches.Length > 1)
            {
                Debug.LogError("Multiple methods named " + name + " found in " + type.Name + " with parameters of types "
                    + string.Join(", ", parameterTypes.Select(t => t.Name)));
                return null;
            }

            return matches[0];
        }

        /// <summary>
        /// Finds the single method named <paramref name="name"/> that takes exactly
        /// <paramref name="parameterCount"/> parameters. For overload sets that differ only in
        /// arity, where naming the types would be more brittle than counting them.
        /// </summary>
        internal static MethodInfo FindMethod(this Type type, string name, int parameterCount)
        {
            MethodInfo[] matches = type.GetMethods(AllMethods)
                .Where(m => m.Name == name && m.GetParameters().Length == parameterCount)
                .ToArray();

            if (matches.Length == 0)
            {
                Debug.LogError($"Method {name} not found in {type.Name} with {parameterCount} parameters");
                return null;
            }

            if (matches.Length > 1)
            {
                Debug.LogError($"Multiple methods named {name} found in {type.Name} with {parameterCount} parameters");
                return null;
            }

            return matches[0];
        }
    }
}
