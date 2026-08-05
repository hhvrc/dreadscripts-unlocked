// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   DestroyVisitor -> DrawStateSection, line 12086
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ================================ LICENCE GATE, NOT PORTED ====================================
//
// The shipped body computes the state count, then wraps everything it draws in the obfuscator's
// inline licence test:
//
//     if (((Func<bool>)delegate
//     {
//         using HMACSHA256 h = new HMACSHA256(Encoding.UTF8.GetBytes("z)lSj/1y ..." + m_BridgeAnnotation));
//         return m_ParamsAnnotation == Convert.ToBase64String(
//             h.ComputeHash(Encoding.UTF8.GetBytes(m_TagAnnotation + _WriterAnnotation)));
//     })())
//     { ...the four section rows... }
//
// -- that is, HMAC(licenseKey)(currentDateStamp + hardwareId) compared against licenseToken, the
// same check the obfuscator scattered through dozens of methods in this assembly (see
// ControllerEditor.State.cs on `licenseToken`). It is dropped on this package's standing basis:
// the vendor's validation endpoint is gone, so the comparison can only ever fail, and an
// unlicensed ControllerEditor draws the whole state section as nothing. The guarded body is ported
// as if the test had passed, so the section is unconditional here.
//
// Note what this means for the shipped tool, because it explains why the count is computed
// *outside* the gate and the drawing *inside* it: unlicensed, this method walks the selection and
// then throws the answer away. Nothing else in the shipped body sits outside the gate.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ==============================
//
// Nothing here is stubbed. What is ported is the whole of the method outside the four rows: the
// visibility guard and the selected-state tally that titles the first row. The four rows
// themselves are deferred, because every participant in them is unported. In shipped order:
//
//   DeleteAnnotation(GetVisitor,   $"State Count: {n}",     showStateCount,   iscont2: true,  3)
//   DeleteAnnotation(CalcVisitor,  "State Settings",        showStateSettings,iscont2: true,  4)
//   -- and, only when AnimatorTypeCache.IsVRCSDKAvailable():
//   DeleteAnnotation(RunVisitor,   "VRC Parameter Drivers", showVRCDrivers,   iscont2: false, 5)
//   DeleteAnnotation(CloneVisitor, "VRC Tracking Control",  showVRCTracking,  iscont2: false, 6)
//
// Blockers, none of them subtle -- all five are members of the god class's still-unported body
// regions:
//
//   DeleteAnnotation (9920) -- the collapsible-row helper the section is built out of. Draws
//     `def()` inside a box with a 7px vertical strip beside it that collapses the row, or, when the
//     row is already collapsed, just a toolbar button carrying the row's title. Its own
//     dependencies are all present in the package already (EditorUtils.Button,
//     EditorUtils.SetGuiStateOnEvent / GetGuiState, BoolSetting.Toggle), so it is portable today --
//     it is left alone only because it is shared with the transition and controller sections and
//     porting it from here would risk a second copy. It is unclaimed; see the task notes.
//   GetVisitor   (12110) -- the "State Count" row: the selection summary, the deselect button and
//     the per-state list.
//   CalcVisitor  (12388) -- the "State Settings" row: the multi-edit AnimatorState inspector.
//   RunVisitor   (12481) -- the "VRC Parameter Drivers" row.
//   CloneVisitor (12488) -- the "VRC Tracking Control" row.
//
// The `iscont2` argument is DeleteAnnotation's "box this row" flag and `visitor3counter` is the
// row's identity in EditorUtils' GUI-state dictionary, under the key `CollapsePart{n}`, which is
// how the collapse strip learns the height of the row it sits next to. The literals 3-6 are
// therefore not free: they are shared with the rows of the other sections, which use the remaining
// numbers. Whoever ports DeleteAnnotation must keep them.
//
// No GUI scope is opened here. Every layout scope of the shipped method lives inside
// DeleteAnnotation, on the far side of the deferral, so this port cannot leave a Begin without its
// End -- see the house rule about unbalanced GUI scopes. That is why the deferral can be clean
// here where ControllerEditor.Window.cs had to record a deviation.
//
// ======================================== NOTES ================================================
//
// The tally counts the three graph pseudo-nodes -- Any State, Entry and Exit -- alongside the real
// states, because from the user's point of view they are things you can select and the section
// header should say so. They cannot be in `selectedStates`: that list holds AnimatorState assets,
// and the pseudo-nodes have no asset behind them, which is why the graph tracks them as three
// separate booleans (see ControllerEditor.State.cs).
//
// GetVisitor recomputes this identical expression as its own first statement rather than taking it
// as an argument. Both copies are transcribed as they stand when that member lands; they are not
// factored into a shared helper here, because introducing one would be an invention and would put
// a member in the package that the assembly does not contain.
//
// `stateSectionVisible` (decompiled `_Descriptor`) is the guard, not the setting. The setting
// `EditorSettings.Instance.editingStates` is mirrored into it by the toolbar in OnGUI, and it is
// also raised by the selection-sync code whenever a state is selected -- which is what makes the
// section sticky, as State.cs records. Reading the setting directly here would drop that.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- decompiled lines 12086-12108 were read in place on this pass and every
// statement above the deferral is transcribed from them, as is the argument list of each of the
// four deferred rows. The five blocking members were confirmed to exist at the lines named
// (12110, 12388, 12481, 12488, 9920) and confirmed absent from the package by search. Their bodies
// were not read beyond what the one-line descriptions above claim, which is why this is PARTIAL.

