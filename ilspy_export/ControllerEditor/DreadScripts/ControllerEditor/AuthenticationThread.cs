using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class AuthenticationThread : TreeView
{
	internal readonly Dictionary<int, VRCExpressionsMenu.Control> m_ReponseThread = new Dictionary<int, VRCExpressionsMenu.Control>();

	internal VRCExpressionsMenu[] poolThread;

	internal static bool m_ParameterThread;

	internal VRCExpressionsMenu composerThread;

	internal Action<VRCExpressionsMenu> repositoryThread;

	internal static AuthenticationThread NewStatus;

	internal AuthenticationThread(VRCExpressionsMenu asset)
		: base(new TreeViewState())
	{
		AwakeRecord(asset);
	}

	protected override TreeViewItem BuildRoot()
	{
		m_ReponseThread.Clear();
		HashSet<VRCExpressionsMenu> hashSet = new HashSet<VRCExpressionsMenu>();
		ContainerThread containerThread = new ContainerThread(null)
		{
			id = 0,
			depth = -1,
			displayName = "Root"
		};
		if (!(composerThread == null))
		{
			VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control
			{
				type = VRCExpressionsMenu.Control.ControlType.SubMenu,
				subMenu = composerThread,
				name = composerThread.name
			};
			ContainerThread containerThread2 = new ContainerThread(control)
			{
				id = 1
			};
			containerThread.AddChild(containerThread2);
			m_ReponseThread.Add(1, control);
			Stack<ContainerThread> stack = new Stack<ContainerThread>();
			stack.Push(containerThread2);
			bool flag = true;
			int num = 2;
			while (stack.Count > 0)
			{
				ContainerThread containerThread3 = stack.Pop();
				VRCExpressionsMenu vRCExpressionsMenu = (flag ? composerThread : containerThread3.classThread?.subMenu);
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
					if (m_ParameterThread || flag2)
					{
						ContainerThread containerThread4 = new ContainerThread(control2)
						{
							id = num++
						};
						m_ReponseThread.Add(containerThread4.id, control2);
						if (flag2 && control2.subMenu != null && !hashSet.Contains(control2.subMenu))
						{
							stack.Push(containerThread4);
						}
						containerThread3.AddChild(containerThread4);
					}
				}
			}
			TreeView.SetupDepthsFromParentsAndChildren(containerThread);
			poolThread = hashSet.ToArray();
			return containerThread;
		}
		return containerThread;
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
		VRCExpressionsMenu.Control classThread = ((ContainerThread)last.item).classThread;
		using (new EditorGUI.DisabledScope(classThread == null || classThread.type != VRCExpressionsMenu.Control.ControlType.SubMenu))
		{
			bool num = classThread == null;
			string text = ((!num) ? classThread.name : "[Missing]");
			string tooltip = ((!num) ? classThread.type.ToString() : "Null");
			bool disabled = false;
			int num2;
			int num3;
			if (!num)
			{
				num2 = ((classThread.type == VRCExpressionsMenu.Control.ControlType.SubMenu) ? 1 : 0);
				if (num2 != 0)
				{
					num3 = ((classThread.subMenu != null) ? 1 : 0);
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
				int num4 = (flag ? classThread.subMenu.controls.Count : 0);
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
				Texture2D texture2D = ((!flag) ? null : classThread.icon);
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
			string text2 = classThread.subMenu.name;
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
			GUI.Label(rowRect, "[" + ((num6 > 1) ? (text2 + "...") : text2) + "]", ClassProperty.CalcError().algoObserver);
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
		return ((ContainerThread)spec).classThread?.name.ToLower().Contains(caller.ToLower()) ?? false;
	}

	protected override bool CanChangeExpandedState(TreeViewItem config)
	{
		VRCExpressionsMenu.Control classThread = ((ContainerThread)config).classThread;
		if (classThread != null)
		{
			if (classThread.type != VRCExpressionsMenu.Control.ControlType.SubMenu || !(classThread.subMenu != null))
			{
				return base.CanChangeExpandedState(config);
			}
			return true;
		}
		return false;
	}

	protected override void DoubleClickedItem(int num_item)
	{
		if (m_ReponseThread.ContainsKey(num_item))
		{
			VRCExpressionsMenu.Control control = m_ReponseThread[num_item];
			if (control != null && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && control.subMenu != null)
			{
				repositoryThread?.Invoke(control.subMenu);
			}
		}
	}

	protected override void ContextClickedItem(int paramcounter)
	{
		if (m_ReponseThread.TryGetValue(paramcounter, out var baseThread) && baseThread != null && baseThread.type == VRCExpressionsMenu.Control.ControlType.SubMenu && baseThread.subMenu != null)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Select"), on: false, delegate
			{
				repositoryThread?.Invoke(baseThread.subMenu);
			});
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Set As Root"), on: false, delegate
			{
				AwakeRecord(baseThread.subMenu);
			});
			genericMenu.ShowAsContext();
		}
	}

	internal void AwakeRecord(VRCExpressionsMenu instance)
	{
		composerThread = instance;
		while (true)
		{
			Reload();
		}
	}

	internal static bool LoginStatus()
	{
		return NewStatus == null;
	}
}
