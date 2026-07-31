// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type. Reconstructed from both, which differ only in obfuscated parameter names:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/GUIColorScope.cs
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/GUIColorScope.cs

using System;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Temporarily overrides one or more of Unity's global GUI tint colours, restoring the previous
    /// values on dispose.
    /// </summary>
    /// <remarks>
    /// Several constructors apply a colour only conditionally. When none is applied the scope is a
    /// no-op and <see cref="Dispose"/> must not restore anything — otherwise it would clobber a
    /// colour set by an enclosing scope with whatever happened to be current at construction.
    /// <see cref="captured"/> tracks that.
    /// </remarks>
    internal sealed class GUIColorScope : IDisposable
    {
        [Flags]
        internal enum ColoringType
        {
            /// <summary><see cref="GUI.backgroundColor"/>.</summary>
            BG = 1,
            /// <summary><see cref="GUI.contentColor"/>.</summary>
            FG = 2,
            /// <summary><see cref="GUI.color"/>.</summary>
            General = 4,
            All = BG | FG | General
        }

        private readonly ColoringType channels;

        private bool captured;
        private Color previousBackground;
        private Color previousContent;
        private Color previousGeneral;

        /// <summary>Always applies <paramref name="color"/>.</summary>
        internal GUIColorScope(ColoringType channels, Color color)
        {
            this.channels = channels;
            Apply(color);
        }

        /// <summary>Applies <paramref name="color"/> only when <paramref name="active"/>; otherwise leaves the GUI colours alone.</summary>
        internal GUIColorScope(ColoringType channels, bool active, Color color)
        {
            this.channels = channels;
            if (active)
            {
                Apply(color);
            }
        }

        /// <summary>Applies one of two colours, picked by <paramref name="condition"/>.</summary>
        internal GUIColorScope(ColoringType channels, bool condition, Color ifTrue, Color ifFalse)
        {
            this.channels = channels;
            Apply(condition ? ifTrue : ifFalse);
        }

        /// <summary>
        /// Applies <c>colors[index]</c>, or nothing when <paramref name="index"/> is negative — the
        /// convention ADOverhaul uses for "no state to highlight".
        /// </summary>
        internal GUIColorScope(ColoringType channels, int index, params Color[] colors)
        {
            this.channels = channels;
            if (index >= 0)
            {
                Apply(colors[index]);
            }
        }

        private void Apply(Color color)
        {
            captured = true;
            previousBackground = GUI.backgroundColor;
            previousContent = GUI.contentColor;
            previousGeneral = GUI.color;

            if (channels.HasFlag(ColoringType.BG))
            {
                GUI.backgroundColor = color;
            }

            if (channels.HasFlag(ColoringType.FG))
            {
                GUI.contentColor = color;
            }

            if (channels.HasFlag(ColoringType.General))
            {
                GUI.color = color;
            }
        }

        public void Dispose()
        {
            if (!captured)
            {
                return;
            }

            if (channels.HasFlag(ColoringType.BG))
            {
                GUI.backgroundColor = previousBackground;
            }

            if (channels.HasFlag(ColoringType.FG))
            {
                GUI.contentColor = previousContent;
            }

            if (channels.HasFlag(ColoringType.General))
            {
                GUI.color = previousGeneral;
            }
        }
    }
}
