// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested class Styles              -> Styles,            lines 667-835
//   static MapRef                    -> the styles accessor, line 3343
//   static field _AttributeSerializer -> stylesInstance,    line 2078
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export -- all 24 members, every property set on each style and
// the order of the three toggle-state colours were re-checked against lines 667-835 on 2026-08-04
// and match.
// One deliberate deviation, recorded here rather than left implicit: the backing field
// (_AttributeSerializer) is `internal` in the shipped build and `private` here. Nothing but the
// accessor reads it in either assembly.
//
// 2019 vs 2022: identical apart from obfuscated member names and the polarity the decompiler chose
// for the isProSkin ternaries (`isProSkin ? gray : note` in one build, `!isProSkin ? note : gray`
// in the other — the same expression). No behavioural divergence.
//
// The five italic annotation styles were five fully written-out initialisers in the shipped build,
// differing only in alignment and two extra properties. They are folded onto a shared Note()
// factory here, matching how ControllerEditor's table expresses the same five styles.
//
// The Color[3] toggle-state palette is ported; its three entries are the shared palette statics in
// ADOEditorUtility.Colors.cs rather than literals repeated here.
//
// Overlap with ControllerEditor's EditorUtils.Styles (documented, deliberately NOT shared —
// consolidating the two products' tables is a separate decision):
//   identical: lightSkinNoteColor, iconButtonSize, title, subtitle, boldLabel, iconButton,
//     centeredRichLabel, centeredBoldRichLabel, tightLabel, wrappedBoldRichLabel, toggleLabel,
//     richLabel, assetLabel, bigTitleBackground, noteLeft, noteCenter, noteRight, noteLeftTight,
//     linkNote
//   near-miss: hugeButton — CE's also sets fixedHeight = 40; this one does not.
//   near-miss: footerButton — CE resolves "RL FooterButton" through GUI.skin.GetStyle and shares
//     the skin's instance; this one wraps it in a new GUIStyle. Kept as the source had it, since a
//     copy is safe to mutate and a shared instance is not.
//   ADOverhaul only: compactIconButton, indentedHeaderLabel, and the pending toggle-state palette.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        private static Styles stylesInstance;

        /// <summary>
        /// The shared style table, built on first use. Not built eagerly: nearly every entry derives
        /// from <see cref="GUI.skin"/>, which is only valid once the editor skin has loaded.
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

            /// <summary>A square the height of one standard control row, for icon buttons.</summary>
            internal readonly GUILayoutOption[] iconButtonSize =
            {
                GUILayout.Width(EditorGUIUtility.singleLineHeight),
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            };

            // ── Labels ──────────────────────────────────────────────────────────────────────
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

            /// <summary>Section heading for the support and changelog panels, indented off the left edge.</summary>
            internal readonly GUIStyle indentedHeaderLabel = new GUIStyle(GUI.skin.label)
            {
                stretchWidth = true,
                fontSize = 15,
                richText = true,
                margin = new RectOffset(10, 0, 0, 0),
                fontStyle = FontStyle.Bold
            };

            /// <summary>
            /// A label carrying the built-in "Toggle" style name, which is what makes IMGUI give it
            /// hover feedback — the trick used to make a plain label read as clickable.
            /// </summary>
            internal readonly GUIStyle toggleLabel = new GUIStyle(GUI.skin.label) { name = "Toggle" };

            // ── Buttons ─────────────────────────────────────────────────────────────────────
            internal readonly GUIStyle hugeButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            /// <summary>An 18x18 slot for an icon drawn as a button.</summary>
            internal readonly GUIStyle iconButton = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedWidth = 18f,
                fixedHeight = 18f
            };

            /// <summary>A button trimmed down to sit inline in a toolbar row.</summary>
            internal readonly GUIStyle compactIconButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 2, 0),
                padding = new RectOffset(1, 1, 1, 1)
            };

            internal readonly GUIStyle footerButton = new GUIStyle("RL FooterButton");

            // ── Containers ──────────────────────────────────────────────────────────────────
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

            /// <summary>An annotation that is also a link — hover turns it blue.</summary>
            internal readonly GUIStyle linkNote = new GUIStyle(Note(TextAnchor.MiddleLeft))
            {
                name = "Toggle",
                hover = { textColor = new Color(0.3f, 0.7f, 1f) }
            };

            // ── Toggle-state palette ────────────────────────────────────────────────────────

            /// <summary>
            /// Button backgrounds for a tri-state toggle, indexed 0 = off, 1 = on, 2 = mixed.
            /// </summary>
            /// <remarks>
            /// Passed straight to the GUIColorScope overload that takes an index and a
            /// colour array, so callers reduce a serialized property to one of the three indices and
            /// let the scope pick. Mixed is yellow rather than a blend, so that a multi-selection
            /// disagreeing about a value reads as something needing attention.
            /// </remarks>
            internal readonly Color[] toggleStateColors = { errorColor, validColor, warningColor };

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
