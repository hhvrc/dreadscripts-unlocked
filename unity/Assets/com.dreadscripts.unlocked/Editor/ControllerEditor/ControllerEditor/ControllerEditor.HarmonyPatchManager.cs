// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   HarmonyPatchManager  -> HarmonyPatchManager, lines 2402-3017 (name already in renames/)
//     PatchSwapEntry     -> PatchSwapEntry,      lines 2404-2434 (members already in renames/)
//       ConnectProduct / ViewProduct() -> dropped, lines 2418 and 2430 (obfuscator sentinel)
//     methodAlgo         -> namedHarmonyInstances, line 2557
//     m_SchemaAlgo       -> deferredPatches,       line 2559
//     m_BroadcasterAlgo  -> NOT NAMED (kept as-is), line 2561 -- see note below
//     _ProxyAlgo         -> hasPatchFailure,       line 2563
//     _StructAlgo        -> isRetrying,            line 2565
//     _ServiceAlgo       -> warningDismissed,      line 2567
//     _StateAlgo         -> hasAutoRetried,        line 2569
//     _GlobalAlgo        -> patchErrorLog,         line 2571
//     _TaskAlgo          -> oneTimeSetup,          line 2573
//     AddTests()         -> DefaultHarmony,        line 2576  [SpecialName getter]
//     ConnectReg         -> RunOneTimeSetup,       line 2582
//     CalculateReg       -> UnpatchAll,            line 2597
//     TestReg            -> PatchMethod(string typeName, ...),        line 2618
//     MapReg             -> PatchMethod(Type, string, ...),           line 2631
//     ValidateReg        -> PatchMethod(Type, Type, string, ...),     line 2636
//     CustomizeReg       -> PatchMethod(Type, Type[], string, ...),   line 2641
//     RateReg            -> Patch(MethodInfo, ...),                   line 2646
//     DestroyReg         -> PatchConstructor(Type, ...),              line 2662
//     GetReg             -> PatchConstructor(Type, Type[], ...),      line 2667
//     CalcReg            -> Patch(ConstructorInfo, ...),              line 2672
//     IncludeReg         -> PatchWhenTriggered(string, Type, string, MethodInfo, Type, string, ...), line 2688
//     RunReg             -> PatchWhenTriggered(string, MethodInfo, MethodInfo, MethodInfo, ...),     line 2695
//     CloneReg           -> ApplyDeferredPatch,    line 2709
//     LoginReg           -> DrawPatchFailureBar,   line 2716
//     ReflectReg         -> RetryPatching,         line 2771
//     DeleteReg          -> GetHarmony,            line 2780
//     <>c.FindTests / <>c.ExcludeTests / <>c.InitTests / <>c.VisitTests -> dissolved back into the
//                           lambdas at their use sites, lines 2510-2554
//
// The MethodInfo-of-delegate helpers and the ref/out delegate family are in
// ControllerEditor.HarmonyPatchManager.Delegates.cs.
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// m_BroadcasterAlgo is deliberately left unnamed: it is written once (`= false` in RetryPatching)
// and read nowhere in the assembly, so there is no evidence of what it means. Naming it would be a
// guess; renaming it later costs nothing.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   RevertWrapper, FindVisitor  -- ControllerEditor outer class body (install every patch; the
//                                  tool's own logging entry point)
//   EditorUtils.CountRules      -- EditorUtils (not yet ported): runs an action on the main thread
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DreadScripts.Common;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// Every Harmony patch the tool installs into the Unity editor, and the recovery UI shown
        /// when one of them fails to apply.
        /// </summary>
        /// <remarks>
        /// Patching runs once per domain load and is not allowed to take the tool down with it: each
        /// call is wrapped, a failure only sets <see cref="hasPatchFailure"/> and appends to
        /// <see cref="patchErrorLog"/>, and the window then shows a bar offering a retry.
        /// The observed cause is special characters in the project path, which is what the bar says.
        ///
        /// <see cref="PatchWhenTriggered"/> is the deferred half: rather than patching an editor
        /// type that may not be loaded yet, it patches a cheap trigger method, and the trigger's
        /// postfix calls <see cref="ApplyDeferredPatch"/> to install the real one.
        /// </remarks>
        internal static partial class HarmonyPatchManager
        {
            /// <summary>
            /// A patch held back until its trigger fires: the trigger method and the postfix that
            /// watches it, plus the patch to install once it does.
            /// </summary>
            internal struct PatchSwapEntry
            {
                internal readonly MethodInfo triggerMethod;

                internal readonly MethodInfo triggerPatch;

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
            /// Harmony instances keyed by id, created on demand so a patch can have its own unpatch
            /// scope. <see cref="DefaultHarmony"/> is one of these, under the tool's own id.
            /// </summary>
            internal static Dictionary<string, Harmony> namedHarmonyInstances;

            private static Dictionary<string, PatchSwapEntry> deferredPatches =
                new Dictionary<string, PatchSwapEntry>();

            // Written once in RetryPatching and read nowhere. Name deliberately not chosen; see the
            // file header.
            internal static bool m_BroadcasterAlgo;

            /// <summary>At least one patch threw while being applied.</summary>
            internal static bool hasPatchFailure;

            /// <summary>An automatic retry is scheduled or running.</summary>
            internal static bool isRetrying;

            /// <summary>The user pressed Hide on the failure bar.</summary>
            internal static bool warningDismissed;

            /// <summary>The one automatic retry has already been spent.</summary>
            internal static bool hasAutoRetried;

            internal static string patchErrorLog;

            /// <summary>
            /// Setup that must run exactly once per domain load, each with a flag saying whether it
            /// already has. <see cref="UnpatchAll"/> clears the flags so a retry re-runs them.
            /// </summary>
            internal static readonly (Action, bool)[] oneTimeSetup = { (RevertWrapper, false) };

            internal static Harmony DefaultHarmony => GetHarmony("com.dreadscripts.controllereditor.tool");

            [CallbackMethod(0)]
            internal static void RunOneTimeSetup()
            {
                for (int i = 0; i < oneTimeSetup.Length; i++)
                {
                    (Action, bool) entry = oneTimeSetup[i];
                    var (action, _) = entry;
                    if (!entry.Item2)
                    {
                        oneTimeSetup[i] = (action, true);
                        oneTimeSetup[i].Item1();
                    }
                }
            }

            [ControllerCallback(0)]
            internal static void UnpatchAll()
            {
                if (namedHarmonyInstances != null)
                {
                    foreach (KeyValuePair<string, Harmony> instance in namedHarmonyInstances)
                    {
                        instance.Value.UnpatchAll(instance.Key);
                    }

                    namedHarmonyInstances.Clear();
                }

                for (int i = 0; i < oneTimeSetup.Length; i++)
                {
                    (Action, bool) entry = oneTimeSetup[i];
                    var (action, _) = entry;
                    if (entry.Item2)
                    {
                        oneTimeSetup[i] = (action, false);
                    }
                }
            }

            /// <summary>Patches a method on a type named at runtime, logging if the type is absent.</summary>
            internal static void PatchMethod(string typeName, string methodName, MethodInfo prefix = null,
                MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
            {
                Type type = EditorUtils.FindType(typeName);
                if (type == null)
                {
                    FindVisitor("Couldn't find patch target type:\n" + typeName, CustomLogType.Error);
                    return;
                }

                PatchMethod(type, methodName, prefix, postfix, transpiler);
            }

            internal static void PatchMethod(Type type, string methodName, MethodInfo prefix = null,
                MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
            {
                Patch(AccessTools.GetDeclaredMethods(type).First(m => m.Name == methodName),
                    prefix, postfix, transpiler);
            }

            /// <summary>Overload for an ambiguous name, disambiguated by one parameter type.</summary>
            internal static void PatchMethod(Type type, Type parameterType, string methodName,
                MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null,
                string harmonyId = "")
            {
                Patch(AccessTools.GetDeclaredMethods(type)
                        .First(m => m.Name == methodName
                                    && m.GetParameters().Any(p => p.ParameterType == parameterType)),
                    prefix, postfix, transpiler);
            }

            /// <summary>Overload for an ambiguous name, disambiguated by the full parameter list.</summary>
            internal static void PatchMethod(Type type, Type[] parameterTypes, string methodName,
                MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null,
                string harmonyId = "")
            {
                Patch(AccessTools.GetDeclaredMethods(type)
                        .First(m => m.Name == methodName
                                    && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)),
                    prefix, postfix, transpiler);
            }

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
                    hasPatchFailure = true;
                    patchErrorLog = patchErrorLog + e.Message + "\n";
                }
            }

            internal static void PatchConstructor(Type type, MethodInfo prefix = null, MethodInfo postfix = null,
                MethodInfo transpiler = null, string harmonyId = "")
            {
                Patch(AccessTools.GetDeclaredConstructors(type).First(), prefix, postfix, transpiler, harmonyId);
            }

            internal static void PatchConstructor(Type type, Type[] parameterTypes, MethodInfo prefix = null,
                MethodInfo postfix = null, MethodInfo transpiler = null, string harmonyId = "")
            {
                Patch(AccessTools.GetDeclaredConstructors(type)
                        .First(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes)),
                    prefix, postfix, transpiler, harmonyId);
            }

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
                    hasPatchFailure = true;
                    patchErrorLog = patchErrorLog + e.Message + "\n";
                }
            }

            /// <summary>
            /// Registers a patch to be installed later, naming the trigger method and the real
            /// target by type and method name.
            /// </summary>
            internal static void PatchWhenTriggered(string id, Type triggerType, string triggerMethodName,
                MethodInfo triggerPatch, Type targetType, string targetMethodName,
                MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
            {
                MethodInfo trigger = AccessTools.GetDeclaredMethods(triggerType)
                    .First(m => m.Name == triggerMethodName);
                MethodInfo target = AccessTools.GetDeclaredMethods(targetType)
                    .First(m => m.Name == targetMethodName);

                PatchWhenTriggered(id, trigger, triggerPatch, target, prefix, postfix, transpiler);
            }

            internal static void PatchWhenTriggered(string id, MethodInfo triggerMethod, MethodInfo triggerPatch,
                MethodInfo targetMethod, MethodInfo prefix = null, MethodInfo postfix = null,
                MethodInfo transpiler = null)
            {
                deferredPatches[id] = new PatchSwapEntry(triggerMethod, triggerPatch, targetMethod,
                    prefix, postfix, transpiler);

                try
                {
                    DefaultHarmony.Patch(triggerMethod, null, new HarmonyMethod(triggerPatch));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            /// <summary>
            /// Removes the trigger patch registered under <paramref name="id"/> and installs the
            /// real patch it was waiting for.
            /// </summary>
            internal static void ApplyDeferredPatch(string id)
            {
                PatchSwapEntry entry = deferredPatches[id];
                DefaultHarmony.Unpatch(entry.triggerMethod, entry.triggerPatch);
                Patch(entry.targetMethod, entry.prefix, entry.postfix, entry.transpiler);
            }

            /// <summary>
            /// The bar shown across the top of the window after a patch failed: one automatic retry
            /// four seconds later, then a manual Retry and a Hide button.
            /// </summary>
            internal static void DrawPatchFailureBar()
            {
                if (!hasPatchFailure || warningDismissed)
                {
                    return;
                }

                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    if (!hasAutoRetried && !isRetrying)
                    {
                        isRetrying = true;
                        Task.Run(async delegate
                        {
                            await Task.Delay(4000);
                            EditorUtils.CountRules(delegate
                            {
                                try
                                {
                                    RetryPatching();
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                }
                                finally
                                {
                                    isRetrying = false;
                                }
                            });
                        });
                    }

                    GUILayout.Label(new GUIContent(EditorUtils.contents.invalidPattern)
                    {
                        tooltip = "This may happen if there were special characters in the project's path.\n\n"
                                  + "Simple error log:\n" + patchErrorLog
                    }, EditorUtils.styles.iconButton, GUILayout.Width(18f));

                    GUILayout.Label("Patching not fully successful. Some functions/patches may be missing.",
                        GUILayout.ExpandWidth(false));

                    if (isRetrying)
                    {
                        GUILayout.Label("Retrying...", GUILayout.ExpandWidth(false));
                    }

                    GUILayout.FlexibleSpace();

                    if (hasAutoRetried)
                    {
                        if (EditorUtils.Button("Hide", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                        {
                            warningDismissed = true;
                        }

                        if (EditorUtils.Button("Retry", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                        {
                            RetryPatching();
                        }
                    }
                }
            }

            private static void RetryPatching()
            {
                hasAutoRetried = true;
                UnpatchAll();
                hasPatchFailure = false;
                m_BroadcasterAlgo = false;
                RevertWrapper();
            }

            /// <summary>
            /// The Harmony instance for an id, created on first use. An empty id means the tool's
            /// own default instance.
            /// </summary>
            private static Harmony GetHarmony(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return DefaultHarmony;
                }

                if (namedHarmonyInstances == null)
                {
                    namedHarmonyInstances = new Dictionary<string, Harmony>();
                }

                if (!namedHarmonyInstances.TryGetValue(id, out Harmony harmony))
                {
                    harmony = new Harmony(id);
                    namedHarmonyInstances.Add(id, harmony);
                }

                return harmony;
            }
        }
    }
}
