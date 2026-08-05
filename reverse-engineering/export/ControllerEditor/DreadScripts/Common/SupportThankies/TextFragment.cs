using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal struct TextFragment
{
	internal GUIContent content;

	internal RemoteTexture image;

	internal bool isImage;

	private static object CompareCode;

	internal TextFragment(GUIContent reference)
	{
		content = reference;
		image = null;
		isImage = false;
	}

	internal TextFragment(RemoteTexture res)
	{
		content = GUIContent.none;
		image = res;
		isImage = true;
	}

	internal void DrawLayout(GUIStyle last, float b = 20f)
	{
		if (isImage)
		{
			GUILayout.Label(image.GetTexture(), last, GUILayout.Width(b), GUILayout.Height(b));
		}
		else
		{
			GUILayout.Label(content, last, GUILayout.ExpandWidth(expand: false), GUILayout.Height(b));
		}
	}

	internal void DrawRect(Rect spec)
	{
		if (isImage)
		{
			image.Draw(spec);
		}
		else
		{
			GUI.Label(spec, content, SupportWindowAssets.GetStyles().Name);
		}
	}

	internal static List<TextFragment> Parse(string spec)
	{
		List<TextFragment> list = new List<TextFragment>();
		Match match = Regex.Match(spec, "<image=(.+?)>");
		while (match.Success)
		{
			string value = match.Groups[1].Value;
			if (match.Index > 0)
			{
				list.Add(new TextFragment(new GUIContent(spec.Substring(0, match.Index))));
			}
			list.Add(new TextFragment(new RemoteTexture(value, overridesecond: true, value)));
			spec = spec.Substring(match.Index + match.Length);
			match = Regex.Match(spec, "<image=(.+?)>");
		}
		if (!string.IsNullOrEmpty(spec))
		{
			list.Add(new TextFragment(new GUIContent(spec)));
		}
		return list;
	}

	internal static bool PublishCode()
	{
		return CompareCode == null;
	}
}
