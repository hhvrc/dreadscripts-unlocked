// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static RunError       -> MaxParameterCost (property), line 7691
//   static recordProcessor    -> cachedMaxParameterCost,      line 2232
//   static InterruptList  -> GetRemainingCost,       line 7724
//   static PatchList      -> CalcSyncedTotalCost,    line 7708
//   static MoveError      -> IsNetworkSynced,        line 7960
//   static helperProcessor    -> networkSyncedField,         line 2234
//   static m_ConsumerProcessor -> networkSyncedFieldResolved, line 2236
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// RunError carries [SpecialName] in the decompilation, i.e. it was a property getter before the
// obfuscator flattened it into a method; it is restored as a property here, matching how
// ParameterCostTracker.IsValid was handled.
//
// CalcSyncedTotalCost and IsNetworkSynced are ported here rather than reported as missing because
// GetRemainingCost cannot compile without them -- they are the entire body of its non-SDK branch.
// If another partial ends up owning them, delete them from here rather than duplicating.
// Audit status: VERIFIED against export
//
// Deliberately unported from the same region: ManageList (line 7738), which strips unnamed and
// duplicate parameters from an asset in place; it is a mutation rather than a cost query and has
// no caller in the reconstructed package yet.

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Resolved value of the SDK's parameter budget; 0 until first read.</summary>
        private static int cachedMaxParameterCost;

        /// <summary><c>VRCExpressionParameters.Parameter.networkSynced</c>, or null on an SDK that
        /// predates the field.</summary>
        private static FieldInfo networkSyncedField;

        /// <summary>Whether <see cref="networkSyncedField"/> has been looked up yet; distinguishes
        /// "not resolved" from "resolved to null".</summary>
        private static bool networkSyncedFieldResolved;

        /// <summary>
        /// The synced expression-parameter memory budget VRChat allows an avatar, in bits.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is read off <c>VRCExpressionParameters.MAX_PARAMETER_COST</c> by reflection,
        /// which looks like an over-complicated way to name a constant until you check how the SDK
        /// declares it: <c>Public, Static, Literal</c> — a C# <c>const</c>. A direct reference to a
        /// <c>const</c> is resolved by the compiler and the literal is copied into this assembly, so
        /// writing <c>VRCExpressionParameters.MAX_PARAMETER_COST</c> here would freeze whatever the
        /// budget happened to be on the machine that built this package and keep asserting it
        /// against every later SDK. Reflection is what makes the number track the installed SDK, and
        /// it is the only thing that does. Do not "simplify" this into a direct reference.
        /// </para>
        /// <para>
        /// If the lookup fails — the SDK is missing, or the field was renamed — the budget falls
        /// back to 256, the value shipped by the SDKs of this era, and a warning is logged. The
        /// direction of that error matters because this feeds a user-facing "this would exceed the
        /// maximum parameters memory" warning: if VRChat has since <em>raised</em> the budget, a
        /// stale 256 under-reports the space left and the tool refuses additions that would in fact
        /// fit — noisy, but safe. If the budget were ever <em>lowered</em>, 256 would over-report and
        /// the tool would wave through an avatar that VRChat later rejects at upload. Neither is
        /// silent corruption, but the fallback is a last resort, not a second source of truth.
        /// </para>
        /// <para>
        /// The result is cached in <see cref="cachedMaxParameterCost"/>, which doubles as the
        /// "not resolved yet" flag by holding 0. A genuine budget of 0 would therefore be looked up
        /// again on every access, which is harmless and cannot occur in practice.
        /// </para>
        /// </remarks>
        internal static int MaxParameterCost
        {
            get
            {
                if (cachedMaxParameterCost == 0)
                {
                    try
                    {
                        cachedMaxParameterCost = (int)FindType("VRCExpressionParameters")
                            .GetField("MAX_PARAMETER_COST", BindingFlags.Static | BindingFlags.Public)
                            .GetValue(null);
                    }
                    catch
                    {
                        Debug.LogWarning("Failed to dynamically get MAX_PARAMETER_COST. Falling back to 256");
                        cachedMaxParameterCost = 256;
                    }
                }

                return cachedMaxParameterCost;
            }
        }

        /// <summary>
        /// How many bits of the avatar's parameter budget <paramref name="parameters"/> leaves free.
        /// Negative when the asset is already over budget.
        /// </summary>
        /// <param name="syncedOnly">
        /// When true the spend is measured with <see cref="CalcSyncedTotalCost"/>, which counts only
        /// what is actually sent over the network; when false it is the SDK's own
        /// <c>CalcTotalCost</c>, which charges for every parameter in the asset. True gives the
        /// figure VRChat enforces; false gives the more pessimistic one, for callers that want to
        /// stay within budget even if the user later marks everything synced.
        /// </param>
        /// <param name="treatMissingAsEmpty">
        /// Decides what a null asset means. True answers with the full budget — the caller is about
        /// to create the asset, so nothing is spent yet. False answers 0, i.e. "no room", so that a
        /// caller which requires an existing asset fails its own fit check instead of proceeding
        /// against an asset that is not there.
        /// </param>
        internal static int GetRemainingCost(this VRCExpressionParameters parameters,
            bool syncedOnly = true, bool treatMissingAsEmpty = true)
        {
            if (parameters == null)
            {
                if (!treatMissingAsEmpty)
                {
                    return 0;
                }

                return MaxParameterCost;
            }

            int usedCost = syncedOnly ? parameters.CalcSyncedTotalCost() : parameters.CalcTotalCost();
            return MaxParameterCost - usedCost;
        }

        /// <summary>
        /// The bits <paramref name="parameters"/> actually spends: unnamed entries, repeats of a name
        /// already counted, and parameters that are not network-synced are all free.
        /// </summary>
        /// <remarks>
        /// This is the counterpart to the SDK's <c>CalcTotalCost</c>, which charges for every entry.
        /// The exclusions match what VRChat itself uploads, so this is the figure to compare against
        /// <see cref="MaxParameterCost"/>.
        /// <para>
        /// The per-type prices are the literal 1 and 8 the original used rather than
        /// <see cref="VRCExpressionParameters.TypeCost"/>; they agree with the SDK today, and are
        /// kept as they shipped.
        /// </para>
        /// <para>
        /// A name is remembered only when it is charged for, so a duplicate name whose first
        /// occurrence was unsynced can still be billed by a later synced occurrence. That falls out
        /// of the original's ordering and is preserved.
        /// </para>
        /// </remarks>
        internal static int CalcSyncedTotalCost(this VRCExpressionParameters parameters)
        {
            int totalCost = 0;
            List<string> countedNames = new List<string>();

            foreach (VRCExpressionParameters.Parameter parameter in parameters.parameters)
            {
                if (!string.IsNullOrEmpty(parameter.name) && !countedNames.Contains(parameter.name)
                    && parameter.IsNetworkSynced())
                {
                    countedNames.Add(parameter.name);
                    totalCost += (parameter.valueType == VRCExpressionParameters.ValueType.Bool) ? 1 : 8;
                }
            }

            return totalCost;
        }

        /// <summary>
        /// Whether <paramref name="parameter"/> is sent to other players, and so occupies part of the
        /// synced budget.
        /// </summary>
        /// <remarks>
        /// <c>networkSynced</c> was added to the SDK partway through this tool's life, so it is read
        /// by reflection and its absence is taken to mean "synced": on an SDK old enough not to have
        /// the field, every parameter was synced. The unknown cases — a null parameter, a missing
        /// field — all answer true, which over-counts rather than under-counts and keeps the caller
        /// on the conservative side of the budget.
        /// <para>
        /// The lookup is done once, off the first parameter seen, and the resulting
        /// <see cref="FieldInfo"/> is reused for every parameter afterwards.
        /// </para>
        /// </remarks>
        internal static bool IsNetworkSynced(this VRCExpressionParameters.Parameter parameter)
        {
            if (parameter == null)
            {
                return true;
            }

            if (!networkSyncedFieldResolved)
            {
                networkSyncedFieldResolved = true;
                networkSyncedField = parameter.GetType()
                    .GetField("networkSynced", BindingFlags.Instance | BindingFlags.Public);
            }

            if (networkSyncedField != null)
            {
                return (bool)networkSyncedField.GetValue(parameter);
            }

            return true;
        }
    }
}
