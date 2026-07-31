using System;

namespace DreadScripts.ControllerEditor;

[AttributeUsage(AttributeTargets.Method)]
internal class ControllerCallbackAttribute : CallbackAttribute
{
	public ControllerCallbackAttribute(int no__task = 0)
	{
		priority = no__task;
	}

	public ControllerCallbackAttribute(object[] info, int next_col)
	{
		args = info;
		priority = next_col;
	}
}
