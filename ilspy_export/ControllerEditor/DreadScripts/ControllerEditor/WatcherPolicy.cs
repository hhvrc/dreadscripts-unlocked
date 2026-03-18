using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.ControllerEditor;

internal sealed class WatcherPolicy
{
	private Texture2D candidatePolicy;

	private bool _ProductPolicy = true;

	internal Action _ExpressionPolicy;

	private readonly string systemPolicy;

	private readonly bool workerPolicy;

	private readonly string _FilterPolicy;

	internal bool m_StubPolicy;

	internal bool m_ReaderPolicy;

	private bool _BridgePolicy;

	private bool m_StrategyPolicy;

	private static WatcherPolicy LogoutDecorator;

	[SpecialName]
	internal Texture2D CountHelper()
	{
		if (m_StubPolicy)
		{
			if (_ProductPolicy && !candidatePolicy)
			{
				EnableHelper();
			}
			return candidatePolicy;
		}
		if (m_ReaderPolicy)
		{
			return null;
		}
		if (!workerPolicy || _BridgePolicy)
		{
			return null;
		}
		_BridgePolicy = true;
		m_ReaderPolicy = true;
		SetupHelper();
		return null;
	}

	internal WatcherPolicy(string item, bool checkselection, string res, bool isident2 = false)
		: this(delegate
		{
			Application.OpenURL("https://dreadrith.com/links");
		}, item, checkselection, res, isident2)
	{
	}

	internal WatcherPolicy(Action instance, string cont, bool controlstop, string second2, bool removeident3 = false)
	{
		systemPolicy = cont;
		workerPolicy = controlstop;
		_FilterPolicy = second2;
		_ExpressionPolicy = instance;
	}

	internal void SetupHelper()
	{
		if (!EnableHelper())
		{
			UnityWebRequest m_ExporterPolicy = new UnityWebRequest(systemPolicy)
			{
				downloadHandler = new DownloadHandlerBuffer()
			};
			m_ReaderPolicy = false;
		}
	}

	internal bool EnableHelper()
	{
		if (_ProductPolicy && !string.IsNullOrWhiteSpace(_FilterPolicy))
		{
			_ProductPolicy = false;
			Texture2D texture2D = ErrorPolicy.ManageRecord(_FilterPolicy);
			if (texture2D != null)
			{
				candidatePolicy = texture2D;
				m_StubPolicy = true;
				m_ReaderPolicy = false;
				_ProductPolicy = true;
			}
		}
		return candidatePolicy;
	}

	internal void PublishHelper(float instance = 7f)
	{
		if (!CancelHelper())
		{
			ConcatHelper(instance);
			return;
		}
		Rect aspectRect = GUILayoutUtility.GetAspectRect((float)CountHelper().width / (float)CountHelper().height);
		MoveHelper(aspectRect);
	}

	internal void PopHelper(EditorWindow res, float result = 0f, float serv = 60f, float first2 = 7f)
	{
		if (res == null)
		{
			PublishHelper(first2);
		}
		else
		{
			ComputeHelper(res.position.width, res.position.height, result, serv, first2);
		}
	}

	internal void ComputeHelper(float key, float ivk, float dir = 0f, float col2 = 60f, float x3 = 7f)
	{
		if (!CancelHelper())
		{
			ConcatHelper(x3);
			return;
		}
		float num = (float)CountHelper().height / (float)CountHelper().width;
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
		MoveHelper(rect);
	}

	private void MoveHelper(Rect init)
	{
		if (_ExpressionPolicy != null && ClassProperty.DefineQueue(init))
		{
			_ExpressionPolicy();
		}
		GUI.DrawTexture(init, CountHelper());
	}

	private void ConcatHelper(float res = 7f)
	{
		Rect aspectRect = GUILayoutUtility.GetAspectRect(res);
		CallHelper(aspectRect);
	}

	private void CallHelper(Rect def)
	{
		GUI.Box(def, GUIContent.none);
	}

	internal bool CancelHelper()
	{
		if (!m_StrategyPolicy)
		{
			if (CountHelper() == null)
			{
				return false;
			}
			if (Event.current.type == EventType.Layout)
			{
				m_StrategyPolicy = true;
			}
			return true;
		}
		return true;
	}

	internal static bool FindDecorator()
	{
		return LogoutDecorator == null;
	}
}
