// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static field threadProcessor       -> customEditorRefsResolved,   line 2208
//   static field m_PolicyProcessor     -> customEditorAttributesType,  line 2210
//   static field m_SerializerProcessor -> monoEditorTypeType,          line 2212
//   static field _PageProcessor        -> customMultiEditorsField,     line 2214
//   static field resolverProcessor     -> inspectorTypeField,          line 2216
//   static field _PredicateProcessor   -> inspectorWindowType,         line 2218
//   static field _RulesProcessor       -> refreshInspectorsMethod,     line 2220
//   static field queueProcessor        -> genericMenuItemsField,       line 2222
//   static field errorProcessor        -> genericMenuItemsFieldResolved, line 2224
//   static CallList   -> OverrideCustomEditor, line 6730
//   static CancelList -> RefreshInspectors,    line 6745
//   static CountList  -> GetMenuItems,         line 6755
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Three pokes at UnityEditor internals that have no public equivalent at all. Each is a genuine
// capability the tool needs and Unity does not offer, so there is no "do it properly" alternative
// to point at -- only a version risk, which is the same in each case: the member is looked up by
// name and the call throws a NullReferenceException on an editor where it has moved.
//
// Internal Unity members bound by this file, all on UnityEditor.dll:
//   type   UnityEditor.CustomEditorAttributes
//   field  CustomEditorAttributes.kSCustomMultiEditors   -- IDictionary: inspected type -> IList of
//                                                          MonoEditorType
//   type   UnityEditor.CustomEditorAttributes+MonoEditorType
//   field  MonoEditorType.m_InspectorType                -- the editor class used for that type
//   type   UnityEditor.InspectorWindow
//   method InspectorWindow.RefreshInspectors()           -- static, private
//   field  UnityEngine.GenericMenu.menuItems             -- named m_MenuItems on older editors
//
// None of these were verified against a live Unity install during this port; they are transcribed
// from the decompiled source as-is. The GenericMenu one is the exception in shape rather than in
// confidence: the vendor tries both spellings, which is itself evidence the field was renamed
// between the versions the tool supported.

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Whether the custom-editor reflection below has been looked up yet.</summary>
        private static bool customEditorRefsResolved;

        private static Type customEditorAttributesType;

        private static Type monoEditorTypeType;

        /// <summary>
        /// CustomEditorAttributes.kSCustomMultiEditors: the editor registry, keyed by inspected
        /// type.
        /// </summary>
        private static FieldInfo customMultiEditorsField;

        /// <summary>MonoEditorType.m_InspectorType: which editor class a registry entry names.</summary>
        private static FieldInfo inspectorTypeField;

        private static Type inspectorWindowType;

        private static MethodInfo refreshInspectorsMethod;

        /// <summary>GenericMenu's backing list of items.</summary>
        private static FieldInfo genericMenuItemsField;

        /// <summary>
        /// Whether <see cref="genericMenuItemsField"/> has been looked up yet, so a failure is
        /// cached rather than retried on every call.
        /// </summary>
        private static bool genericMenuItemsFieldResolved;

        /// <summary>
        /// Replaces the editor Unity uses for <paramref name="inspectedType"/> with
        /// <paramref name="editorType"/>, for the rest of the session, and refreshes any open
        /// inspector.
        /// </summary>
        /// <remarks>
        /// This edits Unity's own registry rather than declaring a [CustomEditor], which is the
        /// point: it can take over a type whose editor is defined in another package, and it can be
        /// undone by writing the original class back. The registry is rebuilt on domain reload, so
        /// the override does not persist.
        /// <para>
        /// Only the first registered editor for the type is replaced, and the call throws if the
        /// type has no entry at all -- i.e. if it is drawn by the default inspector.
        /// </para>
        /// </remarks>
        internal static void OverrideCustomEditor(Type inspectedType, Type editorType)
        {
            if (!customEditorRefsResolved)
            {
                customEditorRefsResolved = true;
                customEditorAttributesType = RequireType(
                    "UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                monoEditorTypeType = RequireType(
                    "UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                customMultiEditorsField = customEditorAttributesType.GetAnyField("kSCustomMultiEditors");
                inspectorTypeField = monoEditorTypeType.GetAnyField("m_InspectorType");
            }

            IList entries = (IList)((IDictionary)customMultiEditorsField.GetValue(null))[inspectedType];
            inspectorTypeField.SetValue(entries[0], editorType);
            RefreshInspectors();
        }

        /// <summary>
        /// Makes every open inspector rebuild its editors -- the only way to make an editor swap
        /// visible without the user reselecting.
        /// </summary>
        internal static void RefreshInspectors()
        {
            if (inspectorWindowType == null)
            {
                inspectorWindowType = RequireType(
                    "UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                refreshInspectorsMethod = inspectorWindowType.GetMethod("RefreshInspectors",
                    BindingFlags.Static | BindingFlags.NonPublic);
            }

            refreshInspectorsMethod.Invoke(null, null);
        }

        /// <summary>
        /// The menu's items, as the untyped list GenericMenu keeps internally, or null on an editor
        /// where the field cannot be found.
        /// </summary>
        /// <remarks>
        /// GenericMenu is write-only in its public API -- items can be added but not counted or
        /// read back -- so this is how the tool knows whether a menu it has built is empty before
        /// showing it. The elements are GenericMenu.MenuItem, which is private, so only Count is
        /// usable without further reflection.
        /// </remarks>
        internal static IList GetMenuItems(this GenericMenu menu)
        {
            if (!genericMenuItemsFieldResolved)
            {
                genericMenuItemsFieldResolved = true;
                genericMenuItemsField = typeof(GenericMenu).GetAnyField("menuItems")
                                        ?? typeof(GenericMenu).GetAnyField("m_MenuItems");
            }

            return genericMenuItemsField == null ? null : (IList)genericMenuItemsField.GetValue(menu);
        }
    }
}
