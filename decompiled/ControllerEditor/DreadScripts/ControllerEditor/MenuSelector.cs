using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class MenuSelector : EditorWindow
{
	private MenuExpressionTreeView treeView;

	private bool showAssetPicker = true;

	private int controlsToAdd = 1;

	private Action<VRCExpressionsMenu> onMenuSelected;

	internal static HashSet<VRCExpressionsMenu> lastRootMenuStack = new HashSet<VRCExpressionsMenu>();

	internal static VRCExpressionsMenu lastRootMenu;

	[SpecialName]
	private VRCExpressionsMenu rootMenu()
	{
		return treeView.rootMenu;
	}

	[SpecialName]
	private VRCExpressionsMenu[] menuStack()
	{
		return treeView.menuStack;
	}

	internal static void Open(VRCExpressionsMenu reference, Action<VRCExpressionsMenu> caller, int column_c = 1, bool istoken2 = true)
	{
		MenuSelector window = EditorWindow.GetWindow<MenuSelector>("Menu Selector");
		VRCExpressionsMenu asset = ((lastRootMenu == null) ? reference : ((!lastRootMenuStack.Contains(reference)) ? reference : lastRootMenu));
		window.treeView = new MenuExpressionTreeView(asset);
		window.treeView.onMenuSelected = window.TrySelect;
		window.onMenuSelected = caller;
		window.controlsToAdd = column_c;
		window.showAssetPicker = istoken2;
	}

	private void TrySelect(VRCExpressionsMenu info)
	{
		if (!(info == null) && (bool)info.QueryError(controlsToAdd))
		{
			onMenuSelected?.Invoke(info);
			Close();
		}
	}

	private void OnGUI()
	{
		if (treeView == null)
		{
			Close();
			return;
		}
		Rect rect = new Rect(0f, 0f, base.position.width, base.position.height - 20f);
		Rect rect2 = new Rect(0f, base.position.height - 30f, base.position.width, 30f);
		Rect rect3 = new Rect(0f, base.position.height - 21f, base.position.width, 1f);
		Rect def = new Rect(rect2);
		GUIContent content = new GUIContent("Show All Controls", "Shows all controls, including those that are not submenus.");
		def.x += 4f;
		def.y += def.height / 2f - 9f;
		def.height = 18f;
		Rect rect4 = def.SliceLeft(120f, isfield: true);
		Rect rect5 = def.SliceRight(80f, isserv: true, 4f, isvisitor3: true);
		Rect spec = def.SliceRight(20f, isserv: true, 4f, isvisitor3: true);
		treeView.OnGUI(rect);
		EditorGUI.DrawRect(rect3, new Color(0.2f, 0.2f, 0.2f));
		EditorGUI.DrawRect(rect2, new Color(0.25f, 0.25f, 0.25f));
		EditorGUI.BeginChangeCheck();
		bool showAllControls = GUI.Toggle(rect4, MenuExpressionTreeView.showAllControls, content);
		if (EditorGUI.EndChangeCheck())
		{
			MenuExpressionTreeView.showAllControls = showAllControls;
			treeView.Reload();
		}
		using (new EditorGUI.DisabledScope(!treeView.HasSelection() || treeView.GetSelection().All((int ident) => !treeView.controlMap.ContainsKey(ident) || !treeView.controlMap[ident].RestartError(1))))
		{
			if (GUI.Button(rect5, "Select"))
			{
				int key = treeView.GetSelection().FirstOrDefault((int last_max) => treeView.controlMap.ContainsKey(last_max) && (bool)treeView.controlMap[last_max].RestartError(1));
				VRCExpressionsMenu.Control control = treeView.controlMap[key];
				TrySelect(control.subMenu);
			}
		}
		if (!showAssetPicker)
		{
			return;
		}
		EditorUtils.AddLinkCursor(spec);
		if (GUI.Button(spec, EditorUtils.contents().selectFolder, EditorUtils.styles().iconButton))
		{
			EditorUtils.ShowObjectPicker(null, typeof(VRCExpressionsMenu), null, null, loaddef3: false, null, delegate(UnityEngine.Object first)
			{
				TrySelect(first as VRCExpressionsMenu);
			});
		}
	}

	[CompilerGenerated]
	private bool ExcludeRecord(int ident)
	{
		if (treeView.controlMap.ContainsKey(ident))
		{
			return !treeView.controlMap[ident].RestartError(1);
		}
		return true;
	}

	[CompilerGenerated]
	private bool InitRecord(int last_max)
	{
		if (!treeView.controlMap.ContainsKey(last_max))
		{
			return false;
		}
		return treeView.controlMap[last_max].RestartError(1);
	}

	[CompilerGenerated]
	private void VisitRecord(UnityEngine.Object first)
	{
		TrySelect(first as VRCExpressionsMenu);
	}
}
