// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static structProperty -> windowTypeCache,      line 2160
//   static PatchRules     -> UnmaximizeAllWindows, line 5518
//   static InterruptRules -> FindWindow,           line 5530
//   static ManageRules    -> TryFindWindow,        line 5548
//   static PrintRules     -> FocusWindow,          line 5554
//   static OrderPredicate   -> SaveToPrefs,        line 3051
//   static ComparePredicate -> LoadFromPrefs,      line 3064
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// One deliberate deviation, behaviourally invisible: the decompiled InterruptRules allocates
// windowTypeCache lazily with a null check on every call, because the field had no initialiser.
// It is a field initialiser here instead; nothing can observe the difference from a static class.
//
// SaveToPrefs/LoadFromPrefs come from elsewhere in the decompiled file (lines 3051-3068) and are
// filed here because EditorWindow is the type they operate on. They serialise the *window* through
// JsonUtility, so every [SerializeField] on it round-trips in one call -- which is how the tool
// remembers a window's settings across a domain reload without writing a key per field.
// Audit status: VERIFIED against decompiled/
//
// Nothing else in the outer class body belongs to this region -- the string-numbering helpers that
// sit just above it (SortRules line 5480, RegisterRules 5497, LogoutRules 5502) and the session
// state dictionary just below it (SearchRules 5571, RevertRules 5580, OrderQueue 5593) are separate
// families and are not ported here.

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
        /// Window type names resolved so far, so that naming a window costs one assembly scan per
        /// name for the lifetime of the domain rather than one per call.
        /// </summary>
        /// <remarks>
        /// Only successful lookups are stored. A name that does not resolve is retried -- and so
        /// re-walks every loaded assembly through <see cref="FindType"/> -- on every call, which is
        /// why the callers here are all one-shot responses to a click rather than per-frame code.
        /// </remarks>
        private static readonly Dictionary<string, Type> windowTypeCache = new Dictionary<string, Type>();

        /// <summary>
        /// Un-maximizes every editor window.
        /// </summary>
        /// <remarks>
        /// A maximized window hides every other one, so anything that needs to show the user a
        /// different window -- a ping, a focus change -- has to undo the maximize first or the
        /// result is invisible.
        /// </remarks>
        internal static void UnmaximizeAllWindows()
        {
            // FindObjectsOfTypeAll rather than a Resources.FindObjectsOfType, because editor windows
            // are hidden objects and the filtered overload would not return them.
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in windows)
            {
                if (window == null)
                {
                    continue;
                }

                window.maximized = false;
            }
        }

        /// <summary>
        /// Finds an open editor window by the name of its type, or returns null when the type does
        /// not exist in this editor or no instance of it is open.
        /// </summary>
        /// <param name="typeName">
        /// A type name accepted by <see cref="FindType"/>: assembly-qualified, full, or bare.
        /// </param>
        /// <remarks>
        /// <para>
        /// Windows are addressed by name because the interesting ones are Unity's own and are
        /// internal -- <c>UnityEditor.ProjectBrowser</c>, the only name this package passes, is
        /// <c>internal sealed</c> and cannot be named in a compile-time expression. Its name has
        /// been stable across every Unity version this package supports, and only the name is
        /// depended on: no field, property or method is reflected off it, so there is nothing here
        /// for a version bump to break beyond a rename of the type itself.
        /// </para>
        /// <para>
        /// A bare name like "ProjectBrowser" is matched by <see cref="FindType"/> per assembly, and
        /// a bare-name hit in an assembly enumerated earlier beats a full-name hit in a later one.
        /// If some other loaded assembly happens to declare a type of the same name, this can
        /// therefore resolve to the wrong type -- which then simply has no instances and reads as
        /// "the window is not open". Pass a namespace-qualified name for an ambiguous window.
        /// </para>
        /// </remarks>
        internal static EditorWindow FindWindow(string typeName)
        {
            if (!windowTypeCache.ContainsKey(typeName))
            {
                Type type = FindType(typeName);
                if (type == null)
                {
                    return null;
                }

                windowTypeCache.Add(typeName, type);
            }

            // FindObjectsOfTypeAll also returns windows sitting in an unselected tab, which is the
            // point: those are exactly the ones a caller wants to bring forward.
            return Resources.FindObjectsOfTypeAll(windowTypeCache[typeName]).FirstOrDefault() as EditorWindow;
        }

        /// <summary>
        /// <see cref="FindWindow"/> in try-get form, for callers that want to branch on the miss.
        /// </summary>
        internal static bool TryFindWindow(string typeName, out EditorWindow window)
        {
            window = FindWindow(typeName);
            return window != null;
        }

        /// <summary>
        /// Brings the window of the named type forward, doing nothing if that type does not exist or
        /// no instance of it is open.
        /// </summary>
        /// <param name="restoreFocus">
        /// Focus the previously focused window again straight afterwards. The target window still
        /// ends up selected in its dock area -- visible, and able to respond to things like a ping --
        /// but keyboard focus returns to where the user left it.
        /// </param>
        /// <remarks>
        /// The <paramref name="restoreFocus"/> dance is how a ping is made to land: pinging an asset
        /// only shows if the Project browser is the active tab of its dock, so the browser is focused
        /// to select that tab and focus is then handed straight back, leaving the caller's own window
        /// active.
        /// </remarks>
        internal static void FocusWindow(string typeName, bool restoreFocus = false)
        {
            if (!TryFindWindow(typeName, out EditorWindow window))
            {
                return;
            }

            EditorWindow previouslyFocused = EditorWindow.focusedWindow;
            if (previouslyFocused == window)
            {
                return;
            }

            window.Focus();

            if (restoreFocus)
            {
                // Ported as written: the decompiled source does not null-check the previously focused
                // window, so calling this with restoreFocus while nothing is focused throws. Every
                // call site is a mouse event inside a window, where that cannot happen.
                previouslyFocused.Focus();
            }
        }
    
        /// <summary>
        /// Writes the window's serialised state to EditorPrefs (or PlayerPrefs) under
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="usePlayerPrefs">
        /// Use PlayerPrefs instead of EditorPrefs. PlayerPrefs is per-project and lives in the
        /// project folder; EditorPrefs is per-machine and shared across projects.
        /// </param>
        internal static void SaveToPrefs<T>(this T window, string key, bool usePlayerPrefs = false)
            where T : EditorWindow
        {
            string json = JsonUtility.ToJson(window, false);
            if (usePlayerPrefs)
            {
                PlayerPrefs.SetString(key, json);
            }
            else
            {
                EditorPrefs.SetString(key, json);
            }
        }

        /// <summary>
        /// Restores the window's serialised state from <paramref name="key"/>, leaving it untouched
        /// if the key is absent.
        /// </summary>
        /// <remarks>
        /// The window's *current* state is passed as the pref's default, so a missing key overwrites
        /// the window with itself. That is what makes this safe to call unconditionally on enable.
        /// </remarks>
        internal static void LoadFromPrefs<T>(this T window, string key, bool usePlayerPrefs = false)
            where T : EditorWindow
        {
            string fallback = JsonUtility.ToJson(window, false);
            string json = usePlayerPrefs
                ? PlayerPrefs.GetString(key, fallback)
                : EditorPrefs.GetString(key, fallback);
            JsonUtility.FromJsonOverwrite(json, window);
        }
    }
}
