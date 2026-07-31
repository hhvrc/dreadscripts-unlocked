using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class ReadableTextureScope : IDisposable
{
	internal bool isTemporary;

	internal Texture2D texture;

	internal ReadableTextureScope(Texture2D config)
	{
		try
		{
			config.GetPixel(0, 0);
			isTemporary = false;
			texture = config;
		}
		catch
		{
			int width = config.width;
			int height = config.height;
			isTemporary = true;
			config.filterMode = FilterMode.Point;
			RenderTexture temporary = RenderTexture.GetTemporary(width, height);
			temporary.filterMode = FilterMode.Point;
			RenderTexture.active = temporary;
			Graphics.Blit(config, temporary);
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			RenderTexture.active = null;
			texture = texture2D;
		}
	}

	public void Dispose()
	{
		if (isTemporary)
		{
			UnityEngine.Object.DestroyImmediate(texture);
		}
	}

	public static implicit operator Texture2D(ReadableTextureScope spec)
	{
		return spec.texture;
	}
}
