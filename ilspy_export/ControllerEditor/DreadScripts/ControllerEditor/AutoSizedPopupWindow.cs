using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class AutoSizedPopupWindow : EditorWindow
{
	private float factoryThread;

	private float _AccountThread;

	private int refThread;

	private Vector2 m_StatusThread;

	private bool _TokenThread;

	private Action codeThread;

	private Action dicThread;

	private float invocationThread;

	private float m_RoleThread;

	private bool paramThread;

	private bool _ModelThread;

	public bool tokenizerThread = true;

	public bool decoratorThread = true;

	private static AutoSizedPopupWindow comparatorThread;

	internal static AutoSizedPopupWindow WriteStatus;

	private void QueryRecord(Action value, Action col, float pool = 100f, float reference2 = 100f, bool countinfo3 = true, bool isspec4 = true, bool rejectinfo5 = true)
	{
		codeThread = value;
		dicThread = col;
		invocationThread = pool;
		m_RoleThread = reference2;
		paramThread = countinfo3;
		_ModelThread = isspec4;
		tokenizerThread = rejectinfo5;
		_TokenThread = true;
	}

	public void OnGUI()
	{
		if (!_TokenThread)
		{
			Close();
			return;
		}
		Event current = Event.current;
		using (new SpecificationThread(ref m_StatusThread))
		{
			EventType type = current.type;
			int num = refThread;
			bool flag;
			bool flag2;
			if (num == 0)
			{
				flag = true;
				flag2 = false;
			}
			else if (num == 1)
			{
				flag = false;
				flag2 = true;
			}
			else
			{
				flag2 = false;
				flag = false;
			}
			refThread++;
			using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(expand: false)))
			{
				if (flag)
				{
					Color backgroundColor = GUI.backgroundColor;
					Color contentColor = GUI.contentColor;
					Color color = GUI.color;
					try
					{
						GUI.backgroundColor = (GUI.contentColor = (GUI.color = Color.clear));
						if (dicThread == null)
						{
							codeThread();
						}
						else
						{
							dicThread();
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
					codeThread();
				}
			}
			if (type != EventType.Repaint)
			{
				return;
			}
			if (!flag)
			{
				if (flag2)
				{
					base.position = new Rect(base.position.x, base.position.y, factoryThread, _AccountThread);
				}
			}
			else
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				factoryThread = lastRect.width;
				_AccountThread = lastRect.height;
			}
		}
	}

	private void OnLostFocus()
	{
		Close();
	}

	internal static void AddRecord(Rect task, Action cont, Action temp, float col2 = 100f, float param3 = 100f, bool isselection4 = true, bool dores5 = true, bool isreference6 = true)
	{
		if (comparatorThread != null)
		{
			try
			{
				comparatorThread.Close();
			}
			catch
			{
				Debug.Log("Failed close");
				try
				{
					UnityEngine.Object.DestroyImmediate(comparatorThread);
				}
				catch
				{
					Debug.Log("Failed destroy??");
				}
			}
			comparatorThread = null;
		}
		comparatorThread = ScriptableObject.CreateInstance<AutoSizedPopupWindow>();
		comparatorThread.QueryRecord(cont, temp, col2, param3, isselection4, dores5, isreference6);
		comparatorThread.ShowUtility();
		comparatorThread.position = task;
	}

	internal static bool RemoveStatus()
	{
		return (object)WriteStatus == null;
	}
}
