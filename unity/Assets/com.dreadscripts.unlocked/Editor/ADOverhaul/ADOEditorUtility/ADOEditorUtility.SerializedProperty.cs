// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static CalcStatus<T>    -> AddElements<T>,          line 2642
//   static DefineStatus<T>  -> RemoveElements<T>,       line 2663
//   static DestroyStatus<T> -> SetElementsPresent<T>(SerializedProperty, bool, params T[]), line 2683
//   static NewStatus<T>     -> SetElementsPresent<T>(SerializedProperty, IEnumerable<T>, bool), line 2688
//   static CompareStatus    -> FindLastElementIndex,    line 2700
//   static VerifyStatus     -> ForEachEditedTarget,     line 2716
//   static SelectStatus     -> ToGUIContent,            line 2884
//   static WriteStatus      -> GetValue,                line 2889
//   static MoveStatus       -> SetValue,                line 2950
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DEFERRED (not ported here — a later pass should pick these up):
//   static OrderStatus<T>, line 2595 — the object-reference array list drawer (inline rows plus a
//     drag-and-drop / click-to-pick footer). It needs five helpers that are not in the package yet:
//     RevertProcess<T> (line 2512, the drag-and-drop accept area), ReadStatus (line 3187, the
//     click-detection helper), IncludeStatus (line 3701, the object picker window), CallStatus
//     (line 3145, the icon button) and CustomizeStatus<T> (line 2766, the Object-to-T coercion).
//     Stubbing any of them would change behaviour, so the drawer waits.
//
// Obfuscator scaffolding deliberately NOT ported: the four compiler-generated closure classes that
// the decompiler emitted for the two element families (_003C_003Ec__DisplayClass24_0/24_1 at lines
// 1687/1749 and _003C_003Ec__DisplayClass26_0/26_1 at lines 1777/1822) are captured-variable
// artifacts, restored to plain lambdas below. Each of them also carried an always-null private
// static `object` field paired with a `... == null` predicate (SetupState, ViewState, SortState,
// FindState) and a set of one-line forwarders around ordinary SerializedProperty members
// (VerifyIterator -> arraySize, SortIterator -> GetArrayElementAtIndex, ...). Those are pure
// obfuscator padding and are inlined or dropped.
//
// 2019 vs 2022: no behavioural divergence. Every member here is statement-for-statement identical
// between the two builds (2019 names: DeleteManager, SelectManager, RunManager, StopManager,
// WriteManager, DefineManager, CompareManager, InterruptManager, and the setter at line 2963); only
// the obfuscated names and the order the decompiler happened to emit the switch cases in differ.
// The set of handled SerializedPropertyType cases is identical in both builds.
//
// Note: ControllerEditor's EditorUtils carries its own copy of this same family (decompiled/
// ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs, lines ~4963-5400). It is a
// separate product namespace and is left to that type's own port rather than shared from here.

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
        /// Appends each of <paramref name="elements"/> to an object-reference array property,
        /// skipping any element the array already holds, and applies the result.
        /// </summary>
        /// <remarks>
        /// Applied per edited target: see <see cref="ForEachEditedTarget"/>. Membership is decided
        /// with Unity's <c>Object</c> equality, so a destroyed object counts as already present
        /// wherever the array holds a null slot.
        /// </remarks>
        internal static void AddElements<T>(this SerializedProperty property, IEnumerable<T> elements) where T : UnityEngine.Object
        {
            T[] toAdd = (elements as T[]) ?? elements.ToArray();
            property.ForEachEditedTarget(delegate (SerializedProperty sp)
            {
                foreach (T element in toAdd)
                {
                    if (sp.FindLastElementIndex((e, i) => e.objectReferenceValue == element) < 0)
                    {
                        int newSize = sp.arraySize + 1;
                        sp.arraySize = newSize;
                        sp.GetArrayElementAtIndex(newSize - 1).objectReferenceValue = element;
                    }
                }
                sp.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Removes each of <paramref name="elements"/> from an object-reference array property and
        /// applies the result.
        /// </summary>
        /// <remarks>
        /// Only the last occurrence of each element is looked up per element, so an array holding
        /// the same reference twice keeps one copy — matching the shipped behaviour.
        /// <para>
        /// Unity's <c>DeleteArrayElementAtIndex</c> on an array of object references nulls the slot
        /// rather than shrinking the array when the slot is non-null, so the entries this leaves
        /// behind are null holes rather than removals. The list drawer that consumes these arrays
        /// compensates by deleting null entries as it walks them.
        /// </para>
        /// </remarks>
        internal static void RemoveElements<T>(this SerializedProperty property, IEnumerable<T> elements) where T : UnityEngine.Object
        {
            T[] toRemove = (elements as T[]) ?? elements.ToArray();
            property.ForEachEditedTarget(delegate (SerializedProperty sp)
            {
                foreach (T element in toRemove)
                {
                    int index = sp.FindLastElementIndex((e, i) => e.objectReferenceValue == element);
                    if (index >= 0)
                    {
                        sp.DeleteArrayElementAtIndex(index);
                    }
                }
                sp.serializedObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Adds or removes <paramref name="elements"/> depending on <paramref name="present"/>,
        /// so a toggle can drive array membership with a single call.
        /// </summary>
        internal static void SetElementsPresent<T>(this SerializedProperty property, IEnumerable<T> elements, bool present) where T : UnityEngine.Object
        {
            if (!present)
            {
                property.RemoveElements(elements);
            }
            else
            {
                property.AddElements(elements);
            }
        }

        /// <inheritdoc cref="SetElementsPresent{T}(SerializedProperty, IEnumerable{T}, bool)"/>
        internal static void SetElementsPresent<T>(this SerializedProperty property, bool present, params T[] elements) where T : UnityEngine.Object
        {
            property.SetElementsPresent(elements, present);
        }

        /// <summary>
        /// Returns the index of the last array element satisfying <paramref name="predicate"/>, or
        /// -1 if none does. The predicate receives the element and its index.
        /// </summary>
        /// <remarks>
        /// The search runs back-to-front, which is what makes repeated remove-then-search passes
        /// stable while earlier indices are still being mutated.
        /// </remarks>
        internal static int FindLastElementIndex(this SerializedProperty property, Func<SerializedProperty, int, bool> predicate)
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
        /// Runs <paramref name="action"/> against the property once, or — when the inspector is
        /// multi-editing and the targets disagree — once per target against a fresh
        /// <see cref="SerializedObject"/> resolved from the same property path.
        /// </summary>
        /// <remarks>
        /// A multi-edit <see cref="SerializedProperty"/> cannot express per-target array contents,
        /// so structural edits have to be re-issued object by object. Note the condition is
        /// <c>hasMultipleDifferentValues</c>, not the target count: when several objects are
        /// selected but already agree, the shared property is edited directly and Unity propagates
        /// the change to all of them.
        /// </remarks>
        internal static void ForEachEditedTarget(this SerializedProperty property, Action<SerializedProperty> action)
        {
            if (!property.hasMultipleDifferentValues)
            {
                action(property);
                return;
            }

            string propertyPath = property.propertyPath;
            UnityEngine.Object[] targetObjects = property.serializedObject.targetObjects;
            foreach (UnityEngine.Object target in targetObjects)
            {
                action(new SerializedObject(target).FindProperty(propertyPath));
            }
        }

        /// <summary>
        /// The label a property would draw with: its display name plus its tooltip attribute.
        /// </summary>
        internal static GUIContent ToGUIContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        /// <summary>
        /// Reads a property's value as a boxed <see cref="object"/>, dispatching on its
        /// <see cref="SerializedPropertyType"/>.
        /// </summary>
        /// <remarks>
        /// Types with no single-value representation (<c>Generic</c>, <c>Gradient</c>,
        /// <c>ManagedReference</c>) warn and return null. Any type not listed at all — notably
        /// <c>Hash128</c> — also returns null, but silently: the shipped switch has a bare
        /// <c>default</c> arm with no warning, so an unsupported property looks identical to a
        /// genuine null reference at the call site. Preserved as shipped.
        /// <para>
        /// <c>Enum</c> yields the enum *index*, not the underlying value, and <c>LayerMask</c>
        /// yields a plain <see cref="int"/> rather than a <see cref="LayerMask"/>; both round-trip
        /// correctly through <see cref="SetValue"/>.
        /// </para>
        /// </remarks>
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
                    Debug.LogWarning("Property type " + propertyType.ToString() + " does not support get value.");
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Writes a boxed value back into a property, dispatching on its
        /// <see cref="SerializedPropertyType"/>. The mirror of <see cref="GetValue"/>.
        /// </summary>
        /// <remarks>
        /// The value is unboxed with a hard cast, so it must be exactly the type
        /// <see cref="GetValue"/> would have produced for the same property — passing a boxed
        /// <see cref="int"/> to a <c>Character</c> property throws, because that arm unboxes to
        /// <see cref="char"/> first.
        /// <para>
        /// <c>Generic</c>, <c>Gradient</c>, <c>ManagedReference</c> and the read-only
        /// <c>FixedBufferSize</c> warn and do nothing. As in <see cref="GetValue"/>, any type not
        /// listed — <c>Hash128</c> — is silently ignored with no warning at all. Preserved as
        /// shipped.
        /// </para>
        /// </remarks>
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
                    Debug.LogWarning("Property type " + propertyType.ToString() + " does not support set value.");
                    break;
            }
        }
    }
}
