// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ControllerEditorWindow.IncludeTests -> DrawBehavioursAndCosmeticsTab, line 3340
// Line numbers are relative to the decompiled snapshot at the time of the port; the member name is
// the durable reference. See ControllerEditorWindow.cs for the full member map and for what this
// type deliberately leaves unported.
//
// IncludeTests is one 270-line method in the decompiled source. It is split here into one method
// per box it draws -- DrawAnimationWindowSection, DrawAnimatorWindowSection and the five section
// bodies beneath it. No control flow changes: each split point is a top-level box scope in the
// original, and the foldout guards that read there as "if (expanded) { ... }" read as an early
// return here only because the guarded body is now the whole method.
//
// One omission lives in this file: the change handler behind the "Overriding" toggle (decompiled
// lines 3363-3368). It is marked at its call site, which carries the current blocker in full. In
// short, and unlike the other deferrals this port started with, it is still genuinely blocked on
// missing code: TestInitializer (line 15253) has no port anywhere in the package, and the two
// fields it clears alongside are ported but `private` on ControllerEditor.
//
// Audit status: PARTIAL.
//   Checked on 2026-08-05, statement by statement against export ControllerEditor.cs:
//     * DrawBehavioursAndCosmeticsTab (3340) and DrawAnimationWindowSection (3341-3390) -- the two
//       members that bracket the deferral -- match, including the five aw_* settings rows, the
//       GUIColorScope over the master toggle and the early return on the collapsed foldout.
//     * The deferred change handler at 3363-3368 was re-derived in full from export, which is how
//       its blocker was corrected; the previous note's claim that all three targets were unported
//       was wrong, and is now stated accurately at the call site.
//   NOT checked: the nine remaining section bodies -- DrawAnimatorWindowSection, DrawLayerOptions,
//     DrawParameterOptions, DrawTransitionOptions, DrawNodeOptions, DrawColorOptions,
//     DrawTransitionColors, DrawGraphColors, DrawNodeColors -- which together carry decompiled
//     lines 3391-3608. Their split from the single 270-line IncludeTests is documented above and
//     their structure was not re-derived on this pass. This file is the one in its folder that
//     still needs a full member-by-member diff.
//
// The decompiled method ends in a chain of early `return`s inside nested `using` blocks (lines
// 3390, 3486, 3599). Every one of them sits in the tail position of its enclosing scope, so they
// are exits, not skips; they are written here as the ordinary nested conditionals they came from.

