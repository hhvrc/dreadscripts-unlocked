// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CalculatePredicate -> CopyLayer,                 line 3417
//   static TestPredicate      -> CopyLayerSettings,         line 3446
//   static ResetPredicate     -> CopyLayers,                line 3355
//   static FlushPredicate     -> CopyLayersAndParameters,   line 3374
//   static InstantiateError   -> CopyLayer/Clone            (local function), line 8340
//   static AwakeError         -> CopyLayer/CopyNestedTrees  (local function), line 8363
//   static ResetError         -> CopyLayer/CloneAll         (local function), line 8381
//   static FlushError         -> CopyLayer/CloneTransitions (local function), line 8392
//   static ConnectError       -> CopyLayer/CopyMachineTree  (local function), line 8405
//   static CalculateError     -> CopyLayer/RemapStates      (local function), line 8427
//   static TestError          -> CopyLayer/RemapTransitions (local function), line 8453
//   static CompareRules       -> CloneSerialized, line 4193, in EditorUtils.Assets.cs
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// ─── THE CAPTURE STRUCT IS AN ARTIFACT ──────────────────────────────────────────────────────────
// The decompilation gives the seven copy helpers as sibling `internal static` methods four thousand
// lines away from their only caller, each taking `ref _003C_003Ec__DisplayClass128_0` (line 1904)
// and each marked [CompilerGenerated]. That is the shape the C# compiler emits for *local
// functions* that capture variables: a by-ref struct closure, not a class, because no delegate is
// ever formed from them. The struct's six fields are exactly the six locals of CalculatePredicate.
// They are restored here as local functions of CopyLayer, which is what the original source was.
// The struct itself is not ported -- reproducing it would be reproducing the compiler's output.
//
// CompareRules (line 4193) is genuinely a class-level helper of the decompiled type, not a local
// function, and it has three other callers in the "Rules" asset family. While that family was
// unported this file carried it as an eighth local function, so as not to claim a member a later
// wave would declare a second time. EditorUtils.Assets.cs has since landed the real class-level
// CloneSerialized, so on 2026-08-05 the local copy was deleted and Clone now calls that one; the
// two bodies were compared line by line first and were identical. Keep them that way if either is
// touched: the deep copy's correctness depends on EditorUtility.CopySerialized preserving every
// field, including the object references that the remapping passes then rewrite.
//
// Not ported here: ConnectPredicate (line 3402), the parameters-only counterpart of
// CopyLayersAndParameters. It is a plain loop over GetOrAddParameter with a progress bar, is not in
// this pass's assigned set, and belongs with the parameter helpers rather than with the layer copy.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Deep-copies <paramref name="source"/> into <paramref name="destination"/>, duplicating
        /// every state machine, state, behaviour, transition and sub-asset blend tree it reaches and
        /// rewiring the copies to point at each other rather than at the originals.
        /// </summary>
        /// <param name="index">
        /// Where to place the new layer. A value outside the destination's existing layer range --
        /// including the default -1 -- appends instead.
        /// </param>
        /// <param name="registerUndo">
        /// Registers each created sub-asset with <see cref="Undo"/> under "Copy Layer". Note that
        /// this covers only the object creation: the layer's own addition to
        /// <see cref="AnimatorController.layers"/> is never recorded, so an undo of a copy leaves the
        /// destination holding a layer whose contents have been destroyed. Callers that want a
        /// cleanly undoable copy must record the controller themselves before calling.
        /// </param>
        /// <remarks>
        /// <para>
        /// ASSET MUTATION. The destination controller is modified in place and gains one sub-asset
        /// per copied object; nothing is overwritten, because every copy is newly created and layer
        /// names are made unique first. No <see cref="EditorUtility.SetDirty"/> or
        /// <see cref="AssetDatabase.SaveAssets"/> call is made on the destination controller here --
        /// <see cref="AnimatorController.AddLayer(AnimatorControllerLayer)"/> and the
        /// <c>layers</c> setter do their own serialisation. Blend trees reached through a state's
        /// motion are the one place a copy writes back with an explicit SetDirty.
        /// </para>
        /// <para>
        /// Nothing outside the copied subgraph is touched, so no existing transition, condition or
        /// parameter driver is left stale by this call. Parameters are a different matter: the copy
        /// carries over the *names* its conditions and drivers reference but does not create those
        /// parameters on the destination, so a layer copied into a controller that lacks them
        /// arrives with conditions bound to parameters that do not exist. That is what
        /// <see cref="CopyLayersAndParameters"/> exists to avoid, and why the single-layer entry
        /// point is normally paired with a parameter copy at the call site.
        /// </para>
        /// <para>
        /// The work is three passes over the graph rather than one, and the order matters. The first
        /// pass copies the state machine tree, which is enough to give every machine an identity in
        /// the two lookup tables. The second copies states and their behaviours, which transitions
        /// need to exist before they can be pointed at. Only then can the third pass copy
        /// transitions, since a transition's destination may be any state or machine anywhere in the
        /// layer, including one the walk has not reached yet. Merging the passes would leave forward
        /// references dangling.
        /// </para>
        /// </remarks>
        internal static AnimatorControllerLayer CopyLayer(AnimatorControllerLayer source,
            AnimatorController destination, int index = -1, bool registerUndo = false)
        {
            AnimatorControllerLayer copy = new AnimatorControllerLayer
            {
                name = destination.MakeUniqueLayerName(source.name)
            };

            CopyLayerSettings(source, copy);

            // The two tables are inverses of each other and both are needed. `clones` makes the copy
            // idempotent -- a state reached twice, once through the tree walk and once as a
            // transition destination, must yield the same copy both times, or the graph would come
            // apart. `sources` is the lookup the remapping passes run in reverse: they are handed a
            // copy and need the original it came from in order to read the wiring still to be
            // translated.
            Dictionary<Object, Object> clones = new Dictionary<Object, Object>();
            Dictionary<Object, Object> sources = new Dictionary<Object, Object>();

            T Clone<T>(T original) where T : Object
            {
                if (!original)
                {
                    return null;
                }

                if (clones.TryGetValue(original, out Object existing))
                {
                    return (T)existing;
                }

                T clone = CloneSerialized(original);
                AssetDatabase.AddObjectToAsset(clone, destination);

                if (registerUndo)
                {
                    Undo.RegisterCreatedObjectUndo(clone, "Copy Layer");
                }

                // Carried over rather than forced: a state machine's children are hidden in the
                // project view by the animator editor, and a copy that showed them would litter the
                // controller asset.
                clone.hideFlags = original.hideFlags;

                clones.Add(original, clone);
                sources.Add(clone, original);
                return clone;
            }

            T[] CloneAll<T>(T[] originals) where T : Object
            {
                T[] result = new T[originals.Length];
                for (int i = 0; i < originals.Length; i++)
                {
                    result[i] = Clone(originals[i]);
                }

                return result;
            }

            T[] CloneTransitions<T>(T[] originals) where T : AnimatorTransitionBase
            {
                T[] result = CloneAll(originals);
                foreach (T transition in result)
                {
                    // The copy still points at the original's destination, because CopySerialized
                    // copied the reference verbatim. Clone() returns the already-made copy for
                    // anything the earlier passes reached, and makes one on the spot otherwise.
                    transition.destinationState = Clone(transition.destinationState);
                    transition.destinationStateMachine = Clone(transition.destinationStateMachine);
                }

                return result;
            }

            T CopyNestedTrees<T>(T motion) where T : Motion
            {
                // Only blend trees stored *inside* a controller are copied. One saved as its own
                // asset is a shared, independently editable file, and duplicating it on every layer
                // copy would be wrong; the test is whether the tree is the main asset at its own
                // path. Animation clips are never copied at all -- they are always separate assets.
                if (motion is BlendTree tree &&
                    AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tree)) != tree)
                {
                    BlendTree treeCopy = Clone(tree);

                    ChildMotion[] children = treeCopy.children;
                    for (int i = 0; i < treeCopy.children.Length; i++)
                    {
                        children[i].motion = CopyNestedTrees(children[i].motion);
                    }

                    treeCopy.children = children;
                    EditorUtility.SetDirty(treeCopy);
                    motion = treeCopy as T;
                }

                return motion;
            }

            AnimatorStateMachine CopyMachineTree(AnimatorStateMachine original)
            {
                AnimatorStateMachine clone = Clone(original);
                if (!clone)
                {
                    return null;
                }

                // A layer's root machine is conventionally named after the layer, so a copy named
                // "Gesture" inside a layer now called "Gesture 1" would read as a stale leftover.
                // Only that one machine is renamed; nested machines keep their names, and a nested
                // machine that happens to share the layer's name is renamed too, which is a
                // cosmetic quirk of testing by name rather than by identity.
                if (clone.name == source.name)
                {
                    clone.name = copy.name;
                }

                clone.behaviours = CloneAll(original.behaviours);

                ChildAnimatorStateMachine[] children = original.stateMachines;
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].stateMachine = CopyMachineTree(original.stateMachines[i].stateMachine);
                }

                clone.stateMachines = children;
                return clone;
            }

            void RemapStates(AnimatorStateMachine clone)
            {
                AnimatorStateMachine original = (AnimatorStateMachine)sources[clone];

                ChildAnimatorState[] states = original.states;
                for (int i = 0; i < states.Length; i++)
                {
                    AnimatorState stateCopy = Clone(original.states[i].state);
                    if (!stateCopy)
                    {
                        continue;
                    }

                    // The position in the graph comes along with the ChildAnimatorState entry; only
                    // the state reference itself is swapped.
                    states[i].state = stateCopy;
                    stateCopy.behaviours = CloneAll(stateCopy.behaviours);

                    if (stateCopy.motion is BlendTree tree &&
                        AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(tree)) != tree)
                    {
                        stateCopy.motion = CopyNestedTrees(tree);
                    }
                }

                clone.states = states;

                foreach (ChildAnimatorStateMachine child in clone.stateMachines)
                {
                    RemapStates(child.stateMachine);
                }
            }

            void RemapTransitions(AnimatorStateMachine clone)
            {
                AnimatorStateMachine original = (AnimatorStateMachine)sources[clone];

                clone.entryTransitions = CloneTransitions(original.entryTransitions);
                clone.anyStateTransitions = CloneTransitions(original.anyStateTransitions);

                foreach (ChildAnimatorState child in clone.states)
                {
                    child.state.transitions =
                        CloneTransitions(((AnimatorState)sources[child.state]).transitions);
                }

                // Transitions into a nested machine are stored on the parent, keyed by the child, so
                // they are read and written through the clone rather than off the child itself.
                foreach (ChildAnimatorStateMachine child in clone.stateMachines)
                {
                    clone.SetStateMachineTransitions(child.stateMachine,
                        CloneTransitions(clone.GetStateMachineTransitions(child.stateMachine)));
                }

                foreach (ChildAnimatorStateMachine child in clone.stateMachines)
                {
                    RemapTransitions(child.stateMachine);
                }

                // Last, because the default state must already have been copied by RemapStates for
                // the lookup to return the copy rather than create a second one.
                clone.defaultState = Clone(original.defaultState);
            }

            copy.stateMachine = CopyMachineTree(source.stateMachine);
            RemapStates(copy.stateMachine);
            RemapTransitions(copy.stateMachine);

            if (index >= 0 && index <= destination.layers.Length - 1)
            {
                AnimatorControllerLayer[] layers = destination.layers;
                ArrayUtility.Insert(ref layers, index, copy);
                destination.layers = layers;
            }
            else
            {
                destination.AddLayer(copy);
            }

            return copy;
        }

        /// <summary>
        /// Copies every layer setting except the name and the state machine: weight, mask, blending
        /// mode, IK pass and the synced-layer pair.
        /// </summary>
        /// <remarks>
        /// The name is deliberately not copied -- callers have already chosen a unique one -- and
        /// neither is the state machine, which is the caller's job to deep-copy.
        /// <para>
        /// The weight is corrected rather than copied when <paramref name="source"/> turns out to be
        /// the *first* layer of the controller it came from. Unity ignores the stored weight of layer
        /// 0 and always plays it at full weight, so that field is routinely left at 0 in shipped
        /// controllers. Copying it verbatim into a non-first position would produce a layer that
        /// silently does nothing, which is the single most confusing outcome of a layer copy; a base
        /// layer is therefore taken to mean weight 1. The owning controller is found by asking the
        /// asset database which controller the source's state machine is a sub-asset of, so a layer
        /// that is not saved to disk yet -- or whose machine was reparented -- falls through to the
        /// stored value.
        /// </para>
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

        /// <summary>
        /// Copies every layer of <paramref name="source"/> into <paramref name="destination"/>,
        /// behind a cancellable-looking but in fact uncancellable progress bar, and reports the
        /// copies in the source's layer order.
        /// </summary>
        /// <remarks>
        /// <para>
        /// ASSET MUTATION, as <see cref="CopyLayer"/>: layers are appended to the destination and
        /// each brings its own sub-assets. Nothing is overwritten and nothing outside the copied
        /// layers is left stale, but no <see cref="Undo"/> is registered -- <see cref="CopyLayer"/>
        /// is called with its undo flag at the default false, so this operation cannot be undone at
        /// all.
        /// </para>
        /// <para>
        /// Parameters are not copied. A layer whose conditions reference parameters the destination
        /// lacks arrives with those conditions intact but unbound; see
        /// <see cref="CopyLayersAndParameters"/>.
        /// </para>
        /// <para>
        /// The <c>try</c>/<c>finally</c> is there only to clear the progress bar. An exception part
        /// way through still propagates, and leaves the destination holding however many layers had
        /// been copied by then -- there is no rollback.
        /// </para>
        /// </remarks>
        internal static void CopyLayers(AnimatorController source, AnimatorController destination,
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
                    copiedLayers[i] = CopyLayer(layers[i], destination);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Copies every layer and every parameter of <paramref name="source"/> into
        /// <paramref name="destination"/>.
        /// </summary>
        /// <param name="copiedParameters">
        /// One entry per source parameter, in source order. An entry is the destination's parameter
        /// when this call created it; when the destination already had a parameter of that name the
        /// entry is <c>null</c> unless <paramref name="includeExisting"/> asks otherwise. The array
        /// is therefore a record of what this call *added*, which is what a caller undoing a merge
        /// needs -- it must not delete parameters the destination owned beforehand.
        /// </param>
        /// <param name="includeExisting">
        /// Reports pre-existing parameters as well, turning the array into a plain name-to-parameter
        /// map for callers that want to rewire conditions rather than track ownership.
        /// </param>
        /// <remarks>
        /// <para>
        /// ASSET MUTATION on <paramref name="destination"/>: layers appended with their sub-assets,
        /// parameters added. No <see cref="Undo"/> is registered anywhere in the operation. Existing
        /// parameters are never overwritten -- a name collision reuses the destination's parameter
        /// and keeps its current default value, and a *type* collision only logs a warning (see
        /// <see cref="GetOrAddParameter(AnimatorController, string, AnimatorControllerParameterType, float, out bool)"/>)
        /// while still reusing it. That last case is the one to watch: the copied layers' conditions
        /// are left bound to a parameter of the wrong type, which Unity resolves by ignoring them.
        /// </para>
        /// <para>
        /// Layers are copied before parameters. Since <see cref="CopyLayer"/> does not consult the
        /// destination's parameter list, the order is not load-bearing, but it does mean the
        /// controller is briefly in a state where the new layers reference parameters that do not
        /// exist yet.
        /// </para>
        /// </remarks>
        internal static void CopyLayersAndParameters(AnimatorController source, AnimatorController destination,
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
                    copiedLayers[i] = CopyLayer(layers[i], destination);
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Copying Parameters",
                        $"Copying parameter {i + 1} of {parameters.Length}", (float)i / parameters.Length);

                    AnimatorControllerParameter parameter =
                        destination.GetOrAddParameter(parameters[i], out bool wasAdded);
                    copiedParameters[i] = wasAdded || includeExisting ? parameter : null;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
