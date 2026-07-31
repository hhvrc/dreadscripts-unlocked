// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/SelectionPool.cs

using System;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A fixed-size set of toggles of which at most one may be on, raising
    /// <see cref="onSelectionChanged"/> whenever the selection moves.
    /// </summary>
    /// <remarks>
    /// The same shape as ADOverhaul's <c>ExclusiveSelectionState</c>, with the change notification
    /// added. The two were not shared between the products and are kept apart here for that reason.
    /// </remarks>
    internal sealed class SelectionPool
    {
        private bool[] selections;

        /// <summary>Index of the selected entry, or -1 when nothing is selected.</summary>
        internal int selectedIndex = -1;

        internal Action onSelectionChanged;

        internal SelectionPool(int count, Action onSelectionChanged = null)
        {
            Resize(count);
            this.onSelectionChanged = onSelectionChanged;
        }

        /// <summary>
        /// Grows or shrinks the set, keeping the current selection where it still fits and dropping
        /// it otherwise.
        /// </summary>
        internal void Resize(int count)
        {
            if (selections == null || selections.Length != count)
            {
                selections = new bool[count];
            }

            // The shipped build tested selectedIndex > 0, so a selection on the first entry was lost
            // on every resize: selectedIndex stayed 0 while the new array had it off.
            if (selectedIndex >= 0)
            {
                if (selectedIndex < selections.Length)
                {
                    selections[selectedIndex] = true;
                }
                else
                {
                    selectedIndex = -1;
                }
            }
        }

        /// <summary>Selects <paramref name="index"/>, deselecting whatever was selected before.</summary>
        internal void Select(int index)
        {
            if (index < 0 || index >= selections.Length || selectedIndex == index)
            {
                return;
            }

            if (selectedIndex >= 0)
            {
                selections[selectedIndex] = false;
            }

            selectedIndex = index;
            selections[index] = true;
            onSelectionChanged?.Invoke();
        }

        /// <summary>Selects or deselects <paramref name="index"/>.</summary>
        internal void SetSelected(int index, bool selected)
        {
            if (index < 0 || index >= selections.Length)
            {
                return;
            }

            if (selected)
            {
                Select(index);
            }
            else if (selectedIndex == index)
            {
                ClearSelection();
            }
        }

        /// <summary>Deselects everything.</summary>
        internal void ClearSelection()
        {
            if (selectedIndex >= 0)
            {
                selections[selectedIndex] = false;
                selectedIndex = -1;
                onSelectionChanged?.Invoke();
            }
        }
    }
}
