// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/SearchablePickerPopup.cs
//   PickerEntry.FirstExtra -> PickerEntry.firstExtra, line 63
// The decompiled type also carries two always-null statics with a "is it still null" method each
// (PickerEntry.MapSystem/AddSystem and WriteSystem/RemoveSystem). Nothing reads either, in this
// assembly or the other; they are obfuscator scaffolding and are deliberately not ported.
//
// Depends on two EditorUtils helpers that are not ported yet, referenced here under the names they
// are expected to take: EditorUtils.ColorTexture (decompiled ReflectList, a 1x1 point-filtered
// texture of a colour) and EditorUtils.Separator (decompiled MapQueue, the thin horizontal rule).

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A dropdown list of arbitrary items with an optional search box, where the caller draws each
    /// row itself. Used wherever a plain <see cref="GenericMenu"/> would be too plain — property and
    /// type pickers that want icons, two-line rows or a filter.
    /// </summary>
    /// <typeparam name="T">The item type being picked.</typeparam>
    /// <remarks>
    /// Rows are drawn by the caller's delegate, so this class never knows how tall or wide a row is
    /// until it has drawn one. Everything awkward here follows from that: the row's clickable area
    /// and the window's width both come from rects measured on a previous frame, which is what
    /// <see cref="isFirstFrame"/> tracks.
    /// </remarks>
    internal class SearchablePickerPopup<T> : PopupWindowContent
    {
        /// <summary>One row: the item, where it came from, and whatever the caller attached to it.</summary>
        internal class PickerEntry
        {
            /// <summary>Position in the list the popup was constructed from, kept through sorting.</summary>
            internal readonly int index;

            internal readonly T value;

            /// <summary>
            /// Anything the row-drawing delegate needs that is not derivable from
            /// <see cref="value"/> — icons, display names — filled in by
            /// <see cref="SetExtraData"/>.
            /// </summary>
            /// <remarks>
            /// Computed once up front rather than in the drawing delegate because that delegate runs
            /// for every visible row on every repaint, and the pickers use it for things like icon
            /// lookups that are far too slow to repeat at that rate.
            /// </remarks>
            internal object[] extraData;

            /// <summary>False while the current search string filters this row out.</summary>
            internal bool isVisible = true;

            /// <summary>Shorthand for the common case of a single piece of extra data.</summary>
            internal object firstExtra => extraData[0];

            internal PickerEntry(T value, int index)
            {
                this.value = value;
                this.index = index;
            }
        }

        private readonly string title;

        private string searchString;

        internal PickerEntry[] entries;

        private readonly Action<PickerEntry> drawEntry;

        private readonly Action<int, T> onSelected;

        private Func<T, string, bool> searchFilter;

        private bool hasSearch;

        /// <summary>Widest row measured on the first frame; the window is sized to it afterwards.</summary>
        private float maxWidth;

        private bool isFirstFrame = true;

        private Vector2 scrollPosition;

        /// <summary>
        /// Where each row was drawn last frame, used as its clickable area this frame. Index-parallel
        /// to <see cref="entries"/> as it stood at construction, so it must not be resized by sorting.
        /// </summary>
        private readonly Rect[] entryRects;

        /// <summary>
        /// A style with nothing but hover and pressed backgrounds, so the click target over a row
        /// shows selection feedback without drawing anything over the row itself.
        /// </summary>
        internal readonly GUIStyle entryStyle = new GUIStyle
        {
            hover = { background = EditorUtils.ColorTexture(new Color(0.302f, 0.302f, 0.302f)) },
            active = { background = EditorUtils.ColorTexture(new Color(0.1725f, 0.3647f, 0.5294f)) }
        };

        /// <param name="title">Heading drawn above the list; pass empty for none.</param>
        /// <param name="items">The items to offer.</param>
        /// <param name="drawEntry">Draws one row. Called for every visible row, every repaint.</param>
        /// <param name="onSelected">Receives the picked item and its original index.</param>
        public SearchablePickerPopup(string title, IEnumerable<T> items, Action<PickerEntry> drawEntry, Action<int, T> onSelected)
        {
            this.title = title;
            this.onSelected = onSelected;
            this.drawEntry = drawEntry;
            entries = items.Select((item, i) => new PickerEntry(item, i)).ToArray();
            entryRects = new Rect[entries.Length];
        }

        /// <summary>
        /// Adds the search box, filtering rows with <paramref name="searchFilter"/> (item, search
        /// string) as the user types.
        /// </summary>
        public void EnableSearch(Func<T, string, bool> searchFilter)
        {
            hasSearch = true;
            this.searchFilter = searchFilter;
        }

        /// <summary>
        /// Reorders the rows by a key. Each entry keeps the index it was constructed with, so
        /// <see cref="onSelected"/> still reports the caller's own ordering.
        /// </summary>
        public void SortBy(Func<T, object> keySelector)
        {
            entries = keySelector == null ? entries : entries.OrderBy(entry => keySelector(entry.value)).ToArray();
        }

        /// <summary>Fills in <see cref="PickerEntry.extraData"/> for every row.</summary>
        public void SetExtraData(Func<T, object[]> selector)
        {
            foreach (PickerEntry entry in entries)
            {
                entry.extraData = selector(entry.value);
            }
        }

        /// <summary>Opens the popup below <paramref name="activatorRect"/>.</summary>
        public void Show(Rect activatorRect)
        {
            PopupWindow.Show(activatorRect, this);
        }

        public override void OnGUI(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                Event current = Event.current;
                using (new ScrollViewScope(ref scrollPosition))
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        GUILayout.Label(title, EditorUtils.styles.centeredBoldRichLabel);
                        EditorUtils.Separator();
                    }

                    if (hasSearch)
                    {
                        EditorGUI.BeginChangeCheck();
                        if (isFirstFrame)
                        {
                            // Named only while the name is needed: the focus below is by name, but
                            // once focus lands IMGUI tracks it by control id, which stays stable
                            // across frames as long as the layout does.
                            GUI.SetNextControlName(title + "SearchBar");
                        }

                        searchString = EditorGUILayout.TextField(searchString, GUI.skin.GetStyle("SearchTextField"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (PickerEntry entry in entries)
                            {
                                entry.isVisible = searchFilter(entry.value, searchString);
                            }
                        }
                    }

                    EventType eventType = current.type;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        PickerEntry entry = entries[i];
                        if (!entry.isVisible)
                        {
                            continue;
                        }

                        // The click target is a fixed-rect button over where this row was drawn last
                        // frame, laid down before the row so the row's own content draws on top of
                        // the highlight. On the first frame no rect has been measured yet, so the
                        // list is not clickable until it has been drawn once.
                        if (!isFirstFrame && GUI.Button(entryRects[i], string.Empty, entryStyle))
                        {
                            onSelected(entry.index, entry.value);
                            editorWindow.Close();
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            drawEntry(entry);
                        }

                        if (eventType == EventType.Repaint)
                        {
                            entryRects[i] = GUILayoutUtility.GetLastRect();
                            if (isFirstFrame && entryRects[i].width > maxWidth)
                            {
                                maxWidth = entryRects[i].width;
                            }
                        }
                    }

                    if (eventType == EventType.Repaint && isFirstFrame)
                    {
                        isFirstFrame = false;

                        // Focus is claimed after the first full pass, not during it: the control has
                        // to exist before it can be focused by name. From here the user can type
                        // straight into the filter without clicking the field, and Escape reaches
                        // the host popup window, which closes itself.
                        GUI.FocusControl(title + "SearchBar");
                    }
                }

                // PopupWindow does not repaint on mouse movement, so the hover highlight would only
                // update when something else forced a frame.
                if (rect.Contains(current.mousePosition))
                {
                    editorWindow.Repaint();
                }
            }
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 windowSize = base.GetWindowSize();
            if (!isFirstFrame)
            {
                // Widest row plus room for the vertical scrollbar. Height is left at the default, so
                // the first frame is drawn at the default size and the window snaps to its real
                // width immediately afterwards.
                windowSize.x = maxWidth + 21f;
            }

            return windowSize;
        }
    }
}
