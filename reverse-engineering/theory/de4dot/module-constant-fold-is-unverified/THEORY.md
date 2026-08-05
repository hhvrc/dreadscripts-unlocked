# Are de4dot's `<Module>::m_*` opaque-predicate constants independently verifiable?

## Status: RESOLVED — the fold is justified. H1 CONFIRMED, H2/H5 REFUTED. `AssetAlgo` is faithful unconditionally.

## Question

.NET Reactor gates the arms of its dispatch machines with opaque predicates of the form
`ldc.i4 <next state>; ldsfld <Module>{guid}::m_<hex>; brfalse <dispatch>`. The branch taken depends
entirely on the value of that module field, which is **not a literal in the IL** — it is assigned by
a computation elsewhere in `<Module>`.

de4dot folds these fields to constants, and every dispatch resolution therefore inherits whichever
branch that fold selected. **Is that fold independently checkable against the original binary, and
does it in particular select the branch the `AssetAlgo` export reproduces?**

A yes/no is possible: either the value is derivable from the original binary without executing it, or
it is not.

## Why it matters

The de4dot export review (`../region-specialisation-export-review/REVIEW.md`) verified six IL methods
against the original binary. Every one of their traces crossed at least one of these predicates, so
every verdict rests on the fold — five were recorded as faithful with the dependency noted, and
`ControllerEditor::AssetAlgo` was recorded as **faithful *conditional* on the fold selecting the
`A.f.f.t` branch**.

`AssetAlgo` is the sharpest case because the two branches are observably different programs: 8
registration calls versus 7. If the fold is wrong there, the export contains a registration that never
executes. (Worth noting which way that cuts — a wrong fold here adds behaviour rather than deleting
it, which is the milder failure. It is still wrong.)

Resolving this would upgrade one conditional verdict to unconditional and retire a caveat that
currently attaches, quietly, to every dispatch resolution in the corpus.

## Hypotheses

### H1: the values are statically derivable from the original binary — CONFIRMED
The store is a pure arithmetic chain over constants, so evaluating `<Module>`'s initialisation
symbolically yields the value without running anything.

Evidence for:
- The one store to `m_a98868ec43754181b06aa648bd84405e` is
  `... ldc.i4 -318882175 ; sub ; ldc.i4 258404895 ; xor ; stsfld` — arithmetic, not a call.

- Traced to the head: the whole producer is `ldc.i4 2011376005 ; ldc.i4 5 ; shl ; ldc.i4 -318882175 ;
  sub ; ldc.i4 258404895 ; xor ; stsfld`. **Every leaf is a literal.**
- The enclosing method `<Module>{guid}::t7092f36793464a398c422c922789217c` is **pure straight-line**:
  337 `ldc.i4`, 112 `stsfld`, arithmetic, and exactly one `ret`. No branches, no calls, no field
  reads. So all 112 stores are unconditional and ordered, and no store can be bypassed.

Evidence against:
- None for the arithmetic. See the residual under H2.

### H2: the values are NOT statically derivable — REFUTED entirely (the residual is closed; see 2026-07-30 (cont. 2))
Reactor computes them at load, from something outside the managed IL (native runtime, checksum,
anti-tamper).

Refuted for the *computation*: the initialiser is constants only, so the values it would store are
fully determined by the file.

**Residual, and it is real:** `t7092f36793464a398c422c922789217c` has **no managed caller anywhere in
the assembly** — 0 hits across all 1105 types, including `<Module>::.cctor`. If it never runs, every
field keeps its default of 0. That matters for exactly one of the three fields, because two of them
are computed to 0 anyway.

### H3: de4dot's fold agrees with whatever H1 yields — SUPPORTED on branch direction, not yet on value
Distinct from H1: even if the value is derivable, de4dot might derive it differently.

Evidence for:
- The three branch directions the independently-computed values imply match, one for one, the
  directions de4dot's resolution took (below). Agreement on all three is not a coincidence.

Still missing:
- de4dot's *value* has not been read directly. Agreement on branch direction is weaker than agreement
  on value — a different value with the same zero/non-zero character would look identical here.
  Instrumenting the module-constant folder to log (field, recovered value) closes this.

### H4: the `AssetAlgo` export corresponds to the fold selecting `A.f.f.t` — SUPPORTED, not proof
Evidence for:
- Fork-tracing the original gives exactly two terminating paths; `A.f.f.t` has 8 registration groups
  matching the export in count, order and generic arguments (`bool>`x3, `Edge>`, `bool>`x2,
  `Vector3[]>`, `Node>`), `A.t.f.t` has 7 and does not match.

