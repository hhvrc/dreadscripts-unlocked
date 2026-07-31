// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/UtilityWindowBase.cs

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Base for the small modal-ish utility windows (rename, quick input, pickers): a scrolling body
    /// supplied by the subclass, an optional info box, and an optional Confirm button that closes the
    /// window.
    /// </summary>
    /// <typeparam name="T">
    /// The concrete window type, so <see cref="Create"/> and <see cref="CloseAll"/> can work in
    /// terms of it without a cast.
    /// </typeparam>
    /// <remarks>
    /// In the shipped build <c>Title</c> and <c>OnCustomGUI</c> were explicit implementations of an
    /// interface named <c>CustomUtilityWindow&lt;T&gt;</c>. Nothing else in the assembly references
    /// that interface and it survives only as a mangled member name in the decompile, so it is
    /// reconstructed here as plain abstract members — which is all the call sites ever needed.
    /// </remarks>
    internal abstract class UtilityWindowBase<T> : EditorWindow where T : UtilityWindowBase<T>
    {
        /// <summary>
        /// Set to the window itself by <see cref="Create"/>. Unity revives editor windows across
        /// domain reloads, but not the non-serialized state a subclass needs, so a revived window
        /// finds this null and closes itself rather than drawing something half-initialised.
        /// </summary>
        private T self;

        private bool showConfirmButton;

        /// <summary>Set false to grey out the Confirm button while input is incomplete.</summary>
        internal bool canConfirm = true;

        /// <summary>Optional message shown in an info box above the body.</summary>
        internal string helpMessage;

        private Vector2 scrollPosition;

        /// <summary>Window title, supplied by the subclass.</summary>
        internal abstract string Title { get; }

        /// <summary>Draws the window body.</summary>
        internal abstract void OnCustomGUI();

        /// <summary>Called when the user confirms, just before the window closes.</summary>
        internal abstract void OnCustomConfirm();

        /// <summary>
        /// Closes any window of this type that is already open and returns a fresh one. Not shown
        /// yet — call <see cref="ShowAt"/>.
        /// </summary>
        internal static T Create(bool showConfirmButton = true, string helpMessage = "")
        {
            CloseAll();

            T window = CreateInstance<T>();
            window.titleContent.text = window.Title;
            window.showConfirmButton = showConfirmButton;
            window.self = window;
            window.helpMessage = helpMessage;
            return window;
        }

        /// <summary>
        /// Closes every open window of this type, so a second invocation replaces the first rather
        /// than stacking on it.
        /// </summary>
        internal static void CloseAll()
        {
            foreach (T window in Resources.FindObjectsOfTypeAll<T>())
            {
                try
                {
                    window.Close();
                }
                catch
                {
                    // A window Unity has partly torn down can throw from Close; destroying it
                    // outright still gets it off the screen.
                    DestroyImmediate(window);
                }
            }
        }

        /// <summary>Shows the window as a utility window at the given screen position and size.</summary>
        internal void ShowAt(Vector2 screenPosition, Vector2 size)
        {
            ShowUtility();
            position = new Rect(screenPosition, size);
        }

        internal void Confirm()
        {
            OnCustomConfirm();
            Close();
        }

        private void OnGUI()
        {
            if (self == null)
            {
                Close();
                return;
            }

            using (new ScrollViewScope(ref scrollPosition))
            {
                if (!string.IsNullOrEmpty(helpMessage))
                {
                    EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
                }

                OnCustomGUI();
            }

            if (!showConfirmButton)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(!canConfirm))
            {
                // EditorUtils has not been ported yet, so this still carries the decompiler's name
                // for it. It is a layout button returning true on click, and needs renaming when
                // EditorUtils lands.
                if (EditorUtils.DisableQueue("Confirm"))
                {
                    Confirm();
                }
            }
        }
    }
}
