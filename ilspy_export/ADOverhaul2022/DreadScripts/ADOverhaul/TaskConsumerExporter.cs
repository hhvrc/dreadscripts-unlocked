using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class TaskConsumerExporter : IDisposable
{
	internal enum ColoringType
	{
		BG = 1,
		FG = 2,
		General = 4,
		All = 7
	}

	private readonly Color[] _ParamMethod = new Color[3];

	private readonly ColoringType m_PrototypeMethod;

	private bool m_BaseMethod;

	private static TaskConsumerExporter InvokeState;

	private void AddIterator()
	{
		m_BaseMethod = true;
		_ParamMethod[0] = GUI.backgroundColor;
		_ParamMethod[1] = GUI.contentColor;
		_ParamMethod[2] = GUI.color;
	}

	private void ValidateIterator(Color def)
	{
		AddIterator();
		if (m_PrototypeMethod.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = def;
		}
		if (m_PrototypeMethod.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = def;
		}
		if (m_PrototypeMethod.HasFlag(ColoringType.General))
		{
			GUI.color = def;
		}
	}

	internal TaskConsumerExporter(ColoringType setup, Color token)
	{
		m_PrototypeMethod = setup;
		ValidateIterator(token);
	}

	internal TaskConsumerExporter(ColoringType res, bool comparepred, Color c)
	{
		m_PrototypeMethod = res;
		if (comparepred)
		{
			ValidateIterator(c);
		}
	}

	internal TaskConsumerExporter(ColoringType v, bool ignorecaller, Color util, Color map2)
	{
		m_PrototypeMethod = v;
		ValidateIterator(ignorecaller ? util : map2);
	}

	internal TaskConsumerExporter(ColoringType i, int positionmap, params Color[] colors)
	{
		m_PrototypeMethod = i;
		if (positionmap >= 0)
		{
			AddIterator();
			ValidateIterator(colors[positionmap]);
		}
	}

	public void Dispose()
	{
		if (m_BaseMethod)
		{
			if (m_PrototypeMethod.HasFlag(ColoringType.BG))
			{
				GUI.backgroundColor = _ParamMethod[0];
			}
			if (m_PrototypeMethod.HasFlag(ColoringType.FG))
			{
				GUI.contentColor = _ParamMethod[1];
			}
			if (m_PrototypeMethod.HasFlag(ColoringType.General))
			{
				GUI.color = _ParamMethod[2];
			}
		}
	}

	internal static bool ConcatState()
	{
		return InvokeState == null;
	}
}
