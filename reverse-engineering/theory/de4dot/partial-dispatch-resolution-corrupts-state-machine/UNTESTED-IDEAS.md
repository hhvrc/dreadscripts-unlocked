# Untested diagnostic ideas for the `EdgeResolver` mis-seeding

Appendix to [`THEORY.md`](THEORY.md). This is what survives of a 584-line brainstormed hypothesis dump
(`chatgpt-spitball.md`, deleted 2026-07-30) once the tested, refuted and implemented entries were
removed. Kept only because the root cause in H6 is **still open** — the symptom is contained by
branch-and-select, the defect is not fixed.

## What the dump actually settled, so it is not re-derived

- **Its top recommendation has been implemented and works.** "Retain a partial rewrite only if concrete
  entry-state traces terminate or match the original machine — the evidence points away from local edge
  heuristics toward whole-machine validation." That is branch-and-select
  (`../../../de4dot/ROADMAP.md` §6): non-terminating machines 19 → 0.
- **Two hypotheses were confirmed** and are recorded in `../../../de4dot/ROADMAP.md` §5, not here:
  a produced seed is consumed as an incoming seed (the double-transform), and the plan validator only
  considers rewritten edges (which is why an exit-free-cycle gate on the plan graph changed nothing —
  the cycle runs through cases the plan never touches).
- **The whole arithmetic-semantics branch is dead.** Recomputing de4dot's derived seed independently
  with explicit 32-bit wrapped semantics reproduced its logged value bit-for-bit, so the emulator's
  multiply, XOR and unsigned-remainder handling are not implicated. Sixteen hypotheses fell to that one
  test. Do not reopen it.
- **Resolver-state hypotheses were refuted by inspection**: the resolver cache is keyed on object
  references rather than offsets, and a fresh `EdgeResolver` is constructed per dispatch node, so no
  state can leak between nested dispatches.

## Ideas still worth trying, if the seeding bug is picked up

Three local heuristics have already failed to separate legitimate loop re-entry from double-apply
(THEORY.md H6, and the reverted provenance guard). These are the untested ideas that do *not* reduce to
another local heuristic:

1. **Log a complete state tuple at every edge**, not just the seed: dispatch id, predecessor block,
   transform block, incoming and outgoing raw state, original case, rewritten case, synthetic state,
   the block that produced the seed, and the resolver phase that supplied it. Every hypothesis resolved
   in this investigation was resolved by instrumentation; two resolved by reasoning alone were later
   refuted.
2. **Track seed provenance as an expression tree** rather than a value, e.g.
   `EntryConstant(1714725738) -> Xor(K) -> Mod(3) -> Case(1) -> Transform(A,B) -> RawState(1198895877)`,
   and detect reapplication *structurally*. The reverted guard compared values, which cannot tell a
   legitimate second lap of the loop from the transform being applied twice; the derivation can.
3. **Give the domains distinct types** — `RawState`, `OriginalCase`, `SyntheticCase`,
   `SwitchTableIndex` as `readonly record struct` wrappers — so cross-domain substitution becomes a
   compile error. Note the "two numbering spaces are mixed" hypothesis was *refuted* as the cause here,
   so this is a guard against a class of mistake, not a fix for this bug.
4. **Compare the original and rewritten transition relations exhaustively.** For every observed original
   entry state, run both machines and compare the payload sequence and termination. This is the
   strongest available correctness statement about a resolution, and stronger than the current gate,
   which only asks whether the result terminates.
5. **Clone each shared transform block per incoming edge** as a pure diagnostic. If the loops disappear,
   shared-block state conflation is central — which would matter, because several case labels sharing
   one body is exactly why the provenance guard was too coarse.
