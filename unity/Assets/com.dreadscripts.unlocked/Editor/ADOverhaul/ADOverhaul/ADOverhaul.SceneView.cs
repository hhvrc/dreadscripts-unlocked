// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: decompiled lines 6060-6478 of the outer ADOverhaul class. Line numbers are
// relative to the current snapshot; the decompiled names below are the durable reference.
//
//   DestroyConfiguration   -> ResolvePhysBoneReflection,  line 6256
//   SortConfiguration      -> SetShapeCapabilities,       line 6391
//   CustomizeConfiguration -> DrawCollisionTagElement,    line 6433
//   ConcatConfiguration    -> DrawCollisionTagsHeader,    line 6474
//
// Field references go through the names agreed in ADOverhaul.State.cs; see that file's header for
// the obfuscated-to-English table. Helpers reused rather than reimplemented:
//   ADOEditorUtility.CustomizeRef()._CreatorSerializer -> ADOEditorUtility.contents.removeSelection
//   ADOEditorUtility.MapRef().utilsMethod              -> ADOEditorUtility.styles.footerButton
//
// ────────────────────────────── WHAT THIS REGION ACTUALLY IS ──────────────────────────────
//
// The region is not one family. Reading 6060-6478 straight through, it is four:
//   (a) the scene-view shape-handle overlay  -- CalculateConfiguration, CalcConfiguration
//   (b) the PhysBone test-mode driver and its scene-view overlay -- DeleteConfiguration,
//       DefineConfiguration, DestroyConfiguration, NewConfiguration, CompareConfiguration,
//       VerifyConfiguration, SetConfiguration
//   (c) two inspector-side helpers with no scene-view involvement at all -- SortConfiguration,
//       InvokeConfiguration
//   (d) two ReorderableList callbacks for the contact "Collision Tags" list -- CustomizeConfiguration
//       (drawElementCallback) and ConcatConfiguration (drawHeaderCallback)
// (b) is by far the bulk of it and is a simulation driver, not an overlay. The file keeps its
// assigned name so the wave's file ownership stays legible, but only (b)'s reflection bootstrap and
// all of (c)/(d) that could be ported are actually in it.
//
// ───────────────────────── SceneView.duringSceneGui REGISTRATION AUDIT ─────────────────────────
//
// Two members of this region are SceneView.duringSceneGui handlers. Neither is ported, but a leaked
// subscription survives domain reload and shows as a permanently stuck overlay, so where each one is
// registered is recorded here for whoever lands them.
//
//   CalculateConfiguration (6060) -- the shape-handle overlay. Subscribed and unsubscribed ONLY by
//       CancelConfiguration (6496), which does `duringSceneGui -= CalculateConfiguration` first and
//       then re-adds it when its argument is true. The remove-then-add idiom is what keeps the
//       handler single-registered across the several inspector OnEnables that call it; on the false
//       path it also restores Tools.hidden, because CalculateConfiguration sets it unconditionally.
//   DeleteConfiguration (6107) -- the test-mode overlay. Added by VerifyConfiguration (6365-6366,
//       again remove-then-add) when test mode starts, removed by SetConfiguration (6372) when it
//       stops. SetConfiguration is reached from NewConfiguration, which is also what
//       FillConfiguration (6488) calls on ExitingEditMode, so entering play mode tears the
//       subscription down. There is no [InitializeOnLoad] unsubscribe, so a domain reload during an
//       active test session does leak this handler -- shipped behaviour, noted here as a hazard, not
//       something this port introduces or repairs.
//
// ───────────────────────────────── DEFERRED, NOT STUBBED ─────────────────────────────────
//
// Nine of the region's thirteen members are omitted. Every one of them funnels, directly or through
// one hop, into the ADOSettings singleton, which is not ported: Editor/Common/Settings/ holds the
// setting *framework* (SettingBase, ValueSettings, CompositeSettings, SettingsPersistence) but not
// the ADOSettings type that declares hideToolsDuringTesting, toolOverlayAlignment and
// hasReadColliderTestingWarning. ADOSettings is the single highest-value unblock for this region.
//
//   CalculateConfiguration  6060  needs WriteIdentifier (8249) and CalcConfiguration.
//   CalcConfiguration       6090  needs PatchConfiguration (6735), owned by the shared-drawing region.
//   DeleteConfiguration     6107  needs ADOSettings.hideToolsDuringTesting, MoveIdentifier (8266),
//                                 PublishIdentifier (8290) and InsertConfiguration (6949).
//   DefineConfiguration     6187  the per-editor-update simulation tick. Reaches only ported state
//                                 fields and the MethodInfos below -- except for its one early exit,
//                                 `if (!testPhysBoneManager) { NewConfiguration(); return; }`, which
//                                 makes it transitively blocked. It is otherwise ready to land the
//                                 moment NewConfiguration does.
//   NewConfiguration        6272  needs VerifyConfiguration and SetConfiguration.
//   CompareConfiguration    6290  needs NewConfiguration.
//   VerifyConfiguration     6299  needs ADOSettings.hasReadColliderTestingWarning, NewIdentifier
//                                 (7806), ViewIdentifier (8410) and DeleteConfiguration.
//   SetConfiguration        6369  needs DefineConfiguration and DeleteConfiguration as delegate
//                                 targets.
//   InvokeConfiguration     6410  needs SearchConfiguration (6717), owned by the shared-drawing
//                                 region.
//
// Two notes for whoever picks these up:
//   * CompareConfiguration (6290) reads `if (isTesting) NewConfiguration(); NewConfiguration();` --
//     i.e. the "Restart" button toggles test mode off and straight back on, and the guard exists
//     only so that pressing Restart while not testing still starts a session rather than stopping a
//     nonexistent one. It is not a decompiler artifact; the 2019 build has the same shape.
//   * VerifyConfiguration's `_003C_003Ec__DisplayClass54_0 pol` and the `ref` parameter on
//     ViewIdentifier are capture-struct artifacts. The original is a local `List<Transform>` closed
//     over by ViewIdentifier; restore the local and pass the list, do not port the struct.
//
// ─────────────────────────────── LICENCE GATE, NOT PORTED ───────────────────────────────
//
// CalculateConfiguration (6067) and CustomizeConfiguration (6451) each open with an inline
// Func<bool> that HMAC-SHA256s two outer-class strings against a hard-coded key and skips the draw
// when the digest does not match. This is the protector's activation gate against a server that no
// longer answers, the same check already dropped from PhysBoneEditor.OnInspectorGUI and
// PhysBoneColliderEditor; it is not reproduced here, so DrawCollisionTagElement draws
// unconditionally. No other behaviour of the method depends on it.
//
// ListAuthentication (8692) -- a one-line `type.GetMethod(name, flags)` wrapper -- is deliberately
// inlined into ResolvePhysBoneReflection rather than declared here, because line 8692 falls in
// another file's region and declaring it twice would not compile. Nothing is lost: it has no callers
// outside this region.
//
// No obfuscator scaffolding was found in 6060-6478. Nothing in the range is an always-null static
// paired with a null-check, an empty marker type or a licensing remnant beyond the two inline gates
// named above, and no member was dropped for that reason.
//
// 2019 vs 2022: the same thirteen members with the same behaviour (2019 lines 6035-6462, under
// different obfuscated names). The only differences are ones the decompiler chose: the enable/disable
// branches of the test-mode tick are written inverted in the two builds, and InvokeConfiguration's
// switch lists its cases 2/1/0 in 2019 against 2/0/1 in 2022. Neither changes evaluation order or
// result. No divergence to record.

