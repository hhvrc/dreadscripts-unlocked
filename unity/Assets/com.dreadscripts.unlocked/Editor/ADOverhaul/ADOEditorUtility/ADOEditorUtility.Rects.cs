// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   static ResetProcess     -> DrawPanelBackground,      line 2245
//   static GetProcess       -> DrawRoundedBox,           line 2250
//   static FlushProcess     -> TryGetSurroundingKeys,    line 2274
//   static ExcludeProcess   -> TryEvaluateTangent,       line 2312
//   static InitProcess      -> EvaluateCatmullRom,       line 2328
//   static ConnectProcess   -> EvaluateSegmentTangent,   line 2337
//   static FindProcess      -> TryAddParameter,          line 2350
//   static RunStatus        -> DrawAnchorPicker,         line 2539
//   static SortStatus       -> SliceHorizontal,          line 2737
//   static CustomizeStatus<T> -> CoerceTo<T>,            line 2766
//   static ConcatStatus<T>  -> GetSetFlags<T>,           line 2780
//   static MapStatus<T>     -> ForEach<T>,               line 2790
//   static FillStatus<T>    -> And<T>,                   line 2798
//   static LogoutStatus     -> MapTransforms,            line 2829
//   static SetupStatus<T>   -> MapComponents<T>,         line 2851
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// ── Already ported elsewhere, deliberately NOT repeated here ────────────────────────────────────
//
//   DefineVal  (3837) -> TrimTransparentBorder, ADOEditorUtility.Icons.cs
//   NewVal     (3917) -> TrimmedIcon,           ADOEditorUtility.Icons.cs
//   DestroyVal (3902) -> SolidColorTexture,     ADOEditorUtility.Textures.cs
// All three were on this file's assignment list; all three were already in the package, so they are
// called from here rather than duplicated. (DrawRoundedBox below is the only caller of
// SolidColorTexture in this region.)
//
// DrawRoundedBox is NOT the same method as DreadScripts.Common.EditorGuiUtils.DrawRoundedBox, close
// as the two look. That one is ported from the shared SupportThankies utility and passes
// `alphaBlend: false` to both GUI.DrawTexture calls; ADOverhaul's copy — and ControllerEditor's own
// third copy at EditorUtils.cs line 2371, which is still unported — pass `alphaBlend: true`. With
// the translucent fills these callers use, that is a visible difference, so the ADOverhaul copy is
// ported rather than redirected. The only other difference is cosmetic: EditorGuiUtils skips the
// grown-rect arithmetic when both colours are clear.
//
// ── 2019 vs 2022 ────────────────────────────────────────────────────────────────────────────────
//
// No behavioural divergence anywhere in this region. Counterparts in ADOverhaul2019: ViewAccount
// 2247, SearchAccount 2252, QueryAccount 2276, OrderAccount 2315, EnableAccount 2331, ConcatAccount
// 2340, LogoutAccount 2353, the anchor picker at 2541, UpdateManager 2749, PrepareManager 2778,
// ListManager 2792, ManageManager 2802, ReadManager 2810, VerifyManager 2841, ConnectManager 2864.
// Every one is statement-for-statement identical; the differences are all ILSpy's:
//   - TryGetSurroundingKeys decompiles as a `while (true)` with a `break` in 2022 and as a
//     `while (true)` with an early `return` in 2019. The 2019 rendering is the coherent one and is
//     what is written below; the 2022 form reaches the same states by a longer route.
//   - The two builds pick opposite polarities for the branches in the anchor picker, the slicer's
//     nested ternary and both hierarchy mappers. Same booleans.
//
// ── Shipped bugs preserved ──────────────────────────────────────────────────────────────────────
//
// Three, each documented at its member: the degrees/radians confusion in EvaluateSegmentTangent,
// the unnormalised curve parameter it passes to EvaluateCatmullRom, and the unguarded index in
// MapComponents. See the remarks there.

