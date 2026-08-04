// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ControllerEditorWindow -> ControllerEditorWindow, lines 3190-3921 (vendor name; unobfuscated)
//     NodeColor            -> NodeColor,             line 3192 (vendor name)
//     m_AdvisorMapper      -> targetAnimator,        line 3203
//     _CallbackMapper      -> alwaysUseTargetAnimator, line 3205
//     indexerMapper        -> tabIndex,              line 3207
//     m_IssuerMapper       -> tabLabels,             line 3209
//     _PrototypeMapper     -> defaultsTabIndex,      line 3211
//     _RuleMapper          -> defaultsTabLabels,     line 3213
//     m_SingletonMapper    -> noOptions,             line 3215
//     _FactoryMapper       -> stateObject,           line 3217
//     m_Name               -> m_Name (kept: surviving vendor name), line 3219
//     _AccountMapper       -> stateSpeed,            line 3221
//     m_RefMapper          -> stateCycleOffset,      line 3223
//     m_StatusMapper       -> stateIKOnFeet,         line 3225
//     _TokenMapper         -> stateWriteDefaults,    line 3227
//     _CodeMapper          -> stateMirror,           line 3229
//     _DicMapper           -> stateSpeedParameterActive,       line 3231
//     invocationMapper     -> stateMirrorParameterActive,      line 3233
//     roleMapper           -> stateCycleOffsetParameterActive, line 3235
//     paramMapper          -> stateTimeParameterActive,        line 3237
//     modelMapper          -> stateMotion,           line 3239
//     tokenizerMapper      -> stateTag,              line 3241
//     _DecoratorMapper     -> stateSpeedParameter,   line 3243
//     _ComparatorMapper    -> stateMirrorParameter,  line 3245
//     m_ExceptionMapper    -> stateCycleOffsetParameter, line 3247
//     objectMapper         -> stateTimeParameter,    line 3249
//     _UtilsMapper         -> transitionObject,      line 3251
//     _ValMapper           -> transitionSolo,        line 3253
//     valueMapper          -> transitionMute,        line 3255
//     _MerchantMapper      -> transitionDuration,    line 3257
//     m_AuthenticationMapper -> transitionOffset,    line 3259
//     reponseMapper        -> transitionExitTime,    line 3261
//     m_PoolMapper         -> transitionHasExitTime, line 3263
//     _ParameterMapper     -> transitionHasFixedDuration,   line 3265
//     _ComposerMapper      -> transitionInterruptionSource, line 3267
//     repositoryMapper     -> transitionOrderedInterruption, line 3269
//     _MappingMapper       -> transitionCanTransitionToSelf, line 3271
//     _BaseMapper          -> dropped,               line 3273 (see DEOBF-BUG below)
//     containerMapper      -> scrollPosition,        line 3275
//     _ClassMapper         -> animationWindowExpanded,   line 3277
//     mockMapper           -> animatorWindowExpanded,    line 3279
//     instanceMapper       -> layersExpanded,            line 3281
//     m_FieldMapper        -> parametersExpanded,        line 3283
//     _AttributeMapper     -> typeIndicatorExpanded,     line 3285
//     _ClientMapper        -> nodesExpanded,             line 3287
//     configMapper         -> transitionsExpanded,       line 3289
//     m_DescriptorMapper   -> graphColorsExpanded,       line 3291
//     templateMapper       -> nodeColorsExpanded,        line 3293
//     m_MessageMapper      -> defaultLayerOptionsExpanded, line 3295
//     collectionMapper     -> colorsExpanded,            line 3297
//     _ParserMapper        -> transitionColorsExpanded,  line 3299
//     PushTests()          -> IsProSkin,             line 3302 [SpecialName getter]
//     CalcTests            -> Open,                  line 3308
//     IncludeTests         -> DrawBehavioursAndCosmetics, line 3340
//     RunTests             -> DrawDefaults,          line 3610
//     CloneTests           -> DrawTransitionDefaults, line 3628
//     LoginTests           -> DrawStateDefaults,     line 3690
//     ReflectTests         -> DrawOtherDefaults,     line 3794
//     DeleteTests          -> RebuildTransitionObject, line 3853
//     CreateTests          -> RebuildStateObject,    line 3875
//     NewTests             -> DrawNodeColorField,    line 3910 (made private, see below)
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// m_Name keeps its decompiled name: it carries no member-family suffix while every field around it
// carries "Mapper", which is the signal RE_NOTES gives for a name that survived obfuscation.
//
// LICENCE CODE REMOVED from OnGUI (export line 3313):
//   - `if (!OrderVisitor(this)) return;` was the licence gate around the entire window body — the
//     same shape as ADOverhaulWindow.OnGUI (RE_NOTES, "Stripping the licence code"). The gate is
//     dropped and the drawing it wrapped is kept, which is what stripping means here; deleting the
//     method would delete the settings window.
//   - `RevertAnnotation();` (export line 3334) drew the "License: <tier>" / "Authorized For: <user>"
//     footer strip. Dropped outright.
//   DefineVisitor() is kept: it is the update/announcement footer and the hamburger menu, not
//   licence code. It lives in the outer class body and is not ported yet.
//
// DEOBF-BUG(guessed) -- OnEnable, export line 3843.
//   export/ renders it as `if (!_BaseMapper) { while (true) { DeleteTests(); CreateTests(); } }`.
//   `_BaseMapper` is a private static bool with no write anywhere in the module, so the guard is
//   always true and the loop would hang the Editor the first time the settings window is enabled —
//   not shippable behaviour. Both are the known de4dot shapes: a Reactor opaque predicate over a
//   never-written static (../de4dot/ROADMAP.md, NeverWrittenStaticFields) plus a flattened `if`
//   recovered as a `while` (RE_NOTES, "Shapes of decompile damage"). Ported as the two calls,
//   unconditionally. GUESSED: the original IL was not traced and no second build carries this
//   method. What would settle it is a trace of the corresponding obfuscated method.
//
// DEOBF-BUG(guessed) -- RebuildTransitionObject, export line 3853.
//   export/ puts `transitionObject = new SerializedObject(...)` and four of the ten FindProperty
//   calls in the `else` of the null check, then the remaining six outside it — so the branch that
//   creates a missing default transition leaves `transitionObject` stale or null and the six calls
//   below it throw. The sibling RebuildStateObject, twenty lines down, does the same job with the
//   assignment outside the null check and every FindProperty after it; that is the shape ported
//   here. GUESSED: an ADOverhaul build would settle it, but neither carries this method, and the
//   original IL was not traced.
//
// DrawNodeColorField is private here, `internal` in export. As `internal` it does not compile:
// EditorSettings is a private nested type, so an internal method cannot take one as a parameter
// (CS0051). The IL has assembly accessibility on the method and private-nested on the type, a
// combination C# cannot express; nothing outside this window calls it, so private is the
// narrowing that keeps every call site working.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   DefineVisitor, SortAlgo, RevertMapper, CustomizeAlgo, TestInitializer, _ObserverAnnotation,
//   _AlgoVisitor, m_VisitorVisitor  -- ControllerEditor outer class body
//   EditorUtils.setterProcessor, EditorUtils.EnableRules, CountPredicate, CreateResolver
//                                   -- EditorUtils (not yet ported); setterProcessor.DrawFitted is
//                                      already named in RemoteTextureView.cs
//
// Audit status: VERIFIED against export member-by-member (2026-08-04), except the two DEOBF-BUG
// sites and the licence removal above.

