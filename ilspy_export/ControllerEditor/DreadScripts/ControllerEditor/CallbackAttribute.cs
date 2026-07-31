using System;

namespace DreadScripts.ControllerEditor;

internal abstract class CallbackAttribute : Attribute
{
	internal object[] m_IndexerServer;

	internal int _IssuerServer;
}
