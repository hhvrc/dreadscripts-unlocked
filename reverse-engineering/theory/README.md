# Theory Tracking

A workspace for open reverse-engineering questions that need more than one session to resolve,
tracked here rather than lost to a single conversation's context. This covers **any** RE work on
the DreadScripts tools, not just de4dot investigations — e.g. an open question about the DRM
protocol, a not-yet-confirmed hypothesis about what a `smethod_N` transform's constants actually
mean, a suspected-but-unverified mapping between an obfuscated class and its original purpose, or
(most commonly so far) a de4dot deobfuscation gap.

**Why this exists:** a hypothesis explored in one session (a suspected root cause, a candidate
de4dot pass, a guess about DRM field semantics, an experiment that didn't pan out) is expensive to
re-derive from scratch in the next one. Writing it down as a theory — with its evidence, not just
its conclusion — lets a future session pick up exactly where the last one left off, re-verify a
stale conclusion against current code instead of trusting it blindly, and avoid repeating a
disproven approach.

**This explicitly applies to de4dot work too** — see `de4dot/` below. de4dot has its own
`IMPROVEMENT_PLAN.md`/`WORKLOG.md` for finished, verified findings; this folder is the messier,
in-progress layer *before* something graduates to those documents. **Nothing about
dreadful-re-specific context (binaries, DRM details, product names) belongs in de4dot's own
repo/docs** — de4dot's copy of a finding must read as a standalone bug report against a general
.NET Reactor sample, with any dreadful-re-specific framing (which product, which binary) staying
here.

## Layout

```
theory/
  de4dot/
    <component-or-bug-slug>/
      THEORY.md       — the question, numbered hypotheses with status, evidence log, conclusion
      evidence/        — saved IL dumps, ilverify output, decompiled snippets referenced by THEORY.md
  <other-topic>/        — e.g. drm-protocol/, string-decryption/, class-identity/ — same shape,
                           for any DreadScripts-tool RE question that isn't specifically about a
                           de4dot pass
    <question-slug>/
      THEORY.md
      evidence/
```

`de4dot/` is just one category under this tree, for questions specifically about a de4dot pass or
mechanism. Any other open RE question about the DreadScripts tools (DRM protocol semantics, string/
constant decryption, class/method identity, control-flow tracing) gets its own top-level category
folder the same way. One folder per distinct question or bug class — not one per session. If you're
resuming work on something already tracked, read its `THEORY.md` and add to it; don't start a
sibling folder for the same question.

## THEORY.md format

```markdown
# <Short question form, e.g. "Which de4dot pass produces the Wrapper.Proxy -> GetMethod bug?">

## Status: OPEN | CONFIRMED | REFUTED | FIXED-UPSTREAM

## Question

What we're actually trying to answer, precisely enough that "yes/no/here's why" is a possible answer.

## Hypotheses

### H1: <one-line claim> — STATUS
Evidence for:
- ...
Evidence against:
- ...

### H2: <one-line claim> — STATUS
...

## Evidence Log

Dated, append-only. Each entry: what was run, what came out, what it does/doesn't confirm.

### 2026-07-29
- Ran X against Y, got Z. Confirms/refutes H1 because ...

## Current Conclusion

The best current answer, or "still open" — and specifically what's needed to move it forward
(the next concrete experiment, not just "investigate more").
```

## Workflow rules

1. **State a falsifiable hypothesis before running an experiment**, not after. A hypothesis written
   to match results you already have isn't testable — the point is to predict, then check.
2. **Log negative results with the same care as positive ones.** A disproven hypothesis saves the
   next session from re-trying it; a silently-abandoned dead end doesn't.
3. **A theory graduates out of this folder once it's CONFIRMED and the fix is verified** (via the
   correctness methodology — ilverify/realBug, not readability heuristics) — at that point, port the
   finding into de4dot's own `IMPROVEMENT_PLAN.md`/`WORKLOG.md` (de4dot-only phrasing, see above),
   and either delete the theory folder or mark it `FIXED-UPSTREAM` with a pointer to which de4dot
   commit/entry it became.
4. **Re-verify, don't trust, a theory's conclusion once code has moved on.** A `CONFIRMED` or
   `REFUTED` verdict was true *as of* its evidence log entries — check whether the referenced files/
   line numbers still match current code before building further work on an old conclusion.
5. Keep dreadful-re-specific details (which binary, which product) in the evidence log here; when a
   finding is ready to move to de4dot, restate it in de4dot's own generic terms (a synthetic or
   anonymized repro shape) rather than copying dreadful-re specifics over.
