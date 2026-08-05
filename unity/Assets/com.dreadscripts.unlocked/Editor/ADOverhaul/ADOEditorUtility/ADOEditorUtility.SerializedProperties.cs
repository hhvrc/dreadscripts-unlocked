// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static SelectStatus  -> GetContent,      line 2884
//   static WriteStatus   -> GetValue,        line 2889
//   static MoveStatus    -> SetValue,        line 2950
//   static VerifyStatus  -> ForEachTarget,   line 2716
//   static CompareStatus -> FindLastIndex,   line 2700
//   static CalcStatus    -> AddToArray,      line 2642
//   static DefineStatus  -> RemoveFromArray, line 2663
//   static NewStatus     -> SetInArray(SerializedProperty, IEnumerable<T>, bool), line 2688
//   static DestroyStatus -> SetInArray(SerializedProperty, bool, params T[]),     line 2683
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above, including the four closure classes named next.
//
// Four compiler-generated closure classes belong to this region and get no file of their own:
// _003C_003Ec__DisplayClass24_0<T> / _003C_003Ec__DisplayClass24_1<T> (lines 1686 and 1748) carry
// AddToArray's per-target body and its per-element comparison, and
// _003C_003Ec__DisplayClass26_0<T> / _003C_003Ec__DisplayClass26_1<T> (lines 1776 and 1821) do the
// same for RemoveFromArray. Between them they carry eleven single-statement static proxies
// (VerifyIterator, SetIterator, SortIterator, InvokeIterator, CustomizeIterator, ConcatIterator,
// FillIterator, CancelIterator, SetupIterator, SelectIterator, WriteIterator, PublishIterator,
// CollectIterator) which are nothing but SerializedProperty.arraySize / GetArrayElementAtIndex /
// objectReferenceValue / DeleteArrayElementAtIndex / serializedObject / ApplyModifiedProperties and
// UnityEngine.Object's == operator; all are inlined back below. Each class also carries a static
// object field paired with a "field == null" predicate (ChangeState/SetupState, PopState/ViewState,
// PushState/SortState, CloneState/FindState) that nothing assigns and nothing reads -- protector
// tamper-bait, dropped.
//
// The list drawer that also lived in this region (OrderStatus, line 2595) is in
// ADOEditorUtility.ObjectListField.cs, because it is a control rather than a property accessor.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The label and tooltip Unity would draw for <paramref name="property"/>, as a
        /// <see cref="GUIContent"/>.
        /// </summary>
        /// <remarks>
        /// For drawing a property with a custom control while keeping the inspector label and the
        /// tooltip from its <c>[Tooltip]</c> attribute.
        /// </remarks>
        internal static GUIContent GetContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        /// <summary>
        /// Reads <paramref name="property"/> through whichever typed accessor matches its
        /// <see cref="SerializedProperty.propertyType"/>, boxed.
        /// </summary>
        /// <returns>
        /// The boxed value, or null for a type that has no single-value accessor -- Generic,
        /// Gradient and ManagedReference, which log a warning, and any type this build predates,
        /// which returns null silently.
        /// </returns>
        internal static object GetValue(this SerializedProperty property)
        {
            SerializedPropertyType propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Integer: return property.intValue;
                case SerializedPropertyType.Boolean: return property.boolValue;
                case SerializedPropertyType.Float: return property.floatValue;
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.Color: return property.colorValue;
                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask: return property.intValue;
                case SerializedPropertyType.Enum: return property.enumValueIndex;
                case SerializedPropertyType.Vector2: return property.vector2Value;
                case SerializedPropertyType.Vector3: return property.vector3Value;
                case SerializedPropertyType.Vector4: return property.vector4Value;
                case SerializedPropertyType.Rect: return property.rectValue;
                case SerializedPropertyType.ArraySize: return property.arraySize;
                case SerializedPropertyType.Character: return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue;
                case SerializedPropertyType.Bounds: return property.boundsValue;
                case SerializedPropertyType.Quaternion: return property.quaternionValue;
                case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int: return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int: return property.vector3IntValue;
                case SerializedPropertyType.RectInt: return property.rectIntValue;
                case SerializedPropertyType.BoundsInt: return property.boundsIntValue;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ManagedReference:
                    Debug.LogWarning("Property type " + propertyType.ToString() + " does not support get value.");
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Writes <paramref name="value"/> into <paramref name="property"/> through whichever typed
        /// accessor matches its <see cref="SerializedProperty.propertyType"/>.
        /// </summary>
        /// <remarks>
        /// The inverse of <see cref="GetValue"/>, with one asymmetry that is in the shipped build:
        /// FixedBufferSize is a read-only size here and is grouped with the unsupported types, while
        /// <see cref="GetValue"/> reads it. An unboxing cast of the wrong type throws -- this is a
        /// round-trip helper, not a converter. Nothing is applied; the caller owns
        /// <see cref="SerializedObject.ApplyModifiedProperties"/>.
        /// </remarks>
        internal static void SetValue(this SerializedProperty property, object value)
        {
            SerializedPropertyType propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Integer: property.intValue = (int)value; break;
                case SerializedPropertyType.Boolean: property.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: property.floatValue = (float)value; break;
                case SerializedPropertyType.String: property.stringValue = (string)value; break;
                case SerializedPropertyType.Color: property.colorValue = (Color)value; break;
                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.LayerMask: property.intValue = (int)value; break;
                case SerializedPropertyType.Enum: property.enumValueIndex = (int)value; break;
                case SerializedPropertyType.Vector2: property.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3: property.vector3Value = (Vector3)value; break;
                case SerializedPropertyType.Vector4: property.vector4Value = (Vector4)value; break;
                case SerializedPropertyType.Rect: property.rectValue = (Rect)value; break;
                case SerializedPropertyType.ArraySize: property.arraySize = (int)value; break;
                case SerializedPropertyType.Character: property.intValue = (char)value; break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = (AnimationCurve)value; break;
                case SerializedPropertyType.Bounds: property.boundsValue = (Bounds)value; break;
                case SerializedPropertyType.Quaternion: property.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.Vector2Int: property.vector2IntValue = (Vector2Int)value; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = (Vector3Int)value; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = (RectInt)value; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = (BoundsInt)value; break;

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.ManagedReference:
                    Debug.LogWarning("Property type " + propertyType.ToString() + " does not support set value.");
                    break;
            }
        }

        /// <summary>
        /// Runs <paramref name="action"/> against <paramref name="property"/>, or -- when the
        /// selection disagrees about its value -- against the same property on each target
        /// separately.
        /// </summary>
        /// <remarks>
        /// A multi-object <see cref="SerializedProperty"/> cannot express per-target array edits: it
        /// reports <c>hasMultipleDifferentValues</c> and refuses to enumerate. Re-resolving the same
        /// property path against a fresh single-target <see cref="SerializedObject"/> per target
        /// sidesteps that, at the cost of one SerializedObject per target.
        /// </remarks>
        internal static void ForEachTarget(this SerializedProperty property, Action<SerializedProperty> action)
        {
            if (!property.hasMultipleDifferentValues)
            {
                action(property);
                return;
            }

            string propertyPath = property.propertyPath;
            UnityEngine.Object[] targets = property.serializedObject.targetObjects;
            for (int i = 0; i < targets.Length; i++)
            {
                action(new SerializedObject(targets[i]).FindProperty(propertyPath));
            }
        }

        /// <summary>
        /// The index of the last element of the array <paramref name="property"/> for which
        /// <paramref name="match"/> holds, or -1.
        /// </summary>
        /// <param name="match">Given the element and its index.</param>
        /// <remarks>
        /// Searches backwards, which is what makes it safe to delete the returned index and search
        /// again without the remaining indices shifting under the caller.
        /// </remarks>
        internal static int FindLastIndex(this SerializedProperty property, Func<SerializedProperty, int, bool> match)
        {
            for (int i = property.arraySize - 1; i >= 0; i--)
            {
                if (match(property.GetArrayElementAtIndex(i), i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Appends every element of <paramref name="values"/> that the array
        /// <paramref name="property"/> does not already hold, and applies the change.
        /// </summary>
        /// <remarks>
        /// Applied per target through <see cref="ForEachTarget"/>, so a multi-object edit adds the
        /// values to each object's own list rather than overwriting all of them with one list.
        /// </remarks>
        internal static void AddToArray<T>(this SerializedProperty property, IEnumerable<T> values) where T : UnityEngine.Object
        {
            // Materialised once, outside the per-target callback, so a lazy source is not enumerated
            // repeatedly.
            T[] elements = (values as T[]) ?? values.ToArray();

            property.ForEachTarget(delegate(SerializedProperty target)
            {
                foreach (T element in elements)
                {
                    if (target.FindLastIndex((SerializedProperty existing, int index) => existing.objectReferenceValue == element) < 0)
                    {
                        int newSize = target.arraySize + 1;
                        target.arraySize = newSize;
                        target.GetArrayElementAtIndex(newSize - 1).objectReferenceValue = element;
                    }
                }

                target.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Removes every element of <paramref name="values"/> from the array
        /// <paramref name="property"/>, and applies the change.
        /// </summary>
        /// <remarks>
        /// Only the last occurrence of each value is removed, because the search finds one index per
        /// value and does not loop. A list holding the same object twice therefore keeps one copy.
        /// That is the shipped behaviour; <see cref="AddToArray"/> makes duplicates unlikely in the
        /// first place.
        /// </remarks>
        internal static void RemoveFromArray<T>(this SerializedProperty property, IEnumerable<T> values) where T : UnityEngine.Object
        {
            T[] elements = (values as T[]) ?? values.ToArray();

            property.ForEachTarget(delegate(SerializedProperty target)
            {
                foreach (T element in elements)
                {
                    int index = target.FindLastIndex((SerializedProperty existing, int i) => existing.objectReferenceValue == element);
                    if (index >= 0)
                    {
                        target.DeleteArrayElementAtIndex(index);
                    }
                }

                target.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Adds or removes <paramref name="values"/> from the array <paramref name="property"/>
        /// depending on <paramref name="shouldContain"/>.
        /// </summary>
        /// <remarks>
        /// For a toggle whose new state the caller already knows: one call handles both directions
        /// instead of branching at every call site.
        /// </remarks>
        internal static void SetInArray<T>(this SerializedProperty property, IEnumerable<T> values, bool shouldContain) where T : UnityEngine.Object
        {
            if (shouldContain)
            {
                property.AddToArray(values);
            }
            else
            {
                property.RemoveFromArray(values);
            }
        }

        /// <inheritdoc cref="SetInArray{T}(SerializedProperty, IEnumerable{T}, bool)"/>
        internal static void SetInArray<T>(this SerializedProperty property, bool shouldContain, params T[] values) where T : UnityEngine.Object
        {
            property.SetInArray(values, shouldContain);
        }
    }
}
