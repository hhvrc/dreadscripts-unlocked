// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested class Contents         -> Contents,          lines 584-665
//   static ManageStatus           -> IconContent,       line 3328
//   static CustomizeRef           -> the contents accessor, line 3337
//   static field factorySerializer -> contentsInstance,  line 2076
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// 2019 vs 2022: the two builds are identical here apart from obfuscated member names. No
// behavioural divergence, so nothing needed choosing between them.
//
// The shipped Contents had a constructor that did nothing but assign six tooltips to contents
// built moments earlier in the field initialisers (pickable, notPickable, settings,
// copyFromComponent, select, edit). Those tooltips are passed inline here instead: the content is
// freshly constructed and unread in between, so the result is the same object state without the
// second pass.
//
// PARTIAL PORT. The seven CachedIcon entries are left out; see the note at their position below.
//
// Overlap with ControllerEditor's EditorUtils.Contents (documented, deliberately NOT shared —
// consolidating the two products' tables is a separate decision):
//   identical icon name + tooltip: upToDate, inspectorWindow, selectFolder, removeSelection,
//     create, lockOff, lockOn, customTool
//   identical icon name, tooltip added here: edit, settings (CE: settingsGear),
//     select (CE: selectable), copyFromComponent (CE: eyeDropper), pickable, notPickable
//   same icon, different tooltip: reset (CE: restoreDefaults, "Restore Defaults")
//   the pending CachedIcon entries overlap CE's cached icons too — noted inline.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        private static Contents contentsInstance;

        /// <summary>
        /// The shared icon table, built on first use. Not built eagerly: every entry calls
        /// <see cref="EditorGUIUtility.IconContent(string)"/>, which is only valid once the editor
        /// skin has loaded.
        /// </summary>
        internal static Contents contents => contentsInstance ?? (contentsInstance = new Contents());

        /// <summary>
        /// Every icon and labelled content the tool draws. Field names come from each entry's label
        /// or tooltip where it has one, and from its built-in icon name otherwise.
        /// </summary>
        internal class Contents
        {
            // ── Session-cached icons ────────────────────────────────────────────────────────
            // NOT YET PORTED. These seven were `CachedIcon` fields built by the decompiled
            // `NewVal(icon, sessionKey, tooltip)` helper, which trims the transparent border off a
            // built-in icon and keeps the result in SessionState. Restoring them needs two pieces
            // of ADOverhaul infrastructure that has not been ported yet: the `CachedIcon` type
            // (decompiled lines 1148-1262) and the border-trimming helper `DefineVal`. They are
            // listed here so the restoration is mechanical once those land — the first, third,
            // fourth, fifth, sixth and seventh are byte-for-byte the same entries as
            // ControllerEditor's contents.updateAvailable / announcement / warning / error /
            // hamburgerMenu / help:
            //
            //   updateAvailable  NewVal("CollabConflict Icon",      "ds-icon-updateAvailable", "Update Available")
            //   checkForUpdate   NewVal("Refresh",                  "ds-icon-checkForUpdate",  "Check For Update")
            //   announcement     NewVal("console.infoicon.sml",     "ds-icon-announcement")
            //   warning          NewVal("console.warnicon.sml",     "ds-icon-warning")
            //   error            NewVal("console.erroricon.sml",    "ds-icon-error")
            //   hamburgerMenu    NewVal("VerticalLayoutGroup Icon", "ds-icon-hamMenu")
            //   help             NewVal("_Help",                    "ds-icon-help")

            // ── Toolbar and window chrome ───────────────────────────────────────────────────
            internal readonly GUIContent upToDate = IconContent("TestPassed", "Up to Date!");
            internal readonly GUIContent inspectorWindow = IconContent("UnityEditor.InspectorWindow");
            internal readonly GUIContent reset = IconContent("Refresh", "Reset");
            internal readonly GUIContent selectFolder = IconContent("FolderOpened Icon", "Select a folder");
            internal readonly GUIContent settings = IconContent("settings", "Open ADO Settings");
            internal readonly GUIContent removeSelection = IconContent("Toolbar Minus", "Remove selection from list");
            internal readonly GUIContent create = IconContent("CollabCreate Icon");
            internal readonly GUIContent lockOff = IconContent("IN LockButton");
            internal readonly GUIContent lockOn = IconContent("IN LockButton on");
            internal readonly GUIContent customTool = IconContent("d_CustomTool@2x");
            internal readonly GUIContent clear = new GUIContent("X", "Clear");

            // ── Scene view tools ────────────────────────────────────────────────────────────
            internal readonly GUIContent edit = IconContent("editicon.sml", "Edit through the scene view");
            internal readonly GUIContent select = IconContent("Selectable Icon", "Select through the scene view");
            internal readonly GUIContent copyFromComponent = IconContent("eyeDropper.Large", "Copy from another component of the same type");

            /// <summary>Scene picking is on: clicks in the scene view reach other objects.</summary>
            internal readonly GUIContent pickable = IconContent("d_scenepicking_pickable_hover@2x", "Scene view clicks are allowed while editing.");

            /// <summary>Scene picking is off, so a stray click cannot change the selection mid-edit.</summary>
            internal readonly GUIContent notPickable = IconContent("d_scenepicking_notpickable@2x", "Scene view clicks are ignored while editing.");

            // ── Settings window fields ──────────────────────────────────────────────────────
            internal readonly GUIContent handleSize = new GUIContent("Handle Size", "The size multiplier of the custom ADO gizmos");
            internal readonly GUIContent animatedFoldouts = new GUIContent("Animated Foldouts", "Enable animated foldouts in the editor");
            internal readonly GUIContent showNameLabels = new GUIContent("Show Name Labels", "Show names of transforms when toggling or selecting");
            internal readonly GUIContent labelColor = new GUIContent("Label Color", "The color of text displayed in the scene view");
            internal readonly GUIContent generalColor = new GUIContent("General Color", "The color of the handles used for editing");
            internal readonly GUIContent activeColor = new GUIContent("Active Color", "The color of handles that are selected");
            internal readonly GUIContent inactiveColor = new GUIContent("Inactive Color", "The color of handles that are not selected");
            internal readonly GUIContent mixedColor = new GUIContent("Mixed Color", "The color of handles that are active in some of the currently selected PhysBones but not others");
            internal readonly GUIContent selectionColor = new GUIContent("Selection Color", "The color of handles when selecting");
            internal readonly GUIContent function = new GUIContent("Function", "What you'd like to set up on the avatar");
            internal readonly GUIContent propertyAndTipOverlay = new GUIContent("Property & Tip Overlay", "Displays the overlay for tooltips and property selection on the scene view");
            internal readonly GUIContent tooltips = new GUIContent("Tooltips", "Displays tooltips on how to use the current tool");
        }

        /// <summary>Copies a built-in editor icon and gives it a tooltip.</summary>
        /// <remarks>
        /// The copy matters: <see cref="EditorGUIUtility.IconContent(string)"/> hands back a shared
        /// instance, so setting the tooltip on it directly would change that icon everywhere in the
        /// editor.
        /// </remarks>
        internal static GUIContent IconContent(string iconName, string tooltip = null)
        {
            return new GUIContent(EditorGUIUtility.IconContent(iconName)) { tooltip = tooltip };
        }
    }
}
