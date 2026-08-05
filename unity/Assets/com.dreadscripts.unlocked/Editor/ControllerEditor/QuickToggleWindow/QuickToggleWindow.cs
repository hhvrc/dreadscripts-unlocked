// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class QuickToggleWindow -> lifted to a top-level type, line 4157
//
//   m_WrapperInitializer        -> root,                     line 4452
//   annotationInitializer       -> targets,                  line 4454
//   _VisitorInitializer         -> states,                   line 4456
//   _AlgoInitializer            -> targetList,               line 4458
//   _MapperInitializer          -> mergeModeColors,          line 4460
//   _InitializerInitializer     -> mergeMode,                line 4467
//   definitionInitializer       -> existingClipCount,        line 4469
//   regInitializer              -> mergePerState,            line 4471
//   testsInitializer            -> hasExistingClips,         line 4473
//   propertyInitializer         -> existingClipsExpanded,    line 4475
//   _ProcessorInitializer       -> labels,                   line 4477
//   UtilityWindowBase<>.title   -> Title,                    line 4485
//   RegisterTests/LogoutTests   -> AdvancedMode (property),  lines 4488, 4494
//   InterruptTests/ManageTests  -> MergeByDefault (property),lines 4500, 4506
//   UpdateTests                 -> CalculateWindowSize,      line 4819
//   ChangeTests                 -> RefreshMergeMode,         line 4824
//   SortTests                   -> ShowAt(Vector2),          line 4829
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Two more members of the decompiled type are ported into the sibling partials, which claim them:
//   OnCustomGUI, the UtilityWindowBase explicit implementation (line 4678) -> QuickToggleWindow.Gui.cs
//   OnCustomConfirm (line 4768) -> QuickToggleWindow.ClipWriting.cs
//
// CLOSED ON 2026-08-05 -- the two [SpecialName] accessor pairs, previously deferred:
//
//   RegisterTests()/LogoutTests(bool)  lines 4488, 4494 -> AdvancedMode
//   InterruptTests()/ManageTests(bool) lines 4500, 4506 -> MergeByDefault
//
// Both were blocked on the ControllerEditor settings singleton, which has since landed:
// EditorSettings.advancedQuickToggle and .mergeQuickToggle are `internal BoolSetting` fields at
// EditorSettings.Fields.cs lines 243 and 250. ILSpy rendered each pair as two [SpecialName]
// methods; they are restored as the properties they were. Neither has a ported caller yet -- their
// only shipped caller is AssetTests below -- so they are deliberately live-but-unused rather than
// omitted, because they are complete faithful ports and not stubs.
//
// STILL DEFERRED, and no longer for the reason previously recorded:
//
//   AssetTests(List<AnimatorState>, Transform, List<GameObject>)   line 4511
//       The factory, and by far the largest member: it seeds the window from the states the user
//       selected in the graph and builds the whole ReorderableList with its four callbacks (the
//       per-row target field, the component-type picker, the property picker and the On/Off or
//       property/value editors).
//
//       ITS BLOCKERS ARE ALL CLEARED. Every dependency the old note named as missing is now in the
//       package, verified by name on 2026-08-05:
//         * EditorSettings.advancedQuickToggle / .mergeQuickToggle -- EditorSettings.Fields.cs
//           lines 243, 250; .defaultState -- line 466.
//         * EditorUtils FlushQueue/ConnectQueue -> EditorUtils.OverlayLabel
//           (EditorUtils.OverlayLabels.cs; the two decompiled methods are collapsed into one, so
//           call sites written against FlushQueue must skip the `draw` argument by name).
//         * EditorUtils ResetQueue -> EditorUtils.HandleScrollWheel (same file, line 5886).
//         * The Type extension InstantiateResolver -> EditorUtils.Is(this Type, Type)
//           (EditorUtils.Types.cs, decompiled line 2659).
//       Its already-ported dependencies, unchanged: SearchablePickerPopup<T>,
//       ReorderableListHelper<T>, ComponentQueue, GUIColorScope, EditorUtils.SliceLeft (decompiled
//       SortResolver) and EditorUtils.HandleMultiDragAndDrop (decompiled AwakeRules).
//
//       It is therefore not blocked -- it is simply not written yet. This pass established that and
//       stopped there rather than attempting a ~200-line reconstruction it could not also verify;
//       whoever picks it up should not go looking for a blocker, because there is not one.
//       Two API differences to expect against the decompiled text: ComponentQueue.PropertyName,
//       .ComponentIndex and .GameObject are properties here where the decompilation shows accessor
//       methods, and SearchablePickerPopup.PickerEntry.FirstExtra() is the property `firstExtra`.
//
// The compiler-generated closure classes <>c (line 4159), <>c__DisplayClass18_0 (4256),
// <>c__DisplayClass18_1 (4399) and <>c__DisplayClass18_2 (4440) are decompiler artifacts of the
// lambdas inside AssetTests and are not types the author wrote; they are not ported, and would be
// restored as ordinary lambdas whenever AssetTests is. Every one of their bodies is a one-liner.
//
// Nothing obfuscator scaffolding-shaped (always-null statics, marker classes, licence gates) is
// present in this type; every member above is live code.
//
// Audit status: VERIFIED -- every member this file declares was diffed against export
// ControllerEditor.cs on 2026-08-05. The eleven fields at 4452-4483 match in type, order and
// initialiser, including both array initialisers element for element; Title (4485), AdvancedMode
// (4488-4497), MergeByDefault (4500-4509), CalculateWindowSize (4819), RefreshMergeMode (4824) and
// ShowAt (4829) match statement for statement. RefreshMergeMode is written as an if/else where the
// decompiler emitted a nested conditional expression; same evaluation order, same result. The
// blocker prose for the one remaining deferral (AssetTests) was re-derived from scratch on this
// pass and every dependency it names was confirmed present by file and line -- it is no longer a
// blocker list but a note that nothing blocks it.

