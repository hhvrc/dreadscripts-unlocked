// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ReflectVisitor -> DrawTransitionSection, line 12529
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// THE FILE NAME IS WRONG AND IS KEPT ONLY BECAUSE IT WAS ASSIGNED. This file was commissioned as
// "ControllerSection" on the strength of ControllerEditor.Window.cs's header, which lists
// `ReflectVisitor()` as "the controller/layer section". That description does not survive contact
// with the decompiled body. Decompiled line 12529 reads
//
//     private void ReflectVisitor()
//     {
//         if (parserAnnotation && <licence predicate>)
//         {
//             string token = $"Transition Count: {_DefinitionAnnotation.Count}";
//             DeleteAnnotation(DeleteVisitor,  token,                    ...showTransitionsCount, ...);
//             DeleteAnnotation(CreateVisitor, "Transition Settings",     ...showTransitionSettings, ...);
//             DeleteAnnotation(NewVisitor,    "Transition Conditions",   ...showTransitionConditions, ...);
//         }
//     }
//
// `parserAnnotation` is `transitionSectionVisible` in ControllerEditor.State.cs's rename table
// (decompiled line 8238), `_DefinitionAnnotation` is `selectedTransitionEdits` (line 8030), and all
// three settings it reads are the `showTransition*` ones. Every observable thing about the member
// says transitions, not controllers or layers: it is the root of the window's TRANSITION section,
// the first of the four sections OnGUI draws. The ported member is therefore named
// DrawTransitionSection rather than being given a name the code would contradict. The file name is
// left as assigned so the orchestration that expects this path still finds it; renaming the file
// and correcting Window.cs's header are both changes to shared files, which this port is not
// permitted to make. Whoever integrates this should fix both together.
//
// Nothing in this file draws a controller or a layer. If you came here looking for the layer list,
// it is not ported yet and it is not ReflectVisitor.
//
// =========================== LICENCE GATE, NOT PORTED =========================================
//
// The shipped guard is a two-term conjunction: the section-visible flag AND an inline
// `(Func<bool>)delegate { ... }` that recomputes an HMACSHA256 over the licence key and date/HWID
// stamp and compares it against `m_ParamsAnnotation` (`licenseToken`). That second term is the
// obfuscator's scattered licence test, the same block that appears verbatim in dozens of other
// members of this assembly. It is dropped here on the package-wide basis: the vendor's validation
// endpoint is gone, so the predicate can only ever evaluate false, and an unlicensed
// ReflectVisitor draws nothing at all -- the entire transition section would be invisible to the
// legitimate holders this restoration exists for. The ported guard keeps only the first term and
// behaves as though the licence check passed.
//
// =========================== PARTIAL PORT, EACH WITH ITS BLOCKER ===============================
//
// The guard is ported for real. The three statements inside it are not, and nothing stands in for
// them: this file declares no empty helper, no placeholder Action and no substitute drawing code.
// Four decompiled members are missing from the package, and all four are needed before a single
// one of the three calls can be written.
//
//   DeleteAnnotation (decompiled 9920) -- the collapsible-section helper every one of the three
//     statements calls. It takes the section's body as an `Action`, the header label, the
//     BoolSetting that remembers whether the section is expanded, whether the body is boxed, and an
//     index used to key the section's measured height in EditorUtils's GUI-state relay. When the
//     setting is off it draws the label as a toolbar button that toggles it back on; when it is on
//     it draws the body beside a thin full-height button that collapses it again. It is portable
//     -- its own dependencies (EditorUtils.Button, SetGuiStateOnEvent, GetGuiState, and
//     BoolSetting.Toggle) all exist in the package -- but it belongs to a different decompiled
//     region and porting it here would claim a member another wave is likely assigned, which is the
//     duplicate-port mistake this repo has already paid for twice. It is named as a blocker rather
//     than ported.
//
//   DeleteVisitor (decompiled 12544) -- the body of the first section, the "Transition Count"
//     list. Draws one clickable row per entry of `selectedTransitionEdits` in three columns, with a
//     deselect button per row and one for the whole set. Blocked in turn on MapVisitor, RunAlgo,
//     CalculateAnnotation and ManageWrapper, the four refresh routines its row click calls; none of
//     the four is ported.
//
//   CreateVisitor (decompiled 12615) -- the body of the second section, "Transition Settings": the
//     property-field grid over `transitionInspectorSerialized`, plus copy and paste buttons.
//     Blocked on CustomizeAlgo (decompiled 14693), the transition-settings copier, which
//     ControllerEditorWindow.Defaults.cs already records as unported.
//
//   NewVisitor (decompiled 12672) -- the body of the third section, "Transition Conditions": the
//     match-parameter/mode/value toggles and whichever of the three condition ReorderableLists is
//     current, with up/down arrow handling that walks focus between the "Threshold<n>" controls.
//     Blocked on UpdateVisitor (decompiled 12980), which its change check calls; the package has
//     that member only as the assignable seam `EditorSettings.onMatchingOptionsChanged`, and
//     calling the seam instead of the method would invert the direction of the dependency -- the
//     seam exists so the settings can notify the window, not so the window can notify itself.
//
// The three statements are left as an inline DEFERRED comment in shipped order, the same
// convention ControllerEditor.Window.cs uses for the section calls it cannot make yet. No GUI scope
// is opened by this member, so nothing here can leave Unity's layout stack unbalanced.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: PARTIAL -- the guard, the three call targets, the three setting names and the
// interpolated "Transition Count" label were transcribed from decompiled lines 12529-12542 on this
// pass, and the four blocker line numbers above were each opened and confirmed to land on the
// member named. The bodies of those four blockers were read only far enough to describe them and
// were not diffed statement by statement, which is why this is PARTIAL rather than VERIFIED.

