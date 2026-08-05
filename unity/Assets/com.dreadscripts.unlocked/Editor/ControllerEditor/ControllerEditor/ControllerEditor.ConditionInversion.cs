// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ResolveAlgo     -> InvertCondition(AnimatorCondition),       line 15115
//   ListAlgo        -> InvertCondition(AnimatorCondition, bool), line 15120
//   CollectAlgo     -> InvertMode,                               line 15102
//   ResetAnnotation -> FindParameter,                            line 9717
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// Reversing a transition condition. `x > 0.5` becomes `x < 0.5`, and -- when the user has asked for
// it -- the threshold is nudged as well, so that the reversed condition covers exactly the values
// the original did not rather than leaving the boundary value satisfying neither. Three of the four
// members below are that operation at three levels: one mode in, one mode out; one condition with
// the caller stating whether to move the threshold; and the entry point that answers that question
// from the user's settings.
//
// The names come from the behaviour, not from the obfuscated identifiers, which are noise even by
// this assembly's standards: nothing here collects, lists or resolves anything, and ResetAnnotation
// resets nothing -- it is a lookup.
//
// ================================ DELIBERATE DEVIATION ========================================
//
// InvertCondition(AnimatorCondition) is `internal` where the shipped member is `private static`.
// Its caller ConditionMultiEditor.Invert could reach a private static of this class while
// ConditionMultiEditor was nested inside it, and cannot now that the port lifts that type to top
// level. This is the same one-word widening already applied to ControllerEditor.selectedStates for
// the same reason and recorded in MultiEditors/BehaviourPropertyMultiEditor.cs. The other three
// members keep the shipped `private static`: every one of their call sites is inside this class.
//
// ======================================== NOTES ================================================
//
// WHY THE THRESHOLD MOVES, AND BY HOW MUCH. Unity's AnimatorConditionMode has Greater and Less but
// no >= or <=, so the exact complement of `x > t` is not expressible. InvertCondition compensates by
// moving the boundary just past t: by 1 for an Int parameter, where the next representable value is
// one away, and by 0.008 for every other parameter type, which is the vendor's chosen epsilon and is
// hardcoded -- the shipped test is `type != Int`, not `type == Float`, so a Bool or Trigger compared
// with Greater/Less (which the editor permits and controllers do contain) takes the 0.008 step. The
// direction follows the *original* mode -- Greater inverts to Less with the threshold raised,
// Less to Greater with it lowered -- which is why both branches test `condition.mode` rather than
// the already-inverted `result.mode`.
//
// This only happens for Greater and Less. Equals/NotEqual and If/IfNot are exact complements
// already, and their thresholds are untouched (If/IfNot do not have one).
//
// WHEN IT HAPPENS. The one-argument entry point computes the flag as the persisted setting
// EditorSettings.Instance.reverseModifiesValues XOR the control key -- the tooltip on that setting
// in ControllerEditorWindow.Cosmetics.cs says so outright ("Hold CTRL to temporarily flip this
// setting while reversing"). The bulk transition-reversal path (decompiled 14928) does not call it;
// it passes the ControllerEditor.reverseModifiesValues field, which ControllerEditor.State.cs
// documents as the same expression snapshotted once when the menu was raised. Both spellings of
// that flag therefore mean the same thing, evaluated at different moments.
//
// FindParameter IS NOT PART OF THIS REGION and is ported here only because it is the single
// unported dependency of the threshold arithmetic above, in the way EditorUtils.AnimatorParameters
// .cs ports DestroyPredicate and IncludePredicate for RatePredicate's sake. It is a class-wide
// helper with about fifteen call sites across the god class (decompiled 9194, 9314, 11525, 12871,
// 12872, 13235-13321, 15131), none of them yet ported. When a partial covering the class's
// parameter caches lands, move it there and take its MAP entry with it rather than porting it a
// second time. Its out-parameter-less wrapper (decompiled 9711, `AwakeAnnotation`) is not ported;
// it is this method with the index discarded, and nothing this file needs calls it.
//
// CONSEQUENCE OF AN UPSTREAM PARTIAL PORT, stated so it is not mistaken for a defect here.
// FindParameter reads ControllerEditor.ActiveController, which ControllerEditor.ControllerContext.cs
// deliberately ports without its lazy initialisation and which therefore reads null until the
// setters land. So FindParameter returns null today, and the threshold adjustment silently does
// nothing while the mode inversion works. That is upstream, not a shortcut taken here: the bodies
// below are complete transcriptions, and they start behaving the moment the controller context does.
//
// NO LICENCE CODE. None of the four members touches the HWID/HMAC validation the class carries
// elsewhere, so nothing was stripped from them.
//
// NO DECOMPILER DAMAGE. All four bodies decompile as straight-line code -- no `while (true)` switch
// dispatch, no unreachable arm, nothing to attribute to the known de4dot control-flow fault. The
// only rewrites below are cosmetic, and this is all of them: the two `bool flag`/`bool flag2`
// temporaries are given names; the unused `out` index at the FindParameter call site is written
// `out _` where the decompilation declares a discarded local; the Int branch is written
// `else if (wasGreater) ... else ...` where the decompilation writes `else if (!flag) ... else ...`,
// the same two arms in the other order; and FindParameter's "no controller" arm is hoisted to an
// early return where the decompilation nests the rest of the body inside an `if`.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- all four bodies transcribed statement by statement from decompiled lines
// 9717-9730, 15102-15113, 15115-15118 and 15120-15157, re-read after the member-rename pass that
// renumbered the file. Every constant (0.008f, 1f, -1), every branch direction and the six-arm
// switch's fall-through to Greater are as shipped.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition inversion

        /// <summary>
        /// <paramref name="condition"/> reversed, moving its threshold as well if the user's
        /// "Reverse Adjusts Values" setting says so.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The entry point the interactive reverse commands use, as opposed to the overload that
        /// takes the flag outright. The setting is read live and combined with the control key, so
        /// holding Ctrl reverses with the opposite behaviour to whatever is configured -- a
        /// deliberate escape hatch rather than a modifier that only ever adds.
        /// </para>
        /// <para>
        /// Because the flag is read from <see cref="Event.current"/>, this may only be called from
        /// inside a GUI callback; the shipped code has the same constraint and both of its call
        /// sites are inspector button handlers.
        /// </para>
        /// </remarks>
        internal static AnimatorCondition InvertCondition(AnimatorCondition condition)
        {
            return InvertCondition(condition,
                (bool)EditorSettings.Instance.reverseModifiesValues ^ Event.current.control);
        }

        /// <summary>
        /// <paramref name="condition"/> with its comparison reversed, and, when
        /// <paramref name="adjustThreshold"/> is set, its threshold nudged past the boundary so the
        /// result is the exact complement of the original.
        /// </summary>
        /// <param name="adjustThreshold">
        /// Whether to move the threshold as well as the comparison. Only affects Greater and Less;
        /// the other modes are exact complements of one another already.
        /// </param>
        /// <remarks>
        /// <para>
        /// Without the adjustment, reversing <c>x &gt; 0.5</c> gives <c>x &lt; 0.5</c> and the value
        /// 0.5 itself satisfies neither, so a parameter sitting exactly on the boundary stops
        /// matching any of the transitions that used to partition it. With it, the threshold moves
        /// to the far side of the boundary instead: by one for an Int parameter, and by 0.008 -- the
        /// vendor's hardcoded epsilon -- for anything else.
        /// </para>
        /// <para>
        /// The parameter is looked up to decide which of those two steps to take, and when it cannot
        /// be found the threshold is left alone rather than guessed at.
        /// </para>
        /// </remarks>
        private static AnimatorCondition InvertCondition(AnimatorCondition condition, bool adjustThreshold)
        {
            AnimatorCondition result = condition;
            result.mode = InvertMode(condition.mode);

            if (adjustThreshold)
            {
                // Both tests are against the incoming mode, not the inverted one: a Greater is what
                // needs its threshold raised, and it has already become a Less in `result`.
                bool wasGreater = condition.mode == AnimatorConditionMode.Greater;
                bool wasLess = condition.mode == AnimatorConditionMode.Less;

                if (wasGreater || wasLess)
                {
                    AnimatorControllerParameter parameter = FindParameter(condition.parameter, out _);
                    if (parameter != null)
                    {
                        if (parameter.type != AnimatorControllerParameterType.Int)
                        {
                            if (wasGreater)
                            {
                                result.threshold += 0.008f;
                            }
                            else
                            {
                                result.threshold -= 0.008f;
                            }
                        }
                        else if (wasGreater)
                        {
                            result.threshold += 1f;
                        }
                        else
                        {
                            result.threshold -= 1f;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>The comparison that accepts everything <paramref name="mode"/> rejects.</summary>
        /// <remarks>
        /// Greater and Less are each other's opposite here only in the loose sense -- the boundary
        /// value belongs to neither -- which is what the threshold adjustment in
        /// <see cref="InvertCondition(AnimatorCondition, bool)"/> exists to repair. Note also that
        /// the final arm is a catch-all rather than a Less case: any mode that is not one of the
        /// five named, including a value Unity might add later, comes back as Greater. That is as
        /// shipped.
        /// </remarks>
        private static AnimatorConditionMode InvertMode(AnimatorConditionMode mode)
        {
            return mode switch
            {
                AnimatorConditionMode.NotEqual => AnimatorConditionMode.Equals,
                AnimatorConditionMode.Equals => AnimatorConditionMode.NotEqual,
                AnimatorConditionMode.If => AnimatorConditionMode.IfNot,
                AnimatorConditionMode.IfNot => AnimatorConditionMode.If,
                AnimatorConditionMode.Greater => AnimatorConditionMode.Less,
                _ => AnimatorConditionMode.Greater,
            };
        }

        /// <summary>
        /// The parameter named <paramref name="name"/> on <see cref="ActiveController"/>, or null
        /// when there is no controller or no such parameter.
        /// </summary>
        /// <param name="index">
        /// Its position in the controller's parameter array, or -1 on either kind of miss. Callers
        /// that go on to edit the array need the position, since writing a parameter back means
        /// assigning the whole array.
        /// </param>
        /// <remarks>
        /// A class-wide helper rather than part of the inversion region; see the file header for why
        /// it is parked here and where it should end up.
        /// </remarks>
        private static AnimatorControllerParameter FindParameter(string name, out int index)
        {
            if (!ActiveController)
            {
                index = -1;
                return null;
            }

            AnimatorControllerParameter[] parameters = ActiveController.parameters;
            if (!parameters.TryGetIndex(p => p.name == name, out index))
            {
                return null;
            }

            return parameters[index];
        }

        #endregion
    }
}
