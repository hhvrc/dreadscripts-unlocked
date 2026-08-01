// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// Ported region: the `[Serializable] private class EditorSettings` nested in the static
// ControllerEditor class, lines 437-1577. Lifted to a top-level type, as this package does with
// every type the decompiled god-class carries as a nested member.
//
// decompiled member -> ported member, line N:
//   EditorSettings                    -> EditorSettings,                  437
//   [SpecialName] GetInstance()       -> Instance (property),            1470
//   private EditorSettings()          -> EditorSettings(),               1479
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// The settings *framework* this class is built on -- SettingBase, BoolSetting, FloatSetting,
// EnumSetting, StringSetting, VectorSetting, ColorSetting, ObjectReferenceSetting,
// NonSerializedSettingAttribute, SettingsChangeScope, SettingsDeferScope and the deferral/save
// plumbing -- was ALREADY PORTED, to Editor/Common/Settings/. It was reconstructed from this class
// and ADOverhaul's ADOSettings together, which shipped the same code twice under different
// obfuscated names. Nothing of it is duplicated here; this file is only the ControllerEditor half
// that Common deliberately left out: the settings fields, the EditorPrefs key, the JSON envelope
// and the singleton.
//
// Note that decompiled EditorSettings members read `GetValue()`/`SetValue(x)` where the Common port
// exposes a `value` property -- those were [SpecialName] property accessors in the original, and the
// port restored them. `EditorSettings.GetInstance()` is the same artifact and is `Instance` here.
//
// DELIBERATELY NOT PORTED -- licensing-gate remnants. Three BoolSettings existed solely to drive the
// product's phone-home license check against a vendor server that no longer exists:
//   a_VerifyOnDisplay              line 1151, default false
//   a_VerifyOnProjectLoad          line 1154, default true
//   a_HasSucceededLastVerification line 1437, default false
// Their only readers (lines 5436-5599, 6404, 10068-10189, 10487, 18143) are all inside the
// verification routine. Omitting them costs nothing at load time: JsonUtility ignores JSON keys with
// no matching field, so a settings block written by the shipped build still reads back.

using System;
using UnityEditor;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// Every persisted preference of the Controller Editor window, as one serializable block.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single instance, reachable through <see cref="Instance"/>, holds the lot. Each field is a
    /// <see cref="SettingBase"/> that writes the whole block out the moment it is assigned, so
    /// there is no explicit save step anywhere in the tool -- see
    /// <see cref="SettingsPersistence"/> for how that is kept affordable, and
    /// <see cref="WriteToPrefs"/> for where it lands.
    /// </para>
    /// <para>
    /// This is the twin of ADOverhaul's <c>ADOSettings</c>: the same design, the same framework, the
    /// same envelope format, differing only in the EditorPrefs key and the settings themselves. The
    /// two products shipped separate assemblies with separate persisted blocks, so they stay
    /// separate types here.
    /// </para>
    /// </remarks>
    [Serializable]
    internal partial class EditorSettings
    {
        static EditorSettings()
        {
            // The framework's Save() fans out to whoever is listening; this is where this product's
            // serializer joins in. Running from the static constructor is enough because nothing can
            // reach a setting -- and so nothing can request a save -- without first touching this
            // type through Instance.
            SettingsPersistence.onSave += WriteToPrefs;
        }

        /// <summary>
        /// The one settings block, loaded from <see cref="EditorPrefs"/> on first use.
        /// </summary>
        internal static EditorSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    Load();
                }

                return instance;
            }
        }

        /// <remarks>
        /// Private so that the singleton is the only way in. The constructor's one job is to fill the
        /// static <see cref="nonSerializedSettingFields"/> cache, which every save and load then
        /// reads -- see <see cref="Load"/> for why that coupling is more delicate than it looks.
        /// </remarks>
        private EditorSettings()
        {
            CacheNonSerializedSettingFields();
        }

        /// <summary>
        /// The state-decoration flags currently enabled, as an enum rather than the stored int.
        /// </summary>
        internal StateCosmeticOptions GetStateCosmetics()
        {
            return stateCosmetics.GetEnumValue<StateCosmeticOptions>();
        }
    }
}
