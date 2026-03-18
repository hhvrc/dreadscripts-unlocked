using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class TaskServiceClass : IDisposable
{
	internal enum ColoringType
	{
		BG = 1,
		FG = 2,
		General = 4,
		All = 7
	}

	private readonly Color[] valueDic = new Color[3];

	private readonly ColoringType _RepositoryDic;

	private bool m_RefDic;

	internal static TaskServiceClass CloneDatabase;

	private void ExcludeMapping()
	{
		m_RefDic = true;
		valueDic[0] = GUI.backgroundColor;
		valueDic[1] = GUI.contentColor;
		valueDic[2] = GUI.color;
	}

	private void AddMapping(Color ident)
	{
		ExcludeMapping();
		if (_RepositoryDic.HasFlag(ColoringType.BG))
		{
			GUI.backgroundColor = ident;
		}
		if (_RepositoryDic.HasFlag(ColoringType.FG))
		{
			GUI.contentColor = ident;
		}
		if (_RepositoryDic.HasFlag(ColoringType.General))
		{
			GUI.color = ident;
		}
	}

	internal TaskServiceClass(ColoringType first, Color reg)
	{
		_RepositoryDic = first;
		AddMapping(reg);
	}

	internal TaskServiceClass(ColoringType item, bool removeb, Color control)
	{
		_RepositoryDic = item;
		if (removeb)
		{
			AddMapping(control);
		}
	}

	internal TaskServiceClass(ColoringType i, bool ordclose, Color util, Color vis2)
	{
		_RepositoryDic = i;
		AddMapping(ordclose ? util : vis2);
	}

	internal TaskServiceClass(ColoringType key, int row_ord, params Color[] colors)
	{
		_RepositoryDic = key;
		if (row_ord >= 0)
		{
			ExcludeMapping();
			AddMapping(colors[row_ord]);
		}
	}

	public void Dispose()
	{
		if (m_RefDic)
		{
			if (_RepositoryDic.HasFlag(ColoringType.BG))
			{
				GUI.backgroundColor = valueDic[0];
			}
			if (_RepositoryDic.HasFlag(ColoringType.FG))
			{
				GUI.contentColor = valueDic[1];
			}
			if (_RepositoryDic.HasFlag(ColoringType.General))
			{
				GUI.color = valueDic[2];
			}
		}
	}

	internal static bool ListDatabase()
	{
		return CloneDatabase == null;
	}
}
