// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
//   ConnectAnnotation -> RefreshParameterNames, line 9739
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// This file exists to own one member, for the same reason ControllerEditor.CollapsibleSection.cs
// does: `ConnectAnnotation` is the only writer of the three parameter-name caches, and it is called
// from a region -- the selection-sync routine -- that belongs to another file. Porting it into
// ControllerEditor.SelectionSync.cs would have meant that file claiming a decompiled member outside
// its own region, so it is claimed here instead and exactly one file claims decompiled line 9739.
//
// ControllerEditor.SelectionSync.cs deferred this call for as long as the ActiveController accessor
// was unported. That accessor landed as the ActiveController property in
// ControllerEditor.ControllerContext.cs, and nothing else in the body is blocked: the three fields
// it writes are declared in ControllerEditor.State.cs (parameterNames 7994, boolParameterNames
// 7996, floatParameterNames 7998), and it calls nothing at all.
//
// The obfuscated name is noise -- the method connects nothing. `RefreshParameterNames` is what the
// body does: it re-derives the dropdown contents that the condition rows and the controller
// section's batch-action fields pick parameter names from.
//
// WHAT THE THREE CACHES ARE FOR, since only one of them is currently read anywhere in the package.
// `parameterNames` is every parameter of the active controller, in declaration order, and is what
// ControllerEditor.ConditionRow.cs and ControllerEditor.ControllerSection.cs offer in their
// parameter pickers. `boolParameterNames` and `floatParameterNames` are the Bool-only and
// Float-only subsets, built for the pickers of the still-unported VRChat parameter-driver rows;
// they are written here regardless, because the shipped method writes them and dropping a written
// field on the grounds that nothing currently reads it would be a behaviour change the moment the
// reader lands.
//
// THE EARLY RETURN IS LOAD-BEARING AND IS NOT A NULL GUARD ONLY. With no controller loaded the
// method returns leaving all three caches at whatever the previous controller left in them, rather
// than clearing them. That is shipped, and it is why ControllerEditor.ControllerSection.cs reads
// `parameterNames ?? new string[0]` -- the null it guards against is the initial one, before any
// controller has ever been active, not one this method produces.
//
// Note the test is `if (!ActiveController())`, i.e. UnityEngine.Object's implicit bool conversion,
// not `== null`: a controller that has been destroyed but whose managed wrapper is still alive is
// treated as absent. Preserved as written.
//
// ================================ DELIBERATE DEVIATION =========================================
//
// The decompiled body reads the ActiveController accessor three times -- once for the guard, once
// for `parameters`, and the shipped source presumably once. It is read twice here, in the guard and
// in the assignment of the local, exactly as decompiled; the local `parameters` is the decompiled
// local and is not an addition.
//
// The per-parameter type test is written as a two-armed `if / else if` where the decompiled body
// nests it inside out -- `if (type != Bool) { if (type == Float) floats.Add(...); } else
// bools.Add(...)`. That is ILSpy's rendering of a branch-if-not-equal, and the two forms are the
// same decision: a parameter type cannot be both Bool and Float, and neither list is touched for
// the Int and Trigger cases under either spelling.
//
// ==================================== 2019 vs 2022 =============================================
//
// ControllerEditor ships a single build, so there is no second decompilation to diff this against.
//
// Audit status: VERIFIED -- the body below was diffed statement for statement against decompiled
// lines 9739-9765: the guard, the `parameters` local, the exact-length allocation of
// `parameterNames`, the two List<string> accumulators, the loop bound, the unconditional write of
// `parameterNames[i]` before the type test, the type test itself and the order of the two trailing
// ToArray assignments (floats first, then bools). The three fields it writes were followed to their
// declarations in ControllerEditor.State.cs and their types confirmed as string[]. The range
// contains no `goto`, no residual `switch` dispatch, no `while (true)` and no unresolved
// `smethod_N`, so no deobfuscator fault applies to it, and it carries no licence gate.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Parameter name caches

        /// <summary>
        /// Re-derives <see cref="parameterNames"/> and its Bool and Float subsets from the active
        /// controller's parameter list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These are the contents of every parameter dropdown the tool draws. They are caches rather
        /// than live reads because a condition row redraws its picker every frame and an
        /// <c>AnimatorController.parameters</c> read allocates a fresh array each time; refreshing
        /// them once per selection change is what keeps the condition editor from allocating per
        /// row per frame.
        /// </para>
        /// <para>
        /// With no controller loaded the caches are left holding the previous controller's names
        /// rather than being cleared. That is shipped behaviour -- see the file header.
        /// </para>
        /// </remarks>
        private static void RefreshParameterNames()
        {
            if (!ActiveController)
            {
                return;
            }

            AnimatorControllerParameter[] parameters = ActiveController.parameters;
            parameterNames = new string[parameters.Length];

            List<string> floats = new List<string>();
            List<string> bools = new List<string>();

            for (int i = 0; i < parameters.Length; i++)
            {
                // Written for every parameter, whatever its type: the full list is the one the
                // condition rows pick from, and it is not filtered.
                parameterNames[i] = parameters[i].name;

                if (parameters[i].type == AnimatorControllerParameterType.Bool)
                {
                    bools.Add(parameters[i].name);
                }
                else if (parameters[i].type == AnimatorControllerParameterType.Float)
                {
                    floats.Add(parameters[i].name);
                }
            }

            floatParameterNames = floats.ToArray();
            boolParameterNames = bools.ToArray();
        }

        #endregion
    }
}
