---
name: splitting-large-classes-into-partials
description: Permanent file-layout convention for a ported class with many nested types (a monolithic static utility class, a big Editor class) — split it into a C# partial class, one file per nested type/logical section under a ClassName/ folder, each with its own export line-range header. Use when a class has enough distinct nested types or sections that a single file makes it hard to tell what's been verified against export/ and what hasn't, not for a class that's just long but coherent.
---

# Splitting Large Classes into Partials

## When to use

- A class being ported mixes many logically-separate nested types/sections in one file — e.g. a
  monolithic static utility class (`ADOEditorUtility`, originally `ExceptionSingletonStruct`) with a
  dozen nested helper classes/structs/enums, or a large Editor class with several `[CompilerGenerated]`
  display classes.
- Concretely, this doesn't help a file that's merely long but coherent (a single big method, or a
  tight cluster of related methods) — that's what `chunking-large-decompiled-files` is for, as a
  *working technique* during editing. This skill is a *permanent file-layout* decision: it changes
  where the code lives, not just how you approach editing it.
- Good signal it's worth doing: you find yourself grepping the same monolith repeatedly to relocate
  a specific nested type's body, or a bug review turns up something wrong in a section that had no
  `TODO` marker and no dedicated place to record "this part is/isn't verified yet" (see the
  `ADOEditorUtility` case below — `ConnectStatus` and `InterpreterSerializer` were both silently wrong
  with no marker at all, because there was no place to record per-section audit status).

## Why do this instead of just chunking in-memory while editing

Chunking (the other skill) is ephemeral — you split for the duration of an edit, merge back, delete
the working files. That doesn't help future sessions, and it doesn't give each nested type its own
place to record export provenance and audit status. A permanent partial-class split does:

- Each file's header states exactly which `export/` file and line range it came from, and whether
  it's been checked against that source yet — instead of one vague file-level comment covering a
  2000+ line file with wildly different provenance per section.
- `find`/`ls` on the folder immediately shows what nested types exist — no grepping a monolith to
  relocate a class.
- Smaller, independently-readable files reduce the chance of a `ConnectStatus`-style bug: something
  wrong in code that has no `TODO` tag and isn't the thing you were explicitly asked to check, sitting
  in a huge file you don't re-read in full.
- Smaller git diffs when only one nested type changes.

Menu items and other Unity attributes are unaffected by this refactor either way — confirmed by
diffing every `[MenuItem(...)]` string in `export/` against the ported source for both ADOverhaul and
ControllerEditor: 100% match. Reactor's string encryption only intercepts runtime string usage via
`smethod_N`, not compile-time attribute constructor arguments, so these were never obfuscated to begin
with and need no special handling here.

## The convention

For a class `Foo` (in namespace `N`, file `Foo.cs`) with nested types `Bar`, `Baz`, ...:

```
Foo/
    Foo.Bar.cs        -- the Bar nested type only
    Foo.Baz.cs        -- the Baz nested type only
    Foo.Rects.cs      -- outer-body members grouped by domain (see "two axes" below)
    Foo.Strings.cs
    ...
```

Each file:
1. Declares `internal static partial class Foo` (or the appropriate access/staticness — match the
   original) wrapping just that one nested type, or one domain group of outer-body members.
2. Starts with a header comment naming the export file and, per member or nested type, the export
   name it came from and what it was renamed to. Plus an explicit audit-status note —
   `-- VERIFIED against export`, `-- NOT YET AUDITED against export`, or `-- KNOWN DIVERGENT, see
   audit notes` (see Workflow step 4). This status note is the whole point: it's what turns "did I
   check this" into something you can `grep` for across the folder instead of silently forgetting.
3. Only lists the `using`s it needs. (This reverses earlier guidance to repeat the monolith's full
   block. Once the outer body is split by domain the blocks genuinely differ per file, and a copied
   block that lists half of UnityEditor in a file that draws nothing is noise.)

### Line numbers in headers: nested types only

Cite a line range only for a nested type extracted as one contiguous block, and mark it
snapshot-relative. A domain group is assembled from members scattered across thousands of lines, so
a range would be a fiction.

Even for nested types the range is a convenience, not an identifier: `export/` is regenerated
wholesale, and a re-export on 2026-07-31 shifted seven ControllerEditor files. **The export type and
member names are the durable reference; line numbers are a hint for finding them again.** Say so in
the header, so nobody trusts a stale number.

