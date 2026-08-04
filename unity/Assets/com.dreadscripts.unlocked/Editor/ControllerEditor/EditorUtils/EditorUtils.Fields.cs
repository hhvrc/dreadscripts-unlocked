// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CallRules    -> IsMissing(this UnityEngine.Object, out bool), line 4427
//   static PopRules     -> AssetField<T>(string, ...),                   line 4302
//   static ComputeRules -> AssetField<T>(GUIContent, ...),               line 4307
// IsMissing is named for what it returns rather than for the question it is usually asked: the
// decompiled body returns true when the reference is *not* usable, and every call site branches on
// that to pick placeholder text, so "IsAssigned" would read backwards at each of them.
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// PopRules and ComputeRules are an obfuscator-split overload pair -- the first does nothing but
// wrap its label in a GUIContent and forward -- and are collapsed here the same way
// EditorUtils.Buttons.cs collapses its own, keeping both entry points because all three ported
// call sites (ControllerPicker.Draw line 1379, ParameterCostTracker.Draw line 1529,
// MenuClipboardState.Draw line 1732) pass a plain string.
//
//   The decompiled parameters map as:
//     asset / i            -> label          the row's leading label, at a fixed 120px column.
//     b / attr             -> valueText      what is drawn *inside* the field. Not the asset's
//                                            name: callers pass "No Menu Selected", "Menu Is
//                                            Missing!" or "[Avatar's Main Menu]" as appropriate,
//                                            which is what IsMissing below exists to choose
//                                            between. Drawn rich-text, so it may carry markup.
//     util / state         -> value          the asset currently referenced; may be null.
//     param2 / selection2  -> onSelected     raised with the newly picked asset, and with null
//                                            when the field is cleared. The caller owns the
//                                            storage -- this method never writes anything back.
//     config3 / pol3       -> validation     drives the tick/warning badge on the right of the
//                                            field. A default (unset) result draws no badge at
//                                            all, which is why ValidationResult.isSet exists.
//     counter4 / visitor4  -> drawCounter    optional trailing tally drawn in the 90px right-hand
//                                            group, before the select/create buttons -- the
//                                            "3/8" cost and slot counters.
//     ivk5 / second5       -> onCreated      raised on an asset the user just created through the
//                                            create button, before onSelected sees it.
//     isord6 / containstask6 -> allowNull    whether the field may be emptied: gates both the
//                                            delete-key clear and forwarding a null pick.
//     cust7 / asset7       -> assetExtension file extension for assets created from this field.
//                                            Null means "look it up from T" (decompiled
//                                            CancelRules), which falls back to "asset".
//
//   The members the body calls, all ported since the first pass on this file:
//     VerifyResolver, line 2873   -> Expand,               EditorUtils.Rects.cs
//     VerifyQueue, line 6295      -> DeletePressed,        EditorUtils.Events.cs
//     PrintRules, line 5554       -> FocusWindow,          EditorUtils.Windows.cs
//     InstantiateRules, line 4817 -> HandleDragAndDrop,    EditorUtils.DragAndDrop.cs
//     MoveRules, line 4382        -> AssetButtons,         EditorUtils.AssetButtons.cs
//     CancelRules, line 4434      -> TryGetAssetExtension, EditorUtils.AssetTypes.cs
//     ConcatList, line 6690       -> ShowObjectPicker,     EditorUtils.Pickers.cs
//     configurationProperty / _WrapperProcessor (lines 2178/2182) -> validColor / warningColor,
//                                    EditorUtils.Colors.cs
// Audit status: UNAUDITED -- was VERIFIED in 2b1c7ff, but the code has changed
// since (-5 code lines); needs re-checking against export/ before the claim is restored.

