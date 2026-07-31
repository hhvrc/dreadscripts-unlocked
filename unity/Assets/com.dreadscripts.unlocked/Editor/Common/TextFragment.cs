// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/TextFragment.cs
//   _Reader -> content, bridge -> image, strategy -> isImage
// Deliberately unported: the CompareCode / PublishCode() pair, an obfuscator-injected null check
// on an always-null static with no callers.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// One run of a supporter's decorated name: either a piece of rich text or a single inline
    /// image.
    /// </summary>
    /// <remarks>
    /// Unity's rich text has no image tag, so a supporter writing <c>&lt;image=URL&gt;</c> has to be
    /// served by splitting the string and drawing the pieces as separate controls -- hence this
    /// type rather than a single label.
    /// </remarks>
    internal struct TextFragment
    {
        private const string ImageTagPattern = "<image=(.+?)>";

        internal GUIContent content;
        internal RemoteTexture image;
        internal bool isImage;

        internal TextFragment(GUIContent content)
        {
            this.content = content;
            image = null;
            isImage = false;
        }

        internal TextFragment(RemoteTexture image)
        {
            content = GUIContent.none;
            this.image = image;
            isImage = true;
        }

        /// <summary>Draws the fragment into the layout, at a fixed height of <paramref name="size"/>.</summary>
        internal void DrawLayout(GUIStyle style, float size = 20f)
        {
            if (isImage)
            {
                // Square, because an inline image stands in for a glyph.
                GUILayout.Label(image.Texture, style, GUILayout.Width(size), GUILayout.Height(size));
            }
            else
            {
                GUILayout.Label(content, style, GUILayout.ExpandWidth(false), GUILayout.Height(size));
            }
        }

        internal void DrawRect(Rect rect)
        {
            if (isImage)
            {
                image.Draw(rect);
            }
            else
            {
                GUI.Label(rect, content, SupportWindowAssets.Styles.Name);
            }
        }

        /// <summary>
        /// Splits a decorated string into text and image runs, in order. Any rich text markup other
        /// than <c>&lt;image=&gt;</c> is left in the text runs for Unity to interpret.
        /// </summary>
        internal static List<TextFragment> Parse(string text)
        {
            List<TextFragment> fragments = new List<TextFragment>();

            Match match = Regex.Match(text, ImageTagPattern);
            while (match.Success)
            {
                string url = match.Groups[1].Value;

                if (match.Index > 0)
                {
                    fragments.Add(new TextFragment(new GUIContent(text.Substring(0, match.Index))));
                }

                // The URL doubles as the session cache key, so the same image referenced by two
                // supporters is only fetched once.
                fragments.Add(new TextFragment(new RemoteTexture(url, true, url)));

                text = text.Substring(match.Index + match.Length);
                match = Regex.Match(text, ImageTagPattern);
            }

            if (!string.IsNullOrEmpty(text))
            {
                fragments.Add(new TextFragment(new GUIContent(text)));
            }

            return fragments;
        }
    }
}
