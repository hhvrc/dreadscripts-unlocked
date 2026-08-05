// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   BehaviourPropertyMultiEditor                     -> BehaviourPropertyMultiEditor,  line 234
//     matched                                        -> matched,                       line 236
//     entry                                          -> entry,                         line 238
//     targets                                        -> targets,                       line 240
//     .ctor(ParameterDriverBinding, int)             -> .ctor,                         line 242
//     AddMatch                                       -> AddMatch,                      line 249
//     ApplyToAll                                     -> ApplyToAll,                    line 255
//     RemoveFromAll                                  -> RemoveFromAll,                 line 265
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
// RemoveFromAll was deferred when this file was written and landed on 2026-08-05; the type is
// complete. Every dependency it was waiting on had in fact already been ported, under the English
// names that replaced the obfuscated ones this note was written against: `m_AlgoAnnotation` is
// ControllerEditor.selectedStates (ControllerEditor.State.cs), LoginPredicate is
// EditorUtils.IndexOfBehaviour and DeletePredicate is EditorUtils.RemoveBehaviourAt (both in
// EditorUtils.Behaviours.cs). Nothing new had to be derived -- the note had simply gone stale
// against the renames, which is worth knowing when reading the other deferral notes in this folder.
//
// The one real change the port needed: ControllerEditor.selectedStates is `internal` rather than the
// shipped `private`, because this type could reach a private static of the window when it was nested
// inside it and cannot now that it is top-level. That file's own remarks record it.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

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

        /// <summary>
        /// Drops this row's entry from every target driver, and deletes any driver behaviour the
        /// removal left with no parameters at all.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="AnimatorTypeCache.ParameterDriverBinding.RemoveParameter"/> reports whether
        /// the driver it just edited has run out of entries. An empty driver is not useful and the
        /// shipped tool does not leave one behind, so it is removed from every selected state — not
        /// only from the state the row was opened on, since one behaviour asset can be shared.
        /// </para>
        /// <para>
        /// The removal is undoable and the lookup is by reference, so a state that does not carry
        /// this behaviour yields index -1 and <see cref="EditorUtils.RemoveBehaviourAt"/> returns
        /// without touching it.
        /// </para>
        /// </remarks>
        internal void RemoveFromAll()
        {
            foreach ((AnimatorTypeCache.ParameterDriverBinding driver, int index) in targets)
            {
                bool driverIsNowEmpty = driver.RemoveParameter(index);
                EditorUtility.SetDirty(driver.behaviour);

                if (driverIsNowEmpty)
                {
                    ControllerEditor.selectedStates.ForEach<AnimatorState>(
                        s => s.RemoveBehaviourAt(s.IndexOfBehaviour(driver.behaviour), withUndo: true));
                }
            }
        }
    }
}
