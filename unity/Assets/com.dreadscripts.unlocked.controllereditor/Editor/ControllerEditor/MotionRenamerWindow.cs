// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class MotionRenamerWindow -> lifted to a top-level type, lines 3923-3996
//     _ManagerMapper       -> motions
//     _ItemMapper          -> newName
//     _SpecificationMapper -> focusPending
//   OnGUI                -> OnGUI,        line 3931
//   OnLostFocus          -> OnLostFocus,  line 3992
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. Nothing from the type is left unported. Lifting the
// nested class to a top-level type is the same treatment PhysBoneEditor received in the ADOverhaul
// assembly.
//
// The only caller is MotionEmbedMenu.RenameMotion (ControllerEditor.cs lines 2303/2314/2324).
// Those three menu members are now ported in MotionEmbedMenu.cs, so this window has its entry
// point; the decompiler's spurious `while (true)` noted in that file belongs to the caller, not
// to here.
//
// ASSET MUTATION — read before calling. See the remarks on OnGUI for the full account of what is
// and is not undoable, and of the name collisions this window can create.
//
// Audit status: PARTIAL -- the mapping above was re-checked against reverse-engineering/export/ (the class at 3923,
// its three fields at 3925/3927/3929, OnGUI at 3931 and OnLostFocus at 3992); the OnGUI body was
// not re-diffed statement by statement.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A one-field utility window that renames a batch of motions to the same new name. Opened from
    /// the animator state's Motion context menu, it collects every selected motion and applies the
    /// typed name to all of them at once.
    /// </summary>
    /// <remarks>
    /// This window predates <see cref="UtilityWindowBase{T}"/> and does not use it: it is created
    /// through <c>GetWindow</c> rather than a factory, precisely so that a second invocation while
    /// it is already open finds the same instance and appends to <see cref="motions"/> — that is how
    /// a multi-selection rename accumulates. Deriving it from the base class, which closes any
    /// existing window before creating a new one, would break that.
    /// </remarks>
    internal class MotionRenamerWindow : EditorWindow
    {
        /// <summary>
        /// Everything to be renamed. The caller adds one motion per invocation; nulls and repeats
        /// are tolerated and filtered out when the rename is applied.
        /// </summary>
        public List<Motion> motions = new List<Motion>();

        /// <summary>The name to apply, seeded by the caller from the first motion added.</summary>
        public string newName = "";

        /// <summary>
        /// True until the text field has been focused once. The field can only be focused after it
        /// has been drawn, so the focus is taken on the frame following its first layout rather than
        /// when the window is created.
        /// </summary>
        private bool focusPending = true;

        /// <summary>
        /// Draws the name field and the Cancel/Ok pair, and applies the rename when the user
        /// confirms with either Ok or Return.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Undo: <see cref="Undo.RecordObjects"/> is registered for every motion, but it only covers
        /// the in-memory <c>name</c> of an embedded motion. Renaming a standalone clip goes through
        /// <see cref="AssetDatabase.RenameAsset"/>, which renames a file on disk and is outside the
        /// undo system entirely — a Ctrl+Z afterwards reverts the recorded object name and leaves it
        /// disagreeing with the file it lives in. Ported as shipped.
        /// </para>
        /// <para>
        /// Overwrites: a standalone clip cannot clobber an existing asset. The target name is put
        /// through <see cref="MotionEmbedMenu.GenerateUniqueName"/> first, so a collision is resolved
        /// by numbering, and every clip that had to be renumbered is listed in a dialog afterwards.
        /// An embedded motion gets the typed string verbatim — no uniqueness check and no file-name
        /// sanitising — so renaming several embedded motions at once deliberately leaves them all
        /// sharing one name, which is legal for sub-assets but makes them indistinguishable in the
        /// animator window.
        /// </para>
        /// <para>
        /// A standalone clip whose name already equals the typed name is skipped entirely, so it is
        /// never renumbered into "Name 1" by a rename that was a no-op.
        /// </para>
        /// </remarks>
        public void OnGUI()
        {
            bool confirmed = false;

            const string controlName = "Rename Field";
            GUI.SetNextControlName(controlName);
            newName = EditorGUILayout.TextField(newName);

            if (focusPending)
            {
                focusPending = false;
                GUI.FocusControl(controlName);
            }

            Event current = Event.current;
            if (current.isKey && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                && GUI.GetNameOfFocusedControl() == controlName)
            {
                confirmed = true;
                current.Use();
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }

                confirmed |= GUILayout.Button("Ok");
            }

            if (!confirmed)
            {
                return;
            }

            Motion[] targets = motions.Where(m => m != null).Distinct().ToArray();
            Undo.RecordObjects(targets, "Rename motion");

            // Collects the clips whose requested name was already taken, to report at the end.
            StringBuilder renumbered = new StringBuilder();

            foreach (Motion motion in targets)
            {
                if (MotionEmbedMenu.IsEmbedded(motion))
                {
                    motion.name = newName;
                }
                else if (motion.name != newName)
                {
                    string previousName = motion.name;
                    string assetPath = AssetDatabase.GetAssetPath(motion);
                    string uniqueName = MotionEmbedMenu.GenerateUniqueName(assetPath, newName);

                    if (newName != uniqueName)
                    {
                        renumbered.AppendLine(previousName + " -> " + uniqueName);
                    }

                    AssetDatabase.RenameAsset(assetPath, uniqueName);
                }

                EditorUtility.SetDirty(motion);

                // Marking every scene dirty once per motion rather than once per batch. Redundant,
                // but MarkAllScenesDirty is idempotent and this is what shipped.
                MotionEmbedMenu.MarkScenesDirty();
            }

            Close();

            if (renumbered.Length > 0)
            {
                EditorUtility.DisplayDialog(
                    "Motion Rename",
                    $"The following clips are not embedded and have been renamed accordingly:\n{renumbered}",
                    "Ok");
            }
        }

        /// <summary>
        /// Closes the window as soon as it loses focus, discarding whatever was typed.
        /// </summary>
        /// <remarks>
        /// The window is a click-through rename prompt positioned next to the inspector, so clicking
        /// anywhere else is read as dismissing it. The cost is that the rename is lost if the user
        /// clicks away to check a name.
        /// </remarks>
        public void OnLostFocus()
        {
            Close();
        }
    }
}
