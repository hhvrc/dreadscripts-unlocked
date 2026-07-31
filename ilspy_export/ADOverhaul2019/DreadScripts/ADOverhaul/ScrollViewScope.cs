using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class ScrollViewScope : IDisposable
{
	private readonly bool m_StubDic;

	internal ScrollViewScope(ref Vector2 spec)
	{
		try
		{
			spec = GUILayout.BeginScrollView(spec);
			m_StubDic = true;
		}
		catch
		{
			m_StubDic = false;
		}
	}

	public void Dispose()
	{
		if (m_StubDic)
		{
			GUILayout.EndScrollView();
		}
	}
}
