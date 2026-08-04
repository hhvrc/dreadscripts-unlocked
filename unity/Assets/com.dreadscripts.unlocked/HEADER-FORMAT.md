# File header format

Every `.cs` file in this package opens with a comment block recording where its
contents came from in `decompiled/` and what was decided during the port. The
format below is what `tools/check-headers.py` enforces.

The point of the strictness is that the header is *machine-checkable*. Two
reconciliations of parallel ports have already turned on being able to ask "which
decompiled member does this correspond to" mechanically, and both were slowed by
headers that answered that question in six different spellings.

## Shape

```
// Reconstructed from: <decompiled path>
//   <additional decompiled path>            (repeat as needed, indented)
//
// <MAP entries>
//
// Line numbers are relative to the decompiled snapshot at the time of the port;
// the member names are the durable reference.
//
// <named sections, any order, all optional>
//
// Audit status: <VERIFIED|PARTIAL|UNAUDITED> -- <note>
```

`Reconstructed from:` must be the first line, and `Audit status:` the last.
Everything between is optional but must match one of the forms below.

## MAP entries

One line per decompiled member the file is responsible for. Five forms, and no
others. All are indented three spaces.

```
//   <decompiled name> -> <ported name>, line <N>
//   <decompiled name> -> <ported name>, lines <N>-<M>
//   <decompiled name> -> <ported name>, line <N>, in <file.cs>
//   <decompiled name> -> NOT PORTED, line <N> -- <reason>
//   <decompiled name> -> <ported name>
```

What separates the fifth form from the first four is that it carries **no line
number**. That is the whole rule: an entry with a line number stands on its own
and is checked against `decompiled/`; an entry without one is a sub-entry, and
belongs to whatever introduced the block it sits in.

- **`line <N>`** — the member is declared in *this* file. The checker verifies
  that a member of that name actually exists here; a header claiming a member the
  file does not declare is an error, not a note.
- **`lines <N>-<M>`** — same, for a nested type or a region spanning a range.
- **`, in <file.cs>`** — this decompiled member was ported, but into another file.
  Use this for cross-references instead of prose, so the checker can confirm the
  target declares it and that exactly one file claims each decompiled member.
- **`NOT PORTED`** — deliberately absent. The reason is required. Use this rather
  than silently omitting the member, so that "unported" and "overlooked" stay
  distinguishable.
- **no line number** — a sub-entry: a field, local or parameter belonging to
  whatever the block is about. A nested class gets one entry with its line range
  followed by a sub-entry per field; a recovered parameter list gets a paragraph
  ending in `:` followed by a sub-entry per parameter. Sub-entries name nothing
  the checker can locate in `decompiled/`, so they are not checked — which is
  also why they must not carry a line number. If a member deserves a line number,
  it is a top-level entry.

  A sub-entry run must be introduced by either a top-level entry or a prose line
  ending in `:`. A stray arrow line with no line number and no introducer is an
  error, because that is indistinguishable from a top-level entry whose line
  number was forgotten.

Two shorthands are allowed in the `<ported name>` column:

- `same` or `unchanged` — the ported identifier equals the decompiled one.
- a prose descriptor for things that are not a plain identifier: `this[string]`,
  `the styles accessor`, `lifted to a top-level type`, `dissolved into <member>`.
  The checker does not verify these, so prefer a real name where one exists.

`<decompiled name>` is the obfuscated identifier as it appears in `decompiled/`.
It is the join key: every `(decompiled file, line)` must be claimed exactly once
across the package. Duplicate claims are how a member ported twice under two
different names gets caught — the C# compiler cannot see those.

`<ported name>` may carry a signature when overloads need distinguishing:
`Button(Rect, string, GUIStyle)`. Keep it to the parameter types.

## Named sections

Optional. Each opens with its exact heading on its own comment line:

| heading | for |
|---|---|
| `PARTIAL PORT` | what is left out and the blocker for each |
| `DELIBERATE DEVIATION` | where the port knowingly differs from the shipped build |
| `SHIPPED BUG` | behaviour reproduced because the original had it |
| `DEOBF-BUG` | a defect introduced by the decompiler, corrected here |
| `NOT PORTED` | a region dropped wholesale, with the reason |
| `2019 vs 2022` | divergence between the two ADOverhaul builds |
| `NOTES` | anything else |

Prose inside a section is free-form. Prose must live in a section — a bare
paragraph between MAP entries is an error, because it is how the arrow lines that
broke earlier tooling got introduced.

## Audit status

```
// Audit status: VERIFIED -- every statement below was transcribed from the region above
// Audit status: PARTIAL -- <what was checked and what was not>
// Audit status: UNAUDITED
```

`VERIFIED` asserts the file was diffed against `export/`. Do not write it
otherwise.

## Known debt

`decompiled/` was re-snapshotted in `561e9ec`, which renumbered every file and
renamed some obfuscated members. Headers written before that commit carry stale
line numbers. They are still correct on member names. The checker reports stale
numbers as warnings, not errors, until that sweep is done.