using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region State section

        /// <summary>
        /// Draws the window's state section: the collapsible rows that summarise and edit whatever
        /// states the animator graph currently has selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The section is drawn only while <see cref="stateSectionVisible"/> is set. That flag is
        /// not the user's "Editing States" setting on its own -- the setting seeds it, but selecting
        /// a state in the graph also raises it, so the section appears when it becomes relevant and
        /// then stays put rather than flickering in and out as the selection changes.
        /// </para>
        /// <para>
        /// What the section contains is four collapsible rows, and all four are deferred on unported
        /// members; see the file header, which names each row, its blocker and the exact arguments
        /// it is called with. The licence test the shipped body wraps those rows in is dropped, also
        /// per the file header.
        /// </para>
        /// </remarks>
        private static void DrawStateSection()
        {
            if (!stateSectionVisible)
            {
                return;
            }

            // The number the first row titles itself with. Any State, Entry and Exit are counted as
            // states here even though they are not AnimatorState assets and so cannot appear in
            // selectedStates: to the user they are four kinds of the same thing. The shipped method
            // computes this before its licence check, so it is computed even when nothing is drawn.
            int selectedStateCount = selectedStates.Count
                + (anyStateNodeSelected ? 1 : 0)
                + (entryNodeSelected ? 1 : 0)
                + (exitNodeSelected ? 1 : 0);

            // DEFERRED, in shipped order, each a DeleteAnnotation row (the collapsible-row helper,
            // decompiled 9920, unported):
            //   GetVisitor   as $"State Count: {selectedStateCount}", EditorSettings.Instance.showStateCount,    boxed,   slot 3
            //   CalcVisitor  as "State Settings",                     EditorSettings.Instance.showStateSettings, boxed,   slot 4
            //   and, when AnimatorTypeCache.IsVRCSDKAvailable():
            //   RunVisitor   as "VRC Parameter Drivers",              EditorSettings.Instance.showVRCDrivers,    unboxed, slot 5
            //   CloneVisitor as "VRC Tracking Control",               EditorSettings.Instance.showVRCTracking,   unboxed, slot 6
            // selectedStateCount is the first row's title and has no other consumer until that row
            // lands. See the file header for each blocker.
        }

        #endregion
    }
}
