// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorPrefsConfig.cs
//   [SpecialName] Item(string) / Item(string, object) -> this[string], line 235
//   [SpecialName] Item(Enum)   / Item(Enum, object)   -> this[Enum],   line 248
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference. The [DefaultMember("Item")] attribute on the decompiled class is what
// identifies those four methods as the two indexers.
//
// This type declares no key strings of its own: every one is supplied by whoever constructs it (see
// prefsKey and settingTypes). The shipped ControllerEditor assembly contains no construction site --
// its own window settings are persisted by a separate mechanism under the EditorPrefs key
// "yOk0XCnENLMO6DIF8cYpSg==SettingsJSON" -- so the format documented below is the whole of the
// on-disk contract this type is responsible for.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// A small named settings block persisted in <see cref="EditorPrefs"/>, able to draw itself as a
    /// list of fields with Revert and Save.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The block is described rather than declared: the caller passes a set of names with a
    /// <see cref="Type"/> and an optional default and tooltip for each, and the values are held in a
    /// dictionary keyed by name. That is what lets one class both persist and draw an arbitrary
    /// settings block, at the cost of every read being an unchecked cast — see
    /// <see cref="GetValue{T}(string)"/>.
    /// </para>
    /// <para>
    /// Only <see cref="bool"/>, <see cref="int"/>, <see cref="float"/> and <see cref="string"/> are
    /// understood. A setting of any other type is silently skipped when loading, drawing and
    /// defaulting alike, so it will be missing from <c>values</c> and reading it throws.
    /// </para>
    /// <para>
    /// The persisted form is a single EditorPrefs string under <see cref="prefsKey"/>, holding a flat
    /// JSON-like object of <c>"name":"value"</c> pairs where every value is quoted regardless of
    /// type: <c>{"showAdvanced":"True","scale":"1.5"}</c>. Values are written with
    /// <see cref="object.ToString"/> and read back by <see cref="JsonValue"/>, which
    /// matches booleans case-insensitively against "true" and parses numbers with
    /// <see cref="float.TryParse(string, out float)"/>. Both sides use the current culture, so a
    /// block saved under a decimal-comma locale reads back as its default elsewhere. Neither side
    /// escapes anything, so a string setting containing a quote or a brace corrupts the block.
    /// </para>
    /// </remarks>
    internal class EditorPrefsConfig
    {
        /// <summary>
        /// Saves the config when the user changed any control drawn inside the scope, for callers
        /// that draw the settings themselves instead of using <see cref="DrawDefault"/>.
        /// </summary>
        /// <remarks>
        /// The inner change-check scope is deliberately not disposed, as in the shipped build:
        /// reading <see cref="EditorGUI.ChangeCheckScope.changed"/> is what ends the change check, so
        /// the begin/end pairing is already balanced by the time <see cref="Dispose"/> returns.
        /// </remarks>
        internal class PrefsChangeScope : IDisposable
        {
            private readonly EditorPrefsConfig config;

            private readonly EditorGUI.ChangeCheckScope changeCheck;

            public PrefsChangeScope(EditorPrefsConfig config)
            {
                this.config = config;
                changeCheck = new EditorGUI.ChangeCheckScope();
            }

            public void Dispose()
            {
                if (changeCheck.changed)
                {
                    config.Save();
                }
            }
        }

        /// <summary>The declared settings: name to value type. Also defines which names exist.</summary>
        internal readonly Dictionary<string, Type> settingTypes;

        /// <summary>Per-setting defaults; a name absent here falls back to its type's zero value.</summary>
        internal readonly Dictionary<string, object> defaultValues;

        internal readonly Dictionary<string, string> tooltips;

        /// <summary>The EditorPrefs key the whole block is stored under.</summary>
        internal readonly string prefsKey;

        internal readonly Action drawAction;

        internal IDictionary<string, object> values;

        /// <summary>Whether the in-memory values differ from what was last saved or loaded.</summary>
        internal bool isDirty;

        internal bool hasTooltips;

        private readonly HashSet<string> hiddenKeys = new HashSet<string>();

        /// <summary>
        /// Describes the block with enum members instead of strings, so that call sites are checked
        /// by the compiler. The names are the enum member names, which is what ends up on disk.
        /// </summary>
        internal EditorPrefsConfig(string prefsKey, Dictionary<Enum, Type> settingTypes, Dictionary<Enum, object> defaultValues, Dictionary<Enum, string> tooltips = null, Action drawAction = null)
            : this(prefsKey,
                settingTypes.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                defaultValues.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                tooltips?.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                drawAction)
        {
        }

        internal EditorPrefsConfig(string prefsKey, Dictionary<string, Type> settingTypes, Dictionary<string, object> defaultValues, Dictionary<string, string> tooltips = null, Action drawAction = null)
        {
            this.prefsKey = prefsKey;
            this.drawAction = drawAction ?? new Action(DrawDefault);
            this.settingTypes = settingTypes;
            this.defaultValues = defaultValues;
            this.tooltips = tooltips;
            hasTooltips = tooltips != null;
            Load();
        }

        internal void Draw()
        {
            drawAction();
        }

        /// <summary>
        /// Draws one field per visible setting, followed by Revert and Save, which are enabled only
        /// once something has been edited.
        /// </summary>
        /// <remarks>
        /// Edits are held in memory and written only on Save, so the Revert button has something to
        /// go back to. The loop runs over a snapshot of the dictionary because it assigns into
        /// <c>values</c> as it goes.
        /// </remarks>
        internal void DrawDefault()
        {
            KeyValuePair<string, object>[] snapshot = values.ToArray();

            EditorGUI.BeginChangeCheck();
            foreach (KeyValuePair<string, object> setting in snapshot)
            {
                string key = setting.Key;
                if (hiddenKeys.Contains(key))
                {
                    continue;
                }

                object value = setting.Value;
                GUIContent label = new GUIContent(
                    ObjectNames.NicifyVariableName(key),
                    !hasTooltips || !tooltips.TryGetValue(key, out string tooltip) ? string.Empty : tooltip);

                if (!settingTypes.TryGetValue(key, out Type type) || type == null)
                {
                    continue;
                }

                if (type == typeof(bool))
                {
                    values[key] = EditorGUILayout.Toggle(label, (bool)value);
                }
                else if (type == typeof(int))
                {
                    values[key] = EditorGUILayout.IntField(label, (int)value);
                }
                else if (type == typeof(float))
                {
                    values[key] = EditorGUILayout.FloatField(label, (float)value);
                }
                else if (type == typeof(string))
                {
                    values[key] = EditorGUILayout.TextField(label, (string)value);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                isDirty = true;
            }

            using (new GUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!isDirty))
                {
                    if (EditorUtils.Button("Revert"))
                    {
                        Load();
                    }

                    if (EditorUtils.Button("Save"))
                    {
                        Save();
                    }
                }
            }
        }

        /// <summary>
        /// Omits a setting from <see cref="DrawDefault"/> without removing it. Hiding affects the
        /// default drawer only: the value is still loaded, saved and readable.
        /// </summary>
        internal void Hide(string key)
        {
            hiddenKeys.Add(key);
        }

        /// <inheritdoc cref="Hide(string)"/>
        internal void Hide(params string[] keys)
        {
            hiddenKeys.UnionWith(keys);
        }

        /// <inheritdoc cref="Hide(string)"/>
        internal void Hide(Enum key)
        {
            Hide(key.ToString());
        }

        /// <inheritdoc cref="Hide(string)"/>
        internal void Hide(params Enum[] keys)
        {
            Hide(keys.Select(k => k.ToString()).ToArray());
        }

        /// <summary>Undoes <see cref="Hide(string)"/>.</summary>
        internal void Show(string key)
        {
            hiddenKeys.Remove(key);
        }

        /// <inheritdoc cref="Show(string)"/>
        internal void Show(params string[] keys)
        {
            hiddenKeys.ExceptWith(keys);
        }

        /// <inheritdoc cref="Show(string)"/>
        internal void Show(Enum key)
        {
            Show(key.ToString());
        }

        /// <inheritdoc cref="Show(string)"/>
        internal void Show(params Enum[] keys)
        {
            Show(keys.Select(k => k.ToString()).ToArray());
        }

        private void LoadDefaults()
        {
            values = new Dictionary<string, object>();
            foreach (string key in settingTypes.Keys)
            {
                LoadDefault(key);
            }

            isDirty = false;
        }

        /// <summary>
        /// Resets one setting to the caller-supplied default, or to its type's zero value if there is
        /// none. A setting of an unsupported type is left absent from <c>values</c> entirely.
        /// </summary>
        private void LoadDefault(string key)
        {
            if (!settingTypes.TryGetValue(key, out Type type))
            {
                return;
            }

            if (defaultValues != null && defaultValues.TryGetValue(key, out object defaultValue))
            {
                values[key] = defaultValue;
            }
            else if (TryGetDefaultValue(type, out object zero))
            {
                values[key] = zero;
            }
        }

        /// <remarks>
        /// The boxed zero is produced by hand rather than by <see cref="Activator.CreateInstance(Type)"/>
        /// so that an unsupported type reports failure instead of silently boxing something the rest
        /// of the class cannot draw or persist.
        /// </remarks>
        private bool TryGetDefaultValue(Type type, out object value)
        {
            value = null;
            if (type != null)
            {
                if (type == typeof(bool))
                {
                    value = false;
                    return true;
                }

                if (type == typeof(int))
                {
                    value = 0;
                    return true;
                }

                if (type == typeof(float))
                {
                    value = 0f;
                    return true;
                }

                if (type == typeof(string))
                {
                    value = string.Empty;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The boxed value of a setting. Assigning marks the config dirty but does not save; call
        /// <see cref="Save"/> to persist.
        /// </summary>
        internal object this[string key]
        {
            get
            {
                return values[key];
            }
            set
            {
                values[key] = value;
                isDirty = true;
            }
        }

        /// <inheritdoc cref="this[string]"/>
        internal object this[Enum key]
        {
            get
            {
                return values[key.ToString()];
            }
            set
            {
                values[key.ToString()] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Reads a setting already cast to its declared type. <typeparamref name="T"/> is not checked
        /// against <see cref="settingTypes"/>, so a mismatch throws at the cast.
        /// </summary>
        internal T GetValue<T>(string key)
        {
            return (T)values[key];
        }

        /// <inheritdoc cref="GetValue{T}(string)"/>
        internal T GetValue<T>(Enum key)
        {
            return (T)values[key.ToString()];
        }

        internal void Save()
        {
            List<(string, string)> entries = new List<(string, string)>();
            foreach (KeyValuePair<string, object> setting in values)
            {
                entries.Add((setting.Key, setting.Value.ToString()));
            }

            EditorPrefs.SetString(prefsKey, Json.ToJsonObject(entries));
            isDirty = false;
        }

        /// <summary>
        /// Reloads every setting from EditorPrefs, discarding unsaved edits.
        /// </summary>
        /// <remarks>
        /// Settings are read by name from the stored block rather than by position, and any name the
        /// block does not carry falls back to its default — so adding a setting to an existing block
        /// does not invalidate the ones already saved. A block that fails to parse is discarded in
        /// full rather than left half-loaded.
        /// </remarks>
        internal void Load()
        {
            if (!EditorPrefs.HasKey(prefsKey))
            {
                LoadDefaults();
                return;
            }

            string stored = EditorPrefs.GetString(prefsKey);
            try
            {
                values = new Dictionary<string, object>();

                JsonObject json = new JsonObject(stored);
                foreach (KeyValuePair<string, Type> setting in settingTypes)
                {
                    string key = setting.Key;
                    Type type = setting.Value;

                    JsonValue value = json[key];
                    if (!value.hasValue)
                    {
                        LoadDefault(key);
                        continue;
                    }

                    if (type == null)
                    {
                        continue;
                    }

                    if (type == typeof(bool))
                    {
                        values[key] = value.boolValue;
                    }
                    else if (type == typeof(int))
                    {
                        // Stored numbers are parsed as float, so an int setting truncates rather
                        // than failing on a value that was written with a decimal point.
                        values[key] = (int)value.floatValue;
                    }
                    else if (type == typeof(float))
                    {
                        values[key] = value.floatValue;
                    }
                    else if (type == typeof(string))
                    {
                        values[key] = value.stringValue;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings from {prefsKey}:\n{e}");
                LoadDefaults();
            }

            isDirty = false;
        }
    }
}
