// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of the whole
// persisted-settings framework, nested inside their respective settings classes. Reconstructed
// from both, which are the same code under different obfuscated names.
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs, class EditorSettings
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs, class ADOSettings
//
// The MAP line numbers below are the ControllerEditor ones; the ADOverhaul2022 counterparts are
// tabulated in the NOTES section at the bottom.
//
//   SettingsChangeScope                  -> SettingsChangeScope,           line 451
//   SettingsChangeScope.IsChanged        -> SettingsChangeScope.changed,   line 460
//   SettingsDeferScope                   -> SettingsDeferScope,            line 491
//   static _InterpreterAlgo              -> SettingsPersistence.suppressSave, line 1391
//   static pendingSave                   -> SettingsPersistence.pendingSave, line 1393
//   static deferred                      -> SettingsPersistence.deferred,  line 1395
//   static GetDeferred                   -> SettingsPersistence.deferred,  line 1453
//   static SetDeferred                   -> SettingsPersistence.SetDeferred, line 1459
//   static SaveSettings (deferral half)  -> SettingsPersistence.Save,      line 1486
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// NOTES
// Where the same member is spelled differently in the two builds, and where it sits in each:
//                                          ControllerEditor / ADOverhaul2022
//   SettingsChangeScope                          451  /  753
//   SettingsChangeScope.IsChanged                460  /  762
//   SettingsDeferScope                           491  /  793
//   suppressSave  (_InterpreterAlgo / _ProxyIdentifier)   1391 / 1428
//   pendingSave   (pendingSave / savePending)            1393 / 1430
//   deferred      (deferred / deferred)                  1395 / 1432
//   the getter    (GetDeferred / IsDeferred)             1453 / 1552
//   SetDeferred   (SetDeferred / SetDeferred)            1459 / 1558
//   Save          (SaveSettings / Save)                  1486 / 1585
//
// Deliberately NOT ported here, because they are product-specific rather than framework: the
// EditorPrefs key each product writes under, the JSON envelope built in SaveSettings/Save, the
// matching LoadSettings/Load, ClearSettings/Clear, PromptClearSettings/PromptClear, the singleton
// accessor (GetInstance/Instance), the nonSerializedSettingFields/nonSerializedFields cache, the
// onSettingsCleared/onCleared hook, and of course the settings fields themselves. Each product's
// settings class keeps those and subscribes its own serializer to onSave.
//
// Audit status: PARTIAL -- every line number in the tables above was checked against decompiled/
// (both builds) and lands on the member named; the bodies were not re-diffed.

using System;
using UnityEditor;

namespace DreadScripts.Common
{
    /// <summary>
    /// The save side of the settings framework: who writes settings to disk, and when they are
    /// allowed to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every setting writes itself out the moment it changes, which for a value driven by a slider
    /// or a colour field means once per repaint. The deferral flag is what makes that affordable:
    /// while it is set, saves are collapsed into a single pending one that is flushed when the flag
    /// is cleared — see <see cref="SettingsDeferScope"/>.
    /// </para>
    /// <para>
    /// In the shipped builds these were static members of each product's own settings class, so the
    /// two products had independent flags. Sharing one framework across both means one flag: a
    /// defer scope opened while drawing one product's settings also defers the other's writes, and
    /// the flush at the end of it saves both. Since <see cref="onSave"/> fans out to each product's
    /// serializer, and each still writes under its own EditorPrefs key, the only observable effect
    /// is an occasional redundant write.
    /// </para>
    /// </remarks>
    internal static class SettingsPersistence
    {
        /// <summary>
        /// Invoked to actually serialize settings. Each product's settings class subscribes its own
        /// writer, which is what decides the EditorPrefs key and the JSON envelope.
        /// </summary>
        internal static event Action onSave;

        private static bool pendingSave;

        private static bool isDeferred;

