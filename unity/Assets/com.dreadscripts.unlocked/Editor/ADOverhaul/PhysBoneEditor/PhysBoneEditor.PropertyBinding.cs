// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the AlgoAuthentication class nested inside PhysBoneEditor, lines 2360-2402 of the
// current snapshot. Line numbers move with the snapshot; the member names below are the durable
// reference.
//
//   AlgoAuthentication                 -> PropertyBinding,      lines 2360-2402
//   roleAuthentication                 -> label,                line 2362
//   m_VisitorAuthentication            -> valueProperty,        line 2364
//   _InvocationAuthentication          -> curveProperty,        line 2366
//   m_ListenerAuthentication           -> valuePropertyPath,    line 2368
//   m_ParserAuthentication             -> curvePropertyPath,    line 2370
//   printerAuthentication              -> hasCurve,             line 2372
//   repositoryAuthentication           -> minValue,             line 2374
//   _DescriptorAuthentication          -> maxValue,             line 2376
//   m_StrategyAuthentication           -> handleMode,           line 2378
//   _GlobalAuthentication              -> hasValue,             line 2380
//   AlgoAuthentication(SerializedProperty, SerializedProperty, float, float, int)
//                                      -> PropertyBinding(...), line 2382
//   AlgoAuthentication(string, SerializedProperty, SerializedProperty, float, float, int)
//                                      -> PropertyBinding(...), line 2387
//
// The two constructors are kept as a pair rather than collapsed into one with an optional label,
// because the label-less overload derives its label from the value property's displayName, which an
// optional parameter cannot express.
//
// NAME. "AlgoAuthentication" is obfuscator output and, unlike the similarly named remnants removed
// from PhysBoneParameter and ObfuscationMarker, this type is live: PhysBoneEditor builds a
// fifteen-entry table of it in PrintSingleton (line 4115) and every property row, the property-edit
// scene tool and the property picker popup read it. It is ported in full.
//
// 2019 vs 2022: identical apart from obfuscated member names.
//
// Audit status: VERIFIED -- the type diffed member by member against AlgoAuthentication in the 2022
// snapshot: all ten readonly fields with their types, both constructors including the `?.displayName`
// chained call and the default parameter values (0f, 1f, 0), and the second constructor's
// assignments, where hasValue/hasCurve are computed before the paths that depend on them, exactly as
// here. Two shape changes, neither behavioural: the fields are grouped differently (the snapshot
// interleaves the paths between the properties and the bounds; the MAP's line numbers reflect the
// snapshot's order, not this file's), and the type is `sealed` here where the snapshot has a plain
// `internal class` with no subclasses. The per-member line numbers were checked against the snapshot
// and are all exact; the type's range ends at 2400, two lines short of the 2402 recorded above.

using UnityEditor;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class PhysBoneEditor
    {
        /// <summary>
        /// One editable PhysBone property, paired with the falloff curve that scales it along the
        /// bone chain and with the range the scene-view handle for it is allowed to produce.
        /// </summary>
        /// <remarks>
        /// PhysBone settings come in value/curve pairs — <c>pull</c> with <c>pullCurve</c>,
        /// <c>radius</c> with <c>radiusCurve</c>, and so on — and every part of this inspector that
        /// touches one has to touch both. Binding them into a single record is what lets the
        /// property rows, the scene handles and the property picker all be driven from one indexed
        /// table instead of from fifty-odd individually named fields.
        /// <para>
        /// The property paths are captured up front because the scene tools edit PhysBones other
        /// than the inspected one: they open a fresh <see cref="SerializedObject"/> per target and
        /// re-resolve the path against it, so a live <see cref="SerializedProperty"/> bound to the
        /// inspector's own object would be no use to them.
        /// </para>
        /// </remarks>
        internal sealed class PropertyBinding
        {
            /// <summary>
            /// Display name for the property row and for the entry in the property-edit picker.
            /// Usually Unity's own <see cref="SerializedProperty.displayName"/>; supplied explicitly
            /// for the three limit-rotation axes, whose properties are the unhelpfully named
            /// <c>x</c>, <c>y</c> and <c>z</c> children of <c>limitRotation</c>.
            /// </summary>
            internal readonly string label;

            /// <summary>
            /// The scalar property itself, resolved against the inspected
            /// <see cref="SerializedObject"/>. Null for a binding that exists only so the table
            /// stays index-aligned with a PhysBone version that has the property.
            /// </summary>
            internal readonly SerializedProperty valueProperty;

            /// <summary>
            /// The falloff curve applied to <see cref="valueProperty"/> along the bone chain, or
            /// null where the property has none.
            /// </summary>
            internal readonly SerializedProperty curveProperty;

            /// <summary>Whether <see cref="valueProperty"/> resolved against the installed SDK.</summary>
            internal readonly bool hasValue;

            /// <summary>Whether <see cref="curveProperty"/> resolved against the installed SDK.</summary>
            internal readonly bool hasCurve;

            /// <summary>Path of <see cref="valueProperty"/>, or the empty string when it is null.</summary>
            internal readonly string valuePropertyPath;

            /// <summary>Path of <see cref="curveProperty"/>, or the empty string when it is null.</summary>
            internal readonly string curvePropertyPath;

            /// <summary>
            /// Lower bound the scene-view handle clamps edits to. Negative — as it is for
            /// <c>gravity</c>, at -1 — also switches the handle from proportional scaling to
            /// additive offset, since a property that may legitimately pass through zero cannot be
            /// scaled by a ratio.
            /// </summary>
            internal readonly float minValue;

            /// <summary>
            /// Upper bound the scene-view handle clamps edits to, and the value the handle treats as
            /// full-scale when positioning itself. Defaults to 1; the angle bindings raise it to
            /// their degree range and the unbounded ones to positive infinity.
            /// </summary>
            internal readonly float maxValue;

            /// <summary>
            /// Which scene handle draws this binding: 0 for the default dotted-line slider that
            /// stands a marker off each bone, 1 for the sphere handle used by <c>radius</c>, which
            /// is a distance in world space and so has a handle of its own.
            /// </summary>
            internal readonly int handleMode;

            /// <summary>
            /// Binds a property and its curve, labelling the row with the property's own
            /// <see cref="SerializedProperty.displayName"/>.
            /// </summary>
            internal PropertyBinding(SerializedProperty valueProperty, SerializedProperty curveProperty, float minValue = 0f, float maxValue = 1f, int handleMode = 0)
                : this(valueProperty?.displayName, valueProperty, curveProperty, minValue, maxValue, handleMode)
            {
            }

            /// <summary>
            /// Binds a property and its curve under an explicit label, for properties whose own
            /// display name would not read sensibly in the inspector.
            /// </summary>
            internal PropertyBinding(string label, SerializedProperty valueProperty, SerializedProperty curveProperty, float minValue = 0f, float maxValue = 1f, int handleMode = 0)
            {
                this.label = label;
                this.valueProperty = valueProperty;
                this.curveProperty = curveProperty;

                hasValue = valueProperty != null;
                valuePropertyPath = hasValue ? valueProperty.propertyPath : string.Empty;

                hasCurve = curveProperty != null;
                curvePropertyPath = hasCurve ? curveProperty.propertyPath : string.Empty;

                this.minValue = minValue;
                this.maxValue = maxValue;
                this.handleMode = handleMode;
            }
        }
    }
}
