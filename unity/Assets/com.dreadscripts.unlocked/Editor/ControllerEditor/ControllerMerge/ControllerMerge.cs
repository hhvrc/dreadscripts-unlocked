// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   static SetAlgo -> ControllerMerge.RenameParameter, line 13483
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// SetAlgo's captured-variable closure, _003C_003Ec__DisplayClass379_0 (line 7128), is not ported as
// a type; its contents are restored as the ParameterRewriter class in this folder. See that file's
// header for the mapping and for the list of things a rename does not reach.
//
// SetAlgo is a static member of the ControllerEditorWindow god class in the decompiled source, as is
// everything else in this family. It is given its own static class here rather than a partial of
// that window because it is not window code: it takes an animator object graph and edits it, has no
// GUI, and the window is only one of its callers.
//
// Audit status: VERIFIED -- RenameParameter is the only member this file declares, and its body was
// diffed statement by statement against export ControllerEditor.cs lines 13483-13545 on 2026-08-05:
// the IsVRCSDKAvailable guard around the driver rewrite, both walker calls with their recursion
// flags off, the early return on !recurse, and the recursion over stateMachines.Select(c =>
// c.stateMachine) with the three captured arguments forwarded. The three delegate bodies the
// decompilation shows inline belong to ParameterRewriter.cs, which claims and documents them; they
// were not re-derived here. The range contains no goto, no residual switch dispatch, no
// `while (true)` and no unresolved smethod_N, so no deobfuscator fault applies.
//
// ============================================================================================
// STILL DEFERRED -- but re-derived on 2026-08-05, and the headline has changed: none of the three
// members below is blocked on missing code any more. Every dependency they were waiting on has
// landed. They are unwritten, not blocked. Nothing below is stubbed.
//
//   CompareAlgo, line 13426 -- the merge itself: copies a source controller's layers into a
//     destination, applying a list of parameter renames to the copies as it goes so they do not
//     collide with names the destination already has.
//
//     ITS BLOCKERS ARE ALL CLEARED as of 2026-08-05. All three named EditorUtils members have
//     landed, and all three are `internal`:
//       * EditorUtils.ResetPredicate (EditorUtils.cs line 3355), the progress-bar-wrapped bulk
//         layer copy -> EditorUtils.CopyLayers, EditorUtils.LayerCopying.cs line 368. The chain the
//         old note described is resolved with it: CalculatePredicate (3417), the deep layer clone,
//         is ported as CopyLayer in the same file.
//       * EditorUtils.SetRules<T> (EditorUtils.cs line 4205), the "duplicate this asset to that
//         path" helper -> EditorUtils.CloneToAsset<T>, EditorUtils.Assets.cs line 210.
//       * EditorUtils.InterruptPredicate (EditorUtils.cs line 4088), the sub-asset re-parenting
//         helper -> EditorUtils.AddSubAsset, EditorUtils.Assets.cs line 103.
//     CompareAlgo's own closure, _003C_003Ec__DisplayClass378_0 (line 6995) -- CloneServer, the
//     %Parameter% token substitution in copied layer and state names; ReflectServer, the
//     externalise-an-embedded-motion step; DeleteServer, the motion deduplication and animation
//     curve rebinding -- is restored as ordinary lambdas whenever CompareAlgo is. The last two are
//     what needed SetRules and InterruptPredicate, so they are unblocked too.
//
//     So CompareAlgo is NOT blocked -- it is unwritten. This pass established that and stopped
//     there rather than attempting the reconstruction without the budget to verify it. Note for
//     whoever writes it: CopyLayers takes `out AnimatorControllerLayer[]` for the copied layers,
//     and neither it nor CopyLayer registers any Undo, which matches the shipped merge's own
//     "confirming registers no Undo anywhere" behaviour noted under ParameterRenameWindow below.
//
//   ParameterRenameWindow, lines 3998-4155 -- the dialog CompareAlgo is driven from. Its
//     OnCustomConfirm (line 4092) is a call to CompareAlgo and its OnCustomGUI (line 4048) is
//     abstract on UtilityWindowBase<T>, so the type cannot be written at all until CompareAlgo
//     exists: both members are required overrides and neither may be stubbed. That ordering still
//     holds, but it is now a sequencing constraint rather than a blocker -- CompareAlgo has nothing
//     left standing in its way either (see above), so this whole family is unwritten rather than
//     blocked. Its other two blockers cleared earlier: EditorUtils.reservedAvatarParameters has
//     landed with 28 entries, and RenameParameter below is available.
//     Notes worth keeping for whoever finishes it, all verified against the decompiled source:
//       * It is a merge conflict dialog, not a rename dialog. It is only ever opened by OrderAlgo,
//         which is handed a source and a destination controller; the row list is the source's
//         parameters minus the reserved VRChat ones, and the "Unique Parameters" toggle
//         (serviceMapper, line 4017) runs each proposed name through a uniqueness pass against
//         both the other rows and the destination controller's existing parameters. Renaming is
//         the means; avoiding a collision on merge is the point.
//       * Its uniqueness pass, ListTests (line 4111), loops until the name stops changing because
//         the two constraints -- unique among the rows, unique against the destination -- can push
//         a name back and forth. The while(true) inside its first predicate is control-flow
//         flattening, not a real loop: it is a plain "no other row already claims this name" scan.
//       * The Write Defaults dropdown (line 4088) is applied after the merge, to the copied layers
//         only, and "No Change" is the default.
//       * Confirming registers no Undo anywhere. See ParameterRewriter.Rewrite.
//
//   OrderAlgo, line 13406 -- the entry point: builds a ParameterRenameWindow for a source and
//     destination controller and shows it centred on the animator window, falling back to the
//     screen centre when the animator window cannot be found. Deferred only because
//     ParameterRenameWindow is. Note for whoever ports it: it reads Event.current.control and
//     Event.current.shift to decide the initial state of the uniqueness toggle -- holding either
//     modifier opens the dialog with renaming off -- so it must be called from inside a GUI event,
//     and AnimatorGraphReflection.GraphAccessors.Tool is now a property rather than the decompiled
//     Tool() method.
// ============================================================================================

