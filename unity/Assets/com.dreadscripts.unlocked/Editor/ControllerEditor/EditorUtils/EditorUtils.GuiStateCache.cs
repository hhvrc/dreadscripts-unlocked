// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static serviceProperty -> guiStateCache,        line 2162
//   static SearchRules     -> SetGuiState,          line 5571
//   static RevertRules     -> GetGuiState(string),  line 5580
//   static OrderQueue      -> GetGuiState<T>,       line 5593
//   static CompareQueue    -> SetGuiStateOnEvent(key, value, eventType),        line 5603
//   static SetQueue        -> SetGuiStateOnEvent(key, value, eventType, actual), line 5608
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The file name is a misnomer inherited from the porting assignment: despite it, nothing here
// touches UnityEditor.SessionState. The store is a plain static Dictionary<string, object> that
// lives and dies with the domain, so it does NOT survive a recompile or an editor restart, no key
// leaves the process, and no value is ever serialised. The genuine SessionState caches in this
// package -- EditorGuiUtils.SaveTextureToSession / LoadTextureFromSession and CachedTextureContent
// -- solve a different problem (keeping decoded image bytes across domain reloads by widening them
// to an int array) and this family neither duplicates nor replaces them.
//
// One deliberate deviation, behaviourally invisible: the decompiled methods allocate the dictionary
// lazily and null-check it on every read, because the field had no initialiser. It is a field
// initialiser here instead, matching EditorUtils.Windows.cs; nothing can observe the difference
// from a static class.
// Audit status: VERIFIED against decompiled/

using System.Collections.Generic;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Values carried from one IMGUI event to the next, keyed by a caller-built string.
        /// </summary>
        /// <remarks>
        /// <para>
        /// IMGUI runs a window's drawing code once per event, and the measurements a layout needs
        /// -- <see cref="GUILayoutUtility.GetLastRect"/> and friends -- are only meaningful during
        /// Repaint. A control that must be sized from something it can only measure then has to
        /// write the measurement down on the Repaint pass and read it back on the Layout pass,
        /// which is exactly what this dictionary is for. It is a one-frame relay, not a cache, even
        /// though entries happen to persist until overwritten.
        /// </para>
        /// <para>
        /// Keys are composed entirely by the callers and this class imposes no scheme, so two
        /// windows drawing the same kind of control must disambiguate their keys themselves. The
        /// call sites do it by appending an identity to a fixed prefix: an index for a repeated
        /// row, <c>Object.GetInstanceID()</c> for a per-object one. Nothing is ever removed, so a
        /// key built from something transient leaks one boxed value for the lifetime of the domain
        /// -- bounded in practice because the identities repeat frame to frame.
        /// </para>
        /// </remarks>
        private static readonly Dictionary<string, object> guiStateCache = new Dictionary<string, object>();

        /// <summary>
        /// Stores <paramref name="value"/> under <paramref name="key"/>, replacing anything already
        /// there.
        /// </summary>
        internal static void SetGuiState(string key, object value)
        {
            guiStateCache[key] = value;
        }

        /// <summary>
        /// Returns the value stored under <paramref name="key"/>, or null when there is none.
        /// </summary>
        internal static object GetGuiState(string key)
        {
            if (guiStateCache.TryGetValue(key, out object value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Returns the value stored under <paramref name="key"/>, or <paramref name="fallback"/>
        /// when there is none.
        /// </summary>
        /// <remarks>
        /// The fallback covers the passes before the first Repaint has run, where the caller wants
        /// a harmless placeholder -- a zero height, <see cref="Rect.zero"/> -- rather than a null
        /// check at every use. A key whose stored value is not a <typeparamref name="T"/> throws
        /// on the cast instead of falling back, so a key reused for two different value types is a
        /// hard error rather than a silent default.
        /// </remarks>
        internal static T GetGuiState<T>(string key, T fallback)
        {
            if (guiStateCache.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return fallback;
        }

        /// <summary>
        /// Stores <paramref name="value"/> only while the current event is of type
        /// <paramref name="eventType"/>, and does nothing otherwise.
        /// </summary>
        /// <remarks>
        /// Callers pass <see cref="EventType.Repaint"/>: the measurement being recorded is only
        /// valid on that pass, and writing the garbage read on a Layout pass would overwrite the
        /// good value from the previous frame.
        /// </remarks>
        internal static void SetGuiStateOnEvent(string key, object value, EventType eventType)
        {
            SetGuiStateOnEvent(key, value, eventType, Event.current.type);
        }

        /// <summary>
        /// <see cref="SetGuiStateOnEvent(string, object, EventType)"/> with the event to test
        /// supplied explicitly, for code that has already captured it or is not running inside an
        /// event where <see cref="Event.current"/> is set.
        /// </summary>
        internal static void SetGuiStateOnEvent(string key, object value, EventType eventType, EventType currentEventType)
        {
            if (currentEventType != eventType)
            {
                return;
            }

            SetGuiState(key, value);
        }
    }
}