using System;
using System.Collections.Generic;
using System.Linq;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        // ── Rect drawing ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Draws the tool's standard dark panel background behind <paramref name="rect"/> and returns
        /// the area safe to put content in.
        /// </summary>
        /// <remarks>
        /// The two colours are the shipped constants for every boxed section ADOverhaul draws: a
        /// near-black translucent fill under a slightly lighter translucent border. They are
        /// translucent so the panel reads as an overlay on whatever it covers — an inspector, or the
        /// scene view.
        /// </remarks>
        internal static Rect DrawPanelBackground(Rect rect, float borderWidth = 2f)
        {
            return DrawRoundedBox(rect, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), borderWidth);
        }

        /// <summary>
        /// Draws a rounded fill and/or a rounded border around <paramref name="rect"/>, and returns
        /// the rect inset by 4px on every side, i.e. the area safe to put content in.
        /// </summary>
        /// <param name="fillColor"><see cref="Color.clear"/> skips the fill pass.</param>
        /// <param name="borderColor"><see cref="Color.clear"/> skips the border pass.</param>
        /// <remarks>
        /// <para>
        /// Both passes go through the eight-argument
        /// <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, float, float)"/>,
        /// which is the only GUI entry point that knows how to round corners. The texture itself is
        /// irrelevant — a 1x1 white pixel tinted wholesale by the colour argument — so the shared,
        /// mutated-in-place <see cref="SolidColorTexture"/> is safe to use for both.
        /// </para>
        /// <para>
        /// The border is drawn on a rect grown by its own width plus 2, and offset by half of that,
        /// so it surrounds the fill rather than overprinting it. The returned inset is a fixed 4px
        /// and does not track <paramref name="borderWidth"/>; a caller asking for a border much
        /// thicker than that will have its content drawn over.
        /// </para>
        /// </remarks>
        internal static Rect DrawRoundedBox(Rect rect, Color fillColor, Color borderColor, float borderWidth = 3f)
        {
            float growth = borderWidth + 2f;

            Rect borderRect = rect;
            borderRect.x -= growth / 2f;
            borderRect.width += growth;
            borderRect.y -= growth / 2f;
            borderRect.height += growth;

            if (fillColor != Color.clear)
            {
                GUI.DrawTexture(rect, SolidColorTexture(fillColor), ScaleMode.StretchToFill, alphaBlend: true, 0f, fillColor, 0f, 8f);
            }

            if (borderColor != Color.clear)
            {
                GUI.DrawTexture(borderRect, SolidColorTexture(borderColor), ScaleMode.StretchToFill, alphaBlend: true, 0f, borderColor, borderWidth, 8f);
            }

            Rect content = rect;
            content.x += 4f;
            content.width -= 8f;
            content.y += 4f;
            content.height -= 8f;
            return content;
        }

        /// <summary>
        /// Draws a 3x3 grid of cells over <paramref name="rect"/>, one per single-bit
        /// <see cref="PositionFlag"/>, and returns the cell the mouse is over — or
        /// <paramref name="current"/> unchanged if it is over none.
        /// </summary>
        /// <param name="current">The currently chosen anchor, returned unchanged when nothing is hovered.</param>
        /// <param name="rect">The area the grid fills. Each cell is a third of it in each axis.</param>
        /// <param name="selectable">
        /// Which anchors may be chosen. Cells outside this mask are tinted red and never returned.
        /// </param>
        /// <remarks>
        /// <para>
        /// The nine cells come from enumerating <see cref="PositionFlag"/> and discarding anything
        /// that is not exactly one bit — which drops <see cref="PositionFlag.All"/> (-1) and would
        /// drop any composite value added later. <see cref="PositionFlag.Middle"/> survives and lands
        /// in the centre cell, because it satisfies neither the left/right nor the top/bottom
        /// predicates and so takes the "+1/3" branch in both axes.
        /// </para>
        /// <para>
        /// SHIPPED QUIRK, preserved: the hover test — and therefore the return value — is inside an
        /// <c>Event.current.type == EventType.Repaint</c> guard, so this reports a new anchor only on
        /// repaint frames. A caller polling it on a MouseDown gets the previous frame's answer. It
        /// works in practice because the grid is a hover-to-choose control that is repainted
        /// continuously while open, but it is why the choice cannot be read from the click event
        /// directly.
        /// </para>
        /// <para>
        /// Each cell is outlined by a grey border drawn on a 1.5px-inset copy of its rect, then
        /// filled: red when unselectable, a faint grey when idle, green when hovered.
        /// </para>
        /// </remarks>
        internal static PositionFlag DrawAnchorPicker(PositionFlag current, Rect rect, PositionFlag selectable = PositionFlag.All)
        {
            AddCursorRect(rect, MouseCursor.Pan);

            float cellWidth = rect.width / 3f;
            float cellHeight = rect.height / 3f;

            foreach (PositionFlag position in PositionFlag.All.GetSetFlags())
            {
                // Single bits only: zero and any composite (All, or a hand-built union) are not cells.
                if (position == 0 || (position & (position - 1)) != 0)
                {
                    continue;
                }

                Rect cell = rect;

                if (position.IsAnchoredRight())
                {
                    cell.x += cellWidth * 2f;
                }
                else if (!position.IsAnchoredLeft())
                {
                    cell.x += cellWidth;
                }

                if (position.IsAnchoredBottom())
                {
                    cell.y += cellHeight * 2f;
                }
                else if (!position.IsAnchoredTop())
                {
                    cell.y += cellHeight;
                }

                cell.width = cellWidth;
                cell.height = cellHeight;

                const float outlineInset = 1.5f;
                Rect outline = cell;
                outline.x += outlineInset;
                outline.y += outlineInset;
                outline.width -= outlineInset * 2f;
                outline.height -= outlineInset * 2f;
                DrawRoundedBox(outline, Color.clear, Color.grey);

                if (!selectable.HasFlag(position))
                {
                    DrawRoundedBox(cell, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
                }
                else if (Event.current.type == EventType.Repaint)
                {
                    if (!cell.Contains(Event.current.mousePosition))
                    {
                        DrawRoundedBox(cell, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
                        continue;
                    }

                    current = position;
                    DrawRoundedBox(cell, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
                }
            }

            return current;
        }

        /// <summary>
        /// Takes a column off the left of <paramref name="rect"/> and, by default, advances
        /// <paramref name="rect"/> past it — so a row of controls can be laid out by calling this
        /// once per control.
        /// </summary>
        /// <param name="rect">The remaining row. Narrowed in place unless <paramref name="consume"/> is false.</param>
        /// <param name="width">
        /// The column's width, as a percentage of the row's current width unless
        /// <paramref name="absoluteWidth"/> is set.
        /// </param>
        /// <param name="absoluteWidth">Treat <paramref name="width"/> as pixels rather than a percentage.</param>
        /// <param name="x">
        /// Where the column starts. -1, the default, means "where the row currently starts".
        /// Otherwise it is a percentage of the row's width from the row's left edge, unless
        /// <paramref name="absoluteX"/> is set.
        /// </param>
        /// <param name="absoluteX">Treat <paramref name="x"/> as an absolute GUI-space coordinate rather than a percentage offset.</param>
        /// <param name="consume">
        /// Whether to narrow <paramref name="rect"/>. False gives an overlay rect — useful for
        /// drawing something on top of the row without disturbing the layout.
        /// </param>
        /// <remarks>
        /// Percentages are always of the row's <em>current</em> width, which shrinks with every
        /// consuming call, so "50, 50" leaves a quarter of the original row rather than nothing.
        /// The sentinel is a literal -1 comparison, so an explicit x of exactly -1 means "row start"
        /// and cannot be expressed as a coordinate.
        /// </remarks>
        internal static Rect SliceHorizontal(this ref Rect rect, float width, bool absoluteWidth = false, float x = -1f, bool absoluteX = false, bool consume = true)
        {
            Rect slice = rect;
            slice.width = absoluteWidth ? width : width * rect.width / 100f;
            slice.height = rect.height;
            slice.x = x == -1f ? rect.x : (absoluteX ? x : rect.x + x * rect.width / 100f);
            slice.y = rect.y;

            if (consume)
            {
                rect.x = slice.x + slice.width;
                rect.width -= slice.width;
            }

            return slice;
        }

        // ── AnimationCurve maths ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Finds the pair of keyframes that bracket <paramref name="time"/>.
        /// </summary>
        /// <param name="before">The last key at or before <paramref name="time"/>.</param>
        /// <param name="after">The first key at or after <paramref name="time"/>. Equal to <paramref name="before"/> when a key sits exactly on <paramref name="time"/>.</param>
        /// <returns>
        /// False when the curve cannot bracket the time at all — it is empty, it has a single key,
        /// or every key is before <paramref name="time"/>. The out parameters are then not a usable
        /// pair, though <paramref name="before"/> may still hold the nearest key.
        /// </returns>
        /// <remarks>
        /// <para>
        /// A linear scan rather than a binary search: the curves this is used on are the handful of
        /// keys in a PhysBone falloff, so the scan is shorter than the setup for anything cleverer.
        /// </para>
        /// <para>
        /// EDGE CASE, preserved: when <paramref name="time"/> falls before the first key, the loop
        /// takes the "at or after" branch immediately and returns true with
        /// <paramref name="before"/> still <c>default(Keyframe)</c> — that is, a key at time 0 with
        /// value 0 and zero tangents, which is not a key of this curve. Callers extrapolating from
        /// the pair will get a wrong slope there.
        /// </para>
        /// </remarks>
        internal static bool TryGetSurroundingKeys(this AnimationCurve curve, float time, out Keyframe before, out Keyframe after)
        {
            before = default(Keyframe);
            after = default(Keyframe);

            if (curve.length == 0)
            {
                return false;
            }

            if (curve.length == 1)
            {
                // Reported through `before` even though this returns false, so a caller that wants
                // the nearest key rather than a bracketing pair can still read it.
                before = curve[0];
                return false;
            }

            for (int i = 0; i < curve.length; i++)
            {
                Keyframe key = curve[i];

                if (key.time == time)
                {
                    before = after = key;
                    return true;
                }

                if (key.time >= time)
                {
                    after = key;
                    return true;
                }

                before = key;
            }

            return false;
        }

        /// <summary>
        /// The slope of <paramref name="curve"/> at <paramref name="time"/>, which
        /// <see cref="AnimationCurve"/> does not expose.
        /// </summary>
        /// <returns>False when the curve has no keys bracketing <paramref name="time"/>.</returns>
        /// <remarks>
        /// A key sitting exactly on <paramref name="time"/> short-circuits to its own out tangent,
        /// which is both cheaper and — at a broken-tangent key, where the true slope is
        /// discontinuous — the answer the caller means: the slope going forward.
        /// </remarks>
        internal static bool TryEvaluateTangent(this AnimationCurve curve, float time, out float tangent)
        {
            tangent = 0f;

            if (!curve.TryGetSurroundingKeys(time, out Keyframe before, out Keyframe after))
            {
                return false;
            }

            if (before.time != after.time)
            {
                tangent = EvaluateSegmentTangent(before, after, time);
                return true;
            }

            tangent = before.outTangent;
            return true;
        }

        /// <summary>
        /// The Catmull-Rom spline through <paramref name="p1"/> and <paramref name="p2"/>, evaluated
        /// at <paramref name="t"/>, with <paramref name="p0"/> and <paramref name="p3"/> as the
        /// neighbouring control points that set the end slopes.
        /// </summary>
        /// <param name="t">Position along the segment, 0 at <paramref name="p1"/> and 1 at <paramref name="p2"/>.</param>
        internal static float EvaluateCatmullRom(float p0, float p1, float p2, float p3, float t)
        {
            float a = 2f * p1;
            float b = p2 - p0;
            float c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            float d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + b * t + c * t * t + d * t * t * t);
        }

        /// <summary>
        /// Approximates the slope of the curve segment running from <paramref name="from"/> to
        /// <paramref name="to"/>, at <paramref name="time"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The segment is modelled as a Catmull-Rom spline whose two outer control points are
        /// synthesised by extrapolating backwards along each key's tangent, and the slope is then
        /// taken numerically as a forward difference over a 1e-5 step. A closed form exists, but the
        /// numerical difference is what shipped.
        /// </para>
        /// <para>
        /// SHIPPED BUGS, both preserved — this method's result should be treated as decorative, and
        /// it is: its only path to the screen is a handle-drawing hint.
        /// </para>
        /// <list type="number">
        /// <item><description>
        /// The tangents are converted to degrees (<c>Mathf.Atan</c> times 57.29578) and then fed
        /// straight back into <see cref="Mathf.Tan"/>, which takes radians — with a further
        /// <c>+ 180f</c> that is presumably meant as "half a turn" but is 180 <em>radians</em> here.
        /// The round trip is therefore not the identity it looks like, and the control points it
        /// produces bear no useful relation to the tangents.
        /// </description></item>
        /// <item><description>
        /// <paramref name="time"/> is passed to <see cref="EvaluateCatmullRom"/> as its
        /// <c>t</c> parameter directly. That parameter is normalised — 0 at
        /// <paramref name="from"/>, 1 at <paramref name="to"/> — but <paramref name="time"/> is an
        /// absolute curve time. The division by the segment duration is missing; note that the
        /// duration <em>is</em> computed, and is then used only for the extrapolation above.
        /// </description></item>
        /// </list>
        /// </remarks>
        internal static float EvaluateSegmentTangent(Keyframe from, Keyframe to, float time)
        {
            float duration = to.time - from.time;

            float outAngle = Mathf.Rad2Deg * Mathf.Atan(from.outTangent);
            float inAngle = Mathf.Rad2Deg * Mathf.Atan(to.inTangent);

            float p1 = from.value;
            float p2 = to.value;
            float p0 = from.value + Mathf.Tan(outAngle + 180f) * duration;
            float p3 = to.value + Mathf.Tan(inAngle + 180f) * duration;

            const float step = 1E-05f;
            float value = EvaluateCatmullRom(p0, p1, p2, p3, time);
            return (EvaluateCatmullRom(p0, p1, p2, p3, time + step) - value) / step;
        }

        // ── Animator ────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a parameter to <paramref name="controller"/> unless one of that name already exists.
        /// </summary>
        /// <param name="defaultValue">
        /// Written to all three default slots — as a float, as an int by truncation, and as a bool by
        /// "not zero" — so one argument serves whichever <paramref name="type"/> is being created.
        /// </param>
        /// <returns>True if the parameter was added, false if the name was already taken.</returns>
        /// <remarks>
        /// The existing parameter's type is not checked, only its name. A controller that already has
        /// a float called "Foo" will not gain a bool called "Foo", and the caller is told nothing
        /// beyond "not added".
        /// </remarks>
        internal static bool TryAddParameter(this AnimatorController controller, string name, AnimatorControllerParameterType type, float defaultValue)
        {
            bool isNew = controller.parameters.All(p => p.name != name);

            if (isNew)
            {
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = name,
                    type = type,
                    defaultBool = defaultValue != 0f,
                    defaultInt = (int)defaultValue,
                    defaultFloat = defaultValue
                });
            }

            return isNew;
        }

        // ── Small generic helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Reinterprets <paramref name="obj"/> as <typeparamref name="T"/>, reaching through a
        /// <see cref="GameObject"/> to one of its components when <typeparamref name="T"/> is a
        /// component type.
        /// </summary>
        /// <remarks>
        /// This is what lets a drag-and-drop target or an object picker accept a whole GameObject
        /// where a specific component is wanted, which is what a user dragging from the hierarchy
        /// always means.
        /// <para>
        /// Note the test is <see cref="Type.IsSubclassOf"/>, so <typeparamref name="T"/> of exactly
        /// <see cref="Component"/> takes the plain-cast path and a GameObject passed for it yields
        /// null rather than an arbitrary component. That is a reasonable answer, and it is what
        /// shipped.
        /// </para>
        /// </remarks>
        internal static T CoerceTo<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
        {
            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                GameObject gameObject = obj as GameObject;
                if (gameObject != null)
                {
                    return gameObject.GetComponent<T>();
                }

                // Not a GameObject, so it is either already the component or unrelated; either way
                // the plain cast below would be the same answer, but the shipped code returns here.
                return null;
            }

            return obj as T;
        }

        /// <summary>
        /// The declared values of <typeparamref name="T"/> that are set in <paramref name="flags"/>.
        /// </summary>
        /// <remarks>
        /// Every declared value is tested, including zero (which <see cref="Enum.HasFlag"/> reports
        /// as always set) and any composite or all-bits value the enum declares. Callers that want
        /// only the individual bits have to filter, as <see cref="DrawAnchorPicker"/> does.
        /// </remarks>
        internal static IEnumerable<T> GetSetFlags<T>(this T flags) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(value => flags.HasFlag(value));
        }

        /// <summary>Applies <paramref name="action"/> to every element, for use at the end of a LINQ chain.</summary>
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Combines two predicates into one that requires both, so a caller holding a filter can
        /// narrow it without knowing what it already tests.
        /// </summary>
        /// <remarks>Short-circuiting: <paramref name="second"/> is not called when <paramref name="first"/> is false.</remarks>
        public static Func<T, bool> And<T>(this Func<T, bool> first, Func<T, bool> second)
        {
            return arg => first(arg) && second(arg);
        }

        // ── Hierarchy mapping ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps each of <paramref name="transforms"/> onto the transform at the same relative path
        /// under <paramref name="targetRoot"/>.
        /// </summary>
        /// <param name="sourceRoot">The hierarchy the given transforms live under. Paths are relative to it.</param>
        /// <param name="targetRoot">The hierarchy to look the same paths up in.</param>
        /// <param name="skipUnmatched">
        /// Whether to omit entries that could not be matched. False includes them with a null value,
        /// so the caller can report exactly which ones were missing.
        /// </param>
        /// <remarks>
        /// <para>
        /// This is how any operation that copies settings between two avatars finds its counterparts:
        /// path equality is the only correspondence available, since the two hierarchies share no
        /// object identity.
        /// </para>
        /// <para>
        /// Two things follow from the plain <see cref="Dictionary{TKey,TValue}.Add"/>: a repeated
        /// transform in <paramref name="transforms"/> throws, and so nothing deduplicates for the
        /// caller. And <paramref name="sourceRoot"/> itself, if passed, has an empty relative path,
        /// which <see cref="Transform.Find(string)"/> answers with null — so the root maps to nothing
        /// rather than to <paramref name="targetRoot"/>.
        /// </para>
        /// </remarks>
        internal static Dictionary<Transform, Transform> MapTransforms(Transform sourceRoot, Transform targetRoot, bool skipUnmatched, params Transform[] transforms)
        {
            Dictionary<Transform, Transform> map = new Dictionary<Transform, Transform>();

            foreach (Transform transform in transforms)
            {
                if (!transform.IsChildOf(sourceRoot))
                {
                    if (!skipUnmatched)
                    {
                        map.Add(transform, null);
                    }

                    continue;
                }

                string path = AnimationUtility.CalculateTransformPath(transform, sourceRoot);
                Transform match = targetRoot.Find(path);

                if (!(match == null && skipUnmatched))
                {
                    map.Add(transform, match);
                }
            }

            return map;
        }

        /// <summary>
        /// Maps each of <paramref name="components"/> onto the component at the same relative path
        /// under <paramref name="targetRoot"/> and in the same ordinal position on that object.
        /// </summary>
        /// <param name="skipUnmatched">
        /// Whether to omit entries whose transform could not be matched. False includes them with a
        /// null value.
        /// </param>
        /// <remarks>
        /// <para>
        /// The component version of <see cref="MapTransforms"/>, with one extra step: a GameObject
        /// may carry several components of the same type, and those are told apart by their index in
        /// <see cref="GameObject.GetComponents{T}"/> order — the order they appear in the inspector.
        /// The nth <typeparamref name="T"/> on the source object maps to the nth on the target.
        /// </para>
        /// <para>
        /// SHIPPED BUG, preserved. When the matched object has fewer components of the type than the
        /// source does, the index is out of range. The guard for it —
        /// <c>index &gt;= targetComponents.Length &amp;&amp; skipUnmatched</c> — only skips the entry
        /// when <paramref name="skipUnmatched"/> is set; with it clear the code falls through and
        /// indexes anyway, throwing <see cref="IndexOutOfRangeException"/> where it evidently meant
        /// to add a null entry. Callers that pass false must therefore already know the two
        /// hierarchies agree on component counts.
        /// </para>
        /// </remarks>
        internal static Dictionary<T, T> MapComponents<T>(Transform sourceRoot, Transform targetRoot, bool skipUnmatched, params T[] components) where T : Component
        {
            Dictionary<T, T> map = new Dictionary<T, T>();

            foreach (T component in components)
            {
                if (!component.transform.IsChildOf(sourceRoot))
                {
                    if (!skipUnmatched)
                    {
                        map.Add(component, null);
                    }

                    continue;
                }

                string path = AnimationUtility.CalculateTransformPath(component.transform, sourceRoot);
                Transform match = targetRoot.Find(path);

                if (match == null)
                {
                    if (!skipUnmatched)
                    {
                        map.Add(component, null);
                    }

                    continue;
                }

                T[] sourceComponents = component.GetComponents<T>();
                T[] targetComponents = match.GetComponents<T>();
                int index = Array.IndexOf(sourceComponents, component);

                // See the remarks: this guard is one condition short of covering the overflow.
                if (!(index >= targetComponents.Length && skipUnmatched))
                {
                    map.Add(component, targetComponents[index]);
                }
            }

            return map;
        }
    }
}
