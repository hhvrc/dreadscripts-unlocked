Status: **CLOSED (2026-07-30)** — root-caused, and fixed upstream by a representation change.

The symptom (statements silently missing from a decompiled body) was contained in 2026-07-29 by
de4dot's branch-and-select, which keeps the unresolved candidate when the resolved one loops. The
root cause is now also gone: `RelationalDispatchResolver` resolves these machines by walking the
whole machine forward and carrying the configuration along the edge, instead of deriving a seed for
one dispatch from case attribution. Branch-and-select rejections across the corpus went **19 → 5 with
zero new rejections**, and `AdvisorTemplate` — the worked example throughout this file — now
decompiles to its true two-step body, `Label(...)` then `RemoveManager(2, 0)` then return.

**Do not re-derive any of this here.** `../../../de4dot/ROADMAP.md` is the single home:
§5 for why no case-attribution rule could ever have worked (the state needed is relational — inner
state *plus* which outer arm fired — and the old `DispatchNode` could not represent the pair), and
§7 item 3 for the design, the measurements and what slice 2 still owes.

**What this file is still good for, and the reason it is kept:** the evidence trail below is how the
bug was localised, and two of its conclusions were later shown to be right for the wrong reason. H9b
correctly identified the double-apply but blamed "the consumer"; the consumer's rule is sound given a
correct attribution, and the attribution was the broken part. H10 correctly refuted its own
hypothesis but could not say why. Both are worth reading before trusting any *new* local heuristic
in this pass.

**Practical consequence for reconstruction here:** unchanged in kind. Keep running
`scripts/detect_broken_state_machines.py` before trusting any `while (true) { switch }` body. Five
methods are still left unresolved corpus-wide and are faithful-but-verbose rather than wrong.

The original investigation follows, kept for the evidence trail.

Historical status line: OPEN — 17 LOOPS confirmed and reproducible. Two fix attempts reverted, three hypotheses refuted. Now localised to how XorSwitch resolves *state-transform blocks*; see H6.

## Question

When de4dot *partially* resolves a switch-dispatch state machine — rewriting some cases to constant
transitions but not all — is the surviving machine still faithful to the original?

This is a different question from `cflow-resolves-to-infinite-loop` (now FIXED-UPSTREAM). That one
was about methods left with **no reachable exit at all**. This one is about machines that still have
a perfectly reachable `ret` in the control-flow graph, but where **no real execution ever gets
there** because a case transitions to the wrong state.

Neither existing gate can see it:
- `ilverify` — an infinite loop is type-safe IL.
- the "reachable exit" gate added for the previous theory — the `ret` *is* a switch target, so it is
  reachable in the CFG. It is only unreachable once you know the state variable's actual values.

## Why it matters (not cosmetic)

A mis-resolved machine silently hides every statement on the unreachable tail. It does not look
broken; it looks like a shorter method.

This already caused a real reconstruction error. `ADOEditorUtility.ShapeSnapshot.ApplyToCollider`
was written without `c.shapeType` because the export's `CustomizeProduct` appeared not to set it —
the rotation case pushes state 4 instead of 5, so it loops on itself and both the shapeType case (5)
and the `ret` (0) fall off every traced path. Decoding the switch table by hand gives the real chain:

```
3 = radius → 2 = height → 6 = position → 7 = rotation → 5 = shapeType → 0 = ret
```

de4dot emits `7 → 4` and `4 → 4`. Fixed in devel (dreadful-re `4fdb360`), but the export is still
wrong, and anything else reconstructed from a LOOPS method is suspect for the same reason.

## Hypotheses

**H1: every partially-resolved dispatch left in the export is mis-resolved, not merely verbose —
CONFIRMED by measurement.**
`scripts/detect_broken_state_machines.py` traces each machine from its seed and classifies it.
Across the whole export: **17 LOOPS, 0 TERMINATES, 8 UNKNOWN.** Not one constant-transition machine
in the corpus terminates. Partial resolution is 100% harmful here.

**H2: the UNKNOWN group is a different class — the genuine two-variable/chained dispatch of
WORKLOG #5 — CONFIRMED.**
All 8 UNKNOWNs are on a *second* state variable (`num2`) with no constant seed, i.e. the outer plain
`switch` wrapped around the inner affine one. They are left fully unresolved, which is faithful.
Note `RunWatcher` appears here — it was also one of the methods in the previous theory.