using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOverhaul
    {
        #region PhysBone test mode

        /// <summary>
        /// Looks up the private Unity messages that PhysBone test mode has to invoke by hand, once
        /// per domain load.
        /// </summary>
        /// <remarks>
        /// Test mode runs the VRChat PhysBone simulation inside the editor, outside play mode, which
        /// means Unity never sends <c>Start</c>, <c>OnEnable</c>, <c>OnDisable</c> or the manager's
        /// <c>LateUpdate</c> to the duplicated components. The driver synthesises them, and the SDK
        /// exposes none of the four publicly, so each is fetched reflectively here and cached.
        /// <para>
        /// The lookups go against the <c>Base</c> types rather than <c>VRCPhysBone</c> /
        /// <c>VRCPhysBoneCollider</c> because that is where the SDK declares the messages; a lookup
        /// on the concrete type with <see cref="System.Reflection.BindingFlags.NonPublic"/> would not
        /// see an inherited private member.
        /// </para>
        /// </remarks>
        private static void ResolvePhysBoneReflection()
        {
            if (physBoneReflectionResolved)
            {
                return;
            }

            physBoneReflectionResolved = true;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            physBoneManagerLateUpdate = typeof(PhysBoneManager).GetMethod("LateUpdate", flags);
            physBoneManagerOnDestroy = typeof(PhysBoneManager).GetMethod("OnDestroy", flags);

            physBoneStart = typeof(VRCPhysBoneBase).GetMethod("Start", flags);
            physBoneOnEnable = typeof(VRCPhysBoneBase).GetMethod("OnEnable", flags);
            physBoneOnDisable = typeof(VRCPhysBoneBase).GetMethod("OnDisable", flags);

            physBoneColliderStart = typeof(VRCPhysBoneColliderBase).GetMethod("Start", flags);
            physBoneColliderOnEnable = typeof(VRCPhysBoneColliderBase).GetMethod("OnEnable", flags);
            physBoneColliderOnDisable = typeof(VRCPhysBoneColliderBase).GetMethod("OnDisable", flags);
        }

        #endregion

        #region Shape-handle capabilities

        /// <summary>
        /// Declares which of the shape properties the component currently being drawn actually has,
        /// and clears the matching edit toggle for any it does not.
        /// </summary>
        /// <remarks>
        /// Called from the inspectors as the shape type changes -- a sphere has a radius but no
        /// height and no meaningful orientation, a plane has neither radius nor height. Clearing the
        /// toggle alongside the capability is what stops a handle the user left switched on for a
        /// capsule from reappearing when they switch that collider to a sphere. Position is
        /// unconditional and so has no capability flag.
        /// </remarks>
        private static void SetShapeCapabilities(bool hasRadius, bool hasHeight, bool hasRotation)
        {
            shapeHasRadius = hasRadius;
            shapeHasHeight = hasHeight;
            shapeHasRotation = hasRotation;

            if (!shapeHasRadius)
            {
                editingRadius = false;
            }

            if (!shapeHasHeight)
            {
                editingHeight = false;
            }

            if (!shapeHasRotation)
            {
                editingRotation = false;
            }
        }

        #endregion

        #region Collision tag list

        /// <summary>
        /// Draws one row of a contact component's collision-tag list: a tag picker, the editable tag
        /// string, and a delete button.
        /// </summary>
        /// <remarks>
        /// This is a <c>ReorderableList</c> element callback, which is why it takes the list property
        /// and an index rather than the element -- and why it has to bounds-check them itself: the
        /// list can be drawn for one more frame after an element is removed, with an index that no
        /// longer exists.
        /// <para>
        /// The picker is a <see cref="EditorGUI.Popup(Rect, int, string[])"/> held permanently at
        /// index -1 so that it shows no current value and acts purely as a menu; picking an entry
        /// writes it into the string field and the popup falls back to -1 on the next frame. Its
        /// options come from <see cref="avatarCollisionTags"/>, where VRChat's built-in tags carry a
        /// "Default/" prefix to group them into a submenu -- the prefix is a display device only, so
        /// it is stripped before the tag is stored. The whole picker is disabled when no target
        /// avatar is set, since the tag list is gathered from that avatar.
        /// </para>
        /// </remarks>
        private static void DrawCollisionTagElement(SerializedProperty collisionTags, Rect rowRect, int index)
        {
            if (index >= collisionTags.arraySize || index < 0)
            {
                return;
            }

            SerializedProperty element = collisionTags.GetArrayElementAtIndex(index);

            rowRect.y += 1f;
            rowRect.height = 18f;
            rowRect.width -= 44f;

            Rect pickerRect = rowRect;
            pickerRect.width = 21f;

            Rect fieldRect = rowRect;
            fieldRect.x += 22f;
            fieldRect.width -= 12f;

            Rect deleteRect = fieldRect;
            deleteRect.x += fieldRect.width;
            deleteRect.width = 28f;

            using (new EditorGUI.DisabledScope(!selectedAvatar))
            {
                int picked = EditorGUI.Popup(pickerRect, -1, avatarCollisionTags);
                if (picked != -1)
                {
                    element.stringValue = Regex.Replace(avatarCollisionTags[picked], "^Default/", string.Empty);
                }
            }

            EditorGUI.PropertyField(fieldRect, element, GUIContent.none);

            if (GUI.Button(deleteRect, ADOEditorUtility.contents.removeSelection, ADOEditorUtility.styles.footerButton))
            {
                collisionTags.DeleteArrayElementAtIndex(index);
            }
        }

        /// <summary>
        /// Draws the header row of the collision-tag list.
        /// </summary>
        /// <remarks>
        /// The style is rebuilt on every call, which for a <c>ReorderableList</c> header means once
        /// per repaint. That is what the shipped build does and it is reproduced rather than hoisted
        /// into the shared style table, so that the port stays a faithful mirror of the original;
        /// the cost is a small per-frame allocation, not a behavioural difference.
        /// </remarks>
        private static void DrawCollisionTagsHeader(Rect headerRect)
        {
            GUIStyle style = new GUIStyle("boldlabel");
            GUI.Label(headerRect, "Collision Tags", style);
        }

        #endregion
    }
}
