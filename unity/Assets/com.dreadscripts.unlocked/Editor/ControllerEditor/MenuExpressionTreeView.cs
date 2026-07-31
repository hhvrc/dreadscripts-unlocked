// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/MenuExpressionTreeView.cs
// Member names are unchanged from the decompiled source; only locals were renamed, since the
// decompiler had reduced them to num/flag artifacts. The goto-based ternary chain in RowGUI and the
// two rects it computes but never uses are decompiler noise and are not reproduced.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Shows a VRChat expressions menu and everything reachable from it as a tree, so the user can
    /// browse down through submenus and pick one.
    /// </summary>
    /// <remarks>
    /// Row ids are handed out by the build, not derived from the assets: id 0 is the invisible root
    /// the <see cref="TreeView"/> requires, id 1 is always the root menu itself, and ids from 2 up
    /// are assigned to controls in the order the build reaches them. <see cref="controlMap"/> is the
    /// only way back from an id to a control, which is why the click handlers look ids up there
    /// instead of trusting the item they were given.
    /// </remarks>
    internal class MenuExpressionTreeView : TreeView
    {
        /// <summary>Maps a row id to the control it was built from. Rebuilt by every reload.</summary>
        internal readonly Dictionary<int, VRCExpressionsMenu.Control> controlMap = new Dictionary<int, VRCExpressionsMenu.Control>();

        /// <summary>
        /// Every menu the last build descended into, root included. Doubles as the set of menus that
        /// belong to the tree currently on screen.
        /// </summary>
        internal VRCExpressionsMenu[] menuStack;

        /// <summary>
        /// Whether non-submenu controls are listed too. Static so the preference survives closing the
        /// window and applies to every tree at once.
        /// </summary>
        internal static bool showAllControls;

        internal VRCExpressionsMenu rootMenu;

        /// <summary>Raised when the user picks a submenu, by double click or context menu.</summary>
        internal Action<VRCExpressionsMenu> onMenuSelected;

        internal MenuExpressionTreeView(VRCExpressionsMenu asset)
            : base(new TreeViewState())
        {
            SetRootMenu(asset);
        }

        protected override TreeViewItem BuildRoot()
        {
            controlMap.Clear();
            HashSet<VRCExpressionsMenu> visitedMenus = new HashSet<VRCExpressionsMenu>();
            MenuControlTreeItem root = new MenuControlTreeItem(null)
            {
                id = 0,
                depth = -1,
                displayName = "Root"
            };

            if (rootMenu == null)
            {
                return root;
            }

            // The root menu is not reached through a control, so it gets a synthetic one. That keeps
            // every visible row backed by a control and lets the root be selected like any submenu.
            VRCExpressionsMenu.Control rootControl = new VRCExpressionsMenu.Control
            {
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = rootMenu,
                name = rootMenu.name
            };
            MenuControlTreeItem rootMenuItem = new MenuControlTreeItem(rootControl)
            {
                id = 1
            };
            root.AddChild(rootMenuItem);
            controlMap.Add(1, rootControl);

            Stack<MenuControlTreeItem> pending = new Stack<MenuControlTreeItem>();
            pending.Push(rootMenuItem);
            bool isRootIteration = true;
            int nextId = 2;
            while (pending.Count > 0)
            {
                MenuControlTreeItem parent = pending.Pop();

                // The first pop is the synthetic root control, whose subMenu is the root menu anyway;
                // the special case is how the decompiled source reads and is kept as-is.
                VRCExpressionsMenu menu = isRootIteration ? rootMenu : parent.control?.subMenu;
                isRootIteration = false;
                if (menu == null)
                {
                    continue;
                }

                visitedMenus.Add(menu);
                foreach (VRCExpressionsMenu.Control control in menu.controls)
                {
                    if (control == null)
                    {
                        continue;
                    }

                    bool isSubMenu = control.type == VRCExpressionsMenu.Control.ControlType.SubMenu;
                    if (!showAllControls && !isSubMenu)
                    {
                        continue;
                    }

                    MenuControlTreeItem item = new MenuControlTreeItem(control)
                    {
                        id = nextId++
                    };
                    controlMap.Add(item.id, control);

                    // Descending only into menus not seen yet stops a menu that links back to an
                    // ancestor — which the SDK allows — from building an infinite tree.
                    if (isSubMenu && control.subMenu != null && !visitedMenus.Contains(control.subMenu))
                    {
                        pending.Push(item);
                    }

                    parent.AddChild(item);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            menuStack = visitedMenus.ToArray();
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            float contentIndent = GetContentIndent(args.item);
            Rect rowRect = args.rowRect;
            if (args.row % 2 == 1)
            {
                EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.07f));
            }

            // Striping covers the full row; the content is inset past the foldout arrow.
            rowRect.x += contentIndent;
            rowRect.width -= contentIndent;

            VRCExpressionsMenu.Control control = ((MenuControlTreeItem)args.item).control;

            // Only submenus can be picked, so everything else is drawn greyed out.
            using (new EditorGUI.DisabledScope(control == null || control.type != VRCExpressionsMenu.Control.ControlType.SubMenu))
            {
                bool isMissing = control == null;
                string label = isMissing ? "[Missing]" : control.name;
                string tooltip = isMissing ? "Null" : control.type.ToString();
                bool isFull = false;

                bool isSubMenu = !isMissing && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu;
                bool hasSubMenu = isSubMenu && control.subMenu != null;

                if (isSubMenu)
                {
                    int controlCount = hasSubMenu ? control.subMenu.controls.Count : 0;
                    int capacity = hasSubMenu ? 8 : 0;
                    label += $" ({controlCount}/{capacity})";
                    isFull = controlCount >= 8;
                }

                GUIContent content = new GUIContent(label, tooltip);
                Vector2 labelSize = GUI.skin.label.CalcSize(content);
                Rect labelRect = new Rect(rowRect.x, rowRect.y, labelSize.x, rowRect.height);
                Rect iconRect = new Rect(rowRect.x + labelSize.x + 4f, rowRect.y, rowRect.height, rowRect.height);

                // A menu already at the 8 control limit still shows, but reads as unavailable.
                using (new EditorGUI.DisabledScope(isFull))
                {
                    Texture2D icon = hasSubMenu ? control.icon : null;
                    if (icon != null)
                    {
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }

                    GUI.Label(labelRect, content);
                }

                if (!hasSubMenu)
                {
                    return;
                }

                // The submenu asset's own name is noted on the right, since a control's name often
                // differs from it. It is halved repeatedly until it fits beside the label rather than
                // being allowed to overlap it, and dropped entirely if halving cannot make it fit.
                string assetName = control.subMenu.name;
                Vector2 noteSize = GUI.skin.label.CalcSize(new GUIContent("[" + assetName + "]"));
                int attempts = 0;
                while (true)
                {
                    attempts++;
                    if (noteSize.x + labelSize.x + rowRect.height < rowRect.width)
                    {
                        break;
                    }

                    assetName = assetName.Substring(0, Mathf.FloorToInt(assetName.Length / 2f));
                    noteSize = GUI.skin.label.CalcSize(new GUIContent("[" + assetName + "...]"));
                    if (attempts >= 30 || assetName.Length <= 1)
                    {
                        return;
                    }
                }

                GUI.Label(rowRect, "[" + (attempts > 1 ? assetName + "..." : assetName) + "]", EditorUtils.styles.noteRight);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return true;
            }

            return ((MenuControlTreeItem)item).control?.name.ToLower().Contains(search.ToLower()) ?? false;
        }

        /// <summary>
        /// Keeps rows that cannot lead anywhere from being expanded, so an empty foldout arrow never
        /// invites a click.
        /// </summary>
        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            VRCExpressionsMenu.Control control = ((MenuControlTreeItem)item).control;
            if (control == null)
            {
                return false;
            }

            if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && control.subMenu != null)
            {
                return true;
            }

            return base.CanChangeExpandedState(item);
        }

        protected override void DoubleClickedItem(int id)
        {
            if (!controlMap.ContainsKey(id))
            {
                return;
            }

            VRCExpressionsMenu.Control control = controlMap[id];
            if (control != null && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && control.subMenu != null)
            {
                onMenuSelected?.Invoke(control.subMenu);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            if (!controlMap.TryGetValue(id, out VRCExpressionsMenu.Control control) || control == null
                || control.type != VRCExpressionsMenu.Control.ControlType.SubMenu || control.subMenu == null)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select"), on: false, delegate
            {
                onMenuSelected?.Invoke(control.subMenu);
            });
            menu.AddSeparator(string.Empty);

            // Re-rooting lets the user work inside a deep submenu without the ancestors in the way.
            menu.AddItem(new GUIContent("Set As Root"), on: false, delegate
            {
                SetRootMenu(control.subMenu);
            });
            menu.ShowAsContext();
        }

        /// <summary>
        /// Rebuilds the tree from <paramref name="menu"/> and records it as the menu the selector
        /// should return to, along with everything reachable from it.
        /// </summary>
        /// <remarks>
        /// The two <see cref="MenuSelector"/> statics are written here rather than by the window so
        /// that re-rooting from the context menu updates them too.
        /// </remarks>
        internal void SetRootMenu(VRCExpressionsMenu menu)
        {
            rootMenu = menu;
            Reload();
            MenuSelector.lastRootMenu = menu;

            // Reload has just refreshed menuStack through BuildRoot -- except when menu is null, in
            // which case BuildRoot leaves it alone and this throws. Kept as the source has it.
            MenuSelector.lastRootMenuStack = new HashSet<VRCExpressionsMenu>(menuStack);
        }
    }
}
