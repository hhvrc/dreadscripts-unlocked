// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ControllerEditorWindow.DeleteTests -> RebuildTransitionSerializedObject, line 3853
//   ControllerEditorWindow.CreateTests -> RebuildStateSerializedObject,      line 3875
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference. See ControllerEditorWindow.cs for the full member map.
//
// Both members are ported in full. RebuildTransitionSerializedObject carries a shipped bug, which
// is reproduced exactly and documented on the method.
//
// Audit status: VERIFIED -- both members were diffed statement by statement against export
// ControllerEditor.cs lines 3853-3907 on 2026-08-05. All 16 FindProperty paths in
// RebuildStateSerializedObject and all 10 in RebuildTransitionSerializedObject match string for
// string and in order. The SHIPPED BUG is confirmed present in export exactly as described: the
// null branch at 3855-3858 creates the transition without building a SerializedObject over it,
// while the six FindProperty calls at 3867-3872 sit outside the if/else and run unconditionally.
// The asymmetry against RebuildStateSerializedObject -- which creates the missing state and then
// binds unconditionally -- is the original's. The requiresStateRename one-shot and its
// ApplyModifiedPropertiesWithoutUndo match. The range contains no goto, no residual switch
// dispatch, no `while (true)` and no unresolved smethod_N, so no deobfuscator fault applies here.

using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditorWindow
    {
        /// <summary>
        /// Rebinds every <see cref="SerializedProperty"/> the transition-defaults tab draws, after
        /// the template transition has been created or replaced.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The property paths are Unity's internal serialised names for
        /// <see cref="AnimatorStateTransition"/>, not its public API, which is what lets the tab
        /// reuse the stock property drawers.
        /// </para>
        /// <para>
        /// SHIPPED BUG, PRESERVED. The two branches are not symmetric. When the template transition
        /// is missing, the shipped code creates one but does <em>not</em> build a
        /// <see cref="SerializedObject"/> over it, then falls through to six unconditional
        /// <c>FindProperty</c> calls against that object -- so the very first run after the setting
        /// is cleared throws <see cref="System.NullReferenceException"/>, and on a later run leaves
        /// the four properties in the else-branch bound to the previous, now-discarded transition.
        /// Compare <see cref="RebuildStateSerializedObject"/>, which handles the same situation
        /// correctly by creating the missing object and then binding unconditionally. The behaviour
        /// is reproduced as shipped; the asymmetry below is the original's, not a transcription
        /// error.
        /// </para>
        /// </remarks>
        internal static void RebuildTransitionSerializedObject()
        {
            if (EditorSettings.Instance.defaultTransition == null)
            {
                EditorSettings.Instance.defaultTransition = new AnimatorStateTransition();
            }
            else
            {
                transitionObject = new SerializedObject(EditorSettings.Instance.defaultTransition);
                transitionSolo = transitionObject.FindProperty("m_Solo");
                transitionMute = transitionObject.FindProperty("m_Mute");
                transitionDuration = transitionObject.FindProperty("m_TransitionDuration");
                transitionOffset = transitionObject.FindProperty("m_TransitionOffset");
            }

            transitionExitTime = transitionObject.FindProperty("m_ExitTime");
            transitionHasExitTime = transitionObject.FindProperty("m_HasExitTime");
            transitionHasFixedDuration = transitionObject.FindProperty("m_HasFixedDuration");
            transitionInterruptionSource = transitionObject.FindProperty("m_InterruptionSource");
            transitionOrderedInterruption = transitionObject.FindProperty("m_OrderedInterruption");
            transitionCanTransitionToSelf = transitionObject.FindProperty("m_CanTransitionToSelf");
        }

        /// <summary>
        /// Rebinds every <see cref="SerializedProperty"/> the state-defaults tab draws, after the
        /// template state has been created or replaced.
        /// </summary>
        /// <remarks>
        /// The rename step exists because a template created from scratch elsewhere in the tool
        /// carries whatever name Unity gave it. <c>requiresStateRename</c> is a one-shot flag: it is
        /// set when a template is made and cleared here, and the write is applied
        /// <em>without</em> undo, so the rename never appears as a user edit that Ctrl+Z could take
        /// back to a name the user never chose.
        /// </remarks>
        internal static void RebuildStateSerializedObject()
        {
            if (EditorSettings.Instance.defaultState == null)
            {
                EditorSettings.Instance.defaultState = new AnimatorState
                {
                    name = "New State"
                };
            }

            stateObject = new SerializedObject(EditorSettings.Instance.defaultState);
            stateName = stateObject.FindProperty("m_Name");

            if (stateName != null && EditorSettings.Instance.requiresStateRename)
            {
                stateName.stringValue = "New State";
                EditorSettings.Instance.requiresStateRename.value = false;
                stateObject.ApplyModifiedPropertiesWithoutUndo();
            }

            stateSpeed = stateObject.FindProperty("m_Speed");
            stateCycleOffset = stateObject.FindProperty("m_CycleOffset");
            stateIkOnFeet = stateObject.FindProperty("m_IKOnFeet");
            stateWriteDefaults = stateObject.FindProperty("m_WriteDefaultValues");
            stateMirror = stateObject.FindProperty("m_Mirror");
            stateSpeedParameterActive = stateObject.FindProperty("m_SpeedParameterActive");
            stateMirrorParameterActive = stateObject.FindProperty("m_MirrorParameterActive");
            stateCycleOffsetParameterActive = stateObject.FindProperty("m_CycleOffsetParameterActive");
            stateTimeParameterActive = stateObject.FindProperty("m_TimeParameterActive");
            stateMotion = stateObject.FindProperty("m_Motion");
            stateTag = stateObject.FindProperty("m_Tag");
            stateSpeedParameter = stateObject.FindProperty("m_SpeedParameter");
            stateMirrorParameter = stateObject.FindProperty("m_MirrorParameter");
            stateCycleOffsetParameter = stateObject.FindProperty("m_CycleOffsetParameter");
            stateTimeParameter = stateObject.FindProperty("m_TimeParameter");
        }
    }
}