**H3: the fix is an exit-free-cycle gate on the rewrite plan — IMPLEMENTED, TESTED, REVERTED.**
This is Exp 2's "all-or-nothing" idea with the gate it was missing. Exp 2 failed for two reasons
recorded in `IMPROVEMENT_PLAN.md`: it trusted `FailedCount == 0` as a "fully resolved" signal (it is
not — passthrough blocks get marked resolved without being rewired), and it paired the idea with
atomic dead-switch removal, which deleted live code. The gate proposed here is different in kind: it
does not ask the pass whether it thinks it succeeded, it **executes the resulting machine** and
checks that the trace terminates. And it needs no deletion step at all — on failure it simply leaves
the dispatch unresolved, which the corpus shows is the faithful outcome.

Built it in both `SwitchRewriter` (Reactor XorSwitch) and `SwitchCflowDeobfuscator` (shared): read
the resolved edges as a directed graph over case indices and reject the whole plan if it contains a
**deterministic cycle with no exit on it**. No seed needed — such a cycle is non-terminating wherever
the machine starts.

Result: **it changed nothing.** LOOPS stayed at exactly 17 (3/1/13, same methods), while costing +1
unresolved `switch` on two samples. The gate fired 19 times across the corpus, so it was rejecting
real rewrites — just never the ones that produce these machines. **Reverted**: an unvalidated guard
with a measurable readability cost and zero measured benefit does not belong in a pass with three
documented failed experiments behind it.

Why it failed is the useful part: **the 17 LOOPS are not produced by the switch-resolution rewrites
at all.** Both gates sit on the rewrite path, and neither sees these machines. So the corrupting step
is somewhere else — see H5.

**H4: the detector itself over-reports — PARTLY CONFIRMED, fixed.**
`IssuerSerializerAdapter` was reported LOOPS but actually terminates. Reactor emits opaque
re-assignments inside a case body:
```csharp
num = 0;
if (710326851u != 0) { num = 1; }   // guard is constant-true; the real transition is 1
```
The detector took the *first* constant assignment and stopped. Fixed to scan the whole case body and
report UNKNOWN whenever there is more than one distinct target, a conditional/derived write, or a
nested switch — a LOOPS verdict has to be trustworthy enough to act on. The `export/` counts did not
change (those 17 are genuinely single-transition), so the headline finding survives, but the earlier
claim rested on a detector that could have been inflating it.

**H5: a pass rewrites the switch TABLE ENTRY itself — REFUTED by instrumentation.**
Evidence, from running the detector against a `--no-cflow-deob` decompile of ADOverhaul2022 and
comparing with the normal one:

| | TERMINATES | LOOPS | UNKNOWN | total machines |
|---|---|---|---|---|
| `--no-cflow-deob` | 11 | 3 | 69 | 83 |
| normal            | 0  | 1 | 4  | 5  |

Three things follow. (a) cflow **resolves** 78 of 83 machines outright — it is doing its job. (b) The
3 pre-cflow LOOPS are *different methods* (`SchemaMapping`, `BroadcasterIdentifier`,
`IssuerSerializerAdapter` — the last now known to be an H4 false positive) from the 1 post-cflow one
(`CustomizeProduct`), so cflow both fixes and breaks machines. (c) Faithful machines demonstrably
exist and the detector finds them (11 TERMINATES), which rules out "the detector always says LOOPS".

Since neither rewrite-path gate touches these, the remaining candidates are the steps that *rewrite
the switch table itself* rather than its predecessors: constant folding collapsing a state-transform
block, then block merging/forwarding re-pointing the switch entry at whatever that block fell through
to. `CustomizeProduct`'s index 4 pointed at the XOR-transform block `IL_0071` in the original and
points at the rotation payload `IL_0003` after deobfuscation — a switch *table entry* changed, which
no predecessor redirect can explain.
NEXT: instrument the switch table itself. Snapshot `switchBlock.Targets` before and after each
`IBlocksDeobfuscator` (the same per-step harness used for the previous theory) and report the first
pass that changes an entry for one of the 17 known-LOOPS methods.

