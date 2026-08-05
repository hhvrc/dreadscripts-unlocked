// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   nested class QuickToggleWindow, line 4157
//   OnCustomConfirm -> OnCustomConfirm, line 4768
// Line numbers are relative to the decompiled snapshot at the time of the port; the member name is
// the durable reference.
//
// DELIBERATE DEVIATION — the body of OnCustomConfirm is NOT ported.
//
// This is the one member of the type that writes to the project. Its dependency list was
// re-derived on 2026-08-05 and is now down to a single missing member; the old claim that they
// were "all still missing" is out of date.
//
//   STILL MISSING:
//   * ControllerEditor.RateAnnotation(bool, string)      (decompiled ControllerEditor.cs:9818) —
//     the warn-and-return-the-condition helper that both of its guard clauses are built on.
//     Nothing in the package claims line 9818. Note for whoever ports it: it is NOT the same
//     member as ControllerEditor.LogWarning (line 10876), which IS ported and is `internal`.
//     The two have identical bodies — `Log(message, CustomLogType.Warning, condition)` — and
//     differ only in parameter ORDER, RateAnnotation taking (bool, string) so it reads as a guard.
//     They are two distinct shipped members; 9818 belongs to ControllerEditor.Logging.cs's region,
//     not to this file's, so it must be ported there rather than substituted for here.
//
//   SINCE LANDED, all `internal` and reachable from this type:
//   * EditorSettings.saveFolder — EditorSettings.Fields.cs line 414 (StringSetting).
//   * ControllerEditor.LogoutMapper() (8509) is now ControllerEditor.ActiveController,
//     ControllerEditor.ControllerContext.cs line 133. `internal static`, so unlike the deferrals in
//     ControllerEditorWindow this one carries no visibility problem.
//   * EditorUtils.InvokePredicate (EditorUtils.cs:3226) is now EditorUtils.CreateCurve,
//     EditorUtils.AnimationCurves.cs line 175.
//   * EditorUtils.FindPredicate (EditorUtils.cs:3242) is now EditorUtils.GetEffectiveLength,
//     EditorUtils.AnimationCurves.cs line 207.
//   * EditorUtils.PrepareAssetPath(string, string, bool), named in the remarks below —
//     EditorUtils.Paths.cs line 191.
//
// UtilityWindowBase<T>.OnCustomConfirm is abstract, so the override cannot simply be omitted the
// way the other deferred members were: without it this type would not compile, and a compiling
// package is the hard constraint. The override is therefore present and empty. It is NOT a
// behavioural guess — it does nothing at all, and nothing in the ported package can reach it,
// because the only thing that opens this window is the equally deferred factory. The full shipped
// behaviour is documented below so that whoever ports the remaining dependencies can restore it
// without going back to the decompile.
//
// Audit status: PARTIAL.
//   Checked on 2026-08-05: the dependency list above was re-derived member by member against
//   export/ and against the package, and four of its five entries were found to have landed since
//   it was written; each is now cited by ported name, file and line, and the one that has not
//   (RateAnnotation, 9818) was confirmed absent by grepping the package for that line and by
//   reading its export body to establish that the ported LogWarning is a different member rather
//   than a rename of it.
//   NOT checked: the shipped behaviour of OnCustomConfirm itself — the three numbered steps, the
//   five user-visible consequences and the SHIPPED BUG in the blend-tree guard, all documented in
//   the remarks below — was NOT re-derived from export on this pass. It is carried over from the
//   pass that wrote it. That prose is the only description of this member anywhere in the package,
//   and the member is unported, so nothing verifies it by construction; treat it as unconfirmed
//   until someone diffs it against export ControllerEditor.cs line 4768 et seq.
//   The empty override itself is intentional and is not a stub of a guessed behaviour: it does
//   nothing, it is required because UtilityWindowBase<T>.OnCustomConfirm is abstract, and nothing
//   in the ported package can reach it because the only thing that opens this window is the
//   equally unwritten factory.

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