using System.Linq;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The engine behind merging one animator controller's layers into another.
    /// </summary>
    /// <remarks>
    /// Only the parameter-rewriting half is present; see this file's header for the merge itself and
    /// its dialog, which are deferred.
    /// </remarks>
    internal static class ControllerMerge
    {
        /// <summary>
        /// Points every reference to <paramref name="oldName"/> within
        /// <paramref name="stateMachine"/> at <paramref name="newName"/> instead.
        /// </summary>
        /// <param name="exactMatch">
        /// True to rename only references equal to <paramref name="oldName"/>. False turns this into
        /// a substring replacement across every parameter reference in the machine, which is a much
        /// broader edit than a rename -- see <see cref="ParameterRewriter.Matches"/>. The merge flow
        /// passes true.
        /// </param>
        /// <param name="recurse">
        /// False confines the work to this machine's own states and transitions, for callers that are
        /// walking the nesting themselves.
        /// </param>
        /// <remarks>
        /// <para>
        /// This does not touch the controller's parameter list -- it only repairs the references. The
        /// caller is expected to have added or renamed the parameter itself; calling this alone
        /// leaves every rewritten reference pointing at a parameter that does not exist.
        /// </para>
        /// <para>
        /// The two walkers are asked not to recurse and the descent into nested machines is done here
        /// instead, because a nested machine needs its own state-machine-level behaviours rewritten
        /// -- something neither walker visits. Recursing through them instead would silently skip
        /// every parameter driver attached to a sub-state-machine.
        /// </para>
        /// <para>
        /// Undo is not registered; only <see cref="UnityEditor.EditorUtility.SetDirty"/> is called.
        /// A parameter rename performed through this method cannot be undone.
        /// </para>
        /// </remarks>
        internal static void RenameParameter(AnimatorStateMachine stateMachine, string oldName, string newName,
            bool exactMatch, bool recurse = true)
        {
            ParameterRewriter rewriter = new ParameterRewriter(oldName, newName, exactMatch);

            if (AnimatorTypeCache.IsVRCSDKAvailable())
            {
                rewriter.RewriteDrivers(stateMachine.behaviours);
            }

            stateMachine.ForEachState(rewriter.RewriteState, recurse: false);
            stateMachine.ForEachTransition(rewriter.RewriteTransition, recurse: false);

            if (!recurse)
            {
                return;
            }

            foreach (AnimatorStateMachine child in stateMachine.stateMachines.Select(c => c.stateMachine))
            {
                RenameParameter(child, oldName, newName, exactMatch);
            }
        }
    }
}
