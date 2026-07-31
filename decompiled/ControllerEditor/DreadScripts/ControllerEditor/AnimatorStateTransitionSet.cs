using System.Runtime.CompilerServices;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor;

internal readonly struct AnimatorStateTransitionSet
{
	internal enum TransitionSourceType
	{
		StateTransition,
		MachineTransition,
		EntryTransition,
		AnyTransition
	}

	internal readonly AnimatorTransitionBase transition;

	internal readonly AnimatorStateTransition stateTransition;

	internal readonly TransitionSourceType sourceType;

	internal readonly AnimatorState sourceState;

	internal readonly AnimatorStateMachine sourceStateMachine;

	internal readonly AnimatorStateMachine parentStateMachine;

	private static object SetupSystem;

	[SpecialName]
	internal string GetSourceName()
	{
		switch (sourceType)
		{
		case TransitionSourceType.AnyTransition:
			return "AnyState";
		default:
			if (sourceState != null)
			{
				return sourceState.name;
			}
			return "!AnyState";
		case TransitionSourceType.MachineTransition:
			if (sourceStateMachine != null)
			{
				return sourceStateMachine.name;
			}
			return "!AnyState";
		case TransitionSourceType.EntryTransition:
			return "Entry";
		}
	}

	[SpecialName]
	internal string GetDestinationName()
	{
		if (GetIsExit())
		{
			return "Exit";
		}
		if (GetHasDestinationStateMachine())
		{
			return GetDestinationStateMachine().name;
		}
		if (GetHasDestinationState())
		{
			return GetDestinationState().name;
		}
		return "Exit";
	}

	[SpecialName]
	internal string GetDisplayName()
	{
		if (!string.IsNullOrEmpty(transition.name))
		{
			return transition.name;
		}
		return GetSourceName() + " -> " + GetDestinationName();
	}

	[SpecialName]
	internal bool GetIsExit()
	{
		return transition.isExit;
	}

	[SpecialName]
	internal void SetIsExit(bool isparam)
	{
		transition.isExit = isparam;
	}

	[SpecialName]
	internal AnimatorState GetDestinationState()
	{
		return transition.destinationState;
	}

	[SpecialName]
	internal void SetDestinationState(AnimatorState init)
	{
		transition.destinationState = init;
	}

	[SpecialName]
	internal bool GetHasDestinationState()
	{
		return GetDestinationState() != null;
	}

	[SpecialName]
	internal AnimatorStateMachine GetDestinationStateMachine()
	{
		return transition.destinationStateMachine;
	}

	[SpecialName]
	internal void SetDestinationStateMachine(AnimatorStateMachine info)
	{
		transition.destinationStateMachine = info;
	}

	[SpecialName]
	internal bool GetHasDestinationStateMachine()
	{
		return GetDestinationStateMachine() != null;
	}

	[SpecialName]
	internal AnimatorCondition[] GetConditions()
	{
		return transition.conditions;
	}

	[SpecialName]
	internal void SetConditions(AnimatorCondition[] item)
	{
		transition.conditions = item;
	}

	internal AnimatorStateTransitionSet(AnimatorTransitionBase param, TransitionSourceType col, AnimatorState util)
	{
		transition = param;
		stateTransition = param as AnimatorStateTransition;
		sourceType = col;
		sourceState = util;
		sourceStateMachine = null;
		parentStateMachine = null;
	}

	internal AnimatorStateTransitionSet(AnimatorTransitionBase init, TransitionSourceType cfg, AnimatorStateMachine third)
	{
		transition = init;
		stateTransition = init as AnimatorStateTransition;
		sourceType = cfg;
		sourceState = null;
		sourceStateMachine = third;
		parentStateMachine = null;
	}

	internal AnimatorStateTransitionSet(AnimatorTransitionBase item, TransitionSourceType pol, AnimatorStateMachine dir, AnimatorStateMachine visitor2)
	{
		transition = item;
		stateTransition = item as AnimatorStateTransition;
		sourceType = pol;
		sourceState = null;
		sourceStateMachine = dir;
		parentStateMachine = visitor2;
	}

	internal void Remove()
	{
		switch (sourceType)
		{
		case TransitionSourceType.AnyTransition:
			if (sourceStateMachine != null)
			{
				sourceStateMachine.RemoveAnyStateTransition(stateTransition);
			}
			break;
		case TransitionSourceType.EntryTransition:
			if (sourceStateMachine != null)
			{
				sourceStateMachine.RemoveEntryTransition((AnimatorTransition)transition);
			}
			break;
		case TransitionSourceType.StateTransition:
			if (sourceState != null)
			{
				sourceState.RemoveTransition(stateTransition);
			}
			break;
		case TransitionSourceType.MachineTransition:
			if (parentStateMachine != null && sourceStateMachine != null)
			{
				parentStateMachine.RemoveStateMachineTransition(sourceStateMachine, (AnimatorTransition)transition);
			}
			break;
		}
	}

	public static implicit operator AnimatorStateTransition(AnimatorStateTransitionSet def)
	{
		return def.stateTransition;
	}

	public static implicit operator AnimatorTransition(AnimatorStateTransitionSet init)
	{
		return (AnimatorTransition)init.transition;
	}

	public static implicit operator AnimatorTransitionBase(AnimatorStateTransitionSet v)
	{
		return v.transition;
	}

	internal static bool ExcludeSystem()
	{
		return SetupSystem == null;
	}
}
