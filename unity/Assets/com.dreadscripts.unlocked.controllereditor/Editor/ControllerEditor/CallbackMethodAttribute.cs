// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/CallbackMethodAttribute.cs
//
// Audit status: VERIFIED -- attribute usage, base class and both constructors diffed statement by
// statement against export/. Only the obfuscated parameter names (key_Ptr, setup, flags_token)
// differ.

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
