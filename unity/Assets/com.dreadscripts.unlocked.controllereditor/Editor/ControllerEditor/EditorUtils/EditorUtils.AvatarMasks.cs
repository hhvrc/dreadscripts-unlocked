// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static AddRules      -> CreateBaseLayerMask,      line 4596
//   static InvokeRules   -> CreateCombinedMask,       line 4621
//   static FindRules     -> CreateMaskForLayer,       line 4634
//   static ExcludeRules  -> EnsureRootTransformPath,  line 4690
//   static InitRules     -> EnsureTransformPaths,     line 4700
//   static VisitRules    -> AddTransformPath(mask, string), line 4714
//   static DefineRules   -> MergeFrom,                line 4721
//   static StartRules    -> GetTransformPaths,        line 4744
//   static ReadRules     -> GetTransformPathSet,      line 4755
//   static SelectRules   -> TryGetBodyPart,           line 4766
//   static RemoveRules   -> CreateEmptyMask,          line 4807
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// AvatarMask is a pair of independent lists: thirteen humanoid body-part toggles, and a list of
// transform paths with an active flag each. Everything below manipulates one or both.
//
// THE DUMMY TRANSFORMS. A mask with an empty transform list is treated by Unity as "no transform
// restriction at all", and one with a single entry does not render its transform tree in the
// inspector. Both are indistinguishable from a mask the user has not filled in yet, so
// EnsureRootTransformPath and EnsureTransformPaths pad the list with throwaway GameObjects created
// and immediately destroyed -- what survives is the *path string*, not the object. The paths that
// result are the empty string and "Dummy Transform".
//
// THE HARD-CODED 13. AvatarMaskBodyPart runs 0..12 with LastBodyPart = 13; the loops iterate the
// bound literally, as shipped, rather than through the enum.
//
// CreateMaskForLayer is the interesting one: it reads what a layer actually animates and builds the
// mask that would allow exactly that. Bindings with a path name a transform, so the path is added
// to the transform list; bindings with an empty path and an Animator type are muscle curves, so
// TryGetBodyPart guesses the body part from the property name. When no avatar root is supplied it
// materialises each path as a temporary GameObject chain instead, because AvatarMask.AddTransformPath
// takes a Transform and there is no string overload.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// A mask for the controller's base layer that permits the union of everything its other
        /// layers' masks permit -- so the base layer no longer competes with them.
        /// </summary>
        /// <returns>
        /// Null if the base layer already has a mask and the user declined the confirmation.
        /// </returns>
        internal static AvatarMask CreateBaseLayerMask(AnimatorController controller)
        {
            bool cancel = controller.layers[0].avatarMask != null;
            if (cancel && EditorUtility.DisplayDialog("Existing Mask!",
                    "The Base Layer already uses a mask! Continue Anyway?", "Continue", "Cancel"))
            {
                cancel = false;
            }

            if (cancel)
            {
                return null;
            }

            AvatarMask mask = CreateEmptyMask();
            mask.EnsureRootTransformPath();

            // Layer 0 is the base layer being built for, hence starting at 1.
            for (int i = 1; i < controller.layers.Length; i++)
            {
                AnimatorControllerLayer layer = controller.layers[i];
                if (layer.avatarMask)
                {
                    mask.MergeFrom(layer.avatarMask);
                }
            }

            mask.EnsureTransformPaths();
            return mask;
        }

        /// <summary>
        /// A mask permitting everything the controller animates, derived from the layers'
        /// contents rather than from their masks.
        /// </summary>
        /// <param name="avatarRoot">
        /// The hierarchy the animated paths are relative to. May be null, in which case each path
        /// is materialised as a temporary object chain -- slower, but it works without an avatar.
        /// </param>
        internal static AvatarMask CreateCombinedMask(AnimatorController controller, Transform avatarRoot)
        {
            AvatarMask mask = CreateEmptyMask();
            mask.EnsureRootTransformPath();

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                mask.MergeFrom(CreateMaskForLayer(layer, avatarRoot));
            }

            mask.EnsureTransformPaths();
            return mask;
        }

        /// <summary>
        /// A mask permitting exactly what <paramref name="layer"/> animates: every transform path
        /// its clips touch, plus the humanoid body parts its Animator-level curves belong to.
        /// </summary>
        /// <param name="avatarRoot">
        /// As <see cref="CreateCombinedMask"/>. When null, a single throwaway "Dummy" object stands
        /// in for the root and each animated path gets its own temporary chain.
        /// </param>
        internal static AvatarMask CreateMaskForLayer(AnimatorControllerLayer layer, Transform avatarRoot)
        {
            AvatarMask mask = new AvatarMask();

            Transform root = avatarRoot;
            if (!avatarRoot)
            {
                root = new GameObject("Dummy").transform;
            }

            mask.AddTransformPath(root, recursive: false);
            if (!avatarRoot)
            {
                UnityEngine.Object.DestroyImmediate(root.gameObject);
            }

            for (int i = 0; i < 13; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }

            HashSet<string> added = new HashSet<string>();
            layer.stateMachine.ForEachState(s =>
            {
                s.motion.ForEachMotion(null, c =>
                {
                    foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(c)
                                 .Concat(AnimationUtility.GetObjectReferenceCurveBindings(c)))
                    {
                        if (binding.path == string.Empty)
                        {
                            // No path means the curve targets the Animator itself -- a muscle or
                            // IK curve -- so it is a body part rather than a transform.
                            if (binding.type == typeof(Animator)
                                && TryGetBodyPart(binding.propertyName, out AvatarMaskBodyPart bodyPart))
                            {
                                mask.SetHumanoidBodyPartActive(bodyPart, true);
                            }

                            continue;
                        }

                        TempGameObjectHierarchy temporary = null;
                        Transform target;
                        if (!avatarRoot)
                        {
                            temporary = new TempGameObjectHierarchy(binding.path);
                            target = temporary.gameObjects.Last().transform;
                        }
                        else
                        {
                            target = avatarRoot.Find(binding.path);
                        }

                        if (target && !added.Contains(binding.path))
                        {
                            mask.AddTransformPath(target, recursive: false);
                            added.Add(binding.path);
                        }

                        temporary?.Destroy();
                    }
                });
            });

            mask.EnsureTransformPaths();
            return mask;
        }

        /// <summary>
        /// Gives the mask a transform list if it has none, so Unity does not read it as
        /// "unrestricted". The entry added is the empty path.
        /// </summary>
        /// <param name="force">Add the entry even if the list is already non-empty.</param>
        internal static void EnsureRootTransformPath(this AvatarMask mask, bool force = false)
        {
            if (!force && mask.transformCount != 0)
            {
                return;
            }

            GameObject temporary = new GameObject();
            mask.AddTransformPath(temporary.transform);
            UnityEngine.Object.DestroyImmediate(temporary);
        }

        /// <summary>
        /// <see cref="EnsureRootTransformPath"/>, plus a second entry ("Dummy Transform") if the
        /// list would otherwise hold only one -- a one-entry list does not draw as a tree in the
        /// inspector.
        /// </summary>
        /// <param name="force">Add the second entry even if the list already has two or more.</param>
        internal static void EnsureTransformPaths(this AvatarMask mask, bool force = false)
        {
            mask.EnsureRootTransformPath();
            if (!force && mask.transformCount > 1)
            {
                return;
            }

            GameObject root = new GameObject();
            GameObject child = new GameObject("Dummy Transform");
            child.transform.parent = root.transform;
            mask.AddTransformPath(child.transform);
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(child);
        }

        /// <summary>
        /// Adds a transform path by name. AvatarMask only accepts a live Transform, so the path is
        /// materialised as a temporary object chain and destroyed again once recorded.
        /// </summary>
        internal static void AddTransformPath(this AvatarMask mask, string path)
        {
            TempGameObjectHierarchy temporary = new TempGameObjectHierarchy(path);
            mask.AddTransformPath(temporary.gameObjects.Last().transform);
            temporary.Destroy();
        }

        /// <summary>
        /// Adds everything <paramref name="other"/> permits to <paramref name="mask"/>, leaving
        /// what <paramref name="mask"/> already permits alone -- a union, never a subtraction.
        /// </summary>
        /// <remarks>
        /// Only the *active* transform paths of <paramref name="other"/> are merged; an inactive
        /// entry is a path the other mask deliberately blocks and carries no permission to add.
        /// </remarks>
        internal static void MergeFrom(this AvatarMask mask, AvatarMask other)
        {
            for (int i = 0; i < 13; i++)
            {
                if (other.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i))
                {
                    mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, true);
                }
            }

            List<(string path, bool active)> paths = other.GetTransformPaths();
            if (paths.Count <= 0)
            {
                return;
            }

            HashSet<string> existing = mask.GetTransformPathSet();
            foreach ((string path, bool _) in paths.Where(p => !existing.Contains(p.path) && p.active))
            {
                mask.AddTransformPath(path);
                existing.Add(path);
            }
        }

        /// <summary>The mask's transform list, in order, each with its active flag.</summary>
        internal static List<(string path, bool active)> GetTransformPaths(this AvatarMask mask)
        {
            List<(string, bool)> paths = new List<(string, bool)>();
            int count = mask.transformCount;
            for (int i = 0; i < count; i++)
            {
                paths.Add((mask.GetTransformPath(i), mask.GetTransformActive(i)));
            }

            return paths;
        }

        /// <summary>
        /// The mask's transform paths as a set, for membership tests. The active flags are dropped.
        /// </summary>
        internal static HashSet<string> GetTransformPathSet(this AvatarMask mask)
        {
            HashSet<string> paths = new HashSet<string>();
            int count = mask.transformCount;
            for (int i = 0; i < count; i++)
            {
                paths.Add(mask.GetTransformPath(i));
            }

            return paths;
        }

        /// <summary>
        /// Guesses which humanoid body part an Animator curve belongs to from its property name --
        /// e.g. "LeftHand.Index.1 Stretch" is LeftFingers.
        /// </summary>
        /// <returns>
        /// False, with <paramref name="bodyPart"/> set to LastBodyPart, when the name matches none
        /// of the families below. Unity's muscle names do not encode the body part, so this is a
        /// substring heuristic rather than a lookup, and the order of the tests is what resolves an
        /// overlap: "Hand" wins over "Arm", and "Root" over everything but "Hand".
        /// </returns>
        internal static bool TryGetBodyPart(string propertyName, out AvatarMaskBodyPart bodyPart)
        {
            string[] armParts = { "Arm", "Forearm", "Shoulder" };
            string[] legParts = { "Leg", "Foot", "Toes" };
            string[] headParts = { "Neck", "Head", "Eye", "Jaw" };
            string[] bodyParts = { "Chest", "Spine" };

            bool isLeft = propertyName.Contains("Left");

            if (propertyName.Contains("Hand"))
            {
                bodyPart = isLeft ? AvatarMaskBodyPart.LeftFingers : AvatarMaskBodyPart.RightFingers;
                return true;
            }

            if (propertyName.Contains("Root"))
            {
                bodyPart = AvatarMaskBodyPart.Root;
                return true;
            }

            if (bodyParts.Any(propertyName.Contains))
            {
                bodyPart = AvatarMaskBodyPart.Body;
                return true;
            }

            if (armParts.Any(propertyName.Contains))
            {
                bodyPart = isLeft ? AvatarMaskBodyPart.LeftArm : AvatarMaskBodyPart.RightArm;
                return true;
            }

            if (legParts.Any(propertyName.Contains))
            {
                bodyPart = isLeft ? AvatarMaskBodyPart.LeftLeg : AvatarMaskBodyPart.RightLeg;
                return true;
            }

            if (headParts.Any(propertyName.Contains))
            {
                bodyPart = AvatarMaskBodyPart.Head;
                return true;
            }

            bodyPart = AvatarMaskBodyPart.LastBodyPart;
            return false;
        }

        /// <summary>
        /// A new mask with every humanoid body part switched off and an empty transform list --
        /// i.e. one that permits nothing until something is merged into it.
        /// </summary>
        internal static AvatarMask CreateEmptyMask()
        {
            AvatarMask mask = new AvatarMask();
            for (int i = 0; i < 13; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }

            return mask;
        }
    }
}
