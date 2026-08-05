// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: MoveSingleton (line 4171) and part of PublishSingleton (line 4209) of the current
// snapshot, plus the m_QueueIdentifier field (line 2988). Line numbers move with the snapshot; the
// member names below are the durable reference.
//
//   m_QueueIdentifier    -> membershipStates,                line 2988
//   MoveSingleton()      -> RefreshColliderStates(),         line 4171
//   PublishSingleton()   -> RefreshIgnoreTransformStates(),  line 4209  (partial, see below)
//
// PARTIAL PORT OF PublishSingleton. The decompiled method is two unrelated routines behind one
// branch on the Ignore Selection tool being armed: the branch ported here recomputes the membership
// states, and the other branch prunes each PhysBone's ignore list of nulls, of entries that are not
// under its root, and of entries already covered by an ancestor entry. That pruning branch reports
// its result through ADOverhaul.NewIdentifier (line 7806), which is not ported, so it is left out
// rather than reproduced without its user-visible feedback.
//
// 2019 vs 2022: identical.

using System.Collections.Generic;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// Per-entry membership of the scene list the armed selection tool is working over —
        /// <see cref="sceneColliders"/> for the collider tools, <see cref="candidateTransforms"/>
        /// for the ignore-transform tools — across the whole of <see cref="selectedPhysBones"/>.
        /// </summary>
        /// <remarks>
        /// Three states: 0 for "in none of the selected PhysBones", 1 for "in all of them", 2 for
        /// "in some but not all". The scene handles colour themselves by this value, which is what
        /// gives a multi-selection a visible mixed state instead of silently showing the first
        /// PhysBone's answer.
        /// <para>
        /// A byte array rather than an enum array because it indexes straight into the settings'
        /// three-colour table.
        /// </para>
        /// </remarks>
        private static byte[] membershipStates;

        /// <summary>
        /// Recomputes <see cref="membershipStates"/> over <see cref="sceneColliders"/>, for the
        /// Collision Selection handles.
        /// </summary>
        /// <remarks>
        /// The loop is nested PhysBone-outermost so that state 2 can be treated as absorbing: once
        /// an entry is known to disagree between two PhysBones no later PhysBone can change that, so
        /// it is skipped for the rest of the pass.
        /// <para>
        /// The first PhysBone establishes the baseline — on that pass every entry is set to 1 or 0
        /// outright — and only from the second onwards can a disagreement promote an entry to 2.
        /// </para>
        /// </remarks>
        private static void RefreshColliderStates()
        {
            membershipStates = new byte[sceneColliders.Length];

            bool isFirstPhysBone = true;
            foreach (VRCPhysBone physBone in selectedPhysBones)
            {
                for (int i = 0; i < membershipStates.Length; i++)
                {
                    if (membershipStates[i] == 2)
                    {
                        continue;
                    }

                    List<VRCPhysBoneColliderBase> physBoneColliders = physBone.colliders;
                    if (physBoneColliders != null && physBoneColliders.Contains(sceneColliders[i]))
                    {
                        membershipStates[i] = (byte)((membershipStates[i] != 0 || isFirstPhysBone) ? 1 : 2);
                    }
                    else if (membershipStates[i] == 1 && !isFirstPhysBone)
                    {
                        membershipStates[i] = 2;
                    }
                    else
                    {
                        membershipStates[i] = 0;
                    }
                }

                isFirstPhysBone = false;
            }
        }

        /// <summary>
        /// Recomputes <see cref="membershipStates"/> over <see cref="candidateTransforms"/>, for the
        /// Ignore Selection handles. Structurally identical to
        /// <see cref="RefreshColliderStates"/>; see there for why the pass is shaped as it is.
        /// </summary>
        private static void RefreshIgnoreTransformStates()
        {
            membershipStates = new byte[candidateTransforms.Length];

            bool isFirstPhysBone = true;
            foreach (VRCPhysBone physBone in selectedPhysBones)
            {
                for (int i = 0; i < membershipStates.Length; i++)
                {
                    if (membershipStates[i] == 2)
                    {
                        continue;
                    }

                    List<Transform> ignored = physBone.ignoreTransforms;
                    if (ignored != null && ignored.Contains(candidateTransforms[i]))
                    {
                        membershipStates[i] = (byte)((membershipStates[i] != 0 || isFirstPhysBone) ? 1 : 2);
                    }
                    else if (membershipStates[i] == 1 && !isFirstPhysBone)
                    {
                        membershipStates[i] = 2;
                    }
                    else
                    {
                        membershipStates[i] = 0;
                    }
                }

                isFirstPhysBone = false;
            }
        }
    }
}
