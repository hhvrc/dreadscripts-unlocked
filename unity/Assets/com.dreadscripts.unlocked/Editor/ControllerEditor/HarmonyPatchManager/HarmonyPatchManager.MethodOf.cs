// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   (nested type ControllerEditor.HarmonyPatchManager, lines 2798-3016)
//
// Forty-four one-line helpers, each `internal static MethodInfo Xxx<...>(SomeDelegate d) => d.Method;`
// and each differing only in the delegate type of its parameter. They are collapsed here onto a
// single overloaded name, MethodOf, as the brief permits for obfuscator-split families -- the
// parameter list already distinguishes every one of them, and no two of the delegate shapes in
// .Delegates.cs are identical, so overload resolution is unambiguous at every arity. Explicit type
// arguments at the call site (`MethodOf<Rect, TreeViewItem>(handler)`) select the overload exactly
// as the decompiled names did.
//
//   decompiled name -> overload, line
//   CreateReg    -> MethodOf(Action),                          line 2798
//   NewReg       -> MethodOf<T>(Action<T>),                     line 2803
//   PushReg      -> MethodOf<T,TT>(Action<T,TT>),               line 2808
//   ViewReg      -> MethodOf<T,TT,T3>(Action<..>),              line 2813
//   CollectReg   -> MethodOf<T,TT,T3,G>(Action<..>),            line 2818
//   ResolveReg   -> MethodOf<T,TT,T3,G,GG>(Action<..>),         line 2823
//   ListReg      -> MethodOf<T,TT,T3,G,GG,A>(Action<..>),       line 2828
//   VerifyReg    -> MethodOf<..7>(Func<..>),                    line 2833
//   FillReg      -> MethodOf<..6>(Func<..>),                    line 2838
//   WriteReg     -> MethodOf<..5>(Func<..>),                    line 2843
//   ForgotReg    -> MethodOf<..4>(Func<..>),                    line 2848
//   StopReg      -> MethodOf<..3>(Func<..>),                    line 2853
//   CheckReg     -> MethodOf<T,TT>(Func<T,TT>),                 line 2858
//   PrepareReg   -> MethodOf<T>(Func<T>),                       line 2863
//   AssetReg     -> MethodOf<T>(RefAction<T>),                  line 2868
//   UpdateReg    -> MethodOf<T,TT>(RefAction<..>),              line 2873
//   ChangeReg    -> MethodOf<..3>(RefAction<..>),               line 2878
//   SortReg      -> MethodOf<..4>(RefAction<..>),               line 2883
//   RegisterReg  -> MethodOf<..5>(RefAction<..>),               line 2888
//   LogoutReg    -> MethodOf<..6>(RefAction<..>),               line 2893
//   PatchReg     -> MethodOf<..7>(RefFunc<..>),                 line 2898
//   InterruptReg -> MethodOf<..6>(RefFunc<..>),                 line 2903
//   ManageReg    -> MethodOf<..5>(RefFunc<..>),                 line 2908
//   PrintReg     -> MethodOf<..4>(RefFunc<..>),                 line 2913
//   SearchReg    -> MethodOf<..3>(RefFunc<..>),                 line 2918
//   RevertReg    -> MethodOf<T,TT>(RefFunc<T,TT>),              line 2923
//   OrderTests   -> MethodOf<T>(OutAction<T>),                  line 2928
//   CompareTests -> MethodOf<T,TT>(OutAction<..>),              line 2933
//   SetTests     -> MethodOf<..3>(OutAction<..>),               line 2938
//   PostTests    -> MethodOf<..4>(OutAction<..>),               line 2943
//   SetupTests   -> MethodOf<..5>(OutAction<..>),               line 2948
//   EnableTests  -> MethodOf<..6>(OutAction<..>),               line 2953
//   PublishTests -> MethodOf<..7>(OutFunc<..>),                 line 2958
//   PopTests     -> MethodOf<..6>(OutFunc<..>),                 line 2963
//   ComputeTests -> MethodOf<..5>(OutFunc<..>),                 line 2968
//   MoveTests    -> MethodOf<..4>(OutFunc<..>),                 line 2973
//   ConcatTests  -> MethodOf<..3>(OutFunc<..>),                 line 2978
//   CallTests    -> MethodOf<T,TT>(OutFunc<T,TT>),              line 2983
//   CancelTests  -> MethodOf<..4>(ValValRefRefAction<..>),      line 2988
//   CountTests   -> MethodOf<T,TT>(RefValAction<T,TT>),         line 2993
//   DisableTests -> MethodOf<T,TT>(ValRefAction<T,TT>),         line 2998
//   InsertTests  -> MethodOf<T,TT>(ValOutAction<T,TT>),         line 3003
//   RestartTests -> MethodOf<..3>(ValValOutAction<..>),         line 3008
//   QueryTests   -> MethodOf<..3>(ValOutValAction<..>),         line 3013
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference. See HarmonyPatchManager.cs for the type-level header.

using System;
using System.Reflection;

namespace DreadScripts.ControllerEditor
{
    internal static partial class HarmonyPatchManager
    {
        // Why these exist at all: Harmony's Patch takes a MethodInfo, and a private static patch
        // method has no other way to name itself than reflection over its own type by string. That
        // string is unchecked -- rename the patch method and the tool silently stops patching. A
        // method group converted to a delegate is checked by the compiler both in name and in
        // signature, and Delegate.Method hands back the MethodInfo, so `MethodOf<Rect>(MyPrefix)`
        // fails to build the moment MyPrefix is renamed or its arguments change. The generic
        // arguments double as an assertion that the patch method's signature is the one the target
        // needs.
        //
        // Every overload is `d.Method`; the bodies say nothing, so they are left undocumented
        // individually.

        internal static MethodInfo MethodOf(Action method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T>(Action<T> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(Action<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(Action<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(Action<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(Action<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(Action<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T>(Func<T> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(Func<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(Func<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(Func<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(Func<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(Func<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A, AA>(Func<T, TT, T3, G, GG, A, AA> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T>(RefAction<T> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(RefAction<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(RefAction<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(RefAction<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(RefAction<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(RefAction<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(RefFunc<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(RefFunc<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(RefFunc<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(RefFunc<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(RefFunc<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A, AA>(RefFunc<T, TT, T3, G, GG, A, AA> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T>(OutAction<T> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(OutAction<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(OutAction<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(OutAction<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(OutAction<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(OutAction<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(OutFunc<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(OutFunc<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(OutFunc<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG>(OutFunc<T, TT, T3, G, GG> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A>(OutFunc<T, TT, T3, G, GG, A> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G, GG, A, AA>(OutFunc<T, TT, T3, G, GG, A, AA> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3, G>(ValValRefRefAction<T, TT, T3, G> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(RefValAction<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(ValRefAction<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT>(ValOutAction<T, TT> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(ValValOutAction<T, TT, T3> method)
        {
            return method.Method;
        }

        internal static MethodInfo MethodOf<T, TT, T3>(ValOutValAction<T, TT, T3> method)
        {
            return method.Method;
        }
    }
}
