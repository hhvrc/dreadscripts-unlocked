// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static InvokeResolver   -> ForEach,                 line 2555
//   static FindResolver     -> IndexOf,                 line 2563
//   static ExcludeResolver  -> TryGetIndex,             line 2580
//   static LoginPredicate   -> IndexOfBehaviour,        line 3615
//   static ReflectPredicate -> RemoveBehaviourOfType,   line 3620
//   static DeletePredicate  -> RemoveBehaviourAt,       line 3628
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Two regions, collected here because the second is written entirely in terms of the first. The
// three sequence helpers (2555-2584) open a run of small LINQ-ish extensions in the decompiled
// class; their neighbours belong to other partials -- InitResolver (line 2586) is a hideFlags
// toggle, ported in EditorUtils.UnityObjects.cs as SetDontSave, and the members immediately before
// InvokeResolver are the IConstraint activation helpers. The three AnimatorState helpers are
// contiguous and complete; their neighbours ClonePredicate (line 3605, a transition dead-end test)
// and CreatePredicate (line 3660, a condition-list multiset compare) belong to the
// transition/condition region and are ported in EditorUtils.AnimatorGraph.cs as IsExitOrDangling
// and ConditionSetsMatch.
//
// IndexOf's decompiled body increments its counter inside checked(), a detail of the original's
// compilation context rather than intent -- an index can only overflow past int.MaxValue elements,
// which no IEnumerable reachable here can produce. It is written as a plain increment.
//
// Nothing here was already ported under another name; the package was searched for each member and
// for each of these English names before the file was added.
//
// NOTES
// The rename maps have since settled on FindIndex / TryFindIndex for FindResolver / ExcludeResolver,
// so the current export/ snapshot shows those two under names this file does not use. The ported
// names IndexOf / TryGetIndex are left as they are -- the left MAP column is the durable key -- but
// the divergence is recorded here so the next reader does not read it as a missing member.
// Audit status: VERIFIED -- all six bodies diffed statement by statement against export/. Two
// shape-only differences, both behaviour-preserving: IndexOf's counter increment is `checked` in the
// decompilation, and RemoveBehaviourAt's decompiled form negates the main-asset test and nests the
// destroy pair in the else, which is the same three-way choice written the other way round.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Runs <paramref name="action"/> over every element of <paramref name="source"/>.</summary>
        /// <remarks>
        /// <see cref="List{T}.ForEach"/> for anything enumerable. It exists because most of the
        /// collections this tool walks are Unity's arrays -- a state's behaviours, a transition's
        /// conditions -- and writing the traversal as a call rather than a statement lets the call
        /// sites nest one walk inside another as expressions without naming a loop variable per
        /// level. Where a call site's receiver is a <see cref="List{T}"/>, the instance method wins
        /// over this extension; the two behave identically.
        /// </remarks>
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Returns the position of the first element satisfying <paramref name="predicate"/>, or -1
        /// when there is none.
        /// </summary>
        /// <remarks>
        /// The sequence counterpart of <see cref="Array.FindIndex{T}(T[], Predicate{T})"/>, which the
        /// callers need because they act on the index rather than the element: removing an entry from
        /// one of Unity's arrays means assigning a whole new array back, so the position is the thing
        /// worth carrying. Null elements are skipped without consulting the predicate -- Unity's
        /// object arrays routinely contain null slots where a sub-asset was lost, and no caller's
        /// predicate is prepared for one -- but they still occupy a position, so the index returned
        /// remains valid against the original sequence.
        /// </remarks>
        internal static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = -1;
            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    index++;
                    if (enumerator.Current != null && predicate(enumerator.Current))
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// <see cref="IndexOf{T}"/> in try-get form: reports whether a match was found and hands back
        /// its position.
        /// </summary>
        /// <remarks>
        /// Purely so a search and its "did it hit" test read as one condition. <paramref name="index"/>
        /// is set to -1 on a miss, not left undefined, so a caller that ignores the return value and
        /// passes the index straight to <see cref="RemoveBehaviourAt"/> still behaves -- that method
        /// treats a negative index as "nothing to do".
        /// </remarks>
        internal static bool TryGetIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate, out int index)
        {
            index = source.IndexOf(predicate);
            return index != -1;
        }

        /// <summary>
        /// Returns the position of <paramref name="behaviour"/> in <paramref name="state"/>'s
        /// behaviour list, or -1 when it is not on this state.
        /// </summary>
        /// <remarks>
        /// The comparison is reference identity on the behaviour asset, which is what the callers
        /// mean: the multi-editors hold a direct reference to one selected state's behaviour and ask
        /// each of the other selected states whether that exact object is theirs.
        /// </remarks>
        internal static int IndexOfBehaviour(this AnimatorState state, StateMachineBehaviour behaviour)
        {
            return state.behaviours.IndexOf(b => b == behaviour);
        }

        /// <summary>
        /// Removes the first behaviour of exactly <paramref name="behaviourType"/> from
        /// <paramref name="state"/>, if it has one.
        /// </summary>
        /// <param name="withUndo">Whether the removal should be undoable; see <see cref="RemoveBehaviourAt"/>.</param>
        /// <remarks>
        /// The type test is exact rather than assignable-from because the types being matched are
        /// resolved reflectively out of the VRChat SDK (see <see cref="AnimatorTypeCache"/>) and a
        /// subclass of, say, the tracking-control behaviour would be somebody else's component that
        /// this tool has no business deleting. Only the first match is removed; nothing in the tool
        /// puts two behaviours of one SDK type on a single state, and the call sites are per-state
        /// "remove the one I am editing" buttons.
        /// </remarks>
        internal static void RemoveBehaviourOfType(this AnimatorState state, Type behaviourType, bool withUndo = false)
        {
            if (state.behaviours.TryGetIndex(b => b.GetType() == behaviourType, out int index))
            {
                state.RemoveBehaviourAt(index, withUndo);
            }
        }

        /// <summary>
        /// Removes the behaviour at <paramref name="index"/> from <paramref name="state"/> and
        /// destroys it, leaving no orphaned sub-asset behind.
        /// </summary>
        /// <param name="index">
        /// A negative index means "no match was found" and is a no-op, so the result of a search can
        /// be passed straight in without a guard at the call site.
        /// </param>
        /// <param name="withUndo">
        /// Whether to route the change through <see cref="Undo"/> so the user can take it back. The
        /// button call sites pass true; internal cleanup passes false.
        /// </param>
        /// <remarks>
        /// <para>
        /// Detaching the behaviour is not enough. Unity stores state machine behaviours as sub-assets
        /// of the controller file, so one merely dropped from the array stays in the .controller
        /// forever as an invisible orphan; the destroy is what actually reclaims it. Which destroy
        /// depends on where it lives: if the behaviour is the main asset at its own path it is a
        /// standalone asset file and the file itself is deleted, otherwise it is a sub-asset or an
        /// unsaved instance and the object is destroyed in place.
        /// </para>
        /// <para>
        /// The array is rebuilt and assigned back because <see cref="AnimatorState.behaviours"/>
        /// returns a copy -- mutating what the getter handed over would change nothing.
        /// </para>
        /// <para>
        /// Preserved as shipped: with <paramref name="withUndo"/> false and the behaviour a sub-asset
        /// of a saved controller, this reaches <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/>
        /// on a persistent object, which Unity refuses ("Destroying assets is not permitted to avoid
        /// data loss") -- the behaviour is detached but survives as an orphan. Only the undo path,
        /// which is what every call site in the shipped tool uses, handles that case, so the defect
        /// is unreachable in practice. <see cref="AssetDatabase.RemoveObjectFromAsset"/> would be the
        /// fix; it is not applied here.
        /// </para>
        /// </remarks>
        internal static void RemoveBehaviourAt(this AnimatorState state, int index, bool withUndo = false)
        {
            if (index < 0)
            {
                return;
            }

            if (withUndo)
            {
                Undo.RecordObject(state, "Delete Behaviour");
            }

            StateMachineBehaviour behaviour = state.behaviours[index];
            StateMachineBehaviour[] behaviours = state.behaviours;
            ArrayUtility.RemoveAt(ref behaviours, index);
            state.behaviours = behaviours;

            string assetPath = AssetDatabase.GetAssetPath(behaviour);
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) == behaviour)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            else if (withUndo)
            {
                Undo.DestroyObjectImmediate(behaviour);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(behaviour);
            }

            EditorUtility.SetDirty(state);
        }
    }
}
