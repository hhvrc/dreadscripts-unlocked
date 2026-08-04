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
//
// NOTES
//
// LAYER TAGS. The tool stores per-layer metadata inside the animator asset itself, because
// AnimatorControllerLayer has nowhere to put custom data. A tag is an any-state transition that is
// muted, marked isExit and given no destination -- so Unity never takes it and it costs nothing at
// runtime -- whose *name* carries the payload. Tags whose name starts with '_' are written by the
// tool (the only one shipped is "_category:<path>", read back by ControllerEditor's layer list);
// tags without the prefix are whatever the user typed. GetSystemTags/GetUserTags are the two
// halves of that split, and the '_' convention is the only thing distinguishing them.
//
// The deep-copy machinery this file used to carry (CopyLayer and the [CompilerGenerated] locals of
// decompiled CalculatePredicate) was moved to EditorUtils.LayerCopying.cs in the port-reconciliation
// merges; the prose describing it went with it. What is left here is the tag helpers, AddLayer and
// CopyParameters.
//
// Audit status: VERIFIED against decompiled/ -- all nine declared members diffed statement by
// statement against decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs at the
// cited lines (VisitPredicate 3275, DefinePredicate 3293, StartPredicate 3302, ReadPredicate 3313,
// SelectPredicate 3319, RemovePredicate 3335, InstantiatePredicate 3341, AwakePredicate 3348,
// ConnectPredicate 3402), all of which still land on the named member in the current snapshot. The
// only differences are decompiler-artifact removals: inverted guard clauses restored to positive
// form (IsTagTransition, AddTag, HasTag) and LINQ query syntax written back as method syntax
// (GetTags, GetSystemTags, GetUserTags). No behavioural divergence found; the header claims no
// member the file does not declare.

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
