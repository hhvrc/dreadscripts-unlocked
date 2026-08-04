// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static IncludeProcess -> HandleDragAndDrop,      line 2487
//   static RevertProcess  -> HandleMultiDragAndDrop, line 2512
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every statement below was transcribed from the region
// above, including the two closure classes named next.
//
// Four compiler-generated closure classes belong to this region and get no file of their own:
// _003C_003Ec__DisplayClass19_0<T> and _003C_003Ec__DisplayClass20_0<T> (lines 1627 and 1659) hold
// the captured filter for the two methods, and each carries a static DeleteIterator/DestroyIterator
// proxy that is nothing but UnityEngine.Object's != operator. They are dissolved back into the
// lambdas below. Each also carries a static object field paired with a "field == null" predicate
// (CancelState/PrepareState, InstantiateState/VisitState) which nothing assigns and nothing reads;
// that is protector tamper-bait, not product behaviour, and is dropped.
//
// Shared with ControllerEditor: EditorUtils.DragAndDrop.cs is the same two methods under the same
// two names. One real difference, preserved here rather than unified: ControllerEditor's port
// factors the payload projection into a ResolveDragged<T> helper and applies a null check on both
// branches, whereas this build's asset branch applies the caller's filter with no null check and
// only the component branch tests for null. The two-branch shape below is what shipped.
// Deliberately NOT consolidated, on the same basis as ADOEditorUtility.Colors.cs.
//
// VENDOR BUG, reproduced as shipped: HandleMultiDragAndDrop's asset branch ignores the caller's
// filter entirely -- it is `objectReferences.OfType<T>().ToArray()` with no Where. The component
// branch does apply it, and the single-object HandleDragAndDrop applies it on both branches, so a
// caller passing a filter gets it honoured everywhere except this one path. Both the 2022 and the
// 2019 build (line 2518) have it. Not corrected here: the correction would change behaviour, and
// the port's job is to restore what shipped.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// Makes <paramref name="rect"/> accept a dragged object of type <typeparamref name="T"/>,
        /// raising <paramref name="onDropped"/> with the first dragged object that matches.
        /// </summary>
        /// <param name="rect">The drop target, in the current GUI space.</param>
        /// <param name="onDropped">Raised once, on the frame the drop is released over the rect.</param>
        /// <param name="filter">Per-candidate predicate; null means "accept any <typeparamref name="T"/>".</param>
        /// <param name="onHovered">
        /// Raised on every frame the drag is over the rect and matches -- repeatedly during
        /// DragUpdated, not once.
        /// </param>
        /// <remarks>
        /// <see cref="Event.Use"/> is called for every DragUpdated and DragPerform over the rect,
        /// including the ones with no match, so a drop target underneath cannot also react to the
        /// same drag even while this one is refusing it. Do not reorder the statements: IMGUI's
        /// drag-and-drop is stateful, and <see cref="DragAndDrop.AcceptDrag"/> must run before the
        /// callback.
        /// </remarks>
        internal static void HandleDragAndDrop<T>(Rect rect, Action<T> onDropped, Func<T, bool> filter = null, Action onHovered = null) where T : UnityEngine.Object
        {
            Event current = Event.current;
            if ((current.type != EventType.DragPerform && current.type != EventType.DragUpdated) || !rect.Contains(current.mousePosition))
            {
                return;
            }

            T dropped;
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                dropped = DragAndDrop.objectReferences.OfType<T>().FirstOrDefault(candidate => filter?.Invoke(candidate) ?? true);
            }
            else
            {
                // A component is never dragged directly -- both the hierarchy and the project
                // browser hand over the GameObject -- so it has to be fetched off the object.
                dropped = DragAndDrop.objectReferences
                    .Select(dragged => (dragged as GameObject)?.GetComponent<T>())
                    .FirstOrDefault(candidate => candidate != null && (filter?.Invoke(candidate) ?? true));
            }

            bool accepted = dropped != null;
            if (accepted)
            {
                onHovered?.Invoke();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            if (current.type == EventType.DragPerform && accepted)
            {
                DragAndDrop.AcceptDrag();
                onDropped(dropped);
            }

            current.Use();
        }

        /// <summary>
        /// The many-object form of <see cref="HandleDragAndDrop{T}"/>: raises
        /// <paramref name="onDropped"/> once with every dragged object that matches, instead of just
        /// the first.
        /// </summary>
        /// <remarks>
        /// Identical in every other respect -- same handled event types, same visualMode and
        /// AcceptDrag placement, same unconditional <see cref="Event.Use"/>. The drag is accepted
        /// when at least one object matches; the rest are dropped from the collection silently. The
        /// result is materialised before the drop is accepted, because the payload must not be read
        /// lazily from inside the callback after AcceptDrag has consumed it.
        /// </remarks>
        internal static void HandleMultiDragAndDrop<T>(Rect rect, Action<IEnumerable<T>> onDropped, Func<T, bool> filter = null, Action onHovered = null) where T : UnityEngine.Object
        {
            Event current = Event.current;
            if ((current.type != EventType.DragPerform && current.type != EventType.DragUpdated) || !rect.Contains(current.mousePosition))
            {
                return;
            }

            T[] dropped;
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                dropped = DragAndDrop.objectReferences.OfType<T>().ToArray();
            }
            else
            {
                dropped = DragAndDrop.objectReferences
                    .Select(dragged => (dragged as GameObject)?.GetComponent<T>())
                    .Where(candidate => candidate != null && (filter?.Invoke(candidate) ?? true))
                    .ToArray();
            }

            bool accepted = dropped.Length != 0;
            if (accepted)
            {
                onHovered?.Invoke();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

            if (current.type == EventType.DragPerform && accepted)
            {
                DragAndDrop.AcceptDrag();
                onDropped(dropped);
            }

            current.Use();
        }
    }
}
