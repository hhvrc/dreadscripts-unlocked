// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static FlushRules     -> DrawObjectListField<T>,  line 4932
//   static CalculateRules -> AddToArray,              line 4979
//   static MapRules       -> RemoveFromArray,         line 4998
//   static ValidateRules  -> SetInArray(params),      line 5018
//   static CustomizeRules -> SetInArray,              line 5023
//   static RateRules      -> FindLastArrayIndex,      line 5035
//   static DestroyRules   -> ForEachTargetProperty,   line 5054
//   static StopRules      -> ToContent,               line 5317
//   static PrepareRules   -> GetValue,                line 5327
//   static AssetRules     -> SetValue,                line 5388
//   class <>c__DisplayClass210_1<T>/<>c__DisplayClass212_1<T> -> dissolved into the lambdas in
//                            AddToArray/RemoveFromArray, lines 1986/2004
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// MULTI-OBJECT EDITING is what most of this file is for. A SerializedProperty taken from a
// multi-object SerializedObject cannot have its array edited: Unity refuses, because it has no way
// to say what "element 3" means across targets of different lengths. ForEachTargetProperty works
// round that by re-resolving the same property path against each target through its own
// SerializedObject and editing each one separately -- so AddToArray and RemoveFromArray work on a
// multi-selection where a plain PropertyField would not.
//
// GetValue/SetValue are the untyped accessors SerializedProperty never shipped. Both warn and
// no-op on the property types with no single value to read or write -- Generic, Gradient and
// ManagedReference, plus FixedBufferSize on the set side, which is genuinely read-only. Note
// Enum is handled as its *index*, not its value: enumValueIndex is a position in
// enumDisplayNames, so round-tripping through these two is safe but comparing the result against a
// cast enum value is not.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Draws an object-reference array as a list of removable rows plus a drop area, and keeps
        /// it free of null entries.
        /// </summary>
        /// <remarks>
        /// Null elements are deleted as they are encountered rather than skipped, which is why the
        /// loop index steps back after a deletion. That also means simply clearing a row in the
        /// inspector removes it on the next repaint.
        /// <para>
        /// A multi-object selection with differing lists is not editable: the rows are replaced by
        /// a note and neither the drop area nor the picker is wired up.
        /// </para>
        /// </remarks>
        internal static void DrawObjectListField<T>(SerializedProperty property) where T : UnityEngine.Object
        {
            bool mixed = property.hasMultipleDifferentValues;
            if (!mixed)
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i);
                    if (element == null)
                    {
                        continue;
                    }

                    if (element.objectReferenceValue == null)
                    {
                        property.DeleteArrayElementAtIndex(i);
                        i--;
                        continue;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(element, GUIContent.none);
                        if (Button(contents.removeSelection, styles.iconButton))
                        {
                            property.DeleteArrayElementAtIndex(i);
                        }
                    }
                }
            }

            Rect dropArea = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            GUIContent label = mixed
                ? new GUIContent("Editing Multiple Lists",
                    "Editing multiple lists with different values is not supported.")
                : new GUIContent("[Drag And Drop Or Click Here]");
            GUI.Label(dropArea, label, styles.noteCenter);

            if (mixed)
            {
                return;
            }

            HandleMultiDragAndDrop<T>(dropArea, property.AddToArray);
            if (ClickArea(dropArea))
            {
                ShowObjectPicker(null, typeof(T), allowSceneObjects: true,
                    onSelectorClosed: delegate(UnityEngine.Object picked)
                    {
                        property.AddToArray(new[] { picked.AsComponentOrAsset<T>() });
                    });
            }
        }

        /// <summary>
        /// Appends each element of <paramref name="values"/> to the array property, skipping any
        /// already present. Applies the change immediately.
        /// </summary>
        internal static void AddToArray<T>(this SerializedProperty property, IEnumerable<T> values)
            where T : UnityEngine.Object
        {
            T[] items = (values as T[]) ?? values.ToArray();
            property.ForEachTargetProperty(sp =>
            {
                foreach (T item in items)
                {
                    if (sp.FindLastArrayIndex((e, _) => e.objectReferenceValue == item) < 0)
                    {
                        sp.GetArrayElementAtIndex(++sp.arraySize - 1).objectReferenceValue = item;
                    }
                }

                sp.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Removes each element of <paramref name="values"/> from the array property, ignoring any
        /// not present. Applies the change immediately.
        /// </summary>
        internal static void RemoveFromArray<T>(this SerializedProperty property, IEnumerable<T> values)
            where T : UnityEngine.Object
        {
            T[] items = (values as T[]) ?? values.ToArray();
            property.ForEachTargetProperty(sp =>
            {
                foreach (T item in items)
                {
                    int index = sp.FindLastArrayIndex((e, _) => e.objectReferenceValue == item);
                    if (index >= 0)
                    {
                        sp.DeleteArrayElementAtIndex(index);
                    }
                }

                sp.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Adds or removes <paramref name="elements"/> depending on <paramref name="present"/> --
        /// the toggle form, for a checkbox that decides membership.
        /// </summary>
        internal static void SetInArray<T>(this SerializedProperty property, bool present, params T[] elements)
            where T : UnityEngine.Object
        {
            property.SetInArray(elements, present);
        }

        /// <summary>
        /// Adds or removes <paramref name="values"/> depending on <paramref name="present"/>.
        /// </summary>
        internal static void SetInArray<T>(this SerializedProperty property, IEnumerable<T> values, bool present)
            where T : UnityEngine.Object
        {
            if (present)
            {
                property.AddToArray(values);
            }
            else
            {
                property.RemoveFromArray(values);
            }
        }

        /// <summary>
        /// The index of the last element satisfying <paramref name="predicate"/>, or -1. The
        /// predicate gets the element property and its index.
        /// </summary>
        /// <remarks>
        /// Searches from the end, which is what makes it safe to delete the result inside a loop:
        /// removing a late element cannot shift an earlier match.
        /// </remarks>
        internal static int FindLastArrayIndex(this SerializedProperty property,
            Func<SerializedProperty, int, bool> predicate)
        {
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                if (predicate(property.GetArrayElementAtIndex(i), i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Runs <paramref name="action"/> against the property itself when it holds a single value,
        /// or against one freshly resolved property per target when it does not.
        /// </summary>
        /// <remarks>
        /// The per-target properties come from new SerializedObjects, so changes made to them do
        /// not appear on the original until it is refreshed -- each callback is expected to apply
        /// its own.
        /// </remarks>
        internal static void ForEachTargetProperty(this SerializedProperty property,
            Action<SerializedProperty> action)
        {
            if (!property.hasMultipleDifferentValues)
            {
                action(property);
                return;
            }

            string propertyPath = property.propertyPath;
            foreach (UnityEngine.Object target in property.serializedObject.targetObjects)
            {
                action(new SerializedObject(target).FindProperty(propertyPath));
            }
        }

        /// <summary>
        /// The property's inspector label and tooltip as a GUIContent, for drawing it by hand.
        /// </summary>
        internal static GUIContent ToContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        /// <summary>
        /// The property's value, boxed. Null -- with a console warning -- for the property types
        /// that have no single value to read.
        /// </summary>
        internal static object GetValue(this SerializedProperty property)
        {
            SerializedPropertyType propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ManagedReference:
                    Debug.LogWarning("Property type " + propertyType + " does not support get value.");
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Writes <paramref name="value"/> into the property, unboxing it to whatever the property
        /// type needs. A mismatched type throws an InvalidCastException; the types with no single
        /// value to write warn and do nothing.
        /// </summary>
        internal static void SetValue(this SerializedProperty property, object value)
        {
            SerializedPropertyType propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int)value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = (int)value;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = (char)value;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (AnimationCurve)value;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds)value;
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion)value;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = (Vector2Int)value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = (Vector3Int)value;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = (RectInt)value;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = (BoundsInt)value;
                    break;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.ManagedReference:
                    Debug.LogWarning("Property type " + propertyType + " does not support set value.");
                    break;
            }
        }
    }
}
