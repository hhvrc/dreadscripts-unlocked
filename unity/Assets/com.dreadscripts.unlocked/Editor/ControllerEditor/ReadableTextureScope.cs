// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ReadableTextureScope.cs

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Yields a CPU-readable copy of a texture, so its pixels can be sampled regardless of whether
    /// the asset was imported with Read/Write enabled. Disposing frees the copy if one was made.
    /// </summary>
    /// <remarks>
    /// A texture without Read/Write enabled has no pixel data on the CPU side. The way to get at it
    /// without changing the user's import settings is to blit through a <see cref="RenderTexture"/>
    /// and read back from the GPU.
    /// </remarks>
    internal sealed class ReadableTextureScope : IDisposable
    {
        /// <summary>True when <see cref="texture"/> is a copy this scope owns and must destroy.</summary>
        internal bool isTemporary;

        internal Texture2D texture;

        internal ReadableTextureScope(Texture2D source)
        {
            // The shipped build probed readability by calling GetPixel inside a try/catch. isReadable
            // answers the same question without throwing and catching an exception every time an
            // unreadable texture is opened.
            if (source.isReadable)
            {
                isTemporary = false;
                texture = source;
                return;
            }

            isTemporary = true;

            int width = source.width;
            int height = source.height;

            RenderTexture temporary = RenderTexture.GetTemporary(width, height);
            RenderTexture previouslyActive = RenderTexture.active;
            try
            {
                // Point filtering on both ends keeps the blit a straight copy rather than a resample.
                temporary.filterMode = FilterMode.Point;
                RenderTexture.active = temporary;
                Graphics.Blit(source, temporary);

                texture = new Texture2D(width, height);
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
            }
            finally
            {
                // The shipped build never released the temporary, leaking one render texture per
                // scope, and cleared RenderTexture.active outright instead of restoring it.
                RenderTexture.active = previouslyActive;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        public void Dispose()
        {
            if (isTemporary)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public static implicit operator Texture2D(ReadableTextureScope scope)
        {
            return scope.texture;
        }
    }
}
