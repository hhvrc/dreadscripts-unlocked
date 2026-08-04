// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
// The by-ref / by-out delegate family of HarmonyPatchManager and the MethodInfo-of-delegate
// helpers, export lines 2436-2494 and 2798-3016 (inside HarmonyPatchManager, lines 2402-3017).
//
//   RefAction`1..`6, RefFunc`2..`7, OutAction`1..`6, OutFunc`2..`7, ValValRefRefAction,
//   RefValAction, ValRefAction, ValOutAction, ValValOutAction, ValOutValAction
//                            -> unchanged, lines 2436-2494 (names already in renames/)
//
// Every one of the following returns `d.Method`; they exist only so a patch can be written as a
// method group instead of a typeof/GetMethod pair. They collapse to one overload set:
//   CreateReg    -> GetMethodInfo(Action),                          line 2798
//   NewReg       -> GetMethodInfo<T>(Action<T>),                    line 2803
//   PushReg      -> GetMethodInfo<T,TT>(Action<T,TT>),              line 2808
//   ViewReg      -> GetMethodInfo<T,TT,T3>(Action<..>),             line 2813
//   CollectReg   -> GetMethodInfo<T,TT,T3,G>(Action<..>),           line 2818
//   ResolveReg   -> GetMethodInfo<T,TT,T3,G,GG>(Action<..>),        line 2823
//   ListReg      -> GetMethodInfo<T,TT,T3,G,GG,A>(Action<..>),      line 2828
//   VerifyReg    -> GetMethodInfo<..,AA>(Func<..>),                 line 2833
//   FillReg      -> GetMethodInfo<..,A>(Func<..>),                  line 2838
//   WriteReg     -> GetMethodInfo<..,GG>(Func<..>),                 line 2843
//   ForgotReg    -> GetMethodInfo<..,G>(Func<..>),                  line 2848
//   StopReg      -> GetMethodInfo<T,TT,T3>(Func<..>),               line 2853
//   CheckReg     -> GetMethodInfo<T,TT>(Func<T,TT>),                line 2858
//   PrepareReg   -> GetMethodInfo<T>(Func<T>),                      line 2863
//   AssetReg     -> GetMethodInfo<T>(RefAction<T>),                 line 2868
//   UpdateReg    -> GetMethodInfo<T,TT>(RefAction<..>),             line 2873
//   ChangeReg    -> GetMethodInfo<T,TT,T3>(RefAction<..>),          line 2878
//   SortReg      -> GetMethodInfo<T,TT,T3,G>(RefAction<..>),        line 2883
//   RegisterReg  -> GetMethodInfo<T,TT,T3,G,GG>(RefAction<..>),     line 2888
//   LogoutReg    -> GetMethodInfo<T,TT,T3,G,GG,A>(RefAction<..>),   line 2893
//   PatchReg     -> GetMethodInfo<..,AA>(RefFunc<..>),              line 2898
//   InterruptReg -> GetMethodInfo<..,A>(RefFunc<..>),               line 2903
//   ManageReg    -> GetMethodInfo<..,GG>(RefFunc<..>),              line 2908
//   PrintReg     -> GetMethodInfo<..,G>(RefFunc<..>),               line 2913
//   SearchReg    -> GetMethodInfo<T,TT,T3>(RefFunc<..>),            line 2918
//   RevertReg    -> GetMethodInfo<T,TT>(RefFunc<T,TT>),             line 2923
//   OrderTests   -> GetMethodInfo<T>(OutAction<T>),                 line 2928
//   CompareTests -> GetMethodInfo<T,TT>(OutAction<..>),             line 2933
//   SetTests     -> GetMethodInfo<T,TT,T3>(OutAction<..>),          line 2938
//   PostTests    -> GetMethodInfo<T,TT,T3,G>(OutAction<..>),        line 2943
//   SetupTests   -> GetMethodInfo<T,TT,T3,G,GG>(OutAction<..>),     line 2948
//   EnableTests  -> GetMethodInfo<T,TT,T3,G,GG,A>(OutAction<..>),   line 2953
//   PublishTests -> GetMethodInfo<..,AA>(OutFunc<..>),              line 2958
//   PopTests     -> GetMethodInfo<..,A>(OutFunc<..>),               line 2963
//   ComputeTests -> GetMethodInfo<..,GG>(OutFunc<..>),              line 2968
//   MoveTests    -> GetMethodInfo<..,G>(OutFunc<..>),               line 2973
//   ConcatTests  -> GetMethodInfo<T,TT,T3>(OutFunc<..>),            line 2978
//   CallTests    -> GetMethodInfo<T,TT>(OutFunc<T,TT>),             line 2983
//   CancelTests  -> GetMethodInfo<..>(ValValRefRefAction<..>),      line 2988
//   CountTests   -> GetMethodInfo<T,TT>(RefValAction<..>),          line 2993
//   DisableTests -> GetMethodInfo<T,TT>(ValRefAction<..>),          line 2998
//   InsertTests  -> GetMethodInfo<T,TT>(ValOutAction<..>),          line 3003
//   RestartTests -> GetMethodInfo<T,TT,T3>(ValValOutAction<..>),    line 3008
//   QueryTests   -> GetMethodInfo<T,TT,T3>(ValOutValAction<..>),    line 3013
//
// Collapsing them to one name is safe: the overloads differ by parameter type, so the metadata keeps
// 44 distinct signatures. An invented per-arity suffix would claim a distinction the original does
// not have. Same reasoning as EditorUtils' Button/ToggleButton families (RE_NOTES, 2026-07-31).
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Audit status: VERIFIED against export member-by-member (2026-08-04).

