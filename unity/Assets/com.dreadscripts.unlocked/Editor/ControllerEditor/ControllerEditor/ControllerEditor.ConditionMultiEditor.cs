// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ConditionMultiEditor  -> ConditionMultiEditor, lines 109-232 (name already in renames/)
//     matched             -> matched,          line 111
//     condition           -> condition,        line 113
//     targets             -> targets,          line 115
//     mixedValues         -> mixedValues,      line 117
//     AddMatch            -> AddMatch,         line 126
//     ApplyToAll(cond)    -> ApplyToAll,       line 132
//     ApplyToAll(func)    -> ApplyToAll,       line 146
//     SetParameter        -> SetParameter,     line 160
//     SetMode             -> SetMode,          line 177
//     SetThreshold        -> SetThreshold,     line 194
//     Invert              -> Invert,           line 211
//     RemoveFromAll       -> RemoveFromAll,    line 217
//     MarkMixedValues     -> MarkMixedValues,  line 225
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The tuple elements of `targets` are given names here (`transition`, `index`); the decompile shows
// them as Item1/Item2 because tuple element names live in an attribute the obfuscator dropped.
//
// ResolveAlgo belongs to the ControllerEditor outer class body, which is not ported yet, so it
// keeps its decompiled name here. It returns the condition with its mode inverted.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// One row of the condition list when several transitions are selected at once: the
        /// condition as shown, plus every (transition, condition index) pair it stands for.
        /// </summary>
        /// <remarks>
        /// Multi-editing is built by matching conditions across the selected transitions into
        /// groups. A group draws once and writes to all of its targets, and <see cref="mixedValues"/>
        /// records per column (parameter, mode, threshold) whether the grouped conditions actually
        /// agree, so the GUI can show the mixed-value dash. Writing any column clears that column's
        /// mixed flag, because after the write every target holds the same value.
        ///
        /// <see cref="AnimatorTransitionBase.conditions"/> returns a copy, so every write has to
        /// read the array, change one entry and assign the array back.
        /// </remarks>
        private class ConditionMultiEditor
        {
            /// <summary>True once a second transition has been folded into this group.</summary>
            internal bool matched;

            /// <summary>The condition as displayed; the first target's value until edited.</summary>
            internal AnimatorCondition condition;

            internal readonly List<(AnimatorTransitionBase transition, int index)> targets;

            /// <summary>Per column — parameter, mode, threshold — whether the targets disagree.</summary>
            internal readonly bool[] mixedValues = new bool[3];

            internal ConditionMultiEditor(AnimatorTransitionBase transition, int conditionIndex)
            {
                matched = false;
                condition = transition.conditions[conditionIndex];
                targets = new List<(AnimatorTransitionBase, int)> { (transition, conditionIndex) };
            }

            internal void AddMatch(AnimatorTransitionBase transition, int conditionIndex)
            {
                matched = true;
                targets.Add((transition, conditionIndex));
            }

            internal void ApplyToAll(AnimatorCondition value)
            {
                Object[] objectsToUndo = targets.Select(t => (Object)t.transition).ToArray();
                Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");

                foreach (var target in targets)
                {
                    AnimatorCondition[] conditions = target.transition.conditions;
                    conditions[target.index] = value;
                    target.transition.conditions = conditions;
                }
            }

            private void ApplyToAll(Func<AnimatorCondition, AnimatorCondition> transform)
            {
                Object[] objectsToUndo = targets.Select(t => (Object)t.transition).ToArray();
                Undo.RecordObjects(objectsToUndo, "Multi-Edit condition");

                foreach (var target in targets)
                {
                    AnimatorCondition[] conditions = target.transition.conditions;
                    conditions[target.index] = transform(conditions[target.index]);
                    target.transition.conditions = conditions;
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

            internal void Invert()
            {
                condition = ResolveAlgo(condition);
                ApplyToAll(ResolveAlgo);
            }

            internal void RemoveFromAll()
            {
                foreach (var target in targets)
                {
                    target.transition.RemoveCondition(condition);
                }
            }

            /// <summary>
            /// Folds another condition's per-column agreement into this group's. The argument is
            /// "these columns match", so a false in any column latches the mixed flag on.
            /// </summary>
            internal void MarkMixedValues(bool[] columnsMatch)
            {
                for (int i = 0; i < 3; i++)
                {
                    mixedValues[i] |= !columnsMatch[i];
                }
            }
        }
    }
}
