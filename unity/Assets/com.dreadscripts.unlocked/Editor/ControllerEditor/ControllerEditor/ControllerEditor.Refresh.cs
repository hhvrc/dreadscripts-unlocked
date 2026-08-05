// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The refresh routines the Controller Editor runs when something *outside* the GUI changes what the
// window is showing: an undo that rewrote the transitions the condition editors were built from, and
// a settings edit that changes the Animator window's graph background. Neither is called from OnGUI;
// both are event handlers, which is why they are grouped here rather than with the drawing code.
//
// The two entries below deliberately carry no line number. That is not an oversight -- the numbers
// are 12980 and 15852 respectively, and the reason they are not written in the MAP column is set out
// under NOTES, "WHY THESE ENTRIES HAVE NO LINE NUMBER". The members this file is responsible for are:
//   UpdateVisitor -> NOT PORTED, see the NOT PORTED section
//   SortAlgo      -> ApplyGraphBackground
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// ======================================= NOT PORTED ===========================================
//
// UpdateVisitor (decompiled 12980) is not ported, and is not written as an empty method, because
// there is nothing of it left once its two statements are removed. Its entire body is:
//
//     sharedConditionEditors = AssetVisitor(selectedTransitions);
//     MapVisitor();
//
// and both calls are blocked:
//
//   AssetVisitor (12961) folds a list of transitions down to the conditions they all share, by
//   building the first transition's condition editors and then intersecting that list against each
//   further transition. It is nine lines and would be easy, except that it delegates to
//   CheckVisitor (12924), which decides whether two AnimatorConditions are "the same condition"
//   through WriteVisitor (12859) -- a 49-line field-by-field comparison that also reports which
//   fields differed, so the surviving editor can be marked mixed-value. WriteVisitor is unported.
//
//   MapVisitor (11763) rebuilds the three ReorderableLists that draw the condition editors
//   (sharedConditionList, allConditionList, focusedConditionList). Every list it builds names four
//   unported callbacks -- PrepareVisitor (12951), TestVisitor, CalculateVisitor and FillVisitor --
//   as its element, header and add handlers. It is also licence gated; that gate would be dropped
//   under this package's usual rule, but dropping it does not make the body portable.
//
// Porting UpdateVisitor therefore means porting AssetVisitor, PrepareVisitor, CheckVisitor,
// WriteVisitor and MapVisitor first: roughly 160 further lines of the god class, spanning the whole
// condition-editor subsystem. That is a port of its own and is deliberately not smuggled in here.
// The knock-on effect is that ControllerEditor.Window.cs's deferred Undo handler (PrintWrapper,
// decompiled 8836) stays deferred -- see the PARTIAL PORT list in that file's header, which names
// UpdateVisitor as its blocker. This file does not change that; it only records precisely why.
//
// The same chain is what stops EditorSettings.onMatchingOptionsChanged (the seam in
// EditorSettings.ChangeHooks.cs that the shipped code satisfies with UpdateVisitor) from being
// wired up. It stays null, and toggling "show matching options" persists correctly while simply not
// rebuilding the editors -- exactly the behaviour that file's header already promises.
//
// ======================================== DEOBF-BUG ===========================================
//
// SortAlgo's colour choice decompiles as a redundant double test. ILSpy renders it as
//
//     bool flag = !cosmeticGraphActive;
//     ...
//     connectionVisitor.SetPixel(0, 0, (!flag && (bool)cosmeticGraphActive)
//         ? gridBackgroundColor.GetValue()
//         : (Color)gridBackgroundColor.defaultValue);
//
// where `!flag` is by construction `cosmeticGraphActive`, so the conjunction re-tests the same
// setting against itself and can only ever equal `!flag`. This is the shape the obfuscator's
// boolean-flattening leaves behind throughout this assembly. The coherent form -- one test on the
// cosmetics master switch -- is what is written below; the behaviour is identical.
//
// ========================================== NOTES =============================================
//
// WHY THESE ENTRIES HAVE NO LINE NUMBER. Decompiled lines 12980 and 15852 are already claimed, with
// line numbers, by Editor/ControllerEditor/EditorSettings/EditorSettings.ChangeHooks.cs, which maps
// them onto the two assignable seams onMatchingOptionsChanged and onGraphBackgroundChanged. Those
// seams exist precisely because these two methods were unported when the settings class landed. Per
// HEADER-FORMAT.md every (decompiled file, line) pair must be claimed exactly once, so writing the
// numbers here would make tools/check-headers.py report both lines as claimed by two files. The
// numbers are stated in prose above instead, and the correct fix is a two-line edit to
// ChangeHooks.cs -- demoting its 12980 and 15852 entries to sub-entries under the introducer it
// already has, since a seam is not a port of the member that used to fill it -- after which this
// header should take the numbers back. That edit is out of scope for this file and is reported to
// the caller rather than made.
//
// WIRING. In the shipped build SortAlgo is not subscribed to anything: it is passed directly as the
// change callback of four settings (graphBackgroundIsTexture, cosmeticGraphActive,
// gridBackgroundColor and graphBackgroundTexture, decompiled 1247-1359) and is additionally called
// outright at 15918 and inside a SettingsChangeScope at 3525. In this package those four settings
// instead raise EditorSettings.onGraphBackgroundChanged, so making ApplyGraphBackground live is one
// assignment to that field. This file does not make it, because the assignment belongs in the
// window's OnEnable -- ControllerEditor.Window.cs, which this port is not permitted to edit. Until
// it is added, editing a graph-background setting persists the value and does not repaint the
// Animator window's background; nothing is broken by the omission, it simply does not take effect
// until the next domain reload primes the style some other way.
//
// THE GUI.skin GUARD. ApplyGraphBackground opens with a try/catch around a null test on GUI.skin
// that then returns either way. That looks pointless and is not: outside a GUI callback Unity's
// GUI.skin getter both can return null and, on some versions and on the serialization thread, can
// throw outright. This method runs from a settings setter, which can fire during deserialization or
// from an inspector on a background-ish callback, so it has to survive both outcomes. It constructs
// a GUIStyle and touches GUIStyle.normal, neither of which is legal without a live skin.
//
// Audit status: PARTIAL -- ApplyGraphBackground was transcribed statement by statement from
// decompiled 15852-15887 and the field names were taken from ControllerEditor.State.cs's rename
// table rather than re-derived. The blocker chain recorded under NOT PORTED was walked in the
// decompiled source (12980 -> 12961 -> 12924 -> 12859, and 12980 -> 11763) and each named line
// lands on the member claimed; the bodies of those blockers were read only far enough to establish
// that they are unported, not audited in full.

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Graph background

        /// <summary>
        /// Rewrites the Animator window's graph background to whatever the cosmetic settings
        /// currently say it should be.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The background is not a hook, a preference or anything else the tool could ask Unity to
        /// re-evaluate: it is a single <c>GUIStyle</c> held in a public static field,
        /// <c>UnityEditor.Graphs.Styles.graphBackground</c>, which the graph reads directly every
        /// time it draws. The only way to change it is to build a replacement style and assign it
        /// over the top by reflection, which is what this method does. That also explains why it has
        /// to be re-run on every settings change rather than merely triggering a repaint -- a
        /// repaint would just redraw the old style.
        /// </para>
        /// <para>
        /// Restoring the stock look is done by writing Unity's own default colour back, not by
        /// putting the original style object back: the original is overwritten on the first call and
        /// is never recovered. That is why turning the cosmetic switch off still goes through the
        /// whole assignment, using <c>gridBackgroundColor</c>'s default value instead of its current
        /// one.
        /// </para>
        /// <para>
        /// The 1x1 texture is allocated once into <see cref="graphBackgroundTexture"/> and then
        /// recoloured in place. Reallocating it would leak a texture per settings change, and this
        /// runs on every drag of the colour picker.
        /// </para>
        /// <para>
        /// See the file header for the guard this opens with, for the redundant double test the
        /// decompiler left in the colour choice, and for the subscription that makes this method
        /// fire, which is not made here.
        /// </para>
        /// </remarks>
        private static void ApplyGraphBackground()
        {
            // Both a null skin and a throwing skin mean "there is no GUI context right now", and
            // both mean the same thing to us: come back later. See the file header.
            try
            {
                if (GUI.skin == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            if (graphBackgroundStyleField == null)
            {
                graphBackgroundStyleField = EditorUtils
                    .RequireQualifiedType("UnityEditor.Graphs.Styles, UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
                    .GetField("graphBackground", BindingFlags.Static | BindingFlags.Public);
            }

            // The master switch. When the cosmetic graph is off the method still runs to completion,
            // writing Unity's stock colour back over whatever was there.
            bool cosmeticsOff = !EditorSettings.Instance.cosmeticGraphActive;

            GUIStyle style = new GUIStyle();
            Texture2D background;

            if (cosmeticsOff || !EditorSettings.Instance.graphBackgroundIsTexture)
            {
                if (graphBackgroundTexture == null)
                {
                    graphBackgroundTexture = new Texture2D(1, 1);
                }

                graphBackgroundTexture.SetPixel(0, 0, cosmeticsOff
                    ? (Color)EditorSettings.Instance.gridBackgroundColor.defaultValue
                    : EditorSettings.Instance.gridBackgroundColor.value);
                graphBackgroundTexture.Apply();
                background = graphBackgroundTexture;
            }
            else
            {
                background = EditorSettings.Instance.graphBackgroundTexture.GetValue<Texture2D>();
            }

            style.normal.background = background;
            graphBackgroundStyleField.SetValue(null, style);
        }

        #endregion
    }
}
