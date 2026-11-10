// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   LoginVisitor        -> RefreshStateProperties,      line 12506
//   PushVisitor         -> RefreshTransitionProperties, line 12714
//   CalculateAnnotation -> RefreshInspectorProperties,  line 9768
//   RunAlgo             -> RebuildTransitionInspector,  line 14801
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// These four are the write side of the two SerializedProperty banks that ControllerEditor.State.cs
// declares and that the window's inspectors read every frame. They live here, in a partial of
// their own, because they belong to no one section: the state bank is drawn by the state section,
// the transition bank by the transition section, and both banks are rebuilt together from the
// selection-sync routine. Splitting them between those callers would have meant one of them
// claiming a decompiled member belonging to another's region, which is the mistake
// ControllerEditor.CollapsibleSection.cs was created to avoid.
//
// None of the four is blocked on anything. All four are leaves: they touch only fields
// ControllerEditor.State.cs already declares, and call nothing outside this file except
// SerializedObject.FindProperty and the SerializedObject constructor. That is why they could be
// ported ahead of the section bodies that need them.
//
// The obfuscated names say nothing about the bodies -- `LoginVisitor` does not log in,
// `PushVisitor` pushes nothing, `CalculateAnnotation` calculates nothing and `RunAlgo` is not an
// algorithm -- so each ported name below says which bank the member rebuilds instead. The
// obfuscated spellings are kept only in the MAP above, as the join key.
//
// Two facts about these banks that are not obvious from the bodies:
//
//   The property handles are only as fresh as the SerializedObject they were taken from. A
//   SerializedProperty is bound to the SerializedObject it came from, so re-pointing
//   `transitionInspectorSerialized` at a different target -- which RebuildTransitionInspector does
//   whenever the selection changes -- leaves every handle in the transition bank bound to the old
//   one. That is why the shipped selection-sync routine calls RunAlgo and then CalculateAnnotation,
//   in that order, and why the same order has to be kept wherever else the pair is called.
//
//   Both refreshers skip rather than clear when their SerializedObject is null, so the bank keeps
//   the handles it last held instead of being nulled out. That is worth knowing for the state bank
//   in particular, because `selectedStatesSerialized` genuinely does go null: the selection-sync
//   routine assigns it null whenever no state is selected (decompiled line 8780). What draws, or
//   declines to draw, from a stale state bank is a question for the state section, not for here.
//
// =============================== DELIBERATE DEVIATION =========================================
//
// The two refreshers are written with an early return where the decompiled bodies wrap their whole
// contents in `if (x != null) { ... }`. Same condition, same guarded statements, one less level of
// indentation across sixteen and ten assignments respectively.
//
// RefreshTransitionProperties drops the decompiled `SerializedObject serializedObject =
// transitionInspectorSerialized;` alias and calls FindProperty on the field directly, matching what
// RefreshStateProperties does with its own field. The alias is a decompiler artefact of the null
// check being hoisted, not something the shipped source can be observed to have had; the field is
// not reassigned anywhere in the method, so the two read the same object.
//
// RebuildTransitionInspector is written as a three-armed if/else chain. The decompiled body is the
// same decision written inside out -- `if (!(focusedTransition.transition != null)) { ... } else
// { ... }` with the assignment lifted into a shared local -- which is the shape ILSpy produces for
// a value assigned on every path of a nested branch. The arms, their order and their conditions
// are unchanged; only the double negation and the lifted local are gone.
//
// RebuildTransitionInspector also drops the decompiled `UnityEngine.Object[] objs =
// selectedStateTransitions.ToArray();` temporary and passes the array straight to the
// SerializedObject constructor. The temporary exists in the decompilation only to spell out the
// array covariance that picks `SerializedObject(Object[])` over `SerializedObject(Object)`; the
// same overload is chosen without it, because `AnimatorStateTransition[]` converts implicitly to
// `Object[]` and no other candidate applies.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- every statement of all four members was compared against decompiled
// ControllerEditor.cs lines 12506-12527, 12714-12730, 9768-9772 and 14801-14821 respectively,
// including the sixteen state property paths and the ten transition property paths, each checked
// string for string and in decompiled order. Every field the four assign or read was followed to
// its declaration in ControllerEditor.State.cs and its type confirmed. None of the four ranges
// contains a `goto`, a residual `switch` dispatch, a `while (true)` or an unresolved `smethod_N`,
// so no deobfuscator fault applies and nothing here is reconstructed rather than transcribed.

