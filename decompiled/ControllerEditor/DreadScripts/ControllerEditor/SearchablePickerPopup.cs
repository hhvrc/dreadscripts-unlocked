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

		internal bool isVisible = true;

		private static object MapSystem;

		[SpecialName]
		internal object FirstExtra()
		{
			return extraData[0];
		}

		internal PickerEntry(T spec, int cust_size)
		{
			value = spec;
			index = cust_size;
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

	private bool isFirstFrame = true;

	private Vector2 scrollPosition;

	private readonly Rect[] entryRects;

	internal readonly GUIStyle entryStyle = new GUIStyle
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
		title = param;
		onSelected = reference2;
		drawEntry = third;
		entries = attr.Select((T item, int i) => new PickerEntry(item, i)).ToArray();
		entryRects = new Rect[entries.Length];
	}

	public void EnableSearch(Func<T, string, bool> key)
	{
		hasSearch = true;
		searchFilter = key;
	}

	public void SortBy(Func<T, object> param)
	{
		entries = ((param == null) ? entries : entries.OrderBy((PickerEntry item) => param(item.value)).ToArray());
	}

	public void SetExtraData(Func<T, object[]> ident)
	{
		PickerEntry[] array = entries;
		foreach (PickerEntry pickerEntry in array)
		{
			pickerEntry.extraData = ident(pickerEntry.value);
		}
	}

	public override void OnGUI(Rect rect)
	{
		using (new GUILayout.AreaScope(rect))
		{
			Event current = Event.current;
			using (new ScrollViewScope(ref scrollPosition))
			{
				if (!string.IsNullOrEmpty(title))
				{
					GUILayout.Label(title, EditorUtils.CalcError()._StructProcessor);
					EditorUtils.MapQueue();
				}
				if (hasSearch)
				{
					EditorGUI.BeginChangeCheck();
					if (isFirstFrame)
					{
						GUI.SetNextControlName(title + "SearchBar");
					}
					searchString = EditorGUILayout.TextField(searchString, GUI.skin.GetStyle("SearchTextField"));
					if (EditorGUI.EndChangeCheck())
					{
						PickerEntry[] array = entries;
						foreach (PickerEntry pickerEntry in array)
						{
							pickerEntry.isVisible = searchFilter(pickerEntry.value, searchString);
						}
					}
				}
				EventType type = current.type;
				for (int j = 0; j < entries.Length; j++)
				{
					PickerEntry pickerEntry2 = entries[j];
					if (!pickerEntry2.isVisible)
					{
						continue;
					}
					if (!isFirstFrame && GUI.Button(entryRects[j], string.Empty, entryStyle))
					{
						onSelected(pickerEntry2.index, pickerEntry2.value);
						base.editorWindow.Close();
					}
					using (new GUILayout.VerticalScope())
					{
						drawEntry(pickerEntry2);
					}
					if (type == EventType.Repaint)
					{
						entryRects[j] = GUILayoutUtility.GetLastRect();
						if (isFirstFrame && entryRects[j].width > maxWidth)
						{
							maxWidth = entryRects[j].width;
						}
					}
				}
				if (type == EventType.Repaint && isFirstFrame)
				{
					isFirstFrame = false;
					GUI.FocusControl(title + "SearchBar");
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
		if (!isFirstFrame)
		{
			windowSize.x = maxWidth + 21f;
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
