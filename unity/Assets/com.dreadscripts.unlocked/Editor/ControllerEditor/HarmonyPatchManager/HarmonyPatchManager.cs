// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   (nested type ControllerEditor.HarmonyPatchManager, lines 2402-3017, lifted to top level here
//   following the precedent set by PhysBoneEditor)
//
//   methodAlgo        -> harmonyInstances,   line 2557
//   m_SchemaAlgo      -> deferredPatches,    line 2559   (see .DeferredPatches.cs)
//   _ProxyAlgo        -> patchingFailed,     line 2563
//   _StateAlgo        -> hasRetried,         line 2569
//   _GlobalAlgo       -> patchErrorLog,      line 2571
//   _TaskAlgo         -> patchAppliers,      line 2573
//   AddTests          -> defaultHarmony,     line 2576   ([SpecialName] -- a property getter that
//                                                        ILSpy could not re-associate because the
//                                                        obfuscator renamed it off get_*)
//   ConnectReg        -> ApplyPatches,       line 2582
//   CalculateReg      -> RemoveAllPatches,   line 2597
//   ReflectReg        -> RetryPatching,      line 2771
//   DeleteReg         -> GetHarmony,         line 2780
//
//   MapReg            -> Patch(Type, string, ...),              line 2631  (.Patching.cs)
//   ValidateReg       -> PatchByParameterType,                  line 2636  (.Patching.cs)
//   CustomizeReg      -> PatchBySignature,                      line 2641  (.Patching.cs)
//   RateReg           -> Patch(MethodInfo, ...),                line 2646  (.Patching.cs)
//   DestroyReg        -> PatchConstructor(Type, ...),           line 2662  (.Patching.cs)
//   GetReg            -> PatchConstructor(Type, Type[], ...),   line 2667  (.Patching.cs)
//   CalcReg           -> Patch(ConstructorInfo, ...),           line 2672  (.Patching.cs)
//   IncludeReg        -> DeferPatch(string, Type, ...),         line 2688  (.DeferredPatches.cs)
//   RunReg            -> DeferPatch(string, MethodInfo, ...),   line 2695  (.DeferredPatches.cs)
//   CloneReg          -> ApplyDeferredPatch,                    line 2709  (.DeferredPatches.cs)
//   PatchSwapEntry    -> PatchSwapEntry,                        line 2404  (.DeferredPatches.cs)
//   RefAction/RefFunc/OutAction/OutFunc/Val* delegates          line 2436-2494 (.Delegates.cs)
//   CreateReg ... QueryTests (48 delegate->MethodInfo helpers)  line 2798-3016 (.MethodOf.cs)
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// ── What this type is ────────────────────────────────────────────────────────────────────────
// Despite the name, HarmonyPatchManager patches nothing in particular. It is the registration and
// lifecycle layer: it owns the Harmony instances, resolves patch targets by name/signature through
// reflection, swallows and accumulates patch failures so a missing internal cannot take the whole
// tool down, and provides the compile-time-checked delegate->MethodInfo helpers that let call sites
// name a patch method without a reflection string. The actual patch targets -- the UnityEditor
// internals -- are named at the call sites in the ControllerEditor god class (decompiled lines
// 15301-17987), not here. See the "What actually gets patched" note at the bottom of this file.
//
// ── Dependency ───────────────────────────────────────────────────────────────────────────────
// HarmonyLib (Lib.Harmony / 0Harmony.dll), a real assembly reference, not a hand-rolled
// equivalent. Two copies exist in this project:
//   * unity/Assets/com.dreadscripts.unlocked/Editor/Dependencies/0Harmony.dll, whose importer
//     carries `defineConstraints: ['!VRC_SDK_VRCSDK3']` -- the bundled copy is excluded whenever
//     the VRChat SDK is installed. (CHANGELOG.ControllerEditor.md: "[Fix] Disabled Harmony when
//     VRCSDK is included.")
//   * unity/Packages/com.vrchat.base/Runtime/VRCSDK/Plugins/Harmony/0Harmony.dll, version 2.0.5,
//     which the VRChat SDK ships and which this project therefore compiles against.
// The exclusion is a duplicate-assembly guard, not a feature toggle: exactly one 0Harmony is ever
// loaded, so HarmonyLib is always available and this file always compiles. Every API used here
// (Harmony ctor, Patch(MethodBase, ...), Unpatch(MethodBase, MethodInfo), UnpatchAll(string),
// AccessTools.GetDeclaredMethods, AccessTools.GetDeclaredConstructors) exists in 2.0.5.
//
// ── Deliberately not ported ──────────────────────────────────────────────────────────────────
// * PatchSwapEntry.ConnectProduct (line 2418) and PatchSwapEntry.ViewProduct() (line 2430): an
//   always-null static object paired with a method that returns whether it is null. Nothing ever
//   assigns it. Obfuscator/licensing-gate scaffolding.
// * _003C_003Ec (line 2498): the compiler-generated closure cache. Its FindTests/ExcludeTests are
//   the lambda `p => p.ParameterType`, restored inline at their two call sites; InitTests and
//   VisitTests are the retry lambda from DrawPatchFailureBar. No such struct exists in the source
//   the decompiler was reading.
// * m_BroadcasterAlgo (line 2561) and its single assignment in RetryPatching (line 2776): a bool
//   that is declared and written once and never read, anywhere in the assembly. Omitting it and
//   its assignment has no behavioural effect.
//
// ── Deferred: needs members that are not in the package yet ──────────────────────────────────
// * TestReg (line 2618) -- "resolve a patch target type by assembly-qualified name, then patch a
//   method on it by name; log an error if the type is missing". It is PatchByName over
//   EditorUtils.FindType plus one call to ControllerEditor.FindVisitor (decompiled line 10886),
//   the god class's own rich-text console logger. FindVisitor is not ported, and EditorUtils'
//   loggers format differently, so substituting one would change observable output.
// * LoginReg (line 2716) -- the IMGUI failure banner drawn into the tool's toolbar ("Patching not
//   fully successful. Some functions/patches may be missing.") with a Hide and a Retry button and
//   an automatic one-shot retry 4 seconds after the banner first appears. It needs
//   EditorUtils.CountRules (decompiled EditorUtils.cs line 4444), the "run this action on the next
//   EditorApplication.delayCall" main-thread marshal, which is not ported; the auto-retry runs on
//   a Task.Run background thread and cannot touch the Harmony API without it.
//   Two fields exist only to serve that banner and go with it: _StructAlgo (line 2565, "the
//   automatic retry is waiting out its 4-second delay") and _ServiceAlgo (line 2567, "the user
//   dismissed the banner for this session"). hasRetried below is the third and is kept because
//   RetryPatching writes it.
//
// ── DELIBERATE DEVIATION ─────────────────────────────────────────────────────────────────────
// patchAppliers is declared empty here. The decompiled initialiser is
//
//     internal static readonly (Action, bool)[] _TaskAlgo = new (Action, bool)[1] { (RevertWrapper, false) };
//
// and RetryPatching's last statement is a direct `RevertWrapper();`. ControllerEditor.RevertWrapper
// (decompiled line 8906) is the god class's "wire up selection callbacks and apply every patch
// set" entry point; it is not ported and lives outside this file's scope. Both references are
// omitted, so ApplyPatches iterates nothing and RetryPatching re-applies nothing. Restoring them
// is two lines once RevertWrapper exists -- put it back in the array initialiser and re-add the
// call marked below. Nothing else in this type changes.

