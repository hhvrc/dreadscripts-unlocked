// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static ListPredicate   -> ForEachClip(BlendTree),      line 3785
//   static VerifyPredicate -> AnyClip(BlendTree),          line 3794
//   static FillPredicate   -> ForEachBlendTree,            line 3824
//   static WritePredicate  -> AnyBlendTree,                line 3833
//   static ForgotPredicate -> ForEachMotion,               line 3853
//   static StopPredicate   -> ForEachMotion(onTree, onClip), line 3866
//   static CheckPredicate  -> AnyMotion,                   line 3879
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// Motion-tree walkers. A Motion is either an AnimationClip (a leaf) or a BlendTree (which holds
// further Motions), so anything that wants to reach the clips under a state has to recurse. Each
// walker comes as a pair: an Any* form whose visitor returns bool and stops the walk on true, and
// a ForEach* form implemented as the Any* form with a visitor that always returns false. The
// vendor wrote them that way and it is kept, because the short-circuit is the part that is easy to
// get wrong.
//
// The recursion flags differ between the two families and this is deliberate on the vendor's part:
//   * AnyClip's includeNested controls whether nested blend trees are descended into at all.
//   * AnyBlendTree has two: includeNested for the recursion, and includeSelf for whether the tree
//     it was called on is itself visited. The recursive call passes includeSelf false, because the
//     child was already visited by the enclosing Any() over children.

using System;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Runs <paramref name="action"/> against every clip under the blend tree.</summary>
        internal static void ForEachClip(this BlendTree tree, Action<AnimationClip> action, bool includeNested = true)
        {
            tree.AnyClip(c =>
            {
                action(c);
                return false;
            }, includeNested);
        }

        /// <summary>
        /// Whether any clip under the blend tree satisfies <paramref name="predicate"/>, stopping
        /// at the first that does.
        /// </summary>
        internal static bool AnyClip(this BlendTree tree, Func<AnimationClip, bool> predicate,
            bool includeNested = true)
        {
            foreach (ChildMotion child in tree.children)
            {
                if (child.motion == null)
                {
                    continue;
                }

                if (child.motion is AnimationClip clip)
                {
                    if (predicate(clip))
                    {
                        return true;
                    }
                }
                else if (includeNested && child.motion is BlendTree nested && nested.AnyClip(predicate))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Runs <paramref name="action"/> against the tree and every tree under it.</summary>
        internal static void ForEachBlendTree(this BlendTree tree, Action<BlendTree> action,
            bool includeNested = true, bool includeSelf = true)
        {
            tree.AnyBlendTree(t =>
            {
                action(t);
                return false;
            }, includeNested, includeSelf);
        }

        /// <summary>
        /// Whether the tree or any tree under it satisfies <paramref name="predicate"/>, stopping
        /// at the first that does.
        /// </summary>
        internal static bool AnyBlendTree(this BlendTree tree, Func<BlendTree, bool> predicate,
            bool includeNested = true, bool includeSelf = true)
        {
            if (includeSelf && predicate(tree))
            {
                return true;
            }

            return tree.children.Any(c =>
            {
                if (!(c.motion is BlendTree child) || !child)
                {
                    return false;
                }

                if (predicate(child))
                {
                    return true;
                }

                return includeNested && child.AnyBlendTree(predicate, includeNested: true, includeSelf: false);
            });
        }

        /// <summary>
        /// Runs <paramref name="action"/> against every motion in the tree rooted at
        /// <paramref name="motion"/> -- both the blend trees and the clips.
        /// </summary>
        internal static void ForEachMotion(this Motion motion, Action<Motion> action)
        {
            motion.AnyMotion(t =>
            {
                action(t);
                return false;
            }, c =>
            {
                action(c);
                return false;
            });
        }

        /// <summary>
        /// <see cref="ForEachMotion(Motion, Action{Motion})"/> with the blend trees and the clips
        /// handled by separate callbacks.
        /// </summary>
        internal static void ForEachMotion(this Motion motion, Action<BlendTree> onTree, Action<AnimationClip> onClip)
        {
            motion.AnyMotion(t =>
            {
                onTree(t);
                return false;
            }, c =>
            {
                onClip(c);
                return false;
            });
        }

        /// <summary>
        /// Whether any motion in the tree rooted at <paramref name="motion"/> satisfies the
        /// predicate for its kind, stopping at the first that does. Either predicate may be null
        /// to ignore that kind; a null motion is not a match.
        /// </summary>
        /// <remarks>
        /// A blend tree is offered to <paramref name="onTree"/> before its children are walked, so
        /// a predicate that matches the root never sees the children at all.
        /// </remarks>
        internal static bool AnyMotion(this Motion motion, Func<BlendTree, bool> onTree,
            Func<AnimationClip, bool> onClip)
        {
            if (motion == null)
            {
                return false;
            }

            if (motion is BlendTree tree)
            {
                if (onTree != null && onTree(tree))
                {
                    return true;
                }

                foreach (ChildMotion child in tree.children)
                {
                    if (child.motion.AnyMotion(onTree, onClip))
                    {
                        return true;
                    }
                }

                return false;
            }

            return onClip != null && motion is AnimationClip clip && onClip(clip);
        }
    }
}
