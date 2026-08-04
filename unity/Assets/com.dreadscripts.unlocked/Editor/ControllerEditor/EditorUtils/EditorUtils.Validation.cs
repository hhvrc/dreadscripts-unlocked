// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static QueryError   -> ValidateCanAddControls(this VRCExpressionsMenu, int),         line 8092
//   static RestartError -> ValidateCanAddControls(this VRCExpressionsMenu.Control, int), line 8079
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The neighbouring members of the same region are deliberately not ported here: InsertError
// (line 8070) and the QueryError overload taking a menu pair (line 8055) are thin wrappers that
// resolve a count from a menu before delegating to these two, and AddError/InvokeError/FindError
// (lines 8109-8135) belong to the control-copying path rather than to validation. They can be
// added as further overloads if a call site needs them.
// Audit status: VERIFIED against export

using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Checks whether <paramref name="controlsToAdd"/> more controls will fit in
        /// <paramref name="menu"/>.
        /// </summary>
        /// <remarks>
        /// The limit is the literal 8 the decompiled source used, not
        /// <see cref="VRCExpressionsMenu.MAX_CONTROLS"/>: the constant would track whatever the
        /// installed SDK allows, and matching the original exactly matters more here than tracking
        /// the SDK. <c>MenuExpressionTreeView</c> hard-codes the same 8 when it greys out a full
        /// menu, so the two agree; both would need changing together if the SDK's limit ever moves.
        ///
        /// The error codes let a caller distinguish the two failures without parsing the message:
        /// 1 for a missing submenu, 2 for one that is too full.
        /// </remarks>
        internal static ValidationResult ValidateCanAddControls(this VRCExpressionsMenu menu, int controlsToAdd)
        {
            if (menu == null)
            {
                return new ValidationResult(false, "SubMenu is Null", 1);
            }

            if (menu.controls.Count + controlsToAdd > 8)
            {
                return new ValidationResult(false, $"Adding {controlsToAdd} controls to {menu.name} would exceed the 8 controls limit", 2);
            }

            return (true, "Can add controls");
        }

        /// <summary>
        /// Checks whether <paramref name="controlsToAdd"/> more controls will fit in the submenu
        /// <paramref name="control"/> points at, rejecting controls that are not submenus at all.
        /// </summary>
        /// <remarks>
        /// A submenu control with a null <c>subMenu</c> is not special-cased: it falls through to the
        /// menu overload, which reports it as the null-submenu case (error code 1).
        /// </remarks>
        internal static ValidationResult ValidateCanAddControls(this VRCExpressionsMenu.Control control, int controlsToAdd)
        {
            if (control == null)
            {
                return (false, "Control is Null");
            }

            if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                return (false, "Control is not a SubMenu");
            }

            return control.subMenu.ValidateCanAddControls(controlsToAdd);
        }
    }
}
