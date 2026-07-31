using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class GUIColorScope : IDisposable
{
	internal enum ColoringType
	{
		BG = 1,
		FG = 2,
		General = 4,
		All = 7
	}

	private readonly Color[] savedColors = new Color[3];

	private readonly ColoringType channels;

	private bool captured;

	private void Capture()
	{
		captured = true;
		savedColors[0] = GUI.backgroundColor;
		savedColors[1] = GUI.contentColor;
		savedColors[2] = GUI.color;
	}

	private void ApplyColor(Color ident)
	{
		Capture();
		if (channels.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = ident;
		}
		if (channels.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = ident;
		}
		if (channels.HasFlag(ColoringType.General))
		{
			GUI.color = ident;
		}
	}

	internal GUIColorScope(ColoringType first, Color reg)
	{
		channels = first;
		ApplyColor(reg);
	}

	internal GUIColorScope(ColoringType item, bool removeb, Color control)
	{
		channels = item;
		if (removeb)
		{
			ApplyColor(control);
		}
	}

	internal GUIColorScope(ColoringType i, bool ordclose, Color util, Color vis2)
	{
		channels = i;
		ApplyColor(ordclose ? util : vis2);
	}

	internal GUIColorScope(ColoringType key, int row_ord, params Color[] colors)
	{
		channels = key;
		if (row_ord >= 0)
		{
			Capture();
			ApplyColor(colors[row_ord]);
		}
	}

	public void Dispose()
	{
		if (captured)
		{
			if (channels.HasFlag(ColoringType.BG))
			{
				GUI.backgroundColor = savedColors[0];
			}
			if (channels.HasFlag(ColoringType.FG))
			{
				GUI.contentColor = savedColors[1];
			}
			if (channels.HasFlag(ColoringType.General))
			{
				GUI.color = savedColors[2];
			}
		}
	}
}
