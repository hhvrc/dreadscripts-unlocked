Status: PARTLY FIXED — reopened 2026-07-31. The three root causes found in 2026-07-29 are genuinely
fixed (de4dot commits 2994ea4e, f154a4f1, 5de92887), but "corpus 21 → 0" was never a measurement of
this bug class. It was a count of *switch state machines*, and a surviving instance has now been
found that is not one.

## Reopened: the "0" came from a detector that cannot see this shape

`VectorSetting.GetValue` in `export/ADOverhaul2022/DreadScripts/ADOverhaul/LicenseManager.cs`
(obfuscated `InstanceRegDic::ConnectUtils`, at the time of writing line 1083):

```csharp
internal Vector3 ConnectUtils() {
    if (!_TagIdentifier) {
        while (true) {
            _TagIdentifier = true;
            filterIdentifier = new Vector3(_valueX, _valueY, _valueZ);
        }
    }
    return filterIdentifier;               // unreachable
}
```

The original is an ordinary lazy-init getter. Nothing inside the loop re-tests the guard, so it never
exits: reading a Vector3 setting hangs the Editor. It reads as plausible at a glance — the loop body
even sets the flag the guard checks — which is precisely why it survived review.

**Both detectors report clean on it, and that is the finding.** `scripts/detect_broken_state_machines.py`
reports `LOOPS: 0` because it classifies `while (true) { switch (...) }` bodies, and this has no
switch. de4dot's gate 5 reports `non-terminating=0` for the same reason. So the corpus number that
closed this theory measured a narrower thing than the theory is about, and nobody noticed because
zero looked like success.

That makes this the project's next instance of *reading an absence of evidence as a clean zero* — the
failure mode `feedback-tooling-lives-in-scripts` already records. The lesson generalises: a gate's
number means what the gate measures, not what the theory it closed was about, and the two drift apart
silently the moment a bug class turns out to be broader than the shape first seen.

**Open questions for whoever picks this up** (de4dot changes are the repo owner's, not to be made
from here):
- Is this the same root cause as the three fixed ones, applied to a non-switch loop, or a fourth?
- How many more non-switch instances exist? Nothing currently counts them, in either repo.
- Should the detector and gate 5 be widened to any reachable `while (true)` with no reachable exit,
  rather than only switch-shaped ones?

Everything below is the original 2026-07-29 investigation, which stands on its own terms.

---

One root cause in three places; all guarded. Ported to de4dot's `WORKLOG.md` item 4d in de4dot-generic
phrasing. Kept here for the evidence trail and the two methodological lessons below.

## Question

Why does de4dot's control-flow deobfuscation turn some Reactor switch state machines into
methods with **no `ret` instruction at all** — a block that branches to itself forever?

Canonical example, `ExceptionSingletonStruct.TaskMethod.CustomizeProduct(VRCPhysBoneCollider)`
(devel: `ADOEditorUtility.ShapeSnapshot`):

```csharp
internal void CustomizeProduct(VRCPhysBoneCollider reference) {
    reference.radius = m_ReaderMethod;
    reference.height = m_StubMethod;
    reference.position = _RulesMethod;
    while (true) { reference.rotation = m_TestsMethod; }   // never returns
}
```

IL: the final block ends `br.s` to its own first instruction, and the method contains no `ret`
anywhere. This is an unconditional Editor hang on a basic "copy collider shape" action, so it cannot
be the original program's behaviour.

**This bug class is invisible to `ilverify`** — an infinite loop is perfectly type-safe. de4dot's
`realBug` metric is 0/0/0 while these methods are still wrong, which is exactly why `realBug` alone
must not be read as "de4dot is correct".

## Scale

Methods with **no `ret`/`throw`/`rethrow`** anywhere in the body — i.e. genuinely non-terminating.
(Counting only `ret` inflates ControllerEditor by 4: compiler-generated iterator
`IEnumerator.Reset()` stubs whose whole body is `newobj NotSupportedException; throw`. Those were
never broken, so the honest starting point is 21, not 25.)

| sample | at open | after H5 fix | after H6 fix | after H7 fix |
|---|---|---|---|---|
| ADOverhaul2019   | 6  | 6  | 2 | **0** |
| ADOverhaul2022   | 4  | 2  | 2 | **0** |
| ControllerEditor | 11 | 11 | 2 | **0** |
| **total**        | **21** | **19** | **6** | **0** |

0 in all samples with `--no-cflow-deob`, which is what implicated a cflow pass in the first place.

