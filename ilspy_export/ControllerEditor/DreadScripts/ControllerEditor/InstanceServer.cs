using System.Runtime.CompilerServices;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor;

internal readonly struct InstanceServer
{
	internal enum TransitionSourceType
	{
		StateTransition,
		MachineTransition,
		EntryTransition,
		AnyTransition
	}

	internal readonly AnimatorTransitionBase fieldServer;

	internal readonly AnimatorStateTransition m_AttributeServer;

	internal readonly TransitionSourceType m_ClientServer;

	internal readonly AnimatorState m_ConfigServer;

	internal readonly AnimatorStateMachine m_DescriptorServer;

	internal readonly AnimatorStateMachine _TemplateServer;

	private static object SetupSystem;

	[SpecialName]
	internal string WriteConnection()
	{
		switch (m_ClientServer)
		{
		case TransitionSourceType.AnyTransition:
			return "AnyState";
		default:
			if (m_ConfigServer != null)
			{
				return m_ConfigServer.name;
			}
			return "!AnyState";
		case TransitionSourceType.MachineTransition:
			if (m_DescriptorServer != null)
			{
				return m_DescriptorServer.name;
			}
			return "!AnyState";
		case TransitionSourceType.EntryTransition:
			return "Entry";
		}
	}

	[SpecialName]
	internal string StopConnection()
	{
		if (UpdateConnection())
		{
			return "Exit";
		}
		if (OrderContext())
		{
			return PrintConnection().name;
		}
		if (InterruptConnection())
		{
			return RegisterConnection().name;
		}
		return "Exit";
	}

	[SpecialName]
	internal string PrepareConnection()
	{
		if (!string.IsNullOrEmpty(fieldServer.name))
		{
			return fieldServer.name;
		}
		return WriteConnection() + " -> " + StopConnection();
	}

	[SpecialName]
	internal bool UpdateConnection()
	{
		return fieldServer.isExit;
	}

	[SpecialName]
	internal void ChangeConnection(bool isparam)
	{
		fieldServer.isExit = isparam;
	}

	[SpecialName]
	internal AnimatorState RegisterConnection()
	{
		return fieldServer.destinationState;
	}

	[SpecialName]
	internal void LogoutConnection(AnimatorState init)
	{
		fieldServer.destinationState = init;
	}

	[SpecialName]
	internal bool InterruptConnection()
	{
		return RegisterConnection() != null;
	}

	[SpecialName]
	internal AnimatorStateMachine PrintConnection()
	{
		return fieldServer.destinationStateMachine;
	}

	[SpecialName]
	internal void SearchConnection(AnimatorStateMachine info)
	{
		fieldServer.destinationStateMachine = info;
	}

	[SpecialName]
	internal bool OrderContext()
	{
		return PrintConnection() != null;
	}

	[SpecialName]
	internal AnimatorCondition[] SetContext()
	{
		return fieldServer.conditions;
	}

	[SpecialName]
	internal void PostContext(AnimatorCondition[] item)
	{
		fieldServer.conditions = item;
	}

	internal InstanceServer(AnimatorTransitionBase param, TransitionSourceType col, AnimatorState util)
	{
		fieldServer = param;
		m_AttributeServer = param as AnimatorStateTransition;
		m_ClientServer = col;
		m_ConfigServer = util;
		m_DescriptorServer = null;
		_TemplateServer = null;
	}

	internal InstanceServer(AnimatorTransitionBase init, TransitionSourceType cfg, AnimatorStateMachine third)
	{
		fieldServer = init;
		m_AttributeServer = init as AnimatorStateTransition;
		m_ClientServer = cfg;
		m_ConfigServer = null;
		m_DescriptorServer = third;
		_TemplateServer = null;
	}

	internal InstanceServer(AnimatorTransitionBase item, TransitionSourceType pol, AnimatorStateMachine dir, AnimatorStateMachine visitor2)
	{
		fieldServer = item;
		m_AttributeServer = item as AnimatorStateTransition;
		m_ClientServer = pol;
		m_ConfigServer = null;
		m_DescriptorServer = dir;
		_TemplateServer = visitor2;
	}

	internal void FillConnection()
	{
		switch (m_ClientServer)
		{
		case TransitionSourceType.AnyTransition:
			if (m_DescriptorServer != null)
			{
				m_DescriptorServer.RemoveAnyStateTransition(m_AttributeServer);
			}
			break;
		case TransitionSourceType.EntryTransition:
			if (m_DescriptorServer != null)
			{
				m_DescriptorServer.RemoveEntryTransition((AnimatorTransition)fieldServer);
			}
			break;
		case TransitionSourceType.StateTransition:
			if (m_ConfigServer != null)
			{
				m_ConfigServer.RemoveTransition(m_AttributeServer);
			}
			break;
		case TransitionSourceType.MachineTransition:
			if (_TemplateServer != null && m_DescriptorServer != null)
			{
				_TemplateServer.RemoveStateMachineTransition(m_DescriptorServer, (AnimatorTransition)fieldServer);
			}
			break;
		}
	}

	public static implicit operator AnimatorStateTransition(InstanceServer def)
	{
		return def.m_AttributeServer;
	}

	public static implicit operator AnimatorTransition(InstanceServer init)
	{
		return (AnimatorTransition)init.fieldServer;
	}

	public static implicit operator AnimatorTransitionBase(InstanceServer v)
	{
		return v.fieldServer;
	}

	internal static bool ExcludeSystem()
	{
		return SetupSystem == null;
	}
}
