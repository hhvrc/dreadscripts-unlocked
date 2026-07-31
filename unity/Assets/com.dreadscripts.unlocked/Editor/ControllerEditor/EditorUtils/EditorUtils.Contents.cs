// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   nested class WatcherProcessor -> Contents, lines 29-235
//   static PushQueue -> IconContent, line 6227
//   static NewList   -> CachedIcon,  line 7381
//   static DestroyError -> the contents accessor, line 6236
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every field below was matched to its icon/tooltip pair.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        private static Contents contentsInstance;

        /// <summary>
        /// The shared icon table, built on first use. Not built eagerly: every entry calls
        /// <see cref="EditorGUIUtility.IconContent(string)"/>, which is only valid once the editor
        /// skin has loaded.
        /// </summary>
        internal static Contents contents => contentsInstance ?? (contentsInstance = new Contents());

        /// <summary>
        /// Every icon the tool draws. Field names come from each entry's tooltip where it has one,
        /// and from its built-in icon name otherwise.
        /// </summary>
        internal sealed class Contents
        {
            // ── Mode toggles ────────────────────────────────────────────────────────────────
            internal readonly GUIContent simpleMode = IconContent("Prefab Icon", "Simple Mode");
            internal readonly GUIContent advancedMode = IconContent("GameObject Icon", "Advanced Mode");

            // ── Clip handling ───────────────────────────────────────────────────────────────
            internal readonly GUIContent defaultMergeClip = IconContent("NetworkStartPosition Icon", "Default Merge Clip");
            internal readonly GUIContent defaultReplaceClip = IconContent("CompositeCollider2D Icon", "Default Replace Clip");
            internal readonly GUIContent keyframeSemiSelected = EditorGUIUtility.IconContent("curvekeyframesemiselectedoverlay");
            internal readonly GUIContent keyframeWeighted = EditorGUIUtility.IconContent("curvekeyframeweighted");
            internal readonly GUIContent linear = IconContent("EditCollider", "Linear");
            internal readonly GUIContent loopingClip = IconContent("preAudioLoopOff", "Looping Clip");

            // ── Clipboard and defaults ──────────────────────────────────────────────────────
            internal readonly GUIContent copy = IconContent("SaveActive", "Copy");
            internal readonly GUIContent paste = IconContent("Clipboard", "Paste");
            internal readonly GUIContent restoreDefaults = IconContent("Refresh", "Restore Defaults");

            // ── Animator graph element types ────────────────────────────────────────────────
            internal readonly GUIContent animatorStates = IconContent("AnimatorState Icon", "Animator States");
            internal readonly GUIContent stateMachines = IconContent("AnimatorStateMachine Icon", "StateMachines");
            internal readonly GUIContent stateMachineBehaviours = IconContent("dll Script Icon", "StateMachine Behaviors");
            internal readonly GUIContent transitions = IconContent("AnimatorStateTransition Icon", "Transitions");
            internal readonly GUIContent blendTrees = IconContent("BlendTree Icon", "BlendTrees");
            internal readonly GUIContent animationClip = IconContent("Animation.Play", "Animation Clip");

            /// <summary>
            /// The six entries above, in the order the element-type filter toolbar draws them.
            /// Built in the constructor because a field initialiser cannot read sibling fields.
            /// </summary>
            internal readonly GUIContent[] animatorElementTypes;

            // ── Layer and transition editing ────────────────────────────────────────────────
            internal readonly GUIContent switchLayer = IconContent("UnityEditor.VersionControl", "Switch");
            internal readonly GUIContent separate = IconContent("BlendTree Icon", "Separate");
            internal readonly GUIContent merge = IconContent("AnimatorStateTransition Icon", "Merge");
            internal readonly GUIContent shared = IconContent("Animation.Record", "Shared?");
            internal readonly GUIContent removeCondition = IconContent("Toolbar Minus", "Remove Condition");
            internal readonly GUIContent removeElement = IconContent("Toolbar Minus", "Remove element from list");
            internal readonly GUIContent removeSelection = IconContent("Toolbar Minus", "Remove selection from list");
            internal readonly GUIContent deselect = IconContent("winbtn_win_close", "Deselect");
            internal readonly GUIContent settings = IconContent("_Popup", "Settings");
            internal readonly GUIContent invalidPattern = IconContent("CollabError", "Invalid Pattern");

            // ── Bare images ─────────────────────────────────────────────────────────────────
            internal readonly Texture bodySilhouette = EditorGUIUtility.IconContent("BodySilhouette").image;
            internal readonly Texture trash = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
            internal readonly GUIContent folderOpened = EditorGUIUtility.IconContent("FolderOpened Icon");

            // ── Session-cached icons ────────────────────────────────────────────────────────
            // These are trimmed copies of built-in icons, so they are kept in SessionState rather
            // than re-trimmed after every domain reload. See CachedIcon.
            internal readonly CachedTextureContent updateAvailable = CachedIcon("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");
            internal readonly CachedTextureContent reset = CachedIcon("Refresh", "ds-icon-refresh", "Reset");
            internal readonly CachedTextureContent announcement = CachedIcon("console.infoicon.sml", "ds-icon-announcement");
            internal readonly CachedTextureContent warning = CachedIcon("console.warnicon.sml", "ds-icon-warning");
            internal readonly CachedTextureContent note = CachedIcon("console.warnicon.inactive.sml", "ds-icon-note");
            internal readonly CachedTextureContent error = CachedIcon("console.erroricon.sml", "ds-icon-error");
            internal readonly CachedTextureContent hamburgerMenu = CachedIcon("VerticalLayoutGroup Icon", "ds-icon-hamMenu");
            internal readonly CachedTextureContent ping = CachedIcon("Lightmapping", "ds-icon-light", "Ping");
            internal readonly CachedTextureContent help = CachedIcon("_Help", "ds-icon-help");
            internal readonly CachedTextureContent visible = CachedIcon("scenevis_visible_hover", "ds-icon-visible", "Visible");
            internal readonly CachedTextureContent hidden = CachedIcon("scenevis_hidden_hover", "ds-icon-hidden", "Hidden");
            internal readonly CachedTextureContent mirror = CachedIcon("Mirror", "ds-icon-mirror", "Mirror");

            // ── Toolbar and window chrome ───────────────────────────────────────────────────
            internal readonly GUIContent upToDate = IconContent("TestPassed", "Up to Date!");
            internal readonly GUIContent inspectorWindow = IconContent("UnityEditor.InspectorWindow");
            internal readonly GUIContent selectFolder = IconContent("FolderOpened Icon", "Select a folder");
            internal readonly GUIContent edit = IconContent("editicon.sml");
            internal readonly GUIContent settingsGear = IconContent("settings");
            internal readonly GUIContent selectable = IconContent("Selectable Icon");
            internal readonly GUIContent eyeDropper = IconContent("eyeDropper.Large");
            internal readonly GUIContent create = IconContent("CollabCreate Icon");
            internal readonly GUIContent addNew = IconContent("d_CreateAddNew@2x");
            internal readonly GUIContent lockOff = IconContent("IN LockButton");
            internal readonly GUIContent lockOn = IconContent("IN LockButton on");
            internal readonly GUIContent pickable = IconContent("d_scenepicking_pickable_hover@2x");
            internal readonly GUIContent notPickable = IconContent("d_scenepicking_notpickable@2x");
            internal readonly GUIContent customTool = IconContent("d_CustomTool@2x");
            internal readonly GUIContent close = IconContent("winbtn_win_close");
            internal readonly GUIContent search = IconContent("Search Icon");
            internal readonly GUIContent blendTree = IconContent("BlendTree Icon");
            internal readonly GUIContent findDependencies = IconContent("UnityEditor.FindDependencies");
            internal readonly GUIContent sceneHierarchy = IconContent("UnityEditor.SceneHierarchyWindow");
            internal readonly GUIContent folderAdded = IconContent("Collab.FolderAdded");
            internal readonly GUIContent sort = IconContent("AlphabeticalSorting", "Sort");

            internal Contents()
            {
                animatorElementTypes = new[]
                {
                    stateMachines, animatorStates, transitions,
                    blendTrees, stateMachineBehaviours, animationClip
                };
            }
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

        /// <summary>
        /// A built-in icon trimmed of its transparent border and cached in the session under
        /// <paramref name="sessionKey"/>.
        /// </summary>
        /// <remarks>
        /// Unity's built-in icons are padded to a fixed cell size. Drawing one in a tight toolbar
        /// slot leaves it looking too small, so the padding is trimmed once and the result kept.
        /// </remarks>
        internal static CachedTextureContent CachedIcon(string iconName, string sessionKey, string tooltip = "")
        {
            CachedTextureContent cached = new CachedTextureContent(sessionKey, tooltip);
            if (cached.texture != null)
            {
                return cached;
            }

            GUIContent builtin = EditorGUIUtility.IconContent(iconName);
            if (builtin?.image != null)
            {
                cached.texture = TrimTransparentBorder(builtin.image as Texture2D);
            }

            return cached;
        }
    }
}
