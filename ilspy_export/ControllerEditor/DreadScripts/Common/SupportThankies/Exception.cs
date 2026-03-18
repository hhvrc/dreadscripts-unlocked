using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class Exception
{
	internal class Value
	{
		internal readonly Customer merchant = new Customer("https://i.imgur.com/iHszIY3.png", overridesecond: true, "ds-supporters-main");

		internal readonly Customer _Authentication = new Customer("https://i.imgur.com/FMv1R6A.png", overridesecond: true, "ds-supporters-kofi");

		internal static Value StopIndexer;

		internal static bool ReflectIndexer()
		{
			return StopIndexer == null;
		}
	}

	internal class Reponse
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

		private static Reponse RateIndexer;

		internal static bool PostIndexer()
		{
			return RateIndexer == null;
		}
	}

	internal static Value _Object;

	internal static Reponse m_Utils;

	internal static Exception GetIndexer;

	[SpecialName]
	internal static Value ChangeWrapper()
	{
		return _Object ?? (_Object = new Value());
	}

	[SpecialName]
	internal static Reponse RegisterWrapper()
	{
		return m_Utils ?? (m_Utils = new Reponse());
	}

	internal static bool VisitIndexer()
	{
		return GetIndexer == null;
	}
}
