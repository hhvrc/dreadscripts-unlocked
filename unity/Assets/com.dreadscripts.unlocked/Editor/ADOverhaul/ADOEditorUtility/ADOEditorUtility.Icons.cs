// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static NewVal    -> TrimmedIcon,            line 3917
//   static DefineVal -> TrimTransparentBorder,  line 3837
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// 2019 vs 2022: the same two methods (2019 lines 4023 and 3941, under different obfuscated names).
// Two decompilation differences, neither of them behavioural:
//   - The 2019 output guards the icon lookup with `while (gUIContent.image != null)` where 2022 has
//     a plain `if`. That is the same decompiler artifact already noted on CachedIcon and on
//     DreadScripts.ControllerEditor.CachedTextureContent; taken literally the 2019 form would spin
//     forever, so the 2022 straight-line form is what shipped.
//   - The two builds order the null-argument throw and the degenerate-size error differently
//     (2019 inverts both conditions and falls through to the throw). Same control flow either way.

using System;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Builds a session-cached toolbar icon from a built-in editor icon, trimmed to its visible
        /// content.
        /// </summary>
        /// <param name="iconName">Name of the built-in icon to copy, as <see cref="EditorGUIUtility.IconContent(string)"/> takes it.</param>
        /// <param name="cacheKey">Session key the trimmed result is stored under.</param>
        /// <remarks>
        /// Built-in icons carry a generous transparent margin sized for the inspector, which leaves
        /// them looking undersized in the tool's tight toolbar rows. Trimming the margin and
        /// re-adding a single pixel of it lets the icon fill its slot. The result is a generated
        /// texture, so it goes through <see cref="CachedIcon"/> to survive domain reloads.
        /// </remarks>
        internal static CachedIcon TrimmedIcon(string iconName, string cacheKey, string tooltip = "")
        {
            Texture2D trimmed = null;

            GUIContent builtIn = EditorGUIUtility.IconContent(iconName);
            if (builtIn != null && builtIn.image != null)
            {
                trimmed = TrimTransparentBorder(builtIn.image as Texture2D);
            }

            // A null texture is not an error here: CachedIcon falls back to whatever this session
            // already has cached under the key.
            return new CachedIcon(trimmed, cacheKey, tooltip);
        }

        /// <summary>
        /// Returns a copy of <paramref name="texture"/> cropped to the pixels at or above
        /// <paramref name="alphaThreshold"/>, surrounded by <paramref name="padding"/> transparent
        /// pixels.
        /// </summary>
        /// <param name="alphaThreshold">
        /// Alpha at or above which a pixel counts as content. Not zero, so that the near-transparent
        /// antialiasing fringe of a built-in icon does not defeat the trim.
        /// </param>
        /// <param name="padding">Transparent border re-added on every side, so the artwork does not sit flush against the edge.</param>
        /// <returns>The cropped copy, or null if no pixel cleared the threshold.</returns>
        private static Texture2D TrimTransparentBorder(Texture2D texture, float alphaThreshold = 0.2f, int padding = 1)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }

            // Built-in editor icons are not import-flagged readable, so the pixels have to come back
            // off the GPU.
            using (ReadableTextureScope readable = new ReadableTextureScope(texture))
            {
                Texture2D source = readable.texture;
                int width = source.width;
                int height = source.height;

                // Seeded inverted, so that an image with no qualifying pixel leaves the bounds
                // crossed over and the degenerate-size check below catches it.
                int minX = width;
                int maxX = 0;
                int minY = height;
                int maxY = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (source.GetPixel(x, y).a >= alphaThreshold)
                        {
                            if (x < minX)
                            {
                                minX = x;
                            }

                            if (x > maxX)
                            {
                                maxX = x;
                            }

                            if (y < minY)
                            {
                                minY = y;
                            }

                            if (y > maxY)
                            {
                                maxY = y;
                            }
                        }
                    }
                }

                int contentWidth = maxX - minX + 1;
                int contentHeight = maxY - minY + 1;
                int paddedWidth = contentWidth + padding * 2;
                int paddedHeight = contentHeight + padding * 2;

                if (contentWidth < 1 || contentHeight < 1)
                {
                    Debug.LogError("Trimmed texture has zero size.");
                    return null;
                }

                Color[] content = source.GetPixels(minX, minY, contentWidth, contentHeight);

                Texture2D result = new Texture2D(paddedWidth, paddedHeight);

                // Only the border is cleared explicitly; the interior is overwritten wholesale by the
                // SetPixels below, so a new Texture2D's undefined initial contents never show.
                for (int y = 0; y < paddedHeight; y++)
                {
                    for (int x = 0; x < paddedWidth; x++)
                    {
                        if (x < padding || x >= padding + contentWidth || y < padding || y >= padding + contentHeight)
                        {
                            result.SetPixel(x, y, Color.clear);
                        }
                    }
                }

                result.SetPixels(padding, padding, contentWidth, contentHeight, content);
                result.Apply();

                return result;
            }
        }
    }
}
