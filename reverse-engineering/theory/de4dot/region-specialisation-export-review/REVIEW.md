# Region specialisation: export review (de4dot `f4129325`, export `de190c6`)

Status: **COMPLETE — all 9 rows resolved, covering all 6 distinct IL methods. No semantic regressions.** Region specialisation must not be closed until
every row below has a verdict derived from the **original binary**.

## Why this file exists

`de190c6` regenerated `export/` after branch-and-select rejections went 5 -> 0. The diff is 563
deletions against 45 insertions, and every aggregate gate is green — which is precisely the state in
which a semantically wrong export gets accepted. Gates 1-7 cannot see a payload that became
unreachable or a side effect that moved; only reading the original can.

**Four of the nine changed methods were never in the rejection set.** Scoping a review to the
rejection log would have skipped them. The set below comes from `scripts/changed_methods.py`, which
maps the commit's own diff hunks to the methods that enclose them via the Roslyn helper — filenames
are far too coarse (one 248-line file diff touched four methods) and the rejection log answers a
different question entirely.

## Method

For each row, through the sanctioned pipeline only:

```bash
python3 scripts/changed_methods.py de190c6            # regenerate this list
python3 scripts/de4dot_lab.py show --type <ORIGINAL TYPE> --sample <SAMPLE> --original --il
```

Derive the original's observable payload sequence and termination by decoding its dispatch by hand,
then compare against the new export. **Do not compare against the previous export** — it is de4dot
output too, and comparing derived-against-derived produces both false alarms and false clearances.
That is not hypothetical: `ComputeWrapper` below looked reordered against a remembered earlier trace,
and the original showed the export was right and the recollection was of a partially-resolved
intermediate.

Verdicts: **faithful** (same observable sequence), **presentation-only** (identical semantics, e.g. a
default argument elided), **uncertain**, **semantically wrong**.

Watch specifically for: deleted calls, reordered side effects, altered conditionals, default
arguments, exception paths, and payloads that became unreachable.

## One IL method can appear as several changed C# methods

`changed_methods.py` reports **decompiled** methods, and ILSpy inlines a lambda's body into every
call site that constructs it. So `<>c::RunWatcher` — one changed IL method, and one that *was* in the
rejection set — shows up as four changed C# methods: itself plus `RevertServer`, `IncludeServer` and
`SetIdentifier`, which merely pass `new Action(RunWatcher)` to a callee.

Checked by reading the original IL: `<>c::RevertServer` is 54 bytes of straight-line code with no
dispatch machine at all. Verifying the machine once, in `RunWatcher`, settles all four rows.

**This collapses the review.** The nine C# rows are six distinct IL methods, and only **one** of them
(`ComputeWrapper`) was outside the rejection set — not four as the raw count suggested.

## Renamed-type mapping

`export/` is named, so export type names differ from the original binary's:

| export type | original type |
|---|---|
| `ADOEditorUtility` (2019) | `AdvisorDicBridge` |
| `ADOEditorUtility` (2022) | `ExceptionSingletonStruct` |
| `LicenseManager` (2022) | `IdentifierSerializerConnector` |
| `EditorLayoutUtils` | `Definition` |
| `ControllerEditor` | `ControllerEditor` |

## Findings

