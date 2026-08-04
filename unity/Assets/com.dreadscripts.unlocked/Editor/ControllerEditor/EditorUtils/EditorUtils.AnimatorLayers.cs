// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static VisitPredicate       -> AddLayer,            line 3275
//   static DefinePredicate      -> IsTagTransition,     line 3293
//   static StartPredicate       -> AddTag,              line 3302
//   static ReadPredicate        -> RemoveTag,           line 3313
//   static SelectPredicate      -> HasTag,              line 3319
//   static RemovePredicate      -> GetTags,             line 3335
//   static InstantiatePredicate -> GetSystemTags,       line 3341
//   static AwakePredicate       -> GetUserTags,         line 3348
//   static ResetPredicate       -> CopyLayers,          line 3355
//   static FlushPredicate       -> CopyLayersAndParameters, line 3374
//   static ConnectPredicate     -> CopyParameters,      line 3402
//   static CalculatePredicate   -> CopyLayer,           line 3417
//   static TestPredicate        -> CopyLayerSettings,   line 3446
//   struct <>c__DisplayClass128_0                       -> the CopyLayer locals, line 1904
//   static InstantiateError<T>  -> CopyLayer's CopyObject,           line 8340
//   static AwakeError<T>        -> CopyLayer's CopyNestedBlendTrees, line 8363
//   static ResetError<T>        -> CopyLayer's CopyObjects,          line 8381
//   static FlushError<T>        -> CopyLayer's CopyTransitions,      line 8392
//   static ConnectError         -> CopyLayer's CopyStateMachineTree, line 8405
//   static CalculateError       -> CopyLayer's CopyStates,           line 8427
//   static TestError            -> CopyLayer's RelinkTransitions,    line 8453
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// LAYER TAGS. The tool stores per-layer metadata inside the animator asset itself, because
// AnimatorControllerLayer has nowhere to put custom data. A tag is an any-state transition that is
// muted, marked isExit and given no destination -- so Unity never takes it and it costs nothing at
// runtime -- whose *name* carries the payload. Tags whose name starts with '_' are written by the
// tool (the only one shipped is "_category:<path>", read back by ControllerEditor's layer list);
// tags without the prefix are whatever the user typed. GetSystemTags/GetUserTags are the two
// halves of that split, and the '_' convention is the only thing distinguishing them.
//
// COPYING A LAYER. CopyLayer is a deep copy: the state machine tree, its states, their behaviours,
// their sub-asset blend trees and every transition between them are all cloned into the target
// controller as sub-assets, then relinked so no clone points back at an original. The seven
// [CompilerGenerated] statics listed above are the local functions that do it -- Roslyn lifts a
// local function's captured variables into a by-ref struct, which is exactly the
// `ref <>c__DisplayClass128_0` parameter they all carry -- so they are written back as local
// functions here rather than as members of the class.
//
// The two dictionaries are inverses of each other and both are needed: copies maps original ->
// clone so a second reference to the same object reuses the clone rather than duplicating it, and
// originals maps clone -> original so the relink pass can ask a clone what it was made from
// (its own fields having already been overwritten).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Adds a new layer with its own state machine to <paramref name="controller"/>, storing
        /// the state machine as a hidden sub-asset of the controller the way Unity's own
        /// "Add Layer" button does.
        /// </summary>
        internal static AnimatorControllerLayer AddLayer(this AnimatorController controller, string name,
            float defaultWeight, AvatarMask mask = null)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = name,
                defaultWeight = defaultWeight,
                avatarMask = mask,
                stateMachine = new AnimatorStateMachine
                {
                    name = name,
                    hideFlags = HideFlags.HideInHierarchy
                }
            };

            AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);
            controller.AddLayer(layer);
            return layer;
        }

        /// <summary>
        /// Whether <paramref name="transition"/> is a tag rather than a real transition -- muted,
        /// exiting and with no destination of either kind.
        /// </summary>
        private static bool IsTagTransition(AnimatorStateTransition transition)
        {
            return transition.isExit
                && transition.mute
                && transition.destinationState == null
                && transition.destinationStateMachine == null;
        }

        /// <summary>Adds <paramref name="tag"/> to the layer, unless it already carries it.</summary>
        internal static void AddTag(this AnimatorControllerLayer layer, string tag)
        {
            if (layer.HasTag(tag))
            {
                return;
            }

            AnimatorStateTransition transition = layer.stateMachine.AddAnyStateTransition((AnimatorState)null);
            transition.isExit = true;
            transition.mute = true;
            transition.name = tag;
        }

        /// <summary>
        /// Removes <paramref name="tag"/> from the layer. Throws if the layer does not carry it --
        /// the vendor's <c>First</c> is kept, since every call site tests with
        /// <see cref="HasTag"/> first.
        /// </summary>
        internal static void RemoveTag(this AnimatorControllerLayer layer, string tag)
        {
            AnimatorStateTransition transition = layer.stateMachine.anyStateTransitions
                .First(t => IsTagTransition(t) && t.name == tag);
            layer.stateMachine.RemoveAnyStateTransition(transition);
        }

        /// <summary>
        /// Whether the layer carries <paramref name="tag"/>. With
        /// <paramref name="exactMatch"/> false the tag only has to be a substring, which is how
        /// a prefixed family such as "_category:" is tested for as a whole.
        /// </summary>
        internal static bool HasTag(this AnimatorControllerLayer layer, string tag, bool exactMatch = true)
        {
            return layer.stateMachine.anyStateTransitions.Any(t =>
            {
                if (!IsTagTransition(t))
                {
                    return false;
                }

                return exactMatch ? t.name == tag : t.name.Contains(tag);
            });
        }

        /// <summary>Every tag on the layer, system and user alike.</summary>
        internal static IEnumerable<string> GetTags(this AnimatorControllerLayer layer)
        {
            return layer.stateMachine.anyStateTransitions.Where(IsTagTransition).Select(t => t.name);
        }

        /// <summary>The tool-written tags -- those whose name starts with '_'.</summary>
        internal static IEnumerable<string> GetSystemTags(this AnimatorControllerLayer layer)
        {
            return layer.GetTags().Where(s => s[0] == '_');
        }

        /// <summary>The user-written tags -- those whose name does not start with '_'.</summary>
        internal static IEnumerable<string> GetUserTags(this AnimatorControllerLayer layer)
        {
            return layer.GetTags().Where(s => s[0] != '_');
        }

        /// <summary>
        /// Deep-copies every layer of <paramref name="source"/> into <paramref name="target"/>,
        /// showing a cancel-less progress bar. The copies are reported in source order.
        /// </summary>
        internal static void CopyLayers(AnimatorController source, AnimatorController target,
            out AnimatorControllerLayer[] copiedLayers)
        {
            try
            {
                AnimatorControllerLayer[] layers = source.layers;
                int count = layers.Length;
                copiedLayers = new AnimatorControllerLayer[count];
                for (int i = 0; i < count; i++)
                {
                    EditorUtility.DisplayProgressBar("Copying Layers", $"Copying layer {i + 1} of {count}",
                        (float)i / count);
                    copiedLayers[i] = CopyLayer(layers[i], target);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// <see cref="CopyLayers"/> plus the controller's parameters.
        /// </summary>
        /// <param name="copiedParameters">
        /// One entry per source parameter, in source order. An entry is null when the parameter
        /// already existed on <paramref name="target"/> and <paramref name="includeExisting"/> is
        /// false, so a caller can tell what it actually added from what it merely matched.
        /// </param>
        internal static void CopyLayersAndParameters(AnimatorController source, AnimatorController target,
            out AnimatorControllerLayer[] copiedLayers, out AnimatorControllerParameter[] copiedParameters,
            bool includeExisting = false)
        {
            try
            {
                AnimatorControllerLayer[] layers = source.layers;
                AnimatorControllerParameter[] parameters = source.parameters;
                int count = layers.Length;
                copiedLayers = new AnimatorControllerLayer[count];
                copiedParameters = new AnimatorControllerParameter[parameters.Length];

                for (int i = 0; i < count; i++)
                {
                    EditorUtility.DisplayProgressBar("Copying Layers", $"Copying layer {i + 1} of {count}",
                        (float)i / count);
                    copiedLayers[i] = CopyLayer(layers[i], target);
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Copying Parameters",
                        $"Copying parameter {i + 1} of {parameters.Length}", (float)i / parameters.Length);
                    AnimatorControllerParameter parameter = target.CopyParameter(parameters[i], out bool added);
                    copiedParameters[i] = (added || includeExisting) ? parameter : null;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Copies every parameter of <paramref name="source"/> onto <paramref name="target"/>.
        /// Entries are null under the same rule as <see cref="CopyLayersAndParameters"/>.
        /// </summary>
        /// <remarks>
        /// Unlike the two above this one clears the progress bar on the success path only -- an
        /// exception thrown mid-copy leaves it on screen. Transcribed as the vendor wrote it.
        /// </remarks>
        internal static AnimatorControllerParameter[] CopyParameters(AnimatorController source,
            AnimatorController target, bool includeExisting = false)
        {
            AnimatorControllerParameter[] parameters = source.parameters;
            AnimatorControllerParameter[] copies = new AnimatorControllerParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Copying Parameters",
                    $"Copying parameter {i + 1} of {parameters.Length}", (float)i / parameters.Length);
                AnimatorControllerParameter parameter = target.CopyParameter(parameters[i], out bool added);
                copies[i] = (added || includeExisting) ? parameter : null;
            }

            EditorUtility.ClearProgressBar();
            return copies;
        }

        /// <summary>
        /// Deep-copies <paramref name="sourceLayer"/> into <paramref name="target"/> and returns
        /// the new layer, which is already attached to the controller.
        /// </summary>
        /// <param name="index">
        /// Where to insert the layer. Out-of-range values (including the -1 default) append.
        /// </param>
        /// <param name="recordUndo">
        /// Register every created sub-asset with <see cref="Undo"/> so the whole copy can be undone
        /// as one step.
        /// </param>
        internal static AnimatorControllerLayer CopyLayer(AnimatorControllerLayer sourceLayer,
            AnimatorController target, int index = -1, bool recordUndo = false)
        {
            AnimatorControllerLayer newLayer = new AnimatorControllerLayer
            {
                name = target.MakeUniqueLayerName(sourceLayer.name)
            };
            CopyLayerSettings(sourceLayer, newLayer);

            // original -> clone, so a second reference to the same object reuses the clone.
            Dictionary<UnityEngine.Object, UnityEngine.Object> copies =
                new Dictionary<UnityEngine.Object, UnityEngine.Object>();

            // clone -> original, so the relink pass can recover what a clone was made from.
            Dictionary<UnityEngine.Object, UnityEngine.Object> originals =
                new Dictionary<UnityEngine.Object, UnityEngine.Object>();

            T CopyObject<T>(T original) where T : UnityEngine.Object
            {
                if (!original)
                {
                    return null;
                }

                if (copies.TryGetValue(original, out UnityEngine.Object existing))
                {
                    return (T)existing;
                }

                T copy = CloneSerialized(original);
                AssetDatabase.AddObjectToAsset(copy, target);
                if (recordUndo)
                {
                    Undo.RegisterCreatedObjectUndo(copy, "Copy Layer");
                }

                copy.hideFlags = original.hideFlags;
                copies.Add(original, copy);
                originals.Add(copy, original);
                return copy;
            }

            T[] CopyObjects<T>(T[] originalArray) where T : UnityEngine.Object
            {
                T[] copyArray = new T[originalArray.Length];
                for (int i = 0; i < originalArray.Length; i++)
                {
                    copyArray[i] = CopyObject(originalArray[i]);
                }

                return copyArray;
            }

            // A blend tree stored as a sub-asset belongs to the controller and has to be cloned
            // with it; one saved as its own asset file is shared deliberately and is left alone.
            T CopyNestedBlendTrees<T>(T motion) where T : Motion
            {
                if (motion is BlendTree tree
                    && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tree)) != tree)
                {
                    BlendTree treeCopy = CopyObject(tree);
                    ChildMotion[] children = treeCopy.children;
                    for (int i = 0; i < treeCopy.children.Length; i++)
                    {
                        children[i].motion = CopyNestedBlendTrees(children[i].motion);
                    }

                    treeCopy.children = children;
                    EditorUtility.SetDirty(treeCopy);
                    motion = treeCopy as T;
                }

                return motion;
            }

            T[] CopyTransitions<T>(T[] transitions) where T : AnimatorTransitionBase
            {
                T[] copyArray = CopyObjects(transitions);
                foreach (T transition in copyArray)
                {
                    transition.destinationState = CopyObject(transition.destinationState);
                    transition.destinationStateMachine = CopyObject(transition.destinationStateMachine);
                }

                return copyArray;
            }

            AnimatorStateMachine CopyStateMachineTree(AnimatorStateMachine original)
            {
                AnimatorStateMachine copy = CopyObject(original);
                if (!copy)
                {
                    return null;
                }

                // The root state machine is normally named after the layer, so it follows the
                // layer's uniqued name; a differently named one keeps its own name.
                if (copy.name == sourceLayer.name)
                {
                    copy.name = newLayer.name;
                }

                copy.behaviours = CopyObjects(original.behaviours);
                ChildAnimatorStateMachine[] children = original.stateMachines;
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].stateMachine = CopyStateMachineTree(original.stateMachines[i].stateMachine);
                }

                copy.stateMachines = children;
                return copy;
            }

            void CopyStates(AnimatorStateMachine copy)
            {
                AnimatorStateMachine original = (AnimatorStateMachine)originals[copy];
                ChildAnimatorState[] states = original.states;
                for (int i = 0; i < states.Length; i++)
                {
                    AnimatorState stateCopy = CopyObject(original.states[i].state);
                    if (!stateCopy)
                    {
                        continue;
                    }

                    states[i].state = stateCopy;
                    stateCopy.behaviours = CopyObjects(stateCopy.behaviours);
                    if (stateCopy.motion is BlendTree tree
                        && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tree)) != tree)
                    {
                        stateCopy.motion = CopyNestedBlendTrees(tree);
                    }
                }

                copy.states = states;
                foreach (ChildAnimatorStateMachine child in copy.stateMachines)
                {
                    CopyStates(child.stateMachine);
                }
            }

            void RelinkTransitions(AnimatorStateMachine copy)
            {
                AnimatorStateMachine original = (AnimatorStateMachine)originals[copy];
                copy.entryTransitions = CopyTransitions(original.entryTransitions);
                copy.anyStateTransitions = CopyTransitions(original.anyStateTransitions);

                foreach (ChildAnimatorState child in copy.states)
                {
                    child.state.transitions =
                        CopyTransitions(((AnimatorState)originals[child.state]).transitions);
                }

                foreach (ChildAnimatorStateMachine child in copy.stateMachines)
                {
                    copy.SetStateMachineTransitions(child.stateMachine,
                        CopyTransitions(copy.GetStateMachineTransitions(child.stateMachine)));
                }

                foreach (ChildAnimatorStateMachine child in copy.stateMachines)
                {
                    RelinkTransitions(child.stateMachine);
                }

                copy.defaultState = CopyObject(original.defaultState);
            }

            newLayer.stateMachine = CopyStateMachineTree(sourceLayer.stateMachine);
            CopyStates(newLayer.stateMachine);
            RelinkTransitions(newLayer.stateMachine);

            if (index >= 0 && index <= target.layers.Length - 1)
            {
                AnimatorControllerLayer[] layers = target.layers;
                ArrayUtility.Insert(ref layers, index, newLayer);
                target.layers = layers;
            }
            else
            {
                target.AddLayer(newLayer);
            }

            return newLayer;
        }

        /// <summary>
        /// Copies everything about a layer except its name and state machine: weight, mask,
        /// blending mode, IK pass and layer sync.
        /// </summary>
        /// <remarks>
        /// The base layer of a controller always runs at full weight regardless of what its
        /// serialised defaultWeight says, so a copy of it would otherwise come out at 0. Hence the
        /// asset lookup: if the source layer is layer 0 of the controller that owns its state
        /// machine, the copy gets weight 1 instead of the stored value.
        /// </remarks>
        internal static void CopyLayerSettings(AnimatorControllerLayer source, AnimatorControllerLayer destination)
        {
            AnimatorController owner =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(source.stateMachine));

            float defaultWeight = source.defaultWeight;
            if (owner && owner.layers.Length != 0 && owner.layers[0].stateMachine == source.stateMachine)
            {
                defaultWeight = 1f;
            }

            destination.defaultWeight = defaultWeight;
            destination.avatarMask = source.avatarMask;
            destination.blendingMode = source.blendingMode;
            destination.iKPass = source.iKPass;
            destination.syncedLayerAffectsTiming = source.syncedLayerAffectsTiming;
            destination.syncedLayerIndex = source.syncedLayerIndex;
        }
    }
}
