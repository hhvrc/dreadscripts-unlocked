// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CallPredicate -> HasSelection, line 3124
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// ReorderableListHelper (its own file in this folder's parent) is where the list *building* lives;
// this is only the selection test, which is needed by callers that never touch the helper.

using UnityEditorInternal;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Whether the list's <c>index</c> currently points at one of its elements.
        /// </summary>
        /// <remarks>
        /// ReorderableList.index is -1 for "nothing selected", but it also keeps its old value
        /// after the backing list shrinks, so it can point past the end. Both cases have to be
        /// excluded before indexing, which is what this does.
        /// </remarks>
        internal static bool HasSelection(this ReorderableList list)
        {
            return list.index.IsValidIndex(list.list);
        }
    }
}
