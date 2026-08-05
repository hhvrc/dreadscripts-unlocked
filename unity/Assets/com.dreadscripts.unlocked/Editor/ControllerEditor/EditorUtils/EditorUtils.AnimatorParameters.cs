// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static IncludePredicate -> GetOrAddParameter(AnimatorController, string, type, float, out bool), line 3547
//   static DestroyPredicate -> GetOrAddParameter(AnimatorController, AnimatorControllerParameter, out bool), line 3511
//   static RatePredicate    -> GetOrAddParameter(AnimatorController, AnimatorControllerParameter),           line 3505
//   static GetPredicate     -> Clone(AnimatorControllerParameter),                                           line 3529
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// RatePredicate and DestroyPredicate are the same operation differing only in whether the caller
// wants to know that the parameter was created, so they are ported as overloads of one name rather
// than as two unrelated verbs; C# has no optional `out`, hence two methods rather than a default.
// DestroyPredicate is not in the assigned set but is ported here because RatePredicate is nothing
// but a call to it, and IncludePredicate likewise because DestroyPredicate is nothing but a call to
// *it*. The three are one method that the obfuscator split across three entry points. Their name in
// the decompilation ("Destroy...") is noise; nothing here destroys anything.
//
// IncludePredicate's body is a while(true)/continue/break loop produced by control-flow flattening.
// It is written out below as the plain foreach it started as; the ordering -- first name match
// wins, type mismatch warns but still returns the existing parameter -- is preserved exactly.
//
// Not ported here, though they sit in the same run of the decompiled file and are the same subject:
// CustomizePredicate (3491, adds unconditionally without the existence check), CalcPredicate (3541,
// IncludePredicate discarding the out flag), MapPredicate (3462, reads a parameter's default as a
// float) and ValidatePredicate (3473, writes one). They were deferred when this file was written and
// have since landed in EditorUtils.AnimatorParameterDefaults.cs, which owns the default-value
// subject; do not re-port them here.
//
// Likewise deferred here and since landed elsewhere: ResetPredicate (line 3355) and FlushPredicate
// (line 3374), the progress-bar-wrapped bulk layer/parameter copy routines, are in
// EditorUtils.LayerCopying.cs as CopyLayers / CopyLayersAndParameters, together with
// CalculatePredicate (line 3417) and the ConnectError/CalculateError/TestError object-graph
// remapping machinery whose absence was the original blocker.
// Audit status: VERIFIED -- all four bodies diffed statement by statement against export/.
// IncludePredicate's flattened while(true)/continue/break form was walked branch by branch against
// the foreach written here: first name match wins, a type mismatch warns and still returns the
// existing parameter with wasAdded false, and the not-found path sets wasAdded true before
// constructing. The warning string is character-for-character the decompiled interpolation.

using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Returns the parameter named <paramref name="name"/> on <paramref name="controller"/>,
        /// adding it when it is not there.
        /// </summary>
        /// <param name="defaultValue">
        /// The starting value for a newly added parameter, written into all three of the typed
        /// default fields so that whichever one matches <paramref name="type"/> is correct. A
        /// non-zero value therefore means <c>true</c> for a bool and is truncated, not rounded, for
        /// an int. Ignored when the parameter already exists.
        /// </param>
        /// <param name="wasAdded">True when the parameter did not exist and has just been created.</param>
        /// <remarks>
        /// <para>
        /// An existing parameter is returned even when its type is wrong; the mismatch is only
        /// reported to the console. That is deliberate on the original's part -- the alternative,
        /// replacing the parameter, would break every transition condition and driver already
        /// pointing at it -- but it does mean a caller can be handed a parameter it cannot use, and
        /// callers that care have to check the type themselves.
        /// </para>
        /// <para>
        /// Nothing here registers an <see cref="UnityEditor.Undo"/> operation, so adding a parameter
        /// through this method is not undoable; callers that want it to be must record the
        /// controller themselves beforehand. Nor does the asset get marked dirty here -- Unity's own
        /// <c>AddParameter</c> handles the serialisation.
        /// </para>
        /// </remarks>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller,
            string name, AnimatorControllerParameterType type, float defaultValue, out bool wasAdded)
        {
            foreach (AnimatorControllerParameter existing in controller.parameters)
            {
                if (existing.name != name)
                {
                    continue;
                }

                if (existing.type != type)
                {
                    $"Type mismatch! Parameter {name} already exists in {controller.name} but with type {existing.type} rather than {type}"
                        .LogColored(LogType.Warning);
                }

                wasAdded = false;
                return existing;
            }

            wasAdded = true;
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = name,
                type = type,
                defaultBool = defaultValue != 0f,
                defaultInt = (int)defaultValue,
                defaultFloat = defaultValue
            };

            controller.AddParameter(parameter);
            return parameter;
        }

        /// <summary>
        /// Reproduces <paramref name="template"/> on <paramref name="controller"/>: returns the
        /// controller's own parameter of that name, creating one to match when it has none.
        /// </summary>
        /// <remarks>
        /// The template's default is flattened to a single float before being handed on -- bool
        /// becomes 1 or 0 -- because that is the one shape
        /// <see cref="GetOrAddParameter(AnimatorController, string, AnimatorControllerParameterType, float, out bool)"/>
        /// takes. The round trip is lossless for the type actually in use.
        /// <para>
        /// A parameter type the switch does not name -- Trigger -- falls through with a default of
        /// 0, which is the right answer for it: a trigger has no persistent value.
        /// </para>
        /// </remarks>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller,
            AnimatorControllerParameter template, out bool wasAdded)
        {
            float defaultValue = 0f;
            switch (template.type)
            {
                case AnimatorControllerParameterType.Bool:
                    defaultValue = template.defaultBool ? 1 : 0;
                    break;
                case AnimatorControllerParameterType.Float:
                    defaultValue = template.defaultFloat;
                    break;
                case AnimatorControllerParameterType.Int:
                    defaultValue = template.defaultInt;
                    break;
            }

            return controller.GetOrAddParameter(template.name, template.type, defaultValue, out wasAdded);
        }

        /// <summary>
        /// <see cref="GetOrAddParameter(AnimatorController, AnimatorControllerParameter, out bool)"/>
        /// for callers that do not care whether the parameter already existed.
        /// </summary>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller,
            AnimatorControllerParameter template)
        {
            bool wasAdded;
            return controller.GetOrAddParameter(template, out wasAdded);
        }

        /// <summary>
        /// A detached copy of <paramref name="parameter"/>, unattached to any controller.
        /// </summary>
        /// <remarks>
        /// <see cref="AnimatorControllerParameter"/> is a reference type that a controller hands out
        /// by reference, so editing the object returned by <c>controller.parameters</c> mutates the
        /// controller. This exists for the cases that need a value to edit freely first -- the
        /// rename and duplicate flows -- and commit deliberately afterwards. Every field is copied,
        /// including the two defaults that do not apply to the parameter's type, so the copy stays
        /// faithful if the type is later changed.
        /// </remarks>
        internal static AnimatorControllerParameter Clone(this AnimatorControllerParameter parameter)
        {
            return new AnimatorControllerParameter
            {
                name = parameter.name,
                type = parameter.type,
                defaultBool = parameter.defaultBool,
                defaultInt = parameter.defaultInt,
                defaultFloat = parameter.defaultFloat
            };
        }
    }
}
