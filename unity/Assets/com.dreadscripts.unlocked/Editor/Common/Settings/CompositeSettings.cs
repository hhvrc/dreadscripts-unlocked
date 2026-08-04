// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of these types,
// nested inside their respective settings classes.
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, class EditorSettings
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, class ADOSettings
//
// Member mapping. Each entry names the decompiled member, then the ported member, then the
// ControllerEditor line and the ADOverhaul2022 line it was read from:
//   VectorSetting                                       -> VectorSetting,                  768 / 1065
//   VectorSetting.DeleteDefinition/CreateDefinition, GetValue/SetValue -> VectorSetting.value, 786 / 1083
//   VectorSetting.IncludeDefinition / Initialize        -> VectorSetting.Initialize,       810 / 1110
//   VectorSetting.RunDefinition / DrawVector2FieldContent -> DrawVector2Field(GUIContent, ...), 834 / 1134
//   VectorSetting.CloneDefinition / DrawVector2Field    -> DrawVector2Field(string, ...),  847 / 1147
//   VectorSetting.LoginDefinition / DrawVector3FieldContent -> DrawVector3Field(GUIContent, ...), 852 / 1152
//   VectorSetting.ReflectDefinition / DrawVector3Field  -> DrawVector3Field(string, ...),  857 / 1157
//   ColorSetting                                        -> ColorSetting,                   944 / 1222
//   ColorSetting.GetValue/SetValue                      -> ColorSetting.value,             961 / 1239
//   ColorSetting.Draw(string) / Draw                    -> ColorSetting.Draw(string, ...), 998 / 1276
//   ColorSetting.Draw(GUIContent) / DrawContent         -> ColorSetting.Draw(GUIContent, ...), 1003 / 1281
//   ObjectReferenceSetting                              -> ObjectReferenceSetting,        1022 / 1300
//   ObjectReferenceSetting.GetValue/SetValue, ForgotPage/UpdatePage -> .value,            1043 / 1321
//   ObjectReferenceSetting.Draw(string) / InvokePage    -> Draw(string, ...),             1083 / 1361
//   ObjectReferenceSetting.Draw(GUIContent) / CustomizePage -> Draw(GUIContent, ...),     1088 / 1366
//   ObjectReferenceSetting.LoadAsset / ConcatPage       -> LoadAsset<T>,                  1100 / 1378
//   ObjectReferenceSetting.GetValue / MapPage           -> GetValue<T>,                   1122 / 1400
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. Where the two sources named the same method differently,
// ADOverhaul's names are used, and the string/GUIContent pairs are collapsed into overloads. The
// two generic entries are written without their type parameter, because the mapping column is
// matched mechanically and cannot contain angle brackets; both are the single-type-argument
// generics they are in the source.
//
// NOTES
// The two copies are behaviourally identical here; they differ only in which product's icon table
// the revert button came from, which is now SettingBase.DrawResetButton.
//
// Audit status: UNAUDITED

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// A persisted <see cref="Vector3"/> setting, drawable as either a two- or three-component
    /// field.
    /// </summary>
    /// <remarks>
    /// The three components are stored as separate floats and recombined on demand, because that is
    /// what survives <see cref="JsonUtility"/> without the vector itself having to be a serialized
    /// field.
    /// </remarks>
    [Serializable]
    internal class VectorSetting : SettingBase
    {
        [SerializeField]
        private float _valueX;

        [SerializeField]
        private float _valueY;

        [SerializeField]
        private float _valueZ;

        internal Action onChange;

        internal bool isCached;

        internal Vector3 cachedValue;

        /// <remarks>
        /// <para>
        /// The cache is not just an optimisation: it is where the vector actually lives once it has
        /// been read, and the setter's change test compares against it rather than against the three
        /// stored floats.
        /// </para>
        /// <para>
        /// That makes the first write after construction or deserialisation compare against
        /// <see cref="Vector3.zero"/> instead of against the real value, so assigning zero to a
        /// setting that was never read is silently dropped. Ported as shipped; a caller that needs
        /// the write to land can read the setting first.
        /// </para>
        /// </remarks>
        internal Vector3 value
        {
            get
            {
                // The ADOverhaul 2022 decompilation renders this as an infinite loop; the 2019
                // build of the same code, and ControllerEditor's copy, both have the plain `if`.
                if (!isCached)
                {
                    isCached = true;
                    cachedValue = new Vector3(_valueX, _valueY, _valueZ);
                }

                return cachedValue;
            }
            set
            {
                if (cachedValue != value)
                {
                    cachedValue = value;
                    _valueX = value.x;
                    _valueY = value.y;
                    _valueZ = value.z;
                    onChange?.Invoke();
                    SettingsPersistence.Save();
                }
            }
        }

        /// <remarks>
        /// Shared by the constructors rather than chained through one of them, as shipped. Note that
        /// it writes the components without priming the cache, which is what leaves the first write
        /// comparing against zero — see <see cref="value"/>.
        /// </remarks>
        internal void Initialize(Vector3 defaultValue, Action onChange)
        {
            this.defaultValue = defaultValue;
            this.onChange = onChange;
            _valueX = defaultValue.x;
            _valueY = defaultValue.y;
            _valueZ = defaultValue.z;
        }

        internal VectorSetting(Vector3 defaultValue, Action onChange = null)
        {
            Initialize(defaultValue, onChange);
        }

        internal VectorSetting(float x, float y, float z, Action onChange = null)
        {
            Initialize(new Vector3(x, y, z), onChange);
        }

        /// <summary>Creates a setting whose Z stays zero, for the many settings that are really 2D.</summary>
        internal VectorSetting(float x, float y, Action onChange = null)
        {
            Initialize(new Vector3(x, y), onChange);
        }

        /// <summary>
        /// Draws X and Y only, with the label as a separate fixed-width column.
        /// </summary>
        /// <remarks>
        /// The label is drawn by hand rather than passed to the field because
        /// <see cref="EditorGUILayout.Vector2Field(GUIContent, Vector2, GUILayoutOption[])"/> puts
        /// its label on a line of its own, which is too tall for a settings row.
        /// </remarks>
        internal void DrawVector2Field(GUIContent label, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.MaxWidth(117f));

                // Assigning a Vector2 back into a Vector3 setting zeroes Z, which is correct here
                // only because these settings are the two-component ones.
                value = EditorGUILayout.Vector2Field(GUIContent.none, value, options);

                if (DrawResetButton())
                {
                    Reset();
                }
            }
        }

        /// <inheritdoc cref="DrawVector2Field(GUIContent, GUILayoutOption[])"/>
        internal void DrawVector2Field(string label, params GUILayoutOption[] options)
        {
            DrawVector2Field(new GUIContent(label), options);
        }

        /// <summary>Draws all three components. Has no revert button, unlike every other setting.</summary>
        internal void DrawVector3Field(GUIContent label, params GUILayoutOption[] options)
        {
            value = EditorGUILayout.Vector3Field(label, value, options);
        }

        /// <inheritdoc cref="DrawVector3Field(GUIContent, GUILayoutOption[])"/>
        internal void DrawVector3Field(string label, params GUILayoutOption[] options)
        {
            DrawVector3Field(new GUIContent(label), options);
        }

        internal override void Reset()
        {
            value = (Vector3)defaultValue;
        }

        public static implicit operator Vector2(VectorSetting setting)
        {
            return setting.value;
        }
    }

    /// <summary>A persisted <see cref="Color"/> setting, stored as its four channels.</summary>
    [Serializable]
    internal class ColorSetting : SettingBase
    {
        internal readonly Action onChange;

        [SerializeField]
        private float r;

        [SerializeField]
        private float g;

        [SerializeField]
        private float b;

        [SerializeField]
        private float a;

        /// <remarks>
        /// Unlike the other settings, the setter has no change test, so every assignment fires
        /// <see cref="onChange"/> and saves — including the one <see cref="Draw(GUIContent, bool, GUILayoutOption[])"/>
        /// makes on each repaint. In practice colour settings are drawn inside a
        /// <see cref="SettingsChangeScope"/>, whose deferral is what stops that from reaching
        /// EditorPrefs. Ported as shipped.
        /// </remarks>
        internal Color value
        {
            get
            {
                return new Color(r, g, b, a);
            }
            set
            {
                r = value.r;
                g = value.g;
                b = value.b;
                a = value.a;
                onChange?.Invoke();
                SettingsPersistence.Save();
            }
        }

        internal ColorSetting(float r, float g, float b, float a = 1f, Action onChange = null)
        {
            defaultValue = new Color(r, g, b, a);
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            this.onChange = onChange;
        }

        internal ColorSetting(Color defaultValue, Action onChange = null)
        {
            this.defaultValue = defaultValue;
            r = defaultValue.r;
            g = defaultValue.g;
            b = defaultValue.b;
            a = defaultValue.a;
            this.onChange = onChange;
        }

        internal void Draw(string label, bool showReset = true, params GUILayoutOption[] options)
        {
            Draw(new GUIContent(label), showReset, options);
        }

        /// <inheritdoc cref="Draw(string, bool, GUILayoutOption[])"/>
        internal void Draw(GUIContent label, bool showReset = true, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                value = EditorGUILayout.ColorField(label, value, options);
                if (showReset && DrawResetButton())
                {
                    Reset();
                }
            }
        }

        internal override void Reset()
        {
            value = (Color)defaultValue;
        }
    }

    /// <summary>
    /// A persisted reference to a project asset.
    /// </summary>
    /// <remarks>
    /// The asset is stored as its GUID plus local file id rather than as a path, so that moving or
    /// renaming it does not break the setting. The local id is what allows a sub-asset — one
    /// animation clip inside an FBX, say — to be named as precisely as a main asset; it is zero for
    /// main assets, and the lookup takes a cheaper path in that case.
    /// </remarks>
    [Serializable]
    internal class ObjectReferenceSetting : SettingBase
    {
        internal readonly Action onChange;

        /// <summary>Constrains the object field; not enforced on the stored id.</summary>
        private readonly Type objectType;

        [SerializeField]
        internal string guid;

        [SerializeField]
        internal long localID;

        private string defaultGuid;

        private long defaultLocalID;

        private bool isCached;

        private UnityEngine.Object cachedObject;

        /// <remarks>
        /// Resolution is deferred to the first read and then cached, because loading assets while
        /// the settings block is being deserialised — which happens during a domain reload — is not
        /// reliable. The cache is never invalidated, so an asset deleted during the session keeps
        /// reading back until the next reload.
        /// </remarks>
        internal UnityEngine.Object value
        {
            get
            {
                if (!isCached)
                {
                    isCached = true;
                    cachedObject = LoadAsset<UnityEngine.Object>(guid, localID);
                }

                return cachedObject;
            }
            set
            {
                // Compared against the cache directly rather than through the property, so that
                // assigning to a setting that has never been read does not force a load first.
                if (cachedObject != value)
                {
                    cachedObject = value;
                    if (value == null)
                    {
                        guid = string.Empty;
                        localID = 0L;
                    }
                    else
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(value, out guid, out localID);
                    }

                    onChange?.Invoke();
                    SettingsPersistence.Save();
                }
            }
        }

        internal ObjectReferenceSetting(Type objectType, string defaultGuid = "", long defaultLocalID = 0L, Action onChange = null)
        {
            this.objectType = objectType;
            this.defaultGuid = defaultGuid;
            this.defaultLocalID = defaultLocalID;
            guid = defaultGuid;
            localID = defaultLocalID;
            this.onChange = onChange;
        }

        internal void Draw(string label, bool showReset = true, params GUILayoutOption[] options)
        {
            Draw(new GUIContent(label), showReset, options);
        }

        /// <inheritdoc cref="Draw(string, bool, GUILayoutOption[])"/>
        internal void Draw(GUIContent label, bool showReset = true, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                // Scene objects are refused because a GUID and local id cannot name one.
                value = EditorGUILayout.ObjectField(label, value, objectType, false, options);
                if (showReset && DrawResetButton())
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Resolves a GUID and local file id back to the asset, or null when the asset is gone.
        /// </summary>
        private static T LoadAsset<T>(string guid, long localID) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return null;
            }

            if (localID == 0L)
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }

            // There is no API that loads a sub-asset by local id, so the whole file is loaded and
            // its objects are asked for their ids one at a time.
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid));
            foreach (UnityEngine.Object asset in assets)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long assetLocalID);
                if (assetLocalID == localID)
                {
                    return (T)asset;
                }
            }

            return null;
        }

        /// <summary>Reads the reference already cast. Throws when the stored asset is not a <typeparamref name="T"/>.</summary>
        internal T GetValue<T>() where T : UnityEngine.Object
        {
            return (T)value;
        }

        /// <remarks>
        /// Resets to the asset the default ids name, resolved now rather than remembered, so a
        /// default whose asset is missing resets to null instead of throwing.
        /// </remarks>
        internal override void Reset()
        {
            value = LoadAsset<UnityEngine.Object>(defaultGuid, defaultLocalID);
        }

        /// <summary>True when an asset is assigned and still resolves.</summary>
        public static implicit operator bool(ObjectReferenceSetting setting)
        {
            return setting.value;
        }
    }
}
