// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   _003C_003Ec__DisplayClass379_0 -> ParameterRewriter, line 7128
//     _ReaderReg     -> exactMatch
//     m_BridgeReg    -> oldName
//     m_StrategyReg  -> newName
//     PushServer     -> Matches,             line 7136
//     ViewServer     -> Rewrite,             line 7149
//     CollectServer  -> RewriteDrivers,      line 7158
//     ResolveServer  -> RewriteBlendTree,    line 7182
//     ListServer     -> RewriteState,        line 7197
//     VerifyServer   -> RewriteTransition,   line 7232
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// _003C_003Ec__DisplayClass379_0 is a [CompilerGenerated] closure class, not a type anyone wrote:
// it is what the C# compiler emitted to carry SetAlgo's three captured locals into the anonymous
// delegates it passes to the state and transition walkers. The closure is NOT ported as such. What
// the original author actually wrote was a method with three parameters and some lambdas over them,
// and that is what is restored here -- as an ordinary class holding the same three values, so the
// rewrite primitives can be named, documented and unit-reasoned about instead of appearing five
// times inline. ControllerMerge.RenameParameter constructs one and calls into it.
//
// The decompilation shows ListServer/RewriteState and VerifyServer/RewriteTransition twice: once as
// named methods on the closure, and again inlined into the delegate literals at the SetAlgo call
// site (ControllerEditor.cs lines 13493 and 13527). Those are the same two method bodies, character
// for character; ILSpy simply printed both the hoisted method and its inlined use. They are ported
// once.
//
// WHAT THIS REWRITER DOES NOT REACH -- these are the user-visible failure modes of a rename, and
// they are documented rather than fixed, per this project's policy:
//   * Only the VRChat parameter driver is understood among state behaviours. Every other
//     StateMachineBehaviour -- VRCAnimatorPlayAudio, VRCAnimatorTrackingControl's parameter-less
//     cases aside, and any third-party or user-written behaviour that names a parameter in a
//     string field -- is walked past untouched. It keeps the old name and silently stops matching.
//   * VRCExpressionParameters and VRCExpressionsMenu assets are not touched at all. They are not
//     part of the animator object graph and nothing here goes looking for them, so a renamed
//     parameter loses its menu control and its expression-parameter entry without any warning.
//   * Nothing here registers an Undo operation; see the remarks on Rewrite.
//
// Audit status: VERIFIED -- the three fields and all six methods were diffed against export
// ControllerEditor.cs on 2026-08-05, against the hoisted closure at lines 7128-7260 and, for
// RewriteState and RewriteTransition, additionally against their inlined duplicates at the SetAlgo
// call site (13493 and 13527), which confirms the header's claim that the two printings are the
// same bodies.
// Three deliberate, behaviour-preserving rewrites, none of them silent:
//   * Matches is written as `if (exactMatch) return reference == oldName; return
//     reference.Contains(oldName);`. Export nests the same three outcomes inside an inverted
//     compound guard (`if (!exact || !(s == old)) { if (!exact) return Contains; return false; }
//     return true;`). Truth table checked case by case: identical, including the null behaviour the
//     remarks on Matches describe.
//   * RewriteState hoists `state.transitions` into a local instead of re-indexing the property each
//     iteration. Unity returns a fresh array per call but the AnimatorStateTransition elements are
//     the same references, and the write-back is per transition, so the result is identical.
//   * RewriteTransition collapses the original's per-edit read/write of the conditions array to one
//     read and one write. Already documented on the method; the array is a detached copy either
//     way.
// The range contains no goto, no residual switch dispatch, no `while (true)` and no unresolved
// smethod_N, so no deobfuscator fault applies here.

