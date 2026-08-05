// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ComponentQueue.cs
//
//   gameObject                           -> gameObject, line 79
//   components                           -> components, line 81
//   componentIndex                       -> componentIndex, line 83
//   target                               -> target, line 85
//   targetType                           -> targetType, line 87
//   propertyNames                        -> propertyNames, line 89
//   propertyIndex                        -> propertyIndex, line 91
//   value                                -> value, line 93
//   toggleableTypes                      -> toggleableTypes, line 95
//   [SpecialName] IsValid()              -> IsValid (property), line 103
//   [SpecialName] IsOn()                 -> IsOn (property), line 113
//   [SpecialName] GameObject()           -> GameObject (property getter), line 119
//   [SpecialName] GameObject(GameObject) -> GameObject (property setter), line 125
//   [SpecialName] ComponentIndex()       -> ComponentIndex (property getter), line 135
//   [SpecialName] ComponentIndex(int)    -> ComponentIndex (property setter), line 141
//   [SpecialName] PropertyName()         -> PropertyName (property), line 152
//   ComponentQueue()                     -> ComponentQueue(), line 161
//   ComponentQueue(GameObject)           -> ComponentQueue(GameObject), line 167
//   Next(bool)                           -> Next(bool), line 175
//   Previous(bool)                       -> Previous(bool), line 185
//   Refresh()                            -> Refresh(), line 195
//   RefreshComponents()                  -> RefreshComponents(), line 218
//   WrapComponentIndex()                 -> WrapComponentIndex(), line 223
//   UpdateTarget()                       -> UpdateTarget(), line 239
//   [SpecialName] IsToggleable()         -> IsToggleable (property), line 268
//   <>c__DisplayClass27_0.ComputePage    -> RefreshPropertyNames(), line 39
//   <>c__DisplayClass27_0.ConcatPage     -> TrySelectProperty(string), line 62
//   <>c__DisplayClass27_0.MovePage       -> the where-clause lambda in RefreshPropertyNames, line 57
//   <>c__DisplayClass27_0.CallPage       -> folded into TrySelectProperty, line 73
//   <>c.PublishPage                      -> the select lambda in RefreshPropertyNames, line 21
//   <>c.PopPage                          -> the orderBy lambda in RefreshPropertyNames, line 26
//   IsToggleable(Type)                   -> folded into the IsToggleable property, line 274
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The accessor pairs the decompiler left as [SpecialName] methods are restored as the properties
// they started life as. The closures the obfuscator hoisted into display classes are restored as
// ordinary members and lambdas: the display class's two fields carry the receiver and the sought
// property name, which are `this` and a parameter here. Nothing of the original type is left
// unported.
//
// Audit status: PARTIAL -- the mapping above was re-derived member by member against
// reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ComponentQueue.cs on 2026-08-05, and
// every line number lands on the member named; the doc-comment prose on the members below was not
// re-checked against the decompiled bodies.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// One row of the Quick Toggle list: a scene object, which of its components is being addressed,
    /// which animatable property of that component to write, and the value to write.
    /// </summary>
    /// <remarks>
    /// The row is edited by cycling: the user picks an object and then steps through its components
    /// with <see cref="Next"/> / <see cref="Previous"/>. Every step has to re-derive what can be
    /// animated on the new target, and the class exists to keep that derived state — the component
    /// array, the property-name list and the two indices into them — consistent with the object
    /// after each edit, so the GUI can address it by index alone.
    /// </remarks>
    internal class ComponentQueue
    {
        /// <summary>
        /// The types whose "is this on" property the simple (non-advanced) Quick Toggle mode is
        /// willing to animate. Cycling in that mode skips every component that is not one of these.
        /// </summary>
        internal static readonly Type[] toggleableTypes =
        {
            typeof(GameObject),
            typeof(Behaviour),
            typeof(Renderer)
        };

        private GameObject gameObject;

        public Component[] components;

        /// <summary>Index into <see cref="components"/>; -1 addresses the GameObject itself.</summary>
        public int componentIndex = -1;

        /// <summary>The GameObject or Component the curve will be written against.</summary>
        public Object target;

        public Type targetType;

        public string[] propertyNames;

        public int propertyIndex;

        public float value = 1f;

        /// <summary>
        /// Whether the row still addresses something that exists. The indices are held across edits
        /// to the object and can outlive the arrays they point into.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (gameObject && componentIndex < components.Length)
                {
                    return propertyIndex < propertyNames.Length;
                }

                return false;
            }
        }

        public bool IsOn => value > 0f;

        /// <summary>
        /// The object being addressed. Assigning a different one re-derives the component list and
        /// tries to keep the currently selected component type.
        /// </summary>
        internal GameObject GameObject
        {
            get
            {
                return gameObject;
            }
            set
            {
                if (gameObject != value)
                {
                    gameObject = value;
                    Refresh();
                }
            }
        }

        public int ComponentIndex
        {
            get
            {
                return componentIndex;
            }
            set
            {
                if (componentIndex != value)
                {
                    componentIndex = value;
                    WrapComponentIndex();
                    UpdateTarget();
                }
            }
        }

        /// <summary>
        /// The selected property name, or empty when the target has nothing animatable — the GUI
        /// draws this straight into a dropdown label, so it must never be null.
        /// </summary>
        public string PropertyName
        {
            get
            {
                if (propertyNames.Length == 0 || propertyIndex >= propertyNames.Length)
                {
                    return string.Empty;
                }

                return propertyNames[propertyIndex];
            }
        }

        public ComponentQueue()
        {
            propertyNames = Array.Empty<string>();
            components = Array.Empty<Component>();
        }

        /// <remarks>
        /// The initial value mirrors the object's current active state, so adding a row and toggling
        /// nothing records what is already on screen.
        /// </remarks>
        public ComponentQueue(GameObject gameObject)
        {
            GameObject = gameObject;
            value = gameObject.activeSelf ? 1 : 0;
            RefreshComponents();
            UpdateTarget();
        }

        /// <summary>Step to the next component, optionally skipping ones that cannot be toggled.</summary>
        public void Next(bool toggleableOnly)
        {
            do
            {
                ComponentIndex = ComponentIndex + 1;
            }
            while (toggleableOnly && !IsToggleable);
        }

        /// <inheritdoc cref="Next"/>
        public void Previous(bool toggleableOnly)
        {
            do
            {
                ComponentIndex = ComponentIndex - 1;
            }
            while (toggleableOnly && !IsToggleable);
        }

        /// <summary>
        /// Re-derives everything from the current object, preferring a component of the same type as
        /// the one that was selected before, so that retargeting a row at a similar object keeps the
        /// user's choice rather than resetting it.
        /// </summary>
        private void Refresh()
        {
            RefreshComponents();
            if (gameObject)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i].GetType() == targetType)
                    {
                        ComponentIndex = i;
                        UpdateTarget();
                        return;
                    }
                }

                componentIndex = -1;
                UpdateTarget();
            }
            else
            {
                UpdateTarget();
            }
        }

        private void RefreshComponents()
        {
            components = GameObject ? GameObject.GetComponents<Component>() : Array.Empty<Component>();
        }

        /// <summary>
        /// Brings <see cref="componentIndex"/> back into range, wrapping past either end so that
        /// stepping cycles through the object's components and back to the GameObject itself.
        /// </summary>
        private void WrapComponentIndex()
        {
            // The index may have been stepped past the end of a stale array, so re-fetch before
            // deciding whether it is genuinely out of range.
            if (components == null || componentIndex >= components.Length)
            {
                RefreshComponents();
            }

            if (componentIndex >= components.Length)
            {
                componentIndex = -1;
            }
            else if (componentIndex < -1)
            {
                componentIndex = components.Length - 1;
            }
        }

        /// <summary>
        /// Points <see cref="target"/> at the addressed object and, when that changed the type,
        /// rebuilds the property list around it.
        /// </summary>
        private void UpdateTarget()
        {
            target = !GameObject ? null : (ComponentIndex != -1 ? (Object)components[ComponentIndex] : GameObject);

            Type previousType = targetType;
            targetType = target ? target.GetType() : null;
            if (!target || previousType == targetType)
            {
                return;
            }

            if (propertyNames == null || propertyIndex >= propertyNames.Length || ComponentIndex == -1)
            {
                RefreshPropertyNames();
                return;
            }

            string previousProperty = propertyNames[propertyIndex];
            RefreshPropertyNames();
            if (propertyNames.Length != 0 && !TrySelectProperty(previousProperty))
            {
                // "Is this thing on" is spelled m_IsActive on a GameObject and m_Enabled on a
                // component, so stepping between the two should carry the selection over rather
                // than fall back to the first property.
                previousProperty = previousProperty == "m_IsActive"
                    ? "m_Enabled"
                    : previousProperty == "m_Enabled"
                        ? "m_IsActive"
                        : string.Empty;

                if (!TrySelectProperty(previousProperty))
                {
                    propertyIndex = 0;
                }
            }
        }

        /// <summary>
        /// Lists the properties Unity will let an animation clip write on the current target, sorted
        /// so the dropdown order does not depend on component order.
        /// </summary>
        /// <remarks>
        /// Asking Unity for the bindings of the whole object and filtering by type is the only way to
        /// get the animatable set of a single component. The GameObject case is not asked at all: its
        /// only meaningful toggle is <c>m_IsActive</c>, and the full binding list would include every
        /// component's properties.
        /// </remarks>
        private void RefreshPropertyNames()
        {
            if (ComponentIndex != -1)
            {
                propertyNames = AnimationUtility.GetAnimatableBindings(GameObject, GameObject)
                    .Where(b => b.type == targetType)
                    .Select(b => b.propertyName)
                    .OrderBy(s => s)
                    .ToArray();
            }
            else
            {
                propertyNames = new[] { "m_IsActive" };
            }

            if (propertyIndex >= propertyNames.Length)
            {
                propertyIndex = Mathf.Max(0, propertyNames.Length - 1);
            }
        }

        /// <summary>
        /// Selects <paramref name="propertyName"/> if the current target has it, reporting whether it
        /// did. Leaves the selection alone otherwise, so a caller can try a second candidate.
        /// </summary>
        private bool TrySelectProperty(string propertyName)
        {
            int index = Array.FindIndex(propertyNames, s => s == propertyName);
            if (index < 0)
            {
                return false;
            }

            propertyIndex = index;
            return true;
        }

        private bool IsToggleable
        {
            get
            {
                return toggleableTypes.Any(t => targetType == t || targetType.IsSubclassOf(t));
            }
        }
    }
}
