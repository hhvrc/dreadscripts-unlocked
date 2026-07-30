using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class SupportWindowAssets
{
	internal class TextureAssets
	{
		internal readonly RemoteTexture merchant = new RemoteTexture("https://i.imgur.com/iHszIY3.png", overridesecond: true, "ds-supporters-main");

		internal readonly RemoteTexture _Authentication = new RemoteTexture("https://i.imgur.com/FMv1R6A.png", overridesecond: true, "ds-supporters-kofi");

		internal static TextureAssets StopIndexer;

		internal static bool ReflectIndexer()
		{
			return StopIndexer == null;
		}
	}

	internal class StyleAssets
	{
		internal readonly GUIStyle _Pool = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			fontSize = 18
		};

		internal readonly GUIStyle composer = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};

		internal readonly GUIStyle repository = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};

		internal readonly GUIStyle m_Mapping = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleRight,
			fontStyle = FontStyle.Bold,
			fontSize = 16,
			richText = true
		};

		private static StyleAssets RateIndexer;

		internal static bool PostIndexer()
		{
			return RateIndexer == null;
		}
	}

	internal static TextureAssets _Object;

	internal static StyleAssets m_Utils;

	internal static SupportWindowAssets GetIndexer;

	[SpecialName]
	internal static TextureAssets ChangeWrapper()
	{
		return _Object ?? (_Object = new TextureAssets());
	}

	[SpecialName]
	internal static StyleAssets RegisterWrapper()
	{
		return m_Utils ?? (m_Utils = new StyleAssets());
	}

	internal static bool VisitIndexer()
	{
		return GetIndexer == null;
	}
}