using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Rewrites every reference to one animator parameter name into another, across the parts of an
    /// animator object graph that name a parameter as a string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity stores parameter references as loose strings -- in transition conditions, in a state's
    /// four optional parameter overrides, in a blend tree's axes, and inside SDK state behaviours --
    /// with no back-reference from the parameter to its users. Renaming a parameter therefore means
    /// walking the graph and patching strings, and anything the walk does not reach keeps pointing
    /// at a name that no longer exists.
    /// </para>
    /// <para>
    /// The instance carries the three values that every primitive needs, which is why this is a
    /// class rather than a set of static methods: it is called several thousand times over a large
    /// controller and threading three arguments through each call buys nothing.
    /// </para>
    /// </remarks>
    internal sealed class ParameterRewriter
    {
        private readonly bool exactMatch;

        private readonly string oldName;

        private readonly string newName;

        /// <param name="exactMatch">
        /// True to rewrite only references whose whole value is <paramref name="oldName"/>. False
        /// makes this a substring replacement -- see the remarks on <see cref="Matches"/>, which is
        /// the more dangerous of the two modes and is not the one the merge flow uses.
        /// </param>
        internal ParameterRewriter(string oldName, string newName, bool exactMatch)
        {
            this.oldName = oldName;
            this.newName = newName;
            this.exactMatch = exactMatch;
        }

        /// <summary>Whether <paramref name="reference"/> is one this rewriter should change.</summary>
        /// <remarks>
        /// <para>
        /// In exact mode the reference has to equal <c>oldName</c> outright, which is what a rename
        /// normally means. In substring mode any reference merely <em>containing</em> <c>oldName</c>
        /// qualifies and every occurrence inside it is replaced -- so renaming "Hat" to "Cap" also
        /// turns "HatColor" into "CapColor" and "ChatOpen" into "CCapOpen". That mode exists for the
        /// bulk prefix-rewriting flows elsewhere in the tool; the merge flow passes exact.
        /// </para>
        /// <para>
        /// A null reference throws here in substring mode, where <see cref="string.Contains(string)"/>
        /// is called on it, but is answered false in exact mode. Callers reach this through
        /// <see cref="Rewrite"/>, which screens nulls out first; the one path that does not is
        /// <see cref="RewriteDrivers"/>, which asks about a driver entry's name directly. Ported as
        /// shipped.
        /// </para>
        /// </remarks>
        internal bool Matches(string reference)
        {
            if (exactMatch)
            {
                return reference == oldName;
            }

            return reference.Contains(oldName);
        }

        /// <summary>
        /// <paramref name="reference"/> with the rename applied, or unchanged when it does not match.
        /// </summary>
        /// <remarks>
        /// Empty and null references are returned untouched, so the many optional parameter fields
        /// that are simply blank cost nothing and cannot throw. Note that nothing in this class
        /// registers an <see cref="Undo"/> operation -- only <see cref="EditorUtility.SetDirty"/> is
        /// called, so the edits persist but a Ctrl+Z after a rename does nothing at all. That is how
        /// the tool shipped and it is the single most important thing to know before calling it.
        /// </remarks>
        internal string Rewrite(string reference)
        {
            if (string.IsNullOrEmpty(reference) || !Matches(reference))
            {
                return reference;
            }

            return reference.Replace(oldName, newName);
        }

        /// <summary>
        /// Rewrites the parameter names inside any VRChat parameter-driver behaviours in
        /// <paramref name="behaviours"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both halves of an entry are rewritten: the parameter being driven, and the parameter
        /// being copied from in the driver's copy mode. The type test is an exact type comparison
        /// rather than an <c>is</c> check, so a subclass of the driver would not be recognised --
        /// irrelevant for the sealed SDK type, but it is why this cannot be widened to other
        /// behaviours by inheritance.
        /// </para>
        /// <para>
        /// The behaviours are reached through <see cref="AnimatorTypeCache.ParameterDriverBinding"/>
        /// so that this file never names an SDK type and compiles without the SDK present. Callers
        /// must still gate the call on <see cref="AnimatorTypeCache.IsVRCSDKAvailable"/>, because
        /// the binding's constructor assumes the driver's serialized layout.
        /// </para>
        /// <para>
        /// The entries are walked backwards. Nothing here removes an entry, so the direction does
        /// not matter; it is preserved because it is what shipped.
        /// </para>
        /// </remarks>
        internal void RewriteDrivers(StateMachineBehaviour[] behaviours)
        {
            foreach (StateMachineBehaviour behaviour in behaviours)
            {
                if (behaviour.GetType() != AnimatorTypeCache.ParameterDriverType)
                {
                    continue;
                }

                AnimatorTypeCache.ParameterDriverBinding driver =
                    new AnimatorTypeCache.ParameterDriverBinding(behaviour);

                for (int i = driver.parameters.Count - 1; i >= 0; i--)
                {
                    AnimatorTypeCache.ParameterDriverBinding.ParameterEntry entry = driver.parameters[i];

                    // The Matches guard is redundant -- Rewrite re-tests it -- but it avoids a
                    // serialized-property write, and an apply, for the entries that do not match.
                    if (Matches(entry.Name))
                    {
                        entry.Name = Rewrite(entry.Name);
                    }

                    if (Matches(entry.Source))
                    {
                        entry.Source = Rewrite(entry.Source);
                    }
                }

                EditorUtility.SetDirty(behaviour);
            }
        }

        /// <summary>
        /// Rewrites a blend tree's two axis parameters, and those of every tree nested inside it.
        /// </summary>
        /// <remarks>
        /// Non-blend-tree motions are ignored, so this can be handed a state's motion field directly.
        /// Both axes are written unconditionally -- <see cref="Rewrite"/> returns the value unchanged
        /// when it does not match -- which means the tree is marked dirty on every visit whether or
        /// not anything changed. A tree reached from two states is therefore visited twice; the
        /// second pass is a no-op because the name it is looking for is already gone.
        /// </remarks>
        internal void RewriteBlendTree(Motion motion)
        {
            BlendTree tree = motion as BlendTree;
            if (tree == null)
            {
                return;
            }

            tree.blendParameter = Rewrite(tree.blendParameter);
            tree.blendParameterY = Rewrite(tree.blendParameterY);
            EditorUtility.SetDirty(tree);

            foreach (Motion child in tree.children.Select(c => c.motion))
            {
                RewriteBlendTree(child);
            }
        }

        /// <summary>
        /// Rewrites every parameter reference owned by <paramref name="state"/>: its four optional
        /// parameter overrides, its motion if that is a blend tree, the conditions on its outgoing
        /// transitions, and its own state behaviours.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The four overrides are only touched when their matching <c>*ParameterActive</c> flag is
        /// set. An inactive override keeps whatever stale name is sitting in it, so turning one back
        /// on after a rename resurrects the old name -- faithful to the shipped behaviour, and
        /// harmless in practice because Unity clears the field when the flag is cleared in the
        /// inspector.
        /// </para>
        /// <para>
        /// A transition's <c>conditions</c> array is a copy, not a view, so each one has to be read
        /// out, edited and assigned back; the assignment is what commits the change.
        /// </para>
        /// </remarks>
        internal void RewriteState(AnimatorState state)
        {
            if (state.cycleOffsetParameterActive)
            {
                state.cycleOffsetParameter = Rewrite(state.cycleOffsetParameter);
            }

            if (state.mirrorParameterActive)
            {
                state.mirrorParameter = Rewrite(state.mirrorParameter);
            }

            if (state.speedParameterActive)
            {
                state.speedParameter = Rewrite(state.speedParameter);
            }

            if (state.timeParameterActive)
            {
                state.timeParameter = Rewrite(state.timeParameter);
            }

            RewriteBlendTree(state.motion);

            AnimatorStateTransition[] transitions = state.transitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                AnimatorCondition[] conditions = transitions[i].conditions;
                for (int j = conditions.Length - 1; j >= 0; j--)
                {
                    conditions[j].parameter = Rewrite(conditions[j].parameter);
                }

                transitions[i].conditions = conditions;
            }

            EditorUtility.SetDirty(state);

            if (AnimatorTypeCache.IsVRCSDKAvailable())
            {
                RewriteDrivers(state.behaviours);
            }
        }

        /// <summary>
        /// Rewrites the conditions on one transition, whichever of the four kinds it is.
        /// </summary>
        /// <remarks>
        /// The original re-read the conditions array from the transition on every iteration and
        /// assigned the whole array back after each single edit, which is O(n) round trips through
        /// Unity's serialisation for an n-condition transition. Collapsed here to one read and one
        /// write: the array is a detached copy either way, so the result is identical.
        /// </remarks>
        internal void RewriteTransition(AnimatorStateTransitionSet transitionSet)
        {
            AnimatorCondition[] conditions = transitionSet.Conditions;
            for (int i = conditions.Length - 1; i >= 0; i--)
            {
                conditions[i].parameter = Rewrite(conditions[i].parameter);
            }

            transitionSet.Conditions = conditions;
            EditorUtility.SetDirty(transitionSet.transition);
        }
    }
}
