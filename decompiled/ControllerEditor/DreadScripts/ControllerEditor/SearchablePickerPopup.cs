using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class SearchablePickerPopup<T> : PopupWindowContent
{
	internal class PickerEntry
	{
		internal readonly int index;

		internal readonly T value;

		internal object[] extraData;

		internal bool isVisible;

		private static object MapSystem;

		[SpecialName]
		internal object FirstExtra()
		{
			return this.m_MerchantServer[0];
		}

		internal PickerEntry(T spec, int cust_size)
		{
			this.authenticationServer = true;
			base._002Ector();
			this._ValueServer = spec;
			this.m_ValServer = cust_size;
		}

		internal static bool AddSystem()
		{
			return MapSystem == null;
		}
	}

	private readonly string _TokenServer;

	private string m_CodeServer;

	internal PickerEntry[] m_DicServer;

	private readonly Action<PickerEntry> invocationServer;

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
			background = EditorUtils.ReflectList(new Color(0.302f, 0.302f, 0.302f))
		},
		active = 
		{
			background = EditorUtils.ReflectList(new Color(0.1725f, 0.3647f, 0.5294f))
		}
	};

	private static object WriteSystem;

	public SearchablePickerPopup(string param, IEnumerable<T> attr, Action<PickerEntry> third, Action<int, T> reference2)
	{
		_TokenServer = param;
		roleServer = reference2;
		invocationServer = third;
		m_DicServer = attr.Select((T item, int i) => new PickerEntry(item, i)).ToArray();
		_ExceptionServer = new Rect[m_DicServer.Length];
	}

	public void GetConnection(Func<T, string, bool> key)
	{
		_ModelServer = true;
		_ParamServer = key;
	}

	public void CalcConnection(Func<T, object> param)
	{
		m_DicServer = ((param == null) ? m_DicServer : m_DicServer.OrderBy((PickerEntry item) => param(item._ValueServer)).ToArray());
	}

	public void IncludeConnection(Func<T, object[]> ident)
	{
		PickerEntry[] dicServer = m_DicServer;
		foreach (PickerEntry pickerEntry in dicServer)
		{
			pickerEntry.m_MerchantServer = ident(pickerEntry._ValueServer);
		}
	}

	public override void OnGUI(Rect rect)
	{
		using (new GUILayout.AreaScope(rect))
		{
			Event current = Event.current;
			using (new ScrollViewScope(ref m_ComparatorServer))
			{
				if (!string.IsNullOrEmpty(_TokenServer))
				{
					GUILayout.Label(_TokenServer, EditorUtils.CalcError()._StructProcessor);
					EditorUtils.MapQueue();
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
						PickerEntry[] dicServer = m_DicServer;
						foreach (PickerEntry pickerEntry in dicServer)
						{
							pickerEntry.authenticationServer = _ParamServer(pickerEntry._ValueServer, m_CodeServer);
						}
					}
				}
				EventType type = current.type;
				for (int j = 0; j < m_DicServer.Length; j++)
				{
					PickerEntry pickerEntry2 = m_DicServer[j];
					if (!pickerEntry2.authenticationServer)
					{
						continue;
					}
					if (!_DecoratorServer && GUI.Button(_ExceptionServer[j], string.Empty, objectServer))
					{
						roleServer(pickerEntry2.m_ValServer, pickerEntry2._ValueServer);
						base.editorWindow.Close();
					}
					using (new GUILayout.VerticalScope())
					{
						invocationServer(pickerEntry2);
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
