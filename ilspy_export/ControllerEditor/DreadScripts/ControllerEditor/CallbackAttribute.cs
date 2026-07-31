using System;

namespace DreadScripts.ControllerEditor;

internal abstract class CallbackAttribute : Attribute
{
	internal object[] args;

	internal int priority;
}
