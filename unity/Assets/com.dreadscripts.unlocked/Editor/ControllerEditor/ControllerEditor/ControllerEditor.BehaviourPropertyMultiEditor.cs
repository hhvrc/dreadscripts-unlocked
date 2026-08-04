// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   BehaviourPropertyMultiEditor -> BehaviourPropertyMultiEditor, lines 234-282 (name already in renames/)
//     matched                    -> matched,        line 236
//     entry                      -> entry,          line 238
//     targets                    -> targets,        line 240
//     AddMatch                   -> AddMatch,       line 249
//     ApplyToAll                 -> ApplyToAll,     line 255
//     RemoveFromAll              -> RemoveFromAll,  line 265
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The tuple elements of `targets` are given names here (`driver`, `index`); the decompile shows
// them as Item1/Item2 because tuple element names live in an attribute the obfuscator dropped.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   RestartAnnotation, m_AlgoAnnotation  -- ControllerEditor outer class body (copy one parameter
//                                           entry onto another; the currently selected states)
//   DeletePredicate, LoginPredicate      -- EditorUtils (not yet ported)
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// One row of the VRC parameter-driver list when several drivers are selected at once: the
        /// entry as shown, plus every (driver, entry index) pair it stands for.
        /// </summary>
        /// <remarks>
        /// The counterpart of <see cref="ConditionMultiEditor"/> for
        /// <c>VRCAvatarParameterDriver.parameters</c>. Removing a row can empty a driver, and an
        /// empty driver is deleted from the states that carry it — which is why
        /// <see cref="RemoveFromAll"/> reaches back out to the selected states.
        /// </remarks>
        private class BehaviourPropertyMultiEditor
        {
            /// <summary>True once a second driver has been folded into this group.</summary>
            internal bool matched;

            /// <summary>The entry as displayed; the first target's value until edited.</summary>
            internal AnimatorTypeCache.ParameterDriverBinding.ParameterEntry entry;

            internal List<(AnimatorTypeCache.ParameterDriverBinding driver, int index)> targets;

            internal BehaviourPropertyMultiEditor(AnimatorTypeCache.ParameterDriverBinding driver, int entryIndex)
            {
                matched = false;
                entry = driver.parameters[entryIndex];
                targets = new List<(AnimatorTypeCache.ParameterDriverBinding, int)> { (driver, entryIndex) };
            }

            internal void AddMatch(AnimatorTypeCache.ParameterDriverBinding driver, int entryIndex)
            {
                matched = true;
                targets.Add((driver, entryIndex));
            }

            internal void ApplyToAll(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry value)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    AnimatorTypeCache.ParameterDriverBinding driver = targets[i].driver;
                    RestartAnnotation(value, driver.parameters[targets[i].index]);
                    EditorUtility.SetDirty(driver.behaviour);
                }
            }

            internal void RemoveFromAll()
            {
                foreach (var target in targets)
                {
                    AnimatorTypeCache.ParameterDriverBinding driver = target.driver;
                    bool driverIsNowEmpty = driver.RemoveParameter(target.index);
                    EditorUtility.SetDirty(driver.behaviour);

                    if (driverIsNowEmpty)
                    {
                        m_AlgoAnnotation.ForEach(delegate(AnimatorState s)
                        {
                            s.DeletePredicate(s.LoginPredicate(driver.behaviour), verifytemp: true);
                        });
                    }
                }
            }
        }
    }
}
