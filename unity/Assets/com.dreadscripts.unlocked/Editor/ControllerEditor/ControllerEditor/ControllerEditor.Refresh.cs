// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// The refresh routine the Controller Editor runs when something *outside* the GUI changes what the
// window is showing: a settings edit, or a play-mode transition, that invalidates the Animator
// window's graph background. It is not called from OnGUI; it is an event handler, which is why it is
// here rather than with the drawing code.
//
// The entry below deliberately carries no line number. That is not an oversight -- the number is
// 15852, and the reason it is not written in the MAP column is set out under NOTES, "WHY THIS ENTRY
// HAS NO LINE NUMBER". The member this file is responsible for is:
//   SortAlgo -> ApplyGraphBackground
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// ======================================= NOTES ================================================
//
// THIS FILE USED TO OWN A SECOND MEMBER AND A LONG NOT PORTED SECTION. Both are gone, and what they
// said is now wrong in every particular, so the note is kept short rather than deleted silently:
//
//   UpdateVisitor (decompiled 12980) was recorded here as unported, with its two statements --
//   `sharedConditionEditors = AssetVisitor(selectedTransitions); MapVisitor();` -- and a chain of
//   five blockers behind them (AssetVisitor 12961, PrepareVisitor 12951, CheckVisitor 12924,
//   WriteVisitor 12859, MapVisitor 11763), described as "roughly 160 further lines of the god class,
//   spanning the whole condition-editor subsystem".
//
//   That subsystem has since landed, in six files: ControllerEditor.ConditionMatching.cs (which is
//   where UpdateVisitor itself is now ported, as RefreshSharedConditions, and where AssetVisitor,
//   CheckVisitor, WriteVisitor and PrepareVisitor live as BuildSharedConditionEditors,
//   IntersectConditionEditors, ConditionsMatch and BuildConditionEditors), .ConditionList.cs (where
//   MapVisitor is RebuildConditionList), .ConditionListHeader.cs, .ConditionRow.cs,
//   .ConditionClipboard.cs and .ConditionMergeSplit.cs. Nothing in the old chain is outstanding.
//
//   The two knock-on claims that section made are both stale as well. ControllerEditor.Window.cs's
//   Undo handler (PrintWrapper, 8836) named UpdateVisitor as its blocker and is now ported there as
//   OnUndoRedo. EditorSettings.ChangeHooks.cs's `onMatchingOptionsChanged` seam, which that section
//   said "stays null", is assigned to ControllerEditor.RefreshSharedConditions in that file today.
//
// The MAP entry is not reassigned to ControllerEditor.ConditionMatching.cs from here, because that
// file already claims the member itself, in the same no-line-number form and for the same reason.
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
// ========================================== NOTES, CONTINUED ==================================
//
// WHY THIS ENTRY HAS NO LINE NUMBER. Decompiled line 15852 is already claimed, with a line number,
// by Editor/ControllerEditor/EditorSettings/EditorSettings.ChangeHooks.cs, which maps it onto the
// assignable seam onGraphBackgroundChanged. That seam exists precisely because this method was
// unported when the settings class landed. Per HEADER-FORMAT.md every (decompiled file, line) pair
// must be claimed exactly once, so writing the number here would make reverse-engineering/tools/check-headers.py report
// 15852 as claimed by two files. The number is stated in prose above instead, and the correct fix is
// a one-line edit to ChangeHooks.cs -- demoting its 15852 entry to a sub-entry under the introducer
// it already has, since a seam is not a port of the member that used to fill it -- after which this
// header should take the number back. That edit is out of scope for this file and is reported to
// the caller rather than made.
//
// WIRING, CORRECTED. This note used to open "In the shipped build SortAlgo is not subscribed to
// anything". That is wrong, and export/ contradicts it directly: decompiled 8862-8863, in the
// window's OnEnable, are `EditorApplication.playmodeStateChanged -= SortAlgo;` followed by `+=`.
// Entering or leaving play mode rebuilds Unity's editor styles, which drops the background this
// method writes into them, so the subscription is not incidental -- without it the cosmetic
// background survives until the first play-mode entry and then silently reverts. That pair is now
// written out in ControllerEditor.Window.cs.
//
// The rest of the note stands. SortAlgo is *also* passed directly as the change callback of four
// settings (graphBackgroundIsTexture, cosmeticGraphActive, gridBackgroundColor and
// graphBackgroundTexture, decompiled 1247-1359), and is additionally called outright at 15918 and
// inside a SettingsChangeScope at 3525. In this package those four settings instead raise
// EditorSettings.onGraphBackgroundChanged, which nothing assigns yet, so editing a graph-background
// setting still persists the value without re-applying it. Wiring it is the same one-line field
// initialiser that ChangeHooks.cs already uses for onMatchingOptionsChanged -- but that assignment
// lives in ChangeHooks.cs, which this port does not own, and it additionally needs
// ApplyGraphBackground widened from private to internal here, exactly as RefreshSharedConditions was
// widened for the other seam. Neither half is done unilaterally: half a paired edit leaves either a
// widened member nothing reads or a reference that does not compile. It is reported to the caller.
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
// table rather than re-derived; the body itself was not re-diffed on the pass that rewrote this
// header. What was re-checked on that pass is everything the header asserts about the rest of the
// package: each of the five members the deleted NOT PORTED section named was located in export/ and
// then found in the package under the ported name recorded above; every call site of SortAlgo in
// export/ was enumerated, which is what turned up the OnEnable subscription at 8862-8863 that the
// WIRING note had denied; and the state of the two ChangeHooks.cs seams was read from that file
// rather than assumed.

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
        /// decompiler left in the colour choice, and for its two callers: the play-mode
        /// subscription, which is made in ControllerEditor.Window.cs's <c>OnEnable</c>, and the
        /// four cosmetic settings, whose seam is not assigned yet.
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
