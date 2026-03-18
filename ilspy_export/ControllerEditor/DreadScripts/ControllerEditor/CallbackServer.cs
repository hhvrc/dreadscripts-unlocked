using System;

namespace DreadScripts.ControllerEditor;

internal abstract class CallbackServer : Attribute
{
	internal object[] m_IndexerServer;

	internal int _IssuerServer;

	internal static CallbackServer DisableSystem;

	internal static bool VerifySystem()
	{
		return DisableSystem == null;
	}
}
