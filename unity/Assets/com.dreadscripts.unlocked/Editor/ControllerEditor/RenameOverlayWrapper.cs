// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/RenameOverlayWrapper.cs
//
// The decompiled type held its bindings in a static field per member and bound them all in one
// eager EnsureInitialized() pass built out of EditorUtils.FillRules / Type.RestartList. Here the
// same bindings are expressed as TypeResolver + ReflectionMemberRef fields, which is what the rest
// of the port uses for internal-API lookups; the mapping is one field to one field:
//   renameOverlayType        -> renameOverlayType     (TypeResolver)
//   beginRenameMethod        -> beginRenameMethod
//   endRenameMethod          -> endRenameMethod
//   isRenamingMethod         -> isRenamingMethod
//   onGUIMethod              -> onGUIMethod
//   onEventMethod            -> onEventMethod
//   clearMethod              -> clearMethod
//   editFieldRectField       -> editFieldRectField
//   userAcceptedRenameField  -> userAcceptedRenameField
//   originalNameField        -> originalNameField
//   nameField                -> nameField
//   userDataField            -> userDataField
//   isWaitingForDelayField   -> isWaitingForDelayField
// The [SpecialName] accessor pairs (Instance/EditFieldRect/UserAcceptedRename/IsRenaming/UserData/
// IsWaitingForDelay/Name/OriginalName) were properties before the obfuscator split them into
// get/set methods, and are properties again here.
//
// DELIBERATE DEVIATION
// The rewrite above changes when binding happens and how it fails. Shipped: one eager
// EnsureInitialized() guarded by a static `initialized` flag binds all thirteen members on the
// first construction, and a failure is logged and rethrown as whatever reflection threw. Here:
// EnsureInitialized() resolves only the type, each member binds lazily on first use, and a missing
// type is logged identically but rethrown as a new Exception naming it. No call site inspects the
// exception, and the log line is unchanged, so the observable difference is confined to which
// exception type escapes a Unity version that has dropped UnityEditor.RenameOverlay.
//
// Audit status: PARTIAL -- every member was matched one-to-one against export/ and checked for
// behavioural equivalence: all thirteen bindings (same member names, same GUIStyle disambiguation
// on OnGUI), the three constructors (including that the Func<object> one deliberately resolves
// nothing), ResolveInstance, the eight accessor pairs, both BeginRename overloads -- with the rect
// still written after BeginRename, not before -- EndRename, OnGUI, OnEvent and Clear. Not marked
// VERIFIED because this file re-expresses rather than transcribes: the eager static-FieldInfo
// binding was rewritten onto TypeResolver/ReflectionMemberRef, so no statement-level diff against
// export/ is possible, and the rewrite inherits TypeResolver.ResolvedType, whose own body is
// unverifiable (see TypeResolver.cs).