        /// <summary>
        /// Suppresses saving outright. Never assigned in either shipped build — it survives as the
        /// escape hatch it presumably was during development, and is kept so that the save path is
        /// a faithful transcription.
        /// </summary>
        internal static bool suppressSave { get; set; }

        /// <summary>
        /// Whether saves are currently being collapsed rather than written through.
        /// </summary>
        internal static bool deferred
        {
            get
            {
                return isDeferred;
            }
        }

        /// <summary>
        /// Turns deferral on or off. Turning it off flushes a save that was requested while it was
        /// on.
        /// </summary>
        /// <remarks>
        /// The flush is conditional on the flag having actually been set beforehand, so that
        /// restoring an already-false flag — which is what the outermost scope does on dispose —
        /// cannot trigger a save on its own.
        /// </remarks>
        internal static void SetDeferred(bool value)
        {
            bool wasDeferred = isDeferred;
            isDeferred = value;
            if (wasDeferred && !isDeferred && pendingSave)
            {
                Save();
            }
        }

        /// <summary>
        /// Writes settings out, or records that they need writing if saves are deferred.
        /// </summary>
        internal static void Save()
        {
            pendingSave = false;
            if (isDeferred)
            {
                pendingSave = true;
                return;
            }

            if (suppressSave)
            {
                return;
            }

            onSave?.Invoke();
        }
    }

    /// <summary>
    /// Defers saving for the duration of a block of settings GUI, and saves once at the end if the
    /// user changed anything in it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The saved-once-at-the-end guarantee is what lets settings be drawn with sliders: the
    /// individual writes each control makes while being dragged are swallowed by the deferral, and
    /// one write happens on dispose.
    /// </para>
    /// <para>
    /// The previous deferral state is captured rather than assumed false, so these scopes nest.
    /// </para>
    /// </remarks>
    internal class SettingsChangeScope : IDisposable
    {
        private readonly Action onChanged;

        private readonly bool previousDeferred;

        private readonly EditorGUI.ChangeCheckScope changeCheck;

        /// <param name="onChanged">
        /// Invoked before the save when something in the scope was edited, for callers that need to
        /// rebuild derived state.
        /// </param>
        public SettingsChangeScope(Action onChanged = null)
        {
            this.onChanged = onChanged;
            previousDeferred = SettingsPersistence.deferred;
            SettingsPersistence.SetDeferred(true);
            changeCheck = new EditorGUI.ChangeCheckScope();
        }

        /// <summary>
        /// Whether anything drawn inside the scope has been edited.
        /// </summary>
        /// <remarks>
        /// Reading this ends the change check, so a caller that tests it mid-scope will not see
        /// edits made after the test. That is also why <see cref="Dispose"/> reads it before
        /// disposing the inner scope rather than after.
        /// </remarks>
        internal bool changed
        {
            get
            {
                return changeCheck.changed;
            }
        }

        public void Dispose()
        {
            bool wasChanged = changeCheck.changed;
            changeCheck.Dispose();
            if (wasChanged)
            {
                onChanged?.Invoke();
                SettingsPersistence.Save();
            }

            SettingsPersistence.SetDeferred(previousDeferred);
        }

        /// <summary>Lets the scope be tested directly: <c>using (var scope = ...) if (scope)</c>.</summary>
        public static implicit operator bool(SettingsChangeScope scope)
        {
            return scope.changeCheck.changed;
        }
    }

    /// <summary>
    /// Collapses every save made inside the block into one, without watching for GUI changes.
    /// </summary>
    /// <remarks>
    /// For code that assigns several settings in a row — restoring a preset, applying a batch of
    /// defaults — where the intermediate states are not worth a write each.
    /// </remarks>
    internal class SettingsDeferScope : IDisposable
    {
        private readonly bool previousDeferred;

        public SettingsDeferScope()
        {
            previousDeferred = SettingsPersistence.deferred;
            SettingsPersistence.SetDeferred(true);
        }

        public void Dispose()
        {
            SettingsPersistence.SetDeferred(previousDeferred);
        }
    }
}
