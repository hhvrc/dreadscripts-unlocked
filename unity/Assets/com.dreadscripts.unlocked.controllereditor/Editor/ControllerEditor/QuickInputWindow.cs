// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/QuickInputWindow.cs
//   static CreateHelper -> Create,        line 60
//   NewHelper           -> SetValue,      line 104
//   PushHelper          -> SetObjectType, line 116
//   ViewHelper          -> GetSize,       line 133
//   CollectHelper       -> ShowAt,        line 142
//   m_ParamPolicy       -> inRow
//   _ModelPolicy        -> values
//   m_TokenizerPolicy   -> labels
//   m_DecoratorPolicy   -> fieldTypes
//   comparatorPolicy    -> rowToggles
//   exceptionPolicy     -> onConfirm
//   m_ObjectPolicy      -> validate
//   m_UtilsPolicy       -> objectTypes
// Line numbers are relative to the port at the time it was written; the member names are the
// durable reference.
//
// Audit status: VERIFIED -- diffed in full against export/. The nested FieldType enum, all eight
// fields, the Title accessor and every method match statement for statement. Create's
// switch-in-a-`while (true)` is the decompiler's flattening of the plain default-value loop
// written here (each case's `goto IL_001e` is the loop increment), and the boxed int on the Float
// case is reproduced as shipped, as the body comment records. OnCustomGUI's inverted null guard,
// the row-toggle handling and the ToggleGroup change-check block all match.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A small throwaway form: a caller describes a handful of fields, the window draws them and
    /// hands the entered values back as an <c>object[]</c> when the user confirms.
    /// </summary>
    /// <remarks>
    /// The values are untyped on purpose — the window exists so that a one-off prompt ("which
    /// animator, and for how long?") does not need its own window class, so the caller supplies the
    /// field list at the call site and casts the results back itself.
    /// </remarks>
    internal class QuickInputWindow : UtilityWindowBase<QuickInputWindow>
    {
        /// <summary>The kinds of field the window knows how to draw.</summary>
        internal enum FieldType
        {
            Object,
            Integer,
            Float,
            String,
            Toggle,
            /// <summary>
            /// A toggle that is mutually exclusive with every other <see cref="ToggleGroup"/> field
            /// in the same window, and cannot be switched off by hand — only by turning another one
            /// on. It is the window's stand-in for a radio group.
            /// </summary>
            ToggleGroup
        }

        /// <summary>Whether a horizontal row opened by <see cref="rowToggles"/> is currently open.</summary>
        private bool inRow;

        /// <summary>The current value of each field, boxed. Index-parallel to <see cref="fieldTypes"/>.</summary>
        private object[] values;

        private GUIContent[] labels;

        private FieldType[] fieldTypes;

        /// <summary>
        /// Optional, index-parallel to <see cref="fieldTypes"/>. Every entry set to true flips the
        /// window in or out of a horizontal row, so a run of fields can be drawn side by side: the
        /// first true opens the row, the next closes it.
        /// </summary>
        internal bool[] rowToggles;

        private Action<object[]> onConfirm;

        /// <summary>
        /// Optional check run every frame, returning one flag per field: true means that field is
        /// wrong. Any flag set disables Confirm and marks the offending field with a warning icon.
        /// </summary>
        private Func<object[], bool[]> validate;

        /// <summary>
        /// The type an <see cref="FieldType.Object"/> field accepts, by field index. See
        /// <see cref="SetObjectType"/>.
        /// </summary>
        private readonly Dictionary<int, Type> objectTypes = new Dictionary<int, Type>();

        /// <summary>
        /// Blank: <see cref="Create"/> overwrites the title with the caller's own straight after the
        /// base class has applied this.
        /// </summary>
        internal override string Title => string.Empty;

        /// <summary>
        /// Builds a window for the given fields, with every value at its type's default. Not shown
        /// yet — set any starting values, then call <see cref="ShowAt"/>.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="fieldTypes">One entry per field, in draw order.</param>
        /// <param name="labels">Field labels, index-parallel to <paramref name="fieldTypes"/>.</param>
        /// <param name="onConfirm">Receives the entered values when the user confirms.</param>
        /// <param name="validate">Optional per-field validity check; see <see cref="validate"/>.</param>
        internal static QuickInputWindow Create(string title, FieldType[] fieldTypes, GUIContent[] labels, Action<object[]> onConfirm, Func<object[], bool[]> validate = null)
        {
            object[] values = new object[fieldTypes.Length];
            for (int i = 0; i < fieldTypes.Length; i++)
            {
                switch (fieldTypes[i])
                {
                    case FieldType.ToggleGroup:
                        values[i] = false;
                        break;
                    case FieldType.Integer:
                        values[i] = 0;
                        break;
                    case FieldType.Toggle:
                        values[i] = false;
                        break;
                    case FieldType.String:
                        values[i] = "";
                        break;
                    case FieldType.Float:
                        // Boxes an int, not a float, exactly as the shipped build did. Drawing such
                        // a field unboxes it as a float and throws, so a Float field only works if
                        // the caller overwrites its value with SetValue first. Ported as-is rather
                        // than corrected to 0f: no shipped call site uses a Float field, so the
                        // faithful reading cannot be checked against observed behaviour.
                        values[i] = 0;
                        break;
                    case FieldType.Object:
                        values[i] = null;
                        break;
                }
            }

            // Qualified: the overload declared here hides the base factory from the unqualified name.
            QuickInputWindow window = UtilityWindowBase<QuickInputWindow>.Create();
            window.titleContent.text = title;
            window.values = values;
            window.fieldTypes = fieldTypes;
            window.labels = labels;
            window.onConfirm = onConfirm;
            window.validate = validate;
            return window;
        }

        /// <summary>Sets the starting value of a field.</summary>
        internal void SetValue(int index, object value)
        {
            values[index] = value;
        }

        /// <summary>
        /// Restricts an <see cref="FieldType.Object"/> field to a type. Required for any object field
        /// that starts empty, since an unrestricted field infers its type from the current value and
        /// there is none to infer from.
        /// </summary>
        internal void SetObjectType(int index, Type type)
        {
            if (objectTypes.ContainsKey(index))
            {
                Debug.LogWarning($"{index} is already set as {type.Name}");
            }
            else
            {
                objectTypes.Add(index, type);
            }
        }

        /// <summary>
        /// The window size that fits the fields: one standard row each, plus the Confirm button and
        /// the help box when there is one.
        /// </summary>
        internal Vector2 GetSize()
        {
            return new Vector2(370f, 26 * fieldTypes.Length + 28 + (!string.IsNullOrEmpty(helpMessage) ? 38 : 0));
        }

        /// <summary>Shows the window at a screen position, sized to its contents.</summary>
        internal void ShowAt(Vector2 screenPosition)
        {
            base.ShowAt(screenPosition, GetSize());
        }

        internal override void OnCustomGUI()
        {
            // Unity revives editor windows across a domain reload, but the field list is not
            // serialized, so a revived window has nothing to draw and closes itself.
            if (values == null)
            {
                Close();
                return;
            }

            bool[] fieldHasError = validate?.Invoke(values);
            canConfirm = fieldHasError == null || !fieldHasError.Any(hasError => hasError);

            bool hasRowToggles = rowToggles != null;
            for (int i = 0; i < fieldTypes.Length; i++)
            {
                if (hasRowToggles && rowToggles[i])
                {
                    inRow = !inRow;
                    if (inRow)
                    {
                        EditorGUILayout.BeginHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();
                    }
                }

                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    switch (fieldTypes[i])
                    {
                        case FieldType.ToggleGroup:
                            EditorGUI.BeginChangeCheck();
                            values[i] = EditorGUILayout.Toggle(labels[i], (bool)values[i]);
                            if (!EditorGUI.EndChangeCheck())
                            {
                                break;
                            }

                            if ((bool)values[i])
                            {
                                // Turning one on turns the rest of the group off.
                                for (int other = 0; other < fieldTypes.Length; other++)
                                {
                                    if (fieldTypes[other] == FieldType.ToggleGroup && other != i)
                                    {
                                        values[other] = false;
                                    }
                                }
                            }
                            else
                            {
                                // Refuses the click: one member of the group is always on, so the
                                // only way out of a choice is into another one.
                                values[i] = true;
                            }

                            break;

                        case FieldType.Float:
                            values[i] = EditorGUILayout.FloatField(labels[i], (float)values[i]);
                            break;

                        case FieldType.Toggle:
                            values[i] = EditorGUILayout.Toggle(labels[i], (bool)values[i]);
                            break;

                        case FieldType.Object:
                            values[i] = EditorGUILayout.ObjectField(labels[i], (UnityEngine.Object)values[i],
                                objectTypes.ContainsKey(i) ? objectTypes[i] : values[i].GetType(), true);
                            break;

                        case FieldType.Integer:
                            values[i] = EditorGUILayout.IntField(labels[i], (int)values[i]);
                            break;

                        case FieldType.String:
                            values[i] = EditorGUILayout.TextField(labels[i], (string)values[i]);
                            break;
                    }

                    if (!canConfirm && fieldHasError[i])
                    {
                        GUILayout.Label(new GUIContent(EditorUtils.contents.warning), EditorUtils.styles.centeredIcon, GUILayout.ExpandWidth(false));
                    }
                }
            }

            // A row left open by an odd number of toggles is closed here, so the layout stack stays
            // balanced whatever the caller passed.
            if (inRow)
            {
                inRow = false;
                EditorGUILayout.EndHorizontal();
            }
        }

        internal override void OnCustomConfirm()
        {
            onConfirm(values);
        }
    }
}
