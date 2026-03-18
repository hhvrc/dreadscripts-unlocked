using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor;

internal struct RepositoryServer
{
	internal bool _MappingServer;

	internal AnimatorState _BaseServer;

	internal AnimatorStateMachine _ContainerServer;

	internal AnimatorStateMachine _ClassServer;

	internal List<AnimatorTransitionBase> mockServer;

	private static object AssetSystem;

	[SpecialName]
	internal AnimatorStateTransition[] CollectConnection()
	{
		if (_MappingServer)
		{
			return _BaseServer.transitions;
		}
		return null;
	}

	[SpecialName]
	internal AnimatorTransition[] ListConnection()
	{
		if (!_MappingServer)
		{
			return _ClassServer.GetStateMachineTransitions(_ContainerServer);
		}
		return null;
	}

	internal RepositoryServer(AnimatorState def)
	{
		_BaseServer = def;
		_ClassServer = (_ContainerServer = null);
		mockServer = new List<AnimatorTransitionBase>();
		_MappingServer = true;
	}

	internal RepositoryServer(AnimatorStateMachine key, AnimatorStateMachine second)
	{
		_BaseServer = null;
		_ContainerServer = second;
		_ClassServer = key;
		mockServer = new List<AnimatorTransitionBase>();
		_MappingServer = false;
	}

	internal static bool SelectSystem()
	{
		return AssetSystem == null;
	}
}
