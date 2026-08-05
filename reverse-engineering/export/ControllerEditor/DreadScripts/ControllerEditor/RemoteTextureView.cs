using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.ControllerEditor;

internal sealed class RemoteTextureView
{
	private Texture2D cachedTexture;

	private bool cacheLookupAllowed = true;

	internal Action onClick;

	private readonly string url;

	private readonly bool autoDownload;

	private readonly string sessionKey;

	internal bool isLoaded;

	internal bool isDownloading;

	private bool downloadAttempted;

	private bool layoutSettled;

	[SpecialName]
	internal Texture2D texture()
	{
		if (isLoaded)
		{
			if (cacheLookupAllowed && !cachedTexture)
			{
				TryLoadFromCache();
			}
			return cachedTexture;
		}
		if (isDownloading)
		{
			return null;
		}
		if (!autoDownload || downloadAttempted)
		{
			return null;
		}
		downloadAttempted = true;
		isDownloading = true;
		Download();
		return null;
	}

	internal RemoteTextureView(string item, bool checkselection, string res, bool isident2 = false)
		: this(delegate
		{
			Application.OpenURL("https://dreadrith.com/links");
		}, item, checkselection, res, isident2)
	{
	}

	internal RemoteTextureView(Action instance, string cont, bool controlstop, string second2, bool removeident3 = false)
	{
		url = cont;
		autoDownload = controlstop;
		sessionKey = second2;
		onClick = instance;
	}

	internal void Download()
	{
		if (TryLoadFromCache())
		{
			return;
		}
		UnityWebRequest m_ExporterPolicy = new UnityWebRequest(url)
		{
			downloadHandler = new DownloadHandlerBuffer()
		};
		m_ExporterPolicy.SendWebRequest().completed += delegate
		{
			if (!m_ExporterPolicy.isDone || m_ExporterPolicy.isHttpError || m_ExporterPolicy.isNetworkError)
			{
				m_ExporterPolicy.Dispose();
				return;
			}
			try
			{
				byte[] data = m_ExporterPolicy.downloadHandler.data;
				cachedTexture = new Texture2D(0, 0);
				cachedTexture.LoadImage(data);
				cachedTexture.Apply();
				isLoaded = true;
				if (!string.IsNullOrWhiteSpace(sessionKey))
				{
					CachedTextureContent.SaveTextureToSession(data, sessionKey);
					cacheLookupAllowed = true;
				}
			}
			finally
			{
				m_ExporterPolicy.Dispose();
			}
		};
		isDownloading = false;
	}

	internal bool TryLoadFromCache()
	{
		if (cacheLookupAllowed && !string.IsNullOrWhiteSpace(sessionKey))
		{
			cacheLookupAllowed = false;
			Texture2D texture2D = CachedTextureContent.LoadTextureFromSession(sessionKey);
			if (texture2D != null)
			{
				cachedTexture = texture2D;
				isLoaded = true;
				isDownloading = false;
				cacheLookupAllowed = true;
			}
		}
		return cachedTexture;
	}

	internal void Draw(float instance = 7f)
	{
		if (!IsReady())
		{
			DrawPlaceholderLayout(instance);
			return;
		}
		Rect aspectRect = GUILayoutUtility.GetAspectRect((float)texture().width / (float)texture().height);
		DrawTexture(aspectRect);
	}

	internal void PopHelper(EditorWindow res, float result = 0f, float serv = 60f, float first2 = 7f)
	{
		if (res == null)
		{
			Draw(first2);
		}
		else
		{
			ComputeHelper(res.position.width, res.position.height, result, serv, first2);
		}
	}

	internal void ComputeHelper(float key, float ivk, float dir = 0f, float col2 = 60f, float x3 = 7f)
	{
		if (!IsReady())
		{
			DrawPlaceholderLayout(x3);
			return;
		}
		float num = (float)texture().height / (float)texture().width;
		float num2 = key;
		float num3 = num2 * num;
		float num4 = ivk - col2;
		if (num3 > num4)
		{
			num3 = num4;
			num2 = num3 / num;
		}
		Rect rect = GUILayoutUtility.GetRect(num2, num3, GUILayout.ExpandWidth(expand: false));
		rect.x += (key - num2) / 2f + dir;
		DrawTexture(rect);
	}

	private void DrawTexture(Rect init)
	{
		if (onClick != null && EditorUtils.ClickArea(init))
		{
			onClick();
		}
		GUI.DrawTexture(init, texture());
	}

	private void DrawPlaceholderLayout(float res = 7f)
	{
		Rect aspectRect = GUILayoutUtility.GetAspectRect(res);
		DrawPlaceholder(aspectRect);
	}

	private void DrawPlaceholder(Rect def)
	{
		GUI.Box(def, GUIContent.none);
	}

	internal bool IsReady()
	{
		if (!layoutSettled)
		{
			if (texture() == null)
			{
				return false;
			}
			if (Event.current.type == EventType.Layout)
			{
				layoutSettled = true;
			}
			return true;
		}
		return true;
	}
}
