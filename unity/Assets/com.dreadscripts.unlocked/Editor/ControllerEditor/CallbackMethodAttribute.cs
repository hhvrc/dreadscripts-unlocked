// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/CallbackMethodAttribute.cs

using System;

namespace DreadScripts.ControllerEditor
{
    /// <summary>Marks a method as a general pipeline callback.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class CallbackMethodAttribute : CallbackAttribute
    {
        public CallbackMethodAttribute(int priority = 0)
        {
            this.priority = priority;
        }

        public CallbackMethodAttribute(object[] args, int priority)
        {
            this.args = args;
            this.priority = priority;
        }
    }
}
