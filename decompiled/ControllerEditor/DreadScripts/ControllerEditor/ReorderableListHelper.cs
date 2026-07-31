using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class ReorderableListHelper<T>
{
	internal readonly IList list;

	private readonly ReorderableList reorderableList;

	private object lastSelected;

	private readonly ReorderableList.ElementCallbackDelegate drawElement;

	private readonly Action drawHeader;

	internal Action onSelectionChanged;

	internal bool expanded = true;

	internal bool drawWhenEmpty;

	internal static object CustomizeStruct;

	[SpecialName]
	internal int Index()
	{
		return reorderableList.index = ClampIndex(reorderableList.index);
	}

	[SpecialName]
	internal void Index(int value_length)
	{
		reorderableList.index = ClampIndex(value_length);
	}

	internal ReorderableListHelper(Action last, IList caller, Action<ReorderableList> proc, ReorderableList.ElementCallbackDelegate attr2, ReorderableList.ElementHeightCallbackDelegate init3 = null)
	{
		drawHeader = last;
		list = caller;
		drawElement = attr2;
		reorderableList = new ReorderableList(caller, typeof(T), draggable: true, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
		{
			headerHeight = 1f,
			footerHeight = 0f,
			drawElementCallback = DrawElement,
			onAddCallback = proc.Invoke
		};
		if (init3 != null)
		{
			reorderableList.elementHeightCallback = init3;
		}
	}

	internal ReorderableListHelper(string info, string pol, IList pool, Action<ReorderableList> item2, ReorderableList.ElementCallbackDelegate ivk3, ReorderableList.ElementHeightCallbackDelegate token4 = null)
		: this((Action)null, pool, item2, ivk3, token4)
	{
		DreadScripts.ControllerEditor.ReorderableListHelper<T> reorderableListHelper = this;
		drawHeader = delegate
		{
			reorderableListHelper.DrawTitle(info, pol);
			reorderableListHelper.DrawHeaderButtons();
		};
	}

	private void DrawElement(Rect param, int pol_low, bool allowcomp, bool skipfirst2)
	{
		if (list.Count != 0 && pol_low >= 0 && pol_low < list.Count)
		{
			if (!GUI.Button(new Rect(param.x + param.width - 28f, param.y + param.height / 2f - 8f, 32f, 18f), EditorUtils.DestroyError().m_DecoratorProcessor, EditorUtils.CalcError()._TemplateProcessor))
			{
				Rect rect = new Rect(param);
				rect.width = param.width - 29f;
				Rect rect2 = rect;
				drawElement(rect2, pol_low, allowcomp, skipfirst2);
			}
			else
			{
				list.RemoveAt(pol_low);
			}
		}
	}

	internal void Draw()
	{
		bool flag = list.Count == 0;
		if (onSelectionChanged != null)
		{
			object obj = ((!flag) ? list[Index()] : null);
			if (obj != lastSelected)
			{
				lastSelected = obj;
				onSelectionChanged();
			}
		}
		if (drawHeader != null)
		{
			using (new EditorGUILayout.HorizontalScope("RL Header"))
			{
				drawHeader();
			}
		}
		if (expanded && (!flag || drawWhenEmpty))
		{
			reorderableList.DoLayoutList();
		}
	}

	internal int ClampIndex(int var1_X)
	{
		return Mathf.Clamp(var1_X, 0, list.Count - 1);
	}

	internal void DrawTitle(string def, string vis = null)
	{
		GUILayout.Label(def, EditorStyles.boldLabel);
		if (!string.IsNullOrEmpty(vis))
		{
			GUILayout.Label(new GUIContent(EditorUtils.DestroyError()._AccountProcessor.texture(), vis), GUILayout.Width(14f), GUILayout.Height(18f));
		}
	}

	internal void DrawHeaderButtons(bool rejectv = true, bool writeattr = true)
	{
		if (!writeattr)
		{
			if (rejectv)
			{
				expanded = EditorUtils.ExcludeQueue(expanded, (!expanded) ? EditorUtils.DestroyError().m_StatusProcessor : EditorUtils.DestroyError()._RefProcessor, EditorStyles.label, GUILayout.Width(18f), GUILayout.Height(18f));
			}
			using (new EditorGUI.DisabledScope(!expanded))
			{
				if (EditorUtils.RestartQueue(EditorGUIUtility.IconContent("d_ol_plus"), GUI.skin.label, GUILayout.Width(18f)))
				{
					reorderableList.onAddCallback(reorderableList);
				}
				return;
			}
		}
		while (true)
		{
			GUILayout.FlexibleSpace();
		}
	}

	internal static bool SearchStruct()
	{
		return CustomizeStruct == null;
	}
}
