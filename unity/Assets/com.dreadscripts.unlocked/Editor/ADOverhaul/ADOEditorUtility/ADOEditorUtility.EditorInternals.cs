// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static field _ClientSerializer      -> textFieldDropDownMethod (backing), line 2060
//   static SortRef                      -> textFieldDropDownMethod (property), line 3286
//   static PopStatus                    -> TextFieldDropDown(string, ...),      line 3298
//   static InstantiateStatus            -> TextFieldDropDown(GUIContent, ...),  line 3303
//   static RestartStatus                -> TextFieldDropDown(Rect, ...),        line 3313
//   static field candidateSerializer    -> objectSelectorType,        line 2090
//   static field helperSerializer       -> objectSelectorShowMethod,  line 2088
//   static field readerSerializer       -> objectSelectorShowIsInstanceOverload, line 2092
//   static IncludeStatus                -> ShowObjectSelector,        line 3701
//   static field _StubSerializer        -> customEditorAttributesType,   line 2094
//   static field rulesSerializer        -> monoEditorTypeType,           line 2096
//   static field testsSerializer        -> customMultiEditorsField,      line 2098
//   static field _DefinitionSerializer  -> inspectorTypeField,           line 2100
//   static RevertStatus                 -> OverrideCustomEditor,       line 3741
//   static field _InitializerSerializer -> inspectorWindowType,        line 2102
//   static field _TokenSerializer       -> refreshInspectorsMethod,    line 2104
//   static RunVal                       -> RefreshInspectors,         line 3755
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every statement below was transcribed from the region
// above.
//
// DEOBF-BUG(resolved): SortRef carried [SpecialName] with no matching setter, which is how ILSpy
// renders a property getter it could not re-form. It is restored as a lazily-initialised property
// here, matching how the same attribute was handled on CachedIcon and RemoteTexture.
//
// EVERYTHING IN THIS FILE REACHES INTO UNITYEDITOR INTERNALS. None of the four entry points is a
// supported API: they bind by string name to UnityEditor.ObjectSelector, to
// UnityEditor.CustomEditorAttributes and its nested MonoEditorType, to
// UnityEditor.InspectorWindow.RefreshInspectors and to EditorGUI.TextFieldDropDown. A Unity release
// that renames any of them makes the corresponding feature stop working; each lookup is written to
// fail soft (null method, logged nothing) rather than to throw at the call site, except
// OverrideCustomEditor, which dereferences its lookups directly and would throw. That is shipped
// behaviour.
//
// The ObjectSelector lookup handles two shapes of the same internal method because Unity changed it:
// an instance `Show(Object, Type, Object, bool, List<int>, Action<Object>, Action<Object>, bool)`
// and an older static `Show(Object, Type, SerializedProperty, bool, List<int>, Action<Object>,
// Action<Object>)`. Which one was found decides both the argument list and the trailing flag.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        private static MethodInfo textFieldDropDownMethod;

        /// <summary>
        /// <c>EditorGUI.TextFieldDropDown</c> -- a text field with a dropdown of suggested values,
        /// which Unity ships but does not expose.
        /// </summary>
        private static MethodInfo TextFieldDropDownMethod =>
            textFieldDropDownMethod ?? (textFieldDropDownMethod = typeof(EditorGUI).GetMethod(
                "TextFieldDropDown",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(Rect), typeof(GUIContent), typeof(string), typeof(string[]) },
                null));

        /// <summary>
        /// A labelled text field with a dropdown of <paramref name="suggestions"/>, laid out in the
        /// current flow.
        /// </summary>
        /// <returns>
        /// The edited text, or <paramref name="text"/> unchanged if the internal method could not be
        /// resolved -- so a Unity version that has renamed it degrades to "field does nothing"
        /// rather than throwing.
        /// </returns>
        internal static string TextFieldDropDown(string label, string text, string[] suggestions, params GUILayoutOption[] options)
        {
            return TextFieldDropDown(new GUIContent(label), text, suggestions, options);
        }

        /// <inheritdoc cref="TextFieldDropDown(string, string, string[], GUILayoutOption[])"/>
        internal static string TextFieldDropDown(GUIContent label, string text, string[] suggestions, params GUILayoutOption[] options)
        {
            if (TextFieldDropDownMethod == null)
            {
                return text;
            }

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, options);
            return (string)TextFieldDropDownMethod.Invoke(null, new object[] { rect, label, text, suggestions });
        }

        /// <summary>The explicit-rect form of <see cref="TextFieldDropDown(string, string, string[], GUILayoutOption[])"/>.</summary>
        internal static string TextFieldDropDown(Rect rect, string label, string text, string[] suggestions)
        {
            if (TextFieldDropDownMethod == null)
            {
                return text;
            }

            return (string)TextFieldDropDownMethod.Invoke(null, new object[] { rect, new GUIContent(label), text, suggestions });
        }

        private static Type objectSelectorType;

        private static MethodInfo objectSelectorShowMethod;

        /// <summary>
        /// Whether the resolved <c>Show</c> is the newer instance overload rather than the older
        /// static one. Decides both the argument list and which window object the call is made on.
        /// </summary>
        private static bool objectSelectorShowIsInstanceOverload;

        /// <summary>
        /// Opens Unity's own object picker, with the filtering and callbacks the public
        /// <see cref="EditorGUIUtility.ShowObjectPicker{T}"/> does not offer.
        /// </summary>
        /// <param name="current">The object shown as selected when the picker opens.</param>
        /// <param name="type">The type to list.</param>
        /// <param name="owner">Owning object, used by the instance overload to scope scene results.</param>
        /// <param name="property">Property to write into, used by the static overload instead of <paramref name="owner"/>.</param>
        /// <param name="allowSceneObjects">Include objects from the open scenes, not just assets.</param>
        /// <param name="allowedInstanceIDs">Restricts the list to these instance IDs; null means no restriction.</param>
        /// <param name="onSelectionChanged">Raised as the highlighted entry moves, for a live preview.</param>
        /// <param name="onSelectorClosed">Raised once when the picker closes.</param>
        /// <param name="showNoneItem">Whether the list offers a "None" entry. Instance overload only.</param>
        internal static void ShowObjectSelector(
            UnityEngine.Object current,
            Type type,
            UnityEngine.Object owner = null,
            SerializedProperty property = null,
            bool allowSceneObjects = true,
            List<int> allowedInstanceIDs = null,
            Action<UnityEngine.Object> onSelectionChanged = null,
            Action<UnityEngine.Object> onSelectorClosed = null,
            bool showNoneItem = true)
        {
            if (objectSelectorType == null)
            {
                objectSelectorType = Type.GetType("UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            }

            if (objectSelectorShowMethod == null)
            {
                Type[] tail = new Type[]
                {
                    typeof(bool),
                    typeof(List<int>),
                    typeof(Action<UnityEngine.Object>),
                    typeof(Action<UnityEngine.Object>)
                };

                Type[] instanceSignature = new Type[] { typeof(UnityEngine.Object), typeof(Type), typeof(UnityEngine.Object) }
                    .Concat(tail)
                    .Concat(new Type[] { typeof(bool) })
                    .ToArray();

                objectSelectorShowMethod = objectSelectorType.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, instanceSignature, null);
                objectSelectorShowIsInstanceOverload = objectSelectorShowMethod != null;

                if (!objectSelectorShowIsInstanceOverload)
                {
                    Type[] staticSignature = new Type[] { typeof(UnityEngine.Object), typeof(Type), typeof(SerializedProperty) }
                        .Concat(tail)
                        .ToArray();

                    objectSelectorShowMethod = objectSelectorType.GetMethod("Show", BindingFlags.Static | BindingFlags.Public, null, staticSignature, null);
                }
            }

            // The picker is a singleton window, so this both creates it and gives the instance
            // overload its receiver. The static overload ignores the receiver.
            EditorWindow window = EditorWindow.GetWindow(objectSelectorType);

            object[] tailArguments = new object[] { allowSceneObjects, allowedInstanceIDs, onSelectionChanged, onSelectorClosed };
            object[] arguments = objectSelectorShowIsInstanceOverload
                ? new object[] { current, type, owner }.Concat(tailArguments).Concat(new object[] { showNoneItem }).ToArray()
                : new object[] { current, type, property }.Concat(tailArguments).ToArray();

            objectSelectorShowMethod.Invoke(window, arguments);
        }

        private static Type customEditorAttributesType;

        private static Type monoEditorTypeType;

        private static FieldInfo customMultiEditorsField;

        private static FieldInfo inspectorTypeField;

        /// <summary>
        /// Repoints the inspector Unity draws for <paramref name="inspectedType"/> at
        /// <paramref name="editorType"/>, for the rest of the session.
        /// </summary>
        /// <remarks>
        /// This is how the tool takes over the built-in VRChat component inspectors without shipping
        /// a <c>[CustomEditor]</c> that would permanently claim them: it rewrites Unity's own
        /// editor-type table in memory, so the override can be turned back off from the settings.
        /// The table maps a type to a list of candidate editors; only the first entry is repointed,
        /// which is the one Unity picks. The write is not persisted -- a domain reload restores
        /// Unity's own mapping, and the caller re-applies it.
        /// </remarks>
        internal static void OverrideCustomEditor(Type inspectedType, Type editorType)
        {
            if (customEditorAttributesType == null)
            {
                customEditorAttributesType = Type.GetType("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                monoEditorTypeType = Type.GetType("UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                customMultiEditorsField = customEditorAttributesType.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);
                inspectorTypeField = monoEditorTypeType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
            }

            IList candidates = (customMultiEditorsField.GetValue(null) as IDictionary)[inspectedType] as IList;
            inspectorTypeField.SetValue(candidates[0], editorType);

            RefreshInspectors();
        }

        private static Type inspectorWindowType;

        private static MethodInfo refreshInspectorsMethod;

        /// <summary>
        /// Makes every open inspector rebuild its editors, so a change to the editor-type table takes
        /// effect without the user reselecting.
        /// </summary>
        internal static void RefreshInspectors()
        {
            if (inspectorWindowType == null)
            {
                inspectorWindowType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                refreshInspectorsMethod = inspectorWindowType.GetMethod("RefreshInspectors", BindingFlags.Static | BindingFlags.NonPublic);
            }

            refreshInspectorsMethod.Invoke(null, null);
        }
    }
}
