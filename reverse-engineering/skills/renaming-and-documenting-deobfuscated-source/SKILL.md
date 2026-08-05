---
name: renaming-and-documenting-deobfuscated-source
description: The workflow and conventions for turning de4dot-renamed export/ classes into clean, semantically-named source in the restored Unity package at ../public/unity/, and for keeping RE_NOTES.md as the authoritative living map. Use whenever porting or updating a file there, renaming a smethod_N/field_N, or starting a session on this project.
---

# Renaming & Documenting Deobfuscated Source

## When to use

- Starting any session on this project — read `RE_NOTES.md` first, every time.
- Porting a new file from an `export/` class into the restored package.
- Naming a `smethod_N` / `field_N` once its purpose becomes clear.
- Unsure whether something belongs in `export/` or the ported package.

## Record the rename in `renames/`, not just in prose

Before doing any of the below by hand: a decision like "`ExceptionSingletonStruct` is really
`ADOEditorUtility`" belongs in `renames/{Assembly}.json`, where `ilrename` applies it to the
assembly metadata before ILSpy runs. Everything then follows automatically — every call site,
`export/`'s contents, and the filename ILSpy picks. See the "Renaming" section of
`dreadful-re/AGENTS.md` for the map format and for `ilrename usages`, which lists an entity's call
sites so the name can be chosen from evidence rather than from the shape of the control flow.

`RE_NOTES.md` still records *why* a name was chosen and what has been verified; `renames/` records
the mapping itself, in a form the pipeline can act on. Keep the two consistent — a name that only
exists in RE_NOTES prose will not appear in `export/`.

**This applies to names decided while porting, too, and that is the case most often missed.** All
four `EditorUtils` partials were ported with every field named in the ported file and nothing written
back, so 130 names existed only in `../public/unity/` while `export/` still showed
`WatcherProcessor::candidateProcessor`. Nothing detects that divergence — both trees look finished.
Write the names back as part of the port, not as a later pass:
`apply_renames.py set --set-type` names the type, `--set old=New` names each member.

## The two-tier source model

```
export/                                    READ-ONLY decompiler output. Never edit.
../public/unity/Assets/
    com.dreadscripts.unlocked/Editor/      Clean ported source — the actual deliverable.
```

`export/` has `renames/` applied to the metadata already, so its type names match the ported source
for everything named so far. `reexport.py --no-rename` rebuilds it raw when a chosen name is itself
in question — ilrename only *warns* when a renamed name also appears in a string literal, so the raw
tree is the ground truth for checking a possible reflection target.

`export/` exists purely as a reference and diff baseline: since it's machine-generated, re-running
the pipeline (see the `triaging-dotnet-reactor-obfuscation` skill) regenerates it deterministically
from the same DLL, so hand-edits there would silently vanish and diffs against a future re-run would
be meaningless noise. All actual understanding and cleanup goes into the ported package.

**The destination changed on 2026-07-31 and older references are dead.** Work used to land in a
`dreadre-devel/` Unity project in this repo. That reconstruction drifted from `export/`, so it was
retired: the project moved to `../public/unity/`, its sources were dropped, and every file is now
re-derived from `export/` and polished on the way in. Two consequences worth holding onto:

- **The two vendor packages are consolidated into one.** There is no longer a
  `com.dreadscripts.<product>/` per tool; shared classes live once under `Editor/Common/`.
- **The licence/DRM code is not ported.** Neither product isolates it: `ADOverhaul` and
  `ControllerEditor` both keep validation inline in their root class, so both are stripped during
  the port, never skipped — excluding either file drops that whole tool.
  `scripts/port_status.py` owns that policy and says what is left to port.

Both `dreadre-devel/` and the older `Assets/DreadScripts/Editor/` are dead paths. Anything still
naming them is stale.

`RE_NOTES.md` is the connective tissue: it tracks why a name was chosen and what has been verified.
What has been *ported* is not written down anywhere by hand — `port_status.py` derives it by diffing
the two trees, precisely so it cannot drift.

## Workflow: turning an export/ class into ported source

1. Read the `export/<Assembly>/<Namespace>/<Class>.cs` file (never edit it).
2. Resolve any `smethod_N` calls first — see the `decrypting-dotnet-reactor-smethod-strings` skill — so the
   source you write reflects actual string literals, not decryption calls.
3. Resolve any remaining switch/XOR control-flow artifacts — see
   the `resolving-residual-control-flow-manually` skill — before renaming, since goto-soup control flow
   makes correct semantic naming much harder to get right.
