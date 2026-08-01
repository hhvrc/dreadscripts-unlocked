// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PrintPredicate -> MarkDirty,                       line 4108
//   static MapError       -> TryRecordPrefabModifications,    line 8478
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// MapError carries [CompilerGenerated] and has exactly one caller, PrintPredicate; it was a local
// function that the obfuscator lifted out and the decompiler then emitted as a sibling method half
// the file away. It is restored as a private helper rather than a local function -- the behaviour is
// identical and the helper is easier to point a <see cref> at -- and is private because nothing else
// ever called it.
//
// Deliberately not ported: ManagePredicate (line 4103), whose whole body is EditorUtility.SetDirty.
// It is the unconditional version of the method below, adds nothing over calling SetDirty directly,
// and is a plausible member of another partial's region.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Tells Unity that <paramref name="target"/> has been changed outside the inspector, by
        /// whichever of the three mechanisms actually applies to it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// "Dirty" means three different things in Unity depending on what the object is, and getting
        /// it wrong loses the edit silently at the next domain reload. An asset needs
        /// <see cref="EditorUtility.SetDirty(Object)"/>. A prefab instance needs its overrides
        /// recorded instead, or the change is treated as belonging to the prefab and reverts. A scene
        /// object needs its scene marked, or the save prompt never appears. This method covers all
        /// three so callers do not have to know which case they are holding.
        /// </para>
        /// <para>
        /// The whole body is wrapped in a bare <c>catch</c>. That is deliberate on the original's
        /// part: this is called from the tail of edit operations, often in a loop over many objects,
        /// and a destroyed or otherwise unusable object among them should not abort the ones after
        /// it. The cost is that a genuine failure to persist an edit is invisible -- if a change made
        /// through this tool does not survive a reload, this swallowed exception is the first place
        /// to look.
        /// </para>
        /// <para>
        /// Nothing here registers an <see cref="Undo"/> operation. Marking dirty is not itself
        /// undoable and does not make the preceding edit undoable either; callers that want undo
        /// support must have recorded the object before changing it.
        /// </para>
        /// </remarks>
        internal static void MarkDirty(this Object target)
        {
            try
            {
                // SetDirty is skipped for prefab instances: recording the property modifications is
                // the correct and sufficient step there, and SetDirty on top of it would flag the
                // prefab asset itself as changed when it has not been.
                if (!TryRecordPrefabModifications(target))
                {
                    EditorUtility.SetDirty(target);
                }

                if (target is GameObject gameObject)
                {
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
                else if (target is Component component)
                {
                    EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Records <paramref name="target"/>'s overrides against its prefab, reporting whether it was
        /// part of a prefab at all.
        /// </summary>
        /// <remarks>
        /// The return value doubles as the answer to "has this already been handled?", which is why
        /// the caller reads it as a condition rather than calling this for its effect.
        /// </remarks>
        private static bool TryRecordPrefabModifications(Object target)
        {
            if (PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.NotAPrefab)
            {
                return false;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            return true;
        }
    }
}
