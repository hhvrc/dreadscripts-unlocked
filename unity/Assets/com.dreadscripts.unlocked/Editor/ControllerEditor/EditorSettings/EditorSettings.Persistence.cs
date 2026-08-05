// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// Ported region: the persistence half of the nested `EditorSettings` class, lines 1393-1577.
//
//   SaveSettings()             -> WriteToPrefs(), line 1486
//   LoadSettings()             -> Load(), line 1518
//   PromptClearSettings()      -> PromptClear(), line 1559
//   ClearSettings()            -> Clear(), line 1567
//   nonSerializedSettingFields -> nonSerializedSettingFields, line 1397
//   instance                   -> instance, line 1399
//   onSettingsCleared          -> onCleared, line 1401
//   private EditorSettings() body -> CacheNonSerializedSettingFields(), line 1481
//   "yOk0XCnENLMO6DIF8cYpSg==SettingsJSON" (inline at 1514, 1521 and 1523) -> prefsKey, line 1514
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The caching of the NonSerializedSetting fields is the whole body of the decompiled parameterless
// constructor (1479-1484); the constructor itself is claimed by EditorSettings.cs, and only its
// body, lifted into CacheNonSerializedSettingFields, is claimed here.
//
// The deferral/pending-save half of the shipped SaveSettings -- the `pendingSave`, `deferred` and
// `_InterpreterAlgo` statics and the branches that read them -- is NOT here: it was already ported
// to DreadScripts.Common.SettingsPersistence, shared with ADOverhaul. WriteToPrefs is what is left
// once those are removed, and it is subscribed to SettingsPersistence.onSave by the static
// constructor in EditorSettings.cs. Callers that used to call SaveSettings() call
// SettingsPersistence.Save().
//
// ADOverhaul's ADOSettings.Save/Load/Clear/PromptClear (reverse-engineering/export/ADOverhaul2022/.../ADOverhaul.cs,
// lines 1585-1676) are the same code with a different key, "No1lKII9IzcBAbihub6nCg==SettingsJSON".
// They stay separate types because the two products persist independent blocks.
//
// Audit status: PARTIAL -- every mapping above, and every line number in it, was re-derived against
// reverse-engineering/export/ControllerEditor on 2026-08-05; the ControllerEditor numbers were already correct, only
// the three inline-key line numbers and the ADOverhaul cross-reference above needed correcting. The
// doc-comment prose on the members below was not re-checked.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    internal partial class EditorSettings
    {
        /// <summary>
        /// The EditorPrefs key the whole settings block lives under.
        /// </summary>
        /// <remarks>
        /// The opaque prefix is a base64 blob baked into the shipped build, not something derived at
        /// runtime, so it is reproduced verbatim: change it and every existing installation loses
        /// its settings. ADOverhaul uses a different one for its own block.
        /// </remarks>
        private const string prefsKey = "yOk0XCnENLMO6DIF8cYpSg==SettingsJSON";

        /// <summary>
        /// Separates the entries of the stored envelope. Three zero-width spaces, chosen so that the
        /// delimiter cannot occur in a JSON payload written by Unity.
        /// </summary>
        private const string entrySeparator = "​​​";

        /// <summary>Matches one <c>name[payload]</c> entry of the envelope.</summary>
        private const string entryPattern = "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B";

        /// <summary>Name of the envelope entry holding the main JSON block.</summary>
        private const string mainEntry = "MAIN";

        /// <summary>
        /// Raised after <see cref="Clear"/> has replaced the instance, for anything holding derived
        /// state that has to be rebuilt against the new one.
        /// </summary>
        internal static Action onCleared;

        private static FieldInfo[] nonSerializedSettingFields;

        private static EditorSettings instance;

        /// <summary>
        /// Collects the fields marked <see cref="NonSerializedSettingAttribute"/>, which are
        /// persisted one envelope entry each instead of riding in the main JSON block.
        /// </summary>
        /// <remarks>
        /// The cache is static but is filled from the instance constructor, which is the shipped
        /// arrangement and the reason <see cref="Load"/> is ordered the way it is.
        /// </remarks>
        private static void CacheNonSerializedSettingFields()
        {
            nonSerializedSettingFields = typeof(EditorSettings)
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
        /// <see cref="entrySeparator"/>: one <c>MAIN</c> entry holding
        /// <see cref="JsonUtility.ToJson(object)"/> of the settings object, then one entry per
        /// <see cref="NonSerializedSettingAttribute"/> field holding
        /// <see cref="EditorJsonUtility.ToJson(object)"/> of that field's value. The extra layer
        /// exists because only <see cref="EditorJsonUtility"/> can write a reference to a Unity
        /// object, and it can only be given one object at a time.
        /// </para>
        /// <para>
        /// A field that fails to serialize is logged and skipped, so one bad reference cannot cost
        /// the user the rest of their settings.
        /// </para>
        /// </remarks>
        private static void WriteToPrefs()
        {
            StringBuilder envelope = new StringBuilder(mainEntry + "[" + JsonUtility.ToJson(Instance) + "]" + entrySeparator);

            foreach (FieldInfo field in nonSerializedSettingFields)
            {
                try
                {
                    string json = EditorJsonUtility.ToJson(field.GetValue(Instance));
                    envelope.Append(field.Name + "[" + json + "]" + entrySeparator);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            EditorPrefs.SetString(prefsKey, envelope.ToString());
        }

        /// <summary>
        /// Builds <see cref="instance"/> from <see cref="EditorPrefs"/>, or from the field
        /// initialisers if there is nothing stored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Entries are looked up by name rather than by position, so a settings block written by an
        /// older version simply leaves the newer settings at their defaults.
        /// </para>
        /// <para>
        /// SHIPPED BEHAVIOUR, preserved: two ways this can throw. A malformed envelope with a
        /// repeated entry name makes <see cref="Dictionary{TKey,TValue}.Add"/> throw rather than
        /// discarding the block. And the loop over <see cref="nonSerializedSettingFields"/> assumes
        /// the cache is already filled, which holds only because every path to here runs the
        /// instance constructor first -- <see cref="JsonUtility.FromJson{T}(string)"/> invokes the
        /// parameterless constructor, and the fallback constructs directly.
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
                instance = JsonUtility.FromJson<EditorSettings>(main);
            }

            if (instance == null)
            {
                instance = new EditorSettings();
            }

            foreach (FieldInfo field in nonSerializedSettingFields)
            {
                // Overwriting an existing object rather than creating one is what lets
                // EditorJsonUtility restore a Unity object reference at all; it has no way to
                // construct the target itself.
                object value = field.GetValue(instance) ?? Activator.CreateInstance(field.FieldType);
                if (entries.TryGetValue(field.Name, out string json))
                {
                    EditorJsonUtility.FromJsonOverwrite(json, value);
                }

                field.SetValue(instance, value);

                // Re-checked through the field because assigning a Unity object that was destroyed
                // during the overwrite stores something that compares equal to null.
                if (field.GetValue(instance) == null)
                {
                    field.SetValue(instance, Activator.CreateInstance(field.FieldType));
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
            instance = new EditorSettings();
            foreach (FieldInfo field in nonSerializedSettingFields)
            {
                field.SetValue(instance, Activator.CreateInstance(field.FieldType));
            }

            onCleared?.Invoke();
            SettingsPersistence.Save();
        }
    }
}
