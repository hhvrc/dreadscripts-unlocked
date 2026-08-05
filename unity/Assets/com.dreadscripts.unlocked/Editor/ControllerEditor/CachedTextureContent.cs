// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/CachedTextureContent.cs
//
// DEOBF-BUG
// The `texture` getter loops `while (true) Load();` in the decompiled source, which would hang the
// editor on the first cache miss. It is read as a single `Load()` here; see the comment at that
// statement for why a miss then settles rather than repeating.
//
// NOTES
// content and texture carry [SpecialName] in the decompiled source, i.e. they are the accessor pairs
// of properties ILSpy could not recombine; they are restored as properties here.
//
// This type reached the decompiler fully obfuscated -- it was `ErrorPolicy`, with every field and
// method renamed. The names in export/ are this project's own, assigned in renames/ and applied by
// the re-export, and this port shortens four of them further, export/ name first:
// LoadTextureFromSession -> LoadTexture, SaveTextureToSession -> SaveTexture, IntsToBytes ->
// ToBytes, BytesToInts -> ToInts. Nothing here is a vendor identifier.
//
// Audit status: VERIFIED -- all five fields, the constructor, both property accessor pairs, Load,
// RebuildContent, both static session helpers, both conversion helpers and the implicit GUIContent
// operator were diffed statement by statement against export/, including the SessionState calls and
// the EraseIntArray on a corrupt entry. Everything matches apart from the loop recorded above; the
// port returns null from the catch where the decompiled source falls through to the same return.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A <see cref="GUIContent"/> whose image is kept in <see cref="SessionState"/>, so it survives
    /// the domain reload that follows every script recompile.
    /// </summary>
    /// <remarks>
    /// A <see cref="Texture2D"/> created at runtime is destroyed on domain reload, which in the
    /// editor happens on every recompile and would leave toolbar icons blank until something rebuilt
    /// them. SessionState persists across reloads but stores only primitives, hence the PNG bytes
    /// smuggled through as an int array.
    /// </remarks>
    internal sealed class CachedTextureContent
    {
        private GUIContent _content;

        private Texture2D _texture;

        private readonly string sessionKey;

        private readonly string tooltip;

        private bool hasTexture;

        public CachedTextureContent(string sessionKey, string tooltip = "")
        {
            this.sessionKey = sessionKey;
            this.tooltip = tooltip;
            Load();
            RebuildContent();
        }

        private GUIContent content
        {
            get
            {
                if (hasTexture && _content.image == null)
                {
                    Load();
                }

                return _content;
            }
            set => _content = value;
        }

        /// <summary>
        /// The cached texture, reloaded from the session cache if a domain reload destroyed it.
        /// </summary>
        internal Texture2D texture
        {
            get
            {
                // The shipped build looped here forever instead of reloading once. Load() goes
                // through the setter below, which clears hasTexture when nothing was cached, so a
                // miss settles rather than repeating.
                if (hasTexture && _texture == null)
                {
                    Load();
                }

                return _texture;
            }
            set
            {
                _texture = value;
                hasTexture = _texture != null;

                if (hasTexture)
                {
                    SaveTexture(value.EncodeToPNG(), sessionKey);
                }

                RebuildContent();
            }
        }

        private void Load()
        {
            texture = LoadTexture(sessionKey);
        }

        private void RebuildContent()
        {
            content = new GUIContent(texture, tooltip);
        }

        internal static Texture2D LoadTexture(string sessionKey)
        {
            int[] stored = SessionState.GetIntArray(sessionKey, null);
            if (stored == null)
            {
                return null;
            }

            try
            {
                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(ToBytes(stored));
                texture.Apply();
                return texture;
            }
            catch (Exception exception)
            {
                // Corrupt or truncated cache entry: drop it so the next call starts clean rather
                // than failing the same way every repaint.
                Debug.LogException(exception);
                SessionState.EraseIntArray(sessionKey);
                return null;
            }
        }

        internal static void SaveTexture(byte[] pngBytes, string sessionKey)
        {
            SessionState.SetIntArray(sessionKey, ToInts(pngBytes));
        }

        private static byte[] ToBytes(int[] values)
        {
            byte[] bytes = new byte[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                bytes[i] = (byte)values[i];
            }

            return bytes;
        }

        private static int[] ToInts(byte[] bytes)
        {
            int[] values = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                values[i] = bytes[i];
            }

            return values;
        }

        public static implicit operator GUIContent(CachedTextureContent cached)
        {
            return cached.content;
        }
    }
}
