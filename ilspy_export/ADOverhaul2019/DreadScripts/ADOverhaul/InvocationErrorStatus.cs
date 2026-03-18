using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul;

internal class InvocationErrorStatus : EditorWindow
{
	private float _ErrorTemplate;

	private float taskTemplate;

	private int _ProducerTemplate;

	private Vector2 methodTemplate;

	private bool resolverTemplate;

	private Action _IteratorTemplate;

	private Action _RulesTemplate;

	private float _TokenizerTemplate;

	private float specificationTemplate;

	private bool accountTemplate;

	private bool managerTemplate;

	public bool _ParamTemplate = true;

	public bool m_EventTemplate = true;

	private static InvocationErrorStatus _TestsTemplate;

	internal static InvocationErrorStatus PostFactory;

	private void VisitAccount(Action key, Action vis, float field = 100f, float selection2 = 100f, bool ignorepol3 = true, bool moveconfig4 = true, bool issecond5 = true)
	{
		_IteratorTemplate = key;
		_RulesTemplate = vis;
		_TokenizerTemplate = field;
		specificationTemplate = selection2;
		accountTemplate = ignorepol3;
		managerTemplate = moveconfig4;
		_ParamTemplate = issecond5;
		resolverTemplate = true;
	}

	public void OnGUI()
	{
		if (!resolverTemplate)
		{
			Close();
			return;
		}
		Event current = Event.current;
		using (new ProcessRulesSpec(ref methodTemplate))
		{
			EventType type = current.type;
			int producerTemplate = _ProducerTemplate;
			bool flag;
			bool flag2;
			if (producerTemplate != 0)
			{
				if (producerTemplate == 1)
				{
					flag = false;
					flag2 = true;
				}
				else
				{
					flag2 = false;
					flag = false;
				}
			}
			else
			{
				flag = true;
				flag2 = false;
			}
			_ProducerTemplate++;
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
						if (_RulesTemplate == null)
						{
							_IteratorTemplate();
						}
						else
						{
							_RulesTemplate();
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
					_IteratorTemplate();
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
					base.position = new Rect(base.position.x, base.position.y, _ErrorTemplate, taskTemplate);
				}
			}
			else
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				_ErrorTemplate = lastRect.width;
				taskTemplate = lastRect.height;
			}
		}
	}

	private void OnLostFocus()
	{
		Close();
	}

	internal static void AwakeAccount(Rect init, Action ord, Action res, float caller2 = 100f, float map3 = 100f, bool isresult4 = true, bool outputcont5 = true, bool isvisitor6 = true)
	{
		if (_TestsTemplate != null)
		{
			try
			{
				_TestsTemplate.Close();
			}
			catch
			{
				Debug.Log("Failed close");
				try
				{
					UnityEngine.Object.DestroyImmediate(_TestsTemplate);
				}
				catch
				{
					Debug.Log("Failed destroy??");
				}
			}
			_TestsTemplate = null;
		}
		_TestsTemplate = ScriptableObject.CreateInstance<InvocationErrorStatus>();
		_TestsTemplate.VisitAccount(ord, res, caller2, map3, isresult4, outputcont5, isvisitor6);
		_TestsTemplate.ShowUtility();
		_TestsTemplate.position = init;
	}

	internal static bool CallFactory()
	{
		return (object)PostFactory == null;
	}
}
