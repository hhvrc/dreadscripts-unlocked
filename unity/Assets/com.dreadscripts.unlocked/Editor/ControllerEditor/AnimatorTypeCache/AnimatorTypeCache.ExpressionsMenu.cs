// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTypeCache.cs
//   ExpressionsMenuBinding                 -> ExpressionsMenuBinding, line 371
//     controls                             -> controls,              line 373
//     GetControl(int)                      -> this[int],             line 382
//   MenuControlBinding                     -> MenuControlBinding,    line 388
//     GetName / SetName                    -> Name,                  line 390
//     GetParameterName / SetParameterName  -> ParameterName,         line 402
//     GetIcon / SetIcon                    -> Icon,                  line 414
//     GetSubmenu / SetSubmenu              -> Submenu,               line 426
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The accessors above are marked [SpecialName] in the shipped assembly, and ExpressionsMenuBinding
// carries [DefaultMember("Item")]: they are properties and an indexer that the obfuscator stripped
// back to methods, restored here.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorTypeCache
    {
        /// <summary>
        /// A VRChat expressions menu asset, indexed by control slot.
        /// </summary>
        /// <remarks>
        /// Reaches the asset's contents through <see cref="SerializedObject"/> rather than through
        /// the SDK's own menu type, which the tool cannot reference — see
        /// <see cref="AnimatorTypeCache"/>.
        /// </remarks>
        internal class ExpressionsMenuBinding : SerializedObjectWrapper
        {
            internal readonly SerializedPropertyWrapper controls;

            internal ExpressionsMenuBinding(Object menuAsset)
                : base(menuAsset)
            {
                controls = FindProperty("controls");
            }

            public MenuControlBinding this[int index]
            {
                get
                {
                    return new MenuControlBinding(controls[index]);
                }
            }
        }

        /// <summary>
        /// One control within an expressions menu: its label, the parameter it drives, its icon, and
        /// the submenu it opens.
        /// </summary>
        /// <remarks>
        /// Nothing here applies the owning serialized object — the caller decides when a batch of
        /// menu edits is written back.
        /// </remarks>
        internal class MenuControlBinding : SerializedPropertyWrapper
        {
            public MenuControlBinding(SerializedProperty controlProperty)
                : base(controlProperty)
            {
            }

            /// <summary>The label shown on the control.</summary>
            internal string Name
            {
                get
                {
                    return this["name"].property.stringValue;
                }
                set
                {
                    this["name"].property.stringValue = value;
                }
            }

            /// <summary>The avatar parameter the control drives.</summary>
            internal string ParameterName
            {
                get
                {
                    return this["parameter"]["name"].property.stringValue;
                }
                set
                {
                    this["parameter"]["name"].property.stringValue = value;
                }
            }

            internal Texture2D Icon
            {
                get
                {
                    return (Texture2D)this["icon"].property.objectReferenceValue;
                }
                set
                {
                    this["icon"].property.objectReferenceValue = value;
                }
            }

            /// <summary>
            /// The menu this control opens. The getter wraps whatever the reference holds without
            /// checking it, so it is only meaningful on a control whose type is Submenu.
            /// </summary>
            internal ExpressionsMenuBinding Submenu
            {
                get
                {
                    return new ExpressionsMenuBinding(this["submenu"].property.objectReferenceValue);
                }
                set
                {
                    this["submenu"].property.objectReferenceValue = value.targetObject;
                }
            }
        }
    }
}
