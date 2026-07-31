using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class ControllerCallbackAttribute : CallbackAttribute
{
	public ControllerCallbackAttribute(int no__task = 0)
	{
		_IssuerServer = no__task;
	}

	public ControllerCallbackAttribute(object[] info, int next_col)
	{
		m_IndexerServer = info;
		_IssuerServer = next_col;
	}
}
