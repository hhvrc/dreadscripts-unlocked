// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/CallbackAttribute.cs

using System;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Base for the attributes that mark a method as a callback to be discovered by reflection and
    /// invoked at a particular point in the generation pipeline.
    /// </summary>
    internal abstract class CallbackAttribute : Attribute
    {
        /// <summary>Arguments to pass when invoking the marked method, or null for none.</summary>
        internal object[] args;

        /// <summary>Callbacks run in ascending priority order.</summary>
        internal int priority;
    }
}
