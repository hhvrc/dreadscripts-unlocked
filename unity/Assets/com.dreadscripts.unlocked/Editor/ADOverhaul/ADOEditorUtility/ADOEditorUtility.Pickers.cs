// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static IncludeStatus  -> ShowObjectPicker,          line 3701
//   static OrderStatus<T> -> DrawObjectReferenceList<T>, line 2595
//   static fields helperSerializer / candidateSerializer / readerSerializer (2088-2092)
//                         -> ObjectSelectorRefs, a private nested cache
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// Both members were deferred by earlier passes -- ShowObjectPicker by ADOEditorUtility.Reflection.cs
// (which owns the surrounding reflection helpers but is not a GUI file) and the list drawer by
// ADOEditorUtility.SerializedProperty.cs (which was missing five dependencies). All five are now in
// place: HandleMultiDragAndDrop, ClickArea, Button(GUIContent, GUIStyle, ...), AddElements and the
// CoerceTo<T> coercion in ADOEditorUtility.Rects.cs.
//
// ── Relationship to EditorUtils.ShowObjectPicker ────────────────────────────────────────────────
//
// ControllerEditor ships the same picker (decompiled ConcatList, line 6690, ported as
// EditorUtils.ShowObjectPicker in Editor/ControllerEditor/EditorUtils/EditorUtils.Pickers.cs). The
// two copies are NOT identical, so this one is ported separately rather than shared:
//
//   * ControllerEditor looks the fallback `Show` overload up with Instance | NonPublic. ADOverhaul
//     looks it up with Static | Public. See the SHIPPED BUG note on ObjectSelectorRefs below.
//   * ControllerEditor resolves each handle through the package's TypeResolver /
//     ReflectionMemberRef, which remember a failed lookup. ADOverhaul hand-rolls three statics with
//     two independent null guards, so a failed method lookup is retried on every call. Kept, since
//     the retry is observable (it is the only reason a mid-session domain change could ever start
//     working).
//
// Otherwise the parameter list, the argument order and the two-overload probe are the same, and the
// callback-ordering trap documented on EditorUtils.ShowObjectPicker applies here verbatim.
//
// ── 2019 vs 2022 ────────────────────────────────────────────────────────────────────────────────
//
// No divergence. Both members are statement-for-statement identical in ADOverhaul2019
// (InstantiateManager, line 3718; CountManager, line 2607) down to the Static | Public binding flags
// on the fallback lookup and the two GUIContent strings. Only the obfuscated names and the polarity
// ILSpy chose for two `if`s differ.
//
// Obfuscator artifact not ported: the decompiled list drawer's picker callback reads
// `asset.CalcStatus<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { ... })`. The
// `_0021_00210` token is ILSpy's rendering of an unnameable generic parameter -- it is just `T`, and
// the cast chain is the closure-capture noise around it. Restored to a plain lambda below.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Lazily-resolved handles onto <c>UnityEditor.ObjectSelector</c>, which is internal to
        /// UnityEditor.dll and so cannot be named at compile time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The two guards are independent, exactly as shipped: the type is resolved once, and the
        /// method is resolved whenever <see cref="show"/> is still null. A version of Unity that has
        /// neither overload therefore pays for the double probe on every picker call, and then
        /// throws on the null invoke.
        /// </para>
        /// <para>
        /// SHIPPED BUG, preserved. The fallback lookup passes
        /// <c>BindingFlags.Static | BindingFlags.Public</c>, but the older <c>Show</c> it is looking
        /// for was an internal <em>instance</em> method — as ControllerEditor's copy of this same
        /// code correctly records. The fallback can therefore never resolve, so on any editor old
        /// enough to lack the modern overload the picker throws a
        /// <see cref="NullReferenceException"/> instead of falling back. Both ADOverhaul builds have
        /// it, so it is the shipped behaviour of this assembly and not a decompiler slip. In
        /// practice it is unreachable: every Unity version ADOverhaul supports has the modern
        /// overload.
        /// </para>
        /// </remarks>
        private static class ObjectSelectorRefs
        {
            /// <summary><c>UnityEditor.ObjectSelector</c>, an internal <see cref="EditorWindow"/>.</summary>
            /// <remarks>
            /// Assembly-qualified with a zeroed version because UnityEditor.dll is always version
            /// 0.0.0.0; that is what lets a plain <see cref="Type.GetType(string)"/> find it without
            /// a domain-wide scan.
            /// </remarks>
            internal static Type objectSelector;

            /// <summary>Whichever <c>Show</c> overload resolved, or null if neither did.</summary>
            internal static MethodInfo show;

            /// <summary>
            /// True when <see cref="show"/> is the modern overload, which takes the edited object and
            /// a <c>showNoneItem</c> flag; false when it is the older, <see cref="SerializedProperty"/>
            /// -taking shape.
            /// </summary>
            internal static bool showTakesObjectBeingEdited;

            internal static void EnsureResolved()
            {
                if (objectSelector == null)
                {
                    objectSelector = Type.GetType(
                        "UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                }

                if (show != null)
                {
                    return;
                }

                // Both overloads share the tail allowSceneObjects, allowedInstanceIDs,
                // onObjectSelectorClosed, onObjectSelectedUpdated; the shipped code built it once and
                // concatenated. Written out here, since spelling a signature in full reads better
                // than assembling it.

                // Show(Object obj, Type requiredType, Object objectBeingEdited, bool allowSceneObjects,
                //      List<int> allowedInstanceIDs, Action<Object> onObjectSelectorClosed,
                //      Action<Object> onObjectSelectedUpdated, bool showNoneItem)
                Type[] modern =
                {
                    typeof(UnityEngine.Object),
                    typeof(Type),
                    typeof(UnityEngine.Object),
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(bool)
                };

                show = objectSelector.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, modern, null);
                showTakesObjectBeingEdited = show != null;

                if (showTakesObjectBeingEdited)
                {
                    return;
                }

                // The older shape, which took the SerializedProperty in place of the edited object
                // and had no showNoneItem flag.
                Type[] legacy =
                {
                    typeof(UnityEngine.Object),
                    typeof(Type),
                    typeof(SerializedProperty),
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>)
                };

                // See the remarks above: these binding flags are wrong, and are kept because they
                // are what shipped.
                show = objectSelector.GetMethod("Show", BindingFlags.Static | BindingFlags.Public, null, legacy, null);
            }
        }

        /// <summary>
        /// Opens Unity's own object picker window and reports the result through callbacks.
        /// </summary>
        /// <param name="obj">The object to show as initially selected. May be null.</param>
        /// <param name="requiredType">Only objects assignable to this type are offered.</param>
        /// <param name="objectBeingEdited">
        /// The object whose field is being picked for. Used only to scope which scene the picker
        /// offers scene objects from; ignored on editors old enough to need the fallback overload.
        /// </param>
        /// <param name="property">
        /// The property being picked for, if the picker is standing in for an object field. Used
        /// only by the fallback overload; the modern one takes <paramref name="objectBeingEdited"/>
        /// instead, so on current Unity versions this argument is dropped.
        /// </param>
        /// <param name="allowedInstanceIDs">
        /// When non-null, restricts the picker to these instance IDs. Null offers everything of the
        /// required type.
        /// </param>
        /// <param name="onSelectorClosed">
        /// Invoked once, with the final selection, when the user dismisses the picker. This is the
        /// callback almost every caller wants.
        /// </param>
        /// <param name="onSelectionChanged">
        /// Invoked on every selection change while the picker is still open, for live previewing.
        /// </param>
        /// <param name="showNoneItem">
        /// Whether the picker offers a "None" entry. Silently ignored on the fallback overload.
        /// </param>
        /// <remarks>
        /// <para>
        /// Reflection is unavoidable: <c>ObjectSelector</c> is internal to UnityEditor.dll, and the
        /// public <c>EditorGUIUtility.ShowObjectPicker&lt;T&gt;</c> cannot stand in for it, because
        /// that one reports back only through <c>ObjectSelectorUpdated</c> /
        /// <c>ObjectSelectorClosed</c> commands routed to whichever window is drawing — which the
        /// callers here, drawing inside someone else's inspector, are not.
        /// </para>
        /// <para>
        /// Note the callback order, which is easy to get backwards: Unity declares the <em>closed</em>
        /// callback before the <em>updated</em> one, so <paramref name="onSelectorClosed"/> is the
        /// seventh parameter and <paramref name="onSelectionChanged"/> the eighth. A handler passed
        /// positionally therefore becomes "closed", which is what callers that just want the picked
        /// object intend.
        /// </para>
        /// <para>
        /// <see cref="EditorWindow.GetWindow(Type)"/> rather than <c>ObjectSelector.get</c>, because
        /// the singleton accessor is as internal as the type is. It is deliberately resolved before
        /// the argument array is built, matching the shipped ordering.
        /// </para>
        /// </remarks>
        internal static void ShowObjectPicker(UnityEngine.Object obj, Type requiredType,
                                              UnityEngine.Object objectBeingEdited = null,
                                              SerializedProperty property = null,
                                              bool allowSceneObjects = true,
                                              List<int> allowedInstanceIDs = null,
                                              Action<UnityEngine.Object> onSelectorClosed = null,
                                              Action<UnityEngine.Object> onSelectionChanged = null,
                                              bool showNoneItem = true)
        {
            ObjectSelectorRefs.EnsureResolved();

            EditorWindow window = EditorWindow.GetWindow(ObjectSelectorRefs.objectSelector);

            object[] arguments = ObjectSelectorRefs.showTakesObjectBeingEdited
                ? new object[]
                {
                    obj, requiredType, objectBeingEdited, allowSceneObjects, allowedInstanceIDs,
                    onSelectorClosed, onSelectionChanged, showNoneItem
                }
                : new object[]
                {
                    obj, requiredType, property, allowSceneObjects, allowedInstanceIDs,
                    onSelectorClosed, onSelectionChanged
                };

            // If neither overload resolved this throws, as it did in the original. There is no
            // sensible fallback for "this editor has no object picker", and swallowing it would
            // leave the caller waiting on a callback that can never arrive.
            ObjectSelectorRefs.show.Invoke(window, arguments);
        }

        /// <summary>
        /// Draws an object-reference array as a plain list of rows, each with a remove button,
        /// followed by a combined drag-and-drop target and click-to-pick footer.
        /// </summary>
        /// <typeparam name="T">
        /// The element type offered by the picker and accepted by the drop target. Dropping a
        /// <see cref="GameObject"/> when <typeparamref name="T"/> is a component type picks the
        /// component off it; see <see cref="CoerceTo{T}"/>.
        /// </typeparam>
        /// <param name="property">The array property. Nothing is applied — the caller owns the <see cref="SerializedObject"/>.</param>
        /// <remarks>
        /// <para>
        /// Deliberately not a <c>ReorderableList</c>: order is not meaningful for any of the lists
        /// this draws, and a footer that is both a drop target and a picker button is not something
        /// <c>ReorderableList</c> offers.
        /// </para>
        /// <para>
        /// The two passes over null slots are how the remove button works, and are not redundant.
        /// Unity's <see cref="SerializedProperty.DeleteArrayElementAtIndex"/> on an array of object
        /// references does not remove the slot when the reference is non-null: it clears the
        /// reference and leaves the slot in place. So the remove button nulls the row, and the next
        /// repaint's null branch — which does not draw anything, deletes the slot, and steps
        /// <c>i</c> back — actually shortens the array. That also cleans up references the user
        /// destroyed elsewhere, for free.
        /// </para>
        /// <para>
        /// Multi-object editing is refused rather than approximated: with differing arrays across
        /// the selection there is no coherent row to draw, so the element loop is skipped entirely
        /// and the footer is replaced with an explanatory label and made inert. Note that the flag
        /// is read once, before the loop, and reused for the footer.
        /// </para>
        /// </remarks>
        internal static void DrawObjectReferenceList<T>(SerializedProperty property) where T : UnityEngine.Object
        {
            bool hasMultipleDifferentValues = property.hasMultipleDifferentValues;

            if (!hasMultipleDifferentValues)
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

                        // Only clears the reference; the null pass above removes the slot on the
                        // following repaint.
                        if (Button(contents.removeSelection, styles.iconButton))
                        {
                            property.DeleteArrayElementAtIndex(i);
                        }
                    }
                }
            }

            Rect footer = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true));

            GUIContent label = hasMultipleDifferentValues
                ? new GUIContent("Editing Multiple Lists", "Editing multiple lists with different values is not supported.")
                : new GUIContent("[Drag And Drop Or Click Here]");

            GUI.Label(footer, label, styles.noteCenter);

            if (hasMultipleDifferentValues)
            {
                return;
            }

            // ControllerEditor's copy of the drop handler, shared by the package. ADOverhaul's own
            // copy differs only in how it applies a caller-supplied filter, and no filter is passed
            // here, so the two are equivalent at this call site. (See the divergence note in
            // ADOEditorUtility.VRChat.cs before adding one.)
            ControllerEditor.EditorUtils.HandleMultiDragAndDrop<T>(footer, property.AddElements);

            if (ClickArea(footer))
            {
                // Positional: this handler is the "selector closed" callback, so the list gains the
                // element once, when the picker is dismissed, rather than on every arrow-key move
                // through it.
                ShowObjectPicker(null, typeof(T), null, null, true, null,
                    o => property.AddElements(new T[1] { o.CoerceTo<T>() }));
            }
        }
    }
}
