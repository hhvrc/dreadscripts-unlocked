using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class ReadableTextureScope : IDisposable
{
	internal bool adapterPolicy;

	internal Texture2D m_InterpreterPolicy;

	internal ReadableTextureScope(Texture2D config)
	{
		try
		{
			config.GetPixel(0, 0);
			adapterPolicy = false;
			m_InterpreterPolicy = config;
		}
		catch
		{
			int width = config.width;
			int height = config.height;
			adapterPolicy = true;
			config.filterMode = FilterMode.Point;
			RenderTexture temporary = RenderTexture.GetTemporary(width, height);
			temporary.filterMode = FilterMode.Point;
			RenderTexture.active = temporary;
			Graphics.Blit(config, temporary);
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			RenderTexture.active = null;
			m_InterpreterPolicy = texture2D;
		}
	}

	public void Dispose()
	{
		if (adapterPolicy)
		{
			UnityEngine.Object.DestroyImmediate(m_InterpreterPolicy);
		}
	}

	public static implicit operator Texture2D(ReadableTextureScope spec)
	{
		return spec.m_InterpreterPolicy;
	}
}
