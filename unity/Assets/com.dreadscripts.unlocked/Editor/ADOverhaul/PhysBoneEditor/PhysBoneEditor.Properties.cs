// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the SerializedProperty cache of PhysBoneEditor, lines 2792-2906 of the current
// snapshot, and PrintSingleton, lines 4113-4208. Line numbers move with the snapshot; the member
// names below are the durable reference.
//
//   PrintSingleton()               -> CacheProperties(),  line 269
//   taskAuthentication             -> bindings,           line 233
//   m_CustomerAuthentication       -> bindingLabels,      line 241
//   m_DatabaseAuthentication       -> bindingPopupValues, line 244
//   _HelperAuthentication          -> popupValueToBindingIndex, line 254
//   _CandidateAuthentication       -> bindingIndexToPopupValue, line 257
//   m_ReaderAuthentication         -> bindingLabelsBuilt, line 262
//
// The fifty-four SerializedProperty statics are renamed to the serialized field each one resolves,
// so the mapping is read off the FindProperty call rather than listed here:
//
//   _ValueIdentifier -> version                m_UtilsAuthentication  -> collisionFilter
//   _ErrorIdentifier -> integrationType        _PageAuthentication    -> radius
//   producerIdentifier -> rootTransform        propertyAuthentication -> radiusCurve
//   m_TemplateIdentifier -> ignoreTransforms   m_SingletonAuthentication -> colliders
//   _WriterIdentifier -> endpointPosition      _AccountAuthentication -> limitType
//   classIdentifier -> multiChildType          m_ParamsAuthentication -> maxAngleX
//   _DicIdentifier -> pull                     importerAuthentication -> maxAngleXCurve
//   _ContainerIdentifier -> pullCurve          serverAuthentication   -> maxAngleZ
//   m_SchemaIdentifier -> spring               m_WatcherAuthentication -> maxAngleZCurve
//   bridgeIdentifier -> springCurve            regAuthentication      -> limitRotation
//   publisherIdentifier -> stiffness           processAuthentication  -> limitRotationX
//   _MerchantIdentifier -> stiffnessCurve      statusAuthentication   -> limitRotationY
//   m_ProcIdentifier -> immobile               m_ValAuthentication    -> limitRotationZ
//   configurationAuthentication -> immobileType    adapterAuthentication -> limitRotationXCurve
//   _IdentifierAuthentication -> immobileCurve     _ProxyAuthentication  -> limitRotationYCurve
//   m_AuthenticationAuthentication -> gravity      m_RefAuthentication   -> limitRotationZCurve
//   contextAuthentication -> gravityCurve          comparatorAuthentication -> allowGrabbing
//   _SerializerAuthentication -> gravityFalloff    iteratorAuthentication   -> allowPosing
//   m_MethodAuthentication -> gravityFalloffCurve  m_PredicateAuthentication -> poseFilter
//   consumerAuthentication -> allowCollision       _ProductAuthentication   -> grabFilter
//   _CollectionAuthentication -> grabMovement      interceptorAuthentication -> snapToHand
//   m_RegistryAuthentication -> stretchMotion      _ClientAuthentication -> stretchMotionCurve
//   m_ObserverAuthentication -> maxStretch         broadcasterAuthentication -> maxStretchCurve
//   m_EventAuthentication -> maxSquish             m_RecordAuthentication -> maxSquishCurve
//   resolverAuthentication -> isAnimated           tagAuthentication -> parameter
//   _FilterAuthentication -> resetWhenDisabled     m_FactoryAuthentication -> showGizmos
//   m_AttributeAuthentication -> boneOpacity       m_InstanceAuthentication -> limitOpacity
//
// 2019 vs 2022: the same fifty-four properties resolved in the same order. No divergence.
//
// Audit status: VERIFIED -- every declaration and every statement diffed against the 2022 snapshot.
// All fifty-four decompiled-to-ported name pairs in the table above were checked mechanically against
// the FindProperty call each one carries in the snapshot; all fifty-four agree, including the three
// limitRotation axes, which come from FindPropertyRelative on limitRotation rather than from the
// serialized object. The fifteen-entry bindings array was compared entry by entry (operand order,
// the -1 lower bound on gravity, radius's infinite upper bound and handle mode 1, the two angle
// ranges, the three explicit "Limit Rotation" labels, maxStretch's infinite bound) and the
// label/index table build below it likewise. The 2019 snapshot was compared property for property in
// order and matches. One correction: the count was stated as fifty-five in two places and is
// fifty-four -- the snapshot's PhysBoneEditor declares exactly fifty-four SerializedProperty statics
// and CacheProperties resolves exactly fifty-four. Line numbers not checked -- located by name.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        private static SerializedProperty version;
        private static SerializedProperty integrationType;
        private static SerializedProperty rootTransform;
        private static SerializedProperty ignoreTransforms;
        private static SerializedProperty endpointPosition;
        private static SerializedProperty multiChildType;

        private static SerializedProperty pull;
        private static SerializedProperty pullCurve;
        private static SerializedProperty spring;
        private static SerializedProperty springCurve;
        private static SerializedProperty stiffness;
        private static SerializedProperty stiffnessCurve;
        private static SerializedProperty immobile;

        /// <summary>
        /// Null on SDK versions that predate the setting; the inspector tests it before drawing the
        /// field rather than assuming it exists.
        /// </summary>
        private static SerializedProperty immobileType;

        private static SerializedProperty immobileCurve;
        private static SerializedProperty gravity;
        private static SerializedProperty gravityCurve;
        private static SerializedProperty gravityFalloff;
        private static SerializedProperty gravityFalloffCurve;

        private static SerializedProperty allowCollision;

        /// <summary>
        /// Present only on SDK versions where <c>allowCollision</c> became a three-state permission
        /// with a companion filter. Its nullness is what the inspector branches on to decide between
        /// drawing a plain toggle and drawing the permission-plus-filter pair.
        /// </summary>
        private static SerializedProperty collisionFilter;

        private static SerializedProperty radius;
        private static SerializedProperty radiusCurve;
        private static SerializedProperty colliders;

        private static SerializedProperty limitType;
        private static SerializedProperty maxAngleX;
        private static SerializedProperty maxAngleXCurve;
        private static SerializedProperty maxAngleZ;
        private static SerializedProperty maxAngleZCurve;

        /// <summary>The Vector3 the three per-axis limit rotations are children of.</summary>
        private static SerializedProperty limitRotation;

        private static SerializedProperty limitRotationX;
        private static SerializedProperty limitRotationY;
        private static SerializedProperty limitRotationZ;
        private static SerializedProperty limitRotationXCurve;
        private static SerializedProperty limitRotationYCurve;
        private static SerializedProperty limitRotationZCurve;

        private static SerializedProperty allowGrabbing;
        private static SerializedProperty allowPosing;
        private static SerializedProperty poseFilter;
        private static SerializedProperty grabFilter;
        private static SerializedProperty grabMovement;
        private static SerializedProperty snapToHand;

        private static SerializedProperty stretchMotion;
        private static SerializedProperty stretchMotionCurve;
        private static SerializedProperty maxStretch;
        private static SerializedProperty maxStretchCurve;
        private static SerializedProperty maxSquish;
        private static SerializedProperty maxSquishCurve;

        private static SerializedProperty isAnimated;
        private static SerializedProperty parameter;
        private static SerializedProperty resetWhenDisabled;

        private static SerializedProperty showGizmos;
        private static SerializedProperty boneOpacity;
        private static SerializedProperty limitOpacity;

        /// <summary>
        /// Every value/curve pair the inspector can draw or edit, in the fixed order the rest of the
        /// editor indexes it by. Index 6 is <c>radius</c> and index 12 is <c>stretchMotion</c>; the
        /// literal indices in the drawing code refer to positions in this array.
        /// </summary>
        internal static PropertyBinding[] bindings;

        /// <summary>
        /// Labels for the property-edit picker, covering only the bindings whose value property
        /// actually resolved.
        /// </summary>
        private static GUIContent[] bindingLabels;

        /// <summary>
        /// Popup values running 0..n-1 alongside <see cref="bindingLabels"/>. These are dense
        /// positions in the filtered label list, not indices into <see cref="bindings"/>;
        /// <see cref="popupValueToBindingIndex"/> converts between them.
        /// </summary>
        private static int[] bindingPopupValues;

        /// <summary>Popup value to index in <see cref="bindings"/>.</summary>
        private static Dictionary<int, int> popupValueToBindingIndex;

        /// <summary>Index in <see cref="bindings"/> to popup value, the inverse of the above.</summary>
        private static Dictionary<int, int> bindingIndexToPopupValue;

        /// <summary>
        /// Set once the picker's label and index tables have been built.
        /// </summary>
        /// <remarks>
        /// The tables depend only on which properties the installed SDK has, which cannot change
        /// without a domain reload, so they survive selection changes. The bindings themselves are
        /// rebuilt every time because they hold live properties of a particular SerializedObject.
        /// </remarks>
        private static bool bindingLabelsBuilt;

        /// <summary>
        /// Resolves every property this inspector draws against the current
        /// <see cref="Editor.serializedObject"/> and rebuilds <see cref="bindings"/> from them.
        /// </summary>
        /// <remarks>
        /// Called at the top of each inspector repaint rather than once on enable, because the
        /// properties are cached in statics shared with the static scene-view handler and a
        /// selection change swaps the SerializedObject out from under them.
        /// <para>
        /// Missing properties are left null instead of being treated as an error: the same inspector
        /// is expected to run against several VRChat SDK releases, and the drawing code checks for
        /// nulls where a property is version-dependent.
        /// </para>
        /// </remarks>
        private void CacheProperties()
        {
            version = serializedObject.FindProperty("version");
            integrationType = serializedObject.FindProperty("integrationType");
            rootTransform = serializedObject.FindProperty("rootTransform");
            ignoreTransforms = serializedObject.FindProperty("ignoreTransforms");
            endpointPosition = serializedObject.FindProperty("endpointPosition");
            multiChildType = serializedObject.FindProperty("multiChildType");

            pull = serializedObject.FindProperty("pull");
            pullCurve = serializedObject.FindProperty("pullCurve");
            spring = serializedObject.FindProperty("spring");
            springCurve = serializedObject.FindProperty("springCurve");
            stiffness = serializedObject.FindProperty("stiffness");
            stiffnessCurve = serializedObject.FindProperty("stiffnessCurve");
            immobile = serializedObject.FindProperty("immobile");
            immobileType = serializedObject.FindProperty("immobileType");
            immobileCurve = serializedObject.FindProperty("immobileCurve");
            gravity = serializedObject.FindProperty("gravity");
            gravityCurve = serializedObject.FindProperty("gravityCurve");
            gravityFalloff = serializedObject.FindProperty("gravityFalloff");
            gravityFalloffCurve = serializedObject.FindProperty("gravityFalloffCurve");

            allowCollision = serializedObject.FindProperty("allowCollision");
            collisionFilter = serializedObject.FindProperty("collisionFilter");
            radius = serializedObject.FindProperty("radius");
            radiusCurve = serializedObject.FindProperty("radiusCurve");
            colliders = serializedObject.FindProperty("colliders");

            limitType = serializedObject.FindProperty("limitType");
            maxAngleX = serializedObject.FindProperty("maxAngleX");
            maxAngleXCurve = serializedObject.FindProperty("maxAngleXCurve");
            maxAngleZ = serializedObject.FindProperty("maxAngleZ");
            maxAngleZCurve = serializedObject.FindProperty("maxAngleZCurve");

            limitRotation = serializedObject.FindProperty("limitRotation");
            limitRotationX = limitRotation.FindPropertyRelative("x");
            limitRotationY = limitRotation.FindPropertyRelative("y");
            limitRotationZ = limitRotation.FindPropertyRelative("z");
            limitRotationXCurve = serializedObject.FindProperty("limitRotationXCurve");
            limitRotationYCurve = serializedObject.FindProperty("limitRotationYCurve");
            limitRotationZCurve = serializedObject.FindProperty("limitRotationZCurve");

            allowGrabbing = serializedObject.FindProperty("allowGrabbing");
            allowPosing = serializedObject.FindProperty("allowPosing");
            poseFilter = serializedObject.FindProperty("poseFilter");
            grabFilter = serializedObject.FindProperty("grabFilter");
            grabMovement = serializedObject.FindProperty("grabMovement");
            snapToHand = serializedObject.FindProperty("snapToHand");

            stretchMotion = serializedObject.FindProperty("stretchMotion");
            stretchMotionCurve = serializedObject.FindProperty("stretchMotionCurve");
            maxStretch = serializedObject.FindProperty("maxStretch");
            maxStretchCurve = serializedObject.FindProperty("maxStretchCurve");
            maxSquish = serializedObject.FindProperty("maxSquish");
            maxSquishCurve = serializedObject.FindProperty("maxSquishCurve");

            isAnimated = serializedObject.FindProperty("isAnimated");
            parameter = serializedObject.FindProperty("parameter");
            resetWhenDisabled = serializedObject.FindProperty("resetWhenDisabled");

            showGizmos = serializedObject.FindProperty("showGizmos");
            boneOpacity = serializedObject.FindProperty("boneOpacity");
            limitOpacity = serializedObject.FindProperty("limitOpacity");

            bindings = new PropertyBinding[15]
            {
                new PropertyBinding(pull, pullCurve),
                new PropertyBinding(spring, springCurve),
                new PropertyBinding(stiffness, stiffnessCurve),
                new PropertyBinding(immobile, immobileCurve),

                // Gravity is signed, so its lower bound is -1 rather than 0. That also puts its
                // scene handle into additive mode; see PropertyBinding.minValue.
                new PropertyBinding(gravity, gravityCurve, -1f),

                new PropertyBinding(gravityFalloff, gravityFalloffCurve),

                // Radius is a world-space distance with no upper bound, and gets the sphere handle.
                new PropertyBinding(radius, radiusCurve, 0f, float.PositiveInfinity, 1),

                new PropertyBinding(maxAngleX, maxAngleXCurve, 0f, 180f),
                new PropertyBinding(maxAngleZ, maxAngleZCurve, 0f, 90f),

                // The three limit rotations are the x/y/z children of a Vector3, whose display names
                // would read as bare axis letters, so they are labelled explicitly.
                new PropertyBinding("Limit Rotation X", limitRotationX, limitRotationXCurve, 0f, 360f),
                new PropertyBinding("Limit Rotation Y", limitRotationY, limitRotationYCurve, 0f, 360f),
                new PropertyBinding("Limit Rotation Z", limitRotationZ, limitRotationZCurve, 0f, 360f),

                new PropertyBinding(stretchMotion, stretchMotionCurve),
                new PropertyBinding(maxStretch, maxStretchCurve, 0f, float.PositiveInfinity),
                new PropertyBinding(maxSquish, maxSquishCurve)
            };

            if (bindingLabelsBuilt)
            {
                return;
            }

            List<GUIContent> labels = new List<GUIContent>();
            popupValueToBindingIndex = new Dictionary<int, int>();
            bindingIndexToPopupValue = new Dictionary<int, int>();

            int popupValue = 0;
            for (int i = 0; i < bindings.Length; i++)
            {
                PropertyBinding binding = bindings[i];
                if (binding.hasValue)
                {
                    labels.Add(new GUIContent(binding.label));
                    popupValueToBindingIndex.Add(popupValue, i);
                    bindingIndexToPopupValue.Add(i, popupValue++);
                }
            }

            bindingLabels = labels.ToArray();
            bindingPopupValues = popupValueToBindingIndex.Keys.ToArray();
            bindingLabelsBuilt = true;
        }
    }
}
