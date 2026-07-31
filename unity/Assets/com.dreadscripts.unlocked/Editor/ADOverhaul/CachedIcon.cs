// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//
// Ported region: the CachedIcon class, lines 1148-1262 of the current snapshot. Line numbers move
// with the snapshot; the member names below are the durable reference.
//
//   GetContent()   -> content (property), line 1161
//   GetTexture()   -> texture (property), line 1174
//   ResolveTexture -> ResolveTexture,     line 1204
//   LoadFromCache  -> LoadFromCache,      line 1230
//   SaveToCache    -> SaveToCache,        line 1252
//   ToBytes/ToInts -> not ported; see the note on the cache helpers below.
//
// The decompiled class is nested inside the static class ADOEditorUtility. ADOEditorUtility is not
// ported yet, so this is lifted to a top-level type in the same namespace, as PhysBoneParameter
// already is; call sites in the original read `ADOEditorUtility.CachedIcon`.
//
// 2019 vs 2022: the same type. The only difference is in the decompilation of the texture getter --
// the 2019 output wraps the resolve in a `while (true)` loop that the 2022 output does not have.
// That loop is the same decompiler artifact already noted on
// DreadScripts.ControllerEditor.CachedTextureContent, not shipped behaviour; the straight-line 2022
// form is ported. No behavioural divergence.
//
// Relationship to the caches already ported: this is ADOverhaul's own icon cache, close to but not
// interchangeable with DreadScripts.ControllerEditor.CachedTextureContent. That type is constructed
// from a session key alone and always begins by reading the cache; this one is constructed from a
// texture the caller has just built, writes it to the cache, and reads only when the caller had
// nothing to give. Its two static cache helpers were the same code as
// DreadScripts.Common.EditorGuiUtils.SaveTextureToSession / LoadTextureFromSession -- same
// SessionState keys, same int-array encoding -- so they forward to those rather than carrying a
// third copy of the conversion loops, and its private ToBytes/ToInts are dropped as a result.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// A toolbar icon held as a <see cref="GUIContent"/> whose image is backed by
    /// <see cref="SessionState"/>, so it survives the domain reload that follows every recompile.
    /// </summary>
    /// <remarks>
    /// The icons this holds are generated rather than loaded from disk: callers take a built-in
    /// editor icon and recolour it, then hand the result here. The generated <see cref="Texture2D"/>
    /// does not survive a domain reload, which in the editor happens on every recompile and would
    /// leave the toolbars blank until something rebuilt them. Caching the encoded bytes in the
    /// session lets the icon be re-decoded instead of regenerated.
    /// </remarks>
    internal sealed class CachedIcon
    {
        /// <summary>
        /// Whether the session cache is still worth consulting. Cleared before a lookup and set
        /// again only if that lookup produced something, so a permanent miss costs one read rather
        /// than one per repaint, while a hit stays re-readable after the next reload.
        /// </summary>
        private bool canResolve = true;

        private GUIContent _content;

        private Texture2D _texture;

        private readonly string cacheKey;

        private readonly string tooltip;

        private GUIContent content
        {
            get
            {
                // Rebuilt lazily rather than only in the constructor, because the constructor may
                // have been handed a null texture and recovered one from the cache afterwards --
                // see the note there.
                if (_content.image == null && canResolve)
                {
                    _content = new GUIContent(texture)
                    {
                        tooltip = tooltip
                    };
                }

                return _content;
            }
        }

        /// <summary>
        /// The icon, re-decoded from the session cache if a domain reload destroyed it, or null when
        /// nothing is cached under <see cref="cacheKey"/>.
        /// </summary>
        internal Texture2D texture
        {
            get
            {
                if (canResolve && _texture == null)
                {
                    canResolve = false;
                    ResolveTexture();
                    canResolve = _texture != null;
                }

                return _texture;
            }
        }

        /// <param name="texture">
        /// The freshly built icon, or null to adopt whatever is already cached under
        /// <paramref name="cacheKey"/>.
        /// </param>
        public CachedIcon(Texture2D texture, string cacheKey, string tooltip = "")
        {
            _texture = texture;
            this.cacheKey = cacheKey;
            this.tooltip = tooltip;

            if (_texture != null)
            {
                SaveToCache(texture.EncodeToPNG(), cacheKey);
            }
            else
            {
                ResolveTexture();
            }

            // Built from the argument rather than from the field, so that when the argument was null
            // and ResolveTexture has just recovered an image, this content is left with a null image
            // on purpose; the content getter notices and rebuilds it on first use.
            _content = new GUIContent(texture)
            {
                tooltip = tooltip
            };
        }

        private void ResolveTexture()
        {
            _texture = LoadFromCache(cacheKey);
        }

        /// <summary>
        /// Decodes an icon previously stored by <see cref="SaveToCache"/>, or null when this editor
        /// session has none under that key.
        /// </summary>
        internal static Texture2D LoadFromCache(string cacheKey)
        {
            return EditorGuiUtils.LoadTextureFromSession(cacheKey);
        }

        /// <summary>Stores encoded image bytes under <paramref name="cacheKey"/> for this editor session.</summary>
        internal static void SaveToCache(byte[] imageBytes, string cacheKey)
        {
            EditorGuiUtils.SaveTextureToSession(imageBytes, cacheKey);
        }

        public static implicit operator GUIContent(CachedIcon icon)
        {
            return icon.content;
        }
    }
}
