// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/AnimatorGraphReflection.cs
//   nested static class TypeResolvers -> TypeResolvers, line 18
//     every resolver field keeps its decompiled name (stateMachineGraph, stateMachineGraphGUI,
//     edgeGUI, animatorControllerTool, breadCrumbElement, graph, graphGUI, stateNode,
//     blendTreeNode, stateMachineNode, entryNode, anyStateNode, exitNode, edgeInfo,
//     transitionEditionContext), lines 20-48
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Each decompiled field spells the assembly-qualified name out in full; the identical suffix is
// factored into a const below, which the compiler folds back into exactly the same literals.
// Audit status: VERIFIED against reverse-engineering/export/ member-by-member (2026-08-04). All 15 resolver fields present.

namespace DreadScripts.ControllerEditor
{
    internal static partial class AnimatorGraphReflection
    {
        /// <summary>
        /// The <c>UnityEditor.Graphs</c> types that make up the Animator window, none of which are
        /// public.
        /// </summary>
        /// <remarks>
        /// Held as <see cref="TypeResolver"/> rather than <see cref="System.Type"/> so that the
        /// lookups happen on first use instead of at class-initialisation time: a Unity version that
        /// has renamed or dropped one of these types then costs a null at the point of use, rather
        /// than a type-load failure that would take the whole tool down.
        /// </remarks>
        internal static class TypeResolvers
        {
            /// <summary>Assembly qualification shared by every type below.</summary>
            private const string GraphsAssembly =
                ", UnityEditor.Graphs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            public static readonly TypeResolver stateMachineGraph =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.Graph" + GraphsAssembly);

            public static readonly TypeResolver stateMachineGraphGUI =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.GraphGUI" + GraphsAssembly);

            public static readonly TypeResolver edgeGUI =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeGUI" + GraphsAssembly);

            /// <summary>The Animator window itself.</summary>
            public static readonly TypeResolver animatorControllerTool =
                new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool" + GraphsAssembly);

            /// <summary>One entry of the Animator window's breadcrumb bar.</summary>
            public static readonly TypeResolver breadCrumbElement =
                new TypeResolver("UnityEditor.Graphs.AnimatorControllerTool+BreadCrumbElement" + GraphsAssembly);

            /// <summary>The graph base type, shared by the state machine and blend tree graphs.</summary>
            public static readonly TypeResolver graph =
                new TypeResolver("UnityEditor.Graphs.Graph" + GraphsAssembly);

            /// <inheritdoc cref="graph"/>
            public static readonly TypeResolver graphGUI =
                new TypeResolver("UnityEditor.Graphs.GraphGUI" + GraphsAssembly);

            public static readonly TypeResolver stateNode =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateNode" + GraphsAssembly);

            public static readonly TypeResolver blendTreeNode =
                new TypeResolver("UnityEditor.Graphs.AnimationBlendTree.Node" + GraphsAssembly);

            public static readonly TypeResolver stateMachineNode =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.StateMachineNode" + GraphsAssembly);

            public static readonly TypeResolver entryNode =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EntryNode" + GraphsAssembly);

            public static readonly TypeResolver anyStateNode =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.AnyStateNode" + GraphsAssembly);

            public static readonly TypeResolver exitNode =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.ExitNode" + GraphsAssembly);

            /// <summary>
            /// The per-edge record the graph keeps, holding every transition drawn on that one arrow.
            /// </summary>
            public static readonly TypeResolver edgeInfo =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.EdgeInfo" + GraphsAssembly);

            /// <summary>A single transition as the graph sees it, together with its two endpoints.</summary>
            public static readonly TypeResolver transitionEditionContext =
                new TypeResolver("UnityEditor.Graphs.AnimationStateMachine.TransitionEditionContext" + GraphsAssembly);
        }
    }
}