Why this is not proof of correctness:
- It establishes which branch de4dot *took*, not which branch is *right*. Confirming the export
  matches the fold is circular as an argument that the fold is correct.

### H5: de4dot folds these fields without proving the initialiser executes — REFUTED as a defect, CONFIRMED as a code property
Evidence for:
- `CflowConstantsInliner` (Reactor v4) picks the **first** sealed type with >=100 fields, then the
  first static, assembly-visible method with a body, constant-folds it, harvests every
  `ldc.i4; stsfld` pair into a dictionary, and `InlineAllConstants()` replaces **every** `ldsfld` of
  those fields module-wide with the harvested value.
- There is **no reachability, caller, or `.cctor` check anywhere in it.** The premise "these stores
  happen" is assumed, never established.
Evidence against (decisive, and it overturns the reading above as a *defect*):
- Instrumenting the pass shows that in **all three** samples the initialiser is called from
  `<Module>{guid}::.cctor()` — the type initialiser of the very type the constant fields live on.
  The CLR guarantees a type initialiser runs before the first static-field access on that type, and
  the accesses being folded are exactly those. So the stores are guaranteed to have happened before
  any read the pass rewrites. The premise is not merely true here, it is structurally true: Reactor
  puts the fields and their initialiser on one type.
- So the code property stands (the pass performs no reachability check, and would inline just as
  happily from a method that never runs), but it was a hardening opportunity, not a live defect.
- **Since fixed.** The pass now refuses to fold unless the declaring type's `.cctor` calls the
  initialiser. Details and the reason a caller count would not have been good enough live in
  `../../../de4dot/ROADMAP.md` §7 item 3 — not restated here.

## Evidence Log

### 2026-07-30
- `trace_original_machine.py --assume-opaque fork` on `ControllerEditor::AssetAlgo` names the three
  predicates it crosses: `m_00c697f65cff441780b484eae581215a`,
  `m_827ed2b704f44cb58edd10a36bec1f35`, `m_a98868ec43754181b06aa648bd84405e`.
- `de4dot_lab.py show --type '<Module>' --sample ControllerEditor --original --il` dumps 354108 lines
  and contains exactly **one** store to `m_a98868ec…`, at `IL_0481`, preceded by
  `ldc.i4 -318882175 ; sub ; ldc.i4 258404895 ; xor`. So the value is *computed*, not a literal —
  consistent with H1 and with H2, and distinguishing them needs the expression traced to its head.

### 2026-07-30 (cont.) — the decisive comparison

Independently computed from the initialiser's arithmetic, **before** looking at which branch the
export took:

| field | expression | value | `brfalse`/`brtrue` implication |
|---|---|---|---|
| `m_a98868ec…` | `((2011376005 << 5) - -318882175) ^ 258404895` | **0** | condition is zero |
| `m_827ed2b7…` | `(1105613058 << 6) ^ 2039758976` | **0** | condition is zero |
| `m_00c697f6…` | `(-1055197544) ^ -1357967777` | **1846933703** | condition is non-zero |

The fork trace maps the predicates in order to `m_a98868ec…`, `m_827ed2b7…`, `m_00c697f6…`, and the
path the export reproduces is `A.f.f.t` — where by this tool's convention `f` means "condition is
zero" and `t` means "non-zero". So the export requires **zero, zero, non-zero**, which is exactly
what the arithmetic yields. The alternative terminating path `A.t.f.t` requires `m_a98868ec… != 0`,
which the arithmetic contradicts.

This derivation used only the original binary. It did not take the export, or de4dot's chosen branch,
as input — which is what makes it non-circular and therefore worth something.

**The residual bites precisely here.** If `t7092…` never executes, all three fields are 0, the third
predicate flips, and the export's path becomes unreachable. Two of the three fields are 0 either way,
so the entire question reduces to whether that one initialiser runs.

### 2026-07-30 (cont.) — what invokes the initialiser: no managed mechanism

- No managed caller in the original, by name or token: 0 hits across 1105 types.
- **Not hidden behind a Reactor proxy either.** The original `<Module>::.cctor` calls only proxy
  dispatchers, so a call through one would be invisible to a textual search — but in de4dot's
  *deobfuscated* output, where `ProxyCallFixer` has resolved them, `<Module>::.cctor` calls exactly
  `smethod_6()` and `RuntimeHelpers::InitializeArray`. `smethod_6` contains **one** `stsfld`, so it is
  not the 112-store initialiser.
