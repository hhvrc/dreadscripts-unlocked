using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class StatusServer<T> : PopupWindowContent
{
	internal class UtilsServer
	{
		internal readonly int m_ValServer;

		internal readonly T _ValueServer;

		internal object[] m_MerchantServer;

		internal bool authenticationServer = true;

		private static object MapSystem;

		[SpecialName]
		internal object CloneConnection()
		{
			return m_MerchantServer[0];
		}

		internal UtilsServer(T spec, int cust_size)
		{
			_ValueServer = spec;
			m_ValServer = cust_size;
		}

		internal static bool AddSystem()
		{
			return MapSystem == null;
		}
	}

	private readonly string _TokenServer;

	private string m_CodeServer;

	internal UtilsServer[] m_DicServer;

	private readonly Action<UtilsServer> invocationServer;

	private readonly Action<int, T> roleServer;

	private Func<T, string, bool> _ParamServer;

	private bool _ModelServer;

	private float m_TokenizerServer;

	private bool _DecoratorServer = true;

	private Vector2 m_ComparatorServer;

	private readonly Rect[] _ExceptionServer;

	internal readonly GUIStyle objectServer = new GUIStyle
	{
		hover = 
		{
			background = ClassProperty.ReflectList(new Color(0.302f, 0.302f, 0.302f))
		},
		active = 
		{
			background = ClassProperty.ReflectList(new Color(0.1725f, 0.3647f, 0.5294f))
		}
	};

	private static object WriteSystem;

	public StatusServer(string param, IEnumerable<T> attr, Action<UtilsServer> third, Action<int, T> reference2)
	{
		_TokenServer = param;
		roleServer = reference2;
		invocationServer = third;
		m_DicServer = attr.Select((T item, int i) => new UtilsServer(item, i)).ToArray();
		_ExceptionServer = new Rect[m_DicServer.Length];
	}

	public void GetConnection(Func<T, string, bool> key)
	{
		_ModelServer = true;
		_ParamServer = key;
	}

	public void CalcConnection(Func<T, object> param)
	{
		m_DicServer = ((param == null) ? m_DicServer : m_DicServer.OrderBy((UtilsServer item) => param(item._ValueServer)).ToArray());
	}

	public void IncludeConnection(Func<T, object[]> ident)
	{
		UtilsServer[] dicServer = m_DicServer;
		foreach (UtilsServer utilsServer in dicServer)
		{
			utilsServer.m_MerchantServer = ident(utilsServer._ValueServer);
		}
	}

	public override void OnGUI(Rect rect)
	{
		using (new GUILayout.AreaScope(rect))
		{
			Event current = Event.current;
			using (new SpecificationThread(ref m_ComparatorServer))
			{
				if (!string.IsNullOrEmpty(_TokenServer))
				{
					GUILayout.Label(_TokenServer, ClassProperty.CalcError()._StructProcessor);
					ClassProperty.MapQueue();
				}
				if (_ModelServer)
				{
					EditorGUI.BeginChangeCheck();
					if (_DecoratorServer)
					{
						GUI.SetNextControlName(_TokenServer + "SearchBar");
					}
					m_CodeServer = EditorGUILayout.TextField(m_CodeServer, GUI.skin.GetStyle("SearchTextField"));
					if (EditorGUI.EndChangeCheck())
					{
						UtilsServer[] dicServer = m_DicServer;
						foreach (UtilsServer utilsServer in dicServer)
						{
							utilsServer.authenticationServer = _ParamServer(utilsServer._ValueServer, m_CodeServer);
						}
					}
				}
				EventType type = current.type;
				for (int j = 0; j < m_DicServer.Length; j++)
				{
					UtilsServer utilsServer2 = m_DicServer[j];
					if (!utilsServer2.authenticationServer)
					{
						continue;
					}
					if (!_DecoratorServer && GUI.Button(_ExceptionServer[j], string.Empty, objectServer))
					{
						roleServer(utilsServer2.m_ValServer, utilsServer2._ValueServer);
						base.editorWindow.Close();
					}
					using (new GUILayout.VerticalScope())
					{
						invocationServer(utilsServer2);
					}
					if (type == EventType.Repaint)
					{
						_ExceptionServer[j] = GUILayoutUtility.GetLastRect();
						if (_DecoratorServer && _ExceptionServer[j].width > m_TokenizerServer)
						{
							m_TokenizerServer = _ExceptionServer[j].width;
						}
					}
				}
				if (type == EventType.Repaint && _DecoratorServer)
				{
					_DecoratorServer = false;
					GUI.FocusControl(_TokenServer + "SearchBar");
				}
			}
			if (rect.Contains(current.mousePosition))
			{
				base.editorWindow.Repaint();
			}
		}
	}

	public override Vector2 GetWindowSize()
	{
		Vector2 windowSize = base.GetWindowSize();
		if (!_DecoratorServer)
		{
			windowSize.x = m_TokenizerServer + 21f;
		}
		return windowSize;
	}

	public void RunConnection(Rect item)
	{
		PopupWindow.Show(item, this);
	}

	internal static bool RemoveSystem()
	{
		return WriteSystem == null;
	}
}
