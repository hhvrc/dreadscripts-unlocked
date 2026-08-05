// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ManagePredicate -> SetDirty, line 4103
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EditorUtils.Dirtying.cs ported the conditional, prefab- and scene-aware MarkDirty and recorded
// this one as deliberately not ported, being a bare EditorUtility.SetDirty. It is ported here
// because the choice between the two is a real one that the shipped call sites make deliberately --
// see the remarks -- and because omitting it would leave nothing to point at from the places that
// must NOT use MarkDirty.
// Audit status: VERIFIED -- the single body is the one EditorUtility.SetDirty call the decompiled
// ManagePredicate makes, on the same receiver.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Marks <paramref name="target"/> as changed, with none of the prefab or scene handling
        /// <see cref="MarkDirty"/> does.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The shipped call sites are all animator sub-assets -- transitions, states, state machines.
        /// Those can never be prefab instances and never belong to a scene, so the extra work
        /// <see cref="MarkDirty"/> does would be two type tests and a
        /// <see cref="PrefabUtility.GetPrefabAssetType"/> call that can only ever answer
        /// "not a prefab". More to the point, <see cref="MarkDirty"/> swallows exceptions, and these
        /// call sites sit inside edit operations where a failure to persist ought to surface. Use
        /// this one when the target is known to be an asset; use <see cref="MarkDirty"/> when it
        /// might be anything.
        /// </para>
        /// <para>
        /// No <see cref="Undo"/> step is registered, and marking dirty does not make the preceding
        /// edit undoable -- the caller must have recorded the object before changing it.
        /// </para>
        /// </remarks>
        internal static void SetDirty(this Object target)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
