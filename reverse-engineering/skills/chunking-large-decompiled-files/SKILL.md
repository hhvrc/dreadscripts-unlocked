---
name: chunking-large-decompiled-files
description: Technique for safely cleaning up a decompiled source file too large to process reliably in one pass (thousands of lines) — split at method/type boundaries, process each piece, merge back with a brace-balance check. Use when a file being ported out of export/ is large enough that renaming/goto-cleanup/reconstruction risks losing track of progress or silently corrupting a distant part of the file.
---

# Chunking Large Decompiled Files

## When to use

- A file is large enough (multiple thousand lines) that a single editing pass over
  it risks incomplete coverage, lost context, or an edit in one region silently breaking brace
  balance somewhere far away.
- Concretely: this was used to process 4 files in one pass — 3,887 / 7,177 / 8,558 / 15,530 lines —
  and separately to work through a 549-`goto` cleanup backlog across 47 files. Both are done now
  (see `RE_NOTES.md`), but the same file-size problem will recur (`ControllerEditor.cs` is currently
  18,527 lines with ~111 stub methods still needing reconstruction, `EditorUtils.cs` is 4,998 lines
  with helper methods still under cryptic export-era names) — this is the applicable technique for
  that kind of work, not a one-off that's now irrelevant.

## The technique

1. **Split at method/type boundaries, never mid-method.** Parse the file tracking brace-nesting
   depth; a valid split point is a line at `root_type_depth + 1` that starts a new method signature,
   field declaration, or nested type — never inside one. A single method/type that exceeds the
   target chunk size stays whole as its own chunk rather than being split further (splitting mid-body
   is how you lose track of what's been changed and what hasn't).
2. **Target ~500 lines per chunk** as a starting budget — small enough to hold in full context while
   working on it, large enough that the chunk still contains a coherent unit of related methods/types
   (adjust up or down based on how tightly related the surrounding code is; a display class and its
   sibling methods belong in the same chunk even if that pushes the budget a bit).
3. **Track chunks with an explicit manifest** (ordered chunk list + original line ranges + a short
   description of the first type/method in each) — this is what lets you resume after an
   interruption without re-deriving which parts of the file are done.
4. **Process smallest file first, smallest chunks first within each file** — build confidence and
   establish naming/style conventions on the easy cases before tackling the file's hardest sections.
5. **Merge back and verify immediately**: matching open/close brace counts, a sane total line count
   relative to the original, and no chunk silently dropped. Do this right after merging, not as an
   afterthought — a dropped chunk is much cheaper to catch immediately than after further work has
   built on top of the merged file.
6. **Delete the chunk working files once merged and verified** — don't leave stale chunk directories
   sitting in the tree; they're workspace artifacts, not part of the deliverable.

## What this looks like for the current largest remaining files

`ControllerEditor.cs` (18,527 lines, 111 stub methods, many inside small `[CompilerGenerated]`
display classes) and `EditorUtils.cs` (4,998 lines, still has cryptic export-era helper names like
`DestroyError()`/`CalcError()`/`SortResolver`/`RestartQueue`) are the two files where this applies
right now. Note the dependency direction: most of `ControllerEditor.cs`'s stub UI-callback methods
call into `EditorUtils.cs` helpers — renaming/clarifying the helper layer first means the callback
reconstructions can be written against clean names instead of needing a second pass later to update
call sites once the helpers eventually get renamed. Chunk `EditorUtils.cs` first for that reason, not
just because it happens to be smaller.

## Common scenarios

**Scenario: about to fill several stub methods in `ControllerEditor.cs` in one sitting.** Group the
work by containing display class/context (the stubs cluster tightly — e.g. three methods inside one
`[CompilerGenerated]` class around a `ReorderableList` callback registration), not by raw line-number
order — a cluster shares context (the same captured fields, the same UI feature) that's expensive to
re-load if you jump around.

**Scenario: a stub's reconstruction turns out to depend on another not-yet-renamed helper.** Don't
guess a plausible name for the helper just to keep the stub's reconstruction moving — that produces
two things needing fixing later (the stub, plus every other call site once the helper is properly
renamed). Either use the helper's current (even if cryptic) name as-is and note the dependency, or
switch to cleaning up the helper layer first per the dependency-order note above.

## Pitfalls

- Don't split a file at an arbitrary line count without checking it lands on a real boundary — a
  chunk that starts mid-method is unrecoverable without re-reading the whole surrounding context.
- Don't skip the post-merge brace/line-count verification "since the chunks looked fine individually"
  — a dropped or duplicated chunk is exactly the kind of error that looks fine per-chunk and only
  shows up in the merged whole.
- Don't leave chunk directories around after merging — check `find ../public/unity -type d -iname
  "chunks_*"` returns nothing once done.
