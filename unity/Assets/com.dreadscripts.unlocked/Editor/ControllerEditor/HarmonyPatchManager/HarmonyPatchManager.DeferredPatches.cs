// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   (nested type ControllerEditor.HarmonyPatchManager)
//   PatchSwapEntry -> PatchSwapEntry,                      line 2404
//   m_SchemaAlgo   -> deferredPatches,                     line 2559
//   IncludeReg     -> DeferPatch(string, Type, ...),       line 2688
//   RunReg         -> DeferPatch(string, MethodInfo, ...), line 2695
//   CloneReg       -> ApplyDeferredPatch,                  line 2709
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. See HarmonyPatchManager.cs for the type-level header.
//
// Not ported from PatchSwapEntry: the static field ConnectProduct (line 2418) and the method
// ViewProduct (line 2430), which returns whether it is null. Nothing anywhere in the assembly
// assigns ConnectProduct, so ViewProduct is a constant true. Obfuscator/licensing scaffolding
// with no behaviour behind it.
//
// Audit status: VERIFIED -- every member this file declares was diffed statement by statement
// against export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs: PatchSwapEntry
// and its six fields and constructor (2404-2436), deferredPatches (2559), IncludeReg (2688),
// RunReg (2695) and ApplyDeferredPatch (2709). All five cited line numbers still land on the named
// member in the current snapshot. The unported-members note was checked too: ViewProduct is
// `return ConnectProduct == null` and ConnectProduct has no assignment anywhere in the decompiled
// assembly (grep: only its declaration at 2418 and that one read at 2432), so the constant-true
// reading holds. Differences, all behaviour-preserving: `readonly` is added to the deferredPatches
// field, which the shipped code declares as a plain `private static` but never reassigns; and
// RunReg's local-then-store (`PatchSwapEntry value = ...; deferredPatches[reference] = value;`) is
// written as a single indexer assignment here.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class HarmonyPatchManager
    {
        /// <summary>
        /// A patch that has been registered but not applied, together with the method whose first
        /// execution should apply it.
        /// </summary>
        internal struct PatchSwapEntry
        {
            /// <summary>The method watched for a first call.</summary>
            internal readonly MethodInfo triggerMethod;

            /// <summary>
            /// The postfix placed on <see cref="triggerMethod"/>, whose job is to call
            /// <see cref="ApplyDeferredPatch"/> and then remove itself.
            /// </summary>
            internal readonly MethodInfo triggerPatch;

            /// <summary>The method that is actually meant to be patched.</summary>
            internal readonly MethodInfo targetMethod;

            internal readonly MethodInfo prefix;

            internal readonly MethodInfo postfix;

            internal readonly MethodInfo transpiler;

            internal PatchSwapEntry(MethodInfo triggerMethod, MethodInfo triggerPatch, MethodInfo targetMethod,
                MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
            {
                this.triggerMethod = triggerMethod;
                this.triggerPatch = triggerPatch;
                this.targetMethod = targetMethod;
                this.prefix = prefix;
                this.postfix = postfix;
                this.transpiler = transpiler;
            }
        }

        /// <summary>
        /// Patches registered for later application, keyed by the name the trigger postfix passes
        /// back to <see cref="ApplyDeferredPatch"/>.
        /// </summary>
        /// <remarks>
        /// A plain string key rather than the trigger method itself because the postfix that fires
        /// the swap is a static method with no access to the entry it belongs to; a literal name is
        /// the only thing it can carry.
        /// </remarks>
        private static readonly Dictionary<string, PatchSwapEntry> deferredPatches = new Dictionary<string, PatchSwapEntry>();

        /// <summary>
        /// Registers a patch to be applied the first time <paramref name="triggerMethodName"/> on
        /// <paramref name="triggerType"/> runs, resolving both methods by name.
        /// </summary>
        /// <param name="key">The name <paramref name="triggerPatch"/> will pass back.</param>
        /// <param name="triggerPatch">
        /// The postfix to install on the trigger. It is expected to call
        /// <see cref="ApplyDeferredPatch"/> with <paramref name="key"/>.
        /// </param>
        internal static void DeferPatch(string key, Type triggerType, string triggerMethodName, MethodInfo triggerPatch,
            Type targetType, string targetMethodName, MethodInfo prefix = null, MethodInfo postfix = null,
            MethodInfo transpiler = null)
        {
            MethodInfo triggerMethod = AccessTools.GetDeclaredMethods(triggerType).First(m => m.Name == triggerMethodName);
            MethodInfo targetMethod = AccessTools.GetDeclaredMethods(targetType).First(m => m.Name == targetMethodName);
            DeferPatch(key, triggerMethod, triggerPatch, targetMethod, prefix, postfix, transpiler);
        }

        /// <summary>
        /// Registers a patch to be applied the first time <paramref name="triggerMethod"/> runs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The indirection exists because some of the tool's targets are only reachable once the
        /// window that owns them has been constructed: patching them at startup would either fail or
        /// force the editor to load a window the user has not opened. Watching a cheap, always-hit
        /// method on that window -- its <c>OnGUI</c> -- and swapping the real patch in on the first
        /// frame it draws costs nothing until the feature is actually wanted.
        /// </para>
        /// <para>
        /// The entry is recorded before the trigger is patched, so that a trigger patch which
        /// somehow fires during <see cref="Harmony.Patch"/> still finds its entry.
        /// </para>
        /// <para>
        /// Failures here are logged as exceptions to the console rather than folded into
        /// <see cref="patchErrorLog"/> and the patch-failure banner, unlike every other patch site
        /// in this type. Ported as shipped.
        /// </para>
        /// </remarks>
        internal static void DeferPatch(string key, MethodInfo triggerMethod, MethodInfo triggerPatch,
            MethodInfo targetMethod, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            deferredPatches[key] = new PatchSwapEntry(triggerMethod, triggerPatch, targetMethod, prefix, postfix, transpiler);

            try
            {
                defaultHarmony.Patch(triggerMethod, null, new HarmonyMethod(triggerPatch));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Removes the trigger patch registered under <paramref name="key"/> and applies the real
        /// patch it was standing in for.
        /// </summary>
        /// <remarks>
        /// Called from the trigger postfix itself, which means Harmony is asked to unpatch a method
        /// that is currently executing. That is supported -- the running invocation finishes against
        /// the already-emitted body and only subsequent calls see the restored method -- but it does
        /// mean the postfix runs exactly once more than a reader might expect on the frame of the
        /// swap.
        /// <para>
        /// Throws <see cref="KeyNotFoundException"/> for an unregistered key, and the entry is left
        /// in the dictionary after the swap, so a second call re-unpatches an already-unpatched
        /// trigger (a no-op) and applies the target patch a second time on top of the first. Nothing
        /// in the tool calls it twice. Ported as shipped.
        /// </para>
        /// </remarks>
        internal static void ApplyDeferredPatch(string key)
        {
            PatchSwapEntry entry = deferredPatches[key];
            defaultHarmony.Unpatch(entry.triggerMethod, entry.triggerPatch);
            Patch(entry.targetMethod, entry.prefix, entry.postfix, entry.transpiler);
        }
    }
}
