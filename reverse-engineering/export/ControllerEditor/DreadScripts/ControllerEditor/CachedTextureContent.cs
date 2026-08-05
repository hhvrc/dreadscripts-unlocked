using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class CachedTextureContent
{
	private GUIContent _content;

	private Texture2D _texture;

	private readonly string sessionKey;

	private readonly string tooltip;

	private bool hasTexture;

	[SpecialName]
	private GUIContent content()
	{
		if (hasTexture && _content.image == null)
		{
			Load();
		}
		return _content;
	}

	[SpecialName]
	private void content(GUIContent item)
	{
		_content = item;
	}

	[SpecialName]
	internal Texture2D texture()
	{
		if (hasTexture && _texture == null)
		{
			while (true)
			{
				Load();
			}
		}
		return _texture;
	}

	[SpecialName]
	internal void texture(Texture2D setup)
	{
		_texture = setup;
		hasTexture = _texture != null;
		if (hasTexture)
		{
			SaveTextureToSession(setup.EncodeToPNG(), sessionKey);
		}
		RebuildContent();
	}

	public CachedTextureContent(string i, string reg = "")
	{
		sessionKey = i;
		tooltip = reg;
		Load();
		RebuildContent();
	}

	private void Load()
	{
		texture(LoadTextureFromSession(sessionKey));
	}

	private void RebuildContent()
	{
		content(new GUIContent(texture(), tooltip));
	}

	private static byte[] IntsToBytes(int[] key)
	{
		byte[] array = new byte[key.Length];
		for (int i = 0; i < key.Length; i++)
		{
			array[i] = (byte)key[i];
		}
		return array;
	}

	private static int[] BytesToInts(byte[] value)
	{
		int num = value.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = value[i];
		}
		return array;
	}

	internal static Texture2D LoadTextureFromSession(string task)
	{
		int[] intArray = SessionState.GetIntArray(task, null);
		if (intArray != null)
		{
			try
			{
				byte[] data = IntsToBytes(intArray);
				Texture2D texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(data);
				texture2D.Apply();
				return texture2D;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				SessionState.EraseIntArray(task);
			}
		}
		return null;
	}

	internal static void SaveTextureToSession(byte[] v, string b)
	{
		int[] value = BytesToInts(v);
		SessionState.SetIntArray(b, value);
	}

	public static implicit operator GUIContent(CachedTextureContent param)
	{
		return param.content();
	}
}
