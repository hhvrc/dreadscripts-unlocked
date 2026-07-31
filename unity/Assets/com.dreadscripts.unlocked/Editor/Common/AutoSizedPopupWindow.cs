// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/FloatingActionWindow.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/AutoSizedPopupWindow.cs

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// A popup that shrinks to fit whatever it draws, and closes as soon as it loses focus. Used for
    /// the small click-through panels that hang off a button.
    /// </summary>
    /// <remarks>
    /// IMGUI gives no way to ask what a block of layout will measure without drawing it, so the
    /// window draws itself once fully transparent, reads the size back from the layout system, and
    /// resizes to it. <see cref="phase"/> tracks that: draw-to-measure, then resize, then draw
    /// normally forever after.
    /// <para>
    /// The phase only advances on repaint. GUILayout requires that every layout pass be matched by a
    /// repaint pass over the same controls, so the content has to be drawn on both — advancing on
    /// every OnGUI call instead, as the shipped build did, spent the measure phase on a layout event
    /// that never reaches the measuring code and then resized the window to the 0x0 it had not
    /// measured yet.
    /// </para>
    /// </remarks>
    internal class AutoSizedPopupWindow : EditorWindow
    {
        private enum Phase
        {
            /// <summary>Drawing transparently to find out how big the content is.</summary>
            Measure,
            /// <summary>Applying the measured size.</summary>
            Resize,
            /// <summary>Settled; drawing normally.</summary>
            Drawing
        }

        private static AutoSizedPopupWindow current;

        private Phase phase;

        private float measuredWidth;
        private float measuredHeight;

        private Vector2 scroll;

        /// <summary>False until <see cref="Show"/> has configured the window. See <see cref="OnGUI"/>.</summary>
        private bool initialized;

        private Action onGUI;

        /// <summary>
        /// Drawn instead of <see cref="onGUI"/> during the measure phase, for content whose real
        /// drawing has side effects that should not happen twice. Optional.
        /// </summary>
        private Action onMeasureGUI;

        /// <summary>
        /// Replaces any popup already open with a new one at <paramref name="position"/>.
        /// </summary>
        internal static void Show(Rect position, Action onGUI, Action onMeasureGUI = null)
        {
            CloseCurrent();

            current = CreateInstance<AutoSizedPopupWindow>();
            current.onGUI = onGUI;
            current.onMeasureGUI = onMeasureGUI;
            current.initialized = true;
            current.ShowUtility();
            current.position = position;
        }

        private static void CloseCurrent()
        {
            if (current == null)
            {
                return;
            }

            try
            {
                current.Close();
            }
            catch
            {
                // A window Unity has partly torn down throws from Close; destroying it still gets
                // it off the screen, and there is nothing useful to do if that fails too.
                try
                {
                    DestroyImmediate(current);
                }
                catch
                {
                    // ignored
                }
            }

            current = null;
        }

        private void OnGUI()
        {
            // Unity revives editor windows after a domain reload, but the delegates above are not
            // serialized, so a revived window has nothing to draw and closes itself.
            if (!initialized)
            {
                Close();
                return;
            }

            bool measuring = phase == Phase.Measure;

            using (new ScrollViewScope(ref scroll))
            {
                using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(false)))
                {
                    if (measuring)
                    {
                        // Transparent rather than skipped: the layout system only reports a size for
                        // content it has actually laid out.
                        using (new GUIColorScope(GUIColorScope.ColoringType.All, Color.clear))
                        {
                            (onMeasureGUI ?? onGUI)();
                        }
                    }
                    else
                    {
                        onGUI();
                    }
                }

                if (Event.current.type != EventType.Repaint)
                {
                    return;
                }

                switch (phase)
                {
                    case Phase.Measure:
                        Rect contentRect = GUILayoutUtility.GetLastRect();
                        measuredWidth = contentRect.width;
                        measuredHeight = contentRect.height;
                        phase = Phase.Resize;
                        break;

                    case Phase.Resize:
                        position = new Rect(position.x, position.y, measuredWidth, measuredHeight);
                        phase = Phase.Drawing;
                        break;
                }
            }
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}
