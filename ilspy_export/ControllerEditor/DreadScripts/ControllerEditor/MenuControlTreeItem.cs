using UnityEditor.IMGUI.Controls;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class MenuControlTreeItem : TreeViewItem
{
	internal readonly VRCExpressionsMenu.Control classThread;

	internal MenuControlTreeItem(VRCExpressionsMenu.Control param)
	{
		classThread = param;
	}
}
