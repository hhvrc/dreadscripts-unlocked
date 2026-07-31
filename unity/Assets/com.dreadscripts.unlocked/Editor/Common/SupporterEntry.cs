// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/SupporterEntry.cs
//   m_Queue -> rawEntry, error -> nameFragments, m_Setter -> prefixFragments,
//   m_Connection -> suffixFragments, m_Consumer -> backgroundTexture,
//   adapter -> backgroundLayout, m_Interpreter -> backgroundColor, _Watcher -> borderColor,
//   candidate -> nameColor, _Product -> tooltip, _Expression -> onClickUrl,
//   system -> splitterState, worker -> cardRect
//
// The splitter weights were an obfuscator-encrypted constant in the decompilation
// (`_003CModule_003E.smethod_5<float[]>(1991865236)`). Decrypting the constant blob out of
// -Module-.cs yields `new float[] { 1f, 1f, 1f }`, which is what appears below.
//
// Uses DreadScripts.Common.GUIColorScope and GUILayoutUtils in place of the GuiColorScope and
// EditorLayoutUtils copies that sit beside this type in the decompiled source; both are already
// ported and behave identically.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// One line of the downloaded supporter list, parsed into a card.
    /// </summary>
    /// <remarks>
    /// A line is a flat sequence of <c>&lt;key=value&gt;</c> attributes, e.g.
    /// <c>&lt;name=Someone&gt;&lt;namecolor=#FFC0CB&gt;&lt;onclick=https://...&gt;</c>. Every
    /// attribute is optional and anything unrecognised is ignored, so the list format can gain new
    /// attributes without older installs of the tool failing to draw it.
    /// </remarks>
    internal class SupporterEntry
    {
        private readonly string rawEntry;

        private readonly List<TextFragment> nameFragments;
        private readonly List<TextFragment> prefixFragments;
        private readonly List<TextFragment> suffixFragments;

        private readonly RemoteTexture backgroundTexture;
        private readonly RemoteTexture.TextureLayoutMethod backgroundLayout;

        private readonly Color? backgroundColor;
        private readonly Color? borderColor;
        private readonly Color? nameColor;

        private readonly string tooltip;
        private readonly string onClickUrl;

        /// <summary>Equal weights for the prefix, name and suffix rows of the card.</summary>
        private readonly object splitterState = GUILayoutUtils.CreateSplitterState(new float[] { 1f, 1f, 1f });

        /// <summary>
        /// The card's rect as of the last Repaint. Backgrounds have to be drawn before the content
        /// that determines their size, so each frame paints into the previous frame's rect.
        /// </summary>
        private Rect cardRect;

        internal SupporterEntry(string rawEntry)
        {
            this.rawEntry = rawEntry;

            TryExtractAttribute("onclick", out onClickUrl);

            tooltip = TryExtractAttribute("tooltip", out string tooltipValue)
                ? tooltipValue
                : SupporterStrings.SupporterTooltips.RandomElement();

            if (!TryExtractAttribute("bgtype", out string bgTypeValue)
                || !Enum.TryParse(bgTypeValue, true, out backgroundLayout))
            {
                backgroundLayout = RemoteTexture.TextureLayoutMethod.Pattern;
            }

            if (TryExtractAttribute("name", out string nameValue))
            {
                nameFragments = TextFragment.Parse(nameValue);
            }

            if (TryExtractAttribute("prefix", out string prefixValue))
            {
                prefixFragments = TextFragment.Parse(prefixValue);
            }

            if (TryExtractAttribute("suffix", out string suffixValue))
            {
                suffixFragments = TextFragment.Parse(suffixValue);
            }

            // An unparseable colour leaves the nullable unset, which each draw site reads as
            // "use the default" rather than as black.
            if (TryExtractAttribute("namecolor", out string nameColorValue))
            {
                nameColor = ColorUtility.TryParseHtmlString(nameColorValue, out Color parsedNameColor)
                    ? parsedNameColor
                    : (Color?)null;
            }

            if (TryExtractAttribute("bgcolor", out string bgColorValue))
            {
                backgroundColor = ColorUtility.TryParseHtmlString(bgColorValue, out Color parsedBgColor)
                    ? parsedBgColor
                    : (Color?)null;
            }

            if (TryExtractAttribute("bordercolor", out string borderColorValue))
            {
                borderColor = ColorUtility.TryParseHtmlString(borderColorValue, out Color parsedBorderColor)
                    ? parsedBorderColor
                    : (Color?)null;
            }

            if (TryExtractAttribute("bgimage", out string bgImageValue))
            {
                backgroundTexture = new RemoteTexture(bgImageValue, true, bgImageValue);
            }
        }

        /// <summary>Draws the supporter's card into the current layout.</summary>
        /// <param name="fragmentSize">Row height for the name, prefix and suffix fragments.</param>
        internal void DrawCard(float fragmentSize = 20f)
        {
            Rect rect = cardRect.Shrink(2f);

            // The tint is pre-composited onto the current GUI colour rather than replacing it, so
            // that a translucent bgcolor darkens the artwork instead of hiding it.
            using (new GUIColorScope(
                GUIColorScope.ColoringType.General,
                backgroundColor.HasValue ? GUI.color.AlphaBlend(backgroundColor.Value) : GUI.color))
            {
                backgroundTexture?.Draw(rect, backgroundLayout);
            }

            // With artwork present the fill is skipped so it is not covered up; without it, and
            // without a bgcolor, a faint black plate keeps the text legible.
            EditorGuiUtils.DrawRoundedBox(
                rect,
                backgroundTexture != null ? Color.clear : backgroundColor ?? new Color(0f, 0f, 0f, 0.4f),
                borderColor.GetValueOrDefault(),
                1f);

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayoutUtils.BeginSplit(splitterState, null, false);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(8f);

                        if (prefixFragments != null)
                        {
                            foreach (TextFragment fragment in prefixFragments)
                            {
                                fragment.DrawLayout(SupportWindowAssets.Styles.Prefix, fragmentSize);
                            }
                        }
                        else
                        {
                            // An empty label still reserves the row, so a card without a prefix is
                            // the same height as one with.
                            GUILayout.Label(GUIContent.none);
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (nameFragments != null)
                        {
                            using (new GUIColorScope(GUIColorScope.ColoringType.General, nameColor ?? GUI.color))
                            {
                                foreach (TextFragment fragment in nameFragments)
                                {
                                    fragment.DrawLayout(SupportWindowAssets.Styles.Name, fragmentSize);
                                }
                            }
                        }

                        GUILayout.FlexibleSpace();
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (suffixFragments == null)
                        {
                            GUILayout.Label(GUIContent.none);
                        }
                        else
                        {
                            foreach (TextFragment fragment in suffixFragments)
                            {
                                fragment.DrawLayout(SupportWindowAssets.Styles.Suffix, fragmentSize);
                            }
                        }

                        GUILayout.Space(8f);
                    }

                    GUILayoutUtils.EndSplit();
                    GUILayout.FlexibleSpace();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    cardRect = GUILayoutUtility.GetLastRect();
                }

                GUILayout.Space(4f);
            }

            // An empty label purely to register the tooltip over the whole card.
            GUI.Label(cardRect, new GUIContent(string.Empty, tooltip));

            if (!string.IsNullOrWhiteSpace(onClickUrl) && EditorGuiUtils.IsClicked(cardRect))
            {
                Application.OpenURL(onClickUrl);
            }
        }

        /// <summary>
        /// Reads a single <c>&lt;key=value&gt;</c> attribute out of the raw line.
        /// </summary>
        /// <remarks>
        /// The value is matched non-greedily but must be followed by the start of another attribute
        /// or the end of the line, so that a value which itself contains markup -- a prefix holding
        /// an <c>&lt;image=&gt;</c> tag, say -- is captured whole rather than truncated at its
        /// first '&gt;'.
        /// </remarks>
        internal bool TryExtractAttribute(string key, out string value)
        {
            Match match = Regex.Match(rawEntry, "<" + key + "=(.*?)>(?:<|$)");
            value = match.Success ? match.Groups[1].Value : null;
            return match.Success;
        }
    }
}
