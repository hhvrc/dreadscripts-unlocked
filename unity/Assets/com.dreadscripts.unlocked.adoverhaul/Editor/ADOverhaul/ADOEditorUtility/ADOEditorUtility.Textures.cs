// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static DestroyVal             -> SolidColorTexture,  line 3902
//   static field m_ThreadSerializer -> solidColorTexture, line 2108
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- the body was re-checked statement by statement against
// lines 3902-3916 on 2026-08-04 and matches, including the texture format, the filter mode and the
// aniso level.
//
// 2019 vs 2022: identical in both (2019 line 4004, under a different obfuscated name).
// No behavioural divergence.
//
// The nested class ReadableTexture (lines 1103-1146) also belongs to this region and was originally
// ported here. ControllerEditor shipped the same type statement-for-statement under the name
// ReadableTextureScope, so the two were consolidated into DreadScripts.Common.ReadableTextureScope;
// see that file's header for the sources and for the deviations it makes from both shipped copies.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        private static Texture2D solidColorTexture;

        /// <summary>
        /// A 1x1 texture filled with <paramref name="color"/>, for drawing flat rectangles through
        /// <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, float, float)"/>.
        /// </summary>
        /// <remarks>
        /// One texture is reused and recoloured on every call rather than one being allocated per
        /// colour, since IMGUI consumes the texture within the same call. A caller that holds the
        /// returned reference across two calls will find it has changed colour.
        /// </remarks>
        internal static Texture2D SolidColorTexture(Color color)
        {
            if (solidColorTexture == null)
            {
                // Float format and point filtering keep the colour exactly as given: no quantisation
                // to 8 bits and no filtering of a single texel across the target rect.
                solidColorTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
                {
                    filterMode = FilterMode.Point,
                    anisoLevel = 0
                };
            }

            solidColorTexture.SetPixel(0, 0, color);
            solidColorTexture.Apply();

            return solidColorTexture;
        }
    }
}
