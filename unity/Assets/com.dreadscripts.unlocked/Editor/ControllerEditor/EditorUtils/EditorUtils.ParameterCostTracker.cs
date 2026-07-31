// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   struct ParameterCostTracker -> ParameterCostTracker, line 1412
//     IsValid()                                        -> IsValid (property; [SpecialName] in the
//                                                         decompilation)
//     Set(descriptor, parameters, allowNull)           -> Set, line 1444
//     Set(parameters, allowNull)                       -> Set, line 1451
//     Set(allowNull)                                   -> Set, line 1457
//     AddCost(label, IEnumerable<VRCExpressionParameters>, condition)           -> AddCost, line 1465
//     AddCost(label, VRCExpressionParameters, condition)                        -> AddCost, line 1473
//     AddCost(label, IEnumerable<VRCExpressionParameters.Parameter>, condition) -> AddCost, line 1481
//     AddCost(label, int, condition)                                            -> AddCost, line 1489
//     DrawWarning()                                    -> DrawWarning, line 1535
//     OnParametersCreated(parameters)                  -> OnParametersCreated, line 1569
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Where the cost numbers come from
// --------------------------------
// Nothing in this type states a bit cost or a budget of its own. Every number is asked of the
// VRChat SDK at the point of use, which is what keeps the tracker honest when VRChat changes the
// budget:
//   * per-parameter cost   -- VRCExpressionParameters.TypeCost(ValueType), the SDK's own static
//                             (1 bit for Bool, 8 for Int and Float as of SDK 3.10.4).
//   * per-asset total      -- VRCExpressionParameters.CalcTotalCost(), also the SDK's.
//   * the budget           -- 256 bits (VRCExpressionParameters.MAX_PARAMETER_COST, SDK 3.10.4),
//                             reached through the decompiled RunError() at line 7691, which reads
//                             MAX_PARAMETER_COST off the SDK type by reflection and caches it,
//                             falling back to a literal 256 only if the field cannot be found. So
//                             the limit is the SDK's live value, not a copy of it; the 256 is a
//                             last resort for an SDK whose field has been renamed, not the normal
//                             path. Preserve that when Process() below is ported -- a hard-coded
//                             limit would silently disagree with the SDK's own memory bar.
// The reflection on MAX_PARAMETER_COST is not incidental. The field is a const, so writing
// VRCExpressionParameters.MAX_PARAMETER_COST directly would bake the value in at the time this
// package was compiled and keep using it against every later SDK; reading it by reflection at
// runtime is what actually follows the installed SDK. The same reasoning applies to any future
// reference to it from this package.
//
// Deliberately unported, each because it calls an EditorUtils member that has not been ported yet:
//   Process(willCreateIfNull), line 1497 -- needs the remaining-budget query (decompiled
//     InterruptList, line 7724) and through it the reflected limit (RunError, line 7691). Note for
//     whoever ports it: the over-budget message at line 1507 reads "... maximum parameters memory
//     of ${RunError()}", with a stray literal '$' in front of the interpolation. That is in the
//     shipped binary, so it should be carried across as-is rather than tidied.
//   Draw(onSelected, label), line 1523 -- needs the shared object-picker row (decompiled PopRules /
//     ComputeRules, line 4302) and the destroyed-object test (CallRules, line 4427).
//   DrawCounter(), line 1546 -- needs the valid/invalid foreground colours (decompiled
//     configurationProperty and _WrapperProcessor, lines 2178 and 2182), which the colour partial
//     does not carry yet.
//   OnParametersSelected(...), line 1554 -- only reachable from Draw, and needs the descriptor
//     assignment that also maintains customExpressions (decompiled CallError, line 8000).
//
// Deliberately unported: the CompareCandidate / PublishCandidate() pair, line 1436 and 1575, an
// obfuscator-injected null check on an always-null static with no callers.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Accumulates the expression-parameter memory a pending operation is about to spend and
        /// weighs it against what the target asset has left.
        /// </summary>
        /// <remarks>
        /// VRChat gives every avatar a fixed synced-parameter budget, so a tool that adds
        /// parameters has to be able to say "this would not fit" before it writes anything. The
        /// intended sequence is: <see cref="Set(VRCExpressionParameters, bool)"/> to choose the
        /// target and clear the tally, one <c>AddCost</c> call per thing that will be added, then
        /// <c>Process</c> to compare the two and produce a message.
        /// <para>
        /// Each cost is recorded with the label that explains it, so the counter drawn next to the
        /// field can break the total down in its tooltip rather than just showing a number the user
        /// cannot account for.
        /// </para>
        /// </remarks>
        internal struct ParameterCostTracker
        {
            /// <summary>The avatar the parameter asset belongs to; unused when
            /// <see cref="useAvatar"/> is false.</summary>
            internal VRCAvatarDescriptor avatar;

            /// <summary>
            /// Whether <see cref="parameters"/> is being tracked as an avatar's asset rather than as
            /// a standalone one.
            /// </summary>
            internal bool useAvatar;

            /// <summary>The asset whose remaining budget is being spent.</summary>
            internal VRCExpressionParameters parameters;

            /// <summary>Bits still free on <see cref="parameters"/> before this operation.</summary>
            internal int availableCost;

            /// <summary>Bits the accumulated <see cref="costs"/> add up to.</summary>
            internal int requestedCost;

            /// <summary>
            /// <see cref="availableCost"/> minus <see cref="requestedCost"/>; negative when the
            /// operation would overflow the budget.
            /// </summary>
            internal int remainingCost;

            internal bool isWithinLimit;

            /// <summary>Whether an empty selection is an acceptable answer rather than an error.</summary>
            internal bool allowNull;

            internal ValidationResult validation;

            /// <summary>What each pending addition is called and what it costs, in the order added.</summary>
            internal List<(string label, int cost)> costs;

            /// <summary>The breakdown of <see cref="costs"/>, ready to hang off the counter label.</summary>
            internal string tooltip;

            /// <summary>Whether the last <c>Process</c> call accepted the current state.</summary>
            internal bool IsValid
            {
                get
                {
                    return validation.isValid;
                }
            }

            /// <summary>
            /// Targets an avatar's expression parameters and clears any tally from a previous pass.
            /// </summary>
            internal void Set(VRCAvatarDescriptor avatar, VRCExpressionParameters parameters, bool allowNull = false)
            {
                this.avatar = avatar;
                useAvatar = true;

                // Faithful to the decompiled source: allowNull is accepted here but not passed on,
                // so it has no effect through this entry point and the flag ends up false. It looks
                // like an oversight -- the sibling ControllerPicker.Set does forward it -- but it is
                // what shipped, so it is reproduced rather than corrected.
                Set(parameters);
            }

            /// <inheritdoc cref="Set(VRCAvatarDescriptor, VRCExpressionParameters, bool)"/>
            internal void Set(VRCExpressionParameters parameters, bool allowNull = false)
            {
                this.parameters = parameters;

                // As above: allowNull is not forwarded to the reset below.
                Set();
            }

            /// <summary>
            /// Resets the tally, leaving the target untouched.
            /// </summary>
            /// <remarks>
            /// The initial result carries error code -1, which no real failure uses, so a caller can
            /// tell "never processed" apart from any genuine outcome.
            /// </remarks>
            internal void Set(bool allowNull = false)
            {
                costs = new List<(string, int)>();
                tooltip = string.Empty;
                validation = new ValidationResult(false, "Unknown Error", -1);
                this.allowNull = allowNull;
            }

            /// <summary>
            /// Adds the combined cost of every parameter in each of <paramref name="assets"/>.
            /// </summary>
            /// <param name="condition">
            /// When false the cost is not recorded at all. Lets a call site express "charge for this
            /// only if the user asked for it" inline, without wrapping the call in an <c>if</c>.
            /// </param>
            internal void AddCost(string label, IEnumerable<VRCExpressionParameters> assets, bool condition = true)
            {
                if (condition)
                {
                    AddCost(label, assets.Sum(p => p.CalcTotalCost()));
                }
            }

            /// <summary>Adds the combined cost of every parameter in <paramref name="asset"/>.</summary>
            /// <inheritdoc cref="AddCost(string, IEnumerable{VRCExpressionParameters}, bool)"/>
            internal void AddCost(string label, VRCExpressionParameters asset, bool condition = true)
            {
                if (condition)
                {
                    AddCost(label, asset.CalcTotalCost());
                }
            }

            /// <summary>
            /// Adds the cost of a loose set of parameters that are not yet part of any asset.
            /// </summary>
            /// <remarks>
            /// Priced with the SDK's own <see cref="VRCExpressionParameters.TypeCost"/> rather than a
            /// local bits-per-type table, so the figure shown here always agrees with the memory bar
            /// on the parameters asset inspector.
            /// </remarks>
            /// <inheritdoc cref="AddCost(string, IEnumerable{VRCExpressionParameters}, bool)"/>
            internal void AddCost(string label, IEnumerable<VRCExpressionParameters.Parameter> parameters,
                bool condition = true)
            {
                if (condition)
                {
                    AddCost(label, parameters.Sum(p => VRCExpressionParameters.TypeCost(p.valueType)));
                }
            }

            /// <summary>Records an already-known cost in bits under <paramref name="label"/>.</summary>
            /// <inheritdoc cref="AddCost(string, IEnumerable{VRCExpressionParameters}, bool)"/>
            internal void AddCost(string label, int cost, bool condition = true)
            {
                if (condition)
                {
                    costs.Add((label, cost));
                }
            }

            /// <summary>
            /// Draws a warning icon carrying the failure message, and nothing at all when the last
            /// check passed.
            /// </summary>
            internal void DrawWarning()
            {
                if (!IsValid)
                {
                    GUILayout.Label(new GUIContent(contents.warning)
                    {
                        tooltip = validation.message
                    }, styles.iconButton);
                }
            }

            /// <summary>
            /// Prepares an expression parameters asset the user just created from the picker.
            /// </summary>
            /// <remarks>
            /// A newly created asset arrives with the SDK's default starter parameters. Emptying it
            /// matters because the tracker is about to charge the user for what it is going to add,
            /// and those defaults would otherwise silently occupy part of the budget.
            /// </remarks>
            private void OnParametersCreated(VRCExpressionParameters parameters)
            {
                parameters.parameters = Array.Empty<VRCExpressionParameters.Parameter>();
                EditorUtility.SetDirty(parameters);
            }
        }
    }
}
