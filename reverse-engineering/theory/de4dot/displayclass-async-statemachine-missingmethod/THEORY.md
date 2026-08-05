# Does DisplayClassCleaner leave a dangling reference to a removed async state-machine method?

## Status: OPEN (single data point, not yet cross-checked against current DisplayClassCleaner code)

## Question

Is there still a de4dot code path (likely `DisplayClassCleaner`, v4 .NET Reactor) that removes a
method/field still referenced by a compiler-generated async state machine's `MoveNext()`, producing
a `MissingMethod` error under `ilverify` — or was this already fixed by the `PruneReferencedRemovals`
fixpoint hardening described in de4dot's `IMPROVEMENT_PLAN.md` "Completed fixes" section?

## Background

An older session's memory (predating this project's current `IMPROVEMENT_PLAN.md`) flagged three
invalid-IL bug classes on the Reactor v6 branch, one being "dangling async `<HandleTask>d__18`
MissingMethod (= DisplayClassCleaner #3/#4)". The current `IMPROVEMENT_PLAN.md` "Completed fixes"
section claims `DisplayClassCleaner` was hardened with a fixpoint reference check
(`PruneReferencedRemovals`) specifically to prevent removing a member still referenced by surviving
code — which sounds like it should cover exactly this case.

## Evidence Log

### 2026-07-29
While reproducing the `reflection-proxy-type-confusion` theory (see sibling folder) against
`dreadful-re/binaries/ADOverhaul2022.dll` with the current de4dot build, both the de4dot console
output and `ilverify` showed this error still occurring:

```
# de4dot console output during deobfuscation:
ERROR: Could not resolve MethodRef System.Boolean DreadScripts.ADOverhaul.ExceptionSingletonStruct/<HandleTask>d__18`1<T>::<obfuscated-name>(System.Threading.Tasks.Task) (0A00020D) (from ADOverhaul2022.dll -> ADOverhaul2022.dll)

# ilverify on the resulting deobfuscated DLL:
[IL]: Error [MissingMethod]: [... : DreadScripts.ADOverhaul.ExceptionSingletonStruct+<HandleTask>d__18`1::MoveNext()] Missing method 'Boolean <HandleTask>d__18`1<!0>.<obfuscated-name>(System.Threading.Tasks.Task)'
```

Not yet investigated further this session — noted here so it isn't lost, per this project's
`tracking-open-questions-with-theory-folders` workflow. This is a **separate** bug class from the
`reflection-proxy-type-confusion` theory; don't conflate the two when eventually porting either to
de4dot's own docs.

## Current Conclusion

Still open — not yet determined whether this is:
(a) a case the `PruneReferencedRemovals` fix doesn't actually cover (e.g. it checks live *code*
references but not compiler-generated state-machine `MoveNext()` bodies specifically, which reference
members in a way that's easy to miss during a reachability scan), or
(b) unrelated to `DisplayClassCleaner` entirely and just resembles the old memory's description
superficially.

**Next step:** instrument or trace `DisplayClassCleaner`'s removal decisions against this specific
method (`ExceptionSingletonStruct`'s `HandleTask` async iterator, in the reproduction sample above)
to confirm which removal pass drops the referenced member, then check whether
`PruneReferencedRemovals`'s reachability scan actually walks into `MoveNext()` bodies of
compiler-generated async state machine types. If it doesn't, that's the gap.
