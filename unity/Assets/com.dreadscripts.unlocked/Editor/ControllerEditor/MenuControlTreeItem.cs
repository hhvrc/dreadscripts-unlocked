// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/MenuControlTreeItem.cs

using UnityEditor.IMGUI.Controls;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A <see cref="TreeViewItem"/> that carries the expressions-menu control it stands for, so the
    /// menu tree view can act on the control directly rather than mapping row ids back to it.
    /// </summary>
    internal class MenuControlTreeItem : TreeViewItem
    {
        internal readonly VRCExpressionsMenu.Control control;

        internal MenuControlTreeItem(VRCExpressionsMenu.Control control)
        {
            this.control = control;
        }
    }
}
