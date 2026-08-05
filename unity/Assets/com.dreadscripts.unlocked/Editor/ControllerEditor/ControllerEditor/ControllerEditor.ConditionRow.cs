// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   TestVisitor    -> DrawConditionRow, line 11510
//   TestAnnotation -> ParameterNameAt, line 9774
//   _003C_003Ec__DisplayClass285_0 -> dissolved into DrawConditionRow, lines 6776-6846
//     _IteratorDefinition       -> the local renaming
//     publisherDefinition       -> the local controlName
//     m_ConfigurationDefinition -> the local condition
//     m_ProcDefinition          -> the local parameterChanged
//     wrapperReg, _AnnotationReg, _VisitorReg, m_AlgoReg -> not ported, cached-delegate fields
//     CompareServer             -> the local function DrawParameterNameField
//     SetServer, PostServer, SetupServer -> the lambdas of the strict-match select button
//     EnableServer, PublishServer, PopServer, ComputeServer -> the rest of the same three predicates
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// One row of the condition editor: the element callback all three condition ReorderableLists share
// (see ControllerEditor.ConditionList.cs). It edits a ConditionMultiEditor, which may stand for the
// same condition on many transitions at once, so every field is wrapped in a MixedValueScope keyed
// on that row's per-field disagreement flags and the writes go through the row's setters rather
// than onto the transition.
//
// The row draws one of two layouts. If the condition names a parameter the controller does not
// have, it gets a parameter picker, the stored name, a "Parameter not found in Controller!" notice
// and a type dropdown that creates the missing parameter. Otherwise it gets the pick/select pair,
// the parameter dropdown, a mode dropdown typed to the parameter, and a threshold field.
//
// The three enums BoolModes / IntModes / FloatModes exist to give each parameter type a dropdown
// with only the comparisons that type admits; all three are cast straight to and from
// AnimatorConditionMode, whose values they share.
//
// The tail of the row -- after both layouts -- is where the edits are committed. The three change
// flags are read there and turned into SetParameter / SetMode / SetThreshold calls on the row, each
// of which rewrites every target in one undo step. Nothing before that point touches a transition,
// which is what lets the mode-coercion block below fix up a nonsensical mode (a Float condition
// left on Equals, say) and have the fix committed by the same mechanism as a user edit.
//
// THE CLOSURE. The decompiled body threads a compiler-generated display class through itself
// because four of its locals are captured -- by the LINQ predicates in the two selection buttons
// and by the parameter-name field, which is a captured method rather than a lambda. It is dissolved
// here: the four fields become locals and CompareServer becomes a local function, which is what the
// source must have been for the compiler to have emitted a display class with a method on it.
//
// THE THRESHOLD CONTROL NAMES. Each threshold field is given the control name "Threshold{n}" from a
// counter that is incremented here and reset elsewhere per frame. Only Float and Int rows take a
// number, so the numbering is over threshold fields rather than over rows, which is what
// ControllerEditor.ConditionSection.cs's up/down arrow handling walks.
//
// ParameterNameAt is a one-line helper of the same god class (decompiled 9774) with three other
// call sites in regions that remain unported; it is claimed here because this is the first ported
// caller. (Phrased that way deliberately: the wording this convention reserves for a file declaring
// its OWN incomplete region is read by port_status.py, and this file has no such region.)
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// Two, both the scattered inline `(Func<bool>)delegate { HMACSHA256 over the licence key, compared
// against licenseToken }` with `if (!that()) return;` in front of what follows, and both dropped on
// the package-wide basis recorded in ControllerEditor.TransitionSection.cs:
//
//   * decompiled 11528-11535, before the row draws anything at all. Under it an unlicensed row
//     would be blank -- the ReorderableList would still reserve its height and draw its background,
//     with no controls in it.
//   * decompiled 11712-11719, between the two layouts and the commit tail. Under it a row would
//     draw and respond normally and then discard every edit, including the remove button.
//
// Both are removed and the code they guarded is kept. Nothing else in decompiled 11510-11761 is
// licence-related.
//
// ============================== DELIBERATE DEVIATION ==========================================
//
// The dead `int num2 = -1;` at decompiled 11602 is dropped: the very next statement overwrites it
// with the EnumPopup result and nothing reads it in between.
//
// `(int)(AnimatorControllerParameterType)(object)EditorGUI.EnumPopup(...)` is written without the
// `(object)`, which is ILSpy's rendering of the unbox and not a conversion the source can have
// contained.
//
// Four inverted tests are written the way round the source must have had them, all ILSpy renderings
// of a branch-if-true: `(!num) ? Rect.zero : new Rect(source){...}`, `(!_Iterator) ? Rect.zero :
// targetRect`, `(type != Trigger) ? 50 : 100` and `(type != Bool) ? 50 : 100`. The mode-coercion
// block's `if (type <= Int || mode <= IfNot) { if (type != Float) { ... } else { ... } } else { ... }`
// is written as the equivalent three-armed chain; the arms and their guards are unchanged.
//
// The decompiled `EditorUtils.contents()` / `styles()` are read as properties, and
// `def2.SliceLeft(3f, isfield: true)` as `SliceLeft(3f, absolute: true)` -- the ported parameter
// name. Both are recorded in the EditorUtils files that own those members.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- diffed statement for statement against decompiled lines 11510-11761 and
// 6776-6846: both layouts in full, every rect slice with its width and absolute/percentage flag,
// all three MixedValueScope indices, the legacy-versus-advanced parameter dropdown pair with the
// reflected advancedPopupMethod's three boxed arguments, the mode dropdown's three typed enums, the
// threshold field's Float/Int split and its control-name counter, the mode-coercion chain, the
// three commit calls, and the remove button's post-conditions. ParameterNameAt was diffed against
// decompiled 9774-9777. Neither range contains a `goto`, a residual `switch` dispatch, a
// `while (true)` or an unresolved `smethod_N`, so no deobfuscator fault applies -- the two licence
// delegates above are the only things removed.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Condition row

        /// <summary>The controller parameter at <paramref name="index"/>, by name.</summary>
        private static string ParameterNameAt(int index)
        {
            return ActiveController.parameters[index].name;
        }

        /// <summary>
        /// Draws one condition of the condition editor and commits whatever the user changed to
        /// every transition that row stands for.
        /// </summary>
        /// <param name="rect">The row strip the list allotted.</param>
        /// <param name="index">The row's position in <see cref="CurrentConditionEditors"/>.</param>
        /// <param name="isActive">Unused, as shipped. Part of the element-callback signature.</param>
        /// <param name="isFocused">Unused, as shipped.</param>
        private static void DrawConditionRow(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!ActiveController)
            {
                return;
            }

            List<ConditionMultiEditor> editors = CurrentConditionEditors;

            // The list can outlive the set it was built around by a frame; drawing a row that is no
            // longer there would throw rather than merely look wrong.
            if (index >= editors.Count || index < 0)
            {
                return;
            }

            ConditionMultiEditor editor = editors[index];
            AnimatorCondition condition = editor.condition;
            AnimatorControllerParameter parameter = FindParameter(condition.parameter, out int parameterIndex);

            bool modeChanged = false;
            bool thresholdChanged = false;

            bool missing = parameter == null;
            string controlName = $"ConditionParameterField{index}";
            bool renaming = GUI.GetNameOfFocusedControl() == controlName;
            bool parameterChanged = false;

            // The shipped code carries this as a method on the row's closure. It is drawn on top of
            // whichever parameter control the layout below uses, and only while it has focus:
            // off-focus it is given Rect.zero, so the dropdown underneath shows through. A
            // right-click anywhere over that control is what gives it focus.
            void DrawParameterNameField(Rect targetRect)
            {
                Event fieldEvent = Event.current;
                if (renaming && fieldEvent.type == EventType.KeyDown && fieldEvent.keyCode == KeyCode.Escape)
                {
                    GUI.FocusControl(string.Empty);
                }

                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(controlName);
                condition.parameter = EditorGUI.DelayedTextField(renaming ? targetRect : Rect.zero, condition.parameter);
                parameterChanged = EditorGUI.EndChangeCheck();

                if (fieldEvent.type == EventType.MouseUp && fieldEvent.button == 1
                    && targetRect.Contains(fieldEvent.mousePosition))
                {
                    GUI.FocusControl(controlName);
                    fieldEvent.Use();
                }
            }

            Rect removeRect = new Rect(rect.width - 22f, rect.y + 2f, 32f, 18f);
            Rect addTypeRect = missing
                ? new Rect(removeRect) { width = 60f, x = removeRect.x - 60f }
                : Rect.zero;
            Rect fieldsRect = new Rect(rect.x, rect.y + 2f, rect.width - addTypeRect.width - 40f,
                EditorGUIUtility.singleLineHeight);

            if (missing)
            {
                Rect pickerRect = fieldsRect.SliceLeft(50f);
                Rect nameRect = new Rect(pickerRect)
                {
                    x = pickerRect.x + 3f,
                    width = pickerRect.width - 3f
                };

                fieldsRect.SliceLeft(3f, absolute: true);
                Rect noticeRect = fieldsRect.SliceLeft(100f);

                DrawParameterNameField(pickerRect);

                if (!renaming)
                {
                    using (new MixedValueScope(editor.mixedValues[0]))
                    {
                        EditorGUI.BeginChangeCheck();
                        int picked = EditorGUI.Popup(pickerRect, -1, parameterNames);
                        if (EditorGUI.EndChangeCheck())
                        {
                            condition.parameter = ParameterNameAt(picked);
                            parameterChanged = true;
                        }
                    }

                    // The dropdown is drawn with no selection, so the stored name is labelled over it.
                    GUI.Label(nameRect, condition.parameter);
                }

                GUI.Label(noticeRect, "Parameter not found in Controller!");

                EditorGUI.BeginChangeCheck();
                int addedType = (int)(AnimatorControllerParameterType)EditorGUI.EnumPopup(
                    addTypeRect, (AnimatorControllerParameterType)(-1));

                Rect addLabelRect = new Rect(addTypeRect) { x = addTypeRect.x + 3f };
                GUI.Label(addLabelRect, "Add");

                if (EditorGUI.EndChangeCheck())
                {
                    string added = string.IsNullOrEmpty(condition.parameter) ? "New Parameter" : condition.parameter;
                    ActiveController.AddParameter(added, (AnimatorControllerParameterType)addedType);

                    // Appended rather than re-read: the name cache is rebuilt elsewhere, and the
                    // dropdown has to show the new parameter on this frame.
                    ArrayUtility.Add(ref parameterNames, added);
                    condition.parameter = added;
                    parameterChanged = true;
                }
            }
            else
            {
                Rect pickRect = fieldsRect.SliceLeft(20f, absolute: true);
                Rect parameterRect = fieldsRect.SliceLeft(
                    parameter.type == AnimatorControllerParameterType.Trigger ? 100 : 50);
                Rect selectRect = new Rect(parameterRect)
                {
                    width = 20f,
                    x = parameterRect.x + parameterRect.width - 40f
                };
                Rect modeRect = fieldsRect.SliceLeft(
                    parameter.type == AnimatorControllerParameterType.Bool ? 100 : 50);
                Rect thresholdRect = fieldsRect.SliceLeft(100f);

                // Adds every transition carrying an equivalent condition to the selection. Matching
                // is strict here regardless of the user's matching options: this button means "this
                // condition", not "a condition I would group with this one".
                if (GUI.Button(pickRect, EditorUtils.contents.pickable, EditorUtils.styles.paddedBox))
                {
                    IEnumerable<IEnumerable<AnimatorStateTransition>> perState = ActiveStateMachine.states
                        .Select(s => s.state.transitions
                            .Where(t => t.conditions.Any(c => ConditionsMatch(condition, c, strict: true))));

                    List<AnimatorTransitionBase> matching = new List<AnimatorTransitionBase>();
                    perState.ForEach(delegate(IEnumerable<AnimatorStateTransition> transitions)
                    {
                        matching.AddRange(transitions);
                    });

                    matching.AddRange(ActiveStateMachine.anyStateTransitions
                        .Where(t => t.conditions.Any(c => ConditionsMatch(condition, c, strict: true))));
                    matching.AddRange(ActiveStateMachine.entryTransitions
                        .Where(t => t.conditions.Any(c => ConditionsMatch(condition, c, strict: true))));

                    Selection.objects = Selection.objects.Concat(matching).Distinct().ToArray();
                }

                // The same, by parameter name only: every transition that mentions this parameter
                // at all. Drawn as a bare invisible button, with its icon labelled over it below.
                if (GUI.Button(selectRect, GUIContent.none, GUIStyle.none))
                {
                    string parameterName = condition.parameter;

                    IEnumerable<IEnumerable<AnimatorStateTransition>> perState = ActiveStateMachine.states
                        .Select(s => s.state.transitions
                            .Where(t => t.conditions.Any(c => c.parameter == parameterName)));

                    List<AnimatorTransitionBase> matching = new List<AnimatorTransitionBase>();
                    perState.ForEach(delegate(IEnumerable<AnimatorStateTransition> transitions)
                    {
                        matching.AddRange(transitions);
                    });

                    matching.AddRange(ActiveStateMachine.anyStateTransitions
                        .Where(t => t.conditions.Any(c => c.parameter == parameterName)));
                    matching.AddRange(ActiveStateMachine.entryTransitions
                        .Where(t => t.conditions.Any(c => c.parameter == parameterName)));

                    Selection.objects = Selection.objects.Concat(matching).Distinct().ToArray();
                }

                DrawParameterNameField(parameterRect);

                if (!parameterChanged && !renaming)
                {
                    using (new MixedValueScope(editor.mixedValues[0]))
                    {
                        if ((bool)EditorSettings.Instance.useLegacyDropdown)
                        {
                            EditorGUI.BeginChangeCheck();
                            condition.parameter = ParameterNameAt(
                                EditorGUI.Popup(parameterRect, parameterIndex, parameterNames));
                            if (EditorGUI.EndChangeCheck())
                            {
                                parameterChanged = true;
                            }
                        }
                        else
                        {
                            // EditorGUI.AdvancedPopup is internal; the MethodInfo is primed by
                            // ControllerEditor.ReflectionPriming.cs. It reports the selection by
                            // return value rather than through a change check.
                            object[] arguments = { parameterRect, parameterIndex, parameterNames };
                            int picked = (int)advancedPopupMethod.Invoke(null, arguments);
                            if (picked != parameterIndex)
                            {
                                condition.parameter = ParameterNameAt(picked);
                                parameterChanged = true;
                            }
                        }
                    }
                }

                GUI.Label(selectRect, EditorUtils.contents.pickable, GUIStyle.none);

                if (parameter.type != AnimatorControllerParameterType.Trigger)
                {
                    using (new MixedValueScope(editor.mixedValues[1]))
                    {
                        EditorGUI.BeginChangeCheck();

                        Enum selected;
                        if (parameter.type == AnimatorControllerParameterType.Int)
                        {
                            selected = (IntModes)condition.mode;
                        }
                        else if (parameter.type == AnimatorControllerParameterType.Bool)
                        {
                            selected = (BoolModes)condition.mode;
                        }
                        else
                        {
                            selected = (FloatModes)condition.mode;
                        }

                        selected = EditorGUI.EnumPopup(modeRect, selected);

                        if (EditorGUI.EndChangeCheck())
                        {
                            condition.mode = (AnimatorConditionMode)selected;
                            modeChanged = true;
                        }
                    }

                    using (new MixedValueScope(editor.mixedValues[2]))
                    {
                        EditorGUI.BeginChangeCheck();

                        if (parameter.type == AnimatorControllerParameterType.Float)
                        {
                            GUI.SetNextControlName("Threshold" + thresholdControlCounter);
                            thresholdControlCounter++;
                            condition.threshold = EditorGUI.FloatField(thresholdRect, condition.threshold);
                        }
                        else if (parameter.type == AnimatorControllerParameterType.Int)
                        {
                            GUI.SetNextControlName("Threshold" + thresholdControlCounter);
                            thresholdControlCounter++;
                            condition.threshold = EditorGUI.IntField(thresholdRect, (int)condition.threshold);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            thresholdChanged = true;
                        }
                    }
                }

                // Coerces a mode the parameter's type cannot express, which is how a condition
                // survives its parameter being retyped under it.
                if (parameter.type > AnimatorControllerParameterType.Int
                    && condition.mode > AnimatorConditionMode.IfNot)
                {
                    condition.mode = AnimatorConditionMode.If;
                    modeChanged = true;
                }
                else if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    if (((int)condition.mode).IsOutside(3, 5))
                    {
                        condition.mode = AnimatorConditionMode.Greater;
                        modeChanged = true;
                    }
                }
                else if (parameter.type == AnimatorControllerParameterType.Int
                         && condition.mode < AnimatorConditionMode.Greater)
                {
                    condition.mode = AnimatorConditionMode.Equals;
                    modeChanged = true;
                }
            }

            // Nothing above has touched a transition; this is where the row's setters push each
            // changed field onto every target it stands for.
            if (parameterChanged)
            {
                editor.SetParameter(condition.parameter);
            }

            if (modeChanged)
            {
                editor.SetMode(condition.mode);
            }

            if (thresholdChanged)
            {
                editor.SetThreshold(condition.threshold);
            }

            if (!GUI.Button(removeRect, EditorUtils.contents.removeCondition, EditorUtils.styles.footerButton))
            {
                return;
            }

            editor.RemoveFromAll();

            // Removing shifts every later condition's index, so the row set has to be rebuilt --
            // except in the whole-selection case, where the shipped code drops the one row and
            // rebuilds only the list. Ported as shipped.
            if (focusedTransition.transition == null)
            {
                if (showSharedConditions)
                {
                    RefreshSharedConditions();
                }
                else
                {
                    allConditionEditors.RemoveAt(index);
                    RebuildConditionList();
                }
            }
        }

        #endregion
    }
}
