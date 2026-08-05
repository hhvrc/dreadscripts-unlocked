// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ReflectionRestoreScope.cs
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/PropertyRestoreScope.cs
//
// Audit status: VERIFIED -- both copies diffed statement by statement against this file: the four
// fields, both constructors, the snapshot delegate and Dispose. The shipped anonymous snapshot
// delegate passed to ToDictionary is lifted to the named private Snapshot(string) method, and the
// two Dispose loops (one per branch of logMissingMembers) are folded into one loop with the branch
// inside it; iteration order and the accessor call made per entry are unchanged. The two shipped
// copies agree statement for statement, including the broken List<T> copy and the unreachable
// `== typeof(Array)` disjunct that the Snapshot comment documents departing from.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DreadScripts.Common
{
    /// <summary>
    /// Snapshots named members of an object on construction and writes them back on dispose. Used to
    /// borrow an internal UnityEditor object, drive it into whatever state a drawing routine needs,
    /// and hand it back untouched.
    /// </summary>
    internal sealed class ReflectionRestoreScope : IDisposable
    {
        internal readonly ReflectionAccessor accessor;

        internal readonly Dictionary<string, object> savedValues;

        /// <summary>When true, a member that does not exist is reported through the Unity console.</summary>
        internal readonly bool logMissingMembers;

        /// <summary>Set to false to keep the changes instead of rolling them back.</summary>
        internal bool restoreOnDispose = true;

        public ReflectionRestoreScope(object instance, params string[] valuesToRestore)
            : this(instance, logMissingMembers: true, valuesToRestore)
        {
        }

        public ReflectionRestoreScope(object instance, bool logMissingMembers, params string[] valuesToRestore)
        {
            this.logMissingMembers = logMissingMembers;
            accessor = new ReflectionAccessor(instance);
            savedValues = valuesToRestore.ToDictionary(name => name, Snapshot);
        }

        private object Snapshot(string name)
        {
            object value;
            if (logMissingMembers)
            {
                value = accessor[name];
            }
            else
            {
                accessor.TryGetValue(name, out value);
            }

            if (value == null)
            {
                return null;
            }

            // A List<T> would be saved by reference, so anything the caller does to it in the
            // meantime would also happen to the "saved" copy and there would be nothing left to
            // restore. Copy it instead.
            //
            // The shipped build built the copy by passing an IEnumerable<object> to the
            // List<T>(IEnumerable<T>) constructor, which reflection rejects for any T other than
            // object, so this branch always threw. Filling an empty list element by element does
            // what was meant and works for every T.
            //
            // The shipped condition also tested `|| genericTypeDefinition == typeof(Array)`, which
            // is dropped here: Array is not a generic type, so GetGenericTypeDefinition() can never
            // equal it and the disjunct was unreachable. Arrays therefore fall through to the
            // by-reference return below, in the shipped build as here.
            Type type = value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList copy = (IList)Activator.CreateInstance(type);
                foreach (object element in (IEnumerable)value)
                {
                    copy.Add(element);
                }

                return copy;
            }

            return value;
        }

        public void Dispose()
        {
            if (!restoreOnDispose)
            {
                return;
            }

            foreach (KeyValuePair<string, object> saved in savedValues)
            {
                if (logMissingMembers)
                {
                    accessor[saved.Key] = saved.Value;
                }
                else
                {
                    accessor.TrySetValue(saved.Key, saved.Value);
                }
            }
        }
    }
}