using DreadScripts.Common;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditorWindow
    {
        /// <summary>
        /// Draws the first tab: which of the tool's editor-window behaviours are active, and how the
        /// Animator window is decorated.
        /// </summary>
        private static void DrawBehavioursAndCosmeticsTab()
        {
            DrawAnimationWindowSection();
            DrawAnimatorWindowSection();
        }

        /// <summary>
        /// The Animation window integrations, behind a master on/off switch. Unlike every other
        /// section here these change what the *Animation* window does, not how the Animator window
        /// looks, which is why they sit apart at the top.
        /// </summary>
        private static void DrawAnimationWindowSection()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    animationWindowExpanded = EditorGUILayout.Foldout(animationWindowExpanded, "Animation Window");
                    GUILayout.FlexibleSpace();

                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, EditorSettings.Instance.aw_active, Color.green, Color.grey))
                    {
                        EditorSettings.Instance.aw_active.value = EditorUtils.ToggleButton(
                            EditorSettings.Instance.aw_active,
                            EditorSettings.Instance.aw_active.value ? "Enabled" : "Disabled");
                    }
                }

                if (!animationWindowExpanded)
                {
                    return;
                }

                using (new EditorGUI.DisabledScope(!EditorSettings.Instance.aw_active))
                {
                    using (new IndentedLayoutScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorSettings.Instance.aw_enableOverride.Draw(new GUIContent("Overriding", "Allows you to explicitly set the controller for selecting clips, and explicitly set the root to change what the paths are relative to."));
                            if (EditorGUI.EndChangeCheck())
                            {
                                // DEFERRED. The shipped build tears the override down here, in
                                // three statements (decompiled lines 3365-3367):
                                //     TestInitializer(null);
                                //     overrideAnimationRoot = null;
                                //     overrideAnimationRootActive = false;
                                //
                                // The two fields HAVE since been ported -- overrideAnimationRoot
                                // (line 8282) and overrideAnimationRootActive (line 8280) are at
                                // ControllerEditor.State.cs lines 1119 and 1112 -- but both are
                                // `private static` on ControllerEditor, which this window cannot
                                // reach now that it is a separate top-level type rather than a
                                // nested one.
                                //
                                // TestInitializer itself (line 15253) is the genuine remaining gap:
                                // nothing in the package claims that line. It is the setter for the
                                // proxy AnimatorController the Animation-window override drives --
                                // passed null it destroys the proxy GameObject, passed a controller
                                // it assigns the controller and pushes the proxy into the Animation
                                // window through the reflected EditGameObjectInternal. It depends
                                // in turn on three more unported god-class members
                                // (InstantiateInitializer, FlushInitializer,
                                // forceGameObjectSelectionUpdate) and on animationWindowType.
                                //
                                // So this needs both a port of TestInitializer and `internal` on
                                // the two fields. The setting still draws and still persists; only
                                // the teardown is missing, so a stale override survives the toggle.
                            }

                            EditorSettings.Instance.aw_enablePropertyEditing.Draw(new GUIContent("Edit Property", "Allows you to drag and drop objects to properties and to edit the properties of the curves with right-click context menu."));
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            EditorSettings.Instance.aw_enableGameObjectDND.Draw(new GUIContent("Drag & Drop", "Allows you to drag and drop GameObjects to the animation window to add them as a new curve."));
                            EditorSettings.Instance.aw_autoSwitchClip.Draw(new GUIContent("Auto-Switch Clip", "Automatically switch the clip in the animation window when selecting a state."));
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            EditorSettings.Instance.aw_warnPropertyMerge.Draw(new GUIContent("Property Merge Log", "Warn in the console when merging properties through property modification."));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Everything the tool changes about the Animator window, grouped the way the window itself
        /// is: layers, parameters, transitions, nodes, colours.
        /// </summary>
        private static void DrawAnimatorWindowSection()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref animatorWindowExpanded, "Animator Window"))
                {
                    if (!animatorWindowExpanded)
                    {
                        return;
                    }

                    DrawLayerOptions();
                    DrawParameterOptions();
                    DrawTransitionOptions();
                    DrawNodeOptions();
                    DrawColorOptions();
                }
            }
        }

        private static void DrawLayerOptions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref layersExpanded, "Layers"))
                {
                    if (!layersExpanded)
                    {
                        return;
                    }

                    EditorSettings.Instance.categoryBaseName.Draw(new GUIContent("Uncategorized Name", "Name of the category for layers without a category."), true, true);
                    EditorSettings.Instance.categoryDelimiter.Draw(new GUIContent("Category Delimiter", "The character used to separate categories in the layer view."), true, true);

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorSettings.Instance.displayCategoryView.Draw(new GUIContent("Category View", "Displays options to view layers in categories."));
                        EditorSettings.Instance.displayLayerCompactView.Draw(new GUIContent("Compact View", "Displays a button to view layers in a compact manner."));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorSettings.Instance.displayLayerIndex.Draw(new GUIContent("Layer Index", "Shows a small number on the layer's GUI for the layer's index in the list of layers."));
                        EditorSettings.Instance.autoFrameLayer.Draw(new GUIContent("Auto-Frame Layer", "Upon selecting a layer, automatically frame the statemachine. Behaviour is similar to pressing 'A' after clicking the graph."));
                    }
                }
            }
        }

        private static void DrawParameterOptions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref parametersExpanded, "Parameters"))
                {
                    if (!parametersExpanded)
                    {
                        return;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        typeIndicatorExpanded = EditorGUILayout.Foldout(typeIndicatorExpanded, "Type Indicator");
                        GUILayout.FlexibleSpace();

                        using (new GUIColorScope(GUIColorScope.ColoringType.BG, EditorSettings.Instance.displayParameterType, Color.green, Color.grey))
                        {
                            EditorSettings.Instance.displayParameterType.value = EditorUtils.ToggleButton(
                                EditorSettings.Instance.displayParameterType,
                                EditorSettings.Instance.displayParameterType.value ? "Enabled" : "Disabled");
                        }
                    }

                    using (new EditorGUI.DisabledScope(!EditorSettings.Instance.displayParameterType))
                    {
                        if (typeIndicatorExpanded)
                        {
                            using (new IndentedLayoutScope())
                            {
                                EditorSettings.Instance.capitalParameterIndicator.Draw(new GUIContent("Capital Letters", "Changes 'f' to 'F' and 'i' to 'I'"));
                                EditorSettings.Instance.parameterLabelFontStyle.DrawEnumPopup<FontStyle>(new GUIContent("Font style", "The font style of the parameter indicators."));
                                EditorSettings.Instance.parameterLabelColor.Draw(new GUIContent("Font Color", "The color of the parameter indicators. Supports Alpha."), true);
                            }
                        }
                    }
                }
            }
        }

        private static void DrawTransitionOptions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref transitionsExpanded, "Transitions"))
                {
                    if (!transitionsExpanded)
                    {
                        return;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            EditorSettings.Instance.autoReverseModes.Draw(new GUIContent("Auto Reverse Mode", "Reverse Transitions should also reverse the condition modes"));
                            EditorSettings.Instance.animateInboundEdges.Draw(new GUIContent("Animate In Transitions", "Incoming transitions to selected states get animated."));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            EditorSettings.Instance.reverseModifiesValues.Draw(new GUIContent("Reverse Adjusts Values", "Reversing a condition will also modify its values appropriately. Hold CTRL to temporarily flip this setting while reversing"));
                            EditorSettings.Instance.animateOutboundEdges.Draw(new GUIContent("Animate Out Transitions", "Outgoing transitions from selected states get animated."));
                        }
                    }

                    // The range is signed because the ratio is measured from the midpoint of the
                    // edge: -1 puts the arrowhead at the source end, +1 at the destination.
                    EditorSettings.Instance.arrowLerpRatio.DrawSlider(new GUIContent("Arrow Location", "Where the arrow exists on transitions."), -1f, 1f, true);
                }
            }
        }

        private static void DrawNodeOptions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref nodesExpanded, "Nodes"))
                {
                    if (!nodesExpanded)
                    {
                        return;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorSettings.Instance.switchDoubleClick.Draw(new GUIContent("Alternate Double Click", "Switch Double click's behaviour on states. Ctrl Double Click will do the other behaviour"));
                        EditorSettings.Instance.stateCosmetics.DrawEnumPopup<StateCosmeticOptions>("State Extras", true);
                    }
                }
            }
        }

        private static void DrawColorOptions()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new FoldoutScope(ref colorsExpanded, "Colors"))
                {
                    if (!colorsExpanded)
                    {
                        return;
                    }

                    DrawTransitionColors();
                    DrawGraphColors();
                    DrawNodeColors();
                }
            }
        }

        private static void DrawTransitionColors()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    transitionColorsExpanded = EditorGUILayout.Foldout(transitionColorsExpanded, "Transition Colors");
                    GUILayout.FlexibleSpace();

                    bool active = EditorSettings.Instance.cosmeticTransitionsActive.value;
                    string label = active ? "Enabled" : "Disabled";

                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, active, Color.green, Color.grey))
                    {
                        EditorSettings.Instance.cosmeticTransitionsActive.value = EditorUtils.ToggleButton(active, label);
                    }
                }

                using (new EditorGUI.DisabledScope(!EditorSettings.Instance.cosmeticTransitionsActive))
                {
                    if (transitionColorsExpanded)
                    {
                        using (new IndentedLayoutScope())
                        {
                            EditorSettings.Instance.normalTransitionColor.Draw("Normal Transition", true);
                            EditorSettings.Instance.entryTransitionColor.Draw("Entry Transition", true);
                            EditorSettings.Instance.selectedTransitionColor.Draw("Selected Transition", true);
                            EditorSettings.Instance.baseTransitionColor.Draw("Base Transition", true);
                        }
                    }
                }
            }
        }

        /// <remarks>
        /// The master switch is the only control on this tab wrapped in a
        /// <see cref="SettingsChangeScope"/>. It has to be: the graph background is not a value the
        /// Animator window reads back every frame but a <c>GUIStyle</c> written into
        /// <c>UnityEditor.Graphs.Styles.graphBackground</c> by reflection, so turning the switch off
        /// has no effect until something rebuilds that style. The other three settings that feed the
        /// same style raise the hook from their own change callbacks instead -- see
        /// <see cref="EditorSettings.onGraphBackgroundChanged"/>.
        /// </remarks>
        private static void DrawGraphColors()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    graphColorsExpanded = EditorGUILayout.Foldout(graphColorsExpanded, "Graph Colors");
                    GUILayout.FlexibleSpace();

                    bool active = EditorSettings.Instance.cosmeticGraphActive.value;
                    string label = active ? "Enabled" : "Disabled";

                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, active, Color.green, Color.grey))
                    {
                        using (new SettingsChangeScope(() => EditorSettings.onGraphBackgroundChanged?.Invoke()))
                        {
                            EditorSettings.Instance.cosmeticGraphActive.value = EditorUtils.ToggleButton(active, label);
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(!EditorSettings.Instance.cosmeticGraphActive))
                {
                    if (!graphColorsExpanded)
                    {
                        return;
                    }

                    using (new IndentedLayoutScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            // The background is either a flat colour or a texture, never both, so
                            // the two settings share one row and the "T" toggle picks which is live.
                            if (EditorSettings.Instance.graphBackgroundIsTexture)
                            {
                                EditorSettings.Instance.graphBackgroundTexture.Draw("Background", false, GUILayout.Height(17f), GUILayout.ExpandWidth(expand: true));
                            }
                            else
                            {
                                EditorSettings.Instance.gridBackgroundColor.Draw("Background", false);
                            }

                            EditorSettings.Instance.graphBackgroundIsTexture.value = EditorUtils.ToggleButton(
                                EditorSettings.Instance.graphBackgroundIsTexture,
                                new GUIContent("T", "Use Texture"),
                                GUI.skin.button,
                                GUILayout.Width(18f),
                                GUILayout.Height(18f));

                            if (EditorUtils.IconButton(EditorUtils.contents.reset))
                            {
                                if (EditorSettings.Instance.graphBackgroundIsTexture)
                                {
                                    EditorSettings.Instance.graphBackgroundTexture.Reset();
                                }
                                else
                                {
                                    EditorSettings.Instance.gridBackgroundColor.Reset();
                                }
                            }
                        }

                        // Only the pair matching the current skin is offered, because the Animator
                        // window's grid patch reads whichever pair the skin selects and the other
                        // two would be edits with no visible result.
                        if (EditorGUIUtility.isProSkin)
                        {
                            EditorSettings.Instance.gridMinorDarkColor.Draw("Minor Line", true);
                            EditorSettings.Instance.gridMajorDarkColor.Draw("Major Line", true);
                        }
                        else
                        {
                            EditorSettings.Instance.gridMinorLightColor.Draw("Minor Line", true);
                            EditorSettings.Instance.gridMajorLightColor.Draw("Major Line", true);
                        }
                    }
                }
            }
        }

        private static void DrawNodeColors()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    nodeColorsExpanded = EditorGUILayout.Foldout(nodeColorsExpanded, "Node Colors");
                    GUILayout.FlexibleSpace();

                    bool active = EditorSettings.Instance.cosmeticNodesActive.value;
                    string label = active ? "Enabled" : "Disabled";

                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, active, Color.green, Color.grey))
                    {
                        EditorSettings.Instance.cosmeticNodesActive.value = EditorUtils.ToggleButton(active, label);
                    }
                }

                using (new EditorGUI.DisabledScope(!EditorSettings.Instance.cosmeticNodesActive))
                {
                    if (!nodeColorsExpanded)
                    {
                        return;
                    }

                    using (new IndentedLayoutScope())
                    {
                        DrawNodeColorField(EditorSettings.Instance.normalStateNodeColor, "State Node");
                        DrawNodeColorField(EditorSettings.Instance.machineStateNodeColor, "Machine Node");
                        DrawNodeColorField(EditorSettings.Instance.defaultStateNodeColor, "Default Node");
                        DrawNodeColorField(EditorSettings.Instance.anyStateNodeColor, "AnyState Node");
                        DrawNodeColorField(EditorSettings.Instance.entryStateNodeColor, "Entry Node");
                        DrawNodeColorField(EditorSettings.Instance.exitStateNodeColor, "Exit Node");
                    }
                }
            }
        }
    }
}
