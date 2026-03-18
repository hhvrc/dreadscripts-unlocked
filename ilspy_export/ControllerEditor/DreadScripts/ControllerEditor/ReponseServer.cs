using System;

namespace DreadScripts.ControllerEditor;

internal sealed class ReponseServer
{
	private bool[] _PoolServer;

	internal int m_ParameterServer = -1;

	internal Action composerServer;

	private static ReponseServer PrintSystem;

	internal ReponseServer(int setup, Action cfg = null)
	{
		CreateConnection(setup);
		composerServer = cfg;
	}

	internal void CreateConnection(int ident_Ptr)
	{
		if (_PoolServer == null || _PoolServer.Length != ident_Ptr)
		{
			_PoolServer = new bool[ident_Ptr];
		}
		if (m_ParameterServer > 0)
		{
			if (m_ParameterServer < _PoolServer.Length)
			{
				_PoolServer[m_ParameterServer] = true;
			}
			else
			{
				m_ParameterServer = -1;
			}
		}
	}

	internal void NewConnection(int max_setup)
	{
		if (max_setup >= 0 && max_setup < _PoolServer.Length && m_ParameterServer != max_setup)
		{
			if (m_ParameterServer >= 0)
			{
				_PoolServer[m_ParameterServer] = false;
			}
			m_ParameterServer = max_setup;
			composerServer?.Invoke();
			_PoolServer[m_ParameterServer] = true;
		}
	}

	internal void PushConnection(int previousvar1, bool addmap)
	{
		if (previousvar1 < 0 || previousvar1 >= _PoolServer.Length)
		{
			return;
		}
		if (m_ParameterServer == previousvar1)
		{
			if (addmap)
			{
				return;
			}
			ViewConnection();
		}
		if (m_ParameterServer >= 0 && addmap)
		{
			_PoolServer[m_ParameterServer] = false;
		}
		if (addmap)
		{
			m_ParameterServer = previousvar1;
			composerServer?.Invoke();
		}
		_PoolServer[previousvar1] = addmap;
	}

	internal void ViewConnection()
	{
		if (m_ParameterServer >= 0)
		{
			_PoolServer[m_ParameterServer] = false;
			m_ParameterServer = -1;
			composerServer?.Invoke();
		}
	}

	internal static bool ResolveSystem()
	{
		return PrintSystem == null;
	}
}
