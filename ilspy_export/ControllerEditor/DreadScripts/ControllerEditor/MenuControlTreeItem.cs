using UnityEditor.IMGUI.Controls;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class MenuControlTreeItem : TreeViewItem
{
	internal readonly VRCExpressionsMenu.Control classThread;

	private static MenuControlTreeItem PrintStatus;

	internal MenuControlTreeItem(VRCExpressionsMenu.Control param)
	{
		classThread = param;
	}

	internal static bool ResolveStatus()
	{
		return PrintStatus == null;
	}
}
