// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PushResolver         -> GetTextWidth(string, GUIStyle),    line 2842
//   static SetupQueue           -> GetTextWidth(string, GUIStyle),    line 5621
//   static PostQueue            -> GetTextWidth(Enum, GUIStyle),      line 5616
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/ -- both GetTextWidth overloads, the only members this
// file declares, were compared statement by statement against reverse-engineering/export/ (EditorUtils.cs:
// PushResolver 2842 and SetupQueue 5621, whose bodies are identical to each other and to the
// collapsed extension below, and PostQueue 5616). All three cited lines still land on the members
// named above, and the header claims no member the file does not declare.
//
// PushResolver (an extension on string) and SetupQueue (a plain static taking a string) have
// identical bodies and cannot both exist here -- as C# members they would have the same signature.
// They are collapsed into the single extension method below; both decompiled names map to it.
//
// DELIBERATE DEVIATION
// PostQueue is a plain static in reverse-engineering/export/ and is ported as an extension on Enum, to match the
// string overload it delegates to. Its body is otherwise transcribed unchanged.
//
// NOTES
// TempContent, whose sharing rules the two measurements below depend on, is no longer declared
// here: the merge of the parallel ports left it in EditorUtils.SharedContent.cs (decompiled
// CreateResolver, line 2812), which documents that the returned GUIContent is one process-wide
// mutable instance, valid only until the next call, and that copy: true is needed wherever it has
// to outlive the call. Both calls below pass it straight into a CalcSize, which is the safe use.

using System;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {

        /// <summary>
        /// How wide <paramref name="text"/> would be drawn in <paramref name="style"/>, or in the
        /// skin's label style when none is given.
        /// </summary>
        internal static float GetTextWidth(this string text, GUIStyle style = null)
        {
            if (style == null)
            {
                style = GUI.skin.label;
            }

            return style.CalcSize(text.TempContent()).x;
        }

        /// <summary>
        /// How wide the enum value's name would be drawn -- for sizing a popup to its widest
        /// entry.
        /// </summary>
        internal static float GetTextWidth(this Enum value, GUIStyle style = null)
        {
            return value.ToString().GetTextWidth(style);
        }
    }
}
