// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   MethodVisitor            -> ControllerSubAssets, lines 36-107
//     _SchemaVisitor         -> blendTrees,     line 38
//     broadcasterVisitor     -> behaviours,     line 40
//     _ProxyVisitor          -> stateMachines,  line 42
//     structVisitor          -> transitions,    line 44
//     m_ServiceVisitor       -> states,         line 46
//     stateVisitor           -> others,         line 48
//     DisableIndexer / VerifyIndexer() -> dropped, lines 50 and 103 (obfuscator sentinel: a
//                               never-written static object plus a "== null" predicate that
//                               nothing calls; see RE_NOTES "Self-referential dead members")
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// LogoutMapper() belongs to the ControllerEditor outer class body, which is not ported yet, so it
// keeps its decompiled name here. It resolves to the controller currently being edited.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// Every sub-asset stored inside one animator controller asset, split by kind in a single
        /// pass.
        /// </summary>
        /// <remarks>
        /// A controller keeps its state machines, states, transitions, blend trees and behaviours
        /// as sub-assets of one <c>.controller</c> file. Anything that has to walk or clean up the
        /// whole file needs them grouped by type, and <see cref="AssetDatabase.LoadAllAssetsAtPath"/>
        /// returns them interleaved, so this sorts them once rather than re-filtering the array per
        /// use. <see cref="others"/> collects sub-assets of no recognised kind, minus the controller
        /// asset itself.
        /// </remarks>
        private struct ControllerSubAssets
        {
            internal readonly List<BlendTree> blendTrees;

            internal readonly List<StateMachineBehaviour> behaviours;

            internal readonly List<AnimatorStateMachine> stateMachines;

            internal readonly List<AnimatorTransitionBase> transitions;

            internal readonly List<AnimatorState> states;

            /// <summary>Sub-assets of no recognised kind. The main controller asset is excluded.</summary>
            internal readonly List<Object> others;

            internal ControllerSubAssets(AnimatorController controller)
            {
                blendTrees = new List<BlendTree>();
                behaviours = new List<StateMachineBehaviour>();
                stateMachines = new List<AnimatorStateMachine>();
                transitions = new List<AnimatorTransitionBase>();
                states = new List<AnimatorState>();
                others = new List<Object>();

                if (!controller)
                {
                    return;
                }

                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller));
                for (int i = 0; i < assets.Length; i++)
                {
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
                    else if (assets[i] != LogoutMapper())
                    {
                        others.Add(assets[i]);
                    }
                }
            }
        }
    }
}
