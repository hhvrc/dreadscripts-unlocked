// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the startup, domain-reload and avatar-refresh plumbing of the outer ADOverhaul
// class. Line numbers are relative to the current snapshot; the decompiled names are the durable
// reference.
//
//   LogoutConfiguration  -> RefreshAvatarTables,          line 6509
//   SetupConfiguration   -> RefreshAvatarParameterNames,  line 6524
//   SelectConfiguration  -> ResetFoldouts,                line 6536
//
// PARTIAL PORT. The rest of the region is left out rather than stubbed, because each member reaches
// something that is not ported yet. Listed with what it needs, so it is mechanical once those land:
//
//   MapConfiguration     line 6480  -- runs an action between a scene-GUI re-subscribe and an avatar
//                                     rescan. Needs CancelConfiguration (below) and
//                                     PrintConfiguration (line 6596).
//   FillConfiguration    line 6488  -- a PlayModeStateChange handler; on ExitingEditMode it toggles
//                                     PhysBone test mode off so the temporary "Physbone Tester"
//                                     hierarchy does not survive into play mode. Needs
//                                     NewConfiguration (line 6273).
//   CancelConfiguration  line 6496  -- subscribes or unsubscribes the scene-view shape handles, and
//                                     on unsubscribe restores Tools.hidden. Needs
//                                     CalculateConfiguration (line 6060).
//   WriteConfiguration   line 6552  -- see the [DidReloadScripts] note below. Needs
//                                     PhysBoneEditor.WriteSingleton,
//                                     PhysBoneColliderEditor.InsertProperty,
//                                     ContactSenderEditor.InvokeProperty and
//                                     ContactReceiverEditor.ReadPage; the two contact editors are
//                                     not ported as types at all.
//   MapIdentifier        line 8096  -- see the [InitializeOnLoadMethod] audit below. Needs
//                                     ConnectSerializer (line 7899), CancelIdentifier (line 8118)
//                                     and SetupIdentifier (line 8185).
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

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEngine.Events;
using VRC.SDK3.Dynamics.Contact.Components;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOverhaul
    {
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
            ADOEditorUtility.GetPlayableLayerOptions(selectedAvatar, ref avatarPlayableLayerNames, ref avatarPlayableLayerTypes);
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
