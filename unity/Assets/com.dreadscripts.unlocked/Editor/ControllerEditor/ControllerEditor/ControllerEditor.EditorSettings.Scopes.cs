// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   SettingsChangeScope  -> SettingsChangeScope, lines 451-489 (name already in renames/)
//     onChanged          -> onChanged,        line 453
//     previousDeferred   -> previousDeferred,  line 455
//     changeCheck        -> changeCheck,       line 457
//     IsChanged()        -> IsChanged,         line 460  [SpecialName property getter]
//   SettingsDeferScope   -> SettingsDeferScope, lines 491-505 (name already in renames/)
//     previousDeferred   -> previousDeferred,  line 493
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// GetDeferred()/SetDeferred() are a [SpecialName] accessor pair restored as the static
// EditorSettings.Deferred property (see ControllerEditor.EditorSettings.cs).
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System;
using UnityEditor;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        private partial class EditorSettings
        {
            /// <summary>
            /// Wraps a block of settings GUI: defers saving for its lifetime, and on exit fires an
            /// optional callback and writes once if anything inside actually changed.
            /// </summary>
            /// <remarks>
            /// Every setter saves the whole blob, so a panel of twenty settings would otherwise
            /// write EditorPrefs twenty times per changed repaint. Deferring collapses that to one
            /// write. The previous deferred flag is restored rather than cleared so the scopes nest.
            /// </remarks>
            internal class SettingsChangeScope : IDisposable
            {
                private readonly Action onChanged;

                private readonly bool previousDeferred;

                private readonly EditorGUI.ChangeCheckScope changeCheck;

                internal bool IsChanged => changeCheck.changed;

                public SettingsChangeScope(Action onChanged = null)
                {
                    this.onChanged = onChanged;
                    previousDeferred = Deferred;
                    Deferred = true;
                    changeCheck = new EditorGUI.ChangeCheckScope();
                }

                public void Dispose()
                {
                    bool changed = changeCheck.changed;
                    changeCheck.Dispose();

                    if (changed)
                    {
                        onChanged?.Invoke();
                        SaveSettings();
                    }

                    Deferred = previousDeferred;
                }

                public static implicit operator bool(SettingsChangeScope scope)
                {
                    return scope.changeCheck.changed;
                }
            }

            /// <summary>
            /// <see cref="SettingsChangeScope"/> without the change check: defers saving for the
            /// lifetime of the block and restores the previous deferred state on exit.
            /// </summary>
            internal class SettingsDeferScope : IDisposable
            {
                private readonly bool previousDeferred;

                public SettingsDeferScope()
                {
                    previousDeferred = Deferred;
                    Deferred = true;
                }

                public void Dispose()
                {
                    Deferred = previousDeferred;
                }
            }
        }
    }
}
