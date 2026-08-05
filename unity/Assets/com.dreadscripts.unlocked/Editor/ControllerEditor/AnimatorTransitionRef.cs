// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTransitionRef.cs
//
// NOTES
// StateTransitions and StateMachineTransitions carry [SpecialName] in the decompiled source, i.e.
// they are property getters ILSpy could not recombine; they are restored as properties here.
//
// NOT PORTED
// The `private static object AssetSystem` field and the `SelectSystem()` method that only tested it
// for null. Protector licence-check scaffolding, the same pattern recorded in Common/SphereHandle.cs
// and ADOverhaul/PhysBoneParameter.cs: nothing assigns the field, so the predicate is a constant
// `true`, and no caller reads either member. (The sibling type AnimatorStateTransitionSet keeps its
// copy of this pair instead of dropping it -- see the note there.)
//
// Audit status: VERIFIED -- all five fields, both constructors and both accessors were diffed
// statement by statement against export/, including which constructor nulls which field. The two
// dropped scaffolding members are recorded above.

using System.Collections.Generic;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Refers to whatever a transition can leave from — either an <see cref="AnimatorState"/> or a
    /// child <see cref="AnimatorStateMachine"/> — so both can be handled through one type.
    /// </summary>
    /// <remarks>
    /// Unity keeps these two cases apart: a state owns its outgoing transitions directly, while a
    /// child state machine's transitions are owned by its <em>parent</em> and reached through
    /// <see cref="AnimatorStateMachine.GetStateMachineTransitions"/>. That asymmetry is the reason
    /// this type exists, and it is why the two accessors below each return null for the case they do
    /// not apply to — check <see cref="isState"/> to know which to ask for.
    /// </remarks>
    internal struct AnimatorTransitionRef
    {
        /// <summary>True when this refers to a state, false when it refers to a child state machine.</summary>
        internal bool isState;

        /// <summary>Set when <see cref="isState"/>; null otherwise.</summary>
        internal AnimatorState state;

        /// <summary>The child state machine the transitions leave from. Null when <see cref="isState"/>.</summary>
        internal AnimatorStateMachine stateMachine;

        /// <summary>The state machine that owns <see cref="stateMachine"/>'s transitions.</summary>
        internal AnimatorStateMachine parentStateMachine;

        /// <summary>Scratch list for transitions being collected against this source.</summary>
        internal List<AnimatorTransitionBase> incomingTransitions;

        internal AnimatorTransitionRef(AnimatorState state)
        {
            this.state = state;
            parentStateMachine = stateMachine = null;
            incomingTransitions = new List<AnimatorTransitionBase>();
            isState = true;
        }

        internal AnimatorTransitionRef(AnimatorStateMachine parentStateMachine, AnimatorStateMachine stateMachine)
        {
            state = null;
            this.stateMachine = stateMachine;
            this.parentStateMachine = parentStateMachine;
            incomingTransitions = new List<AnimatorTransitionBase>();
            isState = false;
        }

        /// <summary>Outgoing transitions when this refers to a state; null otherwise.</summary>
        internal AnimatorStateTransition[] StateTransitions => isState ? state.transitions : null;

        /// <summary>Outgoing transitions when this refers to a child state machine; null otherwise.</summary>
        internal AnimatorTransition[] StateMachineTransitions =>
            isState ? null : parentStateMachine.GetStateMachineTransitions(stateMachine);
    }
}
