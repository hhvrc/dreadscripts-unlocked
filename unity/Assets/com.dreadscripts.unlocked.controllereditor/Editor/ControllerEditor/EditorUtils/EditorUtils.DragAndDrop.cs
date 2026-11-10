// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static InstantiateRules -> HandleDragAndDrop,      line 4817
//   static AwakeRules       -> HandleMultiDragAndDrop, line 4842
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// This is the whole drag-and-drop family in the decompiled EditorUtils -- the two methods sit
// adjacent to each other and nothing else in the file touches UnityEditor.DragAndDrop. The single
// and the multi variant are near-identical apart from picking the first match versus collecting
// all of them; they are kept as two methods here because the decompiled source has two, and
// because collapsing them into overloads would make every call site with a lambda ambiguous.
//
// Parameter renames, applied to both methods (the decompiled names are obfuscator residue):
//   ident / value    -> rect        the area that accepts the drop
//   ivk / ord        -> onDropped   raised once, on DragPerform, with the accepted object(s)
//   control / filter -> filter      per-candidate predicate; null means "accept any T"
//   ident2 / asset2  -> onHovered   raised on every frame the drag is over the rect and matches,
//                                   i.e. repeatedly during DragUpdated, not once
// Audit status: VERIFIED against reverse-engineering/export/
//
// IMGUI drag-and-drop is stateful and order-sensitive; see the remarks on HandleDragAndDrop for
// the accept/Use contract these two rely on. Do not reorder the statements.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Makes <paramref name="rect"/> accept a dragged object of type <typeparamref name="T"/>,
        /// raising <paramref name="onDropped"/> with the first dragged object that matches. Used to
        /// let an asset be dropped straight onto a field instead of picked through the object
        /// picker.
        /// </summary>
        /// <param name="rect">The drop target, in the current GUI space.</param>
        /// <param name="onDropped">Raised once, on the frame the drop is released over the rect.</param>
        /// <param name="filter">
        /// Optional extra acceptance test applied to each candidate after the type check. A
        /// candidate it rejects is skipped, not fatal -- the next dragged object is tried.
        /// </param>
        /// <param name="onHovered">
        /// Optional, raised on every frame an acceptable drag hovers the rect. For repainting or
        /// highlighting; because it fires per <see cref="EventType.DragUpdated"/> rather than once
        /// on entry, it must be cheap and idempotent.
        /// </param>
        /// <remarks>
        /// <para>
        /// Only <see cref="EventType.DragUpdated"/> and <see cref="EventType.DragPerform"/> are
        /// handled, and only while the cursor is inside the rect.
        /// <see cref="EventType.DragExited"/> is deliberately not handled: the method keeps no state
        /// of its own between frames, so there is nothing to unwind when the drag leaves. A drag
        /// that enters the rect and is then released outside it never produces a DragPerform here,
        /// so <paramref name="onDropped"/> is not raised and Unity tears the drag down itself.
        /// </para>
        /// <para>
        /// The accept/consume pairing is the fragile part.
        /// <see cref="DragAndDrop.visualMode"/> is set to <see cref="DragAndDropVisualMode.Copy"/>
        /// only when a match was found -- Unity resets visualMode to
        /// <see cref="DragAndDropVisualMode.None"/> at the start of each DragUpdated, so leaving it
        /// alone is exactly what shows the "rejected" cursor for a drag of the wrong type.
        /// <see cref="DragAndDrop.AcceptDrag"/> is called only on DragPerform and only when there is
        /// a match, never on DragUpdated: calling it while merely hovering would end Unity's drag
        /// while the mouse is still down and leave the drag state stuck for the rest of the session.
        /// </para>
        /// <para>
        /// <see cref="Event.Use"/>, by contrast, is called for every DragUpdated and DragPerform
        /// over the rect, including the ones with no match. That is intentional: the rect claims the
        /// event so an enclosing drop target underneath it cannot also react to the same drag, even
        /// when this one is refusing it.
        /// </para>
        /// </remarks>
        internal static void HandleDragAndDrop<T>(Rect rect, Action<T> onDropped, Func<T, bool> filter = null, Action onHovered = null) where T : UnityEngine.Object
        {
            Event current = Event.current;
            if ((current.type != EventType.DragPerform && current.type != EventType.DragUpdated) || !rect.Contains(current.mousePosition))
            {
                return;
            }

            // Objects of the wrong type are skipped rather than rejecting the whole drag, so a
            // multi-object drag is accepted as long as one of its objects matches -- but only that
            // first match is delivered. Use HandleMultiDragAndDrop to receive all of them.
            T dropped = ResolveDragged<T>().FirstOrDefault(candidate => candidate != null && (filter?.Invoke(candidate) ?? true));

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
        /// AcceptDrag placement, same unconditional <see cref="Event.Use"/>. See
        /// <see cref="HandleDragAndDrop{T}"/> for why that ordering matters. The drag is accepted
        /// when at least one object matches; the non-matching ones are dropped from the collection
        /// silently.
        /// </remarks>
        internal static void HandleMultiDragAndDrop<T>(Rect rect, Action<IEnumerable<T>> onDropped, Func<T, bool> filter = null, Action onHovered = null) where T : UnityEngine.Object
        {
            Event current = Event.current;
            if ((current.type != EventType.DragPerform && current.type != EventType.DragUpdated) || !rect.Contains(current.mousePosition))
            {
                return;
            }

            // Enumerated eagerly: the array is measured before the drop is accepted, and the
            // DragAndDrop payload must not be read lazily from inside the callback, after
            // AcceptDrag has already consumed it.
            T[] dropped = ResolveDragged<T>().Where(candidate => candidate != null && (filter?.Invoke(candidate) ?? true)).ToArray();

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

        /// <summary>
        /// Projects the current drag payload into candidates of type <typeparamref name="T"/>,
        /// before any caller-supplied filter. Entries that cannot yield a <typeparamref name="T"/>
        /// are dropped, so the result may be shorter than
        /// <see cref="DragAndDrop.objectReferences"/> -- or empty.
        /// </summary>
        /// <remarks>
        /// Components need their own path because a component is never dragged directly: the Unity
        /// hierarchy and the project browser both hand over the <see cref="GameObject"/>, so the
        /// component has to be fetched off it. Note that the test is
        /// <see cref="Type.IsSubclassOf(Type)"/>, which is false for <see cref="Component"/> itself,
        /// so a <c>T</c> of exactly <c>Component</c> takes the plain asset path and matches nothing
        /// dragged out of a scene. That is the decompiled behaviour, preserved as-is.
        /// </remarks>
        private static IEnumerable<T> ResolveDragged<T>() where T : UnityEngine.Object
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
            {
                return DragAndDrop.objectReferences.OfType<T>();
            }

            return DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object dragged)
            {
                // The null-conditional is a plain reference check, not Unity's overloaded
                // comparison; a dragged object is either a GameObject or it is not, and a destroyed
                // one would not be in the payload.
                GameObject gameObject = dragged as GameObject;
                return gameObject?.GetComponent<T>();
            });
        }
    }
}
