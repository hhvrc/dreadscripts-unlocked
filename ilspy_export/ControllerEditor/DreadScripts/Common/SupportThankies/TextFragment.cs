using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal struct TextFragment
{
	internal GUIContent _Reader;

	internal RemoteTexture bridge;

	internal bool strategy;

	private static object CompareCode;

	internal TextFragment(GUIContent reference)
	{
		_Reader = reference;
		bridge = null;
		strategy = false;
	}

	internal TextFragment(RemoteTexture res)
	{
		_Reader = GUIContent.none;
		bridge = res;
		strategy = true;
	}

	internal void ReadWrapper(GUIStyle last, float b = 20f)
	{
		if (strategy)
		{
			GUILayout.Label(bridge.ValidateWrapper(), last, GUILayout.Width(b), GUILayout.Height(b));
		}
		else
		{
			GUILayout.Label(_Reader, last, GUILayout.ExpandWidth(expand: false), GUILayout.Height(b));
		}
	}

	internal void SelectWrapper(Rect spec)
	{
		if (strategy)
		{
			bridge.ResetWrapper(spec);
		}
		else
		{
			GUI.Label(spec, _Reader, SupportWindowAssets.RegisterWrapper().composer);
		}
	}

	internal static List<TextFragment> RemoveWrapper(string spec)
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
