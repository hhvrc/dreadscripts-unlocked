// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CloneList -> TrimTransparentBorder, lines 7424-7494
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Partial in progress: the remaining texture helpers in the outer class body (solid-colour
// textures, the readback used by the colour picker) have not been ported yet.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Crops a texture to the bounding box of its non-transparent pixels, leaving a border of
        /// <paramref name="padding"/> transparent pixels.
        /// </summary>
        /// <param name="alphaThreshold">
        /// Alpha at or above which a pixel counts as content. Above zero on purpose: Unity's icons
        /// have faintly non-zero alpha in their antialiased fringe, and trimming to alpha &gt; 0
        /// would keep nearly all of the padding it is the point of this to remove.
        /// </param>
        /// <returns>
        /// The trimmed copy, or null if the texture is entirely transparent. The caller owns the
        /// result and must destroy it.
        /// </returns>
        private static Texture2D TrimTransparentBorder(Texture2D texture, float alphaThreshold = 0.2f, int padding = 1)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            // Built-in icons are not import-flagged readable, so go through a readback copy.
            using (ReadableTextureScope readable = new ReadableTextureScope(texture))
            {
                Texture2D source = readable.texture;
                int width = source.width;
                int height = source.height;

                int minX = width, maxX = 0, minY = height, maxY = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (source.GetPixel(x, y).a < alphaThreshold)
                        {
                            continue;
                        }

                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }

                int trimmedWidth = maxX - minX + 1;
                int trimmedHeight = maxY - minY + 1;

                if (trimmedWidth < 1 || trimmedHeight < 1)
                {
                    Debug.LogError("Trimmed texture has zero size.");
                    return null;
                }

                int resultWidth = trimmedWidth + padding * 2;
                int resultHeight = trimmedHeight + padding * 2;

                Texture2D result = new Texture2D(resultWidth, resultHeight);

                // A fresh Texture2D holds whatever the graphics driver left there, so the padding
                // ring has to be written explicitly. default(Color) is (0,0,0,0), so one bulk clear
                // costs less than the per-pixel border loop the shipped build used.
                result.SetPixels(new Color[resultWidth * resultHeight]);
                result.SetPixels(padding, padding, trimmedWidth, trimmedHeight,
                                 source.GetPixels(minX, minY, trimmedWidth, trimmedHeight));
                result.Apply();

                return result;
            }
        }
    }
}