using System;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The tool's settings window: every preference in <see cref="EditorSettings"/>, plus the
        /// two template assets new states and transitions are cloned from.
        /// </summary>
        /// <remarks>
        /// The templates are real <see cref="AnimatorState"/> and
        /// <see cref="AnimatorStateTransition"/> instances rather than a parallel set of settings, so
        /// they are edited through a <see cref="SerializedObject"/> and Unity's own property drawers.
        /// That is why every property is resolved once into a field here: the window redraws per
        /// event and re-finding twenty properties each time is wasteful, and the objects only change
        /// when the user restores defaults.
        /// </remarks>
        internal class ControllerEditorWindow : EditorWindow
        {
            /// <summary>
            /// The node colour palette, as the popup shows it. The stored setting is the index,
            /// which is why the settings are FloatSettings rather than an enum setting.
            /// </summary>
            private enum NodeColor
            {
                Grey,
                Blue,
                Aqua,
                Green,
                Yellow,
                Orange,
                Red
            }

            /// <summary>The animator masks are built against by default.</summary>
            internal static Animator targetAnimator;

            /// <summary>Use <see cref="targetAnimator"/> even when the selection suggests another.</summary>
            internal static bool alwaysUseTargetAnimator;

            private static int tabIndex;

            private static readonly string[] tabLabels = { "Behaviours & Cosmetics", "Defaults" };

            private static int defaultsTabIndex;

            private static readonly string[] defaultsTabLabels = { "Transition", "State", "Other" };

            /// <summary>Empty option list for the dropdown arrows drawn purely for looks.</summary>
            private static readonly string[] noOptions = Array.Empty<string>();

            private static SerializedObject stateObject;

            private static SerializedProperty m_Name;

            private static SerializedProperty stateSpeed;

            private static SerializedProperty stateCycleOffset;

            private static SerializedProperty stateIKOnFeet;

            private static SerializedProperty stateWriteDefaults;

            private static SerializedProperty stateMirror;

            private static SerializedProperty stateSpeedParameterActive;

            private static SerializedProperty stateMirrorParameterActive;

            private static SerializedProperty stateCycleOffsetParameterActive;

            private static SerializedProperty stateTimeParameterActive;

            private static SerializedProperty stateMotion;

            private static SerializedProperty stateTag;

            private static SerializedProperty stateSpeedParameter;

            private static SerializedProperty stateMirrorParameter;

            private static SerializedProperty stateCycleOffsetParameter;

            private static SerializedProperty stateTimeParameter;

            private static SerializedObject transitionObject;

            private static SerializedProperty transitionSolo;

            private static SerializedProperty transitionMute;

            private static SerializedProperty transitionDuration;

            private static SerializedProperty transitionOffset;

            private static SerializedProperty transitionExitTime;

            private static SerializedProperty transitionHasExitTime;

            private static SerializedProperty transitionHasFixedDuration;

            private static SerializedProperty transitionInterruptionSource;

            private static SerializedProperty transitionOrderedInterruption;

            private static SerializedProperty transitionCanTransitionToSelf;

            private static Vector2 scrollPosition;

            private static bool animationWindowExpanded;

            private static bool animatorWindowExpanded;

            private static bool layersExpanded;

            private static bool parametersExpanded;

            private static bool typeIndicatorExpanded;

            private static bool nodesExpanded;

            private static bool transitionsExpanded;

            private static bool graphColorsExpanded;

            private static bool nodeColorsExpanded;

            private static bool defaultLayerOptionsExpanded;

            private static bool colorsExpanded;

            private static bool transitionColorsExpanded;

            /// <summary>
            /// Whether the editor is on the dark skin. Read by the graph-colour patches, which pick
            /// the light or dark grid colour setting accordingly.
            /// </summary>
            internal static bool IsProSkin => EditorGUIUtility.isProSkin;

            [MenuItem("DreadTools/Controller Editor/Settings", false, 4950)]
            internal static void Open()
            {
                GetWindow<ControllerEditorWindow>(false, "Controller Editor Settings", true);
            }

            private void OnGUI()
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                tabIndex = GUILayout.Toolbar(tabIndex, tabLabels, "toolbarbutton");
                switch (tabIndex)
                {
                    case 0:
                        DrawBehavioursAndCosmetics();
                        break;
                    case 1:
                        DrawDefaults();
                        break;
                }

                EditorUtils.Separator();
                DefineVisitor();
                EditorUtils.setterProcessor.DrawFitted(this);
                EditorGUILayout.EndScrollView();
            }

            private static void DrawBehavioursAndCosmetics()
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        animationWindowExpanded = EditorGUILayout.Foldout(animationWindowExpanded, "Animation Window");
                        GUILayout.FlexibleSpace();
                        using (new GUIColorScope(GUIColorScope.ColoringType.BG,
                                   EditorSettings.Instance.aw_active, Color.green, Color.grey))
                        {
                            EditorSettings.Instance.aw_active.Value = EditorUtils.ToggleButton(
                                EditorSettings.Instance.aw_active,
                                EditorSettings.Instance.aw_active ? "Enabled" : "Disabled");
                        }
                    }

                    if (animationWindowExpanded)
                    {
                        using (new EditorGUI.DisabledScope(!EditorSettings.Instance.aw_active))
                        {
                            using (new IndentedLayoutScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUI.BeginChangeCheck();
                                    EditorSettings.Instance.aw_enableOverride.Draw(new GUIContent("Overriding",
                                        "Allows you to explicitly set the controller for selecting clips, and explicitly set the root to change what the paths are relative to."));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        TestInitializer(null);
                                        _AlgoVisitor = null;
                                        m_VisitorVisitor = false;
                                    }

                                    EditorSettings.Instance.aw_enablePropertyEditing.Draw(new GUIContent("Edit Property",
                                        "Allows you to drag and drop objects to properties and to edit the properties of the curves with right-click context menu."));
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorSettings.Instance.aw_enableGameObjectDND.Draw(new GUIContent("Drag & Drop",
                                        "Allows you to drag and drop GameObjects to the animation window to add them as a new curve."));
                                    EditorSettings.Instance.aw_autoSwitchClip.Draw(new GUIContent("Auto-Switch Clip",
                                        "Automatically switch the clip in the animation window when selecting a state."));
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorSettings.Instance.aw_warnPropertyMerge.Draw(new GUIContent("Property Merge Log",
                                        "Warn in the console when merging properties through property modification."));
                                }
                            }
                        }
                    }
                }

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new FoldoutScope(ref animatorWindowExpanded, "Animator Window"))
                    {
                        if (!animatorWindowExpanded)
                        {
                            return;
                        }

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new FoldoutScope(ref layersExpanded, "Layers"))
                            {
                                if (layersExpanded)
                                {
                                    EditorSettings.Instance.categoryBaseName.Draw(
                                        "Uncategorized Name".CreateResolver(
                                            "Name of the category for layers without a category."));
                                    EditorSettings.Instance.categoryDelimiter.Draw(
                                        "Category Delimiter".CreateResolver(
                                            "The character used to separate categories in the layer view."));

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        EditorSettings.Instance.displayCategoryView.Draw(
                                            "Category View".CreateResolver(
                                                "Displays options to view layers in categories."));
                                        EditorSettings.Instance.displayLayerCompactView.Draw(
                                            "Compact View".CreateResolver(
                                                "Displays a button to view layers in a compact manner."));
                                    }

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        EditorSettings.Instance.displayLayerIndex.Draw(new GUIContent("Layer Index",
                                            "Shows a small number on the layer's GUI for the layer's index in the list of layers."));
                                        EditorSettings.Instance.autoFrameLayer.Draw(new GUIContent("Auto-Frame Layer",
                                            "Upon selecting a layer, automatically frame the statemachine. Behaviour is similar to pressing 'A' after clicking the graph."));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new FoldoutScope(ref parametersExpanded, "Parameters"))
                            {
                                if (parametersExpanded)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        typeIndicatorExpanded = EditorGUILayout.Foldout(typeIndicatorExpanded,
                                            "Type Indicator");
                                        GUILayout.FlexibleSpace();
                                        using (new GUIColorScope(GUIColorScope.ColoringType.BG,
                                                   EditorSettings.Instance.displayParameterType,
                                                   Color.green, Color.grey))
                                        {
                                            EditorSettings.Instance.displayParameterType.Value =
                                                EditorUtils.ToggleButton(EditorSettings.Instance.displayParameterType,
                                                    EditorSettings.Instance.displayParameterType
                                                        ? "Enabled"
                                                        : "Disabled");
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!EditorSettings.Instance.displayParameterType))
                                    {
                                        if (typeIndicatorExpanded)
                                        {
                                            using (new IndentedLayoutScope())
                                            {
                                                EditorSettings.Instance.capitalParameterIndicator.Draw(
                                                    new GUIContent("Capital Letters",
                                                        "Changes 'f' to 'F' and 'i' to 'I'"));
                                                EditorSettings.Instance.parameterLabelFontStyle
                                                    .DrawEnumPopup<FontStyle>(new GUIContent("Font style",
                                                            "The font style of the parameter indicators."),
                                                        false, null, Array.Empty<GUILayoutOption>());
                                                EditorSettings.Instance.parameterLabelColor.Draw(
                                                    new GUIContent("Font Color",
                                                        "The color of the parameter indicators. Supports Alpha."));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new FoldoutScope(ref transitionsExpanded, "Transitions"))
                            {
                                if (transitionsExpanded)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        using (new GUILayout.VerticalScope())
                                        {
                                            EditorSettings.Instance.autoReverseModes.Draw(
                                                new GUIContent("Auto Reverse Mode",
                                                    "Reverse Transitions should also reverse the condition modes"));
                                            EditorSettings.Instance.animateInboundEdges.Draw(
                                                "Animate In Transitions".CreateResolver(
                                                    "Incoming transitions to selected states get animated."));
                                        }

                                        using (new GUILayout.VerticalScope())
                                        {
                                            EditorSettings.Instance.reverseModifiesValues.Draw(
                                                new GUIContent("Reverse Adjusts Values",
                                                    "Reversing a condition will also modify its values appropriately. Hold CTRL to temporarily flip this setting while reversing"));
                                            EditorSettings.Instance.animateOutboundEdges.Draw(
                                                "Animate Out Transitions".CreateResolver(
                                                    "Outgoing transitions from selected states get animated."));
                                        }
                                    }

                                    EditorSettings.Instance.arrowLerpRatio.DrawSlider(
                                        "Arrow Location".CreateResolver("Where the arrow exists on transitions."),
                                        -1f, 1f);
                                }
                            }
                        }

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new FoldoutScope(ref nodesExpanded, "Nodes"))
                            {
                                if (nodesExpanded)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        EditorSettings.Instance.switchDoubleClick.Draw(
                                            new GUIContent("Alternate Double Click",
                                                "Switch Double click's behaviour on states. Ctrl Double Click will do the other behaviour"));
                                        EditorSettings.Instance.stateCosmetics
                                            .DrawEnumPopup<EditorSettings.StateCosmeticOptions>("State Extras",
                                                true, null, Array.Empty<GUILayoutOption>());
                                    }
                                }
                            }
                        }

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new FoldoutScope(ref colorsExpanded, "Colors"))
                            {
                                if (!colorsExpanded)
                                {
                                    return;
                                }

                                using (new GUILayout.VerticalScope(GUI.skin.box))
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        transitionColorsExpanded = EditorGUILayout.Foldout(
                                            transitionColorsExpanded, "Transition Colors");
                                        GUILayout.FlexibleSpace();

                                        bool active = EditorSettings.Instance.cosmeticTransitionsActive.Value;
                                        string label = active ? "Enabled" : "Disabled";
                                        using (new GUIColorScope(GUIColorScope.ColoringType.BG, active,
                                                   Color.green, Color.grey))
                                        {
                                            EditorSettings.Instance.cosmeticTransitionsActive.Value =
                                                EditorUtils.ToggleButton(active, label);
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(
                                               !EditorSettings.Instance.cosmeticTransitionsActive))
                                    {
                                        if (transitionColorsExpanded)
                                        {
                                            using (new IndentedLayoutScope())
                                            {
                                                EditorSettings.Instance.normalTransitionColor.Draw("Normal Transition");
                                                EditorSettings.Instance.entryTransitionColor.Draw("Entry Transition");
                                                EditorSettings.Instance.selectedTransitionColor.Draw("Selected Transition");
                                                EditorSettings.Instance.baseTransitionColor.Draw("Base Transition");
                                            }
                                        }
                                    }
                                }

                                using (new GUILayout.VerticalScope(GUI.skin.box))
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        graphColorsExpanded = EditorGUILayout.Foldout(graphColorsExpanded,
                                            "Graph Colors");
                                        GUILayout.FlexibleSpace();

                                        bool active = EditorSettings.Instance.cosmeticGraphActive.Value;
                                        string label = active ? "Enabled" : "Disabled";
                                        using (new GUIColorScope(GUIColorScope.ColoringType.BG, active,
                                                   Color.green, Color.grey))
                                        {
                                            using (new EditorSettings.SettingsChangeScope(SortAlgo))
                                            {
                                                EditorSettings.Instance.cosmeticGraphActive.Value =
                                                    EditorUtils.ToggleButton(active, label);
                                            }
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!EditorSettings.Instance.cosmeticGraphActive))
                                    {
                                        if (graphColorsExpanded)
                                        {
                                            using (new IndentedLayoutScope())
                                            {
                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    if (!EditorSettings.Instance.graphBackgroundIsTexture)
                                                    {
                                                        EditorSettings.Instance.gridBackgroundColor.Draw(
                                                            "Background", false);
                                                    }
                                                    else
                                                    {
                                                        EditorSettings.Instance.graphBackgroundTexture.Draw(
                                                            "Background", false, GUILayout.Height(17f),
                                                            GUILayout.ExpandWidth(true));
                                                    }

                                                    EditorSettings.Instance.graphBackgroundIsTexture.Value =
                                                        EditorUtils.ToggleButton(
                                                            EditorSettings.Instance.graphBackgroundIsTexture,
                                                            new GUIContent("T", "Use Texture"), GUI.skin.button,
                                                            GUILayout.Width(18f), GUILayout.Height(18f));

                                                    if (EditorUtils.IconButton(EditorUtils.contents.reset))
                                                    {
                                                        if (!EditorSettings.Instance.graphBackgroundIsTexture)
                                                        {
                                                            EditorSettings.Instance.gridBackgroundColor.Reset();
                                                        }
                                                        else
                                                        {
                                                            EditorSettings.Instance.graphBackgroundTexture.Reset();
                                                        }
                                                    }
                                                }

                                                if (EditorGUIUtility.isProSkin)
                                                {
                                                    EditorSettings.Instance.gridMinorDarkColor.Draw("Minor Line");
                                                    EditorSettings.Instance.gridMajorDarkColor.Draw("Major Line");
                                                }
                                                else
                                                {
                                                    EditorSettings.Instance.gridMinorLightColor.Draw("Minor Line");
                                                    EditorSettings.Instance.gridMajorLightColor.Draw("Major Line");
                                                }
                                            }
                                        }
                                    }
                                }

                                using (new GUILayout.VerticalScope(GUI.skin.box))
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        nodeColorsExpanded = EditorGUILayout.Foldout(nodeColorsExpanded,
                                            "Node Colors");
                                        GUILayout.FlexibleSpace();

                                        bool active = EditorSettings.Instance.cosmeticNodesActive.Value;
                                        string label = active ? "Enabled" : "Disabled";
                                        using (new GUIColorScope(GUIColorScope.ColoringType.BG, active,
                                                   Color.green, Color.grey))
                                        {
                                            EditorSettings.Instance.cosmeticNodesActive.Value =
                                                EditorUtils.ToggleButton(active, label);
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(!EditorSettings.Instance.cosmeticNodesActive))
                                    {
                                        if (nodeColorsExpanded)
                                        {
                                            using (new IndentedLayoutScope())
                                            {
                                                DrawNodeColorField(EditorSettings.Instance.normalStateNodeColor,
                                                    "State Node");
                                                DrawNodeColorField(EditorSettings.Instance.machineStateNodeColor,
                                                    "Machine Node");
                                                DrawNodeColorField(EditorSettings.Instance.defaultStateNodeColor,
                                                    "Default Node");
                                                DrawNodeColorField(EditorSettings.Instance.anyStateNodeColor,
                                                    "AnyState Node");
                                                DrawNodeColorField(EditorSettings.Instance.entryStateNodeColor,
                                                    "Entry Node");
                                                DrawNodeColorField(EditorSettings.Instance.exitStateNodeColor,
                                                    "Exit Node");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static void DrawDefaults()
            {
                defaultsTabIndex = GUILayout.Toolbar(defaultsTabIndex, defaultsTabLabels, "toolbarbutton");
                EditorUtils.Separator();

                switch (defaultsTabIndex)
                {
                    case 0:
                        DrawTransitionDefaults();
                        break;
                    case 1:
                        DrawStateDefaults();
                        break;
                    case 2:
                        DrawOtherDefaults();
                        break;
                }
            }

            private static void DrawTransitionDefaults()
            {
                transitionObject.Update();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (EditorUtils.Button(EditorUtils.contents.copy, GUI.skin.label,
                            GUILayout.Width(20f), GUILayout.Height(20f)))
                    {
                        if (_ObserverAnnotation == null)
                        {
                            _ObserverAnnotation = new AnimatorStateTransition();
                        }

                        CustomizeAlgo(EditorSettings.Instance.defaultTransition, _ObserverAnnotation);
                    }

                    using (new EditorGUI.DisabledScope(!_ObserverAnnotation))
                    {
                        if (EditorUtils.Button(EditorUtils.contents.paste, GUI.skin.label,
                                GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            Undo.RecordObject(EditorSettings.Instance.defaultTransition, "PasteSettings");
                            CustomizeAlgo(_ObserverAnnotation, EditorSettings.Instance.defaultTransition);
                        }
                    }

                    if (EditorUtils.Button(EditorUtils.contents.restoreDefaults, GUI.skin.label,
                            GUILayout.Width(20f), GUILayout.Height(20f))
                        && EditorUtility.DisplayDialog("Restoring Default Settings",
                            "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                    {
                        EditorSettings.Instance.defaultTransition = new AnimatorStateTransition();
                        RebuildTransitionObject();
                        EditorSettings.SaveSettings();
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(transitionHasExitTime);
                    using (new EditorGUI.DisabledScope(!transitionHasExitTime.boolValue))
                    {
                        EditorGUILayout.PropertyField(transitionExitTime);
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(transitionHasFixedDuration);
                    EditorGUILayout.PropertyField(transitionDuration);
                }

                EditorGUILayout.PropertyField(transitionOffset);
                EditorGUILayout.PropertyField(transitionInterruptionSource);

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(transitionOrderedInterruption);
                    EditorGUILayout.PropertyField(transitionMute);
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(transitionCanTransitionToSelf);
                    EditorGUILayout.PropertyField(transitionSolo);
                }

                bool modified = transitionObject.hasModifiedProperties;
                transitionObject.ApplyModifiedProperties();
                if (modified)
                {
                    EditorSettings.SaveSettings();
                }
            }

            private static void DrawStateDefaults()
            {
                stateObject.Update();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(EditorUtils.contents.animatorStates, GUILayout.Width(35f), GUILayout.Height(35f));

                    using (new GUILayout.VerticalScope())
                    {
                        EditorGUILayout.PropertyField(m_Name, new GUIContent(string.Empty));

                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUIUtility.labelWidth = 35f;
                            EditorGUILayout.PropertyField(stateTag);
                            EditorGUIUtility.labelWidth = 0f;

                            if (EditorUtils.Button(EditorUtils.contents.restoreDefaults, GUI.skin.label,
                                    GUILayout.Width(20f), GUILayout.Height(20f))
                                && EditorUtility.DisplayDialog("Restoring Default Settings",
                                    "Are you sure you want to restore the default settings?", "Restore", "Cancel"))
                            {
                                EditorSettings.Instance.defaultState = new AnimatorState { name = "New State" };
                                RebuildStateObject();
                                EditorSettings.SaveSettings();
                            }
                        }
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(stateMotion);
                EditorGUILayout.PropertyField(stateSpeed);

                // Each of the four "parameter" rows swaps a value field for a parameter-name field.
                // The disabled popup beside it is decoration: it draws the dropdown arrow Unity's own
                // animator inspector shows, with nothing behind it.
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.indentLevel++;
                    using (new EditorGUI.DisabledScope(!stateSpeedParameterActive.boolValue))
                    {
                        stateSpeedParameter.stringValue = EditorGUILayout.TextField("Multiplier",
                            stateSpeedParameter.stringValue, "textfielddropdowntext");
                    }

                    EditorGUI.indentLevel--;
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.Popup(-1, noOptions, "textfielddropdown", GUILayout.Width(12f));
                    }

                    stateSpeedParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter",
                        stateSpeedParameterActive.boolValue, GUILayout.Width(90f));
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (stateTimeParameterActive.boolValue)
                    {
                        stateTimeParameter.stringValue = EditorGUILayout.TextField("Normalized Time",
                            stateTimeParameter.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.Popup(-1, noOptions, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }
                    else
                    {
                        GUILayout.Label("Normalized Time");
                    }

                    stateTimeParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter",
                        stateTimeParameterActive.boolValue, GUILayout.Width(90f));
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (!stateMirrorParameterActive.boolValue)
                    {
                        EditorGUILayout.PropertyField(stateMirror);
                    }
                    else
                    {
                        stateMirrorParameter.stringValue = EditorGUILayout.TextField("Mirror",
                            stateMirrorParameter.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.Popup(-1, noOptions, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }

                    stateMirrorParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter",
                        stateMirrorParameterActive.boolValue, GUILayout.Width(90f));
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (!stateCycleOffsetParameterActive.boolValue)
                    {
                        stateCycleOffset.floatValue = EditorGUILayout.Slider("Cycle Offset",
                            stateCycleOffset.floatValue, 0f, 1f);
                    }
                    else
                    {
                        stateCycleOffsetParameter.stringValue = EditorGUILayout.TextField("Cycle Offset",
                            stateCycleOffsetParameter.stringValue, "textfielddropdowntext");
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.Popup(-1, noOptions, "textfielddropdown", GUILayout.Width(12f));
                        }
                    }

                    stateCycleOffsetParameterActive.boolValue = EditorGUILayout.ToggleLeft("Parameter",
                        stateCycleOffsetParameterActive.boolValue, GUILayout.Width(90f));
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(stateWriteDefaults, new GUIContent("Write Defaults"));
                    EditorGUILayout.PropertyField(stateIKOnFeet, new GUIContent("Foot IK"));
                }

                bool modified = stateObject.hasModifiedProperties;
                stateObject.ApplyModifiedProperties();
                if (modified)
                {
                    EditorSettings.SaveSettings();
                }
            }

            private static void DrawOtherDefaults()
            {
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    targetAnimator = targetAnimator.CountPredicate(new GUIContent("Targeted Animator",
                        "The Animator that should be targeted by default when building Masks"), true);
                    alwaysUseTargetAnimator = EditorUtils.ToggleButton(alwaysUseTargetAnimator,
                        new GUIContent("Always Use"), null, GUILayout.Width(85f));
                }

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    defaultLayerOptionsExpanded = EditorGUILayout.Foldout(defaultLayerOptionsExpanded,
                        "Default Layer Options");

                    if (defaultLayerOptionsExpanded)
                    {
                        using (new IndentedLayoutScope())
                        {
                            EditorSettings.Instance.defaultLayerWeight.Value = EditorGUILayout.Slider(
                                "Default Layer Weight", EditorSettings.Instance.defaultLayerWeight, 0f, 1f);
                            EditorSettings.Instance.defaultLayerMask.Draw("Default Layer Mask", false);

                            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                            {
                                EditorSettings.Instance.defaultEntryPosition.DrawVector2Field("Entry Position");
                            }

                            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                            {
                                EditorSettings.Instance.defaultAnyPosition.DrawVector2Field("AnyState Position");
                            }

                            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                            {
                                EditorSettings.Instance.defaultExitPosition.DrawVector2Field("Exit Position");
                            }

                            using (new EditorGUI.DisabledScope(!RevertMapper()))
                            {
                                if (EditorUtils.Button("Sample From Active StateMachine"))
                                {
                                    EditorSettings.Instance.defaultEntryPosition.Value =
                                        RevertMapper().entryPosition;
                                    EditorSettings.Instance.defaultAnyPosition.Value =
                                        RevertMapper().anyStatePosition;
                                    EditorSettings.Instance.defaultExitPosition.Value =
                                        RevertMapper().exitPosition;
                                }
                            }
                        }
                    }
                }

                string picked = EditorUtils.EnableRules(EditorSettings.Instance.saveFolder, "Generated Assets Path");
                if (!string.IsNullOrEmpty(picked))
                {
                    EditorSettings.Instance.saveFolder.Value = picked;
                }
            }

            private void OnEnable()
            {
                // DEOBF-BUG(guessed): export/ wraps these in a never-written-static guard and a
                // non-terminating `while`. See the file header.
                RebuildTransitionObject();
                RebuildStateObject();
            }

            /// <summary>
            /// Re-resolves every serialized property of the template transition, creating the
            /// template first if it is missing.
            /// </summary>
            internal static void RebuildTransitionObject()
            {
                // DEOBF-BUG(guessed): export/ puts the assignment and four of the FindProperty calls
                // in the `else` of this check. See the file header.
                if (EditorSettings.Instance.defaultTransition == null)
                {
                    EditorSettings.Instance.defaultTransition = new AnimatorStateTransition();
                }

                transitionObject = new SerializedObject(EditorSettings.Instance.defaultTransition);
                transitionSolo = transitionObject.FindProperty("m_Solo");
                transitionMute = transitionObject.FindProperty("m_Mute");
                transitionDuration = transitionObject.FindProperty("m_TransitionDuration");
                transitionOffset = transitionObject.FindProperty("m_TransitionOffset");
                transitionExitTime = transitionObject.FindProperty("m_ExitTime");
                transitionHasExitTime = transitionObject.FindProperty("m_HasExitTime");
                transitionHasFixedDuration = transitionObject.FindProperty("m_HasFixedDuration");
                transitionInterruptionSource = transitionObject.FindProperty("m_InterruptionSource");
                transitionOrderedInterruption = transitionObject.FindProperty("m_OrderedInterruption");
                transitionCanTransitionToSelf = transitionObject.FindProperty("m_CanTransitionToSelf");
            }

            /// <summary>
            /// Re-resolves every serialized property of the template state, creating the template
            /// first if it is missing.
            /// </summary>
            /// <remarks>
            /// The one-shot <c>requiresStateRename</c> flag exists because an older build shipped the
            /// template with a name the user had edited; it resets the name once and then clears
            /// itself.
            /// </remarks>
            internal static void RebuildStateObject()
            {
                if (EditorSettings.Instance.defaultState == null)
                {
                    EditorSettings.Instance.defaultState = new AnimatorState { name = "New State" };
                }

                stateObject = new SerializedObject(EditorSettings.Instance.defaultState);

                m_Name = stateObject.FindProperty("m_Name");
                if (m_Name != null && EditorSettings.Instance.requiresStateRename)
                {
                    m_Name.stringValue = "New State";
                    EditorSettings.Instance.requiresStateRename.Value = false;
                    stateObject.ApplyModifiedPropertiesWithoutUndo();
                }

                stateSpeed = stateObject.FindProperty("m_Speed");
                stateCycleOffset = stateObject.FindProperty("m_CycleOffset");
                stateIKOnFeet = stateObject.FindProperty("m_IKOnFeet");
                stateWriteDefaults = stateObject.FindProperty("m_WriteDefaultValues");
                stateMirror = stateObject.FindProperty("m_Mirror");
                stateSpeedParameterActive = stateObject.FindProperty("m_SpeedParameterActive");
                stateMirrorParameterActive = stateObject.FindProperty("m_MirrorParameterActive");
                stateCycleOffsetParameterActive = stateObject.FindProperty("m_CycleOffsetParameterActive");
                stateTimeParameterActive = stateObject.FindProperty("m_TimeParameterActive");
                stateMotion = stateObject.FindProperty("m_Motion");
                stateTag = stateObject.FindProperty("m_Tag");
                stateSpeedParameter = stateObject.FindProperty("m_SpeedParameter");
                stateMirrorParameter = stateObject.FindProperty("m_MirrorParameter");
                stateCycleOffsetParameter = stateObject.FindProperty("m_CycleOffsetParameter");
                stateTimeParameter = stateObject.FindProperty("m_TimeParameter");
            }

            /// <summary>
            /// Draws one node-colour setting as a <see cref="NodeColor"/> popup. The setting stores
            /// the palette index as a float, so the value is boxed through the enum.
            /// </summary>
            private static void DrawNodeColorField(EditorSettings.FloatSetting setting, string label)
            {
                using (new GUILayout.HorizontalScope())
                {
                    setting.Value = (float)(NodeColor)(object)EditorGUILayout.EnumPopup(label,
                        (NodeColor)setting.Value);

                    if (EditorUtils.Button(EditorUtils.contents.reset, EditorUtils.styles.tightLabel,
                            GUILayout.Width(18f), GUILayout.Height(18f)))
                    {
                        setting.Reset();
                    }
                }
            }
        }
    }
}
