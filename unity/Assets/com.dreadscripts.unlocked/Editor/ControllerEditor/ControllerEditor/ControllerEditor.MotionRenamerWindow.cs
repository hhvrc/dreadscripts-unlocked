// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   MotionRenamerWindow      -> MotionRenamerWindow, lines 3923-3996 (vendor name; unobfuscated)
//     _ManagerMapper         -> motions,      line 3925
//     _ItemMapper            -> newName,      line 3927
//     _SpecificationMapper   -> focusPending, line 3929
//     OnGUI / OnLostFocus    -> unchanged (Unity messages), lines 3931 and 3992
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The one-field popup behind Motion &gt; Rename: type a name, press Enter or Ok, and every
        /// motion queued into <see cref="motions"/> takes it.
        /// </summary>
        /// <remarks>
        /// An embedded motion is renamed by setting <c>name</c>; a motion that owns its own file has
        /// to go through <see cref="AssetDatabase.RenameAsset"/>, which may pick a different name to
        /// avoid a collision. Those cases are collected and reported in one dialog at the end,
        /// because the user asked for one name and did not get it.
        ///
        /// The window closes on lost focus, so it behaves like the inspector's own rename field.
        /// </remarks>
        internal class MotionRenamerWindow : EditorWindow
        {
            /// <summary>
            /// Every motion to rename. <see cref="MotionEmbedMenu"/> adds to this rather than
            /// opening a second window, so a multi-selection collects into one popup.
            /// </summary>
            public List<Motion> motions = new List<Motion>();

            public string newName = "";

            /// <summary>Focus the text field once, on the first repaint after opening.</summary>
            private bool focusPending = true;

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
                if (current.isKey
                    && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
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

                Object[] targets;
                Object[] toRename = targets = motions.Where(m => m != null).Distinct().ToArray();
                Undo.RecordObjects(targets, "Rename motion");

                StringBuilder renamedDifferently = new StringBuilder();

                foreach (Motion motion in (Motion[])toRename)
                {
                    if (MotionEmbedMenu.IsEmbedded(motion))
                    {
                        motion.name = newName;
                    }
                    else if (motion.name != newName)
                    {
                        string previousName = motion.name;
                        string assetPath = AssetDatabase.GetAssetPath(motion);
                        string actualName = MotionEmbedMenu.GenerateUniqueName(assetPath, newName);

                        if (newName != actualName)
                        {
                            renamedDifferently.AppendLine(previousName + " -> " + actualName);
                        }

                        AssetDatabase.RenameAsset(assetPath, actualName);
                    }

                    EditorUtility.SetDirty(motion);
                    MotionEmbedMenu.MarkScenesDirty();
                }

                Close();

                if (renamedDifferently.Length > 0)
                {
                    EditorUtility.DisplayDialog("Motion Rename",
                        $"The following clips are not embedded and have been renamed accordingly:\n{renamedDifferently}",
                        "Ok");
                }
            }

            public void OnLostFocus()
            {
                Close();
            }
        }
    }
}
