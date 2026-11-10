// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static MapPredicate       -> GetDefaultValue,     line 3462
//   static ValidatePredicate  -> SetDefaultValue,     line 3473
//   static CustomizePredicate -> AddNewParameter,     line 3491
//   static CalcPredicate      -> GetOrAddParameter,   line 3541
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// These four are the remainder of the animator-parameter family whose core -- GetOrAddParameter's
// three other entry points and Clone -- is in EditorUtils.AnimatorParameters.cs, and were recorded
// in that file's header as left for their proper owner. They are collected here rather than there
// because they share one subject the core does not: a parameter's *default value*, and the fact
// that a parameter stores it in one of three separate typed fields.
//
// CalcPredicate is GetOrAddParameter with the "was it added?" flag discarded, so it is ported as a
// fourth overload of that name rather than as a verb of its own; C# has no optional `out`, which is
// the only reason the shipped build needed a second method at all.
//
// CustomizePredicate is NOT another spelling of GetOrAddParameter and must not be collapsed into
// it: it skips the existence check entirely and always calls AddParameter. Hence the distinct name.
// Audit status: VERIFIED -- all four bodies diffed statement by statement against export/. Two
// shape-only differences, both behaviour-preserving: GetDefaultValue's decompiled form is a switch
// *expression* written here as a switch statement, and SetDefaultValue's decompiled form carries an
// empty `case (AnimatorControllerParameterType)2:` (Trigger) that is dropped as a no-op.

using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Reads a parameter's default value as a float, whichever of the three typed default fields
        /// actually holds it.
        /// </summary>
        /// <remarks>
        /// A bool reads back as 1 or 0 and a trigger as 0, which makes the result directly comparable
        /// with the float an <see cref="AnimatorCondition"/> threshold carries. The round trip
        /// through this method and <see cref="SetDefaultValue"/> is lossy for an int outside float's
        /// exact-integer range, which no realistic animator parameter reaches.
        /// </remarks>
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
        /// Writes <paramref name="value"/> into whichever default field matches the parameter's type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only the matching field is written, so unlike the parameter *creation* helpers -- which
        /// fill all three -- this leaves the other two holding whatever they held before. That is
        /// visible if a parameter's type is later changed: the old type's default comes back.
        /// </para>
        /// <para>
        /// A bool takes any value greater than zero as true, so a negative value is false. A trigger
        /// is a no-op: it has no default to set.
        /// </para>
        /// <para>
        /// This mutates the parameter object only. If the parameter came from a controller's
        /// <c>parameters</c> array the change reaches the asset, because Unity hands out live
        /// references, but nothing here calls <see cref="UnityEditor.EditorUtility.SetDirty"/> or
        /// records an <see cref="UnityEditor.Undo"/> step, so the caller owns both.
        /// </para>
        /// </remarks>
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
        /// Adds a parameter to <paramref name="controller"/> without checking whether one of that
        /// name is already there.
        /// </summary>
        /// <remarks>
        /// <para>
        /// ASSET MUTATION: <see cref="AnimatorController.AddParameter(AnimatorControllerParameter)"/>
        /// writes straight through to the controller asset. No <see cref="UnityEditor.Undo"/> step is
        /// registered.
        /// </para>
        /// <para>
        /// Unity permits duplicate parameter names and this method makes no attempt to prevent one.
        /// A duplicate is not harmless: conditions and parameter drivers bind by name, so the second
        /// parameter is unreachable and the animator window shows two rows that cannot be told apart.
        /// Use
        /// <see cref="GetOrAddParameter(AnimatorController, string, AnimatorControllerParameterType, float)"/>
        /// unless the caller has already established that the name is free.
        /// </para>
        /// <para>
        /// <paramref name="defaultValue"/> is written into all three typed fields so that whichever
        /// one matches <paramref name="type"/> is right. That means the bool default is
        /// "any non-zero value", including a negative one -- which is not the same rule
        /// <see cref="SetDefaultValue"/> uses -- and the int default is truncated toward zero rather
        /// than rounded, so 0.9 becomes 0. Both are as shipped.
        /// </para>
        /// </remarks>
        internal static AnimatorControllerParameter AddNewParameter(this AnimatorController controller,
            string name, AnimatorControllerParameterType type, float defaultValue = 0f)
        {
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
        /// Returns the parameter named <paramref name="name"/>, adding it when it is absent, for
        /// callers that do not care which of the two happened.
        /// </summary>
        internal static AnimatorControllerParameter GetOrAddParameter(this AnimatorController controller,
            string name, AnimatorControllerParameterType type, float defaultValue)
        {
            bool wasAdded;
            return controller.GetOrAddParameter(name, type, defaultValue, out wasAdded);
        }
    }
}
