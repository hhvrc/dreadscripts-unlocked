// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static m_CollectionProperty -> sharedContent, line 2116
//   static CreateResolver       -> TempContent,   line 2812
//   static DeleteResolver       -> TempContent,   line 2807
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// DeleteResolver's entire body is `param.CreateResolver("", tokenneeded)`, so the two collapse into
// one method the way EditorUtils.Buttons.cs collapses its obfuscator-split overloads. A call site
// written against DeleteResolver becomes TempContent(text, copy: flag) -- note the argument must be
// named, since it is the third parameter here and was the second there.
//
// m_CollectionProperty is `internal` in the decompiled class but has no caller outside EditorUtils
// itself, so it is private here; that also keeps a later wave from colliding with the name.
//
// PushResolver (line 2842), the text-width measurement helper, is the third member of this region
// and is not ported here: it belongs with the layout/measurement helpers, and has since landed in
// EditorUtils.GuiContent.cs as GetTextWidth. It is the clearest example of the safe use of
// TempContent below -- it passes the content straight to GUIStyle.CalcSize and lets it go.
// Audit status: VERIFIED -- the field initialiser and TempContent's four statements diffed against
// export/ CreateResolver, and DeleteResolver confirmed to be the single forward the collapse note
// describes. The only deviation is visibility: sharedContent is internal in the decompilation and
// private here, as the header records.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// The single <see cref="GUIContent"/> instance <see cref="TempContent"/> hands out.
        /// </summary>
        private static readonly GUIContent sharedContent = new GUIContent();

        /// <summary>
        /// Wraps <paramref name="text"/> and <paramref name="tooltip"/> in a
        /// <see cref="GUIContent"/> for immediate use, without allocating one.
        /// </summary>
        /// <param name="copy">
        /// Returns a private copy instead of the shared instance. Required whenever the content
        /// outlives the call -- see the remarks.
        /// </param>
        /// <remarks>
        /// <para>
        /// This exists because IMGUI redraws several times per frame and a label built with
        /// <c>new GUIContent(...)</c> inside <c>OnGUI</c> is garbage on the next line. Reusing one
        /// instance for every transient label removes that allocation entirely, which is why the
        /// shipped tool routes most of its inline labels through here. Unity's own editor code does
        /// the same thing with its internal <c>EditorGUIUtility.TempContent</c>, hence the name.
        /// </para>
        /// <para>
        /// ALIASING HAZARD. Every caller that does not pass <paramref name="copy"/> gets a reference
        /// to the *same* object, and the next call overwrites its text and tooltip. The result is
        /// only safe when the content is consumed before control returns to the caller -- handed
        /// straight to a draw or measure call and then forgotten. Storing it, putting it in an array,
        /// or holding it across another call to this method yields a label that silently changes to
        /// whatever was requested most recently. The failure is not a crash; it is a UI where several
        /// labels read the same text, which is easy to mistake for a data bug. Two shipped patterns
        /// are the ones to watch:
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// <see cref="UnityEditor.GenericMenu"/> entries. A menu keeps its <see cref="GUIContent"/>
        /// and draws it later, so every menu item built from this must ask for a copy -- which is
        /// exactly what the shipped code does at those call sites, and the reason the flag exists.
        /// </description></item>
        /// <item><description>
        /// Nested composition, e.g. building a content for a field whose drawing code itself calls
        /// this method. The inner call wins and the outer label is lost.
        /// </description></item>
        /// </list>
        /// <para>
        /// The instance is not thread-safe either, but IMGUI is main-thread-only, so that is
        /// theoretical.
        /// </para>
        /// </remarks>
        internal static GUIContent TempContent(this string text, string tooltip = "", bool copy = false)
        {
            sharedContent.text = text;
            sharedContent.tooltip = tooltip;

            if (!copy)
            {
                return sharedContent;
            }

            return new GUIContent(sharedContent);
        }
    }
}