**H6: XorSwitch collapses a *state-dependent* transform block to a *fixed* target — OPEN, current
leading hypothesis.**
What the table instrumentation did show, for `CustomizeProduct`:

```
initial                    2=b16:IL_0126(1)   4=b6:IL_0062(5)   7=b7:IL_0078(5)
after XorSwitchDeobfuscator 2=b16:empty        4=b6:empty        7=b7:empty
after BlockCflowDeobfuscator 2=b16:IL_0000(1)  4=b6:IL_0000(1)   7=b7:IL_0000(1)
```

Those three case targets are the affine state-transform blocks (`ldloc state; ldc A; mul; ldc B;
xor; ...`). XorSwitch resolves each one, strips its instructions, and leaves an empty block whose
fall-through carries the resolved destination; `BlockCflowDeobfuscator` then materialises that as a
single branch, and the final layout folds branch-only blocks away. That folding is why the emitted
table ends up with `1` and `2` both pointing at the height payload, and `4` and `7` both at rotation.

The suspicion is that this is only valid when exactly **one** incoming state reaches a given transform
block. A transform block computes `next = (state * A) ^ B` — its destination is a *function of the
incoming state*. If two different states can reach it, collapsing it to one fixed target is lossy,
and the surviving machine dispatches one of them wrongly. `CustomizeProduct`'s rotation case ending
up with `7 → 4` and `4 → 4` is consistent with exactly that.
NEXT: instrument `EdgeResolver` to log, per transform block it resolves, how many distinct incoming
seeds it saw. If any resolved transform block has more than one, that is the bug, and the fix is to
refuse to collapse it (leave that case unresolved) rather than to pick one seed's answer.

## Evidence Log

- 2026-07-29: Found while investigating whether WORKLOG #5 was worth attempting. Traced
  `CustomizeProduct` by hand: `3→2→6→7→4→4`, never reaching the `ret` at case 0. The `ret` is a
  switch target, so every existing gate passes.
- 2026-07-29: Realised this had already corrupted a reconstruction (`ApplyToCollider` missing
  `shapeType`), and that the earlier claim that the re-export "confirmed" that reconstruction was an
  over-claim — it confirmed four of five assignments and hid the fifth.
- 2026-07-29: Built `scripts/detect_broken_state_machines.py` — read-only, conservative (anything
  undecidable is UNKNOWN, never LOOPS). Validated on two independent cases before trusting it:
  `CustomizeProduct` (trace matches the hand-trace exactly) and
  `AdvisorDicBridge.AdvisorTemplate` (seed 2 → `default` → 0 → no `case 0` → `default` again;
  intended body is plainly `Label(...); RemoveManager(2, 0); return;`).
- 2026-07-29: Corpus result — 17 LOOPS / 0 TERMINATES / 8 UNKNOWN. H1 and H2 confirmed.
- 2026-07-29: Implemented the exit-free-cycle gate in `SwitchRewriter`. First version flagged **218**
  dispatches on one sample — it kept only the first successor per case, so a conditional branch (an
  ordinary loop with a conditional exit) collapsed into a false cycle. Restricting it to *deterministic*
  transitions (exactly one successor) brought it to 2/7/10 across the corpus.
- 2026-07-29: Re-exported. LOOPS **unchanged at 17**. Added the same gate to the shared
  `SwitchCflowDeobfuscator`. Re-exported again: still **17**. Both gates sit on the rewrite path, so
  the corrupting step is not there. H3 refuted as implemented.
- 2026-07-29: Measured the cost — +1 unresolved `switch` on S1 and S2, 0 on S3, no change to any
  other gate (`realBug` 0/0/0, 0 non-terminating, 0 empty bodies, method counts unchanged).
  **Reverted both gates.** Zero measured benefit for a real cost, in the pass the xorswitch skill
  warns hardest about.
- 2026-07-29: Ran the detector against a `--no-cflow-deob` decompile (H5 table above). Found 11
  TERMINATES pre-cflow, which independently validates that the detector can recognise a healthy
  machine, and showed cflow resolves 78 of 83 machines while breaking a different one.
- 2026-07-29: Found and fixed an H4 false-positive class in the detector (opaque constant-true
  re-assignment inside a case body). `export/` counts unchanged.
