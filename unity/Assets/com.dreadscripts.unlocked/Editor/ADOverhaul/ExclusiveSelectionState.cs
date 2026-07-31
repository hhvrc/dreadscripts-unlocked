// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ExclusiveSelectionState.cs

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// A fixed-size set of toggles of which at most one may be on at a time — the backing state for
    /// the radio-button style toolbars in the PhysBone inspector.
    /// </summary>
    /// <remarks>
    /// The toggle array exists alongside <see cref="activeIndex"/> because the GUI draws each toggle
    /// independently and needs a <c>bool</c> per row; <see cref="activeIndex"/> is what makes the
    /// selection exclusive.
    /// </remarks>
    internal sealed class ExclusiveSelectionState
    {
        private bool[] toggles;

        /// <summary>Index of the selected toggle, or -1 when nothing is selected.</summary>
        internal int activeIndex = -1;

        internal ExclusiveSelectionState(int count)
        {
            Resize(count);
        }

        /// <summary>
        /// Grows or shrinks the set, preserving the current selection where it still fits. A
        /// selection that falls outside the new range is dropped.
        /// </summary>
        internal void Resize(int count)
        {
            if (toggles == null || toggles.Length != count)
            {
                toggles = new bool[count];
            }

            // The shipped build tested activeIndex > 0 here, so a selection on the first toggle was
            // silently lost every time the set was resized: activeIndex stayed 0 while the freshly
            // allocated array had it off.
            if (activeIndex >= 0)
            {
                if (activeIndex >= toggles.Length)
                {
                    activeIndex = -1;
                }
                else
                {
                    toggles[activeIndex] = true;
                }
            }
        }

        /// <summary>Selects <paramref name="index"/>, deselecting whatever was selected before.</summary>
        internal void Select(int index)
        {
            if (index < 0 || index >= toggles.Length || activeIndex == index)
            {
                return;
            }

            Clear();
            activeIndex = index;
            toggles[index] = true;
        }

        /// <summary>Selects or deselects <paramref name="index"/>.</summary>
        internal void SetSelected(int index, bool selected)
        {
            if (index < 0 || index >= toggles.Length)
            {
                return;
            }

            if (selected)
            {
                Select(index);
            }
            else if (activeIndex == index)
            {
                Clear();
            }
        }

        /// <summary>Deselects everything.</summary>
        internal void Clear()
        {
            if (activeIndex >= 0)
            {
                toggles[activeIndex] = false;
                activeIndex = -1;
            }
        }
    }
}
