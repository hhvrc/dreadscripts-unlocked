// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/MenuSelector.cs
//   _ExceptionThread -> treeView,                line 13
//   objectThread     -> showAssetPicker,         line 15
//   utilsThread      -> controlsToAdd,           line 17
//   m_ValThread      -> onMenuSelected,          line 19
//   valueThread      -> lastRootMenuStack,       line 21
//   m_MerchantThread -> lastRootMenu,            line 23
//   DefineRecord     -> rootMenu,                line 26
//   ReadRecord       -> menuStack,               line 32
//   InvokeRecord     -> Open,                    line 37
//   FindRecord       -> TrySelect,               line 48
//   OnGUI            -> unchanged,               line 57
//   ExcludeRecord    -> inlined lambda in OnGUI, line 109
//   InitRecord       -> inlined lambda in OnGUI, line 119
//   VisitRecord      -> inlined lambda in OnGUI, line 129
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names are
// the durable reference.
//
// The last three are [CompilerGenerated] lambda bodies that the decompiler lifted out of OnGUI;
// they are ported as the inline lambdas they came from, not as methods.
//
// The four EditorUtils helpers this file calls are all in the package -- SliceLeft and SliceRight in
// EditorUtils.Rects.cs, ShowObjectPicker in EditorUtils.Pickers.cs, and the two
// ValidateCanAddControls overloads taking a control count in EditorUtils.Validation.cs; those
// headers own the mapping back to their decompiled names. Every call below was re-checked against
// the declared signatures, so the named arguments here sit on the parameters the decompiled call
// sites named: absolute on isfield (SortResolver) and isserv (PatchResolver), offsetAbsolute on
// isvisitor3, allowSceneObjects on loaddef3. ShowObjectPicker's seventh parameter is the
// selector-closed callback, which is where the decompiled call passes its delegate positionally.
// Audit status: VERIFIED against reverse-engineering/export/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A picker window for VRChat expression menus: browses a menu tree and hands the chosen submenu
    /// back to whoever opened it.
    /// </summary>
    /// <remarks>
    /// Only menus with room for the caller's controls can be chosen, so the window is also the place
    /// the 8-control limit is enforced before anything is written.
    /// </remarks>
    internal class MenuSelector : EditorWindow
    {
        private MenuExpressionTreeView treeView;

        /// <summary>Whether the footer offers an asset picker as an alternative to the tree.</summary>
        private bool showAssetPicker = true;

        /// <summary>
        /// How many controls the caller intends to add to the chosen menu. A menu without room for
        /// that many cannot be picked.
        /// </summary>
        private int controlsToAdd = 1;

        private Action<VRCExpressionsMenu> onMenuSelected;

        /// <summary>
        /// The menus reachable from <see cref="lastRootMenu"/> the last time a tree was built.
        /// </summary>
        /// <remarks>
        /// Kept so that reopening the window on a menu the user was already browsing can restore the
        /// old root instead of collapsing the view down to that one submenu. Static because the tree
        /// is rebuilt from scratch on every <see cref="Open"/>.
        /// </remarks>
        internal static HashSet<VRCExpressionsMenu> lastRootMenuStack = new HashSet<VRCExpressionsMenu>();

        /// <summary>The root the tree was last built from. See <see cref="lastRootMenuStack"/>.</summary>
        internal static VRCExpressionsMenu lastRootMenu;

        private VRCExpressionsMenu rootMenu => treeView.rootMenu;

        private VRCExpressionsMenu[] menuStack => treeView.menuStack;

        /// <summary>
        /// Opens the selector on <paramref name="menu"/> and calls <paramref name="onSelected"/> once
        /// the user picks a menu with room for <paramref name="controlsToAdd"/> more controls.
        /// </summary>
        /// <param name="showAssetPicker">
        /// Whether the user may also reach for a menu asset that is not in the tree at all.
        /// </param>
        /// <remarks>
        /// If <paramref name="menu"/> is somewhere inside the tree the user was browsing before, the
        /// window reopens at that earlier root, keeping the surrounding menus visible.
        /// </remarks>
        internal static void Open(VRCExpressionsMenu menu, Action<VRCExpressionsMenu> onSelected, int controlsToAdd = 1, bool showAssetPicker = true)
        {
            MenuSelector window = GetWindow<MenuSelector>("Menu Selector");
            VRCExpressionsMenu asset = lastRootMenu == null
                ? menu
                : (lastRootMenuStack.Contains(menu) ? lastRootMenu : menu);

            window.treeView = new MenuExpressionTreeView(asset);
            window.treeView.onMenuSelected = window.TrySelect;
            window.onMenuSelected = onSelected;
            window.controlsToAdd = controlsToAdd;
            window.showAssetPicker = showAssetPicker;
        }

        /// <summary>
        /// Accepts <paramref name="menu"/> and closes, unless it cannot take the caller's controls:
        /// in which case the pick is ignored and the window stays open.
        /// </summary>
        private void TrySelect(VRCExpressionsMenu menu)
        {
            if (menu == null || !(bool)menu.ValidateCanAddControls(controlsToAdd))
            {
                return;
            }

            onMenuSelected?.Invoke(menu);
            Close();
        }

        private void OnGUI()
        {
            // The tree is the window's whole state; without it there is nothing to show. This happens
            // after a domain reload, which clears the non-serialized field but leaves the window.
            if (treeView == null)
            {
                Close();
                return;
            }

            Rect treeRect = new Rect(0f, 0f, position.width, position.height - 20f);
            Rect footerRect = new Rect(0f, position.height - 30f, position.width, 30f);
            Rect separatorRect = new Rect(0f, position.height - 21f, position.width, 1f);

            Rect footerContent = new Rect(footerRect);
            GUIContent showAllContent = new GUIContent("Show All Controls", "Shows all controls, including those that are not submenus.");
            footerContent.x += 4f;
            footerContent.y += footerContent.height / 2f - 9f;
            footerContent.height = 18f;

            // Slicing consumes from footerContent as it goes, so the order fixes the layout: the
            // toggle takes the left, then the Select button the far right, then the asset picker the
            // space just left of it.
            Rect showAllRect = footerContent.SliceLeft(120f, absolute: true);
            Rect selectRect = footerContent.SliceRight(80f, absolute: true, 4f, offsetAbsolute: true);
            Rect pickerRect = footerContent.SliceRight(20f, absolute: true, 4f, offsetAbsolute: true);

            treeView.OnGUI(treeRect);
            EditorGUI.DrawRect(separatorRect, new Color(0.2f, 0.2f, 0.2f));
            EditorGUI.DrawRect(footerRect, new Color(0.25f, 0.25f, 0.25f));

            EditorGUI.BeginChangeCheck();
            bool showAllControls = GUI.Toggle(showAllRect, MenuExpressionTreeView.showAllControls, showAllContent);
            if (EditorGUI.EndChangeCheck())
            {
                MenuExpressionTreeView.showAllControls = showAllControls;
                treeView.Reload();
            }

            // Select stays disabled unless the selection actually points at a submenu with room, so
            // the lookup inside the button body can never miss.
            using (new EditorGUI.DisabledScope(!treeView.HasSelection() || treeView.GetSelection().All(id =>
                !treeView.controlMap.ContainsKey(id) || !treeView.controlMap[id].ValidateCanAddControls(1))))
            {
                if (GUI.Button(selectRect, "Select"))
                {
                    int id = treeView.GetSelection().FirstOrDefault(candidate =>
                        treeView.controlMap.ContainsKey(candidate) && (bool)treeView.controlMap[candidate].ValidateCanAddControls(1));
                    VRCExpressionsMenu.Control control = treeView.controlMap[id];
                    TrySelect(control.subMenu);
                }
            }

            if (!showAssetPicker)
            {
                return;
            }

            EditorUtils.AddLinkCursor(pickerRect);
            if (GUI.Button(pickerRect, EditorUtils.contents.selectFolder, EditorUtils.styles.iconButton))
            {
                EditorUtils.ShowObjectPicker(null, typeof(VRCExpressionsMenu), null, null, allowSceneObjects: false, null, delegate(UnityEngine.Object picked)
                {
                    TrySelect(picked as VRCExpressionsMenu);
                });
            }
        }
    }
}