using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The Quick Toggle window: given a set of animator states the user selected in the graph, it
    /// collects a list of scene objects to toggle and writes a constant curve for each of them into
    /// the clip on every one of those states.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The window only ever touches clips and states that already exist. Despite the name it does
    /// not create a layer, an expression parameter or an expressions-menu control — the caller in
    /// the main editor has already made the states, and this window fills in their motions.
    /// </para>
    /// <para>
    /// Each selected state carries its own merge-or-replace choice (<see cref="mergePerState"/>):
    /// merge adds the curves to the clip already on the state, replace swaps in a brand new clip.
    /// See <see cref="OnCustomConfirm"/> for exactly what that writes to disk.
    /// </para>
    /// </remarks>
    internal partial class QuickToggleWindow : UtilityWindowBase<QuickToggleWindow>
    {
        /// <summary>
        /// The transform the animated paths are made relative to — normally the avatar root, since
        /// that is what the animator resolves paths against.
        /// </summary>
        private Transform root;

        /// <summary>The rows of the list: one object, component and property per row.</summary>
        private List<ComponentQueue> targets;

        /// <summary>The states whose motions will be written.</summary>
        private List<AnimatorState> states;

        private ReorderableListHelper<ComponentQueue> targetList;

        /// <summary>
        /// Backgrounds for the summary button, indexed by <see cref="mergeMode"/>: green for merge,
        /// cyan for replace, yellow for a mixed selection.
        /// </summary>
        private static readonly Color[] mergeModeColors =
        {
            Color.green,
            Color.cyan,
            Color.yellow
        };

        /// <summary>
        /// Summary of <see cref="mergePerState"/>: 0 when every state merges, 1 when every state
        /// replaces, 2 when they disagree. Kept in step by <see cref="RefreshMergeMode"/>.
        /// </summary>
        private int mergeMode;

        /// <summary>
        /// How many of the selected states already had an <see cref="AnimationClip"/> when the
        /// window was opened.
        /// </summary>
        /// <remarks>
        /// This is a snapshot taken at creation and is never recomputed, so the count in the foldout
        /// header can disagree with the number of rows drawn beneath it if a state's motion changes
        /// while the window is open. That is the shipped behaviour.
        /// </remarks>
        private int existingClipCount;

        /// <summary>Per-state merge (true) or replace (false), parallel to <see cref="states"/>.</summary>
        private bool[] mergePerState;

        /// <summary>Whether any selected state has a clip at all; the merge UI is hidden if none do.</summary>
        private bool hasExistingClips;

        private bool existingClipsExpanded;

        /// <summary>
        /// Column labels for the window and the list rows.
        /// </summary>
        /// <remarks>
        /// Only <c>labels[0]</c> ("Root") is used. The remaining three describe the list columns and
        /// are unreferenced in the shipped build — the row drawer spells its labels out inline — but
        /// they are kept here as they document what each column means.
        /// </remarks>
        private static readonly GUIContent[] labels =
        {
            new GUIContent("Root", "Relative path root of the animation"),
            new GUIContent("Target", "Target GameObject or GameObject containing target Component"),
            new GUIContent("Component Index", "Which component to toggle. -1 is GameObject. 0 is Transform (Not toggleable)"),
            new GUIContent("Enabled", "What the toggled state is when animated")
        };

        internal override string Title => "CEditor QuickToggle";

        /// <summary>
        /// Whether the list rows edit an arbitrary animatable property and a numeric value
        /// (advanced), or just an On/Off toggle (simple).
        /// </summary>
        /// <remarks>
        /// This is a view of <see cref="EditorSettings.advancedQuickToggle"/> rather than window
        /// state, so the mode the user last chose is the one the next window opens in. Both
        /// accessors go through the setting, which is what makes the toggle button in the list
        /// header persist immediately.
        /// </remarks>
        private static bool AdvancedMode
        {
            get => EditorSettings.Instance.advancedQuickToggle;
            set => EditorSettings.Instance.advancedQuickToggle.value = value;
        }

        /// <summary>
        /// The merge-or-replace choice a newly opened window starts every state on: true merges into
        /// the clip already on the state, false replaces it.
        /// </summary>
        /// <remarks>
        /// Distinct from <see cref="mergeMode"/>, which is a summary of the per-state flags as they
        /// stand now. This is only ever read when the window is built and when the header button
        /// flips it; see <see cref="RefreshMergeMode"/> for the live summary.
        /// </remarks>
        private static bool MergeByDefault
        {
            get => EditorSettings.Instance.mergeQuickToggle;
            set => EditorSettings.Instance.mergeQuickToggle.value = value;
        }

        /// <summary>
        /// The size the window wants: a fixed frame plus one row per target, plus the help box and
        /// the existing-clips strip when those are shown.
        /// </summary>
        /// <remarks>
        /// At least one row is always allowed for, so an empty list still leaves room for the list's
        /// own "nothing here" placeholder.
        /// </remarks>
        internal Vector2 CalculateWindowSize()
        {
            return new Vector2(370f, 48 + 22 * Mathf.Max(1, targets.Count) + 28
                + (!string.IsNullOrEmpty(helpMessage) ? 38 : 0)
                + (hasExistingClips ? 32 : 0));
        }

        /// <summary>
        /// Recomputes the merge/replace/mixed summary from the per-state flags.
        /// </summary>
        internal void RefreshMergeMode()
        {
            if (mergePerState.All(b => b))
            {
                mergeMode = 0;
            }
            else
            {
                mergeMode = mergePerState.All(b => !b) ? 1 : 2;
            }
        }

        /// <summary>Shows the window at a screen position, sized to its content.</summary>
        internal void ShowAt(Vector2 screenPosition)
        {
            ShowAt(screenPosition, CalculateWindowSize());
        }
    }
}
