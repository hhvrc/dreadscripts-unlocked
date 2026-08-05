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

	private void ApplyColor(Color def)
	{
		Capture();
		if (channels.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = def;
		}
		if (channels.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = def;
		}
		if (channels.HasFlag(ColoringType.General))
		{
			GUI.color = def;
		}
	}

	internal GUIColorScope(ColoringType setup, Color token)
	{
		channels = setup;
		ApplyColor(token);
	}

	internal GUIColorScope(ColoringType res, bool comparepred, Color c)
	{
		channels = res;
		if (comparepred)
		{
			ApplyColor(c);
		}
	}

	internal GUIColorScope(ColoringType v, bool ignorecaller, Color util, Color map2)
	{
		channels = v;
		ApplyColor(ignorecaller ? util : map2);
	}

	internal GUIColorScope(ColoringType i, int positionmap, params Color[] colors)
	{
		channels = i;
		if (positionmap >= 0)
		{
			Capture();
			ApplyColor(colors[positionmap]);
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
