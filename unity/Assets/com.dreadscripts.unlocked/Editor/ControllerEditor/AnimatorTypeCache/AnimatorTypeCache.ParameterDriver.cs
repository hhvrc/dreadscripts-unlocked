// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTypeCache.cs
//   ParameterDriverBinding             -> ParameterDriverBinding,   line 12
//     GetLocalOnly / SetLocalOnly      -> LocalOnly,                line 184
//     GetParameter                     -> GetParameter,             line 209
//     RemoveParameter                  -> RemoveParameter,          line 214
//     AddParameter                     -> AddParameter,             line 226
//     Apply                            -> Apply,                    line 234
//   ParameterDriverBinding.ParameterEntry            -> ParameterEntry,   line 14
//     ChangeType                                     -> ChangeType,       line 16
//     GetDeferApply / SetDeferApply                  -> DeferApply,       line 29
//     GetName / SetName                              -> Name,             line 45
//     GetSource / SetSource                          -> Source,           line 61
//     GetValue / SetValue                            -> Value,            line 90
//     GetChance / SetChance                          -> Chance,           line 106
//     GetValueMin / SetValueMin                      -> ValueMin,         line 122
//     GetValueMax / SetValueMax                      -> ValueMax,         line 138
//     GetChangeType / SetChangeType                  -> Type,             line 154
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Every Get*/Set* pair above is marked [SpecialName] in the shipped assembly - they are property
// accessors whose property definitions the obfuscator stripped, and they are restored as properties
// here.
// Audit status: VERIFIED against export member-by-member (2026-08-04).
// Note: ParameterEntry.Source's setter keeps the decompiled `while (!DeferApply)` loop, which
// matches export exactly but is a probable control-flow-flattening artefact (all seven sibling
// setters use `if`). Preserved faithfully rather than reconstructed; flagged for a preserve/fix call.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorTypeCache
    {
        /// <summary>
        /// Read/write access to a VRChat parameter-driver state behaviour without referencing its
        /// type.
        /// </summary>
        /// <remarks>
        /// The behaviour is held as a plain <see cref="StateMachineBehaviour"/> and every field is
        /// reached through its <see cref="SerializedObject"/>, so this compiles and runs in projects
        /// that do not have the SDK — see <see cref="AnimatorTypeCache"/>. Going through the
        /// serialized object also gets undo and dirtying for free, which direct field assignment on
        /// the behaviour would not.
        /// </remarks>
        internal class ParameterDriverBinding
        {
            internal StateMachineBehaviour behaviour;

            internal SerializedObject serializedObject;

            internal List<ParameterEntry> parameters = new List<ParameterEntry>();

            internal SerializedProperty parametersProperty;

            private readonly SerializedProperty localOnlyProperty;

            /// <summary>
            /// One entry in the driver's parameter list.
            /// </summary>
            /// <remarks>
            /// Each setter applies the serialized object immediately, so a value edited in the
            /// inspector survives even if nothing else forces a write. <see cref="DeferApply"/>
            /// suspends that for the duration of a multi-field edit.
            /// </remarks>
            internal class ParameterEntry
            {
                /// <summary>How the driver changes the parameter it names.</summary>
                internal enum ChangeType
                {
                    Set,
                    Add,
                    Random
                }

                internal ParameterDriverBinding driver;

                internal SerializedProperty property;

                private bool deferApply;

                internal ParameterEntry(ParameterDriverBinding driver, SerializedProperty property)
                {
                    this.driver = driver;
                    this.property = property;
                }

                /// <summary>
                /// While true, setters change the serialized property without applying. Clearing it
                /// applies once, so a group of edits costs a single apply.
                /// </summary>
                internal bool DeferApply
                {
                    get
                    {
                        return deferApply;
                    }
                    set
                    {
                        if (deferApply && !value)
                        {
                            driver.Apply();
                        }

                        deferApply = value;
                    }
                }

                /// <summary>The parameter this entry drives.</summary>
                internal string Name
                {
                    get
                    {
                        return property.FindPropertyRelative("name").stringValue;
                    }
                    set
                    {
                        property.FindPropertyRelative("name").stringValue = value;
                        if (!DeferApply)
                        {
                            driver.Apply();
                        }
                    }
                }

                /// <summary>
                /// The parameter copied from, for the driver's copy mode.
                /// </summary>
                /// <remarks>
                /// Guarded because "source" only exists on SDK versions that support copying; on
                /// older ones <see cref="SerializedProperty.FindPropertyRelative"/> returns null and
                /// the field access throws, which is reported as an empty name rather than an error.
                /// </remarks>
                internal string Source
                {
                    get
                    {
                        try
                        {
                            return property.FindPropertyRelative("source").stringValue;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    }
                    set
                    {
                        try
                        {
                            property.FindPropertyRelative("source").stringValue = value;

                            // Literal from the decompiled source, which has "while" where every
                            // other setter here has "if". Apply() does not clear DeferApply, so this
                            // does not terminate when DeferApply is false; it is almost certainly an
                            // artefact of the obfuscator's control-flow flattening rather than
                            // shipped behaviour. Transcribed as-is rather than guessed at - see the
                            // porting note on this file.
                            while (!DeferApply)
                            {
                                driver.Apply();
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                /// <summary>The value written, added, or used as the fixed operand.</summary>
                internal float Value
                {
                    get
                    {
                        return property.FindPropertyRelative("value").floatValue;
                    }
                    set
                    {
                        property.FindPropertyRelative("value").floatValue = value;
                        if (!DeferApply)
                        {
                            driver.Apply();
                        }
                    }
                }

                /// <summary>Probability the random change picks the upper of its two outcomes.</summary>
                internal float Chance
                {
                    get
                    {
                        return property.FindPropertyRelative("chance").floatValue;
                    }
                    set
                    {
                        property.FindPropertyRelative("chance").floatValue = value;
                        if (!DeferApply)
                        {
                            driver.Apply();
                        }
                    }
                }

                /// <summary>Lower bound of the random range.</summary>
                internal float ValueMin
                {
                    get
                    {
                        return property.FindPropertyRelative("valueMin").floatValue;
                    }
                    set
                    {
                        property.FindPropertyRelative("valueMin").floatValue = value;
                        if (!DeferApply)
                        {
                            driver.Apply();
                        }
                    }
                }

                /// <summary>Upper bound of the random range.</summary>
                internal float ValueMax
                {
                    get
                    {
                        return property.FindPropertyRelative("valueMax").floatValue;
                    }
                    set
                    {
                        property.FindPropertyRelative("valueMax").floatValue = value;
                        if (!DeferApply)
                        {
                            driver.Apply();
                        }
                    }
                }

                /// <summary>
                /// Which operation the entry performs. Backed by the SDK's "type" enum field, read as
                /// an index so the SDK's own enum never has to be named.
                /// </summary>
                /// <remarks>
                /// Unlike the other setters this one applies unconditionally, ignoring
                /// <see cref="DeferApply"/> — changing the operation changes which of the other
                /// fields the inspector draws, so the applied state has to be current before the next
                /// repaint.
                /// </remarks>
                internal ChangeType Type
                {
                    get
                    {
                        return (ChangeType)property.FindPropertyRelative("type").enumValueIndex;
                    }
                    set
                    {
                        property.FindPropertyRelative("type").enumValueIndex = (int)value;
                        driver.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            /// <summary>Whether the driver runs only on the local player.</summary>
            internal bool LocalOnly
            {
                get
                {
                    return localOnlyProperty.boolValue;
                }
                set
                {
                    localOnlyProperty.boolValue = value;
                    Apply();
                }
            }

            internal ParameterDriverBinding(StateMachineBehaviour behaviour)
            {
                this.behaviour = behaviour;
                serializedObject = new SerializedObject(behaviour);
                parametersProperty = serializedObject.FindProperty("parameters");
                localOnlyProperty = serializedObject.FindProperty("localOnly");

                for (int i = 0; i < parametersProperty.arraySize; i++)
                {
                    parameters.Add(new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(i)));
                }
            }

            /// <summary>
            /// A fresh entry for the element at <paramref name="index"/>, which is not the instance
            /// held in <see cref="parameters"/> at that index.
            /// </summary>
            internal ParameterEntry GetParameter(int index)
            {
                return new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(index));
            }

            /// <summary>
            /// Removes an entry. Returns true when the driver has no parameters left, which is the
            /// caller's cue that the behaviour itself is now pointless.
            /// </summary>
            internal bool RemoveParameter(int index)
            {
                parameters.RemoveAt(index);
                parametersProperty.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                return parametersProperty.arraySize == 0;
            }

            internal ParameterEntry AddParameter()
            {
                parametersProperty.InsertArrayElementAtIndex(parametersProperty.arraySize);
                parameters.Add(new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1)));
                serializedObject.ApplyModifiedProperties();
                return GetParameter(parametersProperty.arraySize - 1);
            }

            internal void Apply()
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
