// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ConditionMultiEditor                     -> ConditionMultiEditor,   line 109
//     matched                                -> matched,                line 111
//     condition                              -> condition,              line 113
//     targets                                -> targets,                line 115
//     mixedValues                            -> mixedValues,            line 117
//     .ctor(AnimatorTransitionBase, int)     -> .ctor,                  line 119
//     AddMatch                               -> AddMatch,               line 126
//     ApplyToAll(AnimatorCondition)          -> ApplyToAll,             line 132
//     ApplyToAll(Func<...>)                  -> ApplyToAll,             line 146
//     SetParameter                           -> SetParameter,           line 160
//     SetMode                                -> SetMode,                line 177
//     SetThreshold                           -> SetThreshold,           line 194
//     Invert                                 -> Invert,                 line 211
//     RemoveFromAll                          -> RemoveFromAll,          line 217
//     MarkMixedValues                        -> MarkMixedValues,        line 225
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The type was a private nested type of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// ======================================== NOTES ================================================
//
// The type is complete: every member of decompiled ControllerEditor.ConditionMultiEditor
// (lines 109-232) is ported here.
//
// Invert was deferred when this file was written and landed on 2026-08-05. Its blocker was the
// window's condition-reversal helper, decompiled `ResolveAlgo` (line 15115), which was unported;
// it is now ControllerEditor.InvertCondition(AnimatorCondition), in
// ControllerEditor/ControllerEditor.ConditionInversion.cs, together with the three members it in
// turn needed. That file's header records the one accessibility change the call below required -- the
// helper is `internal` there where the shipped member is `private`, because this type could reach a
// private static of the window while it was nested inside it and cannot now that it is top-level.
// The same widening was already made to ControllerEditor.selectedStates for
// BehaviourPropertyMultiEditor; see that file.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- every member re-diffed statement by statement against decompiled lines
// 109-232 on the pass that added Invert, after the member-rename sweep that renumbered the file;
// all fifteen line numbers above still land on the member named. The two ApplyToAll overloads
// destructure the tuple where the decompilation reads Item1/Item2, and RemoveFromAll records no
// undo, as shipped.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One transition condition presented as a single editable row that writes through to every
    /// selected transition that carries the same condition.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity stores conditions as a value-type array on the transition, so there is no object to
    /// hand an inspector and no way to edit "the same condition" across a multi-selection. This
    /// stands in for one: it holds a working copy of the condition plus the (transition, index)
    /// pairs it was matched against, and every setter rewrites all of them in one undo step.
    /// </para>
    /// <para>
    /// Because the array is a value type, each write must read the array out, replace the element
    /// and assign the whole array back - assigning into <c>transition.conditions[i]</c> directly
    /// would mutate a copy and be lost.
    /// </para>
    /// </remarks>
    internal class ConditionMultiEditor
    {
        /// <summary>
        /// True once a second transition has been matched to this row, i.e. the row is shared.
        /// </summary>
        /// <remarks>
        /// The caller builds candidate rows from the first selected transition and then discards
        /// every row that the remaining transitions failed to match, so this doubles as the
        /// "survived matching" flag for that pass.
        /// </remarks>
        internal bool matched;

        /// <summary>
        /// The value shown in the row. Kept in step with the targets by the setters rather than
        /// read back from them, so a mixed field can still display a chosen value.
        /// </summary>
        internal AnimatorCondition condition;

        /// <summary>The transitions this row edits, each with the index of its condition.</summary>
        internal readonly List<(AnimatorTransitionBase transition, int index)> targets;

        /// <summary>
        /// Per-field "the targets disagree" flags, indexed parameter / mode / threshold, for driving
        /// the inspector's mixed-value display.
        /// </summary>
        internal readonly bool[] mixedValues = new bool[3];

        internal ConditionMultiEditor(AnimatorTransitionBase transition, int index)
        {
            matched = false;
            condition = transition.conditions[index];
            targets = new List<(AnimatorTransitionBase, int)> { (transition, index) };
        }

        /// <summary>Adds another transition whose condition at <paramref name="index"/> this row also edits.</summary>
        internal void AddMatch(AnimatorTransitionBase transition, int index)
        {
            matched = true;
            targets.Add((transition, index));
        }

        /// <summary>Replaces the condition on every target outright.</summary>
        internal void ApplyToAll(AnimatorCondition value)
        {
            UnityEngine.Object[] objectsToUndo = targets.Select(t => t.transition).ToArray();
            Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");

            foreach ((AnimatorTransitionBase transition, int index) in targets)
            {
                AnimatorCondition[] conditions = transition.conditions;
                conditions[index] = value;
                transition.conditions = conditions;
            }
        }

        /// <summary>
        /// Rewrites each target's condition through <paramref name="edit"/>, so a change to one field
        /// leaves the target's other fields alone even where they differ from this row's copy.
        /// </summary>
        private void ApplyToAll(Func<AnimatorCondition, AnimatorCondition> edit)
        {
            UnityEngine.Object[] objectsToUndo = targets.Select(t => t.transition).ToArray();
            Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");

            foreach ((AnimatorTransitionBase transition, int index) in targets)
            {
                AnimatorCondition[] conditions = transition.conditions;
                conditions[index] = edit(conditions[index]);
                transition.conditions = conditions;
            }
        }

        internal void SetParameter(string value)
        {
            condition = new AnimatorCondition
            {
                parameter = value,
                mode = condition.mode,
                threshold = condition.threshold
            };

            ApplyToAll(c => new AnimatorCondition
            {
                parameter = value,
                mode = c.mode,
                threshold = c.threshold
            });

            mixedValues[0] = false;
        }

        internal void SetMode(AnimatorConditionMode value)
        {
            condition = new AnimatorCondition
            {
                parameter = condition.parameter,
                mode = value,
                threshold = condition.threshold
            };

            ApplyToAll(c => new AnimatorCondition
            {
                parameter = c.parameter,
                mode = value,
                threshold = c.threshold
            });

            mixedValues[1] = false;
        }

        internal void SetThreshold(float value)
        {
            condition = new AnimatorCondition
            {
                parameter = condition.parameter,
                mode = condition.mode,
                threshold = value
            };

            ApplyToAll(c => new AnimatorCondition
            {
                parameter = c.parameter,
                mode = c.mode,
                threshold = value
            });

            mixedValues[2] = false;
        }

        /// <summary>Reverses this row's condition, and the matching condition on every target.</summary>
        /// <remarks>
        /// <para>
        /// Each target is reversed from <em>its own</em> value rather than from this row's, so a
        /// row whose targets disagree stays disagreeing instead of being flattened onto the
        /// displayed value -- the same reasoning as the other setters' use of the
        /// <see cref="ApplyToAll(Func{AnimatorCondition, AnimatorCondition})"/> overload.
        /// </para>
        /// <para>
        /// The mixed-value flags are deliberately left alone, unlike in the setters. Reversal does
        /// not make the targets agree, so a field that was mixed before is still mixed after.
        /// </para>
        /// <para>
        /// Whether the thresholds move as well as the comparisons is decided per call by
        /// <see cref="ControllerEditor.InvertCondition(AnimatorCondition)"/>, from the user's
        /// setting and the control key.
        /// </para>
        /// </remarks>
        internal void Invert()
        {
            condition = ControllerEditor.InvertCondition(condition);
            ApplyToAll(ControllerEditor.InvertCondition);
        }

        /// <summary>Deletes this row's condition from every target.</summary>
        /// <remarks>
        /// <para>
        /// Ported literally, including two shipped quirks. First, removal goes through Unity's
        /// <see cref="AnimatorTransitionBase.RemoveCondition"/>, which matches by value and removes
        /// the first equal entry - the stored per-target index is ignored, so on a transition
        /// holding two identical conditions the wrong one can be removed. Second, unlike every
        /// other mutator here this one records no undo, so the deletion is not undoable.
        /// </para>
        /// <para>
        /// Removing a condition also shifts the indices of every later condition on the same
        /// transition, which invalidates the other rows the caller built for it; the caller is
        /// expected to rebuild the row list afterwards.
        /// </para>
        /// </remarks>
        internal void RemoveFromAll()
        {
            foreach ((AnimatorTransitionBase transition, int _) in targets)
            {
                transition.RemoveCondition(condition);
            }
        }

        /// <summary>
        /// Folds one target's per-field comparison result into the row's mixed-value flags.
        /// </summary>
        /// <param name="fieldMatches">
        /// Parameter / mode / threshold, true where the target agrees with this row. A false marks
        /// the field mixed permanently, until a setter for that field clears it.
        /// </param>
        internal void MarkMixedValues(bool[] fieldMatches)
        {
            for (int i = 0; i < 3; i++)
            {
                mixedValues[i] |= !fieldMatches[i];
            }
        }
    }
}
