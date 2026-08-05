// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ReorderableListHelper.cs
//
// The [SpecialName] Index()/Index(int) pair was a property before the obfuscator split it, and is
// one again here. The unused static pair CustomizeStruct/SearchStruct is deliberately not ported:
// the field is never assigned and the method never called anywhere in the assembly, which makes
// both obfuscator decoys rather than part of the type.
//
// DEOBF-BUG
// DrawHeaderButtons has no recoverable ordering: de4dot leaves the FlexibleSpace stranded in an
// empty `while (true)` after the two buttons, with the whole body under an inverted
// `if (!drawFlexibleSpace)`. The order written here is inferred from the call sites. See the
// remarks on the method.
//
// Audit status: PARTIAL -- the nine fields, the Index accessor pair (a property again here), both
// constructors, DrawElement, Draw, ClampIndex and DrawTitle were diffed against export/ and match
// statement for statement, including DrawElement's inverted remove-button branch and Draw's
// selection-change check. DrawHeaderButtons is NOT verified: its statements are all present and
// individually match, but their order is reconstructed from the call sites rather than read off
// export/, for the reason above.

using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A <see cref="ReorderableList"/> preconfigured the way every list in the tool is drawn: a
    /// custom header row with a collapse toggle and an add button, a remove button on each element,
    /// and no list chrome of Unity's own.
    /// </summary>
    /// <typeparam name="T">Element type of the backing list, passed to <see cref="ReorderableList"/>.</typeparam>
    internal class ReorderableListHelper<T>
    {
        internal readonly IList list;

        private readonly ReorderableList reorderableList;

        private object lastSelected;

        /// <summary>Draws one element, minus the strip on the right taken by the remove button.</summary>
        private readonly ReorderableList.ElementCallbackDelegate drawElement;

        private readonly Action drawHeader;

        /// <summary>Raised when the selected element changes, including to and from nothing selected.</summary>
        internal Action onSelectionChanged;

        internal bool expanded = true;

        /// <summary>Whether to draw the (empty) list body when there is nothing in it.</summary>
        internal bool drawWhenEmpty;

        /// <summary>
        /// The selected index, clamped to the list on every read and write so that removing elements
        /// cannot leave it dangling.
        /// </summary>
        internal int Index
        {
            get
            {
                // Writes the clamped value back rather than only returning it, so the underlying
                // list stays in agreement with what callers were told.
                return reorderableList.index = ClampIndex(reorderableList.index);
            }
            set
            {
                reorderableList.index = ClampIndex(value);
            }
        }

        internal ReorderableListHelper(Action drawHeader, IList list, Action<ReorderableList> onAdd,
                                       ReorderableList.ElementCallbackDelegate drawElement,
                                       ReorderableList.ElementHeightCallbackDelegate elementHeight = null)
        {
            this.drawHeader = drawHeader;
            this.list = list;
            this.drawElement = drawElement;

            // Header, add and remove are all drawn by this class instead, so Unity's own are off and
            // the header strip is collapsed to the minimum height the list will accept.
            reorderableList = new ReorderableList(list, typeof(T), draggable: true, displayHeader: false,
                                                  displayAddButton: false, displayRemoveButton: false)
            {
                headerHeight = 1f,
                footerHeight = 0f,
                drawElementCallback = DrawElement,
                onAddCallback = onAdd.Invoke
            };

            if (elementHeight != null)
            {
                reorderableList.elementHeightCallback = elementHeight;
            }
        }

        /// <summary>
        /// Builds a list with the standard header: a bold title, an optional help icon, and the
        /// collapse and add buttons.
        /// </summary>
        internal ReorderableListHelper(string title, string tooltip, IList list, Action<ReorderableList> onAdd,
                                       ReorderableList.ElementCallbackDelegate drawElement,
                                       ReorderableList.ElementHeightCallbackDelegate elementHeight = null)
            : this((Action)null, list, onAdd, drawElement, elementHeight)
        {
            drawHeader = delegate
            {
                DrawTitle(title, tooltip);
                DrawHeaderButtons();
            };
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // The list can be mutated by the element drawers themselves, so an index handed over by
            // ReorderableList is not necessarily still in range by the time it is drawn.
            if (list.Count == 0 || index < 0 || index >= list.Count)
            {
                return;
            }

            Rect removeRect = new Rect(rect.x + rect.width - 28f, rect.y + rect.height / 2f - 8f, 32f, 18f);
            if (GUI.Button(removeRect, EditorUtils.contents.removeSelection, EditorUtils.styles.footerButton))
            {
                // Removed rather than drawn this frame: the element is gone, and drawing it after
                // the removal would be reading past the end of the list.
                list.RemoveAt(index);
                return;
            }

            Rect elementRect = new Rect(rect) { width = rect.width - 29f };
            drawElement(elementRect, index, isActive, isFocused);
        }

        internal void Draw()
        {
            bool isEmpty = list.Count == 0;

            if (onSelectionChanged != null)
            {
                object selected = isEmpty ? null : list[Index];
                if (selected != lastSelected)
                {
                    lastSelected = selected;
                    onSelectionChanged();
                }
            }

            if (drawHeader != null)
            {
                using (new EditorGUILayout.HorizontalScope("RL Header"))
                {
                    drawHeader();
                }
            }

            if (expanded && (!isEmpty || drawWhenEmpty))
            {
                reorderableList.DoLayoutList();
            }
        }

        internal int ClampIndex(int index)
        {
            return Mathf.Clamp(index, 0, list.Count - 1);
        }

        /// <summary>Draws the list's title, followed by a help icon carrying <paramref name="tooltip"/>.</summary>
        internal void DrawTitle(string title, string tooltip = null)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(tooltip))
            {
                GUILayout.Label(new GUIContent(EditorUtils.contents.help.texture, tooltip),
                                GUILayout.Width(14f), GUILayout.Height(18f));
            }
        }

        /// <summary>
        /// Draws the right-hand side of the header: the collapse toggle and the add button, pushed
        /// against the right edge.
        /// </summary>
        /// <param name="drawVisibilityToggle">
        /// False where the caller wants the list permanently expanded.
        /// </param>
        /// <param name="drawFlexibleSpace">
        /// False where the caller has already drawn its own controls into the header and inserted the
        /// flexible space itself.
        /// </param>
        /// <remarks>
        /// de4dot could not recover the ordering here — the shipped build decompiles to the flexible
        /// space stranded in an empty infinite loop after the two buttons. The order below is the one
        /// the call sites imply: they pass <paramref name="drawFlexibleSpace"/> false and emit
        /// <see cref="GUILayout.FlexibleSpace"/> themselves before their own buttons, which only
        /// works if the space would otherwise have come first.
        /// </remarks>
        internal void DrawHeaderButtons(bool drawVisibilityToggle = true, bool drawFlexibleSpace = true)
        {
            if (drawFlexibleSpace)
            {
                GUILayout.FlexibleSpace();
            }

            if (drawVisibilityToggle)
            {
                expanded = EditorUtils.ToggleButton(expanded,
                                                    expanded ? EditorUtils.contents.visible : EditorUtils.contents.hidden,
                                                    EditorStyles.label,
                                                    GUILayout.Width(18f), GUILayout.Height(18f));
            }

            // Adding to a collapsed list would put the new element out of sight.
            using (new EditorGUI.DisabledScope(!expanded))
            {
                if (EditorUtils.Button(EditorGUIUtility.IconContent("d_ol_plus"), GUI.skin.label, GUILayout.Width(18f)))
                {
                    reorderableList.onAddCallback(reorderableList);
                }
            }
        }
    }
}
