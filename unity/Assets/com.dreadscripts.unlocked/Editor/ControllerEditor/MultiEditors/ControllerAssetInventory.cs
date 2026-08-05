// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   MethodVisitor                   -> ControllerAssetInventory,   line 36
//     _SchemaVisitor                -> blendTrees,                 line 38
//     broadcasterVisitor            -> behaviours,                 line 40
//     _ProxyVisitor                 -> stateMachines,              line 42
//     structVisitor                 -> transitions,                line 44
//     m_ServiceVisitor              -> states,                     line 46
//     stateVisitor                  -> others,                     line 48
//     .ctor(AnimatorController)     -> .ctor,                      line 52
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// "MethodVisitor" is an obfuscated name and the type has nothing to do with methods or with the
// visitor pattern - it is a one-pass inventory of an animator controller's sub-assets, bucketed by
// kind, so it is renamed here. Likewise every field carried a "...Visitor" suffix that only
// described the obfuscation pass, not the contents; the buckets are named after what they hold.
// The type was a private nested type of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// Not ported: the static field `DisableIndexer` (line 50) and `VerifyIndexer()` (line 103), which
// returns whether that field is null. The field is never assigned anywhere in the assembly and the
// method has no callers - obfuscator scaffolding, omitted.
//
// DELIBERATE DEVIATION
// --------------------
// The decompiled constructor excludes the controller asset itself from `others` by comparing each
// loaded sub-asset against the window's current controller (`LogoutMapper()`, line 8509), not
// against the `instance` it was handed. This port compares against `instance`.
//   * The two are the same object at the only call site: `_Service = new MethodVisitor(
//     LogoutMapper())` (line 18360) is the sole construction in the assembly.
//   * The static that backs `LogoutMapper()` lives in the not-yet-ported ControllerEditor window
//     class, so referencing it here is not possible without stubbing it.
// If the window's controller ever diverged from the argument, the original would keep the argument
// controller in `others` and drop the window's controller instead; that situation cannot arise
// today.
//
// Audit status: VERIFIED -- the struct, all six list fields and the constructor were diffed
// statement by statement against `private struct MethodVisitor` at line 36 of
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs; every cited line (36,
// 38, 40, 42, 44, 46, 48, 50, 52, 103) still lands on the named member. Field types, the six list
// allocations, the `if (!instance) return;` guard, the LoadAllAssetsAtPath call and the type-test
// order (AnimatorTransitionBase, AnimatorState, BlendTree, StateMachineBehaviour,
// AnimatorStateMachine, else) all match; the port only flattens the decompiler's inverted, nested
// if/else chain into a positive else-if ladder, which preserves the order exactly. The two notes
// were re-checked and both hold: VerifyIndexer is `return DisableIndexer == null`, DisableIndexer
// has no assignment anywhere in the assembly, and the deviation is real -- the shipped constructor
// compares against `ActiveController()` (line 8509, the member the note calls LogoutMapper) rather
// than its own `instance` argument, and the sole construction in the assembly is still
// `new MethodVisitor(ActiveController())` at line 18360, so the two are the same object there.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Every sub-asset of an animator controller file, sorted by kind.
    /// </summary>
    /// <remarks>
    /// An animator controller is a single asset file with its states, transitions, blend trees,
    /// state machines and state behaviours stored inside it as sub-assets. Walking that file is not
    /// free, and the window needs the same buckets repeatedly - to find write-defaults offenders,
    /// to rename orphans, to re-icon everything - so the whole file is read once and bucketed here
    /// rather than re-queried per feature.
    /// </remarks>
    internal struct ControllerAssetInventory
    {
        internal readonly List<BlendTree> blendTrees;

        internal readonly List<StateMachineBehaviour> behaviours;

        internal readonly List<AnimatorStateMachine> stateMachines;

        internal readonly List<AnimatorTransitionBase> transitions;

        internal readonly List<AnimatorState> states;

        /// <summary>
        /// Sub-assets that are none of the above, excluding the controller asset itself.
        /// </summary>
        /// <remarks>
        /// In a well-formed controller this is empty. Anything that lands here is a stray object
        /// somebody embedded in the file - a leftover clip, an avatar mask - which is exactly what
        /// the window wants to surface.
        /// </remarks>
        internal readonly List<UnityEngine.Object> others;

        /// <summary>
        /// Reads <paramref name="controller"/>'s asset file and sorts its contents into the buckets.
        /// </summary>
        /// <remarks>
        /// A null or destroyed controller yields an inventory of empty lists rather than a null one,
        /// so callers can enumerate the buckets without a guard.
        /// </remarks>
        internal ControllerAssetInventory(AnimatorController controller)
        {
            blendTrees = new List<BlendTree>();
            behaviours = new List<StateMachineBehaviour>();
            stateMachines = new List<AnimatorStateMachine>();
            transitions = new List<AnimatorTransitionBase>();
            states = new List<AnimatorState>();
            others = new List<UnityEngine.Object>();

            if (!controller)
            {
                return;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller));
            for (int i = 0; i < assets.Length; i++)
            {
                // Order matters: BlendTree is a Motion and AnimatorTransitionBase covers all four
                // transition classes, so the tests run most-derived first.
                if (assets[i] is AnimatorTransitionBase transition)
                {
                    transitions.Add(transition);
                }
                else if (assets[i] is AnimatorState state)
                {
                    states.Add(state);
                }
                else if (assets[i] is BlendTree blendTree)
                {
                    blendTrees.Add(blendTree);
                }
                else if (assets[i] is StateMachineBehaviour behaviour)
                {
                    behaviours.Add(behaviour);
                }
                else if (assets[i] is AnimatorStateMachine stateMachine)
                {
                    stateMachines.Add(stateMachine);
                }
                else if (assets[i] != controller)
                {
                    others.Add(assets[i]);
                }
            }
        }
    }
}
