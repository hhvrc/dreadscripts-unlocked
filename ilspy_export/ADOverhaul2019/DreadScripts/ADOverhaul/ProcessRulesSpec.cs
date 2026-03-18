using System;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal sealed class ProcessRulesSpec : IDisposable
{
	private readonly bool m_StubDic;

	internal static ProcessRulesSpec CreateDatabase;

	internal ProcessRulesSpec(ref Vector2 spec)
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

	internal static bool CancelDatabase()
	{
		return CreateDatabase == null;
	}
}
