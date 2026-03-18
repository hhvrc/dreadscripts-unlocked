using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class PrototypeServer : CallbackServer
{
	internal static PrototypeServer ConcatSystem;

	public PrototypeServer(int key_Ptr = 0)
	{
		_IssuerServer = key_Ptr;
	}

	public PrototypeServer(object[] setup, int flags_token)
	{
		m_IndexerServer = setup;
		_IssuerServer = flags_token;
	}

	internal static bool CollectSystem()
	{
		return ConcatSystem == null;
	}
}
