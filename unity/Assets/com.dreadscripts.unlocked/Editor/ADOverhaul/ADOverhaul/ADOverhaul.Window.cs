// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// This file ports no member of the decompiled source. It exists only to bridge the two calls
// ADOverhaulWindow makes into the outer ADOverhaul class, which the shipped build could write
// unqualified because the window was a private nested type:
//
//   OnEnable -> PrintConfiguration(ref m_Predicate, ref, line 162
//                                                  _Collection, LogoutConfiguration)
//   DrawEasyDynamicsGUI -> PushConfiguration(), line 89
//
// Both call targets are ported -- RefreshSceneAvatars in ADOverhaul.AvatarSelection.cs and
// RefreshAvatarTables in ADOverhaul.Lifecycle.cs, DrawTargetAvatarSelector in
// ADOverhaul.AvatarSelection.cs -- but two of the three, along with the `selectedAvatar` and
// `sceneAvatars` fields the first pair of arguments name, are private to the class. The window was
// lifted out of ADOverhaul to a top-level type (see the note in ADOverhaul.State.cs), so it can no
// longer reach them. Rather than widen those members' visibility -- which would advertise them to
// every other ported region -- the two call sites are reproduced here verbatim, inside the class
// that owns them, and the window calls these.
//
// These two methods are therefore an artifact of the reconstruction and not of the original. They
// add no behaviour: each is exactly the expression the decompiled window inlined at its call site.

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Re-reads the open scenes' avatar descriptors and, if no avatar was selected yet, picks
        /// one and rebuilds the tables derived from it. Run when the tool window is enabled.
        /// </summary>
        /// <remarks>
        /// SHIPPED BUG, REACHED FROM HERE. <see cref="RefreshAvatarTables"/> dereferences
        /// <see cref="selectedAvatar"/> unconditionally, and <see cref="RefreshSceneAvatars"/>
        /// invokes it whenever the avatar was null on entry -- which includes the case where the
        /// scene contains no descriptor at all and none was assigned. Opening the window in a scene
        /// with no VRChat avatar therefore throws a <see cref="System.NullReferenceException"/> out
        /// of the window's <c>OnEnable</c>, exactly as the shipped 2022 build does; Unity logs it and
        /// carries on, so the window still opens and still draws. The 2019 build returns early
        /// instead and does not throw. See the remarks on <see cref="RefreshAvatarTables"/>.
        /// </remarks>
        internal static void WindowRefreshAvatarSelection()
        {
            RefreshSceneAvatars(ref selectedAvatar, ref sceneAvatars, RefreshAvatarTables);
        }

        /// <summary>
        /// The "Target Avatar" row, drawn for the window's Easy Dynamics pane.
        /// </summary>
        internal static void WindowDrawTargetAvatarSelector()
        {
            DrawTargetAvatarSelector();
        }
    }
}
