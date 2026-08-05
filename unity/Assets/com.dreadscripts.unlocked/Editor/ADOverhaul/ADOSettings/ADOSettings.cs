// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
// Ported region: the `[Serializable] private class ADOSettings` nested in the static ADOverhaul
// class, lines 751-1688. Lifted to a top-level type, as this package does with every type the
// decompiled god-class carries as a nested member.
//
//   ADOSettings                       -> ADOSettings, line 751
//   [SpecialName] Instance()          -> instance (property), line 1569
//   private ADOSettings()             -> ADOSettings(), line 1578
//   [SpecialName] StateColors()       -> StateColors (property), line 1679
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// The settings *framework* this class is built on -- SettingBase, BoolSetting, FloatSetting,
// EnumSetting, StringSetting, VectorSetting, ColorSetting, ObjectReferenceSetting,
// NonSerializedSettingAttribute, SettingsChangeScope, SettingsDeferScope and the
// deferral/pending-save plumbing (_ProxyIdentifier, savePending, deferred, IsDeferred, SetDeferred)
// -- was ALREADY PORTED, to Editor/Common/Settings/. It was reconstructed from this very class and
// ControllerEditor's EditorSettings together, which shipped the same code twice under different
// obfuscated names. Nothing of it is duplicated here; this file and its siblings are only the
// ADOverhaul half that Common deliberately left out: the settings fields, the EditorPrefs key, the
// JSON envelope and the singleton.
//
// Note that decompiled ADOSettings call sites read `GetValue()`/`SetValue(x)` where the Common port
// exposes a `value` property -- those were [SpecialName] property accessors in the original, and the
// port restored them. `ADOSettings.instance()` and `StateColors()` are the same artifact and are
// properties here.
//
// This type is the twin of ControllerEditor's EditorSettings (Editor/ControllerEditor/EditorSettings/),
// whose Save/Load/Clear/PromptClear are the same code differing only in the EditorPrefs key. They
// stay separate types because the two products shipped separate assemblies persisting independent
// blocks under different keys; merging them would merge two independent sets of user settings.
//
// DELIBERATELY NOT PORTED -- licensing-gate remnants. Three BoolSettings existed solely to drive the
// product's phone-home licence check against a vendor server that no longer exists:
//   a_HasSucceededLastVerification line 1474, default false
//   a_VerifyOnDisplay              line 1477, default true
//   a_VerifyOnProjectLoad          line 1480, default false
// Every reader was verified to sit inside the verification path: lines 4767, 4775, 4923-4930, 4988,
// 5017, 5039, 5095 (the activate/deactivate/validate licence routines), 7067-7092
// (DisableConfiguration, the [InitializeOnLoadMethod] startup hook, plus VisitConfiguration and the
// AssetConfiguration guard), 7168, 7194, 7959-7967 (the "Verify/On Display" and
// "Verify/On Project Load" GenericMenu items) and 8478. a_VerifyOnProjectLoad in particular is the
// only thing gating the startup hook that spawns the hardware-fingerprint subprocesses and POSTs the
// result. Omitting the three costs nothing at load time: JsonUtility ignores JSON keys with no
// matching field, so a settings block written by the shipped build still reads back.
//
// The u_* update and announcement settings ARE ported -- they are the update banner's state, a
// feature rather than a gate, and they are read only by the banner.
//
// The ADOverhaul2019 build declares the identical field list, in the identical order, with the
// identical defaults and the identical EditorPrefs key (its lines 1438-1546, 1606-1619). The two
// builds differ only in obfuscated local and parameter names.
//
// Audit status: PARTIAL -- the four MAP entries above were re-checked against reverse-engineering/export/ (the
// accessor at 1569 is named `Instance()` in the current snapshot, not `instance()`, and the three
// member line numbers were each one short of the declaration after the 561e9ec re-snapshot); the
// settings field list and the not-ported claims below were not re-checked.

using System;
using UnityEngine;
using DreadScripts.Common;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// Every persisted preference of ADOverhaul, as one serializable block.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single settingsInstance, reachable through <see cref="instance"/>, holds the lot. Each field is a
    /// <see cref="SettingBase"/> that writes the whole block out the moment it is assigned, so there
    /// is no explicit save step anywhere in the tool -- see <see cref="SettingsPersistence"/> for how
    /// that is kept affordable, and <see cref="Serialize"/> for where it lands.
    /// </para>
    /// <para>
    /// Unlike ControllerEditor's twin, ADOverhaul declares no
    /// <see cref="NonSerializedSettingAttribute"/> field at all: nothing it persists is a Unity
    /// object reference. The reflection cache and the loops over it are therefore always empty. They
    /// are ported anyway, both because they are what shipped and because they are the mechanism a
    /// later object-valued setting would need.
    /// </para>
    /// </remarks>
    [Serializable]
    internal partial class ADOSettings
    {
        static ADOSettings()
        {
            // The framework's Save() fans out to whoever is listening; this is where this product's
            // serializer joins in. Running from the static constructor is enough because nothing can
            // reach a setting -- and so nothing can request a save -- without first touching this
            // type through instance.
            SettingsPersistence.onSave += Serialize;
        }

        /// <summary>
        /// The one settings block, loaded from <see cref="UnityEditor.EditorPrefs"/> on first use.
        /// </summary>
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

        /// <remarks>
        /// Private so that the singleton is the only way in. The constructor's one job is to fill the
        /// static <see cref="nonSerializedFields"/> cache, which every save and load then
        /// reads -- see <see cref="Load"/> for why that coupling is more delicate than it looks.
        /// </remarks>
        private ADOSettings()
        {
            CacheNonSerializedSettingFields();
        }

        /// <summary>
        /// The three membership colours in the order the membership-state codes index them:
        /// 0 = not a member (<see cref="inactiveColor"/>), 1 = a member (<see cref="activeColor"/>),
        /// 2 = a member of some of the multi-edited targets but not all (<see cref="mixedColor"/>).
        /// </summary>
        /// <remarks>
        /// A fresh array on every read, including once per handle drawn in the scene GUI. Ported as
        /// shipped; the allocation is what the original did and the order is the contract that
        /// <c>PhysBoneEditor.membershipStates</c> is indexed against.
        /// </remarks>
        internal Color[] StateColors
        {
            get
            {
                return new Color[3]
                {
                    inactiveColor.value,
                    activeColor.value,
                    mixedColor.value
                };
            }
        }
    }
}
