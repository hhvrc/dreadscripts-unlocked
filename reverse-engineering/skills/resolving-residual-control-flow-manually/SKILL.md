---
name: resolving-residual-control-flow-manually
description: How to read and manually trace .NET Reactor's switch/XOR state-machine control-flow obfuscation in decompiled C# when de4dot's automated resolution fails to fully clean it up, and how to tell that apart from a genuine de4dot bug worth reporting to the fork. Use when export/ or ported source still has goto-heavy switch dispatch instead of straight-line code.
---

# Resolving Residual Control Flow Manually

## When to use

- A method in `export/` still contains a dense `switch` on a state variable with `goto`-heavy case
  bodies after the standard pipeline (see the `triaging-dotnet-reactor-obfuscation` skill).
- You're about to hand-write the equivalent clean logic into `../public/unity/Assets/com.dreadscripts.unlocked/Editor/` and need to be sure
  you've traced every reachable case correctly, not just the common path.
- You want to decide whether a specific unresolved dispatch is worth reporting as a fixable case to
  the local de4dot fork versus just working around it by hand once.

**de4dot lives at `../de4dot`** (sibling to this repo) and is under active development *specifically
to improve .NET Reactor deobfuscation* for exactly the samples this project works with — it's not a
static, unmaintained dependency. Check its `WORKLOG.md`/`IMPROVEMENT_PLAN.md` before manually
working around a gap; it may already be fixed on a branch that just hasn't been rebuilt into
`work/de4dot/de4dot` yet.

## Background: what the obfuscated pattern looks like

.NET Reactor's control-flow obfuscation drives execution through a `switch` on an integer state
variable, where the *next* state value is computed with an affine transform before the jump, e.g.
(post-decompile, names vary):

```csharp
int state = seed;
while (true) {
    switch (state) {
        case 17:
            // do some real work
            state = (state * K1) ^ K2;   // <-- affine update feeding the next dispatch
            continue;
        case 42:
            ...
    }
}
```

This is the same affine-transform family used for string decryption (see
the `decrypting-dotnet-reactor-smethod-strings` skill) applied to control flow instead of string offsets.
De4dot's XorSwitch resolver (in the local fork) tries to statically evaluate this chain and rewrite
it into straight-line/structured code; when it succeeds you won't see this pattern in `export/` at
all. When it doesn't, this is what's left.

## Manual tracing workflow

1. **Identify the state variable and its seed.** Find where it's initialized before the dispatch
   loop — this is the entry state.
2. **Build the case → next-state map by hand** for each reachable case: read the affine update
   expression at the end of each case body and evaluate it against known input, OR pattern-match it
   against a case label already present in the same switch (some case bodies push a literal
   constant rather than an expression — that's a directly-readable edge, no arithmetic needed).
3. **Follow the chain from the seed** until you hit a case with no further state update (a genuine
   exit) or a loop back to an already-visited state (confirms a real loop in the source logic, not
   an obfuscation artifact — the plugin's own logic might legitimately loop here).
4. **Cross-check against other case labels appearing but never reached** in your trace — an
   obfuscator often inserts dead/opaque cases that real execution never hits; don't spend time
   fully reverse-engineering a case your trace shows is unreachable from the actual entry seed.
5. Write the resulting **structured** (if/loop, no goto) equivalent into `../public/unity/Assets/com.dreadscripts.unlocked/Editor/`.

## Distinguishing "expected gap" from "de4dot bug"

Before spending significant manual-tracing effort, check whether this is a known limitation vs. a
regression:

- **Known limitation (expected, trace by hand):** two-variable/nested dispatch (an outer plain
  `switch(state)` wrapping an inner affine xor-switch) — the local de4dot fork's `IMPROVEMENT_PLAN.md`
  documents this as open, unresolved work with several failed automated-fix attempts (all reverted
  because they either produced invalid IL or silently deleted live code). Don't expect this class to
  auto-resolve until that work lands; tracing it by hand for now is the correct approach.
- **Possible bug worth flagging:** a *single-layer* dispatch that looks like it should have resolved
  (matches the basic pattern above, no visible nesting) but didn't, or where the automated resolution
  clearly went wrong (e.g. a resulting `goto` jumps into the middle of what looks like unrelated
  logic, or a method body looks suspiciously *empty* where you know from context it shouldn't be).
  Empty-looking methods after "resolution" are a specific known failure mode from prior experiments
  on the fork (a rewrite pass marked something "resolved" without actually rewiring it) — flag this
  distinctly rather than assuming your trace is wrong.

If in doubt, don't guess — check `../de4dot/WORKLOG.md` and `IMPROVEMENT_PLAN.md` for the current
state of the relevant deobfuscator pass before either (a) sinking hours into manual tracing that a
fork fix would make unnecessary, or (b) writing off a genuine bug as "just how it is."

## Pitfalls

- Don't assume the *first* case label you find reachable from a given state is the only path in —
  re-verify by checking every predecessor, since a wrong assumption here silently produces wrong
  cleaned-up source that looks plausible but doesn't match actual behavior.
- Don't manually "fix" a method by deleting the switch and guessing at intent from method/field
  names alone — the naming is often still generic at this stage (see
  the `renaming-and-documenting-deobfuscated-source` skill); trace the actual transform first.
- A method that looks like an infinite loop in the decompile is not necessarily a de4dot artifact —
  confirm by tracing before assuming it's obfuscation noise rather than real (if unusual) plugin logic.