using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Inspector property banks

        /// <summary>
        /// Re-reads the state inspector's <see cref="SerializedProperty"/> bank from
        /// <see cref="selectedStatesSerialized"/>.
        /// </summary>
        /// <remarks>
        /// Does nothing when no state is selected, which leaves the bank holding the previous
        /// selection's handles rather than nulling them out. See the file header.
        /// </remarks>
        private static void RefreshStateProperties()
        {
            if (selectedStatesSerialized == null)
            {
                return;
            }

            stateNameProperty = selectedStatesSerialized.FindProperty("m_Name");
            stateTagProperty = selectedStatesSerialized.FindProperty("m_Tag");
            stateMotionProperty = selectedStatesSerialized.FindProperty("m_Motion");
            stateSpeedProperty = selectedStatesSerialized.FindProperty("m_Speed");
            stateSpeedParameterProperty = selectedStatesSerialized.FindProperty("m_SpeedParameter");
            stateTimeParameterProperty = selectedStatesSerialized.FindProperty("m_TimeParameter");
            stateMirrorProperty = selectedStatesSerialized.FindProperty("m_Mirror");
            stateCycleOffsetProperty = selectedStatesSerialized.FindProperty("m_CycleOffset");
            stateIkOnFeetProperty = selectedStatesSerialized.FindProperty("m_IKOnFeet");
            stateWriteDefaultsProperty = selectedStatesSerialized.FindProperty("m_WriteDefaultValues");
            stateSpeedParameterActiveProperty = selectedStatesSerialized.FindProperty("m_SpeedParameterActive");
            stateTimeParameterActiveProperty = selectedStatesSerialized.FindProperty("m_TimeParameterActive");
            stateMirrorParameterActiveProperty = selectedStatesSerialized.FindProperty("m_MirrorParameterActive");
            stateCycleOffsetParameterActiveProperty = selectedStatesSerialized.FindProperty("m_CycleOffsetParameterActive");
            stateMirrorParameterProperty = selectedStatesSerialized.FindProperty("m_MirrorParameter");
            stateCycleOffsetParameterProperty = selectedStatesSerialized.FindProperty("m_CycleOffsetParameter");
        }

        /// <summary>
        /// Re-reads the transition inspector's <see cref="SerializedProperty"/> bank from
        /// <see cref="transitionInspectorSerialized"/>.
        /// </summary>
        /// <remarks>
        /// Must run after <see cref="RebuildTransitionInspector"/> whenever that has re-pointed the
        /// SerializedObject, because the handles below are bound to the object they were taken
        /// from. Does nothing when the SerializedObject is null.
        /// </remarks>
        private static void RefreshTransitionProperties()
        {
            if (transitionInspectorSerialized == null)
            {
                return;
            }

            transitionHasExitTimeProperty = transitionInspectorSerialized.FindProperty("m_HasExitTime");
            transitionExitTimeProperty = transitionInspectorSerialized.FindProperty("m_ExitTime");
            transitionHasFixedDurationProperty = transitionInspectorSerialized.FindProperty("m_HasFixedDuration");
            transitionDurationProperty = transitionInspectorSerialized.FindProperty("m_TransitionDuration");
            transitionOffsetProperty = transitionInspectorSerialized.FindProperty("m_TransitionOffset");
            transitionInterruptionSourceProperty = transitionInspectorSerialized.FindProperty("m_InterruptionSource");
            transitionOrderedInterruptionProperty = transitionInspectorSerialized.FindProperty("m_OrderedInterruption");
            transitionCanTransitionToSelfProperty = transitionInspectorSerialized.FindProperty("m_CanTransitionToSelf");
            transitionSoloProperty = transitionInspectorSerialized.FindProperty("m_Solo");
            transitionMuteProperty = transitionInspectorSerialized.FindProperty("m_Mute");
        }

        /// <summary>
        /// Re-reads both inspector property banks. This is the pair the window refreshes together
        /// whenever the selection changes.
        /// </summary>
        private static void RefreshInspectorProperties()
        {
            RefreshStateProperties();
            RefreshTransitionProperties();
        }

        /// <summary>
        /// Points <see cref="transitionInspectorSerialized"/> at whatever the transition inspector
        /// should currently be editing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Three cases, in priority order. A focused transition wins outright and is edited alone.
        /// Otherwise the inspector multi-edits every selected state transition. With neither, it
        /// falls back to <see cref="mixedValueTransitionSerialized"/> -- the pair of throwaway
        /// transitions built to disagree on every field, so that a disabled inspector shows every
        /// row as a mixed value instead of showing one arbitrary transition's settings.
        /// </para>
        /// <para>
        /// This invalidates the transition property bank, so <see cref="RefreshTransitionProperties"/>
        /// has to run afterwards.
        /// </para>
        /// </remarks>
        private static void RebuildTransitionInspector()
        {
            if (focusedTransition.transition != null)
            {
                transitionInspectorSerialized = new SerializedObject(focusedTransition.transition);
            }
            else if (selectedStateTransitions.Count > 0)
            {
                transitionInspectorSerialized = new SerializedObject(selectedStateTransitions.ToArray());
            }
            else
            {
                transitionInspectorSerialized = mixedValueTransitionSerialized;
            }
        }

        #endregion
    }
}