using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Transition section

        /// <summary>
        /// Draws the window's transition section: the transition count list, the transition
        /// settings grid, and the condition editor, each in its own collapsible sub-section.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the first of the four section roots <c>OnGUI</c> calls, and it is a root only --
        /// it owns no drawing of its own. All it does is decide whether the section is shown at all
        /// and then hand three bodies to the shared collapsible-section helper, which draws each
        /// one's header, remembers its expanded state in the corresponding
        /// <see cref="EditorSettings"/> flag, and gives it a collapse handle.
        /// </para>
        /// <para>
        /// The visibility flag it tests is <see cref="transitionSectionVisible"/> rather than
        /// <c>EditorSettings.Instance.editingTransitions</c> directly. Those are not the same thing:
        /// the setting is the user's persisted preference, and the field is the session mirror that
        /// <c>OnGUI</c>'s section toolbar writes whenever the preference changes. Reading the mirror
        /// is what lets other code force the section open for a frame without editing the user's
        /// saved settings.
        /// </para>
        /// <para>
        /// The three sub-section bodies, and the helper itself, are not ported yet. See the file
        /// header: each is named there with the members that block it, and nothing is stubbed in
        /// their place -- the guard below is real, and the body it guards is genuinely absent rather
        /// than faked. The licence predicate that the shipped guard ANDs into this test is also
        /// dropped; see the file header.
        /// </para>
        /// </remarks>
        private void DrawTransitionSection()
        {
            if (!transitionSectionVisible)
            {
                return;
            }

            // DEFERRED, in shipped order. Each line is one call to the collapsible-section helper
            // (decompiled DeleteAnnotation, line 9920), passing the sub-section's body as an
            // Action, its header label, the setting that remembers whether it is expanded, whether
            // its body is drawn boxed, and the index that keys its measured height:
            //
            //   (DrawSelectedTransitionList, $"Transition Count: {selectedTransitionEdits.Count}",
            //    EditorSettings.Instance.showTransitionsCount,     boxed: true,  index 0)
            //   (DrawTransitionSettings,     "Transition Settings",
            //    EditorSettings.Instance.showTransitionSettings,   boxed: true,  index 1)
            //   (DrawTransitionConditions,   "Transition Conditions",
            //    EditorSettings.Instance.showTransitionConditions, boxed: false, index 2)
            //
            // The count label is rebuilt every frame because it interpolates the live selection
            // size, which is why it is the one label the shipped code does not pass as a literal.
        }

        #endregion
    }
}
