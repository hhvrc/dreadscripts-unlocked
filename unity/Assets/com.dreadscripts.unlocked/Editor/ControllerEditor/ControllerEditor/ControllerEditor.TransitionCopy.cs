// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   CustomizeAlgo -> CopyTransitionSettings, line 14693
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ==================================== NOTES ===================================================
//
// This file exists to own one member, on the same basis as ControllerEditor.CollapsibleSection.cs:
// `CustomizeAlgo` sits in the decompiled file's transition-manipulation region, not in any section
// body, and three separate ported files had already named it as their blocker --
// ControllerEditor.TransitionSection.cs for the transition settings copy/paste buttons,
// ControllerEditorWindow.cs and ControllerEditorWindow.Defaults.cs for applying the user's default
// transition settings to a newly created transition. Whichever of them had ported it would have
// been claiming a member belonging to none of their regions, so it is ported here and decompiled
// line 14693 is claimed exactly once.
//
// The obfuscated name is noise: nothing is customised and there is no algorithm. What the body
// does is copy one AnimatorStateTransition's *settings* onto another while leaving the target's
// identity alone, which is the whole reason it is not simply a CopySerialized call.
//
// The five fields it saves and restores are exactly the ones that say which transition this is
// rather than how it behaves -- its conditions, where it goes (a state, a state machine, or the
// exit node), and its name. CopySerialized would overwrite all five, which would turn the target
// into a duplicate of the source and detach it from the graph. Everything CopySerialized copies
// that is not restored afterwards is therefore the definition of a "setting" here: exit time,
// duration, offset, interruption source and ordering, mute, solo, and can-transition-to-self --
// the same set the transition settings section draws.
//
// The neighbouring `RateAlgo` (line 14709), which is nothing but a two-way type test that forwards
// to this member when both transitions are state transitions, is not ported and is not claimed
// here; no ported caller needs it yet.
//
// =============================== DELIBERATE DEVIATION =========================================
//
// The parameters are renamed from the decompiled `reference` and `col` to `source` and `target`.
// `col` in particular is actively misleading -- it is not a collection, it is the transition being
// written to, and it is the second argument at all ten shipped call sites. The order is
// unchanged: source first, target second, matching Unity's own
// `EditorUtility.CopySerialized(source, destination)` which the body wraps.
//
// The decompiled local `string text = col.name;` is renamed `name`, and the restore assignments are
// grouped after the CopySerialized call exactly as decompiled. Nothing else differs.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- every statement below was compared against decompiled
// ControllerEditor.cs lines 14693-14707: the five saved fields, the CopySerialized call, the five
// restore assignments in decompiled order, and the trailing SetDirty on the target. The range
// contains no `goto`, no residual `switch` dispatch, no `while (true)` and no unresolved
// `smethod_N`, so no deobfuscator fault applies here.

using UnityEditor;
using UnityEditor.Animations;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Transition settings copy

        /// <summary>
        /// Copies every behavioural setting of <paramref name="source"/> onto
        /// <paramref name="target"/>, leaving the target's identity in the graph untouched.
        /// </summary>
        /// <param name="source">The transition to read settings from. Not modified.</param>
        /// <param name="target">
        /// The transition to write settings to. Keeps its own conditions, destination and name.
        /// </param>
        /// <remarks>
        /// Implemented as a full <see cref="EditorUtility.CopySerialized"/> with the five identity
        /// fields saved beforehand and put back afterwards, rather than as a field-by-field copy of
        /// the settings. That is deliberate in the shipped code and worth keeping: it means a
        /// transition setting added by a future Unity version is carried across automatically,
        /// where an explicit list would silently drop it.
        /// </remarks>
        private static void CopyTransitionSettings(AnimatorStateTransition source, AnimatorStateTransition target)
        {
            AnimatorCondition[] conditions = target.conditions;
            AnimatorStateMachine destinationStateMachine = target.destinationStateMachine;
            AnimatorState destinationState = target.destinationState;
            bool isExit = target.isExit;
            string name = target.name;

            EditorUtility.CopySerialized(source, target);

            target.conditions = conditions;
            target.destinationStateMachine = destinationStateMachine;
            target.destinationState = destinationState;
            target.isExit = isExit;
            target.name = name;
            EditorUtility.SetDirty(target);
        }

        #endregion
    }
}
