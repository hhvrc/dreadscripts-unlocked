using UnityEditor.IMGUI.Controls;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class ContainerThread : TreeViewItem
{
	internal readonly VRCExpressionsMenu.Control classThread;

	private static ContainerThread PrintStatus;

	internal ContainerThread(VRCExpressionsMenu.Control param)
	{
		classThread = param;
	}

	internal static bool ResolveStatus()
	{
		return PrintStatus == null;
	}
}
