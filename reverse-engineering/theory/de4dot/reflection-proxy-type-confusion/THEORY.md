# Which de4dot pass produces the `Wrapper.Proxy → ((Type)this).GetMethod` bug?

## Status: FIXED-UPSTREAM (2026-07-29) — de4dot commit 2df44c24, WORKLOG item #4

**Answer: no pass "produces" it. The type confusion is pre-existing in the obfuscated input, and
de4dot merely exposes it.** H3 was correct; H1 and H2 were both refuted.

Reactor emits reflection stubs declared as `instance` methods whose `this` slot never holds an
instance of the declaring type — it carries an arbitrary receiver that obfuscated callers pass as a
weakly-typed `object` argument to a *static* proxy dispatcher:

```il
// original stub — verifies fine, because the receiver is only ever an `object` parameter
ldarg.0; ldarg.1; ldarg.2; ldsfld <delegate>
call static MethodInfo WorkerRef::IncludeInterceptor(object, string, BindingFlags, WorkerRef)
```

`ProxyCallFixer` correctly resolves that dispatcher to the real *instance* target. The stack shape is
unchanged (receiver + N args), so the `object`-typed slot is silently reinterpreted as a typed
`this` → mirror-image `StackUnexpected` at both the stub and every call site.

**Fix** (`de4dot.code/deobfuscators/FakeInstanceStubFixer.cs`, run from Reactor v4 `DeobfuscateEnd()`
after ProxyCallFixer): convert such a stub to `static` with the receiver as an explicit leading
parameter typed to the target's declaring type. Needs zero IL edits — static `arg0` occupies the old
`this` slot, and call sites push identical values. Guarded to only touch already-provably-invalid
methods (declaring type non-assignable to target's declaring type), so legitimate forwarding such as
`Editor::get_target` is untouched.

**Result: realBug 17/17/1 → 2/2/1.** Verified: 997 methods before and after (deletes nothing), zero
empty bodies, large sample unaffected. `export/` regenerated — the stubs now decompile as
`static MethodInfo VisitIterator(Type type_0, string def, BindingFlags pol) => type_0.GetMethod(def, pol);`

**Baseline correction:** the "6/6/0" figure this file and de4dot's WORKLOG previously cited was
measured with an **incomplete reference set**. `ilverify` silently skips methods it cannot fully
resolve, so ~447 `FileLoadErrorGeneric` lines dismissed as "VRC SDK is not distributable" noise were
actually hiding 9 real errors per sample. The VRChat SDK *is* checked in at `deps/vrchat/`. True
pre-fix baseline: 17/17/1.

Remaining (tracked as WORKLOG #4b): 2 `MissingMethod` errors on `<>c__DisplayClass*` / async
state-machine members — a different root cause, related to `DisplayClassCleaner`.

---

## Original investigation (kept for the record)

### Status when opened: OPEN (evidence gathered, responsible pass not yet pinpointed)

Tracks de4dot `WORKLOG.md` item #4 — "Reflection-proxy type confusion — the ONLY remaining real
IL-bug class." This file is the *investigation* layer; once the responsible pass is confirmed by
instrumentation and a guarded fix is verified via the ilverify/realBug methodology, port the
finding into `../de4dot/IMPROVEMENT_PLAN.md` + `WORKLOG.md` in de4dot's own generic terms (no
product names, no dreadful-re paths) and mark this `FIXED-UPSTREAM`.

## Question

Which de4dot pass rewrites a proxied reflection call so that a wrapper-typed value (not a real
`System.Type`) ends up as the receiver of a `System.Type` instance method (e.g. `GetMethod`),
producing `StackUnexpected` under `ilverify`? WORKLOG already ruled out the generic `inlineCandidate`
inliner in `ObfuscatedFile.cs` (instrumented; confirmed it doesn't touch these methods) and named two
remaining candidates to instrument: `ProxyCallFixer` (v4) and "the cflow method-call inliner."

## Reproduction (new — not in de4dot's repo, tracked here only)

Sample: `dreadful-re/binaries/ADOverhaul2022.dll` (very likely de4dot's own "S2" test sample per
`IMPROVEMENT_PLAN.md`'s size description — ~16 types — matching this assembly's export file count).

```bash
# from de4dot/
OPENSSL_ENABLE_SHA1_SIGNATURES=1 dotnet Release/net8.0/linux-x64/de4dot.dll \
  -f ../dreadful-re/binaries/ADOverhaul2022.dll -o /tmp/ADOverhaul2022-deobf.dll

DOTNET_ROLL_FORWARD=LatestMajor ilspycmd -il -t "DreadScripts.ADOverhaul.AdvisorMethod" \
  /tmp/ADOverhaul2022-deobf.dll
```

Two methods on `DreadScripts.ADOverhaul.AdvisorMethod` (an `internal static class` — abstract+sealed
at the IL level) show the mirror-image bug:

**`VisitIterator$PST060003AA`** (declared `instance`, 2 explicit params `string def, BindingFlags pol`):
```il
IL_0000: ldarg.0
IL_0001: ldarg.1
IL_0002: ldarg.2
IL_0003: call instance class [mscorlib]System.Reflection.MethodInfo [mscorlib]System.Type::GetMethod(string, valuetype [mscorlib]System.Reflection.BindingFlags)
IL_0008: ret
```
`ldarg.0` here is the method's own implicit `this` (static type `AdvisorMethod`) — pushed directly as
the receiver for `Type::GetMethod`. `ilverify`: `StackUnexpected [offset 0x3] found=AdvisorMethod
expected=System.Type`.

**`ConnectIterator`** (the caller) — pushes a genuine `System.Type` value, then does
`call instance ... VisitIterator$PST060003AA(string, BindingFlags)` as if calling an instance method
on an `AdvisorMethod` receiver. `ilverify`: `StackUnexpected [offset 0x1E] found=System.Type
expected=AdvisorMethod` — the exact mirror of the callee-side error.

Both errors are two views of the same single defect: **`VisitIterator` is declared `instance` (its
first stack slot is treated as `this` of type `AdvisorMethod`), but every real call site — and its
own body — actually treats that first stack slot as an explicit `System.Type` argument.** The method
should be `static` with an explicit leading `Type` parameter; something restored/preserved the wrong
calling convention for it.

Context clue (not verified as causally relevant, just suggestive): per dreadful-re's own RE notes,
this class corresponds to a small reflection-cache helper that legitimately does
`Type.GetType(typeName).GetMethod(methodName, flags)` to reach a Unity-internal API not exposed in
public headers — i.e. the *real* original method plausibly took a type-name string, resolved a
`Type`, and called `GetMethod` on *that*, not on `this`. That resolve-a-Type step is exactly the kind
of small same-type helper `MethodCallInliner` is meant to fold away — see Hypothesis 3.

## Hypotheses

### H1: `ProxyCallFixer` (v4) swaps the call target to the wrong overload — REFUTED
`GetCallInfo` (`de4dot.code/deobfuscators/dotNET_Reactor/v4/ProxyCallFixer.cs`) resolves the real
callee via an exact metadata token (`module.ResolveToken(token)`) read from a decrypted dictionary,
then `FixProxyCalls` (`ProxyCallFixerBase.cs`) only replaces the call *opcode+operand* at a fixed
instruction index — it never touches surrounding instructions, argument count, or the enclosing
method's own signature/`HasThis`. This pass is stack-shape-preserving: whatever was already on the
stack before the delegate `Invoke` call stays exactly as-is, just routed to a different callee. It
cannot explain either the callee-side (`VisitIterator` itself declared wrong) or a caller passing the
wrong receiver type, since it never edits either method's signature. Ruling this out doesn't need
runtime instrumentation — the mechanism structurally can't produce this bug shape.

### H2: The Reactor-registered `MethodCallInliner(false)` inlines a static Type-resolving helper into `VisitIterator`, but mis-maps whose `this` is whose — OPEN, leading hypothesis
`de4dot.code/deobfuscators/dotNET_Reactor/v4/Deobfuscator.cs:145` registers
`new MethodCallInliner(false)` (`inlineInstanceMethods = false`) as one of the shared cflow passes
for Reactor v4. `MethodCallInliner.CanInline` (`de4dot.blocks/cflow/MethodCallInliner.cs`) only
inlines a callee that is (a) non-generic, (b) declared in the *same type* as the caller, and (c)
`static` (since `inlineInstanceMethods=false` for this registration). `InlineOtherMethod` handles
callees whose first instruction is `Ldarg*`/`Call`/`Callvirt`/`Newobj`.

Not yet confirmed: whether `VisitIterator` was *originally* static (with an explicit leading `Type`
param, or a call to a same-type static Type-resolver as its first instruction) and something in this
inlining path caused it to end up looking `instance` in the final output — or whether the instance/
static mismatch is introduced somewhere else entirely and `MethodCallInliner` is a red herring.
**Needs instrumentation**, not further static reading: log every `InlineMethod`/`InlineOtherMethod`/
`InlineLoadMethod` call in this pass (target method token + caller token) while deobfuscating the
repro sample above, and check whether `AdvisorMethod`'s token/RID range appears.

### H3: The instance/static mismatch is pre-existing in the *original* obfuscated binary, not de4dot-introduced — OPEN, not yet ruled out
If true, this would mean item #4 isn't actually a de4dot bug at all, and should be removed from the
`realBug`-counted class entirely (`realBug` explicitly only counts *introduced* failures, filtered
against the original). Attempted to check via MDToken correlation between the deobfuscated output
and the raw original `binaries/ADOverhaul2022.dll` IL dump; **not completed** — name-based grepping
doesn't work (original symbol names are obfuscated garbage, unrelated to de4dot's renamed output) and
token-based correlation needs a small dnlib-based correlation script, not attempted yet due to time.
**This should be checked BEFORE spending effort on H2** — if H3 is true, H2 is moot and item #4
should be reclassified as "pre-existing obfuscator garbage, not de4dot's to fix" rather than pursued
further as a de4dot bug.

## Evidence Log

### 2026-07-29
- Built de4dot (`Release/net8.0/linux-x64/de4dot.dll`, already built, not rebuilt this session).
- Ran full pipeline (de4dot deobfuscate → ilspycmd decompile → ilverify) against
  `binaries/ADOverhaul2022.dll` using a full local Unity Editor install's reference set
  (`<UnityEditor>/Data/Managed`, plus `UnityEngine/*.dll`, MonoBleedingEdge mono 4.5 libs as
  `-s mscorlib`, plus `binaries/0Harmony.dll`) — the complete set rather than the checked-in
  portable subset, which is what ilverify needs.
- `ilverify` on the deobfuscated DLL: 455 total errors, ~99% `FileLoadErrorGeneric` for missing
  third-party refs (`VRCSDK3A`, `netstandard`) — version/dependency noise, not de4dot's fault, exactly
  as the correctness methodology predicts. Two real target-internal-type errors found:
  `AdvisorMethod::VisitIterator$PST060003AA` and `AdvisorMethod::ConnectIterator`, both
  `StackUnexpected`, both AdvisorMethod-vs-Type receiver confusion (mirror images of each other).
  This is a live, independently-reproduced instance of WORKLOG item #4's bug class.
- Also incidentally reproduced the `MissingMethod` on a `<HandleTask>d__18` async state-machine
  `MoveNext` (obfuscated method name) that an older session's memory flagged as a DisplayClassCleaner
  issue — current `IMPROVEMENT_PLAN.md` claims DisplayClassCleaner was hardened
  (`PruneReferencedRemovals`); this error still appearing suggests either a different root cause than
  what was fixed, or a remaining edge case. **Not investigated further — separate theory, not opened
  yet; note here so it isn't lost.**
- Confirmed H1 (`ProxyCallFixer`) refuted by mechanism (see above) without needing to run anything —
  its code only ever swaps a call operand, never touches signatures.
- Found the `MethodCallInliner(false)` registration in Reactor v4's `Deobfuscator.cs:145` as the
  literal "cflow method-call inliner" WORKLOG referred to as the second candidate. Read its
  `CanInline`/`InlineMethod`/`InlineOtherMethod` logic; plausible mechanism identified (H2) but not
  yet confirmed via instrumentation.
- Attempted original-binary correlation for H3; abandoned mid-attempt due to name-based grep not
  applying to obfuscated original symbol names — needs a token-based approach instead, not a
  continuation of the same method.

### 2026-07-29 (continued) — same bug pattern found at 6 more sites, same run
Re-ran the full scorecard (`scripts/de4dot_scorecard.py full ADOverhaul2022`) while auditing
`ADOEditorUtility`/`ADOverhaulSettings`/`LicenseManager` reconstructions. The **10** target-internal
errors are stable across runs and cluster into exactly this bug pattern at 6 more locations beyond
`AdvisorMethod`, all `StackUnexpected` with the identical "found=OwnType expected=System.Type" or
"found=System.Type/other-ref expected=OwnType" shape:

- `IdentifierSerializerConnector::ListAuthentication(string, BindingFlags)` — found=own-type,
  expected=`System.Type` (same shape as `VisitIterator`).
- `IdentifierSerializerConnector+AuthenticationIdentifier::ComputeMethod()` — found=`System.Diagnostics.Process`,
  expected=own-type (a *different* wrong-type pairing than the Type-confusion cases, worth noting —
  not necessarily the same root cause, needs separate confirmation before assuming it's H2 too).
- `IdentifierSerializerConnector+AuthenticationIdentifier::DefineConsumer()` — found=own-type,
  expected=`System.ComponentModel.Component`.
- `IdentifierSerializerConnector+RefImporterDescriptor+ConnectionIdentifierService::.ctor(bool, Action)`
  — found=`bool`, expected=`string`. (This is the `ADOverhaulSettings.cs` devel file's
  `ConnectionIdentifierService` constructor — i.e. this ilverify error is a live, currently-present
  issue in the exact export source that devel's `ADOSettings` settings fields are reconstructed from.)
- `IdentifierSerializerConnector+RefImporterDescriptor+BroadcasterIdentifier::.ctor(float32, Action)`
  — found=`float32`, expected=`string`. (Same file, `BroadcasterIdentifier` constructor — same shape
  as the `ConnectionIdentifierService` one above: a bool/float parameter where a string is expected on
  the stack, suggesting a shared constructor-inlining or overload-resolution mixup across all of
  `RefImporterDescriptor`'s wrapper-value-type constructors, not just the two checked so far.)
- `ExceptionSingletonStruct::SortRef()` — found=`System.Type`, expected=own-type (mirror of the
  `AdvisorMethod` pattern, this time directly on `ADOEditorUtility`'s main class rather than a nested
  helper).
- `ExceptionSingletonStruct::FlushAdapter(string, BindingFlags, Binder, Type[], ParameterModifier[])`
  — found=own-type, expected=`System.Type` (mirror of `SortRef`, same file).

This significantly broadens the evidence base for H2 (a shared cflow pass mis-mapping `this` at many
call sites, not a one-off) — the `ConnectionIdentifierService`/`BroadcasterIdentifier` constructor
pair and the `SortRef`/`FlushAdapter` pair are *each* mirror-image pairs just like
`VisitIterator`/`ConnectIterator`, strongly suggesting one systemic cause rather than several
unrelated bugs. The `AuthenticationIdentifier::ComputeMethod`/`DefineConsumer` two are shaped
differently (wrong concrete type, not a Type-vs-OwnType receiver swap) and should not be assumed to
be the same root cause without separate confirmation.

**Practical note for reconstruction work**: none of these 10 locations are inside `TaskMethod`,
`SchemaMapping`, `EventMethod`, `InterpreterSerializer`, or `CreatorServerStub` — so a clean ilverify
result for those classes is real (if partial) evidence de4dot didn't introduce IL-level type
corruption there. It does NOT cover logic bugs ilverify can't see (e.g. the unrelated `while(true)`
infinite-loop CFG artifact found in `TaskMethod.CustomizeProduct`, tracked separately in
`cflow-resolves-to-infinite-loop/THEORY.md`) — ilverify only proves type-safety, not correct behavior.

## Current Conclusion

Still open. **Next concrete step (in priority order):**
1. **Resolve H3 first** — write a small dnlib-based (or `ildasm`/raw metadata) script that, given the
   deobfuscated DLL's `AdvisorMethod::VisitIterator` MDToken, finds the same-RID method in the
   original `binaries/ADOverhaul2022.dll` and dumps its raw IL, to check whether the instance/static
   mismatch already exists pre-deobfuscation. If yes, item #4 (at least this instance of it) isn't a
   de4dot bug at all — reclassify and stop pursuing H2.
2. If H3 comes back negative (original IL is fine, de4dot broke it), **instrument H2**: add temporary
   logging to `MethodCallInliner.InlineMethod`/`InlineOtherMethod` (or a debugger conditional
   breakpoint) keyed on the caller or callee's declaring type name containing "AdvisorMethod" (or by
   MDToken once known), rerun the repro above, and read off exactly which inline operation touches
   this method pair.
3. Only after the pass is confirmed by instrumentation (not guessed), design a guard: skip the
   inlining/rewrite when it would leave a `HasThis` mismatch between a method's declared calling
   convention and how its call sites actually invoke it — mirroring the "never narrow on partial
   write info" shape of the `TypesRestorer` fix already in `IMPROVEMENT_PLAN.md`. Gate the fix per
   the standard methodology: rerun ilverify across the full corpus, confirm `realBug` drops (not just
   this one method) and no new `emptyM`/stack-underflow regressions appear.