using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Owns the tool's Harmony instances and the apply/remove lifecycle around them, and records
    /// patch failures rather than letting them escape.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Everything this tool changes about the animator, animation and layer windows is a runtime
    /// patch over UnityEditor internals: those windows expose no extension points, so the only way
    /// to draw into them, recolour them or veto their behaviour is to intercept their private
    /// methods. That makes the whole feature set contingent on internals Unity is free to rename
    /// between versions, which is why a failed patch is recorded rather than thrown: the failure is
    /// appended to <see cref="patchErrorLog"/> and flagged in <see cref="patchingFailed"/>, and the
    /// tool runs with that one feature missing instead of failing to load.
    /// </para>
    /// <para>
    /// That safety net has a hole worth knowing about on a Unity version whose internals differ.
    /// It only covers the patching itself; the reflection lookups that find the targets are
    /// evaluated as arguments to the patch calls, so a renamed or removed internal throws
    /// <see cref="InvalidOperationException"/> out of the registration routine that named it and
    /// takes the rest of that routine's patch set with it. Renamed internals therefore fail in
    /// groups, and only failures inside Harmony itself degrade one feature at a time. See the
    /// remarks on the patch helpers.
    /// </para>
    /// <para>
    /// Patches are applied and removed explicitly, not by <c>[InitializeOnLoad]</c>. Application is
    /// driven by the <see cref="CallbackMethodAttribute"/> pipeline the tool runs when its window
    /// opens, and removal by the <see cref="ControllerCallbackAttribute"/> pipeline; a domain
    /// reload therefore drops every patch along with the appdomain and the window re-applies them
    /// when it next opens. This is deliberate: patched editor internals must not stay patched while
    /// the tool's window is closed.
    /// </para>
    /// </remarks>
    internal static partial class HarmonyPatchManager
    {
        /// <summary>The Harmony id used for every patch that does not name its own.</summary>
        private const string defaultHarmonyId = "com.dreadscripts.controllereditor.tool";

        /// <summary>
        /// Every Harmony instance created so far, keyed by id, so that <see cref="RemoveAllPatches"/>
        /// can unpatch each id's patches without knowing which methods they landed on.
        /// </summary>
        /// <remarks>
        /// Left null until the first <see cref="GetHarmony"/> call rather than initialised inline:
        /// creating the dictionary is the only work that happens before any patch is requested, and
        /// <see cref="RemoveAllPatches"/> uses the null to mean "nothing was ever patched".
        /// </remarks>
        internal static Dictionary<string, Harmony> harmonyInstances;

        /// <summary>
        /// Set when any patch attempt threw, which makes the tool draw its "patching not fully
        /// successful" banner.
        /// </summary>
        internal static bool patchingFailed;

        /// <summary>
        /// True once patching has been retried at least once, which is what turns the banner's
        /// automatic retry into the manual Hide/Retry pair.
        /// </summary>
        internal static bool hasRetried;

        /// <summary>
        /// The accumulated exception messages from every failed patch, shown in the banner's tooltip.
        /// </summary>
        /// <remarks>
        /// Never cleared, not even by <see cref="RetryPatching"/>: a successful retry leaves the
        /// previous run's messages in the tooltip. Ported as shipped.
        /// </remarks>
        internal static string patchErrorLog;

        /// <summary>
        /// The patch-application entry points, each paired with whether it has already run.
        /// </summary>
        /// <remarks>
        /// The flag is what makes <see cref="ApplyPatches"/> idempotent: the callback pipeline that
        /// invokes it runs on every tool window that opens, and applying a Harmony patch twice would
        /// stack two copies of every prefix.
        /// <para>
        /// Empty in this port -- see the DELIBERATE DEVIATION note in the file header.
        /// </para>
        /// </remarks>
        internal static readonly (Action apply, bool applied)[] patchAppliers = new (Action apply, bool applied)[0];

        /// <summary>The Harmony instance every patch that does not name an id lands on.</summary>
        private static Harmony defaultHarmony
        {
            get
            {
                return GetHarmony(defaultHarmonyId);
            }
        }

        /// <summary>
        /// Runs every patch-application entry point that has not run yet.
        /// </summary>
        [CallbackMethod(0)]
        internal static void ApplyPatches()
        {
            for (int i = 0; i < patchAppliers.Length; i++)
            {
                (Action apply, bool applied) entry = patchAppliers[i];
                if (!entry.applied)
                {
                    // The flag is written before the call, not after, so an entry point that throws
                    // is not retried by the next ApplyPatches -- a half-applied patch set is left
                    // half-applied rather than being applied twice on top of itself.
                    patchAppliers[i] = (entry.apply, true);
                    patchAppliers[i].apply();
                }
            }
        }

        /// <summary>
        /// Unpatches everything this tool applied and re-arms the entry points, restoring the editor
        /// to stock behaviour.
        /// </summary>
        [ControllerCallback(0)]
        internal static void RemoveAllPatches()
        {
            if (harmonyInstances != null)
            {
                foreach (KeyValuePair<string, Harmony> instance in harmonyInstances)
                {
                    instance.Value.UnpatchAll(instance.Key);
                }

                harmonyInstances.Clear();
            }

            for (int i = 0; i < patchAppliers.Length; i++)
            {
                (Action apply, bool applied) entry = patchAppliers[i];
                if (entry.applied)
                {
                    patchAppliers[i] = (entry.apply, false);
                }
            }
        }

        /// <summary>
        /// Tears down every patch and applies them again from scratch, for when the first attempt
        /// partly failed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A retry is worth offering because the common cause of failure is transient rather than
        /// structural: patching runs early, and a target type whose assembly had not finished
        /// loading resolves fine a few seconds later.
        /// </para>
        /// <para>
        /// The deferred-patch registry is deliberately not cleared here, so a lazy patch that had
        /// already swapped in stays swapped in across a retry -- but its trigger patch was just
        /// removed by <see cref="RemoveAllPatches"/>, so the <see cref="ApplyDeferredPatch"/> call
        /// that would fire it can no longer happen. A retry therefore permanently disarms any
        /// deferred patch that had not triggered yet. Ported as shipped.
        /// </para>
        /// </remarks>
        private static void RetryPatching()
        {
            hasRetried = true;
            RemoveAllPatches();
            patchingFailed = false;

            // DELIBERATE DEVIATION: `RevertWrapper();` -- the god class's apply-everything entry
            // point -- is the next and last statement in the decompiled source, and is omitted here
            // because that member is not ported. See the file header.
            //
            // Note for whoever restores it: it is called directly rather than through
            // patchAppliers, so the entry's `applied` flag stays false even though the patches are
            // now applied. A subsequent ApplyPatches will therefore apply the whole set a second
            // time, stacking duplicate prefixes. That is the shipped behaviour; restore the direct
            // call, not an ApplyPatches call.
        }

        /// <summary>
        /// Returns the Harmony instance for <paramref name="harmonyId"/>, creating it on first use.
        /// </summary>
        /// <param name="harmonyId">
        /// The id to group the patch under. Null, empty or whitespace means the tool's default id.
        /// </param>
        /// <remarks>
        /// Instances are shared per id rather than created per patch because <c>UnpatchAll</c> works
        /// by id: a patch applied through a throwaway instance would still be removable, but only by
        /// an instance that remembered the same id, so keeping one instance per id is what makes
        /// <see cref="RemoveAllPatches"/> complete.
        /// </remarks>
        private static Harmony GetHarmony(string harmonyId)
        {
            if (string.IsNullOrWhiteSpace(harmonyId))
            {
                return defaultHarmony;
            }

            if (harmonyInstances == null)
            {
                harmonyInstances = new Dictionary<string, Harmony>();
            }

            if (!harmonyInstances.TryGetValue(harmonyId, out Harmony harmony))
            {
                harmony = new Harmony(harmonyId);
                harmonyInstances.Add(harmonyId, harmony);
            }

            return harmony;
        }
    }
}

