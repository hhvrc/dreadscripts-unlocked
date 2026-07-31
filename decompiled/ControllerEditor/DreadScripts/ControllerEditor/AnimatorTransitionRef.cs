using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor;

internal struct AnimatorTransitionRef
{
	internal bool isState;

	internal AnimatorState state;

	internal AnimatorStateMachine stateMachine;

	internal AnimatorStateMachine parentStateMachine;

	internal List<AnimatorTransitionBase> incomingTransitions;

	private static object AssetSystem;

	[SpecialName]
	internal AnimatorStateTransition[] StateTransitions()
	{
		if (isState)
		{
			return state.transitions;
		}
		return null;
	}

	[SpecialName]
	internal AnimatorTransition[] StateMachineTransitions()
	{
		if (!isState)
		{
			return parentStateMachine.GetStateMachineTransitions(stateMachine);
		}
		return null;
	}

	internal AnimatorTransitionRef(AnimatorState def)
	{
		state = def;
		parentStateMachine = (stateMachine = null);
		incomingTransitions = new List<AnimatorTransitionBase>();
		isState = true;
	}

	internal AnimatorTransitionRef(AnimatorStateMachine key, AnimatorStateMachine second)
	{
		state = null;
		stateMachine = second;
		parentStateMachine = key;
		incomingTransitions = new List<AnimatorTransitionBase>();
		isState = false;
	}

	internal static bool SelectSystem()
	{
		return AssetSystem == null;
	}
}
