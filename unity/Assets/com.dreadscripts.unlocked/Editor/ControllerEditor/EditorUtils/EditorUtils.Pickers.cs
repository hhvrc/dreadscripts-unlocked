// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ConcatList -> ShowObjectPicker(...), line 6690
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The decompiled method hand-rolls its reflection into three shared static caches on EditorUtils
// (observerProcessor / processorProcessor / m_ServerProcessor); those are replaced here by the
// package's TypeResolver and ReflectionMemberRef, which give the same resolve-once-including-the-
// failure behaviour without three more fields on the partial class.
//
// Internal Unity members bound by this file, all on UnityEditor.dll:
//   type   UnityEditor.ObjectSelector                                        (an internal EditorWindow)
//   method ObjectSelector.Show(Object, Type, Object, bool, List<int>,
//                              Action<Object>, Action<Object>, bool)         -- preferred
//   method ObjectSelector.Show(Object, Type, SerializedProperty, bool,
//                              List<int>, Action<Object>, Action<Object>)    -- fallback
//
// Version range: the preferred overload was verified present, with exactly this signature and
// parameter order, in Unity 2022.3 and Unity 6000.3 (2022.3.22f1 and 6000.3.8f1 were decompiled
// while porting this). The fallback is the older, pre-objectBeingEdited shape that took the
// SerializedProperty directly; it is not present in either of those versions, so it only ever
// fires on the Unity 2018/2019-era editors the original package also supported. That older
// signature was not verified against a live install and is transcribed from the decompiled source
// as-is.
// Audit status: VERIFIED against reverse-engineering/export/
//
// The two callbacks are the subtle part -- see the parameter documentation below.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Lazily-resolved handles onto <c>UnityEditor.ObjectSelector</c>, which is internal to
        /// UnityEditor.dll and so cannot be named at compile time.
        /// </summary>
        private static class ObjectSelectorRefs
        {
            /// <remarks>
            /// Assembly-qualified with a zeroed version because UnityEditor.dll is always version
            /// 0.0.0.0; that is what lets a plain <see cref="Type.GetType(string)"/> find it without
            /// the domain-wide scan.
            /// </remarks>
            internal static readonly TypeResolver objectSelector = new TypeResolver(
                "UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            /// <summary>
            /// <c>Show(Object obj, Type requiredType, Object objectBeingEdited, bool allowSceneObjects,
            /// List&lt;int&gt; allowedInstanceIDs, Action&lt;Object&gt; onObjectSelectorClosed,
            /// Action&lt;Object&gt; onObjectSelectedUpdated, bool showNoneItem)</c>.
            /// </summary>
            internal static readonly ReflectionMemberRef<MethodInfo> showWithObjectBeingEdited =
                new ReflectionMemberRef<MethodInfo>(objectSelector, "Show", new[]
                {
                    typeof(UnityEngine.Object),
                    typeof(Type),
                    typeof(UnityEngine.Object),
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(bool)
                }, BindingFlags.Instance | BindingFlags.NonPublic);

            /// <summary>
            /// The older <c>Show</c>, which took the <see cref="SerializedProperty"/> in place of the
            /// edited object and had no <c>showNoneItem</c> flag.
            /// </summary>
            internal static readonly ReflectionMemberRef<MethodInfo> showWithSerializedProperty =
                new ReflectionMemberRef<MethodInfo>(objectSelector, "Show", new[]
                {
                    typeof(UnityEngine.Object),
                    typeof(Type),
                    typeof(SerializedProperty),
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>)
                }, BindingFlags.Instance | BindingFlags.NonPublic);
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
        /// Whether the picker offers a "None" entry. Silently ignored on the fallback overload,
        /// which always shows one.
        /// </param>
        /// <remarks>
        /// <para>
        /// Reflection is unavoidable here: <c>ObjectSelector</c> is internal to UnityEditor.dll, and
        /// the public <c>EditorGUIUtility.ShowObjectPicker&lt;T&gt;</c> cannot be used in its place
        /// because it reports back only through <c>ObjectSelectorUpdated</c>/<c>ObjectSelectorClosed</c>
        /// commands routed to whichever window is drawing, which the callers here are not.
        /// </para>
        /// <para>
        /// Note the callback order, which is easy to get backwards: Unity declares the *closed*
        /// callback before the *updated* one, so <paramref name="onSelectorClosed"/> is the seventh
        /// parameter and <paramref name="onSelectionChanged"/> the eighth. Passing a handler
        /// positionally therefore hands it to "closed", which is the intended behaviour for callers
        /// that just want the picked object.
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
            MethodInfo show = ObjectSelectorRefs.showWithObjectBeingEdited.Member;
            bool takesObjectBeingEdited = show != null;
            if (!takesObjectBeingEdited)
            {
                show = ObjectSelectorRefs.showWithSerializedProperty.Member;
            }

            // GetWindow rather than ObjectSelector.get, because the singleton accessor is as
            // internal as the type is; the window is a normal EditorWindow, so this both creates and
            // focuses it exactly as Unity's own call would. Deliberately resolved before the
            // arguments are built, matching the original's ordering.
            EditorWindow window = EditorWindow.GetWindow(ObjectSelectorRefs.objectSelector);

            object[] arguments = takesObjectBeingEdited
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
            show.Invoke(window, arguments);
        }
    }
}
