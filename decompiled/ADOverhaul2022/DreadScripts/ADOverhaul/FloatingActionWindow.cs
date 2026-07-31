using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal class FloatingActionWindow : EditorWindow
{
	private float measuredWidth;

	private float measuredHeight;

	private int passIndex;

	private Vector2 scrollPosition;

	private bool isInitialized;

	private Action drawAction;

	private Action measureAction;

	private float statusSerializer;

	private float valSerializer;

	private bool adapterSerializer;

	private bool proxySerializer;

	public bool m_RefSerializer = true;

	public bool _ComparatorSerializer = true;

	private static FloatingActionWindow instance;

	private void Initialize(Action key, Action pol, float pool = 100f, float init2 = 100f, bool forcet3 = true, bool isvisitor4 = true, bool extractpol5 = true)
	{
		drawAction = key;
		measureAction = pol;
		statusSerializer = pool;
		valSerializer = init2;
		adapterSerializer = forcet3;
		proxySerializer = isvisitor4;
		m_RefSerializer = extractpol5;
		isInitialized = true;
	}

	public void OnGUI()
	{
		if (isInitialized)
		{
			Event current = Event.current;
			using (new ScrollViewScope(ref scrollPosition))
			{
				EventType type = current.type;
				int num = passIndex;
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
				passIndex++;
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
							if (measureAction != null)
							{
								measureAction();
							}
							else
							{
								drawAction();
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
						drawAction();
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
						base.position = new Rect(base.position.x, base.position.y, measuredWidth, measuredHeight);
					}
				}
				else
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					measuredWidth = lastRect.width;
					measuredHeight = lastRect.height;
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

	internal static void Open(Rect value, Action col, Action proc, float item2 = 100f, float second3 = 100f, bool isattr4 = true, bool istask5 = true, bool usecol6 = true)
	{
		if (instance != null)
		{
			try
			{
				instance.Close();
			}
			catch
			{
				Debug.Log("Failed close");
				try
				{
					UnityEngine.Object.DestroyImmediate(instance);
				}
				catch
				{
					Debug.Log("Failed destroy??");
				}
			}
			instance = null;
		}
		instance = ScriptableObject.CreateInstance<FloatingActionWindow>();
		instance.Initialize(col, proc, item2, second3, isattr4, istask5, usecol6);
		instance.ShowUtility();
		instance.position = value;
	}
}
