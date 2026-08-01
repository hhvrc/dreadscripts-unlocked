// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// Ported region: the change callbacks passed to the settings field initialisers of the nested
// `EditorSettings` class, lines 1181-1359.
//
// Each of the four is a private static method of the static ControllerEditor class, which is NOT
// yet ported. Rather than stub them -- or drop the callbacks and silently change behaviour -- they
// become assignable seams here, in the manner Editor/Common/Settings/SettingBase.cs already uses
// for its reset button. The ControllerEditor port assigns them; until it does, changing one of the
// settings below persists correctly and simply does not refresh the view it used to refresh.
//
// decompiled member -> ported member, line N:
//   UpdateVisitor()      -> onMatchingOptionsChanged,    12980
//   SortAlgo()           -> onGraphBackgroundChanged,    15852
//   PatchAlgo()          -> onGraphRebuildRequested,     16098
//   PublishAnnotation()  -> onLayerListLayoutChanged,     9116
//   writerVisitor = LayerViewViewType.DefaultView (inside the displayCategoryView callback, line
//                       1277) -> onCategoryViewReset,     8380 (the field)
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.

using System;

namespace DreadScripts.ControllerEditor
{
    internal partial class EditorSettings
    {
        /// <summary>
        /// Raised when the transition condition-matching options are shown or hidden. The window
        /// rebuilds its per-condition multi-editors in response, because which conditions are
        /// grouped together depends on the matching options.
        /// </summary>
        internal static Action onMatchingOptionsChanged;

        /// <summary>
        /// Raised when anything that feeds the Animator window's graph background changes -- the
        /// cosmetic master switch, the texture-vs-colour choice, the texture itself, or the
        /// background colour. The handler rewrites <c>UnityEditor.Graphs.Styles.graphBackground</c>
        /// by reflection, which is why it has to be re-run rather than merely repainted.
        /// </summary>
        internal static Action onGraphBackgroundChanged;

        /// <summary>
        /// Raised when a setting that affects how nodes or transitions are drawn changes, asking the
        /// Animator window to rebuild its graph.
        /// </summary>
        internal static Action onGraphRebuildRequested;

        /// <summary>
        /// Raised when the layer list switches between compact and full rows, so that the
        /// reorderable list can be given its new element height.
        /// </summary>
        internal static Action onLayerListLayoutChanged;

        /// <summary>
        /// Raised when the category view is toggled, to put the layer list back on its default view.
        /// </summary>
        /// <remarks>
        /// Fires on both edges, not just on switching the category view off -- see the remarks on
        /// <see cref="displayCategoryView"/>.
        /// </remarks>
        internal static Action onCategoryViewReset;
    }
}
