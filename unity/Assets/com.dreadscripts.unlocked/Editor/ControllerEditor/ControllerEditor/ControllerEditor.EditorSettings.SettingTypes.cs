// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// The typed settings held by EditorSettings. Export lines 507-1143, inside EditorSettings
// (lines 437-1578).
//
//   SettingBase              -> SettingBase,             line 1138 (name already in renames/)
//   BoolSetting              -> BoolSetting,             line 508
//     GetValue/SetValue      -> Value,                   lines 516/522   [SpecialName pair]
//     Toggle, Draw x2, DrawButton x2, onChange, Reset    -- names already in renames/
//   FloatSetting             -> FloatSetting,            line 585
//     ResetDefinition/FlushDefinition -> Value,          lines 593/599   [SpecialName pair]
//     identifierAlgo         -> onChanged,               line 590
//     VisitDefinition        -> Draw(string,...),        line 616
//     StartDefinition        -> Draw(GUIContent,...),    line 628
//     DefineDefinition       -> DrawWithLabelWidth(string,...),     line 621
//     ReadDefinition         -> DrawWithLabelWidth(GUIContent,...), line 644
//     SelectDefinition       -> DrawSlider(string,...),            line 651
//     RemoveDefinition       -> DrawSlider(GUIContent,...),        line 656
//     InstantiateDefinition  -> DrawNormalizedSlider(string,...),  line 671
//     AwakeDefinition        -> DrawNormalizedSlider(GUIContent,...), line 676
//   EnumSetting              -> EnumSetting,             line 698 (members already in renames/)
//   VectorSetting            -> VectorSetting,           line 768
//     _RegistryAlgo          -> onChanged,               line 779
//     _TagAlgo               -> isCached,                line 781
//     importerAlgo           -> cachedValue,             line 783
//     DeleteDefinition/CreateDefinition -> Value,        lines 786/797  [SpecialName pair]
//     IncludeDefinition      -> Initialize,              line 810
//     RunDefinition          -> DrawVector2Field(GUIContent,...), line 834
//     CloneDefinition        -> DrawVector2Field(string,...),     line 847
//     LoginDefinition        -> DrawVector3Field(GUIContent,...), line 852
//     ReflectDefinition      -> DrawVector3Field(string,...),     line 857
//   StringSetting            -> StringSetting,           line 874 (members already in renames/)
//   ColorSetting             -> ColorSetting,            line 944 (members already in renames/)
//   ObjectReferenceSetting   -> ObjectReferenceSetting,  line 1022 (members already in renames/)
//
// The five [SpecialName] GetValue/SetValue pairs above are property accessors whose Property rows
// the obfuscator dropped; they are restored as properties here, so export/ will keep showing them
// as GetValue()/SetValue() methods. The VectorSetting member names are taken from ADOverhaul's map,
// where the same class is already named (RE_NOTES: converge the typed-setting family).
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// EnumSetting.IntValue carries a [SerializeField] attribute in export. Dropped: UnityEngine's
// SerializeField is AttributeTargets.Field, so it cannot legally sit on a property — it is a
// decompiler mis-attribution, and the serialized state is the inherited FloatSetting._value field.
//
// DEOBF-BUG(resolved) -- StringSetting.Value setter, export line 888.
//   export/ renders the body as `if (_value != value) { while (true) { _value = value; } }`, an
//   assignment that can never terminate and that drops the two statements every sibling setter
//   ends with. ADOverhaul2022's copy of the same class decompiles cleanly
//   (export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs:1188) and reads
//   `if (_value != ident) { _value = ident; onChanged?.Invoke(); Save(); }`. Corroborated by
//   `onChanged` otherwise being assigned in the constructor and read nowhere. Ported as the
//   ADOverhaul form. export/ will keep showing the loop until de4dot changes; do not "fix" it back.
//
// DEOBF-BUG(resolved) -- FloatSetting.DrawSlider, export line 663.
//   export/ renders the reset button as `while (EditorUtils.CallQueue(...)) { Reset(); }`.
//   ADOverhaul2022's DrawSliderContent (ADOverhaul.cs:962) has the same call as a plain `if`, as do
//   the four other draw methods in this very class. Ported as `if`.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04), except the two DEOBF-BUG
// sites above, which deviate deliberately.

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private partial class EditorSettings
        {
            /// <summary>
            /// Common base for every persisted setting: it remembers the value the setting shipped
            /// with, so the reset button beside it has something to restore.
            /// </summary>
            internal abstract class SettingBase
            {
                internal object defaultValue;

                internal abstract void Reset();
            }

            [Serializable]
            internal class BoolSetting : SettingBase
            {
                [SerializeField]
                private bool _value;

                internal readonly Action onChange;

                internal bool Value
                {
                    get => _value;
                    set
                    {
                        if (_value != value)
                        {
                            _value = value;
                            onChange?.Invoke();
                            SaveSettings();
                        }
                    }
                }

                internal BoolSetting(bool defaultValue, Action onChange = null)
                {
                    this.defaultValue = defaultValue;
                    _value = defaultValue;
                    this.onChange = onChange;
                }

                internal void Toggle()
                {
                    Value = !_value;
                }

                internal void Draw(string label, GUIStyle style = null, params GUILayoutOption[] options)
                {
                    Draw(new GUIContent(label), style, options);
                }

                internal void Draw(GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
                {
                    if (style == null)
                    {
                        style = EditorStyles.toggle;
                    }

                    Value = EditorGUILayout.Toggle(label, Value, style, options);
                }

                internal void DrawButton(string label, string offLabel = null, bool toolbarStyle = false,
                    Color? onColor = null, Color? offColor = null, params GUILayoutOption[] options)
                {
                    DrawButton(
                        !string.IsNullOrEmpty(label) ? new GUIContent(label) : GUIContent.none,
                        !string.IsNullOrEmpty(offLabel) ? new GUIContent(offLabel) : GUIContent.none,
                        toolbarStyle, onColor, offColor, options);
                }

                /// <summary>
                /// Draws the setting as a pressed/unpressed button. <paramref name="offLabel"/>, when
                /// given, replaces the label while the setting is off.
                /// </summary>
                internal void DrawButton(GUIContent label, GUIContent offLabel = null, bool toolbarStyle = false,
                    Color? onColor = null, Color? offColor = null, params GUILayoutOption[] options)
                {
                    onColor = onColor ?? GUI.backgroundColor;
                    offColor = offColor ?? GUI.backgroundColor;

                    Color previous = GUI.backgroundColor;
                    GUI.backgroundColor = Value ? onColor.Value : offColor.Value;
                    Value = GUILayout.Toggle(Value, !Value && offLabel != null ? offLabel : label,
                        toolbarStyle ? EditorStyles.toolbarButton : GUI.skin.button, options);
                    GUI.backgroundColor = previous;
                }

                public static implicit operator bool(BoolSetting setting)
                {
                    return setting._value;
                }

                internal override void Reset()
                {
                    Value = (bool)defaultValue;
                }
            }

            [Serializable]
            internal class FloatSetting : SettingBase
            {
                [SerializeField]
                private float _value;

                internal readonly Action onChanged;

                internal float Value
                {
                    get => _value;
                    set
                    {
                        if (_value != value)
                        {
                            _value = value;
                            onChanged?.Invoke();
                            SaveSettings();
                        }
                    }
                }

                internal FloatSetting(float defaultValue, Action onChanged = null)
                {
                    this.defaultValue = defaultValue;
                    _value = defaultValue;
                    this.onChanged = onChanged;
                }

                internal void Draw(string label, bool drawResetButton = true, GUIStyle style = null,
                    params GUILayoutOption[] options)
                {
                    Draw(new GUIContent(label), drawResetButton, style, options);
                }

                internal void Draw(GUIContent label, bool drawResetButton = true, GUIStyle style = null,
                    params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (style == null)
                        {
                            style = EditorStyles.numberField;
                        }

                        Value = EditorGUILayout.FloatField(label, Value, style, options);
                        if (drawResetButton && EditorUtils.IconButton(EditorUtils.contents.reset))
                        {
                            Reset();
                        }
                    }
                }

                internal void DrawWithLabelWidth(string label, float labelWidth, bool drawResetButton = true,
                    GUIStyle style = null, params GUILayoutOption[] options)
                {
                    EditorGUIUtility.labelWidth = labelWidth;
                    Draw(new GUIContent(label), drawResetButton, style, options);
                    EditorGUIUtility.labelWidth = 0f;
                }

                internal void DrawWithLabelWidth(GUIContent label, float labelWidth, bool drawResetButton = true,
                    GUIStyle style = null, params GUILayoutOption[] options)
                {
                    EditorGUIUtility.labelWidth = labelWidth;
                    Draw(label, drawResetButton, style, options);
                    EditorGUIUtility.labelWidth = 0f;
                }

                internal void DrawSlider(string label, float min, float max, bool drawResetButton = true,
                    params GUILayoutOption[] options)
                {
                    DrawSlider(new GUIContent(label), min, max, drawResetButton, options);
                }

                internal void DrawSlider(GUIContent label, float min, float max, bool drawResetButton = true,
                    params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Value = EditorGUILayout.Slider(label, Value, min, max, options);

                        // DEOBF-BUG(resolved): export/ has this as `while (...)`. See the file header.
                        if (drawResetButton && EditorUtils.IconButton(EditorUtils.contents.reset))
                        {
                            Reset();
                        }
                    }
                }

                internal void DrawNormalizedSlider(string label, bool drawResetButton = true,
                    params GUILayoutOption[] options)
                {
                    DrawNormalizedSlider(new GUIContent(label), drawResetButton, options);
                }

                internal void DrawNormalizedSlider(GUIContent label, bool drawResetButton = true,
                    params GUILayoutOption[] options)
                {
                    DrawSlider(label, 0f, 1f, drawResetButton, options);
                }

                internal override void Reset()
                {
                    Value = (float)defaultValue;
                }

                public static implicit operator int(FloatSetting setting)
                {
                    return (int)setting._value;
                }

                public static implicit operator float(FloatSetting setting)
                {
                    return setting._value;
                }
            }

            /// <summary>
            /// A <see cref="FloatSetting"/> addressed as an int, so an enum can be persisted without
            /// a serialized field per enum type.
            /// </summary>
            [Serializable]
            internal class EnumSetting : FloatSetting
            {
                internal int IntValue
                {
                    get => (int)Value;
                    set => Value = value;
                }

                internal EnumSetting(int defaultValue, Action onChanged = null)
                    : base(defaultValue, onChanged)
                {
                }

                internal T GetEnumValue<T>() where T : Enum
                {
                    return (T)(object)IntValue;
                }

                internal void DrawIntField(GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
                {
                    if (style == null)
                    {
                        style = EditorStyles.numberField;
                    }

                    IntValue = EditorGUILayout.IntField(label, IntValue, style, options);
                }

                internal void DrawIntField(string label, GUIStyle style = null, params GUILayoutOption[] options)
                {
                    DrawIntField(new GUIContent(label), style, options);
                }

                internal void DrawEnumPopup<T>(GUIContent label, bool flags = false, GUIStyle style = null,
                    params GUILayoutOption[] options) where T : Enum
                {
                    if (style == null)
                    {
                        style = EditorStyles.popup;
                    }

                    IntValue = flags
                        ? (int)(object)EditorGUILayout.EnumFlagsField(label, (T)(object)IntValue, style, options)
                        : (int)(object)EditorGUILayout.EnumPopup(label, (T)(object)IntValue, style, options);
                }

                internal void DrawEnumPopup<T>(string label, bool flags = false, GUIStyle style = null,
                    params GUILayoutOption[] options) where T : Enum
                {
                    DrawEnumPopup<T>(new GUIContent(label), flags, style, options);
                }

                internal static EnumSetting FromEnum<T>(T defaultValue, Action onChanged = null) where T : Enum
                {
                    return new EnumSetting((int)(object)defaultValue, onChanged);
                }

                public static implicit operator int(EnumSetting setting)
                {
                    return setting.IntValue;
                }

                public static implicit operator float(EnumSetting setting)
                {
                    return setting.IntValue;
                }
            }

            /// <summary>
            /// A <see cref="Vector3"/> persisted as three floats, because Unity's JSON serializer is
            /// asked to write the setting object itself. The composed vector is cached on first read
            /// so the common case does not rebuild it per repaint.
            /// </summary>
            [Serializable]
            internal class VectorSetting : SettingBase
            {
                [SerializeField]
                private float _valueX;

                [SerializeField]
                private float _valueY;

                [SerializeField]
                private float _valueZ;

                internal Action onChanged;

                internal bool isCached;

                internal Vector3 cachedValue;

                internal Vector3 Value
                {
                    get
                    {
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
                            onChanged?.Invoke();
                            SaveSettings();
                        }
                    }
                }

                internal void Initialize(Vector3 defaultValue, Action onChanged)
                {
                    this.defaultValue = defaultValue;
                    this.onChanged = onChanged;
                    _valueX = defaultValue.x;
                    _valueY = defaultValue.y;
                    _valueZ = defaultValue.z;
                }

                internal VectorSetting(Vector3 defaultValue, Action onChanged = null)
                {
                    Initialize(defaultValue, onChanged);
                }

                internal VectorSetting(float x, float y, float z, Action onChanged = null)
                {
                    Initialize(new Vector3(x, y, z), onChanged);
                }

                internal VectorSetting(float x, float y, Action onChanged = null)
                {
                    Initialize(new Vector3(x, y), onChanged);
                }

                internal void DrawVector2Field(GUIContent label, params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(label, GUILayout.MaxWidth(117f));
                        Value = EditorGUILayout.Vector2Field(GUIContent.none, Value, options);
                        if (GUILayout.Button(EditorUtils.contents.reset, EditorUtils.styles.tightLabel,
                                GUILayout.Width(18f), GUILayout.Height(18f)))
                        {
                            Reset();
                        }
                    }
                }

                internal void DrawVector2Field(string label, params GUILayoutOption[] options)
                {
                    DrawVector2Field(new GUIContent(label), options);
                }

                internal void DrawVector3Field(GUIContent label, params GUILayoutOption[] options)
                {
                    Value = EditorGUILayout.Vector3Field(label, Value, options);
                }

                internal void DrawVector3Field(string label, params GUILayoutOption[] options)
                {
                    DrawVector3Field(new GUIContent(label), options);
                }

                internal override void Reset()
                {
                    Value = (Vector3)defaultValue;
                }

                public static implicit operator Vector2(VectorSetting setting)
                {
                    return setting.Value;
                }
            }

            [Serializable]
            internal class StringSetting : SettingBase
            {
                [SerializeField]
                private string _value;

                internal readonly Action onChanged;

                internal string Value
                {
                    get => _value;
                    set
                    {
                        // DEOBF-BUG(resolved): export/ has an unterminating `while` here and drops the
                        // two calls below. See the file header.
                        if (_value != value)
                        {
                            _value = value;
                            onChanged?.Invoke();
                            SaveSettings();
                        }
                    }
                }

                internal StringSetting(string defaultValue = "", Action onChanged = null)
                {
                    this.defaultValue = defaultValue;
                    _value = defaultValue;
                    this.onChanged = onChanged;
                }

                internal void Draw(string label, bool drawResetButton = true, bool delayed = true,
                    GUIStyle style = null, params GUILayoutOption[] options)
                {
                    Draw(new GUIContent(label), drawResetButton, delayed, style, options);
                }

                internal void Draw(GUIContent label, bool drawResetButton = true, bool delayed = true,
                    GUIStyle style = null, params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (style == null)
                        {
                            style = EditorStyles.textField;
                        }

                        Value = delayed
                            ? EditorGUILayout.DelayedTextField(label, Value, style, options)
                            : EditorGUILayout.TextField(label, Value, style, options);

                        if (drawResetButton && EditorUtils.IconButton(EditorUtils.contents.reset))
                        {
                            Reset();
                        }
                    }
                }

                internal override void Reset()
                {
                    Value = (string)defaultValue;
                }

                public override string ToString()
                {
                    return Value;
                }

                public static implicit operator string(StringSetting setting)
                {
                    return setting._value;
                }
            }

            /// <summary>
            /// A <see cref="Color"/> persisted as four floats, for the same reason
            /// <see cref="VectorSetting"/> is three. Unlike the others this setter does not compare
            /// first — it fires the change callback and saves on every write.
            /// </summary>
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

                internal Color Value
                {
                    get => new Color(r, g, b, a);
                    set
                    {
                        r = value.r;
                        g = value.g;
                        b = value.b;
                        a = value.a;
                        onChange?.Invoke();
                        SaveSettings();
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

                internal void Draw(string label, bool drawResetButton = true, params GUILayoutOption[] options)
                {
                    Draw(new GUIContent(label), drawResetButton, options);
                }

                internal void Draw(GUIContent label, bool drawResetButton = true, params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Value = EditorGUILayout.ColorField(label, Value, options);
                        if (drawResetButton && EditorUtils.IconButton(EditorUtils.contents.reset))
                        {
                            Reset();
                        }
                    }
                }

                internal override void Reset()
                {
                    Value = (Color)defaultValue;
                }
            }

            /// <summary>
            /// An asset reference persisted as a GUID plus a local file id, because a
            /// <see cref="Object"/> reference cannot survive a JSON round trip. The resolved object
            /// is cached, including a cached null, so a missing asset is not looked up per repaint.
            /// </summary>
            [Serializable]
            internal class ObjectReferenceSetting : SettingBase
            {
                internal readonly Action onChange;

                private readonly Type objectType;

                [SerializeField]
                internal string guid;

                [SerializeField]
                internal long localID;

                private string defaultGuid;

                private long defaultLocalID;

                private bool isCached;

                private Object cachedObject;

                internal Object Value
                {
                    get
                    {
                        if (!isCached)
                        {
                            isCached = true;
                            cachedObject = LoadAsset<Object>(guid, localID);
                        }

                        return cachedObject;
                    }
                    set
                    {
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
                            SaveSettings();
                        }
                    }
                }

                internal ObjectReferenceSetting(Type objectType, string defaultGuid = "", long defaultLocalID = 0L,
                    Action onChange = null)
                {
                    this.objectType = objectType;
                    this.defaultGuid = defaultGuid;
                    this.defaultLocalID = defaultLocalID;
                    guid = defaultGuid;
                    localID = defaultLocalID;
                    this.onChange = onChange;
                }

                internal void Draw(string label, bool drawResetButton = true, params GUILayoutOption[] options)
                {
                    Draw(new GUIContent(label), drawResetButton, options);
                }

                internal void Draw(GUIContent label, bool drawResetButton = true, params GUILayoutOption[] options)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Value = EditorGUILayout.ObjectField(label, Value, objectType, false, options);
                        if (drawResetButton && EditorUtils.IconButton(EditorUtils.contents.reset))
                        {
                            Reset();
                        }
                    }
                }

                /// <summary>
                /// Resolves a stored (guid, local id) pair. A zero local id means the main asset at
                /// that guid; anything else is a sub-asset, which has to be found by scanning.
                /// </summary>
                private static T LoadAsset<T>(string guid, long localID) where T : Object
                {
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        return null;
                    }

                    if (localID == 0L)
                    {
                        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                    }

                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid));
                    foreach (Object asset in assets)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long id);
                        if (id == localID)
                        {
                            return (T)asset;
                        }
                    }

                    return null;
                }

                internal T GetValue<T>() where T : Object
                {
                    return (T)Value;
                }

                internal override void Reset()
                {
                    Value = LoadAsset<Object>(defaultGuid, defaultLocalID);
                }

                public static implicit operator bool(ObjectReferenceSetting setting)
                {
                    return setting.Value;
                }
            }
        }
    }
}
