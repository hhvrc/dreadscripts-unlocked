// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//   (nested type ControllerEditor.HarmonyPatchManager, delegate declarations at lines 2436-2494)
// Every delegate is ported under its decompiled name; the obfuscator left this family alone
// because the names are load-bearing at the call sites. Line numbers are relative to the
// decompiled snapshot. See HarmonyPatchManager.cs for the type-level header.
//
// Audit status: VERIFIED -- the thirty delegate declarations were extracted from
// export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs lines 2436-2494 and
// text-diffed against the thirty declared here: identical character for character, including
// variance modifiers and parameter names. The cited range is still exact in the current snapshot,
// and it contains nothing but these declarations, so nothing in it is unported.

namespace DreadScripts.ControllerEditor
{
    internal static partial class HarmonyPatchManager
    {
        // Harmony patch methods are ordinary static methods whose parameters are matched by name
        // against the target's arguments, and a patch declares its intent through parameter
        // modifiers: `ref` on a parameter named __result lets a postfix replace the return value,
        // `ref` on a named argument lets a prefix rewrite it in place, and `out` appears wherever
        // the target itself has an out parameter. None of those shapes fit System.Action or
        // System.Func, which is why this family exists: it is the smallest set of delegate types
        // that can name every patch method in the tool, so that a call site can pass a method group
        // and let the compiler check the signature instead of looking the method up by string.
        //
        // The names encode the shape: Ref* takes every parameter by reference, Out* by out, and the
        // mixed Val/Ref/Out forms spell out each position in order. *Action returns void, *Func
        // returns its last type argument.

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
    }
}
