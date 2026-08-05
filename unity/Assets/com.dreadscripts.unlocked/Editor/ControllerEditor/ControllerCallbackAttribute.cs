// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerCallbackAttribute.cs
//
// Audit status: VERIFIED -- attribute usage, base class and both constructors diffed statement by
// statement against export/. Only the obfuscated parameter names (no__task, info, next_col) differ.

using System;

namespace DreadScripts.ControllerEditor
{
    /// <summary>Marks a method as a callback that runs against the generated animator controller.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class ControllerCallbackAttribute : CallbackAttribute
    {
        public ControllerCallbackAttribute(int priority = 0)
        {
            this.priority = priority;
        }

        public ControllerCallbackAttribute(object[] args, int priority)
        {
            this.args = args;
            this.priority = priority;
        }
    }
}
