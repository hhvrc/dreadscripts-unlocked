using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal class ExpressionIdentifierService : EditorWindow
{
	private float _AccountSerializer;

	private float _ParamsSerializer;

	private int importerSerializer;

	private Vector2 m_ServerSerializer;

	private bool m_WatcherSerializer;

	private Action _RegSerializer;

	private Action m_ProcessSerializer;

	private float statusSerializer;

	private float valSerializer;

	private bool adapterSerializer;

	private bool proxySerializer;

	public bool m_RefSerializer = true;

	public bool _ComparatorSerializer = true;

	private static ExpressionIdentifierService productSerializer;

	internal static ExpressionIdentifierService LogoutOrder;

	private void CheckProcess(Action key, Action pol, float pool = 100f, float init2 = 100f, bool forcet3 = true, bool isvisitor4 = true, bool extractpol5 = true)
	{
		_RegSerializer = key;
		m_ProcessSerializer = pol;
		statusSerializer = pool;
		valSerializer = init2;
		adapterSerializer = forcet3;
		proxySerializer = isvisitor4;
		m_RefSerializer = extractpol5;
		m_WatcherSerializer = true;
	}

	public void OnGUI()
	{
		if (m_WatcherSerializer)
		{
			Event current = Event.current;
			using (new IssuerMethod(ref m_ServerSerializer))
			{
				EventType type = current.type;
				int num = importerSerializer;
				bool flag;
				bool flag2;
				if (num != 0)
				{
					if (num != 1)
					{
						flag = false;
						flag2 = false;
					}
					else
					{
						flag2 = false;
						flag = true;
					}
				}
				else
				{
					flag2 = true;
					flag = false;
				}
				importerSerializer++;
				using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(expand: false)))
				{
					if (flag2)
					{
						Color backgroundColor = GUI.backgroundColor;
						Color contentColor = GUI.contentColor;
						Color color = GUI.color;
						try
						{
							GUI.backgroundColor = (GUI.contentColor = (GUI.color = Color.clear));
							if (m_ProcessSerializer != null)
							{
								m_ProcessSerializer();
							}
							else
							{
								_RegSerializer();
							}
						}
						finally
						{
							GUI.backgroundColor = backgroundColor;
							GUI.contentColor = contentColor;
							GUI.color = color;
						}
					}
					else
					{
						_RegSerializer();
					}
				}
				if (type != EventType.Repaint)
				{
					return;
				}
				if (!flag2)
				{
					if (flag)
					{
						base.position = new Rect(base.position.x, base.position.y, _AccountSerializer, _ParamsSerializer);
					}
				}
				else
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					_AccountSerializer = lastRect.width;
					_ParamsSerializer = lastRect.height;
				}
				return;
			}
		}
		Close();
	}

	private void OnLostFocus()
	{
		Close();
	}

	internal static void CallProcess(Rect value, Action col, Action proc, float item2 = 100f, float second3 = 100f, bool isattr4 = true, bool istask5 = true, bool usecol6 = true)
	{
		if (productSerializer != null)
		{
			try
			{
				productSerializer.Close();
			}
			catch
			{
				Debug.Log("Failed close");
				try
				{
					UnityEngine.Object.DestroyImmediate(productSerializer);
				}
				catch
				{
					Debug.Log("Failed destroy??");
				}
			}
			productSerializer = null;
		}
		productSerializer = ScriptableObject.CreateInstance<ExpressionIdentifierService>();
		productSerializer.CheckProcess(col, proc, item2, second3, isattr4, istask5, usecol6);
		productSerializer.ShowUtility();
		productSerializer.position = value;
	}

	internal static bool CreateOrder()
	{
		return (object)LogoutOrder == null;
	}
}