| sample | method | why it changed | original payload sequence | new payload sequence | verdict |
|---|---|---|---|---|---|
| ADOverhaul2019 | `ADOEditorUtility::EnableAccount` | region specialisation resolved a 7-target outer switch chained to a 4-target inner one, over two transforms | `V_5=2p`; `V_1=s-c`; `V_2=2c-5p+4s-d`; `V_3=-c+3p-3s+d`; `return 0.5*(V_5 + V_1*t + V_2*t^2 + V_3*t^3)` | identical, same order | **faithful**, high confidence |
| ADOverhaul2022 | `ADOEditorUtility::ConnectProcess` | region specialisation resolved an 8-target outer switch chained to a 4-target inner one, over two transforms | `V_0=pol.time-instance.time`; `V_1=57.29578*Atan(instance.outTangent)`; `V_2=57.29578*Atan(pol.inTangent)`; `V_3=instance.value`; `V_4=pol.value`; then `(f(serv+1e-5)-f(serv))/1e-5` over `InitProcess(x, V_3, V_4, V_5, ...)` | identical, same order and same argument order | **faithful**, high confidence |
| ADOverhaul2022 | `_003C_003Ec::IncludeServer` | ILSpy inlines `RunWatcher`'s body here | see `RunWatcher` | see `RunWatcher` | **faithful** (inlined copy) |
| ADOverhaul2022 | `_003C_003Ec::RevertServer` | ILSpy inlines `RunWatcher`'s body here; the method itself is straight-line in the original | see `RunWatcher` | see `RunWatcher` | **faithful** (inlined copy) |
| ADOverhaul2022 | `_003C_003Ec::RunWatcher` | region specialisation resolved a 7-target outer switch chained to a 3-target inner one | `_Info=false`, `m_Config=false`, `_Worker=false`, `AssetConfiguration(true)`, `ret` | identical | **faithful**, high confidence — see note |
| ADOverhaul2022 | `LicenseManager::SetIdentifier` | ILSpy inlines `RunWatcher`'s body here | see `RunWatcher` | see `RunWatcher` | **faithful** (inlined copy) |
| ControllerEditor | `EditorLayoutUtils::ComputeWrapper` | region specialisation resolved a 7-target outer switch over three affine transforms | `setup = default(Color)`, `ConcatWrapper(setup)`, `Space(7f)`, `ret` | `LabelField`, `ConcatWrapper()`, `Space(7f)` | **faithful** — `setup` elided because `ConcatWrapper`'s parameter defaults to it |
| ControllerEditor | `_003C_003Ec__DisplayClass432_2::SelectThread` | region specialisation resolved a 7-target outer switch chained to a 3-target inner one | `V_0=RevertMapper().AddAnyStateTransition(baseReg.destinationState)`; `CopySerialized(baseReg, V_0)`; `V_0.conditions=new[]{c}`; `containerReg._ComposerReg.Add(V_0)`; `ret` | identical, same order | **faithful**, high confidence |
| ControllerEditor | `ControllerEditor::AssetAlgo` | region specialisation resolved a machine with three opaque-predicate forks | path `A.f.f.t`: 8 registration groups — `bool>`x3, `Edge>` (with `GetConstructor`), `bool>`x2, `Vector3[]>`, `Node>` | 8 calls: `MapReg`x3, `CalcReg`, `MapReg`x3, `ValidateReg` — same order, generic arguments aligned | **faithful, conditional** — see note |

### Note on `RunWatcher`

The original's step 2 (`m_Config = false`) is reached only if the opaque predicate
`<Module>::m_3efdc708…` is non-zero; the zero path would give `_Info`, `_Worker`, … in a different
order. The structure was derived from the original independently, and the export's sequence is one of
exactly two possible paths — the one where that field is non-zero. So this verdict rests on de4dot's
long-standing module-constant folding being right, which is separately gated, not on anything this
change introduced.

Decoded arithmetic, for reproduction: seed `-1327699375 ^ -448323036` = 1436272757, `% 3` = 2 ->
`_Worker = false`; transform `(s * 424842727) ^ 528674088 ^ -448323036` = 2003973535, `% 3` = 1 ->
`AssetConfiguration(true)` -> outer 5 -> `ret`.

### Note on `EnableAccount`

A cubic Hermite evaluation whose four coefficients were spread across the machine. Decoded from the
original: entry (outer 4) sets `V_5`; inner seed `-1983321664 ^ -1373404918` = 669739210, `%4`=2 ->
`V_1`; transform `(s*1550774083)^497932977^K` = 1806915941, `%4`=1 -> `V_2`, then an opaque predicate
whose non-zero side reaches `V_3`; transform `(s*1017991685)^1218189512^K` = 1155236155, `%4`=3 ->
return. Same opaque-predicate dependency as `RunWatcher`: the zero side would return with `V_3` never
assigned, and the export computing it is the evidence the predicate folds non-zero.

### `AssetAlgo` — the remaining row, and where the trace tool now pays for itself

