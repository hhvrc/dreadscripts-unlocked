using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class SupportWindowAssets
{
	internal class TextureAssets
	{
		internal readonly RemoteTexture Icon = new RemoteTexture("https://i.imgur.com/iHszIY3.png", overridesecond: true, "ds-supporters-main");

		internal readonly RemoteTexture KofiBanner = new RemoteTexture("https://i.imgur.com/FMv1R6A.png", overridesecond: true, "ds-supporters-kofi");
	}

	internal class StyleAssets
	{
		internal readonly GUIStyle Header = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			fontSize = 18
		};

		internal readonly GUIStyle Name = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};

		internal readonly GUIStyle Prefix = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};

		internal readonly GUIStyle Suffix = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleRight,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};
	}

	internal static TextureAssets textures;

	internal static StyleAssets styles;

	[SpecialName]
	internal static TextureAssets GetTextures()
	{
		return textures ?? (textures = new TextureAssets());
	}

	[SpecialName]
	internal static StyleAssets GetStyles()
	{
		return styles ?? (styles = new StyleAssets());
	}
}