// ── What actually gets patched ───────────────────────────────────────────────────────────────
// Recorded here because the call sites are spread through the unported god class and are the only
// place the targets are named. Decompiled line numbers are for the current snapshot; the target
// types are resolved there from assembly-qualified name strings, listed at lines 15282-15287,
// 15694-15695, 15892-15896, 16166, 17121 and 17981.
//
// Animation window -- UnityEditor / UnityEditorInternal (applied at lines 15301-15318):
//   AnimationWindow.ShouldUpdateGameObjectSelection, .ShouldUpdateSelection   prefix
//   AnimationWindowControl.get_canPlay / get_canPreview / get_canRecord       prefix
//   AnimationWindowSelectionItem.GetEditorCurveValueType                      prefix + postfix
//   AnimationWindowSelectionItem.get_rootGameObject                           prefix
//   AddCurvesPopupHierarchyDataSource.FetchData                               prefix + postfix
//   AnimationWindowHierarchyGUI.DoNodeGUI                                     prefix + postfix
//   AnimationWindowHierarchyGUI.GenerateMenu, .DoAddCurveButton               postfix
//   AnimEditor.TabSelectionOnGUI                                              prefix, deferred
//     behind a postfix on AnimationWindow.OnGUI -- see .DeferredPatches.cs.
//   The get_can* prefixes and the two selection prefixes are the ones that return a substituted
//   value or veto the call outright; the rest add drawing and menu entries.
//
// Animator graph -- UnityEditor.Graphs (applied at lines 15701-15709, 15906-15910, 17136-17151):
//   AnimationStateMachine.EdgeGUI.get_selectedEdgeColor, .get_defaultTransitionColor,
//     .get_selectorTransitionColor                                            prefix
//   GraphGUI.get_gridMajorColor, .get_gridMinorColor                          prefix
//     -- six property getters replaced outright, which is how the tool recolours the graph.
//   Edge..ctor(Slot, Slot)                                                    postfix
//   AnimationStateMachine.EdgeGUI.DrawArrows, .EndSlotDragging, .EndDragging
//   Graph.AddNode (the bool overload)                                         postfix
//   AnimationStateMachine.Graph.SetStateMachines, .CreateNodeFromState,
//     .CreateNodeFromStateMachine, .CreateNodes
//   AnimatorControllerTool.DoGraphBottomBar, .AddNewLayer, .AddParameterMenu
//     (the last via ParameterControllerView, line 17987)
//   AnimationStateMachine.GraphGUI.OnGraphGUI, .HandleContextMenu, .NodeGUI,
//     .AddStateEmptyCallback
//   AnimationStateMachine.GraphGUI.HandleObjectDragging                       transpiler
//   AnimationBlendTree.GraphGUI.HandleNodeInput, .NodeGUI
//   StateNode / StateMachineNode / EntryNode / ExitNode / AnyStateNode .NodeUI and .Connect,
//     reached through AnimatorGraphReflection.TypeResolvers rather than by name.
//
// Layer list -- UnityEditor.Graphs.LayerControllerView (applied at lines 16180-16189):
//   .OnToolbarGUI, .OnGUI, .Init, .OnDrawLayer, .OnRemoveLayer, .RenameEnd, .OnSelectLayer
//   .ResetUI                                                                  transpiler
//   UnityEditorInternal.ReorderableList.DoLayoutList                          prefix
//     -- the only patch in the whole set on a public type.
//
// Miscellaneous:
//   UnityEditor.Unsupported.PasteToStateMachineFromPasteboard  prefix + postfix, line 17049
//   UnityEditor.GenericMenu.ShowAsContext                      postfix,          line 17108
//
// The three transpilers are the most version-fragile of the lot: a prefix that fails to resolve
// its target is caught and logged, but a transpiler that resolves and then fails to find the IL
// pattern it rewrites produces a method that compiles and misbehaves.
