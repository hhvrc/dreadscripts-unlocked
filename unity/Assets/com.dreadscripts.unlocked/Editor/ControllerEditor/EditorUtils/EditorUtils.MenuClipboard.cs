// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   struct MenuClipboardState -> MenuClipboardState, line 1581
//     IsValid()  [SpecialName] -> IsValid property, line 1612
//     Set(VRCAvatarDescriptor, VRCExpressionsMenu, VRCExpressionsMenu) -> Set, line 1617
//     Set(VRCAvatarDescriptor, List<Control>, VRCExpressionsMenu)      -> Set, line 1624
//     Set(VRCAvatarDescriptor, int, VRCExpressionsMenu)                -> Set, line 1638
//     Set(VRCExpressionsMenu, VRCExpressionsMenu)                      -> Set, line 1645
//     Set(List<Control>, VRCExpressionsMenu)                           -> Set, line 1662
//     Set(int, VRCExpressionsMenu)                                     -> Set, line 1674
//     Process()                                                        -> Process, line 1681
//     onMenuSelected (private field)                                   -> onMenuSelected, line 1607
//     Draw(Action<VRCExpressionsMenu>, bool, string)                   -> Draw, line 1726
//     DrawCounter()                                                    -> DrawCounter, line 1735
//     OnMenuSelected(VRCExpressionsMenu)                               -> OnMenuSelected, line 1753
//     OnMenuCreated(VRCExpressionsMenu)                                -> OnMenuCreated, line 1768
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The struct is nested inside EditorUtils in the decompiled source (its members sit one indent
// level deeper than EditorUtils' own), so it is ported as a nested type on this partial rather
// than as a file-level type.
//
// The GUI half was held back on the first pass because none of the EditorUtils members it calls
// existed yet. They all do now, and it maps as:
//   PopRules / ComputeRules, line 4302 -> EditorUtils.AssetField<T>, EditorUtils.Fields.cs
//   CallRules, line 4427               -> EditorUtils.IsMissing,     EditorUtils.Fields.cs
//   configurationProperty / _WrapperProcessor, lines 2178/2182 -> validColor / warningColor, EditorUtils.Colors.cs
//   ReadError, line 8263               -> EditorUtils.SetExpressionsMenu, EditorUtils.AvatarDescriptor.cs
//   MenuSelector.InvokeRecord          -> MenuSelector.Open, MenuSelector.cs
// These are call-site cross-references, not claims: each of those members is mapped, with its line
// number, by the file that ports it. They are sub-entries here so that a single decompiled member
// is still claimed by exactly one header.
//
// Also not ported: the static field CancelField (line 1609) and RestartField() (line 1777), which
// returns "CancelField == null". Nothing anywhere assigns the field or calls the method, and the
// sibling ParameterCostTracker carries the identical CompareCandidate/PublishCandidate pair, so
// this is obfuscator scaffolding rather than behaviour -- an always-true method over a field that
// is always null.
// Audit status: PARTIAL -- the struct header and field/method mappings above were re-checked
// against reverse-engineering/export/EditorUtils.cs (MenuClipboardState still opens at line 1581 in the post-561e9ec
// snapshot). The earlier "VERIFIED against reverse-engineering/export/" claim was unsupportable. The GUI-half cross-references were not
// re-walked.

using System;
using System.Collections.Generic;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;
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
            /// The caller's pick callback, handed over by <see cref="Draw"/> and raised from
            /// <see cref="OnMenuSelected"/>.
            /// </summary>
            private Action<VRCExpressionsMenu> onMenuSelected;

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

            /// <summary>
            /// Draws the "Target Menu:" row: an asset field over <see cref="targetMenu"/>, the slot
            /// tally, and the buttons that pick or create a menu.
            /// </summary>
            /// <param name="onSelected">
            /// Raised with whatever the user picks, including null when the field is cleared. The
            /// caller owns the storage -- neither this method nor the field writes
            /// <see cref="targetMenu"/> back.
            /// </param>
            /// <param name="allowNull">Whether the field may be emptied.</param>
            /// <remarks>
            /// Call <see cref="Process"/> before this: the badge comes from
            /// <see cref="validation"/> and the tally from the slot counts, and neither is filled in
            /// until Process has run.
            ///
            /// <paramref name="onSelected"/> is stored in the field rather than passed down, because
            /// what the asset field is given is <see cref="OnMenuSelected"/>, which wraps it. The
            /// assignment has to come first for another reason too: taking a delegate over a struct's
            /// instance method boxes a *copy* of the struct, so the three method groups below capture
            /// this state as it stands at that moment and never see a later change to it.
            /// </remarks>
            internal void Draw(Action<VRCExpressionsMenu> onSelected, bool allowNull = false, string label = "Target Menu:")
            {
                onMenuSelected = onSelected;

                bool isAvatarMainMenu = useAvatar && avatar != null && avatar.expressionsMenu == targetMenu;

                // The field shows a description of the situation rather than the asset's name, so a
                // menu that is merely unset reads differently from one whose asset has been deleted,
                // and the avatar's own main menu is called out wherever it appears.
                string valueText = targetMenu.IsMissing(out bool isDestroyed)
                    ? (isDestroyed
                        ? (isAvatarMainMenu ? "[Avatar's Menu Is Missing!]" : "Menu Is Missing!")
                        : "No Menu Selected")
                    : (isAvatarMainMenu ? "[Avatar's Main Menu]" : targetMenu.name);

                AssetField(label, valueText, targetMenu, OnMenuSelected, validation, DrawCounter, OnMenuCreated, allowNull);
            }

            /// <summary>
            /// The "3/8" tally and the tree-view picker button, drawn in the asset field's right-hand
            /// group.
            /// </summary>
            /// <remarks>
            /// The tooltip carries <see cref="remainingSlots"/>, which goes negative once the batch no
            /// longer fits and is the only place the size of the overrun is shown.
            /// </remarks>
            private void DrawCounter()
            {
                using (new GUIColorScope(GUIColorScope.ColoringType.FG, isWithinLimit, validColor, warningColor))
                {
                    GUILayout.Label(new GUIContent($"{controlsToAdd}/{availableSlots}", $"Remaining: {remainingSlots}"),
                        styles.noteRight, GUILayout.ExpandWidth(expand: false));
                }

                // The selector opens on the current target, so with none set there is no tree to
                // browse and the button is disabled rather than hidden.
                using (new EditorGUI.DisabledScope(targetMenu == null))
                {
                    if (Button(new GUIContent(contents.sceneHierarchy) { tooltip = "Select Menu From TreeView" }, styles.iconButton))
                    {
                        MenuSelector.Open(targetMenu, OnMenuSelected, controlsToAdd);
                    }
                }
            }

            /// <summary>
            /// Forwards the user's pick to the caller, then offers to make it the avatar's main menu
            /// if the avatar has none.
            /// </summary>
            /// <remarks>
            /// The offer is a context menu with a single "Yes" item and no "No": dismissing it by
            /// clicking away is the decline. That is how the original behaves.
            ///
            /// Accepting calls <see cref="SetExpressionsMenu"/>, which writes the descriptor and marks
            /// it dirty without registering an <see cref="Undo"/> -- so the assignment cannot be taken
            /// back with Ctrl+Z. It only fires when the slot was empty, so nothing is displaced here,
            /// but the method itself does not track a menu it replaces.
            ///
            /// The descriptor is copied into a local because a lambda cannot capture a field of
            /// <c>this</c> on a struct.
            /// </remarks>
            private void OnMenuSelected(VRCExpressionsMenu menu)
            {
                onMenuSelected(menu);

                if (useAvatar && avatar != null && avatar.expressionsMenu == null)
                {
                    VRCAvatarDescriptor descriptor = avatar;
                    GenericMenu offer = new GenericMenu();
                    offer.AddItem(new GUIContent("Set As Avatar's Main Menu?/Yes"), on: false, delegate
                    {
                        descriptor.SetExpressionsMenu(menu);
                    });
                    offer.ShowAsContext();
                }
            }

            /// <summary>
            /// Gives a menu asset the user just created through the field an empty controls list.
            /// </summary>
            /// <remarks>
            /// Same repair as the <see cref="Set(VRCExpressionsMenu, VRCExpressionsMenu)"/> overload
            /// performs on a source menu, and for the same reason: a menu asset saved before it ever
            /// held a control has a null list, and everything downstream -- including
            /// <see cref="Process"/>, which reads <c>targetMenu.controls.Count</c> unguarded -- assumes
            /// there is one.
            /// </remarks>
            private void OnMenuCreated(VRCExpressionsMenu menu)
            {
                if (menu.controls == null)
                {
                    menu.controls = new List<VRCExpressionsMenu.Control>();
                    EditorUtility.SetDirty(menu);
                }
            }
        }
    }
}
