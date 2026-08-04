// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static VisitPredicate       -> AddLayer,            line 3275
//   static DefinePredicate      -> IsTagTransition,     line 3293
//   static StartPredicate       -> AddTag,              line 3302
//   static ReadPredicate        -> RemoveTag,           line 3313
//   static SelectPredicate      -> HasTag,              line 3319
//   static RemovePredicate      -> GetTags,             line 3335
//   static InstantiatePredicate -> GetSystemTags,       line 3341
//   static AwakePredicate       -> GetUserTags,         line 3348
//   static ConnectPredicate     -> CopyParameters,      line 3402
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: UNAUDITED -- was VERIFIED in 2b1c7ff, but the code has changed
// since (-216 code lines); needs re-checking against export/ before the claim is restored.
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
                AnimatorControllerParameter parameter = target.GetOrAddParameter(parameters[i], out bool added);
                copies[i] = (added || includeExisting) ? parameter : null;
            }

            EditorUtility.ClearProgressBar();
            return copies;
        }
    }
}
