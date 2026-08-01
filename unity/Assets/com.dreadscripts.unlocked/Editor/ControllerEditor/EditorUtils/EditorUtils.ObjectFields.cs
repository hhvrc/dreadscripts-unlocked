// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CountPredicate -> ObjectField, line 3139
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// One member, and it is one call. It is ported anyway because it is what makes an object field read
// as an assignment at the call site -- `x = x.ObjectField(label)` rather than a four-argument call
// with a typeof() and a cast -- and because ControllerEditorWindow.Defaults (DrawOtherDefaults)
// currently inlines it with a note pointing here. That inlining can now be replaced by a call.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A layout object field that infers its type from the value it is given.
        /// </summary>
        /// <param name="value">
        /// The current reference, and the source of the field's type. Because the type comes from
        /// <typeparamref name="T"/> rather than from the instance, a null value still produces a
        /// correctly typed field; a value whose runtime type is more derived than
        /// <typeparamref name="T"/> still produces a field that accepts any <typeparamref name="T"/>.
        /// </param>
        /// <param name="allowSceneObjects">
        /// Whether a scene object may be dropped in. Left at true, which is right for a field backing
        /// a runtime reference and wrong for one backing something that will be saved to an asset --
        /// a scene reference cannot survive serialisation there.
        /// </param>
        internal static T ObjectField<T>(this T value, GUIContent label, bool allowSceneObjects = true,
            params GUILayoutOption[] options) where T : Object
        {
            return (T)EditorGUILayout.ObjectField(label, value, typeof(T), allowSceneObjects, options);
        }
    }
}
