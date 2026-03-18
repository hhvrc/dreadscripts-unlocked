using System;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal sealed class Serializer : IDisposable
{
	internal enum ColoringType
	{
		BG = 1,
		FG = 2,
		General = 4,
		All = 7
	}

	private readonly Color[] m_Page = new Color[3];

	private readonly ColoringType resolver;

	private bool m_Predicate;

	internal static Serializer AssetCode;

	private void InitWrapper()
	{
		m_Predicate = true;
		m_Page[0] = GUI.backgroundColor;
		m_Page[1] = GUI.contentColor;
		m_Page[2] = GUI.color;
	}

	private void VisitWrapper(Color asset)
	{
		InitWrapper();
		if (resolver.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = asset;
		}
		if (resolver.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = asset;
		}
		if (resolver.HasFlag(ColoringType.General))
		{
			GUI.color = asset;
		}
	}

	internal Serializer(ColoringType ident, Color selection)
	{
		resolver = ident;
		VisitWrapper(selection);
	}

	public void Dispose()
	{
		if (m_Predicate)
		{
			if (resolver.HasFlag(ColoringType.BG))
			{
				GUI.backgroundColor = m_Page[0];
			}
			if (resolver.HasFlag(ColoringType.FG))
			{
				GUI.contentColor = m_Page[1];
			}
			if (resolver.HasFlag(ColoringType.General))
			{
				GUI.color = m_Page[2];
			}
		}
	}

	internal static bool SelectCode()
	{
		return AssetCode == null;
	}
}