- 2026-07-29: Instrumented the switch table per pass (snapshot `switchBlock.Targets` before/after
  every step and every `IBlocksDeobfuscator`). **H5 refuted** — no pass corrupts a table entry; all
  eight targets stay distinct throughout. First reading suggested three had collapsed to `IL_0000`,
  but that was the diagnostic's own ambiguity: de4dot-created instructions have `Offset == 0`, so
  distinct blocks print identically. Adding a positional index disproved it.
  What the run *did* show is H6: XorSwitch empties the three affine transform blocks (cases 2, 4, 7),
  BlockCflow materialises each as a single branch, and final layout folds them — which is how two
  case indices end up sharing one payload. Instrumentation removed; tree clean.

| assembly | TERMINATES | LOOPS | UNKNOWN |
|---|---|---|---|
| ADOverhaul2019   | 0 | 3  | 2 |
| ADOverhaul2022   | 0 | 1  | 4 |
| ControllerEditor | 0 | 13 | 2 |

## Current Conclusion

Partial dispatch resolution is actively harmful on this corpus — it never once produced a faithful
machine. The 17 LOOPS methods are **untrustworthy source**: statements are missing from the export,
silently.

**Immediate practical consequence (independent of any de4dot change):** run the detector before
reconstructing any method containing a `while (true) { switch (...) }`. If it reports LOOPS, read the
IL and decode the switch table by hand rather than trusting the decompiled body. The tail that looks
absent is the part that matters.

**H7: Phase 3 picks arbitrarily among admissible seeds — REFUTED by instrumentation.**
`EdgeResolver` Phase 3 brute-forces `allSeeds`, filters by `VerifySeedRoutesToCase` (which only
constrains `seed % M == ci`, so many seeds pass), and `break`s on the first that yields an in-range
case index. That is H6's shape exactly: a state-dependent answer collapsed to one guess. Instrumented
it to report, per predecessor, how many *distinct* case indices the admissible seeds would produce.
Result across ControllerEditor: **2715 Phase 3 seed-guesses, 0 ambiguous.** Every seed set agrees on
the target, so Phase 3 is not where the wrong edge comes from.

**H8: the `while(true){switch(num)}` shape is XorSwitch's own output — CONFIRMED, and it reframes
everything above.**
A/B run with `XorSwitchDeobfuscator` disabled (`de4dot_lab.py ab`, full reference set both sides):

| | XorSwitch on | off | delta |
|---|---|---|---|
| `switch(var)` dispatch sites | 67 | 46 | −21 |
| ...never terminating | **17** | **0** | −17 |
| ...undecidable | 8 | 0 | −8 |
| total lines | 66 561 | 234 178 | +167 617 |
| `goto` | 65 | 10 484 | +10 419 |

With the pass off there are **no decidable machines at all** — but that is *not* proof the pass
creates broken ones, because the detector only matches `switch (bareLocal)`. Unresolved, the operand
is still an expression (`switch ((num2 = (uint)(num ^ K)) % 3)`), which it cannot see. What the run
does establish is that the bare-variable form the detector reads is manufactured downstream of
XorSwitch, and the +167k lines / +10k gotos confirm the pass is doing the bulk of the recovery.

Ground truth for the simplest offender, `AdvisorTemplate` (ADOverhaul2019), unresolved:
```csharp
int num = 1714725738;
switch ((num2 = (uint)(num ^ 0x3A654A93)) % 3) {
case 1u:  goto IL_0014;
case 2u:  break;
default:  RemoveManager(2, 0); return;
}
IL_0014:
GUILayout.Label(...);
num = ((int)num2 * -593078698) ^ 0x4684C630;
```
versus resolved, where `num` takes values 2/4/1/0 — **not** the obfuscator's state values, and not in
range for modulus 3. So the resolved `num` is a *renumbered* state variable, and the emitted machine
is `2 → default(Label) → num = 0 → default(Label) → …`, never reaching `case 1: return`. The correct
chain is Label once, then `RemoveManager`, then return.

**H8b: is the loop de4dot's or ILSpy's? — SETTLED: de4dot's. ILSpy is faithful.**
The deobfuscated IL for `AdvisorTemplate::.ctor` (91 bytes) carries the state **on the evaluation
stack**, not in a local — `switch` pops it:

```
IL_0012: ldc.i4 2
IL_0017: br.s IL_0025
IL_0019: ldc.i4.2; ldc.i4.0; call RemoveManager(int32,int32)
IL_0020: ldc.i4 1                                  // falls through into the switch
IL_0025: switch (IL_003e, IL_005a, IL_003e, IL_003e, IL_0019)
IL_003e: <Label(second, ...)>
IL_0053: ldc.i4 0
IL_0058: br.s IL_0025
IL_005a: ret
```

Values ever pushed: **2** (entry) and **0** (after Label). Table index 2 → Label, index 0 → Label. So
the emitted IL is `Label → Label → Label …` forever. `IL_005a: ret` is only dispatched by index 1,
pushed exclusively at IL_0020 *inside* index 4 — and **nothing ever pushes 4**. Both the `ret` and the
`RemoveManager` block are CFG-reachable as switch targets and unreachable in execution, which is
precisely why gate 4 (item 4d, "no ret/throw in body") passes and why `ilverify` is silent.

Ground truth from the original affine dispatch, computed exactly:

| step | num | num2 = num ^ K | case = num2 % 3 | action |
|---|---|---|---|---|
| 0 | 1714725738 | 1548872185 | **1** | `Label(...)` |
| 1 | 2098264470 | 1198895877 | **0** | `RemoveManager(2,0); return` |

So the correct machine is exactly two steps. de4dot gets step 0 right and step 1 wrong.

**H9: de4dot pushes the ORIGINAL case value where its own renumbered table index is required —
REFUTED.** Instrumenting `EdgeResolver` showed the real cause is not a numbering-space mix-up; `0`
appearing in both roles was a coincidence. See H9b for what is actually happening. The reasoning
below is kept only because the constants it cites are still the evidence.

**H9b: the affine-transform block is seeded with the state its own transform PRODUCES — CONFIRMED in
code.** With `DE4DOT_DIAG_METHOD` logging every resolved edge, the 3-case inner dispatch of
`AdvisorTemplate::.ctor` resolves exactly two edges:

```
pred=IL_00A2 (seed push)      seed=none/zeroed  -> case=1  targetIncomingState=1548872185   correct
pred=IL_0066 (affine xform)   seed=1198895877   -> case=2  targetIncomingState=-1878369011  WRONG
```