4. Apply the naming taxonomy below to types/methods that still carry decompiler-generated names.
5. Write the clean file to the correct path under `../public/unity/Assets/com.dreadscripts.unlocked/Editor/`.
6. Add or update the row in `RE_NOTES.md`'s source-file table: source path, export origin, and a
   short status note (e.g. "Discrepancies fixed", "Clean 1:1", "Strings filled", "DRM fields
   complete"). Be specific about *what* was fixed, not just "cleaned up" — future sessions rely on
   this to know whether to re-check a file after a pipeline re-run.

## When the ported source must deviate from `export/`: mark it `DEOBF-BUG`

Sometimes `export/` is simply wrong — de4dot's control-flow recovery does not always reproduce what
the vendor shipped, and the ported file has to say something `export/` does not. That is a legitimate
deviation, but it breaks the property every other ported file relies on: that the ported source can be
diffed against `export/` and any difference is a mistake. So every such site carries a greppable
marker, and `grep -rn DEOBF-BUG` over the package is the complete list.

Two forms, because the difference matters a great deal to whoever reads them next:

- **`DEOBF-BUG(resolved)`** — the true behaviour is *established*, not inferred. Acceptable evidence
  is a trace of the original obfuscated IL, or a second build of the same method that decompiles
  cleanly (the two ADOverhaul snapshots and ControllerEditor frequently share a method, and rarely
  fail to decompile together).
- **`DEOBF-BUG(guessed)`** — a best-effort reconstruction with no such backing. State plainly which
  part is guessed and what would settle it. These are the first things to re-derive whenever the
  deobfuscator improves.

Each marker says what `export/` shows, what the ported code does instead, and what evidence closed the
gap. Say explicitly that `export/` will keep showing the wrong form until de4dot changes, so the next
reader does not "fix" the deviation back.

**The known de4dot fault, so it need not be re-derived.** de4dot sometimes recovers a Reactor-flattened
`if` as a `while`, producing a loop that cannot terminate. This was confirmed rather than assumed: on
`AnimatorTypeCache.ParameterEntry.Source` the original method is an XOR-switch state machine, and
tracing its dispatch gives condition-false → body once → return, condition-true → return. Sibling
methods that Reactor never flattened decompile to a correct forward branch, which is the tell — the
flattened ones sit at a conspicuously different RVA and carry a `.try`/`catch Object` wrapper. So
**treat a non-terminating `while` in `export/` as this fault**, and prefer confirming it against the
original IL or a second build over assuming it. Report the sample to the de4dot side; do not try to
fix the deobfuscator from this repo.

## Telling a surviving vendor name from an obfuscated one

Some members kept their original names through obfuscation -- `[SerializeField]` fields must, because
the serialised data is keyed by name, and so must anything reached by reflection or by Unity's
message dispatch. Renaming one of those destroys information and can break the program. `show` cannot
tell them apart, so this is a judgement you have to make per member.

The reliable signal is **the per-type suffix family**. Reactor renames a type's members from a small
vocabulary and gives one type's members a consistent tail: `…Setter`, `…Serializer`, `…Context`,
`…Algo`, `…Policy`, `…Tests`, `…Method`. So within one type, obfuscated members rhyme with each other
and an original name does not. A member with no family suffix, sitting among a dozen that share one,
is almost certainly original.

Corroborating signals, in rough order of strength:

- **Parameter names.** Obfuscated methods carry generated parameter names (`comparedic`, `isasset`,
  `excludeproc`, `iskey3`). A method whose parameters read as English (`willCreateIfNull`) kept its
  signature.
- **`[SerializeField]` / `[NonSerializedSetting]`** on a field: it had to keep its name to round-trip,
  which is exactly why `guid`, `localID`, `defaultState` and `u_updateLink` survived while their
  non-serialised neighbours did not.
- **Unity message names** (`OnGUI`, `OnEnable`, `OnInspectorGUI`) and BCL overrides -- the engine and
  runtime call these by name. `scripts/rename_status.py` already excludes the ones it can be certain
  of; it deliberately does not guess at the vendor-name cases above.
- **The vendor's own prefixes**, e.g. `a_` and `u_` in these products, applied consistently across
  unrelated types.

When the signals disagree, leave it unnamed and say why. An obfuscated name left in place costs a
later pass; an original name overwritten is unrecoverable without going back to the binary.

## Naming taxonomy

De4dot output still carries decompiler-generated names (`Class0`, `smethod_N`, `field_N`) plus some
already-renamed-but-generic class names. When a class/method's role becomes clear, prefer these
established suffixes over inventing new ones (check `dreadful-re/AGENTS.md` for the current list —
it's the source of truth and may have grown since this skill was written):

| Suffix | Role |
|---|---|
| `*Thread` | Async worker / coroutine-like handler |
| `*Server` | Singleton or static service/registry |
| `*Policy` | Strategy or behavior encapsulation |
| `*Property` | Data holder / model field wrapper |
| `*Method` | Functional adapter (command/strategy pattern) |
| `*Connector` | Bridge between two subsystems |
| `*Dispatcher` | Routes messages or tokens |
| `*Adapter` | Converts between representations |
| `*Task` | One-shot async operation |
| `*Exporter` | Serializes / writes out data |
| `*Collection` | Aggregates related model objects |
| `*Struct` | Value-type singleton/exception holder |

Reserve inventing a *new* suffix for a genuinely novel role — check whether an existing one already
fits before adding to this list, since a growing, inconsistent taxonomy is harder to reason about
than reusing an approximate existing category.

## Common scenarios

**Scenario: a class's true purpose only becomes clear after resolving several smethod_N strings.**
Don't rename prematurely off a guess. Do the string/control-flow resolution first (steps 2-3 above),
then name — a name based on decrypted string content (e.g. finding literal DSL keys, URLs, or error
messages inside the method) is far more reliable than one based on control-flow shape alone.

**Scenario: `RE_NOTES.md` says a file is "Clean 1:1" but a pipeline re-run changes the export.**
Re-diff that specific class against the new `export/` output before assuming the ported copy is
still accurate — a de4dot fork improvement can resolve previously-opaque code in ways that change
what "1:1" means. Update the status note either way so the next session knows it was re-checked.

## Pitfalls

- Never edit `export/` to "fix" a bad decompile — the fix belongs in the ported source, and the
  wrongness itself is a useful signal about pipeline limitations (see
  the `triaging-dotnet-reactor-obfuscation` skill).
- Don't let `RE_NOTES.md` fall behind actual file state — a stale "pending" note for a file that's
  actually done (or vice versa) wastes the next session's time re-deriving what's already known.
- Don't reuse a package path from memory without checking the two-tier model above
  first; package-layout paths have changed at least once already.
