// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static CountError   -> ValidateCanAddControls(descriptor, menu),   line 8042
//   static DisableError -> ValidateCanAddControls(menu, menu),         line 8057
//   static InsertError  -> ValidateCanAddControls(control, menu),      line 8070
//   static AddError     -> AddControls(descriptor, menu),              line 8109
//   static InvokeError  -> AddControls(menu, menu),                    line 8122
//   static FindError    -> AddControls(control, menu),                 line 8135
//   static ExcludeError -> AddControls(menu, controls),                line 8148
//   static InitError    -> Clone(Control),                             line 8177
//   static VisitError   -> Clone(Control.Parameter),                   line 8204
//   static DefineError  -> AnyMenu,                                    line 8212
//   static StartError   -> GetOrCreateExpressionsMenu,                 line 8241
//   static SelectError  -> ValidateMatches(Control, Control),          line 8280
//   static RemoveError  -> Matches(Control.Parameter, Control.Parameter), line 8326
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/
//
// The two ValidateCanAddControls overloads that take a count live in EditorUtils.Validation.cs and
// are the bottom of this stack; the three here resolve a count from a menu and delegate to them.
//
// VRChat allows eight controls per menu, and that is the only limit any of this enforces. The limit
// itself is spelled 8 in the throw message and in Validation.cs's arithmetic, as shipped.
//
// AnyMenu walks a menu tree that is allowed to contain cycles -- a submenu can reference an
// ancestor, and the SDK does not stop it -- so it carries a visited set. A menu already visited
// answers false rather than being re-entered, which also means a cycle terminates instead of
// overflowing the stack.
//
// VENDOR BUG in ValidateMatches, transcribed as shipped: for the two axis-puppet control types it
// rejects any pair that *has* sub-parameters. See the remark at the site.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Whether every control of <paramref name="source"/> would fit in the avatar's expressions
        /// menu.
        /// </summary>
        /// <returns>Error code 1 when the avatar has no expressions menu.</returns>
        internal static ValidationResult ValidateCanAddControls(this VRCAvatarDescriptor avatar,
            VRCExpressionsMenu source)
        {
            if (avatar == null)
            {
                return (false, "Avatar is not set (Null)");
            }

            if (avatar.expressionsMenu == null)
            {
                return new ValidationResult(false, "Avatar Expressions Menu is not set (Null)", 1);
            }

            return avatar.expressionsMenu.ValidateCanAddControls(source);
        }

        /// <summary>
        /// Whether every control of <paramref name="source"/> would fit in
        /// <paramref name="target"/>.
        /// </summary>
        internal static ValidationResult ValidateCanAddControls(this VRCExpressionsMenu target,
            VRCExpressionsMenu source)
        {
            if (source == null)
            {
                return new ValidationResult(false, "Expression Menu is not set (Null)");
            }

            if (target == null)
            {
                return new ValidationResult(false, "Target Expression Menu is not set (Null)");
            }

            return target.ValidateCanAddControls(source.controls.Count);
        }

        /// <summary>
        /// Whether every control of <paramref name="source"/> would fit in the submenu
        /// <paramref name="control"/> points at.
        /// </summary>
        internal static ValidationResult ValidateCanAddControls(this VRCExpressionsMenu.Control control,
            VRCExpressionsMenu source)
        {
            if (source == null)
            {
                return (false, "Expression Menu is not set (Null)");
            }

            return control.ValidateCanAddControls(source.controls.Count);
        }

        /// <summary>
        /// Copies every control of <paramref name="source"/> into the avatar's expressions menu.
        /// Throws if the avatar has none -- this is the commit step, past validation.
        /// </summary>
        internal static VRCExpressionsMenu.Control[] AddControls(this VRCAvatarDescriptor avatar,
            VRCExpressionsMenu source)
        {
            if (avatar == null)
            {
                throw new NullReferenceException("Avatar is not set (Null)");
            }

            if (avatar.expressionsMenu == null)
            {
                throw new NullReferenceException("Avatar Expressions Menu is not set (Null)");
            }

            return avatar.expressionsMenu.AddControls(source);
        }

        /// <summary>Copies every control of <paramref name="source"/> into <paramref name="target"/>.</summary>
        internal static VRCExpressionsMenu.Control[] AddControls(this VRCExpressionsMenu target,
            VRCExpressionsMenu source)
        {
            if (target == null)
            {
                throw new NullReferenceException("Target Expression Menu is not set (Null)");
            }

            if (source == null)
            {
                throw new NullReferenceException("Source Expression Menu is not set (Null)");
            }

            return target.AddControls(source.controls);
        }

        /// <summary>
        /// Copies every control of <paramref name="source"/> into the submenu
        /// <paramref name="control"/> points at. Throws if it is not a submenu control.
        /// </summary>
        internal static VRCExpressionsMenu.Control[] AddControls(this VRCExpressionsMenu.Control control,
            VRCExpressionsMenu source)
        {
            if (control == null)
            {
                throw new ArgumentException("Control is Null");
            }

            if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                throw new ArgumentException("Control is not a SubMenu");
            }

            return control.subMenu.AddControls(source);
        }

        /// <summary>
        /// Copies <paramref name="controls"/> into <paramref name="target"/> and returns the
        /// copies. Throws rather than truncating if the result would exceed eight controls.
        /// </summary>
        /// <remarks>
        /// The controls are cloned, so the caller's originals stay attached to wherever they came
        /// from. A null controls list on the target is created rather than treated as an error.
        /// </remarks>
        internal static VRCExpressionsMenu.Control[] AddControls(this VRCExpressionsMenu target,
            IEnumerable<VRCExpressionsMenu.Control> controls)
        {
            if (target == null)
            {
                throw new NullReferenceException("Target Expression Menu is not set (Null)");
            }

            if (target.controls == null)
            {
                target.controls = new List<VRCExpressionsMenu.Control>();
            }

            if (controls == null)
            {
                throw new NullReferenceException("New Controls are Null");
            }

            VRCExpressionsMenu.Control[] source =
                (controls as VRCExpressionsMenu.Control[]) ?? controls.ToArray();

            if (target.controls.Count + source.Length > 8)
            {
                throw new Exception(
                    $"Adding {source.Length} controls to {target.name} would exceed the 8 controls limit");
            }

            VRCExpressionsMenu.Control[] copies = source.Select(Clone).ToArray();
            foreach (VRCExpressionsMenu.Control copy in copies)
            {
                target.controls.Add(copy);
            }

            EditorUtility.SetDirty(target);
            return copies;
        }

        /// <summary>
        /// A detached copy of the control. The submenu and icon are shared, not copied -- they are
        /// assets, and a copied control is meant to point at the same ones.
        /// </summary>
        internal static VRCExpressionsMenu.Control Clone(VRCExpressionsMenu.Control control)
        {
            VRCExpressionsMenu.Control copy = new VRCExpressionsMenu.Control
            {
                type = control.type,
                value = control.value,
                icon = control.icon,
                name = control.name,
                subMenu = control.subMenu,
                parameter = Clone(control.parameter)
            };

            if (control.subParameters != null)
            {
                copy.subParameters = new VRCExpressionsMenu.Control.Parameter[control.subParameters.Length];
                for (int i = 0; i < control.subParameters.Length; i++)
                {
                    copy.subParameters[i] = Clone(control.subParameters[i]);
                }
            }
            else
            {
                copy.subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>();
            }

            return copy;
        }

        /// <summary>
        /// A detached copy of a control's parameter reference. A null reference becomes an empty
        /// one, never null, because the SDK's inspector does not cope with a null here.
        /// </summary>
        internal static VRCExpressionsMenu.Control.Parameter Clone(VRCExpressionsMenu.Control.Parameter parameter)
        {
            return new VRCExpressionsMenu.Control.Parameter
            {
                name = parameter == null ? string.Empty : parameter.name
            };
        }

        /// <summary>
        /// Whether <paramref name="predicate"/> holds for the menu or any menu reachable from it
        /// through submenu controls. Stops at the first that matches.
        /// </summary>
        /// <param name="visited">
        /// Menus already seen. Supply one to continue a walk across several roots; the default
        /// starts a fresh walk. A menu in this set answers false without being examined.
        /// </param>
        internal static bool AnyMenu(this VRCExpressionsMenu menu, Func<VRCExpressionsMenu, bool> predicate,
            HashSet<VRCExpressionsMenu> visited = null)
        {
            if (menu == null)
            {
                return false;
            }

            if (visited == null)
            {
                visited = new HashSet<VRCExpressionsMenu>();
            }

            if (!visited.Add(menu))
            {
                return false;
            }

            if (predicate(menu))
            {
                return true;
            }

            foreach (VRCExpressionsMenu.Control control in menu.controls)
            {
                if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu
                    && control.subMenu.AnyMenu(predicate, visited))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The avatar's expressions menu, creating an empty one under <paramref name="folder"/> if
        /// it has none, and switching custom expressions on either way.
        /// </summary>
        /// <param name="duplicate">
        /// Copy an existing menu to <paramref name="folder"/> and use the copy. Note only the root
        /// menu is copied -- its submenus are still shared with the original.
        /// </param>
        internal static VRCExpressionsMenu GetOrCreateExpressionsMenu(this VRCAvatarDescriptor avatar, string folder,
            bool duplicate = false)
        {
            VRCExpressionsMenu menu = avatar.expressionsMenu;
            if (menu)
            {
                if (duplicate)
                {
                    menu = DuplicateAssetTo(menu, PrepareAssetPath(folder, menu.name + ".asset"));
                }
            }
            else
            {
                menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                menu.controls = new List<VRCExpressionsMenu.Control>();
                AssetDatabase.CreateAsset(menu, PrepareAssetPath(folder, avatar.name + " Menu.asset"));
            }

            avatar.customExpressions = true;
            avatar.expressionsMenu = menu;
            EditorUtility.SetDirty(avatar);
            return menu;
        }

        /// <summary>
        /// Whether two controls do the same thing: same type, same driving parameter, and -- for a
        /// submenu -- the same submenu asset.
        /// </summary>
        /// <returns>
        /// Error code 1 for a type mismatch, 2 for a parameter mismatch, 3 for a submenu mismatch,
        /// 4 for sub-parameters. Two references to the same control, and two nulls, both pass.
        /// </returns>
        /// <remarks>
        /// VENDOR BUG in the sub-parameter branch, transcribed as shipped: the guard reads
        /// <c>!a.IsNullOrEmpty() || !b.IsNullOrEmpty() || lengths differ</c>, so a pair of
        /// axis-puppet controls that both *have* sub-parameters is rejected outright and the
        /// element-by-element comparison below it is unreachable. The evident intent was
        /// <c>a.IsNullOrEmpty() != b.IsNullOrEmpty()</c>. Left alone because correcting it would
        /// change which controls the tool considers matching.
        /// <para>
        /// The branch also only covers TwoAxisPuppet and FourAxisPuppet -- the decompiled test is
        /// <c>(uint)(type - 201) &lt;= 1</c>. RadialPuppet, which also carries a sub-parameter,
        /// falls through and has its sub-parameters ignored entirely.
        /// </para>
        /// </remarks>
        internal static ValidationResult ValidateMatches(this VRCExpressionsMenu.Control control,
            VRCExpressionsMenu.Control other)
        {
            if (control == other)
            {
                return true;
            }

            if (control == null || other == null)
            {
                return new ValidationResult(false, "One of the controls is null");
            }

            if (control.type != other.type)
            {
                return new ValidationResult(false, "Control types do not match", 1);
            }

            if (!control.parameter.Matches(other.parameter))
            {
                return new ValidationResult(false, "Parameter does not match", 2);
            }

            if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                if (control.subMenu != other.subMenu)
                {
                    return new ValidationResult(false, "SubMenus do not match", 3);
                }
            }
            else if (control.type == VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet
                     || control.type == VRCExpressionsMenu.Control.ControlType.FourAxisPuppet)
            {
                if (!control.subParameters.IsNullOrEmpty() || !other.subParameters.IsNullOrEmpty()
                                                           || control.subParameters?.Length !=
                                                           other.subParameters?.Length)
                {
                    return new ValidationResult(false, "SubParameters do not match", 4);
                }

                if (control.subParameters != null && other.subParameters != null)
                {
                    for (int i = 0; i < control.subParameters.Length; i++)
                    {
                        if (!control.subParameters[i].Matches(other.subParameters[i]))
                        {
                            return new ValidationResult(false, "SubParameters do not match", 4);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Whether two control parameter references name the same parameter. Two nulls match; one
        /// null and one non-null do not, whatever the non-null one holds. Two references that are
        /// both blank -- null name or empty name -- also match.
        /// </summary>
        internal static bool Matches(this VRCExpressionsMenu.Control.Parameter parameter,
            VRCExpressionsMenu.Control.Parameter other)
        {
            if ((parameter == null) ^ (other == null))
            {
                return false;
            }

            if (parameter == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(parameter.name) && string.IsNullOrEmpty(other.name))
            {
                return true;
            }

            return parameter.name == other.name;
        }
    }
}