- The method name appears as a string literal nowhere, so it is not invoked by reflection-by-name.

So no managed path reaches it, before or after proxy resolution. The remaining candidate is Reactor's
**native bootstrap** invoking it by token from outside managed IL — which cannot be settled from an IL
dump and needs PE-level inspection.

### 2026-07-30 (cont. 2) — the initialiser is called from its own type's `.cctor`

The previous entry's "no managed mechanism" finding was **wrong**, and the way it was wrong is worth
recording. I searched `<Module>::.cctor` — the module initialiser. The caller is
`<Module>{d638325a-0ef0-4c9b-8fd7-9fadcae08a8a}::.cctor`, a **different type** that merely shares the
`<Module>` prefix. Reactor names its constant-holder types that way, so a search keyed on `<Module>`
looks like it covers the case and does not.

Instrumenting `CflowConstantsInliner` to name its referrers rather than reasoning from a dump settled
it in one run, across all three samples:

| sample | initialiser | constants | inline sites | referenced by |
|---|---|---|---|---|
| ADOverhaul2019 | `h0721dee5…` | 118 | 515 | `<Module>{195366e3-…}::.cctor` |
| ADOverhaul2022 | `f6580f2588…` | 126 | 588 | `<Module>{e6dbd7e3-…}::.cctor` |
| ControllerEditor | `t7092f367…` | 112 | 1533 | `<Module>{d638325a-…}::.cctor` |

That answers the binary question: **`t7092f367…` runs because its type's `.cctor` calls it, and the
CLR runs that `.cctor` before the first read of any field on that type.** No native bootstrap, no
`calli`, no reflection — the mechanism was ordinary type-initialiser semantics all along.

Two lessons worth keeping. First, the count came from instrumenting the pass, which took one build
and one run; the dump-reading that preceded it took far longer and produced a wrong answer. When the
question is "what does this pass see", ask the pass. Second, `<Module>` is a prefix here, not a name.

## Current Conclusion

Still open, and not blocking: the affected verdicts are recorded as conditional rather than asserted.

The arithmetic question is answered: the values are statically derivable, the initialiser is
branch-free and call-free, and the three values imply exactly the branch directions de4dot took.
`AssetAlgo`'s conditional verdict is now conditional on **one** thing rather than on the fold as a
whole.

**The fold is justified, and `AssetAlgo`'s verdict is now unconditional.** The three predicate
constants are statically derivable from the original binary (`0`, `0`, `1846933703`), they
independently select the `A.f.f.t` branch the export reproduces, and the initialiser that stores them
is guaranteed to run before any of the folded reads. Nothing about `AssetAlgo` rests on trusting
de4dot's own choice.

The rest of this section is superseded, and kept only because it records how the wrong answer was
reached:

Two results, one of which is actionable now and one of which is not.

**Actionable: `CflowConstantsInliner` has an unproven premise (H5).** It never checks that the method
whose stores it harvests can execute. That is a defect in the pass regardless of this sample's
outcome, and it is the thing to fix — not by adding general constant propagation, but by requiring
some evidence of execution (a caller, a `.cctor` chain, or an explicit recorded assumption) before
inlining. Note the shape of the risk: it inlines module-wide from a method it never proved runs, so
a wrong premise silently changes control flow everywhere those fields are read, which is exactly the
"locally correct, globally wrong" family in `../../../de4dot/ROADMAP.md` §3.

**Not settled: whether Reactor's native bootstrap invokes it.** No managed path does. Answering this
needs PE-level inspection (native entry point, mixed-mode loader hooks, tokens embedded in native
data) rather than an IL dump, and it is the only thing standing between `AssetAlgo`'s verdict and
unconditional.

**Done, and it closed the question** (kept below as written, since the reasoning that led here is
the useful part). Instrument `CflowConstantsInliner` to log the type and method it selected, how
many constants it harvested, and whether that method has any caller in the module — the last of those
is three lines and turns the premise from implicit into observable. That is worth doing *before*
the PE work, because it decides whether this is one sample's quirk or a pass-level defect that also
affects the other two samples.

Do **not** treat the branch agreement in the previous entry as closing the question. It shows the fold
is self-consistent with the file's arithmetic; it does not show the arithmetic is ever executed.
