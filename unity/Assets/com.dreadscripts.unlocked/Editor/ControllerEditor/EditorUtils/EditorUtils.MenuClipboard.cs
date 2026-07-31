// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   struct MenuClipboardState -> MenuClipboardState, line 1581
//     IsValid()  [SpecialName] -> IsValid property, line 1612
//     Set(VRCAvatarDescriptor, VRCExpressionsMenu, VRCExpressionsMenu) -> Set, line 1617
//     Set(VRCAvatarDescriptor, List<Control>, VRCExpressionsMenu)      -> Set, line 1624
//     Set(VRCAvatarDescriptor, int, VRCExpressionsMenu)                -> Set, line 1638
//     Set(VRCExpressionsMenu, VRCExpressionsMenu)                      -> Set, line 1645
//     Set(List<Control>, VRCExpressionsMenu)                           -> Set, line 1662
//     Set(int, VRCExpressionsMenu)                                     -> Set, line 1674
//     Process()                                                        -> Process, line 1681
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The struct is nested inside EditorUtils in the decompiled source (its members sit one indent
// level deeper than EditorUtils' own), so it is ported as a nested type on this partial rather
// than as a file-level type.
//
// Deliberately not ported yet -- the GUI half of the struct, because every helper it needs is
// still unported and the package has to keep compiling. Named here so the next pass can find
// them:
//   Draw(Action<VRCExpressionsMenu>, bool, string), line 1726 -- the object-field row. Needs
//     EditorUtils.PopRules/ComputeRules (the generic labelled asset field, line 4302) and
//     EditorUtils.CallRules (line 4427, the "is it null vs. is it a destroyed/missing asset"
//     test that picks the placeholder text).
//   DrawCounter(), line 1735 -- the "3/8, remaining 5" tally drawn inside that row, tinted green
//     or yellow. Needs EditorUtils.configurationProperty and EditorUtils._WrapperProcessor
//     (lines 2178/2182, the green/yellow pair) which EditorUtils.Colors.cs does not carry yet.
//     It ends in MenuSelector.Open(targetMenu, OnMenuSelected, controlsToAdd), already ported.
//   OnMenuSelected(VRCExpressionsMenu), line 1753 -- forwards to the caller's callback and then,
//     if the avatar has no main menu at all, offers to make the picked one it. Needs
//     EditorUtils.ReadError (line 8263, assigns VRCAvatarDescriptor.expressionsMenu and keeps
//     customExpressions in step).
//   OnMenuCreated(VRCExpressionsMenu), line 1768 -- gives a freshly created menu asset an empty
//     controls list. Only reachable through Draw, so it is held back with it.
//   private Action<VRCExpressionsMenu> onMenuSelected, line 1607 -- Draw's callback slot.
//
// Also not ported: the static field CancelField (line 1609) and RestartField() (line 1777), which
// returns "CancelField == null". Nothing anywhere assigns the field or calls the method, and the
// sibling ParameterCostTracker carries the identical CompareCandidate/PublishCandidate pair, so
// this is obfuscator scaffolding rather than behaviour -- an always-true method over a field that
// is always null.

using System.Collections.Generic;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Where a batch of VRChat expression-menu controls is about to be written, and whether it
        /// will fit once it gets there.
        /// </summary>
        /// <remarks>
        /// Despite the name this holds no controls of its own and is not a copy buffer: it is the
        /// state behind a "Target Menu:" field. The caller describes the move it wants to make --
        /// this avatar, this source menu or this list of controls, into this target -- calls
        /// <see cref="Process"/>, and reads back a <see cref="ValidationResult"/> plus the slot
        /// counts the field displays. The controls themselves are copied by the caller afterwards.
        ///
        /// The <c>use*</c> flags exist because null means two different things here. A target with
        /// no source menu set is fine if the caller never intended one; it is an error only if the
        /// caller said it would supply one. Each Set overload raises the flag for the input it
        /// takes, so <see cref="Process"/> can tell "not supplied" from "supplied as null".
        ///
        /// The overloads return the struct by value and are meant to be chained
        /// (<c>state = state.Set(avatar, menu).Process()</c>). Because this is a struct, calling
        /// them and discarding the result mutates the copy and loses the change.
        /// </remarks>
        internal struct MenuClipboardState
        {
            internal VRCAvatarDescriptor avatar;

            /// <summary>Whether <see cref="avatar"/> was supplied and must therefore be non-null.</summary>
            internal bool useAvatar;

            /// <summary>The menu the controls are being added to.</summary>
            internal VRCExpressionsMenu targetMenu;

            internal VRCExpressionsMenu sourceMenu;

            /// <summary>Whether <see cref="sourceMenu"/> was supplied and must therefore be non-null.</summary>
            internal bool useSourceMenu;

            internal List<VRCExpressionsMenu.Control> sourceControls;

            /// <summary>Whether <see cref="sourceControls"/> was supplied and must therefore be non-null.</summary>
            internal bool useSourceControls;

            /// <summary>How many controls the caller intends to add to <see cref="targetMenu"/>.</summary>
            internal int controlsToAdd;

            /// <summary>Free slots in <see cref="targetMenu"/> before anything is added.</summary>
            internal int availableSlots;

            /// <summary>Free slots left afterwards; negative when the batch does not fit.</summary>
            internal int remainingSlots;

            internal bool isWithinLimit;

            internal ValidationResult validation;

            /// <summary>
            /// Whether the last <see cref="Process"/> passed. False before Process has ever run,
            /// since the default <see cref="ValidationResult"/> is not valid.
            /// </summary>
            internal bool IsValid => validation.isValid;

            /// <summary>
            /// Sets an avatar and a source menu at once, with the target defaulting to unset.
            /// </summary>
            internal MenuClipboardState Set(VRCAvatarDescriptor avatar, VRCExpressionsMenu sourceMenu, VRCExpressionsMenu targetMenu = null)
            {
                this.avatar = avatar;
                useAvatar = true;
                return Set(sourceMenu, targetMenu);
            }

            /// <inheritdoc cref="Set(List{VRCExpressionsMenu.Control}, VRCExpressionsMenu)"/>
            internal MenuClipboardState Set(VRCAvatarDescriptor avatar, List<VRCExpressionsMenu.Control> sourceControls, VRCExpressionsMenu targetMenu = null)
            {
                this.avatar = avatar;
                useAvatar = true;
                this.sourceControls = sourceControls;
                useSourceControls = true;

                if (sourceControls != null)
                {
                    return Set(sourceControls.Count, targetMenu);
                }

                // Null controls are recorded rather than rejected here; Process turns the raised
                // useSourceControls flag into the error.
                this.targetMenu = targetMenu;
                return this;
            }

            /// <inheritdoc cref="Set(int, VRCExpressionsMenu)"/>
            internal MenuClipboardState Set(VRCAvatarDescriptor avatar, int controlsToAdd, VRCExpressionsMenu targetMenu = null)
            {
                this.avatar = avatar;
                useAvatar = true;
                return Set(controlsToAdd, targetMenu);
            }

            /// <summary>
            /// Takes the whole of <paramref name="sourceMenu"/> as the batch to add.
            /// </summary>
            /// <remarks>
            /// A menu asset saved before it ever had a control has a null <c>controls</c> list, which
            /// every consumer would then have to guard; it is filled in and marked dirty here so that
            /// the rest of the copy path can assume a list exists.
            /// </remarks>
            internal MenuClipboardState Set(VRCExpressionsMenu sourceMenu, VRCExpressionsMenu targetMenu = null)
            {
                this.sourceMenu = sourceMenu;
                useSourceMenu = true;

                if (sourceMenu.controls == null)
                {
                    sourceMenu.controls = new List<VRCExpressionsMenu.Control>();
                    EditorUtility.SetDirty(sourceMenu);
                }

                // Faithful to the decompiled source, which dereferences sourceMenu above before
                // testing it here. The test therefore only catches a destroyed-but-not-null asset
                // (Unity's overloaded ==); a true null reference has already thrown.
                if (sourceMenu != null)
                {
                    return Set(sourceMenu.controls, targetMenu);
                }

                this.targetMenu = targetMenu;
                return this;
            }

            /// <summary>
            /// Takes an explicit list of controls as the batch to add.
            /// </summary>
            internal MenuClipboardState Set(List<VRCExpressionsMenu.Control> sourceControls, VRCExpressionsMenu targetMenu = null)
            {
                this.sourceControls = sourceControls;
                useSourceControls = true;

                if (sourceControls == null)
                {
                    this.targetMenu = targetMenu;
                    return this;
                }

                return Set(sourceControls.Count, targetMenu);
            }

            /// <summary>
            /// The base overload the others funnel into: a plain count and a target, with no source
            /// to validate.
            /// </summary>
            internal MenuClipboardState Set(int controlsToAdd, VRCExpressionsMenu targetMenu)
            {
                this.controlsToAdd = controlsToAdd;
                this.targetMenu = targetMenu;
                return this;
            }

            /// <summary>
            /// Validates what the Set overloads recorded and fills in the slot counts.
            /// </summary>
            /// <remarks>
            /// The checks run in the order the caller supplied the pieces -- avatar, source, target,
            /// capacity -- so the first thing missing is the one reported, and each failure carries a
            /// distinct <see cref="ValidationResult.errorCode"/>: 1 source menu, 2 source controls,
            /// 3 target menu, 4 over the limit. The missing-avatar case is the exception, keeping the
            /// code 0 it gets from the tuple conversion; that is how the decompiled source has it.
            ///
            /// The capacity test duplicates <see cref="ValidateCanAddControls(VRCExpressionsMenu, int)"/>
            /// -- same literal 8, same message -- rather than calling it, because it also needs the
            /// intermediate <see cref="availableSlots"/> and <see cref="remainingSlots"/> for the
            /// counter the field draws. Ported as found; the two would have to move together.
            /// </remarks>
            internal MenuClipboardState Process()
            {
                if (useAvatar && avatar == null)
                {
                    isWithinLimit = false;
                    validation = (false, "Avatar is not set (Null)");
                    return this;
                }

                if (useSourceMenu && sourceMenu == null)
                {
                    isWithinLimit = false;
                    validation = new ValidationResult(false, "Source Menu is not set (Null)", 1);
                    return this;
                }

                if (useSourceControls && sourceControls == null)
                {
                    isWithinLimit = false;
                    validation = new ValidationResult(false, "Source Controls are null", 2);
                    return this;
                }

                if (targetMenu == null)
                {
                    isWithinLimit = false;
                    validation = new ValidationResult(false, "Target Menu is not set (Null)", 3);
                    return this;
                }

                availableSlots = 8 - targetMenu.controls.Count;
                remainingSlots = availableSlots - controlsToAdd;
                isWithinLimit = remainingSlots >= 0;
                validation = isWithinLimit
                    ? new ValidationResult(true, "Adding Controls Validated")
                    : new ValidationResult(false, $"Adding {controlsToAdd} controls to {targetMenu.name} would exceed the 8 controls limit", 4);
                return this;
            }
        }
    }
}