## Hypotheses

**H1: a de4dot control-flow pass produces it — CONFIRMED.**
`--no-cflow-deob` gives 0 methods with no `ret` (vs 4 on ADOverhaul2022).

**H2: pre-existing in the original obfuscated binary — REFUTED.**
The original `CustomizeProduct` is a normal Reactor state machine (365 bytes, `switch` over 8 case
targets driven by `loc 1`, affine `mul`/`xor` state updates). Not an infinite loop.

**H3: `SwitchRewriter.CleanupDeadCases` deletes the block containing `ret` — REFUTED.**
Instrumented it to log whenever it removes a block containing `ret`/`throw`/`rethrow`: **zero hits**.
A defensive `IsMethodExit` guard was added there regardless — it is provably inert on this corpus
(decompiled IL byte-identical with and without it), so it costs nothing and closes the failure mode
for other samples.

**H4: the edge rewriting produces the self-loop, via either (4.1) a 2-block cycle later merged, or
(4.2) `EdgeResolver` picking a wrong target — SUPERSEDED, never directly tested.**
Both were plausible readings of the symptom. They were overtaken by H5, which explains the observed
behaviour without needing either. Recorded honestly: these were *not* refuted by experiment, they
simply stopped being the best explanation. If H5/H6's remaining gap turns out not to cover
everything, 4.2 is still worth testing — it is the same family as the already-fixed phase-6
double-apply bug.

**H5: a partially-resolved dispatch orphans the switch, and with it the unresolved case holding the
exit — CONFIRMED, FIXED (`XorSwitchDeobfuscator` / `SwitchRewriter`).**
Each applied edge redirects one predecessor away from the switch block. Redirecting the *last live*
predecessor makes the switch unreachable, and therefore also every case that was **not** resolved —
which can be the one containing or leading to the method's only `ret`. The blocks still exist at that
moment (so an "is there still a `ret`?" check passes), but de4dot's later dead-block cleanup removes
them.
Decisive evidence: instrumenting entry/exit **reachability** per pass (not mere existence) showed
`CancelUtils` and `ConnectProcess` both going reachable → unreachable inside a single pass, both with
`failed=1` (i.e. an unresolved case).
Fix: `SwitchRewriter.WouldOrphanMethodExit` simulates the pending redirects on a copy of the
successor map and skips the whole dispatch if no `ret`/`throw` would remain reachable from entry.

**H6: the same bug shape exists independently in de4dot's *generic* switch pass — CONFIRMED, FIXED
(`SwitchCflowDeobfuscator`).**
After H5's fix, 2 methods on ADOverhaul2022 still broke, and they never tripped the reachability
check inside XorSwitch — yet disabling XorSwitch alone made them go away, so XorSwitch was only
reshaping the graph and something later finished the job.
Decisive evidence: instrumented `BlocksCflowDeobfuscator`'s iteration loop with the same reachability
check after **every** step (`RemoveDeadBlocks`, `MergeBlocks`, and each `IBlocksDeobfuscator` by
name). All remaining cases reported `exit became UNREACHABLE at step 'SwitchCflowDeobfuscator'`; a
second probe narrowed it to the `DeobfuscateTOS` family.
Mechanism is identical to H5 but in shared code: `DeobfuscateTOS`, `DeobfuscateLdloc` and
`DeobfuscateStLdloc` each walk the switch block's sources and redirect every one to its resolved
target; once the last source is redirected the switch, and any target no source resolved to, is
unreachable.
Fix: all three workers collect their redirects first and validate them together against a
`WouldOrphanMethodExit` simulation before applying any. `Bcc` predecessors are modelled correctly
(only edges pointing at the switch are replaced), so conditional sources keep their other edge.

**H7: the remaining cases are a combined-effect gap inside one `DeobfuscateTOS` call — CONFIRMED, FIXED.**
Within a single call, the `DeobfuscateTos_Ldloc` fallback and the direct redirects are validated
*separately*, each against a graph that does not yet include the other's edits. Neither simulation
sees that together they orphan the exit. Evidence for: after H6's fix the remaining methods still reported the loss at `DeobfuscateTOS` while
the guard demonstrably did not fire. Confirmed by construction: splitting the three workers into a
`Plan*` phase (no mutation) and a single `ApplyPlan` — so a TOS call unions its own plan with the
recursive `PlanTos_Ldloc` plans and validates the union once — took the corpus to 0.

## Evidence Log

