using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies;

internal sealed class Customer
{
	internal enum TextureLayoutMethod
	{
		ScaleToFill,
		StretchToFill,
		ScaleToFit,
		Pattern
	}

	internal struct Writer
	{
		internal readonly bool _Params;

		internal readonly float _Listener;

		internal readonly float getter;

		internal readonly Vector2 interceptor;

		internal static object CustomizeIndexer;

		internal Writer(float init)
			: this(Vector2.zero, init, init)
		{
		}

		internal Writer(float def, float cfg)
			: this(Vector2.zero, def, cfg)
		{
		}

		internal Writer(Vector2 param, float reg)
			: this(param, reg, reg)
		{
		}

		internal Writer(Vector2 i, float selection, float template)
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

	internal static Customer InstantiateCode;

	[SpecialName]
	internal Texture2D ValidateWrapper()
	{
		if (_Registry)
		{
			if (m_Exporter && !m_Database)
			{
				CalculateWrapper();
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
			InstantiateWrapper();
			return null;
		}
		return null;
	}

	internal Customer(string var1, bool overridesecond, string control)
	{
		_Identifier = var1;
		attr = overridesecond;
		_Dispatcher = control;
	}

	internal void InstantiateWrapper()
	{
		if (CalculateWrapper())
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
					Info.LoginWrapper(data, _Dispatcher);
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

	internal void AwakeWrapper(Rect setup, Writer vis = default(Writer))
	{
		FlushWrapper(setup, TextureLayoutMethod.Pattern, vis);
	}

	internal void ResetWrapper(Rect setup)
	{
		FlushWrapper(setup, TextureLayoutMethod.StretchToFill);
	}

	internal void FlushWrapper(Rect spec, TextureLayoutMethod vis, Writer c = default(Writer))
	{
		if (!MapWrapper())
		{
			TestWrapper(spec);
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
				num2 = (num3 = ((float)ValidateWrapper().width / 256f + (float)ValidateWrapper().height / 256f) / 2f);
				position = new Vector2((float)ValidateWrapper().width / 2f, (float)ValidateWrapper().height / 2f);
			}
			float x = spec.width / (float)ValidateWrapper().width * num2;
			float y = spec.height / (float)ValidateWrapper().height * num3;
			GUI.DrawTextureWithTexCoords(texCoords: new Rect(position, new Vector2(x, y)), position: spec, image: ValidateWrapper());
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
		GUI.DrawTexture(spec, ValidateWrapper(), scaleMode);
	}

	internal void ConnectWrapper()
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

	internal bool CalculateWrapper()
	{
		if (m_Exporter && !string.IsNullOrWhiteSpace(_Dispatcher))
		{
			m_Exporter = false;
			Texture2D texture2D = Info.ReflectWrapper(_Dispatcher);
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

	private void TestWrapper(Rect def)
	{
		GUI.Box(def, GUIContent.none);
	}

	internal bool MapWrapper()
	{
		if (!order)
		{
			if (!(ValidateWrapper() == null))
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

	internal static bool RevertCode()
	{
		return InstantiateCode == null;
	}
}