### The two axes, and why one is not enough

Splitting on nested type alone does not work on these classes, because that is not where the volume
is. Measured against the 2026-07-31 snapshot, after peeling off every nested type:

| Class | Lines | Left in the outer class body |
|---|---|---|
| `ControllerEditor` | 18,535 | 11,891 (64%) |
| `ADOverhaul` | 8,900 | 7,288 (82%) |
| `EditorUtils` | 8,514 | 6,521 (77%) |
| `ADOEditorUtility` | 4,062 | 2,069 (51%) |

A `Foo.Core.cs` holding "whatever is left" would therefore *be* the problem, at 6,500-11,900 lines.
So the outer body needs a second axis: **group its members by the type they operate on** — the first
parameter for an extension method, the return type for a factory. In `EditorUtils` that sorts 382
top-level methods into workable groups (string 50, Rect 33, VRCAvatarDescriptor 19,
AnimatorController 17, Vector3 16, Object 14, Transform 13, Type 11).

Grouping by parameter type has one property that matters a lot here: **it works before the naming
pass**, because it does not depend on what the methods are called. Filenames for nested types do, so
those should still be named first.

Do **not** try to group by the name-suffix families de4dot leaves behind (`*Queue`, `*List`,
`*Resolver`, `*Error`). They look like they might recover the vendor's original class boundaries.
They do not — measured on `EditorUtils`, `Queue` spans string, Rect, GUIContent and Animator, and
`List` spans Type, VRCAvatarDescriptor and string. This was checked so it need not be checked again.

### Compiler-generated closures are dissolved, not split

A large share of the "nested types" in these classes are `_003C_003Ec` and
`_003C_003Ec__DisplayClass*` closure classes: 42 in `ControllerEditor` (1,909 lines), 11 in
`EditorUtils`, 8 in `ADOEditorUtility`. **Count them separately and never give them a file.** They
are decompiler artifacts and belong back inline as lambdas at their use sites; a partial file per
closure would enshrine them in the shipped package.

This is also why the nested-type counts in older notes look larger than the real ones — see
"Which classes this applies to" below.

## Workflow

1. Identify nested-type boundaries in the source monolith, at the containing class's indent level,
   **separating real nested types from `_003C`-prefixed closure classes** — the latter get no file
   (see above). Brace-matching from each declaration is more reliable than a plain `grep`, since the
   declarations carry attributes and modifiers in varying order.
2. For each real nested type, extract its line range into `Foo/Foo.TypeName.cs` with the header
   above. Extract mechanically rather than hand-copying, to avoid transcription slips — then polish
   the extracted file, which is where the actual porting work happens.
3. Group the remaining outer-body members by the type they operate on (see "the two axes") into
   `Foo/Foo.<Domain>.cs`. **Do not create a `Foo.Core.cs` catch-all** — on these classes it would
   hold most of the file and defeat the split.
4. **Verify before deleting the original**: brace-balance check per new file (`grep -o "{" | wc -l`
   should equal `grep -o "}" | wc -l`), then run `python3 scripts/typecheck_package.py`, which compiles
   the package against the checked-in reference assemblies and is the real check. Confirm every gap
   between consecutive extracted ranges in the original is only blank lines or now-redundant
   section-divider comments — never a real code line. Carry any useful one-line purpose description
   from a dropped divider comment into the new file's header instead of losing it.
5. Delete the original monolith `.cs` and its `.meta` only once every member has landed somewhere.
   A partially-ported class means the monolith is still the reference — say so in the new files'
   headers ("Partial in progress: ... not ported yet") rather than deleting early.
6. `.meta` files are tracked in this repo, so generate them rather than waiting for Unity — run
   `python3 scripts/gen_meta.py --write`, which creates one for every asset that lacks it and never
   touches an existing one. Don't hand-write them: the GUID has to be unique across the tree and the
   file has to be LF while the `.cs` beside it is CRLF, and both are easy to get wrong by hand.
   The script's header explains why it generates a random GUID rather than one derived from the asset
   path — earlier revisions of this skill asked for a path-derived GUID, but that is not what the tree
   contains and standardising on it now would rewrite 130 GUIDs for no benefit.
   Regenerating is safe for Editor-only types with no serialized references to their GUID; don't
   reorganize a `MonoBehaviour`/`ScriptableObject` file whose GUID is referenced by serialized
   scene/prefab/asset data without checking those references first.
