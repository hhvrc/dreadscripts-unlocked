// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   RestartAlgo -> RenameStates, line 14376
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// One member, in its own file, on the same rule as ControllerEditor.CollapsibleSection.cs: it has
// two callers in two different decompiled regions -- the state rename overlay's `onEndRename`
// handler, built in ControllerEditor.ReflectionPriming.cs, and the still-unported "State Settings"
// row (CalcVisitor, 12404) -- so neither of those files can claim it without claiming a member
// outside its own region.
//
// It is what makes renaming a state in the graph a multi-rename. The overlay edits one name; this
// applies the result to every selected state, which is the behaviour the tool exists for.
//
// THE UNIQUENESS CALL IS PER STATE, NOT ONCE. `MakeUniqueStateName` is asked for a fresh name inside
// the loop, and it consults the machine's current state names each time -- so renaming three
// selected states to "Idle" yields "Idle", "Idle 1", "Idle 2" rather than three collisions. That
// only works because the assignment happens inside the same loop iteration: each rename is visible
// to the next call. Hoisting the call out of the loop, which is the obvious-looking tidy-up, would
// silently produce three states with one name.
//
// THE GUARD IS ON THE TYPED NAME, NOT THE UNIQUE ONE. A state whose name already equals what the
// user typed is skipped entirely -- no undo entry, no write. Without that, re-confirming a rename
// without changing anything would push every selected state through MakeUniqueStateName and rename
// the ones that collide, so pressing Enter twice would append a number. Note the asymmetry it
// creates: the first state keeps the typed name and the rest get numbered, and which one is "first"
// is the selection's order.
//
// ONE UNDO ENTRY PER STATE, all sharing the label "Rename States". Unity coalesces same-label
// records made in one frame into a single undo group, so the user sees one undo despite the loop.
// The label is the shipped string and is not corrected for the singular case.
//
// The parameter is the state machine to make the name unique *within*; the shipped call sites pass
// ActiveStateMachine, i.e. the machine the graph is showing rather than the layer root, so a name is
// unique within the sub-machine on screen and may repeat elsewhere in the layer. That is shipped.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- the body below was diffed statement for statement against decompiled
// lines 14376-14386: the parameter list and its order, the `!=` guard on the typed name, the
// Undo.RecordObject call with its exact label, and the MakeUniqueStateName call being made per
// iteration on the parameter rather than on the state's own machine. Both call sites were read in
// export/ to confirm what is passed (ActiveStateMachine, selectedStates, and the overlay's Name in
// one, a local in the other). The range contains no `goto`, no residual `switch` dispatch, no
// `while (true)` and no unresolved `smethod_N`, so no deobfuscator fault applies to it, and it
// carries no licence gate.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region State renaming

        /// <summary>
        /// Renames every state in <paramref name="states"/> to <paramref name="name"/>, made unique
        /// within <paramref name="stateMachine"/>.
        /// </summary>
        /// <param name="stateMachine">
        /// The machine the new names must be unique within -- the one the graph is showing, not
        /// necessarily the layer root.
        /// </param>
        /// <param name="states">The states to rename. Iterated in order, which decides which one
        /// gets the unnumbered name.</param>
        /// <param name="name">The name the user typed.</param>
        /// <remarks>
        /// States already carrying <paramref name="name"/> are skipped outright, so re-confirming a
        /// rename is a no-op rather than a numbering pass. See the file header.
        /// </remarks>
        private static void RenameStates(AnimatorStateMachine stateMachine, IEnumerable<AnimatorState> states, string name)
        {
            foreach (AnimatorState state in states)
            {
                if (state.name != name)
                {
                    Undo.RecordObject(state, "Rename States");

                    // Asked per state, inside the loop: each rename is visible to the next call,
                    // which is what numbers the duplicates instead of colliding them.
                    state.name = stateMachine.MakeUniqueStateName(name);
                }
            }
        }

        #endregion
    }
}
