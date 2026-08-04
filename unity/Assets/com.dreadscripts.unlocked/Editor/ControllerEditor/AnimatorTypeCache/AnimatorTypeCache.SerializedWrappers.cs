// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/AnimatorTypeCache.cs
//   SerializedObjectWrapper              -> SerializedObjectWrapper,   line 445
//     FindProperty(string)               -> FindProperty(string),      line 453
//   SerializedPropertyWrapper            -> SerializedPropertyWrapper, line 460
//     Item(int)                          -> this[int],                 line 470
//     Item(string)                       -> this[string],              line 476
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Both types carry [DefaultMember("Item")] and their accessors are marked [SpecialName] in the
// shipped assembly: they were indexers that the obfuscator stripped back to methods named Item, and
// they are restored as indexers here.
// Audit status: VERIFIED against decompiled/ member-by-member (2026-08-04).

using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorTypeCache
    {
        /// <summary>
        /// A <see cref="SerializedObject"/> whose <see cref="FindProperty"/> hands back a
        /// <see cref="SerializedPropertyWrapper"/>, so that a path into an SDK asset can be written as
        /// a chain of indexers instead of nested FindPropertyRelative calls.
        /// </summary>
        internal class SerializedObjectWrapper : SerializedObject
        {
            internal SerializedObjectWrapper(Object target)
                : base(target)
            {
            }

            public new SerializedPropertyWrapper FindProperty(string propertyPath)
            {
                return new SerializedPropertyWrapper(base.FindProperty(propertyPath));
            }
        }

        /// <summary>
        /// A <see cref="SerializedProperty"/> that indexes into itself: by integer for an array
        /// element, by name for a child field.
        /// </summary>
        /// <remarks>
        /// The SDK's data types cannot be referenced directly (see <see cref="AnimatorTypeCache"/>),
        /// so navigating one means walking its serialized fields by name. Indexers make that walk
        /// read like the field access it stands in for — <c>menu["controls"][0]["name"]</c> — and the
        /// implicit conversion lets the result be passed straight to any API that wants a plain
        /// <see cref="SerializedProperty"/>.
        /// </remarks>
        internal class SerializedPropertyWrapper
        {
            internal readonly SerializedProperty property;

            public SerializedPropertyWrapper(SerializedProperty property)
            {
                this.property = property;
            }

            public SerializedPropertyWrapper this[int index]
            {
                get
                {
                    return new SerializedPropertyWrapper(property.GetArrayElementAtIndex(index));
                }
            }

            public SerializedPropertyWrapper this[string relativePath]
            {
                get
                {
                    return new SerializedPropertyWrapper(property.FindPropertyRelative(relativePath));
                }
            }

            public static implicit operator SerializedProperty(SerializedPropertyWrapper wrapper)
            {
                return wrapper.property;
            }
        }
    }
}
