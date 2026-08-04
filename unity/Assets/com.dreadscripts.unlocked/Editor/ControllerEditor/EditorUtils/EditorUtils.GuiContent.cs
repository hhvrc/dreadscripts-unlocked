// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static PushResolver         -> GetTextWidth(string, GUIStyle),    line 2842
//   static SetupQueue           -> GetTextWidth(string, GUIStyle),    line 5621
//   static PostQueue            -> GetTextWidth(Enum, GUIStyle),      line 5616
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// PushResolver (an extension on string) and SetupQueue (a plain static taking a string) have
// identical bodies and cannot both exist here -- as C# members they would have the same signature.
// They are collapsed into the single extension method below; both decompiled names map to it.
//
// THE SCRATCH CONTENT IS SHARED AND MUTABLE. TempContent hands back one process-wide GUIContent
// with its text and tooltip overwritten, which is what Unity's own internal EditorGUIUtility
// .TempContent does and why it costs no allocation in a per-frame OnGUI. The consequence is that
// the returned reference is only valid until the next call: it must be passed straight to a draw
// call and never stored, never put in an array, and never used twice in one expression. Pass
// copy: true where it has to outlive the call -- that is what every use in a GUIContent[] does.

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
