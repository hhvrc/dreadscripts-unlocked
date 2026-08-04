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
//     Process(willCreateIfNull)                        -> Process, line 1497
//     Draw(onSelected, label)                          -> Draw, line 1523
//     DrawWarning()                                    -> DrawWarning, line 1535
//     DrawCounter()                                    -> DrawCounter, line 1546
//     OnParametersSelected(parameters, onSelected)     -> OnParametersSelected, line 1554
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
//                             reached through EditorUtils.MaxParameterCost (decompiled RunError,
//                             line 7691), which reads MAX_PARAMETER_COST off the SDK type by
//                             reflection and caches it, falling back to a literal 256 only if the
//                             field cannot be found. So the limit is the SDK's live value, not a
//                             copy of it; the 256 is a last resort for an SDK whose field has been
//                             renamed, not the normal path.
// The reflection on MAX_PARAMETER_COST is not incidental. The field is a const, so writing
// VRCExpressionParameters.MAX_PARAMETER_COST directly would bake the value in at the time this
// package was compiled and keep using it against every later SDK; reading it by reflection at
// runtime is what actually follows the installed SDK. The same reasoning applies to any future
// reference to it from this package. Process() below therefore never writes a limit of its own: it
// gets the free space from EditorUtils.GetRemainingCost and, for the message it shows when that
// space runs out, quotes EditorUtils.MaxParameterCost. Both resolve to the same reflected value.
//
// The members of the outer EditorUtils class this type calls into, and where they now live. These
// are cross-references, not claims -- each of those members is mapped by the file that ports it:
//   InterruptList (line 7724)          -> GetRemainingCost (EditorUtils.Parameters.cs)
//   RunError (line 7691)               -> MaxParameterCost (EditorUtils.Parameters.cs)
//   PopRules (line 4302)               -> AssetField of T (EditorUtils.Fields.cs)
//   CallRules (line 4427)              -> IsMissing (EditorUtils.Fields.cs)
//   configurationProperty / _WrapperProcessor (lines 2178, 2182) -> validColor / warningColor (EditorUtils.Colors.cs)
//   CallError (line 8000)              -> SetExpressionParameters (EditorUtils.AvatarDescriptor.cs)
//
//   CompareCandidate  -> NOT PORTED, line 1436 -- an always-null static, half of an
//                        obfuscator-injected null check with no callers
//   PublishCandidate  -> NOT PORTED, line 1575 -- the other half of that check
//
// Audit status: PARTIAL -- every line number and member name in this header was re-checked against
// decompiled/ in this pass; the method bodies were not re-diffed statement by statement.

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
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
            /// Compares the accumulated <see cref="costs"/> against what the target asset has left
            /// and records the verdict in <see cref="validation"/>.
            /// </summary>
            /// <param name="willCreateIfNull">
            /// What an absent asset means. Passed straight through to
            /// <see cref="GetRemainingCost"/>: true says the caller is about to create the asset, so
            /// the whole budget is free; false says a missing asset has no room at all.
            /// </param>
            /// <remarks>
            /// The checks are ordered so the first thing missing is the one reported, and each
            /// failure carries its own <see cref="ValidationResult.errorCode"/>: 1 for a missing
            /// parameters asset, 2 for over budget. The missing-avatar case keeps the code 0 it gets
            /// by default, which is how the original has it.
            /// <para>
            /// The budget itself is never named here — <see cref="GetRemainingCost"/> resolves it
            /// from the installed SDK, which is the whole point of the reflection described at the
            /// top of this file. The synced-only measure is requested explicitly, so the figure
            /// matches what VRChat actually enforces at upload.
            /// </para>
            /// </remarks>
            internal void Process(bool willCreateIfNull = false)
            {
                if (useAvatar && avatar == null)
                {
                    isWithinLimit = false;
                    validation = new ValidationResult(false, "Avatar is not set (Null)");
                    return;
                }

                if (parameters == null)
                {
                    isWithinLimit = false;
                    validation = new ValidationResult(false, "Expression Parameters is not set (Null)", 1);
                    return;
                }

                availableCost = parameters.GetRemainingCost(syncedOnly: true, treatMissingAsEmpty: willCreateIfNull);
                requestedCost = costs.Sum(c => c.cost);
                remainingCost = availableCost - requestedCost;
                isWithinLimit = remainingCost >= 0;

                validation = isWithinLimit
                    ? new ValidationResult(true, "Success")
                    // The stray '$' in front of the interpolation is deliberate and is not a typo in
                    // this reconstruction: the shipped message really does read "...memory of $256".
                    // It is user-visible, so it is carried across rather than tidied.
                    : new ValidationResult(false,
                        $"Adding {requestedCost} bits of parameters would exceed the maximum parameters memory of ${MaxParameterCost}", 2);

                // Only the labels are listed, not their individual costs; the original builds the
                // breakdown from c.Item1 alone. So the tooltip says what is being charged for, and
                // the counter beside it says how much in total.
                tooltip = $"Remaining: {remainingCost}\n" + string.Join("\n", costs.Select(c => c.label));
            }

            /// <summary>
            /// Draws the "Target Parameters:" asset row, with the cost counter in its right-hand
            /// group and the validity badge from the last <see cref="Process"/>.
            /// </summary>
            /// <param name="onSelected">
            /// Raised with whatever the user picks. This type never stores the pick itself — the
            /// caller owns the field being edited.
            /// </param>
            /// <remarks>
            /// The text drawn inside the field is chosen rather than taken from the asset's name, so
            /// that an avatar's own parameters asset reads as "[Avatar's Parameters]" instead of
            /// repeating a filename the user did not choose, and so that an unassigned field and a
            /// deleted asset are distinguishable — which is what <see cref="IsMissing"/> is for.
            /// <para>
            /// <c>this</c> is copied into a local before being captured, matching the original.
            /// Because this is a struct that copy is a snapshot: the selection callback sees the
            /// state as it was when the row was drawn, and any mutation it makes is made to the copy
            /// and discarded. That is deliberate enough to depend on — <see cref="OnParametersSelected"/>
            /// only reads <see cref="avatar"/> and <see cref="useAvatar"/>, and only forwards.
            /// </para>
            /// </remarks>
            internal void Draw(Action<VRCExpressionParameters> onSelected, string label = "Target Parameters:")
            {
                bool isAvatarsOwn = useAvatar && avatar != null && avatar.expressionParameters == parameters;

                string valueText;
                if (parameters.IsMissing(out bool isDestroyed))
                {
                    valueText = isDestroyed
                        ? (isAvatarsOwn ? "[Avatar's Parameters Are Missing!]" : "Parameters Are Missing!")
                        : "No Parameters Selected";
                }
                else
                {
                    // The useAvatar test is redundant -- isAvatarsOwn already implies it -- but it is
                    // what the original tests, so it is left in place.
                    valueText = (useAvatar && isAvatarsOwn) ? "[Avatar's Parameters]" : parameters.name;
                }

                ParameterCostTracker snapshot = this;
                AssetField(label, valueText, parameters, p => snapshot.OnParametersSelected(p, onSelected),
                    validation, DrawCounter, OnParametersCreated, allowNull);
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
            /// Draws the "spent/available" bit counter that sits inside the asset row, tinted by
            /// whether the pending additions still fit.
            /// </summary>
            /// <remarks>
            /// Green for within budget, yellow for over it — over budget is a warning rather than an
            /// error because nothing has been written yet and the user can still take something out.
            /// The breakdown <see cref="Process"/> built hangs off this label as its tooltip, so the
            /// bare pair of numbers can be accounted for.
            /// </remarks>
            private void DrawCounter()
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.FG, isWithinLimit, validColor, warningColor))
                {
                    GUILayout.Label(new GUIContent($"{requestedCost}/{availableCost}", tooltip), styles.noteRight,
                        GUILayout.ExpandWidth(expand: false));
                }
            }

            /// <summary>
            /// Forwards the user's pick to the caller, then offers to adopt it as the avatar's own
            /// parameters asset when the avatar has none.
            /// </summary>
            /// <remarks>
            /// The offer is a context menu with a single "Yes" item under a nested heading, so the
            /// question is asked without a modal dialog and dismissing it — clicking anywhere else —
            /// is the "no" answer. It only appears when the avatar's slot is genuinely empty; an
            /// avatar that already has a parameters asset is never asked, so this can never silently
            /// replace one.
            /// <para>
            /// Accepting calls <see cref="SetExpressionParameters"/>, which writes the descriptor
            /// directly and marks it dirty <em>without</em> registering an <see cref="Undo"/> — so
            /// the assignment cannot be reverted with Ctrl+Z — and drops whatever reference the slot
            /// held without recording it anywhere. Here the slot is empty by the time the item can be
            /// clicked, so there is nothing to lose in practice; both traits are the original's and
            /// are left alone rather than quietly improved.
            /// </para>
            /// <para>
            /// The avatar is copied into a local before the closure captures it, because this method
            /// runs on a snapshot of the struct (see <see cref="Draw"/>) whose fields must not be
            /// reached through <c>this</c> from a delegate invoked later.
            /// </para>
            /// </remarks>
            private void OnParametersSelected(VRCExpressionParameters selected, Action<VRCExpressionParameters> onSelected)
            {
                onSelected(selected);

                if (useAvatar && avatar != null && avatar.expressionParameters == null)
                {
                    VRCAvatarDescriptor targetAvatar = avatar;
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Set As Avatar's Parameters?/Yes"), on: false, delegate
                    {
                        targetAvatar.SetExpressionParameters(selected);
                    });
                    menu.ShowAsContext();
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
