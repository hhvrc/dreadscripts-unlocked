// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of these types,
// nested inside their respective settings classes.
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, class EditorSettings
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, class ADOSettings
//
// decompiled member -> ported member (ControllerEditor line / ADOverhaul2022 line):
//   BoolSetting                                  -> BoolSetting,                          508 / 810
//   BoolSetting.GetValue/SetValue                -> BoolSetting.value,                    516 / 818
//   BoolSetting.Toggle                           -> BoolSetting.Toggle,                   539 / 841
//   BoolSetting.Draw(string) / Draw              -> BoolSetting.Draw(string, ...),        544 / 846
//   BoolSetting.Draw(GUIContent) / DrawContent   -> BoolSetting.Draw(GUIContent, ...),    549 / 851
//   BoolSetting.DrawButton(string)               -> BoolSetting.DrawButton(string, ...),  558 / 863
//   BoolSetting.DrawButton(GUIContent) / DrawButtonContent -> DrawButton(GUIContent, ...), 563 / 868
//   FloatSetting                                 -> FloatSetting,                         585 / 890
//   FloatSetting.GetValue/SetValue               -> FloatSetting.value,                   593 / 898
//   FloatSetting.VisitDefinition / DrawField     -> DrawField(string, ...),               616 / 921
//   FloatSetting.StartDefinition / DrawFieldContent -> DrawField(GUIContent, ...),        628 / 933
//   FloatSetting.DefineDefinition / DrawFieldWithLabelWidth -> DrawFieldWithLabelWidth(string, ...),   621 / 926
//   FloatSetting.ReadDefinition / DrawFieldWithLabelWidthContent -> DrawFieldWithLabelWidth(GUIContent, ...), 644 / 945
//   FloatSetting.SelectDefinition / DrawSlider   -> DrawSlider(string, ...),              651 / 952
//   FloatSetting.RemoveDefinition / DrawSliderContent -> DrawSlider(GUIContent, ...),     656 / 957
//   FloatSetting.InstantiateDefinition / DrawNormalizedSlider -> DrawNormalizedSlider(string, ...),    671 / 969
//   FloatSetting.AwakeDefinition / DrawNormalizedSliderContent -> DrawNormalizedSlider(GUIContent, ...), 676 / 974
//   EnumSetting                                  -> EnumSetting,                          698 / 996
//   EnumSetting.IntValue                         -> EnumSetting.IntValue,                 701 / 999
//   EnumSetting.GetEnumValue<T>                  -> EnumSetting.GetEnumValue<T>,          718 / 1016
//   EnumSetting.DrawIntField(GUIContent) / DrawIntFieldContent -> DrawIntField(GUIContent, ...), 723 / 1021
//   EnumSetting.DrawIntField(string)             -> DrawIntField(string, ...),            732 / 1026
//   EnumSetting.DrawEnumPopup<T>(GUIContent) / DrawEnumPopupContent<T> -> DrawEnumPopup<T>(GUIContent, ...), 737 / 1031
//   EnumSetting.DrawEnumPopup<T>(string)         -> DrawEnumPopup<T>(string, ...),        746 / 1043
//   EnumSetting.FromEnum<T>                      -> EnumSetting.FromEnum<T>,              751 / 1048
//   StringSetting                                -> StringSetting,                        874 / 1174
//   StringSetting.GetValue/SetValue              -> StringSetting.value,                  882 / 1182
//   StringSetting.Draw(string)                   -> StringSetting.Draw(string, ...),      906 / --
//   StringSetting.Draw(GUIContent)               -> StringSetting.Draw(GUIContent, ...),  911 / --
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. Where the two sources named the same method differently,
// ADOverhaul's names are used: they are ordinary English and ControllerEditor's are not, so they
// are almost certainly what was written. The string/GUIContent pairs the sources carry as
// Draw/DrawContent are collapsed into overloads here.
//
// ADOverhaul's StringSetting has no draw methods at all; the two above come from ControllerEditor
// only. Everything else in this file is present, and behaviourally identical, in both.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>A persisted <see cref="bool"/> setting.</summary>
    [Serializable]
    internal class BoolSetting : SettingBase
    {
        [SerializeField]
        private bool _value;

        internal readonly Action onChange;

        internal bool value
        {
            get
            {
                return _value;
            }
            set
            {
                // Guarded so that redrawing a toggle the user did not touch costs nothing.
                if (_value != value)
                {
                    _value = value;
                    onChange?.Invoke();
                    SettingsPersistence.Save();
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
            value = !_value;
        }

        internal void Draw(string label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            Draw(new GUIContent(label), style, options);
        }

        /// <inheritdoc cref="Draw(string, GUIStyle, GUILayoutOption[])"/>
        internal void Draw(GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style ??= EditorStyles.toggle;

            value = EditorGUILayout.Toggle(label, value, style, options);
        }

        /// <summary>
        /// Draws the setting as a button that stays held down while it is on.
        /// </summary>
        /// <param name="offContent">
        /// Shown instead of <paramref name="content"/> while the setting is off, for buttons that
        /// label themselves with what they will do rather than with what they are.
        /// </param>
        /// <param name="toolbarStyle">Draws with toolbar chrome rather than the plain button skin.</param>
        /// <param name="onColor">Background tint while on; the current tint if omitted.</param>
        /// <param name="offColor">Background tint while off; the current tint if omitted.</param>
        internal void DrawButton(string content, string offContent = null, bool toolbarStyle = false, Color? onColor = null, Color? offColor = null, params GUILayoutOption[] options)
        {
            DrawButton(
                string.IsNullOrEmpty(content) ? GUIContent.none : new GUIContent(content),
                string.IsNullOrEmpty(offContent) ? GUIContent.none : new GUIContent(offContent),
                toolbarStyle,
                onColor,
                offColor,
                options);
        }

        /// <inheritdoc cref="DrawButton(string, string, bool, Color?, Color?, GUILayoutOption[])"/>
        internal void DrawButton(GUIContent content, GUIContent offContent = null, bool toolbarStyle = false, Color? onColor = null, Color? offColor = null, params GUILayoutOption[] options)
        {
            onColor ??= GUI.backgroundColor;
            offColor ??= GUI.backgroundColor;

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = value ? onColor.Value : offColor.Value;
            value = GUILayout.Toggle(
                value,
                !value && offContent != null ? offContent : content,
                toolbarStyle ? EditorStyles.toolbarButton : GUI.skin.button,
                options);
            GUI.backgroundColor = previousBackground;
        }

        public static implicit operator bool(BoolSetting setting)
        {
            return setting._value;
        }

        internal override void Reset()
        {
            value = (bool)defaultValue;
        }
    }

    /// <summary>A persisted <see cref="float"/> setting, drawable as a field or a slider.</summary>
    /// <remarks>
    /// Also the storage for <see cref="EnumSetting"/>, which is why the int conversion below exists:
    /// enums are persisted as the float they round-trip through rather than as a second field type.
    /// </remarks>
    [Serializable]
    internal class FloatSetting : SettingBase
    {
        [SerializeField]
        private float _value;

        internal readonly Action onChange;

        internal float Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    onChange?.Invoke();
                    SettingsPersistence.Save();
                }
            }
        }

        internal FloatSetting(float defaultValue, Action onChange = null)
        {
            this.defaultValue = defaultValue;
            _value = defaultValue;
            this.onChange = onChange;
        }

        /// <param name="showReset">Draws the revert button at the right of the row.</param>
        internal void DrawField(string label, bool showReset = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            DrawField(new GUIContent(label), showReset, style, options);
        }

        /// <inheritdoc cref="DrawField(string, bool, GUIStyle, GUILayoutOption[])"/>
        internal void DrawField(GUIContent label, bool showReset = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (style == null)
                {
                    style = EditorStyles.numberField;
                }

                Value = EditorGUILayout.FloatField(label, Value, style, options);
                if (showReset && DrawResetButton())
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Draws the field with an explicit label width, for settings rows narrower than the
        /// inspector's default label column.
        /// </summary>
        /// <remarks>
        /// The width is reset to zero rather than to whatever it was, matching the shipped builds;
        /// zero restores Unity's own default, so this is only correct when the caller was not
        /// already inside a label-width override of its own.
        /// </remarks>
        internal void DrawFieldWithLabelWidth(string label, float labelWidth, bool showReset = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            EditorGUIUtility.labelWidth = labelWidth;
            DrawField(new GUIContent(label), showReset, style, options);
            EditorGUIUtility.labelWidth = 0f;
        }

        /// <inheritdoc cref="DrawFieldWithLabelWidth(string, float, bool, GUIStyle, GUILayoutOption[])"/>
        internal void DrawFieldWithLabelWidth(GUIContent label, float labelWidth, bool showReset = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            EditorGUIUtility.labelWidth = labelWidth;
            DrawField(label, showReset, style, options);
            EditorGUIUtility.labelWidth = 0f;
        }

        internal void DrawSlider(string label, float min, float max, bool showReset = true, params GUILayoutOption[] options)
        {
            DrawSlider(new GUIContent(label), min, max, showReset, options);
        }

        /// <inheritdoc cref="DrawSlider(string, float, float, bool, GUILayoutOption[])"/>
        internal void DrawSlider(GUIContent label, float min, float max, bool showReset = true, params GUILayoutOption[] options)
        {
            using (new GUILayout.HorizontalScope())
            {
                Value = EditorGUILayout.Slider(label, Value, min, max, options);

                // DEOBF-BUG(resolved): the ControllerEditor export has `while` rather than `if`
                // here, which would spin forever on the click; ADOverhaul's copy of the same method
                // has `if`, so the `while` is a de4dot control-flow-recovery fault. Same fault
                // confirmed against the original IL on AnimatorTypeCache.ParameterEntry.Source.
                if (showReset && DrawResetButton())
                {
                    Reset();
                }
            }
        }

        /// <summary>Draws a slider over the 0..1 range, for the many settings that are ratios.</summary>
        internal void DrawNormalizedSlider(string label, bool showReset = true, params GUILayoutOption[] options)
        {
            DrawNormalizedSlider(new GUIContent(label), showReset, options);
        }

        /// <inheritdoc cref="DrawNormalizedSlider(string, bool, GUILayoutOption[])"/>
        internal void DrawNormalizedSlider(GUIContent label, bool showReset = true, params GUILayoutOption[] options)
        {
            DrawSlider(label, 0f, 1f, showReset, options);
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
    /// A persisted enum setting, stored as the underlying <see cref="FloatSetting"/> so that the
    /// enum type itself never has to be serialized.
    /// </summary>
    /// <remarks>
    /// Nothing ties an instance to one enum type: the type is supplied per call at the drawing and
    /// reading sites. That is what allows the same stored value to be read as a plain enum or as a
    /// flags mask, and it is also why a mismatched <typeparamref name="T"/> is not caught.
    /// </remarks>
    [Serializable]
    internal class EnumSetting : FloatSetting
    {
        /// <remarks>
        /// The decompilation shows <c>[SerializeField]</c> on this property, which cannot be what
        /// was written — the attribute does not apply to properties, and Unity would ignore it in
        /// any case. The persisted state is the inherited float field.
        /// </remarks>
        internal int IntValue
        {
            get => (int)Value;
            set => Value = value;
        }

        internal EnumSetting(int defaultValue, Action onChange = null)
            : base(defaultValue, onChange)
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

        /// <inheritdoc cref="DrawIntField(GUIContent, GUIStyle, GUILayoutOption[])"/>
        internal void DrawIntField(string label, GUIStyle style = null, params GUILayoutOption[] options)
        {
            DrawIntField(new GUIContent(label), style, options);
        }

        /// <param name="flags">
        /// Draws a multi-select mask rather than a single-choice popup, for enums whose members are
        /// bit flags.
        /// </param>
        internal void DrawEnumPopup<T>(GUIContent label, bool flags = false, GUIStyle style = null, params GUILayoutOption[] options) where T : Enum
        {
            if (style == null)
            {
                style = EditorStyles.popup;
            }

            IntValue = flags
                ? (int)(object)EditorGUILayout.EnumFlagsField(label, (T)(object)IntValue, style, options)
                : (int)(object)EditorGUILayout.EnumPopup(label, (T)(object)IntValue, style, options);
        }

        /// <inheritdoc cref="DrawEnumPopup{T}(GUIContent, bool, GUIStyle, GUILayoutOption[])"/>
        internal void DrawEnumPopup<T>(string label, bool flags = false, GUIStyle style = null, params GUILayoutOption[] options) where T : Enum
        {
            DrawEnumPopup<T>(new GUIContent(label), flags, style, options);
        }

        /// <summary>Creates a setting defaulting to an enum member, without the cast at the call site.</summary>
        internal static EnumSetting FromEnum<T>(T defaultValue, Action onChange = null) where T : Enum
        {
            return new EnumSetting((int)(object)defaultValue, onChange);
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

    /// <summary>A persisted <see cref="string"/> setting.</summary>
    [Serializable]
    internal class StringSetting : SettingBase
    {
        [SerializeField]
        private string _value;

        internal readonly Action onChange;

        internal string Value
        {
            get => _value;
            set
            {
                // The ControllerEditor decompilation renders this guard's body as an infinite loop
                // that only assigns the field; ADOverhaul's copy has the three statements below,
                // which is plainly what both were.
                if (_value != value)
                {
                    _value = value;
                    onChange?.Invoke();
                    SettingsPersistence.Save();
                }
            }
        }

        internal StringSetting(string defaultValue = "", Action onChange = null)
        {
            this.defaultValue = defaultValue;
            _value = defaultValue;
            this.onChange = onChange;
        }

        /// <param name="delayed">
        /// Commits only when the user presses Return or leaves the field, rather than on every
        /// keystroke. On by default because the alternative saves — and fires onChange — once per
        /// character typed.
        /// </param>
        internal void Draw(string label, bool showReset = true, bool delayed = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            Draw(new GUIContent(label), showReset, delayed, style, options);
        }

        /// <inheritdoc cref="Draw(string, bool, bool, GUIStyle, GUILayoutOption[])"/>
        internal void Draw(GUIContent label, bool showReset = true, bool delayed = true, GUIStyle style = null, params GUILayoutOption[] options)
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

                if (showReset && DrawResetButton())
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
}
