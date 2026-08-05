// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the startup, domain-reload and avatar-refresh plumbing of the outer ADOverhaul
// class. The decompiled names are the durable reference; the MAP line numbers below are mixed
// vintages and the NOTES section at the bottom says which is which.
//
//   CancelConfiguration  -> SetShapeEditOverlayActive,    line 6700
//   LogoutConfiguration  -> RefreshAvatarTables,          line 6509
//   SetupConfiguration   -> RefreshAvatarParameterNames,  line 6524
//   SelectConfiguration  -> ResetFoldouts,                line 6536
//
// PARTIAL PORT. The rest of the region is left out rather than stubbed. This list was written when
// none of the named blockers existed; most of them have landed since, so each entry now records
// whether it is still blocked or merely unwritten.
//
//   MapConfiguration     line 6480  -- runs an action between a scene-GUI re-subscribe and an avatar
//                                     rescan. FULLY UNBLOCKED, merely unwritten: all three of its
//                                     callees are now ported -- CancelConfiguration as
//                                     SetShapeEditOverlayActive here, PrintConfiguration (line 6596)
//                                     as RefreshSceneAvatars in ADOverhaul.AvatarSelection.cs, and
//                                     LogoutConfiguration as RefreshAvatarTables here. Nothing would
//                                     call it yet: its only call sites are the OnEnable of
//                                     ContactReceiverEditor, ContactSenderEditor and
//                                     PhysBoneColliderEditor, and none of those three is ported.
//   FillConfiguration    line 6488  -- a PlayModeStateChange handler; on ExitingEditMode it toggles
//                                     PhysBone test mode off so the temporary "Physbone Tester"
//                                     hierarchy does not survive into play mode. NOT MISSING: it is
//                                     already ported, in ADOverhaul.SceneView.cs, under the
//                                     deliberately distinct name StopTestModeOnEnteringPlayMode --
//                                     that file owns its only subscription, so it was declared
//                                     beside it. Nothing should be added here for it; see that
//                                     file's header. Its blocker NewConfiguration (line 6272) is
//                                     ported there too, as ToggleTestMode.
//   WriteConfiguration   line 6552  -- see the [DidReloadScripts] note below. Still blocked. Needs
//                                     PhysBoneEditor.WriteSingleton,
//                                     PhysBoneColliderEditor.InsertProperty,
//                                     ContactSenderEditor.InvokeProperty and
//                                     ContactReceiverEditor.ReadPage; the two contact editors are
//                                     not ported as types at all.
//   MapIdentifier        line 8096  -- see the [InitializeOnLoadMethod] audit below. Of the three it
//                                     names, SetupIdentifier (line 8185) is ported as
//                                     ApplyCachedUpdateInfo in ADOverhaul.Menus.cs and
//                                     CancelIdentifier (line 8118) is deliberately dropped there as
//                                     the dead update request -- so only ConnectSerializer
//                                     (line 7899, the `updateCheckedToday` accessor) is genuinely
//                                     missing, and the audit below already says this hook should be
//                                     wired straight to the local half rather than reproduced.
//
// ================================ [InitializeOnLoadMethod] audit ================================
//
// Both of the class's InitializeOnLoadMethod hooks run on every domain reload in every project that
// imports the package, so both were traced end to end before deciding what to do with them.
//
// (1) DisableConfiguration, decompiled line 7063. DELIBERATELY NOT PORTED -- it is the licensing
//     gate and nothing else. In full:
//
//         reads EditorPrefs "No1lKII9IzcBAbihub6nCg==LK" (the stored licence key) and checks it
//         against a hex pattern; sets licenseKeyEntryRequired / licenseCheckedThisSession; then, if
//         a key is present AND the a_VerifyOnProjectLoad setting is on, queues a delayCall to the
//         verification routine (AssetConfiguration, line 7086).
//
//     That routine, on a cache miss, spawns four child processes -- `powershell.exe /c wmic
//     baseboard get *` and three siblings (line 7297) -- to read motherboard, CPU, disk and memory
//     serial numbers, SHA-1s them into the "HWID" field, writes the collected string back to
//     EditorPrefs under "DSLICINF", ensures a persistent install GUID in EditorPrefs
//     "DreadScriptssid", and POSTs all of it as JSON to
//     https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand (line 7767).
//     So: yes to network I/O, yes to subprocesses, yes to machine identifiers, yes to disk writes --
//     all of them gated on the user having previously entered a key and opted in
//     (a_VerifyOnProjectLoad defaults to false, decompiled line 1480). The endpoint is dead and the
//     whole path is what this restoration exists to remove, so the hook is dropped in full. Nothing
//     else in the method has any effect on the tool's behaviour.
//
// (2) MapIdentifier, decompiled line 8096. NOT PORTED HERE, and it should not be ported as written.
//     It is the update/announcement check:
//
//         if the cached "last checked" day is not today, or no version has ever been cached, it
//         queues CancelIdentifier (line 8118), which POSTs {command:"getdownloadinfo", product_id,
//         version} to the same cloud-function endpoint and writes the reply into ADOSettings
//         (update link, message, changelog, version, announcement and its link).
//         Otherwise it calls SetupIdentifier (line 8185), which is purely local: it expires a
//         seven-day-old "announcement hidden" flag and compares the cached remote version against
//         `version` to decide whether to raise the update banner.
//
//     Unlike (1) this fires with no licence and no opt-in -- there is no setting that suppresses it
//     -- but the request carries only the command, the product id and the tool version: no HWID, no
//     session id, no licence key (CancelIdentifier builds its own field list rather than going
//     through CountConfiguration, line 7402, which is what adds those three). No subprocess. Disk
//     writes are limited to ADOSettings. Its endpoint is equally dead, so the network half is
//     useless; the local half (SetupIdentifier) is the part worth keeping, and whoever ports it
//     should wire the hook to that directly rather than reproduce the daily request.
//
// ================================= [DidReloadScripts] note =====================================
//
// WriteConfiguration (line 6552) is the one piece of reload plumbing that is genuinely functional.
// It reflects UnityEditor.CustomEditorAttributes' private static s_Initialized field, polls it every
// 200 ms for up to 30 tries, and once Unity has built its custom-editor table re-registers
// ADOverhaul's four replacement inspectors over the VRChat SDK's own. It has to wait because the
// table is built lazily and overwriting it before Unity populates it would be undone. Its
// captured-variable display class (_003C_003Ec__DisplayClass66_0, line 5522) is a decompiler
// artifact; the original was a local async lambda. Deferred only for its four missing call targets.
//
// SHIPPED BUG PRESERVED in RefreshAvatarTables -- see the remarks on that method. The 2019 build
// does not have it, and this is the one place in this region where the two builds diverge.
//
// ======================================= 2019 vs 2022 =======================================
//
// SetShapeEditOverlayActive is the same method in both builds and does not add to that divergence.
// 2019 calls it ResolveSystem (2019 line 6679) and the decompiler emitted its arms the other way
// round -- `if (!compareres) { Tools.hidden = false; } else { duringSceneGui += SetSystem; }` against
// 2022's `if (isvar1) { += } else { Tools.hidden = false; }`. Same three statements, same condition,
// opposite arm order: a branch inversion the decompiler chose, not a behavioural difference. The
// 2022 shape is the one ported, since 2022 is the build this file follows.
//
// ========================================== NOTES ==========================================
//
// Mixed line numbering in the MAP above, deliberately, and it is not safe to tidy from this file
// alone. CancelConfiguration's entry carries 6700, which is where it stands in the current
// reverse-engineering/export/ snapshot. The other three were written before the 561e9ec re-snapshot and are each
// 204 lines short of it -- RefreshAvatarTables, RefreshAvatarParameterNames and ResetFoldouts really
// begin at 6713, 6728 and 6740. They were NOT re-based, because ADOverhaul.AvatarSelection.cs still
// carries the same pre-561e9ec numbering and claims 6684 and 6740 for ForgotConfiguration and
// CheckConfiguration (really 6888 and 6944): re-basing ResetFoldouts to its true 6740 would leave
// two files claiming one decompiled line, which reverse-engineering/tools/check-headers.py reports as a double port.
// Re-base the two files in one change when the snapshot sweep reaches them.
//
// The line numbers in the prose sections above are pre-561e9ec throughout, and the offset is close
// to but not exactly 204 (attributes and blank lines moved with the members), so take the decompiled
// member names as the reference rather than arithmetic on those numbers.
//
// Audit status: PARTIAL -- SetShapeEditOverlayActive was transcribed statement by statement from
// decompiled 6700-6711, cross-checked against 2019's ResolveSystem, and its call sites at 1867,
// 2094, 2343 and 6686 were read; the three pre-existing members and the prose sections above were
// not re-verified in that pass.

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEngine.Events;
using VRC.SDK3.Dynamics.Contact.Components;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Registers or unregisters <see cref="DrawShapeEditOverlay"/> as a scene-GUI handler, and
        /// hands Unity's own transform tools back when it unregisters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The unsubscribe is unconditional and the re-subscribe only follows it. That idiom is what
        /// keeps the overlay registered exactly once however many inspectors ask for it: three of
        /// them -- <c>ContactReceiverEditor</c>, <c>ContactSenderEditor</c> and
        /// <c>PhysBoneColliderEditor</c> -- reach it with <see langword="false"/> directly from every
        /// <c>OnDisable</c> and with <see langword="true"/> from every <c>OnEnable</c> by way of
        /// <c>MapConfiguration</c>, and Unity enables an inspector again on each selection change, so
        /// a bare <c>+=</c> would accumulate a subscription per enable and draw the panel that many
        /// times over.
        /// </para>
        /// <para>
        /// The unregister path additionally clears <see cref="Tools.hidden"/>, because
        /// <see cref="DrawShapeEditOverlay"/> sets it on every pass it draws and never restores it.
        /// Without this line, closing an inspector while an edit toggle was still on would leave the
        /// scene view with no transform gizmo and nothing left drawing to explain why.
        /// </para>
        /// <para>
        /// Nothing in the package reaches this yet: the three inspector <c>OnEnable</c>/
        /// <c>OnDisable</c> pairs and <c>MapConfiguration</c>, which is what the <c>OnEnable</c> half
        /// goes through, are all still unported -- see the PARTIAL PORT list in the file header.
        /// </para>
        /// </remarks>
        private static void SetShapeEditOverlayActive(bool active)
        {
            SceneView.duringSceneGui -= DrawShapeEditOverlay;

            if (active)
            {
                SceneView.duringSceneGui += DrawShapeEditOverlay;
            }
            else
            {
                Tools.hidden = false;
            }
        }

        /// <summary>
        /// Rebuilds everything the inspectors derive from <see cref="selectedAvatar"/>: the playable
        /// layer lists, the animator parameter names and the collision tag list. Called whenever the
        /// target avatar changes.
        /// </summary>
        /// <remarks>
        /// SHIPPED BUG, PORTED AS-IS. The null-avatar branch clears the parameter names but neither
        /// clears the collision tags nor returns, so both statements after it dereference the null
        /// descriptor and this method throws when no avatar is assigned. The 2019 build's equivalent
        /// (VerifySystem, decompiled 2019 line 6491) clears both arrays and returns; the 2022 build
        /// this port follows is two statements shorter and has neither. The behaviour is reproduced
        /// rather than repaired, per the restoration's faithfulness rule -- but the divergence is
        /// large enough that the possibility of a decompiler slip cannot be ruled out entirely, and
        /// any caller that can pass a null avatar should be read with that in mind.
        /// </remarks>
        private static void RefreshAvatarTables()
        {
            ADOEditorUtility.GetPopulatedPlayableLayers(selectedAvatar, ref avatarPlayableLayerNames, ref avatarPlayableLayerTypes);
            if (!selectedAvatar)
            {
                avatarParameterNames = Array.Empty<string>();
            }

            RefreshAvatarParameterNames();

            // VRChat's built-in tags are removed and re-added under a "Default/" prefix so the
            // dropdown separates them from the avatar's own tags; the prefix is stripped again when
            // a default tag is picked.
            avatarCollisionTags = selectedAvatar.GetComponentsInChildren<VRCContactSender>()
                .SelectMany(sender => sender.collisionTags)
                .Concat(selectedAvatar.GetComponentsInChildren<VRCContactReceiver>()
                    .SelectMany(receiver => receiver.collisionTags))
                .Except(ADOEditorUtility.defaultCollisionTags)
                .Concat(ADOEditorUtility.defaultCollisionTags.Select(tag => "Default/" + tag))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Collects the animator parameter names the avatar's own playable layers declare, for the
        /// parameter-name dropdown next to contact receiver parameter fields.
        /// </summary>
        /// <remarks>
        /// Default layers are skipped -- their parameters are VRChat's, not the user's -- and so are
        /// the reserved parameter names, which a contact must never drive. Each controller is
        /// re-loaded from its asset path rather than used directly, because a descriptor may hold a
        /// runtime controller or a scene instance, and only the asset carries the parameter list the
        /// dropdown should offer.
        /// </remarks>
        private static void RefreshAvatarParameterNames()
        {
            avatarParameterNames = selectedAvatar.baseAnimationLayers
                .Concat(selectedAvatar.specialAnimationLayers)
                .Where(layer => !layer.isDefault && layer.animatorController)
                .Select(layer => AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(layer.animatorController)))
                .Where(controller => controller)
                .SelectMany(controller => controller.parameters)
                .Select(parameter => parameter.name)
                .Where(name => !ADOEditorUtility.reservedAvatarParameters.Contains(name))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Replaces every entry of a foldout-animation array with a fresh <see cref="AnimBool"/>
        /// carrying the same open/closed target, and subscribes <paramref name="onValueChanged"/> to
        /// each.
        /// </summary>
        /// <remarks>
        /// The inspectors keep their foldout animations in static arrays that outlive any one
        /// inspector instance, so the repaint callback has to be re-pointed at the current one every
        /// time an inspector is enabled. Rebuilding rather than re-subscribing is what avoids
        /// accumulating listeners onto destroyed editors; the target is carried across so the user's
        /// foldouts do not snap shut on every selection change. A null entry is a first run and
        /// starts closed.
        /// </remarks>
        private static void ResetFoldouts(AnimBool[] foldouts, UnityAction onValueChanged)
        {
            for (int i = 0; i < foldouts.Length; i++)
            {
                if (foldouts[i] == null)
                {
                    foldouts[i] = new AnimBool();
                }
                else
                {
                    foldouts[i] = new AnimBool(foldouts[i].target);
                }

                foldouts[i].valueChanged.AddListener(onValueChanged);
            }
        }
    }
}
