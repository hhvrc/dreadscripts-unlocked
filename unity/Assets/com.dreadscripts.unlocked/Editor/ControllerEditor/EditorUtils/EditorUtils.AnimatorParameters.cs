// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static MapPredicate       -> GetDefaultValue,                     line 3462
//   static ValidatePredicate  -> SetDefaultValue,                     line 3473
//   static CustomizePredicate -> CreateParameter,                     line 3491
//   static RatePredicate      -> CopyParameter(controller, param),    line 3505
//   static DestroyPredicate   -> CopyParameter(..., out bool added),  line 3511
//   static GetPredicate       -> Clone,                               line 3529
//   static CalcPredicate      -> GetOrAddParameter(...),              line 3541
//   static IncludePredicate   -> GetOrAddParameter(..., out bool added), line 3547
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// AnimatorControllerParameter carries three separate default fields -- defaultBool, defaultInt and
// defaultFloat -- of which only the one matching its type is ever read. GetDefaultValue and
// SetDefaultValue collapse that into a single float so a caller can move a default between
// parameters without switching on the type, which is what the layer/parameter copy path does.
//
// The two "out bool added" overloads report whether a parameter was created or merely matched by
// name; both leave an existing parameter untouched, including its default, and warn to the console
// when the name matched but the type did not.
//
// Not ported from this region: RunPredicate (line 3584). It reflects an internal
// AnimatorController method into a static MethodInfo cache, but the assignment to that cache sits
// inside a `while (true)` de4dot produced from a flattened `if`, so the decompilation never names
// the method it binds and never assigns the field -- as written it can only ever return false or
// hang. Reconstructing it would mean inventing the member name, so it is left out; the whole of
// the reconstructed package compiles without it. See "Shapes of decompile damage" in RE_NOTES.md.

using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The parameter's default, as a float regardless of its type: a bool default becomes 1 or
        /// 0, and a trigger -- which has no default -- becomes 0.
        /// </summary>
        internal static float GetDefaultValue(this AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    return parameter.defaultBool ? 1f : 0f;
                case AnimatorControllerParameterType.Float:
                    return parameter.defaultFloat;
                case AnimatorControllerParameterType.Int:
                    return parameter.defaultInt;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Sets the default field matching the parameter's type from a single float: anything above
        /// zero is true for a bool, and an int is rounded rather than truncated. A trigger is left
        /// alone.
        /// </summary>
        internal static void SetDefaultValue(this AnimatorControllerParameter parameter, float value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    parameter.defaultBool = value > 0f;
                    break;
                case AnimatorControllerParameterType.Int:
                    parameter.defaultInt = Mathf.RoundToInt(value);
                    break;
                case AnimatorControllerParameterType.Float:
                    parameter.defaultFloat = value;
                    break;
            }
        }

        /// <summary>
        /// Adds a parameter to the controller unconditionally -- no name check, so this will
        /// produce a duplicate if one already exists. Use <see cref="GetOrAddParameter"/> unless
        /// that is what you want. Named CreateParameter rather than AddParameter because
        /// AnimatorController already has an instance AddParameter(string, type), which would win
        /// over an extension method of that name and silently return void instead.
        /// </summary>
        /// <remarks>
        /// All three default fields are written, not just the one matching
        /// <paramref name="type"/>, so the parameter survives a later type change with its value
        /// intact. The bool default is <c>value != 0</c> and the int default truncates, which is
        /// deliberately not the rounding <see cref="SetDefaultValue"/> does.
        /// </remarks>
        internal static AnimatorControllerParameter CreateParameter(this AnimatorController controller, string name,
            AnimatorControllerParameterType type, float value = 0f)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = name,
                type = type,
                defaultBool = value != 0f,
                defaultInt = (int)value,
                defaultFloat = value
            };

            controller.AddParameter(parameter);
            return parameter;
        }

        /// <summary>
        /// Copies <paramref name="parameter"/> onto <paramref name="controller"/>, reusing an
        /// existing parameter of the same name if there is one.
        /// </summary>
        internal static AnimatorControllerParameter CopyParameter(this AnimatorController controller,
            AnimatorControllerParameter parameter)
        {
            return controller.CopyParameter(parameter, out bool _);
        }

        /// <summary>
        /// <see cref="CopyParameter(AnimatorController, AnimatorControllerParameter)"/>, reporting
        /// in <paramref name="added"/> whether a new parameter was created.
        /// </summary>
        internal static AnimatorControllerParameter CopyParameter(this AnimatorController controller,
            AnimatorControllerParameter parameter, out bool added)
        {
            return controller.GetOrAddParameter(parameter.name, parameter.type, parameter.GetDefaultValue(), out added);
        }

        /// <summary>A detached copy of the parameter, not attached to any controller.</summary>
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

        /// <summary>
        /// The controller's parameter named <paramref name="name"/>, adding it if absent.
        /// </summary>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller, string name,
            AnimatorControllerParameterType type, float value)
        {
            return controller.GetOrAddParameter(name, type, value, out bool _);
        }

        /// <summary>
        /// The controller's parameter named <paramref name="name"/>, adding it if absent and
        /// reporting in <paramref name="added"/> which of the two happened.
        /// </summary>
        /// <remarks>
        /// An existing parameter is returned as-is even when its type differs from
        /// <paramref name="type"/> -- the mismatch is only warned about, because changing the type
        /// of a parameter already referenced by transitions would silently break them.
        /// </remarks>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller, string name,
            AnimatorControllerParameterType type, float value, out bool added)
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

                added = false;
                return existing;
            }

            added = true;
            return controller.CreateParameter(name, type, value);
        }
    }
}
