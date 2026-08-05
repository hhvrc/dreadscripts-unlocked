using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

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

	private void ApplyColor(Color config)
	{
		Capture();
		if (channels.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = config;
		}
		if (channels.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = config;
		}
		if (channels.HasFlag(ColoringType.General))
		{
			GUI.color = config;
		}
	}

	internal GUIColorScope(ColoringType v, Color connection)
	{
		channels = v;
		ApplyColor(connection);
	}

	internal GUIColorScope(ColoringType setup, bool wantconnection, Color proc)
	{
		channels = setup;
		if (wantconnection)
		{
			ApplyColor(proc);
		}
	}

	internal GUIColorScope(ColoringType def, bool skipcont, Color proc, Color config2)
	{
		channels = def;
		ApplyColor(skipcont ? proc : config2);
	}

	internal GUIColorScope(ColoringType res, int visZ, params Color[] colors)
	{
		channels = res;
		if (visZ >= 0)
		{
			Capture();
			ApplyColor(colors[visZ]);
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
