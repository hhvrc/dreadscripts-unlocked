// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   QuickToggleWindow      -> QuickToggleWindow, lines 4157-4833 (vendor name; unobfuscated)
//     m_WrapperInitializer -> root,                  line 4452
//     annotationInitializer -> rows,                 line 4454
//     _VisitorInitializer  -> states,                line 4456
//     _AlgoInitializer     -> rowList,               line 4458
//     _MapperInitializer   -> mergeModeColors,       line 4460
//     _InitializerInitializer -> mergeMode,          line 4467
//     definitionInitializer -> existingClipCount,    line 4469
//     regInitializer       -> mergePerState,         line 4471
//     testsInitializer     -> hasExistingClips,      line 4473
//     propertyInitializer  -> existingClipsExpanded, line 4475
//     _ProcessorInitializer -> labels,               line 4477
//     ...title             -> Title,                 line 4485 (mangled explicit interface member)
//     RegisterTests / LogoutTests -> Advanced,       lines 4488/4494 [SpecialName pair]
//     InterruptTests / ManageTests -> MergeByDefault, lines 4500/4506 [SpecialName pair]
//     AssetTests           -> Create,                line 4511
//     ...OnCustomGUI       -> OnCustomGUI,           line 4678 (mangled explicit interface member)
//     OnCustomConfirm      -> OnCustomConfirm,       line 4768
//     UpdateTests          -> GetWindowSize,         line 4819
//     ChangeTests          -> RefreshMergeMode,      line 4824
//     SortTests            -> ShowAtAutoSized,       line 4829
//     <>c and <>c__DisplayClass18_0/_1/_2 -> dissolved back into lambdas at their use sites,
//                             lines 4159-4450. Export duplicates every one of those bodies twice:
//                             once as a display-class method and once inline inside Create. Only
//                             the inline copy is real code; the display class is the closure the
//                             compiler hoisted it into.
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// `title` and `OnCustomGUI` decompile as mangled explicit implementations of an interface that
// exists nowhere else in the assembly; UtilityWindowBase.cs already reconstructs them as plain
// abstract members, so they are overrides here.
//
// The row loop in OnCustomConfirm is a plain de-flattening of export's `while (true) { ... }`, not
// a deviation: the `break` that skips the trailing block is a `return`, the `continue` is the loop
// step, and the trailing block runs only when every state was processed.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   RateAnnotation, LogoutMapper -- ControllerEditor outer class body (warn-and-abort helper; the
//                                   controller currently being edited)
//   EditorUtils.FlushQueue, EditorUtils.ResetQueue, EditorUtils.InvokePredicate,
//   AnimationClip.FindPredicate  -- EditorUtils (not yet ported)
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The Quick Toggle dialog: pick some scene objects and it writes a two-key on/off curve for
        /// each of them into the animation clips of the selected states.
        /// </summary>
        /// <remarks>
        /// Two modes. Simple mode animates whether an object or component is enabled and shows one
        /// On/Off button per row. Advanced mode lets a row address any animatable property of the
        /// chosen component and type a value. Both share <see cref="ComponentQueue"/>, which holds
        /// the row's object, its component index and its property.
        ///
        /// A state that already has a clip can either have the new curves merged into it or be given
        /// a fresh clip. That choice is per state — <see cref="mergePerState"/> — with
        /// <see cref="mergeMode"/> summarising it for the header button as merge, replace or mixed.
        /// </remarks>
        internal class QuickToggleWindow : UtilityWindowBase<QuickToggleWindow>
        {
            /// <summary>The transform animation paths are made relative to.</summary>
            private Transform root;

            private List<ComponentQueue> rows;

            private List<AnimatorState> states;

            private ReorderableListHelper<ComponentQueue> rowList;

            /// <summary>Colours for <see cref="mergeMode"/>: merge, replace, mixed.</summary>
            private static readonly Color[] mergeModeColors =
            {
                Color.green,
                Color.cyan,
                Color.yellow
            };

            /// <summary>0 all merge, 1 all replace, 2 mixed. Derived from <see cref="mergePerState"/>.</summary>
            private int mergeMode;

            /// <summary>How many of <see cref="states"/> already hold an AnimationClip.</summary>
            private int existingClipCount;

            /// <summary>Per state in <see cref="states"/>: merge into its clip rather than replace it.</summary>
            private bool[] mergePerState;

            private bool hasExistingClips;

            private bool existingClipsExpanded;

            private static readonly GUIContent[] labels =
            {
                new GUIContent("Root", "Relative path root of the animation"),
                new GUIContent("Target", "Target GameObject or GameObject containing target Component"),
                new GUIContent("Component Index",
                    "Which component to toggle. -1 is GameObject. 0 is Transform (Not toggleable)"),
                new GUIContent("Enabled", "What the toggled state is when animated")
            };

            internal override string Title => "CEditor QuickToggle";

            /// <summary>Advanced mode: address any property, not just the enabled flag.</summary>
            private static bool Advanced
            {
                get => EditorSettings.Instance.advancedQuickToggle;
                set => EditorSettings.Instance.advancedQuickToggle.Value = value;
            }

            /// <summary>Default for a state that already has a clip: merge rather than replace.</summary>
            private static bool MergeByDefault
            {
                get => EditorSettings.Instance.mergeQuickToggle;
                set => EditorSettings.Instance.mergeQuickToggle.Value = value;
            }

            internal static QuickToggleWindow Create(List<AnimatorState> states, Transform root,
                List<GameObject> targets)
            {
                QuickToggleWindow window = UtilityWindowBase<QuickToggleWindow>.Create();
                window.states = states;
                window.mergePerState = new bool[states.Count];

                // A state whose clip is the one the template state carries is not really "existing"
                // content, so it defaults to replace rather than merge.
                AnimatorState templateState = EditorSettings.Instance.defaultState;
                Motion templateMotion = templateState != null ? templateState.motion : null;

                if (MergeByDefault)
                {
                    for (int i = 0; i < states.Count; i++)
                    {
                        AnimatorState state = window.states[i];
                        window.mergePerState[i] = state == null || state.motion != templateMotion;
                    }
                }

                window.existingClipCount = states.Count(s => s.motion as AnimationClip);
                window.hasExistingClips = window.existingClipCount > 0;
                window.root = root;
                window.rows = new List<ComponentQueue>(targets.Select(o => new ComponentQueue(o)));

                window.rowList = new ReorderableListHelper<ComponentQueue>(
                    delegate
                    {
                        window.rowList.DrawTitle("Target GameObjects",
                            "The GameObjects that will be animated by the animation clip");
                        GUILayout.FlexibleSpace();

                        if (window.hasExistingClips && EditorUtils.Button(
                                MergeByDefault
                                    ? EditorUtils.contents.defaultMergeClip
                                    : EditorUtils.contents.defaultReplaceClip,
                                EditorUtils.styles.iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            MergeByDefault = !MergeByDefault;
                        }

                        if (EditorUtils.Button(
                                Advanced ? EditorUtils.contents.advancedMode : EditorUtils.contents.simpleMode,
                                EditorUtils.styles.iconButton, GUILayout.Width(20f), GUILayout.Height(20f)))
                        {
                            Advanced = !Advanced;
                        }

                        window.rowList.DrawHeaderButtons(false, false);
                    },
                    window.rows,
                    delegate { window.rows.Add(new ComponentQueue()); },
                    delegate(Rect rect, int index, bool active, bool focused)
                    {
                        window.DrawRow(rect, index);
                    });

                window.rowList.drawWhenEmpty = true;
                window.RefreshMergeMode();

                // If every selected state reads as an "off" state, the rows default to off; likewise
                // for "on". A mixed selection is left alone.
                if (states.All(s => s.name.IndexOf("off", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    foreach (ComponentQueue row in window.rows)
                    {
                        row.value = 0f;
                    }
                }
                else if (states.All(s => s.name.IndexOf("on", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    foreach (ComponentQueue row in window.rows)
                    {
                        row.value = 1f;
                    }
                }

                return window;
            }

            /// <summary>Draws one row of <see cref="rowList"/>.</summary>
            private void DrawRow(Rect rect, int index)
            {
                ComponentQueue row = rows[index];

                Rect objectFieldRect = rect.SliceLeft(Advanced ? 45 : 80, false, -1f, false, false);
                Rect typeButtonRect = new Rect(objectFieldRect)
                {
                    width = 20f,
                    x = objectFieldRect.x + objectFieldRect.width - 20f
                };

                if (!Advanced)
                {
                    Rect toggleRect = rect.SliceLeft(20f, false, 80f);
                    string toggleLabel = row.IsOn ? "On" : "Off";
                    using (new GUIColorScope(GUIColorScope.ColoringType.BG, row.IsOn, Color.green, Color.red))
                    {
                        if (GUI.Button(toggleRect, toggleLabel))
                        {
                            row.value = row.IsOn ? 0 : 1;
                        }
                    }
                }
                else
                {
                    Rect propertyRect = rect.SliceLeft(40f, false, 45f, false, false);
                    Rect valueRect = rect.SliceLeft(15f, false, 85f);
                    propertyRect.height = 20f;

                    bool hasProperties = row.propertyNames.Length != 0;
                    using (new EditorGUI.DisabledScope(!hasProperties))
                    {
                        if (EditorGUI.DropdownButton(propertyRect, new GUIContent(row.PropertyName),
                                FocusType.Keyboard, EditorStyles.toolbarDropDown))
                        {
                            SearchablePickerPopup<string> picker = new SearchablePickerPopup<string>(
                                "Property", row.propertyNames,
                                entry => GUILayout.Label(entry.value),
                                delegate(int i, string _)
                                {
                                    row.propertyIndex = i;
                                    Repaint();
                                });
                            picker.EnableSearch((p, s) => p.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
                            picker.Show(propertyRect);
                        }
                    }

                    row.value = EditorGUI.FloatField(valueRect, row.value);
                    EditorUtils.FlushQueue(propertyRect, "Property", 180f, 15f, stripresult3: false);
                    EditorUtils.FlushQueue(valueRect, "Value", 50f, 0f, stripresult3: false);
                    if (!hasProperties)
                    {
                        EditorUtils.FlushQueue(propertyRect, "No Valid Properties", 145f);
                    }
                }

                using (new EditorGUI.DisabledScope(!row.GameObject))
                {
                    if (GUI.Button(typeButtonRect, GUIContent.none, GUIStyle.none))
                    {
                        if (Event.current.button == 0)
                        {
                            SearchablePickerPopup<Type> picker = new SearchablePickerPopup<Type>(
                                "Target Type",
                                new[] { typeof(GameObject) }
                                    .Concat(row.components.Select(c => c.GetType()))
                                    .Distinct()
                                    .ToList(),
                                delegate(SearchablePickerPopup<Type>.PickerEntry entry)
                                {
                                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                                    {
                                        GUILayout.Label((GUIContent)entry.firstExtra,
                                            GUILayout.Height(EditorGUIUtility.singleLineHeight));
                                    }
                                },
                                delegate(int i, Type _)
                                {
                                    // Entry 0 is the GameObject itself, which ComponentQueue calls -1.
                                    row.ComponentIndex = i - 1;
                                    Repaint();
                                });

                            picker.SetExtraData(type => new object[]
                            {
                                new GUIContent(
                                    text: type.Name,
                                    image: EditorGUIUtility.ObjectContent(null, type).image
                                           ?? EditorGUIUtility.ObjectContent(null, typeof(MonoBehaviour)).image,
                                    tooltip: type.AssemblyQualifiedName)
                            });

                            if (!EditorSettings.Instance.advancedQuickToggle)
                            {
                                foreach (SearchablePickerPopup<Type>.PickerEntry entry in picker.entries)
                                {
                                    if (!ComponentQueue.toggleableTypes.Any(t => entry.value.Is(t)))
                                    {
                                        entry.isVisible = false;
                                    }
                                }
                            }

                            picker.Show(typeButtonRect);
                        }
                        else
                        {
                            row.Next(!EditorSettings.Instance.advancedQuickToggle);
                        }

                        Event.current.Use();
                    }
                }

                Object target = row.target;
                EditorGUI.BeginChangeCheck();
                target = EditorGUI.ObjectField(objectFieldRect, target, typeof(GameObject), true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (target)
                    {
                        if (target is GameObject gameObject)
                        {
                            row.GameObject = gameObject;
                        }
                        else if (target is Component component)
                        {
                            row.GameObject = component.gameObject;
                        }
                    }
                    else
                    {
                        row.GameObject = null;
                    }
                }

                if (row.GameObject)
                {
                    EditorGUI.DropdownButton(typeButtonRect, GUIContent.none, FocusType.Passive,
                        EditorUtils.styles.dropDownButton);
                }

                EditorUtils.FlushQueue(objectFieldRect, "Target", 200f, 20f, stripresult3: false);

                // Dropping several objects onto one row inserts the rest as further rows.
                EditorUtils.HandleMultiDragAndDrop<GameObject>(objectFieldRect, dropped =>
                    rows.InsertRange(index, dropped.Where(o => o != row.GameObject)
                        .Select(o => new ComponentQueue(o))));

                EditorUtils.ResetQueue(objectFieldRect, delegate(float y)
                {
                    if (y <= 0f)
                    {
                        row.Next(!EditorSettings.Instance.advancedQuickToggle);
                    }
                    else
                    {
                        row.Previous(!EditorSettings.Instance.advancedQuickToggle);
                    }

                    Repaint();
                });
            }

            internal override void OnCustomGUI()
            {
                if (rowList == null)
                {
                    Close();
                    return;
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    root = (Transform)EditorGUILayout.ObjectField(labels[0], root, typeof(Transform), true);
                }

                rowList.Draw();

                if (!hasExistingClips)
                {
                    return;
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        existingClipsExpanded = EditorGUILayout.Foldout(existingClipsExpanded,
                            new GUIContent($"Existing Clips ({existingClipCount})"));
                        GUILayout.FlexibleSpace();

                        GUILayout.Label(new GUIContent(EditorUtils.contents.help.texture,
                                "Merge: Adds the properties to the existing clips on states. Creates a new clip if no clip exists.\n\n"
                                + "Replace: Replaces the existing clips on states with new clips and adds the properties to them."),
                            GUILayout.Width(14f), GUILayout.Height(18f));

                        string modeLabel = mergeMode == 0 ? "Merge" : mergeMode == 1 ? "Replace" : "Mixed";
                        using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergeMode,
                                   mergeModeColors[0], mergeModeColors[1], mergeModeColors[2]))
                        {
                            if (EditorUtils.Button(modeLabel))
                            {
                                switch (mergeMode)
                                {
                                    case 0:
                                        mergeMode = 1;
                                        for (int i = 0; i < mergePerState.Length; i++)
                                        {
                                            mergePerState[i] = false;
                                        }

                                        break;

                                    case 1:
                                    case 2:
                                        mergeMode = 0;
                                        for (int i = 0; i < mergePerState.Length; i++)
                                        {
                                            mergePerState[i] = true;
                                        }

                                        break;
                                }
                            }
                        }
                    }

                    if (!existingClipsExpanded)
                    {
                        return;
                    }

                    using (new IndentedLayoutScope())
                    {
                        for (int i = 0; i < states.Count; i++)
                        {
                            AnimatorState state = states[i];
                            if (!state)
                            {
                                continue;
                            }

                            AnimationClip clip = state.motion as AnimationClip;
                            if (!clip)
                            {
                                continue;
                            }

                            using (new GUILayout.HorizontalScope(GUI.skin.box))
                            {
                                GUILayout.Label(clip.name);
                                GUILayout.FlexibleSpace();

                                string rowLabel = mergePerState[i] ? "Merge" : "Replace";
                                using (new GUIColorScope(GUIColorScope.ColoringType.BG, mergePerState[i],
                                           mergeModeColors[0], mergeModeColors[1]))
                                {
                                    if (EditorUtils.Button(rowLabel))
                                    {
                                        mergePerState[i] = !mergePerState[i];
                                        RefreshMergeMode();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            internal override void OnCustomConfirm()
            {
                if (RateAnnotation(!root, "No Root Set!"))
                {
                    return;
                }

                List<AnimationClip> clips = new List<AnimationClip>();

                for (int i = 0; i < states.Count; i++)
                {
                    AnimatorState state = states[i];
                    Motion motion = state.motion;

                    // A blend tree cannot be merged into, so replacing is the only option; if the
                    // user asked to merge, abort the whole operation with a warning.
                    if (!mergePerState[i]
                        && RateAnnotation(motion is BlendTree,
                            "State " + state.name + " has a Blendtree motion. Can't automatically merge."))
                    {
                        return;
                    }

                    AnimationClip clip = motion as AnimationClip;
                    if (!clip || !mergePerState[i])
                    {
                        Undo.RecordObject(state, "Set Quick Toggle Curve");
                        string folder =
                            $"{EditorSettings.Instance.saveFolder}/Animation Clips/{LogoutMapper().name}";
                        clip = new AnimationClip();
                        string path = EditorUtils.PrepareAssetPath(folder, state.name + ".anim", true);
                        AssetDatabase.CreateAsset(clip, path);
                        state.motion = clip;
                        EditorUtility.SetDirty(state);
                    }

                    clips.Add(clip);
                }

                Object[] targets;
                Object[] distinctClips = targets = clips.Distinct().ToArray();
                Undo.RecordObjects(targets, "Set Quick Toggle Curve");

                foreach (AnimationClip clip in (AnimationClip[])distinctClips)
                {
                    foreach (ComponentQueue row in rows)
                    {
                        if (row.IsValid)
                        {
                            clip.SetCurve(
                                AnimationUtility.CalculateTransformPath(row.GameObject.transform, root),
                                row.targetType,
                                row.PropertyName,
                                EditorUtils.InvokePredicate(AnimationUtility.TangentMode.Linear,
                                    (0f, row.value),
                                    (clip.FindPredicate(), row.value)));
                        }
                    }
                }
            }

            internal Vector2 GetWindowSize()
            {
                return new Vector2(370f,
                    48 + 22 * Mathf.Max(1, rows.Count) + 28
                    + (!string.IsNullOrEmpty(helpMessage) ? 38 : 0)
                    + (hasExistingClips ? 32 : 0));
            }

            /// <summary>Recomputes <see cref="mergeMode"/> from <see cref="mergePerState"/>.</summary>
            internal void RefreshMergeMode()
            {
                mergeMode = mergePerState.All(b => b) ? 0 : mergePerState.All(b => !b) ? 1 : 2;
            }

            internal void ShowAtAutoSized(Vector2 screenPosition)
            {
                ShowAt(screenPosition, GetWindowSize());
            }
        }
    }
}
