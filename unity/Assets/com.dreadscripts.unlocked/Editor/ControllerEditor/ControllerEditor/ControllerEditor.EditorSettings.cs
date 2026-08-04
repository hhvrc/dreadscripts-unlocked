// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   EditorSettings              -> EditorSettings, lines 436-1578 (name already in renames/)
//     StateCosmeticOptions      -> StateCosmeticOptions,        line 439
//     NonSerializedSettingAttribute -> NonSerializedSettingAttribute, line 1146
//     the [SerializeField] settings -> unchanged, lines 1150-1437 (vendor names: they are the JSON
//                                   keys of the saved blob and must not be renamed)
//     parameterLabelStyle, pendingSave, deferred, nonSerializedSettingFields, instance,
//     onSettingsCleared         -> unchanged, lines 1389-1401 (names already in renames/)
//     RebuildParameterLabelStyle, GetStateCosmetics, SaveSettings, LoadSettings,
//     PromptClearSettings, ClearSettings -> unchanged, lines 1439-1577 (names already in renames/)
//     GetDeferred()/SetDeferred() -> Deferred,  lines 1453/1459  [SpecialName pair]
//     GetInstance()               -> Instance,  line 1470        [SpecialName getter]
//     _InterpreterAlgo            -> dropped,   line 1391 (see below)
//     a_VerifyOnDisplay, a_VerifyOnProjectLoad, a_HasSucceededLastVerification
//                                 -> dropped,   lines 1151, 1154, 1437 (licence state; see below)
//
// The nested setting types are in ControllerEditor.EditorSettings.SettingTypes.cs and the two
// IDisposable scopes in ControllerEditor.EditorSettings.Scopes.cs.
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// LICENCE CODE REMOVED, and what was done to keep the saved blob loadable:
//   The three a_* fields above are licence state (RE_NOTES, "Stripping the licence code"). They are
//   plain [SerializeField] BoolSettings, so they existed only as three extra keys inside the MAIN
//   segment's JsonUtility object. Everything that defines the blob's format is therefore untouched:
//   the EditorPrefs key, the "MAIN[...]" segment name, the zero-width-space triple that terminates
//   each segment, the parsing regex, and the set of [NonSerializedSetting] fields that get their own
//   segments. JsonUtility.FromJson ignores JSON keys with no matching field, so a settings string
//   saved by the original tool still loads here, with the three licence keys skipped; the next save
//   simply writes the object without them.
//
// SortAlgo, PatchAlgo, UpdateVisitor, PublishAnnotation and writerVisitor belong to the
// ControllerEditor outer class body, which is not ported yet, so they keep their decompiled names.
//
// _InterpreterAlgo is dropped: a private static bool with no write anywhere in the module, read
// only by `if (_InterpreterAlgo) return;` at the top of SaveSettings, so the branch is dead. This
// is the never-written-static shape de4dot's opaque-predicate fold leaves behind
// (../de4dot/ROADMAP.md, NeverWrittenStaticFields), not vendor logic.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04), minus the licence fields and
// the dead predicate noted above.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// Every persisted preference of the tool, as one serializable object saved into a single
        /// EditorPrefs string.
        /// </summary>
        /// <remarks>
        /// Two kinds of state live here and they are stored differently. Most settings are typed
        /// wrappers (<see cref="BoolSetting"/> and friends) held in <c>[SerializeField]</c> fields,
        /// and go into one <c>MAIN</c> segment written by <see cref="JsonUtility"/>. A few are Unity
        /// objects that <c>JsonUtility</c> cannot round-trip — the template state and transition —
        /// and are marked <see cref="NonSerializedSettingAttribute"/>; each gets its own segment
        /// written by <see cref="EditorJsonUtility"/>, keyed by the field's own name. The segments
        /// are concatenated as <c>Name[json]</c> followed by three zero-width spaces, which is what
        /// <see cref="LoadSettings"/> splits on.
        ///
        /// Every setter saves the whole blob, which is why <see cref="Deferred"/> and the two scopes
        /// in ControllerEditor.EditorSettings.Scopes.cs exist.
        /// </remarks>
        [Serializable]
        private partial class EditorSettings
        {
            /// <summary>The EditorPrefs key the whole settings blob is stored under.</summary>
            private const string SettingsKey = "yOk0XCnENLMO6DIF8cYpSg==SettingsJSON";

            /// <summary>
            /// Terminates each <c>Name[json]</c> segment of the blob: three U+200B zero-width
            /// spaces, spelled as escapes so the delimiter cannot be lost to an editor that
            /// normalises invisible characters. The load-side regex matches the same three.
            /// </summary>
            private const string SegmentTerminator = "\u200b\u200b\u200b";

            /// <summary>Segment name of the JsonUtility-serialized settings object.</summary>
            private const string MainSegment = "MAIN";

            /// <summary>Which extras the graph draws on top of a state node.</summary>
            internal enum StateCosmeticOptions
            {
                none = 0,
                motionName = 1,
                motionIcon = 2,
                coordinates = 4,
                indicators = 8,
                inactiveIndicators = 16,
                quickNewClip = 32,
                all = -1
            }

            /// <summary>
            /// Marks a setting that <see cref="JsonUtility"/> cannot serialize, so it is written to
            /// its own segment with <see cref="EditorJsonUtility"/> instead.
            /// </summary>
            [AttributeUsage(AttributeTargets.Field)]
            internal class NonSerializedSettingAttribute : Attribute
            {
            }

            [SerializeField]
            internal BoolSetting editingTransitions = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting editingStates = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting editingController = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting matchParameter = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting matchMode = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting matchValue = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting showTransitionSettings = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting showTransitionConditions = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting showMatchingOptions = new BoolSetting(false, UpdateVisitor);

            [SerializeField]
            internal BoolSetting showTransitionsCount = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting showStateSettings = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting showStateCount = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting showVRCDrivers = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting showVRCTracking = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting useLegacyDropdown = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting switchDoubleClick = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting autoReverseModes = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting reverseModifiesValues = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting animateInboundEdges = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting animateOutboundEdges = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting autoFrameLayer = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting displayLayerIndex = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting displayParameterType = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting capitalParameterIndicator = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting aw_active = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting aw_autoSwitchClip = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting aw_enablePropertyEditing = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting aw_enableGameObjectDND = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting aw_enableOverride = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting aw_warnPropertyMerge = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting graphBackgroundIsTexture = new BoolSetting(false, SortAlgo);

            [SerializeField]
            internal BoolSetting cosmeticGraphActive = new BoolSetting(false, SortAlgo);

            [SerializeField]
            internal BoolSetting cosmeticNodesActive = new BoolSetting(false, PatchAlgo);

            [SerializeField]
            internal BoolSetting cosmeticTransitionsActive = new BoolSetting(false, PatchAlgo);

            [SerializeField]
            internal BoolSetting hasPingedController = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting requiresStateRename = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting advancedQuickToggle = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting mergeQuickToggle = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting warnParameterConversion = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting displayCategoryView = new BoolSetting(true, delegate
            {
                Instance.sortCategoryViewLayers.Value = false;
                writerVisitor = LayerViewViewType.DefaultView;
            });

            [SerializeField]
            internal BoolSetting sortCategoryViewLayers = new BoolSetting(true);

            [SerializeField]
            internal BoolSetting displayLayerCompactView = new BoolSetting(true, delegate
            {
                Instance.layerCompactView.Value = false;
            });

            [SerializeField]
            internal BoolSetting layerCompactView = new BoolSetting(false, PublishAnnotation);

            [SerializeField]
            internal FloatSetting anyStateNodeColor = new FloatSetting(2f, PatchAlgo);

            [SerializeField]
            internal FloatSetting entryStateNodeColor = new FloatSetting(3f, PatchAlgo);

            [SerializeField]
            internal FloatSetting exitStateNodeColor = new FloatSetting(6f, PatchAlgo);

            [SerializeField]
            internal FloatSetting machineStateNodeColor = new FloatSetting(0f, PatchAlgo);

            [SerializeField]
            internal FloatSetting normalStateNodeColor = new FloatSetting(0f, PatchAlgo);

            [SerializeField]
            internal FloatSetting defaultStateNodeColor = new FloatSetting(5f, PatchAlgo);

            [SerializeField]
            internal FloatSetting defaultLayerWeight = new FloatSetting(1f);

            [SerializeField]
            internal FloatSetting arrowLerpRatio = new FloatSetting(-0.5f);

            [SerializeField]
            internal VectorSetting defaultEntryPosition = new VectorSetting(50f, 120f);

            [SerializeField]
            internal VectorSetting defaultExitPosition = new VectorSetting(800f, 120f);

            [SerializeField]
            internal VectorSetting defaultAnyPosition = new VectorSetting(50f, 20f);

            [SerializeField]
            internal ColorSetting normalTransitionColor = new ColorSetting(1f, 1f, 1f);

            [SerializeField]
            internal ColorSetting entryTransitionColor = new ColorSetting(0.6f, 0.4f, 0f);

            [SerializeField]
            internal ColorSetting selectedTransitionColor = new ColorSetting(0.42f, 0.7f, 1f);

            [SerializeField]
            internal ColorSetting baseTransitionColor = new ColorSetting(0.5f, 0.5f, 0.5f);

            [SerializeField]
            internal ColorSetting gridBackgroundColor = new ColorSetting(0.1647f, 0.1647f, 0.16f, 1f, SortAlgo);

            [SerializeField]
            internal ColorSetting gridMinorLightColor = new ColorSetting(0f, 0f, 0f, 0.1f);

            [SerializeField]
            internal ColorSetting gridMajorLightColor = new ColorSetting(0f, 0f, 0f, 0.15f);

            [SerializeField]
            internal ColorSetting gridMinorDarkColor = new ColorSetting(0f, 0f, 0f, 0.18f);

            [SerializeField]
            internal ColorSetting gridMajorDarkColor = new ColorSetting(0f, 0f, 0f, 0.28f);

            [SerializeField]
            internal ColorSetting parameterLabelColor = new ColorSetting(0.7f, 0.7f, 0.7f);

            [SerializeField]
            internal ObjectReferenceSetting defaultLayerMask = new ObjectReferenceSetting(typeof(AvatarMask));

            [SerializeField]
            internal ObjectReferenceSetting graphBackgroundTexture =
                new ObjectReferenceSetting(typeof(Texture2D), "", 0L, SortAlgo);

            [SerializeField]
            internal StringSetting saveFolder =
                new StringSetting("Assets/DreadScripts/ControllerEditor/Generated Assets");

            [SerializeField]
            internal StringSetting lastAnimationPath = new StringSetting("Assets");

            [SerializeField]
            internal StringSetting lastAnimationName = new StringSetting("New Animation Clip");

            [SerializeField]
            internal StringSetting categoryBaseName = new StringSetting("Base");

            [SerializeField]
            internal StringSetting categoryDelimiter = new StringSetting("/");

            [SerializeField]
            internal EnumSetting parameterLabelFontStyle =
                EnumSetting.FromEnum(FontStyle.Normal, RebuildParameterLabelStyle);

            [SerializeField]
            internal EnumSetting stateCosmetics = EnumSetting.FromEnum(StateCosmeticOptions.all);

            /// <summary>The state new states are cloned from. Saved to its own blob segment.</summary>
            [NonSerializedSetting]
            internal AnimatorState defaultState;

            /// <summary>The transition new transitions are cloned from. Saved to its own segment.</summary>
            [NonSerializedSetting]
            internal AnimatorStateTransition defaultTransition;

            [NonSerialized]
            internal static GUIStyle parameterLabelStyle;

            /// <summary>A save was requested while deferred and still has to happen.</summary>
            private static bool pendingSave;

            private static bool deferred;

            private static FieldInfo[] nonSerializedSettingFields;

            private static EditorSettings instance;

            internal static Action onSettingsCleared;

            // The update/announcement banner state. Written by the update check, read by the footer.
            [SerializeField]
            internal StringSetting u_updateLink = new StringSetting();

            [SerializeField]
            internal StringSetting u_updateVersion = new StringSetting();

            [SerializeField]
            internal StringSetting u_updateMessage = new StringSetting();

            [SerializeField]
            internal StringSetting u_updateChangelog = new StringSetting();

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

            [SerializeField]
            internal BoolSetting u_updateHidden = new BoolSetting(false);

            [SerializeField]
            internal BoolSetting u_announcementHidden = new BoolSetting(false);

            /// <summary>
            /// While true, setters record that a save is owed instead of performing it. Clearing it
            /// performs the owed save, so a whole panel costs one EditorPrefs write.
            /// </summary>
            internal static bool Deferred
            {
                get => deferred;
                set
                {
                    bool wasDeferred = deferred;
                    deferred = value;
                    if (wasDeferred && !deferred && pendingSave)
                    {
                        SaveSettings();
                    }
                }
            }

            internal static EditorSettings Instance
            {
                get
                {
                    if (instance == null)
                    {
                        LoadSettings();
                    }

                    return instance;
                }
            }

            internal static void RebuildParameterLabelStyle()
            {
                parameterLabelStyle = new GUIStyle(EditorUtils.styles.noteRight)
                {
                    fontStyle = Instance.parameterLabelFontStyle.GetEnumValue<FontStyle>()
                };
            }

            internal StateCosmeticOptions GetStateCosmetics()
            {
                return stateCosmetics.GetEnumValue<StateCosmeticOptions>();
            }

            private EditorSettings()
            {
                nonSerializedSettingFields = typeof(EditorSettings)
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(f => f.IsDefined(typeof(NonSerializedSettingAttribute), false))
                    .ToArray();
            }

            internal static void SaveSettings()
            {
                pendingSave = false;
                if (deferred)
                {
                    pendingSave = true;
                    return;
                }

                StringBuilder blob = new StringBuilder(
                    MainSegment + "[" + JsonUtility.ToJson(Instance) + "]" + SegmentTerminator);

                foreach (FieldInfo field in nonSerializedSettingFields)
                {
                    try
                    {
                        string json = EditorJsonUtility.ToJson(field.GetValue(Instance));
                        blob.Append(field.Name + "[" + json + "]" + SegmentTerminator);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                EditorPrefs.SetString(SettingsKey, blob.ToString());
            }

            private static void LoadSettings()
            {
                string blob = string.Empty;
                if (EditorPrefs.HasKey(SettingsKey))
                {
                    blob = EditorPrefs.GetString(SettingsKey, string.Empty);
                }

                Dictionary<string, string> segments = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(blob))
                {
                    MatchCollection matches = Regex.Matches(blob, "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B");
                    for (int i = 0; i < matches.Count; i++)
                    {
                        Match match = matches[i];
                        segments.Add(match.Groups[1].Value, match.Groups[2].Value);
                    }
                }

                if (segments.TryGetValue(MainSegment, out string mainJson))
                {
                    instance = JsonUtility.FromJson<EditorSettings>(mainJson);
                }

                if (instance == null)
                {
                    instance = new EditorSettings();
                }

                foreach (FieldInfo field in nonSerializedSettingFields)
                {
                    object value = field.GetValue(instance) ?? Activator.CreateInstance(field.FieldType);
                    if (segments.TryGetValue(field.Name, out string json))
                    {
                        EditorJsonUtility.FromJsonOverwrite(json, value);
                    }

                    field.SetValue(instance, value);
                    if (field.GetValue(instance) == null)
                    {
                        field.SetValue(instance, Activator.CreateInstance(field.FieldType));
                    }
                }
            }

            internal static void PromptClearSettings()
            {
                if (EditorUtility.DisplayDialog("Clearing Settings",
                        "Are you sure you want to clear the settings?", "Clear", "Cancel"))
                {
                    ClearSettings();
                }
            }

            internal static void ClearSettings()
            {
                instance = new EditorSettings();
                foreach (FieldInfo field in nonSerializedSettingFields)
                {
                    field.SetValue(instance, Activator.CreateInstance(field.FieldType));
                }

                onSettingsCleared?.Invoke();
                SaveSettings();
            }
        }
    }
}
