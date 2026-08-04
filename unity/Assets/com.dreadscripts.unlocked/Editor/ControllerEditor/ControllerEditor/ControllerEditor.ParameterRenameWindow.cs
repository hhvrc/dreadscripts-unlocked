// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   ParameterRenameWindow  -> ParameterRenameWindow, lines 3998-4155 (vendor name; unobfuscated)
//     m_BroadcasterMapper  -> writeDefaultsOptions, line 4011
//     m_ProxyMapper        -> sourceController,     line 4013
//     _StructMapper        -> targetController,     line 4015
//     serviceMapper        -> uniqueParameters,     line 4017
//     stateMapper          -> writeDefaultsMode,    line 4019
//     globalMapper         -> renames,              line 4021
//     ...title             -> Title,                line 4023 (mangled explicit interface member)
//     ResolveTests         -> Create,               line 4025
//     ...OnCustomGUI       -> OnCustomGUI,          line 4048 (mangled explicit interface member)
//     OnCustomConfirm      -> OnCustomConfirm,      line 4092
//     ListTests            -> MakeUnique,           line 4111
//     VerifyTests          -> ShowAtAutoSized,      line 4146
//     FillTests            -> GetWindowSize,        line 4151
//     <>c__DisplayClass11_1.PrepareTests -> dissolved back into a lambda, line 4005
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// `title` and `OnCustomGUI` decompile as mangled explicit implementations of an interface that
// exists nowhere else in the assembly; UtilityWindowBase.cs already reconstructs them as plain
// abstract members, so they are overrides here.
//
// The two search loops in MakeUnique are plain de-flattenings of export's `while (true)` scans,
// not deviations: same predicate, same result.
//
// These belong to code that is not ported yet and keep their decompiled names:
//   MapAnnotation, CloneAnnotation, CompareAlgo -- ControllerEditor outer class body (make a name
//                                                  unique under a predicate; shorten a label with an
//                                                  ellipsis; apply the renames and return the layers)
//   EditorUtils.m_AdapterProcessor, AssetPredicate, PrintPredicate -- EditorUtils (not yet ported)
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        /// <summary>
        /// The dialog shown when copying layers between controllers: one row per parameter the
        /// layers use, with the name it will take in the destination.
        /// </summary>
        /// <remarks>
        /// Copying a layer drags its parameters along, and a name that already exists in the
        /// destination would silently merge two unrelated parameters. With
        /// <see cref="uniqueParameters"/> on, every proposed name is pushed until it collides with
        /// neither another row nor anything already in the destination controller; turning it off
        /// lets the user merge on purpose. VRChat's built-in parameters are excluded from the list
        /// outright — they are supposed to collide.
        /// </remarks>
        internal class ParameterRenameWindow : UtilityWindowBase<ParameterRenameWindow>
        {
            private static readonly string[] writeDefaultsOptions = { "No Change", "Force On", "Force Off" };

            internal AnimatorController sourceController;

            internal AnimatorController targetController;

            /// <summary>Force every copied parameter to a name nothing else in the destination uses.</summary>
            internal bool uniqueParameters = true;

            /// <summary>Index into <see cref="writeDefaultsOptions"/>.</summary>
            private int writeDefaultsMode;

            internal (AnimatorControllerParameter parameter, string newName)[] renames;

            internal override string Title => "Parameter Rename";

            internal static ParameterRenameWindow Create(AnimatorController source, AnimatorController target,
                bool uniqueParameters)
            {
                ParameterRenameWindow window = UtilityWindowBase<ParameterRenameWindow>.Create();
                window.sourceController = source;
                window.targetController = target;
                window.uniqueParameters = uniqueParameters;

                AnimatorControllerParameter[] parameters = source.parameters
                    .Where(p => !EditorUtils.m_AdapterProcessor.Contains(p.name))
                    .ToArray();

                int count = parameters.Length;
                window.renames = new (AnimatorControllerParameter, string)[count];
                for (int i = 0; i < count; i++)
                {
                    window.renames[i] = (parameters[i], parameters[i].name);
                }

                if (uniqueParameters)
                {
                    for (int i = 0; i < count; i++)
                    {
                        window.renames[i].newName = window.MakeUnique(window.renames[i].newName, i);
                    }
                }

                return window;
            }

            internal override void OnCustomGUI()
            {
                if (renames == null)
                {
                    Close();
                    return;
                }

                canConfirm = true;

                EditorGUI.BeginChangeCheck();
                using (new GUIColorScope(GUIColorScope.ColoringType.BG, uniqueParameters, Color.green, Color.grey))
                {
                    uniqueParameters = EditorUtils.ToggleButton(uniqueParameters, "Unique Parameters", GUI.skin.button);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < renames.Length; i++)
                    {
                        renames[i].newName = MakeUnique(renames[i].newName, i);
                    }
                }

                for (int i = 0; i < renames.Length; i++)
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label(
                            new GUIContent(CloneAnnotation(renames[i].parameter.name, 9, 5), renames[i].parameter.name),
                            GUILayout.Width(125f));

                        EditorGUI.BeginChangeCheck();
                        renames[i].newName = EditorGUILayout.TextField(renames[i].newName);
                        if (EditorGUI.EndChangeCheck() && uniqueParameters)
                        {
                            renames[i].newName = MakeUnique(renames[i].newName, i);
                        }

                        if (string.IsNullOrEmpty(renames[i].newName))
                        {
                            canConfirm = false;
                            GUILayout.Label(
                                new GUIContent(EditorUtils.contents.warning.texture, "Parameter must not be empty"),
                                EditorUtils.styles.centeredIcon, GUILayout.ExpandWidth(false));
                        }
                    }
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    writeDefaultsMode = EditorGUILayout.Popup("Write Defaults", writeDefaultsMode,
                        writeDefaultsOptions);
                }
            }

            internal override void OnCustomConfirm()
            {
                AnimatorControllerLayer[] copiedLayers = CompareAlgo(sourceController, targetController, renames);

                if (writeDefaultsMode == 0)
                {
                    return;
                }

                bool writeDefaults = writeDefaultsMode == 1;
                foreach (AnimatorControllerLayer layer in copiedLayers)
                {
                    layer.stateMachine.AssetPredicate(delegate(AnimatorState s)
                    {
                        s.writeDefaultValues = writeDefaults;
                        s.PrintPredicate();
                    });
                }
            }

            /// <summary>
            /// Pushes <paramref name="name"/> until it collides with neither another row nor a
            /// parameter already in the destination controller. Both passes are applied repeatedly,
            /// because resolving one collision can create the other.
            /// </summary>
            private string MakeUnique(string name, int rowIndex)
            {
                string current = name;
                string previous;

                do
                {
                    previous = current;

                    current = MapAnnotation(current, candidate =>
                    {
                        for (int i = 0; i < renames.Length; i++)
                        {
                            if (i != rowIndex && renames[i].newName == candidate)
                            {
                                return false;
                            }
                        }

                        return true;
                    });

                    current = MapAnnotation(current,
                        candidate => targetController.parameters.All(p => p.name != candidate));
                }
                while (previous != current);

                return current;
            }

            internal void ShowAtAutoSized(Vector2 screenPosition)
            {
                ShowAt(screenPosition, GetWindowSize());
            }

            internal Vector2 GetWindowSize()
            {
                return new Vector2(350f, 60f + renames.Length * (EditorGUIUtility.singleLineHeight + 7f));
            }
        }
    }
}
