// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ExclusiveSelectionState.cs
//
// The whole file is one type; every member is ported and keeps its decompiled name, except the
// backing array `toggles`, which becomes `_toggles`.
//
// DELIBERATE DEVIATION
//
// Resize tests `activeIndex >= 0` here where both shipped builds test `activeIndex > 0`. With the
// shipped test a selection on the first toggle was silently lost on every resize: activeIndex stayed
// 0 while the freshly allocated array had that entry off, so the set reported a selection the toggle
// row did not draw. This is the one place the port does not reproduce shipped behaviour; it is fixed
// rather than preserved because the caller (PhysBoneEditor's tool modes) resizes only on construction
// and the stale state is unreachable from the UI, so preserving it would encode a defect no user can
// observe. Recorded here rather than left in a code comment alone.
//
// NOTES
//
// Two shape changes, neither behavioural. Select() calls Clear() instead of inlining
// `if (activeIndex >= 0) _toggles[activeIndex] = false;` -- Clear also writes activeIndex = -1, which
// the next statement overwrites. SetSelected() is restructured from the decompiled straight-line form
// (`if (activeIndex == i) { if (selected) return; Clear(); } if (activeIndex >= 0 && selected)
// _toggles[activeIndex] = false; if (selected) activeIndex = i; _toggles[i] = selected;`) into the
// two-branch form below; both agree on every input, given that only activeIndex is ever true.
//
// Audit status: VERIFIED -- the whole type diffed member by member against the 2022 snapshot: the
// field, activeIndex's -1 seed, the constructor, Resize, Select, SetSelected and Clear. The only
// behavioural difference is the Resize bound recorded under DELIBERATE DEVIATION above, which was
// documented only in a code comment before this pass; the two restructurings under NOTES were traced
// case by case and are equivalent.

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
        private bool[] _toggles;

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
            if (_toggles == null || _toggles.Length != count)
            {
                _toggles = new bool[count];
            }

            // The shipped build tested activeIndex > 0 here, so a selection on the first toggle was
            // silently lost every time the set was resized: activeIndex stayed 0 while the freshly
            // allocated array had it off.
            if (activeIndex >= 0)
            {
                if (activeIndex >= _toggles.Length)
                {
                    activeIndex = -1;
                }
                else
                {
                    _toggles[activeIndex] = true;
                }
            }
        }

        /// <summary>Selects <paramref name="index"/>, deselecting whatever was selected before.</summary>
        internal void Select(int index)
        {
            if (index < 0 || index >= _toggles.Length || activeIndex == index)
            {
                return;
            }

            Clear();
            activeIndex = index;
            _toggles[index] = true;
        }

        /// <summary>Selects or deselects <paramref name="index"/>.</summary>
        internal void SetSelected(int index, bool selected)
        {
            if (index < 0 || index >= _toggles.Length)
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
                _toggles[activeIndex] = false;
                activeIndex = -1;
            }
        }
    }
}