The last unverified method, and unlike the other five it is a long *linear* sequence of registration
calls rather than a handful of assignments — the export shows a dozen-plus `MapReg`/`CalcReg` calls
whose **order and arguments** all have to be checked. That makes it the case the trace-tool threshold
was set for: the same decode (extract transform constants, iterate `(s*mul)^xor^K % M`, map indices
to blocks) would have to be reconstructed across many more states than any method so far, by hand,
with the failure mode being a silently dropped or reordered call in the middle.

`scripts/trace_original_machine.py` is that tool. Read-only, parses ILAsm obtained through
`pipeline.py`, and emits positional block ids (never IL offsets — distinct blocks share offset 0),
entry stack, the affine calculation, the selected case, ordered payload operations with call
arguments, and `Unknown` wherever execution cannot be proven. It deliberately assigns **no** verdict:
a tool that both derives the truth and grades the output can only agree with itself.

**It currently halts partway through `AssetAlgo`, and that is the designed behaviour.** After two
registration pairs it reaches `brfalse` on `<Module>::m_a98868ec…` — a Reactor opaque predicate whose
value is not in the IL — and stops rather than guess. The other five verified methods each depended
on exactly these predicates folding a particular way, which was recorded per method; here the trace
needs the assumption made explicit before it can continue.

`--assume-opaque {stop,zero,nonzero,fork}` is implemented. `stop` is the default; `zero`/`nonzero`
emit assumption-qualified traces; `fork` explores both successors as separate paths with their own
identities, bounded by the existing step/event caps, merging only when block, stack **and** tracked
locals are structurally identical. Each path reports its own calls, assignments, termination and
unresolved points. The tool never picks a branch by resemblance to the export.

**Fork result on `AssetAlgo`: the machine has more than one terminating original trace.** Three
opaque predicates fork, several branches merge back on identical configurations, and two leaves
reach `ret` — `A.f.f.t` and `A.t.f.t` — with *different* registration counts along the way
(`A -> A.f -> A.f.f -> A.f.f.t` versus `A -> A.t -> A.t.f -> A.t.f.t`, where the `A.t` segment
performs one registration where `A.f` performs two).

**Still to do for this row**, and it is now purely comparison rather than derivation:

1. Concatenate the calls along each root-to-leaf terminating path into two complete candidate
   sequences.
2. Compare each against the export's full ordered `MapReg`/`CalcReg` sequence.
3. Check whether the predicates' values are independently established by the existing
   module-constant analysis.
4. If they are, the verdict is unqualified. If they are not, the verdict must be stated as
   **"faithful conditional on the existing opaque-predicate fold selecting branch X"**, with the
   alternative trace shown — the same dependency the other five rows carry, made mechanically
   visible rather than left informal.

### `AssetAlgo` verdict

Fork found exactly two terminating original traces. Concatenating each root-to-leaf chain and
grouping the calls by registration:

- **`A.f.f.t` — 8 groups**, generic arguments `bool>`x3, `Edge>` (preceded by `GetConstructor`),
  `bool>`x2, `Vector3[]>`, `Node>`.
- **`A.t.f.t` — 7 groups**, missing one of the leading `bool>` registrations.

The export has 8 calls — `MapReg`x3, `CalcReg`, `MapReg`x3, `ValidateReg` — matching `A.f.f.t` in
count, order and generic arguments. `A.t.f.t` does not match.

> **Verdict: faithful, conditional on the existing opaque-predicate fold selecting the `A.f.f.t`
> branch.** The predicate values are `<Module>::m_*` fields whose folding is done by de4dot's
> module-constant analysis and is not independently established here — the same dependency the other
> five rows carry, stated mechanically rather than informally.

Worth noting which way the risk runs: the export matches the **longer** path, so if the fold were
wrong the export would contain one registration that never executes — a spurious addition, not a
deleted one. That is the milder of the two failure modes, and it is the opposite of the deletion this
review exists to catch.

## Acceptance before region specialisation is closed

- every row has a verdict derived from the original binary;
- no unexplained changed methods remain (`changed_methods.py` also reports changes landing outside
  any method, which must be explained too);
- all scorecard gates green; rejections `0/0/0`; live-code landmark green;
- repeated exports byte-identical;
- nothing pushed.

If a row comes out **semantically wrong**, stop and reduce it to a fixture under
`../../../de4dot/tests/samples/xorswitch/` before touching the resolver.
