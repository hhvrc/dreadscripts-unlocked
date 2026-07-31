using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class CallbackMethodAttribute : CallbackAttribute
{
	public CallbackMethodAttribute(int key_Ptr = 0)
	{
		priority = key_Ptr;
	}

	public CallbackMethodAttribute(object[] setup, int flags_token)
	{
		args = setup;
		priority = flags_token;
	}
}
