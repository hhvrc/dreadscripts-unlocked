using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class TemplateThread : IDisposable
{
	internal enum ColoringType
	{
		BG = 1,
		FG = 2,
		General = 4,
		All = 7
	}

	private readonly Color[] _MessageThread = new Color[3];

	private readonly ColoringType _CollectionThread;

	private bool m_ParserThread;

	internal static TemplateThread SetupStatus;

	private void CollectRecord()
	{
		m_ParserThread = true;
		_MessageThread[0] = GUI.backgroundColor;
		_MessageThread[1] = GUI.contentColor;
		_MessageThread[2] = GUI.color;
	}

	private void ResolveRecord(Color config)
	{
		CollectRecord();
		if (_CollectionThread.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = config;
		}
		if (_CollectionThread.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = config;
		}
		if (_CollectionThread.HasFlag(ColoringType.General))
		{
			GUI.color = config;
		}
	}

	internal TemplateThread(ColoringType v, Color connection)
	{
		_CollectionThread = v;
		ResolveRecord(connection);
	}

	internal TemplateThread(ColoringType setup, bool wantconnection, Color proc)
	{
		_CollectionThread = setup;
		if (wantconnection)
		{
			ResolveRecord(proc);
		}
	}

	internal TemplateThread(ColoringType def, bool skipcont, Color proc, Color config2)
	{
		_CollectionThread = def;
		ResolveRecord(skipcont ? proc : config2);
	}

	internal TemplateThread(ColoringType res, int visZ, params Color[] colors)
	{
		_CollectionThread = res;
		if (visZ >= 0)
		{
			CollectRecord();
			ResolveRecord(colors[visZ]);
		}
	}

	public void Dispose()
	{
		if (m_ParserThread)
		{
			if (_CollectionThread.HasFlag(ColoringType.BG))
			{
				GUI.backgroundColor = _MessageThread[0];
			}
			if (_CollectionThread.HasFlag(ColoringType.FG))
			{
				GUI.contentColor = _MessageThread[1];
			}
			if (_CollectionThread.HasFlag(ColoringType.General))
			{
				GUI.color = _MessageThread[2];
			}
		}
	}

	internal static bool ExcludeStatus()
	{
		return SetupStatus == null;
	}
}
