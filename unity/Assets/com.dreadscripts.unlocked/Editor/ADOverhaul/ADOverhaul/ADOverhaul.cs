// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// The root of the ADOverhaul class: the type declaration and the product's own identity. Everything
// else lives in a sibling partial, grouped by the type the members operate on. Line numbers move
// with the snapshot; the member names are the durable reference.
//
// Members in this file:
//   m_Expression (line 5742) -> version
//
// The split:
//   ADOverhaul.Logging.cs             -> the [ADOverhaul]-prefixed console log helpers
//   ADOverhaul.Buttons.cs             -> the coloured toggle buttons, by value and by property
//   ADOverhaul.SerializedProperties.cs-> property rows the contact/PhysBone inspectors are built from
//   ADOverhaul.AvatarDescriptors.cs   -> avatar selection, validation and the tables derived from it
//
// NESTED TYPES ARE NOT HERE. Every nested type of the decompiled ADOverhaul has been lifted to a
// top-level type in this namespace, as the ones ported before this file were:
//   ADOverhaulWindow (35), BugReporter (168), ProcessRunner (490), JsonObject (605), JsonValue
//   (680), CustomLogType (743), ADOSettings (751), PhysBoneColliderEditor (2161),
//   PhysBoneEditor (2358)
// The 26 `_003C_003Ec*` compiler-generated closure classes get no file at all; they are dissolved
// back into lambdas and local functions at their use sites.
//
// STILL TO PORT, and deliberately absent rather than stubbed. Everything reachable from the
// following decompiled members is not here yet, because each reaches further unported outer-class
// state. Names are the decompiled ones, with the snapshot line number:
//   the PhysBone testing feature       6060-6300, 6488-6535, 6949  (VerifyConfiguration and friends)
//   the scene-view shape handles       5746-6058, 8298-8408       (RunConfiguration, OrderConfiguration)
//   the scene-view panels              8228-8296                  (SelectIdentifier, WriteIdentifier, MoveIdentifier)
//   the toolbar strip and its menu     7904-8018                  (SortIdentifier, InvokeIdentifier)
//   the update / announcement notice   8020-8227                  (CustomizeIdentifier .. SetupIdentifier)
//   the inspector-override installer   6187-6270, 6552-6559       (DestroyConfiguration, WriteConfiguration)
//   ContactReceiverEditor (1690) and ContactSenderEditor (1959)
//
// LICENCE CODE. This class carried the HWID/HMAC licence validation inline, so it is stripped
// during the port rather than skipped. What that means concretely is recorded in each partial that
// had a region removed; the parts of it that were whole methods are named in the port report and
// are simply absent here. The tool behaves as fully licensed: wherever a gate decided whether a
// feature ran, the feature runs.
//
// Audit status: VERIFIED against export -- this file holds one field.

using DreadScripts.Common;

namespace DreadScripts.ADOverhaul
{
    /// <summary>
    /// Avatar Dynamics Overhaul: replacement inspectors for VRChat's PhysBone, PhysBone collider
    /// and contact components, together with the scene-view handles, overlays and play-mode
    /// PhysBone test harness they drive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every member is static. The shipped type is a plain sealed class rather than a static one
    /// and nothing ever instantiates it; that is transcribed as-is.
    /// </para>
    /// <para>
    /// The inspectors are installed by overwriting Unity's own custom-editor table rather than by
    /// carrying <c>[CustomEditor]</c> attributes, which is what lets the tool be toggled off and
    /// VRChat's own inspectors restored without a domain reload. See
    /// <c>ADOEditorUtility.OverrideCustomEditor</c>.
    /// </para>
    /// <para>
    /// This reconstruction is a partial port — see the file header for what is still missing.
    /// </para>
    /// </remarks>
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// The shipped version, drawn in the toolbar strip and sent with every update check.
        /// </summary>
        internal static readonly SemVer version = new SemVer("0.11.1");
    }
}
