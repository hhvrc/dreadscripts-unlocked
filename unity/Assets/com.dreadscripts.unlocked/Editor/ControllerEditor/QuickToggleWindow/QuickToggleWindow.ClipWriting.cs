// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class QuickToggleWindow, line 4157
//   OnCustomConfirm -> OnCustomConfirm, line 4768
// Line numbers are relative to the decompiled snapshot at the time of the port; the member name is
// the durable reference.
//
// DELIBERATE DEVIATION — the body of OnCustomConfirm is NOT ported.
//
// This is the one member of the type that writes to the project, and it is the one member whose
// dependencies are all still missing. It needs, none of which exist in the package yet:
//   * ControllerEditor.RateAnnotation(bool, string)      (decompiled ControllerEditor.cs:9818) —
//     the warn-and-return-the-condition helper that both of its guard clauses are built on;
//   * EditorSettings.GetInstance().saveFolder            — the output folder setting;
//   * ControllerEditor.LogoutMapper()                    (decompiled ControllerEditor.cs:8509) —
//     the currently edited AnimatorController, whose name is a path segment;
//   * EditorUtils.InvokePredicate(TangentMode, params (float, float)[])  (EditorUtils.cs:3226) and
//     EditorUtils.FindPredicate(this AnimationClip)                      (EditorUtils.cs:3242).
//
// UtilityWindowBase<T>.OnCustomConfirm is abstract, so the override cannot simply be omitted the
// way the other deferred members were: without it this type would not compile, and a compiling
// package is the hard constraint. The override is therefore present and empty. It is NOT a
// behavioural guess — it does nothing at all, and nothing in the ported package can reach it,
// because the only thing that opens this window is the equally deferred factory. The full shipped
// behaviour is documented below so that whoever ports the remaining dependencies can restore it
// without going back to the decompile.

namespace DreadScripts.ControllerEditor
{
    internal partial class QuickToggleWindow
    {
        /// <summary>
        /// In the shipped build: writes a constant curve for every valid row of the list into the
        /// clip of every selected state, creating clips where needed. Not ported — see the file
        /// header. This override does nothing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// What the shipped implementation does, in order:
        /// </para>
        /// <para>
        /// 1. Warns "No Root Set!" and aborts if <c>root</c> is empty; every animated path is
        /// computed relative to it.
        /// </para>
        /// <para>
        /// 2. Walks the selected states. A state whose per-state choice is <em>replace</em>, or
        /// whose motion is not already an <see cref="UnityEngine.AnimationClip"/>, gets a brand new
        /// clip: <c>Undo.RecordObject(state, "Set Quick Toggle Curve")</c>, then a new clip saved to
        /// <c>{saveFolder}/Animation Clips/{controller name}/{state name}.anim</c> via
        /// <see cref="EditorUtils.PrepareAssetPath(string, string, bool)"/> with
        /// <c>makeUnique: true</c>, then <c>state.motion = clip</c> and
        /// <c>EditorUtility.SetDirty(state)</c>.
        /// </para>
        /// <para>
        /// 3. Records the distinct clips with <c>Undo.RecordObjects</c> and, for each valid row,
        /// calls <c>clip.SetCurve(AnimationUtility.CalculateTransformPath(row target, root),
        /// row.targetType, row.PropertyName, curve)</c>, where the curve is two linear-tangent keys
        /// holding the row's value: one at time 0 and one at the clip's current length (or a single
        /// frame's worth of time when the clip is still empty).
        /// </para>
        /// <para>
        /// The user-visible consequences of that, which this project records rather than fixes:
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// Clips are written as <em>standalone .anim files</em> in the save folder, never as
        /// sub-assets of the animator controller.
        /// </description></item>
        /// <item><description>
        /// No existing asset is ever overwritten: the path is made unique first, so a second run
        /// produces <c>State 1.anim</c> beside <c>State.anim</c>. Replacing a state's motion leaves
        /// the previous clip on disk, orphaned but not deleted.
        /// </description></item>
        /// <item><description>
        /// <c>Undo</c> covers the state's motion assignment and the clips' curve data, but
        /// <c>AssetDatabase.CreateAsset</c> is not undoable — undoing leaves the newly created
        /// <c>.anim</c> files in the project.
        /// </description></item>
        /// <item><description>
        /// Setting a curve for a property the clip already animates overwrites that curve, since
        /// <c>SetCurve</c> is keyed on path + type + property.
        /// </description></item>
        /// <item><description>
        /// Nothing here touches the avatar descriptor: no layer, no expression parameter and no
        /// expressions-menu control is created or modified, so none of the "avatar has no
        /// parameters/menu asset yet" cases arise. The states being filled in were created by the
        /// caller before this window opened.
        /// </description></item>
        /// <item><description>
        /// SHIPPED BUG — the blend-tree guard is inverted. The abort condition is
        /// <c>!merge &amp;&amp; motion is BlendTree</c>, so it is <em>replace</em>, not merge, that
        /// trips the warning "State X has a Blendtree motion. Can't automatically merge.", even
        /// though replace would have discarded the blend tree harmlessly. Conversely a state set to
        /// merge whose motion is a blend tree falls through to the create-a-clip branch and has its
        /// blend tree silently swapped out for a new empty clip. The abort also happens part way
        /// through the walk, so states processed before it keep their newly created clips while no
        /// curves are written to any of them.
        /// </description></item>
        /// </list>
        /// </remarks>
        internal override void OnCustomConfirm()
        {
        }
    }
}
