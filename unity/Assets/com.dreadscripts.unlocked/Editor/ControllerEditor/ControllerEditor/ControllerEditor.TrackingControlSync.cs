// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   MoveAnnotation -> RefreshTrackingControlEditor, line 9222
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// One member, in a file of its own, for the reason ControllerEditor.CollapsibleSection.cs and
// ControllerEditor.ParameterNames.cs are: it is the only writer of `trackingControlEditor` and
// `allStatesHaveTrackingControl`, and its only caller in this package is the selection-sync routine,
// which belongs to another file and another decompiled region. Claiming decompiled line 9222 from
// ControllerEditor.SelectionSync.cs would have had that file claim a member outside its own range.
//
// `MoveAnnotation` moves nothing. What the body does is scan the selected states for VRChat's
// Animator Tracking Control behaviour and, only if *every* selected state has at least one, wrap the
// whole set in one TrackingControlEditor so the ten tracking flags can be edited across the
// selection at once.
//
// ALL-OR-NOTHING IS THE POINT, not a shortcut. The editor it builds is a multi-object
// SerializedObject over the behaviours found; if one selected state had no tracking-control
// behaviour, editing a flag through that editor would silently apply to some of the selection and
// not the rest. Rather than draw a partial editor, the shipped code sets the flag that suppresses
// the whole "VRC Tracking Control" sub-section (ControllerEditor.State.cs's
// `allStatesHaveTrackingControl`, which the still-unported CloneVisitor row reads) and leaves
// `trackingControlEditor` holding whatever it last held.
//
// A state with several tracking-control behaviours contributes all of them. The inner loop does not
// break on the first hit, so the resulting SerializedObject can be longer than the selection. That
// is shipped and is coherent: every one of those behaviours is on a state the user has selected.
//
// THE EMPTY SELECTION. With nothing selected the foreach does not run, so the flag survives as true
// until `list.Count > 0` folds it back to false on the next line. That second assignment is what
// makes an empty selection hide the section rather than show an editor over an empty array, and it
// is why the flag is written twice rather than once.
//
// AnimatorTypeCache.GetTrackingControlType() is a method in the decompiled source and a property in
// the port (AnimatorTypeCache.TrackingControlType); it is read as one here. It returns null when the
// VRChat SDK is absent, which cannot arise on this path -- the shipped caller only reaches here
// inside `if (AnimatorTypeCache.IsVRCSDKAvailable())`, and a null right-hand side would simply never
// match a live behaviour's type in any case.
//
// ================================ DELIBERATE DEVIATION =========================================
//
// The decompiled `StateMachineBehaviour[] behaviours = selectedState.behaviours;` temporary, which
// exists only because ILSpy spells out the array the inner foreach enumerates, is dropped and the
// property is enumerated directly. `AnimatorState.behaviours` is a getter that allocates, so this
// does change how many times it is called -- once per state either way, because a foreach evaluates
// its source expression exactly once.
//
// The two decompiled `flag` locals are named for what they carry: the outer one is the field
// itself (there is no outer local), and the inner one becomes `stateHasOne`.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- the body below was diffed statement for statement against decompiled
// lines 9222-9249: the optimistic initial assignment, the accumulator list, the two nested loops,
// the exact-type comparison (`GetType() ==`, not `is`, so a subclass of the SDK behaviour would not
// count), the early `break` on the first state without one, the second assignment folding in
// `list.Count > 0`, and the guarded construction of TrackingControlEditor from `list.ToArray()`.
// Both fields it writes were followed to their declarations in ControllerEditor.State.cs, and
// TrackingControlEditor's constructor signature was checked in
// Editor/ControllerEditor/MultiEditors/TrackingControlEditor.cs. The range contains no `goto`, no
// residual `switch` dispatch, no `while (true)` and no unresolved `smethod_N`, so no deobfuscator
// fault applies to it, and it carries no licence gate.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Tracking control multi-editing

        /// <summary>
        /// Rebuilds <see cref="trackingControlEditor"/> over the VRChat tracking-control behaviours
        /// of the selected states, and sets <see cref="allStatesHaveTrackingControl"/> to say
        /// whether that editor is meaningful.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The editor is built only when every selected state carries at least one such behaviour
        /// and at least one was found overall. Anything less and the flag goes false, which is what
        /// the "VRC Tracking Control" sub-section tests before drawing -- see the file header for
        /// why a partial editor is worse than none.
        /// </para>
        /// <para>
        /// When the flag is false the previous editor is left in place rather than nulled. Nothing
        /// reads it while the flag is false, so the stale editor is unreachable rather than wrong.
        /// </para>
        /// </remarks>
        private static void RefreshTrackingControlEditor()
        {
            allStatesHaveTrackingControl = true;
            List<StateMachineBehaviour> found = new List<StateMachineBehaviour>();

            foreach (AnimatorState state in selectedStates)
            {
                bool stateHasOne = false;

                // Exact type, not `is`: the behaviour type is resolved by name out of the VRChat
                // SDK, and a derived behaviour is not the one this editor's property paths fit.
                foreach (StateMachineBehaviour behaviour in state.behaviours)
                {
                    if (behaviour.GetType() == AnimatorTypeCache.TrackingControlType)
                    {
                        stateHasOne = true;
                        found.Add(behaviour);
                    }
                }

                if (!stateHasOne)
                {
                    allStatesHaveTrackingControl = false;
                    break;
                }
            }

            // The second write is what handles the empty selection: the loop above cannot clear the
            // flag when it never runs. See the file header.
            allStatesHaveTrackingControl = allStatesHaveTrackingControl && found.Count > 0;

            if (allStatesHaveTrackingControl)
            {
                trackingControlEditor = new TrackingControlEditor(found.ToArray());
            }
        }

        #endregion
    }
}
