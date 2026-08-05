// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/SelectionPool.cs
//
// DELIBERATE DEVIATION
// Two places where this knowingly differs from the shipped build:
//   * Resize tests `selectedIndex >= 0` where the shipped build tests `selectedIndex > 0`, so a
//     selection on entry 0 is no longer silently dropped on every resize. See the body comment.
//   * Select and SetSelected raise onSelectionChanged AFTER writing selections[index], where the
//     shipped build raises it in between -- so a handler that reads the pool back sees the new
//     selection here and saw it still unset before. Nothing in either assembly reads the pool from
//     inside the handler, so no shipped behaviour turns on it, but it is a real ordering change
//     and not a transcription.
//
// NOTES
// SetSelected's shipped body is a flattened four-way branch on (selectedIndex == index, selected)
// that collapses to the Select/ClearSelection delegation written here; its trailing unconditional
// `selections[index] = selected` is dead in the one case not covered, because only the selected
// entry is ever true.
//
// Audit status: VERIFIED -- diffed in full against export/. The three fields, the constructor,
// Resize, Select, SetSelected and ClearSelection were each compared statement for statement,
// including tracing SetSelected's flattened form through all four input cases. The two
// differences found are recorded above rather than left implicit.

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