using System;
using System.Reflection;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Drives Unity's internal <c>UnityEditor.RenameOverlay</c> — the in-place text field the editor
    /// shows when an item in a tree view or the project browser is renamed.
    /// </summary>
    /// <remarks>
    /// The overlay is internal to UnityEditor with no public equivalent, so every member is reached
    /// by reflection. Two things are done with it: a wrapper is built around the animator window's
    /// own overlay (constructed from a <see cref="Func{T}"/> so the window is only queried when the
    /// overlay is actually used), and a second, freshly-constructed overlay is driven directly to
    /// rename things the animator window does not know how to rename.
    /// <para>
    /// Members bound, all of them internal API and none of them versioned:
    /// <list type="bullet">
    /// <item><c>bool BeginRename(string name, int userData, float delay)</c></item>
    /// <item><c>void EndRename(bool acceptChanges)</c></item>
    /// <item><c>bool IsRenaming()</c></item>
    /// <item><c>bool OnGUI(GUIStyle textFieldStyle)</c> — the no-argument overload also exists, hence
    /// the explicit parameter type on the lookup</item>
    /// <item><c>bool OnEvent()</c></item>
    /// <item><c>void Clear()</c></item>
    /// <item><c>Rect m_EditFieldRect</c>, <c>bool m_UserAcceptedRename</c>, <c>string m_OriginalName</c>,
    /// <c>string m_Name</c>, <c>int m_UserData</c>, <c>bool m_IsWaitingForDelay</c></item>
    /// </list>
    /// That shape holds across the editor versions the two shipped builds target, 2019.4 through
    /// 2022.3, and has been stable since Unity 5. It is not a supported API and nothing guarantees
    /// it in a later version, which is why a missing type is reported as a clear error rather than
    /// left to surface as a null dereference somewhere in the middle of a repaint.
    /// </para>
    /// </remarks>
    internal class RenameOverlayWrapper
    {
        private const string RenameOverlayTypeName =
            "UnityEditor.RenameOverlay, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private static readonly TypeResolver renameOverlayType = new TypeResolver(RenameOverlayTypeName);

        private static readonly ReflectionMemberRef<MethodInfo> beginRenameMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "BeginRename");

        private static readonly ReflectionMemberRef<MethodInfo> endRenameMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "EndRename");

        private static readonly ReflectionMemberRef<MethodInfo> isRenamingMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "IsRenaming");

        private static readonly ReflectionMemberRef<MethodInfo> onGUIMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "OnGUI", typeof(GUIStyle));

        private static readonly ReflectionMemberRef<MethodInfo> onEventMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "OnEvent");

        private static readonly ReflectionMemberRef<MethodInfo> clearMethod =
            new ReflectionMemberRef<MethodInfo>(renameOverlayType, "Clear");

        private static readonly ReflectionMemberRef<FieldInfo> editFieldRectField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_EditFieldRect");

        private static readonly ReflectionMemberRef<FieldInfo> userAcceptedRenameField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_UserAcceptedRename");

        private static readonly ReflectionMemberRef<FieldInfo> originalNameField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_OriginalName");

        private static readonly ReflectionMemberRef<FieldInfo> nameField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_Name");

        private static readonly ReflectionMemberRef<FieldInfo> userDataField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_UserData");

        private static readonly ReflectionMemberRef<FieldInfo> isWaitingForDelayField =
            new ReflectionMemberRef<FieldInfo>(renameOverlayType, "m_IsWaitingForDelay");

        private object instance;

        private bool instanceResolved;

        private readonly Func<object> instanceGetter;

        /// <summary>Raised after a rename ends, with the accepted/cancelled flag it ended on.</summary>
        internal Action<bool> onEndRename;

        /// <summary>
        /// The RenameOverlay type, or a logged error and a throw if this editor version does not
        /// have it.
        /// </summary>
        private static Type EnsureInitialized()
        {
            Type type = renameOverlayType.ResolvedType;
            if (type == null)
            {
                Debug.LogError("Rename Overlay Wrapper has failed to initialize!");
                throw new Exception("Type \"" + RenameOverlayTypeName + "\" not found.");
            }

            return type;
        }

        /// <summary>Wraps a newly constructed overlay of its own.</summary>
        internal RenameOverlayWrapper()
        {
            Instance = Activator.CreateInstance(EnsureInitialized());
        }

        /// <summary>Wraps an existing overlay instance.</summary>
        internal RenameOverlayWrapper(object value)
        {
            EnsureInitialized();
            Instance = value;
        }

        /// <summary>
        /// Wraps whatever overlay <paramref name="instanceGetter"/> returns, fetched once on first
        /// use.
        /// </summary>
        /// <remarks>
        /// Unlike the other two constructors this one resolves nothing: the getter typically reaches
        /// into an editor window that may not exist yet, so both it and the reflection lookups are
        /// deferred until something actually asks for the instance.
        /// </remarks>
        internal RenameOverlayWrapper(Func<object> instanceGetter)
        {
            this.instanceGetter = instanceGetter;
        }

        /// <summary>
        /// Re-runs the getter, replacing whatever instance was resolved before. Marks the instance
        /// resolved even when the getter is absent or returns null, so a null result is not retried.
        /// </summary>
        internal object ResolveInstance()
        {
            EnsureInitialized();
            instance = instanceGetter?.Invoke();
            instanceResolved = true;
            return instance;
        }

        /// <summary>The wrapped overlay, resolved through the getter on first access if need be.</summary>
        internal object Instance
        {
            get
            {
                if (instance != null || instanceResolved)
                {
                    return instance;
                }

                ResolveInstance();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>Where the text field is drawn.</summary>
        internal Rect EditFieldRect
        {
            get
            {
                return (Rect)editFieldRectField.Member.GetValue(Instance);
            }
            set
            {
                editFieldRectField.Member.SetValue(Instance, value);
            }
        }

        /// <summary>True when the rename that just ended was confirmed rather than cancelled.</summary>
        internal bool UserAcceptedRename
        {
            get
            {
                return (bool)userAcceptedRenameField.Member.GetValue(Instance);
            }
        }

        internal bool IsRenaming
        {
            get
            {
                return (bool)isRenamingMethod.Member.Invoke(Instance, null);
            }
        }

        /// <summary>
        /// Caller-defined payload carried through the rename, used to identify what is being renamed
        /// when the overlay reports back.
        /// </summary>
        internal int UserData
        {
            get
            {
                return (int)userDataField.Member.GetValue(Instance);
            }
            set
            {
                userDataField.Member.SetValue(Instance, value);
            }
        }

        /// <summary>
        /// True while the overlay is sitting out the click-to-rename delay it was started with.
        /// </summary>
        internal bool IsWaitingForDelay
        {
            get
            {
                return (bool)isWaitingForDelayField.Member.GetValue(Instance);
            }
            set
            {
                isWaitingForDelayField.Member.SetValue(Instance, value);
            }
        }

        /// <summary>The text currently in the field.</summary>
        internal string Name
        {
            get
            {
                return (string)nameField.Member.GetValue(Instance);
            }
            set
            {
                nameField.Member.SetValue(Instance, value);
            }
        }

        /// <summary>The name the rename started from, for restoring it when the user cancels.</summary>
        internal string OriginalName
        {
            get
            {
                return (string)originalNameField.Member.GetValue(Instance);
            }
        }

        /// <summary>Starts a rename and places the text field at <paramref name="editFieldRect"/>.</summary>
        /// <remarks>
        /// The rect is written after BeginRename, not before: BeginRename computes a rect of its own,
        /// so setting it first would be overwritten.
        /// </remarks>
        internal bool BeginRename(Rect editFieldRect, string name, int userData, float delay)
        {
            bool result = BeginRename(name, userData, delay);
            EditFieldRect = editFieldRect;
            return result;
        }

        /// <summary>
        /// Starts a rename of <paramref name="name"/>, opening the field after
        /// <paramref name="delay"/> seconds.
        /// </summary>
        internal bool BeginRename(string name, int userData, float delay)
        {
            return (bool)beginRenameMethod.Member.Invoke(Instance, new object[] { name, userData, delay });
        }

        /// <summary>
        /// Ends the rename in progress, if any, accepting the new name when
        /// <paramref name="acceptChanges"/> is true.
        /// </summary>
        /// <param name="clear">
        /// Whether to reset the overlay afterwards. Left on except where the caller still needs to
        /// read the finished rename's name and user data out of it.
        /// </param>
        internal void EndRename(bool acceptChanges, bool clear = true)
        {
            if (!IsRenaming)
            {
                return;
            }

            endRenameMethod.Member.Invoke(Instance, new object[] { acceptChanges });
            onEndRename?.Invoke(acceptChanges);

            if (clear)
            {
                Clear();
            }
        }

        /// <summary>Draws the overlay. Returns false once the rename has ended.</summary>
        internal bool OnGUI(GUIStyle textFieldStyle = null)
        {
            return (bool)onGUIMethod.Member.Invoke(Instance, new object[] { textFieldStyle });
        }

        /// <summary>Lets the overlay consume the current event. Returns false once the rename has ended.</summary>
        internal bool OnEvent()
        {
            return (bool)onEventMethod.Member.Invoke(Instance, null);
        }

        /// <summary>Resets the overlay to its idle state.</summary>
        internal void Clear()
        {
            clearMethod.Member.Invoke(Instance, null);
        }
    }
}
