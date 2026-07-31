// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   nested class BaseProcessor -> Styles, lines 237-515
//   static CalcError -> the styles accessor, line 6242
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- every style was named from the properties it sets.

using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        private static Styles stylesInstance;

        /// <summary>
        /// The shared style table, built on first use. Not built eagerly: nearly every entry derives
        /// from <see cref="GUI.skin"/> or <see cref="EditorStyles"/>, which are only valid once the
        /// editor skin has loaded.
        /// </summary>
        internal static Styles styles => stylesInstance ?? (stylesInstance = new Styles());

        /// <summary>
        /// Every GUIStyle the tool draws with. Names describe what each one looks like rather than
        /// where it is used, since most are used in several places.
        /// </summary>
        internal class Styles
        {
            /// <summary>Grey used for secondary text on the light skin, where <see cref="Color.gray"/> is too pale.</summary>
            internal static readonly Color lightSkinNoteColor = new Color(0.357f, 0.357f, 0.357f);

            // ── Labels ──────────────────────────────────────────────────────────────────────
            internal readonly GUIStyle centeredMiniLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            internal readonly GUIStyle linkLabel = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.cyan },
                hover = { textColor = Color.cyan }
            };

            internal readonly GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };

            internal readonly GUIStyle subtitle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };

            internal readonly GUIStyle boldLabel = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };

            internal readonly GUIStyle centeredTitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };

            internal readonly GUIStyle richLabel = new GUIStyle(GUI.skin.label) { richText = true };

            internal readonly GUIStyle centeredRichLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            internal readonly GUIStyle centeredBoldRichLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                richText = true
            };

            internal readonly GUIStyle wrappedRichLabel = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                wordWrap = true
            };

            internal readonly GUIStyle wrappedBoldRichLabel = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                richText = true,
                wordWrap = true
            };

            internal readonly GUIStyle tightLabel = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(),
                margin = new RectOffset(1, 1, 1, 1)
            };

            // ── Buttons ─────────────────────────────────────────────────────────────────────
            internal readonly GUIStyle compactButton = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 2, 2)
            };

            internal readonly GUIStyle largeButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fixedHeight = 30f
            };

            internal readonly GUIStyle hugeButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 40f,
                fontStyle = FontStyle.Bold
            };

            /// <summary>An 18x18 slot for an icon drawn as a button.</summary>
            internal readonly GUIStyle iconButton = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedWidth = 18f,
                fixedHeight = 18f
            };

            internal readonly GUIStyle dropDownButton = new GUIStyle(GUI.skin.GetStyle("DropDownButton"))
            {
                alignment = TextAnchor.MiddleLeft,
                contentOffset = new Vector2(2.5f, 0f),
                fixedHeight = 0f
            };

            internal readonly GUIStyle footerButton = GUI.skin.GetStyle("RL FooterButton");

            /// <summary>
            /// A label carrying the built-in "Toggle" style name, which is what makes IMGUI give it
            /// hover feedback — the trick used to make a plain label read as clickable.
            /// </summary>
            internal readonly GUIStyle toggleLabel = new GUIStyle(GUI.skin.label) { name = "Toggle" };

            // ── Containers ──────────────────────────────────────────────────────────────────
            internal readonly GUIStyle centeredIcon = new GUIStyle
            {
                margin = new RectOffset(4, 4, 4, 4),
                alignment = TextAnchor.MiddleCenter
            };

            internal readonly GUIStyle paddedBox = new GUIStyle
            {
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset()
            };

            internal readonly GUIStyle richTextArea = new GUIStyle(GUI.skin.textArea) { richText = true };

            internal readonly GUIStyle assetLabel = "AssetLabel";

            internal readonly GUIStyle bigTitleBackground = "in bigtitle";

            // ── Small italic annotations ────────────────────────────────────────────────────
            // Same style at three alignments; the light skin needs a darker grey than Color.gray.
            internal readonly GUIStyle noteLeft = Note(TextAnchor.MiddleLeft);
            internal readonly GUIStyle noteCenter = Note(TextAnchor.MiddleCenter);
            internal readonly GUIStyle noteRight = Note(TextAnchor.MiddleRight);

            internal readonly GUIStyle noteLeftTight = new GUIStyle(Note(TextAnchor.MiddleLeft))
            {
                contentOffset = new Vector2(-3f, 1.5f)
            };

            internal readonly GUIStyle miniNote = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                contentOffset = new Vector2(-3f, 3f)
            };

            /// <summary>An annotation that is also a link — hover turns it blue.</summary>
            internal readonly GUIStyle linkNote = new GUIStyle(Note(TextAnchor.MiddleLeft))
            {
                name = "Toggle",
                hover = { textColor = new Color(0.3f, 0.7f, 1f) }
            };

            /// <summary>A left-aligned label that highlights in the accent colour on hover.</summary>
            internal readonly GUIStyle accentLinkLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                name = "Toggle",
                hover = { textColor = accentColor }
            };

            /// <summary>Right-aligned note nudged to sit flush against the control before it.</summary>
            internal readonly GUIStyle inlineNoteRight;

            /// <summary>Left-aligned note nudged to sit flush against the control before it.</summary>
            internal readonly GUIStyle inlineNoteLeft;

            // ── Contents that live with the styles ──────────────────────────────────────────
            internal readonly GUIContent remove = IconContent("TreeEditor.Trash", "Remove");
            internal readonly GUIContent behaviours = new GUIContent("B", "Behaviours");
            internal readonly GUIContent writeDefaults = new GUIContent("WD", "Write Defaults");

            /// <summary>A square the height of one standard control row, for icon buttons.</summary>
            internal readonly GUILayoutOption[] iconButtonSize =
            {
                GUILayout.Width(EditorGUIUtility.singleLineHeight),
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            };

            private MethodInfo textFieldDropDown;

            /// <summary>
            /// <c>EditorGUILayout.TextFieldDropDown</c>, which Unity does not expose publicly. Picked
            /// by parameter count because the name is overloaded.
            /// </summary>
            internal MethodInfo TextFieldDropDown =>
                textFieldDropDown ?? (textFieldDropDown = typeof(EditorGUILayout)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .First(m => m.Name == "TextFieldDropDown" && m.GetParameters().Length == 3));

            internal Styles()
            {
                // These two derive from another entry in this table, so they cannot be field
                // initialisers — field initialisers run before any sibling is assigned.
                inlineNoteRight = new GUIStyle(noteRight)
                {
                    contentOffset = new Vector2(-2.5f, 0f),
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.gray : Grey(91f) }
                };

                inlineNoteLeft = new GUIStyle(inlineNoteRight)
                {
                    alignment = TextAnchor.MiddleLeft,
                    contentOffset = new Vector2(2.5f, 0f),
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.gray : Grey(91f) }
                };
            }

            /// <summary>The shared shape of the small italic annotation styles.</summary>
            private static GUIStyle Note(TextAnchor alignment)
            {
                return new GUIStyle(GUI.skin.label)
                {
                    alignment = alignment,
                    fontStyle = FontStyle.Italic,
                    richText = true,
                    fontSize = 11,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.gray : lightSkinNoteColor }
                };
            }
        }
    }
}
