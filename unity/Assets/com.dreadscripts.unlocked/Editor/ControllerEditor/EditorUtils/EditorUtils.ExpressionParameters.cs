// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ManageList   -> RemoveInvalidParameters,                          line 7738
//   static PrintList    -> ValidateCanAddParameters(descriptor, asset, ...), line 7762
//   static SearchList   -> ValidateCanAddParameters(descriptor, params, ...), line 7767
//   static RevertList   -> ValidateCanAddParameters(target, asset, ...),     line 7782
//   static OrderError   -> ValidateCanAddParameters(target, assets, ...),    line 7787
//   static CompareError -> ValidateCanAddParameters(target, params, ...),    line 7794
//   static SetError     -> AddParameters(descriptor, asset, ...),            line 7826
//   static PostError    -> AddParameters(descriptor, params, ...),           line 7831
//   static SetupError   -> AddParameters(target, asset, ...),                line 7844
//   static EnableError  -> AddParameters(target, params, ...),               line 7849
//   static PublishError -> Clone,                                            line 7928
//   static PopError     -> CopyTo,                                           line 7935
//   static ComputeError -> SetNetworkSynced,                                 line 7944
//   static CancelError  -> ValidateMatches,                                  line 8017
//   static ConcatError  -> GetOrCreateExpressionParameters,                  line 7978
//   class <>c__DisplayClass465_1 -> dissolved into AddParameters' lambda,    line 2084
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// VRChat gives an avatar a fixed budget of synced parameter bits (see MaxParameterCost in
// EditorUtils.Parameters.cs), so anything that adds parameters has to check first. The five
// ValidateCanAddParameters overloads are that check at five levels of convenience -- avatar or
// asset, one source or several -- and all funnel into the same arithmetic.
//
// AddParameters is where the real work is. Its uniquing pass is the part worth reading twice: when
// asked not to allow duplicate names it does not rename each clashing parameter individually, it
// searches for one numeric suffix that clears *every* name in the batch and applies it to all of
// them. That keeps a set of parameters that belong together ("Foo/A 2", "Foo/B 2") recognisable,
// which per-parameter renaming would destroy.
//
// networkSynced is reached by reflection rather than directly: the field was added to
// VRCExpressionParameters.Parameter partway through the SDK's life, and the tool supports SDK
// versions predating it. Absent, every parameter counts as synced -- the conservative answer, since
// that is what the older SDK did.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Drops unnamed and duplicate-named parameters from the asset, keeping one entry per
        /// name.
        /// </summary>
        /// <remarks>
        /// Walks backwards so that removing an entry cannot shift one not yet examined. A
        /// consequence worth knowing: because the scan runs last-to-first, the occurrence kept out
        /// of a duplicate set is the *last* one in the array, not the first.
        /// </remarks>
        internal static void RemoveInvalidParameters(this VRCExpressionParameters parameters)
        {
            if (!parameters)
            {
                return;
            }

            List<string> seen = new List<string>();
            List<VRCExpressionParameters.Parameter> kept = parameters.parameters.ToList();

            for (int i = parameters.parameters.Length - 1; i >= 0; i--)
            {
                VRCExpressionParameters.Parameter parameter = parameters.parameters[i];
                if (string.IsNullOrEmpty(parameter.name) || seen.Contains(parameter.name))
                {
                    kept.RemoveAt(i);
                }
                else
                {
                    seen.Add(parameter.name);
                }
            }

            parameters.parameters = kept.ToArray();
            EditorUtility.SetDirty(parameters);
        }

        /// <summary>
        /// Whether every parameter of <paramref name="source"/> would fit in the avatar's budget.
        /// </summary>
        internal static ValidationResult ValidateCanAddParameters(this VRCAvatarDescriptor avatar,
            VRCExpressionParameters source, bool syncedOnly = true, bool countExisting = true)
        {
            return avatar.ValidateCanAddParameters(source == null ? null : source.parameters.ToList(), syncedOnly,
                countExisting);
        }

        /// <summary>
        /// Whether <paramref name="source"/> would fit in the avatar's budget.
        /// </summary>
        /// <param name="allowMissingTarget">
        /// Treat an avatar with no expression parameters asset as an empty one rather than an
        /// error -- for a caller that intends to create the asset if the check passes.
        /// </param>
        internal static ValidationResult ValidateCanAddParameters(this VRCAvatarDescriptor avatar,
            IEnumerable<VRCExpressionParameters.Parameter> source, bool syncedOnly = true, bool countExisting = true,
            bool allowMissingTarget = false)
        {
            if (avatar == null)
            {
                return (false, "Avatar is not set (Null)");
            }

            if (!allowMissingTarget && avatar.expressionParameters == null)
            {
                return new ValidationResult(false, "Avatar Expression Parameters are not set (Null)", 1);
            }

            return avatar.expressionParameters.ValidateCanAddParameters(source, syncedOnly, countExisting,
                allowMissingTarget);
        }

        /// <summary>
        /// Whether every parameter of <paramref name="source"/> would fit alongside
        /// <paramref name="target"/>'s.
        /// </summary>
        internal static ValidationResult ValidateCanAddParameters(this VRCExpressionParameters target,
            VRCExpressionParameters source, bool syncedOnly = true, bool countExisting = true)
        {
            return target.ValidateCanAddParameters(source?.parameters.ToList(), syncedOnly, countExisting);
        }

        /// <summary>
        /// Whether the parameters of every asset in <paramref name="sources"/> together would fit
        /// alongside <paramref name="target"/>'s. Null assets and null parameters are skipped.
        /// </summary>
        internal static ValidationResult ValidateCanAddParameters(this VRCExpressionParameters target,
            IEnumerable<VRCExpressionParameters> sources, bool syncedOnly = true, bool countExisting = true)
        {
            return target.ValidateCanAddParameters(
                sources?.Where(p => p != null).SelectMany(p => p.parameters).Where(p => p != null),
                syncedOnly, countExisting);
        }

        /// <summary>
        /// Whether <paramref name="source"/> would fit alongside <paramref name="target"/>'s
        /// parameters, given VRChat's cost limit.
        /// </summary>
        /// <param name="syncedOnly">
        /// Charge only the network-synced parameters for the existing cost. False charges every
        /// parameter, i.e. VRChat's own CalcTotalCost.
        /// </param>
        /// <param name="countExisting">
        /// Charge for a source parameter whose name already exists on the target. False skips
        /// those, because adding them will reuse the existing entry rather than allocate.
        /// </param>
        /// <param name="allowMissingTarget">
        /// Treat a null target as an empty one costing nothing, rather than an error.
        /// </param>
        /// <returns>
        /// Error code 1 for a missing target, 2 for exceeding the limit.
        /// </returns>
        internal static ValidationResult ValidateCanAddParameters(this VRCExpressionParameters target,
            IEnumerable<VRCExpressionParameters.Parameter> source, bool syncedOnly = true, bool countExisting = true,
            bool allowMissingTarget = false)
        {
            if (source == null)
            {
                // Spelling as shipped.
                return (false, "Expression Paramereters are not set (Null)");
            }

            bool noTarget = target == null;
            if (!allowMissingTarget && noTarget)
            {
                return new ValidationResult(false, "Target Expression Parameters are not set (Null)", 1);
            }

            int existingCost = noTarget ? 0 : (syncedOnly ? target.CalcSyncedTotalCost() : target.CalcTotalCost());

            int addedCost = 0;
            foreach (VRCExpressionParameters.Parameter parameter in source)
            {
                if (parameter != null && !string.IsNullOrEmpty(parameter.name) && parameter.IsNetworkSynced()
                    && (noTarget || countExisting || target.FindParameter(parameter.name) == null))
                {
                    addedCost += VRCExpressionParameters.TypeCost(parameter.valueType);
                }
            }

            if (existingCost + addedCost <= MaxParameterCost)
            {
                return (true, string.Empty);
            }

            return new ValidationResult(false, $"Expression Parameters would exceed the {MaxParameterCost} cost limit",
                2);
        }

        /// <summary>
        /// Adds every parameter of <paramref name="source"/> to the avatar's expression parameters.
        /// Throws if the avatar has none.
        /// </summary>
        internal static VRCExpressionParameters.Parameter[] AddParameters(this VRCAvatarDescriptor avatar,
            VRCExpressionParameters source, bool cleanUp = true, bool allowDuplicateNames = true)
        {
            return avatar.AddParameters(source == null ? null : source.parameters, cleanUp, allowDuplicateNames);
        }

        /// <summary>
        /// Adds <paramref name="source"/> to the avatar's expression parameters. Throws if the
        /// avatar or its parameters asset is null -- this is the commit step, past validation.
        /// </summary>
        internal static VRCExpressionParameters.Parameter[] AddParameters(this VRCAvatarDescriptor avatar,
            IEnumerable<VRCExpressionParameters.Parameter> source, bool cleanUp = true,
            bool allowDuplicateNames = true)
        {
            if (avatar == null)
            {
                throw new NullReferenceException("Avatar is not set (Null)");
            }

            if (avatar.expressionParameters == null)
            {
                throw new NullReferenceException("Avatar Expression Parameters are not set (Null)");
            }

            return avatar.expressionParameters.AddParameters(source, cleanUp, allowDuplicateNames);
        }

        /// <summary>
        /// Adds every parameter of <paramref name="source"/> to <paramref name="target"/>.
        /// </summary>
        internal static VRCExpressionParameters.Parameter[] AddParameters(this VRCExpressionParameters target,
            VRCExpressionParameters source, bool cleanUp = true, bool allowDuplicateNames = true)
        {
            return target.AddParameters(source == null ? null : source.parameters, cleanUp, allowDuplicateNames);
        }

        /// <summary>
        /// Adds <paramref name="source"/> to <paramref name="target"/>, copying each parameter
        /// rather than sharing it.
        /// </summary>
        /// <param name="cleanUp">
        /// Run <see cref="RemoveInvalidParameters"/> afterwards.
        /// </param>
        /// <param name="allowDuplicateNames">
        /// False makes the batch unique against the target's existing names by appending one shared
        /// numeric suffix to every parameter in it -- see the note at the top of this file.
        /// </param>
        /// <param name="namePrefix">Prepended to every added parameter's name.</param>
        /// <param name="nameSuffix">Appended to every added parameter's name.</param>
        /// <returns>
        /// The parameters actually added -- those whose final name was not already on the target.
        /// </returns>
        internal static VRCExpressionParameters.Parameter[] AddParameters(this VRCExpressionParameters target,
            IEnumerable<VRCExpressionParameters.Parameter> source, bool cleanUp = true,
            bool allowDuplicateNames = true, string namePrefix = "", string nameSuffix = "")
        {
            if (source == null)
            {
                // Spelling as shipped.
                throw new NullReferenceException("Expression Paramereters are not set (Null)");
            }

            if (target == null)
            {
                throw new NullReferenceException("Target Expression Parameters are not set (Null)");
            }

            if (namePrefix == null)
            {
                namePrefix = string.Empty;
            }

            if (nameSuffix == null)
            {
                nameSuffix = string.Empty;
            }

            string numberSuffix = string.Empty;
            VRCExpressionParameters.Parameter[] parameters = source.ToArray();

            if (!allowDuplicateNames)
            {
                // Try the current suffix against every name; the first clash picks a new suffix
                // from the uniqued name and the whole batch is retried with it.
                bool clash;
                do
                {
                    clash = false;
                    foreach (VRCExpressionParameters.Parameter parameter in parameters)
                    {
                        string wanted = namePrefix + parameter.name + nameSuffix + numberSuffix;
                        string unique = MakeNameUnique(wanted, s => !target.parameters.Any(p => p.name == s));
                        if (unique != wanted)
                        {
                            TryGetTrailingNumber(unique, out int number);
                            numberSuffix = $" {number}";
                            clash = true;
                            break;
                        }
                    }
                }
                while (clash);
            }

            if (namePrefix != string.Empty || nameSuffix != string.Empty || numberSuffix != string.Empty)
            {
                parameters = parameters.Select(p =>
                {
                    VRCExpressionParameters.Parameter renamed = p.Clone();
                    renamed.name = namePrefix + p.name + nameSuffix + numberSuffix;
                    return renamed;
                }).ToArray();
            }

            List<VRCExpressionParameters.Parameter> added = new List<VRCExpressionParameters.Parameter>();
            foreach (VRCExpressionParameters.Parameter parameter in parameters)
            {
                if (!target.parameters.Any(p => p.name == parameter.name))
                {
                    added.Add(parameter.Clone());
                }
            }

            target.parameters = target.parameters.Concat(added).ToArray();
            if (cleanUp)
            {
                target.RemoveInvalidParameters();
            }

            EditorUtility.SetDirty(target);
            return added.ToArray();
        }

        /// <summary>A detached copy of the parameter.</summary>
        internal static VRCExpressionParameters.Parameter Clone(this VRCExpressionParameters.Parameter parameter)
        {
            VRCExpressionParameters.Parameter copy = new VRCExpressionParameters.Parameter();
            parameter.CopyTo(copy);
            return copy;
        }

        /// <summary>
        /// Copies every field of <paramref name="parameter"/> onto <paramref name="destination"/>,
        /// including networkSynced where the SDK has it.
        /// </summary>
        internal static void CopyTo(this VRCExpressionParameters.Parameter parameter,
            VRCExpressionParameters.Parameter destination)
        {
            destination.name = parameter.name;
            destination.valueType = parameter.valueType;
            destination.saved = parameter.saved;
            destination.defaultValue = parameter.defaultValue;
            destination.SetNetworkSynced(parameter.IsNetworkSynced());
        }

        /// <summary>
        /// Sets the parameter's networkSynced flag, if the installed SDK has one. A no-op on older
        /// SDKs, where every parameter is synced and there is nothing to set.
        /// </summary>
        internal static void SetNetworkSynced(this VRCExpressionParameters.Parameter parameter, bool networkSynced)
        {
            if (parameter == null)
            {
                return;
            }

            if (!networkSyncedFieldResolved)
            {
                networkSyncedFieldResolved = true;
                networkSyncedField = parameter.GetType()
                    .GetField("networkSynced", BindingFlags.Instance | BindingFlags.Public);
            }

            networkSyncedField?.SetValue(parameter, networkSynced);
        }

        /// <summary>
        /// Whether two parameters are interchangeable.
        /// </summary>
        /// <param name="ignoreType">
        /// Accept a type difference. Only the name is then required to match.
        /// </param>
        /// <returns>
        /// Error code 1 for a name mismatch, 2 for a type mismatch. Two references to the same
        /// object, and two nulls, both pass.
        /// </returns>
        internal static ValidationResult ValidateMatches(this VRCExpressionParameters.Parameter parameter,
            VRCExpressionParameters.Parameter other, bool ignoreType = true)
        {
            if (parameter == other)
            {
                return true;
            }

            if (parameter == null || other == null)
            {
                return new ValidationResult(false, "One of the parameters is null");
            }

            if (parameter.name != other.name)
            {
                return new ValidationResult(false, "Parameters don't match by name", 1);
            }

            if (parameter.valueType != other.valueType && !ignoreType)
            {
                return new ValidationResult(false, "Parameters don't match by type.", 2);
            }

            return true;
        }

        /// <summary>
        /// The avatar's expression parameters asset, creating an empty one under
        /// <paramref name="folder"/> if it has none, and switching custom expressions on either
        /// way.
        /// </summary>
        /// <param name="duplicate">
        /// Copy an existing asset to <paramref name="folder"/> and use the copy, so edits do not
        /// touch whatever else referenced the original.
        /// </param>
        internal static VRCExpressionParameters GetOrCreateExpressionParameters(this VRCAvatarDescriptor avatar,
            string folder, bool duplicate = false)
        {
            VRCExpressionParameters parameters = avatar.expressionParameters;
            if (parameters)
            {
                if (duplicate)
                {
                    parameters = DuplicateAssetTo(parameters, PrepareAssetPath(folder, parameters.name + ".asset"));
                }
            }
            else
            {
                parameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                parameters.parameters = Array.Empty<VRCExpressionParameters.Parameter>();
                AssetDatabase.CreateAsset(parameters, PrepareAssetPath(folder, avatar.name + " Parameters.asset"));
            }

            avatar.customExpressions = true;
            avatar.expressionParameters = parameters;
            EditorUtility.SetDirty(avatar);
            return parameters;
        }
    }
}
