// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   (nested type ControllerEditor.HarmonyPatchManager)
//   MapReg      -> Patch(Type, string, ...),            line 2631
//   ValidateReg -> PatchByParameterType,                line 2636
//   CustomizeReg-> PatchBySignature,                    line 2641
//   RateReg     -> Patch(MethodInfo, ...),              line 2646
//   DestroyReg  -> PatchConstructor(Type, ...),         line 2662
//   GetReg      -> PatchConstructor(Type, Type[], ...), line 2667
//   CalcReg     -> Patch(ConstructorInfo, ...),         line 2672
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. See HarmonyPatchManager.cs for the type-level header,
// the Harmony dependency and the list of deferred members (TestReg, LoginReg).
//
// The five by-name/by-signature helpers are overload-resolution sugar over the two that do the
// work: they only differ in how they pick the MethodBase. Their names are collapsed onto Patch /
// PatchConstructor overloads here where the parameter list already distinguishes them, and kept
// distinct (PatchByParameterType, PatchBySignature) where two overloads would have been
// ambiguous.
//
// The lambdas that select the parameter types in PatchBySignature and PatchConstructor were
// hoisted by the compiler into a _003C_003Ec closure cache; they are restored inline.
//
// Audit status: VERIFIED -- all seven declared methods were diffed statement by statement against
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs (MapReg 2631,
// PatchByParameterType 2636, PatchBySignature 2641, RateReg 2646, DestroyReg 2662, GetReg 2667,
// CalcReg 2672 -- every cited line still lands on the named member). The two closure-cache methods
// the port inlines were read and are both `return p.ParameterType`, so the restored lambdas are
// faithful. The dropped-harmonyId claim was re-checked against the decompiled bodies: MapReg,
// PatchByParameterType and PatchBySignature really do not forward it, DestroyReg and GetReg do.
// Remaining differences are formatting only: RateReg and CalcReg assign their three HarmonyMethod
// ternaries to locals before the Patch call, which the port folds into the argument list.

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace DreadScripts.ControllerEditor
{
    internal static partial class HarmonyPatchManager
    {
        /// <summary>
        /// Patches the single method named <paramref name="methodName"/> declared on
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="harmonyId">
        /// Accepted and then dropped -- this overload does not forward it. See the remarks.
        /// </param>
        /// <remarks>
        /// <para>
        /// <see cref="AccessTools.GetDeclaredMethods"/> rather than <c>Type.GetMethod</c>: the
        /// targets are private and often property getters, and <c>GetDeclaredMethods</c> already
        /// returns the full declared set with the right binding flags. <c>First</c> throws when the
        /// name is absent, which is the intended signal -- the caller catches it in
        /// <see cref="Patch(MethodInfo, MethodInfo, MethodInfo, MethodInfo, string)"/> and turns it
        /// into a logged failure rather than an exception. It also silently takes the first of
        /// several overloads, which is why the two more specific selectors below exist.
        /// </para>
        /// <para>
        /// <b>Shipped bug, preserved.</b> <paramref name="harmonyId"/> is never passed on, so a
        /// caller that asks for a named Harmony instance still gets its patch applied to the
        /// default one. The consequence is not that the patch fails but that it is filed under the
        /// wrong id, so an <c>UnpatchAll</c> aimed at the caller's id would not remove it. In
        /// practice nothing is harmed because <see cref="RemoveAllPatches"/> unpatches every id it
        /// knows about, and the default id is one of them. The same omission is in
        /// <see cref="PatchByParameterType"/> and <see cref="PatchBySignature"/>; the constructor
        /// helpers below do forward theirs.
        /// </para>
        /// </remarks>
        internal static void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null,
            MethodInfo transpiler = null, string harmonyId = "")
        {
            Patch(AccessTools.GetDeclaredMethods(type).First(m => m.Name == methodName), prefix, postfix, transpiler);
        }

        /// <summary>
        /// Patches the overload of <paramref name="methodName"/> on <paramref name="type"/> that
        /// takes a parameter of type <paramref name="parameterType"/>.
        /// </summary>
        /// <remarks>
        /// For targets whose name alone is ambiguous but where one parameter type is enough to
        /// distinguish them -- the graph's <c>AddNode</c> pair, where only one takes a bool.
        /// <paramref name="harmonyId"/> is dropped; see
        /// <see cref="Patch(Type, string, MethodInfo, MethodInfo, MethodInfo, string)"/>.
        /// </remarks>
        internal static void PatchByParameterType(Type type, Type parameterType, string methodName,
            MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
        {
            Patch(
                AccessTools.GetDeclaredMethods(type)
                    .First(m => m.Name == methodName && m.GetParameters().Any(p => p.ParameterType == parameterType)),
                prefix, postfix, transpiler);
        }

        /// <summary>
        /// Patches the overload of <paramref name="methodName"/> on <paramref name="type"/> whose
        /// parameter types are exactly <paramref name="parameterTypes"/>, in order.
        /// </summary>
        /// <remarks>
        /// <paramref name="harmonyId"/> is dropped; see
        /// <see cref="Patch(Type, string, MethodInfo, MethodInfo, MethodInfo, string)"/>.
        /// </remarks>
        internal static void PatchBySignature(Type type, Type[] parameterTypes, string methodName,
            MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
        {
            Patch(
                AccessTools.GetDeclaredMethods(type)
                    .First(m => m.Name == methodName && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)),
                prefix, postfix, transpiler);
        }

        /// <summary>
        /// Applies the given prefix, postfix and transpiler to <paramref name="target"/>, recording
        /// rather than propagating any failure.
        /// </summary>
        /// <param name="harmonyId">
        /// The instance to file the patch under; empty means the tool's default instance.
        /// </param>
        /// <remarks>
        /// This is where every patch in the tool ultimately lands, and where the fail-soft policy
        /// lives: a target that resolves but cannot be patched -- because Unity changed its
        /// signature, or because Harmony cannot emit over it -- costs the tool one feature and a
        /// line in the error log rather than an exception.
        /// <para>
        /// The catch covers less than it looks like it does, and that is preserved as shipped: the
        /// <c>First(...)</c> selectors in the four helpers above run while evaluating the arguments
        /// to this call, so a target that cannot be <i>found</i> at all throws
        /// <see cref="InvalidOperationException"/> from the helper and escapes to whichever
        /// registration routine named it, aborting the rest of that patch set. Only failures inside
        /// <see cref="Harmony.Patch"/> itself are absorbed. A Unity release that renames an internal
        /// therefore loses a whole group of patches, not just the one.
        /// </para>
        /// </remarks>
        internal static void Patch(MethodInfo target, MethodInfo prefix = null, MethodInfo postfix = null,
            MethodInfo transpiler = null, string harmonyId = "")
        {
            try
            {
                GetHarmony(harmonyId).Patch(target,
                    prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix != null ? new HarmonyMethod(postfix) : null,
                    transpiler != null ? new HarmonyMethod(transpiler) : null);
            }
            catch (Exception e)
            {
                patchingFailed = true;
                patchErrorLog = patchErrorLog + e.Message + "\n";
            }
        }

        /// <summary>
        /// Patches the first declared constructor of <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// "First declared" is only safe for single-constructor types; use
        /// <see cref="PatchConstructor(Type, Type[], MethodInfo, MethodInfo, MethodInfo, string)"/>
        /// otherwise.
        /// </remarks>
        internal static void PatchConstructor(Type type, MethodInfo prefix = null, MethodInfo postfix = null,
            MethodInfo transpiler = null, string harmonyId = "")
        {
            Patch(AccessTools.GetDeclaredConstructors(type).First(), prefix, postfix, transpiler, harmonyId);
        }

        /// <summary>
        /// Patches the constructor of <paramref name="type"/> whose parameter types are exactly
        /// <paramref name="parameterTypes"/>, in order.
        /// </summary>
        internal static void PatchConstructor(Type type, Type[] parameterTypes, MethodInfo prefix = null,
            MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
        {
            Patch(
                AccessTools.GetDeclaredConstructors(type)
                    .First(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)),
                prefix, postfix, transpiler, harmonyId);
        }

        /// <summary>
        /// Applies the given prefix, postfix and transpiler to a constructor, recording rather than
        /// propagating any failure.
        /// </summary>
        /// <remarks>
        /// Identical in every respect to
        /// <see cref="Patch(MethodInfo, MethodInfo, MethodInfo, MethodInfo, string)"/> -- the two
        /// exist separately only because <see cref="Harmony.Patch"/> takes a
        /// <see cref="MethodBase"/> and the tool wanted the argument type to say which kind of
        /// member it is patching.
        /// </remarks>
        internal static void Patch(ConstructorInfo target, MethodInfo prefix = null, MethodInfo postfix = null,
            MethodInfo transpiler = null, string harmonyId = "")
        {
            try
            {
                GetHarmony(harmonyId).Patch(target,
                    prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix != null ? new HarmonyMethod(postfix) : null,
                    transpiler != null ? new HarmonyMethod(transpiler) : null);
            }
            catch (Exception e)
            {
                patchingFailed = true;
                patchErrorLog = patchErrorLog + e.Message + "\n";
            }
        }
    }
}
