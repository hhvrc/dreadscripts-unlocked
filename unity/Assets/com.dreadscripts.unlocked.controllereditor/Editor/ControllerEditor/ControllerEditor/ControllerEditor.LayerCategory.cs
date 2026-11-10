// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//
// Ported region: the layer-category tree -- the machinery that turns the current controller's flat
// layer array into the folder tree the "Category" layer views draw, and the ReorderableList that
// draws one category's worth of it. This is what fires whenever the edited controller changes, and
// it is the reason ActiveController's setter could not be ported before now.
//
//   DisableMapper        -> RebuildLayerCategories,     line 16776
//   InsertMapper         -> RefreshCategoryNames,       line 16852
//   AddMapper            -> RebuildCategoryLayerList,   line 16928
//   InvokeMapper         -> DrawCategoryLayerElement,   line 16961
//   FindMapper           -> OnCategoryLayerSelected,    line 16973
//   ExcludeMapper        -> OnCategoryLayerMouseUp,     line 16978
//   InitMapper           -> ForwardLayerSelection,      line 16983
//   StartMapper          -> GetSelectedLayerIndex,      line 17004
//   ReadMapper           -> GetCategoryTags,            line 17021
//   ValidateInitializer  -> BaseCategoryName,           line 16113
//   CloneInitializer     -> LayerListElementHeight,     line 16137
//   PushInitializer      -> CategoryDelimiter,          line 16159
//   ReadAnnotation       -> GetLayerControllerView,     line 9673
//   _003C_003Ec__DisplayClass616_0 -> dissolved into RebuildCategoryLayerList, lines 7875-7890
//     m_IteratorReg -> the local `view`
//     _PublisherReg -> the local `selectedLayerName`
//     ReflectThread -> the local function `Bind<T>`
//     DeleteThread  -> the lambda passed to List.FindIndex
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// ======================================== NOTES ================================================
//
// SHAPE. ValidateInitializer, CloneInitializer and PushInitializer carry [SpecialName] in the
// decompiled output, with no arguments and no side effects -- ILSpy's rendering of a property getter
// whose `get_` prefix the deobfuscator has already stripped, the same artifact LayerPathNode.cs
// documents for CategoryPath. They are restored as properties here. ReadAnnotation, by contrast,
// carries no [SpecialName] and does create an instance when it finds none, so it stays a method.
//
// WHY THESE THREE SETTINGS ACCESSORS LIVE HERE AND NOT ON LayerPathNode. LayerPathNode.cs took the
// delimiter and the base-category name as constructor arguments rather than reading the settings on
// every call, because EditorSettings was unported when it landed. That decision stands, and this
// file is what supplies those two values at the one place a root node is constructed
// (`new LayerPathNode("Root", "Root")`, line 16791) -- so the two ports meet exactly where that
// file's DELIBERATE DEVIATION section said they would.
//
// GetLayerControllerView is not the `layerControllerView` field declared in ControllerEditor.State.cs
// (decompiled line 8350). That field is the instance a Harmony patch captures as it draws; this
// method reaches the Animator window's own `m_LayerEditor`, creating one if the window has not built
// it yet. Both are LayerControllerView, but only one of them is guaranteed non-null here.
//
// ===================================== SHIPPED BUG =============================================
//
// 1. REBUILDING WITH NO CONTROLLER THROWS. RebuildLayerCategories calls RefreshCategoryNames as its
//    very first statement, outside the try, and RefreshCategoryNames opens with
//    `ActiveController.layers` with no null test -- while the guard that does test for a missing
//    controller sits three statements later, inside the try. So a rebuild triggered while no
//    controller is loaded raises a NullReferenceException out of the caller rather than returning
//    quietly. That is reachable through the ActiveController setter: closing the Animator window
//    makes the graph report a null controller, which is a change from the previous non-null value,
//    which fires this rebuild. Transcribed as shipped -- moving the guard would be a behaviour
//    change, and the vendor's own ordering is what the rest of the tool was tested against.
//
// 2. THE FORWARDED DRAW CALL SWAPS isActive AND isFocused. DrawCategoryLayerElement receives
//    ReorderableList's `(rect, index, isActive, isFocused)` and forwards to Unity's own OnDrawLayer
//    as `(rect, layerIndex, isFocused, isActive)` -- the third and fourth arguments crossed. The
//    decompiled call at line 16968 passes the fourth parameter into the third position and the third
//    into the fourth; there is no overload that would make that a different method. The visible
//    effect is that a category-view row draws its "active" decoration when it has keyboard focus and
//    vice versa. Reproduced rather than corrected.
//
// 3. GrabKeyboardFocus RUNS ON A LIST THAT MAY NOT EXIST. The last statement of
//    RebuildLayerCategories dereferences categoryLayerList unconditionally, but
//    RebuildCategoryLayerList -- the only thing that assigns it -- returns without doing so when the
//    Animator window is closed. On the very first rebuild after a domain reload with no Animator
//    window open, that is a second NullReferenceException, this one after the try block has already
//    swallowed nothing. Note that the early `return`s inside the try skip this statement entirely,
//    since there is no finally; only the completing and the caught-exception paths reach it.
//
// ================================ DELIBERATE DEVIATION ========================================
//
// The layer-category tree does not draw itself from here. RestartMapper (line 16863), the breadcrumb
// strip, and the rest of the category-view GUI are a separate region and are not ported; this file
// stops at the data structure and the ReorderableList that region would draw. Nothing here calls
// into that region, so there is no half-drawn GUI: what is missing is missing entirely.
//
// Audit status: VERIFIED -- every statement in this file was transcribed from decompiled lines
// 16776-16861, 16928-17032, 16113-16116, 16136-16140, 16158-16162 and 9673-9686, and each line
// number in the table above was confirmed to land on the named member in the current snapshot.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal partial class ControllerEditor : EditorWindow
    {
        #region Category settings

        /// <summary>
        /// The configured separator between category names, from the tool's settings. Default "/",
        /// so a layer called <c>Locomotion/Idle</c> files itself under <c>Locomotion</c>.
        /// </summary>
        private static string CategoryDelimiter
        {
            get
            {
                return EditorSettings.Instance.categoryDelimiter;
            }
        }

        /// <summary>
        /// The configured display name of the bucket that holds a category's own layers, as opposed
        /// to those of its sub-categories. Default "Base".
        /// </summary>
        private static string BaseCategoryName
        {
            get
            {
                return EditorSettings.Instance.categoryBaseName;
            }
        }

        /// <summary>
        /// Row height of the category layer list, halved when the compact-view setting is on.
        /// </summary>
        private static float LayerListElementHeight
        {
            get
            {
                return EditorSettings.Instance.layerCompactView ? 20 : 40;
            }
        }

        #endregion

        #region Category tree

        /// <summary>
        /// The category tags written on a layer by this tool -- its system tags of the form
        /// <c>_category:Name</c>, with the prefix stripped.
        /// </summary>
        /// <remarks>
        /// A layer may carry several, which is why the tag view can file one layer under more than
        /// one category. A layer carrying none is filed under <see cref="BaseCategoryName"/>.
        /// </remarks>
        private static IEnumerable<string> GetCategoryTags(UnityEditor.Animations.AnimatorControllerLayer layer)
        {
            foreach (string tag in layer.GetSystemTags())
            {
                Match match = Regex.Match(tag, "^_category:(.+)$");
                if (match.Success)
                {
                    yield return match.Groups[1].Value;
                }
            }
        }

        /// <summary>
        /// Recomputes <see cref="categoryNames"/> -- every distinct category tag in use on the
        /// current controller, sorted -- so the "new category" field can offer completions.
        /// </summary>
        /// <remarks>
        /// Throws if no controller is loaded; see SHIPPED BUG 1 in the file header.
        /// </remarks>
        private static void RefreshCategoryNames()
        {
            UnityEditor.Animations.AnimatorControllerLayer[] layers = ActiveController.layers;
            HashSet<string> names = new HashSet<string>();
            foreach (string tag in layers.SelectMany(GetCategoryTags))
            {
                names.Add(tag);
            }

            categoryNames = names.OrderBy(n => n).ToArray();
        }

        /// <summary>
        /// Rebuilds the whole layer-category tree from the current controller's layers, keeping the
        /// open category and the selected layer where it can, and rebuilds the list that draws them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the change notification behind <see cref="ActiveController"/>'s setter, and it is
        /// also what the layer add/remove/reorder patches call. Everything the category views draw is
        /// derived state: nothing incremental is maintained, the tree is simply thrown away and built
        /// again from <c>layers</c>.
        /// </para>
        /// <para>
        /// The default view has no tree, so the method does its category-name refresh and returns.
        /// The refresh happens either way because the tag completions are offered from the layer
        /// context menu, which the default view still has.
        /// </para>
        /// <para>
        /// The whole rebuild is wrapped in a catch-and-log rather than being allowed to propagate:
        /// it runs from inside Unity's own layer-view GUI through a Harmony patch, and an exception
        /// escaping there would take the Animator window's repaint with it.
        /// </para>
        /// </remarks>
        private static void RebuildLayerCategories()
        {
            RefreshCategoryNames();
            if (layerViewType == LayerViewViewType.DefaultView)
            {
                return;
            }

            try
            {
                if (ActiveController == null)
                {
                    return;
                }

                UnityEditor.Animations.AnimatorControllerLayer[] layers = ActiveController.layers;
                string openCategoryPath = currentLayerCategory?.CategoryPath;
                layerCategoryRoot = new LayerPathNode("Root", "Root", CategoryDelimiter, BaseCategoryName);

                if (layerViewType == LayerViewViewType.CategoryByName)
                {
                    for (int i = 0; i < layers.Length; i++)
                    {
                        UnityEditor.Animations.AnimatorControllerLayer layer = layers[i];
                        layerCategoryRoot.AddLayer(layer.name, layer, i);
                    }
                }
                else if (layerViewType == LayerViewViewType.CategoryByTag)
                {
                    for (int i = 0; i < layers.Length; i++)
                    {
                        UnityEditor.Animations.AnimatorControllerLayer layer = layers[i];
                        string[] tags = GetCategoryTags(layer).ToArray();
                        if (tags.Any())
                        {
                            foreach (string tag in tags)
                            {
                                // The last segment of a path names the layer rather than a category,
                                // so the tag needs a throwaway segment after it to become a folder.
                                layerCategoryRoot.AddLayer(tag + CategoryDelimiter + "DUMMY", layer, i);
                            }
                        }
                        else
                        {
                            layerCategoryRoot.AddLayer(BaseCategoryName, layer, i);
                        }
                    }
                }

                Stack<LayerPathNode> pending = new Stack<LayerPathNode>();
                pending.Push(layerCategoryRoot);
                while (pending.Count > 0)
                {
                    LayerPathNode node = pending.Pop();
                    foreach (LayerPathNode child in node.children)
                    {
                        pending.Push(child);
                    }

                    node.children.Sort((c1, c2) => string.Compare(c1.name, c2.name, StringComparison.Ordinal));
                    if (EditorSettings.Instance.sortCategoryViewLayers)
                    {
                        node.layers.Sort((l1, l2) => string.Compare(l1.layer.name, l2.layer.name, StringComparison.Ordinal));
                    }

                    // The base category is sorted like any other child and then moved to the end, so
                    // a category's own layers always read below its sub-categories.
                    LayerPathNode baseCategory = node.baseCategoryNode;
                    if (baseCategory != null)
                    {
                        node.children.Remove(baseCategory);
                        node.children.Add(baseCategory);
                    }
                }

                currentLayerCategory = openCategoryPath.IsNullOrEmpty()
                    ? layerCategoryRoot
                    : layerCategoryRoot.FindNode(openCategoryPath) ?? layerCategoryRoot;
                RebuildCategoryLayerList();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            categoryLayerList.GrabKeyboardFocus();
        }

        #endregion

        #region Category layer list

        /// <summary>
        /// The Animator window's own <c>LayerControllerView</c>, created on the window if it has not
        /// built one yet, or null when there is no Animator window at all.
        /// </summary>
        /// <remarks>
        /// The tool reaches into this object for the stock row drawing, the rename overlay and the
        /// selected-layer index, so it needs one to exist even before Unity would have made it.
        /// </remarks>
        private static object GetLayerControllerView()
        {
            EditorWindow tool = AnimatorGraphReflection.GraphAccessors.Tool;
            if (tool == null)
            {
                return null;
            }

            object view = layerEditorField.GetValue(tool);
            if (view == null)
            {
                layerEditorField.SetValue(tool, view = Activator.CreateInstance(layerControllerViewType));
            }

            return view;
        }

        /// <summary>
        /// Rebuilds <see cref="categoryLayerList"/> over the open category's layers, re-binding the
        /// three stock callbacks to the live layer view and restoring the selection by layer name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The list is replaced rather than repopulated because <see cref="ReorderableList"/> binds
        /// to the <c>IList</c> it was constructed with, and the open category's list is a different
        /// object after every rebuild.
        /// </para>
        /// <para>
        /// The selection is carried across by name rather than by index: a rebuild can reorder the
        /// category, and an index would silently come back pointing at a different layer.
        /// </para>
        /// </remarks>
        private static void RebuildCategoryLayerList()
        {
            object view = GetLayerControllerView();
            if (view == null)
            {
                return;
            }

            string selectedLayerName = categoryLayerList != null && categoryLayerList.HasSelection()
                ? ((LayerIndexEntry)categoryLayerList.list[categoryLayerList.index]).layer.name
                : "";

            T Bind<T>(string methodName) where T : Delegate
            {
                return (T)Delegate.CreateDelegate(typeof(T), view, layerControllerViewType.GetAnyMethod(methodName));
            }

            drawLayerCallback = Bind<ReorderableList.ElementCallbackDelegate>("OnDrawLayer");
            selectLayerCallback = Bind<ReorderableList.SelectCallbackDelegate>("OnSelectLayer");
            mouseUpLayerCallback = Bind<ReorderableList.SelectCallbackDelegate>("OnMouseUpLayer");

            categoryLayerList = new ReorderableList(currentLayerCategory.layers, typeof(LayerIndexEntry),
                draggable: false, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
            {
                drawElementBackgroundCallback = Bind<ReorderableList.ElementCallbackDelegate>("OnDrawLayerBackground"),
                drawElementCallback = DrawCategoryLayerElement,
                onSelectCallback = OnCategoryLayerSelected,
                onMouseUpCallback = OnCategoryLayerMouseUp,
                showDefaultBackground = false,
                headerHeight = 0f,
                footerHeight = 0f,
                elementHeight = LayerListElementHeight
            };

            if (!selectedLayerName.IsNullOrEmpty())
            {
                int index = currentLayerCategory.layers.FindIndex(l => l.layer.name == selectedLayerName);
                if (index >= 0)
                {
                    categoryLayerList.index = index;
                }
            }
        }

        /// <summary>
        /// Draws one row of the category layer list by delegating to Unity's own layer row drawing,
        /// translating the row's position within the category into the layer's index on the
        /// controller.
        /// </summary>
        /// <remarks>
        /// Both indices are range-checked because the two lists are rebuilt independently: a layer
        /// removed from the controller can still be listed on a category tree that has not been
        /// rebuilt yet, and drawing it would index Unity's list out of bounds.
        ///
        /// SHIPPED BUG, PRESERVED: <paramref name="isActive"/> and <paramref name="isFocused"/> are
        /// forwarded to the stock callback the wrong way round. See SHIPPED BUG 2 in the file header.
        /// </remarks>
        private static void DrawCategoryLayerElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index.IsValidIndex(currentLayerCategory.layers))
            {
                int layerIndex = currentLayerCategory.layers[index].layerIndex;
                if (layerIndex.IsValidIndex(unityLayerList.list))
                {
                    drawLayerCallback(rect, layerIndex, isFocused, isActive);
                }
            }
        }

        /// <summary>Forwards a category-view selection to Unity's own layer-selection handler.</summary>
        private static void OnCategoryLayerSelected(ReorderableList list)
        {
            ForwardLayerSelection(selectLayerCallback);
        }

        /// <summary>Forwards a category-view mouse-up to Unity's own layer mouse-up handler.</summary>
        private static void OnCategoryLayerMouseUp(ReorderableList list)
        {
            ForwardLayerSelection(mouseUpLayerCallback);
        }

        /// <summary>
        /// Points Unity's layer list at whatever the category view has selected and runs one of its
        /// stock callbacks against it.
        /// </summary>
        /// <remarks>
        /// This is how the category view stays a pure front end: every consequence of selecting a
        /// layer -- the graph switching, the inspector retargeting -- is still Unity's code acting on
        /// Unity's list, and the only thing the tool does is move that list's index first.
        /// </remarks>
        private static void ForwardLayerSelection(ReorderableList.SelectCallbackDelegate callback)
        {
            int index = GetSelectedLayerIndex(asControllerIndex: true);
            unityLayerList.index = index;
            callback(unityLayerList);
        }

        /// <summary>
        /// The selected layer, either as its row in whichever list is on screen or as its index on
        /// the controller.
        /// </summary>
        /// <param name="asControllerIndex">
        /// Whether to translate a category-view row into the layer's index on the controller. -1 when
        /// nothing is selected. Ignored in the default view, where the two are the same number.
        /// </param>
        private static int GetSelectedLayerIndex(bool asControllerIndex = false)
        {
            if (layerViewType != LayerViewViewType.DefaultView)
            {
                if (asControllerIndex)
                {
                    if (!categoryLayerList.index.IsValidIndex(categoryLayerList.list))
                    {
                        return -1;
                    }

                    return currentLayerCategory.layers[categoryLayerList.index].layerIndex;
                }

                return categoryLayerList.index;
            }

            return unityLayerList.index;
        }

        #endregion
    }
}
