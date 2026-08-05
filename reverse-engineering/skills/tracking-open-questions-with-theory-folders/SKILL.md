---
name: tracking-open-questions-with-theory-folders
description: Structures multi-session reverse-engineering investigations (which de4dot pass causes a bug, what a DRM field means, what an obfuscated class's real purpose is) as falsifiable hypotheses tracked in per-topic theory folders (theory/topic-name/question-slug/THEORY.md), so evidence and dead ends survive across sessions instead of being re-derived or silently lost. Use whenever an investigation won't resolve in the current session, or when resuming one that was tracked previously.
---

# Tracking Open Questions with Theory Folders

## When to use

- Starting an investigation that probably won't finish in one session: a suspected de4dot bug whose
  root cause isn't yet pinned to a specific pass, an ambiguous DRM protocol field, a guess about what
  an obfuscated class/method was originally for, a control-flow trace that needs more than one sitting.
- Resuming any such investigation — **check `theory/` for an existing folder on the same question
  before starting fresh.** Re-deriving a hypothesis someone already tested (and disproved) last
  session is pure waste.
- An experiment just produced a result (positive OR negative) that's worth keeping — log it before
  moving on, not after you've forgotten the exact command/output.

**This applies to any DreadScripts-tool RE work, not only de4dot investigations** — DRM protocol
questions, string/constant-decryption puzzles, class-identity guesses, and de4dot pass debugging all
use the same `theory/<topic>/<question-slug>/` shape. `de4dot/` is just the topic folder for
questions specifically about a de4dot pass or mechanism; use a different topic folder name (e.g.
`drm-protocol/`, `string-decryption/`, `class-identity/`) for anything else. See `theory/README.md`
at the repo root for the full format spec and current folders.

## The core discipline: falsifiable hypotheses, not narrated conclusions

A theory file is not a summary written after you already know the answer — it's a record of
predictions checked against evidence. The difference matters:

- **Bad:** "Investigated the bug. Turns out it's caused by X." (conclusion-first; no way to tell
  whether X was actually tested or just seems plausible in hindsight.)
- **Good:** "H1: pass X causes this. Evidence for: [mechanism reasoning]. Evidence against: [ran Y,
  got Z, which is inconsistent with H1 because...]. Status: REFUTED."

Write the hypothesis *before* running the experiment that tests it, even if only by a sentence in
your own reasoning before you act. This is what makes a theory file useful to a future session: it
shows what was actually ruled out and why, not just a plausible-sounding narrative that might be
based on partial or cherry-picked evidence.

## Workflow

1. **Check for an existing folder first.** `find theory -iname THEORY.md` or check `theory/README.md`
   if it lists an index. Resuming means reading the existing hypotheses and evidence log in full
   before adding anything — don't start a sibling folder for a question already being tracked.
2. **If new, create `theory/<topic>/<question-slug>/THEORY.md`** using the format in
   `theory/README.md` (Status line, Question, numbered Hypotheses each with their own status,
   dated Evidence Log, Current Conclusion with a concrete next step).
3. **State the hypothesis, then test it.** Prefer an experiment that could come out either way over
   one that can only confirm what you already suspect — a test that can't fail isn't evidence.
4. **Log the result immediately**, in the Evidence Log, dated, with enough command/output detail that
   a future session could re-run it or at least judge whether it's still valid against current code.
   Update the hypothesis's status (OPEN/CONFIRMED/REFUTED) right after logging, not at the end of the
   session — a session can end unexpectedly.
5. **Update "Current Conclusion" with the next concrete step**, not "investigate more." If the next
   step is "instrument pass X," name the exact file/function to instrument, not just "add logging."

## Graduating a confirmed theory

Once a `de4dot/`-topic theory is CONFIRMED and a fix is verified via de4dot's own correctness
methodology (ilverify/realBug — see de4dot's `measuring-deobfuscation-correctness-with-ilverify`
skill; never trust a readability-heuristic improvement alone):

1. Port the finding into de4dot's own `IMPROVEMENT_PLAN.md`/`WORKLOG.md`, written entirely in
   de4dot's own generic terms — a standalone .NET Reactor bug report with a synthetic or anonymized
   repro shape, **not** a copy of the dreadful-re-specific evidence log (no product names, no
   `binaries/*.dll` paths, no dreadful-re file references). See `theory/README.md`'s explicit warning
   on this — de4dot's docs must read as if dreadful-re doesn't exist.
2. Mark the theory folder `FIXED-UPSTREAM` with a pointer to which de4dot commit/WORKLOG entry it
   became, or delete it if it has no further dreadful-re-specific value once resolved.

Theories under any non-`de4dot` topic (DRM protocol, string decryption, class identity) don't
"graduate" anywhere else — once CONFIRMED, fold the finding into `RE_NOTES.md` (or the DRM spec at
`../../DRM.md`, if it is a protocol/crypto fact) as usual
(see the `renaming-and-documenting-deobfuscated-source` and `reversing-unity-license-drm-protocol`
skills) and mark or remove the theory folder.

## Common scenarios

**Scenario: starting a de4dot bug investigation using a real dreadful-re sample as a test case.**
This is exactly what the `de4dot/` topic folders are for — dreadful-re's binaries are useful,
real-world .NET Reactor samples even for questions that are ultimately about de4dot itself. Track the
investigation (sample used, exact commands, IL evidence) here in full detail; only the *portable,
de4dot-generic* version of a confirmed finding goes into de4dot's own docs later.

**Scenario: a hypothesis was marked CONFIRMED last session, but the referenced code has since
changed (a de4dot rebuild, a `RE_NOTES.md` update).** Don't build on it blindly — re-verify against
current state first (this mirrors the general rule for all memory/documentation in this project: a
past conclusion is a claim about *that* point in time, not a standing fact). If it no longer holds,
add a new dated Evidence Log entry noting the discrepancy rather than silently editing away the old
conclusion — the history of "this used to be true, here's what changed" has its own value.

**Scenario: an experiment produces a negative/inconclusive result.** Log it with the same care as a
positive one, and update the hypothesis's status to REFUTED (or leave OPEN with the negative
evidence noted) rather than omitting it. A future session skipping a dead end you already tried is
the entire point of this workflow.

## Pitfalls

- Don't write a theory file as a post-hoc narrative once you already know (or think you know) the
  answer — capture the actual hypothesis-then-test structure, even retroactively reconstructing it
  honestly if you skipped writing it down in the moment.
- Don't start a new topic/question folder without checking `theory/` for an existing one first.
- Don't let a dreadful-re-specific detail (binary name, DRM field name, internal path) leak into a
  finding that's being ported to de4dot's own docs — restate it generically there.
- Don't mark something CONFIRMED on the strength of a mechanism argument alone when a cheap
  instrumentation/experiment could actually test it — reasoning about what code "should" do is a
  hypothesis, not evidence, until it's actually run.
