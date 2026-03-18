using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies;

internal class SupportThankies : EditorWindow
{
	private static bool m_Advisor;

	private static bool _Callback;

	private static bool m_Indexer;

	private static string issuer;

	private static GUIContent m_Prototype;

	private static Rules[] rule;

	private static string m_Singleton;

	private static Rect _Factory = Rect.zero;

	private static Rect _Account = Rect.zero;

	private static Vector2 _Ref;

	private static object m_Status = Definition.SetWrapper(1f);

	private static object code = Definition.SetWrapper(1f);

	private static int m_Dic = 1;

	private static int _Invocation = 1;

	private static SupportThankies RunIndexer;

	[SpecialName]
	private static bool AssetWrapper()
	{
		if (_Callback)
		{
			return true;
		}
		return m_Indexer;
	}

	private static void ResolveWrapper()
	{
		m_Prototype = new GUIContent(Test.m_Model.DestroyWrapper(), Test.m_Tokenizer.DestroyWrapper());
	}

	public static void ListWrapper()
	{
		Rect controlRect = EditorGUILayout.GetControlRect(false, 16f, GUIStyle.none, GUILayout.Width(16f));
		controlRect.x -= 2f;
		Exception.ChangeWrapper().merchant.ResetWrapper(controlRect);
		if (Info.PushWrapper(controlRect))
		{
			VerifyWrapper();
		}
	}

	public static void VerifyWrapper()
	{
		EditorWindow.GetWindow<SupportThankies>(Test._Role.DestroyWrapper()).titleContent.image = Exception.ChangeWrapper().merchant.ValidateWrapper();
	}

	public void OnGUI()
	{
		if (!AssetWrapper() && !m_Advisor)
		{
			StopWrapper();
		}
		if (m_Advisor)
		{
			GUILayout.Label("Loading supporters...", Exception.RegisterWrapper()._Pool);
		}
		if (m_Indexer)
		{
			GUILayout.Label("Failed to load supporters.", Exception.RegisterWrapper()._Pool);
			if (!string.IsNullOrWhiteSpace(issuer))
			{
				EditorGUILayout.HelpBox(issuer, MessageType.Error);
			}
			if (Info.CreateWrapper("Retry", EditorStyles.toolbarButton))
			{
				PrepareWrapper();
			}
		}
		if (_Callback)
		{
			using (new GUILayout.HorizontalScope("in bigtitle"))
			{
				GUILayout.Label(m_Prototype, Exception.RegisterWrapper()._Pool);
			}
			FillWrapper();
		}
		WriteWrapper();
	}

	public void FillWrapper()
	{
		Event current = Event.current;
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(60f), GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true));
		EventType type = current.type;
		if (type != EventType.Repaint)
		{
			if (type == EventType.Layout)
			{
				_Factory = _Account;
			}
		}
		else
		{
			_Account = controlRect;
		}
		int num = rule.Length;
		float f = _Factory.width / _Factory.height;
		float v = 25f;
		int num2 = Mathf.Clamp(Mathf.Min(Mathf.RoundToInt(f), Mathf.CeilToInt(29f * (float)num / _Factory.height)), 1, num);
		int num3 = Mathf.CeilToInt((float)num / (float)num2);
		if (m_Dic != num2)
		{
			m_Dic = num2;
			m_Status = Definition.SetWrapper(Enumerable.Repeat(1f, num2).ToArray());
		}
		if (_Invocation != num3)
		{
			_Invocation = num3;
			code = Definition.SetWrapper(Enumerable.Repeat(1f, num3).ToArray());
		}
		GUILayout.BeginArea(_Factory);
		_Ref = EditorGUILayout.BeginScrollView(_Ref);
		int num4 = 0;
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Space(4f);
			Definition.EnableWrapper(m_Status, null, false);
			for (int i = 0; i < num2; i++)
			{
				using (new GUILayout.HorizontalScope())
				{
					using (new GUILayout.VerticalScope())
					{
						Definition.EnableWrapper(code, null, true);
						for (int j = 0; j < num3; j++)
						{
							if (num4 < rule.Length)
							{
								rule[num4++].DefineWrapper(v);
							}
							else
							{
								GUILayout.Label(GUIContent.none);
							}
						}
						Definition.PublishWrapper();
					}
					if (i < num2 - 1)
					{
						GUILayout.Space(4f);
					}
				}
			}
			Definition.PublishWrapper();
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	public static void WriteWrapper()
	{
		Rect rect = GUILayoutUtility.GetRect(100f, 200f, 16f, 32f);
		Rect setup = Info.CalcWrapper(rect, 6.25f);
		GUI.DrawTexture(rect, Info.CloneWrapper(Color.white), ScaleMode.StretchToFill, alphaBlend: false, 0f, new Color(0.075f, 0.765f, 1f), 0f, 8f);
		Exception.ChangeWrapper()._Authentication.ResetWrapper(setup);
		if (Info.PushWrapper(rect))
		{
			ForgotWrapper();
		}
	}

	public static void ForgotWrapper()
	{
		Application.OpenURL("https://ko-fi.com/dreadrith");
	}

	public async Task StopWrapper()
	{
		if (AssetWrapper() || m_Advisor)
		{
			return;
		}
		m_Advisor = true;
		Annotation awb = new Annotation("https://storage.googleapis.com/dreadscripts-c6b62.appspot.com/Dreadscripts/Supporters.txt", "GET");
		try
		{
			UnityWebRequest request = awb._Algo;
			request.useHttpContinue = false;
			request.downloadHandler = new DownloadHandlerBuffer();
			request.timeout = 10;
			await awb.Process();
			m_Advisor = false;
			if (awb.OrderWrapper())
			{
				m_Indexer = true;
				issuer = request.error;
				return;
			}
			try
			{
				m_Singleton = request.downloadHandler.text;
				CheckWrapper();
				_Callback = true;
			}
			catch (System.Exception ex)
			{
				m_Indexer = true;
				issuer = ex.ToString();
				throw;
			}
		}
		finally
		{
			((IDisposable)awb/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public void CheckWrapper()
	{
		string[] array = m_Singleton.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		rule = new Rules[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			rule[i] = new Rules(array[i]);
		}
		Repaint();
	}

	public void OnEnable()
	{
		ResolveWrapper();
	}

	public static void PrepareWrapper()
	{
		m_Indexer = false;
		_Callback = false;
		m_Advisor = false;
		issuer = null;
	}

	internal static bool ComputeIndexer()
	{
		return (object)RunIndexer == null;
	}
}
