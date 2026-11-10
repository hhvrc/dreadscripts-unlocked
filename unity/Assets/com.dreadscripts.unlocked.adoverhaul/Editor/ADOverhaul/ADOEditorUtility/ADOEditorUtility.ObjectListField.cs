// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static OrderStatus -> ObjectListField, line 2595
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- every statement below was transcribed from the region
// above.
//
// The export's inner call reads
//     asset.CalcStatus<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { o.CustomizeStatus<T>() })
// where `_0021_00210` is ILSpy's spelling of the IL generic-argument token `!!0`, i.e. the method's
// own T. The double cast is the decompiler failing to see that the array already is IEnumerable<T>;
// it is written out below as the plain generic call it must have been.
//
// The array accessors and the drag/drop and click helpers this uses are in
// ADOEditorUtility.SerializedProperties.cs, ADOEditorUtility.DragAndDrop.cs and
// ADOEditorUtility.Buttons.cs; the picker is in ADOEditorUtility.EditorInternals.cs.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Draws an object-array property as a list of removable rows plus a drop target that also
        /// opens the object picker on click.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A deliberately smaller thing than <c>ReorderableList</c>: no reordering, no headers and
        /// no per-element callbacks, so it can be dropped into a row of an inspector without owning
        /// the layout.
        /// </para>
        /// <para>
        /// Null entries are pruned as they are encountered rather than being drawn as empty slots,
        /// which is what keeps a list tidy after its referenced assets are deleted. The loop steps
        /// the index back after a prune so the shifted-down element is still visited.
        /// </para>
        /// <para>
        /// A multi-object selection whose lists disagree is refused rather than merged: the rows are
        /// not drawn and the drop target is inert, because there is no answer to "which list does
        /// this drop go into" that would not silently overwrite one of them.
        /// </para>
        /// </remarks>
        internal static void ObjectListField<T>(SerializedProperty property) where T : UnityEngine.Object
        {
            bool multipleValues = property.hasMultipleDifferentValues;

            if (!multipleValues)
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

            Rect dropArea = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true));
            GUIContent prompt = multipleValues
                ? new GUIContent("Editing Multiple Lists", "Editing multiple lists with different values is not supported.")
                : new GUIContent("[Drag And Drop Or Click Here]");
            GUI.Label(dropArea, prompt, styles.noteCenter);

            if (multipleValues)
            {
                return;
            }

            HandleMultiDragAndDrop<T>(dropArea, property.AddToArray);

            if (ClickArea(dropArea))
            {
                ShowObjectSelector(null, typeof(T), null, null, allowSceneObjects: true, null,
                    onSelectionChanged: selected => property.AddToArray(new T[] { selected.As<T>() }));
            }
        }
    }
}