7. When auditing a split class afterward, update each file's header status note as you verify or fix
   it — that status note is the durable record of audit progress across sessions, replacing "grep the
   whole monolith for TODO and hope that's the full list of what's wrong."

## Which classes this applies to

Run `find export -name '*.cs' -not -name '-Module-*' | xargs wc -l | sort -rn | head` for the current
numbers rather than trusting a figure written here. The shape, which is what matters, has been stable:
a handful of classes are enormous and everything else is small, with a sharp cliff between them and
almost nothing in the middle. As of the 2026-07-31 measurement four portable classes were over 4,000
lines — `ControllerEditor`, `ADOverhaul`, `EditorUtils` and `ADOEditorUtility` — and every other
portable file was under 900.

**When counting nested types, exclude the compiler-generated closures**, or the number will be two to
three times the real one and the split will look far more effective than it is. Earlier revisions of
this skill quoted 91 nested types for `ControllerEditor`, 27 for `EditorUtils` and 23 for
`ADOEditorUtility`; the real figures are 28, 15 and 14. Those real nested types account for only
18-38% of each file, which is what the "two axes" section above exists to deal with.

**Do not expect the split to isolate the licence code.** It is tempting to assume the DRM can be
dropped by simply not porting a partial. In `ControllerEditor` it cannot: there is no licence-named
nested type at all, and the HWID/HMAC validation is spread across **20 regions** between lines 1,773
and 18,150, inside ordinary methods (`RegisterAnnotation`, `WriteAnnotation`, `TestVisitor`,
`CalculateVisitor`, `ReadVisitor`). Three large clusters look wholesale-removable; the rest are call
sites needing stubs. Splitting tells you *which* partials contain it, which is a real improvement
over hunting an 18,000-line file, but each region still has to be excised individually with the
surrounding method left working. See `scripts/port_status.py`, which flags exactly which files must
be stripped rather than skipped.

Order the work **naming, then split, then DRM excision** — splitting before the DRM is understood
risks cutting a partial boundary through one of those 20 regions.

Only the two product classes carry DRM at all. `EditorUtils`, `ADOEditorUtility`,
`AnimatorGraphReflection` and `AnimatorTypeCache` contain zero licence markers, so they can be split
and ported with no licence work.

**Check for a shared type before filing it under one tool.** Both products shipped their own copy of
several of these nested types under different chosen names — `EditorUtils.ResizeHandle` is
`ADOEditorUtility.ResizeHandle`, and `SceneViewPanel` likewise. Those belong in
`public/unity/.../Editor/Common` (namespace `DreadScripts.Common`), ported once. `port_status.py`'s
`SHARED` map records the pairs already found.

Naming before splitting is usually the right order. Partial filenames are derived from nested-type
names, so splitting first means naming files after `ConsumerAlgo` and renaming them all later.

## Common scenarios

**Scenario: mid-way through fixing bugs in a monolith when you realize it qualifies for this split.**
Do the split with the fixes already applied (extract from the current, partially-fixed file state),
not from a pristine copy — the split should capture where things actually stand, and the new files'
audit-status headers should reflect what's actually been checked so far, not overclaim.

**Scenario: a nested-type's body turns out, on later audit, to be entirely wrong (fabricated, not
just buggy).** Don't quietly rewrite it in place without updating the header — flip the status note
to `-- KNOWN DIVERGENT` (or after fixing, `-- VERIFIED`) so the change is visible in a diff and the
next session doesn't have to re-discover it was ever in question.

## Pitfalls

- Don't split a class that's just long-but-coherent — that's wasted churn; use
  `chunking-large-decompiled-files` for the editing pass instead and leave the file whole.
  This is a real trade-off, not a solved default: the split adds file-count and cross-file navigation
  overhead, so only pay for it when the class is genuinely a bag of unrelated nested types.
- Don't skip the gap-content check before deleting the original — this is exactly where an off-by-one
  in the extraction ranges would silently drop or duplicate a real code line.
- Don't hand-write a `.meta` for the split files — `scripts/gen_meta.py --write` exists so that three
  agents porting in parallel cannot collide on a GUID or disagree about the file's shape. And don't do
  this split at all on a type whose GUID is referenced by serialized data without checking first.
- Don't let the audit-status header go stale — a file marked `NOT YET AUDITED` that was actually
  checked and found fine should be updated to `VERIFIED`, otherwise the next session re-does work
  that's already done.
