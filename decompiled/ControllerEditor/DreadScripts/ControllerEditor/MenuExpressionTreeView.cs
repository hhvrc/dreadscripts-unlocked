using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class MenuExpressionTreeView : TreeView
{
	internal readonly Dictionary<int, VRCExpressionsMenu.Control> controlMap = new Dictionary<int, VRCExpressionsMenu.Control>();

	internal VRCExpressionsMenu[] menuStack;

	internal static bool showAllControls;

	internal VRCExpressionsMenu rootMenu;

	internal Action<VRCExpressionsMenu> onMenuSelected;

	internal MenuExpressionTreeView(VRCExpressionsMenu asset)
		: base(new TreeViewState())
	{
		SetRootMenu(asset);
	}

	protected override TreeViewItem BuildRoot()
	{
		controlMap.Clear();
		HashSet<VRCExpressionsMenu> hashSet = new HashSet<VRCExpressionsMenu>();
		MenuControlTreeItem menuControlTreeItem = new MenuControlTreeItem(null)
		{
			id = 0,
			depth = -1,
			displayName = "Root"
		};
		if (!(rootMenu == null))
		{
			VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control
			{
				type = VRCExpressionsMenu.Control.ControlType.SubMenu,
				subMenu = rootMenu,
				name = rootMenu.name
			};
			MenuControlTreeItem menuControlTreeItem2 = new MenuControlTreeItem(control)
			{
				id = 1
			};
			menuControlTreeItem.AddChild(menuControlTreeItem2);
			controlMap.Add(1, control);
			Stack<MenuControlTreeItem> stack = new Stack<MenuControlTreeItem>();
			stack.Push(menuControlTreeItem2);
			bool flag = true;
			int num = 2;
			while (stack.Count > 0)
			{
				MenuControlTreeItem menuControlTreeItem3 = stack.Pop();
				VRCExpressionsMenu vRCExpressionsMenu = (flag ? rootMenu : menuControlTreeItem3.control?.subMenu);
				flag = false;
				if (vRCExpressionsMenu == null)
				{
					continue;
				}
				hashSet.Add(vRCExpressionsMenu);
				foreach (VRCExpressionsMenu.Control control2 in vRCExpressionsMenu.controls)
				{
					if (control2 == null)
					{
						continue;
					}
					bool flag2 = control2.type == VRCExpressionsMenu.Control.ControlType.SubMenu;
					if (showAllControls || flag2)
					{
						MenuControlTreeItem menuControlTreeItem4 = new MenuControlTreeItem(control2)
						{
							id = num++
						};
						controlMap.Add(menuControlTreeItem4.id, control2);
						if (flag2 && control2.subMenu != null && !hashSet.Contains(control2.subMenu))
						{
							stack.Push(menuControlTreeItem4);
						}
						menuControlTreeItem3.AddChild(menuControlTreeItem4);
					}
				}
			}
			TreeView.SetupDepthsFromParentsAndChildren(menuControlTreeItem);
			menuStack = hashSet.ToArray();
			return menuControlTreeItem;
		}
		return menuControlTreeItem;
	}

	protected override void RowGUI(RowGUIArgs last)
	{
		float contentIndent = GetContentIndent(last.item);
		Rect rowRect = last.rowRect;
		if (last.row % 2 == 1)
		{
			EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.07f));
		}
		rowRect.x += contentIndent;
		rowRect.width -= contentIndent;
		VRCExpressionsMenu.Control control = ((MenuControlTreeItem)last.item).control;
		using (new EditorGUI.DisabledScope(control == null || control.type != VRCExpressionsMenu.Control.ControlType.SubMenu))
		{
			bool num = control == null;
			string text = ((!num) ? control.name : "[Missing]");
			string tooltip = ((!num) ? control.type.ToString() : "Null");
			bool disabled = false;
			int num2;
			int num3;
			if (!num)
			{
				num2 = ((control.type == VRCExpressionsMenu.Control.ControlType.SubMenu) ? 1 : 0);
				if (num2 != 0)
				{
					num3 = ((control.subMenu != null) ? 1 : 0);
					goto IL_00a4;
				}
			}
			else
			{
				num2 = 0;
			}
			num3 = 0;
			goto IL_00a4;
			IL_00a4:
			bool flag = (byte)num3 != 0;
			if (num2 != 0)
			{
				int num4 = (flag ? control.subMenu.controls.Count : 0);
				int num5 = (flag ? 8 : 0);
				text += $" ({num4}/{num5})";
				disabled = num4 >= 8;
			}
			GUIContent content = new GUIContent(text, tooltip);
			Vector2 vector = GUI.skin.label.CalcSize(content);
			Rect position = new Rect(rowRect.x, rowRect.y, vector.x, rowRect.height);
			Rect position2 = new Rect(rowRect.x + vector.x + 4f, rowRect.y, rowRect.height, rowRect.height);
			using (new EditorGUI.DisabledScope(disabled))
			{
				Texture2D texture2D = ((!flag) ? null : control.icon);
				if (texture2D != null)
				{
					GUI.DrawTexture(position2, texture2D, ScaleMode.ScaleToFit);
				}
				GUI.Label(position, content);
			}
			if (!flag)
			{
				return;
			}
			string text2 = control.subMenu.name;
			Vector2 vector2 = GUI.skin.label.CalcSize(new GUIContent("[" + text2 + "]"));
			int num6 = 0;
			while (true)
			{
				num6++;
				if (!(vector2.x + vector.x + rowRect.height >= rowRect.width))
				{
					break;
				}
				text2 = text2.Substring(0, Mathf.FloorToInt((float)text2.Length / 2f)) ?? "";
				vector2 = GUI.skin.label.CalcSize(new GUIContent("[" + text2 + "...]"));
				if (num6 >= 30 || text2.Length <= 1)
				{
					return;
				}
			}
			Rect rect = new Rect(rowRect);
			rect.width = vector2.x;
			Rect rect2 = rect;
			rect2.x = rowRect.x + rowRect.width - vector2.x;
			GUI.Label(rowRect, "[" + ((num6 > 1) ? (text2 + "...") : text2) + "]", EditorUtils.CalcError().algoObserver);
		}
	}

	protected override bool CanMultiSelect(TreeViewItem key)
	{
		return false;
	}

	protected override bool DoesItemMatchSearch(TreeViewItem spec, string caller)
	{
		if (string.IsNullOrEmpty(caller))
		{
			return true;
		}
		return ((MenuControlTreeItem)spec).control?.name.ToLower().Contains(caller.ToLower()) ?? false;
	}

	protected override bool CanChangeExpandedState(TreeViewItem config)
	{
		VRCExpressionsMenu.Control control = ((MenuControlTreeItem)config).control;
		if (control != null)
		{
			if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu || !(control.subMenu != null))
			{
				return base.CanChangeExpandedState(config);
			}
			return true;
		}
		return false;
	}

	protected override void DoubleClickedItem(int num_item)
	{
		if (controlMap.ContainsKey(num_item))
		{
			VRCExpressionsMenu.Control control = controlMap[num_item];
			if (control != null && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && control.subMenu != null)
			{
				onMenuSelected?.Invoke(control.subMenu);
			}
		}
	}

	protected override void ContextClickedItem(int paramcounter)
	{
		if (controlMap.TryGetValue(paramcounter, out var baseThread) && baseThread != null && baseThread.type == VRCExpressionsMenu.Control.ControlType.SubMenu && baseThread.subMenu != null)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Select"), on: false, delegate
			{
				onMenuSelected?.Invoke(baseThread.subMenu);
			});
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Set As Root"), on: false, delegate
			{
				SetRootMenu(baseThread.subMenu);
			});
			genericMenu.ShowAsContext();
		}
	}

	internal void SetRootMenu(VRCExpressionsMenu instance)
	{
		rootMenu = instance;
		Reload();
		MenuSelector.m_MerchantThread = instance;
		MenuSelector.valueThread = new HashSet<VRCExpressionsMenu>(menuStack);
	}
}
