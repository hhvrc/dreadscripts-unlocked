using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies;

internal sealed class RemoteTexture
{
	internal enum TextureLayoutMethod
	{
		ScaleToFill,
		StretchToFill,
		ScaleToFit,
		Pattern
	}

	internal struct TextureDisplayParams
	{
		internal readonly bool _Params;

		internal readonly float _Listener;

		internal readonly float getter;

		internal readonly Vector2 interceptor;

		internal static object CustomizeIndexer;

		internal TextureDisplayParams(float init)
			: this(Vector2.zero, init, init)
		{
		}

		internal TextureDisplayParams(float def, float cfg)
			: this(Vector2.zero, def, cfg)
		{
		}

		internal TextureDisplayParams(Vector2 param, float reg)
			: this(param, reg, reg)
		{
		}

		internal TextureDisplayParams(Vector2 i, float selection, float template)
		{
			_Params = true;
			_Listener = selection;
			getter = template;
			interceptor = i;
		}

		internal static bool SearchIndexer()
		{
			return CustomizeIndexer == null;
		}
	}

	private Texture2D m_Database;

	private bool m_Exporter = true;

	private readonly string _Identifier;

	private readonly bool attr;

	private readonly string _Dispatcher;

	internal bool _Registry;

	internal bool importer;

	private bool printer;

	private bool order;

	[SpecialName]
	internal Texture2D GetTexture()
	{
		if (_Registry)
		{
			if (m_Exporter && !m_Database)
			{
				TryLoadFromCache();
			}
			return m_Database;
		}
		if (importer)
		{
			return null;
		}
		if (attr && !printer)
		{
			printer = true;
			importer = true;
			Download();
			return null;
		}
		return null;
	}

	internal RemoteTexture(string var1, bool overridesecond, string control)
	{
		_Identifier = var1;
		attr = overridesecond;
		_Dispatcher = control;
	}

	internal void Download()
	{
		if (TryLoadFromCache())
		{
			return;
		}
		UnityWebRequest creator = new UnityWebRequest(_Identifier)
		{
			downloadHandler = new DownloadHandlerBuffer()
		};
		creator.SendWebRequest().completed += delegate
		{
			if (!creator.isDone || creator.isHttpError || creator.isNetworkError)
			{
				creator.Dispose();
				return;
			}
			try
			{
				byte[] data = creator.downloadHandler.data;
				m_Database = new Texture2D(0, 0);
				m_Database.LoadImage(data);
				m_Database.Apply();
				_Registry = true;
				if (!string.IsNullOrWhiteSpace(_Dispatcher))
				{
					EditorGuiUtils.SaveTextureToSession(data, _Dispatcher);
					m_Exporter = true;
				}
			}
			finally
			{
				creator.Dispose();
			}
		};
		importer = false;
	}

	internal void DrawPattern(Rect setup, TextureDisplayParams vis = default(TextureDisplayParams))
	{
		Draw(setup, TextureLayoutMethod.Pattern, vis);
	}

	internal void Draw(Rect setup)
	{
		Draw(setup, TextureLayoutMethod.StretchToFill);
	}

	internal void Draw(Rect spec, TextureLayoutMethod vis, TextureDisplayParams c = default(TextureDisplayParams))
	{
		if (!IsReady())
		{
			DrawPlaceholder(spec);
			return;
		}
		int num;
		switch (vis)
		{
		case TextureLayoutMethod.Pattern:
		{
			float num2;
			float num3;
			Vector2 position;
			if (c._Params)
			{
				num2 = c._Listener;
				num3 = c.getter;
				position = c.interceptor;
			}
			else
			{
				num2 = (num3 = ((float)GetTexture().width / 256f + (float)GetTexture().height / 256f) / 2f);
				position = new Vector2((float)GetTexture().width / 2f, (float)GetTexture().height / 2f);
			}
			float x = spec.width / (float)GetTexture().width * num2;
			float y = spec.height / (float)GetTexture().height * num3;
			GUI.DrawTextureWithTexCoords(texCoords: new Rect(position, new Vector2(x, y)), position: spec, image: GetTexture());
			return;
		}
		case TextureLayoutMethod.ScaleToFit:
			num = 2;
			break;
		default:
			num = 0;
			break;
		case TextureLayoutMethod.ScaleToFill:
			num = 1;
			break;
		}
		ScaleMode scaleMode = (ScaleMode)num;
		GUI.DrawTexture(spec, GetTexture(), scaleMode);
	}

	internal void Clear()
	{
		if (!string.IsNullOrEmpty(_Dispatcher))
		{
			SessionState.EraseIntArray(_Dispatcher);
		}
		m_Database = null;
		order = false;
		printer = false;
		_Registry = false;
		importer = false;
		m_Exporter = true;
	}

	internal bool TryLoadFromCache()
	{
		if (m_Exporter && !string.IsNullOrWhiteSpace(_Dispatcher))
		{
			m_Exporter = false;
			Texture2D texture2D = EditorGuiUtils.LoadTextureFromSession(_Dispatcher);
			if (texture2D != null)
			{
				m_Database = texture2D;
				_Registry = true;
				importer = false;
				m_Exporter = true;
			}
		}
		return m_Database;
	}

	private void DrawPlaceholder(Rect def)
	{
		GUI.Box(def, GUIContent.none);
	}

	internal bool IsReady()
	{
		if (!order)
		{
			if (!(GetTexture() == null))
			{
				if (Event.current.type == EventType.Layout)
				{
					order = true;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
