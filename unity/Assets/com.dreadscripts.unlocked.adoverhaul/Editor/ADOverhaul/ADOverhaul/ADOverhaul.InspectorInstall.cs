// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the [DidReloadScripts] hook that reinstalls the replacement inspectors, and the
// captured-variable display class holding its body. Line numbers are relative to the current
// snapshot; the decompiled names are the durable reference.
//
//   WriteConfiguration              -> InstallInspectorOverrides,   line 6756
//
// NOTES
// The body lives in a compiler-generated display class, _003C_003Ec__DisplayClass66_0, at lines
// 5726-5766 of the current snapshot: `publisherContext` holds the one captured FieldInfo and
// `CancelProcess()` is the async body. It is a decompiler artifact -- the original was an async
// local function capturing a single local -- so it is folded back into the one method below, where
// the FieldInfo is an ordinary local.
//
// It gets no MAP entry of its own deliberately. It is not a member this file declares, and claiming
// line 5726 collides with ADOverhaul.State.cs, which still carries pre-561e9ec numbering in which
// 5726 is a licence field. That is the known line-number debt recorded in HEADER-FORMAT.md, not two
// files porting one member; when the numbering sweep reaches State.cs this can become a range entry.
//
// All four of the shipped hook's installer calls are reproduced, and all four editor types they
// point at now exist:
//
//   PhysBoneEditor.WriteSingleton          -> PhysBoneEditor.InstallEditorOverride
//   PhysBoneColliderEditor.InsertProperty  -> PhysBoneColliderEditor.InstallEditorOverride
//   ContactSenderEditor.InvokeProperty     -> ContactSenderEditor.InstallEditorOverride
//   ContactReceiverEditor.ReadPage         -> ContactReceiverEditor.InstallEditorOverride
//
// The 200 ms / 30 try poll is ported as-is. Unity builds its custom-editor table lazily, and a write
// that lands before it is populated is silently discarded when Unity populates it -- so the hook has
// to wait for s_Initialized rather than run at reload time. Thirty tries is six seconds; on a slow
// domain reload the original gave up and logged, and so does this.
//
// Audit status: PARTIAL -- the poll loop, its bounds, the s_Initialized lookup, the success guard
// and both log calls were diffed statement by statement against decompiled 5726-5766 and 6756-6763
// and match, including the error message, which is reproduced verbatim, and the two reflection
// lookups' placement outside the try. All four installer calls are ported. The 2019 build was not
// read for this region.

using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        private const string CustomEditorAttributesTypeName =
            "UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Reinstalls the replacement inspectors after every domain reload, once Unity has finished
        /// building the table they are written into.
        /// </summary>
        /// <remarks>
        /// The override is an in-memory edit to <c>CustomEditorAttributes</c> and is not persisted,
        /// so it has to be reapplied on every reload -- entering play mode, editing a script and
        /// reopening the project all discard it.
        /// <para>
        /// Async rather than a delayed call because the wait is bounded by a condition rather than a
        /// duration: the table is built lazily and the point at which it is ready varies with how
        /// much there is to compile.
        /// </para>
        /// </remarks>
        [DidReloadScripts]
        private static async void InstallInspectorOverrides()
        {
            // Outside the try, as the shipped build has them: these two run in WriteConfiguration
            // itself, and only the poll loop and the installer calls are inside CancelProcess's
            // try. A Unity release that renames either lookup should surface as a throw, not as a
            // swallowed log line -- that failure mode is exactly what makes a missing override
            // look like nothing happening at all.
            Type customEditorAttributes = Type.GetType(CustomEditorAttributesTypeName);
            FieldInfo initializedField = customEditorAttributes.GetField(
                "s_Initialized", BindingFlags.Static | BindingFlags.NonPublic);

            try
            {
                int attempts = 0;
                bool initialized;

                while (true)
                {
                    initialized = (bool)initializedField.GetValue(null);
                    if (initialized)
                    {
                        break;
                    }

                    await Task.Delay(200);

                    attempts++;
                    if (attempts > 30)
                    {
                        Debug.LogError("Failed to apply ADO's custom editors automatically.");
                        break;
                    }
                }

                if (initialized)
                {
                    PhysBoneEditor.InstallEditorOverride();
                    PhysBoneColliderEditor.InstallEditorOverride();
                    ContactSenderEditor.InstallEditorOverride();
                    ContactReceiverEditor.InstallEditorOverride();
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
    }
}