using System;
using System.Reflection;

namespace DreadScripts.ControllerEditor
{
    internal sealed partial class ControllerEditor
    {
        internal static partial class HarmonyPatchManager
        {
            // Harmony patch methods routinely take their arguments by ref or out (that is how a
            // prefix rewrites them), and the BCL has no delegate shape for that. These fill the gap
            // so such a patch can still be handed over as a method group.

            internal delegate void RefAction<T>(ref T arg);

            internal delegate void RefAction<T, TT>(ref T arg1, ref TT arg2);

            internal delegate void RefAction<T, TT, T3>(ref T arg1, ref TT arg2, ref T3 arg3);

            internal delegate void RefAction<T, TT, T3, G>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

            internal delegate void RefAction<T, TT, T3, G, GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

            internal delegate void RefAction<T, TT, T3, G, GG, A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

            internal delegate AA RefFunc<T, TT, T3, G, GG, A, out AA>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5, ref A arg6);

            internal delegate A RefFunc<T, TT, T3, G, GG, out A>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4, ref GG arg5);

            internal delegate GG RefFunc<T, TT, T3, G, out GG>(ref T arg1, ref TT arg2, ref T3 arg3, ref G arg4);

            internal delegate G RefFunc<T, TT, T3, out G>(ref T arg1, ref TT arg2, ref T3 arg3);

            internal delegate T3 RefFunc<T, TT, out T3>(ref T arg1, ref TT arg2);

            internal delegate TT RefFunc<T, out TT>(ref T arg);

            internal delegate void OutAction<T>(out T arg);

            internal delegate void OutAction<T, TT>(out T arg1, out TT arg2);

            internal delegate void OutAction<T, TT, T3>(out T arg1, out TT arg2, out T3 arg3);

            internal delegate void OutAction<T, TT, T3, G>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

            internal delegate void OutAction<T, TT, T3, G, GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

            internal delegate void OutAction<T, TT, T3, G, GG, A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

            internal delegate AA OutFunc<T, TT, T3, G, GG, A, out AA>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5, out A arg6);

            internal delegate A OutFunc<T, TT, T3, G, GG, out A>(out T arg1, out TT arg2, out T3 arg3, out G arg4, out GG arg5);

            internal delegate GG OutFunc<T, TT, T3, G, out GG>(out T arg1, out TT arg2, out T3 arg3, out G arg4);

            internal delegate G OutFunc<T, TT, T3, out G>(out T arg1, out TT arg2, out T3 arg3);

            internal delegate T3 OutFunc<T, TT, out T3>(out T arg1, out TT arg2);

            internal delegate TT OutFunc<T, out TT>(out T arg);

            internal delegate void ValValRefRefAction<T, TT, T3, G>(T arg1, TT arg2, ref T3 arg3, ref G arg4);

            internal delegate void RefValAction<T, in TT>(ref T arg1, TT arg2);

            internal delegate void ValRefAction<in T, TT>(T arg1, ref TT arg2);

            internal delegate void ValOutAction<in T, TT>(T arg1, out TT arg2);

            internal delegate void ValValOutAction<in T, in TT, T3>(T arg1, TT arg2, out T3 arg3);

            internal delegate void ValOutValAction<in T, TT, in T3>(T arg1, out TT arg2, T3 arg3);

            // ---------------------------------------------------------------------------------
            // One overload per delegate shape above, so a patch can be named rather than looked up.
            // ---------------------------------------------------------------------------------

            internal static MethodInfo GetMethodInfo(Action d) => d.Method;

            internal static MethodInfo GetMethodInfo<T>(Action<T> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(Action<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(Action<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(Action<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(Action<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(Action<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T>(Func<T> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(Func<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(Func<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(Func<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(Func<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(Func<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A, AA>(Func<T, TT, T3, G, GG, A, AA> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T>(RefAction<T> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(RefAction<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(RefAction<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(RefAction<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(RefAction<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(RefAction<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(RefFunc<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(RefFunc<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(RefFunc<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(RefFunc<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(RefFunc<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A, AA>(RefFunc<T, TT, T3, G, GG, A, AA> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T>(OutAction<T> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(OutAction<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(OutAction<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(OutAction<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(OutAction<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(OutAction<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(OutFunc<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(OutFunc<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(OutFunc<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG>(OutFunc<T, TT, T3, G, GG> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A>(OutFunc<T, TT, T3, G, GG, A> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G, GG, A, AA>(OutFunc<T, TT, T3, G, GG, A, AA> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3, G>(ValValRefRefAction<T, TT, T3, G> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(RefValAction<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(ValRefAction<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT>(ValOutAction<T, TT> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(ValValOutAction<T, TT, T3> d) => d.Method;

            internal static MethodInfo GetMethodInfo<T, TT, T3>(ValOutValAction<T, TT, T3> d) => d.Method;
        }
    }
}
