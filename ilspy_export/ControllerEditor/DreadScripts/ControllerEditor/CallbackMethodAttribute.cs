using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class CallbackMethodAttribute : CallbackAttribute
{
	public CallbackMethodAttribute(int key_Ptr = 0)
	{
		_IssuerServer = key_Ptr;
	}

	public CallbackMethodAttribute(object[] setup, int flags_token)
	{
		m_IndexerServer = setup;
		_IssuerServer = flags_token;
	}
}
