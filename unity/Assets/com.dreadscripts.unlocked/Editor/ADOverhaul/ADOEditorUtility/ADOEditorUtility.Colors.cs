// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static _ObserverSerializer    -> validColor,           line 2062
//   static _BroadcasterSerializer -> errorColor,           line 2064
//   static _EventSerializer       -> warningColor,         line 2066
//   static m_RecordSerializer     -> secondaryActionColor, line 2068
//   static resolverSerializer     -> highlightColor,       line 2070
//   static _TagSerializer         -> cautionColor,         line 2072
//   static _FilterSerializer      -> accentColor,          line 2074
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- all seven literals and their declaration order were
// re-checked against lines 2062-2074 on 2026-08-04 and match. Accessibility matches too (plain
// `internal static Color`, not readonly, as shipped).
//
// 2019 vs 2022: the same seven colours in the same order with the same literals (2019 lines
// 2064-2076, under different obfuscated names). No behavioural divergence.
//
// The names come from the role each colour's call sites give it, not from its hue -- the same basis
// ControllerEditor's EditorUtils.Colors.cs used. Call sites, all in
// decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs:
//   validColor           the "true" half of every conditional GUIColorScope pair (lines 2537, 3307,
//                        4255, 4568, 5782, 6126, 6728, 6742, 6901, 4604, 6160), the enabled "Apply
//                        Changes" button, and the Regular tint of the log colouriser (line 7812).
//   errorColor           the "false" half of those same pairs, the destructive "Stop Testing"
//                        button (lines 4590, 6146, 6876), the Error tint of the log colouriser, and
//                        the prefix colour of thrown exception messages (line 7838).
//   warningColor         the Warning tint of the log colouriser, the "false" half of the pairs at
//                        lines 2537 and 3307, and the Handles colour at line 3675.
//   secondaryActionColor the "Restart" button in all three test-mode toolbars (lines 4597, 6153,
//                        6892), between the red stop and the green apply.
//   highlightColor       the "on" background of the Inside/Outside Bounds toggle at line 5782.
//   cautionColor and accentColor have no surviving call site in either assembly; see their notes.
//
// Shared with ControllerEditor: EditorUtils declares the same literals
// (decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs lines 2176-2190), and
// all seven of these appear there. The two palettes are effectively the same table -- ControllerEditor
// carries one extra entry the ADOverhaul build does not (a pale blue, 0.5/0.8/1, at line 2176) --
// which makes them a candidate for a shared palette in Editor/Common. Deliberately NOT consolidated
// here: that is a cross-product decision, and this file must not reach into the other product's.

using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>Tint for a value or state that is in order, and for ordinary log output.</summary>
        internal static Color validColor = new Color(0.56f, 0.94f, 0.47f);

        /// <summary>
        /// Tint for a failed operation and for an action that undoes or interrupts work.
        /// </summary>
        internal static Color errorColor = new Color(1f, 0.25f, 0.25f);

        /// <summary>Tint for a state that is off or unset, and for warning log output.</summary>
        internal static Color warningColor = new Color(0.99f, 0.95f, 0f);

        /// <summary>
        /// Tint for an action that is neither destructive nor confirming -- in practice the
        /// "Restart" button that sits between the two in every test-mode toolbar.
        /// </summary>
        internal static Color secondaryActionColor = new Color(0.3f, 0.7f, 1f);

        /// <summary>Emphasis tint, used to mark a toggle as active.</summary>
        internal static Color highlightColor = new Color(0.7f, 0.3f, 1f);

        /// <summary>
        /// Reserved status tint, sitting between <see cref="warningColor"/> and
        /// <see cref="errorColor"/> in the ramp.
        /// </summary>
        /// <remarks>
        /// Nothing in either shipped assembly reads this, so the name is inferred from its place in
        /// the palette rather than from a call site. Kept because the field is <c>internal</c> and
        /// user scripts in the same assembly could have referenced it.
        /// </remarks>
        internal static Color cautionColor = new Color(1f, 0.65f, 0f);

        /// <summary>The suite's accent colour.</summary>
        /// <remarks>
        /// Also unread in this assembly. Its ControllerEditor twin -- the one palette entry the
        /// obfuscator left named, as <c>accentColor</c> -- tints hover highlights on clickable
        /// labels, so the name is taken from there.
        /// </remarks>
        internal static Color accentColor = new Color(1f, 0.5f, 0.7f);
    }
}
