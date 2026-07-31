// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_SpecificationProperty -> assetExtensions,      line 2124
//   static CancelRules             -> TryGetAssetExtension, line 4434
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The table is transcribed literally, including the entries whose spelling looks wrong -- see the
// remarks on assetExtensions. Nothing else from the surrounding decompiled region is ported here.

using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// File extension, without the leading dot, to give a newly created asset of a given type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Covers the six types the asset field can create: <see cref="AnimatorController"/>,
        /// <see cref="AnimationClip"/>, <see cref="BlendTree"/>, <see cref="Shader"/>,
        /// <see cref="Material"/> and <see cref="GameObject"/>. Anything else falls back to
        /// "asset" -- see <see cref="TryGetAssetExtension"/>.
        /// </para>
        /// <para>
        /// A plain <see cref="Dictionary{TKey,TValue}"/> with the default comparer, so the lookup
        /// matches the requested type <em>exactly</em> and never walks up the base chain. A subclass
        /// of a listed type therefore does not inherit its entry: an
        /// <c>AnimatorOverrideController</c> field gets "asset" rather than the "overrideController"
        /// Unity itself uses, and the same holds for any user-derived
        /// <see cref="ScriptableObject"/>. That is the shipped behaviour and is preserved
        /// deliberately.
        /// </para>
        /// <para>
        /// Two values are transcribed verbatim despite looking wrong, because the extension decides
        /// what Unity's importer makes of the file and a "corrected" one would silently produce an
        /// asset that cannot be opened:
        /// "blendTree" is camel-cased where every other entry is lower case, and a standalone blend
        /// tree is conventionally saved as ".asset"; and <see cref="GameObject"/> maps to "prefab",
        /// which is right for a prefab but means a plain GameObject cannot be saved through this
        /// path as anything else.
        /// </para>
        /// </remarks>
        internal static readonly Dictionary<Type, string> assetExtensions = new Dictionary<Type, string>
        {
            { typeof(AnimatorController), "controller" },
            { typeof(AnimationClip), "anim" },
            { typeof(BlendTree), "blendTree" },
            { typeof(Shader), "shader" },
            { typeof(Material), "mat" },
            { typeof(GameObject), "prefab" }
        };

        /// <summary>
        /// Resolves the file extension to use when creating a new asset of <paramref name="type"/>.
        /// </summary>
        /// <param name="extension">
        /// The extension for the type, or "asset" when the type is not one of the handful the table
        /// covers. Always set, so a caller that does not care whether the type was known can ignore
        /// the return value and use this directly.
        /// </param>
        /// <returns>True when the type had an entry of its own.</returns>
        internal static bool TryGetAssetExtension(Type type, out string extension)
        {
            if (!assetExtensions.TryGetValue(type, out extension))
            {
                extension = "asset";
                return false;
            }

            return true;
        }
    }
}
