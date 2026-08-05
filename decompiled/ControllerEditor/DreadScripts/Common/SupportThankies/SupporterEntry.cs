using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal class SupporterEntry
{
	internal readonly string rawEntry;

	internal readonly List<TextFragment> nameFragments;

	internal readonly List<TextFragment> prefixFragments;

	internal readonly List<TextFragment> suffixFragments;

	internal readonly RemoteTexture backgroundTexture;

	internal readonly RemoteTexture.TextureLayoutMethod backgroundLayout;

	internal readonly Color? backgroundColor;

	internal readonly Color? borderColor;

	internal readonly Color? nameColor;

	internal readonly string tooltip;

	internal readonly string onClickUrl;

	internal readonly object splitterState = EditorLayoutUtils.CreateSplitterState(global::_003CModule_003E.smethod_5<float[]>(1991865236));

	internal Rect cardRect;

	internal SupporterEntry(string spec)
	{
		rawEntry = spec;
		TryExtractAttribute("onclick", out onClickUrl);
		tooltip = ((!TryExtractAttribute("tooltip", out var col)) ? SupporterStrings.SupporterTooltips.RandomElement() : col);
		if (!TryExtractAttribute("bgtype", out var col2) || !Enum.TryParse<RemoteTexture.TextureLayoutMethod>(col2, ignoreCase: true, out backgroundLayout))
		{
			backgroundLayout = RemoteTexture.TextureLayoutMethod.Pattern;
		}
		if (TryExtractAttribute("name", out var col3))
		{
			nameFragments = TextFragment.Parse(col3);
		}
		if (TryExtractAttribute("prefix", out var col4))
		{
			prefixFragments = TextFragment.Parse(col4);
		}
		if (TryExtractAttribute("suffix", out var col5))
		{
			suffixFragments = TextFragment.Parse(col5);
		}
		if (TryExtractAttribute("namecolor", out var col6))
		{
			nameColor = ((!ColorUtility.TryParseHtmlString(col6, out var color)) ? ((Color?)null) : new Color?(color));
		}
		if (TryExtractAttribute("bgcolor", out var col7))
		{
			backgroundColor = (ColorUtility.TryParseHtmlString(col7, out var color2) ? new Color?(color2) : ((Color?)null));
		}
		if (TryExtractAttribute("bordercolor", out var col8))
		{
			borderColor = ((!ColorUtility.TryParseHtmlString(col8, out var color3)) ? ((Color?)null) : new Color?(color3));
		}
		if (TryExtractAttribute("bgimage", out var col9))
		{
			backgroundTexture = new RemoteTexture(col9, overridesecond: true, col9);
		}
	}

	internal void DrawCard(float v = 20f)
	{
		Rect rect = cardRect.Shrink(2f);
		using (new GuiColorScope(GuiColorScope.ColoringType.General, (!backgroundColor.HasValue) ? GUI.color : GUI.color.AlphaBlend(backgroundColor.Value)))
		{
			backgroundTexture?.Draw(rect, backgroundLayout);
		}
		EditorGuiUtils.DrawRoundedBox(rect, (backgroundTexture != null) ? Color.clear : (backgroundColor ?? new Color(0f, 0f, 0f, 0.4f)), borderColor.GetValueOrDefault(), 1f);
		using (new GUILayout.VerticalScope())
		{
			using (new GUILayout.VerticalScope())
			{
				GUILayout.FlexibleSpace();
				EditorLayoutUtils.BeginSplit(splitterState, null, false);
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(8f);
					if (prefixFragments != null)
					{
						foreach (TextFragment prefixFragment in prefixFragments)
						{
							prefixFragment.DrawLayout(SupportWindowAssets.GetStyles().Prefix, v);
						}
					}
					else
					{
						GUILayout.Label(GUIContent.none);
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (nameFragments != null)
					{
						using (new GuiColorScope(GuiColorScope.ColoringType.General, nameColor ?? GUI.color))
						{
							foreach (TextFragment nameFragment in nameFragments)
							{
								nameFragment.DrawLayout(SupportWindowAssets.GetStyles().Name, v);
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
						foreach (TextFragment suffixFragment in suffixFragments)
						{
							suffixFragment.DrawLayout(SupportWindowAssets.GetStyles().Suffix, v);
						}
					}
					GUILayout.Space(8f);
				}
				EditorLayoutUtils.EndSplit();
				GUILayout.FlexibleSpace();
			}
			if (Event.current.type == EventType.Repaint)
			{
				cardRect = GUILayoutUtility.GetLastRect();
			}
			GUILayout.Space(4f);
		}
		GUI.Label(cardRect, new GUIContent(string.Empty, tooltip));
		if (!string.IsNullOrWhiteSpace(onClickUrl) && EditorGuiUtils.IsClicked(cardRect))
		{
			Application.OpenURL(onClickUrl);
		}
	}

	internal bool TryExtractAttribute(string v, out string col)
	{
		string pattern = "<" + v + "=(.*?)>(?:<|$)";
		Match match = Regex.Match(rawEntry, pattern);
		bool success = match.Success;
		col = (success ? match.Groups[1].Value : null);
		return success;
	}
}