`1198895877` is the value IL_0066's own transform produces from state `1548872185`, so feeding it back
in applies the transform twice. Correct answer is case **0** (`RemoveManager`), emitted is case **2**.
The seed's provenance is Phase 5, and its bookkeeping is *right*: `nextSeed = (seed*mul) ^ xor ^
xorKey = 1198895877`, `nextCase = 1198895877 % 3 = 0`, recorded as `caseStateVar[0]`. Case 0's entry
state really is 1198895877. **The defect is the consumer** — Phase 2 uses that value to seed the very
block that produced it.

**Fix attempt (seed-provenance guard) — IMPLEMENTED, MEASURED, REVERTED.** Recorded which block
produced each derived seed and refused to seed that block with it. Result: `AdvisorTemplate` and
`RecordObserver` fixed, **`FactoryStruct` regressed** — a ctor that was previously resolved to clean
straight-line code (`m_PredicateStruct = first; _value = first; m_TagStruct = selection;`) became a
machine looping `4 → 1 → 0 → 1 → …`, never reaching its `return`. All other gates held (ilverify
0/0/0, 0 empty bodies, 0 methods without ret/throw), but turning a correct method into a
non-terminating one is strictly worse than the two it fixed, so it was reverted.

Why the guard is too coarse: **a block can legitimately be re-entered carrying the state it just
produced** — that is simply the loop going around. `FactoryStruct` makes this visible because several
case labels share one body (`case 0: case 3: case 4:`), so one block is the body of multiple case
indices and each arrival has a different, valid entry state.

**H10: the real discriminator is `BlockToCase[pred]` associating a predecessor with a case whose body
it is not — REFUTED.** Instrumented both methods at the Phase 2 seeding site:

```
AdvisorTemplate  pred=IL_0066 predCase=0 caseBody=IL_0041 predIsCaseBody=False caseBodyReachesPred=False  (wrong)
FactoryStruct    pred=IL_0037 predCase=0 caseBody=IL_0054 predIsCaseBody=False caseBodyReachesPred=False  (correct)
```

Identical on both flags, so this does not separate the broken case from the working one.

**Where that leaves it.** Three local heuristics have now been tried and none discriminates
"legitimate re-entry with a produced state" from "double-apply". That is evidence the discriminator is
not local at all, and supports the conclusion already reached from the `0 TERMINATES` distribution:
the accept/reject decision has to be made by **tracing the resulting machine** and rejecting a
resolution whose trace does not terminate — ROADMAP items 1 and 2 — not by inspecting one edge.

> **Baseline note (2026-07-30).** The detector was rewritten concurrently from regexes to a
> Roslyn-derived analysis (`pipeline.source_analysis`), which is a real upgrade: a parser handles
> every write form natively, so the earlier hand-widened `assigned_var` patch is superseded. The
> classification is stricter and the corpus now reads **7 LOOPS / 18 UNKNOWN**, not 17 / 8.
> `AdvisorTemplate` is still flagged with the same `2 -> 0` trace, so everything above still applies,
> but the fix attempt's "17 → 15" was measured against the old classifier and is **not** comparable
> to the current baseline. Re-measure before trusting any delta.

**(superseded) H9 reasoning:** The evidence is the pair of constants:

- *Entry* pushes `2`, and de4dot's table index 2 → Label. That is a correct **table index**; the
  original case value for this step was `1`, so entry is *not* using the original numbering.
- *After Label* pushes `0`. The correct table index is `4` (→ `RemoveManager`). But `0` is exactly the
  **original mod-3 case value** for "RemoveManager; return" (step 1 above).

Two code paths compute the pushed state and they disagree about which numbering space they are in.
`0` is also a valid Label slot in the 5-entry table, which is why the result is a silent infinite loop
rather than an out-of-range index that would have been caught immediately.

That reframes the whole theory: the defect is **not** "a state-dependent transform collapsed to one
target" (H6). Here the Label block genuinely has one incoming state, so collapsing it is legitimate —
de4dot simply collapsed it to the wrong constant. **Confirming H9 means finding where the emitted
state constant is derived, and checking whether it is a `CaseIndex` into `DispatchNode.CaseTargets` or
a raw post-XOR/modulus value.** `ResolvedEdge.CaseIndex` vs `TargetIncomingStateVar` in
`EdgeResolver` are the two candidates, and `StateUpdateFinder`'s cut point decides which one survives
into the rewritten block.

**What the `0 TERMINATES` row is actually evidence for.** Worth stating explicitly, because it is the
strongest signal in the file and it is easy to under-read. A machine de4dot resolves *fully* does not
appear here at all — the switch becomes unreachable, dead-block removal takes it, and straight-line
code is left behind. So the scanned population is exactly the machines de4dot could **not** fully
resolve, and it splits in two: left completely alone (non-constant transitions ⇒ UNKNOWN, 8, faithful)
and partially resolved (constant transitions ⇒ **17, all broken**). There is no third bucket. The
empty middle is the finding: partial resolution has never once produced a correct machine here.

That licenses **all-or-nothing per dispatch** as the shape of the eventual fix. It does *not* license
retrying it at the rewrite site — H3 already did exactly that in both passes and moved nothing, because
these cycles run through cases the plan never touches and so are invisible in the plan-local edge graph.

**Next concrete step (H6):** instrument `EdgeResolver` to log, per transform block it resolves, how
many distinct incoming seeds it saw. More than one on any resolved block confirms H6, and the fix is to
refuse to collapse that block — leave the case unresolved — rather than pick one seed's answer.

The exit-free-cycle gate design from H3 is kept in this file rather than in the code so it is not
re-derived from scratch. If it is resurrected, two things must change: it belongs at the point where
edges are **derived**, not applied, and its completeness signal must be a real trace of the resulting
machine. `FailedCount == 0` is not that signal — passthrough blocks are marked resolved without being
rewired.

Target when it does land: **0 LOOPS**, with those methods moving to UNKNOWN (verbose but faithful),
not to TERMINATES.
