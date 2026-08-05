using System;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal sealed class GuiColorScope : IDisposable
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

	private void Capture()
	{
		m_Predicate = true;
		m_Page[0] = GUI.backgroundColor;
		m_Page[1] = GUI.contentColor;
		m_Page[2] = GUI.color;
	}

	private void ApplyColor(Color asset)
	{
		Capture();
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

	internal GuiColorScope(ColoringType ident, Color selection)
	{
		resolver = ident;
		ApplyColor(selection);
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
}
