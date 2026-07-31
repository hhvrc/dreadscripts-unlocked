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

	private readonly string title;

	private string searchString;

	internal PickerEntry[] entries;

	private readonly Action<PickerEntry> drawEntry;

	private readonly Action<int, T> onSelected;

	private Func<T, string, bool> searchFilter;

	private bool hasSearch;

	private float maxWidth;

	private bool isFirstFrame;

	private Vector2 scrollPosition;

	private readonly Rect[] entryRects;

	internal readonly GUIStyle entryStyle;

	private static object WriteSystem;

	public SearchablePickerPopup(string param, IEnumerable<T> attr, Action<PickerEntry> third, Action<int, T> reference2)
	{
		this._DecoratorServer = true;
		this.objectServer = new GUIStyle
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
		base._002Ector();
		this._TokenServer = param;
		this.roleServer = reference2;
		this.invocationServer = third;
		this.m_DicServer = attr.Select((T item, int i) => new PickerEntry(item, i)).ToArray();
		this._ExceptionServer = new Rect[this.m_DicServer.Length];
	}

	public void EnableSearch(Func<T, string, bool> key)
	{
		this._ModelServer = true;
		this._ParamServer = key;
	}

	public void SortBy(Func<T, object> param)
	{
		this.m_DicServer = ((param == null) ? this.m_DicServer : this.m_DicServer.OrderBy((PickerEntry item) => param(item._ValueServer)).ToArray());
	}

	public void SetExtraData(Func<T, object[]> ident)
	{
		PickerEntry[] dicServer = this.m_DicServer;
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
			using (new ScrollViewScope(ref this.m_ComparatorServer))
			{
				if (!string.IsNullOrEmpty(this._TokenServer))
				{
					GUILayout.Label(this._TokenServer, EditorUtils.CalcError()._StructProcessor);
					EditorUtils.MapQueue();
				}
				if (this._ModelServer)
				{
					EditorGUI.BeginChangeCheck();
					if (this._DecoratorServer)
					{
						GUI.SetNextControlName(this._TokenServer + "SearchBar");
					}
					this.m_CodeServer = EditorGUILayout.TextField(this.m_CodeServer, GUI.skin.GetStyle("SearchTextField"));
					if (EditorGUI.EndChangeCheck())
					{
						PickerEntry[] dicServer = this.m_DicServer;
						foreach (PickerEntry pickerEntry in dicServer)
						{
							pickerEntry.authenticationServer = this._ParamServer(pickerEntry._ValueServer, this.m_CodeServer);
						}
					}
				}
				EventType type = current.type;
				for (int j = 0; j < this.m_DicServer.Length; j++)
				{
					PickerEntry pickerEntry2 = this.m_DicServer[j];
					if (!pickerEntry2.authenticationServer)
					{
						continue;
					}
					if (!this._DecoratorServer && GUI.Button(this._ExceptionServer[j], string.Empty, this.objectServer))
					{
						this.roleServer(pickerEntry2.m_ValServer, pickerEntry2._ValueServer);
						base.editorWindow.Close();
					}
					using (new GUILayout.VerticalScope())
					{
						this.invocationServer(pickerEntry2);
					}
					if (type == EventType.Repaint)
					{
						this._ExceptionServer[j] = GUILayoutUtility.GetLastRect();
						if (this._DecoratorServer && this._ExceptionServer[j].width > this.m_TokenizerServer)
						{
							this.m_TokenizerServer = this._ExceptionServer[j].width;
						}
					}
				}
				if (type == EventType.Repaint && this._DecoratorServer)
				{
					this._DecoratorServer = false;
					GUI.FocusControl(this._TokenServer + "SearchBar");
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
		if (!this._DecoratorServer)
		{
			windowSize.x = this.m_TokenizerServer + 21f;
		}
		return windowSize;
	}

	public void Show(Rect item)
	{
		PopupWindow.Show(item, this);
	}

	internal static bool RemoveSystem()
	{
		return WriteSystem == null;
	}
}
