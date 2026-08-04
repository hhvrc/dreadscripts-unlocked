// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the ADOSettings class, lines 751-1688 of the current snapshot. Line numbers move
// with the snapshot; the member names below are the durable reference.
//
//   ADOSettings                  -> same, line 751
//   instance (field)             -> settingsInstance, line 1436
//   Instance() [SpecialName]     -> instance (property), line 1569
//   onCleared                    -> same, line 1438
//   nonSerializedFields          -> same, line 1434
//   .ctor()                      -> the static constructor, line 1578 (see "Deviations")
//   Save()                       -> Serialize(), line 1585 (deferral half now in Common)
//   Load()                       -> Load(), line 1617
//   PromptClear()                -> PromptClear(), line 1658
//   Clear()                      -> Clear(), line 1666
//   StateColors() [SpecialName]  -> stateColors (property), line 1679
//   every u_* / on* / gizmo* / *Color settings field -> same name, lines 1440-1549
//
// NOT HERE, because they are the shared settings framework and were ported once into
// DreadScripts.Common (Editor/Common/Settings/): SettingsChangeScope (753), SettingsDeferScope
// (793), BoolSetting (810), FloatSetting (890), EnumSetting (996), VectorSetting (1065),
// StringSetting (1174), ColorSetting (1222), ObjectReferenceSetting (1300), SettingBase (1416),
// NonSerializedSettingAttribute (1423), and the static deferred / savePending / _ProxyIdentifier
// trio with IsDeferred / SetDeferred (1428-1432, 1551-1566). Common's SettingsPersistence owns the
// "should this save happen now" half of Save(); what is left here is the product-specific half —
// the EditorPrefs key and the envelope — subscribed to SettingsPersistence.onSave.
//
// LIFTED OUT OF ADOverhaul. The decompiled type is `private class ADOSettings` nested inside
// ADOverhaul. It is lifted to a top-level `internal` type in the same namespace, matching every
// other nested type already ported out of this file (ADOverhaulWindow, PhysBoneEditor,
// PhysBoneColliderEditor, and BugReporter / ProcessRunner / JsonObject / JsonValue / CustomLogType
// in Common). Nothing outside ADOverhaul named the type, so the change of nesting costs nothing.
//
// LICENCE CODE REMOVED. Three settings fields were licence state and are gone:
//   a_HasSucceededLastVerification  line 1474  BoolSetting(false)
//   a_VerifyOnDisplay               line 1477  BoolSetting(true)
//   a_VerifyOnProjectLoad           line 1480  BoolSetting(false)
// Every reader of all three lived in the licence code (DisableConfiguration, VisitConfiguration,
// AssetConfiguration, PopConfiguration, SetIdentifier, InvokeIdentifier's "Verify/..." menu items
// and the transfer flow), so no surviving caller loses a value.
//
// THE STORED BLOB FORMAT IS UNCHANGED, which matters because it is what previously-saved user
// settings are read back from. Everything that defines the format is transcribed exactly:
//   * the EditorPrefs key, "No1lKII9IzcBAbihub6nCg==SettingsJSON" (the product hash + "SettingsJSON")
//   * the envelope "NAME[<json>]" + U+200B U+200B U+200B, once for MAIN and once per
//     [NonSerializedSetting] field
//   * the read-back regex "(\w+)\[(.*?)\]" + three U+200B
//   * JsonUtility for the MAIN block, EditorJsonUtility for the per-field blocks
//   * the names and declaration order of every surviving [SerializeField] field
// Dropping the three fields only removes their three keys from the MAIN JSON object. JsonUtility
// ignores JSON members with no matching field, so a blob written by the shipped build still loads
// here; and a blob written here still loads in the shipped build, where the three absent members
// simply keep the values their field initialisers gave them. No other key moves, so nothing else
// about a saved settings block changes.
//
// DEVIATIONS from export, both behaviour-preserving and both deliberate:
//   * nonSerializedFields is filled by the static constructor rather than by the instance
//     constructor. The reflection query reads only typeof(ADOSettings), so the result is the same
//     array whenever it runs; running it once removes the assumption that JsonUtility.FromJson
//     invokes the private constructor, which Load() silently depended on (the field is read
//     immediately afterwards and a null would throw).
//   * Save() is split. The shipped Save() did the deferral bookkeeping and then wrote the blob;
//     the bookkeeping is now SettingsPersistence.Save() and the write is Serialize(), reached
//     through SettingsPersistence.onSave. Same order of operations, same guards.
//
// 2019 vs 2022: the two builds are the same code under different obfuscated names. The 2019 build
// carries the same field set, the same key and the same envelope; nothing needed choosing between
// them.
//
// Audit status: VERIFIED against export -- every field, default value, change callback and every
// line of Save/Load/Clear/PromptClear re-read against lines 751-1688 on 2026-08-04.

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// ADOverhaul's persisted settings: one instance, round-tripped through EditorPrefs, holding
    /// every user preference the tool exposes plus the update/announcement notice it caches from
    /// the vendor's server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The settings themselves are <see cref="SettingBase"/> subclasses from
    /// <see cref="DreadScripts.Common"/>, which save the whole block whenever any one of them is
    /// assigned. This class provides the two halves that are specific to the product: where the
    /// block is stored, and what it is wrapped in.
    /// </para>
    /// <para>
    /// Editor-only and per-machine by construction — EditorPrefs is keyed per Unity install, not
    /// per project, so these settings follow the user rather than the project they are working in.
    /// </para>
    /// </remarks>
    [Serializable]
    internal class ADOSettings
    {
        /// <summary>
        /// The EditorPrefs key the whole settings block lives under. The prefix is the product hash
        /// ADOverhaul identifies itself by, so the two DreadScripts tools do not collide.
        /// </summary>
        private const string prefsKey = "No1lKII9IzcBAbihub6nCg==SettingsJSON";

        /// <summary>
        /// Terminates each <c>NAME[json]</c> entry in the stored blob: three zero-width spaces.
        /// </summary>
        /// <remarks>
        /// A separator chosen so it cannot occur inside the JSON it follows. It is part of the
        /// stored format — changing it orphans every settings block already written.
        /// </remarks>
        private const string entryTerminator = "\u200b\u200b\u200b";

        /// <summary>
        /// Matches one <c>NAME[json]</c> entry. Non-greedy on the payload so that a block with
        /// several entries splits at the first terminator rather than the last.
        /// </summary>
        private const string entryPattern = "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B";

        /// <summary>
        /// The settings fields that cannot travel inside the main JSON block and are written as
        /// their own entries instead — see <see cref="NonSerializedSettingAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Empty in both shipped ADOverhaul builds: nothing in this class carries the attribute.
        /// The machinery is kept because it defines the stored format, which has to keep reading
        /// blocks written by the shipped builds.
        /// </remarks>
        private static readonly FieldInfo[] nonSerializedFields;

        private static ADOSettings settingsInstance;

        /// <summary>Invoked after <see cref="Clear"/> has replaced the instance.</summary>
        /// <remarks>
        /// Subscribers use this to drop anything they derived from a setting; the object they were
        /// reading from is gone by the time it fires.
        /// </remarks>
        internal static Action onCleared;

        /// <summary>The settings block, loaded from EditorPrefs on first use.</summary>
        internal static ADOSettings instance
        {
            get
            {
                if (settingsInstance == null)
                {
                    Load();
                }

                return settingsInstance;
            }
        }

        static ADOSettings()
        {
            nonSerializedFields = typeof(ADOSettings)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(NonSerializedSettingAttribute), false))
                .ToArray();

            SettingsPersistence.onSave += Serialize;
        }

        // ── Cached update notice ────────────────────────────────────────────────────────────
        // Written by the update check from the server's reply and read by the notice the toolbar
        // draws. Persisted rather than re-fetched so that the notice survives a domain reload.

        [SerializeField]
        internal StringSetting u_updateLink = new StringSetting();

        [SerializeField]
        internal StringSetting u_updateVersion = new StringSetting();

        [SerializeField]
        internal StringSetting u_updateMessage = new StringSetting();

        [SerializeField]
        internal StringSetting u_updateChangelog = new StringSetting();

        /// <summary>The date the cached notice was fetched on, which is what makes it stale.</summary>
        [SerializeField]
        internal StringSetting u_updateDay = new StringSetting();

        [SerializeField]
        internal StringSetting u_announcement = new StringSetting();

        [SerializeField]
        internal StringSetting u_announcementLink = new StringSetting();

        [SerializeField]
        internal StringSetting u_announcementLinkName = new StringSetting();

        [SerializeField]
        internal StringSetting u_announcementHiddenDate = new StringSetting();

        /// <summary>Set by "Skip for Today"; cleared when a newer notice arrives.</summary>
        [SerializeField]
        internal BoolSetting u_updateHidden = new BoolSetting(false);

        [SerializeField]
        internal BoolSetting u_announcementHidden = new BoolSetting(false);

        // ── Gizmos ──────────────────────────────────────────────────────────────────────────
        // All four push themselves onto every PhysBone in the scene when changed, because VRChat's
        // gizmo flags live on the components rather than in a global.

        [SerializeField]
        internal BoolSetting gizmosActive = new BoolSetting(true, PhysBoneEditor.ApplyGlobalGizmoSettings);

        /// <summary>
        /// Whether the three gizmo settings apply to every PhysBone in the scene rather than only
        /// to the inspected one. Gates the push itself, so turning it off freezes each PhysBone at
        /// whatever it was last given.
        /// </summary>
        [SerializeField]
        internal BoolSetting globalGizmo = new BoolSetting(true, PhysBoneEditor.ApplyGlobalGizmoSettings);

        [SerializeField]
        internal FloatSetting gizmoBoneOpacity = new FloatSetting(0.5f, PhysBoneEditor.ApplyGlobalGizmoSettings);

        [SerializeField]
        internal FloatSetting gizmoLimitOpacity = new FloatSetting(0.5f, PhysBoneEditor.ApplyGlobalGizmoSettings);

        // ── Inspector and scene-view behaviour ──────────────────────────────────────────────

        [SerializeField]
        internal BoolSetting editorAnimatedFoldouts = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneNameLabels = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneToolSelection = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneToolSelectionAlwaysVisible = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneEditingOverlay = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneOverlayInterceptsClick = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting onSceneTooltip = new BoolSetting(true);

        /// <summary>Whether scene picking is suppressed while a scene-view tool is active.</summary>
        [SerializeField]
        internal BoolSetting ignoreSceneClicks = new BoolSetting(true);

        [SerializeField]
        internal BoolSetting hideToolsDuringTesting = new BoolSetting(true);

        /// <summary>
        /// Set by the "Don't ask again" answer to the restart-testing prompt, which is the only
        /// thing that reads it.
        /// </summary>
        [SerializeField]
        internal BoolSetting hasReadColliderTestingWarning = new BoolSetting(false);

        /// <summary>Which corner of the scene view the tool-selection strip docks to.</summary>
        [SerializeField]
        internal EnumSetting toolSelectionOverlayAlignment = EnumSetting.FromEnum(PositionFlag.BottomLeft);

        /// <summary>Which corner of the scene view the tool overlay docks to.</summary>
        [SerializeField]
        internal EnumSetting toolOverlayAlignment = EnumSetting.FromEnum(PositionFlag.BottomRight);

        [SerializeField]
        internal FloatSetting handleSizeMultiplier = new FloatSetting(1f);

        // ── Palette ─────────────────────────────────────────────────────────────────────────

        [SerializeField]
        internal ColorSetting labelColor = new ColorSetting(1f, 1f, 1f);

        [SerializeField]
        internal ColorSetting generalColor = new ColorSetting(1f, 1f, 1f);

        [SerializeField]
        internal ColorSetting activeColor = new ColorSetting(0.56f, 0.94f, 0.47f);

        [SerializeField]
        internal ColorSetting inactiveColor = new ColorSetting(1f, 0f, 0.3765f);

        [SerializeField]
        internal ColorSetting mixedColor = new ColorSetting(1f, 0.65f, 0f);

        [SerializeField]
        internal ColorSetting selectionColor = new ColorSetting(1f, 0.65f, 0f);

        /// <summary>
        /// The user's handle palette in the order a tri-state toggle indexes it: 0 = inactive,
        /// 1 = active, 2 = mixed.
        /// </summary>
        /// <remarks>
        /// Built fresh on every read rather than cached, so it always reflects the current setting;
        /// callers pass it straight to a GUIColorScope and drop it.
        /// </remarks>
        internal Color[] stateColors => new[]
        {
            inactiveColor.value,
            activeColor.value,
            mixedColor.value
        };

        /// <remarks>
        /// Private so that the only way to a settings block is <see cref="instance"/>. Unity's
        /// JsonUtility still reaches it when deserialising.
        /// </remarks>
        private ADOSettings()
        {
        }

        /// <summary>
        /// Writes the whole settings block to EditorPrefs. Subscribed to
        /// <see cref="SettingsPersistence.onSave"/>, which decides whether a save happens at all.
        /// </summary>
        private static void Serialize()
        {
            StringBuilder blob = new StringBuilder("MAIN[" + JsonUtility.ToJson(instance) + "]" + entryTerminator);
            foreach (FieldInfo field in nonSerializedFields)
            {
                try
                {
                    string json = EditorJsonUtility.ToJson(field.GetValue(instance));
                    blob.Append(field.Name + "[" + json + "]" + entryTerminator);
                }
                catch (Exception e)
                {
                    // One field that will not serialise must not cost the user the rest of the
                    // block, so the failure is logged and the remaining entries are still written.
                    Debug.LogError(e);
                }
            }

            EditorPrefs.SetString(prefsKey, blob.ToString());
        }

        /// <summary>
        /// Reads the settings block back, falling back to defaults for anything missing.
        /// </summary>
        /// <remarks>
        /// Every path leaves <see cref="settingsInstance"/> non-null: an absent key, an unparseable
        /// block and a JSON payload JsonUtility rejects all end at a fresh instance whose field
        /// initialisers have supplied the defaults.
        /// </remarks>
        private static void Load()
        {
            string blob = string.Empty;
            if (EditorPrefs.HasKey(prefsKey))
            {
                blob = EditorPrefs.GetString(prefsKey, string.Empty);
            }

            Dictionary<string, string> entries = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(blob))
            {
                MatchCollection matches = Regex.Matches(blob, entryPattern);
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    entries.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
            }

            if (entries.TryGetValue("MAIN", out string main))
            {
                settingsInstance = JsonUtility.FromJson<ADOSettings>(main);
            }

            if (settingsInstance == null)
            {
                settingsInstance = new ADOSettings();
            }

            foreach (FieldInfo field in nonSerializedFields)
            {
                // FromJsonOverwrite needs something to overwrite, and the field can legitimately be
                // null here: JsonUtility does not run field initialisers when it deserialises the
                // MAIN block, so a field it did not restore comes back null.
                object value = field.GetValue(settingsInstance) ?? Activator.CreateInstance(field.FieldType);
                if (entries.TryGetValue(field.Name, out string json))
                {
                    EditorJsonUtility.FromJsonOverwrite(json, value);
                }

                field.SetValue(settingsInstance, value);
                if (field.GetValue(settingsInstance) == null)
                {
                    field.SetValue(settingsInstance, Activator.CreateInstance(field.FieldType));
                }
            }
        }

        /// <summary>Asks before clearing, for the settings window's "Clear Settings" button.</summary>
        internal static void PromptClear()
        {
            if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
            {
                Clear();
            }
        }

        /// <summary>
        /// Replaces the settings block with a fresh one and saves it, so that every setting returns
        /// to the default its field initialiser gives it.
        /// </summary>
        internal static void Clear()
        {
            settingsInstance = new ADOSettings();
            foreach (FieldInfo field in nonSerializedFields)
            {
                field.SetValue(settingsInstance, Activator.CreateInstance(field.FieldType));
            }

            onCleared?.Invoke();
            SettingsPersistence.Save();
        }
    }
}
