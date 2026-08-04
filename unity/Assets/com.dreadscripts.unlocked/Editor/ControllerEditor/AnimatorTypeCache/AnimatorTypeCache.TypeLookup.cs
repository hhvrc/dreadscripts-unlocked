// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTypeCache.cs
//   typeCache               -> typeCache,               line 487
//   sdkAvailable            -> sdkAvailable,            line 489
//   hasChecked              -> hasChecked,              line 491
//   GetAvatarDescriptorType -> AvatarDescriptorType,    line 494
//   GetParameterDriverType  -> ParameterDriverType,     line 500
//   GetTrackingControlType  -> TrackingControlType,     line 506
//   IsVRCSDKAvailable       -> IsVRCSDKAvailable,       line 512
//   ResolveVRCType          -> ResolveVRCType,          line 521
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The decompiled source is a single 547-line class. It is split here into partial files by aspect,
// following the EditorUtils precedent: this file holds the type lookup itself, and one file per
// nested binding type holds the rest. Nothing is left unported.
//
// Dependency not yet in the package: EditorUtils.ForgotRules(string) - the decompiled name for the
// "resolve a type by name, searching every loaded assembly, null if absent" helper in
// EditorUtils.cs (line 5285). It is called here as EditorUtils.FindType; if the EditorUtils
// reflection region is ported under a different name, this call site follows it.
// Audit status: VERIFIED against decompiled/ member-by-member (2026-08-04). AnimatorTypeCache is
// COMPLETE: all six nested types plus the outer type-lookup body are ported across the five files
// in this folder; there are no compiler-generated closures in this class.

using System;
using System.Collections.Generic;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Resolves the VRChat SDK's avatar types by name, and remembers whether the SDK is present at
    /// all.
    /// </summary>
    /// <remarks>
    /// ControllerEditor is shipped as a compiled assembly that must load in projects with or without
    /// the VRChat SDK, so it cannot hold an assembly reference to it: a direct reference would make
    /// the whole tool fail to load wherever the SDK is absent, and the SDK moves types between
    /// assemblies across releases. Every SDK type is therefore reached by name through reflection,
    /// and every SDK component is read and written through <see cref="UnityEditor.SerializedObject"/>
    /// rather than through its own API — see the nested binding types, which name the SDK's
    /// serialized fields as strings for the same reason.
    /// </remarks>
    internal static partial class AnimatorTypeCache
    {
        /// <summary>The type name probed to decide whether the SDK is installed.</summary>
        private const string avatarDescriptorTypeName = "VRCAvatarDescriptor";

        /// <summary>
        /// Resolved types by name. Null until the first lookup, which is also what marks the SDK
        /// probe as not yet run.
        /// </summary>
        private static Dictionary<string, Type> typeCache;

        private static bool sdkAvailable;

        private static bool hasChecked;

        /// <summary>The SDK's avatar descriptor component type, or null when the SDK is absent.</summary>
        internal static Type AvatarDescriptorType
        {
            get
            {
                return ResolveVRCType(avatarDescriptorTypeName);
            }
        }

        /// <summary>
        /// The parameter-driver state behaviour type, or null when the SDK is absent.
        /// </summary>
        internal static Type ParameterDriverType
        {
            get
            {
                return ResolveVRCType("VRCAvatarParameterDriver");
            }
        }

        /// <summary>
        /// The animator tracking-control state behaviour type, or null when the SDK is absent.
        /// </summary>
        internal static Type TrackingControlType
        {
            get
            {
                return ResolveVRCType("VRCAnimatorTrackingControl");
            }
        }

        /// <summary>Whether the VRChat SDK is present in the project.</summary>
        internal static bool IsVRCSDKAvailable()
        {
            if (!hasChecked)
            {
                ResolveVRCType(avatarDescriptorTypeName);
            }

            return sdkAvailable;
        }

        /// <summary>
        /// Resolves an SDK type by name, caching the result — including a miss, so an absent type is
        /// searched for only once.
        /// </summary>
        /// <remarks>
        /// The first call probes for the avatar descriptor and treats it as the presence test for the
        /// SDK as a whole: if it is missing, every later lookup short-circuits to null without
        /// scanning the loaded assemblies again. That verdict stands for the lifetime of the domain,
        /// so installing the SDK requires a domain reload before the tool sees it — which Unity does
        /// anyway on import.
        /// </remarks>
        internal static Type ResolveVRCType(string typeName)
        {
            hasChecked = true;

            if (typeCache == null)
            {
                typeCache = new Dictionary<string, Type>();

                Type descriptorType = EditorUtils.FindType(avatarDescriptorTypeName);
                if (descriptorType != null)
                {
                    sdkAvailable = true;
                    typeCache.Add(avatarDescriptorTypeName, descriptorType);
                }
            }

            if (sdkAvailable)
            {
                if (typeCache.TryGetValue(typeName, out Type cachedType))
                {
                    return cachedType;
                }

                Type resolvedType = EditorUtils.FindType(typeName);
                typeCache.Add(typeName, resolvedType);
                return resolvedType;
            }

            return null;
        }
    }
}
