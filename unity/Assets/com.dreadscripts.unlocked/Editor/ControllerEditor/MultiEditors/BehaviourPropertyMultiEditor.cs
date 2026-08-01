// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   BehaviourPropertyMultiEditor                     -> BehaviourPropertyMultiEditor,  line 234
//     matched                                        -> matched,                       line 236
//     entry                                          -> entry,                         line 238
//     targets                                        -> targets,                       line 240
//     .ctor(ParameterDriverBinding, int)             -> .ctor,                         line 242
//     AddMatch                                       -> AddMatch,                      line 249
//     ApplyToAll                                     -> ApplyToAll,                    line 255
//   ControllerEditor.RestartAnnotation               -> inlined into ApplyToAll,       line 9578
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The type was a private nested type of the ControllerEditor window and is lifted to top level
// here, matching the convention already used for PhysBoneEditor.
//
// `RestartAnnotation` is a private static of the window class that copies one parameter entry's
// values onto another. It is the only part of ApplyToAll that lives outside this type, it is fully
// understood, and it has one other caller in the window; rather than defer ApplyToAll - which is
// the entire point of the type - its body is inlined here verbatim, including the fields it does
// *not* copy (see ApplyToAll's remarks).
//
// Deferred member (depends on code that is not ported yet, omitted rather than stubbed):
//   RemoveFromAll(), line 265 - after dropping the entry from each driver it walks the window's
//   private static selected-state list (`m_AlgoAnnotation`, line 8024) to delete any driver
//   behaviour left with no parameters, using the EditorUtils extensions LoginPredicate and
//   DeletePredicate (EditorUtils.cs lines 3615 and 3628). None of those are ported yet.

using System.Collections.Generic;
using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One VRChat parameter-driver entry presented as a single editable row that writes through to
    /// the matching entry on every selected driver behaviour.
    /// </summary>
    /// <remarks>
    /// The parameter-driver counterpart of <see cref="ConditionMultiEditor"/>: the selected states
    /// each carry their own driver behaviour, and the window matches their parameter lists up so a
    /// single row can edit the same logical entry on all of them.
    /// </remarks>
    internal class BehaviourPropertyMultiEditor
    {
        /// <summary>
        /// True once a second driver has been matched to this row, i.e. the row is shared.
        /// </summary>
        /// <remarks>
        /// The caller builds candidate rows from the first driver and then discards every row the
        /// remaining drivers failed to match, so this doubles as the "survived matching" flag.
        /// </remarks>
        internal bool matched;

        /// <summary>
        /// The entry the row displays and edits. It is one of the targets' real entries, not a copy,
        /// so editing the row's fields already writes to the first target.
        /// </summary>
        internal AnimatorTypeCache.ParameterDriverBinding.ParameterEntry entry;

        /// <summary>The drivers this row edits, each with the index of its matching entry.</summary>
        internal List<(AnimatorTypeCache.ParameterDriverBinding driver, int index)> targets;

        internal BehaviourPropertyMultiEditor(AnimatorTypeCache.ParameterDriverBinding driver, int index)
        {
            matched = false;
            entry = driver.parameters[index];
            targets = new List<(AnimatorTypeCache.ParameterDriverBinding, int)> { (driver, index) };
        }

        /// <summary>Adds another driver whose entry at <paramref name="index"/> this row also edits.</summary>
        internal void AddMatch(AnimatorTypeCache.ParameterDriverBinding driver, int index)
        {
            matched = true;
            targets.Add((driver, index));
        }

        /// <summary>
        /// Copies <paramref name="source"/>'s values onto the matching entry of every target driver.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The copy is bracketed by <c>DeferApply</c> so the eight field writes cost one
        /// <c>ApplyModifiedProperties</c> per target instead of eight.
        /// </para>
        /// <para>
        /// The entry's <c>Source</c> field is deliberately not copied - it belongs to the
        /// copy-parameter mode, which the multi-edit row does not expose - and neither is
        /// <c>LocalOnly</c>, which lives on the driver rather than the entry. This mirrors the
        /// shipped behaviour exactly; do not extend the list.
        /// </para>
        /// <para>
        /// Because every target is written through its own <see cref="SerializedObject"/>, the
        /// behaviour is dirtied explicitly afterwards so the change survives a domain reload.
        /// </para>
        /// </remarks>
        internal void ApplyToAll(AnimatorTypeCache.ParameterDriverBinding.ParameterEntry source)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                AnimatorTypeCache.ParameterDriverBinding driver = targets[i].driver;
                AnimatorTypeCache.ParameterDriverBinding.ParameterEntry target = driver.parameters[targets[i].index];

                target.DeferApply = true;
                target.Chance = source.Chance;
                target.Name = source.Name;
                target.Value = source.Value;
                target.Type = source.Type;
                target.ValueMin = source.ValueMin;
                target.ValueMax = source.ValueMax;
                target.DeferApply = false;

                EditorUtility.SetDirty(driver.behaviour);
            }
        }
    }
}
