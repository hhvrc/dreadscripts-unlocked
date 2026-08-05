// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static TrimTransparentBorder -> unchanged,   lines 7257-7322
//   static ReflectList -> ColorTexture,          line 7339
//   static field connectionProcessor -> sharedColorTexture, line 2228
//   static LoginList   -> SharedColorTexture,    line 7324
//   static DeleteList  -> CircleTexture(color, size),        line 7349
//   static CreateList  -> CircleTexture(color, size, background), line 7354
//   static ViewList    -> ReadPixelsScaled,      line 7404
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The TrimTransparentBorder entry above used to read "CloneList, lines 7424-7494". Both halves were
// stale: the re-snapshot in 561e9ec renumbered the file and gave that member back its real name, so
// it is now TrimTransparentBorder at 7257-7322, and 7424 is MirrorTransform, which
// EditorUtils.TransformGeometry.cs claims. Corrected here; nothing about the port changed.
//
// Complete: every texture helper in the outer class body is now here. CachedIcon (line 7381), which
// also produces a texture, lives with the icon table it feeds in EditorUtils.Contents.cs.
//
// Two nearly identical single-colour helpers, on purpose: ColorTexture allocates a fresh texture
// every call and the caller owns it, while SharedColorTexture overwrites one process-wide texture
// and hands back a reference that is only valid until the next call. Use the shared one for an
// immediate GUI.DrawTexture and the allocating one for anything stored in a GUIStyle.
//
// DELIBERATE DEVIATION
//
// TrimTransparentBorder clears the padding ring with a single SetPixels of a default Color[] where
// the shipped build (7306-7315) walks every pixel of the result and writes Color.clear to the ones
// outside the trimmed region. The written result is the same -- default(Color) is (0,0,0,0), and
// every pixel the loop skipped is overwritten by the block copy that follows in both versions -- so
// this is a cost change, not a behaviour change. The decompiled early-out is also inverted: it
// wraps the whole body in `if (!(ident == null))` and throws at the bottom, which is written here
// as a guard clause that throws first.
//
// Audit status: VERIFIED against reverse-engineering/export/ -- all seven members re-checked statement by statement
// against EditorUtils.cs lines 7257-7322 (after correcting the stale citation above), 7324, 7339,
// 7349, 7354, 7404 and the sharedColorTexture field at 2228. Bodies match, including the RGBA32 vs
// RGBAFloat split between the two single-colour helpers, the inscribed hard-edged circle, and the
// unreleased temporary RenderTexture in ReadPixelsScaled. The one difference is the padding clear
// recorded immediately above.

using System;
using DreadScripts.Common;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A 1x1 texture of a single colour, for use as a flat GUIStyle background.
        /// </summary>
        /// <remarks>
        /// Point filtering and no anisotropy because the texture is always stretched over a rect and
        /// there is nothing to interpolate; leaving the defaults would cost mip generation for a
        /// single pixel.
        /// <para>
        /// Every call allocates a new texture and nothing owns the result, so a caller that invokes
        /// this per frame leaks one texture per frame. The shipped build had no cache here and this
        /// port keeps that behaviour; call it once from a field or style initialiser, not from OnGUI.
        /// </para>
        /// </remarks>
        internal static Texture2D ColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

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

        /// <summary>
        /// The single reusable 1x1 texture behind <see cref="SharedColorTexture"/>.
        /// </summary>
        private static Texture2D sharedColorTexture;

        /// <summary>
        /// A 1x1 texture of <paramref name="color"/> that costs no allocation, at the price of
        /// being valid only until the next call. Pass it straight to a draw call; never store it.
        /// </summary>
        /// <remarks>
        /// RGBAFloat rather than RGBA32, so a colour with components outside 0-1 -- which the tool's
        /// tinting helpers can produce -- survives the round trip instead of being clamped.
        /// </remarks>
        internal static Texture2D SharedColorTexture(Color color)
        {
            if (sharedColorTexture == null)
            {
                sharedColorTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false)
                {
                    filterMode = FilterMode.Point,
                    anisoLevel = 0
                };
            }

            sharedColorTexture.SetPixel(0, 0, color);
            sharedColorTexture.Apply();
            return sharedColorTexture;
        }

        /// <summary>
        /// A square texture holding a filled circle of <paramref name="color"/> on transparency.
        /// The caller owns the result.
        /// </summary>
        internal static Texture2D CircleTexture(Color color, int size)
        {
            return CircleTexture(color, size, Color.clear);
        }

        /// <summary>
        /// A square texture holding a filled circle of <paramref name="color"/> on
        /// <paramref name="background"/>. The caller owns the result.
        /// </summary>
        /// <remarks>
        /// Hard-edged: each pixel is inside or out, with no antialiasing, which is why point
        /// filtering is set. The circle is inscribed, so it touches all four edges.
        /// </remarks>
        internal static Texture2D CircleTexture(Color color, int size, Color background)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };

            Vector2 centre = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = Vector2.Distance(new Vector2(x, y), centre) <= size / 2f;
                    texture.SetPixel(x, y, inside ? color : background);
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Rescales <paramref name="source"/> to <paramref name="width"/> x
        /// <paramref name="height"/> on the GPU and reads the result back to the CPU.
        /// </summary>
        /// <param name="pixels">The rescaled pixels, row-major from the bottom-left.</param>
        /// <param name="pixelsOnly">
        /// Destroy the intermediate texture and return null, for a caller that only wants
        /// <paramref name="pixels"/>. Otherwise the caller owns the returned texture.
        /// </param>
        /// <remarks>
        /// This is the only way to read a texture whose import settings leave it unreadable:
        /// blitting through a RenderTexture goes via the GPU copy, which ReadPixels can then reach.
        /// Point filtering is forced on the source and the render target so a downscale samples
        /// rather than averages -- for an icon that is what preserves its shape.
        /// <para>
        /// The temporary RenderTexture is never released, as shipped. RenderTexture.GetTemporary
        /// pools by descriptor, so a repeated call at the same size reuses one entry rather than
        /// growing without bound, but it is still a leak of one target per distinct size.
        /// </para>
        /// </remarks>
        internal static Texture2D ReadPixelsScaled(Texture2D source, int width, int height, out Color[] pixels,
            bool pixelsOnly = false)
        {
            source.filterMode = FilterMode.Point;

            RenderTexture temporary = RenderTexture.GetTemporary(width, height);
            temporary.filterMode = FilterMode.Point;
            RenderTexture.active = temporary;
            Graphics.Blit(source, temporary);

            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            pixels = result.GetPixels();
            RenderTexture.active = null;

            if (!pixelsOnly)
            {
                return result;
            }

            UnityEngine.Object.DestroyImmediate(result);
            return null;
        }
    }
}