using System;
using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Reports whether <paramref name="obj"/> still refers to a live object, separating the two
        /// ways a reference can be empty: never assigned, and assigned to something that no longer
        /// exists.
        /// </summary>
        /// <param name="isDestroyed">
        /// True only in the second case -- the reference was set, and the object behind it has since
        /// been destroyed or its asset deleted. False both when the reference is live and when it was
        /// never assigned at all.
        /// </param>
        /// <returns>
        /// True when there is nothing usable behind the reference, i.e. either of those two cases.
        /// The asset-field callers invert the meaning of that in their message: an unassigned field
        /// reads "No Menu Selected", a destroyed one "Menu Is Missing!".
        /// </returns>
        /// <remarks>
        /// <para>
        /// The cast to <see cref="object"/> is load-bearing and must not be simplified away.
        /// UnityEngine.Object overloads <c>==</c> so that a managed wrapper whose native object has
        /// been destroyed compares equal to null -- the "fake null" that makes destroyed objects
        /// behave like missing ones. Casting to <see cref="object"/> selects the plain reference
        /// comparison instead, which sees the wrapper that is still very much there.
        /// </para>
        /// <para>
        /// So the two comparisons deliberately disagree, and that disagreement <em>is</em> the
        /// answer: reference-non-null but Unity-null means destroyed. Rewriting either side to match
        /// the other collapses both cases into one and loses the distinction this method exists to
        /// draw.
        /// </para>
        /// </remarks>
        internal static bool IsMissing(this UnityEngine.Object obj, out bool isDestroyed)
        {
            bool hasReference = (object)obj != null;
            isDestroyed = hasReference && obj == null;
            return !hasReference || isDestroyed;
        }

        /// <summary>
        /// Draws a labelled asset row: a label column, an object-field-shaped area showing
        /// <paramref name="valueText"/>, and the select/create buttons. The
        /// <see cref="GUIContent"/> label is wrapped for you.
        /// </summary>
        internal static void AssetField<T>(string label, string valueText, T value, Action<T> onSelected,
                                           ValidationResult validation, Action drawCounter,
                                           Action<T> onCreated = null, bool allowNull = true,
                                           string assetExtension = null) where T : UnityEngine.Object
        {
            AssetField(new GUIContent(label), valueText, value, onSelected, validation, drawCounter, onCreated, allowNull, assetExtension);
        }

        /// <summary>
        /// The tool's stand-in for <see cref="EditorGUILayout.ObjectField(GUIContent, UnityEngine.Object, Type, bool, GUILayoutOption[])"/>:
        /// a labelled row that looks like an object field but draws caller-supplied text in place of
        /// the asset's name, carries a validity badge, and ends in a select/create button group.
        /// </summary>
        /// <param name="label">The row's leading label, in a fixed 120px column.</param>
        /// <param name="valueText">
        /// What is drawn inside the field. Deliberately not the asset's name: callers substitute
        /// "No Menu Selected", "Menu Is Missing!" or "[Avatar's Main Menu]" as the situation calls
        /// for -- which is what <see cref="IsMissing"/> exists to choose between -- and the text is
        /// drawn rich, so it may carry markup.
        /// </param>
        /// <param name="value">The asset currently referenced. May be null.</param>
        /// <param name="onSelected">
        /// Raised with the newly picked asset, and with null when the field is cleared. The caller
        /// owns the storage; this method never writes anything back.
        /// </param>
        /// <param name="validation">
        /// Drives the tick/warning badge at the right of the field. A default (unset) result draws
        /// no badge at all, which is why <see cref="ValidationResult.isSet"/> exists.
        /// </param>
        /// <param name="drawCounter">
        /// Optional trailing tally, drawn in the 90px right-hand group before the buttons -- the
        /// "3/8" cost and slot counters.
        /// </param>
        /// <param name="onCreated">
        /// Raised on an asset the user just created through the create button, before
        /// <paramref name="onSelected"/> sees it.
        /// </param>
        /// <param name="allowNull">
        /// Whether the field may be emptied. Gates both the delete-key clear and the forwarding of a
        /// null pick.
        /// </param>
        /// <param name="assetExtension">
        /// Extension for assets created from this field. Null means "look it up from
        /// <typeparamref name="T"/>" via <see cref="TryGetAssetExtension"/>, which falls back to
        /// "asset".
        /// </param>
        /// <remarks>
        /// <para>
        /// The field area is a horizontal scope styled as <see cref="EditorStyles.objectField"/>
        /// rather than a real object field, so every interaction it would normally give for free has
        /// to be reproduced by hand against the rect the layout system reports: the click-to-pick,
        /// the double-click and right-click behaviour, the ping, the drop target, and the delete-key
        /// clear.
        /// </para>
        /// <para>
        /// The click behaviour is worth spelling out, since it inverts what an object field does. A
        /// plain single left click <em>pings</em> the asset in the Project browser; the picker only
        /// opens on a double click, on a right click, or when the field is empty and so has nothing
        /// to ping. That makes the common case -- "where is this thing?" -- one click, at the cost of
        /// making reassignment two.
        /// </para>
        /// </remarks>
        internal static void AssetField<T>(GUIContent label, string valueText, T value, Action<T> onSelected,
                                           ValidationResult validation, Action drawCounter,
                                           Action<T> onCreated = null, bool allowNull = true,
                                           string assetExtension = null) where T : UnityEngine.Object
        {
            if (assetExtension == null)
            {
                TryGetAssetExtension(typeof(T), out assetExtension);
            }

            bool isSet = validation.isSet;
            bool isValid = validation.isValid;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.MaxWidth(120f));

                using (new GUILayout.HorizontalScope(EditorStyles.objectField, GUILayout.ExpandHeight(expand: true)))
                {
                    // Not a `using`, and not a leak: the scope has to exist only when there is a
                    // validity to tint by, so that an unvalidated field draws in the ordinary content
                    // colour rather than in green. A `using` over a conditionally-null scope would
                    // throw on dispose, and hoisting the condition into the GUIColorScope(bool, Color)
                    // overload would still tint the *false* case, which is not what is wanted here.
                    // The construct/dispose pair brackets exactly the one label below.
                    GUIColorScope colorScope = isSet
                        ? new GUIColorScope(GUIColorScope.ColoringType.FG, isValid, validColor, warningColor)
                        : null;

                    GUILayout.Label(valueText, styles.centeredBoldRichLabel);

                    if (isSet)
                    {
                        colorScope.Dispose();
                    }
                }

                Rect fieldRect = GUILayoutUtility.GetLastRect();

                Texture2D typeIcon = AssetPreview.GetMiniTypeThumbnail(typeof(T));
                if (typeIcon == AssetPreview.GetMiniTypeThumbnail(typeof(ScriptableObject)))
                {
                    // ScriptableObject's thumbnail is the generic script icon Unity hands back for
                    // anything it has no artwork for, so getting it means "no icon" rather than "a
                    // ScriptableObject". The GameObject cube reads better as a placeholder.
                    typeIcon = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
                }

                // Measured, not consumed: both badges are laid over the field rect rather than
                // carved out of it, so the text underneath keeps the full width.
                GUI.DrawTexture(fieldRect.SliceLeft(18f, absolute: true, 2f, offsetAbsolute: true, consume: false).Expand(-1f), typeIcon);

                if (isSet)
                {
                    Texture badgeImage = isValid ? contents.upToDate.image : contents.warning.texture;
                    GUI.Label(fieldRect.SliceRight(18f, absolute: true, 4f, offsetAbsolute: true, consume: false).Expand(-1f),
                        isValid
                            ? new GUIContent(badgeImage) { tooltip = "All Good!" }
                            : new GUIContent(badgeImage) { tooltip = validation.message },
                        styles.iconButton);
                }

                AddLinkCursor(fieldRect);

                EventWrapper mouseDown = new EventWrapper(Event.current).IsMouseDown().InRect(fieldRect);
                int controlID = GUIUtility.GetControlID(FocusType.Keyboard, fieldRect);

                if (allowNull && GUIUtility.keyboardControl == controlID && DeletePressed())
                {
                    onSelected(null);
                }

                if (mouseDown.isValid)
                {
                    GUIUtility.keyboardControl = controlID;

                    if (value == null || (bool)mouseDown.IsDoubleClick().IsLeftButton() || (bool)mouseDown.IsRightButton())
                    {
                        ShowObjectPicker(value, typeof(T), allowSceneObjects: false, onSelectionChanged: delegate(UnityEngine.Object picked)
                        {
                            if (allowNull || picked != null)
                            {
                                onSelected((T)picked);
                            }
                        });
                    }
                    else
                    {
                        // The browser has to be the active tab of its dock for a ping to be visible,
                        // so it is brought forward first and focus handed straight back.
                        FocusWindow("ProjectBrowser", restoreFocus: true);
                        EditorGUIUtility.PingObject(value);
                    }

                    mouseDown.Use();
                }

                HandleDragAndDrop(fieldRect, onSelected);

                using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(90f)))
                {
                    GUILayout.FlexibleSpace();
                    drawCounter?.Invoke();

                    // No ping button: the field itself already pings on a single click.
                    AssetButtons(onSelected, value, onCreated, assetExtension, allowNull, showPing: false);
                }
            }
        }
    }
}
