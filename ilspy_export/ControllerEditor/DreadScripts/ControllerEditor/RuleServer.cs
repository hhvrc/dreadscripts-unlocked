using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class RuleServer : CallbackServer
{
	internal static RuleServer LogoutSystem;

	public RuleServer(int no__task = 0)
	{
		_IssuerServer = no__task;
	}

	public RuleServer(object[] info, int next_col)
	{
		m_IndexerServer = info;
		_IssuerServer = next_col;
	}

	internal static bool FindSystem()
	{
		return LogoutSystem == null;
	}
}