- 2026-07-29: Found while auditing devel's `ShapeSnapshot` against the export. Worked around on the
  devel side by reconstructing the plain assignment every sibling apply-method uses, rather than
  copying the loop.
- 2026-07-29: Bisected with `--no-cflow-deob` (0 vs 4) → a cflow pass. H1 confirmed, H2 refuted.
- 2026-07-29: Bisected by commenting out only `new xorswitch.XorSwitchDeobfuscator()` (0 vs 4)
  → XorSwitch implicated.
- 2026-07-29: Instrumented `CleanupDeadCases` for removal of exit-containing blocks → 0 hits.
  H3 refuted. Defensive guard kept anyway (inert, verified byte-identical output).
- 2026-07-29: Switched the instrumentation from "does a `ret` exist" to "is a `ret` **reachable**".
  This is what made the bug visible at all — the block survives right up until the next iteration's
  dead-block cleanup. `CancelUtils`/`ConnectProcess` caught going reachable → unreachable in one
  pass. H5 confirmed; fix landed (de4dot `2994ea4e`). Corpus 25 → 23.
- 2026-07-29: Instrumented `BlocksCflowDeobfuscator`'s loop per-step and per-deobfuscator; all
  remaining cases pointed at `SwitchCflowDeobfuscator`, then at `DeobfuscateTOS`. H6 confirmed; fix
  landed (de4dot `f154a4f1`). Corpus 23 → 10, with `realBug` held at 0/0/0, zero empty method bodies
  and unchanged method counts (1019/2859) — verified because this is a *shared* pass touching every
  obfuscator, not just Reactor.
- 2026-07-29: Confirmed the remaining cases still report the loss at `DeobfuscateTOS` with the guard
  in place → H7 formed.
- 2026-07-29: Split `DeobfuscateTOS`/`Ldloc`/`StLdloc` into `Plan*` + `ApplyPlan`, so a TOS call
  unions its own plan with the recursive fallback plans and validates once. H7 confirmed by
  construction; fix landed (de4dot `5de92887`). **Corpus 6 → 0.** Gates re-checked corpus-wide:
  `realBug` 0/0/0, zero empty bodies, method counts unchanged (1019/1019/2859).
- 2026-07-29: Re-exported. `CustomizeProduct` now decompiles as an intact state machine reading
  `radius → height → position → rotation → return`, which **independently confirms** the
  hand-reconstruction in devel's `ShapeSnapshot.ApplyToCollider` written while the bug was active.
- 2026-07-29: Corrected the metric itself after finding the 4 `IEnumerator.Reset()` false positives —
  a valid exit is `ret` **or** `throw`/`rethrow`.

## Current Conclusion

RESOLVED. One root cause — **a CFG rewrite that is locally correct but globally severs the only path
to the method's exit** — in three places, all now guarded by the same read-only simulation. Corpus
21 → 0, with `realBug` held at 0/0/0, zero empty method bodies and unchanged method counts throughout
(two of the three sites are shared code, so every check was corpus-wide).

`CustomizeProduct` — the method that opened this file — now decompiles as an intact, traceable state
machine: `radius → height → position → rotation → return`. **That independently confirms the
hand-reconstruction in devel's `ShapeSnapshot.ApplyToCollider`**, which was written from the sibling
methods while this bug was still active. Same four assignments, same order, terminating. The
workaround stands, and no longer needs to be treated as provisional.

### Two lessons worth carrying forward

1. **Check reachability, not existence.** "Does the method still contain a `ret`?" passes right up
   until the next iteration's dead-block cleanup runs. Nothing was visible until the probe became
   "is a `ret` still reachable from entry?". Every one of the three sites was found that way.
2. **Validate a rewrite plan as a whole, never incrementally.** Each individual redirect looked safe;
   it was always the *last* one that orphaned the exit. This is also why undo-after-the-fact is not
   an option here: the rewrites mutate instructions
   (`ReplaceLastNonBranchWithBranch`, an added `pop`), so restoring successors alone would leave
   blocks inconsistent. Plan-then-validate-then-apply is the shape that works.

Regression metric: **count of methods with no `ret`/`throw`/`rethrow`** — unlike unresolved-dispatch
or `goto` counts, deleting code cannot lower it. Now a standing gate in de4dot's `WORKLOG.md`.
Counting only `ret` is wrong: it reports 4 false positives on ControllerEditor (compiler-generated
iterator `IEnumerator.Reset()` stubs that are entirely `newobj NotSupportedException; throw`), which
is why the true starting point was 21 rather than 25.
