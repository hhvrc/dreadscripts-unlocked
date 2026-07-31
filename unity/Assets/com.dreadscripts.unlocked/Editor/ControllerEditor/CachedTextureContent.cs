// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/CachedTextureContent.cs

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
    /// <para>
    /// The member names here are the vendor's own — this type reached the decompiler with its names
    /// intact — so they are kept as they were rather than renamed to match the rest of the port.
    /// </para>
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
