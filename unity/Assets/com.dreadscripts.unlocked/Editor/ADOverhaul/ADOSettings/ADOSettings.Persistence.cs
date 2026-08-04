// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
// Ported region: the persistence half of the nested `ADOSettings` class, lines 1428-1676.
//
//   Save()                     -> Serialize(), line 1585
//   Load()                     -> Load(), line 1617
//   PromptClear()              -> PromptClear(), line 1658
//   Clear()                    -> Clear(), line 1666
//   nonSerializedFields        -> nonSerializedFields, line 1434
//   instance                   -> settingsInstance, line 1436
//   onCleared                  -> onCleared, line 1438
//   private ADOSettings() body -> CacheNonSerializedSettingFields(), line 1580
//   "No1lKII9IzcBAbihub6nCg==SettingsJSON" (inline at 1613, 1620 and 1622) -> prefsKey, line 1613
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The deferral/pending-save half of the shipped Save -- the `savePending`, `deferred` and
// `_ProxyIdentifier` statics, the `IsDeferred`/`SetDeferred` [SpecialName] accessors (1552/1558) and
// the branches that read them -- is NOT here: it was already ported to
// DreadScripts.Common.SettingsPersistence, shared with ControllerEditor. Serialize is what is
// left once those are removed, and it is subscribed to SettingsPersistence.onSave by the static
// constructor in ADOSettings.cs. Callers that used to call ADOSettings.Save() call
// SettingsPersistence.Save().
//
// The caching of the NonSerializedSetting fields is the whole body of the decompiled parameterless
// constructor (1577-1582); the constructor itself is claimed by ADOSettings.cs, and only its body,
// lifted into CacheNonSerializedSettingFields, is claimed here.
//
// ControllerEditor's EditorSettings.SaveSettings/LoadSettings/ClearSettings/PromptClearSettings
// (decompiled/ControllerEditor/.../ControllerEditor.cs, lines 1486-1577) are the same code with a
// different key, "yOk0XCnENLMO6DIF8cYpSg==SettingsJSON". They stay separate types because the two
// products persist independent blocks.
//
// Audit status: PARTIAL -- every mapping above, and every line number in it, was re-derived against
// decompiled/ADOverhaul2022 on 2026-08-05 (the previous numbers predated the 561e9ec re-snapshot and
// were three to four lines short); the doc-comment prose on the members below was not re-checked.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ADOverhaul
{
    internal partial class ADOSettings
    {
        /// <summary>
        /// The EditorPrefs key the whole settings block lives under.
        /// </summary>
        /// <remarks>
        /// The opaque prefix is a base64 blob baked into the shipped build, not something derived at
        /// runtime, so it is reproduced verbatim: change it and every existing installation loses its
        /// settings. Both the 2019 and the 2022 build use this same key. ControllerEditor uses a
        /// different one for its own block.
        /// </remarks>
        private const string prefsKey = "No1lKII9IzcBAbihub6nCg==SettingsJSON";

        /// <summary>
        /// Separates the entries of the stored envelope. Three zero-width spaces, chosen so that the
        /// delimiter cannot occur in a JSON payload written by Unity.
        /// </summary>
        private const string entryTerminator = "​​​";

        /// <summary>Matches one <c>name[payload]</c> entry of the envelope.</summary>
        private const string entryPattern = "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B";

        /// <summary>Name of the envelope entry holding the main JSON block.</summary>
        private const string mainEntry = "MAIN";

        /// <summary>
        /// Raised after <see cref="Clear"/> has replaced the settingsInstance, for anything holding derived
        /// state that has to be rebuilt against the new one.
        /// </summary>
        /// <remarks>
        /// Nothing in either shipped build subscribes; it exists as the seam the settings window
        /// would have used. Kept because it is the only notification a caller gets that
        /// <see cref="instance"/> now refers to a different object.
        /// </remarks>
        internal static Action onCleared;

        private static FieldInfo[] nonSerializedFields;

        private static ADOSettings settingsInstance;

        /// <summary>
        /// Collects the fields marked <see cref="NonSerializedSettingAttribute"/>, which are persisted
        /// one envelope entry each instead of riding in the main JSON block.
        /// </summary>
        /// <remarks>
        /// ADOverhaul declares no such field, so this always produces an empty array and every loop
        /// over it is a no-op. Ported as shipped -- the mechanism is what ControllerEditor's twin uses
        /// for its two asset references, and leaving it out here would make the two halves diverge for
        /// no gain.
        /// </remarks>
        private static void CacheNonSerializedSettingFields()
        {
            nonSerializedFields = typeof(ADOSettings)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(NonSerializedSettingAttribute), false))
                .ToArray();
        }

        /// <summary>
        /// Serializes the whole block into <see cref="EditorPrefs"/>. Subscribed to
        /// <see cref="SettingsPersistence.onSave"/>, which is what decides when it may run.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The stored form is an envelope of <c>name[payload]</c> entries separated by
        /// <see cref="entryTerminator"/>: one <c>MAIN</c> entry holding
        /// <see cref="JsonUtility.ToJson(object)"/> of the settings object, then one entry per
        /// <see cref="NonSerializedSettingAttribute"/> field holding
        /// <see cref="EditorJsonUtility.ToJson(object)"/> of that field's value. The extra layer
        /// exists because only <see cref="EditorJsonUtility"/> can write a reference to a Unity
        /// object, and it can only be given one object at a time. ADOverhaul has no such field, so in
        /// practice the envelope is only ever the MAIN entry.
        /// </para>
        /// <para>
        /// A field that fails to serialize is logged and skipped, so one bad reference cannot cost the
        /// user the rest of their settings.
        /// </para>
        /// </remarks>
        private static void Serialize()
        {
            StringBuilder envelope = new StringBuilder(mainEntry + "[" + JsonUtility.ToJson(instance) + "]" + entryTerminator);

            foreach (FieldInfo field in nonSerializedFields)
            {
                try
                {
                    string json = EditorJsonUtility.ToJson(field.GetValue(instance));
                    envelope.Append(field.Name + "[" + json + "]" + entryTerminator);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            EditorPrefs.SetString(prefsKey, envelope.ToString());
        }

        /// <summary>
        /// Builds <see cref="settingsInstance"/> from <see cref="EditorPrefs"/>, or from the field
        /// initialisers if there is nothing stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Entries are looked up by name rather than by position, so a settings block written by an
        /// older version simply leaves the newer settings at their defaults -- and, by the same token,
        /// a block written by the shipped build still loads here despite the three licence-gate fields
        /// having been dropped.
        /// </para>
        /// <para>
        /// SHIPPED BEHAVIOUR, preserved: two ways this can throw. A malformed envelope with a repeated
        /// entry name makes <see cref="Dictionary{TKey,TValue}.Add"/> throw rather than discarding the
        /// block. And the loop over <see cref="nonSerializedFields"/> assumes the cache is
        /// already filled, which holds only because every path to here runs the settingsInstance constructor
        /// first -- <see cref="JsonUtility.FromJson{T}(string)"/> invokes the parameterless
        /// constructor, and the fallback constructs directly.
        /// </para>
        /// </remarks>
        private static void Load()
        {
            string stored = string.Empty;
            if (EditorPrefs.HasKey(prefsKey))
            {
                stored = EditorPrefs.GetString(prefsKey, string.Empty);
            }

            Dictionary<string, string> entries = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(stored))
            {
                MatchCollection matches = Regex.Matches(stored, entryPattern);
                for (int i = 0; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    entries.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
            }

            if (entries.TryGetValue(mainEntry, out string main))
            {
                settingsInstance = JsonUtility.FromJson<ADOSettings>(main);
            }

            if (settingsInstance == null)
            {
                settingsInstance = new ADOSettings();
            }

            foreach (FieldInfo field in nonSerializedFields)
            {
                // Overwriting an existing object rather than creating one is what lets
                // EditorJsonUtility restore a Unity object reference at all; it has no way to
                // construct the target itself.
                object value = field.GetValue(settingsInstance) ?? Activator.CreateInstance(field.FieldType);
                if (entries.TryGetValue(field.Name, out string json))
                {
                    EditorJsonUtility.FromJsonOverwrite(json, value);
                }

                field.SetValue(settingsInstance, value);

                // Re-checked through the field because assigning a Unity object that was destroyed
                // during the overwrite stores something that compares equal to null.
                if (field.GetValue(settingsInstance) == null)
                {
                    field.SetValue(settingsInstance, Activator.CreateInstance(field.FieldType));
                }
            }
        }

        /// <summary>Asks first, then <see cref="Clear"/>.</summary>
        internal static void PromptClear()
        {
            if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
            {
                Clear();
            }
        }

        /// <summary>
        /// Throws the settings away and starts again from the field initialisers, then saves, so the
        /// stored block is replaced rather than merely orphaned.
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
